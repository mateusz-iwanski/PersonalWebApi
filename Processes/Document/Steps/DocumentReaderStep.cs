using Microsoft.SemanticKernel;
using PersonalWebApi.Processes.Document.Events;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.FileStorage.Models;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public static class DocumentReaderStepFunctions
    {
        public const string ReadUri = nameof(ReadUri);
        public const string PrintIntroMessage = nameof(PrintIntroMessage);
    }


    [Experimental("SKEXP0080")]
    public sealed class DocumentReaderStep : KernelProcessStep
    {
        [KernelFunction(DocumentReaderStepFunctions.ReadUri)]
        public async ValueTask ReadUriAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            var documentReader = kernel.GetRequiredService<IDocumentReaderDocx>();
            var text = await documentReader.ReadAsync(documentStepDto.Uri);

            documentStepDto.Content = text;

            await context.EmitEventAsync(new() { Id = DocumentEvents.Readed, Data = documentStepDto });
        }
    }
}
