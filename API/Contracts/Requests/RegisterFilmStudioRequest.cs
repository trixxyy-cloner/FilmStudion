using System;
using API.Interfaces;

namespace API.Contracts.Requests;

public class RegisterFilmStudioRequest : IRegisterFilmStudio
{
    public string Name {get; set;} = "";
    public string City {get; set;} = "";
    public string Username {get; set;} = "";
    public string Password {get; set;} = "";
}
