using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace PersonalWebApi.Exceptions
{
    public class CustomException : Exception
    {
        private static readonly ILogger<CustomException> _logger;

        static CustomException()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            _logger = loggerFactory.CreateLogger<CustomException>();
        }

        public CustomException(
            string message,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = ""
            ) 
            : base($"File: {filePath}|Line: {lineNumber}|Member: {memberName}|message: {message}")
        {
            LogException(message, null, filePath, lineNumber, memberName);
        }

        public CustomException(
            string message, 
            Exception inner,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = ""
            )
            : base($"{message} (File: {filePath}, Line: {lineNumber}, Member: {memberName})", inner)
        {
            LogException(message, inner, filePath, lineNumber, memberName);
        }

        private void LogException(
            string message, 
            Exception? innerException, 
            string filePath, 
            int lineNumber, 
            string memberName
            )
        {
            var logMessage = $"{message} (File: {filePath}, Line: {lineNumber}, Member: {memberName})";
            if (innerException != null)
            {
                _logger.LogError(innerException, logMessage);
            }
            else
            {
                _logger.LogError(logMessage);
            }
        }
    }
}
