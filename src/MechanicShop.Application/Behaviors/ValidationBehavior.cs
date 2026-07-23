using FluentValidation;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
  : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    if(validator is null)
    {
      return await next(cancellationToken);
    }

    var result = await validator.ValidateAsync(request, cancellationToken);
    if (result.IsValid)
    {
      return await next(cancellationToken);
    }

    var errors = result.Errors.Select(e => Error.Validation(e.PropertyName, e.ErrorMessage)).ToList();

    return (dynamic)errors;
  }
}