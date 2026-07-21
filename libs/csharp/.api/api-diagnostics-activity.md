# [RASM_API_DIAGNOSTICS_ACTIVITY]

`System.Diagnostics` activity tracing is the vendor-neutral span surface every library-tier bracket composes: spans open through one `ActivitySource` per instrumentation scope, listener absence makes `StartActivity` return `null` as the free fast path, and the SDK that samples, exports, and correlates lives at the composition root. No manifest row exists — the surface ships in-box on the framework — and no OpenTelemetry type is reachable from an emitting library. Members are decompile-verified against the `net10.0` framework reference assembly.

## [01]-[PACKAGE_SURFACE]

- Package: BCL inbox
- License: MIT
- Namespace: `System.Diagnostics`
- Asset: `System.Diagnostics.DiagnosticSource.dll` (shared framework)
- Rail: library-tier span emission behind every `rasm.*` instrumentation scope

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]             | [KIND]      | [CAPABILITY]                                              |
| :-----: | :------------------- | :---------- | :-------------------------------------------------------- |
|  [01]   | `ActivitySource`     | scope owner | listener-gated span factory; `IDisposable` owner          |
|  [02]   | `Activity`           | span        | live span carrying status, tags, baggage, trace identity  |
|  [03]   | `ActivityKind`       | enum        | span-role vocabulary; `Internal` the in-process kind      |
|  [04]   | `ActivityStatusCode` | enum        | `Unset`/`Ok`/`Error` terminal verdict vocabulary          |
|  [05]   | `ActivityContext`    | identity    | W3C trace-id/span-id/flags parent slot a propagator fills |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                    | [KIND]  | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `ActivitySource(string name, string? version = "")`                          | mint    | one source per version-stamped scope     |
|  [02]   | `ActivitySource.HasListeners()`                                              | probe   | pre-payload guard for costly tag builds  |
|  [03]   | `StartActivity(string name = "", ActivityKind kind = ActivityKind.Internal)` | open    | `Activity?`; `null` unlistened           |
|  [04]   | `Activity.SetStatus(ActivityStatusCode code, string? description = null)`    | verdict | terminal status; returns the span        |
|  [05]   | `Activity.SetTag(string key, object? value)`                                 | tag     | bounded span dimension; returns the span |
|  [06]   | `Activity.Current`                                                           | read    | ambient span; `Activity?`                |
|  [07]   | `Activity.Dispose()`                                                         | close   | span stop and listener notification      |
|  [08]   | `ActivitySource.Dispose()`                                                   | close   | removes the source from the global list  |

## [04]-[IMPLEMENTATION_LAW]

- One `ActivitySource` per instrumentation scope, name and version identical to the scope's `Meter` mint, so meter and source admit together at the composition root by one spelling.
- `StartActivity` returns `null` when no listener subscribes, so a `using`-scoped bracket costs one null test on the unobserved path and a span body never guards emission itself.
- Failed typed rails stamp `ActivityStatusCode.Error` with the typed error's message; the typed verdict stays domain truth and never demotes to a tag.
- Sampling, exporters, exemplar linkage, W3C propagation, and resource identity are SDK altitude — the composition root's — so an emitting library holds only the source and the bracket.

## [05]-[STACKING]

- `Rasm` `Domain/telemetry#SIGNAL_TAP`: `SpanBand` mints one source per `KernelDomain` row (`rasm.rasm.<domain>` names) and `Traced` is the rail-valued bracket every measured kernel entry composes.
- `System.Diagnostics.Metrics` (`libs/csharp/.api/api-diagnostics-metrics.md`): the sibling metric surface in the same assembly; scope name and version cross both mints.
- Folder overlays carry the genuine slices alone — `Rasm.Bim/.api/api-diagnostics-activity.md` the ambient baggage and W3C context reads, `Rasm.Element/.api/api-diagnostics-activity.md` the `TimeProvider` timing slice; the general surface lives here once.
