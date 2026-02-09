namespace API.Auth;

public record AuthSession(int UserId, string Username, string Role, int? FilmStudioId);
