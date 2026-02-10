using System;
using API.Models.Film;
using API.Models.FilmCopy;
using API.Models.FilmStudio;
using API.Models.User;
using Microsoft.EntityFrameworkCore;


namespace API.Data;

public class FilmStudionDbContext : DbContext
{
    public FilmStudionDbContext(DbContextOptions<FilmStudionDbContext> options) : base(options) { }

    public DbSet<Film> Films => Set<Film>();
    public DbSet<FilmCopyEntity> FilmCopies => Set<FilmCopyEntity>();
    public DbSet<FilmStudio> FilmStudios => Set<FilmStudio>();
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<API.Models.FilmCopy.FilmCopyEntity>()
        .HasKey(x => x.FilmCopyId);

        modelBuilder.Entity<API.Models.User.UserEntity>()
        .HasKey(x => x.UserId);

        base.OnModelCreating(modelBuilder);
    }
}
