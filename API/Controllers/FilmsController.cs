using System;
using API.Auth;
using API.Contracts.Requests;
using API.Contracts.Responses.Films;
using API.Data;
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
            return Ok();
        }

        return Ok();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var session = GetSession();

        if (session is null)
        {
            return Ok();
        }

        return Ok();
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFilmRequest body)
    {
        if (!IsAdmin()) return Unauthorized();

        return Ok();
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
