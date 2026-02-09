using System;
using API.Interfaces;
using API.Models.FilmCopy;

namespace API.Models.FilmStudio;

public class FilmStudio : IFilmStudio
{
    public int FilmStudioId {get; set;}
    public string Name {get; set;} = "";
    public string City {get; set;} = "";

    public List<FilmCopyEntity> RentedFilmCopies { get; set; } = new();
    List<IFilmCopy> IFilmStudio.RentedFilmCopies
    {
        get => RentedFilmCopies.Cast<IFilmCopy>().ToList();
        set => RentedFilmCopies = value.Cast<FilmCopyEntity>().ToList();
    }
}
