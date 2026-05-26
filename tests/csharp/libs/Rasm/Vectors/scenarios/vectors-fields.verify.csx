using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Rasm.RhinoBridge.Protocol;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;

Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");

// --- [SCENARIO: vectors-field-sdf-isosurface] -------------------------------------------
{
    const string theme = "vectors-field-sdf-isosurface";
    Op key = Op.Of(name: theme);
    BoundingBox isoBounds = new(min: new Point3d(-6.0, -6.0, -6.0), max: new Point3d(6.0, 6.0, 6.0));
    BoundingBox nonCubicBounds = new(min: new Point3d(-4.0, -2.0, -1.0), max: new Point3d(4.0, 2.0, 1.0));
    ScalarField sphereField = Probe.Expect(
        ScalarField.Primitive(
            kind: SdfKind.Sphere,
            parameters: ImmutableDictionary<string, double>.Empty.Add(key: "r", value: 3.0),
            pose: Plane.WorldXY,
            key: key),
        "sphere field");
    IsoSurfaceResult analyticIso = Probe.Project<IsoSurfaceResult>(
        intent: VectorIntent.IsoSurface(field: sphereField, bounds: isoBounds, resolution: 18, maxRootSteps: 16, key: key),
        context: context, key: key, label: "analytic iso");
    IsoSurfaceReceipt nonCubicIso = Probe.Project<IsoSurfaceReceipt>(
        intent: VectorIntent.IsoSurface(field: sphereField, bounds: nonCubicBounds, resolution: 4, maxRootSteps: 12, key: key),
        context: context, key: key, label: "non-cubic iso");
    Mesh closedBox = Mesh.CreateFromBox(
        box: new BoundingBox(min: new Point3d(-3.0, -3.0, -3.0), max: new Point3d(3.0, 3.0, 3.0)),
        xCount: 1, yCount: 1, zCount: 1);
    closedBox.Normals.ComputeNormals();
    closedBox.Compact();
    MeshSpace boxSpace = Probe.Expect(MeshSpace.Of(native: closedBox, context: context, key: key), "box space");
    SdfMeshPolicy windingPolicy = Probe.Expect(SdfMeshPolicy.GeneralizedWinding(key: key), "winding policy");
    ScalarField boxField = Probe.Expect(ScalarField.SignedDistanceFromMesh(space: boxSpace, policy: windingPolicy, key: key), "box sdf");
    IsoSurfaceResult meshIso = Probe.Project<IsoSurfaceResult>(
        intent: VectorIntent.IsoSurface(field: boxField, bounds: isoBounds, resolution: 16, maxRootSteps: 16, key: key),
        context: context, key: key, label: "mesh iso");
    SdfSample meshSdf = Probe.Expect(boxField.SampleSdfDetailed(sample: new Point3d(x: 4.0, y: 0.0, z: 0.0), context: context, key: key), "mesh sdf sample");
    SdfMeshReceipt meshReceipt = Probe.ExpectSome(meshSdf.Receipt.Mesh, "mesh receipt");
    Mesh openSquare = Mesh.CreateFromPlane(
        plane: Plane.WorldXY,
        xInterval: new Interval(t0: -1.0, t1: 1.0),
        yInterval: new Interval(t0: -1.0, t1: 1.0),
        xCount: 1, yCount: 1);
    openSquare.Normals.ComputeNormals();
    openSquare.Compact();
    MeshSpace openSpace = Probe.Expect(MeshSpace.Of(native: openSquare, context: context, key: key), "open space");
    SdfMeshPolicy boundaryPolicy = Probe.Expect(SdfMeshPolicy.BoundarySignedHeat(key: key), "boundary policy");
    bool boundaryDefaultToleranceRejected = ScalarField.SignedDistanceFromMesh(space: openSpace, policy: boundaryPolicy, key: key)
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
        context: context, key: key, label: "support containment");
    Fin<VectorIntent> invalidIsoIntent = VectorIntent.IsoSurface(
        field: ScalarField.Constant(value: double.NaN),
        bounds: isoBounds, resolution: 8, maxRootSteps: 4, key: key);
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
        box: isoBounds, resolution: 18, RootFindingMaxSteps: 16);
    Probe.Require(analyticIso.Receipt.ParallelCallback && analyticIso.Receipt.FixedTolerance.Match(Some: static tolerance => Math.Abs(tolerance - 0.001) <= 1.0e-12, None: static () => false), "analytic iso receipt lost native callback/tolerance facts");
    Probe.Require(analyticIso.Mesh.IsValid && analyticIso.Receipt.VertexCount > 0 && analyticIso.Receipt.FaceCount > 0, "analytic iso mesh invalid");
    Probe.Require(analyticIso.Receipt.MeshPreflight.IsNone && meshIso.Receipt.MeshPreflight.IsSome, "iso mesh prewarm receipts lost analytic/mesh distinction");
    Probe.Require(nonCubicIso.Grid.XCells == 16 && nonCubicIso.Grid.YCells == 8 && nonCubicIso.Grid.ZCells == 4 && nonCubicIso.Grid.InitialSampleCount == 1277, $"nonCubic.grid={nonCubicIso.Grid}");
    Probe.Require(meshIso.Mesh.IsValid && meshReceipt.Domain.Equals(SdfMeshDomain.SurfaceMesh) && meshReceipt.Status.Equals(SdfMeshStatus.ApproximateSignClosestDistance), "mesh iso receipt lost generalized-winding facts");
    Probe.Require(boundaryDefaultToleranceRejected, "boundary signed heat default solver tolerance should reject this open-source sample");
    Probe.Require(closedSignedHeatRejected && flippedClosedSignedHeatRejected && closedIsoRejected, "closed signed heat runtime paths should reject current native input until product owner admits valid samples");
    Probe.Require(openClosedSignedHeatRejected, "open mesh must reject closed SignedHeat");
    Probe.Require(RhinoMath.IsValidDouble(x: containment) && containment > 0.0, $"containment={containment:R}");
    Probe.Require(evaluatorFailureRejected && evaluatorFailureReceiptRejected, "native iso-surface evaluator failure path should reject current invalid scalar input");
    Probe.Require(nativeIso is { IsValid: true } && sampleCount > 0 && threadIdsSeen.Count >= 1, "native callback evidence missing");
    BridgeMarker.EmitScenarioHeader(scenario: theme, capturePath: CAPTURE_PATH);
    BridgeMarker.EmitFacts(new Dictionary<string, object> {
        ["analyticVertices"] = analyticIso.Receipt.VertexCount,
        ["nonCubicCells"] = $"{nonCubicIso.Grid.XCells}x{nonCubicIso.Grid.YCells}x{nonCubicIso.Grid.ZCells}",
        ["nonCubicInitialSamples"] = nonCubicIso.Grid.InitialSampleCount,
        ["meshVertices"] = meshIso.Receipt.VertexCount,
        ["fixedTolerance"] = 0.001,
        ["parallelCallback"] = analyticIso.Receipt.ParallelCallback,
        ["meshMethod"] = "GeneralizedWindingNumber",
        ["meshStatus"] = "ApproximateSignClosestDistance",
        ["meshSolid"] = meshReceipt.Topology.IsSolid,
        ["boundaryMethod"] = "BoundarySignedHeat",
        ["boundaryDefaultToleranceRejected"] = boundaryDefaultToleranceRejected,
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
}

// --- [SCENARIO: vectors-atoms-frame] ----------------------------------------------------
{
    const string theme = "vectors-atoms-frame";
    Op key = Op.Of(name: theme);
    Direction x = Probe.Expect(Direction.Of(value: new Vector3d(x: 4.0, y: 0.0, z: 0.0), context: context, key: key), "direction x");
    Vector3d axisX = Probe.Expect(VectorIntent.Axis(axis: SignedAxis.PositiveX).Project<Vector3d>(context: context, key: key), "axis x");
    Plane frame = Probe.Expect(VectorIntent.Frame(origin: Point3d.Origin, normal: Vector3d.ZAxis, xHint: Some(Vector3d.XAxis)).Project<Plane>(context: context, key: key), "frame");
    bool componentsRejected = VectorIntent.Components(anchor: Point3d.Origin, value: new Vector3d(x: 3.0, y: 4.0, z: 0.0), frame: Plane.WorldXY).Project<ValueTuple<double, double>>(context: context, key: key)
        .Match(Succ: static _ => false, Fail: static _ => true);
    VectorCone cone = Probe.Expect(VectorCone.Of(apex: Point3d.Origin, axis: Vector3d.ZAxis, halfAngleRadians: Math.PI / 6.0, context: context, key: key), "cone");
    Vector3d coneAxis = Probe.Expect(VectorIntent.Cone(cone: cone, mode: ConeProjection.Axis).Project<Vector3d>(context: context, key: key), "cone axis");
    bool containsAxis = Probe.Expect(cone.Contains(query: Vector3d.ZAxis, context: context, key: key), "cone contains axis");
    bool rejectsOpposite = Probe.Expect(cone.Contains(query: -Vector3d.ZAxis, context: context, key: key), "cone rejects opposite");
    Probe.Require(Math.Abs(x.Value.Length - 1.0) <= 1.0e-12, $"direction length={x.Value.Length:R}");
    Probe.Require(axisX.IsParallelTo(other: Vector3d.XAxis) == 1, $"axisX={axisX}");
    Probe.Require(frame.IsValid && Vector3d.AreOrthonormal(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis), $"frame={frame}");
    Probe.Require(componentsRejected, "components tuple projection should reject current native result");
    Probe.Require(coneAxis.IsParallelTo(other: Vector3d.ZAxis) == 1, $"coneAxis={coneAxis}");
    Probe.Require(containsAxis && !rejectsOpposite, "cone containment rail inverted");
    BridgeMarker.EmitScenarioHeader(scenario: theme, capturePath: CAPTURE_PATH);
    BridgeMarker.EmitFact(key: "direction", value: x.Value);
    BridgeMarker.EmitFact(key: "axis.x", value: axisX);
    BridgeMarker.EmitFact(key: "frame.z", value: frame.ZAxis);
    BridgeMarker.EmitFact(key: "components.rejected", value: componentsRejected);
    BridgeMarker.EmitFact(key: "cone.axis", value: coneAxis);
}

// --- [SCENARIO: vectors-space-projection] -----------------------------------------------
{
    const string theme = "vectors-space-projection";
    Op key = Op.Of(name: theme);
    SupportSpace sphere = Probe.Expect(SupportSpace.Of(value: new Sphere(Point3d.Origin, 5.0), key: key), "sphere support");
    Point3d sample = new(x: 7.0, y: 0.0, z: 0.0);
    Point3d closest = Probe.Expect(Probe.Expect(VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Closest, key: key), "closest intent").Project<Point3d>(context: context, key: key), "closest point");
    double distance = Probe.Expect(Probe.Expect(VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Distance, key: key), "distance intent").Project<double>(context: context, key: key), "distance");
    Vector3d inward = Probe.Expect(Probe.Expect(VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Span, key: key), "toward span intent").Project<Vector3d>(context: context, key: key), "toward span");
    Vector3d outward = Probe.Expect(Probe.Expect(VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.SignedSpanAway, key: key), "away span intent").Project<Vector3d>(context: context, key: key), "away span");
    bool closestVectorRejected = Probe.Expect(VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Closest, key: key), "closest vector intent").Project<Vector3d>(context: context, key: key)
        .Match(Succ: static _ => false, Fail: static _ => true);
    Point2d uv = Probe.Expect(Probe.Expect(VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Uv, key: key), "uv intent").Project<Point2d>(context: context, key: key), "uv");
    Probe.Require(closest.DistanceTo(new Point3d(x: 5.0, y: 0.0, z: 0.0)) <= 1.0e-6, $"closest={closest}");
    Probe.Require(Math.Abs(distance - 2.0) <= 1.0e-6, $"distance={distance:R}");
    Probe.Require(inward.X < 0.0 && Math.Abs(inward.Length - 2.0) <= 1.0e-6, $"inward={inward}");
    Probe.Require(outward.X > 0.0 && Math.Abs(outward.Length - 2.0) <= 1.0e-6, $"outward={outward}");
    Probe.Require(closestVectorRejected, "closest projection must reject unsupported vector output");
    Probe.Require(RhinoMath.IsValidDouble(uv.X) && RhinoMath.IsValidDouble(uv.Y), $"uv={uv}");
    BridgeMarker.EmitScenarioHeader(scenario: theme, capturePath: CAPTURE_PATH);
    BridgeMarker.EmitFact(key: "closest", value: closest);
    BridgeMarker.EmitFact(key: "distance", value: distance.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
    BridgeMarker.EmitFact(key: "span.toward", value: inward);
    BridgeMarker.EmitFact(key: "span.away", value: outward);
    BridgeMarker.EmitFact(key: "uv", value: uv);
}
