using Azure.Storage.Blobs.Models;
using DocumentFormat.OpenXml.Bibliography;
using iText.Commons.Utils;
using PersonalWebApi.Models.FileStorage;
using PersonalWebApi.Models.Storage;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.Services.Qdrant;
using PersonalWebApi.Services.Services.System;
using PersonalWebApi.Utilities.Document;

/// <summary>
/// Serves as a versatile and extensible foundation for file storage services, encapsulating 
/// essential file operations with the rest of functionality (for example logging). This abstract class is 
/// designed to be inherited by specific storage provider implementations, such as Azure Blob Storage, 
/// while maintaining a consistent interface and behavior.
/// </summary>
/// <remarks>
/// By implementing the <see cref="IFileStorageService"/> interface, this class ensures a standardized 
/// approach to file storage operations. It seamlessly integrates with <see cref="IAssistantHistoryManager"/> 
/// to log storage events, providing a robust and reliable solution for tracking and auditing file activities.
/// </remarks>
public abstract class FileStorageServiceBase : IFileStorageService
{
    protected readonly IAssistantHistoryManager _assistantHistoryManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly IQdrantService _qdrantFileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageServiceBase"/> class.
    /// </summary>
    /// <param name="assistantHistoryManager">The assistant history manager for logging storage events.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor for retrieving context information.</param>
    protected FileStorageServiceBase(IAssistantHistoryManager assistantHistoryManager, IHttpContextAccessor httpContextAccessor)
    {
        _assistantHistoryManager = assistantHistoryManager;
        _httpContextAccessor = httpContextAccessor;
        //_qdrantFileService = qdrantFileService;
    }

    /// <inheritdoc />
    public async Task RemoveFromContainer(string fileName)
    {
        await RemoveFromContainerImpl(fileName);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadFileAsync(Uri fileUri)
    {
        return await DownloadFileAsyncImpl(fileUri);
    }

    /// <inheritdoc />
    public async Task<List<BlobContainerItem>> GetContainersAsync()
    {
        return await GetContainersAsyncImpl();
    }

    /// <inheritdoc />
    public async Task<List<BlobItem>> GetFilesWithMetadataAsync()
    {
        return await GetFilesWithMetadataAsyncImpl();
    }

    /// <inheritdoc />
    public async Task<string> GetFileUrlAsync(string fileName)
    {
        return await GetFileUrlAsyncImpl(fileName);
    }

    /// <inheritdoc />
    public void SetContainer(string name)
    {
        SetContainerImpl(name);
    }

    /// <inheritdoc />
    public async Task<Uri> UploadFromUriAsync(Guid fileId, string fileUri, string fileName, bool overwrite = false, Dictionary<string, string>? metadata = null)
    {
        (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

        // file ID must be in metadata
        if (!string.IsNullOrEmpty(fileId.ToString()))
            if (metadata != null)
                metadata["fileId"] = fileId.ToString();
            else
                metadata = new Dictionary<string, string> { { "fileId", fileId.ToString() } };

        var result = await UploadFromUriAsyncImpl(fileId, fileUri, fileName, overwrite, metadata);

        // Log the upload event
        var storageEvent = new StorageEventsDto(conversationUuid, sessionUuid, fileId)
        {
            EventName = "upload",
            ServiceName = "FileStore",
            IsSuccess = true,
            ActionType = "Upload",
            FileUri = result.AbsoluteUri,
            ErrorMessage = string.Empty,
        };


        FileContentDto fileContentMetadataDto = await FileMetadataCreator.CreateMetadataFromUrlAsync(fileUri, fileId, conversationUuid, sessionUuid);

        await _assistantHistoryManager.SaveAsync(storageEvent);
        await _assistantHistoryManager.SaveAsync(fileContentMetadataDto);

        return result;
    }

    /// <inheritdoc />
    public async Task<Uri> UploadToContainerAsync(Guid fileId, IFormFile file, bool overwrite = false, Dictionary<string, string>? metadata = null)
    {
        //(Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);
        var conversationUuid = Guid.NewGuid();
        var sessionUuid = Guid.NewGuid();

        // file ID must be in metadata
        if (!string.IsNullOrEmpty(fileId.ToString()))
            if (metadata != null)
                metadata["fileId"] = fileId.ToString();
            else
                metadata = new Dictionary<string, string> { { "fileId", fileId.ToString() } };

        var serverUri = await UploadToContainerAsyncImpl(fileId, file, overwrite, metadata);

        // Log the upload event
        var storageEvent = new StorageEventsDto(conversationUuid, sessionUuid, fileId)
        {
            EventName = "upload",
            ServiceName = "FileStore",
            IsSuccess = true,
            ActionType = "Upload",
            FileUri = serverUri.AbsoluteUri,
            ErrorMessage = string.Empty,
        };

        FileContentDto fileContentMetadataDto = await FileMetadataCreator.CreateMetadataFromFormFileAsync(file, fileId, conversationUuid, sessionUuid);

        await _assistantHistoryManager.SaveAsync(storageEvent);
        await _assistantHistoryManager.SaveAsync(fileContentMetadataDto);

        return serverUri;
    }

    /// <summary>
    /// Removes a file from the container. This method must be implemented by derived classes.
    /// </summary>
    /// <param name="fileName">The name of the file to remove.</param>
    protected abstract Task RemoveFromContainerImpl(string fileName);

    /// <summary>
    /// Downloads a file from the specified URI. This method must be implemented by derived classes.
    /// </summary>
    /// <param name="fileUri">The URI of the file to download.</param>
    protected abstract Task<Stream> DownloadFileAsyncImpl(Uri fileUri);

    /// <summary>
    /// Gets a list of blob containers. This method must be implemented by derived classes.
    /// </summary>
    protected abstract Task<List<BlobContainerItem>> GetContainersAsyncImpl();

    /// <summary>
    /// Gets a list of files with metadata. This method must be implemented by derived classes.
    /// </summary>
    protected abstract Task<List<BlobItem>> GetFilesWithMetadataAsyncImpl();

    /// <summary>
    /// Gets the URL of a file. This method must be implemented by derived classes.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    protected abstract Task<string> GetFileUrlAsyncImpl(string fileName);

    /// <summary>
    /// Sets the container to be used for storage operations. This method must be implemented by derived classes.
    /// </summary>
    /// <param name="name">The name of the container.</param>
    protected abstract void SetContainerImpl(string name);

    /// <summary>
    /// Uploads a file from a URI to the container. This method must be implemented by derived classes.
    /// </summary>
    /// <param name="fileUri">The URI of the file to upload.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
    /// <param name="metadata">Optional metadata for the file.</param>
    protected abstract Task<Uri> UploadFromUriAsyncImpl(Guid fileId, string fileUri, string fileName, bool overwrite = false, Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Uploads a file to the container. This method must be implemented by derived classes.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
    /// <param name="metadata">Optional metadata for the file.</param>
    /// <param name="fileId">Optional file identifier.</param>
    protected abstract Task<Uri> UploadToContainerAsyncImpl(Guid fileId, IFormFile file, bool overwrite = false, Dictionary<string, string>? metadata = null);
}
