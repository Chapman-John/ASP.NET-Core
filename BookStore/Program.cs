using BookStore.Data;
using BookStore.Dtos;
using BookStore.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connString = "Data Source=BookStore.db";
builder.Services.AddSqlite<BookStoreContext>(connString);

// Add API Explorer and Swagger for documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure database is created and seeded
await EnsureDatabaseAsync(app);

app.MapBooksEndpoints();
app.MapGenresEndpoints();

app.Run();

// Database initialization method
async Task EnsureDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BookStoreContext>();
    
    await context.Database.EnsureCreatedAsync();
    
    // Seed data if database is empty
    if (!await context.Genres.AnyAsync())
    {
        var genres = new[]
        {
            new Genre { Name = "Fiction" },
            new Genre { Name = "Non-Fiction" },
            new Genre { Name = "Science Fiction" },
            new Genre { Name = "Biography" },
            new Genre { Name = "Self-Help" }
        };
        
        context.Genres.AddRange(genres);
        await context.SaveChangesAsync();
    }
    
    if (!await context.Books.AnyAsync())
    {
        var nonfictionGenre = await context.Genres.FirstAsync(g => g.Name == "Non-Fiction");
        
        var books = new[]
        {
            new Book 
            { 
                Name = "Antifragile", 
                GenreId = nonfictionGenre.Id, 
                Price = 40, 
                ReleaseDate = new DateOnly(2012, 1, 1) 
            },
            new Book 
            { 
                Name = "Out of Control", 
                GenreId = nonfictionGenre.Id, 
                Price = 20.11M, 
                ReleaseDate = new DateOnly(1991, 1, 1) 
            },
            new Book 
            { 
                Name = "The One Thing", 
                GenreId = nonfictionGenre.Id, 
                Price = 20, 
                ReleaseDate = new DateOnly(2014, 1, 1) 
            }
        };
        
        context.Books.AddRange(books);
        await context.SaveChangesAsync();
    }
}
