using FluentValidation;

namespace MechanicShop.Application.Features.Billings.Commands.SettleInvoice;

public sealed class SettleInvoiceCommandValidator : AbstractValidator<SettleInvoiceCommand>
{
  public SettleInvoiceCommandValidator()
  {
    RuleFor(i => i.InvoiceId)
                        .NotEmpty()
                        .WithErrorCode("InvoiceId_Required")
                        .WithMessage("Invoice Id is required.");
  }
}