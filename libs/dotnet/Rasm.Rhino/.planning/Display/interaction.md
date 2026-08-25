# [RASM_RHINO_DISPLAY_INTERACTION]

`DisplayHooks.Mount`, `Gumballs.Mount`, and `WidgetHost` own the three Rhino viewport-interaction modalities. Each admits host input once, emits bounded value facts, and confines callback arguments, mouse state, acquisition handles, and registered UI objects to its lease.

The viewport pointer seam is HOST-SPECIFIC and stays whole — `MouseCallbackEventArgs` carries a veto the host reads back and `RhinoView`'s static event tables have no host-neutral form — but its VERDICT vocabulary is the kernel's: a veto policy answers `Rasm.Interaction`'s `InputVerdict` precedence algebra, never a bare bool. Widget overlay painting rides the draw page's one `Marks.Paint` entry, document mutation remains caller-owned after `GumballRig.Complete` returns its evidence, and `DocumentSession` binds document-local widget registration without exporting `RhinoDoc`.

## [01]-[INDEX]

- [02]-[POINTERS]: `ChannelPlan`, callback pulse projection, the kernel verdict seam, overflow policy, and the pointer lease.
- [03]-[GUMBALL]: geometry seating, pick/update fold, grip custody, and transform evidence.
- [04]-[WIDGETS]: registered grip, direction, rotation, text-dot, SVG, and slider families over one fact channel, at the host's full axis set.
- [05]-[HOOKS]: `DisplayHooks` seats the two display hook points as TYPED kernel bindings.

## [02]-[POINTERS]

- Owner: `ChannelPlan` is the one bounded-channel triple both display grants admit; `ViewportPointerFact` carries phase, edge, viewport identity, point, button, the modifier set, gumball occupancy, the veto verdict, and monotonic ordinal; `PointerPulse` is the ten-row callback table; `PointerLease` owns the mounted hook.
- Entry: `DisplayHooks.Mount` binds the admitted channel plan and veto responder directly to a `PointerLease`.
- Law: the pointer seam IS veto-capable and the veto answers the KERNEL's `InputVerdict` — `MouseCallbackEventArgs` derives `CancelEventArgs`, so a Begin edge asks the mount's admitted responder and writes `Cancel` for a `Handled` or `Capture` verdict, while `Ignored` and `Release` let Rhino's own default handling run; the matching End edge READS `Cancel` back. A bool cannot distinguish the four postures a nested responder tree resolves on, which is the kernel's own law and the reason no second verdict vocabulary exists here.
- Law: the veto's answer and the default's outcome are TWO facts one bool was spelling — `PointerVerdict` closes them as cases: a Begin fact carries `Admitted` or `Vetoed`, an End fact carries `DefaultRan` or `DefaultSuppressed`, and an atomic fact carries `Admitted` because its seam offers nothing to cancel.
- Law: the ten host callbacks are ROWS — `PointerPulse` pairs each override with its phase and edge, so an override is one delegation and the pairing has one authority; the modifier pair rides a `CapabilitySet<PointerModifier>` and gumball occupancy carries the host's own `GumballMode` through `HostRow` rather than discarding the discriminant into a bool.
- Law: arming crosses the marshal seam — `MouseCallback.Enabled` reflects over the subclass to subscribe and unsubscribe the host's own static view-event tables, so both edges run through `HostThread.Run`, and a mount whose arm refuses never returns a live lease.
- Law: retire closes callback admission, disables the hook, settles admitted callbacks within the plan-owned bound, completes the channel, and then snapshots final totals; the gate refuses a close issued from a thread already holding a claim, since such a close waits on its own release.
- Law: the bounded drain rides the SCHEDULER, never the closing caller's thread — `LifecycleGate.Begin` arms the close, runs the stop step inline so a marshalled arm keeps its seam, and hands back the completion a host-thread owner settles off-thread, while `Close` awaits that same completion for a pool caller; a blocking drain on the host thread stalls exactly the callbacks it waits to see released.
- Law: callback faults PARK on the lease's bounded `FaultCell` under the page's own rail point — an unbounded `Atom<Seq<Error>>` ledger grows for process life under a pointer storm, and the cell's `Parked`, `Shed`, and declined parks all read as numbers.
- Boundary: `MouseCallbackEventArgs` and `MouseCallback` never cross the callback adapter.
- Packages: RhinoCommon `Rhino.UI.MouseCallback`/`MouseCursor` (`.api/api-rhinocommon-display.md`); `Rasm.Interaction` (`InputVerdict`); `Rasm.Domain` (`FaultCell`, `HookId`, `Cell`); `System.Threading.Channels`; LanguageExt.Core; `Rasm.Rhino.Document` (`LifecycleGate` — `Document/lifetime.md`).

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Threading.Channels;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.HostUi;
using Rasm.Rhino.Viewport;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.UI;
using Rhino.UI.Gumball;
using Thinktecture;

namespace Rasm.Rhino.Display;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PointerPhase {
    public static readonly PointerPhase Move = new(0);
    public static readonly PointerPhase Down = new(1);
    public static readonly PointerPhase Up = new(2);
    public static readonly PointerPhase DoubleClick = new(3);
    public static readonly PointerPhase Enter = new(4);
    public static readonly PointerPhase Hover = new(5);
    public static readonly PointerPhase Leave = new(6);
}

[SmartEnum<int>]
public sealed partial class PointerEdge {
    public static readonly PointerEdge Begin = new(0);
    public static readonly PointerEdge End = new(1);
    public static readonly PointerEdge Atomic = new(2);
}

[SmartEnum<int>]
public sealed partial class PointerButton {
    public static readonly PointerButton None = new(0);
    public static readonly PointerButton Left = new(1);
    public static readonly PointerButton Right = new(2);
    public static readonly PointerButton Middle = new(3);

    internal static PointerButton Of(MouseButton value) => value switch {
        MouseButton.Left => Left,
        MouseButton.Right => Right,
        MouseButton.Middle => Middle,
        _ => None,
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointerModifier : ICapability<PointerModifier> {
    public static readonly PointerModifier Shift = new(key: "shift");
    public static readonly PointerModifier Control = new(key: "control");

    public static CapabilityLaw<PointerModifier> Law => CapabilityLaw<PointerModifier>.Open;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointerVerdict {
    private PointerVerdict() { }
    public sealed record Admitted : PointerVerdict;
    public sealed record Vetoed : PointerVerdict;
    public sealed record DefaultRan : PointerVerdict;
    public sealed record DefaultSuppressed : PointerVerdict;
}

[SmartEnum<int>]
public sealed partial class PointerOverflow {
    public static readonly PointerOverflow Oldest = new(0, BoundedChannelFullMode.DropOldest);
    public static readonly PointerOverflow Newest = new(1, BoundedChannelFullMode.DropNewest);
    public static readonly PointerOverflow Incoming = new(2, BoundedChannelFullMode.DropWrite);
    internal BoundedChannelFullMode Native { get; }
    internal Channel<T> Bounded<T>(Rasm.Numerics.Dimension capacity, Atom<long> rejected) =>
        Channel.CreateBounded<T>(new BoundedChannelOptions(capacity.Value) {
            FullMode = Native,
            SingleReader = false,
            SingleWriter = true,
        }, _ => ignore(rejected.Swap(static count => count + 1)));
}

public sealed record ChannelPlan(Rasm.Numerics.Dimension Capacity, PointerOverflow Overflow, TimeSpan SettleWithin);

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ViewportPointerFact(
    PointerPhase Phase,
    PointerEdge Edge,
    Guid Viewport,
    Point2d At,
    PointerButton Button,
    CapabilitySet<PointerModifier> Modifiers,
    Option<HostRow<GumballMode>> Gumball,
    PointerVerdict Verdict,
    long Ordinal);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PointerLease : IDisposable {
    private static readonly HookId Rail = HookId.Create(value: "rasm.rhino.display.pointer");

    private readonly Channel<ViewportPointerFact> channel;
    private readonly PointerHook hook;
    private readonly LifecycleGate lifecycle;
    private readonly Atom<long> rejected;
    private readonly FaultCell faults;
    private readonly Op key;
    internal PointerLease(Channel<ViewportPointerFact> channel, Atom<long> rejected, FaultCell faults, LifecycleGate lifecycle, Option<Func<ViewportPointerFact, InputVerdict>> veto, Op key) {
        (this.channel, this.rejected, this.faults, this.lifecycle, this.key) = (channel, rejected, faults, lifecycle, key);
        hook = new PointerHook(channel.Writer, rejected, faults, lifecycle, veto, key);
    }
    public ChannelReader<ViewportPointerFact> Facts => channel.Reader;
    public Seq<IsolatedFault> Faults => faults.Parked;
    public long Shed => faults.Shed;
    public long Submitted => hook.Submitted;
    public long Rejected => rejected.Value;
    public long Vetoed => hook.Vetoed;
    public Option<int> Buffered => channel.Reader.CanCount ? Some(channel.Reader.Count) : None;
    internal Fin<Unit> Enable() => Arm(enabled: true);

    private Fin<Unit> Arm(bool enabled) => HostThread.Run(
        work: new HostWork<Unit>.Execute(Body: () => key.Catch(() => Fin.Succ((hook.Enabled = enabled, unit).Item2))),
        key: key);

    private Fin<Unit> Release() => lifecycle.Close(
        stop: () => Arm(enabled: false),
        settle: () => Fin.Succ((channel.Writer.TryComplete(), unit).Item2),
        key: key);

    public void Dispose() => _ = Release().IfFail(failure => ignore(faults.Park(point: Rail, cause: failure)));
}

[SmartEnum<int>]
internal sealed partial class PointerPulse {
    public static readonly PointerPulse Move = new(0, PointerPhase.Move, PointerEdge.Begin);
    public static readonly PointerPulse EndMove = new(1, PointerPhase.Move, PointerEdge.End);
    public static readonly PointerPulse Down = new(2, PointerPhase.Down, PointerEdge.Begin);
    public static readonly PointerPulse EndDown = new(3, PointerPhase.Down, PointerEdge.End);
    public static readonly PointerPulse Up = new(4, PointerPhase.Up, PointerEdge.Begin);
    public static readonly PointerPulse EndUp = new(5, PointerPhase.Up, PointerEdge.End);
    public static readonly PointerPulse Double = new(6, PointerPhase.DoubleClick, PointerEdge.Atomic);
    public static readonly PointerPulse Enter = new(7, PointerPhase.Enter, PointerEdge.Atomic);
    public static readonly PointerPulse Hover = new(8, PointerPhase.Hover, PointerEdge.Atomic);
    public static readonly PointerPulse Leave = new(9, PointerPhase.Leave, PointerEdge.Atomic);

    internal PointerPhase Phase { get; }
    internal PointerEdge Edge { get; }
}

internal sealed class PointerHook : MouseCallback {
    private static readonly HookId Rail = HookId.Create(value: "rasm.rhino.display.pointer");

    private readonly ChannelWriter<ViewportPointerFact> sink;
    private readonly Atom<long> rejected;
    private readonly Atom<long> vetoed = Atom(0L);
    private readonly Atom<long> ordinal = Atom(0L);
    private readonly FaultCell faults;
    private readonly LifecycleGate lifecycle;
    private readonly Option<Func<ViewportPointerFact, InputVerdict>> veto;
    private readonly Op key;
    internal PointerHook(ChannelWriter<ViewportPointerFact> sink, Atom<long> rejected, FaultCell faults, LifecycleGate lifecycle, Option<Func<ViewportPointerFact, InputVerdict>> veto, Op key) =>
        (this.sink, this.rejected, this.faults, this.lifecycle, this.veto, this.key) = (sink, rejected, faults, lifecycle, veto, key);
    internal long Submitted => ordinal.Value;
    internal long Vetoed => vetoed.Value;

    protected override void OnMouseMove(MouseCallbackEventArgs e) => Emit(PointerPulse.Move, e);
    protected override void OnEndMouseMove(MouseCallbackEventArgs e) => Emit(PointerPulse.EndMove, e);
    protected override void OnMouseDown(MouseCallbackEventArgs e) => Emit(PointerPulse.Down, e);
    protected override void OnEndMouseDown(MouseCallbackEventArgs e) => Emit(PointerPulse.EndDown, e);
    protected override void OnMouseUp(MouseCallbackEventArgs e) => Emit(PointerPulse.Up, e);
    protected override void OnEndMouseUp(MouseCallbackEventArgs e) => Emit(PointerPulse.EndUp, e);
    protected override void OnMouseDoubleClick(MouseCallbackEventArgs e) => Emit(PointerPulse.Double, e);
    protected override void OnMouseEnter(MouseCallbackEventArgs e) => Emit(PointerPulse.Enter, e);
    protected override void OnMouseHover(MouseCallbackEventArgs e) => Emit(PointerPulse.Hover, e);
    protected override void OnMouseLeave(MouseCallbackEventArgs e) => Emit(PointerPulse.Leave, e);

    private void Emit(PointerPulse pulse, MouseCallbackEventArgs e) {
        _ = lifecycle.Within(() => {
            long current = ordinal.Swap(static count => count + 1);
            ViewportPointerFact projected = new(
                pulse.Phase, pulse.Edge, e.View.ActiveViewport.Id, new Point2d(e.ViewportPoint.X, e.ViewportPoint.Y),
                PointerButton.Of(e.MouseButton),
                CapabilitySet<PointerModifier>.Of(toSeq(PointerModifier.Items)
                    .Filter(row => (row == PointerModifier.Shift && e.ShiftKeyDown) || (row == PointerModifier.Control && e.CtrlKeyDown)).ToArray()),
                e.IsOverGumball() is var mode && mode != GumballMode.None
                    ? Some(HostRow<GumballMode>.Row(mode))
                    : Option<HostRow<GumballMode>>.None,
                new PointerVerdict.Admitted(),
                current);
            ViewportPointerFact settled = pulse.Edge == PointerEdge.End
                ? projected with { Verdict = e.Cancel ? new PointerVerdict.DefaultSuppressed() : new PointerVerdict.DefaultRan() }
                : veto.Map(respond => respond(projected)).IfNone(InputVerdict.Ignored) is var verdict
                    && (verdict == InputVerdict.Handled || verdict == InputVerdict.Capture)
                    ? (e.Cancel = true, ignore(vetoed.Swap(static count => count + 1)), projected with { Verdict = new PointerVerdict.Vetoed() }).Item3
                    : projected;
            _ = Op.SideWhen(!sink.TryWrite(settled), () => ignore(rejected.Swap(static count => count + 1)));
            return Fin.Succ(unit);
        }, static () => Fin.Succ(unit), key).IfFail(failure => ignore(faults.Park(point: Rail, cause: failure)));
    }
}

```

## [03]-[GUMBALL]

- Owner: `GumballSeat` closes the host seating family; `GumballMove` closes line-ray and plane updates; `GumballHandle` is the manipulator's handle vocabulary, each row seating its own host member; `GumballLook` closes manipulator appearance over the handle set; `GumballGrip` is the rig's one custody cell — idle, dragging, or released; `GumballEvidence` carries total and incremental transforms beside the grip state without mutating geometry.
- Entry: `Gumballs.Mount` returns the mounted `GumballRig`; the rig returns pick posture and transform evidence from its own operations.
- Law: this section is WIRED from `Commands/acquisition.md` — the point getter is the ONE `Pick` producer: it lends its live `PickContext`/`GetPoint` pair to `GumballRig.Pick` for the length of the call, and the rig retains neither. A rig no getter mounts draws nothing and completes nothing.
- Law: `GumballRig.Complete` returns transform evidence after closing the drag; the caller alone decides whether its transaction applies that transform.
- Law: a move admits its shape through `GumballMove` — ray point and world line, frame plane validity — and the whole native update, evidence read included, runs inside one catch boundary, so a host throw lands as a `Fin` failure.
- Law: the handle roster is a `CapabilitySet<GumballHandle>` whose rows seat their own host member — fourteen `Contains` probes against a `FrozenSet` were the deleted form — and appearance is one admitted value the seat call takes whole: the host settings carrier is a plain settable, so a mount projects `GumballLook` into it once through the row fold and never mutates it after seating.
- Law: drag state and liveness are ONE cell — `GumballGrip` closes idle, dragging, and released as cases stepped through `Cell.Step`, so a drag write after release DECLINES typed, a second release reads `Refused` rather than no-opping, and the interlocked flag pair the rig carried — a mutable `Dragging` settable written inside `Map` and a re-arming release int — has no spelling left. `GumballRig.Pick` returns the host's `PickResult` mode as `Option<HostRow<GumballMode>>`.
- Law: release runs stop-then-dispose with every step attempted and every refusal aggregated through kernel `Custody` — conduit disable, conduit dispose, gumball dispose — and the fold's fault parks on the rig's bounded cell; a failed release does not re-arm because the one-shot cell forecloses double-dispose.
- Boundary: `PickContext` and `GetPoint` are the command rail's, BORROWED by `Pick` for the length of the call; the rig holds neither and disposes neither.
- Packages: RhinoCommon `Rhino.UI.Gumball` (`GumballObject`, `GumballDisplayConduit`, `GumballAppearanceSettings` — `.api/api-rhinocommon-display.md`); `Rasm.Domain` (`Cell`, `Transition`, `FaultCell`, `CapabilitySet`); `Rasm.Domain` (`Custody` — `Domain/rails.md`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GumballSeat {
    private GumballSeat() { }
    public sealed record Bounds(BoundingBox Value, Option<global::Rhino.Geometry.Plane> Frame) : GumballSeat;
    public sealed record Line(global::Rhino.Geometry.Line Value) : GumballSeat;
    public sealed record Plane(global::Rhino.Geometry.Plane Value) : GumballSeat;
    public sealed record Arc(global::Rhino.Geometry.Arc Value) : GumballSeat;
    public sealed record Circle(global::Rhino.Geometry.Circle Value) : GumballSeat;
    public sealed record Ellipse(global::Rhino.Geometry.Ellipse Value) : GumballSeat;
    public sealed record Curve(global::Rhino.Geometry.Curve Value) : GumballSeat;
    public sealed record Extrusion(global::Rhino.Geometry.Extrusion Value) : GumballSeat;
    public sealed record Light(global::Rhino.Geometry.Light Value) : GumballSeat;
    public sealed record Hatch(global::Rhino.Geometry.Hatch Value) : GumballSeat;

    internal bool Valid => Switch(
        bounds: static row => row.Value.IsValid
            && row.Frame.Match(Some: static frame => frame.IsValid, None: static () => true),
        line: static row => row.Value.IsValid,
        plane: static row => row.Value.IsValid,
        arc: static row => row.Value.IsValid,
        circle: static row => row.Value.IsValid,
        ellipse: static row => row.Value.IsValid,
        curve: static row => row.Value is { IsValid: true },
        extrusion: static row => row.Value is { IsValid: true },
        light: static row => row.Value is { IsValid: true },
        hatch: static row => row.Value is { IsValid: true });

    internal Fin<Unit> Apply(GumballObject target, Op key) => Switch(
        (Target: target, Op: key),
        bounds: static (ctx, row) => row.Frame.Match(
            Some: frame => ctx.Op.Confirm(ctx.Target.SetFromBoundingBox(frame, row.Value)),
            None: () => ctx.Op.Confirm(ctx.Target.SetFromBoundingBox(row.Value))),
        line: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromLine(row.Value)),
        plane: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromPlane(row.Value)),
        arc: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromArc(row.Value)),
        circle: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromCircle(row.Value)),
        ellipse: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromEllipse(row.Value)),
        curve: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromCurve(row.Value)),
        extrusion: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromExtrusion(row.Value)),
        light: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromLight(row.Value)),
        hatch: static (ctx, row) => ctx.Op.Confirm(ctx.Target.SetFromHatch(row.Value)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GumballMove {
    private GumballMove() { }
    public sealed record Ray(Point3d Point, Line WorldLine) : GumballMove;
    public sealed record Frame(Plane Value) : GumballMove;

    internal bool Valid => Switch(
        ray: static row => row.Point.IsValid && row.WorldLine.IsValid,
        frame: static row => row.Value.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GumballGrip {
    private GumballGrip() { }
    public sealed record Idle : GumballGrip;
    public sealed record Dragging : GumballGrip;
    public sealed record Released : GumballGrip;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct GumballEvidence(Transform Total, Transform Incremental, GumballGrip Grip);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GumballHandle : ICapability<GumballHandle> {
    public static readonly GumballHandle TranslateX = new(key: "translate-x", seat: static (s, held) => s.TranslateXEnabled = held);
    public static readonly GumballHandle TranslateY = new(key: "translate-y", seat: static (s, held) => s.TranslateYEnabled = held);
    public static readonly GumballHandle TranslateZ = new(key: "translate-z", seat: static (s, held) => s.TranslateZEnabled = held);
    public static readonly GumballHandle TranslateXY = new(key: "translate-xy", seat: static (s, held) => s.TranslateXYEnabled = held);
    public static readonly GumballHandle TranslateYZ = new(key: "translate-yz", seat: static (s, held) => s.TranslateYZEnabled = held);
    public static readonly GumballHandle TranslateZX = new(key: "translate-zx", seat: static (s, held) => s.TranslateZXEnabled = held);
    public static readonly GumballHandle RotateX = new(key: "rotate-x", seat: static (s, held) => s.RotateXEnabled = held);
    public static readonly GumballHandle RotateY = new(key: "rotate-y", seat: static (s, held) => s.RotateYEnabled = held);
    public static readonly GumballHandle RotateZ = new(key: "rotate-z", seat: static (s, held) => s.RotateZEnabled = held);
    public static readonly GumballHandle ScaleX = new(key: "scale-x", seat: static (s, held) => s.ScaleXEnabled = held);
    public static readonly GumballHandle ScaleY = new(key: "scale-y", seat: static (s, held) => s.ScaleYEnabled = held);
    public static readonly GumballHandle ScaleZ = new(key: "scale-z", seat: static (s, held) => s.ScaleZEnabled = held);
    public static readonly GumballHandle Relocate = new(key: "relocate", seat: static (s, held) => s.RelocateEnabled = held);
    public static readonly GumballHandle Menu = new(key: "menu", seat: static (s, held) => s.MenuEnabled = held);

    public static CapabilityLaw<GumballHandle> Law => CapabilityLaw<GumballHandle>.Open;

    [UseDelegateFromConstructor] internal partial void Seat(GumballAppearanceSettings settings, bool held);
}

public sealed record GumballLook {
    private GumballLook(CapabilitySet<GumballHandle> handles, PerceptualColor x, PerceptualColor y, PerceptualColor z, PerceptualColor menu, int radius, int axisThickness, int arcThickness) =>
        (Handles, X, Y, Z, Menu, Radius, AxisThickness, ArcThickness) =
        (handles, x, y, z, menu, radius, axisThickness, arcThickness);

    public CapabilitySet<GumballHandle> Handles { get; }
    public PerceptualColor X { get; }
    public PerceptualColor Y { get; }
    public PerceptualColor Z { get; }
    public PerceptualColor Menu { get; }
    public int Radius { get; }
    public int AxisThickness { get; }
    public int ArcThickness { get; }

    public static Fin<GumballLook> Of(
        CapabilitySet<GumballHandle> handles,
        PerceptualColor x,
        PerceptualColor y,
        PerceptualColor z,
        PerceptualColor menu,
        int radius,
        int axisThickness,
        int arcThickness,
        Op? key = null) =>
        guard(radius > 0 && axisThickness > 0 && arcThickness > 0, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new GumballLook(handles, x, y, z, menu, radius, axisThickness, arcThickness));

    internal GumballAppearanceSettings Native() {
        GumballAppearanceSettings settings = new() {
            ColorX = Quant.Sys(X),
            ColorY = Quant.Sys(Y),
            ColorZ = Quant.Sys(Z),
            ColorMenuButton = Quant.Sys(Menu),
            Radius = Radius,
            AxisThickness = AxisThickness,
            ArcThickness = ArcThickness,
        };
        _ = toSeq(GumballHandle.Items).Fold(unit, (_, row) => Op.Side(() => row.Seat(settings, Handles.Admits(row))));
        return settings;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class GumballRig : IDisposable {
    private static readonly HookId Rail = HookId.Create(value: "rasm.rhino.display.gumball");

    private readonly GumballObject gumball;
    private readonly GumballDisplayConduit conduit;
    private readonly FaultCell faults = DisplayFaults.Cell();
    private readonly Atom<GumballGrip> grip = Atom<GumballGrip>(new GumballGrip.Idle());
    internal GumballRig(GumballObject gumball, GumballDisplayConduit conduit) =>
        (this.gumball, this.conduit) = (gumball, conduit);
    public GumballEvidence Evidence => new(conduit.TotalTransform, conduit.GumballTransform, grip.Value);
    public Seq<IsolatedFault> Faults => faults.Parked;

    internal Fin<Unit> Grip(GumballGrip next, Op key) =>
        Cell.Step(grip, held => held is GumballGrip.Released ? None : Some(next), key.InvalidContext()).Switch(
            state: key,
            committed: static (_, _) => Fin.Succ(unit),
            ceded: static (_, _) => Fin.Succ(unit),
            refused: static (_, row) => Fin.Fail<Unit>(row.Cause),
            contended: static (op, _) => Fin.Fail<Unit>(op.InvalidResult()));

    internal Fin<Option<HostRow<GumballMode>>> Pick(PickContext context, GetPoint point, Op? key = null) {
        Op op = key.OrDefault();
        return guard(context is not null && point is not null, op.InvalidInput()).ToFin()
            .Bind(_ => op.Catch(() => conduit.PickGumball(context, point)))
            .Bind(seated => Grip(seated ? new GumballGrip.Dragging() : new GumballGrip.Idle(), op).Map(_ =>
                seated && conduit.PickResult is { } pick && pick.Mode != GumballMode.None
                    ? Some(HostRow<GumballMode>.Row(pick.Mode))
                    : Option<HostRow<GumballMode>>.None));
    }

    public Fin<GumballEvidence> Move(GumballMove value, Op? key = null) {
        Op op = key.OrDefault();
        return guard(value is not null && value.Valid, op.InvalidInput()).ToFin()
            .Bind(_ => op.Catch(() => value.Switch(
                (Rig: this, Op: op),
                ray: static (ctx, row) => ctx.Op.Confirm(ctx.Rig.conduit.UpdateGumball(row.Point, row.WorldLine)),
                frame: static (ctx, row) => ctx.Op.Confirm(ctx.Rig.conduit.UpdateGumball(row.Value)))))
            .Map(_ => Evidence);
    }

    public Fin<GumballEvidence> Complete(Op? key = null) {
        Op op = key.OrDefault();
        return Grip(new GumballGrip.Idle(), op).Map(_ => Evidence);
    }

    internal Fin<Unit> Release(Op key) =>
        Cell.Step(grip, held => held is GumballGrip.Released ? None : Some<GumballGrip>(new GumballGrip.Released()), key.InvalidContext()).Switch(
            state: (Rig: this, Op: key),
            committed: static (ctx, _) => Custody.Release(
                releases: Seq<Func<Fin<Unit>>>(
                    () => ctx.Op.Catch(() => { ctx.Rig.conduit.Enabled = false; return Fin.Succ(unit); }),
                    () => ctx.Op.Catch(() => { ctx.Rig.conduit.Dispose(); return Fin.Succ(unit); }),
                    () => ctx.Op.Catch(() => { ctx.Rig.gumball.Dispose(); return Fin.Succ(unit); })),
                key: ctx.Op),
            ceded: static (_, _) => Fin.Succ(unit),
            refused: static (_, row) => Fin.Fail<Unit>(row.Cause),
            contended: static (ctx, _) => Fin.Fail<Unit>(ctx.Op.InvalidResult()));

    public void Dispose() => _ = Release(Op.Of(nameof(GumballRig)))
        .IfFail(failure => ignore(faults.Park(point: Rail, cause: failure)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Gumballs {
    public static Fin<GumballRig> Mount(GumballSeat seat, ActiveSpaceUse space, GumballLook look, Op? key = null) {
        Op op = key.OrDefault();
        return guard(seat is not null && seat.Valid && space is not null && look is not null, op.InvalidInput()).ToFin()
            .Bind(_ => op.Catch(() => Fin.Succ(new GumballObject())).Bind(ball =>
            op.Catch(() => Fin.Succ(new GumballDisplayConduit(space.Key))).BiBind(
                Succ: pipe => {
                    GumballRig rig = new(ball, pipe);
                    return (from _ in op.Catch(() => seat.Apply(ball, op))
                            from __ in op.Catch(() => {
                                pipe.SetBaseGumball(ball, look.Native());
                                pipe.Enabled = true;
                                return unit;
                            })
                            select rig).BiBind(
                                Succ: static mounted => Fin.Succ(mounted),
                                Fail: failure => rig.Release(op).Match(
                                    Succ: _ => Fin.Fail<GumballRig>(failure),
                                    Fail: cleanup => Fin.Fail<GumballRig>(failure + cleanup)));
                },
                Fail: failure => op.Catch(() => { ball.Dispose(); return Fin.Succ(unit); }).Match(
                    Succ: _ => Fin.Fail<GumballRig>(failure),
                    Fail: cleanup => Fin.Fail<GumballRig>(failure + cleanup)))));
    }
}
```

## [04]-[WIDGETS]

- Owner: `WidgetSpec` closes grip, direction, rotation, text-dot, SVG, and slider payloads at the host's FULL axis set; `WidgetScope` chooses all-document or document-group registration; `WidgetVisibility` is the posture vocabulary every mount, change, and state reads; `WidgetProbe`/`WidgetAnswer` close the pull reads the host publishes per widget kind.
- Entry: `WidgetHost` returns `WidgetId`, `WidgetState`, `WidgetAnswer`, or `Unit` directly from the operation that owns each value.
- Law: every host axis the widget surface publishes LANDS — the grip's glyph, rotation, stroke width, ink and fill colours, and its three snap-and-cursor switches; the direction widget's arrow glyph; the text dot's hover height; the SVG control's second text channel, alignment pair, world tracking point, and computed hit rectangle; the slider's range-overrun pair and formatted render — a missing axis is a defect, not thrift. Every optional axis rides `Option` and an absent value WRITES NOTHING, because the host publishes its own defaults and a restated default is a forged value.
- Law: pull reads are a PROBE union, not member families — per-viewport arrow and arc visibility, the computed screen rectangle in its named pixel space, and the slider's precision-formatted value each answer through one `Ask` request whose probe case fixes the answer shape, and a probe against a widget kind that does not publish it refuses typed naming the axis.
- Law: adapters project every host callback through one `WidgetSink`; painting draws the native widget visual through `base.OnDraw`, then replays the mounted mark program through the draw page's ONE `Marks.Paint` entry under the `WidgetOverlay` phase the seam occupies — the per-widget sprite cache is gone with the paint absorption, and the sink never invents a renderer.
- Law: the mark program is a swappable slot on the sink, so `Change` retargets an overlay in place beside the host writes — every widget state moves through one request and none demands retire-and-remount, the move that drops the `WidgetId` a consumer keys on; an absent mark program leaves the mounted one standing. The swap is a total replace, so it rides a plain `Swap` by the transition owner's own carve.
- Law: visibility, active-view binding, and registration are ONE `CapabilitySet<WidgetVisibility>` — `Registered` is MEASURED, so the request law bars it from a mount or change posture and only the state sweep may hold it.
- Law: document-local registration uses `ViewUserInterface.Add` and removal uses the same table; all-document registration pairs `RegisterForAllDocuments` with `Unregister`. The two paths are exclusive per widget — a widget registered both ways draws twice and retires once.
- Law: `LifecycleGate` admits operation and callback claims, closes admission before bounded retirement, and retains failed mounts for the next cleanup attempt; sink faults park on the host's bounded `FaultCell` and release fans through kernel `Custody`.
- Law: change captures native state before either write and compensates visibility and active-view binding when either write fails, the compensation itself running all-attempted with faults aggregated.
- Boundary: a mounted widget is visible only through `WidgetId`, `WidgetFact`, `WidgetState`, and `WidgetAnswer` values.
- Packages: RhinoCommon `Rhino.UI` widget estate (`.api/api-rhinocommon-custom-objects.md` `[GRIP_WIDGETS]`/`[CONTROL_WIDGETS]`/`[WIDGET_BASE_AND_REGISTRATION]`); `Rasm.Domain` (`CapabilitySet`, `CapabilityLaw`, `FaultCell`); `Rasm.Rhino.Document` (`LifecycleGate`), `Rasm.Domain` (`Custody`); `Display/draw.md` (`Marks.Paint`, `Canvas.Pipeline`, `Mark`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<Guid>]
public readonly partial struct WidgetId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Widget identity is empty.") : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetConstraint {
    private WidgetConstraint() { }
    public sealed record Free : WidgetConstraint;
    public sealed record Curve(global::Rhino.Geometry.Curve Value) : WidgetConstraint;
    public sealed record Line(global::Rhino.Geometry.Line Value) : WidgetConstraint;
    public sealed record Arc(global::Rhino.Geometry.Arc Value) : WidgetConstraint;
    public sealed record Circle(global::Rhino.Geometry.Circle Value) : WidgetConstraint;

    internal bool Valid => Switch(
        free: static _ => true,
        curve: static row => row.Value is { IsValid: true },
        line: static row => row.Value.IsValid,
        arc: static row => row.Value.IsValid,
        circle: static row => row.Value.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetScope {
    private WidgetScope() { }
    public sealed record AllDocuments : WidgetScope;
    public sealed record Document(DocumentSession Session, Guid Group) : WidgetScope;

    internal bool Valid => Switch(
        allDocuments: static _ => true,
        document: static row => row.Session is not null && row.Group != Guid.Empty);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WidgetVisibility : ICapability<WidgetVisibility> {
    public static readonly WidgetVisibility Shown = new(key: "shown");
    public static readonly WidgetVisibility ActiveViewBound = new(key: "active-view");
    public static readonly WidgetVisibility Registered = new(key: "registered");

    public static CapabilityLaw<WidgetVisibility> RequestLaw => requestLaw.Value;
    private static readonly Lazy<CapabilityLaw<WidgetVisibility>> requestLaw =
        new(static () => CapabilityLaw<WidgetVisibility>.Forbidden(Seq(CapabilitySet<WidgetVisibility>.Of(Registered))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GripSnap : ICapability<GripSnap> {
    public static readonly GripSnap Permitted = new(key: "permitted");
    public static readonly GripSnap SnapCursors = new(key: "snap-cursors");
    public static readonly GripSnap ObjectCursors = new(key: "object-cursors");

    public static CapabilityLaw<GripSnap> Law => CapabilityLaw<GripSnap>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SliderTrait : ICapability<SliderTrait> {
    public static readonly SliderTrait DisplayValue = new(key: "display-value");
    public static readonly SliderTrait OverrunBefore = new(key: "overrun-before");
    public static readonly SliderTrait OverrunAfter = new(key: "overrun-after");

    public static CapabilityLaw<SliderTrait> Law => CapabilityLaw<SliderTrait>.Open;
}

[SmartEnum<bool>]
public sealed partial class PixelSpace {
    public static readonly PixelSpace Device = new(key: false);
    public static readonly PixelSpace Logical = new(key: true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetSpec {
    private WidgetSpec() { }
    public sealed record Grip(Point3d At, double Radius, WidgetConstraint Constraint, Seq<Point3d> Snaps, CapabilitySet<GripSnap> Snap, Option<HostRow<GripUserInterfaceObjectShape>> Shape, Option<VectorAngle> Rotation, Option<double> Stroke, Option<PerceptualColor> Ink, Option<PerceptualColor> Fill) : WidgetSpec;
    public sealed record Direction(Point3d At, Vector3d Vector, double Radius, Option<double> LineLength, bool OneWay, bool GripPointVisible, Option<HostRow<GripUserInterfaceObjectShape>> ArrowShape) : WidgetSpec;
    public sealed record Rotation(Plane Plane, double Radius, bool GripPointVisible) : WidgetSpec;
    public sealed record Text(string Value, Point3d At, int Height, Option<int> HoverHeight, PerceptualColor Ink, PerceptualColor Fill, PerceptualColor Border) : WidgetSpec;
    public sealed record Svg(string Value, Offset2i At, Size2i Extent, Option<string> Text, Option<HostRow<ControlHorizontalAlignment>> AlignAcross, Option<HostRow<ControlVerticalAlignment>> AlignDown, Option<Point3d> Tracking) : WidgetSpec;
    public sealed record Slider(Interval Range, double Value, bool Horizontal, CapabilitySet<SliderTrait> Traits, int Precision) : WidgetSpec;

    internal bool Valid => Switch(
        grip: static row => row.At.IsValid
            && row.Constraint is not null
            && row.Constraint.Valid
            && row.Snaps.ForAll(static point => point.IsValid)
            && row.Radius > 0.0
            && double.IsFinite(row.Radius)
            && row.Stroke.Match(Some: static value => value > 0.0 && double.IsFinite(value), None: static () => true),
        direction: static row => row.At.IsValid
            && row.Vector.IsValid
            && !row.Vector.IsZero
            && row.Radius > 0.0
            && double.IsFinite(row.Radius)
            && row.LineLength.Match(Some: static value => value > 0.0 && double.IsFinite(value), None: static () => true),
        rotation: static row => row.Plane.IsValid && row.Radius > 0.0 && double.IsFinite(row.Radius),
        text: static row => !string.IsNullOrWhiteSpace(row.Value) && row.At.IsValid && row.Height > 0
            && row.HoverHeight.Match(Some: static value => value > 0, None: static () => true),
        svg: static row => !string.IsNullOrWhiteSpace(row.Value) && row.Extent.IsValid,
        slider: static row => double.IsFinite(row.Range.T0)
            && double.IsFinite(row.Range.T1)
            && row.Range.T0 < row.Range.T1
            && row.Value >= row.Range.T0
            && row.Value <= row.Range.T1
            && row.Precision >= 0);
}

[SmartEnum<int>]
public sealed partial class ClickUse {
    public static readonly ClickUse Single = new(key: 0);
    public static readonly ClickUse Double = new(key: 1);
}

[SmartEnum<int>]
internal sealed partial class WidgetPulse {
    public static readonly WidgetPulse Press = new(
        key: 0,
        emit: static (sink, mouse) => sink.Emit(() => new WidgetFact.Pressed(sink.Identity, mouse.View.ActiveViewport.Id, PointerButton.Of(mouse.Button), mouse.FrustumLine)));
    public static readonly WidgetPulse Release = new(
        key: 1,
        emit: static (sink, mouse) => sink.Emit(() => new WidgetFact.Released(sink.Identity, mouse.View.ActiveViewport.Id, PointerButton.Of(mouse.Button), mouse.FrustumLine)));
    public static readonly WidgetPulse Enter = new(key: 2, emit: static (sink, _) => sink.Emit(() => new WidgetFact.Hovered(sink.Identity, true)));
    public static readonly WidgetPulse Leave = new(key: 3, emit: static (sink, _) => sink.Emit(() => new WidgetFact.Hovered(sink.Identity, false)));
    public static readonly WidgetPulse Click = new(key: 4, emit: static (sink, _) => sink.Emit(() => new WidgetFact.Clicked(sink.Identity, ClickUse.Single)));
    public static readonly WidgetPulse DoubleClick = new(key: 5, emit: static (sink, _) => sink.Emit(() => new WidgetFact.Clicked(sink.Identity, ClickUse.Double)));

    [UseDelegateFromConstructor]
    internal partial Unit Emit(WidgetSink sink, MouseState mouse);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetHit {
    private WidgetHit() { }
    public sealed record Curve(Option<double> Parameter) : WidgetHit;
    public sealed record Line(bool Over) : WidgetHit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetFact {
    private WidgetFact() { }
    public sealed record Pressed(WidgetId Widget, Guid Viewport, PointerButton Button, Line Ray) : WidgetFact;
    public sealed record Released(WidgetId Widget, Guid Viewport, PointerButton Button, Line Ray) : WidgetFact;
    public sealed record Clicked(WidgetId Widget, ClickUse Use) : WidgetFact;
    public sealed record Hovered(WidgetId Widget, bool Value) : WidgetFact;
    public sealed record Moved(WidgetId Widget, Point3d To) : WidgetFact;
    public sealed record Rotated(WidgetId Widget, double Angle) : WidgetFact;
    public sealed record Slid(WidgetId Widget, double Value) : WidgetFact;
    public sealed record Hit(WidgetId Widget, WidgetHit Result) : WidgetFact;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetProbe {
    private WidgetProbe() { }
    public sealed record Glyphs(DocumentSession Session, Guid Viewport) : WidgetProbe;
    public sealed record Region(DocumentSession Session, PixelSpace Space) : WidgetProbe;
    public sealed record Formatted : WidgetProbe;

    internal bool Valid => Switch(
        glyphs: static row => row.Session is not null && row.Viewport != Guid.Empty,
        region: static row => row.Session is not null && row.Space is not null,
        formatted: static _ => true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetAnswer : IDetachedDocumentResult {
    private WidgetAnswer() { }
    public sealed record Glyphs(bool Visible) : WidgetAnswer;
    public sealed record Region(Point2d Origin, Vector2d Extent) : WidgetAnswer;
    public sealed record Formatted(string Rendered, double Value) : WidgetAnswer;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WidgetState(WidgetId Widget, CapabilitySet<WidgetVisibility> Posture) : IDetachedDocumentResult;
internal sealed record WidgetMount(UserInterfaceObjectBase Native, WidgetSink Sink, Func<Fin<Unit>> Retire);
internal sealed record WidgetSink(
    WidgetId Identity,
    ChannelWriter<WidgetFact> Writer,
    Atom<Seq<Mark>> Program,
    FaultCell Faults,
    Atom<long> Submitted,
    Atom<long> Rejected,
    LifecycleGate Lifecycle,
    Op Key) {
    private static readonly HookId Rail = HookId.Create(value: "rasm.rhino.display.widget");

    internal Unit Paint(DrawEventArgs args) => Observe(Lifecycle.Within(
        body: () => Marks.Paint(
            new Canvas.Pipeline(ConduitFrame.Of(args.Display, args.Viewport, ConduitPhase.WidgetOverlay)),
            Program.Value,
            Key).Map(static _ => unit),
        refused: static () => Fin.Succ(unit),
        key: Key));

    internal Unit Retarget(Seq<Mark> marks) => ignore(Program.Swap(_ => marks));

    internal Unit Pulse(WidgetPulse pulse, MouseState mouse) => pulse.Emit(this, mouse);
    internal Unit Move(Point3d value) => Emit(() => new WidgetFact.Moved(Identity, value));
    internal Unit Rotate(double value) => Emit(() => new WidgetFact.Rotated(Identity, value));
    internal Unit Slide(double value) => Emit(() => new WidgetFact.Slid(Identity, value));
    internal Unit Hit(Func<WidgetHit> project) => Emit(() => new WidgetFact.Hit(Identity, project()));

    internal Unit Emit(Func<WidgetFact> project) => Observe(Lifecycle.Within(
        body: () => {
            _ = Submitted.Swap(static count => count + 1);
            _ = Op.SideWhen(!Writer.TryWrite(project()), () => ignore(Rejected.Swap(static count => count + 1)));
            return Fin.Succ(unit);
        },
        refused: static () => Fin.Succ(unit),
        key: Key));

    private Unit Observe(Fin<Unit> outcome) => outcome.Match(
        Succ: static _ => unit,
        Fail: failure => ignore(Faults.Park(point: Rail, cause: failure)));
}

// --- [ADAPTERS] ------------------------------------------------------------------------
internal sealed class GripWidget : GripUserInterfaceObject {
    private readonly WidgetSink sink;
    private readonly WidgetConstraint constraint;
    internal GripWidget(WidgetSpec.Grip spec, WidgetSink sink) : base(spec.At) {
        this.sink = sink;
        GripRadius = (float)spec.Radius;
        ObjectSnapPermitted = spec.Snap.Admits(GripSnap.Permitted);
        ObjectSnapCursorsEnabled = spec.Snap.Admits(GripSnap.SnapCursors);
        OnObjectCursorsEnabled = spec.Snap.Admits(GripSnap.ObjectCursors);
        _ = spec.Shape.Iter(row => GripShape = row.Native);
        _ = spec.Rotation.Iter(angle => GripShapeRotationRadians = (float)angle.Value);
        _ = spec.Stroke.Iter(width => GripStrokeWidth = (float)width);
        _ = spec.Ink.Iter(color => GripColor = Quant.Sys(color));
        _ = spec.Fill.Iter(color => GripFillColor = Quant.Sys(color));
        constraint = spec.Constraint;
        _ = spec.Constraint.Switch(
            this,
            free: static (_, _) => unit,
            curve: static (owner, value) => Op.Side(() => owner.Constrain(value.Value)),
            line: static (owner, value) => Op.Side(() => owner.Constrain(value.Value)),
            arc: static (owner, value) => Op.Side(() => owner.Constrain(value.Value)),
            circle: static (owner, value) => Op.Side(() => owner.Constrain(value.Value)));
        _ = Op.SideWhen(!spec.Snaps.IsEmpty, () => SetSnapPoints(spec.Snaps.AsEnumerable()));
    }
    protected override void OnDraw(DrawEventArgs e) { base.OnDraw(e); sink.Paint(e); }
    protected override void OnMouseDown(MouseState e) => sink.Pulse(WidgetPulse.Press, e);
    protected override void OnMouseUp(MouseState e) => sink.Pulse(WidgetPulse.Release, e);
    protected override void OnDrag(Point3d value, MouseState _) => sink.Move(value);
    protected override void OnMouseMove(MouseState e) => _ = constraint.Switch(
        (Sink: sink, Mouse: e),
        free: static (_, _) => unit,
        curve: static (ctx, value) => ctx.Sink.Hit(() => new WidgetHit.Curve(ctx.Mouse.IsMouseOver(value.Value, out double t) ? Some(t) : None)),
        line: static (ctx, value) => ctx.Sink.Hit(() => new WidgetHit.Line(ctx.Mouse.IsMouseOver(value.Value))),
        arc: static (ctx, value) => ctx.Sink.Hit(() => { using Curve curve = value.Value.ToNurbsCurve(); return new WidgetHit.Curve(ctx.Mouse.IsMouseOver(curve, out double t) ? Some(t) : None); }),
        circle: static (ctx, value) => ctx.Sink.Hit(() => { using Curve curve = value.Value.ToNurbsCurve(); return new WidgetHit.Curve(ctx.Mouse.IsMouseOver(curve, out double t) ? Some(t) : None); }));
    protected override void OnMouseEnter(MouseState e) => sink.Pulse(WidgetPulse.Enter, e);
    protected override void OnMouseLeave(MouseState e) => sink.Pulse(WidgetPulse.Leave, e);
    protected override void OnMouseClick(MouseState e) => sink.Pulse(WidgetPulse.Click, e);
    protected override void OnMouseDoubleClick(MouseState e) => sink.Pulse(WidgetPulse.DoubleClick, e);
}

internal sealed class DirectionWidget : DirectionGripUserInterfaceObject {
    private readonly WidgetSink sink;
    internal DirectionWidget(WidgetSpec.Direction spec, WidgetSink sink) : base(spec.At, spec.Vector) {
        this.sink = sink;
        ArrowRadius = (float)spec.Radius;
        OneWay = spec.OneWay;
        GripPointVisible = spec.GripPointVisible;
        _ = spec.LineLength.Iter(value => DirectionLineLength = (float)value);
        _ = spec.ArrowShape.Iter(row => ArrowShape = row.Native);
    }
    protected override void OnDraw(DrawEventArgs e) { base.OnDraw(e); sink.Paint(e); }
    protected override void OnMouseDown(MouseState e) => sink.Pulse(WidgetPulse.Press, e);
    protected override void OnMouseUp(MouseState e) => sink.Pulse(WidgetPulse.Release, e);
    protected override void OnMouseEnter(MouseState e) => sink.Pulse(WidgetPulse.Enter, e);
    protected override void OnMouseLeave(MouseState e) => sink.Pulse(WidgetPulse.Leave, e);
}

internal sealed class RotationWidget : RotationGripUserInterfaceObject {
    private readonly WidgetSink sink;
    internal RotationWidget(WidgetSpec.Rotation spec, WidgetSink sink) : base(spec.Plane, spec.Radius) {
        this.sink = sink;
        GripPointVisible = spec.GripPointVisible;
    }
    protected override void OnDraw(DrawEventArgs e) { base.OnDraw(e); sink.Paint(e); }
    protected override void OnMouseDown(MouseState e) => sink.Pulse(WidgetPulse.Press, e);
    protected override void OnMouseUp(MouseState e) => sink.Pulse(WidgetPulse.Release, e);
    protected override void OnRotationDrag(double value, MouseState _) => sink.Rotate(value);
}

internal sealed class TextWidget : TextDotUserInterfaceObject {
    private readonly WidgetSink sink;
    internal TextWidget(WidgetSpec.Text spec, WidgetSink sink) : base(spec.At, spec.Value) {
        this.sink = sink;
        TextHeight = spec.Height;
        _ = spec.HoverHeight.Iter(value => MouseOverTextHeight = value);
        TextColor = Quant.Sys(spec.Ink);
        DotBackgroundColor = Quant.Sys(spec.Fill);
        DotBorderColor = Quant.Sys(spec.Border);
    }
    protected override void OnDraw(DrawEventArgs e) { base.OnDraw(e); sink.Paint(e); }
    protected override void OnMouseDown(MouseState e) => sink.Pulse(WidgetPulse.Press, e);
    protected override void OnMouseUp(MouseState e) => sink.Pulse(WidgetPulse.Release, e);
    protected override void OnMouseEnter(MouseState e) => sink.Pulse(WidgetPulse.Enter, e);
    protected override void OnMouseLeave(MouseState e) => sink.Pulse(WidgetPulse.Leave, e);
    protected override void OnMouseClick(MouseState e) => sink.Pulse(WidgetPulse.Click, e);
    protected override void OnMouseDoubleClick(MouseState e) => sink.Pulse(WidgetPulse.DoubleClick, e);
}

internal sealed class SvgWidget : UserInterfaceControl {
    private readonly WidgetSink sink;
    internal SvgWidget(WidgetSpec.Svg spec, WidgetSink sink) : base(new System.Drawing.Point(spec.At.X, spec.At.Y), spec.Extent.Native) {
        this.sink = sink;
        SetSvg(spec.Value);
        _ = spec.Text.Iter(value => Text = value);
        _ = spec.AlignAcross.Iter(row => HorizontalAlignment = row.Native);
        _ = spec.AlignDown.Iter(row => VerticalAlignment = row.Native);
        TrackingPoint = Op.ToHostNullable(spec.Tracking);
    }
    protected override void OnDraw(DrawEventArgs e) { base.OnDraw(e); sink.Paint(e); }
    protected override void OnMouseDown(MouseState e) => sink.Pulse(WidgetPulse.Press, e);
    protected override void OnMouseUp(MouseState e) => sink.Pulse(WidgetPulse.Release, e);
    protected override void OnMouseClick(MouseState e) => sink.Pulse(WidgetPulse.Click, e);
    protected override void OnMouseDoubleClick(MouseState e) => sink.Pulse(WidgetPulse.DoubleClick, e);
}

internal sealed class SliderWidget : UserInterfaceSlider {
    private readonly WidgetSink sink;
    internal SliderWidget(WidgetSpec.Slider spec, WidgetSink sink) {
        this.sink = sink;
        Range = spec.Range;
        Value = spec.Value;
        HorizontalOrientation = spec.Horizontal;
        DisplayValue = spec.Traits.Admits(SliderTrait.DisplayValue);
        AllowValueBeforeRangeStart = spec.Traits.Admits(SliderTrait.OverrunBefore);
        AllowValueAfterRangeEnd = spec.Traits.Admits(SliderTrait.OverrunAfter);
        DigitPrecision = spec.Precision;
    }
    protected override void OnDraw(DrawEventArgs e) { base.OnDraw(e); sink.Paint(e); }
    protected override void OnValueChanged() => sink.Slide(Value);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class WidgetHost : IDisposable {
    private static readonly HookId Rail = HookId.Create(value: "rasm.rhino.display.widget");

    private readonly Channel<WidgetFact> channel;
    private readonly Atom<HashMap<WidgetId, WidgetMount>> mounted = Atom(HashMap<WidgetId, WidgetMount>());
    private readonly FaultCell faults = DisplayFaults.Cell();
    private readonly Atom<long> submitted = Atom(0L);
    private readonly Atom<long> rejected;
    private readonly LifecycleGate lifecycle;
    private WidgetHost(Channel<WidgetFact> channel, Atom<long> rejected, LifecycleGate lifecycle) =>
        (this.channel, this.rejected, this.lifecycle) = (channel, rejected, lifecycle);
    public ChannelReader<WidgetFact> Facts => channel.Reader;
    public Seq<IsolatedFault> Faults => faults.Parked;
    public long Shed => faults.Shed;
    public long Submitted => submitted.Value;
    public long Rejected => rejected.Value;
    public Option<int> Buffered => channel.Reader.CanCount ? Some(channel.Reader.Count) : None;

    public static Fin<WidgetHost> Of(ChannelPlan plan, Op? key = null) {
        Op op = key.OrDefault();
        return guard(plan is not null && plan.Overflow is not null, op.InvalidInput()).ToFin()
            .Bind(_ => LifecycleGate.Of(plan.SettleWithin, op).Bind(lifecycle => op.Catch(() => {
                Atom<long> rejected = Atom(0L);
                Channel<WidgetFact> channel = plan.Overflow.Bounded<WidgetFact>(plan.Capacity, rejected);
                return Fin.Succ(new WidgetHost(channel, rejected, lifecycle));
            })));
    }

    public Fin<WidgetId> Mount(WidgetSpec spec, WidgetScope scope, CapabilitySet<WidgetVisibility> posture, Seq<Mark> marks, Op? key = null) {
        Op op = key.OrDefault();
        return lifecycle.Within(
            body: () => guard(
                spec is not null
                    && spec.Valid
                    && scope is not null
                    && scope.Valid
                    && WidgetVisibility.RequestLaw.Admit(held: posture).IsSucc
                    && marks.ForAll(static mark => mark is not null && mark.Valid),
                op.InvalidInput()).ToFin().Bind(_ => {
                    WidgetId identity = WidgetId.Create(Guid.NewGuid());
                    WidgetSink sink = new(identity, channel.Writer, Atom(marks), faults, submitted, rejected, lifecycle, op);
                    return from widget in op.Catch(() => spec.Switch(
                               sink,
                               grip: static (state, row) => (UserInterfaceObjectBase)new GripWidget(row, state),
                               direction: static (state, row) => new DirectionWidget(row, state),
                               rotation: static (state, row) => new RotationWidget(row, state),
                               text: static (state, row) => new TextWidget(row, state),
                               svg: static (state, row) => new SvgWidget(row, state),
                               slider: static (state, row) => new SliderWidget(row, state)))
                           from retire in scope.Switch(
                               (Widget: widget, Op: op),
                               allDocuments: static (ctx, _) => ctx.Op.Confirm(ctx.Widget.RegisterForAllDocuments())
                                   .Map(_ => (Func<Fin<Unit>>)(() => ctx.Op.Catch(ctx.Widget.Unregister))),
                               document: static (ctx, row) => row.Session.Demand(
                                   use: doc => ctx.Op.Confirm(doc.ViewUserInterface.Add(ctx.Widget, row.Group)).Map(static _ => unit),
                                   key: ctx.Op,
                                   needs: [SessionNeed.Mutate])
                                   .Map(_ => (Func<Fin<Unit>>)(() => row.Session.Demand(
                                       use: doc => ctx.Op.Catch(() => ctx.Op.Confirm(doc.ViewUserInterface.Remove(ctx.Widget) > 0)),
                                       key: ctx.Op,
                                       needs: [SessionNeed.Mutate]))))
                           let value = new WidgetMount(widget, sink, retire)
                           from mountedId in SetPosture(value, posture, op)
                               .Bind(_ => op.Catch(() => {
                                   _ = mounted.Swap(items => items.Add(identity, value));
                                   return Fin.Succ(identity);
                               })).BiBind(
                               Succ: static value => Fin.Succ(value),
                               Fail: error => retire().Match(
                                   Succ: _ => Fin.Fail<WidgetId>(error),
                                   Fail: cleanup => Fin.Fail<WidgetId>(error + cleanup)))
                           select mountedId;
                }),
            refused: () => Fin.Fail<WidgetId>(op.InvalidContext()),
            key: op);
    }

    public Fin<WidgetState> Change(WidgetId identity, CapabilitySet<WidgetVisibility> posture, Option<Seq<Mark>> marks = default, Op? key = null) {
        Op op = key.OrDefault();
        return lifecycle.Within(
            body: () => guard(
                identity.Value != Guid.Empty
                    && WidgetVisibility.RequestLaw.Admit(held: posture).IsSucc
                    && marks.Match(
                        Some: static rows => rows.ForAll(static mark => mark is not null && mark.Valid),
                        None: static () => true),
                op.InvalidInput()).ToFin().Bind(_ => Find(identity, op).Bind(value =>
                    op.Catch(() => Fin.Succ(State(identity, value))).Bind(prior =>
                        SetPosture(value, posture, op)
                            .Map(_ => (ignore(marks.Iter(value.Sink.Retarget)), State(identity, value)).Item2)
                            .BindFail(primary => SetPosture(value, prior.Posture, op).Match(
                                Succ: _ => Fin.Fail<WidgetState>(primary),
                                Fail: cleanup => Fin.Fail<WidgetState>(primary + cleanup)))))),
            refused: () => Fin.Fail<WidgetState>(op.InvalidContext()),
            key: op);
    }

    public Fin<WidgetAnswer> Ask(WidgetId identity, WidgetProbe probe, Op? key = null) {
        Op op = key.OrDefault();
        return lifecycle.Within(
            body: () => guard(identity.Value != Guid.Empty && probe is not null && probe.Valid, op.InvalidInput()).ToFin()
                .Bind(_ => Find(identity, op))
                .Bind(value => probe.Switch(
                    (Native: value.Native, Op: op),
                    glyphs: static (ctx, row) => row.Session.Demand(
                        use: doc => ctx.Op.Catch(() =>
                            from view in Optional(doc.Views.Find(row.Viewport)).ToFin(ctx.Op.InvalidInput())
                            from visible in ctx.Native switch {
                                DirectionWidget arrow => Fin.Succ(arrow.ArrowsVisibleInViewport(view.ActiveViewport)),
                                RotationWidget arc => Fin.Succ(arc.ArcVisibleInViewport(view.ActiveViewport)),
                                _ => Fin.Fail<bool>(ctx.Op.InvalidInput(axis: nameof(WidgetProbe.Glyphs))),
                            }
                            select (WidgetAnswer)new WidgetAnswer.Glyphs(visible)),
                        key: ctx.Op,
                        needs: [SessionNeed.Read]),
                    region: static (ctx, row) => ctx.Native is SvgWidget control
                        ? row.Session.Demand(
                            use: doc => ctx.Op.Catch(() => {
                                System.Drawing.RectangleF frame = control.ComputedRectangle(doc.Views.ActiveView, row.Space.Key);
                                return Fin.Succ<WidgetAnswer>(new WidgetAnswer.Region(
                                    Origin: new Point2d(frame.X, frame.Y), Extent: new Vector2d(frame.Width, frame.Height)));
                            }),
                            key: ctx.Op,
                            needs: [SessionNeed.Read])
                        : Fin.Fail<WidgetAnswer>(ctx.Op.InvalidInput(axis: nameof(WidgetProbe.Region))),
                    formatted: static (ctx, _) => ctx.Native is SliderWidget slider
                        ? ctx.Op.Catch(() => Fin.Succ<WidgetAnswer>(new WidgetAnswer.Formatted(
                            Rendered: slider.ValueAsFormattedString(), Value: slider.Value)))
                        : Fin.Fail<WidgetAnswer>(ctx.Op.InvalidInput(axis: nameof(WidgetProbe.Formatted))))),
            refused: () => Fin.Fail<WidgetAnswer>(op.InvalidContext()),
            key: op);
    }

    public Fin<WidgetState> Inspect(WidgetId identity, Op? key = null) {
        Op op = key.OrDefault();
        return lifecycle.Within(
            body: () => guard(identity.Value != Guid.Empty, op.InvalidInput()).ToFin()
                .Bind(_ => Find(identity, op))
                .Map(value => State(identity, value)),
            refused: () => Fin.Fail<WidgetState>(op.InvalidContext()),
            key: op);
    }

    public Fin<Unit> Retire(WidgetId identity, Op? key = null) {
        Op op = key.OrDefault();
        return lifecycle.Within(
            body: () => guard(identity.Value != Guid.Empty, op.InvalidInput()).ToFin()
                .Bind(_ => Find(identity, op))
                .Bind(value => Retire(identity, value, op)),
            refused: () => Fin.Fail<Unit>(op.InvalidContext()),
            key: op);
    }

    private Fin<WidgetMount> Find(WidgetId identity, Op op) => mounted.Value.Find(identity).ToFin(op.InvalidInput());
    private Fin<Unit> Retire(WidgetId identity, WidgetMount value, Op op) =>
        from _ in value.Retire()
        from __ in op.Catch(() => Fin.Succ((mounted.Swap(items => items.Remove(identity)), unit).Item2))
        select unit;

    private static WidgetState State(WidgetId identity, WidgetMount value) => new(
        identity,
        CapabilitySet<WidgetVisibility>.Of(toSeq(WidgetVisibility.Items).Filter(row =>
            (row == WidgetVisibility.Shown && value.Native.Visible)
            || (row == WidgetVisibility.ActiveViewBound && value.Native.BoundToActiveView)
            || (row == WidgetVisibility.Registered && value.Native.IsRegistered())).ToArray()));

    private static Fin<Unit> SetPosture(WidgetMount value, CapabilitySet<WidgetVisibility> posture, Op op) =>
        op.Catch(() => {
            value.Native.Visible = posture.Admits(WidgetVisibility.Shown);
            value.Native.BoundToActiveView = posture.Admits(WidgetVisibility.ActiveViewBound);
            return Fin.Succ(unit);
        });

    private Fin<Unit> ReleaseAll(Op op) => Custody.Release(
        releases: toSeq(mounted.Value)
            .Map(row => (Func<Fin<Unit>>)(() => Retire(identity: row.Key, value: row.Value, op: op)))
            .Add(() => op.Catch(() => Fin.Succ((channel.Writer.TryComplete(), unit).Item2))),
        key: op);

    public void Dispose() => _ = lifecycle.Close(
        stop: static () => Fin.Succ(unit),
        settle: () => ReleaseAll(Op.Of(nameof(WidgetHost))),
        key: Op.Of(nameof(WidgetHost)))
        .IfFail(error => ignore(faults.Park(point: Rail, cause: error)));
}
```

## [05]-[HOOKS]

- Owner: `DisplayHooks.Mount` seats the two display hook points — `rasm.rhino.display.pointer` granting a `PointerLease` and `rasm.rhino.display.widget` granting a `WidgetHost` — as TYPED kernel bindings, each bind minting a fresh owner so no two consumers contend for one bounded channel.
- Law: a binding is `HookBinding<RhinoPoint, PluginKey, TAsk, TGrant>` — the ask and grant types are the binding's own parameters, so the `Type`-pair-plus-`object`-cast erasure the registry mount carried has no spelling left and a mismatched ask is a compile fact. `HookMounts.MountAll` seats both rows and releases the first when the second refuses.
- Law: each point carries the ask its seam can honour — the pointer binding takes its `ChannelPlan` beside the veto responder, while the widget binding takes `ChannelPlan` alone.
- Law: display modality splits by seam, not by page — widget `MouseState` callbacks run post-hoc and observe, while the pointer seam vetoes through the kernel's `InputVerdict`. The two draw-suppression seams (`CullObjectEventArgs.CullObject`, `DrawObjectEventArgs.DrawObject`) stay the conduit owner's.
- Law: `GumballRig` returns transform evidence directly from move and completion, while gumball occupancy rides every `ViewportPointerFact`.
- Packages: `Rasm.Domain` (`HookBinding`, `HookMounts`, `IHookBinding` — `Domain/hooks.md`); `Rasm.Rhino.Document` (`RhinoPoint` roster).

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisplayHooks {
    public static Fin<Seq<Lease<IDisposable>>> Mount(HookMounts<RhinoPoint, PluginKey> mounts, PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        return mounts.MountAll(
            bindings: Seq<IHookBinding<RhinoPoint, PluginKey>>(
                new HookBinding<RhinoPoint, PluginKey, (ChannelPlan Plan, Option<Func<ViewportPointerFact, InputVerdict>> Veto), PointerLease>(
                    Point: RhinoPoint.DisplayPointer,
                    Owner: plugin,
                    Bind: static ask => {
                        Op bind = Op.Of(name: nameof(DisplayHooks));
                        return guard(
                            ask.Plan is not null
                                && ask.Plan.Overflow is not null
                                && ask.Veto.Match(Some: static value => value is not null, None: static () => true),
                            bind.InvalidInput()).ToFin().Bind(_ =>
                                from lifecycle in LifecycleGate.Of(ask.Plan.SettleWithin, bind)
                                from lease in bind.Catch(() => {
                                    Atom<long> rejected = Atom(0L);
                                    FaultCell faults = DisplayFaults.Cell();
                                    Channel<ViewportPointerFact> channel = ask.Plan.Overflow.Bounded<ViewportPointerFact>(ask.Plan.Capacity, rejected);
                                    return Fin.Succ(new PointerLease(channel, rejected, faults, lifecycle, ask.Veto, bind));
                                })
                                from armed in lease.Enable()
                                select lease);
                    }),
                new HookBinding<RhinoPoint, PluginKey, ChannelPlan, WidgetHost>(
                    Point: RhinoPoint.DisplayWidget,
                    Owner: plugin,
                    Bind: static ask => WidgetHost.Of(plan: ask))),
            key: op);
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
