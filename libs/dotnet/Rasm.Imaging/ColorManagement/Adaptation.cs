using System.Numerics;
using UnitsNet.Units;
using Wacton.Unicolour;

namespace Rasm.Imaging.ColorManagement;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct ColorTemperature : IMinMaxValue<ColorTemperature> {
    public static ColorTemperature MinValue { get; } = new(800d);
    public static ColorTemperature MaxValue { get; } = new(100_000d);
    public static ColorTemperature ViewDefault { get; } = new(6500d);
    public static ColorTemperature Flame { get; } = new(2000d);
    public static UnitsNet.UnitInfo Unit { get; } = UnitsNet.Quantity.GetUnitInfo(TemperatureUnit.Kelvin);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct Tint : IMinMaxValue<Tint> {
    public static Tint MinValue { get; } = new(-0.05d);
    public static Tint MaxValue { get; } = new(0.05d);
    public static Tint ViewDefault { get; } = new(0.003333d);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[SmartEnum]
public sealed partial class StandardIlluminant {
    public static readonly StandardIlluminant A = new(2856d, 0d);
    public static readonly StandardIlluminant B = new(4873d, -0.001267d);
    public static readonly StandardIlluminant C = new(6774d, -0.002133d);
    public static readonly StandardIlluminant D50 = new(5002d, 0.0032d);
    public static readonly StandardIlluminant D55 = new(5501d, 0.003333d);
    public static readonly StandardIlluminant D65 = new(6502d, 0.003267d);
    public static readonly StandardIlluminant D75 = new(7506d, 0.0032d);
    public static readonly StandardIlluminant D93 = new(9298d, 0.003167d);
    public static readonly StandardIlluminant E = new(5454d, -0.004333d);
    public static readonly StandardIlluminant F1 = new(6424d, 0.007267d);
    public static readonly StandardIlluminant F2 = new(4225d, 0.001967d);
    public static readonly StandardIlluminant F3 = new(3447d, 0.000833d);
    public static readonly StandardIlluminant F4 = new(2940d, -0.000667d);
    public static readonly StandardIlluminant F5 = new(6342d, 0.0109d);
    public static readonly StandardIlluminant F6 = new(4148d, 0.0062d);
    public static readonly StandardIlluminant F7 = new(6489d, 0.003333d);
    public static readonly StandardIlluminant F8 = new(4995d, 0.003233d);
    public static readonly StandardIlluminant F9 = new(4147d, 0.0001d);
    public static readonly StandardIlluminant F10 = new(4991d, 0.0037d);
    public static readonly StandardIlluminant F11 = new(4001d, 0.000167d);
    public static readonly StandardIlluminant F12 = new(3002d, 0.0002d);
    public static readonly StandardIlluminant LedB1 = new(2733d, -0.000667d);
    public static readonly StandardIlluminant LedB2 = new(2997d, -0.000933d);
    public static readonly StandardIlluminant LedB3 = new(4103d, -0.0006d);
    public static readonly StandardIlluminant LedB4 = new(5108d, 0.000533d);
    public static readonly StandardIlluminant LedB5 = new(6598d, 0.0009d);
    public static readonly StandardIlluminant LedBh1 = new(2852d, -0.0003d);
    public static readonly StandardIlluminant LedRgb1 = new(2840d, 0.004267d);
    public static readonly StandardIlluminant LedV1 = new(3079d, 0.0163d);
    public static readonly StandardIlluminant LedV2 = new(4070d, 0.0011d);

    private StandardIlluminant(double kelvin, double duv) : this(ColorTemperature.Create(kelvin), Tint.Create(duv)) { }

    public ColorTemperature Temperature { get; }
    public Tint Tint { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Adaptation {
    public static WhitePoint White(ColorTemperature temperature, Tint tint) =>
        new Unicolour(Configuration.Default, new Wacton.Unicolour.Temperature(temperature, tint)).Chromaticity switch {
            var (x, y) => new WhitePoint(x, y),
        };

    public static Unicolour Blackbody(Gamut gamut, ColorTemperature temperature) =>
        new Unicolour(gamut.Configuration, temperature).RgbLinear switch {
            var planck => double.Max(planck.R, double.Max(planck.G, planck.B)) switch {
                var peak => new Unicolour(
                    gamut.Configuration,
                    new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, planck.R / peak, planck.G / peak, planck.B / peak).MapToRgbGamut().Chromaticity),
            },
        };

    public static Fin<(ColorTemperature Temperature, Tint Tint)> Correlate(Gamut gamut, Vector3 color) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, color.X, color.Y, color.Z) switch {
            { Xyz.Y: <= 0d and var luminance } => new WhiteWithoutLuminance(luminance),
            { Temperature: { IsValid: true } correlated } =>
                from temperature in ColorTemperature.Validate(correlated.Cct, provider: null, out ColorTemperature kelvin) is { } refused ? refused : (Fin<ColorTemperature>)kelvin
                from tint in Tint.Validate(correlated.Duv, provider: null, out Tint duv) is { } offLocus ? offLocus : (Fin<Tint>)duv
                select (temperature, tint),
            var unresolved => new WhiteUnresolved(unresolved.Chromaticity),
        };
}
