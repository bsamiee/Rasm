using System.Globalization;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Filters.Detail;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct NoiseDeviation : IPresented<NoiseDeviation, float> {
    public static NoiseDeviation Off { get; } = new(0f);
    public static NoiseDeviation MinValue => Off;
    public static NoiseDeviation MaxValue { get; } = new(MathF.Sqrt(2f * MathF.Log(float.MaxValue)));
    public static Presentation<float> Presentation { get; } = new() { Soft = (0.001f, 1f), Scale = TrackScale.Log };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record FilmGrain(
    NoiseDeviation Intensity, ShortSideExtent Size, AxisFraction Chroma, Seed Seed, Timing Timing, Hold Hold,
    AxisFraction Shadows, AxisFraction Midtones, AxisFraction Highlights, Magnitude ShadowEdge, Magnitude MidtoneSpan, Magnitude HighlightSpan, Option<ImageFile> Scan, ShortSideExtent ScanSpan)
    : IStateRecord<FilmGrain, FilmGrainParameter>, IPixelStage<FilmGrain> {
    public static FilmGrain Default { get; } = new(
        NoiseDeviation.Off, ShortSideExtent.Create(1f / ReferenceFrame.Height), AxisFraction.MinValue, Seed.MinValue, Timing.Default, Hold.MinValue, AxisFraction.MaxValue, AxisFraction.MaxValue, AxisFraction.MaxValue, Magnitude.Create(0.09f), Magnitude.Create(0.5f - 0.09f),
        Magnitude.Create(1f - 0.5f), None, ShortSideExtent.Create(ReferenceFrame.GreaterSide));

    public static Option<PixelPass> Pass(FilmGrain state, PassContext context) =>
        state.Intensity == NoiseDeviation.Off || (state.Shadows == AxisFraction.MinValue && state.Midtones == AxisFraction.MinValue && state.Highlights == AxisFraction.MinValue)
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(FilmGrain state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        uint field = CoordinateHash.Field(NoiseStream.FilmGrain, state.Seed, state.Hold.Period(state.Timing.At(context)));
        (double sigma, float bias, Vector4 mix) = (state.Size.Pixels(context.Extent), state.Intensity * state.Intensity / 2f, state.Intensity * Draws.Mix(state.Chroma));
        (double start, double span, double width) = (state.ShadowEdge, state.MidtoneSpan, state.HighlightSpan);
        double highlightScale = width > 0d ? width : double.Max(start, span);
        (double X, double Y, double Z) luminance = context.Working.Luminance;
        return Try.lift(() => {
            Fin<(Action<Span<Vector4>, long, long> Draw, Func<Mat, Fin<Unit>> Shape)> draws = state.Scan.Match(
                Some: scan => context.Derivations.Derived((scan, state.ScanSpan, context.Extent), Scores).Map(tile => (Tiled(tile, field), (Func<Mat, Fin<Unit>>)(static _ => unit))),
                None: () => ((row, column, line) => { for (int x = 0; x < row.Length; x++) row[x] = CoordinateHash.Normals(column + x, line, field); }, header => Normalized(header, sigma)));
            return from noise in draws
                   let plane = new PixelFrame(frame.Origin, frame.Size, frame.Extent)
                   from drawn in new PixelPass.Pointwise(noise.Draw).Run(plane, new Progress<int>())
                   from normalized in plane.Filter(noise.Shape)
                   from applied in new PixelPass.Pointwise((row, _, line) => {
                       ReadOnlySpan<Vector4> draws = plane.Row(line);
                       for (int x = 0; x < row.Length; x++) {
                           double luma = LuminanceSamples.Luminance(row[x].X, row[x].Y, row[x].Z, luminance);
                           double logLuma = double.Log2(double.Max(luma, 0d)) + context.Exposure;
                           double dark = start is 0d ? luma > 0d ? 0d : 1d : 1d - RampInterpolation.Ease.Weight(double.Clamp(double.Exp2(logLuma - double.Log2(start)), 0d, 1d));
                           double distance = highlightScale is 0d ? luma : double.Exp2(logLuma - double.Log2(highlightScale)) - (start / highlightScale) - (span / highlightScale);
                           double bright = width is 0d ? distance > 0d ? 1d : 0d : RampInterpolation.Ease.Weight(double.Clamp(distance, 0d, 1d));
                           double weight = (state.Shadows * dark) + (state.Midtones * (1d - dark - bright)) + (state.Highlights * bright);
                           Vector4 log = mix * draws[x];
                           row[x] *= new Vector4(Vector3.Lerp(Vector3.One, Vector3.Exp(log.AsVector3() + new Vector3(log.W - bias)), (float)weight), 1f);
                       }
                   }).Run(frame, progress)
                   select applied;
        }).Run();

        static Fin<Unit> Normalized(Mat header, double sigma) => Kernels.GaussianKernel(sigma).Map(kernel => {
            const double unitVariance = 1d;
            using (kernel) {
                using Mat squared = new();
                using Mat filtered = new();
                using Mat deviation = new(header.Rows, header.Cols, DepthType.Cv64F, header.NumberOfChannels);
                CvInvoke.Multiply(kernel, kernel, squared);
                deviation.SetTo(new MCvScalar(unitVariance, unitVariance, unitVariance, unitVariance));
                CvInvoke.SepFilter2D(header, filtered, DepthType.Cv64F, kernel, kernel, new System.Drawing.Point(-1, -1), 0d, BorderType.Constant);
                CvInvoke.SepFilter2D(deviation, deviation, DepthType.Cv64F, squared, squared, new System.Drawing.Point(-1, -1), 0d, BorderType.Constant);
                CvInvoke.Sqrt(deviation, deviation);
                CvInvoke.Divide(filtered, deviation, filtered);
                filtered.ConvertTo(header, DepthType.Cv32F);
                return unit;
            }
        });

        static Action<Span<Vector4>, long, long> Tiled(Memory2D<Vector4> tile, uint field) {
            Vector2 random = NoiseFunctions.White(Vector4.Zero, field).AsVector2();
            (double X, double Y) = ((double)random.X * tile.Width, -(double)random.Y * tile.Height);
            return (row, column, line) => {
                ReadOnlySpan2D<Vector4> texels = tile.Span;
                for (int x = 0; x < row.Length; x++) row[x] = texels.Sample((column + x + 0.5d + X, -line - 0.5d + Y), (WrapMode.Periodic, WrapMode.Periodic), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight));
            };
        }

        static Fin<Memory2D<Vector4>> Scores((ImageFile Scan, ShortSideExtent Span, PixelExtent Extent) key) {
            double footprint = key.Span.Pixels(key.Extent);
            (double width, double height) = (double.Ceiling(footprint), double.Ceiling(key.Scan.Frame.Size.Height * footprint / key.Scan.Frame.Size.Width));
            return width * height > System.Array.MaxLength
                ? Error.New(string.Create(CultureInfo.InvariantCulture, $"film grain scan tile {width}x{height} exceeds {System.Array.MaxLength} texels"))
                : Try.lift(() => {
                    System.Drawing.Size dimensions = new((int)width, (int)height);
                    int pixels = dimensions.Width * dimensions.Height;
                    using Mat header = key.Scan.Frame.Header();
                    using Mat source = new();
                    using Mat resized = new();
                    using Mat mixed = new();
                    using Mat mean = new(4, 4, DepthType.Cv64F, 1);
                    Inter interpolation = footprint < header.Cols ? Inter.Area : Inter.Linear;
                    header.ConvertTo(source, DepthType.Cv64F);
                    CvInvoke.Resize(source, resized, dimensions, 0d, 0d, interpolation);
                    const double third = 1d / 3d;
                    mean.SetTo([1d, 0d, 0d, 0d, 0d, 1d, 0d, 0d, 0d, 0d, 1d, 0d, third, third, third, 0d]);
                    CvInvoke.Transform(resized, mixed, mean);
                    float[][] lanes = [.. Enumerable.Range(0, 4).Select(lane => {
                        using Mat channel = new();
                        CvInvoke.ExtractChannel(mixed, channel, lane);
                        double[] values = new double[pixels];
                        channel.CopyTo(values);
                        return values.Ranks(RankDefinition.Min).Zip(ArrayStatistics.RanksInplace(values, RankDefinition.Max), (lower, upper) => (float)Normal.InvCDF(0d, 1d, (lower + upper - 1d) / (2d * pixels))).ToArray();
                    })];
                    Vector4[] scores = [.. Enumerable.Range(0, pixels).Select(texel => new Vector4(lanes[0][texel], lanes[1][texel], lanes[2][texel], lanes[3][texel]))];
                    return scores.AsMemory().AsMemory2D(dimensions.Height, dimensions.Width);
                }).Run();
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmGrainParameter : IStateParameter<FilmGrain> {
    public static readonly FilmGrainParameter Intensity = new("intensity", StateParameter.Bounded(Lens<FilmGrain, NoiseDeviation>.New(static grain => grain.Intensity, static intensity => grain => grain with { Intensity = intensity })),
        new("Intensity", "Spread of the grain factor multiplied into scene light, 0 adding no grain"));
    public static readonly FilmGrainParameter Size = new("size", StateParameter.Bounded(Lens<FilmGrain, ShortSideExtent>.New(static grain => grain.Size, static size => grain => grain with { Size = size }), ShortSideExtent.Presentation with { Soft = (1e-4f, 0.0125f) }),
        new("Size", "Grain size as a share of the frame's short side"));
    public static readonly FilmGrainParameter Chroma = new("chroma", StateParameter.Bounded(Lens<FilmGrain, AxisFraction>.New(static grain => grain.Chroma, static chroma => grain => grain with { Chroma = chroma })),
        new("Chroma", "Share of each channel's own grain against grain every channel shares"));
    public static readonly FilmGrainParameter Seed = new("seed", StateParameter.Bounded(Lens<FilmGrain, Seed>.New(static grain => grain.Seed, static seed => grain => grain with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));
    public static readonly FilmGrainParameter Timing = new("timing", new StateParameter<FilmGrain>.Record<Timing>(Lens<FilmGrain, Timing>.New(static grain => grain.Timing, static timing => grain => grain with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly FilmGrainParameter Hold = new("hold", StateParameter.Bounded(Lens<FilmGrain, Hold>.New(static grain => grain.Hold, static hold => grain => grain with { Hold = hold })),
        new("Hold", "Seconds each drawn pattern holds before the next"));
    public static readonly FilmGrainParameter Shadows = new("shadows", StateParameter.Bounded(Lens<FilmGrain, AxisFraction>.New(static grain => grain.Shadows, static weight => grain => grain with { Shadows = weight })),
        new("Shadows", "Grain weight below the shadow edge"));
    public static readonly FilmGrainParameter Midtones = new("midtones", StateParameter.Bounded(Lens<FilmGrain, AxisFraction>.New(static grain => grain.Midtones, static weight => grain => grain with { Midtones = weight })),
        new("Midtones", "Grain weight between the shadow edge and the highlight start"));
    public static readonly FilmGrainParameter Highlights = new("highlights", StateParameter.Bounded(Lens<FilmGrain, AxisFraction>.New(static grain => grain.Highlights, static weight => grain => grain with { Highlights = weight })),
        new("Highlights", "Grain weight from the highlight end up"));
    public static readonly FilmGrainParameter ShadowEdge = new("shadow-edge", StateParameter.Bounded(Lens<FilmGrain, Magnitude>.New(static grain => grain.ShadowEdge, static edge => grain => grain with { ShadowEdge = edge }), new Presentation<float>()),
        new("Shadow edge", "Exposed luminance where the shadow weight reaches 0"));
    public static readonly FilmGrainParameter MidtoneSpan = new("midtone-span", StateParameter.Bounded(Lens<FilmGrain, Magnitude>.New(static grain => grain.MidtoneSpan, static span => grain => grain with { MidtoneSpan = span }), new Presentation<float>()),
        new("Midtone span", "Luminance span from the shadow edge to the highlight band"));
    public static readonly FilmGrainParameter HighlightSpan = new("highlight-span", StateParameter.Bounded(Lens<FilmGrain, Magnitude>.New(static grain => grain.HighlightSpan, static span => grain => grain with { HighlightSpan = span }), new Presentation<float>()),
        new("Highlight span", "Luminance span over which the highlight weight reaches full"));
    public static readonly FilmGrainParameter Scan = new("scan", new StateParameter<FilmGrain>.Loaded<ImageFile, ImagePath, string>(Lens<FilmGrain, Option<ImageFile>>.New(static grain => grain.Scan, static scan => grain => grain with { Scan = scan }), ImageFile.Load, static scan => scan.Path),
        new("Scan", "Grain image scored by channel ranks"));
    public static readonly FilmGrainParameter ScanSpan = new("scan-span", StateParameter.Bounded(Lens<FilmGrain, ShortSideExtent>.New(static grain => grain.ScanSpan, static span => grain => grain with { ScanSpan = span })),
        new("Scan span", "Scan width as a share of the frame's short side"));

    public StateParameter<FilmGrain> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record SensorNoise(NoiseDeviation Shot, NoiseDeviation Read, AxisFraction Chroma, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<SensorNoise, SensorNoiseParameter>, IPixelStage<SensorNoise> {
    public static SensorNoise Default { get; } = new(
        NoiseDeviation.Create(0.1875f * 0.25f * MathF.Sqrt(Exposure.MiddleGray)),
        NoiseDeviation.Off, AxisFraction.MinValue, Seed.MinValue, Timing.Default, Hold.MinValue);

    public static Option<PixelPass> Pass(SensorNoise state, PassContext context) {
        uint field = CoordinateHash.Field(NoiseStream.SensorNoise, state.Seed, state.Hold.Period(state.Timing.At(context)));
        double shot = double.Log2(state.Shot) - (0.5d * (context.Exposure + double.Log2(Exposure.MiddleGray)));
        double read = double.Log2(state.Read) - context.Exposure;
        Vector4 mix = Draws.Mix(state.Chroma);
        (double X, double Y, double Z) luminance = context.Working.Luminance;
        double Deviation(double signal, double draw) =>
            double.Log2(double.Abs(draw)) switch {
                var amplitude => double.CopySign(double.Hypot(double.Exp2(amplitude + shot + (0.5d * double.Log2(double.Max(signal, 0d)))), double.Exp2(amplitude + read)), draw),
            };
        return state.Shot == NoiseDeviation.Off && state.Read == NoiseDeviation.Off ? None : Some<PixelPass>(new PixelPass.Pointwise((row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                Vector4 draws = mix * CoordinateHash.Normals(column + x, line, field);
                double shared = Deviation(LuminanceSamples.Luminance(row[x].X, row[x].Y, row[x].Z, luminance), draws.W);
                row[x] = new Vector4(
                    (float)(row[x].X + Deviation(row[x].X, draws.X) + shared), (float)(row[x].Y + Deviation(row[x].Y, draws.Y) + shared), (float)(row[x].Z + Deviation(row[x].Z, draws.Z) + shared), row[x].W);
            }
        }));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensorNoiseParameter : IStateParameter<SensorNoise> {
    public static readonly SensorNoiseParameter Shot = new("shot", StateParameter.Bounded(Lens<SensorNoise, NoiseDeviation>.New(static noise => noise.Shot, static shot => noise => noise with { Shot = shot })),
        new("Shot", "Noise deviation at middle gray, rising with the square root of exposed light"));
    public static readonly SensorNoiseParameter Read = new("read", StateParameter.Bounded(Lens<SensorNoise, NoiseDeviation>.New(static noise => noise.Read, static read => noise => noise with { Read = read })),
        new("Read", "Noise deviation independent of the signal"));
    public static readonly SensorNoiseParameter Chroma = new("chroma", StateParameter.Bounded(Lens<SensorNoise, AxisFraction>.New(static noise => noise.Chroma, static chroma => noise => noise with { Chroma = chroma })),
        new("Chroma", "Share of each channel's own noise against noise every channel shares"));
    public static readonly SensorNoiseParameter Seed = new("seed", StateParameter.Bounded(Lens<SensorNoise, Seed>.New(static noise => noise.Seed, static seed => noise => noise with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));
    public static readonly SensorNoiseParameter Timing = new("timing", new StateParameter<SensorNoise>.Record<Timing>(Lens<SensorNoise, Timing>.New(static noise => noise.Timing, static timing => noise => noise with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly SensorNoiseParameter Hold = new("hold", StateParameter.Bounded(Lens<SensorNoise, Hold>.New(static noise => noise.Hold, static hold => noise => noise with { Hold = hold })),
        new("Hold", "Seconds each drawn pattern holds before the next"));

    public StateParameter<SensorNoise> Kind { get; }
    public ParameterText Text { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Draws {
    public static Vector4 Mix(AxisFraction chroma) => new(new Vector3(MathF.Sqrt(chroma)), MathF.Sqrt(1f - chroma));
}
