# Keystone Identity

Keystone Identity is a modular authentication service built with ASP.NET Core and Clean Architecture principles. The goal of this project is to progressively implement the core features of a modern Identity Provider (IdP), eventually supporting OAuth 2.0 and OpenID Connect (OIDC).

This project is being built as a learning exercise with an emphasis on production-style architecture rather than simply following tutorials.

---

## Current Features

### User Registration
- Register new users
- Prevent duplicate usernames and email addresses
- Secure password hashing
- User persistence using Entity Framework Core

### Authentication
- User login with username/email and password
- Password verification
- JWT Access Token generation
- Access token expiration support

### Refresh Tokens
- Cryptographically secure refresh token generation
- Refresh token persistence in the database
- Refresh token rotation
- Automatic revocation of previous refresh tokens during login
- Refresh endpoint for obtaining new access tokens
- Refresh token expiration tracking

---

## Project Structure

### API
Responsible for:
- HTTP endpoints
- Request validation
- Dependency Injection
- Authentication middleware

### Core
Contains:
- Domain models
- DTOs
- Interfaces
- Business services
- Authentication logic

### Infrastructure
Contains:
- Entity Framework Core
- Repository implementations
- JWT token generation
- Password hashing
- Database access

---

## Architecture

```
Client
    │
    ▼
API
    │
    ▼
AuthService
    ├──────────────► UserRepository
    ├──────────────► RefreshTokenRepository
    ├──────────────► PasswordHasher
    └──────────────► JwtTokenService
```

The project follows dependency inversion:
- API depends on Core
- Infrastructure depends on Core
- Core has no dependency on Infrastructure

---

## Technologies

- ASP.NET Core
- C#
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Clean Architecture
- Repository Pattern
- Dependency Injection

---

## Implemented Authentication Flow

### Registration
1. Validate request
2. Check for existing username/email
3. Hash password
4. Save user
5. Return registration response

### Login
1. Validate credentials
2. Verify password
3. Revoke existing refresh tokens
4. Generate JWT access token
5. Generate refresh token
6. Persist refresh token
7. Return authentication response

### Token Refresh
1. Validate refresh token
2. Check expiration
3. Check revocation status
4. Generate new access token
5. Generate new refresh token
6. Revoke previous refresh token
7. Persist new refresh token

---

## Upcoming Features

- Role-based authorization
- Claims and permissions
- Logout endpoint
- JWT authentication middleware
- Protected endpoints
- OAuth 2.0 Clients
- OpenID Connect (OIDC)
- Scopes
- Client Credentials Flow
- Authorization Code Flow
- Background service for refresh token cleanup

---

## Status

🚧 Active Development

Keystone Identity is an ongoing project intended to evolve into a lightweight Identity Provider implementing modern authentication and authorization concepts.
