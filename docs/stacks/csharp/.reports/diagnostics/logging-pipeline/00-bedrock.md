# logging-pipeline — bedrock

## pipeline-as-policy-table

- One `LoggerConfiguration` per process composes six orthogonal rails — `MinimumLevel`, `Enrich`, `Destructure`, `Filter`, `WriteTo`, `AuditTo` — and `CreateLogger()` freezes them.
- A configuration object builds exactly one logger — the build is a one-way freeze, and any "rebuild with tweaks" requirement is met by switches and rules inside the one pipeline, not by a second build.
- Every behavior of the running pipeline is recoverable from that one declaration: the pipeline is a policy table, not code.
- A second `LoggerConfiguration` below the composition root is the defect: it forks level governance, failure listening, and flush ownership.
- The severity floor is a held policy object: `MinimumLevel.ControlledBy(LoggingLevelSwitch)` makes the floor mutable at runtime through the switch's `MinimumLevel` property.
- `LoggingLevelSwitch.MinimumLevelChanged` raises `(OldLevel, NewLevel)` — the switch is itself an observable signal, so a level change is auditable evidence, not silent reconfiguration.
- Hold switches in one frozen row set keyed by concern; handing a raw `LogEventLevel` literal to configuration forecloses live control.
- `MinimumLevel.Override(sourcePrefix, switchOrLevel)` keys per-category floors off the `SourceContext` property; overrides accept a switch, so per-source verbosity is also live-controllable.
- `Override` matches only events that carry `SourceContext` — an event emitted through a context-less logger rides the global floor; this is the first check when an override "does not work".
- `Override` is explicitly unsupported inside sub-loggers: per-source floors are root-pipeline law only.
- Sub-pipelines are derivation, not duplication: `WriteTo.Logger(cfg => …)` events are constrained by parent filters and enriched by parent enrichers.
- A sub-logger can lower verbosity but never raise it above the parent floor — routing topology is monotone; the root admits once, branches only narrow.
- `Filter.ByExcluding` / `ByIncludingOnly` take event predicates; `Matching.FromSource(…)` and `Matching.WithProperty(name, value)` build them from declared identity rather than lambda literals — filters are rows over the property vocabulary.
- Subsystem routing composes the two: a `Matching.FromSource` predicate on a `Conditional` or sub-logger row sends one namespace's events to a dedicated sink with zero emission-side awareness — routing is always identity-keyed, never call-site-flagged.
- Conditional routing is a sink-level predicate, not a logger fork: `WriteTo.Conditional(predicate, sinks)` keeps one pipeline with branch rows.
- `LoggerSinkConfiguration.Wrap(…)` is the sanctioned constructor for decorator sinks; hand-rolled wrapper sinks that re-dispatch lose level and switch plumbing.
- A settings rail (`ReadFrom`) exists for external settings sources — when used, it is one more input to the same single root declaration, never a second pipeline authority.
- The static facade is bootstrap-only surface: process code receives logger handles through composition; ambient static logging from domain code is the rejected form because it bypasses category identity and makes the flush owner ambiguous.
- `WriteTo.Logger(ILogger, attemptDispose)` attaches an independently built logger as a sink — the only sanctioned shape for adopting a pipeline whose lifetime another owner controls; `attemptDispose` declares which side owns disposal.

## ambient-context-admission

- `LogContext` is an ambient property stack: `PushProperty(name, value)` scopes one property, `Push(enricher)` scopes a full enricher, both disposing back to the prior stack.
- Ambient context is inert until admitted: `Enrich.FromLogContext()` is the pipeline row that lets pushed properties reach events — pushes without it are silent no-ops.
- `LogContext.Suspend()` removes all ambient enrichers for a scope — the sanctioned way to keep a sensitive region's ambient facts out of its own emissions.
- `ForContext<T>()` / `ForContext(Type)` stamp source identity under the constant `Serilog.Core.Constants.SourceContextPropertyName`; `ForContext(name, value, destructureObjects)` pins one property onto a derived logger.
- `ForContext(ILogEventEnricher)` composes a derived logger from enricher rows — per-component loggers are derivation, never reconfiguration.
- `Enrich.AtLevel(switchOrLevel, e => …)` gates expensive enrichment by severity — collect costly facts only on warnings and errors; this is the cost-class lever inside the pipeline itself.
- `Enrich.WithProperty(name, value)` is the static-fact row; per-event computation in a property enricher that returns a constant is waste the `AtLevel`/static split exists to delete.
- A level-gated context overload exists on the logger itself — `ForContext(level, name, value, destructure)` attaches the property only when the level is enabled — the per-logger twin of `Enrich.AtLevel` for derivation-time cost gating.
- Ambient pushes dispose LIFO back to the prior stack — interleaved disposal across scopes corrupts the ambient stack contract; pushes pair with `using` declarations, never stored disposables released out of order.

## sink-topology-and-delivery-classes

- The pipeline has exactly two delivery classes and they are contracts, not configuration moods.
- `WriteTo` is safe projection — sink exceptions are swallowed into the failure rail; `AuditTo` is transactional — exceptions from sinks and intermediate filters propagate to the logging caller.
- A sink participates in audit only when its write path fails synchronously; batched sinks are structurally incompatible with audit guarantees — sinks opt in to auditing, it is not conferred by configuration.
- `AuditTo.Logger(…)` exists: audit-grade sub-pipelines compose the same way as safe ones, with the same parent-constraint law.
- Choose the class per fact stream: operational telemetry is `WriteTo`, compliance-grade evidence is `AuditTo`; one event family never rides both accidentally.
- Batching is first-class pipeline law: any `IBatchedLogEventSink` (`EmitBatchAsync(IReadOnlyCollection<LogEvent>)` + `OnEmptyBatchAsync()`) registers through `WriteTo.Sink(batchedSink, BatchingOptions, restrictedToMinimumLevel, levelSwitch)`.
- `BatchingOptions.EagerlyEmitFirstEvent = true`: the first event flushes immediately regardless of batch size — perceived liveness is a declared row, not an accident.
- `BatchingOptions.BatchSizeLimit = 1000` and `BufferingTimeLimit = 2s`: a full batch emits early; the time limit is the tail-latency ceiling for any single event.
- `BatchingOptions.QueueLimit = 100000` (`null` = unbounded): overflow discards — bound it deliberately, because unbounded queues convert sink outage into process memory growth.
- `BatchingOptions.RetryTimeLimit = 10min` is the retry ceiling; lower it under sustained load to cap buffered memory — past the ceiling, the batch is surrendered to the failure rail.
- Batch implementers let exceptions propagate: the batching infrastructure owns retry, diagnostics, and failure reporting — a try/catch inside `EmitBatchAsync` amputates the retry rail silently.
- `OnEmptyBatchAsync` is the sanctioned periodic-work hook — heartbeat and keepalive work without private timers or threads, and therefore without extra flush/shutdown complexity.
- Fallback is declared topology: `WriteTo.FallbackChain(primary, secondary, …)` reroutes when the target sink throws synchronously or reports through the failure-listener contract.
- A fire-and-forget async sink that neither throws nor reports defeats the fallback chain silently — fallback eligibility is a property of the sink's failure surface, audit it per sink.

## failure-rail

- Pipeline failure is a typed rail, not a log line: `ILoggingFailureListener.OnLoggingFailed(sender, LoggingFailureKind, message, events, exception)` delivers the failing component, the affected events, and a kind discriminant.
- `LoggingFailureKind` is a three-case loss taxonomy: `Temporary` (the reporting sink will retry these events), `Permanent` (these events will not be retried — loss), `Final` (the sink is going offline — channel gone).
- Sinks advertise the rail via `ISetLoggingFailureListener.SetFailureListener` — callable only during initialization, on the initialization thread, before logging starts; late wiring is a contract violation.
- Fold failure callbacks into the suite's evidence stream keyed by sink identity; the kind discriminant is the difference between a blip and data loss.
- `SelfLog.Enable(TextWriter | Action<string>)` is the floor beneath the rail — last-resort internal diagnostics when the pipeline itself is the casualty; `SelfLog.FailureListener` bridges self-log conditions onto the typed rail.
- The self-log target must be unconditionally safe — a bounded, never-throwing writer — because it runs exactly when the normal pipeline cannot; routing self-log into a pipeline sink is circular by construction.
- `CloseAndFlush` / `CloseAndFlushAsync` is the drain verb — final batch emission and sink disposal; an exit path that skips it forfeits up to `BufferingTimeLimit` plus queue depth of tail events.
- Flush ownership is singular: the composition root that called `CreateLogger` owns the drain call; libraries and sub-components never flush, because a mid-flight flush from a non-owner reorders batch boundaries and double-disposes owned sinks.
- The failure rail and the audit class compose into a verification matrix: `WriteTo` + listener = observed best-effort; `AuditTo` + propagation = guaranteed-or-thrown; `WriteTo` without a listener = unobserved best-effort — the third cell is the configuration smell, acceptable only for sinks whose loss is genuinely free.

## payload-shaping-bounds

- Destructuring limits are admission caps against payload bombs: `Destructure.ToMaximumDepth` defaults to 10; `ToMaximumStringLength` and `ToMaximumCollectionCount` default to unbounded.
- Pin string and collection caps at the root for any pipeline that accepts foreign object graphs — an unbounded collection turns one log call into a megabyte event.
- `IDestructuringPolicy` rows registered via `Destructure.With` own how foreign types project; policies are also where bounded-payload law is enforced for types the caller cannot annotate.
- The structured value model is closed — `ScalarValue`, `SequenceValue`, `StructureValue`, `DictionaryValue` — so a destructuring policy is a total map from a type family into a four-case algebra.
- Event identity in the structured model is the message template, never the rendered string; `RenderMessage` / `MessageTemplate.Render` are projection-only.
- Template text is constant by law: templates cache by their text, so interpolating dynamic data into the template string explodes the cache and destroys event identity in one move — dynamic data is always a property, never template text.
- The template grammar's capture sigils are payload policy at the hole: the destructure sigil captures structure, the stringify sigil forces scalar text — per-hole shaping that composes with the global destructuring caps rather than replacing them.
- `Write(level, …)` is the one level-parameterized emission verb; the named severity verbs are sugar over it — code that dispatches severity dynamically uses the verb with the level value, never a switch over seven names.
- `BindMessageTemplate` / `BindProperty` validate template-property correspondence ahead of emission — the pre-flight check for templates assembled from vocabulary rather than literals.
- The event value model is fixed: timestamp (offset-bearing), level, exception, message template, property map, plus the trace correlation fields — every formatter and sink consumes this one shape, which is why payload policy lives in destructuring, not in sinks.
- The parts-assembly constructor surface for events is explicitly version-fragile by its own contract — programmatic event construction targets the stable constructor, and anything assembling events from internal parts is a maintenance liability the pipeline does not owe compatibility to.

## abstraction-seam

- The abstraction seam is the `Microsoft.Extensions.Logging` contract set; emission rides the seam and concrete pipelines attach behind it as provider rows — N projection backends fan out from one emission discipline.
- `ILogger<TCategoryName>` carries category identity; `EventId` carries stable event identity (id plus name) — string messages never define identity.
- `ILogger.Log<TState>` with `IsEnabled` guards and `BeginScope<TState>` typed scopes is the complete emission contract; severity extensions are sugar over it.
- Providers join ambient scope through `IExternalScopeProvider` / `ISupportExternalScope` — a provider that ignores the external scope provider drops every scope property silently; first check when scope data vanishes in one exporter but not another.
- `IsEnabled` answers across the provider set — true when any provider would accept the level — so the guard's cost gating is exact for single-provider pipelines and conservative for fan-out: a level only one verbose provider accepts pays state construction for all.
- Category filtering rules compose most-specific-wins, and provider-targeted rules (by alias) beat global rules — one declared rule lattice replaces conditional emission code everywhere.
- `IBufferedLogger` + `BufferedLogRecord` is the provider-side batch contract: records carry timestamp, level, event id, exception, activity ids, thread id, formatted message, template, and attributes in one flush.
- Provider-side buffering and sink-side batching must not stack naively — double-buffering doubles tail loss on crash.
- `ProviderAliasAttribute` names a provider for configuration binding — per-provider level rules address the alias, not the type name.
- Null objects (`NullLogger`, `NullLogger<T>`, `NullLoggerFactory`, `NullLoggerProvider`, `NullExternalScopeProvider`) are the absent-pipeline rows — composition that may run logger-less takes the null object, never a nullable logger.
- `Logger<T>` is the category adapter over `ILoggerFactory`; `LogEntry<TState>` is the provider payload shape — the value samplers and buffers inspect.
- Scopes carry typed projection context and never replace typed receipts — a scope is metadata about emissions inside it, not an outcome record; the receipt rail and the log rail stay orthogonal, joined by correlation fields rather than merged.
- Buffered logging is likewise projection-surface material — a buffer replay is re-projection of already-emitted evidence, never a source of truth a consumer reads back programmatically.
- `CreateLogger<T>()` and `CreateLogger(Type)` are the two category mints — generic for compile-known categories, type-shaped for plugin and dispatch scenarios; string-named categories are reserved for boundary material that has no owning type.
- The severity extension family (`LogTrace` through `LogCritical`) is sugar that allocates on every call when arguments box — its existence is why the generated path is law for production emission and the extensions survive only in throwaway diagnostics.

## generated-emission

- Two source generators answer `[LoggerMessage]` and they are different machines: the base logging generator ships with the abstractions package; the telemetry generator (`Microsoft.Gen.Logging`, an analyzer asset of the telemetry-abstractions package) owns the extended surface.
- The extended surface — `[LogProperties]`, `[TagProvider]`, `[TagName]`, classified parameters — is inert without the telemetry-abstractions package; admitting it upgrades the generation machine for the whole compilation.
- `[LoggerMessage]` payload rows: `EventId`, `EventName`, `Level`, `Message`, `SkipEnabledCheck`.
- Event-identity space is a partitioned registry: id ranges allocate per subsystem as declared bands, so an id collision is a review-time arithmetic fact and an exported id is decodable to its owner without a lookup service — ids are vocabulary, and vocabulary lives in one table.
- `EventName` travels with the id and is the human-stable half: renumbering migrations preserve names, dashboards key on names, and the pair (id stable for wire compactness, name stable for queries) is why neither alone suffices.
- The generated body performs the `IsEnabled` guard unless `SkipEnabledCheck = true` — skip only when the caller already guards; the generated guard is what makes a disabled level zero-allocation.
- An exception-typed parameter on a generated method binds to the entry's exception channel, never to a template hole — exception identity stays structured, and `ToString`-ing an exception into the message is unrepresentable through the generated path.
- The pre-generator fallback is `LoggerMessage.Define<T1..T6>` / `DefineScope` (zero through six template values; `LogDefineOptions` controls the enabled-check) — delegate caching for the rare site a partial method cannot host.
- Six is the hard template arity; beyond it the payload belongs in a `[LogProperties]` object, not positional arguments.
- `[LogProperties]` expands an object parameter into tags named `Parameter.Property` by dot-path; `OmitReferenceName = true` drops the prefix; `SkipNullProperties = true` elides null members; transitive nested expansion is an opt-in knob.
- `[TagName]` renames one emitted tag at the declaration — tag vocabulary is source-controlled, and renaming a property breaks the wire tag loudly at rebuild; that loudness is the feature.
- `[TagProvider(providerType, providerMethod)]` is the projection row for foreign types that cannot carry annotations: a static method receives `ITagCollector` plus the value and emits tags — the one sanctioned escape from declarative expansion, scoped to the parameter that needs it.
- The tag-provider and metric attributes are conditional-compiled out of runtime metadata — they exist for the generator, not for reflection; any design that plans to discover them at runtime discovers nothing.
- `LoggerMessageState` is the generated emission carrier: parallel `TagArray`, `RedactedTagArray`, and `ClassifiedTag { Name, Value, Classifications }` arrays.
- A classified parameter lands in the classified array and the enforcement seam decides what reaches providers — emission code never sees, formats, or branches on sensitivity; the attribute is the policy, the generator is the loom.
- Generated log methods live as partial methods on the type that owns the concern — the generator binds the logger from the containing type's logger field or an explicit parameter, so per-concern log surfaces co-locate with the code they evidence instead of pooling in a central "log messages" class.
- Strongly typed metric factories from `[Counter]` / `[Gauge]` / `[Histogram]` rows are generated by the sibling `Microsoft.Gen.Metrics` machine — shared generation seam, signal-pipeline law.

## head-of-pipeline-volume-policy

- Log sampling and log buffering are head-of-pipeline policies on the abstraction seam — they decide volume before any provider sees the record; that placement is what makes them governance rather than sink configuration.
- `AddTraceBasedSampler()` slaves log volume to the trace sampling verdict: a record is kept iff the ambient activity is recorded, so logs and spans rise and fall as one correlated population — the default for any suite exporting both signals.
- `AddRandomProbabilisticSampler(probability, maxLevel?)` or rule rows (`Probability`, `CategoryName`, `LogLevel` ceiling, `EventId`, `EventName`) cover trace-less processes.
- Sampling rules select by maximum level — they thin the chatty floor and pass the severe ceiling untouched; a rule whose ceiling includes errors is almost always a mistake.
- `AddSampler<T>` / `LoggingSampler.ShouldSample(in LogEntry<TState>)` is the custom row — one override, full pipeline position.
- Global buffering inverts severity economics: matched records are held, not emitted, and replay only on `GlobalLogBuffer.Flush()` — buffer the verbose tiers always, flush only when an incident makes them valuable.
- `LogBufferingFilterRule { CategoryName, LogLevel ceiling, EventId, EventName, Attributes }`: match buffers, no-match emits normally — the rule set partitions the stream into live and deferred populations.
- `GlobalLogBufferingOptions.MaxBufferSizeInBytes = 524_288_000`: oldest records drop on overflow — the buffer is a ring, not a ledger.
- `GlobalLogBufferingOptions.MaxLogRecordSizeInBytes = 51_200`: an oversize record bypasses the buffer and emits normally — surprising and load-bearing; huge records leak past incident gating.
- `GlobalLogBufferingOptions.AutoFlushDuration = 30s`: after a manual flush, buffering stays suspended so the incident window streams live — the suspension is the incident-mode switch.
- The per-request buffer base type exists at the abstraction layer but its implementation ships only with web-host middleware; in a process suite without that host, the global buffer is the only wirable buffering row.
- The buffer's runtime contract is two verbs: `TryEnqueue` (a buffered logger admits a record; refusal means emit normally) and `Flush` (replay) — buffering is best-effort by construction, and code must never assume a record it logged is retrievable from the buffer.
- Compliance-grade event families are exempt from volume policy by rule construction: sampler and buffer rule rows must exclude the categories that ride the audit delivery class — a sampled audit trail is a contradiction the rule table must make impossible, not a runtime check.
- Order the volume ladder deliberately: sampling deletes, buffering defers, batching coalesces — delete before defer before coalesce; reversing sampling and buffering wastes buffer bytes on records sampling would have dropped.

## divergent

- One-pipeline law (maximal unification): a suite declares exactly one emission grammar — generated `[LoggerMessage]` partials over `ILogger<T>` — and one projection pipeline behind the seam, with every variable behavior held as a policy value: level floors as a `LoggingLevelSwitch` row set, routing as `Matching` predicates and `Conditional` rows, volume as sampler and buffer rule rows, delivery class as the `WriteTo`/`AuditTo` discriminant, batching as `BatchingOptions` rows, failure handling as one listener folding `LoggingFailureKind` into the evidence stream.
- Under that law, adding a destination, an incident-buffer category, or a verbosity regime is one row in one table; no emission site, no severity literal, and no sink-local policy changes.
- Rejected forms the one-pipeline law forecloses, each with its specific damage: per-module logger configuration (forks flush and failure ownership), interpolated log calls (defeat caching, identity, and classification), severity constants at call sites (foreclose live control), boolean "verbose" parameters (the switch row is the knob), try/catch around sink writes (amputates the typed failure rail).
- Sourcegen as the telemetry type system: the generated layer makes log emission a compile-checked contract — event identity, tag vocabulary, arity, sensitivity, and cost are all declaration facts the build verifies.
- Deep consequence: a telemetry schema review is a code review of attribute rows, and schema drift between processes is impossible when the annotated state objects are shared contract types — push every "what do we log here" decision into the state object's type; the log method degenerates to one declaration per event family.
- Generation/enforcement interlock: classified tags ride `LoggerMessageState` in parallel arrays precisely so the redaction seam can swap which array providers consume without touching generated call sites — emission and enforcement meet at one seam and nowhere else.
- Pipelines that re-shape payloads after that seam (provider-side scrubbing, sink-side regex masking) re-derive policy the declaration already states, and they see only rendered text where the seam saw typed classified values — the named defect.
- Failure-evidence fold: the three failure kinds plus batching's retry ceiling define a complete loss taxonomy — `Temporary` under ceiling (no loss yet), `Temporary` past `RetryTimeLimit` (bounded loss), `Permanent` (immediate loss), `Final` (channel gone), queue overflow (oldest-first loss), oversize-record bypass (gating leak, not loss).
- Fold all six into one per-sink evidence row; any operational view of "are we losing logs" is then a projection over that fold rather than a grep of self-log text.
- Liveness/throughput duality worth declaring once: `EagerlyEmitFirstEvent` + `BufferingTimeLimit` bound worst-case visibility latency while `BatchSizeLimit` + `QueueLimit` bound throughput cost — the four knobs form one declared latency/throughput budget per sink, and tuning one without restating the other three is how pipelines drift into either chattiness or invisibility.
- Two-backend provider law: the seam supports N providers, but each must have a disjoint delivery mandate — one wire exporter, one operator-local projection; two providers with overlapping mandates re-create the dual-pipeline defect inside a single seam, detectable as the same record arriving at one destination twice with different shapes.
- Level-governance unification across the seam: the seam's per-category configuration rules and the projection pipeline's switch rows both filter by category — declare the floor once at the seam (cheapest rejection point) and reserve pipeline switches for projection-side concerns (sink verbosity, sub-pipeline routing); duplicating one floor in both layers makes "why is this event missing" a two-system investigation.
- Identity mapping at the seam boundary is declared once: category maps to source identity, event id and name map to structured fields, severity maps by the bridge's fixed correspondence — the mapping is bridge-owned and never re-derived per sink, so a query for one event family is the same query against every projection backend.
- The pipeline's full policy surface — switches, rules, options rows, batching squares — binds from typed options and configuration sections end-to-end, which makes the entire logging posture of a process diffable between environments as configuration, with zero code delta.
