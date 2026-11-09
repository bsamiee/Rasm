using Rasm.Geometry;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Decomposition {
    public Transform Composed => Switch(
        rigid: static part => Transform.Translation(part.Translation) * part.Rotation,
        similarity: static part => Transform.Translation(part.Translation) * Transform.Diagonal(part.Dilation, part.Dilation, part.Dilation) * part.Rotation,
        affine: static part => Transform.Translation(part.Translation) * part.Rotation * part.Orthogonal * Transform.Diagonal(part.Diagonal) * part.Orthogonal.Transpose(),
        singular: static part => Transform.Translation(part.Translation) * part.Linear);

    public Option<(Vector3d Translation, Transform Rotation, double Orientation, Transform Stretch)> Polar => Switch(
        rigid: static part => double.CopySign(1d, part.Rotation.Determinant) switch {
            var sign => Some((part.Translation, Transform.Diagonal(sign, sign, sign) * part.Rotation, sign, Transform.Identity)),
        },
        similarity: static part => Math.Abs(part.Dilation) switch {
            var scale => Some((part.Translation, part.Rotation, double.CopySign(1d, part.Dilation), Transform.Diagonal(scale, scale, scale))),
        },
        affine: static part => new Vector3d(double.CopySign(1d, part.Diagonal.X), double.CopySign(1d, part.Diagonal.Y), double.CopySign(1d, part.Diagonal.Z)) switch {
            var signs => (signs.X * signs.Y * signs.Z) switch {
                var sign => Some((
                    part.Translation,
                    Transform.Diagonal(sign, sign, sign) * part.Rotation * part.Orthogonal * Transform.Diagonal(signs) * part.Orthogonal.Transpose(),
                    sign,
                    part.Orthogonal * Transform.Diagonal(Math.Abs(part.Diagonal.X), Math.Abs(part.Diagonal.Y), Math.Abs(part.Diagonal.Z)) * part.Orthogonal.Transpose())),
            },
        },
        singular: static _ => Option<(Vector3d, Transform, double, Transform)>.None);

    public sealed record Rigid(Vector3d Translation, Transform Rotation) : Decomposition;

    public sealed record Similarity(Vector3d Translation, double Dilation, Transform Rotation) : Decomposition;

    public sealed record Affine(Vector3d Translation, Transform Rotation, Transform Orthogonal, Vector3d Diagonal) : Decomposition;

    public sealed record Singular(Vector3d Translation, Transform Linear) : Decomposition;

    public static Fin<Decomposition> Of(Transform xform, double tolerance) =>
        !xform.IsAffine ? new RhinoFault(xform, RhinoReason.Unsupported)
        : xform.DecomposeRigid(out Vector3d rigidTranslation, out Transform rigidRotation, tolerance) is not TransformRigidType.NotRigid ? new Rigid(rigidTranslation, rigidRotation)
        : xform.DecomposeSimilarity(out Vector3d similarTranslation, out double dilation, out Transform similarRotation, tolerance) is not TransformSimilarityType.NotSimilarity
            ? new Similarity(similarTranslation, dilation, similarRotation)
        : xform.DecomposeAffine(out Vector3d affineTranslation, out Transform affineRotation, out Transform orthogonal, out Vector3d diagonal)
            ? new Affine(affineTranslation, affineRotation, orthogonal, diagonal)
        : RhinoFault.Unless(xform.DecomposeAffine(out Vector3d translation, out Transform linear), RhinoReason.Declined, xform).Map(Decomposition (_) => new Singular(translation, linear));

    public Fin<Transform> Interpolate(Decomposition other, double t) =>
        from ends in (Conversions.Within(new Interval(0d, 1d), t), Polar.ToFin(new RhinoFault(this, RhinoReason.Unsupported)), other.Polar.ToFin(new RhinoFault(other, RhinoReason.Unsupported))).Apply(static (_, start, end) => (Start: start, End: end)).As()
        let start = ends.Start
        let end = ends.End
        from oriented in RhinoFault.Unless(start.Orientation == end.Orientation, RhinoReason.Mismatched, (start.Orientation, end.Orientation))
        from turns in (Turn(start.Rotation), Turn(end.Rotation)).Apply(static (first, last) => (First: first, Last: last)).As()
        let startTurn = turns.First
        let endTurn = turns.Last
        let nearest = (startTurn.Scalar * endTurn.Scalar) + (startTurn.Vector * endTurn.Vector) >= 0d ? endTurn : endTurn * -1d
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
        from reverse in IO.lift(() => xform.Determinant < 0d && (subject switch {
            Brep brep => brep.SolidOrientation is BrepSolidOrientation.Outward,
            Mesh mesh => mesh.SolidOrientation() > 0,
            SubD subdivision => subdivision.IsSolid,
            _ => false,
        }))
        from moved in Products.Accepted(() => subject.Transform(xform))
        from flipped in subject switch {
            Brep brep when reverse => IO.lift(brep.Flip),
            Mesh mesh when reverse => IO.lift(() => mesh.Flip(vertexNormals: true, faceNormals: true, faceOrientation: true)),
            SubD subdivision when reverse => Products.Accepted(subdivision.Flip),
            _ => IO.pure(unit),
        }
        select subject;

    // --- [DEFORMABLE]

    internal static IO<T> Deformable<T>(T copy, Option<int> index) where T : GeometryBase =>
        IO.lift(copy.MakeDeformable).Bind(deformable => deformable ? IO.pure(copy) : (GeometryBase)copy switch {
            Extrusion extrusion => Successor<T>(Products.Of<GeometryBase>(() => extrusion.ToBrep(splitKinkyFaces: false))),
            Surface surface => Successor<T>(Products.Of<GeometryBase>(surface.ToNurbsSurface)),
            Curve curve => Successor<T>(Products.Of<GeometryBase>(curve.ToNurbsCurve)),
            _ => IO.fail<T>(new RhinoFault(copy.ObjectType, RhinoReason.Unsupported, index)),
        });

    internal static IO<T> Successor<T>(IO<GeometryBase> successor) where T : GeometryBase =>
        successor.Bind(static made => IO.lift(Conversions.Cast<T>(made)));

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
        public Fin<AffineMap> ToAffine() =>
            RhinoFault.Unless(xform.IsAffine, RhinoReason.Unsupported, xform)
                .Bind(_ => AffineMap.Validated(new(xform.M00, xform.M01, xform.M02), new(xform.M10, xform.M11, xform.M12), new(xform.M20, xform.M21, xform.M22), new(xform.M03, xform.M13, xform.M23)));
    }
    extension(BoundingBox box) {
        public Fin<Bounds> ToBounds() => Bounds.Validated(box.Min.ToVector3D(), box.Max.ToVector3D());
    }
    extension(Bounds bounds) {
        public BoundingBox ToBoundingBox() => new(bounds.Min.ToPoint3d(), bounds.Max.ToPoint3d());
    }
}
