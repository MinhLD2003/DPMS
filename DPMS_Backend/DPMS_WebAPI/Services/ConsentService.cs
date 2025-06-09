using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Consent;
using DPMS_WebAPI.ViewModels.DSAR;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using DPMS_WebAPI.ViewModels.Purpose;
using FlexCel.Report;
using FlexCel.XlsAdapter;
using FluentResults;

namespace DPMS_WebAPI.Services
{
    public class ConsentService : BaseService<Consent>, IConsentService
    {
        private readonly IConsentRepository _consentRepository;
        private readonly IExternalSystemRepository _externalSystemRepository;
        private readonly IExternalSystemService _externalSystemService;
        private readonly IExternalSystemPurposeRepository _externalSystemPurposeRepository;
        private readonly IConsentPurposeService _consentPurposeService;
        private readonly IConsentPurposeRepository _consentPurposeRepository;
        private readonly IConsentTokenRepository _consentTokenRepository;
        private readonly IPurposeService _purposeService;
        private readonly IPrivacyPolicyService _policyService;
        private readonly IMapper _mapper;

        private const string CONSENT_TEMPLATE_PATH = "ExcelTemplates/ConsentTemplate.xlsx"; // export template
        private const string CONSENT_IMPORT_TEMPLATE = "ExcelTemplates/ConsentImportTemplate.xlsx"; // import template

        public ConsentService(IUnitOfWork unitOfWork,
            IConsentRepository consentRepository,
            IConsentTokenRepository consentTokenRepository,
            IMapper mapper,
            IExternalSystemService externalSystemService,
            IExternalSystemRepository externalSystemRepository,
            IPurposeService purposeService,
            IExternalSystemPurposeRepository externalSystemPurposeRepository,
            IConsentPurposeService consentPurposeService,
            IConsentPurposeRepository consentPurposeRepository,
            IPrivacyPolicyService policyService) : base(unitOfWork)
        {
            _consentRepository = consentRepository;
            _mapper = mapper;
            _externalSystemService = externalSystemService;
            _purposeService = purposeService;
            _consentPurposeService = consentPurposeService;
            _externalSystemRepository = externalSystemRepository;
            _consentTokenRepository = consentTokenRepository;
            _externalSystemPurposeRepository = externalSystemPurposeRepository;
            _consentPurposeRepository = consentPurposeRepository;
            _policyService = policyService;
        }

        protected override IRepository<Consent> Repository => _consentRepository;

        /// <summary>
        /// TODO: Apply Result pattern
        /// </summary>
        /// <param name="systemId"></param>
        /// <returns></returns>
        public async Task<Stream> ExportConsentLog(Guid? systemId)
        {
            Stream resultStream = new MemoryStream();
            byte[] templateBytes = null;
            templateBytes = await File.ReadAllBytesAsync(CONSENT_TEMPLATE_PATH);
            Stream templateStream = new MemoryStream(templateBytes);

            // data
            List<Consent> consents = null;
            List<Purpose> purposes = (await _purposeService.GetAllAsync()).ToList();
            List<ConsentPurpose> consentPurposes = (await _consentPurposeService.GetAllAsync()).ToList();

            if (systemId != null)
            {
                ExternalSystem? system = await _externalSystemService.GetByIdAsync(systemId);
                if (system == null)
                {
                    throw new ArgumentException("System not found");
                }

                consents = (await FindAsync(c => c.ExternalSystemId == systemId)).ToList();
            }
            else
            {
                consents = (await GetAllAsync()).ToList();
            }

            // mapping
            List<ConsentVM> consentVMs = _mapper.Map<List<ConsentVM>>(consents);
            List<ConsentPVM> consentPVMs = _mapper.Map<List<ConsentPVM>>(consentPurposes);
            List<PurposeVM> purposeVMs = _mapper.Map<List<PurposeVM>>(purposes);

            FlexCelReport flexCel = new FlexCelReport();

            flexCel.AddTable("ConentSubmission", consentVMs);
            flexCel.AddTable("Purposes", purposeVMs);
            flexCel.AddTable("ConsentPVM", consentPVMs);

            flexCel.Run(templateStream, resultStream);

            await templateStream.DisposeAsync();
            resultStream.Position = 0;
            flexCel.Dispose();

            return resultStream;
        }

        public async Task SubmitConsent(SubmitConsentVM vm)
        {
            // get system purpose and check if the submit is valid
            if (!vm.ConsentPurposes.Any())
                throw new Exception($"Invalid consent");

            // get list system purpose belong to system
            var systemPurpose = await _externalSystemPurposeRepository
                .FindAsync(p => p.ExternalSystemId == vm.ExternalSystemId);

            // map all purpose in vm to system purpose
            var dict = systemPurpose.ToDictionary(sp => sp.PurposeId);

            // check if purpose in VM same with system purpose
            foreach (var key in vm.ConsentPurposes)
            {
                if (!dict.ContainsKey(key.PurposeId))
                    throw new Exception($"The system does not have the purpose key {key.PurposeId}");
            }  

            // map consent
            Consent consent = _mapper.Map<Consent>(vm);

            // load current active => check if a purposeId in ConsentPurposes of VM == consent.purposes.id => get the new
            // Get current activae
            var currentActiveConsent = (await _consentRepository
                .FindAsync(c => c.IsWithdrawn == false && c.Email == vm.Email))
                .OrderByDescending(c => c.ConsentDate)
                .FirstOrDefault();

            // If exist then add the previous consent purpose (consent result that not match)
            if (currentActiveConsent != null)
            {
                // Get all consent purpose of that consent
                var existConsent = await _consentPurposeRepository.FindAsync(cp => cp.ConsentId == currentActiveConsent.Id);
                // Add to consent
                foreach(var cp in existConsent)
                {
                    if (!dict.ContainsKey(cp.PurposeId))
                        consent.Purposes.Add(cp);
                }
                
            }

            // set IsWithdrawn = true and WithdrawDate = CurrentTime for consents of the same person
            // ATM I can only set Widthdraw status to consents based on email
            if (consent.Email != null)
            {
                IEnumerable<Consent> consents = (await FindAsync(c => !c.IsWithdrawn && c.Email == consent.Email));
                foreach (var c in consents)
                {
                    c.IsWithdrawn = true;
                    c.WithdrawnDate = DateTime.Now;
                    _consentRepository.Update(c);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            await AddAsync(consent);
        }

        public async Task<Consent> GetConsentByEmail(string email)
        {
            return await _consentRepository.GetConsentByEmailAsync(email);
        }

        public async Task<PagedResponse<Consent>> GetConsentLogWithPurpose(QueryParams queryParams)
        {
            return await _consentRepository.GetConsentsWithPurposeAsync(queryParams);
        }

        public async Task<bool> ValidateConsentToken(string token)
        {
            var consentToken = await _consentTokenRepository.GetByIdAsync(token);
            if (consentToken == null) return false;
            if (consentToken.TokenString != token) return false;
            if (consentToken.ExpireTime < DateTime.UtcNow) return false;
            return consentToken.IsValid;
        }

        public async Task<string> CreateConsentToken(Guid systemId)
        {
            // generate token
            ConsentToken consentToken = new ConsentToken
            {
                TokenString = ConsentUtils.GenerateTokenString(),
                IsValid = true,
                ExpireTime = DateTime.UtcNow.AddMinutes(5),
                ExternalSystemId = systemId
            };
            var token = await _consentTokenRepository.AddAsync(consentToken);
            await _unitOfWork.SaveChangesAsync();
            return token.TokenString;
        }

        public async Task UpdateToken(string token)
        {
            var consentToken = await _consentTokenRepository.GetByIdAsync(token);
            consentToken.IsValid = false;
            _consentTokenRepository.Update(consentToken);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Guid> GetSystemFromToken(string token)
        {
            var consentToken = await _consentTokenRepository.GetByIdAsync(token);
            return consentToken.ExternalSystemId;
        }

        public async Task<Stream> DownloadImportTemplateAsync(Guid systemId)
        {
            ExternalSystem? system = await _externalSystemService.GetByIdAsync(systemId);
            if (system == null)
            {
                throw new KeyNotFoundException("SystemId not found");
            }

            if (!File.Exists(CONSENT_IMPORT_TEMPLATE))
            {
                throw new FileNotFoundException("Consent import template not found", CONSENT_IMPORT_TEMPLATE);
            }

            Stream resultStream = new MemoryStream();
            byte[] templateBytes = null;
            templateBytes = await File.ReadAllBytesAsync(CONSENT_IMPORT_TEMPLATE);
            Stream templateStream = new MemoryStream(templateBytes);

            // data: get currently active policy
            var policies = await _policyService.FindAsync(p => p.Status == PolicyStatus.Active);
            PrivacyPolicy? policy = policies.FirstOrDefault();

            if (policy == null)
            {
                throw new Exception("There is no active privacy policy");
            }

            IEnumerable<Purpose> purposes = await _externalSystemService.GetSystemPurposesAsync(systemId);

            FlexCelReport flexCel = new FlexCelReport();

            flexCel.AddTable("Purpose", purposes);
            flexCel.SetValue("CurrentActivePolicyId", policy.Id);
            flexCel.SetValue("ExternalSystemId", system.Id);

            flexCel.Run(templateStream, resultStream);

            await templateStream.DisposeAsync();
            resultStream.Position = 0;
            flexCel.Dispose();

            return resultStream;
        }

        private async Task ValidateConsentImportData(List<ConsentImportVM> importData)
        {
            if (importData == null || !importData.Any())
                throw new Exception("No data to import.");

            // Lấy danh sách Purpose hợp lệ theo systemId từ bản ghi đầu tiên
            var systemId = importData.First().ExternalSystemId;
            var validPurposes = await _externalSystemService.GetSystemPurposesAsync(systemId);
            var validPurposeIds = validPurposes.Select(p => p.Id).ToHashSet();

            for (int i = 0; i < importData.Count; i++)
            {
                var record = importData[i];

                // Email bắt buộc
                if (string.IsNullOrWhiteSpace(record.Email))
                    throw new Exception($"Row {i + 6}: Email is required.");

                // ConsentDate không được null hoặc quá xa
                if (record.ConsentDate == default)
                    throw new Exception($"Row {i + 6}: Consent date is invalid.");

                // Mỗi purpose phải hợp lệ
                foreach (var purpose in record.Purposes)
                {
                    if (!validPurposeIds.Contains(purpose.PurposeId))
                        throw new Exception($"Row {i + 6}: Purpose ID {purpose.PurposeId} is not valid for the system.");
                }
            }
        }

        public async Task<Result> DoImportConsentAsync(Stream data)
        {
            XlsFile xls = new XlsFile();
            xls.Open(data);

            // reading SystemId and PolicyId from sheet 2 import template
            xls.ActiveSheet = 2; // sheet DONT DELETE
            string systemIdStr = Convert.ToString(xls.GetCellValue("A2"));
            string policyIdStr = Convert.ToString(xls.GetCellValue("A1"));

            // set active sheet to sheet contains data
            xls.ActiveSheet = 1;

            Guid systemId = Guid.Parse(systemIdStr);
            Guid policyId = Guid.Parse(policyIdStr);

            // reading purpose's ID
            IEnumerable<Purpose> purposes = await _externalSystemService.GetSystemPurposesAsync(systemId);
            // count how many purposes does this system have?
            int endCol = 3 + purposes.Count(); // 3 cột đầu là chắc chắn có, từ cột D là số lượng purposes của System nên có thể thay đổi
            const int startCol = 1;

            const int startRow = 6;

            List<ConsentImportVM> importData = new List<ConsentImportVM>();
            string[] rowData = new string[endCol - startCol + 1];

            for (int r = startRow; r <= xls.RowCount; r++)
            {
                Array.Clear(rowData, 0, rowData.Length);
                for (int cIndex = xls.ColCountInRow(r); cIndex > 0; cIndex--)
                {
                    int col = xls.ColFromIndex(r, cIndex);
                    int XF = 0;
                    object val = xls.GetCellValueIndexed(r, cIndex, ref XF);
                    rowData[col - 1] = Convert.ToString(val);
                }

                try
                {
                    ConsentImportVM tmp = new ConsentImportVM
                    {
                        Email = rowData[0],
                        ConsentMethod = rowData[1].ParseEnum<ConsentMethod>(),
                        ConsentDate = DateTime.Parse(rowData[2]),

                        ExternalSystemId = systemId,
                        PrivacyPolicyId = policyId,
                    };
                    for (int i = 0; i < purposes.Count(); i++)
                    {
                        tmp.Purposes.Add(new ConsentPurposeImportVM
                        {
                            PurposeId = Guid.Parse(Convert.ToString(xls.GetCellValue(5, 4 + i))),
                            Status = Convert.ToBoolean(xls.GetCellValue(r, 4 + i)),
                        });
                    }
                    importData.Add(tmp);
                }
                catch
                {
                    continue;
                }
            }

            // set ExternalSystemId for imported data

            // validate import data
            await ValidateConsentImportData(importData);

            // TODO: only import non-error records
            List<Consent> consents = _mapper.Map<List<Consent>>(importData);
            bool importSuccess = await BulkAddAsync(consents);

            if (importSuccess)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("Import failed");
            }
        }
    }
}
