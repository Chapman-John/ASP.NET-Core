namespace BookStore.Dtos;

public static class BooksEndpoints
{

    const string GetBookEndpointName = "GetBook";

    private static readonly List<BookDto> books = [
        new (
        1,
        "Antifragile",
        "nonfiction",
        40,
        new DateOnly(2012, 1, 01)),
    new (
        2,
        "Out of Control",
        "nonfiction",
        20.11M,
        new DateOnly(1991, 1, 01)),
    new (
        3,
        "The One Thing",
        "nonfiction",
        20,
        new DateOnly(2014, 1, 01))
    ];

    public static RouteGroupBuilder MapBooksEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("books")
            .WithParameterValidation();

        //GET all books
        group.MapGet("/", () => books);

        // GET one book
        group.MapGet("/{id}", (int id) =>
        {
            BookDto? book = books.Find(book => book.Id == id);

            return book is null ? Results.NotFound() : Results.Ok(book);
        })
            .WithName(GetBookEndpointName);

        // POST books
        group.MapPost("/", (CreateBookDto newBook) =>
        {
            BookDto book = new(
                books.Count + 1,
                newBook.Name,
                newBook.Genre,
                newBook.Price,
                newBook.ReleaseDate);

            books.Add(book);

            return Results.CreatedAtRoute(GetBookEndpointName, new { id = book.Id }, book);
        });


        // UPDATE one book
        group.MapPut("/{id}", (int id, UpdateBookDto updatedBook) =>
        {

            var index = books.FindIndex(book => book.Id == id);

            if (index == -1)
            {
                return Results.NotFound();
            }

            books[index] = new BookDto(
                id,
                updatedBook.Name,
                updatedBook.Genre,
                updatedBook.Price,
                updatedBook.ReleaseDate
                );
            return Results.NoContent();
        });

        // DELETE one book
        group.MapDelete("/{id}", (int id) =>
        {

            books.RemoveAll(book => book.Id == id);

            return Results.NoContent();
        });
        return group;
    }
}