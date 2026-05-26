using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;

static T Project<T>(VectorCloud cloud, VectorCloudMetric metric, Context context, Op key) =>
    Probe.Expect(Probe.Expect(VectorIntent.Cloud(cloud: cloud, metric: metric, key: key), $"{metric}: intent").Project<T>(context: context, key: key), $"{metric}: project");

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
Seq<Point3d> ringPoints = Seq(
    new Point3d(x: 0.0, y: 0.0, z: 0.0),
    new Point3d(x: 2.0, y: 0.0, z: 0.0),
    new Point3d(x: 2.0, y: 1.0, z: 0.0),
    new Point3d(x: 0.0, y: 1.0, z: 0.0));
VectorCloud ring = Probe.Expect(VectorCloud.Ring(points: ringPoints, context: context, key: key), "ring");
VectorCloud cluster = Probe.Expect(VectorCloud.WeightedCluster(
    points: Seq(new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 3.0, y: 0.0, z: 0.0), new Point3d(x: 3.0, y: 3.0, z: 0.0)),
    mass: Seq(2.0, 3.0, 5.0),
    context: context,
    key: key), "weighted cluster");
VectorCloud shifted = Probe.Expect(VectorCloud.Cluster(
    points: Seq(new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 4.0, y: 0.0, z: 0.0), new Point3d(x: 4.0, y: 3.0, z: 0.0)),
    context: context,
    key: key), "target cluster");

double area = Project<double>(cloud: ring, metric: VectorCloudMetric.Area, context: context, key: key);
VectorCloudShape shape = Project<VectorCloudShape>(cloud: ring, metric: VectorCloudMetric.Shape, context: context, key: key);
Vector3d spread = Project<Vector3d>(cloud: cluster, metric: VectorCloudMetric.Spread, context: context, key: key);
SinkhornReceipt transport = Probe.Expect(Probe.Expect(VectorIntent.Transport(source: cluster, target: shifted, regularization: 0.5, maxIterations: 64, massRelaxation: 1.0, key: key), "transport intent").Project<SinkhornReceipt>(context: context, key: key), "transport");

Probe.Require(Math.Abs(area - 2.0) <= 1.0e-6, $"area={area:R}");
Probe.Require(shape.Area.IsSome && shape.Perimeter.IsSome && shape.Centroid.DistanceTo(new Point3d(x: 1.0, y: 0.5, z: 0.0)) <= 1.0e-6, $"shape.centroid={shape.Centroid}");
Probe.Require(spread.IsValid && spread.X >= 0.0 && spread.Y >= 0.0 && spread.Z >= 0.0, $"spread={spread}");
Probe.Require(RhinoMath.IsValidDouble(x: transport.Distance) && transport.Iterations >= 1, $"transport={transport}");

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("ring.area", area.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
Evidence.Emit("ring.centroid", shape.Centroid);
Evidence.Emit("cluster.spread", spread);
Evidence.Emit("transport.distance", transport.Distance.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
