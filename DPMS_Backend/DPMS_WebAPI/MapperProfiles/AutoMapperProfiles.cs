using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using DPMS.EmailEngine.EmailTemplates;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Auth;
using DPMS_WebAPI.ViewModels.Comment;
using DPMS_WebAPI.ViewModels.Consent;
using DPMS_WebAPI.ViewModels.Document;
using DPMS_WebAPI.ViewModels.DPIA;
using DPMS_WebAPI.ViewModels.DSAR;
using DPMS_WebAPI.ViewModels.Email;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using DPMS_WebAPI.ViewModels.Form;
using DPMS_WebAPI.ViewModels.IssueTicket;
using DPMS_WebAPI.ViewModels.PrivacyPolicy;
using DPMS_WebAPI.ViewModels.Purpose;
using DPMS_WebAPI.ViewModels.Responsibility;
using DPMS_WebAPI.ViewModels.Risk;
using DPMS_WebAPI.ViewModels.User;

namespace DPMS_WebAPI.MapperProfiles
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<RiskResolveVM, Risk>();

            CreateMap<DPIADocument, DPIADocumentVM>()
                .ForMember(dest => dest.ResponsibilityId, opt => opt.MapFrom(src => src.ResponsibleId))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.FullName));

            CreateMap<ConsentPurposeImportVM, ConsentPurpose>();
            CreateMap<ConsentImportVM, Consent>();

            CreateMap<Risk, ExportRiskVM>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.Strategy, opt => opt.MapFrom(src => src.Strategy.ToString()))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy == null ? "Unknow" : src.CreatedBy.FullName));

            CreateMap<DPIAStartedEvent, DPIAStartVM>();

            CreateMap<DPIAApprovalNotification, DPIAApprovalVM>().ReverseMap();
            CreateMap<DPIAReviewRequestNotification, DPIAReviewRequest>().ReverseMap();
            CreateMap<DPIAStartedEvent, DPIAStartVM>().ReverseMap();
            CreateMap<UserAddedToDPIANotification, UserAddedToDPIAVM>().ReverseMap();
            CreateMap<ResponsibilityCompletedNotification, ResponsibilityCompletedVM>().ReverseMap();


            CreateMap<DsarImportVM, DSAR>();

            CreateMap<User, ProfileVM>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Groups, opt => opt.MapFrom(src => src.Groups.Where(g => g.IsGlobal).Select(g => g.Name).ToList()))
                .ForMember(dest => dest.Systems, opt => opt.MapFrom(src => src.Groups.Where(g => !g.IsGlobal).Select(g => g.System.Name).ToList()))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.UserName))
                .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy.UserName));

            CreateMap<User, CreateAccountVM>().ReverseMap();
            CreateMap<User, UserListVM>()
                //.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();
            CreateMap<User, RegisterVM>().ReverseMap();
            CreateMap<Feature, CreateFeatureVM>().ReverseMap();
            CreateMap<FeatureVM, CreateFeatureVM>().ReverseMap();
            CreateMap<Feature, FeatureVM>()
          .ForMember(dest => dest.Children, opt =>
              opt.MapFrom(src => src.Children)).ReverseMap();
            CreateMap<Feature, FeatureDetailVM>().ReverseMap();
            CreateMap<Group, CreateGroupVM>().ReverseMap();
            CreateMap<GroupVM, CreateGroupVM>().ReverseMap();
            CreateMap<Group, GroupVM>().ReverseMap();
            //CreateMap<GroupVM, CreateGroupVM>().ReverseMap();
            CreateMap<Group, GroupDetailVM>()
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users))
                .ReverseMap();
            CreateMap<User, UserVM>().ReverseMap();

            CreateMap<DPIAEvent, EventDetailVM>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.EventType))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Event))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            ;
            CreateMap<ExternalSystem, ExternalSystemVM>()
                .ForMember(dest => dest.CreateBy, opt => opt.MapFrom(src => src.CreatedBy.FullName))
                .ReverseMap();
            CreateMap<UpdateSystemVM, ExternalSystem>().ReverseMap();
            CreateMap<Form, FormVM>().ReverseMap();
            CreateMap<FormElement, FormElementVM>().ReverseMap();
            CreateMap<IssueTicket, IssueTicketVM>()
            .ForMember(dest => dest.ExternalSystemName, opt => opt.MapFrom(src => src.ExternalSystem != null ? src.ExternalSystem.Name : null))
            .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents ?? new List<IssueTicketDocument>()))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FullName : ""))
                  .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy != null ? src.LastModifiedBy.FullName : ""));
            CreateMap<IssueTicketVM, IssueTicket>()
                .ForMember(dest => dest.Documents, opt => opt.Ignore()); // Handle Documents separately if needed

            CreateMap<IssueTicketDocument, DocumentVM>().ReverseMap();
            CreateMap<IssueTicketCreateVM, IssueTicket>()
           .ForMember(dest => dest.Id, opt => opt.Condition((src, dest, srcMember) => src.Id != Guid.Empty));

            // Optionally add reverse mapping if needed
            CreateMap<IssueTicket, IssueTicketCreateVM>();
            CreateMap<AddCommentVM, Comment>();

            // CreateMap<DPIADetailVM, DPIA>()
            //     .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<IssueTicketStatus>(src.Status, true))) // Chuyển đổi string thành enum
            //     .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Bỏ qua null values

            CreateMap<DPIAMember, DPIAMemberVM>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<DPIAResponsibility, DPIAResponsibilityDetailVM>()
                .ForMember(dest => dest.ResponsibilityName, opt => opt.MapFrom(src => src.Responsibility.Title))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate));

            CreateMap<MemberResponsibility, MemberResponsibilityVM>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Member.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Member.User.Email))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.Member.CreatedAt))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Member.UserId))
                .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId))
                .ForMember(dest => dest.CompletionStatus, opt => opt.MapFrom(src => src.CompletionStatus)); // Map CompletionStatus directly

            CreateMap<Comment, CommentVM>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<DPIAResponsibility, DPIAResponsibilityListVM>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Responsibility.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Responsibility.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // Chuyển đổi enum thành string
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.MemberResponsibilities)) // Map MemberResponsibilities directly
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Bỏ qua null values

            CreateMap<DPIA, DPIADetailVM>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.FullName))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.LastModifiedBy.FullName))
                .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents))
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.DPIAMembers))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // Chuyển đổi enum thành string
                .ForMember(dest => dest.Responsibilities, opt => opt.MapFrom(src => src.Responsibilities))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Bỏ qua null values

            // CreateFormVm -> Form
            CreateMap<CreateFormVm, Form>()
                .ForMember(dest => dest.FormElements, opt => opt.MapFrom(src => src.FormElements));

            // CreateFormElementsVm -> FormElement
            CreateMap<CreateFormElementsVm, FormElement>()
                .ForMember(dest => dest.FormId, opt => opt.Ignore()) // Ignore formId
                .ForMember(dest => dest.ParentId, opt => opt.Ignore()) // Handle hierarchy
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children ?? new List<CreateFormElementsVm>()));

            // Submission --> FICSubmissionVM
            CreateMap<FICSubmissionVM, Submission>().ReverseMap()
                .ForMember(dest => dest.ExternalSystemName, opt => opt.MapFrom(src => src.ExternalSystem.Name))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Form.Name))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy == null ? "UNKNOW" : src.CreatedBy.FullName));

            CreateMap<FormResponse, FormElementVM>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            CreateMap(typeof(PagedResponse<>), typeof(PagedResponse<>));

            CreateMap<AccountCredentialsVM, AccountCredentials>().ReverseMap();
            CreateMap<Purpose, PurposeVM>().ReverseMap();

            CreateMap<SystemGroupVM, Group>().ReverseMap();
            CreateMap<SystemUserVM, User>().ReverseMap();

            CreateMap<ExternalSystemDetailVM, ExternalSystem>().ReverseMap();

            CreateMap<ExternalSystem, ExternalSystemDetailVM>()
                .ForMember(dest => dest.Purposes, opt => opt.MapFrom(src => src.Purposes.Select(p => p.Purpose)))
                .ForMember(dest => dest.HasApiKey, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ApiKeyHash)));

            // CreateMap<DPIAListVM, DPIA>().ReverseMap();

            CreateMap<DPIA, DPIAListVM>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.FullName))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.LastModifiedBy.FullName))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.ExternalSystem.Name))// Convert enum to string
                .ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.ExternalSystem.Id))// Convert enum to string

                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
            CreateMap<DPIACreateVM, DPIA>()
                .ForSourceMember(src => src.Documents, opt => opt.DoNotValidate())
                .ForMember(dest => dest.Documents, opt => opt.Ignore());

            CreateMap<DPIAUpdateVM, DPIA>();

            CreateMap<DPIAResponsibilityCreateVM, DPIAResponsibility>()
                .ForSourceMember(src => src.UserIds, opt => opt.DoNotValidate());

            CreateMap<DPIAMemberCreateVM, DPIAMember>()
                .ForMember(dest => dest.Responsibilities, opt => opt.MapFrom
                            (src => src.Responsibilities.Select(id => new MemberResponsibility { DPIAResponsibilityId = id })));

            CreateMap<ConsentPurposeVM, Purpose>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PurposeId))
                .ReverseMap();

            CreateMap<ConsentPurposeVM, ConsentPurpose>().ReverseMap();
            CreateMap<ConsentPVM, ConsentPurpose>().ReverseMap();

            CreateMap<Consent, ConsentVM>()
                .ForMember(dest => dest.ExternalSystemName, opt => opt.MapFrom(src => src.ExternalSystem.Name))
                .ForMember(dest => dest.ConsentMethod, opt => opt.MapFrom(src => src.ConsentMethod.ToString()))
                .ReverseMap();

            CreateMap<SubmitConsentVM, Consent>()
                .ForMember(dest => dest.Purposes, opt => opt.MapFrom(src => src.ConsentPurposes))
                .ForMember(dest => dest.ConsentDate, opt => opt.MapFrom(_ => DateTime.Now)) // complete this, set ConsentDate to current time
            .ReverseMap();

            // Map from Purpose to PurposeLogVM
            CreateMap<Purpose, ConsentPurposeLogVM>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
            CreateMap<Purpose, ListPurposeVM>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.Email));

            // Map from ConsentPurpose to ConsentPurposeLogVM, including nested Purpose
            CreateMap<ConsentPurpose, ConsentPurposeLogVM>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Purpose.Name))  // Get name from Purpose
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));     // Map status directly

            // Map from Consent to ConsentLogVM
            CreateMap<Consent, ConsentLogVM>()
                .ForMember(dest => dest.ConsentPurpose, opt => opt.MapFrom(src => src.Purposes))
                .ForMember(dest => dest.ExternalSystemName, opt => opt.MapFrom(src => src.ExternalSystem.Name));  // Map nested purposes

            CreateMap<Responsibility, CreateResponsibilityVM>().ReverseMap();

            // CreateMap<DPIADetailVM, DPIA>().ReverseMap();
            // Replace the existing CreateMap<DPIADetailVM, DPIA>().ReverseMap(); with this:
            CreateMap<DPIADetailVM, DPIA>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (src.Status)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
                .ForMember(dest => dest.ExternalSystemId, opt => opt.MapFrom(src => src.ExternalSystemId))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Handle this separately if needed
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore()) // Handle this separately if needed  
                .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.LastModifiedAt))
                .ForMember(dest => dest.Documents, opt => opt.Ignore()) // Handle separately if needed
                .ForMember(dest => dest.DPIAMembers, opt => opt.Ignore()) // Handle separately if needed
                .ForMember(dest => dest.Events, opt => opt.Ignore()) // Typically not mapped from VM to entity
                .ForMember(dest => dest.Responsibilities, opt => opt.Ignore()) // Handle separately if needed
                .ForMember(dest => dest.ExternalSystem, opt => opt.Ignore()) // Handle separately if needed
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Skip null values
            CreateMap<UserAddedToSystemNotification, UserAddedToSystemVM>();

            CreateMap<PrivacyPolicy, PolicyVM>().ReverseMap();
            CreateMap<ListPolicyVM, PrivacyPolicy>().ReverseMap();
            CreateMap<DSAR, SubmitDsarVM>().ReverseMap();
            CreateMap<DSAR, DsarVM>().ReverseMap();
            CreateMap<UpdateStatusVM, DSAR>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ReverseMap();
            CreateMap<Risk, RiskListVM>();
            CreateMap<Risk, RiskVM>().ReverseMap();

            CreateMap<DPIAResponsibility, DPIAResponsibilityDetailVM>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.ResponsibilityId, opt => opt.MapFrom(src => src.ResponsibilityId))
           .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
           .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comment))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))


           .ForMember(dest => dest.ResponsibilityName, opt => opt.MapFrom(src =>
               src.Responsibility != null ? src.Responsibility.Title : null))
           .ForMember(dest => dest.Members, opt => opt.MapFrom(src =>
               src.MemberResponsibilities != null ? src.MemberResponsibilities : new List<MemberResponsibility>()))

           .ForMember(dest => dest.Documents, opt => opt.Ignore());
        }
    }
}
