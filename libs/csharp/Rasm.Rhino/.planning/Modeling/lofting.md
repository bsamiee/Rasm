# [RASM_RHINO_MODELING_LOFTING]

`Lofts.Build` owns loft, one- and two-rail sweep, direct and variational patch, developable construction, and ruling evidence. `LoftOp` admits rails, profiles, constraints, and seed surfaces once; `CurveFit` owns the shared rebuild/refit axis; `LoftRuntime` supplies cancellation and progress; and every geometry product exits through `Built<LoftSlot>`. `SweepFrameLaw` remains the frozen seam consumed by `SubDOp.FromSweepOne`.

## [01]-[INDEX]

- [02]-[SWEEP]: `SweepFrameLaw`, `CurveFit`, `SweepOneMode`, `SweepTwoStations`, `SweepTwoShapeFeature`, and `SweepEnds`.
- [03]-[PATCH]: `PatchLaw`, `VariationalLaw`, `LoftTangency`, `DevelopableLaw`, and `RulingSolve`.
- [04]-[ALGEBRA]: `LoftRuntime`, `LoftSlot`, `LoftOp`, and `Lofts.Build`.
- [05]-[EXECUTION]: compatibility, solver evidence, and geometry custody.

## [02]-[SWEEP]

`CurveFit` collapses loft and sweep fitting into one discriminant. `SweepOneMode` and `SweepTwoStations` encode native modality before dispatch, while `FrozenSet<SweepTwoShapeFeature>` composes height and auto-adjust capabilities. `SweepFrameLaw` alone maps roadlike semantics for both Rhino sweep and SubD sweep consumers.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepFrameLaw {
    private SweepFrameLaw() { }
    public sealed record Freeform : SweepFrameLaw;
    public sealed record RoadlikeTop : SweepFrameLaw;
    public sealed record RoadlikeFront : SweepFrameLaw;
    public sealed record RoadlikeRight : SweepFrameLaw;
    public sealed record RoadlikeDirection(Vector3d Normal) : SweepFrameLaw;

    internal bool Admissible => this is not RoadlikeDirection { Normal: var normal }
        || (normal.IsValid && !normal.IsZero);

    internal (SweepFrame Frame, Vector3d Normal) Native => Switch(
        freeform: static _ => (SweepFrame.Freeform, Vector3d.Unset),
        roadlikeTop: static _ => (SweepFrame.Roadlike, Vector3d.ZAxis),
        roadlikeFront: static _ => (SweepFrame.Roadlike, Vector3d.YAxis),
        roadlikeRight: static _ => (SweepFrame.Roadlike, Vector3d.XAxis),
        roadlikeDirection: static law => (SweepFrame.Roadlike, law.Normal));

    internal Unit Rig(SweepOneRail engine) => Switch(
        engine,
        freeform: static _ => unit,
        roadlikeTop: static sweep => { sweep.SetToRoadlikeTop(); return unit; },
        roadlikeFront: static sweep => { sweep.SetToRoadlikeFront(); return unit; },
        roadlikeRight: static sweep => { sweep.SetToRoadlikeRight(); return unit; },
        roadlikeDirection: static (sweep, law) => { sweep.SetRoadlikeUpDirection(up: law.Normal); return unit; });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveFit {
    private CurveFit() { }
    public sealed record AsIs : CurveFit;
    public sealed record Rebuild(int Points) : CurveFit;
    public sealed record Refit(RefitTarget Target) : CurveFit;

    internal bool IncludesRails => this is Refit { Target: var target } && target == RefitTarget.SectionsAndRails;

    internal bool Admissible => this switch {
        AsIs => true,
        Rebuild { Points: >= 2 } => true,
        Refit { Target: not null } => true,
        _ => false,
    };

    internal (SweepRebuild Kind, int Points, double Tolerance, bool RefitRail) Native(Context domain) => Switch(
        domain,
        asIs: static _ => (SweepRebuild.None, 0, 0.0, false),
        rebuild: static (_, law) => (SweepRebuild.Rebuild, law.Points, 0.0, false),
        refit: static (model, law) => (SweepRebuild.Refit, 0, model.Absolute.Value, law.Target.Native));
}

[SmartEnum<int>]
public sealed partial class RefitTarget {
    public static readonly RefitTarget Sections = new(key: 0, native: false);
    public static readonly RefitTarget SectionsAndRails = new(key: 1, native: true);

    internal bool Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepTwoStations {
    private SweepTwoStations() { }
    public sealed record Static : SweepTwoStations;
    public sealed record Engine(Seq<double> Rail1, Seq<double> Rail2) : SweepTwoStations;
    public sealed record Partitioned(Seq<Point2d> RailParameters) : SweepTwoStations;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepOneMode {
    private SweepOneMode() { }
    public sealed record Static : SweepOneMode;
    public sealed record Segmented : SweepOneMode;
    public sealed record Parameterized(Seq<double> ShapeParameters) : SweepOneMode;
}

[SmartEnum<int>]
public sealed partial class SweepTwoShapeFeature {
    public static readonly SweepTwoShapeFeature MaintainHeight = new(key: 0);
    public static readonly SweepTwoShapeFeature AutoAdjust = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class DevelopableDirection {
    public static readonly DevelopableDirection Forward = new(key: 0, native: (false, false));
    public static readonly DevelopableDirection ReverseRail0 = new(key: 1, native: (true, false));
    public static readonly DevelopableDirection ReverseRail1 = new(key: 2, native: (false, true));
    public static readonly DevelopableDirection ReverseBoth = new(key: 3, native: (true, true));

    internal (bool ReverseRail0, bool ReverseRail1) Native { get; }
}

[SmartEnum<int>]
public sealed partial class SweepClosure {
    public static readonly SweepClosure Open = new(key: 0, native: false);
    public static readonly SweepClosure Closed = new(key: 1, native: true);

    internal bool Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DevelopableLaw {
    private DevelopableLaw() { }
    public sealed record ByDensity(DevelopableDirection Direction, int Density) : DevelopableLaw;
    public sealed record ByRulings(Seq<Point2d> FixedRulings) : DevelopableLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RulingSolve {
    private RulingSolve() { }
    public sealed record Local(Interval Domain0, Interval Domain1) : RulingSolve;
    public sealed record MinTwistSecond(Interval Domain1) : RulingSolve;
    public sealed record MinTwistBoth(Interval Domain0, Interval Domain1) : RulingSolve;
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SweepEnds {
    public Option<Point3d> Start { get; }
    public Option<Point3d> End { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Option<Point3d> start,
        ref Option<Point3d> end) { }

    internal bool Admissible => Start.ForAll(static point => point.IsValid)
        && End.ForAll(static point => point.IsValid);

    internal Point3d StartOrUnset => Start.IfNone(Point3d.Unset);
    internal Point3d EndOrUnset => End.IfNone(Point3d.Unset);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct CurveCompatibility {
    public int SimplifyMethod { get; }
    public int PointCount { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int simplifyMethod,
        ref int pointCount) {
        if (!IsAdmissible(simplifyMethod: simplifyMethod, pointCount: pointCount)) {
            validationError = new ValidationError("Curve compatibility requires a valid simplifier and point count.");
        }
    }

    internal bool Admissible => IsAdmissible(simplifyMethod: SimplifyMethod, pointCount: PointCount);

    private static bool IsAdmissible(int simplifyMethod, int pointCount) => simplifyMethod >= 0 && pointCount >= 2;
}
```

## [03]-[PATCH]

`PatchLaw`, `VariationalLaw`, and `LoftTangency` validate before native solver carriers exist. `PatchEdge` and `LoftTangentEnd` replace positional booleans with bounded policy values. Variational warning, error, and continuity states preserve native absence separately from negative or empty results.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PatchEdge {
    public static readonly PatchEdge North = new(key: 0);
    public static readonly PatchEdge East = new(key: 1);
    public static readonly PatchEdge South = new(key: 2);
    public static readonly PatchEdge West = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class PatchBehavior {
    public static readonly PatchBehavior Trim = new(key: 0);
    public static readonly PatchBehavior Tangency = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class LoftTangentEnd {
    public static readonly LoftTangentEnd Start = new(key: 0, native: (true, false));
    public static readonly LoftTangentEnd End = new(key: 1, native: (false, true));
    public static readonly LoftTangentEnd Both = new(key: 2, native: (true, true));

    internal (bool Start, bool End) Native { get; }
}

[SmartEnum<int>]
public sealed partial class VariationalEdgePolicy {
    public static readonly VariationalEdgePolicy Free = new(key: 0, native: false);
    public static readonly VariationalEdgePolicy Preserve = new(key: 1, native: true);

    internal bool Native { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct PatchLaw {
    public int USpans { get; }
    public int VSpans { get; }
    public FrozenSet<PatchBehavior> Behavior { get; }
    public double PointSpacing { get; }
    public double Flexibility { get; }
    public double SurfacePull { get; }
    public FrozenSet<PatchEdge> Edges { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int uSpans,
        ref int vSpans,
        ref FrozenSet<PatchBehavior> behavior,
        ref double pointSpacing,
        ref double flexibility,
        ref double surfacePull,
        ref FrozenSet<PatchEdge> edges) {
        if (!IsAdmissible(
            uSpans: uSpans,
            vSpans: vSpans,
            behavior: behavior,
            pointSpacing: pointSpacing,
            flexibility: flexibility,
            surfacePull: surfacePull,
            edges: edges)) {
            validationError = new ValidationError("Patch policies, spans, and solver weights are outside the admitted range.");
        }
    }

    internal bool Admissible => IsAdmissible(
        uSpans: USpans,
        vSpans: VSpans,
        behavior: Behavior,
        pointSpacing: PointSpacing,
        flexibility: Flexibility,
        surfacePull: SurfacePull,
        edges: Edges);

    private static bool IsAdmissible(
        int uSpans,
        int vSpans,
        FrozenSet<PatchBehavior>? behavior,
        double pointSpacing,
        double flexibility,
        double surfacePull,
        FrozenSet<PatchEdge>? edges) =>
        uSpans > 0 && vSpans > 0
        && Lofts.Declared(values: behavior, rows: PatchBehavior.Items)
        && Lofts.Declared(values: edges, rows: PatchEdge.Items)
        && double.IsFinite(pointSpacing) && pointSpacing > 0.0
        && double.IsFinite(flexibility) && flexibility >= 0.0
        && double.IsFinite(surfacePull) && surfacePull >= 0.0;

    internal bool[] FixedEdges => [
        Edges.Contains(PatchEdge.North),
        Edges.Contains(PatchEdge.East),
        Edges.Contains(PatchEdge.South),
        Edges.Contains(PatchEdge.West),
    ];
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct VariationalLaw {
    public RhinoVariationalDomain Domain { get; }
    public int DegreeU { get; }
    public int DegreeV { get; }
    public int SpanCountU { get; }
    public int SpanCountV { get; }
    public double Stretching { get; }
    public double Bending { get; }
    public double RocBending { get; }
    public double UVRotation { get; }
    public int MaxRefinements { get; }
    public VariationalEdgePolicy Edges { get; }
    public Option<GeometryHandle> InitialSurface { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref RhinoVariationalDomain domain,
        ref int degreeU,
        ref int degreeV,
        ref int spanCountU,
        ref int spanCountV,
        ref double stretching,
        ref double bending,
        ref double rocBending,
        ref double uvRotation,
        ref int maxRefinements,
        ref VariationalEdgePolicy edges,
        ref Option<GeometryHandle> initialSurface) {
        if (!IsAdmissible(
            domain: domain,
            degreeU: degreeU,
            degreeV: degreeV,
            spanCountU: spanCountU,
            spanCountV: spanCountV,
            stretching: stretching,
            bending: bending,
            rocBending: rocBending,
            uvRotation: uvRotation,
            maxRefinements: maxRefinements,
            edges: edges,
            initialSurface: initialSurface)) {
            validationError = new ValidationError("Variational domain, degree, spans, weights, rotation, and refinements are outside the admitted range.");
        }
    }

    internal bool Admissible => IsAdmissible(
        domain: Domain,
        degreeU: DegreeU,
        degreeV: DegreeV,
        spanCountU: SpanCountU,
        spanCountV: SpanCountV,
        stretching: Stretching,
        bending: Bending,
        rocBending: RocBending,
        uvRotation: UVRotation,
        maxRefinements: MaxRefinements,
        edges: Edges,
        initialSurface: InitialSurface);

    private static bool IsAdmissible(
        RhinoVariationalDomain domain,
        int degreeU,
        int degreeV,
        int spanCountU,
        int spanCountV,
        double stretching,
        double bending,
        double rocBending,
        double uvRotation,
        int maxRefinements,
        VariationalEdgePolicy? edges,
        Option<GeometryHandle> initialSurface) =>
        Enum.IsDefined(domain)
        && degreeU > 0 && degreeV > 0 && spanCountU > 0 && spanCountV > 0 && edges is not null
        && double.IsFinite(stretching) && stretching >= 0.0
        && double.IsFinite(bending) && bending >= 0.0
        && double.IsFinite(rocBending) && rocBending >= 0.0
        && double.IsFinite(uvRotation) && maxRefinements >= 0
        && initialSurface.ForAll(static handle => handle is not null);

    internal Fin<Brep.VariationalPatchSettings> Rig(Context domain, Option<Surface> initial, Op key) =>
        key.Catch(() => Fin.Succ(value: new Brep.VariationalPatchSettings {
            Tolerance = domain.Absolute.Value,
            AngleToleranceRadians = domain.Angle.Value,
            InternalTolerance = domain.Absolute.Value,
            CurvatureRelativeTolerance = domain.Fractional,
            CurvatureZeroTolerance = domain.Absolute.Value,
            DegreeU = DegreeU,
            DegreeV = DegreeV,
            SpanCountU = SpanCountU,
            SpanCountV = SpanCountV,
            Domain = Domain,
            Stretching = Stretching,
            Bending = Bending,
            RocBending = RocBending,
            UVRotation = UVRotation,
            MaxRefinements = MaxRefinements,
            InitialSurface = initial.IfNoneUnsafe((Surface?)null),
            PreserveEdges = Edges.Native,
        }));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct LoftTangency {
    public GeometryHandle StartOwner { get; }
    public int StartTrim { get; }
    public GeometryHandle EndOwner { get; }
    public int EndTrim { get; }
    public LoftTangentEnd Ends { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref GeometryHandle startOwner,
        ref int startTrim,
        ref GeometryHandle endOwner,
        ref int endTrim,
        ref LoftTangentEnd ends) {
        if (!IsAdmissible(
            startOwner: startOwner,
            startTrim: startTrim,
            endOwner: endOwner,
            endTrim: endTrim,
            ends: ends)) {
            validationError = new ValidationError("Loft tangency requires owners, ends, and non-negative trim indices.");
        }
    }

    internal bool Admissible => IsAdmissible(
        startOwner: StartOwner,
        startTrim: StartTrim,
        endOwner: EndOwner,
        endTrim: EndTrim,
        ends: Ends);

    private static bool IsAdmissible(
        GeometryHandle? startOwner,
        int startTrim,
        GeometryHandle? endOwner,
        int endTrim,
        LoftTangentEnd? ends) =>
        startOwner is not null && endOwner is not null && ends is not null && startTrim >= 0 && endTrim >= 0;
}
```

## [04]-[ALGEBRA]

`LoftOp` is the sole construction algebra. `LoftRuntime` owns cancellation and progress, while `VariationalThreading` names solver parallelism. Native engines and settings remain scoped to their consuming arm, and all solver side channels land as typed receipt facts.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct LoftRuntime {
    public Context Domain { get; }
    public CancellationToken Cancellation { get; }
    public Option<IProgress<double>> Progress { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Context domain,
        ref CancellationToken cancellation,
        ref Option<IProgress<double>> progress) {
        if (domain is null) {
            validationError = new ValidationError("Loft runtime requires a domain context.");
        }
    }

    public static implicit operator Context(LoftRuntime runtime) => runtime.Domain;

    internal IProgress<double>? Reporter => Progress.IfNoneUnsafe((IProgress<double>?)null);

    internal Fin<Built<LoftSlot>> Apply(LoftOp operation, Context _) => operation.Apply(this);
}

[SmartEnum<int>]
public sealed partial class VariationalThreading {
    public static readonly VariationalThreading Serial = new(key: 0, native: false);
    public static readonly VariationalThreading Parallel = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class LoftSlot {
    public static readonly LoftSlot Swept = new(key: 0);
    public static readonly LoftSlot Lofted = new(key: 1);
    public static readonly LoftSlot Patched = new(key: 2);
    public static readonly LoftSlot Solved = new(key: 3);
    public static readonly LoftSlot Developed = new(key: 4);
    public static readonly LoftSlot Rulings = new(key: 5);
    public static readonly LoftSlot Warning = new(key: 6);
    public static readonly LoftSlot Error = new(key: 7);
    public static readonly LoftSlot G0Interior = new(key: 8);
    public static readonly LoftSlot G0 = new(key: 9);
    public static readonly LoftSlot G1 = new(key: 10);
    public static readonly LoftSlot G2 = new(key: 11);
    public static readonly LoftSlot Compatible = new(key: 12);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoftOp {
    private LoftOp() { }
    public sealed record SweepOne(
        GeometryHandle Rail, Seq<GeometryHandle> Shapes, SweepEnds Ends, SweepFrameLaw Frame,
        SweepClosure Closure, SweepBlend Blend, SweepMiter Miter, CurveFit Fit, SweepOneMode Mode) : LoftOp;
    public sealed record SweepTwo(
        GeometryHandle Rail1, GeometryHandle Rail2, Seq<GeometryHandle> Shapes, SweepEnds Ends,
        SweepClosure Closure, CurveFit Fit, FrozenSet<SweepTwoShapeFeature> Shape, SweepTwoStations Stations) : LoftOp;
    public sealed record Loft(
        Seq<GeometryHandle> Shapes, SweepEnds Ends, LoftType Kind, SweepClosure Closure,
        CurveFit Fit, Option<LoftTangency> Tangency = default) : LoftOp;
    public sealed record MakeCompatible(Seq<GeometryHandle> Shapes, SweepEnds Ends, CurveCompatibility Law) : LoftOp;
    public sealed record Patch(Seq<GeometryHandle> Geometry, Option<GeometryHandle> StartingSurface, PatchLaw Law) : LoftOp;
    public sealed record Variational(
        Seq<(GeometryHandle Curve, Continuity Continuity)> Edges,
        Seq<(GeometryHandle Curve, Continuity Continuity)> InternalCurves,
        Seq<Point3d> Points, VariationalLaw Law,
        VariationalThreading Threading) : LoftOp;
    public sealed record Developable(GeometryHandle Rail0, GeometryHandle Rail1, DevelopableLaw Law) : LoftOp;
    public sealed record SolveRuling(GeometryHandle Rail0, GeometryHandle Rail1, Point2d Seed, RulingSolve Law) : LoftOp;
    public sealed record AdjustRulings(GeometryHandle Rail0, GeometryHandle Rail1, Seq<Point2d> Rulings) : LoftOp;

    internal Fin<LoftOp> Admitted(Op key) =>
        guard(this switch {
            SweepOne edit => edit.Rail is not null
                && Handles(edit.Shapes)
                && edit.Frame is { Admissible: true }
                && edit.Ends.Admissible
                && edit.Closure is not null
                && edit.Fit is { Admissible: true }
                && Enum.IsDefined(edit.Blend)
                && Enum.IsDefined(edit.Miter)
                && SweepOneAdmitted(edit.Mode),
            SweepTwo edit => edit.Rail1 is not null
                && edit.Rail2 is not null
                && Handles(edit.Shapes)
                && edit.Ends.Admissible
                && edit.Closure is not null
                && edit.Fit is { Admissible: true }
                && Lofts.Declared(values: edit.Shape, rows: SweepTwoShapeFeature.Items)
                && SweepTwoAdmitted(edit.Stations),
            Loft edit => Handles(edit.Shapes)
                && edit.Ends.Admissible
                && edit.Closure is not null
                && edit.Fit is { Admissible: true }
                && Enum.IsDefined(edit.Kind)
                && edit.Tangency.ForAll(static tangency => tangency.Admissible),
            MakeCompatible edit => Handles(edit.Shapes) && edit.Ends.Admissible && edit.Law.Admissible,
            Patch edit => Handles(edit.Geometry)
                && edit.StartingSurface.ForAll(static handle => handle is not null)
                && edit.Law.Admissible,
            Variational edit => Constraints(edit.Edges)
                && Constraints(edit.InternalCurves, allowEmpty: true)
                && edit.Points.ForAll(static point => point.IsValid)
                && edit.Law.Admissible
                && edit.Threading is not null,
            Developable edit => edit.Rail0 is not null
                && edit.Rail1 is not null
                && DevelopableAdmitted(edit.Law),
            SolveRuling edit => edit.Rail0 is not null && edit.Rail1 is not null
                && edit.Seed.IsValid && RulingAdmitted(edit.Law),
            AdjustRulings edit => edit.Rail0 is not null && edit.Rail1 is not null
                && !edit.Rulings.IsEmpty && edit.Rulings.ForAll(static ruling => ruling.IsValid),
            _ => false,
        }, key.InvalidInput()).ToFin().Map(_ => this);

    private static bool Handles(Seq<GeometryHandle> handles, bool allowEmpty = false) =>
        (allowEmpty || !handles.IsEmpty) && handles.ForAll(static handle => handle is not null);

    private static bool Constraints(
        Seq<(GeometryHandle Curve, Continuity Continuity)> rows,
        bool allowEmpty = false) =>
        (allowEmpty || !rows.IsEmpty)
        && rows.ForAll(static row => row.Curve is not null && Enum.IsDefined(row.Continuity));

    private static bool DevelopableAdmitted(DevelopableLaw? law) => law switch {
        DevelopableLaw.ByDensity { Direction: not null, Density: > 0 } => true,
        DevelopableLaw.ByRulings edit => !edit.FixedRulings.IsEmpty
            && edit.FixedRulings.ForAll(static ruling => ruling.IsValid),
        _ => false,
    };

    private static bool RulingAdmitted(RulingSolve? law) => law switch {
        RulingSolve.Local edit => edit.Domain0.IsValid && edit.Domain1.IsValid,
        RulingSolve.MinTwistSecond edit => edit.Domain1.IsValid,
        RulingSolve.MinTwistBoth edit => edit.Domain0.IsValid && edit.Domain1.IsValid,
        _ => false,
    };

    private static bool SweepOneAdmitted(SweepOneMode? mode) => mode switch {
        SweepOneMode.Static or SweepOneMode.Segmented => true,
        SweepOneMode.Parameterized edit => edit.ShapeParameters.ForAll(static parameter => double.IsFinite(parameter)),
        _ => false,
    };

    private static bool SweepTwoAdmitted(SweepTwoStations? stations) => stations switch {
        SweepTwoStations.Static => true,
        SweepTwoStations.Engine edit => edit.Rail1.ForAll(static parameter => double.IsFinite(parameter))
            && edit.Rail2.ForAll(static parameter => double.IsFinite(parameter)),
        SweepTwoStations.Partitioned edit => edit.RailParameters.ForAll(static parameter => parameter.IsValid),
        _ => false,
    };

    internal Fin<Built<LoftSlot>> Apply(LoftRuntime runtime) =>
        Switch(
            runtime,
            sweepOne: static (model, edit) => {
                Op op = Op.Of(name: nameof(SweepOne));
                return ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail, key: op, body: rail =>
                    ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                        edit.Mode.Switch(
                            parameterized: parameterized =>
                                from _ in guard(
                                    parameterized.ShapeParameters.Count == shapes.Count && edit.Ends.Start.IsNone && edit.Ends.End.IsNone
                                    && !edit.Fit.IncludesRails,
                                    op.InvalidInput())
                                from built in op.Catch(() => {
                                    SweepOneRail engine = new() {
                                        SweepTolerance = model.Domain.Absolute.Value,
                                        AngleToleranceRadians = model.Domain.Angle.Value,
                                        ClosedSweep = edit.Closure.Native,
                                        GlobalShapeBlending = edit.Blend == SweepBlend.Global,
                                        MiterType = (int)edit.Miter,
                                    };
                                    _ = edit.Frame.Rig(engine: engine);
                                    (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model);
                                    return ModelGate.Many(op, LoftSlot.Swept, () => kind switch {
                                        SweepRebuild.Rebuild => engine.PerformSweepRebuild(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable(), points),
                                        SweepRebuild.Refit => engine.PerformSweepRefit(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable(), refit),
                                        _ => engine.PerformSweep(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable()),
                                    });
                                })
                                select built,
                            segmented: _ =>
                                from _ in guard(!edit.Fit.IncludesRails, op.InvalidInput())
                                from built in op.Catch(() => {
                                    (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                                    (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model);
                                    return ModelGate.Many(op, LoftSlot.Swept, () => Brep.CreateFromSweepSegmented(
                                        rail: rail, shapes: shapes.AsIterable(), startPoint: edit.Ends.StartOrUnset, endPoint: edit.Ends.EndOrUnset,
                                        frameType: frame, roadlikeNormal: normal, closed: edit.Closure.Native, blendType: edit.Blend, miterType: edit.Miter,
                                        tolerance: model.Domain.Absolute.Value, rebuildType: kind, rebuildPointCount: points, refitTolerance: refit));
                                })
                                select built,
                            @static: _ => op.Catch(() => {
                                    (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                                    (SweepRebuild kind, int points, double refit, bool refitRail) = edit.Fit.Native(domain: model);
                                    return ModelGate.Many(op, LoftSlot.Swept, () => Brep.CreateFromSweep(
                                        rail: rail, shapes: shapes.AsIterable(), startPoint: edit.Ends.StartOrUnset, endPoint: edit.Ends.EndOrUnset,
                                        frameType: frame, roadlikeNormal: normal, closed: edit.Closure.Native, blendType: edit.Blend, miterType: edit.Miter,
                                        tolerance: model.Domain.Absolute.Value, rebuildType: kind, rebuildPointCount: points, refitTolerance: refit, refitRail: refitRail));
                                })));
            },
            sweepTwo: static (model, edit) => {
                Op op = Op.Of(name: nameof(SweepTwo));
                return ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                    ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail2, key: op, body: rail2 =>
                        ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                            edit.Stations.Switch(
                                engine: stations =>
                                    from _ in guard(
                                        stations.Rail1.Count == shapes.Count && stations.Rail2.Count == shapes.Count
                                        && edit.Ends.Start.IsNone && edit.Ends.End.IsNone
                                        && !edit.Fit.IncludesRails,
                                        op.InvalidInput())
                                    from built in op.Catch(() => {
                                        SweepTwoRail engine = new() {
                                            SweepTolerance = model.Domain.Absolute.Value,
                                            AngleToleranceRadians = model.Domain.Angle.Value,
                                            ClosedSweep = edit.Closure.Native,
                                            MaintainHeight = edit.Shape.Contains(SweepTwoShapeFeature.MaintainHeight),
                                            AutoAdjust = edit.Shape.Contains(SweepTwoShapeFeature.AutoAdjust),
                                        };
                                        (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model);
                                        return ModelGate.Many(op, LoftSlot.Swept, () => kind switch {
                                            SweepRebuild.Rebuild => engine.PerformSweepRebuild(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable(), points),
                                            SweepRebuild.Refit => engine.PerformSweepRefit(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable(), refit),
                                            _ => engine.PerformSweep(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable()),
                                        });
                                    })
                                    select built,
                                partitioned: stations =>
                                    from _ in guard(
                                        stations.RailParameters.Count == shapes.Count && edit.Ends.Start.IsNone && edit.Ends.End.IsNone
                                        && edit.Fit is CurveFit.AsIs
                                        && edit.Shape.SetEquals([SweepTwoShapeFeature.AutoAdjust]),
                                        op.InvalidInput())
                                    from built in op.Catch(() => ModelGate.Many(op, LoftSlot.Swept, () => Brep.CreateFromSweepInParts(
                                        rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                        rail_params: stations.RailParameters.AsIterable(), closed: edit.Closure.Native, tolerance: model.Domain.Absolute.Value)))
                                    select built,
                                @static: _ =>
                                    from _ in guard(!edit.Fit.IncludesRails, op.InvalidInput())
                                    from built in op.Catch(() => {
                                        (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model);
                                        return ModelGate.Many(op, LoftSlot.Swept, () => Brep.CreateFromSweep(
                                            rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                            start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset, closed: edit.Closure.Native,
                                            tolerance: model.Domain.Absolute.Value, rebuild: kind, rebuildPointCount: points, refitTolerance: refit,
                                            preserveHeight: edit.Shape.Contains(SweepTwoShapeFeature.MaintainHeight),
                                            autoAdjust: edit.Shape.Contains(SweepTwoShapeFeature.AutoAdjust)));
                                    })
                                    select built))));
            },
            makeCompatible: static (model, edit) => {
                Op op = Op.Of(name: nameof(MakeCompatible));
                return ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                    op.Catch(() => ModelGate.OwnMany(
                            built: NurbsCurve.MakeCompatible(
                                curves: shapes.AsIterable(),
                                startPt: edit.Ends.StartOrUnset,
                                endPt: edit.Ends.EndOrUnset,
                                simplifyMethod: edit.Law.SimplifyMethod,
                                numPoints: edit.Law.PointCount,
                                refitTolerance: model.Domain.Absolute.Value,
                                angleTolerance: model.Domain.Angle.Value),
                            key: op)
                        .Map(owned => Built<LoftSlot>.Of(
                            operation: op,
                            Products: owned,
                            Evidence: BuildReceipt<LoftSlot>.Of(
                                slot: LoftSlot.Compatible,
                                body: new BuildBody.Tally(Count: owned.Count))))));
            },
            loft: static (model, edit) => {
                Op op = Op.Of(name: nameof(Loft));
                return from _ in guard(
                           edit.Kind != LoftType.Developable &&
                           !edit.Fit.IncludesRails,
                           op.InvalidInput())
                       from built in ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                           edit.Tangency.Case switch {
                        LoftTangency tangency => ModelGate.Borrow<Brep, Built<LoftSlot>>(handle: tangency.StartOwner, key: op, body: startOwner =>
                            ModelGate.Borrow<Brep, Built<LoftSlot>>(handle: tangency.EndOwner, key: op, body: endOwner =>
                                from _ in guard(
                                    tangency.StartTrim < startOwner.Trims.Count
                                    && tangency.EndTrim < endOwner.Trims.Count
                                    && edit.Fit is CurveFit.AsIs,
                                    op.InvalidInput())
                                from built in op.Catch(() => ModelGate.Many(op, LoftSlot.Lofted, () => Brep.CreateFromLoft(
                                    curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                    StartTangent: tangency.Ends.Native.Start,
                                    EndTangent: tangency.Ends.Native.End,
                                    StartTrim: startOwner.Trims[tangency.StartTrim], EndTrim: endOwner.Trims[tangency.EndTrim],
                                    loftType: edit.Kind, closed: edit.Closure.Native)))
                                select built)),
                        _ => op.Catch(() => ModelGate.Many(op, LoftSlot.Lofted, () => edit.Fit switch {
                            CurveFit.Rebuild fit => Brep.CreateFromLoftRebuild(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value, rebuildPointCount: fit.Points),
                            CurveFit.Refit => Brep.CreateFromLoftRefit(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value, refitTolerance: model.Domain.Absolute.Value),
                            _ => Brep.CreateFromLoft(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value),
                        })),
                    })
                       select built;
            },
            patch: static (model, edit) => {
                Op op = Op.Of(name: nameof(Patch));
                return ModelGate.BorrowMany<GeometryBase, Built<LoftSlot>>(handles: edit.Geometry, key: op, body: constraints =>
                    edit.StartingSurface.Case switch {
                        GeometryHandle starting => ModelGate.Borrow<Surface, Built<LoftSlot>>(handle: starting, key: op,
                            body: surface => ModelGate.Single(op, LoftSlot.Patched, () => Brep.CreatePatch(
                                geometry: constraints.AsIterable(), startingSurface: surface,
                                uSpans: edit.Law.USpans, vSpans: edit.Law.VSpans,
                                trim: edit.Law.Behavior.Contains(PatchBehavior.Trim),
                                tangency: edit.Law.Behavior.Contains(PatchBehavior.Tangency),
                                pointSpacing: edit.Law.PointSpacing, flexibility: edit.Law.Flexibility, surfacePull: edit.Law.SurfacePull,
                                fixEdges: edit.Law.FixedEdges, tolerance: model.Domain.Absolute.Value))),
                        _ => ModelGate.Single(op, LoftSlot.Patched, () => Brep.CreatePatch(
                            geometry: constraints.AsIterable(), startingSurface: null,
                            uSpans: edit.Law.USpans, vSpans: edit.Law.VSpans,
                            trim: edit.Law.Behavior.Contains(PatchBehavior.Trim),
                            tangency: edit.Law.Behavior.Contains(PatchBehavior.Tangency),
                            pointSpacing: edit.Law.PointSpacing, flexibility: edit.Law.Flexibility, surfacePull: edit.Law.SurfacePull,
                            fixEdges: edit.Law.FixedEdges, tolerance: model.Domain.Absolute.Value)),
                    });
            },
            variational: static (model, edit) => {
                Op op = Op.Of(name: nameof(Variational));
                return ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Edges.Map(static row => row.Curve), key: op, body: edgeCurves =>
                    ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.InternalCurves.Map(static row => row.Curve), key: op, allowEmpty: true, body: interiorCurves => {
                        Fin<Built<LoftSlot>> Solve(Option<Surface> initial) =>
                            from settings in edit.Law.Rig(domain: model, initial: initial, key: op)
                            from built in op.Catch(() => {
                                Brep patch = Brep.CreateVariationalPatch(
                                    edges: edgeCurves.Zip(edit.Edges.Map(static row => row.Continuity))
                                        .Map(static pair => new Brep.CurveConstraint(curve: pair.First, continuity: pair.Second)).AsIterable(),
                                    internalCurves: interiorCurves.Zip(edit.InternalCurves.Map(static row => row.Continuity))
                                        .Map(static pair => new Brep.CurveConstraint(curve: pair.First, continuity: pair.Second)).AsIterable(),
                                    points: edit.Points.Map(static point => new Brep.PointConstraint(point: point)).AsIterable(),
                                    settings: settings, multiThreading: edit.Threading.Native,
                                    cancelToken: model.Cancellation,
                                    progress: model.Reporter,
                                    results: out Brep.VariationalPatchResult verdict);
                                return ModelGate.Own(built: patch, key: op).Map(owned => Built<LoftSlot>.Of(
                                    operation: op,
                                    Products: Seq(owned),
                                    Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Solved, body: new BuildBody.Tally(Count: 1))
                                        + Channel(slot: LoftSlot.Warning, value: Optional(verdict.Warning).Map(static detail => (BuildBody)new BuildBody.Text(Value: detail)))
                                        + Channel(slot: LoftSlot.Error, value: Optional(verdict.Error).Map(static detail => (BuildBody)new BuildBody.Text(Value: detail)))
                                        + Channel(slot: LoftSlot.G0Interior, value: Optional(verdict.G0Int).Map(static held => (BuildBody)new BuildBody.Flag(Value: held)))
                                        + Channel(slot: LoftSlot.G0, value: Optional(verdict.G0).Map(static held => (BuildBody)new BuildBody.Flag(Value: held)))
                                        + Channel(slot: LoftSlot.G1, value: Optional(verdict.G1).Map(static held => (BuildBody)new BuildBody.Flag(Value: held)))
                                        + Channel(slot: LoftSlot.G2, value: Optional(verdict.G2).Map(static held => (BuildBody)new BuildBody.Flag(Value: held)))));
                            })
                            select built;
                        return edit.Law.InitialSurface.Case switch {
                            GeometryHandle seed => ModelGate.Borrow<Surface, Built<LoftSlot>>(handle: seed, key: op, body: surface => Solve(initial: Some(surface))),
                            _ => Solve(initial: Option<Surface>.None),
                        };
                    }));
            },
            developable: static (_, edit) => {
                Op op = Op.Of(name: nameof(Developable));
                return edit.Law.Switch(
                    (Edit: edit, Op: op),
                    byDensity: static (ctx, law) => ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: ctx.Edit.Rail0, key: ctx.Op, body: rail0 =>
                        ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: ctx.Edit.Rail1, key: ctx.Op, body: rail1 =>
                            ModelGate.Many(ctx.Op, LoftSlot.Developed, () => Brep.CreateDevelopableLoft(
                                crv0: rail0,
                                crv1: rail1,
                                reverse0: law.Direction.Native.ReverseRail0,
                                reverse1: law.Direction.Native.ReverseRail1,
                                density: law.Density)))),
                    byRulings: static (ctx, law) => ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: ctx.Edit.Rail0, key: ctx.Op, body: rail0 =>
                        ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: ctx.Edit.Rail1, key: ctx.Op, body: rail1 =>
                            ModelGate.Many(ctx.Op, LoftSlot.Developed, () => Brep.CreateDevelopableLoft(
                                rail0: rail0, rail1: rail1, fixedRulings: law.FixedRulings.AsIterable())))));
            },
            solveRuling: static (_, edit) => {
                Op op = Op.Of(name: nameof(SolveRuling));
                return ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail0, key: op, body: rail0 =>
                    ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                        edit.Law.Switch(
                            (Rail0: rail0, Rail1: rail1, Seed: edit.Seed, Op: op),
                            local: static (ctx, law) => ctx.Op.Catch(() => {
                                double t0 = ctx.Seed.X;
                                double t1 = ctx.Seed.Y;
                                int verdict = DevelopableSrf.GetLocalDevopableRuling(
                                    rail0: ctx.Rail0, t0: ctx.Seed.X, dom0: law.Domain0,
                                    rail1: ctx.Rail1, t1: ctx.Seed.Y, dom1: law.Domain1,
                                    t0_out: ref t0, t1_out: ref t1);
                                return Fin.Succ(value: Built<LoftSlot>.Of(
                                    operation: ctx.Op,
                                    Products: Seq<GeometryHandle>(),
                                    Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.UvRows(Rows: Seq(new Point2d(t0, t1))))
                                        + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.Code(Value: verdict))));
                            }),
                            minTwistSecond: static (ctx, law) => ctx.Op.Catch(() => {
                                double t1 = ctx.Seed.Y;
                                double cosine = 0.0;
                                return ctx.Op.Confirm(success: DevelopableSrf.RulingMinTwist(
                                        rail0: ctx.Rail0, t0: ctx.Seed.X, rail1: ctx.Rail1, t1: ctx.Seed.Y,
                                        dom1: law.Domain1, t1_out: ref t1, cos_twist_out: ref cosine))
                                    .Map(_ => RulingBuilt(operation: ctx.Op, t0: ctx.Seed.X, t1: t1, cosine: cosine));
                            }),
                            minTwistBoth: static (ctx, law) => ctx.Op.Catch(() => {
                                double t0 = ctx.Seed.X;
                                double t1 = ctx.Seed.Y;
                                double cosine = 0.0;
                                return ctx.Op.Confirm(success: DevelopableSrf.RulingMinTwist(
                                        rail0: ctx.Rail0, t0: ctx.Seed.X, dom0: law.Domain0,
                                        rail1: ctx.Rail1, t1: ctx.Seed.Y, dom1: law.Domain1,
                                        t0_out: ref t0, t1_out: ref t1, cos_twist_out: ref cosine))
                                    .Map(_ => RulingBuilt(operation: ctx.Op, t0: t0, t1: t1, cosine: cosine));
                            }))));
            },
            adjustRulings: static (_, edit) => {
                Op op = Op.Of(name: nameof(AdjustRulings));
                return ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail0, key: op, body: rail0 =>
                    ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                        op.Catch(() => {
                            System.Collections.Generic.IEnumerable<Point2d> rulings = edit.Rulings.AsIterable();
                            return op.Confirm(success: DevelopableSrf.UntwistRulings(rail0: rail0, rail1: rail1, rulings: ref rulings))
                                .Map(_ => Built<LoftSlot>.Of(
                                    operation: op,
                                    Products: Seq<GeometryHandle>(),
                                    Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.UvRows(Rows: toSeq(rulings)))));
                        })));
            });

    private static BuildReceipt<LoftSlot> Channel(LoftSlot slot, Option<BuildBody> value) =>
        value.Map(body => BuildReceipt<LoftSlot>.Of(slot: slot, body: body)).IfNone(BuildReceipt<LoftSlot>.Empty);

    private static Built<LoftSlot> RulingBuilt(Op operation, double t0, double t1, double cosine) => Built<LoftSlot>.Of(
        operation: operation,
        Products: Seq<GeometryHandle>(),
        Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.UvRows(Rows: Seq(new Point2d(t0, t1))))
            + BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.Measure(Value: cosine)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Lofts {
    internal static bool Declared<T>(FrozenSet<T>? values, IReadOnlyList<T> rows) where T : class =>
        values is not null && values.All(row => row is not null && rows.Contains(row));

    public static Eff<LoftRuntime, Built<LoftSlot>> Build(params ReadOnlySpan<LoftOp> operations) {
        Op op = Op.Of();
        Seq<LoftOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<LoftRuntime>().Bind(runtime =>
            (from _ in guard(!captured.IsEmpty, op.InvalidInput())
             from admitted in captured.TraverseM(operation =>
                     Optional(operation).ToFin(Fail: op.InvalidInput())
                         .Bind(active => active.Admitted(key: op)))
                 .As()
             from built in ModelGate.Folded(
                 context: runtime.Domain,
                 operations: admitted,
                 apply: runtime.Apply)
             select built).ToEff());
    }
}
```

## [05]-[EXECUTION]

`Lofts.Build` captures and admits every operation before entering `Eff<LoftRuntime, Built<LoftSlot>>`. `LoftOp.Admitted` rejects null handles, policy owners, and default-ghost value policies before `LoftRuntime.Apply` reaches a host arm.

`LoftOp.MakeCompatible` composes `NurbsCurve.MakeCompatible` with context-owned refit and angle tolerances. Consumers feed its owned curve products into `LoftOp.Loft`, `LoftOp.SweepOne`, or `LoftOp.SweepTwo` without a second compatibility surface.

Variational evidence encodes native absence structurally: a nullable channel lands a fact only when the host answered, so an empty `Project` over its slot is the unknown verdict, a present `Flag`/`Text` fact is the answer, and an empty warning string stays distinct from a missing one.
