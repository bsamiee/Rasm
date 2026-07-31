# [RASM_RHINO_MODELING_CURVES]

`Rasm.Rhino.Modeling` owns curve host operations. One `CurveOp` union carries offset, refinement, extension, shortening, splitting, pulling, projection, joining, booleans, planar regions, blend/fillet/tween/match/mean construction, interpolation, analytic and freeform seeding, text outlines, and subd-friendly rebuilds through `Curves.Build`. Policy unions discriminate native overloads from payload shape; `Context` supplies every tolerance and angle. Kernel parametric evaluation, division, curvature, contours, and predicate-exact offsets remain kernel-owned.

## [01]-[INDEX]

- [02]-[OFFSET_POLICY]: `CurveScalar`, `OffsetFrame`, `SurfaceOffsetTarget`, `SurfaceLift`, `RibbonRefit`, `RibbonLaw` — finite scalar admission, offset discriminants, and the ribbon carrier.
- [03]-[SHAPE_POLICY]: `CurveEdit`, `ExtendLaw`, `ShortenLaw`, `SplitCutter`, `PullTarget`, `ProjectTarget`, `CurveBooleanLaw`, `BlendLaw`, `ArcBlendLaw`, `TweenLaw`, `SpiralLaw`, `ParabolaSeed`, `AnalyticCurve`, `CatenaryLaw`, `TextFace`, `FitLaw`, `FilletRailDegree`, `FilletArcDegree`, `RailFilletLaw` — the modality vocabularies.
- [04]-[OPERATION_RAIL]: `CurveSlot`, `CurveOp`, and the `Curves.Build` entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[OFFSET_POLICY]

- Owner: `CurveScalar` admits every finite policy scalar once; `OffsetFrame` closes planar, fitted-normal, and loose-normal framing as explicit cases; `SurfaceOffsetTarget` closes each catalogued host-and-law pair; `SurfaceLift` closes normal and tangent lifting; `RibbonRefit` resolves refit tolerance from a behavior-bearing row; `RibbonLaw` carries the native ribbon policy as one value.
- Law: varying distances are admitted `CurveScalar` rows — the on-surface arm splits `(Parameter, Distance)` rows into the two parallel native arrays at the call, so finiteness and cardinality are proven by construction.
- Law: `RibbonLaw.Rig` is the one site naming `RibbonOffsetParameters` — offset distance, location, plane vector, blend radius, rebuild and refit knobs, cross-section alignment, and the `RibbonOffsetSurfaceMethod` row bake in one member with the tolerance slot reading the regime; the ribbon's rails, cross-sections, and breps cross as products behind per-class tallies.
- Law: an absent optional lowers to the host's own sentinel, never a zero — `BlendRadius` is `Option<CurveScalar>` mapped to `RhinoMath.UnsetValue` and `PlaneVector` is `Option<Vector3d>` mapped to `Vector3d.Unset`, so "no blend" and "a zero-radius blend" stay distinct requests instead of collapsing on a default-constructed slot.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct CurveScalar {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value)
            ? null
            : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"CurveScalar must be finite (got {value:R})."));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetFrame {
    private OffsetFrame() { }
    public sealed record InPlane(Plane Value, CurveOffsetCornerStyle Corner = CurveOffsetCornerStyle.Sharp) : OffsetFrame;
    public sealed record ByNormal(
        Point3d DirectionPoint, Vector3d Normal, CurveOffsetCornerStyle Corner = CurveOffsetCornerStyle.Sharp,
        CurveOffsetEndStyle End = CurveOffsetEndStyle.None) : OffsetFrame;
    public sealed record ByLooseNormal(
        Point3d DirectionPoint, Vector3d Normal, CurveOffsetCornerStyle Corner = CurveOffsetCornerStyle.Sharp,
        CurveOffsetEndStyle End = CurveOffsetEndStyle.None) : OffsetFrame;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceOffsetTarget {
    private SurfaceOffsetTarget() { }
    public sealed record FaceDistance(GeometryHandle Host, int Face, CurveScalar Distance) : SurfaceOffsetTarget;
    public sealed record FacePoint(GeometryHandle Host, int Face, Point2d Point) : SurfaceOffsetTarget;
    public sealed record FaceVarying(GeometryHandle Host, int Face, Seq<(CurveScalar Parameter, CurveScalar Distance)> Rows) : SurfaceOffsetTarget;
    public sealed record SurfaceDistance(GeometryHandle Host, CurveScalar Distance) : SurfaceOffsetTarget;
    public sealed record SurfacePoint(GeometryHandle Host, Point2d Point) : SurfaceOffsetTarget;

    internal Fin<Built<CurveSlot>> Apply(Curve curve, Context model, Op op) => Switch(
        state: (Curve: curve, Model: model, Op: op),
        faceDistance: static (ctx, target) => ModelGate.Borrow<Brep, Built<CurveSlot>>(target.Host, ctx.Op, brep =>
            from _ in guard(target.Face >= 0 && target.Face < brep.Faces.Count, ctx.Op.InvalidInput())
            from built in ModelGate.Many(ctx.Op, CurveSlot.OnSurface,
                () => ctx.Curve.OffsetOnSurface(brep.Faces[target.Face], target.Distance.Value, ctx.Model.Absolute.Value))
            select built),
        facePoint: static (ctx, target) => ModelGate.Borrow<Brep, Built<CurveSlot>>(target.Host, ctx.Op, brep =>
            from _ in guard(target.Face >= 0 && target.Face < brep.Faces.Count, ctx.Op.InvalidInput())
            from built in ModelGate.Many(ctx.Op, CurveSlot.OnSurface,
                () => ctx.Curve.OffsetOnSurface(brep.Faces[target.Face], target.Point, ctx.Model.Absolute.Value))
            select built),
        faceVarying: static (ctx, target) => ModelGate.Borrow<Brep, Built<CurveSlot>>(target.Host, ctx.Op, brep =>
            from _ in guard(target.Face >= 0 && target.Face < brep.Faces.Count && !target.Rows.IsEmpty, ctx.Op.InvalidInput())
            from built in ModelGate.Many(ctx.Op, CurveSlot.OnSurface, () => ctx.Curve.OffsetOnSurface(
                    face: brep.Faces[target.Face],
                    curveParameters: target.Rows.Map(static row => row.Parameter.Value).ToArray(),
                    offsetDistances: target.Rows.Map(static row => row.Distance.Value).ToArray(),
                    fittingTolerance: ctx.Model.Absolute.Value))
            select built),
        surfaceDistance: static (ctx, target) => ModelGate.Borrow<Surface, Built<CurveSlot>>(target.Host, ctx.Op, surface =>
            ModelGate.Many(ctx.Op, CurveSlot.OnSurface,
                () => ctx.Curve.OffsetOnSurface(surface, target.Distance.Value, ctx.Model.Absolute.Value))),
        surfacePoint: static (ctx, target) => ModelGate.Borrow<Surface, Built<CurveSlot>>(target.Host, ctx.Op, surface =>
            ModelGate.Many(ctx.Op, CurveSlot.OnSurface,
                () => ctx.Curve.OffsetOnSurface(surface, target.Point, ctx.Model.Absolute.Value))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceLift {
    private SurfaceLift() { }
    public sealed record Normal : SurfaceLift;
    public sealed record Tangent : SurfaceLift;
}

[SmartEnum<int>]
public sealed partial class RibbonRefit {
    public static readonly RibbonRefit None = new(key: 0, resolve: static _ => 0.0);
    public static readonly RibbonRefit AtTolerance = new(key: 1, resolve: static domain => domain.Absolute.Value);

    [UseDelegateFromConstructor]
    internal partial double Resolve(Context domain);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record RibbonLaw(
    CurveScalar Distance,
    Point3d Location,
    RibbonRefit Refit,
    Option<CurveScalar> BlendRadius = default,
    Option<Vector3d> PlaneVector = default,
    int RebuildPointCount = 0,
    bool AlignCrossSections = false,
    RibbonOffsetSurfaceMethod SurfaceMethod = RibbonOffsetSurfaceMethod.Sweep2) {
    internal Fin<RibbonOffsetParameters> Rig(Context domain, Op key) =>
        key.Catch(() => Fin.Succ(value: new RibbonOffsetParameters {
            OffsetDistance = Distance.Value,
            OffsetLocation = Location,
            OffsetTolerance = domain.Absolute.Value,
            OffsetPlaneVector3d = PlaneVector.IfNone(Vector3d.Unset),
            BlendRadius = BlendRadius.Map(static row => row.Value).IfNone(RhinoMath.UnsetValue),
            RebuildPointCount = RebuildPointCount,
            RefitTolerance = Refit.Resolve(domain),
            AlignCrossSections = AlignCrossSections,
            RibbonSurfaceGenerationMethod = SurfaceMethod,
        }));
}
```

## [03]-[SHAPE_POLICY]

- Owner: the modality vocabularies — `CurveBooleanLaw` carries exactly the source arity consumed by union, intersection, or difference; `ExtendLaw` fuses the extension side, style, and terminal; `ShortenLaw` fuses domain and end trimming; `SplitCutter` fuses parameter, brep, surface, and plane splitting; `PullTarget`/`ProjectTarget` fuse sources with their destinations; `BlendLaw` and `ArcBlendLaw` close blend construction; `TweenLaw` fuses plain, matched, and sampled tweening; `SpiralLaw`, `ParabolaSeed`, `AnalyticCurve`, and `CatenaryLaw` fuse the construction seed families; `FitLaw` carries the advanced `NurbsCurveFitParameters` surface as one policy value.
- Law: the discriminant is the value's shape — every native overload family resolves from the case the caller constructed, so no arm reads a mode flag and no verb family grows a `ByX` sibling.
- Law: `FitAxis` distinguishes fixed intensity from coefficient-bearing custom intensity; `FitCapability` carries orthogonal fit grants as set membership; generated `FitLaw` admits automatic, fixed, or variable control-point modes before `Rig` projects one native interpretation.
- Law: catenary construction is one case over four shape terminals — through-point, length, parameter, and apex select the native static, and the apex, parameter, length, and deviation out-channels land as facts on every form.
- Law: no native mode int crosses a case payload — `ArcDegree` and `ArcSlider` carry the non-rational arc-bezier degree and sliders off the folder's shared owners, `CurveCompatibility` carries the simplify method with its rebuild count, and `TextFace` folds the additive font-style bits out of a `FrozenSet`, so the empty face set IS the host's `0` Normal and no arm re-derives an encoding.
- Law: the rail fillet declares three host rosters the arc-bezier owners do not share — `FilletRailDegree` closes the rail direction at 3 or 5, `FilletArcDegree` closes the arc direction at 2 through 5 where 2 alone yields a rational surface, and the slider pair is a two-slot tuple of `ArcSlider` because the host takes exactly two; a `Seq` of sliders lets the wrong arity reach a native that reads two.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveEdit {
    private CurveEdit() { }
    public sealed record RemoveShort : CurveEdit;
    public sealed record CloseGap : CurveEdit;
    public sealed record TrimDomain(Interval Value) : CurveEdit;
}

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
    public sealed record ToFace(GeometryHandle Brep, int Face) : PullTarget;
    public sealed record ToFaceLoose(GeometryHandle Brep, int Face) : PullTarget;
    public sealed record ToMesh(GeometryHandle Mesh) : PullTarget;
    public sealed record ToMeshLoose(GeometryHandle Mesh) : PullTarget;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectTarget {
    private ProjectTarget() { }
    public sealed record ToBreps(Seq<GeometryHandle> Curves, Seq<GeometryHandle> Breps, Vector3d Direction) : ProjectTarget;
    public sealed record ToMeshes(Seq<GeometryHandle> Curves, Seq<GeometryHandle> Meshes, Vector3d Direction) : ProjectTarget;
    public sealed record ToPlane(GeometryHandle Curve, Plane Plane) : ProjectTarget;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveBooleanLaw {
    private CurveBooleanLaw() { }
    public sealed record Union(Seq<GeometryHandle> Curves) : CurveBooleanLaw;
    public sealed record Intersection(GeometryHandle First, GeometryHandle Second) : CurveBooleanLaw;
    public sealed record Difference(GeometryHandle First, Seq<GeometryHandle> Subtractors) : CurveBooleanLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlendLaw {
    private BlendLaw() { }
    public sealed record EndToEnd(BlendContinuity Continuity, Option<(double BulgeA, double BulgeB)> Bulge = default) : BlendLaw;
    public sealed record AtParameters(double T0, bool Reverse0, BlendContinuity Continuity0, double T1, bool Reverse1, BlendContinuity Continuity1) : BlendLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArcBlendLaw {
    private ArcBlendLaw() { }
    public sealed record ControlPointRatio(CurveScalar Ratio) : ArcBlendLaw;
    public sealed record LineArcRadius(CurveScalar Radius) : ArcBlendLaw;
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

[SmartEnum<int>]
public sealed partial class TextFace {
    public static readonly TextFace Bold = new(key: 1);
    public static readonly TextFace Italic = new(key: 2);

    // RhinoCommon reads `textStyle` additively — 0 Normal, 1 Bold, 2 Italic, "any number of the following" — so an
    // empty set IS Normal and this fold is the whole encoding; no `Normal` row exists to select beside a face.
    internal static int Native(FrozenSet<TextFace> faces) =>
        toSeq(faces).Fold(0, static (bits, face) => bits | face.Key);
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FitAxis {
    private FitAxis() { }
    public sealed record Preset(FitPreset Value) : FitAxis;
    public sealed record Custom(CurveScalar Coefficient) : FitAxis;

    internal (NurbsCurveFitParameters.Intensity Intensity, double Coefficient) Native() => Switch(
        preset: static value => (value.Value.Native, 0.0),
        custom: static value => (NurbsCurveFitParameters.Intensity.Custom, value.Coefficient.Value));
}

[SmartEnum<int>]
public sealed partial class FitPreset {
    public static readonly FitPreset None = new(key: 0, native: NurbsCurveFitParameters.Intensity.None);
    public static readonly FitPreset Low = new(key: 1, native: NurbsCurveFitParameters.Intensity.Low);
    public static readonly FitPreset Moderate = new(key: 2, native: NurbsCurveFitParameters.Intensity.Moderate);
    public static readonly FitPreset Medium = new(key: 3, native: NurbsCurveFitParameters.Intensity.Medium);
    public static readonly FitPreset High = new(key: 4, native: NurbsCurveFitParameters.Intensity.High);
    public static readonly FitPreset Extreme = new(key: 5, native: NurbsCurveFitParameters.Intensity.Extreme);

    public NurbsCurveFitParameters.Intensity Native { get; }
}

[SmartEnum<int>]
public sealed partial class FitCapability {
    public static readonly FitCapability SubDFriendly = new(key: 0);
    public static readonly FitCapability Closed = new(key: 1);
    public static readonly FitCapability TangentsAtKinks = new(key: 2);
    public static readonly FitCapability Optimize = new(key: 3);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct FitLaw {
    public FitAxis Smoothing { get; }
    public FitAxis Uniformity { get; }
    public FitAxis CurvatureBias { get; }
    public FrozenSet<FitCapability> Capabilities { get; }
    public NurbsCurveFitParameters.TangentMatch TangentMatching { get; }
    public NurbsCurveFitParameters.KinkSplit KinkSplitting { get; }
    public int Degree { get; }
    public int PointCount { get; }
    public IndexPair PointCountRange { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FitAxis smoothing,
        ref FitAxis uniformity,
        ref FitAxis curvatureBias,
        ref FrozenSet<FitCapability> capabilities,
        ref NurbsCurveFitParameters.TangentMatch tangentMatching,
        ref NurbsCurveFitParameters.KinkSplit kinkSplitting,
        ref int degree,
        ref int pointCount,
        ref IndexPair pointCountRange) {
        if (!Admits(
            smoothing, uniformity, curvatureBias, capabilities, tangentMatching, kinkSplitting,
            degree, pointCount, pointCountRange)) {
            validationError = new ValidationError("Curve fit policy is inconsistent.");
        }
    }

    internal bool Admissible => Admits(
        Smoothing, Uniformity, CurvatureBias, Capabilities, TangentMatching, KinkSplitting,
        Degree, PointCount, PointCountRange);

    internal Fin<NurbsCurveFitParameters> Rig(Context domain, Op key) =>
        from _ in guard(Admissible, key.InvalidInput()).ToFin()
        from rigged in key.Catch(() => (Smoothing.Native(), Uniformity.Native(), CurvatureBias.Native()) switch {
            var (smoothing, uniformity, curvature) => Fin.Succ(value: new NurbsCurveFitParameters {
                TangentMatching = TangentMatching, KinkSplitting = KinkSplitting,
                SmoothingIntensity = smoothing.Intensity,
                UniformityIntensity = uniformity.Intensity,
                CurvatureBiasIntensity = curvature.Intensity,
                Degree = Degree,
                PointCount = PointCount,
                KinkAngleRadians = domain.Angle.Value,
                SmoothingCoefficient = smoothing.Coefficient,
                UniformityCoefficient = uniformity.Coefficient,
                CurvatureBiasCoefficient = curvature.Coefficient,
                SubDFriendly = Capabilities.Contains(FitCapability.SubDFriendly),
                Closed = Capabilities.Contains(FitCapability.Closed),
                ApplyTangentMatchingAtKinks = Capabilities.Contains(FitCapability.TangentsAtKinks),
                OptimizeCurve = Capabilities.Contains(FitCapability.Optimize),
                PointCountRange = PointCountRange,
            }),
        })
        select rigged;

    private static bool Admits(
        FitAxis? smoothing,
        FitAxis? uniformity,
        FitAxis? curvatureBias,
        FrozenSet<FitCapability>? capabilities,
        NurbsCurveFitParameters.TangentMatch tangentMatching,
        NurbsCurveFitParameters.KinkSplit kinkSplitting,
        int degree,
        int pointCount,
        IndexPair pointCountRange) {
        bool automatic = pointCount == 0 && pointCountRange is { I: 0, J: 0 };
        bool fixedCount = pointCount > degree && pointCountRange is { I: 0, J: 0 };
        bool variableCount = pointCount > degree
            && pointCountRange.I == pointCount
            && pointCountRange.J >= pointCount;
        return Axis(smoothing) && Axis(uniformity) && Axis(curvatureBias)
            && capabilities is not null && !capabilities.Any(static capability => capability is null)
            && Enum.IsDefined(tangentMatching) && Enum.IsDefined(kinkSplitting)
            && degree >= 1 && (automatic || fixedCount || variableCount);
    }

    private static bool Axis(FitAxis? axis) => axis switch {
        FitAxis.Preset { Value: not null } => true,
        FitAxis.Custom => true,
        _ => false,
    };
}

[SmartEnum<int>]
public sealed partial class FilletRailDegree {
    public static readonly FilletRailDegree Cubic = new(key: 3);
    public static readonly FilletRailDegree Quintic = new(key: 5);
}

[SmartEnum<int>]
public sealed partial class FilletArcDegree {
    public static readonly FilletArcDegree Rational = new(key: 2);
    public static readonly FilletArcDegree Cubic = new(key: 3);
    public static readonly FilletArcDegree Quartic = new(key: 4);
    public static readonly FilletArcDegree Quintic = new(key: 5);
}

public sealed record RailFilletLaw(
    FilletRailDegree Rail,
    FilletArcDegree Arc,
    (ArcSlider Tangent, ArcSlider Inner) Sliders,
    int BezierSurfaceCount,
    bool Extend,
    FilletSurfaceSplitType Split);
```

## [04]-[OPERATION_RAIL]

- Owner: `CurveSlot` `[SmartEnum<int>]` — the consequence vocabulary; `CurveOp` `[Union]` — the whole verified curve host-op roster; `Curves` — the one entry folding any operation spread into one `Built<CurveSlot>`.
- Law: refinement is value-semantic — fair, fit, rebuild, smooth, and simplify run the instance member on the borrowed curve and own the returned refinement; the boolean tolerance-less and tween tolerance-less overloads are obsolete, so every arm runs the tolerance form off the regime.
- Law: correspondence maps survive — projection folds its curve and brep source indices, join folds its key map, curve-boolean union folds its index map, and the planar-region product folds its per-region partition as `SourceGroups`, so a consumer never re-derives which input produced which output. Mesh-target projection is the declared exception: `Curve.ProjectToMesh` exposes no index channel on any overload, so the `ToMeshes` arm records the product tally alone and carries no correspondence guarantee.
- Law: the boolean-regions carrier dies at the seam — `CurveBooleanRegions` is read inside the arm, its per-region curves cross as products partitioned by a `SourceGroups` fact, and point assignments carry the `SourceAxis.Region` discriminant.
- Law: `CurveOp.Admitted` closes the whole roster before dispatch — every handle, index, finite scalar, degree-versus-count relation, host enum, direction vector, and nested policy payload admits at the spine, so no arm reaches a native with an ungated numeric and the `Entry` fold reports the complete rejection set.
- Law: end reconciliation answers pairs — `MakeEndsMeet` duplicates both curves, reconciles the duplicates, and crosses both as products, so the mutating native never touches an input handle; the staged harvest rolls both duplicates back on the failure path, because a throwing or refusing native orphans two live copies otherwise.
- Law: compatibility seats once — `CurveCompatibility` is the lofting rail's `[ComplexValueObject]` over the simplify method and rebuild count, and the curve rail's `Compatible` case consumes that owner rather than re-spelling the same two `NurbsCurve.MakeCompatible` arguments as bare ints; the host documents `Point3d.Unset` as the omit spelling for either terminal, so admission accepts an unset endpoint and refuses only a NaN-bearing one.
- Law: direction agreement is evidence — `DirectionsMatch` borrows both curves and lands the host `Curve.DoDirectionsMatch` verdict as a `Flag` fact with no product, so join, sweep, and loft preparation reads the verdict off the receipt rail; the kernel's typed direction-relation vocabulary stays the analysis-altitude owner, and this host boolean serves construction preparation only.
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
    public static readonly CurveSlot Trimmed0 = new(key: 24);
    public static readonly CurveSlot Trimmed1 = new(key: 25);
    public static readonly CurveSlot DirectionVerdict = new(key: 26);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveOp {
    private CurveOp() { }
    public sealed record Offset(GeometryHandle Curve, OffsetFrame Frame, CurveScalar Distance) : CurveOp;
    public sealed record OffsetOnSurface(GeometryHandle Curve, SurfaceOffsetTarget Target) : CurveOp;
    public sealed record OffsetLift(GeometryHandle Curve, GeometryHandle Surface, CurveScalar Height, SurfaceLift Lift) : CurveOp;
    public sealed record Ribbon(GeometryHandle Curve, RibbonLaw Law) : CurveOp;
    public sealed record Fair(GeometryHandle Curve, int ClampStart, int ClampEnd, int Iterations) : CurveOp;
    public sealed record Fit(GeometryHandle Curve, int Degree) : CurveOp;
    public sealed record Rebuild(GeometryHandle Curve, int PointCount, int Degree, bool PreserveTangents) : CurveOp;
    public sealed record Smooth(GeometryHandle Curve, double Factor, bool X, bool Y, bool Z, bool FixBoundaries, SmoothingCoordinateSystem System, Option<Plane> Frame = default) : CurveOp;
    public sealed record Simplify(GeometryHandle Curve, CurveSimplifyOptions Options, Option<CurveEnd> EndOnly = default) : CurveOp;
    public sealed record Edit(GeometryHandle Curve, CurveEdit Verb) : CurveOp;
    public sealed record NurbsFit(GeometryHandle Curve, Interval Domain, FitLaw Law) : CurveOp;
    public sealed record Extend(GeometryHandle Curve, ExtendLaw Law) : CurveOp;
    public sealed record Shorten(GeometryHandle Curve, ShortenLaw Law) : CurveOp;
    public sealed record Split(GeometryHandle Curve, SplitCutter Cutter) : CurveOp;
    public sealed record Pull(GeometryHandle Curve, PullTarget Target) : CurveOp;
    public sealed record Project(ProjectTarget Target) : CurveOp;
    public sealed record Join(Seq<GeometryHandle> Curves, bool PreserveDirection = false, bool Simple = false) : CurveOp;
    public sealed record Boolean(CurveBooleanLaw Law) : CurveOp;
    public sealed record Regions(Seq<GeometryHandle> Curves, Plane Frame, Seq<Point3d> Points, bool Combine) : CurveOp;
    public sealed record Blend(GeometryHandle First, GeometryHandle Second, BlendLaw Law) : CurveOp;
    public sealed record ArcBlend(Point3d Start, Vector3d StartDirection, Point3d End, Vector3d EndDirection, ArcBlendLaw Law) : CurveOp;
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
    public sealed record SubDFriendlyPoints(Seq<Point3d> Points, bool Interpolate, bool PeriodicClosed) : CurveOp;
    public sealed record Compatible(Seq<GeometryHandle> Curves, Point3d Start, Point3d End, CurveCompatibility Law) : CurveOp;
    public sealed record Spiral(SpiralLaw Law, Point3d RadiusPoint, double Pitch, double TurnCount, double Radius0, double Radius1) : CurveOp;
    public sealed record Parabola(ParabolaSeed Seed) : CurveOp;
    public sealed record ArcBezier(ArcDegree Degree, Point3d Center, Point3d Start, Point3d End, double Radius, ArcSlider TanSlider, ArcSlider MidSlider) : CurveOp;
    public sealed record Analytic(AnalyticCurve Seed) : CurveOp;
    public sealed record Catenary(Point3d Start, Point3d End, Vector3d AxisDirection, CatenaryLaw Law, bool Smooth, int PointCount) : CurveOp;
    public sealed record MakeEndsMeet(GeometryHandle First, bool AdjustStartFirst, GeometryHandle Second, bool AdjustStartSecond) : CurveOp;
    public sealed record RailFillet(
        GeometryHandle Rail, GeometryHandle First, int FirstFace, GeometryHandle Second, int SecondFace,
        double U, double V, RailFilletLaw Law) : CurveOp;
    public sealed record TextOutlines(string Text, string Font, double Height, FrozenSet<TextFace> Faces, bool CloseLoops, Plane Frame, double SmallCapsScale = 1.0) : CurveOp;
    public sealed record DirectionsMatch(GeometryHandle First, GeometryHandle Second) : CurveOp;

    internal Fin<CurveOp> Admitted(Op key) =>
        guard(this switch {
            Offset row => Handle(row.Curve) && IsAdmitted(row.Frame),
            OffsetOnSurface row => Handle(row.Curve) && IsAdmitted(row.Target),
            OffsetLift row => Handle(row.Curve) && Handle(row.Surface) && row.Lift is not null,
            Ribbon row => Handle(row.Curve) && IsAdmitted(row.Law),
            Fair row => Handle(row.Curve) && Count(row.ClampStart) && Count(row.ClampEnd) && row.Iterations > 0,
            Fit row => Handle(row.Curve) && Degree(row.Degree),
            Rebuild row => Handle(row.Curve) && Degree(row.Degree) && row.PointCount > row.Degree,
            Smooth row => Handle(row.Curve) && Finite(row.Factor) && Enum.IsDefined(row.System)
                && row.Frame.ForAll(static frame => frame.IsValid),
            // `CurveSimplifyOptions` is a host flag mask, so every bit combination is legal material and only the
            // optional end selector carries a roster to gate.
            Simplify row => Handle(row.Curve) && row.EndOnly.ForAll(static side => Enum.IsDefined(side)),
            Edit row => Handle(row.Curve) && IsAdmitted(row.Verb),
            NurbsFit row => Handle(row.Curve) && row.Domain.IsValid && row.Law.Admissible,
            Extend row => Handle(row.Curve) && IsAdmitted(row.Law),
            Shorten row => Handle(row.Curve) && IsAdmitted(row.Law),
            Split row => Handle(row.Curve) && IsAdmitted(row.Cutter),
            Pull row => Handle(row.Curve) && IsAdmitted(row.Target),
            Project row => IsAdmitted(row.Target),
            Join row => Handles(row.Curves),
            Boolean row => IsAdmitted(row.Law),
            Regions row => Handles(row.Curves) && row.Frame.IsValid && Points(row.Points, allowEmpty: true),
            Blend row => Handle(row.First) && Handle(row.Second) && IsAdmitted(row.Law),
            ArcBlend row => row.Start.IsValid && Direction(row.StartDirection)
                && row.End.IsValid && Direction(row.EndDirection) && IsAdmitted(row.Law),
            FilletCurves row => Handle(row.First) && row.NearFirst.IsValid
                && Handle(row.Second) && row.NearSecond.IsValid && Positive(row.Radius),
            FilletCorners row => Handle(row.Curve) && Positive(row.Radius),
            Tween row => Handle(row.First) && Handle(row.Second) && row.Count > 0 && IsAdmitted(row.Law),
            MatchCurve row => Handle(row.First) && Handle(row.Second)
                && Enum.IsDefined(row.Continuity) && Enum.IsDefined(row.Preserve),
            Mean row => Handle(row.First) && Handle(row.Second),
            TwoView row => Handle(row.First) && Handle(row.Second)
                && Direction(row.FirstDirection) && Direction(row.SecondDirection),
            Interpolated row => Points(row.Points) && Degree(row.Degree) && row.Points.Count > row.Degree
                && row.Knots.ForAll(static knots => Enum.IsDefined(knots))
                && row.Tangents.ForAll(static ends => Direction(ends.Start) && Direction(ends.End)),
            ControlPoints row => Points(row.Points) && Degree(row.Degree) && row.Points.Count > row.Degree,
            FitPoints row => Points(row.Points)
                && row.Constrained.ForAll(static ends => Degree(ends.Degree)
                    && Direction(ends.Start) && Direction(ends.End)),
            HSpline row => Points(row.Points)
                && row.Tangents.ForAll(static ends => Direction(ends.Start) && Direction(ends.End)),
            SoftEdit row => Handle(row.Curve) && Finite(row.T) && row.Delta.IsValid && Positive(row.Length),
            PeriodicClose row => Handle(row.Curve),
            SubDFriendly row => Handle(row.Curve)
                && row.Structure.ForAll(static shape => shape.PointCount > 0),
            SubDFriendlyPoints row => Points(row.Points),
            Compatible row => Handles(row.Curves) && row.Law.Admissible
                && Terminal(row.Start) && Terminal(row.End),
            Spiral row => IsAdmitted(row.Law) && row.RadiusPoint.IsValid && Finite(row.Pitch)
                && Nonzero(row.TurnCount) && Nonnegative(row.Radius0) && Nonnegative(row.Radius1),
            Parabola row => IsAdmitted(row.Seed),
            ArcBezier row => row.Degree is not null && row.Center.IsValid
                && row.Start.IsValid && row.End.IsValid && Positive(row.Radius),
            Analytic row => IsAdmitted(row.Seed),
            Catenary row => row.Start.IsValid && row.End.IsValid && Direction(row.AxisDirection)
                && IsAdmitted(row.Law) && row.PointCount > 1,
            MakeEndsMeet row => Handle(row.First) && Handle(row.Second),
            RailFillet row => Handle(row.Rail) && Handle(row.First) && Index(row.FirstFace)
                && Handle(row.Second) && Index(row.SecondFace)
                && Finite(row.U) && Finite(row.V) && IsAdmitted(row.Law),
            TextOutlines row => row.Faces is not null && Positive(row.Height)
                && row.Frame.IsValid && Positive(row.SmallCapsScale),
            DirectionsMatch row => Handle(row.First) && Handle(row.Second),
            _ => false,
        }, key.InvalidInput()).ToFin().Map(_ => this);

    private static bool IsAdmitted(object? value) => value switch {
        OffsetFrame.InPlane row => row.Value.IsValid && Enum.IsDefined(row.Corner),
        OffsetFrame.ByNormal row => row.DirectionPoint.IsValid && Direction(row.Normal)
            && Enum.IsDefined(row.Corner) && Enum.IsDefined(row.End),
        OffsetFrame.ByLooseNormal row => row.DirectionPoint.IsValid && Direction(row.Normal)
            && Enum.IsDefined(row.Corner) && Enum.IsDefined(row.End),
        SurfaceOffsetTarget.FaceDistance row => Handle(row.Host) && Index(row.Face),
        SurfaceOffsetTarget.FacePoint row => Handle(row.Host) && Index(row.Face) && row.Point.IsValid,
        SurfaceOffsetTarget.FaceVarying row => Handle(row.Host) && Index(row.Face) && !row.Rows.IsEmpty,
        SurfaceOffsetTarget.SurfaceDistance row => Handle(row.Host),
        SurfaceOffsetTarget.SurfacePoint row => Handle(row.Host) && row.Point.IsValid,
        RibbonLaw row => row.Location.IsValid && row.Refit is not null && Count(row.RebuildPointCount)
            && row.PlaneVector.ForAll(static vector => Direction(vector))
            && Enum.IsDefined(row.SurfaceMethod),
        CurveEdit.RemoveShort or CurveEdit.CloseGap => true,
        CurveEdit.TrimDomain row => row.Value.IsValid,
        ExtendLaw.ByLength row => Enum.IsDefined(row.Side) && Positive(row.Length) && Enum.IsDefined(row.Style),
        ExtendLaw.ToGeometry row => Enum.IsDefined(row.Side) && Enum.IsDefined(row.Style) && Handles(row.Bounds),
        ExtendLaw.ToPoint row => Enum.IsDefined(row.Side) && Enum.IsDefined(row.Style) && row.Terminal.IsValid,
        ExtendLaw.ByLine row => Enum.IsDefined(row.Side) && Handles(row.Bounds),
        ExtendLaw.ByArc row => Enum.IsDefined(row.Side) && Handles(row.Bounds),
        ExtendLaw.OnSurface row => Enum.IsDefined(row.Side) && Handle(row.Target)
            && row.Face.ForAll(static face => Index(face)),
        ShortenLaw.ToDomain row => row.Value.IsValid,
        ShortenLaw.AtEnd row => Enum.IsDefined(row.Side) && Positive(row.Length),
        SplitCutter.AtParameters row => !row.Values.IsEmpty && row.Values.ForAll(static t => Finite(t)),
        SplitCutter.ByBrep row => Handle(row.Value),
        SplitCutter.BySurface row => Handle(row.Value),
        SplitCutter.ByPlane row => row.Value.IsValid,
        PullTarget.ToFace row => Handle(row.Brep) && Index(row.Face),
        PullTarget.ToFaceLoose row => Handle(row.Brep) && Index(row.Face),
        PullTarget.ToMesh row => Handle(row.Mesh),
        PullTarget.ToMeshLoose row => Handle(row.Mesh),
        ProjectTarget.ToBreps row => Handles(row.Curves) && Handles(row.Breps) && Direction(row.Direction),
        ProjectTarget.ToMeshes row => Handles(row.Curves) && Handles(row.Meshes) && Direction(row.Direction),
        ProjectTarget.ToPlane row => Handle(row.Curve) && row.Plane.IsValid,
        CurveBooleanLaw.Union row => Handles(row.Curves),
        CurveBooleanLaw.Intersection row => Handle(row.First) && Handle(row.Second),
        CurveBooleanLaw.Difference row => Handle(row.First) && Handles(row.Subtractors),
        BlendLaw.EndToEnd row => Enum.IsDefined(row.Continuity)
            && row.Bulge.ForAll(static bulge => Finite(bulge.BulgeA) && Finite(bulge.BulgeB)),
        BlendLaw.AtParameters row => Finite(row.T0) && Enum.IsDefined(row.Continuity0)
            && Finite(row.T1) && Enum.IsDefined(row.Continuity1),
        ArcBlendLaw.ControlPointRatio row => row.Ratio.Value > 0.0,
        ArcBlendLaw.LineArcRadius row => row.Radius.Value > 0.0,
        TweenLaw.Plain or TweenLaw.Matched => true,
        TweenLaw.Sampled row => row.Samples > 0,
        SpiralLaw.AboutAxis row => row.AxisStart.IsValid && Direction(row.AxisDirection),
        SpiralLaw.AlongRail row => Handle(row.Rail) && Finite(row.T0) && Finite(row.T1) && row.PointsPerTurn > 0,
        ParabolaSeed.FromVertex row => row.Vertex.IsValid && row.Start.IsValid && row.End.IsValid,
        ParabolaSeed.FromFocus row => row.Focus.IsValid && row.Start.IsValid && row.End.IsValid,
        ParabolaSeed.FromPoints row => row.Start.IsValid && row.Inner.IsValid && row.End.IsValid,
        AnalyticCurve.OfLine row => row.Value.IsValid,
        AnalyticCurve.OfArc row => row.Value.IsValid && Structure(row.Structure),
        AnalyticCurve.OfCircle row => row.Value.IsValid && Structure(row.Structure),
        AnalyticCurve.OfEllipse row => row.Value.IsValid,
        CatenaryLaw.ThroughPoint row => row.Value.IsValid,
        CatenaryLaw.FromLength row => Positive(row.Value),
        CatenaryLaw.FromParameter row => Finite(row.Value),
        CatenaryLaw.FromApex row => row.Value.IsValid,
        // `numBezierSrfs` is documented "if > 0, …", so zero is the legal do-not-subdivide value, never a floor breach.
        RailFilletLaw row => row.Rail is not null && row.Arc is not null
            && Count(row.BezierSurfaceCount) && Enum.IsDefined(row.Split),
        _ => false,
    };

    private static bool Handle(GeometryHandle? handle) => handle is not null;
    private static bool Handles(Seq<GeometryHandle> handles, bool allowEmpty = false) =>
        (allowEmpty || !handles.IsEmpty) && handles.ForAll(static handle => handle is not null);
    private static bool Points(Seq<Point3d> points, bool allowEmpty = false) =>
        (allowEmpty || !points.IsEmpty) && points.ForAll(static point => point.IsValid);
    private static bool Index(int value) => value >= 0;
    private static bool Count(int value) => value >= 0;
    private static bool Degree(int value) => value >= 1;
    private static bool Finite(double value) => double.IsFinite(value);
    private static bool Nonzero(double value) => Finite(value) && value != 0.0;
    private static bool Positive(double value) => Finite(value) && value > 0.0;
    private static bool Nonnegative(double value) => Finite(value) && value >= 0.0;
    private static bool Direction(Vector3d value) => value.IsValid && !value.IsZero;
    // `MakeCompatible` documents `Point3d.Unset` as the omit spelling for either terminal, so an unset point is
    // admitted material here and only a NaN-bearing point refuses.
    private static bool Terminal(Point3d value) => value.IsValid || value == Point3d.Unset;
    private static bool Structure(Option<(int Degree, int CvCount)> structure) =>
        structure.ForAll(static shape => Degree(shape.Degree) && shape.CvCount > shape.Degree);

    internal Fin<Built<CurveSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            offset: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Offset)), (curve, op) =>
                edit.Frame.Switch(
                    state: (Curve: curve, Model: model, Edit: edit, Op: op),
                    inPlane: static (ctx, frame) => ModelGate.Many(ctx.Op, CurveSlot.Offset, () => ctx.Curve.Offset(
                        plane: frame.Value, distance: ctx.Edit.Distance.Value, tolerance: ctx.Model.Absolute.Value, cornerStyle: frame.Corner)),
                    byNormal: static (ctx, frame) => OffsetNormal(
                        context: ctx, directionPoint: frame.DirectionPoint, normal: frame.Normal,
                        corner: frame.Corner, end: frame.End, loose: false),
                    byLooseNormal: static (ctx, frame) => OffsetNormal(
                        context: ctx, directionPoint: frame.DirectionPoint, normal: frame.Normal,
                        corner: frame.Corner, end: frame.End, loose: true))),
            offsetOnSurface: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(OffsetOnSurface)),
                (curve, op) => edit.Target.Apply(curve, model, op)),
            offsetLift: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(OffsetLift)), (curve, op) =>
                ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: edit.Surface, key: op, body: surface =>
                    edit.Lift.Switch(
                        state: (Curve: curve, Surface: surface, Edit: edit, Op: op),
                        normal: static ctx => ModelGate.Single(ctx.Op, CurveSlot.Lifted, () => ctx.Curve.OffsetNormalToSurface(
                            surface: ctx.Surface, height: ctx.Edit.Height.Value)),
                        tangent: static ctx => ModelGate.Single(ctx.Op, CurveSlot.Lifted, () => ctx.Curve.OffsetTangentToSurface(
                            surface: ctx.Surface, height: ctx.Edit.Height.Value))))),
            ribbon: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Ribbon)), (curve, op) =>
                from parameters in edit.Law.Rig(domain: model, key: op)
                from built in op.Catch(() => {
                    Curve ribbon = curve.RibbonOffset(
                        ribbonParameters: parameters, railCurves: out Curve[] rails,
                        crossSectionCurves: out Curve[] sections, brepSurfaces: out Brep[] breps);
                    return ModelGate.Staged(op: op,
                        (CurveSlot.Ribboned, (GeometryBase[])[ribbon], false),
                        (CurveSlot.Rails, rails, true),
                        (CurveSlot.Sections, sections, true),
                        (CurveSlot.Surfaced, breps, true));
                })
                select built),
            fair: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Fair)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => curve.Fair(
                    distanceTolerance: model.Absolute.Value, angleTolerance: model.Angle.Value,
                    clampStart: edit.ClampStart, clampEnd: edit.ClampEnd, iterations: edit.Iterations))),
            fit: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Fit)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => curve.Fit(degree: edit.Degree, fitTolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))),
            rebuild: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Rebuild)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => curve.Rebuild(pointCount: edit.PointCount, degree: edit.Degree, preserveTangents: edit.PreserveTangents))),
            smooth: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Smooth)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => edit.Frame.Case switch {
                    Plane frame => curve.Smooth(
                        smoothFactor: edit.Factor, bXSmooth: edit.X, bYSmooth: edit.Y, bZSmooth: edit.Z,
                        bFixBoundaries: edit.FixBoundaries, coordinateSystem: edit.System, plane: frame),
                    _ => curve.Smooth(
                        smoothFactor: edit.Factor, bXSmooth: edit.X, bYSmooth: edit.Y, bZSmooth: edit.Z,
                        bFixBoundaries: edit.FixBoundaries, coordinateSystem: edit.System),
                })),
            simplify: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Simplify)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => edit.EndOnly.Case switch {
                    CurveEnd end => curve.SimplifyEnd(end: end, options: edit.Options,
                        distanceTolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value),
                    _ => curve.Simplify(options: edit.Options,
                        distanceTolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value),
                })),
            edit: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Edit)), (curve, op) =>
                op.Catch(() => {
                    Curve working = (Curve)curve.Duplicate();
                    Fin<Unit> changed = edit.Verb.Switch(
                        state: (Working: working, Domain: model, Op: op),
                        removeShort: static ctx => ctx.Op.Confirm(success: ctx.Working.RemoveShortSegments(tolerance: ctx.Domain.Absolute.Value)),
                        closeGap: static ctx => ctx.Op.Confirm(success: ctx.Working.MakeClosed(tolerance: ctx.Domain.Absolute.Value)),
                        trimDomain: static (ctx, law) => ctx.Op.Confirm(success: ctx.Working.TrimInterval(domain: law.Value)));
                    return changed.Bind(_ => ModelGate.Kept(op, CurveSlot.Refined, working)).Rollback(working);
                })),
            nurbsFit: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(NurbsFit)), (curve, op) =>
                edit.Law.Rig(domain: model, key: op).Bind(parameters => {
                    using (parameters) {
                        return op.Catch(() => {
                            NurbsCurve fitted = Curve.CreateNurbsCurveFit(
                                curve: curve, domain: edit.Domain, rebuildOptions: parameters,
                                maximumSeparation: out Line separation, thisSeparationParameter: out double atSource, nurbsSeparationParameter: out double atFit);
                            return ModelGate.Own(built: fitted, key: op).Map(owned => Built<CurveSlot>.Of(operation: op,
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
                edit.Law.Switch(
                    state: (Curve: curve, Op: op),
                    byLength: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.Extend(
                        side: law.Side, length: law.Length, style: law.Style)),
                    toGeometry: static (ctx, law) => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: ctx.Op,
                        body: bounds => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.Extend(
                            side: law.Side, style: law.Style, geometry: bounds.AsIterable()))),
                    toPoint: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.Extend(
                        side: law.Side, style: law.Style, endPoint: law.Terminal)),
                    byLine: static (ctx, law) => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: ctx.Op,
                        body: bounds => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.ExtendByLine(
                            side: law.Side, geometry: bounds.AsIterable()))),
                    byArc: static (ctx, law) => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: ctx.Op,
                        body: bounds => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.ExtendByArc(
                            side: law.Side, geometry: bounds.AsIterable()))),
                    onSurface: static (ctx, law) => law.Face.Case switch {
                        int face => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Target, key: ctx.Op, body: host =>
                            from _ in guard(face >= 0 && face < host.Faces.Count, ctx.Op.InvalidInput())
                            from built in ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.ExtendOnSurface(side: law.Side, face: host.Faces[face]))
                            select built),
                        _ => ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: law.Target, key: ctx.Op,
                            body: host => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.ExtendOnSurface(side: law.Side, surface: host))),
                    })),
            shorten: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Shorten)), (curve, op) =>
                edit.Law.Switch(
                    state: (Curve: curve, Op: op),
                    toDomain: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Shortened, () => ctx.Curve.Trim(domain: law.Value)),
                    atEnd: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Shortened, () => ctx.Curve.Trim(side: law.Side, length: law.Length)))),
            split: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Split)), (curve, op) =>
                edit.Cutter.Switch(
                    state: (Curve: curve, Model: model, Op: op),
                    atParameters: static (ctx, law) => ModelGate.Many(ctx.Op, CurveSlot.SplitApart, () => ctx.Curve.Split(t: law.Values.AsIterable())),
                    byBrep: static (ctx, law) => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Value, key: ctx.Op, body: cutter =>
                        ModelGate.Many(ctx.Op, CurveSlot.SplitApart, () => ctx.Curve.Split(
                            cutter: cutter, tolerance: ctx.Model.Absolute.Value, angleToleranceRadians: ctx.Model.Angle.Value))),
                    bySurface: static (ctx, law) => ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: law.Value, key: ctx.Op, body: cutter =>
                        ModelGate.Many(ctx.Op, CurveSlot.SplitApart, () => ctx.Curve.Split(
                            cutter: cutter, tolerance: ctx.Model.Absolute.Value, angleToleranceRadians: ctx.Model.Angle.Value))),
                    byPlane: static (ctx, law) => ModelGate.Many(ctx.Op, CurveSlot.SplitApart, () => ctx.Curve.Split(
                        plane: law.Value, tolerance: ctx.Model.Absolute.Value, angleToleranceRadians: ctx.Model.Angle.Value)))),
            pull: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(Pull)), (curve, op) =>
                edit.Target.Switch(
                    state: (Curve: curve, Model: model, Op: op),
                    toFace: static (ctx, law) => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Brep, key: ctx.Op, body: host =>
                        from _ in guard(law.Face >= 0 && law.Face < host.Faces.Count, ctx.Op.InvalidInput())
                        from built in ModelGate.Many(ctx.Op, CurveSlot.Pulled, () => ctx.Curve.PullToBrepFace(
                            face: host.Faces[law.Face], tolerance: ctx.Model.Absolute.Value))
                        select built),
                    toFaceLoose: static (ctx, law) => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Brep, key: ctx.Op, body: host =>
                        from _ in guard(law.Face >= 0 && law.Face < host.Faces.Count, ctx.Op.InvalidInput())
                        from built in ModelGate.Many(ctx.Op, CurveSlot.Pulled, () => Curve.PullToBrepFace(
                            curve: ctx.Curve, face: host.Faces[law.Face], tolerance: ctx.Model.Absolute.Value, loose: true))
                        select built),
                    toMesh: static (ctx, law) => ModelGate.Borrow<Mesh, Built<CurveSlot>>(handle: law.Mesh, key: ctx.Op, body: mesh =>
                        ModelGate.Single(ctx.Op, CurveSlot.Pulled, () => ctx.Curve.PullToMesh(mesh: mesh, tolerance: ctx.Model.Absolute.Value))),
                    toMeshLoose: static (ctx, law) => ModelGate.Borrow<Mesh, Built<CurveSlot>>(handle: law.Mesh, key: ctx.Op, body: mesh =>
                        ModelGate.Single(ctx.Op, CurveSlot.Pulled, () => ctx.Curve.PullToMesh(
                            mesh: mesh, tolerance: ctx.Model.Absolute.Value, loose: true))))),
            project: static (model, edit) => {
                Op op = Op.Of(name: nameof(Project));
                return edit.Target.Switch(
                    state: (Model: model, Op: op),
                    toBreps: static (ctx, law) => ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: law.Curves, key: ctx.Op, body: curves =>
                        ModelGate.BorrowMany<Brep, Built<CurveSlot>>(handles: law.Breps, key: ctx.Op, body: breps =>
                            ctx.Op.Catch(() => {
                                Curve[] projected = Curve.ProjectToBrep(
                                    curves: curves.AsIterable(), breps: breps.AsIterable(), direction: law.Direction,
                                    tolerance: ctx.Model.Absolute.Value,
                                    curveIndices: out int[] curveMap, brepIndices: out int[] brepMap);
                                return ModelGate.Mapped(
                                    op: ctx.Op,
                                    slot: CurveSlot.Projected,
                                    built: projected,
                                    mapLength: projected.Length,
                                    maps: [(SourceAxis.Curve, curveMap), (SourceAxis.Brep, brepMap)]);
                            }))),
                    toMeshes: static (ctx, law) => ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: law.Curves, key: ctx.Op, body: curves =>
                        ModelGate.BorrowMany<Mesh, Built<CurveSlot>>(handles: law.Meshes, key: ctx.Op, body: meshes =>
                            ModelGate.Many(ctx.Op, CurveSlot.Projected, () => Curve.ProjectToMesh(
                                curves: curves.AsIterable(), meshes: meshes.AsIterable(), direction: law.Direction,
                                tolerance: ctx.Model.Absolute.Value)))),
                    toPlane: static (ctx, law) => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.Curve, key: ctx.Op, body: curve =>
                        ModelGate.Single(ctx.Op, CurveSlot.Projected, () => Curve.ProjectToPlane(curve: curve, plane: law.Plane))));
            },
            join: static (model, edit) => {
                Op op = Op.Of(name: nameof(Join));
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Curves, key: op, body: curves =>
                    op.Catch(() => {
                        Curve[] joined = Curve.JoinCurves(
                            inputCurves: curves.AsIterable(), joinTolerance: model.Absolute.Value,
                            preserveDirection: edit.PreserveDirection, simpleJoin: edit.Simple, key: out int[] map);
                        return ModelGate.Mapped(
                            op: op, slot: CurveSlot.Joined, built: joined, mapLength: curves.Count,
                            maps: [(SourceAxis.Input, map)]);
                    }));
            },
            boolean: static (model, edit) => {
                Op op = Op.Of(name: nameof(Boolean));
                return edit.Law.Switch(
                    state: (Model: model, Op: op),
                    union: static (ctx, law) => ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: law.Curves, key: ctx.Op, body: curves =>
                        ctx.Op.Catch(() => {
                            Curve[] fused = Curve.CreateBooleanUnion(
                                curves: curves.AsIterable(), tolerance: ctx.Model.Absolute.Value, indexMap: out int[] map);
                            return ModelGate.Mapped(
                                op: ctx.Op, slot: CurveSlot.Booled, built: fused, mapLength: curves.Count,
                                maps: [(SourceAxis.Input, map)]);
                        })),
                    intersection: static (ctx, law) => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.First, key: ctx.Op, body: first =>
                        ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.Second, key: ctx.Op, body: second =>
                            ModelGate.Many(ctx.Op, CurveSlot.Booled, () => Curve.CreateBooleanIntersection(
                                curveA: first, curveB: second, tolerance: ctx.Model.Absolute.Value)))),
                    difference: static (ctx, law) => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.First, key: ctx.Op, body: first =>
                        ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: law.Subtractors, key: ctx.Op, body: subtractors =>
                            ModelGate.Many(ctx.Op, CurveSlot.Booled, () => Curve.CreateBooleanDifference(
                                curveA: first, subtractors: subtractors.AsIterable(), tolerance: ctx.Model.Absolute.Value)))));
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
                                Seq<(int Region, int Boundary, int Segment, int PlanarCurve, Interval Domain, bool Reversed)> segments =
                                    toSeq(Enumerable.Range(start: 0, count: live.RegionCount)).Bind(region =>
                                        toSeq(Enumerable.Range(start: 0, count: live.BoundaryCount(region))).Bind(boundary =>
                                            toSeq(Enumerable.Range(start: 0, count: live.SegmentCount(region, boundary))).Map(segment => {
                                                int planarCurve = live.SegmentDetails(region, boundary, segment, out Interval domain, out bool reversed);
                                                return (Region: region, Boundary: boundary, Segment: segment, PlanarCurve: planarCurve, Domain: domain, Reversed: reversed);
                                            })));
                                Seq<Seq<int>> groups = regions.Fold(
                                    state: (Offset: 0, Groups: Seq<Seq<int>>()),
                                    folder: static (state, region) => (
                                        Offset: state.Offset + region.Count,
                                        Groups: state.Groups + Seq(toSeq(Enumerable.Range(start: state.Offset, count: region.Count)))))
                                    .Groups;
                                return ModelGate.OwnMany(built: regions.Bind(static region => region), key: op).Map(owned => Built<CurveSlot>.Of(operation: op,
                                    Products: owned,
                                    Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Regions, body: new BuildBody.SourceGroups(
                                        Axis: SourceAxis.Region, Groups: groups))
                                        + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Regions, body: new BuildBody.SourceMap(Axis: SourceAxis.Region, Rows: pointRegions))
                                        + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Regions, body: new BuildBody.RegionSegments(Rows: segments))));
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
                            endToEnd: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Blended, () => law.Bulge.Case switch {
                                (double bulgeA, double bulgeB) => Curve.CreateBlendCurve(
                                    curveA: ctx.First, curveB: ctx.Second, continuity: law.Continuity, bulgeA: bulgeA, bulgeB: bulgeB),
                                _ => Curve.CreateBlendCurve(curveA: ctx.First, curveB: ctx.Second, continuity: law.Continuity),
                            }),
                            atParameters: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Blended, () => Curve.CreateBlendCurve(
                                curve0: ctx.First, t0: law.T0, reverse0: law.Reverse0, continuity0: law.Continuity0,
                                curve1: ctx.Second, t1: law.T1, reverse1: law.Reverse1, continuity1: law.Continuity1)))));
            },
            arcBlend: static (_, edit) => {
                Op op = Op.Of(name: nameof(ArcBlend));
                return edit.Law.Switch(
                    state: (Edit: edit, Op: op),
                    controlPointRatio: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Blended, () => Curve.CreateArcBlend(
                        startPt: ctx.Edit.Start, startDir: ctx.Edit.StartDirection, endPt: ctx.Edit.End,
                        endDir: ctx.Edit.EndDirection, controlPointLengthRatio: law.Ratio.Value)),
                    lineArcRadius: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Blended, () => Curve.CreateArcLineArcBlend(
                        startPt: ctx.Edit.Start, startDir: ctx.Edit.StartDirection, endPt: ctx.Edit.End,
                        endDir: ctx.Edit.EndDirection, radius: law.Radius.Value)));
            },
            filletCurves: static (model, edit) => {
                Op op = Op.Of(name: nameof(FilletCurves));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Many(op, CurveSlot.Filleted, () => Curve.CreateFilletCurves(
                            curve0: first, point0: edit.NearFirst, curve1: second, point1: edit.NearSecond,
                            radius: edit.Radius, join: edit.JoinResult, trim: edit.TrimInputs, arcExtension: edit.ArcExtension,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))));
            },
            filletCorners: static (model, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(FilletCorners)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Filleted, () => Curve.CreateFilletCornersCurve(
                    curve: curve, radius: edit.Radius, tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))),
            tween: static (model, edit) => {
                Op op = Op.Of(name: nameof(Tween));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        edit.Law.Switch(
                            state: (First: first, Second: second, Edit: edit, Model: model, Op: op),
                            plain: static ctx => ModelGate.Many(ctx.Op, CurveSlot.Tweened, () => Curve.CreateTweenCurves(
                                curve0: ctx.First, curve1: ctx.Second, numCurves: ctx.Edit.Count, tolerance: ctx.Model.Absolute.Value)),
                            matched: static ctx => ModelGate.Many(ctx.Op, CurveSlot.Tweened, () => Curve.CreateTweenCurvesWithMatching(
                                curve0: ctx.First, curve1: ctx.Second, numCurves: ctx.Edit.Count, tolerance: ctx.Model.Absolute.Value)),
                            sampled: static (ctx, law) => ModelGate.Many(ctx.Op, CurveSlot.Tweened, () => Curve.CreateTweenCurvesWithSampling(
                                curve0: ctx.First, curve1: ctx.Second, numCurves: ctx.Edit.Count,
                                numSamples: law.Samples, tolerance: ctx.Model.Absolute.Value)))));
            },
            matchCurve: static (_, edit) => {
                Op op = Op.Of(name: nameof(MatchCurve));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Many(op, CurveSlot.Matched, () => Curve.CreateMatchCurve(
                            curve0: first, reverse0: edit.ReverseFirst, continuity: edit.Continuity,
                            curve1: second, reverse1: edit.ReverseSecond, preserve: edit.Preserve, average: edit.Average))));
            },
            mean: static (model, edit) => {
                Op op = Op.Of(name: nameof(Mean));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Single(op, CurveSlot.Constructed, () => Curve.CreateMeanCurve(curveA: first, curveB: second, angleToleranceRadians: model.Angle.Value))));
            },
            twoView: static (model, edit) => {
                Op op = Op.Of(name: nameof(TwoView));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Many(op, CurveSlot.Constructed, () => Curve.CreateCurve2View(
                            curveA: first, curveB: second, vectorA: edit.FirstDirection, vectorB: edit.SecondDirection,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))));
            },
            interpolated: static (_, edit) => {
                Op op = Op.Of(name: nameof(Interpolated));
                return ModelGate.Single(op, CurveSlot.Constructed, () => (edit.Knots.Case, edit.Tangents.Case) switch {
                    (CurveKnotStyle knots, (Vector3d start, Vector3d end)) => Curve.CreateInterpolatedCurve(
                        points: edit.Points.AsIterable(), degree: edit.Degree, knots: knots, startTangent: start, endTangent: end),
                    (CurveKnotStyle knots, _) => Curve.CreateInterpolatedCurve(points: edit.Points.AsIterable(), degree: edit.Degree, knots: knots),
                    _ => Curve.CreateInterpolatedCurve(points: edit.Points.AsIterable(), degree: edit.Degree),
                });
            },
            controlPoints: static (_, edit) => {
                Op op = Op.Of(name: nameof(ControlPoints));
                return ModelGate.Single(op, CurveSlot.Constructed, () => Curve.CreateControlPointCurve(points: edit.Points.AsIterable(), degree: edit.Degree));
            },
            fitPoints: static (model, edit) => {
                Op op = Op.Of(name: nameof(FitPoints));
                return ModelGate.Single(op, CurveSlot.Constructed, () => edit.Constrained.Case switch {
                    (int degree, Vector3d start, Vector3d end) => NurbsCurve.CreateFromFitPoints(
                        points: edit.Points.AsIterable(), tolerance: model.Absolute.Value, degree: degree,
                        periodic: edit.Periodic, startTangent: start, endTangent: end),
                    _ => NurbsCurve.CreateFromFitPoints(points: edit.Points.AsIterable(), tolerance: model.Absolute.Value, periodic: edit.Periodic),
                });
            },
            hSpline: static (_, edit) => {
                Op op = Op.Of(name: nameof(HSpline));
                return ModelGate.Single(op, CurveSlot.Constructed, () => edit.Tangents.Case switch {
                    (Vector3d start, Vector3d end) => NurbsCurve.CreateHSpline(points: edit.Points.AsIterable(), startTangent: start, endTangent: end),
                    _ => NurbsCurve.CreateHSpline(points: edit.Points.AsIterable()),
                });
            },
            softEdit: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(SoftEdit)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => Curve.CreateSoftEditCurve(
                    curve: curve, t: edit.T, delta: edit.Delta, length: edit.Length, fixEnds: edit.FixEnds))),
            periodicClose: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(PeriodicClose)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => Curve.CreatePeriodicCurve(curve: curve, smooth: edit.Smooth))),
            subDFriendly: static (_, edit) => Borrowed(edit.Curve, Op.Of(name: nameof(SubDFriendly)), (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => edit.Structure.Case switch {
                    (int points, bool periodic) => NurbsCurve.CreateSubDFriendly(curve: curve, pointCount: points, periodicClosedCurve: periodic),
                    _ => NurbsCurve.CreateSubDFriendly(curve: curve),
                })),
            subDFriendlyPoints: static (_, edit) => {
                Op op = Op.Of(name: nameof(SubDFriendlyPoints));
                return ModelGate.Single(op, CurveSlot.Refined, () => NurbsCurve.CreateSubDFriendly(
                    points: edit.Points.AsIterable(), interpolatePoints: edit.Interpolate, periodicClosedCurve: edit.PeriodicClosed));
            },
            compatible: static (model, edit) => {
                Op op = Op.Of(name: nameof(Compatible));
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Curves, key: op, body: curves =>
                    ModelGate.Many(op, CurveSlot.Refined, () => NurbsCurve.MakeCompatible(
                        curves: curves.AsIterable(), startPt: edit.Start, endPt: edit.End,
                        simplifyMethod: edit.Law.SimplifyMethod, numPoints: edit.Law.PointCount,
                        refitTolerance: model.Absolute.Value, angleTolerance: model.Angle.Value)));
            },
            spiral: static (_, edit) => {
                Op op = Op.Of(name: nameof(Spiral));
                return edit.Law.Switch(
                    state: (Edit: edit, Op: op),
                    aboutAxis: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Constructed, () => NurbsCurve.CreateSpiral(
                        axisStart: law.AxisStart, axisDir: law.AxisDirection, radiusPoint: ctx.Edit.RadiusPoint,
                        pitch: ctx.Edit.Pitch, turnCount: ctx.Edit.TurnCount, radius0: ctx.Edit.Radius0, radius1: ctx.Edit.Radius1)),
                    alongRail: static (ctx, law) => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.Rail, key: ctx.Op, body: rail =>
                        ModelGate.Single(ctx.Op, CurveSlot.Constructed, () => NurbsCurve.CreateSpiral(
                            railCurve: rail, t0: law.T0, t1: law.T1, radiusPoint: ctx.Edit.RadiusPoint,
                            pitch: ctx.Edit.Pitch, turnCount: ctx.Edit.TurnCount, radius0: ctx.Edit.Radius0,
                            radius1: ctx.Edit.Radius1, pointsPerTurn: law.PointsPerTurn))));
            },
            parabola: static (_, edit) => {
                Op op = Op.Of(name: nameof(Parabola));
                return edit.Seed.Switch(
                    state: op,
                    fromVertex: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromVertex(
                        vertex: seed.Vertex, startPoint: seed.Start, endPoint: seed.End)),
                    fromFocus: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromFocus(
                        focus: seed.Focus, startPoint: seed.Start, endPoint: seed.End)),
                    fromPoints: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromPoints(
                        startPoint: seed.Start, innerPoint: seed.Inner, endPoint: seed.End)));
            },
            arcBezier: static (_, edit) => {
                Op op = Op.Of(name: nameof(ArcBezier));
                return ModelGate.Single(op, CurveSlot.Constructed, () => NurbsCurve.CreateNonRationalArcBezier(
                    degree: edit.Degree.Key, center: edit.Center, start: edit.Start, end: edit.End,
                    radius: edit.Radius, tanSlider: edit.TanSlider.Value, midSlider: edit.MidSlider.Value));
            },
            analytic: static (_, edit) => {
                Op op = Op.Of(name: nameof(Analytic));
                return edit.Seed.Switch(
                    state: op,
                    ofLine: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateFromLine(line: seed.Value)),
                    ofArc: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => seed.Structure.Case switch {
                        (int degree, int cvCount) => NurbsCurve.CreateFromArc(arc: seed.Value, degree: degree, cvCount: cvCount),
                        _ => NurbsCurve.CreateFromArc(arc: seed.Value),
                    }),
                    ofCircle: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => seed.Structure.Case switch {
                        (int degree, int cvCount) => NurbsCurve.CreateFromCircle(circle: seed.Value, degree: degree, cvCount: cvCount),
                        _ => NurbsCurve.CreateFromCircle(circle: seed.Value),
                    }),
                    ofEllipse: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateFromEllipse(ellipse: seed.Value)));
            },
            catenary: static (_, edit) => {
                Op op = Op.Of(name: nameof(Catenary));
                return op.Catch(() => {
                    (Curve Hung, Point3d Apex, double Parameter, double Length, double Deviation) result = edit.Law.Switch(
                        state: edit,
                        throughPoint: static (request, law) => Hung(native: Curve.CreateCatenaryCurveThroughPoint, request: request, shape: law.Value),
                        fromLength: static (request, law) => Hung(native: Curve.CreateCatenaryCurveFromLength, request: request, shape: law.Value),
                        fromParameter: static (request, law) => Hung(native: Curve.CreateCatenaryCurveFromParameter, request: request, shape: law.Value),
                        fromApex: static (request, law) => Hung(native: Curve.CreateCatenaryCurveFromApex, request: request, shape: law.Value));
                    return ModelGate.Own(built: result.Hung, key: op).Map(owned => Built<CurveSlot>.Of(operation: op,
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
                            return ModelGate.Staged(op: op, success: Curve.MakeEndsMeet(
                                curveA: workingFirst, adjustStartCurveA: edit.AdjustStartFirst,
                                curveB: workingSecond, adjustStartCurveB: edit.AdjustStartSecond),
                                (CurveSlot.Reconciled, (GeometryBase[])[workingFirst, workingSecond], false))
                                .Rollback(workingFirst, workingSecond);
                        })));
            },
            railFillet: static (model, edit) => {
                Op op = Op.Of(name: nameof(RailFillet));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Rail, key: op, body: rail =>
                    ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                        ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                            from _ in guard(
                                edit.FirstFace >= 0 && edit.FirstFace < first.Faces.Count
                                && edit.SecondFace >= 0 && edit.SecondFace < second.Faces.Count,
                                op.InvalidInput())
                            from built in op.Catch(() => {
                                System.Collections.Generic.List<Brep> fillets = [];
                                System.Collections.Generic.List<Brep> trimmed0 = [];
                                System.Collections.Generic.List<Brep> trimmed1 = [];
                                return op.Confirm(success: rail.FilletSurfaceToRail(
                                        faceWithCurve: first.Faces[edit.FirstFace], secondFace: second.Faces[edit.SecondFace],
                                        u1: edit.U, v1: edit.V, railDegree: edit.Law.Rail.Key, arcDegree: edit.Law.Arc.Key,
                                        arcSliders: Seq(edit.Law.Sliders.Tangent.Value, edit.Law.Sliders.Inner.Value).AsIterable(),
                                        numBezierSrfs: edit.Law.BezierSurfaceCount,
                                        extend: edit.Law.Extend, split_type: edit.Law.Split, tolerance: model.Absolute.Value,
                                        out_fillets: fillets, out_breps0: trimmed0, out_breps1: trimmed1, fitResults: out double[] fit))
                                    .Bind(_ => OwnRailFillet(fillets: fillets, trimmed0: trimmed0, trimmed1: trimmed1, fit: fit, op: op));
                            })
                            select built)));
            },
            textOutlines: static (model, edit) => {
                Op op = Op.Of(name: nameof(TextOutlines));
                return
                    from text in op.AcceptText(value: edit.Text)
                    from font in op.AcceptText(value: edit.Font)
                    from built in ModelGate.Many(op, CurveSlot.Outlined, () => Curve.CreateTextOutlines(
                        text: text, font: font, textHeight: edit.Height, textStyle: TextFace.Native(edit.Faces),
                        closeLoops: edit.CloseLoops,
                        plane: edit.Frame, smallCapsScale: edit.SmallCapsScale, tolerance: model.Absolute.Value))
                    select built;
            },
            directionsMatch: static (_, edit) => {
                Op op = Op.Of(name: nameof(DirectionsMatch));
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        op.Catch(() => Fin.Succ(value: Built<CurveSlot>.Of(operation: op,
                            Products: Seq<GeometryHandle>(),
                            Evidence: BuildReceipt<CurveSlot>.Of(
                                slot: CurveSlot.DirectionVerdict,
                                body: new BuildBody.Flag(Value: Curve.DoDirectionsMatch(curveA: first, curveB: second))))))));
            });

    private static Fin<Built<CurveSlot>> OffsetNormal(
        (Curve Curve, Context Model, Offset Edit, Op Op) context,
        Point3d directionPoint,
        Vector3d normal,
        CurveOffsetCornerStyle corner,
        CurveOffsetEndStyle end,
        bool loose) =>
        ModelGate.Many(context.Op, CurveSlot.Offset, () => context.Curve.Offset(
            directionPoint: directionPoint, normal: normal, distance: context.Edit.Distance.Value,
            tolerance: context.Model.Absolute.Value, angleTolerance: context.Model.Angle.Value, loose: loose,
            cornerStyle: corner, endStyle: end));

    private static Fin<Built<CurveSlot>> Borrowed(GeometryHandle handle, Op op, Func<Curve, Op, Fin<Built<CurveSlot>>> body) =>
        ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: handle, key: op, body: curve => body(curve, op));

    private static Fin<Built<CurveSlot>> OwnRailFillet(
        System.Collections.Generic.IEnumerable<Brep> fillets,
        System.Collections.Generic.IEnumerable<Brep> trimmed0,
        System.Collections.Generic.IEnumerable<Brep> trimmed1,
        double[] fit,
        Op op) =>
        ModelGate.Staged(op: op,
            extra: toSeq(fit ?? []).Fold(BuildReceipt<CurveSlot>.Empty, (receipt, value) =>
                receipt + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Measure(Value: value))),
            (CurveSlot.Filleted, fillets, false),
            (CurveSlot.Trimmed0, trimmed0, true),
            (CurveSlot.Trimmed1, trimmed1, true));

    private delegate Curve CatenaryNative<in TShape>(
        Point3d catenary_start, Point3d catenary_end, Vector3d axis_dir, TShape shape, bool bSmooth, int point_count,
        out Point3d apex_out, out double parameter_out, out double length_out, out double max_deviation_out);

    private static (Curve Hung, Point3d Apex, double Parameter, double Length, double Deviation) Hung<TShape>(
        CatenaryNative<TShape> native, Catenary request, TShape shape) {
        Curve hung = native(
            request.Start, request.End, request.AxisDirection, shape, request.Smooth, request.PointCount,
            out Point3d apex, out double parameter, out double length, out double deviation);
        return (Hung: hung, Apex: apex, Parameter: parameter, Length: length, Deviation: deviation);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Curves {
    public static Fin<Built<CurveSlot>> Build(Context context, params ReadOnlySpan<CurveOp> operations) =>
        ModelGate.Entry(
            context: context,
            operations: operations,
            admit: static (operation, key) => operation.Admitted(key: key),
            apply: static (operation, model) => operation.Apply(domain: model));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]               | [OWNER]                             | [FORM]                                      | [ENTRY]                  |
| :-----: | :---------------------- | :---------------------------------- | :------------------------------------------ | :----------------------- |
|  [01]   | offset framing          | `OffsetFrame`                       | planar / fitted-normal / loose-normal cases | `CurveOp.Offset`         |
|  [02]   | surface offset          | `CurveScalar`/`SurfaceOffsetTarget` | finite scalar offset modes                  | surface offset cases     |
|  [03]   | ribbon carrier          | `RibbonLaw`                         | `RibbonOffsetParameters` as one value       | `CurveOp.Ribbon`/`Rig`   |
|  [04]   | advanced fit            | `FitLaw`                            | `NurbsCurveFitParameters` as one value      | `CurveOp.NurbsFit`/`Rig` |
|  [05]   | extension modality      | `ExtendLaw`                         | length/geometry/point/line/arc/on-surface   | `CurveOp.Extend`         |
|  [06]   | split modality          | `SplitCutter`                       | parameters/brep/surface/plane               | `CurveOp.Split`          |
|  [07]   | pull and project        | `PullTarget`/`ProjectTarget`        | destinations with brep-side index maps      | `CurveOp.Pull`/`Project` |
|  [08]   | curve booleans          | `CurveBooleanLaw`/`CurveOp`         | verb-valid source arity and region topology | `Boolean`/`Regions`      |
|  [09]   | blend and tween         | `BlendLaw`/`ArcBlendLaw`/`TweenLaw` | payload-valid blend and tween modes         | blend and tween cases    |
|  [10]   | spiral/parabola seeds   | `SpiralLaw`/`ParabolaSeed`          | seed unions selecting statics               | construction cases       |
|  [11]   | analytic/catenary seeds | `AnalyticCurve`/`CatenaryLaw`       | seed unions selecting statics               | construction cases       |
|  [12]   | value-semantic edit     | `CurveEdit`                         | duplicate then cleanup/close/interval trim  | `CurveOp.Edit`           |
|  [13]   | rail surface fillet     | `RailFilletLaw`                     | rail and arc structure plus fit evidence    | `CurveOp.RailFillet`     |
|  [14]   | font style              | `TextFace`                          | additive style bits folded from a set       | `CurveOp.TextOutlines`   |
|  [15]   | admission               | `CurveOp.Admitted`                  | whole-roster payload gate before dispatch   | `Curves.Build`           |
|  [16]   | curve verbs             | `CurveOp`                           | one flat `[Union]` with total dispatch      | `Curves.Build`           |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

- [ARC_BEZIER_SLIDER]-[OPEN]: does `NurbsCurve.CreateNonRationalArcBezier` refuse a negative `tanSlider`/`midSlider`; the host documents both as "a number between zero and one" while the folder's `ArcSlider` admits -1..1 off the `SurfaceFilletBase` band the same concept carries there — settle with a `tools.assay bridge` scenario at -0.5 and compare the curve against the +0.5 run.
