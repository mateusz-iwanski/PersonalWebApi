using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PersonalWebApi.Agent.Memory.Observability;
using PersonalWebApi.Processes.Document.Events;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.Metadata.Events;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.Document.Steps
{
    public static class SummarizeStepFunctions
    {
        public const string SummarizeText = nameof(SummarizeText);
    }

    [Experimental("SKEXP0080")]
    public sealed class SummarizeStep : KernelProcessStep
    {
        [KernelFunction(SummarizeStepFunctions.SummarizeText)]
        public async ValueTask SummarizeTextAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            var memory = kernel.GetRequiredService<KernelMemoryWrapper>();
            await memory.ImportDocumentAsync(
                documentStepDto.Uri.ToString(),
                documentId: documentStepDto.Id.ToString(),
                steps: Constants.PipelineOnlySummary
            );

            var results = await memory.SearchSummariesAsync(filter: MemoryFilters.ByDocument(documentStepDto.Id.ToString()));

            foreach (var result in results)
            {
                documentStepDto.Summary = result.Partitions.First().Text;
            }

            documentStepDto.Events.Add("source content summarized");

            await context.EmitEventAsync(new() { Id = DocumentEvents.Summarized, Data = documentStepDto })
        }
    }
}


