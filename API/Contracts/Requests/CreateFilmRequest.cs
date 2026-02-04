using System;
using API.Interfaces;

namespace API.Contracts.Requests;

public class CreateFilmRequest : ICreateFilm
{
    public string Title {get; set;} = "";
    public int ReleaseYear {get; set;}
    public int NumberOfCopies {get; set;}
}
