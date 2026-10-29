using System.Runtime.Intrinsics;
using MathNet.Numerics.LinearAlgebra;

namespace Rasm.Spatial;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Transforms {
    // --- [APPLICATION]
    public static (Vector256<double> X, Vector256<double> Y, Vector256<double> Z) Lanes(Matrix<double> map) =>
        (Vector256.Create(map.Row(0).ToArray()), Vector256.Create(map.Row(1).ToArray()), Vector256.Create(map.Row(2).ToArray()));

    extension((Vector256<double> X, Vector256<double> Y, Vector256<double> Z) rows) {
        public Vector3D<double> Apply(Vector3D<double> point) =>
            Vector256.Create(point.X, point.Y, point.Z, 1d) switch { var lifted => new Vector3D<double>(Vector256.Dot(rows.X, lifted), Vector256.Dot(rows.Y, lifted), Vector256.Dot(rows.Z, lifted)) };
    }
}
