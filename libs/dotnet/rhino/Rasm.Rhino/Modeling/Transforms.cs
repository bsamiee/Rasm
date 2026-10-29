using MathNet.Numerics.LinearAlgebra;
using Rasm.Spatial;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Decomposition {
    private Decomposition(Transform composed, Option<(Vector3d Translation, Transform Rotation, double Orientation, Transform Stretch)> polar) => (Composed, Polar) = (composed, polar);

    public Transform Composed { get; }

    public Option<(Vector3d Translation, Transform Rotation, double Orientation, Transform Stretch)> Polar { get; }

    public sealed record Rigid(Vector3d Translation, Transform Rotation) : Decomposition(
        Transform.Translation(Translation) * Rotation,
        double.CopySign(1d, Rotation.Determinant) switch {
            var sign => Some((Translation, Transform.Diagonal(sign, sign, sign) * Rotation, sign, Transform.Identity)),
        });

    public sealed record Similarity(Vector3d Translation, double Dilation, Transform Rotation) : Decomposition(
        Transform.Translation(Translation) * Transform.Diagonal(Dilation, Dilation, Dilation) * Rotation,
        Math.Abs(Dilation) switch {
            var scale => Some((Translation, Rotation, double.CopySign(1d, Dilation), Transform.Diagonal(scale, scale, scale))),
        });

    public sealed record Affine(Vector3d Translation, Transform Rotation, Transform Orthogonal, Vector3d Diagonal) : Decomposition(
        Transform.Translation(Translation) * Rotation * Orthogonal * Transform.Diagonal(Diagonal) * Orthogonal.Transpose(),
        new Vector3d(double.CopySign(1d, Diagonal.X), double.CopySign(1d, Diagonal.Y), double.CopySign(1d, Diagonal.Z)) switch {
            var signs => (signs.X * signs.Y * signs.Z) switch {
                var sign => Some((
                    Translation,
                    Transform.Diagonal(sign, sign, sign) * Rotation * Orthogonal * Transform.Diagonal(signs) * Orthogonal.Transpose(),
                    sign,
                    Orthogonal * Transform.Diagonal(Math.Abs(Diagonal.X), Math.Abs(Diagonal.Y), Math.Abs(Diagonal.Z)) * Orthogonal.Transpose())),
            },
        });

    public sealed record Singular(Vector3d Translation, Transform Linear) : Decomposition(Transform.Translation(Translation) * Linear, None);

    public static Fin<Decomposition> Of(Transform xform, double tolerance) =>
        !xform.IsAffine ? new RhinoFault(xform.ToString(), RhinoReason.Unsupported)
        : xform.DecomposeRigid(out Vector3d rigidTranslation, out Transform rigidRotation, tolerance) is not TransformRigidType.NotRigid ? new Rigid(rigidTranslation, rigidRotation)
        : xform.DecomposeSimilarity(out Vector3d similarTranslation, out double dilation, out Transform similarRotation, tolerance) is not TransformSimilarityType.NotSimilarity
            ? new Similarity(similarTranslation, dilation, similarRotation)
        : xform.DecomposeAffine(out Vector3d affineTranslation, out Transform affineRotation, out Transform orthogonal, out Vector3d diagonal)
            ? new Affine(affineTranslation, affineRotation, orthogonal, diagonal)
        : RhinoFault.Unless(xform.DecomposeAffine(out Vector3d translation, out Transform linear), RhinoReason.Declined, xform).Map(Decomposition (_) => new Singular(translation, linear));

    public Fin<Transform> Interpolate(Decomposition other, double t) =>
        from ends in (Conversions.Within(new Interval(0d, 1d), t), Polar.ToFin(new RhinoFault(GetType().Name, RhinoReason.Unsupported)), other.Polar.ToFin(new RhinoFault(other.GetType().Name, RhinoReason.Unsupported))).Apply(static (_, start, end) => (Start: start, End: end)).As()
        let start = ends.Start
        let end = ends.End
        from oriented in RhinoFault.Unless(start.Orientation == end.Orientation, RhinoReason.Mismatched, (start.Orientation, end.Orientation))
        from turns in (Turn(start.Rotation), Turn(end.Rotation)).Apply(static (first, last) => (First: first, Last: last)).As()
        let startTurn = turns.First
        let endTurn = turns.Last
        let nearest = Quaternion.Distance(startTurn, endTurn) <= Quaternion.Distance(startTurn, endTurn * -1d) ? endTurn : endTurn * -1d
        from turn in Quaternion.Slerp(startTurn, nearest, t) switch {
            var slerped => RhinoFault.Unless(slerped.GetRotation(out Transform rotation), RhinoReason.Declined, slerped).Map(_ => rotation),
        }
        select Transform.Translation(start.Translation + (t * (end.Translation - start.Translation)))
            * turn
            * Transform.Diagonal(start.Orientation, start.Orientation, start.Orientation)
            * Blend(start.Stretch, end.Stretch, t);

    private static Fin<Quaternion> Turn(Transform rotation) =>
        RhinoFault.Unless(rotation.GetQuaternion(out Quaternion quaternion), RhinoReason.Declined, rotation).Map(_ => quaternion);

    private static Transform Blend(Transform start, Transform end, double t) =>
        new() {
            M00 = double.Lerp(start.M00, end.M00, t), M01 = double.Lerp(start.M01, end.M01, t), M02 = double.Lerp(start.M02, end.M02, t),
            M10 = double.Lerp(start.M10, end.M10, t), M11 = double.Lerp(start.M11, end.M11, t), M12 = double.Lerp(start.M12, end.M12, t),
            M20 = double.Lerp(start.M20, end.M20, t), M21 = double.Lerp(start.M21, end.M21, t), M22 = double.Lerp(start.M22, end.M22, t),
            M33 = 1d,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Transformations {
    // --- [TRANSFORMS]
    public static IO<T> Transformed<T>(T copy, Transform xform) where T : GeometryBase =>
        from subject in xform.SimilarityType is TransformSimilarityType.NotSimilarity ? Deformable(copy, None) : IO.pure(copy)
        from outward in IO.lift(() => subject switch {
            Brep brep => brep.SolidOrientation is BrepSolidOrientation.Outward,
            Mesh mesh => mesh.SolidOrientation() > 0,
            _ => false,
        })
        from moved in Products.Accepted(() => subject.Transform(xform))
        from flipped in subject switch {
            Brep brep when outward && brep.SolidOrientation is BrepSolidOrientation.Inward => IO.lift(brep.Flip),
            Mesh mesh when outward && mesh.SolidOrientation() < 0 => IO.lift(() => mesh.Flip(vertexNormals: true, faceNormals: true, faceOrientation: true)),
            SubD subdivision when subdivision.IsSolid && xform.Determinant < 0d => Products.Accepted(subdivision.Flip),
            _ => IO.pure(unit),
        }
        select subject;

    // --- [DEFORMABLE]
    internal static IO<T> Deformable<T>(T copy, Option<int> index) where T : GeometryBase =>
        IO.lift(copy.MakeDeformable).Bind(deformable => deformable ? IO.pure(copy) : (GeometryBase)copy switch {
            Extrusion extrusion => Successor<T>(Products.Of<GeometryBase>(() => extrusion.ToBrep(splitKinkyFaces: false))),
            Surface surface => Successor<T>(Products.Of<GeometryBase>(surface.ToNurbsSurface)),
            Curve curve => Successor<T>(Products.Of<GeometryBase>(curve.ToNurbsCurve)),
            _ => IO.fail<T>(new RhinoFault(copy.ObjectType.ToString(), RhinoReason.Unsupported, index)),
        });

    internal static IO<T> Successor<T>(IO<GeometryBase> successor) where T : GeometryBase =>
        successor.Bind(static made => IO.lift(() => Conversions.Cast<T>(made)));

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
    extension(BoundingBox box) {
        public Bounds ToBounds() => new(box.Min.ToVector3D(), box.Max.ToVector3D());
    }
    extension(Bounds bounds) {
        public BoundingBox ToBoundingBox() => new(bounds.Min.ToPoint3d(), bounds.Max.ToPoint3d());
    }
}
