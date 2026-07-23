# [TS_RUNTIME_API_OPENFEATURE_SERVER_SDK]

`@openfeature/server-sdk` is the vendor-neutral server evaluation API: a global `OpenFeature` singleton registering providers and minting clients, a `Provider` interface whose four `resolve*Evaluation` members answer `ResolutionDetails` per kind (boolean/string/number/object over `JsonValue`), a `Hook` lifecycle (`before`/`after`/`error`/`finally`) injectable globally, per-client, or per-invocation, an event plane (`OpenFeatureEventEmitter` + `ProviderEvents`) signalling readiness and configuration change, evaluation context at global/client/invocation altitude with `targetingKey` as the subject identity, and transaction-context propagation over `AsyncLocalStorage`. `proc/flag` implements the `Provider` over its live ruleset cell and consumes the client so hooks, context, and events ride the standard seam; no second evaluation contract exists TS-side, and the C# evaluator shares the same reason/error vocabulary at the wire.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@openfeature/server-sdk`
- package: `@openfeature/server-sdk` (Apache-2.0)
- module format: ESM + CJS dual; re-exports the `@openfeature/core` shared vocabulary (reasons, error codes, types)
- runtime target: node/bun server processes (`runsOn: "server"` provider gate); the browser peer is `@openfeature/web-sdk`, not admitted
- peer: none hard; providers and hooks are ecosystem packages this branch does not admit — the one provider is `proc/flag`'s own
- rail: flag evaluation contract (runtime folder; catalogued here because the seam resource is folder-local)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the evaluation contract
- rail: boundaries

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CONSUMER]                                                    |
| :-----: | :------------------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Provider`                                   | interface     | `proc/flag#PROVIDER_OWNER` — the one implementation (fence)   |
|  [02]   | `ResolutionDetails<T>`                       | result        | provider answers (fence); `Verdict` projects it               |
|  [03]   | `EvaluationDetails<T>`                       | result        | `ResolutionDetails` + `flagKey`; `get*Details` + hook `after` |
|  [04]   | `EvaluationContext`                          | context       | `targetingKey?` + attributes — the bucket identity            |
|  [05]   | `Hook` / `HookContext` / `FlagValue`         | lifecycle     | `proc/flag`'s telemetry hook (`after` span/metric stamp)      |
|  [06]   | `JsonValue`                                  | object kind   | `resolveObjectEvaluation<T extends JsonValue>` payload        |
|  [07]   | `StandardResolutionReasons` / `ErrorCode`    | vocabulary    | reason/code rows `Rollout.reasons`/`Verdict.codes` mirror     |
|  [08]   | `OpenFeatureEventEmitter` / `ProviderEvents` | events        | `Ready`/`Error`/`ConfigurationChanged`/`Stale`; emit on patch |

[PROVIDER]: `Provider.runsOn: "server"` `Provider.metadata: {name:string}` `Provider.hooks: Hook[]` `Provider.events: OpenFeatureEventEmitter` `Provider.initialize(EvaluationContext?) -> Promise<void>` `Provider.onClose() -> Promise<void>` `Provider.resolveBooleanEvaluation(unknown,unknown,unknown) -> Promise<ResolutionDetails<boolean>>`
[RESOLUTION_DETAILS]: `ResolutionDetails.value: T` `ResolutionDetails.variant: string` `ResolutionDetails.reason: string` `ResolutionDetails.errorCode: ErrorCode` `ResolutionDetails.errorMessage: string` `ResolutionDetails.flagMetadata: FlagMetadata`
[EVALUATION_DETAILS]: `EvaluationDetails = ResolutionDetails<T>&{flagKey:string}`
[EVALUATION_CONTEXT]: `EvaluationContext.targetingKey: string` `EvaluationContext[string]: unknown`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration, clients, context, transaction propagation
- rail: boundaries
- leading-dot surfaces are `OpenFeature` singleton members; readers are `get{Boolean,String,Number,Object}Value(flag, fallback, context?)` and the parallel `get*Details`; `AsyncLocalStorageTransactionContextPropagator` backs transaction context.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CONSUMER]                                         |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `.setProviderAndWait(provider)` / `.setProvider(domain, provider)` | register       | `proc/flag` Layer build; rejection is a boot fault |
|  [02]   | `.getClient(domain?)`                                              | client         | the evaluation client the `Flags` service holds    |
|  [03]   | `client.get*Value`                                                 | evaluate       | value-only reads                                   |
|  [04]   | `client.get*Details`                                               | evaluate       | detail reads the `Verdict` projection consumes     |
|  [05]   | `.setContext(context)` / `client.setContext(context)`              | context        | global/client/invocation altitude                  |
|  [06]   | `.addHooks(...)` / `client.addHooks(...)` / invocation `{ hooks }` | lifecycle      | the telemetry hook registration                    |
|  [07]   | `.addHandler(ProviderEvents.X, handler)` / `client.addHandler`     | events         | readiness/error observation                        |
|  [08]   | `.setTransactionContextPropagator(p)`                              | txn context    | install the `AsyncLocalStorage` propagator         |
|  [09]   | `.setTransactionContext(ctx, fn)`                                  | txn context    | per-request subject propagation at the edge        |
|  [10]   | `.close()`                                                         | shutdown       | scope-release teardown in the `Flags` Layer        |
|  [11]   | `client.track(occurrenceKey, context?, details?)`                  | tracking       | growth row — flag-outcome/action association       |
|  [12]   | `InMemoryProvider`                                                 | test provider  | harness pin for specs exercising the SDK seam      |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `effect` (`.api/effect.md`): the provider's promise members bridge through the runtime captured at Layer build (`Effect.runtime` + `Runtime.runPromise`) — the sanctioned callback seam; registration and close convert through `Effect.tryPromise`/`Effect.promise` inside the `Flags` scoped build; the memo tier is `Cache.makeWith` with the reason-keyed TTL, invalidated by the `ConfigurationChanged` handler.
- `proc/flag` rule engine: the provider is a projection over the pure `Rollout.decide` fold and the live ruleset cell — the SDK owns lifecycle and contract, the page owns targeting semantics and bucket parity.
- `net/channel` `Feed`: the live ruleset patches arrive over the SSE seam; the provider emits `ConfigurationChanged` per accepted patch so SDK consumers invalidate on the SDK's own signal.

[LOCAL_ADMISSION]:
- Implement exactly one `Provider`; a second provider or a vendor provider package is a roster decision, never a silent import.
- Answer degradation as data — `errorCode` rows inside `ResolutionDetails` — and never throw from a `resolve*` member; the bridged effect is total.
- Evaluate through the client, never the provider directly — hooks and context altitude exist only on the client path.
- Keep the reason/error vocabularies mirrored, not imported into wire shapes — `Rollout.reasons` and `Verdict.codes` anchor the branch spellings, and the SDK constants are the parity reference.

[RAIL_LAW]:
- Package: `@openfeature/server-sdk`
- Owns: the evaluation contract — `Provider`/`ResolutionDetails`/`EvaluationContext`, the client surface, hooks, events, transaction-context propagation, the reason and error-code vocabulary
- Accept: one `Provider` implementation over the ruleset cell, `setProviderAndWait` at Layer build with `close()` on release, client `get*Details` reads projected into `Verdict`, one telemetry hook, `ConfigurationChanged`-driven invalidation
- Reject: a hand-minted evaluation contract beside the SDK, throwing resolve members, provider-direct evaluation bypassing hooks, a second flag source beside the provider's cell
