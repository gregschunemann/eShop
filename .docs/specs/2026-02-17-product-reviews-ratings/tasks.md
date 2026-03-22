# Spec Tasks

These are the tasks to be completed for the spec detailed in @.docs/specs/2026-02-17-product-reviews-ratings/spec.md

> Created: 2026-02-27
> Status: Implementation Complete (pending migration & tests)
> Completed: 2026-02-27

## Tasks

- [x] 1. Database & Entity Layer
  - [x] 1.1 Create `ProductReview` entity class in `Catalog.API/Model/`
  - [x] 1.2 Add `AverageRating` and `ReviewCount` properties to `CatalogItem` entity
  - [x] 1.3 Create `ProductReviewEntityTypeConfiguration` in `Catalog.API/Infrastructure/EntityConfigurations/`
  - [x] 1.4 Update `CatalogItemEntityTypeConfiguration` with new property defaults
  - [x] 1.5 Add `DbSet<ProductReview>` to `CatalogContext`
  - [ ] 1.6 Generate EF Core migration `AddProductReviews`
  - [ ] 1.7 Verify migration applies cleanly
  - **todo-md ID:** `1f31dbe5-29ec-46e5-b9dd-899d9ca86880`

- [x] 2. API Endpoints (Catalog.API)
  - [x] 2.1 Create request/response DTOs (`CreateReviewRequest`, `ReviewDto`, `ReviewSummaryDto`, `PaginatedReviewsDto`)
  - [x] 2.2 Add review endpoint group in new `ReviewApi.cs` file
  - [x] 2.3 Implement `GET /` handler — paginated reviews (public)
  - [x] 2.4 Implement `GET /summary` handler — average + count (public)
  - [x] 2.5 Implement `POST /` handler — create/upsert review (auth required)
  - [x] 2.6 Implement `GET /user` handler — current user's review (auth required)
  - [x] 2.7 Verify existing catalog endpoints include `AverageRating`/`ReviewCount` in response
  - **todo-md ID:** `0b23ee90-57b1-4c64-879c-d5fd184336b9`

- [x] 3. Frontend DTOs & Service Layer (WebAppComponents)
  - [x] 3.1 Update `CatalogItem` record to add `AverageRating` and `ReviewCount`
  - [x] 3.2 Create review DTO records in `WebAppComponents/Catalog/ReviewModels.cs`
  - [x] 3.3 Add review methods to `ICatalogService` interface
  - [x] 3.4 Implement review methods in `CatalogService`
  - **todo-md ID:** `bac3f5f8-80d1-4c04-9b2d-48ff2638a44f`

- [x] 4. Blazor UI Components (WebApp + WebAppComponents)
  - [x] 4.1 Create `StarRating.razor` + `.css` component (display + interactive modes)
  - [x] 4.2 Create `ReviewCard.razor` + `.css` component
  - [x] 4.3 Create `ReviewForm.razor` + `.css` component
  - [x] 4.4 Modify `ItemPage.razor` — add reviews section + star rating display
  - [x] 4.5 Create `AllReviewsPage.razor` + `.css` at `/item/{id}/reviews`
  - [x] 4.6 Modify `CatalogListItem.razor` — add compact star rating on product cards
  - **todo-md ID:** `297cee12-adaf-44ab-bdb2-350ee44c6245`

- [x] 5. Build Verification & Testing
  - [x] 5.1 Verify `dotnet build` succeeds — Catalog.API, WebApp, eShop.AppHost all pass (0 errors)
  - [ ] 5.2 Update functional tests to account for new fields
  - [ ] 5.3 Run full test suite
  - **todo-md ID:** `1d667ec4-21c2-4645-b751-4a9f4030d73a`
