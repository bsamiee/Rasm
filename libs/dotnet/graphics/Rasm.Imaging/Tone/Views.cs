using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using SuperLinq;
using TinyEXR;
using UnitsNet;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DisplayDevice(ColorEncoding Encoding, Luminance Peak, Gamut LimitingGamut) {
    public const float Bt2390KneeOffset = 0.5f;

    private static readonly Luminance Standard = Luminance.FromNits(DynamicRange.Standard.MaxLuminance);
    private static readonly Luminance High = Luminance.FromNits(DynamicRange.High.MaxLuminance);

    public static DisplayDevice Srgb { get; } = new(new(Gamut.StandardRgb, Transfer.Gamma.Srgb), Standard, Gamut.StandardRgb);
    public static DisplayDevice DisplayP3 { get; } = new(new(Gamut.DisplayP3, Transfer.Gamma.Srgb), Standard, Gamut.DisplayP3);
    public static DisplayDevice Rec1886 { get; } = new(new(Gamut.StandardRgb, Transfer.Gamma.Rec1886), Standard, Gamut.StandardRgb);
    public static DisplayDevice Rec2020 { get; } = new(new(Gamut.Rec2020, Transfer.Gamma.Rec1886), Standard, Gamut.DisplayP3);
    public static DisplayDevice Rec2100PqHdr { get; } = new(new(Gamut.Rec2020, Transfer.Pq.High), High, Gamut.Rec2020);
    public static DisplayDevice Rec2100PqSdr { get; } = new(new(Gamut.Rec2020, Transfer.Pq.High), Standard, Gamut.StandardRgb);
    public static DisplayDevice Rec2100HlgHdr { get; } = new(new(Gamut.Rec2020, Transfer.Hlg.High), High, Gamut.Rec2020);
    public static DisplayDevice Rec2100HlgSdr { get; } = new(new(Gamut.Rec2020, Transfer.Hlg.High), Standard, Gamut.StandardRgb);

    public static Seq<DisplayDevice> Known { get; } = [Srgb, DisplayP3, Rec1886, Rec2020, Rec2100PqHdr, Rec2100PqSdr, Rec2100HlgHdr, Rec2100HlgSdr];

    public Option<Action<Span<Vector4>>> Eetf(Luminance source, float black = 0f, float kneeOffset = Bt2390KneeOffset) =>
        Encoding.Transfer is Transfer.Pq signal && (source > Peak || black is not 0f) ? Some(Rolled(signal, source, black, kneeOffset)) : None;

    private Action<Span<Vector4>> Rolled(Transfer.Pq signal, Luminance source, float black, float kneeOffset) {
        float fitted = (float)((Peak < source ? Peak : source) / signal.White).ToDouble();
        Vector4 levels = new((float)(source / signal.White).ToDouble(), fitted, black * fitted, 1f);
        signal.Encode.Apply(new Span<Vector4>(ref levels));
        (float ceiling, float peak, float floor) = (levels.X, levels.Y / levels.X, levels.Z / levels.X);
        (float start, float power) = (((1f + kneeOffset) * peak) - kneeOffset, float.Min(1f / floor, 4f));
        float gain = 1f / (1f + (floor / peak * float.Pow(1f - peak, power)));
        CubicSpline rolloff = CubicSpline.InterpolateHermiteSorted([0d, 1d], [start, peak], [1d - start, 0d]);
        (LutTable encode, LutTable decode) = (signal.Encode, signal.Decode);
        return row => {
            encode.Apply(row);
            foreach (ref Vector4 pixel in row)
                pixel = new Vector4(Mapped(pixel.X), Mapped(pixel.Y), Mapped(pixel.Z), pixel.W);
            decode.Apply(row);
        };

        float Mapped(float code) =>
            float.Clamp(code / ceiling, 0f, 1f) switch {
                var x when x > start => (float)rolloff.Interpolate((x - start) / (1d - start)),
                var x => x,
            } switch {
                var x => float.Clamp((gain * (x + (floor * float.Pow(1f - x, power)) - floor)) + floor, floor, peak) * ceiling,
            };
    }
}

public sealed partial record ViewTransform(ColorEncoding Scene, Seq<Action<Span<Vector4>>> Steps, Option<ColorEncoding> Signal) {
    private static readonly Vector3 AgXWeights = new(0.2589235355689848f, 0.6104985346066525f, 0.13057792982436284f);
    private static readonly ColorEncoding FalseColorSignal = new(Gamut.StandardRgb, Transfer.Gamma.Rec1886);
    private static readonly ColorEncoding AgXScene = new(Gamut.EGamut, new Transfer.Linear());
    private static readonly Action<Span<Vector4>> Desaturated = ToneCurve.Graded(static color =>
        (float.Max((color.X + color.Y + color.Z) / 3f, 0f), float.Min(color.X, float.Min(color.Y, color.Z))) switch {
            var (average, least) => Vector3.Lerp(color, new Vector3(average), least < 0f ? -least / (average - least) : 0f),
        });
    private static readonly Action<Span<Vector4>> Neutral = ToneCurve.Graded(static color => {
        const float fresnel = 0.04f;
        const float specular = 0.8f - fresnel;
        const float desaturation = 0.15f;
        float least = float.Min(color.X, float.Min(color.Y, color.Z));
        Vector3 shifted = color - new Vector3(least < 2f * fresnel ? least - (least * least / (4f * fresnel)) : fresnel);
        float peak = float.Max(shifted.X, float.Max(shifted.Y, shifted.Z));
        float compressed = 1f - ((1f - specular) * (1f - specular) / (peak + (1f - specular) - specular));
        return peak < specular ? shifted : Vector3.Lerp(shifted * (compressed / peak), new Vector3(compressed), 1f - (1f / ((desaturation * (peak - compressed)) + 1f)));
    });

    public static LutTable AgXGreyscale { get; } = new LutTable.Affine(Gamut.Columns(static rgb => new Vector3(Vector3.Dot(AgXWeights, rgb))));
    public static ViewTransform PbrNeutral { get; } = Shaped(Gamut.StandardRgb, Neutral);

    public Action<Span<Vector4>> Formed(ColorEncoding source, ColorEncoding target) {
        Action<Span<Vector4>>[] chain = [source.To(Scene).Apply, .. Steps, .. Signal.Map<Action<Span<Vector4>>>(signal => signal.To(target).Apply).ToSeq()];
        return row => {
            foreach (Action<Span<Vector4>> step in chain)
                step(row);
        };
    }

    public static ViewTransform AgX(LutSpace space, Seq<Action<Span<Vector4>>> looks, LutTable table, ColorEncoding signal) =>
        new(AgXScene, [.. Looked(space, looks), LutSpace.Filmic.Into.Apply, table.Apply], Some(signal));

    public static ViewTransform FalseColor(LutSpace space, Seq<Action<Span<Vector4>>> looks, LutTable table, Lut1D key) =>
        new(AgXScene, [.. Positioned(space, looks, table), new LutTable.ChannelCurve(key).Apply], Some(FalseColorSignal));

    public static Fin<(Seq<(double At, Vector4 Color)> Bands, Seq<Option<float>> Stops)> FalseColorLegend(LutSpace space, Seq<Action<Span<Vector4>>> looks, LutTable table, Lut1D key) {
        Seq<(int Entry, Vector4 Code)> bands = toSeq(Enumerable.Range(0, key.Length).Select(k => (Entry: k, Code: new Vector4(key.Channel(0)[k], key.Channel(1)[k], key.Channel(2)[k], 1f))).DistinctUntilChanged(static band => band.Code));
        Seq<double> openings = bands.Tail.Map(band => (band.Entry - 0.5d) / (key.Length - 1)).Strict();
        Vector4[] colors = [.. bands.Map(static band => band.Code)];
        Vector4[] positions = [.. Enumerable.Range(0, key.Length).Select(i => new Vector4(new Vector3(i / (key.Length - 1f)), 1f))];
        FalseColorSignal.Transfer.Decode.Apply(colors);
        LutSpace.Filmic.Back.Apply(positions);
        foreach (Action<Span<Vector4>> step in Positioned(space, looks, table))
            step(positions);
        float[] rows = GC.AllocateUninitializedArray<float>(3 * positions.Length);
        MemoryMarshal.Cast<Vector4, float>(positions).AsSpan2D(positions.Length, 4)[.., ..3].CopyTo(rows.AsSpan().AsSpan2D(positions.Length, 3));
        return Lut1D.From(rows, Vector3.Zero, Vector3.One)
            .Bind(static response => LutTable.Of([LutSpace.Filmic.Into, new LutTable.ChannelCurve(response)]).Inverse())
            .Map(scene => {
                Vector4[] edges = [.. openings.Map(static opening => new Vector4(new Vector3((float)opening), 1f))];
                scene.Apply(edges);
                return (0d.Cons(openings).Zip(toSeq(colors)).Map(static stop => (At: stop.First, Color: stop.Second)).Strict(),
                    Option<float>.None.Cons(toSeq(edges).Map(static edge => Some(float.Log2(edge.X / Metering.MiddleGray)))));
            });
    }

    public static ViewTransform Filmic(Lut3D desaturation, Option<Lut1D> look) =>
        new(LutSpace.Filmic.Encoding, [
            LutSpace.Filmic.Into.Apply,
            new LutTable.Cube(desaturation, LutInterpolation.Tetrahedral).Apply,
            new LutTable.Range(Some((0f, 0f)), Some((0.66f, 1f)), Clamps: false).Apply,
            .. look.Map<Action<Span<Vector4>>>(static curve => new LutTable.ChannelCurve(curve).Apply).ToSeq(),
        ], look.Map(static _ => DisplayDevice.Srgb.Encoding));

    public static ViewTransform Lattice(LutSpace shaper, LutTable table, ColorEncoding signal) => new(shaper.Encoding, [shaper.Into.Apply, table.Apply], Some(signal));

    public static ViewTransform Curve(Gamut basis, Func<float, float> curve, float preserveHue = 1f, float purity = 0f, Vector3 inset = default, Vector3 rotation = default) =>
        Shaped(
            basis,
            new LutTable.Affine(basis.Inset(inset, rotation).MatrixTo(basis)).Apply,
            ToneCurve.Graded(color => new Vector3(curve(color.X), curve(color.Y), curve(color.Z)), preserveHue),
            new LutTable.Affine(basis.MatrixTo(basis.Inset(purity * inset, rotation))).Apply);

    public static Fin<ViewTransform> Photoreceptor(Gamut working, PixelFrame frame, float intensity, Option<float> contrast, float light, float chromatic) =>
        from samples in LuminanceSamples.Gather(frame, working)
        from mean in frame.Mean(frame.Window).ToFin(ImagingFault.Of(ImagingReason.Empty, frame.Window))
        let logs = samples.LogStatistics
        let weights = working.Luminance
        let power = contrast.IfNone(() => logs.Maximum == logs.Minimum ? 1f : (float)(0.3d + (0.7d * double.Pow((logs.Maximum - logs.Mean) / (logs.Maximum - logs.Minimum), 1.4d))))
        let global = Vector3.Lerp(new Vector3(Vector3.Dot(weights, mean.AsVector3())), mean.AsVector3(), chromatic)
        select Shaped(working, ToneCurve.Graded(channel =>
            (channel + Vector3.Exp(power * Vector3.Log(float.Exp(-intensity) * Vector3.Lerp(global, Vector3.Lerp(new Vector3(Vector3.Dot(weights, channel)), channel, chromatic), light)))) switch {
                var total => Vector3.ConditionalSelect(Vector3.Equals(total, Vector3.Zero), Vector3.Zero, channel / total),
            }));

    private static ViewTransform Shaped(Gamut basis, params ReadOnlySpan<Action<Span<Vector4>>> steps) =>
        new ColorEncoding(basis, new Transfer.Linear()) switch { var linear => new(linear, [Desaturated, .. steps], Some(linear)) };

    private static Seq<Action<Span<Vector4>>> Looked(LutSpace space, Seq<Action<Span<Vector4>>> looks) =>
        looks.IsEmpty ? [] : [AgXScene.To(space.Encoding).Apply, space.Into.Apply, .. looks, space.Back.Apply, space.Encoding.To(AgXScene).Apply];

    private static Seq<Action<Span<Vector4>>> Positioned(LutSpace space, Seq<Action<Span<Vector4>>> looks, LutTable table) =>
        [.. Looked(space, looks), LutSpace.Filmic.Into.Apply, table.Apply, FalseColorSignal.Transfer.Decode.Apply, AgXGreyscale.Apply,
         new LutTable.Exponent(ExponentStyle.BasicRev, new Vector3(2.5f), Vector3.Zero).Apply];
}

public sealed record FilmStock(
    float PreExposure, Vector3 PreFilter, Vector3 Scale, Vector3 Rotation, Vector3 Multiplier, Vector2 LogRange, Vector2 Toe, Vector2 Shoulder,
    float NegativeExposure, float NegativeDensity, Vector3 Backlight, float PrintExposure, float PrintDensity, Option<float> BlackPoint,
    Vector3 PostFilter, Vector3 Weights, float MidtoneSaturation, Vector2 MidtoneWindow) {
    private const float Unlifted = 1e-4f;

    public static FilmStock Default { get; } = new(
        4.3f, Vector3.One, new(1.05f, 1.12f, 1.045f), new Vector3(0.5f, 2f, 0.1f) * (float.Pi / 180f), Vector3.One, new(-10f, 22f), new(0.44f, 0.28f), new(0.591f, 0.779f),
        6f, 5f, Vector3.One, 6f, 27.5f, None, Vector3.One, new(0.3f, 0.5f, 0.2f), 1.02f, new(0.5f, 0.45f));
    public static FilmStock Nostalgia { get; } =
        Default with { PreExposure = 5.563035f, Multiplier = new(1.1f, 1f, 1.2f), LogRange = new(-10f, 23f), NegativeExposure = 5.8f, Backlight = new(0.99f, 1.1f, 1.035989f), PrintDensity = 40f, BlackPoint = -5f, MidtoneSaturation = 1.1f };
    public static FilmStock Silver { get; } =
        Default with { PreExposure = 3.9f, PreFilter = new(0.95f, 0.975f, 1f), Multiplier = new(1f, 1f, 1.06f), NegativeExposure = 4.7f, NegativeDensity = 7f, Backlight = new(0.9992f, 0.99f, 1f) }
            with { PrintExposure = 4.7f, PrintDensity = 30f, BlackPoint = 0.5f, PostFilter = new(1f, 1f, 0.96f), MidtoneSaturation = 1f, };

    public Fin<ViewTransform> Developed() {
        Gamut basis = Gamut.StandardRgb;
        Vector3 Primary(int channel) =>
            new Unicolour(Configuration.Default, ColourSpace.Hsb, (120d * channel) + double.RadiansToDegrees(Rotation[channel]), 1d / Scale[channel], 1d).Rgb switch {
                var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B) switch { var row => row * (Multiplier[channel] / Vector3.Sum(row)) },
            };
        (Vector3 red, Vector3 green, Vector3 blue) = (Primary(0), Primary(1), Primary(2));
        Matrix4x4 extension = Matrix4x4.Transpose(new(red.X, red.Y, red.Z, 0f, green.X, green.Y, green.Z, 0f, blue.X, blue.Y, blue.Z, 0f, 0f, 0f, 0f, 1f));
        Func<float, float> characteristic = ToneCurve.Segmented((Toe.X, Toe.Y), (Shoulder.X, Shoulder.Y), 1d, 1d, 1d, 1d);
        float Transmittance(float stimulus, float density) =>
            float.Exp2(-density * characteristic(float.Clamp((float.Log2(stimulus + float.Exp2(LogRange.X)) - LogRange.X) / (LogRange.Y - LogRange.X), 0f, 1f)));
        Vector3 Transmitted(Vector3 stimulus, float density) => new(Transmittance(stimulus.X, density), Transmittance(stimulus.Y, density), Transmittance(stimulus.Z, density));
        (Vector3 pre, float negative) = (PreFilter * float.Exp2(PreExposure), float.Exp2(NegativeExposure));
        Vector3 backlight = Vector3.Transform(Backlight, extension) * float.Exp2(PrintExposure);
        Vector3 cap = Transmitted(backlight * float.Exp2(-NegativeDensity), PrintDensity);
        float black = float.Min(BlackPoint.Match(Some: static point => point / 1000f, None: () => Vector3.Dot(Transmitted(backlight, PrintDensity) / cap, Weights)), 0.999f);
        Fin<Matrix4x4> inverted = Matrix4x4.Invert(extension, out Matrix4x4 inverse) ? inverse : ImagingFault.Of(ImagingReason.Degenerate, extension);
        ColorEncoding linear = new(basis, new Transfer.Linear());
        return from back in inverted
               from table in LutSpace.Of(basis, LogCurve.Allocation(-10f, 10f, float.Exp2(-10f))).Bake(80, [ToneCurve.Graded(node => Displayed(node, back))], basis, linear)
               select new ViewTransform(linear, [table.Apply], Some(linear));

        Vector3 Displayed(Vector3 node, Matrix4x4 back) {
            Vector3 printed = Transmitted(backlight * Transmitted(Vector3.Transform(node * pre, extension) * negative, NegativeDensity), PrintDensity) / cap;
            float mono = Vector3.Dot(printed, Weights);
            Vector3 offset = mono < Unlifted ? printed : printed * (float.Clamp((mono - black) / (1f - black), 0f, 1f) / mono);
            Vector3 clipped = Vector3.Clamp(Vector3.Transform(offset, back) * PostFilter, Vector3.Zero, Vector3.One);
            (float most, float least) = (float.Max(clipped.X, float.Max(clipped.Y, clipped.Z)), float.Min(clipped.X, float.Min(clipped.Y, clipped.Z)));
            return Vector3.Lerp(
                clipped,
                new Vector3(most) - ((new Vector3(most) - clipped) * float.MinNumber(MidtoneSaturation, most / (most - least))),
                float.Max(1f - (float.Abs(Vector3.Dot(clipped, Weights) - MidtoneWindow.X) / MidtoneWindow.Y), 0f));
        }
    }
}
