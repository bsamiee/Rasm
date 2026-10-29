using System.Numerics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HueRange(Vector3 Center, Vector3 Width, Vector3 Softness) {
    public static HueRange Open { get; } = new(Vector3.Zero, new Vector3(float.PositiveInfinity), Vector3.Zero);

    public float Membership(GradingStyle style, Vector3 hsy) {
        Vector3 offset = new Vector3(hsy.X, hsy.Y, style.Encode(new Vector3(hsy.Z)).X) - Center;
        Vector3 outside = Vector3.Abs(offset - new Vector3(float.Round(offset.X, MidpointRounding.ToEven), 0f, 0f)) - (0.5f * Width);
        return Inside(outside.X, Softness.X) * Inside(outside.Y, Softness.Y) * Inside(outside.Z, Softness.Z);
    }

    public Action<Span<Vector4>> Graded(GradingStyle style, float shift, float saturation, float stops) =>
        float.Exp2(stops) switch {
            var gain => ToneCurve.Graded(color => style.ToHsy(color) switch {
                var hsy => Vector3.Lerp(color, style.FromHsy(new Vector3(hsy.X + shift, hsy.Y * saturation, hsy.Z)) * gain, Membership(style, hsy)),
            }),
        };

    public Action<Span<Vector4>> Isolated(GradingStyle style, Gamut gamut) =>
        gamut.Luminance switch {
            var luminance => ToneCurve.Graded(color => Vector3.Lerp(new Vector3(Vector3.Dot(luminance, color)), color, Membership(style, style.ToHsy(color)))),
        };

    private static float Inside(float outside, float softness) => outside <= 0f ? 1f : 1f - DistanceField.SmoothStep(0f, softness, outside);
}
