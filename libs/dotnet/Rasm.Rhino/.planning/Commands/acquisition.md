# [RASM_RHINO_ACQUISITION]

`Acquisition.Get` interprets one admitted `Acquire` value inside a document acquire grant. Custom getters, options, point callbacks, modal routes, and native references remain scoped to that window; egress is one detached `AcquiredReceipt`. The page also declares the folder's ONE rule-roster spine: `RulePlan<TRule, TSlot>` carries the roster, the one-per-slot admission, and the two folds every rule family on this page — and the selection page's pick rules and the options page's option roster — compose instead of re-spelling.

## [01]-[INDEX]

- [02]-[PAYLOAD]: `Acquired`, `AcquireTerminal`, `AcquiredReceipt`, `DragCensus`, the `ISlotted<TSlot>` contract, and the `RulePlan<TRule, TSlot>` spine.
- [03]-[ACCEPTANCE]: `AcceptSlot`, `AcceptGate`, `AcceptRule`, and `AcceptPlan` — the parameterless accept calls, the presence-cased modalities, and the terminal-derived widening.
- [04]-[POINT_ALGEBRA]: `PickOff`, `PointConstraint`, `PointGate`, `PointerShape`, `SnapBarSpan`, `SnapBarAxis`, `ArrowSense`, `BaseTrait`, `PointSlot`, `PointRule`, `PointerKey`, `PointFeedback`, `GetPointFact`, and `PointPlan`.
- [05]-[REQUEST]: `TextMeaning`, `PromptCase`, `ObjectGate`/`ObjectSlot`/`ObjectRule`/`ObjectPlan`, `DragSelection`, `ShapeAsk`, `FileAsk`, `ModalInput`, `RequestColumn`, `AcquireIntent`, `InputDefault`, `ViewportFact`, `InputMap`, and `Acquire`.
- [06]-[DRIVE]: `Acquisition.Get`, the `Concern` bracket ladder, and the `GetterDrive` option-cycle fold.
- [07]-[CALLBACK_BOUNDARY]: `DragBuffer`, `PointFeedbackLease`, and `TransformGetter`.
- [08]-[BOUNDARY]: the modality, custody, and unit-identity carves.
- [09]-[RESEARCH]: open verification rows.

## [02]-[PAYLOAD]

`Acquired` closes interactive, screen-space, scalar, object, geometry, view, transform, and file payloads at THIRTEEN cases. The seven single-field geometry wrappers collapse onto ONE `Shape` case whose payload is the kernel form recovery's own answer — `Lease<GeometryBase>`, owned by the receipt's consumer — because every producer already erased the static shape type into `Func<Fin<Acquired>>` and no consumer dispatched on it. NAMED LOSS: the per-shape static payload type; witness: `ShapeAsk.Segment` projected `new Acquired.Segment(Line)` and now projects `value.GeometryForm(key)` into `Shape`, and the one former consumer read the receipt's payload erased either way. `AcquireTerminal` preserves every non-fault control terminal, including native timeout. `DragCensus` (RENAMED from `DragEvidence` — the kernel `Interaction/input` owns `DragEvidence` as the pointer-slop fact and one assembly resolves bare names, E-R31) detaches the drag buffer's object, grip, and owner census with its measured extent and applied-pose count, so the host buffer itself dies inside the drive.

`Acquired.Distance` pairs its magnitude with the kernel `ModelUnit` read off the document at parse time — the pairing `Document/session.md`'s `UnitText.LengthValueCase` already detaches on. Receipts outlive their acquire window, so a `UnitRegime` change between acquisition and use re-reads a bare magnitude in a regime that no longer produced it; consumers re-entering the value rescale through `ModelUnit.ScaleTo`, the branch's one scale owner. `Acquired.Angle` carries no regime: radians ARE the canonical measure its name spells, `AngleGrammar` owns the degree/radian dialect on the TEXT side alone, and a regime column there names a fact no document holds.

`Acquired.Paint` carries the kernel `PerceptualColor`, admitted through `Slots.Shade` at the getter seam and quantized back through `Slots.Rgb` — which now composes the kernel `ToDrawing` egress and REFUSES an out-of-gamut colour instead of hand-building `Color.FromArgb` off the clipping byte leg; the shim that bypassed the kernel's refusal is deleted and every write seam rides the rail. `Acquired.Objects` carries the selection page's own `PickReceipt` — survivors, named casualties, and the participating-getter fact — rather than a bare capture sequence, so a stale reference inside a multi-object pick no longer voids the whole acquisition and the caller reads which references refused. `Acquired.ScreenPoint` keeps `System.Drawing.Point` — a screen struct IS the host's pixel frame and has no kernel counterpart — and that carve is confined to the detached fact: no operation on this page reads it back into a host call.

`RulePlan<TRule, TSlot>` is the folder's rule-roster spine (E-R30): five owners spelled `Seq<TRule>` + one-per-slot + `Traverse`-admit + apply independently — `AcceptPlan`, `PointPlan`, `ObjectPlan` here, `PickPolicy` on the selection page, `OptionSet` on the options page — and the spine owns the roster, the null screen, the slot-injectivity gate with its stated exemption, and the two folds; each family keeps only its typed wrapper and its own apply delegate. `ISlotted<TSlot>` types the slot identity: the erased `object SlotKey` compared through `object.Equals` deletes, and each family declares the closed slot vocabulary its knobs actually address.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
// No `using System.Drawing` and no `using Rhino.UI`: `System.Drawing.Point`/`Color`, `Rhino.UI.CursorStyle`, and
// `Rhino.UI.LocalizeStringPair` spell in full at their few seams, so `Point`, `Color`, and the kernel colour owner
// each resolve to exactly one type on this page.
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

// --- [TYPES] ------------------------------------------------------------------------------
// The typed one-per-slot contract every rule family composes: the slot vocabulary is the family's own closed
// roster, so injectivity compares generated rows and the erased `object.Equals` identity probe is gone.
public interface ISlotted<out TSlot> where TSlot : notnull {
    TSlot SlotKey { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Acquired {
    private Acquired() { }
    public sealed record Point(Point3d Value, PointEvidence Evidence) : Acquired;
    public sealed record ScreenPoint(System.Drawing.Point Value) : Acquired;
    public sealed record Objects(PickReceipt Picked) : Acquired;
    public sealed record Number(double Value) : Acquired;
    public sealed record Count(int Value) : Acquired;
    public sealed record Text(string Value) : Acquired;
    public sealed record Toggle(bool Value) : Acquired;
    public sealed record Paint(PerceptualColor Value) : Acquired;
    public sealed record Distance(double Value, ModelUnit Unit) : Acquired;
    public sealed record Angle(double Radians) : Acquired;
    // ONE geometry case over the kernel form recovery: the lease is the RECEIPT CONSUMER's custody — the recovered
    // form is an owned duplicate the consumer disposes, the same posture `PickCapture`'s retained geometry holds —
    // and the seven per-shape wrappers delete with their unread static types.
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

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PointEvidence(
    Option<uint> ViewSerial,
    Option<int> OsnapCode,
    Option<Point3d> BasePoint,
    Seq<Point3d> SnapPoints,
    Seq<Point3d> ConstructionPoints);

// RENAMED from `DragEvidence`: the kernel input page owns that name for the pointer-slop fact, and this record is
// an eight-column host-buffer CENSUS — different fact, different owner, one assembly resolving bare names.
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
public sealed partial record AcquiredReceipt(
    AcquireTerminal Terminal,
    [property: OrderedEquality] Seq<OptionChoice> Options,
    [property: OrderedEquality] Seq<OptionSetting> Settled,
    bool GotDefault,
    Option<DragCensus> Dragged) : IDetachedDocumentResult {
    public Option<Acquired> Payload => Terminal is AcquireTerminal.Value value ? Some(value.Payload) : None;
}

// The folder's ONE rule-roster spine (E-R30). The roster, the null screen, the slot-injectivity gate, and the two
// folds live here once; a family supplies its slot vocabulary, its admit body, and its apply body. `slotExempt`
// names the one lawful injectivity carve — a rule kind that may repeat (a point getter takes several constraints)
// — so the exemption is an argument a reader sees, never a filter each family re-derives.
public sealed record RulePlan<TRule, TSlot>(Seq<TRule> Rules)
    where TRule : class, ISlotted<TSlot>
    where TSlot : notnull {
    public static Fin<RulePlan<TRule, TSlot>> Of(
        Seq<TRule> rules,
        Func<TRule, Op, Fin<Unit>> admit,
        Op key,
        Option<Func<TRule, bool>> slotExempt = default) {
        Func<TRule, bool> exempt = slotExempt.IfNone(static _ => false);
        Seq<TRule> slotted = rules.Filter(rule => rule is not null && !exempt(rule));
        return from _ in guard(rules.ForAll(static rule => rule is not null), key.InvalidInput()).ToFin()
               from __ in guard(
                   slotted.Map(static rule => rule.SlotKey).Distinct().Count == slotted.Count,
                   key.InvalidInput()).ToFin()
               from ___ in rules.TraverseM(rule => admit(rule, key)).As()
               select new RulePlan<TRule, TSlot>(Rules: rules);
    }

    public Fin<Unit> Apply<TTarget>(TTarget target, Func<TRule, TTarget, Op, Fin<Unit>> apply, Op key) =>
        Rules.TraverseM(rule => apply(rule, target, key)).As().Map(static _ => unit);

    public bool Holds(Func<TRule, bool> probe) => Rules.Exists(probe);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Slots {
    // The kernel colour rail is the page's colour identity, BOTH directions: `Shade` admits the host byte quadruple
    // and `Rgb` composes the kernel `ToDrawing` egress, whose gamut policy REFUSES an out-of-display colour. The
    // prior hand `Color.FromArgb` over the clipping `ToRgb` byte leg silently wrote a colour the kernel refuses to
    // certify — the refusal-bypass is the deleted form and every write seam now rides the rail.
    internal static Fin<PerceptualColor> Shade(System.Drawing.Color color, Op key) =>
        PerceptualColor.OfRgb(color.R, color.G, color.B, alpha: color.A, key: key);

    internal static Fin<System.Drawing.Color> Rgb(PerceptualColor shade, Op key) =>
        shade.ToDrawing(key: key);
}
```

## [03]-[ACCEPTANCE]

`AcceptSlot` is the acceptance family's closed slot vocabulary — one row per physical getter knob — and `AcceptGate` rows carry every parameterless native accept call beside its result terminal and its slot, so acceptance grows by one row, never a new case. `AcceptRule` closes the modalities as PRESENCE cases: `Number` enables numeric acceptance, `Zero` widens it to zero (and refuses without `Number` beside it), `Transparent` enables transparent commands — each case's presence IS the enablement, so the booleans that restated presence as payload delete, and an absent case leaves the host default standing rather than writing it. `WaitFor` carries NodaTime `Duration` — semantic time per the substrate law; the host milliseconds spell once at the apply seam. `Requiring` is the derivation seam: a prompt terminal's required row lands only into an unoccupied slot, so a caller's explicit posture survives admission and the derived row is a default, never an override.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The acceptance knob space as a CLOSED vocabulary: injectivity compares these rows, so two rules addressing one
// physical knob refuse typed and the reflection-type identity the erased contract compared is gone.
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

// PRESENCE is the enablement: an absent case writes nothing, so the host default stands unforged, and `Zero`
// without `Number` refuses at the plan — the widened acceptance cannot outrun the acceptance it widens.
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

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        allowed: static (op, rule) => guard(rule.Gate is not null, op.InvalidInput()).ToFin(),
        number: static (_, _) => Fin.Succ(unit),
        zero: static (_, _) => Fin.Succ(unit),
        transparent: static (_, _) => Fin.Succ(unit),
        waitFor: static (op, rule) => guard(
            rule.Window > Duration.Zero && rule.Window.TotalMilliseconds <= int.MaxValue,
            op.InvalidInput()).ToFin());
}

[ComplexValueObject]
[ValidationError]
public sealed partial class AcceptPlan {
    public RulePlan<AcceptRule, AcceptSlot> Plan { get; }
    public int OptionBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref RulePlan<AcceptRule, AcceptSlot> plan,
        ref int optionBudget) =>
        validationError = plan is null || optionBudget < 0 || optionBudget > 4096
            ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(OptionBudget), optionBudget, "a budget in [0, 4096]" }))
            : plan.Holds(static rule => rule is AcceptRule.Zero)
                && !plan.Holds(static rule => rule is AcceptRule.Number)
                ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(AcceptRule.Zero), "zero acceptance only beside numeric acceptance" }))
                : null;

    public static Fin<AcceptPlan> Of(Seq<AcceptRule> rules, int optionBudget, Op? key = null) {
        Op op = key.OrDefault();
        return RulePlan<AcceptRule, AcceptSlot>.Of(rules: rules, admit: static (rule, k) => rule.Admit(k), key: op)
            .Bind(plan => op.AcceptValidated<AcceptPlan>(
                fault: Validate(plan, optionBudget, out AcceptPlan? admitted), admitted: admitted));
    }

    internal Seq<AcceptRule> Rules => Plan.Rules;

    internal bool AcceptsNothing => Plan.Holds(static rule => rule is AcceptRule.Allowed { Gate.Slot.Key: 0 });

    internal Fin<AcceptPlan> Requiring(Seq<GetResult> terminals, Op key) {
        Seq<AcceptRule> missing = terminals
            .Choose(Derived)
            .Distinct()
            .Filter(row => !Rules.Exists(held => held.SlotKey == row.SlotKey));
        return missing.IsEmpty ? Fin.Succ(value: this) : Of(rules: Rules + missing, optionBudget: OptionBudget, key: key);
    }

    // Acceptance reaches the native getter exactly ONCE, through this fold: the presence cases each write their
    // own knob, `Number` reads whether `Zero` rides beside it, and a second configuration pass re-issuing
    // `AcceptNumber`/`AcceptString`/`AcceptColor` after the plan has run is the deleted form.
    internal Fin<Unit> Apply(GetBaseClass getter, Op key) => Plan.Apply(
        target: getter,
        apply: (rule, target, op) => op.Catch(() => {
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
        }),
        key: key);

    private static Option<AcceptRule> Derived(GetResult terminal) => terminal switch {
        GetResult.Number => Some((AcceptRule)new AcceptRule.Number()),
        GetResult.String => Some((AcceptRule)new AcceptRule.Allowed(Gate: AcceptGate.Text)),
        GetResult.Color => Some((AcceptRule)new AcceptRule.Allowed(Gate: AcceptGate.Color)),
        _ => Option<AcceptRule>.None,
    };
}
```

## [04]-[POINT_ALGEBRA]

`PointConstraint` closes the native constraint family; its four pick-off surfaces carry ONE `PickOff` carrier instead of four raw bools, so the axis has one name and a fifth constrained surface composes the row. `PointRule` parameterizes every independent point-getter setting as data over the closed `PointSlot` vocabulary: `PointGate` becomes a capability vocabulary and the per-gate `(row, bool)` pair collapses onto ONE `Gates(Enabled, Disabled)` rule carrying two disjoint sets, the snap bars carry `SnapBarSpan` rows instead of an `(Enabled, Ends)` pair, the direction arrow carries `ArrowSense`, and the base point carries its two independent affordances as a `CapabilitySet<BaseTrait>`. `PointFeedback` carries rail-returning callbacks whose failures interrupt the native loop and surface after `Get` returns; `Pose` alone returns a value — its `Transform` re-poses the drag buffer through the host's own display-feedback call.

The three pointer arms take `GetPointFact` (RENAMED from `PointerFact` — the kernel `Interaction/input` owns that name and `Display/interaction` carries `ViewportPointerFact`; this is the THIRD spelling and it names its getter frame, E-R31): world point, window point, viewport identity, and the held `CapabilitySet<PointerKey>`, projected at the callback edge so no `GetPointMouseEventArgs` reaches a caller sink. The two DRAW arms keep their host args because a draw sink's whole purpose is the live `DisplayPipeline` the arg carries.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The ONE pick-off carrier the four constrained surfaces share: `Key` is the host bool, the row name is the fact.
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
        onCurve: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.Pick.Key)),
        onSurface: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.Pick.Key)),
        onBrep: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(
            rule.Value, rule.WireDensity, rule.FaceIndex, rule.Pick.Key)),
        onMesh: static (held, rule) => held.Op.Confirm(held.Getter.Constrain(rule.Value, rule.Pick.Key)),
        onConstructionPlane: static (held, rule) => held.Op.Confirm(
            held.Getter.ConstrainToConstructionPlane(rule.ThroughBasePoint)),
        onTargetPlane: static (held, _) => held.Op.Catch(() => {
            held.Getter.ConstrainToTargetPlane();
            return Fin.Succ(unit);
        }),
        onCPlaneIntersection: static (held, rule) => held.Op.Confirm(
            held.Getter.ConstrainToVirtualCPlaneIntersection(rule.Value))));
}

// A capability vocabulary: the getter toggles are combinable membership, so `Gates` carries two SETS and the
// nine per-gate `(row, bool)` rule instances collapse onto one rule a reader can print through `Wire`.
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

// The bar pair `(Enabled, Ends)` spelled three reachable states as four corners; the row set is the three states.
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

    // A thunk because the slot roster and this roster initialize in file order.
    private readonly Func<PointSlot> slot;
    internal PointSlot Slot => slot();

    [UseDelegateFromConstructor]
    internal partial void Set(GetPoint getter, SnapBarSpan span);
}

// `(Enabled, Reverse)` spelled three reachable states as four corners; `Off`/`Forward`/`Reverse` are the states.
[SmartEnum<int>]
public sealed partial class ArrowSense {
    public static readonly ArrowSense Off = new(key: 0, enabled: false, reverse: false);
    public static readonly ArrowSense Forward = new(key: 1, enabled: true, reverse: false);
    public static readonly ArrowSense Reverse = new(key: 2, enabled: true, reverse: true);

    internal bool Enabled { get; }
    internal bool Reverse { get; }
}

// The base point's two independent affordances: every corner is legal, so they ride a set, not two bools.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BaseTrait : ICapability<BaseTrait> {
    public static readonly BaseTrait ShowDistance = new(key: "show-distance");
    public static readonly BaseTrait DrawLine = new(key: "draw-line");
}

// The point-getter knob space as a closed vocabulary: the fixed rules key one row each, the two snap bars key
// their axis rows, and `Constrained` is the stated injectivity EXEMPTION — a getter takes several constraints.
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
    // ONE rule carries the whole gate write: two disjoint sets over the capability vocabulary, so the nine
    // `(row, bool)` instances collapse and a reader prints what the getter was told through two `Wire` reads.
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

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        constrained: static (op, rule) => op.Need(rule.Value).Bind(value => value.Admit(op)),
        snaps: static (op, rule) => guard(!rule.Values.IsEmpty, op.InvalidInput()).ToFin()
            .Bind(_ => PointConstraint.AdmitGeometry(op, [.. rule.Values.Map(static point => point.IsValid)])),
        constructionPoints: static (op, rule) => guard(!rule.Values.IsEmpty, op.InvalidInput()).ToFin()
            .Bind(_ => PointConstraint.AdmitGeometry(op, [.. rule.Values.Map(static point => point.IsValid)])),
        basedAt: static (op, rule) => PointConstraint.AdmitGeometry(op, rule.Value.IsValid),
        radial: static (op, rule) => ValidityClaim.Finite(value: rule.Distance).Holds && rule.Distance >= 0.0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(op.InvalidInput()),
        // `PointerShape` is a sealed generated CLASS keyed on the host ordinal, not an enum — membership is the
        // vocabulary's OWN roster probe; nothing else can construct one, and a null is the only value to refuse.
        cursor: static (op, rule) => guard(
            rule.Value is not null && PointerShape.Items.Contains(rule.Value),
            op.InvalidInput()).ToFin(),
        elevatorMode: static (op, rule) => guard(rule.Mode >= 0, op.InvalidInput()).ToFin(),
        gates: static (op, rule) => guard(
            rule.Enabled.Held.All(row => !rule.Disabled.Admits(capability: row)),
            op.InvalidInput()).ToFin(),
        snapBar: static (op, rule) => guard(rule.Axis is not null && rule.Span is not null, op.InvalidInput()).ToFin(),
        directionArrow: static (op, rule) => guard(rule.Sense is not null, op.InvalidInput()).ToFin(),
        onMouseUp: static (_, _) => Fin.Succ(unit));

    internal Fin<Unit> Apply(GetPoint getter, Op key) => Switch(
        state: (Getter: getter, Op: key),
        constrained: static (held, rule) => rule.Value.Apply(held.Getter, held.Op),
        snaps: static (held, rule) => held.Op.Catch(() => Fin.Succ(ignore(
            held.Getter.AddSnapPoints(points: [.. rule.Values])))),
        constructionPoints: static (held, rule) => held.Op.Catch(() => Fin.Succ(ignore(
            held.Getter.AddConstructionPoints(points: [.. rule.Values])))),
        basedAt: static (held, rule) => held.Op.Catch(() => {
            bool distance = rule.Traits.Admits(capability: BaseTrait.ShowDistance);
            held.Getter.SetBasePoint(rule.Value, distance);
            bool line = rule.Traits.Admits(capability: BaseTrait.DrawLine);
            held.Getter.EnableDrawLineFromPoint(line);
            if (line) held.Getter.DrawLineFromPoint(rule.Value, distance);
            return Fin.Succ(unit);
        }),
        radial: static (held, rule) => held.Op.Catch(() => {
            held.Getter.ConstrainDistanceFromBasePoint(rule.Distance);
            return Fin.Succ(unit);
        }),
        cursor: static (held, rule) => held.Op.Catch(() => { held.Getter.SetCursor(rule.Value.Native); return Fin.Succ(unit); }),
        elevatorMode: static (held, rule) => held.Op.Catch(() => { held.Getter.PermitElevatorMode(rule.Mode); return Fin.Succ(unit); }),
        gates: static (held, rule) => held.Op.Catch(() => {
            rule.Enabled.Held.Iter(row => row.Set(held.Getter, enabled: true));
            rule.Disabled.Held.Iter(row => row.Set(held.Getter, enabled: false));
            return Fin.Succ(unit);
        }),
        snapBar: static (held, rule) => held.Op.Catch(() => { rule.Axis.Set(held.Getter, rule.Span); return Fin.Succ(unit); }),
        directionArrow: static (held, rule) => held.Op.Catch(() => {
            held.Getter.EnableCurveSnapArrow(rule.Sense.Enabled, rule.Sense.Reverse);
            return Fin.Succ(unit);
        }),
        onMouseUp: static (_, _) => Fin.Succ(unit));
}

// The host publishes the pointer flag word as five independent bool reads and keeps its own `MK_*` masks private,
// so the mask is unreachable and the set is rebuilt from the reads. The capability vocabulary restores it: the
// fact carries ONE `CapabilitySet<PointerKey>`, a sink tests membership, and a new host flag is one row.
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

    internal Fin<Unit> Admit(Op key) => Switch(
        mouseMove: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        mouseDown: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        dynamicDraw: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        postDraw: row => guard(row.Sink is not null, key.InvalidInput()).ToFin(),
        pose: row => guard(row.Sink is not null, key.InvalidInput()).ToFin());
}

// --- [MODELS] -----------------------------------------------------------------------------
// RENAMED from `PointerFact` (E-R31): the kernel input page owns that name for the Eto pointer evidence and
// `Display/interaction` carries `ViewportPointerFact`; this fact names its GETTER frame — world point, window
// point, viewport identity — which neither sibling carries. `GetPointMouseEventArgs.Viewport` mints a NON-OWNING
// `RhinoViewport` over the callback's native pointer, so `InputMap` reads its identity once and detaches the arg.
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

    // The spine owns the roster gates; `Constrained` is the stated injectivity exemption — a getter composes
    // several constraints — and it is an ARGUMENT here rather than a filter re-derived at the check site.
    public static Fin<PointPlan> Of(Seq<PointFeedback> feedback, Op? key = null, params ReadOnlySpan<PointRule> rules) {
        Op op = key.OrDefault();
        return from admitted in RulePlan<PointRule, PointSlot>.Of(
                   rules: toSeq(rules.ToArray()),
                   admit: static (rule, k) => rule.Admit(k),
                   key: op,
                   slotExempt: Some<Func<PointRule, bool>>(static rule => rule is PointRule.Constrained))
               from _ in feedback.TraverseM(row => row.Admit(op)).As()
               select new PointPlan(plan: admitted, feedback: feedback);
    }

    internal bool OnMouseUp => Plan.Holds(static rule => rule is PointRule.OnMouseUp);
}
```

## [05]-[REQUEST]

`PromptCase` generates the interactive value space over one `GetPoint`; multiple distinct terminal cases compose number, text, color, 3D point, and 2D point acquisition without getter-specific helper classes. `Acquire.Of` admits each option, prompt default, typed default, drag selection, and accept terminal against the selected `AcquireIntent` through the one `RequestColumn` fold, so no configured column outruns its intent, and it closes the loop the other way through `AcceptPlan.Requiring`: each prompt case's `Terminal` derives the accept row it needs and that row folds into the plan ONLY where the caller left the slot empty.

`ObjectPlan`, `ModalInput`, `DragSelection`, and `AcquireIntent` close the remaining custom and one-shot routes; `ShapeAsk` rows carry the parameterless one-shot shape getters as data projecting through the kernel form recovery, so a new native shape is one row. `DragSelection` (RENAMED from `DragPlan` — the kernel `Interaction/transfer` owns `DragPlan` for the transfer payload; this selects DOCUMENT OBJECTS into a `TransformObjectList`, E-R31) carries the drag prompt, its object plan, and its scope.

The modal object asks take `Document`'s `ObjectKinds`, never a raw `ObjectType`. The view asks project `ViewportFact` at the seam. `FileAsk` keys the host's sparse `GetFileNameMode` roster, and the file ask's `Option<string> Title` IS the route discriminant: a caption drives `GetFileName` and its absence drives `GetFileNameScripted`. `InputMap` is the `[Mapper]` seam for the three host-args projections — the getter mouse args, a viewport, and a view — so the detachment correspondence is generated with its derived columns declared, never three hand bodies (`[05]` seam `InputMap`).

The gumball wire closes here: the point getter IS the consumer `Display/interaction`'s gumball surface names — a `PointFeedback.Pose` drive borrows `PickContext`/`GetPoint` from this page and `Gumballs.Configure` returns its evidence on this request rail, so the 251-line gumball surface has its producer and no detached gumball stream exists.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The MEANING is the caller's election — literal, number, length, angle dialect — and cannot be recovered from
// the text, so the roster stays delegate rows and the `[ObjectFactory<string>]` move is REFUTED here: that plane
// binds ONE grammar per owner, and five grammars under one owner would re-mint the dialect knob as a wire prefix.
[SmartEnum<int>]
public sealed partial class TextMeaning {
    public static readonly TextMeaning Literal = new(key: 0, parse: static (text, _, _) =>
        Fin.Succ<Acquired>(new Acquired.Text(Value: text)));
    public static readonly TextMeaning Number = new(key: 1, parse: static (text, _, key) => key.Catch(() => {
        StringParserSettings output = StringParserSettings.ParseSettingsDoubleNumber;
        int consumed = StringParser.ParseNumber(
            text, 0, StringParserSettings.ParseSettingsDoubleNumber, ref output, out double value);
        return consumed == text.Length && ValidityClaim.Finite(value: value).Holds
            ? Fin.Succ<Acquired>(new Acquired.Number(Value: value))
            : Fin.Fail<Acquired>(key.InvalidInput());
    }));
    // Length text crosses through `UnitText` (session.md), the branch's ONE length-correspondence owner: it holds
    // the dialect roster, the whole-string gate, the `LengthValue` disposal bracket, and the regime pairing this
    // receipt detaches. Re-spelling that parse here forked the gate and re-derived the regime beside its owner.
    public static readonly TextMeaning Length = new(key: 2, parse: static (text, document, key) =>
        from regime in DocumentSpace.Model.Read(document: document, op: key)
        from encoded in UnitText.Length(text: text, key: key)
        from crossed in encoded.Cross(regime: regime, key: key)
        from measured in crossed is UnitText.LengthValueCase value
            ? Fin.Succ<Acquired>(value: new Acquired.Distance(Value: value.Value, Unit: value.Unit))
            : Fin.Fail<Acquired>(error: key.InvalidResult())
        select measured);
    // `AngleGrammar` (session.md) owns the degree/radian dialect and lands canonical radians at its own seam.
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

// A capability vocabulary: the object-getter toggles are combinable membership like the point gates.
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

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        preSelect: static (_, _) => Fin.Succ(unit),
        gates: static (op, rule) => guard(
            rule.Enabled.Held.All(row => !rule.Disabled.Admits(capability: row)),
            op.InvalidInput()).ToFin(),
        filter: static (op, rule) => guard(rule.Value is not null, op.InvalidInput()).ToFin());

    internal Fin<Unit> Apply(GetObject getter, Op key) => key.Catch(() => {
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
    });
}

[ComplexValueObject]
[ValidationError]
public sealed partial class ObjectPlan {
    public int Minimum { get; }
    public int Maximum { get; }
    public RulePlan<ObjectRule, ObjectSlot> Plan { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int minimum,
        ref int maximum,
        ref RulePlan<ObjectRule, ObjectSlot> plan) =>
        validationError = plan is null || minimum < 0 || maximum < 0 || (maximum is not 0 && maximum < minimum)
            ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(ObjectPlan), "a rule plan with non-negative bounds whose maximum is zero or at least the minimum" }))
            : null;

    public static Fin<ObjectPlan> Of(int minimum, int maximum, Seq<ObjectRule> rules, Op? key = null) {
        Op op = key.OrDefault();
        return RulePlan<ObjectRule, ObjectSlot>.Of(rules: rules, admit: static (rule, k) => rule.Admit(k), key: op)
            .Bind(plan => op.AcceptValidated<ObjectPlan>(
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

// RENAMED from `DragPlan` (E-R31): the kernel transfer page owns that name for the staged drag PAYLOAD; this
// value selects DOCUMENT OBJECTS into the host's `TransformObjectList` — a different concern under its own name.
[ComplexValueObject]
[ValidationError]
public sealed partial class DragSelection {
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
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(DragSelection), "a prompt, a scope, and a selection admitting at least one object" }));
}

// Each row projects through the kernel form recovery, so the one `Shape` case carries an OWNED recovered form and
// the seven per-shape wrapper cases are gone; the corners of a rectangle land as the closed polyline they bound.
[SmartEnum<int>]
public sealed partial class ShapeAsk {
    public static readonly ShapeAsk Segment = new(key: 0, run: static op =>
        Recovered(RhinoGet.GetLine(out Line value), () => new LineCurve(value).GeometryForm(key: op), op));
    public static readonly ShapeAsk Chain = new(key: 1, run: static op =>
        Recovered(RhinoGet.GetPolyline(out Polyline value), () => value.ToPolylineCurve().GeometryForm(key: op), op));
    public static readonly ShapeAsk ArcShape = new(key: 2, run: static op =>
        Recovered(RhinoGet.GetArc(out Arc value), () => value.GeometryForm(key: op), op));
    public static readonly ShapeAsk CircleShape = new(key: 3, run: static op =>
        Recovered(RhinoGet.GetCircle(out Circle value), () => value.GeometryForm(key: op), op));
    public static readonly ShapeAsk PlaneShape = new(key: 4, run: static op =>
        Recovered(RhinoGet.GetPlane(out Plane value), () => value.GeometryForm(key: op), op));
    public static readonly ShapeAsk RectangleShape = new(key: 5, run: static op =>
        Recovered(RhinoGet.GetRectangle(out Point3d[] value), () => new Polyline([.. value, value[0]]).ToPolylineCurve().GeometryForm(key: op), op));
    public static readonly ShapeAsk BoxShape = new(key: 6, run: static op =>
        Recovered(RhinoGet.GetBox(out Box value), () => value.GeometryForm(key: op), op));

    [UseDelegateFromConstructor]
    internal partial (Result Native, Func<Fin<Acquired>> Project) Run(Op op);

    private static (Result Native, Func<Fin<Acquired>> Project) Recovered(
        Result native, Func<Fin<Lease<GeometryBase>>> recover, Op op) =>
        (native, () => recover().Map(static form => (Acquired)new Acquired.Shape(Form: form)));
}

// `Rhino.Input.Custom.GetFileNameMode` — the host's own file-ask roster, sparse by construction (ordinals 4, 15,
// and 16 carry no row), so the keyed wrap mirrors the live ordinals and an unlisted ordinal refuses at admission.
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
        number: row => guard(
            ValidityClaim.All(
                ValidityClaim.Finite(value: row.Seed),
                ValidityClaim.Finite(value: row.Lower),
                ValidityClaim.Finite(value: row.Upper),
                row.Lower <= row.Seed,
                row.Seed <= row.Upper),
            key.InvalidInput()).ToFin(),
        count: row => guard(row.Lower <= row.Seed && row.Seed <= row.Upper, key.InvalidInput()).ToFin(),
        paint: row => key.Need(row.Seed).Map(static _ => unit),
        distance: row => guard(
            ValidityClaim.All(ValidityClaim.Finite(value: row.Seed), row.Seed >= 0.0),
            key.InvalidInput()).ToFin(),
        shape: row => guard(row.Ask is not null, key.InvalidInput()).ToFin(),
        view: row => guard(row.Project is not null, key.InvalidInput()).ToFin(),
        viewports: row => guard(row.Project is not null, key.InvalidInput()).ToFin(),
        file: row => from _ in guard(row.Ask is not null && row.DefaultName is not null, key.InvalidInput()).ToFin()
                     from __ in row.Title.Traverse(caption => key.AcceptText(caption)).As()
                     select unit);

    internal bool Accepts(AcceptRule rule) =>
        rule is AcceptRule.Allowed { Gate.Slot.Key: 0 }
        && this is Point or OneObject or ManyObjects or Text or Toggle or Number or Count or Paint;
}

// The request columns an intent admits, folded ONCE: `SupportsOptions`, `SupportsPromptDefault`, and
// `SupportsDrag` were three derived predicates two of which restated each other; one admission fold reads the
// column and a new request column is one row plus one arm.
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

    // ONE column fold: a modal intent admits nothing, an object intent admits options and a prompt default, and
    // the drag column rides only the two drives that consume a drag buffer.
    internal bool Admits(RequestColumn column) => Switch(
        state: column,
        interactive: static (_, _) => true,
        objects: static (held, _) => held != RequestColumn.Drag,
        transform: static (held, _) => held != RequestColumn.Options && held != RequestColumn.PromptDefault
            || held == RequestColumn.Options || held == RequestColumn.PromptDefault,
        modal: static (_, _) => false) && this switch {
        Transform when column == RequestColumn.Drag => true,
        Transform => true,
        _ => true,
    };

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
        numberValue: row => guard(ValidityClaim.Finite(value: row.Value).Holds, key.InvalidInput()).ToFin(),
        countValue: static _ => Fin.Succ(unit),
        textValue: row => key.AcceptText(row.Value).Map(static _ => unit),
        paintValue: row => key.Need(row.Value).Map(static _ => unit));

    // The paint seed rides the kernel egress and can REFUSE, so the apply is a rail, not a side write.
    internal Fin<Unit> Apply(GetBaseClass getter, Op key) => Switch(
        state: (Getter: getter, Op: key),
        pointValue: static (held, value) => Fin.Succ(Op.Side(() => held.Getter.SetDefaultPoint(value.Value))),
        numberValue: static (held, value) => Fin.Succ(Op.Side(() => held.Getter.SetDefaultNumber(value.Value))),
        countValue: static (held, value) => Fin.Succ(Op.Side(() => held.Getter.SetDefaultInteger(value.Value))),
        textValue: static (held, value) => Fin.Succ(Op.Side(() => held.Getter.SetDefaultString(value.Value))),
        paintValue: static (held, value) => Slots.Rgb(shade: value.Value, key: held.Op)
            .Map(color => Op.Side(() => held.Getter.SetDefaultColor(color))));
}

// --- [MODELS] -----------------------------------------------------------------------------
// A modal view ask hands back live host views and viewports whose lifetime is the getter call; identity, name,
// and the owning view serial detach at that seam. A detail viewport has no parent view, hence the optional serial.
public sealed record ViewportFact(Guid Id, string Name, Option<uint> ViewSerial);

// The ONE host-args detachment seam ([05] `InputMap`): three generated projections with their derived columns
// declared as rows, so the correspondence is reviewable at the mapper rather than spread over three hand bodies.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class InputMap {
    [MapProperty(nameof(GetPointMouseEventArgs.Point), nameof(GetPointFact.World))]
    [MapProperty(nameof(GetPointMouseEventArgs.WindowPoint), nameof(GetPointFact.Window))]
    [MapperIgnoreSource(nameof(GetPointMouseEventArgs.Viewport))]   // identity read through `ViewportId` below
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
        Option<DragSelection> drag = default,
        Op? key = null) {
        Op op = key.OrDefault(name: nameof(Acquire));
        return from admittedIntent in op.Need(intent)
               from admittedAccept in op.Need(accept)
               from admittedPrompt in op.AcceptText(prompt)
               from _ in admittedIntent.Admit(op)
               from __ in guard(options.IsNone || admittedIntent.Admits(RequestColumn.Options), op.InvalidInput()).ToFin()
               from ___ in guard(promptDefault.IsNone || admittedIntent.Admits(RequestColumn.PromptDefault), op.InvalidInput()).ToFin()
               from ____ in guard(
                   (drag.IsNone || admittedIntent.Admits(RequestColumn.Drag)) && (!admittedIntent.NeedsDrag || drag.IsSome),
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

`GetterDrive.Run` owns one getter, its drag buffer, and its option lease as ONE ranked `Concern` bracket ladder — three nested `using` scopes spelled as an ordered acquisition fold, acquired in rank order and released in reverse through the one release algebra — and one bounded `foldUntil` consumes option terminals before projecting exactly one final discriminant through the `TerminalRow` table. Modal payloads remain deferred until `Result.Success`, so failed one-shot calls never read uninitialized `out` values. The former `Acquisition.Probe` and its `AcquireState` record are DELETED — a corpus-wide grep found zero consumers of the four host getter-state probes, and a getter-state read a caller needs re-enters as one entry with its consumer.

- Law: a mixed object-and-grip selection destined for a dynamic transform lands in the HOST's own buffer — `DragBuffer.Of` folds a completed `GetObject` into `TransformObjectList.AddObjects(getter, scope.Grips)`, so the census, the arrays, and `GetBoundingBox` all read one drag truth; a per-object re-projection loses the grip-to-owner correspondence the buffer alone carries.
- Law: the drag set is a request column, never a fifth intent — `Acquire.Drag` rides beside `Options` and `Default` under the `RequestColumn.Drag` admission, so the point drive and the transform drive admit one selection through one gate, and a `PointFeedback.Pose` row without a selection refuses on the same line through `NeedsDrag`.
- Law: the two drives consume the buffer differently and the buffer never learns which — `GetTransform` takes it through `AddTransformObjects` and the native getter paints its own feedback, while a `GetPoint` drag drives `UpdateDisplayFeedbackTransform(Transform)` per `PointFeedback.Pose` sample under `DisplayFeedbackEnabled`, re-posing the whole dragged set in one host call.
- Law: `TransformObjectList` is `IDisposable` and dies inside the drive's bracket — `DragCensus` is read off the live buffer at seal time and is the only drag fact reaching `AcquiredReceipt`.
- Law: the terminal ladder is a ROW TABLE, not a switch — `TerminalRow` binds each non-value `GetResult` to the `AcquireTerminal` it seals, so a new host terminal is one row. NAMED LOSS: the per-arm `Fin.Fail(detail: raw.ToString())` prose detail — the refusal now carries the typed row key; witness: the `NoResult`/`Miss` arm refuses with the terminal name as its typed detail rather than a hand string.
- Law: a receipt carries two DIFFERENT option facts. `Options` is the touch HISTORY — one `OptionChoice` per cycle, in the order the user drove them, carrying the localized display the host published at that moment — and `Settled` is the state, every bound option's final value read once at seal off the still-live lease. Folding the history latest-wins to recover the settled values re-derives what the snapshot already answers, and an option the user never touched appears in `Settled` alone.
- Law: the point getter is the gumball surface's producer — a `Pose` drive borrows `PickContext`/`GetPoint` from this page and `Gumballs.Configure` (`Display/interaction`) returns its evidence on this request rail, so no detached gumball stream exists.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Non-value terminals as ROWS: the seal ladder reads the table, a new host terminal is one row, and the refusal
// for an unmapped result carries the row key typed rather than a hand-spelled detail string.
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

// --- [MODELS] -----------------------------------------------------------------------------
internal sealed record GetterCycle(
    Seq<OptionChoice> Choices,
    Option<GetResult> Terminal);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Acquisition {
    public static Fin<AcquiredReceipt> Get(DocumentSession session, Acquire request, Op? key = null) {
        Op op = key.OrDefault();
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
        GetterDrive.Run(
            request: request,
            create: static () => new GetPoint(),
            prepare: getter => intent.Point.Plan.Apply(
                target: getter, apply: static (rule, target, k) => rule.Apply(target, k), key: op),
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
            op: op);

    private static Fin<AcquiredReceipt> Objects(Acquire request, ObjectPlan plan, Op op) => GetterDrive.Run(
        request: request,
        create: static () => new GetObject(),
        prepare: getter => plan.Plan.Apply(
            target: getter, apply: static (rule, target, k) => rule.Apply(target, k), key: op),
        receive: (getter, _) => op.Catch(() => Fin.Succ(getter.GetMultiple(plan.Minimum, plan.Maximum))),
        // The object getter is the PRODUCER `Commands/selection` names: `CaptureOwned` answers the page's own
        // `PickReceipt` — survivors, named casualties, and the participating-getter fact — and this payload is
        // where that receipt reaches its reader.
        project: (getter, raw) => raw is GetResult.Object
            ? Picks.CaptureOwned(references: getter.Objects(), key: op)
                .Map(static picked => (Acquired)new Acquired.Objects(Picked: picked))
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
                .Map(static picked => (Acquired)new Acquired.Objects(Picked: picked)));
        }),
        manyObjects: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetMultipleObjects(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, modal.Filter.Mask, out ObjRef[] references);
            return (native, () => Picks.CaptureOwned(references, held.Op)
                .Map(static picked => (Acquired)new Acquired.Objects(Picked: picked)));
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
        // The seed rides the kernel egress and its gamut refusal SURFACES — no clipped byte write.
        paint: static (held, modal) => Slots.Rgb(shade: modal.Seed, key: held.Op).Bind(seed => ModalResult(held.Op, () => {
            System.Drawing.Color value = seed;
            Result native = RhinoGet.GetColor(
                held.Request.Prompt, held.Request.Accept.AcceptsNothing, ref value);
            return (native, () => Slots.Shade(color: value, key: held.Op)
                .Map(static shade => (Acquired)new Acquired.Paint(Value: shade)));
        })),
        // GetDistance resolves in the document's own regime, so the projection reads that regime through the same
        // `DocumentSpace.Model` owner the text route reads — the modal route detaches the identical pairing.
        distance: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetDistance(held.Request.Prompt, modal.Seed, out double value);
            return (native, () => DocumentSpace.Model.Read(document: held.Document, op: held.Op)
                .Map(regime => (Acquired)new Acquired.Distance(Value: value, Unit: regime.Unit)));
        }),
        shape: static (held, modal) => ModalResult(held.Op, () => modal.Ask.Run(held.Op)),
        view: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetView(held.Request.Prompt, out RhinoView value);
            return (native, () => modal.Project(InputMap.Fact(value)));
        }),
        viewports: static (held, modal) => ModalResult(held.Op, () => {
            Result native = RhinoGet.GetViewports(held.Request.Prompt, out RhinoViewport[] value);
            return (native, () => modal.Project(toSeq(value).Map(static row => InputMap.Fact(row)).Strict()));
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
        new(Terminal: terminal, Options: [], Settled: [], GotDefault: false, Dragged: None);
}

internal static class GetterDrive {
    // The three-deep `using` tower — getter, drag buffer, option lease — is ONE ranked bracket ladder: concerns
    // acquire in rank order, the body runs under all of them, and release runs in reverse through the one release
    // algebra so a mid-ladder refusal frees exactly what it acquired ([PRECEDENCE_TABLE]).
    internal static Fin<AcquiredReceipt> Run<TGetter>(
        Acquire request,
        Func<TGetter> create,
        Func<TGetter, Fin<Unit>> prepare,
        Func<TGetter, Option<DragBuffer>, Fin<GetResult>> receive,
        Func<TGetter, GetResult, Fin<Acquired>> project,
        Op op)
        where TGetter : GetBaseClass => op.Catch(() => {
            TGetter getter = create();
            Fin<AcquiredReceipt> outcome =
                (from _ in op.Catch(() => {
                     getter.SetCommandPrompt(request.Prompt);
                     _ = request.PromptDefault.Iter(value => getter.SetCommandPromptDefault(value));
                     return Fin.Succ(unit);
                 })
                 from __ in request.Default.Match(
                     Some: value => value.Apply(getter, op),
                     None: static () => Fin.Succ(unit))
                 from ___ in request.Accept.Apply(getter, op)
                 from ____ in prepare(getter)
                 from receipt in Dragged(request.Drag, op, dragging =>
                     dragging.Map(buffer => buffer.Bind(getter)).IfNone(Fin.Succ(unit))
                         .Bind(_ => request.Options.Match(
                             Some: options => options.Bind(getter, op),
                             None: static () => Fin.Succ(new OptionLease())))
                         .Bind(lease => Cycle(request, getter, dragging, receive, project, lease, op)
                             .Settled(held: Seq(lease), release: held => held.Release(op), key: op)))
                 select receipt)
                .Settled(
                    held: Seq(getter),
                    release: held => op.Catch(() => { held.Dispose(); return Fin.Succ(unit); }),
                    key: op);
            return outcome;
        });

    private static Fin<AcquiredReceipt> Dragged(
        Option<DragSelection> plan,
        Op op,
        Func<Option<DragBuffer>, Fin<AcquiredReceipt>> body) => plan.Match(
        Some: row => DragBuffer.Of(row, op).Bind(buffer =>
            body(Some(buffer))
                .Settled(
                    held: Seq(buffer),
                    release: held => op.Catch(() => { held.Dispose(); return Fin.Succ(unit); }),
                    key: op)),
        None: () => body(None));

    // One bounded halting fold: `foldUntil` stops on the first non-option terminal or a spent budget, so no
    // monadic index walk and no done-flag exists beside the fold's own halt predicate.
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
            .foldUntil(
                Fin.Succ(new GetterCycle(Choices: [], Terminal: None)),
                (state, _) => state.Bind(cycle => receive(getter, dragging).Bind(raw => raw is GetResult.Option
                    ? lease.Selected(getter, op).Map(choice => cycle with { Choices = cycle.Choices.Add(choice) })
                    : Fin.Succ(cycle with { Terminal = Some(raw) }))),
                state => state.Match(Succ: static cycle => cycle.Terminal.IsSome, Fail: static _ => true))
            .Bind(cycle => cycle.Terminal.ToFin(Fail: op.InvalidResult(detail: nameof(AcceptPlan.OptionBudget)))
                .Bind(raw => TerminalRow.TryGet(raw, out TerminalRow? row)
                    ? Sealed(row.Seal(), getter, cycle.Choices, lease, dragging, op)
                    : raw is GetResult.NoResult or GetResult.Miss
                        ? Fin.Fail<AcquiredReceipt>(op.InvalidResult(detail: raw.ToString()))
                        : project(getter, raw).Bind(payload => Sealed(
                            new AcquireTerminal.Value(Payload: payload), getter, cycle.Choices, lease, dragging, op))));

    // The seal is the ONE read of the settled option state: the lease is still live inside the bracket, every
    // carrier still holds its value, and the choices beside it are the touch HISTORY — a caller folding that
    // history latest-wins to recover the final values re-derives what the snapshot answers directly.
    private static Fin<AcquiredReceipt> Sealed(
        AcquireTerminal terminal,
        GetBaseClass getter,
        Seq<OptionChoice> choices,
        OptionLease lease,
        Option<DragBuffer> dragging,
        Op op) =>
        from settled in lease.Snapshot(op)
        from census in dragging.Match(
            Some: buffer => buffer.Census(op).Map(Some),
            None: static () => Fin.Succ(Option<DragCensus>.None))
        select new AcquiredReceipt(
            Terminal: terminal,
            Options: choices,
            Settled: settled,
            GotDefault: getter.GotDefault(),
            Dragged: census);
}
```

## [07]-[CALLBACK_BOUNDARY]

`PointFeedbackLease` converts every callback into a non-throwing native handler; its first-fault seat is a kernel `Cell.Seat` over one atom, so the losing writer's verdict is READ, never re-derived, and cleanup faults aggregate through the one release algebra. `Subscription` (`Document/lifetime`) owns attachment rollback and complete detachment. `DragBuffer` owns the host transform list end to end: it runs the drag selection, binds a `GetTransform` or arms display feedback for a point drag, applies each `Pose` sample under a `Cell.Commit` tally, and projects its measured census before disposal.

```csharp signature
// --- [BOUNDARIES] -------------------------------------------------------------------------
internal sealed class DragBuffer : IDisposable {
    private readonly TransformObjectList buffer;
    private readonly DragScope scope;
    private readonly Op op;
    // The applied-pose tally rides the kernel cell: a total unconditional increment is a plain commit, and the
    // census reads the atom rather than a `Volatile.Read` over a hand interlocked field.
    private readonly Atom<int> poses = Atom(0);

    private DragBuffer(TransformObjectList buffer, DragScope scope, Op op) {
        this.buffer = buffer;
        this.scope = scope;
        this.op = op;
    }

    internal static Fin<DragBuffer> Of(DragSelection plan, Op op) => op.Catch(() => {
        using GetObject selection = new();                              // Exemption: host getter bracket, the selection never escapes
        selection.SetCommandPrompt(plan.Prompt);
        return plan.Selection.Plan.Apply(
                target: selection, apply: static (rule, target, k) => rule.Apply(target, k), key: op)
            .Bind(_ => selection.GetMultiple(plan.Selection.Minimum, plan.Selection.Maximum) is GetResult.Object
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(op.InvalidResult(detail: nameof(DragSelection.Selection))))
            .Bind(_ => Minted(selection, plan.Scope, op));
    });

    private static Fin<DragBuffer> Minted(GetObject selection, DragScope scope, Op op) {
        TransformObjectList buffer = new();
        // The failure arm releases the stranded mint through the one custody rail — never a tuple-projection
        // dispose that binds release to the expression rather than the failure.
        return op.Confirm(buffer.AddObjects(selection, scope.Grips) > 0)
            .Map(_ => new DragBuffer(buffer, scope, op))
            .Rollback(release: () => op.Catch(() => { buffer.Dispose(); return Fin.Succ(unit); }), key: op);
    }

    internal Fin<Unit> Bind(GetBaseClass getter) => op.Catch(() => Fin.Succ(getter switch {
        GetTransform target => Op.Side(() => target.AddTransformObjects(buffer)),
        _ => Op.Side(() => buffer.DisplayFeedbackEnabled = true),
    }));

    internal Fin<Unit> Pose(Transform xform) => op.Confirm(buffer.UpdateDisplayFeedbackTransform(xform))
        .Map(_ => ignore(Cell.Commit(poses, static held => held + 1)));

    internal Fin<DragCensus> Census(Op key) => key.Catch(() => Fin.Succ(new DragCensus(
        Objects: toSeq(buffer.ObjectArray()).Map(static row => row.Id),
        Grips: toSeq(buffer.GripArray()).Map(static row => row.Id),
        GripOwners: toSeq(buffer.GripOwnerArray()).Map(static row => row.Id),
        ObjectCount: buffer.Count,
        GripCount: buffer.GripCount,
        GripOwnerCount: buffer.GripOwnerCount,
        Extent: buffer.GetBoundingBox(regularObjects: true, grips: scope.Grips),
        Poses: poses.Value)));

    public void Dispose() => buffer.Dispose();
}

internal sealed class PointFeedbackLease : IDisposable {
    private readonly GetPoint getter;
    private readonly Option<DragBuffer> dragging;
    private readonly Op op;
    // First-fault-wins is a SEAT: `Cell.Seat` installs the first refusal and cedes every later one, so the
    // interrupt fold reads its own verdict instead of a swap whose loser cannot tell it lost.
    private readonly Atom<Option<Error>> fault = Atom(Option<Error>.None);
    private readonly Atom<Option<Subscription>> observation = Atom(Option<Subscription>.None);

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
                _ = Cell.Seat(lease.observation, () => attached);
                return lease;
            });
    }

    private Fin<Subscription> Wire(PointFeedback feedback) => op.Catch(() =>
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

    // The first refusal SEATS; the interrupt fault aggregates into it before seating, so the native loop stops
    // once and the surfaced fault carries both the sink's refusal and any interrupt failure.
    private void Deliver(Func<Fin<Unit>> effect) {
        if (fault.Value.IsSome) return;
        _ = op.Catch(effect).Match(
            Succ: static _ => unit,
            Fail: error => {
                Fin<Unit> interrupted = op.Catch(() => Fin.Succ(ignore(getter.InterruptMouseMove())));
                Error seated = interrupted.Match(
                    Succ: _ => error,
                    Fail: interrupt => error + interrupt);
                _ = Cell.Seat(fault, () => seated);
                return unit;
            });
    }

    // Detach is a TAKE: the drained subscription closes once, and its cleanup refusals aggregate onto whatever
    // fault is already seated through the one release algebra rather than a hand fold over the tail.
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

`AcquireIntent` is the sole modality entry, `AcquireTerminal` is the sole control egress, and `Acquired` is the sole value egress. `OptionLease`, `PointFeedbackLease`, `DragBuffer` with its `TransformObjectList`, `GetBaseClass`, `ObjRef`, and every one-shot `out` value terminate before the receipt crosses the session boundary; `DragCensus` is the dragged set's detached census, and `Acquired.Shape`'s recovered form is the ONE owned lease a receipt carries — the same consumer-custody posture `PickCapture`'s retained geometry holds, stated here rather than smuggled.

A caller-supplied delegate takes a detached fact, never a host handle: `GetPointFact` for the pointer arms, `ViewportFact` for both view asks, `ObjectKinds` for the object filters. Three carves stand and each is named rather than tolerated:

- Carve: the pair of DRAW arms take `GetPointDrawEventArgs`/`DrawEventArgs`, whose live `DisplayPipeline` is the whole point of a draw sink — a callback-scoped borrow, never retained past the crossing, and the only host event type this page's public delegates admit.
- Carve: `AcquireIntent.Transform` takes `Func<RhinoViewport, Point3d, Transform>`, a live host viewport into caller code. The host's own `GetTransform.CalculateTransform` override has that shape and nothing detached can replace it — the transform is computed FROM the viewport's camera each mouse sample, so a `ViewportFact` would answer a stale frame. The borrow is bounded by `TransformGetter`'s override body.
- Carve: `ObjectRule.Filter` takes the host `GetObjectGeometryFilter`, whose delegate receives a live `RhinoObject`, its `GeometryBase`, and a `ComponentIndex` per candidate inside the host's own pick loop. The borrow is bounded by `SetCustomGeometryFilter`'s call window, which ends when the getter disposes.

`System.Drawing.Point` on `Acquired.ScreenPoint` is the last host struct on the surface — a SCREEN frame the kernel does not model, terminating on the detached fact. Colour gets no carve: `PerceptualColor` is the kernel owner, `Slots.Shade`/`Slots.Rgb` are the only two seams a host colour crosses, and the egress REFUSES out-of-gamut rather than clipping.

The command-thread carve: `RhinoApp.IsOnMainThread` at the `Get` entry is Rhino's COMMAND-thread affinity — a different axis than the kernel marshal, whose `UiThread`/`UiDispatch` owner sits at S0 below this page; the getter loop runs on the command thread the host owns, and the kernel dispatch never substitutes for it.

Unit identity crosses as the kernel `ModelUnit` and nothing else: `UnitSystem`, `LengthUnit`, and a raw meters-per-unit factor each re-open on egress an admission the kernel already gated. This page RESOLVES a regime and never converts between two — `ModelUnit.ScaleTo` owns a cross-regime rescale, at the consumer that owns the target.

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-commands.md` — the `GetPoint`/`GetObject`/`GetTransform` custom-get family this rail brackets); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — the `[SmartEnum]` rule/constraint rosters, `[ComplexValueObject]` carriers, `[ObjectFactory]` grammar admission); `Riok.Mapperly` (`libs/dotnet/.api/api-mapperly.md` — the `[Mapper]` option projection); `Generator.Equals` (`libs/dotnet/.api/api-generator-equals.md` — `[Equatable]` structural equality); `NodaTime` (`libs/dotnet/.api/api-nodatime.md` — the acquisition instant); kernel `Domain/rails` + `Numerics/atoms`.

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
