using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FilmStock(
    float PreExposure, Vector3 PreFilter, Vector3 Scale, Vector3 Rotation, Vector3 Multiplier,
    float SigmoidMinimum, float SigmoidSpan, float ToeX, float ToeY, float ShoulderReachX, float ShoulderReachY,
    float NegativeExposure, float NegativeDensity, Vector3 Backlight,
    float PrintExposure, float PrintDensity, Vector3 PostFilter, float MidtoneSaturation) {
    private static readonly LogCurve Window = LogCurve.Allocation(-10f, 10f, float.ScaleB(1f, -10));
    private static readonly Vector256<double> Weights = Vector256.Create(0.3d, 0.5d, 0.2d, 0d);

    public static FilmStock Default { get; } = new(
        4.3f, Vector3.One, new Vector3(1.05f, 1.12f, 1.045f), new Vector3(float.DegreesToRadians(0.5f), float.DegreesToRadians(2f), float.DegreesToRadians(0.1f)), Vector3.One,
        -10f, 22f - -10f, 0.44f, 0.28f, (0.591f - 0.44f) / (1f - 0.44f), (0.779f - 0.28f) / (1f - 0.28f),
        6f, 5f, Vector3.One, 6f, 27.5f, Vector3.One, 1.02f);

    public Option<float> BlackPoint { get; init; }

    public static Action<Span<Vector4>> Formed(Gamut working, DisplayDevice device, LutTable developed) =>
        device.Formed(working, Gamut.StandardRgb, [developed.Apply], new ColorEncoding(Gamut.StandardRgb, new Transfer.Linear()));

    public Fin<LutTable> Developed() {
        Matrix<double> extension = Matrix<double>.Build.DiagonalOfDiagonalArray([Multiplier.X, Multiplier.Y, Multiplier.Z])
            * Matrix<double>.Build.DenseOfRowArrays(
                Seq((Scale: Scale.X, Rotation: Rotation.X), (Scale: Scale.Y, Rotation: Rotation.Y), (Scale: Scale.Z, Rotation: Rotation.Z))
                    .Map(static (primary, index) => new Unicolour(Configuration.Default, ColourSpace.Hsb, (120d * index) + double.RadiansToDegrees(primary.Rotation), 1d / primary.Scale, 1d).Rgb.ToArray()))
                .NormalizeRows(1d);
        (Func<Vector256<double>, Vector256<double>> gamut, Func<Vector256<double>, Vector256<double>> back) = (Transform(extension), Transform(extension.Inverse()));
        (double toeX, double toeY) = (ToeX, ToeY);
        (double shoulderX, double shoulderY) = (toeX + (ShoulderReachX * (1d - toeX)), toeY + (ShoulderReachY * (1d - toeY)));
        double slope = (shoulderY - toeY) / (shoulderX - toeX);
        (double low, double span, double offset) = (SigmoidMinimum, SigmoidSpan, double.Exp2(SigmoidMinimum));
        (double negative, Vector256<double> pre, Vector256<double> post) = (
            double.Exp2(NegativeExposure), Vector256.Create(PreFilter.X, PreFilter.Y, PreFilter.Z, 0d) * double.Exp2(PreExposure), Vector256.Create(PostFilter.X, PostFilter.Y, PostFilter.Z, 0d));
        Vector256<double> backlight = gamut(Vector256.Create(Backlight.X, Backlight.Y, Backlight.Z, 0d)) * double.Exp2(PrintExposure);
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
            Vector256<double> printed = Transmitted(backlight * Transmitted(gamut(Vector256.Create(node.X, node.Y, node.Z, 0d) * pre) * negative, NegativeDensity), PrintDensity) / cap;
            double mono = Vector256.Dot(printed, Weights);
            Vector256<double> clipped = Vector256.Clamp(back(printed * (double.Clamp((mono - black) / (1d - black), 0d, 1d) / mono)) * post, Vector256<double>.Zero, Vector256<double>.One);
            (double most, double least) = (double.Max(clipped[0], double.Max(clipped[1], clipped[2])), double.Min(clipped[0], double.Min(clipped[1], clipped[2])));
            Vector256<double> midtone = Vector256.Lerp(
                clipped,
                Vector256.Create(most) - ((Vector256.Create(most) - clipped) * double.MinNumber(MidtoneSaturation, most / (most - least))),
                Vector256.Create(double.Max(1d - (double.Abs(Vector256.Dot(clipped, Weights) - 0.5d) / 0.45d), 0d)));
            return new Vector3((float)midtone[0], (float)midtone[1], (float)midtone[2]);
        }
    }

    private static Func<Vector256<double>, Vector256<double>> Transform(Matrix<double> matrix) =>
        Lanes.Of(matrix.Transpose()) switch {
            var (red, green, blue) => value => Vector256.Create(Vector256.Dot(red, value), Vector256.Dot(green, value), Vector256.Dot(blue, value), 0d),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AgX {
    public static LutTable Grey { get; } = (X: 0.2589235355689848d, Y: 0.6104985346066525d, Z: 0.13057792982436284d) switch {
        var weights => new LutTable.Affine(Matrix<double>.Build.Dense(3, 3, (_, column) => column switch { 0 => weights.X, 1 => weights.Y, _ => weights.Z })),
    };

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
        Seq<(int Entry, Vector4 Code)> bands = codes
            .Zip(Option<Vector4>.None.Cons(codes.Map(static code => Some(code))))
            .Map(static (pair, k) => (Entry: k, Code: pair.First, Before: pair.Second))
            .Filter(static step => step.Before != Some(step.Code))
            .Map(static step => (step.Entry, step.Code));
        Seq<double> openings = bands.Tail.Map(band => (band.Entry - 0.5d) / (key.Length - 1)).Strict();
        Vector4[] colors = [.. bands.Map(static band => band.Code)];
        Vector4[] positions = [.. toSeq(Range(0, key.Length)).Map(i => new Vector4(new Vector3(i / (key.Length - 1f)), 1f))];
        Signal.Transfer.Decode(colors);
        new LutTable.Antilog(space.Allocation).Apply(positions);
        foreach (Action<Span<Vector4>> step in Positioned(space, graded, table))
            step(positions);
        return Lut1D.From(key.Length, positions.SelectMany(static position => (float[])[position.X, position.Y, position.Z]).ToArray(), Vector3.Zero, Vector3.One)
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
        (int height, int width, int depth, uint format) = bytes.Length < Header ? (0, 0, 0, 0u) : (
            BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]), BinaryPrimitives.ReadInt32LittleEndian(bytes[16..]),
            BinaryPrimitives.ReadInt32LittleEndian(bytes[24..]), BinaryPrimitives.ReadUInt32LittleEndian(bytes[128..]));
        return from edge in width == height && width == depth && format == SharedExponent && bytes.Length == Header + (Texel * width * height * depth)
                ? Fin.Succ(width)
                : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Tony texture must hold a cubic R9G9B9E5 volume, holds {bytes.Length} bytes of {width}x{height}x{depth} format {format}"))
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
