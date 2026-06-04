using FluentValidation;
using text_cooker.Models;

namespace text_cooker.Validators;

public class LoginRequestValidator : AbstractValidator<AuthModels.LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Имя пользователя обязательно");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}