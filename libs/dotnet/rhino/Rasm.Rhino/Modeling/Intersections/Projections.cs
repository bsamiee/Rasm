using Rasm.Rhino.Document.Units;
using Rhino.Geometry.Intersect;

namespace Rasm.Rhino.Modeling.Intersections;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ProjectionTargets {
    public sealed record OnMeshes(Seq<Mesh> Meshes, Vector3d Direction) : ProjectionTargets;

    public sealed record OnBreps(Seq<Brep> Breps, Vector3d Direction) : ProjectionTargets;

    public sealed record OnPlane(Plane Plane, Option<Vector3d> Along) : ProjectionTargets {
        public Fin<Transform> Projection =>
            Along.Match(Some: along => Transform.ProjectAlong(Plane, along), None: () => Transform.PlanarProjection(Plane)) switch {
                var xform => RhinoFault.Unless(!xform.IsIdentity, RhinoReason.Invalid, Along).Map(_ => xform),
            };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Projections {
    // --- [POINTS]
    public static IO<Seq<(Point3d Point, int Index)>> Points(Tolerances tolerances, ProjectionTargets targets, Seq<Point3d> points) =>
        targets.Switch(
            (Points: points, tolerances.Absolute),
            onMeshes: static (at, target) => MeshIntersections.Points(() => (Intersection.ProjectPointsToMeshesEx(target.Meshes, at.Points, target.Direction, at.Absolute, out int[] sources), sources)),
            onBreps: static (at, target) => MeshIntersections.Points(() => (Intersection.ProjectPointsToBrepsEx(target.Breps, at.Points, target.Direction, at.Absolute, out int[] sources), sources)),
            onPlane: static (at, target) => IO.lift(() => target.Projection.Map(xform => at.Points.Map((point, index) => (Point: xform * point, Index: index)).Strict())));

    // --- [CURVES]
    public static IO<Seq<(Curve Curve, Option<int> Source, Option<int> Target)>> Curves(Tolerances tolerances, ProjectionTargets targets, Seq<Curve> curves, bool loose) =>
        targets.Switch(
            (Curves: curves, tolerances.Absolute, Loose: loose),
            onMeshes: static (at, target) => Products.Many(() => Curve.ProjectToMesh(at.Curves, target.Meshes, target.Direction, at.Absolute, at.Loose))
                .Map(static projected => projected.Map(static curve => (Curve: curve, Source: Option<int>.None, Target: Option<int>.None)).Strict()),
            onBreps: static (at, target) =>
                from answered in IO.lift(() => (Curves: Curve.ProjectToBrep(at.Curves, target.Breps, target.Direction, at.Absolute, at.Loose, out int[]? sources, out int[]? targets), Sources: sources, Targets: targets))
                from projected in Products.Many(() => answered.Curves)
                select projected.Zip(Conversions.Rows(answered.Sources).Zip(Conversions.Rows(answered.Targets)), static (curve, row) => (Curve: curve, Source: Some(row.First), Target: Some(row.Second))).Strict(),
            onPlane: static (at, target) =>
                from xform in IO.lift(() => target.Projection)
                from projected in at.Curves.Map(static (curve, index) => (Curve: curve, Index: index)).TraverseM(row =>
                    from copy in Products.Of(row.Curve.DuplicateCurve)
                    from moved in Transformations.Transformed(copy, xform)
                    select (Curve: moved, Source: Some(row.Index), Target: Option<int>.None)).As()
                select projected);
}
