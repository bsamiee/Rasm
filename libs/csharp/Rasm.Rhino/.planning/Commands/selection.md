# [RASM_RHINO_SELECTION]

`Picks` owns native-reference projection, programmatic picking, detached evidence, geometry retention, and measured-query re-entry. Every owned `ObjRef` is projected and disposed inside one terminal window; every borrowed `ObjRef` remains scoped to the caller.

## [01]-[INDEX]

- [02]-[EVIDENCE]: `PickMethod`, `PickOrigin`, `PickView`, and the detached `PickCapture`.
- [03]-[PARTS]: `Picked` and the `PartKind` projector roster.
- [04]-[POLICY]: `PickGesture`, `PickRender`, `PickRule`, `PickPolicy`, and `PickReceipt`.
- [05]-[PROJECTION]: the `Picks` capture, part, retain, execute, and measure entries.
- [06]-[BOUNDARY]: the detachment and affinity carves.
- [07]-[RESEARCH]: open verification rows.

## [02]-[EVIDENCE]

`PickCapture` is `IDetachedDocumentResult` — it crosses `Demand` by construction — and carries durable object identity, view identity, detail identity, and an evidence-shaped `PickOrigin`.

- Law: the two parameter probes are HANDLE-RETURNING, not scalar-returning — `CurveParameter` and `SurfaceParameter` each hand back a live geometry wrapper whose parent is a fresh host `ObjRef`, so the capture brackets each wrapper at its own call and only the admitted scalar leaves. Reading the `out` value and discarding the return strands one native reference per pick per axis, and the leak is invisible because the scalar arrives correctly. Its outer admission re-validates every public nested case and rebuilds canonical evidence before storage. `PickMethod` re-closes the host `SelectionMethod` wire as a keyed row — `Other` is the ordinal `0` every non-mouse selection (`SelAll`, a script, a saved set) reports, so admission is a roster lookup and never a positivity bound, which would refuse exactly those picks. `PickOrigin` carries that row beside the total point/curve/surface evidence product.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Generic;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PickMethod {
    public static readonly PickMethod Other = new(key: (int)SelectionMethod.Other);
    public static readonly PickMethod MousePick = new(key: (int)SelectionMethod.MousePick);
    public static readonly PickMethod WindowBox = new(key: (int)SelectionMethod.WindowBox);
    public static readonly PickMethod CrossingBox = new(key: (int)SelectionMethod.CrossingBox);

    internal SelectionMethod Native => (SelectionMethod)Key;

    internal static Fin<PickMethod> Of(SelectionMethod native, Op key) =>
        key.Row<int, PickMethod>((int)native);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PickOrigin {
    private PickOrigin() { }
    public sealed record Point(PickMethod Method, Point3d Value) : PickOrigin;
    public sealed record Curve(PickMethod Method, Point3d Point, double Parameter) : PickOrigin;
    public sealed record Surface(PickMethod Method, Point3d Point, Point2d Uv) : PickOrigin;
    public sealed record CurveOnSurface(
        PickMethod Method,
        Point3d Point,
        double Parameter,
        Point2d Uv) : PickOrigin;

    internal Fin<PickOrigin> Admit(Op key) => this switch {
        Point row => Admit(row.Method, row.Value, None, None, key),
        Curve row => Admit(row.Method, row.Point, Some(row.Parameter), None, key),
        Surface row => Admit(row.Method, row.Point, None, Some(row.Uv), key),
        CurveOnSurface row => Admit(row.Method, row.Point, Some(row.Parameter), Some(row.Uv), key),
    };

    internal static Fin<PickOrigin> Admit(
        PickMethod method,
        Point3d point,
        Option<double> curve,
        Option<Point2d> surface,
        Op key) =>
        from _ in guard(
                flag: method is not null
                    && point.IsValid
                    && curve.ForAll(double.IsFinite)
                    && surface.ForAll(static uv => uv.IsValid),
                False: key.InvalidResult())
            .ToFin()
        select (curve.Case, surface.Case) switch {
            (double parameter, Point2d uv) => (PickOrigin)new CurveOnSurface(
                Method: method, Point: point, Parameter: parameter, Uv: uv),
            (double parameter, _) => new Curve(Method: method, Point: point, Parameter: parameter),
            (_, Point2d uv) => new Surface(Method: method, Point: point, Uv: uv),
            _ => new Point(Method: method, Value: point),
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PickView {
    private PickView() { }
    public sealed record Main(uint RuntimeSerial) : PickView;
    public sealed record Detail(uint RuntimeSerial, uint DetailSerial) : PickView;

    internal Fin<PickView> Admit(Op key) => this switch {
        Main row => guard(row.RuntimeSerial > 0, key.InvalidResult()).ToFin()
            .Map(_ => (PickView)new Main(RuntimeSerial: row.RuntimeSerial)),
        Detail row => guard(row.RuntimeSerial > 0 && row.DetailSerial > 0, key.InvalidResult()).ToFin()
            .Map(_ => (PickView)new Detail(RuntimeSerial: row.RuntimeSerial, DetailSerial: row.DetailSerial)),
    };

    internal static Fin<Option<PickView>> Admit(Option<RhinoView> view, uint detailSerial, Op key) =>
        view.Match(
            Some: live => guard(flag: live.RuntimeSerialNumber > 0, False: key.InvalidResult()).ToFin().Bind(_ =>
                (detailSerial is 0
                    ? (PickView)new Main(RuntimeSerial: live.RuntimeSerialNumber)
                    : new Detail(RuntimeSerial: live.RuntimeSerialNumber, DetailSerial: detailSerial))
                .Admit(key)
                .Map(Some)),
            None: () => detailSerial is 0
                ? Fin.Succ(Option<PickView>.None)
                : Fin.Fail<Option<PickView>>(error: key.InvalidResult()));

    internal Fin<RhinoView> Live(Op key) =>
        key.Catch(() => Optional(RhinoView.FromRuntimeSerialNumber(serialNumber: Switch(
                main: static row => row.RuntimeSerial,
                detail: static row => row.RuntimeSerial)))
            .ToFin(Fail: key.MissingContext()));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PickCapture(
    Guid ObjectId,
    ComponentIndex Component,
    PickOrigin Origin,
    Option<PickView> View) : IDetachedDocumentResult {
    internal static Fin<PickCapture> Admit(
        Guid objectId,
        ComponentIndex component,
        PickOrigin origin,
        Option<PickView> view,
        Op key) =>
        from admittedOrigin in Optional(origin).ToFin(Fail: key.InvalidResult()).Bind(value => value.Admit(key))
        from admittedView in view.Match(
            Some: value => Optional(value).ToFin(Fail: key.InvalidResult()).Bind(candidate => candidate.Admit(key)).Map(Some),
            None: () => Fin.Succ(Option<PickView>.None))
        from _ in guard(
                flag: objectId != Guid.Empty
                    && component is { ComponentIndexType: ComponentIndexType.InvalidType, Index: -1 }
                        or { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 },
                False: key.InvalidResult())
            .ToFin()
        select new PickCapture(
                ObjectId: objectId,
                Component: component,
                Origin: admittedOrigin,
                View: admittedView);
}
```

## [03]-[PARTS]

`Picked` closes every catalogued `ObjRef` projection: one generic `Shaped<T>` case carries every `GeometryBase`-derived part, and the object, grip, and SubD-component cases carry the references that are not geometry — `SubDFace`/`SubDEdge`/`SubDVertex` derive from `SubDComponent`, not `GeometryBase`, so their parts never enter the geometry egress. `Picked` is the manual generic family the generator cannot lift. `PartKind` binds each requested capability directly to its native member, so absence fails as an unsupported part and never falls through reflection or assignability.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
public abstract record Picked {
    private Picked() { }

    private interface IShapedView {
        GeometryBase Shape { get; }
    }

    public sealed record Whole(RhinoObject Value) : Picked;
    public sealed record DefinitionPart(RhinoObject Value) : Picked;
    public sealed record GripPart(GripObject Value) : Picked;
    public sealed record SubDFacePart(SubDFace Value) : Picked;
    public sealed record SubDEdgePart(SubDEdge Value) : Picked;
    public sealed record SubDVertexPart(SubDVertex Value) : Picked;

    public sealed record Shaped<T>(T Value) : Picked, IShapedView where T : GeometryBase {
        GeometryBase IShapedView.Shape => Value;
    }

    public Option<GeometryBase> Geometry => this is IShapedView shaped ? Some(shaped.Shape) : None;
}

[SmartEnum<int>]
public sealed partial class PartKind {
    public static readonly PartKind Whole = new(key: 0, project: static reference =>
        Optional(reference.Object()).Map(static value => (Picked)new Picked.Whole(Value: value)));
    public static readonly PartKind Definition = new(key: 1, project: static reference =>
        Optional(reference.InstanceDefinitionPart()).Map(static value => (Picked)new Picked.DefinitionPart(Value: value)));
    public static readonly PartKind Grip = new(key: 2, project: static reference =>
        Optional(reference.Object()).Bind(static value => value is GripObject grip
            ? Some((Picked)new Picked.GripPart(Value: grip))
            : None));
    public static readonly PartKind Geometry = new(key: 3, project: static reference => Shaped(reference.Geometry()));
    public static readonly PartKind BrepWhole = new(key: 4, project: static reference => Shaped(reference.Brep()));
    public static readonly PartKind Face = new(key: 5, project: static reference => Shaped(reference.Face()));
    public static readonly PartKind Edge = new(key: 6, project: static reference => Shaped(reference.Edge()));
    public static readonly PartKind Trim = new(key: 7, project: static reference => Shaped(reference.Trim()));
    public static readonly PartKind SubDWhole = new(key: 8, project: static reference => Shaped(reference.SubD()));
    public static readonly PartKind SubDFace = new(key: 9, project: static reference =>
        Optional(reference.SubDFace()).Map(static value => (Picked)new Picked.SubDFacePart(Value: value)));
    public static readonly PartKind SubDEdge = new(key: 10, project: static reference =>
        Optional(reference.SubDEdge()).Map(static value => (Picked)new Picked.SubDEdgePart(Value: value)));
    public static readonly PartKind SubDVertex = new(key: 11, project: static reference =>
        Optional(reference.SubDVertex()).Map(static value => (Picked)new Picked.SubDVertexPart(Value: value)));
    public static readonly PartKind CurveKind = new(key: 12, project: static reference => Shaped(reference.Curve()));
    public static readonly PartKind SurfaceKind = new(key: 13, project: static reference => Shaped(reference.Surface()));
    public static readonly PartKind MeshKind = new(key: 14, project: static reference => Shaped(reference.Mesh()));
    public static readonly PartKind PointKind = new(key: 15, project: static reference => Shaped(reference.Point()));
    public static readonly PartKind Cloud = new(key: 16, project: static reference => Shaped(reference.PointCloud()));
    public static readonly PartKind Dot = new(key: 17, project: static reference => Shaped(reference.TextDot()));
    public static readonly PartKind Annotation = new(key: 18, project: static reference => Shaped(reference.TextEntity()));
    public static readonly PartKind LightKind = new(key: 19, project: static reference => Shaped(reference.Light()));
    public static readonly PartKind HatchKind = new(key: 20, project: static reference => Shaped(reference.Hatch()));
    public static readonly PartKind Clip = new(key: 21, project: static reference => Shaped(reference.ClippingPlaneSurface()));

    [UseDelegateFromConstructor]
    internal partial Option<Picked> Project(ObjRef reference);

    private static Option<Picked> Shaped<T>(T? value) where T : GeometryBase =>
        Optional(value).Map(static shape => (Picked)new Picked.Shaped<T>(Value: shape));
}
```

## [04]-[POLICY]

`PickPolicy` is generated from `PickRule` data. One row owns each independent `PickContext` dimension, and duplicate dimensions fail admission; adding a host dimension extends the case family instead of widening a constructor bag. Every host discriminant re-closes as a keyed row — `PickGesture` over `PickStyle`, `PickRender` over `PickMode`, each keyed on the host ordinal so the roster cannot silently outgrow a hand-numbered literal — and the view dimension carries the durable `PickView` serial, resolved to a live `RhinoView` at `Apply`. A stored policy therefore holds no host handle and no raw host enum, so it survives a view closing between authoring and execution as a typed refusal rather than a dangling reference.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PickRule : ISlotted {
    private PickRule() { }
    public sealed record InView(PickView Value) : PickRule;
    public sealed record Along(Line Value) : PickRule;
    public sealed record Styled(PickGesture Value) : PickRule;
    public sealed record Rendered(PickRender Value) : PickRule;
    public sealed record Grouped(bool Enabled) : PickRule;
    public sealed record SubObjected(bool Enabled) : PickRule;
    public sealed record Transformed(Transform Value) : PickRule;
    public sealed record RefreshClipping : PickRule;

    public virtual object SlotKey => GetType();

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        inView: static (op, rule) => op.Need(rule.Value)
            .Bind(view => view.Admit(op)).Map(static _ => unit),
        along: static (op, rule) => guard(rule.Value.IsValid, op.InvalidInput()).ToFin(),
        styled: static (op, rule) => guard(rule.Value is not null, op.InvalidInput()).ToFin(),
        rendered: static (op, rule) => guard(rule.Value is not null, op.InvalidInput()).ToFin(),
        // The three payload-free toggles each state their own admission; a catch-all admits the next case that
        // grows a payload without anyone noticing it was never validated.
        grouped: static (_, _) => Fin.Succ(value: unit),
        subObjected: static (_, _) => Fin.Succ(value: unit),
        transformed: static (op, rule) => guard(rule.Value.IsValid, op.InvalidInput()).ToFin(),
        refreshClipping: static (_, _) => Fin.Succ(value: unit));

    internal Fin<Unit> Apply(PickContext context, Op key) =>
        Switch(
            state: (Target: context, Op: key),
            inView: static (state, rule) => rule.Value.Live(state.Op)
                .Bind(view => Write(state.Op, () => state.Target.View = view)),
            along: static (state, rule) => Write(state.Op, () => state.Target.PickLine = rule.Value),
            styled: static (state, rule) => Write(state.Op, () => state.Target.PickStyle = rule.Value.Native),
            rendered: static (state, rule) => Write(state.Op, () => state.Target.PickMode = rule.Value.Native),
            grouped: static (state, rule) => Write(state.Op, () => state.Target.PickGroupsEnabled = rule.Enabled),
            subObjected: static (state, rule) => Write(state.Op, () => state.Target.SubObjectSelectionEnabled = rule.Enabled),
            transformed: static (state, rule) => Write(state.Op, () => state.Target.SetPickTransform(rule.Value)),
            refreshClipping: static (state, _) => Write(state.Op, state.Target.UpdateClippingPlanes));

    private static Fin<Unit> Write(Op key, Action write) =>
        key.Catch(() => {
            write();
            return Fin.Succ(value: unit);
        });
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PickPolicy {
    private PickPolicy(Seq<PickRule> rules) => Rules = rules;

    public Seq<PickRule> Rules { get; }

    public static PickPolicy PointAt { get; } = new(rules: [
        new PickRule.Styled(Value: PickGesture.Point),
        new PickRule.Rendered(Value: PickRender.Shaded),
        new PickRule.Grouped(Enabled: false),
        new PickRule.SubObjected(Enabled: true),
        new PickRule.RefreshClipping(),
    ]);

    public static Fin<PickPolicy> Of(params ReadOnlySpan<PickRule> rules) {
        Op op = Op.Of(name: nameof(PickPolicy));
        Seq<PickRule> admitted = toSeq(rules.ToArray());
        return from _ in guard(admitted.ForAll(static rule => rule is not null), op.InvalidInput())
               from __ in admitted.TraverseM(rule => rule.Admit(op)).As()
               from ___ in guard(admitted.OnePer(), op.InvalidInput())
               select new PickPolicy(rules: admitted);
    }
}

public sealed record PickReceipt(
    bool GetterParticipated,
    Seq<PickCapture> Captures) : IDetachedDocumentResult;
```

## [05]-[PROJECTION]

`Picks.Capture` projects borrowed references without taking custody. `CaptureOwned` consumes a returned reference sequence, accumulates every independent projection failure, and releases every entry. `Execute` derives and disposes one `PickContext`, captures `GetObjectUsed`, and returns only detached evidence. `Part` is a SCOPED projector, never an accessor: it mints the `Picked` view, hands it to the caller's projection, and lets it die with the call — the live `RhinoObject`, `GripObject`, `SubDComponent`, and `GeometryBase` it wraps carry no lease and no scope of their own, so returning the `Picked` itself is the deleted form and `Retain` is the one crossing that converts a part into owned custody.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Picks {
    public static Fin<PickCapture> Capture(ObjRef reference, Op key) =>
        from _ in guard(RhinoApp.IsOnMainThread, key.InvalidContext())
        from active in key.Need(reference)
        from capture in
            from admittedMethod in key.Catch(() => PickMethod.Of(native: active.SelectionMethod(), key: key))
            from curve in CurveAt(reference: active, key: key)
            from surface in SurfaceAt(reference: active, key: key)
            from origin in key.Catch(() => PickOrigin.Admit(
                method: admittedMethod,
                point: active.SelectionPoint(),
                curve: curve,
                surface: surface,
                key: key))
            from view in key.Catch(() => PickView.Admit(
                view: Optional(active.SelectionView()),
                detailSerial: active.SelectionViewDetailSerialNumber(),
                key: key))
            from admitted in key.Catch(() => PickCapture.Admit(
                objectId: active.ObjectId,
                component: active.GeometryComponentIndex,
                origin: origin,
                view: view,
                key: key))
            select admitted
        select capture;

    // `ObjRef.CurveParameter`/`SurfaceParameter` do not answer a scalar — each returns a LIVE geometry wrapper
    // beside its `out` value, and for a non-top-level pointer the host mints a FRESH `ObjRef` as that wrapper's
    // parent inside `ObjRefToGeometryHelper`. Reading the scalar and dropping the wrapper on the floor therefore
    // strands one native reference per pick per axis. Each probe brackets its wrapper on the same statement, so
    // the parent becomes unreachable at once and only the scalar leaves; the parent itself is GC-reclaimed, the
    // residual this seam cannot close because the host exposes no handle to it.
    private static Fin<Option<double>> CurveAt(ObjRef reference, Op key) => key.Catch(() => {
        using Curve? curve = reference.CurveParameter(parameter: out double parameter);
        return Fin.Succ(value: curve is null ? Option<double>.None : Some(parameter));
    });

    private static Fin<Option<Point2d>> SurfaceAt(ObjRef reference, Op key) => key.Catch(() => {
        using Surface? surface = reference.SurfaceParameter(u: out double u, v: out double v);
        return Fin.Succ(value: surface is null ? Option<Point2d>.None : Some(new Point2d(x: u, y: v)));
    });

    public static Fin<Seq<PickCapture>> CaptureOwned(IEnumerable<ObjRef> references, Op key) {
        return key.Need(references).Bind(source => key.Catch(() => {
            List<ObjRef> owned = [];
            try {
                foreach (ObjRef reference in source) owned.Add(reference);
                return toSeq(owned)
                    .Traverse(reference => Capture(reference: reference, key: key).ToValidation())
                    .As()
                    .ToFin();
            } finally {
                owned.Iter(static reference => { if (reference is not null) reference.Dispose(); });
            }
        }));
    }

    public static Fin<TOut> Part<TOut>(ObjRef reference, PartKind ask, Func<Picked, Fin<TOut>> project, Op key)
        where TOut : notnull =>
        from _ in guard(RhinoApp.IsOnMainThread, key.InvalidContext())
        from active in key.Need(reference)
        from kind in key.Need(ask)
        from body in key.Need(project)
        from part in key.Catch(() => kind.Project(reference: active)
            .ToFin(key.Unsupported(geometryType: typeof(ObjRef), outputType: typeof(Picked))))
        from result in key.Catch(() => body(arg: part))
        select result;

    public static Fin<GeometryHandle> Retain(ObjRef reference, PartKind ask, Op key) =>
        Part(
            reference: reference,
            ask: ask,
            project: part => part.Geometry
                .ToFin(key.Unsupported(geometryType: typeof(Picked), outputType: typeof(GeometryBase)))
                .Bind(geometry => GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach, key: key)),
            key: key);

    public static Fin<PickReceipt> Execute(DocumentSession session, PickPolicy policy) {
        Op op = Op.Of();
        return from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from target in op.Need(session)
               from active in op.Need(policy)
               from receipt in target.Demand(
                   use: document =>
                       from defaultView in Optional(document.Views.ActiveView).ToFin(Fail: op.MissingContext())
                       from projected in op.Catch(() => {
                           using PickContext context = new() { View = defaultView };
                           return active.Rules
                               .FoldM<Fin, PickContext>(
                                   context,
                                   (target, rule) => rule.Apply(context: target, key: op).Map(_ => target))
                               .Bind(target => op.Catch(() => {
                                   ObjRef[] references = document.Objects.PickObjects(pickContext: target);
                                   bool getterParticipated = target.GetObjectUsed is not null;
                                   return CaptureOwned(references: references, key: op).Map(captures => new PickReceipt(
                                       GetterParticipated: getterParticipated,
                                       Captures: captures));
                               }));
                       })
                       select projected,
                   key: op,
                   needs: [SessionNeed.Read])
               select receipt;
    }

    public static Fin<Seq<TOut>> Measured<TOut>(
        DocumentSession session,
        AnalysisQuery ask,
        Seq<GeometryBase> subjects)
        where TOut : notnull {
        Op op = Op.Of();
        return from active in op.Need(session)
               from query in op.Need(ask)
               from _ in guard(
                   !subjects.IsEmpty && subjects.ForAll(static shape => shape is not null),
                   op.InvalidInput()).ToFin()
               from domain in active.Context(key: op)
               from measured in Analyze.In(context: domain)
                   .Run(operation: Analyze.Query<GeometryBase, TOut>(query: query, key: op), input: [.. subjects])
                   .ToFin()
               select measured;
    }
}
```

## [06]-[BOUNDARY]

`PickCapture` crosses into `Objects` as detached identity and selection evidence, and `GeometryHandle` crosses into document geometry custody. `PickPolicy` is durable by design and holds only detached rows — `PickView` serials, keyed `PickGesture`/`PickRender`, admitted value structs — so no `ObjRef`, `RhinoView`, `PickContext`, or live geometry payload becomes durable state. `Picked` is call-window-bounded structurally rather than by convention: `Part` scopes it into one projection and no entry on the page returns it.

`Commands/**` is S1 and the marshal seam `HostThread` is S2, so an affinity guard here reads `RhinoApp.IsOnMainThread` directly — composing the seam downward is the forbidden upward edge, and the cost of that carve is that command-lane crossings stay off the marshal-latency ledger.

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
