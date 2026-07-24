using FluentValidation;
using MechanicShop.Application.Features.Billings.Queries.GetInvoicePdf;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public sealed class GetInvoicePdfQueryValidator : AbstractValidator<GetInvoicePdfQuery>
{
    public GetInvoicePdfQueryValidator()
    {
        RuleFor(request => request.InvoiceId)
            .NotEmpty()
            .WithErrorCode("InvoiceId_Is_Required")
            .WithMessage("InvoiceId is required.");
    }
}