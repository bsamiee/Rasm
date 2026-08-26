# `instrument.md` Surgical Refinement Audit

Scope: [`libs/dotnet/Rasm/.planning/Domain/instrument.md`](libs/dotnet/Rasm/.planning/Domain/instrument.md). Apply the moves in order. Each `From` excerpt is the current fence; later moves describe the intended form after earlier moves. No move changes another spec-sheet.

## 1. State optional invalidity directly

### Location

`InstrumentSpec.ValidateFactoryArguments`, anchored by `bounds.Map` and `ceiling.Map` at current lines 172-177.

### From

```csharp
&& bounds.Map(row =>
    string.Equals(row.Unit, unit, StringComparison.Ordinal)
    || (string.Equals(row.Unit, Buckets.Dimensionless, StringComparison.Ordinal) && unit.StartsWith('{')))
    .IfNone(true)
&& tag.IsSome == kind.Equals(InstrumentKind.Levels)
&& ceiling.Map(static bound => bound > 0).IfNone(true)
```

### To

```csharp
&& !bounds.Exists(row =>
    !string.Equals(row.Unit, unit, StringComparison.Ordinal)
    && !(string.Equals(row.Unit, Buckets.Dimensionless, StringComparison.Ordinal) && unit.StartsWith('{')))
&& tag.IsSome == kind.Equals(InstrumentKind.Levels)
&& !ceiling.Exists(static bound => bound <= 0)
```

### Effect

- Fenced LOC: `-1`.
- Symbols: unchanged.
- Logic: absence still passes; a present invalid bucket row or non-positive ceiling still refuses.

### API and consumer proof

`libs/dotnet/.api/api-languageext.md` owns `Option.Exists(Func<A,bool>)`; it returns `false` for `None`. Negating the invalid-present predicate is therefore truth-table equivalent to `Map(valid).IfNone(true)` and avoids two intermediate `Option<bool>` projections.

Ripple: none.

## 2. Reuse the BCL measurement callback and delete `ReadingFold`

### Location

The `ReadingFold` declaration at current line 108, `MeasureForm.Heard<T>`, and `InstrumentTally.Fold`.

### From

```csharp
internal delegate void ReadingFold(Instrument instrument, double measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags);

[UseDelegateFromConstructor]
internal partial void Heard(MeterListener listener, ReadingFold fold);

private static void Heard<T>(MeterListener listener, ReadingFold fold) where T : struct, INumberBase<T> =>
    listener.SetMeasurementEventCallback<T>(
        (instrument, measurement, tags, _) => fold(instrument, double.CreateSaturating(measurement), tags));
```

```csharp
private void Fold(Instrument instrument, double measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags) {
```

### To

```csharp
[UseDelegateFromConstructor]
internal partial void Heard(MeterListener listener, MeasurementCallback<double> fold);

private static void Heard<T>(MeterListener listener, MeasurementCallback<double> fold) where T : struct, INumberBase<T> =>
    listener.SetMeasurementEventCallback<T>((instrument, measurement, tags, state) =>
        fold(instrument, double.CreateSaturating(measurement), tags, state));
```

```csharp
private void Fold(Instrument instrument, double measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? _) {
```

### Effect

- Fenced LOC: `-1` in final formatting.
- Symbols: `-1` internal module-level delegate type.
- Logic: the form row still widens every supported measurement type to `double`; the listener state slot passes through unchanged and remains unused by the tally.

### API and consumer proof

`libs/dotnet/.api/api-diagnostics-metrics.md` defines `MeasurementCallback<T>(Instrument, T, ReadOnlySpan<KeyValuePair<string,object?>>, object?)` and `MeterListener.SetMeasurementEventCallback<T>`. `ReadingFold` is its `double` specialization with the state argument removed, and no other file names it.

Ripple: remove `ReadingFold` from `[01]-[INDEX]` and `[02]-[SPEC]` when the fence lands.

## 3. Use `AtomHashMap` at key grain and absorb attach/retire

### Location

`LevelCells`, anchored by the three `Atom<HashMap<...>>` fields at current lines 233-237 and the `Level`/`Bind`/`Attached`/`Retired` block at current lines 239-262.

### From

```csharp
private readonly Atom<HashMap<(InstrumentSpec Row, Option<string> Key), double>> cells =
    Atom(HashMap<(InstrumentSpec Row, Option<string> Key), double>());
private readonly Atom<HashMap<InstrumentSpec, Seq<LevelProbe>>> probes =
    Atom(HashMap<InstrumentSpec, Seq<LevelProbe>>());
private readonly Atom<HashMap<InstrumentSpec, Error>> raised = Atom(HashMap<InstrumentSpec, Error>());
```

```csharp
internal Unit Level(InstrumentSpec row, Option<string> key, double value) =>
    ignore(cells.Swap(held => held.AddOrUpdate((row, key), value)));

internal Fin<IDisposable> Bind(InstrumentSpec row, Func<double> read, KeyValuePair<string, object?>[] tags, Op key) =>
    key.Need(read).Map(admitted => Attached(row: row, probe: new LevelProbe(Tags: tags, Read: admitted)));

private IDisposable Attached(InstrumentSpec row, LevelProbe probe) {
    ignore(probes.Swap(held => held.AddOrUpdate(row, live => live.Add(probe), () => Seq(probe))));
    return new HookDetacher(Detach: () => ignore(probes.Swap(held => Retired(held, row, probe))));
}

private static HashMap<InstrumentSpec, Seq<LevelProbe>> Retired(
    HashMap<InstrumentSpec, Seq<LevelProbe>> held, InstrumentSpec row, LevelProbe probe) =>
    held.Find(row).Map(live => live.Filter(entry => !ReferenceEquals(entry, probe)).Strict()).Match(
        Some: live => live.IsEmpty ? held.Remove(row) : held.AddOrUpdate(row, live),
        None: () => held);
```

### To

```csharp
private readonly AtomHashMap<(InstrumentSpec Row, Option<string> Key), double> cells =
    AtomHashMap(HashMap<(InstrumentSpec Row, Option<string> Key), double>());
private readonly AtomHashMap<InstrumentSpec, Seq<LevelProbe>> probes =
    AtomHashMap(HashMap<InstrumentSpec, Seq<LevelProbe>>());
private readonly AtomHashMap<InstrumentSpec, Error> raised = AtomHashMap(HashMap<InstrumentSpec, Error>());
```

```csharp
internal Unit Level(InstrumentSpec row, Option<string> key, double value) => cells.AddOrUpdate((row, key), value);

internal Fin<IDisposable> Bind(InstrumentSpec row, Func<double> read, KeyValuePair<string, object?>[] tags, Op key) =>
    key.Need(read).Map(admitted => {
        LevelProbe probe = new(Tags: tags, Read: admitted);
        probes.AddOrUpdate(row, live => live.Add(probe), () => Seq(probe));
        return (IDisposable)new HookDetacher(Detach: () => probes.SwapKey(row, live => live
            .Map(rows => rows.Filter(entry => !ReferenceEquals(entry, probe)).Strict())
            .Filter(rows => !rows.IsEmpty)));
    });
```

Then apply these direct read/write substitutions.

```csharp
public Option<Error> Raised(InstrumentSpec row) => raised.Value.Find(row);

probes.Value.ContainsKey(row)
cells.Value.AsIterable()
probes.Value.Find(row)
ignore(raised.Swap(held => held.AddOrUpdate(row, seat => seat + cause, () => cause)))
cells.Value.Find((row, Option<string>.None))
```

```csharp
internal Option<Error> Raised(InstrumentSpec row) => raised.Find(row);

probes.Find(row).IsSome
cells.AsIterable()
probes.Find(row)
raised.AddOrUpdate(row, seat => seat + cause, () => cause)
cells.Find((row, Option<string>.None))
```

These replace, respectively, `raised.Value.Find`, `probes.Value.ContainsKey`, `cells.Value.AsIterable`, `probes.Value.Find`, the `ignore(raised.Swap(...))` call, and `cells.Value.Find`.

### Effect

- Fenced LOC: `-4`.
- Symbols: `-2` private members (`Attached`, `Retired`); `Raised` leaves the public surface.
- Runtime shape: keyed writes no longer replace a whole immutable map; last-probe retirement remains one CAS commit and removes the empty row.

### API and consumer proof

`libs/dotnet/.api/api-languageext.md` makes `AtomHashMap` the keyed shared-state owner and explicitly rejects `Atom<HashMap<K,V>>` for per-key mutation. It proves `AddOrUpdate`, `Find`, `AsIterable`, and `SwapKey(K, Func<Option<V>,Option<V>>)`. No caller reads a mutation's post-state. `Raised` is consumed only by `InstrumentTally` in this assembly.

Ripple: `[03]-[MOUNT]` must continue to state set registration and last-probe removal without naming the retired whole-map carrier.

## 4. Collapse the three single-use read helpers

### Location

`LevelCells.Reader<T>`, `Keyed<T>`, `Probed<T>`, `Held<T>`, and `Cell<T>` at current lines 247-283. This move assumes Move 3.

### From

```csharp
public Func<IEnumerable<Measurement<T>>> Reader<T>(InstrumentSpec row, Option<string> tag = default)
    where T : struct, INumberBase<T> =>
    () => tag.Match(
        Some: key => Keyed<T>(row, key) + Probed<T>(row),
        None: () => probes.Find(row).IsSome ? Probed<T>(row) : Cell<T>(row));

private Seq<Measurement<T>> Keyed<T>(InstrumentSpec row, string tag) where T : struct, INumberBase<T> =>
    cells.AsIterable().Filter(pair => pair.Key.Row.Equals(row))
        .Map(pair => pair.Key.Key.Match(
            Some: key => new Measurement<T>(T.CreateSaturating(pair.Value), new KeyValuePair<string, object?>(tag, key)),
            None: () => new Measurement<T>(T.CreateSaturating(pair.Value))));

private Seq<Measurement<T>> Probed<T>(InstrumentSpec row) where T : struct, INumberBase<T> =>
    probes.Find(row).Map(live => live.Bind(probe => Held<T>(row, probe)).Strict())
        .IfNone(Seq<Measurement<T>>());

private Seq<Measurement<T>> Held<T>(InstrumentSpec row, LevelProbe probe) where T : struct, INumberBase<T> =>
    PullOp.Catch(() => Fin.Succ(Seq(new Measurement<T>(T.CreateSaturating(probe.Read()), probe.Tags)))).Match(
        Succ: static held => held,
        Fail: cause => (raised.AddOrUpdate(row, seat => seat + cause, () => cause),
            Seq<Measurement<T>>()).Item2);

private Seq<Measurement<T>> Cell<T>(InstrumentSpec row) where T : struct, INumberBase<T> =>
    cells.Find((row, Option<string>.None)).Match(
        Some: held => Seq(new Measurement<T>(T.CreateSaturating(held))),
        None: static () => Seq<Measurement<T>>());
```

### To

```csharp
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
    live.Bind(probe => PullOp.Catch(() =>
            Fin.Succ(Seq(new Measurement<T>(T.CreateSaturating(probe.Read()), probe.Tags))))
        .IfFail(cause => (raised.AddOrUpdate(row, seat => seat + cause, () => cause),
            Seq<Measurement<T>>()).Item2)).Strict();
```

### Effect

- Fenced LOC: `-2`.
- Symbols: `-3` private members (`Keyed`, `Held`, `Cell`); `Reader<T>` leaves the public surface.
- Logic: the scalar arm probes once; a present probe set still wins over the raw cell, a keyed family still unions keyed cells with probes, and each raising probe records its refusal without stopping later probes.

### API and consumer proof

`libs/dotnet/.api/api-languageext.md` proves `Option.Match`, `Option.Map`, `Option.ToSeq`, `Fin.IfFail`, and `Iterable.ToSeq`. The explicit `ToSeq()` is required before concatenating the keyed `Iterable<Measurement<T>>` with the probe `Seq<Measurement<T>>`; the deleted `Keyed<T>` return type previously performed that landing. Repo-wide, `MeasureForm.Bound<T>` is the only `Reader<T>` consumer.

Ripple: `[03]-[MOUNT]` Boundary should state that read, write, registration, and refusal access are assembly mechanics; `LevelCells` remains publicly constructible for composition.

## 5. Replace `Mounted` with its structural pair and remove the one-hop row mirror

### Location

The `Mounted` declaration at current lines 286-288 and `InstrumentSet` at current lines 316-410.

### From

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Mounted(InstrumentSpec Row, Instrument Handle);
```

```csharp
public sealed record InstrumentSet(Seq<Mounted> Mounts, LevelCells Cells) {
    private FrozenDictionary<InstrumentSpec, Mounted> Seats { get; } =
        Mounts.ToFrozenDictionary(static seat => seat.Row);

    public Seq<InstrumentSpec> Rows => Mounts.Map(static seat => seat.Row);

    public Option<InstrumentSpec> Published(Instrument handle) =>
        Declared.TryGetValue(handle, out InstrumentSpec? row) ? Some(row) : None;
```

```csharp
Mounts: declared.Map(pair => new Mounted(
    Row: pair.Row, Handle: pair.Row.Form.Mint(pair.Row, pair.Meter, cells))).Strict(),
```

```csharp
public Fin<Unit> Write(InstrumentSpec row, double measurement, in TagList tags = default) {
    if (!Seats.TryGetValue(row, out Mounted seat)) { return Fin.Fail<Unit>(Unmounted(row)); }
    return row.Form.Switch(
        state: (Seat: seat, Value: measurement, Tags: tags),
        whole: static bind => Pushed(bind.Seat, long.CreateSaturating(bind.Value), in bind.Tags),
        real: static bind => Pushed(bind.Seat, bind.Value, in bind.Tags));
}
```

```csharp
rows.Exists(row => !Seats.TryGetValue(row, out Mounted seat) || seat.Handle.Enabled);

private static Fin<Unit> Pushed<T>(Mounted seat, T value, in TagList tags)
```

### To

Delete `Mounted` and its now-empty `[MODELS]` separator.

```csharp
public sealed record InstrumentSet(Seq<(InstrumentSpec Row, Instrument Handle)> Mounts, LevelCells Cells) {
    private FrozenDictionary<InstrumentSpec, (InstrumentSpec Row, Instrument Handle)> Seats { get; } =
        Mounts.ToFrozenDictionary(static seat => seat.Row);

    internal Option<InstrumentSpec> Published(Instrument handle) =>
        Declared.TryGetValue(handle, out InstrumentSpec? row) ? Some(row) : None;
```

```csharp
Mounts: declared.Map(pair => (
    Row: pair.Row, Handle: pair.Row.Form.Mint(pair.Row, pair.Meter, cells))).Strict(),
```

```csharp
public Fin<Unit> Write(InstrumentSpec row, double measurement, in TagList tags = default) =>
    !Seats.TryGetValue(row, out (InstrumentSpec Row, Instrument Handle) seat) ? Fin.Fail<Unit>(Unmounted(row))
        : row.Form.Switch(
            state: (Seat: seat, Value: measurement, Tags: tags),
            whole: static bind => Pushed(bind.Seat, long.CreateSaturating(bind.Value), in bind.Tags),
            real: static bind => Pushed(bind.Seat, bind.Value, in bind.Tags));
```

```csharp
rows.Exists(row => !Seats.TryGetValue(row, out (InstrumentSpec Row, Instrument Handle) seat) || seat.Handle.Enabled);

private static Fin<Unit> Pushed<T>((InstrumentSpec Row, Instrument Handle) seat, T value, in TagList tags)
```

### Effect

- Fenced LOC: `-5`.
- Symbols: `-1` public module-level type and `-1` public projection member (`Rows`); `Published` leaves the public surface.
- Logic: named tuple fields preserve the ordered authority, structural equality, and every `Row`/`Handle` read; `Write` becomes one result expression without changing its unmounted-first admission order.

### API and consumer proof

`Mounted` has no admission, behavior, alternate construction, or identity beyond `(Row, Handle)`. Its only external shape consumer, `libs/dotnet/Rasm.AppUi/.planning/Diagnostics/proof.md:314-315`, reads `set.Mounts.Find(seat => seat.Row.Name == instrument)` then `seat.Handle`; it does not name the type, so the named tuple is source-identical there. `Rows` is consumed only by the tally and mirrors `Mounts`; Move 9 projects from `Mounts`. `Published` is called only by the tally. Explicit tuple types retain the repository's no-`var` law.

Ripple: remove `Mounted` from `[01]-[INDEX]` and `[03]-[MOUNT]`; describe `InstrumentSet` as holding row/handle pairs directly.

## 6. Make seeded and standing readings one fold

### Location

`ReadingCell.Of`/`Advance` at current lines 442-452 and their `Seated` call at current lines 523-528.

### From

```csharp
public static Fin<ReadingCell> Of(Seq<KeyValuePair<string, object?>> tags, double measurement, Op key) =>
    Scalar.From(measurement).Bind(sample => Stat<Scalar>.Of(values: Seq(sample), key: key))
        .Map(summary => new ReadingCell(Tags: tags, Summary: summary, Sum: measurement, Last: measurement));

public Fin<ReadingCell> Advance(double measurement, InstrumentKind kind, Op key) =>
    Scalar.From(measurement).Bind(sample => Stat<Scalar>.Update(prior: Summary, sample: sample, key: key))
        .Map(summary => this with {
            Summary = summary,
            Sum = kind.Pulled ? measurement : Sum + measurement,
            Last = measurement,
        });
```

```csharp
return held.Cells.Find(key)
    .Match(
        Some: cell => cell.Advance(measurement: measurement, kind: key.Row.Kind, key: TallyOp),
        None: () => ReadingCell.Of(
            tags: standing || seatable ? tags : Overflow, measurement: measurement, key: TallyOp))
    .Match(
```

### To

```csharp
internal static Fin<ReadingCell> Advance(
    Option<ReadingCell> prior, Seq<KeyValuePair<string, object?>> tags,
    double measurement, InstrumentKind kind, Op key) =>
    Scalar.From(measurement).Bind(sample => prior.Match(
        Some: cell => Stat<Scalar>.Update(prior: cell.Summary, sample: sample, key: key)
            .Map(summary => cell with {
                Summary = summary,
                Sum = kind.Pulled ? measurement : cell.Sum + measurement,
                Last = measurement,
            }),
        None: () => Stat<Scalar>.Of(values: Seq(sample), key: key)
            .Map(summary => new ReadingCell(Tags: tags, Summary: summary, Sum: measurement, Last: measurement))));
```

```csharp
return ReadingCell.Advance(
        prior: held.Cells.Find(key), tags: standing || seatable ? tags : Overflow,
        measurement: measurement, kind: key.Row.Kind, key: TallyOp)
    .Match(
```

### Effect

- Fenced LOC: unchanged.
- Symbols: `-1` member overall; the surviving fold becomes `internal` instead of public.
- Logic: scalar admission still runs once, absence still seeds through `Stat.Of`, presence still advances through `Stat.Update`, and pulled sums replace while pushed sums accumulate.

### API and consumer proof

`Option.Match` preserves the exact absent/present split. No external consumer calls either method; `Observability/bundles.md` consumes the resulting `ReadingCell` values through `InstrumentReading` only. The operation the tally actually needs is one fold over `Option<ReadingCell>`.

Ripple: `[05]-[TALLY]` may describe `ReadingCell` as one seeded-or-standing recurrence.

## 7. Replace `Series` with its private structural key

### Location

The `Series` declaration at current lines 436-437 and its uses in `TallyState`, `plane`, `Fold`, and `Seated`.

### From

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct Series(InstrumentSpec Row, UInt128 Key);

HashMap<Series, ReadingCell> Cells,

Cells: HashMap<Series, ReadingCell>(),

Series at = new(Row: declared, Key: Keyed(tags: row));

private TallyState Seated(
    TallyState held, Series at, Seq<KeyValuePair<string, object?>> tags, double measurement) {

Series key = standing || seatable ? at : new Series(Row: at.Row, Key: OverflowKey);
```

### To

```csharp
HashMap<(InstrumentSpec Row, UInt128 Key), ReadingCell> Cells,

Cells: HashMap<(InstrumentSpec Row, UInt128 Key), ReadingCell>(),

(InstrumentSpec Row, UInt128 Key) at = (declared, Keyed(tags: row));

private TallyState Seated(
    TallyState held, (InstrumentSpec Row, UInt128 Key) at, Seq<KeyValuePair<string, object?>> tags, double measurement) {

(InstrumentSpec Row, UInt128 Key) key = standing || seatable ? at : (at.Row, OverflowKey);
```

### Effect

- Fenced LOC: `-2`.
- Symbols: `-1` public module-level type.
- Logic: named `ValueTuple` equality and hashing preserve the exact `(InstrumentSpec, UInt128)` map identity.

### API and consumer proof

`Series` has one constructor, no invariant, no behavior, and no consumer beyond the tally's private map. The tuple is already the complete identity every read and write uses; the nominal wrapper adds semantic-only differentiation.

Ripple: remove `Series` from `[01]-[INDEX]` and `[05]-[TALLY]`; describe the accumulator key as `(row, digest)`.

## 8. Collapse the single-use ceiling constant without changing the contract

### Location

`InstrumentTally.DefaultCeiling` at current line 467 and `InstrumentTally.Of` at current line 485.

### From

```csharp
private const int DefaultCeiling = 2048;

public static InstrumentTally Of(InstrumentSet set, int ceiling = DefaultCeiling) {
```

### To

```csharp
public static InstrumentTally Of(InstrumentSet set, int ceiling = 2048) {
```

### Effect

- Fenced LOC: `-1`.
- Symbols: `-1` private constant.
- Logic: the public optional-argument contract and its default value remain byte-for-byte the same in call-site metadata.

### API and consumer proof

The constant has exactly one code read, in the default-argument declaration. Inlining preserves every existing and future omitted-argument call; unlike requiring `ceiling`, it does not move policy ownership or change source compatibility.

Ripple: none.

## 9. Inline the single-use read projection

### Location

`InstrumentTally.Read` at current lines 495-500 and `Projected` at current lines 538-547. This move assumes Moves 3, 5, and 7.

### From

```csharp
public Fin<Seq<InstrumentReading>> Read(Op key) {
    TallyState settled = plane.Value;
    return key.Catch(listener.RecordObservableInstruments)
        .MapFail(cause => (ignore(plane.Swap(_ => settled)), cause).Item2)
        .Map(_ => Projected(plane.Value));
}
```

```csharp
private Seq<InstrumentReading> Projected(TallyState held) {
    HashMap<InstrumentSpec, Seq<ReadingCell>> byRow = held.Cells.AsIterable().Fold(
        HashMap<InstrumentSpec, Seq<ReadingCell>>(),
        static (rows, pair) => rows.AddOrUpdate(pair.Key.Row, cell => pair.Value.Cons(cell), () => [pair.Value]));
    return set.Rows.Map(row => new InstrumentReading(
        Row: row,
        Cells: byRow.Find(row).IfNone(Seq<ReadingCell>()),
        Refused: toSeq(Seq(held.Refused.Find(row), set.Cells.Raised(row)).Somes())
            .Fold(Option<Error>.None, static (seat, cause) => Some(seat.Match(Some: first => first + cause, None: () => cause)))));
}
```

### To

```csharp
public Fin<Seq<InstrumentReading>> Read(Op key) {
    TallyState settled = plane.Value;
    return key.Catch(listener.RecordObservableInstruments)
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
```

### Effect

- Fenced LOC: unchanged in final formatting.
- Symbols: `-1` private member; the redundant `toSeq` admission also disappears.
- Logic: projection still runs only after a successful observable drive, snapshots `plane.Value` once, and restores the pre-drive state only on failure.

### API and consumer proof

`Projected` has one caller and no independent policy; it is exactly `Read`'s success projection. `Seq<Option<Error>>.Somes()` already returns `Seq<Error>`, so `toSeq` is an identity hop. `held.Cells.AsIterable()` remains required because the fold consumes both map key and value. `set.Mounts` is the one row authority after Move 5.

Ripple: none.

## Net effect

- Fenced LOC: `-16`.
- Module-level types: `-3` (`ReadingFold`, `Mounted`, `Series`).
- Members: `-9` (`Attached`, `Retired`, `Keyed`, `Held`, `Cell`, `InstrumentSet.Rows`, one of `ReadingCell.Of`/`Advance`, `DefaultCeiling`, `Projected`).
- Public surface additionally narrows `LevelCells.Raised`, `LevelCells.Reader<T>`, and `InstrumentSet.Published` to `internal`.
- No new type, enum, helper, abstraction, package, storage plane, or cross-file code edit.

## Protected non-moves

- Keep `Buckets.Advised<T>`: `libs/dotnet/Rasm/RULINGS.md:24` seats saturation on the bucket row; inlining it into `MeasureForm.Bound<T>` moves policy off its owner.
- Keep `InstrumentKind.Pulled`: Materials and AppUi consumers read declaration polarity before any handle exists, so `Instrument.IsObservable` cannot replace it.
- Keep `MeasureForm`: its two rows carry real mint and listener behavior columns; this is not a passive two-case vocabulary.
- Keep `LevelProbe` as a reference record: `ReferenceEquals` distinguishes separately registered equal payloads during detach; a tuple would require a new identity token.
- Keep `TallyState`: cells, census, and refusals restore as one atomic value after a failed observable cycle.
- Keep `TelemetryIdentity.Metered` and `Mint`: meter-only and paired source/meter results are distinct composition shapes, not suffix twins.
- Do not replace `Bind`'s first-tag polarity probe with `None` in this refinement queue: that changes scalar tagged-registration admission and needs a separately authorized semantic correction.
- Do not change overflow census routing here: correcting repeated overflow counts changes produced diagnostic state and is not an equivalent refinement.
