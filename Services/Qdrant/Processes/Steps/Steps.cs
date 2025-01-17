using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static PersonalWebApi.Services.Services.Qdrant.QdrantService;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{
    // https://github.com/microsoft/semantic-kernel/blob/39934f5fa338141c8a64de96895a4e1f440638d7/dotnet/samples/GettingStartedWithProcesses/README.md
    // https://www.linkedin.com/pulse/introducing-semantic-kernel-process-library-new-era-ai-latorre-g8tef
    // https://github.com/microsoft/semantic-kernel/blob/39934f5fa338141c8a64de96895a4e1f440638d7/dotnet/samples/GettingStartedWithProcesses/Step00/Steps/DoSomeWorkStep.cs
    [Experimental("SKEXP0080")]
    public sealed class Steps : KernelProcessStep
    {
        private UserInputState? _state;

        public static class Functions
        {
            public const string MyExecute = nameof(MyExecute);
        }

        [KernelFunction]
        public async ValueTask ExecuteAsync(KernelProcessStepContext context)
        {
            Debug.WriteLine($"Step - Doing Some Work...\n");
        }

        //public override ValueTask ActivateAsync(KernelProcessStepState<UserInputState> state)
        //{
        //    _state = state.State;
        //    return ValueTask.CompletedTask;
        //}

        //[KernelFunction]
        //public async ValueTask ExecuteAsync(KernelProcessStepContext context)
        //{
        //    Debug.WriteLine($"Step 1 - Doing Some Work...\n");
        //}
    }

    [Experimental("SKEXP0080")]
    public class IntroStep : KernelProcessStep
    {
        [KernelFunction]
        public void PrintIntroMessage()  // ## 3
        {
            Debug.WriteLine("Welcome to the Advanced Semantic Kernel Chatbot!\nType 'exit' at any time to quit.\n");
        }
    }

    [Experimental("SKEXP0080")]
    public class UserInputStep : KernelProcessStep<UserInputState>
    {
        private UserInputState? _state;

        public override ValueTask ActivateAsync(KernelProcessStepState<UserInputState> state)  // ## 4
        {
            if (state.State == null)
            {
                state = state with { State = new UserInputState() };
            }
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        [KernelFunction("GetUserInput")]
        public async ValueTask GetUserInputAsync(KernelProcessStepContext context)  // ## 5 , 8
        {
            Debug.WriteLine("You: ");
            var input = "Mój input";//Debug.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                input = "Hello"; // Default input
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                await context.EmitEventAsync(new() { Id = ChatBotEvents.Exit });
                return;
            }

            _state!.UserInputs.Add(input);
            await context.EmitEventAsync(new() { Id = ChatBotEvents.UserInputReceived, Data = input });
        }
    }

    [Experimental("SKEXP0080")]
    public class ChatBotResponseStep : KernelProcessStep<ChatBotState>
    {
        private ChatBotState? _state;

        public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)  // ## 6
        {
            if (state.State == null)
            {
                state = state with { State = new ChatBotState() };
            }
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        [KernelFunction("GetChatResponse")]
        public async Task GetChatResponseAsync(KernelProcessStepContext context, string userMessage, Kernel _kernel)  // ## 7 , 9
        {
            _state!.ChatMessages.Add(new(AuthorRole.User, userMessage));

            IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();
            ChatMessageContent response = await chatService.GetChatMessageContentAsync(_state.ChatMessages);

            if (response != null)
            {
                _state.ChatMessages.Add(response);
                Debug.WriteLine($"Bot: {response.Content}\n");
            }

            // Emit event to continue the conversation
            await context.EmitEventAsync(new() { Id = ChatBotEvents.ResponseGenerated });
        }
    }

    [Experimental("SKEXP0080")]
    public class ExitStep : KernelProcessStep
    {
        [KernelFunction]
        public void HandleExit()
        {
            Debug.WriteLine("Thank you for using the chatbot. Goodbye!");
        }
    }

    public class UserInputState
    {
        public List<string> UserInputs { get; set; } = new();  // ## 1
    }

    public class ChatBotState
    {
        public ChatHistory ChatMessages { get; set; } = new(); // ## 2
    }

    public static class ChatBotEvents
    {
        public const string StartProcess = "StartProcess";
        public const string UserInputReceived = "UserInputReceived";
        public const string ResponseGenerated = "ResponseGenerated";
        public const string Exit = "Exit";
    }
}
