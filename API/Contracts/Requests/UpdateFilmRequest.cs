using System;

namespace API.Contracts.Requests;

public class UpdateFilmRequest
{
    public string? Title {get; set;}
    public int? ReleaseYear {get; set;}
    
    // Använda om man vill ändra antal exemplar
    public int? NumberOfCopies {get; set;}
}
