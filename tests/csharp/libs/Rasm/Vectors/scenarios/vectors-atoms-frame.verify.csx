using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
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

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("direction", x.Value);
Evidence.Emit("axis.x", axisX);
Evidence.Emit("frame.z", frame.ZAxis);
Evidence.Emit("components.rejected", componentsRejected);
Evidence.Emit("cone.axis", coneAxis);
