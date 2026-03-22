# Database Architecture - eShop

> Last Updated: 2026-02-17

## Overview

eShop uses PostgreSQL as its primary relational database with the pgvector extension for AI-powered semantic search. Each bounded context owns its own logical database within a shared PostgreSQL instance.

## Database Topology

```mermaid
graph TB
  subgraph "PostgreSQL Instance (ankane/pgvector)"
    CatalogDB["catalogdb"]
    IdentityDB["identitydb"]
    OrderingDB["orderingdb"]
    WebhooksDB["webhooksdb"]
  end

  CatalogAPI["Catalog API"] --> CatalogDB
  IdentityAPI["Identity API"] --> IdentityDB
  OrderingAPI["Ordering API"] --> OrderingDB
  WebhooksAPI["Webhooks API"] --> WebhooksDB
  OrderProcessor["Order Processor"] --> OrderingDB
```

## Database Schemas

### catalogdb — Catalog Database

Stores product catalog data including items, brands, types, and vector embeddings for semantic search.

```mermaid
erDiagram
  CatalogItem {
    int Id PK
    string Name
    string Description
    decimal Price
    string PictureFileName
    int CatalogTypeId FK
    int CatalogBrandId FK
    int AvailableStock
    int RestockThreshold
    int MaxStockThreshold
    vector Embedding
  }
  CatalogType {
    int Id PK
    string Type
  }
  CatalogBrand {
    int Id PK
    string Brand
  }
  CatalogType ||--o{ CatalogItem : "has"
  CatalogBrand ||--o{ CatalogItem : "has"
```

**Key Features:**
- pgvector `Embedding` column on `CatalogItem` for AI semantic search
- Stock management fields for inventory tracking
- EF Core with Npgsql provider

### identitydb — Identity Database

Managed by Duende IdentityServer and ASP.NET Core Identity. Stores users, roles, claims, and IdentityServer operational/configuration data.

**Tables (managed by frameworks):**
- ASP.NET Core Identity: `AspNetUsers`, `AspNetRoles`, `AspNetUserClaims`, `AspNetUserRoles`, etc.
- Duende IdentityServer: `Clients`, `ApiResources`, `ApiScopes`, `PersistedGrants`, etc.

### orderingdb — Ordering Database

Implements DDD patterns with a dedicated `ordering` schema. Uses EF Core with explicit entity configurations.

```mermaid
erDiagram
  Order {
    int Id PK
    datetime OrderDate
    int OrderStatusId
    string Description
    int BuyerId FK
    int PaymentMethodId FK
    string Street
    string City
    string State
    string Country
    string ZipCode
  }
  OrderItem {
    int Id PK
    int OrderId FK
    int ProductId
    string ProductName
    decimal UnitPrice
    decimal Discount
    int Units
    string PictureUrl
  }
  Buyer {
    int Id PK
    string IdentityGuid
    string Name
  }
  PaymentMethod {
    int Id PK
    int BuyerId FK
    string Alias
    string CardNumber
    string SecurityNumber
    string CardHolderName
    datetime Expiration
    int CardTypeId FK
  }
  CardType {
    int Id PK
    string Name
  }
  ClientRequest {
    string Id PK
    string Name
    datetime Time
  }

  Order ||--o{ OrderItem : "contains"
  Buyer ||--o{ Order : "places"
  Buyer ||--o{ PaymentMethod : "has"
  CardType ||--o{ PaymentMethod : "type of"
```

**Schema:** `ordering`
**Entity Configurations (Fluent API):**
- `OrderEntityTypeConfiguration`
- `OrderItemEntityTypeConfiguration`
- `BuyerEntityTypeConfiguration`
- `PaymentMethodEntityTypeConfiguration`
- `CardTypeEntityTypeConfiguration`
- `ClientRequestEntityTypeConfiguration` (idempotency)

**Integration Event Log:** Uses `IntegrationEventLogEF` for durable event publishing via `UseIntegrationEventLogs()`.

### webhooksdb — Webhooks Database

Stores webhook subscriptions and delivery records.

## Cache Layer — Redis

```mermaid
graph LR
  BasketAPI["Basket API"] -->|Read/Write| Redis["Redis"]
  Redis -->|CustomerBasket JSON| BasketAPI
```

- **Purpose:** Stores shopping baskets as serialized `CustomerBasket` objects
- **Key Pattern:** Customer/buyer ID as key
- **Model:** `CustomerBasket` → `List<BasketItem>`
- **Persistence:** Non-persistent (cache-only, session lifetime)

## Data Access Patterns

| Service | ORM | Pattern |
|---------|-----|---------|
| Catalog API | EF Core (Npgsql) | DbContext direct queries + pgvector |
| Ordering API | EF Core (Npgsql) | Repository pattern + Unit of Work |
| Ordering Queries | Dapper | Lightweight read-model queries |
| Identity API | EF Core (Npgsql) | ASP.NET Core Identity + IdentityServer |
| Basket API | StackExchange.Redis | Key-value serialization |
| Webhooks API | EF Core (Npgsql) | DbContext |

## Migration Strategy

- **Tool:** `dotnet ef migrations`
- **Location:** `Ordering.Infrastructure/Migrations/`
- **Command:** `dotnet ef migrations add --startup-project Ordering.API --context OrderingContext [migration-name]`
- **Catalog:** Seed data loaded from JSON files (`Setup/` folder)
