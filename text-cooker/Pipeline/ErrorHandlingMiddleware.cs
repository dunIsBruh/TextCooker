using FluentValidation.Results;
using text_cooker.Core;
using text_cooker.Core.Interfaces;

namespace text_cooker.Pipeline;

public class ErrorHandlingMiddleware : IMiddleware
{
    
    public async Task InvokeAsync(ContextEx context, Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (BadRequestException e)
        { 
            await context.BadRequest(new { message = $"Bad request: {e.Message}" });
        }
        catch (ValidationException vex)
        {
            var details = vex.Errors
                .Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                .ToArray();
            
            await context.BadRequest(new { error = "validation failed", details });
        }
        catch (UnauthorizedAccessException)
        {
            await context.Unauthorized(new { error = "unauthorized" });
        }
        catch (Exception ex)
        {
            await context.InternalServerError(new { error = $"internal server error {ex.Message}" });
        }
    }
}

public class ForbiddenException() : Exception("Forbidden");
public class BadRequestException : Exception
{
    public BadRequestException() : base("Bad request"){}
    public BadRequestException(string message) : base(message) {}
}

public class ValidationException(IEnumerable<ValidationFailure> errors) : Exception("Validation failed")
{
    public IEnumerable<ValidationFailure> Errors { get; } = errors;
}