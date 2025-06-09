using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Form;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IFormService : IService<Form>
    {
        /// <summary>
        /// TODO: Check for invalid submissionId
        /// </summary>
        /// <param name="submissionId"></param>
        /// <returns></returns>
        public Task<Stream> ExportFicSubmission(Guid submissionId);
    }
}
