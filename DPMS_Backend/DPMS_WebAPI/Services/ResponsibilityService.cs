using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Repositories;

namespace DPMS_WebAPI.Services
{
    public class ResponsibilityService : BaseService<Responsibility>, IResponsibilityService
    {
        private readonly IResponsibilityRepository _responsibilityRepository;
        private readonly IMapper _mapper;

        public ResponsibilityService(IUnitOfWork unitOfWork, IResponsibilityRepository responsibility, IMapper mapper) : base(unitOfWork)
        {
            _responsibilityRepository = responsibility ?? throw new ArgumentNullException(nameof(responsibility)); 
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override IRepository<Responsibility> Repository => _responsibilityRepository;
    }
}
