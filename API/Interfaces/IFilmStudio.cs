using System;

namespace API.Interfaces;

public interface IFilmStudio
{
    int FilmStudioId {get; set;}
    string Name {get; set;}
    string City {get; set;}
    List<IFilmCopy> RentedFilmCopies {get; set;}
}
