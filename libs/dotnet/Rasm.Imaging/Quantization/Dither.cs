using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Quantization;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IOffsetField {
    public static abstract float Draw(int column, int line);
}

// --- [MODELS] --------------------------------------------------------------------------
internal readonly struct NoOffset : IOffsetField {
    public static float Draw(int column, int line) => 0.5f;
}

internal readonly struct WhiteNoise : IOffsetField {
    public static float Draw(int column, int line) =>
        1103515245u switch {
            var scale => unchecked(scale * ((scale * (((uint)column >> 1) ^ (uint)line)) ^ ((scale * (((uint)line >> 1) ^ (uint)column)) >> 3))) * (1f / uint.MaxValue),
        };
}

internal readonly struct BlueNoise : IOffsetField {
    private static readonly Memo<ReadOnlyMemory2D<float>> Draws = memo(Read);

    public static float Draw(int column, int line) =>
        Draws.Value.Span switch {
            var draws => draws[line & (draws.Height - 1), column & (draws.Width - 1)],
        };

    private static ReadOnlyMemory2D<float> Read() {
        using Stream stream = typeof(BlueNoise).Assembly.GetManifestResourceStream(typeof(BlueNoise), "BlueNoise.png")!;
        using Image<Rgba64> image = PngDecoder.Instance.Decode<Rgba64>(new DecoderOptions { SkipMetadata = true }, stream);
        Rgba64[] ranks = new Rgba64[image.Width * image.Height];
        image.CopyPixelDataTo(ranks);
        const float codes = ushort.MaxValue + 1f;
        float half = 0.5f * codes / ranks.Length;
        return new(System.Array.ConvertAll(ranks, rank => (rank.R + half) / codes), image.Height, image.Width);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidQuantization>]
public sealed partial class Dither : IStateRecord<Dither, DitherParameter, InvalidQuantization>, IPixelStage<Dither> {
    public static readonly Dither Off = new("off", static levels => new PixelPass.Pointwise(levels.Quantize<NoOffset>));
    public static readonly Dither WhiteNoise = new("white-noise", static levels => new PixelPass.Pointwise(levels.Quantize<WhiteNoise>));
    public static readonly Dither BlueNoise = new("blue-noise", static levels => new PixelPass.Pointwise(levels.Quantize<BlueNoise>));
    public static readonly Dither FloydSteinberg = new("floyd-steinberg", static levels => new PixelPass.Frame((frame, progress) => Diffuse(levels, frame, progress)));

    [IgnoreMember]
    public static Dither Default => WhiteNoise;

    [UseDelegateFromConstructor]
    public partial PixelPass Pass(Quantizer levels);

    public static Option<PixelPass> Pass(Dither state, PassContext context) => context.Depth.Levels.Map(state.Pass);

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
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidQuantization>]
public sealed partial class DitherParameter : IStateParameter<Dither> {
    public static readonly DitherParameter Method = new("method", new StateParameter<Dither>.Choice<Dither, InvalidQuantization>(Lens.identity<Dither>()));

    public StateParameter<Dither> Kind { get; }
}
