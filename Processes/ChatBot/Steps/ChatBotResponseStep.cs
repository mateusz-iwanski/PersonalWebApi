using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PersonalWebApi.Processes.ChatBot.Events;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.ChatBot.Steps
{

    public static class Functions
    {
        public const string GetChatResponse = nameof(GetChatResponse);
    }

    // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithProcesses/Step01/Step01_Processes.cs
    /// <summary>
    /// Represents a step in the process that handles chat bot responses.
    /// </summary>
    [Experimental("SKEXP0080")]
    public sealed class ChatBotResponseStep : KernelProcessStep<ChatBotState>
    {
        /// <summary>
        /// The internal state object for the chat bot response step.
        /// </summary>
        internal ChatBotState? _state;

        /// <summary>
        /// ActivateAsync is the place to initialize the state object for the step.
        /// </summary>
        /// <param name="state">An instance of <see cref="ChatBotState"/></param>
        /// <returns>A <see cref="ValueTask"/></returns>
        public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Generates a response from the chat completion service.
        /// </summary>
        /// <param name="context">The context for the current step and process. <see cref="KernelProcessStepContext"/></param>
        /// <param name="userMessage">The user message from a previous step.</param>
        /// <param name="_kernel">A <see cref="Kernel"/> instance.</param>
        /// <returns></returns>
        [KernelFunction(Functions.GetChatResponse)]
        public async Task GetChatResponseAsync(KernelProcessStepContext context, string userMessage, Kernel _kernel)
        {
            _state!.ChatMessages.Add(new(AuthorRole.User, userMessage));
            IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();
            ChatMessageContent response = await chatService.GetChatMessageContentAsync(_state.ChatMessages).ConfigureAwait(false);
            if (response == null)
            {
                throw new InvalidOperationException("Failed to get a response from the chat completion service.");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"ASSISTANT: {response.Content}");
            Console.ResetColor();

            // Update state with the response
            _state.ChatMessages.Add(response);

            // emit event: assistantResponse
            await context.EmitEventAsync(new KernelProcessEvent { Id = ChatBotEvents.AssistantResponseGenerated, Data = response });
        }
    }

    /// <summary>
    /// The state object for the <see cref="ChatBotResponseStep"/>.
    /// </summary>
    public sealed class ChatBotState
    {
        internal ChatHistory ChatMessages { get; } = new();
    }
}
