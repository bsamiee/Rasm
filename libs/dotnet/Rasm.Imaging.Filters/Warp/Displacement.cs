using System.Drawing;
using System.Numerics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Stylize;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct TwirlAngle : IMinMaxValue<TwirlAngle> {
    public static TwirlAngle MinValue { get; } = new(-2f * float.Tau);
    public static TwirlAngle MaxValue { get; } = new(2f * float.Tau);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct FalloffPower : IMinMaxValue<FalloffPower> {
    public static FalloffPower MinValue { get; } = new(0.1f);
    public static FalloffPower MaxValue { get; } = new(8f);
    public static FalloffPower Linear { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Uniform", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct ArmCount : IMinMaxValue<ArmCount> {
    public static ArmCount MinValue => Uniform;
    public static ArmCount MaxValue { get; } = new(16);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct WarpAmount : IMinMaxValue<WarpAmount> {
    public static WarpAmount MinValue { get; } = new(-1f);
    public static WarpAmount MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct WaveCount : IMinMaxValue<WaveCount> {
    public static WaveCount MinValue { get; } = new(1);
    public static WaveCount MaxValue { get; } = new(64);
    public static WaveCount Standard { get; } = new(5);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct DropSize : IMinMaxValue<DropSize> {
    public static DropSize MinValue { get; } = new(0.01f);
    public static DropSize MaxValue { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct DropLens : IMinMaxValue<DropLens> {
    public static DropLens MinValue { get; } = new(0f);
    public static DropLens MaxValue { get; } = new(10f);
    public static DropLens Standard { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BulgeMode {
    public static readonly BulgeMode Round = new("round", Vector2.One);
    public static readonly BulgeMode Horizontal = new("horizontal", Vector2.UnitX);
    public static readonly BulgeMode Vertical = new("vertical", Vector2.UnitY);

    public Vector2 Axes { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveFront {
    public static readonly WaveFront Linear = new("linear", None);
    public static readonly WaveFront Radial = new("radial", Some(Vector2.UnitX));
    public static readonly WaveFront Zigzag = new("zigzag", Some(Vector2.UnitY));
    public static readonly WaveFront Pond = new("pond", Some(new Vector2(float.Sqrt(0.5f))));

    public Option<Vector2> Ring { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapReading {
    public static readonly MapReading Gradient = new("gradient", Some<Action<Mat, Mat, float, PassContext>>(static (map, plane, sigma, context) =>
        Height(map, sigma, context, height => Displacements.Slope(height, plane, -double.Sqrt(double.Tau) * sigma / 8d))));
    public static readonly MapReading Luma = new("luma", Some<Action<Mat, Mat, float, PassContext>>(static (map, plane, sigma, context) =>
        Height(map, sigma, context, height => Displacements.Transform(height, plane, 2, [2f, -1f, 2f, -1f]))));
    public static readonly MapReading Offsets = new("offsets", Some<Action<Mat, Mat, float, PassContext>>(static (map, plane, _, _) =>
        Displacements.Transform(map, plane, 2, [2f, 0f, 0f, 0f, -1f, 0f, -2f, 0f, 0f, 1f])));
    public static readonly MapReading Coordinates = new("coordinates", None);

    internal Option<Action<Mat, Mat, float, PassContext>> Plane { get; }

    private static void Height(Mat map, float sigma, PassContext context, Action<Mat> read) {
        (Vector3 luminance, float grey) = (context.Working.Luminance, Exposure.MiddleGrey / context.Exposure.Scale);
        using Mat height = new();
        Displacements.Transform(map, height, 1, [luminance.X, luminance.Y, luminance.Z, 0f]);
        foreach (ref float luma in height.GetSpan<float>()) luma = float.Max(luma, 0f) switch { var lit => lit / (lit + grey) };
        Displacements.Smooth(height, sigma);
        read(height);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefractionField {
    public static readonly RefractionField Raindrops = new("raindrops", static (state, held, field) => {
        (Matrix3x2 fall, Matrix3x2 rise) = (Matrix3x2.CreateRotation(state.Direction + (float.Pi / 2f)), Matrix3x2.CreateRotation(-state.Direction - (float.Pi / 2f)));
        return state.Density == Likelihood.Off ? None : Some<Func<Vector2, Vector4>>(q => {
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
        });
    });
    public static readonly RefractionField Blocks = new("blocks", static (state, _, _) => Glass(state, static cell =>
        Vector2.Abs((2f * Fraction(cell)) - Vector2.One) switch { var edge => (Vector2.One - (edge * edge * edge * edge)) switch { var bevel => bevel.X * bevel.Y } }));
    public static readonly RefractionField Canvas = new("canvas", static (state, _, _) => Glass(state, static cell => 0.5f + (0.5f * float.SinPi(2f * cell.X) * float.SinPi(2f * cell.Y))));
    public static readonly RefractionField Frosted = new("frosted", static (state, _, field) =>
        Glass(state, cell => 0.5f + (0.5f * NoiseDimensions.Two.Fbm(new Vector4(cell, 0f, 0f), Octaves.Default, CoordinateHash.Branch(field, 2u)))));
    public static readonly RefractionField TinyLens = new("tiny-lens", static (state, _, _) =>
        Glass(state, static cell => float.Sqrt(float.Max(0f, 1f - (4f * (Fraction(cell) - new Vector2(0.5f)).LengthSquared())))));

    [UseDelegateFromConstructor]
    internal partial Option<Func<Vector2, Vector4>> Lanes(Refraction state, float held, uint field);

    private static Option<Func<Vector2, Vector4>> Glass(Refraction state, Func<Vector2, float> height) =>
        (state.Strength, state.Blur) == (ShortSideOffset.Neutral, ShortSideLength.Neutral) ? None : Some<Func<Vector2, Vector4>>(q => new Vector4(Vector2.Zero, height(q / state.Scale), 1f));

    private static Vector2 Fraction(Vector2 cell) => cell - Vector2.Round(cell, MidpointRounding.ToNegativeInfinity);
}

public sealed record Twirl(TwirlAngle Angle, ShortSideLength Radius, FalloffPower Falloff, ArmCount Arms, SignedAngle ArmOffset, FramePosition Center, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Twirl, TwirlParameter, InvalidWarp>, IPixelStage<Twirl> {
    public static Twirl Default { get; } = new(TwirlAngle.Off, Displacements.Inscribed, FalloffPower.Linear, ArmCount.Uniform, SignedAngle.Neutral, FramePosition.Center, Sampling.Linear, WrapMode.Periodic);

    public static Option<PixelPass> Pass(Twirl state, PassContext context) =>
        (state.Radius.Pixels(context.Extent), 1f / state.Falloff) switch {
            var (radius, exponent) => Displacements.Pass(state.Angle == TwirlAngle.Off || state.Radius == ShortSideLength.Neutral, state.Sampling, state.Wrap, state.Center.Point(context.Extent), d =>
                Vector2.Transform(d, Matrix3x2.CreateRotation(state.Angle * (float)RampInterpolation.Ease.Weight(Easing.Saturate(1f - float.Pow(d.Length() / radius, exponent)))
                    * (0.5f + (0.5f * float.Cos(state.Arms * (float.Atan2(-d.Y, d.X) - state.ArmOffset))))))),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TwirlParameter : IStateParameter<Twirl> {
    private static readonly (StateParameter<Twirl> X, StateParameter<Twirl> Y) Centered =
        FramePosition.Kinds(Lens<Twirl, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center }));

    public static readonly TwirlParameter Angle = new("angle", new StateParameter<Twirl>.Bounded<TwirlAngle, float, InvalidWarp>(
        Lens<Twirl, TwirlAngle>.New(static state => state.Angle, static angle => state => state with { Angle = angle }),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Soft = (-float.Tau, float.Tau), Origin = (float)TwirlAngle.Off }));
    public static readonly TwirlParameter Radius = new("radius", new StateParameter<Twirl>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Twirl, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius }), ShortSideLength.Presentation));
    public static readonly TwirlParameter Falloff = new("falloff", new StateParameter<Twirl>.Bounded<FalloffPower, float, InvalidWarp>(
        Lens<Twirl, FalloffPower>.New(static state => state.Falloff, static falloff => state => state with { Falloff = falloff }), new() { Soft = (0.25f, 4f), Scale = TrackScale.Log }));
    public static readonly TwirlParameter Arms = new("arms", new StateParameter<Twirl>.Bounded<ArmCount, int, InvalidWarp>(
        Lens<Twirl, ArmCount>.New(static state => state.Arms, static arms => state => state with { Arms = arms }), new() { Soft = (0, 8) }));
    public static readonly TwirlParameter ArmOffset = new("arm-offset", new StateParameter<Twirl>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Twirl, SignedAngle>.New(static state => state.ArmOffset, static offset => state => state with { ArmOffset = offset }), SignedAngle.Presentation));
    public static readonly TwirlParameter CenterX = new("center-x", Centered.X);
    public static readonly TwirlParameter CenterY = new("center-y", Centered.Y);
    public static readonly TwirlParameter Sampling = new("sampling", new StateParameter<Twirl>.Choice<Sampling, InvalidWarp>(
        Lens<Twirl, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly TwirlParameter Wrap = new("wrap", new StateParameter<Twirl>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Twirl, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Twirl> Kind { get; }
}

public sealed record Bulge(WarpAmount Amount, BulgeMode Mode, ShortSideLength Radius, FramePosition Center, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Bulge, BulgeParameter, InvalidWarp>, IPixelStage<Bulge> {
    public static Bulge Default { get; } = new(WarpAmount.Off, BulgeMode.Round, Displacements.Inscribed, FramePosition.Center, Sampling.Linear, WrapMode.Clamp);

    public static Option<PixelPass> Pass(Bulge state, PassContext context) =>
        (state.Radius.Pixels(context.Extent), (float)state.Amount) switch {
            var (radius, amount) => Displacements.Pass(state.Amount == WarpAmount.Off || state.Radius == ShortSideLength.Neutral, state.Sampling, state.Wrap, state.Center.Point(context.Extent), d =>
                (d * state.Mode.Axes) switch {
                    var v when v.Length() / radius is var u and > 0f and < 1f =>
                        d + ((((amount >= 0f ? float.Lerp(u, 2f / float.Pi * float.Asin(u), amount) : float.Lerp(u, float.SinPi(u / 2f), -amount)) / u) - 1f) * v),
                    _ => d,
                }),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BulgeParameter : IStateParameter<Bulge> {
    private static readonly (StateParameter<Bulge> X, StateParameter<Bulge> Y) Centered =
        FramePosition.Kinds(Lens<Bulge, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center }));

    public static readonly BulgeParameter Amount = new("amount", new StateParameter<Bulge>.Bounded<WarpAmount, float, InvalidWarp>(
        Lens<Bulge, WarpAmount>.New(static state => state.Amount, static amount => state => state with { Amount = amount }), new() { Origin = (float)WarpAmount.Off }));
    public static readonly BulgeParameter Mode = new("mode", new StateParameter<Bulge>.Choice<BulgeMode, InvalidWarp>(
        Lens<Bulge, BulgeMode>.New(static state => state.Mode, static mode => state => state with { Mode = mode })));
    public static readonly BulgeParameter Radius = new("radius", new StateParameter<Bulge>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Bulge, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius }), ShortSideLength.Presentation));
    public static readonly BulgeParameter CenterX = new("center-x", Centered.X);
    public static readonly BulgeParameter CenterY = new("center-y", Centered.Y);
    public static readonly BulgeParameter Sampling = new("sampling", new StateParameter<Bulge>.Choice<Sampling, InvalidWarp>(
        Lens<Bulge, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly BulgeParameter Wrap = new("wrap", new StateParameter<Bulge>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Bulge, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Bulge> Kind { get; }
}

public sealed record Wave(
    WaveFront Front, WaveProfile Profile, WaveCount Count, ShortSideExtent MinWavelength, ShortSideExtent MaxWavelength, AxisFraction MinAmplitude, AxisFraction MaxAmplitude,
    ShortSideOffset Horizontal, ShortSideOffset Vertical, SignedAngle Direction, SignedAngle Spread, SignedAngle Phase, ShortSideLength Radius, FramePosition Center, Frequency Rate,
    Seed Seed, Timing Timing, Hold Hold, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Wave, WaveParameter, InvalidWarp>, IPixelStage<Wave> {
    public static Wave Default { get; } = new(
        WaveFront.Linear, WaveProfile.Sine, WaveCount.Standard, ShortSideExtent.Create(10f / ReferenceFrame.Height), ShortSideExtent.Create(120f / ReferenceFrame.Height), AxisFraction.Create(5f / 35f),
        AxisFraction.MaxValue, ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, SignedAngle.Neutral, SignedAngle.Neutral, Displacements.Inscribed, FramePosition.Center,
        Frequency.Neutral, Seed.MinValue, Timing.Default, Hold.MinValue, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(Wave state, PassContext context) {
        (PixelExtent extent, int count, uint field, Matrix3x2 across) = (context.Extent, state.Count, CoordinateHash.Field(NoiseStream.Wave, state.Seed, 0u), Matrix3x2.CreateRotation(-float.Pi / 2f));
        (Vector2 amounts, float radius, float shift) =
            (extent.ShortSide * new Vector2(state.Horizontal, state.Vertical), state.Radius.Pixels(extent), (state.Phase / float.Tau) - (state.Rate * state.Hold.Held(state.Timing.At(context))));
        (bool ring, Matrix3x2 turn) = state.Front.Ring.Match(Some: weights => (true, (Matrix3x2.Identity * weights.X) + (across * weights.Y)), None: () => (false, across));
        (Vector2 Along, float Length, float Amplitude, float Phase)[] waves = [.. Enumerable.Range(0, count).Select(i =>
            (NoiseFunctions.White(Vector4.Zero, CoordinateHash.Branch(field, (uint)i)), state.Direction + (state.Spread * (i - ((count - 1) / 2f)) / int.Max(count - 1, 1))) switch {
                var (draw, angle) => (Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(-angle)), float.Lerp(state.MinWavelength.Pixels(extent), state.MaxWavelength.Pixels(extent), draw.X),
                    float.Lerp(state.MinAmplitude, state.MaxAmplitude, draw.Y), draw.Z + shift),
            })];
        return Displacements.Pass(amounts == Vector2.Zero || (state.MinAmplitude, state.MaxAmplitude) == (AxisFraction.MinValue, AxisFraction.MinValue) || (ring && state.Radius == ShortSideLength.Neutral),
            state.Sampling, state.Wrap, state.Center.Point(extent), d => {
                float reach = d.Length();
                (Vector2 spoke, Vector2 sum) = (reach > 0f ? d / reach : Vector2.UnitX, Vector2.Zero);
                foreach ((Vector2 along, float length, float amplitude, float phase) in waves) sum += Crest(d, ring ? spoke : along, length, amplitude, phase);
                return d + (amounts * sum * (ring ? float.Max(0f, 1f - (reach / radius)) : 1f) / count);
            });

        Vector2 Crest(Vector2 d, Vector2 axis, float length, float amplitude, float phase) =>
            amplitude * ((2f * state.Profile.Level((Vector2.Dot(d, axis) / length) + phase)) - 1f) * Vector2.TransformNormal(axis, turn);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveParameter : IStateParameter<Wave> {
    private static readonly (StateParameter<Wave> X, StateParameter<Wave> Y) Centered =
        FramePosition.Kinds(Lens<Wave, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center }));
    private static readonly Presentation<ShortSideExtent, float> Wavelength = ShortSideExtent.Presentation with { Soft = (0.005f, 1f) };

    public static readonly WaveParameter Front = new("front", new StateParameter<Wave>.Choice<WaveFront, InvalidWarp>(
        Lens<Wave, WaveFront>.New(static state => state.Front, static front => state => state with { Front = front })));
    public static readonly WaveParameter Profile = new("profile", new StateParameter<Wave>.Choice<WaveProfile, InvalidGenerator>(
        Lens<Wave, WaveProfile>.New(static state => state.Profile, static profile => state => state with { Profile = profile })));
    public static readonly WaveParameter Count = new("count", new StateParameter<Wave>.Bounded<WaveCount, int, InvalidWarp>(
        Lens<Wave, WaveCount>.New(static state => state.Count, static count => state => state with { Count = count }), new() { Soft = (1, 16) }));
    public static readonly WaveParameter MinWavelength = new("min-wavelength", new StateParameter<Wave>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Wave, ShortSideExtent>.New(static state => state.MinWavelength, static wavelength => state => state with { MinWavelength = wavelength }), Wavelength));
    public static readonly WaveParameter MaxWavelength = new("max-wavelength", new StateParameter<Wave>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Wave, ShortSideExtent>.New(static state => state.MaxWavelength, static wavelength => state => state with { MaxWavelength = wavelength }), Wavelength));
    public static readonly WaveParameter MinAmplitude = new("min-amplitude", new StateParameter<Wave>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Wave, AxisFraction>.New(static state => state.MinAmplitude, static amplitude => state => state with { MinAmplitude = amplitude }), new()));
    public static readonly WaveParameter MaxAmplitude = new("max-amplitude", new StateParameter<Wave>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Wave, AxisFraction>.New(static state => state.MaxAmplitude, static amplitude => state => state with { MaxAmplitude = amplitude }), new()));
    public static readonly WaveParameter Horizontal = new("horizontal", new StateParameter<Wave>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Wave, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Displacements.Shift));
    public static readonly WaveParameter Vertical = new("vertical", new StateParameter<Wave>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Wave, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Displacements.Shift));
    public static readonly WaveParameter Direction = new("direction", new StateParameter<Wave>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Wave, SignedAngle>.New(static state => state.Direction, static direction => state => state with { Direction = direction }), SignedAngle.Presentation));
    public static readonly WaveParameter Spread = new("spread", new StateParameter<Wave>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Wave, SignedAngle>.New(static state => state.Spread, static spread => state => state with { Spread = spread }), SignedAngle.Presentation with { Soft = (0f, float.Pi) }));
    public static readonly WaveParameter Phase = new("phase", new StateParameter<Wave>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Wave, SignedAngle>.New(static state => state.Phase, static phase => state => state with { Phase = phase }), SignedAngle.Presentation));
    public static readonly WaveParameter Radius = new("radius", new StateParameter<Wave>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Wave, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius }), ShortSideLength.Presentation));
    public static readonly WaveParameter CenterX = new("center-x", Centered.X);
    public static readonly WaveParameter CenterY = new("center-y", Centered.Y);
    public static readonly WaveParameter Rate = new("rate", new StateParameter<Wave>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Wave, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (-2f, 2f) }));
    public static readonly WaveParameter Seed = new("seed", new StateParameter<Wave>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Wave, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly WaveParameter Timing = new("timing", new StateParameter<Wave>.Record<Timing>(
        Lens<Wave, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));
    public static readonly WaveParameter Hold = new("hold", new StateParameter<Wave>.Bounded<Hold, float, InvalidGenerator>(
        Lens<Wave, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly WaveParameter Sampling = new("sampling", new StateParameter<Wave>.Choice<Sampling, InvalidWarp>(
        Lens<Wave, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly WaveParameter Wrap = new("wrap", new StateParameter<Wave>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Wave, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Wave> Kind { get; }
}

public sealed record Turbulence(
    NoiseBasis Basis, ShortSideExtent ScaleX, ShortSideExtent ScaleY, ShortSideOffset Horizontal, ShortSideOffset Vertical, Frequency Rate, Seed Seed, Timing Timing, Hold Hold,
    Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Turbulence, TurbulenceParameter, InvalidWarp>, IPixelStage<Turbulence> {
    public static Turbulence Default { get; } = new(
        NoiseBasis.Default, Displacements.Uniform, Displacements.Uniform, ShortSideOffset.Neutral, ShortSideOffset.Neutral, Frequency.Neutral, Seed.MinValue, Timing.Default, Hold.MinValue,
        Sampling.Linear, WrapMode.Clamp);

    public static Option<PixelPass> Pass(Turbulence state, PassContext context) {
        (PixelExtent extent, Func<Vector4, uint, NoiseSample> sampler, uint field) = (context.Extent, state.Basis.Sampler, CoordinateHash.Field(NoiseStream.Turbulence, state.Seed, 0u));
        (Vector2 scale, Vector2 amounts, float depth) = (new Vector2(state.ScaleX.Pixels(extent), -state.ScaleY.Pixels(extent)), extent.ShortSide * new Vector2(state.Horizontal, state.Vertical),
            state.Rate * state.Hold.Held(state.Timing.At(context)));
        (uint right, uint left, uint down, uint up) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u));
        return Displacements.Pass(amounts == Vector2.Zero, state.Sampling, state.Wrap, FramePosition.Center.Point(extent), d => new Vector4(d / scale, depth, 0f) switch {
            var at => d + (amounts * new Vector2(sampler(at, right).Value - sampler(at, left).Value, sampler(at, down).Value - sampler(at, up).Value)),
        });
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TurbulenceParameter : IStateParameter<Turbulence> {
    private static readonly Presentation<ShortSideExtent, float> Feature = ShortSideExtent.Presentation with { Soft = (0.01f, 2f) };

    public static readonly TurbulenceParameter Basis = new("basis", new StateParameter<Turbulence>.Record<NoiseBasis>(
        Lens<Turbulence, NoiseBasis>.New(static state => state.Basis, static basis => state => state with { Basis = basis })));
    public static readonly TurbulenceParameter ScaleX = new("scale-x", new StateParameter<Turbulence>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Turbulence, ShortSideExtent>.New(static state => state.ScaleX, static scale => state => state with { ScaleX = scale }), Feature));
    public static readonly TurbulenceParameter ScaleY = new("scale-y", new StateParameter<Turbulence>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Turbulence, ShortSideExtent>.New(static state => state.ScaleY, static scale => state => state with { ScaleY = scale }), Feature));
    public static readonly TurbulenceParameter Horizontal = new("horizontal", new StateParameter<Turbulence>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Turbulence, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Displacements.Shift));
    public static readonly TurbulenceParameter Vertical = new("vertical", new StateParameter<Turbulence>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Turbulence, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Displacements.Shift));
    public static readonly TurbulenceParameter Rate = new("rate", new StateParameter<Turbulence>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Turbulence, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (-2f, 2f) }));
    public static readonly TurbulenceParameter Seed = new("seed", new StateParameter<Turbulence>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Turbulence, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly TurbulenceParameter Timing = new("timing", new StateParameter<Turbulence>.Record<Timing>(
        Lens<Turbulence, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));
    public static readonly TurbulenceParameter Hold = new("hold", new StateParameter<Turbulence>.Bounded<Hold, float, InvalidGenerator>(
        Lens<Turbulence, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly TurbulenceParameter Sampling = new("sampling", new StateParameter<Turbulence>.Choice<Sampling, InvalidWarp>(
        Lens<Turbulence, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly TurbulenceParameter Wrap = new("wrap", new StateParameter<Turbulence>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Turbulence, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Turbulence> Kind { get; }
}

public sealed record MapDisplacement(
    Option<ImageFile> Source, MapReading Reading, Gated<ShortSideExtent> TileSize, ShortSideExtent Softness, ShortSideOffset Horizontal, ShortSideOffset Vertical, SignedAngle Rotation,
    Sampling Sampling, WrapMode Wrap)
    : IStateRecord<MapDisplacement, MapDisplacementParameter, InvalidWarp>, IPixelStage<MapDisplacement> {
    public static MapDisplacement Default { get; } = new(
        None, MapReading.Gradient, new(Enabled: false, ShortSideExtent.Create(1f)), ShortSideExtent.Create(0.015f * ReferenceFrame.GreaterSide), ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(MapDisplacement state, PassContext context) =>
        state.Reading.Plane.Match(
            Some: plane => (state.Horizontal, state.Vertical) == (ShortSideOffset.Neutral, ShortSideOffset.Neutral)
                ? None
                : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kernel(plane, context, frame, progress))),
            None: () => state.Source.Map(image => (PixelPass)new PixelPass.Frame(new CoordinateMap.Sampled(image.Frame, state.Sampling, state.Wrap).Apply)));

    private Fin<Unit> Kernel(Action<Mat, Mat, float, PassContext> plane, PassContext context, PixelFrame frame, IProgress<int> progress) {
        PixelFrame map = Source.Match(Some: image => Fitted(image, frame), None: () => frame);
        using Mat header = map.Header();
        using Mat offsets = new();
        plane(header, offsets, Softness.Pixels(context.Extent), context);
        (Matrix3x2 bend, Point corner, int width) =
            (Matrix3x2.CreateScale(context.Extent.ShortSide * new Vector2(Horizontal, Vertical)) * Matrix3x2.CreateRotation(-Rotation), frame.Origin, map.Size.Width);
        return new CoordinateMap.Analytic((points, _, _) => {
            ReadOnlySpan<Vector2> read = offsets.GetSpan<Vector2>();
            foreach (ref Vector2 point in points) point -= Vector2.TransformNormal(read[(((int)point.Y - corner.Y) * width) + (int)point.X - corner.X], bend);
        }, Sampling, Wrap).Apply(frame, progress);
    }

    private PixelFrame Fitted(ImageFile image, PixelFrame frame) =>
        TileSize.Active.Match(
            Some: tile => (new Vector2(image.Frame.Extent.Height / tile.Pixels(frame.Extent)), WrapMode.Periodic),
            None: () => (new Vector2(image.Frame.Extent.Width, image.Frame.Extent.Height) / new Vector2(frame.Extent.Width, frame.Extent.Height), WrapMode.Clamp)) switch {
                var (scale, wrap) => new CoordinateMap.Analytic(Displacements.Kernel(Vector2.Zero, point => (point + new Vector2(frame.Origin.X, frame.Origin.Y)) * scale), Sampling.Linear, wrap)
                    .Resample(image.Frame, frame.Size),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapDisplacementParameter : IStateParameter<MapDisplacement> {
    public static readonly MapDisplacementParameter Source = new("source", new StateParameter<MapDisplacement>.Loaded<ImageFile, ImagePath, string, InvalidPixelValue>(
        Lens<MapDisplacement, Option<ImageFile>>.New(static state => state.Source, static source => state => state with { Source = source }), ImageFile.Load, static image => image.Path));
    public static readonly MapDisplacementParameter Reading = new("reading", new StateParameter<MapDisplacement>.Choice<MapReading, InvalidWarp>(
        Lens<MapDisplacement, MapReading>.New(static state => state.Reading, static reading => state => state with { Reading = reading })));
    public static readonly MapDisplacementParameter TileSize = new("tile-size", new StateParameter<MapDisplacement>.OptionalBounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<MapDisplacement, Gated<ShortSideExtent>>.New(static state => state.TileSize, static size => state => state with { TileSize = size }), ShortSideExtent.Presentation with { Soft = (0.05f, 2f) }));
    public static readonly MapDisplacementParameter Softness = new("softness", new StateParameter<MapDisplacement>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<MapDisplacement, ShortSideExtent>.New(static state => state.Softness, static softness => state => state with { Softness = softness }),
        ShortSideExtent.Presentation with { Soft = (0.001f, 0.1f), Scale = TrackScale.Linear }));
    public static readonly MapDisplacementParameter Horizontal = new("horizontal", new StateParameter<MapDisplacement>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<MapDisplacement, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Displacements.Shift));
    public static readonly MapDisplacementParameter Vertical = new("vertical", new StateParameter<MapDisplacement>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<MapDisplacement, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Displacements.Shift));
    public static readonly MapDisplacementParameter Rotation = new("rotation", new StateParameter<MapDisplacement>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<MapDisplacement, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation));
    public static readonly MapDisplacementParameter Sampling = new("sampling", new StateParameter<MapDisplacement>.Choice<Sampling, InvalidWarp>(
        Lens<MapDisplacement, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly MapDisplacementParameter Wrap = new("wrap", new StateParameter<MapDisplacement>.Choice<WrapMode, InvalidPixelValue>(
        Lens<MapDisplacement, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<MapDisplacement> Kind { get; }
}

public sealed record Refraction(
    RefractionField Field, ShortSideExtent Scale, ShortSideOffset Strength, ShortSideExtent Smoothness, ShortSideLength Blur, Likelihood Density, DropSize Size, AxisFraction Variation,
    AxisFraction Softness, DropLens Lens, AxisFraction Trails, SignedAngle Direction, Drift Fall, Seed Seed, Timing Timing, Hold Hold, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Refraction, RefractionParameter, InvalidWarp>, IPixelStage<Refraction> {
    public static Refraction Default { get; } = new(
        RefractionField.Raindrops, ShortSideExtent.Create(Displacements.Uniform / 3.8f), ShortSideOffset.Neutral, ShortSideExtent.Create(3f / ReferenceFrame.Height),
        ShortSideLength.Create(14.6f / ReferenceFrame.Height), Likelihood.Create(0.5f), DropSize.MaxValue, AxisFraction.Half, AxisFraction.Create(0.05f), DropLens.Standard,
        AxisFraction.Create(0.3f), SignedAngle.Down, Drift.Neutral, Seed.MinValue, Timing.Default, Hold.MinValue, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(Refraction state, PassContext context) =>
        state.Field.Lanes(state, state.Hold.Held(state.Timing.At(context)), CoordinateHash.Field(NoiseStream.Refraction, state.Seed, 0u))
            .Map(lanes => (PixelPass)new PixelPass.Frame((frame, progress) => state.Kernel(lanes, context, frame, progress)));

    private Fin<Unit> Kernel(Func<Vector2, Vector4> lanes, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (PixelExtent extent, Vector2 half) = (context.Extent, FramePosition.Center.Point(context.Extent));
        (PixelFrame field, PixelFrame refracted) = (new(Point.Empty, extent, extent, static _ => { }), new(frame.Origin, frame.Size, frame.Extent, block => frame.Block.CopyTo(block)));
        return new PixelPass.Pointwise((row, column, line) => {
            for (int x = 0; x < row.Length; x++) row[x] = new Vector4(extent.ShortSide, extent.ShortSide, 1f, 1f) * lanes((new Vector2(column + x + 0.5f, extent.Height - line - 0.5f) - half) / extent.ShortSide);
        }).Run(field, new Progress<int>()).Bind(_ => {
            using Mat drawn = field.Header();
            using Mat level = new();
            using Mat slope = new();
            using Mat source = refracted.Header();
            CvInvoke.ExtractChannel(drawn, level, 2);
            Displacements.Smooth(level, Smoothness.Pixels(extent));
            Displacements.Slope(level, slope, Strength.Pixels(extent) * Scale.Pixels(extent) / 8d);
            Displacements.Smooth(source, Blur.Pixels(extent));
            return new CoordinateMap.Analytic((points, coverage, _) => {
                ReadOnlySpan<Vector2> bend = slope.GetSpan<Vector2>();
                ReadOnlySpan<Vector4> drops = drawn.GetSpan<Vector4>();
                for (int i = 0; i < points.Length; i++)
                    (points[i], coverage[i]) = (((int)points[i].Y * extent.Width) + (int)points[i].X) switch { var at => (points[i] + drops[at].AsVector2() - bend[at], coverage[i] * drops[at].W) };
            }, Sampling, Wrap).Apply(refracted, progress);
        }).Map(_ => {
            Blend.Composite(frame, refracted, BlendingMode.Mix, Mix.Full, []).View.Span.CopyTo(frame.View.Span);
            return unit;
        });
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefractionParameter : IStateParameter<Refraction> {
    public static readonly RefractionParameter Field = new("field", new StateParameter<Refraction>.Choice<RefractionField, InvalidWarp>(
        Lens<Refraction, RefractionField>.New(static state => state.Field, static field => state => state with { Field = field })));
    public static readonly RefractionParameter Scale = new("scale", new StateParameter<Refraction>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Refraction, ShortSideExtent>.New(static state => state.Scale, static scale => state => state with { Scale = scale }), ShortSideExtent.Presentation with { Soft = (0.005f, 1f) }));
    public static readonly RefractionParameter Strength = new("strength", new StateParameter<Refraction>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Refraction, ShortSideOffset>.New(static state => state.Strength, static strength => state => state with { Strength = strength }), Displacements.Shift));
    public static readonly RefractionParameter Smoothness = new("smoothness", new StateParameter<Refraction>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Refraction, ShortSideExtent>.New(static state => state.Smoothness, static smoothness => state => state with { Smoothness = smoothness }),
        ShortSideExtent.Presentation with { Soft = (0.001f, 0.05f) }));
    public static readonly RefractionParameter Blur = new("blur", new StateParameter<Refraction>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Refraction, ShortSideLength>.New(static state => state.Blur, static blur => state => state with { Blur = blur }), ShortSideLength.Presentation with { Soft = (0f, 0.05f) }));
    public static readonly RefractionParameter Density = new("density", new StateParameter<Refraction>.Bounded<Likelihood, float, InvalidStylize>(
        Lens<Refraction, Likelihood>.New(static state => state.Density, static density => state => state with { Density = density }), new()));
    public static readonly RefractionParameter Size = new("size", new StateParameter<Refraction>.Bounded<DropSize, float, InvalidWarp>(
        Lens<Refraction, DropSize>.New(static state => state.Size, static size => state => state with { Size = size }), new()));
    public static readonly RefractionParameter Variation = new("variation", new StateParameter<Refraction>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Refraction, AxisFraction>.New(static state => state.Variation, static variation => state => state with { Variation = variation }), new()));
    public static readonly RefractionParameter Softness = new("softness", new StateParameter<Refraction>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Refraction, AxisFraction>.New(static state => state.Softness, static softness => state => state with { Softness = softness }), new()));
    public static readonly RefractionParameter Lens = new("lens", new StateParameter<Refraction>.Bounded<DropLens, float, InvalidWarp>(
        Lens<Refraction, DropLens>.New(static state => state.Lens, static lens => state => state with { Lens = lens }), new()));
    public static readonly RefractionParameter Trails = new("trails", new StateParameter<Refraction>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Refraction, AxisFraction>.New(static state => state.Trails, static trails => state => state with { Trails = trails }), new()));
    public static readonly RefractionParameter Direction = new("direction", new StateParameter<Refraction>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Refraction, SignedAngle>.New(static state => state.Direction, static direction => state => state with { Direction = direction }), SignedAngle.Presentation));
    public static readonly RefractionParameter Fall = new("fall", new StateParameter<Refraction>.Bounded<Drift, float, InvalidGenerator>(
        Lens<Refraction, Drift>.New(static state => state.Fall, static fall => state => state with { Fall = fall }), Drift.Presentation with { Soft = (-1f, 1f) }));
    public static readonly RefractionParameter Seed = new("seed", new StateParameter<Refraction>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Refraction, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly RefractionParameter Timing = new("timing", new StateParameter<Refraction>.Record<Timing>(
        Lens<Refraction, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));
    public static readonly RefractionParameter Hold = new("hold", new StateParameter<Refraction>.Bounded<Hold, float, InvalidGenerator>(
        Lens<Refraction, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly RefractionParameter Sampling = new("sampling", new StateParameter<Refraction>.Choice<Sampling, InvalidWarp>(
        Lens<Refraction, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly RefractionParameter Wrap = new("wrap", new StateParameter<Refraction>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Refraction, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Refraction> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Displacements {
    public static ShortSideLength Inscribed { get; } = ShortSideLength.Create(0.5f);
    public static ShortSideExtent Uniform { get; } = ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f);
    public static Presentation<ShortSideOffset, float> Shift { get; } = ShortSideOffset.Presentation with { Soft = (-0.1f, 0.1f) };

    public static Option<PixelPass> Pass(bool rests, Sampling sampling, WrapMode wrap, Vector2 origin, Func<Vector2, Vector2> field) =>
        rests ? None : Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(Kernel(origin, field), sampling, wrap).Apply));

    public static Action<Span<Vector2>, Span<float>, PixelExtent> Kernel(Vector2 origin, Func<Vector2, Vector2> field) =>
        (points, _, _) => {
            foreach (ref Vector2 point in points) point = origin + field(point - origin);
        };

    public static void Smooth(Mat plane, float sigma) =>
        CvInvoke.GaussianBlur(plane, plane, PixelSampling.GaussianTaps(sigma) switch { var taps => new Size(taps, taps) }, sigma, sigma, BorderType.Replicate);

    public static void Slope(Mat level, Mat plane, double scale) {
        using Mat across = new();
        using Mat down = new();
        CvInvoke.Sobel(level, across, DepthType.Cv32F, 1, 0, 3, scale, 0d, BorderType.Replicate);
        CvInvoke.Sobel(level, down, DepthType.Cv32F, 0, 1, 3, scale, 0d, BorderType.Replicate);
        using VectorOfMat pair = new(across, down);
        CvInvoke.Merge(pair, plane);
    }

    public static void Transform(Mat source, Mat target, int rows, float[] weights) {
        using Mat matrix = new(rows, weights.Length / rows, DepthType.Cv32F, 1);
        matrix.SetTo(weights);
        CvInvoke.Transform(source, target, matrix);
    }
}
