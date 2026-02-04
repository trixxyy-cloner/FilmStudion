namespace API.Contracts.Responses.Filmstudios;

public record class FilmCopyDto(
    int FilmCopyId,
    int FilmId,
    int? RentedByFilmStudioId
);
