# Product Mission - eShop (AdventureWorks)

> Last Updated: 2026-02-17
> Version: 1.0.0

## Pitch

eShop (AdventureWorks) is a reference .NET application that helps .NET developers learn and implement microservices-based e-commerce architectures by providing a fully functional, production-style e-commerce platform built with .NET Aspire and modern cloud-native patterns.

## Users

### Primary Customers

- **.NET Developers:** Engineers learning microservices architecture, .NET Aspire, DDD, CQRS, and event-driven patterns through a real-world reference implementation.
- **Solution Architects:** Technical leaders evaluating .NET Aspire and cloud-native patterns for their own e-commerce or distributed systems.

### User Personas

**Developer Dan**
- **Role:** .NET Backend Developer
- **Context:** Building distributed systems at a mid-size company
- **Pain Points:** Lack of real-world microservices examples, difficulty understanding DDD in practice, unclear integration patterns between services
- **Goals:** Learn production-quality microservices patterns, understand .NET Aspire orchestration, study DDD aggregate design
- **Details:** Primarily uses Visual Studio or VS Code on Windows, works with Docker daily, familiar with EF Core and ASP.NET Core

**Architect Ana**
- **Role:** Solutions Architect
- **Context:** Evaluating technology stacks for new e-commerce platform
- **Pain Points:** Need to see complete, working microservices systems rather than isolated samples
- **Goals:** Understand service decomposition, inter-service communication patterns, and .NET Aspire hosting model
- **Details:** Reviews architecture decisions, evaluates scalability patterns, needs to present options to stakeholders

## The Problem

### Learning Microservices Architecture in Practice

Developers struggle to understand how microservices, DDD, CQRS, and event-driven architecture work together in a realistic application. Isolated samples don't show the complexity of service interaction, data consistency, and operational concerns.

**Our Solution:** A complete, runnable e-commerce application demonstrating all these patterns working together with .NET Aspire for orchestration, PostgreSQL for persistence, RabbitMQ for messaging, and Redis for caching.

## Differentiators

### Complete Reference Architecture

Unlike simple "Todo" or single-service samples, eShop provides a full microservices ecosystem with identity management, catalog browsing, shopping basket, order processing, payment handling, and webhooks — all orchestrated through .NET Aspire with proper observability.

### Production-Quality Patterns

Unlike tutorial-grade code, eShop implements real DDD aggregates, CQRS with MediatR, integration events, Unit of Work, and proper separation of concerns that teams can directly adopt for production systems.

## Key Features

### Core Features

- **Product Catalog:** Browse, search, and filter products with paginated results and AI-powered search capabilities (pgvector)
- **Shopping Basket:** Redis-backed basket management via gRPC service
- **Order Management:** Full order lifecycle with DDD aggregates, CQRS commands/queries, and domain events
- **Payment Processing:** Asynchronous payment handling through integration events
- **Identity & Authentication:** Duende IdentityServer with ASP.NET Core Identity, OpenID Connect, and JWT Bearer

### Collaboration Features

- **Webhook System:** Subscribe to and receive notifications for order status changes and catalog updates
- **AI Chatbot:** Optional Azure OpenAI / Ollama integration for natural language product search
- **Mobile BFF:** YARP-based reverse proxy providing a mobile-optimized API gateway
