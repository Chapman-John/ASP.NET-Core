using System.ComponentModel.DataAnnotations;

namespace BookStore.Dtos;

public record class CreateBookDto(
    [Required][StringLength(50)] string Name,
    [Required][StringLength(20)] string Genre,
    [Range(1, 100)] decimal Price,
    DateOnly ReleaseDate
    );
