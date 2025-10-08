namespace eShop.Reviews.API.Application.Commands;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Review>
{
    private readonly ReviewsContext _context;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(
        ReviewsContext context,
        ILogger<CreateReviewCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Review> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = request.UserId,
            Rating = request.Rating,
            ReviewText = request.ReviewText,
            CreatedDate = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created review {ReviewId} for product {ProductId} by user {UserId}", 
            review.Id, review.ProductId, review.UserId);

        return review;
    }
}
