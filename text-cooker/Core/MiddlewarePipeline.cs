using text_cooker.Core.Interfaces;

namespace text_cooker.Core;

public class MiddlewarePipeline 
{
    private readonly Stack<Func<IServiceProvider, IMiddleware>> _middlewares = new();
    
    public MiddlewarePipeline Use<TMiddleware>() where TMiddleware : IMiddleware
    {
        _middlewares.Push(provider => provider.GetService(typeof(TMiddleware)) as IMiddleware 
                                      ?? throw new InvalidOperationException("can't register the middleware service"));
        return this;
    }

    public async Task ExecuteAsync(ContextEx contextEx, IServiceProvider serviceProvider)
    {
        var middlewares = _middlewares.Select(func => func(serviceProvider)).ToArray();
        
        var next = () => Task.CompletedTask;

        foreach (var middleware in middlewares.AsEnumerable())
        {
            var nextCopy = next;

            next = () => middleware.InvokeAsync(contextEx, nextCopy);
        }

        await next();
    }
}