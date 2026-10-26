using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Pixels;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IRowFold {
    public int Bins { get; }
    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ClipLevels(float Low, float High);

[SmartEnum]
public sealed partial class ClipEnd {
    public static readonly ClipEnd Low = new(static (pixel, levels) => float.Min(pixel.X, float.Min(pixel.Y, pixel.Z)) < levels.Low);
    public static readonly ClipEnd High = new(static (pixel, levels) => float.Max(pixel.X, float.Max(pixel.Y, pixel.Z)) >= levels.High);

    [UseDelegateFromConstructor]
    public partial bool Clips(Vector4 pixel, ClipLevels levels);
}

[SmartEnum]
public sealed partial class TracePlane {
    public static readonly TracePlane Luma = new(CurveChannel.Master, Option<Lens<Vector3, float>>.None, static traces => traces.Luma);
    public static readonly TracePlane Red = new(CurveChannel.Red, Some(Lens<Vector3, float>.New(static rgb => rgb.X, static x => rgb => rgb with { X = x })), static traces => traces.Red);
    public static readonly TracePlane Green = new(CurveChannel.Green, Some(Lens<Vector3, float>.New(static rgb => rgb.Y, static y => rgb => rgb with { Y = y })), static traces => traces.Green);
    public static readonly TracePlane Blue = new(CurveChannel.Blue, Some(Lens<Vector3, float>.New(static rgb => rgb.Z, static z => rgb => rgb with { Z = z })), static traces => traces.Blue);

    public CurveChannel Curve { get; }
    public Option<Lens<Vector3, float>> Lane { get; }
    public Vector3 Color => Lane.Map(static lane => lane.Set(1f, Vector3.Zero)).IfNone(Vector3.One);

    [UseDelegateFromConstructor]
    public partial BinGrid Bins(Traces traces);
}

[ComplexValueObject]
public readonly partial struct BinRange {
    public int First { get; }
    public int Last { get; }

    public bool Holds(double level) => !double.IsNaN(level) && HistogramAxis.Bin(level) is var bin && bin >= First && bin <= Last;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int first, ref int last) =>
        validationError = 0 <= first && first <= last && last < HistogramAxis.Bins ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"Bin range [{first}, {last}] must ascend within [0, {HistogramAxis.Bins - 1}]"));
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

public readonly record struct Clips(Channels<int> Low, Channels<int> High) {
    internal const int Ends = 6;

    internal static Clips Of(ReadOnlySpan<int> counts) =>
        counts[..Ends] switch {
            var ends => new(new(ends[0], ends[1], ends[2]), new(ends[3], ends[4], ends[5])),
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScopeRequest {
    private ScopeRequest(Gamut gamut) => Gamut = gamut;

    public Gamut Gamut { get; }

    public abstract ScopeReading Read(PixelFrame frame);

    public sealed record Histogram(Gamut Gamut, Option<ClipLevels> Clipping, Option<(PixelFrame Frame, Gamut Gamut)> Input) : ScopeRequest(Gamut) {
        public override ScopeReading.Histogram Read(PixelFrame frame) =>
            BandFold.Run(frame, new HistogramFold(Gamut.Luminance, Clipping)) switch {
                var counts => new ScopeReading.Histogram(
                    this,
                    Traces.Of(counts, HistogramAxis.Bins, 1),
                    Clipping.Map(_ => Clips.Of(counts.AsSpan(4 * HistogramAxis.Bins))),
                    Input.Map(static input => Traces.Of(BandFold.Run(input.Frame, new HistogramFold(input.Gamut.Luminance, None)), HistogramAxis.Bins, 1))),
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

    public sealed record Vectorscope(Gamut Gamut, PixelExtent Plot, PositiveMagnitude Magnification) : ScopeRequest(Gamut) {
        public float Reach => 0.5f / Magnification;

        public override ScopeReading.Vectorscope Read(PixelFrame frame) =>
            new(
                this,
                new BinGrid(Plot.Width, Plot.Height, BandFold.Run(frame, new VectorscopeFold(Gamut.ToYpbpr(Gamut.Ybr), Reach, Plot.Width, Plot.Height))));
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
    public sealed record Histogram(ScopeRequest.Histogram Request, Traces Bins, Option<Clips> Clipped, Option<Traces> Input) : ScopeReading {
        public Option<double> Below(int bin) =>
            (TensorPrimitives.Sum(Bins.Luma.Row(0)[..(bin + 1)]), TensorPrimitives.Sum(Bins.Luma.Row(0))) switch {
                (_, 0) => None,
                var (below, covered) => Some((double)below / covered),
            };
    }
    public sealed record Waveform(ScopeRequest.Waveform Request, Traces Bins) : ScopeReading;
    public sealed record Vectorscope(ScopeRequest.Vectorscope Request, BinGrid Bins) : ScopeReading;
    public sealed record Chromaticity(ScopeRequest.Chromaticity Request, BinGrid Bins) : ScopeReading;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HistogramAxis {
    private const int StopBins = 256;
    public const int TopStop = 4;

    public const int Bins = (StopBins * (TopStop + 1)) + 1;

    public static Seq<float> Ticks { get; } = toSeq(Range(1, 4)).Map(static quarter => quarter / 4f) + toSeq(Range(1, TopStop)).Map(static stop => float.Exp2(stop));

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

internal static class Lanes {
    public static (Vector256<double> Red, Vector256<double> Green, Vector256<double> Blue) Of(Matrix<double> matrix) =>
        (Lane(matrix, 0), Lane(matrix, 1), Lane(matrix, 2));

    private static Vector256<double> Lane(Matrix<double> matrix, int column) => Vector256.Create(matrix[0, column], matrix[1, column], matrix[2, column], 0d);
}

internal readonly struct VectorscopeFold(Matrix<double> forward, float reach, int columns, int rows) : IRowFold {
    private readonly (Vector256<double> Red, Vector256<double> Green, Vector256<double> Blue) _lanes = Lanes.Of(forward);

    public int Bins => columns * rows;

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        Span2D<int> grid = bins.AsSpan2D(rows, columns);
        foreach (Vector4 pixel in row) {
            Vector256<double> ycc = (_lanes.Red * pixel.X) + (_lanes.Green * pixel.Y) + (_lanes.Blue * pixel.Z);
            (double u, double v) = ((ycc[1] + reach) / (2d * reach), (ycc[2] + reach) / (2d * reach));
            if (pixel.W > 0f && u >= 0f && u < 1f && v >= 0f && v < 1f)
                grid[(int)(v * rows), (int)(u * columns)]++;
        }
    }
}

internal readonly struct ChromaticityFold(Matrix<double> toXyz, Transfer encoding, int columns, int rows) : IRowFold {
    private readonly (Vector256<double> Red, Vector256<double> Green, Vector256<double> Blue) _lanes = Lanes.Of(toXyz);

    public int Bins => columns * rows;

    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins) {
        using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(row.Length);
        row.CopyTo(scratch.Span);
        encoding.Decode(scratch.Span, Nits.ReferenceWhite);
        Span2D<int> grid = bins.AsSpan2D(rows, columns);
        foreach (Vector4 rgb in scratch.Span) {
            Vector256<double> xyz = (_lanes.Red * rgb.X) + (_lanes.Green * rgb.Y) + (_lanes.Blue * rgb.Z);
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
