using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using PeterO.Numbers;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LevelCount : IMinMaxValue<LevelCount> {
    public static LevelCount MinValue { get; } = new(2);
    public static LevelCount MaxValue { get; } = new(int.MaxValue);
    public static LevelCount Eighths { get; } = new(8 + 1);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ColorCount : IMinMaxValue<ColorCount> {
    public static ColorCount MinValue { get; } = new(QuantizerConstants.MinColors);
    public static ColorCount MaxValue { get; } = new(QuantizerConstants.MaxColors);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
[ObjectFactory<string>]
[ObjectFactory<Seq<Swatch>>]
public sealed partial class Palette : IConvertible<string>, IConvertible<Seq<Swatch>> {
    public Seq<Swatch> Colors { get; }

    public static Palette CubeCorners { get; } = new(Seq(
        Swatch.Black, Swatch.Create(0xFF0000), Swatch.Create(0x00FF00), Swatch.Create(0x0000FF),
        Swatch.Create(0xFFFF00), Swatch.Create(0xFF00FF), Swatch.Create(0x00FFFF), Swatch.White));

    public string ToValue() => string.Join(';', Colors.Map(static swatch => ((int)swatch).ToString(CultureInfo.InvariantCulture)));

    Seq<Swatch> IConvertible<Seq<Swatch>>.ToValue() => Colors;

    static ValidationError? IObjectFactory<Palette, string, ValidationError>.Validate(string? value, IFormatProvider? provider, out Palette? item) {
        item = null;
        return value is null ? null
            : toSeq(value.Split(';')).Traverse(static field =>
                Invariant.Number<int>(field, NumberStyles.None).Bind(static key => Swatch.Validated(key).ToOption())).As().Case is Seq<Swatch> colors
                ? Validate(colors, out item)
                : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"palette '{value}' must be ';'-separated RGB integers in [{Swatch.MinValue}, {Swatch.MaxValue}]"));
    }

    static ValidationError? IObjectFactory<Palette, Seq<Swatch>, ValidationError>.Validate(Seq<Swatch> value, IFormatProvider? provider, out Palette? item) => Validate(value, out item);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<Swatch> colors) =>
        validationError = ColorCount.Validate(colors.Count, provider: null, out _);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteDither : ICaptioned {
    public static readonly PaletteDither Bayer2 = new("bayer-2", KnownDitherings.Bayer2x2, "Bayer 2 × 2");
    public static readonly PaletteDither Ordered3 = new("ordered-3", KnownDitherings.Ordered3x3, "Ordered 3 × 3");
    public static readonly PaletteDither Bayer4 = new("bayer-4", KnownDitherings.Bayer4x4, "Bayer 4 × 4");
    public static readonly PaletteDither Bayer8 = new("bayer-8", KnownDitherings.Bayer8x8, "Bayer 8 × 8");
    public static readonly PaletteDither Bayer16 = new("bayer-16", KnownDitherings.Bayer16x16, "Bayer 16 × 16");
    public static readonly PaletteDither FloydSteinberg = new("floyd-steinberg", KnownDitherings.FloydSteinberg, "Floyd-Steinberg");
    public static readonly PaletteDither Atkinson = new("atkinson", KnownDitherings.Atkinson, "Atkinson");
    public static readonly PaletteDither Burkes = new("burkes", KnownDitherings.Burks, "Burkes");
    public static readonly PaletteDither JarvisJudiceNinke = new("jarvis-judice-ninke", KnownDitherings.JarvisJudiceNinke, "Jarvis-Judice-Ninke");
    public static readonly PaletteDither Stucki = new("stucki", KnownDitherings.Stucki, "Stucki");
    public static readonly PaletteDither Sierra2 = new("sierra-2", KnownDitherings.Sierra2, "Sierra 2");
    public static readonly PaletteDither Sierra3 = new("sierra-3", KnownDitherings.Sierra3, "Sierra 3");
    public static readonly PaletteDither SierraLite = new("sierra-lite", KnownDitherings.SierraLite, "Sierra Lite");
    public static readonly PaletteDither StevensonArce = new("stevenson-arce", KnownDitherings.StevensonArce, "Stevenson-Arce");

    internal IDither Kernel { get; }
    public string Caption { get; }
}

public sealed record Posterize(LevelCount Levels, AxisFraction RetainHue, AxisFraction DitherStrength, Gated<ShortSideExtent> DitherCell)
    : IStateRecord<Posterize, PosterizeParameter>, IPixelStage<Posterize> {
    public static Posterize Default { get; } = new(LevelCount.Eighths, AxisFraction.MinValue, AxisFraction.MinValue, new(Enabled: false, ShortSideExtent.MinValue));

    public static Option<PixelPass> Pass(Posterize state, PassContext context) => Some<PixelPass>(new PixelPass.Pointwise(Rows(state, context.Extent)));

    private static Action<Span<Vector4>, long, long> Rows(Posterize state, PixelExtent extent) {
        double steps = state.Levels - 1;
        ERational cell = state.DitherCell.Active.Map(size => ERational.FromSingle(size).Multiply(extent.ShortSide)).IfNone(ERational.One);
        (float strength, float retain, int height) = (state.DitherStrength, state.RetainHue, extent.Height);
        return (row, column, line) => {
            int v = WrapMode.Periodic.Fold(ERational.FromInt64((2L * (height - line)) - 1L).Divide(2).Divide(cell), 8).Point.ToEInteger().ToInt32Checked();
            for (int x = 0; x < row.Length; x++) {
                int b = WrapMode.Periodic.Fold(ERational.FromInt64((2L * (column + x)) + 1L).Divide(2).Divide(cell), 8).Point.ToEInteger().ToInt32Checked() ^ v;
                int rank = ((b & 1) << 5) | ((v & 1) << 4) | ((b & 2) << 2) | ((v & 2) << 1) | ((b & 4) >> 1) | ((v & 4) >> 2);
                double offset = strength * (((rank + 0.5d) / 64d) - 0.5d);
                Vector256<double> codes = Vector256.Floor((Vector256.Clamp(Vector256.Create(row[x].X, row[x].Y, row[x].Z, 0d), Vector256<double>.Zero, Vector256.Create(1d)) * steps) + Vector256.Create(0.5d + offset)) / steps;
                Vector4 level = new((float)codes.GetElement(0), (float)codes.GetElement(1), (float)codes.GetElement(2), row[x].W);
                row[x] = retain > 0f
                    ? new Vector4(Vector3.Lerp(level.AsVector3(), (Hsy.From(level) with { Hue = Hsy.From(row[x]).Hue }).ToRgb(row[x].W).AsVector3(), retain), row[x].W)
                    : level;
            }
        };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PosterizeParameter : IStateParameter<Posterize> {
    public static readonly PosterizeParameter Levels = new("levels", StateParameter.Bounded(Lens<Posterize, LevelCount>.New(static posterize => posterize.Levels, static levels => posterize => posterize with { Levels = levels }), new Presentation<int> { Soft = (2, 256) }),
        new("Levels", "Even output levels per channel from black to white"));
    public static readonly PosterizeParameter RetainHue = new("retain-hue", StateParameter.Bounded(Lens<Posterize, AxisFraction>.New(static posterize => posterize.RetainHue, static retain => posterize => posterize with { RetainHue = retain })),
        new("Retain hue", "Share of input hue kept at rounded saturation and luma"));
    public static readonly PosterizeParameter DitherStrength = new("dither-strength", StateParameter.Bounded(Lens<Posterize, AxisFraction>.New(static posterize => posterize.DitherStrength, static strength => posterize => posterize with { DitherStrength = strength })),
        new("Dither amount", "Dither offset, 0% rounding to nearest level or color"));
    public static readonly PosterizeParameter DitherCell = new("dither-cell", StateParameter.OptionalBounded(Lens<Posterize, Gated<ShortSideExtent>>.New(static posterize => posterize.DitherCell, static cell => posterize => posterize with { DitherCell = cell }), ShortSideExtent.Presentation with { Soft = (0.001f, 0.25f) }),
        new("Dither cell", "Ordered dither cell in frame short sides, off drawing one cell per pixel"));

    public StateParameter<Posterize> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PaletteReduction(ColorCount Colors, PaletteDither Dithering, AxisFraction DitherStrength)
    : IStateRecord<PaletteReduction, PaletteReductionParameter>, IPixelStage<PaletteReduction> {
    public static PaletteReduction Default { get; } = new(ColorCount.MaxValue, PaletteDither.FloydSteinberg, AxisFraction.MaxValue);

    public static Option<PixelPass> Pass(PaletteReduction state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(Reduction.Kernel(new WuQuantizer(Reduction.Options(state.Colors, state.Dithering, state.DitherStrength)))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteReductionParameter : IStateParameter<PaletteReduction> {
    public static readonly PaletteReductionParameter Colors = new("colors", StateParameter.Bounded(Lens<PaletteReduction, ColorCount>.New(static reduction => reduction.Colors, static colors => reduction => reduction with { Colors = colors }), new Presentation<int> { Scale = TrackScale.Log }),
        new("Colors", "Palette size the frame's colors split into by variance"));
    public static readonly PaletteReductionParameter Dithering = new("dithering", new StateParameter<PaletteReduction>.Choice<PaletteDither>(Lens<PaletteReduction, PaletteDither>.New(static reduction => reduction.Dithering, static dithering => reduction => reduction with { Dithering = dithering })),
        new("Dither", "Ordered matrix or error diffusion kernel spreading rounding error"));
    public static readonly PaletteReductionParameter DitherStrength = new("dither-strength", StateParameter.Bounded(Lens<PaletteReduction, AxisFraction>.New(static reduction => reduction.DitherStrength, static strength => reduction => reduction with { DitherStrength = strength })),
        new("Dither amount", "Dither offset, 0% rounding to nearest level or color"));

    public StateParameter<PaletteReduction> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PaletteMapping(Palette Palette, PaletteDither Dithering, AxisFraction DitherStrength)
    : IStateRecord<PaletteMapping, PaletteMappingParameter>, IPixelStage<PaletteMapping> {
    public static PaletteMapping Default { get; } = new(Palette.CubeCorners, PaletteDither.FloydSteinberg, AxisFraction.MaxValue);

    public static Option<PixelPass> Pass(PaletteMapping state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(Reduction.Kernel(new PaletteQuantizer(
            state.Palette.Colors.Map(static swatch => SixLabors.ImageSharp.Color.FromScaledVector(swatch.Display)).ToArray(),
            Reduction.Options(ColorCount.MaxValue, state.Dithering, state.DitherStrength)))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteMappingParameter : IStateParameter<PaletteMapping> {
    public static readonly PaletteMappingParameter Palette = new("palette", new StateParameter<PaletteMapping>.Swatches<Palette>(Lens<PaletteMapping, Palette>.New(static mapping => mapping.Palette, static palette => mapping => mapping with { Palette = palette })),
        new("Palette", "Colors each pixel maps to by nearest match"));
    public static readonly PaletteMappingParameter Dithering = new("dithering", new StateParameter<PaletteMapping>.Choice<PaletteDither>(Lens<PaletteMapping, PaletteDither>.New(static mapping => mapping.Dithering, static dithering => mapping => mapping with { Dithering = dithering })),
        new("Dither", "Ordered matrix or error diffusion kernel spreading rounding error"));
    public static readonly PaletteMappingParameter DitherStrength = new("dither-strength", StateParameter.Bounded(Lens<PaletteMapping, AxisFraction>.New(static mapping => mapping.DitherStrength, static strength => mapping => mapping with { DitherStrength = strength })),
        new("Dither amount", "Dither offset, 0% rounding to nearest level or color"));

    public StateParameter<PaletteMapping> Kind { get; }
    public ParameterText Text { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Reduction {
    public static QuantizerOptions Options(ColorCount colors, PaletteDither dithering, AxisFraction strength) =>
        new() {
            MaxColors = colors,
            Dither = strength == AxisFraction.MinValue ? null : dithering.Kernel,
            DitherScale = strength,
            ColorMatchingMode = ColorMatchingMode.Exact,
        };

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> Kernel(IQuantizer quantizer) =>
        (frame, progress) => frame.Mutate(image => image.Mutate(context => context.Quantize(quantizer))).Map(_ => {
            progress.Report(frame.Size.Height);
            return unit;
        });
}
