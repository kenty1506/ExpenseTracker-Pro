# Phase 1: Reliability and security baseline

## Implemented

### Secret and configuration protection

- Removed the tracked JWT signing value.
- Added startup checks for signing-secret length and configuration validity.
- Rejects a token-shaped value accidentally supplied as a signing key.
- Disabled development data seeding by default.
- Expanded ignore rules for local databases, secrets, logs, test output, and
  generated files.

### Authentication hardening

- Access-token lifetime reduced to 15 minutes.
- Added 30-day rotating refresh tokens.
- Only a SHA-256 refresh-token hash is stored in SQL Server.
- Added authenticated logout and refresh-token revocation.
- Added five-attempt login lockout for 15 minutes.
- Increased password length and complexity requirements.
- Added validation to authentication request DTOs.
- Added Google Identity Services registration/sign-in with server-side ID-token
  verification.
- Added passwordless mobile registration/login through Twilio Verify.
- Added SendGrid forgot-password delivery with 30-minute reset tokens.
- Successful password resets revoke the stored refresh session.
- Added filtered unique email and E.164 mobile-number database indexes.

API additions:

- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `POST /api/v1/auth/google`
- `POST /api/v1/auth/mobile/register/request-code`
- `POST /api/v1/auth/mobile/register/verify`
- `POST /api/v1/auth/mobile/login/request-code`
- `POST /api/v1/auth/mobile/login/verify`
- `POST /api/v1/auth/forgot-password`
- `POST /api/v1/auth/reset-password`

The login response remains compatible through its existing `token` field and
now also returns `tokenExpiresAtUtc` and `refreshToken`.

### API and data protection

- Global limit: 120 requests per client per minute.
- Registration/login limit: 5 requests per client per minute.
- Mobile and password-reset verification limit: 3 requests per client per
  minute.
- Refresh/logout limit: 10 requests per client per minute.
- Added `X-Correlation-ID` request/response tracing.
- Added safe security response headers.
- Removed detailed database exception text from readiness responses.
- Added database audit records for POST, PUT, PATCH, and DELETE requests without
  storing request bodies or credentials.
- Added authenticated, user-scoped module and consolidated audit-trail APIs.
- Audit events now include module, operation, entity ID when available,
  success status, request duration, and correlation ID.
- Added SQL Server row-version concurrency tokens to financial entities.

### Delivery safety

- Added automated identity, validation, and concurrency-model tests.
- Added CI checks for build, tests, coverage collection, committed runtime
  artifacts, and committed JWT secrets.
- Added repository security and setup documentation.

## Required first-run steps

1. Rotate the old signing value because it was present in the earlier archive.
2. Store the new signing secret in User Secrets or `Jwt__Key`.
3. Review the configured SQL Server connection.
4. Apply all migrations through
   `20260718143000_AddExternalAuthenticationSupport`.
5. Run `dotnet test ExpenseTracker.Pro.sln`.
6. Test login, refresh, logout, and simultaneous transaction updates in a
   non-production database.

## Optional development sample data

Set `DevelopmentSeeder:Enabled` to `true` and keep the email non-production.
An existing registered user does not require a configured password. A password
is only required when the seeder must create the user:

```bash
dotnet user-secrets set "DevelopmentSeeder:Password" "<strong-local-password>"
```

Never enable this seeder in production.

## Remaining Phase 1 work

These items need deployment-specific decisions and should be completed next:

- Authenticated linking between existing local and Google accounts
- Email confirmation for new local email/password registrations
- Multi-factor authentication
- Centralized production log destination and retention policy
- Audit-log retention/archival job and authorized audit viewer
- Production database backup and restore drill
- Dependency vulnerability scanning and deployment pipeline
- End-to-end tests using a temporary SQL Server instance
