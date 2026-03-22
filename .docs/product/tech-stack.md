# Tech Stack - eShop (AdventureWorks)

> Version: 1.0.0
> Last Updated: 2026-02-17

## Context

eShop is a reference .NET application implementing an e-commerce website using a microservices-based architecture with .NET Aspire for orchestration. It demonstrates production-quality patterns including DDD, CQRS, event-driven messaging, and cloud-native design.

## Core Technologies

### Application Framework
- **Framework:** .NET Aspire
- **Version:** 13.1.0 (Aspire SDK)
- **Language:** C# (.NET 10)
- **SDK:** .NET 10.0.100

### Orchestration
- **Host:** Aspire.AppHost.Sdk 13.1.0
- **Service Discovery:** Microsoft.Extensions.ServiceDiscovery 10.1.0
- **Reverse Proxy:** YARP (via Aspire.Hosting.Yarp)

### Database
- **Primary:** PostgreSQL (with pgvector extension)
- **Image:** ankane/pgvector:latest
- **ORM:** Entity Framework Core (Npgsql provider) 10.0.0
- **Databases:** catalogdb, identitydb, orderingdb, webhooksdb

### Cache
- **Engine:** Redis
- **Client:** StackExchange.Redis (via Aspire.StackExchange.Redis)
- **Use:** Basket service data storage

### Message Broker
- **Engine:** RabbitMQ
- **Client:** Aspire.RabbitMQ.Client 13.1.0
- **Use:** Integration events between microservices

## Frontend Stack

### Frontend Framework
- **Framework:** Blazor Server (Interactive Server rendering)
- **Version:** .NET 10
- **Build Tool:** dotnet (MSBuild)
- **Rendering:** Server-side with SignalR

### Component Libraries
- **Shared Library:** WebAppComponents (custom Razor component library)
- **Quick Grid:** Microsoft.AspNetCore.Components.QuickGrid 10.0.1

### CSS Framework
- **Framework:** Custom CSS (no third-party framework detected)
- **Static Assets:** wwwroot folder in WebApp project

## Projects

### Source Projects (19)

| Project | Type | Description |
|---------|------|-------------|
| eShop.AppHost | Aspire Host | Service orchestrator and entry point |
| eShop.ServiceDefaults | Library | Shared service configuration (OpenTelemetry, health checks) |
| Basket.API | Web API (gRPC) | Shopping basket management |
| Catalog.API | Web API (REST) | Product catalog with versioned APIs |
| Identity.API | Web API | Authentication/authorization (IdentityServer) |
| Ordering.API | Web API (REST) | Order management (DDD/CQRS) |
| Ordering.Domain | Library | Domain entities, aggregates, value objects |
| Ordering.Infrastructure | Library | EF Core context, repositories, migrations |
| OrderProcessor | Worker Service | Background order processing |
| PaymentProcessor | Worker Service | Payment handling via integration events |
| WebApp | Blazor Server | E-commerce storefront |
| WebAppComponents | Razor Library | Shared UI components |
| WebhookClient | Web App | Webhook consumer/subscriber |
| Webhooks.API | Web API | Webhook management and dispatch |
| EventBus | Library | Event bus abstractions and interfaces |
| EventBusRabbitMQ | Library | RabbitMQ event bus implementation |
| IntegrationEventLogEF | Library | EF-based integration event durability |
| HybridApp | MAUI Hybrid | .NET MAUI hybrid mobile/desktop app |
| ClientApp | Client App | Client-side application |

### Test Projects (5)

| Project | Type | Framework |
|---------|------|-----------|
| Basket.UnitTests | Unit Tests | MSTest 4.0.2 |
| Catalog.FunctionalTests | Functional Tests | MSTest |
| Ordering.UnitTests | Unit Tests | MSTest |
| Ordering.FunctionalTests | Functional Tests | MSTest |
| ClientApp.UnitTests | Unit Tests | MSTest |

### Development Style
- **Architecture:** Microservices with .NET Aspire orchestration
- **Domain Modeling:** Domain-Driven Design (DDD) in Ordering bounded context
- **Command/Query:** CQRS via MediatR 13.0.0
- **Messaging:** Event-driven with RabbitMQ integration events
- **API Design:** REST (Catalog, Ordering, Webhooks) + gRPC (Basket)
- **Testing:** Unit tests + Functional tests + E2E (Playwright)
- **Package Management:** Central Package Management (Directory.Packages.props)

## Key Dependencies

### API & Web
- Asp.Versioning.Http 8.1.0 — API versioning
- FluentValidation 12.0.0 — Input validation
- Scalar.AspNetCore 2.8.6 — API documentation UI
- Microsoft.AspNetCore.OpenApi 10.0.1 — OpenAPI spec generation
- Google.Protobuf 3.33.0 — Protocol Buffers
- Grpc.AspNetCore 2.71.0 — gRPC server
- Grpc.Net.ClientFactory 2.71.0 — gRPC client

### Data & Messaging
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 — EF Core PostgreSQL provider
- Pgvector 0.3.2 / Pgvector.EntityFrameworkCore 0.2.2 — Vector similarity search
- Dapper 2.1.35 — Lightweight ORM (read queries)
- MediatR 13.0.0 — Mediator pattern (CQRS)

### Identity
- Duende.IdentityServer 7.3.2 — OAuth2/OpenID Connect server
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.1 — User management
- IdentityModel 7.0.0 — Identity protocol helpers

### AI
- Aspire.Azure.AI.OpenAI 13.1.0-preview — Azure OpenAI integration
- CommunityToolkit.Aspire.OllamaSharp 9.8.0-beta.395 — Local LLM support

### Observability
- OpenTelemetry.Exporter.OpenTelemetryProtocol 1.14.0
- OpenTelemetry.Extensions.Hosting 1.14.0
- OpenTelemetry.Instrumentation.AspNetCore 1.14.0
- OpenTelemetry.Instrumentation.Http 1.14.0
- OpenTelemetry.Instrumentation.GrpcNetClient 1.14.0-beta.1
- OpenTelemetry.Instrumentation.Runtime 1.14.0

### Testing
- MSTest 4.0.2 — Test framework
- xunit.v3.mtp-v2 3.2.1 — xUnit v3 adapter
- NSubstitute 5.3.0 — Mocking
- Microsoft.AspNetCore.Mvc.Testing 10.0.1 — Integration test host
- Playwright (npm) 1.42.1 — E2E browser testing

### Dev Tools
- Microsoft.EntityFrameworkCore.Tools 10.0.1 — EF migrations CLI
- Microsoft.Web.LibraryManager.Build 3.0.71 — Client-side library management

## Infrastructure

### Application Hosting
- **Platform:** Local development via Docker / .NET Aspire
- **Service:** Container-based with Aspire orchestration
- **Region:** Local (development focus)

### Database Hosting
- **Platform:** Docker container (ankane/pgvector)
- **Persistence:** Persistent container lifetime
- **Databases:** catalogdb, identitydb, orderingdb, webhooksdb

### Cache & Messaging
- **Redis:** Docker container via Aspire
- **RabbitMQ:** Docker container with persistent lifetime

## Deployment

### CI/CD Pipeline
- **Platform:** Azure DevOps (1ES Pipeline Templates)
- **Trigger:** Push to `main` branch (batch mode)
- **Tests:** `dotnet build eShop.Web.slnf`
- **E2E:** Playwright tests (addItem, browseItem, removeItem)

### Environments
- **Development:** Local via `dotnet run --project src/eShop.AppHost` with Docker
- **CI:** Azure DevOps with Windows build agents (NetCore1ESPool-Svc-Internal)
