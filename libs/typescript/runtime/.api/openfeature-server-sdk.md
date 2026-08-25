# [TS_RUNTIME_API_OPENFEATURE_SERVER_SDK]

`@openfeature/server-sdk` mints the vendor-neutral server evaluation contract: a global `OpenFeature` singleton registers a `Provider` and mints clients that answer `ResolutionDetails` per value kind, threading a `Hook` lifecycle, a provider event plane, and evaluation context keyed by `targetingKey` over `AsyncLocalStorage`. `proc/flag` is the one `Provider`, projecting its live ruleset onto this seam so every hook, context, and event rides the client path.

## [01]-[PUBLIC_TYPES]

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

[PROVIDER]: `resolve{Boolean,String,Number,Object}Evaluation(flagKey, defaultValue, EvaluationContext, Logger) -> Promise<ResolutionDetails<T>>`; lifecycle `metadata` `runsOn` `hooks?` `events?` `initialize?(EvaluationContext?, string?)` `onClose?`

- `Provider.initialize`: receives the domain `setProvider(domain, provider)` bound this instance to, absent on the default registration — one provider class therefore specializes per domain at init instead of forking into a class per domain.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration, client reads, context altitude, transaction propagation. Leading-dot surfaces are `OpenFeature` singleton statics, `client.*` are instance reads, and `get{Boolean,String,Number,Object}Value`/`get*Details` take `(flag, fallback, context?, options?: FlagEvaluationOptions)` — the fourth slot carries per-call `hooks`/`hookHints`.

| [INDEX] | [SURFACE]                                                          | [SHAPE]         | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `.setProviderAndWait(provider)` / `.setProvider(domain, provider)` | static          | register the provider, await readiness        |
|  [02]   | `.getClient(context?)`                                             | static          | client on the default domain                  |
|  [03]   | `.getClient(domain, context?)`                                     | static          | client bound to a domain                      |
|  [04]   | `.getClient(domain, version, context?)`                            | static          | domain client stamped with a version          |
|  [05]   | `client.get*Value` / `client.get*Details`                          | instance        | value and `Verdict`-fed detail reads          |
|  [06]   | `.setContext(context)` / `client.setContext(context)`              | static/instance | context at global or client altitude          |
|  [07]   | `.addHooks(...)` / `client.addHooks(...)` / invocation `{ hooks }` | static/instance | register lifecycle hooks                      |
|  [08]   | `.addHandler(ProviderEvents.X, handler)` / `client.addHandler`     | static/instance | observe readiness and config events           |
|  [09]   | `.setTransactionContextPropagator(p)`                              | static          | install the `AsyncLocalStorage` propagator    |
|  [10]   | `.setTransactionContext(ctx, fn, ...args)`                         | static          | run `fn` under a request-scoped context       |
|  [11]   | `.close()`                                                         | static          | scope-release teardown in `Flags` Layer       |
|  [12]   | `client.track(name, context?, details?)`                           | instance        | associate a flag outcome with an action       |
|  [13]   | `TypedInMemoryProvider`                                            | class           | in-memory provider for SDK-seam specs         |
|  [14]   | `CommonProvider.domainScoped?` / `CommonProvider.track?`           | contract        | optional members a provider literal opts into |

- `.getClient`: its one-argument arm reads as CONTEXT whenever the argument is not a string, so a domain always travels with its own slot.
- No-op propagation is the default, so request-scoped context reaches nothing until a root installs a propagator.
- `CommonProvider.track?` is the outcome seat a provider literal implements, and `CommonProvider.domainScoped?` opts a provider into per-domain instantiation.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every flag read folds through the client, never the provider directly — the client owns hook firing, context altitude, and event handlers, while the provider answers `ResolutionDetails` as total data and never throws from a `resolve*` member.
- Domain binding reaches the provider at init — `initialize` receives the domain the registration named, so a deployment serving several domains from one ruleset source specializes inside one provider class and the domain never becomes a second registry keyed outside the SDK.

[STACKING]:
- `effect`(`.api/effect.md`): the provider's promise members bridge through the runtime captured at Layer build — `Effect.runtime` + `Runtime.runPromise` for the callback seam, `Effect.tryPromise`/`Effect.promise` converting registration and `close` inside the `Flags` scoped build; a `Cache.makeWith` memo tier keys on reason with a TTL the `ConfigurationChanged` handler invalidates.
- `proc/flag` rule engine: the provider projects the pure `Rollout.decide` fold over the live ruleset cell — the SDK owns lifecycle and contract, the page owns targeting semantics and bucket parity.
- `net/channel` `Feed`: ruleset patches arrive over the SSE seam, and the provider emits `ConfigurationChanged` per accepted patch so consumers invalidate on the SDK's own signal.

[LOCAL_ADMISSION]:
- `proc/flag` implements exactly one `Provider`; a second or vendor provider is a roster decision, never a silent import. Domain fan-out rides that one class through the `initialize` domain argument, costing a specialization arm rather than a provider.
- Reason and error vocabularies mirror rather than import into wire shapes — `Rollout.reasons` and `Verdict.codes` anchor the branch spellings against the SDK constants.
