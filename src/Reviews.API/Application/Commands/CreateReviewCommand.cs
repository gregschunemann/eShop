namespace eShop.Reviews.API.Application.Commands;

public record CreateReviewCommand(
    int ProductId,
    string UserId,
    int Rating,
    string? ReviewText) : IRequest<Review>;
