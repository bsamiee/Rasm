# [RASM_APPHOST_API_POLLY_RATELIMITING]

`Polly.RateLimiting` supplies rate-limiter strategy options, rejected execution
values, rate-limiter callbacks, and builder extensions for concurrency and rate
limiting inside resilience pipelines.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Polly.RateLimiting`
- package: `Polly.RateLimiting`
- assembly: `Polly.RateLimiting`
- namespace: `Polly`
- namespace: `Polly.RateLimiting`
- companion namespace: `System.Threading.RateLimiting`
- asset: runtime library
- rail: resilience

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: rate-limit family
- rail: resilience

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]       | [RAIL]                |
| :-----: | :----------------------------------------------- | :------------------ | :-------------------- |
|  [01]   | `RateLimiterStrategyOptions`                     | strategy options    | rate-limiter policy   |
|  [02]   | `RateLimiterArguments`                           | callback arguments  | limiter lease request |
|  [03]   | `OnRateLimiterRejectedArguments`                 | callback arguments  | rejection callback    |
|  [04]   | `RateLimiterRejectedException`                   | rejection exception | rejected execution    |
|  [05]   | `RateLimiterResiliencePipelineBuilderExtensions` | builder extension   | limiter admission     |
|  [06]   | `ConcurrencyLimiterOptions`                      | option value        | permit/queue policy   |

[PUBLIC_TYPE_SCOPE]: companion limiter primitives — `System.Threading.RateLimiting`
- rail: resilience

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [RAIL]                                                                                  |
| :-----: | :-------------------------------- | :----------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `RateLimiter`                     | limiter base       | abstract `IAsyncDisposable`/`IDisposable`; the lease producer a `RateLimiterStrategyOptions.RateLimiter` delegate returns |
|  [02]   | `RateLimitLease`                  | lease value        | abstract `IDisposable` — `IsAcquired`, `TryGetMetadata`, `MetadataName.RetryAfter` back-pressure hint |
|  [03]   | `ReplenishingRateLimiter`         | replenishing base  | `RateLimiter` subclass auto-refilling on a period — the sliding-window/token-bucket base |
|  [04]   | `SlidingWindowRateLimiter`        | limiter            | `sealed : ReplenishingRateLimiter` — segmented fixed-window admission (the webhook segment cap) |
|  [05]   | `TokenBucketRateLimiter`          | limiter            | `sealed : ReplenishingRateLimiter` — token-bucket admission (the redial-paced peer hop) |
|  [06]   | `SlidingWindowRateLimiterOptions` | option value       | settable `Window`/`SegmentsPerWindow`/`PermitLimit`/`QueueLimit`/`QueueProcessingOrder`/`AutoReplenishment` (default `true`) |
|  [07]   | `TokenBucketRateLimiterOptions`   | option value       | settable `ReplenishmentPeriod`/`TokensPerPeriod`/`TokenLimit`/`QueueLimit`/`QueueProcessingOrder`/`AutoReplenishment` (default `true`) |
|  [08]   | `QueueProcessingOrder`            | enum               | `OldestFirst`/`NewestFirst` — pipeline-head limiter queue fairness order               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: limiter operations
- rail: resilience

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]    | [RAIL]                         |
| :-----: | :----------------------------------------- | :---------------- | :----------------------------- |
|  [01]   | `AddConcurrencyLimiter(limit, queueLimit)` | builder extension | simple concurrency cap         |
|  [02]   | `AddConcurrencyLimiter(options)`           | builder extension | configured concurrency limiter |
|  [03]   | `AddRateLimiter(rateLimiter)`              | builder extension | concrete limiter admission     |
|  [04]   | `AddRateLimiter(options)`                  | builder extension | strategy option admission      |
|  [05]   | `RateLimiter`                              | option delegate   | lease producer callback        |
|  [06]   | `DefaultRateLimiterOptions`                | option value      | default concurrency options    |
|  [07]   | `OnRejected`                               | option delegate   | rejection callback             |
|  [08]   | `RetryAfter`                               | exception value   | retry-after projection         |

[ENTRYPOINT_SCOPE]: companion limiter construction and acquisition — `System.Threading.RateLimiting`
- rail: resilience

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [RAIL]                                                            |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `new SlidingWindowRateLimiter(SlidingWindowRateLimiterOptions options)`      | ctor           | constructs the segmented-window limiter                          |
|  [02]   | `new TokenBucketRateLimiter(TokenBucketRateLimiterOptions options)`          | ctor           | constructs the token-bucket limiter                             |
|  [03]   | `RateLimiter.AcquireAsync(int permitCount = 1, CancellationToken = default)` | acquire call   | returns `ValueTask<RateLimitLease>` — the strategy `RateLimiter` delegate body |
|  [04]   | `RateLimiter.AttemptAcquire(int permitCount = 1)`                            | acquire call   | synchronous fast-path lease, returns `RateLimitLease`           |
|  [05]   | `RateLimitLease.IsAcquired`                                                  | property       | whether the lease granted its permits                          |

## [04]-[IMPLEMENTATION_LAW]

[RATE_LIMIT_TOPOLOGY]:
- namespaces: `Polly.RateLimiting`, `System.Threading.RateLimiting`
- builder surface: concurrency limiter and rate limiter strategy admission
- strategy surface: delegate-based limiter, default concurrency limiter options, rejection callback
- execution surface: rejected executions surface as `RateLimiterRejectedException`
- retry-after surface: rejected exceptions may carry a retry-after duration
- default-options surface: `RateLimiterStrategyOptions.DefaultRateLimiterOptions` is typed `System.Threading.RateLimiting.ConcurrencyLimiterOptions`, a `sealed class` carrying settable `int PermitLimit`, `int QueueLimit`, and `QueueProcessingOrder QueueProcessingOrder` members and a parameterless constructor; the pipeline-head limiter binds `PermitLimit`/`QueueLimit` from policy values
- companion limiter selection: when the admission shape is non-default the `RateLimiterStrategyOptions.RateLimiter` delegate returns a `System.Threading.RateLimiting` limiter — a `SlidingWindowRateLimiter` over `SlidingWindowRateLimiterOptions` or a `TokenBucketRateLimiter` over `TokenBucketRateLimiterOptions` (both `ReplenishingRateLimiter`, `AutoReplenishment = true`) — acquired through `RateLimiter.AcquireAsync(permitCount, ct)`; `QueueProcessingOrder.OldestFirst` is the fairness order on every replenishing shape, and `RateLimiterRejectedException.RetryAfter` (read off the rejection `RateLimitLease` metadata) feeds the retry `DelayGenerator`

[LOCAL_ADMISSION]:
- Rate limiting is a boundary policy in the resilience pipeline.
- Queue limit and permit limit are explicit policy values, never ambient defaults.
- Rejection callbacks observe and project rejection; they do not perform side-effect retries.
- Rate limiter leases are owned by the strategy surface.

[RAIL_LAW]:
- Package: `Polly.RateLimiting`
- Owns: Polly rate-limiter strategy integration
- Accept: explicit concurrency and rate-limit policy
- Reject: hidden semaphores around resilient operations
