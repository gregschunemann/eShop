using System.Net.Http.Json;

namespace eShop.WebAppComponents.Services;

public class ReviewService(HttpClient httpClient) : IReviewService
{
    private readonly string remoteServiceBaseUrl = "api/reviews/";

    public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
    {
        var uri = $"{remoteServiceBaseUrl}product/{productId}";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<Review>>(uri);
        return result ?? Enumerable.Empty<Review>();
    }

    public async Task<ReviewSummary> GetProductReviewSummaryAsync(int productId)
    {
        var uri = $"{remoteServiceBaseUrl}product/{productId}/summary";
        var result = await httpClient.GetFromJsonAsync<ReviewSummary>(uri);
        return result ?? new ReviewSummary(productId, 0, 0);
    }

    public async Task<Review> CreateReviewAsync(CreateReviewRequest request)
    {
        var response = await httpClient.PostAsJsonAsync(remoteServiceBaseUrl, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Review>();
        return result!;
    }
}
