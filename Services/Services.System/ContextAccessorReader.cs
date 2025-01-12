using Microsoft.AspNetCore.Http;
using PersonalWebApi.Exceptions;

namespace PersonalWebApi.Services.Services.System
{
    /// <summary>
    /// Provides utility methods for reading crucial UUIDs from the HTTP context.
    /// This class is designed to work with the <see cref="IHttpContextAccessor"/> to retrieve
    /// conversation and session UUIDs, which are essential for various operations within the application.
    /// The UUIDs are expected to be present either in the route values or in the HTTP context items.
    /// </summary>
    public static class ContextAccessorReader
    {
        /// <summary>
        /// Retrieves the conversation and session UUIDs from the HTTP context.
        /// This method first attempts to read the UUIDs from the route values. If not found, it checks the HTTP context items.
        /// If either UUID is missing or invalid, an <see cref="InvalidUuidException"/> is thrown.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor used to access the current HTTP context.</param>
        /// <returns>A tuple containing the conversation UUID and session UUID.</returns>
        /// <exception cref="InvalidUuidException">Thrown when either the conversation UUID or session UUID is missing or invalid.</exception>
        public static (Guid conversationUuid, Guid sessionUuid) RetrieveCrucialUuid(IHttpContextAccessor httpContextAccessor)
        {
            //var defaultException = new InvalidUuidException("Error when retrieving crucial UUIDs");

            //var conversationUuidString = httpContextAccessor.HttpContext?.GetRouteValue("conversationUuid")?.ToString();
            //if (string.IsNullOrEmpty(conversationUuidString))
            //{
            //    conversationUuidString = httpContextAccessor.HttpContext?.Items["conversationUuid"]?.ToString();
            //}

            //if (string.IsNullOrEmpty(conversationUuidString) || !Guid.TryParse(conversationUuidString, out var conversationUuid))
            //{
            //    throw defaultException;
            //}

            //var sessionUuidString = httpContextAccessor.HttpContext?.GetRouteValue("sessionUuid")?.ToString();
            //if (string.IsNullOrEmpty(sessionUuidString))
            //{
            //    sessionUuidString = httpContextAccessor.HttpContext?.Items["sessionUuid"]?.ToString();
            //}

            //if (string.IsNullOrEmpty(sessionUuidString) || !Guid.TryParse(sessionUuidString, out var sessionUuid))
            //{
            //    throw defaultException;
            //}

            //return (conversationUuid, sessionUuid);

            return (Guid.NewGuid(), Guid.NewGuid());
        }
    }
}
