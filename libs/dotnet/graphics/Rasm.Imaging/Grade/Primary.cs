using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Xml;
using System.Xml.Linq;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Wacton.Unicolour;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WheelOffset(float Hue, float Strength, float Luma) {
    public static WheelOffset Neutral { get; } = new(0f, 0f, 0f);

    public Vector3 Multiplier(Gamut gamut) => Tint(gamut, 0.5f * Strength * new Vector2(float.Cos(Hue), float.Sin(Hue))) * float.Exp2(Luma);

    public static Vector3 Tint(Gamut gamut, Vector2 chroma) =>
        new Unicolour(Configuration.Default, ColourSpace.Oklab, 1d, chroma.X, chroma.Y).ConvertToConfiguration(gamut.Configuration).RgbLinear switch { var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B) };

    public static Fin<WheelOffset> FromMultiplier(Vector3 multiplier, Gamut gamut, float hue) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, multiplier.X, multiplier.Y, multiplier.Z).ConvertToConfiguration(Configuration.Default).Oklab switch {
            { L: <= 0d } => new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"wheel multiplier lightness {multiplier}"), ImagingReason.Range),
            var lab => new Vector2((float)(lab.A / lab.L), (float)(lab.B / lab.L)) switch {
                var chroma => new WheelOffset(
                    chroma.Length() < 1e-6f ? hue : (float.Atan2(chroma.Y, chroma.X) + float.Tau) % float.Tau, 2f * chroma.Length(), 3f * float.Log2((float)lab.L)),
            },
        };

    public static (Vector3 Low, Vector3 High) ChannelBounds(Gamut gamut) =>
        toSeq(Range(0, 360))
            .Map(static step => float.DegreesToRadians(step))
            .Map(angle => Tint(gamut, 0.5f * new Vector2(float.Cos(angle), float.Sin(angle))))
            .Fold((Low: new Vector3(float.MaxValue), High: new Vector3(float.MinValue)), static (bounds, tint) => (Vector3.Min(bounds.Low, tint), Vector3.Max(bounds.High, tint)));
}

public sealed record ColorWheels(WheelOffset Global, WheelOffset Shadows, WheelOffset Midtones, WheelOffset Highlights, float ShadowsEnd, float HighlightsStart, float HighlightsWidth) {
    public Action<Span<Vector4>> Graded(Gamut working) {
        (Vector256<double> weights, Vector3 all, Vector3 low, Vector3 middle, Vector3 high) =
            (working.Luminance, Global.Multiplier(working), Shadows.Multiplier(working), Midtones.Multiplier(working), Highlights.Multiplier(working));
        return row => {
            foreach (ref Vector4 pixel in row) {
                double luma = Vector256.Dot(weights, pixel.Wide);
                (float lifted, float raised) = ((float)Smooth(0d, ShadowsEnd, luma), (float)Smooth(HighlightsStart, HighlightsStart + HighlightsWidth, luma));
                pixel = new Vector4(pixel.AsVector3() * all * ((low * (1f - lifted)) + (middle * lifted * (1f - raised)) + (high * lifted * raised)), pixel.W);
            }
        };

        static double Smooth(double low, double high, double y) =>
            ShapeEdge.Smooth(double.Clamp((y - low) / (high - low), 0d, 1d));
    }
}

[SmartEnum<string>(KeyMemberName = "Extension")]
public sealed partial class CdlFormat {
    public static readonly CdlFormat Correction = new(".cc", []);
    public static readonly CdlFormat Collection = new(".ccc", ["ColorCorrectionCollection"]);
    public static readonly CdlFormat DecisionList = new(".cdl", ["ColorDecisionList", "ColorDecision"]);

    private const string Declaration = """<?xml version="1.0" encoding="UTF-8"?>""";
    private const string CorrectionNode = "ColorCorrection";
    private const string IdAttribute = "id";
    private static readonly XNamespace Asc = "urn:ASC:CDL:v1.01";

    public Seq<string> Ancestors { get; }

    public static Fin<IterableNE<(Option<string> Id, LutTable.Cdl Correction)>> Read(string text) =>
        Try.lift(() => XDocument.Parse(text)).Run()
            .MapFail(static error => error.HasException<XmlException>() ? new ImagingFault("CDL text", ImagingReason.Malformed, Inner: error) : error)
            .Bind(static document => toSeq(document.Descendants()).Filter(static element => element.Name.LocalName is CorrectionNode)
                .Traverse(static correction => ProcessLists.CorrectionValues(correction, static name => new ImagingFault($"CDL correction {name}", ImagingReason.Unreadable))
                    .Map(values => (Optional(correction.Attribute(IdAttribute)?.Value), new LutTable.Cdl(CdlStyle.FwdNoClamp, values.Slope, values.Offset, values.Power, values.Saturation))))
                .As())
            .Bind(static corrections => corrections.AsIterableNE().ToFin(new ImagingFault($"{CorrectionNode} element in CDL text", ImagingReason.Empty)));

    public string Write(LutTable.Cdl correction, Option<string> id) =>
        $"{Declaration}\n{Ancestors.FoldBack(
            new XElement(Asc + CorrectionNode, id.ToSeq().Map(static value => new XAttribute(IdAttribute, value)), ProcessLists.CorrectionNodes(Asc, correction)),
            static (inner, ancestor) => new XElement(Asc + ancestor, inner))}\n";
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Primary {
    public static Action<Span<Vector4>> Contrast(float contrast, float pivot, float vibrance, float saturation, Gamut gamut) {
        Vector256<double> luminance = gamut.Luminance;
        return row => {
            foreach (ref Vector4 pixel in row) {
                (double red, double green, double blue) = (Curved(pixel.X), Curved(pixel.Y), Curved(pixel.Z));
                double high = double.Max(red, double.Max(green, blue));
                double spread = high > 0d ? 1d - (double.Max(double.Min(red, double.Min(green, blue)), 0d) / high) : 1d;
                double gain = saturation * (1d + (vibrance * (vibrance > 0f ? (1d - spread) * (1d - spread) : spread * spread / 2d)));
                double gray = Vector256.Dot(luminance, Vector256.Create(red, green, blue, 0d));
                pixel = new Vector4((float)double.Lerp(gray, red, gain), (float)double.Lerp(gray, green, gain), (float)double.Lerp(gray, blue, gain), pixel.W);
            }
        };

        double Curved(float value) => double.CopySign(pivot * double.Pow(double.Abs(value) / pivot, contrast), value);
    }

    public static LutTable.Affine Levels(float black, float stops) => double.Exp2(-stops) switch { var gain => new(Matrix<double>.Build.DenseDiagonal(3, gain), (-black * gain, -black * gain, -black * gain)) };

    public static LutTable.Affine WhiteBalance(Gamut working, Wacton.Unicolour.Temperature white) =>
        new(working.Adapting(Adaptation.White(white), working.Configuration.Xyz.WhitePoint));

    public static Fin<Wacton.Unicolour.Temperature> PickedWhite(PixelFrame frame, Option<Rectangle> region, Gamut working) {
        FrameWindow bounds = region.Match(
            Some: area => new FrameWindow(long.Max(area.Left, frame.Window.Left), long.Max(area.Top, frame.Window.Top), long.Min((long)area.X + area.Width, frame.Window.Right), long.Min((long)area.Y + area.Height, frame.Window.Bottom)),
            None: () => frame.Window);
        int width = (int)long.Max(bounds.Right - bounds.Left, 0L);
        Vector256<double> sum = Vector256<double>.Zero;
        for (long line = bounds.Top; width > 0 && line < bounds.Bottom; line++) {
            foreach (Vector4 pixel in frame.View.Span.GetRowSpan((int)(line - frame.Origin.Y)).Slice((int)(bounds.Left - frame.Origin.X), width))
                sum += pixel.W > 0f && Vector3.AllWhereAllBitsSet(Vector3.IsFinite(pixel.AsVector3())) ? Vector256.Create(pixel.X, pixel.Y, pixel.Z, 1d) : Vector256<double>.Zero;
        }
        return sum[3] is 0d
            ? new ImagingFault($"white balance region {bounds}", ImagingReason.Empty)
            : Adaptation.Correlate(working, (sum / sum[3]).Narrowed.AsVector3());
    }

    public static Action<Span<Vector4>> Mapped(ReadOnlyMemory<Vector4> gradient, Gamut gamut) {
        (float top, Vector256<double> luminance) = (gradient.Length - 1, gamut.Luminance);
        return row => {
            ReadOnlySpan<Vector4> table = gradient.Span;
            foreach (ref Vector4 pixel in row) {
                float position = float.Clamp((float)double.Cbrt(Vector256.Dot(luminance, pixel.Wide)), 0f, 1f) * top;
                int low = int.Clamp((int)float.MaxNumber(position, 0f), 0, int.Max(table.Length - 2, 0));
                Vector4 mapped = Vector4.Lerp(table[low], table[int.Min(low + 1, table.Length - 1)], position - low);
                pixel = new Vector4(Vector3.Lerp(pixel.AsVector3(), mapped.AsVector3(), mapped.W), pixel.W);
            }
        };
    }
}
