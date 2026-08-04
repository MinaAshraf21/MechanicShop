using System.Security.Claims;
using MechanicShop.Application.Abstractions;
using MechanicShop.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Identity.Policies;

public class LaborAssignedRequirement : IAuthorizationRequirement;

public class LaborAssignedHandler(IAppDbContext dbContext, IHttpContextAccessor httpContextAccessor) 
  : AuthorizationHandler<LaborAssignedRequirement>
{
  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, LaborAssignedRequirement requirement)
  {
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if(string.IsNullOrEmpty(userId))
    {
      context.Fail();
      return;
    }
    if (context.User.IsInRole(nameof(Role.Manager)))
    {
      context.Succeed(requirement);
      return;
    }

    // extract workOrderId from the route
    var workOrderIdString = httpContextAccessor.HttpContext?.Request.RouteValues["WorkOrderId"]?.ToString();
    if (!Guid.TryParse(workOrderIdString, out var workOrderId))
    {
      context.Fail();
      return;
    }

    var isAssigned = await dbContext.WorkOrders
                            .AnyAsync(w =>
                                        w.Id == workOrderId &&
                                        w.LaborId == Guid.Parse(userId)
                                      );

    if (isAssigned)
    {
      context.Succeed(requirement);
      return;
    }
    context.Fail();
  }
}