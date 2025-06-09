using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class PrivacyPolicyRepository : BaseRepository<PrivacyPolicy>, IPrivacyPolicyRepository
    {
        public PrivacyPolicyRepository(DPMSContext context) : base(context)
        {
        }

        public async Task ActivePolicy(Guid id)
        {
            var activePolicy = await _context.PrivacyPolicies.FirstOrDefaultAsync(x => x.Id == id);
            activePolicy.Status = PolicyStatus.Active;
            var policies = await _context.PrivacyPolicies.Where(policy => policy.Status == PolicyStatus.Active).ToListAsync();
            foreach (var p in policies)
            {
                p.Status = PolicyStatus.Inactive;
            }
            policies.Add(activePolicy);
            _context.PrivacyPolicies.UpdateRange(policies);
        }
    }
}
