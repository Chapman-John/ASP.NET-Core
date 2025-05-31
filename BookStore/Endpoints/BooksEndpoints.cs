using BookStore.Data;
using BookStore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Dtos;

public static class BooksEndpoints
{
    const string GetBookEndpointName = "GetBook";

    public static RouteGroupBuilder MapBooksEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("books")
            .WithParameterValidation()
            .WithTags("Books");

        // GET all books
        group.MapGet("/", async (BookStoreContext context) =>
        {
            var books = await context.Books
                .Include(b => b.Genre)
                .Select(b => new BookDto(
                    b.Id,
                    b.Name,
                    b.Genre!.Name,
                    b.Price,
                    b.ReleaseDate))
                .ToListAsync();
            
            return Results.Ok(books);
        })
        .WithSummary("Get all books")
        .WithDescription("Retrieve a list of all books in the store");

        // GET one book
        group.MapGet("/{id}", async (int id, BookStoreContext context) =>
        {
            var book = await context.Books
                .Include(b => b.Genre)
                .Where(b => b.Id == id)
                .Select(b => new BookDto(
                    b.Id,
                    b.Name,
                    b.Genre!.Name,
                    b.Price,
                    b.ReleaseDate))
                .FirstOrDefaultAsync();

            return book is null ? Results.NotFound() : Results.Ok(book);
        })
        .WithName(GetBookEndpointName)
        .WithSummary("Get book by ID")
        .WithDescription("Retrieve a specific book by its ID");

        // POST books
        group.MapPost("/", async (CreateBookDto newBook, BookStoreContext context) =>
        {
            // Find genre by name
            var genre = await context.Genres
                .FirstOrDefaultAsync(g => g.Name.ToLower() == newBook.Genre.ToLower());
            
            if (genre is null)
            {
                return Results.BadRequest($"Genre '{newBook.Genre}' not found");
            }

            var book = new Book
            {
                Name = newBook.Name,
                GenreId = genre.Id,
                Price = newBook.Price,
                ReleaseDate = newBook.ReleaseDate
            };

            context.Books.Add(book);
            await context.SaveChangesAsync();

            var bookDto = new BookDto(
                book.Id,
                book.Name,
                genre.Name,
                book.Price,
                book.ReleaseDate);

            return Results.CreatedAtRoute(GetBookEndpointName, new { id = book.Id }, bookDto);
        })
        .WithSummary("Create new book")
        .WithDescription("Add a new book to the store");

        // PUT (update) one book
        group.MapPut("/{id}", async (int id, UpdateBookDto updatedBook, BookStoreContext context) =>
        {
            var book = await context.Books.FindAsync(id);
            
            if (book is null)
            {
                return Results.NotFound();
            }

            // Find genre by name
            var genre = await context.Genres
                .FirstOrDefaultAsync(g => g.Name.ToLower() == updatedBook.Genre.ToLower());
            
            if (genre is null)
            {
                return Results.BadRequest($"Genre '{updatedBook.Genre}' not found");
            }

            book.Name = updatedBook.Name;
            book.GenreId = genre.Id;
            book.Price = updatedBook.Price;
            book.ReleaseDate = updatedBook.ReleaseDate;

            await context.SaveChangesAsync();
            
            return Results.NoContent();
        })
        .WithSummary("Update book")
        .WithDescription("Update an existing book's details");

        // DELETE one book
        group.MapDelete("/{id}", async (int id, BookStoreContext context) =>
        {
            var book = await context.Books.FindAsync(id);
            
            if (book is null)
            {
                return Results.NotFound();
            }

            context.Books.Remove(book);
            await context.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithSummary("Delete book")
        .WithDescription("Remove a book from the store");

        return group;
    }
}
