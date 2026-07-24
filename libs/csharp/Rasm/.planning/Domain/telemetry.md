# [RASM_TELEMETRY]

`Rasm.Domain` (`Domain/Telemetry.cs`) owns the C# branch's one OTel-free signal capsule — the generic hook, instrument, and receipt mechanism every stratum composes downward as an instance — and is its own first consumer. One home holds the capsule TYPE; a stratum above composes an INSTANCE against its own fact union.

Every owner is instance-owned and composition-entered — the evidence cell, meter, and registry arrive from the composing app, so two compositions never contend for one slot. Dependency split draws the boundary: this page carries `System.Diagnostics.Metrics`, LanguageExt, and Thinktecture; OTel-SDK wiring, exporters, sampling, the source roster, and the correlation-and-tenant-laced receipt envelope stay at the app platform. Fire is synchronous from any stratum.

## [01]-[INDEX]

- [02]-[SIGNAL_CAPSULE]: `HookPoint<TFact>` fires one synchronous point over the id grammar, veto/observe/replay modality, and the frozen registry.
- [03]-[INSTRUMENT_MECHANISM]: `Buckets` advice, `LevelCells` levels, and the `ReceiptFan` kind-keyed wire-projection fold.
- [04]-[SIGNAL_TAP]: `SignalFact` and its keyed rail, `TelemetrySink.Tap` the one emission entry, the `rasm.kernel` meters and trace band.
- [05]-[OP_COST]: `OpCost` bills each `Op` — elapsed, allocated bytes, and item count.
- [06]-[BENCH_LEDGER]: `BenchClaim` rows fold into the duplicate-refusing `BenchLedger` the corpus gate ingests.

## [02]-[SIGNAL_CAPSULE]

- Owner: `HookId` keys points under the estate grammar `rasm.<pkg>.<domain>.<point>`; `HookModality` carries `CanVeto` as row data, so veto admission is the modality's own column, never a subscriber-supplied flag — `Veto` transforms or refuses, `Observe` taps fault-isolated, `Replay` buffers for late drain.
- Entry: `Fire` discriminates by call shape — unary publishes a settled fact, the guarded form refuses vetoes BEFORE the seam body and runs observe taps only from its success path; `Veto`, `Observe`, and `Drain` are the subscriber entries. A null delegate refuses on the typed rail (`Fire`, `Veto`) or throws at `Observe`'s argument contract, so no null reaches mount or dispatch.
- Auto: fire order is law — retention first so replay truth is the last fact even under a veto refusal; the veto left-fold second, its first refusal the verdict parked beside the return; observe taps last, each forked before its shielded run so the synchronous path returns without waiting. A fork refusal or throwing tap parks as `IsolatedFault` and delivery continues; a replay point prunes its buffer to `depth` oldest-first per fire and hands a fresh subscriber the held window on attach.
- Receipt: a point mints nothing — the fire IS the evidence event, the emitter's typed receipt already carrying the fact; one shared fault cell records veto refusals and shielded tap faults point-attributed, drained by the composing app or projected onto a rejects counter through its `Change` tap.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new delivery semantics is one `HookModality` row breaking every modality dispatch at compile time; a consuming folder's new point is one `HookPoint<TFact>` value on its own vocabulary — the capsule type never widens per folder.
- Boundary: `TFact` closes at declaration as a typed record or union case from the owning folder, so a stringly payload cannot enter the rail; a subscriber failure is evidence or a refusal, never a broken emitter or a starved sibling, because every tap runs inside its own shield.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct HookId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Split('.') is ["rasm", var pkg, var domain, var point]
            && pkg.Length > 0 && domain.Length > 0 && point.Length > 0
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-' or '_')
            ? null
            : new ValidationError(message: $"HookId requires the rasm.<pkg>.<domain>.<point> grammar: {value}");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HookModality {
    public static readonly HookModality Veto = new("veto", canVeto: true);
    public static readonly HookModality Observe = new("observe", canVeto: false);
    public static readonly HookModality Replay = new("replay", canVeto: false);

    public bool CanVeto { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IsolatedFault(HookId Point, Error Cause);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct HookDetacher(Action Detach) : IDisposable {
    public void Dispose() => Detach();
}

// --- [SERVICES] -----------------------------------------------------------------------------
public interface IHookPoint {
    HookId Id { get; }
    HookModality Modality { get; }
    Type Fact { get; }
}

// Evidence cell is a ctor param from the owning composition, never process-static — two compositions hold two cells.
public sealed class HookPoint<TFact> : IHookPoint {
    private readonly HookId id;
    private readonly HookModality modality;
    private readonly Atom<Seq<IsolatedFault>> faults;
    private readonly int depth;
    private readonly Atom<Seq<Func<TFact, Fin<TFact>>>> vetoes = Atom(Seq<Func<TFact, Fin<TFact>>>());
    private readonly Atom<Seq<Func<TFact, IO<Unit>>>> taps = Atom(Seq<Func<TFact, IO<Unit>>>());
    private readonly Atom<Seq<TFact>> buffer = Atom(Seq<TFact>());

    public HookPoint(HookId id, HookModality modality, Atom<Seq<IsolatedFault>> faults, int depth = 64) {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        this.id = id;
        this.modality = modality;
        this.faults = faults;
        this.depth = depth;
    }

    public HookId Id => id;
    public HookModality Modality => modality;
    public Type Fact => typeof(TFact);

    public Fin<TFact> Fire(TFact fact) => Fire(fact: fact, body: Fin.Succ);

    // Null body refuses on the rail before retention, matching every delegate admission on this capsule.
    public Fin<T> Fire<T>(TFact fact, Func<Fin<T>> body) =>
        body is null
            ? Fin.Fail<T>(new Fault.InvalidValue(Label: Id.ToString(), Requirement: "a guarded fire requires a non-null body"))
            : Fire(fact: fact, body: _ => body());

    private Fin<T> Fire<T>(TFact fact, Func<TFact, Fin<T>> body) {
        Unit _ = Retain(fact: fact);
        return Admitted(fact: fact).Bind(admitted => body(admitted).Map(value => (Dispatch(fact: admitted), value).Item2));
    }

    public Fin<IDisposable> Veto(Func<TFact, Fin<TFact>> gate) =>
        gate is null
            ? Fin.Fail<IDisposable>(new Fault.InvalidValue(Label: Id.ToString(), Requirement: "a veto subscription requires a non-null gate"))
            : Modality.CanVeto
            ? Fin.Succ<IDisposable>(Attach(cell: vetoes, row: gate))
            : Fin.Fail<IDisposable>(new Fault.InvalidValue(Label: Id.ToString(), Requirement: "a veto subscription requires a veto-capable point"));

    public IDisposable Observe(Func<TFact, IO<Unit>> tap) {
        ArgumentNullException.ThrowIfNull(argument: tap);
        IDisposable detach = Attach(cell: taps, row: tap);
        ignore(buffer.Value.Iter(held => Forked(fact: held, tap: tap)));
        return detach;
    }

    public Seq<TFact> Drain() => buffer.Value;

    private Fin<TFact> Admitted(TFact fact) =>
        vetoes.Value.Fold(Fin.Succ(fact), static (state, veto) => state.Bind(veto))
            .MapFail(refusal => (Park(cause: refusal), refusal).Item2);

    private Unit Retain(TFact fact) =>
        Modality.Equals(HookModality.Replay)
            ? ignore(buffer.Swap(held => (held.Add(fact) is var next && next.Count > depth ? next.Skip(next.Count - depth) : next).Strict()))
            : unit;

    private Unit Dispatch(TFact fact) => ignore(taps.Value.Iter(tap => Forked(fact: fact, tap: tap)));

    // Fork before forcing: Run forces only the fork queue, never the subscriber body; fork and subscriber faults share one parked-evidence arm.
    private Unit Forked(TFact fact, Func<TFact, IO<Unit>> tap) =>
        Try.lift(() => IO.lift(() => Shielded(fact: fact, tap: tap)).Fork(None).Run()).Run().Match(
            Succ: static _ => unit,
            Fail: error => Park(cause: error));

    private Unit Shielded(TFact fact, Func<TFact, IO<Unit>> tap) =>
        Try.lift(() => tap(fact).Run()).Run().Match(
            Succ: static _ => unit,
            Fail: error => Park(cause: error));

    private Unit Park(Error cause) => ignore(faults.Swap(held => held.Add(new IsolatedFault(Point: Id, Cause: cause))));

    private static IDisposable Attach<T>(Atom<Seq<T>> cell, T row) {
        ignore(cell.Swap(held => held.Add(row)));
        return new HookDetacher(Detach: () => ignore(cell.Swap(held => held.Filter(entry => !ReferenceEquals(entry, row)).ToSeq().Strict())));
    }
}

// Frozen mount table is the audit surface; a fired id outside it is unreachable because firing requires the declared point value.
public sealed record HookRegistry(FrozenDictionary<string, IHookPoint> Points) {
    public static HookRegistry Mount(params ReadOnlySpan<IHookPoint> points) =>
        new(Points: points.ToArray().ToFrozenDictionary(static point => point.Id.ToString(), static point => point, StringComparer.Ordinal));
}
```

## [03]-[INSTRUMENT_MECHANISM]

- Owner: `Buckets` is the one advice holder — every stratum reads a named bound row and binds it through `Advised<T>`, so a folder-local bound array is the forked-policy defect; an `InstrumentRow`'s bind delegate whose created type IS the instrument kind fuses declaration and kind in one row.
- Entry: `LevelCells.Level` and `Reader` each discriminate scalar versus keyed by call shape; `ReceiptFan.Of` merges contributed arm tables and a duplicate kind throws at the frozen merge; `Mint` stamps the semconv coordinate as `MeterOptions.TelemetrySchemaUrl`.
- Auto: instrument identity de-duplicates by name inside a meter, so a row carries name, unit, and state-reader once and an inline create with a drifted unit is the forked-stream defect; a keyed level family projects each map entry as one tagged `Measurement<long>`, so per-key cardinality rides ONE instrument and a per-key instrument mint is the deleted form; an unmapped kind projects nothing and stays receipt-only.
- Packages: LanguageExt.Core, BCL inbox (`System.Diagnostics.Metrics`, `System.Diagnostics.DiagnosticSource`, `System.Text.Json`).
- Growth: a new bucket policy is one `Buckets` row, a new instrument kind a bind delegate at its declaring row, a new projected kind one arm-table row in the contributing folder, a new level family one `Level` write site and one `Reader`-bound gauge row.
- Boundary: `TelemetryContributorPort` self-identifies by plain `string` scope — the app platform maps it into its own meter and source admission, so a contributor never names a platform type and crosses every stratum legally; meter and instrument lifetime ride the minting factory, so no capsule retains a meter handle or disposes one, and a `new Meter(...)` construction is the rejected form everywhere.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Named bound rows are estate policy data — the fallback a backend without base2-exponential histograms reads.
public static class Buckets {
    public static readonly ImmutableArray<double> HopSeconds = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10];
    public static readonly ImmutableArray<double> RemoteSeconds = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30];
    public static readonly ImmutableArray<double> ModelSeconds = [0.1, 0.25, 0.5, 1, 2, 5, 10, 30, 60];
    public static readonly ImmutableArray<double> BenchSeconds = [0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 1, 10];
    public static readonly ImmutableArray<double> DecodeSeconds = [0.01, 0.05, 0.1, 0.5, 1, 5, 15, 60, 300];
    public static readonly ImmutableArray<double> FoldSeconds = [0.0005, 0.001, 0.005, 0.01, 0.05, 0.1, 0.5, 1, 5, 10];
    public static readonly ImmutableArray<double> ProfileSeconds = [0.001, 0.01, 0.05, 0.1, 0.5, 1, 5, 15, 60];
    public static readonly ImmutableArray<double> CanvasFrameSeconds = [0.0005, 0.001, 0.0025, 0.005, 0.008, 0.017, 0.033, 0.066, 0.1, 0.25];
    public static readonly ImmutableArray<double> UiFrameSeconds = [0.002, 0.004, 0.008, 0.0167, 0.0333, 0.0667, 0.1, 0.25, 1];
    public static readonly ImmutableArray<double> AckSeconds = [0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5];
    public static readonly ImmutableArray<double> InteractionSeconds = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5];
    public static readonly ImmutableArray<double> SolveSeconds = [0.00001, 0.0001, 0.001, 0.01, 0.1, 0.5, 1, 5];
    public static readonly ImmutableArray<double> CompileSeconds = [0.0001, 0.001, 0.01, 0.05, 0.1, 0.5, 1, 5];
    public static readonly ImmutableArray<double> CadenceSeconds = [0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 15, 60];
    public static readonly ImmutableArray<double> CycleSeconds = [1, 10, 60, 300, 900, 3600, 14400, 86400];
    public static readonly ImmutableArray<double> RefreshSeconds = [60, 300, 900, 3600, 14400, 86400, 604800];
    public static readonly ImmutableArray<double> Fractions = [0.01, 0.05, 0.1, 0.25, 0.5, 0.75, 0.9, 1.0];
    public static readonly ImmutableArray<double> GoverningRatio = [0.25, 0.5, 0.75, 0.9, 1, 1.1, 1.25, 1.5, 2, 4];
    public static readonly ImmutableArray<double> DivergenceRatio = [0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2];
    public static readonly ImmutableArray<double> ResidualDecades = [1e-9, 1e-8, 1e-7, 1e-6, 1e-5, 1e-4, 1e-3, 1e-2, 1e-1, 1];
    public static readonly ImmutableArray<double> IterationCounts = [1, 2, 5, 10, 25, 50, 100, 250, 500, 1000, 2500];
    public static readonly ImmutableArray<double> Hypervolume = [0.05, 0.1, 0.2, 0.35, 0.5, 0.65, 0.8, 0.9, 0.95, 1];
    public static readonly ImmutableArray<double> CostUnitDecades = [0.0001, 0.001, 0.01, 0.1, 1, 10, 100, 1000];
    public static readonly ImmutableArray<double> Millimeters = [0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 1.0];
    public static readonly ImmutableArray<long> TokenCounts = [16, 64, 256, 1024, 4096, 16384, 65536];
    public static readonly ImmutableArray<long> GraphCounts = [10, 100, 1_000, 10_000, 100_000, 1_000_000];
    public static readonly ImmutableArray<long> ByteSizes = [10_000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000];
    public static readonly ImmutableArray<long> PayloadBytes = [1_024, 16_384, 262_144, 4_194_304, 67_108_864, 536_870_912];

    public static Histogram<T> Advised<T>(Meter meter, string name, string unit, string text, ImmutableArray<T> bounds) where T : struct =>
        meter.CreateHistogram<T>(name, unit, text, tags: null, advice: new InstrumentAdvice<T> { HistogramBucketBoundaries = bounds });
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record InstrumentRow(
    string Name,
    string Unit,
    string Description,
    Func<Meter, LevelCells, string, string, string, Instrument> Bind,
    Seq<string> Dimensions = default) {
    public static InstrumentRow Observable<T>(
        string name,
        string unit,
        string description,
        Seq<string> dimensions = default) where T : struct =>
        new(name, unit, description,
            (meter, cells, boundName, boundUnit, boundText) =>
                meter.CreateObservableGauge<T>(boundName, cells.Reader<T>(boundName), boundUnit, boundText),
            dimensions);
}

public sealed record TelemetryContributorPort(string Scope, string Version, string SchemaUrl, Seq<InstrumentRow> Instruments);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class LevelCells {
    private readonly Atom<HashMap<string, double>> scalars = Atom(HashMap<string, double>());
    private readonly Atom<HashMap<(string Family, string Key), long>> families = Atom(HashMap<(string Family, string Key), long>());
    private readonly Atom<HashMap<(string Name, Type Value), Delegate>> readers = Atom(HashMap<(string Name, Type Value), Delegate>());

    public Unit Level(string name, double value) => ignore(scalars.Swap(held => held.AddOrUpdate(name, value)));

    public Unit Level(string family, string key, long value) => ignore(families.Swap(held => held.AddOrUpdate((family, key), value)));

    public Fin<Unit> Bind<T>(string name, Func<T> read) where T : struct {
        if (string.IsNullOrWhiteSpace(name) || read is null)
            return Fin.Fail<Unit>(new Fault.InvalidValue(
                Label: nameof(InstrumentRow.Observable),
                Requirement: "a named non-null state reader"));
        readers.Swap(held => held.AddOrUpdate((name, typeof(T)), read));
        return Fin.Succ(unit);
    }

    public Func<double> Reader(string name) => () => scalars.Value.Find(name).IfNone(0d);

    public Func<T> Reader<T>(string name) where T : struct =>
        () => readers.Value.Find((name, typeof(T))).Match(
            Some: static read => ((Func<T>)read)(),
            None: static () => default);

    public Func<IEnumerable<Measurement<long>>> Reader(string family, string tag) =>
        () => toSeq(families.Value).Filter(pair => pair.Key.Family == family)
            .Map(pair => new Measurement<long>(pair.Value, new KeyValuePair<string, object?>(tag, pair.Key.Key)));
}

public sealed record InstrumentSet(FrozenDictionary<string, Instrument> ByName, LevelCells Cells) {
    public static InstrumentSet Of(Meter meter, LevelCells cells, Seq<InstrumentRow> rows) =>
        new(
            ByName: rows.Map(row => KeyValuePair.Create(row.Name, row.Bind(meter, cells, row.Name, row.Unit, row.Description)))
                .ToFrozenDictionary(StringComparer.Ordinal),
            Cells: cells);

    public Fin<Unit> Bind<T>(string name, Func<T> read) where T : struct => Cells.Bind(name, read);

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

public delegate void InstrumentArm(InstrumentSet set, LevelCells cells, JsonElement payload);

// Arm bodies are the one place wire names meet instrument writes.
public sealed record ReceiptFan(InstrumentSet Set, FrozenDictionary<string, InstrumentArm> Arms) {
    public static ReceiptFan Of(InstrumentSet set, params ReadOnlySpan<FrozenDictionary<string, InstrumentArm>> tables) =>
        new(Set: set, Arms: toSeq(tables.ToArray())
            .Bind(static table => toSeq(table.AsEnumerable()))
            .ToFrozenDictionary(StringComparer.Ordinal));

    public Unit Project(string kind, JsonElement payload) =>
        Arms.TryGetValue(kind, out InstrumentArm? arm) ? fun(() => arm(Set, Set.Cells, payload))() : unit;
}

public static class TelemetryIdentity {
    public static (ActivitySource Source, Meter Meter) Mint(
        IMeterFactory factory, string scope, string version, string schemaUrl,
        params ReadOnlySpan<KeyValuePair<string, object?>> tags) =>
        (new ActivitySource(scope, version),
         factory.Create(new MeterOptions(scope) {
             Version = version,
             TelemetrySchemaUrl = schemaUrl,
             Tags = [.. tags],
         }));
}
```

## [04]-[SIGNAL_TAP]

- Owner: `KernelDomain` rows derive both `SourceName` (`rasm.rasm.<domain>`) and their point ids off one row key — span source and hook-point prefix are ONE derivation, never two spellings. `SignalFact`'s abstract `At` projects each case's own `Point` storage, so identity moves `with`-safe. `SignalRail` is the keyed capsule instance; `TelemetrySink` the composition capsule `Env` carries.
- Cases: `ReceiptCase`, `FaultCase`, and `CostCase`; the `Receipt`/`Fault`/`Cost` factories derive canonical points — `<domain>.cost`, `<domain>.fault`, caller-named for receipts.
- Entry: `SignalRail.Point` declares-or-resolves a point, the first declaration fixing its modality, and `Publish` admits only a declared point before firing; `TelemetrySink.Tap` discriminates on the fact case through the generated `Switch`, writes instruments, then publishes — one entry, never a `RecordCost`/`CountFault`/`PublishReceipt` verb family.
- Auto: instrument writes carry the op key and domain as tag rows; a veto binds only at gate points consulted BEFORE the guarded action, so a post-hoc fact publishes for observation with its veto verdict advisory, and a refusal travels the same `Fin` rail every kernel failure travels.
- Receipt: fact payloads are evidence, never live resources — `ReceiptCase` carries the receipt value, `FaultCase` the already-lowered `Error` (both the substrate `Fault` union and the band-2400 `GeometryFault` arrive as `Error`, so one case serves both), and no case retains geometry, leases, or handles; both fault families land in ONE tag-discriminated counter, never two.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new fact kind is one `SignalFact` case and one `Tap` arm; a new instrument one `KernelInstruments` row and one write in the owning arm; a new sub-domain one `KernelDomain` row, span source and point prefix deriving; a package trace plane one `TraceScope` row admitted when the composition mints its band.
- Boundary: `TelemetrySink` is composition-entered — an app stratum mints one `TelemetrySink.Of(factory)` per composition and threads it on `Env` or as an explicit trailing parameter to a synchronous kernel below the `Eff` floor per the rails threading law, and a kernel page never constructs, caches, or reaches an ambient sink. Instrument custody is one-per-composition — `TelemetrySink.Of` or an app fan materializes the `KernelInstruments.Telemetry` port, never both. `HasListeners` gates every bracket, so an unlistened span costs one null test and a failing rail lands `SetStatus(ActivityStatusCode.Error, message)`, the typed verdict never an error tag; a package stratum composes its external scope rows into this band, never a second bracket owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KernelDomain {
    public static readonly KernelDomain Domain = new("domain");
    public static readonly KernelDomain Numerics = new("numerics");
    public static readonly KernelDomain Spatial = new("spatial");
    public static readonly KernelDomain Parametric = new("parametric");
    public static readonly KernelDomain Meshing = new("meshing");
    public static readonly KernelDomain Processing = new("processing");
    public static readonly KernelDomain Solving = new("solving");
    public static readonly KernelDomain Drawing = new("drawing");
    public static readonly KernelDomain Analysis = new("analysis");

    public TraceScope Trace => TraceScope.Create(value: $"rasm.rasm.{Key}");
    public string SourceName => Trace.ToString();

    [BoundaryAdapter] public HookId Point(string point) => HookId.Create(value: $"{SourceName}.{point}");
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct TraceScope {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Split('.') is ["rasm", var package, var plane]
            && package.Length > 0 && plane.Length > 0
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-' or '_')
            ? null
            : new ValidationError(message: $"TraceScope requires the rasm.<package>.<plane> grammar: {value}");
}

// --- [MODELS] -------------------------------------------------------------------------------
[Union]
public abstract partial record SignalFact {
    private SignalFact() { }

    // At projects each case's stored Point — a second stored copy diverges under `with`.
    public abstract HookId At { get; }

    public sealed record ReceiptCase(HookId Point, Op Key, IValidityEvidence Receipt) : SignalFact { public override HookId At => Point; }
    public sealed record FaultCase(HookId Point, Op Key, Error Fault) : SignalFact { public override HookId At => Point; }
    public sealed record CostCase(HookId Point, OpCost Cost) : SignalFact { public override HookId At => Point; }

    public static SignalFact Receipt(HookId point, Op key, IValidityEvidence receipt) => new ReceiptCase(Point: point, Key: key, Receipt: receipt);
    public static SignalFact Fault(KernelDomain domain, Op key, Error fault) => new FaultCase(Point: domain.Point(point: "fault"), Key: key, Fault: fault);
    public static SignalFact Cost(OpCost cost) => new CostCase(Point: cost.Domain.Point(point: "cost"), Cost: cost);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class SignalRail {
    private readonly Atom<HashMap<HookId, HookPoint<SignalFact>>> points = Atom(HashMap<HookId, HookPoint<SignalFact>>());

    public Atom<Seq<IsolatedFault>> Faults { get; } = Atom(Seq<IsolatedFault>());

    public HookPoint<SignalFact> Point(HookId point, HookModality modality) =>
        points.Swap(held => held.ContainsKey(point) ? held : held.Add(point, new HookPoint<SignalFact>(id: point, modality: modality, faults: Faults)))
            .Find(point).IfNone(() => throw new InvalidOperationException($"declared signal point {point} was not retained"));

    public Fin<SignalFact> Publish(SignalFact fact) =>
        points.Value.Find(fact.At)
            .ToFin(new Fault.InvalidValue(Label: fact.At.ToString(), Requirement: "a signal point declared before publication"))
            .Bind(point => point.Fire(fact: fact));
}

public sealed class KernelInstruments {
    public const string MeterName = "rasm.kernel";
    private const string OpDuration = "rasm.kernel.op.duration";
    private const string OpAllocated = "rasm.kernel.op.allocated";
    private const string OpItems = "rasm.kernel.op.items";
    private const string FaultCount = "rasm.kernel.fault.count";

    public static readonly Seq<InstrumentRow> Rows = Seq(
        new InstrumentRow(Name: OpDuration, Unit: "s", Description: "Kernel operation wall time.",
            Bind: static (meter, _, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.BenchSeconds),
            Dimensions: ["rasm.op", "rasm.domain"]),
        new InstrumentRow(Name: OpAllocated, Unit: "By", Description: "Kernel operation allocated bytes.",
            Bind: static (meter, _, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.ByteSizes),
            Dimensions: ["rasm.op", "rasm.domain"]),
        new InstrumentRow(Name: OpItems, Unit: "{item}", Description: "Kernel operation item count.",
            Bind: static (meter, _, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.GraphCounts),
            Dimensions: ["rasm.op", "rasm.domain"]),
        new InstrumentRow(Name: FaultCount, Unit: "{fault}", Description: "Kernel fault stream by category, case, and code.",
            Bind: static (meter, _, name, unit, text) => meter.CreateCounter<long>(name: name, unit: unit, description: text),
            Dimensions: ["rasm.op", "rasm.fault.category", "rasm.fault.case", "rasm.fault.code"]));

    private readonly InstrumentSet set;

    private KernelInstruments(InstrumentSet set) => this.set = set;

    public static KernelInstruments Of(IMeterFactory factory) =>
        new(set: InstrumentSet.Of(meter: factory.Create(new MeterOptions(MeterName)), cells: new LevelCells(), rows: Rows));

    public static TelemetryContributorPort Telemetry(string version, string schemaUrl) =>
        new(Scope: MeterName, Version: version, SchemaUrl: schemaUrl, Instruments: Rows);

    public Unit Cost(OpCost cost) {
        KeyValuePair<string, object?> op = new("rasm.op", cost.Key.ToString());
        KeyValuePair<string, object?> domain = new("rasm.domain", cost.Domain.Key);
        ignore(set.Record(OpDuration, cost.Elapsed.TotalSeconds, op, domain));
        ignore(set.Record(OpAllocated, cost.AllocatedBytes, op, domain));
        return set.Record(OpItems, cost.Items, op, domain);
    }

    public Unit Fault(Op key, Error fault) =>
        set.Count(FaultCount, 1L,
            new KeyValuePair<string, object?>("rasm.op", key.ToString()),
            new KeyValuePair<string, object?>("rasm.fault.category", fault.Category),
            new KeyValuePair<string, object?>("rasm.fault.case", fault.GetType().Name),
            new KeyValuePair<string, object?>("rasm.fault.code", fault.Code));
}

public sealed class TelemetrySink {
    private readonly KernelInstruments instruments;

    private TelemetrySink(SignalRail rail, KernelInstruments instruments) {
        Rail = rail;
        this.instruments = instruments;
    }

    public SignalRail Rail { get; }

    public static TelemetrySink Of(IMeterFactory factory) =>
        new(rail: new SignalRail(), instruments: KernelInstruments.Of(factory: factory));

    public Fin<SignalFact> Tap(SignalFact fact) {
        Unit _ = fact.Switch(
            state: instruments,
            receiptCase: static (_, _) => unit,
            faultCase: static (spine, f) => spine.Fault(key: f.Key, fault: f.Fault),
            costCase: static (spine, c) => spine.Cost(cost: c.Cost));
        return Rail.Publish(fact: fact);
    }
}

public sealed class SpanBand : IDisposable {
    private readonly FrozenDictionary<string, ActivitySource> sources;

    private SpanBand(FrozenDictionary<string, ActivitySource> sources) => this.sources = sources;

    public static SpanBand Of(string version, params ReadOnlySpan<TraceScope> externalScopes) =>
        new(sources: KernelDomain.Items.AsIterable().Map(static row => row.Trace)
            .Concat(Iterable<TraceScope>.FromSpan(externalScopes))
            .ToFrozenDictionary(static scope => scope.ToString(), scope => new ActivitySource(scope.ToString(), version), StringComparer.Ordinal));

    public Fin<T> Traced<T>(KernelDomain domain, Op key, Func<Fin<T>> body) =>
        Traced(domain.Trace, key, _ => body());

    public Fin<T> Traced<T>(TraceScope scope, Op key, Func<Activity?, Fin<T>> body) {
        ActivitySource source = sources[scope.ToString()];
        if (!source.HasListeners()) { return body(null); }
        using Activity? span = source.StartActivity(key.ToString(), ActivityKind.Internal);
        return body(span).MapFail(error => {
            ignore(span?.SetStatus(ActivityStatusCode.Error, error.Message));
            return error;
        });
    }

    public void Dispose() {
        foreach (ActivitySource source in sources.Values) { source.Dispose(); }  // Exemption: disposal sweep over the frozen source set
    }
}
```

## [05]-[OP_COST]

- Owner: `CostMark` is the capture pair — a monotonic tick and the thread allocation counter, minted by `Start()` before the guarded work and folded by `Stop` into `OpCost`. `OpCost` is the uniform per-op evidence (`Op` key, owning `KernelDomain`, wall span, allocated-byte delta, item count, success bit) — the kernel-side billing truth the app strata attribute to tenants.
- Law: one capture per operation runtime — `Operation.Apply` marks before its body fold, the `Prepare` gate inside the marked window so admission cost charges to the operation that demanded it, and charges on BOTH exits: the success leg records `Succeeded: true`, the fail leg `Succeeded: false` and publishes the fault fact, so cost and failure evidence never diverge.
- Law: allocation delta is thread-local evidence, valid because the synchronous runtime runs the marked window on one thread; a thread-hopping lane keeps elapsed truth and reads the delta as an allocation floor, never a total.
- Boundary: `OpCost` registers `IValidityEvidence`, so the fact reaches the one acceptance oracle like every kernel receipt; the capsule never wraps a second timer or a sampling profiler — profile capture is the app stratum's, this row the per-op scalar truth.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Diagnostics;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct OpCost(Op Key, KernelDomain Domain, TimeSpan Elapsed, long AllocatedBytes, int Items, bool Succeeded) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Nonnegative(value: Elapsed.TotalSeconds),
        ValidityClaim.Of(holds: AllocatedBytes >= 0L),
        ValidityClaim.CountAtLeast(count: Items, floor: 0));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CostMark(long Timestamp, long Allocated) {
    public static CostMark Start() => new(Timestamp: Stopwatch.GetTimestamp(), Allocated: GC.GetAllocatedBytesForCurrentThread());

    public OpCost Stop(Op key, KernelDomain domain, int items, bool succeeded) =>
        new(Key: key, Domain: domain,
            Elapsed: Stopwatch.GetElapsedTime(startingTimestamp: Timestamp),
            AllocatedBytes: long.Max(0L, GC.GetAllocatedBytesForCurrentThread() - Allocated),
            Items: items, Succeeded: succeeded);
}
```

## [06]-[BENCH_LEDGER]

- Owner: `BenchClaim` is the typed speed-claim row — the `Op` key naming the gated lane, the exact vectorized and reference member spellings under measurement, and the `SpeedupFloor` the corpus gate enforces. `BenchLedger` is the enumerable fold: `Of` refuses an invalid row and a duplicate claim key on the typed rail, `Rows` is the enumeration the corpus gate ingests, and `Unproven` returns every claim lacking a proven receipt, so an unproven speed claim is a visible ledger defect, never a prose hunt.
- Law: claim rows live BESIDE the lanes they gate as `static readonly` rows on their owning pages, and the app composition root composes them into the ledger — the substrate floor never references an upper stratum, so the ledger cannot mint the rows itself.
- Law: a claim is correctness-independent — the vectorized lane's result never depends on it; the claim gates only admission to the hot path, and a lane whose claim fails reverts to its reference row with zero behavior change.
- Boundary: `Rasm.AppHost`'s corpus gate reads `Rows` and resolves each claim to its `BenchmarkReceipt` verdict; judging, regression budgets, and host-evidence binding are the gate's — this ledger owns only the typed enumeration and the duplicate-refusal fold.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;

namespace Rasm.Domain;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record BenchClaim(Op Claim, string VectorizedLane, string ReferenceLane, double SpeedupFloor) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: SpeedupFloor),
        ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: VectorizedLane)),
        ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: ReferenceLane)));
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class BenchLedger {
    private BenchLedger(Seq<BenchClaim> rows) => Rows = rows;

    public Seq<BenchClaim> Rows { get; }

    public static Fin<BenchLedger> Of(params ReadOnlySpan<BenchClaim> claims) {
        Seq<BenchClaim> rows = toSeq(claims.ToArray());
        return rows.Exists(static row => !row.IsValid)
            ? Fin.Fail<BenchLedger>(new Fault.InvalidValue(Label: nameof(BenchClaim), Requirement: "positive speedup floor and non-blank lane spellings"))
            : rows.Map(static row => row.Claim).Distinct().Count != rows.Count
                ? Fin.Fail<BenchLedger>(new Fault.InvalidValue(Label: nameof(BenchLedger), Requirement: "one ledger row per claim key"))
                : Fin.Succ(new BenchLedger(rows: rows));
    }

    public Seq<BenchClaim> Unproven(Seq<Op> proven) => Rows.Filter(row => !proven.Contains(row.Claim));
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Kernel signal fabric
    accDescr: Typed receipts, faults, and cost capsules flow through one sink onto instruments and the keyed signal rail, every stratum composes the same capsule downward, and claim rows fold into the ledger the AppHost gate ingests.
    Capsule["signal capsule · HookPoint · InstrumentSet · Buckets · LevelCells · ReceiptFan"] -->|composed downward| Strata["L2 domain · L3 platform · L4 host instances"]
    Receipts["typed receipts · faults · OpCost"] -->|SignalFact factories| Sink["TelemetrySink.Tap — ONE entry"]
    Sink -->|case Switch| Instruments["KernelInstruments — rasm.kernel.* UCUM"]
    Sink -->|Publish| Rail["SignalRail — keyed capsule instance"]
    Rail -->|veto fold| Verdict["Fin — first refusal"]
    Rail -->|forked shielded taps| Faults["IsolatedFault evidence cell"]
    Env["Analysis Env.Telemetry"] -->|CostMark Start / Stop| Sink
    Claims["Simplify / Parametric / Surfaces / Flatten claim rows"] -->|BenchLedger.Of| Ledger["BenchLedger.Rows"]
    Ledger -.->|corpus gate + contributor ports| AppHost["Rasm.AppHost Observability"]
```

## [07]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface, and a stratum consumer is one composed instance of the capsule, never a re-declared type.

| [INDEX] | [AXIS_CONCERN]     | [OWNER]                               | [RAIL]                              | [CASES] |
| :-----: | :----------------- | :------------------------------------ | :---------------------------------- | :-----: |
|  [01]   | Point identity     | `HookId` + `HookModality`             | grammar key + modality rows         |  1 + 3  |
|  [02]   | Hook capsule       | `HookPoint<TFact>` + `HookRegistry`   | `Fire → Fin` (unary and guarded)    |    —    |
|  [03]   | Instrument rows    | `InstrumentRow` + `InstrumentSet`     | name-keyed tagged writes            |    —    |
|  [04]   | Advice + levels    | `Buckets` + `LevelCells`              | named rows + keyed `Reader`         |    —    |
|  [05]   | Wire projection    | `InstrumentArm` + `ReceiptFan`        | kind-keyed arm fold                 |    —    |
|  [06]   | Contribution       | `TelemetryContributorPort` + identity | string scope + `Mint` pair          |    —    |
|  [07]   | Sub-domain rows    | `KernelDomain`                        | discriminant (`SourceName` derives) |    9    |
|  [08]   | Fact vocabulary    | `SignalFact`                          | carrier + factories                 |    3    |
|  [09]   | Kernel rail + sink | `SignalRail` + `TelemetrySink`        | `Tap → Fin<SignalFact>`             |    —    |
|  [10]   | Trace band         | `TraceScope` + `SpanBand`             | `Traced → Fin` bracket              |    —    |
|  [11]   | Op-cost capsule    | `OpCost` + `CostMark`                 | evidence (oracle-registered)        |    —    |
|  [12]   | Bench claims       | `BenchClaim` + `BenchLedger`          | `Of → Fin<BenchLedger>`             |    —    |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

- [VETO_GATE_SITES]-[OPEN]: which kernel gate points consult `Veto` before their guarded action, and what dictionary-miss cost a subscriber-empty `Publish` owes per operation; land the first consultation at `Analysis/query.md` `Prepare` and measure the empty-rail fast path against the `BenchLedger` gate.
