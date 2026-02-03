using System;
using API.Interfaces;

namespace API.Models.Film;

public class Film : IFilm
{
    public int FilmId {get; set;}
    public string Title {get; set;} = "";
    public int ReleaseYear {get; set;}
    public List<IFilmCopy> FilmCopies {get; set;} = new();
}
