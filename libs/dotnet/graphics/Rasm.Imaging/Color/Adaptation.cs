using System.Globalization;
using System.Numerics;
using UnitsNet.Units;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ColorTemperature : IMinMaxValue<ColorTemperature> {
    public static ColorTemperature MinValue { get; } = new(800d);
    public static ColorTemperature MaxValue { get; } = new(100_000d);
    public static ColorTemperature ViewDefault { get; } = new(6500d);
    public static ColorTemperature Flame { get; } = new(2000d);
    public static UnitsNet.UnitInfo Unit { get; } = UnitsNet.Quantity.GetUnitInfo(TemperatureUnit.Kelvin);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Tint : IMinMaxValue<Tint> {
    public static Tint MinValue { get; } = new(-0.05d);
    public static Tint MaxValue { get; } = new(0.05d);
    public static Tint ViewDefault { get; } = new(0.003333d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StandardIlluminant {
    public static readonly StandardIlluminant A = new("A", 2856d, 0d);
    public static readonly StandardIlluminant B = new("B", 4873d, -0.001267d);
    public static readonly StandardIlluminant C = new("C", 6774d, -0.002133d);
    public static readonly StandardIlluminant D50 = new("D50", 5002d, 0.0032d);
    public static readonly StandardIlluminant D55 = new("D55", 5501d, 0.003333d);
    public static readonly StandardIlluminant D65 = new("D65", 6502d, 0.003267d);
    public static readonly StandardIlluminant D75 = new("D75", 7506d, 0.0032d);
    public static readonly StandardIlluminant D93 = new("D93", 9298d, 0.003167d);
    public static readonly StandardIlluminant E = new("E", 5454d, -0.004333d);
    public static readonly StandardIlluminant F1 = new("F1", 6424d, 0.007267d);
    public static readonly StandardIlluminant F2 = new("F2", 4225d, 0.001967d);
    public static readonly StandardIlluminant F3 = new("F3", 3447d, 0.000833d);
    public static readonly StandardIlluminant F4 = new("F4", 2940d, -0.000667d);
    public static readonly StandardIlluminant F5 = new("F5", 6342d, 0.0109d);
    public static readonly StandardIlluminant F6 = new("F6", 4148d, 0.0062d);
    public static readonly StandardIlluminant F7 = new("F7", 6489d, 0.003333d);
    public static readonly StandardIlluminant F8 = new("F8", 4995d, 0.003233d);
    public static readonly StandardIlluminant F9 = new("F9", 4147d, 0.0001d);
    public static readonly StandardIlluminant F10 = new("F10", 4991d, 0.0037d);
    public static readonly StandardIlluminant F11 = new("F11", 4001d, 0.000167d);
    public static readonly StandardIlluminant F12 = new("F12", 3002d, 0.0002d);
    public static readonly StandardIlluminant LedB1 = new("LED-B1", 2733d, -0.000667d);
    public static readonly StandardIlluminant LedB2 = new("LED-B2", 2997d, -0.000933d);
    public static readonly StandardIlluminant LedB3 = new("LED-B3", 4103d, -0.0006d);
    public static readonly StandardIlluminant LedB4 = new("LED-B4", 5108d, 0.000533d);
    public static readonly StandardIlluminant LedB5 = new("LED-B5", 6598d, 0.0009d);
    public static readonly StandardIlluminant LedBh1 = new("LED-BH1", 2852d, -0.0003d);
    public static readonly StandardIlluminant LedRgb1 = new("LED-RGB1", 2840d, 0.004267d);
    public static readonly StandardIlluminant LedV1 = new("LED-V1", 3079d, 0.0163d);
    public static readonly StandardIlluminant LedV2 = new("LED-V2", 4070d, 0.0011d);

    private StandardIlluminant(string key, double kelvin, double duv) : this(key, ColorTemperature.Create(kelvin), Tint.Create(duv)) { }

    public ColorTemperature Temperature { get; }
    public Tint Tint { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Adaptation {
    public static WhitePoint White(ColorTemperature temperature, Tint tint) =>
        new Unicolour(Configuration.Default, new Wacton.Unicolour.Temperature(temperature, tint)).Chromaticity switch {
            var (x, y) => new WhitePoint(x, y),
        };

    public static Unicolour Peaked(Unicolour color) =>
        color.RgbLinear switch {
            var rgb => double.Max(double.Abs(rgb.R), double.Max(double.Abs(rgb.G), double.Abs(rgb.B))) switch {
                var peak => new Unicolour(color.Configuration, ColourSpace.RgbLinear, rgb.R / peak, rgb.G / peak, rgb.B / peak),
            },
        };

    public static Unicolour Blackbody(Gamut gamut, ColorTemperature temperature) =>
        new(gamut.Configuration, Peaked(new Unicolour(gamut.Configuration, temperature)).MapToRgbGamut().Chromaticity);

    public static Fin<(ColorTemperature Temperature, Tint Tint)> Correlate(Gamut gamut, Vector3 color) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, color.X, color.Y, color.Z) switch {
            { Xyz.Y: <= 0d and var luminance } => Error.New(string.Create(CultureInfo.InvariantCulture, $"picked white of luminance {luminance} has no luminance to correlate")),
            { Temperature: { IsValid: true } correlated } =>
                (ColorTemperature.Validated(correlated.Cct), Tint.Validated(correlated.Duv)).Apply(static (temperature, tint) => (temperature, tint)).As(),
            var unresolved => Error.New(string.Create(CultureInfo.InvariantCulture, $"picked white at ({unresolved.Chromaticity.X}, {unresolved.Chromaticity.Y}) has no temperature and tint")),
        };
}
