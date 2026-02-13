using System;
using API.Interfaces;

namespace API.Contracts.Requests;

public class UpdateFilmFullRequest : IFilm
{
    public int FilmId { get; set; }
    public string Title { get; set; } = "";
    public int ReleaseYear { get; set; }
    public List<IFilmCopy> FilmCopies { get; set; } = new();
}
