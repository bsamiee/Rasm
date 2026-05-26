using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
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

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("closest", closest);
Evidence.Emit("distance", distance.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
Evidence.Emit("span.toward", inward);
Evidence.Emit("span.away", outward);
Evidence.Emit("uv", uv);
