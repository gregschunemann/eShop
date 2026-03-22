# Domain-Driven Design & CQRS Patterns - eShop

> Last Updated: 2026-02-17

## Overview

The Ordering bounded context is the primary DDD implementation in eShop. It demonstrates aggregates, value objects, domain events, repositories, and CQRS with MediatR. Other services (Catalog, Basket) use simpler patterns appropriate to their complexity.

## Ordering Bounded Context

### Layer Architecture

```mermaid
graph TB
  subgraph "Ordering.API"
    Controllers["API Endpoints"]
    Commands["Commands<br/>(MediatR)"]
    Queries["Queries<br/>(Dapper)"]
    Behaviors["Behaviors<br/>(Pipeline)"]
    Validations["Validations<br/>(FluentValidation)"]
    IntEvents["Integration Event<br/>Handlers"]
  end

  subgraph "Ordering.Domain"
    Aggregates["Aggregates"]
    DomainEvents["Domain Events"]
    SeedWork["SeedWork<br/>(Base types)"]
  end

  subgraph "Ordering.Infrastructure"
    DbContext["OrderingContext"]
    Repos["Repositories"]
    EntityConfig["Entity<br/>Configurations"]
    Migrations["EF Migrations"]
  end

  Controllers --> Commands
  Controllers --> Queries
  Commands --> Aggregates
  Commands --> Repos
  Queries --> DbContext
  Behaviors --> Commands
  Validations --> Commands
  Aggregates --> DomainEvents
  Aggregates --> SeedWork
  Repos --> DbContext
  DbContext --> EntityConfig
  IntEvents --> Commands
```

### Aggregate Design

```mermaid
classDiagram
  class Entity {
    <<abstract>>
    +int Id
    +List~INotification~ DomainEvents
    +AddDomainEvent()
    +RemoveDomainEvent()
    +ClearDomainEvents()
  }

  class IAggregateRoot {
    <<interface>>
  }

  class ValueObject {
    <<abstract>>
    +GetEqualityComponents()*
    +Equals()
    +GetHashCode()
  }

  class Order {
    +DateTime OrderDate
    +Address Address
    +int? BuyerId
    +OrderStatus OrderStatus
    +string Description
    +IReadOnlyCollection~OrderItem~ OrderItems
    +NewDraft() Order
    +AddOrderItem()
    +SetAwaitingValidationStatus()
    +SetStockConfirmedStatus()
    +SetPaidStatus()
    +SetShippedStatus()
    +SetCancelledStatus()
    +GetTotal() decimal
  }

  class OrderItem {
    +string ProductName
    +string PictureUrl
    +decimal UnitPrice
    +decimal Discount
    +int Units
    +int ProductId
    +SetNewDiscount()
    +AddUnits()
  }

  class Address {
    +string Street
    +string City
    +string State
    +string Country
    +string ZipCode
  }

  class Buyer {
    +string IdentityGuid
    +string Name
    +IReadOnlyCollection~PaymentMethod~ PaymentMethods
    +VerifyOrAddPaymentMethod()
  }

  class PaymentMethod {
    +string Alias
    +string CardNumber
    +string SecurityNumber
    +string CardHolderName
    +DateTime Expiration
    +CardType CardType
    +IsEqualTo()
  }

  class CardType {
    <<enumeration>>
    +Amex
    +Visa
    +MasterCard
  }

  class OrderStatus {
    <<enumeration>>
    +Submitted
    +AwaitingValidation
    +StockConfirmed
    +Paid
    +Shipped
    +Cancelled
  }

  Entity <|-- Order
  Entity <|-- OrderItem
  Entity <|-- Buyer
  Entity <|-- PaymentMethod
  IAggregateRoot <|.. Order
  IAggregateRoot <|.. Buyer
  ValueObject <|-- Address
  Order *-- OrderItem
  Order --> Address
  Order --> OrderStatus
  Order --> Buyer
  Buyer *-- PaymentMethod
  PaymentMethod --> CardType
```

### Domain Events

Domain events are raised by aggregates and dispatched by the `MediatorExtension` before saving changes to the database.

| Event | Raised By | Purpose |
|-------|-----------|---------|
| `OrderStartedDomainEvent` | Order (constructor) | Triggers buyer/payment verification |
| `OrderCancelledDomainEvent` | `SetCancelledStatus()` | Notifies of cancellation |
| `OrderShippedDomainEvent` | `SetShippedStatus()` | Notifies of shipment |
| `OrderStatusChangedToAwaitingValidationDomainEvent` | `SetAwaitingValidationStatus()` | Triggers stock validation |
| `OrderStatusChangedToStockConfirmedDomainEvent` | `SetStockConfirmedStatus()` | Stock confirmed |
| `OrderStatusChangedToPaidDomainEvent` | `SetPaidStatus()` | Payment confirmed |
| `BuyerPaymentMethodVerifiedDomainEvent` | Buyer aggregate | Payment method validated |

### CQRS Command Flow

```mermaid
sequenceDiagram
  participant API as API Endpoint
  participant Mediator as MediatR
  participant Validator as FluentValidation<br/>Behavior
  participant Logger as Logging<br/>Behavior
  participant Handler as Command Handler
  participant Aggregate as Order Aggregate
  participant Repo as IOrderRepository
  participant UoW as Unit of Work
  participant Events as Domain Event<br/>Dispatcher

  API->>Mediator: Send(CreateOrderCommand)
  Mediator->>Logger: LoggingBehavior
  Logger->>Validator: ValidatorBehavior
  Validator->>Handler: CreateOrderCommandHandler
  Handler->>Aggregate: new Order(...)
  Aggregate->>Aggregate: AddDomainEvent(OrderStarted)
  Handler->>Repo: Add(order)
  Handler->>UoW: SaveEntitiesAsync()
  UoW->>Events: DispatchDomainEventsAsync()
  Events->>Mediator: Publish(OrderStartedDomainEvent)
  UoW->>UoW: SaveChangesAsync()
```

### Commands

| Command | Handler | Description |
|---------|---------|-------------|
| `CreateOrderCommand` | `CreateOrderCommandHandler` | Create new order from basket checkout |
| `CancelOrderCommand` | `CancelOrderCommandHandler` | Cancel an existing order |
| `ShipOrderCommand` | `ShipOrderCommandHandler` | Mark order as shipped |
| `SetAwaitingValidationOrderStatusCommand` | Handler | Transition to awaiting validation |
| `SetStockConfirmedOrderStatusCommand` | Handler | Confirm stock availability |
| `SetStockRejectedOrderStatusCommand` | Handler | Reject due to insufficient stock |
| `SetPaidOrderStatusCommand` | Handler | Mark order as paid |
| `CreateOrderDraftCommand` | `CreateOrderDraftCommandHandler` | Create draft order (preview) |
| `IdentifiedCommand<T>` | `IdentifiedCommandHandler<T>` | Idempotent command wrapper |

### Read Queries (Dapper)

Queries bypass the domain model and read directly from the database using Dapper for performance.

- `OrderQueries` implements `IOrderQueries`
- Returns `OrderViewModel` DTOs
- Used for listing orders and order details

## SeedWork Base Types

| Type | Purpose |
|------|---------|
| `Entity` | Base class with ID, domain event collection |
| `IAggregateRoot` | Marker interface for aggregate roots |
| `IRepository<T>` | Generic repository interface |
| `IUnitOfWork` | Unit of Work abstraction (`SaveEntitiesAsync`) |
| `ValueObject` | Base class for value objects with equality by components |

## Repository Pattern

```mermaid
classDiagram
  class IRepository~T~ {
    <<interface>>
    +IUnitOfWork UnitOfWork
  }

  class IOrderRepository {
    <<interface>>
    +Add(Order) Order
    +Update(Order)
    +GetAsync(int) Task~Order~
  }

  class IBuyerRepository {
    <<interface>>
    +Add(Buyer) Buyer
    +Update(Buyer)
    +FindAsync(string) Task~Buyer~
    +FindByIdAsync(int) Task~Buyer~
  }

  IRepository <|-- IOrderRepository
  IRepository <|-- IBuyerRepository
```

Repositories are implemented in `Ordering.Infrastructure/Repositories/` and inject `OrderingContext` for data access.
