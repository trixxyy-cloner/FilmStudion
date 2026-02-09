using System;
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
        // Skapa admin om body.IsAdmin == true
        // Returnera endast AdminRegisteredUserDto
        
        return Ok();
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] UserAuthenticateRequest body)
    {
        // hitta user, verifiera password
        // skapa token: Guid.NewGuid().ToString("N")
        // spara i user.AuthToken
        // returnera AuthenticateReponseDto (Token + safe user)
        
        return Ok();
    }

    private AuthSession? GetSession()
        => HttpContext.Items.TryGetValue("session", out var s) ? s as AuthSession : null;
}
