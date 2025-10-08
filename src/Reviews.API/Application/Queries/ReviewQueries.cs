namespace eShop.Reviews.API.Application.Queries;

public class ReviewQueries : IReviewQueries
{
    private readonly ReviewsContext _context;

    public ReviewQueries(ReviewsContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
    {
        return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
    {
        return await _context.Reviews
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<ReviewSummary> GetProductReviewSummaryAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        if (!reviews.Any())
        {
            return new ReviewSummary(productId, 0, 0);
        }

        var averageRating = reviews.Average(r => r.Rating);
        var totalReviews = reviews.Count;

        return new ReviewSummary(productId, averageRating, totalReviews);
    }
}
