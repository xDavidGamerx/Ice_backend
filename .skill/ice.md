# Ice Project Evolution

## Project Overview
**Ice Backend** is a high-performance gaming backend built with .NET 8 and PostgreSQL, following Clean Architecture principles. It handles player identity, external authentication (Microsoft/Google), and a comprehensive cosmetics system.

## Technical Stack
- **Runtime**: .NET 8.0
- **Database**: PostgreSQL
- **Architecture**: Clean Architecture (Api, Application, Domain, Infrastructure)
- **Primary Features**:
    - Identity System (Premium/Ice UUIDs)
    - OAuth Integration (Microsoft, Google)
    - Cosmetics System (Hats, Wings, Capes, etc.)
    - Asset Management with SHA256 verification

## Project Structure
- `src/IceBackend.Api`: Entry point and Controllers.
- `src/IceBackend.Application`: Business logic and interfaces.
- `src/IceBackend.Domain`: Entities and value objects.
- `src/IceBackend.Infrastructure`: Data access and external services.
- `docs/database.sql`: Current database schema.

## Development Guidelines
1. **Purity**: Keep Domain free of external dependencies.
2. **Persistence**: All database changes must be reflected in `docs/database.sql`.
3. **Security**: Handle password hashes and tokens with care (SHA256/Session hashing).
4. **Consistency**: Use the established naming conventions for entities and assets.

## Current State
- [x] Initial Clean Architecture structure.
- [x] Database schema defined for Identity and Cosmetics.
- [x] EF Core & PostgreSQL infrastructure configuration.
- [x] Health Check implementation (/health endpoint).
- [ ] Implementation of Auth Providers.
- [ ] Implementation of Asset Delivery API.
- [ ] Database migrations execution.
