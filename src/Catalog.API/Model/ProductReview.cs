using System.ComponentModel.DataAnnotations;

namespace eShop.Catalog.API.Model;

public class ProductReview
{
    public int Id { get; set; }

    public int CatalogItemId { get; set; }

    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = default!;

    [Required]
    [MaxLength(256)]
    public string UserName { get; set; } = default!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? ReviewText { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
