export const meta = {
    name: 'observability',
    description:
        'Realize one phase of the observability + analytics campaign against .claude/scratch/observability/CAMPAIGN.md. args = {phase: 1..5}, bare number accepted, empty = no-op. Per phase: territory-congruent recon lanes write dossiers to run scratch, then each territory runs implement then critique then red-team over its own disjoint page set, then a residual drain fixpoint and a verify lane close the phase. Phase 1 runs a kernel barrier before its consumer fan, phase 3 mints the collector catalog and spec axes before its one deep writer, phase 5 adds ordered serial closers.',
    whenToUse: 'Landing a phase of the observability + analytics campaign in the Rasm planning corpus.',
    phases: [
        { title: 'Recon', detail: 'territory-congruent mapping lanes; dossier to disk, thin receipt on the wire', model: 'sonnet' },
        { title: 'Kernel', detail: 'serial barrier writer for owners the territory fan composes' },
        { title: 'Implement', detail: 'one ground-up writer per territory over its disjoint page set' },
        { title: 'Critique', detail: 'predicate-positive conformance and capability audit, repaired in place' },
        { title: 'RedTeam', detail: 'predicate-negative pre-mortem, rebuilt in place' },
        { title: 'Serial', detail: 'ordered closers over shared governance surfaces' },
        { title: 'Drain', detail: 'cluster pooled residuals by shared file, fix and verify each cluster' },
        { title: 'Verify', detail: 'adversarial proof on disk against the campaign verification section' },
    ],
};

// --- [CONSTANTS] ------------------------------------------------------------------------

const CAMPAIGN = '.claude/scratch/observability/CAMPAIGN.md';
// Durable across runs, in the campaign home rather than per-instance run scratch: a residual a phase
// cannot close because a LATER phase owns the file has no other path forward across a gated boundary.
const CARRY = '.claude/scratch/observability/residuals-open.json';
const REPO = '/Users/bardiasamiee/Documents/99.Github/Rasm';
const DRAIN_ROUNDS = 3;
// Ceiling on concurrent drain lanes per round. Clusters stay atomic and are PACKED into lanes by
// weight, so one lane closes several file-disjoint clusters instead of one lane per cluster — the
// unpacked form spawns a lane per residual island and most do a few minutes of work.
const DRAIN_LANES = 6;

// One row per phase. `territories` fan concurrently (each owns a disjoint page set and runs its own
// implement -> critique -> redteam chain). `barrier` runs to completion first. `serial` runs in order
// after the fan, for closers that share one governance surface. `light` territories skip the reviewer
// pair, for mechanical truth work a verify already covers.
const PHASE_ROWS = {
    1: {
        title: 'C# kernel owners',
        barrier: {
            key: 'kernel',
            pages: ['libs/csharp/Rasm/.planning/Domain/telemetry.md'],
            charter:
                'Sections [01.1] through [01.4]. Mint every kernel owner the six consumer pages compose, in the signal capsule ' +
                'beside TelemetryContributorPort: the four causal-frame primitives (CorrelationId, TenantId/TenantContext with ' +
                'TenantSlot and the Stamp disposal restore plus its absent-tenant arm, ReceiptEnvelope, ReceiptSinkPort), the one ' +
                'InstrumentSpec owner carrying the union of both twin kind rosters as a [SmartEnum] and both construction shapes as ' +
                'static factories on one record, and the SLO algebra transcribed from libs/typescript/core/.planning/observe/slo.md ' +
                'in C# spelling (Sli closed family, Objective with derived budget, the four-row multi-window multi-burn-rate table, ' +
                'one AlertSeverity [SmartEnum<string>] over page|ticket, one PanelKind over board.md eight closed panel rows). ' +
                'Pin the semconv schema-url constant here beside TelemetryIdentity.Mint so all three signals bump together. ' +
                'The InstrumentRow five-arg Bind signature is CORRECT and stays; add no Cells member to ReceiptFan, since Set.Cells ' +
                'already reaches the value. Declarations must be verbatim-faithful to their current AppHost homes where the campaign ' +
                'says verbatim. You own this page ALONE; the consumer pages are rewritten by a later fan and you edit none of them.',
        },
        territories: [
            {
                key: 'apphost',
                pages: [
                    'libs/csharp/Rasm.AppHost/.planning/Runtime/lifecycle.md',
                    'libs/csharp/Rasm.AppHost/.planning/Runtime/ports.md',
                    'libs/csharp/Rasm.AppHost/.planning/Runtime/profiles.md',
                    'libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md',
                    'libs/csharp/Rasm.AppHost/.planning/Observability/health.md',
                ],
                charter:
                    'Sections [01.1], [01.3], [01.4]. Delete the four causal-frame primitives from lifecycle.md and ports.md now that ' +
                    'the kernel owns them, and repair every citation on these pages to cite the kernel. Delete health.md ' +
                    '[05]-[ALERT_ENGINE] local severity, burn, and panel types against the kernel SLO algebra, keeping the ' +
                    'domain-specific Sli instances; narrow health.md [06]-[TS_PROJECTION] frozen AlertSeverityKey to page|ticket at ' +
                    'BOTH ends. Rewrite all 21+ HostInstruments rows in instruments.md to the five-arg Bind form ' +
                    '(meter, cells, name, unit, text) => ..., dropping the enclosing Rows(LevelCells cells) capture; fix ' +
                    'new InstrumentSet(<one positional arg>) and the ReceiptFan.Of(set:, cells:, tables:) call that passes a cells: ' +
                    'the record does not declare, with the fan.Cells read becoming fan.Set.Cells.',
            },
            {
                key: 'appui-compute',
                pages: ['libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md', 'libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md'],
                charter:
                    'Sections [01.2], [01.3]. These two pages hold the divergent InstrumentSpec twins. Delete both and rewrite the ' +
                    'AppUi TelemetryRow / AppUiTelemetry.Contribute and the Compute ComputeInstrumentFan fences against the one kernel ' +
                    'owner. Delete the AppUi local severity/burn/panel types against the kernel SLO algebra, keeping the ' +
                    'domain-specific Sli instances. Cite the kernel for every causal-frame primitive.',
            },
            {
                key: 'element-bim',
                pages: ['libs/csharp/Rasm.Element/.planning/Projection/observe.md', 'libs/csharp/Rasm.Bim/.planning/Model/observability.md'],
                charter:
                    'Section [01.4]. Rewrite the InstrumentRow fences on both pages to the five-arg Bind form, dropping the enclosing ' +
                    'Rows(LevelCells cells) capture; the Element static Counter<long> Counted(Meter, string, string, string) helper ' +
                    'takes the cells thread too. Fix InstrumentSet.Of called without cells: on both pages. Cite the kernel for every ' +
                    'causal-frame primitive.',
            },
            {
                key: 'materials-fabrication',
                pages: [
                    'libs/csharp/Rasm.Materials/.planning/Projection/observability.md',
                    'libs/csharp/Rasm.Fabrication/.planning/Process/telemetry.md',
                ],
                charter:
                    'Sections [01.1], [01.3], [01.4]. Rewrite the InstrumentRow fences on both pages to the five-arg Bind form. Delete ' +
                    'the Materials [06] and the Fabrication [08]-[SLO_ROWS] local severity, burn, and panel types against the kernel ' +
                    'SLO algebra, keeping the domain-specific Sli instances — FabricationSlo must become expressible as a pack on the ' +
                    'shared Sli/Objective carrier, since a later phase makes it a real Boards provenance producer. The Fabrication ' +
                    'page lead stating the causal-frame primitives arrive from the AppHost port vocabulary becomes a kernel citation.',
            },
            {
                key: 'persistence',
                pages: ['libs/csharp/Rasm.Persistence/.planning/Store/observability.md', 'libs/csharp/Rasm.Persistence/README.md'],
                charter:
                    'Sections [01.1], [01.4]. Rewrite the InstrumentRow fences to the five-arg Bind form. Fix the UNCONDITIONAL tenant ' +
                    'attribute write at UsageReceipt.Measured so an absent tenant writes NO attribute, matching the kernel ' +
                    'TenantContext.Stamp absent-tenant arm. Both the page lead and the README lead stating the causal-frame primitives ' +
                    'arrive from the AppHost port vocabulary become kernel citations.',
            },
        ],
    },
    2: {
        title: 'Tier-0 conformance',
        territories: [
            {
                key: 'cs-apphost',
                pages: [
                    'libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md',
                    'libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md',
                    'libs/csharp/Rasm.AppHost/.planning/Runtime/profiles.md',
                ],
                charter:
                    'Sections [02.1], [02.2], and the AppHost half of [02.3]. Every governance-table edit, the resource-identity triple ' +
                    'repair, both logs legs including the persistent OTLP buffering owner, the baggage-store unification through ' +
                    'TenantContext.Stamp mirroring onto Activity.Current, Activity.AddException composition, and the boundary reword ' +
                    'naming Rasm.AppHost the branch telemetry composition owner. Verify the log-side baggage member on the assay rail ' +
                    'BEFORE writing it — if no such member exists the row becomes a BaseProcessor<LogRecord> in the branch own code.',
            },
            {
                key: 'cs-consumers',
                pages: [
                    'libs/csharp/Rasm.Bim/.planning/Model/observability.md',
                    'libs/csharp/Rasm.Materials/.planning/Projection/observability.md',
                    'libs/csharp/Rasm.Persistence/.planning/Store/observability.md',
                    'libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md',
                    'libs/csharp/Rasm.Element/.planning/Projection/observe.md',
                    'libs/csharp/Rasm.Fabrication/.planning/Process/telemetry.md',
                ],
                charter:
                    'The consumer half of [02.1] and [02.3]. Rename every metric name carrying a baked unit — the five .bytes names — ' +
                    'to .size with UCUM unit By, leaving .ratio names alone. Delete rasm.model as a metric dimension from every Bim ' +
                    'InstrumentRow and keep it as a span attribute. Route the Bim baggage read through the kernel TenantContext ' +
                    'accessor rather than Activity.Current?.GetBaggageItem or raw OTel Baggage. Add ActivityLink span links at the two ' +
                    'named fan-in boundaries: the Kafka consumer leg StoreWire<TKey,TValue> and the outbox drain OutboundSurface.Run. ' +
                    'Close [LOGLEVEL_WARNING_MEMBER]-[BLOCKED] on the Materials page against the LogLevel to SeverityNumber mapping ' +
                    'the AppHost territory lands, and close [ENVELOPE_KEY_PROMOTION]-[OPEN].',
            },
            {
                key: 'py-runtime',
                pages: [
                    'libs/python/runtime/.planning/observability/telemetry.md',
                    'libs/python/runtime/.planning/observability/metrics.md',
                    'libs/python/runtime/.planning/observability/receipts.md',
                    'libs/python/runtime/.planning/execution/lanes.md',
                ],
                charter:
                    'Section [02.4] except the producer pages. Fill all 15 INSTRUMENTS census rows including the two NEW domains ' +
                    'compute and runtime, each row carrying InstrumentKind, UCUM unit, slot, and domain. Mint the view and cardinality ' +
                    'owner in metrics.md transcribing runtime/otel/meter.md, with views= passed on MeterProvider construction in ' +
                    'telemetry.md and a SignalProfile field carrying the cardinality budget as policy. Extend WIRE_TEMPORALITY to ' +
                    'ObservableCounter and ObservableUpDownCounter, and change rasm.lane.drained from an observable counter fed by a ' +
                    'REPLACED MetricState.drain to a synchronous Counter incremented per drain. Land the scope-stamping helper beside ' +
                    'ScopeKey in receipts.md supplying (scope, version, SCHEMA_URL) and stamp every call site on these pages. Add ' +
                    'SignalProfile.sample_ratio with ParentBased(root=TraceIdRatioBased(ratio)). Resolve the two NoCompression ' +
                    'profiles per the campaign. Add the exemplar reservoir selection row. ' +
                    'The census domain set MUST equal what the producer territory records — file any mismatch as a residual naming ' +
                    'both files.',
            },
            {
                key: 'py-logs',
                pages: ['libs/python/runtime/.planning/observability/logging.md', 'libs/python/runtime/.planning/observability/bundle.md'],
                charter:
                    'Section [02.5], all seven defects, repaired in place — this page is the weakest in its folder and the rebuild is ' +
                    'ground-up. The attribute-shape defect is the largest: under the in-process OTLP arm the chain must terminate in a ' +
                    'processor handing the event dict through as attributes rather than a rendered JSON string, with ' +
                    'ProcessorFormatter.wrap_for_formatter kept for the console arm alone. Replace the ship rows, add all four ' +
                    'LogLimits fields, add critical to the LogLevel literal and the LEVEL_METHOD table, key the bridge by ScopeKey ' +
                    'using the partial(structlog.get_logger, composition=scope) pattern receipts.md already established, return a ' +
                    'LogReceipt, grow bundle.md _installs from four owners to five, and add the ' +
                    'SimpleLogRecordProcessor / InMemoryLogExporter test rail.',
            },
            {
                key: 'py-producers',
                pages: ['libs/python/geometry/.planning/graduation.md', 'libs/python/compute/.planning/graduation/observability.md'],
                charter:
                    'The producer half of section [02.4]. Every measure these pages emit must be spelled exactly as the census rows the ' +
                    'py-runtime territory lands — 9 geometry measures, 4 compute measures. Make graduation.md:17 assertion that the ' +
                    'census gates the MEASURES roster TRUE. A total-map _DOMAIN_SLOT lookup means a spelling mismatch is a ' +
                    'producer-killing KeyError, so a name you cannot reconcile is a residual naming BOTH the producer page and the ' +
                    'census page, never a guess.',
            },
            {
                key: 'py-data',
                pages: [
                    'libs/python/data/.planning/tabular/query.md',
                    'libs/python/data/.planning/tabular/lakehouse.md',
                    'libs/python/data/.planning/tabular/egress.md',
                    'libs/python/data/.planning/tabular/materialize.md',
                    'libs/python/data/.planning/tabular/profile.md',
                ],
                charter:
                    'Section [02.6] plus the four scope-stamping sites on these pages from [02.4]. Rewrite both _ir_plan substrait ' +
                    'arms against data/.api/duckdb-extensions.md [03]-[SUBSTRAIT]: table functions reached as CALL through con.execute, ' +
                    'never the four connection methods that MISS on duckdb 1.5.5. Turn the silent connectorx divert into a reach-matrix ' +
                    'refusal returning a typed fault naming the absent driver, with a version-gate row on RemoteDriver.CONNECTORX, and ' +
                    'apply the same guard to the tensorstore class at gridded/store. Stamp version= and schema_url= on the get_meter ' +
                    'and get_tracer call sites here through the receipts.md helper.',
            },
            {
                key: 'ts-otel',
                pages: [
                    'libs/typescript/runtime/.planning/otel/emit.md',
                    'libs/typescript/runtime/.planning/otel/meter.md',
                    'libs/typescript/runtime/.planning/otel/profile.md',
                ],
                charter:
                    'Section [02.7] except the Convention rows. Register the global W3C composite propagator ALONGSIDE the typed ' +
                    'Carrier path and reword the law stating the inverse; install AsyncLocalStorageContextManager on the node lanes; ' +
                    'set gzip on all three exporter arms and the native lane; resolve the JSON serialization arm; configure the ' +
                    'TraceBased exemplar filter; complete BufferConfig with scheduledDelayMillis and exportTimeoutMillis; add the ' +
                    'explicit-bucket view arm keyed by instrument name; admit and register the three server-side instrumentations with ' +
                    'the stated precedence law; bridge diag into the Effect logger at the composition root; stamp ' +
                    'Convention.profile.id on the named long-lived spans and route profile.md tags through Convention.profiled. ' +
                    'New package admissions land their manifest row and .api catalog in the same pass.',
            },
            {
                key: 'ts-convention',
                pages: ['libs/typescript/core/.planning/observe/convention.md'],
                charter:
                    'The Convention half of section [02.7]. Make service.namespace REQUIRED in Convention.identity, stamped rasm by ' +
                    'default, with tenant correctly staying off resource identity. Then run the consumer-earned cull under the page own ' +
                    'law at :24 — TWO moves, not one: rows whose consumer this campaign lands (k8s.* and container.* for the collector ' +
                    'k8sattributes processor, cloud.* for the placement arms, browser.*/device.*/session.* for the RUM stamps the vital ' +
                    'territory adds) STAY and gain their consumer; every remaining zero-consumer row is deleted. Verify each surviving ' +
                    'semconv constant against the installed distribution before keeping it — the incubating feature_flag.result.* ' +
                    'namespace was renamed within recent minors.',
            },
            {
                key: 'ts-vital',
                pages: ['libs/typescript/runtime/.planning/otel/vital.md', 'libs/typescript/ui/.planning/system/vital.md'],
                charter:
                    'Section [02.8]. runtime/otel/vital.md becomes the one CWV owner and sources all five metrics from web-vitals 6.0.0, ' +
                    'deleting the hand-rolled folds and the zero web-vitals claim; the raw PerformanceObserver path survives ONLY for ' +
                    'the render rows the library does not cover. ui/.planning/system/vital.md keeps its display-only viewer probe, stops ' +
                    'capturing CWV, and reaches the two instruments through the Vital.Report tap it already names. One seam, one ' +
                    'capture, one grade table. The three non-conformance findings (INP interaction grouping, LCP finalization, TTFB ' +
                    'activationStart) are WHY the library wins, not defects to hand-fix.',
            },
        ],
    },
    3: {
        title: 'iac deploy plane',
        barrier: {
            key: 'iac-ground',
            parallel: [
                {
                    key: 'iac-api',
                    pages: ['libs/typescript/iac/.api/'],
                    charter:
                        'Section [05.3] row [03] only, landed early because the typed collector owner in [03.4] needs a verified member ' +
                        'set. Mint the OpenTelemetry Collector chart-values and config-schema catalog at the iac .api tier: receivers ' +
                        '(otlp, prometheus, postgresql, sqlquery), processors (memory_limiter, batch, k8sattributes, resourcedetection, ' +
                        'transform, filter, tail_sampling), connectors (spanmetrics, servicegraph), exporters with sending_queue and ' +
                        'retry_on_failure field rosters, extensions (file_storage, health_check, pprof, zpages), and the service block ' +
                        'including telemetry.metrics/logs/traces. Field names verify against the real chart and collector-contrib ' +
                        'config schema — this is the most drift-prone surface in the estate and a guessed field is the defect the ' +
                        'catalog exists to foreclose. Also carry the pulumiverse-grafana NotificationPolicyArgs full field set ' +
                        'including groupBies, which unblocks [03.6].',
                },
                {
                    key: 'iac-spec',
                    pages: ['libs/typescript/iac/.planning/program/spec.md', 'libs/.planning/ARCHITECTURE.md'],
                    charter:
                        'Section [03.5]. Add the three _Observe axes — sampling head|tail, topology gateway|agent, buffer file|broker — ' +
                        'so Tier-0 arms [02] through [04] each resolve to a named spec value. On Tier-0 [FLEET_ESCALATION], delete the ' +
                        'contrib-image half of arm [02] coordinate: file_storage, prometheus, postgresql, and sqlquery are ALL ' +
                        'collector-contrib components the standing config already uses, so the contrib image is a standing fact on the ' +
                        'collector chart, never an escalation. The real arm [02] coordinate is the kafka pipeline rows alone. Touch ' +
                        'nothing else in Tier-0 — the [08] domain-vocabulary row belongs to phase 5.',
                },
            ],
        },
        territories: [
            {
                key: 'iac-observe',
                pages: ['libs/typescript/iac/.planning/operate/observe.md'],
                charter:
                    'Sections [03.1], [03.2], [03.3], [03.4], [03.6], [03.7] — the whole deploy plane on one page. Fix the collector ' +
                    'endpoint FIRST: it resolves to nothing today because the Helm fullname helper does not collapse for a release name ' +
                    'not containing the chart name, and it feeds StackOutputs.otlp, the workload env row, both ebpf endpoints, and the ' +
                    'Dev parity claim. Move it into _urls beside its five siblings. Then make the store family uniform — translation as ' +
                    'an eighth _stores column so a store swap stops silently breaking every dashboard query, the mimir ruler row, ' +
                    'recording rules compiled from the core-owned Query.breach projection, the VictoriaMetrics completion, the ' +
                    'remote-write refusal recorded as a decision, and the Thanos/Cortex growth note. Configure the chart rows taking {} ' +
                    'today — loki and tempo — including the metrics/logs tenancy asymmetry fixed at BOTH ends. Land the Schema.Class ' +
                    'collector-config owner and every absent processor, connector, and extension, the proper persistent queue with its ' +
                    'corrected durability prose, receiver hardening, the split pg-receiver DSN, and the Doppler DSN custody route. Arm ' +
                    'the two stale BLOCKED rows and close the Pyroscope one as a truthful refusal. Resolve the seven-key provenance ' +
                    'tuple per key. Read the iac-api catalog dossier before writing any collector field. ' +
                    'The spec axes land in a sibling stage — compose them as found, and file a residual if an axis you need is absent.',
            },
        ],
    },
    4: {
        title: 'Telemetry as analytics subject',
        territories: [
            {
                key: 'cs-analytics',
                pages: [
                    'libs/csharp/Rasm.Persistence/.planning/Query/columnar.md',
                    'libs/csharp/Rasm.Persistence/.planning/Store/observability.md',
                    'libs/csharp/Rasm.AppUi/.planning/Charts/telemetry.md',
                    'libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md',
                ],
                charter:
                    'Section [04.1]. Rasm.Persistence is the single columnar custodian per the branch ruling, so every piece lands ' +
                    'there and producers hand typed record-batch schemas across the seam. Land the telemetry SeriesKind row with the ' +
                    'full hypertable treatment including continuous aggregates, the AnalyticsSchema + ArtifactClass for the ' +
                    'kernel-homed ReceiptEnvelope transcribing the Rasm.Materials analytics producer-side pattern, and the chargeback ' +
                    'fact table giving UsageReceipt a named StoreSlot and a flat-table projection. Then bind the three AppUi telemetry ' +
                    'board tiles to the real projection and DELETE the designed-empty declaration. EvidenceJoin.Correlate and ' +
                    'TenantUsageFold.Fold keep their in-process live folds and gain a durable counterpart read over the columnar table. ' +
                    'Signals stay the lossy health projection; the receipt stream is the evidence plane and the only thing ingested.',
            },
            {
                key: 'py-journal',
                pages: ['libs/python/runtime/.planning/observability/'],
                charter:
                    'The journal half of section [04.2]. Python has no journal at all while the TypeScript one is the system of record. ' +
                    'Mint the journal owner under this folder transcribing libs/typescript/data/.planning/journal/{fact,append,evolve,' +
                    'retain}.md in Python spelling: append-only fact stream, the closed AuditFact/MeterFact union, exact-decimal ' +
                    'rating, the Retain.Class retention vocabulary, and the evidence-vs-series law stated identically. Erasure is ' +
                    'crypto-shredding, never deletion. Land its INSTRUMENTS census rows in the same pass. Register the new page in the ' +
                    'folder README router and ARCHITECTURE codemap.',
            },
            {
                key: 'py-sink',
                pages: ['libs/python/data/.planning/tabular/'],
                charter:
                    'The sink half of section [04.2]. A DatasetKind member for the receipt stream written through the existing ' +
                    'Lakehouse LakeOp x TableFormat matrix — Delta for the mutable evidence table, Parquet on the object plane for the ' +
                    'cold tail — partitioned on (domain, date) and Z-ordered on (tenant, content_key), with the existing WriteTuning ' +
                    'statistics and bloom-filter machinery applying unchanged. CostLedger keeps its in-process harvest and gains a ' +
                    'lake-sourced path so a cost frame reconstructs from the sink rather than only from live baggage; CostDomain gains ' +
                    'a telemetry member. Add aws, azure, and postgres_scanner to the extension roster.',
            },
            {
                key: 'ts-olap',
                pages: ['libs/typescript/data/.planning/lane/olap.md'],
                charter:
                    'Section [04.3]. Write the missing [05]-[FLIGHT] section — the cluster is declared, the catalog, manifest, README ' +
                    'row, and TASKLOG card all exist, and six of seven clusters are realized. Scoped createFlightSqlClient off lane ' +
                    'config, query/executeUpdate/prepared/transaction/metadata, decodeFlightDataToTable onto the same Arrow plane ' +
                    'Olap.ingest rides, encodeRecordBatchesToFlightData for doPut, the FlightError family folded into the existing ' +
                    'OlapFault wire reason, transport reusing the one @connectrpc/connect stack. Close ' +
                    '[FLIGHT_SQL_INGRESS_ROW]-[QUEUED]. Add the CREATE SECRET credential rail transcribing the C# reference form, ' +
                    'sourcing material from the object plane existing grant. Complete _extensions with postgres, sqlite, aws, azure. ' +
                    'Land the OTLP-to-lake lane with identity as the join key so the derived plane stays derived. Grow the lead ' +
                    'paragraph to name PROFILE, ARROW_WIRE, and FLIGHT.',
            },
        ],
    },
    5: {
        title: 'Contract, .api truth, manifests, closure',
        territories: [
            {
                key: 'api-repairs',
                pages: ['libs/csharp/.api/', 'libs/python/.api/', 'libs/typescript/runtime/.api/', 'libs/typescript/.api/'],
                charter:
                    'Section [05.2]. Seven fabricated or wrong rows to correct or delete, ten uncataloged members that fences already ' +
                    'compose to add, and the 61-constant semconv verification. EVERY row verifies before it lands on the rail its ' +
                    'language names in the campaign verification section — assay api query for C# host and NuGet surfaces, live ' +
                    'reflection against the repo .venv for Python, node_modules inspection plus Context7 for TypeScript. ' +
                    'api-otel-persistent-storage.md is re-extracted whole, being the weakest catalog in the set. ' +
                    'SpanProcessor.onEnding is the single most consequential uncataloged member on the TypeScript branch and its row ' +
                    'carries the experimental flag the consuming fence comment already names. A member you cannot verify is NEVER ' +
                    'authored — file it as a residual naming the catalog and the consuming fence.',
            },
            {
                key: 'api-mints',
                pages: ['libs/python/.api/', 'libs/typescript/runtime/.api/', 'libs/csharp/Rasm.Persistence/.api/'],
                charter:
                    'Section [05.3] rows [01], [02], [04], [05], [06], [07] — row [03], the collector catalog, already landed in phase ' +
                    '3 and is not re-minted. Mint each catalog at full depth with verified members and a STACKING section, homed to the ' +
                    'owning tier. Also resolve the memray question at its owner: admit it with its catalog or record the refusal on the ' +
                    'bundle page boundary, since the support-bundle profile artifact has no producer either way today.',
            },
            {
                key: 'dual-home',
                light: true,
                pages: ['libs/csharp/.api/', 'libs/csharp/Rasm.AppHost/.api/', 'libs/csharp/Rasm.Persistence/.api/'],
                charter:
                    'Section [05.4]. Eight packages carry both a branch-tier and a folder-tier catalog. Keep the branch tier — the ' +
                    'deeper file in every pair — merge any folder-tier row the branch tier lacks, delete the folder-tier file, and ' +
                    're-point every research-row route that currently reaches the thinner tier first. Then the mirror defect in the ' +
                    'opposite direction: api-otel-instrumentation-aws.md sits at the shared tier with one folder consumer and moves ' +
                    'down. The catalog-alignment law in campaign-method.md [03] governs.',
            },
            {
                key: 'manifests',
                light: true,
                pages: ['Directory.Packages.props', 'pyproject.toml', 'pnpm-workspace.yaml', 'libs/typescript/runtime/RULINGS.md'],
                charter:
                    'Section [05.5]. Every new package row confirms on the feed before it lands — mcp__nuget__get_latest_package_version ' +
                    'for NuGet, the registry for npm. Resolve the @opentelemetry/api 1.9.1 pin question against the registry: it is the ' +
                    'one pin on the observability roster that does not corroborate. Rewrite the runtime RULINGS @opentelemetry/* row to ' +
                    'govern the ELEVEN real independent pin lines rather than the two it claims. Narrow the AppHost no-in-memory-' +
                    'exporter prose to production tiers rather than dropping the package. Split Lgtm.Versions so a chart version and an ' +
                    'image reference are distinct fields.',
            },
            {
                key: 'stacks-exemplar',
                light: true,
                pages: ['docs/stacks/typescript/rails-and-effects.md'],
                charter:
                    'Section [05.7], and NOTHING else in docs/stacks/ is touched and no card is minted against it. Fix the three metric ' +
                    'names to the dotted no-suffix grammar, keep the snippet project-agnostic, and state the UCUM unit carrier: Effect ' +
                    'metric constructors accept no unit option and the @effect/opentelemetry bridge derives the descriptor unit from a ' +
                    'metric tag named unit, so construction applies Metric.tagged and a generated SDK view strips the synthetic tag from ' +
                    'exported attributes while preserving the descriptor unit. Add the base2-exponential default beside the ' +
                    'explicit-bucket fallback in [BOUNDED_DIMENSIONS].',
            },
        ],
        serial: [
            {
                key: 'namespace-authority',
                pages: ['libs/.planning/ARCHITECTURE.md'],
                charter:
                    'Section [05.6]. 601 distinct rasm.* names exist, minted independently per branch under three conventions, with ' +
                    'four prefixes claimed by two or three packages each and nothing detecting a collision. The overlaps are NOT ' +
                    'defects — the domain segment names a capability domain, not a package — but that is nowhere stated. Land one ' +
                    'Tier-0 [08] row fixing the closed domain vocabulary with each branch mapping its packages onto it, plus the gate ' +
                    'that a domain segment outside the vocabulary fails the check. Derive the vocabulary from the real name census on ' +
                    'disk, never from a guess.',
            },
            {
                key: 'contract',
                pages: ['tests/contracts/MANIFEST.md', 'tests/RULINGS.md', 'libs/.planning/ARCHITECTURE.md'],
                charter:
                    'Section [05.1]. One infrastructure entry, ledger row [13], following the [ENTRY_SCHEMA] in ' +
                    'tests/contracts/README.md EXACTLY. Its shape is the closed row set the campaign [02] audit table enumerates, and ' +
                    'the metric-name grammar carries the domain vocabulary the previous serial stage landed. Rewrite the Tier-0 [08] ' +
                    'carve sentence: transcription remains how each branch spells the rows, and the corpus entry is what proves the ' +
                    'three spellings agree — a manifest entry is a schema plus frozen assets, not a shared library, so the ' +
                    'split-maturity reason never applied to it. Role-aware absence is part of the shape, not an exception to it. ' +
                    'tests/RULINGS.md gains its first observability row recording that.',
            },
            {
                key: 'closure',
                pages: [
                    'libs/.planning/RULINGS.md',
                    'libs/csharp/.planning/RULINGS.md',
                    'libs/typescript/runtime/RULINGS.md',
                    'libs/typescript/data/RULINGS.md',
                    'docs/laws/topology.md',
                    'libs/.planning/IDEAS.md',
                    'libs/.planning/TASKLOG.md',
                ],
                charter:
                    'Section [05.8], the terminal closer. Land all seven RULINGS rows at the narrowest owning tier and all four new ' +
                    'topology couplings, and grow [INSTRUMENT_ADMISSION] with its C# and TypeScript halves — today it binds Python ' +
                    'producers only while the same census law applies to InstrumentFan rows and Convention._instrument rows. Reconcile ' +
                    'the cards: reopen each of the four falsely-[COMPLETE] cards with its specific residual and then close it against ' +
                    'the landed work rather than deleting the history; the two [BLOCKED] cards stay blocked and gain the arming ' +
                    'coordinate phase 3 made real. Verify every claim against current disk — earlier phases may have already closed ' +
                    'part of a card. Finally delete observability-initial-plan.md at the repo root. ' +
                    'IDEAS cards for security.audit, grasshopper.fan, and compute.descriptor land at their OWNING folders, not here.',
            },
        ],
    },
    6: {
        title: 'Campaign finalize',
        territories: [
            {
                key: 'conformance',
                kind: 'verify',
                light: true,
                pages: [
                    'libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md',
                    'libs/python/runtime/.planning/observability/telemetry.md',
                    'libs/typescript/core/.planning/observe/convention.md',
                    'libs/typescript/runtime/.planning/otel/emit.md',
                ],
                charter:
                    'The [02] conformance table, re-walked row by row across all three branches. Thirteen rows x three branches = 39 ' +
                    'cells, and the bar is zero FAIL and zero PARTIAL. Each cell is proven by a probe against the owning fence on ' +
                    'disk, never by the campaign doc claim that it was fixed: grep the resource-attribute fence for ' +
                    'service.namespace, the governance row for delta and base2_exponential, every exporter arm for gzip, every ' +
                    'get_meter/get_tracer/Mint call site for the version and schema_url arguments, the propagator registration, the ' +
                    'exemplar configuration, the tenant view and its cardinality cap, and the absent-tenant arm. Confirm the semconv ' +
                    'schema pin is the SAME literal in all three branches. Confirm no metric name anywhere carries a unit suffix. ' +
                    'Repair every cell that fails.',
            },
            {
                key: 'seams',
                kind: 'verify',
                light: true,
                pages: ['libs/csharp/Rasm/.planning/Domain/telemetry.md', 'libs/python/runtime/.planning/observability/metrics.md'],
                charter:
                    'The fence-seam sweep and the census gate. Grep EVERY composing fence in libs/csharp for InstrumentRow(, ' +
                    'InstrumentSet.Of(, ReceiptFan.Of(, InstrumentSpec, AlertSeverity, and PanelKind, and confirm each matches its ' +
                    'kernel declaration on spelling, arity, and rail shape — the [FENCE_SEAM] law own check, and the corpus-wide ' +
                    'drain the [RETIRED_SURFACE_DRAIN] scar demands, so the boundary is zero occurrences of the retired form ' +
                    'anywhere, never the anchor list this charter names. Then the census equality probe: the domain set recorded in ' +
                    'the Python INSTRUMENTS census equals the domain set every producer records, checked from BOTH sides, and every ' +
                    'rasm.* measure name in libs/python resolves to a census row. Then confirm every rasm.* domain segment across all ' +
                    'three branches resolves to the Tier-0 domain vocabulary phase 5 landed.',
            },
            {
                key: 'arms',
                kind: 'verify',
                light: true,
                pages: [
                    'libs/.planning/ARCHITECTURE.md',
                    'libs/typescript/iac/.planning/operate/observe.md',
                    'libs/typescript/iac/.planning/program/spec.md',
                ],
                charter:
                    'Arm resolution and endpoint resolution — the two checks that failed hardest at campaign start. Every Tier-0 ' +
                    '[FLEET_ESCALATION] and [PROFILE_SWAP] coordinate resolves to a named spec value or a landed row AT THE OWNER IT ' +
                    'NAMES: open each named owner and confirm the row exists there, since three of four arms named surfaces that did ' +
                    'not exist. Then every URL the iac plane publishes derives from _urls and survives the Helm fullname collapse — ' +
                    'check each release name against its chart name and confirm the rendered service name, since that collapse is ' +
                    'exactly what made the collector endpoint dead. No endpoint is spelled outside _urls. Confirm every _stores row ' +
                    'answers every column the family carries, translation strategy included.',
            },
            {
                key: 'truth',
                kind: 'verify',
                light: true,
                pages: ['libs/csharp/.api/', 'libs/python/.api/', 'libs/typescript/runtime/.api/', 'libs/typescript/iac/.api/'],
                charter:
                    'Member and manifest truth across everything this campaign landed. Every .api row added or corrected re-verifies ' +
                    'on its language rail, and every NEW member any fence composes has a catalog row. Confirm no dual-homed catalog ' +
                    'pair survives and no folder-tier file redirects to a substrate-tier catalogue. Confirm every package a fence ' +
                    'composes carries its central-manifest row, its folder README registry row, and its owning .api catalog — the ' +
                    'three-way touch-point alignment, checked in both directions so an orphan is repaired at its owner rather than ' +
                    'removed. Re-confirm every new manifest version on its feed. A member that will not verify is DELETED from the ' +
                    'catalog and its consuming fence repaired, never left standing.',
            },
        ],
        serial: [
            {
                key: 'custody',
                pages: [
                    'libs/.planning/ARCHITECTURE.md',
                    'libs/.planning/RULINGS.md',
                    'docs/laws/topology.md',
                    'tests/contracts/MANIFEST.md',
                    'libs/.planning/IDEAS.md',
                    'libs/.planning/TASKLOG.md',
                ],
                charter:
                    'The campaign custody close. Dispatch the infra-custodian subagent over the touched infra set — Tier-0, the four ' +
                    'RULINGS tiers, docs/laws/topology.md, tests/contracts/MANIFEST.md, and the card files — and IMPLEMENT its ' +
                    'returned verdict rows yourself; its findings are signals you re-verify on disk, never law. Then run ' +
                    'uv run python -m tools.assay docs check over every page this campaign touched and clear what it reports, with ' +
                    'tables re-padded to the 150-column cap. Confirm every card the campaign claimed to close is genuinely closed ' +
                    'against landed work and that no card was closed by deleting its history. Confirm observability-initial-plan.md ' +
                    'is gone from the repo root. Land the pooled harvest through the docs/laws/README.md admission ladder, ' +
                    'refutation-first — landing nothing is a first-class verdict.',
            },
        ],
    },
};

// --- [INPUTS] ---------------------------------------------------------------------------

const parsed = typeof args === 'string' && /^\s*[[{]/.test(args) ? JSON.parse(args) : args;
const rawPhase = parsed && typeof parsed === 'object' ? parsed.phase : parsed;
const PHASE = Number.isFinite(Number(rawPhase)) ? String(Number(rawPhase)) : '';
const ROW = PHASE_ROWS[PHASE] || null;

const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH = '.claude/scratch/observability-p' + (PHASE || '0') + '-' + fnv1a(JSON.stringify({ phase: PHASE }));

// --- [MODELS] ---------------------------------------------------------------------------

const ANCHOR = {
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: { type: 'string' },
        line: { type: 'integer' },
        role: { type: 'string', enum: ['state', 'defect', 'ruling', 'catalog', 'counterpart', 'absence'] },
        note: { type: 'string' },
    },
};

const DOSSIER = {
    type: 'object',
    additionalProperties: false,
    required: ['facts', 'members', 'seams', 'coverage', 'summary'],
    properties: {
        facts: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['topic', 'statement', 'anchors'],
                properties: { topic: { type: 'string' }, statement: { type: 'string' }, anchors: { type: 'array', items: ANCHOR } },
            },
        },
        members: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['package', 'symbol', 'signature', 'tier', 'status', 'route'],
                properties: {
                    package: { type: 'string' },
                    symbol: { type: 'string' },
                    signature: { type: 'string' },
                    tier: { type: 'string' },
                    status: { type: 'string', enum: ['used', 'underutilized', 'unused', 'absent', 'unverified'] },
                    route: { type: 'string' },
                },
            },
        },
        seams: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['concept', 'ends', 'note'],
                properties: { concept: { type: 'string' }, ends: { type: 'array', items: { type: 'string' } }, note: { type: 'string' } },
            },
        },
        coverage: {
            type: 'object',
            additionalProperties: false,
            required: ['requested', 'read', 'skipped', 'unverified'],
            properties: {
                requested: { type: 'array', items: { type: 'string' } },
                read: { type: 'array', items: { type: 'string' } },
                skipped: { type: 'array', items: { type: 'string' } },
                unverified: { type: 'array', items: { type: 'string' } },
            },
        },
        summary: { type: 'string' },
    },
};

const RECEIPT = {
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'headline', 'failure'],
    properties: {
        ok: { type: 'boolean' },
        report: { type: 'string' },
        entries: { type: 'integer' },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};

// `class` is the drain's admission gate, schema-enforced so it cannot be argued around in prose: the
// drain works capability, truth, and seam rows and DROPS cosmetic ones. Classifying by enum beats
// pattern-matching claim text, which fractures per lane and silently admits the next spelling.
const RESIDUAL = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['files', 'claim', 'owner', 'class'],
        properties: {
            files: { type: 'array', items: { type: 'string' } },
            claim: { type: 'string' },
            owner: { type: 'string' },
            class: { type: 'string', enum: ['capability', 'truth', 'seam', 'cosmetic'] },
        },
    },
};

const HARVEST = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['lesson', 'hardens', 'evidence'],
        properties: { lesson: { type: 'string' }, hardens: { type: 'string' }, evidence: { type: 'string' } },
    },
};

const FIXLOG = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'deltas', 'landed', 'beyond', 'residual', 'harvest', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        deltas: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['symbol', 'change'],
                properties: { symbol: { type: 'string' }, change: { type: 'string' } },
            },
        },
        landed: { type: 'array', items: { type: 'string' } },
        beyond: { type: 'array', items: { type: 'string' } },
        residual: RESIDUAL,
        harvest: HARVEST,
        summary: { type: 'string' },
    },
};

const CARRYIN = {
    type: 'object',
    additionalProperties: false,
    required: ['existed', 'rows'],
    properties: { existed: { type: 'boolean' }, rows: RESIDUAL },
};

const DRAINLOG = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'resolved', 'open', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        resolved: { type: 'array', items: { type: 'string' } },
        open: RESIDUAL,
        summary: { type: 'string' },
    },
};

const VERDICT = {
    type: 'object',
    additionalProperties: false,
    required: ['checks', 'repaired', 'blocked', 'summary'],
    properties: {
        checks: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['check', 'passed', 'evidence'],
                properties: { check: { type: 'string' }, passed: { type: 'boolean' }, evidence: { type: 'string' } },
            },
        },
        repaired: { type: 'array', items: { type: 'string' } },
        blocked: RESIDUAL,
        summary: { type: 'string' },
    },
};

// --- [DOCTRINE] -------------------------------------------------------------------------

const LAW_GROUND =
    'GROUND: this repo is in a planning phase — the artifact is the design corpus, and to implement is to author or deepen a ' +
    'markdown CODE FENCE inside a design page. No source tree lands. Read, at source and in full, before any edit: the campaign ' +
    'root doc ' +
    CAMPAIGN +
    ', the repo CLAUDE.md, libs/.planning/{ARCHITECTURE,RULINGS,campaign-method,README}.md, docs/laws/{scars,topology}.md, and the ' +
    'route-owned code doctrine under docs/stacks/<language>/ for every language you touch. For a design page under ' +
    'libs/<language>/, ALSO read every file in libs/<language>/.planning/ and both .api tiers that own the packages your fences ' +
    'compose — the shared libs/<language>/.api/ substrate and the folder <root>/.api/ tier. That doctrine read is never delegated ' +
    'and never ridden off a summary.';

const LAW_WRITE =
    'WRITER BAR: author ground-up, never patch. New capability weaves into the owning shape as if designed in from the start — a ' +
    'case, row, field, operation, or policy value INSIDE an existing owner before any new surface appears. No shims, aliases, ' +
    'migration layers, obsolete markers, or append-beside; the API breaks when the collapse improves the system. Capability is ' +
    'conserved absolutely: densify, never delete, and zero current consumers never lowers the bar. Assume 10x the complexity and ' +
    'demand on every surface — a naive, shallow, or surface-level form is rejected and rebuilt. Variation lives in input shape, ' +
    'policy values, and table rows behind one polymorphic entrypoint per concern; anything that can vary is parameterized, never ' +
    'hardcoded. Mine every admitted package to the operator depth it ships — a hand-rolled reimplementation of shipped capability ' +
    'is a defect. Naivety is intolerable on three axes: COVERAGE (a thin slice of a domain that carries far more), APPROACH ' +
    '(enumerated instances where a parameterized algorithmic owner generates the space), AUTHORITY (a profile, provider, receipt, ' +
    'or external package treated as the semantic owner).';

const LAW_MEMBER =
    'MEMBER TRUTH: every external member you write verifies first on its language rail — uv run python -m tools.assay api query ' +
    '--key <key> --symbol <Symbol> for C# host and NuGet surfaces, live reflection against ' +
    REPO +
    '/.venv for Python distributions, node_modules inspection plus the context7 MCP for TypeScript packages, and the nuget MCP ' +
    'for package existence and newest version. An unverifiable member is NEVER authored — it becomes a residual naming the ' +
    'catalog and the consuming fence. When a fence and its catalog disagree, resolve on the rail and correct the losing surface.';

const LAW_PROSE =
    'PROSE: declarative, assertive, present tense, active voice; every word load-bearing. No hedging, no future-gating, no meta ' +
    'commentary, no narration trails, no counts or version literals that go stale by construction, no emojis. Never open a ' +
    'sentence or a leader tail on an article. Tables repair in place at the 150-column cap. Prose is a system prompt for a cold ' +
    'agent reader — human-facing narration is a defect.';

const LAW_RIPPLE =
    'RIPPLE: fix every defect you find at its ROOT in the same pass, including defects outside your listed pages, EXCEPT where a ' +
    'live sibling territory in this same run owns the file — those become residuals. Your listed pages are where you look FIRST, ' +
    'never the bound on what you may fix. When you edit a page, land its counterpart obligations the same pass: the folder README ' +
    'router row, the folder ARCHITECTURE codemap and seam ledger at BOTH ends, the .api catalog row, and the central manifest row. ' +
    'A settled decision with no home lands as a RULINGS row at the narrowest owning tier; a new-owner or scope-expansion finding ' +
    'lands as a complete card at the narrowest tier following that file own template comment. docs/laws/topology.md binds ' +
    'counterpart obligations — consult it before any multi-surface edit.';

const LAW_RESIDUAL =
    'RESIDUAL SHAPE: a residual is work you could not land because a LIVE sibling territory in this run owns the file, or because ' +
    'a member could not be verified. Every residual names the FULL file list it spans (so the drain can cluster by shared file), ' +
    'the claim as fact, the canonical owner, and its class. A residual is never a note-to-self and never a substitute for fixing ' +
    'something you can reach. ' +
    'CLASS decides whether the drain ever sees the row: `capability` is missing or wrong design capability, `truth` is a member, ' +
    'catalog, or manifest correctness defect, `seam` is a cross-file ownership or counterpart obligation — the drain works all ' +
    'three. `cosmetic` is table padding, column width, header or index numbering, section-marker spelling, and every other ' +
    'mechanical prose-gate finding; the drain DISCARDS it unread. Fix a cosmetic defect only in a file you already have open for ' +
    'a substantive reason, and otherwise leave it: it costs a later pass nothing to fix incidentally, and sweeping it across the ' +
    'corpus buries the campaign diff under noise nobody asked for. Classifying a formatting defect as `capability` to smuggle it ' +
    'past this gate is the defect the enum exists to foreclose.';

const LAW_HARVEST =
    'HARVEST: required but usually EMPTY. Nominate only a generalizable lesson — a reusable collapse pattern, an unnamed naivety ' +
    'class, a hard-won coupling, a review rule that would catch the defect class before review — each citing the exact existing ' +
    'clause it hardens or proving absence across the surfaces you searched. A stage-local fix NEVER nominates.';

const DOCTRINE = LAW_GROUND + '\n\n' + LAW_WRITE + '\n\n' + LAW_MEMBER + '\n\n' + LAW_PROSE + '\n\n' + LAW_RIPPLE;

// --- [OPERATIONS] -----------------------------------------------------------------------

const pagesOf = (t) => t.pages.join(', ');

const reconPrompt = (t) =>
    'ROLE: read-only mapping lane for the observability campaign. You WRITE NOTHING except your own dossier file. ' +
    'Read ' +
    CAMPAIGN +
    ' IN FULL first — it is the campaign blueprint and names your territory charter. ' +
    'TERRITORY: ' +
    pagesOf(t) +
    '. CHARTER THIS TERRITORY WILL EXECUTE: ' +
    t.charter +
    ' YOUR JOB: read every territory page IN FULL, plus the folder README/ARCHITECTURE/RULINGS that govern them and BOTH .api ' +
    'tiers owning the packages their fences compose. Produce a consumer-scoped map for the writer that follows you: (1) FACTS — ' +
    'current-state statements anchored to path and line, including every defect the charter names, verified present or already ' +
    'absent; (2) MEMBERS — every package member the territory composes or plainly could, classified used / underutilized / unused ' +
    '/ absent / unverified, each with its verified signature, its owning .api tier, and the route you verified it on; ' +
    'shallow usage of a member whose surface carries a family counts as UNDERUTILIZED; (3) SEAMS — every cross-file and ' +
    'cross-territory endpoint this territory touches, both ends named. ' +
    'LAW: your product carries INFORMATION, never prescriptions — anchored facts, verified spellings, seam endpoints, capability ' +
    'the concept admits but nothing exploits. An entry telling the writer what to build is this lane defect. EMPTINESS IS NOT ' +
    'EVIDENCE: a probe returning nothing proves absence only after you re-run it in a second form; garbled output is your own ' +
    'tooling error, never a property of the territory. Record honest skips in coverage. ' +
    LAW_MEMBER;

const writePrompt = (t, dossierPath, priorNav) =>
    'ROLE: implementation writer owning ' +
    pagesOf(t) +
    ' in the observability campaign. ' +
    DOCTRINE +
    '\n\nCONSUMPTION LADDER, in this order and no other: (1) YOUR OWN BLIND PASS FIRST — open your territory pages and derive your ' +
    'own defect list, collapse targets, and design rulings from disk BEFORE opening any recon product; the majority of your diff ' +
    'comes from your own attack, and a diff that maps one-to-one onto the recon rows has failed this mandate. (2) THEN read the ' +
    'recon dossier at ' +
    dossierPath +
    ' IN FULL as grounding to verify and exceed — never a ceiling, never a settled question you relitigate. ' +
    (priorNav ? '(3) Navigation from an already-landed sibling stage, location facts only: ' + JSON.stringify(priorNav) + '. ' : '') +
    '\n\nCHARTER: ' +
    t.charter +
    '\n\n' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST +
    '\n\nWrite every change in place with the Edit and Write tools. Report files touched, symbol deltas as data, what landed against ' +
    'the charter, what you fixed BEYOND the charter on your own authority, residuals, and harvest.';

const critiquePrompt = (t, dossierPath, nav) =>
    'ROLE: CRITIQUE reviewer with full writer authority over ' +
    pagesOf(t) +
    '. The pages were authored by ANOTHER engineer and are naive, shallow, or illusory until they survive a real attack; the ' +
    'burden of proof is on the work. Dense, confident, idiom-fluent output is the PRIME suspect for hollowness — a name promising ' +
    'capability the body omits, decorative density carrying nothing, a stub dressed as finished work. ' +
    DOCTRINE +
    '\n\nCOLD PASS FIRST: derive your own defect list from the pages on disk before consulting anything below. ' +
    'Then the recon dossier at ' +
    dossierPath +
    ' as grounding, and these location facts to reach touched territory fast: ' +
    JSON.stringify(nav) +
    '\n\nYOUR OBJECTIVE is predicate-POSITIVE — the clause-by-clause conformance and capability-completeness audit, distinct from ' +
    'the red-team that follows you: DOCTRINE, every route-owned clause holds at its owning fence; SHAPE, the collapse, owner, knob, ' +
    'aspect, rail, language, and entry-point laws hold and repeated structure folds into its algebraic owner; CAPABILITY, bodies ' +
    'deliver every capability their names, cards, packages, and boundaries promise; TRUTH, both .api tiers support every external ' +
    'member and capability claim; OWNERSHIP, strata, seams, and rulings agree at every touched end. ' +
    'Cite the clause for every repair. Go BEYOND fixing: types, constants, and functions sharing a discriminant COLLAPSE into ' +
    'stronger owners, thin owners EXTEND to the full domain their concept carries, and a fundamentally stronger design once seen ' +
    'is BUILT. ' +
    'CHARTER THE WRITER WORKED TO: ' +
    t.charter +
    '\n\nNO CHURN: every edit names a violated law or invariant and the concrete case that breaks it. A clean verdict earned by an ' +
    'attack that finds nothing is a first-class result. ' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

const redteamPrompt = (t, nav, claims) =>
    'ROLE: RED-TEAM rebuilder with full writer authority over ' +
    pagesOf(t) +
    '. You are the terminal pre-mortem on this territory and you RECONSTRUCT rather than annotate. ' +
    DOCTRINE +
    '\n\nCOLD PASS FIRST: derive your own attack from the pages on disk. Location facts to reach territory fast: ' +
    JSON.stringify(nav) +
    ' A prior reviewer filed these as landed — treat them strictly as UNVERIFIED CLAIMS to refute against the current pages, ' +
    'never as a settled record: ' +
    JSON.stringify(claims) +
    '\n\nYOUR OBJECTIVE is predicate-NEGATIVE: COUNTERFACTUAL — remove the central owner, enumeration, dispatch, or hand-rolled ' +
    'kernel and land the stronger form the removal exposes. GROWTH — owner data absorbs the next case, dimension, modality, ' +
    'provider, or consumer, and the proof of a correct shape is the diff of the next feature: one declaration inside the owner, ' +
    'every consumer untouched or loudly broken. LONG TAIL — runtime edge states and failure modes (empty, singular, plural, ' +
    'stream, malformed, concurrent, cancelled, partial-failure) preserve the declared rails, parameterization, and boundaries. ' +
    'COMPOSITION — re-derive package choice, lower-stratum ownership, policy resolution, routing, lifecycle, and caller ' +
    'orchestration. INTEGRITY — repair downward dependency, duplicated ownership, host leakage, sibling-interior coupling, sprawl, ' +
    'and phantom members at every touched end. COLD CLOSE — re-judge every conformance dimension by name against the rebuilt ' +
    'result before your verdict. ' +
    'CHARTER: ' +
    t.charter +
    '\n\n' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

const finalizePrompt = (t, dossierPath, nav) =>
    'ROLE: campaign-terminal VERIFY lane owning one lens over the WHOLE landed observability campaign. You are adversarial, never ' +
    'confirming, and you hold full WRITER authority — you repair what you disprove rather than reporting it. ' +
    'The campaign you are checking closed six cards as [COMPLETE] that a later audit refuted; assume this run did the same to ' +
    'itself and hunt accordingly. ' +
    DOCTRINE +
    '\n\nLENS: ' +
    t.charter +
    '\n\nMETHOD: run every probe as a REAL command against current disk, never from the campaign doc claims. Re-derive whether each ' +
    'claimed change was NECESSARY and prove on disk that it LANDED — a claim that does not reproduce is a finding. Status flips ' +
    'against evidence alone: a gap closes only against a cited .api line, a grep result, or harness output pasted as evidence. ' +
    'EMPTINESS IS NOT EVIDENCE — a probe returning nothing proves absence only after you re-run it in a second form; a malformed ' +
    'flag, wrong glob, ignored directory, or path typo returns exactly the silence a clean surface returns, and garbled output is ' +
    'your own tooling error, never a property of the territory. ' +
    'Where a check fails, repair at the ROOT: a single-point patch is itself the defect you repair wherever a denser ' +
    'reconstruction of the same files is available. ' +
    '\n\nGrounding, read AFTER your own cold probes have run: the recon dossier at ' +
    dossierPath +
    ', and these location facts from the phases that landed: ' +
    JSON.stringify(nav) +
    '\n\nReport each probe you ran and its verdict in `landed` as `<check> :: PASS|FAIL :: <evidence>`, what you repaired in ' +
    '`files` and `beyond`, and anything genuinely unreachable as a residual. ' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

const serialPrompt = (t, landedNav) =>
    'ROLE: serial closer owning ' +
    pagesOf(t) +
    ' in the observability campaign. These are shared governance surfaces, so you run alone — no sibling is writing them. ' +
    DOCTRINE +
    '\n\nCHARTER: ' +
    t.charter +
    '\n\nEvery claim you land verifies against CURRENT disk — earlier stages in this run already changed the corpus, and a claim ' +
    'carried from the campaign doc that disk now refutes is corrected, not transcribed. Location facts from the stages that ran ' +
    'before you: ' +
    JSON.stringify(landedNav) +
    '\n\n' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

// Stage-one route: a territory declares its kind, and the row picks the builder. A verify-kind
// territory carries a lens over the whole landed campaign, never a page set to author.
const STAGE_ONE = { write: writePrompt, verify: finalizePrompt };
const openPrompt = (t, dossierPath, nav) => (STAGE_ONE[t.kind] || writePrompt)(t, dossierPath, nav);

const navOf = (r) => (r ? { files: r.files || [], deltas: r.deltas || [], beyond: r.beyond || [] } : { files: [], deltas: [], beyond: [] });

const lane = (t, promptText) =>
    agent(
        promptText +
            '\n\nPRODUCT TO DISK: write your COMPLETE dossier as one JSON file matching this schema at ' +
            REPO +
            '/' +
            SCRATCH +
            '/' +
            t.key +
            '-recon-dossier.json (Write tool, absolute path; delete any prior file at that path first). Schema: ' +
            JSON.stringify(DOSSIER) +
            ' — then return ONLY the receipt: ok, the report path, the entries count, a one-line mechanical headline, failure empty.',
        { label: 'recon:' + t.key, phase: 'Recon', model: 'sonnet', effort: 'high', schema: RECEIPT },
    ).then((r) => ({
        key: t.key,
        pages: t.pages,
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'lane died'),
    }));

const clusterByFile = (rows) => {
    const parent = rows.map((_, i) => i);
    const find = (i) => (parent[i] === i ? i : (parent[i] = find(parent[i])));
    const union = (a, b) => {
        const ra = find(a);
        const rb = find(b);
        if (ra !== rb) parent[ra] = rb;
    };
    const byFile = new Map();
    rows.forEach((r, i) => {
        (r.files || []).forEach((f) => {
            if (byFile.has(f)) union(byFile.get(f), i);
            else byFile.set(f, i);
        });
    });
    const groups = new Map();
    rows.forEach((_, i) => {
        const k = find(i);
        if (!groups.has(k)) groups.set(k, []);
        groups.get(k).push(rows[i]);
    });
    return [...groups.values()];
};

const dedupe = (rows) => [...new Map(rows.map((r) => [(r.files || []).slice().sort().join(',') + '|' + r.claim, r])).values()];

// Cost of one cluster: its rows plus the distinct files those rows span. Balancing on row COUNT alone
// pairs a one-row-fifty-file cluster with a fifty-row-one-file cluster and recreates the long pole.
const weigh = (cl) => cl.length + new Set(cl.flatMap((r) => r.files || [])).size;
const clusterKey = (cl) =>
    cl
        .map((r) => (r.files || []).slice().sort().join(',') + '|' + r.claim)
        .sort()
        .join('~');

// Longest-processing-time greedy: heaviest cluster first into the lightest lane. Clusters are
// file-disjoint by construction, so any packing of them stays disjoint across lanes and the lanes
// still write concurrently without collision. Ties break on a content key, never on iteration order,
// so the packing is identical on a resume.
const pack = (clusters, lanes) => {
    const bins = Array.from({ length: Math.max(1, Math.min(lanes, clusters.length)) }, () => ({ weight: 0, clusters: [] }));
    clusters
        .slice()
        .sort((a, b) => weigh(b) - weigh(a) || clusterKey(a).localeCompare(clusterKey(b)))
        .forEach((cl) => {
            const lightest = bins.reduce((min, b) => (b.weight < min.weight ? b : min), bins[0]);
            lightest.clusters.push(cl);
            lightest.weight += weigh(cl);
        });
    return bins.filter((b) => b.clusters.length);
};

// --- [COMPOSITION] ----------------------------------------------------------------------

if (!ROW) {
    log('no phase — pass {phase: 1..5}; available: ' + Object.keys(PHASE_ROWS).join(', '));
    return { skipped: true, reason: 'no phase argument', available: Object.keys(PHASE_ROWS) };
}

log('phase ' + PHASE + ' — ' + ROW.title + ' · ' + ROW.territories.length + ' territories · scratch ' + SCRATCH);

// --- [RECON]

phase('Recon');
const barrierLanes = ROW.barrier ? (ROW.barrier.parallel ? ROW.barrier.parallel : [ROW.barrier]) : [];
const reconTargets = barrierLanes.concat(ROW.territories);
const dossiers = (await parallel(reconTargets.map((t) => () => lane(t, reconPrompt(t))))).filter(Boolean);
const dossierOf = (key) => {
    const d = dossiers.find((x) => x.key === key);
    return d && d.ok ? d.report : '(no dossier — this lane failed; do your own cold read of every territory page first)';
};
log(dossiers.filter((d) => d.ok).length + '/' + dossiers.length + ' recon lanes landed');

// --- [KERNEL]

let barrierLogs = [];
if (barrierLanes.length) {
    phase('Kernel');
    barrierLogs = (
        await parallel(
            barrierLanes.map(
                (b) => () =>
                    agent(writePrompt(b, dossierOf(b.key), null), {
                        label: 'kernel:' + b.key,
                        phase: 'Kernel',
                        effort: 'max',
                        schema: FIXLOG,
                    }).then((r) => ({ key: b.key, ...(r || {}) })),
            ),
        )
    ).filter(Boolean);
    log('barrier landed: ' + barrierLogs.map((b) => b.key + '(' + (b.files || []).length + ' files)').join(' · '));
}
const barrierNav = barrierLogs.length ? { barrier: barrierLogs.map((b) => ({ key: b.key, ...navOf(b) })) } : null;

// --- [IMPLEMENT]

const chains = (
    await pipeline(
        ROW.territories,
        (t) =>
            agent(openPrompt(t, dossierOf(t.key), barrierNav), {
                label: (t.kind === 'verify' ? 'lens:' : 'write:') + t.key,
                phase: 'Implement',
                effort: 'max',
                schema: FIXLOG,
            }),
        (fix, t) =>
            t.light
                ? { fix, crit: null }
                : agent(critiquePrompt(t, dossierOf(t.key), navOf(fix)), {
                      label: 'critique:' + t.key,
                      phase: 'Critique',
                      effort: 'max',
                      schema: FIXLOG,
                  }).then((crit) => ({ fix, crit })),
        (prev, t) =>
            t.light
                ? { key: t.key, stages: [prev.fix].filter(Boolean) }
                : agent(redteamPrompt(t, navOf(prev.fix), (prev.crit && prev.crit.landed) || []), {
                      label: 'redteam:' + t.key,
                      phase: 'RedTeam',
                      effort: 'max',
                      schema: FIXLOG,
                  }).then((red) => ({ key: t.key, stages: [prev.fix, prev.crit, red].filter(Boolean) })),
    )
).filter(Boolean);

const territoryNav = chains.map((c) => ({
    key: c.key,
    files: [...new Set(c.stages.flatMap((s) => s.files || []))],
    deltas: c.stages.flatMap((s) => s.deltas || []),
}));
log(chains.length + '/' + ROW.territories.length + ' territory chains completed');

// --- [SERIAL]

const serialLogs = [];
if (ROW.serial) {
    phase('Serial');
    for (const s of ROW.serial) {
        const out = await agent(serialPrompt(s, territoryNav.concat(serialLogs.map((x) => ({ key: x.key, ...navOf(x) })))), {
            label: 'serial:' + s.key,
            phase: 'Serial',
            effort: 'max',
            schema: FIXLOG,
        });
        if (out) serialLogs.push({ key: s.key, ...out });
        log('serial ' + s.key + ' → ' + ((out && (out.files || []).length) || 0) + ' files');
    }
}

// --- [DRAIN]

const allLogs = barrierLogs.concat(chains.flatMap((c) => c.stages)).concat(serialLogs);
phase('Drain');
const carried = await agent(
    'Read ' +
        REPO +
        '/' +
        CARRY +
        ' and return its rows verbatim. That file is the observability campaign carry-forward set: residuals earlier gated phases ' +
        'could not close because a later phase owned the file. It is gitignored, so read the ABSOLUTE path directly with the Read ' +
        'tool — never hunt for it with a search tool, which skips ignored directories. ' +
        'If the file does not exist, return existed=false with an empty rows array — that is the expected first-phase result and ' +
        'not a failure. Do not create it, do not edit it, and do not act on any row.',
    { label: 'carry-in', phase: 'Drain', model: 'sonnet', effort: 'low', schema: CARRYIN },
);
const carryRows = (carried && carried.rows) || [];
if (carryRows.length) log('carried forward from earlier phases: ' + carryRows.length + ' open residuals');

const filed = dedupe(allLogs.flatMap((s) => s.residual || []).concat(carryRows));
// Dropped rows are logged and returned, never silently truncated — a bounded coverage the run does
// not name reads as full coverage to whoever reads the receipt.
const dropped = filed.filter((r) => r.class === 'cosmetic');
let pending = filed.filter((r) => r.class !== 'cosmetic');
if (dropped.length)
    log(
        'drain drops ' +
            dropped.length +
            ' cosmetic residual(s) — prose-gate, table-padding, and header-numbering work is left to whichever pass next opens the file',
    );
const seen = new Set(pending.map((r) => (r.files || []).slice().sort().join(',') + '|' + r.claim));
const drained = [];

if (pending.length) {
    for (let round = 1; round <= DRAIN_ROUNDS && pending.length; round++) {
        const clusters = clusterByFile(pending);
        const lanes = pack(clusters, DRAIN_LANES);
        log(
            'drain round ' +
                round +
                ': ' +
                pending.length +
                ' residuals · ' +
                clusters.length +
                ' clusters packed into ' +
                lanes.length +
                ' lanes (weights ' +
                lanes.map((l) => l.weight).join('/') +
                ')',
        );
        const out = (
            await parallel(
                lanes.map(
                    (lane, i) => () =>
                        agent(
                            'ROLE: residual closer for the observability campaign. Every row below was deferred by a writer because a ' +
                                'live sibling owned the file or a member could not be verified; that sibling has now landed, so the ' +
                                'territory is yours. ' +
                                DOCTRINE +
                                '\n\nYou hold ' +
                                lane.clusters.length +
                                ' INDEPENDENT residual clusters, delivered as an array of arrays. Every cluster is file-disjoint from ' +
                                'every other cluster in your lane AND from every cluster held by a concurrent sibling lane, so no ' +
                                'sibling is writing any file you touch. Work them one cluster at a time, closing each fully before ' +
                                'opening the next — a cluster is the coherence boundary, and interleaving them loses the shared-file ' +
                                'context that made them one cluster. Report one merged result across every cluster you held.' +
                                '\n\nRe-verify EVERY row against current disk FIRST and cull with proof any row a later stage already ' +
                                'resolved — a stale residual is the common case, not the exception. Fix the rest at their ROOT, reading ' +
                                'every listed file in full. Report only rows you could NOT close, each naming its blocker and owner.' +
                                '\n\nSCOPE: cosmetic defects were filtered out before you were dispatched and are NOT your work. A ' +
                                'mechanical prose-gate finding you notice mid-fix — table padding, column width, header or index ' +
                                'numbering, marker spelling — is corrected only inside a file you already opened to close a ' +
                                'substantive row, and never pursued into a file outside these clusters. A corpus-wide sweep is ' +
                                'reserved for draining a surface THIS campaign retired, where zero remaining occurrences is the ' +
                                'proof; it is never licensed by formatting drift that predates the campaign.' +
                                '\n\nCLUSTERS: ' +
                                JSON.stringify(lane.clusters),
                            { label: 'drain:' + round + ':' + i, phase: 'Drain', effort: 'high', schema: DRAINLOG },
                        ),
                ),
            )
        ).filter(Boolean);
        const changed = out.some((o) => (o.files || []).length);
        drained.push(...out.flatMap((o) => o.resolved || []));
        const next = [];
        for (const o of out)
            for (const r of o.open || []) {
                const k = (r.files || []).slice().sort().join(',') + '|' + r.claim;
                if (!seen.has(k)) {
                    seen.add(k);
                    next.push(r);
                }
            }
        if (!changed) {
            pending = next;
            log('drain round ' + round + ' changed no file — stopping');
            break;
        }
        pending = next;
    }
}

// --- [VERIFY]

phase('Verify');
const verdict = await agent(
    'ROLE: terminal VERIFY lane for phase ' +
        PHASE +
        ' (' +
        ROW.title +
        ') of the observability campaign. You are adversarial, never confirming, and you WRITE the improvement your proof exposes. ' +
        DOCTRINE +
        '\n\nRun the checks in section [06]-[VERIFICATION] of ' +
        CAMPAIGN +
        ' that apply to THIS phase, on disk, with real commands — the fence-seam greps, the census equality probe, the conformance ' +
        'table re-walk, the arm-resolution check, the endpoint-resolution check, and uv run python -m tools.assay docs check over ' +
        'every touched page. Re-derive whether each claimed change was NECESSARY and prove on disk that it LANDED; a claim that does ' +
        'not reproduce is a finding. Where a check fails, REPAIR it at the root rather than reporting it — a single-point patch is ' +
        'itself the defect you repair wherever a denser reconstruction of the same files is available. Status flips against evidence ' +
        'alone: a gap marks closed only against a cited .api line or harness output. ' +
        'EMPTINESS IS NOT EVIDENCE — re-run any probe that returns nothing in a second form before concluding absence, and read a ' +
        'garbled result as your own tooling error. ' +
        '\n\nPAGES THIS PHASE TOUCHED: ' +
        JSON.stringify([...new Set(allLogs.flatMap((s) => s.files || []))]) +
        '\n\nCLAIMS the stages filed as landed. You held none of these positions. Treat every one as an UNVERIFIED claim to refute ' +
        'against the current pages — a claim that does not reproduce on disk is this phase primary finding, and the campaign exists ' +
        'because six such claims stood as [COMPLETE] against a corpus that refuted them: ' +
        JSON.stringify(allLogs.flatMap((s) => (s.landed || []).map((c) => ({ from: s.key || '', claim: c })))) +
        '\n\nRESIDUALS STILL OPEN after the drain — verify each is genuinely blocked rather than merely deferred, and close every one ' +
        'you can reach: ' +
        JSON.stringify(pending) +
        '\n\nCARRY-FORWARD, your last act and NOT optional: write the rows you could not close to ' +
        REPO +
        '/' +
        CARRY +
        ' as a JSON object {"existed": true, "rows": [...]} matching ' +
        JSON.stringify(CARRYIN) +
        ' — each row carrying the FULL file list it spans, the claim as fact, and the owner. This file is how a residual survives ' +
        'the gated boundary to the phase that owns its files; a row you drop here is lost from the campaign entirely. Write the file ' +
        'even when the set is empty, with rows as an empty array, so the next phase reads a true state rather than a stale one. ' +
        (PHASE === '6'
            ? 'THIS IS THE TERMINAL PHASE: a non-empty set here is a campaign-level failure, not a handoff — say so plainly in your ' +
              'summary and name each blocker and its owner, because no later phase exists to receive it.'
            : 'Rows whose files a LATER phase owns are correct to carry; rows you merely found inconvenient are not.'),
    { label: 'verify:p' + PHASE, phase: 'Verify', effort: 'max', schema: VERDICT },
);

const harvest = dedupe(allLogs.flatMap((s) => s.harvest || []).map((h) => ({ files: [h.hardens], claim: h.lesson, owner: h.evidence }))).map((h) => ({
    lesson: h.claim,
    hardens: h.files[0],
    evidence: h.owner,
}));

return {
    phase: Number(PHASE),
    title: ROW.title,
    scratch: SCRATCH,
    recon: { landed: dossiers.filter((d) => d.ok).length, failed: dossiers.filter((d) => !d.ok).map((d) => d.key) },
    barrier: barrierLogs.map((b) => ({ key: b.key, files: (b.files || []).length, summary: b.summary })),
    territories: chains.map((c) => ({
        key: c.key,
        stages: c.stages.length,
        files: [...new Set(c.stages.flatMap((s) => s.files || []))],
        beyond: c.stages.flatMap((s) => s.beyond || []),
    })),
    serial: serialLogs.map((s) => ({ key: s.key, files: (s.files || []).length, summary: s.summary })),
    drain: { resolved: drained.length, open: pending, droppedCosmetic: dropped },
    verify: verdict,
    harvest,
};
