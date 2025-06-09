using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Consent;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class ConsentRepository : BaseRepository<Consent>, IConsentRepository
    {
        public ConsentRepository(DPMSContext context) : base(context)
        {

        }

        public async Task<Consent?> GetConsentByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            return await _context.Consents.Include(c => c.Purposes)
                .OrderByDescending(d => d.ConsentDate)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<PagedResponse<Consent>> GetConsentsWithPurposeAsync(QueryParams queryParams)
        {
            IQueryable<Consent> query = _context.Consents
                .Include(c => c.Purposes)
                    .ThenInclude(cp => cp.Purpose)
                .Include(c => c.ExternalSystem);
            // Apply filtering
            query = query.ApplyFiltering(queryParams.Filters);
            // Apply sorting
            query = query.ApplySorting(queryParams.SortBy, queryParams.SortDirection);
            return query.ToPagedResponse(queryParams.PageNumber, queryParams.PageSize);
        }

        /// <summary>
        /// override default impl, including ExternalSystem prop
        /// </summary>
        /// <returns></returns>
        public override async Task<IEnumerable<Consent>> GetAllAsync()
        {
            return await _context.Consents.Include(c => c.ExternalSystem).ToListAsync();
        }
    }
}
