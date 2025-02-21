﻿using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using PersonalWebApi.Agent;
using PersonalWebApi.Agent.SemanticKernel.Plugins.DataGathererPlugin;
using PersonalWebApi.Controllers.Agent;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.Document.Steps;
using PersonalWebApi.Processes.Metadata.Events;
using PersonalWebApi.Processes.Qdrant.Events;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.Metadata.Steps
{
    public static class TagifyStepFunctions
    {
        public const string GenerateTags = nameof(GenerateTags);
        public const string GenerateChunksTags = nameof(GenerateChunksTags);
    }

    /// <summary>
    /// Generate tag for file content
    /// </summary>
    [Experimental("SKEXP0080")]
    public sealed class TagifyStep : KernelProcessStep
    {
        [KernelFunction(TagifyStepFunctions.GenerateTags)]
        public async ValueTask GenerateTagsAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            // get kernel by appsettings.StepAgentMappings
            var config = kernel.GetRequiredService<IConfiguration>();
            var agent = new AgentRouter(config).GetStepKernel(TagifyStepFunctions.GenerateTags);

            var tagifyPlugin = new TagCollectorPlugin();
            var result = await tagifyPlugin.GenerateTagsPluginAsync(documentStepDto.Content, agent);
            
            foreach (var tag in result)
            {
                documentStepDto.Tags.Add(tag);
            }

            documentStepDto.Events.Add("file tugified");

            await context.EmitEventAsync(new() { Id = TagifyStepEvents.TagsGenerated, Data = documentStepDto });
        }
    }
}

