# [RASM_GRASSHOPPER_ETO_RUNTIME]

Eto runtime floor of the Grasshopper boundary is now the KERNEL `Rasm/Interaction` estate — marshal (`UiThread` over the `UiDispatch` crossing family), repeating-beat identity (`UiClock`/`PulseBeat`/`FaultPosture`), data transfer (`Transfer`), display and input facts (`Displays`/`InputState`), and OS presence (`Presence`) — composed directly by every consumer. This page keeps ONLY what the kernel's own boundary laws assign to the platform: the leased `UITimer` that DRIVES a kernel clock (kernel clock law — "the clock owns identity and the host owns the timer"), and the measured frame-interval producer that FEEDS `UiThread.Tune` so the stall watchdog budgets against the display this process runs on (E-G41).

`EtoTimer` mints the platform lease: one `UITimer` whose `Elapsed` calls the kernel clock's tick, minted inside a kernel marshal and released once by the `TimerHold` capsule, which parks a release refusal on the composition's `FaultCell` — the cell the telemetry root already reads. `FrameTune` is the pace producer: it admits the measured minimum refresh interval (from `Platform/native.md`'s display-link measurement on macOS, or the display metadata fallback), scales the kernel `PaceBand` to it, and seats the result through `UiThread.Tune` — a fabricated 60 Hz period reads every frame of a 120 Hz display as on-time, which is the exact stall class the watchdog exists for.

## [01]-[INDEX]

- [02]-[TIMER]: `EtoTimer`/`TimerHold` — the leased `UITimer` platform half of the kernel clock.
- [03]-[PACE]: `FrameTune` — the measured frame-interval producer seating `UiThread.Tune`.

## [02]-[TIMER]

- Owner: `EtoTimer` — the one `UITimer` supplier: `Drive(PositiveMagnitude cadence, Action tick, FaultCell faults, Op? key = null)` → `Fin<Lease<TimerHold>>` mints the timer at the admitted cadence, wires `Elapsed` to the supplied tick, starts it, and returns `Owned` over the `TimerHold` capsule; `TimerHold.Dispose` stops the timer, detaches the handler, and disposes — construction and release each marshal through the kernel `UiThread.Run` blocking arity because `UITimer` is UI-affine.
- Law: the lease's `Owned` case carries a VALUE alone, so the release lives on the leased type — `TimerHold` is the UI-affine capsule holding the timer, the handler it must detach, and the mint key its release marshal reuses; a lease taken over the bare timer cannot detach the handler, because the handler is not recoverable from the `UITimer` it was attached to.
- Law: a release refusal PARKS on the composition's `FaultCell` under the capsule's own point id — the cell's `Parked`/`Lost` gauges are the telemetry root's reads, so a still-attached handler is counted evidence, never an invisible discard; a capsule-local `Atom<Seq<Error>>` ledger nothing read was the deleted FaultCell twin.
- Law: identity lives in the KERNEL clock — the tick this timer drives is `UiClock`'s own; drift, misses, ordinals, postures, observers, and fault custody are the kernel's, and a body here that computes any of them re-derives what the beat already carries. This page supplies the platform lease and NOTHING else, per the kernel clock boundary law.
- Law: this is the folder's ONE repeating-tick platform surface — a second `System.Threading.Timer`, `Task.Delay` loop, or per-consumer `UITimer` beside it is the deleted form; high-cadence display-link pacing is `Platform/native.md`'s macOS replacement seam, selected by the consumer, never a fork inside this owner.
- Packages: Eto (`UITimer.Interval`/`Elapsed`/`Start`/`Stop`), `Rasm.Interaction` (`UiThread`, `UiDispatch`, `DispatchLane`), `Rasm.Numerics` (`PositiveMagnitude`), `Rasm.Domain` (`Op`, `Lease<T>`, `FaultCell`, `HookId`, `Custody`), LanguageExt.Core (`Fin`, `Seq`).
- Growth: none expected — a new platform tick source is a sibling supplier at ITS platform page, never a widening here.

## [03]-[PACE]

- Owner: `FrameTune` — the pace producer: `Feed(PositiveMagnitude interval, Option<MonotonicTimeline> clock = default, Op? key = null)` → `Fin<Unit>` scales the kernel `PaceBand` to the measured minimum refresh interval and seats `UiThread.Tune(new StallPolicy(Pace: scaled, Stretch: ...), clock, key)`; the per-lane stretch map stays kernel-default empty unless a measured host pathology earns a named override row.
- Law: the interval is MEASURED, never declared — `Platform/native.md`'s display-link measurement produces it on macOS and the display metadata read is the fallback; the kernel seeds `StallPolicy.Portable` so an untuned floor over-reports a stall and never hides one, and this producer only ever tightens toward the real display.
- Law: the seat transition is the kernel's — `Tune` answers the kernel `Transition<StallPolicy>` semantics, so a tune that lost a race under contention is a read case, never an assumed swap; this producer retries nothing and reports the refusal.
- Boundary: WHO calls `Feed` is `Platform/composition.md`'s load roster (the pacer row) and `Platform/native.md`'s re-measure on display change; this page owns the producer spelling alone.
- Packages: `Rasm.Interaction` (`UiThread.Tune`, `StallPolicy`, `DispatchLane`), `Rasm.Parametric` (`PaceBand`, `MonotonicTimeline`), `Rasm.Numerics` (`PositiveMagnitude`), `Rasm.Domain` (`Op`).
- Growth: a measured per-lane pathology is one stretch row in the policy this producer seats; the entry never widens.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Grasshopper.Eto;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class TimerHold : IDisposable {
    private readonly Lazy<Unit> release;

    internal TimerHold(UITimer timer, EventHandler<EventArgs> handler, FaultCell faults, HookId point, Op key) =>
        release = new Lazy<Unit>(() => UiThread.Run(
                new UiDispatch<Unit>.Blocking(() => Release(timer, handler, key)), DispatchLane.Interactive, key)
                .Match(Succ: static _ => unit, Fail: fault => ignore(faults.Park(point: point, cause: fault))),
            LazyThreadSafetyMode.ExecutionAndPublication);

    public void Dispose() => ignore(release.Value);

    internal static Fin<Unit> Release(UITimer timer, EventHandler<EventArgs> handler, Op key) =>
        Custody.Release(Seq<Func<Fin<Unit>>>(
            () => { timer.Stop(); return Fin.Succ(unit); },
            () => { timer.Elapsed -= handler; return Fin.Succ(unit); },
            () => { timer.Dispose(); return Fin.Succ(unit); }), key);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static class EtoTimer {
    private static readonly HookId Rail = HookId.Create(value: "rasm.grasshopper.eto.timer");

    public static Fin<Lease<TimerHold>> Drive(PositiveMagnitude cadence, Action tick, FaultCell faults, Op? key = null) {
        Op op = key.OrDefault();
        return from body in op.Need(tick)
               from minted in UiThread.Run(new UiDispatch<(UITimer Timer, EventHandler<EventArgs> Handler)>.Blocking(() => {
                       UITimer? native = null;
                       EventHandler<EventArgs>? handler = null;
                       Fin<(UITimer Timer, EventHandler<EventArgs> Handler)> opened = op.Catch(body: () => {
                           native = new UITimer { Interval = (double)cadence };
                           handler = (_, _) => op.Catch(body)
                               .IfFail(fault => ignore(faults.Park(point: Rail, cause: fault)));
                           native.Elapsed += handler;
                           native.Start();
                           return Fin.Succ((Timer: native, Handler: handler));
                       });
                       return opened.Rollback(
                           release: () => native is not null && handler is not null
                               ? TimerHold.Release(native, handler, op)
                               : Fin.Succ(unit),
                           key: op);
                   }), DispatchLane.Interactive, op)
               select (Lease<TimerHold>)new Lease<TimerHold>.Owned(
                   Value: new TimerHold(
                       timer: minted.Timer, handler: minted.Handler, faults: faults, point: Rail, key: op));
    }
}

[BoundaryAdapter]
public static class FrameTune {
    public static Fin<Unit> Feed(PositiveMagnitude interval, Option<MonotonicTimeline> clock = default, Op? key = null) {
        Op op = key.OrDefault();
        return from scaled in PaceBand.Portable.ScaleTo(reference: interval, key: op)
               from seated in UiThread.Tune(
                   policy: new StallPolicy(Pace: scaled, Stretch: HashMap<DispatchLane, double>()),
                   clock: clock,
                   key: op)
               select seated;
    }
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]            | [OWNER]     | [RAIL]                                 |
| :-----: | :------------------- | :---------- | :------------------------------------- |
|  [01]   | platform timer lease | `EtoTimer`  | `Drive → Fin<Lease<TimerHold>>`        |
|  [02]   | release custody      | `TimerHold` | `Dispose → FaultCell`                  |
|  [03]   | pace production      | `FrameTune` | `Feed → Fin<Unit>` via `UiThread.Tune` |

Everything else this page once owned is the kernel's, composed directly: `EtoDispatch`/`DispatchLane`/`PulseLane`/`DispatchPulse`/`DispatchEcho`/the watchdog → kernel `UiThread`/`UiDispatch`/`DispatchLane`/`StallPolicy`/`Watch`/`Tap`; `UiCadence`/`ClockBeat`/`UiClock`/`FaultPosture` → kernel `UiClock`/`PulseBeat`/`FaultPosture` (cadence = `PositiveMagnitude`); `TransferSurface`/`TransferPayload`/`PayloadShape`/`Transfer` → kernel `Transfer` estate; `DisplayMetrics`/`Display`/`PointerSnapshot`/`InputState`/`ModifierWatch` → kernel `Displays`/`InputState`; `Notice`/`NoticeMount`/`TrayMount`/`NoticeSurface` → kernel `Presence`. `RuntimeLog` partial and the `DispatchEcho` echo stream retire with their owners — queued-crossing settlement evidence is the kernel's `Tap`, and session queue faults park on the caller's `FaultCell`.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
