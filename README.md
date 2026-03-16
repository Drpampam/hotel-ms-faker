# Hotelier Core App

> Multi-tenant hospitality backbone: Identity, Access, Subscription, Module, Property & Operational Intelligence – built on a scalable, secure, API-first platform.

---
## 1. Vision & Value Proposition
Hotelier Core App is the foundational backend for next-generation hospitality SaaS solutions. It enables rapid launch of Property Management Systems (PMS), guest engagement platforms, channel management, subscription billing, and operational tooling.

Designed for:
- Hotel groups scaling digital operations
- SaaS startups entering hospitality tech
- Enterprise integrations needing secure, composable core services

Key Differentiators:
- True multi-tenancy (schema isolation) for compliance & data segregation
- Modular architecture for feature expansion (modules, subscriptions, policies)
- Enterprise-grade identity, RBAC, auditing, and extensibility baked in
- Cloud-native design ready for container orchestration & global scaling

---
## 2. Market Opportunity (Investor Snapshot)
The global hotel technology market is shifting toward API-first, modular ecosystems. Legacy PMS vendors are monolithic and slow to innovate.

Opportunity Drivers:
- Fragmented operational tooling (guest apps, housekeeping, revenue management)
- Rising demand for real-time analytics & automation
- Increasing compliance and data governance pressure (multi-tenancy & audit trails)
- Subscription business models with tiered feature activation

Hotelier Core App positions itself as the accelerating engine for vertical hospitality platforms.

---
## 3. Architecture Overview
Layered architecture promotes separation of concerns, testability, and extensibility.

| Layer | Purpose |
|-------|---------|
| API (`*.API`) | Controllers, middleware, versioning, Swagger, auth bootstrap |
| Services (`*.Service`) | Business logic, orchestration, transactional boundaries |
| Repository (`*.Repository`) | EF Core & Dapper data access, query optimizations |
| Model (`*.Model`) | Domain entities, DTOs, configuration contracts |
| Migrations (`*.Migrations`) | EF Core DbContext, tenant schema binding, seeding |
| Core (`*.Core`) | Cross-cutting (constants, interceptors, DI modules) |
| Test (`*.Test`) | Unit/integration tests (future expansion) |

Patterns & Practices:
- Dependency Injection via Autofac modules
- EF Core for ORM + Dapper for targeted performance paths
- DTO mapping with AutoMapper
- FluentValidation pipeline integration
- API Versioning (current: v1)
- Global exception handling returning consistent `BaseResponse`

---
## 4. Multi-Tenancy Strategy
Approach: Schema-per-tenant isolation within a single PostgreSQL instance.

Mechanism:
- `TenantMiddleware` extracts `X-Tenant-Id` header
- Maps to schema: `tenant_{id}`; fallback: `public`
- `AppDbContext.OnModelCreating` dynamically sets schema per request lifetime

Benefits:
- Logical isolation without connection overhead
- Easier backup & migration per tenant (schema-level operations)
- Future flexibility: upgrade path to database-per-tenant

Header Usage:
```
X-Tenant-Id: 37  =>  schema tenant_37
```

---
## 5. Security & Compliance Model
Security Layers:
- Authentication: JWT Bearer tokens (configurable issuer/audience)
- Authorization: Role-based & policy-based (e.g., `DeveloperPolicy`, `AdminPolicy`)
- Identity: ASP.NET Core Identity (`ApplicationUser`, `ApplicationRole`)
- Auditing: `AuditLog` captured for sensitive mutations (who, what, when, where)
- Concurrency: RowVersion handled via EF Core on `ApplicationUser`

Configurable via `JwtTokenSettings`:
```
TokenKey      => Symmetric signing key (rotate periodically)
TokenIssuer   => Issuer & audience alignment
TokenExpiry   => Access token lifetime (minutes)
```

Roadmap Enhancements:
- Refresh token rotation hardening
- Password policy configuration endpoints
- SSO / OpenID Connect provider integration
- Field-level encryption for PII
- GDPR/PCI alignment (data retention & masking)

---
## 6. Core Domain Features
Current Implemented:
- User lifecycle (create, activate, deactivate, login, refresh token)
- Roles & role management endpoints
- Policy groups & permission scaffolding (extensible)
- Subscription plan creation, assignment to tenants
- Modules & module groups for progressive feature rollout
- Foundational entities: Property, Room, Reservation, Payment, Discount, Loyalty, ServiceRequest
- Audit logging for traceability
- Localization (`en-GB`, `en-US`)

Extensible Foundations:
- Add services for reservations, pricing rules, occupancy analytics
- Introduce workflow engines (e.g., housekeeping task automation)

---
## 7. Technology Stack
- Runtime: .NET 8 | Language: C# 12
- Web/API: ASP.NET Core Web API
- Data: PostgreSQL 15+, EF Core, Dapper
- Identity & Auth: ASP.NET Core Identity + JWT
- DI Container: Autofac
- Validation: FluentValidation
- Mapping: AutoMapper
- API Versioning: Asp.Versioning
- Documentation: Swagger/OpenAPI
- Infrastructure: Docker, docker-compose, optional Nginx reverse proxy

---
## 8. Operational Excellence
Included:
- Health checks (`/healthz` outside development)
- Configurable DB read timeouts
- Structured exception middleware
- Environment overrides for configuration (`ConnectionStrings__DbConnectionString` etc.)

Planned:
- Structured logging (Serilog) + correlation IDs
- Distributed tracing (OpenTelemetry)
- Metrics export (Prometheus scraping)
- Rate limiting & defensive throttling

---
## 9. Deployment & Scaling Strategy
Phase 1: Docker compose (local / PoC)
Phase 2: Container registry + CI/CD pipeline
Phase 3: Orchestrated (Kubernetes / Helm) with:
- Horizontal Pod Autoscaling (stateless API layer)
- Managed PostgreSQL (read replicas for analytics)
Phase 4: Global multi-region + traffic steering (Edge CDN, geolocation routing)

Future Data Strategy:
- Shard high-volume tenant schemas
- Optional database-per-enterprise-tier tenant
- Read model projections for analytics (CQRS-lite)

---
## 10. Getting Started (Local Development)
Prerequisites:
- .NET 8 SDK
- PostgreSQL 15+
- Docker (optional)

Configure:
Edit: `src/hotelier-core-app.API/appsettings.json`
```
"ConnectionStrings": {
  "DbConnectionString": "Host=localhost;Port=5432;Database=Hotelier.Core;Username=postgres;Password=YourPassword;Include Error Detail=true"
}
```
Run:
```
dotnet restore
dotnet build
dotnet run --project src/hotelier-core-app.API
```
Access:
- Swagger: `/swagger`
- Auth endpoints: `/api/v1/user/login`

Docker:
```
setx POSTGRES_PASSWORD YourPassword
docker compose up --build
```

---
## 11. Configuration Reference
| Key | Purpose |
|-----|---------|
| ConnectionStrings:DbConnectionString | PostgreSQL DSN |
| AppSettings:OrmType | Primary ORM selection (e.g., SQL_EFCore) |
| JwtTokenSettings:TokenKey | JWT signing secret |
| JwtTokenSettings:TokenIssuer | Issuer & audience alignment |
| JwtTokenSettings:TokenExpiryPeriod | Token lifetime (minutes) |

Environment variable override pattern:
```
ConnectionStrings__DbConnectionString
JwtTokenSettings__TokenKey
JwtTokenSettings__TokenIssuer
```

---
## 12. Selected API Endpoints (v1)
Users (`/api/v1/user`):
- POST `/login`
- POST `/create-user`
- PUT  `/activate-user`
- PUT  `/deactivate-user`
- POST `/refresh-token?currentRefreshToken=...`

Roles (`/api/v1/role`):
- POST `/create-role`
- PUT  `/update-role`
- GET  `/{id}`
- GET  `/`
- DELETE `/{id}`

Subscriptions (`/api/v1/subscription`):
- POST `/create-plan` (Developer policy)
- GET  `/{id}` (AllowAnonymous)
- GET  `/` (AllowAnonymous)
- DELETE `/{id}` (Developer policy)
- POST `/subscribe`

Modules (`/api/v1/module`):
- POST `/module-groups`
- PUT  `/module-groups/{id}`
- DELETE `/module-groups/{id}`
- GET  `/module-groups`
- POST `/`
- PUT  `/{id}`
- DELETE `/{id}`
- GET  `/`

Headers:
```
Authorization: Bearer <token>
X-Tenant-Id: <tenantId>
```

---
## 13. Data Model Snapshot (Representative)
Entities include:
- `ApplicationUser`: Identity, tenant link, refresh token, auditing fields
- `Tenant`: Logical grouping & isolation indicator
- `SubscriptionPlan`: Name, pricing metadata (extensible)
- `Module / ModuleGroup`: Feature segmentation
- `Reservation, Room, Property`: Domain scaffolding for PMS features
- `AuditLog`: Action traceability

Future Enhancements:
- RatePlan, InventoryBlock, HousekeepingTask, GuestProfile

---
## 14. Roadmap (Phased)
Phase 0 (Done): Core identity, modules, subscriptions, multi-tenancy, auditing
Phase 1: Reservations service, property onboarding, basic reporting
Phase 2: Pricing & revenue engine, housekeeping workflows, payment integrations
Phase 3: Channel manager adapters (OTA sync), loyalty tiers, guest engagement APIs
Phase 4: AI/ML recommendations (rate optimization), anomaly detection, predictive maintenance
Phase 5: Marketplace platform (3rd-party module publishing)

---
## 15. KPIs & Investor Metrics (Target Examples)
- Tenant Onboarding Time: < 5 minutes (automated provisioning)
- Average API Latency (P95): < 150ms
- Uptime SLA (Phase 3+): 99.95%
- Data Isolation Incidents: 0 (schema isolation enforcement)
- Feature Activation Lead Time: < 1 day (modular deployment)

---
## 16. Monetization Model (Strategic Direction)
- Base Subscription: Core identity + modules + tenant provisioning
- Tiered Plans: Feature gates (advanced reservations, analytics, loyalty)
- Usage Add-ons: API call volume, premium reporting, automation workflows
- Marketplace Revenue Share: 3rd-party integrations sold through platform

---
## 17. Development & Contribution
Internal Contributions:
- Follow conventional commits (`feat:`, `fix:` ...)
- Enforce code review & automated tests (pipeline target)

External (Future):
- Contributor guide
- API schema references / Postman collection
- License finalization (commercial + optional OSS components)

---
## 18. Scaling & Engineering Enhancements (Planned)
- Introduce Redis for caching hot tenant configs
- Adopt Outbox pattern + message broker (Kafka/RabbitMQ) for eventual consistency
- Event-driven module activation
- Introduce gRPC or GraphQL for performance & flexibility in client integrations

---
## 19. Risk & Mitigation
| Risk | Mitigation |
|------|------------|
| Single DB saturation | Shard tenants / read replicas |
| Token compromise | Short-lived access tokens + refresh rotation |
| Feature sprawl | Strict module lifecycle & governance |
| Compliance escalation | Audit log & schema isolation foundation |

---
## 20. License
Currently UNLICENSED. For external distribution add MIT / Apache-2.0 or proprietary license strategy.

---
## 21. Contact
For partnership, integration, or investment inquiries: Reach out to project maintainers.

---
## Quick Start TL;DR
```
# Run locally
dotnet run --project src/hotelier-core-app.API
# Login then call authorized endpoints
curl -X POST https://localhost:44366/api/v1/user/login -d '{"email":"admin@hotelier.io","password":"P@ssw0rd!"}' -H "Content-Type: application/json" -i
```
Add headers: `Authorization: Bearer <Token>` and `X-Tenant-Id: 1` for protected, tenant-scoped operations.

---
## Future Strategic Expansion
- AI-powered dynamic pricing & demand forecasting
- Unified operations dashboard (cross-tenant metrics)
- Partner integration toolkit (SDK + webhook framework)
- Data warehouse export + BI connectors (Snowflake, BigQuery)

---
Empowering hospitality platforms to build faster, scale confidently, and innovate continuously.
