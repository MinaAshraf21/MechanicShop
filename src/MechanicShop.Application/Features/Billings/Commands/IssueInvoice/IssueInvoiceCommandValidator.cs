using FluentValidation;

namespace MechanicShop.Application.Features.Billings.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
  public IssueInvoiceCommandValidator()
  {
    RuleFor(i => i.WorkOrderId)
                            .NotEmpty()
                            .WithErrorCode("WorkOrderId_Required")
                            .WithMessage("Work order Id is required.");
  }
}