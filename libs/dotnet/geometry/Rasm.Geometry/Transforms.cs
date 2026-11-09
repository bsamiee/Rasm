using System.Diagnostics.CodeAnalysis;
using MathNet.Numerics.LinearAlgebra;

namespace Rasm.Geometry;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record AffineMap {
    private AffineMap(Vector3D<double> x, Vector3D<double> y, Vector3D<double> z, Vector3D<double> translation) => (X, Y, Z, Translation) = (x, y, z, translation);
    public Vector3D<double> X { get; }
    public Vector3D<double> Y { get; }
    public Vector3D<double> Z { get; }
    public Vector3D<double> Translation { get; }

    public AffineMap Inverse() => Of(Linear.Inverse(), new Vector3D<double>(0d, 0d, 0d)) switch { var inverse => new AffineMap(inverse.X, inverse.Y, inverse.Z, -inverse.Apply(Translation)) };

    private Matrix<double> Linear => Matrix<double>.Build.DenseOfRowArrays([X.X, X.Y, X.Z], [Y.X, Y.Y, Y.Z], [Z.X, Z.Y, Z.Z]);

    [SuppressMessage("Performance", "MA0168:Use readonly struct for in or ref readonly parameter", Justification = "HyperJet declares readonly Dot with an in parameter of mutable Vector3D<T>, preserving row arithmetic.")]
    public Vector3D<double> Apply(Vector3D<double> point) => new(X.Dot(point) + Translation.X, Y.Dot(point) + Translation.Y, Z.Dot(point) + Translation.Z);

    public Fin<AffineMap> Then(AffineMap next) => Of(next.Linear * Linear, next.Apply(Translation)) switch { var composed => Validated(composed.X, composed.Y, composed.Z, composed.Translation) };

    public Option<double> UniformScale(double tolerance) =>
        Some(Linear).Filter(linear => linear.ConditionNumber() - 1d <= tolerance).Map(static linear => double.Cbrt(double.Abs(linear.Determinant())));

    public static Fin<AffineMap> Validated(Vector3D<double> x, Vector3D<double> y, Vector3D<double> z, Vector3D<double> translation) =>
        new AffineMap(x, y, z, translation) switch {
            var map => from finite in GeometryFault.Unless(x.IsFinite && y.IsFinite && z.IsFinite && translation.IsFinite, GeometryRule.Range, map)
                       from regular in GeometryFault.Unless(map.Linear.Determinant() is not 0d && map.Inverse() is { X.IsFinite: true, Y.IsFinite: true, Z.IsFinite: true, Translation.IsFinite: true }, GeometryRule.Singular, map)
                       select map,
        };

    private static AffineMap Of(Matrix<double> linear, Vector3D<double> translation) =>
        new(new(linear[0, 0], linear[0, 1], linear[0, 2]), new(linear[1, 0], linear[1, 1], linear[1, 2]), new(linear[2, 0], linear[2, 1], linear[2, 2]), translation);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Vectors {
    extension(Vector3D<double> vector) {
        internal bool IsFinite => double.IsFinite(vector.X) && double.IsFinite(vector.Y) && double.IsFinite(vector.Z);
    }
}
