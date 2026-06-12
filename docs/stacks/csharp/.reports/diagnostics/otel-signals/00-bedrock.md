# otel-signals — bedrock

## sdk-composition-at-process-roots

- One `AddOpenTelemetry()` call at the composition root opens the telemetry builder; `WithTracing(…)`, `WithMetrics(…)`, `WithLogging(…)` admit the three signal pipelines and `ConfigureResource(…)` stamps shared identity.
- Provider lifetime is hosted-service owned: construction, flush, and shutdown ride host lifetime with zero manual provider management; manual lifetime management is the rejected form.
- `Sdk.CreateTracerProviderBuilder()` / `CreateMeterProviderBuilder()` + `Build()` exist for processes with no host; mixing hosted and manual styles in one process splits resource identity and drain ownership.
- The factory set is asymmetric on purpose: no public standalone log-provider factory exists — the log signal composes only through the logging-builder integration, so the hosted path is the only complete three-signal root and a hostless process exports logs by carrying the logging builder anyway.
- Endpoint scheme carries the transport-security decision: a secure scheme negotiates TLS on the channel, a plain scheme does not — there is no separate TLS flag to forget, and a plaintext endpoint in production configuration is visible in one glance at the URL.
- SDK initialization itself installs process-wide invariants: the default id format is forced to W3C, and self-diagnostics initialize — these are facts of loading the SDK, not opt-in configuration.
- `Sdk.SuppressInstrumentation` (scope-based) suppresses signal collection inside a region — the mechanism that keeps an exporter's own outbound calls from generating telemetry about exporting telemetry; any custom exporter or publisher that calls instrumented clients wraps its I/O in the suppression scope.
- Log-signal capture policy lives on `OpenTelemetryLoggerOptions`: `IncludeFormattedMessage`, `IncludeScopes`, `ParseStateValues` all default `false` — exported records carry structured state only unless the root opts in.
- `OpenTelemetryLoggerOptions.AddProcessor` accepts an instance or an `IServiceProvider` factory — log processors are composition values with container access, not inline lambdas.
- `ForceFlush(timeout)` and `Shutdown(timeout)` are the bounded drain verbs on every provider; both return success/failure rather than throwing.
- Drain choreography at process exit: flush providers, then dispose — disposal without flush forfeits one full batch window of tail telemetry.
- Builder-level `ConfigureResource` is the only resource call that reaches all three providers in one declaration — per-signal resource configuration exists but reopens the identity-split hazard the builder call closes.
- `AddInstrumentation<T>` is the lifetime hook for instrumentation objects that must construct with the provider and dispose with it — stateful listeners ride provider lifetime through it instead of owning their own.
- Processor order is registration order: shaping and filtering processors register before export processors, or the exporter serializes spans the shaping never touched — pipeline order is a declaration-order fact, reviewable at the root.
- `AddLegacySource(name)` admits activities created outside any source (legacy diagnostics emitters) — a bridge row for foreign code, never for new emission.
- Two span APIs exist and a suite picks one: the platform-native activity surface is what the entire instrumentation ecosystem shares; the spec-shaped tracer/span shim wraps it for portability-styled code — mixing both in one suite produces two idioms for one concern with zero capability gain.
- A direct log-emission bridge exists below the seam (`EmitLog` over log-record data and attribute lists) for components that must write the log pipeline without a logging-contract dependency — boundary material only; in-suite emission stays on the contract seam.

## resource-identity

- Resource identity is required before provider construction and is the correlation anchor for every exported signal.
- `ResourceBuilder.AddService(serviceName, serviceNamespace, serviceVersion, autoGenerateServiceInstanceId = true, serviceInstanceId)`: the auto-generated instance id is a fresh GUID per process start.
- Restart lineage is invisible under auto-generation — pin `serviceInstanceId` from the suite's boot-minted identity, and "which process said this" becomes answerable across restarts; leaving it auto-minted is a deliberate anonymity choice, not a default to inherit.
- `AddTelemetrySdk()` stamps SDK identity; `AddAttributes(…)` adds suite attribute rows; `IResourceDetector` rows fold discovered identity.
- `AddEnvironmentVariableDetector()` binds `OTEL_RESOURCE_ATTRIBUTES` / `OTEL_SERVICE_NAME` — deployment can extend identity with zero code.
- `ConfigureResource` augments and composes across registrations; `SetResourceBuilder` replaces and silently discards earlier identity — augmentation is the default verb, replacement is a deliberate reset.

## metric-views-and-cardinality-governance

- Views are declaration-time stream surgery: `AddView(instrumentName, name)` renames, `AddView(instrumentName, MetricStreamConfiguration)` reshapes, `AddView(Func<Instrument, MetricStreamConfiguration?>)` matches programmatically.
- `MetricStreamConfiguration.Drop` deletes a stream — the cheap kill switch for an instrument a dependency emits but the suite does not want to pay aggregation for.
- `MetricStreamConfiguration.TagKeys` projects the tag set and is copied on assignment, not aliased — views own their key lists.
- Cardinality is budgeted, not unbounded: the provider holds a metric-stream ceiling of 1000 and a per-stream cardinality default of 2000; `SetDefaultCardinalityLimit` overrides globally, `MetricStreamConfiguration.CardinalityLimit` per view.
- On cardinality breach the SDK does not drop measurements — it folds them into one reserved overflow point tagged `otel.metric.overflow = true`; two points per stream are reserved for the mechanism.
- Point storage pre-allocates against the limit, so the cardinality limit is also a fixed memory commitment per stream — raising the global default multiplies memory across every stream; raise per view where one instrument genuinely needs the width.
- Dashboards that ignore the overflow tag silently undercount; the tag's presence is the canonical tag-explosion alarm — alert on it directly.
- Histogram shape is policy: `ExplicitBucketHistogramConfiguration` carries hand-declared boundaries; `Base2ExponentialBucketHistogramConfiguration { MaxSize ≥ 2, MaxScale ∈ [-11, 20] }` auto-ranges.
- Exponential histograms are the default choice for latency families because boundary guesses are the thing they delete; explicit buckets remain for distributions with known fixed thresholds.
- Instrument-side `InstrumentAdvice<T>.HistogramBucketBoundaries` lets the declaring meter suggest boundaries without the root knowing the instrument — advice composes with views, views win.

## exemplars-and-reader-cadence

- `SetExemplarFilter(ExemplarFilterType.{AlwaysOff, AlwaysOn, TraceBased})` is the exemplar admission row.
- `TraceBased` records exemplars only on measurements taken inside sampled activities — the exemplar population automatically matches the trace population, and a metrics spike clicks through to representative traces at zero extra sampling budget.
- Reader cadence is environment-bindable policy: `PeriodicExportingMetricReaderOptions` binds `OTEL_METRIC_EXPORT_INTERVAL` / `OTEL_METRIC_EXPORT_TIMEOUT` at construction; the interval defaults to one minute.
- `MetricReaderOptions.TemporalityPreference` defaults `Cumulative`; `Delta` is the row for backends wanting pre-computed change rates — temporality is a reader fact, never an instrument fact.
- `MetricReader.Collect` / `ForceFlush` are the manual snapshot and drain verbs — test harnesses and pre-shutdown drains use them; domain code never does.
- Cumulative-vs-delta is a backend-compatibility decision with a memory consequence: cumulative readers hold every seen tag combination for process lifetime; delta readers can reclaim idle points — long-lived processes with churning tag values lean delta where the backend permits.

## native-emission-conventions

- The emission primitives are the platform pair and they are free until admitted: `ActivitySource.StartActivity` returns `null` when no listener admits the source, so an un-exported span site costs one null check.
- The law: emit natively everywhere, admit selectively at the root via `AddSource(name)` / `AddMeter(name)`; `HasListeners()` guards expensive tag computation ahead of the call.
- `StartActivity` overloads accept explicit parent context, creation-time tags, links, and a start time — creation-time tags and links participate in the sampling decision, post-start mutation does not; evidence that should influence admission goes in at creation.
- `CreateActivity` returns an unstarted activity for precise timing control — the rare row for spans whose true start predates the code path that constructs them; `StartActivity` remains the default verb.
- Source and meter identity is constructed evidence: `ActivitySource(name, version, tags)` and `Meter(MeterOptions { Name, Version, Tags, Scope, TelemetrySchemaUrl })` — version and static tags ride every span and measurement from that owner.
- Meters are factory-minted: `IMeterFactory` is the container mint, caches on identical identity, and disposes meters with the container.
- A `static` meter outliving its provider is the named defect: its instruments silently stop aggregating after provider shutdown while call sites keep paying measurement cost.
- The instrument family is closed and polarity-split: synchronous `Counter` / `UpDownCounter` / `Histogram` / `Gauge` record at the call site; `ObservableCounter` / `ObservableUpDownCounter` / `ObservableGauge` pull via `Func<T>`, `Func<Measurement<T>>`, or `Func<IEnumerable<Measurement<T>>>` at collection time.
- Push for event-shaped facts, pull for level-shaped facts: a polled level recorded through a synchronous gauge produces cadence-aliased data.
- Observable callbacks execute at collection cadence on the collecting thread — they must be fast, non-blocking, and thread-safe against the state they read; a callback taking a lock the hot path holds turns metric collection into a stall injector.
- Multi-measurement observable shapes (`Func<IEnumerable<Measurement<T>>>`) exist so one callback reports a whole tag family per collection — one registry row covering a dimension sweep, instead of N single-value observables over shared state.
- Leak diagnostics are an instrument-row pattern, not a tool: an observable up-down counter folding acquire/release counts from a resource boundary is the leak detector — baseline-returning is healthy, a monotone climb is the leak signature, and the alarm is a metric query instead of a heap dump.
- The acquire/release fold belongs to the boundary that owns the resource; the diagnostics contribution is the registered instrument row and its alarm shape — measurement, not custody.
- Instruments-are-rows: every instrument a process emits is declared once in one registry owner at composition — names, units, descriptions, tag vocabulary, advice — and call sites receive the typed instrument.
- Never create instruments inline: instrument identity de-duplicates by name, and a drifted unit or description forks the stream.
- Naming discipline is registry law: instrument names are dot-separated stable identifiers and units are declared on the instrument, not encoded in the name — `duration` with a time unit, never `duration_ms`; the registry review enforces it once instead of every dashboard discovering it.
- Measurement tags are the cardinality currency: the registry row for an instrument declares its allowed tag keys, and any tag key outside the row is either projected away by a view or burns cardinality budget — tag vocabulary is closed per instrument, open per registry edit.
- The attribute-generated factory rows (`[Counter]` / `[Gauge]` / `[Histogram]` methods) compile the registry: tag names become compile-time facts and the registry is reviewable as attribute rows.
- Span mutation is a closed verb set: `SetStatus(ActivityStatusCode, description)`, `AddEvent`, `AddLink`, tag setters, `TraceStateString` — status is the typed verdict (`Ok`/`Error`/`Unset`), never a string tag.
- `ActivityKind` (internal, client, server, producer, consumer) is semantic routing for backends — boundary spans declare their kind; defaulting everything to internal degrades every service-map view downstream.
- Spans end deterministically or not at all: an activity is disposable and a span never stopped is a span never reported — using-scoped span lifetime is the default spelling, and long-lived manually stopped spans carry an explicit owner.
- Status discipline: the description accompanies the error verdict only — a status description on an ok span is dead payload, and error-shaped information in tags instead of status is invisible to every backend's error filter.
- Span events versus log records is a retention decision, not a style one: events ride the span payload and die with the span's sampling verdict; log records ride the log pipeline and survive span sampling — facts that must outlive an unsampled trace are log records by law, and duplicating one fact on both channels doubles its export cost for zero query value.
- Exception evidence has three candidate channels — span-event recording at the instrumented seam, the listener-level exception recorder, and exception-bearing log records — and a suite declares exactly one owner per exception class; three channels firing for one throw is triple-billing the same incident and triple-counting it in every error-rate view.
- Both emission owners carry an optional telemetry schema URL in their identity — pinning it stamps every span and measurement with the semantic-convention vintage the emitter wrote against, which is what lets a collector translate conventions per source instead of guessing.
- The instrument registry plus pinned schema vintage form a machine-checkable dashboard contract: a dashboard's referenced instruments, tag keys, and units validate against the registry at review time — dashboard drift becomes a diff, not a discovery.
- Component lifetime is provider lifetime, transitively: processors, readers, and exporters registered into a provider are owned by it and drain with it — external code holding a processor or exporter reference for direct use is reaching inside another owner's lifecycle.

## instrumented-admission

- Instrumentation packages are admitted only where they own a foreign library's emission; where the platform emits natively, admission is `AddMeter` / `AddSource` over the platform names.
- Runtime metrics are the canonical case: current runtimes emit the `System.Runtime` meter natively and the runtime-instrumentation package's registration reduces to subscribing it; the package's own meter and `process.runtime.dotnet.*` instrument names exist only as a legacy-runtime shim.
- The trap is the identity break across that shim boundary: legacy instrument names differ from the native convention, so dashboards bind to exactly one convention per deployment — mixed fleets need a views row, not dashboard forks.
- The views table is also the repair tool for the break: rename rows map shim names onto the native convention (or `Drop` rows silence the shim side) so the fleet converges on one identity during migration instead of carrying parallel dashboards.
- Outbound-HTTP tracing subscribes the `System.Net.Http` activity source; HTTP metrics admission covers the `System.Net.Http` and `System.Net.NameResolution` meters — all native emission, admitted by name.
- The HTTP trace options object is the interception proof shape: `FilterHttpRequestMessage` gates collection per request; `EnrichWithHttpRequestMessage` / `EnrichWithHttpResponseMessage` / `EnrichWithException` are enrichment policy values; `RecordException` (default `false`) opts exceptions into span events.
- A throwing filter drops the request silently — filters must be total and side-effect free; the drop-on-throw behavior is the contract, not a style preference.
- Trace options resolve through named options monitors — instrumentation policy is options-system material, configurable per named client, never code at call sites; the named-registration overload exists precisely so two client families carry two filter/enrich policies from one package admission.
- Readers admit as instance, generic type, or container factory (`AddReader` three ways) — a reader is a composition value with container access like any processor, which is how test harnesses substitute manual readers for the periodic one without touching pipeline declarations.
- Interception, not instrumented call sites: cross-cutting span and measurement shaping lives in `BaseProcessor<T>.OnStart` / `OnEnd` rows and options-bound delegates; domain code emits, policy shapes.
- Processor topology is a small closed family: `SimpleExportProcessor` (inline export per signal — test and audit shapes), `BatchExportProcessor` (queued, the production default), `CompositeProcessor` (fan-out chain) — and `OnEnd` runs synchronously at span end, so a slow processor taxes the instrumented hot path directly; processors compute nothing they can defer to the exporter.
- `ExportResult` is the exporter's binary verdict; exporters batch over `Batch<T>` and own their own retry interpretation — a custom exporter that throws instead of returning failure breaks the processor's accounting.
- The raw admission seam under the SDK is `ActivityListener` (`ShouldListenTo`, `Sample` / `SampleUsingParentId`, `ActivityStarted` / `ActivityStopped`, `ExceptionRecorder`) — never hand-wired in a suite running the SDK, but it is what samplers and processors stand on, and the reason a second listener-based agent in-process double-samples.

## sampling-governance

- The sampler taxonomy is four rows with one composition root: `AlwaysOnSampler`, `AlwaysOffSampler`, `TraceIdRatioBasedSampler(probability)`, `ParentBasedSampler(rootSampler, …)`.
- A fifth row is open by subclassing: `Sampler.ShouldSample(in SamplingParameters)` receives parent context, trace id, name, kind, tags, and links — custom admission policy (per-tenant budgets, name-keyed floors) is one override returning a `SamplingResult`, registered at the root like any other row.
- `ParentBasedSampler` exposes four delegated slots with declared defaults — `remoteParentSampled = AlwaysOn`, `remoteParentNotSampled = AlwaysOff`, `localParentSampled = AlwaysOn`, `localParentNotSampled = AlwaysOff` — parent-respect is itself a policy matrix, not a boolean.
- `TraceIdRatioBasedSampler` is deterministic over trace-id bytes (threshold = probability × 2⁶³ against the leading eight bytes): every service running the same ratio reaches the same verdict for the same trace.
- Determinism is why the ratio sampler wraps inside `ParentBased` as the root-only decision rather than re-rolling per hop — head sampling stays consistent across process hops without coordination.
- Ratio changes are fleet-atomic in intent: during a staggered rollout two ratios coexist and cross-service consistency holds only within each — roll sampling-ratio changes fast and fleet-wide, or accept a window of partial traces and exclude it from trace-completeness metrics.
- The sampling verdict is the spine other volume policies derive from: the recorded flag drives the trace-based log sampler and the trace-based exemplar filter — one root sampler row simultaneously governs span volume, log volume, and exemplar volume.
- Declare sampling once, derive thrice; independent per-signal sampling probabilities are the rejected form because they destroy cross-signal joinability.
- Sampling is head-decision by construction in-process; tail decisions belong to the collector tier — the in-process lever for "keep errors, thin successes" is processor-stage filtering and log-side rules, never a content-inspecting sampler.
- The sampler's `Description` strings compose mechanically (the parent-based wrapper embeds its root's description) — exported provider diagnostics therefore state the full sampling policy in one string, which makes "what sampling is this process actually running" answerable from telemetry metadata.

## otlp-export-governance

- Export topology is itself a governed decision: the default shape is process → local collector (agent or sidecar) over OTLP, with backend routing, tail decisions, and credential custody at the collector tier — direct-to-backend export couples every process to backend authentication and retry semantics, and is admitted only where no collector tier exists.
- `UseOtlpExporter()` on the root builder is the one-call export law: it wires all three signals to one OTLP destination and honors per-signal environment overrides.
- It may be called exactly once and throws `NotSupportedException` when mixed with per-signal `AddOtlpExporter` registrations — the restriction is the feature: one export owner per process, detected at build time rather than discovered as duplicate traffic.
- Transport defaults discriminate by protocol: gRPC targets port 4317 with no path; http/protobuf targets 4318 and appends `/v1/traces`, `/v1/metrics`, `/v1/logs` under the one-call law.
- Per-signal registration over http/protobuf requires the full signal path in the endpoint — the classic silent-404 misconfiguration.
- `OtlpExporterOptions` rows: `Endpoint`, `Protocol` (gRPC default), `Headers`, `TimeoutMilliseconds = 10000`, `Compression` (none default — gzip is the first row to flip on constrained links), `ExportProcessorType = Batch`, `BatchExportProcessorOptions`, `HttpClientFactory`, `UserAgentProductIdentifier`.
- `Headers` takes delimited key-value pairs — one string row carries the collector's auth and tenancy headers, which is exactly why it binds from environment and never appears in source.
- `HttpClientFactory` is the client-policy seam — proxy, client credentials, and handler composition enter there; mTLS material additionally binds through dedicated certificate/key path configuration keys.
- Environment binding covers `OTEL_EXPORTER_OTLP_{ENDPOINT, PROTOCOL, HEADERS, TIMEOUT, COMPRESSION}` plus `_TRACES_` / `_METRICS_` / `_LOGS_` variants — deployment retargets export with zero code.
- The `OTEL_*` keys bind through the configuration abstraction, not raw process environment alone — hosted composition can supply the same keys from any configuration source, which unifies telemetry deployment knobs with the rest of the suite's configuration admission.
- One reader pairs with one metric exporter; additional readers multiply collection passes over every instrument — multi-destination metric export is a collector-tier fan-out concern, not a second in-process reader.
- Export pressure is governed by the batch-processor square: `MaxQueueSize = 2048`, `ScheduledDelayMilliseconds = 5000`, `ExporterTimeoutMilliseconds = 30000`, `MaxExportBatchSize = 512`, with batch clamped to queue.
- The queue is drop-on-full: telemetry loss under burst is silent at the call site and visible only through SDK self-diagnostics — treat `(peak rate × delay) > queue` as a config-time arithmetic check, not a production discovery.
- Transient-failure retry is opt-in policy: `OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY = in_memory` buffers and retries failed exports; `disk` persists with periodic resend and a directory binding — without one, a collector outage longer than the queue window is unrecorded loss.
- Logs and traces ride batch processors by default; metrics ride the periodic reader — three cadences, one governance table: export interval, batch delay, drain timeout, and host-stop drain order declare together because shutdown drain is bounded by the slowest of the three.
- The export-durability budget is the sum of three declared rows — batch square (in-memory window), retry row (outage window), drain timeouts (shutdown window) — and each window has a distinct loss mode: burst overflow, outage exhaustion, drain truncation; an incident review that cannot name which window lost the data has an ungoverned export path.
- Drain order at shutdown is signal-ranked: traces and metrics drain before the log provider, because the log pipeline is what evidences the drain itself — flushing logs first silences the very records that would report a failed trace flush.
- Headers carry collector authentication as deployment material (environment-bound), never as code constants — the headers row is where per-environment tenancy lands without rebuilds.

## divergent

- SDK-composition maximal form: the entire signal stack of a process is one declaration — resource fold (service identity, pinned instance id, suite attributes), three `With*` admissions, view table, sampler matrix, exemplar row, export row — and every named thing in it is a value: sampler rows, view rows, batch squares, cadence rows.
- All growth axes are row-shaped: a new subsystem is one `AddSource` / `AddMeter` admission; a new latency family is one view row; a new deployment is environment binding; a new histogram policy is one configuration object.
- Forms the composition law forecloses: per-library tracer or meter provider construction, exporter packages referenced below the root, sampling decided per call site, and "configure telemetry" helper layers that rename builder calls one-to-one.
- Native-emission admission doctrine at full depth: instrumentation-package admission is a three-way verdict per dependency — platform-native source/meter exists (admit by name, zero packages); the dependency emits its own source/meter (admit by name, zero packages); only a contrib package can see the traffic (admit the package as an options-bound row).
- The verdict re-evaluates when the platform moves emission native, and the shim's instrument-identity break is the standing reason to prefer the first two rows — every contrib package admitted is a future renaming migration.
- Spanning proof matrix: governance completeness is checkable as a grid — axes signal {traces, metrics, logs} × concern {identity, admission, volume, shaping, cardinality, export, drain}; each cell names its owning row.
- Matrix cell examples that catch real gaps: metrics×volume = views plus cardinality limit; traces×volume = sampler matrix; logs×volume = derived from traces; logs×shaping = logger-options capture flags — an empty cell is an ungoverned signal path; a cell with two owners is a split-brain defect.
- The matrix folds to per-cell typed audits at boot: assert one sampler, one export owner, one resource fold, instrument registry closed — governance becomes a startup proof rather than a review checklist.
- Measurement-cost asymmetry worth legislating: spans cost at start/stop once a listener admits the source (only the null return is free), while measurements into a dropped view cost nearly nothing after the view resolves.
- Therefore thin span families by not admitting sources or by sampling at the root, and thin metric families by `Drop` views — reversing the two levers (dropping spans in a processor, gating meters at call sites) pays the maximum cost for the minimum control.
- Exporter self-instrumentation closure: the suppression scope plus the export pipeline's own HTTP emission form a feedback loop hazard unique to telemetry — the audit row is "no span whose source is the exporter's own client", checkable from exported data.
- Identity-mint unification: source identity, meter identity, resource identity, and schema vintage are four facets of one minted registry — declared in one owner at the root, version-stamped together, and the registry is the single diff surface when a release changes what a process emits; scattering the four across files is how emission identity drifts between processes of one suite.
- The cost model generalizes to a placement rule: pay-per-emission costs (span start/stop, tag computation) are minimized by admission control at the root; pay-per-aggregation costs (cardinality, streams) are minimized by view control at the root; pay-per-wire costs (batch, compression, retry) are minimized by export control at the root — every cost lever lives at the root by construction, which is the deepest argument for the composition law: there is no call-site optimization left to make.
- Receipts project to signals, they do not become them: typed algorithm receipts keep their evidence fields and project to span attributes through their own span-formattable implementations — one formatting surface feeds tags without intermediate strings — while operational facts fold into the owner-keyed evidence stream that views derive from.
- A generic receipt interface as the telemetry carrier is the named defect at this seam too: erasing receipt fields to fit a common projection shape deletes exactly the route, status, and measurement evidence that made the receipt worth exporting; the projection adapts per receipt type, the receipt never adapts to the projection.
