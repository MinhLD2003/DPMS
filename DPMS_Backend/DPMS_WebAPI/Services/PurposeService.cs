using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Services
{
    public class PurposeService : BaseService<Purpose>, IPurposeService
    {
        private readonly IPurposeRepository _purposeRepository;
        private readonly IMapper _mapper;

        public PurposeService(IUnitOfWork unitOfWork, IPurposeRepository purposeRepository, IMapper mapper) : base(unitOfWork)
        {
            _purposeRepository = purposeRepository ?? throw new ArgumentNullException(nameof(purposeRepository)); 
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override IRepository<Purpose> Repository => _purposeRepository;
    }
}
