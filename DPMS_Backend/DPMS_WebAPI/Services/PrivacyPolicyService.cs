using AutoMapper;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Services
{
    public class PrivacyPolicyService : BaseService<PrivacyPolicy>, IPrivacyPolicyService
    {
        private readonly IPrivacyPolicyRepository _privacyPolicyRepository;
        private readonly IMapper _mapper;

        public PrivacyPolicyService(IUnitOfWork unitOfWork, IPrivacyPolicyRepository privacyPolicyRepository, IMapper mapper) : base(unitOfWork)
        {
            _privacyPolicyRepository = privacyPolicyRepository ?? throw new ArgumentNullException(nameof(privacyPolicyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override IRepository<PrivacyPolicy> Repository => _privacyPolicyRepository;

        public async Task ActivePolicy(Guid id)
        {
           await _privacyPolicyRepository.ActivePolicy(id);
           await _unitOfWork.SaveChangesAsync();
        }
    }
}
