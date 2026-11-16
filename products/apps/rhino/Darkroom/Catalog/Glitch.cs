using System.Numerics;
using System.Runtime.Intrinsics;
using Darkroom.Chain;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;
using UnitsNet;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record PixelSort(SortMetric Key, float Low, float High, SortDirection Direction) : IPixelStage<PixelSort> {
    public static PixelSort Default { get; } = new(SortMetric.Luma, 0f, 0.3f, SortDirection.Right);

    public static Fin<Option<PixelPass>> Pass(PixelSort state, PassContext context) =>
        Some(state.Direction.Sorted(state.Key, context.Formed ? GradingStyle.Video : GradingStyle.Linear, context.Working.Luminance, state.Low, state.High));
}

internal sealed record CodecDamage(int Quality, JpegColor ColorType, int Generations) : IPixelStage<CodecDamage> {
    public static CodecDamage Default { get; } = new(75, JpegColor.Ratio420, 1);

    public static CodecDamage Jpeg { get; } = Default with { Quality = 60, Coded = 360 };

    public static Seq<(string Name, Func<CodecDamage, CodecDamage> Look)> Looks { get; } = [(nameof(Jpeg), static _ => Jpeg)];

    public Option<int> Coded { get; init; }

    public static Fin<Option<PixelPass>> Pass(CodecDamage state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
            from size in state.Coded.Traverse(lines =>
                PixelExtent.Validated(int.CreateSaturating(double.Max(1d, double.Round((double)frame.Size.Width * lines / frame.Size.Height, MidpointRounding.ToEven))), lines)).As()
            from recoded in Video.Recompressed(frame, VideoStandard.Encoding(context), new RasterCodec.Jpeg(state.Quality, state.ColorType), state.Generations, size)
            select progress.Iter(report => report.Report(frame.Size.Height))));
}

internal sealed record Glitch(
    ShortSideLength BlockWidth, ShortSideLength BlockHeight, float BlockDensity, ShortSideLength TearPeriod, float Split, Angle SplitDirection, float SplitJitter, float Slide,
    VideoStandard Matrix, float LumaCorruption, float ChromaCorruption, WrapMode Wrap, int Seed, Timing Timing, Hold Hold) : IPixelStage<Glitch> {
    public static Glitch Default { get; } = new(
        ShortSideLength.Create(1f / (5f * 0.1f)), ShortSideLength.Create(1f / 5f), 0f, ShortSideLength.Create(MathF.Tau / 20f), 0f, Angle.Zero, 0f, 0f, VideoStandard.Rec601, 0f, 0f,
        WrapMode.Periodic, 0, Timing.Default, Hold.MinValue);

    public static Glitch Corruption { get; } = Default with {
        BlockWidth = ShortSideLength.Create(0.03f * ReferenceFrame.GreaterSide),
        BlockHeight = ShortSideLength.Create(0.03f * ReferenceFrame.GreaterSide),
        BlockDensity = 0.2f,
        BlockShift = ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide),
        ChromaCorruption = 0.6f,
        Hold = Hold.Create(Duration.FromSeconds(12d / 24d)),
    };

    public static Glitch BlockGlitch { get; } = Default with {
        BlockDensity = 0.2f,
        BlockShift = ShortSideLength.Create(0.05f * ReferenceFrame.GreaterSide),
        Split = 0.03f * ReferenceFrame.GreaterSide,
        Hold = Hold.Create(Duration.FromSeconds(2d / 24d)),
    };

    public static Seq<(string Name, Func<Glitch, Glitch> Look)> Looks { get; } = [(nameof(Corruption), static _ => Corruption), (nameof(BlockGlitch), static _ => BlockGlitch)];

    public Option<ShortSideLength> BlockShift { get; init; }
    public Option<ShortSideLength> BlockLift { get; init; }
    public Option<ShortSideLength> Tear { get; init; }

    private bool Blocks => BlockDensity is not 0f && (BlockShift.IsSome || BlockLift.IsSome);

    private bool Moves => Blocks || Tear.IsSome;

    private bool Corrupts => BlockDensity is not 0f && (LumaCorruption is not 0f || ChromaCorruption is not 0f);

    public static Fin<Option<PixelPass>> Pass(Glitch state, PassContext context) =>
        state.Moves || state.Split is not 0f || state.Corrupts ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress))) : None;

    private static Fin<Unit> Kernel(Glitch state, PassContext context, PixelFrame frame, Option<IProgress<int>> progress) {
        PixelExtent extent = context.Extent;
        uint field = CoordinateHash.Branch(state.Seed, state.Hold.Period(state.Timing.At(context)), 3u);
        (uint sites, Vector4 draw) = (CoordinateHash.Branch(field, 1u), CoordinateHash.Uniforms(Vector128<int>.Zero, CoordinateHash.Branch(field, 3u)));
        CoordinateMap blocks = new CoordinateMap.Analytic(Displacement(state, extent, sites, draw.X), Sampling.Nearest, (state.Wrap, state.Wrap));
        return from moved in (state.Split is not 0f, state.Moves) switch {
            (true, _) => Splits(state, context, draw.Y, blocks, frame, progress),
            (false, true) => blocks.Apply(frame, progress),
            (false, false) => unit,
        }
               from corrupted in state.Corrupts
                   ? state.Matrix.Decomposed(frame, context, (luma, blue, red) => {
                       Video.Corrupted(luma, blue, red, frame, state.LumaCorruption, state.ChromaCorruption, point => state.Cell(extent, point, sites).Map(static cell => cell.Site),
                           CoordinateHash.Branch(field, 2u));
                       return unit;
                   })
                   : unit
               select corrupted;
    }

    private static Action<Span<Vector2>, Span<float>, PixelExtent> Displacement(Glitch state, PixelExtent extent, uint sites, float phase) {
        Vector2 center = new Vector2(extent.Width, extent.Height) / 2f;
        (float shift, float lift) = ((float)state.BlockShift.Pixels(extent), (float)state.BlockLift.Pixels(extent));
        (float tear, float period) = ((float)state.Tear.Pixels(extent), (float)state.TearPeriod.Pixels(extent));
        return (points, _, _) => {
            foreach (ref Vector2 point in points) {
                Vector2 offset = point - center;
                Vector2 moved = state.Blocks
                    ? state.Cell(extent, offset, sites).Map(cell => new Vector2(shift * ((2f * cell.Draw.Y) - 1f), lift * ((2f * cell.Draw.Z) - 1f))).IfNone(Vector2.Zero)
                    : Vector2.Zero;
                float band = ((offset.Y / period) + phase) switch { var turns => turns - float.Floor(turns) - 0.5f };
                point -= moved + new Vector2(tear * band, 0f);
            }
        };
    }

    private static Fin<Unit> Splits(Glitch state, PassContext context, float draw, CoordinateMap blocks, PixelFrame frame, Option<IProgress<int>> progress) {
        PixelExtent extent = context.Extent;
        float reach = state.Split * extent.ShortSide * (1f - state.SplitJitter + (state.SplitJitter * ((2f * draw) - 1f)));
        Vector2 direction = float.SinCos((float)state.SplitDirection.Radians.ToDouble()) switch { var (sin, cos) => new(cos, -sin) };
        return reach is 0f ? state.Moves ? blocks.Apply(frame, progress) : unit
            : CoordinateMap.Integrated(context.Integration,
                tau => Subframe(reach * (1d - state.Slide + (state.Slide * tau)), Vector4.UnitX),
                tau => Subframe(-reach * (1d - state.Slide + (state.Slide * tau)), Vector4.UnitZ),
                _ => Subframe(0d, new Vector4(0f, 1f, 0f, 1f))).Run(frame, progress);

        Fin<(CoordinateMap Map, Vector4 Gains)> Subframe(double shift, Vector4 gains) =>
            new CoordinateMap.Planar(Matrix3x2.CreateTranslation(-(float)shift * direction), Sampling.Linear, (state.Wrap, state.Wrap)) switch {
                var moved => (state.Moves ? new CoordinateMap.Composed(blocks, moved) : moved, gains),
            };
    }

    private Option<(System.Drawing.Point Site, Vector4 Draw)> Cell(PixelExtent extent, Vector2 offset, uint sites) {
        System.Drawing.Point site = new((int)float.Floor(offset.X / (float)BlockWidth.Pixels(extent)), (int)float.Floor(offset.Y / (float)BlockHeight.Pixels(extent)));
        Vector4 draw = CoordinateHash.Uniforms(Vector128.Create(site.X, site.Y, 0, 0), sites);
        return draw.X < BlockDensity ? Some((site, draw)) : None;
    }
}
