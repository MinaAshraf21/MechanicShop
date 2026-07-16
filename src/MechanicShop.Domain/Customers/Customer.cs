using System.Net.Mail;
using System.Text.RegularExpressions;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Domain.Customers;

public class Customer : AuditableEntity
{
  public string Name { get; private set; }
  public string? Email { get; private set; }
  public string PhoneNumber { get; private set; }

  private readonly List<Vehicle> _vehicles = new();
  public IEnumerable<Vehicle> Vehicles => _vehicles.AsReadOnly();

  private Customer(){}
  private Customer(Guid id,string name, string? email, string phoneNumber, List<Vehicle> vehicles) : base(id)
  {
    Name = name;
    Email = email;
    PhoneNumber = phoneNumber;
    _vehicles = vehicles;
  }

  public static Result<Customer> Create(Guid id, string name, string? email, string phoneNumber, List<Vehicle> vehicles)
  {
    if(id == Guid.Empty)
    {
      return CustomerErrors.IdRequired;
    }
    if(string.IsNullOrWhiteSpace(name))
    {
      return CustomerErrors.NameRequired;
    }
    if(string.IsNullOrWhiteSpace(phoneNumber)  || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
    {
      return CustomerErrors.InvalidPhoneNumber;
    }
    if(string.IsNullOrWhiteSpace(email))
    {
      return CustomerErrors.EmailRequired;
    }
    try
    {
      //trying to parse and validate an email address according to RFC 2822 standards.
      // If the email is invalid, it throws a FormatException
      _ = new MailAddress(email);
    }
    catch
    {
      return CustomerErrors.InvalidEmail;
    }
    return new Customer(Guid.NewGuid(), name, email, phoneNumber, vehicles);
  }

  public Result<Updated> Update(string name, string? email, string phoneNumber)
  {
    if(string.IsNullOrWhiteSpace(name))
    {
      return CustomerErrors.NameRequired;
    }
    if(string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
    {
      return CustomerErrors.InvalidPhoneNumber;
    }
    if(string.IsNullOrWhiteSpace(email))
    {
      return CustomerErrors.EmailRequired;
    }
    try
    {
      //trying to parse and validate an email address according to RFC 2822 standards.
      // If the email is invalid, it throws a FormatException
      _ = new MailAddress(email);
    }
    catch
    {
      return CustomerErrors.InvalidEmail;
    }

    Name = name;
    Email = email;
    PhoneNumber = phoneNumber;

    return Result.Updated;
  }

  public Result<Updated> UpdateParts(List<Vehicle> incomingVehicles)
  {
    _vehicles.RemoveAll(v => !incomingVehicles.Any(iv => iv.Id == v.Id));
    // _vehicles.RemoveAll(v => incomingVehicles.All(iv => iv.Id != v.Id));
    foreach (var v in incomingVehicles)
    {
      var existingVehicle = _vehicles.FirstOrDefault(e => v.Id == e.Id);
      if(existingVehicle is null)
        _vehicles.Add(v);
      else
      {
        var updateVehicleResult = existingVehicle.Update(v.Make, v.Model, v.Year, v.LicensePlate);
        if(updateVehicleResult.IsFailure)
          return updateVehicleResult.Errors!;
      }
    }
    return Result.Updated;
  }
}