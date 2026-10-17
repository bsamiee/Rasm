using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Filters.Detail;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DetailStrength : IMinMaxValue<DetailStrength> {
    public static DetailStrength MinValue { get; } = new(1f);
    public static DetailStrength MaxValue { get; } = new(2.2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record Sharpen(Steepness Sharpness) : IStateRecord<Sharpen, SharpenParameter>, IPixelStage<Sharpen> {
    public static Sharpen Default { get; } = new(Steepness.Zero);

    public static Option<PixelPass> Pass(Sharpen state, PassContext context) =>
        state.Sharpness == Steepness.Zero ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static unsafe Fin<Unit> Kernel(Sharpen state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
        (int width, (double X, double Y, double Z) luminance) = (frame.Size.Width, context.Working.Luminance);
        using Mat linear = new();
        using Mat light = new(frame.Size.Height, width, DepthType.Cv64F, 4);
        using Mat weights = new(2, 4, DepthType.Cv64F, 1);
        using Mat laplacian = new();
        using Mat luma = new();
        using Mat rise = new();
        using Mat cross = CvInvoke.GetStructuringElement(MorphShapes.Cross, new Size(3, 3), new Point(-1, -1));
        header.ConvertTo(linear, DepthType.Cv64F);
        CvInvoke.MixChannels(linear, light, [3, 0, 3, 1, 3, 2, 3, 3]);
        CvInvoke.Multiply(linear, light, light);
        CvInvoke.Laplacian(light, laplacian, DepthType.Cv64F, 1, 1d, 0d, BorderType.Replicate);
        weights.SetTo([luminance.X, luminance.Y, luminance.Z, 0d, -luminance.X, -luminance.Y, -luminance.Z, 0d]);
        CvInvoke.Transform(light, luma, weights);
        CvInvoke.Dilate(luma, rise, cross, new Point(-1, -1), 1, BorderType.Replicate, default);
        CvInvoke.Subtract(rise, luma, rise);
        return new PixelPass.Pointwise((row, _, line) => {
            int start = frame.RowIndex(line) * width;
            ReadOnlySpan<Vector256<double>> deltas = new Span<Vector256<double>>(laplacian.DataPointer.ToPointer(), laplacian.Rows * laplacian.Cols).Slice(start, width);
            ReadOnlySpan<Vector128<double>> steps = new Span<Vector128<double>>(rise.DataPointer.ToPointer(), rise.Rows * rise.Cols).Slice(start, width);
            for (int x = 0; x < row.Length; x++) {
                if (row[x].W <= 0f) continue;
                double factor = double.Max(1d - double.Exp2(context.Exposure + double.Log2(double.Max(steps[x].GetElement(0), steps[x].GetElement(1)))), 0d) * state.Sharpness / 6d / row[x].W;
                row[x] = new((float)(row[x].X - (factor * deltas[x].GetElement(0))), (float)(row[x].Y - (factor * deltas[x].GetElement(1))), (float)(row[x].Z - (factor * deltas[x].GetElement(2))), row[x].W);
            }
        }).Run(frame, progress);
    });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SharpenParameter : IStateParameter<Sharpen> {
    public static readonly SharpenParameter Sharpness = new("sharpness", StateParameter.Bounded(Lens<Sharpen, Steepness>.New(static sharpen => sharpen.Sharpness, static sharpness => sharpen => sharpen with { Sharpness = sharpness }), new Presentation<float>()),
        new("Sharpness", "Gain of each pixel's difference from its four axis neighbors, none across an edge whose exposed luminance step reaches 1"));

    public StateParameter<Sharpen> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record LocalContrast(AxisFraction Highlights, AxisFraction Shadows, DetailStrength Detail)
    : IStateRecord<LocalContrast, LocalContrastParameter>, IPixelStage<LocalContrast> {
    public static LocalContrast Default { get; } = new(AxisFraction.MaxValue, AxisFraction.MaxValue, DetailStrength.MinValue);

    public static Option<PixelPass> Pass(LocalContrast state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(LocalContrast state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        double grid = 64d / ReferenceFrame.Height * context.Extent.ShortSide;
        int pitch = (int)double.Ceiling(grid);
        return (Spread(grid), Spread(0.5d * ReferenceFrame.GreaterSide / 6d * context.Extent.ShortSide))
            .Apply((fine, broad) => Filter(state, context, frame, progress, pitch, fine, broad)).As().Flatten();

        Fin<(double Sigma, int Taps)> Spread(double sigma) =>
            (double.Sqrt(double.Max((sigma * sigma) - ((double)pitch * pitch / 4d), 0d)) / pitch) switch { var spread => PixelSampling.GaussianTaps<int>(spread).Map(taps => (spread, taps)) };
    }

    private static unsafe Fin<Unit> Filter(LocalContrast state, PassContext context, PixelFrame frame, IProgress<int> progress, int pitch, (double Sigma, int Taps) fine, (double Sigma, int Taps) broad) {
        const float LogFloor = -8f;
        const float Spacing = (HistogramAxis.TopStop - LogFloor) / (32 - 1);
        return frame.Filter(header => {
            (int width, (double X, double Y, double Z) luminance, double grey, (WrapMode, WrapMode) edge) = (frame.Size.Width, context.Working.Luminance, double.Log2(Exposure.MiddleGrey), (WrapMode.Clamp, WrapMode.Clamp));
            (int across, int down) = ((width + pitch - 1) / pitch, (frame.Size.Height + pitch - 1) / pitch);
            using Mat linear = new();
            using Mat weights = new(1, 4, DepthType.Cv64F, 1);
            using Mat luma = new();
            using Mat alpha = new();
            using Mat mask = new();
            using ScalarArray zero = new(0d);
            weights.SetTo([luminance.X, luminance.Y, luminance.Z, 0d]);
            header.ConvertTo(linear, DepthType.Cv64F);
            CvInvoke.Transform(linear, luma, weights);
            CvInvoke.ExtractChannel(linear, alpha, 3);
            CvInvoke.Compare(alpha, zero, mask, CmpType.GreaterThan);
            CvInvoke.MinMaxIdx(luma, out double low, out double high, minIdx: null, maxIdx: null, mask);
            (int first, int slices) = Place(low, 1d, 0).Slice switch { var lowest => (lowest, Place(high, 1d, 0).Slice - lowest + 2) };
            int cellCount = down * across;
            using VectorOfMat lattice = new(slices);
            Mat[] planes = [.. Enumerable.Range(0, slices).Select(index => lattice[index])];
            foreach (Mat plane in planes) {
                plane.Create(down, across, DepthType.Cv64F, 4);
                new Span<Vector256<double>>(plane.DataPointer.ToPointer(), cellCount).Clear();
            }
            using Mat cells = Mat.Zeros(down, across, DepthType.Cv64F, 4);
            ReadOnlySpan<double> lumas = new(luma.DataPointer.ToPointer(), luma.Rows * luma.Cols);
            ReadOnlySpan<double> coverage = new(alpha.DataPointer.ToPointer(), alpha.Rows * alpha.Cols);
            Span2D<Vector256<double>> marginals = new Span<Vector256<double>>(cells.DataPointer.ToPointer(), cellCount).AsSpan2D(down, across);
            for (int index = 0; index < lumas.Length; index++) {
                if (coverage[index] <= 0f) continue;
                ((Vector256<double> sample, int slice, double upper), int cellY, int cellX) = (Place(lumas[index], coverage[index], first), index / width / pitch, index % width / pitch);
                new Span<Vector256<double>>(planes[slice].DataPointer.ToPointer(), cellCount)[(cellY * across) + cellX] += (1d - upper) * sample;
                new Span<Vector256<double>>(planes[slice + 1].DataPointer.ToPointer(), cellCount)[(cellY * across) + cellX] += upper * sample;
                marginals[cellY, cellX] += sample;
            }
            foreach (Mat plane in planes)
                CvInvoke.GaussianBlur(plane, plane, new Size(fine.Taps, fine.Taps), fine.Sigma, fine.Sigma, BorderType.Constant);
            CvInvoke.GaussianBlur(cells, cells, new Size(broad.Taps, broad.Taps), broad.Sigma, broad.Sigma, BorderType.Constant);
            return new PixelPass.Pointwise((row, _, line) => {
                int y = frame.RowIndex(line);
                ReadOnlySpan<double> rowLumas = new Span<double>(luma.DataPointer.ToPointer(), luma.Rows * luma.Cols).Slice(y * width, width);
                ReadOnlySpan2D<Vector256<double>> blurred = new Span<Vector256<double>>(cells.DataPointer.ToPointer(), cellCount).AsSpan2D(down, across);
                for (int x = 0; x < row.Length; x++) {
                    if (row[x].W <= 0f) continue;
                    ((Vector256<double> own, int slice, double upper), (double X, double Y) at) = (Place(rowLumas[x], 1d, first), ((x + 0.5d) / pitch, (y + 0.5d) / pitch));
                    Vector256<double> graded = ((1d - upper) * new ReadOnlySpan<Vector256<double>>(planes[slice].DataPointer.ToPointer(), cellCount).AsSpan2D(down, across).Sample(at, edge, Vector256<double>.Zero, static (sum, pixel, weight) => sum + (pixel * weight)))
                    + (upper * new ReadOnlySpan<Vector256<double>>(planes[slice + 1].DataPointer.ToPointer(), cellCount).AsSpan2D(down, across).Sample(at, edge, Vector256<double>.Zero, static (sum, pixel, weight) => sum + (pixel * weight)));
                    double surround = blurred.Sample(at, edge, Vector256<double>.Zero, static (sum, pixel, weight) => sum + (pixel * weight)) switch { var smooth => smooth.GetElement(0) / smooth.GetElement(1) };
                    double centered = double.Lerp(graded.GetElement(1) > 0d ? graded.GetElement(0) / graded.GetElement(1) : surround, surround, 0.6d) - grey;
                    double gain = double.Exp2(grey + (centered * (centered > 0d ? state.Highlights : state.Shadows)) + ((own.GetElement(0) - grey - centered) * state.Detail) - own.GetElement(0));
                    row[x] = new((float)(row[x].X * gain), (float)(row[x].Y * gain), (float)(row[x].Z * gain), row[x].W);
                }
            }).Run(frame, progress);
        });

        static (Vector256<double> Sample, int Slice, double Upper) Place(double luma, double weight, int first) =>
            double.Log2(double.Max(luma, LuminanceSamples.Floor)) switch {
                var level => ((level - LogFloor) / Spacing) switch { var stop => (int)double.Floor(stop) switch { var lower => (weight * Vector256.Create(level, 1d, 0d, 0d), lower - first, stop - lower) } },
            };

    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LocalContrastParameter : IStateParameter<LocalContrast> {
    public static readonly LocalContrastParameter Highlights = new("highlights", StateParameter.Bounded(Lens<LocalContrast, AxisFraction>.New(static contrast => contrast.Highlights, static highlights => contrast => contrast with { Highlights = highlights })),
        new("Highlights", "Base contrast above middle gray, 100% unchanged and 0% flat"));
    public static readonly LocalContrastParameter Shadows = new("shadows", StateParameter.Bounded(Lens<LocalContrast, AxisFraction>.New(static contrast => contrast.Shadows, static shadows => contrast => contrast with { Shadows = shadows })),
        new("Shadows", "Base contrast below middle gray, 100% unchanged and 0% flat"));
    public static readonly LocalContrastParameter Detail = new("detail", StateParameter.Bounded(Lens<LocalContrast, DetailStrength>.New(static contrast => contrast.Detail, static detail => contrast => contrast with { Detail = detail }), new Presentation<float>()),
        new("Clarity", "Detail contrast about the base, 1 unchanged"));

    public StateParameter<LocalContrast> Kind { get; }
    public ParameterText Text { get; }
}
