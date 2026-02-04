namespace API.Contracts.Responses.Films;

public record class FilmAuthDto(
    int FilmId,
    string Title,
    int ReleaseYear,
    List<FilmCopyDto> FilmCopies
);
