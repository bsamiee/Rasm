using System.Diagnostics.CodeAnalysis;
using MathNet.Numerics.LinearAlgebra;

namespace Rasm.Geometry;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Transforms {
    public static (Vector3D<double> X, Vector3D<double> Y, Vector3D<double> Z, Vector3D<double> Translation) Rows(Matrix<double> map) =>
        (new(map[0, 0], map[0, 1], map[0, 2]), new(map[1, 0], map[1, 1], map[1, 2]), new(map[2, 0], map[2, 1], map[2, 2]), new(map[0, 3], map[1, 3], map[2, 3]));

    extension((Vector3D<double> X, Vector3D<double> Y, Vector3D<double> Z, Vector3D<double> Translation) rows) {
        [SuppressMessage("Performance", "MA0168:Use readonly struct for in or ref readonly parameter", Justification = "HyperJet declares readonly Dot with an in parameter of mutable Vector3D<T>, preserving row arithmetic.")]
        public Vector3D<double> Apply(Vector3D<double> point) =>
            new(rows.X.Dot(point) + rows.Translation.X, rows.Y.Dot(point) + rows.Translation.Y, rows.Z.Dot(point) + rows.Translation.Z);
    }
}
