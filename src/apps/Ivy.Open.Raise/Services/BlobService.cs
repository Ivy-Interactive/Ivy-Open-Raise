using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

namespace Ivy.Open.Raise.Services;

public interface IBlobService
{
    Task<bool> CreateContainerAsync(string containerName, CancellationToken cancellationToken = default);

    Task<bool> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListContainersAsync(CancellationToken cancellationToken = default);

    Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default);

    Task UploadAsync(string containerName, string blobName, Stream data, string? contentType = null,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> ListBlobsAsync(string containerName, string? prefix = null,
        CancellationToken cancellationToken = default);

    Task<BlobInfo?> GetBlobAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default);

    string GetDownloadUrl(string containerName, string blobName, TimeSpan? expiresIn = null);
}

public sealed record BlobInfo(string ContainerName, string BlobName, string ContentType, long Size, DateTime LastModified);

public class BlobService : IBlobService
{
    private readonly IAmazonS3 _s3Client;

    public BlobService(string endpoint, string accessKey, string secretKey)
    {
        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true
        };

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        _s3Client = new AmazonS3Client(credentials, config);
    }

    public async Task<bool> CreateContainerAsync(string containerName, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutBucketRequest
            {
                BucketName = containerName
            };
            await _s3Client.PutBucketAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou")
        {
            return false;
        }
    }

    public async Task<bool> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteBucketRequest
            {
                BucketName = containerName
            };
            await _s3Client.DeleteBucketAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            return false;
        }
    }

    public async Task<IEnumerable<string>> ListContainersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.ListBucketsAsync(cancellationToken);
        return response.Buckets.Select(b => b.BucketName);
    }

    public async Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetBucketLocationAsync(containerName, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            return false;
        }
    }

    public async Task UploadAsync(string containerName, string blobName, Stream data, string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var detectedContentType = contentType ?? ContentTypeDetector.Detect(data, blobName);

        var request = new PutObjectRequest
        {
            BucketName = containerName,
            Key = blobName,
            InputStream = data,
            ContentType = detectedContentType
        };
        await _s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = containerName,
            Key = blobName
        };
        var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        return response.ResponseStream;
    }

    public async Task<bool> DeleteAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = containerName,
                Key = blobName
            };
            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception)
        {
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = containerName,
                Key = blobName
            };
            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<IEnumerable<BlobInfo>> ListBlobsAsync(string containerName, string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = containerName,
            Prefix = prefix
        };

        var response = await _s3Client.ListObjectsV2Async(request, cancellationToken);

        return response.S3Objects.Select(obj => new BlobInfo(
            ContainerName: containerName,
            BlobName: obj.Key,
            ContentType: "application/octet-stream", // S3 ListObjects doesn't return content type
            Size: obj.Size ?? 0,
            LastModified: obj.LastModified ?? DateTime.UtcNow
        ));
    }

    public async Task<BlobInfo?> GetBlobAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = containerName,
                Key = blobName
            };

            var metadata = await _s3Client.GetObjectMetadataAsync(request, cancellationToken);

            return new BlobInfo(
                ContainerName: containerName,
                BlobName: blobName,
                ContentType: metadata.Headers.ContentType ?? "application/octet-stream",
                Size: metadata.ContentLength,
                LastModified: metadata.LastModified ?? DateTime.UtcNow
            );
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public string GetDownloadUrl(string containerName, string blobName, TimeSpan? expiresIn = null)
    {
        var expiration = expiresIn ?? TimeSpan.FromHours(1);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = containerName,
            Key = blobName,
            Expires = DateTime.UtcNow.Add(expiration),
            Protocol = Protocol.HTTP
        };

        return _s3Client.GetPreSignedURL(request);
    }
}

public static class BlobServiceExtensions
{
    public static void UseBlobs(this IServiceCollection services)
    {
        services.AddSingleton<IBlobService>(s =>
        {
            var configuration = s.GetRequiredService<IConfiguration>();
            var endpoint = configuration["Blobs:Endpoint"];
            var accessKey = configuration["Blobs:AccessKey"];
            var secretKey = configuration["Blobs:SecretKey"];
            return new BlobService(endpoint, accessKey, secretKey);
        });
    }
}

public class BlobSecrets : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return
        [
            new("Blobs:Endpoint"),
            new("Blobs:AccessKey"),
            new("Blobs:SecretKey")
        ];
    }
}