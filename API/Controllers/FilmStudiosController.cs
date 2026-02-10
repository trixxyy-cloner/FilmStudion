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
            return Ok();
        }

        return Ok();
    }

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}
