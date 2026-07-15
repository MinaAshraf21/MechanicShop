using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
      public UpdateVehicleCommandValidator()
      {
            RuleFor(c => c.Id)
                  .NotEmpty();
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
