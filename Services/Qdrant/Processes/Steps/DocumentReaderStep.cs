using Microsoft.SemanticKernel;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    public static class DocumentReaderStepFunctions
    {
        public const string ReadUri = nameof(ReadUri);
    }

    public static class DocumentReaderStepOutputEvents
    {
        public const string Readed = nameof(Readed);
    }

    [Experimental("SKEXP0080")]
    public sealed class DocumentReaderStep : KernelProcessStep
    {
        [KernelFunction(DocumentReaderStepFunctions.ReadUri)]
        public async ValueTask ReadUriAsync(KernelProcessStepContext context, Kernel kernel, Uri uri)
        {
            var documentReader = kernel.GetRequiredService<IDocumentReaderDocx>();
            var text = await documentReader.ReadAsync(uri);

            await context.EmitEventAsync(new() { Id = DocumentReaderStepOutputEvents.Readed, Data = text });
        }
    }
}
