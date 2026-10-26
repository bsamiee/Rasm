using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Quantization;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Dither {
    public static readonly Dither Off = new("off", static levels => new PixelPass.Pointwise((row, column, line) => levels.Quantize(row, column, line, Flat)));
    public static readonly Dither WhiteNoise = new("white-noise", static levels => new PixelPass.Pointwise((row, column, line) => levels.Quantize(row, column, line, White)));
    public static readonly Dither BlueNoise = new("blue-noise", static levels => new PixelPass.Pointwise((row, column, line) => levels.Quantize(row, column, line, Blue)));
    public static readonly Dither FloydSteinberg = new("floyd-steinberg", static levels => new PixelPass.Frame((frame, progress) => Diffuse(levels, frame, progress)));

    private static readonly Memo<ReadOnlyMemory2D<float>> Draws = memo(Read);

    [UseDelegateFromConstructor]
    public partial PixelPass Pass(Quantizer levels);

    private static Unit Diffuse(Quantizer levels, PixelFrame frame, IProgress<int> progress) {
        Span2D<Vector4> rows = frame.View.Span;
        using SpanOwner<Vector4> rental = SpanOwner<Vector4>.Allocate(2 * (rows.Width + 2), AllocationMode.Clear);
        Span2D<Vector4> carried = rental.Span.AsSpan2D(2, rows.Width + 2);
        for (int line = 0; line < rows.Height; line++) {
            Span<Vector4> current = carried.GetRowSpan(line & 1);
            Span<Vector4> next = carried.GetRowSpan(~line & 1);
            Span<Vector4> row = rows.GetRowSpan(line);
            int step = 1 - (2 * (line & 1));
            next.Clear();
            for (int visit = 0; visit < row.Length; visit++) {
                int x = step > 0 ? visit : row.Length - 1 - visit;
                Vector4 pixel = row[x];
                Vector4 held = Vector4.Clamp((levels.MaxCode * pixel) + current[x + 1], Vector4.Zero, Vector4.Create(levels.MaxCode));
                Vector4 code = levels.Round(held);
                Vector4 error = held - code;
                row[x] = Vector4.Create((code / levels.MaxCode).AsVector3(), pixel.W);
                current[x + 1 + step] += 7f / 16f * error;
                next[x + 1 - step] += 3f / 16f * error;
                next[x + 1] += 5f / 16f * error;
                next[x + 1 + step] += 1f / 16f * error;
            }
            progress.Report(line + 1);
        }
        return unit;
    }

    private static float Flat(long column, long line) => 0.5f;

    private static float White(long column, long line) =>
        1103515245u switch {
            var scale => unchecked(scale * ((scale * (((uint)column >> 1) ^ (uint)line)) ^ ((scale * (((uint)line >> 1) ^ (uint)column)) >> 3))) * (1f / uint.MaxValue),
        };

    private static float Blue(long column, long line) =>
        Draws.Value.Span switch {
            var draws => draws[(int)(line & (draws.Height - 1)), (int)(column & (draws.Width - 1))],
        };

    private static ReadOnlyMemory2D<float> Read() {
        const float codes = ushort.MaxValue + 1f;
        using Stream stream = typeof(Dither).Assembly.GetManifestResourceStream(typeof(Dither), "BlueNoise.tif")!;
        using Image<Rgba64> image = Image.Load<Rgba64>(stream);
        using SpanOwner<Rgba64> pixels = SpanOwner<Rgba64>.Allocate(image.Width * image.Height);
        using SpanOwner<ushort> ranks = SpanOwner<ushort>.Allocate(pixels.Length);
        float[] draws = GC.AllocateUninitializedArray<float>(pixels.Length);
        image.CopyPixelDataTo(pixels.Span);
        MemoryMarshal.Cast<Rgba64, ushort>(pixels.Span).AsSpan2D(pixels.Length, 4).GetColumn(0).CopyTo(ranks.Span);
        TensorPrimitives.ConvertChecked<ushort, float>(ranks.Span, draws);
        TensorPrimitives.Add(draws, 0.5f * codes / draws.Length, draws);
        TensorPrimitives.Divide(draws, codes, draws);
        return new(draws, image.Height, image.Width);
    }
}
