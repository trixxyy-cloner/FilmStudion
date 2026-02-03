using System;

namespace API.Interfaces;

public interface IFilm
{
    int FilmId {get; set;}
    string Title {get; set;}
    int ReleaseYear {get; set;}
    List<IFilmCopy> FilmCopies {get; set;}
}
