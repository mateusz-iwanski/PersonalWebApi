namespace PersonalWebApi.Exceptions
{
    public class ChatHistoryMessageException : CustomException
    {
        public ChatHistoryMessageException(string message) : base(message)
        {
        }

        public ChatHistoryMessageException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
