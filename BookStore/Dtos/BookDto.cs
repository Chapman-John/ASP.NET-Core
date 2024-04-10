namespace BookStore.Dtos;

public record class BookDto(int Id, string Name, string Genre, decimal Price, DateOnly ReleaseDate);
