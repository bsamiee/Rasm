# [RASM_DISPATCH]

`Rasm.Interaction` owns the branch's one crossing into the Eto control tree. A crossing is a VALUE — the modality is the case, never a member name — so a synchronous assert, an in-frame invoke, an awaited marshal, a queued post, and a pump iteration are five cases of one union under one entry rather than five sibling members at each host boundary. Every crossing is gauged against its lane's frame budget, every refusal is a case of one fault family banded at the kernel registry, and a headless process refuses at the entry instead of dead-locking on an application that was never started.

Both host boundaries reach this owner directly and neither adapts it: the Rhino command-thread affinity axis and the Grasshopper canvas session are genuinely host-specific and stay there, while the marshal itself, its budget, its stall evidence, and its fault vocabulary are one kernel body both compose. The gauge is `Parametric/projections`' — `DispatchLane` realizes `IGaugeLane<DispatchLane>` so the lane's bound has one owner, and the pace it derives from is `PaceBand`, the same band the motion drive paces against.

## [01]-[INDEX]

- [02]-[MODALITY]: `UiDispatch<T>`, `ISyncCrossing<T>`, `IAsyncCrossing<T>`, `DispatchLane` — the five crossing cases, the two return-shape markers, and the gauge lane roster that budgets them.
- [03]-[THREAD]: `UiThread` — the two marshal arities, the affinity probe, the gauge tune, and the observer taps.
- [04]-[PULSE]: `StallPolicy`, `DispatchPulse`, `DispatchEcho` — the seated pace, the gauged crossing evidence, and the queued-crossing echo.
- [05]-[FAULT]: `RejectReason`, `UiFault`, `FaultRail` — the refusal-clause vocabulary, the closed refusal family on the kernel `FaultBand.Interaction` row, and the two host-raise projections every surface on this sub-domain routes through.

## [02]-[MODALITY]

- Owner: `UiDispatch<T>` the closed crossing family, every case carrying the `Func<Fin<T>>` body it marshals; `ISyncCrossing<T>` and `IAsyncCrossing<T>` the two return-shape markers a case declares; `DispatchLane` the `[SmartEnum<int>]` gauge roster realizing `IGaugeLane<DispatchLane>`, whose column is a FRAME MULTIPLE rather than a wall-clock span, so one roster serves a sixty-hertz display and a portable panel without a second table.
- Cases: `Current` asserts affinity and refuses off-thread; `Blocking` runs in-frame when already on the marshal and invokes otherwise; `Awaited` marshals and awaits the result; `Queued` posts and publishes a `DispatchEcho` when the body settles; `Pumped` runs one application iteration so a modal wait can drain without re-entering the marshal.
- Cases: `DispatchLane` carries six rows — `Immediate`, `Interactive`, `Deferred`, `Modal`, `Background`, and `Paced`, whose one-frame multiple against the pace band's `Period` makes it the display-link tick's own budget without a seventh number anywhere.
- Entry: `UiThread.Run(crossing, lane, key)` is the ONE entry name; the CASE's static type selects the arity, so a `Current`, `Blocking`, or `Pumped` crossing lands `Fin<T>` inside the caller's own LINQ query and an `Awaited` or `Queued` crossing lands `ValueTask<Fin<T>>`. A caller holding an erased `UiDispatch<T>` switches, because the return shape genuinely differs and a uniform `ValueTask` over a synchronous crossing forces every host gate to block on a completed task.
- Auto: the lane's `Bound` derives as `Pace.Period × Frames × Stretch`, so the frame period has ONE owner — the seated `PaceBand` — and a host widening one lane states a dimensionless multiple rather than a second millisecond table. The two hand-written millisecond tables both boundaries carry today have no place to reappear.
- Law: a marker interface is implemented by union cases alone and its `Crossing` member is answered ONCE on the root — the projection is the same identity at every case, so a case declares its arity and no body. The root constructor is private, so no foreign implementor can produce a value the total `Switch` does not already own, and the arity split costs no openness.
- Law: `Queued` returns the same shape every other asynchronous case returns. NAMED LOSS: the Grasshopper form returned queue-ADMISSION alone, so a caller could not distinguish "accepted for later" from "ran and succeeded". That distinction survives as the `DispatchEcho` tap rather than as a different return type, because a return shape that differs per case is exactly what makes the modality unrecoverable from the value.
- Law: a `ref struct` never crosses a case body — a span-shaped result copies into owned storage before the crossing, because a case body is a delegate seam and closure capture is the same escape as a box.
- Law: NAMED LOSS — the census `DispatchLane.Marshal` column does not land. The crossing CASE is the marshal choice; a lane naming a marshal shape beside a case naming another is two authorities over one crossing, and the disagreement is unresolvable at the call site. Witness: a lane-selected marshal reads as `UiThread.Run(new UiDispatch<T>.Blocking(body), DispatchLane.Immediate, key)`, where the lane budgets and the case marshals.
- Growth: a new crossing modality is one case, one marker, and one arm at every dispatch site, breaking loudly; a new budget lane is one row the seated pace already budgets.
- Boundary: Rhino's command-thread affinity (`HostThread`/`HostWork<T>`) is a DIFFERENT axis over the Rhino command queue and stays plural at that boundary; what re-points here are its marshal lane and its latency gauge, which were this owner's shape all along.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ISyncCrossing<TResult> { UiDispatch<TResult> Crossing { get; } }

public interface IAsyncCrossing<TResult> { UiDispatch<TResult> Crossing { get; } }

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UiDispatch<TResult> {
    private UiDispatch() { }

    public UiDispatch<TResult> Crossing => this;

    public sealed record Current(Func<Fin<TResult>> Body) : UiDispatch<TResult>, ISyncCrossing<TResult>;
    public sealed record Blocking(Func<Fin<TResult>> Body) : UiDispatch<TResult>, ISyncCrossing<TResult>;
    public sealed record Pumped(Func<Fin<TResult>> Body) : UiDispatch<TResult>, ISyncCrossing<TResult>;
    public sealed record Awaited(Func<Fin<TResult>> Body) : UiDispatch<TResult>, IAsyncCrossing<TResult>;
    public sealed record Queued(Func<Fin<TResult>> Body) : UiDispatch<TResult>, IAsyncCrossing<TResult>;

    public Func<Fin<TResult>> Body => Switch(
        current: static crossing => crossing.Body,
        blocking: static crossing => crossing.Body,
        pumped: static crossing => crossing.Body,
        awaited: static crossing => crossing.Body,
        queued: static crossing => crossing.Body);
}

[SmartEnum<int>]
public sealed partial class DispatchLane : IGaugeLane<DispatchLane> {
    public static readonly DispatchLane Immediate = new(key: 0, frames: 1d);
    public static readonly DispatchLane Interactive = new(key: 1, frames: 4d);
    public static readonly DispatchLane Deferred = new(key: 2, frames: 6d);
    public static readonly DispatchLane Modal = new(key: 3, frames: 1d);
    public static readonly DispatchLane Background = new(key: 4, frames: 1d);
    public static readonly DispatchLane Paced = new(key: 5, frames: 1d);

    public double Frames { get; }

    public TimeSpan Bound => StallPolicy.Seated.Bound(lane: this);
}
```

## [03]-[THREAD]

- Owner: `UiThread` — the one marshal surface: the two crossing arities, the affinity probe, the gauge tune, and the two observer taps.
- Entry: `Run` dispatches the crossing case, brackets the body in the lane's gauged span, and lands the arity the case's marker declares; `OnMarshal` is the PUBLISHED affinity probe so no consumer inlines `Application.Instance.IsUIThread` and no consumer reads an absent instance as "not on the thread"; `Tune` seats the pace and the gauge clock; `Watch` and `Tap` lease the pulse and echo observers.
- Auto: every entry admits the application through `Optional(Application.Instance).ToFin(key.MissingContext())`, so a headless process — a test host, a compute node, a CLI — refuses TYPED at the entry rather than blocking on a marshal that will never run. That refusal is the reason the probe is published: an affinity test that answers `false` in a headless process is indistinguishable from one that answers `false` on a worker thread, and only one of them is recoverable.
- Auto: the gauged bracket is `MonotonicTimeline.Gauged<T, DispatchLane>`, which takes NO bound — the lane row owns it — and answers `(Fin<T> Value, GaugedSpan<DispatchLane> Span)`, so the span lands on a refused body too and the pulse publishes from every crossing rather than from the settled ones alone.
- Law: the marshal is the only site that names an Eto application, and it carries `[BoundaryAdapter]` — every interior owner on this sub-domain takes admitted values and returns rails, so the host surface is one page wide.
- Law: a crossing never swallows: a body's `Fin` failure rides out unchanged, and only the CROSSING itself — a dead context, a refused post, a headless entry — becomes a `UiFault`.
- Law: an observer raise never fails the crossing it observed. Publication runs through `FaultRail.Isolate`, which parks the subscriber's refusal on the sub-domain fault cell and settles the crossing, because a tap that can fail a marshal turns instrumentation into a liveness dependency.
- Receipt: `DispatchPulse` per crossing and `DispatchEcho` per queued settle, both published to leased observers rather than returned, so a caller that wants only the value pays nothing.
- Packages: Eto.Forms for `Application` (registered at `libs/dotnet/.api/api-eto-runtime.md`); LanguageExt.Core for the rails and the `Lease`; `Parametric/projections` for the timeline and the gauge lane floor.
- Growth: a new observer is one lease; a new lane is one row the seated pace already budgets.
- Boundary: `Application.Instance` is read at the entry and never stored — a captured instance outlives a host restart and marshals onto a dead context that never re-posts.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Eto.Forms;
using Rasm.Domain;
using Rasm.Parametric;

namespace Rasm.Interaction;

// --- [SERVICES] ------------------------------------------------------------------------
public static class UiThread {
    [BoundaryAdapter] public static Fin<T> Run<T>(ISyncCrossing<T> crossing, DispatchLane lane, Op? key = null);
    [BoundaryAdapter] public static ValueTask<Fin<T>> Run<T>(IAsyncCrossing<T> crossing, DispatchLane lane, Op? key = null);

    [BoundaryAdapter] public static Fin<bool> OnMarshal(Op? key = null);

    [BoundaryAdapter] public static Fin<Unit> Tune(StallPolicy policy, Option<MonotonicTimeline> clock = default, Op? key = null);

    [BoundaryAdapter] public static Fin<Lease<IDisposable>> Watch(Action<DispatchPulse> observer, Op? key = null);
    [BoundaryAdapter] public static Fin<Lease<IDisposable>> Tap(Action<DispatchEcho> observer, Op? key = null);

    public static Option<DispatchPulse> LastPulse { get; }
    public static Option<DispatchPulse> LastStall { get; }
}
```

## [04]-[PULSE]

- Owner: `StallPolicy` seats the pace band, the per-lane stretch, and the process gauge; `DispatchPulse` is the gauged crossing evidence; `DispatchEcho` is the queued-crossing settle fact.
- Auto: the seat commits on the kernel default `Cell.SwapBudget` (branch RULINGS `[02]`) — this page measures no divergent contention and a one-member budget shell beside the kernel row is two authorities over one ceiling.
- Auto: `Breached` is DERIVED from the span and its bound, never a stored flag — `GaugedSpan` already answers it and a second stored verdict is a fabricated measurement the moment a bound moves.
- Auto: the seat is an atom holding the PORTABLE band until a host tunes, so a lane's `Bound` is answerable before any tune and a display migration re-seats one value rather than every lane row. `PaceBand.Portable` is the DECLARED row both boundaries hardcoded as the same sixteen-millisecond frame before this owner existed.
- Law: a per-lane widening is a dimensionless STRETCH on the seated policy, never a millisecond override. A stored millisecond bound beside a frame multiple is two authorities over one budget, and the pair disagrees the first time a host publishes a real refresh interval. NAMED LOSS: a host can no longer state one lane's budget in absolute time; it states the multiple and the pace, and the product is the budget.
- Law: every seat transition answers a `Transition<StallPolicy>` verdict, so a tune that lost a race under contention reads its case rather than assuming a swap.
- Receipt: `DispatchPulse` carries the operation, the lane, the elapsed span, and the derived breach; `DispatchEcho` carries the operation and the settled outcome, so a queued crossing's failure is observable without the caller awaiting it.
- Growth: a new measured coordinate is one column on the pulse, answered by the one gauge that produces it.
- Boundary: the pulse is EVIDENCE and never a gate — no crossing branches on a prior pulse, because a budget that steers the next crossing turns a measurement into a feedback loop nothing declared.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Parametric;

namespace Rasm.Interaction;

// --- [POLICIES] ------------------------------------------------------------------------
public sealed record StallPolicy(PaceBand Pace, HashMap<DispatchLane, double> Stretch) {
    public static readonly StallPolicy Portable = new(Pace: PaceBand.Portable, Stretch: HashMap<DispatchLane, double>());

    private static readonly Atom<StallPolicy> seat = Atom(Portable);

    internal static StallPolicy Seated => seat.Value;

    internal static Transition<StallPolicy> Seat(StallPolicy policy) =>
        Cell.Commit(seat, _ => policy);

    internal TimeSpan Bound(DispatchLane lane) =>
        Pace.Period * lane.Frames * Stretch.Find(lane).IfNone(1d);
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DispatchPulse(GaugedSpan<DispatchLane> Span) : IValidityEvidence {
    public Op Operation => Span.Work;
    public DispatchLane Lane => Span.Lane;
    public TimeSpan Elapsed => Span.Elapsed;
    public bool Breached => Span.Breached;
    public bool IsValid => Span.IsValid;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DispatchEcho(Op Operation, Fin<Unit> Outcome);
```

## [05]-[FAULT]

- Owner: `UiFault` — the closed refusal family for every interaction crossing, deriving `Fault` and coded off the kernel `FaultBand.Interaction` row; `RejectReason` — the sub-domain's one refusal-clause vocabulary; `FaultRail` — the two projections a host raise crosses, one failing the operation and one isolating an observer.
- Cases: `Dismissed` (the user closed the surface), `Cancelled` (the caller withdrew), `Unavailable` (the host lacks a rostered `PlatformCapability`), `OffThread` (a `Current` crossing ran off the marshal), `Rejected` (a `FieldTag` refused under a `RejectReason` row), `AbsentPayload` (a transfer read found nothing of the wanted format), `HostRejected` (the platform refused the operation), `Released` (the surface was torn down mid-crossing), `Headless` (no application instance exists).
- Auto: each `[FaultCase]` ordinal generates the cached numeric identity against the root's one `FamilyBand`; `Message` is the family-owned total presentation fold.
- Law: the root derives `Fault` directly. Numeric identity is generated, while the case payload and total message projection remain authored here.
- Law: the band is the kernel registry's `FaultBand.Interaction` row and every code is `Code(offset)` off it; the registry's disjointness proof keeps this family clear of the app-platform bands, and every host composes this one UI-refusal family.
- Law: `Cancelled` is a case of THIS family and never a transient — it is the first arm every retry predicate refuses, and it never collapses into a host-rejection case.
- Law: recovery reads the case and immutable posture, never rendered text. A generic wire may project numeric code and recovery only; no category or case-name mirror crosses with it. Every payload identity column is admitted — `Unavailable` names a `PlatformCapability` row and `Rejected` a `FieldTag` beside a `RejectReason` row.
- Law: a refusal clause is a `RejectReason` ROW and its sentence rides that row's `Requirement`. A joined-sentence reason column is the deleted form: a caller filtering on which clause fired had to parse prose, and a surface rendering it had no clause to name. A refusal violating several clauses lowers one fault per row through `Error.Many`, so the whole violated set survives.
- Boundary: `FieldTag` (`Interaction/control`) and `PlatformCapability` (`Interaction/platform`) are IDENTITY value objects on this sub-domain's own assembly, carrying no behaviour this family could invert onto. The fault family reading them is one namespace resolving one vocabulary, never a stratum reaching upward.
- Law: an observer fault and an operation fault are different postures and `FaultRail` states both. `Host` lifts a platform raise onto the rail as `HostRejected`; `Isolate` parks a subscriber's raise on the `FaultCell` and settles, so a tap storm sheds into a bounded ring rather than failing the surface that published.
- Growth: a new refusal is one case, one message arm, declaration-order ordinal compaction, and the matching `FaultBand.Interaction.Span` edit; a new refusal CLAUSE is one `RejectReason` row and no case edit.
- Boundary: a documented platform raise funnels through `FaultRail.Host` and lands as `HostRejected` carrying the exact captured `Error` cause; a returned failed rail passes through unchanged.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RejectReason {
    // --- [FUSION]
    public static readonly RejectReason SeedFlow = new(key: "seed-flow", requirement: "one-time flow and seed input imply each other");
    public static readonly RejectReason SeedTiming = new(key: "seed-timing", requirement: "seed input admits edit timing alone");
    public static readonly RejectReason DebouncedPath = new(key: "debounced-path", requirement: "debounced timing requires a context path relaying into the control");
    public static readonly RejectReason CommitFlow = new(key: "commit-flow", requirement: "commit timing requires a flow relaying into the source");
    public static readonly RejectReason ManualTiming = new(key: "manual-timing", requirement: "manual flow admits edit timing alone");

    // --- [SEAM]
    public static readonly RejectReason NoChildPath = new(key: "no-child-path", requirement: "a source shape carrying a live child path");
    public static readonly RejectReason ControlType = new(key: "control-type", requirement: "a control of the type the plan selects its binding on");
    public static readonly RejectReason EmptyLatch = new(key: "empty-latch", requirement: "a latch holding a pending write");
    public static readonly RejectReason Capacity = new(key: "capacity", requirement: "an admitted positive bound");

    // --- [CHROME]
    public static readonly RejectReason SheetInset = new(key: "sheet-inset", requirement: "margins leaving a drawable extent inside the laid sheet");
    public static readonly RejectReason PageSpan = new(key: "page-span", requirement: "selected pages forming an ordered subset of the job");
    public static readonly RejectReason HostSelection = new(key: "host-selection", requirement: "a host publishing a current selection to print");
    public static readonly RejectReason UnmatchedLeave = new(key: "unmatched-leave", requirement: "a mount visit entered before it is left");
    public static readonly RejectReason TrayAnchor = new(key: "tray-anchor", requirement: "a tray anchor where the platform's own notification demands one");
    public static readonly RejectReason PackedRows = new(key: "packed-rows", requirement: "a row buffer packed to the extent and carriage it declares");
    public static readonly RejectReason RootFaceSize = new(key: "root-face-size", requirement: "a root face naming the size a type scale multiplies");

    public string Requirement { get; }
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UiFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Interaction;
    private UiFault() { }

    [FaultCase(0)] public sealed partial record Dismissed(Op Key) : UiFault;
    [FaultCase(1)] public sealed partial record Cancelled(Op Key) : UiFault;
    [FaultCase(2)] public sealed partial record Unavailable(Op Key, PlatformCapability Capability) : UiFault;
    [FaultCase(3)] public sealed partial record OffThread(Op Key) : UiFault;
    [FaultCase(4)] public sealed partial record Rejected(Op Key, FieldTag Field, RejectReason Reason) : UiFault;
    [FaultCase(5)] public sealed partial record AbsentPayload(Op Key, Mime Wanted) : UiFault;
    [FaultCase(6)] public sealed partial record HostRejected(Op Key, Error Cause) : UiFault, ICausedFault;
    [FaultCase(7)] public sealed partial record Released(Op Key) : UiFault;
    [FaultCase(8)] public sealed partial record Headless(Op Key) : UiFault;
    [FaultCase(9)] public sealed partial record Absent(Op Key, string Member) : UiFault;

    public sealed override string Message => Switch(
        dismissed:     static fault => $"Interaction operation '{fault.Key}' was dismissed.",
        cancelled:     static fault => $"Interaction operation '{fault.Key}' was cancelled.",
        unavailable:   static fault => $"Interaction operation '{fault.Key}' requires unavailable capability '{fault.Capability.Key}'.",
        offThread:     static fault => $"Interaction operation '{fault.Key}' requires the UI thread.",
        rejected:      static fault => $"Interaction operation '{fault.Key}' rejected field '{fault.Field.Value}': {fault.Reason.Requirement}.",
        absentPayload: static fault => $"Interaction operation '{fault.Key}' found no payload matching '{fault.Wanted.Value}'.",
        hostRejected:  static fault => $"Host rejected interaction operation '{fault.Key}': {fault.Cause.Message}",
        released:      static fault => $"Interaction operation '{fault.Key}' reached a released surface.",
        headless:      static fault => $"Interaction operation '{fault.Key}' requires a running application.",
        absent:        static fault => $"Interaction operation '{fault.Key}' requires host member '{fault.Member}'.");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FaultRail {
    private static readonly HookId Rail = HookId.Create(value: "rasm.kernel.interaction.dispatch");

    public static Fin<T> Host<T>(Func<Fin<T>> body, Op key) =>
        key.Catch<T, UiFault.HostRejected>(
            body: body,
            provider: cause => Some(new UiFault.HostRejected(Key: key, Cause: cause)));

    public static Unit Isolate(FaultCell faults, Action publish, Op key) =>
        key.Catch(publish).Match(Succ: static _ => unit, Fail: cause => ignore(faults.Park(point: Rail, cause: cause)));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
