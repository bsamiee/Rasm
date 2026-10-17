using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Rasm.Imaging.Color;
using Rasm.Imaging.Tone;

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
public readonly partial struct BinRange {
    public int First { get; }
    public int Last { get; }

    public bool Holds(double level) => !double.IsNaN(level) && HistogramAxis.Bin(level) is var bin && bin >= First && bin <= Last;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int first, ref int last) =>
        validationError = 0 <= first && first <= last && last < HistogramAxis.Bins ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"bin range [{first}, {last}] must ascend within [0, {HistogramAxis.Bins - 1}]"));
}

[SmartEnum<int>]
public sealed partial class ChromaZoom {
    public static readonly ChromaZoom Normal = new(1);
    public static readonly ChromaZoom Doubled = new(2);

    public float Reach => 0.5f / Key;
}

[SmartEnum<int>]
public sealed partial class SampleSquare {
    public static readonly SampleSquare One = new(1);
    public static readonly SampleSquare Three = new(3);
    public static readonly SampleSquare Five = new(5);
    public static readonly SampleSquare Seven = new(7);

    public Option<Vector4> Mean(PixelFrame frame, (double X, double Y) pixel) {
        (double x, double y) = (double.Floor(pixel.X - frame.Origin.X) - (Key / 2), double.Floor(pixel.Y - frame.Origin.Y) - (Key / 2));
        (double left, double top, double right, double bottom) =
            (double.Max(x, 0d), double.Max(y, 0d), double.Min(x + Key, frame.Size.Width), double.Min(y + Key, frame.Size.Height));
        (Vector256<double> sum, int count) = (Vector256<double>.Zero, 0);
        if (right > left && bottom > top)
            foreach (Vector4 sample in frame.View.Span.Slice((int)top, (int)left, (int)(bottom - top), (int)(right - left)))
                (sum, count) = sample.W > 0f ? (sum + Vector256.Create(sample.X, sample.Y, sample.Z, sample.W), count + 1) : (sum, count);
        return count > 0 ? Some((sum / count) switch { var mean => new Vector4((float)mean[0], (float)mean[1], (float)mean[2], (float)mean[3]) }) : None;
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

public readonly record struct ChromaAxes((double X, double Y, double Z) Weights) {
    public Matrix<double> Forward => Matrix<double>.Build.DenseOfRowMajor(3, 3, [
        Weights.X, -Weights.X / (2d * (1d - Weights.Z)), 0.5d,
        Weights.Y, -Weights.Y / (2d * (1d - Weights.Z)), -Weights.Y / (2d * (1d - Weights.X)),
        Weights.Z, 0.5d, -Weights.Z / (2d * (1d - Weights.X)),
    ]);

    public Matrix<double> Inverse => Matrix<double>.Build.DenseOfRowMajor(3, 3, [
        1d, 1d, 1d,
        0d, -2d * Weights.Z * (1d - Weights.Z) / Weights.Y, 2d * (1d - Weights.Z),
        2d * (1d - Weights.X), -2d * Weights.X * (1d - Weights.X) / Weights.Y, 0d,
    ]);
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

    public static int Bin(double value) =>
        value switch {
            >= 1 << TopStop => Bins - 1,
            >= 1d => StopBins + (int)(double.Log2(value) * StopBins),
            _ => int.Clamp(int.CreateSaturating(value * StopBins), 0, StopBins - 1),
        };

    public static float LowerBound(int bin) => bin < StopBins ? (float)bin / StopBins : float.Exp2((float)(bin - StopBins) / StopBins);
}

// --- [FOLDS] ---------------------------------------------------------------------------
internal readonly struct HistogramFold((double X, double Y, double Z) luminance, Option<ClipLevels> clipping) : IRowFold {
    public int Bins => (4 * HistogramAxis.Bins) + (Clips.Ends * clipping.Count());

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        ReadOnlySpan<ClipLevels> clips = clipping.ToSpan();
        Span2D<int> planes = bins[..(4 * HistogramAxis.Bins)].AsSpan2D(4, HistogramAxis.Bins);
        foreach (Vector4 pixel in row) {
            Vector256<double> levels = Vector256.Create(pixel.X, pixel.Y, pixel.Z, LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, luminance));
            int covered = pixel.W > 0f ? 1 : 0;
            for (int plane = 0; plane < 4; plane++)
                planes[plane, HistogramAxis.Bin(levels[plane])] += double.IsNaN(levels[plane]) ? 0 : covered;
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

internal readonly struct WaveformFold((double X, double Y, double Z) luminance, int columns, int rows, int width) : IRowFold {
    public int Bins => 4 * columns * rows;

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        Span2D<int> planes = bins.AsSpan2D(4 * rows, columns);
        for (int x = 0; x < row.Length; x++) {
            Vector4 pixel = row[x];
            Vector256<double> levels = Vector256.Create(pixel.X, pixel.Y, pixel.Z, LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, luminance));
            (int covered, int column) = (row[x].W > 0f ? 1 : 0, (int)((long)x * columns / width));
            for (int plane = 0; plane < 4; plane++)
                planes[(plane * rows) + int.Clamp(int.CreateSaturating(levels[plane] * rows), 0, rows - 1), column] += double.IsNaN(levels[plane]) ? 0 : covered;
        }
    }
}

internal readonly struct VectorscopeFold(Matrix<double> forward, float reach, int columns, int rows) : IRowFold {
    public int Bins => columns * rows;

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        Span2D<int> grid = bins.AsSpan2D(rows, columns);
        DenseVector source = new(3);
        DenseVector ycc = new(3);
        foreach (Vector4 pixel in row) {
            (source[0], source[1], source[2]) = (pixel.X, pixel.Y, pixel.Z);
            forward.LeftMultiply(source, ycc);
            (double u, double v) = ((ycc[1] + reach) / (2d * reach), (ycc[2] + reach) / (2d * reach));
            if (pixel.W > 0f && u >= 0f && u < 1f && v >= 0f && v < 1f)
                grid[(int)(v * rows), (int)(u * columns)]++;
        }
    }
}

internal readonly struct ChromaticityFold(Matrix<double> toXyz, Transfer encoding, int columns, int rows) : IRowFold {
    private readonly Vector256<double> _red = Vector256.Create(toXyz[0, 0], toXyz[1, 0], toXyz[2, 0], 0d);
    private readonly Vector256<double> _green = Vector256.Create(toXyz[0, 1], toXyz[1, 1], toXyz[2, 1], 0d);
    private readonly Vector256<double> _blue = Vector256.Create(toXyz[0, 2], toXyz[1, 2], toXyz[2, 2], 0d);

    public int Bins => columns * rows;

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(row.Length);
        row.CopyTo(scratch.Span);
        encoding.Decode(scratch.Span, Nits.ReferenceWhite);
        Span2D<int> grid = bins.AsSpan2D(rows, columns);
        foreach (Vector4 rgb in scratch.Span) {
            Vector256<double> xyz = (_red * rgb.X) + (_green * rgb.Y) + (_blue * rgb.Z);
            double sum = Vector256.Sum(xyz);
            (double x, double y) = (xyz[0] / sum, xyz[1] / sum);
            if (rgb.W > 0f && sum > 0d && x >= 0d && x < 1d && y >= 0d && y < 1d)
                grid[(int)(y * rows), (int)(x * columns)]++;
        }
    }
}

// --- [BANDS] ---------------------------------------------------------------------------
internal static class BandFold {
    public static int[] Run<TFold>(PixelFrame frame, TFold fold) where TFold : struct, IRowFold {
        int bands = int.Min(frame.Size.Height, Environment.ProcessorCount);
        int[][] counts = new int[bands][];
        ParallelHelper.For(0, bands, new BandAction<TFold>(frame.View, counts, fold));
        Span<int> merged = counts[0];
        for (int band = 1; band < bands; band++)
            TensorPrimitives.Add(merged, counts[band], merged);
        return counts[0];
    }
}

file readonly struct BandAction<TFold>(Memory2D<Vector4> rows, int[][] counts, TFold fold) : IAction where TFold : struct, IRowFold {
    public void Invoke(int i) {
        int[] bins = new int[fold.Bins];
        counts[i] = bins;
        for (int line = (int)((long)i * rows.Height / counts.Length); line < (int)((long)(i + 1) * rows.Height / counts.Length); line++)
            fold.Fold(rows.Span.GetRowSpan(line), bins);
    }
}
