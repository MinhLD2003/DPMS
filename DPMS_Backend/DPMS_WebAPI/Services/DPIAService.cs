using AutoMapper;
using DPMS_WebAPI.Builders;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Repositories;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Comment;
using DPMS_WebAPI.ViewModels.DPIA;
using MediatR;
using System.Security.Claims;

namespace DPMS_WebAPI.Services
{
    public class DPIAService : BaseService<DPIA>, IDPIAService
    {
        private readonly IMapper _mapper;
        private readonly IEventMessageBuilder _messageBuilder;
        private readonly IDPIARepository _dpiaRepository;
        private readonly IFileRepository _fileRepos;
        private readonly IMemberResponsibilityRepository _mRes;
        private readonly IDPIAMemberRepository _dpiaMember;
        private readonly IMediator _mediator;
        private readonly IExternalSystemService _systemService;

        public DPIAService(IUnitOfWork unitOfWork,
            IMapper mapper,
            IEventMessageBuilder messageBuilder,
            IDPIARepository dpiaRepository,
            IFileRepository fileRepos,
            IMemberResponsibilityRepository mRes,
            IDPIAMemberRepository dpiaMember,
            IMediator mediator,
            IExternalSystemService systemService
            ) : base(unitOfWork)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _messageBuilder = messageBuilder;
            _dpiaRepository = dpiaRepository ?? throw new ArgumentNullException(nameof(dpiaRepository));
            _fileRepos = fileRepos;
            _mRes = mRes;
            _dpiaMember = dpiaMember;
            _mediator = mediator;
            _systemService = systemService;
        }

        /// <summary>
        /// TODO: Handle case DPIA regulatory
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DPIA> AddAsync(DPIACreateVM entity, ClaimsPrincipal principal)
        {
            var groupCheckResult = await _unitOfWork.Users.CheckUserInGroup(Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims")), PermissionGroup.DPO);
            if (groupCheckResult.IsFailed || !groupCheckResult.Value)
            {
                throw new UnauthorizedAccessException("User is not in DPO group");
            }

            ExternalSystem? system = await _systemService.GetByIdAsync(entity.ExternalSystemId) ?? throw new Exception("System not found");

            if (system.Status == ExternalSystemStatus.Inactive || system.Status == ExternalSystemStatus.WaitingForFIC)
            {
                throw new Exception($"Cannot create DPIA when system is in {system.Status}");
            }

            // Validate the DPIA due date and DPIAResponsibilities DueDate
            if (entity.DueDate != null && entity.DueDate < DateTime.UtcNow)
            {
                throw new ArgumentException("Due date cannot be in the past.");
            }

            if (entity.Responsibilities != null && entity.Responsibilities.Any(r => r.DueDate < DateTime.UtcNow))
            {
                throw new ArgumentException("Responsibility due date cannot be in the past.");
            }

            // Responsibility due dates should be set before the DPIA due date
            if (entity.DueDate != null && entity.Responsibilities != null)
            {
                foreach (var responsibility in entity.Responsibilities)
                {
                    if (responsibility.DueDate > entity.DueDate)
                    {
                        throw new ArgumentException("Responsibility due date cannot be later than the DPIA due date.");
                    }
                }
            }


            // Validate PICs are included in UserIds
            // Validate PICs are included in UserIds if specified
            if (entity.Responsibilities != null)
            {
                foreach (var responsibility in entity.Responsibilities)
                {
                    if (responsibility.Pic.HasValue && !responsibility.UserIds.Contains(responsibility.Pic.Value))
                    {
                        throw new ArgumentException("Person in charge must be included in the user list for the responsibility.");
                    }
                }

                // PIC is required if UserIds is specified
                if (entity.Responsibilities.Any(r => r.UserIds.Count != 0 && !r.Pic.HasValue))
                {
                    throw new ArgumentException("Person in charge must be included in the user list for the responsibility.");
                }
            }

            // Add DPIA record
            DPIA dPIA = _mapper.Map<DPIA>(entity);

            // Add DPIA Members
            List<Guid> memberIds = new List<Guid>();
            if (entity.Responsibilities != null)
            {
                memberIds = entity.Responsibilities
                    .Where(r => r.UserIds != null)
                    .SelectMany(r => r.UserIds)
                    .Distinct()
                    .ToList();
            }

            // Sort out the members with the userIds that are not existing in the database
            var existingMembers = await _unitOfWork.Users.FindAsync(m => memberIds.Contains(m.Id));

            if (existingMembers.Count() != memberIds.Count())
            {
                throw new KeyNotFoundException("User not found");
            }

            memberIds = existingMembers.Select(m => m.Id).ToList();

            List<DPIAMember> members = memberIds.Select(m => new DPIAMember { UserId = m }).ToList();

            dPIA.DPIAMembers = members;

            // Add DPIA events
            var message = _messageBuilder.BuildDPIACreatedEvent(principal.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Unknown");
            DPIAEvent dPIAEvent = new DPIAEvent
            {
                Event = message,
                UserId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)),
                EventType = DPIAEventType.Initiated,
            };

            dPIA.Events.Add(dPIAEvent);

            // Add DPIA documents
            if (entity.Documents != null && entity.Documents.Count != 0)
            {
                foreach (var file in entity.Documents)
                {
                    using Stream stream = file.OpenReadStream();
                    string key = $"{DocumentType.DPIA}/{dPIA.Id}/{file.FileName}";
                    string fileUrl = await _fileRepos.UploadFileAsync(stream, key, file.ContentType);

                    dPIA.Documents.Add(new DPIADocument
                    {
                        Title = file.FileName,
                        FileFormat = file.ContentType,
                        FileUrl = key,
                    });
                }
            }
            await _unitOfWork.DPIAs.AddAsync(dPIA);
            await _unitOfWork.SaveChangesAsync();

            // Add responsibilities
            List<MemberResponsibility> mRes = new List<MemberResponsibility>();

            foreach (var responsibility in dPIA.Responsibilities)
            {
                var mres = entity.Responsibilities?
                    .FirstOrDefault(r => r.ResponsibilityId == responsibility.ResponsibilityId)?.UserIds;
                
                if (mres == null || mres.Count == 0)
                {
                    continue; // Skip if no members are assigned to this responsibility
                }

                foreach (var member in dPIA.DPIAMembers)
                {
                    if (!mres.Contains(member.UserId))
                    {
                        continue; // Skip if the member is not assigned to this responsibility
                    }

                    // Check if the member is the PIC for this responsibility
                    // and add the member responsibility to the list
                    mRes.Add(new MemberResponsibility
                    {
                        MemberId = member.Id,
                        DPIAResponsibilityId = responsibility.Id,
                        IsPic = member.UserId == entity.Responsibilities?
                            .FirstOrDefault(r => r.ResponsibilityId == responsibility.ResponsibilityId)?.Pic // Check if the member is the PIC for this responsibility
                    });
                }
            }

            if (mRes.Count != 0)
            {
                await _mRes.BulkAddAsync(mRes);
                await _unitOfWork.SaveChangesAsync();
            }

            // raise event
            //DPIACreatedEvent dPIACreatedEvent = new DPIACreatedEvent
            //{
            //    ExternalSystemId = dPIA.ExternalSystemId,
            //};
            //await _mediator.Publish(new DPIACreatedNotification(dPIACreatedEvent));

            return dPIA;
        }

        protected override IRepository<DPIA> Repository => _dpiaRepository;
        public async Task<DPIADetailVM> GetDPIADetailByMemberId(Guid id, Guid DPIAMemberId)
        {
            var dpia = await _dpiaRepository.GetDetailAsync(id,
                d => d.ExternalSystem,
                d => d.Documents
                );


            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            try
            {
                var members = await _dpiaRepository.GetDPIAMembersAsync(id);
                // Use ToList() to materialize the result instead of casting
                dpia.DPIAMembers = members.ToList();

                var responsibilities = await _dpiaRepository.GetDPIAResponsibilitiesAsync(id);
                var filteredResponsibilities = responsibilities.Where(r =>
                    r.MemberResponsibilities.Any(mr => mr.MemberId == DPIAMemberId));

                // Materialize the filtered result before mapping
                var responsibilityList = filteredResponsibilities.ToList();

                // Now map from the materialized list
                var responsibilityVMs = _mapper.Map<List<DPIAResponsibilityListVM>>(responsibilityList);

                // Set the filtered responsibilities
                dpia.Responsibilities = responsibilityList;

                var dpiaVM = _mapper.Map<DPIADetailVM>(dpia);
                dpiaVM.Responsibilities = responsibilityVMs;
                return dpiaVM;
            }


            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<DPIADetailVM> GetDPIAByIdAsync(Guid id)
        {
            var dpia = await _dpiaRepository.GetDPIADetailAsync(id) ?? throw new Exception("DPIA not found");

            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            try
            {
                var members = await _dpiaRepository.GetDPIAMembersAsync(id);
                dpia.DPIAMembers = (List<DPIAMember>)members;

                var responsibilities = await _dpiaRepository.GetDPIAResponsibilitiesAsync(id);
                var responsibilityVMs = _mapper.Map<List<DPIAResponsibilityListVM>>((List<DPIAResponsibility>)responsibilities);
                dpia.Responsibilities = responsibilities.ToList();

                var dpiaVM = _mapper.Map<DPIADetailVM>(dpia);
                dpiaVM.Responsibilities = responsibilityVMs;
                dpiaVM.ExternalSystemId = dpia.ExternalSystemId.Value;
                dpiaVM.CreatedAt = dpia.CreatedAt;
                dpiaVM.CreatedById = dpia.CreatedById;
                dpiaVM.UpdatedById = dpia.LastModifiedById;
                return dpiaVM;
            }

            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// /// Deletes a comment by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id, ClaimsPrincipal principal)
        {
            var dpia = await _dpiaRepository.GetByIdAsync(id);
            if (dpia == null)
            {
                throw new KeyNotFoundException("DPIA not found");
            }

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }
            
            // await CheckDPOorAdminAsync(userId);

            // Check status transition (can only delete from Draft status)
            if (dpia.Status != DPIAStatus.Draft)
            {
                throw new InvalidOperationException("DPIA can only be deleted from Draft status");
            }

            // Delete the DPIA
            await _dpiaRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task<List<Comment>> GetCommentsAsync(Guid id, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));

            await CheckDPIAMembershipAsync(id, userId);
            
            var dpia = await _unitOfWork.DPIAs.GetByIdAsync(id);
            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            var comments = await _dpiaRepository.GetCommentsAsync(id);
            return comments.ToList();
        }

        public async Task<List<EventDetailVM>> GetHistoryAsync(Guid id, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));
            
            await CheckDPIAMembershipAsync(id, userId);

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }

            var dpia = await _unitOfWork.DPIAs.GetByIdAsync(id);
            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            var events = await _unitOfWork.DPIAs.GetEventsAsync(id);
            var eventVMs = _mapper.Map<List<EventDetailVM>>(events);
            return eventVMs;
        }

        public async Task SaveEventsAsync(Guid id, DPIAEvent dpiaEvent)
        {
            dpiaEvent.DPIAId = id;

            await _dpiaRepository.SaveEventsAsync(dpiaEvent);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<DPIAMemberVM>> GetMembersAsync(Guid id, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));
            await CheckDPIAMembershipAsync(id, userId);
            
            var members = await _dpiaRepository.GetDPIAMembersAsync(id);
            var memberVM = _mapper.Map<List<DPIAMemberVM>>(members);
            return memberVM;
        }

        public async Task AddCommentAsync(AddCommentVM comment, ClaimsPrincipal principal)
        {
            var dpia = await _dpiaRepository.GetByIdAsync(comment.ReferenceId);
            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));
            await CheckDPIAMembershipAsync(comment.ReferenceId, userId);
            
            // Set the user ID from claims
            comment.UserId = userId;
            
            var commentModel = _mapper.Map<Comment>(comment);
            await _dpiaRepository.SaveCommentAsync(commentModel);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddMembersAsync(Guid id, List<DPIAMemberCreateVM> members)
        {   
            var dpia = await _dpiaRepository.GetByIdAsync(id);
            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            // Map the incoming ViewModel to DPIAMember entities
            var dpiaMembers = _mapper.Map<List<DPIAMember>>(members);
            dpiaMembers.ForEach(m => m.DPIAId = id);

            // Get existing members to avoid duplication
            var existingMembers = await _dpiaRepository.GetDPIAMembersAsync(id);
            var newMembers = dpiaMembers
                .Where(m => existingMembers.All(em => em.UserId != m.UserId))
                .ToList();

            if (newMembers.Count == 0)
            {
                throw new Exception("Members already exist in the DPIA.");
            }

            try
            {
                foreach (var member in newMembers)
                {
                    // Find the existing user by ID
                    var existingUser = await _unitOfWork.Users.GetByIdAsync(member.UserId);
                    if (existingUser == null)
                    {
                        throw new KeyNotFoundException($"User with ID {member.UserId} not found.");
                    }

                    foreach (var responsibility in member.Responsibilities)
                    {
                        // Find the existing responsibility by ID
                        var existingResponsibility = await _unitOfWork.Responsibilities.GetByIdAsync(responsibility.DPIAResponsibilityId);
                        if (existingResponsibility == null)
                        {
                            throw new KeyNotFoundException($"Responsibility with ID {responsibility.DPIAResponsibilityId} not found.");
                        }
                    }
                }

                // Bulk add the members
                await _dpiaRepository.BulkAddMembersAsync(newMembers);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new Exception($"Add failed: {e.Message}");
            }
        }

        public async Task UpdateMembersAsync(Guid id, List<DPIAMemberCreateVM> members)
        {
            // Get the DPIA by ID
            var dpia = await _dpiaRepository.GetDetailAsync(id,
                d => d.ExternalSystem,
                d => d.Documents);
            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            // Map the incoming ViewModel to DPIAMember entities
            var dpiaMembers = _mapper.Map<List<DPIAMember>>(members);
            dpiaMembers.ForEach(m => m.DPIAId = id);

            // Get existing members for this DPIA
            var existingMembers = await _unitOfWork.DPIAMembers.FindAsync(m => m.DPIAId == id);

            // Determine new members to add and old members to remove
            var newMembers = dpiaMembers.Where(m => existingMembers.All(em => em.UserId != m.UserId)).ToList();
            var removedMembers = existingMembers.Where(em => dpiaMembers.All(m => m.UserId != em.UserId)).ToList();
            var updatedMembers = dpiaMembers.Where(m => existingMembers.Any(em => em.UserId == m.UserId)).ToList();

            foreach (var updatedMember in updatedMembers)
            {
                var existingMember = existingMembers.First(em => em.UserId == updatedMember.UserId);
                updatedMember.Id = existingMember.Id;
            }

            // Verify all users exist
            var allUserIds = dpiaMembers.Select(m => m.UserId).Distinct().ToList();
            foreach (var userId in allUserIds)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }
            }

            // Bulk add new members
            if (newMembers.Any())
            {
                try
                {
                    await _unitOfWork.DPIAMembers.BulkAddAsync(newMembers);
                    await _unitOfWork.SaveChangesAsync();

                    // Only send notifications to new members if DPIA is Started 
                    if (dpia.Status == DPIAStatus.Started)
                    {
                        // Send notification to new members
                        foreach (var newMember in newMembers)
                        {
                            var user = await _unitOfWork.Users.GetByIdAsync(newMember.UserId);
                            if (user != null)
                            {
                                var notification = new UserAddedToDPIANotification
                                {
                                    Email = user.Email,
                                    FullName = user.FullName,
                                    RoleName = PermissionGroup.Auditor.ToString(),
                                    DPIATitle = dpia.Title,
                                    SystemName = dpia.ExternalSystem?.Name ?? "Unknown"
                                };
                                await _mediator.Publish(notification);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to add new members: {e.Message}");
                }
            }

            // ** Update Existing Members' Responsibilities **
            if (updatedMembers.Any())
            {
                try
                {
                    await _unitOfWork.DPIAMembers.UpdateAsync(updatedMembers);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to update existing members: {e.InnerException?.Message}");
                }
            }

            // Bulk delete removed members
            if (removedMembers.Any())
            {
                try
                {
                    await _unitOfWork.DPIAMembers.BulkDeleteAsync(removedMembers);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to delete removed members: {e.Message}");
                }
            }
            // Save changes
        }

        public async Task UpdateMemberResponsibilitiesAsync(Guid id, List<DPIAResponsibilityVM> responsibilityVMs)
        {
            // Validate the DPIA exists
            var dpia = await _dpiaRepository.GetDetailAsync(id,
                d => d.Responsibilities,
                d => d.ExternalSystem
            );

            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            // Get all responsibility IDs from the view models
            var responsibilityIds = responsibilityVMs.Select(vm => vm.ResponsibilityId).ToList();

            // Retrieve existing DPIA responsibilities for this DPIA
            var existingDpiaResponsibilities = await _unitOfWork.DPIAResponsibilities
                .FindAsync(r => r.DPIAId == id && responsibilityIds.Contains(r.ResponsibilityId));

            var dpiaMembers = _unitOfWork.DPIAMembers
                .FindAsync(m => m.DPIAId == id)
                .Result.ToList();

            // Get dictionaries for faster lookups
            var dpiaResponsibilityDictionary = existingDpiaResponsibilities.ToDictionary(r => r.ResponsibilityId, r => r);
            var memberDictionary = dpiaMembers.ToDictionary(m => m.UserId, m => m);

            // Get all unique user IDs from the view models
            var allUserIds = responsibilityVMs
                .SelectMany(vm => vm.UserId)
                .Distinct()
                .ToList();

            // Create any missing DPIA members
            var missingUserIds = allUserIds
                .Where(uid => !memberDictionary.ContainsKey(uid))
                .ToList();

            if (missingUserIds.Any())
            {
                var newMembers = missingUserIds.Select(uid => new DPIAMember { UserId = uid, DPIAId = id }).ToList();
                await _unitOfWork.DPIAMembers.BulkAddAsync(newMembers);
                await _unitOfWork.SaveChangesAsync();

                foreach (var newMember in newMembers)
                {
                    memberDictionary[newMember.UserId] = newMember;
                }
            }

            // Update existing responsibilities and add new ones
            foreach (var vm in responsibilityVMs)
            {
                // Find the existing responsibility or create a new one
                if (!dpiaResponsibilityDictionary.TryGetValue(vm.ResponsibilityId, out var dpiaResponsibility))
                {
                    dpiaResponsibility = new DPIAResponsibility
                    {
                        ResponsibilityId = vm.ResponsibilityId,
                        DPIAId = id,
                        Status = ResponsibilityStatus.NotStarted,
                        DueDate = vm.DueDate,
                    };
                    await _unitOfWork.DPIAResponsibilities.AddAsync(dpiaResponsibility);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    // Update existing responsibility's DueDate if it changed
                    if (vm.DueDate != dpiaResponsibility.DueDate)
                    {
                        dpiaResponsibility.DueDate = vm.DueDate;
                        _unitOfWork.DPIAResponsibilities.Update(dpiaResponsibility);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }


                var currentAssignments = await _unitOfWork.MemberResponsibilities.FindAsync(m => m.DPIAResponsibilityId == dpiaResponsibility.Id);

                var currentUserIds = currentAssignments.Select(m => m.MemberId).ToList();

                var requestedMemberIds = vm.UserId
                    .Select(uid => memberDictionary[uid].Id)
                    .ToList();

                // Find assignments to remove
                var assignmentsToRemove = currentAssignments
                    .Where(a => !requestedMemberIds.Contains(a.MemberId))
                    .ToList();

                if (assignmentsToRemove.Any())
                {
                    // _logger.LogInformation($"Removing {assignmentsToRemove.Count} member responsibility assignments");
                    _unitOfWork.MemberResponsibilities.BulkDeleteAsync(assignmentsToRemove).Wait();
                }

                // Find member IDs to add new assignments for
                var membersIdsToAdd = requestedMemberIds
                    .Where(uid => !currentUserIds.Contains(uid))
                    .ToList();

                if (membersIdsToAdd.Any())
                {
                    // _logger.LogInformation($"Adding {membersIdsToAdd.Count} member responsibility assignments");
                    var newAssignments = membersIdsToAdd
                        .Select(uid => new MemberResponsibility
                        {
                            MemberId = uid,
                            IsPic = vm.Pic == uid,
                            DPIAResponsibilityId = dpiaResponsibility.Id
                        })
                        .ToList();

                    await _unitOfWork.MemberResponsibilities.BulkAddAsync(newAssignments);
                    await _unitOfWork.SaveChangesAsync();

                    // Only send notifications to new members if DPIA is Started
                    if (dpia.Status == DPIAStatus.Started)
                    {
                        // _logger.LogInformation($"Sending notifications to {newAssignments.Count} new members");
                        // Send notification to new members
                        foreach (var newMember in newAssignments)
                        {
                            var user = await _unitOfWork.Users.GetByIdAsync(newMember.MemberId);
                            if (user != null)
                            {
                                var notification = new UserAddedToDPIANotification
                                {
                                    Email = user.Email,
                                    FullName = user.FullName,
                                    RoleName = PermissionGroup.Auditor.ToString(),
                                    DPIATitle = dpia.Title,
                                    SystemName = dpia.ExternalSystem?.Name ?? "Unknown"
                                };
                                await _mediator.Publish(notification);
                            }
                        }
                    }
                }

                // Reload current assignments to get the latest state
                currentAssignments = await _unitOfWork.MemberResponsibilities
                    .FindAsync(m => m.DPIAResponsibilityId == dpiaResponsibility.Id);

                // Update the PIC status for the responsibility
                var picAssignment = currentAssignments.FirstOrDefault(a => a.MemberId == memberDictionary[vm.Pic].Id);
                var oldPicAssignment = currentAssignments.FirstOrDefault(a => a.IsPic == true && a.MemberId != memberDictionary[vm.Pic].Id);

                if (oldPicAssignment != null)
                {
                    oldPicAssignment.IsPic = false;
                    _unitOfWork.MemberResponsibilities.Update(oldPicAssignment);
                }
                if (picAssignment != null)
                {
                    picAssignment.IsPic = true;
                    _unitOfWork.MemberResponsibilities.Update(picAssignment);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            // Save changes to the DPIA responsibilities
            await _unitOfWork.SaveChangesAsync();
            var allMemberResponsibilities = await _unitOfWork.MemberResponsibilities
                // .FindAsync(m => m.DPIAResponsibilityId == id);
                .FindAsync(m => dpia.Responsibilities.Select(r => r.Id).Contains(m.DPIAResponsibilityId));

            var membersWithResponsibilities = allMemberResponsibilities
                .Select(mr => mr.MemberId)
                .Distinct()
                .ToList();

            // Find all members who are not in the responsibilities
            var membersToRemove = dpiaMembers
                .Where(m => !membersWithResponsibilities.Contains(m.Id))
                .ToList();

            // Remove members who are not in the responsibilities
            if (membersToRemove.Any())
            {
                await _unitOfWork.DPIAMembers.BulkDeleteAsync(membersToRemove);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteResponsibilityAsync(Guid dpiaId, Guid responsibilityId, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            // Only DPO/Admin can delete responsibilities
            await CheckDPOorAdminAsync(userId);
            
            var dpia = await _dpiaRepository.GetByIdAsync(dpiaId);
            if (dpia == null)
                throw new KeyNotFoundException("DPIA not found");

            if (dpia.Status != DPIAStatus.Draft)
                throw new InvalidOperationException("DPIA is not in Draft status");

            var dpiaResponsibility = (await _unitOfWork.DPIAResponsibilities
                .FindAsync(r => r.DPIAId == dpiaId && r.ResponsibilityId == responsibilityId))
                .FirstOrDefault();

            if (dpiaResponsibility == null)
                throw new KeyNotFoundException("Responsibility not found");

            // Find all member responsibilities to delete
            var memberResponsibilities = await _unitOfWork.MemberResponsibilities.FindAsync(m => m.DPIAResponsibilityId == dpiaResponsibility.Id);
            if (memberResponsibilities.Any())
                await _unitOfWork.MemberResponsibilities.BulkDeleteAsync(memberResponsibilities);
            await _unitOfWork.SaveChangesAsync();

            // Delete the responsibility
            await _unitOfWork.DPIAResponsibilities.DeleteAsync(dpiaResponsibility.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<DPIAResponsibilityDetailVM> GetResponsibilityAsync(Guid dpiaId, Guid responsibilityId, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            await CheckDPIAMembershipAsync(dpiaId, userId);
            
            var responsibility = (await _unitOfWork.DPIAResponsibilities
                .FindAsync(r => r.DPIAId == dpiaId && r.ResponsibilityId == responsibilityId)).FirstOrDefault();

            if (responsibility == null)
            {
                throw new KeyNotFoundException("Responsibility not found");
            }

            responsibility = await _unitOfWork.DPIAResponsibilities
                .GetDetailAsync(responsibility.Id,
                    r => r.DPIA,
                    r => r.Responsibility);

            var comments = await _unitOfWork.Comments.FindAsync(c => c.ReferenceId == responsibility.Id);
            var members = await _unitOfWork.DPIAResponsibilities.GetMembersAsync(responsibility.Id);

            var responsibilityVM = _mapper.Map<DPIAResponsibilityDetailVM>(responsibility);
            var commentsVM = _mapper.Map<List<CommentVM>>(comments);
            var membersVM = _mapper.Map<List<MemberResponsibilityVM>>(members);

            responsibilityVM.Members = membersVM;
            responsibilityVM.Comments = commentsVM;

            var dpiaWithDocs = await _dpiaRepository.GetDetailAsync(dpiaId,
                d => d.Documents);

            responsibilityVM.Documents = dpiaWithDocs?.Documents;

            return responsibilityVM;
        }

        public async Task RestartResponsibilityAsync(Guid id, Guid responsibilityId, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));
            
            // Get the responsibility first to check against it
            var responsibility = (await _unitOfWork.DPIAResponsibilities
                .FindAsync(r => r.DPIAId == id && r.ResponsibilityId == responsibilityId))
                .FirstOrDefault();

            if (responsibility == null)
                throw new KeyNotFoundException("DPIA responsibility not found");
                
            // Check if user is PIC, DPO, or admin
            await CheckResponsibilityPICAsync(id, responsibility.Id, userId);

            if (responsibility.Status != ResponsibilityStatus.Completed)
                throw new InvalidOperationException("Responsibility must be completed before restarting");

            // Restart all member responsibilities (change status to In Progress)
            var memberResponsibilities = await _unitOfWork.MemberResponsibilities.FindAsync(m => m.DPIAResponsibilityId == responsibility.Id);
            if (memberResponsibilities.Any())
                foreach (var memberResponsibility in memberResponsibilities)
                    memberResponsibility.CompletionStatus = CompletionStatus.InProgress;

            responsibility.Status = ResponsibilityStatus.InProgress;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task StartDPIAAsync(Guid id, ClaimsPrincipal principal)
        {
            try
            {
                // Get the user ID from claims
                var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));

                // Check if user is in DPO group
                // var groupCheckResult = await _unitOfWork.Users.CheckUserInGroup(userId,     PermissionGroup.DPO);
                // if (groupCheckResult.IsFailed || !groupCheckResult.Value)
                //     throw new Exception("User is not in DPO group");

                await CheckDPOorAdminAsync(userId);

                // Get user details for event messaging
                var userResult = await _unitOfWork.Users.GetUserProfileAsync(userId);

                var user = userResult.Value;

                // Retrieve the DPIA
                var dpia = await _dpiaRepository.GetByIdAsync(id);
                if (dpia == null)
                    throw new Exception("DPIA not found");
                // Validate status transition (can only start from Draft)
                if (dpia.Status != DPIAStatus.Draft)
                    throw new Exception("DPIA can only be started from Draft status");
                var dpiaResponsibilities = await _unitOfWork.DPIAResponsibilities.FindAsync(r => r.DPIAId == id);
                if (!dpiaResponsibilities.Any())
                    throw new Exception("DPIA has no responsibilities");

                // Check if all responsibilities are ready  
                var allResponsibilitiesReady = dpiaResponsibilities.All(r => r.Status == ResponsibilityStatus.Ready);
                if (!allResponsibilitiesReady)
                    throw new Exception("All responsibilities must be ready before starting DPIA");

                // Update DPIA status
                dpia.Status = DPIAStatus.Started;

                // Update all responsibilities to In Progress
                foreach (var responsibility in dpiaResponsibilities)
                {
                    responsibility.Status = ResponsibilityStatus.InProgress;
                    _unitOfWork.DPIAResponsibilities.Update(responsibility);
                }


                // Create DPIA event
                var eventMessage = _messageBuilder.BuildDPIAStatusChangeEvent(
                    user.FullName,
                    PermissionGroup.QAManager,
                    "Started");

                var dpiaEvent = new DPIAEvent
                {
                    DPIAId = id,
                    Event = eventMessage,
                    EventType = DPIAEventType.Updated,
                    UserId = userId
                };

                // Save the event
                await _dpiaRepository.SaveEventsAsync(dpiaEvent);
                await _unitOfWork.SaveChangesAsync();

                // get all DPIA members
                List<string> emails = await _dpiaMember.GetDpiaMemberEmail(id);

                // send email notification to all members
                DPIAStartedEvent startedEvent = new DPIAStartedEvent
                {
                    QAManagerEmail = user.Email,
                    QAManagerName = user.FullName,
                    DPIAName = dpia.Title,
                    DPIAStartTime = dpiaEvent.CreatedAt,
                    Emails = emails
                };

                await _mediator.Publish(new DPIAStartedNotification(startedEvent));
            }
            catch (Exception)
            {
                // Handle exceptions (e.g., log them)
                throw;
            }
        }

        public async Task RequestApprovalAsync(Guid id, ClaimsPrincipal principal)
        {
            try
            {
                // Get the user ID from claims
                var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == Guid.Empty)
                    throw new Exception("User ID not found in claims");

                // Get user details for event messaging
                var userResult = await _unitOfWork.Users.GetUserProfileAsync(userId);
                if (userResult.IsFailed)
                    throw new KeyNotFoundException("User not found");

                var user = userResult.Value;

                // Retrieve the DPIA
                var dpia = await _dpiaRepository.GetDetailAsync(id,
                    d => d.Responsibilities,
                    d => d.ExternalSystem
                );
                if (dpia == null)
                    throw new KeyNotFoundException("DPIA not found");

                // Validate status transition (can only request approval from Started status)
                if (dpia.Status != DPIAStatus.Started)
                    throw new InvalidOperationException("DPIA can only be requested for approval from Started status");

                // Check if all responsibilities are completed
                var allResponsibilitiesComplete = dpia.Responsibilities.All(r => r.Status == ResponsibilityStatus.Completed);

                if (dpia.Responsibilities.Count == 0)
                    allResponsibilitiesComplete = true; // No responsibilities to check

                if (!allResponsibilitiesComplete)
                    throw new InvalidOperationException("All responsibilities must be completed before requesting approval");

                // Update DPIA status
                dpia.Status = DPIAStatus.Approved;

                // Check if user is in QA Manager group
                var groupCheckResult = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.QAManager);
                if (groupCheckResult.IsFailed || !groupCheckResult.Value)
                    throw new Exception("User is not in QA Manager group");

                // Create DPIA event
                var eventMessage = _messageBuilder.BuildDPIAReviewRequestEvent(
                    user.FullName,
                    PermissionGroup.QAManager);

                var dpiaEvent = new DPIAEvent
                {
                    DPIAId = id,
                    Event = eventMessage,
                    EventType = DPIAEventType.Requested,
                    UserId = userId
                };

                // Save the event
                await _dpiaRepository.SaveEventsAsync(dpiaEvent);
                await _unitOfWork.SaveChangesAsync();

                // Send notification to DPO who created the DPIA
                var dpo = await _unitOfWork.Users.GetByIdAsync(dpia.CreatedBy.Id);
                if (dpo != null)
                {
                    var notification = new DPIAReviewRequestNotification
                    {
                        Email = dpo.Email,
                        FullName = dpo.FullName,
                        DPIAName = dpia.Title,
                        Timestamp = DateTime.UtcNow,
                        SystemName = dpia.ExternalSystem?.Name ?? "Unknown"
                    };
                    await _mediator.Publish(notification);
                }
            }
            catch (Exception)
            {
                // Handle exceptions (e.g., log them)
                throw;
            }
        }

        public async Task ApproveAsync(Guid id, ClaimsPrincipal principal)
        {
            try
            {
                // Get the user ID from claims
                var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in claims");

                await CheckDPOorAdminAsync(userId);
                // // Check if user is in DPO group
                // var groupCheckResult = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.DPO);
                // if (groupCheckResult.IsFailed || !groupCheckResult.Value)
                //     throw new UnauthorizedAccessException("User is not in DPO group");

                // Get user details for event messaging
                var userResult = await _unitOfWork.Users.GetUserProfileAsync(userId);
                if (userResult.IsFailed)
                    throw new KeyNotFoundException("User not found");

                var user = userResult.Value;

                // Retrieve the DPIA
                var dpia = await _dpiaRepository.GetDetailAsync(id,
                    d => d.Responsibilities,
                    d => d.ExternalSystem
                );
                if (dpia == null)
                    throw new KeyNotFoundException("DPIA not found");

                // Validate status transition (can only approve from Started status)
                if (dpia.Status != DPIAStatus.Started)
                    throw new InvalidOperationException("DPIA can only be approved from Started status");

                // Check if all responsibilities are completed
                var allResponsibilitiesComplete = dpia.Responsibilities.All(r => r.Status == ResponsibilityStatus.Completed);
                if (!allResponsibilitiesComplete)
                    throw new InvalidOperationException("All responsibilities must be completed before approving DPIA");

                // Update DPIA status
                dpia.Status = DPIAStatus.Approved;

                // Create DPIA event
                var eventMessage = _messageBuilder.BuildDPIAApprovalEvent(
                    user.FullName,
                    "approved");

                var dpiaEvent = new DPIAEvent
                {
                    DPIAId = id,
                    Event = eventMessage,
                    EventType = DPIAEventType.Approved,
                    UserId = userId
                };

                // Save the event
                await _dpiaRepository.SaveEventsAsync(dpiaEvent);
                await _unitOfWork.SaveChangesAsync();

                // Send notification to all members of the DPIA
                var members = await _dpiaRepository.GetDPIAMembersAsync(id);
                var dpo = await _unitOfWork.Users.GetByIdAsync(dpia.CreatedBy.Id);
                foreach (var member in members)
                {
                    var userMember = await _unitOfWork.Users.GetByIdAsync(member.UserId);
                    if (userMember != null)
                    {
                        var notification = new DPIAApprovalNotification
                        {
                            Email = userMember.Email,
                            FullName = userMember.FullName,
                            DPIAName = dpia.Title,
                            IsApproved = true,
                            DPOName = dpo.FullName,
                            Timestamp = DateTime.UtcNow,
                            SystemName = dpia.ExternalSystem?.Name ?? "Unknown"
                        };
                        await _mediator.Publish(notification);
                    }
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                throw new Exception($"Error approving DPIA: {ex.Message}");
            }
        }

        public async Task RejectAsync(Guid id, ClaimsPrincipal principal)
        {
            try
            {
                // Get the user ID from claims
                var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in claims");

                // Check if user is in DPO group
                // await CheckDPOorAdminAsync(userId);
               

                // Get user details for event messaging
                var userResult = await _unitOfWork.Users.GetUserProfileAsync(userId);
                if (userResult.IsFailed)
                    throw new KeyNotFoundException("User not found");

                var user = userResult.Value;

                // Retrieve the DPIA
                var dpia = await _dpiaRepository.GetDetailAsync(id,
                    d => d.Responsibilities,
                    d => d.ExternalSystem
                );
                if (dpia == null)
                    throw new KeyNotFoundException("DPIA not found");

                // Validate status transition (can only reject from Started status)
                if (dpia.Status != DPIAStatus.Started)
                    throw new InvalidOperationException("DPIA can only be rejected from Started status");

                // Update DPIA status
                dpia.Status = DPIAStatus.Rejected;

                // Create DPIA event
                var eventMessage = _messageBuilder.BuildDPIAApprovalEvent(
                    user.FullName,
                    "rejected");

                var dpiaEvent = new DPIAEvent
                {
                    DPIAId = id,
                    Event = eventMessage,
                    EventType = DPIAEventType.Rejected,
                    UserId = userId
                };

                // Save the event
                await _dpiaRepository.SaveEventsAsync(dpiaEvent);
                await _unitOfWork.SaveChangesAsync();

                // Send notification to all members of the DPIA
                var members = await _dpiaRepository.GetDPIAMembersAsync(id);
                var dpo = await _unitOfWork.Users.GetByIdAsync(dpia.CreatedBy.Id);
                foreach (var member in members)
                {
                    var userMember = await _unitOfWork.Users.GetByIdAsync(member.UserId);
                    if (userMember != null)
                    {
                        var notification = new DPIAApprovalNotification
                        {
                            Email = userMember.Email,
                            FullName = userMember.FullName,
                            DPIAName = dpia.Title,
                            IsApproved = false,
                            DPOName = dpo.FullName,
                            Timestamp = DateTime.UtcNow,
                            SystemName = dpia.ExternalSystem?.Name ?? "Unknown"
                        };
                        await _mediator.Publish(notification);
                    }
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                throw new Exception($"Error rejecting DPIA: {ex.Message}");
            }
        }

        public async Task RestartAsync(Guid id, ClaimsPrincipal principal)
        {
            try
            {
                // Get the user ID from claims
                var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in claims");


                // Get user details for event messaging
                var userResult = await _unitOfWork.Users.GetUserProfileAsync(userId);
                if (userResult.IsFailed)
                    throw new KeyNotFoundException("User not found");

                var user = userResult.Value;

                // Retrieve the DPIA
                var dpia = await _dpiaRepository.GetByIdAsync(id);
                if (dpia == null)
                    throw new KeyNotFoundException("DPIA not found");

                // Validate status transition (can only restart from Rejected status)
                if (dpia.Status != DPIAStatus.Rejected)
                    throw new InvalidOperationException("DPIA can only be restarted from Rejected status");

                // Update DPIA status
                dpia.Status = DPIAStatus.Started;

                // Check if user is in QA Manager group
                var groupCheckResult = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.QAManager);
                if (groupCheckResult.IsFailed || !groupCheckResult.Value)
                    throw new Exception("User is not in QA Manager group");

                // Create DPIA event
                var eventMessage = _messageBuilder.BuildDPIAStatusChangeEvent(
                    user.FullName,
                    PermissionGroup.QAManager,
                    "Started (Restarted)");

                var dpiaEvent = new DPIAEvent
                {
                    DPIAId = id,
                    Event = eventMessage,
                    EventType = DPIAEventType.Updated,
                    UserId = userId
                };

                // Save the event
                await _dpiaRepository.SaveEventsAsync(dpiaEvent);
                await _unitOfWork.SaveChangesAsync();

                // return Result.Ok();
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                throw new Exception($"Error restarting DPIA: {ex.Message}");
            }
        }

        public async Task UpdateResponsibilityMembersAsync(Guid dpiaId, Guid responsibilityId, List<Guid> userIds, Guid? picId, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get responsibility to pass the correct ID
            var dpiaResponsibility = await _unitOfWork.DPIAResponsibilities
                .FindAsync(r => r.DPIAId == dpiaId && r.ResponsibilityId == responsibilityId);

            if (!dpiaResponsibility.Any())
            {
                throw new Exception("Responsibility not found for this DPIA");
            }
            
            // Check if user is PIC, DPO, or admin
            await CheckResponsibilityPICAsync(dpiaId, dpiaResponsibility.First().Id, userId);
            
            // Continue with the original implementation
            // Validate the DPIA exists
            var dpia = await _dpiaRepository.GetDetailAsync(dpiaId,
                d => d.Responsibilities,
                d => d.ExternalSystem
            );
            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            var responsibility = dpiaResponsibility.First();

            // Get existing DPIA members
            var dpiaMembers = await _unitOfWork.DPIAMembers.FindAsync(m => m.DPIAId == dpiaId);
            var memberDictionary = dpiaMembers.ToDictionary(m => m.UserId, m => m);

            // Create any missing DPIA members
            var missingUserIds = userIds
                .Where(uid => !memberDictionary.ContainsKey(uid))
                .ToList();

            if (missingUserIds.Any())
            {
                var newMembers = missingUserIds
                    .Select(uid => new DPIAMember { UserId = uid, DPIAId = dpiaId })
                    .ToList();

                await _unitOfWork.DPIAMembers.BulkAddAsync(newMembers);
                await _unitOfWork.SaveChangesAsync();

                // Only send notifications if DPIA is in the Started status
                if (dpia.Status == DPIAStatus.Started)
                {
                    // Send notification to new members
                    foreach (var newMember in newMembers)
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(newMember.UserId);
                        if (user != null)
                        {
                            var notification = new UserAddedToDPIANotification
                            {
                                Email = user.Email,
                                FullName = user.FullName,
                                RoleName = PermissionGroup.Auditor.ToString(),
                                DPIATitle = dpia.Title,
                                SystemName = dpia.ExternalSystem?.Name ?? "Unknown"
                            };
                            await _mediator.Publish(notification);
                        }
                    }
                }

                // Update member dictionary with new members
                foreach (var newMember in newMembers)
                {
                    memberDictionary[newMember.UserId] = newMember;
                }
            }

            // Get all current member assignments for this responsibility
            var currentAssignments = await _unitOfWork.MemberResponsibilities.FindAsync(m => m.DPIAResponsibilityId == responsibility.Id);

            // Create a lookup of existing assignments by member ID
            var existingAssignmentsByMemberId = currentAssignments.ToDictionary(a => a.MemberId);

            // Get member IDs for the requested users
            var requestedMemberIds = userIds
                .Where(uid => memberDictionary.ContainsKey(uid))
                .Select(uid => memberDictionary[uid].Id)
                .ToHashSet();

            // Find assignments to remove
            var assignmentsToRemove = currentAssignments
                .Where(a => !requestedMemberIds.Contains(a.MemberId))
                .ToList();

            if (assignmentsToRemove.Any())
            {
                await _unitOfWork.MemberResponsibilities.BulkDeleteAsync(assignmentsToRemove);
                await _unitOfWork.SaveChangesAsync();
            }

            // Find member IDs that need new assignments
            var memberIdsToAdd = requestedMemberIds
                .Where(mid => !existingAssignmentsByMemberId.ContainsKey(mid))
                .ToList();

            if (memberIdsToAdd.Any())
            {
                // Only create assignments for members that don't already have one
                var newAssignments = memberIdsToAdd
                    .Select(mid =>
                    {
                        // Find the original user ID for this member
                        var userId = memberDictionary.FirstOrDefault(kvp => kvp.Value.Id == mid).Key;

                        return new MemberResponsibility
                        {
                            MemberId = mid,
                            DPIAResponsibilityId = responsibility.Id,
                            CompletionStatus = CompletionStatus.NotStarted,
                            IsPic = userId == picId
                        };
                    })
                    .ToList();

                await _unitOfWork.MemberResponsibilities.BulkAddAsync(newAssignments);
                await _unitOfWork.SaveChangesAsync();
            }

            // Handle PIC status updates for existing assignments
            if (picId.HasValue)
            {
                // Find the member ID for the PIC
                if (memberDictionary.TryGetValue(picId.Value, out var picMember))
                {
                    // Update PIC status for all existing assignments
                    foreach (var assignment in currentAssignments)
                    {
                        bool shouldBePic = assignment.MemberId == picMember.Id;

                        // Only update if the PIC status needs to change
                        if (assignment.IsPic != shouldBePic)
                        {
                            assignment.IsPic = shouldBePic;
                            _unitOfWork.MemberResponsibilities.Update(assignment);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
            }
            else
            {
                // If no PIC provided, clear all PIC flags
                var picAssignments = currentAssignments.Where(a => a.IsPic).ToList();
                foreach (var assignment in picAssignments)
                {
                    assignment.IsPic = false;
                    _unitOfWork.MemberResponsibilities.Update(assignment);
                }

                if (picAssignments.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

        public async Task UploadDocumentAsync(Guid dpiaId, Guid responsibilityId, IFormFile file, ClaimsPrincipal claims)
        {
            try
            {
                // Get the user ID from claims
                var userId = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in claims");

                // Get DPIA with detail
                var dpia = await _dpiaRepository.GetDetailAsync(dpiaId);
                if (dpia == null)
                    throw new KeyNotFoundException("DPIA not found");

                // Check if the responsibility exists for this DPIA
                var responsibility = await _unitOfWork.DPIAResponsibilities
                    .FindAsync(r => r.DPIAId == dpiaId && r.ResponsibilityId == responsibilityId);

                if (!responsibility.Any())
                {
                    throw new KeyNotFoundException("Responsibility not found for this DPIA");
                }

                var dpiaResponsibility = responsibility.First();

                // Get the DPIA member for the current user
                // var dpiaMember = await _unitOfWork.DPIAMembers
                //     .FindAsync(m => m.DPIAId == dpiaId && m.UserId == userId);

                // if (!dpiaMember.Any())
                // {
                //     throw new UnauthorizedAccessException("User is not a member of this DPIA");
                // }

                // var currentMember = dpiaMember.First();

                // // Check if user is assigned to this responsibility
                // var memberResponsibility = await _unitOfWork.MemberResponsibilities.FindAsync(
                //     mr => mr.MemberId == currentMember.Id &&
                //           mr.DPIAResponsibilityId == dpiaResponsibility.Id);
                // if (!memberResponsibility.Any())
                // {
                //     throw new UnauthorizedAccessException("User is not assigned to this responsibility");
                // }

                await CheckResponsibilityMemberAsync(dpiaId, dpiaResponsibility.Id, userId);

                // Upload file to storage
                using Stream stream = file.OpenReadStream();
                string key = $"{DocumentType.DPIA}/{dpiaId}/Responsibility/{responsibilityId}/{file.FileName}";
                string fileUrl = await _fileRepos.UploadFileAsync(stream, key, file.ContentType);

                var document = new DPIADocument
                {
                    Title = file.FileName,
                    FileFormat = file.ContentType,
                    FileUrl = key,
                    ResponsibleId = dpiaResponsibility.Id
                };

                // Save the document to the database
                await _unitOfWork.DPIAResponsibilities.UploadDocumentAsync(dpiaId, responsibilityId, document);
                await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                throw new Exception($"Error uploading document: {ex.Message}");
            }
        }

        

        public async Task DeleteDocumentAsync(Guid id, Guid documentId, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            await CheckDPIAMembershipAsync(id, userId);
            
            var document = await _unitOfWork.DPIADocuments.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new KeyNotFoundException("Document not found");
            }

            // Try to delete the file from storage, but don't fail if it doesn't exist
            var result = await _fileRepos.DeleteFileAsync(document.FileUrl);
            if (!result)
            {
                throw new Exception("File not found in storage");
            }

            await _unitOfWork.DPIADocuments.DeleteAsync(documentId);
            await _unitOfWork.SaveChangesAsync();
        }


        public async Task UpdateMemberResponsibilityStatusAsync(Guid id, MemberTaskStatus status, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));
            await CheckDPIAMembershipAsync(id, userId);
            
            // Validate the DPIA exists
            var dpia = await _dpiaRepository.GetDetailAsync(id, d => d.Responsibilities);
            if (dpia == null)
            {
                throw new KeyNotFoundException("DPIA not found");
            }

            var memberResponsibility = await _unitOfWork.MemberResponsibilities.GetByIdAsync(status.MemberResponsibilityId);
            if (memberResponsibility == null)
            {
                throw new KeyNotFoundException("Member responsibility not found");
            }

            // Check if the member is part of the DPIA
            var dpiaResponsibility = await _unitOfWork.DPIAResponsibilities
                .FindAsync(r => r.DPIAId == id && r.Id == memberResponsibility.DPIAResponsibilityId);
            if (!dpiaResponsibility.Any())
            {
                throw new KeyNotFoundException("Responsibility not found for this DPIA");
            }

            // Update the status
            memberResponsibility.CompletionStatus = status.CompletionStatus;

            _unitOfWork.MemberResponsibilities.Update(memberResponsibility);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateResponsibilityStatusAsync(Guid dpiaId, DPIAResponsibilityUpdateStatusVM status, ClaimsPrincipal principal)
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));
            
            // Validate the DPIA exists
            var dpia = await _dpiaRepository.GetDetailAsync(dpiaId, d => d.Responsibilities, d => d.ExternalSystem);
            if (dpia == null)
            {
                throw new Exception("DPIA not found");
            }

            var dpiaResponsibility = await _unitOfWork.DPIAResponsibilities.GetDetailAsync(status.DPIAResponsibilityId, r => r.Responsibility);
            if (dpiaResponsibility == null)
            {
                throw new Exception("Responsibility not found");
            }
            
            // Check if user is PIC, DPO, or admin
            await CheckResponsibilityPICAsync(dpiaId, dpiaResponsibility.Id, userId);

            // Update the status
            dpiaResponsibility.Status = status.Status;
            _unitOfWork.DPIAResponsibilities.Update(dpiaResponsibility);
            await _unitOfWork.SaveChangesAsync();

            // If the responsibility is completed, notify to DPO (mail)
            if (status.Status == ResponsibilityStatus.Completed)
            {
                var dpo = await _unitOfWork.Users.GetByIdAsync(dpia.CreatedById);
                if (dpo != null)
                {
                    var notification = new ResponsibilityCompletedNotification
                    {
                        Email = dpo.Email,
                        FullName = dpo.FullName,
                        ResponsibilityName = dpiaResponsibility.Responsibility.Title,
                        DPIAName = dpia.Title,
                        SystemName = dpia?.ExternalSystem?.Name ?? "Unknown"
                    };
                    await _mediator.Publish(notification);
                }
            }
        }

        public async Task UpdateCommentAsync(Guid id, AddCommentVM comment, ClaimsPrincipal principal)
        {
            try
            {
                var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in claims"));
                comment.UserId = userId;
                
                var commentEntity = _mapper.Map<Comment>(comment);

                var existingComment = await _unitOfWork.Comments.GetByIdAsync(id);
                if (existingComment == null)
                {
                    throw new KeyNotFoundException("Comment not found");
                }

                if (existingComment.UserId != commentEntity.UserId)
                {
                    throw new UnauthorizedAccessException("User is not authorized to update this comment");
                }

                // Verify DPIA membership
                await CheckDPIAMembershipAsync(existingComment.ReferenceId, userId);

                // Update the comment entity
                existingComment.Content = commentEntity.Content;

                _unitOfWork.Comments.Update(existingComment);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Update failed: {ex.Message}");
            }
        }

        public async Task UploadDocumentAsync(Guid id, IFormFile file, ClaimsPrincipal user)
        {
            try
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in claims");

                await CheckDPIAMembershipAsync(id, userId);

                // Get DPIA with detail
                var dpia = await _dpiaRepository.GetDetailAsync(id);
                if (dpia == null)
                    throw new KeyNotFoundException("DPIA not found");

                // Upload file to storage
                using Stream stream = file.OpenReadStream();
                string key = $"{DocumentType.DPIA}/{id}/Document/{file.FileName}";
                string fileUrl = await _fileRepos.UploadFileAsync(stream, key, file.ContentType);

                var document = new DPIADocument
                {
                    Title = file.FileName,
                    FileFormat = file.ContentType,
                    FileUrl = key,
                    ResponsibleId = id
                };

                // Save the document to the database
                await _dpiaRepository.UploadDocumentAsync(id, document);
                await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading document: {ex.Message}");
            }
        }

        public async Task<IEnumerable<UserVM>> GetUsersForDPIA()
        {
            QueryParams query = new QueryParams() { PageSize = int.MaxValue };
            var groups = await _unitOfWork.Groups.GetPagedAsync(query, g => g.Users);

            var users = groups.Data
                .SelectMany(g => g.Users)
                .Where(u => u.Status == UserStatus.Activated)
                .Distinct()
                .ToList();
            return _mapper.Map<List<UserVM>>(users);
        }
    
        public async Task<PagedResponse<DPIAListVM>> GetDPIAs(QueryParams queryParams, ClaimsPrincipal principal)
        {
            // Get the user ID from claims
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Check if user is DPO/Admin (can view all DPIAs)
            var isDPO = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.DPO);
            var isAdmin = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS);
            
            // User can view all if they are either DPO or Admin
            bool canViewAll = (isDPO.IsSuccess && isDPO.Value) || (isAdmin.IsSuccess && isAdmin.Value);

            PagedResponse<DPIA> dpias;

            // For non-admin users, we need to manually filter their DPIAs
            if (!canViewAll)
            {
                // First get all DPIA IDs that the user is a member of
                var userDPIAIds = await _unitOfWork.DPIAMembers
                    .FindAsync(m => m.UserId == userId)
                    .ContinueWith(t => t.Result.Select(m => m.DPIAId).ToList());

                // Add a filter to only show these DPIAs
                if (!queryParams.Filters.ContainsKey("Id"))
                {
                    queryParams.Filters["Ids"] = string.Join(",", userDPIAIds);
                }

                // Filter the dpias collection to only include those in userDPIAIds
            }

            // Now get the DPIAs with the filtered IDs
            dpias = await _dpiaRepository.GetPagedAsync(
                queryParams,
                d => d.ExternalSystem,
                d => d.CreatedBy,
                d => d.LastModifiedBy);

            return _mapper.Map<PagedResponse<DPIAListVM>>(dpias);
        }

        private async Task CheckDPIAMembershipAsync(Guid id, Guid userId)
        {
            var isDPO = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.DPO);
            var isAdmin = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS);
            
            bool canViewAll = (isDPO.IsSuccess && isDPO.Value) || (isAdmin.IsSuccess && isAdmin.Value);
            if (canViewAll)
            {
                return;
            }

            var dpiaMember = await _unitOfWork.DPIAMembers.FindAsync(m => m.DPIAId == id && m.UserId == userId);
            if (!dpiaMember.Any())
            {
                throw new UnauthorizedAccessException("User is not a member of this DPIA");
            }
        }

        private async Task CheckResponsibilityPICAsync(Guid dpiaId, Guid responsibilityId, Guid userId)
        {
            // DPO and ADMIN always have access
            var isDPO = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.DPO);
            var isAdmin = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS);
            
            if ((isDPO.IsSuccess && isDPO.Value) || (isAdmin.IsSuccess && isAdmin.Value))
            {
                return;
            }

            // Get the member record for this user
            var dpiaMember = await _unitOfWork.DPIAMembers.FindAsync(m => m.DPIAId == dpiaId && m.UserId == userId);
            if (!dpiaMember.Any())
            {
                throw new UnauthorizedAccessException("User is not a member of this DPIA");
            }

            var memberId = dpiaMember.First().Id;
            
            // Get the responsibility
            var responsibility = await _unitOfWork.DPIAResponsibilities
                .FindAsync(r => r.Id == responsibilityId && r.DPIAId == dpiaId);
                
            if (!responsibility.Any())
            {
                throw new KeyNotFoundException("Responsibility not found");
            }
            
            // Check if user is PIC for this responsibility
            var memberResponsibility = await _unitOfWork.MemberResponsibilities.FindAsync(
                mr => mr.DPIAResponsibilityId == responsibility.First().Id && 
                mr.MemberId == memberId && 
                mr.IsPic);
                
            if (!memberResponsibility.Any())
            {
                throw new UnauthorizedAccessException("Only the PIC, DPO, or admin can perform this action");
            }
        }

        private async Task CheckResponsibilityMemberAsync(Guid dpiaId, Guid ResponsibilityId, Guid userId)
        {

            // DPO and ADMIN always have access
            var isDPO = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.DPO);
            var isAdmin = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS);
            
            if ((isDPO.IsSuccess && isDPO.Value) || (isAdmin.IsSuccess && isAdmin.Value))
            {
                return;
            }

            // Get the member record for this user
            var dpiaMember = await _unitOfWork.DPIAMembers.FindAsync(m => m.DPIAId == dpiaId && m.UserId == userId);
            if (!dpiaMember.Any())
            {
                throw new UnauthorizedAccessException("User is not a member of this DPIA");
            }

            var memberResponsibility = await _unitOfWork.MemberResponsibilities
                .FindAsync(m => m.DPIAResponsibilityId == ResponsibilityId && m.MemberId == dpiaMember.First().Id);
            if (!memberResponsibility.Any())
            {
                throw new UnauthorizedAccessException("User is not assigned to this responsibility");
            }
        }

        private async Task CheckDPOorAdminAsync(Guid userId)
        {
            var isDPO = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.DPO);
            var isAdmin = await _unitOfWork.Users.CheckUserInGroup(userId, PermissionGroup.ADMIN_DPMS);
            
            if ((!isDPO.IsSuccess || !isDPO.Value) && (!isAdmin.IsSuccess || !isAdmin.Value))
            {
                throw new UnauthorizedAccessException("Only DPO or admin can perform this action");
            }
        }

        
    }

}