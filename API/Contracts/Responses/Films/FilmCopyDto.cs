namespace API.Contracts.Responses.Films;

public record class FilmCopyDto(
    int FilmCopyId,
    int FilmId,
    int? RentedByFilmStudioId
);