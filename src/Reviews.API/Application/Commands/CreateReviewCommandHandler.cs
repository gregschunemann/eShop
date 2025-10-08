using eShop.Reviews.API.IntegrationEvents;

namespace eShop.Reviews.API.Application.Commands;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Review>
{
    private readonly ReviewsContext _context;
    private readonly ILogger<CreateReviewCommandHandler> _logger;
    private readonly IEventBus _eventBus;

    public CreateReviewCommandHandler(
        ReviewsContext context,
        ILogger<CreateReviewCommandHandler> logger,
        IEventBus eventBus)
    {
        _context = context;
        _logger = logger;
        _eventBus = eventBus;
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

        var integrationEvent = new ReviewCreatedIntegrationEvent(
            review.ProductId,
            review.UserId,
            review.Rating);

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);

        _logger.LogInformation("Published ReviewCreatedIntegrationEvent for review {ReviewId}", review.Id);

        return review;
    }
}
