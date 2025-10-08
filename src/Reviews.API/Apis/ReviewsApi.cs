using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Reviews.API.Apis;

public static class ReviewsApi
{
    public static RouteGroupBuilder MapReviewsApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/reviews").HasApiVersion(1.0);

        api.MapGet("/product/{productId:int}", GetReviewsByProductId)
            .WithName("GetReviewsByProductId")
            .WithSummary("Get reviews for a product")
            .WithDescription("Get all reviews for a specific product");

        api.MapGet("/product/{productId:int}/summary", GetProductReviewSummary)
            .WithName("GetProductReviewSummary")
            .WithSummary("Get review summary for a product")
            .WithDescription("Get average rating and total reviews for a specific product");

        api.MapGet("/user", GetReviewsByUser)
            .WithName("GetReviewsByUser")
            .WithSummary("Get reviews by current user")
            .WithDescription("Get all reviews created by the current authenticated user");

        api.MapPost("/", CreateReview)
            .WithName("CreateReview")
            .WithSummary("Create a new review")
            .WithDescription("Create a new product review with rating and optional text");

        return api;
    }

    public static async Task<Ok<IEnumerable<Review>>> GetReviewsByProductId(
        int productId,
        [FromServices] IReviewQueries queries)
    {
        var reviews = await queries.GetReviewsByProductIdAsync(productId);
        return TypedResults.Ok(reviews);
    }

    public static async Task<Ok<ReviewSummary>> GetProductReviewSummary(
        int productId,
        [FromServices] IReviewQueries queries)
    {
        var summary = await queries.GetProductReviewSummaryAsync(productId);
        return TypedResults.Ok(summary);
    }

    public static async Task<Ok<IEnumerable<Review>>> GetReviewsByUser(
        [FromServices] IReviewQueries queries,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.Ok(Enumerable.Empty<Review>());
        }

        var reviews = await queries.GetReviewsByUserIdAsync(userId);
        return TypedResults.Ok(reviews);
    }

    public static async Task<Results<Created<Review>, BadRequest<string>, ValidationProblem>> CreateReview(
        [FromBody] CreateReviewRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.BadRequest("User must be authenticated");
        }

        var command = new CreateReviewCommand(
            request.ProductId,
            userId,
            request.Rating,
            request.ReviewText);

        try
        {
            var review = await mediator.Send(command);
            return TypedResults.Created($"/api/reviews/{review.Id}", review);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}

public record CreateReviewRequest(
    int ProductId,
    int Rating,
    string? ReviewText);
