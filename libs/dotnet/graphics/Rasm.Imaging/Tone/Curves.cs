using System.Numerics;
using TinyEXR;
using UnitsNet;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToneCurve {
    public static Func<float, float> Sigmoid(float contrast, float skew, float black, Luminance peak) {
        const double gray = Metering.MiddleGray;
        double ceiling = peak.Nits.ToDouble() / DynamicRange.Standard.WhiteLuminance;
        double bend = double.Pow(5d, -skew);
        double power = contrast * (1d - gray) / (bend * (1d - double.Pow(gray / ceiling, 1d / bend)));
        double towardGray = double.Pow(ceiling / gray, 1d / bend) - 1d;
        double floor = double.Pow(black, 1d / bend);
        double lifted = double.Pow(towardGray * floor, 1d / power);
        double fog = gray * lifted / (double.Pow(1d - floor, 1d / power) - lifted);
        (float white, float paper, float offset, float exponent, float skewed) = ((float)ceiling, (float)(double.Pow(fog + gray, power) * towardGray), (float)fog, (float)power, (float)bend);
        return value => white * float.Pow(1f + (paper * float.Pow(offset + float.MaxNumber(value, 0f), -exponent)), -skewed);
    }

    public static Func<float, float> Piecewise(float toeStrength, float toeLength, float shoulderAngle, float gamma, float shoulderStrength, float shoulderLength) {
        double toeX = 0.5d * double.Pow(toeLength, 2.2d);
        double toeY = (1d - toeStrength) * toeX;
        double shoulderX = toeX + ((1d - shoulderLength) * (1d - toeY));
        double white = toeX + (1d - toeY) + double.Exp2(shoulderStrength) - 1d;
        double overshoot = shoulderAngle * shoulderStrength;
        return Segmented((toeX, toeY), (shoulderX, toeY + shoulderX - toeX), white * (1d + (2d * overshoot)), 1d + (0.5d * overshoot), gamma, white);
    }

    public static Func<float, float> Drago(float bias, float white, Luminance peak) {
        (float scale, float normalizer, float inverseWhite, float power) = (
            (float)(peak.Nits.ToDouble() / DynamicRange.Standard.WhiteLuminance), float.Log10P1(white), 1f / white, (float)(double.Log(bias) / double.Log(0.5d)));
        return value => float.MaxNumber(value, 0f) switch { var x => scale * float.LogP1(x) / (normalizer * float.Log(2f + (8f * float.Pow(x * inverseWhite, power)))) };
    }

    public static Func<float, float> Operator(ToneMapOperator operation, ToneMapParameters parameters) =>
        value => {
            float mapped = value;
            ImageProcessing.ToneMap(new ReadOnlySpan<float>(in value), new Span<float>(ref mapped), 1, operation, parameters);
            return mapped;
        };

    public static Action<Span<Vector4>> Graded(Func<Vector3, Vector3> curve, float preserveHue = 0f) =>
        row => {
            foreach (ref Vector4 pixel in row)
                pixel = new Vector4(preserveHue is 0f ? curve(pixel.AsVector3()) : PreserveHue(pixel.AsVector3(), curve(pixel.AsVector3()), preserveHue), pixel.W);
        };

    public static Vector3 PreserveHue(Vector3 input, Vector3 mapped, float preserve) {
        (int min, int mid, int max) = input.X >= input.Y
            ? input.Y > input.Z ? (2, 1, 0) : input.Z > input.X ? (1, 0, 2) : input.Z > input.Y ? (1, 2, 0) : (2, 1, 0)
            : input.X >= input.Z ? (2, 0, 1) : input.Z > input.Y ? (0, 1, 2) : (0, 2, 1);
        float chroma = input[max] - input[min];
        float midscale = chroma is not 0f ? (input[mid] - input[min]) / chroma : 0f;
        float naive = ((1f - preserve) * mapped[mid]) + (preserve * (mapped[min] + ((mapped[max] - mapped[min]) * midscale)));
        float blend = input[min] + input[mid] is not 0f ? 2f * input[min] / (input[min] + input[mid]) : 0f;
        float target = (blend * (mapped.X + mapped.Y + mapped.Z)) + ((1f - blend) * (mapped[min] + naive + mapped[max]));
        (float low, float middle, float high) = naive <= mapped[mid]
            ? ((((1f - preserve) * mapped[mid]) + (preserve * ((midscale * mapped[max]) + ((1f - midscale) * (target - mapped[max]))))) / (1f + (preserve * (1f - midscale)))) switch {
                var corrected => (target - mapped[max] - corrected, corrected, mapped[max]),
            }
            : ((((1f - preserve) * mapped[mid]) + (preserve * ((mapped[min] * (1f - midscale)) + (midscale * (target - mapped[min]))))) / (1f + (preserve * midscale))) switch {
                var corrected => (mapped[min], corrected, target - mapped[min] - corrected),
            };
        return new(Placed(0), Placed(1), Placed(2));

        float Placed(int channel) => channel == min ? low : channel == mid ? middle : high;
    }

    internal static Func<float, float> Segmented((double X, double Y) toe, (double X, double Y) shoulder, double end, double top, double gamma, double white) {
        double slope = (shoulder.Y - toe.Y) / (shoulder.X - toe.X);
        double ceiling = double.Pow(top, gamma);
        (double toeValue, double shoulderValue) = (double.Pow(toe.Y, gamma), double.Pow(shoulder.Y, gamma));
        (double toePower, double shoulderPower) = (gamma * slope * toe.X / toe.Y, gamma * slope * double.Pow(shoulder.Y, gamma - 1d) * (end - shoulder.X) / (ceiling - shoulderValue));
        double Raw(double x) =>
            x < toe.X ? toeValue * double.Pow(x / toe.X, toePower)
            : x < shoulder.X ? double.Pow(toe.Y + (slope * (x - toe.X)), gamma)
            : x < end ? ceiling - ((ceiling - shoulderValue) * double.Pow((end - x) / (end - shoulder.X), shoulderPower))
            : ceiling;
        double scale = 1d / Raw(white);
        return value => (float)(scale * Raw(float.MaxNumber(value, 0f)));
    }
}
