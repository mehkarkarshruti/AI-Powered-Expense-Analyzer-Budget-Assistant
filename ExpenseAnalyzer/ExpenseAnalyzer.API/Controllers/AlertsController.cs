using ExpenseAnalyzer.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseAnalyzer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController(IAlertService alertService) : ControllerBase
{
    private int? GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadAlerts()
    {
        int? userId = GetUserId();
        if (userId == null) return Unauthorized();

        var alerts = await alertService.GetUnreadAlertsAsync(userId.Value);
        return Ok(alerts);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAlertAsRead(int id)
    {
        int? userId = GetUserId();
        if (userId == null) return Unauthorized();

        await alertService.MarkAlertAsReadAsync(id, userId.Value);
        return NoContent();
    }
}
