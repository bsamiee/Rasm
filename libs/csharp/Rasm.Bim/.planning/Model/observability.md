# [BIM_OBSERVABILITY]

Composition-scoped observability for the BIM-and-exchange engine: `BimHooks` composes the kernel signal capsule into the closed `rasm.bim.<domain>.<point>` roster `BimPoint` declares, `BimTelemetry` projects typed receipts onto `rasm.bim.<domain>.<measure>` instruments as a registry subscriber — domain code fires facts and observability projects them, zero emit calls inside a projector or codec arm — and `BimBenchReceipt` closes the evidence loop: every Rasm.Bim performance claim is a typed, corpus-gated receipt, never a prose number.

Wire posture: HOST-LOCAL, BCL-only. Point, instrument-spec, advice-bucket, contributor-port, package-identity, tenancy-frame, fault-category, and trace-band machinery arrives settled from the kernel signal capsule, so no OpenTelemetry package is reachable here; SDK composition, exporters, exemplar policy, views, and cardinality caps stay at the app roots, which admit the `Rasm.Bim` meter by name and `BimPoint.Scopes` into their one `SpanBand`. Subscriber failure parks point-attributed on the composition's evidence cell, the emitter untouched.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: `BimPoint` closes the point vocabulary on its kernel `Modality` column and its derived `TraceScope` plane, `BimHooks` mints that roster as one per-composition registry record over the kernel point capsule, `BimFact` closes the payload family every point types over, and `StageMark` is the one stage-evidence carrier every native lane's roster projects.
- [03]-[TELEMETRY_TAP]: `BimTelemetry` declares the `rasm.bim.<domain>.<measure>` roster as kernel `InstrumentSpec` rows, mints the contributor port, rails the tap subscriptions, and owns the span and attribution law over the kernel `SpanBand`.
- [04]-[BENCH_RECEIPTS]: `BimBenchClaims` rosters the per-op kernel `BenchClaim` rows, `BimBenchReceipt` carries the run evidence, and the corpus-gate row admits a claim as standing.

## [02]-[HOOK_RAIL]

- Owner: `BimPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.bim.<domain>.<point>` with the kernel `HookModality` column and the kernel `TraceScope` plane derived off the id's own head; `BimHooks` the per-composition registry record — one instance per app composition, so two apps built on the library never fight over hook slots and no process-global registry exists; `BimFact` the closed payload family every point types over. Point capsule, modality rows, detacher, and isolation are the kernel signal capsule composed as settled vocabulary.
- Cases: point roster rows — `rasm.bim.exchange.progress` (observe, `BimFact.Progress` — the DWG/DXF decode lane's `StageMark` stream off ACadSharp `ICadReader.OnProgress`, the import lane's own stage roster projecting the mark), `rasm.bim.exchange.imported` (observe, `BimFact.Imported` — the `ModelLoad` receipt fact post-decode), `rasm.bim.exchange.exported` (observe, `BimFact.Exported` — the export-rail artifact emit), `rasm.bim.projection.lowered` (observe, `BimFact.Lowered` — the seam `GraphDelta` magnitude off the semantic projector), `rasm.bim.projection.legality` (veto, `BimFact.Admission` — an app policy refuses a graph delta before it lands), `rasm.bim.projection.emit` (veto, `BimFact.Egress` — the `Projection/egress#IFC_EGRESS` `Emit` fold consults it against the elected format and target schema BEFORE authoring, so a deliverable policy refuses on the exchange coordinates rather than on a delta that already landed), `rasm.bim.review.verdict` (replay, `BimFact.Verdict` — IDS-facet and template-audit outcomes, buffered so a late panel drains the recent window), `rasm.bim.energy.progress` (observe, `BimFact.Progress` — the energy translate lane's `StageMark` stream, its `TranslateStage` rows projecting the mark off OpenStudio `ProgressBar.onPercentageUpdated`), `rasm.bim.planning.progress` (observe, `BimFact.Progress` — the `Planning/schedule#SCHEDULE_NETWORK` calendar walk and the `Planning/cost#COST_ROLLUP` decomposition rollup projecting their own stage rosters over long networks), `rasm.bim.energy.emitted` (observe, `BimFact.Emitted` — the `EnergyReceipt` fact per artifact), `rasm.bim.exchange.textured` (observe, `BimFact.Textured` — the appearance channel census `MaterialFinish.Author` and `AppearanceProjection.TexturesOf` fire once per authored surface style, so the one exchange leg that drops payload by design is counted rather than silent), `rasm.bim.exchange.degrade` (observe, `BimFact.Degraded` — an exchange leg completing while shedding capability, its closed lane and reason keys banding the counter while the identifier-grade subject rides the fact alone).
- Entry: `BimHooks.Live()` mints the roster once at composition by seating one kernel point per `BimPoint` row; an emitting page fires its declared point value (`hooks.Imported.Fire(fact)`), so a name-resolved lookup surface never exists; `Veto`, `Observe`, and `Drain` are the capsule's subscriber entries, each returning the disposable detacher; `Points` hands the point set to `HookRegistry.Mount` at the app root, and `BimPoint.Scopes` enters that root's `SpanBand.Of(version, scopes)`.
- Auto: fire order is the capsule's law — retention first, the veto fold second (the first refusal is the emitter's verdict AND parks on the evidence cell), observe taps forked and shielded last, so `Fire` returns without waiting on any tap; `BimIo.ImportGeometry` and `EnergyTranslate.Run` take `Option<BimHooks> hooks = default` — the optional slot every later fire-site entry repeats — so a hook-less composition pays one `IsNone` test and a fired point with zero subscribers costs one empty fold.
- Receipt: a hook fire is the evidence event itself — the emitter's typed receipt already carries the fact, so a point mints nothing; the `Faults` cell (`Atom<Seq<IsolatedFault>>`) is the one registry evidence surface — veto refusals and shielded tap faults, point-attributed — drained by the composing app and projected onto the `[03]` rejects counter through the cell's `Change` tap.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm, BCL inbox.
- Growth: a new point is one `BimPoint` row, one `BimHooks` field with its `Live()` seat, and one `BimFact` case, and a point on a new domain segment arms its span plane with no roster edit because the plane derives off the id; a new subscriber is one `Observe`/`Veto` call at composition; delivery semantics are the kernel modality rows; a new native lane keeps its OWN closed stage roster projecting `StageMark` — the carrier is this page's, the rows are the lane's, and a second mark shape or a free-text stage slot is the deleted form.
- Boundary: point ids compose the kernel `HookId` grammar with the package segment pinned `bim`, so a Bim point joins any app-tier registry census unrenamed — Bim declares its points here and the composing app subscribes direct; ids and modalities live on the roster rows alone, so a `Live()` seat re-spelling either is the forked-vocabulary defect; the fire path is the one synchronous shape, and an effect-rail caller lifts `Fire` at its own composition seam; the payload closes at declaration — every `BimHooks` field types its point to one `BimFact` case, so a stringly payload cannot enter the rail; telemetry is a tap, never a producer — `[03]-[TELEMETRY_TAP]` subscribes as observe rows here.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// Point roster keyed rasm.bim.<domain>.<point> — the kernel HookId four-segment grammar. Modality is the kernel
// column deciding veto admission and replay retention; Plane is the id's own rasm.<pkg>.<domain> head, the
// kernel KernelDomain derivation in this package's vocabulary, so a span scope can never fork from the point it
// brackets and a new domain segment arms its scope with no second spelling.
[SmartEnum<string>]
public sealed partial class BimPoint {
    public static readonly BimPoint ExchangeProgress = new("rasm.bim.exchange.progress", modality: HookModality.Observe);
    public static readonly BimPoint Imported = new("rasm.bim.exchange.imported", modality: HookModality.Observe);
    public static readonly BimPoint Exported = new("rasm.bim.exchange.exported", modality: HookModality.Observe);
    public static readonly BimPoint Lowered = new("rasm.bim.projection.lowered", modality: HookModality.Observe);
    public static readonly BimPoint Legality = new("rasm.bim.projection.legality", modality: HookModality.Veto);
    public static readonly BimPoint Egress = new("rasm.bim.projection.emit", modality: HookModality.Veto);
    public static readonly BimPoint Verdict = new("rasm.bim.review.verdict", modality: HookModality.Replay);
    public static readonly BimPoint EnergyProgress = new("rasm.bim.energy.progress", modality: HookModality.Observe);
    public static readonly BimPoint PlanningProgress = new("rasm.bim.planning.progress", modality: HookModality.Observe);
    public static readonly BimPoint Emitted = new("rasm.bim.energy.emitted", modality: HookModality.Observe);
    public static readonly BimPoint Textured = new("rasm.bim.exchange.textured", modality: HookModality.Observe);
    public static readonly BimPoint ExchangeDegrade = new("rasm.bim.exchange.degrade", modality: HookModality.Observe);

    // Items-derived index materializes on first read, so a bracket pays a lookup rather than re-parsing the id.
    static readonly Lazy<FrozenDictionary<BimPoint, TraceScope>> Planes = new(
        static () => Items.ToFrozenDictionary(static row => row, static row =>
            TraceScope.Create(value: string.Join('.', row.Key.Split('.')[..3]))),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public HookModality Modality { get; }

    public TraceScope Plane => Planes.Value[this];

    // Composing roots admit this roster into SpanBand.Of; points share planes per domain head, so the projection
    // deduplicates, and an unadmitted scope refuses on the kernel rail rather than dropping every span silently.
    public static Seq<TraceScope> Scopes => toSeq(Planes.Value.Values).Distinct().Strict();
}

// --- [MODELS] -----------------------------------------------------------------------------
// The ONE stage-evidence carrier every native lane's stage roster projects: Done the published fraction of the
// whole lane at the mark, Witness the lane-owned stage token. Rosters stay PLURAL per lane (the kernel
// ArrangeStage is internal to Rasm.Meshing, so a cross-package roster owner would invert strata) — a discrete
// ladder projects its declared rows, a continuous native callback mints marks with a live Done under one
// witness. The fraction is REQUIRED: a declared stage always carries one, and a free-text stage with no
// fraction is unrepresentable on the fact.
public readonly record struct StageMark(double Done, string Witness);

// One closed payload family every hook point types over: one Op-keyed case per fact shape, so a point's
// fact type is a case and the tap reads typed evidence. Format, codec, leg, tier, and outcome slots carry
// each CLOSED vocabulary KEY the firing page projects down (InterchangeFormat.Key, EnergyLeg key, verdict
// row key) — so the S0 Model stratum consumes no Exchange/Energy/Review sibling type, and tag cardinality
// stays bounded because every key originates in a closed vocabulary at the fire site.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BimFact {
    private BimFact(Op key) => Key = key;

    public Op Key { get; }

    public sealed record Progress(Op Key, string Domain, StageMark Stage) : BimFact(Key);
    public sealed record Imported(Op Key, string Format, string Codec, long Bytes, int Blocks, int Instances, Duration Elapsed) : BimFact(Key);
    public sealed record Exported(Op Key, string Format, long Bytes, Duration Elapsed) : BimFact(Key);
    public sealed record Lowered(Op Key, string Projector, int Nodes, int Edges) : BimFact(Key);
    public sealed record Admission(Op Key, GraphDelta Delta) : BimFact(Key);
    // The egress admission the Emit fold consults BEFORE authoring: format and target schema are the two facts
    // an app policy refuses on, and Nodes is the scope magnitude a deliverable gate bounds — the fact is
    // PRE-artifact, disjoint from Exported, which reports the artifact a passed emit produced.
    public sealed record Egress(Op Key, string Format, string Schema, int Nodes) : BimFact(Key);
    public sealed record Verdict(Op Key, string Tier, string Outcome, int Findings) : BimFact(Key);
    public sealed record Emitted(Op Key, string Leg, string Format, int Warnings) : BimFact(Key);
    // Texture binding is the one exchange leg that drops payload BY DESIGN: a channel with no admitted
    // target, an unresolvable image reference, and a coordinate set the target format cannot carry all
    // fall out silently at the appearance projection. The three counts are disjoint by construction —
    // Bound is what reached the artifact, Dropped is what a target refused, Unresolved is what never
    // resolved to bytes — so the sum is the authored channel census and a missing texture is attributable
    // to its cause rather than merely absent.
    public sealed record Textured(Op Key, string Format, int Bound, int Dropped, int Unresolved) : BimFact(Key);
    // The exchange lane's NAMED degradation: a leg that completed while shedding capability — a codec falling
    // back, a feature the target format cannot carry, a substituted approximation. Lane and Reason are CLOSED
    // vocabulary keys the firing leg projects down, so the counter bands bounded; Subject is the identifier-grade
    // element or artifact the degradation landed on and rides the fact for a reader, never a metric dimension.
    public sealed record Degraded(Op Key, string Lane, string Reason, string Subject) : BimFact(Key);
}

// --- [COMPOSITION] ------------------------------------------------------------------------
// Per-composition registry record: one Live() per app composition, every point reached through its
// declared field, one shared evidence cell per composition — the kernel capsule seated per roster row, so id
// and modality belong to the roster and never to a construction literal.
public sealed record BimHooks(
    HookPoint<BimFact.Progress> ExchangeProgress,
    HookPoint<BimFact.Imported> Imported,
    HookPoint<BimFact.Exported> Exported,
    HookPoint<BimFact.Lowered> Lowered,
    HookPoint<BimFact.Admission> Legality,
    HookPoint<BimFact.Egress> Egress,
    HookPoint<BimFact.Verdict> Verdict,
    HookPoint<BimFact.Progress> EnergyProgress,
    HookPoint<BimFact.Progress> PlanningProgress,
    HookPoint<BimFact.Emitted> Emitted,
    HookPoint<BimFact.Textured> Textured,
    HookPoint<BimFact.Degraded> ExchangeDegrade,
    Atom<Seq<IsolatedFault>> Faults) {
    public static BimHooks Live() {
        Atom<Seq<IsolatedFault>> faults = Atom(Seq<IsolatedFault>());
        return new(
            Seat<BimFact.Progress>(BimPoint.ExchangeProgress, faults),
            Seat<BimFact.Imported>(BimPoint.Imported, faults),
            Seat<BimFact.Exported>(BimPoint.Exported, faults),
            Seat<BimFact.Lowered>(BimPoint.Lowered, faults),
            Seat<BimFact.Admission>(BimPoint.Legality, faults),
            Seat<BimFact.Egress>(BimPoint.Egress, faults),
            Seat<BimFact.Verdict>(BimPoint.Verdict, faults),
            Seat<BimFact.Progress>(BimPoint.EnergyProgress, faults),
            Seat<BimFact.Progress>(BimPoint.PlanningProgress, faults),
            Seat<BimFact.Emitted>(BimPoint.Emitted, faults),
            Seat<BimFact.Textured>(BimPoint.Textured, faults),
            Seat<BimFact.Degraded>(BimPoint.ExchangeDegrade, faults),
            faults);
    }

    // Mount table the app root audits every registered point through.
    public Seq<IHookPoint> Points =>
        Seq<IHookPoint>(ExchangeProgress, Imported, Exported, Lowered, Legality, Egress, Verdict, EnergyProgress,
            PlanningProgress, Emitted, Textured, ExchangeDegrade);

    static HookPoint<TFact> Seat<TFact>(BimPoint row, Atom<Seq<IsolatedFault>> faults) =>
        new(id: HookId.Create(value: row.Key), modality: row.Modality, faults: faults);
}
```

## [03]-[TELEMETRY_TAP]

- Owner: `BimTelemetry` the one roster and projection owner — receipts stay billing truth, instruments are the lossy dashboard channel projected from them; rows are kernel `InstrumentSpec` declarations carrying kind, measurement form, and their closed `Dimensions` columns, advice bounds read the kernel `Buckets` holder, and the write capsule is the kernel `InstrumentSet`.
- Cases: projection map — `rasm.bim.exchange.import.duration`/`import.size`/`instancing` off `BimFact.Imported` (duration, payload size, and instance placements sharing one evidence read), `rasm.bim.exchange.export.duration` off `BimFact.Exported`, `rasm.bim.projection.nodes`/`edges` off `BimFact.Lowered`, `rasm.bim.legality.rejects` off the `Faults` evidence cell through its `Change` tap (veto refusals and hook-tap isolations, banded by point and kernel fault category), `rasm.bim.review.verdicts` off `BimFact.Verdict`, `rasm.bim.energy.exchanges`/`warnings` off `BimFact.Emitted`, `rasm.bim.exchange.texture.drops` off `BimFact.Textured` banded by format and drop cause (the two loss causes write, the bound count does not — it is the artifact's own evidence), `rasm.bim.exchange.degrades` off `BimFact.Degraded` banded by lane and reason.
- Entry: `BimTelemetry.Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl)` — the string-scoped contributor port the composing root materializes, scope the kernel `TelemetrySource.Bim` identity and the semconv coordinate defaulting to the kernel pin; a root outside that fan binds `InstrumentSet.Of(cells, (meter, Rows))` directly against its own minted meter, never both. `BimTelemetry.Tap(BimHooks hooks, InstrumentSet set)` mounts the observe subscriptions at composition, so declaration and write calls live only inside this spine. `BimTelemetry.Traced<T>(SpanBand? band, BimPoint at, Op op, string model, Func<Fin<T>> body, params ReadOnlySpan<(string Slot, object? Value)> marks)` is the span wrapper every long-running entry composes over the composing root's kernel band, model identity a required argument so no Bim span exists unattributed and the band nullable so a composition admitting no scope runs the identical rail untraced.
- Auto: every advised row ships its explicit-bucket bounds through the kernel's `InstrumentKind` x `MeasureForm` derivation, so this page names a bound row and never a create call; instrument identity de-duplicates by name inside the meter, so name, unit, kind, and description are declaration facts this roster carries once; tag values ride the typed fact's own vocabulary keys, never free text.
- Receipt: none — the tap projects receipts and hook facts; a metric minted beside it is a second truth. Five points project NO series by design: the three `Progress` streams are operator feedback whose live fraction is span-and-panel material, not a bounded series, and the two veto points reach the rejects counter only through their parked refusals on the `Faults` cell — so an un-projected point is a stated posture, never an oversight. Every arm returns the kernel write rail and subscribes through the capsule's own rail-shaped `Observe`, which lifts the refusal and parks it point-attributed, so a folder-local rail-to-effect adapter has nothing to add; the rejects counter alone discards, because its park re-enters the cell it observes.
- Packages: LanguageExt.Core, NodaTime, Rasm (the kernel instrument mechanism, the scope identity roster, the fault-category slot, the tenancy frame, and the trace band), BCL inbox.
- Growth: a new projected fact is one `InstrumentSpec` roster row with its `Dimensions` and one `Tap` subscription arm; every row therefore declares the kernel tenant slot beside its fact dimensions, while a new span dimension is one slot row here and one `marks` pair at the composing entry; a per-vocabulary instrument family derives from its owning vocabulary rows, never hand-enumerated names.
- Boundary: library altitude holds zero OpenTelemetry reference and zero span custody — the meter reaches the process only through the composing root's mint, so provider disposal owns instrument lifetime, and the kernel `SpanBand` owns the one `ActivitySource` per admitted scope, its listener gate, its `using` close, and its typed fail-leg status, so this page declares `BimPoint.Scopes` and holds no source, no wrapper, and no disposable, and a composition holding no band runs untraced on the null receiver rather than minting one; instrument custody is one-per-composition — either the app fan materializes the `Telemetry` port or a root binds `InstrumentSet.Of` locally, never both; subscription law — `Tap` mounts at composition ahead of the first fire, because the capsule fans a `Replay` point's held window to each fresh subscriber and a late attach therefore re-counts that window onto the verdict counter; span law — the span name IS the kernel `Op` and the plane the point's own id head, so bracket and fact never name two scopes, and the typed verdict — not a tag — carries the error fact; attribution law — dimension slots carry this package's dotted `rasm.bim.<dimension>` namespace so a concept a sibling package also tags never collides, the fault-category band reads the kernel slot rather than re-declaring one, tenancy is the kernel `TenantContext` projection every metric write folds so this page holds no tenant key and no baggage read, and model identity is identifier-grade: it rides the span alone as `Traced`'s own required argument, because one metric series per model is unbounded cardinality no view cap recovers, no baggage writer exists to read it from, and a slot left to caller discipline is a slot no caller stamps; the span fold never re-stamps the tenant partition the app root's baggage promotion already carries; SDK composition, exporters, exemplars, views, and cardinality caps stay at the app roots.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Diagnostics;
using LanguageExt;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [OPERATIONS] -------------------------------------------------------------------------
// Instrument names and dimension slots are constants both the declaration rows and the write arms compose, so a
// dimension cannot drift from the tag its own series carries. Kind and MeasureForm are row columns the kernel
// derives every create body from; Scope is the kernel's minted package identity, the tenant slot rides the
// kernel tenancy frame, and the fault-category band reads the kernel slot — this page mints no telemetry
// identity and no cross-package dimension of its own.
public static class BimTelemetry {
    public static readonly string Scope = TelemetrySource.Bim.Key;

    public const string ImportDuration = "rasm.bim.exchange.import.duration";
    // Size, never bytes: the estate name grammar carries no unit suffix and the UCUM By unit states the measure.
    public const string ImportSize = "rasm.bim.exchange.import.size";
    public const string Instancing = "rasm.bim.exchange.instancing";
    public const string ExportDuration = "rasm.bim.exchange.export.duration";
    public const string ProjectionNodes = "rasm.bim.projection.nodes";
    public const string ProjectionEdges = "rasm.bim.projection.edges";
    public const string LegalityRejects = "rasm.bim.legality.rejects";
    public const string ReviewVerdicts = "rasm.bim.review.verdicts";
    public const string EnergyExchanges = "rasm.bim.energy.exchanges";
    public const string EnergyWarnings = "rasm.bim.energy.warnings";
    public const string TextureDrops = "rasm.bim.exchange.texture.drops";
    public const string ExchangeDegrades = "rasm.bim.exchange.degrades";

    // Model identity stays SPAN-only: identifier-grade cardinality is free on a sampler-thinned span and
    // unbounded on a series. The slot carries the package namespace like every sibling dimension — a bare
    // `rasm.model` forks the moment a second package names a model, which the fabrication roster already does.
    // Traced stamps it from its own required argument, so this key has exactly one write site.
    public const string ModelSlot = "rasm.bim.model";

    public const string CodecSlot = "rasm.bim.codec";
    public const string FormatSlot = "rasm.bim.format";
    public const string LegSlot = "rasm.bim.energy.leg";
    public const string OutcomeSlot = "rasm.bim.review.outcome";
    public const string PointSlot = "rasm.bim.point";
    public const string ProjectorSlot = "rasm.bim.projector";
    public const string TierSlot = "rasm.bim.review.tier";
    // Drop cause is the whole point of this counter: a refused target and an unresolvable image are
    // different exchange defects, so one counter banded by cause replaces two counters that would drift.
    public const string ChannelSlot = "rasm.bim.exchange.texture.cause";
    // A degradation's two BOUNDED axes. The fact's Subject stays off the series for the ModelSlot reason: it is
    // identifier-grade and would multiply the counter by the elements a lane degraded.
    public const string LaneSlot = "rasm.bim.exchange.lane";
    public const string ReasonSlot = "rasm.bim.exchange.reason";

    public static readonly Seq<InstrumentSpec> Rows = Seq(
        InstrumentSpec.Advised(ImportDuration, "s", "foreign-bytes decode wall duration per format and codec",
            MeasureForm.Real, Buckets.DecodeSeconds, TenantContext.TenantSlot, FormatSlot, CodecSlot),
        InstrumentSpec.Advised(ImportSize, "By", "decoded source byte count per format",
            MeasureForm.Whole, Buckets.ByteSizes, TenantContext.TenantSlot, FormatSlot),
        InstrumentSpec.Advised(Instancing, "{instance}", "instance placements per decoded pool",
            MeasureForm.Whole, Buckets.GraphCounts, TenantContext.TenantSlot, FormatSlot),
        InstrumentSpec.Advised(ExportDuration, "s", "artifact emit wall duration per format",
            MeasureForm.Real, Buckets.DecodeSeconds, TenantContext.TenantSlot, FormatSlot),
        InstrumentSpec.Advised(ProjectionNodes, "{node}", "seam delta node magnitude per projector",
            MeasureForm.Whole, Buckets.GraphCounts, TenantContext.TenantSlot, ProjectorSlot),
        InstrumentSpec.Advised(ProjectionEdges, "{edge}", "seam delta edge magnitude per projector",
            MeasureForm.Whole, Buckets.GraphCounts, TenantContext.TenantSlot, ProjectorSlot),
        InstrumentSpec.Count(LegalityRejects, "{reject}", "legality and hook rejections banded by point and fault category",
            MeasureForm.Whole, TenantContext.TenantSlot, PointSlot, KernelInstruments.CategorySlot),
        InstrumentSpec.Count(ReviewVerdicts, "{verdict}", "review verdicts by tier and outcome",
            MeasureForm.Whole, TenantContext.TenantSlot, TierSlot, OutcomeSlot),
        InstrumentSpec.Count(EnergyExchanges, "{exchange}", "energy artifacts by leg and format",
            MeasureForm.Whole, TenantContext.TenantSlot, LegSlot, FormatSlot),
        InstrumentSpec.Count(EnergyWarnings, "{warning}", "energy exchange warning tallies per format",
            MeasureForm.Whole, TenantContext.TenantSlot, FormatSlot),
        InstrumentSpec.Count(TextureDrops, "{channel}", "appearance texture channels lost at binding, banded by cause",
            MeasureForm.Whole, TenantContext.TenantSlot, FormatSlot, ChannelSlot),
        InstrumentSpec.Count(ExchangeDegrades, "{degradation}", "exchange legs completing with shed capability, banded by lane and reason",
            MeasureForm.Whole, TenantContext.TenantSlot, LaneSlot, ReasonSlot));

    public static TelemetryContributorPort Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl) =>
        new(Scope: Scope, Version: version, Instruments: Rows, Planes: BimPoint.Scopes, SchemaUrl: schemaUrl);

    // Telemetry-as-tap: the whole projection mounts as observe subscriptions — zero emit calls inside a
    // projector, codec arm, or review fold. The evidence cell taps through its Atom Change event, so a veto
    // refusal and an isolated tap fault land on the rejects counter banded by point and kernel fault category.
    // The cursor claims the span BETWEEN the last counted length and the observed one inside a single CAS, so a
    // batched swap appending several faults counts several and two racing swaps never double-count the same
    // fault — reading held.Last counted exactly one per swap regardless of what the swap appended. The rejects
    // write DISCARDS its rail on purpose: parking a refusal would append to the very cell this handler observes
    // and re-enter Change without bound.
    public static Seq<IDisposable> Tap(BimHooks hooks, InstrumentSet set) {
        Atom<(int Counted, Seq<IsolatedFault> Fresh)> cursor = Atom((Counted: 0, Fresh: Seq<IsolatedFault>()));
        AtomChangedEvent<Seq<IsolatedFault>> rejected = held =>
            cursor.Swap(prior => (Counted: held.Count, Fresh: held.Skip(prior.Counted).Strict())).Fresh.Iter(fault =>
                ignore(set.Write(LegalityRejects, 1L,
                    InstrumentSet.Tags(TenantContext.Current, (PointSlot, fault.Point.ToString()), (KernelInstruments.CategorySlot, fault.Cause.Category)))));
        hooks.Faults.Change += rejected;
        IDisposable drained = new HookDetacher(Detach: () => hooks.Faults.Change -= rejected);
        return Subscriptions(hooks: hooks, set: set).Add(drained);
    }

    // Every arm hands the kernel write rail straight to the capsule's rail-shaped Observe, which lifts a refused
    // write onto the IO error channel and parks it point-attributed beside every other tap fault — a folder-local
    // Fin-to-IO adapter re-mints exactly that lift and adds nothing the capsule does not already own.
    static Seq<IDisposable> Subscriptions(BimHooks hooks, InstrumentSet set) => Seq(
        // The shared tag row is a PURE value, so it binds as a value ahead of the rail — a `Fin.Succ` query
        // head sequences an effect or captures a pre-mutation read, and neither is what a tag array is.
        hooks.Imported.Observe(fact =>
            InstrumentSet.Tags(TenantContext.Current, (FormatSlot, fact.Format), (CodecSlot, fact.Codec)) switch {
                var tags => set.Write(ImportDuration, fact.Elapsed.TotalSeconds, tags)
                    .Bind(_ => set.Write(ImportSize, fact.Bytes, tags))
                    .Bind(_ => set.Write(Instancing, (long)fact.Instances, tags)),
            }),
        hooks.Exported.Observe(fact =>
            set.Write(ExportDuration, fact.Elapsed.TotalSeconds, InstrumentSet.Tags(TenantContext.Current, (FormatSlot, fact.Format)))),
        hooks.Lowered.Observe(fact =>
            InstrumentSet.Tags(TenantContext.Current, (ProjectorSlot, fact.Projector)) switch {
                var tags => set.Write(ProjectionNodes, (long)fact.Nodes, tags)
                    .Bind(_ => set.Write(ProjectionEdges, (long)fact.Edges, tags)),
            }),
        // Verdict is the one Replay point, and the capsule fans its held window to a fresh subscriber on attach —
        // so this counter is mounted at composition, before the first fire, where the window is provably empty.
        hooks.Verdict.Observe(fact =>
            set.Write(ReviewVerdicts, 1L, InstrumentSet.Tags(TenantContext.Current, (TierSlot, fact.Tier), (OutcomeSlot, fact.Outcome)))),
        hooks.Emitted.Observe(fact =>
            set.Write(EnergyExchanges, 1L, InstrumentSet.Tags(TenantContext.Current, (LegSlot, fact.Leg), (FormatSlot, fact.Format)))
             .Bind(_ => set.Write(EnergyWarnings, (long)fact.Warnings, InstrumentSet.Tags(TenantContext.Current, (FormatSlot, fact.Format))))),
        // Only the LOSSES write. A bound channel is the artifact's own evidence and needs no counter; the
        // two loss causes write under one instrument banded by cause, so the drop total and its attribution
        // are one series rather than two that drift.
        hooks.Textured.Observe(fact =>
            set.Write(TextureDrops, (long)fact.Dropped,
                    InstrumentSet.Tags(TenantContext.Current, (FormatSlot, fact.Format), (ChannelSlot, "target-refused")))
                .Bind(_ => set.Write(TextureDrops, (long)fact.Unresolved,
                    InstrumentSet.Tags(TenantContext.Current, (FormatSlot, fact.Format), (ChannelSlot, "image-unresolved"))))),
        // The degradation counter bands on the fact's two CLOSED axes alone — the Subject the fact carries for a
        // reader is identifier-grade and never enters a series.
        hooks.ExchangeDegrade.Observe(fact =>
            set.Write(ExchangeDegrades, 1L, InstrumentSet.Tags(TenantContext.Current, (LaneSlot, fact.Lane), (ReasonSlot, fact.Reason)))));

    // Span wrapper every long-running Bim entry composes over the composing root's kernel band, which owns the
    // source, the listener gate, the ActivityKind.Internal open, and the fail-leg status verdict — this page
    // adds attribution alone. The band arrives NULLABLE for the same reason the hook slot arrives optional: a
    // headless or plugin composition admits no scope, and a null receiver runs the identical rail untraced
    // where a required band would force that composition to mint an ActivitySource its root never disposes.
    // Model identity is a REQUIRED parameter rather than a caller-chosen mark row: every Bim entry runs against
    // exactly one model, and a slot published for a caller to remember is a slot a caller forgets — which is
    // what left the key declared and never stamped. Further marks stay the caller's identifier-grade rows; each
    // stamps post-start, so no mark reaches the sampling verdict and a mark-less call still carries its model.
    // Exemption: a params span cannot cross a lambda, so materializing it is the one statement seam here.
    public static Fin<T> Traced<T>(
        SpanBand? band, BimPoint at, Op op, string model, Func<Fin<T>> body, params ReadOnlySpan<(string Slot, object? Value)> marks) {
        Seq<(string Slot, object? Value)> stamps = (ModelSlot, (object?)model).Cons(toSeq(marks.ToArray()));
        return band is null
            ? body()
            : band.Traced(at.Plane, op, span => (Stamped(span, stamps), body()).Item2);
    }

    // Span attribution takes marks alone: the app root's baggage promotion already stamps rasm.tenant on every
    // span, so folding the metric plane's Tagged here would double-stamp the partition.
    static Unit Stamped(Activity? span, Seq<(string Slot, object? Value)> marks) =>
        ignore(marks.Iter(mark => ignore(span?.SetTag(mark.Slot, mark.Value))));
}
```

## [04]-[BENCH_RECEIPTS]

- Owner: `BimBenchClaims` the folder claim roster — `static readonly` kernel `BenchClaim` rows per the kernel law that claim rows live BESIDE the lanes they gate on their owning pages; every Rasm.Bim performance claim names its row and a folder-local claim type is the deleted form. `BimBenchReceipt` the typed run evidence a bench run mints per claim.
- Cases: claim rows — `ImportGlb`, `ImportIfc`, `ImportDwg`, `ImportPly`, `ImportScene`, `ImportUsd`, `ImportDotbim` (foreign-bytes decode per `BimIo` codec arm), `EgressReauthor` (IFC re-author over an admitted graph), `QueryMedium`/`QueryLarge` (element-set predicate folds at the two corpus graph scales), `GeoVectorRead`/`GeoRasterRead` (geospatial-seam ingest), `TessellationRoundTrip` (tessellation-bridge companion round trip) — each row carrying its `Corpus` slug, the estate corpus artifact whose content fingerprint the receipt stamps. A row name never equals its lane owner's type name, so the `nameof` derivation resolves the type rather than the field beside it.
- Entry: the bench project constructs `BimBenchReceipt` rows at its edge — one per claim per run — and the corpus-gate admission row below is the ONE path a receipt becomes a standing claim.
- Auto: `CorpusFingerprint` derives through the one kernel content hasher over the corpus artifact bytes, so a claim binds to the exact input it measured and a corpus revision invalidates every dependent claim structurally, never by prose; a corpus-bound claim discharges `BenchLedger.Unproven` only through a proof pair whose fingerprint is present.
- Receipt: `BimBenchReceipt` — claim, corpus fingerprint, median / p95 / interquartile wall duration, allocated bytes, operation count, instant; the column set IS the app-tier `BenchMeasurement` carrier the gate fold consumes, so a receipt lands whole rather than defaulting a spread the gate reads. Distribution truth, no verdict field — judging is the gate fold's, not the receipt's.
- Packages: LanguageExt.Core, NodaTime, Rasm, BCL inbox.
- Growth: a new measured operation is one `BenchClaim` row on `BimBenchClaims`; a new measured axis is one field on the receipt breaking the gate mapping at compile time.
- Boundary: corpus-gate admission — a speed or allocation claim on any Rasm.Bim page resolves to a `BimBenchReceipt` the estate BenchmarkDotNet corpus gate stamped: the branch bench project folds each receipt into the app-tier benchmark envelope (suite `rasm.bim`, case the claim key) and the AppHost `BenchmarkGate.Judge` fold owns pass-or-regress under the host-evidence and budget law; BenchmarkDotNet binds in the branch test and benchmark projects per the Test Stack manifest tier, never `Rasm.Bim.csproj`, so no benchmark type crosses into this package; a hand-rolled kernel is admitted only after its receipt defeats the library route under that gate.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// Folder claim roster: kernel BenchClaim rows, one per measured operation, per the kernel law that
// claim rows live beside the lanes they gate. Every row is a corpus-REGRESSION claim — the measured lane is
// judged against its own prior stamped receipt on the same corpus — so Regression folds the kernel row's two
// lane columns onto ONE spelling at the no-regression 1.0 floor, and the spelling is nameof-DERIVED at the
// measured member so a rename breaks this roster at compile time where a literal strands the gate against a
// lane it can no longer bind. The Corpus column DECLARES the fixture roster — the tests-estate benchmark
// corpus manifest realizes each slug as its CorpusEntry.RelativePath row under the corpus BENCHMARK_CLAIM
// contract, and the receipt stamps the MEASURED CorpusEntry.Key at run — so no fingerprint pins here, a
// divergent realization fails the corpus-gate admission rather than this page, and the declaration is the
// authority the manifest transcribes, never the reverse.
public static class BimBenchClaims {
    private static readonly string Decode = $"{nameof(BimIo)}.{nameof(BimIo.ImportGeometry)}";
    private static readonly string DecodeIfc = $"{nameof(BimIo)}.{nameof(BimIo.ImportIfc)}";
    private static readonly string Reauthor = $"{nameof(BimExport)}.{nameof(BimExport.ExportIfc)}";
    private static readonly string Select = $"{nameof(ElementSet)}.{nameof(ElementSet.Query)}";
    private static readonly string Vector = $"{nameof(GeoVector)}.{nameof(GeoVector.Read)}";
    private static readonly string Raster = $"{nameof(GeoRaster)}.{nameof(GeoRaster.Read)}";
    private static readonly string Tessellate = $"{nameof(TessellationRequest)}.{nameof(TessellationRequest.Plan)}";

    public static readonly BenchClaim ImportGlb = Regression("import-glb", Decode, "corpus-scene-glb");
    public static readonly BenchClaim ImportIfc = Regression("import-ifc", DecodeIfc, "corpus-model-ifc");
    public static readonly BenchClaim ImportDwg = Regression("import-dwg", Decode, "corpus-drawing-dwg");
    public static readonly BenchClaim ImportPly = Regression("import-ply", Decode, "corpus-mesh-ply");
    public static readonly BenchClaim ImportScene = Regression("import-scene", Decode, "corpus-scene-fbx");
    public static readonly BenchClaim ImportUsd = Regression("import-usd", Decode, "corpus-stage-usd");
    public static readonly BenchClaim ImportDotbim = Regression("import-dotbim", Decode, "corpus-model-bim");
    public static readonly BenchClaim EgressReauthor = Regression("egress-reauthor", Reauthor, "corpus-model-ifc");
    public static readonly BenchClaim QueryMedium = Regression("query-medium", Select, "corpus-graph-100k");
    public static readonly BenchClaim QueryLarge = Regression("query-large", Select, "corpus-graph-1m");
    public static readonly BenchClaim GeoVectorRead = Regression("geo-vector", Vector, "corpus-geo-gpkg");
    public static readonly BenchClaim GeoRasterRead = Regression("geo-raster", Raster, "corpus-geo-cog");
    public static readonly BenchClaim TessellationRoundTrip = Regression("tessellation-roundtrip", Tessellate, "corpus-model-ifc");

    private static BenchClaim Regression(string claim, string lane, string corpus) =>
        new(Op.Of(name: claim), lane, lane, 1.0, Some(corpus));
}

// --- [MODELS] -----------------------------------------------------------------------------
// Distribution truth per claim per run: the verdict lives on the app-tier gate fold, never here; the corpus
// fingerprint binds the claim to the exact measured input through the one kernel content hasher and is the
// presence witness the kernel BenchLedger.Unproven proof pair reads.
public sealed record BimBenchReceipt(
    BenchClaim Claim,
    UInt128 CorpusFingerprint,
    Duration Median,
    Duration P95,
    Duration Iqr,
    long AllocatedBytes,
    long Operations,
    Instant At);
```

## [05]-[RESEARCH]

(none)
