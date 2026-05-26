using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
BoundingBox isoBounds = new(min: new Point3d(-6.0, -6.0, -6.0), max: new Point3d(6.0, 6.0, 6.0));

ScalarField sphereField = Probe.Expect(
    ScalarField.Primitive(
        kind: SdfKind.Sphere,
        parameters: ImmutableDictionary<string, double>.Empty.Add(key: "r", value: 3.0),
        pose: Plane.WorldXY,
        key: key),
    "sphere field");
IsoSurfaceResult analyticIso = Probe.Project<IsoSurfaceResult>(
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
MeshSpace boxSpace = Probe.Expect(MeshSpace.Of(native: closedBox, context: context, key: key), "box space");
SdfMeshPolicy windingPolicy = Probe.Expect(SdfMeshPolicy.GeneralizedWinding(key: key), "winding policy");
ScalarField boxField = Probe.Expect(ScalarField.SignedDistanceFromMesh(space: boxSpace, policy: windingPolicy, key: key), "box sdf");
IsoSurfaceResult meshIso = Probe.Project<IsoSurfaceResult>(
    intent: VectorIntent.IsoSurface(field: boxField, bounds: isoBounds, resolution: 16, maxRootSteps: 16, key: key),
    context: context,
    key: key,
    label: "mesh iso");
SdfSample meshSdf = Probe.Expect(boxField.SampleSdfDetailed(sample: new Point3d(x: 4.0, y: 0.0, z: 0.0), context: context, key: key), "mesh sdf sample");
SdfMeshReceipt meshReceipt = Probe.ExpectSome(meshSdf.Receipt.Mesh, "mesh receipt");

Mesh openSquare = Mesh.CreateFromPlane(
    plane: Plane.WorldXY,
    xInterval: new Interval(t0: -1.0, t1: 1.0),
    yInterval: new Interval(t0: -1.0, t1: 1.0),
    xCount: 1,
    yCount: 1);
openSquare.Normals.ComputeNormals();
openSquare.Compact();
MeshSpace openSpace = Probe.Expect(MeshSpace.Of(native: openSquare, context: context, key: key), "open space");
SdfMeshPolicy boundaryPolicy = Probe.Expect(SdfMeshPolicy.BoundarySignedHeat(key: key), "boundary policy");
bool boundarySignedHeatRejected = ScalarField.SignedDistanceFromMesh(space: openSpace, policy: boundaryPolicy, key: key)
    .Bind(field => field.SampleSdfDetailed(sample: new Point3d(x: 0.25, y: 0.25, z: 0.1), context: context, key: key))
    .Match(Succ: static _ => false, Fail: static _ => true);

VolumeGridPolicy closedGrid = Probe.Expect(VolumeGridPolicy.ByResolution(resolution: 8, padding: 1.0, key: key), "closed grid policy");
SdfMeshPolicy closedPolicy = Probe.Expect(SdfMeshPolicy.ClosedSignedHeat(grid: closedGrid, key: key), "closed signed heat policy");
ScalarField closedField = Probe.Expect(ScalarField.SignedDistanceFromMesh(space: boxSpace, policy: closedPolicy, key: key), "closed signed heat field");
bool closedSignedHeatRejected = closedField.SampleSdfDetailed(sample: Point3d.Origin, context: context, key: key)
    .Match(Succ: static _ => false, Fail: static _ => true);
SdfMeshPolicy flippedClosedPolicy = Probe.Expect(SdfMeshPolicy.ClosedSignedHeat(grid: closedGrid, signConvention: SdfSignConvention.PositiveInsideNegativeOutside, key: key), "flipped closed signed heat policy");
ScalarField flippedClosedField = Probe.Expect(ScalarField.SignedDistanceFromMesh(space: boxSpace, policy: flippedClosedPolicy, key: key), "flipped closed signed heat field");
bool flippedClosedSignedHeatRejected = flippedClosedField.SampleSdfDetailed(sample: Point3d.Origin, context: context, key: key)
    .Match(Succ: static _ => false, Fail: static _ => true);
bool closedIsoRejected = VectorIntent.IsoSurface(field: closedField, bounds: new BoundingBox(min: new Point3d(x: -3.5, y: -3.5, z: -3.5), max: new Point3d(x: 3.5, y: 3.5, z: 3.5)), resolution: 12, maxRootSteps: 16, key: key)
    .Bind(intent => intent.Project<IsoSurfaceReceipt>(context: context, key: key))
    .Match(Succ: static _ => false, Fail: static _ => true);

bool openClosedSignedHeatRejected = ScalarField.SignedDistanceFromMesh(space: openSpace, policy: closedPolicy, key: key)
    .Bind(field => field.SampleSdfDetailed(sample: Point3d.Origin, context: context, key: key))
    .Match(Succ: static _ => false, Fail: static _ => true);

SupportSpace sphereSupport = Probe.Expect(SupportSpace.Of(value: new Sphere(center: Point3d.Origin, radius: 3.0), key: key), "sphere support");
double containment = Probe.Project<double>(
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
bool evaluatorFailureReceiptRejected = Probe.Expect(invalidIsoIntent, "invalid iso intent").Project<IsoSurfaceReceipt>(context: context, key: key)
    .Match(Succ: static _ => false, Fail: static _ => true);

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

Probe.Require(analyticIso.Receipt.ParallelCallback && analyticIso.Receipt.FixedTolerance.Match(Some: static tolerance => Math.Abs(tolerance - 0.001) <= 1.0e-12, None: static () => false), "analytic iso receipt lost native callback/tolerance facts");
Probe.Require(analyticIso.Mesh.IsValid && analyticIso.Receipt.VertexCount > 0 && analyticIso.Receipt.FaceCount > 0, "analytic iso mesh invalid");
Probe.Require(meshIso.Mesh.IsValid && meshReceipt.Domain.Equals(SdfMeshDomain.SurfaceMesh) && meshReceipt.Status.Equals(SdfMeshStatus.ApproximateSignClosestDistance), "mesh iso receipt lost generalized-winding facts");
Probe.Require(boundarySignedHeatRejected, "boundary signed heat open-source sample should reject until product owner admits a valid native result");
Probe.Require(closedSignedHeatRejected && flippedClosedSignedHeatRejected && closedIsoRejected, "closed signed heat runtime paths should reject current native input until product owner admits valid samples");
Probe.Require(openClosedSignedHeatRejected, "open mesh must reject closed SignedHeat");
Probe.Require(RhinoMath.IsValidDouble(x: containment) && containment > 0.0, $"containment={containment:R}");
Probe.Require(evaluatorFailureRejected && evaluatorFailureReceiptRejected, "native iso-surface evaluator failure path should reject current invalid scalar input");
Probe.Require(nativeIso is { IsValid: true } && sampleCount > 0 && threadIdsSeen.Count >= 1, "native callback evidence missing");

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.EmitFacts(new Dictionary<string, object> {
    ["analyticVertices"] = analyticIso.Receipt.VertexCount,
    ["meshVertices"] = meshIso.Receipt.VertexCount,
    ["fixedTolerance"] = 0.001,
    ["parallelCallback"] = analyticIso.Receipt.ParallelCallback,
    ["meshMethod"] = "GeneralizedWindingNumber",
    ["meshStatus"] = "ApproximateSignClosestDistance",
    ["meshSolid"] = meshReceipt.Topology.IsSolid,
    ["boundaryMethod"] = "BoundarySignedHeat",
    ["boundaryRejected"] = boundarySignedHeatRejected,
    ["closedMethod"] = "ClosedSurfaceSignedHeat",
    ["closedRejected"] = closedSignedHeatRejected,
    ["flippedClosedRejected"] = flippedClosedSignedHeatRejected,
    ["closedIsoRejected"] = closedIsoRejected,
    ["openClosedSignedHeatRejected"] = openClosedSignedHeatRejected,
    ["evaluatorFailureRejected"] = evaluatorFailureRejected,
    ["evaluatorFailureReceiptRejected"] = evaluatorFailureReceiptRejected,
    ["threadIdsSeen"] = threadIdsSeen.Count,
    ["sampleCount"] = sampleCount,
});
