namespace eShop.Reviews.API.Application.Queries;

public interface IReviewQueries
{
    Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
    Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
    Task<ReviewSummary> GetProductReviewSummaryAsync(int productId);
}

public record ReviewSummary(
    int ProductId,
    double AverageRating,
    int TotalReviews);
