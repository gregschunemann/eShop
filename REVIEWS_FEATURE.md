# Product Reviews & Ratings Feature

This document describes the Product Reviews & Ratings feature implementation for eShop.

## Overview

The Product Reviews & Ratings feature allows authenticated users to:
- Rate products on a scale of 1 to 5 stars
- Write optional text reviews
- View reviews from other users
- See aggregated review statistics (average rating, total reviews)

## Architecture

The feature follows eShop's microservices architecture and best practices:

### Microservices
- **Reviews.API**: Dedicated microservice for review management
- Follows CQRS pattern with MediatR
- Uses PostgreSQL for data storage
- Publishes integration events via RabbitMQ
- Secured with JWT authentication

### Frontend
- **Blazor Components**: Reusable UI components for web and MAUI
  - `StarRating`: Display and interactive star rating component
  - `ReviewList`: Displays product reviews with summary
  - `ReviewForm`: Allows users to submit new reviews
- **WebApp Integration**: Reviews displayed on product detail pages
- **MAUI/HybridApp Integration**: Full support for mobile applications

### Integration
- **Mobile BFF**: YARP reverse proxy routes configured for mobile access
- **Aspire Orchestration**: Reviews.API integrated into local development stack
- **Event Bus**: ReviewCreatedIntegrationEvent published for cross-service notifications

## Implementation Details

### Database Schema

**Reviews Table**
- `Id` (int, primary key)
- `ProductId` (int, required, indexed)
- `UserId` (string, required, indexed, max 256 chars)
- `Rating` (int, required, 1-5)
- `ReviewText` (string, optional, max 2000 chars)
- `CreatedDate` (DateTime, required)

### API Endpoints

Base URL: `/api/reviews` (v1.0)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/` | Create a new review | Yes |
| GET | `/product/{productId}` | Get all reviews for a product | Yes |
| GET | `/product/{productId}/summary` | Get review summary for a product | Yes |
| GET | `/user` | Get all reviews by current user | Yes |

### Request/Response Models

**CreateReviewRequest**
```json
{
  "productId": 1,
  "rating": 5,
  "reviewText": "Great product!"
}
```

**Review**
```json
{
  "id": 1,
  "productId": 1,
  "userId": "user-123",
  "rating": 5,
  "reviewText": "Great product!",
  "createdDate": "2024-01-15T10:30:00Z"
}
```

**ReviewSummary**
```json
{
  "productId": 1,
  "averageRating": 4.5,
  "totalReviews": 10
}
```

### Validation Rules

- Rating must be between 1 and 5 (inclusive)
- ProductId must be greater than 0
- UserId is required (extracted from JWT token)
- ReviewText is optional but limited to 2000 characters

### Integration Events

**ReviewCreatedIntegrationEvent**
- Published when a new review is created
- Contains: ProductId, UserId, Rating
- Can be consumed by Webhooks.API or other services for notifications

## File Structure

### Reviews.API Project
```
src/Reviews.API/
├── Apis/
│   └── ReviewsApi.cs                    # API endpoints
├── Application/
│   ├── Behaviors/
│   │   ├── LoggingBehavior.cs          # Request logging
│   │   └── ValidatorBehavior.cs        # Request validation
│   ├── Commands/
│   │   ├── CreateReviewCommand.cs
│   │   ├── CreateReviewCommandHandler.cs
│   │   └── CreateReviewCommandValidator.cs
│   ├── Models/
│   │   └── Review.cs                    # Domain model
│   └── Queries/
│       ├── IReviewQueries.cs
│       └── ReviewQueries.cs
├── Extensions/
│   └── Extensions.cs                    # Service registration
├── Infrastructure/
│   ├── ReviewsContext.cs               # EF Core DbContext
│   └── ReviewsContextSeed.cs           # Database seeding
├── IntegrationEvents/
│   └── ReviewCreatedIntegrationEvent.cs
├── Properties/
│   └── launchSettings.json
├── GlobalUsings.cs
├── Program.cs
├── Reviews.API.csproj
├── appsettings.json
├── appsettings.Development.json
└── README.md
```

### UI Components
```
src/WebAppComponents/Item/
├── ReviewForm.razor                     # Review submission form
├── ReviewForm.razor.css
├── ReviewList.razor                     # Display reviews
├── ReviewList.razor.css
├── StarRating.razor                     # Star rating component
└── StarRating.razor.css

src/WebAppComponents/Services/
├── IReviewService.cs                    # Service interface
└── ReviewService.cs                     # HTTP client service
```

### Mobile Integration
```
src/HybridApp/Services/
└── ReviewService.cs                     # MAUI HTTP client service
```

### Tests
```
tests/Reviews.FunctionalTests/
├── AutoAuthorizeMiddleware.cs
├── GlobalUsings.cs
├── ReviewsApiFixture.cs                # Test fixture
├── ReviewsApiTests.cs                  # Functional tests
└── Reviews.FunctionalTests.csproj
```

## Configuration Changes

### AppHost (Aspire Orchestration)
- Added `reviewsDb` PostgreSQL database
- Added `reviewsApi` project reference
- Configured Reviews.API with RabbitMQ and database
- Updated Mobile BFF routes to include reviews endpoints
- Updated WebApp to reference Reviews.API

### WebApp
- Added ReviewService HTTP client with authentication
- Updated ItemPage to display reviews and submission form
- Registered ReviewService in DI container

### HybridApp (MAUI)
- Added ReviewService for mobile API calls
- Registered ReviewService in MauiProgram
- Reviews accessible through existing ItemPage component

## Usage

### For Users (Web)
1. Navigate to a product detail page
2. View existing reviews and average rating
3. If logged in, submit a new review:
   - Select star rating (1-5)
   - Optionally add review text
   - Click "Submit Review"
4. See updated reviews immediately after submission

### For Users (Mobile)
1. Open product detail in mobile app
2. View and submit reviews (same as web)
3. Reviews synchronized across all platforms

### For Developers

**Running Locally**
```bash
cd src/eShop.AppHost
dotnet run
```
Access Reviews.API at the URL shown in Aspire dashboard.

**Creating Database Migrations**
```bash
cd src/Reviews.API
dotnet ef migrations add MigrationName --context ReviewsContext
```

**Running Tests**
```bash
cd tests/Reviews.FunctionalTests
dotnet test
```

## Security Considerations

- All endpoints require authentication via JWT tokens
- UserId extracted from token claims (not from request body)
- Prevents users from submitting reviews on behalf of others
- Rate limiting should be considered for production (not implemented)

## Performance Considerations

- Reviews indexed by ProductId and UserId for fast queries
- Review text limited to 2000 characters
- Consider pagination for products with many reviews (future enhancement)
- Caching could be added for review summaries (future enhancement)

## Future Enhancements

Potential improvements not included in initial implementation:
- Pagination for review lists
- Review editing and deletion
- Review helpfulness voting
- Image attachments
- Review moderation
- Review analytics dashboard
- Email notifications via Webhooks
- Review reply functionality
- Verified purchase badges
- Aggregate ratings by star level (e.g., 5★: 60%, 4★: 30%, etc.)

## Testing

The feature includes comprehensive functional tests:
- ✅ Creating reviews with valid data
- ✅ Validation of invalid ratings (e.g., rating > 5)
- ✅ Querying reviews by product ID
- ✅ Calculating review summaries with correct averages
- ✅ Authentication integration via test middleware

## Dependencies

- **.NET 10.0**: Target framework
- **ASP.NET Core**: Web framework
- **Entity Framework Core**: ORM
- **PostgreSQL**: Database
- **MediatR**: CQRS pattern
- **FluentValidation**: Input validation
- **RabbitMQ**: Event bus
- **Blazor**: UI components
- **MAUI**: Mobile support
- **Aspire**: Orchestration and service defaults
- **xUnit**: Testing framework

## Conclusion

The Product Reviews & Ratings feature is fully integrated into the eShop application, following all established patterns and best practices. It provides a seamless experience for users to share feedback on products across web and mobile platforms, with proper security, validation, and event-driven architecture.
