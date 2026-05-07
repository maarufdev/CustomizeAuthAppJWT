using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomizeAuthAppJWT.Data;
using CustomizeAuthAppJWT.DTOs;
using CustomizeAuthAppJWT.Models;
using CustomizeAuthAppJWT.Services;

namespace CustomizeAuthAppJWT.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password are required.");

        var exists = await _db.Users.AnyAsync(u => u.Username == req.Username);
        if (exists) return Conflict("Username already taken.");

        // Hash password using BCrypt (explicit)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var user = new User
        {
            Username = req.Username,
            PasswordHash = passwordHash,
            Role = req.Role == "Admin" ? "Admin" : "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.Username, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user == null) return Unauthorized("Invalid credentials");

        // Verify password explicitly
        var valid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!valid) return Unauthorized("Invalid credentials");

        var token = _tokenService.GenerateToken(user, out var expiresAt);

        return Ok(new DTOs.AuthResponse { Token = token, ExpiresAt = expiresAt });
    }
}