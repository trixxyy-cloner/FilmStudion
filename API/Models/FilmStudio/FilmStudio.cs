using System;
using API.Interfaces;

namespace API.Models.FilmStudio;

public class FilmStudio : IFilmStudio
{
    public int FilmStudioId {get; set;}
    public string Name {get; set;} = "";
    public string City {get; set;} = "";

    public List<IFilmCopy> RentedFilmCopies {get; set;} = new();
}
