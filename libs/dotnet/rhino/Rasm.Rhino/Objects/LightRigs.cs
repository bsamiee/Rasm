using System.Globalization;
using LanguageExt.ClassInstances;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Front", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Azimuth : System.Numerics.IMinMaxValue<Azimuth> {
    public static Azimuth MinValue { get; } = new(-Math.PI);
    public static Azimuth MaxValue { get; } = new(Math.PI);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Horizon", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Elevation : System.Numerics.IMinMaxValue<Elevation> {
    public static Elevation MinValue { get; } = new(-Math.PI / 2d);
    public static Elevation MaxValue { get; } = new(Math.PI / 2d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct RadiusMultiple : System.Numerics.IMinMaxValue<RadiusMultiple> {
    public static RadiusMultiple MinValue { get; } = new(0.3);
    public static RadiusMultiple MaxValue { get; } = new(50d);
    public static RadiusMultiple Default { get; } = new(2.5);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct AngularDiameter : System.Numerics.IMinMaxValue<AngularDiameter> {
    public static AngularDiameter MinValue { get; } = new(0d);
    public static AngularDiameter MaxValue { get; } = new(Math.PI / 2d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "AtKey", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct LightStops : System.Numerics.IMinMaxValue<LightStops> {
    public static LightStops MinValue { get; } = new((float)Exposure.MinValue);
    public static LightStops MaxValue { get; } = new((float)Exposure.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TargetRadius : System.Numerics.IMinMaxValue<TargetRadius> {
    public static TargetRadius MinValue { get; } = new(double.BitIncrement(0d));
    public static TargetRadius MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct KeySoftness : System.Numerics.IMinMaxValue<KeySoftness> {
    public static KeySoftness MinValue { get; } = new(0d);
    public static KeySoftness MaxValue { get; } = new(10d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ColorSaturation : System.Numerics.IMinMaxValue<ColorSaturation> {
    public static ColorSaturation MinValue { get; } = new(0d);
    public static ColorSaturation MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct LightCount : System.Numerics.IMinMaxValue<LightCount> {
    public static LightCount MinValue { get; } = new(1);
    public static LightCount MaxValue { get; } = new(8);
    public static LightCount Default { get; } = new(3);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[SmartEnum<string>(KeyMemberName = "Name", SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightRole {
    public static readonly LightRole Key = new("key");
    public static readonly LightRole Fill = new("fill");
    public static readonly LightRole Rim = new("rim");
    public static readonly LightRole Accent = new("accent");
    public static readonly LightRole Top = new("top");
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigEmitter {
    public sealed record PointLight() : RigEmitter;

    public sealed record SpotLight(SpotAngle Angle, HotSpot HotSpot) : RigEmitter;

    public sealed record DirectionalLight() : RigEmitter;

    public sealed record RectangularLight() : RigEmitter;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigColor {
    public sealed record Blackbody(ColorTemperature Kelvin) : RigColor;

    public sealed record Linear(ColorFilter Filter) : RigColor;
}

public sealed record RigLight(
    LightRole Role, RigEmitter Emitter, Azimuth Azimuth, Elevation Elevation, RadiusMultiple Distance,
    AngularDiameter Size, LightStops Stops, RigColor Color, Light.Attenuation Falloff) {
    public (Vector3d Toward, Vector3d Across, Vector3d Rise) Axes =>
        (Math.SinCos(Azimuth), Math.SinCos(Elevation)) switch {
            var ((sinA, cosA), (sinE, cosE)) => (new Vector3d(cosE * sinA, sinE, cosE * cosA), new Vector3d(cosA, 0d, -sinA), new Vector3d(-sinE * sinA, cosE, -sinE * cosA)),
        };

    public DistantLight Distant =>
        (Axes.Toward, Color.Switch(
            blackbody: static blackbody => Adaptation.Blackbody(Gamut.StandardRgb, blackbody.Kelvin),
            linear: static linear => linear.Filter.Unicolour)) switch {
                var (toward, color) => (color.RgbLinear, color.Xyz.Y) switch {
                    var (linear, luminance) => new(
                        new System.Numerics.Vector3((float)toward.X, (float)toward.Y, (float)toward.Z),
                        new System.Numerics.Vector3((float)(linear.R / luminance), (float)(linear.G / luminance), (float)(linear.B / luminance)), (float)(double)Stops),
                },
            };
}

public sealed record MatchedLook(
    LightCount Count, RigEmitter Key, KeySoftness Softness, AngularDiameter FillSize, Option<LightStops> Fill,
    Option<LightStops> Rim, ColorFilter Tint, ColorSaturation Saturation, RadiusMultiple Distance) {
    public Fin<Seq<RigLight>> Derived(LightingReading reading) =>
        from azimuth in Fin.Succ(reading.Direction.X * Math.PI / 2d)
        let elevation = reading.Direction.Y * Math.PI / 2d
        let size = reading.Hardness.Filter(static hardness => hardness > 0f).Match(Some: hardness => Math.Atan2(Softness * reading.Ratio, hardness), None: static () => (double)AngularDiameter.MaxValue)
        let behind = reading.Backlight.Exists(static stops => stops > Math.Log2(1.35)) ? Math.PI : Math.IEEERemainder(azimuth + Math.PI, Math.Tau)
        let colors = reading.Gels.Match(
            Some: static gels => (Key: gels.Key.Color, Fill: gels.Accent.Color, Rim: gels.Accent.Color),
            None: () => (Key: reading.KeyColor, Fill: reading.FillColor, Rim: reading.KeyColor))
        let keyed = reading.Gels.Map(static gels => gels.Key) | reading.Palette.Head
        from lights in (Seq(Row(LightRole.Key, Key, azimuth, elevation, size, LightStops.AtKey, colors.Key),
                    Row(LightRole.Fill, new RigEmitter.RectangularLight(), -azimuth, Elevation.Horizon, FillSize, Fill.Map(static stops => (double)stops).IfNone(-reading.Ratio), colors.Fill))
                + Rim.Map(rim => Row(LightRole.Rim, Key, behind, elevation, size, rim, colors.Rim)).ToSeq()
                + reading.Palette.Filter(entry => keyed != Some(entry)).Map(entry => Row(
                    LightRole.Accent, Key, ((2d * entry.Position.X) - 1d) * Math.PI / 2d, (1d - (2d * entry.Position.Y)) * Math.PI / 2d, size,
                    Math.Log2(MathF.Max(System.Numerics.Vector3.Dot(Gamut.StandardRgb.Luminance, entry.Color), LuminanceSamples.Floor)
                        / MathF.Max(System.Numerics.Vector3.Dot(Gamut.StandardRgb.Luminance, colors.Key), LuminanceSamples.Floor)), entry.Color)))
            .Take(Count)
            .Traverse(static light => light)
            .As()
            .ToFin()
        select lights;

    private Validation<Error, RigLight> Row(LightRole role, RigEmitter emitter, double azimuth, double elevation, double size, double stops, System.Numerics.Vector3 color) =>
        (Conversions.Validated<Azimuth, double, InvalidRhinoValue>(azimuth).ToValidation(),
         Conversions.Validated<Elevation, double, InvalidRhinoValue>(elevation).ToValidation(),
         Conversions.Validated<AngularDiameter, double, InvalidRhinoValue>(size).ToValidation(),
         Conversions.Validated<LightStops, double, InvalidRhinoValue>(stops).ToValidation(),
         MathF.Max(color.X, MathF.Max(color.Y, color.Z)) switch {
             var peak => System.Numerics.Vector3.Lerp(System.Numerics.Vector3.One, peak > 0f ? color / peak : System.Numerics.Vector3.One, (float)(double)Saturation) switch {
                 var tone => ColorFilter.Validate(Tint.Red * tone.X, Tint.Green * tone.Y, Tint.Blue * tone.Z, out ColorFilter? filter) is { } error
                     ? Validation.Fail<Error, ColorFilter>(error) : Validation.Success<Error, ColorFilter>(filter!),
             },
         })
            .Apply((a, e, s, st, filter) => new RigLight(role, emitter, a, e, Distance, s, st, filter, Light.Attenuation.InverseSquared))
            .As();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigLayout {
    public sealed record Arranged(Seq<RigLight> Rows) : RigLayout;

    public sealed record Matched(MatchedLook Look) : RigLayout;

    public Fin<Seq<RigLight>> Lights(Option<LightingReading> reading) =>
        Switch(
            reading,
            arranged: static (_, arranged) => Fin.Succ(arranged.Rows),
            matched: static (held, matched) => held.ToFin(new Missing(nameof(LightingReading))).Bind(matched.Look.Derived));
}

[SmartEnum<string>(SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RigPreset {
    public static readonly RigPreset ThreePoint = new("three-point", static () => Arranged(
        new PresetLight(LightRole.Key, new Vector3d(1.6, -1.6, 0.3), 0.6, 250d, 5500d),
        new PresetLight(LightRole.Fill, new Vector3d(-1.6, -1.2, -0.1), 1.5, 90d, 6500d),
        new PresetLight(LightRole.Rim, new Vector3d(0.5, 1.6, 0.8), 0.4, 400d, 5000d)));
    public static readonly RigPreset HardLowKey = new("hard-low-key", static () => Arranged(
        new PresetLight(LightRole.Key, new Vector3d(2.0, -1.0, 0.4), 0.4, 450d, 4500d),
        new PresetLight(LightRole.Fill, new Vector3d(-1.5, -1.0, -0.2), 1.0, 25d, 7000d),
        new PresetLight(LightRole.Rim, new Vector3d(-0.4, 1.5, 0.8), 0.3, 200d, 4200d)));
    public static readonly RigPreset SideKey = new("side-key", static () => Arranged(
        new PresetLight(LightRole.Key, new Vector3d(2.4, -0.6, 1.0), 0.3, 550d, 4200d)));
    public static readonly RigPreset SoftHighKey = new("soft-high-key", static () => Arranged(
        new PresetLight(LightRole.Key, new Vector3d(1.2, -2.0, 0.5), 2.0, 300d, 5500d),
        new PresetLight(LightRole.Fill, new Vector3d(-1.2, -2.0, 0.5), 2.0, 250d, 5500d),
        new PresetLight(LightRole.Top, new Vector3d(0d, 0d, 2.2), 1.5, 180d, 5500d)));
    public static readonly RigPreset RimOnly = new("rim-only", static () => Arranged(
        new PresetLight(LightRole.Rim, new Vector3d(0d, 1.5, 1.2), 0.5, 600d, 5000d)));
    public static readonly RigPreset Glamour = new("glamour", static () => Arranged(
        new PresetLight(LightRole.Key, new Vector3d(0d, -1.6, 1.4), 0.7, 350d, 5500d),
        new PresetLight(LightRole.Fill, new Vector3d(0d, -1.0, -0.4), 1.5, 80d, 6000d)));
    public static readonly RigPreset Auto = new("auto", static () => Matched(new RigEmitter.RectangularLight(), 1.0, None, Some(1.0), (1d, 1d, 1d), 0.85, 2.5));
    public static readonly RigPreset Portrait = new("portrait", static () => Matched(new RigEmitter.RectangularLight(), 1.1, None, Some(1.0), (1d, 0.97, 0.92), 0.85, 2.4));
    public static readonly RigPreset Rembrandt = new("rembrandt", static () => Matched(SpotKey, 0.9, Some(0.12), Some(0.8), (1d, 0.95, 0.85), 0.9, 2.4));
    public static readonly RigPreset Butterfly = new("butterfly", static () => Matched(new RigEmitter.RectangularLight(), 1.3, Some(0.45), Some(0.6), (1d, 0.97, 0.95), 0.8, 2.3));
    public static readonly RigPreset Loop = new("loop", static () => Matched(new RigEmitter.RectangularLight(), 1.1, Some(0.3), Some(0.7), (1d, 0.96, 0.9), 0.85, 2.4));
    public static readonly RigPreset Split = new("split", static () => Matched(new RigEmitter.RectangularLight(), 0.8, Some(0.08), Some(0.5), (1d, 0.98, 0.95), 0.9, 2.4));
    public static readonly RigPreset Clamshell = new("clamshell", static () => Matched(new RigEmitter.RectangularLight(), 2.2, Some(0.85), None, (1d, 0.97, 0.96), 0.7, 2.2));
    public static readonly RigPreset Beauty = new("beauty", static () => Matched(new RigEmitter.RectangularLight(), 2.4, Some(0.8), Some(0.5), (1d, 0.96, 0.95), 0.75, 2.2));
    public static readonly RigPreset Cinematic = new("cinematic", static () => Matched(SpotKey, 0.7, Some(0.15), Some(1.5), (1.05, 0.95, 0.85), 1.0, 2.6));
    public static readonly RigPreset Dramatic = new("dramatic", static () => Matched(SpotKey, 0.45, Some(0.05), Some(0.8), (1d, 0.98, 0.92), 0.9, 2.5));
    public static readonly RigPreset Noir = new("noir", static () => Matched(SpotKey, 0.4, Some(0.03), Some(1.2), (0.98, 0.99, 1d), 0.95, 2.5));
    public static readonly RigPreset LowKey = new("low-key", static () => Matched(SpotKey, 0.6, Some(0.05), Some(0.6), (1d, 0.97, 0.92), 0.9, 2.5));
    public static readonly RigPreset HighKey = new("high-key", static () => Matched(new RigEmitter.RectangularLight(), 2.6, Some(0.9), None, (1d, 1d, 1d), 0.55, 2.8));
    public static readonly RigPreset Studio = new("studio", static () => Matched(new RigEmitter.RectangularLight(), 2.2, Some(0.7), None, (1d, 1d, 1d), 0.6, 2.8));
    public static readonly RigPreset Product = new("product", static () => Matched(new RigEmitter.RectangularLight(), 2.0, Some(0.6), None, (1d, 1d, 1d), 0.6, 2.7));
    public static readonly RigPreset SoftEven = new("soft-even", static () => Matched(new RigEmitter.RectangularLight(), 3.0, Some(0.95), None, (1d, 1d, 1d), 0.5, 3.0));
    public static readonly RigPreset Rim = new("rim", static () => Matched(new RigEmitter.RectangularLight(), 1.0, Some(0.3), Some(2.0), (1d, 0.98, 0.95), 0.85, 2.5));
    public static readonly RigPreset Backlight = new("backlight", static () => Matched(new RigEmitter.RectangularLight(), 1.2, Some(0.25), None, (1d, 0.97, 0.9), 0.9, 2.7));
    public static readonly RigPreset Outdoor = new("outdoor", static () => Matched(new RigEmitter.DirectionalLight(), 1.0, Some(0.4), None, (1d, 0.97, 0.9), 0.85, 3.0));
    public static readonly RigPreset Sunset = new("sunset", static () => Matched(new RigEmitter.DirectionalLight(), 1.0, Some(0.35), None, (1.1, 0.82, 0.55), 1.0, 3.0));
    public static readonly RigPreset Twilight = new("twilight", static () => Matched(new RigEmitter.RectangularLight(), 1.6, Some(0.6), None, (0.7, 0.8, 1.05), 0.9, 2.9));
    public static readonly RigPreset Neon = new("neon", static () => Matched(new RigEmitter.RectangularLight(), 1.0, Some(0.4), Some(1.4), (1d, 1d, 1d), 1.0, 2.5));
    public static readonly RigPreset Candlelight = new("candlelight", static () => Matched(new RigEmitter.PointLight(), 1.4, Some(0.5), None, (1.12, 0.72, 0.42), 1.0, 2.4));
    public static readonly RigPreset Moonlight = new("moonlight", static () => Matched(new RigEmitter.DirectionalLight(), 1.2, Some(0.3), None, (0.6, 0.72, 1.05), 0.95, 3.0));
    public static readonly RigPreset Underlight = new("underlight", static () => Matched(SpotKey, 0.9, Some(0.15), None, (1d, 0.95, 0.9), 0.9, 2.4));

    [UseDelegateFromConstructor]
    public partial Fin<RigLayout> Layout();

    private static Validation<Error, RigEmitter> SpotKey =>
        (Conversions.Validated<SpotAngle, double, InvalidRhinoValue>(RhinoMath.ToRadians(45d) / 2d).ToValidation(),
         Conversions.Validated<HotSpot, double, InvalidRhinoValue>(1d / (1d + 0.15)).ToValidation())
            .Apply(static (angle, hotSpot) => (RigEmitter)new RigEmitter.SpotLight(angle, hotSpot))
            .As();

    private static Fin<RigLayout> Arranged(PresetLight reference, params PresetLight[] others) =>
        reference.Cons(toSeq(others))
            .Traverse(light =>
                (Conversions.Validated<Azimuth, double, InvalidRhinoValue>(Math.Atan2(light.Offset.X, -light.Offset.Y)).ToValidation(),
                 Conversions.Validated<Elevation, double, InvalidRhinoValue>(Math.Atan2(light.Offset.Z, double.Hypot(light.Offset.X, light.Offset.Y))).ToValidation(),
                 Conversions.Validated<AngularDiameter, double, InvalidRhinoValue>(2d * Math.Atan(light.Side / (2d * light.Offset.Length))).ToValidation(),
                 Conversions.Validated<LightStops, double, InvalidRhinoValue>(Math.Log2(light.Watts * reference.Offset.SquareLength / (reference.Watts * light.Offset.SquareLength))).ToValidation(),
                 Conversions.Validated<ColorTemperature, double, InvalidColor>(light.Kelvin).ToValidation())
                    .Apply((a, e, s, st, k) => new RigLight(light.Role, new RigEmitter.RectangularLight(), a, e, RadiusMultiple.Default, s, st, k, Light.Attenuation.InverseSquared))
                    .As())
            .As()
            .ToFin()
            .Map(static lights => (RigLayout)new RigLayout.Arranged(lights));

    private static Fin<RigLayout> Matched(
        Validation<Error, RigEmitter> key, double softness, Option<double> fillRatio, Option<double> rimStrength,
        (double Red, double Green, double Blue) tint, double saturation, double distance) =>
        (key,
         Conversions.Validated<KeySoftness, double, InvalidRhinoValue>(softness).ToValidation(),
         fillRatio.Traverse(static ratio => Conversions.Validated<LightStops, double, InvalidRhinoValue>(Math.Log2(ratio)).ToValidation()).As(),
         rimStrength.Traverse(static strength => Conversions.Validated<LightStops, double, InvalidRhinoValue>(Math.Log2(strength)).ToValidation()).As(),
         ColorFilter.Validate(tint.Red, tint.Green, tint.Blue, out ColorFilter? filter) is { } error ? Validation.Fail<Error, ColorFilter>(error) : filter!,
         Conversions.Validated<ColorSaturation, double, InvalidRhinoValue>(saturation).ToValidation(),
         Conversions.Validated<RadiusMultiple, double, InvalidRhinoValue>(distance).ToValidation())
            .Apply(static (emitter, soft, fill, rim, filtered, saturated, reach) =>
                (RigLayout)new RigLayout.Matched(new MatchedLook(LightCount.Default, emitter, soft, AngularDiameter.MaxValue, fill, rim, filtered, saturated, reach)))
            .As()
            .ToFin();

    private sealed record PresetLight(LightRole Role, Vector3d Offset, double Side, double Watts, double Kelvin);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigTarget {
    public sealed record Subjects(IterableNE<Guid> Ids) : RigTarget;

    public sealed record Fixed(Point3d Focus, TargetRadius Radius) : RigTarget;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigBasis {
    public sealed record World() : RigBasis;

    public sealed record View(ViewportTarget Target) : RigBasis;

    public sealed record Frame(Guid Object) : RigBasis;
}

public sealed record LightRig(
    Guid Id, RigTarget Target, RigBasis Basis, LightIntensity Level, Azimuth Rotation, Elevation Elevation, RigLayout Layout, Option<RigPreset> Preset);

public sealed record RigCommit(Func<LightRole, string> Caption, string Name, RedrawPolicy Redraw);

public sealed record PlacedLight(LightRole Role, LightShape Shape, LightColor Color, LightIntensity Intensity, Vector3d AttenuationVector) {
    public LightSpec Spec(string name, bool enabled, LightPower watts, ShadowIntensity shadow) =>
        new(Conversions.Present(name), enabled, Shape, Color, Intensity, watts, shadow, AttenuationVector);
}

public sealed record RigLabel(Guid Rig, int Slot, LightRole Role, Option<RigPreset> Preset) {
    public static HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> Strings(Option<RigLabel> label) =>
        HashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(
            (LightKey.Rig.Key, label.Map(static row => row.Rig.ToString("D"))),
            (LightKey.Slot.Key, label.Map(static row => row.Slot.ToString(CultureInfo.InvariantCulture))),
            (LightKey.Role.Key, label.Map(static row => row.Role.Name)),
            (LightKey.Preset.Key, label.Bind(static row => row.Preset).Map(static preset => preset.Key)));
}

public sealed record HeldLight(Guid Id, uint Serial, HashMap<EqStringOrdinalIgnoreCase, string, string> Strings) {
    public Option<int> Slot =>
        Strings.Find(LightKey.Slot.Key).Bind(static text => Callbacks.Found(int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int slot), slot));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RigChange {
    public sealed record Create(RigLabel Label, PlacedLight Placed, Option<Guid> Source) : RigChange;

    public sealed record Place(Guid Light, PlacedLight Placed, Option<RigLabel> Label) : RigChange;

    public sealed record Release(Guid Light) : RigChange;

    public sealed record Remove(Guid Light) : RigChange;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LightRigs {
    // --- [TARGET]
    public static IO<RigTarget.Fixed> Target(RhinoDoc doc, RigTarget target) =>
        target.Switch(
            doc,
            subjects: static (document, subjects) => from found in new ObjectTarget.Ids(subjects.Ids.AsIterable().ToSeq()).Objects(document)
                                                     let bounds = Measurements.Bounds(found.Map(static held => held.Geometry.GetBoundingBox(accurate: true)))
                                                     from box in IO.lift(bounds.ToFin(new Missing(nameof(GeometryBase.GetBoundingBox))))
                                                     from radius in IO.lift(Conversions.Validated<TargetRadius, double, InvalidRhinoValue>(box.Diagonal.Length / 2d))
                                                     select new RigTarget.Fixed(box.Center, radius),
            @fixed: static (_, held) => IO.pure(held));

    public static IO<Plane> Basis(RhinoDoc doc, RigBasis basis, Point3d focus) =>
        basis.Switch(
            (Document: doc, Focus: focus),
            world: static (state, _) => IO.pure(new Plane(state.Focus, Vector3d.XAxis, Vector3d.ZAxis)),
            view: static (state, view) =>
                use(Viewports.ResolveViewport(state.Document, view.Target)).Bind(static row => Cameras.ReadPose(row.Viewport)).Bracket()
                    .Map(pose => new Plane(state.Focus, pose.Frame.XAxis, pose.Frame.YAxis)),
            frame: static (state, frame) => IO.lift(() => Missing.Unless(state.Document.Objects.FindId(frame.Object), nameof(ObjectTable.FindId)))
                .Bind(static found => IO.lift(() => Conversions.Present(found.ObjectFrame()).ToFin(new InvalidAnswer(nameof(RhinoObject.ObjectFrame)))))
                .Map(axes => new Plane(state.Focus, axes.XAxis, axes.ZAxis)));

    // --- [PLACEMENT]
    public static Fin<Seq<PlacedLight>> Placed(LightRig rig, Seq<RigLight> lights, RigTarget.Fixed target, Plane basis, LengthUnit modelUnit) =>
        from placing in Fin.Succ(Transform.Rotation(rig.Rotation, basis.YAxis, basis.Origin)
            * Transform.Rotation(-(double)rig.Elevation, basis.XAxis, basis.Origin)
            * Transform.PlaneToPlane(Plane.WorldXY, basis))
        from placed in lights.Traverse(light => from reach in Conversions.Validated<TargetRadius, double, InvalidRhinoValue>(light.Distance * target.Radius).ToValidation()
                                                let axes = light.Axes
                                                let radius = reach * Math.Tan(light.Size / 2d)
                                                let side = 2d * radius
                                                let at = placing * new Point3d(reach * axes.Toward)
                                                let metres = Quantities.From(reach, modelUnit).Meters.ToDouble()
                                                let falloff = light.Falloff switch {
                                                    Light.Attenuation.Constant => (Gain: 1d, Vector: Light.ConstantAttenuationVector),
                                                    Light.Attenuation.Linear => (Gain: metres, Vector: Light.LinearAttenuationVector),
                                                    Light.Attenuation.InverseSquared => (Gain: metres * metres, Vector: Light.InverseSquaredAttenuationVector),
                                                }
                                                let shaped = light.Emitter.Switch(
                                                    (At: at, Aim: target.Focus - at, Radius: radius, Side: side, Size: (double)light.Size, falloff.Gain, Across: side * (placing * axes.Across), Rise: side * (placing * axes.Rise)),
                                                    pointLight: static (state, _) => (
                                                        Shape: Conversions.Validated<EmitterRadius, double, InvalidRhinoValue>(state.Radius).ToValidation()
                                                            .Map(emitter => (LightShape)new LightShape.PointLight(state.At, emitter, CameraRelative: false)),
                                                         state.Gain),
                                                    spotLight: static (state, spot) => (
                                                        Shape: Conversions.Validated<EmitterRadius, double, InvalidRhinoValue>(state.Radius).ToValidation()
                                                            .Map(emitter => (LightShape)new LightShape.SpotLight(state.At, state.Aim, spot.Angle, spot.HotSpot, emitter, CameraRelative: false)),
                                                         state.Gain),
                                                    directionalLight: static (state, _) => (
                                                        Shape: Conversions.Validated<SunAngle, double, InvalidRhinoValue>(state.Size).ToValidation()
                                                            .Map(angle => (LightShape)new LightShape.DirectionalLight(state.At, state.Aim, angle, CameraRelative: false)),
                                                        Gain: 1d),
                                                    rectangularLight: static (state, _) => (
                                                        Shape: double.IsFinite(state.Side)
                                                            ? Validation.Success<Error, LightShape>(new LightShape.RectangularLight(state.At - (state.Across / 2d) - (state.Rise / 2d), state.Across, state.Rise, state.Aim))
                                                            : Validation.Fail<Error, LightShape>(new InvalidRhinoValue()),
                                                         state.Gain))
                                                from result in fun((LightShape shape, LightIntensity intensity) => new PlacedLight(
                                                        light.Role, shape, light.Color.Switch(blackbody: static blackbody => (LightColor)blackbody.Kelvin, linear: static linear => (LightColor)linear.Filter), intensity, falloff.Vector))
                                                    .Map(shaped.Shape)
                                                    .Apply(Conversions.Validated<LightIntensity, double, InvalidRhinoValue>(rig.Level == LightIntensity.MinValue ? 0d : rig.Level * double.Exp2(light.Stops) * shaped.Gain).ToValidation())
                                                    .As()
                                                select result).As().ToFin()
        select placed;

    // --- [RECONCILE]
    public static IO<Committed<Seq<RigChange>>> Reconcile(RhinoDoc doc, LightRig rig, Option<LightingReading> reading, RigCommit commit, IPlugInSink sink) =>
        Commits.Commit(doc, RowText.Localize(commit.Name, table: Some<object>(sink)), commit.Redraw,
            from lights in IO.lift(rig.Layout.Lights(reading))
            from target in Target(doc, rig.Target)
            from basis in Basis(doc, rig.Basis, target.Focus)
            from placed in IO.lift(() => Placed(rig, lights, target, basis, doc.ModelUnits))
            from held in Held(doc, rig.Id)
            let changes = Plan(rig, held, placed)
            from applied in Apply(doc, changes, role => RowText.Localize(commit.Caption(role), table: Some<object>(sink)))
            select changes);

    public static IO<Seq<HeldLight>> Held(RhinoDoc doc, Guid rig) =>
        IO.lift(() => Conversions.Rows(doc.Lights).Filter(static light => !light.IsDeleted)
            .Map(static row => new HeldLight(row.Id, row.RuntimeSerialNumber, UserStrings.Held(row.Attributes.GetUserStrings())))
            .Filter(light => light.Strings.Find(LightKey.Rig.Key) == Some(rig.ToString("D"))).Strict());

    public static Seq<RigChange> Plan(LightRig rig, Seq<HeldLight> held, Seq<PlacedLight> placed) =>
        toSeq(held.OrderBy(static light => light.Serial)).Fold(
            (Members: Map<int, HeldLight>(), Released: Seq<HeldLight>()),
            static (state, light) => light.Slot.Filter(slot => !state.Members.ContainsKey(slot)).Match(
                Some: slot => (state.Members.Add(slot, light), state.Released),
                None: () => (state.Members, state.Released.Add(light)))) switch {
                    var (members, released) =>
                        (from row in placed.Map((placement, slot) => (Placement: placement, Label: new RigLabel(rig.Id, slot, placement.Role, rig.Preset)))
                         let member = members.Find(row.Label.Slot)
                         let labelled = member.Exists(light => RigLabel.Strings(Some(row.Label)).ForAll((key, value) => light.Strings.Find(key) == value))
                         select member.Match(
                             Some: light => (RigChange)new RigChange.Place(light.Id, row.Placement, Callbacks.Found(!labelled, row.Label)),
                             None: () => new RigChange.Create(row.Label, row.Placement, members.Min.Map(static lowest => lowest.Value.Id))))
                        + toSeq(members.Filter((slot, _) => slot >= placed.Count).Values).Map(static member => (RigChange)new RigChange.Remove(member.Id))
                        + released.Map(static light => (RigChange)new RigChange.Release(light.Id)),
                };

    private static IO<Unit> Apply(RhinoDoc doc, Seq<RigChange> changes, Func<LightRole, string> caption) =>
        from created in changes.TraverseM(change => change.Switch(
                (Document: doc, Caption: caption),
                create: static (state, create) => Created(state.Document, create, state.Caption),
                place: static (state, place) => Moved(state.Document, place, state.Caption),
                release: static (state, release) => Labelled(state.Document, release.Light, None).Map(static _ => Seq<Guid>()),
                remove: static (state, remove) =>
                    Lights.Apply(state.Document, new LightOp.Delete(new ComponentRef<LightObject>.ById(remove.Light), Quiet: true)).Map(static _ => Seq<Guid>()))).As()
        let fresh = created.Flatten()
        from grouped in unless(fresh.IsEmpty, Lights.Apply(doc, new LightOp.Grouping(
            fresh.Map<ComponentRef<LightObject>>(static id => new ComponentRef<LightObject>.ById(id)), On: true)).Map(static _ => unit)).As()
        select unit;

    private static IO<Seq<Guid>> Created(RhinoDoc doc, RigChange.Create create, Func<LightRole, string> caption) =>
        from source in create.Source.Traverse(member => IO.lift(() => Missing.Unless(doc.Objects.FindId(member), nameof(ObjectTable.FindId)))).As()
        let joined = source.ToSeq().Bind(static found => Seq(
            AttributeEdits.LayerIndex(new LayerRef.Row(new ComponentRef<Layer>.ByIndex(found.Attributes.LayerIndex))),
            AttributeEdits.Groups(new RowsEdit<ComponentRef<Group>, ComponentRef<Group>>.Add(
                Conversions.Rows(found.Attributes.GetGroupList()).Map<ComponentRef<Group>>(static index => new ComponentRef<Group>.ByIndex(index))))))
        from added in TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes => from edited in AttributeOps.Apply(doc, attributes, joined.Add(AttributeEdits.SetUserStrings(RigLabel.Strings(Some(create.Label)))))
                                                                                               from light in Lights.Apply(doc, new LightOp.Add(
                                                                                                   create.Placed.Spec(caption(create.Placed.Role), enabled: true, LightPower.Default, ShadowIntensity.Default), Some(attributes)))
                                                                                               select light)
        select source.IsNone ? added : Seq<Guid>();

    private static IO<Seq<Guid>> Moved(RhinoDoc doc, RigChange.Place place, Func<LightRole, string> caption) =>
        new ComponentRef<LightObject>.ById(place.Light) switch {
            var address =>
                from held in Lights.Read(doc, address)
                from moved in Lights.Apply(doc, new LightOp.Modify(
                    address, place.Placed.Spec(caption(place.Placed.Role), held.Spec.Enabled, held.Spec.Watts, held.Spec.ShadowIntensity)))
                from labelled in place.Label.Traverse(label => Labelled(doc, place.Light, Some(label))).As()
                select Seq<Guid>(),
        };

    private static IO<Seq<Guid>> Labelled(RhinoDoc doc, Guid light, Option<RigLabel> label) =>
        TableOps.Apply(doc, AttributeOps.Modify(doc, new ObjectTarget.Ids(Seq(light)), Seq(AttributeEdits.SetUserStrings(RigLabel.Strings(label))), quiet: true));

    // --- [FOLLOW]
    public static IO<Option<IDisposable>> Follow(RhinoDoc doc, LightRig rig, Option<LightingReading> reading, RigCommit commit, IPlugInSink sink) =>
        rig.Target is RigTarget.Subjects subjects
            ? from initial in Target(doc, rig.Target)
              from acquisition in IO.lift(() => {
                  Atom<(RigTarget.Fixed Current, bool Changed)> previous = Atom((Current: initial, Changed: false));
                  LanguageExt.HashSet<Guid> ids = toHashSet(subjects.Ids);
                  uint serial = doc.RuntimeSerialNumber;
                  CallbackSite site = new(sink, typeof(LightRigs), nameof(Follow));
                  Seq<HostEvent<(Unit, Unit)>> events = Seq(
                      EventKind.ReplaceRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
                      EventKind.DeleteRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
                      EventKind.UndeleteRhinoObject.In(serial).Choose(args => Callbacks.Found(ids.Contains(args.ObjectId), (unit, unit))),
                      EventKind.TransformObjects.In(serial).Choose(args => Callbacks.Found(args.ObjectIds.Exists(ids.Contains) || args.GripOwnerIds.Exists(ids.Contains), (unit, unit))));
                  IO<Unit> Resync(HashMap<Unit, Unit> _) =>
                      from target in Target(doc, rig.Target)
                      from moved in IO.lift(() => previous.Swap(held => (target, held.Current != target)).Changed)
                      from changed in when(moved, Reconcile(doc, rig with { Target = target }, reading, commit, sink).Map(static _ => unit)).As()
                      select unit;
                  return
                      from mailbox in Subscriptions.Idle<Unit, Unit>(Resync)(site)
                      from watched in DisposalOps.OnFailure(
                          DisposalOps.AcquireAll(events.Map(row => row.Inline(mailbox.Post, sink)), DisposalOps.Release), IO.lift(mailbox.Release.Dispose))
                      select DisposalOps.Composite(Seq(mailbox.Release) + watched, site);
              })
              from followed in acquisition
              select Some(followed)
            : IO.pure(Option<IDisposable>.None);
}
