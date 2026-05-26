using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;

static Mesh Tetrahedron() {
    Mesh mesh = new();
    _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 0.0, y: 1.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 1.0);
    _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 2, vertex3: 1);
    _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 3);
    _ = mesh.Faces.AddFace(vertex1: 1, vertex2: 2, vertex3: 3);
    _ = mesh.Faces.AddFace(vertex1: 2, vertex2: 0, vertex3: 3);
    _ = mesh.Normals.ComputeNormals();
    _ = mesh.Compact();
    return mesh;
}
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
static Mesh DegenerateFaceMesh() {
    Mesh mesh = new();
    _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 0.0, y: 1.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 1.0);
    _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 1.0, y: 1.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 1.0);
    _ = mesh.Vertices.Add(x: 2.0, y: 0.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 2.0, y: 1.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 2.0, y: 0.0, z: 1.0);
    _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 2);
    _ = mesh.Faces.AddFace(vertex1: 3, vertex2: 4, vertex3: 5);
    _ = mesh.Faces.AddFace(vertex1: 6, vertex2: 7, vertex3: 8);
    _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 3, vertex3: 6);
    _ = mesh.Normals.ComputeNormals();
    return mesh;
}
static Mesh TorusMesh(int uCount, int vCount) {
    Mesh mesh = new();
    double major = 3.0;
    double minor = 0.75;
    for (int u = 0; u < uCount; u++) {
        double theta = 2.0 * Math.PI * u / uCount;
        for (int v = 0; v < vCount; v++) {
            double phi = 2.0 * Math.PI * v / vCount;
            double radius = major + (minor * Math.Cos(d: phi));
            _ = mesh.Vertices.Add(x: radius * Math.Cos(d: theta), y: radius * Math.Sin(a: theta), z: minor * Math.Sin(a: phi));
        }
    }
    for (int u = 0; u < uCount; u++) {
        for (int v = 0; v < vCount; v++) {
            int a = (u * vCount) + v;
            int b = (((u + 1) % uCount) * vCount) + v;
            int c = (((u + 1) % uCount) * vCount) + ((v + 1) % vCount);
            int d = (u * vCount) + ((v + 1) % vCount);
            _ = mesh.Faces.AddFace(vertex1: a, vertex2: b, vertex3: c, vertex4: d);
        }
    }
    _ = mesh.Normals.ComputeNormals();
    _ = mesh.Compact();
    return mesh;
}

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
using Mesh native = Tetrahedron();
MeshSpace space = Probe.Expect(MeshSpace.Of(native: native, context: context, key: key), "space");
DiscreteCalculus calculus = Probe.Project<DiscreteCalculus>(intent: VectorIntent.DiscreteCalculus(space: space, key: key), context: context, key: key, label: "dec");
SpectralAssemblyReceipt receipt = calculus.Receipt;
using Mesh openNative = OpenSquare();
MeshSpace openSpace = Probe.Expect(MeshSpace.Of(native: openNative, context: context, key: key), "open space");
SpectralAssemblyReceipt openReceipt = Probe.Project<SpectralAssemblyReceipt>(intent: VectorIntent.DiscreteCalculus(space: openSpace, key: key), context: context, key: key, label: "open dec");
using Mesh degenerateNative = DegenerateFaceMesh();
MeshSpace degenerateSpace = Probe.Expect(MeshSpace.Of(native: degenerateNative, context: context, key: key), "degenerate space");
SpectralAssemblyReceipt degenerateReceipt = Probe.Project<SpectralAssemblyReceipt>(intent: VectorIntent.DiscreteCalculus(space: degenerateSpace, key: key), context: context, key: key, label: "degenerate dec");
using Mesh torusNative = TorusMesh(uCount: 8, vCount: 6);
MeshSpace torusSpace = Probe.Expect(MeshSpace.Of(native: torusNative, context: context, key: key), "torus space");
TopologyReceipt torusTopology = Probe.Project<TopologyReceipt>(intent: VectorIntent.Topology(space: torusSpace, key: key), context: context, key: key, label: "torus topology");
bool genusPositiveHodgeUnsupported = VectorField.Hodge(source: VectorField.Constant(value: Vector3d.XAxis), space: torusSpace, key: key)
    .Bind(field => VectorIntent.Probe(source: ExtractionProbe.Vector(source: field), sample: torusNative.Vertices[0], key: key))
    .Bind(intent => intent.Project<Vector3d>(context: context, key: key))
    .Match(Succ: static _ => false, Fail: static error => error.Category() == "Unsupported");

Probe.Require(calculus.IsValid, "valid DEC");
Probe.Require(receipt.VertexCount == 4 && receipt.EdgeCount == 6 && receipt.FaceCount == 4, $"receipt={receipt}");
Probe.Require(receipt.BoundaryCompositionResidual <= RhinoMath.SqrtEpsilon, $"d1d0={receipt.BoundaryCompositionResidual:R}");
Probe.Require(receipt.PositiveStar0Count == 4 && receipt.PositiveStar2Count == 4, $"stars={receipt}");
Probe.Require(receipt.Genus.Map(static genus => genus == 0).IfNone(false), $"genus={receipt.Genus}");
Probe.Require(receipt.BoundaryEdgeCount == 0 && receipt.BoundaryComponentCount == 0 && receipt.NonManifoldEdgeCount == 0 && receipt.EulerCharacteristic == 2 && receipt.TopologyEulerValidated, $"closed.topology={receipt}");
Probe.Require(openReceipt.BoundaryEdgeCount > 0 && openReceipt.BoundaryComponentCount > 0 && openReceipt.HarmonicDimension == 0, $"open.receipt={openReceipt}");
Probe.Require(degenerateReceipt.Kind.Equals(SpectralAssemblyKind.Dec) && degenerateReceipt.SkippedDegenerateFaces > 0, $"degenerate.receipt={degenerateReceipt}");
Probe.Require(torusTopology.Genus.Map(static genus => genus > 0).IfNone(false) && genusPositiveHodgeUnsupported, $"torus.topology={torusTopology}");

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("dec.nonzeros", receipt.NonZeros);
Evidence.Emit("dec.d1d0", receipt.BoundaryCompositionResidual);
Evidence.Emit("open.boundaryEdges", openReceipt.BoundaryEdgeCount);
Evidence.Emit("torus.genus", torusTopology.Genus.IfNone(-1));
