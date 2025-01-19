using Microsoft.SemanticKernel;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.FileStorage;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.FileStorage.Events;
using PersonalWebApi.Processes.FileStorage.Models;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Services.Qdrant.Processes.Steps;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.FileStorage.Steps
{
    public static class FileStorageFunctions
    {
        public const string UploadIFormFile = nameof(UploadIFormFile);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <return>Uri of uploaded file</return>
    [Experimental("SKEXP0080")]
    public sealed class FileStorageStep : KernelProcessStep
    {
        [KernelFunction(FileStorageFunctions.UploadIFormFile)]
        public async ValueTask UploadIFormFileAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepItem)
        {
            var fileStorageService = kernel.GetRequiredService<IFileStorageService>();
            var configuration = kernel.GetRequiredService<IConfiguration>();

            fileStorageService.SetContainer(
                configuration.GetSection("Qdrant:Container:Name").Value ?? throw new SettingsException("Qdrant:Container:Name not exists")
                );

            var uri = await fileStorageService.UploadToContainerAsync(
                fileId: documentStepItem.FileId,
                file: documentStepItem.iFormFile,
                overwrite: documentStepItem.Overwrite,
                metadata: documentStepItem.Metadata
                );

            documentStepItem.Uri = uri;
            documentStepItem.Events.Add("Uploaded on external server");

            await context.EmitEventAsync(
                new() 
                { 
                    Id = FileEvents.Uploaded, 
                    Data = documentStepItem
                });
        }
    }
}
