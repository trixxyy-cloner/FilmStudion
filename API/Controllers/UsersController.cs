using System;
using System.Security.Cryptography.X509Certificates;
using API.Auth;
using API.Contracts.Requests;
using API.Contracts.Responses.Users;
using API.Data;
using API.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly FilmStudionDbContext _db;
    public UsersController(FilmStudionDbContext db) => _db = db;
    
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest body)
    {
        if (body is null) return BadRequest();

        if (!body.IsAdmin) return BadRequest("Only admin registration is allowed here.");

        if (string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
            return BadRequest("Username and password are required");

        var usernameExists = await _db.Users.AnyAsync(u => u.Username == body.Username);
        if (usernameExists) return Conflict("Username already exists");

        var user = new UserEntity
        {
            Username = body.Username.Trim(),
            Password = body.Password,
            Role = "admin",
            FilmStudioId = null,
            AuthToken = null
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var dto = new AdminRegisteredUserDto(user.UserId, user.Username, user.Role);
        return Ok(dto);

    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] UserAuthenticateRequest body)
    {
        if (body is null) return BadRequest();

        if (string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
            return BadRequest("Username and password are required");

        var user = await _db.Users
            .Include(u => u.FilmStudio)
            .FirstOrDefaultAsync(u => u.Username == body.Username);

        if (user is null) return Unauthorized();

        if (user.Password != body.Password) return Unauthorized();

        var token = Guid.NewGuid().ToString("N");
        user.AuthToken = token;
        await _db.SaveChangesAsync();

        var role = user.Role?.ToLowerInvariant() ?? "";

        FilmStudioSummaryDto? filmStudioSummary = null;
        int? filmStudioId = null;

        if (role == "filmstudio")
        {
            filmStudioId = user.FilmStudioId;

            if (user.FilmStudio != null)
            {
                filmStudioSummary = new FilmStudioSummaryDto(
                    user.FilmStudio.FilmStudioId,
                    user.FilmStudio.Name,
                    user.FilmStudio.City
                );
            }
        }

        var userDto = new AuthenticatedUserDto(
            user.UserId,
            user.Username,
            role,
            filmStudioId,
            filmStudioSummary
        );

        return Ok(new AuthenticateResponseDto(token, userDto));
    }

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}
