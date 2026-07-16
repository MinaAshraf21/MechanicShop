using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Domain.Employees;

namespace MechanicShop.Application.Features.Labor.Mappers;

public static class LaborMappers
{
  public static LaborDto ToDto(this Employee employee)
  {
    return new LaborDto(employee.Id, employee.FirstName, employee.LastName);
  }
  public static List<LaborDto> ToDtos(this IEnumerable<Employee> employees)
  {
    return employees.Select(e => e.ToDto()).ToList();
  }
}