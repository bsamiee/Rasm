using System.Numerics;
using System.Runtime.InteropServices;
using BitMiracle.LibTiff.Classic;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Quantization;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IOffsetField {
    public static abstract float Draw(long column, long line);
}

// --- [MODELS] --------------------------------------------------------------------------
internal readonly struct NoOffset : IOffsetField {
    public static float Draw(long column, long line) => 0.5f;
}

internal readonly struct WhiteNoise : IOffsetField {
    public static float Draw(long column, long line) =>
        1103515245u switch {
            var scale => unchecked(scale * ((scale * (((uint)column >> 1) ^ (uint)line)) ^ ((scale * (((uint)line >> 1) ^ (uint)column)) >> 3))) * (1f / uint.MaxValue),
        };
}

internal readonly struct BlueNoise : IOffsetField {
    private static readonly Memo<ReadOnlyMemory2D<float>> Draws = memo(Read);

    public static float Draw(long column, long line) =>
        Draws.Value.Span switch {
            var draws => draws[(int)(line & (draws.Height - 1)), (int)(column & (draws.Width - 1))],
        };

    private static ReadOnlyMemory2D<float> Read() {
        const string Resource = "BlueNoise.tif";
        using Stream stream = typeof(BlueNoise).Assembly.GetManifestResourceStream(typeof(BlueNoise), Resource)!;
        using Tiff tiff = Tiff.ClientOpen(Resource, "r", stream, new TiffStream());
        (int width, int height, int channels) = (tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt(), tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt(), tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt());
        const float codes = ushort.MaxValue + 1f;
        float half = 0.5f * codes / (width * height);
        float[] draws = new float[width * height];
        byte[] line = new byte[tiff.ScanlineSize()];
        for (int row = 0; row < height; row++) {
            _ = tiff.ReadScanline(line, row);
            ReadOnlySpan<ushort> ranks = MemoryMarshal.Cast<byte, ushort>(line);
            for (int column = 0; column < width; column++)
                draws[(row * width) + column] = (ranks[column * channels] + half) / codes;
        }
        return new(draws, height, width);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public partial class Dither {
    public static readonly Dither Off = new Pointwise<NoOffset>("off");
    public static readonly Dither WhiteNoise = new Pointwise<WhiteNoise>("white-noise");
    public static readonly Dither BlueNoise = new Pointwise<BlueNoise>("blue-noise");
    public static readonly Dither FloydSteinberg = new("floyd-steinberg", static levels => new PixelPass.Frame((frame, progress) => Diffuse(levels, frame, progress)));

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

    private sealed class Pointwise<TField>(string key) : Dither(key, static levels => new PixelPass.Pointwise(levels.Quantize<TField>)) where TField : IOffsetField;
}
