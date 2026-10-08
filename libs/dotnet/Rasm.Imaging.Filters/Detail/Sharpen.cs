using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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
public readonly partial struct ContrastScale : IMinMaxValue<ContrastScale> {
    public static ContrastScale MinValue { get; } = new(0f);
    public static ContrastScale MaxValue { get; } = new(1f);

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
        (Vector3 luminance, Point anchor) = (context.Working.Luminance, new Point(-1, -1));
        using Mat header = frame.Header();
        using Mat weights = new(1, 4, DepthType.Cv32F, 1);
        using Mat laplacian = new();
        using Mat luma = new();
        using Mat dilation = new();
        using Mat erosion = new();
        using Mat step = new();
        using Mat zero = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
        using Mat cross = CvInvoke.GetStructuringElement(MorphShapes.Cross, new Size(3, 3), anchor);
        weights.SetTo([luminance.X, luminance.Y, luminance.Z, 0f]);
        CvInvoke.Laplacian(header, laplacian, DepthType.Cv32F, 1, 1d, 0d, BorderType.Replicate);
        CvInvoke.Transform(header, luma, weights);
        CvInvoke.Dilate(luma, dilation, cross, anchor, 1, BorderType.Replicate, new MCvScalar());
        CvInvoke.Erode(luma, erosion, cross, anchor, 1, BorderType.Replicate, new MCvScalar());
        CvInvoke.Subtract(dilation, luma, dilation);
        CvInvoke.Subtract(luma, erosion, erosion);
        CvInvoke.Max(dilation, erosion, step);
        step.ConvertTo(step, DepthType.Cv32F, -context.Exposure.Scale, 1d);
        CvInvoke.Max(step, zero, step);
        using Mat gate = new();
        using VectorOfMat planes = new(step, step, step, zero);
        CvInvoke.Merge(planes, gate);
        CvInvoke.Multiply(laplacian, gate, laplacian, state.Sharpness / 6f);
        CvInvoke.Subtract(header, laplacian, header);
        progress.Report(frame.Size.Height);
        return unit;
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

public sealed record LocalContrast(ContrastScale Highlights, ContrastScale Shadows, DetailStrength Detail)
    : IStateRecord<LocalContrast, LocalContrastParameter, InvalidDetail>, IPixelStage<LocalContrast> {
    public static LocalContrast Default { get; } = new(ContrastScale.MaxValue, ContrastScale.MaxValue, DetailStrength.MinValue);

    private static ShortSideLength Cell { get; } = ShortSideLength.Create(1920f / 64f / 1080f);
    private static ShortSideLength Blur { get; } = ShortSideLength.Create(0.5f * 1920f / 1080f);

    public static Option<PixelPass> Pass(LocalContrast state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(LocalContrast state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        const int Slices = 32;
        (int height, int width, Vector3 luminance) = (frame.Size.Height, frame.Size.Width, context.Working.Luminance);
        int cell = (int)float.Ceiling(Cell.Pixels(context.Extent));
        (int across, int down) = ((width + cell - 1) / cell, (height + cell - 1) / cell);
        float sigma = Blur.Pixels(context.Extent) / 6f / cell;
        int taps = (2 * (int)float.Ceiling(3f * sigma)) + 1;
        (float highlights, float shadows, float detail, float grey, Vector2 limit) = (state.Highlights, state.Shadows, state.Detail, float.Log2(Exposure.MiddleGrey), new Vector2(across - 1, down - 1));
        using Mat header = frame.Header();
        using Mat weights = new(1, 4, DepthType.Cv32F, 1);
        using Mat log = new();
        using Mat alpha = new();
        using Mat mask = new();
        using Mat grid = Mat.Zeros(down, Slices * across, DepthType.Cv32F, 2);
        using Mat cells = Mat.Zeros(down, across, DepthType.Cv32F, 2);
        using ScalarArray floor = new(LuminanceSamples.Floor);
        using ScalarArray zero = new(0d);
        weights.SetTo([luminance.X, luminance.Y, luminance.Z, 0f]);
        CvInvoke.Transform(header, log, weights);
        CvInvoke.Max(log, floor, log);
        CvInvoke.Log(log, log);
        log.ConvertTo(log, DepthType.Cv32F, double.Log2(double.E));
        CvInvoke.ExtractChannel(header, alpha, 3);
        CvInvoke.Compare(alpha, zero, mask, CmpType.GreaterThan);
        (double low, double high, Point lowAt, Point highAt) = (0d, 0d, Point.Empty, Point.Empty);
        CvInvoke.MinMaxLoc(log, ref low, ref high, ref lowAt, ref highAt, mask);
        (float least, float depth) = ((float)low, high > low ? (Slices - 1) / (float)(high - low) : 0f);
        ReadOnlySpan2D<float> levels = log.GetSpan<float>().AsSpan2D(height, width);
        ReadOnlySpan2D<byte> covered = mask.GetSpan<byte>().AsSpan2D(height, width);
        Span2D<Vector2> lattice = grid.GetSpan<Vector2>().AsSpan2D(down, Slices * across);
        Span2D<Vector2> marginals = cells.GetSpan<Vector2>().AsSpan2D(down, across);
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (covered[y, x] != 0) {
                    ((int slice, float upper), Vector2 sample) = (Slice((levels[y, x] - least) * depth), new Vector2(levels[y, x], 1f));
                    lattice[y / cell, (Slices * (x / cell)) + slice] += (1f - upper) * sample;
                    lattice[y / cell, (Slices * (x / cell)) + slice + 1] += upper * sample;
                    marginals[y / cell, x / cell] += sample;
                }
            }
        }
        CvInvoke.GaussianBlur(cells, cells, new Size(taps, taps), sigma, sigma, BorderType.Constant);
        return new PixelPass.Pointwise((row, _, line) => {
            int y = frame.Line(line);
            ReadOnlySpan<float> rowLevels = log.GetSpan<float>().Slice(y * width, width);
            ReadOnlySpan2D<Vector2> bilateral = grid.GetSpan<Vector2>().AsSpan2D(down, Slices * across);
            ReadOnlySpan2D<Vector2> blurred = cells.GetSpan<Vector2>().AsSpan2D(down, across);
            for (int x = 0; x < row.Length; x++) {
                if (row[x].W > 0f) {
                    Vector2 at = Vector2.Clamp(((new Vector2(x, y) + new Vector2(0.5f)) / cell) - new Vector2(0.5f), Vector2.Zero, limit);
                    (float level, (int slice, float upper)) = (rowLevels[x], Slice((rowLevels[x] - least) * depth));
                    (Vector2 graded, Vector2 smooth) = (Vector2.Lerp(Bilinear(bilateral, at, Slices, slice), Bilinear(bilateral, at, Slices, slice + 1), upper), Bilinear(blurred, at, 1, 0));
                    float surround = smooth.X / smooth.Y;
                    float baseline = float.Lerp(graded.Y < 0.001f ? surround : graded.X / graded.Y, surround, 0.6f);
                    float centered = baseline - grey;
                    row[x] = new Vector4(row[x].AsVector3() * float.Exp2(grey + (centered * (centered > 0f ? highlights : shadows)) + ((level - baseline) * detail) - level), row[x].W);
                }
            }
        }).Run(frame, progress);

        static (int Slice, float Upper) Slice(float position) =>
            int.Min((int)position, Slices - 2) switch {
                var slice => (slice, position - slice),
            };

        static Vector2 Bilinear(ReadOnlySpan2D<Vector2> plane, Vector2 at, int stride, int slice) =>
            (Near: Vector2.Truncate(at), Far: Vector2.Min(Vector2.Truncate(at) + Vector2.One, new Vector2((plane.Width / stride) - 1, plane.Height - 1))) switch {
                var (near, far) => Vector2.Lerp(
                    Vector2.Lerp(plane[(int)near.Y, (stride * (int)near.X) + slice], plane[(int)near.Y, (stride * (int)far.X) + slice], at.X - near.X),
                    Vector2.Lerp(plane[(int)far.Y, (stride * (int)near.X) + slice], plane[(int)far.Y, (stride * (int)far.X) + slice], at.X - near.X),
                    at.Y - near.Y),
            };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LocalContrastParameter : IStateParameter<LocalContrast> {
    public static readonly LocalContrastParameter Highlights = new("highlights", new StateParameter<LocalContrast>.Bounded<ContrastScale, float, InvalidDetail>(
        Lens<LocalContrast, ContrastScale>.New(static contrast => contrast.Highlights, static highlights => contrast => contrast with { Highlights = highlights }), new()));
    public static readonly LocalContrastParameter Shadows = new("shadows", new StateParameter<LocalContrast>.Bounded<ContrastScale, float, InvalidDetail>(
        Lens<LocalContrast, ContrastScale>.New(static contrast => contrast.Shadows, static shadows => contrast => contrast with { Shadows = shadows }), new()));
    public static readonly LocalContrastParameter Detail = new("detail", new StateParameter<LocalContrast>.Bounded<DetailStrength, float, InvalidDetail>(
        Lens<LocalContrast, DetailStrength>.New(static contrast => contrast.Detail, static detail => contrast => contrast with { Detail = detail }), new()));

    public StateParameter<LocalContrast> Kind { get; }
}
