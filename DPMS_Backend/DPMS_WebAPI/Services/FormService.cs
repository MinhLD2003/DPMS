using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Form;
using FlexCel.Report;

namespace DPMS_WebAPI.Services
{
    public class FormService : BaseService<Form>, IFormService
    {
        private readonly IFormRepository _formRepository;

        private const string FIC_EXPORT_TEMPLATE_PATH = "ExcelTemplates/ExportFicTemplate.xlsx";

        public FormService(IUnitOfWork unitOfWork, IFormRepository formRepository) : base(unitOfWork)
        {
            _formRepository = formRepository;
        }

        protected override IRepository<Form> Repository => _unitOfWork.Forms;

        public async Task<Stream> ExportFicSubmission(Guid submissionId)
        {
            Stream resultStream = new MemoryStream();
            byte[] templateBytes = null;
            templateBytes = await File.ReadAllBytesAsync(FIC_EXPORT_TEMPLATE_PATH);
            Stream templateStream = new MemoryStream(templateBytes);

            List<ExportFICSubmissionVM> data = await _formRepository.GetFicExportData(submissionId);
            Submission submission = await _formRepository.GetSubmissionDetailsAsync(submissionId);

            FlexCelReport report = new FlexCelReport();

            report.AddTable("Data", data);

            report.SetValue("CurrentTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            report.SetValue("Time", submission.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"));
            report.SetValue("Department", "DPMS");
            report.SetValue("CreatedBy", submission.CreatedBy?.UserName);
            report.SetValue("FormName", submission.Form?.Name);
            report.SetValue("SystemName", submission.ExternalSystem?.Name);

            report.Run(templateStream, resultStream);
            await templateStream.DisposeAsync();
            resultStream.Position = 0;
            report.Dispose();

            return resultStream;
        }
    }
}
