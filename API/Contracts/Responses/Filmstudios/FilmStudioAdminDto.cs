namespace API.Contracts.Responses.Filmstudios;

public record class FilmStudioAdminDto(
    int FilmStudioId,
    string Name,
    string City,
    List<FilmCopyDto> RentedFilmCopies
);
