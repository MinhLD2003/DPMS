using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories;
public interface IFileRepository
{
    Task<string> UploadFileAsync(Stream fileStream, string key, string contentType );
    Task<bool> DeleteFileAsync(string fileUrl);
    string GetFileUrl(string fileName);
    Task<Stream?> GetFileAsync(string fileName);
  
}
