# [BIM_OBSERVABILITY]

Composition-scoped observability for the BIM-and-exchange engine: `BimHooks` is the per-composition hook registry — a closed point roster keyed `rasm.bim.<domain>.<point>` over the import/export codecs, the semantic projection, the legality gate, the review verdicts, and the energy translators — and `BimTelemetry` is the one meter owner projecting typed receipts onto `rasm.bim.<domain>.<measure>` instruments as a registry subscriber, so domain code fires facts and observability projects them with zero emit calls inside a projector or codec arm. `BimBenchReceipt` closes the evidence loop: every Rasm.Bim performance claim is a typed, corpus-gated receipt, never a prose number.

Wire posture: HOST-LOCAL, BCL-only. Emission composes `System.Diagnostics.Metrics` (`IMeterFactory`/`Meter`) and the BCL `ActivitySource` alone — no OpenTelemetry package is reachable from this folder; SDK composition, exporters, exemplar policy, views, and cardinality caps stay the `csharp:Rasm.AppHost/Observability` composition roots'. Faults route the `Model/faults#FAULT_BAND` arms — a modality-refused subscription `ModelRejected` (`hook-modality`), a throwing observe tap `CodecReject` (`hook-tap`) shielded into the registry evidence cell, the emitter untouched.

## [01]-[INDEX]

- [01]-[HOOK_RAIL]: the `rasm.bim.<domain>.<point>` point roster, the veto/observe/replay `BimHookModality` union, the `BimFact` payload family, the per-composition `BimHooks` registry record, and the subscriber-fault isolation law onto the `BimFault` rail.
- [02]-[TELEMETRY_TAP]: the `rasm.bim.<domain>.<measure>` instrument roster, the `BimTelemetry` meter owner over injected `IMeterFactory`, the `ActivitySource` span law, and the baggage-sourced tenant/model tag law.
- [03]-[BENCH_RECEIPTS]: the `BimBenchClaim` per-op claim roster, the `BimBenchReceipt` evidence record, and the corpus-gate admission row.

## [02]-[HOOK_RAIL]

- Owner: `BimHooks` the per-composition registry record — one instance per app composition, so two apps built on the library never fight over hook slots and no process-global registry exists; `BimHookPoint<TFact>` the typed point capsule over `IBimHookPoint`; `BimHookModality` the three delivery rows; `BimHookId` the grammar-admitted point name; `BimFact` the closed payload family every point types over.
- Cases: point roster rows — `rasm.bim.exchange.progress` (observe, `BimFact.Progress` — the ACadSharp `ICadReader.OnProgress` stage stream on the DWG/DXF decode arm, `Exchange/import#IMPORT_RAIL`), `rasm.bim.exchange.imported` (observe, `BimFact.Imported` — the `ModelLoad` receipt fact post-decode), `rasm.bim.exchange.exported` (observe, `BimFact.Exported` — the `Exchange/export#EXPORT_RAIL` artifact emit), `rasm.bim.projection.lowered` (observe, `BimFact.Lowered` — the seam `GraphDelta` magnitude off `Projection/semantic#SEMANTIC_PROJECTOR`), `rasm.bim.projection.legality` (veto, `BimFact.Admission` — an app policy refuses an emit before `Projection/egress#IFC_EGRESS` authors it), `rasm.bim.review.verdict` (replay, `BimFact.Verdict` — `Review/validation#IDS_FACETS` and template-audit outcomes, buffered so a late panel drains the recent window), `rasm.bim.energy.progress` (observe, `BimFact.Progress` — the OpenStudio `ProgressBar.onPercentageUpdated` percentage stream on the `Energy/derive#TRANSLATE_MATRIX` rows), `rasm.bim.energy.emitted` (observe, `BimFact.Emitted` — the `EnergyReceipt` fact per artifact); modality rows — `Veto` (synchronous transform-or-reject on the emitter's own rail), `Observe` (asynchronous tap), `Replay` (asynchronous tap with a bounded buffer late subscribers drain).
- Entry: `BimHooks.Live()` mints the roster once at composition; an emitting page fires its declared point value (`hooks.Imported.Fire(fact)`), so a name-resolved lookup surface never exists; `Veto`, `Observe`, and `Drain` are the subscriber entries, each returning the disposable detacher; `BimHookId.Validate` admits the `rasm.bim.<domain>.<point>` grammar — four dot-separated lowercase segments, `rasm` then `bim` fixed — so a malformed id is a typed refusal at declaration, never a silent registry miss.
- Auto: veto subscribers fold left in registration order through `Bind`, so the first refusal short-circuits with its typed fault on the emitter's rail AND folds into the registry's `Faults` evidence cell; observe taps run shielded — a throwing tap converts to `BimFault.CodecReject` (`hook-tap`) carrying the fact's kernel `Op` and folds into the same cell, the emitter's `Fire` result untouched; a replay point prunes its buffer to `depth` oldest-first on every fire, and a fresh subscriber drains the held window on attach; `BimIo.ImportGeometry` and `EnergyTranslate.Run` take `Option<BimHooks> hooks = default` — the optional slot every later fire-site entry repeats — so a hook-less composition pays one `IsNone` test and a fired point with zero subscribers costs one empty fold.
- Receipt: a hook fire is the evidence event itself — the emitter's typed receipt already carries the fact, so a point mints nothing; the `Faults` cell (`Atom<Seq<Error>>`) is the one registry evidence surface — veto refusals and shielded tap faults — drained by the composing app and projected onto the `[03]` rejects counter through the cell's `Change` tap.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm, BCL inbox.
- Growth: a new point is one `BimHooks` field, one `Live()` row, and one `BimFact` case; a new delivery semantics is one `BimHookModality` row breaking every modality dispatch at compile time; a new subscriber is one `Observe`/`Veto` call at composition.
- Boundary: the id grammar is the app-tier hook grammar with the package segment pinned `bim`, so a Bim point mounts unrenamed into an app-tier registry census (`csharp:Rasm.AppHost/Observability/hooks.md` `[04]-[HOOK_REGISTRY]` owns that census; Bim declares points, apps subscribe); `Synchronous` is the modality's own column, so an emitter reads its blocking obligation off the row it declared; the payload closes at declaration — `TFact : BimFact` bounds every point to the typed family, so a stringly payload cannot enter the rail; telemetry is a tap, never a producer — `[03]-[TELEMETRY_TAP]` subscribes as observe rows here, and a subscriber that must never lose an event is a durable outbox consumer, not a hook subscriber.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// Three delivery rows; Synchronous is the emitter's blocking obligation read off the row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimHookModality {
    public static readonly BimHookModality Veto = new("veto", synchronous: true);
    public static readonly BimHookModality Observe = new("observe", synchronous: false);
    public static readonly BimHookModality Replay = new("replay", synchronous: false);

    public bool Synchronous { get; }
}

// Point-name grammar rasm.bim.<domain>.<point> — the app-tier hook grammar with the package segment pinned
// bim, admitted once at declaration; a malformed id is a typed BimFault refusal, never a silent registry miss.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<BimFault>]
public readonly partial struct BimHookId {
    static partial void ValidateFactoryArguments(ref BimFault? validationError, ref string value) =>
        validationError = value.Split('.') is ["rasm", "bim", var domain, var point]
            && domain.Length > 0 && point.Length > 0
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-')
            ? null
            : BimFault.Create($"hook-id-malformed:{value}");
}

// --- [MODELS] -----------------------------------------------------------------------------
// One closed payload family every hook point types over: one Op-keyed case per fact shape, so a point's
// TFact bound is a case and the tap reads typed evidence. Format, codec, leg, tier, and outcome slots carry
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
```

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
public interface IBimHookPoint {
    BimHookId Id { get; }
    BimHookModality Modality { get; }
    Type Fact { get; }
}

// Typed point capsule: the veto fold runs first and its refusal returns on the emitter's own rail while the
// same refusal folds into the registry evidence cell, the observe fan posts shielded to every tap, and a
// replay point folds the fact into its bounded buffer. The evidence cell arrives from the owning BimHooks
// composition — never a process-static — so two compositions hold two cells; Seq<Error> because a veto gate
// refuses with any typed Error while the shield always mints BimFault.CodecReject.
public sealed class BimHookPoint<TFact>(BimHookId id, BimHookModality modality, Atom<Seq<Error>> faults, int depth = 64) : IBimHookPoint
    where TFact : BimFact {
    readonly Atom<Seq<Func<TFact, Fin<TFact>>>> vetoes = Atom(Seq<Func<TFact, Fin<TFact>>>());
    readonly Atom<Seq<Func<TFact, IO<Unit>>>> taps = Atom(Seq<Func<TFact, IO<Unit>>>());
    readonly Atom<Seq<TFact>> buffer = Atom(Seq<TFact>());

    public BimHookId Id => id;
    public BimHookModality Modality => modality;
    public Type Fact => typeof(TFact);

    public Fin<TFact> Fire(TFact fact) =>
        vetoes.Value.Fold(Fin.Succ(fact), static (state, veto) => state.Bind(veto))
            .MapFail(refusal => { ignore(faults.Swap(held => held.Add(refusal))); return refusal; })
            .Map(admitted => {
                if (Modality == BimHookModality.Replay)
                    ignore(buffer.Swap(held => (held.Add(admitted) is var next && next.Count > depth ? next.Skip(next.Count - depth) : next).Strict()));
                ignore(taps.Value.Iter(tap => Shielded(admitted, tap)));
                return admitted;
            });

    public Fin<IDisposable> Veto(Func<TFact, Fin<TFact>> gate) =>
        Modality == BimHookModality.Veto
            ? Fin.Succ(Attach(vetoes, gate))
            : Fin.Fail<IDisposable>(new BimFault.ModelRejected(Op.Of(name: nameof(Veto)), $"hook-modality:{Id}:{Modality.Key}"));

    public IDisposable Observe(Func<TFact, IO<Unit>> tap) {
        var detach = Attach(taps, tap);
        ignore(buffer.Value.Iter(held => Shielded(held, tap)));
        return detach;
    }

    public Seq<TFact> Drain() => buffer.Value;

    // Subscriber-fault isolation: a throwing or failing tap converts to CodecReject (hook-tap) carrying the
    // fact's kernel Op, folds into the composition's evidence cell beside the recorded veto refusals, and the
    // emitter's Fire result is untouched.
    Unit Shielded(TFact fact, Func<TFact, IO<Unit>> tap) =>
        Try.lift(() => tap(fact).Run()).Run().Match(
            Succ: static _ => unit,
            Fail: error => ignore(faults.Swap(held => held.Add(new BimFault.CodecReject(fact.Key, $"hook-tap:{Id}:{error.Message}")))));

    static IDisposable Attach<T>(Atom<Seq<T>> cell, T row) {
        ignore(cell.Swap(held => held.Add(row)));
        return new BimHookDetacher(() => ignore(cell.Swap(held => held.Filter(entry => !ReferenceEquals(entry, row)).ToSeq().Strict())));
    }
}

file sealed record BimHookDetacher(Action Release) : IDisposable {
    public void Dispose() => Release();
}

// --- [COMPOSITION] ------------------------------------------------------------------------
// Per-composition registry record: one Live() per app composition, every point reached through its
// declared field, one shared evidence cell per composition — no process-global registry, no lookup surface.
public sealed record BimHooks(
    BimHookPoint<BimFact.Progress> ExchangeProgress,
    BimHookPoint<BimFact.Imported> Imported,
    BimHookPoint<BimFact.Exported> Exported,
    BimHookPoint<BimFact.Lowered> Lowered,
    BimHookPoint<BimFact.Admission> Legality,
    BimHookPoint<BimFact.Verdict> Verdict,
    BimHookPoint<BimFact.Progress> EnergyProgress,
    BimHookPoint<BimFact.Emitted> Emitted,
    Atom<Seq<Error>> Faults) {
    public static BimHooks Live() {
        var faults = Atom(Seq<Error>());
        return new(
            new(BimHookId.Create("rasm.bim.exchange.progress"), BimHookModality.Observe, faults),
            new(BimHookId.Create("rasm.bim.exchange.imported"), BimHookModality.Observe, faults),
            new(BimHookId.Create("rasm.bim.exchange.exported"), BimHookModality.Observe, faults),
            new(BimHookId.Create("rasm.bim.projection.lowered"), BimHookModality.Observe, faults),
            new(BimHookId.Create("rasm.bim.projection.legality"), BimHookModality.Veto, faults),
            new(BimHookId.Create("rasm.bim.review.verdict"), BimHookModality.Replay, faults),
            new(BimHookId.Create("rasm.bim.energy.progress"), BimHookModality.Observe, faults),
            new(BimHookId.Create("rasm.bim.energy.emitted"), BimHookModality.Observe, faults),
            faults);
    }
}
```

## [03]-[TELEMETRY_TAP]

- Owner: `BimTelemetry` the one meter owner and instrument roster — receipts stay billing truth, instruments are the lossy dashboard channel projected from them; `BimInstrumentRow` the roster row (name, bind delegate); `BimBuckets` the explicit-bound advice policy; `BimTelemetry.Source` the one package `ActivitySource`.
- Cases: instrument roster — `rasm.bim.exchange.import.duration` (histogram `double`, `s`, tags `format`/`codec`, source `BimFact.Imported`), `rasm.bim.exchange.import.bytes` (histogram `long`, `By`, tag `format`, source `BimFact.Imported`), `rasm.bim.exchange.instancing` (histogram `long`, `{instance}`, tag `format`, source `BimFact.Imported` `Instances` sharing evidence), `rasm.bim.exchange.export.duration` (histogram `double`, `s`, tag `format`, source `BimFact.Exported`), `rasm.bim.projection.nodes` (histogram `long`, `{node}`, tag `projector`, source `BimFact.Lowered`), `rasm.bim.projection.edges` (histogram `long`, `{edge}`, tag `projector`, source `BimFact.Lowered`), `rasm.bim.legality.rejects` (counter `long`, `{reject}`, tag `category`, source the `Faults` registry evidence cell — veto refusals and hook-tap isolations — through the cell's `Change` tap, banded by `error.Category()`), `rasm.bim.review.verdicts` (counter `long`, `{verdict}`, tags `tier`/`outcome`, source `BimFact.Verdict`), `rasm.bim.energy.exchanges` (counter `long`, `{exchange}`, tags `leg`/`format`, source `BimFact.Emitted`), `rasm.bim.energy.warnings` (counter `long`, `{warning}`, tag `format`, source `BimFact.Emitted` `Warnings`).
- Entry: `BimTelemetry.Mount(IMeterFactory factory, string version)` — the one meter mint through `IMeterFactory.Create(MeterOptions)`, scope name the package id `Rasm.Bim`, materializing every roster row over the minted meter; `BimTelemetry.Tap(BimHooks hooks, BimInstrumentSet set)` mounts the observe subscriptions at composition, so create and write calls live only inside this spine and an emitting page declares facts, never an inline create; `BimTelemetry.Traced(Op op, Func<Fin<T>> body)` the span wrapper every long-running entry composes.
- Auto: every histogram row ships `InstrumentAdvice<T>` explicit-bucket boundaries at creation — the fallback a backend without exponential histograms reads, the default aggregation staying base2-exponential at the AppHost provider rows; instrument identity de-duplicates by name inside the meter, so name, unit, and description are declaration facts this roster carries once; tag values ride the typed fact's own vocabulary keys (`InterchangeFormat.Key`, `EnergyLeg` key, `Category`), never free text.
- Receipt: none — the tap is a projection of receipts and hook facts; a metric minted beside it is a second truth.
- Packages: LanguageExt.Core, NodaTime, Rasm, BCL inbox (`System.Diagnostics.Metrics`, `System.Diagnostics.DiagnosticSource`).
- Growth: a new projected fact is one roster row and one `Tap` subscription arm; a new span dimension is one tag row at activity start; a per-vocabulary instrument family derives from its owning vocabulary rows, never hand-enumerated names.
- Boundary: library altitude forbids any OpenTelemetry reference — the meter reaches the process only through the injected `IMeterFactory`, so provider disposal owns instrument lifetime and a `new Meter(...)` construction never appears; span law — one `ActivitySource` named `Rasm.Bim`, span names derive from the kernel `Op` (`op.ToString()`, never a fresh literal), `HasListeners()` gates the wrapper ahead of the call, `ActivityKind.Internal` states the in-process boundary, and a failing rail lands `SetStatus(ActivityStatusCode.Error, message)` so the typed verdict — not a tag — carries the error fact; baggage law — every instrument write appends `tenant` and `model` tags read once per fact from `Activity.Current?.GetBaggageItem` under the declared envelope keys `rasm.tenant`/`rasm.model` (identifiers the app-tier envelope seam promotes onto W3C baggage; an absent key omits its tag, so no empty-string series mints), and a tenant read never threads a parameter through a domain signature; SDK composition, exporters, trace-based exemplars, `AddView` shapes, and cardinality caps stay `csharp:Rasm.AppHost/Observability/telemetry.md`'s — the AppHost root admits the `Rasm.Bim` meter and source by name.

```csharp signature
// --- [CONSTANTS] --------------------------------------------------------------------------
// Explicit-bound advice rows — the fallback a backend without base2-exponential histograms reads.
public static class BimBuckets {
    public static readonly ImmutableArray<double> DecodeSeconds = [0.01, 0.05, 0.1, 0.5, 1, 5, 15, 60, 300];
    public static readonly ImmutableArray<double> ByteSizes = [1e4, 1e5, 1e6, 1e7, 1e8, 1e9];
    public static readonly ImmutableArray<double> GraphCounts = [10, 100, 1000, 10_000, 100_000, 1_000_000];
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record BimInstrumentRow(string Name, string Unit, string Description, Func<Meter, string, string, string, Instrument> Bind);

public sealed record BimInstrumentSet(Meter Meter, FrozenDictionary<string, Instrument> ByName) {
    // Statement seam: the params span cannot cross a lambda, so each write branches in place.
    public Unit Count(string name, long value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
        if (ByName[name] is Counter<long> counter) counter.Add(value, tags);
        return unit;
    }

    public Unit Record(string name, double value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
        if (ByName[name] is Histogram<double> histogram) histogram.Record(value, tags);
        return unit;
    }

    public Unit Record(string name, long value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
        if (ByName[name] is Histogram<long> histogram) histogram.Record(value, tags);
        return unit;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BimTelemetry {
    public const string Scope = "Rasm.Bim";
    public const string TenantKey = "rasm.tenant";
    public const string ModelKey = "rasm.model";

    // One package ActivitySource; span names derive from the kernel Op, never a fresh literal.
    public static readonly ActivitySource Source = new(Scope);

    static Histogram<T> Advised<T>(Meter meter, string name, string unit, string text, ImmutableArray<double> bounds) where T : struct =>
        meter.CreateHistogram<T>(name, unit, text, tags: null,
            advice: new InstrumentAdvice<T> { HistogramBucketBoundaries = [.. bounds.Select(static b => (T)Convert.ChangeType(b, typeof(T), CultureInfo.InvariantCulture))] });

    public static readonly Seq<BimInstrumentRow> Rows = Seq(
        new BimInstrumentRow("rasm.bim.exchange.import.duration", "s", "foreign-bytes decode wall duration per format and codec",
            static (meter, name, unit, text) => Advised<double>(meter, name, unit, text, BimBuckets.DecodeSeconds)),
        new BimInstrumentRow("rasm.bim.exchange.import.bytes", "By", "decoded source byte count per format",
            static (meter, name, unit, text) => Advised<long>(meter, name, unit, text, BimBuckets.ByteSizes)),
        new BimInstrumentRow("rasm.bim.exchange.instancing", "{instance}", "instance placements per decoded pool",
            static (meter, name, unit, text) => Advised<long>(meter, name, unit, text, BimBuckets.GraphCounts)),
        new BimInstrumentRow("rasm.bim.exchange.export.duration", "s", "artifact emit wall duration per format",
            static (meter, name, unit, text) => Advised<double>(meter, name, unit, text, BimBuckets.DecodeSeconds)),
        new BimInstrumentRow("rasm.bim.projection.nodes", "{node}", "seam delta node magnitude per projector",
            static (meter, name, unit, text) => Advised<long>(meter, name, unit, text, BimBuckets.GraphCounts)),
        new BimInstrumentRow("rasm.bim.projection.edges", "{edge}", "seam delta edge magnitude per projector",
            static (meter, name, unit, text) => Advised<long>(meter, name, unit, text, BimBuckets.GraphCounts)),
        new BimInstrumentRow("rasm.bim.legality.rejects", "{reject}", "legality and hook rejections banded by fault category",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new BimInstrumentRow("rasm.bim.review.verdicts", "{verdict}", "review verdicts by tier and outcome",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new BimInstrumentRow("rasm.bim.energy.exchanges", "{exchange}", "energy artifacts by leg and format",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new BimInstrumentRow("rasm.bim.energy.warnings", "{warning}", "energy exchange warning tallies per format",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)));

    public static BimInstrumentSet Mount(IMeterFactory factory, string version) {
        var meter = factory.Create(new MeterOptions(Scope) { Version = version });
        return new(meter, Rows
            .Map(row => KeyValuePair.Create(row.Name, row.Bind(meter, row.Name, row.Unit, row.Description)))
            .ToFrozenDictionary(StringComparer.Ordinal));
    }

    // Telemetry-as-tap: the whole projection mounts as observe subscriptions — zero emit calls inside a
    // projector, codec arm, or review fold; every write appends the baggage-sourced tenant/model tags. The
    // registry evidence cell taps through its Atom Change event (synchronous, one appended fault per swap),
    // so a veto refusal and a shielded tap fault land on the rejects counter banded by the kernel Category.
    public static Seq<IDisposable> Tap(BimHooks hooks, BimInstrumentSet set) {
        AtomChangedEvent<Seq<Error>> rejected = held => held.Last.Iter(fault =>
            ignore(set.Count("rasm.bim.legality.rejects", 1L, Attributed(("category", fault.Category())))));
        hooks.Faults.Change += rejected;
        IDisposable drained = new BimHookDetacher(() => hooks.Faults.Change -= rejected);
        return Subscriptions(hooks, set).Add(drained);
    }

    static Seq<IDisposable> Subscriptions(BimHooks hooks, BimInstrumentSet set) => Seq(
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

    // Span wrapper every long-running Bim entry composes: HasListeners gates the whole wrapper, the span
    // name IS the kernel Op, and a failing rail lands the typed Error verdict — never an error fact in a tag.
    public static Fin<T> Traced<T>(Op op, Func<Fin<T>> body) {
        if (!Source.HasListeners()) { return body(); }
        using var activity = Source.StartActivity(op.ToString(), ActivityKind.Internal);
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
- Cases: claim rows — `ImportGlb`, `ImportIfc`, `ImportDwg`, `ImportPly`, `ImportScene`, `ImportUsd`, `ImportDotbim` (foreign-bytes decode per codec arm), `EgressReauthor` (`Projection/egress#IFC_EGRESS` re-author over an admitted graph), `QueryMedium`/`QueryLarge` (`Model/query#ELEMENT_SET` predicate folds at the two corpus graph scales), `GeoVector`/`GeoRaster` (`Semantics/geospatial#GEOSPATIAL_SEAM` ingest), `TessellationRoundTrip` (`Exchange/tessellation#TESSELLATION_BRIDGE` companion round trip) — each row carrying its `Corpus` column, the estate corpus artifact slug whose content fingerprint the receipt stamps.
- Entry: the bench project constructs `BimBenchReceipt` rows at its edge — one per claim per run — and the corpus-gate admission row below is the ONE path a receipt becomes a standing claim.
- Auto: `CorpusFingerprint` derives through the one kernel content hasher over the corpus artifact bytes, so a claim binds to the exact input it measured and a corpus revision invalidates every dependent claim structurally, never by prose.
- Receipt: `BimBenchReceipt` — claim, corpus fingerprint, median and p95 wall duration, allocated bytes, operation count, instant; distribution truth, no verdict field — judging is the gate fold's, not the receipt's.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm, BCL inbox.
- Growth: a new measured operation is one `BimBenchClaim` row; a new measured axis is one field on the receipt breaking the gate mapping at compile time.
- Boundary: corpus-gate admission — a speed or allocation claim on any Rasm.Bim page resolves to a `BimBenchReceipt` the estate BenchmarkDotNet corpus gate stamped: the branch bench project folds each receipt into the app-tier benchmark envelope (suite `rasm.bim`, case the claim key) and the `csharp:Rasm.AppHost/Observability/benchmarks.md` `[02]-[BENCHMARK_RECEIPT]` `BenchmarkGate.Judge` fold owns pass-or-regress under the host-evidence and budget law; BenchmarkDotNet binds in the branch test and benchmark projects per the Test Stack manifest tier, never `Rasm.Bim.csproj`, so no benchmark type crosses into this package; a hand-rolled kernel is admitted only after its receipt defeats the library route under that gate.

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

- [ENVELOPE_KEY_PROMOTION]: `rasm.tenant`/`rasm.model` baggage keys ground against the app-tier envelope seam that must declare and promote them — `csharp:Rasm.AppHost/Observability/telemetry.md` today sets only the `CorrelationId` baggage key and carries the per-tenant dimension as `TenantContext.Tag`; two baggage stores live in one process and never synchronize (the OpenTelemetry `Baggage.Current` SDK store versus the BCL `Activity` baggage chain), and a BCL-only reader reaches ONLY `Activity.Current.GetBaggageItem`, so the promotion leg must write through the Activity-visible store (`Activity.AddBaggage` or the `DistributedContextPropagator` extract path), never `Baggage.SetBaggage` alone; confirm the promoted key spellings AND the store the seam writes against the AppHost envelope declaration when the tenant-baggage promotion row lands there, re-anchoring `BimTelemetry.TenantKey`/`ModelKey` if either differs.
- [CORPUS_ARTIFACT_SLUGS]: `BimBenchClaim.Corpus` slugs ground against the estate benchmark corpus registry under `tests/` — confirm the artifact slug vocabulary and the fingerprint derivation seam against the bench-harness corpus manifest when the branch bench project lands, via `tests/README.md` routing.
