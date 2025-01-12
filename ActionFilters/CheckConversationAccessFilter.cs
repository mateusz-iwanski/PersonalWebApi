using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace PersonalWebApi.ActionFilters
{
    public class CheckConversationAccessFilter : IActionFilter
    {
        private readonly ClaimsPrincipal _userClaimsPrincipal;

        public CheckConversationAccessFilter(IHttpContextAccessor httpContextAccessor)
        {
            _userClaimsPrincipal = httpContextAccessor.HttpContext?.User ?? throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext.User));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.TryGetValue("conversationUuid", out var conversationUuid))
            {

                context.HttpContext.Items["conversationUuid"] = conversationUuid;
                context.HttpContext.Items["sessionUuid"] = Guid.NewGuid().ToString();
            }
            else
            {
                context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult("Conversation UUID is required.");
                return;
            }

            // Additional logic to check access can be added here
        }
    }
}
