using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

static T Expect<T>(Fin<T> result, string label) =>
    result.Match(Succ: static value => value, Fail: error => throw new InvalidOperationException($"{label}: {error.Message}"));
static void Require(bool condition, string message) =>
    _ = condition ? true : throw new InvalidOperationException(message);

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
SupportSpace sphere = Expect(SupportSpace.Of(value: new Sphere(Point3d.Origin, 5.0), key: key), "sphere support");
Point3d sample = new(x: 7.0, y: 0.0, z: 0.0);

Point3d closest = Expect(VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Closest).Project<Point3d>(context: context, key: key), "closest point");
double distance = Expect(VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Distance).Project<double>(context: context, key: key), "distance");
Vector3d inward = Expect(VectorIntent.Between(origin: sample, target: sphere, sense: BoundarySense.Toward).Project<Vector3d>(context: context, key: key), "toward span");
Vector3d outward = Expect(VectorIntent.Between(origin: sample, target: sphere, sense: BoundarySense.Away).Project<Vector3d>(context: context, key: key), "away span");
bool uvRejected = VectorIntent.Support(space: sphere, sample: sample, projection: SupportProjection.Uv).Project<Point2d>(context: context, key: key)
    .Match(Succ: static _ => false, Fail: static _ => true);

Require(closest.DistanceTo(new Point3d(x: 5.0, y: 0.0, z: 0.0)) <= 1.0e-6, $"closest={closest}");
Require(Math.Abs(distance - 2.0) <= 1.0e-6, $"distance={distance:R}");
Require(inward.X < 0.0 && Math.Abs(inward.Length - 2.0) <= 1.0e-6, $"inward={inward}");
Require(outward.X > 0.0 && Math.Abs(outward.Length - 2.0) <= 1.0e-6, $"outward={outward}");
Require(uvRejected, "sphere UV projection must reject unsupported output");

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"capture={CAPTURE_PATH}");
Console.WriteLine($"closest={closest}");
Console.WriteLine($"distance={distance:F6}");
Console.WriteLine($"span.toward={inward}");
Console.WriteLine($"span.away={outward}");
