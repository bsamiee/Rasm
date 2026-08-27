# [RASM_RHINO_ACQUISITION]

`Acquisition.Get` interprets one admitted `Acquire` value inside a document acquire grant. Custom getters, options, point callbacks, modal routes, and native references remain scoped to that window; egress is one detached `AcquireOutcome`. The page also declares the folder's ONE rule-roster spine: `RulePlan<TRule, TSlot>` carries the roster, the one-per-slot admission, and the two folds every rule family on this page — and the selection page's pick rules and the options page's option roster — compose instead of re-spelling.

## [01]-[INDEX]

- [02]-[PAYLOAD]: `Acquired`, `AcquireTerminal`, `AcquireOutcome`, `DragCensus`, the `ISlotted<TSlot>` contract, and the `RulePlan<TRule, TSlot>` spine.
- [03]-[ACCEPTANCE]: `AcceptSlot`, `AcceptGate`, `AcceptRule`, and `AcceptPlan` — the parameterless accept calls, the presence-cased modalities, and the terminal-derived widening.
- [04]-[POINT_ALGEBRA]: `PickOff`, `PointConstraint`, `PointGate`, `PointerShape`, `SnapBarSpan`, `SnapBarAxis`, `ArrowSense`, `BaseTrait`, `PointSlot`, `PointRule`, `PointerKey`, `PointFeedback`, `GetPointFact`, and `PointPlan`.
- [05]-[REQUEST]: `TextMeaning`, `PromptCase`, `ObjectGate`/`ObjectSlot`/`ObjectRule`/`ObjectPlan`, `DragSelection`, `ShapeAsk`, `FileAsk`, `ModalInput`, `RequestColumn`, `AcquireIntent`, `InputDefault`, `ViewportFact`, `InputMap`, and `Acquire`.
- [06]-[DRIVE]: `Acquisition.Get`, the `Concern` bracket ladder, and the `GetterDrive` option-cycle fold.
- [07]-[CALLBACK_BOUNDARY]: `DragBuffer`, `PointFeedbackLease`, and `TransformGetter`.
- [08]-[BOUNDARY]: the modality, custody, and unit-identity carves.
- [09]-[RESEARCH]: open verification rows.

## [02]-[PAYLOAD]

`Acquired` closes interactive, screen-space, scalar, object, geometry, view, transform, and file payloads at THIRTEEN cases. The seven single-field geometry wrappers collapse onto ONE `Shape` case whose payload is the kernel form recovery's own answer — `Lease<GeometryBase>`, owned by the outcome's consumer — because every producer already erased the static shape type into `Func<Fin<Acquired>>` and no consumer dispatched on it. NAMED LOSS: the per-shape static payload type; witness: `ShapeAsk.Segment` projected `new Acquired.Segment(Line)` and now projects `value.GeometryForm(key)` into `Shape`, and the one former consumer read the outcome's payload erased either way. `AcquireTerminal` preserves every non-fault control terminal, including native timeout. `DragCensus` (RENAMED from `DragEvidence` — the kernel `Interaction/input` owns `DragEvidence` as the pointer-slop fact and one assembly resolves bare names, E-R31) detaches the drag buffer's object, grip, and owner census with its measured extent and applied-pose count, so the host buffer itself dies inside the drive.

`Acquired.Distance` pairs its magnitude with the kernel `ModelUnit` read off the document at parse time — the pairing `Document/session.md`'s `UnitText.LengthValueCase` already detaches on. An `AcquireOutcome` outlives its acquire window, so a `UnitRegime` change between acquisition and use re-reads a bare magnitude in a regime that no longer produced it; consumers re-entering the value rescale through `ModelUnit.ScaleTo`, the branch's one scale owner. `Acquired.Angle` carries no regime: radians ARE the canonical measure its name spells, `AngleGrammar` owns the degree/radian dialect on the TEXT side alone, and a regime column there names a fact no document holds.

`Acquired.Paint` carries the kernel `PerceptualColor`, admitted through `Slots.Shade` at the getter boundary and quantized back through `Slots.Rgb` — which now composes the kernel `ToDrawing` egress and REFUSES an out-of-gamut colour instead of hand-building `Color.FromArgb` off the clipping byte leg; the shim that bypassed the kernel's refusal is deleted and every write boundary rides the carrier. `Acquired.Objects` carries the selection page's own `PickOutcome` — survivors, named casualties, and the participating-getter fact — rather than a bare capture sequence, so a stale reference inside a multi-object pick no longer voids the whole acquisition and the caller reads which references refused. `Acquired.ScreenPoint` keeps `System.Drawing.Point` — a screen struct IS the host's pixel frame and has no kernel counterpart — and that carve is confined to the detached fact: no operation on this page reads it back into a host call.

`RulePlan<TRule, TSlot>` is the folder's rule-roster spine (E-R30): five owners spelled `Seq<TRule>` + one-per-slot + `Traverse`-admit + apply independently — `AcceptPlan`, `PointPlan`, `ObjectPlan` here, `PickPolicy` on the selection page, `OptionSet` on the options page — and the spine owns the roster, the null screen, the slot-injectivity gate with its stated exemption, and the two folds; each family keeps only its typed wrapper and its own apply delegate. `ISlotted<TSlot>` types the slot identity: the erased `object SlotKey` compared through `object.Equals` deletes, and each family declares the closed slot vocabulary its knobs actually address.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NodaTime;
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
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ISlotted<out TSlot> where TSlot : notnull {
    TSlot SlotKey { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Acquired {
    private Acquired() { }
    public sealed record Point(Point3d Value, PointEvidence Evidence) : Acquired;
    public sealed record ScreenPoint(System.Drawing.Point Value) : Acquired;
    public sealed record Objects(PickOutcome Picked) : Acquired;
    public sealed record Number(double Value) : Acquired;
    public sealed record Count(int Value) : Acquired;
    public sealed record Text(string Value) : Acquired;
    public sealed record Toggle(bool Value) : Acquired;
    public sealed record Paint(PerceptualColor Value) : Acquired;
    public sealed record Distance(double Value, ModelUnit Unit) : Acquired;
    public sealed record Angle(double Radians) : Acquired;
    public sealed record Shape(Lease<GeometryBase> Form) : Acquired;
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PointEvidence(
    Option<uint> ViewSerial,
    Option<int> OsnapCode,
    Option<Point3d> BasePoint,
    Seq<Point3d> SnapPoints,
    Seq<Point3d> ConstructionPoints);

public sealed record DragCensus(
    Seq<Guid> Objects,
    Seq<Guid> Grips,
    Seq<Guid> GripOwners,
    int ObjectCount,
    int GripCount,
    int GripOwnerCount,
    BoundingBox Extent,
    int Poses);

[Equatable]
public sealed partial record AcquireOutcome(
    AcquireTerminal Terminal,
    [property: OrderedEquality] Seq<OptionChoice> Options,
    [property: OrderedEquality] Seq<OptionSetting> Settled,
    bool GotDefault,
    Option<DragCensus> Dragged) : IDetachedDocumentResult {
    public Option<Acquired> Payload => Terminal is AcquireTerminal.Value value ? Some(value.Payload) : None;
}

public sealed record RulePlan<TRule, TSlot>(Seq<TRule> Rules)
    where TRule : class, ISlotted<TSlot>
    where TSlot : notnull {
    public static Fin<RulePlan<TRule, TSlot>> Of(
        Seq<TRule> rules,
        Func<TRule, Fin<Unit>> admit,
        Option<Func<TRule, bool>> slotExempt = default) {
        Func<TRule, bool> exempt = slotExempt.IfNone(static _ => false);
        Seq<TRule> slotted = rules.Filter(rule => rule is not null && !exempt(rule));
        return from _ in guard(rules.ForAll(static rule => rule is not null), new KernelFault.InvalidInput()).ToFin()
               from __ in guard(
                   slotted.Map(static rule => rule.SlotKey).Distinct().Count == slotted.Count,
                   new KernelFault.InvalidInput())
               from ___ in rules.TraverseM(rule => admit(rule, key)).As()
               select new RulePlan<TRule, TSlot>(Rules: rules);
    }

    public Fin<Unit> Apply<TTarget>(TTarget target, Func<TRule, TTarget, Fin<Unit>> apply) =>
        Rules.TraverseM(rule => apply(rule, target, key)).As().Map(static _ => unit);

    public bool Holds(Func<TRule, bool> probe) => Rules.Exists(probe);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Slots {
    internal static Fin<PerceptualColor> Shade(System.Drawing.Color color) =>
        PerceptualColor.OfRgb(color.R, color.G, color.B, alpha: color.A);

    internal static Fin<System.Drawing.Color> Rgb(PerceptualColor shade) =>
        shade.ToDrawing();
}
```

## [03]-[ACCEPTANCE]

`AcceptSlot` is the acceptance family's closed slot vocabulary — one row per physical getter knob — and `AcceptGate` rows carry every parameterless native accept call beside its result terminal and its slot, so acceptance grows by one row, never a new case. `AcceptRule` closes the modalities as PRESENCE cases: `Number` enables numeric acceptance, `Zero` widens it to zero (and refuses without `Number` beside it), `Transparent` enables transparent commands — each case's presence IS the enablement, so the booleans that restated presence as payload delete, and an absent case leaves the host default standing rather than writing it. `WaitFor` carries NodaTime `Duration` — semantic time per the substrate law; the host milliseconds spell once at the apply boundary. `Requiring` is the derivation step: a prompt terminal's required row lands only into an unoccupied slot, so a caller's explicit posture survives admission and the derived row is a default, never an override.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class AcceptSlot {
    public static readonly AcceptSlot Nothing = new(key: 0);
    public static readonly AcceptSlot Undo = new(key: 1);
    public static readonly AcceptSlot Enter = new(key: 2);
    public static readonly AcceptSlot Point = new(key: 3);
    public static readonly AcceptSlot Color = new(key: 4);
    public static readonly AcceptSlot Text = new(key: 5);
    public static readonly AcceptSlot Number = new(key: 6);
    public static readonly AcceptSlot Zero = new(key: 7);
    public static readonly AcceptSlot Transparent = new(key: 8);
    public static readonly AcceptSlot Wait = new(key: 9);
}

[SmartEnum<int>]
public sealed partial class AcceptGate {
    public static readonly AcceptGate Nothing = new(key: 0, slot: AcceptSlot.Nothing, terminal: None, enable: static target => target.AcceptNothing(enable: true));
    public static readonly AcceptGate Undo = new(key: 1, slot: AcceptSlot.Undo, terminal: None, enable: static target => target.AcceptUndo(enable: true));
    public static readonly AcceptGate Enter = new(key: 2, slot: AcceptSlot.Enter, terminal: None, enable: static target => target.AcceptEnterWhenDone(enable: true));
    public static readonly AcceptGate Point = new(key: 3, slot: AcceptSlot.Point, terminal: Some(GetResult.Point), enable: static target => target.AcceptPoint(enable: true));
    public static readonly AcceptGate Color = new(key: 4, slot: AcceptSlot.Color, terminal: Some(GetResult.Color), enable: static target => target.AcceptColor(enable: true));
    public static readonly AcceptGate Text = new(key: 5, slot: AcceptSlot.Text, terminal: Some(GetResult.String), enable: static target => target.AcceptString(enable: true));

    public AcceptSlot Slot { get; }
    public Option<GetResult> Terminal { get; }

    [UseDelegateFromConstructor]
    internal partial void Enable(GetBaseClass getter);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcceptRule : ISlotted<AcceptSlot> {
    private AcceptRule() { }
    public sealed record Allowed(AcceptGate Gate) : AcceptRule;
    public sealed record Number : AcceptRule;
    public sealed record Zero : AcceptRule;
    public sealed record Transparent : AcceptRule;
    public sealed record WaitFor(Duration Window) : AcceptRule;

    public AcceptSlot SlotKey => Switch(
        allowed: static rule => rule.Gate.Slot,
        number: static _ => AcceptSlot.Number,
        zero: static _ => AcceptSlot.Zero,
        transparent: static _ => AcceptSlot.Transparent,
        waitFor: static _ => AcceptSlot.Wait);

    internal Option<GetResult> Terminal => Switch(
        allowed: static rule => rule.Gate.Terminal,
        number: static _ => Some(GetResult.Number),
        zero: static _ => Option<GetResult>.None,
        transparent: static _ => Option<GetResult>.None,
        waitFor: static _ => Option<GetResult>.None);

    internal Fin<Unit> Admit() => Switch(
        state: key,
        allowed: static (rule) => guard(rule.Gate is not null, new KernelFault.InvalidInput()).ToFin(),
        number: static (_, _) => Fin.Succ(unit),
        zero: static (_, _) => Fin.Succ(unit),
        transparent: static (_, _) => Fin.Succ(unit),
        waitFor: static (rule) => guard(
            rule.Window > Duration.Zero && rule.Window.TotalMilliseconds <= int.MaxValue,
            new KernelFault.InvalidInput()).ToFin());
}

[ComplexValueObject]
[ValidationError]
public sealed partial class AcceptPlan {
    public RulePlan<AcceptRule, AcceptSlot> Plan { get; }
    public int OptionBudget { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref RulePlan<AcceptRule, AcceptSlot> plan,
        ref int optionBudget) =>
        validationError = plan is null || optionBudget < 0 || optionBudget > 4096
            ? new ValidationError(string.Join(" | ", new object?[] { nameof(OptionBudget), optionBudget, "a budget in [0, 4096]" }))
            : plan.Holds(static rule => rule is AcceptRule.Zero)
                && !plan.Holds(static rule => rule is AcceptRule.Number)
                ? new ValidationError(string.Join(" | ", new object?[] { nameof(AcceptRule.Zero), "zero acceptance only beside numeric acceptance" }))
                : null;

    public static Fin<AcceptPlan> Of(Seq<AcceptRule> rules, int optionBudget) {
        return RulePlan<AcceptRule, AcceptSlot>.Of(rules: rules, admit: static (rule, k) => rule.Admit(k))
            .Bind(plan => FactoryBridge.Accept<AcceptPlan>(
                fault: Validate(plan, optionBudget, out AcceptPlan? admitted), admitted: admitted));
    }

    internal Seq<AcceptRule> Rules => Plan.Rules;

    internal bool AcceptsNothing => Plan.Holds(static rule => rule is AcceptRule.Allowed { Gate.Slot.Key: 0 });

    internal Fin<AcceptPlan> Requiring(Seq<GetResult> terminals) {
        Seq<AcceptRule> missing = terminals
            .Choose(Derived)
            .Distinct()
            .Filter(row => !Rules.Exists(held => held.SlotKey == row.SlotKey));
        return missing.IsEmpty ? Fin.Succ(value: this) : Of(rules: Rules + missing, optionBudget: OptionBudget);
    }

    internal Fin<Unit> Apply(GetBaseClass getter) => Plan.Apply(
        target: getter,
        apply: (rule, target, op) => Try.lift(() => {
            rule.Switch(
                state: (Target: target, Zero: Plan.Holds(static held => held is AcceptRule.Zero)),
                allowed: static (held, row) => { row.Gate.Enable(held.Target); return unit; },
                number: static (held, _) => { held.Target.AcceptNumber(enable: true, acceptZero: held.Zero); return unit; },
                zero: static (_, _) => unit,
                transparent: static (held, _) => { held.Target.EnableTransparentCommands(enable: true); return unit; },
                waitFor: static (held, row) => {
                    held.Target.SetWaitDuration(milliseconds: checked((int)Math.Ceiling(row.Window.TotalMilliseconds)));
                    return unit;
                });
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner));

    private static Option<AcceptRule> Derived(GetResult terminal) => terminal switch {
        GetResult.Number => Some((AcceptRule)new AcceptRule.Number()),
        GetResult.String => Some((AcceptRule)new AcceptRule.Allowed(Gate: AcceptGate.Text)),
        GetResult.Color => Some((AcceptRule)new AcceptRule.Allowed(Gate: AcceptGate.Color)),
        _ => Option<AcceptRule>.None,
    };
}
```

## [04]-[POINT_ALGEBRA]

`PointConstraint` closes the native constraint family; its four pick-off surfaces carry ONE `PickOff` carrier instead of four raw bools, so the axis has one name and a fifth constrained surface composes the row. `PointRule` parameterizes every independent point-getter setting as data over the closed `PointSlot` vocabulary: `PointGate` becomes a capability vocabulary and the per-gate `(row, bool)` pair collapses onto ONE `Gates(Enabled, Disabled)` rule carrying two disjoint sets, the snap bars carry `SnapBarSpan` rows instead of an `(Enabled, Ends)` pair, the direction arrow carries `ArrowSense`, and the base point carries its two independent affordances as a `CapabilitySet<BaseTrait>`. `PointFeedback` carries result-returning callbacks whose failures interrupt the native loop and surface after `Get` returns; `Pose` alone returns a value — its `Transform` re-poses the drag buffer through the host's own display-feedback call.

The three pointer arms take `GetPointFact` (RENAMED from `PointerFact` — the kernel `Interaction/input` owns that name and `Display/interaction` carries `ViewportPointerFact`; this is the THIRD spelling and it names its getter frame, E-R31): world point, window point, viewport identity, and the held `CapabilitySet<PointerKey>`, projected at the callback edge so no `GetPointMouseEventArgs` reaches a caller sink. The two DRAW arms keep their host args because a draw sink's whole purpose is the live `DisplayPipeline` the arg carries.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class PickOff {
    public static readonly PickOff Locked = new(key: false);
    public static readonly PickOff Free = new(key: true);
}

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
    public sealed record OnCurve(Curve Value, PickOff Pick) : PointConstraint;
    public sealed record OnSurface(Surface Value, PickOff Pick) : PointConstraint;
    public sealed record OnBrep(Brep Value, int WireDensity, int FaceIndex, PickOff Pick) : PointConstraint;
    public sealed record OnMesh(Mesh Value, PickOff Pick) : PointConstraint;
    public sealed record OnConstructionPlane(bool ThroughBasePoint) : PointConstraint;
    public sealed record OnTargetPlane : PointConstraint;
    public sealed record OnCPlaneIntersection(Plane Value) : PointConstraint;

    internal Fin<Unit> Admit() => Try.lift(() => AdmitGeometry(Switch(
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
        onConstructionPlane: static _ => true,
        onTargetPlane: static _ => true,
        onCPlaneIntersection: static row => row.Value.IsValid))).Run().Bind(static inner => inner);

    internal static Fin<Unit> AdmitGeometry(params ReadOnlySpan<bool> validity) =>
        guard(flag: validity.IndexOf(false) < 0, False: new KernelFault.InvalidInput()).ToFin();

    internal Fin<Unit> Apply(GetPoint getter) => Try.lift(() => Switch(
        state: getter,
        onSegment: static (held, rule) => Admit.Confirm(held.Constrain(rule.From, rule.To)),
        onLine: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value)),
        onArc: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value)),
        onCircle: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value)),
        onPlane: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value, rule.AllowElevator)),
        onSphere: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value)),
        onCylinder: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value)),
        onCurve: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value, rule.Pick.Key)),
        onSurface: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value, rule.Pick.Key)),
        onBrep: static (held, rule) => Admit.Confirm(held.Constrain(
            rule.Value, rule.WireDensity, rule.FaceIndex, rule.Pick.Key)),
        onMesh: static (held, rule) => Admit.Confirm(held.Constrain(rule.Value, rule.Pick.Key)),
        onConstructionPlane: static (held, rule) => Admit.Confirm(
            held.ConstrainToConstructionPlane(rule.ThroughBasePoint)),
        onTargetPlane: static (held, _) => Try.lift(() => {
            held.ConstrainToTargetPlane();
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner),
        onCPlaneIntersection: static (held, rule) => Admit.Confirm(
            held.ConstrainToVirtualCPlaneIntersection(rule.Value)))).Run().Bind(static inner => inner);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointGate : ICapability<PointGate> {
    public static readonly PointGate ObjectSnapCursor = new(key: "osnap-cursor", set: static (getter, on) => getter.EnableObjectSnapCursors(on));
    public static readonly PointGate Ortho = new(key: "ortho", set: static (getter, on) => getter.PermitOrthoSnap(on));
    public static readonly PointGate ObjectSnap = new(key: "osnap", set: static (getter, on) => getter.PermitObjectSnap(on));
    public static readonly PointGate ConstraintOptions = new(key: "constraint-options", set: static (getter, on) => getter.PermitConstraintOptions(on));
    public static readonly PointGate FromOption = new(key: "from-option", set: static (getter, on) => getter.PermitFromOption(on));
    public static readonly PointGate TabMode = new(key: "tab-mode", set: static (getter, on) => getter.PermitTabMode(on));
    public static readonly PointGate Curves = new(key: "snap-curves", set: static (getter, on) => getter.EnableSnapToCurves(on));
    public static readonly PointGate ExitRedraw = new(key: "exit-redraw", set: static (getter, on) => getter.EnableNoRedrawOnExit(on));
    public static readonly PointGate FullFrame = new(key: "full-frame", set: static (getter, on) => getter.FullFrameRedrawDuringGet = on);

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
public sealed partial class SnapBarSpan {
    public static readonly SnapBarSpan Off = new(key: 0, enabled: false, ends: false);
    public static readonly SnapBarSpan Bar = new(key: 1, enabled: true, ends: false);
    public static readonly SnapBarSpan BarWithEnds = new(key: 2, enabled: true, ends: true);

    internal bool Enabled { get; }
    internal bool Ends { get; }
}

[SmartEnum<int>]
public sealed partial class SnapBarAxis {
    public static readonly SnapBarAxis Tangent = new(key: 0, slot: static () => PointSlot.TangentBar, set: static (getter, span) => getter.EnableCurveSnapTangentBar(span.Enabled, span.Ends));
    public static readonly SnapBarAxis Perpendicular = new(key: 1, slot: static () => PointSlot.PerpendicularBar, set: static (getter, span) => getter.EnableCurveSnapPerpBar(span.Enabled, span.Ends));

    private readonly Func<PointSlot> slot;
    internal PointSlot Slot => slot();

    [UseDelegateFromConstructor]
    internal partial void Set(GetPoint getter, SnapBarSpan span);
}

[SmartEnum<int>]
public sealed partial class ArrowSense {
    public static readonly ArrowSense Off = new(key: 0, enabled: false, reverse: false);
    public static readonly ArrowSense Forward = new(key: 1, enabled: true, reverse: false);
    public static readonly ArrowSense Reverse = new(key: 2, enabled: true, reverse: true);

    internal bool Enabled { get; }
    internal bool Reverse { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BaseTrait : ICapability<BaseTrait> {
    public static readonly BaseTrait ShowDistance = new(key: "show-distance");
    public static readonly BaseTrait DrawLine = new(key: "draw-line");
}

[SmartEnum<int>]
public sealed partial class PointSlot {
    public static readonly PointSlot Constrained = new(key: 0);
    public static readonly PointSlot Snaps = new(key: 1);
    public static readonly PointSlot ConstructionPoints = new(key: 2);
    public static readonly PointSlot Based = new(key: 3);
    public static readonly PointSlot Radial = new(key: 4);
    public static readonly PointSlot Cursor = new(key: 5);
    public static readonly PointSlot Elevator = new(key: 6);
    public static readonly PointSlot Gates = new(key: 7);
    public static readonly PointSlot TangentBar = new(key: 8);
    public static readonly PointSlot PerpendicularBar = new(key: 9);
    public static readonly PointSlot Arrow = new(key: 10);
    public static readonly PointSlot MouseUp = new(key: 11);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointRule : ISlotted<PointSlot> {
    private PointRule() { }
    public sealed record Constrained(PointConstraint Value) : PointRule;
    public sealed record Snaps(Seq<Point3d> Values) : PointRule;
    public sealed record ConstructionPoints(Seq<Point3d> Values) : PointRule;
    public sealed record BasedAt(Point3d Value, CapabilitySet<BaseTrait> Traits) : PointRule;
    public sealed record Radial(double Distance) : PointRule;
    public sealed record Cursor(PointerShape Value) : PointRule;
    public sealed record ElevatorMode(int Mode) : PointRule;
    public sealed record Gates(CapabilitySet<PointGate> Enabled, CapabilitySet<PointGate> Disabled) : PointRule;
    public sealed record SnapBar(SnapBarAxis Axis, SnapBarSpan Span) : PointRule;
    public sealed record DirectionArrow(ArrowSense Sense) : PointRule;
    public sealed record OnMouseUp : PointRule;

    public PointSlot SlotKey => Switch(
        constrained: static _ => PointSlot.Constrained,
        snaps: static _ => PointSlot.Snaps,
        constructionPoints: static _ => PointSlot.ConstructionPoints,
        basedAt: static _ => PointSlot.Based,
        radial: static _ => PointSlot.Radial,
        cursor: static _ => PointSlot.Cursor,
        elevatorMode: static _ => PointSlot.Elevator,
        gates: static _ => PointSlot.Gates,
        snapBar: static rule => rule.Axis.Slot,
        directionArrow: static _ => PointSlot.Arrow,
        onMouseUp: static _ => PointSlot.MouseUp);

    internal Fin<Unit> Admit() => Switch(
        state: key,
        constrained: static (rule) => Admit.Need(rule.Value).Bind(value => value.Admit()),
        snaps: static (rule) => guard(!rule.Values.IsEmpty, new KernelFault.InvalidInput()).ToFin()
            .Bind(_ => PointConstraint.AdmitGeometry([.. rule.Values.Map(static point => point.IsValid)])),
        constructionPoints: static (rule) => guard(!rule.Values.IsEmpty, new KernelFault.InvalidInput()).ToFin()
            .Bind(_ => PointConstraint.AdmitGeometry([.. rule.Values.Map(static point => point.IsValid)])),
        basedAt: static (rule) => PointConstraint.AdmitGeometry(rule.Value.IsValid),
        radial: static (rule) => ValidityClaim.Finite(value: rule.Distance).Holds && rule.Distance >= 0.0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidInput()),
        cursor: static (rule) => guard(
            rule.Value is not null && PointerShape.Items.Contains(rule.Value),
            new KernelFault.InvalidInput()).ToFin(),
        elevatorMode: static (rule) => guard(rule.Mode >= 0, new KernelFault.InvalidInput()).ToFin(),
        gates: static (rule) => guard(
            rule.Enabled.Held.All(row => !rule.Disabled.Admits(capability: row)),
            new KernelFault.InvalidInput()).ToFin(),
        snapBar: static (rule) => guard(rule.Axis is not null && rule.Span is not null, new KernelFault.InvalidInput()).ToFin(),
        directionArrow: static (rule) => guard(rule.Sense is not null, new KernelFault.InvalidInput()).ToFin(),
        onMouseUp: static (_, _) => Fin.Succ(unit));

    internal Fin<Unit> Apply(GetPoint getter) => Switch(
        state: getter,
        constrained: static (held, rule) => rule.Value.Apply(held),
        snaps: static (held, rule) => Try.lift(() => Fin.Succ(ignore(
            held.AddSnapPoints(points: [.. rule.Values])))).Run().Bind(static inner => inner),
        constructionPoints: static (held, rule) => Try.lift(() => Fin.Succ(ignore(
            held.AddConstructionPoints(points: [.. rule.Values])))).Run().Bind(static inner => inner),
        basedAt: static (held, rule) => Try.lift(() => {
            bool distance = rule.Traits.Admits(capability: BaseTrait.ShowDistance);
            held.SetBasePoint(rule.Value, distance);
            bool line = rule.Traits.Admits(capability: BaseTrait.DrawLine);
            held.EnableDrawLineFromPoint(line);
            if (line) held.DrawLineFromPoint(rule.Value, distance);
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner),
        radial: static (held, rule) => Try.lift(() => {
            held.ConstrainDistanceFromBasePoint(rule.Distance);
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner),
        cursor: static (held, rule) => Try.lift(() => { held.SetCursor(rule.Value.Native); return Fin.Succ(unit); }).Run().Bind(static inner => inner),
        elevatorMode: static (held, rule) => Try.lift(() => { held.PermitElevatorMode(rule.Mode); return Fin.Succ(unit); }).Run().Bind(static inner => inner),
        gates: static (held, rule) => Try.lift(() => {
            rule.Enabled.Held.Iter(row => row.Set(held, enabled: true));
            rule.Disabled.Held.Iter(row => row.Set(held, enabled: false));
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner),
        snapBar: static (held, rule) => Try.lift(() => { rule.Axis.Set(held, rule.Span); return Fin.Succ(unit); }).Run().Bind(static inner => inner),
        directionArrow: static (held, rule) => Try.lift(() => {
            held.EnableCurveSnapArrow(rule.Sense.Enabled, rule.Sense.Reverse);
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner),
        onMouseUp: static (_, _) => Fin.Succ(unit));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointerKey : ICapability<PointerKey> {
    public static readonly PointerKey LeftButton = new(key: "left", read: static args => args.LeftButtonDown);
    public static readonly PointerKey MiddleButton = new(key: "middle", read: static args => args.MiddleButtonDown);
    public static readonly PointerKey RightButton = new(key: "right", read: static args => args.RightButtonDown);
    public static readonly PointerKey Shift = new(key: "shift", read: static args => args.ShiftKeyDown);
    public static readonly PointerKey Control = new(key: "control", read: static args => args.ControlKeyDown);

    [UseDelegateFromConstructor]
    internal partial bool Read(GetPointMouseEventArgs args);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointFeedback {
    private PointFeedback() { }
    public sealed record MouseMove(Func<GetPointFact, Fin<Unit>> Sink) : PointFeedback;
    public sealed record MouseDown(Func<GetPointFact, Fin<Unit>> Sink) : PointFeedback;
    public sealed record DynamicDraw(Func<GetPointDrawEventArgs, Fin<Unit>> Sink) : PointFeedback;
    public sealed record PostDraw(Func<DrawEventArgs, Fin<Unit>> Sink) : PointFeedback;
    public sealed record Pose(Func<GetPointFact, Fin<Transform>> Sink) : PointFeedback;

    internal Fin<Unit> Admit() => Switch(
        mouseMove: row => guard(row.Sink is not null, new KernelFault.InvalidInput()).ToFin(),
        mouseDown: row => guard(row.Sink is not null, new KernelFault.InvalidInput()).ToFin(),
        dynamicDraw: row => guard(row.Sink is not null, new KernelFault.InvalidInput()).ToFin(),
        postDraw: row => guard(row.Sink is not null, new KernelFault.InvalidInput()).ToFin(),
        pose: row => guard(row.Sink is not null, new KernelFault.InvalidInput()).ToFin());
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GetPointFact(
    Point3d World,
    System.Drawing.Point Window,
    Guid Viewport,
    CapabilitySet<PointerKey> Keys);

public sealed record PointPlan {
    private PointPlan(RulePlan<PointRule, PointSlot> plan, Seq<PointFeedback> feedback) {
        Plan = plan;
        Feedback = feedback;
    }

    public RulePlan<PointRule, PointSlot> Plan { get; }
    public Seq<PointFeedback> Feedback { get; }
    public Seq<PointRule> Rules => Plan.Rules;
    public static PointPlan Free { get; } = new(plan: new RulePlan<PointRule, PointSlot>(Rules: []), feedback: []);

    public static Fin<PointPlan> Of(Seq<PointFeedback> feedback, params ReadOnlySpan<PointRule> rules) {
        return from admitted in RulePlan<PointRule, PointSlot>.Of(
                   rules: toSeq(rules.ToArray()),
                   admit: static (rule, k) => rule.Admit(k),
                   slotExempt: Some<Func<PointRule, bool>>(static rule => rule is PointRule.Constrained))
               from _ in feedback.TraverseM(row => row.Admit()).As()
               select new PointPlan(plan: admitted, feedback: feedback);
    }

    internal bool OnMouseUp => Plan.Holds(static rule => rule is PointRule.OnMouseUp);
}
```

## [05]-[REQUEST]

`PromptCase` generates the interactive value space over one `GetPoint`; multiple distinct terminal cases compose number, text, color, 3D point, and 2D point acquisition without getter-specific helper classes. `Acquire.Of` admits each option, prompt default, typed default, drag selection, and accept terminal against the selected `AcquireIntent` through the one `RequestColumn` fold, so no configured column outruns its intent, and it closes the loop the other way through `AcceptPlan.Requiring`: each prompt case's `Terminal` derives the accept row it needs and that row folds into the plan ONLY where the caller left the slot empty.

`ObjectPlan`, `ModalInput`, `DragSelection`, and `AcquireIntent` close the remaining custom and one-shot routes; `ShapeAsk` rows carry the parameterless one-shot shape getters as data projecting through the kernel form recovery, so a new native shape is one row. `DragSelection` (RENAMED from `DragPlan` — the kernel `Interaction/transfer` owns `DragPlan` for the transfer payload; this selects DOCUMENT OBJECTS into a `TransformObjectList`, E-R31) carries the drag prompt, its object plan, and its scope.

The modal object asks take `Document`'s `ObjectKinds`, never a raw `ObjectType`. The view asks project `ViewportFact` at the boundary. `FileAsk` keys the host's sparse `GetFileNameMode` roster, and the file ask's `Option<string> Title` IS the route discriminant: a caption drives `GetFileName` and its absence drives `GetFileNameScripted`. `InputMap` is the `[Mapper]` boundary for the three host-args projections — the getter mouse args, a viewport, and a view — so the detachment correspondence is generated with its derived columns declared, never three hand bodies (`[05]` boundary `InputMap`).

The point getter lends `PickContext` and `GetPoint` to `GumballRig.Pick`; move and completion return transform evidence directly from the rig.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TextMeaning {
    public static readonly TextMeaning Literal = new(key: 0, parse: static (text, _, _) =>
        Fin.Succ<Acquired>(new Acquired.Text(Value: text)));
    public static readonly TextMeaning Number = new(key: 1, parse: static (text, _, key) => Try.lift(() => {
        StringParserSettings output = StringParserSettings.ParseSettingsDoubleNumber;
        int consumed = StringParser.ParseNumber(
            text, 0, StringParserSettings.ParseSettingsDoubleNumber, ref output, out double value);
        return consumed == text.Length && ValidityClaim.Finite(value: value).Holds
            ? Fin.Succ<Acquired>(new Acquired.Number(Value: value))
            : Fin.Fail<Acquired>(new KernelFault.InvalidInput());
    }).Run().Bind(static inner => inner));
    public static readonly TextMeaning Length = new(key: 2, parse: static (text, document, key) =>
        from regime in DocumentSpace.Model.Read(document: document)
        from encoded in UnitText.Length(text: text)
        from crossed in encoded.Cross(regime: regime)
        from measured in crossed is UnitText.LengthValueCase value
            ? Fin.Succ<Acquired>(value: new Acquired.Distance(Value: value.Value, Unit: value.Unit))
            : Fin.Fail<Acquired>(error: new KernelFault.InvalidResult())
        select measured);
    public static readonly TextMeaning AngleDegrees = new(key: 3, parse: static (text, _, key) =>
        AngleGrammar.Degrees.Parse(text: text)
            .Map(static radians => (Acquired)new Acquired.Angle(Radians: radians)));
    public static readonly TextMeaning AngleRadians = new(key: 4, parse: static (text, _, key) =>
        AngleGrammar.Radians.Parse(text: text)
            .Map(static radians => (Acquired)new Acquired.Angle(Radians: radians)));

    [UseDelegateFromConstructor]
    public partial Fin<Acquired> Parse(string text, RhinoDoc document);
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

    internal Fin<Unit> Admit() => guard(this is not TextValue { Meaning: null }, new KernelFault.InvalidInput()).ToFin();

    internal bool Accepts(InputDefault value) => (this, value) switch {
        (Point3, InputDefault.PointValue) => true,
        (NumberValue rule, InputDefault.NumberValue value) => rule.Band.Contains(value.Value),
        (CountValue rule, InputDefault.CountValue value) => rule.Band.Contains(value.Value),
        (TextValue, InputDefault.TextValue) => true,
        (PaintValue, InputDefault.PaintValue) => true,
        _ => false,
    };

    internal Fin<Acquired> Project(GetPoint getter, RhinoDoc document) => Switch(
        state: (Getter: getter, Document: document),
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
            : Fin.Fail<Acquired>(new KernelFault.InvalidInput()),
        countValue: static (held, rule) => held.Getter.Number() is var raw
            && raw == Math.Truncate(raw) && raw >= int.MinValue && raw <= int.MaxValue && rule.Band.Contains((int)raw)
            ? Fin.Succ<Acquired>(new Acquired.Count(Value: (int)raw))
            : Fin.Fail<Acquired>(new KernelFault.InvalidInput()),
        textValue: static (held, rule) => rule.Meaning.Parse(
            text: held.Getter.StringResult(), document: held.Document),
        paintValue: static (held, _) => Slots.Shade(color: held.Getter.Color())
            .Map(static shade => (Acquired)new Acquired.Paint(Value: shade)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectGate : ICapability<ObjectGate> {
    public static readonly ObjectGate PostSelect = new(key: "post-select", set: static (getter, on) => getter.EnablePostSelect(on));
    public static readonly ObjectGate Previous = new(key: "previous", set: static (getter, on) => getter.EnableSelPrevious(on));
    public static readonly ObjectGate Highlight = new(key: "highlight", set: static (getter, on) => getter.EnableHighlight(on));
    public static readonly ObjectGate IgnoreGrips = new(key: "ignore-grips", set: static (getter, on) => getter.EnableIgnoreGrips(on));
    public static readonly ObjectGate EnterPrompt = new(key: "enter-prompt", set: static (getter, on) => getter.EnablePressEnterWhenDonePrompt(on));

    [UseDelegateFromConstructor]
    internal partial void Set(GetObject getter, bool enabled);
}

[SmartEnum<int>]
public sealed partial class ObjectSlot {
    public static readonly ObjectSlot PreSelect = new(key: 0);
    public static readonly ObjectSlot Gates = new(key: 1);
    public static readonly ObjectSlot Filter = new(key: 2);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectRule : ISlotted<ObjectSlot> {
    private ObjectRule() { }
    public sealed record PreSelect(bool Enabled, bool IgnoreUnacceptable) : ObjectRule;
    public sealed record Gates(CapabilitySet<ObjectGate> Enabled, CapabilitySet<ObjectGate> Disabled) : ObjectRule;
    public sealed record Filter(GetObjectGeometryFilter Value) : ObjectRule;

    public ObjectSlot SlotKey => Switch(
        preSelect: static _ => ObjectSlot.PreSelect,
        gates: static _ => ObjectSlot.Gates,
        filter: static _ => ObjectSlot.Filter);

    internal Fin<Unit> Admit() => Switch(
        state: key,
        preSelect: static (_, _) => Fin.Succ(unit),
        gates: static (rule) => guard(
            rule.Enabled.Held.All(row => !rule.Disabled.Admits(capability: row)),
            new KernelFault.InvalidInput()).ToFin(),
        filter: static (rule) => guard(rule.Value is not null, new KernelFault.InvalidInput()).ToFin());

    internal Fin<Unit> Apply(GetObject getter) => Try.lift(() => {
        Switch(
            state: getter,
            preSelect: static (target, rule) => { target.EnablePreSelect(rule.Enabled, rule.IgnoreUnacceptable); return unit; },
            gates: static (target, rule) => {
                rule.Enabled.Held.Iter(row => row.Set(target, enabled: true));
                rule.Disabled.Held.Iter(row => row.Set(target, enabled: false));
                return unit;
            },
            filter: static (target, rule) => { target.SetCustomGeometryFilter(rule.Value); return unit; });
        return Fin.Succ(unit);
    }).Run().Bind(static inner => inner);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class ObjectPlan {
    public int Minimum { get; }
    public int Maximum { get; }
    public RulePlan<ObjectRule, ObjectSlot> Plan { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int minimum,
        ref int maximum,
        ref RulePlan<ObjectRule, ObjectSlot> plan) =>
        validationError = plan is null || minimum < 0 || maximum < 0 || (maximum is not 0 && maximum < minimum)
            ? new ValidationError(string.Join(" | ", new object?[] { nameof(ObjectPlan), "a rule plan with non-negative bounds whose maximum is zero or at least the minimum" }))
            : null;

    public static Fin<ObjectPlan> Of(int minimum, int maximum, Seq<ObjectRule> rules) {
        return RulePlan<ObjectRule, ObjectSlot>.Of(rules: rules, admit: static (rule, k) => rule.Admit(k))
            .Bind(plan => FactoryBridge.Accept<ObjectPlan>(
                fault: Validate(minimum, maximum, plan, out ObjectPlan? admitted), admitted: admitted));
    }

    internal Seq<ObjectRule> Rules => Plan.Rules;
}

[SmartEnum<int>]
public sealed partial class DragScope {
    public static readonly DragScope ObjectsOnly = new(key: 0, grips: false);
    public static readonly DragScope ObjectsAndGrips = new(key: 1, grips: true);

    public bool Grips { get; }
}

[ComplexValueObject]
[ValidationError]
public sealed partial class DragSelection {
    public string Prompt { get; }
    public ObjectPlan Selection { get; }
    public DragScope Scope { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string prompt,
        ref ObjectPlan selection,
        ref DragScope scope) =>
        validationError = !string.IsNullOrWhiteSpace(prompt) && selection is not null && scope is not null && selection.Minimum >= 1
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(DragSelection), "a prompt, a scope, and a selection admitting at least one object" }));
}

[SmartEnum<int>]
public sealed partial class ShapeAsk {
    public static readonly ShapeAsk Segment = new(key: 0, run: static op =>
        Recovered(RhinoGet.GetLine(out Line value), () => new LineCurve(value).GeometryForm()));
    public static readonly ShapeAsk Chain = new(key: 1, run: static op =>
        Recovered(RhinoGet.GetPolyline(out Polyline value), () => value.ToPolylineCurve().GeometryForm()));
    public static readonly ShapeAsk ArcShape = new(key: 2, run: static op =>
        Recovered(RhinoGet.GetArc(out Arc value), () => value.GeometryForm()));
    public static readonly ShapeAsk CircleShape = new(key: 3, run: static op =>
        Recovered(RhinoGet.GetCircle(out Circle value), () => value.GeometryForm()));
    public static readonly ShapeAsk PlaneShape = new(key: 4, run: static op =>
        Recovered(RhinoGet.GetPlane(out Plane value), () => value.GeometryForm()));
    public static readonly ShapeAsk RectangleShape = new(key: 5, run: static op =>
        Recovered(RhinoGet.GetRectangle(out Point3d[] value), () => new Polyline([.. value, value[0]]).ToPolylineCurve().GeometryForm()));
    public static readonly ShapeAsk BoxShape = new(key: 6, run: static op =>
        Recovered(RhinoGet.GetBox(out Box value), () => value.GeometryForm()));

    [UseDelegateFromConstructor]
    internal partial (Result Native, Func<Fin<Acquired>> Project) Run();

    private static (Result Native, Func<Fin<Acquired>> Project) Recovered(
        Result native, Func<Fin<Lease<GeometryBase>>> recover) =>
        (native, () => recover().Map(static form => (Acquired)new Acquired.Shape(Form: form)));
}

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

    internal Fin<Unit> Admit() => Switch(
        point: static _ => Fin.Succ(unit),
        oneObject: row => Optional(row.Filter).Map(static _ => unit).ToFin(Fail: new KernelFault.InvalidInput()),
        manyObjects: row => Optional(row.Filter).Map(static _ => unit).ToFin(Fail: new KernelFault.InvalidInput()),
        text: row => Optional(row.Seed).Map(static _ => unit).ToFin(Fail: new KernelFault.InvalidInput()),
        toggle: row => from _ in Acceptance.Text(row.Off)
                       from __ in Acceptance.Text(row.On)
                       from ___ in guard(!string.Equals(row.Off, row.On, StringComparison.OrdinalIgnoreCase), new KernelFault.InvalidInput())
                       select unit,
        number: row => guard(
            ValidityClaim.All(
                ValidityClaim.Finite(value: row.Seed),
                ValidityClaim.Finite(value: row.Lower),
                ValidityClaim.Finite(value: row.Upper),
                row.Lower <= row.Seed,
                row.Seed <= row.Upper),
            new KernelFault.InvalidInput()).ToFin(),
        count: row => guard(row.Lower <= row.Seed && row.Seed <= row.Upper, new KernelFault.InvalidInput()).ToFin(),
        paint: row => Admit.Need(row.Seed).Map(static _ => unit),
        distance: row => guard(
            ValidityClaim.All(ValidityClaim.Finite(value: row.Seed), row.Seed >= 0.0),
            new KernelFault.InvalidInput()).ToFin(),
        shape: row => guard(row.Ask is not null, new KernelFault.InvalidInput()).ToFin(),
        view: row => guard(row.Project is not null, new KernelFault.InvalidInput()).ToFin(),
        viewports: row => guard(row.Project is not null, new KernelFault.InvalidInput()).ToFin(),
        file: row => from _ in guard(row.Ask is not null && row.DefaultName is not null, new KernelFault.InvalidInput()).ToFin()
                     from __ in row.Title.Traverse(caption => Acceptance.Text(caption)).As()
                     select unit);

    internal bool Accepts(AcceptRule rule) =>
        rule is AcceptRule.Allowed { Gate.Slot.Key: 0 }
        && this is Point or OneObject or ManyObjects or Text or Toggle or Number or Count or Paint;
}

[SmartEnum<int>]
public sealed partial class RequestColumn {
    public static readonly RequestColumn Options = new(key: 0);
    public static readonly RequestColumn PromptDefault = new(key: 1);
    public static readonly RequestColumn Drag = new(key: 2);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AcquireIntent {
    private AcquireIntent() { }
    public sealed record Interactive(Seq<PromptCase> Cases, PointPlan Point) : AcquireIntent;
    public sealed record Objects(ObjectPlan Plan) : AcquireIntent;
    public sealed record Transform(Func<RhinoViewport, Point3d, Transform> Calculate) : AcquireIntent;
    public sealed record Modal(ModalInput Input) : AcquireIntent;

    internal bool Admits(RequestColumn column) => Switch(
        state: column,
        interactive: static (_, _) => true,
        objects: static (held, _) => held != RequestColumn.Drag,
        transform: static (_, _) => true,
        modal: static (_, _) => false);

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

    internal Fin<Unit> Admit() => Switch(
        interactive: row => from _ in guard(!row.Cases.IsEmpty
                               && row.Cases.ForAll(static value => value is not null)
                               && row.Cases.Map(static value => value.Terminal).Distinct().Count == row.Cases.Count,
                               new KernelFault.InvalidInput()).ToFin()
                            from __ in guard(row.Point is not null, new KernelFault.InvalidInput())
                            from ____ in row.Cases.TraverseM(value => value.Admit()).As()
                            select unit,
        objects: row => guard(row.Plan is not null, new KernelFault.InvalidInput()).ToFin(),
        transform: row => guard(row.Calculate is not null, new KernelFault.InvalidInput()).ToFin(),
        modal: row => Admit.Need(row.Input).Bind(value => value.Admit()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InputDefault {
    private InputDefault() { }
    public sealed record PointValue(Point3d Value) : InputDefault;
    public sealed record NumberValue(double Value) : InputDefault;
    public sealed record CountValue(int Value) : InputDefault;
    public sealed record TextValue(string Value) : InputDefault;
    public sealed record PaintValue(PerceptualColor Value) : InputDefault;

    internal Fin<Unit> Admit() => Switch(
        pointValue: static _ => Fin.Succ(unit),
        numberValue: row => guard(ValidityClaim.Finite(value: row.Value).Holds, new KernelFault.InvalidInput()).ToFin(),
        countValue: static _ => Fin.Succ(unit),
        textValue: row => Acceptance.Text(row.Value).Map(static _ => unit),
        paintValue: row => Admit.Need(row.Value).Map(static _ => unit));

    internal Fin<Unit> Apply(GetBaseClass getter) => Switch(
        state: getter,
        pointValue: static (held, value) => Fin.Succ(HostEdge.Side(() => held.SetDefaultPoint(value.Value))),
        numberValue: static (held, value) => Fin.Succ(HostEdge.Side(() => held.SetDefaultNumber(value.Value))),
        countValue: static (held, value) => Fin.Succ(HostEdge.Side(() => held.SetDefaultInteger(value.Value))),
        textValue: static (held, value) => Fin.Succ(HostEdge.Side(() => held.SetDefaultString(value.Value))),
        paintValue: static (held, value) => Slots.Rgb(shade: value.Value)
            .Map(color => HostEdge.Side(() => held.SetDefaultColor(color))));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ViewportFact(Guid Id, string Name, Option<uint> ViewSerial);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class InputMap {
    [MapProperty(nameof(GetPointMouseEventArgs.Point), nameof(GetPointFact.World))]
    [MapProperty(nameof(GetPointMouseEventArgs.WindowPoint), nameof(GetPointFact.Window))]
    [MapperIgnoreSource(nameof(GetPointMouseEventArgs.Viewport))]
    internal static partial GetPointFact Fact(GetPointMouseEventArgs args);

    private static Guid ViewportId(GetPointMouseEventArgs args) => args.Viewport.Id;
    private static CapabilitySet<PointerKey> Keys(GetPointMouseEventArgs args) =>
        CapabilitySet<PointerKey>.Of([.. toSeq(PointerKey.Items).Filter(row => row.Read(args))]);

    internal static ViewportFact Fact(RhinoViewport viewport) => new(
        Id: viewport.Id,
        Name: viewport.Name,
        ViewSerial: Optional(viewport.ParentView).Map(static view => view.RuntimeSerialNumber));

    internal static ViewportFact Fact(RhinoView view) => Fact(view.MainViewport);
}

public sealed record Acquire {
    private Acquire(
        AcquireIntent intent,
        string prompt,
        AcceptPlan accept,
        Option<string> promptDefault,
        Option<InputDefault> @default,
        Option<OptionSet> options,
        Option<DragSelection> drag) {
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
    public Option<DragSelection> Drag { get; }

    public static Fin<Acquire> Of(
        AcquireIntent intent,
        string prompt,
        AcceptPlan accept,
        Option<string> promptDefault = default,
        Option<InputDefault> @default = default,
        Option<OptionSet> options = default,
        Option<DragSelection> drag = default) {
        return from admittedIntent in Admit.Need(intent)
               from admittedAccept in Admit.Need(accept)
               from admittedPrompt in Acceptance.Text(prompt)
               from _ in admittedIntent.Admit()
               from __ in guard(options.IsNone || admittedIntent.Admits(RequestColumn.Options), new KernelFault.InvalidInput())
               from ___ in guard(promptDefault.IsNone || admittedIntent.Admits(RequestColumn.PromptDefault), new KernelFault.InvalidInput())
               from ____ in guard(
                   (drag.IsNone || admittedIntent.Admits(RequestColumn.Drag)) && (!admittedIntent.NeedsDrag || drag.IsSome),
                   new KernelFault.InvalidInput())
               from _____ in guard(admittedAccept.Rules.ForAll(rule => admittedIntent.Accepts(rule)), new KernelFault.InvalidInput())
               from ______ in promptDefault.TraverseM(value => Acceptance.Text(value).Map(static _ => unit)).As()
                   .Map(static _ => unit)
               from _______ in @default.TraverseM(value => value.Admit()
                       .Bind(_ => guard(admittedIntent.Accepts(value), new KernelFault.InvalidInput()).ToFin())).As()
                   .Map(static _ => unit)
               from complete in admittedAccept.Requiring(terminals: admittedIntent.Terminals)
               select new Acquire(
                   admittedIntent, admittedPrompt, complete, promptDefault, @default, options, drag);
    }
}
```

## [06]-[DRIVE]

`GetterDrive.Run` owns one getter, its drag buffer, and its option lease as ONE ranked `Concern` bracket ladder — three nested `using` scopes spelled as an ordered acquisition fold, acquired in rank order and released in reverse through the one release algebra — and one bounded `foldUntil` consumes option terminals before projecting exactly one final discriminant through the `TerminalRow` table. Modal payloads remain deferred until `Result.Success`, so failed one-shot calls never read uninitialized `out` values. The former `Acquisition.Probe` and its `AcquireState` record are DELETED — a corpus-wide grep found zero consumers of the four host getter-state probes, and a getter-state read a caller needs re-enters as one entry with its consumer.

- Law: a mixed object-and-grip selection destined for a dynamic transform lands in the HOST's own buffer — `DragBuffer.Of` folds a completed `GetObject` into `TransformObjectList.AddObjects(getter, scope.Grips)`, so the census, the arrays, and `GetBoundingBox` all read one drag truth; a per-object re-projection loses the grip-to-owner correspondence the buffer alone carries.
- Law: the drag set is a request column, never a fifth intent — `Acquire.Drag` rides beside `Options` and `Default` under the `RequestColumn.Drag` admission, so the point drive and the transform drive admit one selection through one gate, and a `PointFeedback.Pose` row without a selection refuses on the same line through `NeedsDrag`.
- Law: the two drives consume the buffer differently and the buffer never learns which — `GetTransform` takes it through `AddTransformObjects` and the native getter paints its own feedback, while a `GetPoint` drag drives `UpdateDisplayFeedbackTransform(Transform)` per `PointFeedback.Pose` sample under `DisplayFeedbackEnabled`, re-posing the whole dragged set in one host call.
- Law: `TransformObjectList` is `IDisposable` and dies inside the drive's bracket — `DragCensus` is read off the live buffer at seal time and is the only drag fact reaching `AcquireOutcome`.
- Law: the terminal ladder is a ROW TABLE, not a switch — `TerminalRow` binds each non-value `GetResult` to the `AcquireTerminal` it seals, so a new host terminal is one row. NAMED LOSS: the per-arm `Fin.Fail(detail: raw.ToString())` prose detail — the refusal now carries the typed row key; witness: the `NoResult`/`Miss` arm refuses with the terminal name as its typed detail rather than a hand string.
- Law: `AcquireOutcome` carries two DIFFERENT option facts. `Options` is the touch HISTORY — one `OptionChoice` per cycle, in the order the user drove them, carrying the localized display the host published at that moment — and `Settled` is the state, every bound option's final value read once at seal off the still-live lease. Folding the history latest-wins to recover the settled values re-derives what the snapshot already answers, and an option the user never touched appears in `Settled` alone.
- Law: a `Pose` drive lends `PickContext` and `GetPoint` to `GumballRig.Pick`; move and completion return transform evidence directly from the rig.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<GetResult>]
internal sealed partial class TerminalRow {
    internal static readonly TerminalRow Cancel = new(key: GetResult.Cancel, seal: static () => new AcquireTerminal.Cancelled());
    internal static readonly TerminalRow Nothing = new(key: GetResult.Nothing, seal: static () => new AcquireTerminal.Nothing());
    internal static readonly TerminalRow Undo = new(key: GetResult.Undo, seal: static () => new AcquireTerminal.Undone());
    internal static readonly TerminalRow Timeout = new(key: GetResult.Timeout, seal: static () => new AcquireTerminal.TimedOut());
    internal static readonly TerminalRow ExitRhino = new(key: GetResult.ExitRhino, seal: static () => new AcquireTerminal.Exit());

    [UseDelegateFromConstructor]
    internal partial AcquireTerminal Seal();
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record GetterCycle(
    Seq<OptionChoice> Choices,
    Option<GetResult> Terminal);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Acquisition {
    public static Fin<AcquireOutcome> Get(DocumentSession session, Acquire request) {
        return from _ in guard(RhinoApp.IsOnMainThread, new KernelFault.InvalidContext())
               from target in Admit.Need(session)
               from active in Admit.Need(request)
               from outcome in target.Demand(
                   use: document => active.Intent.Switch(
                       state: (Request: active, Document: document),
                       interactive: static (held, intent) => Interactive(held.Request, intent, held.Document),
                       objects: static (held, intent) => Objects(held.Request, intent.Plan),
                       transform: static (held, intent) => Transform(held.Request, intent.Calculate),
                       modal: static (held, intent) => Modal(held.Request, intent.Input, held.Document)),
                   needs: [SessionNeed.Acquire])
               select outcome;
    }

    private static Fin<AcquireOutcome> Interactive(
        Acquire request,
        AcquireIntent.Interactive intent,
        RhinoDoc document) =>
        GetterDrive.Run(
            request: request,
            create: static () => new GetPoint(),
            prepare: getter => intent.Point.Plan.Apply(
                target: getter, apply: static (rule, target, k) => rule.Apply(target, k)),
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
                .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(raw.ToString())))
                .Bind(row => row.Project(getter, document)));

    private static Fin<AcquireOutcome> Objects(Acquire request, ObjectPlan plan) => GetterDrive.Run(
        request: request,
        create: static () => new GetObject(),
        prepare: getter => plan.Plan.Apply(
            target: getter, apply: static (rule, target, k) => rule.Apply(target, k)),
        receive: (getter, _) => Try.lift(() => Fin.Succ(getter.GetMultiple(plan.Minimum, plan.Maximum))).Run().Bind(static inner => inner),
        project: (getter, raw) => raw is GetResult.Object
            ? Picks.CaptureOwned(references: getter.Objects())
                .Map(static picked => (Acquired)new Acquired.Objects(Picked: picked))
            : Fin.Fail<Acquired>(new KernelFault.InvalidResult(Detail: Some(raw.ToString()))));

    private static Fin<AcquireOutcome> Transform(
        Acquire request,
        Func<RhinoViewport, Point3d, Transform> calculate) => GetterDrive.Run(
        request: request,
        create: () => new TransformGetter(calculate),
        prepare: static _ => Fin.Succ(unit),
        receive: (getter, _) => Try.lift(() => Fin.Succ(getter.GetXform())).Run().Bind(static inner => inner),
        project: (getter, raw) => getter.Fault.Match(
            Some: Fin.Fail<Acquired>,
            None: () => getter.Calculated
                .Map(static value => (Acquired)new Acquired.Xform(Value: value))
                .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(raw.ToString())))));

    private static Fin<AcquireOutcome> Modal(
        Acquire request,
        ModalInput input,
        RhinoDoc document) => input.Switch(
        state: (Request: request, Document: document),
        point: static (held, _) => ModalResult(() => {
            Result native = RhinoGet.GetPoint(held.Request.Prompt, held.Request.Accept.AcceptsNothing, out Point3d value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Point(
                Value: value,
                Evidence: new PointEvidence(None, None, None, [], []))));
        }),
        oneObject: static (held, modal) => ModalResult(() => {
            Result native = RhinoGet.GetOneObject(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Filter.Mask, out ObjRef reference);
            return (native, () => Picks.CaptureOwned([reference])
                .Map(static picked => (Acquired)new Acquired.Objects(Picked: picked)));
        }),
        manyObjects: static (held, modal) => ModalResult(() => {
            Result native = RhinoGet.GetMultipleObjects(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Filter.Mask, out ObjRef[] references);
            return (native, () => Picks.CaptureOwned(references)
                .Map(static picked => (Acquired)new Acquired.Objects(Picked: picked)));
        }),
        text: static (held, modal) => ModalResult(() => {
            string value = modal.Seed;
            Result native = RhinoGet.GetString(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Text(Value: value)));
        }),
        toggle: static (held, modal) => ModalResult(() => {
            bool value = modal.Seed;
            Result native = RhinoGet.GetBool(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Off, modal.On, ref value);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Toggle(Value: value)));
        }),
        number: static (held, modal) => ModalResult(() => {
            double value = modal.Seed;
            Result native = RhinoGet.GetNumber(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value, modal.Lower, modal.Upper);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Number(Value: value)));
        }),
        count: static (held, modal) => ModalResult(() => {
            int value = modal.Seed;
            Result native = RhinoGet.GetInteger(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value, modal.Lower, modal.Upper);
            return (native, () => Fin.Succ<Acquired>(new Acquired.Count(Value: value)));
        }),
        paint: static (held, modal) => Slots.Rgb(shade: modal.Seed).Bind(seed => ModalResult(() => {
            System.Drawing.Color value = seed;
            Result native = RhinoGet.GetColor(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value);
            return (native, () => Slots.Shade(color: value)
                .Map(static shade => (Acquired)new Acquired.Paint(Value: shade)));
        })),
        distance: static (held, modal) => ModalResult(() => {
            Result native = RhinoGet.GetDistance(held.Request.Prompt, modal.Seed, out double value);
            return (native, () => DocumentSpace.Model.Read(document: held.Document)
                .Map(regime => (Acquired)new Acquired.Distance(Value: value, Unit: regime.Unit)));
        }),
        shape: static (held, modal) => ModalResult(() => modal.Ask.Run()),
        view: static (held, modal) => ModalResult(() => {
            Result native = RhinoGet.GetView(held.Request.Prompt, out RhinoView value);
            return (native, () => modal.Project(InputMap.Fact(value)));
        }),
        viewports: static (held, modal) => ModalResult(() => {
            Result native = RhinoGet.GetViewports(held.Request.Prompt, out RhinoViewport[] value);
            return (native, () => modal.Project(toSeq(value).Map(static row => InputMap.Fact(row)).Strict()));
        }),
        file: static (held, modal) => Try.lift(() => {
            string value = modal.Title.Match(
                Some: caption => RhinoGet.GetFileName(modal.Ask.Key, modal.DefaultName, caption, parent: null),
                None: () => RhinoGet.GetFileNameScripted(modal.Ask.Key, modal.DefaultName));
            return string.IsNullOrWhiteSpace(value)
                ? Fin.Succ(Outcome(new AcquireTerminal.Cancelled()))
                : Fin.Succ(Outcome(new AcquireTerminal.Value(new Acquired.FileName(Value: value))));
        }).Run().Bind(static inner => inner));

    private static Fin<AcquireOutcome> ModalResult(Func<(Result Native, Func<Fin<Acquired>> Project)> run) => Try.lift(() => {
        (Result native, Func<Fin<Acquired>> project) = run();
        return native switch {
            Result.Success => project().Map(payload => Outcome(new AcquireTerminal.Value(Payload: payload))),
            Result.Cancel => Fin.Succ(Outcome(new AcquireTerminal.Cancelled())),
            Result.Nothing => Fin.Succ(Outcome(new AcquireTerminal.Nothing())),
            Result.ExitRhino => Fin.Succ(Outcome(new AcquireTerminal.Exit())),
            _ => Fin.Fail<AcquireOutcome>(new KernelFault.InvalidResult(Detail: Some(native.ToString()))),
        };
    }).Run().Bind(static inner => inner);

    private static AcquireOutcome Outcome(AcquireTerminal terminal) =>
        new(Terminal: terminal, Options: [], Settled: [], GotDefault: false, Dragged: None);
}

internal static class GetterDrive {
    internal static Fin<AcquireOutcome> Run<TGetter>(
        Acquire request,
        Func<TGetter> create,
        Func<TGetter, Fin<Unit>> prepare,
        Func<TGetter, Option<DragBuffer>, Fin<GetResult>> receive,
        Func<TGetter, GetResult, Fin<Acquired>> project)
        where TGetter : GetBaseClass => Try.lift(() => {
            TGetter getter = create();
            Fin<AcquireOutcome> outcome =
                (from _ in Try.lift(() => {
                     getter.SetCommandPrompt(request.Prompt);
                     _ = request.PromptDefault.Iter(value => getter.SetCommandPromptDefault(value));
                     return Fin.Succ(unit);
                 }).Run().Bind(static inner => inner)
                 from __ in request.Default
                     .TraverseM(value => value.Apply(getter, op))
                     .As()
                     .Map(static _ => unit)
                 from ___ in request.Accept.Apply(getter, op)
                 from ____ in prepare(getter)
                 from outcome in Dragged(request.Drag, op, dragging =>
                     dragging.Map(buffer => buffer.Bind(getter)).IfNone(Fin.Succ(unit))
                         .Bind(_ => request.Options.Match(
                             Some: options => options.Bind(getter, op),
                             None: static () => Fin.Succ(new OptionLease())))
                         .Bind(lease => Cycle(request, getter, dragging, receive, project, lease, op)
                             .Settled(held: Seq(lease), release: held => held.Release())))
                 select outcome)
                .Settled(
                    held: Seq(getter),
                    release: held => Try.lift(() => { held.Dispose(); return Fin.Succ(unit); }).Run().Bind(static inner => inner));
            return outcome;
        }).Run().Bind(static inner => inner);

    private static Fin<AcquireOutcome> Dragged(
        Option<DragSelection> plan,
        Func<Option<DragBuffer>, Fin<AcquireOutcome>> body) => plan.Match(
        Some: row => DragBuffer.Of(row, op).Bind(buffer =>
            body(Some(buffer))
                .Settled(
                    held: Seq(buffer),
                    release: held => Try.lift(() => { held.Dispose(); return Fin.Succ(unit); }).Run().Bind(static inner => inner))),
        None: () => body(None));

    private static Fin<AcquireOutcome> Cycle<TGetter>(
        Acquire request,
        TGetter getter,
        Option<DragBuffer> dragging,
        Func<TGetter, Option<DragBuffer>, Fin<GetResult>> receive,
        Func<TGetter, GetResult, Fin<Acquired>> project,
        OptionLease lease)
        where TGetter : GetBaseClass =>
        Prelude.Range(0, request.Accept.OptionBudget + 1)
            .FoldUntil(
                Fin.Succ(new GetterCycle(Choices: [], Terminal: None)),
                (state, _) => state.Bind(cycle => receive(getter, dragging).Bind(raw => raw is GetResult.Option
                    ? lease.Selected(getter, op).Map(choice => cycle with { Choices = cycle.Choices.Add(choice) })
                    : Fin.Succ(cycle with { Terminal = Some(raw) }))),
                pair => pair.State.Match(Succ: static cycle => cycle.Terminal.IsSome, Fail: static _ => true))
            .Bind(cycle => cycle.Terminal.ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(nameof(AcceptPlan.OptionBudget))))
                .Bind(raw => TerminalRow.TryGet(raw, out TerminalRow? row)
                    ? Sealed(row.Seal(), getter, cycle.Choices, lease, dragging, op)
                    : raw is GetResult.NoResult or GetResult.Miss
                        ? Fin.Fail<AcquireOutcome>(new KernelFault.InvalidResult(Detail: Some(raw.ToString())))
                        : project(getter, raw).Bind(payload => Sealed(
                            new AcquireTerminal.Value(Payload: payload), getter, cycle.Choices, lease, dragging, op))));

    private static Fin<AcquireOutcome> Sealed(
        AcquireTerminal terminal,
        GetBaseClass getter,
        Seq<OptionChoice> choices,
        OptionLease lease,
        Option<DragBuffer> dragging) =>
        from settled in lease.Snapshot()
        from census in dragging.TraverseM(buffer => buffer.Census()).As()
        select new AcquireOutcome(
            Terminal: terminal,
            Options: choices,
            Settled: settled,
            GotDefault: getter.GotDefault(),
            Dragged: census);
}
```

## [07]-[CALLBACK_BOUNDARY]

`PointFeedbackLease` converts every callback into a non-throwing native handler; its first-fault seat is a kernel `Cell.Seat` over one atom, so the losing writer's verdict is READ, never re-derived, and cleanup faults aggregate through the one release algebra. `Subscription` (`Document/lifetime`) owns attachment rollback and complete detachment. `DragBuffer` owns the host transform list end to end: it runs the drag selection, binds a `GetTransform` or arms display feedback for a point drag, applies each `Pose` sample under a `Cell.Commit` tally, and projects its measured census before disposal.

```csharp
// --- [BOUNDARIES] ----------------------------------------------------------------------
internal sealed class DragBuffer : IDisposable {
    private readonly TransformObjectList buffer;
    private readonly DragScope scope;
    private readonly Atom<int> poses = Atom(0);

    private DragBuffer(TransformObjectList buffer, DragScope scope) {
        this.buffer = buffer;
        this.scope = scope;
        this.op = op;
    }

    internal static Fin<DragBuffer> Of(DragSelection plan) => Try.lift(() => {
        using GetObject selection = new();
        selection.SetCommandPrompt(plan.Prompt);
        return plan.Selection.Plan.Apply(
                target: selection, apply: static (rule, target, k) => rule.Apply(target, k))
            .Bind(_ => selection.GetMultiple(plan.Selection.Minimum, plan.Selection.Maximum) is GetResult.Object
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new KernelFault.InvalidResult(Detail: Some(nameof(DragSelection.Selection)))))
            .Bind(_ => Minted(selection, plan.Scope, op));
    }).Run().Bind(static inner => inner);

    private static Fin<DragBuffer> Minted(GetObject selection, DragScope scope) {
        TransformObjectList buffer = new();
        return Admit.Confirm(buffer.AddObjects(selection, scope.Grips) > 0)
            .Map(_ => new DragBuffer(buffer, scope, op))
            .Rollback(release: () => Try.lift(() => { buffer.Dispose(); return Fin.Succ(unit); }).Run().Bind(static inner => inner));
    }

    internal Fin<Unit> Bind(GetBaseClass getter) => Try.lift(() => Fin.Succ(getter switch {
        GetTransform target => HostEdge.Side(() => target.AddTransformObjects(buffer)),
        _ => HostEdge.Side(() => buffer.DisplayFeedbackEnabled = true),
    })).Run().Bind(static inner => inner);

    internal Fin<Unit> Pose(Transform xform) => Admit.Confirm(buffer.UpdateDisplayFeedbackTransform(xform))
        .Map(_ => ignore(Cell.Commit(poses, static held => held + 1)));

    internal Fin<DragCensus> Census() => Try.lift(() => Fin.Succ(new DragCensus(
        Objects: toSeq(buffer.ObjectArray()).Map(static row => row.Id),
        Grips: toSeq(buffer.GripArray()).Map(static row => row.Id),
        GripOwners: toSeq(buffer.GripOwnerArray()).Map(static row => row.Id),
        ObjectCount: buffer.Count,
        GripCount: buffer.GripCount,
        GripOwnerCount: buffer.GripOwnerCount,
        Extent: buffer.GetBoundingBox(regularObjects: true, grips: scope.Grips),
        Poses: poses.Value))).Run().Bind(static inner => inner);

    public void Dispose() => buffer.Dispose();
}

internal sealed class PointFeedbackLease : IDisposable {
    private readonly GetPoint getter;
    private readonly Option<DragBuffer> dragging;
    private readonly Atom<Option<Error>> fault = Atom(Option<Error>.None);
    private readonly Atom<Option<Subscription>> observation = Atom(Option<Subscription>.None);

    private PointFeedbackLease(GetPoint getter, Option<DragBuffer> dragging) {
        this.getter = getter;
        this.dragging = dragging;
        this.op = op;
    }

    internal Option<Error> Fault => fault.Value;

    internal static Fin<PointFeedbackLease> Attach(
        GetPoint getter,
        Seq<PointFeedback> feedback,
        Option<DragBuffer> dragging) {
        PointFeedbackLease lease = new(getter, dragging);
        return Subscription.AttachAll(feedback.Map(row => (Func<Fin<Subscription>>)(() => lease.Wire(row))))
            .Map(attached => {
                _ = Cell.Seat(lease.observation, () => attached);
                return lease;
            });
    }

    private Fin<Subscription> Wire(PointFeedback feedback) => Try.lift(() =>
        feedback.Switch(
            state: this,
            mouseMove: static (lease, row) => lease.Hook<GetPointMouseEventArgs>(
                args => row.Sink(InputMap.Fact(args)),
                handler => lease.getter.MouseMove += handler, handler => lease.getter.MouseMove -= handler),
            mouseDown: static (lease, row) => lease.Hook<GetPointMouseEventArgs>(
                args => row.Sink(InputMap.Fact(args)),
                handler => lease.getter.MouseDown += handler, handler => lease.getter.MouseDown -= handler),
            dynamicDraw: static (lease, row) => lease.Hook<GetPointDrawEventArgs>(row.Sink,
                handler => lease.getter.DynamicDraw += handler, handler => lease.getter.DynamicDraw -= handler),
            postDraw: static (lease, row) => {
                lease.getter.FullFrameRedrawDuringGet = true;
                return lease.Hook<DrawEventArgs>(row.Sink,
                    handler => lease.getter.PostDrawObjects += handler, handler => lease.getter.PostDrawObjects -= handler);
            },
            pose: static (lease, row) => lease.Hook<GetPointMouseEventArgs>(
                args => row.Sink(InputMap.Fact(args)).Bind(lease.Reposed),
                handler => lease.getter.MouseMove += handler, handler => lease.getter.MouseMove -= handler))).Run().Bind(static inner => inner);

    private Fin<Unit> Reposed(Transform xform) => dragging.Match(
        Some: buffer => buffer.Pose(xform),
        None: () => Fin.Fail<Unit>(new KernelFault.InvalidContext()));

    private Fin<Subscription> Hook<TArgs>(
        Func<TArgs, Fin<Unit>> sink,
        Action<EventHandler<TArgs>> attach,
        Action<EventHandler<TArgs>> remove) {
        EventHandler<TArgs> handler = (_, args) => Deliver(() => sink(args));
        return Subscription.Attach(subscribe: attach, unsubscribe: remove, handler: handler);
    }

    private void Deliver(Func<Fin<Unit>> effect) {
        if (fault.Value.IsSome) return;
        _ = Try.lift(effect).Run().Bind(static inner => inner).Match(
            Succ: static _ => unit,
            Fail: error => {
                Fin<Unit> interrupted = Try.lift(() => Fin.Succ(ignore(getter.InterruptMouseMove()))).Run().Bind(static inner => inner);
                Error seated = interrupted.Match(
                    Succ: _ => error,
                    Fail: interrupt => error + interrupt);
                _ = Cell.Seat(fault, () => seated);
                return unit;
            });
    }

    public void Dispose() {
        Transition<Option<Subscription>> drained = Cell.Take(observation);
        if (drained is not Transition<Option<Subscription>>.Committed { State: var taken } || taken.IsNone) return;
        _ = taken.Iter(attached => {
            if (attached.Close() is SubscriptionRelease.Faulted failed) {
                Error cleanup = Error.Many(failed.Errors);
                _ = Cell.Commit(fault, current => Some(current.Match(
                    Some: primary => primary + cleanup,
                    None: () => cleanup)));
            }
        });
    }
}

internal sealed class TransformGetter(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
    internal Option<Transform> Calculated { get; private set; }
    internal Option<Error> Fault { get; private set; }

    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) {
        if (Fault.IsSome) return Transform.Unset;
        return Try.lift(() => Fin.Succ(calculate(viewport, point))).Run().Bind(static inner => inner).Match(
            Succ: value => {
                if (!value.IsValid) {
                    Fault = Some(new KernelFault.InvalidResult(Detail: Some(nameof(Transform))));
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

`AcquireIntent` is the sole modality entry, `AcquireTerminal` is the sole control egress, and `Acquired` is the sole value egress. `OptionLease`, `PointFeedbackLease`, `DragBuffer` with its `TransformObjectList`, `GetBaseClass`, `ObjRef`, and every one-shot `out` value terminate before the outcome crosses the session boundary; `DragCensus` is the dragged set's detached census, and `Acquired.Shape`'s recovered form is the ONE owned lease an outcome carries — the same consumer-custody posture `PickCapture`'s retained geometry holds, stated here rather than smuggled.

A caller-supplied delegate takes a detached fact, never a host handle: `GetPointFact` for the pointer arms, `ViewportFact` for both view asks, `ObjectKinds` for the object filters. Three carves stand and each is named rather than tolerated:

- Carve: the pair of DRAW arms take `GetPointDrawEventArgs`/`DrawEventArgs`, whose live `DisplayPipeline` is the whole point of a draw sink — a callback-scoped borrow, never retained past the crossing, and the only host event type this page's public delegates admit.
- Carve: `AcquireIntent.Transform` takes `Func<RhinoViewport, Point3d, Transform>`, a live host viewport into caller code. The host's own `GetTransform.CalculateTransform` override has that shape and nothing detached can replace it — the transform is computed FROM the viewport's camera each mouse sample, so a `ViewportFact` would answer a stale frame. The borrow is bounded by `TransformGetter`'s override body.
- Carve: `ObjectRule.Filter` takes the host `GetObjectGeometryFilter`, whose delegate receives a live `RhinoObject`, its `GeometryBase`, and a `ComponentIndex` per candidate inside the host's own pick loop. The borrow is bounded by `SetCustomGeometryFilter`'s call window, which ends when the getter disposes.

`System.Drawing.Point` on `Acquired.ScreenPoint` is the last host struct on the surface — a SCREEN frame the kernel does not model, terminating on the detached fact. Colour gets no carve: `PerceptualColor` is the kernel owner, `Slots.Shade`/`Slots.Rgb` are the only two boundaries a host colour crosses, and the egress REFUSES out-of-gamut rather than clipping.

The command-thread carve: `RhinoApp.IsOnMainThread` at the `Get` entry is Rhino's COMMAND-thread affinity — a different axis than the kernel marshal, whose `UiThread`/`UiDispatch` owner sits at S0 below this page; the getter loop runs on the command thread the host owns, and the kernel dispatch never substitutes for it.

Unit identity crosses as the kernel `ModelUnit` and nothing else: `UnitSystem`, `LengthUnit`, and a raw meters-per-unit factor each re-open on egress an admission the kernel already gated. This page RESOLVES a regime and never converts between two — `ModelUnit.ScaleTo` owns a cross-regime rescale, at the consumer that owns the target.

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-commands.md` — the `GetPoint`/`GetObject`/`GetTransform` custom-get family this pipeline brackets); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — the `[SmartEnum]` rule/constraint rosters, `[ComplexValueObject]` carriers, `[ObjectFactory]` grammar admission); `Riok.Mapperly` (`libs/dotnet/.api/api-mapperly.md` — the `[Mapper]` option projection); `Generator.Equals` (`libs/dotnet/.api/api-generator-equals.md` — `[Equatable]` structural equality); `NodaTime` (`libs/dotnet/.api/api-nodatime.md` — the acquisition instant); kernel `Domain/results` + `Numerics/atoms`.

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
