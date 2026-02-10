using System;

namespace API.Interfaces;

public interface IUserAuthenticate
{
    string Username { get; set; }
    string Password { get; set; }
}
