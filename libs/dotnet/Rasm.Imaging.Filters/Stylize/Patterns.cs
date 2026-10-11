using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using PeterO.Numbers;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct CellOffset : IMinMaxValue<CellOffset> {
    public static CellOffset MinValue => Neutral;
    public static CellOffset MaxValue { get; } = new(float.BitDecrement(1f));
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

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
    public static readonly InkScreenParameter Angle = new("angle", StateParameter.Bounded<InkScreen, SignedAngle, float, InvalidPixelValue>(Lens<InkScreen, SignedAngle>.New(static ink => ink.Angle, static angle => ink => ink with { Angle = angle }), SignedAngle.Presentation));
    public static readonly InkScreenParameter U = new("u", StateParameter.Bounded<InkScreen, CellOffset, float, InvalidStylize>(Lens<InkScreen, CellOffset>.New(static ink => ink.U, static u => ink => ink with { U = u }), CellOffset.Presentation));
    public static readonly InkScreenParameter V = new("v", StateParameter.Bounded<InkScreen, CellOffset, float, InvalidStylize>(Lens<InkScreen, CellOffset>.New(static ink => ink.V, static v => ink => ink with { V = v }), CellOffset.Presentation));

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

    internal float Threshold((ERational U, ERational V) cell) =>
        Spot(NoiseFunctions.Split(cell.U).Local.Multiply(2).Subtract(1).ToSingle(), NoiseFunctions.Split(cell.V).Local.Multiply(2).Subtract(1).ToSingle());

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
    public static readonly Separation Mono = new("mono", static (rgb, weights) => new Vector4(Vector3.Zero, 1f - (float)double.Clamp((weights.X * rgb.X) + (weights.Y * rgb.Y) + (weights.Z * rgb.Z), 0d, 1d)));
    public static readonly Separation Process = new("process", static (rgb, _) => Vector3.Clamp(rgb, Vector3.Zero, Vector3.One) switch {
        var saturated => float.Max(saturated.X, float.Max(saturated.Y, saturated.Z)) switch {
            > 0f and var most => new Vector4(Vector3.One - (saturated / most), 1f - most),
            _ => Vector4.UnitW,
        },
    });

    [UseDelegateFromConstructor]
    public partial Vector4 Levels(Vector3 rgb, (double X, double Y, double Z) weights);
}

public sealed record Halftone(
    ScreenShape Shape, ShortSideExtent Size, Separation Separation, InkScreen Cyan, InkScreen Magenta, InkScreen Yellow, InkScreen Key,
    AxisFraction CrossOnset, Swatch Ink, Gated<Swatch> Paper, BlendingMode Mode)
    : IStateRecord<Halftone, HalftoneParameter, InvalidStylize>, IPixelStage<Halftone> {
    public static Halftone Default { get; } = new(
        ScreenShape.Round, ShortSideExtent.Create(1f / (2f * 2f * 4f) * ReferenceFrame.GreaterSide), Separation.Mono, InkScreen.Cyan, InkScreen.Magenta, InkScreen.Yellow, InkScreen.Key,
        AxisFraction.MaxValue, Swatch.Black, new(Enabled: true, Swatch.White), BlendingMode.Mix);

    public static Option<PixelPass> Pass(Halftone state, PassContext context) =>
        Some(Blend.Mixed(new PixelPass.Pointwise(Print(state, context)), state.Mode, Mix.Full, []));

    private static Action<Span<Vector4>, long, long> Print(Halftone state, PassContext context) {
        (ERational pitch, int height, (double X, double Y, double Z) weights) = (ERational.FromSingle(state.Size).Multiply(context.Extent.ShortSide), context.Extent.Height, context.Working.Luminance);
        Screen Turned(InkScreen ink, float turn, float onset) =>
            new(Matrix3x2.CreateRotation(ink.Angle + turn) * Matrix3x2.CreateTranslation(ink.U, ink.V), pitch, state.Shape, onset, 1f / (1f - onset));
        Plate Inked(InkScreen ink, Vector3 absorbance, Vector4 lane) =>
            Turned(ink, 0f, 0f) switch { var screen => new(state.CrossOnset < AxisFraction.MaxValue ? [screen, Turned(ink, float.Pi / 2f, state.CrossOnset)] : [screen], absorbance, lane) };
        Arr<Plate> plates = [
            Inked(state.Cyan, Vector3.UnitX, Vector4.UnitX), Inked(state.Magenta, Vector3.UnitY, Vector4.UnitY), Inked(state.Yellow, Vector3.UnitZ, Vector4.UnitZ),
            Inked(state.Key, Vector3.One - state.Ink.Display.AsVector3(), Vector4.UnitW)];
        Option<Vector3> paper = state.Paper.Active.Map(static swatch => swatch.Display.AsVector3());
        return (row, column, line) => {
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(
                    paper.IfNone(row[x].AsVector3()) * Plate.Transmitted(
                        plates, (column + x + 0.5d - (context.Extent.Width / 2d), height - line - 0.5d - (height / 2d)),
                        state.Separation.Levels(row[x].AsVector3(), weights)), row[x].W);
        };
    }

    private readonly record struct Screen(Matrix3x2 Frame, ERational Pitch, ScreenShape Shape, float Onset, float Gain) {
        public float Coverage((double X, double Y) p, float level) {
            (ERational x, ERational y) = (ERational.FromDouble(p.X), ERational.FromDouble(p.Y));
            (ERational acrossX, ERational acrossY, ERational downX, ERational downY) = (ERational.FromSingle(Frame.M11).Divide(Pitch), ERational.FromSingle(Frame.M12).Divide(Pitch),
                ERational.FromSingle(Frame.M21).Divide(Pitch), ERational.FromSingle(Frame.M22).Divide(Pitch));
            (ERational u, ERational v) = (x.Multiply(acrossX).Add(y.Multiply(downX)).Add(ERational.FromSingle(Frame.M31)), x.Multiply(acrossY).Add(y.Multiply(downY)).Add(ERational.FromSingle(Frame.M32)));
            (float t, float k) = (Shape.Threshold((u, v)), Easing.Saturate((level - Onset) * Gain));
            float w = float.Abs(Shape.Threshold((u.Add(acrossX), v.Add(acrossY))) - t) + float.Abs(Shape.Threshold((u.Add(downX), v.Add(downY))) - t);
            return w > 0f ? Easing.Saturate((((k * (1f + w)) - (w / 2f) - t) / w) + 0.5f) : t < k ? 1f : 0f;
        }
    }

    private readonly record struct Plate(Arr<Screen> Screens, Vector3 Absorbance, Vector4 Lane) {
        public static Vector3 Transmitted(Arr<Plate> plates, (double X, double Y) p, Vector4 levels) {
            Vector3 transmitted = Vector3.One;
            foreach (Plate plate in plates.AsSpan())
                transmitted *= Vector3.One - ((1f - plate.Clear(p, Vector4.Dot(levels, plate.Lane))) * plate.Absorbance);
            return transmitted;
        }

        private float Clear((double X, double Y) p, float level) {
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
    public static readonly HalftoneParameter Size = new("size", StateParameter.Bounded<Halftone, ShortSideExtent, float, InvalidPixelValue>(Lens<Halftone, ShortSideExtent>.New(static halftone => halftone.Size, static size => halftone => halftone with { Size = size }),
    ShortSideExtent.Presentation with { Soft = (1f / (2f * 64f) * ReferenceFrame.GreaterSide, 0.25f) }));
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
    public static readonly HalftoneParameter CrossOnset = new("cross-onset", StateParameter.Bounded<Halftone, AxisFraction, float, InvalidGrade>(Lens<Halftone, AxisFraction>.New(static halftone => halftone.CrossOnset, static onset => halftone => halftone with { CrossOnset = onset }), new()));
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

public sealed record Mosaic(MosaicCell Cell, ShortSideExtent Size, Cellular Cells, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Mosaic, MosaicParameter, InvalidStylize>, IPixelStage<Mosaic> {
    public static Mosaic Default { get; } = new(MosaicCell.Square, ShortSideExtent.Create(0.1f * 0.05f * ReferenceFrame.GreaterSide), Cellular.Default, Seed.MinValue, Timing.Default with { Pace = Pace.MinValue }, Hold.MinValue);

    public static Option<PixelPass> Pass(Mosaic state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(par(state.Cell.Map(square: Blocks, cellular: Sites), state, context)));

    private static Fin<Unit> Blocks(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
        ERational cell = ERational.FromSingle(state.Size).Multiply(context.Extent.ShortSide);
        (EInteger Columns, EInteger Rows) grid = CellGrid.Fit(context.Extent, cell, cell);
        using Mat source = header.Clone();
        Func<(EInteger Column, EInteger Row), Vector3> mean = CellGrid.Means(frame, source, grid);
        Func<(EInteger Column, EInteger Row), Vector3> colors = memoUnsafe(mean);
        return new PixelPass.Pointwise((row, column, line) => {
            EInteger cy = CellGrid.Coordinate(context.Extent.Height - 1L - line, context.Extent.Height, grid.Rows).Cell;
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(colors((CellGrid.Coordinate(column + x, context.Extent.Width, grid.Columns).Cell, cy)), row[x].W);
        }).Run(frame, progress);
    });

    private static unsafe Fin<Unit> Sites(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
        using Mat copy = header.Clone();
        (ERational centerX, ERational centerY, ERational pitch) = (ERational.FromInt32(context.Extent.Width).Divide(2), ERational.FromInt32(context.Extent.Height).Divide(2),
            ERational.FromSingle(state.Size).Multiply(context.Extent.ShortSide));
        uint field = CoordinateHash.Field(NoiseStream.Mosaic, state.Seed, state.Hold.Period(state.Timing.At(context)));
        return new PixelPass.Pointwise((row, column, line) => {
            ReadOnlySpan2D<Vector4> source = new Span<Vector4>(copy.DataPointer.ToPointer(), copy.Rows * copy.Cols).AsSpan2D(copy.Rows, copy.Cols);
            for (int x = 0; x < row.Length; x++) {
                (ERational X, ERational Y) = NoiseFunctions.F1(
                    ERational.FromInt64((2L * (column + x)) + 1L).Divide(2).Subtract(centerX).Divide(pitch),
                    ERational.FromInt64((2L * (context.Extent.Height - line)) - 1L).Divide(2).Subtract(centerY).Divide(pitch), state.Cells, field).Site;
                Vector4 color = source.Sample(
                    (ERational.FromEInteger(NoiseFunctions.Split(X.Multiply(pitch).Add(centerX)).Cell).ToDouble() + 0.5d,
                        ERational.FromEInteger(NoiseFunctions.Split(Y.Multiply(pitch).Add(centerY)).Cell).ToDouble() + 0.5d),
                    (WrapMode.Clamp, WrapMode.Clamp), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight), Some((frame.Origin, frame.Extent)));
                row[x] = new Vector4(color.AsVector3(), row[x].W);
            }
        }).Run(frame, progress);
    });
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MosaicParameter : IStateParameter<Mosaic> {
    public static readonly MosaicParameter Cell = new("cell", new StateParameter<Mosaic>.Choice<MosaicCell, InvalidStylize>(
        Lens<Mosaic, MosaicCell>.New(static mosaic => mosaic.Cell, static cell => mosaic => mosaic with { Cell = cell })));
    public static readonly MosaicParameter Size = new("size", StateParameter.Bounded<Mosaic, ShortSideExtent, float, InvalidPixelValue>(Lens<Mosaic, ShortSideExtent>.New(static mosaic => mosaic.Size, static size => mosaic => mosaic with { Size = size }),
    ShortSideExtent.Presentation with { Soft = (0.001f, 0.1f * ReferenceFrame.GreaterSide) }));
    public static readonly MosaicParameter Cells = new("cells", new StateParameter<Mosaic>.Record<Cellular>(
        Lens<Mosaic, Cellular>.New(static mosaic => mosaic.Cells, static cells => mosaic => mosaic with { Cells = cells })));
    public static readonly MosaicParameter Seed = new("seed", StateParameter.Bounded<Mosaic, Seed, int, InvalidGenerator>(Lens<Mosaic, Seed>.New(static mosaic => mosaic.Seed, static seed => mosaic => mosaic with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly MosaicParameter Timing = new("timing", new StateParameter<Mosaic>.Record<Timing>(
        Lens<Mosaic, Timing>.New(static mosaic => mosaic.Timing, static timing => mosaic => mosaic with { Timing = timing })));
    public static readonly MosaicParameter Hold = new("hold", StateParameter.Bounded<Mosaic, Hold, float, InvalidGenerator>(Lens<Mosaic, Hold>.New(static mosaic => mosaic.Hold, static hold => mosaic => mosaic with { Hold = hold }), Generators.Hold.Presentation));

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
    Glyphs Glyphs, HersheyFonts Font, ShortSideExtent Size, GlyphColoring Coloring, Swatch Text, Swatch Background, Mix Saturation,
    AxisFraction Jitter, Seed Seed, Timing Timing, Hold Hold, BlendingMode Mode)
    : IStateRecord<GlyphCells, GlyphCellsParameter, InvalidStylize>, IPixelStage<GlyphCells> {
    public static GlyphCells Default { get; } = new(
        Glyphs.Symbols, HersheyFonts.Simplex, ShortSideExtent.Create(1f / 24f), GlyphColoring.Input, Swatch.White, Swatch.Black, Mix.Full,
        AxisFraction.Create(0.1f), Seed.MinValue, Timing.Default, Hold.Create(6f / 24f), BlendingMode.Mix);

    public static Option<PixelPass> Pass(GlyphCells state, PassContext context) =>
        Some(Blend.Mixed(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)), state.Mode, Mix.Full, []));

    private static unsafe Fin<Unit> Kernel(GlyphCells state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
        using VectorOfMat atlas = new();
        (Arr<int> ranked, (EInteger Columns, EInteger Rows) grid, Size tile) = Atlas(atlas, state.Glyphs, state.Font, ERational.FromSingle(state.Size).Multiply(context.Extent.ShortSide), context.Extent);
        using Mat source = header.Clone();
        Func<(EInteger Column, EInteger Row), Vector3> mean = CellGrid.Means(frame, source, grid);
        uint field = CoordinateHash.Field(NoiseStream.GlyphCells, state.Seed, state.Hold.Period(state.Timing.At(context)));
        (uint gray, uint tint, (double X, double Y, double Z) weights) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), context.Working.Luminance);
        Func<(EInteger Column, EInteger Row), (int Tile, Vector3 Color)> cells = memoUnsafe<(EInteger Column, EInteger Row), (int Tile, Vector3 Color)>(cell => {
            Vector3 color = mean(cell);
            (Vector4 u, Vector4 c) = (CoordinateHash.Uniforms(cell.Column, cell.Row, gray), CoordinateHash.Uniforms(cell.Column, cell.Row, tint));
            float level = (float)double.Clamp(double.Clamp((weights.X * color.X) + (weights.Y * color.Y) + (weights.Z * color.Z), 0d, 1d) + (state.Jitter * (u.X - 0.5d)), 0d, 1d);
            return (ranked[int.Min(ranked.Count - 1, (int)(level * ranked.Count))], state.Coloring.Switch(
                (Mean: color, Gray: u.Y, Tint: c.AsVector3(), State: state),
                input: static s => s.Mean,
                text: static s => s.State.Text.Display.AsVector3(),
                perCell: static s => Vector3.Lerp(new Vector3(s.Gray), s.Tint, s.State.Saturation)));
        });
        (int width, int height, Vector3 background) = (context.Extent.Width, context.Extent.Height, state.Background.Display.AsVector3());
        return new PixelPass.Pointwise((row, column, line) => {
            (EInteger cy, int ry) = CellGrid.Coordinate(height - 1L - line, height, grid.Rows);
            int maskRow = (int)((long)ry * tile.Height / height);
            for (int x = 0; x < row.Length; x++) {
                (EInteger cx, int rx) = CellGrid.Coordinate(column + x, width, grid.Columns);
                (int index, Vector3 color) = cells((cx, cy));
                using Mat glyph = atlas[index];
                ReadOnlySpan<byte> mask = new(glyph.GetDataPointer(maskRow).ToPointer(), tile.Width);
                float g = mask[(int)((long)rx * tile.Width / width)] / (float)byte.MaxValue;
                row[x] = new Vector4(Vector3.Lerp(background, color, g), row[x].W);
            }
        }).Run(frame, progress);
    });

    private static (Arr<int> Ranked, (EInteger Columns, EInteger Rows) Grid, Size Tile) Atlas(VectorOfMat planes, Glyphs glyphs, HersheyFonts font, ERational cell, PixelExtent extent) {
        string set = glyphs;
        (Size Size, int Baseline)[] measures = [.. set.Select(glyph => {
            int baseline = 0;
            return (CvInvoke.GetTextSize(glyph.ToString(), font, 1d, 1, ref baseline), baseline);
        })];
        (int widest, int tallest) = (measures.Max(static measure => measure.Size.Width), measures.Max(static measure => measure.Size.Height + measure.Baseline));
        (EInteger Columns, EInteger Rows) grid = CellGrid.Fit(extent, cell.Multiply(widest).Divide(tallest), cell);
        Size tile = new(EInteger.FromInt32(extent.Width).Add(grid.Columns).Subtract(1).Divide(grid.Columns).ToInt32Checked(),
            EInteger.FromInt32(extent.Height).Add(grid.Rows).Subtract(1).Divide(grid.Rows).ToInt32Checked());
        double scale = double.Min((double)tile.Height / tallest, (double)tile.Width / widest);
        for (int i = 0; i < set.Length; i++) {
            using Mat plane = Mat.Zeros(tile.Height, tile.Width, DepthType.Cv8U, 1);
            CvInvoke.PutText(
                plane, set[i].ToString(), new Point((int)double.Round((tile.Width - (measures[i].Size.Width * scale)) / 2d, MidpointRounding.ToEven), tile.Height - (int)double.Ceiling(measures[i].Baseline * scale)),
                font, scale, new MCvScalar(byte.MaxValue), int.Max(1, (int)double.Round(scale, MidpointRounding.ToEven)), LineType.AntiAlias);
            planes.Push(plane);
        }
        return ([.. Enumerable.Range(0, set.Length).OrderBy(i => {
            using Mat glyph = planes[i];
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
    public static readonly GlyphCellsParameter Size = new("size", StateParameter.Bounded<GlyphCells, ShortSideExtent, float, InvalidPixelValue>(Lens<GlyphCells, ShortSideExtent>.New(static cells => cells.Size, static size => cells => cells with { Size = size }), ShortSideExtent.Presentation with { Soft = (1f / 128f, 0.25f) }));
    public static readonly GlyphCellsParameter Coloring = new("coloring", new StateParameter<GlyphCells>.Choice<GlyphColoring, InvalidStylize>(
        Lens<GlyphCells, GlyphColoring>.New(static cells => cells.Coloring, static coloring => cells => cells with { Coloring = coloring })));
    public static readonly GlyphCellsParameter Text = new("text", new StateParameter<GlyphCells>.Color(
        Lens<GlyphCells, Swatch>.New(static cells => cells.Text, static text => cells => cells with { Text = text })));
    public static readonly GlyphCellsParameter Background = new("background", new StateParameter<GlyphCells>.Color(
        Lens<GlyphCells, Swatch>.New(static cells => cells.Background, static background => cells => cells with { Background = background })));
    public static readonly GlyphCellsParameter Saturation = new("saturation", StateParameter.Bounded<GlyphCells, Mix, float, InvalidGrade>(Lens<GlyphCells, Mix>.New(static cells => cells.Saturation, static saturation => cells => cells with { Saturation = saturation }), new()));
    public static readonly GlyphCellsParameter Jitter = new("jitter", StateParameter.Bounded<GlyphCells, AxisFraction, float, InvalidGrade>(Lens<GlyphCells, AxisFraction>.New(static cells => cells.Jitter, static jitter => cells => cells with { Jitter = jitter }), new()));
    public static readonly GlyphCellsParameter Seed = new("seed", StateParameter.Bounded<GlyphCells, Seed, int, InvalidGenerator>(Lens<GlyphCells, Seed>.New(static cells => cells.Seed, static seed => cells => cells with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly GlyphCellsParameter Timing = new("timing", new StateParameter<GlyphCells>.Record<Timing>(
        Lens<GlyphCells, Timing>.New(static cells => cells.Timing, static timing => cells => cells with { Timing = timing })));
    public static readonly GlyphCellsParameter Hold = new("hold", StateParameter.Bounded<GlyphCells, Hold, float, InvalidGenerator>(Lens<GlyphCells, Hold>.New(static cells => cells.Hold, static hold => cells => cells with { Hold = hold }), Generators.Hold.Presentation with { Soft = (Generators.Hold.MinValue, 120f / 24f) }));
    public static readonly GlyphCellsParameter Mode = new("mode", new StateParameter<GlyphCells>.Choice<BlendingMode, InvalidGrade>(
        Lens<GlyphCells, BlendingMode>.New(static cells => cells.Mode, static mode => cells => cells with { Mode = mode })));

    public StateParameter<GlyphCells> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SortKey {
    public static readonly SortKey Luma = new("luma", static (color, weights) => (weights.X * color.X) + (weights.Y * color.Y) + (weights.Z * color.Z));
    public static readonly SortKey Hue = new("hue", static (color, _) => RangeAxis.Hue.Coordinate(Hsy.From(color)));
    public static readonly SortKey Saturation = new("saturation", static (color, _) => RangeAxis.Saturation.Coordinate(Hsy.From(color)));

    [UseDelegateFromConstructor]
    public partial double Level(Vector4 color, (double X, double Y, double Z) weights);
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
    public partial int Order(double key, double other);
}

public sealed record PixelSort(SortKey Key, AxisFraction Low, AxisFraction High, SortDirection Direction)
    : IStateRecord<PixelSort, PixelSortParameter, InvalidStylize>, IPixelStage<PixelSort> {
    public static PixelSort Default { get; } = new(SortKey.Luma, AxisFraction.MinValue, AxisFraction.Create(0.3f), SortDirection.Right);

    public static Option<PixelPass> Pass(PixelSort state, PassContext context) {
        PixelPass.Pointwise rows = new((row, _, _) => state.Runs(row, context.Working.Luminance));
        unsafe Fin<Unit> Columns(PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
            using Mat lines = new();
            CvInvoke.Transpose(header, lines);
            Span<Vector4> pixels = new(lines.DataPointer.ToPointer(), lines.Rows * lines.Cols);
            for (int y = 0; y < lines.Rows; y++)
                rows.Row(pixels.Slice(y * lines.Cols, lines.Cols), 0, y);
            CvInvoke.Transpose(lines, header);
            progress.Report(frame.Size.Height);
            return unit;
        });
        return Some<PixelPass>(new PixelPass.Frame(state.Direction.Map(right: rows.Run, left: rows.Run, down: Columns, up: Columns)));
    }

    private void Runs(Span<Vector4> row, (double X, double Y, double Z) weights) {
        using SpanOwner<double> owner = SpanOwner<double>.Allocate(row.Length);
        Span<double> keys = owner.Span;
        for (int x = 0; x < row.Length; x++)
            keys[x] = Key.Level(row[x], weights);
        bool Holds(double key) => Low <= High ? key >= Low && key <= High : key >= Low || key <= High;
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
    public static readonly PixelSortParameter Low = new("low", StateParameter.Bounded<PixelSort, AxisFraction, float, InvalidGrade>(Lens<PixelSort, AxisFraction>.New(static sort => sort.Low, static low => sort => sort with { Low = low }), new()));
    public static readonly PixelSortParameter High = new("high", StateParameter.Bounded<PixelSort, AxisFraction, float, InvalidGrade>(Lens<PixelSort, AxisFraction>.New(static sort => sort.High, static high => sort => sort with { High = high }), new()));
    public static readonly PixelSortParameter Direction = new("direction", new StateParameter<PixelSort>.Choice<SortDirection, InvalidStylize>(
        Lens<PixelSort, SortDirection>.New(static sort => sort.Direction, static direction => sort => sort with { Direction = direction })));

    public StateParameter<PixelSort> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class CellGrid {
    public static (EInteger Columns, EInteger Rows) Fit(PixelExtent extent, ERational width, ERational height) {
        return (Count(extent.Width, width), Count(extent.Height, height));

        static EInteger Count(int pixels, ERational size) => ERational.FromInt32(pixels).Divide(size) switch {
            var count => EInteger.Max(EInteger.One, EFloat.FromEInteger(count.Numerator).DivideToExponent(EFloat.FromEInteger(count.Denominator), 0L, ERounding.HalfEven).ToEInteger()),
        };
    }

    public static (EInteger Cell, int Remainder) Coordinate(long pixel, int extent, EInteger cells) =>
        NoiseFunctions.Split(ERational.FromInt64(pixel).Multiply(ERational.FromEInteger(cells)).Divide(extent)) switch {
            var (cell, local) => (cell, local.Multiply(extent).ToInt32Checked()),
        };

    public static unsafe Func<(EInteger Column, EInteger Row), Vector3> Means(PixelFrame frame, Mat plane, (EInteger Columns, EInteger Rows) grid) {
        foreach (ref Vector4 pixel in new Span<Vector4>(plane.DataPointer.ToPointer(), plane.Rows * plane.Cols))
            pixel = new Vector4(pixel.AsVector3() * pixel.W, pixel.W);
        bool native = frame.Whole && grid.Columns.Multiply(grid.Rows).CompareTo(int.MaxValue) <= 0 && grid.Columns.Multiply(plane.NumberOfChannels).Add(grid.Rows).CompareTo(int.MaxValue) <= 0;
        if (native)
            CvInvoke.Resize(plane, plane, new Size(grid.Columns.ToInt32Checked(), grid.Rows.ToInt32Checked()), 0d, 0d, Inter.Area);
        return native
            ? cell => new Span<Vector4>(plane.DataPointer.ToPointer(), plane.Rows * plane.Cols)[(cell.Row.ToInt32Checked() * grid.Columns.ToInt32Checked()) + cell.Column.ToInt32Checked()] switch { var pixel => pixel.W > 0f ? pixel.AsVector3() / pixel.W : Vector3.Zero }
            : cell => Mean(plane, frame, grid, cell);
    }

    private static unsafe Vector3 Mean(Mat source, PixelFrame frame, (EInteger Columns, EInteger Rows) grid, (EInteger Column, EInteger Row) cell) {
        double Interpolated(EInteger index, int extent, EInteger cells) =>
            NoiseFunctions.Split(ERational.FromEInteger(index).Multiply(extent).Divide(ERational.FromEInteger(cells))) switch {
                var (pixel, local) =>
                    (Pixel: pixel, Remainder: local.Multiply(ERational.FromEInteger(cells)).ToEIntegerIfExact()) switch {
                        var at => ERational.FromEInteger(at.Pixel).ToDouble() + 0.5d + (index.Sign >= 0 && index.CompareTo(cells) < 0 && at.Pixel.CompareTo(extent - 1) == 0 ? 0d
                            : double.Max(0d, ERational.FromEInteger(EInteger.FromInt32(extent).Subtract(cells).Add(at.Remainder)).Divide(extent).ToDouble())),
                    },
            };
        if (grid.Columns.CompareTo(frame.Extent.Width) > 0 || grid.Rows.CompareTo(frame.Extent.Height) > 0) {
            ReadOnlySpan2D<Vector4> plane = new Span<Vector4>(source.DataPointer.ToPointer(), source.Rows * source.Cols).AsSpan2D(source.Rows, source.Cols);
            Vector4 pixel = plane.Sample(
                (Interpolated(cell.Column, frame.Extent.Width, grid.Columns), Interpolated(cell.Row, frame.Extent.Height, grid.Rows)),
                (WrapMode.Black, WrapMode.Black), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight), Some((frame.Origin, frame.Extent)));
            return pixel.W > 0f ? pixel.AsVector3() / pixel.W : Vector3.Zero;
        }
        (double Left, double Right) Bounds(EInteger index, int extent, int origin, int size, EInteger cells) =>
            index.Multiply(extent).Subtract(cells.Multiply(origin)) switch {
                var start => (double.Max(0d, ERational.FromEInteger(start).Divide(ERational.FromEInteger(cells)).ToDouble()),
                    double.Min(size, ERational.FromEInteger(start.Add(extent)).Divide(ERational.FromEInteger(cells)).ToDouble())),
            };
        (double left, double right) = Bounds(cell.Column, frame.Extent.Width, frame.Origin.X, frame.Size.Width, grid.Columns);
        (double top, double bottom) = Bounds(cell.Row, frame.Extent.Height, frame.Origin.Y, frame.Size.Height, grid.Rows);
        ReadOnlySpan<Vector4> pixels = new(source.DataPointer.ToPointer(), source.Rows * source.Cols);
        Vector256<double> sum = Vector256<double>.Zero;
        for (int y = (int)double.Floor(top); y < (int)double.Ceiling(bottom); y++) {
            double down = double.Min(y + 1d, bottom) - double.Max(y, top);
            for (int x = (int)double.Floor(left); x < (int)double.Ceiling(right); x++) {
                Vector4 pixel = pixels[(y * source.Cols) + x];
                sum += Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W) * (down * (double.Min(x + 1d, right) - double.Max(x, left)));
            }
        }
        return sum.GetElement(3) > 0d
            ? new Vector3((float)(sum.GetElement(0) / sum.GetElement(3)), (float)(sum.GetElement(1) / sum.GetElement(3)), (float)(sum.GetElement(2) / sum.GetElement(3)))
            : Vector3.Zero;
    }
}
