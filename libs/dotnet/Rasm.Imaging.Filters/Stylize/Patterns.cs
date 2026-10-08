using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Stylize;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IScreenSpot {
    public static abstract float Threshold(float u, float v);
}

internal interface ISortRule {
    public static abstract float Key(Vector4 color, Vector3 weights);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct CellSize : IMinMaxValue<CellSize> {
    public static CellSize MinValue { get; } = new(ShortSideExtent.MinValue);
    public static CellSize MaxValue { get; } = new(1f / 4f);
    public static CellSize Standard { get; } = new(1f / (2f * 2f * 4f) * (1920f / 1080f));
    public static CellSize Pixelate { get; } = new(0.1f * 0.05f * (1920f / 1080f));
    public static CellSize Glyph { get; } = new(1f / 24f);

    public float Pixels(PixelExtent extent) => _value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct CellOffset : IMinMaxValue<CellOffset> {
    public static CellOffset MinValue => Neutral;
    public static CellOffset MaxValue { get; } = new(float.BitDecrement(1f));

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct KeyLevel : IMinMaxValue<KeyLevel> {
    public static KeyLevel MinValue { get; } = new(0f);
    public static KeyLevel MaxValue => Highest;
    public static KeyLevel Highest { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct MappingJitter : IMinMaxValue<MappingJitter> {
    public static MappingJitter MinValue => Neutral;
    public static MappingJitter MaxValue { get; } = new(1f);

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

public sealed record InkScreen(SignedAngle Angle, CellOffset U, CellOffset V) {
    public static InkScreen Cyan { get; } = new(SignedAngle.Diagonal, CellOffset.Create(0.193f * 4f), CellOffset.Create(0.052f * 4f));
    public static InkScreen Magenta { get; } = new(SignedAngle.Diagonal, CellOffset.Create(0.052f * 4f), CellOffset.Create(0.193f * 4f));
    public static InkScreen Yellow { get; } = new(SignedAngle.Diagonal, CellOffset.Neutral, CellOffset.Create(0.2f * 4f));
    public static InkScreen Key { get; } = new(SignedAngle.Diagonal, CellOffset.Create(0.1414f * 4f), CellOffset.Create(0.1414f * 4f));

    private static readonly Lens<InkScreen, SignedAngle> AngleOf = Lens<InkScreen, SignedAngle>.New(static ink => ink.Angle, static angle => ink => ink with { Angle = angle });
    private static readonly Lens<InkScreen, CellOffset> UOf = Lens<InkScreen, CellOffset>.New(static ink => ink.U, static u => ink => ink with { U = u });
    private static readonly Lens<InkScreen, CellOffset> VOf = Lens<InkScreen, CellOffset>.New(static ink => ink.V, static v => ink => ink with { V = v });
    private static readonly Presentation<CellOffset, float> Phase = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

    internal static (StateParameter<TRecord> Angle, StateParameter<TRecord> U, StateParameter<TRecord> V) Kinds<TRecord>(Lens<TRecord, InkScreen> ink) =>
        (new StateParameter<TRecord>.Bounded<SignedAngle, float, InvalidPixelValue>(lens(ink, AngleOf), new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral }),
         new StateParameter<TRecord>.Bounded<CellOffset, float, InvalidStylize>(lens(ink, UOf), Phase),
         new StateParameter<TRecord>.Bounded<CellOffset, float, InvalidStylize>(lens(ink, VOf), Phase));
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScreenShape {
    public static readonly ScreenShape Round = new("round");
    public static readonly ScreenShape Ellipse = new("ellipse");
    public static readonly ScreenShape Diamond = new("diamond");
    public static readonly ScreenShape Square = new("square");
    public static readonly ScreenShape Line = new("line");
    public static readonly ScreenShape WavyLine = new("wavy-line");
    public static readonly ScreenShape Cross = new("cross");
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Separation {
    public static readonly Separation Mono = new("mono");
    public static readonly Separation Process = new("process");
}

public sealed record Halftone(
    ScreenShape Shape, CellSize Size, Separation Separation, InkScreen Cyan, InkScreen Magenta, InkScreen Yellow, InkScreen Key,
    KeyLevel CrossOnset, Swatch Ink, Option<Swatch> Paper, BlendingMode Mode)
    : IStateRecord<Halftone, HalftoneParameter, InvalidStylize>, IPixelStage<Halftone> {
    private const double Elongation = double.Pi / 1.6d;

    public static Halftone Default { get; } = new(
        ScreenShape.Round, CellSize.Standard, Separation.Mono, InkScreen.Cyan, InkScreen.Magenta, InkScreen.Yellow, InkScreen.Key,
        KeyLevel.Highest, Swatch.Black, Some(Swatch.White), BlendingMode.Mix);

    public static Option<PixelPass> Pass(Halftone state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Shape.Switch(
            (State: state, Context: context),
            round: static s => Kernel<RoundSpot>(s.State, s.Context),
            ellipse: static s => Kernel<EllipseSpot>(s.State, s.Context),
            diamond: static s => Kernel<DiamondSpot>(s.State, s.Context),
            square: static s => Kernel<SquareSpot>(s.State, s.Context),
            line: static s => Kernel<LineSpot>(s.State, s.Context),
            wavyLine: static s => Kernel<WavyLineSpot>(s.State, s.Context),
            cross: static s => Kernel<CrossSpot>(s.State, s.Context))));

    private static Action<Span<Vector4>, int, int> Kernel<TSpot>(Halftone state, PassContext context) where TSpot : struct, IScreenSpot {
        float pitch = state.Size.Pixels(context.Extent);
        (Arr<Plate> plates, Func<Vector3, Vector4> levels) = state.Separation.Switch<(Halftone State, Vector3 Weights, Func<InkScreen, Vector3, int, Plate> Plate), (Arr<Plate>, Func<Vector3, Vector4>)>(
            (state, context.Working.Luminance, (ink, color, lane) => Plate.Of(ink, state.CrossOnset, pitch, context.Extent, color, lane)),
            mono: static s => (
                [s.Plate(s.State.Key, s.State.Ink.Display.AsVector3(), 3)],
                rgb => new Vector4(Vector3.Zero, 1f - Easing.Saturate(Vector3.Dot(s.Weights, rgb)))),
            process: static s => (
                [s.Plate(s.State.Cyan, new Vector3(0f, 1f, 1f), 0),
                 s.Plate(s.State.Magenta, new Vector3(1f, 0f, 1f), 1),
                 s.Plate(s.State.Yellow, new Vector3(1f, 1f, 0f), 2),
                 s.Plate(s.State.Key, s.State.Ink.Display.AsVector3(), 3)],
                static rgb => Vector3.Clamp(rgb, Vector3.Zero, Vector3.One) switch {
                    var saturated => float.Max(saturated.X, float.Max(saturated.Y, saturated.Z)) switch {
                        > 0f and var most => new Vector4(Vector3.One - (saturated / most), 1f - most),
                        _ => Vector4.UnitW,
                    },
                }));
        Option<Vector3> paper = state.Paper.Map(static swatch => swatch.Display.AsVector3());
        (BlendingMode mode, int height) = (state.Mode, context.Extent.Height);
        return (row, column, line) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            for (int x = 0; x < row.Length; x++) {
                (Vector2 p, Vector4 level, Vector3 transmitted) = (new(column + x + 0.5f, height - line - 0.5f), levels(row[x].AsVector3()), Vector3.One);
                foreach (Plate plate in plates)
                    transmitted *= plate.Transmittance<TSpot>(p, level[plate.Lane]);
                layer.Span[x] = new Vector4(paper.IfNone(row[x].AsVector3()) * transmitted, row[x].W);
            }
            mode.Over(row, layer.Span);
        };
    }

    private static double Quarter(double rho, double width, double height) =>
        double.Min(rho, width) switch {
            var reach => double.Min(double.Sqrt(double.Max((rho * rho) - (height * height), 0d)), reach) switch {
                var knee => (height * knee) + Arc(rho, reach) - Arc(rho, knee),
            },
        };

    private static double Arc(double rho, double x) =>
        double.Sqrt((rho * rho) - (x * x)) switch { var y => ((x * y) + (rho * rho * double.Atan2(x, y))) / 2d };

    private readonly record struct Screen(Vector2 Origin, Vector2 Across, Vector2 Down, float Onset, float Gain) {
        public static Screen Of(float angle, InkScreen ink, float pitch, PixelExtent extent, float onset, float gain) =>
            float.SinCos(angle) switch {
                var (sin, cos) => (new Vector2(cos, sin) / pitch, new Vector2(-sin, cos) / pitch) switch {
                    var (across, down) => new(new Vector2(ink.U, ink.V) - (extent.Width / 2f * across) - (extent.Height / 2f * down), across, down, onset, gain),
                },
            };

        public float Coverage<TSpot>(Vector2 p, float level) where TSpot : struct, IScreenSpot {
            Vector2 c = Origin + (p.X * Across) + (p.Y * Down);
            float t = Threshold<TSpot>(c);
            float w = float.Abs(Threshold<TSpot>(c + Across) - t) + float.Abs(Threshold<TSpot>(c + Down) - t);
            return w > 0f ? Easing.Saturate((((level * (1f + w)) - (w / 2f) - t) / w) + 0.5f) : t < level ? 1f : 0f;
        }

        private static float Threshold<TSpot>(Vector2 c) where TSpot : struct, IScreenSpot =>
            ((2f * (c - Vector2.Round(c, MidpointRounding.ToNegativeInfinity))) - Vector2.One) switch { var uv => TSpot.Threshold(uv.X, uv.Y) };
    }

    private readonly record struct Plate(Arr<Screen> Screens, Vector3 Ink, int Lane) {
        public static Plate Of(InkScreen ink, KeyLevel onset, float pitch, PixelExtent extent, Vector3 color, int lane) =>
            Screen.Of(ink.Angle, ink, pitch, extent, 0f, 1f) switch {
                var screen => new(onset < KeyLevel.Highest ? [screen, Screen.Of(ink.Angle + (float.Pi / 2f), ink, pitch, extent, onset, 1f / (1f - onset))] : [screen], color, lane),
            };

        public Vector3 Transmittance<TSpot>(Vector2 p, float level) where TSpot : struct, IScreenSpot {
            float clear = 1f;
            foreach (Screen screen in Screens)
                clear *= 1f - screen.Coverage<TSpot>(p, Easing.Saturate((level - screen.Onset) * screen.Gain));
            return Vector3.One - ((1f - clear) * (Vector3.One - Ink));
        }
    }

    private readonly struct RoundSpot : IScreenSpot {
        public static float Threshold(float u, float v) => (float)Quarter(double.Hypot(u, v), 1d, 1d);
    }

    private readonly struct EllipseSpot : IScreenSpot {
        public static float Threshold(float u, float v) => (float)(Elongation * Quarter(double.Hypot(u, v / Elongation), 1d, 1d / Elongation));
    }

    private readonly struct DiamondSpot : IScreenSpot {
        public static float Threshold(float u, float v) =>
            (float.Abs(u) + float.Abs(v)) switch {
                <= 1f and var d => d * d / 2f,
                var d => 1f - ((2f - d) * (2f - d) / 2f),
            };
    }

    private readonly struct SquareSpot : IScreenSpot {
        public static float Threshold(float u, float v) => float.Max(float.Abs(u), float.Abs(v)) switch { var m => m * m };
    }

    private readonly struct LineSpot : IScreenSpot {
        public static float Threshold(float u, float v) => float.Abs(v);
    }

    private readonly struct WavyLineSpot : IScreenSpot {
        public static float Threshold(float u, float v) => (v - (0.5f * float.SinPi(u))) switch { var w => float.Abs(w - (2f * float.Floor((w + 1f) / 2f))) };
    }

    private readonly struct CrossSpot : IScreenSpot {
        public static float Threshold(float u, float v) => float.Min(float.Abs(u), float.Abs(v)) switch { var m => 1f - ((1f - m) * (1f - m)) };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HalftoneParameter : IStateParameter<Halftone> {
    private static readonly (StateParameter<Halftone> Angle, StateParameter<Halftone> U, StateParameter<Halftone> V) CyanScreen =
        InkScreen.Kinds(Lens<Halftone, InkScreen>.New(static halftone => halftone.Cyan, static ink => halftone => halftone with { Cyan = ink }));
    private static readonly (StateParameter<Halftone> Angle, StateParameter<Halftone> U, StateParameter<Halftone> V) MagentaScreen =
        InkScreen.Kinds(Lens<Halftone, InkScreen>.New(static halftone => halftone.Magenta, static ink => halftone => halftone with { Magenta = ink }));
    private static readonly (StateParameter<Halftone> Angle, StateParameter<Halftone> U, StateParameter<Halftone> V) YellowScreen =
        InkScreen.Kinds(Lens<Halftone, InkScreen>.New(static halftone => halftone.Yellow, static ink => halftone => halftone with { Yellow = ink }));
    private static readonly (StateParameter<Halftone> Angle, StateParameter<Halftone> U, StateParameter<Halftone> V) KeyScreen =
        InkScreen.Kinds(Lens<Halftone, InkScreen>.New(static halftone => halftone.Key, static ink => halftone => halftone with { Key = ink }));

    public static readonly HalftoneParameter Shape = new(
        "shape", new StateParameter<Halftone>.Choice<ScreenShape, InvalidStylize>(
            Lens<Halftone, ScreenShape>.New(static halftone => halftone.Shape, static shape => halftone => halftone with { Shape = shape })));
    public static readonly HalftoneParameter Size = new(
        "size", new StateParameter<Halftone>.Bounded<CellSize, float, InvalidStylize>(
            Lens<Halftone, CellSize>.New(static halftone => halftone.Size, static size => halftone => halftone with { Size = size }),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Scale = TrackScale.Log, Soft = (1f / (2f * 64f) * (1920f / 1080f), CellSize.MaxValue) }));
    public static readonly HalftoneParameter Separation = new(
        "separation", new StateParameter<Halftone>.Choice<Separation, InvalidStylize>(
            Lens<Halftone, Separation>.New(static halftone => halftone.Separation, static separation => halftone => halftone with { Separation = separation })));
    public static readonly HalftoneParameter CyanAngle = new("cyan-angle", CyanScreen.Angle);
    public static readonly HalftoneParameter CyanU = new("cyan-u", CyanScreen.U);
    public static readonly HalftoneParameter CyanV = new("cyan-v", CyanScreen.V);
    public static readonly HalftoneParameter MagentaAngle = new("magenta-angle", MagentaScreen.Angle);
    public static readonly HalftoneParameter MagentaU = new("magenta-u", MagentaScreen.U);
    public static readonly HalftoneParameter MagentaV = new("magenta-v", MagentaScreen.V);
    public static readonly HalftoneParameter YellowAngle = new("yellow-angle", YellowScreen.Angle);
    public static readonly HalftoneParameter YellowU = new("yellow-u", YellowScreen.U);
    public static readonly HalftoneParameter YellowV = new("yellow-v", YellowScreen.V);
    public static readonly HalftoneParameter KeyAngle = new("key-angle", KeyScreen.Angle);
    public static readonly HalftoneParameter KeyU = new("key-u", KeyScreen.U);
    public static readonly HalftoneParameter KeyV = new("key-v", KeyScreen.V);
    public static readonly HalftoneParameter CrossOnset = new(
        "cross-onset", new StateParameter<Halftone>.Bounded<KeyLevel, float, InvalidStylize>(
            Lens<Halftone, KeyLevel>.New(static halftone => halftone.CrossOnset, static onset => halftone => halftone with { CrossOnset = onset }), new()));
    public static readonly HalftoneParameter Ink = new(
        "ink", new StateParameter<Halftone>.Color(Lens<Halftone, Swatch>.New(static halftone => halftone.Ink, static ink => halftone => halftone with { Ink = ink })));
    public static readonly HalftoneParameter Paper = new(
        "paper", new StateParameter<Halftone>.OptionalColor(
            Lens<Halftone, Option<Swatch>>.New(static halftone => halftone.Paper, static paper => halftone => halftone with { Paper = paper })));
    public static readonly HalftoneParameter Mode = new(
        "mode", new StateParameter<Halftone>.Choice<BlendingMode, InvalidGrade>(
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
    public static Mosaic Default { get; } = new(MosaicCell.Square, CellSize.Pixelate, Cellular.Default, Seed.MinValue, Timing.Still, Hold.MinValue);

    public static Option<PixelPass> Pass(Mosaic state, PassContext context) =>
        Some<PixelPass>(state.Cell.Switch(
            (State: state, Context: context),
            square: static s => new PixelPass.Frame((frame, progress) => Square(s.State, s.Context, frame, progress)),
            cellular: static s => new PixelPass.Frame((frame, progress) => CellSites(s.State, s.Context, frame, progress))));

    private static Fin<Unit> Square(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress) {
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

    private static Fin<Unit> CellSites(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        using Mat header = frame.Header();
        using Mat copy = header.Clone();
        (int width, int height, float pitch) = (context.Extent.Width, context.Extent.Height, state.Size.Pixels(context.Extent));
        Vector2 center = new Vector2(width, height) / 2f;
        uint field = CoordinateHash.Field(NoiseStream.Mosaic, state.Seed, state.Hold.Period(state.Timing.At(context)));
        return new PixelPass.Pointwise((row, column, line) => {
            ReadOnlySpan<Vector4> source = copy.GetSpan<Vector4>();
            for (int x = 0; x < row.Length; x++) {
                Vector2 point = (new Vector2(column + x + 0.5f, height - line - 0.5f) - center) / pitch;
                Vector2 site = center + (pitch * NoiseDimensions.Two.F1(new Vector4(point, 0f, 0f), Octaves.Plain, state.Cells, field).Site.AsVector2());
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
    private static readonly (StateParameter<Mosaic> Clock, StateParameter<Mosaic> Pace) Time =
        Timing.Kinds(Lens<Mosaic, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));

    public static readonly MosaicParameter Cell = new(
        "cell", new StateParameter<Mosaic>.Choice<MosaicCell, InvalidStylize>(
            Lens<Mosaic, MosaicCell>.New(static mosaic => mosaic.Cell, static cell => mosaic => mosaic with { Cell = cell })));
    public static readonly MosaicParameter Size = new(
        "size", new StateParameter<Mosaic>.Bounded<CellSize, float, InvalidStylize>(
            Lens<Mosaic, CellSize>.New(static mosaic => mosaic.Size, static size => mosaic => mosaic with { Size = size }),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Scale = TrackScale.Log, Soft = (CellSize.MinValue, 0.1f * (1920f / 1080f)) }));
    public static readonly MosaicParameter Cells = new("cells", new StateParameter<Mosaic>.Record<Cellular>(
        Lens<Mosaic, Cellular>.New(static state => state.Cells, static value => state => state with { Cells = value })));
    public static readonly MosaicParameter Seed = new(
        "seed", new StateParameter<Mosaic>.Bounded<Seed, int, InvalidGenerator>(
            Lens<Mosaic, Seed>.New(static mosaic => mosaic.Seed, static seed => mosaic => mosaic with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly MosaicParameter Clock = new("clock", Time.Clock);
    public static readonly MosaicParameter Pace = new("pace", Time.Pace);
    public static readonly MosaicParameter Hold = new(
        "hold", new StateParameter<Mosaic>.Bounded<Hold, float, InvalidGenerator>(
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
    MappingJitter Jitter, Seed Seed, Timing Timing, Hold Hold, BlendingMode Mode)
    : IStateRecord<GlyphCells, GlyphCellsParameter, InvalidStylize>, IPixelStage<GlyphCells> {
    public static GlyphCells Default { get; } = new(
        Glyphs.Symbols, HersheyFonts.Simplex, CellSize.Glyph, GlyphColoring.Input, Swatch.White, Swatch.Black, Mix.Full,
        MappingJitter.Create(0.1f), Seed.MinValue, Timing.Standard, Hold.Glyphs, BlendingMode.Mix);

    public static Option<PixelPass> Pass(GlyphCells state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(GlyphCells state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (Mat plane, Arr<int> ranked, (int Columns, int Rows) grid, (int Width, int Height) tile) = Atlas(state.Glyphs, state.Font, state.Size.Pixels(context.Extent), context.Extent);
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
            float level = Easing.Saturate(Vector3.Dot(weights, cells[index].AsVector3())) * (1f - state.Jitter + (state.Jitter * u.X));
            tiles[index] = ranked[int.Min(ranked.Count - 1, (int)(Easing.Saturate(level) * ranked.Count))];
            cells[index] = new Vector4(state.Coloring.Switch(
                (Mean: cells[index].AsVector3(), Gray: u.Y, Tint: c.AsVector3(), State: state),
                input: static s => s.Mean,
                text: static s => s.State.Text.Display.AsVector3(),
                perCell: static s => Vector3.Lerp(new Vector3(s.Gray), s.Tint, s.State.Saturation)), cells[index].W);
        }
        (int width, int height, int stride, Vector3 background) = (context.Extent.Width, context.Extent.Height, ranked.Count * tile.Width, state.Background.Display.AsVector3());
        return new PixelPass.Pointwise((row, column, line) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            ReadOnlySpan<byte> glyphs = atlas.GetSpan<byte>();
            ReadOnlySpan<Vector4> colors = means.GetSpan<Vector4>();
            ReadOnlySpan<int> indices = ranks.GetSpan<int>();
            (long cy, long ry) = long.DivRem((long)(height - 1 - line) * grid.Rows, height);
            for (int x = 0; x < row.Length; x++) {
                (long cx, long rx) = long.DivRem((long)(column + x) * grid.Columns, width);
                int cell = int.CreateSaturating((cy * grid.Columns) + cx);
                float g = glyphs[int.CreateSaturating((ry * tile.Height / height * stride) + (indices[cell] * tile.Width) + (rx * tile.Width / width))] / (float)byte.MaxValue;
                layer.Span[x] = new Vector4(Vector3.Lerp(background, colors[cell].AsVector3(), g), row[x].W);
            }
            state.Mode.Over(row, layer.Span);
        }).Run(frame, progress);
    }

    private static (Mat Plane, Arr<int> Ranked, (int Columns, int Rows) Grid, (int Width, int Height) Tile) Atlas(Glyphs glyphs, HersheyFonts font, float cell, PixelExtent extent) {
        string set = glyphs;
        (Size Size, int Baseline)[] measures = [.. set.Select(glyph => {
            int baseline = 0;
            return (CvInvoke.GetTextSize(glyph.ToString(), font, 1d, 1, ref baseline), baseline);
        })];
        (int widest, int tallest) = (measures.Max(static measure => measure.Size.Width), measures.Max(static measure => measure.Size.Height + measure.Baseline));
        (int Columns, int Rows) grid = CellGrid.Fit(extent, cell * widest / tallest, cell);
        (int Width, int Height) tile = ((extent.Width + grid.Columns - 1) / grid.Columns, (extent.Height + grid.Rows - 1) / grid.Rows);
        double scale = double.Min((double)tile.Height / tallest, (double)tile.Width / widest);
        Mat plane = Mat.Zeros(tile.Height, set.Length * tile.Width, DepthType.Cv8U, 1);
        for (int i = 0; i < set.Length; i++) {
            CvInvoke.PutText(
                plane, set[i].ToString(), new Point((i * tile.Width) + (int)double.Round((tile.Width - (measures[i].Size.Width * scale)) / 2d, MidpointRounding.ToEven), tile.Height - (int)double.Ceiling(measures[i].Baseline * scale)),
                font, scale, new MCvScalar(byte.MaxValue), int.Max(1, (int)double.Round(scale, MidpointRounding.ToEven)), LineType.AntiAlias);
        }
        double[] coverage = [.. Enumerable.Range(0, set.Length).Select(i => {
            using Mat glyph = new(plane, new Rectangle(i * tile.Width, 0, tile.Width, tile.Height));
            return CvInvoke.Mean(glyph).V0;
        })];
        return (plane, [.. Enumerable.Range(0, set.Length).OrderBy(i => coverage[i])], grid, tile);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlyphCellsParameter : IStateParameter<GlyphCells> {
    private static readonly (StateParameter<GlyphCells> Clock, StateParameter<GlyphCells> Pace) Time =
        Timing.Kinds(Lens<GlyphCells, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly GlyphCellsParameter Glyphs = new(
        "glyphs", new StateParameter<GlyphCells>.Keyed<Glyphs, string, InvalidStylize>(
            Lens<GlyphCells, Glyphs>.New(static cells => cells.Glyphs, static glyphs => cells => cells with { Glyphs = glyphs })));
    public static readonly GlyphCellsParameter Font = new(
        "font", new StateParameter<GlyphCells>.Enumerated<HersheyFonts>(
            Lens<GlyphCells, HersheyFonts>.New(static cells => cells.Font, static font => cells => cells with { Font = font })));
    public static readonly GlyphCellsParameter Size = new(
        "size", new StateParameter<GlyphCells>.Bounded<CellSize, float, InvalidStylize>(
            Lens<GlyphCells, CellSize>.New(static cells => cells.Size, static size => cells => cells with { Size = size }),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Scale = TrackScale.Log, Soft = (1f / 128f, CellSize.MaxValue) }));
    public static readonly GlyphCellsParameter Coloring = new(
        "coloring", new StateParameter<GlyphCells>.Choice<GlyphColoring, InvalidStylize>(
            Lens<GlyphCells, GlyphColoring>.New(static cells => cells.Coloring, static coloring => cells => cells with { Coloring = coloring })));
    public static readonly GlyphCellsParameter Text = new(
        "text", new StateParameter<GlyphCells>.Color(Lens<GlyphCells, Swatch>.New(static cells => cells.Text, static text => cells => cells with { Text = text })));
    public static readonly GlyphCellsParameter Background = new(
        "background", new StateParameter<GlyphCells>.Color(
            Lens<GlyphCells, Swatch>.New(static cells => cells.Background, static background => cells => cells with { Background = background })));
    public static readonly GlyphCellsParameter Saturation = new(
        "saturation", new StateParameter<GlyphCells>.Bounded<Mix, float, InvalidGrade>(
            Lens<GlyphCells, Mix>.New(static cells => cells.Saturation, static saturation => cells => cells with { Saturation = saturation }), new()));
    public static readonly GlyphCellsParameter Jitter = new(
        "jitter", new StateParameter<GlyphCells>.Bounded<MappingJitter, float, InvalidStylize>(
            Lens<GlyphCells, MappingJitter>.New(static cells => cells.Jitter, static jitter => cells => cells with { Jitter = jitter }), new()));
    public static readonly GlyphCellsParameter Seed = new(
        "seed", new StateParameter<GlyphCells>.Bounded<Seed, int, InvalidGenerator>(
            Lens<GlyphCells, Seed>.New(static cells => cells.Seed, static seed => cells => cells with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly GlyphCellsParameter Clock = new("clock", Time.Clock);
    public static readonly GlyphCellsParameter Pace = new("pace", Time.Pace);
    public static readonly GlyphCellsParameter Hold = new(
        "hold", new StateParameter<GlyphCells>.Bounded<Hold, float, InvalidGenerator>(
            Lens<GlyphCells, Hold>.New(static cells => cells.Hold, static hold => cells => cells with { Hold = hold }),
            Generators.Hold.Presentation with { Soft = (Generators.Hold.MinValue, 120f / 24f) }));
    public static readonly GlyphCellsParameter Mode = new(
        "mode", new StateParameter<GlyphCells>.Choice<BlendingMode, InvalidGrade>(
            Lens<GlyphCells, BlendingMode>.New(static cells => cells.Mode, static mode => cells => cells with { Mode = mode })));

    public StateParameter<GlyphCells> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SortKey {
    public static readonly SortKey Luma = new("luma");
    public static readonly SortKey Hue = new("hue");
    public static readonly SortKey Saturation = new("saturation");
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SortDirection {
    public static readonly SortDirection Right = new("right");
    public static readonly SortDirection Left = new("left");
    public static readonly SortDirection Down = new("down");
    public static readonly SortDirection Up = new("up");
}

public sealed record PixelSort(SortKey Key, KeyLevel Low, KeyLevel High, SortDirection Direction)
    : IStateRecord<PixelSort, PixelSortParameter, InvalidStylize>, IPixelStage<PixelSort> {
    public static PixelSort Default { get; } = new(SortKey.Luma, KeyLevel.MinValue, KeyLevel.Create(0.3f), SortDirection.Right);

    public static Option<PixelPass> Pass(PixelSort state, PassContext context) =>
        Some<PixelPass>(state.Key.Switch(
            (State: state, Weights: context.Working.Luminance),
            luma: static s => Sorted<LumaKey>(s.State, s.Weights),
            hue: static s => Sorted<HueKey>(s.State, s.Weights),
            saturation: static s => Sorted<SaturationKey>(s.State, s.Weights)));

    private static PixelPass.Frame Sorted<TRule>(PixelSort state, Vector3 weights) where TRule : struct, ISortRule =>
        state.Direction.Switch(
            (State: state, Weights: weights),
            right: static s => new PixelPass.Frame((frame, progress) => Rows<TRule>(s.State, s.Weights, 1f, frame, progress)),
            left: static s => new PixelPass.Frame((frame, progress) => Rows<TRule>(s.State, s.Weights, -1f, frame, progress)),
            down: static s => new PixelPass.Frame((frame, progress) => Columns<TRule>(s.State, s.Weights, 1f, frame, progress)),
            up: static s => new PixelPass.Frame((frame, progress) => Columns<TRule>(s.State, s.Weights, -1f, frame, progress)));

    private static Fin<Unit> Rows<TRule>(PixelSort state, Vector3 weights, float sign, PixelFrame frame, IProgress<int> progress) where TRule : struct, ISortRule =>
        new PixelPass.Pointwise((row, _, _) => {
            using SpanOwner<float> keys = SpanOwner<float>.Allocate(row.Length);
            state.Spans<TRule>(row, keys.Span, weights, sign);
        }).Run(frame, progress);

    private static Fin<Unit> Columns<TRule>(PixelSort state, Vector3 weights, float sign, PixelFrame frame, IProgress<int> progress) where TRule : struct, ISortRule {
        using Mat header = frame.Header();
        using Mat scratch = new();
        CvInvoke.Transpose(header, scratch);
        using SpanOwner<float> keys = SpanOwner<float>.Allocate(scratch.Cols);
        Span<Vector4> lines = scratch.GetSpan<Vector4>();
        for (int y = 0; y < scratch.Rows; y++)
            state.Spans<TRule>(lines.Slice(y * scratch.Cols, scratch.Cols), keys.Span, weights, sign);
        CvInvoke.Transpose(scratch, header);
        progress.Report(frame.Size.Height);
        return unit;
    }

    private void Spans<TRule>(Span<Vector4> row, Span<float> keys, Vector3 weights, float sign) where TRule : struct, ISortRule {
        for (int x = 0; x < row.Length; x++)
            keys[x] = sign * TRule.Key(row[x], weights);
        int start = 0;
        for (int x = 0; x <= row.Length; x++) {
            if (x < row.Length && row[x].W > 0f && Holds(sign * keys[x]))
                continue;
            keys[start..x].Sort(row[start..x]);
            start = x + 1;
        }
    }

    private bool Holds(float key) => Low <= High ? key >= Low && key <= High : key >= Low || key <= High;

    private readonly struct LumaKey : ISortRule {
        public static float Key(Vector4 color, Vector3 weights) => Vector3.Dot(weights, color.AsVector3());
    }

    private readonly struct HueKey : ISortRule {
        public static float Key(Vector4 color, Vector3 weights) => RangeAxis.Hue.Coordinate(Hsy.From(color));
    }

    private readonly struct SaturationKey : ISortRule {
        public static float Key(Vector4 color, Vector3 weights) => RangeAxis.Saturation.Coordinate(Hsy.From(color));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PixelSortParameter : IStateParameter<PixelSort> {
    public static readonly PixelSortParameter SortKey = new(
        "key", new StateParameter<PixelSort>.Choice<SortKey, InvalidStylize>(
            Lens<PixelSort, SortKey>.New(static sort => sort.Key, static key => sort => sort with { Key = key })));
    public static readonly PixelSortParameter Low = new(
        "low", new StateParameter<PixelSort>.Bounded<KeyLevel, float, InvalidStylize>(
            Lens<PixelSort, KeyLevel>.New(static sort => sort.Low, static low => sort => sort with { Low = low }), new()));
    public static readonly PixelSortParameter High = new(
        "high", new StateParameter<PixelSort>.Bounded<KeyLevel, float, InvalidStylize>(
            Lens<PixelSort, KeyLevel>.New(static sort => sort.High, static high => sort => sort with { High = high }), new()));
    public static readonly PixelSortParameter Direction = new(
        "direction", new StateParameter<PixelSort>.Choice<SortDirection, InvalidStylize>(
            Lens<PixelSort, SortDirection>.New(static sort => sort.Direction, static direction => sort => sort with { Direction = direction })));

    public StateParameter<PixelSort> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class CellGrid {
    public static (int Columns, int Rows) Fit(PixelExtent extent, float width, float height) =>
        (int.Max(1, (int)float.Round(extent.Width / width, MidpointRounding.ToEven)), int.Max(1, (int)float.Round(extent.Height / height, MidpointRounding.ToEven)));

    public static Mat Means(Mat header, (int Columns, int Rows) grid) {
        using VectorOfMat planes = new();
        using Mat premultiplied = new();
        CvInvoke.Split(header, planes);
        using Mat alpha = planes[3];
        for (int lane = 0; lane < 3; lane++) {
            using Mat plane = planes[lane];
            CvInvoke.Multiply(plane, alpha, plane);
        }
        CvInvoke.Merge(planes, premultiplied);
        Mat means = new();
        CvInvoke.Resize(premultiplied, means, new Size(grid.Columns, grid.Rows), 0d, 0d, Inter.Area);
        foreach (ref Vector4 cell in means.GetSpan<Vector4>())
            cell = cell.W > 0f ? new Vector4(cell.AsVector3() / cell.W, cell.W) : Vector4.Zero;
        return means;
    }
}

file static class Layers {
    extension(BlendingMode mode) {
        public void Over(Span<Vector4> row, Span<Vector4> layer) {
            using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
            weights.Span.Fill(1f);
            mode.Mixed(row, layer, weights.Span);
            layer.CopyTo(row);
        }
    }
}
