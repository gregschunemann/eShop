# Reviews.API

The Reviews API microservice provides functionality for users to rate and review products in the eShop application.

## Features

- **Create Reviews**: Users can submit reviews with a 1-5 star rating and optional review text
- **Query Reviews**: Retrieve reviews by product ID or user ID
- **Review Summary**: Get aggregated review data including average rating and total review count
- **Integration Events**: Publishes ReviewCreatedIntegrationEvent when new reviews are submitted

## Architecture

This microservice follows the eShop architecture patterns:

### CQRS Pattern
- **Commands**: `CreateReviewCommand` - Creates a new product review
- **Queries**: 
  - `GetReviewsByProductIdAsync` - Retrieves all reviews for a specific product
  - `GetReviewsByUserIdAsync` - Retrieves all reviews by a specific user
  - `GetProductReviewSummaryAsync` - Gets aggregated review statistics

### Database
- PostgreSQL database with Entity Framework Core
- `ReviewsContext` manages the `Reviews` table
- Includes database seeding with sample data

### Authentication
- JWT token-based authentication via Identity.API
- All endpoints require authentication
- User ID is extracted from JWT claims

### Validation
- FluentValidation for command validation
- Rating must be between 1 and 5
- Review text limited to 2000 characters
- Product ID and User ID required

## API Endpoints

All endpoints are versioned (v1.0) and require authentication.

### Create Review
```http
POST /api/reviews
Content-Type: application/json
Authorization: Bearer {token}

{
  "productId": 1,
  "rating": 5,
  "reviewText": "Excellent product!"
}
```

Response: `201 Created` with Review object

### Get Reviews by Product
```http
GET /api/reviews/product/{productId}
Authorization: Bearer {token}
```

Response: `200 OK` with array of Review objects

### Get Product Review Summary
```http
GET /api/reviews/product/{productId}/summary
Authorization: Bearer {token}
```

Response: `200 OK` with ReviewSummary object
```json
{
  "productId": 1,
  "averageRating": 4.5,
  "totalReviews": 10
}
```

### Get Reviews by Current User
```http
GET /api/reviews/user
Authorization: Bearer {token}
```

Response: `200 OK` with array of Review objects

## Integration Events

### ReviewCreatedIntegrationEvent
Published when a new review is created:
```csharp
{
  "productId": int,
  "userId": string,
  "rating": int
}
```

This event can be consumed by other microservices (e.g., Webhooks.API) to send notifications.

## Development

### Running Locally with Aspire
The Reviews.API is configured in the Aspire AppHost and will start automatically with the eShop application:

```bash
cd src/eShop.AppHost
dotnet run
```

### Database Migrations
Create a new migration:
```bash
cd src/Reviews.API
dotnet ef migrations add MigrationName --context ReviewsContext
```

Apply migrations (handled automatically on startup):
```bash
dotnet ef database update --context ReviewsContext
```

## Testing

Functional tests are located in `tests/Reviews.FunctionalTests`:

```bash
cd tests/Reviews.FunctionalTests
dotnet test
```

Tests cover:
- Creating reviews with valid data
- Validation of invalid ratings
- Querying reviews by product
- Calculating review summaries

## Dependencies

- **eShop.ServiceDefaults**: Common service configuration
- **EventBusRabbitMQ**: Event bus integration
- **Aspire.Npgsql.EntityFrameworkCore.PostgreSQL**: PostgreSQL database
- **MediatR**: CQRS implementation
- **FluentValidation**: Command validation
- **Asp.Versioning.Http**: API versioning

## Configuration

Key configuration in `appsettings.json`:
- `ConnectionStrings:reviewsdb` - PostgreSQL connection string (provided by Aspire)
- `Identity:Url` - Identity API URL for JWT validation
