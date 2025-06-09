using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class DPIAResponsibilityRepository : BaseRepository<DPIAResponsibility>, IDPIAResponsibilityRepository
    {

        public DPIAResponsibilityRepository(DPMSContext context) : base(context)
        {
        }


        public async Task<IEnumerable<MemberResponsibility>> GetMembersAsync(Guid id)
        {
            var memberResponsibilities = await _context.MemberResponsibilities
                .Where(mr => mr.DPIAResponsibilityId == id)
                .Include(mr => mr.Member)
                    .ThenInclude(m => m.User)
                .ToListAsync();
            return memberResponsibilities;
        }   


        public async Task<IEnumerable<DSAR>> GetOverDue()
        {
            return await _context.DSARs
                .Include(d => d.ExternalSystem)
                .Where(d => d.Status == DSARStatus.Submitted &&
                                d.RequiredResponse >= DateTime.UtcNow.AddDays(-1)).ToListAsync();
        }

        public async Task UploadDocumentAsync(Guid dpiaId, Guid responsibilityId, DPIADocument document)
        {
            document.DPIAId = dpiaId;
            document.ResponsibleId = responsibilityId;
            _context.DPIADocuments.Add(document);
            await _context.SaveChangesAsync();
        }
    }
}
