# [RASM_FAULTS]

`GeometryFault` is the geometry domain's direct kernel `Fault` union. Its typed leaves carry the complete failure evidence and lift bare onto `Fin`, `Validation<Error, T>`, and `Eff`; no registry, cluster, message grammar, or lowering wrapper intervenes.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: `GeometryFault` and the typed stage, carrier, and witness vocabularies its leaves compose.

## [02]-[FAULT_BAND]

- Owner: `GeometryFault` the direct `FaultBand.Geometry` union; `ParametricStage`, `DevelopmentStage`, `ParametricCarrier`, and the witness rosters type its payloads.
- Cases: the 28 direct leaves in the fence, each at its compact generated ordinal.
- Entry: construct a case directly and lift it bare onto the `Error` rail.
- Auto: `[FaultCase]` generates the cached numeric identity from `FamilyBand`; the typed payload and total presentation message remain authored on this family.
- Law: ordinals are contiguous union structure; no cluster arithmetic, category roster, or wrapper may duplicate or erase a case.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, and the kernel fault substrate.
- Growth: add one direct leaf, compact declaration-order ordinals to `0..N-1`, and set the band span to `N` in the same edit; never add an offset registry or message parser.
- Boundary: geometry failures remain here; structural, BIM, material, fabrication, and host failures keep their owning fault families.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Meshing;
using Rasm.Processing;
using Rasm.Spatial;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParametricStage {
    public static readonly ParametricStage Construction = new("construction");
    public static readonly ParametricStage Evaluation   = new("evaluation");
    public static readonly ParametricStage Station      = new("station");
    public static readonly ParametricStage Offset       = new("offset");
    public static readonly ParametricStage Encode       = new("encode");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DevelopmentStage {
    public static readonly DevelopmentStage Subdivision = new("subdivision");
    public static readonly DevelopmentStage Strip       = new("strip");
    public static readonly DevelopmentStage Panel       = new("panel");
    public static readonly DevelopmentStage Pattern     = new("pattern");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParametricCarrier {
    public static readonly ParametricCarrier Curve    = new("curve");
    public static readonly ParametricCarrier Surface  = new("surface");
    public static readonly ParametricCarrier Knots    = new("knots");
    public static readonly ParametricCarrier Fill     = new("fill");
    public static readonly ParametricCarrier Station  = new("station");
    public static readonly ParametricCarrier Geodesic = new("geodesic");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TessellationWitness {
    public static readonly TessellationWitness OffHullWalk            = new("off-hull-walk");
    public static readonly TessellationWitness WalkOverran            = new("walk-overran");
    public static readonly TessellationWitness EmptyCavity            = new("empty-cavity");
    public static readonly TessellationWitness OnFaceCdtetGated       = new("on-face-cdtet-gated");
    public static readonly TessellationWitness NoCrossing             = new("no-crossing");
    public static readonly TessellationWitness NoBlockingFace         = new("no-blocking-face");
    public static readonly TessellationWitness FlipBudgetSpent        = new("flip-budget-spent");
    public static readonly TessellationWitness BisectorDenominator    = new("bisector-denominator");
    public static readonly TessellationWitness UnrepresentableVertex  = new("unrepresentable-vertex");
    public static readonly TessellationWitness ProjectionMismatch     = new("projection-mismatch");
    public static readonly TessellationWitness ImplicitBearingDual    = new("implicit-bearing-dual");
    public static readonly TessellationWitness CollinearTriangle      = new("collinear-triangle");
    public static readonly TessellationWitness CircumcircleOverflow   = new("circumcircle-overflow");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArrangementWitness {
    public static readonly ArrangementWitness LatticeUnavailable   = new("lattice-unavailable");
    public static readonly ArrangementWitness WindingUnavailable   = new("winding-index-unavailable");
    public static readonly ArrangementWitness Substrate            = new("substrate");
    public static readonly ArrangementWitness NoNativeCellComplex  = new("no-native-cell-complex");
    public static readonly ArrangementWitness OperandRejected      = new("operand-rejected");
    public static readonly ArrangementWitness BooleanStatus        = new("boolean-status");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AbandonWitness {
    public static readonly AbandonWitness SubdivideA      = new("subdivide-a", done: Some(0.00));
    public static readonly AbandonWitness SubdivideB      = new("subdivide-b", done: Some(0.25));
    public static readonly AbandonWitness Classify        = new("classify", done: Some(0.50));
    public static readonly AbandonWitness Weld            = new("weld", done: Some(0.75));
    public static readonly AbandonWitness NativeCancelled = new("native-cancelled", done: None);

    public Option<double> Done { get; }
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Geometry;
    private GeometryFault() { }

    [FaultCase(0)] public sealed partial record DegenerateInput(Kind Kind, Option<int> Index, string Witness) : GeometryFault;
    [FaultCase(1)] public sealed partial record IndexMismatch(EntityKind Kind, int Expected, int Actual) : GeometryFault;
    [FaultCase(2)] public sealed partial record KindMismatch(SpatialKind Index, QueryKind Query) : GeometryFault;
    [FaultCase(3)] public sealed partial record RunAbandoned(Kind Kind, UnitInterval Progress, AbandonWitness Witness) : GeometryFault;

    [FaultCase(4)] public sealed partial record NameCollision(TopoName Name, EntityKind Kind) : GeometryFault;
    [FaultCase(5)] public sealed partial record HashMismatch(TopoName Name, EntityKind Kind) : GeometryFault;

    [FaultCase(6)] public sealed partial record UnrepairableMesh(HealStage Stage, Option<Dimension> Budget, int Remaining) : GeometryFault;

    [FaultCase(7)] public sealed partial record OverConstrained(int RedundantRows, double Residual) : GeometryFault;
    [FaultCase(8)] public sealed partial record SingularSystem(int Rank, int Parameters) : GeometryFault;

    [FaultCase(9)] public sealed partial record DegenerateOffset(int WavefrontVertex, Option<double> Time = default) : GeometryFault;
    [FaultCase(10)] public sealed partial record SkeletonStalled(int PendingEvents, Option<double> Time = default) : GeometryFault { public override Retriability Retriability => Retriability.Transient; }
    [FaultCase(11)] public sealed partial record CollapseStalled(int Iteration, double Residual) : GeometryFault { public override Retriability Retriability => Retriability.Transient; }

    [FaultCase(12)] public sealed partial record DegenerateArrangement(int CellCount, ArrangementWitness ManifoldWitness, Option<int> Native = default) : GeometryFault;
    [FaultCase(13)] public sealed partial record ConstraintUnrecoverable(int Constraint, int Budget) : GeometryFault { public override Retriability Retriability => Retriability.Transient; }
    [FaultCase(14)] public sealed partial record DegenerateTessellation(int Simplex, TessellationWitness Witness) : GeometryFault;
    [FaultCase(15)] public sealed partial record NativeAssetMissing(NativeEngine Engine, string Rid, long Ceiling) : GeometryFault;

    [FaultCase(16)] public sealed partial record IntersectionFault(PrimitiveKind A, PrimitiveKind B, Option<int> Junction = default) : GeometryFault;
    [FaultCase(17)] public sealed partial record SectionFault(int Layer, double Elevation, int OpenChains) : GeometryFault;

    [FaultCase(18)] public sealed partial record FitFault(UnitInterval Inliers, UnitInterval Floor) : GeometryFault;

    [FaultCase(19)] public sealed partial record ParameterizationFault(Option<ChartId> Chart, double Distortion) : GeometryFault;

    [FaultCase(20)] public sealed partial record ProjectionFault(EdgeKind Kind, int Segment) : GeometryFault;
    [FaultCase(21)] public sealed partial record HatchFault(HatchPattern Pattern, int Region, string Witness) : GeometryFault;

    [FaultCase(22)] public sealed partial record DecimationFault(int FaceBudget, int Achieved) : GeometryFault;
    [FaultCase(23)] public sealed partial record RemeshStalled(PositiveMagnitude TargetLength, Option<double> Achieved, int Iterations) : GeometryFault { public override Retriability Retriability => Retriability.Transient; }

    [FaultCase(24)] public sealed partial record EncodingFault(
        EncodingChannel Channel,
        ChannelDtype Dtype,
        EncodingStage Stage,
        Option<double> Expected = default,
        Option<double> Actual = default) : GeometryFault;

    [FaultCase(25)] public sealed partial record ParametricFault(ParametricStage Stage, ParametricCarrier Carrier, string Witness) : GeometryFault;
    [FaultCase(26)] public sealed partial record DevelopmentFault(DevelopmentStage Stage, Option<int> Panel, string Witness, Option<double> Measure = default) : GeometryFault;
    [FaultCase(27)] public sealed partial record CotangentQuality(int Face, PositiveMagnitude Ratio, PositiveMagnitude Ceiling) : GeometryFault;

    public sealed override string Message => Switch(
        degenerateInput: static fault => $"Degenerate {fault.Kind} input at {fault.Index}: {fault.Witness}.",
        indexMismatch: static fault => $"{fault.Kind} index count mismatch: expected {fault.Expected}, actual {fault.Actual}.",
        kindMismatch: static fault => $"Spatial index {fault.Index} cannot answer query {fault.Query}.",
        runAbandoned: static fault => $"{fault.Kind} run abandoned at {fault.Progress} during {fault.Witness}.",
        nameCollision: static fault => $"Topology name {fault.Name} collides for {fault.Kind}.",
        hashMismatch: static fault => $"Topology hash for {fault.Name} no longer matches {fault.Kind}.",
        unrepairableMesh: static fault => $"Mesh repair stopped at {fault.Stage}; remaining={fault.Remaining}, budget={fault.Budget}.",
        overConstrained: static fault => $"Constraint system has {fault.RedundantRows} redundant rows at residual {fault.Residual:R}.",
        singularSystem: static fault => $"Constraint system rank {fault.Rank} is singular for {fault.Parameters} parameters.",
        degenerateOffset: static fault => $"Offset wavefront degenerated at vertex {fault.WavefrontVertex}, time={fault.Time}.",
        skeletonStalled: static fault => $"Skeleton propagation stalled with {fault.PendingEvents} pending events, time={fault.Time}.",
        collapseStalled: static fault => $"Collapse stalled at iteration {fault.Iteration}, residual={fault.Residual:R}.",
        degenerateArrangement: static fault => $"Arrangement of {fault.CellCount} cells failed under {fault.ManifoldWitness}; native={fault.Native}.",
        constraintUnrecoverable: static fault => $"Constraint {fault.Constraint} exhausted recovery budget {fault.Budget}.",
        degenerateTessellation: static fault => $"Tessellation simplex {fault.Simplex} failed under {fault.Witness}.",
        nativeAssetMissing: static fault => $"Native engine {fault.Engine} has no asset for {fault.Rid} within ceiling {fault.Ceiling}.",
        intersectionFault: static fault => $"Intersection between {fault.A} and {fault.B} failed at junction {fault.Junction}.",
        sectionFault: static fault => $"Section layer {fault.Layer} at elevation {fault.Elevation:R} has {fault.OpenChains} open chains.",
        fitFault: static fault => $"Fit inlier fraction {fault.Inliers} is below floor {fault.Floor}.",
        parameterizationFault: static fault => $"Parameterization failed for chart {fault.Chart}; distortion={fault.Distortion:R}.",
        projectionFault: static fault => $"Projection of {fault.Kind} segment {fault.Segment} failed.",
        hatchFault: static fault => $"Hatch {fault.Pattern} region {fault.Region} failed: {fault.Witness}.",
        decimationFault: static fault => $"Decimation achieved {fault.Achieved} faces against budget {fault.FaceBudget}.",
        remeshStalled: static fault => $"Remeshing stalled after {fault.Iterations} iterations; target={fault.TargetLength}, achieved={fault.Achieved}.",
        encodingFault: static fault => $"Encoding {fault.Channel}/{fault.Dtype} failed at {fault.Stage}; expected={fault.Expected}, actual={fault.Actual}.",
        parametricFault: static fault => $"Parametric {fault.Carrier} failed at {fault.Stage}: {fault.Witness}.",
        developmentFault: static fault => $"Development failed at {fault.Stage}, panel={fault.Panel}, measure={fault.Measure}: {fault.Witness}.",
        cotangentQuality: static fault => $"Face {fault.Face} cotangent aspect ratio {fault.Ratio} exceeds ceiling {fault.Ceiling}.");
}

```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
