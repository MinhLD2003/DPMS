using AutoMapper;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Tests.IntegrationTests;
using DPMS_WebAPI.ViewModels.Form;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DPMS_WebAPI.Tests.Controllers
{
    public class FormControllerTests : TestEnvironment
    {
        private DPMSContext GetInMemoryDbContext()
        {
            return _context;
        }

        private void SeedForms(DPMSContext context)
        {
            context.Forms.AddRange(new List<Form>
        {
            new Form {  Id = Guid.NewGuid(), Name = "Form A", Status = FormStatus.Draft },
            new Form { Id =  Guid.NewGuid(), Name = "Form B", Status = FormStatus.Activated },
            new Form { Id = Guid.NewGuid(), Name = "Form C", Status = FormStatus.Draft }
        });
            context.SaveChanges();
        }
        public static List<FormElement> GetMockFormElements()
        {
            return new List<FormElement>
        {
            new FormElement
            {
                Id = Guid.NewGuid(),
                FormId = Guid.NewGuid(),
                Name = "Root Element 1",
                DataType = FormElementTypes.Text,
                OrderIndex = 1,
            }
        };
        }
        #region GetTemplate Tests
        [Fact]
        public void GetFormTemplates_ReturnsAllForms_WhenFormStatusIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedForms(context);

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetFormTemplates(null);

            // Assert
            var list = Assert.IsType<List<Form>>(result.Value);
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void GetFormTemplates_ReturnsFilteredForms_WhenFormStatusIsProvided()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            SeedForms(context);

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetFormTemplates((int)FormStatus.Draft);

            // Assert
            var list = Assert.IsType<List<Form>>(result.Value);
            Assert.Equal(2, list.Count);
            Assert.All(list, f => Assert.Equal(FormStatus.Draft, f.Status));
        }
        [Fact]
        public void GetFormTemplates_ReturnsEmptyList_WhenNoFormsExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetFormTemplates(null);

            // Assert
            var list = Assert.IsType<List<Form>>(result.Value);
            Assert.Empty(list);
        }
#endregion

        #region UpdateFormStatus Tests
        [Fact]
        public void UpdateStatus_UpdatesFormStatus()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", Status = FormStatus.Draft };
            context.Forms.Add(form);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.UpdateStatus(form.Id, (int)FormStatus.Activated);

            // Assert
            var updatedForm = context.Forms.Find(form.Id);
            Assert.NotNull(updatedForm);
            Assert.Equal(FormStatus.Activated, updatedForm.Status);
        }
        #endregion

        #region AddForm Test
        [Fact]
        public void Addform_ReturnsForm()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", Status = FormStatus.Draft };
            context.Forms.Add(form);
            context.SaveChanges();


            // Act
            var addedForm = context.Forms.Find(form.Id);
            Assert.NotNull(addedForm);
            Assert.Equal(form.Name, addedForm.Name);
            Assert.Equal(form.Status, addedForm.Status);
        }
        #endregion

        //#region SaveForm Test(CreateFormVM) failed
        //[Fact]
        //public void SaveForm_ReturnsForm()
        //{

        //    // Arrange
        //    var context = GetInMemoryDbContext();
        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockMapper = new Mock<IMapper>();
        //    var mockService = new Mock<IFormService>();
        //    var mockMediator = new Mock<IMediator>();

        //    var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);
        //    //    var listElements = new List<CreateFormElementsVm>
        //    //{
        //    //    new CreateFormElementsVm
        //    //    {
        //    //        Name = "Root Element 1",
        //    //        DataType = FormElementTypes.Text,
        //    //        OrderIndex = 1,
        //    //    }
        //    //};
        //    //    var listElements = new List<FormElement>
        //    //{
        //    //    new FormElement
        //    //    {
        //    //        Id = Guid.NewGuid(),
        //    //        FormId = Guid.NewGuid(),
        //    //        Name = "Root Element 1",
        //    //        DataType = FormElementTypes.Text,
        //    //        OrderIndex = 1,
        //    //    }
        //    //};
        //    //    var form1 = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft, FormElements = listElements };
        //    //    context.Forms.Add(form1);
        //    //    context.SaveChanges();
        //    var form = new CreateFormVm { Name = "Test Form hihi", FormType = FormTypes.FIC };
        //    var updatedFormVm = new CreateFormVm { Name = "Test Form", FormType = FormTypes.FIC, FormElements = new List<CreateFormElementsVm> { new CreateFormElementsVm { Name = "Updated Element" } } };

        //    var result = controller.SaveForm(updatedFormVm);

        //    // Act
        //    var addedForm = context.Forms.Where(f => f.Name == form.Name).FirstOrDefault();
        //    Assert.NotNull(addedForm);
        //}
        //#endregion

        #region GetFormDetails without elements Test
        [Fact]
        public void GetFormDetails_ReturnsFormDetails_WhenIdExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", Status = FormStatus.Draft };
            context.Forms.Add(form);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<FormVM>(It.IsAny<Form>())).Returns(new FormVM { Id = form.Id, Name = form.Name, Status = form.Status });
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetFormDetails(form.Id);

            // Assert
            var formVm = Assert.IsType<FormVM>(result.Value);
            Assert.Equal(form.Id, formVm.Id);
            Assert.Equal(form.Name, formVm.Name);
            Assert.Equal(form.Status, formVm.Status);
        }
        #endregion

        #region EditForm Tests

        [Fact]
        public void EditForm_UpdatesForm_WhenFormIsInDraftStatus()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft, Version = 1 };
            context.Forms.Add(form);
            context.SaveChanges();

            var updatedFormVm = new CreateFormVm { Name = "", FormElements = new List<CreateFormElementsVm> { new CreateFormElementsVm { Name = "Updated Element" } } };
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<Form>(It.IsAny<CreateFormVm>())).Returns(new Form { Id = form.Id, Name = updatedFormVm.Name, FormType = updatedFormVm.FormType, FormElements = new List<FormElement> { new FormElement { Name = "Updated Element" } } });
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.EditForm(updatedFormVm).Result;

            // Assert
            var updatedForm = context.Forms.Include(f => f.FormElements).FirstOrDefault(f => f.Id == form.Id);
            Assert.NotNull(updatedForm);
            Assert.Equal("Updated Element", updatedForm.FormElements.First().Name);
        }
        [Fact]
        public void UpdateForm_CreatesNewVersionOfForm()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft, Version = 1 };
            context.Forms.Add(form);
            context.SaveChanges();

            var updatedFormVm = new CreateFormVm { Name = "Test Form", FormType = FormTypes.FIC, FormElements = new List<CreateFormElementsVm> { new CreateFormElementsVm { Name = "Updated Element" } } };
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<Form>(It.IsAny<CreateFormVm>())).Returns(new Form { Id = Guid.NewGuid(), Name = updatedFormVm.Name, FormType = updatedFormVm.FormType, FormElements = new List<FormElement> { new FormElement { Name = "Updated Element" } } });
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.UpdateForm(updatedFormVm);

            // Assert
            var updatedForm = context.Forms.FirstOrDefault(f => f.Name == "Test Form" && f.Version == 2);
            Assert.NotNull(updatedForm);
            Assert.Equal(2, updatedForm.Version);
        }
        #endregion

        #region Submit Tests
        [Fact]
        public void SubmitForm_AddsSubmissionToDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var sys = new ExternalSystem { Id = Guid.NewGuid(), Name = "Mock Sys" };
            var listElements = new List<FormElement>
        {
            new FormElement
            {
                Id = Guid.NewGuid(),
                FormId = Guid.NewGuid(),
                Name = "Root Element 1",
                DataType = FormElementTypes.Text,
                OrderIndex = 1,
            }
        };
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft, FormElements = listElements };
            context.Forms.Add(form);
            context.ExternalSystems.Add(sys);
            context.SaveChanges();

            var submissionVm = new FormSubmissionVM { FormId = form.Id, SystemId = sys.Id, Responses = new List<FormResponseVM> { new FormResponseVM { FormElementId = listElements.FirstOrDefault().Id, Value = "Mock Response" } } };
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.SubmitForm(submissionVm).Result;

            // Assert
            var submission = context.Submissions.FirstOrDefault(s => s.FormId == form.Id);
            Assert.NotNull(submission);
            Assert.Equal(submissionVm.FormId, submission.FormId);
        }
        #endregion

        #region Delete Form Tests
        [Fact]
        public void DeleteForm_RemovesFormFromDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var listElements = new List<FormElement>
        {
            new FormElement
            {
                Id = Guid.NewGuid(),
                FormId = Guid.NewGuid(),
                Name = "Root Element 1",
                DataType = FormElementTypes.Text,
                OrderIndex = 1,
            }
        };
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft, FormElements = listElements };
            context.Forms.Add(form);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            var mockService = new Mock<IFormService>();
            mockService.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.DeleteForm(form.Id).Result;

            // Assert
            var deletedForm = context.Forms.FirstOrDefault(f => f.Id == form.Id);
            Assert.IsType<OkResult>(result);
        }
        #endregion

        #region GetFICSubmissions Tests

        [Fact]
        public void GetFICSubmissions_ReturnsAllSubmissions_WhenSystemIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft };
            var submission1 = new Submission { Id = Guid.NewGuid(), FormId = form.Id };
            var submission2 = new Submission { Id = Guid.NewGuid(), FormId = form.Id };
            context.Forms.Add(form);
            context.Submissions.AddRange(submission1, submission2);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<FICSubmissionVM>>(It.IsAny<List<Submission>>()))
                      .Returns(new List<FICSubmissionVM>
                      {
                  new FICSubmissionVM { Id = submission1.Id, FormId = form.Id, Name = form.Name },
                  new FICSubmissionVM { Id = submission2.Id, FormId = form.Id, Name = form.Name }
                      });
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetFICSubmissions(null);

            // Assert
            var list = Assert.IsType<List<FICSubmissionVM>>(result.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void GetFICSubmissions_ReturnsFilteredSubmissions_WhenSystemIdIsProvided()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft };
            var system = new ExternalSystem { Id = Guid.NewGuid(), Name = "Test System" };
            var submission1 = new Submission { Id = Guid.NewGuid(), FormId = form.Id, SystemId = system.Id };
            var submission2 = new Submission { Id = Guid.NewGuid(), FormId = form.Id };
            context.Forms.Add(form);
            context.ExternalSystems.Add(system);
            context.Submissions.AddRange(submission1, submission2);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<FICSubmissionVM>>(It.IsAny<List<Submission>>()))
                      .Returns(new List<FICSubmissionVM>
                      {
                  new FICSubmissionVM { Id = submission1.Id, FormId = form.Id, Name = form.Name, SystemId = system.Id }
                      });
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetFICSubmissions(system.Id);

            // Assert
            var list = Assert.IsType<List<FICSubmissionVM>>(result.Value);
            Assert.Single(list);
            Assert.Equal(system.Id, list.First().SystemId);
        }
        [Fact]
        public void GetFICSubmissions_ReturnsEmptyList_WhenNoSubmissionsExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft };
            var system = new ExternalSystem { Id = Guid.NewGuid(), Name = "Test System" };
            context.Forms.Add(form);
            context.ExternalSystems.Add(system);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<FICSubmissionVM>>(It.IsAny<List<Submission>>()))
                      .Returns(new List<FICSubmissionVM>());
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetFICSubmissions(system.Id);

            // Assert
            var list = Assert.IsType<List<FICSubmissionVM>>(result.Value);
            Assert.Empty(list);
        }
        [Fact]
        public void GetFICSubmissions_ReturnsEmptyList_WhenSystemIdIsInvalid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft };
            var system = new ExternalSystem { Id = Guid.NewGuid(), Name = "Test System" };
            var invalidSystemId = Guid.NewGuid(); // This ID does not exist in the database
            context.Forms.Add(form);
            context.ExternalSystems.Add(system);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<FICSubmissionVM>>(It.IsAny<List<Submission>>()))
                      .Returns(new List<FICSubmissionVM>());
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetFICSubmissions(invalidSystemId);

            // Assert
            var list = Assert.IsType<List<FICSubmissionVM>>(result.Value);
            Assert.Empty(list);
        }



        #endregion

        #region GetSubmissionDetails Tests

        [Fact]
        public void GetSubmissionDetails_ReturnsSubmissionDetails_WhenIdExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft };
            var submission = new Submission { Id = Guid.NewGuid(), FormId = form.Id, CreatedById = Guid.NewGuid() };
            var formElement = new FormElement { Id = Guid.NewGuid(), FormId = form.Id, Name = "Element 1" };
            var formResponse = new FormResponse { Id = Guid.NewGuid(), SubmissionId = submission.Id, FormElementId = formElement.Id, Value = "Response 1" };
            context.Forms.Add(form);
            context.Submissions.Add(submission);
            context.FormElements.Add(formElement);
            context.FormResponses.Add(formResponse);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<FormVM>(It.IsAny<Form>())).Returns(new FormVM { Id = form.Id, FormType = form.FormType, Status = form.Status, FormElements = new List<FormElementVM> { new FormElementVM { Id = formElement.Id, Name = formElement.Name, Value = formResponse.Value } } });
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetSubmissionDetails(submission.Id);

            // Assert
            var formVm = Assert.IsType<FormVM>(result.Value);
            Assert.Equal(form.Id, formVm.Id);
            Assert.Equal(form.FormType, formVm.FormType);
            Assert.Equal(form.Status, formVm.Status);
            Assert.Single(formVm.FormElements);
            Assert.Equal(formElement.Id, formVm.FormElements.First().Id);
            Assert.Equal(formResponse.Value, formVm.FormElements.First().Value);
        }
        [Fact]
        public void GetSubmissionDetails_ReturnsNotFound_WhenSubmissionDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetSubmissionDetails(Guid.NewGuid());

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Submission not found", notFoundResult.Value);
        }

        [Fact]
        public void GetSubmissionDetails_ReturnsNotFound_WhenFormDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var submission = new Submission { Id = Guid.NewGuid(), FormId = Guid.NewGuid(), CreatedById = Guid.NewGuid() };
            context.Submissions.Add(submission);
            context.SaveChanges();

            var mockLogger = new Mock<ILogger<FormController>>();
            var mockMapper = new Mock<IMapper>();
            var mockService = new Mock<IFormService>();
            var mockMediator = new Mock<IMediator>();

            var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

            // Act
            var result = controller.GetSubmissionDetails(submission.Id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Form not found", notFoundResult.Value);
        }

        #endregion

        #region AssignFormAndParentIds Tests

        //[Fact]
        //public void AssignFormAndParentIds_SetsFormIdAndParentId()
        //{
        //    // Arrange
        //    var context = GetInMemoryDbContext();
        //    var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft };
        //    var formElement = new FormElement { Id = Guid.NewGuid(), Name = "Element 1" };
        //    context.Forms.Add(form);
        //    context.FormElements.Add(formElement);
        //    context.SaveChanges();

        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockMapper = new Mock<IMapper>();
        //    var mockService = new Mock<IFormService>();
        //    var mockMediator = new Mock<IMediator>();

        //    var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

        //    // Act
        //    controller.AssignFormAndParentIds(formElement, form.Id, null);

        //    // Assert
        //    Assert.Equal(form.Id, formElement.FormId);
        //    Assert.Null(formElement.ParentId);
        //}

        //[Fact]
        //public void AssignFormAndParentIds_SetsFormIdAndParentId_ForChildElements()
        //{
        //    // Arrange
        //    var context = GetInMemoryDbContext();
        //    var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft };
        //    var parentElement = new FormElement { Id = Guid.NewGuid(), Name = "Parent Element" };
        //    var childElement = new FormElement { Id = Guid.NewGuid(), Name = "Child Element" };
        //    parentElement.Children.Add(childElement);
        //    context.Forms.Add(form);
        //    context.FormElements.Add(parentElement);
        //    context.FormElements.Add(childElement);
        //    context.SaveChanges();

        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockMapper = new Mock<IMapper>();
        //    var mockService = new Mock<IFormService>();
        //    var mockMediator = new Mock<IMediator>();

        //    var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

        //    // Act
        //    controller.AssignFormAndParentIds(parentElement, form.Id, null);

        //    // Assert
        //    Assert.Equal(form.Id, parentElement.FormId);
        //    Assert.Null(parentElement.ParentId);
        //    Assert.Equal(form.Id, childElement.FormId);
        //    Assert.Equal(parentElement.Id, childElement.ParentId);
        //}

        //[Fact]
        //public void AssignFormAndParentIds_SetsFormIdAndParentId_ForNestedChildElements()
        //{
        //    // Arrange
        //    var context = GetInMemoryDbContext();
        //    var form = new Form { Id = Guid.NewGuid(), Name = "Test Form", FormType = FormTypes.FIC, Status = FormStatus.Draft };
        //    var parentElement = new FormElement { Id = Guid.NewGuid(), Name = "Parent Element" };
        //    var childElement = new FormElement { Id = Guid.NewGuid(), Name = "Child Element" };
        //    var grandChildElement = new FormElement { Id = Guid.NewGuid(), Name = "Grandchild Element" };
        //    childElement.Children.Add(grandChildElement);
        //    parentElement.Children.Add(childElement);
        //    context.Forms.Add(form);
        //    context.FormElements.Add(parentElement);
        //    context.FormElements.Add(childElement);
        //    context.FormElements.Add(grandChildElement);
        //    context.SaveChanges();

        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockMapper = new Mock<IMapper>();
        //    var mockService = new Mock<IFormService>();
        //    var mockMediator = new Mock<IMediator>();

        //    var controller = new FormController(context, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

        //    // Act
        //    controller.AssignFormAndParentIds(parentElement, form.Id, null);

        //    // Assert
        //    Assert.Equal(form.Id, parentElement.FormId);
        //    Assert.Null(parentElement.ParentId);
        //    Assert.Equal(form.Id, childElement.FormId);
        //    Assert.Equal(parentElement.Id, childElement.ParentId);
        //    Assert.Equal(form.Id, grandChildElement.FormId);
        //    Assert.Equal(childElement.Id, grandChildElement.ParentId);
        //}

        #endregion

    //    #region AssignValues Tests

    //    [Fact]
    //    public void AssignValues_SetsValuesForFormElements()
    //    {
    //        // Arrange
    //        var formElements = new List<FormElementVM>
    //{
    //    new FormElementVM { Id = Guid.NewGuid(), Name = "Element 1" },
    //    new FormElementVM { Id = Guid.NewGuid(), Name = "Element 2" }
    //};
    //        var responses = new List<FormResponse>
    //{
    //    new FormResponse { FormElementId = formElements[0].Id, Value = "Value 1" },
    //    new FormResponse { FormElementId = formElements[1].Id, Value = "Value 2" }
    //};

    //        var mockLogger = new Mock<ILogger<FormController>>();
    //        var mockMapper = new Mock<IMapper>();
    //        var mockService = new Mock<IFormService>();
    //        var mockMediator = new Mock<IMediator>();

    //        var controller = new FormController(null, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

    //        // Act
    //        controller.AssignValues(formElements, responses);

    //        // Assert
    //        Assert.Equal("Value 1", formElements[0].Value);
    //        Assert.Equal("Value 2", formElements[1].Value);
    //    }

    //    [Fact]
    //    public void AssignValues_SetsValuesForNestedFormElements()
    //    {
    //        // Arrange
    //        var childElement = new FormElementVM { Id = Guid.NewGuid(), Name = "Child Element" };
    //        var parentElement = new FormElementVM { Id = Guid.NewGuid(), Name = "Parent Element", Children = new List<FormElementVM> { childElement } };
    //        var responses = new List<FormResponse>
    //{
    //    new FormResponse { FormElementId = parentElement.Id, Value = "Parent Value" },
    //    new FormResponse { FormElementId = childElement.Id, Value = "Child Value" }
    //};

    //        var mockLogger = new Mock<ILogger<FormController>>();
    //        var mockMapper = new Mock<IMapper>();
    //        var mockService = new Mock<IFormService>();
    //        var mockMediator = new Mock<IMediator>();

    //        var controller = new FormController(null, mockMapper.Object, mockLogger.Object, mockService.Object, mockMediator.Object);

    //        // Act
    //        controller.AssignValues(new List<FormElementVM> { parentElement }, responses);

    //        // Assert
    //        Assert.Equal("Parent Value", parentElement.Value);
    //        Assert.Equal("Child Value", childElement.Value);
    //    }

    //    #endregion




    }

}