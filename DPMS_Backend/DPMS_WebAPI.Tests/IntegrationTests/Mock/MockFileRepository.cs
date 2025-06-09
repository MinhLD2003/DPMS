using DPMS_WebAPI.Interfaces.Repositories;

namespace DPMS_WebAPI.Tests.IntegrationTests.Mock
{
    public class MockFileRepository : IFileRepository
    {
        private readonly Dictionary<string, Stream> _files = new Dictionary<string, Stream>();

        public Task<string> UploadFileAsync(Stream fileStream, string key, string contentType)
        {
            _files[key] = fileStream;
            return Task.FromResult($"https://mock-storage.com/{key}");
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            // Extract the key from the URL
            var key = fileUrl.Replace("https://mock-storage.com/", "");
            
            if (_files.ContainsKey(key))
            {
                _files.Remove(key);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public string GetFileUrl(string fileName)
        {
            return $"https://mock-storage.com/{fileName}";
        }

        public Task<Stream?> GetFileAsync(string fileName)
        {
            if (_files.TryGetValue(fileName, out var stream))
            {
                return Task.FromResult<Stream?>(stream);
            }
            
            return Task.FromResult<Stream?>(null);
        }
    }
} 