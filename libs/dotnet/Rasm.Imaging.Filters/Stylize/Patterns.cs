using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct CellSize : IMinMaxValue<CellSize> {
    public static CellSize MinValue { get; } = new(ShortSideExtent.MinValue);
    public static CellSize MaxValue { get; } = new(1f / 4f);
    public static CellSize Standard { get; } = new(1f / (2f * 2f * 4f) * ReferenceFrame.GreaterSide);
    public static CellSize Pixelate { get; } = new(0.1f * 0.05f * ReferenceFrame.GreaterSide);
    public static CellSize Glyph { get; } = new(1f / 24f);
    public static Presentation<CellSize, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Scale = TrackScale.Log };

    public float Pixels(PixelExtent extent) => _value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct CellOffset : IMinMaxValue<CellOffset> {
    public static CellOffset MinValue => Neutral;
    public static CellOffset MaxValue { get; } = new(float.BitDecrement(1f));
    public static Presentation<CellOffset, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<string>(SkipIParsable = true)]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Glyphs {
    public static Glyphs Binary { get; } = new(" 01");
    public static Glyphs Numbers { get; } = new(" 0123456789");
    public static Glyphs Symbols { get; } = new(" .:-=+*#%@");
    public static Glyphs Math { get; } = new(" +-*/=<>^%");
    public static Glyphs Latin { get; } = new(" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
    public static Glyphs Mixed { get; } = new(" .:-=+*#%@01xX");

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.AsSpan().ContainsAnyExceptInRange(' ', '~') && toHashSet(value).Count == value.Length ? null : new InvalidStylize();
}

public sealed record InkScreen(SignedAngle Angle, CellOffset U, CellOffset V) : IStateRecord<InkScreen, InkScreenParameter, InvalidStylize> {
    private const float ChannelScale = 4f;

    public static InkScreen Default { get; } = new(SignedAngle.Diagonal, CellOffset.Neutral, CellOffset.Neutral);
    public static InkScreen Cyan { get; } = Shifted(0.193f, 0.052f);
    public static InkScreen Magenta { get; } = Shifted(0.052f, 0.193f);
    public static InkScreen Yellow { get; } = Shifted(0f, 0.2f);
    public static InkScreen Key { get; } = Shifted(0.1414f, 0.1414f);

    private static InkScreen Shifted(float u, float v) => Default with { U = CellOffset.Create(u * ChannelScale), V = CellOffset.Create(v * ChannelScale) };
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InkScreenParameter : IStateParameter<InkScreen> {
    public static readonly InkScreenParameter Angle = new("angle", new StateParameter<InkScreen>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<InkScreen, SignedAngle>.New(static ink => ink.Angle, static angle => ink => ink with { Angle = angle }), SignedAngle.Presentation));
    public static readonly InkScreenParameter U = new("u", new StateParameter<InkScreen>.Bounded<CellOffset, float, InvalidStylize>(
        Lens<InkScreen, CellOffset>.New(static ink => ink.U, static u => ink => ink with { U = u }), CellOffset.Presentation));
    public static readonly InkScreenParameter V = new("v", new StateParameter<InkScreen>.Bounded<CellOffset, float, InvalidStylize>(
        Lens<InkScreen, CellOffset>.New(static ink => ink.V, static v => ink => ink with { V = v }), CellOffset.Presentation));

    public StateParameter<InkScreen> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScreenShape {
    private const double Elongation = double.Pi / 1.6d;

    public static readonly ScreenShape Round = new("round", static (u, v) => (float)Quarter(double.Hypot(u, v), 1d, 1d));
    public static readonly ScreenShape Ellipse = new("ellipse", static (u, v) => (float)(Elongation * Quarter(double.Hypot(u, v / Elongation), 1d, 1d / Elongation)));
    public static readonly ScreenShape Diamond = new("diamond", static (u, v) => (float.Abs(u) + float.Abs(v)) switch { <= 1f and var d => d * d / 2f, var d => 1f - ((2f - d) * (2f - d) / 2f) });
    public static readonly ScreenShape Square = new("square", static (u, v) => float.Max(float.Abs(u), float.Abs(v)) switch { var m => m * m });
    public static readonly ScreenShape Line = new("line", static (_, v) => float.Abs(v));
    public static readonly ScreenShape WavyLine = new("wavy-line", static (u, v) => (v - (0.5f * float.SinPi(u))) switch { var w => float.Abs(w - (2f * float.Floor((w + 1f) / 2f))) });
    public static readonly ScreenShape Cross = new("cross", static (u, v) => float.Min(float.Abs(u), float.Abs(v)) switch { var m => 1f - ((1f - m) * (1f - m)) });

    [UseDelegateFromConstructor]
    public partial float Spot(float u, float v);

    internal float Threshold(Vector2 cell) => ((2f * (cell - Vector2.Round(cell, MidpointRounding.ToNegativeInfinity))) - Vector2.One) switch { var uv => Spot(uv.X, uv.Y) };

    private static double Quarter(double rho, double width, double height) =>
        double.Min(rho, width) switch {
            var reach => double.Min(double.Sqrt(double.Max((rho * rho) - (height * height), 0d)), reach) switch { var knee => (height * knee) + Arc(rho, reach) - Arc(rho, knee) },
        };

    private static double Arc(double rho, double x) => double.Sqrt((rho * rho) - (x * x)) switch { var y => ((x * y) + (rho * rho * double.Atan2(x, y))) / 2d };
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Separation {
    public static readonly Separation Mono = new("mono", static (rgb, weights) => new Vector4(Vector3.Zero, 1f - Easing.Saturate(Vector3.Dot(weights, rgb))));
    public static readonly Separation Process = new("process", static (rgb, _) => Vector3.Clamp(rgb, Vector3.Zero, Vector3.One) switch {
        var saturated => float.Max(saturated.X, float.Max(saturated.Y, saturated.Z)) switch {
            > 0f and var most => new Vector4(Vector3.One - (saturated / most), 1f - most),
            _ => Vector4.UnitW,
        },
    });

    [UseDelegateFromConstructor]
    public partial Vector4 Levels(Vector3 rgb, Vector3 weights);
}

public sealed record Halftone(
    ScreenShape Shape, CellSize Size, Separation Separation, InkScreen Cyan, InkScreen Magenta, InkScreen Yellow, InkScreen Key,
    AxisFraction CrossOnset, Swatch Ink, Gated<Swatch> Paper, BlendingMode Mode)
    : IStateRecord<Halftone, HalftoneParameter, InvalidStylize>, IPixelStage<Halftone> {
    public static Halftone Default { get; } = new(
        ScreenShape.Round, CellSize.Standard, Separation.Mono, InkScreen.Cyan, InkScreen.Magenta, InkScreen.Yellow, InkScreen.Key,
        AxisFraction.MaxValue, Swatch.Black, new(Enabled: true, Swatch.White), BlendingMode.Mix);

    public static Option<PixelPass> Pass(Halftone state, PassContext context) =>
        Some(Blend.Mixed(new PixelPass.Pointwise(Print(state, context)), state.Mode, Mix.Full, [], context.Extent));

    private static Action<Span<Vector4>, int, int> Print(Halftone state, PassContext context) {
        (float pitch, int height, Vector3 weights) = (state.Size.Pixels(context.Extent), context.Extent.Height, context.Working.Luminance);
        Matrix3x2 centered = Matrix3x2.CreateTranslation(context.Extent.Width / -2f, height / -2f);
        Screen Turned(InkScreen ink, float turn, float onset) =>
            new(centered * Matrix3x2.CreateRotation(ink.Angle + turn) * Matrix3x2.CreateScale(1f / pitch) * Matrix3x2.CreateTranslation(ink.U, ink.V), state.Shape, onset, 1f / (1f - onset));
        Plate Inked(InkScreen ink, Vector3 absorbance, Vector4 lane) =>
            Turned(ink, 0f, 0f) switch { var screen => new(state.CrossOnset < AxisFraction.MaxValue ? [screen, Turned(ink, float.Pi / 2f, state.CrossOnset)] : [screen], absorbance, lane) };
        Arr<Plate> plates = [
            Inked(state.Cyan, Vector3.UnitX, Vector4.UnitX), Inked(state.Magenta, Vector3.UnitY, Vector4.UnitY), Inked(state.Yellow, Vector3.UnitZ, Vector4.UnitZ),
            Inked(state.Key, Vector3.One - state.Ink.Display.AsVector3(), Vector4.UnitW)];
        Option<Vector3> paper = state.Paper.Active.Map(static swatch => swatch.Display.AsVector3());
        return (row, column, line) => {
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(
                    paper.IfNone(row[x].AsVector3()) * Plate.Transmitted(plates, new(column + x + 0.5f, height - line - 0.5f), state.Separation.Levels(row[x].AsVector3(), weights)), row[x].W);
        };
    }

    private readonly record struct Screen(Matrix3x2 Frame, ScreenShape Shape, float Onset, float Gain) {
        public float Coverage(Vector2 p, float level) {
            (float t, float k) = (Shape.Threshold(Vector2.Transform(p, Frame)), Easing.Saturate((level - Onset) * Gain));
            float w = float.Abs(Shape.Threshold(Vector2.Transform(p + Vector2.UnitX, Frame)) - t) + float.Abs(Shape.Threshold(Vector2.Transform(p + Vector2.UnitY, Frame)) - t);
            return w > 0f ? Easing.Saturate((((k * (1f + w)) - (w / 2f) - t) / w) + 0.5f) : t < k ? 1f : 0f;
        }
    }

    private readonly record struct Plate(Arr<Screen> Screens, Vector3 Absorbance, Vector4 Lane) {
        public static Vector3 Transmitted(Arr<Plate> plates, Vector2 p, Vector4 levels) {
            Vector3 transmitted = Vector3.One;
            foreach (Plate plate in plates.AsSpan())
                transmitted *= Vector3.One - ((1f - plate.Clear(p, Vector4.Dot(levels, plate.Lane))) * plate.Absorbance);
            return transmitted;
        }

        private float Clear(Vector2 p, float level) {
            float clear = 1f;
            foreach (Screen screen in level > 0f ? Screens.AsSpan() : [])
                clear *= 1f - screen.Coverage(p, level);
            return clear;
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HalftoneParameter : IStateParameter<Halftone> {
    public static readonly HalftoneParameter Shape = new("shape", new StateParameter<Halftone>.Choice<ScreenShape, InvalidStylize>(
        Lens<Halftone, ScreenShape>.New(static halftone => halftone.Shape, static shape => halftone => halftone with { Shape = shape })));
    public static readonly HalftoneParameter Size = new("size", new StateParameter<Halftone>.Bounded<CellSize, float, InvalidStylize>(
        Lens<Halftone, CellSize>.New(static halftone => halftone.Size, static size => halftone => halftone with { Size = size }),
        CellSize.Presentation with { Soft = (1f / (2f * 64f) * ReferenceFrame.GreaterSide, CellSize.MaxValue) }));
    public static readonly HalftoneParameter Separation = new("separation", new StateParameter<Halftone>.Choice<Separation, InvalidStylize>(
        Lens<Halftone, Separation>.New(static halftone => halftone.Separation, static separation => halftone => halftone with { Separation = separation })));
    public static readonly HalftoneParameter Cyan = new("cyan", new StateParameter<Halftone>.Record<InkScreen>(
        Lens<Halftone, InkScreen>.New(static halftone => halftone.Cyan, static ink => halftone => halftone with { Cyan = ink })));
    public static readonly HalftoneParameter Magenta = new("magenta", new StateParameter<Halftone>.Record<InkScreen>(
        Lens<Halftone, InkScreen>.New(static halftone => halftone.Magenta, static ink => halftone => halftone with { Magenta = ink })));
    public static readonly HalftoneParameter Yellow = new("yellow", new StateParameter<Halftone>.Record<InkScreen>(
        Lens<Halftone, InkScreen>.New(static halftone => halftone.Yellow, static ink => halftone => halftone with { Yellow = ink })));
    public static readonly HalftoneParameter KeyScreen = new("key", new StateParameter<Halftone>.Record<InkScreen>(
        Lens<Halftone, InkScreen>.New(static halftone => halftone.Key, static ink => halftone => halftone with { Key = ink })));
    public static readonly HalftoneParameter CrossOnset = new("cross-onset", new StateParameter<Halftone>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Halftone, AxisFraction>.New(static halftone => halftone.CrossOnset, static onset => halftone => halftone with { CrossOnset = onset }), new()));
    public static readonly HalftoneParameter Ink = new("ink", new StateParameter<Halftone>.Color(
        Lens<Halftone, Swatch>.New(static halftone => halftone.Ink, static ink => halftone => halftone with { Ink = ink })));
    public static readonly HalftoneParameter Paper = new("paper", new StateParameter<Halftone>.OptionalColor(
        Lens<Halftone, Gated<Swatch>>.New(static halftone => halftone.Paper, static paper => halftone => halftone with { Paper = paper })));
    public static readonly HalftoneParameter Mode = new("mode", new StateParameter<Halftone>.Choice<BlendingMode, InvalidGrade>(
        Lens<Halftone, BlendingMode>.New(static halftone => halftone.Mode, static mode => halftone => halftone with { Mode = mode })));

    public StateParameter<Halftone> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MosaicCell {
    public static readonly MosaicCell Square = new("square");
    public static readonly MosaicCell Cellular = new("cellular");
}

public sealed record Mosaic(MosaicCell Cell, CellSize Size, Cellular Cells, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Mosaic, MosaicParameter, InvalidStylize>, IPixelStage<Mosaic> {
    public static Mosaic Default { get; } = new(MosaicCell.Square, CellSize.Pixelate, Cellular.Default, Seed.MinValue, Timing.Default with { Pace = Pace.MinValue }, Hold.MinValue);

    public static Option<PixelPass> Pass(Mosaic state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(par(state.Cell.Map(square: Blocks, cellular: Sites), state, context)));

    private static Fin<Unit> Blocks(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        float cell = state.Size.Pixels(context.Extent);
        using Mat header = frame.Header();
        using Mat alpha = new();
        CvInvoke.ExtractChannel(header, alpha, 3);
        using Mat means = CellGrid.Means(header, CellGrid.Fit(context.Extent, cell, cell));
        CvInvoke.Resize(means, header, header.Size, 0d, 0d, Inter.Nearest);
        CvInvoke.InsertChannel(alpha, header, 3);
        progress.Report(frame.Size.Height);
        return unit;
    }

    private static Fin<Unit> Sites(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        using Mat header = frame.Header();
        using Mat copy = header.Clone();
        (int width, int height, float pitch) = (context.Extent.Width, context.Extent.Height, state.Size.Pixels(context.Extent));
        (Vector2 center, uint field) = (new Vector2(width, height) / 2f, CoordinateHash.Field(NoiseStream.Mosaic, state.Seed, state.Hold.Period(state.Timing.At(context))));
        return new PixelPass.Pointwise((row, column, line) => {
            ReadOnlySpan<Vector4> source = copy.GetSpan<Vector4>();
            for (int x = 0; x < row.Length; x++) {
                Vector4 point = new((new Vector2(column + x + 0.5f, height - line - 0.5f) - center) / pitch, 0f, 0f);
                Vector2 site = center + (pitch * NoiseDimensions.Two.F1(point, Octaves.Plain, state.Cells, AxisFraction.MinValue, field).Site.AsVector2());
                row[x] = new Vector4(source[(int.Clamp((int)float.Floor(site.Y), 0, height - 1) * width) + int.Clamp((int)float.Floor(site.X), 0, width - 1)].AsVector3(), row[x].W);
            }
        }).Run(frame, progress);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MosaicParameter : IStateParameter<Mosaic> {
    public static readonly MosaicParameter Cell = new("cell", new StateParameter<Mosaic>.Choice<MosaicCell, InvalidStylize>(
        Lens<Mosaic, MosaicCell>.New(static mosaic => mosaic.Cell, static cell => mosaic => mosaic with { Cell = cell })));
    public static readonly MosaicParameter Size = new("size", new StateParameter<Mosaic>.Bounded<CellSize, float, InvalidStylize>(
        Lens<Mosaic, CellSize>.New(static mosaic => mosaic.Size, static size => mosaic => mosaic with { Size = size }),
        CellSize.Presentation with { Soft = (CellSize.MinValue, 0.1f * ReferenceFrame.GreaterSide) }));
    public static readonly MosaicParameter Cells = new("cells", new StateParameter<Mosaic>.Record<Cellular>(
        Lens<Mosaic, Cellular>.New(static mosaic => mosaic.Cells, static cells => mosaic => mosaic with { Cells = cells })));
    public static readonly MosaicParameter Seed = new("seed", new StateParameter<Mosaic>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Mosaic, Seed>.New(static mosaic => mosaic.Seed, static seed => mosaic => mosaic with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly MosaicParameter Timing = new("timing", new StateParameter<Mosaic>.Record<Timing>(
        Lens<Mosaic, Timing>.New(static mosaic => mosaic.Timing, static timing => mosaic => mosaic with { Timing = timing })));
    public static readonly MosaicParameter Hold = new("hold", new StateParameter<Mosaic>.Bounded<Hold, float, InvalidGenerator>(
        Lens<Mosaic, Hold>.New(static mosaic => mosaic.Hold, static hold => mosaic => mosaic with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<Mosaic> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlyphColoring {
    public static readonly GlyphColoring Input = new("input");
    public static readonly GlyphColoring Text = new("text");
    public static readonly GlyphColoring PerCell = new("per-cell");
}

public sealed record GlyphCells(
    Glyphs Glyphs, HersheyFonts Font, CellSize Size, GlyphColoring Coloring, Swatch Text, Swatch Background, Mix Saturation,
    AxisFraction Jitter, Seed Seed, Timing Timing, Hold Hold, BlendingMode Mode)
    : IStateRecord<GlyphCells, GlyphCellsParameter, InvalidStylize>, IPixelStage<GlyphCells> {
    public static GlyphCells Default { get; } = new(
        Glyphs.Symbols, HersheyFonts.Simplex, CellSize.Glyph, GlyphColoring.Input, Swatch.White, Swatch.Black, Mix.Full,
        AxisFraction.Create(0.1f), Seed.MinValue, Timing.Default, Hold.Create(6f / 24f), BlendingMode.Mix);

    public static Option<PixelPass> Pass(GlyphCells state, PassContext context) =>
        Some(Blend.Mixed(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)), state.Mode, Mix.Full, [], context.Extent));

    private static Fin<Unit> Kernel(GlyphCells state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (Mat plane, Arr<int> ranked, (int Columns, int Rows) grid, Size tile) = Atlas(state.Glyphs, state.Font, state.Size.Pixels(context.Extent), context.Extent);
        using Mat atlas = plane;
        using Mat header = frame.Header();
        using Mat means = CellGrid.Means(header, grid);
        using Mat ranks = new(grid.Rows, grid.Columns, DepthType.Cv32S, 1);
        uint field = CoordinateHash.Field(NoiseStream.GlyphCells, state.Seed, state.Hold.Period(state.Timing.At(context)));
        (uint gray, uint tint, Vector3 weights) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), context.Working.Luminance);
        Span<Vector4> cells = means.GetSpan<Vector4>();
        Span<int> tiles = ranks.GetSpan<int>();
        for (int index = 0; index < cells.Length; index++) {
            Vector4 at = new(index % grid.Columns, index / grid.Columns, 0f, 0f);
            (Vector4 u, Vector4 c) = (NoiseFunctions.White(at, gray), NoiseFunctions.White(at, tint));
            float level = Easing.Saturate(Easing.Saturate(Vector3.Dot(weights, cells[index].AsVector3())) + (state.Jitter * (u.X - 0.5f)));
            tiles[index] = ranked[int.Min(ranked.Count - 1, (int)(level * ranked.Count))];
            cells[index] = new Vector4(state.Coloring.Switch(
                (Mean: cells[index].AsVector3(), Gray: u.Y, Tint: c.AsVector3(), State: state),
                input: static s => s.Mean,
                text: static s => s.State.Text.Display.AsVector3(),
                perCell: static s => Vector3.Lerp(new Vector3(s.Gray), s.Tint, s.State.Saturation)), cells[index].W);
        }
        (int width, int height, int stride, Vector3 background) = (context.Extent.Width, context.Extent.Height, ranked.Count * tile.Width, state.Background.Display.AsVector3());
        return new PixelPass.Pointwise((row, column, line) => {
            ReadOnlySpan<byte> glyphs = atlas.GetSpan<byte>();
            ReadOnlySpan<Vector4> colors = means.GetSpan<Vector4>();
            ReadOnlySpan<int> indices = ranks.GetSpan<int>();
            (int cy, int ry) = int.DivRem((height - 1 - line) * grid.Rows, height);
            for (int x = 0; x < row.Length; x++) {
                (int cx, int rx) = int.DivRem((column + x) * grid.Columns, width);
                int cell = (cy * grid.Columns) + cx;
                float g = glyphs[(ry * tile.Height / height * stride) + (indices[cell] * tile.Width) + (rx * tile.Width / width)] / (float)byte.MaxValue;
                row[x] = new Vector4(Vector3.Lerp(background, colors[cell].AsVector3(), g), row[x].W);
            }
        }).Run(frame, progress);
    }

    private static (Mat Plane, Arr<int> Ranked, (int Columns, int Rows) Grid, Size Tile) Atlas(Glyphs glyphs, HersheyFonts font, float cell, PixelExtent extent) {
        string set = glyphs;
        (Size Size, int Baseline)[] measures = [.. set.Select(glyph => {
            int baseline = 0;
            return (CvInvoke.GetTextSize(glyph.ToString(), font, 1d, 1, ref baseline), baseline);
        })];
        (int widest, int tallest) = (measures.Max(static measure => measure.Size.Width), measures.Max(static measure => measure.Size.Height + measure.Baseline));
        (int Columns, int Rows) grid = CellGrid.Fit(extent, cell * widest / tallest, cell);
        Size tile = new((extent.Width + grid.Columns - 1) / grid.Columns, (extent.Height + grid.Rows - 1) / grid.Rows);
        double scale = double.Min((double)tile.Height / tallest, (double)tile.Width / widest);
        Mat plane = Mat.Zeros(tile.Height, set.Length * tile.Width, DepthType.Cv8U, 1);
        for (int i = 0; i < set.Length; i++) {
            CvInvoke.PutText(
                plane, set[i].ToString(), new Point((i * tile.Width) + (int)double.Round((tile.Width - (measures[i].Size.Width * scale)) / 2d, MidpointRounding.ToEven), tile.Height - (int)double.Ceiling(measures[i].Baseline * scale)),
                font, scale, new MCvScalar(byte.MaxValue), int.Max(1, (int)double.Round(scale, MidpointRounding.ToEven)), LineType.AntiAlias);
        }
        return (plane, [.. Enumerable.Range(0, set.Length).OrderBy(i => {
            using Mat glyph = new(plane, new Rectangle(i * tile.Width, 0, tile.Width, tile.Height));
            return CvInvoke.Mean(glyph).V0;
        })], grid, tile);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlyphCellsParameter : IStateParameter<GlyphCells> {
    public static readonly GlyphCellsParameter Glyphs = new("glyphs", new StateParameter<GlyphCells>.Keyed<Glyphs, string, InvalidStylize>(
        Lens<GlyphCells, Glyphs>.New(static cells => cells.Glyphs, static glyphs => cells => cells with { Glyphs = glyphs })));
    public static readonly GlyphCellsParameter Font = new("font", new StateParameter<GlyphCells>.Enumerated<HersheyFonts>(
        Lens<GlyphCells, HersheyFonts>.New(static cells => cells.Font, static font => cells => cells with { Font = font })));
    public static readonly GlyphCellsParameter Size = new("size", new StateParameter<GlyphCells>.Bounded<CellSize, float, InvalidStylize>(
        Lens<GlyphCells, CellSize>.New(static cells => cells.Size, static size => cells => cells with { Size = size }), CellSize.Presentation with { Soft = (1f / 128f, CellSize.MaxValue) }));
    public static readonly GlyphCellsParameter Coloring = new("coloring", new StateParameter<GlyphCells>.Choice<GlyphColoring, InvalidStylize>(
        Lens<GlyphCells, GlyphColoring>.New(static cells => cells.Coloring, static coloring => cells => cells with { Coloring = coloring })));
    public static readonly GlyphCellsParameter Text = new("text", new StateParameter<GlyphCells>.Color(
        Lens<GlyphCells, Swatch>.New(static cells => cells.Text, static text => cells => cells with { Text = text })));
    public static readonly GlyphCellsParameter Background = new("background", new StateParameter<GlyphCells>.Color(
        Lens<GlyphCells, Swatch>.New(static cells => cells.Background, static background => cells => cells with { Background = background })));
    public static readonly GlyphCellsParameter Saturation = new("saturation", new StateParameter<GlyphCells>.Bounded<Mix, float, InvalidGrade>(
        Lens<GlyphCells, Mix>.New(static cells => cells.Saturation, static saturation => cells => cells with { Saturation = saturation }), new()));
    public static readonly GlyphCellsParameter Jitter = new("jitter", new StateParameter<GlyphCells>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<GlyphCells, AxisFraction>.New(static cells => cells.Jitter, static jitter => cells => cells with { Jitter = jitter }), new()));
    public static readonly GlyphCellsParameter Seed = new("seed", new StateParameter<GlyphCells>.Bounded<Seed, int, InvalidGenerator>(
        Lens<GlyphCells, Seed>.New(static cells => cells.Seed, static seed => cells => cells with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly GlyphCellsParameter Timing = new("timing", new StateParameter<GlyphCells>.Record<Timing>(
        Lens<GlyphCells, Timing>.New(static cells => cells.Timing, static timing => cells => cells with { Timing = timing })));
    public static readonly GlyphCellsParameter Hold = new("hold", new StateParameter<GlyphCells>.Bounded<Hold, float, InvalidGenerator>(
        Lens<GlyphCells, Hold>.New(static cells => cells.Hold, static hold => cells => cells with { Hold = hold }), Generators.Hold.Presentation with { Soft = (Generators.Hold.MinValue, 120f / 24f) }));
    public static readonly GlyphCellsParameter Mode = new("mode", new StateParameter<GlyphCells>.Choice<BlendingMode, InvalidGrade>(
        Lens<GlyphCells, BlendingMode>.New(static cells => cells.Mode, static mode => cells => cells with { Mode = mode })));

    public StateParameter<GlyphCells> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SortKey {
    public static readonly SortKey Luma = new("luma", static (color, weights) => Vector3.Dot(weights, color.AsVector3()));
    public static readonly SortKey Hue = new("hue", static (color, _) => RangeAxis.Hue.Coordinate(Hsy.From(color)));
    public static readonly SortKey Saturation = new("saturation", static (color, _) => RangeAxis.Saturation.Coordinate(Hsy.From(color)));

    [UseDelegateFromConstructor]
    public partial float Level(Vector4 color, Vector3 weights);
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SortDirection {
    public static readonly SortDirection Right = new("right", static (key, other) => key.CompareTo(other));
    public static readonly SortDirection Left = new("left", static (key, other) => other.CompareTo(key));
    public static readonly SortDirection Down = new("down", static (key, other) => key.CompareTo(other));
    public static readonly SortDirection Up = new("up", static (key, other) => other.CompareTo(key));

    [UseDelegateFromConstructor]
    public partial int Order(float key, float other);
}

public sealed record PixelSort(SortKey Key, AxisFraction Low, AxisFraction High, SortDirection Direction)
    : IStateRecord<PixelSort, PixelSortParameter, InvalidStylize>, IPixelStage<PixelSort> {
    public static PixelSort Default { get; } = new(SortKey.Luma, AxisFraction.MinValue, AxisFraction.Create(0.3f), SortDirection.Right);

    public static Option<PixelPass> Pass(PixelSort state, PassContext context) {
        PixelPass.Pointwise rows = new((row, _, _) => state.Runs(row, context.Working.Luminance));
        Fin<Unit> Columns(PixelFrame frame, IProgress<int> progress) {
            using Mat header = frame.Header();
            using Mat lines = new();
            CvInvoke.Transpose(header, lines);
            Span<Vector4> pixels = lines.GetSpan<Vector4>();
            for (int y = 0; y < lines.Rows; y++)
                rows.Row(pixels.Slice(y * lines.Cols, lines.Cols), 0, y);
            CvInvoke.Transpose(lines, header);
            progress.Report(frame.Size.Height);
            return unit;
        }
        return Some<PixelPass>(new PixelPass.Frame(state.Direction.Map(right: rows.Run, left: rows.Run, down: Columns, up: Columns)));
    }

    private void Runs(Span<Vector4> row, Vector3 weights) {
        using SpanOwner<float> owner = SpanOwner<float>.Allocate(row.Length);
        Span<float> keys = owner.Span;
        for (int x = 0; x < row.Length; x++)
            keys[x] = Key.Level(row[x], weights);
        bool Holds(float key) => Low <= High ? key >= Low && key <= High : key >= Low || key <= High;
        for (int x = 0, start = 0; x <= row.Length; x++) {
            if (x < row.Length && row[x].W > 0f && Holds(keys[x]))
                continue;
            keys[start..x].Sort(row[start..x], Direction.Order);
            start = x + 1;
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PixelSortParameter : IStateParameter<PixelSort> {
    public static readonly PixelSortParameter SortKey = new("key", new StateParameter<PixelSort>.Choice<SortKey, InvalidStylize>(
        Lens<PixelSort, SortKey>.New(static sort => sort.Key, static key => sort => sort with { Key = key })));
    public static readonly PixelSortParameter Low = new("low", new StateParameter<PixelSort>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<PixelSort, AxisFraction>.New(static sort => sort.Low, static low => sort => sort with { Low = low }), new()));
    public static readonly PixelSortParameter High = new("high", new StateParameter<PixelSort>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<PixelSort, AxisFraction>.New(static sort => sort.High, static high => sort => sort with { High = high }), new()));
    public static readonly PixelSortParameter Direction = new("direction", new StateParameter<PixelSort>.Choice<SortDirection, InvalidStylize>(
        Lens<PixelSort, SortDirection>.New(static sort => sort.Direction, static direction => sort => sort with { Direction = direction })));

    public StateParameter<PixelSort> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class CellGrid {
    public static (int Columns, int Rows) Fit(PixelExtent extent, float width, float height) =>
        (int.Max(1, (int)float.Round(extent.Width / width, MidpointRounding.ToEven)), int.Max(1, (int)float.Round(extent.Height / height, MidpointRounding.ToEven)));

    public static Mat Means(Mat header, (int Columns, int Rows) grid) {
        using Mat premultiplied = header.Clone();
        foreach (ref Vector4 pixel in premultiplied.GetSpan<Vector4>())
            pixel = new Vector4(pixel.AsVector3() * pixel.W, pixel.W);
        Mat means = new();
        CvInvoke.Resize(premultiplied, means, new Size(grid.Columns, grid.Rows), 0d, 0d, Inter.Area);
        foreach (ref Vector4 cell in means.GetSpan<Vector4>())
            cell = cell.W > 0f ? new Vector4(cell.AsVector3() / cell.W, cell.W) : Vector4.Zero;
        return means;
    }
}
