using Microsoft.AspNetCore.Mvc;
using CustomizeAuthAppJWT.Filters;

namespace CustomizeAuthAppJWT.Controllers;

[ApiController]
[Route("permissions")]
public class PermissionController : ControllerBase
{
    [HttpGet("read-reports")]
    [Permission("ReadReports")]
    public IActionResult ReadReports() => Ok(new { message = "You can read reports" });

    [HttpPost("write-reports")]
    [Permission("WriteReports")]
    public IActionResult WriteReports() => Ok(new { message = "You can write reports" });

    [HttpPost("manage-users")]
    [Permission("ManageUsers")]
    public IActionResult ManageUsers() => Ok(new { message = "You can manage users" });
}