# [RASM_RHINO_MODELING_MESHING]

`Meshes.Build` owns admitted mesh creation, transformation, projection, evidence, and egress. `MeshOp.QuadRemesh` remains the sole mesh-to-`SubDOp.FromMesh` seam.

## [01]-[INDEX]

- [02]-[FIDELITY]: `MeshPreset`, `MeshLaw`, and `MeshFidelity`.
- [03]-[POLICY]: `QuadLaw`, `WrapLaw`, `ReduceLaw`, `ExtrudeLaw`, and mesh generation policies.
- [04]-[ALGEBRA]: `MeshRuntime`, `MeshSlot`, `MeshEdit`, `MeshOp`, and `Meshes.Build`.
- [05]-[EXECUTION]: native carrier lifetime, typed evidence, and geometry custody.

## [02]-[FIDELITY]

`MeshFidelity` is the sole fidelity discriminant. `MeshLaw` validates the complete custom parameter set, `MeshPreset` carries live host factories, and `Rig` creates one disposable `MeshingParameters` carrier inside the consuming arm.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MeshPreset {
    public static readonly MeshPreset Minimal = new(key: 0, static () => MeshingParameters.Minimal);
    public static readonly MeshPreset Standard = new(key: 1, static () => MeshingParameters.Default);
    public static readonly MeshPreset FastRender = new(key: 2, static () => MeshingParameters.FastRenderMesh);
    public static readonly MeshPreset QualityRender = new(key: 3, static () => MeshingParameters.QualityRenderMesh);
    public static readonly MeshPreset Analysis = new(key: 4, static () => MeshingParameters.DefaultAnalysisMesh);

    [UseDelegateFromConstructor]
    internal partial MeshingParameters Mint();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshFidelity {
    private MeshFidelity() { }
    public sealed record Preset(MeshPreset Row) : MeshFidelity;
    public sealed record Density(MeshDensity Value) : MeshFidelity;
    public sealed record Custom(MeshLaw Law) : MeshFidelity;

    internal Fin<MeshingParameters> Rig(Context domain, Op key) =>
        key.Catch(() => Fin.Succ(value: Switch(
            domain,
            preset: static (_, fidelity) => fidelity.Row.Mint(),
            density: static (_, fidelity) => fidelity.Value.MinimumEdgeLength.Case switch {
                double minimum => new MeshingParameters(density: fidelity.Value.Value, minimumEdgeLength: minimum),
                _ => new MeshingParameters(density: fidelity.Value.Value),
            },
            custom: static (model, fidelity) => fidelity.Law.Mint(domain: model))));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshDensity {
    public double Value { get; }
    public Option<double> MinimumEdgeLength { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double value,
        ref Option<double> minimumEdgeLength) {
        if (!double.IsFinite(value) || value is < 0.0 or > 1.0 ||
            minimumEdgeLength.Exists(static length => !double.IsFinite(length) || length < 0.0)) {
            validationError = new ValidationError("Mesh density requires a finite normalized density and finite non-negative edge length.");
        }
    }
}

[SmartEnum<int>]
public sealed partial class MeshFidelityFeature {
    public static readonly MeshFidelityFeature JaggedSeams = new(key: 0);
    public static readonly MeshFidelityFeature RefineGrid = new(key: 1);
    public static readonly MeshFidelityFeature DoublePrecision = new(key: 2);
    public static readonly MeshFidelityFeature SimplePlanes = new(key: 3);
    public static readonly MeshFidelityFeature ComputeCurvature = new(key: 4);
    public static readonly MeshFidelityFeature ClosedObjectPostProcess = new(key: 5);
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshLaw {
    public MeshingParameterTextureRange TextureRange { get; }
    public FrozenSet<MeshFidelityFeature> Features { get; }
    public int GridMinCount { get; }
    public int GridMaxCount { get; }
    public double GridAspectRatio { get; }
    public double GridAmplification { get; }
    public double MinimumEdgeLength { get; }
    public double MaximumEdgeLength { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MeshingParameterTextureRange textureRange,
        ref FrozenSet<MeshFidelityFeature> features,
        ref int gridMinCount,
        ref int gridMaxCount,
        ref double gridAspectRatio,
        ref double gridAmplification,
        ref double minimumEdgeLength,
        ref double maximumEdgeLength) {
        // MaximumEdgeLength 0.0 is the unbounded sentinel; every other value admits finite and non-negative only.
        if (gridMinCount < 0 || gridMaxCount < gridMinCount ||
            !double.IsFinite(gridAspectRatio) || gridAspectRatio < 0.0 ||
            !double.IsFinite(gridAmplification) || gridAmplification <= 0.0 ||
            !double.IsFinite(minimumEdgeLength) || minimumEdgeLength < 0.0 ||
            !double.IsFinite(maximumEdgeLength) || maximumEdgeLength < 0.0 ||
            (maximumEdgeLength > 0.0 && maximumEdgeLength < minimumEdgeLength)) {
            validationError = new ValidationError("Mesh fidelity bounds are inconsistent.");
        }
    }

    internal MeshingParameters Mint(Context domain) => new() {
        TextureRange = TextureRange,
        JaggedSeams = Features.Contains(MeshFidelityFeature.JaggedSeams),
        RefineGrid = Features.Contains(MeshFidelityFeature.RefineGrid),
        DoublePrecision = Features.Contains(MeshFidelityFeature.DoublePrecision),
        SimplePlanes = Features.Contains(MeshFidelityFeature.SimplePlanes),
        ComputeCurvature = Features.Contains(MeshFidelityFeature.ComputeCurvature),
        ClosedObjectPostProcess = Features.Contains(MeshFidelityFeature.ClosedObjectPostProcess),
        GridMinCount = GridMinCount,
        GridMaxCount = GridMaxCount,
        GridAngle = domain.Angle.Value,
        GridAspectRatio = GridAspectRatio,
        GridAmplification = GridAmplification,
        Tolerance = domain.Absolute.Value,
        MinimumTolerance = domain.Absolute.Value,
        RelativeTolerance = domain.Fractional,
        MinimumEdgeLength = MinimumEdgeLength,
        MaximumEdgeLength = MaximumEdgeLength,
        RefineAngle = domain.Angle.Value,
    };
}
```

## [03]-[POLICY]

`QuadLaw`, `WrapLaw`, `ReduceLaw`, and `ExtrudeLaw` reject invalid counts and ranges before native configuration exists. Smart-enum rows make native boolean modalities structural. Cancellation and progress belong to `MeshRuntime`, never an operation or policy.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct MeshCount {
    private static ValidationError? Validate(int value) => value > 0
        ? null
        : new ValidationError("Mesh counts must be positive.");
}

[SmartEnum<int>]
public sealed partial class QuadFeature {
    public static readonly QuadFeature AdaptiveCount = new(key: 0);
    public static readonly QuadFeature DetectHardEdges = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class WrapFeature {
    public static readonly WrapFeature FillInputHoles = new(key: 0);
    public static readonly WrapFeature InflatePoints = new(key: 1);
    public static readonly WrapFeature PreserveColors = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class ReduceFeature {
    public static readonly ReduceFeature AllowDistortion = new(key: 0);
    public static readonly ReduceFeature NormalizeSize = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class MeshExtrusionFrame {
    public static readonly MeshExtrusionFrame Transform = new(key: 0, native: (false, false));
    public static readonly MeshExtrusionFrame UVN = new(key: 1, native: (true, false));
    public static readonly MeshExtrusionFrame EdgeUVN = new(key: 2, native: (false, true));

    internal (bool UVN, bool EdgeUVN) Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshOriginalFaces {
    public static readonly MeshOriginalFaces Replace = new(key: 0, native: false);
    public static readonly MeshOriginalFaces Keep = new(key: 1, native: true);

    internal bool Native { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct QuadLaw {
    public int TargetQuadCount { get; }
    public double TargetEdgeLength { get; }
    public double AdaptiveSize { get; }
    public FrozenSet<QuadFeature> Features { get; }
    public int GuideCurveInfluence { get; }
    public int PreserveMeshArrayEdgesMode { get; }
    public QuadRemeshSymmetryAxis Symmetry { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int targetQuadCount,
        ref double targetEdgeLength,
        ref double adaptiveSize,
        ref FrozenSet<QuadFeature> features,
        ref int guideCurveInfluence,
        ref int preserveMeshArrayEdgesMode,
        ref QuadRemeshSymmetryAxis symmetry) {
        if (targetQuadCount <= 0 || !double.IsFinite(targetEdgeLength) || targetEdgeLength < 0.0 ||
            !double.IsFinite(adaptiveSize) || adaptiveSize is < 0.0 or > 100.0) {
            validationError = new ValidationError("Quad-remesh targets are outside the admitted range.");
        }
    }

    internal Fin<QuadRemeshParameters> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new QuadRemeshParameters {
            TargetQuadCount = TargetQuadCount,
            TargetEdgeLength = TargetEdgeLength,
            AdaptiveSize = AdaptiveSize,
            AdaptiveQuadCount = Features.Contains(QuadFeature.AdaptiveCount),
            DetectHardEdges = Features.Contains(QuadFeature.DetectHardEdges),
            GuideCurveInfluence = GuideCurveInfluence,
            PreserveMeshArrayEdgesMode = PreserveMeshArrayEdgesMode,
            SymmetryAxis = Symmetry,
        }));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct WrapLaw {
    public double TargetEdgeLength { get; }
    public double Offset { get; }
    public int SmoothingIterations { get; }
    public FrozenSet<WrapFeature> Features { get; }
    public int PolygonOptimization { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double targetEdgeLength,
        ref double offset,
        ref int smoothingIterations,
        ref FrozenSet<WrapFeature> features,
        ref int polygonOptimization) {
        if (!double.IsFinite(targetEdgeLength) || targetEdgeLength <= 0.0 || !double.IsFinite(offset) ||
            smoothingIterations < 0 || polygonOptimization < 0) {
            validationError = new ValidationError("Shrink-wrap policy requires positive scale and non-negative passes.");
        }
    }

    internal Fin<ShrinkWrapParameters> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new ShrinkWrapParameters {
            TargetEdgeLength = TargetEdgeLength,
            Offset = Offset,
            SmoothingIterations = SmoothingIterations,
            FillHolesInInputObjects = Features.Contains(WrapFeature.FillInputHoles),
            PolygonOptimization = PolygonOptimization,
            InflateVerticesAndPoints = Features.Contains(WrapFeature.InflatePoints),
            PreserveColors = Features.Contains(WrapFeature.PreserveColors),
        }));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ReduceLaw {
    public int DesiredPolygonCount { get; }
    public FrozenSet<ReduceFeature> Features { get; }
    public int Accuracy { get; }
    public Seq<int> FaceTags { get; }
    public Seq<ComponentIndex> LockedComponents { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int desiredPolygonCount,
        ref FrozenSet<ReduceFeature> features,
        ref int accuracy,
        ref Seq<int> faceTags,
        ref Seq<ComponentIndex> lockedComponents) {
        if (desiredPolygonCount <= 0 || accuracy is < 1 or > 10) {
            validationError = new ValidationError("Mesh reduction requires a positive target and bounded accuracy.");
        }
    }

    internal Fin<ReduceMeshParameters> Rig(MeshRuntime runtime, Op key) =>
        key.Catch(() => Fin.Succ(value: new ReduceMeshParameters {
            DesiredPolygonCount = DesiredPolygonCount,
            AllowDistortion = Features.Contains(ReduceFeature.AllowDistortion),
            Accuracy = Accuracy,
            NormalizeMeshSize = Features.Contains(ReduceFeature.NormalizeSize),
            FaceTags = FaceTags.ToArray(),
            LockedComponents = LockedComponents.ToArray(),
            CancelToken = runtime.Cancellation,
            ProgressReporter = runtime.ScalarReporter,
        }));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ExtrudeLaw {
    public Transform Motion { get; }
    public MeshExtrusionFrame Frame { get; }
    public MeshOriginalFaces OriginalFaces { get; }
    public MeshExtruderParameterMode TextureCoordinates { get; }
    public MeshExtruderParameterMode SurfaceParameters { get; }
    public MeshExtruderFaceDirectionMode FaceDirection { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Transform motion,
        ref MeshExtrusionFrame frame,
        ref MeshOriginalFaces originalFaces,
        ref MeshExtruderParameterMode textureCoordinates,
        ref MeshExtruderParameterMode surfaceParameters,
        ref MeshExtruderFaceDirectionMode faceDirection) {
        if (!motion.IsValid || motion.IsZero || frame is null || originalFaces is null) {
            validationError = new ValidationError("Mesh extrusion requires a valid non-zero motion transform and complete frame and face policies.");
        }
    }
}
```

## [04]-[ALGEBRA]

`MeshOp` is the sole construction algebra and `MeshEdit` the sole value-semantic mutation algebra. `MeshRuntime` owns cancellation plus integer and scalar progress. Frozen capability sets carry fidelity, remesh, wrap, reduction, shut-line, smoothing, orientation, edge-matching, and rebuild behavior; native bit products never cross admission. Boolean verdicts, source maps, hull facets, created components, wall faces, and edit tallies remain typed `BuildReceipt<MeshSlot>` evidence.

- Law: struct policies share one owner-local predicate between generated factories and outer operation admission; factory creation rejects invalid values, and the outer seam rejects default ghosts without duplicating rules.
- Growth: a new mesher, engine, or edit verb is one case with its arm; the spine and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshRuntime {
    public Context Domain { get; }
    public CancellationToken Cancellation { get; }
    public Option<IProgress<int>> IntegerProgress { get; }
    public Option<IProgress<double>> ScalarProgress { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Context domain,
        ref CancellationToken cancellation,
        ref Option<IProgress<int>> integerProgress,
        ref Option<IProgress<double>> scalarProgress) {
        if (domain is null) {
            validationError = new ValidationError("Mesh runtime requires a domain context.");
        }
    }

    public static implicit operator Context(MeshRuntime runtime) => runtime.Domain;

    internal IProgress<int>? IntegerReporter => IntegerProgress.IfNoneUnsafe((IProgress<int>?)null);

    internal IProgress<double>? ScalarReporter => ScalarProgress.IfNoneUnsafe((IProgress<double>?)null);

    internal Fin<Built<MeshSlot>> Apply(MeshOp operation, Context _) => operation.Apply(this);
}

[SmartEnum<int>]
public sealed partial class MeshSlot {
    public static readonly MeshSlot Meshed = new(key: 0);
    public static readonly MeshSlot Seeded = new(key: 1);
    public static readonly MeshSlot Remeshed = new(key: 2);
    public static readonly MeshSlot Wrapped = new(key: 3);
    public static readonly MeshSlot Piped = new(key: 4);
    public static readonly MeshSlot Extruded = new(key: 5);
    public static readonly MeshSlot Isosurfaced = new(key: 6);
    public static readonly MeshSlot Networked = new(key: 7);
    public static readonly MeshSlot Hulled = new(key: 8);
    public static readonly MeshSlot Patched = new(key: 9);
    public static readonly MeshSlot Rebuilt = new(key: 10);
    public static readonly MeshSlot Cleaned = new(key: 11);
    public static readonly MeshSlot Subdivided = new(key: 12);
    public static readonly MeshSlot Booled = new(key: 13);
    public static readonly MeshSlot SplitApart = new(key: 14);
    public static readonly MeshSlot EdgeMatched = new(key: 15);
    public static readonly MeshSlot Edited = new(key: 16);
    public static readonly MeshSlot WallFaces = new(key: 17);
    public static readonly MeshSlot Appended = new(key: 18);
    public static readonly MeshSlot Faces = new(key: 19);
    public static readonly MeshSlot Boundaries = new(key: 20);
}

[SmartEnum<int>]
public sealed partial class MeshSmoothAxis {
    public static readonly MeshSmoothAxis X = new(key: 0);
    public static readonly MeshSmoothAxis Y = new(key: 1);
    public static readonly MeshSmoothAxis Z = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class MeshOrientationTarget {
    public static readonly MeshOrientationTarget VertexNormals = new(key: 0);
    public static readonly MeshOrientationTarget FaceNormals = new(key: 1);
    public static readonly MeshOrientationTarget FaceOrientation = new(key: 2);
    public static readonly MeshOrientationTarget NgonBoundaries = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class MeshMatchPolicy {
    public static readonly MeshMatchPolicy SimpleSplits = new(key: 0);
    public static readonly MeshMatchPolicy Ratchet = new(key: 1);
    public static readonly MeshMatchPolicy Average = new(key: 2);
    public static readonly MeshMatchPolicy JoinResult = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class MeshRebuildAttribute {
    public static readonly MeshRebuildAttribute TextureCoordinates = new(key: 0);
    public static readonly MeshRebuildAttribute VertexColors = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class MeshBoundaryMotion {
    public static readonly MeshBoundaryMotion Free = new(key: 0, native: false);
    public static readonly MeshBoundaryMotion Fixed = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshThresholdSide {
    public static readonly MeshThresholdSide Below = new(key: 0, native: false);
    public static readonly MeshThresholdSide Above = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshSplitPolicy {
    public static readonly MeshSplitPolicy Separate = new(key: 0, native: (false, false));
    public static readonly MeshSplitPolicy Coplanar = new(key: 1, native: (true, false));
    public static readonly MeshSplitPolicy Ngons = new(key: 2, native: (false, true));
    public static readonly MeshSplitPolicy CoplanarNgons = new(key: 3, native: (true, true));

    internal (bool Coplanar, bool Ngons) Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshCaps {
    public static readonly MeshCaps None = new(key: 0, native: (false, false));
    public static readonly MeshCaps Bottom = new(key: 1, native: (true, false));
    public static readonly MeshCaps Top = new(key: 2, native: (false, true));
    public static readonly MeshCaps Both = new(key: 3, native: (true, true));

    internal (bool Bottom, bool Top) Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshCircumscription {
    public static readonly MeshCircumscription Inscribed = new(key: 0, native: false);
    public static readonly MeshCircumscription Circumscribed = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshCapFaces {
    public static readonly MeshCapFaces Triangles = new(key: 0, native: false);
    public static readonly MeshCapFaces Quads = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshShell {
    public static readonly MeshShell Open = new(key: 0, native: false);
    public static readonly MeshShell Solid = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshSurfaceParameters {
    public static readonly MeshSurfaceParameters Discard = new(key: 0, native: false);
    public static readonly MeshSurfaceParameters Preserve = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshNormalUpdate {
    public static readonly MeshNormalUpdate Preserve = new(key: 0, native: false);
    public static readonly MeshNormalUpdate Update = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshSelectionScope {
    public static readonly MeshSelectionScope All = new(key: 0, native: false);
    public static readonly MeshSelectionScope Selective = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshFaceting {
    public static readonly MeshFaceting Smooth = new(key: 0, native: false);
    public static readonly MeshFaceting Faceted = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshTextureCoordinates {
    public static readonly MeshTextureCoordinates Omit = new(key: 0, native: false);
    public static readonly MeshTextureCoordinates Include = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshVertexAdmission {
    public static readonly MeshVertexAdmission Fixed = new(key: 0, native: false);
    public static readonly MeshVertexAdmission Extend = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshTrim {
    public static readonly MeshTrim Preserve = new(key: 0, native: false);
    public static readonly MeshTrim Trimmed = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshEdgeSoftenFeature {
    public static readonly MeshEdgeSoftenFeature Chamfer = new(key: 0);
    public static readonly MeshEdgeSoftenFeature Force = new(key: 1);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshEdgeSoftenLaw {
    public double Radius { get; }
    public FrozenSet<MeshEdgeSoftenFeature> Features { get; }
    public MeshFaceting Faceting { get; }
    public double AngleThreshold { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double radius,
        ref FrozenSet<MeshEdgeSoftenFeature> features,
        ref MeshFaceting faceting,
        ref double angleThreshold) {
        if (!double.IsFinite(radius) || radius <= 0.0 || !double.IsFinite(angleThreshold) || angleThreshold < 0.0) {
            validationError = new ValidationError("Mesh edge softening requires a finite positive radius and finite non-negative angle threshold.");
        }
    }
}

[SmartEnum<int>]
public sealed partial class MeshNakedMatch {
    public static readonly MeshNakedMatch Direct = new(key: 0, native: false);
    public static readonly MeshNakedMatch Ratchet = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class MeshCountMode {
    public static readonly MeshCountMode Faces = new(key: 0, native: (false, false));
    public static readonly MeshCountMode Triangles = new(key: 1, native: (false, true));
    public static readonly MeshCountMode SumFaces = new(key: 2, native: (true, false));
    public static readonly MeshCountMode SumTriangles = new(key: 3, native: (true, true));

    internal (bool CountSum, bool CountTriangles) Native { get; }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshSmoothLaw {
    public double Factor { get; }
    public int Steps { get; }
    public FrozenSet<MeshSmoothAxis> Axes { get; }
    public MeshBoundaryMotion Boundaries { get; }
    public SmoothingCoordinateSystem System { get; }
    public Plane Frame { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double factor,
        ref int steps,
        ref FrozenSet<MeshSmoothAxis> axes,
        ref MeshBoundaryMotion boundaries,
        ref SmoothingCoordinateSystem system,
        ref Plane frame) {
        if (!Admits(factor, steps, axes, boundaries, system, frame)) {
            validationError = new ValidationError(
                "Mesh smoothing requires a finite factor, positive passes, axes, boundary policy, coordinate system, and valid frame.");
        }
    }

    internal bool Admissible => Admits(Factor, Steps, Axes, Boundaries, System, Frame);

    internal bool Apply(Mesh target, Option<Seq<int>> vertices = default) =>
        vertices.Case switch {
            Seq<int> selected => target.Smooth(
                vertexIndices: selected.AsIterable(), smoothFactor: Factor, numSteps: Steps,
                bXSmooth: Axes.Contains(MeshSmoothAxis.X), bYSmooth: Axes.Contains(MeshSmoothAxis.Y),
                bZSmooth: Axes.Contains(MeshSmoothAxis.Z), bFixBoundaries: Boundaries.Native,
                coordinateSystem: System, plane: Frame),
            _ => target.Smooth(
                smoothFactor: Factor, numSteps: Steps,
                bXSmooth: Axes.Contains(MeshSmoothAxis.X), bYSmooth: Axes.Contains(MeshSmoothAxis.Y),
                bZSmooth: Axes.Contains(MeshSmoothAxis.Z), bFixBoundaries: Boundaries.Native,
                coordinateSystem: System, plane: Frame),
        };

    private static bool Admits(
        double factor,
        int steps,
        FrozenSet<MeshSmoothAxis>? axes,
        MeshBoundaryMotion? boundaries,
        SmoothingCoordinateSystem system,
        Plane frame) =>
        double.IsFinite(factor)
        && steps > 0
        && axes is { Count: > 0 }
        && boundaries is not null
        && Enum.IsDefined(system)
        && frame.IsValid;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshOrientationLaw {
    public FrozenSet<MeshOrientationTarget> Targets { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenSet<MeshOrientationTarget> targets) {
        if (targets.Count == 0) {
            validationError = new ValidationError("Mesh orientation requires at least one target.");
        }
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshMatchLaw {
    public double Distance { get; }
    public FrozenSet<MeshMatchPolicy> Capabilities { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double distance,
        ref FrozenSet<MeshMatchPolicy> capabilities) {
        if (!Admits(distance, capabilities)) {
            validationError = new ValidationError("Mesh edge matching requires a finite positive distance and a capability set.");
        }
    }

    internal bool Admissible => Admits(Distance, Capabilities);

    private static bool Admits(double distance, FrozenSet<MeshMatchPolicy>? capabilities) =>
        double.IsFinite(distance) && distance > 0.0 && capabilities is not null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshEdit {
    private MeshEdit() { }
    public sealed record Reduce(ReduceLaw Law) : MeshEdit;
    public sealed record Weld(MeshSurfaceParameters SurfaceParameters) : MeshEdit;
    public sealed record Unweld(MeshNormalUpdate Normals) : MeshEdit;
    public sealed record UnweldEdges(Seq<int> Edges, MeshNormalUpdate Normals) : MeshEdit;
    public sealed record UnweldVertices(Seq<int> TopologyVertices, MeshNormalUpdate Normals) : MeshEdit;
    public sealed record Offset(double Distance, MeshShell Shell) : MeshEdit;
    public sealed record OffsetDirection(double Distance, MeshShell Shell, Vector3d Direction) : MeshEdit;
    public sealed record Heal(double Distance) : MeshEdit;
    public sealed record FillHoles : MeshEdit;
    public sealed record FillHole(int TopologyEdge) : MeshEdit;
    public sealed record MatchNaked(double Distance, MeshNakedMatch Mode) : MeshEdit;
    public sealed record MergeCoplanar : MeshEdit;
    public sealed record Smooth(MeshSmoothLaw Law) : MeshEdit;
    public sealed record SmoothVertices(Seq<int> Vertices, MeshSmoothLaw Law) : MeshEdit;
    public sealed record CollapseEdgeLength(MeshThresholdSide Side, double EdgeLength) : MeshEdit;
    public sealed record CollapseArea(double LessThanArea, double GreaterThanArea) : MeshEdit;
    public sealed record CollapseAspectRatio(double Value) : MeshEdit;
    public sealed record RebuildNormals : MeshEdit;
    public sealed record UnifyNormals : MeshEdit;
    public sealed record Orient(MeshOrientationLaw Law) : MeshEdit;
    public sealed record Compact : MeshEdit;
    public sealed record ExtractNonManifold(MeshSelectionScope Scope) : MeshEdit;
    public sealed record EdgeSoften(MeshEdgeSoftenLaw Law) : MeshEdit;
    public sealed record ShutLine(Seq<ShutLineProfile> Profiles, MeshFaceting Faceting) : MeshEdit;
    public sealed record Displace(DisplacementLaw Law) : MeshEdit;

    internal bool Admissible => this switch {
        Reduce edit => edit.Law.DesiredPolygonCount > 0
            && edit.Law.Features is not null
            && edit.Law.Accuracy is >= 1 and <= 10
            && edit.Law.LockedComponents.ForAll(static component => component.Index >= 0),
        Weld edit => edit.SurfaceParameters is not null,
        Unweld edit => edit.Normals is not null,
        UnweldEdges edit => !edit.Edges.IsEmpty && edit.Edges.ForAll(static edge => edge >= 0) && edit.Normals is not null,
        UnweldVertices edit => !edit.TopologyVertices.IsEmpty
            && edit.TopologyVertices.ForAll(static vertex => vertex >= 0)
            && edit.Normals is not null,
        Offset edit => Positive(edit.Distance) && edit.Shell is not null,
        OffsetDirection edit => Positive(edit.Distance) && edit.Shell is not null && edit.Direction.IsValid && !edit.Direction.IsZero,
        Heal edit => Positive(edit.Distance),
        FillHoles => true,
        FillHole edit => edit.TopologyEdge >= 0,
        MatchNaked edit => Positive(edit.Distance) && edit.Mode is not null,
        MergeCoplanar => true,
        Smooth edit => edit.Law.Admissible,
        SmoothVertices edit => !edit.Vertices.IsEmpty
            && edit.Vertices.ForAll(static vertex => vertex >= 0)
            && edit.Law.Admissible,
        CollapseEdgeLength edit => edit.Side is not null && Positive(edit.EdgeLength),
        CollapseArea edit => Positive(edit.LessThanArea) && Positive(edit.GreaterThanArea),
        CollapseAspectRatio edit => Positive(edit.Value),
        RebuildNormals or UnifyNormals => true,
        Orient edit => edit.Law.Targets is { Count: > 0 },
        Compact => true,
        ExtractNonManifold edit => edit.Scope is not null,
        EdgeSoften edit => Positive(edit.Law.Radius)
            && edit.Law.Features is not null
            && edit.Law.Faceting is not null
            && double.IsFinite(edit.Law.AngleThreshold)
            && edit.Law.AngleThreshold >= 0.0,
        ShutLine edit => !edit.Profiles.IsEmpty
            && edit.Faceting is not null
            && edit.Profiles.ForAll(static profile => profile.Admissible),
        Displace edit => edit.Law.Texture is not null
            && edit.Law.Mapping is not null
            && double.IsFinite(edit.Law.Black)
            && double.IsFinite(edit.Law.White)
            && edit.Law.White > edit.Law.Black
            && double.IsFinite(edit.Law.BlackMove)
            && double.IsFinite(edit.Law.WhiteMove)
            && double.IsFinite(edit.Law.PostWeldAngle)
            && double.IsFinite(edit.Law.RefineSensitivity)
            && edit.Law.RefineSensitivity is >= 0.0 and <= 1.0
            && Positive(edit.Law.SweepPitch)
            && edit.Law.ChannelNumber > 0
            && edit.Law.FaceLimit > 0
            && edit.Law.FairingAmount >= 0
            && edit.Law.RefineStepCount >= 0
            && edit.Law.MemoryLimit > 0
            && edit.Law.MappingTransform.ForAll(static transform => transform.IsValid)
            && edit.Law.InstanceTransform.ForAll(static transform => transform.IsValid),
        _ => false,
    };

    private static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
}

[SmartEnum<int>]
public sealed partial class ShutLineFeature {
    public static readonly ShutLineFeature Pull = new(key: 0);
    public static readonly ShutLineFeature Bump = new(key: 1);
    public static readonly ShutLineFeature Enabled = new(key: 2);
}

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ShutLineProfile {
    public GeometryHandle Curve { get; }
    public double Radius { get; }
    public int Profile { get; }
    public FrozenSet<ShutLineFeature> Features { get; }
    public Seq<Interval> Intervals { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref GeometryHandle curve,
        ref double radius,
        ref int profile,
        ref FrozenSet<ShutLineFeature> features,
        ref Seq<Interval> intervals) {
        if (!Admits(curve, radius, profile, features, intervals)) {
            validationError = new ValidationError(
                "Shut-line profiles require a curve, finite positive radius, profile row, features, and valid intervals.");
        }
    }

    internal bool Admissible => Admits(Curve, Radius, Profile, Features, Intervals);

    private static bool Admits(
        GeometryHandle? curve,
        double radius,
        int profile,
        FrozenSet<ShutLineFeature>? features,
        Seq<Interval> intervals) =>
        curve is not null
        && double.IsFinite(radius)
        && radius > 0.0
        && profile >= 0
        && features is not null
        && intervals.ForAll(static interval => interval.IsValid);

    internal ShutLiningCurveInfo Rig(Curve curve) =>
        new(curve: curve, radius: Radius, profile: Profile,
            pull: Features.Contains(ShutLineFeature.Pull),
            isBump: Features.Contains(ShutLineFeature.Bump),
            curveIntervals: Intervals.ToArray(), enabled: Features.Contains(ShutLineFeature.Enabled));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DisplacementLaw {
    public RenderTexture Texture { get; }
    public TextureMapping Mapping { get; }
    public double Black { get; }
    public double White { get; }
    public double BlackMove { get; }
    public double WhiteMove { get; }
    public Option<Transform> MappingTransform { get; }
    public Option<Transform> InstanceTransform { get; }
    public double PostWeldAngle { get; }
    public double RefineSensitivity { get; }
    public double SweepPitch { get; }
    public int ChannelNumber { get; }
    public int FaceLimit { get; }
    public int FairingAmount { get; }
    public int RefineStepCount { get; }
    public int MemoryLimit { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref RenderTexture texture,
        ref TextureMapping mapping,
        ref double black,
        ref double white,
        ref double blackMove,
        ref double whiteMove,
        ref Option<Transform> mappingTransform,
        ref Option<Transform> instanceTransform,
        ref double postWeldAngle,
        ref double refineSensitivity,
        ref double sweepPitch,
        ref int channelNumber,
        ref int faceLimit,
        ref int fairingAmount,
        ref int refineStepCount,
        ref int memoryLimit) {
        if (!double.IsFinite(black) || !double.IsFinite(white) || white <= black ||
            !double.IsFinite(blackMove) || !double.IsFinite(whiteMove) || !double.IsFinite(postWeldAngle) ||
            !double.IsFinite(refineSensitivity) || refineSensitivity is < 0.0 or > 1.0 ||
            !double.IsFinite(sweepPitch) || sweepPitch <= 0.0 ||
            channelNumber <= 0 || faceLimit <= 0 || fairingAmount < 0 || refineStepCount < 0 || memoryLimit <= 0) {
            validationError = new ValidationError("Displacement bounds are inconsistent.");
        }
    }

    internal MeshDisplacementInfo Rig() => new(texture: Texture, mapping: Mapping) {
        Black = Black, White = White, BlackMove = BlackMove, WhiteMove = WhiteMove,
        MappingTransform = MappingTransform.IfNone(Transform.Identity),
        InstanceTransform = InstanceTransform.IfNone(Transform.Identity),
        PostWeldAngle = PostWeldAngle, RefineSensitivity = RefineSensitivity, SweepPitch = SweepPitch,
        ChannelNumber = ChannelNumber, FaceLimit = FaceLimit, FairingAmount = FairingAmount,
        RefineStepCount = RefineStepCount, MemoryLimit = MemoryLimit,
    };
}

[ComplexValueObject]
public sealed partial class ClosedPolyline {
    public Seq<Point3d> Points { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<Point3d> points) {
        Polyline boundary = new(collection: points.AsIterable());
        if (points.Count < 4 || !points.ForAll(static point => point.IsValid) || !boundary.IsClosed) {
            validationError = new ValidationError("Closed polylines require four valid points and matching endpoints.");
        }
    }

    internal Polyline Native => new(collection: Points.AsIterable());
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshOp {
    private MeshOp() { }
    public sealed record FromGeometry(GeometryHandle Source, MeshFidelity Fidelity) : MeshOp;
    public sealed record FromSubD(GeometryHandle Source, SubDDisplayParameters.Density Level) : MeshOp;
    public sealed record Cage(GeometryHandle Source, MeshTextureCoordinates TextureCoordinates) : MeshOp;
    public sealed record FromBoundary(GeometryHandle Boundary, MeshFidelity Fidelity) : MeshOp;
    public sealed record SeedPlane(Plane Frame, Interval X, Interval Y, MeshCount XCount, MeshCount YCount) : MeshOp;
    public sealed record SeedBox(Box Box, MeshCount XCount, MeshCount YCount, MeshCount ZCount) : MeshOp;
    public sealed record SeedSphere(Sphere Sphere, MeshCount XCount, MeshCount YCount) : MeshOp;
    public sealed record SeedIcoSphere(Sphere Sphere, MeshCount Subdivisions) : MeshOp;
    public sealed record SeedQuadSphere(Sphere Sphere, MeshCount Subdivisions) : MeshOp;
    public sealed record SeedCylinder(Cylinder Cylinder, MeshCount Vertical, MeshCount Around, MeshCaps Caps, MeshCircumscription Circumscription, MeshCapFaces CapFaces) : MeshOp;
    public sealed record SeedCone(Cone Cone, MeshCount Vertical, MeshCount Around, MeshShell Shell, MeshCapFaces CapFaces) : MeshOp;
    public sealed record SeedTorus(Torus Torus, MeshCount Vertical, MeshCount Around) : MeshOp;
    public sealed record SeedClosedPolyline(ClosedPolyline Boundary) : MeshOp;
    public sealed record QuadRemesh(GeometryHandle Source, QuadLaw Law, Seq<GeometryHandle> Guides, Seq<int> FaceBlocks = default) : MeshOp;
    public sealed record Wrap(Seq<GeometryHandle> Sources, WrapLaw Law, Option<MeshFidelity> Fidelity = default) : MeshOp;
    public sealed record CurvePipe(GeometryHandle Curve, double Radius, MeshCount Segments, int Accuracy, MeshPipeCapStyle Cap, MeshFaceting Faceting, Seq<Interval> Intervals = default) : MeshOp;
    public sealed record CurveExtrude(GeometryHandle Curve, Vector3d Direction, Option<MeshFidelity> Fidelity = default, Option<BoundingBox> Bounds = default) : MeshOp;
    public sealed record Isosurface(Func<Point3d, double> Field, BoundingBox Box, MeshCount Resolution, int RootFindingMaxSteps) : MeshOp;
    public sealed record FromLines(Seq<GeometryHandle> Lines, int MaxFaceValence) : MeshOp;
    public sealed record Tessellate(Seq<Point3d> Points, Seq<Seq<Point3d>> Edges, Plane Frame, MeshVertexAdmission VertexAdmission) : MeshOp;
    public sealed record ConvexHull(Seq<Point3d> Points) : MeshOp;
    public sealed record Patch(Seq<Point3d> OuterBoundary, Option<GeometryHandle> PullbackSurface, Seq<GeometryHandle> InnerBoundaries, Seq<GeometryHandle> BothSideCurves, Seq<Point3d> InnerPoints, MeshTrim Trim, int Divisions) : MeshOp;
    public sealed record Rebuild(GeometryHandle Source, FrozenSet<MeshRebuildAttribute> Attributes) : MeshOp;
    public sealed record Cleanup(Seq<GeometryHandle> Sources) : MeshOp;
    public sealed record RefineLoop(GeometryHandle Source, MeshRefinements.LoopFormula Formula, MeshCount Level, MeshRefinements.CreaseEdges NakedEdges) : MeshOp;
    public sealed record RefineCatmullClark(GeometryHandle Source, MeshCount Level, MeshRefinements.CreaseEdges NakedEdges) : MeshOp;
    public sealed record SubdivideMidEdge(GeometryHandle Source, Seq<int> Faces) : MeshOp;
    public sealed record BooleanUnion(Seq<GeometryHandle> Inputs) : MeshOp;
    public sealed record BooleanIntersection(Seq<GeometryHandle> First, Seq<GeometryHandle> Second) : MeshOp;
    public sealed record BooleanDifference(Seq<GeometryHandle> First, Seq<GeometryHandle> Second) : MeshOp;
    public sealed record BooleanSplit(Seq<GeometryHandle> Targets, Seq<GeometryHandle> Cutters) : MeshOp;
    public sealed record SplitPlane(GeometryHandle Target, Plane Plane) : MeshOp;
    public sealed record SplitMeshes(GeometryHandle Target, Seq<GeometryHandle> Cutters, MeshSplitPolicy Policy) : MeshOp;
    public sealed record SplitDisjoint(GeometryHandle Target) : MeshOp;
    public sealed record SplitNonManifold(GeometryHandle Target) : MeshOp;
    public sealed record SplitProjectedPolylines(GeometryHandle Target, Seq<GeometryHandle> Curves) : MeshOp;
    public sealed record SplitUnweldedEdges(GeometryHandle Target) : MeshOp;
    public sealed record SplitCount(GeometryHandle Target, int MaxCount, MeshCountMode Mode) : MeshOp;
    public sealed record Partition(GeometryHandle Target, int MaxVertexCount, int MaxFaceCount) : MeshOp;
    public sealed record MatchEdges(Seq<GeometryHandle> Targets, MeshMatchLaw Law) : MeshOp;
    public sealed record Append(Seq<GeometryHandle> Sources) : MeshOp;
    public sealed record ProjectFaces(GeometryHandle Target, Seq<int> Indices) : MeshOp;
    public sealed record ProjectNakedEdges(GeometryHandle Target) : MeshOp;
    public sealed record ProjectOutlines(GeometryHandle Target, Plane Frame) : MeshOp;
    public sealed record Edit(GeometryHandle Target, MeshEdit Verb) : MeshOp;
    public sealed record Extrude(GeometryHandle Target, Seq<ComponentIndex> Components, ExtrudeLaw Law) : MeshOp;

    internal Fin<MeshOp> Admitted(Op key) =>
        guard(this switch {
            FromGeometry edit => edit.Source is not null && FidelityAdmissible(edit.Fidelity),
            FromSubD edit => edit.Source is not null,
            Cage edit => edit.Source is not null && edit.TextureCoordinates is not null,
            FromBoundary edit => edit.Boundary is not null && FidelityAdmissible(edit.Fidelity),
            SeedPlane edit => edit.Frame.IsValid && edit.X.IsValid && edit.Y.IsValid
                && CountAdmissible(edit.XCount) && CountAdmissible(edit.YCount),
            SeedBox edit => edit.Box.IsValid && CountAdmissible(edit.XCount)
                && CountAdmissible(edit.YCount) && CountAdmissible(edit.ZCount),
            SeedSphere edit => edit.Sphere.IsValid && CountAdmissible(edit.XCount) && CountAdmissible(edit.YCount),
            SeedIcoSphere edit => edit.Sphere.IsValid && CountAdmissible(edit.Subdivisions),
            SeedQuadSphere edit => edit.Sphere.IsValid && CountAdmissible(edit.Subdivisions),
            SeedCylinder edit => edit.Cylinder.IsValid && CountAdmissible(edit.Vertical) && CountAdmissible(edit.Around)
                && edit.Caps is not null && edit.Circumscription is not null && edit.CapFaces is not null,
            SeedCone edit => edit.Cone.IsValid && CountAdmissible(edit.Vertical) && CountAdmissible(edit.Around)
                && edit.Shell is not null && edit.CapFaces is not null,
            SeedTorus edit => edit.Torus.IsValid && CountAdmissible(edit.Vertical) && CountAdmissible(edit.Around),
            SeedClosedPolyline edit => edit.Boundary is not null,
            QuadRemesh edit => edit.Source is not null && QuadAdmissible(edit.Law)
                && Handles(edit.Guides, allowEmpty: true)
                && edit.FaceBlocks.ForAll(static face => face >= 0),
            Wrap edit => Handles(edit.Sources) && WrapAdmissible(edit.Law)
                && edit.Fidelity.ForAll(static fidelity => FidelityAdmissible(fidelity)),
            CurvePipe edit => edit.Curve is not null && Positive(edit.Radius) && CountAdmissible(edit.Segments)
                && edit.Accuracy > 0 && edit.Faceting is not null
                && edit.Intervals.ForAll(static interval => interval.IsValid),
            CurveExtrude edit => edit.Curve is not null && edit.Direction.IsValid && !edit.Direction.IsZero
                && edit.Fidelity.ForAll(static fidelity => FidelityAdmissible(fidelity))
                && edit.Bounds.ForAll(static bounds => bounds.IsValid),
            Isosurface edit => edit.Field is not null && edit.Box.IsValid
                && CountAdmissible(edit.Resolution) && edit.RootFindingMaxSteps > 0,
            FromLines edit => Handles(edit.Lines) && edit.MaxFaceValence >= 3,
            Tessellate edit => Points(edit.Points)
                && !edit.Edges.IsEmpty
                && edit.Edges.ForAll(static edge => Points(edge))
                && edit.Frame.IsValid
                && edit.VertexAdmission is not null,
            ConvexHull edit => edit.Points.Count >= 4 && edit.Points.ForAll(static point => point.IsValid),
            Patch edit => edit.OuterBoundary.Count >= 4
                && edit.OuterBoundary.ForAll(static point => point.IsValid)
                && edit.PullbackSurface.ForAll(static handle => handle is not null)
                && Handles(edit.InnerBoundaries, allowEmpty: true)
                && Handles(edit.BothSideCurves, allowEmpty: true)
                && edit.InnerPoints.ForAll(static point => point.IsValid)
                && edit.Trim is not null
                && edit.Divisions > 0,
            Rebuild edit => edit.Source is not null && edit.Attributes is not null,
            Cleanup edit => Handles(edit.Sources),
            RefineLoop edit => edit.Source is not null && CountAdmissible(edit.Level),
            RefineCatmullClark edit => edit.Source is not null && CountAdmissible(edit.Level),
            SubdivideMidEdge edit => edit.Source is not null && edit.Faces.ForAll(static face => face >= 0),
            BooleanUnion edit => Handles(edit.Inputs),
            BooleanIntersection edit => Handles(edit.First) && Handles(edit.Second),
            BooleanDifference edit => Handles(edit.First) && Handles(edit.Second),
            BooleanSplit edit => Handles(edit.Targets) && Handles(edit.Cutters),
            SplitPlane edit => edit.Target is not null && edit.Plane.IsValid,
            SplitMeshes edit => edit.Target is not null && Handles(edit.Cutters) && edit.Policy is not null,
            SplitDisjoint edit => edit.Target is not null,
            SplitNonManifold edit => edit.Target is not null,
            SplitProjectedPolylines edit => edit.Target is not null && Handles(edit.Curves),
            SplitUnweldedEdges edit => edit.Target is not null,
            SplitCount edit => edit.Target is not null && edit.MaxCount > 0 && edit.Mode is not null,
            Partition edit => edit.Target is not null && edit.MaxVertexCount > 0 && edit.MaxFaceCount > 0,
            MatchEdges edit => Handles(edit.Targets) && edit.Law.Admissible,
            Append edit => Handles(edit.Sources),
            ProjectFaces edit => edit.Target is not null && !edit.Indices.IsEmpty
                && edit.Indices.ForAll(static index => index >= 0),
            ProjectNakedEdges edit => edit.Target is not null,
            ProjectOutlines edit => edit.Target is not null && edit.Frame.IsValid,
            Edit edit => edit.Target is not null && edit.Verb is { Admissible: true },
            Extrude edit => edit.Target is not null
                && !edit.Components.IsEmpty
                && edit.Components.ForAll(static component => component.Index >= 0)
                && ExtrudeAdmissible(edit.Law),
            _ => false,
        }, key.InvalidInput()).ToFin().Map(_ => this);

    private static bool FidelityAdmissible(MeshFidelity? fidelity) => fidelity switch {
        MeshFidelity.Preset { Row: not null } => true,
        MeshFidelity.Density edit => double.IsFinite(edit.Value.Value)
            && edit.Value.Value is >= 0.0 and <= 1.0
            && edit.Value.MinimumEdgeLength.ForAll(static length => double.IsFinite(length) && length >= 0.0),
        MeshFidelity.Custom edit => edit.Law.Features is not null
            && edit.Law.GridMinCount >= 0
            && edit.Law.GridMaxCount >= edit.Law.GridMinCount
            && double.IsFinite(edit.Law.GridAspectRatio)
            && edit.Law.GridAspectRatio >= 0.0
            && Positive(edit.Law.GridAmplification)
            && double.IsFinite(edit.Law.MinimumEdgeLength)
            && edit.Law.MinimumEdgeLength >= 0.0
            && double.IsFinite(edit.Law.MaximumEdgeLength)
            && edit.Law.MaximumEdgeLength >= 0.0
            && (edit.Law.MaximumEdgeLength == 0.0 || edit.Law.MaximumEdgeLength >= edit.Law.MinimumEdgeLength),
        _ => false,
    };

    private static bool QuadAdmissible(QuadLaw law) => law.TargetQuadCount > 0
        && double.IsFinite(law.TargetEdgeLength)
        && law.TargetEdgeLength >= 0.0
        && double.IsFinite(law.AdaptiveSize)
        && law.AdaptiveSize is >= 0.0 and <= 100.0
        && law.Features is not null;

    private static bool WrapAdmissible(WrapLaw law) => Positive(law.TargetEdgeLength)
        && double.IsFinite(law.Offset)
        && law.SmoothingIterations >= 0
        && law.Features is not null
        && law.PolygonOptimization >= 0;

    private static bool ExtrudeAdmissible(ExtrudeLaw law) => law.Motion.IsValid
        && !law.Motion.IsZero
        && law.Frame is not null
        && law.OriginalFaces is not null;

    private static bool CountAdmissible(MeshCount count) => count.Value > 0;

    private static bool Handles(Seq<GeometryHandle> handles, bool allowEmpty = false) =>
        (allowEmpty || !handles.IsEmpty) && handles.ForAll(static handle => handle is not null);

    private static bool Points(Seq<Point3d> points) =>
        !points.IsEmpty && points.ForAll(static point => point.IsValid);

    private static bool Positive(double value) => double.IsFinite(value) && value > 0.0;

    internal Fin<Built<MeshSlot>> Apply(MeshRuntime runtime) =>
        Switch(
            runtime,
            fromGeometry: static (model, edit) => {
                Op op = Op.Of(name: nameof(FromGeometry));
                return ModelGate.Borrow<GeometryBase, Built<MeshSlot>>(handle: edit.Source, key: op, body: source =>
                    from parameters in edit.Fidelity.Rig(domain: model, key: op)
                    from built in op.Catch(() => {
                        using MeshingParameters live = parameters;
                        return source switch {
                            Brep brep => ModelGate.Many(op, MeshSlot.Meshed, () => Mesh.CreateFromBrep(brep: brep, meshingParameters: live)),
                            Surface surface => ModelGate.Single(op, MeshSlot.Meshed, () => Mesh.CreateFromSurface(surface: surface, meshingParameters: live)),
                            Extrusion extrusion => ModelGate.Single(op, MeshSlot.Meshed, () => Mesh.CreateFromExtrusion(extrusion: extrusion, meshingParameters: live)),
                            _ => Fin.Fail<Built<MeshSlot>>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Mesh))),
                        };
                    })
                    select built);
            },
            fromSubD: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromSubD));
                return ModelGate.Borrow<SubD, Built<MeshSlot>>(handle: edit.Source, key: op, body: subd =>
                    ModelGate.Single(op, MeshSlot.Meshed, () => Mesh.CreateFromSubD(subd: subd, displayDensity: edit.Level)));
            },
            cage: static (_, edit) => {
                Op op = Op.Of(name: nameof(Cage));
                return ModelGate.Borrow<GeometryBase, Built<MeshSlot>>(handle: edit.Source, key: op, body: source =>
                    source switch {
                        SubD subd => ModelGate.Single(op, MeshSlot.Meshed, () => edit.TextureCoordinates.Native
                            ? Mesh.CreateFromSubDControlNetWithTextureCoordinates(subd: subd)
                            : Mesh.CreateFromSubDControlNet(subd: subd)),
                        Surface surface => ModelGate.Single(op, MeshSlot.Meshed, () => Mesh.CreateFromSurfaceControlNet(surface: surface)),
                        _ => Fin.Fail<Built<MeshSlot>>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Mesh))),
                    });
            },
            fromBoundary: static (model, edit) => {
                Op op = Op.Of(name: nameof(FromBoundary));
                return ModelGate.Borrow<Curve, Built<MeshSlot>>(handle: edit.Boundary, key: op, body: boundary =>
                    from parameters in edit.Fidelity.Rig(domain: model, key: op)
                    from built in op.Catch(() => {
                        using MeshingParameters live = parameters;
                        return ModelGate.Single(op, MeshSlot.Meshed, () => Mesh.CreateFromPlanarBoundary(
                            boundary: boundary, parameters: live, tolerance: model.Domain.Absolute.Value));
                    })
                    select built);
            },
            seedPlane: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedPlane));
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateFromPlane(
                    plane: edit.Frame,
                    xInterval: edit.X,
                    yInterval: edit.Y,
                    xCount: edit.XCount.Value,
                    yCount: edit.YCount.Value));
            },
            seedBox: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedBox));
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateFromBox(
                    box: edit.Box,
                    xCount: edit.XCount.Value,
                    yCount: edit.YCount.Value,
                    zCount: edit.ZCount.Value));
            },
            seedSphere: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedSphere));
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateFromSphere(
                    sphere: edit.Sphere,
                    xCount: edit.XCount.Value,
                    yCount: edit.YCount.Value));
            },
            seedIcoSphere: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedIcoSphere));
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateIcoSphere(
                    sphere: edit.Sphere,
                    subdivisions: edit.Subdivisions.Value));
            },
            seedQuadSphere: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedQuadSphere));
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateQuadSphere(
                    sphere: edit.Sphere,
                    subdivisions: edit.Subdivisions.Value));
            },
            seedCylinder: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedCylinder));
                (bool bottom, bool top) = edit.Caps.Native;
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateFromCylinder(
                    cylinder: edit.Cylinder,
                    vertical: edit.Vertical.Value,
                    around: edit.Around.Value,
                    capBottom: bottom,
                    capTop: top,
                    circumscribe: edit.Circumscription.Native,
                    quadCaps: edit.CapFaces.Native));
            },
            seedCone: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedCone));
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateFromCone(
                    cone: edit.Cone,
                    vertical: edit.Vertical.Value,
                    around: edit.Around.Value,
                    solid: edit.Shell.Native,
                    quadCaps: edit.CapFaces.Native));
            },
            seedTorus: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedTorus));
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateFromTorus(
                    torus: edit.Torus,
                    vertical: edit.Vertical.Value,
                    around: edit.Around.Value));
            },
            seedClosedPolyline: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedClosedPolyline));
                return ModelGate.Single(op, MeshSlot.Seeded, () => Mesh.CreateFromClosedPolyline(
                    polyline: edit.Boundary.Native));
            },
            quadRemesh: static (model, edit) => {
                Op op = Op.Of(name: nameof(QuadRemesh));
                return ModelGate.Borrow<GeometryBase, Built<MeshSlot>>(handle: edit.Source, key: op, body: source =>
                    ModelGate.BorrowMany<Curve, Built<MeshSlot>>(handles: edit.Guides, key: op, allowEmpty: true, body: guides =>
                        from parameters in edit.Law.Rig(key: op)
                        from built in (source switch {
                            Brep brep => ModelGate.Single(op, MeshSlot.Remeshed, () => Mesh.QuadRemeshBrep(
                                brep: brep, parameters: parameters, guideCurves: guides.AsIterable(),
                                progress: model.IntegerReporter, cancelToken: model.Cancellation)),
                            Mesh mesh when !edit.FaceBlocks.IsEmpty => ModelGate.Single(op, MeshSlot.Remeshed, () => mesh.QuadRemesh(
                                faceBlocks: edit.FaceBlocks.AsIterable(), parameters: parameters, guideCurves: guides.AsIterable(),
                                progress: model.IntegerReporter, cancelToken: model.Cancellation)),
                            Mesh mesh when model.IntegerProgress.IsSome || model.Cancellation.CanBeCanceled => ModelGate.Single(
                                op, MeshSlot.Remeshed, () => mesh.QuadRemesh(
                                    faceBlocks: System.Linq.Enumerable.Range(0, mesh.Faces.Count),
                                    parameters: parameters, guideCurves: guides.AsIterable(),
                                    progress: model.IntegerReporter, cancelToken: model.Cancellation)),
                            Mesh mesh => ModelGate.Single(op, MeshSlot.Remeshed, () => mesh.QuadRemesh(
                                parameters: parameters, guideCurves: guides.AsIterable())),
                            _ => Fin.Fail<Built<MeshSlot>>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Mesh))),
                        })
                        select built));
            },
            wrap: static (model, edit) => {
                Op op = Op.Of(name: nameof(Wrap));
                return ModelGate.BorrowMany<GeometryBase, Built<MeshSlot>>(handles: edit.Sources, key: op, body: sources =>
                    from parameters in edit.Law.Rig(key: op)
                    from built in op.Catch(() => (
                        AllMeshes: sources.ForAll(static value => value is Mesh),
                        Cloud: sources.Count == 1 ? sources[0] as PointCloud : null,
                        Fidelity: edit.Fidelity.Case) switch {
                        (true, _, null) => ModelGate.Single(op, MeshSlot.Wrapped, () => Mesh.ShrinkWrap(
                            meshes: sources.Map(static value => (Mesh)value).AsIterable(),
                            parameters: parameters,
                            token: model.Cancellation)),
                        (true, _, _) => Fin.Fail<Built<MeshSlot>>(error: op.InvalidInput()),
                        (false, PointCloud cloud, null) => ModelGate.Single(op, MeshSlot.Wrapped, () => Mesh.ShrinkWrap(
                            pointCloud: cloud,
                            parameters: parameters,
                            token: model.Cancellation)),
                        (false, PointCloud, _) => Fin.Fail<Built<MeshSlot>>(error: op.InvalidInput()),
                        (false, _, MeshFidelity fidelity) => fidelity.Rig(domain: model, key: op).Bind(meshing => op.Catch(() => {
                            using MeshingParameters live = meshing;
                            return ModelGate.Single(op, MeshSlot.Wrapped, () => Mesh.ShrinkWrap(
                                geometryBases: sources.AsIterable(),
                                parameters: parameters,
                                meshingParameters: live,
                                token: model.Cancellation));
                        })),
                        _ => Fin.Fail<Built<MeshSlot>>(error: op.MissingContext()),
                    })
                    select built);
            },
            curvePipe: static (_, edit) => {
                Op op = Op.Of(name: nameof(CurvePipe));
                return ModelGate.Borrow<Curve, Built<MeshSlot>>(handle: edit.Curve, key: op, body: curve =>
                    ModelGate.Single(op, MeshSlot.Piped, () => Mesh.CreateFromCurvePipe(
                        curve: curve, radius: edit.Radius, segments: edit.Segments.Value, accuracy: edit.Accuracy,
                        capType: edit.Cap, faceted: edit.Faceting.Native,
                        intervals: edit.Intervals.IsEmpty ? null : edit.Intervals.AsIterable())));
            },
            curveExtrude: static (model, edit) => {
                Op op = Op.Of(name: nameof(CurveExtrude));
                return ModelGate.Borrow<Curve, Built<MeshSlot>>(handle: edit.Curve, key: op, body: curve =>
                    edit.Fidelity.Case switch {
                        MeshFidelity fidelity => fidelity.Rig(domain: model, key: op).Bind(parameters => op.Catch(() => {
                            using MeshingParameters live = parameters;
                            return ModelGate.Single(op, MeshSlot.Extruded, () => edit.Bounds.Case switch {
                                BoundingBox bounds => Mesh.CreateFromCurveExtrusion(curve: curve, direction: edit.Direction, parameters: live, boundingBox: bounds),
                                _ => Mesh.CreateExtrusion(profile: curve, direction: edit.Direction, parameters: live),
                            });
                        })),
                        _ => edit.Bounds.IsSome
                            ? Fin.Fail<Built<MeshSlot>>(error: op.InvalidInput())
                            : ModelGate.Single(op, MeshSlot.Extruded, () => Mesh.CreateExtrusion(profile: curve, direction: edit.Direction)),
                    });
            },
            isosurface: static (_, edit) => {
                Op op = Op.Of(name: nameof(Isosurface));
                return ModelGate.Single(op, MeshSlot.Isosurfaced, () => Mesh.CreateFromIsosurface(
                    scalarFieldEvaluator: edit.Field, box: edit.Box,
                    resolution: edit.Resolution.Value, RootFindingMaxSteps: edit.RootFindingMaxSteps));
            },
            fromLines: static (model, edit) => {
                Op op = Op.Of(name: nameof(FromLines));
                return ModelGate.BorrowMany<Curve, Built<MeshSlot>>(handles: edit.Lines, key: op, body: lines =>
                    ModelGate.Single(op, MeshSlot.Networked, () => Mesh.CreateFromLines(
                        lines: lines.ToArray(), maxFaceValence: edit.MaxFaceValence, tolerance: model.Domain.Absolute.Value)));
            },
            tessellate: static (_, edit) => {
                Op op = Op.Of(name: nameof(Tessellate));
                return ModelGate.Single(op, MeshSlot.Networked, () => Mesh.CreateFromTessellation(
                    points: edit.Points.AsIterable(),
                    edges: edit.Edges.Map(static loop => loop.AsIterable()).AsIterable(),
                    plane: edit.Frame, allowNewVertices: edit.VertexAdmission.Native));
            },
            convexHull: static (model, edit) => {
                Op op = Op.Of(name: nameof(ConvexHull));
                return op.Catch(() => {
                    Mesh hull = Mesh.CreateConvexHull3D(
                        points: edit.Points.AsIterable(), hullFacets: out int[][] facets,
                        tolerance: model.Domain.Absolute.Value, angleTolerance: model.Domain.Angle.Value);
                    return ModelGate.Own(built: hull, key: op).Map(owned => Built<MeshSlot>.Of(
                        operation: op,
                        Products: Seq(owned),
                        Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Hulled, body: new BuildBody.Tally(Count: 1))
                            + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Hulled, body: new BuildBody.SourceGroups(
                                Axis: SourceAxis.Input,
                                Groups: toSeq(facets ?? []).Map(static rows => toSeq(rows))))));
                });
            },
            patch: static (model, edit) => {
                Op op = Op.Of(name: nameof(Patch));
                return ModelGate.BorrowMany<Curve, Built<MeshSlot>>(handles: edit.InnerBoundaries, key: op, allowEmpty: true, body: inner =>
                    ModelGate.BorrowMany<Curve, Built<MeshSlot>>(handles: edit.BothSideCurves, key: op, allowEmpty: true, body: bothSides =>
                        ModelGate.BorrowMany<Surface, Built<MeshSlot>>(
                            handles: edit.PullbackSurface.ToSeq(),
                            key: op,
                            allowEmpty: true,
                            body: pullbacks => ModelGate.Single(op, MeshSlot.Patched, () => Mesh.CreatePatch(
                                outerBoundary: new Polyline(collection: edit.OuterBoundary.AsIterable()),
                                angleToleranceRadians: model.Domain.Angle.Value,
                                pullbackSurface: pullbacks.IsEmpty ? null : pullbacks[0],
                                innerBoundaryCurves: inner.AsIterable(),
                                innerBothSideCurves: bothSides.AsIterable(),
                                innerPoints: edit.InnerPoints.AsIterable(),
                                trimback: edit.Trim.Native,
                                divisions: edit.Divisions)))));
            },
            rebuild: static (_, edit) => {
                Op op = Op.Of(name: nameof(Rebuild));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Source, key: op, body: mesh =>
                    ModelGate.Single(op, MeshSlot.Rebuilt, () => Mesh.RebuildMesh(
                        mesh: mesh,
                        preserveTextureCoordinates: edit.Attributes.Contains(MeshRebuildAttribute.TextureCoordinates),
                        preserveVertexColors: edit.Attributes.Contains(MeshRebuildAttribute.VertexColors))));
            },
            cleanup: static (model, edit) => {
                Op op = Op.Of(name: nameof(Cleanup));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Sources, key: op, body: sources =>
                    op.Catch(() => {
                        bool required = Mesh.RequireIterativeCleanup(meshes: sources.AsIterable(), tolerance: model.Domain.Absolute.Value);
                        return ModelGate.OwnMany(built: Mesh.CreateFromIterativeCleanup(
                                meshes: sources.AsIterable(), tolerance: model.Domain.Absolute.Value), key: op)
                            .Map(owned => Built<MeshSlot>.Of(
                                operation: op,
                                Products: owned,
                                Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Cleaned, body: new BuildBody.Tally(Count: owned.Count))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Cleaned, body: new BuildBody.Flag(Value: required))));
                    }));
            },
            refineLoop: static (_, edit) => {
                Op op = Op.Of(name: nameof(RefineLoop));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Source, key: op, body: mesh =>
                    ModelGate.Single(op, MeshSlot.Subdivided, () => Mesh.CreateRefinedLoopMesh(
                        mesh: mesh,
                        formula: edit.Formula,
                        settings: new MeshRefinements.RefinementSettings {
                            Level = edit.Level.Value,
                            NakedEdgeMode = edit.NakedEdges,
                        })));
            },
            refineCatmullClark: static (_, edit) => {
                Op op = Op.Of(name: nameof(RefineCatmullClark));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Source, key: op, body: mesh =>
                    ModelGate.Single(op, MeshSlot.Subdivided, () => Mesh.CreateRefinedCatmullClarkMesh(
                        mesh: mesh,
                        settings: new MeshRefinements.RefinementSettings {
                            Level = edit.Level.Value,
                            NakedEdgeMode = edit.NakedEdges,
                        })));
            },
            subdivideMidEdge: static (_, edit) => {
                Op op = Op.Of(name: nameof(SubdivideMidEdge));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Source, key: op, body: mesh => op.Catch(() => {
                    Mesh working = (Mesh)mesh.Duplicate();
                    return op.Confirm(success: edit.Faces.IsEmpty
                            ? working.Subdivide()
                            : working.Subdivide(faceIndices: edit.Faces.AsIterable()))
                        .Bind(_ => ModelGate.Kept(op, MeshSlot.Subdivided, working))
                        .Rollback(working);
                }));
            },
            booleanUnion: static (model, edit) => {
                Op op = Op.Of(name: nameof(BooleanUnion));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Inputs, key: op, body: inputs =>
                    Booled(op, model, options => {
                        Mesh[] products = Mesh.CreateBooleanUnion(
                            meshes: inputs.AsIterable(), options: options,
                            commandResult: out Rhino.Commands.Result verdict, inputMap: out int[][] map);
                        return (products, verdict, map);
                    }));
            },
            booleanIntersection: static (model, edit) => {
                Op op = Op.Of(name: nameof(BooleanIntersection));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.First, key: op, body: first =>
                    ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Second, key: op, body: second =>
                        Booled(op, model, options => {
                            Mesh[] products = Mesh.CreateBooleanIntersection(
                                firstSet: first.AsIterable(), secondSet: second.AsIterable(), options: options,
                                result: out Rhino.Commands.Result verdict, inputMap: out int[][] map);
                            return (products, verdict, map);
                        })));
            },
            booleanDifference: static (model, edit) => {
                Op op = Op.Of(name: nameof(BooleanDifference));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.First, key: op, body: first =>
                    ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Second, key: op, body: second =>
                        Booled(op, model, options => {
                            Mesh[] products = Mesh.CreateBooleanDifference(
                                firstSet: first.AsIterable(), secondSet: second.AsIterable(), options: options,
                                result: out Rhino.Commands.Result verdict, inputMap: out int[][] map);
                            return (products, verdict, map);
                        })));
            },
            booleanSplit: static (model, edit) => {
                Op op = Op.Of(name: nameof(BooleanSplit));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Targets, key: op, body: targets =>
                    ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Cutters, key: op, body: cutters =>
                        Booled(op, model, options => {
                            Mesh[] products = Mesh.CreateBooleanSplit(
                                meshesToSplit: targets.AsIterable(), meshSplitters: cutters.AsIterable(), options: options,
                                result: out Rhino.Commands.Result verdict, inputMap: out int[][] map);
                            return (products, verdict, map);
                        })));
            },
            splitPlane: static (_, edit) => {
                Op op = Op.Of(name: nameof(SplitPlane));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, MeshSlot.SplitApart, () => mesh.Split(plane: edit.Plane)));
            },
            splitMeshes: static (model, edit) => {
                Op op = Op.Of(name: nameof(SplitMeshes));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Cutters, key: op, body: cutters => {
                        (bool coplanar, bool ngons) = edit.Policy.Native;
                        return ModelGate.Many(op, MeshSlot.SplitApart, () => mesh.Split(
                            meshes: cutters.AsIterable(),
                            tolerance: model.Domain.MeshIntersectionTolerance,
                            splitAtCoplanar: coplanar,
                            createNgons: ngons,
                            textLog: null,
                            cancel: model.Cancellation,
                            progress: model.ScalarReporter));
                    }));
            },
            splitDisjoint: static (_, edit) => {
                Op op = Op.Of(name: nameof(SplitDisjoint));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, MeshSlot.SplitApart, () => mesh.SplitDisjointPieces()));
            },
            splitNonManifold: static (_, edit) => {
                Op op = Op.Of(name: nameof(SplitNonManifold));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, MeshSlot.SplitApart, () => mesh.SplitNon2Manifolds()));
            },
            splitProjectedPolylines: static (model, edit) => {
                Op op = Op.Of(name: nameof(SplitProjectedPolylines));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.BorrowMany<PolylineCurve, Built<MeshSlot>>(handles: edit.Curves, key: op, body: curves =>
                        ModelGate.Many(op, MeshSlot.SplitApart, () => mesh.SplitWithProjectedPolylines(
                            curves: curves.AsIterable(), tolerance: model.Domain.Absolute.Value))));
            },
            splitUnweldedEdges: static (_, edit) => {
                Op op = Op.Of(name: nameof(SplitUnweldedEdges));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, MeshSlot.SplitApart, () => mesh.ExplodeAtUnweldedEdges()));
            },
            splitCount: static (_, edit) => {
                Op op = Op.Of(name: nameof(SplitCount));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh => {
                    (bool countSum, bool countTriangles) = edit.Mode.Native;
                    return ModelGate.Many(op, MeshSlot.SplitApart, () => Mesh.SplitMesh(
                        mesh: mesh, maxCount: edit.MaxCount, countSum: countSum, countTriangles: countTriangles));
                });
            },
            partition: static (_, edit) => {
                Op op = Op.Of(name: nameof(Partition));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, MeshSlot.SplitApart, () => Mesh.PartitionMesh(
                        mesh: mesh, maxVertexCount: edit.MaxVertexCount, maxFaceCount: edit.MaxFaceCount)));
            },
            matchEdges: static (_, edit) => {
                Op op = Op.Of(name: nameof(MatchEdges));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Targets, key: op, body: meshes =>
                    ModelGate.Many(op, MeshSlot.EdgeMatched, () => Mesh.MatchEdges(
                        inputMeshes: meshes.AsIterable(), distance: edit.Law.Distance,
                        simpleSplits: edit.Law.Capabilities.Contains(MeshMatchPolicy.SimpleSplits),
                        rachet: edit.Law.Capabilities.Contains(MeshMatchPolicy.Ratchet),
                        average: edit.Law.Capabilities.Contains(MeshMatchPolicy.Average),
                        join: edit.Law.Capabilities.Contains(MeshMatchPolicy.JoinResult))));
            },
            append: static (_, edit) => {
                Op op = Op.Of(name: nameof(Append));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Sources, key: op, body: sources =>
                    op.Catch(() => {
                        Mesh working = new();
                        working.Append(meshes: sources.AsIterable());
                        return ModelGate.Own(built: working, key: op).Map(owned => Built<MeshSlot>.Of(
                            operation: op,
                            Products: Seq(owned),
                            Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Appended, body: new BuildBody.Tally(Count: sources.Count))));
                    }));
            },
            projectFaces: static (_, edit) => {
                Op op = Op.Of(name: nameof(ProjectFaces));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh => {
                    FrozenSet<int> selected = edit.Indices.ToFrozenSet();
                    return from _ in guard(
                               selected.All(index => index < mesh.Faces.Count),
                               op.InvalidInput())
                           from built in ModelGate.Single(op, MeshSlot.Faces, () => Mesh.CreateFromFilteredFaceList(
                               original: mesh,
                               inclusion: Enumerable.Range(start: 0, count: mesh.Faces.Count).Select(selected.Contains)))
                           select built;
                });
            },
            projectNakedEdges: static (_, edit) => {
                Op op = Op.Of(name: nameof(ProjectNakedEdges));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.OwnMany(
                            built: toSeq(mesh.GetNakedEdges() ?? []).Map(static row => new PolylineCurve(polyline: row)),
                            key: op,
                            allowEmpty: true)
                        .Map(owned => Built<MeshSlot>.Of(
                            operation: op,
                            Products: owned,
                            Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Boundaries, body: new BuildBody.Tally(Count: owned.Count)))));
            },
            projectOutlines: static (_, edit) => {
                Op op = Op.Of(name: nameof(ProjectOutlines));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.OwnMany(
                            built: toSeq(mesh.GetOutlines(edit.Frame) ?? []).Map(static outline => new PolylineCurve(polyline: outline)),
                            key: op,
                            allowEmpty: true)
                        .Map(owned => Built<MeshSlot>.Of(
                            operation: op,
                            Products: owned,
                            Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Boundaries, body: new BuildBody.Tally(Count: owned.Count)))));
            },
            edit: static (model, request) => {
                Op op = Op.Of(name: nameof(Edit));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: request.Target, key: op, body: source =>
                    op.Catch(() => {
                        Mesh working = (Mesh)source.Duplicate();
                        return Edited(working: working, verb: request.Verb, runtime: model, op: op).Rollback(working);
                    }));
            },
            extrude: static (_, edit) => {
                Op op = Op.Of(name: nameof(Extrude));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    op.Catch(() => {
                        (bool uvn, bool edgeUvn) = edit.Law.Frame.Native;
                        using MeshExtruder engine = new(inputMesh: mesh, componentIndices: edit.Components.AsIterable()) {
                            Transform = edit.Law.Motion,
                            UVN = uvn,
                            EdgeBasedUVN = edgeUvn,
                            KeepOriginalFaces = edit.Law.OriginalFaces.Native,
                            TextureCoordinateMode = edit.Law.TextureCoordinates,
                            SurfaceParameterMode = edit.Law.SurfaceParameters,
                            FaceDirectionMode = edit.Law.FaceDirection,
                        };
                        Seq<Line> preview = toSeq(engine.PreviewLines ?? []);
                        return op.Confirm(success: engine.ExtrudedMesh(
                                extrudedMeshOut: out Mesh extruded, componentIndicesOut: out System.Collections.Generic.List<ComponentIndex> created))
                            .Bind(_ => ModelGate.Own(built: extruded, key: op).Map(owned => Built<MeshSlot>.Of(
                                operation: op,
                                Products: Seq(owned),
                                Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Extruded, body: new BuildBody.Tally(Count: 1))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Extruded, body: new BuildBody.ComponentRows(Indices: toSeq(created ?? [])))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Extruded, body: new BuildBody.Segments(Lines: preview))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.WallFaces, body: new BuildBody.Components(Indices: toSeq(engine.GetWallFaces()))))));
                    }));
            });

    private static Fin<Built<MeshSlot>> Edited(Mesh working, MeshEdit verb, MeshRuntime runtime, Op op) =>
        verb.Switch(
            (Working: working, Runtime: runtime, Op: op),
            reduce: static (ctx, edit) =>
                from parameters in edit.Law.Rig(runtime: ctx.Runtime, key: ctx.Op)
                // Host Reduce(parameters, threaded: true) ignores CancelToken and ProgressReporter; the rigged controls demand the main-thread path.
                from _ in ctx.Op.Confirm(success: ctx.Working.Reduce(parameters: parameters, threaded: false))
                from built in ModelGate.Kept(
                    ctx.Op,
                    MeshSlot.Edited,
                    ctx.Working,
                    extra: Optional(parameters.Error)
                        .Map(detail => BuildReceipt<MeshSlot>.Of(
                            slot: MeshSlot.Edited,
                            body: new BuildBody.Text(Value: detail)))
                        .IfNone(BuildReceipt<MeshSlot>.Empty))
                select built,
            weld: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Weld(
                    angleToleranceRadians: ctx.Runtime.Domain.Angle.Value,
                    preserveSurfaceParameters: edit.SurfaceParameters.Native);
                return ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working);
            }),
            unweld: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Unweld(angleToleranceRadians: ctx.Runtime.Domain.Angle.Value, modifyNormals: edit.Normals.Native);
                return ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working);
            }),
            unweldEdges: static (ctx, edit) => ctx.Op
                .Confirm(success: ctx.Working.UnweldEdge(edgeIndices: edit.Edges.AsIterable(), modifyNormals: edit.Normals.Native))
                .Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            unweldVertices: static (ctx, edit) => ctx.Op
                .Confirm(success: ctx.Working.UnweldVertices(topologyVertexIndices: edit.TopologyVertices.AsIterable(), modifyNormals: edit.Normals.Native))
                .Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            offset: static (ctx, edit) => ModelGate.Owned(ctx.Op, MeshSlot.Edited, ctx.Working,
                () => ctx.Working.Offset(distance: edit.Distance, solidify: edit.Shell.Native)),
            offsetDirection: static (ctx, edit) => ctx.Op.Catch(() => {
                Mesh shelled = ctx.Working.Offset(
                    distance: edit.Distance,
                    solidify: edit.Shell.Native,
                    direction: edit.Direction,
                    wallFacesOut: out System.Collections.Generic.List<int> walls);
                return ModelGate.Owned(ctx.Op, MeshSlot.Edited, ctx.Working, () => shelled,
                    extra: BuildReceipt<MeshSlot>.Of(
                        slot: MeshSlot.WallFaces,
                        body: new BuildBody.Components(Indices: toSeq(walls ?? []))));
            }),
            heal: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.HealNakedEdges(distance: edit.Distance)).Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            fillHoles: static ctx => ctx.Op.Confirm(success: ctx.Working.FillHoles())
                .Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            fillHole: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.FillHole(topologyEdgeIndex: edit.TopologyEdge))
                .Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            matchNaked: static (ctx, edit) => ctx.Op.Confirm(
                    success: ctx.Working.MatchEdges(distance: edit.Distance, rachet: edit.Mode.Native))
                .Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            mergeCoplanar: static ctx => ctx.Op
                .Confirm(success: ctx.Working.MergeAllCoplanarFaces(tolerance: ctx.Runtime.Domain.Absolute.Value, angleTolerance: ctx.Runtime.Domain.Angle.Value))
                .Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            smooth: static (ctx, edit) => ctx.Op.Confirm(success: edit.Law.Apply(target: ctx.Working))
                .Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            smoothVertices: static (ctx, edit) => ctx.Op.Confirm(
                    success: edit.Law.Apply(target: ctx.Working, vertices: Some(edit.Vertices)))
                .Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            collapseEdgeLength: static (ctx, edit) => ctx.Op.Catch(() => ctx.Working.CollapseFacesByEdgeLength(
                    bGreaterThan: edit.Side.Native, edgeLength: edit.EdgeLength))
                .Bind(collapsed => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working, extra: BuildReceipt<MeshSlot>.Of(
                    slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: collapsed)))),
            collapseArea: static (ctx, edit) => ctx.Op.Catch(() => ctx.Working.CollapseFacesByArea(
                    lessThanArea: edit.LessThanArea, greaterThanArea: edit.GreaterThanArea))
                .Bind(collapsed => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working, extra: BuildReceipt<MeshSlot>.Of(
                    slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: collapsed)))),
            collapseAspectRatio: static (ctx, edit) => ctx.Op.Catch(() => ctx.Working.CollapseFacesByByAspectRatio(
                    aspectRatio: edit.Value))
                .Bind(collapsed => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working, extra: BuildReceipt<MeshSlot>.Of(
                    slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: collapsed)))),
            rebuildNormals: static ctx => ctx.Op.Catch(() => {
                ctx.Working.RebuildNormals();
                return ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working);
            }),
            unifyNormals: static ctx => ctx.Op.Catch(() => {
                int modified = ctx.Working.UnifyNormals();
                return ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working, extra: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: modified)));
            }),
            orient: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Flip(
                    vertexNormals: edit.Law.Targets.Contains(MeshOrientationTarget.VertexNormals),
                    faceNormals: edit.Law.Targets.Contains(MeshOrientationTarget.FaceNormals),
                    faceOrientation: edit.Law.Targets.Contains(MeshOrientationTarget.FaceOrientation),
                    ngonsBoundaryDirection: edit.Law.Targets.Contains(MeshOrientationTarget.NgonBoundaries));
                return ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working);
            }),
            compact: static ctx => ctx.Op.Confirm(success: ctx.Working.Compact()).Bind(_ => ModelGate.Kept(ctx.Op, MeshSlot.Edited, ctx.Working)),
            extractNonManifold: static (ctx, edit) => ctx.Op.Catch(() =>
                ModelGate.Staged(op: ctx.Op,
                    (MeshSlot.Edited, (GeometryBase[])[ctx.Working.ExtractNonManifoldEdges(selective: edit.Scope.Native), ctx.Working], false))),
            edgeSoften: static (ctx, edit) => ModelGate.Owned(ctx.Op, MeshSlot.Edited, ctx.Working,
                () => ctx.Working.WithEdgeSoftening(
                    softeningRadius: edit.Law.Radius,
                    chamfer: edit.Law.Features.Contains(MeshEdgeSoftenFeature.Chamfer),
                    faceted: edit.Law.Faceting.Native,
                    force: edit.Law.Features.Contains(MeshEdgeSoftenFeature.Force),
                    angleThreshold: edit.Law.AngleThreshold)),
            shutLine: static (ctx, edit) =>
                ModelGate.BorrowMany<Curve, Built<MeshSlot>>(
                    handles: edit.Profiles.Map(static row => row.Curve),
                    key: ctx.Op,
                    body: curves => ModelGate.Owned(ctx.Op, MeshSlot.Edited, ctx.Working,
                        () => ctx.Working.WithShutLining(
                            faceted: edit.Faceting.Native,
                            tolerance: ctx.Runtime.Domain.Absolute.Value,
                            curves: curves.Zip(edit.Profiles).Map(static pair => pair.Item2.Rig(curve: pair.Item1)).AsEnumerable()))),
            displace: static (ctx, edit) => ModelGate.Owned(ctx.Op, MeshSlot.Edited, ctx.Working,
                () => ctx.Working.WithDisplacement(displacement: edit.Law.Rig())));

    private static Fin<Built<MeshSlot>> Booled(
        Op op, MeshRuntime model,
        Func<MeshBooleanOptions, (Mesh[] Products, Rhino.Commands.Result Verdict, int[][] Map)> run) =>
        op.Catch(() => {
            (Mesh[] products, Rhino.Commands.Result verdict, int[][] map) = run(new MeshBooleanOptions {
                Tolerance = model.Domain.MeshIntersectionTolerance,
                CancellationToken = model.Cancellation,
                ProgressReporter = model.ScalarReporter,
            });
            return ModelGate.Staged(op: op, success: verdict == Rhino.Commands.Result.Success,
                extra: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Booled, body: new BuildBody.Code(Value: (int)verdict))
                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Booled, body: new BuildBody.SourceGroups(
                        Axis: SourceAxis.Input,
                        Groups: toSeq(map ?? []).Map(static rows => toSeq(rows)))),
                (MeshSlot.Booled, products, true));
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Meshes {
    public static Eff<MeshRuntime, Built<MeshSlot>> Build(params ReadOnlySpan<MeshOp> operations) {
        Op op = Op.Of();
        Seq<MeshOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<MeshRuntime>().Bind(runtime =>
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

`Meshes.Build` admits a non-empty operation span before `MeshRuntime.Apply` threads one runtime through `ModelGate.Folded`.

`MeshOp.ProjectFaces`, `MeshOp.ProjectNakedEdges`, and `MeshOp.ProjectOutlines` keep projection discriminants on the operation owner. Polyline values become owned `PolylineCurve` products before egress. `Mesh.CreateContourCurves` and `Mesh.ComputeThickness` remain kernel analysis.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
