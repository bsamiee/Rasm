# [RASM_RHINO_MODELING_SURFACES]

`Rasm.Rhino.Modeling` owns freeform surface construction. One `SurfaceOp` union carries network fitting with error evidence, rail revolve, point-grid interpolation, ruled and corner surfaces, curve-on-surface fitting, subd-friendly rebuilds, analytic seeding, compatibility reparameterization, iso-edge matching, extrusion, periodic closure, soft editing, rolling-ball fillets, tween sampling, sum surfaces, bounded planes, and value-semantic fit/rebuild through `Surfaces.Build`. Input shape selects each overload family, and `SurfaceFitLaw` collapses the full instance fit/rebuild family. Kernel NURBS evaluation, division, tessellation, and analysis remain kernel-owned; `Context` supplies every tolerance.

## [01]-[INDEX]

- [02]-[FIT_POLICY]: `NetContinuity`, `ParametricAxis`, `NetworkLaw`, `GridFit`, `CornerSeed`, `SurfaceFitLaw`, `ExtrudeTerminal`, `RollingSeed`, `AnalyticSeed`, `SurfaceForm`, `PlaneFrame`, `RevolveProfile`, `SumExtent` — the construction discriminants.
- [03]-[OPERATION_RAIL]: `SurfaceSlot`, `SurfaceOp`, and the `Surfaces.Build` entry.
- [04]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[FIT_POLICY]

- Owner: `NetContinuity` and `ParametricAxis` `[SmartEnum<int>]` — native continuity and parametric-direction vocabularies; `NetworkLaw`, `GridFit`, and `CornerSeed` close source arity; `SurfaceFitLaw` closes fit/rebuild modality; `ExtrudeTerminal`, `RollingSeed`, `AnalyticSeed`, `SurfaceForm`, `PlaneFrame`, `RevolveProfile`, and `SumExtent` carry only modality-valid construction payloads.
- Law: the continuity code never travels bare — `NetContinuity` keys the native integer so a network arm reads `(int)row`, and an out-of-vocabulary code is unconstructible.
- Law: one analytic vocabulary serves two representations — `AnalyticSeed.Build` dispatches the primitive once, while each `SurfaceForm` row supplies the four constructor delegates through `[UseDelegateFromConstructor]`; neither axis reconstructs the other.

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
public sealed partial class ParametricAxis {
    public static readonly ParametricAxis U = new(key: 0);
    public static readonly ParametricAxis V = new(key: 1);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NetworkLaw {
    private NetworkLaw() { }
    public sealed record Auto(Seq<GeometryHandle> Curves, NetContinuity Continuity) : NetworkLaw;
    public sealed record Uv(
        Seq<GeometryHandle> UCurves, NetContinuity UStart, NetContinuity UEnd,
        Seq<GeometryHandle> VCurves, NetContinuity VStart, NetContinuity VEnd) : NetworkLaw;

    internal bool Admissible => Switch(
        auto: static law => Handles(law.Curves) && Declared(law.Continuity),
        uv: static law => Handles(law.UCurves) && Handles(law.VCurves)
            && Declared(law.UStart) && Declared(law.UEnd)
            && Declared(law.VStart) && Declared(law.VEnd));

    internal Fin<(NurbsSurface Product, int Error)> Build(Context domain, Op key) =>
        Switch(
            state: (Domain: domain, Op: key),
            auto: static (ctx, law) => ModelGate.BorrowMany<Curve, (NurbsSurface Product, int Error)>(
                handles: law.Curves, key: ctx.Op, body: curves => Captured(ctx.Op, () => {
                    NurbsSurface? product = NurbsSurface.CreateNetworkSurface(
                        curves: curves.AsIterable(), continuity: (int)law.Continuity,
                        edgeTolerance: ctx.Domain.Absolute.Value, interiorTolerance: ctx.Domain.Absolute.Value,
                        angleTolerance: ctx.Domain.Angle.Value, error: out int error);
                    return (Product: product, Error: error);
                })),
            uv: static (ctx, law) => ModelGate.BorrowMany<Curve, (NurbsSurface Product, int Error)>(
                handles: law.UCurves, key: ctx.Op, body: uCurves =>
                ModelGate.BorrowMany<Curve, (NurbsSurface Product, int Error)>(handles: law.VCurves, key: ctx.Op, body: vCurves =>
                    Captured(ctx.Op, () => {
                        NurbsSurface? product = NurbsSurface.CreateNetworkSurface(
                            uCurves: uCurves.AsIterable(), uContinuityStart: (int)law.UStart, uContinuityEnd: (int)law.UEnd,
                            vCurves: vCurves.AsIterable(), vContinuityStart: (int)law.VStart, vContinuityEnd: (int)law.VEnd,
                            edgeTolerance: ctx.Domain.Absolute.Value, interiorTolerance: ctx.Domain.Absolute.Value,
                            angleTolerance: ctx.Domain.Angle.Value, error: out int error);
                        return (Product: product, Error: error);
                    }))));

    private static bool Handles(Seq<GeometryHandle> handles) =>
        !handles.IsEmpty && handles.ForAll(static handle => handle is not null);

    private static bool Declared(NetContinuity? value) =>
        value is not null && NetContinuity.Items.Contains(value);

    private static Fin<(NurbsSurface Product, int Error)> Captured(
        Op key,
        Func<(NurbsSurface? Product, int Error)> build) =>
        key.Catch(() => {
            (NurbsSurface? product, int error) = build();
            if (product is not null && error == 0) {
                return Fin.Succ(value: (Product: product, Error: error));
            }

            product?.Dispose();
            return Fin.Fail<(NurbsSurface Product, int Error)>(
                error: key.InvalidResult(detail: error.ToString(CultureInfo.InvariantCulture)));
        });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GridFit {
    private GridFit() { }
    public sealed record Control : GridFit;
    public sealed record Through(FrozenSet<ParametricAxis> ClosedAxes) : GridFit;

    internal bool Admissible => Switch(
        control: static () => true,
        through: static fit => Surfaces.Declared(values: fit.ClosedAxes, rows: ParametricAxis.Items));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CornerSeed {
    private CornerSeed() { }
    public sealed record Triangle(Point3d A, Point3d B, Point3d C) : CornerSeed;
    public sealed record Quad(Point3d A, Point3d B, Point3d C, Point3d D) : CornerSeed;

    internal bool Admissible => Switch(
        triangle: static seed => seed.A.IsValid && seed.B.IsValid && seed.C.IsValid,
        quad: static seed => seed.A.IsValid && seed.B.IsValid && seed.C.IsValid && seed.D.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceFitLaw {
    private SurfaceFitLaw() { }
    public sealed record ToTolerance(int UDegree, int VDegree) : SurfaceFitLaw;
    public sealed record ToGrid(int UDegree, int VDegree, int UPoints, int VPoints) : SurfaceFitLaw;
    public sealed record InDirection(ParametricAxis Axis, int PointCount, LoftType Kind) : SurfaceFitLaw;

    internal bool Admissible => Switch(
        toTolerance: static law => Surfaces.Degrees(law.UDegree, law.VDegree),
        toGrid: static law => Surfaces.GridShape(law.UPoints, law.VPoints, law.UDegree, law.VDegree),
        inDirection: static law => law.Axis is not null && ParametricAxis.Items.Contains(law.Axis)
            && law.PointCount > 0 && Enum.IsDefined(law.Kind));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrudeTerminal {
    private ExtrudeTerminal() { }
    public sealed record Along(Vector3d Direction) : ExtrudeTerminal;
    public sealed record ToApex(Point3d Apex) : ExtrudeTerminal;

    internal bool Admissible => Switch(
        along: static terminal => terminal.Direction.IsValid && !terminal.Direction.IsZero,
        toApex: static terminal => terminal.Apex.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RollingSeed {
    private RollingSeed() { }
    public sealed record Auto : RollingSeed;
    public sealed record Flipped(bool FlipFirst, bool FlipSecond) : RollingSeed;
    public sealed record AtUv(Point2d First, Point2d Second) : RollingSeed;

    internal bool Admissible => Switch(
        auto: static () => true,
        flipped: static _ => true,
        atUv: static seed => seed.First.IsValid && seed.Second.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticSeed {
    private AnalyticSeed() { }
    public sealed record OfCone(Cone Value) : AnalyticSeed;
    public sealed record OfCylinder(Cylinder Value) : AnalyticSeed;
    public sealed record OfSphere(Sphere Value) : AnalyticSeed;
    public sealed record OfTorus(Torus Value) : AnalyticSeed;

    internal bool Admissible => Switch(
        ofCone: static seed => seed.Value.IsValid,
        ofCylinder: static seed => seed.Value.IsValid,
        ofSphere: static seed => seed.Value.IsValid,
        ofTorus: static seed => seed.Value.IsValid);

    internal GeometryBase? Build(SurfaceForm form) => Switch(
        state: form,
        ofCone: static (surfaceForm, seed) => surfaceForm.BuildCone(seed.Value),
        ofCylinder: static (surfaceForm, seed) => surfaceForm.BuildCylinder(seed.Value),
        ofSphere: static (surfaceForm, seed) => surfaceForm.BuildSphere(seed.Value),
        ofTorus: static (surfaceForm, seed) => surfaceForm.BuildTorus(seed.Value));
}

[SmartEnum<int>]
public sealed partial class SurfaceForm {
    public static readonly SurfaceForm Nurbs = new(
        key: 0,
        buildCone: static value => NurbsSurface.CreateFromCone(cone: value),
        buildCylinder: static value => NurbsSurface.CreateFromCylinder(cylinder: value),
        buildSphere: static value => NurbsSurface.CreateFromSphere(sphere: value),
        buildTorus: static value => NurbsSurface.CreateFromTorus(torus: value));
    public static readonly SurfaceForm Revolved = new(
        key: 1,
        buildCone: static value => RevSurface.CreateFromCone(cone: value),
        buildCylinder: static value => RevSurface.CreateFromCylinder(cylinder: value),
        buildSphere: static value => RevSurface.CreateFromSphere(sphere: value),
        buildTorus: static value => RevSurface.CreateFromTorus(torus: value));

    [UseDelegateFromConstructor]
    internal partial GeometryBase? BuildCone(Cone value);
    [UseDelegateFromConstructor]
    internal partial GeometryBase? BuildCylinder(Cylinder value);
    [UseDelegateFromConstructor]
    internal partial GeometryBase? BuildSphere(Sphere value);
    [UseDelegateFromConstructor]
    internal partial GeometryBase? BuildTorus(Torus value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlaneFrame {
    private PlaneFrame() { }
    public sealed record OfPlane(Plane Value) : PlaneFrame;
    public sealed record OfLine(Line LineInPlane, Vector3d VectorInPlane) : PlaneFrame;

    internal bool Admissible => Switch(
        ofPlane: static frame => frame.Value.IsValid,
        ofLine: static frame => frame.LineInPlane.IsValid
            && frame.VectorInPlane.IsValid && !frame.VectorInPlane.IsZero);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RevolveProfile {
    private RevolveProfile() { }
    public sealed record OfCurve(GeometryHandle Value) : RevolveProfile;
    public sealed record OfLine(Line Value) : RevolveProfile;

    internal bool Admissible => Switch(
        ofCurve: static profile => profile.Value is not null,
        ofLine: static profile => profile.Value.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SumExtent {
    private SumExtent() { }
    public sealed record ByDirection(Vector3d Direction) : SumExtent;
    public sealed record ByCurve(GeometryHandle Second) : SumExtent;

    internal bool Admissible => Switch(
        byDirection: static extent => extent.Direction.IsValid && !extent.Direction.IsZero,
        byCurve: static extent => extent.Second is not null);
}
```

## [03]-[OPERATION_RAIL]

- Owner: `SurfaceSlot` `[SmartEnum<int>]` — the consequence vocabulary; `SurfaceOp` `[Union]` — the whole verified freeform-construction verb roster; `Surfaces` — the one entry folding any operation spread into one `Built<SurfaceSlot>`.
- Law: the network error code is evidence — `NetworkLaw.Build` captures both native topologies and refuses a null product or nonzero code before ownership; successful code becomes a `Code` fact beside the surface.
- Law: `SurfaceOp.Admitted` closes every policy vocabulary and payload before any host lease; point grids use positive degree-preserving dimensions and widened cardinality arithmetic.
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
    public static readonly SurfaceSlot Offset = new(key: 18);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceOp {
    private SurfaceOp() { }
    public sealed record Network(NetworkLaw Law) : SurfaceOp;
    public sealed record RailRevolve(GeometryHandle Profile, GeometryHandle Rail, Line Axis, bool ScaleHeight = false) : SurfaceOp;
    public sealed record Grid(Seq<Point3d> Points, int UCount, int VCount, int UDegree, int VDegree, GridFit Fit) : SurfaceOp;
    public sealed record Corners(CornerSeed Seed) : SurfaceOp;
    public sealed record Ruled(GeometryHandle First, GeometryHandle Second) : SurfaceOp;
    public sealed record GeodesicCurve(GeometryHandle Surface, Seq<Point2d> Points, bool Periodic = false) : SurfaceOp;
    public sealed record GeodesicSamples(GeometryHandle Surface, Seq<Point2d> FixedPoints, bool Periodic, int InitCount, int Levels) : SurfaceOp;
    public sealed record SubDFriendly(GeometryHandle Surface) : SurfaceOp;
    public sealed record Seed(AnalyticSeed Value, SurfaceForm Form) : SurfaceOp;
    public sealed record PlaneGrid(Plane Frame, Interval U, Interval V, int UDegree, int VDegree, int UPoints, int VPoints) : SurfaceOp;
    public sealed record Compatible(GeometryHandle First, GeometryHandle Second) : SurfaceOp;
    public sealed record MatchEdge(GeometryHandle Surface, IsoStatus Side, GeometryHandle TargetCurve, double MaxEndDistance, double MaxInteriorDistance, int MaxLevel) : SurfaceOp;
    public sealed record Extruded(GeometryHandle Profile, ExtrudeTerminal Terminal) : SurfaceOp;
    public sealed record Periodic(GeometryHandle Surface, ParametricAxis Axis, bool Smooth = true) : SurfaceOp;
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

    internal Fin<SurfaceOp> Admitted(Op key) =>
        guard(this switch {
            Network edit => edit.Law is not null && edit.Law.Admissible,
            RailRevolve edit => edit.Profile is not null && edit.Rail is not null && edit.Axis.IsValid,
            Grid edit => edit.Fit is not null && edit.Fit.Admissible
                && Surfaces.GridShape(edit.UCount, edit.VCount, edit.UDegree, edit.VDegree)
                && edit.Points.Count == (long)edit.UCount * edit.VCount
                && edit.Points.ForAll(static point => point.IsValid),
            Corners edit => edit.Seed is not null && edit.Seed.Admissible,
            Ruled edit => edit.First is not null && edit.Second is not null,
            GeodesicCurve edit => edit.Surface is not null && Points(edit.Points),
            GeodesicSamples edit => edit.Surface is not null && Points(edit.FixedPoints)
                && edit.InitCount > 0 && edit.Levels >= 0,
            SubDFriendly edit => edit.Surface is not null,
            Seed edit => edit.Value is not null && edit.Value.Admissible
                && edit.Form is not null && SurfaceForm.Items.Contains(edit.Form),
            PlaneGrid edit => edit.Frame.IsValid && edit.U.IsValid && edit.V.IsValid
                && Surfaces.GridShape(edit.UPoints, edit.VPoints, edit.UDegree, edit.VDegree),
            Compatible edit => edit.First is not null && edit.Second is not null,
            MatchEdge edit => edit.Surface is not null && edit.TargetCurve is not null
                && Enum.IsDefined(edit.Side)
                && NonNegative(edit.MaxEndDistance) && NonNegative(edit.MaxInteriorDistance)
                && edit.MaxLevel >= 0,
            Extruded edit => edit.Profile is not null && edit.Terminal is not null && edit.Terminal.Admissible,
            Periodic edit => edit.Surface is not null && edit.Axis is not null && ParametricAxis.Items.Contains(edit.Axis),
            SoftEdit edit => edit.Surface is not null && edit.Uv.IsValid && edit.Delta.IsValid
                && Positive(edit.ULength) && Positive(edit.VLength),
            RollingBall edit => edit.First is not null && edit.Second is not null
                && Positive(edit.Radius) && edit.At is not null && edit.At.Admissible,
            Tween edit => edit.First is not null && edit.Second is not null && edit.Count > 0 && edit.Samples > 0,
            Sum edit => edit.Profile is not null && edit.Extent is not null && edit.Extent.Admissible,
            BoundedPlane edit => edit.Frame is not null && edit.Frame.Admissible && edit.Box.IsValid,
            Revolve edit => edit.Profile is not null && edit.Profile.Admissible && edit.Axis.IsValid
                && edit.Sweep.ForAll(static sweep => double.IsFinite(sweep.StartRadians) && double.IsFinite(sweep.EndRadians)),
            Fit edit => edit.Surface is not null && edit.Law is not null && edit.Law.Admissible,
            VariableOffset edit => edit.Surface is not null
                && Finite(edit.UMinVMin, edit.UMinVMax, edit.UMaxVMin, edit.UMaxVMax)
                && edit.Interior.ForAll(static row => row.Uv.IsValid && double.IsFinite(row.Distance)),
            _ => false,
        }, key.InvalidInput()).ToFin().Map(_ => this);

    private static bool Points(Seq<Point2d> points) =>
        !points.IsEmpty && points.ForAll(static point => point.IsValid);

    private static bool Positive(double value) => double.IsFinite(value) && value > 0.0;

    private static bool NonNegative(double value) => double.IsFinite(value) && value >= 0.0;

    private static bool Finite(params ReadOnlySpan<double> values) => values.ToArray().All(double.IsFinite);

    internal Fin<Built<SurfaceSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            network: static (model, edit) => {
                Op op = Op.Of(name: nameof(Network));
                return edit.Law.Build(domain: model, key: op)
                    .Bind(result => ModelGate.Own(built: result.Product, key: op)
                        .Map(owned => Built<SurfaceSlot>.Of(operation: op,
                            Products: Seq(owned),
                            Evidence: BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Networked, body: new BuildBody.Tally(Count: 1))
                                + BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Networked, body: new BuildBody.Code(Value: result.Error)))));
            },
            railRevolve: static (_, edit) => {
                Op op = Op.Of(name: nameof(RailRevolve));
                return ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Profile, key: op, body: profile =>
                    ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Rail, key: op, body: rail =>
                        ModelGate.Single(op, SurfaceSlot.Revolved, () => NurbsSurface.CreateRailRevolvedSurface(
                            profile: profile, rail: rail, axis: edit.Axis, scaleHeight: edit.ScaleHeight))));
            },
            grid: static (_, edit) => {
                Op op = Op.Of(name: nameof(Grid));
                return edit.Fit.Switch(
                    state: (Edit: edit, Op: op),
                    control: static ctx => ModelGate.Single(ctx.Op, SurfaceSlot.Gridded, () => NurbsSurface.CreateFromPoints(
                        points: ctx.Edit.Points.AsIterable(), uCount: ctx.Edit.UCount, vCount: ctx.Edit.VCount,
                        uDegree: ctx.Edit.UDegree, vDegree: ctx.Edit.VDegree)),
                    through: static (ctx, fit) => ModelGate.Single(ctx.Op, SurfaceSlot.Gridded, () => NurbsSurface.CreateThroughPoints(
                        points: ctx.Edit.Points.AsIterable(), uCount: ctx.Edit.UCount, vCount: ctx.Edit.VCount,
                        uDegree: ctx.Edit.UDegree, vDegree: ctx.Edit.VDegree,
                        uClosed: fit.ClosedAxes.Contains(ParametricAxis.U),
                        vClosed: fit.ClosedAxes.Contains(ParametricAxis.V))));
            },
            corners: static (model, edit) => {
                Op op = Op.Of(name: nameof(Corners));
                return edit.Seed.Switch(
                    state: (Model: model, Op: op),
                    triangle: static (ctx, seed) => ModelGate.Single(ctx.Op, SurfaceSlot.Cornered, () => NurbsSurface.CreateFromCorners(
                        corner1: seed.A, corner2: seed.B, corner3: seed.C)),
                    quad: static (ctx, seed) => ModelGate.Single(ctx.Op, SurfaceSlot.Cornered, () => NurbsSurface.CreateFromCorners(
                        corner1: seed.A, corner2: seed.B, corner3: seed.C, corner4: seed.D,
                        tolerance: ctx.Model.Absolute.Value)));
            },
            ruled: static (_, edit) => {
                Op op = Op.Of(name: nameof(Ruled));
                return ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Single(op, SurfaceSlot.Ruled, () => NurbsSurface.CreateRuledSurface(curveA: first, curveB: second))));
            },
            geodesicCurve: static (model, edit) => {
                Op op = Op.Of(name: nameof(GeodesicCurve));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    ModelGate.Single(op, SurfaceSlot.Geodesic, () => NurbsSurface.CreateCurveOnSurface(
                        surface: surface, points: edit.Points.AsIterable(), tolerance: model.Absolute.Value, periodic: edit.Periodic)));
            },
            geodesicSamples: static (model, edit) => {
                Op op = Op.Of(name: nameof(GeodesicSamples));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    op.Catch(() => Optional(NurbsSurface.CreateCurveOnSurfacePoints(
                            surface: surface, fixedPoints: edit.FixedPoints.AsIterable(), tolerance: model.Absolute.Value,
                            periodic: edit.Periodic, initCount: edit.InitCount, levels: edit.Levels))
                        .ToFin(Fail: op.InvalidResult())
                        .Map(samples => Built<SurfaceSlot>.Of(operation: op,
                            Products: Seq<GeometryHandle>(),
                            Evidence: BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Geodesic, body: new BuildBody.UvRows(Rows: toSeq(samples)))))));
            },
            subDFriendly: static (_, edit) => {
                Op op = Op.Of(name: nameof(SubDFriendly));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    ModelGate.Single(op, SurfaceSlot.SubDReady, () => NurbsSurface.CreateSubDFriendly(surface: surface)));
            },
            seed: static (_, edit) => {
                Op op = Op.Of(name: nameof(Seed));
                return op.Catch(() => ModelGate.Own(built: edit.Value.Build(form: edit.Form), key: op)).Map(product => Built<SurfaceSlot>.Of(operation: op,
                    Products: Seq(product),
                    Evidence: BuildReceipt<SurfaceSlot>.Of(slot: SurfaceSlot.Seeded, body: new BuildBody.Tally(Count: 1))));
            },
            planeGrid: static (_, edit) => {
                Op op = Op.Of(name: nameof(PlaneGrid));
                return ModelGate.Single(op, SurfaceSlot.Seeded, () => NurbsSurface.CreateFromPlane(
                    plane: edit.Frame, uInterval: edit.U, vInterval: edit.V,
                    uDegree: edit.UDegree, vDegree: edit.VDegree, uPointCount: edit.UPoints, vPointCount: edit.VPoints));
            },
            compatible: static (_, edit) => {
                Op op = Op.Of(name: nameof(Compatible));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Second, key: op, body: second =>
                        op.Catch(() =>
                            ModelGate.Staged(op: op, success: NurbsSurface.MakeCompatible(
                                surface0: first, surface1: second, nurb0: out NurbsSurface nurb0, nurb1: out NurbsSurface nurb1),
                                (SurfaceSlot.Compatible, (GeometryBase[])[nurb0, nurb1], false))));
            },
            matchEdge: static (model, edit) => {
                Op op = Op.Of(name: nameof(MatchEdge));
                return ModelGate.Borrow<NurbsSurface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.TargetCurve, key: op, body: target =>
                        ModelGate.Single(op, SurfaceSlot.EdgeMatched, () => surface.MatchToCurve(
                            side: edit.Side, targetCurve: target, maxEndDistance: edit.MaxEndDistance,
                            maxInteriorDistance: edit.MaxInteriorDistance, matchTolerance: model.Absolute.Value, maxLevel: edit.MaxLevel))));
            },
            extruded: static (_, edit) => {
                Op op = Op.Of(name: nameof(Extruded));
                return ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Profile, key: op, body: profile =>
                    edit.Terminal.Switch(
                        state: (Profile: profile, Op: op),
                        along: static (ctx, terminal) => ModelGate.Single(ctx.Op, SurfaceSlot.Extruded, () => Surface.CreateExtrusion(
                            profile: ctx.Profile, direction: terminal.Direction)),
                        toApex: static (ctx, terminal) => ModelGate.Single(ctx.Op, SurfaceSlot.Extruded, () => Surface.CreateExtrusionToPoint(
                            profile: ctx.Profile, apexPoint: terminal.Apex))));
            },
            periodic: static (_, edit) => {
                Op op = Op.Of(name: nameof(Periodic));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    ModelGate.Single(op, SurfaceSlot.Closed, () => Surface.CreatePeriodicSurface(
                        surface: surface, direction: (int)edit.Axis, bSmooth: edit.Smooth)));
            },
            softEdit: static (model, edit) => {
                Op op = Op.Of(name: nameof(SoftEdit));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    ModelGate.Single(op, SurfaceSlot.SoftEdited, () => Surface.CreateSoftEditSurface(
                        surface: surface, uv: edit.Uv, delta: edit.Delta, uLength: edit.ULength, vLength: edit.VLength,
                        tolerance: model.Absolute.Value, fixEnds: edit.FixEnds)));
            },
            rollingBall: static (model, edit) => {
                Op op = Op.Of(name: nameof(RollingBall));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Second, key: op, body: second =>
                        edit.At.Switch(
                            state: (First: first, Second: second, Radius: edit.Radius, Tolerance: model.Absolute.Value, Op: op),
                            auto: static ctx => ModelGate.Many(ctx.Op, SurfaceSlot.Filleted, () => Surface.CreateRollingBallFillet(
                                surfaceA: ctx.First, surfaceB: ctx.Second, radius: ctx.Radius, tolerance: ctx.Tolerance)),
                            flipped: static (ctx, seed) => ModelGate.Many(ctx.Op, SurfaceSlot.Filleted, () => Surface.CreateRollingBallFillet(
                                surfaceA: ctx.First, flipA: seed.FlipFirst, surfaceB: ctx.Second, flipB: seed.FlipSecond,
                                radius: ctx.Radius, tolerance: ctx.Tolerance)),
                            atUv: static (ctx, seed) => ModelGate.Many(ctx.Op, SurfaceSlot.Filleted, () => Surface.CreateRollingBallFillet(
                                surfaceA: ctx.First, uvA: seed.First, surfaceB: ctx.Second, uvB: seed.Second,
                                radius: ctx.Radius, tolerance: ctx.Tolerance)))));
            },
            tween: static (model, edit) => {
                Op op = Op.Of(name: nameof(Tween));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Many(op, SurfaceSlot.Tweened, () => Surface.CreateTweenSurfacesWithSampling(
                            surface0: first, surface1: second, numSurfaces: edit.Count, numSamples: edit.Samples, tolerance: model.Absolute.Value))));
            },
            sum: static (_, edit) => {
                Op op = Op.Of(name: nameof(Sum));
                return ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: edit.Profile, key: op, body: profile =>
                    edit.Extent.Switch(
                        state: (Profile: profile, Op: op),
                        byDirection: static (ctx, extent) => ModelGate.Single(ctx.Op, SurfaceSlot.Summed, () => SumSurface.Create(
                            curve: ctx.Profile, extrusionDirection: extent.Direction)),
                        byCurve: static (ctx, extent) => ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: extent.Second, key: ctx.Op,
                            body: second => ModelGate.Single(ctx.Op, SurfaceSlot.Summed, () => SumSurface.Create(curveA: ctx.Profile, curveB: second)))));
            },
            boundedPlane: static (_, edit) => {
                Op op = Op.Of(name: nameof(BoundedPlane));
                return edit.Frame.Switch(
                    state: (Box: edit.Box, Op: op),
                    ofPlane: static (ctx, frame) => ModelGate.Single(ctx.Op, SurfaceSlot.Bounded, () => PlaneSurface.CreateThroughBox(
                        plane: frame.Value, box: ctx.Box)),
                    ofLine: static (ctx, frame) => ModelGate.Single(ctx.Op, SurfaceSlot.Bounded, () => PlaneSurface.CreateThroughBox(
                        lineInPlane: frame.LineInPlane, vectorInPlane: frame.VectorInPlane, box: ctx.Box)));
            },
            revolve: static (_, edit) => {
                Op op = Op.Of(name: nameof(Revolve));
                return edit.Profile.Switch(
                    state: (Edit: edit, Op: op),
                    ofCurve: static (ctx, profile) => ModelGate.Borrow<Curve, Built<SurfaceSlot>>(handle: profile.Value, key: ctx.Op,
                        body: revolute => ModelGate.Single(ctx.Op, SurfaceSlot.Revolved, () => ctx.Edit.Sweep.Case switch {
                            (double start, double end) => RevSurface.Create(
                                revoluteCurve: revolute, axisOfRevolution: ctx.Edit.Axis, startAngleRadians: start, endAngleRadians: end),
                            _ => RevSurface.Create(revoluteCurve: revolute, axisOfRevolution: ctx.Edit.Axis),
                        })),
                    ofLine: static (ctx, profile) => ModelGate.Single(ctx.Op, SurfaceSlot.Revolved, () => ctx.Edit.Sweep.Case switch {
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
                        toTolerance: static (ctx, law) => ModelGate.Single(ctx.Op, SurfaceSlot.Refitted, () => ctx.Surface.Fit(
                            uDegree: law.UDegree, vDegree: law.VDegree, fitTolerance: ctx.Domain.Absolute.Value)),
                        toGrid: static (ctx, law) => ModelGate.Single(ctx.Op, SurfaceSlot.Refitted, () => ctx.Surface.Rebuild(
                            uDegree: law.UDegree, vDegree: law.VDegree, uPointCount: law.UPoints, vPointCount: law.VPoints)),
                        inDirection: static (ctx, law) => ModelGate.Single(ctx.Op, SurfaceSlot.Refitted, () => ctx.Surface.RebuildOneDirection(
                            direction: (int)law.Axis, pointCount: law.PointCount, loftType: law.Kind,
                            refitTolerance: ctx.Domain.Absolute.Value))));
            },
            variableOffset: static (model, edit) => {
                Op op = Op.Of(name: nameof(VariableOffset));
                return ModelGate.Borrow<Surface, Built<SurfaceSlot>>(handle: edit.Surface, key: op, body: surface =>
                    ModelGate.Single(op, SurfaceSlot.Offset, () => edit.Interior.IsEmpty
                        ? surface.VariableOffset(
                            uMinvMin: edit.UMinVMin, uMinvMax: edit.UMinVMax, uMaxvMin: edit.UMaxVMin, uMaxvMax: edit.UMaxVMax,
                            tolerance: model.Absolute.Value)
                        : surface.VariableOffset(
                            uMinvMin: edit.UMinVMin, uMinvMax: edit.UMinVMax, uMaxvMin: edit.UMaxVMin, uMaxvMax: edit.UMaxVMax,
                            interiorParameters: edit.Interior.Map(static row => row.Uv).AsEnumerable(),
                            interiorDistances: edit.Interior.Map(static row => row.Distance).AsEnumerable(),
                            tolerance: model.Absolute.Value)));
            });

}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Surfaces {
    internal const int MaximumNurbsDegree = 11;

    internal static bool Declared<T>(FrozenSet<T>? values, IReadOnlyList<T> rows) where T : class =>
        values is not null && values.All(row => row is not null && rows.Contains(row));

    internal static bool Degrees(int uDegree, int vDegree) =>
        uDegree is > 0 and <= MaximumNurbsDegree && vDegree is > 0 and <= MaximumNurbsDegree;

    internal static bool GridShape(int uCount, int vCount, int uDegree, int vDegree) =>
        Degrees(uDegree: uDegree, vDegree: vDegree) && uCount > uDegree && vCount > vDegree;

    public static Fin<Built<SurfaceSlot>> Build(Context context, params ReadOnlySpan<SurfaceOp> operations) {
        Op op = Op.Of();
        Seq<SurfaceOp> captured = toSeq(operations.ToArray());
        return from domain in Optional(context).ToFin(Fail: op.MissingContext())
               from _ in guard(!captured.IsEmpty, op.InvalidInput())
               from admitted in captured.TraverseM(operation =>
                       Optional(operation).ToFin(Fail: op.InvalidInput())
                           .Bind(active => active.Admitted(key: op)))
                   .As()
               from built in ModelGate.Folded(
                   context: domain,
                   operations: admitted,
                   apply: static (operation, model) => operation.Apply(domain: model))
               select built;
    }
}
```

## [04]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]          | [FORM]                                       | [ENTRY]                             |
| :-----: | :------------------- | :--------------- | :------------------------------------------- | :---------------------------------- |
|  [01]   | network continuity   | `NetworkLaw`     | auto-sorted or U/V topology with typed codes | `SurfaceOp.Network`                 |
|  [02]   | network diagnostics  | `SurfaceOp`      | `out int error` as a `Code` fact             | `SurfaceSlot.Networked` facts       |
|  [03]   | grid fitting         | `GridFit`        | control or through with closed-axis set      | `SurfaceOp.Grid`                    |
|  [04]   | geodesic modalities  | `SurfaceOp`      | fitted curve product or uv-sample evidence   | `GeodesicCurve` / `GeodesicSamples` |
|  [05]   | analytic seeding     | `SurfaceForm`    | constructor-bearing rows over `AnalyticSeed` | `SurfaceOp.Seed`                    |
|  [06]   | rolling-ball seeding | `RollingSeed`    | auto, flipped, or uv-seeded as one union     | `SurfaceOp.RollingBall`             |
|  [07]   | revolve profile      | `RevolveProfile` | leased curve or bare line                    | `SurfaceOp.Revolve`                 |
|  [08]   | fit and rebuild      | `SurfaceFitLaw`  | full value-semantic instance rebuild family  | `SurfaceOp.Fit`                     |
|  [09]   | corner arity         | `CornerSeed`     | triangle or tolerance-driven quad            | `SurfaceOp.Corners`                 |
|  [10]   | sum extent           | `SumExtent`      | direction or second-curve payload            | `SurfaceOp.Sum`                     |
|  [11]   | surface verbs        | `SurfaceOp`      | one flat `[Union]`, total generated dispatch | `Surfaces.Build`                    |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
