namespace eShop.WebAppComponents.Catalog;

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

public record CreateReviewRequest(
    int Rating,
    string? ReviewText);
