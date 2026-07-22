# [BIM_OBSERVABILITY]

Composition-scoped observability for the BIM-and-exchange engine: `BimHooks` composes the kernel signal capsule into the closed `rasm.bim.<domain>.<point>` roster, `BimTelemetry` projects typed receipts onto `rasm.bim.<domain>.<measure>` instruments as a registry subscriber — domain code fires facts and observability projects them, zero emit calls inside a projector or codec arm — and `BimBenchReceipt` closes the evidence loop: every Rasm.Bim performance claim is a typed, corpus-gated receipt, never a prose number.

Wire posture: HOST-LOCAL, BCL-only. Point, modality, detacher, isolation, instrument-row, advice-bucket, contributor-port, and identity-mint machinery arrives settled from the kernel signal capsule, so no OpenTelemetry package is reachable from this folder; SDK composition, exporters, exemplar policy, views, and cardinality caps stay at the app composition roots. Subscriber failure parks point-attributed on the composition's evidence cell, the emitter untouched.

## [01]-[INDEX]

- [01]-[HOOK_RAIL]: the `rasm.bim.<domain>.<point>` point roster, the `BimFact` payload family, and the per-composition `BimHooks` registry record over the kernel point capsule.
- [02]-[TELEMETRY_TAP]: the `rasm.bim.<domain>.<measure>` instrument roster, the mount and contributor-port mints, the tap subscriptions, and the span and attribution law.
- [03]-[BENCH_RECEIPTS]: the `BimBenchClaim` per-op claim roster, the `BimBenchReceipt` evidence record, and the corpus-gate admission row.

## [02]-[HOOK_RAIL]

- Owner: `BimHooks` the per-composition registry record — one instance per app composition, so two apps built on the library never fight over hook slots and no process-global registry exists; `BimFact` the closed payload family every point types over. Point capsule, modality rows, detacher, and isolation are the kernel signal capsule composed as settled vocabulary.
- Cases: point roster rows — `rasm.bim.exchange.progress` (observe, `BimFact.Progress` — the ACadSharp `ICadReader.OnProgress` stage stream on the DWG/DXF decode arm), `rasm.bim.exchange.imported` (observe, `BimFact.Imported` — the `ModelLoad` receipt fact post-decode), `rasm.bim.exchange.exported` (observe, `BimFact.Exported` — the export-rail artifact emit), `rasm.bim.projection.lowered` (observe, `BimFact.Lowered` — the seam `GraphDelta` magnitude off the semantic projector), `rasm.bim.projection.legality` (veto, `BimFact.Admission` — an app policy refuses an emit before the IFC egress authors it), `rasm.bim.review.verdict` (replay, `BimFact.Verdict` — IDS-facet and template-audit outcomes, buffered so a late panel drains the recent window), `rasm.bim.energy.progress` (observe, `BimFact.Progress` — the OpenStudio `ProgressBar.onPercentageUpdated` percentage stream on the energy translate rows), `rasm.bim.energy.emitted` (observe, `BimFact.Emitted` — the `EnergyReceipt` fact per artifact).
- Entry: `BimHooks.Live()` mints the roster once at composition; an emitting page fires its declared point value (`hooks.Imported.Fire(fact)`), so a name-resolved lookup surface never exists; `Veto`, `Observe`, and `Drain` are the capsule's subscriber entries, each returning the disposable detacher.
- Auto: fire order is the capsule's law — retention first, the veto fold second (the first refusal is the emitter's verdict AND parks on the evidence cell), observe taps forked and shielded last, so `Fire` returns without waiting on any tap; `BimIo.ImportGeometry` and `EnergyTranslate.Run` take `Option<BimHooks> hooks = default` — the optional slot every later fire-site entry repeats — so a hook-less composition pays one `IsNone` test and a fired point with zero subscribers costs one empty fold.
- Receipt: a hook fire is the evidence event itself — the emitter's typed receipt already carries the fact, so a point mints nothing; the `Faults` cell (`Atom<Seq<IsolatedFault>>`) is the one registry evidence surface — veto refusals and shielded tap faults, point-attributed — drained by the composing app and projected onto the `[02]` rejects counter through the cell's `Change` tap.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm, BCL inbox.
- Growth: a new point is one `BimHooks` field, one `Live()` row, and one `BimFact` case; a new subscriber is one `Observe`/`Veto` call at composition; delivery semantics are the kernel modality rows.
- Boundary: point ids compose the kernel `HookId` grammar with the package segment pinned `bim`, so a Bim point joins any app-tier registry census unrenamed — Bim declares its points here and the composing app subscribes direct; the fire path is the one synchronous shape, and an effect-rail caller lifts `Fire` at its own composition seam; the payload closes at declaration — every `BimHooks` field types its point to one `BimFact` case, so a stringly payload cannot enter the rail; telemetry is a tap, never a producer — `[02]-[TELEMETRY_TAP]` subscribes as observe rows here, and a subscriber that must never lose an event is a durable outbox consumer, not a hook subscriber.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Model;

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
// declared field, one shared evidence cell per composition — the kernel capsule minted per point row.
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
            new(HookId.Create("rasm.bim.exchange.progress"), HookModality.Observe, faults),
            new(HookId.Create("rasm.bim.exchange.imported"), HookModality.Observe, faults),
            new(HookId.Create("rasm.bim.exchange.exported"), HookModality.Observe, faults),
            new(HookId.Create("rasm.bim.projection.lowered"), HookModality.Observe, faults),
            new(HookId.Create("rasm.bim.projection.legality"), HookModality.Veto, faults),
            new(HookId.Create("rasm.bim.review.verdict"), HookModality.Replay, faults),
            new(HookId.Create("rasm.bim.energy.progress"), HookModality.Observe, faults),
            new(HookId.Create("rasm.bim.energy.emitted"), HookModality.Observe, faults),
            faults);
    }
}
```

## [03]-[TELEMETRY_TAP]

- Owner: `BimTelemetry` the one roster and projection owner — receipts stay billing truth, instruments are the lossy dashboard channel projected from them; rows are kernel `InstrumentRow` declarations with their closed `Dimensions` columns, advice bounds read the kernel `Buckets` holder, and the write capsule is the kernel `InstrumentSet`.
- Cases: projection map — `rasm.bim.exchange.import.duration`/`import.bytes`/`instancing` off `BimFact.Imported` (duration, bytes, and instance placements sharing one evidence read), `rasm.bim.exchange.export.duration` off `BimFact.Exported`, `rasm.bim.projection.nodes`/`edges` off `BimFact.Lowered`, `rasm.bim.legality.rejects` off the `Faults` evidence cell through its `Change` tap (veto refusals and hook-tap isolations, banded by point and fault category), `rasm.bim.review.verdicts` off `BimFact.Verdict`, `rasm.bim.energy.exchanges`/`warnings` off `BimFact.Emitted`.
- Entry: `BimTelemetry.Mount(IMeterFactory factory, string version, string schemaUrl)` — one kernel identity mint returning the `(ActivitySource, InstrumentSet)` pair, scope the package id `Rasm.Bim`, the semconv coordinate stamped as `MeterOptions.TelemetrySchemaUrl`; `BimTelemetry.Telemetry(string version, string schemaUrl)` — the string-scoped contributor port an app fan materializes instead; `BimTelemetry.Tap(BimHooks hooks, InstrumentSet set)` mounts the observe subscriptions at composition, so create and write calls live only inside this spine; `BimTelemetry.Traced(ActivitySource source, Op op, Func<Fin<T>> body)` the span wrapper every long-running entry composes over the minted source half.
- Auto: every histogram row ships its explicit-bucket advice at creation through the one kernel `Advised<T>` bind; instrument identity de-duplicates by name inside the meter, so name, unit, and description are declaration facts this roster carries once; tag values ride the typed fact's own vocabulary keys, never free text.
- Receipt: none — the tap is a projection of receipts and hook facts; a metric minted beside it is a second truth.
- Packages: LanguageExt.Core, NodaTime, Rasm, BCL inbox.
- Growth: a new projected fact is one roster row through `Dimensions` and one `Tap` subscription arm; every row therefore declares the common `tenant`/`model` baggage dimensions beside its fact dimensions, while a new span dimension is one tag row at activity start; a per-vocabulary instrument family derives from its owning vocabulary rows, never hand-enumerated names.
- Boundary: library altitude holds zero OpenTelemetry reference — the meter reaches the process only through the injected `IMeterFactory` via the kernel mint, so provider disposal owns instrument lifetime; instrument custody is one-per-composition — either `Mount` materializes the rows or an app fan materializes the `Telemetry` port, never both; span law — span names derive from the kernel `Op` (`op.ToString()`, never a fresh literal), `HasListeners()` gates the wrapper ahead of the call, `ActivityKind.Internal` states the in-process boundary, and a failing rail lands `SetStatus(ActivityStatusCode.Error, message)` so the typed verdict — not a tag — carries the error fact; baggage law — every instrument write appends `tenant` and `model` tags read once per fact from `Activity.Current?.GetBaggageItem` under the declared envelope keys `rasm.tenant`/`rasm.model` (an absent key omits its tag, so no empty-string series mints), and a tenant read never threads a parameter through a domain signature; SDK composition, exporters, exemplars, views, and cardinality caps stay at the app roots, which admit the `Rasm.Bim` meter and source by name.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BimTelemetry {
    public const string Scope = "Rasm.Bim";
    public const string TenantKey = "rasm.tenant";
    public const string ModelKey = "rasm.model";

    static Seq<string> Dimensions(params string[] fact) => toSeq(fact).Add("tenant").Add("model");

    public static readonly Seq<InstrumentRow> Rows = Seq(
        new InstrumentRow("rasm.bim.exchange.import.duration", "s", "foreign-bytes decode wall duration per format and codec",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.DecodeSeconds), Dimensions("format", "codec")),
        new InstrumentRow("rasm.bim.exchange.import.bytes", "By", "decoded source byte count per format",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.ByteSizes), Dimensions("format")),
        new InstrumentRow("rasm.bim.exchange.instancing", "{instance}", "instance placements per decoded pool",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.GraphCounts), Dimensions("format")),
        new InstrumentRow("rasm.bim.exchange.export.duration", "s", "artifact emit wall duration per format",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.DecodeSeconds), Dimensions("format")),
        new InstrumentRow("rasm.bim.projection.nodes", "{node}", "seam delta node magnitude per projector",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.GraphCounts), Dimensions("projector")),
        new InstrumentRow("rasm.bim.projection.edges", "{edge}", "seam delta edge magnitude per projector",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.GraphCounts), Dimensions("projector")),
        new InstrumentRow("rasm.bim.legality.rejects", "{reject}", "legality and hook rejections banded by point and fault category",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text), Dimensions("point", "category")),
        new InstrumentRow("rasm.bim.review.verdicts", "{verdict}", "review verdicts by tier and outcome",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text), Dimensions("tier", "outcome")),
        new InstrumentRow("rasm.bim.energy.exchanges", "{exchange}", "energy artifacts by leg and format",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text), Dimensions("leg", "format")),
        new InstrumentRow("rasm.bim.energy.warnings", "{warning}", "energy exchange warning tallies per format",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text), Dimensions("format")));

    // One materialization per composition: Mount binds the rows here or an app fan materializes the Telemetry port — never both.
    public static (ActivitySource Source, InstrumentSet Set) Mount(IMeterFactory factory, string version, string schemaUrl) {
        (ActivitySource source, Meter meter) = TelemetryIdentity.Mint(factory: factory, scope: Scope, version: version, schemaUrl: schemaUrl);
        return (source, InstrumentSet.Of(meter: meter, rows: Rows));
    }

    public static TelemetryContributorPort Telemetry(string version, string schemaUrl) =>
        new(Scope: Scope, Version: version, SchemaUrl: schemaUrl, Instruments: Rows);

    // Telemetry-as-tap: the whole projection mounts as observe subscriptions — zero emit calls inside a
    // projector, codec arm, or review fold; every write appends the baggage-sourced tenant/model tags. The
    // evidence cell taps through its Atom Change event (synchronous, one appended fault per swap), so a
    // veto refusal and a shielded tap fault land on the rejects counter banded by point and kernel Category.
    public static Seq<IDisposable> Tap(BimHooks hooks, InstrumentSet set) {
        AtomChangedEvent<Seq<IsolatedFault>> rejected = held => held.Last.Iter(fault =>
            ignore(set.Count("rasm.bim.legality.rejects", 1L,
                Attributed(("point", fault.Point.ToString()), ("category", fault.Cause.Category())))));
        hooks.Faults.Change += rejected;
        IDisposable drained = new HookDetacher(Detach: () => hooks.Faults.Change -= rejected);
        return Subscriptions(hooks: hooks, set: set).Add(drained);
    }

    static Seq<IDisposable> Subscriptions(BimHooks hooks, InstrumentSet set) => Seq(
        hooks.Imported.Observe(fact => IO.lift(() => {
            var tags = Attributed(("format", fact.Format), ("codec", fact.Codec));
            ignore(set.Record("rasm.bim.exchange.import.duration", fact.Elapsed.TotalSeconds, tags));
            ignore(set.Record("rasm.bim.exchange.import.bytes", fact.Bytes, tags));
            return set.Record("rasm.bim.exchange.instancing", (long)fact.Instances, tags);
        })),
        hooks.Exported.Observe(fact => IO.lift(() =>
            set.Record("rasm.bim.exchange.export.duration", fact.Elapsed.TotalSeconds, Attributed(("format", fact.Format))))),
        hooks.Lowered.Observe(fact => IO.lift(() => {
            var tags = Attributed(("projector", fact.Projector));
            ignore(set.Record("rasm.bim.projection.nodes", (long)fact.Nodes, tags));
            return set.Record("rasm.bim.projection.edges", (long)fact.Edges, tags);
        })),
        hooks.Verdict.Observe(fact => IO.lift(() =>
            set.Count("rasm.bim.review.verdicts", 1L, Attributed(("tier", fact.Tier), ("outcome", fact.Outcome))))),
        hooks.Emitted.Observe(fact => IO.lift(() => {
            ignore(set.Count("rasm.bim.energy.exchanges", 1L, Attributed(("leg", fact.Leg), ("format", fact.Format))));
            return set.Count("rasm.bim.energy.warnings", fact.Warnings, Attributed(("format", fact.Format)));
        })));

    // Span wrapper every long-running Bim entry composes over the minted source half: HasListeners gates the
    // whole wrapper, the span name IS the kernel Op, and a failing rail lands the typed Error verdict.
    public static Fin<T> Traced<T>(ActivitySource source, Op op, Func<Fin<T>> body) {
        if (!source.HasListeners()) { return body(); }
        using var activity = source.StartActivity(op.ToString(), ActivityKind.Internal);
        return body().MapFail(error => {
            ignore(activity?.SetStatus(ActivityStatusCode.Error, error.Message));
            return error;
        });
    }

    // Baggage-sourced attribution: tenant/model identifiers ride W3C baggage from the app-tier envelope seam;
    // an absent key omits its tag, so no empty-string series mints and no domain signature grows a tenant slot.
    static KeyValuePair<string, object?>[] Attributed(params ReadOnlySpan<(string Key, string Value)> pairs) {
        var held = new List<KeyValuePair<string, object?>>(pairs.Length + 2);
        foreach (var (key, value) in pairs) { held.Add(new(key, value)); }
        if (Activity.Current?.GetBaggageItem(TenantKey) is { Length: > 0 } tenant) { held.Add(new("tenant", tenant)); }
        if (Activity.Current?.GetBaggageItem(ModelKey) is { Length: > 0 } model) { held.Add(new("model", model)); }
        return [.. held];
    }
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

- [ENVELOPE_KEY_PROMOTION]-[OPEN]: which store does the app-tier envelope seam write the promoted `rasm.tenant`/`rasm.model` baggage keys through — the BCL `Activity` chain (the only store `Activity.Current.GetBaggageItem` reads) or the SDK `Baggage.Current` store — and do the promoted spellings match `TenantKey`/`ModelKey`; verify against the AppHost envelope declaration when its tenant-promotion row lands, re-anchoring both keys on divergence.
- [CORPUS_ARTIFACT_SLUGS]-[OPEN]: which corpus-manifest rows ground the `BimBenchClaim.Corpus` slugs and the content-fingerprint derivation over the artifact bytes; verify each slug against the `tests/csharp/_benchmarks/` corpus manifest when it lands and re-anchor any divergent row here.
