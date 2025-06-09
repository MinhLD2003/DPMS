using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels.DSAR;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using FlexCel.Report;
using FlexCel.XlsAdapter;
using FluentResults;
using MimeKit.Tnef;

namespace DPMS_WebAPI.Services
{
    public class DsarService : BaseService<DSAR>, IDsarService
    {
        private readonly IDsarRepository _dsarRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DsarService> _logger;
        private readonly IUserService _userService;
        private readonly IExternalSystemRepository _systemRepos;

        private const string IMPORT_TEMPLATE_PATH = "ExcelTemplates/template_import_dsar.xlsx";

        public DsarService(IUnitOfWork unitOfWork,
            IDsarRepository dsarRepository,
            IMapper mapper,
            ILogger<DsarService> logger,
            IUserService userService,
            IExternalSystemRepository systemRepos) : base(unitOfWork)
        {
            _dsarRepository = dsarRepository ?? throw new ArgumentNullException(nameof(dsarRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
            _userService = userService;
            _systemRepos = systemRepos;
        }

        protected override IRepository<DSAR> Repository => _dsarRepository;

        public async Task BulkUpdatetStatus(List<UpdateStatusVM> vms)
        {
            var ids = vms.Select(vm => vm.Id);
            // Fetch the DSAR request by ID
            var dsars = (await _dsarRepository.FindAsync(d => ids.Contains(d.Id)));
            if (!dsars.Any())
            {
                throw new ArgumentNullException("cannot find dsar");
            }
            foreach (var vm in vms)
            {
                var dsar = dsars.FirstOrDefault(d => d.Id == vm.Id);
                if (dsar != null)
                {
                    _mapper.Map(vm, dsar); // Map only the updated fields
                    _dsarRepository.Update(dsar);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChangeStatus()
        {
            try
            {
                var now = DateTime.UtcNow;
                // Get all DSARs that are past their RequiredResponseDate and are not already updated
                var overdueDsars = await _dsarRepository.FindAsync(d => d.Status == DSARStatus.Submitted &&
                                d.RequiredResponse <= now);

                if (!overdueDsars.Any())
                {
                    _logger.LogInformation("No overdue DSARs found.");
                    return;
                }

                foreach (var dsar in overdueDsars)
                {
                    // Update status to "RequiredResponse"
                    dsar.Status = DSARStatus.RequiredReponse;
                    _dsarRepository.Update(dsar);
                    _logger.LogInformation($"Updated DSAR ID: {dsar.Id} to 'Required Response'.");
                }
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Processed {overdueDsars.Count()} DSAR(s) for required response status update.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating DSAR status.");
            }
        }

        public async Task<Stream> DownloadImportTemplate(User loggedInUser)
        {
            if (loggedInUser == null)
            {
                throw new ArgumentNullException("LoggedInUser is null");
            }

            if (!File.Exists(IMPORT_TEMPLATE_PATH))
            {
                throw new FileNotFoundException("Import template not found", IMPORT_TEMPLATE_PATH);
            }

            Stream resultStream = new MemoryStream();
            byte[] templateBytes = null;
            templateBytes = await File.ReadAllBytesAsync(IMPORT_TEMPLATE_PATH);
            Stream templateStream = new MemoryStream(templateBytes);

            // data: get all systems this user manages
            List<ExternalSystemVM> systems = await _userService.GetManageSystems(loggedInUser.Email);

            FlexCelReport flexCel = new FlexCelReport();

            flexCel.AddTable("System", systems);

            flexCel.Run(templateStream, resultStream);

            await templateStream.DisposeAsync();
            resultStream.Position = 0;
            flexCel.Dispose();

            return resultStream;
        }

        /// <summary>
        /// TODO: Validate more
        /// </summary>
        /// <param name="importData"></param>
        /// <returns></returns>
        private async Task ValidateDsarImportData(List<DsarImportVM> importData)
        {
            const int offset = 8;
            for (int i = 0; i < importData.Count; i++)
            {
                DsarImportVM row = importData[i];
                // 1. validate systemId
                Guid? systemId = await _systemRepos.GetIdByName(row.ExternalSystemName);
                if (systemId == null)
                {
                    row.Error += $"Line {i + offset}: System {row.ExternalSystemName} not found;";
                }
                else
                {
                    row.ExternalSystemId = systemId;
                }

                // 2. validate status: Status submitted/requiredReponse then CompletedDate must be empty, Complete/Reject status then CompleteDate must be valid
                try
                {
                    if (string.IsNullOrEmpty(row.StatusStr))
                        row.Status = null;
                    else
                        row.Status = row.StatusStr.ParseEnum<DSARStatus>();
                }
                catch
                {
                    row.Status = null;
                    row.Error += $"Line {i + offset}: Status {row.StatusStr} not found;";
                }
                try
                {
                    if (string.IsNullOrEmpty(row.TypeStr))
                        row.Type = null;
                    else
                        row.Type = row.TypeStr.ParseEnum<DSARType>();
                }
                catch
                {
                    row.Type = null;
                    row.Error += $"Line {i + offset}: DSAR Type {row.TypeStr} not found;";
                }

                // 3. validate datetime
                try
                {
                    if (string.IsNullOrEmpty(row.CompletedDateStr))
                        row.CompletedDate = null;
                    else
                        row.CompletedDate = DateTime.Parse(row.CompletedDateStr);
                }
                catch
                {
                    row.CompletedDate = null;
                    row.Error += $"Line {i + offset}: CompletedDate {row.CompletedDateStr} invalid;";
                }

                try
                {
                    if (string.IsNullOrEmpty(row.RequiredResponseStr))
                        row.RequiredResponse = null;
                    else
                        row.RequiredResponse = DateTime.Parse(row.RequiredResponseStr);
                }
                catch
                {
                    row.RequiredResponse = null;
                    row.Error += $"Line {i + offset}: RequiredResponse {row.RequiredResponseStr} invalid;";
                }

                // validate logic
                if (row.Status != null && (row.Status == DSARStatus.Submitted || row.Status == DSARStatus.RequiredReponse) && row.CompletedDate != null)
                {
                    row.Error += $"Line {i + offset}: CompleteDate must be empty when status is Submitted or RequiredReponse";
                }
                if (row.Status != null && (row.Status == DSARStatus.Rejected || row.Status == DSARStatus.Completed) && row.CompletedDate == null)
                {
                    row.Error += $"Line {i + offset}: CompleteDate is required when status is Rejected or Completed";
                }
            }
        }

        public async Task<Result> DoImportDsarAsync(Stream data)
        {
            XlsFile xls = new XlsFile();
            xls.Open(data);

            const int startCol = 1;
            const int endCol = 10;

            const int startRow = 8;

            List<DsarImportVM> importData = new List<DsarImportVM>();
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

                importData.Add(new DsarImportVM
                {
                    RequesterName = rowData[0],
                    RequesterEmail = rowData[1],
                    PhoneNumber = rowData[2],
                    Address = rowData[3],
                    Description = rowData[4],
                    TypeStr = rowData[5],
                    StatusStr = rowData[6],
                    RequiredResponseStr = rowData[7],
                    CompletedDateStr = rowData[8],
                    ExternalSystemName = rowData[9]
                });
            }

            // set ExternalSystemId for imported data

            // validate import data
            await ValidateDsarImportData(importData);

            // TODO: only import non-error records
            IEnumerable<IError> errors = importData.Where(d => !string.IsNullOrEmpty(d.Error)).Select(d => new Error(d.Error));
            if (errors.Any())
            {
                return Result.Fail(errors);
            }

            List<DSAR> dSARs = _mapper.Map<List<DSAR>>(importData);
            await BulkAddAsync(dSARs);

            return Result.Ok();
        }

        //    public async Task NotifyDsar()
        //    {
        //        try
        //        {
        //            var now = DateTime.UtcNow.Date;
        //            var overdueDsars = await _dsarRepository.GetOverDue();
        //            if (!overdueDsars.Any())
        //            {
        //                _logger.LogInformation("No overdue DSARs today.");
        //                return;
        //            }

        //            var systemIds = overdueDsars.Select(d => d.ExternalSystemId);
        //            foreach(var id in systemIds)
        //            {
        //                var mails = (await _externalSystemRepository.GetUsersAsync(id)).Select(u => u.Email.ToList();
        //            }
        //            await _emailTemplateService.SendNotifyDsar(
        //                "admin@company.com",
        //                "Daily Overdue DSARs Report",
        //                sb.ToString());

        //            _logger.LogInformation("Daily DSAR status update and email sent successfully.");
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error updating DSAR status or sending email.");
        //        }
        //    }
    }
}
