# [RASM_RHINO_MODELING_MESHING]

`HostMeshes.Build` owns admitted mesh creation, transformation, projection, and egress. `MeshOp.QuadRemesh` remains the sole mesh-to-`SubDOp.FromMesh` seam.

## [01]-[INDEX]

- [02]-[FIDELITY]: `MeshPreset`, `MeshLaw`, and `MeshFidelity`.
- [03]-[POLICY]: `QuadLaw`, `WrapLaw`, `ReduceLaw`, `ExtrudeLaw`, `SmoothLaw`, and mesh generation policies.
- [04]-[ALGEBRA]: policy values shared by mutation and construction.
- [05]-[MUTATION]: `MeshEditIntent` and the value-semantic edit policies it carries.
- [06]-[OPERATION_RAIL]: `MeshOp` and `HostMeshes.Build` over the spine's `ModelRuntime`.

## [02]-[FIDELITY]

- Owner: `MeshFidelity` is the sole fidelity discriminant; `MeshLaw` admits the complete custom parameter set; `MeshPreset` carries the live host factories; `MeshDensity` closes the normalized density pair; `MeshFidelityFeature` is the feature vocabulary `MeshLaw` reads.
- Law: `Rig` mints one disposable `MeshingParameters` carrier inside the consuming arm and nowhere else, so the native's lifetime is a `using` inside one borrow window and no policy value holds a live host carrier.
- Law: the six `MeshingParameters` bits are a `CapabilitySet` column, never six bools and never a `FrozenSet` — a frozen set held by a `[ComplexValueObject]` compares by REFERENCE, so two byte-identical fidelity laws read unequal, and the capability column carries `Admits`, rank-ordered `Wire`, and unrepresentable off-roster membership instead.
- Growth: a new preset is one `MeshPreset` row; a new host bit is one `MeshFidelityFeature` row read once in `Mint`.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `MeshingParameters` and its factories, `MeshingParameterTextureRange`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/rails` (`ValidityClaim`, `Op`, `Fin`), kernel `Domain/context` (`Context`, `Tolerance`, `ToleranceLane`), `Modeling/solids.md` (`ModelGate`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
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
public abstract partial record MeshFidelity : IValidityEvidence {
    private MeshFidelity() { }
    public sealed record Preset(MeshPreset Row) : MeshFidelity;
    public sealed record Density(MeshDensity Value) : MeshFidelity;
    public sealed record Custom(MeshLaw Law) : MeshFidelity;

    public bool IsValid => Switch(
        preset: static fidelity => (ValidityClaim)(fidelity.Row is not null),
        density: static fidelity => (ValidityClaim)fidelity.Value.IsValid,
        custom: static fidelity => (ValidityClaim)fidelity.Law.IsValid);

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
public readonly partial struct MeshDensity : IValidityEvidence {
    public double Value { get; }
    public Option<double> MinimumEdgeLength { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double value,
        ref Option<double> minimumEdgeLength) {
        if (!Admits(value: value, minimumEdgeLength: minimumEdgeLength)) {
            validationError = new ValidationError("Mesh density requires a finite normalized density and finite non-negative edge length.");
        }
    }

    public bool IsValid => Admits(value: Value, minimumEdgeLength: MinimumEdgeLength);

    private static ValidityClaim Admits(double value, Option<double> minimumEdgeLength) => ValidityClaim.All(
        ValidityClaim.UnitInterval(value: value),
        ValidityClaim.WhenPresent(facet: minimumEdgeLength, claim: static length => ValidityClaim.Nonnegative(value: length)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeshFidelityFeature : ICapability<MeshFidelityFeature> {
    public static readonly MeshFidelityFeature JaggedSeams = new(key: "jagged-seams");
    public static readonly MeshFidelityFeature RefineGrid = new(key: "refine-grid");
    public static readonly MeshFidelityFeature DoublePrecision = new(key: "double-precision");
    public static readonly MeshFidelityFeature SimplePlanes = new(key: "simple-planes");
    public static readonly MeshFidelityFeature ComputeCurvature = new(key: "compute-curvature");
    public static readonly MeshFidelityFeature ClosedObjectPostProcess = new(key: "closed-post-process");
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshLaw : IValidityEvidence {
    public MeshingParameterTextureRange TextureRange { get; }
    public CapabilitySet<MeshFidelityFeature> Features { get; }
    public int GridMinCount { get; }
    public int GridMaxCount { get; }
    public double GridAspectRatio { get; }
    public double GridAmplification { get; }
    public double MinimumEdgeLength { get; }
    public double MaximumEdgeLength { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MeshingParameterTextureRange textureRange,
        ref CapabilitySet<MeshFidelityFeature> features,
        ref int gridMinCount,
        ref int gridMaxCount,
        ref double gridAspectRatio,
        ref double gridAmplification,
        ref double minimumEdgeLength,
        ref double maximumEdgeLength) {
        if (!Admits(gridMinCount, gridMaxCount, gridAspectRatio, gridAmplification, minimumEdgeLength, maximumEdgeLength)) {
            validationError = new ValidationError("Mesh fidelity bounds are inconsistent.");
        }
    }

    public bool IsValid => Admits(
        GridMinCount, GridMaxCount, GridAspectRatio, GridAmplification, MinimumEdgeLength, MaximumEdgeLength);

    private static ValidityClaim Admits(
        int gridMinCount, int gridMaxCount, double gridAspectRatio,
        double gridAmplification, double minimumEdgeLength, double maximumEdgeLength) => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: gridMinCount, floor: 0),
        ValidityClaim.CountAtLeast(count: gridMaxCount, floor: gridMinCount),
        ValidityClaim.Nonnegative(value: gridAspectRatio),
        ValidityClaim.Positive(value: gridAmplification),
        ValidityClaim.Nonnegative(value: minimumEdgeLength),
        ValidityClaim.Nonnegative(value: maximumEdgeLength),
        maximumEdgeLength == 0.0 || ValidityClaim.Ordered(lower: minimumEdgeLength, upper: maximumEdgeLength));

    internal MeshingParameters Mint(Context domain) => new() {
        TextureRange = TextureRange,
        JaggedSeams = Features.Admits(capability: MeshFidelityFeature.JaggedSeams),
        RefineGrid = Features.Admits(capability: MeshFidelityFeature.RefineGrid),
        DoublePrecision = Features.Admits(capability: MeshFidelityFeature.DoublePrecision),
        SimplePlanes = Features.Admits(capability: MeshFidelityFeature.SimplePlanes),
        ComputeCurvature = Features.Admits(capability: MeshFidelityFeature.ComputeCurvature),
        ClosedObjectPostProcess = Features.Admits(capability: MeshFidelityFeature.ClosedObjectPostProcess),
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

- Owner: `MeshCount` admits every positive host count once; `QuadLaw`, `WrapLaw`, `ReduceLaw`, and `ExtrudeLaw` carry the four native configuration surfaces; `QuadFeature`, `WrapFeature`, and `ReduceFeature` are their capability vocabularies; `MeshExtrusionFrame` rows the extruder's correlated frame pair.
- Law: a policy refuses at construction and answers the same fold at `IsValid`, so the generated factory and the outer operation seam share ONE authority and the rail never re-derives a bound. `MeshCount`'s hook was named `Validate`, which the value-object generator never calls — every zero and negative count reached the natives unrefused — and the admitted spelling is `ValidateFactoryArguments`; `ExtrudeLaw`'s hook read an `originalFaces` parameter no signature declared and did not compile.
- Law: cancellation, progress, and the regime belong to `ModelRuntime` and never to an operation or a policy — `ReduceLaw.Rig` and the boolean options read the runtime handed to the arm, so no policy value stores a token and no arm mints one.
- Law: `Rig` is a capability projection on the fault rail, NOT a `[Mapper]` transcription — the `CapabilitySet` collapse consumed the field-for-field mirroring a Mapperly seat owns, so every host slot a `Rig` writes reads `Admits(capability: …)` off a grant column, threads the runtime, and returns `Fin<T>` inside its `key.Catch` window. Mapperly maps a declared source property onto a same-shaped target property on a pure signature and expresses none of those three, so a mapper seated here carries a hand-written body per slot and maps nothing; the folder's `[Mapper]` seats stay on the pages transcribing a foreign record field-for-field.
- Law: a row vocabulary is earned, never reflexive — a `[SmartEnum]` stands where its rows carry a column beyond the bit (a writer, a native factory, or a correlated host tuple such as `MeshExtrusionFrame`, `MeshSplitPolicy`, and `MeshCountMode`), a set of INDEPENDENT host bits is a `CapabilitySet` because they reach the native as adjacent arguments a call site transposes in silence, and a two-state modality that is the whole fact travels as a named `bool` on its owning case.
- Law: `SmoothLaw` is the shared owner — `Curve.Smooth` and `Mesh.Smooth` take the identical five knobs, so `Modeling/curves.md` composes this page's law instead of respelling it and the mesh-only pass count and vertex selection ride the mesh cases.
- Growth: a new native surface is one policy value with its `Rig`; a new host bit is one row on the owning capability vocabulary.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `QuadRemeshParameters`, `ShrinkWrapParameters`, `ReduceMeshParameters`, `MeshExtruder`, `QuadRemeshSymmetryAxis`, `MeshExtruderParameterMode`, `MeshExtruderFaceDirectionMode`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/rails` (`ValidityClaim`, `IValidityEvidence`, `Op`, `Fin`), `Modeling/solids.md` (`ModelRuntime`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct MeshCount {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = ValidityClaim.CountAtLeast(count: value, floor: 1)
            ? null
            : new ValidationError("Mesh counts must be positive.");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QuadFeature : ICapability<QuadFeature> {
    public static readonly QuadFeature AdaptiveCount = new(key: "adaptive-count");
    public static readonly QuadFeature DetectHardEdges = new(key: "detect-hard-edges");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WrapFeature : ICapability<WrapFeature> {
    public static readonly WrapFeature FillInputHoles = new(key: "fill-input-holes");
    public static readonly WrapFeature InflatePoints = new(key: "inflate-points");
    public static readonly WrapFeature PreserveColors = new(key: "preserve-colors");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReduceFeature : ICapability<ReduceFeature> {
    public static readonly ReduceFeature AllowDistortion = new(key: "allow-distortion");
    public static readonly ReduceFeature NormalizeSize = new(key: "normalize-size");
}

[SmartEnum<int>]
public sealed partial class MeshExtrusionFrame {
    public static readonly MeshExtrusionFrame Transform = new(key: 0, native: (false, false));
    public static readonly MeshExtrusionFrame UVN = new(key: 1, native: (true, false));
    public static readonly MeshExtrusionFrame EdgeUVN = new(key: 2, native: (false, true));

    internal (bool UVN, bool EdgeUVN) Native { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct QuadLaw : IValidityEvidence {
    public int TargetQuadCount { get; }
    public double TargetEdgeLength { get; }
    public double AdaptiveSize { get; }
    public CapabilitySet<QuadFeature> Features { get; }
    public int GuideCurveInfluence { get; }
    public int PreserveMeshArrayEdgesMode { get; }
    public QuadRemeshSymmetryAxis Symmetry { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int targetQuadCount,
        ref double targetEdgeLength,
        ref double adaptiveSize,
        ref CapabilitySet<QuadFeature> features,
        ref int guideCurveInfluence,
        ref int preserveMeshArrayEdgesMode,
        ref QuadRemeshSymmetryAxis symmetry) {
        if (!Admits(targetQuadCount, targetEdgeLength, adaptiveSize)) {
            validationError = new ValidationError("Quad-remesh targets are outside the admitted range.");
        }
    }

    public bool IsValid => Admits(TargetQuadCount, TargetEdgeLength, AdaptiveSize);

    private static ValidityClaim Admits(int targetQuadCount, double targetEdgeLength, double adaptiveSize) =>
        ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: targetQuadCount, floor: 1),
            ValidityClaim.Nonnegative(value: targetEdgeLength),
            ValidityClaim.Nonnegative(value: adaptiveSize), adaptiveSize <= 100.0);

    internal Fin<QuadRemeshParameters> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new QuadRemeshParameters {
            TargetQuadCount = TargetQuadCount,
            TargetEdgeLength = TargetEdgeLength,
            AdaptiveSize = AdaptiveSize,
            AdaptiveQuadCount = Features.Admits(capability: QuadFeature.AdaptiveCount),
            DetectHardEdges = Features.Admits(capability: QuadFeature.DetectHardEdges),
            GuideCurveInfluence = GuideCurveInfluence,
            PreserveMeshArrayEdgesMode = PreserveMeshArrayEdgesMode,
            SymmetryAxis = Symmetry,
        }));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct WrapLaw : IValidityEvidence {
    public double TargetEdgeLength { get; }
    public double Offset { get; }
    public int SmoothingIterations { get; }
    public CapabilitySet<WrapFeature> Features { get; }
    public int PolygonOptimization { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double targetEdgeLength,
        ref double offset,
        ref int smoothingIterations,
        ref CapabilitySet<WrapFeature> features,
        ref int polygonOptimization) {
        if (!Admits(targetEdgeLength, offset, smoothingIterations, polygonOptimization)) {
            validationError = new ValidationError("Shrink-wrap policy requires positive scale and non-negative passes.");
        }
    }

    public bool IsValid => Admits(TargetEdgeLength, Offset, SmoothingIterations, PolygonOptimization);

    private static ValidityClaim Admits(
        double targetEdgeLength, double offset, int smoothingIterations, int polygonOptimization) =>
        ValidityClaim.All(
            ValidityClaim.Positive(value: targetEdgeLength), ValidityClaim.Finite(value: offset),
            ValidityClaim.CountAtLeast(count: smoothingIterations, floor: 0),
            ValidityClaim.CountAtLeast(count: polygonOptimization, floor: 0));

    internal Fin<ShrinkWrapParameters> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new ShrinkWrapParameters {
            TargetEdgeLength = TargetEdgeLength,
            Offset = Offset,
            SmoothingIterations = SmoothingIterations,
            FillHolesInInputObjects = Features.Admits(capability: WrapFeature.FillInputHoles),
            PolygonOptimization = PolygonOptimization,
            InflateVerticesAndPoints = Features.Admits(capability: WrapFeature.InflatePoints),
            PreserveColors = Features.Admits(capability: WrapFeature.PreserveColors),
        }));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ReduceLaw : IValidityEvidence {
    public int DesiredPolygonCount { get; }
    public CapabilitySet<ReduceFeature> Features { get; }
    public int Accuracy { get; }
    public Seq<int> FaceTags { get; }
    public Seq<ComponentIndex> LockedComponents { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int desiredPolygonCount,
        ref CapabilitySet<ReduceFeature> features,
        ref int accuracy,
        ref Seq<int> faceTags,
        ref Seq<ComponentIndex> lockedComponents) {
        if (!Admits(desiredPolygonCount, accuracy, lockedComponents)) {
            validationError = new ValidationError("Mesh reduction requires a positive target, bounded accuracy, and indexed locks.");
        }
    }

    public bool IsValid => Admits(DesiredPolygonCount, Accuracy, LockedComponents);

    private static ValidityClaim Admits(int desiredPolygonCount, int accuracy, Seq<ComponentIndex> lockedComponents) =>
        ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: desiredPolygonCount, floor: 1),
            ValidityClaim.CountAtLeast(count: accuracy, floor: 1), accuracy <= 10,
            ModelClaim.Rows(
                rows: lockedComponents,
                claim: static component => ValidityClaim.CountAtLeast(count: component.Index, floor: 0),
                allowEmpty: true));

    internal Fin<ReduceMeshParameters> Rig(ModelRuntime runtime, Op key) =>
        key.Catch(() => Fin.Succ(value: new ReduceMeshParameters {
            DesiredPolygonCount = DesiredPolygonCount,
            AllowDistortion = Features.Admits(capability: ReduceFeature.AllowDistortion),
            Accuracy = Accuracy,
            NormalizeMeshSize = Features.Admits(capability: ReduceFeature.NormalizeSize),
            FaceTags = FaceTags.ToArray(),
            LockedComponents = LockedComponents.ToArray(),
            CancelToken = runtime.Cancellation,
            ProgressReporter = runtime.ScalarReporter,
        }));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ExtrudeLaw : IValidityEvidence {
    public Transform Motion { get; }
    public MeshExtrusionFrame Frame { get; }
    public bool KeepOriginalFaces { get; }
    public MeshExtruderParameterMode TextureCoordinates { get; }
    public MeshExtruderParameterMode SurfaceParameters { get; }
    public MeshExtruderFaceDirectionMode FaceDirection { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Transform motion,
        ref MeshExtrusionFrame frame,
        ref bool keepOriginalFaces,
        ref MeshExtruderParameterMode textureCoordinates,
        ref MeshExtruderParameterMode surfaceParameters,
        ref MeshExtruderFaceDirectionMode faceDirection) {
        if (!Admits(motion, frame, textureCoordinates, surfaceParameters, faceDirection)) {
            validationError = new ValidationError("Mesh extrusion requires a valid non-zero motion transform and complete frame and face policies.");
        }
    }

    public bool IsValid => Admits(Motion, Frame, TextureCoordinates, SurfaceParameters, FaceDirection);

    private static ValidityClaim Admits(
        Transform motion, MeshExtrusionFrame? frame, MeshExtruderParameterMode textureCoordinates,
        MeshExtruderParameterMode surfaceParameters, MeshExtruderFaceDirectionMode faceDirection) =>
        ValidityClaim.All(
            motion.IsValid, !motion.IsZero, frame is not null,
            Enum.IsDefined(textureCoordinates), Enum.IsDefined(surfaceParameters), Enum.IsDefined(faceDirection));
}
```

## [04]-[ALGEBRA]

Frozen capability sets carry fidelity, remesh, wrap, reduction, shut-line, smoothing, orientation, edge-matching, and rebuild behavior; native bit products never cross admission.

- Law: struct policies share one owner-local predicate between generated factories and outer operation admission; factory creation rejects invalid values, and the outer seam rejects default ghosts without duplicating rules.
- Growth: a new policy surface is one value object beside the ones here; the mutation and operation sections read it with zero new surface.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`), kernel `Domain/rails` (`ValidityClaim`, `IValidityEvidence`, `Op`, `Fin`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SmoothAxis : ICapability<SmoothAxis> {
    public static readonly SmoothAxis X = new(key: "x");
    public static readonly SmoothAxis Y = new(key: "y");
    public static readonly SmoothAxis Z = new(key: "z");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeshOrientationTarget : ICapability<MeshOrientationTarget> {
    public static readonly MeshOrientationTarget VertexNormals = new(key: "vertex-normals");
    public static readonly MeshOrientationTarget FaceNormals = new(key: "face-normals");
    public static readonly MeshOrientationTarget FaceOrientation = new(key: "face-orientation");
    public static readonly MeshOrientationTarget NgonBoundaries = new(key: "ngon-boundaries");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeshMatchPolicy : ICapability<MeshMatchPolicy> {
    public static readonly MeshMatchPolicy SimpleSplits = new(key: "simple-splits");
    public static readonly MeshMatchPolicy Ratchet = new(key: "ratchet");
    public static readonly MeshMatchPolicy Average = new(key: "average");
    public static readonly MeshMatchPolicy JoinResult = new(key: "join-result");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeshRebuildAttribute : ICapability<MeshRebuildAttribute> {
    public static readonly MeshRebuildAttribute TextureCoordinates = new(key: "texture-coordinates");
    public static readonly MeshRebuildAttribute VertexColors = new(key: "vertex-colors");
}

[SmartEnum<int>]
public sealed partial class MeshSplitPolicy {
    public static readonly MeshSplitPolicy Separate = new(key: 0, native: (false, false));
    public static readonly MeshSplitPolicy Coplanar = new(key: 1, native: (true, false));
    public static readonly MeshSplitPolicy Ngons = new(key: 2, native: (false, true));
    public static readonly MeshSplitPolicy CoplanarNgons = new(key: 3, native: (true, true));

    internal (bool Coplanar, bool Ngons) Native { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeedGrant : ICapability<SeedGrant> {
    public static readonly SeedGrant Circumscribe = new(key: "circumscribe");
    public static readonly SeedGrant QuadCaps = new(key: "quad-caps");
    public static readonly SeedGrant Solid = new(key: "solid");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeshEdgeSoftenFeature : ICapability<MeshEdgeSoftenFeature> {
    public static readonly MeshEdgeSoftenFeature Chamfer = new(key: "chamfer");
    public static readonly MeshEdgeSoftenFeature Force = new(key: "force");
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshEdgeSoftenLaw : IValidityEvidence {
    public double Radius { get; }
    public CapabilitySet<MeshEdgeSoftenFeature> Features { get; }
    public bool Faceted { get; }
    public double AngleThreshold { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double radius,
        ref CapabilitySet<MeshEdgeSoftenFeature> features,
        ref bool faceted,
        ref double angleThreshold) {
        if (!Admits(radius: radius, angleThreshold: angleThreshold)) {
            validationError = new ValidationError("Mesh edge softening requires a finite positive radius and finite non-negative angle threshold.");
        }
    }

    public bool IsValid => Admits(radius: Radius, angleThreshold: AngleThreshold);

    private static ValidityClaim Admits(double radius, double angleThreshold) => ValidityClaim.All(
        ValidityClaim.Positive(value: radius), ValidityClaim.Nonnegative(value: angleThreshold));
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
public readonly partial struct SmoothLaw : IValidityEvidence {
    public double Factor { get; }
    public CapabilitySet<SmoothAxis> Axes { get; }
    public bool FixBoundaries { get; }
    public SmoothingCoordinateSystem System { get; }
    public Plane Frame { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double factor,
        ref CapabilitySet<SmoothAxis> axes,
        ref bool fixBoundaries,
        ref SmoothingCoordinateSystem system,
        ref Plane frame) {
        if (!Admits(factor, axes, system, frame)) {
            validationError = new ValidationError(
                "Smoothing requires a finite factor, at least one axis, a declared coordinate system, and a valid frame.");
        }
    }

    public bool IsValid => Admits(Factor, Axes, System, Frame);

    internal bool Apply(Mesh target, int steps, Option<Seq<int>> vertices = default) =>
        vertices.Case switch {
            Seq<int> selected => target.Smooth(
                vertexIndices: selected.AsIterable(), smoothFactor: Factor, numSteps: steps,
                bXSmooth: Axes.Admits(capability: SmoothAxis.X), bYSmooth: Axes.Admits(capability: SmoothAxis.Y),
                bZSmooth: Axes.Admits(capability: SmoothAxis.Z), bFixBoundaries: FixBoundaries,
                coordinateSystem: System, plane: Frame),
            _ => target.Smooth(
                smoothFactor: Factor, numSteps: steps,
                bXSmooth: Axes.Admits(capability: SmoothAxis.X), bYSmooth: Axes.Admits(capability: SmoothAxis.Y),
                bZSmooth: Axes.Admits(capability: SmoothAxis.Z), bFixBoundaries: FixBoundaries,
                coordinateSystem: System, plane: Frame),
        };

    internal Curve? Apply(Curve target) => target.Smooth(
        smoothFactor: Factor,
        bXSmooth: Axes.Admits(capability: SmoothAxis.X), bYSmooth: Axes.Admits(capability: SmoothAxis.Y),
        bZSmooth: Axes.Admits(capability: SmoothAxis.Z), bFixBoundaries: FixBoundaries,
        coordinateSystem: System, plane: Frame);

    private static ValidityClaim Admits(
        double factor,
        CapabilitySet<SmoothAxis> axes,
        SmoothingCoordinateSystem system,
        Plane frame) => ValidityClaim.All(
        ValidityClaim.Finite(value: factor),
        ValidityClaim.CountAtLeast(count: axes.Held.Count, floor: 1),
        Enum.IsDefined(system),
        frame.IsValid);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshOrientationLaw : IValidityEvidence {
    public CapabilitySet<MeshOrientationTarget> Targets { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilitySet<MeshOrientationTarget> targets) {
        if (!Admits(targets: targets)) {
            validationError = new ValidationError("Mesh orientation requires at least one target.");
        }
    }

    public bool IsValid => Admits(targets: Targets);

    private static ValidityClaim Admits(CapabilitySet<MeshOrientationTarget> targets) =>
        ValidityClaim.CountAtLeast(count: targets.Held.Count, floor: 1);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MeshMatchLaw : IValidityEvidence {
    public double Distance { get; }
    public CapabilitySet<MeshMatchPolicy> Capabilities { get; }

    private static readonly CapabilityLaw<MeshMatchPolicy> MatchGrants = CapabilityLaw<MeshMatchPolicy>.Open;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double distance,
        ref CapabilitySet<MeshMatchPolicy> capabilities) {
        if (!Admits(distance, capabilities)) {
            validationError = new ValidationError("Mesh edge matching requires a finite positive distance and an admitted capability set.");
        }
    }

    public bool IsValid => Admits(Distance, Capabilities);

    private static ValidityClaim Admits(double distance, CapabilitySet<MeshMatchPolicy> capabilities) =>
        ValidityClaim.All(ValidityClaim.Positive(value: distance), MatchGrants.Admit(held: capabilities).IsSucc);
}

```

## [05]-[MUTATION]

- Owner: `MeshEditIntent` is the sole value-semantic mutation algebra and owns its own `Apply` dispatch; `SmoothLaw`, `MeshOrientationLaw`, `MeshEdgeSoftenLaw`, `ShutLineProfile`, `DisplacementLaw`, and `ClosedPolyline` are the policy values its cases carry; `ShutLineFeature` is their remaining capability vocabulary.
- Law: mutation is value-semantic — the rail duplicates the borrowed mesh, runs the in-place host member on the working copy, and owns the copy (or the member's returned mesh, disposing the copy); no verb mutates the geometry behind an input handle, and the failure path rolls the duplicate back.
- Law: the edit algebra owns its own dispatch, so `MeshOp.Edit` hands the working copy to `MeshEditIntent.Apply` and holds no per-verb knowledge — a new verb lands as one case with its arm and the construction rail is untouched.
- Law: the entry family renames at the boundary — the kernel owns `MeshEdit` (`Rasm/Meshing/edit.md`, the single-writer SoA build arena), so this host mutation roster is `MeshEditIntent` under the branch rule that a boundary declaration whose simple name matches a kernel owner renames on the host side.
- Growth: a new edit verb is one case with its arm; a new policy surface is one value object beside the ones here.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — the `Mesh` weld, offset, heal, collapse, normal, shut-lining, and displacement members; `MeshDisplacementInfo`, `ShutLiningCurveInfo`, `Polyline`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`), kernel `Domain/rails` (`ValidityClaim`, `IValidityEvidence`, `Op`, `Fin`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`, `ModelRuntime`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshEditIntent : IValidityEvidence {
    private MeshEditIntent() { }
    public sealed record Reduce(ReduceLaw Law) : MeshEditIntent;
    public sealed record Weld(bool PreserveSurfaceParameters) : MeshEditIntent;
    public sealed record Unweld(bool ModifyNormals) : MeshEditIntent;
    public sealed record UnweldEdges(Seq<int> Edges, bool ModifyNormals) : MeshEditIntent;
    public sealed record UnweldVertices(Seq<int> TopologyVertices, bool ModifyNormals) : MeshEditIntent;
    public sealed record Offset(double Distance, bool Solid) : MeshEditIntent;
    public sealed record OffsetDirection(double Distance, bool Solid, Vector3d Direction) : MeshEditIntent;
    public sealed record Heal(double Distance) : MeshEditIntent;
    public sealed record FillHoles : MeshEditIntent;
    public sealed record FillHole(int TopologyEdge) : MeshEditIntent;
    public sealed record MatchNaked(double Distance, bool Ratchet) : MeshEditIntent;
    public sealed record MergeCoplanar : MeshEditIntent;
    public sealed record Smooth(SmoothLaw Law, MeshCount Steps) : MeshEditIntent;
    public sealed record SmoothVertices(Seq<int> Vertices, SmoothLaw Law, MeshCount Steps) : MeshEditIntent;
    public sealed record CollapseEdgeLength(bool AboveThreshold, double EdgeLength) : MeshEditIntent;
    public sealed record CollapseArea(double LessThanArea, double GreaterThanArea) : MeshEditIntent;
    public sealed record CollapseAspectRatio(double Value) : MeshEditIntent;
    public sealed record RebuildNormals : MeshEditIntent;
    public sealed record UnifyNormals : MeshEditIntent;
    public sealed record Orient(MeshOrientationLaw Law) : MeshEditIntent;
    public sealed record Compact : MeshEditIntent;
    public sealed record ExtractNonManifold(bool Selective) : MeshEditIntent;
    public sealed record EdgeSoften(MeshEdgeSoftenLaw Law) : MeshEditIntent;
    public sealed record ShutLine(Seq<ShutLineProfile> Profiles, bool Faceted) : MeshEditIntent;
    public sealed record Displace(DisplacementLaw Law) : MeshEditIntent;

    public bool IsValid => Switch(
        reduce: static edit => (ValidityClaim)edit.Law.IsValid,
        weld: static _ => (ValidityClaim)true,
        unweld: static _ => (ValidityClaim)true,
        unweldEdges: static edit => ModelClaim.Rows(
            rows: edit.Edges, claim: static edge => ValidityClaim.CountAtLeast(count: edge, floor: 0)),
        unweldVertices: static edit => ModelClaim.Rows(
            rows: edit.TopologyVertices, claim: static vertex => ValidityClaim.CountAtLeast(count: vertex, floor: 0)),
        offset: static edit => ValidityClaim.Positive(value: edit.Distance),
        offsetDirection: static edit => ValidityClaim.All(
            ValidityClaim.Positive(value: edit.Distance), ValidityClaim.Direction(value: edit.Direction)),
        heal: static edit => ValidityClaim.Positive(value: edit.Distance),
        fillHoles: static () => (ValidityClaim)true,
        fillHole: static edit => ValidityClaim.CountAtLeast(count: edit.TopologyEdge, floor: 0),
        matchNaked: static edit => ValidityClaim.Positive(value: edit.Distance),
        mergeCoplanar: static () => (ValidityClaim)true,
        smooth: static edit => (ValidityClaim)edit.Law.IsValid,
        smoothVertices: static edit => ValidityClaim.All(
            ModelClaim.Rows(rows: edit.Vertices, claim: static vertex => ValidityClaim.CountAtLeast(count: vertex, floor: 0)),
            edit.Law.IsValid),
        collapseEdgeLength: static edit => ValidityClaim.Positive(value: edit.EdgeLength),
        collapseArea: static edit => ValidityClaim.Ordered(lower: edit.LessThanArea, upper: edit.GreaterThanArea),
        collapseAspectRatio: static edit => ValidityClaim.Positive(value: edit.Value),
        rebuildNormals: static () => (ValidityClaim)true,
        unifyNormals: static () => (ValidityClaim)true,
        orient: static edit => (ValidityClaim)edit.Law.IsValid,
        compact: static () => (ValidityClaim)true,
        extractNonManifold: static _ => (ValidityClaim)true,
        edgeSoften: static edit => (ValidityClaim)edit.Law.IsValid,
        shutLine: static edit => ModelClaim.Rows(rows: edit.Profiles, claim: static profile => profile.IsValid),
        displace: static edit => (ValidityClaim)edit.Law.IsValid);

    internal Fin<Seq<GeometryHandle>> Apply(Mesh working, ModelRuntime runtime, Op op) =>
        Switch(
            (Working: working, Runtime: runtime, Op: op),
            reduce: static (ctx, edit) =>
                from parameters in edit.Law.Rig(runtime: ctx.Runtime, key: ctx.Op)
                from _ in ctx.Op.Catch(
                    () => ctx.Op.Confirm(success: ctx.Working.Reduce(parameters: parameters, threaded: false)),
                    token: ctx.Runtime.Cancellation)
                from built in ModelGate.Kept(ctx.Op, ctx.Working)
                select built,
            weld: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Weld(
                    angleToleranceRadians: ctx.Runtime.Domain.Angle.Value,
                    preserveSurfaceParameters: edit.PreserveSurfaceParameters);
                return ModelGate.Kept(ctx.Op, ctx.Working);
            }),
            unweld: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Unweld(angleToleranceRadians: ctx.Runtime.Domain.Angle.Value, modifyNormals: edit.ModifyNormals);
                return ModelGate.Kept(ctx.Op, ctx.Working);
            }),
            unweldEdges: static (ctx, edit) => ctx.Op
                .Confirm(success: ctx.Working.UnweldEdge(edgeIndices: edit.Edges.AsIterable(), modifyNormals: edit.ModifyNormals))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            unweldVertices: static (ctx, edit) => ctx.Op
                .Confirm(success: ctx.Working.UnweldVertices(topologyVertexIndices: edit.TopologyVertices.AsIterable(), modifyNormals: edit.ModifyNormals))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            offset: static (ctx, edit) => ModelGate.Owned(ctx.Op, ctx.Working,
                () => ctx.Working.Offset(distance: edit.Distance, solidify: edit.Solid)),
            offsetDirection: static (ctx, edit) => ctx.Op.Catch(() => {
                Mesh shelled = ctx.Working.Offset(
                    distance: edit.Distance,
                    solidify: edit.Solid,
                    direction: edit.Direction,
                    wallFacesOut: out _);
                return ModelGate.Owned(ctx.Op, ctx.Working, () => shelled);
            }),
            heal: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.HealNakedEdges(distance: edit.Distance)).Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            fillHoles: static ctx => ctx.Op.Confirm(success: ctx.Working.FillHoles())
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            fillHole: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.FillHole(topologyEdgeIndex: edit.TopologyEdge))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            matchNaked: static (ctx, edit) => ctx.Op.Confirm(
                    success: ctx.Working.MatchEdges(distance: edit.Distance, rachet: edit.Ratchet))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            mergeCoplanar: static ctx => ctx.Op
                .Confirm(success: ctx.Working.MergeAllCoplanarFaces(tolerance: ctx.Runtime.Domain.Absolute.Value, angleTolerance: ctx.Runtime.Domain.Angle.Value))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            smooth: static (ctx, edit) => ctx.Op.Confirm(success: edit.Law.Apply(target: ctx.Working, steps: edit.Steps.Value))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            smoothVertices: static (ctx, edit) => ctx.Op.Confirm(
                    success: edit.Law.Apply(target: ctx.Working, steps: edit.Steps.Value, vertices: Some(edit.Vertices)))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            collapseEdgeLength: static (ctx, edit) => ctx.Op.Catch(() => ctx.Working.CollapseFacesByEdgeLength(
                    bGreaterThan: edit.AboveThreshold, edgeLength: edit.EdgeLength))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            collapseArea: static (ctx, edit) => ctx.Op.Catch(() => ctx.Working.CollapseFacesByArea(
                    lessThanArea: edit.LessThanArea, greaterThanArea: edit.GreaterThanArea))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            collapseAspectRatio: static (ctx, edit) => ctx.Op.Catch(() => ctx.Working.CollapseFacesByByAspectRatio(
                    aspectRatio: edit.Value))
                .Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            rebuildNormals: static ctx => ctx.Op.Catch(() => {
                ctx.Working.RebuildNormals();
                return ModelGate.Kept(ctx.Op, ctx.Working);
            }),
            unifyNormals: static ctx => ctx.Op.Catch(() => {
                ctx.Working.UnifyNormals();
                return ModelGate.Kept(ctx.Op, ctx.Working);
            }),
            orient: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Flip(
                    vertexNormals: edit.Law.Targets.Admits(capability: MeshOrientationTarget.VertexNormals),
                    faceNormals: edit.Law.Targets.Admits(capability: MeshOrientationTarget.FaceNormals),
                    faceOrientation: edit.Law.Targets.Admits(capability: MeshOrientationTarget.FaceOrientation),
                    ngonsBoundaryDirection: edit.Law.Targets.Admits(capability: MeshOrientationTarget.NgonBoundaries));
                return ModelGate.Kept(ctx.Op, ctx.Working);
            }),
            compact: static ctx => ctx.Op.Confirm(success: ctx.Working.Compact()).Bind(_ => ModelGate.Kept(ctx.Op, ctx.Working)),
            extractNonManifold: static (ctx, edit) => ctx.Op.Catch(() =>
                ModelGate.Staged(op: ctx.Op,
                    ((GeometryBase[])[ctx.Working.ExtractNonManifoldEdges(selective: edit.Selective), ctx.Working], false))),
            edgeSoften: static (ctx, edit) => ModelGate.Owned(ctx.Op, ctx.Working,
                () => ctx.Working.WithEdgeSoftening(
                    softeningRadius: edit.Law.Radius,
                    chamfer: edit.Law.Features.Admits(capability: MeshEdgeSoftenFeature.Chamfer),
                    faceted: edit.Law.Faceted,
                    force: edit.Law.Features.Admits(capability: MeshEdgeSoftenFeature.Force),
                    angleThreshold: edit.Law.AngleThreshold)),
            shutLine: static (ctx, edit) =>
                ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(
                    handles: edit.Profiles.Map(static row => row.Curve),
                    key: ctx.Op,
                    body: curves => ModelGate.Owned(ctx.Op, ctx.Working,
                        () => ctx.Working.WithShutLining(
                            faceted: edit.Faceted,
                            tolerance: ctx.Runtime.Domain.Absolute.Value,
                            curves: curves.Zip(edit.Profiles).Map(static pair => pair.Item2.Rig(curve: pair.Item1)).AsEnumerable()))),
            displace: static (ctx, edit) => ModelGate.Owned(ctx.Op, ctx.Working,
                () => ctx.Working.WithDisplacement(displacement: edit.Law.Rig())));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShutLineFeature : ICapability<ShutLineFeature> {
    public static readonly ShutLineFeature Pull = new(key: "pull");
    public static readonly ShutLineFeature Bump = new(key: "bump");
    public static readonly ShutLineFeature Enabled = new(key: "enabled");
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ShutLineProfile : IValidityEvidence {
    public GeometryHandle Curve { get; }
    public double Radius { get; }
    public int Profile { get; }
    public CapabilitySet<ShutLineFeature> Features { get; }
    public Seq<Interval> Intervals { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref GeometryHandle curve,
        ref double radius,
        ref int profile,
        ref CapabilitySet<ShutLineFeature> features,
        ref Seq<Interval> intervals) {
        if (!Admits(curve, radius, profile, features, intervals)) {
            validationError = new ValidationError(
                "Shut-line profiles require a curve, finite positive radius, profile row, features, and valid intervals.");
        }
    }

    public bool IsValid => Admits(Curve, Radius, Profile, Features, Intervals);

    private static ValidityClaim Admits(
        GeometryHandle? curve,
        double radius,
        int profile,
        CapabilitySet<ShutLineFeature> features,
        Seq<Interval> intervals) => ValidityClaim.All(
        ModelClaim.Handle(handle: curve),
        ValidityClaim.Positive(value: radius),
        ValidityClaim.CountAtLeast(count: profile, floor: 0),
        ModelClaim.Rows(rows: intervals, claim: static interval => (ValidityClaim)interval.IsValid, allowEmpty: true));

    internal ShutLiningCurveInfo Rig(Curve curve) =>
        new(curve: curve, radius: Radius, profile: Profile,
            pull: Features.Admits(capability: ShutLineFeature.Pull),
            isBump: Features.Admits(capability: ShutLineFeature.Bump),
            curveIntervals: Intervals.ToArray(), enabled: Features.Admits(capability: ShutLineFeature.Enabled));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DisplacementLaw : IValidityEvidence {
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
        if (!Admits(texture, mapping, black, white, blackMove, whiteMove, mappingTransform, instanceTransform,
                postWeldAngle, refineSensitivity, sweepPitch, channelNumber, faceLimit, fairingAmount,
                refineStepCount, memoryLimit)) {
            validationError = new ValidationError("Displacement bounds are inconsistent.");
        }
    }

    public bool IsValid => Admits(
        Texture, Mapping, Black, White, BlackMove, WhiteMove, MappingTransform, InstanceTransform,
        PostWeldAngle, RefineSensitivity, SweepPitch, ChannelNumber, FaceLimit, FairingAmount,
        RefineStepCount, MemoryLimit);

    private static ValidityClaim Admits(
        RenderTexture? texture, TextureMapping? mapping, double black, double white, double blackMove,
        double whiteMove, Option<Transform> mappingTransform, Option<Transform> instanceTransform,
        double postWeldAngle, double refineSensitivity, double sweepPitch, int channelNumber,
        int faceLimit, int fairingAmount, int refineStepCount, int memoryLimit) => ValidityClaim.All(
        texture is not null, mapping is not null,
        ValidityClaim.Ordered(lower: black, upper: white), black != white,
        ValidityClaim.Finite(value: blackMove), ValidityClaim.Finite(value: whiteMove),
        ValidityClaim.WhenPresent(facet: mappingTransform, claim: static row => (ValidityClaim)row.IsValid),
        ValidityClaim.WhenPresent(facet: instanceTransform, claim: static row => (ValidityClaim)row.IsValid),
        ValidityClaim.Finite(value: postWeldAngle), ValidityClaim.UnitInterval(value: refineSensitivity),
        ValidityClaim.Positive(value: sweepPitch),
        ValidityClaim.CountAtLeast(count: channelNumber, floor: 1),
        ValidityClaim.CountAtLeast(count: faceLimit, floor: 1),
        ValidityClaim.CountAtLeast(count: fairingAmount, floor: 0),
        ValidityClaim.CountAtLeast(count: refineStepCount, floor: 0),
        ValidityClaim.CountAtLeast(count: memoryLimit, floor: 1));

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
public sealed partial class ClosedPolyline : IValidityEvidence {
    public Seq<Point3d> Points { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<Point3d> points) {
        if (!Admits(points: points)) {
            validationError = new ValidationError("Closed polylines require four valid points and matching endpoints.");
        }
    }

    public bool IsValid => Admits(points: Points);

    private static ValidityClaim Admits(Seq<Point3d> points) => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: points.Count, floor: 4),
        ModelClaim.Points(points: points),
        new Polyline(collection: points.AsIterable()).IsClosed);

    internal Polyline Native => new(collection: Points.AsIterable());
}

```

## [06]-[OPERATION_RAIL]

- Owner: `MeshOp` `[Union]` — the whole verified mesh-construction verb roster; `HostMeshes` — the one entry folding any operation spread into owned geometry handles over the spine's `ModelRuntime`.
- Law: the entry family renames at the boundary — the kernel owns `Meshes` (`Rasm/Analysis/inspect.md`), so this host roster is `HostMeshes` beside `HostCurves` and `HostSurfaces` under the same branch rule.
- Law: `HostMeshes.Build` materializes the operation span ahead of the runtime bind — a span cannot cross the `Eff.runtime<ModelRuntime>()` lambda — then runs the spine's `ModelGate.Entry`, so capture, the non-empty guard, accumulating admission, and the fold are the spine's while the apply lambda threads one `ModelRuntime` through every arm.
- Law: admission NAMES its axis — `MeshOp.Admitted` dispatches through the generated total `Switch` into `ModelClaim.Admits`, so a request breaching several constraints reports all of them and a new case breaks the compile instead of falling through a catch-all.
- Law: the async remesh is the one host family honouring both governance columns on a WHOLE mesh, so `QuadRemesh` executes through `Mesh.QuadRemeshAsync`, its face-block overload when the case carries `FaceBlocks`, and `Mesh.QuadRemeshBrepAsync` for a Brep source; the synchronous whole-mesh overloads accept neither progress nor cancellation and the synchronous face-block overload accepts both only by inventing a face grouping the caller never asked for, so `ModelRuntime.Await` is the ONE seam collapsing a host `Task<T>` back onto this page's synchronous rail under the runtime's own token.
- Law: the mesh boolean family runs through `MeshBooleanOptions`, confirms the native `Result`, owns the returned meshes, discards the unconsumed input map at the native call, and allocates no `TextLog`.
- Law: the mesh crossing gate reads `ToleranceLane.MeshIntersection`, never a bare absolute tolerance and never a page-local coefficient — the host's own `MeshIntersectionsTolerancesCoefficient` composes once at the kernel lane.
- Boundary: `MeshOp.QuadRemesh` remains the sole mesh-to-`SubDOp.FromMesh` seam; `Mesh.CreateContourCurves` and `Mesh.ComputeThickness` remain kernel analysis; `ProjectFaces`, `ProjectNakedEdges`, and `ProjectOutlines` keep every projection discriminant on this operation owner rather than on a projection sibling; polyline values become owned `PolylineCurve` products before egress.
- Growth: a new mesher or engine is one case with its arm; the spine and every consumer read it with zero new surface.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — the `Mesh` construction, seed, remesh, wrap, boolean, split, partition, match, and projection rosters; `MeshBooleanOptions`, `MeshRefinements`, `MeshExtruder`, `TextLog`), kernel `Domain/rails` (`Op`, `KernelFault.InvalidInput(Key, Axis)`, `Fin`), kernel `Domain/validation` (`CapabilitySet`, `CapabilityLaw`), kernel `Domain/context` (`Context`, `ToleranceLane`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`, `ModelRuntime`, `CapEnd`), LanguageExt.Core (`Eff.runtime`, `Seq`), Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshOp {
    private MeshOp() { }
    public sealed record FromGeometry(GeometryHandle Source, MeshFidelity Fidelity) : MeshOp;
    public sealed record FromSubD(GeometryHandle Source, SubDDisplayParameters.Density Level) : MeshOp;
    public sealed record Cage(GeometryHandle Source, bool TextureCoordinates) : MeshOp;
    public sealed record FromBoundary(GeometryHandle Boundary, MeshFidelity Fidelity) : MeshOp;
    public sealed record SeedPlane(Plane Frame, Interval X, Interval Y, MeshCount XCount, MeshCount YCount) : MeshOp;
    public sealed record SeedBox(Box Box, MeshCount XCount, MeshCount YCount, MeshCount ZCount) : MeshOp;
    public sealed record SeedSphere(Sphere Sphere, MeshCount XCount, MeshCount YCount) : MeshOp;
    public sealed record SeedIcoSphere(Sphere Sphere, MeshCount Subdivisions) : MeshOp;
    public sealed record SeedQuadSphere(Sphere Sphere, MeshCount Subdivisions) : MeshOp;
    public sealed record SeedCylinder(Cylinder Cylinder, MeshCount Vertical, MeshCount Around, CapabilitySet<CapEnd> Caps, CapabilitySet<SeedGrant> Grants) : MeshOp;
    public sealed record SeedCone(Cone Cone, MeshCount Vertical, MeshCount Around, CapabilitySet<SeedGrant> Grants) : MeshOp;
    public sealed record SeedTorus(Torus Torus, MeshCount Vertical, MeshCount Around) : MeshOp;
    public sealed record SeedClosedPolyline(ClosedPolyline Boundary) : MeshOp;
    public sealed record QuadRemesh(GeometryHandle Source, QuadLaw Law, Seq<GeometryHandle> Guides, Seq<int> FaceBlocks = default) : MeshOp;
    public sealed record Wrap(Seq<GeometryHandle> Sources, WrapLaw Law, Option<MeshFidelity> Fidelity = default) : MeshOp;
    public sealed record CurvePipe(GeometryHandle Curve, double Radius, MeshCount Segments, int Accuracy, MeshPipeCapStyle Cap, bool Faceted, Seq<Interval> Intervals = default) : MeshOp;
    public sealed record CurveExtrude(GeometryHandle Curve, Vector3d Direction, Option<MeshFidelity> Fidelity = default, Option<BoundingBox> Bounds = default) : MeshOp;
    public sealed record Isosurface(Func<Point3d, double> Field, BoundingBox Box, MeshCount Resolution, int RootFindingMaxSteps) : MeshOp;
    public sealed record FromLines(Seq<GeometryHandle> Lines, int MaxFaceValence) : MeshOp;
    public sealed record Tessellate(Seq<Point3d> Points, Seq<Seq<Point3d>> Edges, Plane Frame, bool AllowNewVertices) : MeshOp;
    public sealed record ConvexHull(Seq<Point3d> Points) : MeshOp;
    public sealed record Patch(Seq<Point3d> OuterBoundary, Option<GeometryHandle> PullbackSurface, Seq<GeometryHandle> InnerBoundaries, Seq<GeometryHandle> BothSideCurves, Seq<Point3d> InnerPoints, bool Trimback, int Divisions) : MeshOp;
    public sealed record Rebuild(GeometryHandle Source, CapabilitySet<MeshRebuildAttribute> Attributes) : MeshOp;
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
    public sealed record Edit(GeometryHandle Target, MeshEditIntent Verb) : MeshOp;
    public sealed record Extrude(GeometryHandle Target, Seq<ComponentIndex> Components, ExtrudeLaw Law) : MeshOp;

    private static readonly CapabilityLaw<SeedGrant> CylinderGrants =
        CapabilityLaw<SeedGrant>.Forbidden(barred: Seq(CapabilitySet<SeedGrant>.Of(SeedGrant.Solid)));

    private static readonly CapabilityLaw<SeedGrant> ConeGrants =
        CapabilityLaw<SeedGrant>.Forbidden(barred: Seq(CapabilitySet<SeedGrant>.Of(SeedGrant.Circumscribe)));

    internal Fin<MeshOp> Admitted(Op key) =>
        Switch(
            context: key,
            fromGeometry: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.Fidelity), row.Fidelity is { IsValid: true })),
            fromSubD: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.Level), Enum.IsDefined(row.Level))),
            cage: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source))),
            fromBoundary: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Boundary), ModelClaim.Handle(handle: row.Boundary)),
                (nameof(row.Fidelity), row.Fidelity is { IsValid: true })),
            seedPlane: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Frame), row.Frame.IsValid), (nameof(row.X), row.X.IsValid), (nameof(row.Y), row.Y.IsValid)),
            seedBox: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Box), row.Box.IsValid)),
            seedSphere: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Sphere), row.Sphere.IsValid)),
            seedIcoSphere: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Sphere), row.Sphere.IsValid)),
            seedQuadSphere: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Sphere), row.Sphere.IsValid)),
            seedCylinder: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Cylinder), row.Cylinder.IsValid),
                (nameof(row.Grants), CylinderGrants.Admit(held: row.Grants).IsSucc)),
            seedCone: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Cone), row.Cone.IsValid),
                (nameof(row.Grants), ConeGrants.Admit(held: row.Grants).IsSucc)),
            seedTorus: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Torus), row.Torus.IsValid)),
            seedClosedPolyline: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Boundary), row.Boundary is { IsValid: true })),
            quadRemesh: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.Law), row.Law.IsValid),
                (nameof(row.Guides), ModelClaim.Handles(handles: row.Guides, allowEmpty: true)),
                (nameof(row.FaceBlocks), ModelClaim.Rows(
                    rows: row.FaceBlocks, claim: static face => ValidityClaim.CountAtLeast(count: face, floor: 0), allowEmpty: true))),
            wrap: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Sources), ModelClaim.Handles(handles: row.Sources)),
                (nameof(row.Law), row.Law.IsValid),
                (nameof(row.Fidelity), ValidityClaim.WhenPresent(
                    facet: row.Fidelity, claim: static fidelity => (ValidityClaim)fidelity.IsValid))),
            curvePipe: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.Radius), ValidityClaim.Positive(value: row.Radius)),
                (nameof(row.Accuracy), ValidityClaim.CountAtLeast(count: row.Accuracy, floor: 1)),
                (nameof(row.Cap), Enum.IsDefined(row.Cap)),
                (nameof(row.Intervals), ModelClaim.Rows(
                    rows: row.Intervals, claim: static interval => (ValidityClaim)interval.IsValid, allowEmpty: true))),
            curveExtrude: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.Direction), ValidityClaim.Direction(value: row.Direction)),
                (nameof(row.Fidelity), ValidityClaim.WhenPresent(
                    facet: row.Fidelity, claim: static fidelity => (ValidityClaim)fidelity.IsValid)),
                (nameof(row.Bounds), ValidityClaim.WhenPresent(
                    facet: row.Bounds, claim: static bounds => (ValidityClaim)bounds.IsValid))),
            isosurface: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Field), row.Field is not null), (nameof(row.Box), row.Box.IsValid),
                (nameof(row.RootFindingMaxSteps), ValidityClaim.CountAtLeast(count: row.RootFindingMaxSteps, floor: 1))),
            fromLines: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Lines), ModelClaim.Handles(handles: row.Lines)),
                (nameof(row.MaxFaceValence), ValidityClaim.CountAtLeast(count: row.MaxFaceValence, floor: 3))),
            tessellate: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Points), ModelClaim.Points(points: row.Points)),
                (nameof(row.Edges), ModelClaim.Rows(rows: row.Edges, claim: static loop => ModelClaim.Points(points: loop))),
                (nameof(row.Frame), row.Frame.IsValid)),
            convexHull: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Points), ValidityClaim.All(
                    ValidityClaim.CountAtLeast(count: row.Points.Count, floor: 4), ModelClaim.Points(points: row.Points)))),
            patch: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.OuterBoundary), ValidityClaim.All(
                    ValidityClaim.CountAtLeast(count: row.OuterBoundary.Count, floor: 4),
                    ModelClaim.Points(points: row.OuterBoundary))),
                (nameof(row.PullbackSurface), ValidityClaim.WhenPresent(
                    facet: row.PullbackSurface, claim: static handle => ModelClaim.Handle(handle: handle))),
                (nameof(row.InnerBoundaries), ModelClaim.Handles(handles: row.InnerBoundaries, allowEmpty: true)),
                (nameof(row.BothSideCurves), ModelClaim.Handles(handles: row.BothSideCurves, allowEmpty: true)),
                (nameof(row.InnerPoints), ModelClaim.Points(points: row.InnerPoints, allowEmpty: true)),
                (nameof(row.Divisions), ValidityClaim.CountAtLeast(count: row.Divisions, floor: 1))),
            rebuild: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source))),
            cleanup: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Sources), ModelClaim.Handles(handles: row.Sources))),
            refineLoop: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.Formula), Enum.IsDefined(row.Formula)), (nameof(row.NakedEdges), Enum.IsDefined(row.NakedEdges))),
            refineCatmullClark: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.NakedEdges), Enum.IsDefined(row.NakedEdges))),
            subdivideMidEdge: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.Faces), ModelClaim.Rows(
                    rows: row.Faces, claim: static face => ValidityClaim.CountAtLeast(count: face, floor: 0), allowEmpty: true))),
            booleanUnion: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Inputs), ModelClaim.Handles(handles: row.Inputs))),
            booleanIntersection: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handles(handles: row.First)),
                (nameof(row.Second), ModelClaim.Handles(handles: row.Second))),
            booleanDifference: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handles(handles: row.First)),
                (nameof(row.Second), ModelClaim.Handles(handles: row.Second))),
            booleanSplit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Targets), ModelClaim.Handles(handles: row.Targets)),
                (nameof(row.Cutters), ModelClaim.Handles(handles: row.Cutters))),
            splitPlane: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)), (nameof(row.Plane), row.Plane.IsValid)),
            splitMeshes: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Cutters), ModelClaim.Handles(handles: row.Cutters)),
                (nameof(row.Policy), row.Policy is not null)),
            splitDisjoint: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target))),
            splitNonManifold: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target))),
            splitProjectedPolylines: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Curves), ModelClaim.Handles(handles: row.Curves))),
            splitUnweldedEdges: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target))),
            splitCount: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.MaxCount), ValidityClaim.CountAtLeast(count: row.MaxCount, floor: 1)),
                (nameof(row.Mode), row.Mode is not null)),
            partition: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.MaxVertexCount), ValidityClaim.CountAtLeast(count: row.MaxVertexCount, floor: 1)),
                (nameof(row.MaxFaceCount), ValidityClaim.CountAtLeast(count: row.MaxFaceCount, floor: 1))),
            matchEdges: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Targets), ModelClaim.Handles(handles: row.Targets)), (nameof(row.Law), row.Law.IsValid)),
            append: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Sources), ModelClaim.Handles(handles: row.Sources))),
            projectFaces: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Indices), ModelClaim.Rows(
                    rows: row.Indices, claim: static index => ValidityClaim.CountAtLeast(count: index, floor: 0)))),
            projectNakedEdges: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target))),
            projectOutlines: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)), (nameof(row.Frame), row.Frame.IsValid)),
            edit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Verb), row.Verb is { IsValid: true })),
            extrude: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Components), ModelClaim.Rows(
                    rows: row.Components, claim: static component => ValidityClaim.CountAtLeast(count: component.Index, floor: 0))),
                (nameof(row.Law), row.Law.IsValid)));

    internal Fin<Seq<GeometryHandle>> Apply(ModelRuntime runtime) =>
        Switch(
            runtime,
            fromGeometry: static (model, edit) => {
                Op op = FromGeometry.SelfOp;
                return ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Source, key: op, body: source =>
                    from parameters in edit.Fidelity.Rig(domain: model, key: op)
                    from built in op.Catch(() => {
                        using MeshingParameters live = parameters;
                        return source switch {
                            Brep brep => ModelGate.Many(op, () => Mesh.CreateFromBrep(brep: brep, meshingParameters: live)),
                            Surface surface => ModelGate.Single(op, () => Mesh.CreateFromSurface(surface: surface, meshingParameters: live)),
                            Extrusion extrusion => ModelGate.Single(op, () => Mesh.CreateFromExtrusion(extrusion: extrusion, meshingParameters: live)),
                            _ => Fin.Fail<Seq<GeometryHandle>>(error: op.Unsupported(inputType: source.GetType(), outputType: typeof(Mesh))),
                        };
                    })
                    select built);
            },
            fromSubD: static (_, edit) => {
                Op op = FromSubD.SelfOp;
                return ModelGate.Borrow<SubD, Seq<GeometryHandle>>(handle: edit.Source, key: op, body: subd =>
                    ModelGate.Single(op, () => Mesh.CreateFromSubD(subd: subd, displayDensity: edit.Level)));
            },
            cage: static (_, edit) => {
                Op op = Cage.SelfOp;
                return ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Source, key: op, body: source =>
                    source switch {
                        SubD subd => ModelGate.Single(op, () => edit.TextureCoordinates
                            ? Mesh.CreateFromSubDControlNetWithTextureCoordinates(subd: subd)
                            : Mesh.CreateFromSubDControlNet(subd: subd)),
                        Surface surface => ModelGate.Single(op, () => Mesh.CreateFromSurfaceControlNet(surface: surface)),
                        _ => Fin.Fail<Seq<GeometryHandle>>(error: op.Unsupported(inputType: source.GetType(), outputType: typeof(Mesh))),
                    });
            },
            fromBoundary: static (model, edit) => {
                Op op = FromBoundary.SelfOp;
                return ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Boundary, key: op, body: boundary =>
                    from parameters in edit.Fidelity.Rig(domain: model, key: op)
                    from built in op.Catch(() => {
                        using MeshingParameters live = parameters;
                        return ModelGate.Single(op, () => Mesh.CreateFromPlanarBoundary(
                            boundary: boundary, parameters: live, tolerance: model.Domain.Absolute.Value));
                    })
                    select built);
            },
            seedPlane: static (_, edit) => {
                Op op = SeedPlane.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromPlane(
                    plane: edit.Frame,
                    xInterval: edit.X,
                    yInterval: edit.Y,
                    xCount: edit.XCount.Value,
                    yCount: edit.YCount.Value));
            },
            seedBox: static (_, edit) => {
                Op op = SeedBox.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromBox(
                    box: edit.Box,
                    xCount: edit.XCount.Value,
                    yCount: edit.YCount.Value,
                    zCount: edit.ZCount.Value));
            },
            seedSphere: static (_, edit) => {
                Op op = SeedSphere.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromSphere(
                    sphere: edit.Sphere,
                    xCount: edit.XCount.Value,
                    yCount: edit.YCount.Value));
            },
            seedIcoSphere: static (_, edit) => {
                Op op = SeedIcoSphere.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateIcoSphere(
                    sphere: edit.Sphere,
                    subdivisions: edit.Subdivisions.Value));
            },
            seedQuadSphere: static (_, edit) => {
                Op op = SeedQuadSphere.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateQuadSphere(
                    sphere: edit.Sphere,
                    subdivisions: edit.Subdivisions.Value));
            },
            seedCylinder: static (_, edit) => {
                Op op = SeedCylinder.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromCylinder(
                    cylinder: edit.Cylinder,
                    vertical: edit.Vertical.Value,
                    around: edit.Around.Value,
                    capBottom: edit.Caps.Admits(capability: CapEnd.Lower),
                    capTop: edit.Caps.Admits(capability: CapEnd.Upper),
                    circumscribe: edit.Grants.Admits(capability: SeedGrant.Circumscribe),
                    quadCaps: edit.Grants.Admits(capability: SeedGrant.QuadCaps)));
            },
            seedCone: static (_, edit) => {
                Op op = SeedCone.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromCone(
                    cone: edit.Cone,
                    vertical: edit.Vertical.Value,
                    around: edit.Around.Value,
                    solid: edit.Grants.Admits(capability: SeedGrant.Solid),
                    quadCaps: edit.Grants.Admits(capability: SeedGrant.QuadCaps)));
            },
            seedTorus: static (_, edit) => {
                Op op = SeedTorus.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromTorus(
                    torus: edit.Torus,
                    vertical: edit.Vertical.Value,
                    around: edit.Around.Value));
            },
            seedClosedPolyline: static (_, edit) => {
                Op op = SeedClosedPolyline.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromClosedPolyline(
                    polyline: edit.Boundary.Native));
            },
            quadRemesh: static (model, edit) => {
                Op op = QuadRemesh.SelfOp;
                return ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Source, key: op, body: source =>
                    ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Guides, key: op, allowEmpty: true, body: guides =>
                        from parameters in edit.Law.Rig(key: op)
                        from remeshed in (source switch {
                            Brep brep => model.Await(() => Mesh.QuadRemeshBrepAsync(
                                brep: brep, parameters: parameters, guideCurves: guides.AsIterable(),
                                progress: model.IntegerReporter, cancelToken: model.Cancellation), op),
                            Mesh mesh when !edit.FaceBlocks.IsEmpty => model.Await(() => mesh.QuadRemeshAsync(
                                faceBlocks: edit.FaceBlocks.AsIterable(), parameters: parameters, guideCurves: guides.AsIterable(),
                                progress: model.IntegerReporter, cancelToken: model.Cancellation), op),
                            Mesh mesh => model.Await(() => mesh.QuadRemeshAsync(
                                parameters: parameters, guideCurves: guides.AsIterable(),
                                progress: model.IntegerReporter, cancelToken: model.Cancellation), op),
                            _ => Fin.Fail<Mesh>(error: op.Unsupported(inputType: source.GetType(), outputType: typeof(Mesh))),
                        })
                        from built in ModelGate.Single(op, () => remeshed)
                        select built));
            },
            wrap: static (model, edit) => {
                Op op = Wrap.SelfOp;
                return ModelGate.BorrowMany<GeometryBase, Seq<GeometryHandle>>(handles: edit.Sources, key: op, body: sources =>
                    from parameters in edit.Law.Rig(key: op)
                    from built in op.Catch(() => (
                        AllMeshes: sources.ForAll(static value => value is Mesh),
                        Cloud: sources.Count == 1 ? sources[0] as PointCloud : null,
                        Fidelity: edit.Fidelity.Case) switch {
                        (true, _, null) => ModelGate.Single(op, () => Mesh.ShrinkWrap(
                            meshes: sources.Map(static value => (Mesh)value).AsIterable(),
                            parameters: parameters,
                            token: model.Cancellation), token: model.Cancellation),
                        (true, _, _) => Fin.Fail<Seq<GeometryHandle>>(error: op.InvalidInput()),
                        (false, PointCloud cloud, null) => ModelGate.Single(op, () => Mesh.ShrinkWrap(
                            pointCloud: cloud,
                            parameters: parameters,
                            token: model.Cancellation), token: model.Cancellation),
                        (false, PointCloud, _) => Fin.Fail<Seq<GeometryHandle>>(error: op.InvalidInput()),
                        (false, _, MeshFidelity fidelity) => fidelity.Rig(domain: model, key: op).Bind(meshing => op.Catch(() => {
                            using MeshingParameters live = meshing;
                            return ModelGate.Single(op, () => Mesh.ShrinkWrap(
                                geometryBases: sources.AsIterable(),
                                parameters: parameters,
                                meshingParameters: live,
                                token: model.Cancellation), token: model.Cancellation);
                        })),
                        _ => Fin.Fail<Seq<GeometryHandle>>(error: op.MissingContext()),
                    })
                    select built);
            },
            curvePipe: static (_, edit) => {
                Op op = CurvePipe.SelfOp;
                return ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Curve, key: op, body: curve =>
                    ModelGate.Single(op, () => Mesh.CreateFromCurvePipe(
                        curve: curve, radius: edit.Radius, segments: edit.Segments.Value, accuracy: edit.Accuracy,
                        capType: edit.Cap, faceted: edit.Faceted,
                        intervals: edit.Intervals.IsEmpty ? null : edit.Intervals.AsIterable())));
            },
            curveExtrude: static (model, edit) => {
                Op op = CurveExtrude.SelfOp;
                return ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Curve, key: op, body: curve =>
                    edit.Fidelity.Case switch {
                        MeshFidelity fidelity => fidelity.Rig(domain: model, key: op).Bind(parameters => op.Catch(() => {
                            using MeshingParameters live = parameters;
                            return ModelGate.Single(op, () => edit.Bounds.Case switch {
                                BoundingBox bounds => Mesh.CreateFromCurveExtrusion(curve: curve, direction: edit.Direction, parameters: live, boundingBox: bounds),
                                _ => Mesh.CreateExtrusion(profile: curve, direction: edit.Direction, parameters: live),
                            });
                        })),
                        _ => edit.Bounds.IsSome
                            ? Fin.Fail<Seq<GeometryHandle>>(error: op.InvalidInput())
                            : ModelGate.Single(op, () => Mesh.CreateExtrusion(profile: curve, direction: edit.Direction)),
                    });
            },
            isosurface: static (_, edit) => {
                Op op = Isosurface.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromIsosurface(
                    scalarFieldEvaluator: edit.Field, box: edit.Box,
                    resolution: edit.Resolution.Value, RootFindingMaxSteps: edit.RootFindingMaxSteps));
            },
            fromLines: static (model, edit) => {
                Op op = FromLines.SelfOp;
                return ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Lines, key: op, body: lines =>
                    ModelGate.Single(op, () => Mesh.CreateFromLines(
                        lines: lines.ToArray(), maxFaceValence: edit.MaxFaceValence, tolerance: model.Domain.Absolute.Value)));
            },
            tessellate: static (_, edit) => {
                Op op = Tessellate.SelfOp;
                return ModelGate.Single(op, () => Mesh.CreateFromTessellation(
                    points: edit.Points.AsIterable(),
                    edges: edit.Edges.Map(static loop => loop.AsIterable()).AsIterable(),
                    plane: edit.Frame, allowNewVertices: edit.AllowNewVertices));
            },
            convexHull: static (model, edit) => {
                Op op = ConvexHull.SelfOp;
                return op.Catch(() => {
                    Mesh hull = Mesh.CreateConvexHull3D(
                        points: edit.Points.AsIterable(), hullFacets: out _,
                        tolerance: model.Domain.Absolute.Value, angleTolerance: model.Domain.Angle.Value);
                    return ModelGate.Own(built: hull, key: op).Map(handle => Seq(handle));
                });
            },
            patch: static (model, edit) => {
                Op op = Patch.SelfOp;
                return ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.InnerBoundaries, key: op, allowEmpty: true, body: inner =>
                    ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.BothSideCurves, key: op, allowEmpty: true, body: bothSides =>
                        ModelGate.BorrowMany<Surface, Seq<GeometryHandle>>(
                            handles: edit.PullbackSurface.ToSeq(),
                            key: op,
                            allowEmpty: true,
                            body: pullbacks => ModelGate.Single(op, () => Mesh.CreatePatch(
                                outerBoundary: new Polyline(collection: edit.OuterBoundary.AsIterable()),
                                angleToleranceRadians: model.Domain.Angle.Value,
                                pullbackSurface: pullbacks.IsEmpty ? null : pullbacks[0],
                                innerBoundaryCurves: inner.AsIterable(),
                                innerBothSideCurves: bothSides.AsIterable(),
                                innerPoints: edit.InnerPoints.AsIterable(),
                                trimback: edit.Trimback,
                                divisions: edit.Divisions)))));
            },
            rebuild: static (_, edit) => {
                Op op = Rebuild.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Source, key: op, body: mesh =>
                    ModelGate.Single(op, () => Mesh.RebuildMesh(
                        mesh: mesh,
                        preserveTextureCoordinates: edit.Attributes.Admits(capability: MeshRebuildAttribute.TextureCoordinates),
                        preserveVertexColors: edit.Attributes.Admits(capability: MeshRebuildAttribute.VertexColors))));
            },
            cleanup: static (model, edit) => {
                Op op = Cleanup.SelfOp;
                return ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Sources, key: op, body: sources =>
                    ModelGate.Many(op, () => Mesh.CreateFromIterativeCleanup(
                        meshes: sources.AsIterable(), tolerance: model.Domain.Absolute.Value)));
            },
            refineLoop: static (_, edit) => {
                Op op = RefineLoop.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Source, key: op, body: mesh =>
                    ModelGate.Single(op, () => Mesh.CreateRefinedLoopMesh(
                        mesh: mesh,
                        formula: edit.Formula,
                        settings: new MeshRefinements.RefinementSettings {
                            Level = edit.Level.Value,
                            NakedEdgeMode = edit.NakedEdges,
                        })));
            },
            refineCatmullClark: static (_, edit) => {
                Op op = RefineCatmullClark.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Source, key: op, body: mesh =>
                    ModelGate.Single(op, () => Mesh.CreateRefinedCatmullClarkMesh(
                        mesh: mesh,
                        settings: new MeshRefinements.RefinementSettings {
                            Level = edit.Level.Value,
                            NakedEdgeMode = edit.NakedEdges,
                        })));
            },
            subdivideMidEdge: static (_, edit) => {
                Op op = SubdivideMidEdge.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Source, key: op, body: mesh => op.Catch(() => {
                    Mesh working = (Mesh)mesh.Duplicate();
                    return op.Confirm(success: edit.Faces.IsEmpty
                            ? working.Subdivide()
                            : working.Subdivide(faceIndices: edit.Faces.AsIterable()))
                        .Bind(_ => ModelGate.Kept(op, working))
                        .Rollback(working);
                }));
            },
            booleanUnion: static (model, edit) => {
                Op op = BooleanUnion.SelfOp;
                return ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Inputs, key: op, body: inputs =>
                    Booled(op, model, options => {
                        Mesh[] products = Mesh.CreateBooleanUnion(
                            meshes: inputs.AsIterable(), options: options,
                            commandResult: out Rhino.Commands.Result verdict, inputMap: out _);
                        return (products, verdict);
                    }));
            },
            booleanIntersection: static (model, edit) => {
                Op op = BooleanIntersection.SelfOp;
                return ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.First, key: op, body: first =>
                    ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Second, key: op, body: second =>
                        Booled(op, model, options => {
                            Mesh[] products = Mesh.CreateBooleanIntersection(
                                firstSet: first.AsIterable(), secondSet: second.AsIterable(), options: options,
                                result: out Rhino.Commands.Result verdict, inputMap: out _);
                            return (products, verdict);
                        })));
            },
            booleanDifference: static (model, edit) => {
                Op op = BooleanDifference.SelfOp;
                return ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.First, key: op, body: first =>
                    ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Second, key: op, body: second =>
                        Booled(op, model, options => {
                            Mesh[] products = Mesh.CreateBooleanDifference(
                                firstSet: first.AsIterable(), secondSet: second.AsIterable(), options: options,
                                result: out Rhino.Commands.Result verdict, inputMap: out _);
                            return (products, verdict);
                        })));
            },
            booleanSplit: static (model, edit) => {
                Op op = BooleanSplit.SelfOp;
                return ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Targets, key: op, body: targets =>
                    ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Cutters, key: op, body: cutters =>
                        Booled(op, model, options => {
                            Mesh[] products = Mesh.CreateBooleanSplit(
                                meshesToSplit: targets.AsIterable(), meshSplitters: cutters.AsIterable(), options: options,
                                result: out Rhino.Commands.Result verdict, inputMap: out _);
                            return (products, verdict);
                        })));
            },
            splitPlane: static (_, edit) => {
                Op op = SplitPlane.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, () => mesh.Split(plane: edit.Plane)));
            },
            splitMeshes: static (model, edit) => {
                Op op = SplitMeshes.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Cutters, key: op, body: cutters =>
                        op.Catch(() => {
                            (bool coplanar, bool ngons) = edit.Policy.Native;
                            using TextLog log = new();
                            return ModelGate.Many(op, () => mesh.Split(
                                    meshes: cutters.AsIterable(),
                                    tolerance: model.Domain.For(lane: ToleranceLane.MeshIntersection).Value,
                                    splitAtCoplanar: coplanar,
                                    createNgons: ngons,
                                    textLog: log,
                                    cancel: model.Cancellation,
                                    progress: model.ScalarReporter), token: model.Cancellation);
                        }, token: model.Cancellation)));
            },
            splitDisjoint: static (_, edit) => {
                Op op = SplitDisjoint.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, () => mesh.SplitDisjointPieces()));
            },
            splitNonManifold: static (_, edit) => {
                Op op = SplitNonManifold.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, () => mesh.SplitNon2Manifolds()));
            },
            splitProjectedPolylines: static (model, edit) => {
                Op op = SplitProjectedPolylines.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.BorrowMany<PolylineCurve, Seq<GeometryHandle>>(handles: edit.Curves, key: op, body: curves =>
                        ModelGate.Many(op, () => mesh.SplitWithProjectedPolylines(
                            curves: curves.AsIterable(), tolerance: model.Domain.Absolute.Value))));
            },
            splitUnweldedEdges: static (_, edit) => {
                Op op = SplitUnweldedEdges.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, () => mesh.ExplodeAtUnweldedEdges()));
            },
            splitCount: static (_, edit) => {
                Op op = SplitCount.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh => {
                    (bool countSum, bool countTriangles) = edit.Mode.Native;
                    return ModelGate.Many(op, () => Mesh.SplitMesh(
                        mesh: mesh, maxCount: edit.MaxCount, countSum: countSum, countTriangles: countTriangles));
                });
            },
            partition: static (_, edit) => {
                Op op = Partition.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, () => Mesh.PartitionMesh(
                        mesh: mesh, maxVertexCount: edit.MaxVertexCount, maxFaceCount: edit.MaxFaceCount)));
            },
            matchEdges: static (_, edit) => {
                Op op = MatchEdges.SelfOp;
                return ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Targets, key: op, body: meshes =>
                    ModelGate.Many(op, () => Mesh.MatchEdges(
                        inputMeshes: meshes.AsIterable(), distance: edit.Law.Distance,
                        simpleSplits: edit.Law.Capabilities.Admits(capability: MeshMatchPolicy.SimpleSplits),
                        rachet: edit.Law.Capabilities.Admits(capability: MeshMatchPolicy.Ratchet),
                        average: edit.Law.Capabilities.Admits(capability: MeshMatchPolicy.Average),
                        join: edit.Law.Capabilities.Admits(capability: MeshMatchPolicy.JoinResult))));
            },
            append: static (_, edit) => {
                Op op = Append.SelfOp;
                return ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Sources, key: op, body: sources =>
                    op.Catch(() => {
                        Mesh working = new();
                        working.Append(meshes: sources.AsIterable());
                        return ModelGate.Own(built: working, key: op).Map(handle => Seq(handle));
                    }));
            },
            projectFaces: static (_, edit) => {
                Op op = ProjectFaces.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh => {
                    FrozenSet<int> selected = edit.Indices.ToFrozenSet();
                    return from _ in guard(
                               selected.All(index => index < mesh.Faces.Count),
                               op.InvalidInput())
                           from built in ModelGate.Single(op, () => Mesh.CreateFromFilteredFaceList(
                               original: mesh,
                               inclusion: Enumerable.Range(start: 0, count: mesh.Faces.Count).Select(selected.Contains)))
                           select built;
                });
            },
            projectNakedEdges: static (_, edit) => {
                Op op = ProjectNakedEdges.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, () => Optional(mesh.GetNakedEdges())
                        .Map(static rows => rows.Map(static row => new PolylineCurve(polyline: row)))
                        .IfNone(Seq<PolylineCurve>()), allowEmpty: true));
            },
            projectOutlines: static (_, edit) => {
                Op op = ProjectOutlines.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    ModelGate.Many(op, () => Optional(mesh.GetOutlines(edit.Frame))
                        .Map(static rows => rows.Map(static outline => new PolylineCurve(polyline: outline)))
                        .IfNone(Seq<PolylineCurve>()), allowEmpty: true));
            },
            edit: static (model, request) => {
                Op op = Edit.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: request.Target, key: op, body: source =>
                    op.Catch(() => {
                        Mesh working = (Mesh)source.Duplicate();
                        return request.Verb.Apply(working: working, runtime: model, op: op).Rollback(working);
                    }));
            },
            extrude: static (_, edit) => {
                Op op = Extrude.SelfOp;
                return ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(handle: edit.Target, key: op, body: mesh =>
                    op.Catch(() => {
                        (bool uvn, bool edgeUvn) = edit.Law.Frame.Native;
                        using MeshExtruder engine = new(inputMesh: mesh, componentIndices: edit.Components.AsIterable()) {
                            Transform = edit.Law.Motion,
                            UVN = uvn,
                            EdgeBasedUVN = edgeUvn,
                            KeepOriginalFaces = edit.Law.KeepOriginalFaces,
                            TextureCoordinateMode = edit.Law.TextureCoordinates,
                            SurfaceParameterMode = edit.Law.SurfaceParameters,
                            FaceDirectionMode = edit.Law.FaceDirection,
                        };
                        return op.Confirm(success: engine.ExtrudedMesh(
                                extrudedMeshOut: out Mesh extruded, componentIndicesOut: out _))
                            .Bind(_ => ModelGate.Own(built: extruded, key: op).Map(handle => Seq(handle)));
                    }));
            });

    private static Fin<Seq<GeometryHandle>> Booled(
        Op op, ModelRuntime model,
        Func<MeshBooleanOptions, (Mesh[] Products, Rhino.Commands.Result Verdict)> run) =>
        op.Catch(() => {
            (Mesh[] products, Rhino.Commands.Result verdict) = run(new MeshBooleanOptions {
                Tolerance = model.Domain.For(lane: ToleranceLane.MeshIntersection).Value,
                CancellationToken = model.Cancellation,
                ProgressReporter = model.ScalarReporter,
            });
            return op.Confirm(success: verdict == Rhino.Commands.Result.Success)
                .Bind(_ => ModelGate.OwnMany(built: products, key: op, allowEmpty: true));
        }, token: model.Cancellation);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostMeshes {
    public static Eff<ModelRuntime, Seq<GeometryHandle>> Build(params ReadOnlySpan<MeshOp> operations) {
        Seq<MeshOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<ModelRuntime>().Bind(runtime =>
            ModelGate.Entry(
                runtime: runtime,
                operations: captured,
                admit: static (operation, key) => operation.Admitted(key: key),
                apply: (operation, _) => operation.Apply(runtime)).ToEff());
    }
}
```

## [07]-[RESEARCH]

(none)
