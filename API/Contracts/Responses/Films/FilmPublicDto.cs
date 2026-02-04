namespace API.Contracts.Responses.Films;

public record class FilmPublicDto(
    int FilmId,
    string Title,
    int ReleaseYear
);
