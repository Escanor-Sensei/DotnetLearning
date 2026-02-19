# JWT Authentication & Role-Based Authorization

## What is Authentication vs Authorization?

**Authentication** answers "Who are you?" — verifying a user's identity through credentials (username/password, tokens, certificates). **Authorization** answers "What can you do?" — determining what resources an authenticated user can access.

In web APIs, these are separate concerns handled by different middleware in the request pipeline.

## Understanding JWT (JSON Web Tokens)

### What is a JWT?

A JWT is a compact, self-contained token format used for securely transmitting information between parties as a JSON object. It's digitally signed, so the information can be verified and trusted.

### JWT Structure

A JWT consists of three Base64-encoded parts separated by dots:

```
Header.Payload.Signature
```

- **Header** — Contains the token type (`JWT`) and the signing algorithm (e.g., `HS256`, `RS256`)
- **Payload** — Contains _claims_ — statements about the user and additional metadata
- **Signature** — Created by hashing the encoded header + payload with a secret key, ensuring token integrity

### Claims — The Core of JWT

Claims are key-value pairs embedded in the token payload. They carry identity and authorization data:

| Claim Type | Purpose | Example |
|---|---|---|
| `sub` (Subject) | Unique user identifier | `"123"` |
| `name` | Display name | `"john_doe"` |
| `role` | User's role(s) | `"Admin"` |
| `exp` (Expiration) | Token expiry timestamp | `1645123456` |
| `iss` (Issuer) | Who issued the token | `"TaskManagementAPI"` |
| `aud` (Audience) | Intended recipient | `"TaskManagementAPI-Users"` |
| `iat` (Issued At) | When token was created | `1645119856` |

**Registered claims** (`sub`, `exp`, `iss`, `aud`, `iat`) are predefined by the JWT spec. You can also define **custom claims** for application-specific data.

### Why JWT for APIs?

| Benefit | Explanation |
|---|---|
| **Stateless** | Server doesn't need to store session data — the token itself contains everything |
| **Scalable** | Works across multiple servers/microservices without shared session stores |
| **Self-contained** | Token carries user info, reducing database lookups on every request |
| **Cross-domain** | Works seamlessly across different domains and services |
| **Standard** | Industry-standard (RFC 7519), widely supported across languages and platforms |

### JWT vs Other Token Types

| Feature | JWT | Opaque Tokens | Session Cookies |
|---|---|---|---|
| Self-contained | Yes | No | No |
| Server storage needed | No | Yes (token store) | Yes (session store) |
| Scalability | High | Medium | Low |
| Revocation | Hard (needs blocklist) | Easy (delete from store) | Easy (delete session) |
| Cross-domain | Easy | Hard | Hard (CORS issues) |

## Token Lifecycle

```
1. Client sends credentials (login)
2. Server validates credentials against database
3. Server generates JWT with user claims
4. Server returns JWT to client
5. Client stores JWT (localStorage, cookie, memory)
6. Client sends JWT in Authorization header on subsequent requests
7. Server validates JWT signature + expiration on each request
8. Server extracts claims and authorizes the request
```

## Authentication Pipeline in ASP.NET Core

The authentication system in ASP.NET Core follows a layered architecture:

### Middleware Order (Critical)

The order of middleware registration is important because each middleware processes the request sequentially:

```
Request → Exception Handling → HTTPS Redirect → Routing → Authentication → Authorization → Controller
```

- **Authentication middleware** examines the incoming token and establishes the user identity (`HttpContext.User`)
- **Authorization middleware** checks whether the authenticated user has permission for the requested resource
- If authentication is placed after authorization, the system won't know who the user is when checking permissions

### Authentication Schemes

ASP.NET Core supports multiple authentication schemes simultaneously:
- **JWT Bearer** — Token-based authentication for APIs
- **Cookie** — Session-based for web apps
- **OAuth/OpenID Connect** — Third-party authentication (Google, Microsoft, etc.)
- **API Key** — Simple key-based authentication

A **default scheme** handles unauthenticated requests. Multiple schemes can coexist for different parts of an application.

## Role-Based Authorization (RBAC)

### Concept

RBAC restricts access based on roles assigned to users. Each user has one or more roles, and each resource/endpoint specifies which roles are allowed.

### Authorization Levels

| Level | Description | Use Case |
|---|---|---|
| **No attribute** | Public, no auth needed | Health checks, public data |
| **[Authorize]** | Any authenticated user | General user endpoints |
| **[Authorize(Roles = "Admin")]** | Specific role only | Administrative functions |
| **[Authorize(Roles = "User,Admin")]** | Any of listed roles | Shared functionality |

### Policy-Based Authorization (Advanced)

Beyond simple roles, ASP.NET Core supports **policies** which combine multiple requirements:
- **Claims-based** — User must have specific claims
- **Custom requirements** — Arbitrary logic (age verification, subscription level, etc.)
- **Resource-based** — Authorization depends on the specific resource being accessed

## Password Security

### Hashing vs Encryption

| Aspect | Hashing | Encryption |
|---|---|---|
| Reversible | No (one-way) | Yes (two-way) |
| Purpose | Verify passwords | Protect data in transit/storage |
| Key needed | No (uses salt) | Yes (encryption key) |
| Use for passwords | Yes | No |

### BCrypt — Industry Standard

BCrypt is the recommended password hashing algorithm because:
- **Adaptive** — Work factor can be increased as hardware improves
- **Salted** — Automatically generates and stores unique salts per password
- **Slow by design** — Makes brute-force attacks computationally expensive
- **Work factor** — Controls the computational cost (typically 10-12 rounds)

### Password Verification Flow

```
Registration: Password → BCrypt.HashPassword(password) → Store hash in DB
Login:        Password → BCrypt.Verify(password, storedHash) → true/false
```

## Token Validation Parameters

When the server receives a JWT, it validates several aspects:

| Parameter | What It Checks |
|---|---|
| **ValidateIssuerSigningKey** | Signature matches the expected signing key |
| **ValidateIssuer** | Token's `iss` claim matches expected issuer |
| **ValidateAudience** | Token's `aud` claim matches expected audience |
| **ValidateLifetime** | Token hasn't expired (`exp` claim) |
| **ClockSkew** | Allowed time difference between server clocks (default: 5 min) |

## High-Level Implementation Architecture

### Components Needed

1. **User Model** — Simple entity with Id, Username, PasswordHash, Role
2. **JWT Service** — Responsible for token generation; reads JWT settings (secret, issuer, audience, expiration) from configuration
3. **Auth Controller** — Exposes login/register endpoints; validates credentials, calls JWT service
4. **Middleware Configuration** — Registers JWT Bearer authentication scheme in `Program.cs` with validation parameters
5. **Endpoint Protection** — Apply `[Authorize]` and `[Authorize(Roles = "...")]` attributes to controllers/actions

### Configuration Requirements

- JWT settings stored in `appsettings.json` (SecretKey, Issuer, Audience, ExpirationMinutes)
- Secret key must be at least 256 bits (32 characters) for HMAC-SHA256
- Connection between configuration and JWT service through dependency injection

### Request Flow

```
POST /api/auth/login → Validate credentials → Generate JWT → Return token
GET /api/tasks (with Bearer token) → Validate JWT → Extract claims → Check authorization → Execute action
```

## HTTP Status Codes in Auth Context

| Code | Meaning | When Returned |
|---|---|---|
| `200 OK` | Successful login | Valid credentials, token returned |
| `401 Unauthorized` | Identity not established | Missing token, invalid token, expired token, bad credentials |
| `403 Forbidden` | Identity known, insufficient permissions | Valid token but wrong role for the endpoint |

## Security Best Practices

### Token Security
- **Short expiration** — 15-60 minutes for access tokens
- **Refresh tokens** — Long-lived tokens to get new access tokens without re-login
- **HTTPS only** — Never transmit tokens over unencrypted connections
- **Secure storage** — HttpOnly cookies preferred over localStorage (prevents XSS)

### Key Management
- **Strong secret keys** — Minimum 256-bit keys, randomly generated
- **Key rotation** — Periodically change signing keys
- **Environment-specific** — Different keys for dev/staging/production
- **Secret management** — Use Azure Key Vault, AWS Secrets Manager, etc. (never hardcode)

### Common Vulnerabilities

| Vulnerability | Description | Mitigation |
|---|---|---|
| **Token theft** | Attacker steals JWT from client | Short expiration, HTTPS, secure storage |
| **Brute force** | Repeated login attempts | Rate limiting, account lockout |
| **JWT alg:none** | Attacker removes signature | Always validate signing algorithm |
| **Weak secrets** | Predictable signing keys | Use cryptographically random keys |
| **Token replay** | Reusing stolen tokens | Short expiration, token binding |

## Advanced Concepts

### Refresh Token Pattern
- **Access token** — Short-lived (15 min), used for API calls
- **Refresh token** — Long-lived (days/weeks), stored securely, used only to get new access tokens
- When access token expires, client sends refresh token to get a new access token without re-entering credentials

### OAuth 2.0 & OpenID Connect
- **OAuth 2.0** — Authorization framework for delegated access (e.g., "Login with Google")
- **OpenID Connect** — Identity layer on top of OAuth 2.0, provides authentication
- **Flows** — Authorization Code (web apps), Client Credentials (server-to-server), PKCE (SPAs/mobile)

### Claims Transformation
- Middleware can modify or enrich claims after authentication
- Useful for adding database-driven permissions not stored in the token
- Example: Token has `userId` claim → middleware looks up user's permissions and adds them as claims

---

*JWT authentication provides a stateless, scalable security model for APIs. The key is understanding that the token itself carries identity information, and the server only needs to validate the signature to trust it.*