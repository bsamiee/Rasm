# 1. Make the failure posture keyless and park once

From:

`libs/dotnet/Rasm/.planning/Interaction/clock.md:[02]-[CLOCK] code fence, FaultPosture declaration, lines 41–50`
```csharp
[SmartEnum<int>]
public sealed partial class FaultPosture {
    public static readonly FaultPosture Halt = new(key: 0,
        settle: static (faults, point, cause) =>
            (ignore(faults.Park(point: point, cause: cause)), Fin.Fail<Unit>(cause)).Item2);
    public static readonly FaultPosture Continue = new(key: 1,
        settle: static (faults, point, cause) =>
            (ignore(faults.Park(point: point, cause: cause)), Fin.Succ(unit)).Item2);

    [UseDelegateFromConstructor] internal partial Fin<Unit> Settle(FaultCell faults, HookId point, Error cause);
}
```

To:

```csharp
[SmartEnum]
public sealed partial class FaultPosture {
    public static readonly FaultPosture Halt = new(settle: Fin.Fail<Unit>);
    public static readonly FaultPosture Continue = new(settle: static _ => Fin.Succ(unit));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Settle(Error cause);
}
```

Why: The rows have no wire, persistence, lookup, or host identity, and fault parking is identical in both behaviors. The posture only decides whether the failure remains failed.

Change: Use the keyless generated owner and move the single park operation to tick recovery.

Delta: -3 authored LOC and -2 delegate parameters; no authored type/member reduction, while the generated key, keyed lookup, conversion, parsing, and formatting surface is removed.

# 2. Keep only cadence-specific pulse evidence

From:

`libs/dotnet/Rasm/.planning/Interaction/clock.md:[03]-[BEAT] code fence, PulseBeat declaration, lines 137–149`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct PulseBeat(MonotonicBeat Evidence, TimeSpan Interval, TimeSpan Drift, long Missed) : IValidityEvidence {
    public static PulseBeat Of(MonotonicBeat beat, Option<PulseBeat> prior, PositiveMagnitude cadence) => new(
        Evidence: beat,
        Interval: beat.Delta,
        Drift: beat.Delta - TimeSpan.FromSeconds(value: cadence.Value),
        Missed: prior.Map(held => beat.Ordinal - held.Evidence.Ordinal - 1L).IfNone(noneValue: 0L));

    public bool IsValid => ValidityClaim.All(
        Interval >= TimeSpan.Zero,
        Missed >= 0L,
        ValidityClaim.Evidence(evidence: Optional(Evidence)));
}
```

To:

```csharp
public readonly record struct PulseBeat : IValidityEvidence {
    internal PulseBeat(MonotonicBeat evidence, BeatSeed seed, PositiveMagnitude cadence) =>
        (Evidence, Drift, Missed) = (evidence,
            evidence.Delta - TimeSpan.FromSeconds(cadence.Value),
            seed.Switch(state: evidence.Ordinal,
                origin: static (long _, MonotonicStamp _) => 0L,
                previous: static (long ordinal, MonotonicBeat prior) => ordinal - prior.Ordinal - 1L));
    public MonotonicBeat Evidence { get; }
    public TimeSpan Drift { get; }
    public long Missed { get; }
    public bool IsValid => Evidence is { IsValid: true } && Missed >= 0L;
}
```

Why: `Evidence.Delta` already is the interval, and `BeatSeed` already carries either the origin or the exact predecessor. The public positional constructor and public factory allow unrelated evidence to be combined, while the layout attribute serves no interop boundary.

Change: Restrict construction to the clock, dispatch the existing seed exhaustively, remove the duplicate interval field, and retain default-struct rejection through `IValidityEvidence`.

Delta: 0 authored LOC, -4 public members (`PulseBeat(...)`, `Deconstruct`, `Interval`, and `Of`) and -1 stored field; no type reduction.

# 3. Use BeatSeed as the cursor state

From:

`libs/dotnet/Rasm/.planning/Interaction/clock.md:[02]-[CLOCK] code fence, ClockCursor/cursor declarations and Advance/Seat members, lines 54, 60, and 96–110`
```csharp
internal sealed record ClockCursor(BeatSeed Seed, Option<PulseBeat> Last);
private readonly Atom<ClockCursor> cursor;
private Fin<PulseBeat> Advance() =>
    from held in Fin.Succ(cursor.Value)
    from minted in timeline.Beat(seed: held.Seed, cadence: cadence)
    let pulse = PulseBeat.Of(beat: minted, prior: held.Last, cadence: cadence)
    from seated in Seat(
        observed: held.Seed,
        next: new ClockCursor(Seed: BeatSeed.Previous(minted), Last: Some(pulse)))
    select pulse;

private Fin<Unit> Seat(BeatSeed observed, ClockCursor next) =>
    Cell.Step(cursor, held => held.Seed == observed ? Some(next) : None, new KernelFault.InvalidResult()).Switch(
        committed: static _ => Fin.Succ(unit),
        ceded: static (_) => Fin.Fail<Unit>(new KernelFault.InvalidResult()),
        refused: static row => Fin.Fail<Unit>(row.Cause),
        contended: static (_) => Fin.Fail<Unit>(new KernelFault.InvalidResult()));
```

To:

```csharp
private readonly Atom<BeatSeed> cursor;
private Fin<PulseBeat> Advance() {
    BeatSeed observed = cursor.Value;
    return timeline.Beat(seed: observed, cadence: cadence).Bind(minted =>
        Cell.Step(cursor, held => held == observed ? Some(BeatSeed.Previous(minted)) : None,
            new KernelFault.InvalidResult()).Switch(
            committed: _ => Fin.Succ(new PulseBeat(minted, observed, cadence)),
            ceded: static _ => Fin.Fail<PulseBeat>(new KernelFault.InvalidResult()),
            refused: static row => Fin.Fail<PulseBeat>(row.Cause),
            contended: static _ => Fin.Fail<PulseBeat>(new KernelFault.InvalidResult())));
}
```

Why: `BeatSeed.Previous` already stores the predecessor `MonotonicBeat`; `ClockCursor.Last` duplicates that evidence. `Seat` only forwards one `Cell.Step` result.

Change: Store the generated seed union directly and return the pulse from the committed transition arm.

Delta: -5 authored LOC, -1 module-level type, -1 private member, and one allocation per committed tick; the cursor loses one duplicated field.

# 4. Return the clock directly and remove timer lifecycle

From:

`libs/dotnet/Rasm/.planning/Interaction/clock.md:[02]-[CLOCK] code fence, UiClock factory and lifecycle declarations, lines 68–78`
```csharp
public static Fin<Lease<UiClock>> Of(
    PositiveMagnitude cadence,
    Func<PulseBeat, Fin<Unit>> beat,
    Option<FaultPosture> posture = default,
    Option<FaultCell> faults = default,
    Option<MonotonicTimeline> clock = default);

public Fin<Unit> Start();
public Fin<Unit> Stop();
public Fin<Unit> Pause();
public Fin<Unit> Resume();
```

To:

```csharp
public static Fin<UiClock> Of(
    PositiveMagnitude cadence,
    Func<PulseBeat, Fin<Unit>> beat,
    FaultPosture posture,
    FaultCell faults,
    MonotonicTimeline timeline);
```

Why: Every `UiClock` is freshly constructed and owned, so `Lease<UiClock>.Borrowed` is unreachable. The owner holds no platform timer and therefore cannot implement start, stop, pause, or resume; optional defaults also permit a second timeline and fault cell despite the composition requiring shared instances.

Change: Return the disposable clock directly, require its composition-owned dependencies, and leave lifecycle on the platform timer attachment.

Delta: -4 authored LOC, -4 public members, and -1 carrier layer; no type reduction.

Ripples: `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/motion.md` must retain the `UiClock` beside the `TimerHold` returned by `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md`, drive `UiClock.Tick`, and dispose the timer attachment to stop. `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md` must let its timer attachment own pause, resume, and release while retaining the clock for ticks. Both call sites pass `FaultPosture`, `FaultCell`, and `MonotonicTimeline` directly.

# 5. Return the observer detacher directly

From:

`libs/dotnet/Rasm/.planning/Interaction/clock.md:[02]-[CLOCK] code fence, Tap and failure projections, lines 80–83`
```csharp
public Fin<Lease<IDisposable>> Tap(Action<PulseBeat> observer);
public Seq<IsolatedFault> Failures => faults.Parked;
public long Shed => faults.Shed;
```

To:

```csharp
public Fin<IDisposable> Tap(Action<PulseBeat> observer) =>
    Admit.Need(observer).Map(admitted => {
        Action<PulseBeat> registered = pulse => admitted(pulse);
        ignore(observers.Swap(held => held.Add(registered)));
        return (IDisposable)new HookDetacher(Detach: () =>
            ignore(observers.Swap(held => held.Filter(
                row => !ReferenceEquals(row, registered)).Strict())));
    });
```

Why: A tap always creates an owned detacher, so a lease adds an ownership case no caller can select. The identity-distinct registration ensures disposing one of two taps made with the same delegate removes only that tap. `Failures` and `Shed` merely forward the supplied `FaultCell` and omit its `Lost` reading.

Change: Return the detacher itself and remove partial fault-cell projections.

Delta: +5 authored LOC, -2 public members, and -1 carrier layer; no type reduction.

Ripples: `libs/dotnet/Rasm/.planning/Interaction/input.md` must change `OnClock`'s tap parameter to `Func<UiClock, Action<PulseBeat>, Fin<IDisposable>>`, remove the unused third parameter from the `UiSource.Beat` tap lambda, and retain the returned detacher directly.

# 6. Expose and simplify the host-driven tick

From:

`libs/dotnet/Rasm/.planning/Interaction/clock.md:[02]-[CLOCK] code fence, Tick and Publish members, lines 85–94`
```csharp
private Fin<Unit> Tick() =>
    (from pulse in Advance()
     from published in Fin.Succ(Publish(pulse: pulse))
     from settled in body(arg: pulse)
     select settled).Match(
        Succ: static _ => Fin.Succ(unit),
        Fail: cause => posture.Settle(faults: faults, point: Point, cause: cause));

private Unit Publish(PulseBeat pulse) => observers.Value.Fold(
    unit, (_, observer) => FaultGate.Isolate(faults: faults, publish: () => observer(obj: pulse)));
```

To:

```csharp
public Fin<Unit> Tick() =>
    Advance()
        .Bind(pulse => (Publish(pulse), body(pulse)).Item2)
        .BindFail(cause =>
            (ignore(faults.Park(Point, cause)), posture.Settle(cause)).Item2);

private Unit Publish(PulseBeat pulse) => observers.Value.Iter(
    observer => FaultGate.Isolate(faults, Point, () => observer(pulse)));
```

Why: The platform timer needs the result-returning tick callback. `Fin.Succ(Publish(...))` wraps an infallible value only to bind it, `Match` rebuilds the unchanged success branch, and `Fold` carries unused state. The existing `FaultGate` attributes every publisher's observer failure to the dispatch hook rather than the clock hook.

Change: Make `Tick` the public host callback, sequence publication directly, park once before posture recovery, iterate observers without an accumulator, and pass the owning hook identity to isolation.

Delta: -3 authored LOC; no type or member-count reduction.

Ripples: `libs/dotnet/Rasm/.planning/Interaction/dispatch.md` must replace `FaultGate.Isolate(FaultCell, Action)` with `FaultGate.Isolate(FaultCell, HookId, Action)` and delete its dispatch-wide private hook field. `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md` must accept `Func<Fin<Unit>>` and stop its `UITimer` on a failed tick without parking the same cause again. `libs/dotnet/Rasm/.planning/Interaction/paint.md` and `libs/dotnet/Rasm/.planning/Interaction/platform.md` must pass their own declared `HookId` values to isolation.
