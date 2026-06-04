using FluentValidation;
using FluentValidation.Results;
using text_cooker.Core;
using text_cooker.Core.Interfaces;

namespace text_cooker.Pipeline;

public class ValidatorMiddleware(IServiceProvider serviceProvider) : IMiddleware
{
    public async Task InvokeAsync(ContextEx contextEx, Func<Task> next)
    {
        var modelType = contextEx.BodyInputType;
        var model = contextEx.BodyInput;
        
        if (model is null || modelType is  null)
        {
            await next();
            return;
        }

        var validatorType = typeof(AbstractValidator<>).MakeGenericType(modelType);
        object? validator = serviceProvider.GetService(validatorType);

        if (validator is not null)
        {
            var validateMethod = validator.GetType().GetMethod("Validate", [modelType]);
            var result = (ValidationResult)validateMethod!.Invoke(validator, [model]);
            if (result is { IsValid: false })
            {
                throw new ValidationException(result.Errors);
            }
        } 
        await next();
    }
}