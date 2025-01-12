using Microsoft.AspNetCore.Mvc.Filters;

namespace PersonalWebApi.ActionFilters
{
    public class CheckConversationAccessFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Extract conversationUuid from action arguments and inject it into HttpContext.Items
            if (context.ActionArguments.TryGetValue("conversationUuid", out var conversationUuid))
            {
                context.HttpContext.Items["conversationUuid"] = conversationUuid;
            }
            context.HttpContext.Items["sessionId"] = Guid.NewGuid().ToString();

            // Add logic to check access to the conversation
            // For example, you can check if the user has access to the conversationUuid
            // If not, you can set the result to a 403 Forbidden response
            var userHasAccess = CheckUserAccessToConversation(context.HttpContext.User, conversationUuid?.ToString());
            if (!userHasAccess)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes, if needed.
        }

        private bool CheckUserAccessToConversation(System.Security.Claims.ClaimsPrincipal user, string? conversationUuid)
        {
            // Implement your logic to check if the user has access to the conversation
            // This is just a placeholder implementation
            return !string.IsNullOrEmpty(conversationUuid) && user.Identity?.IsAuthenticated == true;
        }
    }
}
