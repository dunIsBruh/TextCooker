namespace text_cooker.Core.Interfaces;

public interface IMiddleware
{
    public Task InvokeAsync(ContextEx contextEx, Func<Task> next);
}