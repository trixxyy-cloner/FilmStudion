using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using API.Data;
using API.Auth;
using API.Models.Film;
using API.Models.FilmCopy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthorization();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// InMemory DB: data finns så länge API:t kör (försvinner vid omstart)
builder.Services.AddDbContext<FilmStudionDbContext>(options =>
    options.UseInMemoryDatabase("FilmStudionDb"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FilmStudionDbContext>();

    // Seed: lägg in filmer om de saknas + se till att de har minst X kopior.
    EnsureFilm(db, title: "The Matrix",     releaseYear: 1999, minCopies: 3);
    EnsureFilm(db, title: "Inception",      releaseYear: 2010, minCopies: 2);
    EnsureFilm(db, title: "Interstellar",   releaseYear: 2014, minCopies: 6);
    EnsureFilm(db, title: "The Dark Knight",releaseYear: 2008, minCopies: 2);
    EnsureFilm(db, title: "Pulp Fiction",   releaseYear: 1994, minCopies: 7);
    EnsureFilm(db, title: "Parasite",       releaseYear: 2019, minCopies: 2);
    EnsureFilm(db, title: "Fight Club",     releaseYear: 1999, minCopies: 8);
    EnsureFilm(db, title: "The Godfather",  releaseYear: 1972, minCopies: 9);
    EnsureFilm(db, title: "Gladiator",      releaseYear: 2000, minCopies: 3);
    EnsureFilm(db, title: "Whiplash",       releaseYear: 2014, minCopies: 5);
    EnsureFilm(db, title: "Se7en",          releaseYear: 1995, minCopies: 3);
    EnsureFilm(db, title: "Bad Boys",       releaseYear: 1995, minCopies: 6);

    db.SaveChanges();
}

static void EnsureFilm(FilmStudionDbContext db, string title, int releaseYear, int minCopies)
{
    var film = db.Films
        .Include(f => f.FilmCopies)
        .FirstOrDefault(f => f.Title == title);

    if (film is null)
    {
        film = new Film
        {
            Title = title,
            ReleaseYear = releaseYear,
        };

        for (var i = 0; i < minCopies; i++)
            film.FilmCopies.Add(new FilmCopyEntity());

        db.Films.Add(film);
        return;
    }

    film.ReleaseYear = releaseYear;

    while (film.FilmCopies.Count < minCopies)
        film.FilmCopies.Add(new FilmCopyEntity());
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("frontend");

app.UseMiddleware<AuthenticationMiddleware>();

app.MapControllers();

app.Run();
