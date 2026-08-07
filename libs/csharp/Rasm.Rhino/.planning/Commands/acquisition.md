# [RASM_RHINO_ACQUISITION]

`Acquisition.Get` interprets one admitted `Acquire` value inside a document acquire grant. Custom getters, options, point callbacks, modal routes, and native references remain scoped to that window; egress is one detached `AcquiredReceipt`.

## [01]-[INDEX]

- [02]-[PAYLOAD]: `Acquired`, `AcquireTerminal`, `AcquiredReceipt`, and the `ISlotted`/`Slots` one-per-slot contract every rule family on the page composes.
- [03]-[ACCEPTANCE]: `AcceptGate`, `AcceptRule`, and `AcceptPlan` — the parameterless accept calls, the gated modalities, and the terminal-derived widening.
- [04]-[POINT_ALGEBRA]: `PointConstraint`, `PointGate`, `PointerShape`, `SnapBarAxis`, `PointRule`, `PointerKey`, `PointFeedback`, `PointerFact`, and `PointPlan`.
- [05]-[REQUEST]: `TextMeaning`, `PromptCase`, `ObjectRule`/`ObjectPlan`, `DragPlan`, `ShapeAsk`, `FileAsk`, `ModalInput`, `AcquireIntent`, `InputDefault`, `ViewportFact`, and `Acquire`.
- [06]-[DRIVE]: `Acquisition.Probe`/`Get` and the `GetterDrive` option-cycle fold.
- [07]-[CALLBACK_BOUNDARY]: `DragBuffer`, `PointFeedbackLease`, and `TransformGetter`.
- [08]-[BOUNDARY]: the modality, custody, and unit-identity carves.
- [09]-[RESEARCH]: open verification rows.

## [02]-[PAYLOAD]

`Acquired` closes interactive, screen-space, scalar, object, geometry, view, transform, and file payloads. `AcquireTerminal` preserves every non-fault control terminal, including native timeout. `DragEvidence` detaches the drag buffer's object, grip, and owner census with its measured extent and applied-pose count, so the host buffer itself dies inside the drive.

`Acquired.Distance` pairs its magnitude with the kernel `ModelUnit` read off the document at parse time — the pairing `Document/session.md`'s `UnitText.LengthValueCase` already detaches on. Receipts outlive their acquire window, so a `UnitRegime` change between acquisition and use re-reads a bare magnitude in a regime that no longer produced it; consumers re-entering the value rescale through `ModelUnit.ScaleTo`, the branch's one scale owner.

`Acquired.Angle` carries no regime: radians ARE the canonical measure its name spells, `AngleGrammar` owns the degree/radian dialect on the TEXT side alone, and a regime column there names a fact no document holds.

`Acquired.Paint` carries the kernel `PerceptualColor`, admitted through `Slots.Shade` at the getter seam and quantized back through `Slots.Rgb` at the two write seams. `Acquired.ScreenPoint` keeps `System.Drawing.Point` — a screen struct IS the host's pixel frame and has no kernel counterpart — and that carve is confined to the detached fact: no operation on this page reads it back into a host call, so the struct never re-enters the boundary it came from.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
// No `using System.Drawing` and no `using Rhino.UI`: `System.Drawing.Point`/`Color`, `Rhino.UI.CursorStyle`, and
// `Rhino.UI.LocalizeStringPair` spell in full at their few seams, so `Point`, `Color`, and the kernel colour owner
// each resolve to exactly one type on this page.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ------------------------------------------------------------------------------
// The one-per-slot contract every rule family on this page composes — accept rules, point rules, object rules, and
// the selection page's pick rules — so it seats at the page's type tier rather than inside one family's section.
public interface ISlotted {
    object SlotKey { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Acquired {
    private Acquired() { }
    public sealed record Point(Point3d Value, PointEvidence Evidence) : Acquired;
    public sealed record ScreenPoint(System.Drawing.Point Value) : Acquired;
    public sealed record Objects(Seq<PickCapture> Picks) : Acquired;
    public sealed record Number(double Value) : Acquired;
    public sealed record Count(int Value) : Acquired;
    public sealed record Text(string Value) : Acquired;
    public sealed record Toggle(bool Value) : Acquired;
    public sealed record Paint(PerceptualColor Value) : Acquired;
    public sealed record Distance(double Value, ModelUnit Unit) : Acquired;
    public sealed record Angle(double Radians) : Acquired;
    public sealed record Segment(Line Value) : Acquired;
    public sealed record Chain(Polyline Value) : Acquired;
    public sealed record ArcShape(Arc Value) : Acquired;
    public sealed record CircleShape(Circle Value) : Acquired;
    public sealed record PlaneShape(Plane Value) : Acquired;
    public sealed record RectangleShape(Arr<Point3d> Corners) : Acquired;
    public sealed record BoxShape(Box Value) : Acquired;
    public sealed record Xform(Transform Value) : Acquired;
    public sealed record FileName(string Value) : Acquired;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcquireTerminal {
    private AcquireTerminal() { }
    public sealed record Value(Acquired Payload) : AcquireTerminal;
    public sealed record Cancelled : AcquireTerminal;
    public sealed record Nothing : AcquireTerminal;
    public sealed record Undone : AcquireTerminal;
    public sealed record TimedOut : AcquireTerminal;
    public sealed record Exit : AcquireTerminal;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PointEvidence(
    Option<uint> ViewSerial,
    Option<int> OsnapCode,
    Option<Point3d> BasePoint,
    Seq<Point3d> SnapPoints,
    Seq<Point3d> ConstructionPoints);

public sealed record DragEvidence(
    Seq<Guid> Objects,
    Seq<Guid> Grips,
    Seq<Guid> GripOwners,
    int ObjectCount,
    int GripCount,
    int GripOwnerCount,
    BoundingBox Extent,
    int Poses);

public sealed record AcquiredReceipt(
    AcquireTerminal Terminal,
    Seq<OptionChoice> Options,
    bool GotDefault,
    Option<DragEvidence> Dragged) : IDetachedDocumentResult {
    public Option<Acquired> Payload => Terminal is AcquireTerminal.Value value ? Some(value.Payload) : None;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Slots {
    public static bool OnePer<T>(this Seq<T> rules) where T : ISlotted =>
        rules.Map(static rule => rule.SlotKey).Distinct().Count == rules.Count;

    // The kernel colour rail is the page's colour identity; `System.Drawing.Color` survives only as the byte
    // quadruple the host getter reads and writes, and it crosses through this one pair — a raw host colour on a
    // receipt column would re-open the sRGB component arithmetic the kernel owner already closed.
    internal static Fin<PerceptualColor> Shade(System.Drawing.Color color, Op key) =>
        PerceptualColor.OfRgb(color.R, color.G, color.B, alpha: color.A, key: key);

    internal static System.Drawing.Color Rgb(PerceptualColor shade) =>
        shade.ToRgb() switch {
            var (red, green, blue, alpha) => System.Drawing.Color.FromArgb(alpha: alpha, red: red, green: green, blue: blue),
        };
}
```

## [03]-[ACCEPTANCE]

`AcceptGate` rows carry every parameterless native accept call beside its result terminal, so acceptance grows by one row, never a new case. `AcceptRule` closes the gated, numeric, transparency, and wait modalities; each rule family derives its one-row-per-slot admission from `SlotKey`, the case identity a parameterized case overrides with its row value. Wait duration and option-cycle bounds are admitted once; no getter receives a raw flag bag. `Requiring` is the derivation seam: a prompt terminal's required row lands only into an unoccupied slot, so a caller's `AcceptRule.Number(Zero: false)` survives admission and the derived `Zero: true` is a default, never an override.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class AcceptGate {
    public static readonly AcceptGate Nothing = new(key: 0, terminal: None, enable: static target => target.AcceptNothing(enable: true));
    public static readonly AcceptGate Undo = new(key: 1, terminal: None, enable: static target => target.AcceptUndo(enable: true));
    public static readonly AcceptGate Enter = new(key: 2, terminal: None, enable: static target => target.AcceptEnterWhenDone(enable: true));
    public static readonly AcceptGate Point = new(key: 3, terminal: Some(GetResult.Point), enable: static target => target.AcceptPoint(enable: true));
    public static readonly AcceptGate Color = new(key: 4, terminal: Some(GetResult.Color), enable: static target => target.AcceptColor(enable: true));
    public static readonly AcceptGate Text = new(key: 5, terminal: Some(GetResult.String), enable: static target => target.AcceptString(enable: true));

    public Option<GetResult> Terminal { get; }

    [UseDelegateFromConstructor]
    internal partial void Enable(GetBaseClass getter);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcceptRule : ISlotted {
    private AcceptRule() { }
    public sealed record Allowed(AcceptGate Gate) : AcceptRule { public override object SlotKey => Gate; }
    public sealed record Number(bool Zero) : AcceptRule;
    public sealed record Transparent(bool Enabled) : AcceptRule;
    public sealed record WaitFor(TimeSpan Duration) : AcceptRule;

    public virtual object SlotKey => GetType();

    internal Option<GetResult> Terminal => Switch(
        allowed: static rule => rule.Gate.Terminal,
        number: static _ => Some(GetResult.Number),
        transparent: static _ => Option<GetResult>.None,
        waitFor: static _ => Option<GetResult>.None);

    internal Fin<Unit> Apply(GetBaseClass getter, Op key) => key.Catch(() => {
        Switch(
            state: getter,
            allowed: static (target, rule) => { rule.Gate.Enable(target); return unit; },
            number: static (target, rule) => { target.AcceptNumber(enable: true, acceptZero: rule.Zero); return unit; },
            transparent: static (target, rule) => { target.EnableTransparentCommands(enable: rule.Enabled); return unit; },
            waitFor: static (target, rule) => {
                int milliseconds = checked((int)Math.Ceiling(rule.Duration.TotalMilliseconds));
                target.SetWaitDuration(milliseconds: milliseconds);
                return unit;
            });
        return Fin.Succ(unit);
    });
}

[ComplexValueObject]
public sealed partial class AcceptPlan {
    public Seq<AcceptRule> Rules { get; }
    public int OptionBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<AcceptRule> rules,
        ref int optionBudget) {
        bool waitInvalid = rules.Exists(static rule => rule is AcceptRule.WaitFor wait
            && (wait.Duration <= TimeSpan.Zero || wait.Duration.TotalMilliseconds > int.MaxValue));
        validationError = rules.Exists(static rule => rule is null || rule is AcceptRule.Allowed { Gate: null })
            || optionBudget < 0 || optionBudget > 4096
            || !rules.OnePer()
            || waitInvalid
            ? new ValidationError(message: "accept plan is invalid")
            : validationError;
    }

    internal bool AcceptsNothing => Rules.Exists(static rule => rule is AcceptRule.Allowed allowed && allowed.Gate == AcceptGate.Nothing);

    internal Fin<AcceptPlan> Requiring(Seq<GetResult> terminals, Op key) {
        Seq<AcceptRule> missing = terminals
            .Choose(Derived)
            .Distinct()
            .Filter(row => !Rules.Exists(held => held.SlotKey.Equals(row.SlotKey)));
        return missing.IsEmpty
            ? Fin.Succ(value: this)
            : key.AcceptValidated<AcceptPlan>(
                fault: Validate(Rules + missing, OptionBudget, out AcceptPlan? widened),
                admitted: widened);
    }

    internal Fin<Unit> Apply(GetBaseClass getter, Op key) =>
        Rules.TraverseM(rule => rule.Apply(getter: getter, key: key)).As().Map(static _ => unit);

    private static Option<AcceptRule> Derived(GetResult terminal) => terminal switch {
        GetResult.Number => Some((AcceptRule)new AcceptRule.Number(Zero: true)),
        GetResult.String => Some((AcceptRule)new AcceptRule.Allowed(Gate: AcceptGate.Text)),
        GetResult.Color => Some((AcceptRule)new AcceptRule.Allowed(Gate: AcceptGate.Color)),
        _ => Option<AcceptRule>.None,
    };
}
```

## [04]-[POINT_ALGEBRA]

`PointConstraint` closes the native constraint family. `PointRule` parameterizes every independent point-getter setting as data — `PointGate` rows carry the boolean getter toggles and `SnapBarAxis` rows the curve-snap bars, so a new toggle is one row, never a new case — while `PointFeedback` carries rail-returning callbacks whose failures interrupt the native loop and surface after `Get` returns. `Pose` alone returns a value: its `Transform` re-poses the drag buffer through the host's own display-feedback call, moving the whole selection in one crossing.

The three pointer arms take `PointerFact` — world point, window point, viewport identity, and the `PointerKey` set — projected at the callback edge, so no `GetPointMouseEventArgs` reaches a caller sink and the arg's non-owning viewport wrapper dies with the crossing. The two DRAW arms keep their host args because a draw sink's whole purpose is the live `DisplayPipeline` the arg carries: the borrow is valid only inside the callback and the page states that rather than detaching a pipeline the sink cannot draw without.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointConstraint {
    private PointConstraint() { }
    public sealed record OnSegment(Point3d From, Point3d To) : PointConstraint;
    public sealed record OnLine(Line Value) : PointConstraint;
    public sealed record OnArc(Arc Value) : PointConstraint;
    public sealed record OnCircle(Circle Value) : PointConstraint;
    public sealed record OnPlane(Plane Value, bool AllowElevator) : PointConstraint;
    public sealed record OnSphere(Sphere Value) : PointConstraint;
    public sealed record OnCylinder(Cylinder Value) : PointConstraint;
    public sealed record OnCurve(Curve Value, bool AllowPickingOff) : PointConstraint;
    public sealed record OnSurface(Surface Value, bool AllowPickingOff) : PointConstraint;
    public sealed record OnBrep(Brep Value, int WireDensity, int FaceIndex, bool AllowPickingOff) : PointConstraint;
    public sealed record OnMesh(Mesh Value, bool AllowPickingOff) : PointConstraint;
    public sealed record OnConstructionPlane(bool ThroughBasePoint) : PointConstraint;
    public sealed record OnTargetPlane : PointConstraint;
    public sealed record OnCPlaneIntersection(Plane Value) : PointConstraint;

    internal Fin<Unit> Admit(Op key) => key.Catch(() => AdmitGeometry(key, Switch(
        onSegment: static row => row.From.IsValid && row.To.IsValid,
        onLine: static row => row.Value.IsValid,
        onArc: static row => row.Value.IsValid,
        onCircle: static row => row.Value.IsValid,
        onPlane: static row => row.Value.IsValid,
        onSphere: static row => row.Value.IsValid,
        onCylinder: static row => row.Value.IsValid,
        onCurve: static row => row.Value is { } value && value.IsValidWithLog(out _),
        onSurface: static row => row.Value is { } value && value.IsValidWithLog(out _),
        onBrep: static row => row.Value is { } value
            && value.IsValidWithLog(out _)
            && row.WireDensity >= 0
            && row.FaceIndex >= -1,
        onMesh: static row => row.Value is { } value && value.IsValidWithLog(out _),
        // The two cplane cases carry no geometry to admit — the host resolves the plane at `Apply` — so each
        // states its own admission instead of riding a catch-all a new payload-bearing case would join silently.
        onConstructionPlane: static _ => true,
        onTargetPlane: static _ => true,
        onCPlaneIntersection: static row => row.Value.IsValid)));

    internal static Fin<Unit> AdmitGeometry(Op key, params ReadOnlySpan<bool> validity) =>
        guard(flag: validity.IndexOf(false) < 0, False: key.InvalidInput()).ToFin();

    internal Fin<Unit> Apply(GetPoint getter, Op key) => key.Catch(() => Switch(
        state: (Getter: getter, Op: key),
        onSegment: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.From, rule.To)),
        onLine: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onArc: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onCircle: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onPlane: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.AllowElevator)),
        onSphere: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onCylinder: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value)),
        onCurve: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.AllowPickingOff)),
        onSurface: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.AllowPickingOff)),
        onBrep: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(
            rule.Value, rule.WireDensity, rule.FaceIndex, rule.AllowPickingOff)),
        onMesh: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.AllowPickingOff)),
        onConstructionPlane: static (held, rule) => held.Op.Confirm(
            held.Getter.ConstrainToConstructionPlane(rule.ThroughBasePoint)),
        onTargetPlane: static (held, _) => held.Op.Catch(() => {
            held.Getter.ConstrainToTargetPlane();
            return Fin.Succ(unit);
        }),
        onCPlaneIntersection: static (held, rule) => held.Op.Confirm(
            held.Getter.ConstrainToVirtualCPlaneIntersection(rule.Value))));
}

[SmartEnum<int>]
public sealed partial class PointGate {
    public static readonly PointGate ObjectSnapCursor = new(key: 0, set: static (getter, on) => getter.EnableObjectSnapCursors(on));
    public static readonly PointGate Ortho = new(key: 1, set: static (getter, on) => getter.PermitOrthoSnap(on));
    public static readonly PointGate ObjectSnap = new(key: 2, set: static (getter, on) => getter.PermitObjectSnap(on));
    public static readonly PointGate ConstraintOptions = new(key: 3, set: static (getter, on) => getter.PermitConstraintOptions(on));
    public static readonly PointGate FromOption = new(key: 4, set: static (getter, on) => getter.PermitFromOption(on));
    public static readonly PointGate TabMode = new(key: 5, set: static (getter, on) => getter.PermitTabMode(on));
    public static readonly PointGate Curves = new(key: 6, set: static (getter, on) => getter.EnableSnapToCurves(on));
    public static readonly PointGate ExitRedraw = new(key: 7, set: static (getter, on) => getter.EnableNoRedrawOnExit(on));
    public static readonly PointGate FullFrame = new(key: 8, set: static (getter, on) => getter.FullFrameRedrawDuringGet = on);

    [UseDelegateFromConstructor]
    internal partial void Set(GetPoint getter, bool enabled);
}

[SmartEnum<int>]
public sealed partial class PointerShape {
    public static readonly PointerShape Default = new(key: (int)global::Rhino.UI.CursorStyle.Default);
    public static readonly PointerShape Wait = new(key: (int)global::Rhino.UI.CursorStyle.Wait);
    public static readonly PointerShape CrossHair = new(key: (int)global::Rhino.UI.CursorStyle.CrossHair);
    public static readonly PointerShape Hand = new(key: (int)global::Rhino.UI.CursorStyle.Hand);
    public static readonly PointerShape Rotate = new(key: (int)global::Rhino.UI.CursorStyle.Rotate);
    public static readonly PointerShape Magnify = new(key: (int)global::Rhino.UI.CursorStyle.Magnify);
    public static readonly PointerShape ArrowCopy = new(key: (int)global::Rhino.UI.CursorStyle.ArrowCopy);
    public static readonly PointerShape CrosshairCopy = new(key: (int)global::Rhino.UI.CursorStyle.CrosshairCopy);

    internal global::Rhino.UI.CursorStyle Native => (global::Rhino.UI.CursorStyle)Key;
}

[SmartEnum<int>]
public sealed partial class SnapBarAxis {
    public static readonly SnapBarAxis Tangent = new(key: 0, set: static (getter, on, ends) => getter.EnableCurveSnapTangentBar(on, ends));
    public static readonly SnapBarAxis Perpendicular = new(key: 1, set: static (getter, on, ends) => getter.EnableCurveSnapPerpBar(on, ends));

    [UseDelegateFromConstructor]
    internal partial void Set(GetPoint getter, bool enabled, bool ends);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointRule : ISlotted {
    private PointRule() { }
    public sealed record Constrained(PointConstraint Value) : PointRule;
    public sealed record Snaps(Seq<Point3d> Values) : PointRule;
    public sealed record ConstructionPoints(Seq<Point3d> Values) : PointRule;
    public sealed record BasedAt(Point3d Value, bool ShowDistance, bool DrawLine) : PointRule;
    public sealed record Radial(double Distance) : PointRule;
    public sealed record Cursor(PointerShape Value) : PointRule;
    public sealed record ElevatorMode(int Mode) : PointRule;
    public sealed record Gated(PointGate Gate, bool Enabled) : PointRule { public override object SlotKey => Gate; }
    public sealed record SnapBar(SnapBarAxis Axis, bool Enabled, bool Ends) : PointRule { public override object SlotKey => Axis; }
    public sealed record DirectionArrow(bool Enabled, bool Reverse) : PointRule;
    public sealed record OnMouseUp : PointRule;

    public virtual object SlotKey => GetType();

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        constrained: static (op, rule) => op.Need(rule.Value).Bind(value => value.Admit(op)),
        snaps: static (op, rule) => guard(!rule.Values.IsEmpty, op.InvalidInput()).ToFin()
            .Bind(_ => PointConstraint.AdmitGeometry(op, [.. rule.Values.Map(static point => point.IsValid)])),
        constructionPoints: static (op, rule) => guard(!rule.Values.IsEmpty, op.InvalidInput()).ToFin()
            .Bind(_ => PointConstraint.AdmitGeometry(op, [.. rule.Values.Map(static point => point.IsValid)])),
        basedAt: static (op, rule) => PointConstraint.AdmitGeometry(op, rule.Value.IsValid),
        radial: static (op, rule) => guard(double.IsFinite(rule.Distance) && rule.Distance >= 0.0, op.InvalidInput()).ToFin(),
        // `PointerShape` is a sealed generated CLASS keyed on the host ordinal, not an enum — `Enum.IsDefined` has
        // no overload it binds — so membership is the vocabulary's OWN roster probe. A row is admitted when it is
        // one of the declared rows; nothing else can construct one, and a null is the only value to refuse.
        cursor: static (op, rule) => guard(
            rule.Value is not null && PointerShape.Items.Contains(rule.Value),
            op.InvalidInput()).ToFin(),
        elevatorMode: static (op, rule) => guard(rule.Mode >= 0, op.InvalidInput()).ToFin(),
        gated: static (op, rule) => guard(rule.Gate is not null, op.InvalidInput()).ToFin(),
        snapBar: static (op, rule) => guard(rule.Axis is not null, op.InvalidInput()).ToFin(),
        directionArrow: static (_, _) => Fin.Succ(unit),
        onMouseUp: static (_, _) => Fin.Succ(unit));

    internal Fin<Unit> Apply(GetPoint getter, Op key) => Switch(
        state: (Getter: getter, Op: key),
        constrained: static (held, rule) => rule.Value.Apply(held.Getter, held.Op),
        snaps: static (held, rule) => held.Op.Catch(() => Fin.Succ(ignore(
            held.Getter.AddSnapPoints(points: [.. rule.Values])))),
        constructionPoints: static (held, rule) => held.Op.Catch(() => Fin.Succ(ignore(
            held.Getter.AddConstructionPoints(points: [.. rule.Values])))),
        basedAt: static (held, rule) => held.Op.Catch(() => {
            held.Getter.SetBasePoint(rule.Value, rule.ShowDistance);
            held.Getter.EnableDrawLineFromPoint(rule.DrawLine);
            if (rule.DrawLine) held.Getter.DrawLineFromPoint(rule.Value, rule.ShowDistance);
            return Fin.Succ(unit);
        }),
        radial: static (held, rule) => held.Op.Catch(() => {
            held.Getter.ConstrainDistanceFromBasePoint(rule.Distance);
            return Fin.Succ(unit);
        }),
        cursor: static (held, rule) => held.Op.Catch(() => { held.Getter.SetCursor(rule.Value.Native); return Fin.Succ(unit); }),
        elevatorMode: static (held, rule) => held.Op.Catch(() => { held.Getter.PermitElevatorMode(rule.Mode); return Fin.Succ(unit); }),
        gated: static (held, rule) => held.Op.Catch(() => { rule.Gate.Set(held.Getter, rule.Enabled); return Fin.Succ(unit); }),
        snapBar: static (held, rule) => held.Op.Catch(() => { rule.Axis.Set(held.Getter, rule.Enabled, rule.Ends); return Fin.Succ(unit); }),
        directionArrow: static (held, rule) => held.Op.Catch(() => { held.Getter.EnableCurveSnapArrow(rule.Enabled, rule.Reverse); return Fin.Succ(unit); }),
        onMouseUp: static (_, _) => Fin.Succ(unit));
}

// The host publishes the pointer flag word as five independent bool reads and keeps its own `MK_*` masks private, so
// the mask is unreachable and the set is rebuilt from the reads. One row per axis restores it: a sink tests membership
// instead of carrying five columns, and a new host flag is one row.
[SmartEnum<int>]
public sealed partial class PointerKey {
    public static readonly PointerKey LeftButton = new(key: 0, read: static args => args.LeftButtonDown);
    public static readonly PointerKey MiddleButton = new(key: 1, read: static args => args.MiddleButtonDown);
    public static readonly PointerKey RightButton = new(key: 2, read: static args => args.RightButtonDown);
    public static readonly PointerKey Shift = new(key: 3, read: static args => args.ShiftKeyDown);
    public static readonly PointerKey Control = new(key: 4, read: static args => args.ControlKeyDown);

    [UseDelegateFromConstructor]
    internal partial bool Read(GetPointMouseEventArgs args);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointFeedback {
    private PointFeedback() { }
    public sealed record MouseMove(Func<PointerFact, Fin<Unit>> Sink) : PointFeedback;
    public sealed record MouseDown(Func<PointerFact, Fin<Unit>> Sink) : PointFeedback;
    public sealed record DynamicDraw(Func<GetPointDrawEventArgs, Fin<Unit>> Sink) : PointFeedback;
    public sealed record PostDraw(Func<DrawEventArgs, Fin<Unit>> Sink) : PointFeedback;
    public sealed record Pose(Func<PointerFact, Fin<Transform>> Sink) : PointFeedback;

    internal Fin<Unit> Admit(Op key) => Switch(
        mouseMove: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        mouseDown: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        dynamicDraw: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        postDraw: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        pose: row => guard(row.Sink is not null, key.InvalidInput()).ToFin());
}

// --- [MODELS] -----------------------------------------------------------------------------
// `GetPointMouseEventArgs.Viewport` mints a NON-OWNING `RhinoViewport` over the callback's native pointer and caches
// it on the arg, so the wrapper dies with the crossing and is never bracketed; `PointerFact` reads its identity once
// and detaches the whole arg, so no host event type reaches a caller-supplied sink.
public sealed record PointerFact(
    Point3d World,
    System.Drawing.Point Window,
    Guid Viewport,
    Seq<PointerKey> Keys) {

    internal static PointerFact Of(GetPointMouseEventArgs args) => new(
        World: args.Point,
        Window: args.WindowPoint,
        Viewport: args.Viewport.Id,
        Keys: toSeq(PointerKey.Items).Filter(row => row.Read(args)).Strict());
}

public sealed record PointPlan {
    private PointPlan(Seq<PointRule> rules, Seq<PointFeedback> feedback) {
        Rules = rules;
        Feedback = feedback;
    }

    public Seq<PointRule> Rules { get; }
    public Seq<PointFeedback> Feedback { get; }
    public static PointPlan Free { get; } = new(rules: [], feedback: []);

    public static Fin<PointPlan> Of(Seq<PointFeedback> feedback, params ReadOnlySpan<PointRule> rules) {
        PointPlan plan = new(rules: toSeq(rules.ToArray()), feedback: feedback);
        return plan.Admit(Op.Of(name: nameof(PointPlan))).Map(_ => plan);
    }

    internal bool OnMouseUp => Rules.Exists(static rule => rule is PointRule.OnMouseUp);

    internal Fin<Unit> Admit(Op op) {
        Seq<PointRule> singleton = Rules.Filter(static rule => rule is not null && rule is not PointRule.Constrained);
        return guard(
            Rules.ForAll(static rule => rule is not null)
            && Feedback.ForAll(static row => row is not null)
            && singleton.OnePer(),
            op.InvalidInput()).ToFin()
            .Bind(_ => Rules.TraverseM(rule => rule.Admit(op)).As().Map(static _ => unit))
            .Bind(_ => Feedback.TraverseM(row => row.Admit(op)).As().Map(static _ => unit));
    }
}
```

## [05]-[REQUEST]

`PromptCase` generates the interactive value space over one `GetPoint`; multiple distinct terminal cases compose number, text, color, 3D point, and 2D point acquisition without getter-specific helper classes. `Acquire.Of` admits each option, prompt default, typed default, drag plan, and accept terminal against the selected `AcquireIntent`, so no configured terminal outruns its projector, and it closes the loop the other way through `AcceptPlan.Requiring`: each prompt case's `Terminal` derives the accept row it needs and that row folds into the plan ONLY where the caller left the slot empty. Acceptance therefore reaches the native getter exactly once, through `AcceptPlan.Apply`. A second configuration pass re-issuing `AcceptNumber`/`AcceptString`/`AcceptColor` after the plan has run is the deleted form — it lands after the plan by construction, so its literals silently overwrite the caller's admitted policy and nothing raises.

`ObjectPlan`, `ModalInput`, `DragPlan`, and `AcquireIntent` close the remaining custom and one-shot routes; `ShapeAsk` rows carry the parameterless one-shot shape getters as data, so a new native shape is one row.

The modal object asks take `Document`'s `ObjectKinds`, never a raw `ObjectType` — the type vocabulary is one S0 owner this page and the S2 properties-page scope both compose, and the flag mask is spelled at the `RhinoGet` call alone through `Filter.Mask`. The view asks project `ViewportFact` at the same seam, so a live `RhinoView` or `RhinoViewport` never reaches a caller projector. `FileAsk` keys the host's sparse `GetFileNameMode` roster, and the file ask's `Option<string> Title` IS the route discriminant: a caption drives `GetFileName` against the host's own main window and its absence drives `GetFileNameScripted`, which needs no window at all — so the untyped `object parent` the host overload takes never reaches a signature.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TextMeaning {
    public static readonly TextMeaning Literal = new(key: 0, parse: static (text, _, _) =>
        Fin.Succ<Acquired>(new Acquired.Text(Value: text)));
    public static readonly TextMeaning Number = new(key: 1, parse: static (text, _, key) => key.Catch(() => {
        StringParserSettings output = StringParserSettings.ParseSettingsDoubleNumber;
        int consumed = StringParser.ParseNumber(
            text, 0, StringParserSettings.ParseSettingsDoubleNumber, ref output, out double value);
        return consumed == text.Length && double.IsFinite(value)
            ? Fin.Succ<Acquired>(new Acquired.Number(Value: value))
            : Fin.Fail<Acquired>(key.InvalidInput());
    }));
    // Length text crosses through `UnitText` (session.md), the branch's ONE length-correspondence owner: it holds
    // the dialect roster, the whole-string `parsedAll`/`IsUnset` gate, the `LengthValue` disposal bracket, and the
    // regime pairing this receipt detaches. Re-spelling that parse here forked the gate and re-derived the regime
    // from `document.ModelUnits` beside the owner that already reads it.
    public static readonly TextMeaning Length = new(key: 2, parse: static (text, document, key) =>
        from regime in DocumentSpace.Model.Read(document: document, op: key)
        from encoded in UnitText.Length(text: text, key: key)
        from crossed in encoded.Cross(regime: regime, key: key)
        from measured in crossed is UnitText.LengthValueCase value
            ? Fin.Succ<Acquired>(value: new Acquired.Distance(Value: value.Value, Unit: value.Unit))
            : Fin.Fail<Acquired>(error: key.InvalidResult())
        select measured);
    // `AngleGrammar` (session.md) owns the degree/radian dialect and lands canonical radians at its own seam, so
    // both rows are one composition each and the degrees row cannot drift from the owner's `RhinoMath.ToRadians`.
    public static readonly TextMeaning AngleDegrees = new(key: 3, parse: static (text, _, key) =>
        AngleGrammar.Degrees.Parse(text: text, op: key)
            .Map(static radians => (Acquired)new Acquired.Angle(Radians: radians)));
    public static readonly TextMeaning AngleRadians = new(key: 4, parse: static (text, _, key) =>
        AngleGrammar.Radians.Parse(text: text, op: key)
            .Map(static radians => (Acquired)new Acquired.Angle(Radians: radians)));

    [UseDelegateFromConstructor]
    public partial Fin<Acquired> Parse(string text, RhinoDoc document, Op key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PromptCase {
    private PromptCase() { }
    public sealed record Point3 : PromptCase;
    public sealed record Point2 : PromptCase;
    public sealed record NumberValue(NumericBand<double> Band) : PromptCase;
    public sealed record CountValue(NumericBand<int> Band) : PromptCase;
    public sealed record TextValue(TextMeaning Meaning) : PromptCase;
    public sealed record PaintValue : PromptCase;

    public GetResult Terminal => Switch(
        point3: static _ => GetResult.Point,
        point2: static _ => GetResult.Point2d,
        numberValue: static _ => GetResult.Number,
        countValue: static _ => GetResult.Number,
        textValue: static _ => GetResult.String,
        paintValue: static _ => GetResult.Color);

    internal Fin<Unit> Admit(Op key) => guard(this is not TextValue { Meaning: null }, key.InvalidInput()).ToFin();

    internal bool Accepts(InputDefault value) => (this, value) switch {
        (Point3, InputDefault.PointValue) => true,
        (NumberValue rule, InputDefault.NumberValue value) => rule.Band.Contains(value.Value),
        (CountValue rule, InputDefault.CountValue value) => rule.Band.Contains(value.Value),
        (TextValue, InputDefault.TextValue) => true,
        (PaintValue, InputDefault.PaintValue) => true,
        _ => false,
    };

    internal Fin<Acquired> Project(GetPoint getter, RhinoDoc document, Op key) => Switch(
        state: (Getter: getter, Document: document, Op: key),
        point3: static (held, _) => {
            Point3d value = held.Getter.Point();
            return Fin.Succ<Acquired>(new Acquired.Point(
                Value: value,
                Evidence: new PointEvidence(
                    ViewSerial: Optional(held.Getter.View()).Map(static view => view.RuntimeSerialNumber),
                    OsnapCode: Some(Convert.ToInt32(held.Getter.OsnapEventType, CultureInfo.InvariantCulture)),
                    BasePoint: held.Getter.TryGetBasePoint(basePoint: out Point3d anchor) ? Some(anchor) : None,
                    SnapPoints: toSeq(held.Getter.GetSnapPoints()),
                    ConstructionPoints: toSeq(held.Getter.GetConstructionPoints()))));
        },
        point2: static (held, _) => Fin.Succ<Acquired>(new Acquired.ScreenPoint(Value: held.Getter.Point2d())),
        numberValue: static (held, rule) => held.Getter.Number() is var value && rule.Band.Contains(value)
            ? Fin.Succ<Acquired>(new Acquired.Number(Value: value))
            : Fin.Fail<Acquired>(held.Op.InvalidInput()),
        countValue: static (held, rule) => held.Getter.Number() is var raw
            && raw == Math.Truncate(raw) && raw >= int.MinValue && raw <= int.MaxValue && rule.Band.Contains((int)raw)
            ? Fin.Succ<Acquired>(new Acquired.Count(Value: (int)raw))
            : Fin.Fail<Acquired>(held.Op.InvalidInput()),
        textValue: static (held, rule) => rule.Meaning.Parse(
            text: held.Getter.StringResult(), document: held.Document, key: held.Op),
        paintValue: static (held, _) => Slots.Shade(color: held.Getter.Color(), key: held.Op)
            .Map(static shade => (Acquired)new Acquired.Paint(Value: shade)));
}

[SmartEnum<int>]
public sealed partial class ObjectGate {
    public static readonly ObjectGate PostSelect = new(key: 0, set: static (getter, on) => getter.EnablePostSelect(on));
    public static readonly ObjectGate Previous = new(key: 1, set: static (getter, on) => getter.EnableSelPrevious(on));
    public static readonly ObjectGate Highlight = new(key: 2, set: static (getter, on) => getter.EnableHighlight(on));
    public static readonly ObjectGate IgnoreGrips = new(key: 3, set: static (getter, on) => getter.EnableIgnoreGrips(on));
    public static readonly ObjectGate EnterPrompt = new(key: 4, set: static (getter, on) => getter.EnablePressEnterWhenDonePrompt(on));

    [UseDelegateFromConstructor]
    internal partial void Set(GetObject getter, bool enabled);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectRule : ISlotted {
    private ObjectRule() { }
    public sealed record PreSelect(bool Enabled, bool IgnoreUnacceptable) : ObjectRule;
    public sealed record Gated(ObjectGate Gate, bool Enabled) : ObjectRule { public override object SlotKey => Gate; }
    public sealed record Filter(GetObjectGeometryFilter Value) : ObjectRule;

    public virtual object SlotKey => GetType();

    internal Fin<Unit> Apply(GetObject getter, Op key) => key.Catch(() => {
        Switch(
            state: getter,
            preSelect: static (target, rule) => { target.EnablePreSelect(rule.Enabled, rule.IgnoreUnacceptable); return unit; },
            gated: static (target, rule) => { rule.Gate.Set(target, rule.Enabled); return unit; },
            filter: static (target, rule) => { target.SetCustomGeometryFilter(rule.Value); return unit; });
        return Fin.Succ(unit);
    });
}

[ComplexValueObject]
public sealed partial class ObjectPlan {
    public int Minimum { get; }
    public int Maximum { get; }
    public Seq<ObjectRule> Rules { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int minimum,
        ref int maximum,
        ref Seq<ObjectRule> rules) {
        validationError = rules.Exists(static rule => rule is null
                || rule is ObjectRule.Filter { Value: null }
                || rule is ObjectRule.Gated { Gate: null })
            || minimum < 0
            || maximum < 0
            || maximum is not 0 && maximum < minimum
            || !rules.OnePer()
            ? new ValidationError(message: "object plan is invalid")
            : validationError;
    }
}

[SmartEnum<int>]
public sealed partial class DragScope {
    public static readonly DragScope ObjectsOnly = new(key: 0, grips: false);
    public static readonly DragScope ObjectsAndGrips = new(key: 1, grips: true);

    public bool Grips { get; }
}

[ComplexValueObject]
public sealed partial class DragPlan {
    public string Prompt { get; }
    public ObjectPlan Selection { get; }
    public DragScope Scope { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string prompt,
        ref ObjectPlan selection,
        ref DragScope scope) =>
        validationError = !string.IsNullOrWhiteSpace(prompt) && selection is not null && scope is not null && selection.Minimum >= 1
            ? validationError
            : new ValidationError(message: "drag plan needs a prompt, a scope, and a selection admitting at least one object");
}

[SmartEnum<int>]
public sealed partial class ShapeAsk {
    public static readonly ShapeAsk Segment = new(key: 0, run: static () =>
        Projected(RhinoGet.GetLine(out Line value), () => new Acquired.Segment(Value: value)));
    public static readonly ShapeAsk Chain = new(key: 1, run: static () =>
        Projected(RhinoGet.GetPolyline(out Polyline value), () => new Acquired.Chain(Value: value)));
    public static readonly ShapeAsk ArcShape = new(key: 2, run: static () =>
        Projected(RhinoGet.GetArc(out Arc value), () => new Acquired.ArcShape(Value: value)));
    public static readonly ShapeAsk CircleShape = new(key: 3, run: static () =>
        Projected(RhinoGet.GetCircle(out Circle value), () => new Acquired.CircleShape(Value: value)));
    public static readonly ShapeAsk PlaneShape = new(key: 4, run: static () =>
        Projected(RhinoGet.GetPlane(out Plane value), () => new Acquired.PlaneShape(Value: value)));
    public static readonly ShapeAsk RectangleShape = new(key: 5, run: static () =>
        Projected(RhinoGet.GetRectangle(out Point3d[] value), () => new Acquired.RectangleShape(Corners: toArr(value))));
    public static readonly ShapeAsk BoxShape = new(key: 6, run: static () =>
        Projected(RhinoGet.GetBox(out Box value), () => new Acquired.BoxShape(Value: value)));

    [UseDelegateFromConstructor]
    internal partial (Result Native, Func<Fin<Acquired>> Project) Run();

    private static (Result Native, Func<Fin<Acquired>> Project) Projected(Result native, Func<Acquired> wrap) =>
        (native, () => Fin.Succ(wrap()));
}

// `Rhino.Input.Custom.GetFileNameMode` — the host's own file-ask roster, sparse by construction (ordinals 4, 15, and 16
// carry no row), so the keyed wrap mirrors the live ordinals and an unlisted ordinal refuses at admission.
[SmartEnum<GetFileNameMode>]
public sealed partial class FileAsk {
    public static readonly FileAsk Open = new(key: GetFileNameMode.Open);
    public static readonly FileAsk OpenTemplate = new(key: GetFileNameMode.OpenTemplate);
    public static readonly FileAsk OpenImage = new(key: GetFileNameMode.OpenImage);
    public static readonly FileAsk OpenRhinoOnly = new(key: GetFileNameMode.OpenRhinoOnly);
    public static readonly FileAsk OpenTextFile = new(key: GetFileNameMode.OpenTextFile);
    public static readonly FileAsk OpenWorksession = new(key: GetFileNameMode.OpenWorksession);
    public static readonly FileAsk Import = new(key: GetFileNameMode.Import);
    public static readonly FileAsk Attach = new(key: GetFileNameMode.Attach);
    public static readonly FileAsk LoadPlugIn = new(key: GetFileNameMode.LoadPlugIn);
    public static readonly FileAsk Save = new(key: GetFileNameMode.Save);
    public static readonly FileAsk SaveSmall = new(key: GetFileNameMode.SaveSmall);
    public static readonly FileAsk SaveTemplate = new(key: GetFileNameMode.SaveTemplate);
    public static readonly FileAsk SaveImage = new(key: GetFileNameMode.SaveImage);
    public static readonly FileAsk Export = new(key: GetFileNameMode.Export);
    public static readonly FileAsk SaveTextFile = new(key: GetFileNameMode.SaveTextFile);
    public static readonly FileAsk SaveWorksession = new(key: GetFileNameMode.SaveWorksession);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModalInput {
    private ModalInput() { }
    public sealed record Point : ModalInput;
    public sealed record OneObject(ObjectKinds Filter) : ModalInput;
    public sealed record ManyObjects(ObjectKinds Filter) : ModalInput;
    public sealed record Text(string Seed) : ModalInput;
    public sealed record Toggle(string Off, string On, bool Seed) : ModalInput;
    public sealed record Number(double Seed, double Lower, double Upper) : ModalInput;
    public sealed record Count(int Seed, int Lower, int Upper) : ModalInput;
    public sealed record Paint(PerceptualColor Seed) : ModalInput;
    public sealed record Distance(double Seed) : ModalInput;
    public sealed record Shape(ShapeAsk Ask) : ModalInput;
    public sealed record View(Func<ViewportFact, Fin<Acquired>> Project) : ModalInput;
    public sealed record Viewports(Func<Seq<ViewportFact>, Fin<Acquired>> Project) : ModalInput;
    public sealed record File(FileAsk Ask, string DefaultName, Option<string> Title) : ModalInput;

    internal Fin<Unit> Admit(Op key) => Switch(
        point: static _ => Fin.Succ(unit),
        oneObject: row => Optional(row.Filter).Map(static _ => unit).ToFin(Fail: key.InvalidInput()),
        manyObjects: row => Optional(row.Filter).Map(static _ => unit).ToFin(Fail: key.InvalidInput()),
        text: row => Optional(row.Seed).Map(static _ => unit).ToFin(Fail: key.InvalidInput()),
        toggle: row => from _ in key.AcceptText(row.Off)
                       from __ in key.AcceptText(row.On)
                       from ___ in guard(!string.Equals(row.Off, row.On, StringComparison.OrdinalIgnoreCase), key.InvalidInput())
                       select unit,
        number: row => guard(double.IsFinite(row.Seed)
            && double.IsFinite(row.Lower)
            && double.IsFinite(row.Upper)
            && row.Lower <= row.Seed
            && row.Seed <= row.Upper, key.InvalidInput()).ToFin(),
        count: row => guard(row.Lower <= row.Seed && row.Seed <= row.Upper, key.InvalidInput()).ToFin(),
        paint: row => key.Need(row.Seed).Map(static _ => unit),
        distance: row => guard(double.IsFinite(row.Seed) && row.Seed >= 0.0, key.InvalidInput()).ToFin(),
        shape: row => guard(row.Ask is not null, key.InvalidInput()).ToFin(),
        view: row => guard(row.Project is not null, key.InvalidInput()).ToFin(),
        viewports: row => guard(row.Project is not null, key.InvalidInput()).ToFin(),
        file: row => from _ in guard(row.Ask is not null && row.DefaultName is not null, key.InvalidInput()).ToFin()
                     from __ in row.Title.Traverse(caption => key.AcceptText(caption)).As()
                     select unit);

    internal bool Accepts(AcceptRule rule) =>
        rule is AcceptRule.Allowed allowed && allowed.Gate == AcceptGate.Nothing
        && this is Point or OneObject or ManyObjects or Text or Toggle or Number or Count or Paint;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcquireIntent {
    private AcquireIntent() { }
    public sealed record Interactive(Seq<PromptCase> Cases, PointPlan Point) : AcquireIntent;
    public sealed record Objects(ObjectPlan Plan) : AcquireIntent;
    public sealed record Transform(Func<RhinoViewport, Point3d, Transform> Calculate) : AcquireIntent;
    public sealed record Modal(ModalInput Input) : AcquireIntent;

    internal bool SupportsOptions => this is not Modal;

    internal bool SupportsPromptDefault => SupportsOptions;

    internal bool SupportsDrag => this is Interactive or Transform;

    internal bool NeedsDrag =>
        this is Interactive row && row.Point.Feedback.Exists(static feed => feed is PointFeedback.Pose);

    internal bool Accepts(InputDefault value) =>
        this is Interactive row && row.Cases.Exists(prompt => prompt.Accepts(value));

    internal Seq<GetResult> Terminals => Switch(
        interactive: static row => row.Cases.Map(static prompt => prompt.Terminal).Distinct(),
        objects: static _ => Seq<GetResult>(),
        transform: static _ => Seq<GetResult>(),
        modal: static _ => Seq<GetResult>());

    internal bool Accepts(AcceptRule rule) => Switch(
        state: rule,
        interactive: static (accept, row) => accept.Terminal.ForAll(
            terminal => row.Cases.Exists(prompt => prompt.Terminal == terminal)),
        objects: static (accept, _) => accept.Terminal.IsNone,
        transform: static (accept, _) => accept.Terminal.IsNone,
        modal: static (accept, row) => row.Input.Accepts(accept));

    internal Fin<Unit> Admit(Op key) => Switch(
        interactive: row => from _ in guard(!row.Cases.IsEmpty
                               && row.Cases.ForAll(static value => value is not null)
                               && row.Cases.Map(static value => value.Terminal).Distinct().Count == row.Cases.Count,
                               key.InvalidInput()).ToFin()
                            from __ in guard(row.Point is not null, key.InvalidInput()).ToFin()
                            from ___ in row.Point.Admit(key)
                            from ____ in row.Cases.TraverseM(value => value.Admit(key)).As()
                            select unit,
        objects: row => guard(row.Plan is not null, key.InvalidInput()).ToFin(),
        transform: row => guard(row.Calculate is not null, key.InvalidInput()).ToFin(),
        modal: row => key.Need(row.Input).Bind(value => value.Admit(key)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InputDefault {
    private InputDefault() { }
    public sealed record PointValue(Point3d Value) : InputDefault;
    public sealed record NumberValue(double Value) : InputDefault;
    public sealed record CountValue(int Value) : InputDefault;
    public sealed record TextValue(string Value) : InputDefault;
    public sealed record PaintValue(PerceptualColor Value) : InputDefault;

    internal Fin<Unit> Admit(Op key) => Switch(
        pointValue: static _ => Fin.Succ(unit),
        numberValue: row => guard(double.IsFinite(row.Value), key.InvalidInput()).ToFin(),
        countValue: static _ => Fin.Succ(unit),
        textValue: row => key.AcceptText(row.Value).Map(static _ => unit),
        paintValue: row => key.Need(row.Value).Map(static _ => unit));

    internal Unit Apply(GetBaseClass getter) => Switch(
        state: getter,
        pointValue: static (target, value) => Op.Side(() => target.SetDefaultPoint(value.Value)),
        numberValue: static (target, value) => Op.Side(() => target.SetDefaultNumber(value.Value)),
        countValue: static (target, value) => Op.Side(() => target.SetDefaultInteger(value.Value)),
        textValue: static (target, value) => Op.Side(() => target.SetDefaultString(value.Value)),
        paintValue: static (target, value) => Op.Side(() => target.SetDefaultColor(Slots.Rgb(shade: value.Value))));
}

// --- [MODELS] -----------------------------------------------------------------------------
// A modal view ask hands back live host views and viewports whose lifetime is the getter call; identity, name, and the
// owning view serial detach at that seam, so a projector composes evidence and no `RhinoView`/`RhinoViewport` escapes.
// A detail viewport has no parent view, hence the optional serial.
public sealed record ViewportFact(Guid Id, string Name, Option<uint> ViewSerial) {
    internal static ViewportFact Of(RhinoViewport viewport) => new(
        Id: viewport.Id,
        Name: viewport.Name,
        ViewSerial: Optional(viewport.ParentView).Map(static view => view.RuntimeSerialNumber));

    internal static ViewportFact Of(RhinoView view) => Of(view.MainViewport);
}

public sealed record Acquire {
    private Acquire(
        AcquireIntent intent,
        string prompt,
        AcceptPlan accept,
        Option<string> promptDefault,
        Option<InputDefault> @default,
        Option<OptionSet> options,
        Option<DragPlan> drag) {
        Intent = intent;
        Prompt = prompt;
        Accept = accept;
        PromptDefault = promptDefault;
        Default = @default;
        Options = options;
        Drag = drag;
    }

    public AcquireIntent Intent { get; }
    public string Prompt { get; }
    public AcceptPlan Accept { get; }
    public Option<string> PromptDefault { get; }
    public Option<InputDefault> Default { get; }
    public Option<OptionSet> Options { get; }
    public Option<DragPlan> Drag { get; }

    public static Fin<Acquire> Of(
        AcquireIntent intent,
        string prompt,
        AcceptPlan accept,
        Option<string> promptDefault = default,
        Option<InputDefault> @default = default,
        Option<OptionSet> options = default,
        Option<DragPlan> drag = default) {
        Op op = Op.Of(name: nameof(Acquire));
        return from admittedIntent in op.Need(intent)
               from admittedAccept in op.Need(accept)
               from admittedPrompt in op.AcceptText(prompt)
               from _ in admittedIntent.Admit(op)
               from __ in guard(options.IsNone || admittedIntent.SupportsOptions, op.InvalidInput()).ToFin()
               from ___ in guard(promptDefault.IsNone || admittedIntent.SupportsPromptDefault, op.InvalidInput()).ToFin()
               from ____ in guard(
                   (drag.IsNone || admittedIntent.SupportsDrag) && (!admittedIntent.NeedsDrag || drag.IsSome),
                   op.InvalidInput()).ToFin()
               from _____ in guard(admittedAccept.Rules.ForAll(rule => admittedIntent.Accepts(rule)), op.InvalidInput()).ToFin()
               from ______ in promptDefault.Match(
                   Some: value => op.AcceptText(value).Map(static _ => unit),
                   None: static () => Fin.Succ(unit))
               from _______ in @default.Match(
                   Some: value => value.Admit(op).Bind(_ => guard(admittedIntent.Accepts(value), op.InvalidInput()).ToFin()),
                   None: static () => Fin.Succ(unit))
               from complete in admittedAccept.Requiring(terminals: admittedIntent.Terminals, key: op)
               select new Acquire(
                   admittedIntent, admittedPrompt, complete, promptDefault, @default, options, drag);
    }
}
```

## [06]-[DRIVE]

`Acquisition.Probe` projects the four host getter-state probes inside one read grant. `GetterDrive.Run` owns one getter and option lease. One bounded `FoldM` consumes option terminals, then projects exactly one final discriminant. Modal payloads remain deferred until `Result.Success`, so failed one-shot calls never read uninitialized `out` values.

- Law: a mixed object-and-grip selection destined for a dynamic transform lands in the HOST's own buffer — `DragBuffer.Of` folds a completed `GetObject` into `TransformObjectList.AddObjects(getter, scope.Grips)`, so `ObjectArray`, `GripArray`, `GripOwnerArray`, the `Count`/`GripCount`/`GripOwnerCount` census, and `GetBoundingBox(regularObjects, grips)` all read one drag truth; a per-object re-projection loses the grip-to-owner correspondence the buffer alone carries.
- Law: the drag set is a request column, never a fifth intent — `Acquire.Drag` rides beside `Options` and `Default` under the `SupportsDrag` gate, so the point drive and the transform drive admit one plan through one gate, a `GetObject` or one-shot request carrying a drag plan refuses at `Acquire.Of`, and a `PointFeedback.Pose` row without a plan refuses on the same line through `NeedsDrag`.
- Law: the two drives consume the buffer differently and the buffer never learns which — `GetTransform` takes it through `AddTransformObjects` and the native getter paints its own feedback, while a `GetPoint` drag drives `UpdateDisplayFeedbackTransform(Transform)` per `PointFeedback.Pose` sample under `DisplayFeedbackEnabled`, re-posing the whole dragged set in one host call instead of one per selected object, which is the hand-roll the geometry catalog's reject clause names.
- Law: `TransformObjectList` is `IDisposable` and dies inside `GetterDrive.Run`'s bracket — `DragEvidence` is read off the live buffer at seal time and is the only drag fact reaching `AcquiredReceipt`, so the `Poses` count is a measured tally of applied feedback samples and `Clear` stays scope-internal.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
internal sealed record GetterCycle(
    Seq<OptionChoice> Choices,
    Option<GetResult> Terminal);

public sealed record AcquireState(
    bool Any,
    bool Point,
    bool Object,
    bool DocumentPoint);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Acquisition {
    public static Fin<AcquireState> Probe(DocumentSession session) {
        Op op = Op.Of();
        return from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from target in op.Need(session)
               from state in target.Demand(
                   use: document => op.Catch(() => Fin.Succ(new AcquireState(
                       Any: RhinoGet.InGet(document),
                       Point: RhinoGet.InGetPoint(document),
                       Object: RhinoGet.InGetObject(document),
                       DocumentPoint: document.InGetPoint))),
                   key: op,
                   needs: [SessionNeed.Read])
               select state;
    }

    public static Fin<AcquiredReceipt> Get(DocumentSession session, Acquire request) {
        Op op = Op.Of();
        return from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from target in op.Need(session)
               from active in op.Need(request)
               from receipt in target.Demand(
                   use: document => active.Intent.Switch(
                       state: (Request: active, Document: document, Op: op),
                       interactive: static (held, intent) => Interactive(held.Request, intent, held.Document, held.Op),
                       objects: static (held, intent) => Objects(held.Request, intent.Plan, held.Op),
                       transform: static (held, intent) => Transform(held.Request, intent.Calculate, held.Op),
                       modal: static (held, intent) => Modal(held.Request, intent.Input, held.Document, held.Op)),
                   key: op,
                   needs: [SessionNeed.Acquire])
               select receipt;
    }

    private static Fin<AcquiredReceipt> Interactive(
        Acquire request,
        AcquireIntent.Interactive intent,
        RhinoDoc document,
        Op op) =>
        from receipt in GetterDrive.Run(
            request: request,
            create: static () => new GetPoint(),
            prepare: getter => intent.Point.Rules.TraverseM(rule => rule.Apply(getter, op)).As().Map(static _ => unit),
            receive: (getter, dragging) => PointFeedbackLease.Attach(getter, intent.Point.Feedback, dragging, op).Bind(callbacks => {
                GetResult raw;
                using (callbacks) {
                    raw = getter.Get(
                        onMouseUp: intent.Point.OnMouseUp,
                        get2DPoint: intent.Cases.Exists(static row => row.Terminal is GetResult.Point2d));
                }
                return callbacks.Fault.Match(
                    Some: Fin.Fail<GetResult>,
                    None: () => Fin.Succ(raw));
            }),
            project: (getter, raw) => intent.Cases.Find(row => row.Terminal == raw)
                .ToFin(Fail: op.InvalidResult(detail: raw.ToString()))
                .Bind(row => row.Project(getter, document, op)),
            op: op)
        select receipt;

    private static Fin<AcquiredReceipt> Objects(Acquire request, ObjectPlan plan, Op op) => GetterDrive.Run(
        request: request,
        create: static () => new GetObject(),
        prepare: getter => plan.Rules.TraverseM(rule => rule.Apply(getter, op)).As().Map(static _ => unit),
        receive: (getter, _) => op.Catch(() => Fin.Succ(getter.GetMultiple(plan.Minimum, plan.Maximum))),
        project: (getter, raw) => raw is GetResult.Object
            ? Picks.CaptureOwned(references: getter.Objects(), key: op)
                .Map(static picks => (Acquired)new Acquired.Objects(Picks: picks))
            : Fin.Fail<Acquired>(op.InvalidResult(detail: raw.ToString())),
        op: op);

    private static Fin<AcquiredReceipt> Transform(
        Acquire request,
        Func<RhinoViewport, Point3d, Transform> calculate,
        Op op) => GetterDrive.Run(
        request: request,
        create: () => new TransformGetter(calculate),
        prepare: static _ => Fin.Succ(unit),
        receive: (getter, _) => op.Catch(() => Fin.Succ(getter.GetXform())),
        project: (getter, raw) => getter.Fault.Match(
            Some: Fin.Fail<Acquired>,
            None: () => getter.Calculated
                .Map(static value => (Acquired)new Acquired.Xform(Value: value))
                .ToFin(Fail: op.InvalidResult(detail: raw.ToString()))),
        op: op);

    private static Fin<AcquiredReceipt> Modal(
        Acquire request,
        ModalInput input,
        RhinoDoc document,
        Op op) => input.Switch(
        state: (Request: request, Document: document, Op: op),
        point: static (held, _) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetPoint(held.Request.Prompt, held.Request.Accept.AcceptsNothing, out Point3d value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Point(
                Value: value,
                Evidence: new PointEvidence(None, None, None, [], []))));
        }),
        oneObject: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetOneObject(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Filter.Mask, out ObjRef reference);
            return (native, () => Picks.CaptureOwned([reference], held.Op)
                .Map(static picks => (Acquired)new Acquired.Objects(Picks: picks)));
        }),
        manyObjects: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetMultipleObjects(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Filter.Mask, out ObjRef[] references);
            return (native, () => Picks.CaptureOwned(references, held.Op)
                .Map(static picks => (Acquired)new Acquired.Objects(Picks: picks)));
        }),
        text: static (held, modal) => ModalResult(held.Op, () => {
            string value = modal.Seed;
            Result native = RhinoGet.GetString(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Text(Value: value)));
        }),
        toggle: static (held, modal) => ModalResult(held.Op, () => {
            bool value = modal.Seed;
            Result native = RhinoGet.GetBool(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Off, modal.On, ref value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Toggle(Value: value)));
        }),
        number: static (held, modal) => ModalResult(held.Op, () => {
            double value = modal.Seed;
            Result native = RhinoGet.GetNumber(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value, modal.Lower, modal.Upper);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Number(Value: value)));
        }),
        count: static (held, modal) => ModalResult(held.Op, () => {
            int value = modal.Seed;
            Result native = RhinoGet.GetInteger(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value, modal.Lower, modal.Upper);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Count(Value: value)));
        }),
        paint: static (held, modal) => ModalResult(held.Op, () => {
            System.Drawing.Color value = Slots.Rgb(shade: modal.Seed);
            Result native = RhinoGet.GetColor(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value);
            return (native, () => Slots.Shade(color: value, key: held.Op)
                .Map(static shade => (Acquired)new Acquired.Paint(Value: shade)));
        }),
        // GetDistance resolves in the document's own regime, so the projection reads that regime through the same
        // `DocumentSpace.Model` owner the text route reads — the modal route detaches the identical pairing.
        distance: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetDistance(held.Request.Prompt, modal.Seed, out double value);
            return (native, () => DocumentSpace.Model.Read(document: held.Document, op: held.Op)
                .Map(regime => (Acquired)new Acquired.Distance(Value: value, Unit: regime.Unit)));
        }),
        shape: static (held, modal) => ModalResult(held.Op, modal.Ask.Run),
        view: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetView(held.Request.Prompt, out RhinoView value);
            return (native, () => modal.Project(ViewportFact.Of(value)));
        }),
        viewports: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetViewports(held.Request.Prompt, out RhinoViewport[] value);
            return (native, () => modal.Project(toSeq(value).Map(static row => ViewportFact.Of(row)).Strict()));
        }),
        // Title presence IS the route: a caption drives the native dialog, its absence the command-line ask.
        file: static (held, modal) => held.Op.Catch(() => {
            string value = modal.Title.Match(
                Some: caption => RhinoGet.GetFileName(modal.Ask.Key, modal.DefaultName, caption, parent: null),
                None: () => RhinoGet.GetFileNameScripted(modal.Ask.Key, modal.DefaultName));
            return string.IsNullOrWhiteSpace(value)
                ? Fin.Succ(Receipt(new AcquireTerminal.Cancelled()))
                : Fin.Succ(Receipt(new AcquireTerminal.Value(new Acquired.FileName(Value: value))));
        }));

    private static Fin<AcquiredReceipt> ModalResult(
        Op op,
        Func<(Result Native, Func<Fin<Acquired>> Project)> run) => op.Catch(() => {
        (Result native, Func<Fin<Acquired>> project) = run();
        return native switch {
            Result.Success => project().Map(payload => Receipt(new AcquireTerminal.Value(Payload: payload))),
            Result.Cancel => Fin.Succ(Receipt(new AcquireTerminal.Cancelled())),
            Result.Nothing => Fin.Succ(Receipt(new AcquireTerminal.Nothing())),
            Result.ExitRhino => Fin.Succ(Receipt(new AcquireTerminal.Exit())),
            _ => Fin.Fail<AcquiredReceipt>(op.InvalidResult(detail: native.ToString())),
        };
    });

    private static AcquiredReceipt Receipt(AcquireTerminal terminal) =>
        new(Terminal: terminal, Options: [], GotDefault: false, Dragged: None);
}

internal static class GetterDrive {
    internal static Fin<AcquiredReceipt> Run<TGetter>(
        Acquire request,
        Func<TGetter> create,
        Func<TGetter, Fin<Unit>> prepare,
        Func<TGetter, Option<DragBuffer>, Fin<GetResult>> receive,
        Func<TGetter, GetResult, Fin<Acquired>> project,
        Op op)
        where TGetter : GetBaseClass => op.Catch(() => {
            using TGetter getter = create();
            getter.SetCommandPrompt(request.Prompt);
            _ = request.PromptDefault.Iter(getter.SetCommandPromptDefault);
            _ = request.Default.Iter(value => value.Apply(getter));
            return request.Accept.Apply(getter, op)
                .Bind(_ => prepare(getter))
                .Bind(_ => Dragging(request.Drag, op, dragging =>
                    dragging.Map(buffer => buffer.Bind(getter)).IfNone(Fin.Succ(unit))
                        .Bind(_ => request.Options.Match(
                            Some: options => options.Bind(getter, op),
                            None: static () => Fin.Succ(new OptionLease())))
                        .Bind(lease => {
                            using (lease) {                              // Exemption: option-lease bracket, the page's host-handle seam
                                return Cycle(request, getter, dragging, receive, project, lease, op);
                            }
                        })));
        });

    private static Fin<AcquiredReceipt> Dragging(
        Option<DragPlan> plan,
        Op op,
        Func<Option<DragBuffer>, Fin<AcquiredReceipt>> body) => plan.Match(
        Some: row => DragBuffer.Of(row, op).Bind(buffer => {
            using (buffer) {                                            // Exemption: drag-buffer bracket, the host list dies here
                return body(Some(buffer));
            }
        }),
        None: () => body(None));

    private static Fin<AcquiredReceipt> Cycle<TGetter>(
        Acquire request,
        TGetter getter,
        Option<DragBuffer> dragging,
        Func<TGetter, Option<DragBuffer>, Fin<GetResult>> receive,
        Func<TGetter, GetResult, Fin<Acquired>> project,
        OptionLease lease,
        Op op)
        where TGetter : GetBaseClass =>
        toSeq(Enumerable.Range(0, request.Accept.OptionBudget + 1))
            .FoldM<Fin, GetterCycle>(
                new GetterCycle(Choices: [], Terminal: None),
                (cycle, _) => cycle.Terminal.IsSome
                    ? Fin.Succ(cycle)
                    : receive(getter, dragging).Bind(raw => raw is GetResult.Option
                        ? lease.Selected(getter, op).Map(choice => cycle with {
                            Choices = cycle.Choices.Add(choice),
                        })
                        : Fin.Succ(cycle with { Terminal = Some(raw) })))
            .As()
            .Bind(cycle => cycle.Terminal.ToFin(Fail: op.InvalidResult(detail: nameof(AcceptPlan.OptionBudget)))
                .Bind(raw => raw switch {
                    GetResult.Cancel => Sealed(new AcquireTerminal.Cancelled(), getter, cycle.Choices, dragging),
                    GetResult.Nothing => Sealed(new AcquireTerminal.Nothing(), getter, cycle.Choices, dragging),
                    GetResult.Undo => Sealed(new AcquireTerminal.Undone(), getter, cycle.Choices, dragging),
                    GetResult.Timeout => Sealed(new AcquireTerminal.TimedOut(), getter, cycle.Choices, dragging),
                    GetResult.ExitRhino => Sealed(new AcquireTerminal.Exit(), getter, cycle.Choices, dragging),
                    GetResult.NoResult or GetResult.Miss =>
                        Fin.Fail<AcquiredReceipt>(op.InvalidResult(detail: raw.ToString())),
                    _ => project(getter, raw).Bind(payload => Sealed(
                        new AcquireTerminal.Value(Payload: payload), getter, cycle.Choices, dragging)),
                }));

    private static Fin<AcquiredReceipt> Sealed(
        AcquireTerminal terminal,
        GetBaseClass getter,
        Seq<OptionChoice> choices,
        Option<DragBuffer> dragging) =>
        dragging.Match(
            Some: buffer => buffer.Evidence().Map(Some),
            None: static () => Fin.Succ(Option<DragEvidence>.None))
        .Map(evidence => new AcquiredReceipt(
            Terminal: terminal,
            Options: choices,
            GotDefault: getter.GotDefault(),
            Dragged: evidence));
}
```

## [07]-[CALLBACK_BOUNDARY]

`PointFeedbackLease` converts every callback into a non-throwing native handler. `Subscription` owns attachment rollback and complete detachment. Callback, interrupt, and cleanup failures combine before acquisition resumes. `DragBuffer` owns the host transform list end to end: it runs the drag selection, binds a `GetTransform` or arms display feedback for a point drag, applies each `Pose` sample, and projects its measured census before disposal.

```csharp signature
// --- [BOUNDARIES] -------------------------------------------------------------------------
internal sealed class DragBuffer : IDisposable {
    private readonly TransformObjectList buffer;
    private readonly DragScope scope;
    private readonly Op op;
    private int poses;

    private DragBuffer(TransformObjectList buffer, DragScope scope, Op op) {
        this.buffer = buffer;
        this.scope = scope;
        this.op = op;
    }

    internal static Fin<DragBuffer> Of(DragPlan plan, Op op) => op.Catch(() => {
        using GetObject selection = new();                              // Exemption: host getter bracket, the selection never escapes
        selection.SetCommandPrompt(plan.Prompt);
        return plan.Selection.Rules.TraverseM(rule => rule.Apply(selection, op)).As()
            .Bind(_ => selection.GetMultiple(plan.Selection.Minimum, plan.Selection.Maximum) is GetResult.Object
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(op.InvalidResult(detail: nameof(DragPlan.Selection))))
            .Bind(_ => Minted(selection, plan.Scope, op));
    });

    private static Fin<DragBuffer> Minted(GetObject selection, DragScope scope, Op op) {
        TransformObjectList buffer = new();
        return op.Confirm(buffer.AddObjects(selection, scope.Grips) > 0)
            .Map(_ => new DragBuffer(buffer, scope, op))
            .MapFail(fault => (fun(buffer.Dispose)(), fault).Item2);
    }

    internal Fin<Unit> Bind(GetBaseClass getter) => op.Catch(() => Fin.Succ(getter switch {
        GetTransform target => Op.Side(() => target.AddTransformObjects(buffer)),
        _ => Op.Side(() => buffer.DisplayFeedbackEnabled = true),
    }));

    internal Fin<Unit> Pose(Transform xform) => op.Confirm(buffer.UpdateDisplayFeedbackTransform(xform))
        .Map(_ => ignore(Interlocked.Increment(ref poses)));

    internal Fin<DragEvidence> Evidence() => op.Catch(() => Fin.Succ(new DragEvidence(
        Objects: toSeq(buffer.ObjectArray()).Map(static row => row.Id),
        Grips: toSeq(buffer.GripArray()).Map(static row => row.Id),
        GripOwners: toSeq(buffer.GripOwnerArray()).Map(static row => row.Id),
        ObjectCount: buffer.Count,
        GripCount: buffer.GripCount,
        GripOwnerCount: buffer.GripOwnerCount,
        Extent: buffer.GetBoundingBox(regularObjects: true, grips: scope.Grips),
        Poses: Volatile.Read(ref poses))));

    public void Dispose() => buffer.Dispose();
}

internal sealed class PointFeedbackLease : IDisposable {
    private readonly GetPoint getter;
    private readonly Option<DragBuffer> dragging;
    private readonly Op op;
    private readonly Atom<Option<Error>> fault = Atom(Option<Error>.None);
    private Subscription? observation;

    private PointFeedbackLease(GetPoint getter, Option<DragBuffer> dragging, Op op) {
        this.getter = getter;
        this.dragging = dragging;
        this.op = op;
    }

    internal Option<Error> Fault => fault.Value;

    internal static Fin<PointFeedbackLease> Attach(
        GetPoint getter,
        Seq<PointFeedback> feedback,
        Option<DragBuffer> dragging,
        Op op) {
        PointFeedbackLease lease = new(getter, dragging, op);
        return Subscription.AttachAll(feedback.Map(row => (Func<Fin<Subscription>>)(() => lease.Wire(row))))
            .Map(attached => {
                lease.observation = attached;
                return lease;
            });
    }

    private Fin<Subscription> Wire(PointFeedback feedback) => op.Catch(() =>
        feedback.Switch(
            state: this,
            mouseMove: static (lease, row) => lease.Hook<GetPointMouseEventArgs>(
                args => row.Sink(PointerFact.Of(args)),
                handler => lease.getter.MouseMove += handler, handler => lease.getter.MouseMove -= handler),
            mouseDown: static (lease, row) => lease.Hook<GetPointMouseEventArgs>(
                args => row.Sink(PointerFact.Of(args)),
                handler => lease.getter.MouseDown += handler, handler => lease.getter.MouseDown -= handler),
            dynamicDraw: static (lease, row) => lease.Hook<GetPointDrawEventArgs>(row.Sink,
                handler => lease.getter.DynamicDraw += handler, handler => lease.getter.DynamicDraw -= handler),
            postDraw: static (lease, row) => {
                lease.getter.FullFrameRedrawDuringGet = true;
                return lease.Hook<DrawEventArgs>(row.Sink,
                    handler => lease.getter.PostDrawObjects += handler, handler => lease.getter.PostDrawObjects -= handler);
            },
            pose: static (lease, row) => lease.Hook<GetPointMouseEventArgs>(
                args => row.Sink(PointerFact.Of(args)).Bind(lease.Reposed),
                handler => lease.getter.MouseMove += handler, handler => lease.getter.MouseMove -= handler)));

    private Fin<Unit> Reposed(Transform xform) => dragging.Match(
        Some: buffer => buffer.Pose(xform),
        None: () => Fin.Fail<Unit>(op.InvalidContext()));

    private Fin<Subscription> Hook<TArgs>(
        Func<TArgs, Fin<Unit>> sink,
        Action<EventHandler<TArgs>> attach,
        Action<EventHandler<TArgs>> remove) {
        EventHandler<TArgs> handler = (_, args) => Deliver(() => sink(args));
        return Subscription.Attach(subscribe: attach, unsubscribe: remove, handler: handler);
    }

    private void Deliver(Func<Fin<Unit>> effect) {
        if (fault.Value.IsSome) return;
        _ = op.Catch(effect).Match(
            Succ: static _ => unit,
            Fail: error => {
                Fin<Unit> interrupted = op.Catch(() => Fin.Succ(ignore(getter.InterruptMouseMove())));
                _ = fault.Swap(current => Some(interrupted.Match(
                    Succ: _ => current.IfNone(error),
                    Fail: interrupt => current.IfNone(error) + interrupt)));
                return unit;
            });
    }

    public void Dispose() {
        Subscription? attached = Interlocked.Exchange(ref observation, null);
        if (attached?.Close() is not SubscriptionRelease.Faulted failed) return;
        _ = failed.Errors.Head.Iter(first => {
            Error cleanup = failed.Errors.Tail.Fold(first, static (all, next) => all + next);
            _ = fault.Swap(current => Some(current.Match(
                Some: primary => primary + cleanup,
                None: () => cleanup)));
        });
    }
}

internal sealed class TransformGetter(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
    internal Option<Transform> Calculated { get; private set; }
    internal Option<Error> Fault { get; private set; }

    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) {
        if (Fault.IsSome) return Transform.Unset;
        Op op = Op.Of(name: nameof(CalculateTransform));
        return op.Catch(() => Fin.Succ(calculate(viewport, point))).Match(
            Succ: value => {
                if (!value.IsValid) {
                    Fault = Some(op.InvalidResult(detail: nameof(Transform)));
                    return Transform.Unset;
                }
                Calculated = Some(value);
                return value;
            },
            Fail: error => { Fault = Some(error); return Transform.Unset; });
    }
}
```

## [08]-[BOUNDARY]

`AcquireIntent` is the sole modality entry, `AcquireTerminal` is the sole control egress, and `Acquired` is the sole value egress. `OptionLease`, `PointFeedbackLease`, `DragBuffer` with its `TransformObjectList`, `GetBaseClass`, `ObjRef`, and every one-shot `out` value terminate before the receipt crosses the session boundary; `DragEvidence` is the dragged set's detached census.

A caller-supplied delegate takes a detached fact, never a host handle: `PointerFact` for the pointer arms, `ViewportFact` for both view asks, `ObjectKinds` for the object filters. Three carves stand and each is named rather than tolerated:

- Carve: the pair of DRAW arms take `GetPointDrawEventArgs`/`DrawEventArgs`, whose live `DisplayPipeline` is the whole point of a draw sink — a callback-scoped borrow, never retained past the crossing, and the only host event type this page's public delegates admit.
- Carve: `AcquireIntent.Transform` takes `Func<RhinoViewport, Point3d, Transform>`, a live host viewport into caller code. The host's own `GetTransform.CalculateTransform` override has that shape and nothing detached can replace it — the transform is computed FROM the viewport's camera each mouse sample, so a `ViewportFact` would answer a stale frame. The borrow is bounded by `TransformGetter`'s override body; retaining the viewport past the callback is the deleted form.
- Carve: `ObjectRule.Filter` takes the host `GetObjectGeometryFilter`, whose delegate receives a live `RhinoObject`, its `GeometryBase`, and a `ComponentIndex` per candidate. The host calls it inside its own pick loop for every object under the cursor, so the filter cannot be lifted onto detached evidence without materializing a snapshot per candidate per sample. The borrow is bounded by `SetCustomGeometryFilter`'s call window, which ends when the getter disposes.

`System.Drawing.Point` on `Acquired.ScreenPoint` is the fourth and last host struct on the surface. It is a SCREEN frame the kernel does not model, and it terminates on the detached fact: nothing on this page reads it back into a host call, so it never re-crosses the boundary that produced it. Colour does not get that carve — `PerceptualColor` is the kernel owner and `Slots.Shade`/`Slots.Rgb` are the only two seams a host colour crosses.

Unit identity crosses as the kernel `ModelUnit` and nothing else: `UnitSystem`, `LengthUnit`, and a raw meters-per-unit factor each re-open on egress an admission the kernel already gated, and a bare magnitude beside a stated regime elsewhere on the receipt is the split the one paired case forecloses. This page RESOLVES a regime and never converts between two — `ModelUnit.ScaleTo` owns a cross-regime rescale, at the consumer that owns the target.

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
