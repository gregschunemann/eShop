using System.ComponentModel.DataAnnotations;

namespace eShop.Catalog.API.Model;

public record CreateReviewRequest(
    [property: Range(1, 5)] int Rating,
    [property: MaxLength(2000)] string? ReviewText);

public record ReviewDto(
    int Id,
    int CatalogItemId,
    string UserId,
    string UserName,
    int Rating,
    string? ReviewText,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ReviewSummaryDto(
    int CatalogItemId,
    double AverageRating,
    int ReviewCount);

public record PaginatedReviewsDto(
    int PageIndex,
    int PageSize,
    long Count,
    List<ReviewDto> Data);
