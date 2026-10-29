using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Dithering;

namespace Rasm.Imaging.Quantization;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IThreshold {
    public static abstract bool Noise { get; }

    public static abstract Vector4 Draw(int x, int y);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Dither {
    public static readonly Dither Off = new("off", static (levels, _) => Thresholded<Ordered>(levels, 0f));
    public static readonly Dither WhiteNoise = new("white-noise", static (levels, amount) => Thresholded<White>(levels, amount));
    public static readonly Dither BlueNoise = new("blue-noise", static (levels, amount) => Thresholded<Blue>(levels, amount));
    public static readonly Dither Bayer = new("bayer", static (levels, amount) => Thresholded<Ordered>(levels, amount));
    public static readonly Dither FloydSteinberg = new("floyd-steinberg", Diffused(ErrorDither.FloydSteinberg));
    public static readonly Dither Atkinson = new("atkinson", Diffused(ErrorDither.Atkinson));
    public static readonly Dither Burkes = new("burkes", Diffused(ErrorDither.Burkes));
    public static readonly Dither JarvisJudiceNinke = new("jarvis-judice-ninke", Diffused(ErrorDither.JarvisJudiceNinke));
    public static readonly Dither Sierra2 = new("sierra-2", Diffused(ErrorDither.Sierra2));
    public static readonly Dither Sierra3 = new("sierra-3", Diffused(ErrorDither.Sierra3));
    public static readonly Dither SierraLite = new("sierra-lite", Diffused(ErrorDither.SierraLite));
    public static readonly Dither StevensonArce = new("stevenson-arce", Diffused(ErrorDither.StevensonArce));
    public static readonly Dither Stucki = new("stucki", Diffused(ErrorDither.Stucki));

    public Func<Quantizer, float, PixelPass> Pass { get; }

    public static Fin<PixelPass> Posterized(Quantizer levels, float amount, float preserveHue, float cell) =>
        ImagingFault.Unless(float.IsFinite(cell) && cell > 0f, ImagingReason.Range, cell)
            .Map(_ => (PixelPass)new PixelPass.Positional((row, x, y) => Quantize<Ordered>(row, x, y, levels, amount, preserveHue, 1f / cell)));

    private static PixelPass.Positional Thresholded<T>(Quantizer levels, float amount) where T : struct, IThreshold =>
        new((row, x, y) => Quantize<T>(row, x, y, levels, amount, 0f, 1f));

    private static Func<Quantizer, float, PixelPass> Diffused(ErrorDither kernel) =>
        (levels, amount) => new PixelPass.Frame((frame, progress) =>
            frame.Mutate(image => kernel.ApplyPaletteDither(new Rounding(levels, amount), image.Frames.RootFrame, image.Bounds))
                .Map(_ => progress.Iter(held => held.Report(frame.Size.Height))));

    private static void Quantize<T>(Span<Vector4> row, int x, int y, Quantizer levels, float amount, float preserveHue, float scale) where T : struct, IThreshold {
        int line = (int)float.Floor((y + 0.5f) * scale);
        for (int at = 0; at < row.Length; at++) {
            (Vector4 pixel, Vector4 code, Vector4 draw) = (row[at], levels.MaxCode * row[at], T.Draw((int)float.Floor((x + at + 0.5f) * scale), line));
            Vector3 mapped = (levels.Round(code + (amount * (T.Noise ? Triangular(draw, code, levels.MaxCode) : draw - new Vector4(0.5f)))) / levels.MaxCode).AsVector3();
            row[at] = new Vector4(preserveHue > 0f ? ToneCurve.PreserveHue(pixel.AsVector3(), mapped, preserveHue) : mapped, pixel.W);
        }
    }

    private static Vector4 Triangular(Vector4 draw, Vector4 code, float maxCode) {
        (Vector4 signed, Vector4 rectangular) = ((2f * draw) - Vector4.One, draw - new Vector4(0.5f));
        return rectangular + (Vector4.Clamp(2f * Vector4.Min(code, new Vector4(maxCode) - code), Vector4.Zero, Vector4.One)
            * (Vector4.CopySign(Vector4.One - Vector4.SquareRoot(Vector4.One - Vector4.Abs(signed)), signed) - rectangular));
    }

    private readonly struct Rounding(Quantizer levels, float amount) : IPaletteDitherImageProcessor<RgbaVector> {
        public Configuration Configuration => Configuration.Default;
        public ReadOnlyMemory<RgbaVector> Palette => ReadOnlyMemory<RgbaVector>.Empty;
        public float DitherScale => amount;

        public RgbaVector GetPaletteColor(RgbaVector color) =>
            RgbaVector.FromVector4(new Vector4((levels.Round(levels.MaxCode * color.ToVector4()) / levels.MaxCode).AsVector3(), color.A));
    }

    private readonly struct Ordered : IThreshold {
        public static bool Noise => false;

        public static Vector4 Draw(int x, int y) =>
            ((x ^ y) & 7, y & 7) switch {
                var (b, v) => new Vector4(((((b & 1) << 5) | ((v & 1) << 4) | ((b & 2) << 2) | ((v & 2) << 1) | ((b & 4) >> 1) | ((v & 4) >> 2)) + 0.5f) / 64f),
            };
    }

    private readonly struct White : IThreshold {
        private static readonly Vector128<uint> Scale = Vector128.Create(1103515245u);
        private static readonly Vector128<uint> Salts = Vector128.Create(0u, 1u, 2u, 3u) * Scale;

        public static bool Noise => true;

        public static Vector4 Draw(int x, int y) =>
            (Vector128.Create((uint)x) ^ Salts, Vector128.Create((uint)y)) switch {
                var (column, line) => Vector128.ConvertToSingle(Scale * ((Scale * ((column >> 1) ^ line)) ^ ((Scale * ((line >> 1) ^ column)) >> 3))).AsVector4() * (1f / uint.MaxValue),
            };
    }

    private readonly struct Blue : IThreshold {
        private static readonly Memo<ReadOnlyMemory2D<Vector4>> Draws = memo(Read);

        public static bool Noise => true;

        public static Vector4 Draw(int x, int y) => Draws.Value.Span switch { var draws => draws[y & (draws.Height - 1), x & (draws.Width - 1)] };

        private static ReadOnlyMemory2D<Vector4> Read() {
            const float codes = ushort.MaxValue + 1f;
            using Stream stream = typeof(Dither).Assembly.GetManifestResourceStream(typeof(Dither), "BlueNoise.tif")!;
            using Image<RgbaVector> image = Image.Load<RgbaVector>(stream);
            float[] draws = GC.AllocateUninitializedArray<float>(4 * image.Width * image.Height);
            image.CopyPixelDataTo(MemoryMarshal.Cast<float, RgbaVector>(draws.AsSpan()));
            TensorPrimitives.Multiply(draws, ushort.MaxValue / codes, draws);
            TensorPrimitives.Add(draws, 0.5f / codes, draws);
            return new Memory<float>(draws).Cast<float, Vector4>().AsMemory2D(image.Height, image.Width);
        }
    }
}
