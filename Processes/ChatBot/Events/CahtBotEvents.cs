namespace PersonalWebApi.Processes.ChatBot.Events
{
    public static class ChatBotEvents
    {
        public const string StartProcess = nameof(StartProcess);
        public const string IntroComplete = nameof(IntroComplete);
        public const string AssistantResponseGenerated = nameof(AssistantResponseGenerated);
        public const string Exit = nameof(Exit);
    }
}
