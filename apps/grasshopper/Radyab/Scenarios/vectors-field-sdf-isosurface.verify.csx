using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

static T Expect<T>(Fin<T> result, string label) =>
    result.Match(Succ: static value => value, Fail: error => throw new InvalidOperationException($"{label}: {error.Message}"));
static T ExpectSome<T>(Option<T> result, string label) =>
    result.Match(Some: static value => value, None: () => throw new InvalidOperationException($"{label}: missing"));
static void Require(bool condition, string message) =>
    _ = condition ? true : throw new InvalidOperationException(message);
static T Project<T>(Fin<VectorIntent> intent, Context context, Op key, string label) =>
    Expect(Expect(intent, $"{label}: intent").Project<T>(context: context, key: key), $"{label}: project");

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
BoundingBox isoBounds = new(min: new Point3d(-6.0, -6.0, -6.0), max: new Point3d(6.0, 6.0, 6.0));

ScalarField sphereField = Expect(
    ScalarField.Primitive(
        kind: SdfKind.Sphere,
        parameters: ImmutableDictionary<string, double>.Empty.Add(key: "r", value: 3.0),
        pose: Plane.WorldXY,
        key: key),
    "sphere field");
IsoSurfaceResult analyticIso = Project<IsoSurfaceResult>(
    intent: VectorIntent.IsoSurface(field: sphereField, bounds: isoBounds, resolution: 18, maxRootSteps: 16, key: key),
    context: context,
    key: key,
    label: "analytic iso");

Mesh closedBox = Mesh.CreateFromBox(
    box: new BoundingBox(min: new Point3d(-3.0, -3.0, -3.0), max: new Point3d(3.0, 3.0, 3.0)),
    xCount: 1,
    yCount: 1,
    zCount: 1);
closedBox.Normals.ComputeNormals();
closedBox.Compact();
MeshSpace boxSpace = Expect(MeshSpace.Of(native: closedBox, context: context, key: key), "box space");
SdfMeshPolicy windingPolicy = Expect(SdfMeshPolicy.GeneralizedWinding(key: key), "winding policy");
ScalarField boxField = Expect(ScalarField.SignedDistanceFromMesh(space: boxSpace, policy: windingPolicy, key: key), "box sdf");
IsoSurfaceResult meshIso = Project<IsoSurfaceResult>(
    intent: VectorIntent.IsoSurface(field: boxField, bounds: isoBounds, resolution: 16, maxRootSteps: 16, key: key),
    context: context,
    key: key,
    label: "mesh iso");
SdfSample meshSdf = Expect(boxField.SampleSdfDetailed(sample: new Point3d(x: 4.0, y: 0.0, z: 0.0), context: context, key: key), "mesh sdf sample");
SdfMeshReceipt meshReceipt = ExpectSome(meshSdf.Receipt.Mesh, "mesh receipt");

Mesh openSquare = Mesh.CreateFromPlane(
    plane: Plane.WorldXY,
    xInterval: new Interval(t0: -1.0, t1: 1.0),
    yInterval: new Interval(t0: -1.0, t1: 1.0),
    xCount: 1,
    yCount: 1);
openSquare.Normals.ComputeNormals();
openSquare.Compact();
MeshSpace openSpace = Expect(MeshSpace.Of(native: openSquare, context: context, key: key), "open space");
SdfMeshPolicy boundaryPolicy = Expect(SdfMeshPolicy.BoundarySignedHeat(key: key), "boundary policy");
ScalarField boundaryField = Expect(ScalarField.SignedDistanceFromMesh(space: openSpace, policy: boundaryPolicy, key: key), "boundary signed heat sdf");
SdfSample boundarySample = Expect(boundaryField.SampleSdfDetailed(sample: new Point3d(x: 0.25, y: 0.25, z: 0.1), context: context, key: key), "boundary signed heat sample");
SdfMeshReceipt boundaryReceipt = ExpectSome(boundarySample.Receipt.Mesh, "boundary receipt");
SignedHeatReceipt signedHeat = ExpectSome(boundaryReceipt.SignedHeat, "signed heat receipt");
SolveReceipt boundaryHeatSolve = ExpectSome(signedHeat.HeatSolve, "boundary heat solve");

VolumeGridPolicy closedGrid = Expect(VolumeGridPolicy.ByResolution(resolution: 8, padding: 1.0, key: key), "closed grid policy");
SdfMeshPolicy closedPolicy = Expect(SdfMeshPolicy.ClosedSignedHeat(grid: closedGrid, key: key), "closed signed heat policy");
ScalarField closedField = Expect(ScalarField.SignedDistanceFromMesh(space: boxSpace, policy: closedPolicy, key: key), "closed signed heat field");
SdfSample closedInside = Expect(closedField.SampleSdfDetailed(sample: Point3d.Origin, context: context, key: key), "closed signed heat inside");
SdfSample closedOutside = Expect(closedField.SampleSdfDetailed(sample: new Point3d(x: 3.4, y: 0.0, z: 0.0), context: context, key: key), "closed signed heat outside");
SdfMeshPolicy flippedClosedPolicy = Expect(SdfMeshPolicy.ClosedSignedHeat(grid: closedGrid, signConvention: SdfSignConvention.PositiveInsideNegativeOutside, key: key), "flipped closed signed heat policy");
ScalarField flippedClosedField = Expect(ScalarField.SignedDistanceFromMesh(space: boxSpace, policy: flippedClosedPolicy, key: key), "flipped closed signed heat field");
SdfSample flippedClosedInside = Expect(flippedClosedField.SampleSdfDetailed(sample: Point3d.Origin, context: context, key: key), "flipped closed signed heat inside");
SdfSample flippedClosedOutside = Expect(flippedClosedField.SampleSdfDetailed(sample: new Point3d(x: 3.4, y: 0.0, z: 0.0), context: context, key: key), "flipped closed signed heat outside");
SdfMeshReceipt closedReceipt = ExpectSome(closedInside.Receipt.Mesh, "closed receipt");
VolumeGridReceipt volumeReceipt = ExpectSome(closedReceipt.VolumeGrid, "closed volume grid receipt");
IsoSurfaceReceipt closedIsoReceipt = Project<IsoSurfaceReceipt>(
    intent: VectorIntent.IsoSurface(field: closedField, bounds: new BoundingBox(min: new Point3d(-3.5, -3.5, -3.5), max: new Point3d(3.5, 3.5, 3.5)), resolution: 12, maxRootSteps: 16, key: key),
    context: context,
    key: key,
    label: "closed signed heat iso receipt");
SdfMeshReceipt closedIsoPreflight = ExpectSome(closedIsoReceipt.MeshPreflight, "closed iso mesh preflight");

bool openClosedSignedHeatRejected = ScalarField.SignedDistanceFromMesh(space: openSpace, policy: closedPolicy, key: key)
    .Bind(field => field.SampleSdfDetailed(sample: Point3d.Origin, context: context, key: key))
    .Match(Succ: static _ => false, Fail: static _ => true);

SupportSpace sphereSupport = Expect(SupportSpace.Of(value: new Sphere(center: Point3d.Origin, radius: 3.0), key: key), "sphere support");
double containment = Project<double>(
    intent: VectorIntent.Support(space: sphereSupport, sample: new Point3d(x: 4.0, y: 0.0, z: 0.0), projection: SupportProjection.ContainmentDistance, key: key),
    context: context,
    key: key,
    label: "support containment");

Fin<VectorIntent> invalidIsoIntent = VectorIntent.IsoSurface(
        field: ScalarField.Constant(value: double.NaN),
        bounds: isoBounds,
        resolution: 8,
        maxRootSteps: 4,
        key: key);
bool evaluatorFailureRejected = invalidIsoIntent
    .Bind(intent => intent.Project<IsoSurfaceResult>(context: context, key: key))
    .Match(Succ: static _ => false, Fail: static _ => true);
IsoSurfaceReceipt evaluatorFailureReceipt = Expect(Expect(invalidIsoIntent, "invalid iso intent").Project<IsoSurfaceReceipt>(context: context, key: key), "invalid iso receipt");

ConcurrentDictionary<int, int> threadIdsSeen = new();
int sampleCount = 0;
Mesh nativeIso = Mesh.CreateFromIsosurface(
    scalarFieldEvaluator: point => {
        _ = threadIdsSeen.AddOrUpdate(key: Environment.CurrentManagedThreadId, addValue: 1, updateValueFactory: static (_, count) => count + 1);
        _ = Interlocked.Increment(location: ref sampleCount);
        return point.DistanceTo(other: Point3d.Origin) - 2.0;
    },
    box: isoBounds,
    resolution: 18,
    RootFindingMaxSteps: 16);

Require(analyticIso.Receipt.ParallelCallback && analyticIso.Receipt.FixedTolerance.Match(Some: static tolerance => Math.Abs(tolerance - 0.001) <= 1.0e-12, None: static () => false), "analytic iso receipt lost native callback/tolerance facts");
Require(analyticIso.Mesh.IsValid && analyticIso.Receipt.VertexCount > 0 && analyticIso.Receipt.FaceCount > 0, "analytic iso mesh invalid");
Require(meshIso.Mesh.IsValid && meshReceipt.Domain.Equals(SdfMeshDomain.SurfaceMesh) && meshReceipt.Status.Equals(SdfMeshStatus.ApproximateSignClosestDistance), "mesh iso receipt lost generalized-winding facts");
Require(boundaryReceipt.Domain.Equals(SdfMeshDomain.BoundarySource) && boundaryReceipt.Status.Equals(SdfMeshStatus.BoundarySourceSignedHeat), "boundary signed heat receipt lost status");
Require(signedHeat.BoundarySourceVertexCount > 0 && signedHeat.BoundaryEncodedEdgeSourceCount > 0 && boundaryHeatSolve.Solution.Count > 0 && signedHeat.PoissonSolve.Solution.Count > 0, "boundary signed heat solve/source facts invalid");
Require(closedReceipt.Domain.Equals(SdfMeshDomain.VolumeGrid) && closedReceipt.Status.Equals(SdfMeshStatus.ClosedSurfaceSignedHeat) && volumeReceipt.NodeCount > 0 && volumeReceipt.OperatorNonZeros > 0, "closed SignedHeat volume-grid facts invalid");
Require(closedInside.Value < 0.0 && closedOutside.Value > 0.0, $"closed signs invalid inside={closedInside.Value:R} outside={closedOutside.Value:R}");
Require(flippedClosedInside.Value > 0.0 && flippedClosedOutside.Value < 0.0, $"flipped closed signs invalid inside={flippedClosedInside.Value:R} outside={flippedClosedOutside.Value:R}");
Require(closedIsoPreflight.Domain.Equals(SdfMeshDomain.VolumeGrid) && closedIsoPreflight.Status.Equals(SdfMeshStatus.ClosedSurfaceSignedHeat) && closedIsoPreflight.VolumeGrid.Match(Some: static grid => grid.NodeCount > 0, None: static () => false), "closed iso mesh preflight lost volume-grid facts");
Require(openClosedSignedHeatRejected, "open mesh must reject closed SignedHeat");
Require(RhinoMath.IsValidDouble(x: containment) && containment > 0.0, $"containment={containment:R}");
Require(evaluatorFailureRejected && evaluatorFailureReceipt.Status.Equals(IsoSurfaceStatus.EvaluatorFailure), "native iso-surface evaluator failure receipt must be preserved");
Require(nativeIso is { IsValid: true } && sampleCount > 0 && threadIdsSeen.Count >= 1, "native callback evidence missing");

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"capture={CAPTURE_PATH}");
Console.WriteLine($"facts={{\"analyticVertices\":{analyticIso.Receipt.VertexCount},\"meshVertices\":{meshIso.Receipt.VertexCount},\"fixedTolerance\":0.001,\"parallelCallback\":{analyticIso.Receipt.ParallelCallback.ToString().ToLowerInvariant()},\"meshMethod\":\"GeneralizedWindingNumber\",\"meshStatus\":\"ApproximateSignClosestDistance\",\"meshSolid\":{meshReceipt.Topology.IsSolid.ToString().ToLowerInvariant()},\"boundaryMethod\":\"BoundarySignedHeat\",\"boundaryStatus\":\"BoundarySourceSignedHeat\",\"boundaryComponents\":{boundaryReceipt.Topology.BoundaryComponents},\"nonManifoldEdges\":{boundaryReceipt.Topology.NonManifoldEdges},\"signedHeatSources\":{signedHeat.BoundarySourceVertexCount},\"signedHeatEncodedEdges\":{signedHeat.BoundaryEncodedEdgeSourceCount},\"heatResidual\":{boundaryHeatSolve.Residual:R},\"poissonResidual\":{signedHeat.PoissonSolve.Residual:R},\"closedMethod\":\"ClosedSurfaceSignedHeat\",\"closedDomain\":\"VolumeGrid\",\"closedNodes\":{volumeReceipt.NodeCount},\"closedCells\":{volumeReceipt.CellCount},\"closedSources\":{volumeReceipt.SourceTriangleCount},\"closedResidual\":{volumeReceipt.Residual:R},\"closedInside\":{closedInside.Value:R},\"closedOutside\":{closedOutside.Value:R},\"flippedClosedInside\":{flippedClosedInside.Value:R},\"flippedClosedOutside\":{flippedClosedOutside.Value:R},\"openClosedSignedHeatRejected\":{openClosedSignedHeatRejected.ToString().ToLowerInvariant()},\"evaluatorFailureRejected\":{evaluatorFailureRejected.ToString().ToLowerInvariant()},\"evaluatorFailureStatus\":\"EvaluatorFailure\",\"threadIdsSeen\":{threadIdsSeen.Count},\"sampleCount\":{sampleCount}}}");
