# [RASM_APPHOST_API_OPENFEATURE]

`OpenFeature` supplies the vendor-neutral feature-flag evaluation contract: a singleton evaluation `Api`, typed flag clients, an immutable targeting `EvaluationContext`, provider `ResolutionDetails`/`FlagEvaluationDetails` carriers, reason and error vocabularies, and an `InMemoryProvider` whose `Flag<T>` rows carry variant maps and a context evaluator for sticky bucketing. It serves the AppHost runtime/features rail as the config-backed provider, sticky bucketing, and variant evaluation surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenFeature`

- package: `OpenFeature`
- assembly: `OpenFeature`
- namespace: `OpenFeature`
- namespace: `OpenFeature.Model`
- namespace: `OpenFeature.Constant`
- namespace: `OpenFeature.Providers.Memory`
- namespace: `OpenFeature.Error`
- asset: runtime library
- rail: features

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: evaluation api and client family

- rail: features

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                      |
| :-----: | :---------------------- | :-------------- | :-------------------------- |
|  [01]   | `Api`                   | evaluation root | singleton provider registry |
|  [02]   | `IFeatureClient`        | client contract | typed flag evaluation       |
|  [03]   | `FeatureClient`         | client          | concrete typed evaluation   |
|  [04]   | `IEventBus`             | event contract  | provider event subscription |
|  [05]   | `FlagEvaluationOptions` | options carrier | per-call hooks and hints    |
|  [06]   | `ClientMetadata`        | metadata        | client name and version     |

[PUBLIC_TYPE_SCOPE]: targeting context family

- rail: features

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [RAIL]                          |
| :-----: | :------------------------- | :-------------- | :------------------------------ |
|  [01]   | `EvaluationContext`        | targeting value | immutable bucketing attributes  |
|  [02]   | `EvaluationContextBuilder` | builder         | targeting-key and attribute set |
|  [03]   | `Value`                    | dynamic value   | object-flag and attribute value |
|  [04]   | `Structure`                | nested map      | structured attribute map        |
|  [05]   | `StructureBuilder`         | builder         | structured map construction     |
|  [06]   | `ImmutableMetadata`        | metadata        | typed flag metadata bag         |

[PUBLIC_TYPE_SCOPE]: provider and result family

- rail: features

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]                 |
| :-----: | :------------------------- | :---------------- | :--------------------------- |
|  [01]   | `FeatureProvider`          | provider base     | abstract resolution contract |
|  [02]   | `InMemoryProvider`         | provider          | config-backed evaluation     |
|  [03]   | `Flag<T>`                  | flag definition   | variant map plus evaluator   |
|  [04]   | `Flag`                     | flag interface    | non-generic flag base        |
|  [05]   | `ResolutionDetails<T>`     | provider result   | resolved value and reason    |
|  [06]   | `FlagEvaluationDetails<T>` | client result     | client-facing detail carrier |
|  [07]   | `Metadata`                 | provider metadata | provider name                |

[FLAG_BASE]: `Flag` exposes `bool Disabled`, is implemented by `Flag<T>`, and is the value type of the `InMemoryProvider` flag map.

[PUBLIC_TYPE_SCOPE]: outcome vocabulary family

- rail: features

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [RAIL]                       |
| :-----: | :------------------------- | :--------------- | :--------------------------- |
|  [01]   | `Reason`                   | static strings   | evaluation reason vocabulary |
|  [02]   | `ErrorType`                | error enum       | typed evaluation failure     |
|  [03]   | `ProviderStatus`           | lifecycle enum   | provider readiness state     |
|  [04]   | `ProviderEventTypes`       | event enum       | provider event kind          |
|  [05]   | `FeatureProviderException` | provider failure | resolution exception base    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: api registration and client acquisition

- rail: features

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]     | [RAIL]                     |
| :-----: | :------------------------------------------ | :----------------- | :------------------------- |
|  [01]   | `Api.Instance`                              | singleton accessor | global evaluation root     |
|  [02]   | `SetProviderAsync(provider)`                | provider registry  | default provider with init |
|  [03]   | `SetProviderAsync(domain, provider)`        | provider registry  | domain-scoped provider     |
|  [04]   | `GetClient(name, version, logger, context)` | client factory     | typed client acquisition   |
|  [05]   | `SetContext(context)`                       | context mutation   | global targeting context   |
|  [06]   | `AddHooks(hook)`                            | hook registration  | global evaluation hooks    |
|  [07]   | `AddHandler(type, handler)`                 | event subscription | provider event handling    |
|  [08]   | `ShutdownAsync()`                           | lifecycle          | provider teardown          |

[ENTRYPOINT_SCOPE]: flag evaluation operations

- rail: features

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]    | [RAIL]                        |
| :-----: | :---------------------------------------------- | :---------------- | :---------------------------- |
|  [01]   | `GetBooleanValueAsync(flagKey, default, ctx)`   | value evaluation  | boolean flag value            |
|  [02]   | `GetStringValueAsync(flagKey, default, ctx)`    | value evaluation  | string flag value             |
|  [03]   | `GetIntegerValueAsync(flagKey, default, ctx)`   | value evaluation  | integer flag value            |
|  [04]   | `GetDoubleValueAsync(flagKey, default, ctx)`    | value evaluation  | double flag value             |
|  [05]   | `GetObjectValueAsync(flagKey, default, ctx)`    | value evaluation  | structured `Value` flag       |
|  [06]   | `GetBooleanDetailsAsync(flagKey, default, ctx)` | detail evaluation | boolean detail with reason    |
|  [07]   | `GetObjectDetailsAsync(flagKey, default, ctx)`  | detail evaluation | structured detail with reason |
|  [08]   | `Track(name, ctx, details)`                     | tracking          | experimentation event emit    |

[ENTRYPOINT_SCOPE]: context and provider construction

- rail: features

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]    | [CAPABILITY]              |
| :-----: | :----------------------------------------------------------------- | :---------------- | :------------------------ |
|  [01]   | `EvaluationContext.Builder()`                                      | builder factory   | context construction      |
|  [02]   | `EvaluationContextBuilder.SetTargetingKey(key)`                    | builder mutation  | sticky bucketing key      |
|  [03]   | `EvaluationContextBuilder.Set(key, value)`                         | builder mutation  | targeting attribute       |
|  [04]   | `EvaluationContextBuilder.Build()`                                 | builder finalize  | immutable context         |
|  [05]   | `new InMemoryProvider(flags)`                                      | provider ctor     | config-backed flag set    |
|  [06]   | `InMemoryProvider.UpdateFlagsAsync(flags)`                         | provider mutation | live flag reconfiguration |
|  [07]   | `new Flag<T>(variants, default, evaluator?, metadata?, disabled?)` | flag ctor         | flag construction         |

[FLAG_DISABLE_GATE]: The `disabled` constructor argument is the provider's sole off-gate; the `Evaluate` branch reads this argument rather than flag metadata.

## [04]-[IMPLEMENTATION_LAW]

[FEATURES_TOPOLOGY]:

- namespaces: evaluation api, model, constant vocabulary, in-memory provider, error
- evaluation root: `Api` is a process singleton reached through `Api.Instance`; it holds the default provider, domain-scoped providers, the global `EvaluationContext`, hooks, and the transaction-context propagator
- provider registration: `SetProviderAsync` runs provider `InitializeAsync` before the returned task completes; awaiting it is how readiness is observed
- client surface: `GetClient` returns a `FeatureClient` whose `ProviderStatus` mirrors `ProviderStatus.NotReady`/`Ready`/`Stale`/`Error`/`Fatal`
- value surface: `Get<Type>ValueAsync` returns the resolved value or the supplied default; `Get<Type>DetailsAsync` returns `FlagEvaluationDetails<T>` carrying `Value`, `FlagKey`, `Reason`, `Variant`, `ErrorType`, `ErrorMessage`, and `FlagMetadata`
- object surface: `GetObjectValueAsync`/`GetObjectDetailsAsync` carry a `Value` whose `IsBoolean`/`IsNumber`/`IsString`/`IsStructure`/`IsList`/`IsDateTime` discriminators gate `AsBoolean`/`AsDouble`/`AsString`/`AsStructure`/`AsList`/`AsDateTime` nullable accessors
- targeting surface: `EvaluationContext` is immutable; `EvaluationContextBuilder.SetTargetingKey` carries the sticky bucketing identity and `Set` overloads admit `string`, `int`, `double`, `long`, `bool`, `DateTime`, `Structure`, and `Value` attributes
- provider contract: `FeatureProvider` is abstract over `ResolveBooleanValueAsync`, `ResolveStringValueAsync`, `ResolveIntegerValueAsync`, `ResolveDoubleValueAsync`, and `ResolveStructureValueAsync`, each returning `ResolutionDetails<T>`
- in-memory surface: `InMemoryProvider` is constructed from an `IDictionary<string, Flag>` keyed by flag id over the non-generic `Flag` interface (`bool Disabled`) that each `Flag<T>` implements, so a mixed-type flag set rides one map; `Flag<T>` carries a `Dictionary<string, T>` variant map, a default variant name, an optional `Func<EvaluationContext, string>` context evaluator that picks the variant from targeting, an `ImmutableMetadata` bag, and a `disabled` flag
- reason vocabulary: `Reason.TargetingMatch`, `Reason.Split`, `Reason.Disabled`, `Reason.Default`, `Reason.Static`, `Reason.Cached`, `Reason.Unknown`, and `Reason.Error` are the string reasons surfaced on results
- error vocabulary: `ErrorType` carries `None`, `ProviderNotReady`, `FlagNotFound`, `ParseError`, `TypeMismatch`, `General`, `InvalidContext`, `TargetingKeyMissing`, and `ProviderFatal`, each tagged with a wire `[Description]`
- failure discipline: provider failures surface as `ErrorType` plus `Reason.Error` on the result, not as thrown exceptions across the client boundary

[LOCAL_ADMISSION]:

- The features rail registers exactly one `InMemoryProvider` per domain through `SetProviderAsync` at AppHost composition, sourced from configuration-defined `Flag<T>` rows.
- Sticky bucketing lives in the `Flag<T>` context evaluator: the `Func<EvaluationContext, string>` reads the `EvaluationContext` targeting key and attributes and returns the variant name, never re-implemented in calling code.
- Callers evaluate through `Get<Type>DetailsAsync` and consume the typed `FlagEvaluationDetails<T>` carrier, reading `Reason` and `Variant` for experimentation decisions instead of re-deriving them.
- Targeting context is built once per request through `EvaluationContext.Builder().SetTargetingKey(...)` and passed to the evaluation call; ambient global context stays for cross-cutting attributes only.
- `ErrorType` and `Reason` values map to canonical features-rail receipts at the boundary; raw `FeatureProviderException` instances never cross into domain logic.

[RAIL_LAW]:

- Package: `OpenFeature`
- Owns: feature-flag and experimentation evaluation with sticky bucketing and variants
- Accept: config-backed `InMemoryProvider`, `Flag<T>` variant maps, and explicit `EvaluationContext` targeting
- Reject: hand-rolled flag lookup, ad hoc percentage rollout math, or string-keyed config reads bypassing the provider
