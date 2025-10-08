using System.Net;
using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace eShop.Reviews.FunctionalTests;

public sealed class ReviewsApiTests : IClassFixture<ReviewsApiFixture>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReviewsApiTests(ReviewsApiFixture fixture)
    {
        var handler = new ApiVersionHandler(new QueryStringApiVersionWriter(), new ApiVersion(1.0));

        _webApplicationFactory = fixture;
        _httpClient = _webApplicationFactory.CreateDefaultClient(handler);
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [Fact]
    public async Task CreateReviewWorks()
    {
        // Arrange
        var reviewRequest = new
        {
            ProductId = 1,
            Rating = 5,
            ReviewText = "Great product!"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(reviewRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _httpClient.PostAsync("api/reviews", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateReviewWithInvalidRatingFails()
    {
        // Arrange
        var reviewRequest = new
        {
            ProductId = 1,
            Rating = 6, // Invalid rating (should be 1-5)
            ReviewText = "Test review"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(reviewRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _httpClient.PostAsync("api/reviews", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetReviewsByProductIdWorks()
    {
        // Arrange - First create a review
        var reviewRequest = new
        {
            ProductId = 2,
            Rating = 4,
            ReviewText = "Good product"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(reviewRequest),
            Encoding.UTF8,
            "application/json");
        await _httpClient.PostAsync("api/reviews", content);

        // Act
        var response = await _httpClient.GetAsync("api/reviews/product/2");
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductReviewSummaryWorks()
    {
        // Arrange - Create multiple reviews for the same product
        var productId = 3;
        var reviews = new[]
        {
            new { ProductId = productId, Rating = 5, ReviewText = "Excellent!" },
            new { ProductId = productId, Rating = 4, ReviewText = "Very good" },
            new { ProductId = productId, Rating = 5, ReviewText = "Amazing!" }
        };

        foreach (var review in reviews)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(review),
                Encoding.UTF8,
                "application/json");
            await _httpClient.PostAsync("api/reviews", content);
        }

        // Act
        var response = await _httpClient.GetAsync($"api/reviews/product/{productId}/summary");
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var summary = JsonSerializer.Deserialize<ReviewSummary>(responseContent, _jsonOptions);
        Assert.NotNull(summary);
        Assert.Equal(productId, summary.ProductId);
        Assert.True(summary.TotalReviews >= 3);
        Assert.True(summary.AverageRating >= 4.0);
    }

    private record ReviewSummary(int ProductId, double AverageRating, int TotalReviews);
}
