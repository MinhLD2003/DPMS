using AutoMapper;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using DPMS_WebAPI.ViewModels.Group;
using DPMS_WebAPI.ViewModels.Purpose;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Services
{
    public class ExternalSystemService : BaseService<ExternalSystem>, IExternalSystemService
    {
        private readonly IExternalSystemRepository _externalSystemRepository;
        private readonly IMapper _mapper;
        private readonly IGroupRepository _groupRepository;
        private readonly IExternalSystemPurposeRepository _externalSystemPurposeRepository;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        public ExternalSystemService(IUnitOfWork unitOfWork,
            IExternalSystemPurposeRepository externalSystemPurposeRepository,
            IExternalSystemRepository externalSystemRepository,
            IMapper mapper,
            IGroupRepository groupRepository,
            IMediator mediator,
            IUserService userService
            )
            : base(unitOfWork)
        {
            _externalSystemPurposeRepository = externalSystemPurposeRepository;
            _externalSystemRepository = externalSystemRepository;
            _mapper = mapper;
            _groupRepository = groupRepository;
            _mediator = mediator;
            _userService = userService;
            // _userRepository = unitOfWork.Users;
        }

        protected override IRepository<ExternalSystem> Repository => _unitOfWork.ExternalSystems;

        public async Task<ExternalSystem> AddExternalSystem(AddSystemVM model)
        {
            // First validate all users exist
            var businessOwners = await _unitOfWork.Users
                .FindAsync(u => model.BusinessOwnerEmails.Contains(u.Email));

            if (businessOwners.Count() != model.BusinessOwnerEmails.Count)
            {
                throw new ArgumentException("Some Business Owner emails do not exist.");
            }

            var productDevs = await _unitOfWork.Users
                .FindAsync(u => model.ProductDevEmails.Contains(u.Email));

            if (productDevs.Count() != model.ProductDevEmails.Count)
            {
                throw new ArgumentException("Some Product Developer emails do not exist.");
            }

            // Create and add the external system
            ExternalSystem externalSystem = new ExternalSystem
            {
                Name = model.Name,
                Domain = model.Domain,
                Description = model.Description,
                Status = ExternalSystemStatus.WaitingForFIC,
            };  

            await Repository.AddAsync(externalSystem);
            await _unitOfWork.SaveChangesAsync();

            // Create and add the groups
            var group_BO = new Group
            {
                Name = $"BO {model.Name}",
                Description = $"Business Owner Group for {model.Name}",
                SystemId = externalSystem.Id,
                Users = new List<User>()
            };

            var group_PD = new Group
            {
                Name = $"PD {model.Name}",
                Description = $"Product Developer Group for {model.Name}",
                SystemId = externalSystem.Id,
                Users = new List<User>()
            };

            await _unitOfWork.Groups.AddAsync(group_BO);
            await _unitOfWork.Groups.AddAsync(group_PD);
            await _unitOfWork.SaveChangesAsync();

            // Add users to groups
            group_BO.Users.AddRange(businessOwners);
            group_PD.Users.AddRange(productDevs);

            externalSystem.Groups.Add(group_BO);
            externalSystem.Groups.Add(group_PD);

            await _unitOfWork.SaveChangesAsync();

            // Send notifications
            foreach (var user in businessOwners)
            {
                var notification = new UserAddedToSystemNotification
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleName = group_BO.Name,
                    SystemName = externalSystem.Name
                };

                await _mediator.Publish(notification);
            }

            foreach (var user in productDevs)
            {
                var notification = new UserAddedToSystemNotification
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleName = group_PD.Name,
                    SystemName = externalSystem.Name
                };

                await _mediator.Publish(notification);
            }

            return externalSystem;
        }

        // public IEnumerable<User> GetUsersFromExternalSystem(Guid systemId)
        // {

        // }

        public Task<IEnumerable<User>> GetUsersFromExternalSystem(Guid systemId)
        {
            throw new NotImplementedException();
        }


        public async Task<ExternalSystemPurpose> AddPurposeToSystemAsync(Guid SystemId, Guid purposeId)
        {
            var system = await _unitOfWork.ExternalSystems.GetByIdAsync(SystemId);
            if (system == null) throw new ArgumentException("External System not found.");

            var purpose = await _unitOfWork.Purposes.GetByIdAsync(purposeId);
            if (purpose == null) throw new ArgumentException("Purpose not found.");

            var existingRelation = await _unitOfWork.ExternalSystemPurposes
                .FindAsync(e => e.ExternalSystemId == SystemId && e.PurposeId == purposeId);

            if (existingRelation.Any())
                throw new InvalidOperationException("This Purpose is already linked to the External System.");

            var newRelation = new ExternalSystemPurpose
            {
                ExternalSystemId = SystemId,
                PurposeId = purposeId
            };

            await _unitOfWork.ExternalSystemPurposes.AddAsync(newRelation);
            await _unitOfWork.SaveChangesAsync();
            return newRelation;
        }

        public async Task<IEnumerable<ExternalSystemPurpose>> BulkAddPurposeToSystemAsync(Guid systemId, List<Guid> purposeIds)
        {
            try
            {
                var validPurposes = await _unitOfWork.Purposes.FindAsync(p => purposeIds.Contains(p.Id));
                var validPurposeIds = validPurposes.Select(p => p.Id).ToList();

                if (!validPurposeIds.Any())
                    throw new ArgumentException("No valid Purpose IDs provided.");

                var existingRelations = await _unitOfWork.ExternalSystemPurposes
                    .FindAsync(esp => esp.ExternalSystemId == systemId);

                if (existingRelations.Any())
                {
                    await _unitOfWork.ExternalSystemPurposes.BulkDeleteAsync(existingRelations);
                }

                var newRelations = validPurposeIds
                    .Select(pid => new ExternalSystemPurpose
                    {
                        ExternalSystemId = systemId,
                        PurposeId = pid
                    })
                    .ToList();

                if (newRelations.Any())
                {
                    await _unitOfWork.ExternalSystemPurposes.BulkAddAsync(newRelations);
                }

                await _unitOfWork.SaveChangesAsync();

                return newRelations;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error while updating purposes for system {systemId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> BulkRemoveSystemPurposeAsync(Guid systemId, List<Guid> purposeIds)
        {
            var relations = await _unitOfWork.ExternalSystemPurposes
                 .FindAsync(esp => esp.ExternalSystemId == systemId && purposeIds.Contains(esp.PurposeId));

            if (!relations.Any()) return false;

            await _unitOfWork.ExternalSystemPurposes.BulkDeleteAsync(relations);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Purpose>> GetSystemPurposesAsync(Guid systemId)
        {
            var externalSystemPurposes = await _unitOfWork.ExternalSystemPurposes
                .FindAsync(esp => esp.ExternalSystemId == systemId);

            var purposeIds = externalSystemPurposes.Select(esp => esp.PurposeId).ToList();

            if (!purposeIds.Any())
                return new List<Purpose>();

            var purposes = await _unitOfWork.Purposes.FindAsync(p => purposeIds.Contains(p.Id));

            return purposes;
        }
        public async Task<bool> RemoveSystemPurposeAsync(Guid systemId, Guid purposeId)
        {
            var relation = await _unitOfWork.ExternalSystemPurposes
                .FindAsync(e => e.ExternalSystemId == systemId && e.PurposeId == purposeId);

            if (!relation.Any()) return false;

            await _unitOfWork.ExternalSystemPurposes.DeleteAsync(relation.FirstOrDefault().Id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ActionResult<List<SystemUserVM>>> GetAllUsersAsync(Guid systemId)
        {
            var system = await _unitOfWork.ExternalSystems.GetByIdAsync(systemId);
            if (system == null) throw new ArgumentException("External System not found.");

            var users = await _externalSystemRepository.GetUsersAsync(systemId);
            var userVMs = _mapper.Map<List<SystemUserVM>>(users);

            return userVMs;
        }

        public async Task<ExternalSystemDetailVM> GetExternalSystemDetailAsync(Guid systemId)
        {
            var externalSystem = await _externalSystemRepository.GetDetailAsync(systemId,
                es => es.Groups,
                es => es.Purposes,
                es => es.CreatedBy,
                x => x.LastModifiedBy
                );

            var users = await _externalSystemRepository.GetUsersAsync(systemId);

            var purposeIds = externalSystem.Purposes.Select(p => p.PurposeId).ToList();

            var purposes = await _unitOfWork.Purposes.FindAsync(p => purposeIds.Contains(p.Id));

            var userVMs = _mapper.Map<List<SystemUserVM>>(users);


            var purposeVMs = _mapper.Map<List<PurposeVM>>(purposes);

            var externalSystemDetailVM = _mapper.Map<ExternalSystemDetailVM>(externalSystem);
            externalSystemDetailVM.Users = userVMs;
            externalSystemDetailVM.Purposes = purposeVMs;

            externalSystemDetailVM.CreatedBy = externalSystem.CreatedBy?.FullName;
            externalSystemDetailVM.LastModifiedBy = externalSystem.LastModifiedBy?.FullName;
            return externalSystemDetailVM;
        }

        public async Task UpdateSystemStatus(SystemStatusVM model, List<ExternalSystemStatus> allowedStatus)
        {
            var system = await _externalSystemRepository.GetByIdAsync(model.SystemId);
            if (system == null) throw new ArgumentException("External System not found.");

            if (!allowedStatus.Contains(model.Status))
                throw new InvalidOperationException("Invalid Status.");

            // validate if we Activate an external system
            // if(model.Status == ExternalSystemStatus.Active)
            // {
                // only allow active an external system if there is at least 1 approved DPIA for that system
            // }

            system.Status = model.Status;
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RemoveExternalSystem(Guid systemId)
        {
            var system = await _externalSystemRepository.GetByIdAsync(systemId);
            if (system == null) throw new ArgumentException("External System not found.");

            var allowedStatus = new List<ExternalSystemStatus>
            {
                ExternalSystemStatus.WaitingForFIC
            };

            if (!allowedStatus.Contains(system.Status))
                throw new InvalidOperationException("Cannot remove External System with current status: " + system.Status);

            // Remove all purposes associated with the system
            var purposes = await _unitOfWork.ExternalSystemPurposes.FindAsync(esp => esp.ExternalSystemId == systemId);
            if (purposes.Any())
            {
                await _unitOfWork.ExternalSystemPurposes.BulkDeleteAsync(purposes);
            }

            // Remove all groups associated with the system
            var groups = await _unitOfWork.Groups.FindAsync(g => g.SystemId == systemId);

            foreach (var group in groups)
            {
                await _unitOfWork.Groups.DeleteAsync(group.Id);
            }

            await _unitOfWork.ExternalSystems.DeleteAsync(systemId);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task UpdateSystemUsers(Guid systemId, List<GroupUserVM> model)
        {
            var system = await _unitOfWork.ExternalSystems.GetDetailAsync(systemId, es => es.Groups);
            if (system == null) throw new ArgumentException("External System not found.");

            var groupIds = model.Select(g => g.GroupId).ToList();
            var systemGroups = system.Groups;

            foreach (var groupId in groupIds)
            {
                if (!systemGroups.Any(g => g.Id == groupId))
                    throw new ArgumentException("Invalid Group ID.");
            }

            foreach (var groupUser in model)
            {
                var group = await _groupRepository.GetDetailAsync(groupUser.GroupId, g => g.Users);
                if (group == null) throw new ArgumentException($"Group {groupUser.GroupId} not found.");
                var currentUsers = group.Users!.Select(u => u.Id).ToList();
                var newUsers = groupUser.UserIds.Except(currentUsers).ToList();
                var removedUsers = currentUsers.Except(groupUser.UserIds).ToList();
                List<User> userList = (await _userService.FindAsync(u => newUsers.Contains(u.Id))).ToList();

                if (newUsers.Any())
                {
                    int result = await _groupRepository.AddUserToGroup(groupUser.GroupId, newUsers);
                    if (result > 0)
                    {
                        foreach (var user in userList)
                        {
                            var notification = new UserAddedToSystemNotification
                            {
                                Email = user.Email,
                                FullName = user.FullName,
                                RoleName = group.Name,
                                SystemName = system.Name,
                            };

                            // raise event
                            await _mediator.Publish(notification);
                        }
                    }
                }

                if (removedUsers.Any())
                {
                    await _groupRepository.DeleteUserFromGroup(groupUser.GroupId, removedUsers);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ValidateApiKeyAsync(string apikey)
        {
            // Compute hash for the provided API key (using the same method used when storing)
            var hashedProvidedKey = ConsentUtils.HashApiKey(apikey);

            var systems = await FindAsync(x => x.ApiKeyHash == hashedProvidedKey && x.Status == ExternalSystemStatus.Active);

            return systems.Any();
        }

        public async Task UpdateSystem(UpdateSystemVM model, Guid systemId)
        {
            var system = await _externalSystemRepository.GetByIdAsync(systemId);
            if (system == null) throw new ArgumentException("External System not found.");

            _mapper.Map(model, system);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
