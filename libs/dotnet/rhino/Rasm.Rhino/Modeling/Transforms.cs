using System.Globalization;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Spatial;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Frames {
    public sealed record Through(Seq<Plane> Frames) : Frames;

    public sealed record Spaced : Frames {
        private Spaced(Point3d start, Point3d end, double interval) => (Start, End, Interval) = (start, end, interval);
        public Point3d Start { get; }
        public Point3d End { get; }
        public double Interval { get; }
        public static Fin<Spaced> Validated(Point3d start, Point3d end, double interval) =>
            !(end - start).IsTiny() && interval > 0d ? new Spaced(start, end, interval) : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"spaced frames need distinct ends and a positive interval, received {start} to {end} by {interval}"));
    }

    public sealed record Linear : Frames {
        private Linear(Plane start, Vector3d step, int count) => (Start, Step, Count) = (start, step, count);
        public Plane Start { get; }
        public Vector3d Step { get; }
        public int Count { get; }
        public static Fin<Linear> Validated(Plane start, Vector3d step, int count) => Counted(count).Map(_ => new Linear(start, step, count));
    }

    public sealed record Polar : Frames {
        private Polar(Plane start, Line axis, int count, double sweep) => (Start, Axis, Count, Sweep) = (start, axis, count, sweep);
        public Plane Start { get; }
        public Line Axis { get; }
        public int Count { get; }
        public double Sweep { get; }
        public static Fin<Polar> Validated(Plane start, Line axis, int count, double sweep) => Counted(count).Map(_ => new Polar(start, axis, count, sweep));
    }

    public sealed record Along(Curve Rail, CurveFrames Frames) : Frames;

    public sealed record OnSurface(Surface Surface, Seq<Point2d> At) : Frames;

    public Fin<Seq<Plane>> Planes =>
        Switch<Fin<Seq<Plane>>>(
            through: static through => through.Frames,
            spaced: static spaced => (spaced.End - spaced.Start) switch {
                var span => Stepped(new Plane(spaced.Start, span), span / span.Length * spaced.Interval, (int)Math.Floor(span.Length / spaced.Interval) + 1),
            },
            linear: static linear => Stepped(linear.Start, linear.Step, linear.Count),
            polar: static polar =>
                Callbacks.Each(
                    toSeq(Enumerable.Range(0, polar.Count)).Map(step => Transform.Rotation(step * polar.Sweep / polar.Count, polar.Axis.Direction, polar.Axis.From)).Strict(),
                    (rotation, index) => polar.Start switch {
                        var plane => Refused.Unless(plane.Transform(rotation), plane, nameof(Plane.Transform), index),
                    }),
            along: static along => CurveEvaluation.Frames(along.Rail, along.Frames),
            onSurface: static onSurface =>
                Callbacks.Each(onSurface.At, (uv, index) => Refused.Unless(onSurface.Surface.FrameAt(uv.X, uv.Y, out Plane frame), frame, nameof(Surface.FrameAt), index)));

    public Fin<Seq<Transform>> Placements(Plane basis) =>
        Planes.Map(planes => planes.Map(plane => Transform.PlaneToPlane(basis, plane)).Strict());

    private static Fin<Unit> Counted(int count) => count >= 0 ? unit : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"frames need a count at or above 0, received {count}"));

    private static Seq<Plane> Stepped(Plane start, Vector3d step, int count) =>
        toSeq(Enumerable.Range(0, count)).Map(at => start with { Origin = start.Origin + (step * at) }).Strict();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Transformations {
    // --- [TRANSFORMS]
    public static IO<T> Transformed<T>(T copy, Transform xform, Action<IDisposable> retain) where T : GeometryBase =>
        from subject in xform.SimilarityType is TransformSimilarityType.NotSimilarity ? Deformable(copy, None, retain) : IO.pure(copy)
        from outward in IO.lift(() => subject switch {
            Brep brep => brep.SolidOrientation is BrepSolidOrientation.Outward,
            Mesh mesh => mesh.SolidOrientation() > 0,
            _ => false,
        })
        from moved in IO.lift(() => Refused.Unless(subject.Transform(xform), subject, nameof(GeometryBase.Transform)))
        from flipped in IO.lift(() => Flipped(moved, outward, xform))
        select moved;

    private static Fin<Unit> Flipped(GeometryBase moved, bool outward, Transform xform) {
        switch (moved) {
            case Brep brep when outward && brep.SolidOrientation is BrepSolidOrientation.Inward:
                brep.Flip();
                return unit;
            case Mesh mesh when outward && mesh.SolidOrientation() < 0:
                mesh.Flip(vertexNormals: true, faceNormals: true, faceOrientation: true);
                return unit;
            case SubD subdivision when subdivision.IsSolid && xform.Determinant < 0d:
                return Refused.Unless(subdivision.Flip(), nameof(SubD.Flip));
            default:
                return unit;
        }
    }

    // --- [DEFORMABLE]
    internal static IO<T> Deformable<T>(T copy, Option<int> index, Action<IDisposable> retain) where T : GeometryBase =>
        IO.lift(copy.MakeDeformable).Bind(deformable => (Deformable: deformable, Copy: (GeometryBase)copy) switch {
            (Deformable: true, _) => IO.pure(copy),
            (_, Copy: Extrusion extrusion) => Successor<T>(Copies.Acquire(nameof(Extrusion.ToBrep), _ => Answer.One<GeometryBase>(extrusion.ToBrep(splitKinkyFaces: false)), retain), index),
            (_, Copy: Surface surface) => Successor<T>(Copies.Acquire(nameof(Surface.ToNurbsSurface), _ => Answer.One<GeometryBase>(surface.ToNurbsSurface()), retain), index),
            (_, Copy: Curve curve) => Successor<T>(Copies.Acquire(nameof(Curve.ToNurbsCurve), _ => Answer.One<GeometryBase>(curve.ToNurbsCurve()), retain), index),
            _ => IO.fail<T>(index.Match(
                Some: static at => new Refused(nameof(GeometryBase.MakeDeformable), Refused.Declined, at),
                None: static () => new Refused(nameof(GeometryBase.MakeDeformable), Refused.Declined))),
        });

    internal static IO<T> Successor<T>(IO<GeometryBase> successor, Option<int> index) where T : GeometryBase =>
        successor.Bind(made => IO.lift(() => made is T typed
            ? Fin.Succ(typed)
            : Error.New(ErrorCodes.ValidationFailed,
                $"{nameof(GeometryBase.MakeDeformable)} successor {made.GetType().Name} is no {typeof(T).Name}{index.Map(static at => " at index " + at.ToString(CultureInfo.InvariantCulture)).IfNone("")}")));

    // --- [CONVERSIONS]
    extension(Point3d point) {
        public Vector3D<double> ToVector3D() => new(point.X, point.Y, point.Z);
    }
    extension(Vector3d vector) {
        public Vector3D<double> ToVector3D() => new(vector.X, vector.Y, vector.Z);
    }
    extension(Vector3D<double> point) {
        public Point3d ToPoint3d() => new(point.X, point.Y, point.Z);
        public Point3d ToPoint3d(Plane frame) => frame.PointAt(point.X, point.Y, point.Z);
    }
    extension(Interval interval) {
        public Fin<ParameterSpan> ToSpan() => ParameterSpan.Validated(interval.T0, interval.T1);
    }
    extension(Transform xform) {
        public Matrix<double> ToMatrix() => Matrix<double>.Build.Dense(4, 4, xform.ToDoubleArray(rowDominant: false));
    }
    extension(Matrix<double> map) {
        public Transform ToTransform() => new() {
            M00 = map[0, 0], M01 = map[0, 1], M02 = map[0, 2], M03 = map[0, 3], M10 = map[1, 0], M11 = map[1, 1], M12 = map[1, 2], M13 = map[1, 3],
            M20 = map[2, 0], M21 = map[2, 1], M22 = map[2, 2], M23 = map[2, 3], M30 = map[3, 0], M31 = map[3, 1], M32 = map[3, 2], M33 = map[3, 3],
        };
    }
    extension(BoundingBox box) {
        public Bounds ToBounds() => new(box.Min.ToVector3D(), box.Max.ToVector3D());
    }
    extension(Bounds bounds) {
        public BoundingBox ToBoundingBox() => new(bounds.Min.ToPoint3d(), bounds.Max.ToPoint3d());
    }
}
