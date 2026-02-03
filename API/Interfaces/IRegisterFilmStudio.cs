using System;

namespace API.Interfaces;

public interface IRegisterFilmStudio
{
    string Name {get; set;}
    string City {get; set;}
    string Username {get; set;}
    string Password {get; set;}
}
