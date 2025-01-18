using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MongoDB.Driver;
using PersonalWebApi.Models.Storage;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.FileStorage.Events;
using PersonalWebApi.Processes.FileStorage.Models;
using PersonalWebApi.Processes.Metadata.Steps;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Services.Services.System;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.FileStorage.Steps
{
    public static class LogFileActionStepFunctions
    {
        public const string SaveActionLog = nameof(SaveActionLog);
    }

    [Experimental("SKEXP0080")]
    public class LogFileActionStep : KernelProcessStep
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="kernel"></param>
        /// <param name="content"></param>
        /// <returns>Null</returns>
        /// <remarks>
        /// 
        /// Required service from the provider:
        ///     - IAssistantHistoryManager
        ///     - IHttpContextAccessor
        /// 
        /// </remarks>
        [KernelFunction(LogFileActionStepFunctions.SaveActionLog)]
        public async ValueTask SaveActionLogAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            var assistantHistoryManager = kernel.GetRequiredService<IAssistantHistoryManager>();
            var httpContextAccessor = kernel.GetRequiredService<IHttpContextAccessor>();

            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(httpContextAccessor);

            var storageEvent = new StorageEventsDto(conversationUuid, sessionUuid, documentStepDto.FileId)
            {
                EventName = "upload",
                ServiceName = "fileStore",
                IsSuccess = true,
                ActionType = "upload",
                FileUri = documentStepDto.Uri.ToString(),
            };

            await assistantHistoryManager.SaveAsync(storageEvent);

            await context.EmitEventAsync(new() { Id = FileEvents.ActionLogSaved , Data = documentStepDto });
        }

    }
}
