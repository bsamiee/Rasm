# [RASM_CLOCK]

`Rasm.Interaction` owns the repeating UI beat: one host-driven clock, one beat value carrying its monotonic evidence beside the drift and the misses the interval actually accumulated, and one posture deciding whether a failing tick halts the clock or lets it run on. Both host boundaries drove their own timer, minted their own cadence value object, and derived their own drift; all three were the same body and all three now read this owner while each boundary keeps only the timer attachment its platform demands.

The beat composes the kernel timeline rather than re-minting temporal identity: `MonotonicBeat` is the evidence and `MonotonicTimeline.Beat` its one mint, so a host beat and a kernel span order against one clock and a flat host beat cannot fork the causal order. The chain's own tail is a cursor this owner holds, because `BeatSeed` names an origin or a predecessor and nothing else remembers which of the two the next tick owes.

## [01]-[INDEX]

- [02]-[CLOCK]: `UiClock`, `FaultPosture` — the repeating clock, the posture carrying its own failure settle, and the beat-chain tail it advances.
- [03]-[BEAT]: `PulseBeat` — the per-tick value carrying monotonic evidence, drift, and miss count.

## [02]-[CLOCK]

- Owner: `UiClock` — the repeating clock. It mints on a cadence, a tick body, a posture, a `FaultCell`, and a timeline. `BeatSeed` is the internal chain tail: the origin the first beat advances from or the beat the next tick advances from.
- Cases: `FaultPosture` closes the failure axis at two rows, each CARRYING its settle — `Halt` fails the tick and `Continue` lets the beat advance. A mirror bool buys a name and leaves every consumer to re-derive the consequence; a caller-supplied boolean cannot state which of the two a consumer chose, and both boundaries chose differently.
- Entry: `Of(cadence, beat, posture, faults, timeline)` mints; `Tick` is the host callback and `Tap` attaches a beat observer.
- Auto: the cursor transition takes the kernel default `Cell.SwapBudget` (branch RULINGS `[02]`) — a guarded step spends no budget of its own, and a one-member shell beside the kernel row is two authorities over one ceiling.
- Auto: the cadence admits as `PositiveMagnitude` (`Numerics/atoms`) — the kernel's one positive-scalar owner — so no second cadence value object exists. The census proposed one; it never mints, because a cadence is a positive magnitude in seconds and nothing about a UI timer makes it a distinct invariant.
- Auto: the cursor advances through `Cell.Step` GUARDED on the seed the mint measured, so a tick whose seed moved reads its transition case rather than assuming a swap, and the seed it hands `MonotonicTimeline.Beat` is `Origin` exactly once and `Previous` on every tick after. The compare-and-swap arm is the rejected placement: its body re-runs on every contended retry and re-installs a candidate derived from a predecessor the winner already replaced, so the seed-moved corner has to DECLINE rather than recompute. The timeline's own tail gate refuses a replayed predecessor, so a doubled host callback lands a typed refusal instead of a duplicate ordinal.
- Law: the clock owns identity and the host owns the timer. `UiClock` advances the beat, derives the drift, counts the misses, and applies the posture; the boundary supplies the platform lease — a display link, a `UITimer`, an idle callback — and does nothing else. A boundary that computes a drift or a miss count is re-deriving what the beat already carries.
- Law: the clock takes a `MonotonicTimeline`, never a bare `TimeProvider`. A provider is the timeline's own admission argument, and a clock holding one would mint a second timeline whose stamps no kernel span can order against the first. NAMED LOSS: a caller supplying a test provider now supplies `MonotonicTimeline.Of(provider)` instead. Witness: a fake-clock test seats `MonotonicTimeline.Of(new FakeTimeProvider())` and passes the timeline.
- Law: a failing tick NEVER silently stops, and the posture ROW decides which way. Tick recovery parks the cause once before `Settle` answers the result — `Continue` succeeds and the beat advances, `Halt` fails and the error is the terminal reading — so the tick fold names no posture and a third posture is one row. A clock that dies quietly is indistinguishable from a host that stopped scheduling it.
- Law: the fault sink is the branch's `FaultCell` and never a raw `Action<Error>`. A `void` delegate licenses a silent discard and grows nothing a consumer can bound; the cell is a bounded ring whose parks, sheds, and declined parks all read as numbers, and `Publish` captures each observer directly into it.
- Law: an observer raise never fails the tick that fed it — publication runs through `Try.lift(...).Run().Match(...)` on this clock's own cell under `UiClock.Point`, exactly as the dispatch observers do, because a beat observer that can fail the clock turns instrumentation into a liveness dependency.
- Law: the missed count is MEASURED from the beat ordinal, never assumed. `MonotonicBeat.Ordinal` counts CADENCE PERIODS (`Parametric/projections` `[05]`), so a host that coalesces ticks under load leaves a gap in the ordinal and the gap IS the miss count — a wall-clock interval divided at the consumer is the deleted form.
- Output: `PulseBeat` per tick; fault history remains on the supplied `FaultCell`, so a caller that only wants the beat pays nothing for a forwarded reading.
- Packages: LanguageExt.Core for the result, presence, and collection carriers; `Parametric/projections` for `MonotonicTimeline`, `BeatSeed`, and `MonotonicBeat`; `Domain/hooks` for `FaultCell` and `HookId`.
- Growth: a new posture is one row carrying its own settle arm, and no consumer edits.
- Boundary: the platform timer's construction, lifecycle, and run-loop mode are the boundary's — `CADisplayLink`, `UITimer`, and idle-callback attachment never enter this owner.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class FaultPosture {
    public static readonly FaultPosture Halt = new(settle: Fin.Fail<Unit>);
    public static readonly FaultPosture Continue = new(settle: static _ => Fin.Succ(unit));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Settle(Error cause);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class UiClock : IDisposable {
    private static readonly HookId Point = HookId.Create(value: "rasm.kernel.interaction.clock");

    private readonly Atom<BeatSeed> cursor;
    private readonly Atom<Seq<Action<PulseBeat>>> observers = Atom(Seq<Action<PulseBeat>>());
    private readonly PositiveMagnitude cadence;
    private readonly MonotonicTimeline timeline;
    private readonly FaultPosture posture;
    private readonly Func<PulseBeat, Fin<Unit>> body;
    private readonly FaultCell faults;

    public static Fin<UiClock> Of(
        PositiveMagnitude cadence,
        Func<PulseBeat, Fin<Unit>> beat,
        FaultPosture posture,
        FaultCell faults,
        MonotonicTimeline timeline);

    public Fin<IDisposable> Tap(Action<PulseBeat> observer) =>
        Admit.Need(observer).Map(admitted => {
            Action<PulseBeat> registered = pulse => admitted(pulse);
            ignore(observers.Swap(held => held.Add(registered)));
            return (IDisposable)new HookDetacher(Detach: () =>
                ignore(observers.Swap(held => held.Filter(
                    row => !ReferenceEquals(row, registered)).Strict())));
        });

    public Fin<Unit> Tick() =>
        Advance()
            .Bind(pulse => (Publish(pulse), body(pulse)).Item2)
            .BindFail(cause =>
                (ignore(faults.Park(Point, cause)), posture.Settle(cause)).Item2);

    private Unit Publish(PulseBeat pulse) => observers.Value.Iter(
        observer => Try.lift(() => {
            observer(pulse);
            return unit;
        }).Run().Match(
            Succ: static _ => unit,
            Fail: cause => ignore(faults.Park(point: Point, cause: cause))));

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

    public void Dispose();
}
```

## [03]-[BEAT]

- Owner: `PulseBeat` — the per-tick value: the kernel `MonotonicBeat` as its temporal evidence, the drift against the declared cadence, and the count of beats the host coalesced away.
- Auto: interval, drift, and misses all DERIVE at mint from the beat and its predecessor — the interval is the beat's own delta, the drift is that delta against the declared period, and the miss count is `beat.Ordinal − prior.Ordinal − 1`. A stored drift a caller sets is a fabricated measurement.
- Law: the first beat of a sequence has no predecessor and therefore no misses — absence reads as zero here because the ordinal gap is undefined against an origin, not because zero was substituted for a measurement.
- Law: temporal identity is the kernel `MonotonicBeat`'s and this value extends it with cadence-only columns — a flat host beat carrying its own timestamp re-mints the identity the branch already federates (branch RULINGS `[02]`).
- Law: drift is SIGNED. A tick that fired early reads negative and a tick that fired late reads positive, so a host running fast is distinguishable from a host running slow; an absolute drift collapses the two directions a pace correction has to tell apart.
- Law: the beat is an ordinary value with no host reference, so a tap that outlives the clock reads a settled fact rather than a dangling timer.
- Law: the beat IS the evidence of one tick; the clock keeps no history beyond its failures.
- Growth: a new measured coordinate is one column the mint derives.
- Boundary: a host presentation clock's target timestamp is NOT a monotonic counter and never enters this value — a display link's predicted present time stays at the boundary that reads it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Interaction;

// --- [MODELS] --------------------------------------------------------------------------
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

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
