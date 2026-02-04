using System;
using API.Interfaces;

namespace API.Contracts.Requests;

public class UserAuthenticateRequest : IUserAuthenticate
{
    public string Username {get; set;} = "";
    public string Password {get; set;} = "";
}
