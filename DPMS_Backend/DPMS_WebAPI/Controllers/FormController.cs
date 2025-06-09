using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Form;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for Form-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly DPMSContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<FormController> _logger;
        private readonly IFormService _formService;
        private readonly IMediator _mediator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        /// <param name="formService"></param>
        public FormController(DPMSContext context, IMapper mapper, ILogger<FormController> logger, IFormService formService, IMediator mediator)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _formService = formService;
            _mediator = mediator;
        }

        /// <summary>
        /// Get all form templates
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-templates")]
        public ActionResult<List<Form>> GetFormTemplates(int? formStatus)
        {
            if (formStatus == null)
                return _context.Forms.ToList();

            return _context.Forms.Where(f => f.Status == (FormStatus)formStatus).ToList();
        }

        [HttpPut("update-status/{id:guid}")]
        public ActionResult UpdateStatus(Guid id, int formStatus)
        {
            Form? form = _context.Forms.FirstOrDefault(f => f.Id == id);
            if (form == null)
            {
                return NotFound();
            }

            if (Enum.IsDefined(typeof(FormStatus), formStatus))
            {
                form.Status = (FormStatus)formStatus;
                _context.SaveChanges();
                return Ok();
            }

            return BadRequest("Invalid form status");
        }

        /// <summary>
        /// Get form details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:Guid}")]
        public ActionResult<FormVM> GetFormDetails(Guid id)
        {
            var form = _context.Forms
                .Where(f => f.Id == id)
                .Include(f => f.FormElements) // Load all elements in one query
                                              //.AsNoTracking()
                .FirstOrDefault();

            if (form == null)
            {
                return BadRequest("Form not found");
            }

            List<FormElement> elements = form.FormElements!.ToList();
            form.FormElements!.Clear();

            form.FormElements = elements.Where(e => e.ParentId == null).ToList();

            FormVM result = _mapper.Map<FormVM>(form);
            return result;
        }

        /// <summary>
        /// Save form
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost("save")]
        public ActionResult<CreateFormVm> SaveForm(CreateFormVm form)
        {
            var existing = _context.Forms
                .Where(f => f.Name == form.Name && f.FormType == form.FormType)
                .FirstOrDefault();

            if (existing != null)
            {
                return BadRequest("Form already exists");
            }

            Form formEntity = _mapper.Map<Form>(form);

            if (form.FormElements != null)
            {
                foreach (var element in formEntity.FormElements!)
                {
                    AssignFormAndParentIds(element, formEntity.Id, null);
                }
            }

            formEntity.Version = 1;
            formEntity.Status = FormStatus.Draft;

            _context.Forms.Add(formEntity);
            _context.SaveChanges();

            return form;
        }

        /// <summary>
        /// Only Edit forms with Draft Status (Lastest version), and this will not create a new version of the form
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        public async Task<ActionResult> EditForm(CreateFormVm form)
        {
            var existings = await _context.Forms
                .Include(f => f.FormElements)
                .Where(f => f.Name == form.Name && f.FormType == form.FormType)
                .ToArrayAsync();

            var lastest = existings.OrderByDescending(f => f.Version).FirstOrDefault();

            if (lastest == null)
            {
                return BadRequest("Form not found");
            }

            if (lastest.Status != FormStatus.Draft)
            {
                return BadRequest("Form is not in Draft status");
            }


            _context.FormElements.RemoveRange(lastest.FormElements!); // Remove all elements
            await _context.SaveChangesAsync();

            var formEntity = _mapper.Map<Form>(form);

            if (form.FormElements != null)
            {
                foreach (var element in formEntity.FormElements!)
                {
                    AssignFormAndParentIds(element, lastest.Id, null);
                    _context.FormElements.Add(element);
                }
            }
            _context.Entry(lastest).State = EntityState.Modified;
            _context.Update(lastest);
            await _context.SaveChangesAsync();
            return Ok("Form updated successfully");
        }

        /// <summary>
        /// Update form, create a new version of the form
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost("update")]
        public ActionResult<CreateFormVm> UpdateForm(CreateFormVm form)
        {
            var existing_num = _context.Forms
                .Where(f => f.Name == form.Name && f.FormType == form.FormType)
                .Count();

            if (existing_num == 0)
            {
                return BadRequest("Form not found");
            }

            Form formEntity = _mapper.Map<Form>(form);

            if (form.FormElements != null)
            {
                foreach (var element in formEntity.FormElements!)
                {
                    AssignFormAndParentIds(element, formEntity.Id, null);
                }
            }

            formEntity.Version = existing_num + 1;
            formEntity.Status = FormStatus.Draft;

            _context.Forms.Add(formEntity);
            _context.SaveChanges();

            return form;
        }

        private void AssignFormAndParentIds(FormElement element, Guid formId, Guid? parentId)
        {
            element.FormId = formId; // Set FormId for the element
            element.ParentId = parentId; // Set ParentId (if applicable)

            if (element.Children != null && element.Children.Any())
            {
                foreach (var child in element.Children)
                {
                    AssignFormAndParentIds(child, formId, element.Id); // Recursively process children

                }
            }
        }

        /// <summary>
        /// Submit form
        /// </summary>
        /// <param name="submission"></param>
        /// <returns></returns>
        [HttpPost("submit")]
        public async Task<ActionResult<FormSubmissionVM>> SubmitForm(FormSubmissionVM submission)
        {
            Form? form = _context.Forms
                .Where(f => f.Id == submission.FormId)
                .Include(f => f.FormElements)
                .FirstOrDefault();

            if (form == null)
            {
                return BadRequest("Form not found");
            }

            var system = _context.ExternalSystems.FirstOrDefault(s => s.Id == submission.SystemId);
            if (system == null)
            {
                return BadRequest("System not found");
            }

            // Validate submission
            foreach (var response in submission.Responses)
            {
                FormElement? element = form.FormElements?.FirstOrDefault(e => e.Id == response.FormElementId);

                if (element == null)
                {
                    return BadRequest("Invalid form element");
                }
            }

            var submissionEntity = new Submission
            {
                FormId = submission.FormId,
                SystemId = submission.SystemId,
                SubmissionElements = submission.Responses.Select(r => new FormResponse
                {
                    FormElementId = r.FormElementId,
                    Value = r.Value
                }).ToList()
            };

            _context.Submissions.Add(submissionEntity);
            _context.SaveChanges();

            FICSubmittedEvent submitEvent = new FICSubmittedEvent
            {
                SystemId = system.Id,
                SubmissionId = submissionEntity.Id
            };
            await _mediator.Publish(new FICSubmittedNotification(submitEvent));

            return submission;
        }

        /// <summary>
        /// Get all FIC submissions (for an external system)
        /// </summary>
        /// <param name="systemId">External System Id</param>
        /// <returns></returns>
        [HttpGet("get-submissions")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public ActionResult<List<FICSubmissionVM>> GetFICSubmissions(Guid systemId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Submission> submissions;

            submissions = _context.Submissions.Include(s => s.Form).Include(s => s.CreatedBy).Include(s => s.ExternalSystem).Where(s => s.SystemId == systemId).ToList();

            List<FICSubmissionVM> result = _mapper.Map<List<FICSubmissionVM>>(submissions);
            return result;
        }

        [HttpGet("get-all-submissions")]
        [Authorize(Policy = Policies.FeatureRequired)]
        public ActionResult<List<FICSubmissionVM>> GetAllSubmissions()
        {
            List<Submission> submissions = new List<Submission>();

            submissions = _context.Submissions.Include(s => s.Form).Include(s => s.CreatedBy).Include(s => s.ExternalSystem).ToList();

            List<FICSubmissionVM> result = _mapper.Map<List<FICSubmissionVM>>(submissions);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">submission id</param>
        /// <returns></returns>
        [HttpGet("submission/{id:guid}")]
        public ActionResult<FormVM> GetSubmissionDetails(Guid id)
        {
            try
            {
                var submission = _context.Submissions
                    .Where(s => s.Id == id)
                    .Include(s => s.Form)
                    .FirstOrDefault();

                if (submission == null)
                {
                    return NotFound("Submission not found");
                }
                var formId = submission.FormId;
                List<FormResponse> responses = _context.FormResponses.Where(fr => fr.SubmissionId == id).ToList();

                // loading form structure
                var form = _context.Forms.Where(f => f.Id == formId).Include(f => f.FormElements).FirstOrDefault();

                if (form == null)
                {
                    return NotFound("Form not found");
                }

                List<FormElement> elements = form.FormElements!.ToList();
                form.FormElements!.Clear();
                form.FormElements = elements.Where(e => e.ParentId == null).ToList();
                FormVM result = _mapper.Map<FormVM>(form);

                AssignValues(result.FormElements!, responses);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Recursively assigns values from FormResponses to FormElementVMs
        /// </summary>
        private void AssignValues(ICollection<FormElementVM> formElements, List<FormResponse> responses)
        {
            if (formElements == null || formElements.Count == 0)
                return;

            foreach (var element in formElements)
            {
                element.Value = responses.FirstOrDefault(r => r.FormElementId == element.Id)?.Value;

                // Recursively assign values to child elements
                if (element.Children != null && element.Children.Count > 0)
                {
                    AssignValues(element.Children, responses);
                }
            }
        }

        /// <summary>
        /// Delete form
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteForm(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool deleteSuccess = await _formService.DeleteAsync(id);
            if (deleteSuccess)
            {
                _logger.LogInformation("Delete form id {id} successfully", id);
                return Ok();
            }
            else
            {
                _logger.LogInformation("Delete form id {id} failed", id);
                return BadRequest("Delete failed, something wrong happened");
            }
        }

        /// <summary>
        /// Export FIC submission to Excel
        /// </summary>
        /// <param name="submissionId"></param>
        /// <returns></returns>
        [HttpGet("export-submission/{submissionId}")]
        public async Task<ActionResult<List<ExportFICSubmissionVM>>> ExportSubmission(Guid submissionId)
        {
            try
            {
                Stream exportData = await _formService.ExportFicSubmission(submissionId);
                if (exportData == null)
                {
                    return NotFound();
                }

                return File(exportData, "application/octet-stream", $"FIC_Submission_{DateTime.Now}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(ex.Message);
            }
        }
    }
}
