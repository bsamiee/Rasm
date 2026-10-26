using System.Drawing;
using System.Numerics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using PeterO.Numbers;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp.PixelFormats;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TwirlAngle : IMinMaxValue<TwirlAngle> {
    public static TwirlAngle MinValue { get; } = new(-2f * float.Tau);
    public static TwirlAngle MaxValue { get; } = new(2f * float.Tau);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FalloffPower : IMinMaxValue<FalloffPower> {
    public static FalloffPower MinValue { get; } = new(0.1f);
    public static FalloffPower MaxValue { get; } = new(8f);
    public static FalloffPower Linear { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Uniform", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ArmCount : IMinMaxValue<ArmCount> {
    public static ArmCount MinValue => Uniform;
    public static ArmCount MaxValue { get; } = new(16);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct WaveCount : IMinMaxValue<WaveCount> {
    public static WaveCount MinValue { get; } = new(1);
    public static WaveCount MaxValue { get; } = new(64);
    public static WaveCount Standard { get; } = new(5);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DropSize : IMinMaxValue<DropSize> {
    public static DropSize MinValue { get; } = new(0.01f);
    public static DropSize MaxValue { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BulgeMode : ICaptioned {
    public static readonly BulgeMode Round = new("round", Vector2.One, "Normal");
    public static readonly BulgeMode Horizontal = new("horizontal", Vector2.UnitX, "Horizontal only");
    public static readonly BulgeMode Vertical = new("vertical", Vector2.UnitY, "Vertical only");

    public Vector2 Axes { get; }
    public string Caption { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveFront : ICaptioned {
    public static readonly WaveFront Linear = new("linear", None, "Straight");
    public static readonly WaveFront Radial = new("radial", Some(Vector2.UnitX), "Out from center");
    public static readonly WaveFront Zigzag = new("zigzag", Some(Vector2.UnitY), "Around center");
    public static readonly WaveFront Pond = new("pond", Some(new Vector2(float.Sqrt(0.5f))), "Pond ripples");

    public Option<Vector2> Ring { get; }
    public string Caption { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapReading : ICaptioned {
    public static readonly MapReading Gradient = new("gradient", Some<Func<Mat, Mat, double, PassContext, Fin<Unit>>>(static (map, plane, sigma, context) =>
        Height(map, sigma, context, height => Displacements.Slope(height, plane, -double.Sqrt(double.Tau) * sigma / 8d))), "Luma gradient");
    public static readonly MapReading Luma = new("luma", Some<Func<Mat, Mat, double, PassContext, Fin<Unit>>>(static (map, plane, sigma, context) =>
        Height(map, sigma, context, height => {
            Displacements.Transform(height, plane, 2, [2d, -1d, 2d, -1d]);
            return unit;
        })), "Luma");
    public static readonly MapReading Offsets = new("offsets", Some<Func<Mat, Mat, double, PassContext, Fin<Unit>>>(static (map, plane, _, _) => {
        Displacements.Transform(map, plane, 2, [2d, 0d, 0d, 0d, -1d, 0d, -2d, 0d, 0d, 1d]);
        return unit;
    }), "Red and green");
    public static readonly MapReading Coordinates = new("coordinates", None, "ST map");

    internal Option<Func<Mat, Mat, double, PassContext, Fin<Unit>>> Plane { get; }
    public string Caption { get; }

    private static unsafe Fin<Unit> Height(Mat map, double sigma, PassContext context, Func<Mat, Fin<Unit>> read) {
        ((double X, double Y, double Z) luminance, double logGray) = (context.Working.Luminance, double.Log(Exposure.MiddleGray) - (context.Exposure * double.Log(2d)));
        using Mat linear = new();
        using Mat height = new();
        using Mat plane = new();
        map.ConvertTo(linear, DepthType.Cv64F);
        Displacements.Transform(linear, height, 1, [luminance.X, luminance.Y, luminance.Z, 0d]);
        foreach (ref double luma in new Span<double>((void*)height.DataPointer, height.Rows * height.Cols))
            luma = double.Log(double.Max(luma, 0d)) switch {
                var logLuma => double.Exp(logLuma - double.Max(logLuma, logGray) - double.LogP1(double.Exp(-double.Abs(logLuma - logGray)))),
            };
        height.ConvertTo(plane, DepthType.Cv32F);
        return plane.Blur(plane, 3d * sigma).Bind(_ => read(plane));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefractionField : ICaptioned {
    public static readonly RefractionField Raindrops = new("raindrops", static (state, held, field) => {
        (Matrix3x2 fall, Matrix3x2 rise) = (Matrix3x2.CreateRotation(state.Direction + (float.Pi / 2f)), Matrix3x2.CreateRotation(-state.Direction - (float.Pi / 2f)));
        return Some<Func<Vector2, Vector4>>(q => {
            Vector2 at = (Vector2.Transform(q, fall) - new Vector2(0f, state.Fall * held)) / state.Scale;
            Vector2 cell = Vector2.Round(at, MidpointRounding.ToNegativeInfinity);
            Vector4 draw = NoiseFunctions.White(new Vector4(cell, 0f, 0f), field);
            Vector2 off = at - cell - new Vector2(0.25f) - (0.5f * new Vector2(draw.Y, draw.Z));
            (float radius, float lag) = (0.5f * state.Size * (1f - (state.Variation * draw.W)), off.Y <= 0f ? state.Trails * (1f + (off.Y / (0.25f + (0.5f * draw.Z)))) : 0f);
            float drop = (float)RampInterpolation.Ease.Weight(Easing.Saturate((radius - off.Length()) / (radius * state.Softness)));
            float trail = (float)RampInterpolation.Ease.Weight(Easing.Saturate(((0.5f * radius) - float.Abs(off.X)) / (0.5f * radius * state.Softness)));
            return draw.X < state.Density
                ? new Vector4(drop > 0f ? -(1f + state.Lens) * state.Scale * Vector2.Transform(off, rise) : Vector2.Zero,
                    lag * (1f - drop) * float.Sqrt(float.Max(0f, 1f - float.Pow(off.X / (0.5f * radius), 2f))), float.Max(drop, lag * trail))
                : Vector4.Zero;
        }).Filter(_ => state.Density != AxisFraction.MinValue);
    });
    public static readonly RefractionField Blocks = new("blocks", static (state, _, _) => Glass(state, static cell =>
        Vector2.Abs((2f * Fraction(cell)) - Vector2.One) switch { var edge => (Vector2.One - (edge * edge * edge * edge)) switch { var bevel => bevel.X * bevel.Y } }));
    public static readonly RefractionField Canvas = new("canvas", static (state, _, _) => Glass(state, static cell => 0.5f + (0.5f * float.SinPi(2f * cell.X) * float.SinPi(2f * cell.Y))));
    public static readonly RefractionField Frosted = new("frosted", static (state, _, field) =>
        Glass(state, cell => 0.5f + (0.5f * NoiseDimensions.Two.Fbm(new Vector4(cell, 0f, 0f), Octaves.Default, CoordinateHash.Branch(field, 2u)))));
    public static readonly RefractionField TinyLens = new("tiny-lens", static (state, _, _) =>
        Glass(state, static cell => float.Sqrt(float.Max(0f, 1f - (4f * (Fraction(cell) - new Vector2(0.5f)).LengthSquared())))));

    [UseDelegateFromConstructor]
    internal partial Option<Func<Vector2, Vector4>> Sampler(Refraction state, float held, uint field);

    private static Option<Func<Vector2, Vector4>> Glass(Refraction state, Func<Vector2, float> height) =>
        (state.Strength, state.Blur) == (ShortSideOffset.Neutral, ShortSideLength.Neutral) ? None : Some<Func<Vector2, Vector4>>(q => new Vector4(Vector2.Zero, height(q / state.Scale), 1f));

    private static Vector2 Fraction(Vector2 cell) => cell - Vector2.Round(cell, MidpointRounding.ToNegativeInfinity);
}

public sealed record Twirl(TwirlAngle Angle, ShortSideLength Radius, FalloffPower Falloff, ArmCount Arms, SignedAngle ArmOffset, FramePosition Center, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Twirl, TwirlParameter>, IPixelStage<Twirl> {
    public static Twirl Default { get; } = new(TwirlAngle.Off, Displacements.Inscribed, FalloffPower.Linear, ArmCount.Uniform, SignedAngle.Neutral, FramePosition.Center, Sampling.Linear, WrapMode.Periodic);

    public static Option<PixelPass> Pass(Twirl state, PassContext context) =>
        (state.Radius.Pixels(context.Extent), 1f / state.Falloff) switch {
            var (radius, exponent) => Displacements.Pass(state.Angle == TwirlAngle.Off || state.Radius == ShortSideLength.Neutral, state.Sampling, state.Wrap, state.Center.Point(context.Extent), d =>
                double.SinCos(state.Angle * RampInterpolation.Ease.Weight(double.Clamp(1d - double.Pow(double.Hypot(d.X, d.Y) / radius, exponent), 0d, 1d))
                    * (0.5d + (0.5d * double.Cos(state.Arms * (double.Atan2(-d.Y, d.X) - state.ArmOffset))))) switch {
                        var (sin, cos) => ((d.X * cos) - (d.Y * sin), (d.X * sin) + (d.Y * cos)),
                    }),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TwirlParameter : IStateParameter<Twirl> {
    public static readonly TwirlParameter Angle = new("angle", StateParameter.Bounded(Lens<Twirl, TwirlAngle>.New(static state => state.Angle, static angle => state => state with { Angle = angle }), new Presentation<float> { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Soft = (-float.Tau, float.Tau), Origin = (float)TwirlAngle.Off }),
        new("Angle", "Counterclockwise turn at center, none at radius"));
    public static readonly TwirlParameter Radius = new("radius", StateParameter.Bounded(Lens<Twirl, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius })),
        new("Radius", "Twirl reach in frame short sides"));
    public static readonly TwirlParameter Falloff = new("falloff", StateParameter.Bounded(Lens<Twirl, FalloffPower>.New(static state => state.Falloff, static falloff => state => state with { Falloff = falloff }), new Presentation<float> { Soft = (0.25f, 4f), Scale = TrackScale.Log }),
        new("Falloff", "Turn falloff exponent, 1 linear"));
    public static readonly TwirlParameter Arms = new("arms", StateParameter.Bounded(Lens<Twirl, ArmCount>.New(static state => state.Arms, static arms => state => state with { Arms = arms }), new Presentation<int> { Soft = (0, 8) }),
        new("Arms", "Spiral arms the turn gathers into, 0 turning every direction alike"));
    public static readonly TwirlParameter ArmOffset = new("arm-offset", StateParameter.Bounded(Lens<Twirl, SignedAngle>.New(static state => state.ArmOffset, static offset => state => state with { ArmOffset = offset })),
        new("Arm offset", "Arm turn about center"));
    public static readonly TwirlParameter Center = new("center", new StateParameter<Twirl>.Record<FramePosition>(Lens<Twirl, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center })),
        new("Center", "Point the warp centers on"));
    public static readonly TwirlParameter Sampling = new("sampling", new StateParameter<Twirl>.Choice<Sampling>(Lens<Twirl, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly TwirlParameter Wrap = new("wrap", new StateParameter<Twirl>.Choice<WrapMode>(Lens<Twirl, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<Twirl> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Bulge(SignedFraction Amount, BulgeMode Mode, ShortSideLength Radius, FramePosition Center, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Bulge, BulgeParameter>, IPixelStage<Bulge> {
    public static Bulge Default { get; } = new(SignedFraction.Neutral, BulgeMode.Round, Displacements.Inscribed, FramePosition.Center, Sampling.Linear, WrapMode.Clamp);

    public static Option<PixelPass> Pass(Bulge state, PassContext context) =>
        Displacements.Pass(state.Amount == SignedFraction.Neutral || state.Radius == ShortSideLength.Neutral, state.Sampling, state.Wrap, state.Center.Point(context.Extent), d =>
            (X: d.X * state.Mode.Axes.X, Y: d.Y * state.Mode.Axes.Y) switch {
                var v when double.Hypot(v.X, v.Y) / state.Radius.Pixels(context.Extent) is var u and > 0d and < 1d =>
                    ((((float)state.Amount >= 0f ? double.Lerp(u, 2d / double.Pi * double.Asin(u), (float)state.Amount) : double.Lerp(u, double.SinPi(u / 2d), -(float)state.Amount)) / u) - 1d) switch {
                        var scale => (d.X + (scale * v.X), d.Y + (scale * v.Y)),
                    },
                _ => d,
            });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BulgeParameter : IStateParameter<Bulge> {
    public static readonly BulgeParameter Amount = new("amount", StateParameter.Bounded(Lens<Bulge, SignedFraction>.New(static state => state.Amount, static amount => state => state with { Amount = amount })),
        new("Amount", "Bulge above 0 and pinch below, ±1 a full hemisphere"));
    public static readonly BulgeParameter Mode = new("mode", new StateParameter<Bulge>.Choice<BulgeMode>(Lens<Bulge, BulgeMode>.New(static state => state.Mode, static mode => state => state with { Mode = mode })),
        new("Mode", "Axes the bulge acts along"));
    public static readonly BulgeParameter Radius = new("radius", StateParameter.Bounded(Lens<Bulge, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius })),
        new("Radius", "Bulged disc radius in frame short sides"));
    public static readonly BulgeParameter Center = new("center", new StateParameter<Bulge>.Record<FramePosition>(Lens<Bulge, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center })),
        new("Center", "Point the warp centers on"));
    public static readonly BulgeParameter Sampling = new("sampling", new StateParameter<Bulge>.Choice<Sampling>(Lens<Bulge, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly BulgeParameter Wrap = new("wrap", new StateParameter<Bulge>.Choice<WrapMode>(Lens<Bulge, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<Bulge> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Wave(
    WaveFront Front, WaveProfile Profile, WaveCount Count, ShortSideExtent MinWavelength, ShortSideExtent MaxWavelength, AxisFraction MinAmplitude, AxisFraction MaxAmplitude,
    ShortSideOffset Horizontal, ShortSideOffset Vertical, SignedAngle Direction, SignedAngle Spread, SignedAngle Phase, ShortSideLength Radius, FramePosition Center, Frequency Rate,
    Seed Seed, Timing Timing, Hold Hold, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Wave, WaveParameter>, IPixelStage<Wave> {
    public static Wave Default { get; } = new(
        WaveFront.Linear, WaveProfile.Sine, WaveCount.Standard, ShortSideExtent.Create(10f / ReferenceFrame.Height), ShortSideExtent.Create(120f / ReferenceFrame.Height), AxisFraction.Create(5f / 35f),
        AxisFraction.MaxValue, ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, SignedAngle.Neutral, SignedAngle.Neutral, Displacements.Inscribed, FramePosition.Center,
        Frequency.Neutral, Seed.MinValue, Timing.Default, Hold.MinValue, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(Wave state, PassContext context) {
        (PixelExtent extent, int count, uint field, Matrix3x2 across) = (context.Extent, state.Count, CoordinateHash.Field(NoiseStream.Wave, state.Seed, 0u), Matrix3x2.CreateRotation(-float.Pi / 2f));
        ((double X, double Y) amounts, double radius, float shift) =
            ((state.Horizontal.Pixels(extent), state.Vertical.Pixels(extent)), state.Radius.Pixels(extent), (state.Phase / float.Tau) - (state.Rate * state.Hold.Held(state.Timing.At(context))));
        (bool ring, Matrix3x2 turn) = state.Front.Ring.Match(Some: weights => (true, (Matrix3x2.Identity * weights.X) + (across * weights.Y)), None: () => (false, across));
        (Vector2 Along, double Length, float Amplitude, float Phase)[] waves = [.. Enumerable.Range(0, count).Select(i =>
            (NoiseFunctions.White(Vector4.Zero, CoordinateHash.Branch(field, (uint)i)), state.Direction + (state.Spread * (i - ((count - 1) / 2f)) / int.Max(count - 1, 1))) switch {
                var (draw, angle) => (Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(-angle)), double.Lerp(state.MinWavelength.Pixels(extent), state.MaxWavelength.Pixels(extent), draw.X),
                    float.Lerp(state.MinAmplitude, state.MaxAmplitude, draw.Y), draw.Z + shift),
            })];
        return Displacements.Pass(amounts == (0d, 0d) || (state.MinAmplitude, state.MaxAmplitude) == (AxisFraction.MinValue, AxisFraction.MinValue) || (ring && state.Radius == ShortSideLength.Neutral),
            state.Sampling, state.Wrap, state.Center.Point(extent), d => {
                double reach = double.Hypot(d.X, d.Y);
                ((double X, double Y) spoke, (double X, double Y) sum) = (reach > 0d ? (d.X / reach, d.Y / reach) : (1d, 0d), (0d, 0d));
                foreach ((Vector2 along, double length, float amplitude, float phase) in waves) {
                    (double x, double y) = Crest(d, ring ? spoke : (along.X, along.Y), length, amplitude, phase);
                    sum = (sum.X + x, sum.Y + y);
                }
                double falloff = (ring ? double.Max(0d, 1d - (reach / radius)) : 1d) / count;
                return (d.X + (amounts.X * sum.X * falloff), d.Y + (amounts.Y * sum.Y * falloff));
            });

        (double X, double Y) Crest((double X, double Y) d, (double X, double Y) axis, double length, float amplitude, float phase) =>
            (amplitude * ((2f * state.Profile.Level((float)((((d.X * axis.X) + (d.Y * axis.Y)) / length) + phase))) - 1f)) switch {
                var level => (level * ((axis.X * turn.M11) + (axis.Y * turn.M21)), level * ((axis.X * turn.M12) + (axis.Y * turn.M22))),
            };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveParameter : IStateParameter<Wave> {
    private static readonly Presentation<float> Wavelength = ShortSideExtent.Presentation with { Soft = (0.005f, 1f) };

    public static readonly WaveParameter Front = new("front", new StateParameter<Wave>.Choice<WaveFront>(Lens<Wave, WaveFront>.New(static state => state.Front, static front => state => state with { Front = front })),
        new("Form", "Straight waves or center rings moving outward, around, or both"));
    public static readonly WaveParameter Profile = new("profile", new StateParameter<Wave>.Choice<WaveProfile>(Lens<Wave, WaveProfile>.New(static state => state.Profile, static profile => state => state with { Profile = profile })),
        new("Profile", "Shape of each wave period"));
    public static readonly WaveParameter Count = new("count", StateParameter.Bounded(Lens<Wave, WaveCount>.New(static state => state.Count, static count => state => state with { Count = count }), new Presentation<int> { Soft = (1, 16) }),
        new("Generators", "Waves summed into the field"));
    public static readonly WaveParameter MinWavelength = new("min-wavelength", StateParameter.Bounded(Lens<Wave, ShortSideExtent>.New(static state => state.MinWavelength, static wavelength => state => state with { MinWavelength = wavelength }), Wavelength),
        new("Wavelength min", "Shortest wave in frame short sides"));
    public static readonly WaveParameter MaxWavelength = new("max-wavelength", StateParameter.Bounded(Lens<Wave, ShortSideExtent>.New(static state => state.MaxWavelength, static wavelength => state => state with { MaxWavelength = wavelength }), Wavelength),
        new("Wavelength max", "Longest wave in frame short sides"));
    public static readonly WaveParameter MinAmplitude = new("min-amplitude", StateParameter.Bounded(Lens<Wave, AxisFraction>.New(static state => state.MinAmplitude, static amplitude => state => state with { MinAmplitude = amplitude })),
        new("Amplitude min", "Weakest wave as share of amounts"));
    public static readonly WaveParameter MaxAmplitude = new("max-amplitude", StateParameter.Bounded(Lens<Wave, AxisFraction>.New(static state => state.MaxAmplitude, static amplitude => state => state with { MaxAmplitude = amplitude })),
        new("Amplitude max", "Strongest wave as share of amounts"));
    public static readonly WaveParameter Horizontal = new("horizontal", StateParameter.Bounded(Lens<Wave, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Displacements.Shift),
        new("Amount X", "Horizontal move at full strength in frame short sides"));
    public static readonly WaveParameter Vertical = new("vertical", StateParameter.Bounded(Lens<Wave, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Displacements.Shift),
        new("Amount Y", "Vertical move at full strength in frame short sides"));
    public static readonly WaveParameter Direction = new("direction", StateParameter.Bounded(Lens<Wave, SignedAngle>.New(static state => state.Direction, static direction => state => state with { Direction = direction })),
        new("Direction", "Straight wave angle counterclockwise from right"));
    public static readonly WaveParameter Spread = new("spread", StateParameter.Bounded(Lens<Wave, SignedAngle>.New(static state => state.Spread, static spread => state => state with { Spread = spread }), SignedAngle.Presentation with { Soft = (0f, float.Pi) }),
        new("Spread", "Wave direction fan about travel direction"));
    public static readonly WaveParameter Phase = new("phase", StateParameter.Bounded(Lens<Wave, SignedAngle>.New(static state => state.Phase, static phase => state => state with { Phase = phase })),
        new("Phase", "Turn added to every wave period"));
    public static readonly WaveParameter Radius = new("radius", StateParameter.Bounded(Lens<Wave, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius })),
        new("Radius", "Ring reach in frame short sides, fading to none"));
    public static readonly WaveParameter Center = new("center", new StateParameter<Wave>.Record<FramePosition>(Lens<Wave, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center })),
        new("Center", "Point the warp centers on"));
    public static readonly WaveParameter Rate = new("rate", StateParameter.Bounded(Lens<Wave, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (-2f, 2f) }),
        new("Rate", "Wave periods traveled per second"));
    public static readonly WaveParameter Seed = new("seed", StateParameter.Bounded(Lens<Wave, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly WaveParameter Timing = new("timing", new StateParameter<Wave>.Record<Timing>(Lens<Wave, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly WaveParameter Hold = new("hold", StateParameter.Bounded(Lens<Wave, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly WaveParameter Sampling = new("sampling", new StateParameter<Wave>.Choice<Sampling>(Lens<Wave, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly WaveParameter Wrap = new("wrap", new StateParameter<Wave>.Choice<WrapMode>(Lens<Wave, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<Wave> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Turbulence(
    NoiseBasis Basis, ShortSideExtent ScaleX, ShortSideExtent ScaleY, ShortSideOffset Horizontal, ShortSideOffset Vertical, Frequency Rate, Seed Seed, Timing Timing, Hold Hold,
    Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Turbulence, TurbulenceParameter>, IPixelStage<Turbulence> {
    public static Turbulence Default { get; } = new(
        NoiseBasis.Default, Displacements.Uniform, Displacements.Uniform, ShortSideOffset.Neutral, ShortSideOffset.Neutral, Frequency.Neutral, Seed.MinValue, Timing.Default, Hold.MinValue,
        Sampling.Linear, WrapMode.Clamp);

    public static Option<PixelPass> Pass(Turbulence state, PassContext context) {
        PixelExtent extent = context.Extent;
        Func<LatticePoint, uint, NoiseSample<LatticePoint>> sampler = state.Basis.Sampler;
        uint field = CoordinateHash.Field(NoiseStream.Turbulence, state.Seed, 0u);
        ERational side = ERational.FromInt32(extent.ShortSide);
        ((ERational X, ERational Y) scale, (double X, double Y) amounts, ERational depth) =
            ((ERational.FromSingle(state.ScaleX) * side, -ERational.FromSingle(state.ScaleY) * side), (state.Horizontal.Pixels(extent), state.Vertical.Pixels(extent)),
                ERational.FromSingle(state.Rate) * ERational.FromSingle(state.Hold.Held(state.Timing.At(context))));
        (uint right, uint left, uint down, uint up) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u));
        return Displacements.Pass(amounts == (0d, 0d), state.Sampling, state.Wrap, FramePosition.Center.Point(extent), d =>
            new LatticePoint(ERational.FromDouble(d.X) / scale.X, ERational.FromDouble(d.Y) / scale.Y, depth, ERational.Zero) switch {
                var at => (d.X + (amounts.X * (sampler(at, right).Value - sampler(at, left).Value)), d.Y + (amounts.Y * (sampler(at, down).Value - sampler(at, up).Value))),
            });
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TurbulenceParameter : IStateParameter<Turbulence> {
    private static readonly Presentation<float> Feature = ShortSideExtent.Presentation with { Soft = (0.01f, 2f) };

    public static readonly TurbulenceParameter Basis = new("basis", new StateParameter<Turbulence>.Variant<NoiseBasis>(Lens<Turbulence, NoiseBasis>.New(static state => state.Basis, static basis => state => state with { Basis = basis })),
        new("Noise", "Displacement field basis"));
    public static readonly TurbulenceParameter ScaleX = new("scale-x", StateParameter.Bounded(Lens<Turbulence, ShortSideExtent>.New(static state => state.ScaleX, static scale => state => state with { ScaleX = scale }), Feature),
        new("Size X", "Horizontal noise feature size in frame short sides"));
    public static readonly TurbulenceParameter ScaleY = new("scale-y", StateParameter.Bounded(Lens<Turbulence, ShortSideExtent>.New(static state => state.ScaleY, static scale => state => state with { ScaleY = scale }), Feature),
        new("Size Y", "Vertical noise feature size in frame short sides"));
    public static readonly TurbulenceParameter Horizontal = new("horizontal", StateParameter.Bounded(Lens<Turbulence, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Displacements.Shift),
        new("Amount X", "Horizontal move at full strength in frame short sides"));
    public static readonly TurbulenceParameter Vertical = new("vertical", StateParameter.Bounded(Lens<Turbulence, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Displacements.Shift),
        new("Amount Y", "Vertical move at full strength in frame short sides"));
    public static readonly TurbulenceParameter Rate = new("rate", StateParameter.Bounded(Lens<Turbulence, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (-2f, 2f) }),
        new("Evolution", "Noise units the field evolves per second"));
    public static readonly TurbulenceParameter Seed = new("seed", StateParameter.Bounded(Lens<Turbulence, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly TurbulenceParameter Timing = new("timing", new StateParameter<Turbulence>.Record<Timing>(Lens<Turbulence, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly TurbulenceParameter Hold = new("hold", StateParameter.Bounded(Lens<Turbulence, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly TurbulenceParameter Sampling = new("sampling", new StateParameter<Turbulence>.Choice<Sampling>(Lens<Turbulence, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly TurbulenceParameter Wrap = new("wrap", new StateParameter<Turbulence>.Choice<WrapMode>(Lens<Turbulence, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<Turbulence> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record MapDisplacement(
    Option<ImageFile> Source, MapReading Reading, Gated<ShortSideExtent> TileSize, ShortSideExtent Softness, ShortSideOffset Horizontal, ShortSideOffset Vertical, SignedAngle Rotation,
    Sampling Sampling, WrapMode Wrap)
    : IStateRecord<MapDisplacement, MapDisplacementParameter>, IPixelStage<MapDisplacement> {
    public static MapDisplacement Default { get; } = new(
        None, MapReading.Gradient, new(Enabled: false, ShortSideExtent.Create(1f)), ShortSideExtent.Create(0.015f * ReferenceFrame.GreaterSide), ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(MapDisplacement state, PassContext context) =>
        state.Reading.Plane.Match(
            Some: plane => (state.Horizontal, state.Vertical) == (ShortSideOffset.Neutral, ShortSideOffset.Neutral)
                ? None
                : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kernel(plane, context, frame, progress))),
            None: () => state.Source.Map(image => (PixelPass)new PixelPass.Frame(new CoordinateMap.Sampled(image.Frame, state.Sampling, (state.Wrap, state.Wrap)).Apply)));

    private unsafe Fin<Unit> Kernel(Func<Mat, Mat, double, PassContext, Fin<Unit>> plane, PassContext context, PixelFrame frame, IProgress<int> progress) =>
        Source.Match(Some: image => Fitted(image, frame), None: frame).Filter(header => {
            using Mat offsets = new();
            ((double X, double Y) amount, (double Sin, double Cos) turn, Point corner, int width) =
                ((Horizontal.Pixels(context.Extent), Vertical.Pixels(context.Extent)), double.SinCos(-Rotation), frame.Origin, header.Cols);
            return plane(header, offsets, Softness.Pixels(context.Extent), context).Bind(_ => new CoordinateMap.Analytic((points, _, _) => {
                ReadOnlySpan<Vector2> read = new((void*)offsets.DataPointer, offsets.Rows * offsets.Cols);
                foreach (ref (double X, double Y) point in points) {
                    Vector2 offset = read[((int)double.Floor(point.Y - corner.Y) * width) + (int)double.Floor(point.X - corner.X)];
                    (double x, double y) = (offset.X * amount.X, offset.Y * amount.Y);
                    point = (point.X - (x * turn.Cos) + (y * turn.Sin), point.Y - (x * turn.Sin) - (y * turn.Cos));
                }
            }, Sampling, (Wrap, Wrap)).Apply(frame, progress));
        });

    private PixelFrame Fitted(ImageFile image, PixelFrame frame) =>
        TileSize.Active.Match(
            Some: tile => (image.Frame.Extent.Height / tile.Pixels(frame.Extent)) switch { var scale => ((X: scale, Y: scale), WrapMode.Periodic) },
            None: () => ((X: (double)image.Frame.Extent.Width / frame.Extent.Width, Y: (double)image.Frame.Extent.Height / frame.Extent.Height), WrapMode.Clamp)) switch {
                var (scale, wrap) => new CoordinateMap.Analytic(Displacements.Kernel((0d, 0d), point => ((point.X + frame.Origin.X) * scale.X, (point.Y + frame.Origin.Y) * scale.Y)), Sampling.Linear, (wrap, wrap))
                    .Resample(image.Frame, frame.Size),
            };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapDisplacementParameter : IStateParameter<MapDisplacement> {
    public static readonly MapDisplacementParameter Source = new("source", new StateParameter<MapDisplacement>.Loaded<ImageFile, ImagePath, string>(Lens<MapDisplacement, Option<ImageFile>>.New(static state => state.Source, static source => state => state with { Source = source }), ImageFile.Load, static image => image.Path),
        new("Map image", "Image file, frame itself when empty"));
    public static readonly MapDisplacementParameter Reading = new("reading", new StateParameter<MapDisplacement>.Choice<MapReading>(Lens<MapDisplacement, MapReading>.New(static state => state.Reading, static reading => state => state with { Reading = reading })),
        new("Reading", "Map values moving frame"));
    public static readonly MapDisplacementParameter TileSize = new("tile-size", StateParameter.OptionalBounded(Lens<MapDisplacement, Gated<ShortSideExtent>>.New(static state => state.TileSize, static size => state => state with { TileSize = size }), ShortSideExtent.Presentation with { Soft = (0.05f, 2f) }),
        new("Tile size", "Map tile height in frame short sides, off stretching to fit"));
    public static readonly MapDisplacementParameter Softness = new("softness", StateParameter.Bounded(Lens<MapDisplacement, ShortSideExtent>.New(static state => state.Softness, static softness => state => state with { Softness = softness }), ShortSideExtent.Presentation with { Soft = (0.001f, 0.1f), Scale = TrackScale.Linear }),
        new("Softness", "Map luma blur before moving frame"));
    public static readonly MapDisplacementParameter Horizontal = new("horizontal", StateParameter.Bounded(Lens<MapDisplacement, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Displacements.Shift),
        new("Amount X", "Horizontal move at full strength in frame short sides"));
    public static readonly MapDisplacementParameter Vertical = new("vertical", StateParameter.Bounded(Lens<MapDisplacement, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Displacements.Shift),
        new("Amount Y", "Vertical move at full strength in frame short sides"));
    public static readonly MapDisplacementParameter Rotation = new("rotation", StateParameter.Bounded(Lens<MapDisplacement, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation })),
        new("Rotation", "Counterclockwise turn of map moves"));
    public static readonly MapDisplacementParameter Sampling = new("sampling", new StateParameter<MapDisplacement>.Choice<Sampling>(Lens<MapDisplacement, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly MapDisplacementParameter Wrap = new("wrap", new StateParameter<MapDisplacement>.Choice<WrapMode>(Lens<MapDisplacement, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<MapDisplacement> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Refraction(
    RefractionField Field, ShortSideExtent Scale, ShortSideOffset Strength, ShortSideExtent Smoothness, ShortSideLength Blur, AxisFraction Density, DropSize Size, AxisFraction Variation,
    AxisFraction Softness, Steepness Lens, AxisFraction Trails, SignedAngle Direction, Drift Fall, Seed Seed, Timing Timing, Hold Hold, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Refraction, RefractionParameter>, IPixelStage<Refraction> {
    public static Refraction Default { get; } = new(
        RefractionField.Raindrops, ShortSideExtent.Create(Displacements.Uniform / 3.8f), ShortSideOffset.Neutral, ShortSideExtent.Create(3f / ReferenceFrame.Height),
        ShortSideLength.Create(14.6f / ReferenceFrame.Height), AxisFraction.Create(0.5f), DropSize.MaxValue, AxisFraction.Half, AxisFraction.Create(0.05f), Steepness.Create(2f),
        AxisFraction.Create(0.3f), SignedAngle.Down, Drift.Neutral, Seed.MinValue, Timing.Default, Hold.MinValue, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(Refraction state, PassContext context) =>
        state.Field.Sampler(state, state.Hold.Held(state.Timing.At(context)), CoordinateHash.Field(NoiseStream.Refraction, state.Seed, 0u))
            .Map(sample => (PixelPass)new PixelPass.Frame((frame, progress) => state.Kernel(sample, context, frame, progress)));

    private unsafe Fin<Unit> Kernel(Func<Vector2, Vector4> sample, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (PixelExtent extent, (double X, double Y) half) = (context.Extent, FramePosition.Center.Point(context.Extent));
        (PixelFrame field, PixelFrame refracted) = (new(frame.Origin, frame.Size, frame.Extent), frame.Copy());
        return (from painted in new PixelPass.Pointwise((row, column, line) => {
            for (int x = 0; x < row.Length; x++) row[x] = new Vector4(extent.ShortSide, extent.ShortSide, 1f, 1f) * sample(new Vector2(
                (float)(((double)column + x + 0.5d - half.X) / extent.ShortSide), (float)(((double)extent.Height - line - 0.5d - half.Y) / extent.ShortSide)));
        }).Run(field, new Progress<int>())
                from displaced in field.Filter(drawn => {
                    using Mat level = new();
                    using Mat slope = new();
                    using Mat source = refracted.Header();
                    CvInvoke.ExtractChannel(drawn, level, 2);
                    return from prepared in (level.Blur(level, 3d * Smoothness.Pixels(extent)).Map(_ => Displacements.Slope(level, slope, Strength.Pixels(extent) * Scale.Pixels(extent) / 8d)),
                               source.Blur(source, 3d * Blur.Pixels(extent))).Apply(static (_, _) => unit).As()
                           from applied in new CoordinateMap.Analytic((points, coverage, _) => {
                               int pixels = drawn.Rows * drawn.Cols;
                               ReadOnlySpan<Vector2> bend = new((void*)slope.DataPointer, pixels);
                               ReadOnlySpan<Vector4> drops = new((void*)drawn.DataPointer, pixels);
                               for (int i = 0; i < points.Length; i++)
                                   (points[i], coverage[i]) = (((int)double.Floor(points[i].Y - field.Origin.Y) * field.Size.Width) + (int)double.Floor(points[i].X - field.Origin.X)) switch {
                                       var at => ((points[i].X + drops[at].X - bend[at].X, points[i].Y + drops[at].Y - bend[at].Y), coverage[i] * drops[at].W),
                                   };
                           }, Sampling, (Wrap, Wrap)).Apply(refracted, progress)
                           select applied;
                })
                select Blend.Composite(frame, refracted, BlendingMode.Mix, AxisFraction.MaxValue, [], PixelAlphaRepresentation.Unassociated)).Map(composite => {
                    composite.View.Span.CopyTo(frame.View.Span);
                    return unit;
                });
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefractionParameter : IStateParameter<Refraction> {
    public static readonly RefractionParameter Field = new("field", new StateParameter<Refraction>.Choice<RefractionField>(Lens<Refraction, RefractionField>.New(static state => state.Field, static field => state => state with { Field = field })),
        new("Texture", "Surface the frame is seen through"));
    public static readonly RefractionParameter Scale = new("scale", StateParameter.Bounded(Lens<Refraction, ShortSideExtent>.New(static state => state.Scale, static scale => state => state with { Scale = scale }), ShortSideExtent.Presentation with { Soft = (0.005f, 1f) }),
        new("Scale", "Texture cell size in frame short sides"));
    public static readonly RefractionParameter Strength = new("strength", StateParameter.Bounded(Lens<Refraction, ShortSideOffset>.New(static state => state.Strength, static strength => state => state with { Strength = strength }), Displacements.Shift),
        new("Distortion", "Frame bend through surface, below 0 inverting"));
    public static readonly RefractionParameter Smoothness = new("smoothness", StateParameter.Bounded(Lens<Refraction, ShortSideExtent>.New(static state => state.Smoothness, static smoothness => state => state with { Smoothness = smoothness }), ShortSideExtent.Presentation with { Soft = (0.001f, 0.05f) }),
        new("Smoothness", "Surface blur before slope bends frame"));
    public static readonly RefractionParameter Blur = new("blur", StateParameter.Bounded(Lens<Refraction, ShortSideLength>.New(static state => state.Blur, static blur => state => state with { Blur = blur }), ShortSideLength.Presentation with { Soft = (0f, 0.05f) }),
        new("Blur", "Frame softening through surface"));
    public static readonly RefractionParameter Density = new("density", StateParameter.Bounded(Lens<Refraction, AxisFraction>.New(static state => state.Density, static density => state => state with { Density = density })),
        new("Density", "Share of cells holding a raindrop"));
    public static readonly RefractionParameter Size = new("size", StateParameter.Bounded(Lens<Refraction, DropSize>.New(static state => state.Size, static size => state => state with { Size = size }), new Presentation<float>()),
        new("Drop size", "Largest raindrop radius as texture cell share"));
    public static readonly RefractionParameter Variation = new("variation", StateParameter.Bounded(Lens<Refraction, AxisFraction>.New(static state => state.Variation, static variation => state => state with { Variation = variation })),
        new("Variation", "Seeded raindrop radius reduction share"));
    public static readonly RefractionParameter Softness = new("softness", StateParameter.Bounded(Lens<Refraction, AxisFraction>.New(static state => state.Softness, static softness => state => state with { Softness = softness })),
        new("Drop softness", "Raindrop radius share blending rim"));
    public static readonly RefractionParameter Lens = new("lens", StateParameter.Bounded(Lens<Refraction, Steepness>.New(static state => state.Lens, static lens => state => state with { Lens = lens }), new Presentation<float>()),
        new("Lens", "Raindrop magnification"));
    public static readonly RefractionParameter Trails = new("trails", StateParameter.Bounded(Lens<Refraction, AxisFraction>.New(static state => state.Trails, static trails => state => state with { Trails = trails })),
        new("Trails", "Raindrop trail height relative to drop"));
    public static readonly RefractionParameter Direction = new("direction", StateParameter.Bounded(Lens<Refraction, SignedAngle>.New(static state => state.Direction, static direction => state => state with { Direction = direction })),
        new("Direction", "Raindrop run angle counterclockwise from right"));
    public static readonly RefractionParameter Fall = new("fall", StateParameter.Bounded(Lens<Refraction, Drift>.New(static state => state.Fall, static fall => state => state with { Fall = fall }), Drift.Presentation with { Soft = (-1f, 1f) }),
        new("Fall", "Raindrop travel per second in frame short sides"));
    public static readonly RefractionParameter Seed = new("seed", StateParameter.Bounded(Lens<Refraction, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly RefractionParameter Timing = new("timing", new StateParameter<Refraction>.Record<Timing>(Lens<Refraction, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly RefractionParameter Hold = new("hold", StateParameter.Bounded(Lens<Refraction, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly RefractionParameter Sampling = new("sampling", new StateParameter<Refraction>.Choice<Sampling>(Lens<Refraction, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly RefractionParameter Wrap = new("wrap", new StateParameter<Refraction>.Choice<WrapMode>(Lens<Refraction, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<Refraction> Kind { get; }
    public ParameterText Text { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Displacements {
    public static ShortSideLength Inscribed { get; } = ShortSideLength.Create(0.5f);
    public static ShortSideExtent Uniform { get; } = ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f);
    public static Presentation<float> Shift { get; } = ShortSideOffset.Presentation with { Soft = (-0.1f, 0.1f) };

    public static Option<PixelPass> Pass(bool rests, Sampling sampling, WrapMode wrap, (double X, double Y) origin, Func<(double X, double Y), (double X, double Y)> field) =>
        rests ? None : Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(Kernel(origin, field), sampling, (wrap, wrap)).Apply));

    public static Action<Span<(double X, double Y)>, Span<float>, PixelExtent> Kernel((double X, double Y) origin, Func<(double X, double Y), (double X, double Y)> field) =>
        (points, _, _) => {
            foreach (ref (double X, double Y) point in points) point = field((point.X - origin.X, point.Y - origin.Y)) switch { var at => (origin.X + at.X, origin.Y + at.Y) };
        };

    public static Unit Slope(Mat level, Mat plane, double scale) {
        using Mat across = new();
        using Mat down = new();
        CvInvoke.Sobel(level, across, DepthType.Cv32F, 1, 0, 3, scale, 0d, BorderType.Replicate);
        CvInvoke.Sobel(level, down, DepthType.Cv32F, 0, 1, 3, scale, 0d, BorderType.Replicate);
        using VectorOfMat pair = new(across, down);
        CvInvoke.Merge(pair, plane);
        return unit;
    }

    public static void Transform(Mat source, Mat target, int rows, double[] weights) {
        using Mat matrix = new(rows, weights.Length / rows, DepthType.Cv64F, 1);
        matrix.SetTo(weights);
        CvInvoke.Transform(source, target, matrix);
    }
}
