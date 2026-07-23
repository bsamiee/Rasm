# [RASM_API_DIAGNOSTICS_ACTIVITY]

`System.Diagnostics` activity tracing owns span identity, lifetime, and payload for every library-tier bracket: one `ActivitySource` per instrumentation scope mints spans a registered listener gates. Trace identity, W3C context, and the carrier propagator ship in the same assembly, so an emitting library reaches the whole boundary with no SDK reference.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Diagnostics.DiagnosticSource`
- package: BCL inbox (MIT)
- assembly: `System.Diagnostics.DiagnosticSource.dll` (shared framework)
- namespace: `System.Diagnostics`
- rail: library-tier span emission behind every `rasm.*` instrumentation scope

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: span owners, trace-identity value types, payload carriers, listener contracts.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `ActivitySource`               | class         | listener-gated span factory for one scope; `IDisposable` owner      |
|  [02]   | `ActivitySourceOptions`        | class         | mint payload: `Name`, `Version`, `Tags`, `TelemetrySchemaUrl`       |
|  [03]   | `Activity`                     | class         | live span: status, tags, events, links, baggage, trace identity     |
|  [04]   | `Activity.Enumerator<T>`       | struct        | `ref`-current walk over tags, events, links with no allocation      |
|  [05]   | `ActivityKind`                 | enum          | `Internal` `Server` `Client` `Producer` `Consumer` span role        |
|  [06]   | `ActivityStatusCode`           | enum          | `Unset` `Ok` `Error` terminal verdict                               |
|  [07]   | `ActivityIdFormat`             | enum          | `W3C` `Hierarchical` `Unknown` id shape                             |
|  [08]   | `ActivityTraceFlags`           | enum          | `None` `Recorded` sampling bit carried on the wire                  |
|  [09]   | `ActivityContext`              | struct        | trace id, span id, flags, `tracestate`, remote bit                  |
|  [10]   | `ActivityTraceId`              | struct        | 16-byte trace identity over hex, UTF-8, and byte admission          |
|  [11]   | `ActivitySpanId`               | struct        | 8-byte span identity over hex, UTF-8, and byte admission            |
|  [12]   | `ActivityEvent`                | struct        | named timestamped point carrying its own tag set                    |
|  [13]   | `ActivityLink`                 | struct        | causal edge to a foreign `ActivityContext`                          |
|  [14]   | `ActivityTagsCollection`       | class         | heap tag dictionary event and link construction takes               |
|  [15]   | `TagList`                      | struct        | inline tag buffer; `params ReadOnlySpan` ctor, no heap              |
|  [16]   | `ActivityListener`             | class         | start/stop, sampling, and exception-recorder subscription           |
|  [17]   | `ActivitySamplingResult`       | enum          | `None` `PropagationData` `AllData` `AllDataAndRecorded`             |
|  [18]   | `ActivityCreationOptions<T>`   | struct        | sampling input: parent, tags, links, `SamplingTags`, `TraceState`   |
|  [19]   | `SampleActivity<T>`            | delegate      | `ref` sampling verdict over the creation options                    |
|  [20]   | `ExceptionRecorder`            | delegate      | listener-owned `ref TagList` exception projection                   |
|  [21]   | `ActivityChangedEventArgs`     | struct        | `Previous`/`Current` pair on ambient-span change                    |
|  [22]   | `DistributedContextPropagator` | class         | `Inject`/`ExtractTraceIdAndState`/`ExtractBaggage` over any carrier |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scope mint, listener gate, open and close bracket, payload writes, and context propagation.

Every `tags` parameter resolves to `IEnumerable<KeyValuePair<string, object?>>?`, every `links` parameter to `IEnumerable<ActivityLink>?`, and `start` to `DateTimeOffset`.

| [INDEX] | [SURFACE]                                                                                 | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `ActivitySource(string, string?, tags)`                                                   | ctor     | version-stamped scope mint       |
|  [02]   | `ActivitySource(ActivitySourceOptions)`                                                   | ctor     | mint carrying a schema url       |
|  [03]   | `ActivitySource.HasListeners() -> bool`                                                   | instance | pre-payload gate before the open |
|  [04]   | `ActivitySource.StartActivity(string, ActivityKind) -> Activity?`                         | instance | open under the ambient parent    |
|  [05]   | `ActivitySource.StartActivity(string, ActivityKind, ActivityContext, tags, links, start)` | instance | explicit parent, links, backdate |
|  [06]   | `ActivitySource.CreateActivity(string, ActivityKind) -> Activity?`                        | instance | build unstarted for later arming |
|  [07]   | `ActivitySource.AddActivityListener(ActivityListener)`                                    | static   | process-wide subscription        |
|  [08]   | `ActivitySource.Dispose()`                                                                | instance | drop from the global source list |
|  [09]   | `Activity.Start() -> Activity`                                                            | instance | arm a built span                 |
|  [10]   | `Activity.Stop()`                                                                         | instance | stop with listener notification  |
|  [11]   | `Activity.Dispose()`                                                                      | instance | `using`-bracket stop             |
|  [12]   | `Activity.SetStartTime(DateTime) -> Activity`                                             | instance | backdate an armed span           |
|  [13]   | `Activity.SetEndTime(DateTime) -> Activity`                                               | instance | close on a captured instant      |
|  [14]   | `Activity.IsAllDataRequested -> bool`                                                     | property | listener verdict gating tag cost |
|  [15]   | `Activity.SetStatus(ActivityStatusCode, string?) -> Activity`                             | instance | terminal verdict; yields itself  |
|  [16]   | `Activity.SetTag(string, object?) -> Activity`                                            | instance | set-or-replace one dimension     |
|  [17]   | `Activity.AddTag(string, object?) -> Activity`                                            | instance | append with no replace scan      |
|  [18]   | `Activity.GetTagItem(string) -> object?`                                                  | instance | one tag read                     |
|  [19]   | `Activity.AddEvent(ActivityEvent) -> Activity`                                            | instance | timestamped point append         |
|  [20]   | `Activity.AddLink(ActivityLink) -> Activity`                                              | instance | causal edge append after start   |
|  [21]   | `Activity.AddException(Exception, in TagList, DateTimeOffset) -> Activity`                | instance | exception event via the recorder |
|  [22]   | `Activity.SetCustomProperty(string, object?)`                                             | instance | process-local, never exported    |
|  [23]   | `Activity.GetCustomProperty(string) -> object?`                                           | instance | process-local payload read       |
|  [24]   | `Activity.EnumerateTagObjects() -> Activity.Enumerator<KeyValuePair<string, object?>>`    | instance | allocation-free tag walk         |
|  [25]   | `Activity.EnumerateEvents() -> Activity.Enumerator<ActivityEvent>`                        | instance | allocation-free event walk       |
|  [26]   | `Activity.EnumerateLinks() -> Activity.Enumerator<ActivityLink>`                          | instance | allocation-free link walk        |
|  [27]   | `Activity.DisplayName -> string`                                                          | property | name distinct from operation id  |
|  [28]   | `Activity.Duration -> TimeSpan`                                                           | property | elapsed span, valid after stop   |
|  [29]   | `Activity.Current -> Activity?`                                                           | property | ambient span across async flow   |
|  [30]   | `Activity.CurrentChanged`                                                                 | static   | ambient-span change event        |
|  [31]   | `Activity.AddBaggage(string, string?) -> Activity`                                        | instance | promote a key down the chain     |
|  [32]   | `Activity.SetBaggage(string, string?) -> Activity`                                        | instance | set-or-replace one baggage key   |
|  [33]   | `Activity.GetBaggageItem(string) -> string?`                                              | instance | baggage read across parents      |
|  [34]   | `Activity.Context -> ActivityContext`                                                     | property | outbound propagation payload     |
|  [35]   | `Activity.Id -> string?`                                                                  | property | W3C `traceparent` value          |
|  [36]   | `Activity.TraceStateString -> string?`                                                    | property | W3C `tracestate` passthrough     |
|  [37]   | `Activity.SetParentId(ActivityTraceId, ActivitySpanId, ActivityTraceFlags) -> Activity`   | instance | manual parent outside a mint     |
|  [38]   | `ActivityContext.TryParse(string?, string?, bool, out ActivityContext) -> bool`           | static   | `traceparent` admission          |
|  [39]   | `ActivityTraceId.CreateRandom() -> ActivityTraceId`                                       | static   | fresh trace identity             |
|  [40]   | `ActivitySpanId.CreateRandom() -> ActivitySpanId`                                         | static   | fresh span identity              |
|  [41]   | `TagList(params ReadOnlySpan<KeyValuePair<string, object?>>)`                             | ctor     | stack tag buffer for a write     |
|  [42]   | `DistributedContextPropagator.Current`                                                    | static   | process propagator seat          |
|  [43]   | `DistributedContextPropagator.CreateW3CPropagator()`                                      | static   | W3C `traceparent` carrier codec  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `ActivitySource` per instrumentation scope owns every span that scope emits, minted at composition and disposed with it.
- `StartActivity` yields `null` under listener absence, so a `using`-scoped bracket costs one null test on the unobserved path and a span body never guards emission.
- `HasListeners()` gates the whole payload build ahead of the open; `IsAllDataRequested` gates per-span tag cost once a listener admits the span.
- Failed typed rails stamp `ActivityStatusCode.Error` with the error message and record the exception through `AddException` over a stack-allocated `TagList`; the typed verdict stays domain truth, never a tag.
- Parent context, causal links, and a backdated start ride the parent-bearing `StartActivity` overload; the name-and-kind form parents on `Activity.Current`.
- `Activity.Current` is the Activity-visible baggage store — a BCL-only reader reaches only this chain, never the OTel SDK `Baggage.Current` store; app-tier propagation promotes allowed keys through `AddBaggage` or the extraction path before library instruments read them.
- `AddActivityListener`, `ActivityListener.Sample`, and `DistributedContextPropagator.Current` bind at the composition root, so an emitting library holds the source and the bracket alone.
- Exporters, resource identity, and exemplar linkage live outside this assembly on the OpenTelemetry SDK.

[STACKING]:
- `System.Diagnostics.Metrics`(`.api/api-diagnostics-metrics.md`): one scope name and version spells both this `ActivitySource` mint and the `Meter` mint, so span and instrument admit together at the composition root.
- `OpenTelemetry`(`.api/api-opentelemetry.md`): `TracerProviderBuilder.AddSource(params string[])` admits these source names and seats the sampler and processor chain an emitting library never references.
- `SpanBand`: freezes one `ActivitySource` per `KernelDomain` trace scope, and `Traced` folds `HasListeners`, the `using` open, and `MapFail`-driven `SetStatus` into one `Fin` bracket every measured kernel entry composes.
- `Rasm.Element`: `GraphInstrument.Traced` composes the general bracket — `StartActivity(name, ActivityKind.Internal)` over a rail-valued fold, `SetStatus(ActivityStatusCode.Error, error.Message)` on the fail side.
- `Rasm.Bim`: `BimTelemetry.Attributed` reads `rasm.tenant`/`rasm.model` off `GetBaggageItem` once per fact; an absent key omits its tag, so no empty-string series mints and no domain signature grows a tenant slot.

[LOCAL_ADMISSION]:
- Span opens live inside a package's declared telemetry-spine fence; an emitting page declares its trace scopes and the spine owns every `StartActivity` call.

[RAIL_LAW]:
- Package: `System.Diagnostics.DiagnosticSource`
- Owns: vendor-neutral span identity, lifetime, payload, and W3C context for every library-tier emission.
- Accept: scope-keyed source mints, listener-gated brackets yielding the rail's value, typed-error status stamps, stack-allocated tag writes.
- Reject: a hand-rolled stopwatch-and-log span pair, a per-call-site source construction, an OpenTelemetry type referenced from an emitting library.
