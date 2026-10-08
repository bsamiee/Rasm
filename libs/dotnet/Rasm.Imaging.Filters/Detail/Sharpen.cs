using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Filters.Detail;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidDetail>]
public readonly partial struct Sharpness : IMinMaxValue<Sharpness> {
    public static Sharpness MinValue => Off;
    public static Sharpness MaxValue { get; } = new(10f);

    static partial void ValidateFactoryArguments(ref InvalidDetail? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidDetail();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidDetail>]
public readonly partial struct DetailStrength : IMinMaxValue<DetailStrength> {
    public static DetailStrength MinValue { get; } = new(1f);
    public static DetailStrength MaxValue { get; } = new(2.2f);

    static partial void ValidateFactoryArguments(ref InvalidDetail? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidDetail();
}

public sealed record Sharpen(Sharpness Sharpness) : IStateRecord<Sharpen, SharpenParameter, InvalidDetail>, IPixelStage<Sharpen> {
    public static Sharpen Default { get; } = new(Sharpness.Off);

    public static Option<PixelPass> Pass(Sharpen state, PassContext context) =>
        state.Sharpness == Sharpness.Off ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(Sharpen state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (Point anchor, int width, Vector3 luminance) = (new Point(-1, -1), frame.Size.Width, context.Working.Luminance);
        using Mat header = frame.Header();
        using Mat light = new(frame.Size.Height, width, DepthType.Cv32F, 4);
        using Mat weights = new(2, 4, DepthType.Cv32F, 1);
        using Mat laplacian = new();
        using Mat luma = new();
        using Mat rise = new();
        using Mat cross = CvInvoke.GetStructuringElement(MorphShapes.Cross, new Size(3, 3), anchor);
        CvInvoke.MixChannels(header, light, [3, 0, 3, 1, 3, 2, 3, 3]);
        CvInvoke.Multiply(header, light, light);
        CvInvoke.Laplacian(light, laplacian, DepthType.Cv32F, 1, 1d, 0d, BorderType.Replicate);
        weights.SetTo([luminance.X, luminance.Y, luminance.Z, 0f, -luminance.X, -luminance.Y, -luminance.Z, 0f]);
        CvInvoke.Transform(light, luma, weights);
        CvInvoke.Dilate(luma, rise, cross, anchor, 1, BorderType.Replicate, borderValue: default);
        CvInvoke.Subtract(rise, luma, rise);
        return new PixelPass.Pointwise((row, _, line) => {
            int start = frame.Line(line) * width;
            ReadOnlySpan<Vector4> deltas = laplacian.GetSpan<Vector4>().Slice(start, width);
            ReadOnlySpan<Vector2> steps = rise.GetSpan<Vector2>().Slice(start, width);
            for (int x = 0; x < row.Length; x++)
                row[x] -= row[x].W > 0f ? new Vector4(float.Max(1f - (context.Exposure.Scale * float.Max(steps[x].X, steps[x].Y)), 0f) * state.Sharpness / 6f / row[x].W * deltas[x].AsVector3(), 0f) : Vector4.Zero;
        }).Run(frame, progress);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SharpenParameter : IStateParameter<Sharpen> {
    public static readonly SharpenParameter Sharpness = new("sharpness", new StateParameter<Sharpen>.Bounded<Sharpness, float, InvalidDetail>(
        Lens<Sharpen, Sharpness>.New(static sharpen => sharpen.Sharpness, static sharpness => sharpen => sharpen with { Sharpness = sharpness }), new()));

    public StateParameter<Sharpen> Kind { get; }
}

public sealed record LocalContrast(AxisFraction Highlights, AxisFraction Shadows, DetailStrength Detail)
    : IStateRecord<LocalContrast, LocalContrastParameter, InvalidDetail>, IPixelStage<LocalContrast> {
    public static LocalContrast Default { get; } = new(AxisFraction.MaxValue, AxisFraction.MaxValue, DetailStrength.MinValue);

    public static Option<PixelPass> Pass(LocalContrast state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(LocalContrast state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        const float LogFloor = -8f;
        const float Spacing = (HistogramAxis.TopStop - LogFloor) / (32 - 1);
        (int width, Vector3 luminance, float grey, float grid, (WrapMode, WrapMode) edge) = (frame.Size.Width, context.Working.Luminance, float.Log2(Exposure.MiddleGrey), 64f / ReferenceFrame.Height * context.Extent.ShortSide, (WrapMode.Clamp, WrapMode.Clamp));
        (int pitch, int across, int down) = (int)float.Ceiling(grid) switch { var cell => (cell, (width + cell - 1) / cell, (frame.Size.Height + cell - 1) / cell) };
        using Mat header = frame.Header();
        using Mat weights = new(1, 4, DepthType.Cv32F, 1);
        using Mat luma = new();
        using Mat alpha = new();
        using Mat mask = new();
        using ScalarArray zero = new(0d);
        weights.SetTo([luminance.X, luminance.Y, luminance.Z, 0f]);
        CvInvoke.Transform(header, luma, weights);
        CvInvoke.ExtractChannel(header, alpha, 3);
        CvInvoke.Compare(alpha, zero, mask, CmpType.GreaterThan);
        CvInvoke.MinMaxIdx(luma, out double low, out double high, minIdx: null, maxIdx: null, mask);
        (int first, int slices) = Place((float)low, 1f, 0).Slice switch { var lowest => (lowest, Place((float)high, 1f, 0).Slice - lowest + 2) };
        ((float fine, int fineTaps), (float broad, int broadTaps)) = (Spread(grid), Spread(0.5f * ReferenceFrame.GreaterSide / 6f * context.Extent.ShortSide));
        int stride = down + (fineTaps / 2);
        using Mat lattice = Mat.Zeros(slices * stride, across, DepthType.Cv32F, 4);
        using Mat cells = Mat.Zeros(down, across, DepthType.Cv32F, 4);
        ReadOnlySpan<float> lumas = luma.GetSpan<float>();
        ReadOnlySpan<float> coverage = alpha.GetSpan<float>();
        Span2D<Vector4> splat = lattice.GetSpan<Vector4>().AsSpan2D(slices * stride, across);
        Span2D<Vector4> marginals = cells.GetSpan<Vector4>().AsSpan2D(down, across);
        for (int index = 0; index < lumas.Length; index++) {
            if (coverage[index] <= 0f) continue;
            ((Vector4 sample, int slice, float upper), int cellY, int cellX) = (Place(lumas[index], coverage[index], first), index / width / pitch, index % width / pitch);
            splat[(slice * stride) + cellY, cellX] += (1f - upper) * sample;
            splat[((slice + 1) * stride) + cellY, cellX] += upper * sample;
            marginals[cellY, cellX] += sample;
        }
        CvInvoke.GaussianBlur(lattice, lattice, new Size(fineTaps, fineTaps), fine, fine, BorderType.Constant);
        CvInvoke.GaussianBlur(cells, cells, new Size(broadTaps, broadTaps), broad, broad, BorderType.Constant);
        return new PixelPass.Pointwise((row, _, line) => {
            int y = frame.Line(line);
            ReadOnlySpan<float> rowLumas = luma.GetSpan<float>().Slice(y * width, width);
            ReadOnlySpan2D<Vector4> bilateral = lattice.GetSpan<Vector4>().AsSpan2D(slices * stride, across);
            ReadOnlySpan2D<Vector4> blurred = cells.GetSpan<Vector4>().AsSpan2D(down, across);
            for (int x = 0; x < row.Length; x++) {
                if (row[x].W <= 0f) continue;
                ((Vector4 own, int slice, float upper), Vector2 at) = (Place(rowLumas[x], 1f, first), new Vector2(x + 0.5f, y + 0.5f) / pitch);
                Vector4 graded = Vector4.Lerp(bilateral.Slice(slice * stride, 0, down, across).Sample(at, edge), bilateral.Slice((slice + 1) * stride, 0, down, across).Sample(at, edge), upper);
                float surround = blurred.Sample(at, edge) switch { var smooth => smooth.X / smooth.Y };
                float centered = float.Lerp(graded.Y > 0f ? graded.X / graded.Y : surround, surround, 0.6f) - grey;
                row[x] *= new Vector4(new Vector3(float.Exp2(grey + (centered * (centered > 0f ? state.Highlights : state.Shadows)) + ((own.X - grey - centered) * state.Detail) - own.X)), 1f);
            }
        }).Run(frame, progress);

        static (Vector4 Sample, int Slice, float Upper) Place(float luma, float weight, int first) =>
            float.Log2(float.Max(luma, LuminanceSamples.Floor)) switch {
                var level => ((level - LogFloor) / Spacing) switch { var stop => (int)float.Floor(stop) switch { var lower => (weight * new Vector4(level, 1f, 0f, 0f), lower - first, stop - lower) } },
            };

        (float Sigma, int Taps) Spread(float sigma) =>
            (float.Sqrt(float.Max((sigma * sigma) - (pitch * pitch / 4f), 0f)) / pitch) switch { var spread => (spread, PixelSampling.GaussianTaps(spread)) };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LocalContrastParameter : IStateParameter<LocalContrast> {
    public static readonly LocalContrastParameter Highlights = new("highlights", new StateParameter<LocalContrast>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<LocalContrast, AxisFraction>.New(static contrast => contrast.Highlights, static highlights => contrast => contrast with { Highlights = highlights }), new()));
    public static readonly LocalContrastParameter Shadows = new("shadows", new StateParameter<LocalContrast>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<LocalContrast, AxisFraction>.New(static contrast => contrast.Shadows, static shadows => contrast => contrast with { Shadows = shadows }), new()));
    public static readonly LocalContrastParameter Detail = new("detail", new StateParameter<LocalContrast>.Bounded<DetailStrength, float, InvalidDetail>(
        Lens<LocalContrast, DetailStrength>.New(static contrast => contrast.Detail, static detail => contrast => contrast with { Detail = detail }), new()));

    public StateParameter<LocalContrast> Kind { get; }
}
