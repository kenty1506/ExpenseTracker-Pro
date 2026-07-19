# Security policy

## Reporting a vulnerability

Do not open a public issue containing secrets, access tokens, personal finance
data, or reproducible account-access details. Report the issue privately to the
project owner and include the affected version, impact, and a minimal safe
reproduction.

## Secret handling

- Never store connection passwords, JWT signing secrets, seeded-user
  passwords, Google client configuration, Twilio credentials, or SendGrid API
  keys in Git.
- Use .NET User Secrets for local development.
- Use environment variables or a managed secret store for deployments.
- Rotate a secret immediately if it appears in a commit, build log, screenshot,
  or shared archive.
- Do not log authorization headers, passwords, refresh tokens, or request bodies
  from authentication endpoints.
- Do not log Google ID tokens, SMS verification codes, or password-reset tokens.

## Supported baseline

The project targets .NET 8. Apply supported .NET and NuGet security updates,
run the automated test suite, and review migration scripts before deployment.
