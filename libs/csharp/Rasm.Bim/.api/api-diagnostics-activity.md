# [RASM_BIM_API_DIAGNOSTICS_ACTIVITY]

Ambient W3C context and baggage reads are this overlay's slice: `Activity.Current` carries trace identity and promoted baggage across async flow, and the Bim instrument tap reads tenant and model attribution off it with zero domain-signature threading. General span surface — source mint, listener gate, bracket, status — lives at `libs/csharp/.api/api-diagnostics-activity.md`.

## [01]-[PACKAGE_SURFACE]

- Package: BCL inbox
- Assembly: `System.Diagnostics.DiagnosticSource.dll`
- Namespace: `System.Diagnostics`
- Asset: `net10.0`
- Rail: ambient trace-identity and baggage propagation reads

## [02]-[ENTRYPOINTS]

| [INDEX] | [MEMBER]                                                       | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `Activity.Current` -> `Activity?`                              | ambient span read                           |
|  [02]   | `Activity.Id` -> `string?`                                     | W3C `traceparent` value under W3C id format |
|  [03]   | `Activity.TraceStateString` -> `string?`                       | W3C `tracestate` value                      |
|  [04]   | `Activity.GetBaggageItem(string key)` -> `string?`             | ambient baggage read                        |
|  [05]   | `Activity.AddBaggage(string key, string? value)` -> `Activity` | Activity-visible baggage promotion          |

## [03]-[IMPLEMENTATION_LAW]

- `Activity.Current` is the Activity-visible baggage store — a BCL-only reader reaches ONLY this chain, never the OTel SDK `Baggage.Current` store; app-tier propagation promotes allowed keys through `AddBaggage` or the extraction path into this store before library instruments read them.
- `BimTelemetry.Attributed` reads `rasm.tenant`/`rasm.model` off `GetBaggageItem` once per fact; an absent key omits its tag, so no empty-string series mints and no domain signature grows a tenant slot.
