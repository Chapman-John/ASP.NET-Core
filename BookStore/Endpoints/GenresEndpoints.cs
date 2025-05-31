using BookStore.Data;
using BookStore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Dtos;

public record class GenreDto(int Id, string Name);
public record class CreateGenreDto([Required][StringLength(30)] string Name);
public record class UpdateGenreDto([Required][StringLength(30)] string Name);

public static class GenresEndpoints
{
    const string GetGenreEndpointName = "GetGenre";

    public static RouteGroupBuilder MapGenresEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("genres")
            .WithParameterValidation()
            .WithTags("Genres");

        // GET all genres
        group.MapGet("/", async (BookStoreContext context) =>
        {
            var genres = await context.Genres
                .Select(g => new GenreDto(g.Id, g.Name))
                .ToListAsync();
            
            return Results.Ok(genres);
        })
        .WithSummary("Get all genres")
        .WithDescription("Retrieve a list of all available genres");

        // GET one genre
        group.MapGet("/{id}", async (int id, BookStoreContext context) =>
        {
            var genre = await context.Genres
                .Where(g => g.Id == id)
                .Select(g => new GenreDto(g.Id, g.Name))
                .FirstOrDefaultAsync();

            return genre is null ? Results.NotFound() : Results.Ok(genre);
        })
        .WithName(GetGenreEndpointName)
        .WithSummary("Get genre by ID")
        .WithDescription("Retrieve a specific genre by its ID");

        // POST genres
        group.MapPost("/", async (CreateGenreDto newGenre, BookStoreContext context) =>
        {
            // Check if genre already exists
            var existingGenre = await context.Genres
                .FirstOrDefaultAsync(g => g.Name.ToLower() == newGenre.Name.ToLower());
            
            if (existingGenre is not null)
            {
                return Results.Conflict($"Genre '{newGenre.Name}' already exists");
            }

            var genre = new Genre
            {
                Name = newGenre.Name
            };

            context.Genres.Add(genre);
            await context.SaveChangesAsync();

            var genreDto = new GenreDto(genre.Id, genre.Name);

            return Results.CreatedAtRoute(GetGenreEndpointName, new { id = genre.Id }, genreDto);
        })
        .WithSummary("Create new genre")
        .WithDescription("Add a new genre to the system");

        // PUT (update) one genre
        group.MapPut("/{id}", async (int id, UpdateGenreDto updatedGenre, BookStoreContext context) =>
        {
            var genre = await context.Genres.FindAsync(id);
            
            if (genre is null)
            {
                return Results.NotFound();
            }

            // Check if another genre with the same name exists
            var existingGenre = await context.Genres
                .FirstOrDefaultAsync(g => g.Name.ToLower() == updatedGenre.Name.ToLower() && g.Id != id);
            
            if (existingGenre is not null)
            {
                return Results.Conflict($"Genre '{updatedGenre.Name}' already exists");
            }

            genre.Name = updatedGenre.Name;
            await context.SaveChangesAsync();
            
            return Results.NoContent();
        })
        .WithSummary("Update genre")
        .WithDescription("Update an existing genre's name");

        // DELETE one genre
        group.MapDelete("/{id}", async (int id, BookStoreContext context) =>
        {
            var genre = await context.Genres.FindAsync(id);
            
            if (genre is null)
            {
                return Results.NotFound();
            }

            // Check if any books are using this genre
            var booksUsingGenre = await context.Books.AnyAsync(b => b.GenreId == id);
            
            if (booksUsingGenre)
            {
                return Results.BadRequest("Cannot delete genre that is being used by books");
            }

            context.Genres.Remove(genre);
            await context.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithSummary("Delete genre")
        .WithDescription("Remove a genre from the system (only if not used by any books)");

        return group;
    }
}
