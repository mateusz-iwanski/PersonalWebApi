using Microsoft.SemanticKernel;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Services.FileStorage;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public record FileStorageIFormFileStepItem(IFormFile Document, bool Overwrite, Guid FileId);

    public static class FileStorageFunctions
    {
        public const string UploadIFormFile = nameof(UploadIFormFile);
    }

    public static class FileStorageEvents
    {
        public const string Uploaded = nameof(Uploaded);
    }

    [Experimental("SKEXP0080")]
    public sealed class FileStorageStep : KernelProcessStep
    {

        [KernelFunction(FileStorageFunctions.UploadIFormFile)]
        public async ValueTask UploadIFormFileAsync(KernelProcessStepContext context, Kernel kernel, FileStorageIFormFileStepItem fileStorageStepItem)
        {
            var fileStorageService = kernel.GetRequiredService<IFileStorageService>();
            var configuration = kernel.GetRequiredService<IConfiguration>();

            fileStorageService.SetContainer(
                configuration.GetSection("Qdrant:Container:Name").Value ?? throw new SettingsException("Qdrant:Container:Name not exists")
                );
            
            var uri = await fileStorageService.UploadToContainerAsync(
                file: fileStorageStepItem.Document, 
                overwrite: fileStorageStepItem.Overwrite, 
                fileId: fileStorageStepItem.FileId
                );

            await context.EmitEventAsync(new() { Id = FileStorageEvents.Uploaded, Data = uri });
        }
    }
}
