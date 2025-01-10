namespace PersonalWebApi.Exceptions
{
    public class InvalidUuidException : CustomException
    {
        public InvalidUuidException(string message) : base(message)
        {
        }

        public InvalidUuidException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
