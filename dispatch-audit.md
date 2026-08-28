# 1. Replace the unused union with a case container

[02]-[MODALITY] code fence — lines 36–50, crossing interfaces and `UiDispatch<TResult>` declaration

From:

```csharp
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
```

To:

```csharp
public static class UiDispatch<TResult> {
    public sealed record Current(Func<Fin<TResult>> Body);
    public sealed record Blocking(Func<Fin<TResult>> Body);
    public sealed record Queued(Func<Fin<TResult>> Body);
```

Why: No caller holds or dispatches the root value once `Run` accepts concrete cases; the union, two forwarding interfaces, and identity projection generate a surface that has no consumer. `Current`, `Blocking`, and `Queued` have call sites, while `Pumped` and `Awaited` do not.

Change: Retain the three used request records under a static generic container and delete the unused generated union and modalities.

Delta: Net −11 LOC, −4 types, −3 forwarding property members, and the generated `Switch`/`Map` surface.

# 2. Delete the repeated body fold

[02]-[MODALITY] code fence — lines 52–57, `UiDispatch<TResult>.Body`

From:

```csharp
public Func<Fin<TResult>> Body => Switch(
    current: static crossing => crossing.Body,
    blocking: static crossing => crossing.Body,
    pumped: static crossing => crossing.Body,
    awaited: static crossing => crossing.Body,
    queued: static crossing => crossing.Body);
```

To:

```csharp
// UiDispatch<TResult>.Body DELETED
```

Why: Each concrete case already owns `Body`; the root property repeats an identical projection across every arm.

Change: Delete the fold and read the case payload in the matching `Run` overload.

Delta: Net −6 LOC and −1 member.

# 3. Dispatch on the concrete case types

[03]-[THREAD] code fence — lines 99–100, `UiThread.Run`

From:

```csharp
public static Fin<T> Run<T>(ISyncCrossing<T> crossing, DispatchLane lane);
public static ValueTask<Fin<T>> Run<T>(IAsyncCrossing<T> crossing, DispatchLane lane);
```

To:

```csharp
public static Fin<T> Run<T>(UiDispatch<T>.Current crossing, DispatchLane lane);
public static Fin<T> Run<T>(UiDispatch<T>.Blocking crossing, DispatchLane lane);
public static ValueTask<Fin<T>> Run<T>(UiDispatch<T>.Queued crossing, DispatchLane lane);
```

Why: The concrete parameter type selects both the Eto operation and its synchronous or asynchronous result carrier without an open interface seam.

Change: Replace the interface overloads with overloads for the three retained cases.

Delta: Net +1 LOC and +1 method; no type is added.

Ripples: Remove the undefined third `active` argument from `UiThread.Run` calls in `libs/dotnet/Rasm.Grasshopper/.planning/Document/history.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Document/solution.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Shell/chrome.md`, and `libs/dotnet/Rasm.Grasshopper/.planning/Shell/editor.md`; the crossing and lane are the complete entry contract.

# 4. Delete the unused background budget

[02]-[MODALITY] code fence — line 66, `DispatchLane.Background`

From:

```csharp
public static readonly DispatchLane Background = new(key: 4, frames: 1d);
```

To:

```csharp
// DispatchLane.Background DELETED
```

Why: No dispatch or gauge site selects this row. `Paced` remains keyed at `5` because its key is emitted as telemetry identity.

Change: Delete the unreferenced lane without renumbering the published rows.

Delta: Net −1 LOC and −1 row member.

# 5. Narrow the thread surface to retained evidence

[03]-[THREAD] code fence — lines 102–110, affinity, tuning, observer, and snapshot members

From:

```csharp
public static Fin<bool> OnMarshal();

public static Fin<Unit> Tune(StallPolicy policy, Option<MonotonicTimeline> clock = default);

public static Fin<Lease<IDisposable>> Watch(Action<DispatchPulse> observer);
public static Fin<Lease<IDisposable>> Tap(Action<DispatchEcho> observer);

public static Option<DispatchPulse> LastPulse { get; }
public static Option<DispatchPulse> LastStall { get; }
```

To:

```csharp
public static Fin<bool> IsUIThread();

public static Fin<Transition<StallPolicy>> Tune(StallPolicy policy, Option<MonotonicTimeline> clock = default);

public static Fin<Lease<IDisposable>> Watch(Action<GaugedSpan<DispatchLane>> observer);
```

Why: The affinity probe projects Eto's `IsUIThread`; tuning must preserve `StallPolicy.Seat`'s contention verdict; the pulse only forwards `GaugedSpan<DispatchLane>`; and no code reads either global snapshot or the echo observer.

Change: Use the host predicate name, return the existing transition, publish the gauge directly, and delete the unused echo and snapshots.

Delta: Net −4 LOC and −3 members; no type is added.

Ripples: Rename `UiThread.OnMarshal` in `libs/dotnet/Rasm.Grasshopper/.planning/Document/solution.md`; change `FrameTune.Feed` in `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md` to return `Fin<Transition<StallPolicy>>`; keep the explicit tune discard and remove `LastPulse` prose in `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md`; change `GhInstruments.Marshalled` in `libs/dotnet/Rasm.Grasshopper/.planning/Shell/telemetry.md` to consume `GaugedSpan<DispatchLane>`; remove `Tap` and `DispatchEcho` prose from `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md` and `libs/dotnet/Rasm.Grasshopper/.planning/Shell/session.md`.

# 6. Delete the gauge wrapper

[04]-[PULSE] code fence — lines 149–155, `DispatchPulse`

From:

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct DispatchPulse(GaugedSpan<DispatchLane> Span) : IValidityEvidence {
    public DispatchLane Lane => Span.Lane;
    public TimeSpan Elapsed => Span.Elapsed;
    public bool Breached => Span.Breached;
    public bool IsValid => Span.IsValid;
}
```

To:

```csharp
// DispatchPulse DELETED
```

Why: The type adds no identity, validation, or policy and forwards the complete gauge result through five members.

Change: Delete the wrapper and use `GaugedSpan<DispatchLane>` directly.

Delta: Net −7 LOC, −1 type, and −5 members.

Ripples: Replace `DispatchPulse` with `GaugedSpan<DispatchLane>` in `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md` and `libs/dotnet/Rasm.Grasshopper/.planning/Shell/telemetry.md`.

# 7. Delete the queued-outcome wrapper

[04]-[PULSE] code fence — lines 157–158, `DispatchEcho`

From:

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct DispatchEcho(Fin<Unit> Outcome);
```

To:

```csharp
// DispatchEcho DELETED
```

Why: The type is a one-field rename of `Fin<Unit>` and does not carry the operation evidence claimed by the specification.

Change: Delete the wrapper; the queued caller observes the existing eventual result directly.

Delta: Net −2 LOC, −1 type, and −1 member.

Ripples: Remove `DispatchEcho` and `Tap` references from `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md` and `libs/dotnet/Rasm.Grasshopper/.planning/Shell/session.md`; retain `SettleDeferred(ValueTask<Fin<Unit>>, FaultCell)` as the concrete queued-failure consumer.

# 8. Admit stall policies at construction

[04]-[PULSE] code fence — lines 134–146, `StallPolicy`

From:

```csharp
public sealed record StallPolicy(PaceBand Pace, HashMap<DispatchLane, double> Stretch) {
    public static readonly StallPolicy Portable = new(Pace: PaceBand.Portable, Stretch: HashMap<DispatchLane, double>());

    private static readonly Atom<StallPolicy> seat = Atom(Portable);

    internal static StallPolicy Seated => seat.Value;

    internal static Transition<StallPolicy> Seat(StallPolicy policy) =>
        Cell.Commit(seat, _ => policy);

    internal TimeSpan Bound(DispatchLane lane) =>
        Pace.Period * lane.Frames * Stretch.Find(lane).IfNone(1d);
}
```

To:

```csharp
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

Why: The positional constructor admits a null pace and nonpositive or nonfinite factors, allowing `Bound` to produce an invalid duration. The generated owner admits both independent conditions once.

Change: Replace the raw record constructor with Thinktecture construction and one accumulating validation hook.

Delta: Net +3 LOC and +1 authored validation member; the type and its two value properties are unchanged, and the public raw constructor is removed.

Ripples: In `libs/dotnet/Rasm.Grasshopper/.planning/Eto/runtime.md` and `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md`, replace direct construction with `StallPolicy.Validate(...)` lifted by `FactoryBridge.Lift<StallPolicy>`; `FactoryBridge.Accept` is not the complex-value-object bridge.

# 9. Replace the unused generated reason roster

[05]-[FAULT] code fence — lines 184–192, `RejectReason` declaration and fusion rows

From:

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RejectReason {
public static readonly RejectReason SeedFlow = new(key: "seed-flow", requirement: "one-time flow and seed input imply each other");
public static readonly RejectReason SeedTiming = new(key: "seed-timing", requirement: "seed input admits edit timing alone");
public static readonly RejectReason DebouncedPath = new(key: "debounced-path", requirement: "debounced timing requires a context path relaying into the control");
public static readonly RejectReason CommitFlow = new(key: "commit-flow", requirement: "commit timing requires a flow relaying into the source");
public static readonly RejectReason ManualTiming = new(key: "manual-timing", requirement: "manual flow admits edit timing alone");
```

To:

```csharp
public sealed class RejectReason {
private RejectReason(string requirement) => Requirement = requirement;
public static readonly RejectReason SeedFlow = new(requirement: "one-time flow and seed input imply each other");
public static readonly RejectReason SeedTiming = new(requirement: "seed input admits edit timing alone");
public static readonly RejectReason DebouncedPath = new(requirement: "debounced timing requires a context path relaying into the control");
public static readonly RejectReason CommitFlow = new(requirement: "commit timing requires a flow relaying into the source");
public static readonly RejectReason ManualTiming = new(requirement: "manual flow admits edit timing alone");
```

Why: No consumer enumerates or looks up the roster, dispatches on a reason, or reads its key. The fusion keys also duplicate the corresponding `FusionLaw` keys.

Change: Replace the generated smart enum with a private-constructor class and remove the fusion-row key strings.

Delta: Net −1 LOC and +1 authored constructor; no type or row is added, and the generated key, `Items`, lookup, conversion, comparison, and formatting surface is removed.

# 10. Remove boundary-clause key strings

[05]-[FAULT] code fence — lines 195–198, boundary `RejectReason` rows

From:

```csharp
public static readonly RejectReason NoChildPath = new(key: "no-child-path", requirement: "a source shape carrying a live child path");
public static readonly RejectReason ControlType = new(key: "control-type", requirement: "a control of the type the plan selects its binding on");
public static readonly RejectReason EmptyLatch = new(key: "empty-latch", requirement: "a latch holding a pending write");
public static readonly RejectReason Capacity = new(key: "capacity", requirement: "an admitted positive bound");
```

To:

```csharp
public static readonly RejectReason NoChildPath = new(requirement: "a source shape carrying a live child path");
public static readonly RejectReason ControlType = new(requirement: "a control of the type the plan selects its binding on");
public static readonly RejectReason EmptyLatch = new(requirement: "a latch holding a pending write");
public static readonly RejectReason Capacity = new(requirement: "an admitted positive bound");
```

Why: These singleton rows are matched by identity and rendered through `Requirement`; their key strings have no consumer.

Change: Remove the four constructor keys after the owner becomes keyless.

Delta: Net 0 LOC, types, and authored members; four duplicate strings are removed.

# 11. Remove chrome-clause keys and the unused page clause

[05]-[FAULT] code fence — lines 201–207, chrome `RejectReason` rows

From:

```csharp
public static readonly RejectReason SheetInset = new(key: "sheet-inset", requirement: "margins leaving a drawable extent inside the laid sheet");
public static readonly RejectReason PageSpan = new(key: "page-span", requirement: "selected pages forming an ordered subset of the job");
public static readonly RejectReason HostSelection = new(key: "host-selection", requirement: "a host publishing a current selection to print");
public static readonly RejectReason UnmatchedLeave = new(key: "unmatched-leave", requirement: "a mount visit entered before it is left");
public static readonly RejectReason TrayAnchor = new(key: "tray-anchor", requirement: "a tray anchor where the platform's own notification demands one");
public static readonly RejectReason PackedRows = new(key: "packed-rows", requirement: "a row buffer packed to the extent and carriage it declares");
public static readonly RejectReason RootFaceSize = new(key: "root-face-size", requirement: "a root face naming the size a type scale multiplies");
```

To:

```csharp
public static readonly RejectReason SheetInset = new(requirement: "margins leaving a drawable extent inside the laid sheet");
public static readonly RejectReason HostSelection = new(requirement: "a host publishing a current selection to print");
public static readonly RejectReason UnmatchedLeave = new(requirement: "a mount visit entered before it is left");
public static readonly RejectReason TrayAnchor = new(requirement: "a tray anchor where the platform's own notification demands one");
public static readonly RejectReason PackedRows = new(requirement: "a row buffer packed to the extent and carriage it declares");
public static readonly RejectReason RootFaceSize = new(requirement: "a root face naming the size a type scale multiplies");
```

Why: The retained rows have no key consumer, and `PageSpan` has no raising or matching site; the generated `PageSpan` owner already admits page order and bounds.

Change: Remove the six retained key strings and delete the duplicate page clause.

Delta: Net −1 LOC and −1 row member; seven duplicate strings are removed.

# 12. Delete unraised interaction faults

[05]-[FAULT] code fence — lines 218–227, `UiFault` cases

From:

```csharp
[FaultCase(0)] public sealed partial record Dismissed() : UiFault;
[FaultCase(1)] public sealed partial record Cancelled() : UiFault;
[FaultCase(2)] public sealed partial record Unavailable(PlatformCapability Capability) : UiFault;
[FaultCase(3)] public sealed partial record OffThread() : UiFault;
[FaultCase(4)] public sealed partial record Rejected(FieldTag Field, RejectReason Reason) : UiFault;
[FaultCase(5)] public sealed partial record AbsentPayload(Mime Wanted) : UiFault;
[FaultCase(6)] public sealed partial record HostRejected(Error Cause) : UiFault, ICausedFault;
[FaultCase(7)] public sealed partial record Released() : UiFault;
[FaultCase(8)] public sealed partial record Headless() : UiFault;
[FaultCase(9)] public sealed partial record Absent(string Member) : UiFault;
```

To:

```csharp
[FaultCase(0)] public sealed partial record Dismissed() : UiFault;
[FaultCase(1)] public sealed partial record OffThread() : UiFault;
[FaultCase(2)] public sealed partial record Rejected(FieldTag Field, RejectReason Reason) : UiFault;
[FaultCase(3)] public sealed partial record AbsentPayload(Mime Wanted) : UiFault;
[FaultCase(4)] public sealed partial record HostRejected(Error Cause) : UiFault, ICausedFault;
[FaultCase(5)] public sealed partial record Released() : UiFault;
```

Why: `Cancelled`, `Unavailable`, and string-only `Absent` have no construction or recovery site. Headless dispatch already returns `KernelFault.MissingContext`. `OffThread` and `Released` remain because both have concrete consumers and preserve distinct recovery cases.

Change: Delete the four unraised leaves and compact the retained ordinals.

Delta: Net −4 LOC and −4 nested types.

Ripples: Change `FaultBand.Interaction` span from `10` to `6` in `libs/dotnet/Rasm/.planning/Domain/results.md`; remove the deleted-case prose in the target sheet. Cancellation continues through `KernelFault.Cancelled` or `Errors.Cancelled`.

# 13. Render fault payloads that exist

[05]-[FAULT] code fence — lines 229–239, `UiFault.Message`

From:

```csharp
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
```

To:

```csharp
public sealed override string Message => Switch(
    dismissed:     static _ => "Interaction was dismissed.",
    offThread:     static _ => "Interaction requires the UI thread.",
    rejected:      static fault => $"Field '{fault.Field.Value}' requires {fault.Reason.Requirement}.",
    absentPayload: static fault => $"No payload matches '{fault.Wanted.Value}'.",
    hostRejected:  static fault => $"Host rejected interaction: {fault.Cause.Message}",
    released:      static _ => "Interaction reached a released surface.");
```

Why: Regular-union leaves expose no `Key`; every existing arm therefore reads a nonexistent member. The retained payloads and generated fault identity already carry all discriminating evidence.

Change: Remove invalid key interpolation and the four deleted case arms.

Delta: Net −4 LOC; no type or member count changes.

Ripples: In `libs/dotnet/Rasm.Grasshopper/.planning/Shell/chrome.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Shell/editor.md`, `libs/dotnet/Rasm.Rhino/.planning/HostUi/dialogs.md`, `libs/dotnet/Rasm.Rhino/.planning/HostUi/pages.md`, `libs/dotnet/Rasm.Rhino/.planning/HostUi/panels.md`, and `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md`, wrap each detail string with `Error.New(...)` as the `Cause`, remove the extra `Key`, and construct `Released` without the unsupported key payload.

# 14. Capture host failures and delete observer isolation

[05]-[FAULT] code fence — lines 243–251, `FaultGate`

From:

```csharp
public static class FaultGate {
    private static readonly HookId Point = HookId.Create(value: "rasm.kernel.interaction.dispatch");

    public static Fin<T> Host<T>(Func<Fin<T>> body) =>
        Try.lift(body).Run().Bind(static inner => inner);

    public static Unit Isolate(FaultCell faults, Action publish) =>
        Try.lift(publish).Run().Match(Succ: static _ => unit, Fail: cause => ignore(faults.Park(point: Point, cause: cause)));
}
```

To:

```csharp
public static class FaultGate {
    public static Fin<T> Capture<T>(Func<Fin<T>> body) =>
        Try.lift(body).Run().BiBind(
            Succ: static inner => inner,
            Fail: static cause => Fin.Fail<T>(new UiFault.HostRejected(cause)));
}
```

Why: `Host` currently forwards the nested result without creating the promised `HostRejected` case. Separating the outer failure maps only a raised host error and preserves a returned `Fin` failure. `Isolate` is a single-call wrapper that also misattributes every clock, paint, and platform observer failure to dispatch.

Change: Rename the host operation with an action verb, classify the outer failure through `BiBind`, and delete the observer wrapper and its global point.

Delta: Net −3 LOC, −1 field, and −1 method; the type count is unchanged.

Ripples: Rename the four `FaultGate.Host` calls to `FaultGate.Capture` and remove the unsupported extra `key` argument in `libs/dotnet/Rasm/.planning/Interaction/platform.md`. Inline the `Try.lift(...).Run().Match(...)` publication at `UiThread`, `UiClock.Publish` in `libs/dotnet/Rasm/.planning/Interaction/clock.md`, `Handlers.Census` in `libs/dotnet/Rasm/.planning/Interaction/platform.md`, and the surface paint callback in `libs/dotnet/Rasm/.planning/Interaction/paint.md`; each parks with its producer-owned `HookId`, including `UiClock.Point`.
