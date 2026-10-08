using System.Globalization;
using System.Numerics;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct LevelCount : IMinMaxValue<LevelCount> {
    public static LevelCount MinValue { get; } = new(2);
    public static LevelCount MaxValue { get; } = new(256);
    public static LevelCount Eighths { get; } = new(8 + 1);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct ColorCount : IMinMaxValue<ColorCount> {
    public static ColorCount MinValue { get; } = new(QuantizerConstants.MinColors);
    public static ColorCount MaxValue { get; } = new(QuantizerConstants.MaxColors);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ComplexValueObject]
[ValidationError<InvalidStylize>]
[ObjectFactory<string>]
[ObjectFactory<Seq<Swatch>>]
public sealed partial class Palette : IConvertible<string>, IConvertible<Seq<Swatch>> {
    public Seq<Swatch> Colors { get; }

    public static Palette CubeCorners { get; } = new(Seq(
        Swatch.Black, Swatch.Create(0xFF0000), Swatch.Create(0x00FF00), Swatch.Create(0x0000FF),
        Swatch.Create(0xFFFF00), Swatch.Create(0xFF00FF), Swatch.Create(0x00FFFF), Swatch.White));

    public string ToValue() => string.Join(';', Colors.Map(static swatch => ((int)swatch).ToString(CultureInfo.InvariantCulture)));

    Seq<Swatch> IConvertible<Seq<Swatch>>.ToValue() => Colors;

    static InvalidStylize? IObjectFactory<Palette, string, InvalidStylize>.Validate(string? value, IFormatProvider? provider, out Palette? item) {
        item = null;
        return value is null ? null
            : toSeq(value.Split(';')).Traverse(static field =>
                int.TryParse(field, NumberStyles.None, CultureInfo.InvariantCulture, out int key) && Swatch.Validate(key, provider: null, out Swatch swatch) is null ? Some(swatch) : None).As().Case is Seq<Swatch> colors
                ? Validate(colors, out item)
                : new InvalidStylize();
    }

    static InvalidStylize? IObjectFactory<Palette, Seq<Swatch>, InvalidStylize>.Validate(Seq<Swatch> value, IFormatProvider? provider, out Palette? item) => Validate(value, out item);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref Seq<Swatch> colors) =>
        validationError = ColorCount.Validate(colors.Count, provider: null, out _);
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteDither {
    public static readonly PaletteDither Bayer2 = new("bayer-2", KnownDitherings.Bayer2x2);
    public static readonly PaletteDither Ordered3 = new("ordered-3", KnownDitherings.Ordered3x3);
    public static readonly PaletteDither Bayer4 = new("bayer-4", KnownDitherings.Bayer4x4);
    public static readonly PaletteDither Bayer8 = new("bayer-8", KnownDitherings.Bayer8x8);
    public static readonly PaletteDither Bayer16 = new("bayer-16", KnownDitherings.Bayer16x16);
    public static readonly PaletteDither FloydSteinberg = new("floyd-steinberg", KnownDitherings.FloydSteinberg);
    public static readonly PaletteDither Atkinson = new("atkinson", KnownDitherings.Atkinson);
    public static readonly PaletteDither Burkes = new("burkes", KnownDitherings.Burks);
    public static readonly PaletteDither JarvisJudiceNinke = new("jarvis-judice-ninke", KnownDitherings.JarvisJudiceNinke);
    public static readonly PaletteDither Stucki = new("stucki", KnownDitherings.Stucki);
    public static readonly PaletteDither Sierra2 = new("sierra-2", KnownDitherings.Sierra2);
    public static readonly PaletteDither Sierra3 = new("sierra-3", KnownDitherings.Sierra3);
    public static readonly PaletteDither SierraLite = new("sierra-lite", KnownDitherings.SierraLite);
    public static readonly PaletteDither StevensonArce = new("stevenson-arce", KnownDitherings.StevensonArce);

    internal IDither Kernel { get; }
}

public sealed record Posterize(LevelCount Levels, Mix RetainHue, Mix DitherStrength, Option<CellSize> DitherCell)
    : IStateRecord<Posterize, PosterizeParameter, InvalidStylize>, IPixelStage<Posterize> {
    public static Posterize Default { get; } = new(LevelCount.Eighths, Mix.Full, Mix.MinValue, None);

    public static Option<PixelPass> Pass(Posterize state, PassContext context) => Some<PixelPass>(new PixelPass.Pointwise(Rows(state, context.Extent)));

    private static Action<Span<Vector4>, int, int> Rows(Posterize state, PixelExtent extent) {
        float steps = state.Levels - 1;
        float cell = state.DitherCell.Map(size => float.Max(1f, size.Pixels(extent))).IfNone(1f);
        (float strength, float retain, int height) = (state.DitherStrength, state.RetainHue, extent.Height);
        return (row, column, line) => {
            int v = (int)float.Floor((height - line - 0.5f) / cell) & 7;
            for (int x = 0; x < row.Length; x++) {
                int b = ((int)float.Floor((column + x + 0.5f) / cell) & 7) ^ v;
                int rank = ((b & 1) << 5) | ((v & 1) << 4) | ((b & 2) << 2) | ((v & 2) << 1) | ((b & 4) >> 1) | ((v & 4) >> 2);
                float offset = strength * (((rank + 0.5f) / 64f) - 0.5f);
                Vector4 level = new(Vector3.Clamp(Vector3.Round((row[x].AsVector3() * steps) + new Vector3(0.5f + offset), MidpointRounding.ToNegativeInfinity), Vector3.Zero, new Vector3(steps)) / steps, row[x].W);
                row[x] = retain > 0f ? Vector4.Lerp(level, (Hsy.From(level) with { Hue = Hsy.From(row[x]).Hue }).ToRgb(row[x].W), retain) : level;
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PosterizeParameter : IStateParameter<Posterize> {
    public static readonly PosterizeParameter Levels = new(
        "levels", new StateParameter<Posterize>.Bounded<LevelCount, int, InvalidStylize>(
            Lens<Posterize, LevelCount>.New(static posterize => posterize.Levels, static levels => posterize => posterize with { Levels = levels }), new()));
    public static readonly PosterizeParameter RetainHue = new(
        "retain-hue", new StateParameter<Posterize>.Bounded<Mix, float, InvalidGrade>(
            Lens<Posterize, Mix>.New(static posterize => posterize.RetainHue, static retain => posterize => posterize with { RetainHue = retain }), new()));
    public static readonly PosterizeParameter DitherStrength = new(
        "dither-strength", new StateParameter<Posterize>.Bounded<Mix, float, InvalidGrade>(
            Lens<Posterize, Mix>.New(static posterize => posterize.DitherStrength, static strength => posterize => posterize with { DitherStrength = strength }), new()));
    public static readonly PosterizeParameter DitherCell = new(
        "dither-cell", new StateParameter<Posterize>.OptionalBounded<CellSize, float, InvalidStylize>(
            Lens<Posterize, Option<CellSize>>.New(static posterize => posterize.DitherCell, static cell => posterize => posterize with { DitherCell = cell }),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));

    public StateParameter<Posterize> Kind { get; }
}

public sealed record PaletteReduction(ColorCount Colors, PaletteDither Dithering, Mix DitherStrength)
    : IStateRecord<PaletteReduction, PaletteReductionParameter, InvalidStylize>, IPixelStage<PaletteReduction> {
    public static PaletteReduction Default { get; } = new(ColorCount.MaxValue, PaletteDither.FloydSteinberg, Mix.Full);

    public static Option<PixelPass> Pass(PaletteReduction state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(Reduction.Kernel(new WuQuantizer(Reduction.Options(state.Colors, state.Dithering, state.DitherStrength)))));
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteReductionParameter : IStateParameter<PaletteReduction> {
    public static readonly PaletteReductionParameter Colors = new(
        "colors", new StateParameter<PaletteReduction>.Bounded<ColorCount, int, InvalidStylize>(
            Lens<PaletteReduction, ColorCount>.New(static reduction => reduction.Colors, static colors => reduction => reduction with { Colors = colors }),
            new() { Scale = TrackScale.Log }));
    public static readonly PaletteReductionParameter Dithering = new(
        "dithering", new StateParameter<PaletteReduction>.Choice<PaletteDither, InvalidStylize>(
            Lens<PaletteReduction, PaletteDither>.New(static reduction => reduction.Dithering, static dithering => reduction => reduction with { Dithering = dithering })));
    public static readonly PaletteReductionParameter DitherStrength = new(
        "dither-strength", new StateParameter<PaletteReduction>.Bounded<Mix, float, InvalidGrade>(
            Lens<PaletteReduction, Mix>.New(static reduction => reduction.DitherStrength, static strength => reduction => reduction with { DitherStrength = strength }), new()));

    public StateParameter<PaletteReduction> Kind { get; }
}

public sealed record PaletteMapping(Palette Palette, PaletteDither Dithering, Mix DitherStrength)
    : IStateRecord<PaletteMapping, PaletteMappingParameter, InvalidStylize>, IPixelStage<PaletteMapping> {
    public static PaletteMapping Default { get; } = new(Palette.CubeCorners, PaletteDither.FloydSteinberg, Mix.Full);

    public static Option<PixelPass> Pass(PaletteMapping state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(Reduction.Kernel(new PaletteQuantizer(
            state.Palette.Colors.Map(static swatch => Color.FromScaledVector(swatch.Display)).ToArray(),
            Reduction.Options(ColorCount.MaxValue, state.Dithering, state.DitherStrength)))));
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteMappingParameter : IStateParameter<PaletteMapping> {
    public static readonly PaletteMappingParameter Palette = new(
        "palette", new StateParameter<PaletteMapping>.Swatches<Palette, InvalidStylize>(
            Lens<PaletteMapping, Palette>.New(static mapping => mapping.Palette, static palette => mapping => mapping with { Palette = palette })));
    public static readonly PaletteMappingParameter Dithering = new(
        "dithering", new StateParameter<PaletteMapping>.Choice<PaletteDither, InvalidStylize>(
            Lens<PaletteMapping, PaletteDither>.New(static mapping => mapping.Dithering, static dithering => mapping => mapping with { Dithering = dithering })));
    public static readonly PaletteMappingParameter DitherStrength = new(
        "dither-strength", new StateParameter<PaletteMapping>.Bounded<Mix, float, InvalidGrade>(
            Lens<PaletteMapping, Mix>.New(static mapping => mapping.DitherStrength, static strength => mapping => mapping with { DitherStrength = strength }), new()));

    public StateParameter<PaletteMapping> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Reduction {
    public static QuantizerOptions Options(ColorCount colors, PaletteDither dithering, Mix strength) =>
        new() {
            MaxColors = colors,
            Dither = strength == Mix.MinValue ? null : dithering.Kernel,
            DitherScale = strength,
            ColorMatchingMode = ColorMatchingMode.Exact,
        };

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> Kernel(IQuantizer quantizer) =>
        (frame, progress) => {
            using Image<RgbaVector> image = frame.Image();
            image.Mutate(context => context.Quantize(quantizer));
            frame.Write(image);
            progress.Report(frame.Size.Height);
            return unit;
        };
}
