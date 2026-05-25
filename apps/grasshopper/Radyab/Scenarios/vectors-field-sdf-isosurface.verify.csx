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
ScalarField boxField = Expect(ScalarField.SignedDistanceFromMesh(space: boxSpace, method: SdfMeshMethod.GeneralizedWindingNumber, key: key), "box sdf");
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
ScalarField boundaryField = Expect(ScalarField.SignedDistanceFromMesh(space: openSpace, method: SdfMeshMethod.BoundarySignedHeat, key: key), "boundary signed heat sdf");
SdfSample boundarySample = Expect(boundaryField.SampleSdfDetailed(sample: new Point3d(x: 0.25, y: 0.25, z: 0.1), context: context, key: key), "boundary signed heat sample");
SdfMeshReceipt boundaryReceipt = ExpectSome(boundarySample.Receipt.Mesh, "boundary receipt");
SignedHeatReceipt signedHeat = ExpectSome(boundaryReceipt.SignedHeat, "signed heat receipt");

bool closedSignedHeatRejected = ScalarField.SignedDistanceFromMesh(space: boxSpace, method: SdfMeshMethod.ClosedSurfaceSignedHeat, key: key)
    .Bind(field => field.SampleSdfDetailed(sample: Point3d.Origin, context: context, key: key))
    .Match(Succ: static _ => false, Fail: static _ => true);

SupportSpace sphereSupport = Expect(SupportSpace.Of(value: new Sphere(center: Point3d.Origin, radius: 3.0), key: key), "sphere support");
double containment = Project<double>(
    intent: VectorIntent.Support(space: sphereSupport, sample: new Point3d(x: 4.0, y: 0.0, z: 0.0), projection: SupportProjection.ContainmentDistance, key: key),
    context: context,
    key: key,
    label: "support containment");

bool evaluatorFailureRejected = VectorIntent.IsoSurface(
        field: ScalarField.Constant(value: double.NaN),
        bounds: isoBounds,
        resolution: 8,
        maxRootSteps: 4,
        key: key)
    .Bind(intent => intent.Project<IsoSurfaceResult>(context: context, key: key))
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

Require(analyticIso.Receipt.ParallelCallback && analyticIso.Receipt.FixedTolerance.Match(Some: static tolerance => Math.Abs(tolerance - 0.001) <= 1.0e-12, None: static () => false), "analytic iso receipt lost native callback/tolerance facts");
Require(analyticIso.Mesh.IsValid && analyticIso.Receipt.VertexCount > 0 && analyticIso.Receipt.FaceCount > 0, "analytic iso mesh invalid");
Require(meshIso.Mesh.IsValid && meshReceipt.UsesGeneralizedWindingApproximation && meshReceipt.Status.Equals(SdfMeshStatus.ApproximateSignClosestDistance), "mesh iso receipt lost generalized-winding facts");
Require(boundaryReceipt.UsesBoundarySignedHeat && boundaryReceipt.Status.Equals(SdfMeshStatus.BoundarySourceSignedHeat), "boundary signed heat receipt lost status");
Require(signedHeat.BoundarySourceVertexCount > 0 && signedHeat.BoundaryEncodedEdgeSourceCount > 0 && signedHeat.HeatSolve.Solution.Count > 0 && signedHeat.PoissonSolve.Solution.Count > 0, "boundary signed heat solve/source facts invalid");
Require(closedSignedHeatRejected, "closed SignedHeat must remain unsupported");
Require(RhinoMath.IsValidDouble(x: containment) && containment > 0.0, $"containment={containment:R}");
Require(evaluatorFailureRejected, "native iso-surface evaluator failure must reject");
Require(nativeIso is { IsValid: true } && sampleCount > 0 && threadIdsSeen.Count >= 1, "native callback evidence missing");

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"capture={CAPTURE_PATH}");
Console.WriteLine($"facts={{\"analyticVertices\":{analyticIso.Receipt.VertexCount},\"meshVertices\":{meshIso.Receipt.VertexCount},\"fixedTolerance\":0.001,\"parallelCallback\":{analyticIso.Receipt.ParallelCallback.ToString().ToLowerInvariant()},\"meshMethod\":\"GeneralizedWindingNumber\",\"meshStatus\":\"ApproximateSignClosestDistance\",\"meshSolid\":{meshReceipt.IsSolid.ToString().ToLowerInvariant()},\"boundaryMethod\":\"BoundarySignedHeat\",\"boundaryStatus\":\"BoundarySourceSignedHeat\",\"boundaryComponents\":{boundaryReceipt.BoundaryComponents},\"nonManifoldEdges\":{boundaryReceipt.NonManifoldEdges},\"signedHeatSources\":{signedHeat.BoundarySourceVertexCount},\"signedHeatEncodedEdges\":{signedHeat.BoundaryEncodedEdgeSourceCount},\"heatResidual\":{signedHeat.HeatSolve.Residual:R},\"poissonResidual\":{signedHeat.PoissonSolve.Residual:R},\"closedSignedHeatRejected\":{closedSignedHeatRejected.ToString().ToLowerInvariant()},\"evaluatorFailureRejected\":{evaluatorFailureRejected.ToString().ToLowerInvariant()},\"threadIdsSeen\":{threadIdsSeen.Count},\"sampleCount\":{sampleCount}}}");
