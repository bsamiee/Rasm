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

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
VectorCloud cloud = Expect(VectorCloud.Cluster(
    points: Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.0, y: 0.0, z: 1.0),
        new Point3d(x: 0.25, y: 0.25, z: 0.25)),
    context: context,
    key: key), "cluster");
CloudHullResult result = Expect(Expect(VectorIntent.Hull(source: cloud, kind: CloudHullKind.Convex3D, key: key), "hull intent").Project<CloudHullResult>(context: context, key: key), "hull");
Mesh hull = Expect(result.Mesh.ToFin(Fail: key.InvalidResult()), "hull mesh");

Require(result.Receipt.Status.Equals(CloudHullStatus.Completed), $"status={result.Receipt.Status}");
Require(result.Receipt.NativeRouted && result.Receipt.NativeFacetCount > 0, $"facets={result.Receipt.NativeFacetCount}");
Require(result.Receipt.InputCount == 5 && result.Receipt.OutputVertexCount >= 4, $"receipt={result.Receipt}");
Require(hull.IsValid && hull.Faces.Count > 0, $"mesh.faces={hull.Faces.Count}");

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"capture={CAPTURE_PATH}");
Console.WriteLine($"hull.vertices={result.Receipt.OutputVertexCount}");
Console.WriteLine($"hull.facets={result.Receipt.NativeFacetCount}");
