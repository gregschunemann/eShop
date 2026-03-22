using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Catalog.API;

public static class ReviewApi
{
    public static IEndpointRouteBuilder MapReviewApi(this IEndpointRouteBuilder app)
    {
        var vApi = app.NewVersionedApi("Reviews");
        var api = vApi.MapGroup("api/catalog/items/{itemId:int}/reviews").HasApiVersion(1, 0);

        api.MapGet("/", GetReviews)
            .WithName("GetItemReviews")
            .WithSummary("Get reviews for a catalog item")
            .WithDescription("Get a paginated list of reviews for the specified catalog item, sorted by newest first")
            .WithTags("Reviews");

        api.MapGet("/summary", GetReviewSummary)
            .WithName("GetItemReviewSummary")
            .WithSummary("Get review summary for a catalog item")
            .WithDescription("Get the average rating and review count for the specified catalog item")
            .WithTags("Reviews");

        api.MapPost("/", CreateOrUpdateReview)
            .WithName("CreateOrUpdateReview")
            .WithSummary("Submit or update a review")
            .WithDescription("Submit a new review or update an existing one for the specified catalog item. One review per user per item.")
            .RequireAuthorization()
            .WithTags("Reviews");

        api.MapGet("/user", GetUserReview)
            .WithName("GetUserReview")
            .WithSummary("Get the current user's review for a catalog item")
            .WithDescription("Get the authenticated user's review for the specified catalog item, if one exists")
            .RequireAuthorization()
            .WithTags("Reviews");

        return app;
    }

    public static async Task<Results<Ok<PaginatedReviewsDto>, NotFound>> GetReviews(
        [Description("The catalog item id")] int itemId,
        [Description("Zero-based page index")] int pageIndex = 0,
        [Description("Number of reviews per page (max 50)")] int pageSize = 10,
        CatalogContext context = default!)
    {
        // Verify item exists
        var itemExists = await context.CatalogItems.AnyAsync(ci => ci.Id == itemId);
        if (!itemExists)
        {
            return TypedResults.NotFound();
        }

        pageSize = Math.Clamp(pageSize, 1, 50);

        var totalCount = await context.ProductReviews
            .Where(r => r.CatalogItemId == itemId)
            .LongCountAsync();

        var reviews = await context.ProductReviews
            .Where(r => r.CatalogItemId == itemId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewDto(
                r.Id, r.CatalogItemId, r.UserId, r.UserName,
                r.Rating, r.ReviewText, r.CreatedAt, r.UpdatedAt))
            .ToListAsync();

        return TypedResults.Ok(new PaginatedReviewsDto(pageIndex, pageSize, totalCount, reviews));
    }

    public static async Task<Results<Ok<ReviewSummaryDto>, NotFound>> GetReviewSummary(
        [Description("The catalog item id")] int itemId,
        CatalogContext context = default!)
    {
        var item = await context.CatalogItems
            .Where(ci => ci.Id == itemId)
            .Select(ci => new { ci.Id, ci.AverageRating, ci.ReviewCount })
            .FirstOrDefaultAsync();

        if (item is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new ReviewSummaryDto(item.Id, item.AverageRating, item.ReviewCount));
    }

    public static async Task<Results<Ok<ReviewDto>, BadRequest<ProblemDetails>, NotFound>> CreateOrUpdateReview(
        [Description("The catalog item id")] int itemId,
        CreateReviewRequest request,
        HttpContext httpContext,
        CatalogContext context)
    {
        // Validate rating range
        if (request.Rating < 1 || request.Rating > 5)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Detail = "Rating must be between 1 and 5."
            });
        }

        // Validate review text length
        if (request.ReviewText is { Length: > 2000 })
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Detail = "Review text must not exceed 2000 characters."
            });
        }

        // Verify item exists
        var item = await context.CatalogItems.FindAsync(itemId);
        if (item is null)
        {
            return TypedResults.NotFound();
        }

        var userId = httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? throw new InvalidOperationException("User ID claim not found");

        var userName = httpContext.User.FindFirst("name")?.Value
            ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
            ?? "Anonymous";

        var now = DateTime.UtcNow;

        // Check for existing review (upsert)
        var existingReview = await context.ProductReviews
            .FirstOrDefaultAsync(r => r.CatalogItemId == itemId && r.UserId == userId);

        if (existingReview is not null)
        {
            existingReview.Rating = request.Rating;
            existingReview.ReviewText = request.ReviewText;
            existingReview.UserName = userName;
            existingReview.UpdatedAt = now;
        }
        else
        {
            existingReview = new ProductReview
            {
                CatalogItemId = itemId,
                UserId = userId,
                UserName = userName,
                Rating = request.Rating,
                ReviewText = request.ReviewText,
                CreatedAt = now,
                UpdatedAt = now
            };
            context.ProductReviews.Add(existingReview);
        }

        // Recalculate denormalized rating on CatalogItem
        // We need to account for the current review (new or updated)
        await context.SaveChangesAsync();

        var stats = await context.ProductReviews
            .Where(r => r.CatalogItemId == itemId)
            .GroupBy(r => r.CatalogItemId)
            .Select(g => new { Avg = g.Average(r => r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync();

        item.AverageRating = stats?.Avg ?? 0;
        item.ReviewCount = stats?.Count ?? 0;
        await context.SaveChangesAsync();

        return TypedResults.Ok(new ReviewDto(
            existingReview.Id,
            existingReview.CatalogItemId,
            existingReview.UserId,
            existingReview.UserName,
            existingReview.Rating,
            existingReview.ReviewText,
            existingReview.CreatedAt,
            existingReview.UpdatedAt));
    }

    public static async Task<Results<Ok<ReviewDto>, NotFound>> GetUserReview(
        [Description("The catalog item id")] int itemId,
        HttpContext httpContext,
        CatalogContext context)
    {
        var userId = httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? throw new InvalidOperationException("User ID claim not found");

        var review = await context.ProductReviews
            .FirstOrDefaultAsync(r => r.CatalogItemId == itemId && r.UserId == userId);

        if (review is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new ReviewDto(
            review.Id,
            review.CatalogItemId,
            review.UserId,
            review.UserName,
            review.Rating,
            review.ReviewText,
            review.CreatedAt,
            review.UpdatedAt));
    }
}
