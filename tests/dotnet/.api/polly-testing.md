# [DOTNET_TESTING_API_POLLY_TESTING]

`Polly.Testing` opens a BUILT `ResiliencePipeline` for inspection: one extension flattens the composed component tree into an ordered `ResilienceStrategyDescriptor` roster carrying each strategy's options, so a spec asserts what a composition assembled rather than what its builder body reads like. It closes the gap a resolution probe leaves — a pipeline resolves by key while silently missing a strategy arm, and only the descriptor separates those two states.

## [01]-[PACKAGE_SURFACE]

- package: `Polly.Testing`
- license: `BSD-3-Clause`
- namespace: `Polly.Testing`
- asset: `lib/net8.0/Polly.Testing.dll` over `Polly.Core`
- rail: evidence — composition inspection of built resilience pipelines; injected per suite with `PrivateAssets="all"`

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                       | [KIND]       | [CAPABILITY]                                                      |
| :-----: | :----------------------------- | :----------- | :---------------------------------------------------------------- |
|  [01]   | `ResiliencePipelineDescriptor` | descriptor   | flattened strategy roster and reload flag over one BUILT pipeline |
|  [02]   | `ResilienceStrategyDescriptor` | descriptor   | one strategy's options carrier beside its opaque instance handle  |
|  [03]   | `ResiliencePipelineExtensions` | static entry | the `GetPipelineDescriptor` pair over both pipeline arities       |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                             | [KIND]    | [CAPABILITY]                                                       |
| :-----: | :---------------------------------------------------- | :-------- | :----------------------------------------------------------------- |
|  [01]   | `ResiliencePipeline.GetPipelineDescriptor()`          | extension | descriptor over the non-generic pipeline                           |
|  [02]   | `ResiliencePipeline<TResult>.GetPipelineDescriptor()` | extension | descriptor over the result-typed pipeline                          |
|  [03]   | `ResiliencePipelineDescriptor.Strategies`             | property  | `IReadOnlyList<ResilienceStrategyDescriptor>` in composition order |
|  [04]   | `ResiliencePipelineDescriptor.FirstStrategy`          | property  | `Strategies[0]`; an empty pipeline throws rather than refusing     |
|  [05]   | `ResiliencePipelineDescriptor.IsReloadable`           | property  | a reload wrapper stands somewhere in the component tree            |
|  [06]   | `ResilienceStrategyDescriptor.Options`                | property  | `ResilienceStrategyOptions?` — null where a strategy carried none  |
|  [07]   | `ResilienceStrategyDescriptor.StrategyInstance`       | property  | `object`; every concrete strategy type is internal                 |

```csharp
namespace Polly.Testing;

public static class ResiliencePipelineExtensions {
    public static ResiliencePipelineDescriptor GetPipelineDescriptor<TResult>(this ResiliencePipeline<TResult> pipeline);
    public static ResiliencePipelineDescriptor GetPipelineDescriptor(this ResiliencePipeline pipeline);
}

public sealed class ResiliencePipelineDescriptor {
    public IReadOnlyList<ResilienceStrategyDescriptor> Strategies { get; }
    public ResilienceStrategyDescriptor FirstStrategy { get; }
    public bool IsReloadable { get; }
}

public sealed class ResilienceStrategyDescriptor {
    public ResilienceStrategyOptions? Options { get; }
    public object StrategyInstance { get; }
}
```

## [04]-[IMPLEMENTATION_LAW]

[FLATTENING]: expansion walks the component tree and yields a FLAT roster, so nesting depth is invisible to the assertion — a composite recurses into its children, and the execution-tracking, dispose-callback, and external wrappers each unwrap to their inner component without appearing as strategies. Only the reload wrapper survives the walk, and it still never reaches `Strategies` because the projection admits bridge components alone; `IsReloadable` is its sole observable. Nested `AddPipeline` composition therefore reads as its constituent strategies inline, never as one entry.

[OPTIONS_ARE_THE_ORACLE]: every concrete strategy class is internal, so `StrategyInstance` types to `object` and carries no assertable identity — the strategy's public identity is its `Options` type (`RetryStrategyOptions<T>`, `CircuitBreakerStrategyOptions`, `TimeoutStrategyOptions`, and kin) beside the `Name` that options row sets. Strategies admitted through a bare custom-strategy arm carry no options at all, so `Options` is nullable by construction and a roster assertion pattern-matches the option type rather than indexing blind.

[STACKING]:
- `Polly.Core` (`libs/dotnet/Rasm.AppHost/.api/api-polly-core.md`): the descriptor reads what `ResiliencePipelineBuilder` assembled, and the option types it surfaces are that catalogue's strategy-options family.
- `Polly.Extensions` (`libs/dotnet/Rasm.AppHost/.api/api-polly-extensions.md`): a registry-resolved pipeline inspects identically, so a keyed pipeline pulled from `ResiliencePipelineProvider<TKey>` is the natural subject; the telemetry strategy `ConfigureTelemetry` inserts at the head appears in the roster like any other.
- `Rasm.TestKit`: roster comparisons ride the kit's table folds — one row per expected strategy — so a missing arm names itself in the verdict rather than failing an opaque count.

[LOCAL_ADMISSION]:
- Inspection targets a BUILT pipeline; a spec that re-runs the builder body to predict its own expectation is the mirror-test defect and proves nothing about composition.
- Order is semantics: the roster asserts the composed SEQUENCE, because strategy order decides whether a timeout bounds a retry or each attempt.
- Executing a pipeline to infer its shape from observed behavior is the rejected form where the descriptor answers directly; behavioral proof stays for the strategies' own effects.

[RAIL_LAW]:
- Package: `Polly.Testing`
- Owns: composition inspection of built resilience pipelines inside C# specs.
- Accept: descriptor roster assertions over option types, strategy names, order, and the reload flag.
- Reject: assertions on `StrategyInstance` runtime types, resolution-only probes standing in for composition proof, and hand-rolled reflection over pipeline internals.
