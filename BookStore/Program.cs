using BookStore.Data;
using BookStore.Dtos;

var builder = WebApplication.CreateBuilder(args);

var connString = "Data Source=BookStore.db";
builder.Services.AddSqlite<BookStoreContext>(connString);

var app = builder.Build();

app.MapBooksEndpoints();

app.Run();


