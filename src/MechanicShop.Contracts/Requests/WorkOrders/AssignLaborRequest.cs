using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class AssignLaborRequest
{
  [Required(ErrorMessage = "Labor id is required")]
  public Guid LaborId { get; set; }
}