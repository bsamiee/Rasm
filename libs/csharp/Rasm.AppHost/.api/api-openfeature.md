# [RASM_APPHOST_API_OPENFEATURE]

`OpenFeature` owns config-backed flag evaluation for the AppHost features rail: a process-singleton `Api` binds one `InMemoryProvider` per domain, and each `Flag<T>` folds a variant map and a context evaluator for sticky bucketing into typed results. Provider failures surface as `ErrorType` on the result, never as thrown exceptions across the client boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenFeature`
- package: `OpenFeature`
- assembly: `OpenFeature`
- namespace: `OpenFeature`, `OpenFeature.Model`, `OpenFeature.Constant`, `OpenFeature.Providers.Memory`, `OpenFeature.Error`
- asset: runtime library
- rail: features

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: evaluation api and client family

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                |
| :-----: | :---------------------- | :-------------- | :-------------------------- |
|  [01]   | `Api`                   | evaluation root | singleton provider registry |
|  [02]   | `IFeatureClient`        | client contract | typed flag evaluation       |
|  [03]   | `FeatureClient`         | client          | concrete typed evaluation   |
|  [04]   | `IEventBus`             | event contract  | provider event subscription |
|  [05]   | `FlagEvaluationOptions` | options carrier | per-call hooks and hints    |
|  [06]   | `ClientMetadata`        | metadata        | client name and version     |

[PUBLIC_TYPE_SCOPE]: targeting context family

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                    |
| :-----: | :------------------------- | :-------------- | :------------------------------ |
|  [01]   | `EvaluationContext`        | targeting value | immutable bucketing attributes  |
|  [02]   | `EvaluationContextBuilder` | builder         | targeting-key and attribute set |
|  [03]   | `Value`                    | dynamic value   | object-flag and attribute value |
|  [04]   | `Structure`                | nested map      | structured attribute map        |
|  [05]   | `StructureBuilder`         | builder         | structured map construction     |
|  [06]   | `ImmutableMetadata`        | metadata        | typed flag metadata bag         |

- `EvaluationContextBuilder.Set`: overloads admit `string`, `int`, `double`, `long`, `bool`, `DateTime`, `Structure`, `Value`.
- `Value`: `IsBoolean`/`IsNumber`/`IsString`/`IsStructure`/`IsList`/`IsDateTime` discriminators gate the matching nullable `As*` accessors.

[PUBLIC_TYPE_SCOPE]: provider and result family

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]                 |
| :-----: | :------------------------- | :---------------- | :--------------------------- |
|  [01]   | `FeatureProvider`          | provider base     | abstract resolution contract |
|  [02]   | `InMemoryProvider`         | provider          | config-backed evaluation     |
|  [03]   | `Flag<T>`                  | flag definition   | variant map and evaluator    |
|  [04]   | `Flag`                     | flag interface    | non-generic flag base        |
|  [05]   | `ResolutionDetails<T>`     | provider result   | resolved value and reason    |
|  [06]   | `FlagEvaluationDetails<T>` | client result     | client-facing detail carrier |
|  [07]   | `Metadata`                 | provider metadata | provider name                |

- `Flag`: exposes `bool Disabled`, is implemented by `Flag<T>`, and is the value type of the `InMemoryProvider` flag map.
- `FlagEvaluationDetails<T>`: carries `Value`, `FlagKey`, `Reason`, `Variant`, `ErrorType`, `ErrorMessage`, and `FlagMetadata`.

[PUBLIC_TYPE_SCOPE]: outcome vocabulary family

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [CAPABILITY]                 |
| :-----: | :------------------------- | :--------------- | :--------------------------- |
|  [01]   | `Reason`                   | static strings   | evaluation reason vocabulary |
|  [02]   | `ErrorType`                | error enum       | typed evaluation failure     |
|  [03]   | `ProviderStatus`           | lifecycle enum   | provider readiness state     |
|  [04]   | `ProviderEventTypes`       | event enum       | provider event kind          |
|  [05]   | `FeatureProviderException` | provider failure | resolution exception base    |

- [REASON]: `TargetingMatch` `Split` `Disabled` `Default` `Static` `Cached` `Unknown` `Error`
- [ERROR_TYPE]: `None` `ProviderNotReady` `FlagNotFound` `ParseError` `TypeMismatch` `General` `InvalidContext` `TargetingKeyMissing` `ProviderFatal`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: api registration and client acquisition

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------ | :------- | :------------------------- |
|  [01]   | `Api.Instance`                              | static   | global evaluation root     |
|  [02]   | `SetProviderAsync(provider)`                | instance | default provider with init |
|  [03]   | `SetProviderAsync(domain, provider)`        | instance | domain-scoped provider     |
|  [04]   | `GetClient(name, version, logger, context)` | factory  | typed client acquisition   |
|  [05]   | `SetContext(context)`                       | instance | global targeting context   |
|  [06]   | `AddHooks(hook)`                            | instance | global evaluation hooks    |
|  [07]   | `AddHandler(type, handler)`                 | instance | provider event handling    |
|  [08]   | `ShutdownAsync()`                           | instance | provider teardown          |

[ENTRYPOINT_SCOPE]: flag evaluation operations

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :---------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `GetBooleanValueAsync(flagKey, default, ctx)`   | instance | boolean flag value            |
|  [02]   | `GetStringValueAsync(flagKey, default, ctx)`    | instance | string flag value             |
|  [03]   | `GetIntegerValueAsync(flagKey, default, ctx)`   | instance | integer flag value            |
|  [04]   | `GetDoubleValueAsync(flagKey, default, ctx)`    | instance | double flag value             |
|  [05]   | `GetObjectValueAsync(flagKey, default, ctx)`    | instance | structured `Value` flag       |
|  [06]   | `GetBooleanDetailsAsync(flagKey, default, ctx)` | instance | boolean detail with reason    |
|  [07]   | `GetObjectDetailsAsync(flagKey, default, ctx)`  | instance | structured detail with reason |
|  [08]   | `Track(name, ctx, details)`                     | instance | experimentation event emit    |

[ENTRYPOINT_SCOPE]: context and provider construction

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------------------ | :------- | :------------------------ |
|  [01]   | `EvaluationContext.Builder()`                                             | static   | context construction      |
|  [02]   | `EvaluationContextBuilder.SetTargetingKey(key)`                           | instance | sticky bucketing key      |
|  [03]   | `EvaluationContextBuilder.Set(key, value)`                                | instance | targeting attribute       |
|  [04]   | `EvaluationContextBuilder.Build()`                                        | instance | immutable context         |
|  [05]   | `new InMemoryProvider(flags)`                                             | ctor     | config-backed flag set    |
|  [06]   | `InMemoryProvider.UpdateFlagsAsync(flags)`                                | instance | live flag reconfiguration |
|  [07]   | `new Flag<T>(variants, defaultVariant, evaluator?, metadata?, disabled?)` | ctor     | flag construction         |

- `Flag<T>`: `disabled` ctor argument is the sole off-gate that the `Evaluate` branch reads rather than flag metadata.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Api` is a process singleton reached through `Api.Instance`, holding the default and domain-scoped providers, the global `EvaluationContext`, hooks, and the transaction-context propagator.
- `SetProviderAsync` runs the provider `InitializeAsync` before the returned task completes, so awaiting registration observes readiness.
- `EvaluationContext` is immutable; `EvaluationContextBuilder.SetTargetingKey` seats the sticky-bucketing identity.
- `FeatureProvider` is abstract over `ResolveBooleanValueAsync`, `ResolveStringValueAsync`, `ResolveIntegerValueAsync`, `ResolveDoubleValueAsync`, and `ResolveStructureValueAsync`, each returning `ResolutionDetails<T>`.
- `InMemoryProvider` binds one `IDictionary<string, Flag>` over the non-generic `Flag`, so a mixed-type flag set rides one map; each `Flag<T>` folds a `Dictionary<string, T>` variant map, a default variant, an optional `Func<EvaluationContext, string>` context evaluator, an `ImmutableMetadata` bag, and the disabled gate.
- Provider failures surface as `ErrorType` and `Reason.Error` on the result, never as thrown exceptions across the client boundary.

[STACKING]:
- `Microsoft.Extensions.Configuration`(`.api/api-config.md`): the config source chain binds each `Flag<T>` variant row and the targeting-rule order, so a flag edit is one config transition rather than a parallel store.
- `Runtime/features.md` `FlagCompilation`: folds every config `FlagDefinition` into one `Flag<Value>` carrying the `XxHash3` bucketing `Func<EvaluationContext, string>`, registers one `InMemoryProvider` per domain through `SetProviderAsync`, and projects each `Get<Type>DetailsAsync` result onto the canonical `FlagVerdict` seam.

[LOCAL_ADMISSION]:
- One `InMemoryProvider` per domain registers at AppHost composition through `SetProviderAsync`, sourced from configuration-defined `Flag<T>` rows.
- Sticky bucketing lives in the `Flag<T>` context evaluator, which reads the `EvaluationContext` targeting key and returns the variant, never re-implemented at a call site.
- Callers evaluate through `Get<Type>DetailsAsync`, read `Reason` and `Variant` off the typed `FlagEvaluationDetails<T>`, and build targeting context once per request through `EvaluationContext.Builder().SetTargetingKey(...)`.
- `ErrorType` and `Reason` map to features-rail receipts at the boundary; `FeatureProviderException` never crosses into domain logic.

[RAIL_LAW]:
- Package: `OpenFeature`
- Owns: feature-flag and experimentation evaluation with sticky bucketing and variants
- Accept: config-backed `InMemoryProvider`, `Flag<T>` variant maps, and explicit `EvaluationContext` targeting
- Reject: hand-rolled flag lookup, ad hoc percentage rollout math, or string-keyed config reads bypassing the provider
