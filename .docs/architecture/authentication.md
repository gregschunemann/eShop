# Authentication & Authorization Architecture - eShop

> Last Updated: 2026-02-17

## Overview

eShop uses Duende IdentityServer as a centralized identity provider, implementing OpenID Connect and OAuth2 protocols. ASP.NET Core Identity manages user accounts, and JWT Bearer tokens secure API endpoints.

## Identity Architecture

```mermaid
graph TB
  subgraph "Identity API"
    IdentityServer["Duende IdentityServer 7.3"]
    ASPNetIdentity["ASP.NET Core Identity"]
    IdentityDB["PostgreSQL (identitydb)"]
    Views["MVC Views<br/>(Login, Consent, Logout)"]
  end

  subgraph "Client Applications"
    WebApp["WebApp<br/>(OpenID Connect)"]
    WebhookClient["WebhookClient<br/>(OpenID Connect)"]
    BasketAPI["Basket API<br/>(JWT Bearer)"]
    OrderingAPI["Ordering API<br/>(JWT Bearer)"]
    WebhooksAPI["Webhooks API<br/>(JWT Bearer)"]
  end

  WebApp -->|Authorization Code + PKCE| IdentityServer
  WebhookClient -->|Authorization Code + PKCE| IdentityServer
  BasketAPI -->|Validate JWT| IdentityServer
  OrderingAPI -->|Validate JWT| IdentityServer
  WebhooksAPI -->|Validate JWT| IdentityServer
  IdentityServer --> ASPNetIdentity
  ASPNetIdentity --> IdentityDB
```

## Authentication Flows

### Web Application (OpenID Connect)

```mermaid
sequenceDiagram
  participant User
  participant WebApp
  participant IdentityAPI

  User->>WebApp: Access protected page
  WebApp->>IdentityAPI: Redirect to /connect/authorize
  IdentityAPI->>User: Show login page
  User->>IdentityAPI: Enter credentials
  IdentityAPI->>IdentityAPI: Validate credentials
  IdentityAPI->>WebApp: Redirect with auth code
  WebApp->>IdentityAPI: POST /connect/token
  IdentityAPI-->>WebApp: ID Token + Access Token
  WebApp->>WebApp: Create auth session
  WebApp-->>User: Authenticated content
```

### API Authorization (JWT Bearer)

```mermaid
sequenceDiagram
  participant Client
  participant API
  participant IdentityAPI

  Client->>API: Request with Bearer token
  API->>API: Validate JWT signature
  API->>API: Check claims/scopes
  alt Token Valid
    API-->>Client: 200 OK + Response
  else Token Invalid
    API-->>Client: 401 Unauthorized
  end
```

## Registered Clients

The Identity API maintains callback URLs for all client applications:

| Client | Type | Callback URLs |
|--------|------|---------------|
| WebApp | Web Application | Self-referencing endpoint |
| WebhookClient | Web Application | Self-referencing endpoint |
| Basket API | API Resource | HTTP endpoint |
| Ordering API | API Resource | HTTP endpoint |
| Webhooks API | API Resource | HTTP endpoint |

## Identity Configuration

- **Token Signing:** Temporary development key (`tempkey.jwk`) — not for production use
- **User Management:** ASP.NET Core Identity with EF Core PostgreSQL store
- **User Secrets:** Configured for development environment
- **Views:** Server-rendered MVC views for login, consent, logout, device authorization, and diagnostics

## Security Considerations

- JWT Bearer tokens used for service-to-service authentication
- OpenID Connect with PKCE for web application authentication
- Identity URL passed via environment variables for service discovery
- Cyclic references between Identity API and client apps handled via Aspire environment configuration
