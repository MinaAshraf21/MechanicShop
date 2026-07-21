using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

public sealed class GenerateTokenQueryValidator : AbstractValidator<GenerateTokenQuery>
{
  public GenerateTokenQueryValidator()
  {
    RuleFor(t => t.Email)
                        .NotEmpty()
                        .WithErrorCode("Email_Null_Or_Empty")
                        .WithMessage("Email is required.")
                        .EmailAddress()
                        .WithMessage("Invalid email address.");

    RuleFor(t => t.Password)
                        .NotEmpty()
                        .WithErrorCode("Password_Null_Or_Empty")
                        .WithMessage("Password is required.");
  }
}