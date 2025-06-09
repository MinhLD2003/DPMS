using DPMS_WebAPI.Builders;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Repositories;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.Tests.IntegrationTests.Mock;
using MediatR;
using Moq;

namespace DPMS_WebAPI.Tests.IntegrationTests
{
    public class ServiceTestEnvironment : TestEnvironment
    {
        protected readonly IUserRepository _userRepository;
        protected readonly IGroupRepository _groupRepository;
        protected readonly IFeatureRepository _featureRepository;
        protected readonly IUnitOfWork _unitOfWork;
        // protected readonly IGroupService _groupService;
        // protected readonly ILogger<GroupService> _logger;
        protected readonly IEventMessageBuilder _messageBuilder;
        protected readonly Mock<IMediator> _mediatorMock;
        protected readonly IDPIARepository _dpiaRepository;
        protected readonly IFileRepository _fileRepository; 
        protected readonly IMemberResponsibilityRepository _mRes;
        protected readonly IDPIAMemberRepository _dpiaMember;
        protected readonly IExternalSystemService _externalSystemService;
        protected readonly IExternalSystemPurposeRepository _externalSystemPurposeRepository;
        protected readonly IExternalSystemRepository _externalSystemRepository;
        protected readonly IUserService _userService;
        protected readonly IIssueTicketRepository _issueTicketRepository;
        protected readonly IRiskService _riskService;
        protected readonly IRiskRepository _riskRepository;
        public ServiceTestEnvironment()
        {
            // Get the actual repository instances with the in-memory DB context
            _userRepository = new UserRepository(_context);
            _groupRepository = new GroupRepository(_context);
            _featureRepository = new FeatureRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
            _issueTicketRepository = new IssueTicketRepository(_context);
            // _logger = new LoggerFactory().CreateLogger<GroupService>();
            _messageBuilder = new DPIAEventMessageBuilder();
            _mediatorMock = new Mock<IMediator>();
            _dpiaRepository = new DPIARepository(_context); 
            _fileRepository = new MockFileRepository();
            _mRes = new MemberResponsibilityRepository(_context);
            _dpiaMember = new DPIAMemberRepository(_context);
            _externalSystemService = new ExternalSystemService(_unitOfWork, _externalSystemPurposeRepository, _externalSystemRepository, _mapper, _groupRepository, _mediatorMock.Object, _userService);
            // Initialize services with real repositories
            // _groupService = new GroupService(_unitOfWork, _groupRepository, _featureRepository, _mapper, _logger);
            _riskRepository = new RiskRepository(_context);
            _riskService = new RiskService(_unitOfWork, _riskRepository, _mapper);
        }
    }
}
