# API Specification

This is the API specification for the spec detailed in [spec.md](../spec.md)

> Created: 2026-02-17
> Version: 1.0.0

## Overview

New review endpoints are added to the existing Catalog API as a versioned route group under `/api/catalog/items/{itemId}/reviews`. Endpoints follow the existing Minimal API patterns in [CatalogApi.cs](../../../../src/Catalog.API/Apis/CatalogApi.cs) with `Asp.Versioning` and OpenAPI documentation.

## Endpoints

### GET `/api/catalog/items/{itemId}/reviews`

**Description:** Get paginated reviews for a catalog item, sorted by newest first.

**Authentication:** None (public)

**Parameters:**

| Parameter | Location | Type | Required | Default | Description |
|-----------|----------|------|----------|---------|-------------|
| `itemId` | Path | int | Yes | — | Catalog item ID |
| `pageIndex` | Query | int | No | 0 | Zero-based page index |
| `pageSize` | Query | int | No | 10 | Number of reviews per page (max 50) |

**Response 200:**

```json
{
  "pageIndex": 0,
  "pageSize": 10,
  "count": 47,
  "data": [
    {
      "id": 1,
      "catalogItemId": 5,
      "userId": "user-guid",
      "userName": "John D.",
      "rating": 4,
      "reviewText": "Great product, works as expected.",
      "createdAt": "2026-02-15T10:30:00Z",
      "updatedAt": "2026-02-15T10:30:00Z"
    }
  ]
}
```

**Response 404:** Item not found

---

### GET `/api/catalog/items/{itemId}/reviews/summary`

**Description:** Get the review summary (average rating + count) for a catalog item. This endpoint is available for scenarios where only the summary is needed without individual reviews.

**Authentication:** None (public)

**Parameters:**

| Parameter | Location | Type | Required | Description |
|-----------|----------|------|----------|-------------|
| `itemId` | Path | int | Yes | Catalog item ID |

**Response 200:**

```json
{
  "catalogItemId": 5,
  "averageRating": 4.2,
  "reviewCount": 47
}
```

**Response 404:** Item not found

---

### POST `/api/catalog/items/{itemId}/reviews`

**Description:** Submit or update a review for a catalog item. If the authenticated user already has a review for this item, it is replaced (upsert).

**Authentication:** Required (JWT Bearer)

**Parameters:**

| Parameter | Location | Type | Required | Description |
|-----------|----------|------|----------|-------------|
| `itemId` | Path | int | Yes | Catalog item ID |

**Request Body:**

```json
{
  "rating": 4,
  "reviewText": "Great product, works as expected."
}
```

**Validation Rules:**

| Field | Rule |
|-------|------|
| `rating` | Required, integer, 1–5 |
| `reviewText` | Optional, max 2000 characters |

**Response 200:** Review created/updated successfully

```json
{
  "id": 1,
  "catalogItemId": 5,
  "userId": "user-guid",
  "userName": "John D.",
  "rating": 4,
  "reviewText": "Great product, works as expected.",
  "createdAt": "2026-02-15T10:30:00Z",
  "updatedAt": "2026-02-17T14:00:00Z"
}
```

**Response 400:** Validation error (invalid rating, text too long)
**Response 401:** Not authenticated
**Response 404:** Item not found

---

### GET `/api/catalog/items/{itemId}/reviews/user`

**Description:** Get the authenticated user's review for a specific item (if it exists). Used to pre-populate the review form.

**Authentication:** Required (JWT Bearer)

**Parameters:**

| Parameter | Location | Type | Required | Description |
|-----------|----------|------|----------|-------------|
| `itemId` | Path | int | Yes | Catalog item ID |

**Response 200:** User's review

```json
{
  "id": 1,
  "catalogItemId": 5,
  "userId": "user-guid",
  "userName": "John D.",
  "rating": 4,
  "reviewText": "Great product, works as expected.",
  "createdAt": "2026-02-15T10:30:00Z",
  "updatedAt": "2026-02-15T10:30:00Z"
}
```

**Response 404:** User has not reviewed this item (not an error; UI treats as "no existing review")

## Data Models

### Request DTOs

```csharp
public record CreateReviewRequest(
    [Range(1, 5)] int Rating,
    [MaxLength(2000)] string? ReviewText);
```

### Response DTOs

```csharp
public record ReviewDto(
    int Id,
    int CatalogItemId,
    string UserId,
    string UserName,
    int Rating,
    string? ReviewText,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ReviewSummaryDto(
    int CatalogItemId,
    double AverageRating,
    int ReviewCount);

public record PaginatedReviewsDto(
    int PageIndex,
    int PageSize,
    int Count,
    List<ReviewDto> Data);
```

### CatalogItem Record Modifications (WebAppComponents)

The existing `CatalogItem` record in `WebAppComponents/Catalog/CatalogItem.cs` needs two new properties:

```csharp
public record CatalogItem(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string PictureUrl,
    int CatalogBrandId,
    CatalogBrand CatalogBrand,
    int CatalogTypeId,
    CatalogItemType CatalogType,
    double AverageRating,     // NEW
    int ReviewCount);         // NEW
```

## Implementation Pattern

### Endpoint Registration

New endpoints follow the existing pattern in `CatalogApi.cs` using `MapGroup` and `Asp.Versioning`:

```csharp
// Inside MapCatalogApi method
var reviewsApi = api.MapGroup("/items/{itemId:int}/reviews");

reviewsApi.MapGet("/", GetReviews)
    .WithName("GetItemReviews")
    .WithSummary("Get reviews for a catalog item")
    .WithDescription("Get a paginated list of reviews for the specified catalog item")
    .WithTags("Reviews");

reviewsApi.MapGet("/summary", GetReviewSummary)
    .WithName("GetItemReviewSummary")
    .WithSummary("Get review summary for a catalog item")
    .WithTags("Reviews");

reviewsApi.MapPost("/", CreateOrUpdateReview)
    .WithName("CreateOrUpdateReview")
    .WithSummary("Submit or update a review")
    .RequireAuthorization()
    .WithTags("Reviews");

reviewsApi.MapGet("/user", GetUserReview)
    .WithName("GetUserReview")
    .WithSummary("Get the current user's review for a catalog item")
    .RequireAuthorization()
    .WithTags("Reviews");
```

### Error Handling

- Returns `Results.NotFound()` when the catalog item doesn't exist
- Returns `Results.ValidationProblem()` for invalid input
- Returns `Results.Unauthorized()` for unauthenticated review submission
- Follows existing `ProblemDetails` pattern for error responses

## CatalogService Extensions (WebAppComponents)

New methods to add to `CatalogService.cs` and `ICatalogService.cs`:

```csharp
// ICatalogService additions
Task<PaginatedReviewsDto> GetReviews(int itemId, int pageIndex = 0, int pageSize = 10);
Task<ReviewDto?> GetUserReview(int itemId);
Task<ReviewDto> SubmitReview(int itemId, int rating, string? reviewText);
```

## Catalog Listing Integration

The existing `CatalogItem` DTO returned by `GET /api/catalog/items` already carries all properties. By adding `AverageRating` and `ReviewCount` to the `CatalogItem` entity and DTO, the catalog listing response includes rating data without additional API calls.

No changes needed to the listing endpoints themselves — the new fields are automatically serialized.
