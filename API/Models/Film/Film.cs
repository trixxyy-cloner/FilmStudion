using System;
using API.Interfaces;
using API.Models.FilmCopy;

namespace API.Models.Film;

public class Film : IFilm
{
    public int FilmId {get; set;}
    public string Title {get; set;} = "";
    public int ReleaseYear {get; set;}
    public List<FilmCopyEntity> FilmCopies { get; set; } = new();
    List<IFilmCopy> IFilm.FilmCopies
    {
        get => FilmCopies.Cast<IFilmCopy>().ToList();
        set => FilmCopies = value.Cast<FilmCopyEntity>().ToList();
    }
}
