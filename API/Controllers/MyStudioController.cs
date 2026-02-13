using System;
using API.Auth;
using API.Contracts.Responses.Filmstudios;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/mystudio")]
public class MyStudioController : ControllerBase
{
    private readonly FilmStudionDbContext _db;
    public MyStudioController(FilmStudionDbContext db) => _db = db;

    
    [HttpGet("rentals")]
    public async Task<IActionResult> GetRentals()
    {
        var session = GetSession();
        if (session is null) return Unauthorized();

        if (!session.Role.Equals("filmstudio", StringComparison.OrdinalIgnoreCase)) 
            return Unauthorized();

        var studioId = session.FilmStudioId;
        if (studioId is null) return Unauthorized();

        var rentals = await _db.FilmCopies
            .AsNoTracking()
            .Where(c => c.RentedByFilmStudioId == studioId)
            .ToListAsync();

        var dtos = rentals.Select(c => new FilmCopyDto(c.FilmCopyId, c.FilmId, c.RentedByFilmStudioId)).ToList();
        
        return Ok(dtos);
    }

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}