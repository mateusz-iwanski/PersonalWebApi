namespace PersonalWebApi.Exceptions
{
    public class SettingsException : CustomException
    {
        public SettingsException(string message) : base(message)
        {
        }

        public SettingsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
