# [RASM_DISPATCH]

`Rasm.Interaction` owns the branch's one crossing into the Eto control tree. A crossing is a VALUE, so a synchronous assert, an in-frame invoke, and a queued post are three request records under one generic case container rather than sibling entry names at each host boundary. Every crossing is gauged against its lane's frame budget, every refusal is a case of one fault family banded at the kernel registry, and a headless process refuses at the entry instead of dead-locking on an application that was never started.

Both host boundaries reach this owner directly and neither adapts it: the Rhino command-thread affinity axis and the Grasshopper canvas session are genuinely host-specific and stay there, while the marshal itself, its budget, its stall evidence, and its fault vocabulary are one kernel body both compose. The gauge is `Parametric/projections`' — `DispatchLane` realizes `IGaugeLane<DispatchLane>` so the lane's bound has one owner, and the pace it derives from is `PaceBand`, the same band the motion drive paces against.

## [01]-[INDEX]

- [02]-[MODALITY]: `UiDispatch<T>`, `DispatchLane` — the three crossing requests and the gauge lane roster that budgets them.
- [03]-[THREAD]: `UiThread` — the three marshal overloads, the affinity probe, the gauge tune, and the observer lease.
- [04]-[PULSE]: `StallPolicy`, `GaugedSpan<DispatchLane>` — the seated pace and gauged crossing evidence.
- [05]-[FAULT]: `RejectReason`, `UiFault`, `FaultGate` — the refusal-clause vocabulary, the closed refusal family on the kernel `FaultBand.Interaction` row, and the host-raise projection every surface on this sub-domain routes through.

## [02]-[MODALITY]

- Owner: `UiDispatch<T>` the static generic container whose three request records each carry the `Func<Fin<T>>` body they marshal; `DispatchLane` the `[SmartEnum<int>]` gauge roster realizing `IGaugeLane<DispatchLane>`, whose column is a FRAME MULTIPLE rather than a wall-clock span, so one roster serves a sixty-hertz display and a portable panel without a second table.
- Cases: `Current` asserts affinity and refuses off-thread; `Blocking` runs in-frame when already on the marshal and invokes otherwise; `Queued` posts and returns the body's eventual result.
- Cases: `DispatchLane` carries five rows — `Immediate`, `Interactive`, `Deferred`, `Modal`, and `Paced`, whose one-frame multiple against the pace band's `Period` makes it the display-link tick's own budget without a sixth number anywhere.
- Entry: `UiThread.Run(crossing, lane)` is the ONE entry name; the concrete request type selects the overload, so `Current` and `Blocking` land `Fin<T>` inside the caller's own LINQ query while `Queued` lands `ValueTask<Fin<T>>`.
- Auto: the lane's `Bound` derives as `Pace.Period × Frames × Stretch`, so the frame period has ONE owner — the seated `PaceBand` — and a host widening one lane states a dimensionless multiple rather than a second millisecond table. The two hand-written millisecond tables both boundaries carry today have no place to reappear.
- Law: the concrete request type selects both the Eto operation and the result carrier, so no forwarding interface or root projection stands between a caller and the body it supplied.
- Law: `Queued` returns the body's eventual result, so a caller distinguishes queue admission from settled success without a second observer surface.
- Law: a `ref struct` never crosses a case body — a span-shaped result copies into owned storage before the crossing, because a case body is a delegate boundary and closure capture is the same escape as a box.
- Law: NAMED LOSS — the census `DispatchLane.Marshal` column does not land. The crossing CASE is the marshal choice; a lane naming a marshal shape beside a case naming another is two authorities over one crossing, and the disagreement is unresolvable at the call site. Witness: a lane-selected marshal reads as `UiThread.Run(new UiDispatch<T>.Blocking(body), DispatchLane.Immediate)`, where the lane budgets and the case marshals.
- Growth: a new crossing modality is one request record and one `Run` overload; a new budget lane is one row the seated pace already budgets.
- Boundary: Rhino's command-thread affinity (`HostThread`/`HostWork<T>`) is a DIFFERENT axis over the Rhino command queue and stays plural at that boundary; what re-points here are its marshal lane and its latency gauge, which were this owner's shape all along.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
public static class UiDispatch<TResult> {
    public sealed record Current(Func<Fin<TResult>> Body);
    public sealed record Blocking(Func<Fin<TResult>> Body);
    public sealed record Queued(Func<Fin<TResult>> Body);
}

[SmartEnum<int>]
public sealed partial class DispatchLane : IGaugeLane<DispatchLane> {
    public static readonly DispatchLane Immediate = new(key: 0, frames: 1d);
    public static readonly DispatchLane Interactive = new(key: 1, frames: 4d);
    public static readonly DispatchLane Deferred = new(key: 2, frames: 6d);
    public static readonly DispatchLane Modal = new(key: 3, frames: 1d);
    public static readonly DispatchLane Paced = new(key: 5, frames: 1d);

    public double Frames { get; }

    public TimeSpan Bound => StallPolicy.Seated.Bound(lane: this);
}
```

## [03]-[THREAD]

- Owner: `UiThread` — the one marshal surface: the three concrete crossing overloads, the affinity probe, the gauge tune, and the gauge observer lease.
- Entry: `Run` dispatches the concrete crossing case, brackets the body in the lane's gauged span, and lands the case's result carrier; `IsUIThread` is the PUBLISHED affinity probe so no consumer inlines `Application.Instance.IsUIThread` and no consumer reads an absent instance as "not on the thread"; `Tune` seats the pace and the gauge clock; `Watch` leases the gauge observer.
- Auto: every entry admits the application through `Optional(Application.Instance).ToFin(new KernelFault.MissingContext())`, so a headless process — a test host, a compute node, a CLI — refuses TYPED at the entry rather than blocking on a marshal that will never run. That refusal is the reason the probe is published: an affinity test that answers `false` in a headless process is indistinguishable from one that answers `false` on a worker thread, and only one of them is recoverable.
- Auto: the gauged bracket is `MonotonicTimeline.Gauged<T, DispatchLane>`, which takes NO bound — the lane row owns it — and answers `(Fin<T> Value, GaugedSpan<DispatchLane> Span)`, so the span lands on a refused body too and the gauge publishes from every crossing rather than from the settled ones alone.
- Law: the marshal is the only site that names an Eto application — every interior owner on this sub-domain takes admitted values and returns results, so the host surface is one page wide.
- Law: a crossing never swallows: a body's `Fin` failure rides out unchanged, and only the CROSSING itself — a dead context, a refused post, a headless entry — becomes a `UiFault`.
- Law: an observer raise never fails the crossing it observed. `UiThread` captures each publication through `Try.lift(...).Run().Match(...)`, parks the subscriber's refusal under its own `HookId`, and settles the crossing, because a watcher that can fail a marshal turns instrumentation into a liveness dependency.
- Output: `GaugedSpan<DispatchLane>` per crossing, published to leased observers rather than returned, so a caller that wants only the value pays nothing.
- Packages: Eto.Forms for `Application` (registered at `libs/dotnet/.api/api-eto-runtime.md`); LanguageExt.Core for the types and the `Lease`; `Parametric/projections` for the timeline and the gauge lane floor.
- Growth: a new observer is one lease; a new lane is one row the seated pace already budgets.
- Boundary: `Application.Instance` is read at the entry and never stored — a captured instance outlives a host restart and marshals onto a dead context that never re-posts.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using Rasm.Domain;
using Rasm.Parametric;

namespace Rasm.Interaction;

// --- [SERVICES] ------------------------------------------------------------------------
public static class UiThread {
    private static readonly HookId Point = HookId.Create(value: "rasm.kernel.interaction.dispatch");

    public static Fin<T> Run<T>(UiDispatch<T>.Current crossing, DispatchLane lane);
    public static Fin<T> Run<T>(UiDispatch<T>.Blocking crossing, DispatchLane lane);
    public static ValueTask<Fin<T>> Run<T>(UiDispatch<T>.Queued crossing, DispatchLane lane);

    public static Fin<bool> IsUIThread();

    public static Fin<Transition<StallPolicy>> Tune(StallPolicy policy, Option<MonotonicTimeline> clock = default);

    public static Fin<Lease<IDisposable>> Watch(Action<GaugedSpan<DispatchLane>> observer);
}
```

## [04]-[PULSE]

- Owner: `StallPolicy` seats the pace band and per-lane stretch; `GaugedSpan<DispatchLane>` is the gauged crossing evidence.
- Auto: the seat commits on the kernel default `Cell.SwapBudget` (branch RULINGS `[02]`) — this page measures no divergent contention and a one-member budget shell beside the kernel row is two authorities over one ceiling.
- Auto: `Breached` is DERIVED from the span and its bound, never a stored flag — `GaugedSpan` already answers it and a second stored verdict is a fabricated measurement the moment a bound moves.
- Auto: the seat is an atom holding the PORTABLE band until a host tunes, so a lane's `Bound` is answerable before any tune and a display migration re-seats one value rather than every lane row. `PaceBand.Portable` is the DECLARED row both boundaries hardcoded as the same sixteen-millisecond frame before this owner existed.
- Law: a per-lane widening is a dimensionless STRETCH on the seated policy, never a millisecond override. A stored millisecond bound beside a frame multiple is two authorities over one budget, and the pair disagrees the first time a host publishes a real refresh interval. NAMED LOSS: a host can no longer state one lane's budget in absolute time; it states the multiple and the pace, and the product is the budget.
- Law: every seat transition answers a `Transition<StallPolicy>` verdict, so a tune that lost a race under contention reads its case rather than assuming a swap.
- Output: `GaugedSpan<DispatchLane>` carries the lane, elapsed span, and derived breach.
- Growth: a new measured coordinate belongs to the gauge owner that produces it; this page adds no wrapper column.
- Boundary: the gauge is EVIDENCE and never a gate — no crossing branches on a prior span, because a budget that steers the next crossing turns a measurement into a feedback loop nothing declared.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Parametric;

namespace Rasm.Interaction;

// --- [POLICIES] ------------------------------------------------------------------------
[Thinktecture.ComplexValueObject]
public sealed partial class StallPolicy {
    public static readonly StallPolicy Portable = Create(PaceBand.Portable, HashMap<DispatchLane, double>());
    private static readonly Atom<StallPolicy> seat = Atom(Portable);
    public PaceBand Pace { get; }
    public HashMap<DispatchLane, double> Stretch { get; }
    internal static StallPolicy Seated => seat.Value;
    internal static Transition<StallPolicy> Seat(StallPolicy policy) => Cell.Commit(seat, _ => policy);
    internal TimeSpan Bound(DispatchLane lane) => Pace.Period * lane.Frames * Stretch.Find(lane).IfNone(1d);
    static partial void ValidateFactoryArguments(ref Thinktecture.ValidationError? validationError,
        ref PaceBand pace, ref HashMap<DispatchLane, double> stretch) =>
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (pace is null, static () => new ValidationClause("a pace band")),
            (stretch.Values.Exists(static factor => !double.IsFinite(factor) || factor <= 0d),
                static () => new ValidationClause("positive finite stretch values"))));
}
```

## [05]-[FAULT]

- Owner: `UiFault` — the closed refusal family for every interaction crossing, deriving `Fault` and coded off the kernel `FaultBand.Interaction` row; `RejectReason` — the sub-domain's one refusal-clause vocabulary; `FaultGate` — the host-raise projection that classifies raised failures without rewriting returned failures.
- Cases: `Dismissed` (the user closed the surface), `OffThread` (a `Current` crossing ran off the marshal), `Rejected` (a `FieldTag` refused under a `RejectReason` row), `AbsentPayload` (a transfer read found nothing of the wanted format), `HostRejected` (the platform refused the operation), and `Released` (the surface was torn down mid-crossing).
- Auto: each `[FaultCase]` ordinal generates the cached numeric identity against the root's one `FamilyBand`; `Message` is the family-owned total presentation fold.
- Law: the root derives `Fault` directly. Numeric identity is generated, while the case payload and total message projection remain authored here.
- Law: the band is the kernel registry's `FaultBand.Interaction` row and every code is `Code(offset)` off it; the registry's disjointness proof keeps this family clear of the app-platform bands, and every host composes this one UI-refusal family.
- Law: cancellation remains `KernelFault.Cancelled` or `Errors.Cancelled`; interaction faults do not duplicate that posture.
- Law: recovery reads the case and immutable posture, never rendered text. A generic wire may project numeric code and recovery only; no category or case-name mirror crosses with it. Every payload identity column is admitted — `Rejected` names a `FieldTag` beside a `RejectReason` row.
- Law: a refusal clause is a `RejectReason` singleton and its sentence rides that value's `Requirement`. A joined-sentence reason column is the deleted form: a caller filtering on which clause fired had to parse prose, and a surface rendering it had no clause to name. A refusal violating several clauses lowers one fault per singleton through `Error.Many`, so the whole violated set survives.
- Boundary: `FieldTag` (`Interaction/control`) is an IDENTITY value object on this sub-domain's own assembly, carrying no behaviour this family could invert onto.
- Law: an observer fault and an operation fault are different postures. `Capture` lifts a platform raise onto the result as `HostRejected`; each observer producer captures publication directly and parks a subscriber's raise under its own `HookId`, so a watcher storm sheds into a bounded ring rather than failing the surface that published.
- Growth: a new refusal is one case, one message arm, declaration-order ordinal compaction, and the matching `FaultBand.Interaction.Span` edit; a new refusal CLAUSE is one `RejectReason` row and no case edit.
- Boundary: a documented platform raise funnels through `FaultGate.Capture` and lands as `HostRejected` carrying the exact captured `Error` cause; a returned failed result passes through unchanged.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
public sealed class RejectReason {
    private RejectReason(string requirement) => Requirement = requirement;
    // --- [FUSION]
    public static readonly RejectReason SeedFlow = new(requirement: "one-time flow and seed input imply each other");
    public static readonly RejectReason SeedTiming = new(requirement: "seed input admits edit timing alone");
    public static readonly RejectReason DelayedPath = new(requirement: "delayed timing requires a context path relaying into the control");
    public static readonly RejectReason CommitFlow = new(requirement: "commit timing requires a flow relaying into the source");
    public static readonly RejectReason ManualTiming = new(requirement: "manual flow admits edit timing alone");

    // --- [BOUNDARY]
    public static readonly RejectReason ControlType = new(requirement: "a control of the type the plan selects its binding on");

    // --- [CHROME]
    public static readonly RejectReason SheetInset = new(requirement: "margins leaving a drawable extent inside the laid sheet");
    public static readonly RejectReason PageSpan = new(requirement: "selected pages forming an ordered subset of the job");
    public static readonly RejectReason UnmatchedLeave = new(requirement: "a mount visit entered before it is left");
    public static readonly RejectReason TrayAnchor = new(requirement: "a tray anchor where the platform's own notification demands one");
    public static readonly RejectReason PackedRows = new(requirement: "a row buffer packed to the extent and carriage it declares");
    public static readonly RejectReason RootFaceSize = new(requirement: "a root face naming the size a type scale multiplies");

    public string Requirement { get; }
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UiFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Interaction;
    private UiFault() { }

    [FaultCase(0)] public sealed partial record Dismissed() : UiFault;
    [FaultCase(1)] public sealed partial record OffThread() : UiFault;
    [FaultCase(2)] public sealed partial record Rejected(FieldTag Field, RejectReason Reason) : UiFault;
    [FaultCase(3)] public sealed partial record AbsentPayload(Mime Wanted) : UiFault;
    [FaultCase(4)] public sealed partial record HostRejected(Error Cause) : UiFault, ICausedFault;
    [FaultCase(5)] public sealed partial record Released() : UiFault;

    public sealed override string Message => Switch(
        dismissed:     static _ => "Interaction was dismissed.",
        offThread:     static _ => "Interaction requires the UI thread.",
        rejected:      static fault => $"Field '{fault.Field.Value}' requires {fault.Reason.Requirement}.",
        absentPayload: static fault => $"No payload matches '{fault.Wanted.Value}'.",
        hostRejected:  static fault => $"Host rejected interaction: {fault.Cause.Message}",
        released:      static _ => "Interaction reached a released surface.");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FaultGate {
    public static Fin<T> Capture<T>(Func<Fin<T>> body) =>
        Try.lift(body).Run().BiBind(
            Succ: static inner => inner,
            Fail: static cause => Fin.Fail<T>(new UiFault.HostRejected(cause)));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
