using System.Text.RegularExpressions;
using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
  public CreateCustomerCommandValidator()
  {
    RuleFor(c => c.Email)
          .EmailAddress().WithMessage("Invalid email address.")
          .NotEmpty().WithMessage("Email field is required.")
          .MaximumLength(40).WithMessage("Email length cannot exceed 40 chars.");

    RuleFor(c => c.Name)
          .NotEmpty().WithMessage("Name field is required.")
          .MaximumLength(70).WithMessage("Name length cannot exceed 70 chars.");

    RuleFor(c => c.PhoneNumber)
          .NotEmpty().WithMessage("Phone number field is required.")
          .Matches(@"^\+?\d{7,15}$").WithMessage("Phone number must be at least 7 numbers and may be start with +.");

    RuleFor(c => c.Vehicles)
          .NotNull().WithMessage("Vehicle list cannot be null")
          .Must(v => v.Count > 0).WithMessage("At least one vehicle is required.");

    RuleForEach(c => c.Vehicles).SetValidator(new CreateVehicleCommandValidator());
  }
}
