using System.Numerics;
using System.Runtime.Intrinsics;
using Darkroom.Chain;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class Separation {
    public static readonly Separation Mono = new("mono", static (rgb, luminance) => new Vector4(Vector3.Zero, 1f - float.Clamp(Vector3.Dot(luminance, rgb), 0f, 1f)));
    public static readonly Separation Process = new("process", static (rgb, _) => Vector3.Clamp(rgb, Vector3.Zero, Vector3.One) switch {
        var saturated => float.Max(saturated.X, float.Max(saturated.Y, saturated.Z)) switch {
            > 0f and var most => new Vector4(Vector3.One - (saturated / most), 1f - most),
            _ => Vector4.UnitW,
        },
    });

    [UseDelegateFromConstructor]
    public partial Vector4 Levels(Vector3 rgb, Vector3 luminance);
}

[SmartEnum<string>]
public sealed partial class MosaicCell {
    public static readonly MosaicCell Square = new("square", static (state, context, frame, progress) => {
        double cell = state.Size.Pixels(context.Extent);
        CellGrid grid = CellGrid.Fit(context.Extent, cell, cell);
        Func<(long Column, long Row), Vector3> colors = grid.Means(frame);
        return new PixelPass.Positional((row, x, y) => {
            long down = grid.Row(y, context.Extent.Height).Cell;
            for (int at = 0; at < row.Length; at++)
                row[at] = new Vector4(colors((grid.Column(x + at, context.Extent.Width).Cell, down)), row[at].W);
        }).Run(frame, progress);
    });
    public static readonly MosaicCell Cellular = new("cellular", static (state, context, frame, progress) => {
        PixelFrame copy = frame.Copy();
        (Vector2 center, float pitch) = (new Vector2(context.Extent.Width, context.Extent.Height) / 2f, (float)state.Size.Pixels(context.Extent));
        (uint field, Octaves octaves) = (CoordinateHash.Branch(state.Seed, state.Hold.Period(state.Timing.At(context)), 4u), NoiseBasis.Perlin.Plain.Octaves);
        return VoronoiFeature.F1.Sampler(NoiseDimensions.Two, octaves, state.Exponent, state.Randomness, 0f).Bind(sampling => new PixelPass.Positional((row, x, y) => {
            for (int at = 0; at < row.Length; at++) {
                Vector4 site = sampling.Sample(new Vector4((new Vector2(x + at + 0.5f, y + 0.5f) - center) / pitch, 0f, 0f), field).Site;
                Vector2 source = Vector128.Floor(((new Vector2(site.X, site.Y) * pitch) + center).AsVector128()).AsVector2() + new Vector2(0.5f);
                row[at] = new Vector4(copy.Sample(source, (WrapMode.Clamp, WrapMode.Clamp)).AsVector3(), row[at].W);
            }
        }).Run(frame, progress));
    });

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Run(Mosaic state, PassContext context, PixelFrame frame, Option<IProgress<int>> progress);
}

[SmartEnum<string>]
public sealed partial class GlyphColoring {
    public static readonly GlyphColoring Input = new("input", static (_, _, mean, _, _) => mean);
    public static readonly GlyphColoring Text = new("text", static (_, text, _, _, _) => text);
    public static readonly GlyphColoring PerCell = new("per-cell", static (state, _, _, gray, tint) => Vector3.Lerp(new Vector3(gray), tint, state.Saturation));

    [UseDelegateFromConstructor]
    public partial Vector3 Tint(GlyphCells state, Vector3 text, Vector3 mean, float gray, Vector3 tint);
}

public sealed record HalftoneScreen(Angle Angle, float U, float V) {
    private const float ChannelScale = 4f;

    public static HalftoneScreen Default { get; } = new(Angle.FromDegrees(45d), 0f, 0f);
    public static HalftoneScreen Cyan { get; } = Default with { U = 0.193f * ChannelScale, V = 0.052f * ChannelScale };
    public static HalftoneScreen Magenta { get; } = Default with { U = 0.052f * ChannelScale, V = 0.193f * ChannelScale };
    public static HalftoneScreen Yellow { get; } = Default with { U = 0f, V = 0.2f * ChannelScale };
    public static HalftoneScreen Key { get; } = Default with { U = 0.1414f * ChannelScale, V = 0.1414f * ChannelScale };

    public Matrix3x2 Frame(double pitch, float turn) =>
        Matrix3x2.CreateScale((float)(1d / pitch)) * Matrix3x2.CreateRotation((float)Angle.Radians.ToDouble() + turn) * Matrix3x2.CreateTranslation(U, V);
}

public sealed record Halftone(
    SpotFunction Shape, ShortSideLength Size, Separation Separation, HalftoneScreen Cyan, HalftoneScreen Magenta, HalftoneScreen Yellow, HalftoneScreen Key, float CrossOnset,
    Unicolour KeyColor, Option<Unicolour> Paper, BlendingMode Mode) : IPixelStage<Halftone> {
    public static Halftone Default { get; } = new(
        SpotFunction.Round, ShortSideLength.Create(1f / (2f * 2f * 4f) * ReferenceFrame.GreaterSide), Separation.Mono, HalftoneScreen.Cyan, HalftoneScreen.Magenta, HalftoneScreen.Yellow,
        HalftoneScreen.Key, 1f, new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), BlendingMode.Normal);

    public static Fin<Option<PixelPass>> Pass(Halftone state, PassContext context) => state.Mode.Mixed(new PixelPass.Positional(Print(state, context)), 1f, []);

    private static Action<Span<Vector4>, int, int> Print(Halftone state, PassContext context) {
        (double pitch, Vector2 middle, Vector3 luminance) = (state.Size.Pixels(context.Extent), context.Extent.Point(Vector2.One) / 2f, context.Working.Luminance);
        Arr<(Arr<(Matrix3x2 Frame, float Onset)> Screens, Vector3 Absorbance, Vector4 Channel)> plates = [
            Plate(state.Cyan, Vector3.UnitX, Vector4.UnitX), Plate(state.Magenta, Vector3.UnitY, Vector4.UnitY), Plate(state.Yellow, Vector3.UnitZ, Vector4.UnitZ),
            Plate(state.Key, Vector3.One - RampLight.Light(state.KeyColor, context).AsVector3(), Vector4.UnitW)];
        Option<Vector3> paper = state.Paper.Map(color => RampLight.Light(color, context).AsVector3());
        return (row, x, y) => {
            for (int at = 0; at < row.Length; at++) {
                Vector2 point = new Vector2(x + at + 0.5f, y + 0.5f) - middle;
                Vector4 levels = state.Separation.Levels(row[at].AsVector3(), luminance);
                Vector3 transmitted = Vector3.One;
                foreach ((Arr<(Matrix3x2 Frame, float Onset)> screens, Vector3 absorbance, Vector4 channel) in plates.AsSpan())
                    transmitted *= Vector3.One - ((1f - Clear(screens.AsSpan(), point, Vector4.Dot(levels, channel))) * absorbance);
                row[at] = new Vector4(paper.IfNone(row[at].AsVector3()) * transmitted, row[at].W);
            }
        };

        float Clear(ReadOnlySpan<(Matrix3x2 Frame, float Onset)> screens, Vector2 point, float level) {
            if (level <= 0f)
                return 1f;
            float clear = 1f;
            foreach ((Matrix3x2 frame, float onset) in screens)
                clear *= 1f - state.Shape.Coverage(point, level, frame, onset);
            return clear;
        }

        (Arr<(Matrix3x2 Frame, float Onset)> Screens, Vector3 Absorbance, Vector4 Channel) Plate(HalftoneScreen screen, Vector3 absorbance, Vector4 channel) =>
            (state.CrossOnset < 1f ? [(screen.Frame(pitch, 0f), 0f), (screen.Frame(pitch, float.Pi / 2f), state.CrossOnset)] : [(screen.Frame(pitch, 0f), 0f)], absorbance, channel);
    }
}

public sealed record Mosaic(MosaicCell Cell, ShortSideLength Size, float Exponent, float Randomness, int Seed, Timing Timing, Hold Hold) : IPixelStage<Mosaic> {
    public static Mosaic Default { get; } = new(
        MosaicCell.Square, ShortSideLength.Create(0.1f * 0.05f * ReferenceFrame.GreaterSide), NoiseBasis.Voronoi.Default.Exponent, NoiseBasis.Voronoi.Default.Randomness, 0,
        Timing.Default with { Pace = 0f }, Hold.MinValue);

    public static Fin<Option<PixelPass>> Pass(Mosaic state, PassContext context) => Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Cell.Run(state, context, frame, progress)));
}

public sealed record GlyphCells(
    string Characters, HersheyFonts Font, ShortSideLength Size, GlyphColoring Coloring, Unicolour Text,
    Unicolour Background, float Saturation, float Jitter, int Seed, Timing Timing, Hold Hold, BlendingMode Mode)
    : IPixelStage<GlyphCells> {
    public static GlyphCells Default { get; } = new(
        " .:-=+*#%@", HersheyFonts.Simplex, ShortSideLength.Create(1f / 24f), GlyphColoring.Input, new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d),
        1f, 0.1f, 0, Timing.Default, Hold.Create(Duration.FromSeconds(6d / 24d)), BlendingMode.Normal);

    public static GlyphCells Binary { get; } = Default with { Characters = " 01" };
    public static GlyphCells Numbers { get; } = Default with { Characters = " 0123456789" };
    public static GlyphCells Arithmetic { get; } = Default with { Characters = " +-*/=<>^%" };
    public static GlyphCells Latin { get; } = Default with { Characters = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" };
    public static GlyphCells Mixed { get; } = Default with { Characters = " .:-=+*#%@01xX" };

    public static Seq<(string Name, Func<GlyphCells, GlyphCells> Look)> Looks { get; } = [
        (nameof(Binary), static _ => Binary), (nameof(Numbers), static _ => Numbers), (nameof(Arithmetic), static _ => Arithmetic), (nameof(Latin), static _ => Latin), (nameof(Mixed), static _ => Mixed)];

    public static Fin<Option<PixelPass>> Pass(GlyphCells state, PassContext context) => state.Mode.Mixed(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)), 1f, []);

    private static Fin<Unit> Kernel(GlyphCells state, PassContext context, PixelFrame frame, Option<IProgress<int>> progress) =>
        GlyphAtlas.Rendered(state.Characters, state.Font, state.Size.Pixels(context.Extent), context.Extent).Bind(atlas => {
            Func<(long Column, long Row), Vector3> mean = atlas.Grid.Means(frame);
            uint field = CoordinateHash.Branch(state.Seed, state.Hold.Period(state.Timing.At(context)), 5u);
            (uint gray, uint tint, Vector3 luminance, Vector3 text) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), context.Working.Luminance, RampLight.Light(state.Text, context).AsVector3());
            Func<(long Column, long Row), (int Glyph, Vector3 Color)> cells = memoUnsafe<(long Column, long Row), (int Glyph, Vector3 Color)>(cell => {
                Vector3 color = mean(cell);
                Vector128<int> at = Vector128.Create((int)cell.Column, (int)cell.Row, 0, 0);
                (Vector4 u, Vector4 c) = (CoordinateHash.Uniforms(at, gray), CoordinateHash.Uniforms(at, tint));
                float level = float.Clamp(float.Clamp(Vector3.Dot(luminance, color), 0f, 1f) + (state.Jitter * (u.X - 0.5f)), 0f, 1f);
                return (atlas.Glyph(level), state.Coloring.Tint(state, text, color, u.Y, c.AsVector3()));
            });
            (int width, int height, Vector3 background) = (context.Extent.Width, context.Extent.Height, RampLight.Light(state.Background, context).AsVector3());
            return new PixelPass.Positional((row, x, y) => {
                long down = atlas.Grid.Row(y, height).Cell;
                for (int at = 0; at < row.Length; at++) {
                    (int glyph, Vector3 color) = cells((atlas.Grid.Column(x + at, width).Cell, down));
                    row[at] = new Vector4(Vector3.Lerp(background, color, atlas.Coverage(glyph, x + at, y)), row[at].W);
                }
            }).Run(frame, progress);
        });
}
