using Microsoft.SemanticKernel;
using PersonalWebApi.Agent;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using PersonalWebApi.Processes.Document.Events;
using PersonalWebApi.Processes.Document.Models;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.Document.Steps
{
    public static class DocumentLanguageStepFunctions
    {
        public const string SpecifyDocumentLanguage = nameof(SpecifyDocumentLanguage);
    }

    [Experimental("SKEXP0080")]
    public sealed class SpecifyDocumentLanguageStep : KernelProcessStep
    {
        [KernelFunction(DocumentLanguageStepFunctions.SpecifyDocumentLanguage)]
        public async ValueTask SpecifyDocumentLanguageAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            // get kernel by appsettings.StepAgentMappings
            var config = kernel.GetRequiredService<IConfiguration>();
            var agent = new AgentRouter(config).GetStepKernel(DocumentLanguageStepFunctions.SpecifyDocumentLanguage);

            var documentInfo = new DocumentInfoPlugin();

            var docLanguage = await documentInfo.SpecifyDocumentLanguage(documentStepDto.Content, agent);

            documentStepDto.Language = docLanguage;

            documentStepDto.Events.Add("specify document language");

            await context.EmitEventAsync(new() { Id = DocumentEvents.SpecifiedDocumentLanguage, Data = documentStepDto });
        }
    }
}
