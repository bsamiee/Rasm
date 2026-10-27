using System.Runtime.Intrinsics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace Rasm.Spatial;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Decomposition {
    private Decomposition(Vector3D<double> translation, Matrix<double> linear) => (Translation, Linear) = (translation, linear);
    public Vector3D<double> Translation { get; }
    public Matrix<double> Linear { get; }
    public bool Reversing => Linear.Determinant() < 0d;
    public Matrix<double> Compose() => Transforms.Affine(Linear, Translation);

    public sealed record Rigid(Vector3D<double> Translation, Matrix<double> Rotation) : Decomposition(Translation, Rotation);
    public sealed record Similarity(Vector3D<double> Translation, double Dilation, Matrix<double> Rotation) : Decomposition(Translation, Rotation * Dilation);
    public sealed record Affine(Vector3D<double> Translation, Matrix<double> Rotation, Matrix<double> Stretch) : Decomposition(Translation, Rotation * Stretch);
    public sealed record Singular(Vector3D<double> Translation, Matrix<double> Linear) : Decomposition(Translation, Linear);

    public static Fin<Decomposition> Of(Matrix<double> map, double tolerance) =>
        from affine in SpatialFault.Unless(map is { RowCount: 4, ColumnCount: 4 } && map.Row(3).Equals(Vector<double>.Build.Dense([0d, 0d, 0d, 1d])), SpatialRule.Shape, (map.RowCount, map.ColumnCount))
        let translation = new Vector3D<double>(map[0, 3], map[1, 3], map[2, 3])
        let linear = map.SubMatrix(0, 3, 0, 3)
        let svd = linear.Svd(computeVectors: true)
        let polar = Polar(svd)
        select svd.S switch {
            var scales when scales[2] <= tolerance => (Decomposition)new Singular(translation, linear),
            var scales when (scales - 1d).InfinityNorm() <= tolerance => new Rigid(translation, polar.Rotation),
            var scales when scales[0] - scales[2] <= tolerance => new Similarity(translation, scales.Sum() / scales.Count, polar.Rotation),
            _ => new Affine(translation, polar.Rotation, polar.Stretch),
        };

    public Fin<Matrix<double>> Interpolate(Decomposition other, double at) =>
        from oriented in SpatialFault.Unless(this is not Singular && other is not Singular && Reversing == other.Reversing, SpatialRule.Singular, (Reversing, other.Reversing))
        let start = Polar(Linear.Svd(computeVectors: true))
        let end = Polar(other.Linear.Svd(computeVectors: true))
        let relative = start.Rotation.TransposeThisAndMultiply(end.Rotation)
        let axis = (relative - Matrix<double>.Build.DenseIdentity(3)).Svd(computeVectors: true).VT.Row(2)
        let skew = Vector<double>.Build.Dense([relative[2, 1] - relative[1, 2], relative[0, 2] - relative[2, 0], relative[1, 0] - relative[0, 1]])
        let turn = Transforms.Rotation(new(axis[0], axis[1], axis[2]), at * Math.Atan2(axis.DotProduct(skew) / 2d, (relative.Trace() - 1d) / 2d))
        select Transforms.Affine(start.Rotation * turn * (start.Stretch + ((end.Stretch - start.Stretch) * at)), Translation + ((other.Translation - Translation) * at));

    private static (Matrix<double> Rotation, Matrix<double> Stretch) Polar(Svd<double> svd) => (svd.U * svd.VT, svd.VT.TransposeThisAndMultiply(svd.W * svd.VT));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Transforms {
    // --- [CONSTRUCTION]
    public static Matrix<double> Affine(Matrix<double> linear, Vector3D<double> translation) =>
        linear.Append(Matrix<double>.Build.Dense(3, 1, [translation.X, translation.Y, translation.Z])).Stack(Matrix<double>.Build.Dense(1, 4, [0d, 0d, 0d, 1d]));

    public static Matrix<double> Rotation(Vector3D<double> axis, double angle) =>
        (Direction: Column(axis.Normalize()), Turn: Math.SinCos(angle)) switch {
            var (k, (sin, cos)) => (Matrix<double>.Build.DenseIdentity(3) * cos) + (Matrix<double>.Build.DenseOfRowArrays([0d, -k[2], k[1]], [k[2], 0d, -k[0]], [-k[1], k[0], 0d]) * sin) + (Vector<double>.OuterProduct(k, k) * (1d - cos)),
        };

    public static Matrix<double> Mirror(Vector3D<double> normal) => Column(normal.Normalize()) switch { var direction => Matrix<double>.Build.DenseIdentity(3) - (Vector<double>.OuterProduct(direction, direction) * 2d) };

    private static Vector<double> Column(Vector3D<double> direction) => Vector<double>.Build.Dense([direction.X, direction.Y, direction.Z]);

    // --- [APPLICATION]
    public static Func<Vector3D<double>, Vector3D<double>> Applied(Matrix<double> map) =>
        (X: Vector256.Create(map.Row(0).ToArray()), Y: Vector256.Create(map.Row(1).ToArray()), Z: Vector256.Create(map.Row(2).ToArray())) switch {
            var rows => point => Vector256.Create(point.X, point.Y, point.Z, 1d) switch { var lifted => new Vector3D<double>(Vector256.Dot(rows.X, lifted), Vector256.Dot(rows.Y, lifted), Vector256.Dot(rows.Z, lifted)) },
        };
}
