# [RASM_INSTRUMENT]

`Rasm.Domain` owns the branch's one measurement plane: a declaration row naming an instrument family and its measurement type, the bucket-advice vocabulary those rows read, the mount that binds a row to a meter, the pushed and pulled write entries, and the backend-free tally a doctor verb reads without an exporter, a collector, or a store. Folders declare rows and write measurements; none spells a counter, gauge, or histogram create, holds a meter, or reaches a cell.

Every write addresses by ROW, never by name. `InstrumentSpec` is in scope at every write site because the declaring folder holds it as roster data, so the mount table keys on the declaration itself and the two-map-one-key pair a string key forced — one map to prove the row, a second to fetch the handle, with a throwing indexer between them — has no spelling. Identity IS the declaration: a re-typed unit or a drifted bucket set is a different row and therefore a different instrument, which is exactly the forked-stream defect the declaration exists to refuse.

## [01]-[INDEX]

- [02]-[SPEC]: `Buckets`, `InstrumentKind`, `MeasureForm`, `InstrumentSpec` — the advice roster, the family and measurement-type axes, and the one declaration row every sink composes.
- [03]-[MOUNT]: `TelemetryIdentity`, `LevelProbe`, `LevelCells` — the metered scope and the pulled cell store with its registered probes.
- [04]-[WRITE]: `InstrumentSet` — the mounted roster with its two derived indexes and the pushed, pulled, and registered measurement entries.
- [05]-[TALLY]: `ReadingCell`, `InstrumentReading`, `TallyState`, `InstrumentTally` — the diagnostic read plane over a mounted set.

## [02]-[SPEC]

- Owner: `Buckets` is the one advice holder — each row carries its UCUM unit and its boundary vector as columns and mints the advised histogram itself, so a folder-local bound array has nowhere to enter; `InstrumentKind` names the instrument family and carries write polarity as its own column; `MeasureForm` closes the measurement type and carries BOTH the mint and the listen half of each type; `InstrumentSpec` is the ONE declaration row every sink composes and the one admission every row's values cross.
- Cases: eight kinds span the whole instrument space — `Count` and `Delta` the synchronous monotone and signed writes, `Distribution` the histogram, `Reading` the call-site last value, `Total` and `Balance` the pulled monotone and signed totals, `Level` the pulled scalar, `Levels` the pulled family whose per-entry key is optional; two forms close the measurement type, so one generic bind body spells each create exactly once.
- Entry: `InstrumentSpec.Create` is the ONLY construction path and takes the family as a VALUE — the nine name-suffixed factories it replaces differed by one literal each and re-spelled the discriminant the `Kind` argument already carries. `Buckets.Advised<T>` is the advised-histogram mint each row owns.
- Auto: the kind-to-payload correspondence proves at admission and normalizes there — bounds ride a `Distribution` alone, a tag rides a `Levels` family exactly, and a keyed family's tag HEADS its own dimensions, so a panel break key, a partition indicator, and a view tag key resolve against one roster on every construction path rather than on one factory's habit. `Distribution` rows with no bounds bind the plain histogram, so base2-exponential aggregation stays the wire default and an explicit-bucket row is the per-instrument fallback the declaration re-arms.
- Law: bucket vectors admit at the `Buckets` ROW — a named UCUM unit, at least one boundary, every value finite, strictly ascending — so the forked-policy law finally has a producer: an unordered or NaN-bearing vector cannot become a row, and the advised declaration takes the row rather than any array a caller assembles.
- Law: a ladder carries its own UNIT and `InstrumentSpec` admission proves it against the declaring row's, so bare boundary numbers are readable as measurements — the ceiling proof at `Domain/objective` compares a latency bound against a seconds ladder because the pair is proven equal here, where a unit-blind roster left every consumer to guess the quantity.
- Law: a row carries name, unit, description, and state-reader once, so instrument identity de-duplicates inside a meter and an inline create with a drifted unit is the forked-stream defect this row deletes.
- Law: `Ceiling` is the declared per-row cardinality cap — the kernel-side bound a per-face or per-texel producer states, read by the tally's seating fold and the governance view caps, tightening the arming composition's ceiling and never widening it, because only the declaring row knows its own key space.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Diagnostics.Metrics`, `System.Numerics`).
- Growth: a new bucket policy is one `Buckets` row; a new instrument family one `InstrumentKind` row breaking the one generic bind at compile time; a new measurement type one `MeasureForm` row carrying BOTH its mint and its listen column, so a tally can never drop a type a mint admits.
- Boundary: `InstrumentSpec` families partition by UCUM unit and never by domain case — the case key rides `Dimensions`, so a landed unit needs no roster edit (branch RULINGS `[03]`). Meter and instrument lifetime ride the minting factory, so no owner here retains a meter handle or disposes one, and a `new Meter(...)` construction is the rejected form everywhere.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using System.Diagnostics.Metrics;
using System.Numerics;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Buckets {
    public const string Dimensionless = "1";
    public const string Seconds = "s";

    public static readonly Buckets HopSeconds = new("hop-seconds", Seconds, [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10]);
    public static readonly Buckets RemoteSeconds = new("remote-seconds", Seconds, [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30]);
    public static readonly Buckets ModelSeconds = new("model-seconds", Seconds, [0.1, 0.25, 0.5, 1, 2, 5, 10, 30, 60]);
    public static readonly Buckets BenchSeconds = new("bench-seconds", Seconds, [0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 1, 10]);
    public static readonly Buckets DecodeSeconds = new("decode-seconds", Seconds, [0.01, 0.05, 0.1, 0.5, 1, 5, 15, 60, 300]);
    public static readonly Buckets FoldSeconds = new("fold-seconds", Seconds, [0.0005, 0.001, 0.005, 0.01, 0.05, 0.1, 0.5, 1, 5, 10]);
    public static readonly Buckets ProfileSeconds = new("profile-seconds", Seconds, [0.001, 0.01, 0.05, 0.1, 0.5, 1, 5, 15, 60]);
    public static readonly Buckets CanvasFrameSeconds = new("canvas-frame-seconds", Seconds, [0.0005, 0.001, 0.0025, 0.005, 0.008, 0.017, 0.033, 0.066, 0.1, 0.25]);
    public static readonly Buckets UiFrameSeconds = new("ui-frame-seconds", Seconds, [0.002, 0.004, 0.008, 0.0167, 0.0333, 0.0667, 0.1, 0.25, 1]);
    public static readonly Buckets AckSeconds = new("ack-seconds", Seconds, [0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5]);
    public static readonly Buckets InteractionSeconds = new("interaction-seconds", Seconds, [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5]);
    public static readonly Buckets SolveSeconds = new("solve-seconds", Seconds, [0.00001, 0.0001, 0.001, 0.01, 0.1, 0.5, 1, 5]);
    public static readonly Buckets CompileSeconds = new("compile-seconds", Seconds, [0.0001, 0.001, 0.01, 0.05, 0.1, 0.5, 1, 5]);
    public static readonly Buckets CadenceSeconds = new("cadence-seconds", Seconds, [0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 15, 60]);
    public static readonly Buckets CycleSeconds = new("cycle-seconds", Seconds, [1, 10, 60, 300, 900, 3600, 14400, 86400]);
    public static readonly Buckets RefreshSeconds = new("refresh-seconds", Seconds, [60, 300, 900, 3600, 14400, 86400, 604800]);
    public static readonly Buckets Fractions = new("fractions", Dimensionless, [0.01, 0.05, 0.1, 0.25, 0.5, 0.75, 0.9, 1.0]);
    public static readonly Buckets GoverningRatio = new("governing-ratio", Dimensionless, [0.25, 0.5, 0.75, 0.9, 1, 1.1, 1.25, 1.5, 2, 4]);
    public static readonly Buckets DivergenceRatio = new("divergence-ratio", Dimensionless, [0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2]);
    public static readonly Buckets ResidualDecades = new("residual-decades", Dimensionless, [1e-9, 1e-8, 1e-7, 1e-6, 1e-5, 1e-4, 1e-3, 1e-2, 1e-1, 1]);
    public static readonly Buckets IterationCounts = new("iteration-counts", Dimensionless, [1, 2, 5, 10, 25, 50, 100, 250, 500, 1000, 2500]);
    public static readonly Buckets Hypervolume = new("hypervolume", Dimensionless, [0.05, 0.1, 0.2, 0.35, 0.5, 0.65, 0.8, 0.9, 0.95, 1]);
    public static readonly Buckets CostUnitDecades = new("cost-unit-decades", Dimensionless, [0.0001, 0.001, 0.01, 0.1, 1, 10, 100, 1000]);
    public static readonly Buckets Millimeters = new("millimeters", "mm", [0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 1.0]);
    public static readonly Buckets TokenCounts = new("token-counts", Dimensionless, [16, 64, 256, 1024, 4096, 16384, 65536]);
    public static readonly Buckets GraphCounts = new("graph-counts", Dimensionless, [10, 100, 1_000, 10_000, 100_000, 1_000_000]);
    public static readonly Buckets ByteSizes = new("byte-sizes", "By", [10_000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000]);
    public static readonly Buckets PayloadBytes = new("payload-bytes", "By", [1_024, 16_384, 262_144, 4_194_304, 67_108_864, 536_870_912]);

    public string Unit { get; }

    public ImmutableArray<double> Bounds { get; }

    public static Fin<Unit> Proof() =>
        toSeq(Items)
            .Filter(static row => string.IsNullOrWhiteSpace(row.Unit)
                || row.Bounds.IsEmpty
                || row.Bounds.Any(static bound => !double.IsFinite(bound))
                || row.Bounds.Zip(row.Bounds.Skip(1)).Any(static pair => pair.First >= pair.Second))
            .Map(static row => (Error)new KernelFault.InvalidValue(
                Label: row.Key, Requirement: "a named unit and strictly ascending finite bounds"))
            is { IsEmpty: false } faults
            ? Fin.Fail<Unit>(Error.Many(faults.Strict()))
            : Fin.Succ(unit);

    public Histogram<T> Advised<T>(Meter meter, InstrumentSpec row) where T : struct, INumberBase<T> =>
        meter.CreateHistogram<T>(row.Name, row.Unit, row.Description, tags: null,
            advice: new InstrumentAdvice<T> { HistogramBucketBoundaries = [.. Bounds.Select(static bound => T.CreateSaturating(bound))] });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InstrumentKind {
    public static readonly InstrumentKind Count = new("count", pulled: false);
    public static readonly InstrumentKind Delta = new("delta", pulled: false);
    public static readonly InstrumentKind Distribution = new("distribution", pulled: false);
    public static readonly InstrumentKind Reading = new("reading", pulled: false);
    public static readonly InstrumentKind Total = new("total", pulled: true);
    public static readonly InstrumentKind Balance = new("balance", pulled: true);
    public static readonly InstrumentKind Level = new("level", pulled: true);
    public static readonly InstrumentKind Levels = new("levels", pulled: true);

    public bool Pulled { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeasureForm {
    public static readonly MeasureForm Whole = new("long", Bound<long>, Heard<long>);
    public static readonly MeasureForm Real = new("double", Bound<double>, Heard<double>);

    [UseDelegateFromConstructor]
    public partial Instrument Mint(InstrumentSpec row, Meter meter, LevelCells cells);

    [UseDelegateFromConstructor]
    internal partial void Heard(MeterListener listener, MeasurementCallback<double> fold);

    private static void Heard<T>(MeterListener listener, MeasurementCallback<double> fold) where T : struct, INumberBase<T> =>
        listener.SetMeasurementEventCallback<T>((instrument, measurement, tags, state) =>
            fold(instrument, double.CreateSaturating(measurement), tags, state));

    private static Instrument Bound<T>(InstrumentSpec row, Meter meter, LevelCells cells)
        where T : struct, INumberBase<T> =>
        row.Kind.Switch(
            state: (Row: row, Meter: meter, Cells: cells),
            count: static bind => (Instrument)bind.Meter.CreateCounter<T>(bind.Row.Name, bind.Row.Unit, bind.Row.Description),
            delta: static bind => bind.Meter.CreateUpDownCounter<T>(bind.Row.Name, bind.Row.Unit, bind.Row.Description),
            distribution: static bind => bind.Row.Bounds.Match(
                Some: advice => advice.Advised<T>(bind.Meter, bind.Row),
                None: () => bind.Meter.CreateHistogram<T>(bind.Row.Name, bind.Row.Unit, bind.Row.Description)),
            reading: static bind => bind.Meter.CreateGauge<T>(bind.Row.Name, bind.Row.Unit, bind.Row.Description),
            total: static bind => bind.Meter.CreateObservableCounter(
                bind.Row.Name, bind.Cells.Reader<T>(bind.Row), bind.Row.Unit, bind.Row.Description),
            balance: static bind => bind.Meter.CreateObservableUpDownCounter(
                bind.Row.Name, bind.Cells.Reader<T>(bind.Row), bind.Row.Unit, bind.Row.Description),
            level: static bind => bind.Meter.CreateObservableGauge(
                bind.Row.Name, bind.Cells.Reader<T>(bind.Row), bind.Row.Unit, bind.Row.Description),
            levels: static bind => bind.Meter.CreateObservableGauge(
                bind.Row.Name, bind.Cells.Reader<T>(bind.Row, bind.Row.Tag), bind.Row.Unit, bind.Row.Description));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class InstrumentSpec {
    public static Fin<Unit> Named(Seq<(string Key, string Name)> rows) =>
        rows.Filter(static row => !string.Equals(row.Key, row.Name, StringComparison.Ordinal))
            .Map(static row => (Error)new KernelFault.InvalidValue(
                Label: row.Key, Requirement: $"an instrument named {row.Name}"))
            is { IsEmpty: false } faults
            ? Fin.Fail<Unit>(Error.Many(faults.Strict()))
            : Fin.Succ(unit);

    public string Name { get; }
    public InstrumentKind Kind { get; }
    public MeasureForm Form { get; }
    public string Unit { get; }
    public string Description { get; }
    public Seq<string> Dimensions { get; }
    public Option<Buckets> Bounds { get; }
    public Option<string> Tag { get; }
    public Option<int> Ceiling { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string name, ref InstrumentKind kind, ref MeasureForm form, ref string unit, ref string description,
        ref Seq<string> dimensions, ref Option<Buckets> bounds, ref Option<string> tag, ref Option<int> ceiling) {
        dimensions = tag.Match(
            Some: key => key.Cons(dimensions.Filter(row => !string.Equals(row, key, StringComparison.Ordinal))).Strict(),
            None: () => dimensions);
        validationError =
            !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(unit) && !string.IsNullOrWhiteSpace(description)
            && !dimensions.Exists(string.IsNullOrWhiteSpace)
            && dimensions.Distinct().Count == dimensions.Count
            && (bounds.IsNone || kind.Equals(InstrumentKind.Distribution))
            && !bounds.Exists(row =>
                !string.Equals(row.Unit, unit, StringComparison.Ordinal)
                && !(string.Equals(row.Unit, Buckets.Dimensionless, StringComparison.Ordinal) && unit.StartsWith('{')))
            && tag.IsSome == kind.Equals(InstrumentKind.Levels)
            && !ceiling.Exists(static bound => bound <= 0)
                ? null
                : new ValidationError(message:
                    $"InstrumentSpec requires a named row, a distinct dimension set, bounds only on a distribution "
                    + $"and in the row's own unit, a tag exactly on a levels family, and a positive series ceiling: {name}");
    }
}
```

## [03]-[MOUNT]

- Owner: `TelemetryIdentity` mints the metered scope under the branch's one semantic-convention pin; `LevelProbe` pairs one registered read with the tag set that read reports under; `LevelCells` is the raw pulled store holding pushed cells beside registered probes.
- Entry: `Metered` is the meter-only mint for a scope whose spans ride the signal capsule's band, `Mint` the pair form for a scope owning its own `ActivitySource`; `LevelCells.Reader<T>` is the ONE pulled projection for both cell shapes; `Bind` registers one owner's `Func<double>` read and returns the scope that retires exactly that registration.
- Auto: registration is a SET per row, not a slot — a bound row carries every live owner's read, so a lane limiter, a worker pool, and a durable intake each publish their own point under their own tags instead of the last registration silently deleting the readings before it; retiring the LAST probe drops the slot rather than leaving an empty sequence, because an empty bound slot still takes the probe arm and publishes nothing while the cell fallback stays unreachable. Bound probes win over the raw cell on the scalar shape and a keyed family UNIONS both, since a producer pushing per-key levels and a bounded owner registering its own key fill one family and neither earns a second instrument.
- Auto: one cell store keyed `(row, Option<string> key)` — a scalar level and a family's UNPARTITIONED entry are the same cell reached by the same write, and the mounted row's `InstrumentKind`, never a second store, decides whether the reader projects that cell tagged or bare. Present keys emit one tagged `Measurement<T>`, an absent one emits the same value with ZERO tags, so per-key cardinality and an unpartitioned composition report the identical series on ONE instrument; that absent key mirrors the settled tenancy arm exactly, where `TenantContext.Key` is `None` and `Tags` is empty.
- Law: an absent KEY is not an absent CELL. `None`-keyed entries carry a value a producer measured and the family reports untagged; a missing entry is the map's own absence and reports nothing at all, so the untagged arm never fabricates the zero the unmeasured law deletes everywhere else.
- Law: a probe that RAISES is not a probe that never wrote. Raises funnel through `Try.lift`, seat their cause on the store's own refusal cell keyed by row, and the tally reports it beside the cells, so a broken owner and a quiet one never read alike at collection — a cancelled probe keeps `KernelFault.Cancelled` rather than parking as an ordinary absence.
- Law: `SchemaUrl` is a pin, never a parameter — tracer, meter, and logger bump together on one coordinate, and no call site names it.
- Exemption: `LevelProbe.Tags` materializes to an array at registration — a probe registration is one-time and the per-collection `Measurement<T>` construction reads that array untouched, where the pushed path keeps its `TagList` on the stack.
- Law: a probe reads `double` and the mounted row saturates it into the declared carrier at collection, so a registration keys on the ROW alone — the `(row, Type)` slot and the `Delegate` cast back inside the SDK's own loop were the erasure pair the sibling mechanism forbids by name.
- Output: `Bind` returns the scope that ENDS a reading, because a level whose owner retired and whose value freezes at that owner's last write is indistinguishable at every collection from a live level nothing is moving.
- Packages: LanguageExt.Core, BCL inbox (`System.Diagnostics`, `System.Diagnostics.Metrics`, `System.Numerics`).
- Growth: a new bounded owner reporting its own saturation is one `Bind` scope over that owner's lifetime under its own tags, its probe joining the declared row's series and leaving with it.
- Boundary: every read, write, registration, and refusal member on `LevelCells` is assembly-internal — the cell store stays publicly constructible for composition — so `InstrumentSet` is the only reachable pulled entry from any consuming package and an ungated cell write has no spelling outside this assembly. Cells hold `double` at either key half, so the whole (kind × form) product carries its declared measurement type and a keyed real-valued level never truncates.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Numerics;

namespace Rasm.Domain;

// --- [SERVICES] ------------------------------------------------------------------------
public static class TelemetryIdentity {
    public const string SchemaUrl = "https://opentelemetry.io/schemas/1.43.0";

    public static Meter Metered(
        IMeterFactory factory, TelemetrySource scope, string version,
        params ReadOnlySpan<KeyValuePair<string, object?>> tags) =>
        factory.Create(new MeterOptions(scope.Key) {
            Version = version,
            TelemetrySchemaUrl = SchemaUrl,
            Tags = [.. tags],
        });

    public static (ActivitySource Source, Meter Meter) Mint(
        IMeterFactory factory, TelemetrySource scope, string version,
        params ReadOnlySpan<KeyValuePair<string, object?>> tags) =>
        (new ActivitySource(scope.Key, version), Metered(factory, scope, version, tags));
}

internal sealed record LevelProbe(KeyValuePair<string, object?>[] Tags, Func<double> Read);

public sealed class LevelCells {
    private readonly AtomHashMap<(InstrumentSpec Row, Option<string> Key), double> cells =
        AtomHashMap(HashMap<(InstrumentSpec Row, Option<string> Key), double>());
    private readonly AtomHashMap<InstrumentSpec, Seq<LevelProbe>> probes =
        AtomHashMap(HashMap<InstrumentSpec, Seq<LevelProbe>>());
    private readonly AtomHashMap<InstrumentSpec, Error> raised = AtomHashMap(HashMap<InstrumentSpec, Error>());

    internal Unit Level(InstrumentSpec row, Option<string> key, double value) => cells.AddOrUpdate((row), value);

    internal Fin<IDisposable> Bind(InstrumentSpec row, Func<double> read, KeyValuePair<string, object?>[] tags) =>
        Admit.Need(read).Map(admitted => {
            LevelProbe probe = new(Tags: tags, Read: admitted);
            probes.AddOrUpdate(row, live => live.Add(probe), () => Seq(probe));
            return (IDisposable)new HookDetacher(Detach: () => probes.SwapKey(row, live => live
                .Map(rows => rows.Filter(entry => !ReferenceEquals(entry, probe)).Strict())
                .Filter(rows => !rows.IsEmpty)));
        });

    internal Option<Error> Raised(InstrumentSpec row) => raised.Find(row);

    internal Func<IEnumerable<Measurement<T>>> Reader<T>(InstrumentSpec row, Option<string> tag = default)
        where T : struct, INumberBase<T> =>
        () => tag.Match(
            Some: tag => cells.AsIterable()
                .Filter(pair => pair.Key.Row.Equals(row))
                .Map(pair => pair.Key.Key.Match(
                    Some: key => new Measurement<T>(
                        T.CreateSaturating(pair.Value), new KeyValuePair<string, object?>(tag, key)),
                    None: () => new Measurement<T>(T.CreateSaturating(pair.Value))))
                .ToSeq() + probes.Find(row)
                    .Map(live => Probed<T>(row, live)).IfNone(Seq<Measurement<T>>()),
            None: () => probes.Find(row).Match(
                Some: live => Probed<T>(row, live),
                None: () => cells.Find((row, Option<string>.None))
                    .Map(held => new Measurement<T>(T.CreateSaturating(held))).ToSeq()));

    private Seq<Measurement<T>> Probed<T>(InstrumentSpec row, Seq<LevelProbe> live) where T : struct, INumberBase<T> =>
        live.Bind(probe => Try.lift(() =>
                Fin.Succ(Seq(new Measurement<T>(T.CreateSaturating(probe.Read()), probe.Tags)))).Run().Bind(static inner => inner)
            .IfFail(cause => (raised.AddOrUpdate(row, seat => seat + cause, () => cause),
                Seq<Measurement<T>>()).Item2)).Strict();
}
```

## [04]-[WRITE]

- Owner: `InstrumentSet` is the mounted roster — one row-and-handle pair sequence as the authority with two DERIVED frozen indexes, the pushed `Write`, the pulled `Level`, the registered `Bind`, the `Tags` projection every arm consumes, and the `Enabled` listener gate an emitting fold reads before that projection.
- Entry: `Of` mounts any number of `(meter, rows)` pairs against one cell store, so a one-meter root is its one-element call, and it returns the typed result — a row declared twice across two meters binds a second handle for one name, which is the defect the roster proof already legislates and the mount now refuses BEFORE any handle is created. `Write` and `Level` are the pushed and pulled measurement entries; `Level` carries one optional key so a scalar cell, a partitioned family entry, and an unpartitioned one ride one signature; `Bind` is `Level`'s registered peer.
- Auto: `Write` dispatches through `row.Form.Switch` and then `row.Kind.Switch` — the two generated total folds over the axes the declaration already closes — so the four pulled arms ARE the polarity refusal, no separate polarity test precedes them, and the instrument cast is total because the mint built exactly that (form × kind) pair. Both discriminants read off the declaration rather than off the bound handle's shape, so a polarity breach the row declares files as itself instead of under the type-mismatch verdict.
- Auto: both indexes derive from the one roster, so no pair can disagree: `Seats` answers a declared row's mount for the write plane, and `Declared` answers a published handle's row for the listener plane. Handle identity IS the listener index's key comparison, so the reference probe a tally hand-wrote beside a name lookup has no reason to exist.
- Law: the optional key and the tag shape are the discriminants, never a `bool` beside them — the `Option` and the tag set already answer everything a `keyed:` flag re-describes.
- Law: a measurement crosses as `double` and the ROW decides its storage type — the read plane already widens every measurement to `double`, the cell store already holds `double` at either key half, and a registered probe hands one back too, so NO write or registration entry carries a type parameter and the `(row, Type)` slot a caller-chosen carrier forced has no spelling. `Reader<T>` alone stays generic, because the mint chose `T` and the reader answers the instrument the mint built.
- Law: `Tags` is the ONE stack-allocated projection every write arm consumes and takes tenancy EXPLICITLY — the root row's tags are empty, so a single-tenant process mints no tenant dimension and a page-local baggage read has no reason to exist. The tenant-free arity is a LANGUAGE constraint, never a knob: the fact span is `params` and must trail, so a defaulted leading tenant cannot be spelled and the two arities are one call the compiler forces apart; a `Tenancy.None` composition therefore reaches the shorter one and every tenanted plane the longer, and neither reconstructs a value the other supplies.
- Exemption: `Tags` builds through a mutable `TagList` `Add` loop — the BCL type inlines eight tags before its own spill and no fold member reaches its `in` write overloads, so the statement form is the whole point of the projection.
- Output: every entry returns the typed result — an unmounted row, a pushed-versus-pulled polarity breach, and a key handed to a scalar pulled row each land a refusal carrying the offending row, so a measurement never disappears into a silent no-op and never throws a lookup exception into an emitting fold.
- Packages: LanguageExt.Core, BCL inbox (`System.Collections.Frozen`, `System.Diagnostics`, `System.Diagnostics.Metrics`, `System.Numerics`).
- Growth: a new level family is one `Level` write site and one `Levels` declaration; a ninth instrument family breaks `Write`'s `Switch` at compile time.
- Boundary: `InstrumentKind.Pulled` is the enforced column, and `LevelCells`'s writes are assembly-internal, so an ungated level write cannot be composed from any consuming package.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Numerics;

namespace Rasm.Domain;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record InstrumentSet(Seq<(InstrumentSpec Row, Instrument Handle)> Mounts, LevelCells Cells) {
    private FrozenDictionary<InstrumentSpec, (InstrumentSpec Row, Instrument Handle)> Seats { get; } =
        Mounts.ToFrozenDictionary(static seat => seat.Row);

    private FrozenDictionary<Instrument, InstrumentSpec> Declared { get; } =
        Mounts.ToFrozenDictionary(static seat => seat.Handle, static seat => seat.Row);

    internal Option<InstrumentSpec> Published(Instrument handle) =>
        Declared.TryGetValue(handle, out InstrumentSpec? row) ? Some(row) : None;

    public static Fin<InstrumentSet> Of(
        LevelCells cells, params ReadOnlySpan<(Meter Meter, Seq<InstrumentSpec> Rows)> mounts) {
        Seq<(Meter Meter, InstrumentSpec Row)> declared = toSeq(mounts.ToArray())
            .Bind(mount => mount.Rows.Map(row => (mount.Meter, Row: row))).Strict();
        Seq<InstrumentSpec> collided = declared.Collisions(static pair => pair.Row);
        return collided.IsEmpty
            ? Fin.Succ(new InstrumentSet(
                Mounts: declared.Map(pair => (
                    Row: pair.Row, Handle: pair.Row.Form.Mint(pair.Row, pair.Meter, cells))).Strict(),
                Cells: cells))
            : Fin.Fail<InstrumentSet>(new KernelFault.InvalidValue(
                Label: string.Join(", ", collided.Map(static row => row.Name)),
                Requirement: "one mount per declaration row across every contributed meter"));
    }

    public Fin<Unit> Write(InstrumentSpec row, double measurement, in TagList tags = default) =>
        !Seats.TryGetValue(row, out (InstrumentSpec Row, Instrument Handle) seat) ? Fin.Fail<Unit>(Unmounted(row))
            : row.Form.Switch(
                state: (Seat: seat, Value: measurement, Tags: tags),
                whole: static bind => Pushed(bind.Seat, long.CreateSaturating(bind.Value), in bind.Tags),
                real: static bind => Pushed(bind.Seat, bind.Value, in bind.Tags));

    public Fin<Unit> Level(InstrumentSpec row, double value, Option<string> key = default) =>
        Pulled(row, key).Map(admitted => Cells.Level(admitted, key, value));

    public Fin<IDisposable> Bind(InstrumentSpec row, Func<double> read, in TagList tags = default) {
        KeyValuePair<string, object?>[] stamped = [.. tags];
        return Pulled(row, toSeq(stamped).Map(static tag => tag.Key).Head)
            .Bind(admitted => Cells.Bind(admitted, read, stamped));
    }

    public bool Enabled(Seq<InstrumentSpec> rows) =>
        rows.Exists(row => !Seats.TryGetValue(row, out (InstrumentSpec Row, Instrument Handle) seat) || seat.Handle.Enabled);

    public static TagList Tags(TenantContext tenant, params ReadOnlySpan<(string Slot, object? Value)> facts) {
        TagList row = Tags(facts: facts);
        foreach (KeyValuePair<string, object?> tag in tenant.Tags) { row.Add(tag.Key, tag.Value); }
        return row;
    }
    public static TagList Tags(params ReadOnlySpan<(string Slot, object? Value)> facts) {
        TagList row = default;
        foreach ((string slot, object? value) in facts) { row.Add(slot, value); }
        return row;
    }

    private Fin<InstrumentSpec> Pulled(InstrumentSpec row, Option<string> key) =>
        !Seats.ContainsKey(row) ? Fin.Fail<InstrumentSpec>(Unmounted(row))
        : !row.Kind.Pulled ? Fin.Fail<InstrumentSpec>(new KernelFault.InvalidValue(
            Label: row.Name, Requirement: "a pulled instrument row"))
        : key.IsSome && !row.Kind.Equals(InstrumentKind.Levels) ? Fin.Fail<InstrumentSpec>(new KernelFault.InvalidValue(
            Label: row.Name, Requirement: "a keyed levels family"))
        : Fin.Succ(row);

    private static Fin<Unit> Pushed<T>((InstrumentSpec Row, Instrument Handle) seat, T value, in TagList tags) where T : struct, INumberBase<T> =>
        seat.Row.Kind.Switch(
            state: (Seat: seat, Value: value, Tags: tags),
            count: static bind => {
                ((Counter<T>)bind.Seat.Handle).Add(bind.Value, in bind.Tags);
                return Fin.Succ(unit);
            },
            delta: static bind => {
                ((UpDownCounter<T>)bind.Seat.Handle).Add(bind.Value, in bind.Tags);
                return Fin.Succ(unit);
            },
            distribution: static bind => {
                ((Histogram<T>)bind.Seat.Handle).Record(bind.Value, in bind.Tags);
                return Fin.Succ(unit);
            },
            reading: static bind => {
                ((Gauge<T>)bind.Seat.Handle).Record(bind.Value, in bind.Tags);
                return Fin.Succ(unit);
            },
            total: static bind => Fin.Fail<Unit>(Polarity(bind.Seat.Row)),
            balance: static bind => Fin.Fail<Unit>(Polarity(bind.Seat.Row)),
            level: static bind => Fin.Fail<Unit>(Polarity(bind.Seat.Row)),
            levels: static bind => Fin.Fail<Unit>(Polarity(bind.Seat.Row)));

    private static Error Unmounted(InstrumentSpec row) =>
        new KernelFault.InvalidValue(Label: row.Name, Requirement: "a mounted instrument row");

    private static Error Polarity(InstrumentSpec row) =>
        new KernelFault.InvalidValue(Label: row.Name, Requirement: "a pushed instrument row");
}
```

## [05]-[TALLY]

- Owner: `ReadingCell` is the one measured shape — one accumulator per `(row, digest)` pair, the digest framing its tag set — nesting the branch's `Stat` recurrence and adding the two columns `Stat` cannot express; `InstrumentReading` is the per-row projection with its three read states; `TallyState` is the one fold state cells, census, and refusals advance in together; `InstrumentTally` is the backend-free read plane over a mounted set.
- Cases: three read states, never two — a row carrying cells is MEASURED, a row with neither cells nor a refusal is QUIET, and a row whose probe or measurement refused is BROKEN. QUIET and BROKEN stay distinct, so a doctor archive separates a producer that never ran from one that raised on every collection.
- Entry: `Of(set, ceiling)` opens the read plane under its distinct-series bound and `Read()` is its one entry, driving the observables then projecting every declared row.
- Auto: admission is HANDLE identity through the set's own listener index, so a foreign instrument sharing a declared name never enters the read. Pushed measurements ACCUMULATE their sum and pulled ones REPLACE it, because an observable republishes its whole value each collection and accumulating one compounds a level into a total no producer measured; count, minimum, and maximum ride `Stat` on both arms.
- Auto: admission and fold run in ONE swap step, so the ceiling test reads the map the fold is about to write and two racing measurements cannot both seat the cell that crosses it. Standing series fold in place, a new one seats while the map is under the tally ceiling AND the row's own declared `Ceiling` bound, and every further series folds onto its row's own overflow cell — bounded past either ceiling by the declared row count alone, never by the tag space. Per-row census rides the fold state, so the new-series branch reads a count instead of re-walking every key.
- Law: `Stat`'s own count floor IS the seed guard — `Update` refuses an invalid prior and a zero-count cell is exactly that, so the first measurement mints through `Stat.Of` and no arm fabricates a minimum no producer measured (`Domain/stats` is the branch's one moment mint under `Rasm` RULINGS `[02]`).
- Law: a non-finite measurement REFUSES rather than seating. `Stat` rejects it, the admission fails, and the cause seats on the row's refusal half of the same swap, so a producer recording `NaN` reads as a named defect instead of a cell whose moments are quietly undefined.
- Law: every capture funnels through `Try.lift`, so a cancelled collection keeps `KernelFault.Cancelled`; the drive is ATOMIC in the fold state, because a cycle that throws has already seated whatever callbacks ran ahead of the raising one and the surviving partial fold is a half-filled map no later read can tell from a complete cycle.
- Output: `InstrumentReading` carries the row, its cells, and its joined refusal; the read plane ACCUMULATES and never emits, so a tally reading a stream is not a second truth beside the instruments and a projection written back onto an instrument from a reading is the deleted form.
- Packages: LanguageExt.Core, BCL inbox (`System.Diagnostics.Metrics`).
- Growth: a new read moment is one `ReadingCell` column; a tightened diagnostic memory bar is one `ceiling` value at the arming composition; a per-row cardinality bound is one `Ceiling` value on the declaring row.
- Boundary: the tally is a DIAGNOSTIC composition an operating profile arms and disposes, never a standing emission leg — it holds one accumulator per (row, tag set) for the life of the listener, bounded by a ceiling the arming composition supplies, and the arming seat stays a policy row at the app platform.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Diagnostics.Metrics;

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ReadingCell(
    Seq<KeyValuePair<string, object?>> Tags, Stat<Scalar> Summary, double Sum, double Last) {
    internal static Fin<ReadingCell> Advance(
        Option<ReadingCell> prior, Seq<KeyValuePair<string, object?>> tags,
        double measurement, InstrumentKind kind) =>
        Scalar.From(measurement).Bind(sample => prior.Match(
            Some: cell => Stat<Scalar>.Update(prior: cell.Summary, sample: sample)
                .Map(summary => cell with {
                    Summary = summary,
                    Sum = kind.Pulled ? measurement : cell.Sum + measurement,
                    Last = measurement,
                }),
            None: () => Stat<Scalar>.Of(values: Seq(sample))
                .Map(summary => new ReadingCell(Tags: tags, Summary: summary, Sum: measurement, Last: measurement))));
}

public sealed record InstrumentReading(InstrumentSpec Row, Seq<ReadingCell> Cells, Option<Error> Refused);

internal readonly record struct TallyState(
    HashMap<(InstrumentSpec Row, UInt128 Key), ReadingCell> Cells,
    HashMap<InstrumentSpec, int> Census,
    HashMap<InstrumentSpec, Error> Refused);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class InstrumentTally : IDisposable {
    public const string OverflowSlot = "otel.metric.overflow";

    private static readonly Seq<KeyValuePair<string, object?>> Overflow =
        Seq(new KeyValuePair<string, object?>(OverflowSlot, true));

    private static readonly UInt128 OverflowKey = Keyed(tags: Overflow);

    private readonly Atom<TallyState> plane = Atom(new TallyState(
        Cells: HashMap<(InstrumentSpec Row, UInt128 Key), ReadingCell>(),
        Census: HashMap<InstrumentSpec, int>(),
        Refused: HashMap<InstrumentSpec, Error>()));

    private readonly MeterListener listener = new();
    private readonly InstrumentSet set;
    private readonly int ceiling;

    private InstrumentTally(InstrumentSet mounted, int bound) => (set, ceiling) = (mounted, bound);

    public static InstrumentTally Of(InstrumentSet set, int ceiling = 2048) {
        InstrumentTally tally = new(set, ceiling);
        tally.listener.InstrumentPublished = (instrument, listening) => {
            if (set.Published(instrument).IsSome) { listening.EnableMeasurementEvents(instrument, state: null); }
        };
        MeasureForm.Items.AsIterable().Iter(form => form.Heard(tally.listener, tally.Fold));
        tally.listener.Start();
        return tally;
    }

    public Fin<Seq<InstrumentReading>> Read() {
        TallyState settled = plane.Value;
        return Try.lift(listener.RecordObservableInstruments).Run().Bind(static inner => inner)
            .MapFail(cause => (ignore(plane.Swap(_ => settled)), cause).Item2)
            .Map(_ => {
                TallyState held = plane.Value;
                HashMap<InstrumentSpec, Seq<ReadingCell>> byRow = held.Cells.AsIterable().Fold(
                    HashMap<InstrumentSpec, Seq<ReadingCell>>(),
                    static (rows, pair) => rows.AddOrUpdate(pair.Key.Row, cell => pair.Value.Cons(cell), () => [pair.Value]));
                return set.Mounts.Map(seat => new InstrumentReading(
                    Row: seat.Row,
                    Cells: byRow.Find(seat.Row).IfNone(Seq<ReadingCell>()),
                    Refused: Seq(held.Refused.Find(seat.Row), set.Cells.Raised(seat.Row)).Somes()
                        .Fold(Option<Error>.None, static (seat, cause) => Some(seat.Match(Some: first => first + cause, None: () => cause)))));
            });
    }

    public void Dispose() => listener.Dispose();

    private static UInt128 Keyed(Seq<KeyValuePair<string, object?>> tags) =>
        ContentHash.Of(tags, static (rows, writer) => writer.Sorted(
            rows: rows,
            key: static tag => tag.Key,
            order: StringComparer.Ordinal,
            field: static (tag, framed) => framed.String(tag.Key).String(tag.Value?.ToString() ?? string.Empty)));

    private void Fold(Instrument instrument, double measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? _) {
        if (set.Published(instrument) is not { IsSome: true, Case: InstrumentSpec declared }) { return; }
        Seq<KeyValuePair<string, object?>> row = toSeq(tags.ToArray());
        (InstrumentSpec Row, UInt128 Key) at = (declared, Keyed(tags: row));
        ignore(plane.Swap(held => Seated(held, at, row, measurement)));
    }

    private TallyState Seated(
        TallyState held, (InstrumentSpec Row, UInt128 Key) at, Seq<KeyValuePair<string, object?>> tags, double measurement) {
        bool standing = held.Cells.ContainsKey(at);
        bool seatable = held.Cells.Count < ceiling && held.Census.Find(at.Row).IfNone(0) < at.Row.Ceiling.IfNone(ceiling);
        (InstrumentSpec Row, UInt128 Key) key = standing || seatable ? at : (at.Row, OverflowKey);
        return ReadingCell.Advance(
                prior: held.Cells.Find(), tags: standing || seatable ? tags : Overflow,
                measurement: measurement, kind: key.Row.Kind, key: TallyOp)
            .Match(
                Succ: cell => held with {
                    Cells = held.Cells.AddOrUpdate(cell),
                    Census = standing ? held.Census : held.Census.AddOrUpdate(key.Row, static seats => seats + 1, static () => 1),
                },
                Fail: cause => held with {
                    Refused = held.Refused.AddOrUpdate(key.Row, seat => seat + cause, () => cause),
                });
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
