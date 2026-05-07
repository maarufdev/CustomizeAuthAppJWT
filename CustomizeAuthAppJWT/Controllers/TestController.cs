using Microsoft.AspNetCore.Mvc;
using CustomizeAuthAppJWT.Filters;
using CustomizeAuthAppJWT.Data;
using System.Security.Claims;

namespace CustomizeAuthAppJWT.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _db;

    public TestController(AppDbContext db)
    {
        _db = db;
    }

    // GET /test/ping - public
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { message = "pong" });

    // GET /test/protected - requires authentication
    [HttpGet("protected")]
    public IActionResult ProtectedEndpoint()
    {
        var user = HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
            return Unauthorized(new { message = "Authentication required" });

        var name = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.Name) ?? "unknown";
        var role = user.FindFirstValue(ClaimTypes.Role) ?? "unknown";

        return Ok(new { message = "You are authenticated", user = name, role });
    }

    // GET /test/role-user - requires User or Admin role
    [HttpGet("role-user")]
    public IActionResult RequireUserRole()
    {
        var user = HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
            return Unauthorized();

        if (!user.IsInRole("User") && !user.IsInRole("Admin"))
            return Forbid();

        return Ok(new { message = "Access granted for User or Admin" });
    }

    // GET /test/role-admin - requires Admin role via custom filter
    [HttpGet("role-admin")]
    [AdminOnly]
    public IActionResult RequireAdminRole()
    {
        return Ok(new { message = "Access granted for Admin" });
    }

    // GET /test/users - admin-only: list users
    [HttpGet("users")]
    [AdminOnly]
    public IActionResult ListUsers()
    {
        var users = _db.Users.Select(u => new { u.Id, u.Username, u.Role }).ToList();
        return Ok(users);
    }
}