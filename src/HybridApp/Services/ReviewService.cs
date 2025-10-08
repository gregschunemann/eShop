using System.Net.Http.Json;
using eShop.WebAppComponents.Services;

namespace eShop.HybridApp.Services;

public class ReviewService(HttpClient httpClient) : IReviewService
{
    private readonly string remoteServiceBaseUrl = "api/reviews/";

    public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
    {
        var uri = $"{remoteServiceBaseUrl}product/{productId}?api-version=1.0";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<Review>>(uri);
        return result ?? Enumerable.Empty<Review>();
    }

    public async Task<ReviewSummary> GetProductReviewSummaryAsync(int productId)
    {
        var uri = $"{remoteServiceBaseUrl}product/{productId}/summary?api-version=1.0";
        var result = await httpClient.GetFromJsonAsync<ReviewSummary>(uri);
        return result ?? new ReviewSummary(productId, 0, 0);
    }

    public async Task<Review> CreateReviewAsync(CreateReviewRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"{remoteServiceBaseUrl}?api-version=1.0", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Review>();
        return result!;
    }
}
