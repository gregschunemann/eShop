using System.ComponentModel.DataAnnotations;

namespace eShop.Reviews.API.Application.Models;

public class Review
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    public string? ReviewText { get; set; }

    public DateTime CreatedDate { get; set; }

    public Review()
    {
        CreatedDate = DateTime.UtcNow;
    }
}
