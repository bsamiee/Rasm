using System.Numerics;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade.Curves;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GradientMap {
    public static void Mapped(Span<Vector4> row, ReadOnlySpan<Vector4> table, (double R, double G, double B) luminance) {
        float top = table.Length - 1;
        foreach (ref Vector4 pixel in row) {
            float position = float.Clamp((float)double.Cbrt(LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, luminance)), 0f, 1f) * top;
            int low = int.Clamp((int)float.MaxNumber(position, 0f), 0, int.Max(table.Length - 2, 0));
            Vector4 mapped = Vector4.Lerp(table[low], table[int.Min(low + 1, table.Length - 1)], position - low);
            pixel = new Vector4(Vector3.Lerp(pixel.AsVector3(), mapped.AsVector3(), mapped.W), pixel.W);
        }
    }
}
