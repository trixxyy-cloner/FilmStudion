using System;
using API.Interfaces;

namespace API.Contracts.Requests;

public class UserRegisterRequest : IUserRegister
{
    public string Username {get; set;} = "";
    public string Password {get; set;} = "";
    public bool IsAdmin {get; set;}
}
