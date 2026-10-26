using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;

namespace Rasm.Imaging.Tone;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Exposure {
    public const float MiddleGray = 0.18f;

    public static LutTable.Affine Shifted(float stops) => new(Matrix<double>.Build.DenseDiagonal(3, double.Exp2(stops)));
}
