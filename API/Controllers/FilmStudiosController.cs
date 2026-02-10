using System;
using API.Auth;
using API.Contracts.Responses.Filmstudios;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers;

[ApiController]
[Route("api/filmstudios")]
public class FilmStudiosController : ControllerBase
{
    private readonly FilmStudionDbContext _db;
    public FilmStudiosController(FilmStudionDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var session = GetSession();
        var isAdmin = session?.Role.Equals("admin", StringComparison.OrdinalIgnoreCase) == true;

        if (isAdmin)
        {
            var studios = await _db.FilmStudios
                .Include(s => s.RentedFilmCopies)
                .AsNoTracking()
                .ToListAsync();

            var adminDtos = studios.Select(s => new FilmStudioAdminDto(
                s.FilmStudioId,
                s.Name,
                s.City,
                s.RentedFilmCopies.Select(c => new FilmCopyDto(c.FilmCopyId, c.FilmId, c.RentedByFilmStudioId)).ToList()
            )).ToList();

            return Ok(adminDtos);
        }

        var publicStudios = await _db.FilmStudios
            .AsNoTracking()
            .ToListAsync();

        var publicDtos = publicStudios
            .Select(s => new FilmStudioPublicDto(s.FilmStudioId, s.Name))
            .ToList();

        return Ok(publicDtos);
    }

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}
