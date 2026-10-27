using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using SuperLinq;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FilmStock(
    float PreExposure, Channels<float> PreFilter, Channels<float> Scale, Channels<float> Rotation, Channels<float> Multiplier,
    float SigmoidMinimum, float SigmoidSpan, float ToeX, float ToeY, float ShoulderReachX, float ShoulderReachY,
    float NegativeExposure, float NegativeDensity, Channels<float> Backlight,
    float PrintExposure, float PrintDensity, Channels<float> PostFilter, float MidtoneSaturation) {
    private static readonly LogCurve Window = LogCurve.Allocation(-10f, 10f, float.ScaleB(1f, -10));
    private static readonly Vector256<double> Weights = Vector256.Create(0.3d, 0.5d, 0.2d, 0d);

    public static FilmStock Default { get; } = new(
        4.3f, new(1f), new(1.05f, 1.12f, 1.045f), new(float.DegreesToRadians(0.5f), float.DegreesToRadians(2f), float.DegreesToRadians(0.1f)), new(1f),
        -10f, 22f - -10f, 0.44f, 0.28f, (0.591f - 0.44f) / (1f - 0.44f), (0.779f - 0.28f) / (1f - 0.28f),
        6f, 5f, new(1f), 6f, 27.5f, new(1f), 1.02f);

    public Option<float> BlackPoint { get; init; }

    public static Action<Span<Vector4>> Formed(Gamut working, DisplayDevice device, LutTable developed) =>
        device.Formed(working, Gamut.StandardRgb, [developed.Apply], new ColorEncoding(Gamut.StandardRgb, new Transfer.Linear()));

    public Fin<LutTable> Developed() {
        Matrix<double> extension = Matrix<double>.Build.DiagonalOfDiagonalArray([Multiplier.Red, Multiplier.Green, Multiplier.Blue])
            * Matrix<double>.Build.DenseOfRowArrays(
                Seq((Scale: Scale.Red, Rotation: Rotation.Red), (Scale: Scale.Green, Rotation: Rotation.Green), (Scale: Scale.Blue, Rotation: Rotation.Blue))
                    .Map(static (primary, index) => new Unicolour(Configuration.Default, ColourSpace.Hsb, (120d * index) + double.RadiansToDegrees(primary.Rotation), 1d / primary.Scale, 1d).Rgb.ToArray()))
                .NormalizeRows(1d);
        ((Vector256<double> Red, Vector256<double> Green, Vector256<double> Blue) gamut, (Vector256<double> Red, Vector256<double> Green, Vector256<double> Blue) back) = (Lanes.Of(extension.Transpose()), Lanes.Of(extension.Inverse().Transpose()));
        (double toeX, double toeY) = (ToeX, ToeY);
        (double shoulderX, double shoulderY) = (toeX + (ShoulderReachX * (1d - toeX)), toeY + (ShoulderReachY * (1d - toeY)));
        double slope = (shoulderY - toeY) / (shoulderX - toeX);
        (double low, double span, double offset) = (SigmoidMinimum, SigmoidSpan, double.Exp2(SigmoidMinimum));
        (double negative, Vector256<double> pre, Vector256<double> post) = (
            double.Exp2(NegativeExposure), PreFilter.Lanes * double.Exp2(PreExposure), PostFilter.Lanes);
        Vector256<double> backlight = Transformed(gamut, Backlight.Lanes) * double.Exp2(PrintExposure);
        Vector256<double> cap = Transmitted(backlight * double.Exp2(-NegativeDensity), PrintDensity);
        double black = BlackPoint.Match(
            Some: static offset => offset / 1000d,
            None: () => double.Min(Vector256.Dot(Transmitted(backlight, PrintDensity) / cap, Weights), 0.999d));
        return Bakes.Lattice(80, [row => {
            foreach (ref Vector4 node in row)
                node = new Vector4(Displayed(Window.Inverse(node.AsVector3())), node.W);
        }]).Map(static lattice => (LutTable)new LutTable.Sequence([new LutTable.Log(Window), new LutTable.Cube(lattice, LutInterpolation.Tetrahedral)]));

        double Sigmoid(double x) =>
            x < toeX ? toeY * double.Pow(x / toeX, slope * toeX / toeY)
            : x < shoulderX ? toeY + (slope * (x - toeX))
            : 1d - ((1d - shoulderY) * double.Pow((1d - x) / (1d - shoulderX), slope * (1d - shoulderX) / (1d - shoulderY)));

        double Transmittance(double stimulus, double density) => double.Exp2(-density * Sigmoid(double.Clamp((double.Log2(stimulus + offset) - low) / span, 0d, 1d)));

        Vector256<double> Transmitted(Vector256<double> stimulus, double density) =>
            Vector256.Create(Transmittance(stimulus[0], density), Transmittance(stimulus[1], density), Transmittance(stimulus[2], density), 1d);

        Vector3 Displayed(Vector3 node) {
            Vector256<double> printed = Transmitted(backlight * Transmitted(Transformed(gamut, Vector256.Create(node.X, node.Y, node.Z, 0d) * pre) * negative, NegativeDensity), PrintDensity) / cap;
            double mono = Vector256.Dot(printed, Weights);
            Vector256<double> clipped = Vector256.Clamp(Transformed(back, printed * (double.Clamp((mono - black) / (1d - black), 0d, 1d) / mono)) * post, Vector256<double>.Zero, Vector256<double>.One);
            (double most, double least) = (double.Max(clipped[0], double.Max(clipped[1], clipped[2])), double.Min(clipped[0], double.Min(clipped[1], clipped[2])));
            Vector256<double> midtone = Vector256.Lerp(
                clipped,
                Vector256.Create(most) - ((Vector256.Create(most) - clipped) * double.MinNumber(MidtoneSaturation, most / (most - least))),
                Vector256.Create(double.Max(1d - (double.Abs(Vector256.Dot(clipped, Weights) - 0.5d) / 0.45d), 0d)));
            return new Vector3((float)midtone[0], (float)midtone[1], (float)midtone[2]);
        }

        static Vector256<double> Transformed((Vector256<double> Red, Vector256<double> Green, Vector256<double> Blue) lanes, Vector256<double> value) =>
            Vector256.Create(Vector256.Dot(lanes.Red, value), Vector256.Dot(lanes.Green, value), Vector256.Dot(lanes.Blue, value), 0d);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AgX {
    public static LutTable Grey { get; } = new LutTable.Affine(Matrix<double>.Build.DenseOfRowArrays(Enumerable.Repeat<double[]>([0.2589235355689848d, 0.6104985346066525d, 0.13057792982436284d], 3)));

    public static Action<Span<Vector4>> Formed(Gamut working, DisplayDevice device, LogSpace space, Seq<Action<Span<Vector4>>> graded, LutTable table, ColorEncoding signal) =>
        device.Formed(working, space.Gamut, [.. graded, new LutTable.Log(space.Allocation).Apply, table.Apply], signal);

    public static Seq<Action<Span<Vector4>>> Looked(LogSpace space, Action<Span<Vector4>> look) =>
        [.. space.Into.Map<Action<Span<Vector4>>>(static step => step.Apply), look, .. space.Back.Map<Action<Span<Vector4>>>(static step => step.Apply)];
}

public static class FalseColor {
    private static readonly ColorEncoding Signal = new(Gamut.StandardRgb, Transfer.Gamma.Rec1886);

    public static Action<Span<Vector4>> Formed(Gamut working, DisplayDevice device, LogSpace space, Seq<Action<Span<Vector4>>> graded, LutTable table, Lut1D key) =>
        device.Formed(working, space.Gamut, Positioned(space, graded, table).Add(new LutTable.ChannelCurve(key).Apply), Signal);

    public static Fin<(Seq<(double At, Vector4 Color)> Bands, Seq<Option<float>> Stops)> Legend(LogSpace space, Seq<Action<Span<Vector4>>> graded, LutTable table, Lut1D key) {
        Seq<Vector4> codes = toSeq(Range(0, key.Length)).Map(k => new Vector4(key.Channel(0)[k], key.Channel(1)[k], key.Channel(2)[k], 1f)).Strict();
        Seq<(int Entry, Vector4 Code)> bands = toSeq(codes.Select(static (code, k) => (Entry: k, Code: code)).DistinctUntilChanged(static step => step.Code));
        Seq<double> openings = bands.Tail.Map(band => (band.Entry - 0.5d) / (key.Length - 1)).Strict();
        Vector4[] colors = [.. bands.Map(static band => band.Code)];
        Vector4[] positions = [.. toSeq(Range(0, key.Length)).Map(i => new Vector4(new Vector3(i / (key.Length - 1f)), 1f))];
        Signal.Transfer.Decode(colors);
        new LutTable.Antilog(space.Allocation).Apply(positions);
        foreach (Action<Span<Vector4>> step in Positioned(space, graded, table))
            step(positions);
        float[] rows = GC.AllocateUninitializedArray<float>(3 * positions.Length);
        MemoryMarshal.Cast<Vector4, float>(positions).AsSpan2D(positions.Length, 4)[.., ..3].CopyTo(rows.AsSpan().AsSpan2D(positions.Length, 3));
        return Lut1D.From(key.Length, rows, Vector3.Zero, Vector3.One)
            .Bind(response => new LutTable.Sequence([new LutTable.Log(space.Allocation), new LutTable.ChannelCurve(response)]).Inverse())
            .Map(scene => (0d.Cons(openings).Zip(toSeq(colors)).Map(static stop => (At: stop.First, Color: stop.Second)).Strict(), Labels(openings, scene)));

        static Seq<Option<float>> Labels(Seq<double> openings, LutTable scene) {
            Vector4[] edges = [.. openings.Map(static v => new Vector4(new Vector3((float)v), 1f))];
            scene.Apply(edges);
            return Option<float>.None.Cons(toSeq(edges).Map(static edge => Some(float.Log2(edge.X / Exposure.MiddleGray))));
        }
    }

    private static Seq<Action<Span<Vector4>>> Positioned(LogSpace space, Seq<Action<Span<Vector4>>> graded, LutTable table) =>
        [.. graded, new LutTable.Log(space.Allocation).Apply, table.Apply, Transfer.Gamma.Rec1886.Decode, AgX.Grey.Apply,
         new LutTable.Exponent(ExponentStyle.BasicRev, new Vector3(2.5f), Vector3.Zero).Apply];
}

public static class Filmic {
    public static Action<Span<Vector4>> Formed(Gamut working, DisplayDevice device, Lut3D desaturation, Option<Lut1D> look) =>
        device.Formed(
            working, LogSpace.Filmic.Gamut,
            [.. LogSpace.Filmic.Into.Map<Action<Span<Vector4>>>(static step => step.Apply),
             new LutTable.Cube(desaturation, LutInterpolation.Tetrahedral).Apply,
             new LutTable.Remap(new RemapBounds.Between(new RemapBound(0f, 0f), new RemapBound(0.66f, 1f), Clamps: false)).Apply,
             .. look.Map<Action<Span<Vector4>>>(static curve => new LutTable.ChannelCurve(curve).Apply).ToSeq()],
            look.Match(Some: static _ => DisplayDevice.Srgb.Encoding, None: () => device.Encoding));
}

public static class Tony {
    private const uint Mantissa = 0x1FF;
    private const int Texel = sizeof(uint);
    private const int Header = 148;
    private const uint SharedExponent = 67;

    public static Fin<Lut3D> Table(ReadOnlyMemory<byte> texture) {
        ReadOnlySpan<byte> bytes = texture.Span;
        if (bytes.Length < Header)
            return new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Tony cube texture of {bytes.Length} bytes"), ImagingReason.Unsupported);
        (int height, int width, int depth, uint format) = (
            BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]), BinaryPrimitives.ReadInt32LittleEndian(bytes[16..]),
            BinaryPrimitives.ReadInt32LittleEndian(bytes[24..]), BinaryPrimitives.ReadUInt32LittleEndian(bytes[128..]));
        return from edge in width == height && width == depth && format == SharedExponent && bytes.Length == Header + (Texel * width * height * depth)
                ? Fin.Succ(width)
                : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Tony R9G9B9E5 cube texture of {bytes.Length} bytes at {width}x{height}x{depth} format {format}"), ImagingReason.Unsupported)
               from table in Lut3D.From(edge, Decoded(texture[Header..].Span, edge), Vector3.Zero, Vector3.One)
               select table;
    }

    public static Action<Span<Vector4>> Formed(Gamut working, DisplayDevice device, Lut3D table) =>
        device.Formed(
            working, Gamut.StandardRgb,
            [static row => {
                foreach (ref Vector4 pixel in row)
                    pixel = Vector3.Max(pixel.AsVector3(), Vector3.Zero) switch {
                        var stimulus => new Vector4(stimulus / (stimulus + Vector3.One), pixel.W),
                    };
            }, new LutTable.Cube(table, LutInterpolation.Tetrahedral).Apply],
            new ColorEncoding(Gamut.StandardRgb, new Transfer.Linear()));

    private static float[] Decoded(ReadOnlySpan<byte> texels, int edge) {
        float[] samples = GC.AllocateUninitializedArray<float>(3 * edge * edge * edge);
        Span<Vector3> nodes = MemoryMarshal.Cast<float, Vector3>(samples.AsSpan());
        for (int node = 0; node < nodes.Length; node++)
            nodes[node] = BinaryPrimitives.ReadUInt32LittleEndian(texels[(Texel * node)..]) switch {
                var texel => new Vector3(texel & Mantissa, (texel >> 9) & Mantissa, (texel >> 18) & Mantissa) * float.ScaleB(1f, (int)(texel >> 27) - 24),
            };
        return samples;
    }
}
