using Asp.Versioning;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.Features.Billings.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billings.Commands.SettleInvoice;
using MechanicShop.Application.Features.Billings.Dtos;
using MechanicShop.Application.Features.Billings.Queries.GetInvoicePdf;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/invoices")]
[ApiVersion("1.0")]
[Authorize(Policy = "ManagerOnly")]
public sealed class InvoicesController(ISender sender) : ApiController
{
    [HttpPost("workorders/{workOrderId:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Issues an invoice for a work order.")]
    [EndpointDescription("Creates a new invoice for the specified work order and returns the created invoice resource.")]
    [EndpointName("IssueInvoiceForWorkOrder")]
    public async Task<IActionResult> IssueInvoice(Guid workOrderId, CancellationToken ct)
    {
        var command = new IssueInvoiceCommand(workOrderId);
        var result = await sender.Send(command, ct);
        return result.Match(
            response => CreatedAtAction(nameof(GetInvoice), new { invoiceId = response.InvoiceId }, response),
            Problem);
    }

    [HttpPut("{invoiceId:guid}/payments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Marks an invoice as paid.")]
    [EndpointDescription("Settles the specified invoice. Only users with the Manager role are authorized to perform this operation.")]
    [EndpointName("SettleInvoice")]
    public async Task<IActionResult> SettleInvoice(Guid invoiceId, CancellationToken ct)
    {
        var command = new SettleInvoiceCommand(invoiceId);

        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpGet("{invoiceId:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves an invoice by ID.")]
    [EndpointDescription("Returns detailed information about the specified invoice. Only users with the Manager role are authorized.")]
    [EndpointName("GetInvoice")]
    public async Task<IActionResult> GetInvoice(Guid invoiceId, CancellationToken ct)
    {
        var result = await sender.Send(new GetInvoiceByIdQuery(invoiceId), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("{invoiceId:guid}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Downloads the invoice as a PDF file.")]
    [EndpointDescription("Returns the invoice PDF file for the specified invoice ID. Only users with the Manager role are authorized.")]
    [EndpointName("GetInvoicePdf")]
    public async Task<IActionResult> GetInvoicePdf(Guid invoiceId, CancellationToken ct)
    {
        var result = await sender.Send(new GetInvoicePdfQuery(invoiceId), ct);

        return result.Match(
            response => File(response.Content!, response.ContentType!, response.FileName), Problem
        );
    }

}