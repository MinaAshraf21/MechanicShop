
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MechanicShop.Domain.Common.Results;

public class Result
{
  public static Success Success => default;
  public static Created Created => default;
  public static Updated Updated => default;
  public static Deleted Deleted => default;
}

public class Result<TValue> : IResult<TValue>
{
  private readonly TValue? _value = default;
  private readonly List<Error>? _errors = default;

  public bool IsSuccess { get; }
  public bool IsFailure => !IsSuccess;

  public List<Error>? Errors => IsFailure? _errors : [];
  public TValue Value => IsSuccess? _value! : default!;
  public Error TopError => (_errors?.Count > 0)? _errors[0] : default;

  // Json Serializer needs public constructor to serialize the Result
  [JsonConstructor]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Obsolete("For serialization only", true)]
  public Result(TValue? value, List<Error>? errors, bool isSuccess)
  {
    if (isSuccess)
    {
      _value = value ?? throw new ArgumentNullException(nameof(value));
      _errors = [];
      IsSuccess = true;
    }
    else
    {
      if(errors is null || errors.Count == 0)
        throw new ArgumentException(
          "Cannot create an ErrorOr<TValue> from an empty collection of errors. Provide at least one error."
          , nameof(errors));
      value = default;
      _errors = errors;
      IsSuccess = false;
    }
  } 
  private Result(Error error)
  {
    _errors = [error];
    IsSuccess = false;
  }
  private Result(List<Error> errors)
  {
    if(errors is null || errors.Count == 0)
    {
      throw new ArgumentException(
        "Cannot create an ErrorOr<TValue> from an empty collection of errors. Provide at least one error."
        , nameof(errors));
    }
    _errors = errors;
    IsSuccess = false;
  }
  private Result(TValue value)
  {
    if(value is null)
    {
      throw new ArgumentException(
        "Value cannot be null."
        , nameof(value));
    }
    _value = value;
    IsSuccess = true;
  }

  public TNextValue Match<TNextValue>(Func<TValue, TNextValue> OnValue, Func<List<Error>, TNextValue> OnFailure)
      => IsSuccess? OnValue(_value!) : OnFailure(_errors!);

  public static implicit operator Result<TValue>(Error error) => new(error);
  public static implicit operator Result<TValue>(List<Error> errors) => new(errors);
  public static implicit operator Result<TValue>(TValue value) => new(value);
}

public readonly record struct Success;
public readonly record struct Created;
public readonly record struct Updated;
public readonly record struct Deleted;