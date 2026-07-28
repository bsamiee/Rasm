# [BIM_OBSERVABILITY]

Composition-scoped observability for the BIM-and-exchange engine: `BimHooks` composes the kernel signal capsule into the closed `rasm.bim.<domain>.<point>` roster `BimPoint` declares, `BimTelemetry` projects typed receipts onto `rasm.bim.<domain>.<measure>` instruments as a registry subscriber — domain code fires facts and observability projects them, zero emit calls inside a projector or codec arm — and `BimBenchReceipt` closes the evidence loop: every Rasm.Bim performance claim is a typed, corpus-gated receipt, never a prose number.

Wire posture: HOST-LOCAL, BCL-only. Point, instrument-spec, advice-bucket, contributor-port, package-identity, tenancy-frame, fault-category, and trace-band machinery arrives settled from the kernel signal capsule, so no OpenTelemetry package is reachable here; SDK composition, exporters, exemplar policy, views, and cardinality caps stay at the app roots, which admit the `Rasm.Bim` meter by name and `BimPoint.Scopes` into their one `SpanBand`. Subscriber failure parks point-attributed on the composition's evidence cell, the emitter untouched.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: `BimPoint` closes the eight-row point vocabulary on its kernel `Modality` column and its derived `TraceScope` plane, `BimHooks` mints that roster as one per-composition registry record over the kernel point capsule, and `BimFact` closes the payload family every point types over.
- [03]-[TELEMETRY_TAP]: `BimTelemetry` declares the `rasm.bim.<domain>.<measure>` roster as kernel `InstrumentSpec` rows, mints the contributor port, rails the tap subscriptions, and owns the span and attribution law over the kernel `SpanBand`.
- [04]-[BENCH_RECEIPTS]: `BimBenchClaim` rosters the per-op claims, `BimBenchReceipt` carries the run evidence, and the corpus-gate row admits a claim as standing.

## [02]-[HOOK_RAIL]

- Owner: `BimPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.bim.<domain>.<point>` with the kernel `HookModality` column and the kernel `TraceScope` plane derived off the id's own head; `BimHooks` the per-composition registry record — one instance per app composition, so two apps built on the library never fight over hook slots and no process-global registry exists; `BimFact` the closed payload family every point types over. Point capsule, modality rows, detacher, and isolation are the kernel signal capsule composed as settled vocabulary.
- Cases: point roster rows — `rasm.bim.exchange.progress` (observe, `BimFact.Progress` — the ACadSharp `ICadReader.OnProgress` stage stream on the DWG/DXF decode arm), `rasm.bim.exchange.imported` (observe, `BimFact.Imported` — the `ModelLoad` receipt fact post-decode), `rasm.bim.exchange.exported` (observe, `BimFact.Exported` — the export-rail artifact emit), `rasm.bim.projection.lowered` (observe, `BimFact.Lowered` — the seam `GraphDelta` magnitude off the semantic projector), `rasm.bim.projection.legality` (veto, `BimFact.Admission` — an app policy refuses an emit before the IFC egress authors it), `rasm.bim.review.verdict` (replay, `BimFact.Verdict` — IDS-facet and template-audit outcomes, buffered so a late panel drains the recent window), `rasm.bim.energy.progress` (observe, `BimFact.Progress` — the OpenStudio `ProgressBar.onPercentageUpdated` percentage stream on the energy translate rows), `rasm.bim.energy.emitted` (observe, `BimFact.Emitted` — the `EnergyReceipt` fact per artifact).
- Entry: `BimHooks.Live()` mints the roster once at composition by seating one kernel point per `BimPoint` row; an emitting page fires its declared point value (`hooks.Imported.Fire(fact)`), so a name-resolved lookup surface never exists; `Veto`, `Observe`, and `Drain` are the capsule's subscriber entries, each returning the disposable detacher; `Points` hands the point set to `HookRegistry.Mount` at the app root, and `BimPoint.Scopes` enters that root's `SpanBand.Of(version, scopes)`.
- Auto: fire order is the capsule's law — retention first, the veto fold second (the first refusal is the emitter's verdict AND parks on the evidence cell), observe taps forked and shielded last, so `Fire` returns without waiting on any tap; `BimIo.ImportGeometry` and `EnergyTranslate.Run` take `Option<BimHooks> hooks = default` — the optional slot every later fire-site entry repeats — so a hook-less composition pays one `IsNone` test and a fired point with zero subscribers costs one empty fold.
- Receipt: a hook fire is the evidence event itself — the emitter's typed receipt already carries the fact, so a point mints nothing; the `Faults` cell (`Atom<Seq<IsolatedFault>>`) is the one registry evidence surface — veto refusals and shielded tap faults, point-attributed — drained by the composing app and projected onto the `[03]` rejects counter through the cell's `Change` tap.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm, BCL inbox.
- Growth: a new point is one `BimPoint` row, one `BimHooks` field with its `Live()` seat, and one `BimFact` case, and a point on a new domain segment arms its span plane with no roster edit because the plane derives off the id; a new subscriber is one `Observe`/`Veto` call at composition; delivery semantics are the kernel modality rows.
- Boundary: point ids compose the kernel `HookId` grammar with the package segment pinned `bim`, so a Bim point joins any app-tier registry census unrenamed — Bim declares its points here and the composing app subscribes direct; ids and modalities live on the roster rows alone, so a `Live()` seat re-spelling either is the forked-vocabulary defect; the fire path is the one synchronous shape, and an effect-rail caller lifts `Fire` at its own composition seam; the payload closes at declaration — every `BimHooks` field types its point to one `BimFact` case, so a stringly payload cannot enter the rail; telemetry is a tap, never a producer — `[03]-[TELEMETRY_TAP]` subscribes as observe rows here, and a subscriber that must never lose an event is a durable outbox consumer, not a hook subscriber.

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
    public static readonly BimPoint Verdict = new("rasm.bim.review.verdict", modality: HookModality.Replay);
    public static readonly BimPoint EnergyProgress = new("rasm.bim.energy.progress", modality: HookModality.Observe);
    public static readonly BimPoint Emitted = new("rasm.bim.energy.emitted", modality: HookModality.Observe);

    // Items-derived index materializes on first read, so a bracket pays a lookup rather than re-parsing the id.
    static readonly Lazy<FrozenDictionary<BimPoint, TraceScope>> Planes = new(
        static () => Items.ToFrozenDictionary(static row => row, static row =>
            TraceScope.Create(value: string.Join('.', row.Key.Split('.')[..3]))),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public HookModality Modality { get; }

    public TraceScope Plane => Planes.Value[this];

    // Composing roots admit this roster into SpanBand.Of; eight points share four planes, so the projection
    // deduplicates, and an unadmitted scope refuses on the kernel rail rather than dropping every span silently.
    public static Seq<TraceScope> Scopes => toSeq(Planes.Value.Values).Distinct().Strict();
}

// --- [MODELS] -----------------------------------------------------------------------------
// One closed payload family every hook point types over: one Op-keyed case per fact shape, so a point's
// fact type is a case and the tap reads typed evidence. Format, codec, leg, tier, and outcome slots carry
// each CLOSED vocabulary KEY the firing page projects down (InterchangeFormat.Key, EnergyLeg key, verdict
// row key) — so the S0 Model stratum consumes no Exchange/Energy/Review sibling type, and tag cardinality
// stays bounded because every key originates in a closed vocabulary at the fire site.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BimFact {
    private BimFact(Op key) => Key = key;

    public Op Key { get; }

    public sealed record Progress(Op Key, string Domain, string Stage, Option<double> Fraction) : BimFact(Key);
    public sealed record Imported(Op Key, string Format, string Codec, long Bytes, int Blocks, int Instances, Duration Elapsed) : BimFact(Key);
    public sealed record Exported(Op Key, string Format, long Bytes, Duration Elapsed) : BimFact(Key);
    public sealed record Lowered(Op Key, string Projector, int Nodes, int Edges) : BimFact(Key);
    public sealed record Admission(Op Key, GraphDelta Delta) : BimFact(Key);
    public sealed record Verdict(Op Key, string Tier, string Outcome, int Findings) : BimFact(Key);
    public sealed record Emitted(Op Key, string Leg, string Format, int Warnings) : BimFact(Key);
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
    HookPoint<BimFact.Verdict> Verdict,
    HookPoint<BimFact.Progress> EnergyProgress,
    HookPoint<BimFact.Emitted> Emitted,
    Atom<Seq<IsolatedFault>> Faults) {
    public static BimHooks Live() {
        Atom<Seq<IsolatedFault>> faults = Atom(Seq<IsolatedFault>());
        return new(
            Seat<BimFact.Progress>(BimPoint.ExchangeProgress, faults),
            Seat<BimFact.Imported>(BimPoint.Imported, faults),
            Seat<BimFact.Exported>(BimPoint.Exported, faults),
            Seat<BimFact.Lowered>(BimPoint.Lowered, faults),
            Seat<BimFact.Admission>(BimPoint.Legality, faults),
            Seat<BimFact.Verdict>(BimPoint.Verdict, faults),
            Seat<BimFact.Progress>(BimPoint.EnergyProgress, faults),
            Seat<BimFact.Emitted>(BimPoint.Emitted, faults),
            faults);
    }

    // Mount table the app root audits every registered point through.
    public Seq<IHookPoint> Points =>
        Seq<IHookPoint>(ExchangeProgress, Imported, Exported, Lowered, Legality, Verdict, EnergyProgress, Emitted);

    static HookPoint<TFact> Seat<TFact>(BimPoint row, Atom<Seq<IsolatedFault>> faults) =>
        new(id: HookId.Create(value: row.Key), modality: row.Modality, faults: faults);
}
```

## [03]-[TELEMETRY_TAP]

- Owner: `BimTelemetry` the one roster and projection owner — receipts stay billing truth, instruments are the lossy dashboard channel projected from them; rows are kernel `InstrumentSpec` declarations carrying kind, measurement form, and their closed `Dimensions` columns, advice bounds read the kernel `Buckets` holder, and the write capsule is the kernel `InstrumentSet`.
- Cases: projection map — `rasm.bim.exchange.import.duration`/`import.size`/`instancing` off `BimFact.Imported` (duration, payload size, and instance placements sharing one evidence read), `rasm.bim.exchange.export.duration` off `BimFact.Exported`, `rasm.bim.projection.nodes`/`edges` off `BimFact.Lowered`, `rasm.bim.legality.rejects` off the `Faults` evidence cell through its `Change` tap (veto refusals and hook-tap isolations, banded by point and kernel fault category), `rasm.bim.review.verdicts` off `BimFact.Verdict`, `rasm.bim.energy.exchanges`/`warnings` off `BimFact.Emitted`.
- Entry: `BimTelemetry.Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl)` — the string-scoped contributor port the composing root materializes, scope the kernel `TelemetrySource.Bim` identity and the semconv coordinate defaulting to the kernel pin; a root outside that fan binds `InstrumentSet.Of(cells, (meter, Rows))` directly against its own minted meter, never both. `BimTelemetry.Tap(BimHooks hooks, InstrumentSet set)` mounts the observe subscriptions at composition, so declaration and write calls live only inside this spine. `BimTelemetry.Traced<T>(SpanBand? band, BimPoint at, Op op, string model, Func<Fin<T>> body, params ReadOnlySpan<(string Slot, object? Value)> marks)` is the span wrapper every long-running entry composes over the composing root's kernel band, model identity a required argument so no Bim span exists unattributed and the band nullable so a composition admitting no scope runs the identical rail untraced.
- Auto: every advised row ships its explicit-bucket bounds through the kernel's `InstrumentKind` x `MeasureForm` derivation, so this page names a bound row and never a create call; instrument identity de-duplicates by name inside the meter, so name, unit, kind, and description are declaration facts this roster carries once; tag values ride the typed fact's own vocabulary keys, never free text.
- Receipt: none — the tap projects receipts and hook facts; a metric minted beside it is a second truth. Every arm returns the kernel write rail and subscribes through the capsule's own rail-shaped `Observe`, which lifts the refusal and parks it point-attributed, so a folder-local rail-to-effect adapter has nothing to add; the rejects counter alone discards, because its park re-enters the cell it observes.
- Packages: LanguageExt.Core, NodaTime, Rasm (the kernel instrument mechanism, the scope identity roster, the fault-category slot, the tenancy frame, and the trace band), BCL inbox.
- Growth: a new projected fact is one `InstrumentSpec` roster row with its `Dimensions` and one `Tap` subscription arm; every row therefore declares the kernel tenant slot beside its fact dimensions, while a new span dimension is one slot row here and one `marks` pair at the composing entry; a per-vocabulary instrument family derives from its owning vocabulary rows, never hand-enumerated names.
- Boundary: library altitude holds zero OpenTelemetry reference and zero span custody — the meter reaches the process only through the composing root's mint, so provider disposal owns instrument lifetime, and the kernel `SpanBand` owns the one `ActivitySource` per admitted scope, its listener gate, its `using` close, and its typed fail-leg status, so this page declares `BimPoint.Scopes` and holds no source, no wrapper, and no disposable, and a composition holding no band runs untraced on the null receiver rather than minting one; instrument custody is one-per-composition — either the app fan materializes the `Telemetry` port or a root binds `InstrumentSet.Of` locally, never both; subscription law — `Tap` mounts at composition ahead of the first fire, because the capsule fans a `Replay` point's held window to each fresh subscriber and a late attach therefore re-counts that window onto the verdict counter; span law — the span name IS the kernel `Op` and the plane the point's own id head, so bracket and fact never name two scopes, and the typed verdict — not a tag — carries the error fact; attribution law — dimension slots carry this package's dotted `rasm.bim.<dimension>` namespace so a concept a sibling package also tags never collides, the fault-category band reads the kernel slot rather than re-declaring one, tenancy is the kernel `TenantContext` projection every metric write folds so this page holds no tenant key and no baggage read, and model identity is identifier-grade: it rides the span alone as `Traced`'s own required argument, because one metric series per model is unbounded cardinality no view cap recovers, no baggage writer exists to read it from, and a slot left to caller discipline is a slot no caller stamps; the span fold never re-stamps the tenant partition the app root's baggage promotion already carries; SDK composition, exporters, exemplars, views, and cardinality caps stay at the app roots.

```csharp signature
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
            MeasureForm.Whole, TenantContext.TenantSlot, FormatSlot));

    public static TelemetryContributorPort Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl) =>
        new(Scope: Scope, Version: version, Instruments: Rows, Planes: BimPoint.Scopes, SchemaUrl: schemaUrl);

    // Telemetry-as-tap: the whole projection mounts as observe subscriptions — zero emit calls inside a
    // projector, codec arm, or review fold. The evidence cell taps through its Atom Change event (synchronous,
    // one appended fault per swap), so a veto refusal and an isolated tap fault land on the rejects counter
    // banded by point and kernel fault category. The rejects write DISCARDS its rail on purpose: parking a
    // refusal would append to the very cell this handler observes and re-enter Change without bound.
    public static Seq<IDisposable> Tap(BimHooks hooks, InstrumentSet set) {
        AtomChangedEvent<Seq<IsolatedFault>> rejected = held => held.Last.Iter(fault =>
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
             .Bind(_ => set.Write(EnergyWarnings, (long)fact.Warnings, InstrumentSet.Tags(TenantContext.Current, (FormatSlot, fact.Format))))));

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

- Owner: `BimBenchClaim` the `[SmartEnum<string>]` per-op claim roster — every Rasm.Bim performance claim names its row; `BimBenchReceipt` the typed run evidence a bench run mints per claim.
- Cases: claim rows — `ImportGlb`, `ImportIfc`, `ImportDwg`, `ImportPly`, `ImportScene`, `ImportUsd`, `ImportDotbim` (foreign-bytes decode per codec arm), `EgressReauthor` (IFC re-author over an admitted graph), `QueryMedium`/`QueryLarge` (element-set predicate folds at the two corpus graph scales), `GeoVector`/`GeoRaster` (geospatial-seam ingest), `TessellationRoundTrip` (tessellation-bridge companion round trip) — each row carrying its `Corpus` column, the estate corpus artifact slug whose content fingerprint the receipt stamps.
- Entry: the bench project constructs `BimBenchReceipt` rows at its edge — one per claim per run — and the corpus-gate admission row below is the ONE path a receipt becomes a standing claim.
- Auto: `CorpusFingerprint` derives through the one kernel content hasher over the corpus artifact bytes, so a claim binds to the exact input it measured and a corpus revision invalidates every dependent claim structurally, never by prose.
- Receipt: `BimBenchReceipt` — claim, corpus fingerprint, median and p95 wall duration, allocated bytes, operation count, instant; distribution truth, no verdict field — judging is the gate fold's, not the receipt's.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm, BCL inbox.
- Growth: a new measured operation is one `BimBenchClaim` row; a new measured axis is one field on the receipt breaking the gate mapping at compile time.
- Boundary: corpus-gate admission — a speed or allocation claim on any Rasm.Bim page resolves to a `BimBenchReceipt` the estate BenchmarkDotNet corpus gate stamped: the branch bench project folds each receipt into the app-tier benchmark envelope (suite `rasm.bim`, case the claim key) and the AppHost `BenchmarkGate.Judge` fold owns pass-or-regress under the host-evidence and budget law; BenchmarkDotNet binds in the branch test and benchmark projects per the Test Stack manifest tier, never `Rasm.Bim.csproj`, so no benchmark type crosses into this package; a hand-rolled kernel is admitted only after its receipt defeats the library route under that gate.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Per-op claim roster: every Rasm.Bim performance claim names its row; Corpus is the estate corpus
// artifact slug whose content fingerprint the receipt stamps.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimBenchClaim {
    public static readonly BimBenchClaim ImportGlb = new("import-glb", corpus: "corpus-scene-glb");
    public static readonly BimBenchClaim ImportIfc = new("import-ifc", corpus: "corpus-model-ifc");
    public static readonly BimBenchClaim ImportDwg = new("import-dwg", corpus: "corpus-drawing-dwg");
    public static readonly BimBenchClaim ImportPly = new("import-ply", corpus: "corpus-mesh-ply");
    public static readonly BimBenchClaim ImportScene = new("import-scene", corpus: "corpus-scene-fbx");
    public static readonly BimBenchClaim ImportUsd = new("import-usd", corpus: "corpus-stage-usd");
    public static readonly BimBenchClaim ImportDotbim = new("import-dotbim", corpus: "corpus-model-bim");
    public static readonly BimBenchClaim EgressReauthor = new("egress-reauthor", corpus: "corpus-model-ifc");
    public static readonly BimBenchClaim QueryMedium = new("query-medium", corpus: "corpus-graph-100k");
    public static readonly BimBenchClaim QueryLarge = new("query-large", corpus: "corpus-graph-1m");
    public static readonly BimBenchClaim GeoVector = new("geo-vector", corpus: "corpus-geo-gpkg");
    public static readonly BimBenchClaim GeoRaster = new("geo-raster", corpus: "corpus-geo-cog");
    public static readonly BimBenchClaim TessellationRoundTrip = new("tessellation-roundtrip", corpus: "corpus-model-ifc");

    public string Corpus { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// Distribution truth per claim per run: the verdict lives on the app-tier gate fold, never here; the corpus
// fingerprint binds the claim to the exact measured input through the one kernel content hasher.
public sealed record BimBenchReceipt(
    BimBenchClaim Claim,
    UInt128 CorpusFingerprint,
    Duration Median,
    Duration P95,
    long AllocatedBytes,
    long Operations,
    Instant At);
```

## [05]-[RESEARCH]

- [CORPUS_ARTIFACT_SLUGS]-[OPEN]: which corpus-manifest rows ground the `BimBenchClaim.Corpus` slugs and the content-fingerprint derivation over the artifact bytes; verify each slug against the `tests/csharp/_benchmarks/` corpus manifest when it lands and re-anchor any divergent row here.
