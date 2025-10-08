namespace eShop.Reviews.API.Infrastructure;

public class ReviewsContextSeed : IDbSeeder<ReviewsContext>
{
    public async Task SeedAsync(ReviewsContext context)
    {
        if (!context.Reviews.Any())
        {
            await context.Reviews.AddRangeAsync(GetPreconfiguredReviews());
            await context.SaveChangesAsync();
        }
    }

    private static IEnumerable<Review> GetPreconfiguredReviews()
    {
        return new List<Review>
        {
            new Review
            {
                ProductId = 1,
                UserId = "test-user-1",
                Rating = 5,
                ReviewText = "Excellent product! Highly recommended.",
                CreatedDate = DateTime.UtcNow.AddDays(-10)
            },
            new Review
            {
                ProductId = 1,
                UserId = "test-user-2",
                Rating = 4,
                ReviewText = "Good quality, fast delivery.",
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            },
            new Review
            {
                ProductId = 2,
                UserId = "test-user-1",
                Rating = 3,
                ReviewText = "Average product, could be better.",
                CreatedDate = DateTime.UtcNow.AddDays(-3)
            }
        };
    }
}
