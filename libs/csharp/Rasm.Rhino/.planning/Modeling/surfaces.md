# [RASM_RHINO_MODELING_SURFACES]

`Rasm.Rhino.Modeling` owns freeform surface construction. One `SurfaceOp` union carries network fitting with error evidence, rail revolve, point-grid interpolation, ruled and corner surfaces, curve-on-surface fitting, subd-friendly rebuilds, analytic seeding, compatibility reparameterization, iso-edge matching, extrusion, periodic closure, soft editing, rolling-ball fillets, tween sampling, sum surfaces, bounded planes, and value-semantic fit/rebuild through `Surfaces.Build`. Input shape selects each overload family, and `SurfaceFitLaw` collapses the full instance fit/rebuild family. Kernel NURBS evaluation, division, tessellation, and analysis remain kernel-owned; `Context` supplies every tolerance.

## [01]-[INDEX]

- [02]-[FIT_POLICY]: `NetContinuity`, `GridFit`, `SurfaceFitLaw`, `ExtrudeTerminal`, `RollingSeed`, `AnalyticSeed`, `SurfaceForm`, `PlaneFrame`, `RevolveProfile` — the construction discriminants.
- [03]-[OPERATION_RAIL]: `SurfaceSlot`, `SurfaceOp`, and the `Surfaces.Build` entry.
- [04]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[FIT_POLICY]

- Owner: `NetContinuity` `[SmartEnum<int>]` — network boundary-continuity codes keyed by the native integer; `GridFit` `[Union]` — control-point or through-point fitting; `SurfaceFitLaw` `[Union]` — tolerance fit, grid rebuild, or directional rebuild; `ExtrudeTerminal`, `RollingSeed`, `AnalyticSeed`, `SurfaceForm`, `PlaneFrame`, and `RevolveProfile` — construction discriminants carrying only modality-valid payloads.
- Law: the continuity code never travels bare — `NetContinuity` keys the native integer so a network arm reads `(int)row`, and an out-of-vocabulary code is unconstructible.
- Law: one analytic vocabulary serves two representations — `AnalyticSeed` holds the cone, cylinder, sphere, or torus value once and `SurfaceForm` selects the `NurbsSurface.CreateFrom*` or `RevSurface.CreateFrom*` constructor, so the four primitives never duplicate per representation.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class NetContinuity {
    public static readonly NetContinuity Loose = new(key: 0);
    public static readonly NetContinuity Position = new(key: 1);
    public static readonly NetContinuity Tangent = new(key: 2);
    public static readonly NetContinuity Curvature = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class SurfaceForm {
    public static readonly SurfaceForm Nurbs = new(key: 0);
    public static readonly SurfaceForm Revolved = new(key: 1);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GridFit {
    private GridFit() { }
    public sealed record Control : GridFit;
    public sealed record Through(bool UClosed = false, bool VClosed = false) : GridFit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceFitLaw {
    private SurfaceFitLaw() { }
    public sealed record ToTolerance(int UDegree, int VDegree) : SurfaceFitLaw;
    public sealed record ToGrid(int UDegree, int VDegree, int UPoints, int VPoints) : SurfaceFitLaw;
    public sealed record InDirection(int Direction, int PointCount, LoftType Kind) : SurfaceFitLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrudeTerminal {
    private ExtrudeTerminal() { }
    public sealed record Along(Vector3d Direction) : ExtrudeTerminal;
    public sealed record ToApex(Point3d Apex) : ExtrudeTerminal;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RollingSeed {
    private RollingSeed() { }
    public sealed record Auto : RollingSeed;
    public sealed record Flipped(bool FlipFirst, bool FlipSecond) : RollingSeed;
    public sealed record AtUv(Point2d First, Point2d Second) : RollingSeed;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticSeed {
    private AnalyticSeed() { }
    public sealed record OfCone(Cone Value) : AnalyticSeed;
    public sealed record OfCylinder(Cylinder Value) : AnalyticSeed;
    public sealed record OfSphere(Sphere Value) : AnalyticSeed;
    public sealed record OfTorus(Torus Value) : AnalyticSeed;

    internal Fin<GeometryHandle> Build(SurfaceForm form, Op key) =>
        Switch(
            context: (Form: form, Op: key),
            ofCone: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: ctx.Form == SurfaceForm.Nurbs
                ? NurbsSurface.CreateFromCone(cone: seed.Value) : RevSurface.CreateFromCone(cone: seed.Value), key: ctx.Op)),
            ofCylinder: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: ctx.Form == SurfaceForm.Nurbs
                ? NurbsSurface.CreateFromCylinder(cylinder: seed.Value) : RevSurface.CreateFromCylinder(cylinder: seed.Value), key: ctx.Op)),
            ofSphere: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: ctx.Form == SurfaceForm.Nurbs
                ? NurbsSurface.CreateFromSphere(sphere: seed.Value) : RevSurface.CreateFromSphere(sphere: seed.Value), key: ctx.Op)),
            ofTorus: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: ctx.Form == SurfaceForm.Nurbs
                ? NurbsSurface.CreateFromTorus(torus: seed.Value) : RevSurface.CreateFromTorus(torus: seed.Value), key: ctx.Op)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlaneFrame {
    private PlaneFrame() { }
    public sealed record OfPlane(Plane Value) : PlaneFrame;
    public sealed record OfLine(Line LineInPlane, Vector3d VectorInPlane) : PlaneFrame;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RevolveProfile {
    private RevolveProfile() { }
    public sealed record OfCurve(GeometryHandle Value) : RevolveProfile;
    public sealed record OfLine(Line Value) : RevolveProfile;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SumExtent {
    private SumExtent() { }
    public sealed record ByDirection(Vector3d Direction) : SumExtent;
    public sealed record ByCurve(GeometryHandle Second) : SumExtent;
}
```

## [03]-[OPERATION_RAIL]

- Owner: `SurfaceSlot` `[SmartEnum<int>]` — the consequence vocabulary; `SurfaceOp` `[Union]` — the whole verified freeform-construction verb roster; `Surfaces` — the one entry folding any operation spread into one `Built<SurfaceSlot>`.
- Law: the network error code is evidence — `CreateNetworkSurface` folds its `out int error` into a `Code` fact beside the surface, and the single-set auto-sorted overload rides the same case through an empty v-set, so failure diagnosis never re-runs the fit.
- Law: geodesic fitting has two modalities on one vocabulary — `GeodesicCurve` answers the fitted `NurbsCurve` as a product, `GeodesicSamples` answers the intermediate uv rows as a `UvRows` fact with no product; both consume the same surface lease and point rows.
- Law: fit and rebuild are value-semantic constructions — `SurfaceFitLaw` selects tolerance fit, grid rebuild, or directional rebuild inside one `Fit` arm, and each member owns the returned surface without mutating the input handle.
- Law: compatibility answers pairs — `MakeCompatible` confirms and crosses both reparameterized surfaces with symmetric disposal on a half-crossed failure.
- Law: variable offsetting is corner-driven construction — `VariableOffset` carries the four corner distances plus optional interior `(uv, distance)` rows, the row set selects the host overload, and the offset tolerance derives from the domain absolute tolerance, never a payload literal.
- Growth: a new freeform constructor is one case with its arm; the spine and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SurfaceSlot {
    public static readonly SurfaceSlot Networked = new(key: 0);
    public static readonly SurfaceSlot Revolved = new(key: 1);
    public static readonly SurfaceSlot Gridded = new(key: 2);
    public static readonly SurfaceSlot Cornered = new(key: 3);
    public static readonly SurfaceSlot Ruled = new(key: 4);
    public static readonly SurfaceSlot Geodesic = new(key: 5);
    public static readonly SurfaceSlot SubDReady = new(key: 6);
    public static readonly SurfaceSlot Seeded = new(key: 7);
    public static readonly SurfaceSlot Compatible = new(key: 8);
    public static readonly SurfaceSlot EdgeMatched = new(key: 9);
    public static readonly SurfaceSlot Extruded = new(key: 10);
    public static readonly SurfaceSlot Closed = new(key: 11);
    public static readonly SurfaceSlot SoftEdited = new(key: 12);
    public static readonly SurfaceSlot Filleted = new(key: 13);
    public static readonly SurfaceSlot Tweened = new(key: 14);
    public static readonly SurfaceSlot Summed = new(key: 15);
    public static readonly SurfaceSlot Bounded = new(key: 16);
    public static readonly SurfaceSlot Refitted = new(key: 17);
    public static readonly SurfaceSlot Offsetted = new(key: 18);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceOp {
    private SurfaceOp() { }
    public sealed record Network(
        Seq<GeometryHandle> UCurves, NetContinuity UStart, NetContinuity UEnd,
        Seq<GeometryHandle> VCurves, NetContinuity VStart, NetContinuity VEnd) : SurfaceOp;
    public sealed record RailRevolve(GeometryHandle Profile, GeometryHandle Rail, Line Axis, bool ScaleHeight = false) : SurfaceOp;
    public sealed record Grid(Seq<Point3d> Points, int UCount, int VCount, int UDegree, int VDegree, GridFit Fit) : SurfaceOp;
    public sealed record Corners(Point3d A, Point3d B, Point3d C, Option<Point3d> D = default) : SurfaceOp;
    public sealed record Ruled(GeometryHandle First, GeometryHandle Second) : SurfaceOp;
    public sealed record GeodesicCurve(GeometryHandle Surface, Seq<Point2d> Points, bool Periodic = false) : SurfaceOp;
    public sealed record GeodesicSamples(GeometryHandle Surface, Seq<Point2d> FixedPoints, bool Periodic, int InitCount, int Levels) : SurfaceOp;
    public sealed record SubDFriendly(GeometryHandle Surface) : SurfaceOp;
    public sealed record Seed(AnalyticSeed Value, SurfaceForm Form) : SurfaceOp;
    public sealed record PlaneGrid(Plane Frame, Interval U, Interval V, int UDegree, int VDegree, int UPoints, int VPoints) : SurfaceOp;
    public sealed record Compatible(GeometryHandle First, GeometryHandle Second) : SurfaceOp;
    public sealed record MatchEdge(GeometryHandle Surface, IsoStatus Side, GeometryHandle TargetCurve, double MaxEndDistance, double MaxInteriorDistance, int MaxLevel) : SurfaceOp;
    public sealed record Extruded(GeometryHandle Profile, ExtrudeTerminal Terminal) : SurfaceOp;
    public sealed record Periodic(GeometryHandle Surface, int Direction, bool Smooth = true) : SurfaceOp;
    public sealed record SoftEdit(GeometryHandle Surface, Point2d Uv, Vector3d Delta, double ULength, double VLength, bool FixEnds = true) : SurfaceOp;
    public sealed record RollingBall(GeometryHandle First, GeometryHandle Second, double Radius, RollingSeed At) : SurfaceOp;
    public sealed record Tween(GeometryHandle First, GeometryHandle Second, int Count, int Samples) : SurfaceOp;
    public sealed record Sum(GeometryHandle Profile, SumExtent Extent) : SurfaceOp;
    public sealed record BoundedPlane(PlaneFrame Frame, BoundingBox Box) : SurfaceOp;
    public sealed record Revolve(RevolveProfile Profile, Line Axis, Option<(double StartRadians, double EndRadians)> Sweep = default) : SurfaceOp;
    public sealed record Fit(GeometryHandle Surface, SurfaceFitLaw Law) : SurfaceOp;
    public sealed record VariableOffset(
        GeometryHandle Surface, double UMinVMin, double UMinVMax, double UMaxVMin, double UMaxVMax,
        Seq<(Point2d Uv, double Distance)> Interior = default) : SurfaceOp;

    internal Fin<Built<SurfaceSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            network: static (model, edit) => {
                Op op = Op.Of(name: nameof(Network));
                return ModelGate.BorrowMany<Curve, Built<SurfaceSlot>>(handles: edit.UCurves, key: op, body: uCurves =>
                    ModelGate.BorrowMany<Curve, Built<SurfaceSlot>>(handles: edit.VCurves, key: op, allowEmpty: true, body: vCurves =>
                        op.Catch(() => {
                            int error;
                            NurbsSurface fitted = vCurves.IsEmpty
                                ? NurbsSurface.CreateNetworkSurface(
                                    curves: uCurves.AsIterable(), continuity: (int)edit.UStart,
                                    edgeTolerance: model.Absolute.Value,
                                    interiorTolerance: model.Absolute.Value,
                                    angleTolerance: model.Angle.Value, error: out error)
                                : NurbsSurface.CreateNetworkSurface(
                                    uCurves: uCurves.AsIterable(), uContinuityStart: (int)edit.UStart, uContinuityEnd: (int)edit.UEnd,
                                    vCurves: vCurves.AsIterable(), vContinuityStart: (int)edit.VStart, vContinuityEnd: (int)edit.VEnd,
                                    edgeTolerance: model.Absolute.Value,
                                    interiorTolerance: model.Absolute.Value,
                                    angleTolerance: model.Angle.Value, error: out error);
                            return ModelGate.Own(built: fitted, key: op)
                                .MapFail(fault => fault + op.InvalidResult(detail: error.ToString(CultureInfo.InvariantCulture)))
                                .Map(owned => new Built<SurfaceSlot>(
                                    Products: Seq(owned),
                                    Evidence: BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Networked, body: new BuildBody.Tally(Count: 1))
                                        + BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Networked, body: new BuildBody.Code(Value: error))));
                        })));
            },
            railRevolve: static (_, edit) => {
                Op op = Op.Of(name: nameof(RailRevolve));
                return ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Profile, key: op, body: profile =>
                    ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Rail, key: op, body: rail =>
                        Single(op, SurfaceSlot.Revolved, () => NurbsSurface.CreateRailRevolvedSurface(
                            profile: profile, rail: rail, axis: edit.Axis, scaleHeight: edit.ScaleHeight))));
            },
            grid: static (_, edit) => {
                Op op = Op.Of(name: nameof(Grid));
                return
                    from _ in guard(edit.Points.Count == edit.UCount * edit.VCount, op.InvalidInput())
                    from built in edit.Fit.Switch(
                        state: (Edit: edit, Op: op),
                        control: static ctx => Single(ctx.Op, SurfaceSlot.Gridded, () => NurbsSurface.CreateFromPoints(
                            points: ctx.Edit.Points.AsIterable(), uCount: ctx.Edit.UCount, vCount: ctx.Edit.VCount,
                            uDegree: ctx.Edit.UDegree, vDegree: ctx.Edit.VDegree)),
                        through: static (ctx, fit) => Single(ctx.Op, SurfaceSlot.Gridded, () => NurbsSurface.CreateThroughPoints(
                            points: ctx.Edit.Points.AsIterable(), uCount: ctx.Edit.UCount, vCount: ctx.Edit.VCount,
                            uDegree: ctx.Edit.UDegree, vDegree: ctx.Edit.VDegree, uClosed: fit.UClosed, vClosed: fit.VClosed)))
                    select built;
            },
            corners: static (model, edit) => {
                Op op = Op.Of(name: nameof(Corners));
                return Single(op, SurfaceSlot.Cornered, () => edit.D.Case switch {
                    Point3d d => NurbsSurface.CreateFromCorners(corner1: edit.A, corner2: edit.B, corner3: edit.C, corner4: d, tolerance: model.Absolute.Value),
                    _ => NurbsSurface.CreateFromCorners(corner1: edit.A, corner2: edit.B, corner3: edit.C),
                });
            },
            ruled: static (_, edit) => {
                Op op = Op.Of(name: nameof(Ruled));
                return ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Second, key: op, body: second =>
                        Single(op, SurfaceSlot.Ruled, () => NurbsSurface.CreateRuledSurface(curveA: first, curveB: second))));
            },
            geodesicCurve: static (model, edit) => {
                Op op = Op.Of(name: nameof(GeodesicCurve));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    Single(op, SurfaceSlot.Geodesic, () => NurbsSurface.CreateCurveOnSurface(
                        surface: surface, points: edit.Points.AsIterable(), tolerance: model.Absolute.Value, periodic: edit.Periodic)));
            },
            geodesicSamples: static (model, edit) => {
                Op op = Op.Of(name: nameof(GeodesicSamples));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    op.Catch(() => Optional(NurbsSurface.CreateCurveOnSurfacePoints(
                            surface: surface, fixedPoints: edit.FixedPoints.AsIterable(), tolerance: model.Absolute.Value,
                            periodic: edit.Periodic, initCount: edit.InitCount, levels: edit.Levels))
                        .ToFin(Fail: op.InvalidResult())
                        .Map(samples => new Built<SurfaceSlot>(
                            Products: Seq<GeometryHandle>(),
                            Evidence: BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Geodesic, body: new BuildBody.UvRows(Rows: toSeq(samples)))))));
            },
            subDFriendly: static (_, edit) => {
                Op op = Op.Of(name: nameof(SubDFriendly));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    Single(op, SurfaceSlot.SubDReady, () => NurbsSurface.CreateSubDFriendly(surface: surface)));
            },
            seed: static (_, edit) => {
                Op op = Op.Of(name: nameof(Seed));
                return edit.Value.Build(form: edit.Form, key: op).Map(product => new Built<SurfaceSlot>(
                    Products: Seq(product),
                    Evidence: BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Seeded, body: new BuildBody.Tally(Count: 1))));
            },
            planeGrid: static (_, edit) => {
                Op op = Op.Of(name: nameof(PlaneGrid));
                return Single(op, SurfaceSlot.Seeded, () => NurbsSurface.CreateFromPlane(
                    plane: edit.Frame, uInterval: edit.U, vInterval: edit.V,
                    uDegree: edit.UDegree, vDegree: edit.VDegree, uPointCount: edit.UPoints, vPointCount: edit.VPoints));
            },
            compatible: static (_, edit) => {
                Op op = Op.Of(name: nameof(Compatible));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Second, key: op, body: second =>
                        op.Catch(() =>
                            op.Confirm(success: NurbsSurface.MakeCompatible(
                                surface0: first, surface1: second, nurb0: out NurbsSurface nurb0, nurb1: out NurbsSurface nurb1))
                            .Bind(_ =>
                                from owned in ModelGate.Own(built: nurb0, key: op)
                                from other in ModelGate.Own(built: nurb1, key: op).MapFail(error => { owned.Dispose(); return error; })
                                select new Built<SurfaceSlot>(
                                    Products: Seq(owned, other),
                                    Evidence: BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Compatible, body: new BuildBody.Tally(Count: 2)))))));
            },
            matchEdge: static (model, edit) => {
                Op op = Op.Of(name: nameof(MatchEdge));
                return ModelGate.Borrow<NurbsSurface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.TargetCurve, key: op, body: target =>
                        Single(op, SurfaceSlot.EdgeMatched, () => surface.MatchToCurve(
                            side: edit.Side, targetCurve: target, maxEndDistance: edit.MaxEndDistance,
                            maxInteriorDistance: edit.MaxInteriorDistance, matchTolerance: model.Absolute.Value, maxLevel: edit.MaxLevel))));
            },
            extruded: static (_, edit) => {
                Op op = Op.Of(name: nameof(Extruded));
                return ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Profile, key: op, body: profile =>
                    edit.Terminal.Switch(
                        state: (Profile: profile, Op: op),
                        along: static (ctx, terminal) => Single(ctx.Op, SurfaceSlot.Extruded, () => Surface.CreateExtrusion(
                            profile: ctx.Profile, direction: terminal.Direction)),
                        toApex: static (ctx, terminal) => Single(ctx.Op, SurfaceSlot.Extruded, () => Surface.CreateExtrusionToPoint(
                            profile: ctx.Profile, apexPoint: terminal.Apex))));
            },
            periodic: static (_, edit) => {
                Op op = Op.Of(name: nameof(Periodic));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    from _ in guard(edit.Direction is 0 or 1, op.InvalidInput())
                    from built in Single(op, SurfaceSlot.Closed, () => Surface.CreatePeriodicSurface(
                        surface: surface, direction: edit.Direction, bSmooth: edit.Smooth))
                    select built);
            },
            softEdit: static (model, edit) => {
                Op op = Op.Of(name: nameof(SoftEdit));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    Single(op, SurfaceSlot.SoftEdited, () => Surface.CreateSoftEditSurface(
                        surface: surface, uv: edit.Uv, delta: edit.Delta, uLength: edit.ULength, vLength: edit.VLength,
                        tolerance: model.Absolute.Value, fixEnds: edit.FixEnds)));
            },
            rollingBall: static (model, edit) => {
                Op op = Op.Of(name: nameof(RollingBall));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Second, key: op, body: second =>
                        edit.At.Switch(
                            state: (First: first, Second: second, Radius: edit.Radius, Tolerance: model.Absolute.Value, Op: op),
                            auto: static ctx => Many(ctx.Op, SurfaceSlot.Filleted, () => Surface.CreateRollingBallFillet(
                                surfaceA: ctx.First, surfaceB: ctx.Second, radius: ctx.Radius, tolerance: ctx.Tolerance)),
                            flipped: static (ctx, seed) => Many(ctx.Op, SurfaceSlot.Filleted, () => Surface.CreateRollingBallFillet(
                                surfaceA: ctx.First, flipA: seed.FlipFirst, surfaceB: ctx.Second, flipB: seed.FlipSecond,
                                radius: ctx.Radius, tolerance: ctx.Tolerance)),
                            atUv: static (ctx, seed) => Many(ctx.Op, SurfaceSlot.Filleted, () => Surface.CreateRollingBallFillet(
                                surfaceA: ctx.First, uvA: seed.First, surfaceB: ctx.Second, uvB: seed.Second,
                                radius: ctx.Radius, tolerance: ctx.Tolerance)))));
            },
            tween: static (model, edit) => {
                Op op = Op.Of(name: nameof(Tween));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Second, key: op, body: second =>
                        Many(op, SurfaceSlot.Tweened, () => Surface.CreateTweenSurfacesWithSampling(
                            surface0: first, surface1: second, numSurfaces: edit.Count, numSamples: edit.Samples, tolerance: model.Absolute.Value))));
            },
            sum: static (_, edit) => {
                Op op = Op.Of(name: nameof(Sum));
                return ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Profile, key: op, body: profile =>
                    edit.Extent.Switch(
                        state: (Profile: profile, Op: op),
                        byDirection: static (ctx, extent) => Single(ctx.Op, SurfaceSlot.Summed, () => SumSurface.Create(
                            curve: ctx.Profile, extrusionDirection: extent.Direction)),
                        byCurve: static (ctx, extent) => ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: extent.Second, key: ctx.Op,
                            body: second => Single(ctx.Op, SurfaceSlot.Summed, () => SumSurface.Create(curveA: ctx.Profile, curveB: second)))));
            },
            boundedPlane: static (_, edit) => {
                Op op = Op.Of(name: nameof(BoundedPlane));
                return edit.Frame.Switch(
                    state: (Box: edit.Box, Op: op),
                    ofPlane: static (ctx, frame) => Single(ctx.Op, SurfaceSlot.Bounded, () => PlaneSurface.CreateThroughBox(
                        plane: frame.Value, box: ctx.Box)),
                    ofLine: static (ctx, frame) => Single(ctx.Op, SurfaceSlot.Bounded, () => PlaneSurface.CreateThroughBox(
                        lineInPlane: frame.LineInPlane, vectorInPlane: frame.VectorInPlane, box: ctx.Box)));
            },
            revolve: static (_, edit) => {
                Op op = Op.Of(name: nameof(Revolve));
                return edit.Profile.Switch(
                    state: (Edit: edit, Op: op),
                    ofCurve: static (ctx, profile) => ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: profile.Value, key: ctx.Op,
                        body: revolute => Single(ctx.Op, SurfaceSlot.Revolved, () => ctx.Edit.Sweep.Case switch {
                            (double start, double end) => RevSurface.Create(
                                revoluteCurve: revolute, axisOfRevolution: ctx.Edit.Axis, startAngleRadians: start, endAngleRadians: end),
                            _ => RevSurface.Create(revoluteCurve: revolute, axisOfRevolution: ctx.Edit.Axis),
                        })),
                    ofLine: static (ctx, profile) => Single(ctx.Op, SurfaceSlot.Revolved, () => ctx.Edit.Sweep.Case switch {
                        (double start, double end) => RevSurface.Create(
                            revoluteLine: profile.Value, axisOfRevolution: ctx.Edit.Axis, startAngleRadians: start, endAngleRadians: end),
                        _ => RevSurface.Create(revoluteLine: profile.Value, axisOfRevolution: ctx.Edit.Axis),
                    }));
            },
            fit: static (model, edit) => {
                Op op = Op.Of(name: nameof(Fit));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    edit.Law.Switch(
                        state: (Surface: surface, Domain: model, Op: op),
                        toTolerance: static (ctx, law) => Single(ctx.Op, SurfaceSlot.Refitted, () => ctx.Surface.Fit(
                            uDegree: law.UDegree, vDegree: law.VDegree, fitTolerance: ctx.Domain.Absolute.Value)),
                        toGrid: static (ctx, law) => Single(ctx.Op, SurfaceSlot.Refitted, () => ctx.Surface.Rebuild(
                            uDegree: law.UDegree, vDegree: law.VDegree, uPointCount: law.UPoints, vPointCount: law.VPoints)),
                        inDirection: static (ctx, law) =>
                            from _ in guard(law.Direction is 0 or 1, ctx.Op.InvalidInput())
                            from built in Single(ctx.Op, SurfaceSlot.Refitted, () => ctx.Surface.RebuildOneDirection(
                                direction: law.Direction, pointCount: law.PointCount, loftType: law.Kind,
                                refitTolerance: ctx.Domain.Absolute.Value))
                            select built));
            },
            variableOffset: static (model, edit) => {
                Op op = Op.Of(name: nameof(VariableOffset));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    Single(op, SurfaceSlot.Offsetted, () => edit.Interior.IsEmpty
                        ? surface.VariableOffset(
                            uMinvMin: edit.UMinVMin, uMinvMax: edit.UMinVMax, uMaxvMin: edit.UMaxVMin, uMaxvMax: edit.UMaxVMax,
                            tolerance: model.Absolute.Value)
                        : surface.VariableOffset(
                            uMinvMin: edit.UMinVMin, uMinvMax: edit.UMinVMax, uMaxvMin: edit.UMaxVMin, uMaxvMax: edit.UMaxVMax,
                            interiorParameters: edit.Interior.Map(static row => row.Uv).AsEnumerable(),
                            interiorDistances: edit.Interior.Map(static row => row.Distance).AsEnumerable(),
                            tolerance: model.Absolute.Value)));
            });

    private static Fin<Built<SurfaceSlot>> Single(Op op, SurfaceSlot slot, Func<GeometryBase?> run) =>
        op.Catch(() => ModelGate.Own(built: run(), key: op).Map(owned => new Built<SurfaceSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<SurfaceSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: 1)))));

    private static Fin<Built<SurfaceSlot>> Many(Op op, SurfaceSlot slot, Func<System.Collections.Generic.IEnumerable<Surface>> run) =>
        op.Catch(() => ModelGate.OwnMany(built: run(), key: op).Map(owned => new Built<SurfaceSlot>(
            Products: owned,
            Evidence: BuildReceipt<SurfaceSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count)))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Surfaces {
    public static Fin<Built<SurfaceSlot>> Build(Context context, params ReadOnlySpan<SurfaceOp> operations) {
        Op op = Op.Of();
        return from domain in Optional(context).ToFin(Fail: op.MissingContext())
               from _ in guard(operations.Length > 0, op.InvalidInput())
               from built in ModelGate.Folded(
                   context: domain,
                   operations: toSeq(operations.ToArray()),
                   apply: static (operation, model) => operation.Apply(domain: model))
               select built;
    }
}
```

## [04]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]          | [FORM]                                        | [ENTRY]                       |
| :-----: | :------------------- | :--------------- | :-------------------------------------------- | :---------------------------- |
|  [01]   | network continuity   | `NetContinuity`  | rows whose key is the native code             | `SurfaceOp.Network`           |
|  [02]   | network diagnostics  | `SurfaceOp`      | `out int error` as a `Code` fact              | `SurfaceSlot.Networked` facts |
|  [03]   | grid fitting         | `GridFit`        | control versus through with closure grants    | `SurfaceOp.Grid`              |
|  [04]   | geodesic modalities  | `SurfaceOp`      | fitted curve product or uv-sample evidence    | `GeodesicCurve` / `Samples`   |
|  [05]   | analytic seeding     | `AnalyticSeed`   | one primitive vocabulary, two representations | `SurfaceOp.Seed`              |
|  [06]   | rolling-ball seeding | `RollingSeed`    | auto, flipped, or uv-seeded as one union      | `SurfaceOp.RollingBall`       |
|  [07]   | revolve profile      | `RevolveProfile` | leased curve or bare line                     | `SurfaceOp.Revolve`           |
|  [08]   | fit and rebuild      | `SurfaceFitLaw`  | full value-semantic instance rebuild family   | `SurfaceOp.Fit`               |
|  [09]   | surface verbs        | `SurfaceOp`      | one flat `[Union]`, total generated dispatch  | `Surfaces.Build`              |
