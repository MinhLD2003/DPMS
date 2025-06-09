using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Risk;
using FlexCel.Report;

namespace DPMS_WebAPI.Services
{
    public class RiskService : BaseService<Risk>, IRiskService
    {
        private readonly IRiskRepository _riskRepository;
        private readonly IMapper _mapper;

        private const string EXPORT_TEMPLATE_PATH = "ExcelTemplates/RiskExportTemplate.xlsx";

        public RiskService(IUnitOfWork unitOfWork, IRiskRepository riskRepository, IMapper mapper) : base(unitOfWork)
        {
            _riskRepository = riskRepository ?? throw new ArgumentNullException(nameof(riskRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override IRepository<Risk> Repository => _riskRepository;

        public async Task<Stream> ExportAsync()
        {
            Stream resultStream = new MemoryStream();
            byte[] templateBytes = null;
            templateBytes = await File.ReadAllBytesAsync(EXPORT_TEMPLATE_PATH);
            Stream templateStream = new MemoryStream(templateBytes);

            // data
            List<Risk> data = await _riskRepository.GetExportData();

            // mapping
            List<ExportRiskVM> model = _mapper.Map<List<ExportRiskVM>>(data);

            FlexCelReport flexCel = new FlexCelReport();

            flexCel.AddTable("Risk", model);

            flexCel.Run(templateStream, resultStream);

            await templateStream.DisposeAsync();
            resultStream.Position = 0;
            flexCel.Dispose();

            return resultStream;
        }
    }
}
