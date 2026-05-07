using Microsoft.AspNetCore.Mvc;
using CustomizeAuthAppJWT.Filters;
using System.Security.Claims;

namespace CustomizeAuthAppJWT.Controllers;

[ApiController]
[Route("api")]
public class DataController : ControllerBase
{
    // GET /api/public
    [HttpGet("public")]
    public IActionResult Public() => Ok(new { message = "This endpoint is public." });

    // GET /api/user-data - must be role User or Admin
    [HttpGet("user-data")]
    public IActionResult UserData()
    {
        var user = HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
            return Unauthorized();

        if (!user.IsInRole("User") && !user.IsInRole("Admin"))
            return Forbid();

        var name = user.FindFirstValue(ClaimTypes.Name) ?? user.Identity?.Name;
        var role = user.FindFirstValue(ClaimTypes.Role) ?? "Unknown";

        return Ok(new { message = "Protected user data", user = name, role });
    }

    // GET /api/admin-only - enforced by custom filter
    [HttpGet("admin-only")]
    [AdminOnly]
    public IActionResult AdminOnly()
    {
        var user = HttpContext.User;
        var name = user.FindFirstValue(ClaimTypes.Name) ?? user.Identity?.Name;
        return Ok(new { message = "Welcome, admin!", user = name });
    }
}