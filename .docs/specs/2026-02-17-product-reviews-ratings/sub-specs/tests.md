# Tests Specification

This is the tests coverage details for the spec detailed in [spec.md](../spec.md)

> Created: 2026-02-17
> Version: 1.0.0

## Test Coverage

### Unit Tests

New test project or extend existing `Catalog.FunctionalTests` for API-level tests. Recommend adding a `Catalog.UnitTests` project for pure domain logic, or adding to the functional tests project given the existing pattern.

**ProductReview Entity**
- Verify `Rating` constrained to 1–5 range (if domain validation added)
- Verify `ReviewText` max length enforcement

**Review Business Logic (CatalogApi review handlers)**
- Submit review with valid rating (1–5) returns success
- Submit review with rating 0 returns validation error
- Submit review with rating 6 returns validation error
- Submit review with review text exceeding 2000 characters returns validation error
- Submit review with empty review text (null) succeeds (text is optional)
- Upsert: submitting a second review for same user + item updates existing review
- Upsert: updated review has new `UpdatedAt` timestamp
- Denormalized `AverageRating` updates correctly after review submit
- Denormalized `ReviewCount` updates correctly after review submit

### Integration / Functional Tests

**Review Submission (POST /api/catalog/items/{id}/reviews)**
- Authenticated user can submit a review → 200 OK with ReviewDto
- Unauthenticated request → 401 Unauthorized
- Review for non-existent item → 404 Not Found
- Valid rating values (1, 2, 3, 4, 5) all succeed
- Invalid rating values (0, -1, 6, 100) all return 400
- Review text at exactly 2000 characters succeeds
- Review text at 2001 characters returns 400
- Submitting a second review for same user+item replaces the first (upsert)
- After upsert, `AverageRating` and `ReviewCount` on CatalogItem are recalculated

**Review Retrieval (GET /api/catalog/items/{id}/reviews)**
- Returns paginated reviews sorted by newest first
- Default page size is 10
- `pageSize=3` returns exactly 3 reviews (for "recent reviews" use case)
- `pageIndex` and `pageSize` pagination works correctly
- Returns 404 for non-existent item
- Empty reviews list returns valid response with `count: 0`

**Review Summary (GET /api/catalog/items/{id}/reviews/summary)**
- Returns correct `averageRating` and `reviewCount`
- Returns `averageRating: 0` and `reviewCount: 0` for item with no reviews
- Returns 404 for non-existent item

**User Review (GET /api/catalog/items/{id}/reviews/user)**
- Authenticated user with existing review → 200 OK with their review
- Authenticated user with no review → 404
- Unauthenticated → 401

**Catalog Item Integration**
- `GET /api/catalog/items` response includes `averageRating` and `reviewCount` fields
- `GET /api/catalog/items/{id}` response includes `averageRating` and `reviewCount` fields
- Average rating is 0 for items with no reviews

### E2E / Feature Tests (Playwright)

**Submit and View Review Flow**
- Log in → navigate to product page → verify review form visible
- Submit star rating + review text → verify review appears in recent reviews section
- Verify average rating and review count update on product page
- Navigate to "See all ratings and reviews" → verify all-reviews page loads

**Anonymous User Flow**
- Navigate to product page (not logged in) → verify review form is NOT visible
- Verify existing reviews are still visible to anonymous users
- Verify "See all ratings and reviews" link works for anonymous users

**Catalog Listing Stars**
- Navigate to catalog listing → verify star ratings appear on product cards
- Verify product with no reviews shows no/zero rating

**Pagination**
- Navigate to all-reviews page for product with > 10 reviews
- Verify pagination controls are visible
- Navigate between pages and verify different reviews load

### Mocking Requirements

- **Identity/Authentication:** Use `WebApplicationFactory` with test authentication handler (existing pattern in `Catalog.FunctionalTests`)
- **Database:** Use in-memory or TestContainers PostgreSQL for isolated test database (follow existing Catalog.FunctionalTests pattern)
- **No external service mocks needed** — reviews are self-contained within Catalog API

### Test Data Setup

- Seed catalog items for review tests
- Create test users with known IDs for review submission
- Seed reviews at known timestamps for pagination/ordering assertions
- Seed items with pre-calculated `AverageRating`/`ReviewCount` for listing tests
