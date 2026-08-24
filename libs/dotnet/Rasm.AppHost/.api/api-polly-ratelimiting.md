# [RASM_APPHOST_API_POLLY_RATELIMITING]

`Polly.RateLimiting` folds one rate-limiter admission strategy onto a resilience pipeline: a null `RateLimiter` delegate makes the strategy MINT and own a built-in concurrency limiter, a non-null delegate binds any `System.Threading.RateLimiting` limiter the caller keeps owning, and a denied lease raises `RateLimiterRejectedException` carrying whatever retry-after hint that limiter family publishes. Admission counts logical calls, so this strategy belongs outside every retry loop it guards.

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

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]      | [CAPABILITY]                             |
| :-----: | :----------------------------------------------- | :----------------- | :--------------------------------------- |
|  [01]   | `RateLimiterStrategyOptions`                     | strategy options   | rate-limiter policy                      |
|  [02]   | `RateLimiterArguments`                           | callback arguments | lease request, carries `Context` alone   |
|  [03]   | `OnRateLimiterRejectedArguments`                 | callback arguments | rejection, carries `Context` and `Lease` |
|  [04]   | `RateLimiterRejectedException`                   | exception          | rejected execution                       |
|  [05]   | `RateLimiterResiliencePipelineBuilderExtensions` | builder extension  | limiter admission                        |

[PUBLIC_TYPE_SCOPE]: companion limiter primitives — `System.Threading.RateLimiting`

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]     | [CAPABILITY]                       |
| :-----: | :---------------------------------- | :---------------- | :--------------------------------- |
|  [01]   | `RateLimiter`                       | limiter base      | lease producer, statistics reader  |
|  [02]   | `RateLimitLease`                    | lease value       | acquisition result, back-pressure  |
|  [03]   | `ReplenishingRateLimiter`           | replenishing base | periodic refill, manual replenish  |
|  [04]   | `ConcurrencyLimiter`                | limiter           | bounded-permit admission           |
|  [05]   | `SlidingWindowRateLimiter`          | limiter           | segmented-window admission         |
|  [06]   | `TokenBucketRateLimiter`            | limiter           | token-bucket admission             |
|  [07]   | `FixedWindowRateLimiter`            | limiter           | fixed-window admission             |
|  [08]   | `ConcurrencyLimiterOptions`         | option value      | permit/queue policy                |
|  [09]   | `SlidingWindowRateLimiterOptions`   | option value      | sliding-window policy              |
|  [10]   | `TokenBucketRateLimiterOptions`     | option value      | token-bucket policy                |
|  [11]   | `FixedWindowRateLimiterOptions`     | option value      | fixed-window policy                |
|  [12]   | `QueueProcessingOrder`              | enum              | queue fairness, `OldestFirst`-led  |
|  [13]   | `RateLimiterStatistics`             | statistics value  | permits, queue depth, lease totals |
|  [14]   | `MetadataName` / `MetadataName<T>`  | metadata key      | typed lease-metadata lookup key    |
|  [15]   | `PartitionedRateLimiter`            | partition factory | resource-keyed limiter composition |
|  [16]   | `PartitionedRateLimiter<TResource>` | partitioned base  | per-partition lease production     |
|  [17]   | `RateLimitPartition`                | partition factory | per-key limiter row minting        |
|  [18]   | `RateLimitPartition<TKey>`          | partition value   | partition key beside its factory   |

Each options value carries its settable members, and `AutoReplenishment` defaults `true` on every replenishing shape:
- `ConcurrencyLimiterOptions`: `PermitLimit` `QueueLimit` `QueueProcessingOrder`
- `SlidingWindowRateLimiterOptions`: `Window` `SegmentsPerWindow` `PermitLimit` `QueueLimit` `QueueProcessingOrder` `AutoReplenishment`
- `TokenBucketRateLimiterOptions`: `ReplenishmentPeriod` `TokensPerPeriod` `TokenLimit` `QueueLimit` `QueueProcessingOrder` `AutoReplenishment`
- `FixedWindowRateLimiterOptions`: `Window` `PermitLimit` `QueueLimit` `QueueProcessingOrder` `AutoReplenishment`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline admission and strategy options

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `AddConcurrencyLimiter(int permitLimit, int queueLimit = 0)` | static   | permit/queue concurrency cap          |
|  [02]   | `AddConcurrencyLimiter(ConcurrencyLimiterOptions)`           | static   | configured concurrency limiter        |
|  [03]   | `AddRateLimiter(RateLimiter)`                                | static   | concrete limiter admission            |
|  [04]   | `AddRateLimiter(RateLimiterStrategyOptions)`                 | static   | strategy-option admission             |
|  [05]   | `RateLimiterStrategyOptions.RateLimiter`                     | property | lease-producer delegate               |
|  [06]   | `RateLimiterStrategyOptions.DefaultRateLimiterOptions`       | property | built-in limiter policy, `[Required]` |
|  [07]   | `RateLimiterStrategyOptions.OnRejected`                      | property | rejection callback                    |
|  [08]   | `RateLimiterStrategyOptions.Name`                            | property | `strategy.name` dimension             |
|  [09]   | `RateLimiterArguments.Context`                               | property | executing context per lease call      |
|  [10]   | `OnRateLimiterRejectedArguments.Lease`                       | property | denied lease, metadata readable       |
|  [11]   | `RateLimiterRejectedException.RetryAfter`                    | property | retry-after projection                |
|  [12]   | `RateLimiterRejectedException.TelemetrySource`               | property | refusing pipeline and strategy        |

[ENTRYPOINT_SCOPE]: companion limiter construction and acquisition — `System.Threading.RateLimiting`

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :-------------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `ConcurrencyLimiter(ConcurrencyLimiterOptions)`                       | ctor     | bounded-permit construction    |
|  [02]   | `SlidingWindowRateLimiter(SlidingWindowRateLimiterOptions)`           | ctor     | segmented-window construction  |
|  [03]   | `TokenBucketRateLimiter(TokenBucketRateLimiterOptions)`               | ctor     | token-bucket construction      |
|  [04]   | `FixedWindowRateLimiter(FixedWindowRateLimiterOptions)`               | ctor     | fixed-window construction      |
|  [05]   | `RateLimiter.AcquireAsync(int permitCount = 1, CancellationToken)`    | instance | asynchronous lease             |
|  [06]   | `RateLimiter.AttemptAcquire(int permitCount = 1)`                     | instance | synchronous fast path          |
|  [07]   | `RateLimiter.GetStatistics() -> RateLimiterStatistics?`               | instance | live permit and queue readout  |
|  [08]   | `RateLimiter.IdleDuration`                                            | property | reclaim evidence for a pool    |
|  [09]   | `RateLimiter.CreateChained(params RateLimiter[])`                     | static   | conjunction over one resource  |
|  [10]   | `ReplenishingRateLimiter.TryReplenish() -> bool`                      | instance | manual refill when auto is off |
|  [11]   | `ReplenishingRateLimiter.IsAutoReplenishing` / `.ReplenishmentPeriod` | property | refill posture readout         |
|  [12]   | `RateLimitLease.IsAcquired`                                           | property | permit verdict                 |
|  [13]   | `RateLimitLease.TryGetMetadata<T>(MetadataName<T>, out T) -> bool`    | instance | typed metadata probe           |
|  [14]   | `RateLimitLease.MetadataNames` / `.GetAllMetadata()`                  | member   | published metadata roster      |
|  [15]   | `MetadataName.RetryAfter` (`MetadataName<TimeSpan>`)                  | static   | back-pressure window key       |
|  [16]   | `MetadataName.ReasonPhrase` (`MetadataName<string>`)                  | static   | refusal-reason key             |

[ENTRYPOINT_SCOPE]: partitioned admission — one limiter instance per resource key

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `PartitionedRateLimiter.Create<TResource, TPartitionKey>(partitioner, comparer?)`    | static   | resource-to-partition fold   |
|  [02]   | `PartitionedRateLimiter.CreateChained<TResource>(params limiters)`                   | static   | conjunction of partitions    |
|  [03]   | `PartitionedRateLimiter<TResource>.AcquireAsync(TResource, int, CancellationToken)`  | instance | per-partition lease          |
|  [04]   | `PartitionedRateLimiter<TResource>.AttemptAcquire(TResource, int)`                   | instance | per-partition fast path      |
|  [05]   | `PartitionedRateLimiter<TResource>.GetStatistics(TResource)`                         | instance | per-partition readout        |
|  [06]   | `PartitionedRateLimiter<TResource>.WithTranslatedKey<TOuter>(keyAdapter, leaveOpen)` | instance | key-shape adaptation         |
|  [07]   | `RateLimitPartition.GetTokenBucketLimiter<TKey>(key, factory)`                       | static   | token-bucket partition row   |
|  [08]   | `RateLimitPartition.GetConcurrencyLimiter<TKey>(key, factory)`                       | static   | concurrency partition row    |
|  [09]   | `RateLimitPartition.GetSlidingWindowLimiter<TKey>(key, factory)`                     | static   | sliding-window partition row |
|  [10]   | `RateLimitPartition.GetFixedWindowLimiter<TKey>(key, factory)`                       | static   | fixed-window partition row   |
|  [11]   | `RateLimitPartition.GetNoLimiter<TKey>(key)`                                         | static   | exempt partition row         |
|  [12]   | `RateLimitPartition.Get<TKey>(key, Func<TKey, RateLimiter> factory)`                 | static   | arbitrary limiter partition  |

- `RateLimiter` is `IDisposable` and `IAsyncDisposable`; `PartitionedRateLimiter<TResource>` implements both and disposes every partition it minted, while `RateLimitLease` is `IDisposable` alone and releases its permits back to the instance that ISSUED it.
- `AddConcurrencyLimiter` is not a distinct strategy: both overloads construct a `RateLimiterStrategyOptions` carrying `DefaultRateLimiterOptions` and forward to `AddRateLimiter`, so a chain stacking both verbs seats two limiter strategies, not one limiter with two policies.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Rate limiting folds onto the resilience pipeline as one admission strategy binding `PermitLimit`, `QueueLimit`, and `QueueProcessingOrder` from policy; a lease spans the whole guarded call, so admission placed inside a retry loop converts retry storms into permit starvation.
- `RateLimiterStrategyOptions.RateLimiter` left null makes the strategy construct a `ConcurrencyLimiter` over `DefaultRateLimiterOptions` and acquire ONE permit per execution; a non-null delegate binds any `System.Threading.RateLimiting` limiter and receives the executing `ResilienceContext` per lease request.
- Denied leases raise `RateLimiterRejectedException` on the `Polly.Core` `ExecutionRejectedException` rail with `TelemetrySource` stamped at throw, so one exception type serves every limiter row and the refusing strategy resolves through that stamp rather than message text.

[LEASE_LAW]:
- Rejection runs in a FIXED order — the strategy reports its `OnRateLimiterRejected` telemetry event at `ResilienceEventSeverity.Error`, then invokes `OnRejected`, then constructs and throws — so the meter records every refusal whether or not a callback is seated.
- Strategy code holds the lease in a `using` across the guarded call AND across the whole rejection path, so `OnRejected` reads live lease metadata but a callback storing that lease reads disposed state the moment it returns.
- `RetryAfter` on the exception exists only where the lease published `MetadataName.RetryAfter`: `ConcurrencyLimiter` publishes `MetadataName.ReasonPhrase` ALONE and never a retry-after window, while the token-bucket, sliding-window, and fixed-window families publish one whenever they can compute the wait.
- Consequence for a delay generator: a retry row projecting `RetryAfter` into its `DelayGenerator` gets a real window from the replenishing rows and a null from the concurrency row, which falls back to the configured backoff curve rather than failing.

[LIFETIME_LAW]:
- Ownership follows CONSTRUCTION rather than use: the built-in limiter the strategy mints from `DefaultRateLimiterOptions` disposes with the pipeline, while a limiter reached through `AddRateLimiter(RateLimiter)` or a lease-producer delegate is NEVER disposed by the strategy — which is exactly what lets one shared or partitioned limiter span N pipelines.
- Composition therefore owns every caller-supplied limiter's release, and that release waits on outstanding leases: a permit returns to the instance that ISSUED it, so disposing under a live lease strands the permit and a resize that swaps instances must retire the old one rather than dispose it inline.
- Partition instances belong to their `PartitionedRateLimiter<TResource>`, so disposing that owner releases the whole partition set and a hand-rolled dictionary of limiters re-implements a lifetime this type already holds.

[DEFAULT_HAZARDS]: unset knobs carry shipped values that silently change admission meaning

| [INDEX] | [KNOB]                                                 | [SHIPPED_DEFAULT]     | [CONSEQUENCE_WHEN_UNSET]                         |
| :-----: | :----------------------------------------------------- | :-------------------- | :----------------------------------------------- |
|  [01]   | `RateLimiterStrategyOptions.DefaultRateLimiterOptions` | 1000 permits, 0 queue | admission caps at a thousand and never queues    |
|  [02]   | `RateLimiterStrategyOptions.Name`                      | `RateLimiter`         | two limiter rows merge into one telemetry series |
|  [03]   | `RateLimiterStrategyOptions.OnRejected`                | `null`                | rejection reaches the meter, never a receipt     |
|  [04]   | `AddConcurrencyLimiter(permitLimit, queueLimit)`       | queue 0               | an over-limit call refuses instead of waiting    |
|  [05]   | `ConcurrencyLimiterOptions.QueueProcessingOrder`       | `OldestFirst`         | fairness is FIFO unless a row states otherwise   |

[STACKING]:
- `Polly.Core`(`.api/api-polly-core.md`): `AddRateLimiter`/`AddConcurrencyLimiter` append onto `ResiliencePipelineBuilderBase` through `AddStrategy`, so both verbs bind the generic and non-generic builders alike; `RateLimiterRejectedException` extends `ExecutionRejectedException` and carries that package's `ResilienceTelemetrySource`, and `RateLimiterArguments.Context` is its pooled `ResilienceContext` whose `CancellationToken` every lease call observes.
- `Polly.Extensions`(`.api/api-polly-extensions.md`): the rejection event rides the `Polly` meter's `resilience.polly.strategy.events` stream under `event.name` `OnRateLimiterRejected`, so a `SeverityProvider` row demoting or promoting admission churn reads that spelling and a `MeteringEnricher` row adds the partition dimension the library never mints.
- `System.Threading.RateLimiting`: limiter construction, lease acquisition, statistics, and partitioning all live in the BCL and enter through the transitive closure, never a direct pin.
- AppHost composition: `Wire/outbound#KEYED_PIPELINES` selects one limiter shape per hop through its `HopRateLimit` admission column, and `Runtime/laneguard#LANE_GUARD` seats a lane-pool row beside a per-tenant row so a tenant burst bounds at its own bucket rather than consuming the lane pool.

[LOCAL_ADMISSION]:
- Rate limiting composes once as a boundary policy on the resilience pipeline.
- `PermitLimit` and `QueueLimit` are explicit policy values, never ambient defaults.
- Every limiter row carries its own `Name` so two admission strategies in one pipeline stay distinguishable at the meter.
- `OnRejected` observes and projects rejection; it runs no side-effect retry and retains nothing from the lease.
- Per-key admission composes through the partitioned surface rather than a hand-held limiter map.

[RAIL_LAW]:
- Package: `Polly.RateLimiting`
- Owns: rate-limiter admission on the resilience pipeline
- Accept: explicit concurrency, windowed, and partitioned rate-limit policy
- Reject: hidden semaphores around resilient operations
