# [RASM_RHINO_SELECTION]

`Picks` owns native-reference projection, programmatic picking, detached evidence, geometry retention, and measured-query re-entry. Every owned `ObjRef` is projected and disposed inside one terminal window; every borrowed `ObjRef` remains scoped to the caller.

## [01]-[EVIDENCE]

`PickCapture` carries durable object identity, view identity, detail identity, and an evidence-shaped `PickOrigin`. Its outer admission re-validates every public nested case and rebuilds canonical evidence before storage. `PickOrigin` replaces the raw host `SelectionMethod` wire with the total point/curve/surface evidence product; its positive native code remains diagnostic data, never a branch vocabulary.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PickOrigin {
    private PickOrigin() { }
    public sealed record Point(int NativeCode, Point3d Value) : PickOrigin;
    public sealed record Curve(int NativeCode, Point3d Point, double Parameter) : PickOrigin;
    public sealed record Surface(int NativeCode, Point3d Point, Point2d Uv) : PickOrigin;
    public sealed record CurveOnSurface(
        int NativeCode,
        Point3d Point,
        double Parameter,
        Point2d Uv) : PickOrigin;

    internal Fin<PickOrigin> Admit(Op key) => this switch {
        Point row => Admit(row.NativeCode, row.Value, None, None, key),
        Curve row => Admit(row.NativeCode, row.Point, Some(row.Parameter), None, key),
        Surface row => Admit(row.NativeCode, row.Point, None, Some(row.Uv), key),
        CurveOnSurface row => Admit(row.NativeCode, row.Point, Some(row.Parameter), Some(row.Uv), key),
    };

    internal static Fin<PickOrigin> Admit(
        int nativeCode,
        Point3d point,
        Option<double> curve,
        Option<Point2d> surface,
        Op key) =>
        from _ in guard(
                flag: nativeCode > 0
                    && point.IsValid
                    && curve.ForAll(double.IsFinite)
                    && surface.ForAll(static uv => uv.IsValid),
                False: key.InvalidResult())
            .ToFin()
        select (curve.Case, surface.Case) switch {
            (double parameter, Point2d uv) => (PickOrigin)new CurveOnSurface(
                NativeCode: nativeCode, Point: point, Parameter: parameter, Uv: uv),
            (double parameter, _) => new Curve(NativeCode: nativeCode, Point: point, Parameter: parameter),
            (_, Point2d uv) => new Surface(NativeCode: nativeCode, Point: point, Uv: uv),
            _ => new Point(NativeCode: nativeCode, Value: point),
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
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PickCapture(
    Guid ObjectId,
    ComponentIndex Component,
    PickOrigin Origin,
    Option<PickView> View) {
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

## [02]-[PARTS]

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

## [03]-[POLICY]

`PickPolicy` is generated from `PickRule` data. One row owns each independent `PickContext` dimension, and duplicate dimensions fail admission; adding a host dimension extends the case family instead of widening a constructor bag.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PickRule : ISlotted {
    private PickRule() { }
    public sealed record InView(RhinoView Value) : PickRule;
    public sealed record Along(Line Value) : PickRule;
    public sealed record Styled(PickStyle Value) : PickRule;
    public sealed record Rendered(PickMode Value) : PickRule;
    public sealed record Grouped(bool Enabled) : PickRule;
    public sealed record SubObjected(bool Enabled) : PickRule;
    public sealed record Transformed(Transform Value) : PickRule;
    public sealed record RefreshClipping : PickRule;

    public virtual object SlotKey => GetType();

    internal Fin<Unit> Admit(Op key) => guard(this switch {
        InView row => row.Value is not null,
        Along row => row.Value.IsValid,
        Styled row => Enum.IsDefined(row.Value),
        Rendered row => Enum.IsDefined(row.Value),
        Transformed row => row.Value.IsValid,
        _ => true,
    }, key.InvalidInput()).ToFin();

    internal Fin<Unit> Apply(PickContext context, Op key) => key.Catch(() => {
        Switch(
            state: context,
            inView: static (target, rule) => { target.View = rule.Value; return unit; },
            along: static (target, rule) => { target.PickLine = rule.Value; return unit; },
            styled: static (target, rule) => { target.PickStyle = rule.Value; return unit; },
            rendered: static (target, rule) => { target.PickMode = rule.Value; return unit; },
            grouped: static (target, rule) => { target.PickGroupsEnabled = rule.Enabled; return unit; },
            subObjected: static (target, rule) => { target.SubObjectSelectionEnabled = rule.Enabled; return unit; },
            transformed: static (target, rule) => { target.SetPickTransform(rule.Value); return unit; },
            refreshClipping: static (target, _) => { target.UpdateClippingPlanes(); return unit; });
        return Fin.Succ(unit);
    });
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PickPolicy {
    private PickPolicy(Seq<PickRule> rules) => Rules = rules;

    public Seq<PickRule> Rules { get; }

    public static PickPolicy PointAt { get; } = new(rules: [
        new PickRule.Styled(Value: PickStyle.PointPick),
        new PickRule.Rendered(Value: PickMode.Shaded),
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

## [04]-[PROJECTION]

`Picks.Capture` projects borrowed references without taking custody. `CaptureOwned` consumes a returned reference sequence, accumulates every independent projection failure, and releases every entry. `Execute` derives and disposes one `PickContext`, captures `GetObjectUsed`, and returns only detached evidence.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Picks {
    public static Fin<PickCapture> Capture(ObjRef reference, Op key) =>
        from _ in guard(RhinoApp.IsOnMainThread, key.InvalidContext())
        from active in Optional(reference).ToFin(Fail: key.InvalidInput())
        from capture in key.Catch(() => {
            Point3d point = active.SelectionPoint();
            int methodCode = Convert.ToInt32(active.SelectionMethod(), CultureInfo.InvariantCulture);
            Option<double> curve = active.CurveParameter(parameter: out double t) is null ? None : Some(t);
            Option<Point2d> surface = active.SurfaceParameter(u: out double u, v: out double v) is null
                ? None
                : Some(new Point2d(x: u, y: v));
            uint detailSerial = active.SelectionViewDetailSerialNumber();
            return
                from origin in PickOrigin.Admit(
                    nativeCode: methodCode,
                    point: point,
                    curve: curve,
                    surface: surface,
                    key: key)
                from view in PickView.Admit(
                    view: Optional(active.SelectionView()),
                    detailSerial: detailSerial,
                    key: key)
                from admitted in PickCapture.Admit(
                    objectId: active.ObjectId,
                    component: active.GeometryComponentIndex,
                    origin: origin,
                    view: view,
                    key: key)
                select admitted;
        })
        select capture;

    public static Fin<Seq<PickCapture>> CaptureOwned(IEnumerable<ObjRef> references, Op key) {
        return Optional(references).ToFin(Fail: key.InvalidInput()).Bind(source => key.Catch(() => {
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

    public static Fin<Picked> Part(ObjRef reference, PartKind ask, Op key) =>
        from _ in guard(RhinoApp.IsOnMainThread, key.InvalidContext())
        from active in Optional(reference).ToFin(Fail: key.InvalidInput())
        from kind in Optional(ask).ToFin(Fail: key.InvalidInput())
        from part in key.Catch(() => kind.Project(reference: active)
            .ToFin(key.Unsupported(geometryType: typeof(ObjRef), outputType: typeof(Picked))))
        select part;

    public static Fin<GeometryHandle> Retain(Picked part, Op key) =>
        from active in Optional(part).ToFin(Fail: key.InvalidInput())
        from geometry in active.Geometry
            .ToFin(key.Unsupported(geometryType: typeof(Picked), outputType: typeof(GeometryBase)))
        from handle in GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach, key: key)
        select handle;

    public static Fin<PickReceipt> Execute(DocumentSession session, PickPolicy policy) {
        Op op = Op.Of();
        return from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from target in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(policy).ToFin(Fail: op.InvalidInput())
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

    public static Validation<Error, Seq<TOut>> Measured<TOut>(
        DocumentSession session,
        AnalysisQuery ask,
        Seq<GeometryBase> subjects)
        where TOut : notnull {
        Op op = Op.Of();
        return Optional(session).ToFin(Fail: op.InvalidInput()).Bind(active => active.Context(key: op)).Match(
            Succ: domain => Analyze.In(context: domain)
                .Run(operation: Analyze.Query<GeometryBase, TOut>(query: ask, key: op), input: [.. subjects]),
            Fail: static Validation<Error, Seq<TOut>> (error) => error);
    }
}
```

## [05]-[BOUNDARY]

`PickCapture` crosses into `Objects` as detached identity and selection evidence. `GeometryHandle` crosses into document geometry custody. `Picked` and `PickPolicy` remain call-window values, and no `ObjRef`, `RhinoView`, `PickContext`, or live geometry payload becomes durable state.
