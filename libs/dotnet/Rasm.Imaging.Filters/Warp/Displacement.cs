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

// --- [CONSTANTS] -----------------------------------------------------------------------
file static class Amounts {
    public static Presentation<ShortSideOffset, float> Shift { get; } = ShortSideOffset.Presentation with { Soft = (-0.1f, 0.1f) };
}

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

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct WarpShare : IMinMaxValue<WarpShare> {
    public static WarpShare MinValue => Neutral;
    public static WarpShare MaxValue { get; } = new(1f);

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
    public static readonly MapReading Gradient = new("gradient", Some<Action<Mat, Mat, float, PassContext>>(static (map, plane, sigma, context) => {
        using Mat height = Height(map, sigma, context);
        using Mat across = new();
        using Mat down = new();
        double scale = double.Sqrt(double.Tau) * sigma / 8d;
        CvInvoke.Sobel(height, across, DepthType.Cv32F, 1, 0, 3, scale, 0d, BorderType.Replicate);
        CvInvoke.Sobel(height, down, DepthType.Cv32F, 0, 1, 3, scale, 0d, BorderType.Replicate);
        using VectorOfMat pair = new(across, down);
        CvInvoke.Merge(pair, plane);
    }));
    public static readonly MapReading Luma = new("luma", Some<Action<Mat, Mat, float, PassContext>>(static (map, plane, sigma, context) => {
        using Mat height = Height(map, sigma, context);
        height.ConvertTo(height, DepthType.Cv32F, 2d, -1d);
        using VectorOfMat pair = new(height, height);
        CvInvoke.Merge(pair, plane);
    }));
    public static readonly MapReading Offsets = new("offsets", Some<Action<Mat, Mat, float, PassContext>>(static (map, plane, _, _) => {
        using Mat weights = new(2, 5, DepthType.Cv32F, 1);
        weights.SetTo([2f, 0f, 0f, 0f, -1f, 0f, -2f, 0f, 0f, 1f]);
        CvInvoke.Transform(map, plane, weights);
    }));
    public static readonly MapReading Coordinates = new("coordinates", None);

    internal Option<Action<Mat, Mat, float, PassContext>> Plane { get; }

    private static Mat Height(Mat map, float sigma, PassContext context) {
        using Mat weights = new(1, 4, DepthType.Cv32F, 1);
        weights.SetTo([context.Working.Luminance.X, context.Working.Luminance.Y, context.Working.Luminance.Z, 0f]);
        Mat height = new();
        CvInvoke.Transform(map, height, weights);
        using ScalarArray none = new(0d);
        CvInvoke.Max(height, none, height);
        using ScalarArray grey = new(Exposure.MiddleGrey / context.Exposure.Scale);
        using Mat total = new();
        CvInvoke.Add(height, grey, total);
        CvInvoke.Divide(height, total, height);
        int taps = (2 * (int)float.Ceiling(3f * sigma)) + 1;
        CvInvoke.GaussianBlur(height, height, new Size(taps, taps), sigma, sigma, BorderType.Replicate);
        return height;
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapFit {
    public static readonly MapFit Stretch = new("stretch", WrapMode.Clamp, static (_, image, frame) => new Vector2(image.Width, image.Height) / new Vector2(frame.Width, frame.Height));
    public static readonly MapFit Tile = new("tile", WrapMode.Periodic, static (state, image, frame) => new Vector2(image.Height / state.TileSize.Pixels(frame)));

    public WrapMode Wrap { get; }

    [UseDelegateFromConstructor]
    public partial Vector2 Scale(MapDisplacement state, PixelExtent image, PixelExtent frame);
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefractionField {
    public static readonly RefractionField Raindrops = new("raindrops", Some(0.01f), static (state, time) => {
        (Vector2 fall, Vector2 across) = (Toward(state.Direction), Toward(state.Direction + (float.Pi / 2f)));
        (float scale, float density, float trails, float drift) = (state.Scale, state.Density, state.Trails, state.Fall * time);
        uint field = CoordinateHash.Field(NoiseStream.Refraction, state.Seed, state.Hold.Period(time));
        return q => {
            Vector2 cell = new Vector2(Vector2.Dot(q, across), Vector2.Dot(q, fall) - drift) / scale;
            Vector2 floor = Vector2.Round(cell, MidpointRounding.ToNegativeInfinity);
            Vector4 draw = NoiseFunctions.White(new Vector4(floor, 0f, 0f), field);
            Vector2 center = floor + new Vector2(0.25f) + (0.5f * new Vector2(draw.Y, draw.Z));
            float radius = 0.1f + (0.15f * draw.X / density);
            return draw.X < density
                ? float.Max(
                    float.Sqrt(float.Max(0f, 1f - float.Pow(Vector2.Distance(cell, center) / radius, 2f))),
                    cell.Y <= center.Y ? trails * float.Sqrt(float.Max(0f, 1f - float.Pow((cell.X - center.X) / (0.5f * radius), 2f))) * (cell.Y - floor.Y) / (center.Y - floor.Y) : 0f)
                : 0f;
        };
    });
    public static readonly RefractionField Blocks = new("blocks", None, static (state, _) => {
        float scale = state.Scale;
        return q => Vector2.Abs((2f * Fraction(q / scale)) - Vector2.One) switch {
            var edge => (Vector2.One - (edge * edge * edge * edge)) switch { var bevel => bevel.X * bevel.Y },
        };
    });
    public static readonly RefractionField Canvas = new("canvas", None, static (state, _) => {
        float scale = state.Scale;
        return q => 0.5f + (0.5f * float.SinPi(2f * q.X / scale) * float.SinPi(2f * q.Y / scale));
    });
    public static readonly RefractionField Frosted = new("frosted", None, static (state, _) => {
        (float scale, uint field) = (state.Scale, CoordinateHash.Branch(CoordinateHash.Field(NoiseStream.Refraction, state.Seed, 0u), 2u));
        return q => 0.5f + (0.5f * NoiseDimensions.Two.Fbm(new Vector4(q / scale, 0f, 0f), Octaves.Default, field));
    });
    public static readonly RefractionField TinyLens = new("tiny-lens", None, static (state, _) => {
        float scale = state.Scale;
        return q => float.Sqrt(float.Max(0f, 1f - (4f * (Fraction(q / scale) - new Vector2(0.5f)).LengthSquared())));
    });

    public Option<float> CoverageRamp { get; }

    [UseDelegateFromConstructor]
    internal partial Func<Vector2, float> Height(Refraction state, Clock time);

    private static Vector2 Toward(float angle) => Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(-angle));

    private static Vector2 Fraction(Vector2 cell) => cell - Vector2.Round(cell, MidpointRounding.ToNegativeInfinity);
}

public sealed record Twirl(TwirlAngle Angle, ShortSideLength Radius, FalloffPower Falloff, ArmCount Arms, SignedAngle ArmOffset, FramePosition Center, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Twirl, TwirlParameter, InvalidWarp>, IPixelStage<Twirl> {
    public static Twirl Default { get; } = new(TwirlAngle.Off, ShortSideLength.Create(0.5f), FalloffPower.Linear, ArmCount.Uniform, SignedAngle.Neutral, FramePosition.Center, Sampling.Linear, WrapMode.Periodic);

    public static Option<PixelPass> Pass(Twirl state, PassContext context) =>
        state.Angle == TwirlAngle.Off || state.Radius == ShortSideLength.Neutral
            ? None
            : Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(state.Kernel(context.Extent), state.Sampling, state.Wrap).Apply));

    private Action<Span<Vector2>, Span<float>, PixelExtent> Kernel(PixelExtent extent) {
        (Vector2 center, float radius, float angle, float falloff, int arms, float offset) = (Center.Point(extent), Radius.Pixels(extent), Angle, Falloff, Arms, ArmOffset);
        return (points, _, _) => {
            foreach (ref Vector2 point in points) {
                Vector2 d = point - center;
                float u = d.Length() / radius;
                float share = arms == ArmCount.Uniform ? 1f : 0.5f + (0.5f * float.Cos(arms * (float.Atan2(-d.Y, d.X) - offset)));
                point = u < 1f ? center + Vector2.Transform(d, Matrix3x2.CreateRotation(angle * float.Pow(1f - u, falloff) * share)) : point;
            }
        };
    }
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
    public static Bulge Default { get; } = new(WarpAmount.Off, BulgeMode.Round, ShortSideLength.Create(0.5f), FramePosition.Center, Sampling.Linear, WrapMode.Clamp);

    public static Option<PixelPass> Pass(Bulge state, PassContext context) =>
        state.Amount == WarpAmount.Off || state.Radius == ShortSideLength.Neutral
            ? None
            : Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(state.Kernel(context.Extent), state.Sampling, state.Wrap).Apply));

    private Action<Span<Vector2>, Span<float>, PixelExtent> Kernel(PixelExtent extent) {
        (Vector2 center, Vector2 axes, float radius, float amount) = (Center.Point(extent), Mode.Axes, Radius.Pixels(extent), Amount);
        return (points, _, _) => {
            foreach (ref Vector2 point in points) {
                Vector2 v = (point - center) * axes;
                float u = v.Length() / radius;
                float bent = amount >= 0f ? float.Lerp(u, 2f / float.Pi * float.Asin(u), amount) : float.Lerp(u, float.SinPi(u / 2f), -amount);
                point = u is > 0f and < 1f ? point + (((bent / u) - 1f) * v) : point;
            }
        };
    }
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
    WaveFront Front, WaveProfile Profile, WaveCount Count, ShortSideExtent MinWavelength, ShortSideExtent MaxWavelength, WarpShare MinAmplitude, WarpShare MaxAmplitude,
    ShortSideOffset Horizontal, ShortSideOffset Vertical, SignedAngle Direction, SignedAngle Spread, ShortSideLength Radius, FramePosition Center, Frequency Rate,
    Seed Seed, Timing Timing, Hold Hold, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Wave, WaveParameter, InvalidWarp>, IPixelStage<Wave> {
    public static Wave Default { get; } = new(
        WaveFront.Linear, WaveProfile.Sine, WaveCount.Standard, ShortSideExtent.Create(10f / 1080f), ShortSideExtent.Create(120f / 1080f), WarpShare.Create(5f / 35f), WarpShare.MaxValue,
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, SignedAngle.Neutral, ShortSideLength.Create(0.5f), FramePosition.Center, Frequency.Neutral,
        Seed.MinValue, Timing.Standard, Hold.MinValue, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(Wave state, PassContext context) =>
        (state.Horizontal == ShortSideOffset.Neutral && state.Vertical == ShortSideOffset.Neutral)
        || (state.MinAmplitude == WarpShare.Neutral && state.MaxAmplitude == WarpShare.Neutral)
        || (state.Front.Ring.IsSome && state.Radius == ShortSideLength.Neutral)
            ? None
            : Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(state.Kernel(context), state.Sampling, state.Wrap).Apply));

    private Action<Span<Vector2>, Span<float>, PixelExtent> Kernel(PassContext context) {
        (Vector2 center, float side) = (Center.Point(context.Extent), context.Extent.ShortSide);
        (Vector2 amounts, float radius, float phase, int count) = (side * new Vector2(Horizontal, Vertical), Radius, Rate * Hold.Held(Timing.At(context)), Count);
        uint field = CoordinateHash.Field(NoiseStream.Wave, Seed, 0u);
        (Vector2 Along, Vector2 Across, float Wavelength, float Amplitude, float Phase)[] waves = [.. Enumerable.Range(0, count).Select(i =>
            (NoiseFunctions.White(Vector4.Zero, CoordinateHash.Branch(field, (uint)i)), Matrix3x2.CreateRotation(-(count == 1 ? Direction : Direction + (Spread * (((float)i / (count - 1)) - 0.5f))))) switch {
                var (draw, turn) => (Vector2.Transform(Vector2.UnitX, turn), Vector2.Transform(-Vector2.UnitY, turn),
                    float.Lerp(float.Min(MinWavelength, MaxWavelength), float.Max(MinWavelength, MaxWavelength), draw.X),
                    float.Lerp(float.Min(MinAmplitude, MaxAmplitude), float.Max(MinAmplitude, MaxAmplitude), draw.Y), draw.Z),
            })];
        Func<Vector2, int, (float Argument, Vector2 Direction)> wave = Front.Ring.Match(
            Some: ring => (Func<Vector2, int, (float, Vector2)>)((d, i) => d.Length() switch {
                var r => ((r / waves[i].Wavelength) + waves[i].Phase - phase,
                    Vector2.Transform(new Vector2(ring.X, -ring.Y), Matrix3x2.CreateRotation(-float.Atan2(-d.Y, d.X))) * float.Max(0f, 1f - (r / radius))),
            }),
            None: () => (d, i) => ((Vector2.Dot(d, waves[i].Along) / waves[i].Wavelength) + waves[i].Phase + phase, waves[i].Across));
        return (points, _, _) => {
            foreach (ref Vector2 point in points) {
                Vector2 sum = Vector2.Zero;
                for (int i = 0; i < waves.Length; i++) {
                    (float argument, Vector2 direction) = wave((point - center) / side, i);
                    sum += waves[i].Amplitude * ((2f * Profile.Level(float.Tau * argument)) - 1f) * direction;
                }
                point += amounts * sum / waves.Length;
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveParameter : IStateParameter<Wave> {
    private static readonly (StateParameter<Wave> Clock, StateParameter<Wave> Pace) Time =
        Timing.Kinds(Lens<Wave, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    private static readonly (StateParameter<Wave> X, StateParameter<Wave> Y) Centered =
        FramePosition.Kinds(Lens<Wave, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center }));
    private static readonly Presentation<ShortSideExtent, float> Wavelength =
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.005f, 1f), Scale = TrackScale.Log };

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
    public static readonly WaveParameter MinAmplitude = new("min-amplitude", new StateParameter<Wave>.Bounded<WarpShare, float, InvalidWarp>(
        Lens<Wave, WarpShare>.New(static state => state.MinAmplitude, static amplitude => state => state with { MinAmplitude = amplitude }), new()));
    public static readonly WaveParameter MaxAmplitude = new("max-amplitude", new StateParameter<Wave>.Bounded<WarpShare, float, InvalidWarp>(
        Lens<Wave, WarpShare>.New(static state => state.MaxAmplitude, static amplitude => state => state with { MaxAmplitude = amplitude }), new()));
    public static readonly WaveParameter Horizontal = new("horizontal", new StateParameter<Wave>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Wave, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Amounts.Shift));
    public static readonly WaveParameter Vertical = new("vertical", new StateParameter<Wave>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Wave, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Amounts.Shift));
    public static readonly WaveParameter Direction = new("direction", new StateParameter<Wave>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Wave, SignedAngle>.New(static state => state.Direction, static direction => state => state with { Direction = direction }), SignedAngle.Presentation));
    public static readonly WaveParameter Spread = new("spread", new StateParameter<Wave>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Wave, SignedAngle>.New(static state => state.Spread, static spread => state => state with { Spread = spread }), SignedAngle.Presentation with { Soft = (0f, float.Pi) }));
    public static readonly WaveParameter Radius = new("radius", new StateParameter<Wave>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Wave, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius }), ShortSideLength.Presentation));
    public static readonly WaveParameter CenterX = new("center-x", Centered.X);
    public static readonly WaveParameter CenterY = new("center-y", Centered.Y);
    public static readonly WaveParameter Rate = new("rate", new StateParameter<Wave>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Wave, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (-2f, 2f) }));
    public static readonly WaveParameter Seed = new("seed", new StateParameter<Wave>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Wave, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly WaveParameter Clock = new("clock", Time.Clock);
    public static readonly WaveParameter Pace = new("pace", Time.Pace);
    public static readonly WaveParameter Hold = new("hold", new StateParameter<Wave>.Bounded<Hold, float, InvalidGenerator>(
        Lens<Wave, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly WaveParameter Sampling = new("sampling", new StateParameter<Wave>.Choice<Sampling, InvalidWarp>(
        Lens<Wave, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly WaveParameter Wrap = new("wrap", new StateParameter<Wave>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Wave, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Wave> Kind { get; }
}

public sealed record Turbulence(
    NoiseBasis Basis, ShortSideExtent Scale, ShortSideOffset Horizontal, ShortSideOffset Vertical, Frequency Rate, Seed Seed, Timing Timing, Hold Hold, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Turbulence, TurbulenceParameter, InvalidWarp>, IPixelStage<Turbulence> {
    public static Turbulence Default { get; } = new(
        NoiseBasis.Default, ShortSideExtent.Create(1920f / 1080f), ShortSideOffset.Neutral, ShortSideOffset.Neutral, Frequency.Neutral,
        Seed.MinValue, Timing.Standard, Hold.MinValue, Sampling.Linear, WrapMode.Clamp);

    public static Option<PixelPass> Pass(Turbulence state, PassContext context) =>
        state.Horizontal == ShortSideOffset.Neutral && state.Vertical == ShortSideOffset.Neutral
            ? None
            : Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(state.Kernel(context), state.Sampling, state.Wrap).Apply));

    private Action<Span<Vector2>, Span<float>, PixelExtent> Kernel(PassContext context) {
        (Vector2 half, float side) = (new Vector2(context.Extent.Width, context.Extent.Height) / 2f, context.Extent.ShortSide);
        (Func<Vector4, uint, float> sampler, Vector2 amounts, float scale, float depth) = (Basis.Sampler, side * new Vector2(Horizontal, Vertical), Scale, Rate * Hold.Held(Timing.At(context)));
        uint field = CoordinateHash.Field(NoiseStream.Turbulence, Seed, 0u);
        (uint right, uint left, uint down, uint up) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u));
        return (points, _, _) => {
            foreach (ref Vector2 point in points) {
                Vector2 q = (point - half) / side;
                Vector4 at = new(q.X / scale, -q.Y / scale, depth, 0f);
                point += amounts * new Vector2(sampler(at, right) - sampler(at, left), sampler(at, down) - sampler(at, up));
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TurbulenceParameter : IStateParameter<Turbulence> {
    private static readonly (StateParameter<Turbulence> Clock, StateParameter<Turbulence> Pace) Time =
        Timing.Kinds(Lens<Turbulence, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));

    public static readonly TurbulenceParameter Basis = new("basis", new StateParameter<Turbulence>.Record<NoiseBasis>(
        Lens<Turbulence, NoiseBasis>.New(static state => state.Basis, static basis => state => state with { Basis = basis })));
    public static readonly TurbulenceParameter Scale = new("scale", new StateParameter<Turbulence>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Turbulence, ShortSideExtent>.New(static state => state.Scale, static scale => state => state with { Scale = scale }),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log }));
    public static readonly TurbulenceParameter Horizontal = new("horizontal", new StateParameter<Turbulence>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Turbulence, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Amounts.Shift));
    public static readonly TurbulenceParameter Vertical = new("vertical", new StateParameter<Turbulence>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Turbulence, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Amounts.Shift));
    public static readonly TurbulenceParameter Rate = new("rate", new StateParameter<Turbulence>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Turbulence, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (-2f, 2f) }));
    public static readonly TurbulenceParameter Seed = new("seed", new StateParameter<Turbulence>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Turbulence, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly TurbulenceParameter Clock = new("clock", Time.Clock);
    public static readonly TurbulenceParameter Pace = new("pace", Time.Pace);
    public static readonly TurbulenceParameter Hold = new("hold", new StateParameter<Turbulence>.Bounded<Hold, float, InvalidGenerator>(
        Lens<Turbulence, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly TurbulenceParameter Sampling = new("sampling", new StateParameter<Turbulence>.Choice<Sampling, InvalidWarp>(
        Lens<Turbulence, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly TurbulenceParameter Wrap = new("wrap", new StateParameter<Turbulence>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Turbulence, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Turbulence> Kind { get; }
}

public sealed record MapDisplacement(
    Option<ImageFile> Source, MapReading Reading, MapFit Fit, ShortSideExtent TileSize, ShortSideExtent Softness, ShortSideOffset Horizontal, ShortSideOffset Vertical,
    SignedAngle Rotation, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<MapDisplacement, MapDisplacementParameter, InvalidWarp>, IPixelStage<MapDisplacement> {
    public static MapDisplacement Default { get; } = new(
        None, MapReading.Gradient, MapFit.Stretch, ShortSideExtent.Create(1f), ShortSideExtent.Create(0.015f * 1920f / 1080f), ShortSideOffset.Neutral, ShortSideOffset.Neutral,
        SignedAngle.Neutral, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(MapDisplacement state, PassContext context) =>
        state.Reading.Plane.Match(
            Some: plane => state.Horizontal == ShortSideOffset.Neutral && state.Vertical == ShortSideOffset.Neutral
                ? None
                : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kernel(plane, context, frame, progress))),
            None: () => state.Source.Map(image => (PixelPass)new PixelPass.Frame(new CoordinateMap.Sampled(image.Frame, state.Sampling, state.Wrap).Apply)));

    private Fin<Unit> Kernel(Action<Mat, Mat, float, PassContext> plane, PassContext context, PixelFrame frame, IProgress<int> progress) {
        PixelFrame source = Source.Match(Some: image => Fitted(image, frame), None: () => frame);
        using Mat map = source.Header();
        using Mat offsets = new();
        plane(map, offsets, Softness.Pixels(context.Extent), context);
        GC.KeepAlive(source);
        (Vector2 amounts, Matrix3x2 turn) = (context.Extent.ShortSide * new Vector2(Horizontal, Vertical), Matrix3x2.CreateRotation(-Rotation));
        (Vector2 corner, int width) = (new Vector2(frame.Origin.X, frame.Origin.Y), frame.Size.Width);
        return new CoordinateMap.Analytic((points, _, _) => {
            ReadOnlySpan<Vector2> read = offsets.GetSpan<Vector2>();
            foreach (ref Vector2 point in points) {
                Vector2 local = point - corner;
                point += Vector2.Transform(amounts * read[((int)local.Y * width) + (int)local.X], turn);
            }
        }, Sampling, Wrap).Apply(frame, progress);
    }

    private PixelFrame Fitted(ImageFile image, PixelFrame frame) {
        (Vector2 corner, Vector2 scale) = (new Vector2(frame.Origin.X, frame.Origin.Y), Fit.Scale(this, image.Frame.Extent, frame.Extent));
        return new CoordinateMap.Analytic((points, _, _) => {
            foreach (ref Vector2 point in points) point = (point + corner) * scale;
        }, Sampling.Linear, Fit.Wrap).Resample(image.Frame, frame.Size);
    }
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
    public static readonly MapDisplacementParameter Fit = new("fit", new StateParameter<MapDisplacement>.Choice<MapFit, InvalidWarp>(
        Lens<MapDisplacement, MapFit>.New(static state => state.Fit, static fit => state => state with { Fit = fit })));
    public static readonly MapDisplacementParameter TileSize = new("tile-size", new StateParameter<MapDisplacement>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<MapDisplacement, ShortSideExtent>.New(static state => state.TileSize, static size => state => state with { TileSize = size }),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.05f, 2f), Scale = TrackScale.Log }));
    public static readonly MapDisplacementParameter Softness = new("softness", new StateParameter<MapDisplacement>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<MapDisplacement, ShortSideExtent>.New(static state => state.Softness, static softness => state => state with { Softness = softness }),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.001f, 0.1f) }));
    public static readonly MapDisplacementParameter Horizontal = new("horizontal", new StateParameter<MapDisplacement>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<MapDisplacement, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Amounts.Shift));
    public static readonly MapDisplacementParameter Vertical = new("vertical", new StateParameter<MapDisplacement>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<MapDisplacement, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Amounts.Shift));
    public static readonly MapDisplacementParameter Rotation = new("rotation", new StateParameter<MapDisplacement>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<MapDisplacement, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation));
    public static readonly MapDisplacementParameter Sampling = new("sampling", new StateParameter<MapDisplacement>.Choice<Sampling, InvalidWarp>(
        Lens<MapDisplacement, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly MapDisplacementParameter Wrap = new("wrap", new StateParameter<MapDisplacement>.Choice<WrapMode, InvalidPixelValue>(
        Lens<MapDisplacement, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<MapDisplacement> Kind { get; }
}

public sealed record Refraction(
    RefractionField Field, ShortSideExtent Scale, ShortSideOffset Strength, ShortSideExtent Smoothness, ShortSideLength Blur, Likelihood Density, WarpShare Trails,
    SignedAngle Direction, Drift Fall, Seed Seed, Timing Timing, Hold Hold, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Refraction, RefractionParameter, InvalidWarp>, IPixelStage<Refraction> {
    public static Refraction Default { get; } = new(
        RefractionField.Raindrops, ShortSideExtent.Create(1920f / 3.8f / 1080f), ShortSideOffset.Neutral, ShortSideExtent.Create(3f / 1080f), ShortSideLength.Create(14.6f / 1080f),
        Likelihood.Create(0.5f), WarpShare.Create(0.3f), SignedAngle.Down, Drift.Neutral, Seed.MinValue, Timing.Standard, Hold.MinValue, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(Refraction state, PassContext context) =>
        state.Strength == ShortSideOffset.Neutral && state.Blur == ShortSideLength.Neutral
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kernel(context, frame, progress)));

    private Fin<Unit> Kernel(PassContext context, PixelFrame frame, IProgress<int> progress) {
        PixelExtent extent = context.Extent;
        (Vector2 half, float side) = (new Vector2(extent.Width, extent.Height) / 2f, extent.ShortSide);
        Func<Vector2, float> height = Field.Height(this, Timing.At(context));
        PixelFrame heights = new(Point.Empty, extent, extent, static block => System.Array.Clear(block));
        return new PixelPass.Pointwise((row, column, line) => {
            for (int x = 0; x < row.Length; x++) row[x].X = height((new Vector2(column + x + 0.5f, extent.Height - line - 0.5f) - half) / side);
        }).Run(heights, progress).Bind(_ => {
            using Mat header = heights.Header();
            using Mat level = new();
            CvInvoke.ExtractChannel(header, level, 0);
            float sigma = Smoothness.Pixels(extent);
            int taps = (2 * (int)float.Ceiling(3f * sigma)) + 1;
            CvInvoke.GaussianBlur(level, level, new Size(taps, taps), sigma, sigma, BorderType.Replicate);
            using Mat across = new();
            using Mat down = new();
            CvInvoke.Sobel(level, across, DepthType.Cv32F, 1, 0, 3, 1d / 8d, 0d, BorderType.Replicate);
            CvInvoke.Sobel(level, down, DepthType.Cv32F, 0, 1, 3, 1d / 8d, 0d, BorderType.Replicate);
            using VectorOfMat pair = new(across, down);
            using Mat slope = new();
            CvInvoke.Merge(pair, slope);
            float push = Strength.Pixels(extent) * Scale.Pixels(extent);
            void Bend(Span<Vector2> points) {
                ReadOnlySpan<Vector2> read = slope.GetSpan<Vector2>();
                foreach (ref Vector2 point in points) point -= push * read[((int)point.Y * extent.Width) + (int)point.X];
            }
            PixelFrame refracted = new(frame.Origin, frame.Size, frame.Extent, block => frame.Block.CopyTo(block));
            if (Blur != ShortSideLength.Neutral) {
                using Mat image = refracted.Header();
                float blur = Blur.Pixels(extent);
                int span = (2 * (int)float.Ceiling(3f * blur)) + 1;
                CvInvoke.GaussianBlur(image, image, new Size(span, span), blur, blur, BorderType.Replicate);
            }
            return Field.CoverageRamp.Match(
                Some: ramp => new CoordinateMap.Analytic((points, coverage, _) => {
                    ReadOnlySpan<float> read = level.GetSpan<float>();
                    for (int i = 0; i < points.Length; i++) coverage[i] *= float.Min(1f, read[((int)points[i].Y * extent.Width) + (int)points[i].X] / ramp);
                    Bend(points);
                }, Sampling, Wrap).Apply(refracted, progress).Map(_ => Blend.Composite(frame, refracted, BlendingMode.Mix, Mix.Full, [])),
                None: () => new CoordinateMap.Analytic((points, _, _) => Bend(points), Sampling, Wrap).Apply(refracted, progress).Map(_ => refracted))
                .Map(result => {
                    result.View.CopyTo(frame.View);
                    return unit;
                });
        });
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefractionParameter : IStateParameter<Refraction> {
    private static readonly (StateParameter<Refraction> Clock, StateParameter<Refraction> Pace) Time =
        Timing.Kinds(Lens<Refraction, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly RefractionParameter Field = new("field", new StateParameter<Refraction>.Choice<RefractionField, InvalidWarp>(
        Lens<Refraction, RefractionField>.New(static state => state.Field, static field => state => state with { Field = field })));
    public static readonly RefractionParameter Scale = new("scale", new StateParameter<Refraction>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Refraction, ShortSideExtent>.New(static state => state.Scale, static scale => state => state with { Scale = scale }),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.005f, 1f), Scale = TrackScale.Log }));
    public static readonly RefractionParameter Strength = new("strength", new StateParameter<Refraction>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<Refraction, ShortSideOffset>.New(static state => state.Strength, static strength => state => state with { Strength = strength }), Amounts.Shift));
    public static readonly RefractionParameter Smoothness = new("smoothness", new StateParameter<Refraction>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Refraction, ShortSideExtent>.New(static state => state.Smoothness, static smoothness => state => state with { Smoothness = smoothness }),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.001f, 0.05f), Scale = TrackScale.Log }));
    public static readonly RefractionParameter Blur = new("blur", new StateParameter<Refraction>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Refraction, ShortSideLength>.New(static state => state.Blur, static blur => state => state with { Blur = blur }),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.05f) }));
    public static readonly RefractionParameter Density = new("density", new StateParameter<Refraction>.Bounded<Likelihood, float, InvalidStylize>(
        Lens<Refraction, Likelihood>.New(static state => state.Density, static density => state => state with { Density = density }), new()));
    public static readonly RefractionParameter Trails = new("trails", new StateParameter<Refraction>.Bounded<WarpShare, float, InvalidWarp>(
        Lens<Refraction, WarpShare>.New(static state => state.Trails, static trails => state => state with { Trails = trails }), new()));
    public static readonly RefractionParameter Direction = new("direction", new StateParameter<Refraction>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Refraction, SignedAngle>.New(static state => state.Direction, static direction => state => state with { Direction = direction }), SignedAngle.Presentation));
    public static readonly RefractionParameter Fall = new("fall", new StateParameter<Refraction>.Bounded<Drift, float, InvalidGenerator>(
        Lens<Refraction, Drift>.New(static state => state.Fall, static fall => state => state with { Fall = fall }),
        Drift.Presentation with { Soft = (-1f, 1f) }));
    public static readonly RefractionParameter Seed = new("seed", new StateParameter<Refraction>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Refraction, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly RefractionParameter Clock = new("clock", Time.Clock);
    public static readonly RefractionParameter Pace = new("pace", Time.Pace);
    public static readonly RefractionParameter Hold = new("hold", new StateParameter<Refraction>.Bounded<Hold, float, InvalidGenerator>(
        Lens<Refraction, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly RefractionParameter Sampling = new("sampling", new StateParameter<Refraction>.Choice<Sampling, InvalidWarp>(
        Lens<Refraction, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly RefractionParameter Wrap = new("wrap", new StateParameter<Refraction>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Refraction, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Refraction> Kind { get; }
}
