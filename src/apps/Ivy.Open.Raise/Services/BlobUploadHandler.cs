using System.Collections.Immutable;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Open.Raise.Services;

/// <summary>
/// Factory for creating blob upload handlers that upload files to blob storage.
/// </summary>
public static class BlobUploadHandler
{
    /// <summary>
    /// Creates an upload handler from an IAnyState by automatically detecting the state type.
    /// Supports: FileUpload&lt;BlobInfo&gt;?, ImmutableArray&lt;FileUpload&lt;BlobInfo&gt;&gt;
    /// </summary>
    public static IUploadHandler Create(
        IAnyState anyState,
        IBlobService blobService,
        string containerName,
        Func<FileUpload, string>? getBlobName = null,
        int chunkSize = 8192)
    {
        var stateType = anyState.GetStateType();
        
        var underlyingType = Nullable.GetUnderlyingType(stateType) ?? stateType;
        
        if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(FileUpload<>))
        {
            var contentType = underlyingType.GetGenericArguments()[0];

            if (contentType == typeof(BlobInfo))
            {
                return Create(anyState.As<FileUpload<BlobInfo>?>(), blobService, containerName, getBlobName, chunkSize);
            }
        }
        
        if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(ImmutableArray<>))
        {
            var elementType = underlyingType.GetGenericArguments()[0];

            if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(FileUpload<>))
            {
                var contentType = elementType.GetGenericArguments()[0];

                if (contentType == typeof(BlobInfo))
                {
                    return Create(anyState.As<ImmutableArray<FileUpload<BlobInfo>>>(), blobService, containerName, getBlobName, chunkSize);
                }
            }
        }

        throw new ArgumentException(
            $@"Unsupported state type: {stateType}. Supported types are: FileUpload<BlobInfo>?, ImmutableArray<FileUpload<BlobInfo>>",
            nameof(anyState));
    }

    /// <summary>
    /// Creates an upload handler for a single file that uploads to blob storage.
    /// </summary>
    /// <param name="singleState">State to store the file upload with BlobInfo.</param>
    /// <param name="blobService">Service for uploading to blob storage.</param>
    /// <param name="containerName">Name of the blob container.</param>
    /// <param name="getBlobName">Optional function to generate blob name from file upload. Defaults to using file name.</param>
    /// <param name="chunkSize">Size of chunks for reading the stream.</param>
    public static IUploadHandler Create(
        IState<FileUpload<BlobInfo>?> singleState,
        IBlobService blobService,
        string containerName,
        Func<FileUpload, string>? getBlobName = null,
        int chunkSize = 8192)
        => new BlobUploadHandlerImpl(
            new SingleFileSink<BlobInfo>(singleState),
            blobService,
            containerName,
            getBlobName ?? (f => f.FileName),
            chunkSize);

    /// <summary>
    /// Creates an upload handler for multiple files that uploads to blob storage.
    /// </summary>
    /// <param name="manyState">State to store multiple file uploads with BlobInfo.</param>
    /// <param name="blobService">Service for uploading to blob storage.</param>
    /// <param name="containerName">Name of the blob container.</param>
    /// <param name="getBlobName">Optional function to generate blob name from file upload. Defaults to using file name.</param>
    /// <param name="chunkSize">Size of chunks for reading the stream.</param>
    public static IUploadHandler Create(
        IState<ImmutableArray<FileUpload<BlobInfo>>> manyState,
        IBlobService blobService,
        string containerName,
        Func<FileUpload, string>? getBlobName = null,
        int chunkSize = 8192)
        => new BlobUploadHandlerImpl(
            new MultipleFileSink<BlobInfo>(manyState),
            blobService,
            containerName,
            getBlobName ?? (f => f.FileName),
            chunkSize);
}

/// <summary>
/// Internal implementation of the blob upload handler.
/// Handles uploading to blob storage and cleaning up partial uploads on failure.
/// </summary>
internal sealed class BlobUploadHandlerImpl : IUploadHandler
{
    private readonly IFileUploadSink<BlobInfo> _sink;
    private readonly IBlobService _blobService;
    private readonly string _containerName;
    private readonly Func<FileUpload, string> _getBlobName;
    private readonly int _chunkSize;

    internal BlobUploadHandlerImpl(
        IFileUploadSink<BlobInfo> sink,
        IBlobService blobService,
        string containerName,
        Func<FileUpload, string> getBlobName,
        int chunkSize = 8192)
    {
        _sink = sink;
        _blobService = blobService;
        _containerName = containerName;
        _getBlobName = getBlobName;
        _chunkSize = chunkSize;
    }

    public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        var blobName = _getBlobName(fileUpload);
        Guid key = fileUpload.Id;

        try
        {
            // Start tracking the upload in state
            key = _sink.Start(fileUpload);

            // Upload to blob storage with progress tracking
            await UploadWithProgressAsync(
                stream,
                blobName,
                fileUpload.ContentType,
                fileUpload.Length,
                p => _sink.Progress(key, p),
                cancellationToken
            );

            // Get the blob info after successful upload
            var blobInfo = await _blobService.GetBlobAsync(_containerName, blobName, cancellationToken);
            if (blobInfo == null)
            {
                throw new InvalidOperationException($"Blob '{blobName}' was uploaded but could not be retrieved.");
            }

            _sink.Complete(key, blobInfo);
        }
        catch (OperationCanceledException)
        {
            // Update state to aborted
            _sink.Aborted(key);

            // Handler is responsible for cleanup - delete partial blob
            await _blobService.DeleteAsync(_containerName, blobName, cancellationToken);
            throw;
        }
        catch (Exception)
        {
            // Update state to failed
            _sink.Failed(key);

            // Handler is responsible for cleanup - delete partial blob
            await _blobService.DeleteAsync(_containerName, blobName, cancellationToken);
            throw;
        }
    }

    private async Task UploadWithProgressAsync(
        Stream stream,
        string blobName,
        string contentType,
        long totalLength,
        Action<float> onProgress,
        CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        var buffer = new byte[_chunkSize];
        long processedBytes = 0L;
        int bytesRead;
        float lastReportedProgress = 0f;
        const float progressThreshold = 0.05f; // Only report every 5%

        // Read the entire stream with progress tracking
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            ct.ThrowIfCancellationRequested();
            await memoryStream.WriteAsync(buffer, 0, bytesRead, ct);
            processedBytes += bytesRead;
            var progress = totalLength > 0 ? (float)processedBytes / totalLength : 0f;

            // Only report progress if it changed by at least 5%
            if (progress - lastReportedProgress >= progressThreshold)
            {
                onProgress(progress);
                lastReportedProgress = progress;
            }
        }

        // Reset stream position and upload to blob storage
        memoryStream.Position = 0;
        await _blobService.UploadAsync(_containerName, blobName, memoryStream, contentType, ct);
    }
}
