using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Closest(Point3d Point, Option<ComponentIndex> Component, Seq<double> Parameters, Option<Vector3d> Normal);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Interior {
    public sealed record Solid : Interior {
        private Solid(Brep brep) => Brep = brep;

        public Brep Brep { get; }

        public static Fin<Solid> Validated(Brep brep) => RhinoFault.Unless(brep.IsSolid, RhinoReason.Invalid, brep.IsSolid).Map(_ => new Solid(brep));

        public static IO<Solid> Validated(SubD subd, SubDToBrepOptions options) =>
            Products.Of(() => subd.ToBrep(options)).Bind(static brep => IO.lift(Validated(brep)));
    }

    public sealed record Closed : Interior {
        private Closed(Mesh mesh) => Mesh = mesh;

        public Mesh Mesh { get; }

        public static Fin<Closed> Validated(Mesh mesh) =>
            (mesh.IsClosed, mesh.IsManifold()) switch {
                var held => RhinoFault.Unless(held is (true, true), RhinoReason.Invalid, held).Map(_ => new Closed(mesh)),
            };
    }

    public sealed record Planar : Interior {
        private Planar(Curve curve, Plane plane) => (Curve, Plane) = (curve, plane);

        public Curve Curve { get; }

        public Plane Plane { get; }

        public static Fin<Planar> Validated(Curve curve, Plane plane) =>
            (RhinoFault.Unless(curve.IsClosed, RhinoReason.Invalid, curve.IsClosed), RhinoFault.Unless(plane.IsValid, RhinoReason.Invalid, plane))
                .Apply((_, _) => new Planar(curve, plane)).As();
    }

    public PointContainment Contains(Tolerances tolerances, Point3d point) =>
        Switch(
            (Point: point, Tolerance: tolerances.Absolute),
            solid: static (at, solid) => Classified(strictlyIn => solid.Brep.IsPointInside(at.Point, at.Tolerance, strictlyIn)),
            closed: static (at, closed) => Classified(strictlyIn => closed.Mesh.IsPointInside(at.Point, at.Tolerance, strictlyIn)),
            planar: static (at, planar) => planar.Curve.Contains(at.Point, planar.Plane, at.Tolerance));

    private static PointContainment Classified(Func<bool, bool> inside) =>
        inside(true) ? PointContainment.Inside
        : inside(false) ? PointContainment.Coincident
        : PointContainment.Outside;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Proximity {
    // --- [NEAREST]
    public static Fin<Option<Closest>> Nearest(GeometryBase subject, Point3d query, Option<double> limit) =>
        subject switch {
            Curve curve => Found(curve.ClosestPoint(query, out double t, limit.IfNone(0d)), limit, () => new Closest(
                curve.PointAt(t), curve is PolyCurve polycurve ? Some(new ComponentIndex(ComponentIndexType.PolycurveSegment, polycurve.SegmentIndex(t))) : None, [t], Some(curve.TangentAt(t)))),
            BrepFace face => Found(face.ClosestPointOnFace(query, out double u, out double v, limit.IfNone(0d)), limit, () => new Closest(
                face.PointAt(u, v), Some(new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex)), [u, v], Some(face.NormalAt(u, v)))),
            Extrusion { CapCount: > 0 } extrusion => Capped(extrusion, query, limit),
            Surface surface => Found(surface.ClosestPoint(query, out double u, out double v), None, () => new Closest(
                surface.PointAt(u, v), None, [u, v], Some(surface.NormalAt(u, v)))).Map(found => found.Filter(at => Inside(query, at, limit))),
            Brep brep => Found(brep.ClosestPoint(query, out Point3d point, out ComponentIndex component, out double s, out double t, limit.IfNone(0d), out Vector3d normal), limit, () =>
                new Closest(point, Some(component), component.ComponentIndexType is ComponentIndexType.BrepEdge ? [s] : [s, t], Some(normal))),
            Mesh mesh => Found(Optional(mesh.ClosestMeshPoint(query, limit.IfNone(0d))), limit).Map(found => found.Map(at => new Closest(
                at.Point, Some(at.ComponentIndex), at.ComponentIndex.ComponentIndexType switch {
                    ComponentIndexType.MeshFace => toSeq(at.T),
                    ComponentIndexType.MeshTopologyEdge => [at.EdgeParameter],
                    _ => Seq<double>(),
                }, Some(mesh.NormalAt(at))))),
            SubD subd => Found(subd.ClosestPoint(query, out Point3d point, out SubDComponentParameter at, limit.IfNone(0d)), limit, () => new Closest(
                point, Some(at.ComponentIndex), at.IsFaceParameter ? [at.FaceCornerIndex, at.FaceCornerParameters.X, at.FaceCornerParameters.Y] : at.IsEdgeParameter ? [at.EdgeParameter] : [], None)),
            PointCloud cloud => Found(Conversions.Present(cloud.ClosestPoint(query)), None).Map(found => found.Map(index => new Closest(
                cloud[index].Location, Some(new ComponentIndex(ComponentIndexType.PointCloudPoint, index)), [], cloud.ContainsNormals ? Some(cloud[index].Normal) : None)).Filter(at => Inside(query, at, limit))),
            _ => new RhinoFault(subject.ObjectType.ToString(), RhinoReason.Mismatched),
        };

    public static Fin<Option<(Point3d OnCurve, Point3d OnObject, T Geometry)>> Nearest<T>(Curve curve, Seq<T> candidates, Option<double> limit) where T : GeometryBase =>
        Found(curve.ClosestPoints(candidates, out Point3d onCurve, out Point3d onObject, out int index, limit.IfNone(0d)), limit, () => (OnCurve: onCurve, OnObject: onObject, Geometry: candidates[index]));

    private static Fin<Option<Closest>> Capped(Extrusion extrusion, Point3d query, Option<double> limit) {
        using Brep? brep = extrusion.ToBrep(splitKinkyFaces: false);
        return Conversions.Required(brep).Bind(solid => Nearest(solid, query, limit));
    }

    private static Fin<Option<T>> Found<T>(bool found, Option<double> limit, Func<T> answer) => Found(found ? Some(answer()) : None, limit);

    private static Fin<Option<T>> Found<T>(Option<T> found, Option<double> limit) =>
        found.IsSome || limit.IsSome ? found : new RhinoFault(nameof(Nearest), RhinoReason.Missing);

    private static bool Inside(Point3d query, Closest at, Option<double> limit) => limit.ForAll(distance => query.DistanceTo(at.Point) <= distance);
}
