using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

static Mesh OpenSquare() {
    Mesh mesh = Mesh.CreateFromPlane(
        plane: Plane.WorldXY,
        xInterval: new Interval(t0: -1.0, t1: 1.0),
        yInterval: new Interval(t0: -1.0, t1: 1.0),
        xCount: 1,
        yCount: 1);
    _ = mesh.Normals.ComputeNormals();
    _ = mesh.Compact();
    return mesh;
}

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
using Mesh native = OpenSquare();
MeshSpace space = Probe.Expect(MeshSpace.Of(native: native, context: context, key: key), "space");
VolumeSolverPolicy solver = Probe.Expect(VolumeSolverPolicy.SparseCholesky(residualTolerance: 1.0e-4, key: key), "solver policy");
SdfMeshPolicy policy = Probe.Expect(SdfMeshPolicy.BoundarySignedHeat(solver: solver, key: key), "boundary policy");
ScalarField field = Probe.Expect(ScalarField.SignedDistanceFromMesh(space: space, policy: policy, key: key), "boundary signed heat field");
SdfSample sample = Probe.Expect(field.SampleSdfDetailed(sample: new Point3d(x: 0.25, y: 0.25, z: 0.1), context: context, key: key), "boundary signed heat sample");
SdfMeshReceipt mesh = Probe.ExpectSome(result: sample.Receipt.Mesh, label: "mesh receipt");
SignedHeatReceipt signed = Probe.ExpectSome(result: mesh.SignedHeat, label: "signed heat receipt");
SpectralAssemblyReceipt edge = Probe.ExpectSome(result: signed.EdgeAssembly, label: "edge assembly receipt");
SolveReceipt heat = Probe.ExpectSome(result: signed.HeatSolve, label: "heat solve");

Probe.Require(edge.Kind == SpectralAssemblyKind.EdgeConnection && edge.EdgeCount > 0 && edge.ComponentCount == 2, $"edge.space={edge}");
Probe.Require(edge.MatrixRows == edge.EdgeCount * edge.ComponentCount && edge.MatrixCols == edge.MatrixRows, $"edge.shape={edge}");
Probe.Require(edge.AdmittedFaceCount + edge.SkippedDegenerateFaces == edge.FaceCount, $"edge.faces={edge}");
Probe.Require(edge.PositiveMassCount > 0 && edge.PositiveMassCount <= edge.EdgeCount, $"edge.mass={edge}");
Probe.Require(edge.SymmetryResidual <= RhinoMath.SqrtEpsilon && edge.FactorNonZeros.IsSome, $"edge.factor={edge}");
Probe.Require(heat.IsUsable && signed.PoissonSolve.IsUsable, $"solves heat={heat} poisson={signed.PoissonSolve}");

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("edgeDofs", edge.EdgeCount);
Evidence.Emit("edgeRows", edge.MatrixRows);
Evidence.Emit("edgeNonZeros", edge.NonZeros);
Evidence.Emit("edgeFactorNonZeros", edge.FactorNonZeros.IfNone(0));
Evidence.Emit("solverTolerance", solver.ResidualTolerance.Value);
