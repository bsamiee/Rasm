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
Direction x = Expect(Direction.Of(value: new Vector3d(x: 4.0, y: 0.0, z: 0.0), context: context, key: key), "direction x");
Plane frame = Expect(VectorIntent.Frame(origin: Point3d.Origin, normal: Vector3d.ZAxis, xHint: Some(Vector3d.XAxis)).Project<Plane>(context: context, key: key), "frame");
(double cx, double cy) = Expect(VectorIntent.Components(anchor: Point3d.Origin, value: new Vector3d(x: 3.0, y: 4.0, z: 0.0), frame: Plane.WorldXY).Project<ValueTuple<double, double>>(context: context, key: key), "components");
VectorCone cone = Expect(VectorCone.Of(apex: Point3d.Origin, axis: Vector3d.ZAxis, halfAngleRadians: Math.PI / 6.0, context: context, key: key), "cone");
Vector3d coneAxis = Expect(VectorIntent.Cone(cone: cone, mode: ConeProjection.Axis).Project<Vector3d>(context: context, key: key), "cone axis");
bool containsAxis = Expect(cone.Contains(query: Vector3d.ZAxis, context: context, key: key), "cone contains axis");
bool rejectsOpposite = Expect(cone.Contains(query: -Vector3d.ZAxis, context: context, key: key), "cone rejects opposite");

Require(Math.Abs(x.Value.Length - 1.0) <= 1.0e-12, $"direction length={x.Value.Length:R}");
Require(frame.IsValid && Vector3d.AreOrthonormal(x: frame.XAxis, y: frame.YAxis, z: frame.ZAxis), $"frame={frame}");
Require(Math.Abs(cx - 3.0) <= 1.0e-9 && Math.Abs(cy - 4.0) <= 1.0e-9, $"components=({cx:R},{cy:R})");
Require(coneAxis.IsParallelTo(other: Vector3d.ZAxis) == 1, $"coneAxis={coneAxis}");
Require(containsAxis && !rejectsOpposite, "cone containment rail inverted");

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"capture={CAPTURE_PATH}");
Console.WriteLine($"direction={x.Value}");
Console.WriteLine($"frame.z={frame.ZAxis}");
Console.WriteLine($"components=({cx:F6},{cy:F6})");
Console.WriteLine($"cone.axis={coneAxis}");
