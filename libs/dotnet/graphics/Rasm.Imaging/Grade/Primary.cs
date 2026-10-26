using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Primary {
    public static void Contrast(Span<Vector4> row, float contrast, float pivot, float vibrance, float saturation, (double R, double G, double B) luminance) {
        foreach (ref Vector4 pixel in row) {
            (double red, double green, double blue) = (Curved(pixel.X), Curved(pixel.Y), Curved(pixel.Z));
            double high = double.Max(red, double.Max(green, blue));
            double spread = high > 0d ? 1d - (double.Max(double.Min(red, double.Min(green, blue)), 0d) / high) : 1d;
            double gain = saturation * (1d + (vibrance * (vibrance > 0f ? (1d - spread) * (1d - spread) : spread * spread / 2d)));
            double gray = LuminanceSamples.Luminance(red, green, blue, luminance);
            pixel = new Vector4((float)double.Lerp(gray, red, gain), (float)double.Lerp(gray, green, gain), (float)double.Lerp(gray, blue, gain), pixel.W);
        }

        double Curved(float value) => double.CopySign(pivot * double.Pow(double.Abs(value) / pivot, contrast), value);
    }

    public static LutTable.Affine Levels(float black, float stops) =>
        double.Exp2(-stops) switch {
            var gain => new(Matrix<double>.Build.DenseDiagonal(3, gain), (-black * gain, -black * gain, -black * gain)),
        };

    public static LutTable.Affine WhiteBalance(Gamut working, Wacton.Unicolour.Temperature white) =>
        new(working.Adapting(Adaptation.White(white), working.Configuration.Xyz.WhitePoint));

    public static Fin<Wacton.Unicolour.Temperature> PickedWhite(PixelFrame frame, Option<Rectangle> region, Gamut working) {
        (long Left, long Top, long Right, long Bottom) bounds = region.Match(
            Some: area => (long.Max(area.Left, frame.Window.Left), long.Max(area.Top, frame.Window.Top), long.Min((long)area.X + area.Width, frame.Window.Right), long.Min((long)area.Y + area.Height, frame.Window.Bottom)),
            None: () => frame.Window);
        int width = (int)long.Max(bounds.Right - bounds.Left, 0L);
        Vector256<double> sum = Vector256<double>.Zero;
        for (long line = bounds.Top; width > 0 && line < bounds.Bottom; line++) {
            foreach (Vector4 pixel in frame.View.Span.GetRowSpan((int)(line - frame.Origin.Y)).Slice((int)(bounds.Left - frame.Origin.X), width))
                sum += pixel.W > 0f && Vector3.AllWhereAllBitsSet(Vector3.IsFinite(pixel.AsVector3())) ? Vector256.Create(pixel.X, pixel.Y, pixel.Z, 1d) : Vector256<double>.Zero;
        }
        return sum[3] is 0d
            ? Error.New($"white balance region {bounds} holds no covered finite pixel")
            : Adaptation.Correlate(working, Vector256.Narrow(sum / sum[3], Vector256<double>.Zero).GetLower().AsVector3());
    }
}
