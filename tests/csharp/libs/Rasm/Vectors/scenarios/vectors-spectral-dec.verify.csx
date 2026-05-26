using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;

static T Expect<T>(Fin<T> result, string label) =>
    result.Match(Succ: static value => value, Fail: error => throw new InvalidOperationException($"{label}: {error.Message}"));
static void Require(bool condition, string message) =>
    _ = condition ? true : throw new InvalidOperationException(message);
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

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
using Mesh native = Tetrahedron();
MeshSpace space = Expect(MeshSpace.Of(native: native, context: context, key: key), "space");
DiscreteCalculus calculus = Expect(Expect(VectorIntent.DiscreteCalculus(space: space, key: key), "dec intent").Project<DiscreteCalculus>(context: context, key: key), "dec");
SpectralAssemblyReceipt receipt = calculus.Receipt;

Require(calculus.IsValid, "valid DEC");
Require(receipt.VertexCount == 4 && receipt.EdgeCount == 6 && receipt.FaceCount == 4, $"receipt={receipt}");
Require(receipt.BoundaryCompositionResidual <= RhinoMath.SqrtEpsilon, $"d1d0={receipt.BoundaryCompositionResidual:R}");
Require(receipt.PositiveStar0Count == 4 && receipt.PositiveStar2Count == 4, $"stars={receipt}");
Require(receipt.Genus.Map(static genus => genus == 0).IfNone(false), $"genus={receipt.Genus}");

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"capture={CAPTURE_PATH}");
Console.WriteLine($"dec.nonzeros={receipt.NonZeros}");
Console.WriteLine($"dec.d1d0={receipt.BoundaryCompositionResidual:R}");
