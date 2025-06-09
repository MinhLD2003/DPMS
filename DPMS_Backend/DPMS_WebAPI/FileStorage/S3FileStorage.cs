using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.FileStorage;
public class S3FileStorage : IFileRepository
{
    private readonly DPMSContext _context;
    private readonly AmazonS3Client _s3Client;
    private readonly string _bucketName;

    public S3FileStorage(DPMSContext context, IConfiguration config)
    {
        _context = context;
        _s3Client = new AmazonS3Client(
            config["AWS:AccessKey"],
            config["AWS:SecretKey"],
            RegionEndpoint.GetBySystemName(config["AWS:Region"])
        );
        _bucketName = config["AWS:BucketName"]!;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string key, string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,  
            InputStream = fileStream,
            ContentType = contentType
        };
        await _s3Client.PutObjectAsync(request);
        return $"https://{_bucketName}.s3.amazonaws.com/{key}";
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        var fileName = fileUrl.Split('/').Last();
        var request = new DeleteObjectRequest { BucketName = _bucketName, Key = fileName };
        var response = await _s3Client.DeleteObjectAsync(request);
        return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
    }

    public string GetFileUrl(string fileName)
    {
        return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
    }

    public async Task<Stream?> GetFileAsync(string fileName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // File not found
        }
    }

}
