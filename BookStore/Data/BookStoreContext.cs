using BookStore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data;

public class BookStoreContext(DbContextOptions<BookStoreContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    public DbSet<Genre> Genres => Set<Genre>();
}
