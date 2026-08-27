# [RASM_RHINO_SELECTION]

`Picks` owns native-reference projection, programmatic picking, detached evidence, geometry retention, and measured-query re-entry. Every owned `ObjRef` is projected and disposed inside one terminal window; every borrowed `ObjRef` remains scoped to the caller.

## [01]-[INDEX]

- [02]-[EVIDENCE]: `PickMethod`, `PartIndex`, `PickOrigin`, `PickView`, and the detached `PickCapture`.
- [03]-[PARTS]: `Picked` and the `PartKind` projector roster.
- [04]-[POLICY]: `PickGesture`, `PickRender`, `PickSlot`, `PickGate`, `PickRule`, `PickPolicy`, `PickGetterFact`, and `PickOutcome`.
- [05]-[PROJECTION]: the `Picks` capture, part, retain, execute, and measure entries.
- [06]-[BOUNDARY]: the detachment and affinity carves.
- [07]-[RESEARCH]: open verification rows.

## [02]-[EVIDENCE]

`PickCapture` is `IDetachedDocumentResult` — it crosses `Demand` by construction — and carries durable object identity, an admitted component index, view identity, and an evidence-shaped `PickOrigin`.

- Law: the two parameter probes are HANDLE-RETURNING, not scalar-returning — `CurveParameter` and `SurfaceParameter` each hand back a live geometry wrapper whose parent is a fresh host `ObjRef`, so the capture brackets each wrapper at its own call and only the admitted scalar leaves. Reading the `out` value and discarding the return strands one native reference per pick per axis, and the leak hides behind a scalar that arrives correctly.
- Law: admission happens ONCE, at each owner's construction. `PickOrigin`, `PickView`, and `PartIndex` are admitted values that cannot exist un-admitted, so the capture entry admits only what still arrives RAW from the host — the component index and the object id — and the outer re-validation pass that walked every nested case is gone.
- Law: `PickMethod` re-closes the host `SelectionMethod` wire as a keyed row — `Other` is the ordinal `0` every non-mouse selection (`SelAll`, a script, a saved set) reports, so admission reads the roster; a positivity bound refuses exactly those picks.
- Law: pick provenance is ONE record over two independent presence axes, not four cases. A curve parameter and a surface parameter are each present or absent; the four case names spelled that cross product, the union's own producer already supplied the two `Option`s, and both hand ladders re-derived the product it had just destructured. NAMED LOSS: the arm names `Point`/`Curve`/`Surface`/`CurveOnSurface`; the discriminant is recoverable from `(Parameter, Uv)` presence and the sole producer at `Picks.Capture` reads them straight off the host probes.
- Law: view identity is ONE record over a durable runtime serial and an OPTIONAL detail serial. The host spells "no detail" as `0`, so the sentinel dies at admission and the two former cases — whose only behavioural difference was a name, since both answered the same serial to `Live` — collapse. NAMED LOSS: the arm names `Main`/`Detail`, recoverable from `DetailSerial` presence.
- Law: the component index is ADMITTED, not guarded per use. The host's two legal corners — an invalid type at index `-1`, a named type at a non-negative index — are the value object's construction law, so `Objects/state` composes the same owner instead of re-spelling the pattern (E-R55, seated at the LOWER stratum: `ARCHITECTURE.md:107,161` places Objects above Commands, so the shared owner lands here and Objects imports it).
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<TKey>]`, `[ValueObject<T>]`, `[ComplexValueObject]`, `[Union]`, `[ValidationError]`, `[UseDelegateFromConstructor]`, `[KeyMemberEqualityComparer<TAccessor, TKey>]`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`, `Traverse`, `PartitionFallible`); Generator.Equals (`api-generator-equals.md` — `[Equatable]`, `[OrderedEquality]`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`), `Domain/results` (`HostEdge.Side`, `ValidityClaim`, the `Rollback` custody extension), `Analysis/query` (`AnalysisQuery`, `Analyze`); `Document/session` (`DraftFault`, `DocumentSession`, `SessionNeed`), `Document/geometry` (`GeometryCrossing`, `CrossingMode`, `GeometryHandle`); RhinoCommon commands (`Rasm.Rhino/.api/api-rhinocommon-commands.md:217-219` — the `ObjRef` projector roster, `PickContext`, `ObjectTable.PickObjects`, the `GetBaseClass` result reads), RhinoCommon objects (`api-rhinocommon-objects.md:184` — `ObjRef` identity projection).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PickMethod {
    public static readonly PickMethod Other = new(key: (int)SelectionMethod.Other);
    public static readonly PickMethod MousePick = new(key: (int)SelectionMethod.MousePick);
    public static readonly PickMethod WindowBox = new(key: (int)SelectionMethod.WindowBox);
    public static readonly PickMethod CrossingBox = new(key: (int)SelectionMethod.CrossingBox);

    internal static Fin<PickMethod> Of(SelectionMethod native) =>
        FactoryBridge.Row<int, PickMethod>((int)native);
}

[ValueObject<ComponentIndex>]
[ValidationError]
public readonly partial struct PartIndex {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ComponentIndex value) {
        ComponentIndex component = value;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (component is not ({ ComponentIndexType: ComponentIndexType.InvalidType, Index: -1 }
                    or { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 }),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(PartIndex), "an invalid type at index -1, or a named type at a non-negative index" })))));
    }
}

[ComplexValueObject]
[ValidationError]
public sealed partial class PickOrigin {
    public PickMethod Method { get; }
    public Point3d Point { get; }
    public Option<double> Parameter { get; }
    public Option<Point2d> Uv { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref PickMethod method,
        ref Point3d point,
        ref Option<double> parameter,
        ref Option<Point2d> uv) {
        PickMethod row = method;
        Point3d seat = point;
        Option<double> curve = parameter;
        Option<Point2d> surface = uv;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (row is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Method) }))),
                (!seat.IsValid, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Point), 0d, "a valid pick point" }))),
                (curve.Exists(static value => !ValidityClaim.Finite(value).Holds),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Parameter), curve.IfNone(0d), "a finite curve parameter" }))),
                (surface.Exists(static value => !value.IsValid),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Uv), 0d, "a valid surface parameter" })))));
    }

    internal static Fin<PickOrigin> Of(
        PickMethod method,
        Point3d point,
        Option<double> parameter,
        Option<Point2d> uv) =>
        FactoryBridge.Accept<PickOrigin>(
            fault: Validate(method, point, parameter, uv, out PickOrigin? admitted), admitted: admitted);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class PickView {
    public uint RuntimeSerial { get; }
    public Option<uint> DetailSerial { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref uint runtimeSerial,
        ref Option<uint> detailSerial) {
        uint serial = runtimeSerial;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (serial is 0u, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(RuntimeSerial), serial, "a live view serial" }))),
                (detailSerial.Exists(static value => value is 0u),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(DetailSerial), 0d, "a live detail serial" })))));
    }

    internal static Fin<Option<PickView>> Admit(Option<RhinoView> view, uint detailSerial) => view.Match(
        Some: live => FactoryBridge.Accept<PickView>(
                fault: Validate(
                    live.RuntimeSerialNumber,
                    detailSerial is 0u ? Option<uint>.None : Some(detailSerial),
                    out PickView? admitted),
                admitted: admitted)
            .Map(Some),
        None: () => detailSerial is 0u
            ? Fin.Succ(Option<PickView>.None)
            : Fin.Fail<Option<PickView>>(error: new KernelFault.InvalidResult()));

    internal Fin<RhinoView> Live() =>
        Try.lift(() => Optional(RhinoView.FromRuntimeSerialNumber(serialNumber: RuntimeSerial))
            .ToFin(Fail: new KernelFault.MissingContext())).Run().Bind(static inner => inner);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PickCapture(
    Guid ObjectId,
    PartIndex Component,
    PickOrigin Origin,
    Option<PickView> View) : IDetachedDocumentResult {
    internal static Fin<PickCapture> Admit(
        Guid objectId,
        ComponentIndex component,
        PickOrigin origin,
        Option<PickView> view) =>
        from part in FactoryBridge.Accept<PartIndex, ComponentIndex>(candidate: component)
        from admittedOrigin in Admit.Need(origin)
        from _ in guard(objectId != Guid.Empty, new KernelFault.InvalidResult(Detail: Some(nameof(ObjectId))))
        select new PickCapture(
            ObjectId: objectId,
            Component: part,
            Origin: admittedOrigin,
            View: view);
}
```

## [03]-[PARTS]

`Picked` closes every catalogued `ObjRef` projection at FOUR cases over one base column: `Shaped<T>` carries every `GeometryBase`-derived part, and the object, grip, and SubD-component cases carry what is not geometry — `SubDFace`/`SubDEdge`/`SubDVertex` derive from `SubDComponent`, so their parts never enter the geometry egress. `Picked` is the manual generic family the generator cannot lift. `PartKind` binds each capability to its native member, so absence fails as an unsupported part rather than falling through reflection.

- Law: every part states the CAPABILITY that produced it. `Whole` and `DefinitionPart` were byte-identical single-field wrappers whose only difference was the `PartKind` row that built them, and the three `SubD*Part` wrappers differed only in a payload type every producer erased into `Option<Picked>` before any consumer saw it. One `PartKind Origin` base column carries the discriminant for all four surviving cases. NAMED LOSS: the static payload type on five of the seven former arms; recovered from `Origin`, and the witness is the projector roster below, where each row already knows which member it read.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public abstract record Picked {
    private Picked(PartKind origin) => Origin = origin;

    private interface IShapedView {
        GeometryBase Shape { get; }
    }

    public PartKind Origin { get; }

    public sealed record Object(PartKind Origin, RhinoObject Value) : Picked(Origin);
    public sealed record Grip(PartKind Origin, GripObject Value) : Picked(Origin);
    public sealed record SubDPart(PartKind Origin, SubDComponent Value) : Picked(Origin);

    public sealed record Shaped<T>(PartKind Origin, T Value) : Picked(Origin), IShapedView where T : GeometryBase {
        GeometryBase IShapedView.Shape => Value;
    }

    public Option<GeometryBase> Geometry => this is IShapedView shaped ? Some(shaped.Shape) : None;
}

[SmartEnum<int>]
public sealed partial class PartKind {
    public static readonly PartKind Whole = new(key: 0, project: static reference =>
        Optional(reference.Object()).Map(static value => (Picked)new Picked.Object(Whole, value)));
    public static readonly PartKind Definition = new(key: 1, project: static reference =>
        Optional(reference.InstanceDefinitionPart()).Map(static value => (Picked)new Picked.Object(Definition, value)));
    public static readonly PartKind Grip = new(key: 2, project: static reference =>
        Optional(reference.Object()).Bind(static value => value is GripObject grip
            ? Some((Picked)new Picked.Grip(Grip, grip))
            : None));
    public static readonly PartKind Geometry = new(key: 3, project: static reference => Shaped(Geometry, reference.Geometry()));
    public static readonly PartKind BrepWhole = new(key: 4, project: static reference => Shaped(BrepWhole, reference.Brep()));
    public static readonly PartKind Face = new(key: 5, project: static reference => Shaped(Face, reference.Face()));
    public static readonly PartKind Edge = new(key: 6, project: static reference => Shaped(Edge, reference.Edge()));
    public static readonly PartKind Trim = new(key: 7, project: static reference => Shaped(Trim, reference.Trim()));
    public static readonly PartKind SubDWhole = new(key: 8, project: static reference => Shaped(SubDWhole, reference.SubD()));
    public static readonly PartKind SubDFace = new(key: 9, project: static reference =>
        Optional(reference.SubDFace()).Map(static value => (Picked)new Picked.SubDPart(SubDFace, value)));
    public static readonly PartKind SubDEdge = new(key: 10, project: static reference =>
        Optional(reference.SubDEdge()).Map(static value => (Picked)new Picked.SubDPart(SubDEdge, value)));
    public static readonly PartKind SubDVertex = new(key: 11, project: static reference =>
        Optional(reference.SubDVertex()).Map(static value => (Picked)new Picked.SubDPart(SubDVertex, value)));
    public static readonly PartKind CurveKind = new(key: 12, project: static reference => Shaped(CurveKind, reference.Curve()));
    public static readonly PartKind SurfaceKind = new(key: 13, project: static reference => Shaped(SurfaceKind, reference.Surface()));
    public static readonly PartKind MeshKind = new(key: 14, project: static reference => Shaped(MeshKind, reference.Mesh()));
    public static readonly PartKind PointKind = new(key: 15, project: static reference => Shaped(PointKind, reference.Point()));
    public static readonly PartKind Cloud = new(key: 16, project: static reference => Shaped(Cloud, reference.PointCloud()));
    public static readonly PartKind Dot = new(key: 17, project: static reference => Shaped(Dot, reference.TextDot()));
    public static readonly PartKind Annotation = new(key: 18, project: static reference => Shaped(Annotation, reference.TextEntity()));
    public static readonly PartKind LightKind = new(key: 19, project: static reference => Shaped(LightKind, reference.Light()));
    public static readonly PartKind HatchKind = new(key: 20, project: static reference => Shaped(HatchKind, reference.Hatch()));
    public static readonly PartKind Clip = new(key: 21, project: static reference => Shaped(Clip, reference.ClippingPlaneSurface()));

    [UseDelegateFromConstructor]
    internal partial Option<Picked> Project(ObjRef reference);

    private static Option<Picked> Shaped<T>(PartKind origin, T? value) where T : GeometryBase =>
        Optional(value).Map(shape => (Picked)new Picked.Shaped<T>(origin, shape));
}
```

## [04]-[POLICY]

`PickPolicy` generates from `PickRule` data over the folder's one rule-roster spine. One row owns each independent `PickContext` dimension, duplicates refuse against the closed `PickSlot` vocabulary, and a new host dimension extends the case family instead of widening a constructor bag. `PickGesture` over `PickStyle` and `PickRender` over `PickMode` re-close each host discriminant on its own ordinal, while the view dimension carries the durable `PickView` serial that resolves to a live `RhinoView` at `Apply` — so a stored policy holds no host handle and no raw host enum, and a view closing between authoring and execution surfaces as a typed refusal.

- Law: the two payload-free context toggles are COMBINABLE membership, not two cases carrying a bool. `PickGate` rows own their own host write, and one `Gates` rule carries two disjoint sets — the same shape `PointGate` and `ObjectGate` already run on the acquisition page — so a reader prints what the context was told through two `Wire` reads and a third gate is one row.
- Law: the slot identity is TYPED. `ISlotted<PickSlot>` closes the knob space this family addresses, so injectivity compares generated rows instead of boxing `GetType()` and comparing through `object.Equals`.
- Law: `PickOutcome` names WHICH getter participated, never that one did. `PickContext.GetObjectUsed` is the host's own null sentinel and its projection into `Option<PickGetterFact>` is the last line naming it; the fact carries the terminal the participating getter reported and the option seat it ended on.
- Law: a stale reference does not void the pick. `CaptureOwned` partitions survivors from casualties and `PickOutcome` carries both, so a forty-object pick with one dead reference answers thirty-nine captures and one named refusal. NAMED LOSS: whole-batch atomicity — a caller that needs all-or-nothing reads `Rejected.IsEmpty` at the entry, and the release of every owned reference is unchanged on both branches.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PickGesture {
    public static readonly PickGesture None = new(key: (int)PickStyle.None);
    public static readonly PickGesture Point = new(key: (int)PickStyle.PointPick);
    public static readonly PickGesture Window = new(key: (int)PickStyle.WindowPick);
    public static readonly PickGesture Crossing = new(key: (int)PickStyle.CrossingPick);

    internal PickStyle Native => (PickStyle)Key;
}

[SmartEnum<int>]
public sealed partial class PickRender {
    public static readonly PickRender Wireframe = new(key: (int)PickMode.Wireframe);
    public static readonly PickRender Shaded = new(key: (int)PickMode.Shaded);

    internal PickMode Native => (PickMode)Key;
}

[SmartEnum<int>]
public sealed partial class PickSlot {
    public static readonly PickSlot View = new(key: 0);
    public static readonly PickSlot Line = new(key: 1);
    public static readonly PickSlot Style = new(key: 2);
    public static readonly PickSlot Mode = new(key: 3);
    public static readonly PickSlot Gates = new(key: 4);
    public static readonly PickSlot Pose = new(key: 5);
    public static readonly PickSlot Clipping = new(key: 6);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PickGate : ICapability<PickGate> {
    public static readonly PickGate Groups = new(key: "groups", set: static (target, on) => target.PickGroupsEnabled = on);
    public static readonly PickGate SubObjects = new(key: "sub-objects", set: static (target, on) => target.SubObjectSelectionEnabled = on);

    [UseDelegateFromConstructor]
    internal partial void Set(PickContext target, bool enabled);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PickRule : ISlotted<PickSlot> {
    private PickRule() { }
    public sealed record InView(PickView Value) : PickRule;
    public sealed record Along(Line Value) : PickRule;
    public sealed record Styled(PickGesture Value) : PickRule;
    public sealed record Rendered(PickRender Value) : PickRule;
    public sealed record Gates(CapabilitySet<PickGate> Enabled, CapabilitySet<PickGate> Disabled) : PickRule;
    public sealed record Transformed(Transform Value) : PickRule;
    public sealed record RefreshClipping : PickRule;

    public PickSlot SlotKey => Switch(
        inView: static _ => PickSlot.View,
        along: static _ => PickSlot.Line,
        styled: static _ => PickSlot.Style,
        rendered: static _ => PickSlot.Mode,
        gates: static _ => PickSlot.Gates,
        transformed: static _ => PickSlot.Pose,
        refreshClipping: static _ => PickSlot.Clipping);

    internal Fin<Unit> Admit() => Switch(
        state: key,
        inView: static (rule) => Admit.Need(rule.Value).Map(static _ => unit),
        along: static (rule) => guard(rule.Value.IsValid, new KernelFault.InvalidInput(Axis: Some(nameof(Along)))).ToFin(),
        styled: static (rule) => guard(rule.Value is not null, new KernelFault.InvalidInput(Axis: Some(nameof(Styled)))).ToFin(),
        rendered: static (rule) => guard(rule.Value is not null, new KernelFault.InvalidInput(Axis: Some(nameof(Rendered)))).ToFin(),
        gates: static (rule) => guard(
            rule.Enabled.Held.All(row => !rule.Disabled.Admits(capability: row)),
            new KernelFault.InvalidInput(Axis: Some(nameof(Gates)))).ToFin(),
        transformed: static (rule) => guard(rule.Value.IsValid, new KernelFault.InvalidInput(Axis: Some(nameof(Transformed)))).ToFin(),
        refreshClipping: static (_, _) => Fin.Succ(value: unit));

    internal Fin<Unit> Apply(PickContext context) => Switch(
        state: context,
        inView: static (state, rule) => rule.Value.Live()
            .Bind(view => Try.lift(() => Fin.Succ(HostEdge.Side(() => state.View = view))).Run().Bind(static inner => inner)),
        along: static (state, rule) => Try.lift(() => Fin.Succ(HostEdge.Side(() => state.PickLine = rule.Value))).Run().Bind(static inner => inner),
        styled: static (state, rule) => Try.lift(() => Fin.Succ(HostEdge.Side(() => state.PickStyle = rule.Value.Native))).Run().Bind(static inner => inner),
        rendered: static (state, rule) => Try.lift(() => Fin.Succ(HostEdge.Side(() => state.PickMode = rule.Value.Native))).Run().Bind(static inner => inner),
        gates: static (state, rule) => Try.lift(() => Fin.Succ(HostEdge.Side(() => {
            rule.Enabled.Held.Iter(row => row.Set(state, enabled: true));
            rule.Disabled.Held.Iter(row => row.Set(state, enabled: false));
        }))).Run().Bind(static inner => inner),
        transformed: static (state, rule) => Try.lift(() => Fin.Succ(HostEdge.Side(() => state.SetPickTransform(rule.Value)))).Run().Bind(static inner => inner),
        refreshClipping: static (state, _) => Try.lift(() => Fin.Succ(HostEdge.Side(state.UpdateClippingPlanes))).Run().Bind(static inner => inner));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PickPolicy {
    private PickPolicy(RulePlan<PickRule, PickSlot> plan) => Plan = plan;

    internal RulePlan<PickRule, PickSlot> Plan { get; }

    public Seq<PickRule> Rules => Plan.Rules;

    public static Fin<PickPolicy> PointAt { get; } = Of(rules: [
        new PickRule.Styled(Value: PickGesture.Point),
        new PickRule.Rendered(Value: PickRender.Shaded),
        new PickRule.Gates(
            Enabled: CapabilitySet<PickGate>.Of(PickGate.SubObjects),
            Disabled: CapabilitySet<PickGate>.Of(PickGate.Groups)),
        new PickRule.RefreshClipping(),
    ]);

    public static Fin<PickPolicy> Of(Seq<PickRule> rules) =>
        RulePlan<PickRule, PickSlot>.Of(
                rules: rules,
                admit: static (rule, k) => rule.Admit(k),
                key: key.OrDefault(name: nameof(PickPolicy)))
            .Map(static plan => new PickPolicy(plan: plan));

    internal Fin<Unit> Apply(PickContext target) => Plan.Apply(
        target: target, apply: static (rule, context, op) => rule.Apply(context));
}

public sealed record PickGetterFact(GetResult Terminal, Option<int> Selected);

[Equatable]
public sealed partial record PickOutcome(
    Option<PickGetterFact> Getter,
    [property: OrderedEquality] Seq<PickCapture> Captures,
    [property: OrderedEquality] Seq<Error> Rejected) : IDetachedDocumentResult;
```

## [05]-[PROJECTION]

`Picks.Capture` projects borrowed references without taking custody. `CaptureOwned` consumes a returned reference sequence, partitions casualties from survivors, and releases every entry on both branches. `Execute` derives and disposes one `PickContext`, projects `GetObjectUsed`, and returns only detached evidence. `Part` is a SCOPED projector: it mints the `Picked` view, hands it to the caller's projection, and lets it die with the call, because the live `RhinoObject`, `GripObject`, `SubDComponent`, and `GeometryBase` it wraps carry no lease — returning the `Picked` itself is the deleted form and `Retain` is the one crossing into owned custody.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Picks {
    public static Fin<PickCapture> Capture(ObjRef reference) {
        return from _ in guard(RhinoApp.IsOnMainThread, new KernelFault.InvalidContext()).ToFin()
               from active in Admit.Need(reference)
               from admittedMethod in Try.lift(() => PickMethod.Of(native: active.SelectionMethod())).Run().Bind(static inner => inner)
               from curve in CurveAt(reference: active)
               from surface in SurfaceAt(reference: active)
               from origin in Try.lift(() => PickOrigin.Of(
                   method: admittedMethod,
                   point: active.SelectionPoint(),
                   parameter: curve,
                   uv: surface)).Run().Bind(static inner => inner)
               from view in Try.lift(() => PickView.Admit(
                   view: Optional(active.SelectionView()),
                   detailSerial: active.SelectionViewDetailSerialNumber())).Run().Bind(static inner => inner)
               from capture in Try.lift(() => PickCapture.Admit(
                   objectId: active.ObjectId,
                   component: active.GeometryComponentIndex,
                   origin: origin,
                   view: view)).Run().Bind(static inner => inner)
               select capture;
    }

    private static Fin<Option<double>> CurveAt(ObjRef reference) => Try.lift(() => {
        using Curve? curve = reference.CurveParameter(parameter: out double parameter);
        return Fin.Succ(value: curve is null ? Option<double>.None : Some(parameter));
    }).Run().Bind(static inner => inner);

    private static Fin<Option<Point2d>> SurfaceAt(ObjRef reference) => Try.lift(() => {
        using Surface? surface = reference.SurfaceParameter(u: out double u, v: out double v);
        return Fin.Succ(value: surface is null ? Option<Point2d>.None : Some(new Point2d(x: u, y: v)));
    }).Run().Bind(static inner => inner);

    public static Fin<PickOutcome> CaptureOwned(IEnumerable<ObjRef> references) {
        return from source in Admit.Need(references)
               from owned in Try.lift(() => Fin.Succ(toSeq(source).Strict())).Run().Bind(static inner => inner)
               from _ in guard(
                   owned.ForAll(static reference => reference is not null),
                   new KernelFault.InvalidResult(Detail: Some(nameof(references))))
               from outcome in owned
                   .Map(reference => Capture(reference: reference))
                   .PartitionFallible()
                   .As()
                   .Map(static split => new PickOutcome(
                       Getter: None, Captures: split.Succs, Rejected: split.Fails))
                   .Settled(release: () => Released(owned))
               select outcome;
    }

    private static Fin<Unit> Released(Seq<ObjRef> owned) => Custody.Release(
        held: owned,
        release: reference => Try.lift(() => Fin.Succ(value: HostEdge.Side(reference.Dispose))).Run().Bind(static inner => inner));

    public static Fin<TOut> Part<TOut>(ObjRef reference, PartKind ask, Func<Picked, Fin<TOut>> project)
        where TOut : notnull {
        return from _ in guard(RhinoApp.IsOnMainThread, new KernelFault.InvalidContext()).ToFin()
               from active in Admit.Need(reference)
               from kind in Admit.Need(ask)
               from body in Admit.Need(project)
               from part in Try.lift(() => kind.Project(reference: active)
                   .ToFin(new KernelFault.Unsupported(InputType: typeof(ObjRef), OutputType: typeof(Picked)))).Run().Bind(static inner => inner)
               from result in Try.lift(() => body(arg: part)).Run().Bind(static inner => inner)
               select result;
    }

    public static Fin<GeometryHandle> Retain(ObjRef reference, PartKind ask) {
        return Part(
            reference: reference,
            ask: ask,
            project: part => part.Geometry
                .ToFin(new KernelFault.Unsupported(InputType: typeof(Picked), OutputType: typeof(GeometryBase)))
                .Bind(geometry => GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach)));
    }

    public static Fin<PickOutcome> Execute(DocumentSession session, PickPolicy policy) {
        return from _ in guard(RhinoApp.IsOnMainThread, new KernelFault.InvalidContext()).ToFin()
               from target in Admit.Need(session)
               from active in Admit.Need(policy)
               from outcome in target.Demand(
                   use: document =>
                       from defaultView in Optional(document.Views.ActiveView).ToFin(Fail: new KernelFault.MissingContext())
                       from projected in Try.lift(() => {
                           using PickContext context = new() { View = defaultView };
                           return active.Apply(target: context, key: op)
                               .Bind(_ => Try.lift(() => Fin.Succ(document.Objects.PickObjects(pickContext: context))).Run().Bind(static inner => inner))
                               .Bind(references => CaptureOwned(references: references, key: op)
                                   .Map(held => held with { Getter = Participant(context) }));
                       }).Run().Bind(static inner => inner)
                       select projected,
                   needs: [SessionNeed.Read])
               select outcome;
    }

    private static Option<PickGetterFact> Participant(PickContext context) =>
        Optional(context.GetObjectUsed).Map(static used => new PickGetterFact(
            Terminal: used.Result(),
            Selected: used.OptionIndex() is int seat && seat >= 0 ? Some(seat) : None));

    public static Fin<Seq<TOut>> Measured<TOut>(
        DocumentSession session,
        AnalysisQuery ask,
        Seq<GeometryBase> subjects)
        where TOut : notnull {
        return from active in Admit.Need(session)
               from query in Admit.Need(ask)
               from _ in guard(
                   !subjects.IsEmpty && subjects.ForAll(static shape => shape is not null),
                   new KernelFault.InvalidInput(Axis: Some(nameof(subjects))))
               from domain in active.Context()
               from measured in Analyze.In(context: domain)
                   .Run(operation: Analyze.Query<GeometryBase, TOut>(query: query), input: [.. subjects])
                   .ToFin()
               select measured;
    }
}
```

## [06]-[BOUNDARY]

`PickCapture` crosses into `Objects` as detached identity and selection evidence, `PartIndex` crosses as the admitted component owner both planes read, and `GeometryHandle` crosses into document geometry custody. `PickPolicy` is durable by design over detached rows alone — `PickView` serials, keyed `PickGesture`/`PickRender`, `PickGate` sets, admitted value structs — so no `ObjRef`, `RhinoView`, `PickContext`, or live geometry payload becomes durable state. `Part` scopes `Picked` into one projection and no entry returns it, which bounds the call window structurally.

`Picks.Execute`, `Measured`, `Retain`, `Part`, `PickPolicy`, and the `PartKind` roster are the PUBLISHED surface — a command body in the `apps/<app>/` plugin shell composes them, so a corpus-wide caller census answers zero for them exactly as it does for `Acquisition.Get`. The page's INTERNAL reach is what `libs/dotnet` must construct, and it is fully wired: `Capture` from `Objects/authoring`'s pick and picked programs, and `CaptureOwned` from `Commands/acquisition`'s object getter and its two modal object routes, whose `Acquired.Objects` payload IS this page's `PickOutcome`.

A detached capture carries NO `ModelUnit`. A regime change rescales the document's geometry with it, so a stored pick point re-read afterwards is read against geometry that moved the same way; the branch ruling binds a detached MAGNITUDE the user authored — `Acquired.Distance` is its case — and a document-space position is not one. The curve parameter and the surface `Uv` are dimensionless by construction.

The command-thread carve: `RhinoApp.IsOnMainThread` at each public entry is Rhino's COMMAND-thread affinity — a different axis than the kernel marshal, whose `UiThread`/`UiDispatch` owner sits at S0 below this page, and the two are different threads by construction on Windows; this probe is the command lane's own gate, never a re-spelling of the kernel marshal.

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
