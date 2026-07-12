# [RASM_RHINO_SELECTION]

The `ObjRef` projection owner (`Rasm.Rhino.Commands`). Every admissible projection off a native object reference is a declared capability contract: one `PartKind` row per part — whole object, definition part, grip, brep and its face/edge/trim components, SubD and its face/edge/vertex components, curve, surface, mesh, point, cloud, dot, text, light, hatch, clipping plane — each row a typed delegate onto one `Picked` payload union, so the census-era `FrozenDictionary<Type, Func<ObjRef, object?>>` extractor roster with its reflection-assignability fallback is dead and an inadmissible ask fails typed from the row itself. Selection evidence — method, pick point, view and viewport identity, detail serial, curve parameter, surface uv — captures eagerly into one `PickCapture` snapshot inside the reference window, so no `ObjRef` survives past the seam. Pick execution is one host policy dimension: `PickPolicy` derives the whole `PickContext` configuration, and geometry retention composes the document custody seam so a kept payload is always a leased detached copy. Measured questions over picked geometry re-enter the kernel through the frozen `Analyze` contract.

## [01]-[INDEX]

- [02]-[CAPTURE_EVIDENCE]: `PickCapture` — the eager identity and evidence snapshot.
- [03]-[PART_CONTRACTS]: the `Picked` payload union and the `PartKind` capability rows.
- [04]-[PICK_POLICY]: `PickPolicy` — the `PickContext` host policy dimension and the pick execution entry.
- [05]-[PROJECTION_ENTRY]: `Picks` — capture, part projection, custody retention, and the kernel analysis re-entry.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[CAPTURE_EVIDENCE]

- Owner: `PickCapture` — the one immutable selection snapshot: object identity, component index, the host `SelectionMethod` discriminant carried at the seam, the pick point, the selecting view's runtime serial and viewport id, the page-detail serial, and the mouse-pick parametric evidence (curve parameter, surface uv) projected as absence everywhere the method did not produce them.
- Entry: `Picks.Capture(ObjRef, Op) : Fin<PickCapture>` — the one read site; every evidence member is read inside the call window and the reference never escapes.
- Law: parametric evidence exists only under a mouse pick — `CurveParameter` and `SurfaceParameter` answer garbage under window and crossing selection, so the capture gates both on the method discriminant and projects absence otherwise.
- Law: view identity is serial-keyed — the selecting `RhinoView` projects to `RuntimeSerialNumber` and its viewport to the id, with the page-detail serial resolving the detail viewport when the pick landed inside a layout detail; a consumer re-resolves the live view through the host registry at consumption time.
- Law: an unset pick point is absence — the host answers `Point3d.Unset` for keyboard and scripted selection, and the capture projects it to `None` so no consumer branches on the sentinel.
- Boundary: `SelectionMethod` is a host wire enum carried at this seam only; interior code reads the capture's projected facts, never the discriminant.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PickCapture(
    Guid ObjectId,
    ComponentIndex Component,
    SelectionMethod Method,
    Option<Point3d> PickPoint,
    Option<uint> ViewSerial,
    Option<Guid> ViewportId,
    uint DetailSerial,
    Option<double> CurveParameter,
    Option<Point2d> SurfaceUv);
```

## [03]-[PART_CONTRACTS]

- Owner: `Picked` `[Union]` — one typed case per admissible part payload; `PartKind` `[SmartEnum<int>]` — one capability row per projection, each a `[UseDelegateFromConstructor]` `Project(ObjRef)` delegate answering the typed case or absence, so the requested part, the native member, and the returned payload are one declared contract.
- Law: absence and inadmissibility are distinct — a row whose native member answers null projects `None` (the reference simply is not that part), and `Picks.Part` lifts `None` onto the typed `Unsupported` fault carrying the asked row; no assignability fallback exists, because the grip and whole-object cases are their own rows discriminated by the runtime object type at exactly one site.
- Law: `Picked` payloads are read-window values — the geometry a case carries is document-controlled host state, valid for immediate reading; retention crosses through `Retain`, which detaches onto the document `GeometryHandle` capsule per the crossing custody law, so a stored live part is unreachable through this seam.
- Growth: a new host part is one `Picked` case plus one `PartKind` row; `Part`, `Retain`, and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Picked {
    private Picked() { }
    public sealed record Whole(RhinoObject Value) : Picked;
    public sealed record DefinitionPart(RhinoObject Value) : Picked;
    public sealed record GripPart(GripObject Value) : Picked;
    public sealed record BrepWhole(Brep Value) : Picked;
    public sealed record FacePart(BrepFace Value) : Picked;
    public sealed record EdgePart(BrepEdge Value) : Picked;
    public sealed record TrimPart(BrepTrim Value) : Picked;
    public sealed record SubDWhole(SubD Value) : Picked;
    public sealed record SubDFacePart(SubDFace Value) : Picked;
    public sealed record SubDEdgePart(SubDEdge Value) : Picked;
    public sealed record SubDVertexPart(SubDVertex Value) : Picked;
    public sealed record CurvePart(Curve Value) : Picked;
    public sealed record SurfacePart(Surface Value) : Picked;
    public sealed record MeshPart(Mesh Value) : Picked;
    public sealed record PointPart(global::Rhino.Geometry.Point Value) : Picked;
    public sealed record CloudPart(PointCloud Value) : Picked;
    public sealed record DotPart(TextDot Value) : Picked;
    public sealed record TextPart(TextEntity Value) : Picked;
    public sealed record LightPart(Light Value) : Picked;
    public sealed record HatchPart(Hatch Value) : Picked;
    public sealed record ClipPart(ClippingPlaneSurface Value) : Picked;

    public Option<GeometryBase> Geometry =>
        Switch(
            whole: static _ => Option<GeometryBase>.None,
            definitionPart: static _ => Option<GeometryBase>.None,
            gripPart: static _ => Option<GeometryBase>.None,
            brepWhole: static part => Some<GeometryBase>(part.Value),
            facePart: static part => Some<GeometryBase>(part.Value),
            edgePart: static part => Some<GeometryBase>(part.Value),
            trimPart: static part => Some<GeometryBase>(part.Value),
            subDWhole: static part => Some<GeometryBase>(part.Value),
            subDFacePart: static part => Some<GeometryBase>(part.Value),
            subDEdgePart: static part => Some<GeometryBase>(part.Value),
            subDVertexPart: static part => Some<GeometryBase>(part.Value),
            curvePart: static part => Some<GeometryBase>(part.Value),
            surfacePart: static part => Some<GeometryBase>(part.Value),
            meshPart: static part => Some<GeometryBase>(part.Value),
            pointPart: static part => Some<GeometryBase>(part.Value),
            cloudPart: static part => Some<GeometryBase>(part.Value),
            dotPart: static part => Some<GeometryBase>(part.Value),
            textPart: static part => Some<GeometryBase>(part.Value),
            lightPart: static _ => Option<GeometryBase>.None,
            hatchPart: static part => Some<GeometryBase>(part.Value),
            clipPart: static part => Some<GeometryBase>(part.Value));
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
            : Option<Picked>.None));
    public static readonly PartKind BrepWhole = new(key: 3, project: static reference =>
        Optional(reference.Brep()).Map(static value => (Picked)new Picked.BrepWhole(Value: value)));
    public static readonly PartKind Face = new(key: 4, project: static reference =>
        Optional(reference.Face()).Map(static value => (Picked)new Picked.FacePart(Value: value)));
    public static readonly PartKind Edge = new(key: 5, project: static reference =>
        Optional(reference.Edge()).Map(static value => (Picked)new Picked.EdgePart(Value: value)));
    public static readonly PartKind Trim = new(key: 6, project: static reference =>
        Optional(reference.Trim()).Map(static value => (Picked)new Picked.TrimPart(Value: value)));
    public static readonly PartKind SubDWhole = new(key: 7, project: static reference =>
        Optional(reference.SubD()).Map(static value => (Picked)new Picked.SubDWhole(Value: value)));
    public static readonly PartKind SubDFace = new(key: 8, project: static reference =>
        Optional(reference.SubDFace()).Map(static value => (Picked)new Picked.SubDFacePart(Value: value)));
    public static readonly PartKind SubDEdge = new(key: 9, project: static reference =>
        Optional(reference.SubDEdge()).Map(static value => (Picked)new Picked.SubDEdgePart(Value: value)));
    public static readonly PartKind SubDVertex = new(key: 10, project: static reference =>
        Optional(reference.SubDVertex()).Map(static value => (Picked)new Picked.SubDVertexPart(Value: value)));
    public static readonly PartKind CurveKind = new(key: 11, project: static reference =>
        Optional(reference.Curve()).Map(static value => (Picked)new Picked.CurvePart(Value: value)));
    public static readonly PartKind SurfaceKind = new(key: 12, project: static reference =>
        Optional(reference.Surface()).Map(static value => (Picked)new Picked.SurfacePart(Value: value)));
    public static readonly PartKind MeshKind = new(key: 13, project: static reference =>
        Optional(reference.Mesh()).Map(static value => (Picked)new Picked.MeshPart(Value: value)));
    public static readonly PartKind PointKind = new(key: 14, project: static reference =>
        Optional(reference.Point()).Map(static value => (Picked)new Picked.PointPart(Value: value)));
    public static readonly PartKind Cloud = new(key: 15, project: static reference =>
        Optional(reference.PointCloud()).Map(static value => (Picked)new Picked.CloudPart(Value: value)));
    public static readonly PartKind Dot = new(key: 16, project: static reference =>
        Optional(reference.TextDot()).Map(static value => (Picked)new Picked.DotPart(Value: value)));
    public static readonly PartKind Annotation = new(key: 17, project: static reference =>
        Optional(reference.TextEntity()).Map(static value => (Picked)new Picked.TextPart(Value: value)));
    public static readonly PartKind LightKind = new(key: 18, project: static reference =>
        Optional(reference.Light()).Map(static value => (Picked)new Picked.LightPart(Value: value)));
    public static readonly PartKind HatchKind = new(key: 19, project: static reference =>
        Optional(reference.Hatch()).Map(static value => (Picked)new Picked.HatchPart(Value: value)));
    public static readonly PartKind Clip = new(key: 20, project: static reference =>
        Optional(reference.ClippingPlaneSurface()).Map(static value => (Picked)new Picked.ClipPart(Value: value)));

    [UseDelegateFromConstructor]
    internal partial Option<Picked> Project(ObjRef reference);
}
```

## [04]-[PICK_POLICY]

- Owner: `PickPolicy` — the whole `PickContext` configuration as one host policy value: target view (defaulting to the active view), pick line, pick style and mode carried as the host wire enums at this seam, group and sub-object grants, an optional pick transform, and the clipping-plane refresh decision. `PickReceipt` — the detached pick product: the resolved view's runtime serial and viewport id beside the capture sequence, so a policy that defaulted to the active view still reports which view the pick ran against.
- Entry: `Picks.Execute(DocumentSession, PickPolicy) : Fin<PickReceipt>` — one `Demand` window under `SessionNeed.Read`: resolve the view, derive the context inside a disposal window, run `PickObjects`, and capture every hit before the context closes; an empty pick is a valid empty capture sequence, never a fault.
- Law: the context is derived per pick and disposed with it — `PickContext` state never outlives the call, so two picks never share stale transform or clipping state, and the clipping-plane refresh runs against the resolved view exactly when the policy demands it.
- RESEARCH: the `PickStyle` window and crossing member spellings are unverified beyond `PointPick`; the window and crossing preset rows land as data once the roster verifies, and a caller meanwhile passes the host value directly through `Style`.
- Boundary: which views exist and how a pick renders belong to the viewport and display owners; this policy only crosses the configuration onto the host context.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PickPolicy(
    Option<RhinoView> View = default,
    Option<Line> PickLine = default,
    PickStyle Style = PickStyle.PointPick,
    PickMode Mode = PickMode.Shaded,
    bool PickGroups = false,
    bool SubObjects = true,
    Option<Transform> PickTransform = default,
    bool RefreshClippingPlanes = true) {
    public static PickPolicy PointAt { get; } = new();
}

public sealed record PickReceipt(uint ViewSerial, Guid ViewportId, Seq<PickCapture> Captures) : IDetachedDocumentResult;
```

## [05]-[PROJECTION_ENTRY]

- Owner: `Picks` — the one projection surface: evidence capture, capability projection, custody retention, pick execution, and the kernel measured-query re-entry.
- Entry: `Capture(ObjRef, Op)`, `Part(ObjRef, PartKind, Op) : Fin<Picked>`, `Retain(Picked, Op) : Fin<GeometryHandle>`, `Execute(DocumentSession, PickPolicy)`, and `Measured<TOut>(DocumentSession, AnalysisQuery, Seq<GeometryBase>) : Validation<Error, Seq<TOut>>`.
- Law: retention is the custody seam — a geometry-bearing case crosses through `GeometryCrossing.Cross` under `CrossingMode.Detach`, so the kept payload is always the document unit's `GeometryHandle` capsule over an independent deep copy; the object-shaped cases refuse retention typed, because a `RhinoObject` is table state addressed by id through the table rail, never a leased value.
- Law: measured questions re-enter the kernel through the frozen contract — `session.Context` re-reads the live tolerance bundle, `Analyze.In` scopes it, and `Analyze.Query` builds the operation, so no selection consumer constructs a second analysis path or a stale local tolerance.
- Boundary: this page names `ObjRef` members and `PickObjects` only; id-set addressing, mutation, and selection-state changes ride the document table rail through `TableTarget` and `TableOp.State`.

```csharp
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Picks {
    public static Fin<PickCapture> Capture(ObjRef reference, Op key) =>
        Optional(reference).ToFin(Fail: key.InvalidInput()).Bind(active => key.Catch(() => {
            SelectionMethod method = active.SelectionMethod();
            Point3d point = active.SelectionPoint();
            RhinoView? view = active.SelectionView();
            uint detailSerial = active.SelectionViewDetailSerialNumber();
            Option<double> curve = method is SelectionMethod.MousePick && active.CurveParameter(parameter: out double t) is not null
                ? Some(t)
                : Option<double>.None;
            Option<Point2d> surface = method is SelectionMethod.MousePick && active.SurfaceParameter(u: out double u, v: out double v) is not null
                ? Some(new Point2d(x: u, y: v))
                : Option<Point2d>.None;
            return Fin.Succ(value: new PickCapture(
                ObjectId: active.ObjectId,
                Component: active.GeometryComponentIndex,
                Method: method,
                PickPoint: point.IsValid ? Some(point) : Option<Point3d>.None,
                ViewSerial: Optional(view).Map(static live => live.RuntimeSerialNumber),
                ViewportId: Optional(view).Map(static live => live.ActiveViewportID),
                DetailSerial: detailSerial,
                CurveParameter: curve,
                SurfaceUv: surface));
        }));

    public static Fin<Picked> Part(ObjRef reference, PartKind ask, Op key) =>
        from active in Optional(reference).ToFin(Fail: key.InvalidInput())
        from part in ask.Project(reference: active).ToFin(key.Unsupported(geometryType: typeof(ObjRef), outputType: typeof(Picked)))
        select part;

    public static Fin<GeometryHandle> Retain(Picked part, Op key) =>
        from active in Optional(part).ToFin(Fail: key.InvalidInput())
        from geometry in active.Geometry.ToFin(key.Unsupported(geometryType: typeof(Picked), outputType: typeof(GeometryBase)))
        from handle in GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach, key: key)
        select handle;

    public static Fin<PickReceipt> Execute(DocumentSession session, PickPolicy policy) {
        Op op = Op.Of();
        return from active in Optional(policy).ToFin(Fail: op.InvalidInput())
               from receipt in session.Demand(
                   use: document =>
                       from view in active.View.Case switch {
                           RhinoView chosen => Fin.Succ(value: chosen),
                           _ => Optional(document.Views.ActiveView).ToFin(Fail: op.MissingContext()),
                       }
                       from captures in op.Catch(() => {
                           using PickContext context = new();
                           context.View = view;
                           _ = active.PickLine.Iter(line => context.PickLine = line);
                           context.PickStyle = active.Style;
                           context.PickMode = active.Mode;
                           context.PickGroupsEnabled = active.PickGroups;
                           context.SubObjectSelectionEnabled = active.SubObjects;
                           _ = active.PickTransform.Iter(transform => context.SetPickTransform(transform));
                           _ = Op.SideWhen(active.RefreshClippingPlanes, context.UpdateClippingPlanes);
                           return toSeq(document.Objects.PickObjects(pickContext: context))
                               .TraverseM(reference => Capture(reference: reference, key: op)).As();
                       })
                       select new PickReceipt(
                           ViewSerial: view.RuntimeSerialNumber, ViewportId: view.ActiveViewportID, Captures: captures),
                   key: op,
                   needs: [SessionNeed.Read])
               select receipt;
    }

    public static Validation<Error, Seq<TOut>> Measured<TOut>(DocumentSession session, AnalysisQuery ask, Seq<GeometryBase> subjects)
        where TOut : notnull {
        Op op = Op.Of();
        return session.Context(key: op).Match(
            Succ: domain => Analyze.In(context: domain)
                .Run(operation: Analyze.Query<GeometryBase, TOut>(query: ask, key: op), input: [.. subjects]),
            Fail: static Validation<Error, Seq<TOut>> (error) => error);
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]       | [FORM]                                             | [ENTRY]                   |
| :-----: | :------------------ | :------------ | :------------------------------------------------- | :------------------------ |
|  [01]   | selection evidence  | `PickCapture` | eager snapshot inside the reference window         | `Picks.Capture`           |
|  [02]   | part payloads       | `Picked`      | one union, typed cases, geometry projection        | `Picks.Part` / `Geometry` |
|  [03]   | capability contract | `PartKind`    | rows with typed `Project(ObjRef)` delegate columns | `Project(reference)`      |
|  [04]   | pick configuration  | `PickPolicy`  | one host policy value over `PickContext`           | `Picks.Execute`           |
|  [05]   | custody retention   | `Picks`       | detach onto `GeometryHandle` via the crossing      | `Retain(part, key)`       |
|  [06]   | measured re-entry   | `Picks`       | frozen `Analyze`/`AnalysisQuery`/`Env` contract    | `Measured<TOut>`          |
