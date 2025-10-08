namespace eShop.WebAppComponents.Services;

public interface IReviewService
{
    Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
    Task<ReviewSummary> GetProductReviewSummaryAsync(int productId);
    Task<Review> CreateReviewAsync(CreateReviewRequest request);
}

public record Review(
    int Id,
    int ProductId,
    string UserId,
    int Rating,
    string? ReviewText,
    DateTime CreatedDate);

public record ReviewSummary(
    int ProductId,
    double AverageRating,
    int TotalReviews);

public record CreateReviewRequest(
    int ProductId,
    int Rating,
    string? ReviewText);
