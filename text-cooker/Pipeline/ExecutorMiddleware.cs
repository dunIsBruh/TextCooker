using System.Reflection;
using text_cooker.Core;
using text_cooker.Core.Attributes;
using text_cooker.Core.Interfaces;

namespace text_cooker.Pipeline;

public class ExecutorMiddleware : IMiddleware
{
    public async Task InvokeAsync(ContextEx context, Func<Task> next)
    {
        if (context.IsStaticFile)
        {
            await next();
            return;
        }
        
        var handler = context.EndpointHandler;
        if (handler == null)
        {
            throw new InvalidOperationException("No endpoint handler found in context.");
        }

        var method = handler.Method;
        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];

            // 1) ContextEx
            if (p.ParameterType == typeof(ContextEx))
            {
                args[i] = context;
                continue;
            }

            // 2) Body model
            if (context.BodyInputType != null && p.GetCustomAttribute<BodyAttribute>() != null)
            {
                args[i] = context.BodyInput;
                continue;
            }

            // 3) Route parameters (with attribute)
            var routeAttr = p.GetCustomAttribute<RouteAttribute>();
            if (routeAttr != null)
            {
                var raw = context.RouteParams![routeAttr.Name];
                args[i] = Convert.ChangeType(raw, p.ParameterType);
                continue;
            }

            // 4) Route parameters by name fallback
            if (context.RouteParams != null && context.RouteParams.TryGetValue(p.Name!, out var valueStr))
            {
                args[i] = Convert.ChangeType(valueStr, p.ParameterType);
                continue;
            }

            // 5) Default values
            if (p.HasDefaultValue)
            {
                args[i] = p.DefaultValue;
                continue;
            }

            throw new Exception($"Cannot resolve parameter '{p.Name}' for endpoint '{method.Name}'");
        }

        var result = method.Invoke(handler.Target, args);

        if (result is Task task)
            await task;
    }
}
