using System.Drawing;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Rasm.Imaging.ColorManagement;
using TinyEXR;

namespace Rasm.Imaging.Pixels;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IRowFold {
    public int Bins { get; }
    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ClipLevels(float Low, float High);

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ClipEnd {
    public static readonly ClipEnd Low = new(below: true, above: false);
    public static readonly ClipEnd High = new(below: false, above: true);
    public static readonly ClipEnd Both = new(below: true, above: true);

    public bool Below { get; }
    public bool Above { get; }
}

[ComplexValueObject]
[ValidationError<InvalidPixelValue>]
public readonly partial struct BinRange {
    public int First { get; }
    public int Last { get; }

    public bool Holds(float level) => !float.IsNaN(level) && HistogramAxis.Bin(level) is var bin && bin >= First && bin <= Last;

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref int first, ref int last) =>
        validationError = 0 <= first && first <= last && last < HistogramAxis.Bins ? null : new InvalidPixelValue();
}

[SmartEnum<int>]
[ValidationError<InvalidPixelValue>]
public sealed partial class ChromaZoom {
    public static readonly ChromaZoom Normal = new(1);
    public static readonly ChromaZoom Doubled = new(2);

    public float Reach => 0.5f / Key;
}

[SmartEnum<int>]
[ValidationError<InvalidPixelValue>]
public sealed partial class SampleSquare {
    public static readonly SampleSquare One = new(1);
    public static readonly SampleSquare Three = new(3);
    public static readonly SampleSquare Five = new(5);
    public static readonly SampleSquare Seven = new(7);

    public Option<Vector4> Mean(PixelFrame frame, Point pixel) {
        Rectangle square = Rectangle.Intersect(
            new(pixel.X - frame.Origin.X - (Key / 2), pixel.Y - frame.Origin.Y - (Key / 2), Key, Key),
            new(0, 0, frame.Size.Width, frame.Size.Height));
        (Vector4 sum, int count) = (Vector4.Zero, 0);
        foreach (Vector4 sample in frame.View.Span.Slice(square.Y, square.X, square.Height, square.Width))
            (sum, count) = sample.W > 0f ? (sum + sample, count + 1) : (sum, count);
        return count > 0 ? sum / count : None;
    }
}

public sealed record BinGrid {
    internal BinGrid(int columns, int rows, ReadOnlyMemory<int> counts) => (Columns, Rows, Counts) = (columns, rows, counts);

    public int Columns { get; }
    public int Rows { get; }
    public ReadOnlyMemory<int> Counts { get; }

    public ReadOnlySpan<int> Row(int row) => Counts.Span.AsSpan2D(Rows, Columns).GetRowSpan(row);
}

public sealed record Traces(BinGrid Red, BinGrid Green, BinGrid Blue, BinGrid Luma) {
    internal static Traces Of(int[] counts, int columns, int rows) =>
        (columns * rows) switch {
            var cells => new(
                new BinGrid(columns, rows, counts.AsMemory(0, cells)),
                new BinGrid(columns, rows, counts.AsMemory(cells, cells)),
                new BinGrid(columns, rows, counts.AsMemory(2 * cells, cells)),
                new BinGrid(columns, rows, counts.AsMemory(3 * cells, cells))),
        };
}

public readonly record struct ChannelCounts(int Red, int Green, int Blue);

public readonly record struct Clips(ChannelCounts Low, ChannelCounts High) {
    internal const int Ends = 6;

    internal static Clips Of(ReadOnlySpan<int> counts) =>
        counts[..Ends] switch {
            var ends => new(new(ends[0], ends[1], ends[2]), new(ends[3], ends[4], ends[5])),
        };
}

public readonly record struct ChromaAxes(Vector3 Weights) {
    public Matrix4x4 Forward => new(
        Weights.X, -Weights.X / (2f * (1f - Weights.Z)), 0.5f, 0f,
        Weights.Y, -Weights.Y / (2f * (1f - Weights.Z)), -Weights.Y / (2f * (1f - Weights.X)), 0f,
        Weights.Z, 0.5f, -Weights.Z / (2f * (1f - Weights.X)), 0f,
        0f, 0f, 0f, 1f);

    public Matrix4x4 Inverse => new(
        1f, 1f, 1f, 0f,
        0f, -2f * Weights.Z * (1f - Weights.Z) / Weights.Y, 2f * (1f - Weights.Z), 0f,
        2f * (1f - Weights.X), -2f * Weights.X * (1f - Weights.X) / Weights.Y, 0f, 0f,
        0f, 0f, 0f, 1f);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScopeRequest {
    private ScopeRequest(Gamut gamut) => Gamut = gamut;

    public Gamut Gamut { get; }

    public abstract ScopeReading Read(PixelFrame frame);

    public sealed record Histogram(Gamut Gamut, Option<ClipLevels> Clipping) : ScopeRequest(Gamut) {
        public override ScopeReading.Histogram Read(PixelFrame frame) =>
            BandFold.Run(frame, new HistogramFold(Gamut.Luminance, Clipping)) switch {
                var counts => new ScopeReading.Histogram(
                    this,
                    Traces.Of(counts, HistogramAxis.Bins, 1),
                    Clipping.Map(_ => Clips.Of(counts.AsSpan(4 * HistogramAxis.Bins)))),
            };
    }

    public sealed record Waveform(Gamut Gamut, PixelExtent Plot) : ScopeRequest(Gamut) {
        public override ScopeReading.Waveform Read(PixelFrame frame) =>
            int.Min(Plot.Width, frame.Size.Width) switch {
                var columns => new ScopeReading.Waveform(
                    this,
                    Traces.Of(BandFold.Run(frame, new WaveformFold(Gamut.Luminance, columns, Plot.Height, frame.Size.Width)), columns, Plot.Height)),
            };
    }

    public sealed record Vectorscope(Gamut Gamut, PixelExtent Plot, ChromaZoom Zoom) : ScopeRequest(Gamut) {
        public override ScopeReading.Vectorscope Read(PixelFrame frame) =>
            new(
                this,
                new BinGrid(Plot.Width, Plot.Height, BandFold.Run(frame, new VectorscopeFold(new ChromaAxes(Gamut.Luminance).Forward, Zoom.Reach, Plot.Width, Plot.Height))));
    }

    public sealed record Chromaticity(Gamut Gamut, Transfer Encoding, PixelExtent Plot) : ScopeRequest(Gamut) {
        public override ScopeReading.Chromaticity Read(PixelFrame frame) =>
            new(
                this,
                new BinGrid(Plot.Width, Plot.Height, BandFold.Run(frame, new ChromaticityFold(Gamut.ToXyz, Encoding, Plot.Width, Plot.Height))));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScopeReading {
    public sealed record Histogram(ScopeRequest.Histogram Request, Traces Bins, Option<Clips> Clipped) : ScopeReading;
    public sealed record Waveform(ScopeRequest.Waveform Request, Traces Bins) : ScopeReading;
    public sealed record Vectorscope(ScopeRequest.Vectorscope Request, BinGrid Bins) : ScopeReading;
    public sealed record Chromaticity(ScopeRequest.Chromaticity Request, BinGrid Bins) : ScopeReading;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HistogramAxis {
    private const int StopBins = 256;
    public const int TopStop = 4;

    public const int Bins = (StopBins * (TopStop + 1)) + 1;

    public static int Bin(float value) =>
        value switch {
            >= 1 << TopStop => Bins - 1,
            >= 1f => StopBins + (int)(float.Log2(value) * StopBins),
            _ => int.Clamp(int.CreateSaturating(value * StopBins), 0, StopBins - 1),
        };

    public static float LowerBound(int bin) => bin < StopBins ? (float)bin / StopBins : float.Exp2((float)(bin - StopBins) / StopBins);
}

// --- [FOLDS] ---------------------------------------------------------------------------
internal readonly struct HistogramFold(Vector3 luminance, Option<ClipLevels> clipping) : IRowFold {
    public int Bins => (4 * HistogramAxis.Bins) + (Clips.Ends * clipping.Count());

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        ReadOnlySpan<ClipLevels> clips = clipping.ToSpan();
        Span2D<int> planes = bins[..(4 * HistogramAxis.Bins)].AsSpan2D(4, HistogramAxis.Bins);
        foreach (Vector4 pixel in row) {
            Vector4 levels = pixel with { W = Vector3.Dot(luminance, pixel.AsVector3()) };
            int covered = pixel.W > 0f ? 1 : 0;
            for (int plane = 0; plane < 4; plane++)
                planes[plane, HistogramAxis.Bin(levels[plane])] += float.IsNaN(levels[plane]) ? 0 : covered;
            foreach (ClipLevels clip in clips) {
                Span<int> ends = bins.Slice(4 * HistogramAxis.Bins, Clips.Ends);
                ends[0] += pixel.X < clip.Low ? covered : 0;
                ends[1] += pixel.Y < clip.Low ? covered : 0;
                ends[2] += pixel.Z < clip.Low ? covered : 0;
                ends[3] += pixel.X >= clip.High ? covered : 0;
                ends[4] += pixel.Y >= clip.High ? covered : 0;
                ends[5] += pixel.Z >= clip.High ? covered : 0;
            }
        }
    }
}

internal readonly struct WaveformFold(Vector3 luminance, int columns, int rows, int width) : IRowFold {
    public int Bins => 4 * columns * rows;

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        Span2D<int> planes = bins.AsSpan2D(4 * rows, columns);
        for (int x = 0; x < row.Length; x++) {
            Vector4 levels = row[x] with { W = Vector3.Dot(luminance, row[x].AsVector3()) };
            (int covered, int column) = (row[x].W > 0f ? 1 : 0, (int)((long)x * columns / width));
            for (int plane = 0; plane < 4; plane++)
                planes[(plane * rows) + int.Clamp(int.CreateSaturating(levels[plane] * rows), 0, rows - 1), column] += float.IsNaN(levels[plane]) ? 0 : covered;
        }
    }
}

internal readonly struct VectorscopeFold(Matrix4x4 forward, float reach, int columns, int rows) : IRowFold {
    public int Bins => columns * rows;

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        Span2D<int> grid = bins.AsSpan2D(rows, columns);
        foreach (Vector4 pixel in row) {
            Vector4 ycc = Vector4.Transform(pixel, forward);
            (float u, float v) = ((ycc.Y + reach) / (2f * reach), (ycc.Z + reach) / (2f * reach));
            if (pixel.W > 0f && u >= 0f && u < 1f && v >= 0f && v < 1f)
                grid[(int)(v * rows), (int)(u * columns)]++;
        }
    }
}

internal readonly struct ChromaticityFold(ColorMatrix3x3 toXyz, Transfer encoding, int columns, int rows) : IRowFold {
    public int Bins => columns * rows;

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(row.Length);
        row.CopyTo(scratch.Span);
        encoding.Decode(scratch.Span, Nits.ReferenceWhite);
        Span<float> lanes = MemoryMarshal.Cast<Vector4, float>(scratch.Span);
        ImageProcessing.ApplyColorMatrix(lanes, lanes, 4, toXyz);
        Span2D<int> grid = bins.AsSpan2D(rows, columns);
        foreach (Vector4 xyz in scratch.Span) {
            float sum = xyz.X + xyz.Y + xyz.Z;
            (float x, float y) = (xyz.X / sum, xyz.Y / sum);
            if (xyz.W > 0f && sum > 0f && x >= 0f && x < 1f && y >= 0f && y < 1f)
                grid[(int)(y * rows), (int)(x * columns)]++;
        }
    }
}

// --- [BANDS] ---------------------------------------------------------------------------
internal static class BandFold {
    public static int[] Run<TFold>(PixelFrame frame, TFold fold) where TFold : struct, IRowFold {
        int bands = int.Min(frame.Size.Height, Environment.ProcessorCount);
        using MemoryOwner<int> counts = MemoryOwner<int>.Allocate(bands * fold.Bins, AllocationMode.Clear);
        ParallelHelper.For(0, bands, new BandAction<TFold>(frame.View, bands, counts.Memory, fold));
        Span<int> merged = counts.Span[..fold.Bins];
        for (int band = 1; band < bands; band++)
            TensorPrimitives.Add(merged, counts.Span.Slice(band * fold.Bins, fold.Bins), merged);
        return merged.ToArray();
    }
}

file readonly struct BandAction<TFold>(Memory2D<Vector4> rows, int bands, Memory<int> counts, TFold fold) : IAction where TFold : struct, IRowFold {
    public void Invoke(int band) {
        Span<int> bins = counts.Span.Slice(band * fold.Bins, fold.Bins);
        for (int line = (int)((long)band * rows.Height / bands); line < (int)((long)(band + 1) * rows.Height / bands); line++)
            fold.Fold(rows.Span.GetRowSpan(line), bins);
    }
}
