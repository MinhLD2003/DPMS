using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Services
{
    public class ConsentPurposeService : BaseService<ConsentPurpose>, IConsentPurposeService
    {
        public ConsentPurposeService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        protected override IRepository<ConsentPurpose> Repository => _unitOfWork.ConsentPurposes;
    }
}
