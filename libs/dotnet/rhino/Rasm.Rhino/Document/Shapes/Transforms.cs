using Rhino;

namespace Rasm.Rhino.Document.Shapes;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record Decomposition {
    public sealed record Rigid(Vector3d Translation, Transform Rotation) : Decomposition;
    public sealed record Similarity(Vector3d Translation, double Dilation, Transform Rotation) : Decomposition;
    public sealed record Affine(Vector3d Translation, Transform Rotation, Transform Orthogonal, Vector3d Diagonal) : Decomposition;

    public static Option<Decomposition> Of(Transform xform, double scaleTolerance = RhinoMath.ZeroTolerance) =>
        xform.DecomposeRigid(out Vector3d rigidTranslation, out Transform rigidRotation, scaleTolerance) is not TransformRigidType.NotRigid
            ? new Rigid(rigidTranslation, rigidRotation)
            : xform.DecomposeSimilarity(out Vector3d translation, out double dilation, out Transform rotation, scaleTolerance) is not TransformSimilarityType.NotSimilarity
                ? new Similarity(translation, dilation, rotation)
                : xform.DecomposeAffine(out Vector3d shift, out Transform turn, out Transform orthogonal, out Vector3d diagonal)
                    ? new Affine(shift, turn, orthogonal, diagonal)
                    : None;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Transformations {
    public static IO<T> Transformed<T>(T copy, Transform xform) where T : GeometryBase =>
        IO.lift(() => Refused.Unless(copy.IsDeformable || xform.SimilarityType is not TransformSimilarityType.NotSimilarity || copy.MakeDeformable(), nameof(GeometryBase.MakeDeformable))
            .Bind(_ => Refused.Unless(copy.Transform(xform), copy, nameof(GeometryBase.Transform))));
}
