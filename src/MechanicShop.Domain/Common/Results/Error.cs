namespace MechanicShop.Domain.Common.Results;

// public sealed record Error
public readonly record struct Error
{
  public string Code { get; }
  public string Description { get; }
  public ErrorType Type { get; }

  private Error(string code, string description, ErrorType type)
  {
    Code = code;
    Description = description;
    Type = type;
  }

  public static Error Failure(
    string code = nameof(Failure),
    string description = "General Failure.",
    ErrorType errorType = ErrorType.Failure
    ) => new(code,description,errorType);
  public static Error Unexpected(
    string code = nameof(Unexpected),
    string description = "Unexpected error.",
    ErrorType errorType = ErrorType.Unexpected
    ) => new(code,description,errorType);
  public static Error Validation(
    string code = nameof(Validation),
    string description = "Validation error.",
    ErrorType errorType = ErrorType.Validation
    ) => new(code,description,errorType);
  public static Error Conflict(
    string code = nameof(Conflict),
    string description = "Conflict error.",
    ErrorType errorType = ErrorType.Conflict
    ) => new(code,description,errorType);
  public static Error NotFound(
    string code = nameof(NotFound),
    string description = "NotFound error.",
    ErrorType errorType = ErrorType.NotFound
    ) => new(code,description,errorType);
  public static Error Unauthorized(
    string code = nameof(Unauthorized),
    string description = "Unauthorized error.",
    ErrorType errorType = ErrorType.Unauthorized
    ) => new(code,description,errorType);
  public static Error Forbidden(
    string code = nameof(Forbidden),
    string description = "Forbidden error.",
    ErrorType errorType = ErrorType.Forbidden
    ) => new(code,description,errorType);
}