# [RASM_FAULTS]

`GeometryFault` is the geometry domain's direct kernel `Fault` union. Its typed leaves carry the complete failure evidence and lift bare onto `Fin`, `Validation<Error, T>`, and `Eff`; no registry, cluster, message grammar, or lowering wrapper intervenes.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: `GeometryFault` and the typed evidence its direct leaves carry.

## [02]-[FAULT_BAND]

- Owner: `GeometryFault` the direct `FaultBand.Geometry` union.
- Cases: the 60 direct leaves in the fence, each at its compact generated ordinal.
- Entry: construct a case directly and lift it bare onto the `Error` base.
- Auto: `[FaultCase]` generates the cached numeric identity from `FamilyBand`; the typed payload and total presentation message remain authored on this family.
- Law: ordinals are contiguous union structure; no cluster arithmetic, category roster, or wrapper may duplicate or erase a case.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, and the kernel fault substrate.
- Growth: add one direct leaf, compact declaration-order ordinals to `0..N-1`, and set the band span to `N` in the same edit; never add an offset registry or message parser.
- Boundary: geometry failures remain here; structural, BIM, material, fabrication, and host failures keep their owning fault families.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Meshing;
using Rasm.Processing;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;

namespace Rasm.Numerics;

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Geometry;
    private GeometryFault() { }

    [FaultCase(0)] public sealed partial record DegenerateInput(Kind Kind, Option<int> Index, string Detail) : GeometryFault;
    [FaultCase(1)] public sealed partial record PrimitiveCountMismatch(int Expected, int Actual) : GeometryFault;
    [FaultCase(2)] public sealed partial record ArrangementCancelled(ArrangementStage Stage, Option<int> Operand, UnitInterval Progress) : GeometryFault;

    [FaultCase(3)] public sealed partial record NameCollision(TopoName Name, EntityKind Kind) : GeometryFault;
    [FaultCase(4)] public sealed partial record TopologyContentMissing(TopoName Name, EntityKind Kind) : GeometryFault;

    [FaultCase(5)] public sealed partial record UnrepairableMesh(HealStage Stage, Option<Dimension> Budget, int Remaining) : GeometryFault;

    [FaultCase(6)] public sealed partial record InconsistentConstraints(int DependentRows, double Residual) : GeometryFault;
    [FaultCase(7)] public sealed partial record SingularSystem(int Rank, int Parameters) : GeometryFault;

    [FaultCase(8)] public sealed partial record DegenerateOffset(int WavefrontVertex, Option<double> Time = default) : GeometryFault;
    [FaultCase(9)] public sealed partial record CurveSkeletonStalled(int RemainingFaces) : GeometryFault;
    [FaultCase(10)] public sealed partial record ContractionUnconverged(int Iteration, double Residual) : GeometryFault;
    [FaultCase(11)] public sealed partial record StraightSkeletonUnconverged(int PendingEvents, double Time) : GeometryFault;

    [FaultCase(12)] public sealed partial record CellComplexScaleExceeded(long Faces, Dimension Ceiling) : GeometryFault;
    [FaultCase(13)] public sealed partial record ArrangementSubdivisionFailed(int Operand, int Face, Error Cause) : GeometryFault, ICausedFault;
    [FaultCase(14)] public sealed partial record ManifoldOperandRejected(int Operand, ManifoldError Status) : GeometryFault;
    [FaultCase(15)] public sealed partial record ManifoldBooleanRejected(ManifoldError Status) : GeometryFault;
    [FaultCase(16)] public sealed partial record ConstraintRecoveryExhausted(int Constraint, Dimension Budget) : GeometryFault;
    [FaultCase(17)] public sealed partial record WalkExitedHull(int Simplex) : GeometryFault;
    [FaultCase(18)] public sealed partial record WalkLimitReached(int Simplex, int Limit) : GeometryFault;
    [FaultCase(19)] public sealed partial record EmptyCavity(int Simplex) : GeometryFault;
    [FaultCase(20)] public sealed partial record CoplanarInsertionUnsupported(int Simplex) : GeometryFault;
    [FaultCase(21)] public sealed partial record ConstraintCrossingMissing(int A, int B) : GeometryFault;
    [FaultCase(22)] public sealed partial record BlockingFaceMissing(int A, int B) : GeometryFault;
    [FaultCase(23)] public sealed partial record FlipLimitReached(int Edges, Dimension Limit) : GeometryFault;
    [FaultCase(24)] public sealed partial record BisectorUndefined(int Site) : GeometryFault;
    [FaultCase(25)] public sealed partial record VertexUnrepresentable(int Vertex) : GeometryFault;
    [FaultCase(26)] public sealed partial record UnsupportedTessellationProjection(TessellationKind Kind, Type Output) : GeometryFault;
    [FaultCase(27)] public sealed partial record DualRequiresExplicitVertex(int Simplex) : GeometryFault;
    [FaultCase(28)] public sealed partial record CollinearTriangle(int Simplex) : GeometryFault;
    [FaultCase(29)] public sealed partial record CircumcenterInvalid(int Simplex) : GeometryFault;
    [FaultCase(30)] public sealed partial record ManifoldLibraryUnavailable(string RuntimeIdentifier, long Faces, Dimension ManagedCeiling) : GeometryFault;

    [FaultCase(31)] public sealed partial record NonManifoldIntersection(PrimitiveKind A, PrimitiveKind B, int Junction) : GeometryFault;
    [FaultCase(32)] public sealed partial record MissingIntersectionVertex(PrimitiveKind A, PrimitiveKind B, int Vertex) : GeometryFault;
    [FaultCase(33)] public sealed partial record IncompleteIntersectionWalk(PrimitiveKind A, PrimitiveKind B, int From, int To) : GeometryFault;
    [FaultCase(34)] public sealed partial record OpenSection(int Layer, double Elevation, int Chains) : GeometryFault;
    [FaultCase(35)] public sealed partial record InvalidSectionNesting(int Layer, double Elevation, int Contours) : GeometryFault;

    [FaultCase(36)] public sealed partial record InsufficientInliers(UnitInterval Inliers, UnitInterval Floor) : GeometryFault;

    [FaultCase(37)] public sealed partial record InvalidChartBoundary(int Loops, Option<int> Vertices) : GeometryFault;
    [FaultCase(38)] public sealed partial record IncompleteParameterizationSpectrum(int Expected, int Actual) : GeometryFault;
    [FaultCase(39)] public sealed partial record ParameterizationUnconverged(Option<double> Residual, int Iterations) : GeometryFault;
    [FaultCase(40)] public sealed partial record FlippedChart(ChartId Chart, double MaxConformal) : GeometryFault;

    [FaultCase(41)] public sealed partial record EmptyProjection : GeometryFault { }
    [FaultCase(42)] public sealed partial record HatchFailed(HatchPattern Pattern, int Region, string Detail) : GeometryFault;

    [FaultCase(43)] public sealed partial record FaceBudgetExceeded(int Budget, int Actual) : GeometryFault;
    [FaultCase(44)] public sealed partial record RemeshUnconverged(PositiveMagnitude TargetLength, Option<double> Achieved, int Iterations) : GeometryFault;

    [FaultCase(45)] public sealed partial record ChannelWidthMismatch(EncodingChannel Channel, int Actual) : GeometryFault;
    [FaultCase(46)] public sealed partial record DuplicateEncodingChannel(EncodingChannel Channel) : GeometryFault;
    [FaultCase(47)] public sealed partial record ChannelArityMismatch(EncodingChannel Channel, long Expected, long Actual) : GeometryFault;
    [FaultCase(48)] public sealed partial record UnboundEncodingChannel(EncodingChannel Channel) : GeometryFault;
    [FaultCase(49)] public sealed partial record EncodingPayloadTooLarge(long Bytes) : GeometryFault;
    [FaultCase(50)] public sealed partial record EncodingRoundTripExceeded(EncodingChannel Channel, double Error) : GeometryFault;
    [FaultCase(51)] public sealed partial record MissingEncodingChannel(EncodingChannel Channel) : GeometryFault;

    [FaultCase(52)] public sealed partial record InvalidKnotVector(int Degree, int KnotCount, string Detail) : GeometryFault;
    [FaultCase(53)] public sealed partial record LengthInversionUnconverged(double Target) : GeometryFault;
    [FaultCase(54)] public sealed partial record CurveProjectionUnconverged(Point3d Probe) : GeometryFault;
    [FaultCase(55)] public sealed partial record OffsetUnconverged(Kind Kind, double Deviation) : GeometryFault;
    [FaultCase(56)] public sealed partial record NoDevelopableStrips : GeometryFault { }
    [FaultCase(57)] public sealed partial record StripIsometryExceeded(int Strip, double Distortion, Tolerance Limit) : GeometryFault;
    [FaultCase(58)] public sealed partial record PanelPlanarityExceeded(int Panel, double Deviation, Tolerance Limit) : GeometryFault;
    [FaultCase(59)] public sealed partial record FaceAspectRatioExceeded(int Face, PositiveMagnitude AspectRatio, PositiveMagnitude Ceiling) : GeometryFault;

    public sealed override string Message => Switch(
        degenerateInput: static fault => $"Degenerate {fault.Kind} input at {fault.Index}: {fault.Detail}.",
        primitiveCountMismatch: static fault => $"Spatial primitive count mismatch: expected {fault.Expected}, actual {fault.Actual}.",
        arrangementCancelled: static fault => $"Arrangement {fault.Stage} cancelled at {fault.Progress}; operand={fault.Operand}.",
        nameCollision: static fault => $"Topology name {fault.Name} collides for {fault.Kind}.",
        topologyContentMissing: static fault => $"Topology content for {fault.Name} ({fault.Kind}) is absent from the rebuild.",
        unrepairableMesh: static fault => $"Mesh repair stopped at {fault.Stage}; remaining={fault.Remaining}, budget={fault.Budget}.",
        inconsistentConstraints: static fault => $"Constraint system has {fault.DependentRows} dependent rows at residual {fault.Residual:R}.",
        singularSystem: static fault => $"Constraint system rank {fault.Rank} is singular for {fault.Parameters} parameters.",
        degenerateOffset: static fault => $"Offset wavefront degenerated at vertex {fault.WavefrontVertex}, time={fault.Time}.",
        curveSkeletonStalled: static fault => $"Curve-skeleton surgery stalled with {fault.RemainingFaces} faces remaining.",
        contractionUnconverged: static fault => $"Mesh contraction did not converge after {fault.Iteration} iterations; residual={fault.Residual:R}.",
        straightSkeletonUnconverged: static fault => $"Straight-skeleton propagation did not converge; pending={fault.PendingEvents}, time={fault.Time:R}.",
        cellComplexScaleExceeded: static fault => $"Cell complex has {fault.Faces} faces; managed ceiling={fault.Ceiling}.",
        arrangementSubdivisionFailed: static fault => $"Arrangement operand {fault.Operand} face {fault.Face} subdivision failed: {fault.Cause.Message}",
        manifoldOperandRejected: static fault => $"Manifold operand {fault.Operand} was rejected with status {fault.Status}.",
        manifoldBooleanRejected: static fault => $"Manifold boolean was rejected with status {fault.Status}.",
        constraintRecoveryExhausted: static fault => $"Constraint {fault.Constraint} exhausted recovery budget {fault.Budget}.",
        walkExitedHull: static fault => $"Tessellation walk exited the hull from simplex {fault.Simplex}.",
        walkLimitReached: static fault => $"Tessellation walk from simplex {fault.Simplex} reached limit {fault.Limit}.",
        emptyCavity: static fault => $"Tessellation cavity at simplex {fault.Simplex} has no boundary.",
        coplanarInsertionUnsupported: static fault => $"Coplanar insertion at simplex {fault.Simplex} is unsupported in tetrahedralization.",
        constraintCrossingMissing: static fault => $"Constraint edge ({fault.A}, {fault.B}) has no crossing or on-segment vertex.",
        blockingFaceMissing: static fault => $"Constraint edge ({fault.A}, {fault.B}) has no blocking tetrahedron face.",
        flipLimitReached: static fault => $"{fault.Edges} tessellation edges exceeded flip limit {fault.Limit}.",
        bisectorUndefined: static fault => $"Voronoi bisector at site {fault.Site} is numerically undefined.",
        vertexUnrepresentable: static fault => $"Tessellation vertex {fault.Vertex} cannot be represented explicitly.",
        unsupportedTessellationProjection: static fault => $"{fault.Kind} cannot project to {fault.Output.Name}.",
        dualRequiresExplicitVertex: static fault => $"Dual simplex {fault.Simplex} contains an implicit vertex.",
        collinearTriangle: static fault => $"Dual simplex {fault.Simplex} is collinear.",
        circumcenterInvalid: static fault => $"Dual simplex {fault.Simplex} produced an invalid circumcenter.",
        manifoldLibraryUnavailable: static fault => $"Manifold library is unavailable for {fault.RuntimeIdentifier}; faces={fault.Faces}, managed ceiling={fault.ManagedCeiling}.",
        nonManifoldIntersection: static fault => $"Intersection between {fault.A} and {fault.B} branches at vertex {fault.Junction}.",
        missingIntersectionVertex: static fault => $"Intersection between {fault.A} and {fault.B} is missing vertex {fault.Vertex}.",
        incompleteIntersectionWalk: static fault => $"Intersection between {fault.A} and {fault.B} left edge ({fault.From}, {fault.To}) unvisited.",
        openSection: static fault => $"Section layer {fault.Layer} at elevation {fault.Elevation:R} has {fault.Chains} open chains.",
        invalidSectionNesting: static fault => $"Section layer {fault.Layer} at elevation {fault.Elevation:R} has invalid nesting across {fault.Contours} contours.",
        insufficientInliers: static fault => $"Fit inlier fraction {fault.Inliers} is below floor {fault.Floor}.",
        invalidChartBoundary: static fault => $"Chart boundary is invalid; loops={fault.Loops}, vertices={fault.Vertices}.",
        incompleteParameterizationSpectrum: static fault => $"Parameterization spectrum has {fault.Actual} modes; expected {fault.Expected}.",
        parameterizationUnconverged: static fault => $"Parameterization did not converge after {fault.Iterations} iterations; residual={fault.Residual}.",
        flippedChart: static fault => $"Chart {fault.Chart} contains a flipped face; maximum conformal distortion={fault.MaxConformal:R}.",
        emptyProjection: static _ => "Projection produced no drawable edges.",
        hatchFailed: static fault => $"Hatch {fault.Pattern} region {fault.Region} failed: {fault.Detail}.",
        faceBudgetExceeded: static fault => $"Decimation produced {fault.Actual} faces; budget={fault.Budget}.",
        remeshUnconverged: static fault => $"Remeshing did not converge after {fault.Iterations} iterations; target={fault.TargetLength}, achieved={fault.Achieved}.",
        channelWidthMismatch: static fault => $"Encoding channel {fault.Channel} requires width {fault.Channel.Dtype.Width}; actual={fault.Actual}.",
        duplicateEncodingChannel: static fault => $"Encoding channel {fault.Channel} is duplicated.",
        channelArityMismatch: static fault => $"Encoding channel {fault.Channel} requires {fault.Expected} values; actual={fault.Actual}.",
        unboundEncodingChannel: static fault => $"Encoding channel {fault.Channel} has no bound lane.",
        encodingPayloadTooLarge: static fault => $"Encoding payload has {fault.Bytes} bytes; maximum={Array.MaxLength}.",
        encodingRoundTripExceeded: static fault => $"Encoding channel {fault.Channel} round-trip error {fault.Error:R} exceeds {fault.Channel.Dtype.Tolerance:R}.",
        missingEncodingChannel: static fault => $"Encoding channel {fault.Channel} is missing.",
        invalidKnotVector: static fault => $"Knot vector degree {fault.Degree}, count {fault.KnotCount} is invalid: {fault.Detail}.",
        lengthInversionUnconverged: static fault => $"Length inversion did not converge at target {fault.Target:R}.",
        curveProjectionUnconverged: static fault => $"Curve projection did not converge for point {fault.Probe}.",
        offsetUnconverged: static fault => $"{fault.Kind} offset did not converge; deviation={fault.Deviation:R}.",
        noDevelopableStrips: static _ => "Development produced no developable strips.",
        stripIsometryExceeded: static fault => $"Strip {fault.Strip} distortion {fault.Distortion:R} exceeds {fault.Limit}.",
        panelPlanarityExceeded: static fault => $"Panel {fault.Panel} deviation {fault.Deviation:R} exceeds {fault.Limit}.",
        faceAspectRatioExceeded: static fault => $"Face {fault.Face} aspect ratio {fault.AspectRatio} exceeds ceiling {fault.Ceiling}.");
}

```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
