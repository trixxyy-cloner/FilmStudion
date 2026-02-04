namespace API.Contracts.Responses.Users;

public record class AuthenticatedUserDto(
    int UserId,
    string Username,
    string Role,
    int? FilmStudioId,
    FilmStudioSummaryDto? FilmStudio
);
