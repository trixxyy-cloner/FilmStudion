using System;
using API.Auth;
using API.Contracts.Requests;
using API.Contracts.Responses.Films;
using API.Data;
using API.Models.Film;
using API.Models.FilmCopy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/films")]
public class FilmsController : ControllerBase
{
    private readonly FilmStudionDbContext _db;
    public FilmsController(FilmStudionDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var session = GetSession();

        if (session is null)
        {
            var films = await _db.Films.AsNoTracking().ToListAsync();
            return Ok(films.Select(f => new FilmPublicDto(f.FilmId, f.Title, f.ReleaseYear)).ToList());
        }

        var authFilms = await _db.Films
            .Include(f => f.FilmCopies)
            .AsNoTracking()
            .ToListAsync();

        return Ok(authFilms.Select(f => new FilmAuthDto(
            f.FilmId,
            f.Title,
            f.ReleaseYear,
            f.FilmCopies.Select(c => new FilmCopyDto(c.FilmCopyId, c.FilmId, c.RentedByFilmStudioId)).ToList()
        )).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var session = GetSession();

        if (session is null)
        {
            var film = await _db.Films.AsNoTracking().FirstOrDefaultAsync(f => f.FilmId == id);
            if (film is null) return NotFound();
            return Ok(new FilmPublicDto(film.FilmId, film.Title, film.ReleaseYear));
        }

        var authFilm = await _db.Films
            .Include(f => f.FilmCopies)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FilmId == id);

        if (authFilm is null) return NotFound();

        return Ok(new FilmAuthDto(
            authFilm.FilmId,
            authFilm.Title,
            authFilm.ReleaseYear,
            authFilm.FilmCopies.Select(c => new FilmCopyDto(c.FilmCopyId, c.FilmId, c.RentedByFilmStudioId)).ToList()
        ));
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFilmRequest body)
    {
        if (!IsAdmin()) return Unauthorized();

        if (body is null) return BadRequest();
        if (string.IsNullOrWhiteSpace(body.Title)) return BadRequest("Title is required");
        if (body.NumberOfCopies < 0) return BadRequest("Number of copies must be >= 0");

        var film = new Film
        {
            Title = body.Title.Trim(),
            ReleaseYear = body.ReleaseYear
        };

        for (var i = 0; i < body.NumberOfCopies; i++)
            film.FilmCopies.Add(new FilmCopyEntity());

        _db.Films.Add(film);
        await _db.SaveChangesAsync();

        var dto = new FilmAuthDto(
            film.FilmId,
            film.Title,
            film.ReleaseYear,
            film.FilmCopies.Select(c => new FilmCopyDto(c.FilmCopyId, c.FilmId, c.RentedByFilmStudioId)).ToList()
        );

        return Ok(dto);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateFilmRequest body) 
    {
        if (GetSession() is null) return Unauthorized();
        if (!IsAdmin()) return Unauthorized();

        return Ok();
    }

    [HttpPost("rent")]
    public async Task<IActionResult> Rent([FromQuery] int id, [FromQuery] int studioId)
    {
        var session = GetSession();
        if (session is null) return Unauthorized();
        if (!session.Role.Equals("filmstudio", StringComparison.OrdinalIgnoreCase)) return Unauthorized();
        if (session.FilmStudioId != studioId) return Unauthorized();

        return Ok();
    }

    [HttpPost("return")]
    public async Task<IActionResult> Return([FromQuery] int id, [FromQuery] int studioid)
    {
        var session = GetSession();
        if (session is null) return Unauthorized();
        if (!session.Role.Equals("filmstudio", StringComparison.OrdinalIgnoreCase)) return Unauthorized();
        if (session.FilmStudioId != studioid) return Unauthorized();

        
        return Ok();
    }

    private bool IsAdmin()
        => GetSession()?.Role.Equals("admin", StringComparison.OrdinalIgnoreCase) == true;

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}
