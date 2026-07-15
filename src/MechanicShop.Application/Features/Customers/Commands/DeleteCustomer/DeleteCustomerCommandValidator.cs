using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.DeleteCustomer;

public sealed class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
  public DeleteCustomerCommandValidator()
  {
    RuleFor(c => c.CustomerId).NotEmpty().WithMessage("Customer id is required.");
  }
}