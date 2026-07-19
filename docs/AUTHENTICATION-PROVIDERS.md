# Google, mobile, and password-reset authentication

ExpenseTracker Pro now supports three production-oriented authentication
flows in addition to the existing email/password login:

- Google Identity Services registration and sign-in
- Passwordless mobile registration and sign-in through Twilio Verify SMS
- Forgot-password email delivery through SendGrid

The API starts normally when these optional providers are not configured, so
the existing email/password endpoints remain available. A provider endpoint
returns `503 Service Unavailable` when its required provider configuration is
missing, except forgot-password, which always returns the same generic `202`
response to prevent account discovery.

## Store provider configuration safely

Run these commands from the `ExpenseTracker.Api` directory in PowerShell.
Replace the placeholders locally and never commit the real values:

```powershell
dotnet user-secrets set "AuthenticationProviders:Google:ClientId" "<google-web-client-id>"
dotnet user-secrets set "AuthenticationProviders:TwilioVerify:AccountSid" "<twilio-account-sid>"
dotnet user-secrets set "AuthenticationProviders:TwilioVerify:AuthToken" "<twilio-auth-token>"
dotnet user-secrets set "AuthenticationProviders:TwilioVerify:ServiceSid" "<twilio-verify-service-sid>"
dotnet user-secrets set "AuthenticationProviders:SendGrid:ApiKey" "<sendgrid-api-key>"
dotnet user-secrets set "AuthenticationProviders:SendGrid:FromEmail" "<verified-sender-email>"
dotnet user-secrets set "AuthenticationProviders:SendGrid:FromName" "ExpenseTracker Pro"
dotnet user-secrets set "AuthenticationProviders:SendGrid:PasswordResetUrl" "https://localhost:4200/reset-password"
```

For a deployed environment, use the equivalent environment variables or a
managed secret store. For example,
`AuthenticationProviders__TwilioVerify__AuthToken` maps to the Twilio auth
token setting.

## Apply the database migration

From the solution directory, run this as one PowerShell line:

```powershell
dotnet ef database update --project ".\ExpenseTracker.Infrastructure\ExpenseTracker.Infrastructure.csproj" --startup-project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
```

If `dotnet ef` is unavailable:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
dotnet tool update --global dotnet-ef --version 8.*
```

Only one of the install/update commands is needed. Restart PowerShell if the
new global tool is not immediately found.

The migration `20260718143000_AddExternalAuthenticationSupport` creates
filtered unique indexes for normalized email and E.164 mobile number. It does
not remove or replace existing users.

## Google registration and sign-in

1. Create a Google OAuth 2.0 Web application client.
2. Configure the allowed JavaScript origins for the frontend.
3. Render the Google Identity Services button in the frontend.
4. Send the returned Google `credential` value to the API as `idToken`.

```http
POST /api/v1/auth/google
Content-Type: application/json

{
  "idToken": "<google-credential-id-token>"
}
```

The API verifies the token signature, issuer, expiry, audience, subject, and
verified email. A new verified Google user is created when no account exists.
If the verified email already belongs to an email/password account, the API
does not silently link it; the user must continue using the existing sign-in
method until an authenticated account-linking feature is added.

## Passwordless mobile registration

Mobile numbers must use E.164 format, such as `+639171234567`.

Request a registration code:

```http
POST /api/v1/auth/mobile/register/request-code
Content-Type: application/json

{
  "fullName": "Mobile Test User",
  "phoneNumber": "+639171234567"
}
```

Verify the code sent by Twilio:

```http
POST /api/v1/auth/mobile/register/verify
Content-Type: application/json

{
  "phoneNumber": "+639171234567",
  "code": "123456"
}
```

Successful verification confirms the number and returns the same JWT access
token and rotating refresh token shape as email/password login. The API never
stores the SMS code.

## Passwordless mobile login

```http
POST /api/v1/auth/mobile/login/request-code
Content-Type: application/json

{
  "phoneNumber": "+639171234567"
}
```

```http
POST /api/v1/auth/mobile/login/verify
Content-Type: application/json

{
  "phoneNumber": "+639171234567",
  "code": "123456"
}
```

Failed code checks count toward the existing five-attempt, 15-minute account
lockout. Twilio controls code generation, expiry, verification, and one-time
use.

## Forgot and reset password

Request the reset email:

```http
POST /api/v1/auth/forgot-password
Content-Type: application/json

{
  "email": "testuser1@gmail.com"
}
```

The response is intentionally identical whether the account exists. SendGrid
sends a link to the configured `PasswordResetUrl` with URL-encoded `email` and
`token` query parameters. The frontend reset page should submit them with the
new password:

```http
POST /api/v1/auth/reset-password
Content-Type: application/json

{
  "email": "testuser1@gmail.com",
  "token": "<token-from-reset-link>",
  "newPassword": "<new-strong-password>"
}
```

Reset tokens expire after 30 minutes. A successful reset revokes the account's
stored refresh token, so existing refresh sessions can no longer be renewed.
For a multi-instance or container deployment, persist and protect the ASP.NET
Core Data Protection key ring in a shared durable location so valid reset
tokens continue to work across restarts and instances.

## Security behavior

- Provider credentials are configuration-only and are never placed in source.
- Google ID tokens, SMS codes, passwords, reset tokens, and request bodies are
  excluded from audit records.
- Verification endpoints are limited to three requests per client per minute.
- Unknown accounts receive generic code-request and forgot-password responses.
- Mobile numbers and normalized emails are unique at the database layer.
- Provider network failures are returned as safe `503` problem responses.
- All mutations are still recorded by the authenticated module/consolidated
  audit-trail system.

Use provider test credentials and a non-production database while validating
these flows. Sending SMS and email can consume provider quota or incur cost.
