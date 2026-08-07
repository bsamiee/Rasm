# [RASM_RHINO_DISPLAY_INTERACTION]

`Pointers.Configure`, `Gumballs.Configure`, and `WidgetHost.Configure` own the three Rhino viewport-interaction modalities. Each admits host input once, emits bounded value facts, and confines callback arguments, mouse state, acquisition handles, and registered UI objects to its lease.

`Marks.Render` remains the widget paint seam, document mutation remains downstream of `GumballReceipt.Completed` carrying `GumballDecision.Accept`, and `DocumentSession` binds document-local widget registration without exporting `RhinoDoc`.

## [01]-[INDEX]

- [02]-[POINTERS]: callback phase projection, overflow policy, and pointer lease.
- [03]-[GUMBALL]: geometry seating, pick/update fold, and transform evidence.
- [04]-[WIDGETS]: registered grip, label, SVG, and slider families over one fact channel.
- [05]-[HOOKS]: `DisplayHooks` mounts the pointer and widget hook points onto the registry.

## [02]-[POINTERS]

- Owner: `PointerFact` carries phase, edge, button, modifiers, gumball occupancy, viewport identity, point, cancellation evidence, and monotonic ordinal.
- Entry: `Pointers.Configure` mounts or retires a hook and sets tooltip text through one request family.
- Law: the pointer seam IS veto-capable — `MouseCallbackEventArgs` derives `CancelEventArgs`, so a Begin edge asks the mount's admitted predicate and writes `Cancel` to suppress Rhino's own default handling, while the matching End edge READS `Cancel` back as evidence of whether that default ran. The predicate is the one synchronous obligation on the callback thread; everything else stays one non-blocking `TryWrite`, overflow behavior is explicit policy, and drop and veto counts both remain readable from the lease.
- Law: arming crosses the marshal seam — `MouseCallback.Enabled` reflects over the subclass to subscribe and unsubscribe the host's own static view-event tables, so both edges run through `HostThread.Run`, and a mount whose arm refuses never returns a live lease.
- Law: retire closes callback admission, disables the hook, settles admitted callbacks within the request-owned bound, completes the channel, and then snapshots final totals; the gate refuses a close issued from a thread already holding a claim, since such a close waits on its own release.
- Law: the bounded drain rides the SCHEDULER, never the closing caller's thread — `LifecycleGate.Begin` arms the close, runs the stop step inline so a marshalled arm keeps its seam, and hands back the completion a host-thread owner settles off-thread, while `Close` awaits that same completion for a pool caller; a blocking drain on the host thread stalls exactly the callbacks it waits to see released.
- Boundary: `MouseCallbackEventArgs` and `MouseCallback` never cross the callback adapter.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Threading.Channels;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.HostUi;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
// LifecycleGate and LeaseState compose from Rasm.Rhino.Document (Document/events.md) — the package's one
// claims/close/retry lifecycle capsule; the async Begin/Drain shape lives at that owner.


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

[SmartEnum<int>]
public sealed partial class PointerOverflow {
    public static readonly PointerOverflow Oldest = new(0, BoundedChannelFullMode.DropOldest);
    public static readonly PointerOverflow Newest = new(1, BoundedChannelFullMode.DropNewest);
    public static readonly PointerOverflow Incoming = new(2, BoundedChannelFullMode.DropWrite);
    internal BoundedChannelFullMode Native { get; }
    internal Channel<T> Bounded<T>(int capacity, Atom<long> rejected) =>
        Channel.CreateBounded<T>(new BoundedChannelOptions(capacity) {
            FullMode = Native,
            SingleReader = false,
            SingleWriter = true,
        }, _ => ignore(rejected.Swap(static count => count + 1)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointerRequest {
    private PointerRequest() { }
    // `Veto` is the one synchronous callback obligation on the mount: it runs on the host pointer thread inside the claim
    // and its verdict writes `MouseCallbackEventArgs.Cancel`, so it stays a pure predicate over the already-projected fact.
    public sealed record Mount(int Capacity, PointerOverflow Overflow, TimeSpan SettleWithin, Option<Func<PointerFact, bool>> Veto = default) : PointerRequest;
    public sealed record Tooltip(string Text) : PointerRequest;
    public sealed record Retire(PointerLease Lease) : PointerRequest;

    internal bool Valid => Switch(
        mount: static row => row.Overflow is not null
            && row.Veto.Match(Some: static value => value is not null, None: static () => true),
        tooltip: static row => row.Text is not null,
        retire: static row => row.Lease is not null);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointerReceipt {
    private PointerReceipt() { }
    public sealed record Mounted(PointerLease Lease) : PointerReceipt;
    public sealed record TooltipSet : PointerReceipt;
    public sealed record Retired(long Submitted, long Rejected, long Vetoed) : PointerReceipt;
}

// --- [MODELS] -------------------------------------------------------------------------------
// `Cancelled` is host evidence, not a request: a Begin-edge fact reports the verdict the veto policy just returned, and an
// End-edge fact reports whether Rhino's own default handling ran.
public readonly record struct PointerFact(
    PointerPhase Phase,
    PointerEdge Edge,
    Guid Viewport,
    Point2d At,
    PointerButton Button,
    bool Shift,
    bool Control,
    bool OverGumball,
    bool Cancelled,
    long Ordinal);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class PointerLease : IDisposable {
    private readonly Channel<PointerFact> channel;
    private readonly PointerHook hook;
    private readonly LifecycleGate lifecycle;
    private readonly Atom<long> rejected;
    private readonly Atom<Seq<Error>> faults;
    private readonly Op key;
    internal PointerLease(Channel<PointerFact> channel, Atom<long> rejected, Atom<Seq<Error>> faults, LifecycleGate lifecycle, Option<Func<PointerFact, bool>> veto, Op key) {
        (this.channel, this.rejected, this.faults, this.lifecycle, this.key) = (channel, rejected, faults, lifecycle, key);
        hook = new PointerHook(channel.Writer, rejected, faults, lifecycle, veto, key);
    }
    public ChannelReader<PointerFact> Facts => channel.Reader;
    public Seq<Error> Faults => faults.Value;
    public long Submitted => hook.Submitted;
    public long Rejected => rejected.Value;
    public long Vetoed => hook.Vetoed;
    public Option<int> Buffered => channel.Reader.CanCount ? Some(channel.Reader.Count) : None;
    internal Fin<Unit> Enable() => Arm(enabled: true);
    internal Fin<PointerReceipt> Retire() => Release().Map(_ =>
        (PointerReceipt)new PointerReceipt.Retired(Submitted, Rejected, Vetoed));

    // `MouseCallback.Enabled` subscribes and unsubscribes the host's own static `RhinoView` event tables, so both edges
    // cross the one marshal seam; a background arm races the table the pointer thread is walking.
    private Fin<Unit> Arm(bool enabled) => HostThread.Run(
        work: new HostWork<Unit>.Execute(Body: () => key.Catch(() => Fin.Succ((hook.Enabled = enabled, unit).Item2))),
        key: key);

    private Fin<Unit> Release() => lifecycle.Close(
        stop: () => Arm(enabled: false),
        settle: () => Fin.Succ((channel.Writer.TryComplete(), unit).Item2),
        key: key);

    public void Dispose() => _ = Release().IfFail(failure => ignore(faults.Swap(rows => rows.Add(failure))));
}

internal sealed class PointerHook : MouseCallback {
    private readonly ChannelWriter<PointerFact> sink;
    private readonly Atom<long> rejected;
    private readonly Atom<long> vetoed = Atom(0L);
    private readonly Atom<Seq<Error>> faults;
    private readonly LifecycleGate lifecycle;
    private readonly Option<Func<PointerFact, bool>> veto;
    private readonly Op key;
    private long ordinal;
    internal PointerHook(ChannelWriter<PointerFact> sink, Atom<long> rejected, Atom<Seq<Error>> faults, LifecycleGate lifecycle, Option<Func<PointerFact, bool>> veto, Op key) =>
        (this.sink, this.rejected, this.faults, this.lifecycle, this.veto, this.key) = (sink, rejected, faults, lifecycle, veto, key);
    internal long Submitted => Interlocked.Read(ref ordinal);
    internal long Vetoed => vetoed.Value;

    protected override void OnMouseMove(MouseCallbackEventArgs e) => Emit(PointerPhase.Move, PointerEdge.Begin, e);
    protected override void OnEndMouseMove(MouseCallbackEventArgs e) => Emit(PointerPhase.Move, PointerEdge.End, e);
    protected override void OnMouseDown(MouseCallbackEventArgs e) => Emit(PointerPhase.Down, PointerEdge.Begin, e);
    protected override void OnEndMouseDown(MouseCallbackEventArgs e) => Emit(PointerPhase.Down, PointerEdge.End, e);
    protected override void OnMouseUp(MouseCallbackEventArgs e) => Emit(PointerPhase.Up, PointerEdge.Begin, e);
    protected override void OnEndMouseUp(MouseCallbackEventArgs e) => Emit(PointerPhase.Up, PointerEdge.End, e);
    protected override void OnMouseDoubleClick(MouseCallbackEventArgs e) => Emit(PointerPhase.DoubleClick, PointerEdge.Atomic, e);
    protected override void OnMouseEnter(MouseCallbackEventArgs e) => Emit(PointerPhase.Enter, PointerEdge.Atomic, e);
    protected override void OnMouseHover(MouseCallbackEventArgs e) => Emit(PointerPhase.Hover, PointerEdge.Atomic, e);
    protected override void OnMouseLeave(MouseCallbackEventArgs e) => Emit(PointerPhase.Leave, PointerEdge.Atomic, e);

    private void Emit(PointerPhase phase, PointerEdge edge, MouseCallbackEventArgs e) {
        _ = lifecycle.Within(() => {
            long current = Interlocked.Increment(ref ordinal);
            // An End edge reads the host's own `Cancel` to report whether the default ran; a Begin edge asks the admitted
            // policy and WRITES it, which is the whole veto — the projected fact is what the policy decides on.
            PointerFact projected = new(
                phase, edge, e.View.ActiveViewport.Id, new Point2d(e.ViewportPoint.X, e.ViewportPoint.Y),
                PointerButton.Of(e.MouseButton), e.ShiftKeyDown, e.CtrlKeyDown,
                e.IsOverGumball() != GumballMode.None, e.Cancel, current);
            PointerFact settled = edge == PointerEdge.End
                ? projected
                : veto.Map(decide => decide(projected)).IfNone(false) switch {
                    true => (e.Cancel = true, ignore(vetoed.Swap(static count => count + 1)), projected with { Cancelled = true }).Item3,
                    false => projected,
                };
            _ = Op.SideWhen(!sink.TryWrite(settled), () => ignore(rejected.Swap(static count => count + 1)));
            return Fin.Succ(unit);
        }, static () => Fin.Succ(unit), key).IfFail(failure => ignore(faults.Swap(rows => rows.Add(failure))));
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Pointers {
    public static Fin<PointerReceipt> Configure(PointerRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return guard(request is not null && request.Valid, op.InvalidInput()).ToFin().Bind(_ => request.Switch(
            op,
            mount: static (op, row) => from capacity in op.Positive(row.Capacity)
                                      from lifecycle in LifecycleGate.Of(row.SettleWithin, op)
                                      from lease in op.Catch(() => {
                                          Atom<long> rejected = Atom(0L);
                                          Atom<Seq<Error>> faults = Atom(Seq<Error>());
                                          Channel<PointerFact> channel = row.Overflow.Bounded<PointerFact>((int)capacity, rejected);
                                          return Fin.Succ(new PointerLease(channel, rejected, faults, lifecycle, row.Veto, op));
                                      })
                                      from armed in lease.Enable()
                                      select (PointerReceipt)new PointerReceipt.Mounted(lease),
            tooltip: static (op, row) => from text in op.AcceptText(row.Text)
                                         from _ in op.Catch(() => MouseCursor.SetToolTip(text))
                                         select (PointerReceipt)new PointerReceipt.TooltipSet(),
            retire: static (_, row) => row.Lease.Retire()));
    }
}
```

## [03]-[GUMBALL]

- Owner: `GumballSeat` closes the host seating family; `GumballMove` closes line-ray and plane updates; `GumballLook` closes manipulator appearance; `GumballEvidence` carries total and incremental transforms without mutating geometry.
- Entry: `Gumballs.Configure` owns mount, pick, move, inspect, and completion over one request algebra.
- Law: completion returns accepted or rejected evidence; the caller alone applies an accepted transform through its transaction rail.
- Law: a move admits its shape at the request — ray point and world line, frame plane validity — and the whole native update, evidence read included, runs inside one catch boundary, so a host throw lands as a `Fin` failure, never an escape from `Gumballs.Configure`.
- Law: appearance is one admitted value the seat call takes whole — the host settings carrier is a plain settable, so a mount projects `GumballLook` into it once and never mutates it after seating; a default-constructed settings object hands the manipulator whatever the host happens to default to.
- Law: the rig owns exactly the two host objects it minted — `GumballObject` and `GumballDisplayConduit`, both disposable — and `Release` disables the conduit before disposing both, records every step's failure on its own fault cell, and re-arms `Dispose` for retry.
- Boundary: `PickContext` and `GetPoint` are the command rail's, BORROWED by `Pick` for the length of the call; the rig holds neither and disposes neither.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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

[SmartEnum<int>]
public sealed partial class GumballDecision {
    public static readonly GumballDecision Reject = new(key: 0, accepted: false);
    public static readonly GumballDecision Accept = new(key: 1, accepted: true);

    public bool Accepted { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GumballRequest {
    private GumballRequest() { }
    public sealed record Mount(GumballSeat Seat, ActiveSpaceUse Space, GumballLook Look) : GumballRequest;
    // `Pick` BORROWS the caller's acquisition pair for the length of the call: `PickContext` and `GetPoint` belong to the
    // command rail that opened them, and the rig disposes neither.
    internal sealed record Pick(GumballRig Rig, PickContext Context, GetPoint Point) : GumballRequest;
    public sealed record Move(GumballRig Rig, GumballMove Value) : GumballRequest;
    public sealed record Inspect(GumballRig Rig) : GumballRequest;
    public sealed record Complete(GumballRig Rig, GumballDecision Decision) : GumballRequest;

    internal bool Valid => Switch(
        mount: static row => row.Seat is not null && row.Seat.Valid && row.Space is not null && row.Look is not null,
        pick: static row => row.Rig is not null && row.Rig.IsLive && row.Context is not null && row.Point is not null,
        move: static row => row.Rig is not null && row.Rig.IsLive && row.Value is not null && row.Value.Valid,
        inspect: static row => row.Rig is not null && row.Rig.IsLive,
        complete: static row => row.Rig is not null && row.Rig.IsLive && row.Decision is not null);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GumballReceipt {
    private GumballReceipt() { }
    public sealed record Mounted(GumballRig Rig) : GumballReceipt;
    public sealed record Picked(bool Value) : GumballReceipt;
    public sealed record Moved(GumballEvidence Evidence) : GumballReceipt;
    public sealed record Inspected(GumballEvidence Evidence) : GumballReceipt;
    public sealed record Completed(GumballEvidence Evidence, GumballDecision Decision) : GumballReceipt;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct GumballEvidence(Transform Total, Transform Incremental, bool Dragging);

// The manipulator's whole appearance as one admitted value the seat call takes: the host settings object is a plain
// settable carrier, so a mount hands it over complete rather than mutating it in place after seating.
[SmartEnum<int>]
public sealed partial class GumballHandle {
    public static readonly GumballHandle TranslateX = new(0);
    public static readonly GumballHandle TranslateY = new(1);
    public static readonly GumballHandle TranslateZ = new(2);
    public static readonly GumballHandle TranslateXY = new(3);
    public static readonly GumballHandle TranslateYZ = new(4);
    public static readonly GumballHandle TranslateZX = new(5);
    public static readonly GumballHandle RotateX = new(6);
    public static readonly GumballHandle RotateY = new(7);
    public static readonly GumballHandle RotateZ = new(8);
    public static readonly GumballHandle ScaleX = new(9);
    public static readonly GumballHandle ScaleY = new(10);
    public static readonly GumballHandle ScaleZ = new(11);
    public static readonly GumballHandle Relocate = new(12);
    public static readonly GumballHandle Menu = new(13);
}

public sealed record GumballLook {
    private GumballLook(FrozenSet<GumballHandle> handles, PerceptualColor x, PerceptualColor y, PerceptualColor z, PerceptualColor menu, int radius, int axisThickness, int arcThickness) =>
        (Handles, X, Y, Z, Menu, Radius, AxisThickness, ArcThickness) =
        (handles, x, y, z, menu, radius, axisThickness, arcThickness);

    public FrozenSet<GumballHandle> Handles { get; }
    public PerceptualColor X { get; }
    public PerceptualColor Y { get; }
    public PerceptualColor Z { get; }
    public PerceptualColor Menu { get; }
    public int Radius { get; }
    public int AxisThickness { get; }
    public int ArcThickness { get; }

    public static Fin<GumballLook> Of(
        FrozenSet<GumballHandle> handles,
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
            TranslateXEnabled = Handles.Contains(GumballHandle.TranslateX),
            TranslateYEnabled = Handles.Contains(GumballHandle.TranslateY),
            TranslateZEnabled = Handles.Contains(GumballHandle.TranslateZ),
            TranslateXYEnabled = Handles.Contains(GumballHandle.TranslateXY),
            TranslateYZEnabled = Handles.Contains(GumballHandle.TranslateYZ),
            TranslateZXEnabled = Handles.Contains(GumballHandle.TranslateZX),
            RotateXEnabled = Handles.Contains(GumballHandle.RotateX),
            RotateYEnabled = Handles.Contains(GumballHandle.RotateY),
            RotateZEnabled = Handles.Contains(GumballHandle.RotateZ),
            ScaleXEnabled = Handles.Contains(GumballHandle.ScaleX),
            ScaleYEnabled = Handles.Contains(GumballHandle.ScaleY),
            ScaleZEnabled = Handles.Contains(GumballHandle.ScaleZ),
            RelocateEnabled = Handles.Contains(GumballHandle.Relocate),
            MenuEnabled = Handles.Contains(GumballHandle.Menu),
            ColorX = Quant.Sys(X),
            ColorY = Quant.Sys(Y),
            ColorZ = Quant.Sys(Z),
            ColorMenuButton = Quant.Sys(Menu),
            Radius = Radius,
            AxisThickness = AxisThickness,
            ArcThickness = ArcThickness,
        };
        return settings;
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class GumballRig : IDisposable {
    private readonly GumballObject gumball;
    private readonly GumballDisplayConduit conduit;
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private int released;
    internal GumballRig(GumballObject gumball, GumballDisplayConduit conduit) =>
        (this.gumball, this.conduit) = (gumball, conduit);
    internal bool IsLive => Volatile.Read(ref released) == 0;
    internal bool Dragging { get; set; }
    internal GumballEvidence Evidence => new(conduit.TotalTransform, conduit.GumballTransform, Dragging);
    internal GumballDisplayConduit Conduit => conduit;
    public Seq<Error> Faults => faults.Value;

    internal Fin<Unit> Release(Op key) {
        if (Interlocked.Exchange(ref released, 1) != 0) { return Fin.Succ(unit); }
        Seq<Error> trouble = Seq(
                key.Catch(() => { conduit.Enabled = false; return Fin.Succ(unit); }),
                key.Catch(() => { conduit.Dispose(); return Fin.Succ(unit); }),
                key.Catch(() => { gumball.Dispose(); return Fin.Succ(unit); }))
            .Choose(static step => step.Match(
                Succ: static _ => Option<Error>.None,
                Fail: static failure => Some(failure)));
        _ = Op.SideWhen(!trouble.IsEmpty, () => ignore(faults.Swap(rows => rows + trouble)));
        return trouble.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(trouble.Fold(Errors.None, static (folded, failure) => folded + failure));
    }

    // A failed release re-arms so the next attempt retries, and the fault stays readable off the rig — the same shape
    // `ConduitLease` and `RetainedOverlay` hold.
    public void Dispose() => _ = Release(Op.Of(nameof(GumballRig)))
        .IfFail(_ => Volatile.Write(ref released, 0));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Gumballs {
    public static Fin<GumballReceipt> Configure(GumballRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return guard(request is not null && request.Valid, op.InvalidInput()).ToFin().Bind(_ => request.Switch(
            op,
            mount: static (op, row) => Mount(row, op).Map(static rig => (GumballReceipt)new GumballReceipt.Mounted(rig)),
            // The drag flag is an EFFECT, so it lands as one on the effect path — a statement body inside `Map` writes
            // host-adjacent state on the success rail, where a reader expects a pure projection of the value it carries.
            pick: static (op, row) => op.Catch(() => row.Rig.Conduit.PickGumball(row.Context, row.Point))
                .Bind(value => op.Catch(() => Fin.Succ((Op.Side(() => row.Rig.Dragging = value), value).Item2)))
                .Map(static value => (GumballReceipt)new GumballReceipt.Picked(value)),
            move: static (op, row) => op.Catch(() => row.Value.Switch(
                    (Rig: row.Rig, Op: op),
                    ray: static (ctx, value) => ctx.Op.Confirm(ctx.Rig.Conduit.UpdateGumball(value.Point, value.WorldLine)),
                    frame: static (ctx, value) => ctx.Op.Confirm(ctx.Rig.Conduit.UpdateGumball(value.Value)))
                .Map(_ => (GumballReceipt)new GumballReceipt.Moved(row.Rig.Evidence))),
            inspect: static (op, row) => op.Catch(() => Fin.Succ<GumballReceipt>(new GumballReceipt.Inspected(row.Rig.Evidence))),
            // Tuple elements settle left to right, so the flag clears before `Evidence` reads it and the receipt carries
            // the settled drag state rather than the one the completing call arrived under.
            complete: static (op, row) => op.Catch(() => Fin.Succ((
                Op.Side(() => row.Rig.Dragging = false),
                (GumballReceipt)new GumballReceipt.Completed(row.Rig.Evidence, row.Decision)).Item2))));
    }

    private static Fin<GumballRig> Mount(GumballRequest.Mount row, Op op) =>
        op.Catch(() => Fin.Succ(new GumballObject())).Bind(ball =>
            op.Catch(() => Fin.Succ(new GumballDisplayConduit(row.Space.Key))).BiBind(
                Succ: pipe => {
                    GumballRig rig = new(ball, pipe);
                    return (from _ in op.Catch(() => row.Seat.Apply(ball, op))
                            from __ in op.Catch(() => {
                                pipe.SetBaseGumball(ball, row.Look.Native());
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
                    Fail: cleanup => Fin.Fail<GumballRig>(failure + cleanup))));
}
```

## [04]-[WIDGETS]

- Owner: `WidgetSpec` closes grip, direction, rotation, text-dot, SVG, and slider payloads; `WidgetScope` chooses all-document or document-group registration.
- Entry: `WidgetHost.Configure` mounts, changes, inspects, or retires one widget and returns typed identity and state receipts.
- Law: adapters project every host callback through one `WidgetSink`; painting draws the native widget visual through `base.OnDraw`, then overlays the mounted `Seq<Mark>` through `Marks.Render` under the `WidgetOverlay` phase the seam occupies, and never invents a renderer.
- Law: the mark program is a swappable slot on the sink, so `Change` retargets an overlay in place beside the two host writes — every widget state moves through one request and none demands retire-and-remount, the move that drops the `WidgetId` a consumer keys on; an absent mark program leaves the mounted one standing.
- Law: document-local registration uses `ViewUserInterface.Add` and removal uses the same table; all-document registration pairs `RegisterForAllDocuments` with `Unregister`.
- Law: `LifecycleGate` admits configuration and callback claims, closes admission before bounded retirement, and retains failed mounts for the next cleanup attempt.
- Law: change captures native state before either write and compensates visibility and active-view binding independently when either write fails.
- Boundary: a mounted widget is visible only through `WidgetId`, `WidgetFact`, and `WidgetState` values.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetSpec {
    private WidgetSpec() { }
    public sealed record Grip(Point3d At, double Radius, WidgetConstraint Constraint, Seq<Point3d> Snaps, bool ObjectSnap) : WidgetSpec;
    public sealed record Direction(Point3d At, Vector3d Vector, double Radius, Option<double> LineLength, bool OneWay, bool GripPointVisible) : WidgetSpec;
    public sealed record Rotation(Plane Plane, double Radius, bool GripPointVisible) : WidgetSpec;
    public sealed record Text(string Value, Point3d At, int Height, PerceptualColor Ink, PerceptualColor Fill, PerceptualColor Border) : WidgetSpec;
    // The SVG control seats at an INTEGER screen origin, so the spec carries the folder's integer offset owner and the
    // adapter projects it at the host boundary — a `Point2d` here truncated a continuous coordinate no caller measured
    // in pixels, and the negative origin it admitted is a value the offset owner refuses at its own factory.
    public sealed record Svg(string Value, Offset2i At, Size2i Extent) : WidgetSpec;
    public sealed record Slider(Interval Range, double Value, bool Horizontal, bool DisplayValue, int Precision) : WidgetSpec;

    internal bool Valid => Switch(
        grip: static row => row.At.IsValid
            && row.Constraint is not null
            && row.Constraint.Valid
            && row.Snaps.ForAll(static point => point.IsValid)
            && row.Radius > 0.0
            && double.IsFinite(row.Radius),
        direction: static row => row.At.IsValid
            && row.Vector.IsValid
            && !row.Vector.IsZero
            && row.Radius > 0.0
            && double.IsFinite(row.Radius)
            && row.LineLength.Match(Some: static value => value > 0.0 && double.IsFinite(value), None: static () => true),
        rotation: static row => row.Plane.IsValid && row.Radius > 0.0 && double.IsFinite(row.Radius),
        text: static row => !string.IsNullOrWhiteSpace(row.Value) && row.At.IsValid && row.Height > 0,
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

// Every adapter forwards the SAME six pointer callbacks, and only the host base type differs — a base type is no reason
// to re-spell a table six times. `WidgetPulse` IS that table: each row names the fact one host callback projects, so an
// adapter binds the pointer surface as six one-line delegations to one sink member and a new adapter adds no seventh
// spelling. Hover carries its own edge in the row rather than a bool argument two call sites could disagree on.
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
public abstract partial record WidgetRequest {
    private WidgetRequest() { }
    public sealed record Mount(WidgetSpec Spec, WidgetScope Scope, bool ActiveViewOnly, bool Visible, Seq<Mark> Marks) : WidgetRequest;
    // An absent mark program leaves the mounted one standing, so a visibility change never re-states the overlay.
    public sealed record Change(WidgetId Widget, bool Visible, bool ActiveViewOnly, Option<Seq<Mark>> Marks = default) : WidgetRequest;
    public sealed record Inspect(WidgetId Widget) : WidgetRequest;
    public sealed record Retire(WidgetId Widget) : WidgetRequest;

    internal bool Valid => Switch(
        mount: static row => row.Spec is not null
            && row.Spec.Valid
            && row.Scope is not null
            && row.Scope.Valid
            && row.Marks.ForAll(static mark => mark is not null && mark.Valid),
        change: static row => row.Widget.Value != Guid.Empty
            && row.Marks.Match(
                Some: static marks => marks.ForAll(static mark => mark is not null && mark.Valid),
                None: static () => true),
        inspect: static row => row.Widget.Value != Guid.Empty,
        retire: static row => row.Widget.Value != Guid.Empty);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetReceipt {
    private WidgetReceipt() { }
    public sealed record Mounted(WidgetId Widget) : WidgetReceipt;
    public sealed record Changed(WidgetState State) : WidgetReceipt;
    public sealed record Inspected(WidgetState State) : WidgetReceipt;
    public sealed record Retired(WidgetId Widget) : WidgetReceipt;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct WidgetState(WidgetId Widget, bool Visible, bool ActiveViewOnly, bool Registered);
internal sealed record WidgetEffect : IDetachedDocumentResult;
internal sealed record WidgetMount(UserInterfaceObjectBase Native, WidgetSink Sink, Func<Fin<Unit>> Retire);
// `Program` is a SLOT, not a construction argument: a widget's overlay marks are the one piece of its state that would
// otherwise need retire-and-remount, which drops the `WidgetId` every consumer keys on.
internal sealed record WidgetSink(
    WidgetId Identity,
    ChannelWriter<WidgetFact> Writer,
    Atom<Seq<Mark>> Program,
    SpriteSheet Sprites,
    Atom<Seq<Error>> Faults,
    Atom<long> Submitted,
    Atom<long> Rejected,
    LifecycleGate Lifecycle,
    Op Key) {
    internal Unit Paint(DrawEventArgs args) => Observe(Lifecycle.Within(
        body: () => Marks.Render(
            new Canvas.Pipeline(ConduitFrame.Of(args.Display, args.Viewport, ConduitPhase.WidgetOverlay)),
            Sprites,
            Program.Value).Map(static _ => unit),
        refused: static () => Fin.Succ(unit),
        key: Key));

    internal Unit Retarget(Seq<Mark> marks) => ignore(Program.Swap(_ => marks));

    // The pointer surface enters through ONE member the `WidgetPulse` table drives; the value-carrying callbacks keep
    // their own members because each admits a payload the pointer table has nowhere to put.
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
        Fail: failure => ignore(Faults.Swap(rows => rows.Add(failure))));
}

// --- [ADAPTERS] -----------------------------------------------------------------------------
internal sealed class GripWidget : GripUserInterfaceObject {
    private readonly WidgetSink sink;
    private readonly WidgetConstraint constraint;
    internal GripWidget(WidgetSpec.Grip spec, WidgetSink sink) : base(spec.At) {
        this.sink = sink;
        GripRadius = (float)spec.Radius;
        ObjectSnapPermitted = spec.ObjectSnap;
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
        DisplayValue = spec.DisplayValue;
        DigitPrecision = spec.Precision;
    }
    protected override void OnDraw(DrawEventArgs e) { base.OnDraw(e); sink.Paint(e); }
    protected override void OnValueChanged() => sink.Slide(Value);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class WidgetHost : IDisposable {
    private readonly Channel<WidgetFact> channel;
    private readonly Atom<HashMap<WidgetId, WidgetMount>> mounted = Atom(HashMap<WidgetId, WidgetMount>());
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private readonly Atom<long> submitted = Atom(0L);
    private readonly Atom<long> rejected;
    private readonly LifecycleGate lifecycle;
    private readonly SpriteSheet sprites = new();
    private WidgetHost(Channel<WidgetFact> channel, Atom<long> rejected, LifecycleGate lifecycle) =>
        (this.channel, this.rejected, this.lifecycle) = (channel, rejected, lifecycle);
    public ChannelReader<WidgetFact> Facts => channel.Reader;
    public Seq<Error> Faults => faults.Value;
    public long Submitted => submitted.Value;
    public long Rejected => rejected.Value;
    public Option<int> Buffered => channel.Reader.CanCount ? Some(channel.Reader.Count) : None;

    public static Fin<WidgetHost> Of(int capacity, PointerOverflow overflow, TimeSpan settleWithin, Op? key = null) {
        Op op = key.OrDefault();
        return guard(overflow is not null, op.InvalidInput()).ToFin()
            .Bind(_ => op.Positive(capacity))
            .Bind(value => LifecycleGate.Of(settleWithin, op).Bind(lifecycle => op.Catch(() => {
                Atom<long> rejected = Atom(0L);
                Channel<WidgetFact> channel = overflow.Bounded<WidgetFact>((int)value, rejected);
                return Fin.Succ(new WidgetHost(channel, rejected, lifecycle));
            })));
    }

    public Fin<WidgetReceipt> Configure(WidgetRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return lifecycle.Within(
            body: () => guard(request is not null && request.Valid, op.InvalidInput()).ToFin().Bind(_ => request.Switch(
                (Host: this, Op: op),
                mount: static (ctx, row) => ctx.Host.Mount(row, ctx.Op),
                change: static (ctx, row) => ctx.Host.Change(row, ctx.Op),
                inspect: static (ctx, row) => ctx.Host.Find(row.Widget, ctx.Op).Bind(value => ctx.Op.Catch(() =>
                    Fin.Succ<WidgetReceipt>(new WidgetReceipt.Inspected(ctx.Host.State(row.Widget, value))))),
                retire: static (ctx, row) => from value in ctx.Host.Find(row.Widget, ctx.Op)
                                             from _ in ctx.Host.Retire(row.Widget, value, ctx.Op)
                                             select (WidgetReceipt)new WidgetReceipt.Retired(row.Widget))),
            refused: () => Fin.Fail<WidgetReceipt>(op.InvalidContext()),
            key: op);
    }

    private Fin<WidgetReceipt> Mount(WidgetRequest.Mount row, Op op) {
        WidgetId identity = WidgetId.Create(Guid.NewGuid());
        WidgetSink sink = new(identity, channel.Writer, Atom(row.Marks), sprites, faults, submitted, rejected, lifecycle, op);
        return from widget in op.Catch(() => row.Spec.Switch(
                   sink,
                   grip: static (state, spec) => (UserInterfaceObjectBase)new GripWidget(spec, state),
                   direction: static (state, spec) => new DirectionWidget(spec, state),
                   rotation: static (state, spec) => new RotationWidget(spec, state),
                   text: static (state, spec) => new TextWidget(spec, state),
                   svg: static (state, spec) => new SvgWidget(spec, state),
                   slider: static (state, spec) => new SliderWidget(spec, state)))
               from retire in row.Scope.Switch(
                   (Widget: widget, Op: op),
                   allDocuments: static (ctx, _) => ctx.Op.Confirm(ctx.Widget.RegisterForAllDocuments())
                       .Map(_ => (Func<Fin<Unit>>)(() => ctx.Op.Catch(ctx.Widget.Unregister))),
                   document: static (ctx, scope) => scope.Session.Demand(
                       use: doc => ctx.Op.Confirm(doc.ViewUserInterface.Add(ctx.Widget, scope.Group)).Map(static _ => new WidgetEffect()),
                       key: ctx.Op,
                       needs: [SessionNeed.Mutate])
                       .Map(_ => (Func<Fin<Unit>>)(() => scope.Session.Demand(
                           use: doc => ctx.Op.Catch(() => ctx.Op.Confirm(doc.ViewUserInterface.Remove(ctx.Widget) > 0)
                               .Map(static _ => new WidgetEffect())),
                           key: ctx.Op,
                           needs: [SessionNeed.Mutate]).Map(static _ => unit))))
               let value = new WidgetMount(widget, sink, retire)
               from receipt in SetState(identity, value, row.Visible, row.ActiveViewOnly, op)
                   .Bind(_ => op.Catch(() => {
                       _ = mounted.Swap(items => items.Add(identity, value));
                       return Fin.Succ<WidgetReceipt>(new WidgetReceipt.Mounted(identity));
                   })).BiBind(
                   Succ: static value => Fin.Succ(value),
                   Fail: error => retire().Match(
                       Succ: _ => Fin.Fail<WidgetReceipt>(error),
                       Fail: cleanup => Fin.Fail<WidgetReceipt>(error + cleanup)))
               select receipt;
    }

    // The mark program swaps in place beside the two host writes, so an overlay change keeps the `WidgetId` every consumer
    // keys on; the swap is pure, so only the host writes need compensation.
    private Fin<WidgetReceipt> Change(WidgetRequest.Change row, Op op) =>
        Find(row.Widget, op).Bind(value => op.Catch(() => Fin.Succ(State(row.Widget, value))).Bind(prior =>
            SetState(row.Widget, value, row.Visible, row.ActiveViewOnly, op)
                .Map(state => (WidgetReceipt)new WidgetReceipt.Changed(
                    (ignore(row.Marks.Iter(value.Sink.Retarget)), state).Item2))
                .BindFail(primary => Restore(value, prior, op).Match(
                    Succ: _ => Fin.Fail<WidgetReceipt>(primary),
                    Fail: cleanup => Fin.Fail<WidgetReceipt>(primary + cleanup)))));

    private Fin<WidgetMount> Find(WidgetId identity, Op op) => mounted.Value.Find(identity).ToFin(op.InvalidInput());
    private Fin<Unit> Retire(WidgetId identity, WidgetMount value, Op op) =>
        from _ in value.Retire()
        from __ in op.Catch(() => Fin.Succ((mounted.Swap(items => items.Remove(identity)), unit).Item2))
        select unit;

    private static WidgetState State(WidgetId identity, WidgetMount value) =>
        new(identity, value.Native.Visible, value.Native.BoundToActiveView, value.Native.IsRegistered());

    private static Fin<WidgetState> SetState(WidgetId identity, WidgetMount value, bool visible, bool activeViewOnly, Op op) =>
        op.Catch(() => {
            value.Native.Visible = visible;
            value.Native.BoundToActiveView = activeViewOnly;
            return Fin.Succ(State(identity, value));
        });

    private static Fin<Unit> Restore(WidgetMount value, WidgetState prior, Op op) {
        Seq<Error> trouble = Seq(
                op.Catch(() => Fin.Succ((value.Native.Visible = prior.Visible, unit).Item2)),
                op.Catch(() => Fin.Succ((value.Native.BoundToActiveView = prior.ActiveViewOnly, unit).Item2)))
            .Choose(static step => step.Match(
                Succ: static _ => Option<Error>.None,
                Fail: static failure => Some(failure)));
        return trouble.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(trouble.Fold(Errors.None, static (folded, failure) => folded + failure));
    }

    private Fin<Unit> ReleaseAll(Op op) {
        return Aggregate(toSeq(mounted.Value)
                .Map(row => Retire(identity: row.Key, value: row.Value, op: op)))
            .Bind(_ => Aggregate(Seq(
                op.Catch(() => Fin.Succ((channel.Writer.TryComplete(), unit).Item2)),
                op.Catch(() => { sprites.Dispose(); return Fin.Succ(unit); }))));
    }

    private static Fin<Unit> Aggregate(Seq<Fin<Unit>> steps) => steps
        .Choose(static step => step.Match(
            Succ: static _ => Option<Error>.None,
            Fail: static failure => Some(failure)))
        .Strict() is var trouble && trouble.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(trouble.Fold(Errors.None, static (folded, failure) => folded + failure));

    public void Dispose() => _ = lifecycle.Close(
        stop: static () => Fin.Succ(unit),
        settle: () => ReleaseAll(Op.Of(nameof(WidgetHost))),
        key: Op.Of(nameof(WidgetHost)))
        .IfFail(error => ignore(faults.Swap(rows => rows.Add(error))));
}
```

## [05]-[HOOKS]

- Owner: `DisplayHooks.Mount` registers the two display hook points — `rasm.rhino.display.pointer` granting a `PointerLease` and `rasm.rhino.display.widget` granting a `WidgetHost` — each bind minting a fresh owner so no two consumers contend for one bounded channel.
- Law: each point carries the ask its seam can honour — both admit the same channel triple, but only the pointer seam vetoes, so `WidgetAsk` states capacity, overflow, and settle bound with NO veto slot while `PointerRequest.Mount` keeps its own; the shared triple is a shape coincidence, and reusing the pointer request handed the widget point a veto policy it silently dropped. `MountRegistry.MountAll` releases the first seat when the second point refuses.
- Law: display modality splits by seam, not by page — widget `MouseState` callbacks run post-hoc and observe, while the pointer seam vetoes: `MouseCallbackEventArgs` derives `CancelEventArgs`, so the pointer point's grant carries the mount's own veto policy and no second point is minted for it. The two draw-suppression seams (`CullObjectEventArgs.CullObject`, `DrawObjectEventArgs.DrawObject`) stay the conduit owner's.
- Law: gumball evidence returns on the `Gumballs.Configure` request rail, never as a detached stream, so no gumball point exists — a detached fact stream is the point prerequisite, and gumball occupancy already rides every `PointerFact`.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// The widget seam OBSERVES — its `MouseState` callbacks run post-hoc — so its ask carries the channel triple and no
// veto slot at all. A caller that spelled a veto against a seam with nothing to cancel had it accepted and discarded.
public sealed record WidgetAsk(int Capacity, PointerOverflow Overflow, TimeSpan SettleWithin);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class DisplayHooks {
    public static Fin<Seq<IDisposable>> Mount(PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        return MountRegistry.MountAll(
            mounts: Seq(
                (Func<Fin<IDisposable>>)(() => MountRegistry.Mount(
                    mount: new HookMount(
                    Point: RhinoPoint.DisplayPointer,
                    Plugin: plugin,
                    Ask: typeof(PointerRequest.Mount),
                    Grant: typeof(PointerLease),
                    Bind: static ask => ask switch {
                        PointerRequest.Mount request => Pointers.Configure(request).Bind(static receipt => receipt switch {
                            PointerReceipt.Mounted mounted => Fin.Succ<object>(value: mounted.Lease),
                            _ => Fin.Fail<object>(error: Op.Of(name: nameof(DisplayHooks)).InvalidResult()),
                        }),
                        _ => Fin.Fail<object>(error: Op.Of(name: nameof(DisplayHooks)).InvalidInput()),
                    }),
                    key: op)),
                () => MountRegistry.Mount(
                    mount: new HookMount(
                    Point: RhinoPoint.DisplayWidget,
                    Plugin: plugin,
                    Ask: typeof(WidgetAsk),
                    Grant: typeof(WidgetHost),
                    Bind: static ask => ask switch {
                        WidgetAsk request => WidgetHost.Of(
                                capacity: request.Capacity,
                                overflow: request.Overflow,
                                settleWithin: request.SettleWithin)
                            .Map(static host => (object)host),
                        _ => Fin.Fail<object>(error: Op.Of(name: nameof(DisplayHooks)).InvalidInput()),
                    }),
                    key: op)),
            key: op);
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
