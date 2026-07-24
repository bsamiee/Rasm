# [TS_RUNTIME_API_OPENFEATURE_SERVER_SDK]

`@openfeature/server-sdk` mints the vendor-neutral server evaluation contract: a global `OpenFeature` singleton registers a `Provider` and mints clients that answer `ResolutionDetails` per value kind, threading a `Hook` lifecycle, a provider event plane, and evaluation context keyed by `targetingKey` over `AsyncLocalStorage`. `proc/flag` is the one `Provider`, projecting its live ruleset onto this seam so every hook, context, and event rides the client path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@openfeature/server-sdk`
- package: `@openfeature/server-sdk` (Apache-2.0)
- module: ESM + CJS dual, re-exporting the `@openfeature/core` reason, error-code, and type vocabulary
- runtime: node/bun server processes under the `runsOn: "server"` paradigm gate the SDK enforces
- rail: flag evaluation contract

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider answers, hook lifecycle, reason and error vocabulary, provider events

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Provider`                                   | interface     | the one contract `proc/flag` implements over its cell         |
|  [02]   | `ResolutionDetails<T>`                       | record        | `value` + `reason`/`errorCode`/`variant`/`flagMetadata`       |
|  [03]   | `EvaluationDetails<T>`                       | record        | `ResolutionDetails<T>` + `flagKey`; `get*Details` returns it  |
|  [04]   | `EvaluationContext`                          | record        | `targetingKey?` + attributes, the bucket identity             |
|  [05]   | `Hook` / `HookContext`                       | interface     | `before`/`after`/`error`/`finally` taps and per-stage context |
|  [06]   | `FlagValue`                                  | union         | `boolean \| string \| number \| JsonValue`                    |
|  [07]   | `JsonValue`                                  | union         | `resolveObjectEvaluation<T>` payload                          |
|  [08]   | `StandardResolutionReasons` / `ErrorCode`    | vocabulary    | reason and degradation-code spellings `Verdict` mirrors       |
|  [09]   | `OpenFeatureEventEmitter` / `ProviderEvents` | events        | emitter; `Ready`/`Error`/`ConfigurationChanged`/`Stale`       |

[PROVIDER]: `resolve{Boolean,String,Number,Object}Evaluation(flagKey, defaultValue, EvaluationContext, Logger) -> Promise<ResolutionDetails<T>>`; lifecycle `metadata` `runsOn` `hooks?` `events?` `initialize?(EvaluationContext?)` `onClose?`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration, client reads, context altitude, transaction propagation. Leading-dot surfaces are `OpenFeature` singleton statics, `client.*` are instance reads, and `get{Boolean,String,Number,Object}Value`/`get*Details` take `(flag, fallback, context?)`.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]         | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :----------------------------------------- |
|  [01]   | `.setProviderAndWait(provider)` / `.setProvider(domain, provider)`        | static          | register the provider, await readiness     |
|  [02]   | `.getClient(domain?)`                                                     | static          | mint the `Flags` service client            |
|  [03]   | `client.get*Value` / `client.get*Details`                                 | instance        | value and `Verdict`-fed detail reads       |
|  [04]   | `.setContext(context)` / `client.setContext(context)`                     | static/instance | context at global or client altitude       |
|  [05]   | `.addHooks(...)` / `client.addHooks(...)` / invocation `{ hooks }`        | static/instance | register lifecycle hooks                   |
|  [06]   | `.addHandler(ProviderEvents.X, handler)` / `client.addHandler`            | static/instance | observe readiness and config events        |
|  [07]   | `.setTransactionContextPropagator(p)` / `.setTransactionContext(ctx, fn)` | static          | install the `AsyncLocalStorage` propagator |
|  [08]   | `.close()`                                                                | static          | scope-release teardown in `Flags` Layer    |
|  [09]   | `client.track(name, context?, details?)`                                  | instance        | associate a flag outcome with an action    |
|  [10]   | `InMemoryProvider`                                                        | class           | in-memory provider for SDK-seam specs      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every flag read folds through the client, never the provider directly — the client owns hook firing, context altitude, and event handlers, while the provider answers `ResolutionDetails` as total data and never throws from a `resolve*` member.

[STACKING]:
- `effect`(`.api/effect.md`): the provider's promise members bridge through the runtime captured at Layer build — `Effect.runtime` + `Runtime.runPromise` for the callback seam, `Effect.tryPromise`/`Effect.promise` converting registration and `close` inside the `Flags` scoped build; a `Cache.makeWith` memo tier keys on reason with a TTL the `ConfigurationChanged` handler invalidates.
- `proc/flag` rule engine: the provider projects the pure `Rollout.decide` fold over the live ruleset cell — the SDK owns lifecycle and contract, the page owns targeting semantics and bucket parity.
- `net/channel` `Feed`: ruleset patches arrive over the SSE seam, and the provider emits `ConfigurationChanged` per accepted patch so consumers invalidate on the SDK's own signal.

[LOCAL_ADMISSION]:
- `proc/flag` implements exactly one `Provider`; a second or vendor provider is a roster decision, never a silent import.
- Reason and error vocabularies mirror rather than import into wire shapes — `Rollout.reasons` and `Verdict.codes` anchor the branch spellings against the SDK constants.

[RAIL_LAW]:
- Package: `@openfeature/server-sdk`
- Owns: the evaluation contract — `Provider`/`ResolutionDetails`/`EvaluationContext`, the client surface, hooks, events, transaction-context propagation, the reason and error-code vocabulary
- Accept: one `Provider` over the ruleset cell, `setProviderAndWait` at Layer build with `close()` on release, `get*Details` reads projected into `Verdict`, one telemetry hook, `ConfigurationChanged`-driven invalidation
- Reject: a hand-minted evaluation contract, a throwing resolve member, provider-direct evaluation bypassing hooks, a second flag source beside the provider's cell
