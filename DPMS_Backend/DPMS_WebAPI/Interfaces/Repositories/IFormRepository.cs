using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Form;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IFormRepository : IRepository<Form>
    {
        public Task<List<ExportFICSubmissionVM>> GetFicExportData(Guid submissionId);
        /// <summary>
        /// TODO: Apply result pattern
        /// </summary>
        /// <param name="submissionId"></param>
        /// <returns></returns>
        public Task<Submission> GetSubmissionDetailsAsync(Guid submissionId);
    }
}
