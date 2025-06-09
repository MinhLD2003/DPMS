using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class DPIAMemberRepository : BaseRepository<DPIAMember>, IDPIAMemberRepository
    {
        //private readonly DPMSContext _context;

        public DPIAMemberRepository(DPMSContext context) : base(context)
        {
            //_context = context;
        }

        public async Task UpdateAsync(List<DPIAMember> updatedMembers)
        {
            foreach (var updatedMember in updatedMembers)
            {
                var existingMember = await _context.DPIAMembers
                    .Include(m => m.Responsibilities)
                    .FirstOrDefaultAsync(m => m.Id == updatedMember.Id);
                if (existingMember == null)
                {
                    throw new Exception("Member not found");
                }

                var currentResponsibilities = existingMember.Responsibilities.ToList();

                var newResponsibilities = updatedMember.Responsibilities
                    .Where(r => currentResponsibilities.All(cr => cr.DPIAResponsibilityId != r.DPIAResponsibilityId))
                    .ToList();

                var removedResponsibilities = currentResponsibilities
                    .Where(cr => updatedMember.Responsibilities.All(r => r.DPIAResponsibilityId != cr.DPIAResponsibilityId))
                    .ToList();

                foreach (var responsibility in newResponsibilities)
                {
                    existingMember.Responsibilities.Add(responsibility);
                    _context.Entry(responsibility).State = EntityState.Added;
                }

                foreach (var responsibility in removedResponsibilities)
                {
                    existingMember.Responsibilities.Remove(responsibility);
                    _context.Entry(responsibility).State = EntityState.Deleted;
                }

                _context.Entry(existingMember).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Update failed: {ex.Message}");
            }
        }

        public async Task<List<string>> GetDpiaMemberEmail(Guid dpiaId)
        {
            return await _context.DPIAMembers
                .Include(dm => dm.User)
                .Where(dm => dm.DPIAId == dpiaId)
                .Select(dm => dm.User.Email)
                .ToListAsync();
        }
    }
}
