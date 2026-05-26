using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
VectorCloud tooFew = Probe.Expect(VectorCloud.Cluster(
    points: Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0)),
    context: context,
    key: key), "too few cluster");
VectorCloud coplanar = Probe.Expect(VectorCloud.Cluster(
    points: Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0)),
    context: context,
    key: key), "coplanar cluster");
VectorCloud cloud = Probe.Expect(VectorCloud.Cluster(
    points: Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.0, y: 0.0, z: 1.0),
        new Point3d(x: 0.25, y: 0.25, z: 0.25)),
    context: context,
    key: key), "cluster");
VectorCloud duplicateFootprint = Probe.Expect(VectorCloud.Cluster(
    points: Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 2.0, y: 0.0, z: 0.0),
        new Point3d(x: 2.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.5, z: 0.0)),
    context: context,
    key: key), "duplicate footprint cluster");
VectorCloud collinear = Probe.Expect(VectorCloud.Cluster(
    points: Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 2.0, y: 0.0, z: 0.0)),
    context: context,
    key: key), "collinear cluster");
CloudHullReceipt tooFewReceipt = Probe.Project<CloudHullReceipt>(intent: VectorIntent.Hull(source: tooFew, kind: CloudHullKind.Convex3D, key: key), context: context, key: key, label: "too few");
CloudHullReceipt coplanarReceipt = Probe.Project<CloudHullReceipt>(intent: VectorIntent.Hull(source: coplanar, kind: CloudHullKind.Convex3D, key: key), context: context, key: key, label: "coplanar");
CloudHullResult result = Probe.Project<CloudHullResult>(intent: VectorIntent.Hull(source: cloud, kind: CloudHullKind.Convex3D, key: key), context: context, key: key, label: "hull");
Mesh hull = Probe.Expect(result.Mesh.ToFin(Fail: key.InvalidResult()), "hull mesh");
CloudHullResult footprint = Probe.Project<CloudHullResult>(intent: VectorIntent.Hull(source: duplicateFootprint, kind: CloudHullKind.ConvexFootprint2D, key: key), context: context, key: key, label: "footprint");
CloudHullResult collinearFootprint = Probe.Project<CloudHullResult>(intent: VectorIntent.Hull(source: collinear, kind: CloudHullKind.ConvexFootprint2D, key: key), context: context, key: key, label: "collinear");
CloudHullReceipt alphaReceipt = Probe.Project<CloudHullReceipt>(intent: VectorIntent.Hull(source: cloud, kind: CloudHullKind.AlphaShape, key: key), context: context, key: key, label: "alpha");

Probe.Require(tooFewReceipt.Status.Equals(CloudHullStatus.Rejected) && tooFewReceipt.NativeRouted && tooFewReceipt.ContainmentRejectedCount == 3, $"tooFew={tooFewReceipt}");
Probe.Require(coplanarReceipt.Status.Equals(CloudHullStatus.Rejected) && coplanarReceipt.CoplanarRejected && coplanarReceipt.NativeRouted, $"coplanar={coplanarReceipt}");
Probe.Require(result.Receipt.Status.Equals(CloudHullStatus.Completed), $"status={result.Receipt.Status}");
Probe.Require(result.Receipt.NativeRouted && result.Receipt.NativeFacetCount > 0, $"facets={result.Receipt.NativeFacetCount}");
Probe.Require(result.Receipt.InputCount == 5 && result.Receipt.OutputVertexCount >= 4, $"receipt={result.Receipt}");
Probe.Require(hull.IsValid && hull.Faces.Count > 0, $"mesh.faces={hull.Faces.Count}");
Probe.Require(footprint.Receipt.Status.Equals(CloudHullStatus.Completed) && footprint.Receipt.NativeRouted && footprint.Receipt.InputCount == 6 && footprint.Receipt.OutputVertexCount == 4, $"footprint={footprint.Receipt}");
Probe.Require(collinearFootprint.Receipt.Status.Equals(CloudHullStatus.Rejected) && collinearFootprint.Mesh.IsNone && collinearFootprint.Receipt.NativeRouted && collinearFootprint.Receipt.ContainmentRejectedCount == 3, $"collinear={collinearFootprint.Receipt}");
Probe.Require(alphaReceipt.Status.Equals(CloudHullStatus.Unsupported) && !alphaReceipt.NativeRouted, $"alpha={alphaReceipt}");

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("hull.vertices", result.Receipt.OutputVertexCount);
Evidence.Emit("hull.facets", result.Receipt.NativeFacetCount);
Evidence.Emit("footprint.vertices", footprint.Receipt.OutputVertexCount);
Evidence.Emit("collinear.status", collinearFootprint.Receipt.Status);
