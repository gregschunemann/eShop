namespace eShop.Reviews.API.IntegrationEvents;

public record ReviewCreatedIntegrationEvent : IntegrationEvent
{
    public int ProductId { get; init; }
    public string UserId { get; init; }
    public int Rating { get; init; }

    public ReviewCreatedIntegrationEvent(int productId, string userId, int rating)
    {
        ProductId = productId;
        UserId = userId;
        Rating = rating;
    }
}
