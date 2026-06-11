# [RASM_APPHOST_API_RESILIENCE]

`Microsoft.Extensions.Http.Resilience` supplies outbound HTTP resilience handlers and policy options for remote hops.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Http.Resilience`
- package: `Microsoft.Extensions.Http.Resilience`
- assembly: `Microsoft.Extensions.Http.Resilience`
- namespace: `Microsoft.Extensions.Http.Resilience`
- asset: runtime library
- rail: resilience

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: resilience family
- rail: resilience

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]    | [CAPABILITY]            |
| :-----: | :---------------------------------- | :---------------- | :---------------------- |
|   [1]   | `HttpStandardResilienceOptions`     | policy object     | carries policy input    |
|   [2]   | `HttpRetryStrategyOptions`          | policy object     | carries policy input    |
|   [3]   | `HttpTimeoutStrategyOptions`        | policy object     | carries policy input    |
|   [4]   | `HttpCircuitBreakerStrategyOptions` | policy object     | carries policy input    |
|   [5]   | `HttpRateLimiterStrategyOptions`    | policy object     | carries policy input    |
|   [6]   | `ResilienceHandlerContext`          | operation context | carries operation state |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: handler operations
- rail: resilience

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]           | [CAPABILITY]              |
| :-----: | :----------------------------- | :--------------------- | :------------------------ |
|   [1]   | `AddStandardResilienceHandler` | DI extension           | admits configured surface |
|   [2]   | `AddResilienceHandler`         | DI extension           | admits configured surface |
|   [3]   | `Configure`                    | configuration delegate | applies policy value      |
|   [4]   | `DisableForUnsafeHttpMethods`  | resilience filter      | protects unsafe calls     |
|   [5]   | `SelectPipelineByAuthority`    | pipeline selector      | keys remote pipeline      |

## [4]-[IMPLEMENTATION_LAW]

[RESILIENCE_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.Resilience`
- handler rails: standard resilience handler, custom resilience handler, standard hedging handler
- policy families: retry, timeout, circuit breaker, rate limiter, attempt timeout, total request timeout
- selection surface: authority-based pipeline selection
- predicate surface: transient HTTP status, exception, timeout, cancellation boundaries

[LOCAL_ADMISSION]:
- Each outbound seam has one resilience policy chain.
- Hedging is admitted only when the remote operation is idempotent by policy.
- Domain retry schedules and HTTP resilience pipelines never stack on the same seam.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Http.Resilience`
- Owns: HTTP boundary resilience
- Accept: outbound retry policy stays seam-local
- Reject: nested retry loops
