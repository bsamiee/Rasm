using System.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using UnitsNet;

namespace Rasm.Imaging.Tone;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Peak {
    public const float Bt2390 = 0.5f;

    public static Fin<Luminance> Measured(PixelFrame frame, Luminance white) =>
        LuminanceSamples.Greatest(frame).Map(samples => white * samples.FrameWhite);

    public static Option<Action<Span<Vector4>>> Rolled(Transfer.Pq signal, Luminance source, Luminance target, float black, float knee) {
        Luminance fitted = target < source ? target : source;
        Vector4 levels = new(new Vector3((float)(source / signal.White).ToDouble(), (float)(fitted / signal.White).ToDouble(), black * (float)(fitted / signal.White).ToDouble()), 1f);
        signal.Encode(new Span<Vector4>(ref levels));
        (float ceiling, float peak, float floor) = (levels.X, levels.Y / levels.X, levels.Z / levels.X);
        (float start, float power) = (((1f + knee) * peak) - knee, float.Min(1f / floor, 4f));
        float gain = 1f / (1f + (floor / peak * float.Pow(1f - peak, power)));
        CubicSpline rolloff = CubicSpline.InterpolateHermiteSorted([0d, 1d], [start, peak], [1d - start, 0d]);
        return source > target || black is not 0f
            ? Some<Action<Span<Vector4>>>(row => {
                signal.Encode(row);
                foreach (ref Vector4 pixel in row)
                    pixel = new Vector4(Mapped(pixel.X), Mapped(pixel.Y), Mapped(pixel.Z), pixel.W);
                signal.Decode(row);
            })
            : None;

        float Mapped(float code) =>
            float.Clamp(code / ceiling, 0f, 1f) switch {
                var x when x > start => (float)rolloff.Interpolate((x - start) / (1d - start)),
                var x => x,
            } switch {
                var x => float.Clamp((gain * (x + (floor * float.Pow(1f - x, power)) - floor)) + floor, floor, peak) * ceiling,
            };
    }
}
