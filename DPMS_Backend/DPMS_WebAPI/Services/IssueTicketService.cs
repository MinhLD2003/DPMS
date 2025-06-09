using Amazon.S3.Model.Internal.MarshallTransformations;
using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.IssueTicket;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DPMS_WebAPI.Services
{
    public class IssueTicketService : BaseService<IssueTicket>, IIssueTicketService
    {
        private readonly IMapper _mapper;
        private readonly IFileRepository _fileRepository;

        // Constants for file validation
        private const long MAX_FILE_SIZE = 25 * 1024 * 1024; // 25MB in bytes
        private readonly string[] ALLOWED_EXTENSIONS = { ".xlsx", ".docx", ".pdf", ".png", ".jpeg" };

        public IssueTicketService(IUnitOfWork unitOfWork, IMapper mapper, IFileRepository fileRepository)
            : base(unitOfWork)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        }

        protected override IRepository<IssueTicket> Repository => _unitOfWork.IssueTickets;

        /// <summary>
        /// Validates files against size and type constraints
        /// </summary>
        /// <param name="files">Files to validate</param>
        /// <exception cref="ArgumentException">Thrown when a file violates constraints</exception>
        private void ValidateFiles(List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return;

            foreach (var file in files)
            {
                if (file.Length > MAX_FILE_SIZE)
                {
                    throw new ArgumentException($"File '{file.FileName}' exceeds the maximum allowed size of 25MB.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!ALLOWED_EXTENSIONS.Contains(extension))
                {
                    throw new ArgumentException($"File '{file.FileName}' has an invalid extension. Only .xlsx, .docx,.pdf,.png and .jpeg files are allowed.");
                }
            }
        }

        public async Task<IssueTicket> CreateIssueTicket(IssueTicket issueTicket, List<IFormFile> files)
        {
            // Validate files before processing
            ValidateFiles(files);

            var uploadedDocuments = new List<IssueTicketDocument>();
            issueTicket.Documents = uploadedDocuments;

            foreach (var file in files)
            {
                using var stream = file.OpenReadStream();
                string key = $"{DocumentType.IssueTicket}/{issueTicket.Id}/{file.FileName}";
                string fileUrl = await _fileRepository.UploadFileAsync(stream, key, file.ContentType);

                uploadedDocuments.Add(new IssueTicketDocument
                {
                    Title = file.FileName,
                    IssueTicketId = issueTicket.Id,
                    FileUrl = key,
                    FileFormat = Path.GetExtension(file.FileName).TrimStart('.'),
                });
            }

            var result = await AddAsync(issueTicket);

            return result;
        }

        public async Task<List<IssueTicketDocument>> UpdateIssueTicketFilesOnS3(Guid id, List<IFormFile> newFiles, List<string> removedFiles)
        {
            // Validate new files before processing
            ValidateFiles(newFiles);

            // Get only the relevant files for this ticket
            var existingFiles = await _unitOfWork.IssueTicketDocuments.FindAsync(f => f.IssueTicketId == id);

            List<IssueTicketDocument> removedDocuments = new List<IssueTicketDocument>();
            List<IssueTicketDocument> newDocuments = new List<IssueTicketDocument>();

            // Handle removed files
            if (removedFiles != null && removedFiles.Any())
            {
                removedDocuments = existingFiles.Where(f => removedFiles.Contains(f.FileUrl)).ToList();

                if (removedDocuments.Any())
                {
                    await _unitOfWork.IssueTicketDocuments.BulkDeleteAsync(removedDocuments);
                    await Task.WhenAll(removedDocuments.Select(f => _fileRepository.DeleteFileAsync(f.FileUrl)));
                }
            }

            // Handle new files
            if (newFiles != null && newFiles.Any())
            {
                foreach (var file in newFiles)
                {
                    string fileKey = $"IssueTicket/{id}/{file.FileName}";
                    using var stream = file.OpenReadStream();
                    string fileUrl = await _fileRepository.UploadFileAsync(stream, fileKey, file.ContentType);

                    newDocuments.Add(new IssueTicketDocument
                    {
                        Title = file.FileName,
                        IssueTicketId = id,
                        FileUrl = fileKey,
                        FileFormat = Path.GetExtension(file.FileName).TrimStart('.'),
                    });
                }

                if (newDocuments.Any())
                {
                    await _unitOfWork.IssueTicketDocuments.BulkAddAsync(newDocuments);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var list = await _unitOfWork.IssueTicketDocuments.FindAsync(f => f.IssueTicketId == id);
            return list.ToList();
        }
    }
}
