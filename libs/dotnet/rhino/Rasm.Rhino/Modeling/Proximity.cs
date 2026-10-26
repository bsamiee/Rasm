using System.Globalization;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Proximity {
    public sealed record Curve(global::Rhino.Geometry.Curve Geometry) : Proximity;

    public sealed record Surface : Proximity {
        private Surface(global::Rhino.Geometry.Surface geometry) => Geometry = geometry;

        public global::Rhino.Geometry.Surface Geometry { get; }

        public static Fin<Surface> Validated(global::Rhino.Geometry.Surface geometry) =>
            geometry is Extrusion { CapCount: > 0 and var caps }
                ? Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{nameof(Extrusion)} with {caps} caps answers its wall alone, its {nameof(Extrusion.ToBrep)} reaches the caps"))
                : new Surface(geometry);
    }

    public sealed record Face(BrepFace Geometry) : Proximity;

    public sealed record Brep(global::Rhino.Geometry.Brep Geometry) : Proximity;

    public sealed record Mesh(global::Rhino.Geometry.Mesh Geometry) : Proximity;

    public sealed record SubD(global::Rhino.Geometry.SubD Geometry) : Proximity;

    public sealed record Cloud(PointCloud Geometry) : Proximity;

    public Seq<Option<Point3d>> Nearest(Seq<Point3d> points, Option<SearchDistance> limit) =>
        Switch(
            limit.Map(static distance => (double)distance) switch {
                var held => (Points: points, Limit: held, Bound: held.IfNone(0d)),
            },
            curve: static (at, of) => at.Points.Map(point => at.Limit
                .Match(
                    Some: distance => Callbacks.Found(of.Geometry.ClosestPoint(point, out double t, distance), t),
                    None: () => Callbacks.Found(of.Geometry.ClosestPoint(point, out double t), t))
                .Map(of.Geometry.PointAt)).Strict(),
            surface: static (at, of) => at.Points.Map(point => Callbacks.Found(of.Geometry.ClosestPoint(point, out double u, out double v), (U: u, V: v))
                .Map(uv => of.Geometry.PointAt(uv.U, uv.V))
                .Filter(found => at.Limit.ForAll(distance => point.DistanceTo(found) <= distance))).Strict(),
            face: static (at, of) => at.Points.Map(point => Callbacks.Found(of.Geometry.ClosestPointOnFace(point, out double u, out double v, at.Bound), (U: u, V: v))
                .Map(uv => of.Geometry.PointAt(uv.U, uv.V))).Strict(),
            brep: static (at, of) => at.Points.Map(point => Callbacks.Found(
                of.Geometry.ClosestPoint(point, out Point3d closest, out ComponentIndex _, out double _, out double _, at.Bound, out Vector3d _), closest)).Strict(),
            mesh: static (at, of) => at.Points.Map(point => Optional(of.Geometry.ClosestMeshPoint(point, at.Bound)).Map(static found => found.Point)).Strict(),
            subD: static (at, of) => {
                _ = of.Geometry.ClosestPoints(at.Points, out Point3d[] closest, out SubDComponentParameter[] _, at.Bound);
                return toSeq(closest).Map(Conversions.Present).Strict();
            },
            cloud: static (at, of) => at.Points.Map(point => Conversions.Present(of.Geometry.ClosestPoint(point))
                .Map(of.Geometry.PointAt)
                .Filter(found => at.Limit.ForAll(distance => point.DistanceTo(found) <= distance))).Strict());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Region {
    public sealed record Solid : Region {
        private Solid(Brep brep) => Brep = brep;

        public Brep Brep { get; }

        public static Fin<Solid> Validated(Brep brep) =>
            brep.IsSolid ? new Solid(brep) : Error.New(ErrorCodes.ValidationFailed, $"{nameof(Brep)} is no closed oriented manifold");

        public static IO<Solid> Validated(SubD subd, SubDToBrepOptions options, Action<IDisposable> retain) =>
            Copies.Acquire(nameof(SubD.ToBrep), _ => Answer.One(subd.ToBrep(options)), retain).Bind(static brep => IO.lift(() => Validated(brep)));
    }

    public sealed record Closed : Region {
        private Closed(Mesh mesh) => Mesh = mesh;

        public Mesh Mesh { get; }

        public static Fin<Closed> Validated(Mesh mesh) =>
            mesh.IsClosed && mesh.IsManifold() ? new Closed(mesh) : Error.New(ErrorCodes.ValidationFailed, $"{nameof(Mesh)} is not closed and manifold");
    }

    public sealed record Planar : Region {
        private Planar(Curve curve, Plane plane) => (Curve, Plane) = (curve, plane);

        public Curve Curve { get; }

        public Plane Plane { get; }

        public static Fin<Planar> Validated(Curve curve, Plane plane) =>
            (curve.IsClosed, plane.IsValid) switch {
                (true, true) => new Planar(curve, plane),
                (false, _) => Error.New(ErrorCodes.ValidationFailed, $"{nameof(Curve)} is open"),
                (_, false) => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{plane} is no valid plane")),
            };
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
