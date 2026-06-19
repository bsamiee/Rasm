# [PY_RUNTIME_API_STAMINA]

`stamina` supplies production retry policy on top of tenacity: a `@retry` decorator, a `retry_context` iterator for inline blocks, reusable retrying-caller objects, exponential backoff with jitter and caps, an on-retry instrumentation hook surface, and a test-mode switch. It is the single resilience owner; no hand-rolled retry loops survive.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stamina`
- package: `stamina`
- import: `stamina`
- owner: `runtime`
- rail: resilience
- namespaces: `stamina`, `stamina.instrumentation`, `stamina.typing`
- capability: retry decorator, inline retry context, reusable retrying callers, backoff/jitter/caps, on-retry instrumentation hooks, active/test toggles

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: retry-caller family
- rail: resilience

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :------------------------- | :------------ | :--------------------------- |
|  [01]   | `RetryingCaller`           | caller        | reusable sync retry wrapper  |
|  [02]   | `AsyncRetryingCaller`      | caller        | reusable async retry wrapper |
|  [03]   | `BoundRetryingCaller`      | caller        | policy-bound sync caller     |
|  [04]   | `BoundAsyncRetryingCaller` | caller        | policy-bound async caller    |
|  [05]   | `Attempt`                  | attempt       | per-iteration retry context  |

[PUBLIC_TYPE_SCOPE]: instrumentation family
- rail: resilience

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :-------------------------------------- | :------------ | :--------------------------- |
|  [01]   | `instrumentation.RetryHook`             | hook          | on-retry callback contract   |
|  [02]   | `instrumentation.RetryHookFactory`      | hook factory  | lazy hook construction       |
|  [03]   | `instrumentation.RetryDetails`          | record        | per-retry detail payload     |
|  [04]   | `instrumentation.StructlogOnRetryHook`  | hook          | structlog retry emitter      |
|  [05]   | `instrumentation.LoggingOnRetryHook`    | hook          | stdlib-logging retry emitter |
|  [06]   | `instrumentation.PrometheusOnRetryHook` | hook          | prometheus retry counter     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: retry operations
- rail: resilience

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :----------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `retry`                              | decorator       | retry callable on listed exceptions |
|  [02]   | `retry_context`                      | iterator        | retry inline statement block        |
|  [03]   | `RetryingCaller`                     | caller build    | construct reusable sync caller      |
|  [04]   | `AsyncRetryingCaller`                | caller build    | construct reusable async caller     |
|  [05]   | `is_active` / `set_active`           | toggle          | enable/disable retrying globally    |
|  [06]   | `is_testing` / `set_testing`         | toggle          | deterministic test mode             |
|  [07]   | `instrumentation.set_on_retry_hooks` | instrumentation | register retry hooks                |
|  [08]   | `instrumentation.get_on_retry_hooks` | instrumentation | read active retry hooks             |

## [04]-[IMPLEMENTATION_LAW]

[RESILIENCE_TOPOLOGY]:
- retry law: every transient-failure boundary call is wrapped with `@retry(on=..., attempts=..., timeout=...)` or a `retry_context` block; explicit retry loops with manual `sleep` are deleted.
- backoff law: exponential backoff with jitter and a wait cap is the default schedule expressed through `retry` parameters; the schedule is a decorator argument, never a hand-coded delay computation.
- selectivity law: `on` names the exact retryable exception set; non-transient faults are not retried and surface immediately as `Error(BoundaryFault(...))`.
- instrumentation law: retry observability is one `set_on_retry_hooks([StructlogOnRetryHook()])` registration feeding the structlog/OTel receipt surface; per-call logging is not duplicated.
- test law: deterministic specs call `set_testing(True)` to collapse backoff; production code never branches on test mode.

[LOCAL_ADMISSION]:
- The lane and transport surfaces compose `retry`/`retry_context` around fallible I/O; the runtime owns no second retry implementation.
- Retry hooks feed the existing receipt/observability owner; stamina contributes the on-retry signal, the receipt owner records it.

[RAIL_LAW]:
- Package: `stamina`
- Owns: retry policy, exponential-backoff scheduling, reusable retrying callers, and on-retry instrumentation
- Accept: `@retry` boundaries, `retry_context` blocks, explicit retryable exception sets, registered retry hooks
- Reject: manual retry loops, hand-coded backoff delays, blanket exception retrying, duplicated retry logging
