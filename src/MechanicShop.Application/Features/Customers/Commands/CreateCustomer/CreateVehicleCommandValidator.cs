using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
      public CreateVehicleCommandValidator()
      {
            RuleFor(c => c.Make)
                  .NotEmpty().WithMessage("Make field is required.")
                  .MaximumLength(50).WithMessage("Make length cannot exceed 50 chars.");

            RuleFor(c => c.Model)
                  .NotEmpty()
                  .MaximumLength(50);

            RuleFor(c => c.LicensePlate)
                  .NotEmpty()
                  .MaximumLength(10);

            RuleFor(c => c.Year)
                  .NotEmpty();
            }
}