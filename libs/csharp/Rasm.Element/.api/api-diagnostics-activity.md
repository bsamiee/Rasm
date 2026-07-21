# [RASM_ELEMENT_API_DIAGNOSTICS_ACTIVITY]

Injectable monotonic timing is this overlay's slice: `TimeProvider` supplies the timestamp pair every timed seam decoration derives its `Duration` from, so elapsed truth never rides a wall-clock diff. General span surface — source mint, listener gate, bracket, status — lives at `libs/csharp/.api/api-diagnostics-activity.md`. Members are decompile-verified against the `net10.0` framework reference assembly.

## [01]-[PACKAGE_SURFACE]

- Package: BCL inbox
- License: MIT
- Namespace: `System`
- Asset: shared framework
- Rail: injected monotonic clock behind every `rasm.element.*` timed decoration

## [02]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                             | [KIND]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `TimeProvider.System`                                 | default | process system clock singleton        |
|  [02]   | `TimeProvider.GetTimestamp()`                         | read    | monotonic timestamp                   |
|  [03]   | `TimeProvider.GetElapsedTime(long startingTimestamp)` | derive  | elapsed `TimeSpan` from one timestamp |

## [03]-[IMPLEMENTATION_LAW]

- `ElementHookRail` carries the injected `TimeProvider` and `ElementTap.Timed` is the one timing kernel — the timestamp read precedes the body, elapsed derives from the rail's clock, and a test host swaps the provider without touching a seam.
- `GraphInstrument.Traced` (`.planning/Projection/observe.md`) composes the general bracket: `StartActivity(name, ActivityKind.Internal)` over a rail-valued fold, `SetStatus(ActivityStatusCode.Error, error.Message)` on the fail side.
