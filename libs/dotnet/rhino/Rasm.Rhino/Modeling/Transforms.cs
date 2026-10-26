using System.Globalization;
using Rasm.Rhino.Modeling.Curves;
using Rhino;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Decomposition {
    public sealed record Rigid(Vector3d Translation, Transform Rotation) : Decomposition;
    public sealed record Similarity(Vector3d Translation, double Dilation, Transform Rotation) : Decomposition;
    public sealed record Affine(Vector3d Translation, Transform Rotation, Transform Orthogonal, Vector3d Diagonal) : Decomposition;
    public sealed record Singular(Vector3d Translation, Transform Linear) : Decomposition;

    private readonly record struct Polar(Vector3d Translation, Transform Rotation, double Orientation, Transform Stretch);

    public bool Reversing =>
        Switch(
            rigid: static rigid => rigid.Rotation.Determinant < 0d,
            similarity: static similarity => similarity.Dilation < 0d,
            affine: static affine => affine.Diagonal.X * affine.Diagonal.Y * affine.Diagonal.Z < 0d,
            singular: static singular => singular.Linear.Determinant < 0d);

    public static Fin<Decomposition> Of(Transform xform, double tolerance = RhinoMath.ZeroTolerance) =>
        !xform.IsAffine
            ? Error.New(ErrorCodes.ValidationFailed, $"{nameof(Transform.IsAffine)} is false for {xform}")
            : xform.DecomposeRigid(out Vector3d rigidTranslation, out Transform rigidRotation, tolerance) is not TransformRigidType.NotRigid
                ? new Rigid(rigidTranslation, rigidRotation)
                : xform.DecomposeSimilarity(out Vector3d similarTranslation, out double dilation, out Transform similarRotation, tolerance) is not TransformSimilarityType.NotSimilarity
                    ? new Similarity(similarTranslation, dilation, similarRotation)
                    : xform.DecomposeAffine(out Vector3d affineTranslation, out Transform affineRotation, out Transform orthogonal, out Vector3d diagonal)
                        ? new Affine(affineTranslation, affineRotation, orthogonal, diagonal)
                        : Refused.Unless<Decomposition>(xform.DecomposeAffine(out Vector3d translation, out Transform linear), new Singular(translation, linear), nameof(Transform.DecomposeAffine));

    public Transform Compose() =>
        Switch(
            rigid: static rigid => Transform.Translation(rigid.Translation) * rigid.Rotation,
            similarity: static similarity => Transform.Translation(similarity.Translation) * Transform.Diagonal(similarity.Dilation, similarity.Dilation, similarity.Dilation) * similarity.Rotation,
            affine: static affine => Transform.Translation(affine.Translation) * affine.Rotation * affine.Orthogonal * Transform.Diagonal(affine.Diagonal) * affine.Orthogonal.Transpose(),
            singular: static singular => Transform.Translation(singular.Translation) * singular.Linear);

    public Fin<Transform> Interpolate(Decomposition other, double t) =>
        from at in Conversions.Within(new Interval(0d, 1d), t, nameof(Quaternion.Slerp))
        from start in Polarized()
        from end in other.Polarized()
        from oriented in guard(start.Orientation == end.Orientation, Error.New(ErrorCodes.ValidationFailed, $"{nameof(Interpolate)} ends of opposite orientation cross a singular transform"))
        from startTurn in Turn(start.Rotation)
        from endTurn in Turn(end.Rotation)
        from turn in Turned(Quaternion.Slerp(startTurn, Quaternion.Distance(startTurn, endTurn) <= Quaternion.Distance(startTurn, endTurn * -1d) ? endTurn : endTurn * -1d, at))
        select Transform.Translation(start.Translation + (at * (end.Translation - start.Translation)))
            * turn
            * Transform.Diagonal(start.Orientation, start.Orientation, start.Orientation)
            * Blend(start.Stretch, end.Stretch, at);

    private Fin<Polar> Polarized() =>
        Switch<Fin<Polar>>(
            rigid: static rigid => double.CopySign(1d, rigid.Rotation.Determinant) switch {
                var sign => new Polar(rigid.Translation, Transform.Diagonal(sign, sign, sign) * rigid.Rotation, sign, Transform.Identity),
            },
            similarity: static similarity => Math.Abs(similarity.Dilation) switch {
                var scale => new Polar(similarity.Translation, similarity.Rotation, double.CopySign(1d, similarity.Dilation), Transform.Diagonal(scale, scale, scale)),
            },
            affine: static affine => new Vector3d(double.CopySign(1d, affine.Diagonal.X), double.CopySign(1d, affine.Diagonal.Y), double.CopySign(1d, affine.Diagonal.Z)) switch {
                var signs => (signs.X * signs.Y * signs.Z) switch {
                    var sign => new Polar(
                        affine.Translation,
                        Transform.Diagonal(sign, sign, sign) * affine.Rotation * affine.Orthogonal * Transform.Diagonal(signs) * affine.Orthogonal.Transpose(),
                        sign,
                        affine.Orthogonal * Transform.Diagonal(Math.Abs(affine.Diagonal.X), Math.Abs(affine.Diagonal.Y), Math.Abs(affine.Diagonal.Z)) * affine.Orthogonal.Transpose()),
                },
            },
            singular: static _ => Error.New(ErrorCodes.ValidationFailed, $"{nameof(Singular)} {nameof(Decomposition)} has no polar form to {nameof(Interpolate)}"));

    private static Fin<Quaternion> Turn(Transform rotation) =>
        Refused.Unless(rotation.GetQuaternion(out Quaternion quaternion), quaternion, nameof(Transform.GetQuaternion));

    private static Fin<Transform> Turned(Quaternion quaternion) =>
        Refused.Unless(quaternion.GetRotation(out Transform rotation), rotation, nameof(Quaternion.GetRotation));

    private static Transform Blend(Transform start, Transform end, double at) =>
        toSeq(Enumerable.Range(0, 3)).Bind(static row => toSeq(Enumerable.Range(0, 3)).Map(column => (Row: row, Column: column)))
            .Fold(Transform.Identity, (blended, entry) => {
                blended[entry.Row, entry.Column] = start[entry.Row, entry.Column] + (at * (end[entry.Row, entry.Column] - start[entry.Row, entry.Column]));
                return blended;
            });
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Frames {
    public sealed record Through(Seq<Plane> Frames) : Frames;

    public sealed record Spaced(Point3d Start, Point3d End, double Interval) : Frames;

    public sealed record Linear(Plane Start, Vector3d Step, int Count) : Frames;

    public sealed record Polar(Plane Start, Line Axis, int Count, double Sweep) : Frames;

    public sealed record Along(Curve Rail, CurveFrames Frames) : Frames;

    public sealed record OnSurface(Surface Surface, Seq<Point2d> At) : Frames;

    public Fin<Seq<Plane>> Planes =>
        Switch(
            through: static through => through.Frames,
            spaced: static spaced => (spaced.End - spaced.Start) switch {
                var span when span.IsTiny() || !(double.IsFinite(spaced.Interval) && spaced.Interval > 0d) =>
                    Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{nameof(Spaced)} needs distinct ends and a positive interval, received {spaced.Start} to {spaced.End} by {spaced.Interval}")),
                var span => Stepped(new Plane(spaced.Start, span), span / span.Length * spaced.Interval, (int)Math.Floor(span.Length / spaced.Interval) + 1),
            },
            linear: static linear => Counted(linear.Count, nameof(Linear)).Map(count => Stepped(linear.Start, linear.Step, count)),
            polar: static polar =>
                from count in Counted(polar.Count, nameof(Polar))
                from axis in polar.Axis.IsValid && double.IsFinite(polar.Sweep)
                    ? Fin.Succ(polar.Axis)
                    : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{nameof(Polar)} needs a valid axis and a finite sweep, received {polar.Axis} and {polar.Sweep}"))
                select toSeq(Enumerable.Range(0, count)).Map(step => Moved(polar.Start, Transform.Rotation(step * polar.Sweep / count, axis.Direction, axis.From))).Strict(),
            along: static along => CurveEvaluation.Frames(along.Rail, along.Frames),
            onSurface: static onSurface =>
                Callbacks.Each(onSurface.At, (uv, index) => Refused.Unless(onSurface.Surface.FrameAt(uv.X, uv.Y, out Plane frame), frame, nameof(Surface.FrameAt), index)));

    public Fin<Seq<Transform>> Placements(Plane basis) =>
        Planes.Map(planes => planes.Map(plane => Transform.PlaneToPlane(basis, plane)).Strict());

    private static Fin<int> Counted(int count, string kind) =>
        count >= 0 ? count : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{kind} needs a count at or above 0, received {count}"));

    private static Seq<Plane> Stepped(Plane start, Vector3d step, int count) =>
        toSeq(Enumerable.Range(0, count)).Map(at => Moved(start, Transform.Translation(step * at))).Strict();

    private static Plane Moved(Plane plane, Transform xform) =>
        new(xform * plane.Origin, xform * plane.XAxis, xform * plane.YAxis);
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
}
