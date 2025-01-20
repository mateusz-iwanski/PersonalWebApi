using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using PersonalWebApi.Agent;
using PersonalWebApi.Agent.Memory.Observability;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using PersonalWebApi.Processes.Document.Events;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.Qdrant.Events;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.Document.Steps
{
    public static class DocumentInfoStepFunctions
    {
        public const string SpecifyDocumentType = nameof(SpecifyDocumentType);
    }

    [Experimental("SKEXP0080")]
    public sealed class SpecifyDocumentTypeStep : KernelProcessStep
    {
        [KernelFunction(DocumentInfoStepFunctions.SpecifyDocumentType)]
        public async ValueTask SpecifyDocumentTypeAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            // get kernel by appsettings.StepAgentMappings
            var config = kernel.GetRequiredService<IConfiguration>();
            var agent = new AgentRouter(config).GetStepKernel(DocumentInfoStepFunctions.SpecifyDocumentType);

            var documentInfo = new DocumentInfoPlugin();

            var docType = await documentInfo.SpecifyDocumentType(documentStepDto.Content, agent);

            foreach (var dType in docType)
            {
                documentStepDto.DocumentType.Add(dType);
            }

            documentStepDto.Events.Add("specify document type");

            await context.EmitEventAsync(new() { Id = DocumentEvents.SpecifiedDocumentType, Data = documentStepDto });
        }
    }

}
