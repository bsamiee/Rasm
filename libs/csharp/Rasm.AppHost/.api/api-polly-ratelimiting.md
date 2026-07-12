# [RASM_APPHOST_API_POLLY_RATELIMITING]

`Polly.RateLimiting` supplies rate-limiter strategy options, rejected execution values, callbacks, and builder extensions for resilience-pipeline admission.

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

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [ROLE]                     |
| :-----: | :-------------------------------- | :---------------- | :------------------------- |
|  [01]   | `RateLimiter`                     | limiter base      | lease producer             |
|  [02]   | `RateLimitLease`                  | lease value       | acquisition result         |
|  [03]   | `ReplenishingRateLimiter`         | replenishing base | periodic refill            |
|  [04]   | `SlidingWindowRateLimiter`        | limiter           | segmented-window admission |
|  [05]   | `TokenBucketRateLimiter`          | limiter           | token-bucket admission     |
|  [06]   | `SlidingWindowRateLimiterOptions` | option value      | sliding-window policy      |
|  [07]   | `TokenBucketRateLimiterOptions`   | option value      | token-bucket policy        |
|  [08]   | `QueueProcessingOrder`            | enum              | queue fairness             |

[RateLimiter]:

- Contract: abstract `IAsyncDisposable` and `IDisposable`
- Producer: returned by the `RateLimiterStrategyOptions.RateLimiter` delegate

[RateLimitLease]:

- Contract: abstract `IDisposable`
- Surface: `IsAcquired`, `TryGetMetadata`, and `MetadataName.RetryAfter`
- Role: back-pressure hint carrier

[ReplenishingRateLimiter]:

- Inheritance: `RateLimiter`
- Behavior: automatic periodic refill
- Specializations: sliding-window and token-bucket limiters

[SlidingWindowRateLimiter]:

- Declaration: `sealed : ReplenishingRateLimiter`
- Admission: segmented fixed-window webhook cap

[TokenBucketRateLimiter]:

- Declaration: `sealed : ReplenishingRateLimiter`
- Admission: token-bucket redial-paced peer hop

[SlidingWindowRateLimiterOptions]:

- Settable members: `Window`, `SegmentsPerWindow`, `PermitLimit`, `QueueLimit`, `QueueProcessingOrder`, and `AutoReplenishment`
- Default: `AutoReplenishment = true`

[TokenBucketRateLimiterOptions]:

- Settable members: `ReplenishmentPeriod`, `TokensPerPeriod`, `TokenLimit`, `QueueLimit`, `QueueProcessingOrder`, and `AutoReplenishment`
- Default: `AutoReplenishment = true`

[QueueProcessingOrder]:

- Values: `OldestFirst` and `NewestFirst`
- Role: pipeline-head limiter queue fairness

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

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [ROLE]                        |
| :-----: | :--------------------------- | :------------- | :---------------------------- |
|  [01]   | `SlidingWindowRateLimiter`   | ctor           | segmented-window construction |
|  [02]   | `TokenBucketRateLimiter`     | ctor           | token-bucket construction     |
|  [03]   | `RateLimiter.AcquireAsync`   | acquire call   | asynchronous lease            |
|  [04]   | `RateLimiter.AttemptAcquire` | acquire call   | synchronous fast path         |
|  [05]   | `RateLimitLease.IsAcquired`  | property       | permit verdict                |

[SlidingWindowRateLimiter]:

- Signature: `new SlidingWindowRateLimiter(SlidingWindowRateLimiterOptions options)`

[TokenBucketRateLimiter]:

- Signature: `new TokenBucketRateLimiter(TokenBucketRateLimiterOptions options)`

[RateLimiter.AcquireAsync]:

- Signature: `RateLimiter.AcquireAsync(int permitCount = 1, CancellationToken = default)`
- Result: `ValueTask<RateLimitLease>`
- Role: strategy `RateLimiter` delegate body

[RateLimiter.AttemptAcquire]:

- Signature: `RateLimiter.AttemptAcquire(int permitCount = 1)`
- Result: synchronous `RateLimitLease`

[RateLimitLease.IsAcquired]:

- Result: whether the lease granted its permits

## [04]-[IMPLEMENTATION_LAW]

[RATE_LIMIT_TOPOLOGY]:
- namespaces: `Polly.RateLimiting`, `System.Threading.RateLimiting`
- builder surface: concurrency limiter and rate limiter strategy admission
- strategy surface: delegate-based limiter, default concurrency limiter options, rejection callback
- execution surface: rejected executions surface as `RateLimiterRejectedException`
- retry-after surface: rejected exceptions may carry a retry-after duration
- default-options type: `RateLimiterStrategyOptions.DefaultRateLimiterOptions` is a `System.Threading.RateLimiting.ConcurrencyLimiterOptions` value
- concurrency option declaration: `ConcurrencyLimiterOptions` is a sealed class with a parameterless constructor
- concurrency option members: settable `int PermitLimit`, `int QueueLimit`, and `QueueProcessingOrder QueueProcessingOrder`
- pipeline-head policy: `PermitLimit` and `QueueLimit` bind from policy values
- custom admission: `RateLimiterStrategyOptions.RateLimiter` returns a `System.Threading.RateLimiting` limiter for non-default admission shapes
- sliding-window admission: `SlidingWindowRateLimiter` binds `SlidingWindowRateLimiterOptions`
- token-bucket admission: `TokenBucketRateLimiter` binds `TokenBucketRateLimiterOptions`
- replenishing admission: both limiter shapes derive from `ReplenishingRateLimiter` and carry `AutoReplenishment = true`
- lease acquisition: `RateLimiter.AcquireAsync(permitCount, ct)` acquires the limiter
- fairness: `QueueProcessingOrder.OldestFirst` governs every replenishing limiter queue
- retry delay: `RateLimiterRejectedException.RetryAfter` reads the rejection `RateLimitLease` metadata and feeds the retry `DelayGenerator`

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
