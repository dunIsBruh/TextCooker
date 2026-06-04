using FluentValidation;
using text_cooker.Models;

namespace text_cooker.Validators;

public class RegisterRequestValidator : AbstractValidator<AuthModels.RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Имя пользователя обязательно")
            .MinimumLength(8).WithMessage("Имя должно содержать не менее 8 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль должен содержать не менее 8 символов")
            .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches(@"[!@#$%^&*]").WithMessage("Пароль должен содержать хотя бы один спецсимвол (!@#$%^&*)");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Подтверждение пароля обязательно")
            .Equal(x => x.Password).WithMessage("Пароль и его подтверждение не совпадают");
    }
}