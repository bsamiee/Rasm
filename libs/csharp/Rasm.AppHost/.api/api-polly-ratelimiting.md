# [RASM_APPHOST_API_POLLY_RATELIMITING]

`Polly.RateLimiting` folds one rate-limiter admission strategy onto the resilience-pipeline head: a null `RateLimiter` delegate runs the built-in concurrency limiter, a non-null delegate binds any `System.Threading.RateLimiting` limiter, and a denied lease raises `RateLimiterRejectedException` carrying the retry-after hint.

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

[PUBLIC_TYPE_SCOPE]: rate-limit strategy and admission family

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]      | [CAPABILITY]          |
| :-----: | :----------------------------------------------- | :----------------- | :-------------------- |
|  [01]   | `RateLimiterStrategyOptions`                     | strategy options   | rate-limiter policy   |
|  [02]   | `RateLimiterArguments`                           | callback arguments | limiter lease request |
|  [03]   | `OnRateLimiterRejectedArguments`                 | callback arguments | rejection callback    |
|  [04]   | `RateLimiterRejectedException`                   | exception          | rejected execution    |
|  [05]   | `RateLimiterResiliencePipelineBuilderExtensions` | builder extension  | limiter admission     |
|  [06]   | `ConcurrencyLimiterOptions`                      | option value       | permit/queue policy   |

[PUBLIC_TYPE_SCOPE]: companion limiter primitives — `System.Threading.RateLimiting`

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [CAPABILITY]                      |
| :-----: | :-------------------------------- | :---------------- | :-------------------------------- |
|  [01]   | `RateLimiter`                     | limiter base      | lease producer                    |
|  [02]   | `RateLimitLease`                  | lease value       | acquisition result, back-pressure |
|  [03]   | `ReplenishingRateLimiter`         | replenishing base | periodic refill                   |
|  [04]   | `SlidingWindowRateLimiter`        | limiter           | segmented-window admission        |
|  [05]   | `TokenBucketRateLimiter`          | limiter           | token-bucket admission            |
|  [06]   | `SlidingWindowRateLimiterOptions` | option value      | sliding-window policy             |
|  [07]   | `TokenBucketRateLimiterOptions`   | option value      | token-bucket policy               |
|  [08]   | `QueueProcessingOrder`            | enum              | queue fairness, `OldestFirst`-led |

Each options value carries its settable members, and `AutoReplenishment` defaults `true` on both replenishing shapes:
- `ConcurrencyLimiterOptions`: `PermitLimit` `QueueLimit` `QueueProcessingOrder`
- `SlidingWindowRateLimiterOptions`: `Window` `SegmentsPerWindow` `PermitLimit` `QueueLimit` `QueueProcessingOrder` `AutoReplenishment`
- `TokenBucketRateLimiterOptions`: `ReplenishmentPeriod` `TokensPerPeriod` `TokenLimit` `QueueLimit` `QueueProcessingOrder` `AutoReplenishment`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline admission and strategy options

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `AddConcurrencyLimiter(int, int)`                      | static   | permit/queue concurrency cap   |
|  [02]   | `AddConcurrencyLimiter(ConcurrencyLimiterOptions)`     | static   | configured concurrency limiter |
|  [03]   | `AddRateLimiter(RateLimiter)`                          | static   | concrete limiter admission     |
|  [04]   | `AddRateLimiter(RateLimiterStrategyOptions)`           | static   | strategy-option admission      |
|  [05]   | `RateLimiterStrategyOptions.RateLimiter`               | property | lease-producer delegate        |
|  [06]   | `RateLimiterStrategyOptions.DefaultRateLimiterOptions` | property | default concurrency options    |
|  [07]   | `RateLimiterStrategyOptions.OnRejected`                | property | rejection callback             |
|  [08]   | `RateLimiterRejectedException.RetryAfter`              | property | retry-after projection         |

[ENTRYPOINT_SCOPE]: companion limiter construction and acquisition — `System.Threading.RateLimiting`

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------------------ | :------- | :---------------------------- |
|  [01]   | `SlidingWindowRateLimiter(SlidingWindowRateLimiterOptions)`                     | ctor     | segmented-window construction |
|  [02]   | `TokenBucketRateLimiter(TokenBucketRateLimiterOptions)`                         | ctor     | token-bucket construction     |
|  [03]   | `RateLimiter.AcquireAsync(int, CancellationToken) -> ValueTask<RateLimitLease>` | instance | asynchronous lease            |
|  [04]   | `RateLimiter.AttemptAcquire(int) -> RateLimitLease`                             | instance | synchronous fast path         |
|  [05]   | `RateLimitLease.IsAcquired`                                                     | property | permit verdict                |
|  [06]   | `RateLimitLease.TryGetMetadata(MetadataName, out) -> bool`                      | instance | back-pressure metadata probe  |

- `RateLimiter`/`RateLimitLease`: abstract `IDisposable` (`RateLimiter` also `IAsyncDisposable`); the strategy surface owns limiter and lease lifetime.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Rate limiting folds onto the resilience-pipeline head as one admission strategy binding `PermitLimit`, `QueueLimit`, and `QueueProcessingOrder` from policy.
- A null `RateLimiterStrategyOptions.RateLimiter` runs the built-in `ConcurrencyLimiter` over `DefaultRateLimiterOptions` (a `ConcurrencyLimiterOptions`); a non-null delegate binds any `System.Threading.RateLimiting` limiter per lease request.
- A denied lease raises `RateLimiterRejectedException`; `RetryAfter` projects the lease `MetadataName.RetryAfter` hint into the retry `DelayGenerator`.

[STACKING]:
- `api-polly-core.md` (`ResiliencePipelineBuilder`): `AddRateLimiter`/`AddConcurrencyLimiter` append the limiter strategy onto `ResiliencePipelineBuilderBase`, and `RateLimiterRejectedException` extends its `ExecutionRejectedException` rail.
- `System.Threading.RateLimiting`: `RateLimiterStrategyOptions.RateLimiter` returns a `SlidingWindowRateLimiter`/`TokenBucketRateLimiter` from its options ctor, and `AcquireAsync`/`AttemptAcquire` yield the `RateLimitLease` the strategy admits.

[LOCAL_ADMISSION]:
- Rate limiting composes once as a boundary policy on the resilience pipeline.
- `PermitLimit` and `QueueLimit` are explicit policy values, never ambient defaults.
- `OnRejected` observes and projects rejection; it runs no side-effect retry.

[RAIL_LAW]:
- Package: `Polly.RateLimiting`
- Owns: rate-limiter admission on the resilience pipeline
- Accept: explicit concurrency and windowed rate-limit policy
- Reject: hidden semaphores around resilient operations
