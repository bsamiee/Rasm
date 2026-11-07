using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using Darkroom.Chain;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Color;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
internal sealed partial class VideoStandard {
    public static readonly VideoStandard Rec601 = new("rec601", YbrConfiguration.Rec601);
    public static readonly VideoStandard Rec709 = new("rec709", YbrConfiguration.Rec709);

    public YbrConfiguration Ybr { get; }

    public Vector3 Luminance => new((float)Ybr.Kr, (float)Ybr.Kg, (float)Ybr.Kb);

    public static Transfer Encoding(PassContext context) => context.Formed ? new Transfer.Linear() : context.Display.Encoding.Transfer;

    public Fin<Unit> Decomposed(PixelFrame frame, PassContext context, Func<Mat, Mat, Mat, Fin<Unit>> planes) => Video.Decomposed(frame, Ybr, Encoding(context), planes);
}

[ValueObject<int>(
    ConversionToKeyMemberType = ConversionOperatorsGeneration.None, UnsafeConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit, SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<DarkroomFault>]
internal sealed partial class ScanLines {
    static partial void ValidateFactoryArguments(ref DarkroomFault? validationError, ref int value) =>
        validationError = value >= 1 ? null : new DarkroomFault(value, DarkroomReason.Invalid, Expression: "lines");
}

internal sealed record CrtDisplay(
    PhosphorMask Mask, ShortSideLength Pitch, float MaskStrength, ScanLines Lines, bool Interlaced, float ScanlineDepth, float Collapse, float Curvature, float Framing, Timing Timing)
    : IPixelStage<CrtDisplay> {
    private const float NearShare = 0.890909f;

    public static CrtDisplay Default { get; } = new(PhosphorMask.SlotMask, ShortSideLength.Create(8f / ReferenceFrame.Height), 0.8f, ScanLines.Create(480), Interlaced: false, 1f, 0f, 0f, 0f, Timing.Default);

    public static CrtDisplay CathodeRayTube { get; } = Default with {
        Mask = PhosphorMask.ApertureGrille,
        Pitch = ShortSideLength.Create(ReferenceFrame.GreaterSide / 640f),
        MaskStrength = 0.705394f,
        Lines = ScanLines.Create(240),
        ScanlineDepth = 0.5f,
        Glow = ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide / 3f),
        Curvature = 0.0497677f,
    };

    public static CrtDisplay Scanlines { get; } = Default with { MaskStrength = 0f, ScanlineDepth = 0.4f };

    public static Seq<(string Name, Func<CrtDisplay, CrtDisplay> Look)> Looks { get; } = [(nameof(CathodeRayTube), static _ => CathodeRayTube), (nameof(Scanlines), static _ => Scanlines)];

    public Option<ShortSideLength> Glow { get; init; }

    private bool Lit => MaskStrength is not 0f || ScanlineDepth is not 0f || Collapse is not 0f;

    private Vector2 Scales =>
        (2f * Collapse, 1f / Lines) switch {
            var (collapse, line) => new(float.Min(1f, float.Max(2f - collapse, line)), float.Max(1f - collapse, line)),
        };

    public static Fin<Option<PixelPass>> Pass(CrtDisplay state, PassContext context) =>
        (state.Lit, state.Collapse is 0f && state.Glow.IsNone && state.Curvature is 0f) switch {
            (false, true) => None,
            (true, true) => Some(Raster(state, context)),
            _ => Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from collapsed in state.Collapse is 0f ? unit
                    : new CoordinateMap.Planar(Matrix3x2.CreateScale(Vector2.One / state.Scales, context.Extent.Point(new Vector2(0.5f))), Sampling.Linear, (WrapMode.Black, WrapMode.Black)).Apply(frame, progress)
                from glowed in state.Glow.Traverse(glow => Video.Halation(frame, Lobes(glow.Pixels(context.Extent)))).As()
                from rastered in state.Lit ? Raster(state, context).Run(frame, progress) : unit
                from warped in state.Curvature is 0f ? unit : state.Glass(context.Extent).Bind(glass => glass.Apply(frame, progress))
                select warped)),
        };

    private static Seq<(Vector2 Sigma, float Weight)> Lobes(double sigma) =>
        [(new Vector2((float)sigma), NearShare), (new Vector2(float.Sqrt(17f), float.Sqrt(2f)) * (float)sigma, 1f - NearShare)];

    private static PixelPass Raster(CrtDisplay state, PassContext context) =>
        Video.Raster(context.Extent, state.Lines, state.Interlaced, TelevisionSystem.Ntsc.Frame((float)state.Timing.At(context).TotalSeconds), state.Scales, state.Mask,
            state.Pitch.Pixels(context.Extent), state.MaskStrength, state.ScanlineDepth);

    private Fin<CoordinateMap> Glass(PixelExtent extent) =>
        from window in ViewWindow.Centered(2f * extent.Width / int.Max(extent.Width, extent.Height), extent)
        from glass in new LensProfile(new LensDistortion(-Curvature, 0d, 0d, 0d, 0d), 0d, LensDirection.Distort, Framing, new Resampling(Sampling.Linear, WrapMode.Black))
            .Map(new Projection.Rectilinear(extent, window))
        select glass;
}

internal sealed record AnalogVideo(
    VideoStandard Matrix, ScanLines Lines, float ChromaDelay, float SmearMix, float Ringing, float RingingDelay, float Emboss, float Ghost, float GhostDelay, float CrossColor,
    float LumaNoise, float ChromaNoise, ShortSideLength NoiseLength, float Snow, float SnowDensity, ShortSideLength SnowLength, float Interlace, ShortSideLength JitterPeriod,
    float Wrinkle, ShortSideLength WrinkleHeight, float WrinkleRate, float HeadSwitch, ShortSideLength HeadSwitchHeight, float RollRate, float Hum, float HumRate, int Seed,
    Timing Timing) : IPixelStage<AnalogVideo> {
    public static AnalogVideo Default { get; } = new(
        VideoStandard.Rec601, ScanLines.Create(480), 0f, 0.3f * 0.25f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, ShortSideLength.Create(ReferenceFrame.GreaterSide / 40f), 0f, 0f,
        ShortSideLength.Create(0.04f * ReferenceFrame.GreaterSide), 0f, ShortSideLength.Create(1f / 65f), 0f, ShortSideLength.Create(0.15f), 0f, 0f,
        ShortSideLength.Create(0.31f / (32f - (28.8f * 0.5f))), 0f, 0f, 60f / 1001f, 0, Timing.Default);

    public static AnalogVideo ChromaBlur { get; } =
        Default with { ChromaBandwidth = ShortSideLength.Create(128f / ReferenceFrame.Height / 3f), ChromaVertical = ShortSideLength.Create(12.8f / ReferenceFrame.Height / 3f) };

    public static Seq<(string Name, Func<AnalogVideo, AnalogVideo> Look)> Looks { get; } = [(nameof(ChromaBlur), static _ => ChromaBlur)];

    public Option<ShortSideLength> LumaBandwidth { get; init; }
    public Option<ShortSideLength> ChromaBandwidth { get; init; }
    public Option<ShortSideLength> ChromaVertical { get; init; }
    public Option<ShortSideLength> ChromaPitch { get; init; }
    public Option<ShortSideLength> Smear { get; init; }
    public Option<ShortSideLength> Jitter { get; init; }
    public Option<ShortSideLength> Bounce { get; init; }

    private bool Moves => Interlace is not 0f || Jitter.IsSome || Wrinkle is not 0f || HeadSwitch is not 0f || Bounce.IsSome || RollRate is not 0f;

    private bool Speckled => (Snow is not 0f && SnowDensity is not 0f) || Hum is not 0f;

    private bool Filters(PixelExtent extent) =>
        LumaBandwidth.IsSome || ChromaBandwidth.IsSome || ChromaVertical.IsSome || ChromaPitch.Exists(pitch => pitch.Pixels(extent) > 1d)
        || ChromaDelay is not 0f || (Smear.IsSome && SmearMix is not 0f) || Ghost is not 0f || CrossColor is not 0f || LumaNoise is not 0f || ChromaNoise is not 0f;

    public static Fin<Option<PixelPass>> Pass(AnalogVideo state, PassContext context) =>
        state.Moves || state.Filters(context.Extent) || state.Speckled ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress))) : None;

    private static Fin<Unit> Kernel(AnalogVideo state, PassContext context, PixelFrame frame, Option<IProgress<int>> progress) {
        (PixelExtent extent, float now) = (context.Extent, (float)state.Timing.At(context).TotalSeconds);
        uint field = CoordinateHash.Branch(state.Seed, BitConverter.SingleToUInt32Bits(now), 2u);
        return from synced in state.Moves ? Sync(state, extent, now, field, frame, progress) : unit
               from signaled in state.Filters(extent)
                   ? state.Matrix.Decomposed(frame, context, (luma, blue, red) => Signal(state, extent, now, field, frame, (luma, blue, red))).Map(_ => progress.Iter(report => report.Report(frame.Size.Height)))
                   : unit
               from speckled in state.Speckled
                   ? Video.Snow(extent, state.Lines, state.SnowLength.Pixels(extent), state.Snow, state.SnowDensity, state.Hum, Fraction((double)state.HumRate * now), state.Matrix.Luminance,
                       CoordinateHash.Branch(field, 1u)).Run(frame, progress)
                   : unit
               select speckled;
    }

    private static Fin<Unit> Sync(AnalogVideo state, PixelExtent extent, float now, uint field, PixelFrame frame, Option<IProgress<int>> progress) {
        (float rise, float crest) = ((float)(Fraction((double)state.RollRate * now) * extent.Height), (float)((1d - Fraction((double)state.WrinkleRate * now)) * extent.Height));
        float jump = (float)(state.Bounce.Pixels(extent) * ((2d * CoordinateHash.Uniforms(Vector128<int>.Zero, CoordinateHash.Branch(field, 3u)).X) - 1d));
        return Gaussian.Kernel(state.Jitter.IsSome ? state.JitterPeriod.Pixels(extent) * state.Lines / extent.Height : 0d, NormType.L2).Map(static taps => {
            using (taps) {
                double[] weights = new double[taps.Rows];
                taps.CopyTo(weights);
                return weights;
            }
        }).Bind(weights => new CoordinateMap.Composed(
            new CoordinateMap.Composed(
                new CoordinateMap.Planar(Matrix3x2.CreateTranslation(0f, rise), Sampling.Linear, (WrapMode.Periodic, WrapMode.Periodic)),
                new CoordinateMap.Planar(Matrix3x2.CreateTranslation(0f, -jump), Sampling.Linear, (WrapMode.Black, WrapMode.Black))),
            new CoordinateMap.Analytic(Shifted(state, extent, crest, weights, CoordinateHash.Branch(field, 2u)), Sampling.Linear, (WrapMode.Clamp, WrapMode.Clamp))).Apply(frame, progress));
    }

    private static Action<Span<Vector2>, Span<float>, PixelExtent> Shifted(AnalogVideo state, PixelExtent extent, float crest, double[] weights, uint draws) {
        const double crestShare = 0.9d;
        (double interlace, double jitter, double wrinkle, double wrinkleBand) =
            ((double)state.Interlace * extent.ShortSide, state.Jitter.Pixels(extent), (double)state.Wrinkle * extent.ShortSide, state.WrinkleHeight.Pixels(extent));
        (double switching, double switchBand) = ((double)state.HeadSwitch * extent.ShortSide, state.HeadSwitchHeight.Pixels(extent));
        return (points, _, _) => {
            long line = Video.Line((long)float.Floor(points[0].Y), state.Lines, extent.Height);
            double draw = 0d;
            for (int tap = 0; tap < weights.Length; tap++)
                draw += weights[tap] * CoordinateHash.Normals(Vector128.Create(0, (int)(line + tap - (weights.Length / 2)), 0, 0), draws).X;
            foreach (ref Vector2 point in points) {
                double distance = point.Y - crest;
                distance -= double.Floor((distance / extent.Height) + 0.5d) * extent.Height;
                double band = (extent.Height - point.Y) / switchBand;
                double head = band is >= 0d and < 1d ? switching * (band < crestShare ? band / crestShare : (1d - band) / (1d - crestShare)) : 0d;
                point.X -= (float)((long.IsEvenInteger(line) ? 0d : interlace) + (jitter * draw) + (wrinkle * double.Max(0d, 1d - (double.Abs(distance) / wrinkleBand))) + head);
            }
        };
    }

    private static Fin<Unit> Signal(AnalogVideo state, PixelExtent extent, float now, uint field, PixelFrame frame, (Mat Luma, Mat Blue, Mat Red) planes) {
        if (state.CrossColor is not 0f)
            TelevisionSystem.Ntsc.CrossColor(planes.Luma, planes.Blue, planes.Red, frame, state.Lines, TelevisionSystem.Ntsc.Frame(now), state.CrossColor);
        return (state is { LumaNoise: 0f, ChromaNoise: 0f } ? unit
                : Video.LineNoise(planes.Luma, planes.Blue, planes.Red, frame, state.Lines, state.NoiseLength.Pixels(extent), state.LumaNoise, state.ChromaNoise, CoordinateHash.Branch(field, 0u)))
            .Bind(_ => Luma(state, extent, planes.Luma))
            .Bind(_ => Chroma(state, extent, planes.Blue))
            .Bind(_ => Chroma(state, extent, planes.Red));
    }

    private static Fin<Unit> Luma(AnalogVideo state, PixelExtent extent, Mat luma) {
        double sigma = state.LumaBandwidth.Pixels(extent);
        (bool rings, bool embosses) = (sigma > 0d && state.Ringing is not 0f, sigma > 0d && state.Emboss is not 0f);
        using Mat ring = new();
        using Mat relief = new();
        return (sigma > 0d ? Filtered(luma, Gaussian.Kernel(sigma)) : unit)
            .Bind(_ => {
                if (rings)
                    CvInvoke.Sobel(luma, ring, DepthType.Cv32F, 2, 0, 1, 1d, 0d, BorderType.Replicate);
                if (embosses)
                    CvInvoke.Sobel(luma, relief, DepthType.Cv32F, 1, 0, 1, 1d, 0d, BorderType.Replicate);
                return rings ? Filtered(ring, Video.Echo(0d, (double)state.RingingDelay * extent.ShortSide, 1f)) : unit;
            })
            .Bind(_ => state.Smear.Filter(_ => state.SmearMix is not 0f).Map(smear => smear.Pixels(extent))
                .Traverse(smear => Filtered(luma, Video.Echo(smear, smear, state.SmearMix))).As())
            .Bind(_ => {
                if (rings)
                    CvInvoke.ScaleAdd(ring, -state.Ringing * sigma * sigma, luma, luma);
                if (embosses)
                    CvInvoke.ScaleAdd(relief, -state.Emboss * sigma / 2d, luma, luma);
                return state.Ghost is 0f ? unit : Filtered(luma, Video.Echo(0d, (double)state.GhostDelay * extent.ShortSide, state.Ghost));
            });
    }

    private static Fin<Unit> Chroma(AnalogVideo state, PixelExtent extent, Mat plane) {
        foreach (double pitch in state.ChromaPitch.Map(chroma => chroma.Pixels(extent)).Filter(static pitch => pitch > 1d).ToSpan())
            Resampled(plane, pitch);
        return state.ChromaBandwidth.Traverse(band => Filtered(plane, Gaussian.Kernel(band.Pixels(extent)))).As()
            .Bind(_ => state.ChromaVertical.Traverse(band => Filtered(plane, Gaussian.Kernel(band.Pixels(extent)), vertical: true)).As())
            .Bind(_ => state.ChromaDelay is 0f ? unit : Filtered(plane, Video.Echo(0d, (double)state.ChromaDelay * extent.ShortSide, 1f)))
            .Bind(_ => state.Ghost is 0f ? unit : Filtered(plane, Video.Echo(0d, (double)state.GhostDelay * extent.ShortSide, state.Ghost)));
    }

    private static void Resampled(Mat plane, double pitch) {
        using Mat coarse = new();
        CvInvoke.Resize(plane, coarse, new Size(int.Max(1, (int)Math.Round(plane.Cols / pitch, MidpointRounding.ToEven)), plane.Rows), interpolation: Inter.Area);
        CvInvoke.Resize(coarse, plane, plane.Size, interpolation: Inter.Linear);
    }

    private static Fin<Unit> Filtered(Mat plane, Fin<Mat> taps, bool vertical = false) =>
        taps.Map(kernel => {
            using (kernel)
                Video.Filter(plane, kernel, vertical);
            return unit;
        });

    private static double Fraction(double turns) => turns - double.Floor(turns);
}
