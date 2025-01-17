using Microsoft.SemanticKernel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.Qdrant.Processes.Steps
{

    [Experimental("SKEXP0080")]
    public class TestIntroStep : KernelProcessStep<Message>
    {
        [KernelFunction]
        public void PrintIntroMessage(string k)  // ## 3
        {
            Debug.WriteLine($"Welcome to the Advanced {k} Semantic Kernel Chatbot!\nType 'exit' at any time to quit.\n");
        }

        private Message? _state;

        //public override ValueTask ActivateAsync(KernelProcessStepState<Message> message)
        //{
        //    if (message.State == null)
        //    {
        //        message = message with { State = new Message() };
        //    }
        //    _state = message.State;
        //    return ValueTask.CompletedTask;
        //}

        [KernelFunction("SetMessage")]
        public async ValueTask SetMessageAsync(KernelProcessStepContext context)  // ## 5 , 8
        {
            var input = "Wiadomość Hello World";
            _state!.Text = input;
            await context.EmitEventAsync(new() { Id = ChatBotEvents2.UserInputReceived, Data = input });
        }
    }

    [Experimental("SKEXP0080")]
    public class TestStepsOne : KernelProcessStep<Message2>
    {
        private Message? _state;

        public override ValueTask ActivateAsync(KernelProcessStepState<Message2> message)
        {

            return ValueTask.CompletedTask;
        }

        [KernelFunction("MyReader")]
        public async ValueTask MyReaderAsync(KernelProcessStepContext context, string userMessage)
        {
            Debug.WriteLine($"Step - {userMessage}\n");
            await context.EmitEventAsync(new() { Id = ChatBotEvents2.UserInputReceived, Data = _state.Text });
        }
    }

    [Experimental("SKEXP0080")]
    public class TestSteps2 : KernelProcessStep
    {
        [KernelFunction("MyExecute")]
        public async ValueTask MyExecuteAsync(KernelProcessStepContext context, string userMessage)
        {
            Debug.WriteLine($"Step - {userMessage}\n");
            await context.EmitEventAsync(new() { Id = ChatBotEvents2.Exit });
        }
    }

    public static class ChatBotEvents2
    {
        public const string StartProcess = "StartProcess";
        public const string UserInputReceived = "UserInputReceived";
        public const string ResponseGenerated = "ResponseGenerated";
        public const string Exit = "Exit";
    }

    public class Message
    {
        public string Text { get; set; } = string.Empty;
    }

    public class Message2
    {
        public string Text { get; set; } = string.Empty;
    }
}
