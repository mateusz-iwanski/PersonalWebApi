using Microsoft.SemanticKernel;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.NoSQLDB.Events;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Services.NoSQLDB;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.NoSQLDB.Steps
{

    public static class DocumentDbFunctions
    {
        public const string Save = nameof(Save);
    }

    [Experimental("SKEXP0080")]
    public class DocumentDbCreateStep : KernelProcessStep
    {
        [KernelFunction(DocumentDbFunctions.Save)]
        public async ValueTask SaveAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            var fileStorageService = kernel.GetRequiredService<INoSqlDbService>();

            await fileStorageService.CreateItemAsync(documentStepDto);

            // Save document to DocumentDB
            await context.EmitEventAsync(
                new()
                {
                    Id = NoSqlDbEvents.Saved,
                    Data = documentStepDto
                });
        }
    }
}
