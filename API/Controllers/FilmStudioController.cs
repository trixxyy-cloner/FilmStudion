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
        return Ok();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var session = GetSession();
        var isAdmin = session?.Role.Equals("filmstudio", StringComparison.OrdinalIgnoreCase) == true;
        var isSelf = session?.Role.Equals("filmstudio", StringComparison.OrdinalIgnoreCase) == true
                    && session.FilmStudioId == id;

        if (isAdmin || isSelf)
        {
            return Ok();
        }


        return Ok();
    }

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}
