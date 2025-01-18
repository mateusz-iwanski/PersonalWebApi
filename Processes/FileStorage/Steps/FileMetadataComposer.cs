using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.SemanticKernel;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.FileStorage.Events;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.FileStorage.Steps
{
    public static class FileMetadataComposerFunction
    {
        public const string Collect = nameof(Collect);
    }

    [Experimental("SKEXP0080")]
    public class FileMetadataComposer : KernelProcessStep
    {
        [KernelFunction(FileMetadataComposerFunction.Collect)]
        public async ValueTask CollectAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            if (documentStepDto.iFormFile is null)
            {
                throw new System.ArgumentNullException(nameof(documentStepDto.iFormFile));
            }

            documentStepDto.Metadata.Add("Name", documentStepDto.iFormFile.FileName);
            documentStepDto.Metadata.Add("ContentType", documentStepDto.iFormFile.ContentType);
            documentStepDto.Metadata.Add("Length", documentStepDto.iFormFile.Length.ToString());
            documentStepDto.Metadata.Add("FileName", documentStepDto.iFormFile.FileName);
            documentStepDto.Metadata.Add("ContentDisposition", documentStepDto.iFormFile.ContentDisposition);
            documentStepDto.Metadata.Add("Header", documentStepDto.iFormFile.Headers?.ToString() ?? "");
            documentStepDto.Metadata.Add("HashCode", documentStepDto.iFormFile.GetHashCode().ToString());
            documentStepDto.Metadata.Add("UploadedAt", DateTime.UtcNow.ToString("o"));
            documentStepDto.Metadata.Add("IFomrFile", "True");
            documentStepDto.Metadata.Add("HeaderName", documentStepDto.iFormFile.Name);

            // Add all headers to metadata
            if (documentStepDto.iFormFile.Headers != null)
            {
                foreach (var header in documentStepDto.iFormFile.Headers)
                {
                    documentStepDto.Metadata.Add($"Header-{header.Key}", header.Value.ToString());
                }
            }
            await context.EmitEventAsync(
                new()
                {
                    Id = FileEvents.MetadataCollected,
                    Data = documentStepDto
                });

        }

    }
}
