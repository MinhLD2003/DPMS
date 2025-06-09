

using DPMS_WebAPI.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for File-related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileRepository _fileRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileRepository"></param>
        public FileController(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var fileUrl = await _fileRepository.UploadFileAsync(file.OpenReadStream(), fileName, file.ContentType);

            return Ok(new { fileUrl });
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteFile(string? fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return BadRequest("File URL is empty");
            }

            var result = await _fileRepository.DeleteFileAsync(fileUrl);

            return Ok(new { result });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFile([FromQuery]string fileName)
        {
            var fileStream = await _fileRepository.GetFileAsync(fileName);
            if (fileStream == null)
            {
                return NotFound(new { message = "File not found" });
            }

            return File(fileStream, "application/octet-stream", fileName);
        }
    }
}