using System.Globalization;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Proximity {
    public sealed record OnCurve(Curve Geometry) : Proximity;

    public sealed record OnSurface : Proximity {
        private OnSurface(Surface geometry) => Geometry = geometry;

        public Surface Geometry { get; }

        public static Fin<OnSurface> Validated(Surface geometry) =>
            geometry is Extrusion { CapCount: > 0 and var caps }
                ? new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{geometry.ObjectType} with {caps} caps"), RhinoReason.Mismatched)
                : new OnSurface(geometry);
    }

    public sealed record OnFace(BrepFace Geometry) : Proximity;

    public sealed record OnBrep(Brep Geometry) : Proximity;

    public sealed record OnMesh(Mesh Geometry) : Proximity;

    public sealed record OnSubD(SubD Geometry) : Proximity;

    public sealed record OnCloud(PointCloud Geometry) : Proximity;

    public Seq<Option<Point3d>> Nearest(Seq<Point3d> points, Option<double> limit) =>
        Switch(
            limit switch {
                var held => (Points: points, Limit: held, Bound: held.IfNone(0d)),
            },
            onCurve: static (at, of) => at.Points.Map(point => at.Limit
                .Match(
                    Some: distance => Conversions.Found(of.Geometry.ClosestPoint(point, out double t, distance), t),
                    None: () => Conversions.Found(of.Geometry.ClosestPoint(point, out double t), t))
                .Map(of.Geometry.PointAt)).Strict(),
            onSurface: static (at, of) => at.Points.Map(point => Conversions.Found(of.Geometry.ClosestPoint(point, out double u, out double v), (U: u, V: v))
                .Map(uv => of.Geometry.PointAt(uv.U, uv.V))
                .Filter(found => at.Limit.ForAll(distance => point.DistanceTo(found) <= distance))).Strict(),
            onFace: static (at, of) => at.Points.Map(point => Conversions.Found(of.Geometry.ClosestPointOnFace(point, out double u, out double v, at.Bound), (U: u, V: v))
                .Map(uv => of.Geometry.PointAt(uv.U, uv.V))).Strict(),
            onBrep: static (at, of) => at.Points.Map(point => Conversions.Found(
                of.Geometry.ClosestPoint(point, out Point3d closest, out ComponentIndex _, out double _, out double _, at.Bound, out Vector3d _), closest)).Strict(),
            onMesh: static (at, of) => at.Points.Map(point => Optional(of.Geometry.ClosestMeshPoint(point, at.Bound)).Map(static found => found.Point)).Strict(),
            onSubD: static (at, of) => {
                _ = of.Geometry.ClosestPoints(at.Points, out Point3d[] closest, out SubDComponentParameter[] _, at.Bound);
                return toSeq(closest).Map(Conversions.Present).Strict();
            },
            onCloud: static (at, of) => at.Points.Map(point => Conversions.Present(of.Geometry.ClosestPoint(point))
                .Map(of.Geometry.PointAt)
                .Filter(found => at.Limit.ForAll(distance => point.DistanceTo(found) <= distance))).Strict());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Interior {
    public sealed record Solid : Interior {
        private Solid(Brep brep) => Brep = brep;

        public Brep Brep { get; }

        public static Fin<Solid> Validated(Brep brep) => RhinoFault.Unless(brep.IsSolid).Map(_ => new Solid(brep));

        public static IO<Solid> Validated(SubD subd, SubDToBrepOptions options) =>
            Products.Made(subd.ToBrep(options)).Bind(static brep => IO.lift(() => Validated(brep)));
    }

    public sealed record Closed : Interior {
        private Closed(Mesh mesh) => Mesh = mesh;

        public Mesh Mesh { get; }

        public static Fin<Closed> Validated(Mesh mesh) => RhinoFault.Unless(mesh.IsClosed && mesh.IsManifold()).Map(_ => new Closed(mesh));
    }

    public sealed record Planar : Interior {
        private Planar(Curve curve, Plane plane) => (Curve, Plane) = (curve, plane);

        public Curve Curve { get; }

        public Plane Plane { get; }

        public static Fin<Planar> Validated(Curve curve, Plane plane) =>
            (RhinoFault.Unless(curve.IsClosed), RhinoFault.Unless(plane.IsValid)).Apply((_, _) => new Planar(curve, plane)).As();
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
