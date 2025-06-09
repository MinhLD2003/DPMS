using DPMS_WebAPI.Interfaces.Repositories;

public class FileStorageService
{
    private readonly IFileRepository _fileRepository;

    public FileStorageService(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    //public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    //{
    //    return await _fileRepository.UploadFileAsync(fileStream, fileName, contentType);
    //}

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        return await _fileRepository.DeleteFileAsync(fileUrl);
    }

    public string GetFileUrl(string fileName)
    {
        return _fileRepository.GetFileUrl(fileName);
    }
}
