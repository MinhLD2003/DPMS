using AutoMapper;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.DPIA;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DPMS_WebAPI.Repositories
{
    public class DPIARepository : BaseRepository<DPIA>, IDPIARepository
    {
        public DPIARepository(DPMSContext context) : base(context)
        {
        }

        public Task AddMemberAsync(DPIAMember member)
        {
            throw new NotImplementedException();
        }

        public async Task BulkAddMembersAsync(List<DPIAMember> newMembers)
        {
            try
            {
                await _context.DPIAMembers.AddRangeAsync(newMembers);
            }
            catch (Exception ex)
            {
                throw new Exception($"Bulk insert failed: {ex.Message}");
            }
        }

        public async Task BulkAddResponsibilitiesAsync(List<DPIAResponsibility> newResponsibilities)
        {
            try
            {
                await _context.DPIAResponsibilities.AddRangeAsync(newResponsibilities);
            }
            catch (Exception ex)
            {
                throw new Exception($"Bulk insert failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Comment>> GetCommentsAsync(Guid id)
        {
            var comments = await _context.Comments
                .Where(c => c.ReferenceId == id)
                .Include(c => c.User)
                .ToListAsync();

            return comments;
        }

        public async Task<IEnumerable<DPIAMember>> GetDPIAMembersAsync(Guid id)
        {
            var members = await _context.DPIAMembers
                .Where(m => m.DPIAId == id)
                .Include(m => m.User)
                .Include(m => m.Responsibilities)
                .ThenInclude(r => r.DPIAResponsibility)
                .ThenInclude(dr => dr.Responsibility)
                .ToListAsync();

            return members;
        }

        public async Task<IEnumerable<DPIAResponsibility>> GetDPIAResponsibilitiesAsync(Guid id)
        {
            var responsibilities = await _context.DPIAResponsibilities
                .Where(r => r.DPIAId == id)
                .Include(r => r.Responsibility)
                .ToListAsync();

            return responsibilities;
        }

        public async Task<DPIA?> GetDPIADetailAsync(Guid dpiaId)
        {
            return await _context.DPIAs
                .Include(d => d.ExternalSystem)
                .Include(d => d.Documents)
                .ThenInclude(docs => docs.CreatedBy)
                .FirstOrDefaultAsync(d => d.Id == dpiaId);
        }

        public async Task<List<DPIAEvent>> GetEventsAsync(Guid id)
        {
            return await _context.DPIAEvents
                .Where(e => e.DPIAId == id)
                .Include(e => e.User)
                .ToListAsync();
        }

        public async Task SaveCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            // return _context.SaveChangesAsync();
        }

        public async Task SaveEventsAsync(DPIAEvent dpiaEvent)
        {
            await _context.DPIAEvents.AddAsync(dpiaEvent);
            // return _context.SaveChangesAsync();
        }

        public async Task UploadDocumentAsync(Guid dpiaId, DPIADocument document)
        {
            document.DPIAId = dpiaId;
            document.ResponsibleId = null;
            _context.DPIADocuments.Add(document);
            await _context.SaveChangesAsync();
        }

        // Override GetPagedAsync to include related entities
        public override async Task<PagedResponse<DPIA>> GetPagedAsync(QueryParams queryParams, params Expression<Func<DPIA, object>>[]? includes)
        {
            IQueryable<DPIA> query = _dbSet;

            // Apply includes for related entities if provided
            if (includes != null && includes.Any())
            {
                query = query.ApplyIncludes(includes);
            }

            // Apply filtering
            // Special handling for "Ids" filter
            if (queryParams.Filters.TryGetValue("Ids", out var idsValue) && !string.IsNullOrEmpty(idsValue))
            {
                var ids = idsValue.Split(',').Select(id => Guid.Parse(id.Trim())).ToList();
                query = query.Where(dpia => ids.Contains(dpia.Id));
            }

            query = query.ApplyFiltering(queryParams.Filters);

            // Apply sorting
            query = query.ApplySorting(queryParams.SortBy, queryParams.SortDirection);

            return query.ToPagedResponse(queryParams.PageNumber, queryParams.PageSize);
        }

        // public virtual async Task<PagedResponse<T>> GetPagedAsync(QueryParams queryParams, params Expression<Func<T, object>>[]?includes)
        // {
        //     IQueryable<T> query = _dbSet;

        //     // Apply includes for related entities if provided
        //     if (includes != null && includes.Any())
        //     {
        //         query = query.ApplyIncludes(includes);
        //     }

        //     // Apply filtering
        //     query = query.ApplyFiltering(queryParams.Filters);

        //     // Apply sorting
        //     query = query.ApplySorting(queryParams.SortBy, queryParams.SortDirection);

        //     // Apply pagination and return results
        //     return query.ToPagedResponse(queryParams.PageNumber, queryParams.PageSize);
        // }

    }
}