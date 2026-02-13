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

    // Om man inte är inloggad: returnera FilmPublicDto (utan FilmCopies).
    // Om man är inloggad: returnera FilmAuthDto (med FilmCopies).
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

    // Admin-only: skapar film och rätt antal copies.
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
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateFilmFullRequest body) 
    {
        if (GetSession() is null) return Unauthorized();
        if (!IsAdmin()) return Unauthorized();

        var film = await _db.Films
            .Include(f => f.FilmCopies)
            .FirstOrDefaultAsync(f => f.FilmId == id);

        if (film is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(body.Title))
            film.Title = body.Title.Trim();

        film.ReleaseYear = body.ReleaseYear;

        var desired = body.FilmCopies?.Count ?? 0;
        if (desired < 0) return BadRequest("Number of copies must be >= 0");

        var current = film.FilmCopies.Count;
        if (desired > current)
        {
            for (var i = 0; i < desired - current; i++)
                film.FilmCopies.Add(new FilmCopyEntity());
        }
        else if (desired < current)
        {
            var removable = film.FilmCopies
                .Where(c => c.RentedByFilmStudioId == null)
                .OrderByDescending(c => c.FilmCopyId)
                .ToList();

            var toRemoveCount = current - desired;
            if (removable.Count < toRemoveCount)
                return Conflict("Not enough available copies to remove");

            _db.FilmCopies.RemoveRange(removable.Take(toRemoveCount));
        }

        await _db.SaveChangesAsync();

        var dto = new FilmAuthDto(
            film.FilmId,
            film.Title,
            film.ReleaseYear,
            film.FilmCopies.Select(c => new FilmCopyDto(c.FilmCopyId, c.FilmId, c.RentedByFilmStudioId)).ToList()
        );

        return Ok(dto);
    }

    // Filmstudio-only: hyr ett exemplar om det finns ledigt.
    [HttpPost("rent")]
    public async Task<IActionResult> Rent([FromQuery(Name = "id")] int id, [FromQuery(Name = "studioid")] int studioId)
    {
        var session = GetSession();
        if (session is null) return Unauthorized();

        if (!session.Role.Equals("filmstudio", StringComparison.OrdinalIgnoreCase)) return Unauthorized();
        if (session.FilmStudioId is null || session.FilmStudioId.Value != studioId) return Unauthorized();

        var film = await _db.Films
            .Include(f => f.FilmCopies)
            .FirstOrDefaultAsync(f => f.FilmId == id);

        if (film is null) return Conflict("Film not found.");

        if (film.FilmCopies.Any(c => c.RentedByFilmStudioId == studioId))
            return StatusCode(403, "Studio already rents a copy of this film.");

        var freeCopy = film.FilmCopies.FirstOrDefault(c => c.RentedByFilmStudioId == null);
        if (freeCopy is null) return Conflict("No available copies.");

        freeCopy.RentedByFilmStudioId = studioId;
        await _db.SaveChangesAsync();

        return Ok();
    }

    // Filmstudio-only: lämnar tillbaka ett hyrt exemplar.
    [HttpPost("return")]
    public async Task<IActionResult> Return([FromQuery] int id, [FromQuery(Name = "studioid")] int studioId)
    {
        var session = GetSession();
        if (session is null) return Unauthorized();

        if (!session.Role.Equals("filmstudio", StringComparison.OrdinalIgnoreCase)) return Unauthorized();
        if (session.FilmStudioId is null || session.FilmStudioId.Value != studioId) return Unauthorized();

        var filmExists = await _db.Films.AnyAsync(f => f.FilmId == id);
        if (!filmExists) return Conflict("Film not found.");

        var rentedCopy = await _db.FilmCopies
            .FirstOrDefaultAsync(c => c.FilmId == id && c.RentedByFilmStudioId == studioId);

        if (rentedCopy is null) return Conflict("No rented copy found for this studio.");

        rentedCopy.RentedByFilmStudioId = null;
        await _db.SaveChangesAsync();

        
        return Ok();
    }

    private bool IsAdmin()
        => GetSession()?.Role.Equals("admin", StringComparison.OrdinalIgnoreCase) == true;

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}
