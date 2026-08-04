using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.Features.Customers.Queries.GetCustomers;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Application.Features.Customers.Commands.DeleteCustomer;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/customers")]
[ApiVersion("1.0")]
public class CustomerController(ISender sender) : ApiController
{
  [HttpGet]
  [Authorize]
  [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetCustomers")]
  [EndpointSummary("Retrieves list of customers.")]
  [EndpointDescription("Returns all customers associated with the current user.")]
  [ProducesDefaultResponseType]
  [OutputCache(Duration = 60)]
  public async Task<ActionResult> GetAll(CancellationToken ct)
  {
    var result = await sender.Send(new GetCustomersQuery(), ct);
    return result.Match(Ok, Problem);
  }

  [HttpGet("{customerId:guid}")]
  [Authorize]
  [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetCustomerById")]
  [EndpointSummary("Retrieves a customer by id.")]
  [EndpointDescription("Returns detailed information about the specified customer if found.")]
  [ProducesDefaultResponseType]
  [OutputCache(Duration = 60)]
  public async Task<ActionResult> GetById(Guid customerId, CancellationToken ct)
  {
    var result = await sender.Send(new GetCustomerByIdQuery(customerId), ct);
    return result.Match(Ok, Problem);
  }
  [HttpPost]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("CreateCustomer")]
  [EndpointSummary("Creates a new customer.")]
  [EndpointDescription("Adds a new customer to the system.")]

  public async Task<ActionResult> CreateCustomer(CreateCustomerRequest request, CancellationToken ct)
  {
    var vehicles = request.Vehicles.Select(v => new CreateVehicleCommand(v.Make, v.Model, v.Year, v.LicensePlate)).ToList();
    var result = await sender.Send(new CreateCustomerCommand(request.Name, request.Email, request.PhoneNumber, vehicles), ct);
    return result.Match(Ok, Problem);
  }
  [HttpPut("{customerId:guid}")]
  [Authorize(Roles = "Manager")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("UpdateCustomer")]
  [EndpointSummary("Update an existing customer.")]
  [EndpointDescription("Updates a customer and its associated vehicle.")]
  public async Task<ActionResult> UpdateCustomer(Guid customerId, UpdateCustomerRequest request, CancellationToken ct)
  {
    var vehicles = request.Vehicles.Select(v => new UpdateVehicleCommand(v.VehicleId, v.Make, v.Model, v.Year, v.LicensePlate)).ToList();
    var result = await sender.Send(new UpdateCustomerCommand(customerId, request.Name, request.Email, request.PhoneNumber, vehicles), ct);
    return result.Match(r => NoContent(), Problem);
  }

  [HttpDelete("{customerId:guid}")]
  [Authorize(Roles = "Manager")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("DeleteCustomer")]
  [EndpointSummary("Delete a customer.")]
  [EndpointDescription("Delete an existing customer and its associated vehicles.")]
  public async Task<ActionResult> DeleteCustomer(Guid customerId, CancellationToken ct)
  {
    var result = await sender.Send(new DeleteCustomerCommand(customerId), ct);
    return result.Match(r => NoContent(), Problem);
  }
}