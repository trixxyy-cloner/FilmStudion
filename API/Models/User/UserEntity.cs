using System;

namespace API.Models.User;

public class UserEntity
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public string Role { get; set; } = ""; 

    public int? FilmStudioId { get; set; }
    public Models.FilmStudio.FilmStudio? FilmStudio { get; set; }

    public string? AuthToken { get; set; }
}
