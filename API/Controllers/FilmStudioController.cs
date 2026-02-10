using System;
using API.Auth;
using API.Contracts.Requests;
using API.Contracts.Responses.Filmstudios;
using API.Data;
using API.Models.FilmStudio;
using API.Models.User;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;


[ApiController]
[Route("api/filmstudio")]
public class FilmStudioController : ControllerBase
{
    private readonly FilmStudionDbContext _db;
    public FilmStudioController(FilmStudionDbContext db) => _db = db;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterFilmStudioRequest body) 
    {
        if (body is null) return BadRequest();

        if (string.IsNullOrWhiteSpace(body.Name) ||
            string.IsNullOrWhiteSpace(body.City) ||
            string.IsNullOrWhiteSpace(body.Username) ||
            string.IsNullOrWhiteSpace(body.Password)
        )
        {
            return BadRequest("Name, city, username and password are required");
        }

        var normalized = body.Username.Trim().ToLowerInvariant();
        var usernameExists = await _db.Users.AnyAsync(u => u.Username == body.Username);
        if (usernameExists) return Conflict("Username already exists");

        var studio = new FilmStudio
        {
            Name = body.Name.Trim(),
            City = body.City.Trim()
        };

        var user = new UserEntity
        {
            Username = body.Username.Trim(),
            Password = body.Password,
            Role = "filmstudio",
            FilmStudio = studio
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var dto = new FilmStudioAdminDto(
            studio.FilmStudioId,
            studio.Name,
            studio.City,
            new List<FilmCopyDto>()
        );

        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var session = GetSession();
        var isAdmin = session?.Role.Equals("admin", StringComparison.OrdinalIgnoreCase) == true;
        var isSelf = session?.Role.Equals("filmstudio", StringComparison.OrdinalIgnoreCase) == true
                    && session.FilmStudioId == id;

        if (isAdmin || isSelf)
        {
            var studio = await _db.FilmStudios
                .Include(s => s.RentedFilmCopies)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.FilmStudioId == id);

            if (studio is null) return NotFound();

            var dto = new FilmStudioAdminDto(
                studio.FilmStudioId,
                studio.Name,
                studio.City,
                studio.RentedFilmCopies.Select(c =>
                    new FilmCopyDto(c.FilmCopyId, c.FilmId, c.RentedByFilmStudioId)
                ).ToList()
            );

            return Ok(dto);
        }

        var publicStudio = await _db.FilmStudios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.FilmStudioId == id);

        if (publicStudio is null) return NotFound();

        return Ok(new FilmStudioPublicDto(publicStudio.FilmStudioId, publicStudio.Name));

        
    }

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}
