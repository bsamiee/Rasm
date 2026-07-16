# [RASM_RHINO_MODELING_CURVES]

`Rasm.Rhino.Modeling` owns curve host operations. One `CurveOp` union carries offset, refinement, extension, shortening, splitting, pulling, projection, joining, booleans, planar regions, blend/fillet/tween/match/mean construction, interpolation, analytic and freeform seeding, text outlines, and subd-friendly rebuilds through `Curves.Build`. Policy unions discriminate native overloads from payload shape; `Context` supplies every tolerance and angle. Kernel parametric evaluation, division, curvature, contours, and predicate-exact offsets remain kernel-owned.

## [01]-[INDEX]

- [02]-[OFFSET_POLICY]: `OffsetFrame`, `SurfaceOffsetLaw`, `RibbonLaw` — the offset discriminants and the ribbon carrier.
- [03]-[SHAPE_POLICY]: `ExtendLaw`, `ShortenLaw`, `SplitCutter`, `PullTarget`, `ProjectTarget`, `BlendLaw`, `TweenLaw`, `SpiralLaw`, `ParabolaSeed`, `AnalyticCurve`, `CatenaryLaw`, `FitLaw` — the modality vocabularies.
- [04]-[OPERATION_RAIL]: `CurveSlot`, `CurveOp`, and the `Curves.Build` entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[OFFSET_POLICY]

- Owner: `OffsetFrame` `[Union]` — a planar offset frames in its plane, a freeform offset frames by direction point and normal with corner and end style; `SurfaceOffsetLaw` `[Union]` — constant distance, through-point, or the parameter/distance varying rows for on-surface offsetting; `RibbonLaw` — the whole `RibbonOffsetParameters` carrier as one policy value.
- Law: varying distances are rows — the on-surface arm splits `(Parameter, Distance)` rows into the two parallel native arrays at the call, so cardinality is proven by construction.
- Law: `RibbonLaw.Rig` is the one site naming `RibbonOffsetParameters` — offset distance, location, plane vector, blend radius, rebuild and refit knobs, cross-section alignment, and the `RibbonOffsetSurfaceMethod` row bake in one member with the tolerance slot reading the regime; the ribbon's rails, cross-sections, and breps cross as products behind per-class tallies.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetFrame {
    private OffsetFrame() { }
    public sealed record InPlane(Plane Value, CurveOffsetCornerStyle Corner = CurveOffsetCornerStyle.Sharp) : OffsetFrame;
    public sealed record ByNormal(
        Point3d DirectionPoint, Vector3d Normal, CurveOffsetCornerStyle Corner = CurveOffsetCornerStyle.Sharp,
        CurveOffsetEndStyle End = CurveOffsetEndStyle.None, bool Loose = false) : OffsetFrame;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceOffsetLaw {
    private SurfaceOffsetLaw() { }
    public sealed record ByDistance(double Value) : SurfaceOffsetLaw;
    public sealed record ThroughPoint(Point2d Value) : SurfaceOffsetLaw;
    public sealed record Varying(Seq<(double Parameter, double Distance)> Rows) : SurfaceOffsetLaw;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record RibbonLaw(
    double Distance,
    Point3d Location,
    double BlendRadius = 0.0,
    Option<Vector3d> PlaneVector = default,
    int RebuildPointCount = 0,
    bool Refit = false,
    bool AlignCrossSections = false,
    RibbonOffsetSurfaceMethod SurfaceMethod = RibbonOffsetSurfaceMethod.Sweep2) {
    internal Fin<RibbonOffsetParameters> Rig(Context domain, Op key) =>
        key.Catch(() => Fin.Succ(value: new RibbonOffsetParameters {
            OffsetDistance = Distance,
            OffsetLocation = Location,
            OffsetTolerance = domain.Absolute.Value,
            OffsetPlaneVector3d = PlaneVector.IfNone(Vector3d.Unset),
            BlendRadius = BlendRadius,
            RebuildPointCount = RebuildPointCount,
            RefitTolerance = Refit ? domain.Absolute.Value : 0.0,
            AlignCrossSections = AlignCrossSections,
            RibbonSurfaceGenerationMethod = SurfaceMethod,
        }));
}
```

## [03]-[SHAPE_POLICY]

- Owner: the modality vocabularies — `ExtendLaw` fuses the extension side, style, and terminal (length, geometry, point, line, arc, on-surface); `ShortenLaw` fuses domain and end trimming; `SplitCutter` fuses parameter, brep, surface, and plane splitting; `PullTarget`/`ProjectTarget` fuse pull and projection destinations; `BlendLaw` fuses end-to-end and at-parameter blending; `TweenLaw` fuses plain, matched, and sampled tweening; `SpiralLaw`, `ParabolaSeed`, `AnalyticCurve`, and `CatenaryLaw` fuse the construction seed families; `FitLaw` carries the advanced `NurbsCurveFitParameters` surface as one policy value.
- Law: the discriminant is the value's shape — every native overload family resolves from the case the caller constructed, so no arm reads a mode flag and no verb family grows a `ByX` sibling.
- Law: `FitLaw.Rig` is the one site naming `NurbsCurveFitParameters` — tangent matching, kink splitting, the three intensity axes, degree, point count, subd-friendliness, closure, and optimization bake in one member; the fit's maximum-separation line and parameters land as receipt facts beside the fitted curve.
- Law: catenary construction is one case over four shape terminals — through-point, length, parameter, and apex select the native static, and the apex, parameter, length, and deviation out-channels land as facts on every form.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtendLaw {
    private ExtendLaw() { }
    public sealed record ByLength(CurveEnd Side, double Length, CurveExtensionStyle Style) : ExtendLaw;
    public sealed record ToGeometry(CurveEnd Side, CurveExtensionStyle Style, Seq<GeometryHandle> Bounds) : ExtendLaw;
    public sealed record ToPoint(CurveEnd Side, CurveExtensionStyle Style, Point3d Terminal) : ExtendLaw;
    public sealed record ByLine(CurveEnd Side, Seq<GeometryHandle> Bounds) : ExtendLaw;
    public sealed record ByArc(CurveEnd Side, Seq<GeometryHandle> Bounds) : ExtendLaw;
    public sealed record OnSurface(CurveEnd Side, GeometryHandle Target, Option<int> Face = default) : ExtendLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShortenLaw {
    private ShortenLaw() { }
    public sealed record ToDomain(Interval Value) : ShortenLaw;
    public sealed record AtEnd(CurveEnd Side, double Length) : ShortenLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SplitCutter {
    private SplitCutter() { }
    public sealed record AtParameters(Seq<double> Values) : SplitCutter;
    public sealed record ByBrep(GeometryHandle Value) : SplitCutter;
    public sealed record BySurface(GeometryHandle Value) : SplitCutter;
    public sealed record ByPlane(Plane Value) : SplitCutter;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PullTarget {
    private PullTarget() { }
    public sealed record ToFace(GeometryHandle Brep, int Face, bool Loose = false) : PullTarget;
    public sealed record ToMesh(GeometryHandle Mesh, bool Loose = false) : PullTarget;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectTarget {
    private ProjectTarget() { }
    public sealed record ToBreps(Seq<GeometryHandle> Values, Vector3d Direction, bool Loose = false) : ProjectTarget;
    public sealed record ToMeshes(Seq<GeometryHandle> Values, Vector3d Direction, bool Loose = false) : ProjectTarget;
    public sealed record ToPlane(Plane Value) : ProjectTarget;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlendLaw {
    private BlendLaw() { }
    public sealed record EndToEnd(BlendContinuity Continuity, Option<(double BulgeA, double BulgeB)> Bulge = default) : BlendLaw;
    public sealed record AtParameters(double T0, bool Reverse0, BlendContinuity Continuity0, double T1, bool Reverse1, BlendContinuity Continuity1) : BlendLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TweenLaw {
    private TweenLaw() { }
    public sealed record Plain : TweenLaw;
    public sealed record Matched : TweenLaw;
    public sealed record Sampled(int Samples) : TweenLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpiralLaw {
    private SpiralLaw() { }
    public sealed record AboutAxis(Point3d AxisStart, Vector3d AxisDirection) : SpiralLaw;
    public sealed record AlongRail(GeometryHandle Rail, double T0, double T1, int PointsPerTurn) : SpiralLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParabolaSeed {
    private ParabolaSeed() { }
    public sealed record FromVertex(Point3d Vertex, Point3d Start, Point3d End) : ParabolaSeed;
    public sealed record FromFocus(Point3d Focus, Point3d Start, Point3d End) : ParabolaSeed;
    public sealed record FromPoints(Point3d Start, Point3d Inner, Point3d End) : ParabolaSeed;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticCurve {
    private AnalyticCurve() { }
    public sealed record OfLine(Line Value) : AnalyticCurve;
    public sealed record OfArc(Arc Value, Option<(int Degree, int CvCount)> Structure = default) : AnalyticCurve;
    public sealed record OfCircle(Circle Value, Option<(int Degree, int CvCount)> Structure = default) : AnalyticCurve;
    public sealed record OfEllipse(Ellipse Value) : AnalyticCurve;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CatenaryLaw {
    private CatenaryLaw() { }
    public sealed record ThroughPoint(Point3d Value) : CatenaryLaw;
    public sealed record FromLength(double Value) : CatenaryLaw;
    public sealed record FromParameter(double Value) : CatenaryLaw;
    public sealed record FromApex(Point3d Value) : CatenaryLaw;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FitLaw(
    NurbsCurveFitParameters.TangentMatch TangentMatching = NurbsCurveFitParameters.TangentMatch.AtStartAndEnd,
    NurbsCurveFitParameters.KinkSplit KinkSplitting = NurbsCurveFitParameters.KinkSplit.AtG1Changes,
    NurbsCurveFitParameters.Intensity Smoothing = NurbsCurveFitParameters.Intensity.Moderate,
    NurbsCurveFitParameters.Intensity Uniformity = NurbsCurveFitParameters.Intensity.Moderate,
    NurbsCurveFitParameters.Intensity CurvatureBias = NurbsCurveFitParameters.Intensity.Moderate,
    int Degree = 3,
    int PointCount = 0,
    double SmoothingCoefficient = 0.0,
    double UniformityCoefficient = 0.0,
    double CurvatureBiasCoefficient = 0.0,
    bool SubDFriendly = false,
    bool Closed = false,
    bool ApplyTangentMatchingAtKinks = false,
    bool OptimizeCurve = true,
    IndexPair PointCountRange = default) {
    internal Fin<NurbsCurveFitParameters> Rig(Context domain, Op key) =>
        key.Catch(() => Fin.Succ(value: new NurbsCurveFitParameters {
            TangentMatching = TangentMatching,
            KinkSplitting = KinkSplitting,
            SmoothingIntensity = Smoothing,
            UniformityIntensity = Uniformity,
            CurvatureBiasIntensity = CurvatureBias,
            Degree = Degree,
            PointCount = PointCount,
            KinkAngleRadians = domain.Angle.Value,
            SmoothingCoefficient = SmoothingCoefficient,
            UniformityCoefficient = UniformityCoefficient,
            CurvatureBiasCoefficient = CurvatureBiasCoefficient,
            SubDFriendly = SubDFriendly,
            Closed = Closed,
            ApplyTangentMatchingAtKinks = ApplyTangentMatchingAtKinks,
            OptimizeCurve = OptimizeCurve,
            PointCountRange = PointCountRange,
        }));
}
```

## [04]-[OPERATION_RAIL]

- Owner: `CurveSlot` `[SmartEnum<int>]` — the consequence vocabulary; `CurveOp` `[Union]` — the whole verified curve host-op roster; `Curves` — the one entry folding any operation spread into one `Built<CurveSlot>`.
- Law: refinement is value-semantic — fair, fit, rebuild, smooth, and simplify run the instance member on the borrowed curve and own the returned refinement; the boolean tolerance-less and tween tolerance-less overloads are obsolete, so every arm runs the tolerance form off the regime.
- Law: correspondence maps survive — projection folds its curve and brep source indices, join folds its key map, curve-boolean union folds its index map, and the planar-region product folds its per-region partition as `SourceGroups`, so a consumer never re-derives which input produced which output.
- Law: the boolean-regions carrier dies at the seam — `CurveBooleanRegions` is read inside the arm, its per-region curves cross as products partitioned by a `SourceGroups` fact, its point-to-region assignments land as a `SourceMap` fact, and the disposable carrier never escapes.
- Law: end reconciliation answers pairs — `MakeEndsMeet` duplicates both curves, reconciles the duplicates, and crosses both as products, so the mutating native never touches an input handle.
- Growth: a new curve host verb is one case with its arm; a new modality is one case on the owning policy union.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CurveSlot {
    public static readonly CurveSlot Offset = new(key: 0);
    public static readonly CurveSlot OnSurface = new(key: 1);
    public static readonly CurveSlot Lifted = new(key: 2);
    public static readonly CurveSlot Ribboned = new(key: 3);
    public static readonly CurveSlot Rails = new(key: 4);
    public static readonly CurveSlot Sections = new(key: 5);
    public static readonly CurveSlot Surfaced = new(key: 6);
    public static readonly CurveSlot Refined = new(key: 7);
    public static readonly CurveSlot FitEvidence = new(key: 8);
    public static readonly CurveSlot Extended = new(key: 9);
    public static readonly CurveSlot Shortened = new(key: 10);
    public static readonly CurveSlot SplitApart = new(key: 11);
    public static readonly CurveSlot Pulled = new(key: 12);
    public static readonly CurveSlot Projected = new(key: 13);
    public static readonly CurveSlot Joined = new(key: 14);
    public static readonly CurveSlot Booled = new(key: 15);
    public static readonly CurveSlot Regions = new(key: 16);
    public static readonly CurveSlot Blended = new(key: 17);
    public static readonly CurveSlot Filleted = new(key: 18);
    public static readonly CurveSlot Tweened = new(key: 19);
    public static readonly CurveSlot Matched = new(key: 20);
    public static readonly CurveSlot Constructed = new(key: 21);
    public static readonly CurveSlot Reconciled = new(key: 22);
    public static readonly CurveSlot Outlined = new(key: 23);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveOp {
    private CurveOp() { }
    public sealed record Offset(GeometryHandle Curve, OffsetFrame Frame, double Distance) : CurveOp;
    public sealed record OffsetOnSurface(GeometryHandle Curve, GeometryHandle Host, Option<int> Face, SurfaceOffsetLaw Law) : CurveOp;
    public sealed record OffsetLift(GeometryHandle Curve, GeometryHandle Surface, double Height, bool Tangent = false) : CurveOp;
    public sealed record Ribbon(GeometryHandle Curve, RibbonLaw Law) : CurveOp;
    public sealed record Fair(GeometryHandle Curve, int ClampStart, int ClampEnd, int Iterations) : CurveOp;
    public sealed record Fit(GeometryHandle Curve, int Degree) : CurveOp;
    public sealed record Rebuild(GeometryHandle Curve, int PointCount, int Degree, bool PreserveTangents) : CurveOp;
    public sealed record Smooth(GeometryHandle Curve, double Factor, bool X, bool Y, bool Z, bool FixBoundaries, SmoothingCoordinateSystem System, Option<Plane> Frame = default) : CurveOp;
    public sealed record Simplify(GeometryHandle Curve, CurveSimplifyOptions Options, Option<CurveEnd> EndOnly = default) : CurveOp;
    public sealed record NurbsFit(GeometryHandle Curve, Interval Domain, FitLaw Law) : CurveOp;
    public sealed record Extend(GeometryHandle Curve, ExtendLaw Law) : CurveOp;
    public sealed record Shorten(GeometryHandle Curve, ShortenLaw Law) : CurveOp;
    public sealed record Split(GeometryHandle Curve, SplitCutter Cutter) : CurveOp;
    public sealed record Pull(GeometryHandle Curve, PullTarget Target) : CurveOp;
    public sealed record Project(Seq<GeometryHandle> Curves, ProjectTarget Target) : CurveOp;
    public sealed record Join(Seq<GeometryHandle> Curves, bool PreserveDirection = false, bool Simple = false) : CurveOp;
    public sealed record Boolean(BooleanVerb Verb, Seq<GeometryHandle> First, Seq<GeometryHandle> Second) : CurveOp;
    public sealed record Regions(Seq<GeometryHandle> Curves, Plane Frame, Seq<Point3d> Points, bool Combine) : CurveOp;
    public sealed record Blend(GeometryHandle First, GeometryHandle Second, BlendLaw Law) : CurveOp;
    public sealed record ArcBlend(Point3d Start, Vector3d StartDirection, Point3d End, Vector3d EndDirection, double RatioOrRadius, bool LineArc = false) : CurveOp;
    public sealed record FilletCurves(GeometryHandle First, Point3d NearFirst, GeometryHandle Second, Point3d NearSecond, double Radius, bool JoinResult, bool TrimInputs, bool ArcExtension) : CurveOp;
    public sealed record FilletCorners(GeometryHandle Curve, double Radius) : CurveOp;
    public sealed record Tween(GeometryHandle First, GeometryHandle Second, int Count, TweenLaw Law) : CurveOp;
    public sealed record MatchCurve(GeometryHandle First, bool ReverseFirst, BlendContinuity Continuity, GeometryHandle Second, bool ReverseSecond, PreserveEnd Preserve, bool Average) : CurveOp;
    public sealed record Mean(GeometryHandle First, GeometryHandle Second) : CurveOp;
    public sealed record TwoView(GeometryHandle First, GeometryHandle Second, Vector3d FirstDirection, Vector3d SecondDirection) : CurveOp;
    public sealed record Interpolated(Seq<Point3d> Points, int Degree, Option<CurveKnotStyle> Knots = default, Option<(Vector3d Start, Vector3d End)> Tangents = default) : CurveOp;
    public sealed record ControlPoints(Seq<Point3d> Points, int Degree = 3) : CurveOp;
    public sealed record FitPoints(Seq<Point3d> Points, bool Periodic = false, Option<(int Degree, Vector3d Start, Vector3d End)> Constrained = default) : CurveOp;
    public sealed record HSpline(Seq<Point3d> Points, Option<(Vector3d Start, Vector3d End)> Tangents = default) : CurveOp;
    public sealed record SoftEdit(GeometryHandle Curve, double T, Vector3d Delta, double Length, bool FixEnds = true) : CurveOp;
    public sealed record PeriodicClose(GeometryHandle Curve, bool Smooth = true) : CurveOp;
    public sealed record SubDFriendly(GeometryHandle Curve, Option<(int PointCount, bool PeriodicClosed)> Structure = default) : CurveOp;
    public sealed record Spiral(SpiralLaw Law, Point3d RadiusPoint, double Pitch, double TurnCount, double Radius0, double Radius1) : CurveOp;
    public sealed record Parabola(ParabolaSeed Seed) : CurveOp;
    public sealed record ArcBezier(int Degree, Point3d Center, Point3d Start, Point3d End, double Radius, double TanSlider, double MidSlider) : CurveOp;
    public sealed record Analytic(AnalyticCurve Seed) : CurveOp;
    public sealed record Catenary(Point3d Start, Point3d End, Vector3d AxisDirection, CatenaryLaw Law, bool Smooth, int PointCount) : CurveOp;
    public sealed record MakeEndsMeet(GeometryHandle First, bool AdjustStartFirst, GeometryHandle Second, bool AdjustStartSecond) : CurveOp;
    public sealed record TextOutlines(string Text, string Font, double Height, int Style, bool CloseLoops, Plane Frame, double SmallCapsScale = 1.0) : CurveOp;

    internal Fin<Built<CurveSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            offset: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Offset)), (curve, op) =>
                Many(op, CurveSlot.Offset, () => edit.Frame switch {
                    OffsetFrame.InPlane frame => curve.Offset(
                        plane: frame.Value, distance: edit.Distance, tolerance: model.Absolute.Value, cornerStyle: frame.Corner),
                    OffsetFrame.ByNormal frame => curve.Offset(
                        directionPoint: frame.DirectionPoint, normal: frame.Normal, distance: edit.Distance,
                        tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value, loose: frame.Loose,
                        cornerStyle: frame.Corner, endStyle: frame.End),
                    _ => [],
                })),
            offsetOnSurface: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(OffsetOnSurface)), (curve, op) =>
                edit.Face.Case switch {
                    int face => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: edit.Host, key: op, body: host =>
                        from _ in guard(face >= 0 && face < host.Faces.Count, op.InvalidInput())
                        from built in Many(op, CurveSlot.OnSurface, () => edit.Law switch {
                            SurfaceOffsetLaw.ByDistance law => curve.OffsetOnSurface(face: host.Faces[face], distance: law.Value, fittingTolerance: model.Absolute.Value),
                            SurfaceOffsetLaw.ThroughPoint law => curve.OffsetOnSurface(face: host.Faces[face], throughPoint: law.Value, fittingTolerance: model.Absolute.Value),
                            SurfaceOffsetLaw.Varying law => curve.OffsetOnSurface(
                                face: host.Faces[face],
                                curveParameters: law.Rows.Map(static row => row.Parameter).ToArray(),
                                offsetDistances: law.Rows.Map(static row => row.Distance).ToArray(),
                                fittingTolerance: model.Absolute.Value),
                            _ => [],
                        })
                        select built),
                    _ => ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: edit.Host, key: op, body: host =>
                        Many(op, CurveSlot.OnSurface, () => edit.Law switch {
                            SurfaceOffsetLaw.ByDistance law => curve.OffsetOnSurface(surface: host, distance: law.Value, fittingTolerance: model.Absolute.Value),
                            SurfaceOffsetLaw.ThroughPoint law => curve.OffsetOnSurface(surface: host, throughPoint: law.Value, fittingTolerance: model.Absolute.Value),
                            SurfaceOffsetLaw.Varying law => curve.OffsetOnSurface(
                                surface: host,
                                curveParameters: law.Rows.Map(static row => row.Parameter).ToArray(),
                                offsetDistances: law.Rows.Map(static row => row.Distance).ToArray(),
                                fittingTolerance: model.Absolute.Value),
                            _ => [],
                        })),
                }),
            offsetLift: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(OffsetLift)), (curve, op) =>
                ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: edit.Surface, key: op, body: surface =>
                    Single(op, CurveSlot.Lifted, () => edit.Tangent
                        ? curve.OffsetTangentToSurface(surface: surface, height: edit.Height)
                        : curve.OffsetNormalToSurface(surface: surface, height: edit.Height)))),
            ribbon: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Ribbon)), (curve, op) =>
                from parameters in edit.Law.Rig(domain: model, key: op)
                from built in op.Catch(() => {
                    Curve ribbon = curve.RibbonOffset(
                        ribbonParameters: parameters, railCurves: out Curve[] rails,
                        crossSectionCurves: out Curve[] sections, brepSurfaces: out Brep[] breps);
                    return
                        from primary in ModelGate.Own(built: ribbon, key: op)
                        from railed in ModelGate.OwnMany(built: rails, key: op, allowEmpty: true).MapFail(e => { primary.Dispose(); return e; })
                        from sectioned in ModelGate.OwnMany(built: sections, key: op, allowEmpty: true).MapFail(e => { primary.Dispose(); _ = railed.Iter(static h => h.Dispose()); return e; })
                        from surfaced in ModelGate.OwnMany(built: breps, key: op, allowEmpty: true).MapFail(e => {
                            primary.Dispose();
                            _ = railed.Iter(static h => h.Dispose());
                            _ = sectioned.Iter(static h => h.Dispose());
                            return e;
                        })
                        select new Built<CurveSlot>(
                            Products: Seq(primary) + railed + sectioned + surfaced,
                            Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Ribboned, body: new BuildBody.Tally(Count: 1))
                                + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Rails, body: new BuildBody.Tally(Count: railed.Count))
                                + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Sections, body: new BuildBody.Tally(Count: sectioned.Count))
                                + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Surfaced, body: new BuildBody.Tally(Count: surfaced.Count)));
                })
                select built),
            fair: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Fair)), (curve, op) =>
                Single(op, CurveSlot.Refined, () => curve.Fair(
                    distanceTolerance: model.Absolute.Value, angleTolerance: model.Angle.Value,
                    clampStart: edit.ClampStart, clampEnd: edit.ClampEnd, iterations: edit.Iterations))),
            fit: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Fit)), (curve, op) =>
                Single(op, CurveSlot.Refined, () => curve.Fit(degree: edit.Degree, fitTolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))),
            rebuild: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Rebuild)), (curve, op) =>
                Single(op, CurveSlot.Refined, () => curve.Rebuild(pointCount: edit.PointCount, degree: edit.Degree, preserveTangents: edit.PreserveTangents))),
            smooth: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Smooth)), (curve, op) =>
                Single(op, CurveSlot.Refined, () => edit.Frame.Case switch {
                    Plane frame => curve.Smooth(
                        smoothFactor: edit.Factor, bXSmooth: edit.X, bYSmooth: edit.Y, bZSmooth: edit.Z,
                        bFixBoundaries: edit.FixBoundaries, coordinateSystem: edit.System, plane: frame),
                    _ => curve.Smooth(
                        smoothFactor: edit.Factor, bXSmooth: edit.X, bYSmooth: edit.Y, bZSmooth: edit.Z,
                        bFixBoundaries: edit.FixBoundaries, coordinateSystem: edit.System),
                })),
            simplify: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Simplify)), (curve, op) =>
                Single(op, CurveSlot.Refined, () => edit.EndOnly.Case switch {
                    CurveEnd end => curve.SimplifyEnd(end: end, options: edit.Options,
                        distanceTolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value),
                    _ => curve.Simplify(options: edit.Options,
                        distanceTolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value),
                })),
            nurbsFit: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(NurbsFit)), (curve, op) =>
                edit.Law.Rig(domain: model, key: op).Bind(parameters => {
                    using (parameters) {
                        return op.Catch(() => {
                            NurbsCurve fitted = Curve.CreateNurbsCurveFit(
                                curve: curve, domain: edit.Domain, rebuildOptions: parameters,
                                maximumSeparation: out Line separation, thisSeparationParameter: out double atSource, nurbsSeparationParameter: out double atFit);
                            return ModelGate.Own(built: fitted, key: op).Map(owned => new Built<CurveSlot>(
                                Products: Seq(owned),
                                Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Refined, body: new BuildBody.Tally(Count: 1))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Measure(Value: separation.Length))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Marks(Points: Seq(separation.From, separation.To)))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Measure(Value: atSource))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Measure(Value: atFit))));
                        });
                    }
                })),
            extend: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Extend)), (curve, op) =>
                edit.Law switch {
                    ExtendLaw.ByLength law => Single(op, CurveSlot.Extended, () => curve.Extend(side: law.Side, length: law.Length, style: law.Style)),
                    ExtendLaw.ToPoint law => Single(op, CurveSlot.Extended, () => curve.Extend(side: law.Side, style: law.Style, endPoint: law.Terminal)),
                    ExtendLaw.ToGeometry law => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: op,
                        body: bounds => Single(op, CurveSlot.Extended, () => curve.Extend(side: law.Side, style: law.Style, geometry: bounds.AsIterable()))),
                    ExtendLaw.ByLine law => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: op,
                        body: bounds => Single(op, CurveSlot.Extended, () => curve.ExtendByLine(side: law.Side, geometry: bounds.AsIterable()))),
                    ExtendLaw.ByArc law => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: op,
                        body: bounds => Single(op, CurveSlot.Extended, () => curve.ExtendByArc(side: law.Side, geometry: bounds.AsIterable()))),
                    ExtendLaw.OnSurface law => law.Face.Case switch {
                        int face => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Target, key: op, body: host =>
                            from _ in guard(face >= 0 && face < host.Faces.Count, op.InvalidInput())
                            from built in Single(op, CurveSlot.Extended, () => curve.ExtendOnSurface(side: law.Side, face: host.Faces[face]))
                            select built),
                        _ => ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: law.Target, key: op,
                            body: host => Single(op, CurveSlot.Extended, () => curve.ExtendOnSurface(side: law.Side, surface: host))),
                    },
                    _ => Fin.Fail<Built<CurveSlot>>(error: op.InvalidInput()),
                }),
            shorten: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Shorten)), (curve, op) =>
                edit.Law.Switch(
                    state: (Curve: curve, Op: op),
                    toDomain: static (ctx, law) => Single(ctx.Op, CurveSlot.Shortened, () => ctx.Curve.Trim(domain: law.Value)),
                    atEnd: static (ctx, law) => Single(ctx.Op, CurveSlot.Shortened, () => ctx.Curve.Trim(side: law.Side, length: law.Length)))),
            split: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Split)), (curve, op) =>
                edit.Cutter switch {
                    SplitCutter.AtParameters law => Many(op, CurveSlot.SplitApart, () => curve.Split(t: law.Values.AsIterable())),
                    SplitCutter.ByPlane law => Many(op, CurveSlot.SplitApart, () => curve.Split(
                        plane: law.Value, tolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value)),
                    SplitCutter.ByBrep law => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Value, key: op, body: cutter =>
                        Many(op, CurveSlot.SplitApart, () => curve.Split(cutter: cutter, tolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value))),
                    SplitCutter.BySurface law => ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: law.Value, key: op, body: cutter =>
                        Many(op, CurveSlot.SplitApart, () => curve.Split(cutter: cutter, tolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value))),
                    _ => Fin.Fail<Built<CurveSlot>>(error: op.InvalidInput()),
                }),
            pull: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Pull)), (curve, op) =>
                edit.Target switch {
                    PullTarget.ToFace law => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Brep, key: op, body: host =>
                        from _ in guard(law.Face >= 0 && law.Face < host.Faces.Count, op.InvalidInput())
                        from built in law.Loose
                            ? Many(op, CurveSlot.Pulled, () => Curve.PullToBrepFace(curve: curve, face: host.Faces[law.Face], tolerance: model.Absolute.Value, loose: true))
                            : Many(op, CurveSlot.Pulled, () => curve.PullToBrepFace(face: host.Faces[law.Face], tolerance: model.Absolute.Value))
                        select built),
                    PullTarget.ToMesh law => ModelGate.Borrow<Mesh, Built<CurveSlot>>(handle: law.Mesh, key: op, body: mesh =>
                        Single(op, CurveSlot.Pulled, () => law.Loose
                            ? curve.PullToMesh(mesh: mesh, tolerance: model.Absolute.Value, loose: true)
                            : curve.PullToMesh(mesh: mesh, tolerance: model.Absolute.Value))),
                    _ => Fin.Fail<Built<CurveSlot>>(error: op.InvalidInput()),
                }),
            project: static (model, edit) => {
                Op op = Op.Of(name: nameof(Project));
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Curves, key: op, body: curves =>
                    edit.Target switch {
                        ProjectTarget.ToBreps law => ModelGate.BorrowMany<Brep, Built<CurveSlot>>(handles: law.Values, key: op, body: breps =>
                            op.Catch(() => {
                                Curve[] projected = Curve.ProjectToBrep(
                                    curves: curves.AsIterable(), breps: breps.AsIterable(), direction: law.Direction,
                                    tolerance: model.Absolute.Value, loose: law.Loose, curveIndices: out int[] curveMap, brepIndices: out int[] brepMap);
                                return ModelGate.OwnMany(built: projected, key: op).Map(owned => new Built<CurveSlot>(
                                    Products: owned,
                                    Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Projected, body: new BuildBody.Tally(Count: owned.Count))
                                        + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Projected, body: new BuildBody.SourceMap(Rows: toSeq(curveMap ?? [])))
                                        + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Projected, body: new BuildBody.SourceMap(Rows: toSeq(brepMap ?? [])))));
                            })),
                        ProjectTarget.ToMeshes law => ModelGate.BorrowMany<Mesh, Built<CurveSlot>>(handles: law.Values, key: op, body: meshes =>
                            Many(op, CurveSlot.Projected, () => Curve.ProjectToMesh(
                                curves: curves.AsIterable(), meshes: meshes.AsIterable(), direction: law.Direction,
                                tolerance: model.Absolute.Value, loose: law.Loose))),
                        ProjectTarget.ToPlane law => curves.Count == 1
                            ? Single(op, CurveSlot.Projected, () => Curve.ProjectToPlane(curve: curves[0], plane: law.Value))
                            : Fin.Fail<Built<CurveSlot>>(error: op.InvalidInput()),
                        _ => Fin.Fail<Built<CurveSlot>>(error: op.InvalidInput()),
                    });
            },
            join: static (model, edit) => {
                Op op = Op.Of(name: nameof(Join));
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Curves, key: op, body: curves =>
                    op.Catch(() => {
                        Curve[] joined = Curve.JoinCurves(
                            inputCurves: curves.AsIterable(), joinTolerance: model.Absolute.Value,
                            preserveDirection: edit.PreserveDirection, simpleJoin: edit.Simple, key: out int[] map);
                        return ModelGate.OwnMany(built: joined, key: op).Map(owned => new Built<CurveSlot>(
                            Products: owned,
                            Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Joined, body: new BuildBody.Tally(Count: owned.Count))
                                + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Joined, body: new BuildBody.SourceMap(Rows: toSeq(map ?? [])))));
                    }));
            },
            boolean: static (model, edit) => {
                Op op = Op.Of(name: nameof(Boolean));
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.First, key: op, body: first =>
                    ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Second, key: op, allowEmpty: !edit.Verb.RequiresSecond, body: second =>
                        edit.Verb.Switch(
                            state: (First: first, Second: second, Tolerance: model.Absolute.Value, Op: op),
                            union: static ctx => ctx.Op.Catch(() => {
                                Curve[] fused = Curve.CreateBooleanUnion(curves: ctx.First.AsIterable(), tolerance: ctx.Tolerance, indexMap: out int[] map);
                                return ModelGate.OwnMany(built: fused, key: ctx.Op).Map(owned => new Built<CurveSlot>(
                                    Products: owned,
                                    Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Booled, body: new BuildBody.Tally(Count: owned.Count))
                                        + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Booled, body: new BuildBody.SourceMap(Rows: toSeq(map ?? [])))));
                            }),
                            intersection: static ctx => ctx.First.Count == 1 && ctx.Second.Count == 1
                                ? Many(ctx.Op, CurveSlot.Booled, () => Curve.CreateBooleanIntersection(curveA: ctx.First[0], curveB: ctx.Second[0], tolerance: ctx.Tolerance))
                                : Fin.Fail<Built<CurveSlot>>(error: ctx.Op.InvalidInput()),
                            difference: static ctx => ctx.First.Count == 1
                                ? Many(ctx.Op, CurveSlot.Booled, () => Curve.CreateBooleanDifference(curveA: ctx.First[0], subtractors: ctx.Second.AsIterable(), tolerance: ctx.Tolerance))
                                : Fin.Fail<Built<CurveSlot>>(error: ctx.Op.InvalidInput()),
                            split: static ctx => Fin.Fail<Built<CurveSlot>>(error: ctx.Op.Unsupported(geometryType: typeof(Curve), outputType: typeof(Curve))))));
            },
            regions: static (model, edit) => {
                Op op = Op.Of(name: nameof(Regions));
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Curves, key: op, body: curves =>
                    op.Catch(() => {
                        CurveBooleanRegions? acquired = edit.Points.IsEmpty
                            ? Curve.CreateBooleanRegions(curves: curves.AsIterable(), plane: edit.Frame, combineRegions: edit.Combine, tolerance: model.Absolute.Value)
                            : Curve.CreateBooleanRegions(curves: curves.AsIterable(), plane: edit.Frame, points: edit.Points.AsIterable(), combineRegions: edit.Combine, tolerance: model.Absolute.Value);
                        return Optional(acquired).ToFin(Fail: op.InvalidResult()).Bind(live => {
                            using (live) {
                            Seq<Seq<Curve>> regions = toSeq(Enumerable.Range(start: 0, count: live.RegionCount))
                                .Map(region => toSeq(live.RegionCurves(regionIndex: region)));
                            Seq<int> pointRegions = toSeq(Enumerable.Range(start: 0, count: live.PointCount))
                                .Map(point => live.RegionPointIndex(pointIndex: point));
                            int offset = 0;
                            Seq<Seq<int>> groups = regions.Map(region => {
                                Seq<int> rows = toSeq(Enumerable.Range(start: offset, count: region.Count));
                                offset += region.Count;
                                return rows;
                            });
                            return ModelGate.OwnMany(built: regions.Bind(static region => region), key: op).Map(owned => new Built<CurveSlot>(
                                Products: owned,
                                Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Regions, body: new BuildBody.SourceGroups(Groups: groups))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Regions, body: new BuildBody.SourceMap(Rows: pointRegions))));
                            }
                        });
                    }));
            },
            blend: static (_, edit) => {
                Op op = Op.Of(name: nameof(Blend));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        edit.Law.Switch(
                            state: (First: first, Second: second, Op: op),
                            endToEnd: static (ctx, law) => Single(ctx.Op, CurveSlot.Blended, () => law.Bulge.Case switch {
                                (double bulgeA, double bulgeB) => Curve.CreateBlendCurve(
                                    curveA: ctx.First, curveB: ctx.Second, continuity: law.Continuity, bulgeA: bulgeA, bulgeB: bulgeB),
                                _ => Curve.CreateBlendCurve(curveA: ctx.First, curveB: ctx.Second, continuity: law.Continuity),
                            }),
                            atParameters: static (ctx, law) => Single(ctx.Op, CurveSlot.Blended, () => Curve.CreateBlendCurve(
                                curve0: ctx.First, t0: law.T0, reverse0: law.Reverse0, continuity0: law.Continuity0,
                                curve1: ctx.Second, t1: law.T1, reverse1: law.Reverse1, continuity1: law.Continuity1)))));
            },
            arcBlend: static (_, edit) => {
                Op op = Op.Of(name: nameof(ArcBlend));
                return Single(op, CurveSlot.Blended, () => edit.LineArc
                    ? Curve.CreateArcLineArcBlend(startPt: edit.Start, startDir: edit.StartDirection, endPt: edit.End, endDir: edit.EndDirection, radius: edit.RatioOrRadius)
                    : Curve.CreateArcBlend(startPt: edit.Start, startDir: edit.StartDirection, endPt: edit.End, endDir: edit.EndDirection, controlPointLengthRatio: edit.RatioOrRadius));
            },
            filletCurves: static (model, edit) => {
                Op op = Op.Of(name: nameof(FilletCurves));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        Many(op, CurveSlot.Filleted, () => Curve.CreateFilletCurves(
                            curve0: first, point0: edit.NearFirst, curve1: second, point1: edit.NearSecond,
                            radius: edit.Radius, join: edit.JoinResult, trim: edit.TrimInputs, arcExtension: edit.ArcExtension,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))));
            },
            filletCorners: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(FilletCorners)), (curve, op) =>
                Single(op, CurveSlot.Filleted, () => Curve.CreateFilletCornersCurve(
                    curve: curve, radius: edit.Radius, tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))),
            tween: static (model, edit) => {
                Op op = Op.Of(name: nameof(Tween));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        Many(op, CurveSlot.Tweened, () => edit.Law switch {
                            TweenLaw.Matched => Curve.CreateTweenCurvesWithMatching(curve0: first, curve1: second, numCurves: edit.Count, tolerance: model.Absolute.Value),
                            TweenLaw.Sampled law => Curve.CreateTweenCurvesWithSampling(curve0: first, curve1: second, numCurves: edit.Count, numSamples: law.Samples, tolerance: model.Absolute.Value),
                            _ => Curve.CreateTweenCurves(curve0: first, curve1: second, numCurves: edit.Count, tolerance: model.Absolute.Value),
                        })));
            },
            matchCurve: static (_, edit) => {
                Op op = Op.Of(name: nameof(MatchCurve));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        Many(op, CurveSlot.Matched, () => Curve.CreateMatchCurve(
                            curve0: first, reverse0: edit.ReverseFirst, continuity: edit.Continuity,
                            curve1: second, reverse1: edit.ReverseSecond, preserve: edit.Preserve, average: edit.Average))));
            },
            mean: static (model, edit) => {
                Op op = Op.Of(name: nameof(Mean));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        Single(op, CurveSlot.Constructed, () => Curve.CreateMeanCurve(curveA: first, curveB: second, angleToleranceRadians: model.Angle.Value))));
            },
            twoView: static (model, edit) => {
                Op op = Op.Of(name: nameof(TwoView));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        Many(op, CurveSlot.Constructed, () => Curve.CreateCurve2View(
                            curveA: first, curveB: second, vectorA: edit.FirstDirection, vectorB: edit.SecondDirection,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))));
            },
            interpolated: static (_, edit) => {
                Op op = Op.Of(name: nameof(Interpolated));
                return Single(op, CurveSlot.Constructed, () => (edit.Knots.Case, edit.Tangents.Case) switch {
                    (CurveKnotStyle knots, (Vector3d start, Vector3d end)) => Curve.CreateInterpolatedCurve(
                        points: edit.Points.AsIterable(), degree: edit.Degree, knots: knots, startTangent: start, endTangent: end),
                    (CurveKnotStyle knots, _) => Curve.CreateInterpolatedCurve(points: edit.Points.AsIterable(), degree: edit.Degree, knots: knots),
                    _ => Curve.CreateInterpolatedCurve(points: edit.Points.AsIterable(), degree: edit.Degree),
                });
            },
            controlPoints: static (_, edit) => {
                Op op = Op.Of(name: nameof(ControlPoints));
                return Single(op, CurveSlot.Constructed, () => Curve.CreateControlPointCurve(points: edit.Points.AsIterable(), degree: edit.Degree));
            },
            fitPoints: static (model, edit) => {
                Op op = Op.Of(name: nameof(FitPoints));
                return Single(op, CurveSlot.Constructed, () => edit.Constrained.Case switch {
                    (int degree, Vector3d start, Vector3d end) => NurbsCurve.CreateFromFitPoints(
                        points: edit.Points.AsIterable(), tolerance: model.Absolute.Value, degree: degree,
                        periodic: edit.Periodic, startTangent: start, endTangent: end),
                    _ => NurbsCurve.CreateFromFitPoints(points: edit.Points.AsIterable(), tolerance: model.Absolute.Value, periodic: edit.Periodic),
                });
            },
            hSpline: static (_, edit) => {
                Op op = Op.Of(name: nameof(HSpline));
                return Single(op, CurveSlot.Constructed, () => edit.Tangents.Case switch {
                    (Vector3d start, Vector3d end) => NurbsCurve.CreateHSpline(points: edit.Points.AsIterable(), startTangent: start, endTangent: end),
                    _ => NurbsCurve.CreateHSpline(points: edit.Points.AsIterable()),
                });
            },
            softEdit: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(SoftEdit)), (curve, op) =>
                Single(op, CurveSlot.Refined, () => Curve.CreateSoftEditCurve(
                    curve: curve, t: edit.T, delta: edit.Delta, length: edit.Length, fixEnds: edit.FixEnds))),
            periodicClose: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(PeriodicClose)), (curve, op) =>
                Single(op, CurveSlot.Refined, () => Curve.CreatePeriodicCurve(curve: curve, smooth: edit.Smooth))),
            subDFriendly: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(SubDFriendly)), (curve, op) =>
                Single(op, CurveSlot.Refined, () => edit.Structure.Case switch {
                    (int points, bool periodic) => NurbsCurve.CreateSubDFriendly(curve: curve, pointCount: points, periodicClosedCurve: periodic),
                    _ => NurbsCurve.CreateSubDFriendly(curve: curve),
                })),
            spiral: static (_, edit) => {
                Op op = Op.Of(name: nameof(Spiral));
                return edit.Law switch {
                    SpiralLaw.AboutAxis law => Single(op, CurveSlot.Constructed, () => NurbsCurve.CreateSpiral(
                        axisStart: law.AxisStart, axisDir: law.AxisDirection, radiusPoint: edit.RadiusPoint,
                        pitch: edit.Pitch, turnCount: edit.TurnCount, radius0: edit.Radius0, radius1: edit.Radius1)),
                    SpiralLaw.AlongRail law => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.Rail, key: op, body: rail =>
                        Single(op, CurveSlot.Constructed, () => NurbsCurve.CreateSpiral(
                            railCurve: rail, t0: law.T0, t1: law.T1, radiusPoint: edit.RadiusPoint,
                            pitch: edit.Pitch, turnCount: edit.TurnCount, radius0: edit.Radius0, radius1: edit.Radius1, pointsPerTurn: law.PointsPerTurn))),
                    _ => Fin.Fail<Built<CurveSlot>>(error: op.InvalidInput()),
                };
            },
            parabola: static (_, edit) => {
                Op op = Op.Of(name: nameof(Parabola));
                return edit.Seed.Switch(
                    state: op,
                    fromVertex: static (key, seed) => Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromVertex(
                        vertex: seed.Vertex, startPoint: seed.Start, endPoint: seed.End)),
                    fromFocus: static (key, seed) => Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromFocus(
                        focus: seed.Focus, startPoint: seed.Start, endPoint: seed.End)),
                    fromPoints: static (key, seed) => Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromPoints(
                        startPoint: seed.Start, innerPoint: seed.Inner, endPoint: seed.End)));
            },
            arcBezier: static (_, edit) => {
                Op op = Op.Of(name: nameof(ArcBezier));
                return Single(op, CurveSlot.Constructed, () => NurbsCurve.CreateNonRationalArcBezier(
                    degree: edit.Degree, center: edit.Center, start: edit.Start, end: edit.End,
                    radius: edit.Radius, tanSlider: edit.TanSlider, midSlider: edit.MidSlider));
            },
            analytic: static (_, edit) => {
                Op op = Op.Of(name: nameof(Analytic));
                return edit.Seed.Switch(
                    state: op,
                    ofLine: static (key, seed) => Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateFromLine(line: seed.Value)),
                    ofArc: static (key, seed) => Single(key, CurveSlot.Constructed, () => seed.Structure.Case switch {
                        (int degree, int cvCount) => NurbsCurve.CreateFromArc(arc: seed.Value, degree: degree, cvCount: cvCount),
                        _ => NurbsCurve.CreateFromArc(arc: seed.Value),
                    }),
                    ofCircle: static (key, seed) => Single(key, CurveSlot.Constructed, () => seed.Structure.Case switch {
                        (int degree, int cvCount) => NurbsCurve.CreateFromCircle(circle: seed.Value, degree: degree, cvCount: cvCount),
                        _ => NurbsCurve.CreateFromCircle(circle: seed.Value),
                    }),
                    ofEllipse: static (key, seed) => Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateFromEllipse(ellipse: seed.Value)));
            },
            catenary: static (_, edit) => {
                Op op = Op.Of(name: nameof(Catenary));
                return op.Catch(() => {
                    (Curve Hung, Point3d Apex, double Parameter, double Length, double Deviation) result = edit.Law.Switch(
                        state: edit,
                        throughPoint: static (request, law) => RunCatenary(
                            run: (out Point3d apex, out double parameter, out double length, out double deviation) => Curve.CreateCatenaryCurveThroughPoint(
                                catenary_start: request.Start, catenary_end: request.End, axis_dir: request.AxisDirection, law.Value,
                                bSmooth: request.Smooth, point_count: request.PointCount,
                                apex_out: out apex, parameter_out: out parameter, length_out: out length, max_deviation_out: out deviation)),
                        fromLength: static (request, law) => RunCatenary(
                            run: (out Point3d apex, out double parameter, out double length, out double deviation) => Curve.CreateCatenaryCurveFromLength(
                                catenary_start: request.Start, catenary_end: request.End, axis_dir: request.AxisDirection, law.Value,
                                bSmooth: request.Smooth, point_count: request.PointCount,
                                apex_out: out apex, parameter_out: out parameter, length_out: out length, max_deviation_out: out deviation)),
                        fromParameter: static (request, law) => RunCatenary(
                            run: (out Point3d apex, out double parameter, out double length, out double deviation) => Curve.CreateCatenaryCurveFromParameter(
                                catenary_start: request.Start, catenary_end: request.End, axis_dir: request.AxisDirection, law.Value,
                                bSmooth: request.Smooth, point_count: request.PointCount,
                                apex_out: out apex, parameter_out: out parameter, length_out: out length, max_deviation_out: out deviation)),
                        fromApex: static (request, law) => RunCatenary(
                            run: (out Point3d apex, out double parameter, out double length, out double deviation) => Curve.CreateCatenaryCurveFromApex(
                                catenary_start: request.Start, catenary_end: request.End, axis_dir: request.AxisDirection, law.Value,
                                bSmooth: request.Smooth, point_count: request.PointCount,
                                apex_out: out apex, parameter_out: out parameter, length_out: out length, max_deviation_out: out deviation)));
                    return ModelGate.Own(built: result.Hung, key: op).Map(owned => new Built<CurveSlot>(
                        Products: Seq(owned),
                        Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Constructed, body: new BuildBody.Marks(Points: Seq(result.Apex)))
                            + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Constructed, body: new BuildBody.Measure(Value: result.Parameter))
                            + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Constructed, body: new BuildBody.Measure(Value: result.Length))
                            + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Constructed, body: new BuildBody.Measure(Value: result.Deviation))));
                });
            },
            makeEndsMeet: static (_, edit) => {
                Op op = Op.Of(name: nameof(MakeEndsMeet));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        op.Catch(() => {
                            Curve workingFirst = (Curve)first.Duplicate();
                            Curve workingSecond = (Curve)second.Duplicate();
                            return op.Confirm(success: Curve.MakeEndsMeet(
                                curveA: workingFirst, adjustStartCurveA: edit.AdjustStartFirst,
                                curveB: workingSecond, adjustStartCurveB: edit.AdjustStartSecond)).Match(
                                Succ: _ =>
                                    from owned in ModelGate.Own(built: workingFirst, key: op)
                                    from other in ModelGate.Own(built: workingSecond, key: op).MapFail(e => { owned.Dispose(); return e; })
                                    select new Built<CurveSlot>(
                                        Products: Seq(owned, other),
                                        Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Reconciled, body: new BuildBody.Tally(Count: 2))),
                                Fail: error => {
                                    workingFirst.Dispose();
                                    workingSecond.Dispose();
                                    return Fin.Fail<Built<CurveSlot>>(error: error);
                                });
                        })));
            },
            textOutlines: static (model, edit) => {
                Op op = Op.Of(name: nameof(TextOutlines));
                return
                    from text in op.AcceptText(value: edit.Text)
                    from font in op.AcceptText(value: edit.Font)
                    from built in Many(op, CurveSlot.Outlined, () => Curve.CreateTextOutlines(
                        text: text, font: font, textHeight: edit.Height, textStyle: edit.Style, closeLoops: edit.CloseLoops,
                        plane: edit.Frame, smallCapsScale: edit.SmallCapsScale, tolerance: model.Absolute.Value))
                    select built;
            });

    private static Fin<Built<CurveSlot>> Borrowed(GeometryHandle handle, Op op, Func<Curve, Op, Fin<Built<CurveSlot>>> body) =>
        ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: handle, key: op, body: curve => body(curve, op));

    private static Fin<Built<CurveSlot>> Single(Op op, CurveSlot slot, Func<GeometryBase?> run) =>
        op.Catch(() => ModelGate.Own(built: run(), key: op).Map(owned => new Built<CurveSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<CurveSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: 1)))));

    private static Fin<Built<CurveSlot>> Many(Op op, CurveSlot slot, Func<System.Collections.Generic.IEnumerable<Curve>> run) =>
        op.Catch(() => ModelGate.OwnMany(built: run(), key: op).Map(owned => new Built<CurveSlot>(
            Products: owned,
            Evidence: BuildReceipt<CurveSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count)))));

    private delegate Curve CatenaryRun(out Point3d apex, out double parameter, out double length, out double deviation);

    private static (Curve Hung, Point3d Apex, double Parameter, double Length, double Deviation) RunCatenary(CatenaryRun run) {
        Curve hung = run(out Point3d apex, out double parameter, out double length, out double deviation);
        return (Hung: hung, Apex: apex, Parameter: parameter, Length: length, Deviation: deviation);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Curves {
    public static Fin<Built<CurveSlot>> Build(Context context, params ReadOnlySpan<CurveOp> operations) {
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

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]            | [FORM]                                              | [ENTRY]                          |
| :-----: | :----------------- | :----------------- | :--------------------------------------------------- | :-------------------------------- |
|  [01]   | offset framing     | `OffsetFrame`      | planar versus normal-framed with corner/end style   | `CurveOp.Offset`                 |
|  [02]   | on-surface offset  | `SurfaceOffsetLaw` | constant, through-point, varying rows               | `CurveOp.OffsetOnSurface`        |
|  [03]   | ribbon carrier     | `RibbonLaw`        | whole `RibbonOffsetParameters` as one value         | `CurveOp.Ribbon` / `Rig`         |
|  [04]   | advanced fit       | `FitLaw`           | whole `NurbsCurveFitParameters` as one value        | `CurveOp.NurbsFit` / `Rig`       |
|  [05]   | extension modality | `ExtendLaw`        | length, geometry, point, line, arc, on-surface      | `CurveOp.Extend`                 |
|  [06]   | split modality     | `SplitCutter`      | parameters, brep, surface, plane                    | `CurveOp.Split`                  |
|  [07]   | pull and project   | `PullTarget` / `ProjectTarget` | destinations with loose grants and index maps | `CurveOp.Pull` / `Project` |
|  [08]   | curve booleans     | `CurveOp`          | verb rows + disposable region carrier at the seam   | `Boolean` / `Regions`            |
|  [09]   | blend and tween    | `BlendLaw` / `TweenLaw` | end-to-end, at-parameter, matched, sampled     | `CurveOp.Blend` / `Tween`        |
|  [10]   | construction seeds | `SpiralLaw` / `ParabolaSeed` / `AnalyticCurve` / `CatenaryLaw` | seed unions selecting statics | construction cases |
|  [11]   | curve verbs        | `CurveOp`          | one flat `[Union]`, total generated dispatch        | `Curves.Build`                   |
