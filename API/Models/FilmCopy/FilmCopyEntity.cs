using System;
using API.Interfaces;

namespace API.Models.FilmCopy;

public class FilmCopyEntity : IFilmCopy
{
    public int FilmCopyId {get; set;}
    public int FilmId {get; set;}
    public Models.Film.Film? Film {get; set;}
    public int? RentedByFilmStudioId {get; set;}
    public Models.FilmStudio.FilmStudio? RentedByFilmStudio {get; set;}
}
