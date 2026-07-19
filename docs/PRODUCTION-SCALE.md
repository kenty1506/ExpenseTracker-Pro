# Production scale readiness

The codebase now has a stronger production baseline, but supporting one million
users is an infrastructure and operations target—not a promise that can be made
from an application build alone.

## Improvements already in the application

- HTTP-only browser refresh sessions with rotation and logout revocation
- Pooled EF Core contexts and resilient SQL connections
- Brotli/Gzip API response compression
- Database-side dashboard aggregates instead of loading all transactions
- Paginated recent dashboard activity
- Bounded, keyset-paginated background user processing
- Idempotent notification keys, updates, stale-alert deactivation, and safe
  internal action URLs
- Exact-origin credentialed CORS configuration
- Per-user/IP rate limits, health endpoints, correlation IDs, audit logs, and
  optimistic concurrency
- Lazy frontend routes, intent-based route preloading, resilient API retries,
  and a recoverable application error boundary
- Server-side MoMo conversation engine with bounded user context, deterministic
  finance calculations, follow-up awareness, and per-user throttling

## Required before a million-user launch

1. Replace the single refresh-token fields on `ApplicationUser` with a
   multi-session table. Rotate tokens atomically, support a short overlap grace
   window, and let users revoke individual devices.
2. Run recurring and notification work in dedicated workers backed by a durable
   queue. Add a distributed lease so multiple API replicas cannot scan the same
   users simultaneously.
3. Put rate limiting at the edge or in a distributed store. The in-process
   limiter is defense in depth, not the global quota authority for many replicas.
4. Persist ASP.NET Data Protection keys in a shared protected store so password
   reset and identity tokens survive restarts and work across replicas.
5. Use a managed SQL tier sized from load tests. Monitor slow queries, connection
   pool pressure, deadlocks, index usage, storage growth, and backup restore time.
   Partition or archive high-growth audit and transaction tables when measurements
   show it is necessary.
6. Serve the static client through a CDN and keep `/api` same-origin through the
   edge proxy. Use immutable caching for hashed assets and no-store for financial
   API responses.
7. Add distributed tracing, metrics, centralized structured logs, alerting, and
   redaction checks. Define service-level objectives before capacity testing.
8. Run staged load tests for registration/login, dashboard, transaction creation,
   reports, refresh-token bursts, and background processing. Test gradual ramps,
   sudden spikes, database failover, and one-replica loss.
9. Add automated deployment migrations, canary releases, rollback procedures,
   encrypted backups, and regularly tested disaster recovery.
10. Cache or precompute MoMo's read-heavy monthly snapshots when load tests show
    database pressure. Maintain intent, calculation, Taglish, and follow-up
    regression suites whenever conversation rules change.

## Suggested release gates

- No failed requests at the expected steady-state load except deliberate `429`s
- p95 API latency below the product target and p99 without long-tail timeouts
- Database CPU, connections, and lock waits retain safe headroom during peaks
- Refresh bursts and replica restarts do not log users out unexpectedly
- Background work remains within its processing window without duplicate effects
- MoMo meets answer-quality, calculation, safety, and latency targets under load
- A production-like restore drill meets the recovery objectives
