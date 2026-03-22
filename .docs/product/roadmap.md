# Product Roadmap

> Last Updated: 2026-02-17
> Version: 1.0.0
> Status: Development

## Phase 0: Existing Implementation (Completed)

**Goal:** Establish core e-commerce microservices architecture with .NET Aspire
**Success Criteria:** Fully functional e-commerce application with catalog, basket, ordering, identity, and payment services

### Completed Features

- [x] Catalog API - Product browsing, search, pagination with API versioning
- [x] Basket API - Redis-backed basket with gRPC service
- [x] Ordering API - DDD/CQRS-based order management with MediatR
- [x] Identity API - Duende IdentityServer with ASP.NET Core Identity
- [x] Payment Processor - Async payment handling via integration events
- [x] Order Processor - Background order processing service
- [x] WebApp - Blazor Server frontend with interactive components
- [x] Webhooks API & Client - Event subscription and notification system
- [x] Mobile BFF - YARP reverse proxy for mobile clients
- [x] AI Integration - Optional Azure OpenAI / Ollama chatbot
- [x] Event Bus - RabbitMQ-based integration event system
- [x] .NET Aspire Orchestration - Full service orchestration and discovery
- [x] OpenTelemetry - Distributed tracing and metrics
- [x] E2E Tests - Playwright-based browser tests
- [x] Unit & Functional Tests - MSTest/xUnit test suites

## Phase 1: Ratings System (Planned)

**Goal:** Allow customers to rate and review products
**Success Criteria:** Users can submit ratings, view average ratings on product listings, and read reviews

### Must-Have Features

- [ ] Ratings API - New microservice for product ratings and reviews `Medium`
- [ ] Rating model - Star rating (1-5) with optional text review `Small`
- [ ] Catalog integration - Display average ratings on product cards `Small`
- [ ] Product detail ratings - Full review list on product detail page `Medium`

### Should-Have Features

- [ ] Moderation - Basic review moderation workflow `Medium`
- [ ] Sorting/filtering - Sort products by rating, filter by minimum stars `Small`
- [ ] User review history - Users can view and manage their submitted reviews `Small`

### Dependencies

- Identity API for user authentication
- Catalog API for product reference data
- RabbitMQ for integration events (rating submitted, review moderated)

## Phase 2: Future Enhancements (Ideation)

**Goal:** Extend the platform with additional e-commerce capabilities
**Success Criteria:** TBD based on community and team input

### Should-Have Features

- [ ] Wishlist functionality - Save products for later `Medium`
- [ ] Order history - Detailed order history with re-order capability `Medium`
- [ ] Inventory management - Stock tracking and low-stock alerts `Large`
- [ ] Promotions engine - Discount codes and promotional pricing `Large`

### Dependencies

- Ratings system completion (Phase 1)
- Community feedback and prioritization
