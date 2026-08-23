using FluentValidation;
using FluentValidation.Results;
using MechanicShop.Application.Behaviors;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Tests.Common.WorkOrders;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
  private readonly IValidator<CreateWorkOrderCommand> _validator = Substitute.For<IValidator<CreateWorkOrderCommand>>();
  private readonly ValidationBehavior<CreateWorkOrderCommand, Result<WorkOrderDto>> _validationBehavior;

  public ValidationBehaviorTests()
  {
    _validationBehavior = new(_validator);
  }

  [Fact]
  public async Task Handle_WhenValidatorIsNull_ShouldInvokeNext()
  {
    // Given
    var request = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();
    var response = WorkOrderFactory.CreateWorkOrder().Value.ToDto();
    var validationBehavior = new ValidationBehavior<CreateWorkOrderCommand, Result<WorkOrderDto>>();

    // When
    var result = await validationBehavior.Handle(request, _ => Task.FromResult((Result<WorkOrderDto>)response), CancellationToken.None);
  
    // Then
    Assert.True(result.IsSuccess);
    Assert.Equal(response, result.Value);
  }

  [Fact]
  public async Task Handle_WhenValidatorResultIsValid_ShouldInvokeNextBehavior()
  {
    // Given
    var request = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();
    var response = WorkOrderFactory.CreateWorkOrder().Value.ToDto();
    _validator.ValidateAsync(request, Arg.Any<CancellationToken>()).Returns(new ValidationResult());
    // When
    var result = await _validationBehavior.Handle(request, _ => Task.FromResult((Result<WorkOrderDto>)response), CancellationToken.None);
  
    // Then
    Assert.True(result.IsSuccess);
    Assert.Equal(response, result.Value);
  }

  [Fact]
  public async Task Handle_WhenValidatorResultIsNotValid_ShouldReturnListOfErrors()
  {
    // Given
    var request = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();
    var response = WorkOrderFactory.CreateWorkOrder().Value.ToDto();
    List<ValidationFailure> validationFailures = [new(propertyName: "property1", errorMessage: "property1 is invalid")];
    _validator.ValidateAsync(request, Arg.Any<CancellationToken>()).Returns(new ValidationResult(validationFailures));
    // When
    var result = await _validationBehavior.Handle(request, _ => Task.FromResult((Result<WorkOrderDto>)response), CancellationToken.None);
  
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal("property1", result.TopError.Code);
    Assert.Equal("property1 is invalid", result.TopError.Description);
  }
}