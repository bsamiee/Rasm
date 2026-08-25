# [RASM_CLOCK]

`Rasm.Interaction` owns the repeating UI beat: one leased clock over a host timer, one beat value carrying its monotonic evidence beside the drift and the misses the interval actually accumulated, and one posture deciding whether a failing tick halts the clock or lets it run on. Both host boundaries drove their own timer, minted their own cadence value object, and derived their own drift; all three were the same body and all three now read this owner while each boundary keeps only the timer lease its platform demands.

The beat composes the kernel timeline rather than re-minting temporal identity: `MonotonicBeat` is the evidence and `MonotonicTimeline.Beat` its one mint, so a host beat and a kernel span order against one clock and a flat host beat cannot fork the causal order. The chain's own tail is a cursor this owner holds, because `BeatSeed` names an origin or a predecessor and nothing else remembers which of the two the next tick owes.

## [01]-[INDEX]

- [02]-[CLOCK]: `UiClock`, `FaultPosture`, `ClockCursor` — the leased repeating clock, its lifecycle, the posture carrying its own failure settle, and the beat-chain tail it advances.
- [03]-[BEAT]: `PulseBeat` — the per-tick value carrying monotonic evidence, interval, drift, and miss count.

## [02]-[CLOCK]

- Owner: `UiClock` — the leased repeating clock. It mints on a cadence, a tick body, a posture, a `FaultCell`, and a timeline, and returns as `Lease<UiClock>` so the caller's disposal path and the clock's own teardown are one custody rather than two. `ClockCursor` is the internal chain tail: the seed the next beat advances from beside the beat it last published.
- Cases: `FaultPosture` closes the failure axis at two rows, each CARRYING its settle — `Halt` parks the cause and fails the tick, `Continue` parks it and lets the beat advance. A mirror bool restating the key buys a name and leaves every consumer to re-derive the consequence; a caller-supplied boolean cannot state which of the two a consumer chose, and both boundaries chose differently.
- Entry: `Of(cadence, beat, posture, faults, clock, key)` mints; `Start`/`Stop`/`Pause`/`Resume` are the lifecycle; `Tap` leases a beat observer; `Failures` reads the cell's parked refusals and `Shed` the parks the bound dropped.
- Auto: the cursor transition takes the kernel default `Cell.SwapBudget` (branch RULINGS `[02]`) — a guarded step spends no budget of its own, and a one-member shell beside the kernel row is two authorities over one ceiling.
- Auto: the cadence admits as `PositiveMagnitude` (`Numerics/atoms`) — the kernel's one positive-scalar owner — so no second cadence value object exists. The census proposed one; it never mints, because a cadence is a positive magnitude in seconds and nothing about a UI timer makes it a distinct invariant.
- Auto: the cursor advances through `Cell.Step` GUARDED on the seed the mint measured, so a tick that raced a `Stop` reads its transition case rather than assuming a swap, and the seed it hands `MonotonicTimeline.Beat` is `Origin` exactly once and `Previous` on every tick after. The compare-and-swap arm is the rejected placement: its body re-runs on every contended retry and re-installs a candidate derived from a predecessor the winner already replaced, so the seed-moved corner has to DECLINE rather than recompute. The timeline's own tail gate refuses a replayed predecessor, so a doubled host callback lands a typed refusal instead of a duplicate ordinal.
- Law: the clock owns identity and the host owns the timer. `UiClock` advances the beat, derives the drift, counts the misses, and applies the posture; the boundary supplies the platform lease — a display link, a `UITimer`, an idle callback — and does nothing else. A boundary that computes a drift or a miss count is re-deriving what the beat already carries.
- Law: the clock takes a `MonotonicTimeline`, never a bare `TimeProvider`. A provider is the timeline's own admission argument, and a clock holding one would mint a second timeline whose stamps no kernel span can order against the first. NAMED LOSS: a caller supplying a test provider now supplies `MonotonicTimeline.Of(provider, key)` instead. Witness: a fake-clock test seats `MonotonicTimeline.Of(new FakeTimeProvider(), key)` and passes the timeline.
- Law: a failing tick NEVER silently stops, and the posture ROW decides which way. `Settle` parks the cause on the cell under both rows and answers the rail — `Continue` succeeds and the beat advances, `Halt` fails and the error is the terminal reading — so the tick fold names no posture and a third posture is one row. A clock that dies quietly is indistinguishable from a host that stopped scheduling it.
- Law: the fault sink is the branch's `FaultCell` and never a raw `Action<Error>`. A `void` delegate licenses a silent discard and grows nothing a consumer can bound; the cell is a bounded ring whose parks, sheds, and declined parks all read as numbers, and it is the ARGUMENT `FaultRail.Isolate` demands — which is what makes the stated composition spellable and spelled at `Publish`.
- Law: an observer raise never fails the tick that fed it — publication runs through `FaultRail.Isolate` on this clock's own cell, exactly as the dispatch observers do, because a beat observer that can fail the clock turns instrumentation into a liveness dependency.
- Law: the missed count is MEASURED from the beat ordinal, never assumed. `MonotonicBeat.Ordinal` counts CADENCE PERIODS (`Parametric/projections` `[05]`), so a host that coalesces ticks under load leaves a gap in the ordinal and the gap IS the miss count — a wall-clock interval divided at the consumer is the deleted form.
- Receipt: `PulseBeat` per tick, `Failures` as the cell's bounded parked refusals beside `Shed`; neither is a return value, so a caller that only wants the beat pays nothing for the history.
- Packages: Eto.Forms for the timer surface; LanguageExt.Core for the rails and the lease; `Parametric/projections` for `MonotonicTimeline`, `BeatSeed`, and `MonotonicBeat`; `Domain/hooks` for `FaultCell`, `IsolatedFault`, and `HookId`.
- Growth: a new lifecycle verb is one member; a new posture is one row carrying its own settle arm, and no consumer edits.
- Boundary: the platform timer's construction, its disposal, and its run-loop mode are the boundary's — `CADisplayLink` lifecycle, `UITimer` disposal, and idle-callback registration never enter this owner, and this owner never holds a live host timer past its lease.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record ClockCursor(BeatSeed Seed, Option<PulseBeat> Last);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class UiClock : IDisposable {
    private static readonly HookId Rail = HookId.Create(value: "rasm.kernel.interaction.clock");

    private readonly Atom<ClockCursor> cursor;
    private readonly Atom<Seq<Action<PulseBeat>>> observers = Atom(Seq<Action<PulseBeat>>());
    private readonly PositiveMagnitude cadence;
    private readonly MonotonicTimeline timeline;
    private readonly FaultPosture posture;
    private readonly Func<PulseBeat, Fin<Unit>> body;
    private readonly FaultCell faults;

    [BoundaryAdapter]
    public static Fin<Lease<UiClock>> Of(
        PositiveMagnitude cadence,
        Func<PulseBeat, Fin<Unit>> beat,
        Option<FaultPosture> posture = default,
        Option<FaultCell> faults = default,
        Option<MonotonicTimeline> clock = default,
        Op? key = null);

    public Fin<Unit> Start(Op key);
    public Fin<Unit> Stop(Op key);
    public Fin<Unit> Pause(Op key);
    public Fin<Unit> Resume(Op key);

    public Fin<Lease<IDisposable>> Tap(Action<PulseBeat> observer, Op key);

    public Seq<IsolatedFault> Failures => faults.Parked;
    public long Shed => faults.Shed;

    private Fin<Unit> Tick(Op key) =>
        (from pulse in Advance(key: key)
         from published in Fin.Succ(Publish(pulse: pulse, key: key))
         from settled in body(arg: pulse)
         select settled).Match(
            Succ: static _ => Fin.Succ(unit),
            Fail: cause => posture.Settle(faults: faults, point: Rail, cause: cause));

    private Unit Publish(PulseBeat pulse, Op key) => observers.Value.Fold(
        unit, (_, observer) => FaultRail.Isolate(faults: faults, publish: () => observer(obj: pulse), key: key));

    private Fin<PulseBeat> Advance(Op key) =>
        from held in Fin.Succ(cursor.Value)
        from minted in timeline.Beat(seed: held.Seed, cadence: cadence, key: key)
        let pulse = PulseBeat.Of(beat: minted, prior: held.Last, cadence: cadence)
        from seated in Seat(
            observed: held.Seed,
            next: new ClockCursor(Seed: BeatSeed.Previous(minted), Last: Some(pulse)),
            key: key)
        select pulse;

    private Fin<Unit> Seat(BeatSeed observed, ClockCursor next, Op key) =>
        Cell.Step(cursor, held => held.Seed == observed ? Some(next) : None, key.InvalidResult()).Switch(
            state: key,
            committed: static (_, _) => Fin.Succ(unit),
            ceded: static (op, _) => Fin.Fail<Unit>(op.InvalidResult()),
            refused: static (_, row) => Fin.Fail<Unit>(row.Cause),
            contended: static (op, _) => Fin.Fail<Unit>(op.InvalidResult()));

    public void Dispose();
}
```

## [03]-[BEAT]

- Owner: `PulseBeat` — the per-tick value: the kernel `MonotonicBeat` as its temporal evidence, the interval the tick actually spanned, the drift against the declared cadence, and the count of beats the host coalesced away.
- Auto: interval, drift, and misses all DERIVE at mint from the beat and its predecessor — the interval is the beat's own delta, the drift is that delta against the declared period, and the miss count is `beat.Ordinal − prior.Ordinal − 1`. A stored drift a caller sets is a fabricated measurement.
- Law: the first beat of a sequence has no predecessor and therefore no misses — absence reads as zero here because the ordinal gap is undefined against an origin, not because zero was substituted for a measurement.
- Law: temporal identity is the kernel `MonotonicBeat`'s and this value extends it with cadence-only columns — a flat host beat carrying its own timestamp re-mints the identity the branch already federates (branch RULINGS `[02]`).
- Law: drift is SIGNED. A tick that fired early reads negative and a tick that fired late reads positive, so a host running fast is distinguishable from a host running slow; an absolute drift collapses the two directions a pace correction has to tell apart.
- Law: the beat is an ordinary value with no host reference, so a tap that outlives the clock reads a settled fact rather than a dangling timer.
- Receipt: the beat IS the receipt of one tick; the clock keeps no history beyond its failures.
- Growth: a new measured coordinate is one column the mint derives.
- Boundary: a host presentation clock's target timestamp is NOT a monotonic counter and never enters this value — a display link's predicted present time stays at the boundary that reads it.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Interaction;

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
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

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
