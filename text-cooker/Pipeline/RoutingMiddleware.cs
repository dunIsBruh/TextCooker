using System.Reflection;
using Microsoft.Extensions.Configuration;
using text_cooker.Core;
using text_cooker.Core.Attributes;
using text_cooker.Core.Interfaces;
using text_cooker.Entities;

namespace text_cooker.Pipeline;

public class RoutingMiddleware(RouteRegistry registry, IConfiguration configuration) : IMiddleware
{
    public async Task InvokeAsync(ContextEx context, Func<Task> next)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", configuration["Headers:Access-Control-Allow-Origin"]);
        context.Response.Headers.Add("Access-Control-Allow-Methods", configuration["Headers:Access-Control-Allow-Methods"]);
        context.Response.Headers.Add("Access-Control-Allow-Headers", configuration["Headers:Access-Control-Allow-Headers"]);
        context.Response.Headers.Add("Access-Control-Allow-Credentials", configuration["Headers:Access-Control-Allow-Credentials"]);
        
        
        var request = context.Request;
        var method = request.HttpMethod;
        var path = request.Url?.AbsolutePath ?? "/";

        // 1. Поиск маршрута
        if (!registry.TryMatch(method, path, out var match, out var route))
        {
            if (path.StartsWith("/api"))
            {
                await context.NotFound($"Route not found: {method} {path}");
                return;
            }

            context.IsStaticFile = true;
            await next();
            return;
        }

        // 2. Проверка прав
        if (!await CheckAccess(route, context))
            return;

        var handler = match.Handler;
        var methodInfo = handler.Method;
        var parameters = methodInfo.GetParameters();

        context.RouteParams = match.Parameters;
        context.EndpointHandler = handler;

        var bodyParam = parameters.FirstOrDefault(
            p => p.GetCustomAttribute<BodyAttribute>() != null
            );
        if (bodyParam != null)
        {
            context.BodyInputType = bodyParam.ParameterType;
            context.BodyInput = await context.ReadJsonAsync(bodyParam.ParameterType);
        }

        // 4. Передаём дальше (валидация/исполнение)
        await next();
    }

    private async Task<bool> CheckAccess(Route route, ContextEx ctx)
    {
        if (route.AccessLevel == Role.Anonymous)
            return true;

        if (ctx.CurrentUser == null)
        {
            await ctx.Unauthorized(new { message = "unauthorized" }); 
            return false;
        }

        if (route.AccessLevel == Role.User)
            return true;

        if (route.AccessLevel == Role.Admin && ctx.CurrentUser.Role != Role.Admin)
        {
            await ctx.Forbidden(new { message = "admin only" });
            return false;
        }

        return true;
    }
}
