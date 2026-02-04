namespace API.Contracts.Responses.Users;

public record class FilmStudioSummaryDto(
    int FilmStudioId,
    string Name,
    string City
);
