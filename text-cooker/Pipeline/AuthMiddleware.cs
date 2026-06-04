using System.Security.Claims;
using text_cooker.Core;
using text_cooker.Core.Interfaces;

namespace text_cooker.Pipeline;

public class AuthMiddleware(IJwtService jwt, IUserRepository users) : IMiddleware
{
    public async Task InvokeAsync(ContextEx ctx, Func<Task> next)
    {
        var authHeader = ctx.HttpContext.Request.Headers["Authorization"];

        if (authHeader is not null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length);

            var principal = jwt.ValidateAccessToken(token);
            if (principal != null)
            {
                ctx.Claims = principal;

                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var user = await users.GetByIdAsync(userId);
                ctx.CurrentUser = user;
            }
        }
        
        await next();
    }
}
