using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Attribute to authorize a user based on dynamic roles configuration.
/// </summary>
/// <remarks>
/// Look on appsettings.json for the configuration of the roles (RolesConfig section)
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class DynamicRoleAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _functionName;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicRoleAuthorizeAttribute"/> class.
    /// </summary>
    /// <param name="functionName">The function name to authorize against.</param>
    public DynamicRoleAuthorizeAttribute(string functionName)
    {
        _functionName = functionName;
    }

    /// <summary>
    /// Called to check if the user is authorized to access the resource.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var rolesConfig = configuration.GetSection("RolesConfig").Get<Dictionary<string, string[]>>();
        if (rolesConfig != null && rolesConfig.TryGetValue(_functionName, out var requiredRoles))
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated || !requiredRoles.Any(role => user.IsInRole(role)))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
            }
        }
        await Task.CompletedTask;
    }
}
