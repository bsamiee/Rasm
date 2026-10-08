using System.Numerics;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Generators;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum NoiseStream {
    FilmGrain, SensorNoise, AnalogVideo, Glitch, Mosaic, GlyphCells, FilmDamage, FilmFlicker, Wave, Turbulence, Refraction, Shake,
    NoiseTexture, Fire, Fog, Starfield, ParticleField, Shape, Lightning, Grid, LensFlare, LightRays, LensDirt,
}

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class ReferenceFrame {
    public const float GreaterSide = 1920f / 1080f;
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Seed : IMinMaxValue<Seed> {
    public static Seed MinValue { get; } = new(0);
    public static Seed MaxValue { get; } = new(int.MaxValue);
    public static Presentation<Seed, int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Clock : IMinMaxValue<Clock> {
    public static Clock MinValue { get; } = new(0f);
    public static Clock MaxValue { get; } = new(1 << 14);
    public static Presentation<Clock, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    public uint Term => BitConverter.SingleToUInt32Bits(_value);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Pace : IMinMaxValue<Pace> {
    public static Pace MinValue { get; } = new(0f);
    public static Pace MaxValue { get; } = new(float.MaxValue);
    public static Pace Standard { get; } = new(1f);
    public static Presentation<Pace, float> Presentation { get; } = new() { Soft = (0f, 4f) };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Hold : IMinMaxValue<Hold> {
    public static Hold MinValue { get; } = new(0.001f);
    public static Hold MaxValue { get; } = new(8f);
    public static Hold Bolt { get; } = new(0.05f);
    public static Hold Damage { get; } = new(6f / 24f);
    public static Hold Glyphs { get; } = new(6f / 24f);
    public static Presentation<Hold, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second), Scale = TrackScale.Log };

    public uint Period(Clock clock) => (uint)Math.Floor(clock / (double)_value * (1d + (1d / (1 << 22))));

    public float Held(Clock clock) => Period(clock) * _value;

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Instant", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ShutterTime : IMinMaxValue<ShutterTime> {
    public static ShutterTime MinValue => Instant;
    public static ShutterTime MaxValue { get; } = new(0.5f);
    public static ShutterTime Film { get; } = new(0.5f / 24f);
    public static Presentation<ShutterTime, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Frequency : IMinMaxValue<Frequency> {
    public static Frequency MinValue { get; } = new(-60f);
    public static Frequency MaxValue { get; } = new(60f);
    public static Frequency Wiggle { get; } = new(5f);
    public static Frequency Flicker { get; } = new(5f * 0.5f * 24f);
    public static Frequency Weave { get; } = new(5f * 0.05f * 0.5f * 24f);
    public static Presentation<Frequency, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(FrequencyUnit.Hertz), Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Drift : IMinMaxValue<Drift> {
    public static Drift MinValue { get; } = new(-10f);
    public static Drift MaxValue { get; } = new(10f);
    public static Drift Rise { get; } = new(5f);
    public static Presentation<Drift, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioChangeRateUnit.DecimalFractionPerSecond), Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Spin : IMinMaxValue<Spin> {
    public static Spin MinValue { get; } = new(-2f * float.Tau);
    public static Spin MaxValue { get; } = new(2f * float.Tau);
    public static Presentation<Spin, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RotationalSpeedUnit.RadianPerSecond), Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct FractalDetail : IMinMaxValue<FractalDetail> {
    public static FractalDetail MinValue { get; } = new(0f);
    public static FractalDetail MaxValue { get; } = new(15f);
    public static FractalDetail Standard { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct FractalLacunarity : IMinMaxValue<FractalLacunarity> {
    public static FractalLacunarity MinValue { get; } = new(0f);
    public static FractalLacunarity MaxValue { get; } = new(MathF.Pow(NoiseFunctions.Ceiling / NoiseFunctions.Reach, 1f / FractalDetail.MaxValue));
    public static FractalLacunarity Standard { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct FractalOffset : IMinMaxValue<FractalOffset> {
    public static FractalOffset MaxValue { get; } = new(MathF.Pow(NoiseFunctions.Ceiling, 1f / (FractalDetail.MaxValue + 1f)) - 1f - NoiseFunctions.Peak);
    public static FractalOffset MinValue { get; } = new(-MaxValue._value);
    public static Presentation<FractalOffset, float> Presentation { get; } = new() { Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct FractalGain : IMinMaxValue<FractalGain> {
    public static FractalGain MinValue { get; } = new(0f);
    public static FractalGain MaxValue { get; } = new(1000f);
    public static FractalGain Standard { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct NoiseDistortion : IMinMaxValue<NoiseDistortion> {
    public static NoiseDistortion MinValue { get; } = new(-1000f);
    public static NoiseDistortion MaxValue { get; } = new(1000f);
    public static Presentation<NoiseDistortion, float> Presentation { get; } = new() { Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct MinkowskiExponent : IMinMaxValue<MinkowskiExponent> {
    public static MinkowskiExponent MinValue { get; } = new(1f / 32f);
    public static MinkowskiExponent MaxValue { get; } = new(32f);
    public static MinkowskiExponent Standard { get; } = new(0.5f);
    public static Presentation<MinkowskiExponent, float> Presentation { get; } = new() { Scale = TrackScale.Log };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct GaborFrequency : IMinMaxValue<GaborFrequency> {
    public static GaborFrequency MinValue { get; } = new(0.001f);
    public static GaborFrequency MaxValue { get; } = new(1000f);
    public static GaborFrequency Standard { get; } = new(2f);
    public static Presentation<GaborFrequency, float> Presentation { get; } = new() { Scale = TrackScale.Log };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct GaborOrientation : IMinMaxValue<GaborOrientation> {
    public static GaborOrientation MinValue { get; } = new(0f);
    public static GaborOrientation MaxValue { get; } = new(float.Pi);
    public static GaborOrientation Standard { get; } = new(float.Pi / 4f);
    public static Presentation<GaborOrientation, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct MagicDepth : IMinMaxValue<MagicDepth> {
    public static MagicDepth MinValue { get; } = new(0);
    public static MagicDepth MaxValue { get; } = new(10);
    public static MagicDepth Standard { get; } = new(2);
    public static Presentation<MagicDepth, int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct WaveDetailScale : IMinMaxValue<WaveDetailScale> {
    public static WaveDetailScale MinValue { get; } = new(0f);
    public static WaveDetailScale MaxValue { get; } = new(1000f);
    public static WaveDetailScale Standard { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CellMetric {
    public static readonly CellMetric Euclidean = new("euclidean", static (delta, _) => delta.Length());
    public static readonly CellMetric Manhattan = new("manhattan", static (delta, _) => Vector4.Sum(Vector4.Abs(delta)));
    public static readonly CellMetric Chebyshev = new("chebyshev", static (delta, _) =>
        MathF.Max(MathF.Max(MathF.Abs(delta.X), MathF.Abs(delta.Y)), MathF.Max(MathF.Abs(delta.Z), MathF.Abs(delta.W))));
    public static readonly CellMetric Minkowski = new("minkowski", static (delta, exponent) =>
        MathF.Pow(Vector4.Sum(Vector4.Exp(exponent * Vector4.Log(Vector4.Abs(delta)))), 1f / exponent));

    [UseDelegateFromConstructor]
    public partial float Length(Vector4 delta, float exponent);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveForm {
    public static readonly WaveForm Bands = new("bands", static point => point.X);
    public static readonly WaveForm Rings = new("rings", static point => point.AsVector2().Length());

    [UseDelegateFromConstructor]
    public partial float Distance(Vector4 point);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveProfile {
    public static readonly WaveProfile Sine = new("sine", static argument => 0.5f + (0.5f * MathF.Sin(argument - (MathF.PI / 2f))));
    public static readonly WaveProfile Saw = new("saw", static argument => (argument / MathF.Tau) - MathF.Floor(argument / MathF.Tau));
    public static readonly WaveProfile Triangle = new("triangle", static argument =>
        2f * MathF.Abs((argument / MathF.Tau) - MathF.Floor((argument / MathF.Tau) + 0.5f)));
    public static readonly WaveProfile Square = new("square", static argument =>
        MathF.Floor(2f * ((argument / MathF.Tau) - MathF.Floor(argument / MathF.Tau))));

    [UseDelegateFromConstructor]
    public partial float Level(float argument);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BasisKind {
    public static readonly BasisKind Fbm = new("fbm");
    public static readonly BasisKind Multifractal = new("multifractal");
    public static readonly BasisKind HeteroTerrain = new("hetero-terrain");
    public static readonly BasisKind HybridMultifractal = new("hybrid-multifractal");
    public static readonly BasisKind RidgedMultifractal = new("ridged-multifractal");
    public static readonly BasisKind Cellular = new("cellular");
    public static readonly BasisKind EdgeDistance = new("edge-distance");
    public static readonly BasisKind Gabor = new("gabor");
    public static readonly BasisKind Magic = new("magic");
    public static readonly BasisKind Wave = new("wave");
    public static readonly BasisKind White = new("white");
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CellFeatureKind {
    public static readonly CellFeatureKind F1 = new("f1", static (noise, point, field) => noise.Dimensions.F1(point, noise.Octaves, noise.Cells, field));
    public static readonly CellFeatureKind F2 = new("f2", static (noise, point, field) => noise.Dimensions.F2(point, noise.Octaves, noise.Cells, field));
    public static readonly CellFeatureKind SmoothF1 = new("smooth-f1", static (noise, point, field) => noise.Dimensions.SmoothF1(point, noise.Octaves, noise.Cells, noise.Smoothness, field));

    [UseDelegateFromConstructor]
    internal partial CellFeature Sample(CellularNoise noise, Vector4 point, uint field);
}

internal sealed record Neighborhood(int Rank, Vector4 Live, Arr<Vector4> Corners, Arr<Vector4> Near, Arr<Vector4> Wide) {
    public static Neighborhood Of(int rank) =>
        Vector4.Clamp(Vector4.Create(rank) - new Vector4(0f, 1f, 2f, 3f), Vector4.Zero, Vector4.One) switch {
            var live => new(rank, live, Offsets(rank, live, 0, 1), Offsets(rank, live, -1, 1), Offsets(rank, live, -2, 2)),
        };

    private static Arr<Vector4> Offsets(int rank, Vector4 live, int low, int high) =>
        (high - low + 1) switch {
            var span => [.. Enumerable.Range(0, (int)Math.Pow(span, rank)).Select(index =>
                (new Vector4(index % span, index / span % span, index / (span * span) % span, index / (span * span * span) % span) + Vector4.Create(low)) * live)],
        };
}

internal sealed record GaborKernel(Neighborhood Cells, float Deviation, Func<float, float, Vector4, Vector4> Orient) {
    internal const int Impulses = 8;
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseDimensions {
    private static readonly GaborKernel Planar = new(Neighborhood.Of(2), MathF.Sqrt(GaborKernel.Impulses * 0.5f * 0.25f), static (angle, isotropy, draw) =>
        MathF.SinCos(angle + ((draw.X - 0.5f) * float.Pi * isotropy)) switch { var (sin, cos) => new Vector4(cos, sin, 0f, 0f) });
    private static readonly GaborKernel Spherical = new(Neighborhood.Of(3), MathF.Sqrt(GaborKernel.Impulses * 0.5f / (4f * MathF.Sqrt(2f))), static (angle, isotropy, draw) =>
        (MathF.SinCos((float.Pi / 2f) + (draw.X * float.Pi * isotropy)), MathF.SinCos(angle + (draw.Y * float.Pi * isotropy))) switch {
            var ((sinInclination, cosInclination), (sinAzimuth, cosAzimuth)) => new Vector4(sinInclination * cosAzimuth, sinInclination * sinAzimuth, cosInclination, 0f),
        });

    public static readonly NoiseDimensions One = new("1d", Neighborhood.Of(1), 0.25f, Planar, static (word, offset) =>
        (word >> 28) switch { var h => (1u + (h & 7u)) * ((h & 8u) != 0u ? -offset.X : offset.X) });
    public static readonly NoiseDimensions Two = new("2d", Planar.Cells, 0.6616f, Planar, static (word, offset) =>
        (word >> 29) switch { var h => (((h & 1u) != 0u ? -1f : 1f) * (h < 4u ? offset.X : offset.Y)) + (((h & 2u) != 0u ? -2f : 2f) * (h < 4u ? offset.Y : offset.X)) });
    public static readonly NoiseDimensions Three = new("3d", Spherical.Cells, 0.982f, Spherical, static (word, offset) =>
        (word >> 28) switch {
            var h => (((h & 1u) != 0u ? -1f : 1f) * (h < 8u ? offset.X : offset.Y)) + (((h & 2u) != 0u ? -1f : 1f) * (h < 4u ? offset.Y : h is 12u or 14u ? offset.X : offset.Z)),
        });
    public static readonly NoiseDimensions Four = new("4d", Neighborhood.Of(4), 0.8344f, Spherical, static (word, offset) =>
        (word >> 27) switch {
            var h => (((h & 1u) != 0u ? -1f : 1f) * (h < 24u ? offset.X : offset.Y)) + (((h & 2u) != 0u ? -1f : 1f) * (h < 16u ? offset.Y : offset.Z))
                + (((h & 4u) != 0u ? -1f : 1f) * (h < 8u ? offset.Z : offset.W)),
        });

    internal Neighborhood Cells { get; }
    internal float Scale { get; }
    internal GaborKernel Gabor { get; }

    [UseDelegateFromConstructor]
    internal partial float Slope(uint word, Vector4 offset);
}

public sealed record Timing(Clock Clock, Pace Pace) {
    public static Timing Standard { get; } = new(Clock.MinValue, Pace.Standard);
    public static Timing Still { get; } = new(Clock.MinValue, Pace.MinValue);

    public Clock At(PassContext context) =>
        context.Time.Map(time => new Clock((float)((Clock + ((double)Pace * time.TotalSeconds)) % (float)Clock.MaxValue))).IfNone(Clock);

    internal static (StateParameter<TRecord> Clock, StateParameter<TRecord> Pace) Kinds<TRecord>(Lens<TRecord, Timing> timing) =>
        (new StateParameter<TRecord>.Bounded<Clock, float, InvalidGenerator>(
             lens(timing, Lens<Timing, Clock>.New(static at => at.Clock, static clock => at => at with { Clock = clock })), Clock.Presentation),
         new StateParameter<TRecord>.Bounded<Pace, float, InvalidGenerator>(
             lens(timing, Lens<Timing, Pace>.New(static at => at.Pace, static pace => at => at with { Pace = pace })), Pace.Presentation));
}

public sealed record Octaves(FractalDetail Detail, AxisFraction Roughness, FractalLacunarity Lacunarity) : IStateRecord<Octaves, OctavesParameter, InvalidGenerator> {
    public static Octaves Default { get; } = new(FractalDetail.Standard, AxisFraction.Half, FractalLacunarity.Standard);
    public static Octaves Plain { get; } = Default with { Detail = FractalDetail.MinValue };

    internal int Cells => Roughness == AxisFraction.MinValue ? 1 : (int)MathF.Ceiling(Detail) + 1;
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OctavesParameter : IStateParameter<Octaves> {
    public static readonly OctavesParameter Detail = new("detail", new StateParameter<Octaves>.Bounded<FractalDetail, float, InvalidGenerator>(
        Lens<Octaves, FractalDetail>.New(static state => state.Detail, static value => state => state with { Detail = value }), new()));
    public static readonly OctavesParameter Roughness = new("roughness", new StateParameter<Octaves>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Octaves, AxisFraction>.New(static state => state.Roughness, static value => state => state with { Roughness = value }), new()));
    public static readonly OctavesParameter Lacunarity = new("lacunarity", new StateParameter<Octaves>.Bounded<FractalLacunarity, float, InvalidGenerator>(
        Lens<Octaves, FractalLacunarity>.New(static state => state.Lacunarity, static value => state => state with { Lacunarity = value }), new()));

    public StateParameter<Octaves> Kind { get; }
}

public sealed record Cellular(CellMetric Metric, MinkowskiExponent Exponent, AxisFraction Randomness) : IStateRecord<Cellular, CellularParameter, InvalidGenerator> {
    public static Cellular Default { get; } = new(CellMetric.Euclidean, MinkowskiExponent.Standard, AxisFraction.MaxValue);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CellularParameter : IStateParameter<Cellular> {
    public static readonly CellularParameter Metric = new("metric", new StateParameter<Cellular>.Choice<CellMetric, InvalidGenerator>(
        Lens<Cellular, CellMetric>.New(static state => state.Metric, static value => state => state with { Metric = value })));
    public static readonly CellularParameter Exponent = new("exponent", new StateParameter<Cellular>.Bounded<MinkowskiExponent, float, InvalidGenerator>(
        Lens<Cellular, MinkowskiExponent>.New(static state => state.Exponent, static value => state => state with { Exponent = value }), MinkowskiExponent.Presentation));
    public static readonly CellularParameter Randomness = new("randomness", new StateParameter<Cellular>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Cellular, AxisFraction>.New(static state => state.Randomness, static value => state => state with { Randomness = value }), new()));

    public StateParameter<Cellular> Kind { get; }
}

public sealed record NoiseBasis(
    BasisKind Kind, NoiseDimensions Dimensions, Octaves Octaves, NoiseDistortion Distortion, Cellular Cellular, CellFeatureKind Feature,
    Option<FractalOffset> FractalOffset, Option<FractalGain> FractalGain, Option<AxisFraction> CellSmoothness,
    Option<GaborFrequency> GaborFrequency, Option<AxisFraction> GaborAnisotropy, Option<GaborOrientation> GaborOrientation,
    Option<MagicDepth> MagicDepth, Option<WaveForm> WaveForm, Option<WaveProfile> WaveProfile, Option<SignedAngle> WavePhase,
    Option<WaveDetailScale> WaveDetailScale) : IStateRecord<NoiseBasis, NoiseBasisParameter, InvalidGenerator> {
    public static NoiseBasis Default { get; } = new(
        BasisKind.Fbm, NoiseDimensions.Three, Octaves.Default, NoiseDistortion.Neutral, Cellular.Default, CellFeatureKind.F1, None, None, None, None, None, None, None, None, None, None, None);
    public static NoiseBasis Plain { get; } = Default with { Octaves = Octaves.Plain };

    internal Func<Vector4, uint, float> Sampler =>
        Kind.Switch(
            this,
            fbm: static b => b.Distorted((point, hash) => 0.5f + (0.5f * b.Dimensions.Fbm(point, b.Octaves, hash))),
            multifractal: static b => b.Distorted((point, hash) => b.Dimensions.Multifractal(point, b.Octaves, hash)),
            heteroTerrain: static b => b.FractalOffset.IfNone(Generators.FractalOffset.Neutral) switch {
                var offset => b.Distorted((point, hash) => b.Dimensions.HeteroTerrain(point, b.Octaves, offset, hash)),
            },
            hybridMultifractal: static b => (b.FractalOffset.IfNone(Generators.FractalOffset.Neutral), b.FractalGain.IfNone(Generators.FractalGain.Standard)) switch {
                var (offset, gain) => b.Distorted((point, hash) => b.Dimensions.HybridMultifractal(point, b.Octaves, offset, gain, hash)),
            },
            ridgedMultifractal: static b => (b.FractalOffset.IfNone(Generators.FractalOffset.Neutral), b.FractalGain.IfNone(Generators.FractalGain.Standard)) switch {
                var (offset, gain) => b.Distorted((point, hash) => b.Dimensions.RidgedMultifractal(point, b.Octaves, offset, gain, hash)),
            },
            cellular: static b => new CellularNoise(b.Dimensions, b.Octaves, b.Cellular, b.Feature, b.CellSmoothness.IfNone(AxisFraction.MaxValue)) switch {
                var noise => (point, hash) => noise.Feature.Sample(noise, point, hash).Distance,
            },
            edgeDistance: static b => (point, hash) => b.Dimensions.EdgeDistance(point, b.Octaves, b.Cellular.Randomness, hash),
            gabor: static b => (b.GaborFrequency.IfNone(Generators.GaborFrequency.Standard), b.GaborAnisotropy.IfNone(AxisFraction.MaxValue),
                b.GaborOrientation.IfNone(Generators.GaborOrientation.Standard)) switch {
                    var (frequency, anisotropy, orientation) =>
                        (point, hash) => 0.5f + (0.5f * b.Dimensions.Gabor(point, frequency, anisotropy, orientation, hash).Y),
                },
            magic: static b => b.MagicDepth.IfNone(Generators.MagicDepth.Standard) switch {
                var depth => (point, _) => Vector3.Sum(NoiseFunctions.Magic(point, depth, b.Distortion)) / 3f,
            },
            wave: static b => (b.WaveForm.IfNone(Generators.WaveForm.Bands), b.WaveProfile.IfNone(Generators.WaveProfile.Sine),
                (float)b.WavePhase.IfNone(SignedAngle.Neutral), b.WaveDetailScale.IfNone(Generators.WaveDetailScale.Standard)) switch {
                    var (form, profile, phase, detailScale) => (point, hash) =>
                        b.Dimensions.Wave(point, form, profile, phase, b.Distortion, b.Octaves, detailScale, hash),
                },
            white: static b => (point, hash) => NoiseFunctions.White(point * b.Dimensions.Cells.Live, hash).X);

    private Func<Vector4, uint, float> Distorted(Func<Vector4, uint, float> sum) =>
        (point, field) => sum(Dimensions.Distort(point, Distortion, CoordinateHash.Branch(field, 0u)), CoordinateHash.Branch(field, 1u));
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseBasisParameter : IStateParameter<NoiseBasis> {
    public static readonly NoiseBasisParameter Basis = new("kind", new StateParameter<NoiseBasis>.Choice<BasisKind, InvalidGenerator>(Lens<NoiseBasis, BasisKind>.New(static b => b.Kind, static kind => b => b with { Kind = kind })));
    public static readonly NoiseBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis>.Choice<NoiseDimensions, InvalidGenerator>(
        Lens<NoiseBasis, NoiseDimensions>.New(static b => b.Dimensions, static dimensions => b => b with { Dimensions = dimensions })));
    public static readonly NoiseBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis>.Record<Octaves>(
        Lens<NoiseBasis, Octaves>.New(static state => state.Octaves, static value => state => state with { Octaves = value })));
    public static readonly NoiseBasisParameter Distortion = new("distortion", new StateParameter<NoiseBasis>.Bounded<NoiseDistortion, float, InvalidGenerator>(
        Lens<NoiseBasis, NoiseDistortion>.New(static b => b.Distortion, static distortion => b => b with { Distortion = distortion }), NoiseDistortion.Presentation));
    public static readonly NoiseBasisParameter Cellular = new("cellular", new StateParameter<NoiseBasis>.Record<Cellular>(
        Lens<NoiseBasis, Cellular>.New(static state => state.Cellular, static value => state => state with { Cellular = value })));
    public static readonly NoiseBasisParameter Feature = new("feature", new StateParameter<NoiseBasis>.Choice<CellFeatureKind, InvalidGenerator>(
        Lens<NoiseBasis, CellFeatureKind>.New(static state => state.Feature, static value => state => state with { Feature = value })));
    public static readonly NoiseBasisParameter Offset = new("offset", new StateParameter<NoiseBasis>.OptionalBounded<FractalOffset, float, InvalidGenerator>(
        Lens<NoiseBasis, Option<FractalOffset>>.New(static b => b.FractalOffset, static offset => b => b with { FractalOffset = offset }),
        FractalOffset.Presentation));
    public static readonly NoiseBasisParameter Gain = new("gain", new StateParameter<NoiseBasis>.OptionalBounded<FractalGain, float, InvalidGenerator>(
        Lens<NoiseBasis, Option<FractalGain>>.New(static b => b.FractalGain, static gain => b => b with { FractalGain = gain }), new()));
    public static readonly NoiseBasisParameter Smoothness = new("smoothness", new StateParameter<NoiseBasis>.OptionalBounded<AxisFraction, float, InvalidGrade>(
        Lens<NoiseBasis, Option<AxisFraction>>.New(static b => b.CellSmoothness, static smoothness => b => b with { CellSmoothness = smoothness }), new()));
    public static readonly NoiseBasisParameter Frequency = new("frequency", new StateParameter<NoiseBasis>.OptionalBounded<GaborFrequency, float, InvalidGenerator>(
        Lens<NoiseBasis, Option<GaborFrequency>>.New(static b => b.GaborFrequency, static frequency => b => b with { GaborFrequency = frequency }),
        GaborFrequency.Presentation));
    public static readonly NoiseBasisParameter Anisotropy = new("anisotropy", new StateParameter<NoiseBasis>.OptionalBounded<AxisFraction, float, InvalidGrade>(
        Lens<NoiseBasis, Option<AxisFraction>>.New(static b => b.GaborAnisotropy, static anisotropy => b => b with { GaborAnisotropy = anisotropy }), new()));
    public static readonly NoiseBasisParameter Orientation = new("orientation", new StateParameter<NoiseBasis>.OptionalBounded<GaborOrientation, float, InvalidGenerator>(
        Lens<NoiseBasis, Option<GaborOrientation>>.New(static b => b.GaborOrientation, static orientation => b => b with { GaborOrientation = orientation }),
        GaborOrientation.Presentation));
    public static readonly NoiseBasisParameter Depth = new("depth", new StateParameter<NoiseBasis>.OptionalBounded<MagicDepth, int, InvalidGenerator>(
        Lens<NoiseBasis, Option<MagicDepth>>.New(static b => b.MagicDepth, static depth => b => b with { MagicDepth = depth }), MagicDepth.Presentation));
    public static readonly NoiseBasisParameter Form = new("form", new StateParameter<NoiseBasis>.OptionalChoice<WaveForm, InvalidGenerator>(
        Lens<NoiseBasis, Option<WaveForm>>.New(static b => b.WaveForm, static form => b => b with { WaveForm = form })));
    public static readonly NoiseBasisParameter Profile = new("profile", new StateParameter<NoiseBasis>.OptionalChoice<WaveProfile, InvalidGenerator>(
        Lens<NoiseBasis, Option<WaveProfile>>.New(static b => b.WaveProfile, static profile => b => b with { WaveProfile = profile })));
    public static readonly NoiseBasisParameter Phase = new("phase", new StateParameter<NoiseBasis>.OptionalBounded<SignedAngle, float, InvalidPixelValue>(
        Lens<NoiseBasis, Option<SignedAngle>>.New(static b => b.WavePhase, static phase => b => b with { WavePhase = phase }),
        new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = 0f }));
    public static readonly NoiseBasisParameter DetailScale = new("detail-scale", new StateParameter<NoiseBasis>.OptionalBounded<WaveDetailScale, float, InvalidGenerator>(
        Lens<NoiseBasis, Option<WaveDetailScale>>.New(static b => b.WaveDetailScale, static scale => b => b with { WaveDetailScale = scale }), new()));

    public StateParameter<NoiseBasis> Kind { get; }
}

public sealed record CellularNoise(NoiseDimensions Dimensions, Octaves Octaves, Cellular Cells, CellFeatureKind Feature, AxisFraction Smoothness)
    : IStateRecord<CellularNoise, CellularNoiseParameter, InvalidGenerator> {
    public static CellularNoise Default { get; } = new(NoiseDimensions.Three, Octaves.Plain, Cellular.Default, CellFeatureKind.F1, AxisFraction.MaxValue);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CellularNoiseParameter : IStateParameter<CellularNoise> {
    public static readonly CellularNoiseParameter Dimensions = new("dimensions", new StateParameter<CellularNoise>.Choice<NoiseDimensions, InvalidGenerator>(
        Lens<CellularNoise, NoiseDimensions>.New(static state => state.Dimensions, static value => state => state with { Dimensions = value })));
    public static readonly CellularNoiseParameter Octaves = new("octaves", new StateParameter<CellularNoise>.Record<Octaves>(
        Lens<CellularNoise, Octaves>.New(static state => state.Octaves, static value => state => state with { Octaves = value })));
    public static readonly CellularNoiseParameter Cells = new("cells", new StateParameter<CellularNoise>.Record<Cellular>(
        Lens<CellularNoise, Cellular>.New(static state => state.Cells, static value => state => state with { Cells = value })));
    public static readonly CellularNoiseParameter Feature = new("feature", new StateParameter<CellularNoise>.Choice<CellFeatureKind, InvalidGenerator>(
        Lens<CellularNoise, CellFeatureKind>.New(static state => state.Feature, static value => state => state with { Feature = value })));
    public static readonly CellularNoiseParameter Smoothness = new("smoothness", new StateParameter<CellularNoise>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<CellularNoise, AxisFraction>.New(static state => state.Smoothness, static value => state => state with { Smoothness = value }), new()));

    public StateParameter<CellularNoise> Kind { get; }
}

internal readonly record struct CellFeature(float Distance, Vector4 Site);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Easing {
    public static float Saturate(float value) => value > 0f ? float.Min(value, 1f) : 0f;

    public static float SmoothStep(float t) => t * t * (3f - (2f * t));

    public static float SmootherStep(float t) => t * t * t * ((t * ((6f * t) - 15f)) + 10f);
}

internal static class CoordinateHash {
    public static (uint X, uint Y, uint Z) Pcg3d(uint x, uint y, uint z) {
        const uint multiplier = 1664525u;
        const uint increment = 1013904223u;
        unchecked {
            (x, y, z) = ((x * multiplier) + increment, (y * multiplier) + increment, (z * multiplier) + increment);
            x += y * z;
            y += z * x;
            z += x * y;
            (x, y, z) = (x ^ (x >> 16), y ^ (y >> 16), z ^ (z >> 16));
            x += y * z;
            y += z * x;
            z += x * y;
            return (x, y, z);
        }
    }

    public static float Uniform(uint h) => ((h >> 9) + 0.5f) * (1f / (1 << 23));

    public static uint Field(NoiseStream stream, Seed seed, uint term) => Pcg3d((uint)(int)seed, term, (uint)stream).X;

    public static uint Branch(uint field, uint index) => Pcg3d(field, index, 0u).X;

    public static Vector4 Normals(int column, int line, uint field) {
        Vector4 u = NoiseFunctions.White(new Vector4(column, line, 0f, 0f), field);
        Vector4 t = Vector4.SquareRoot(-2f * Vector4.Log(Vector4.Min(u, Vector4.One - u)));
        Vector4 n = (((t * 0.010328f) + Vector4.Create(0.802853f)) * t) + Vector4.Create(2.515517f);
        Vector4 d = (((((t * 0.001308f) + Vector4.Create(0.189269f)) * t) + Vector4.Create(1.432788f)) * t) + Vector4.One;
        return Vector4.CopySign(t - (n / d), u - Vector4.Create(0.5f));
    }
}

internal static class NoiseFunctions {
    // --- [DRAWS]
    public static (uint X, uint Y, uint Z) Hash(Vector4 cell, uint field) =>
        unchecked(CoordinateHash.Pcg3d((uint)(int)cell.X, (uint)(int)cell.Y, CoordinateHash.Pcg3d((uint)(int)cell.Z, (uint)(int)cell.W, field).X));

    public static Vector4 White(Vector4 point, uint field) =>
        Hash(Vector4.Round(point, MidpointRounding.ToNegativeInfinity), field) switch {
            var (x, y, z) => new Vector4(CoordinateHash.Uniform(x), CoordinateHash.Uniform(y), CoordinateHash.Uniform(z), CoordinateHash.Uniform(CoordinateHash.Pcg3d(x, y, z).X)),
        };

    // --- [FRACTALS]
    internal static readonly float Peak = NoiseDimensions.Items.Max(static lattice => lattice.Scale * Enumerable.Range(0, 32).Max(top =>
        lattice.Cells.Corners.Max(corner => MathF.Abs(lattice.Slope((uint)top << 27, ((2f * corner) - Vector4.One) * lattice.Cells.Live)))));
    internal static readonly float Ceiling = float.ScaleB(1f, float.ILogB(float.MaxValue));
    internal static readonly float Reach = ((Frequency.MaxValue * (float)Clock.MaxValue) + (NoiseDistortion.MaxValue * Peak)) * WaveDetailScale.MaxValue;

    // --- [PATTERNS]
    private static readonly Func<Vector3, float, Vector3>[] Turbulence = [
        static (v, d) => v with { Y = -MathF.Cos(v.X - v.Y + v.Z) * d },
        static (v, d) => v with { X = MathF.Cos(v.X - v.Y - v.Z) * d },
        static (v, d) => v with { Z = MathF.Sin(-v.X - v.Y - v.Z) * d },
        static (v, d) => v with { X = -MathF.Cos(-v.X + v.Y - v.Z) * d },
        static (v, d) => v with { Y = -MathF.Sin(-v.X + v.Y + v.Z) * d },
        static (v, d) => v with { Y = -MathF.Cos(-v.X + v.Y + v.Z) * d },
        static (v, d) => v with { X = MathF.Cos(v.X + v.Y + v.Z) * d },
        static (v, d) => v with { Z = MathF.Sin(v.X + v.Y - v.Z) * d },
        static (v, d) => v with { X = -MathF.Cos(-v.X - v.Y + v.Z) * d },
        static (v, d) => v with { Y = -MathF.Sin(v.X - v.Y + v.Z) * d },
    ];

    public static Vector3 Magic(Vector4 point, MagicDepth depth, NoiseDistortion distortion) {
        Vector3 p = (point - (Vector4.Tau * Vector4.Round(point / float.Tau, MidpointRounding.ToNegativeInfinity))).AsVector3();
        Vector3 turned = new Vector3(MathF.Sin((p.X + p.Y + p.Z) * 5f), MathF.Cos((-p.X + p.Y - p.Z) * 5f), -MathF.Cos((-p.X - p.Y + p.Z) * 5f))
            * (depth == MagicDepth.MinValue ? 1f : distortion);
        foreach (Func<Vector3, float, Vector3> step in Turbulence.AsSpan(0, depth))
            turned = step(turned, distortion);
        return new Vector3(0.5f) - (distortion == NoiseDistortion.Neutral ? turned : turned / (2f * distortion));
    }

    extension(NoiseDimensions lattice) {
        // --- [DRAWS]
        public float Gradient(Vector4 point, uint field) {
            const float repeat = 100000f;
            Vector4 wrapped = (point - (repeat * Vector4.Truncate(point / repeat))
                + Vector4.ConditionalSelect(Vector4.GreaterThanOrEqual(Vector4.Abs(point), Vector4.Create(10f * repeat)), Vector4.Create(0.5f), Vector4.Zero)
                + (100f * (Vector4.One + White(Vector4.Zero, field)))) * lattice.Cells.Live;
            Vector4 cell = Vector4.Round(wrapped, MidpointRounding.ToNegativeInfinity);
            Vector4 local = wrapped - cell;
            Vector4 fade = new(Easing.SmootherStep(local.X), Easing.SmootherStep(local.Y), Easing.SmootherStep(local.Z), Easing.SmootherStep(local.W));
            float level = 0f;
            foreach (Vector4 corner in lattice.Cells.Corners.AsSpan()) {
                Vector4 weights = Vector4.Lerp(Vector4.One - fade, fade, corner);
                level += weights.X * weights.Y * weights.Z * weights.W * lattice.Slope(Hash(cell + corner, field).X, local - corner);
            }
            return lattice.Scale * level;
        }

        // --- [FRACTALS]
        public float Fbm(Vector4 point, Octaves octaves, uint field) {
            float detail = octaves.Detail;
            (float sum, float total, float amplitude, float scale) = (0f, 0f, 1f, 1f);
            for (int octave = 0; octave <= detail; octave++)
                (sum, total, amplitude, scale) = (sum + (lattice.Gradient(scale * point, field) * amplitude), total + amplitude, amplitude * octaves.Roughness, scale * octaves.Lacunarity);
            float remainder = detail - MathF.Floor(detail);
            return remainder == 0f ? sum / total : float.Lerp(sum / total, (sum + (lattice.Gradient(scale * point, field) * amplitude)) / (total + amplitude), remainder);
        }

        public float Multifractal(Vector4 point, Octaves octaves, uint field) {
            float detail = octaves.Detail;
            (float value, float power, Vector4 p) = (1f, 1f, point);
            for (int octave = 0; octave <= detail; octave++)
                (value, power, p) = (value * ((power * lattice.Gradient(p, field)) + 1f), power * octaves.Roughness, p * octaves.Lacunarity);
            float remainder = detail - MathF.Floor(detail);
            return remainder == 0f ? value : value * ((remainder * power * lattice.Gradient(p, field)) + 1f);
        }

        public float HeteroTerrain(Vector4 point, Octaves octaves, FractalOffset offset, uint field) {
            float detail = octaves.Detail;
            (float value, float power, Vector4 p) = (offset + lattice.Gradient(point, field), octaves.Roughness, point * octaves.Lacunarity);
            for (int octave = 1; octave <= detail; octave++)
                (value, power, p) = (value + ((lattice.Gradient(p, field) + offset) * power * value), power * octaves.Roughness, p * octaves.Lacunarity);
            float remainder = detail - MathF.Floor(detail);
            return remainder == 0f ? value : value + (remainder * ((lattice.Gradient(p, field) + offset) * power * value));
        }

        public float HybridMultifractal(Vector4 point, Octaves octaves, FractalOffset offset, FractalGain gain, uint field) {
            float detail = octaves.Detail;
            (float value, float weight, float power, Vector4 p) = (0f, 1f, 1f, point);
            for (int octave = 0; weight > 0.001f && octave <= detail; octave++) {
                float signal = (lattice.Gradient(p, field) + offset) * power;
                (value, weight, power, p) = (value + (float.Min(weight, 1f) * signal), float.Min(weight, 1f) * gain * signal, power * octaves.Roughness, p * octaves.Lacunarity);
            }
            float remainder = detail - MathF.Floor(detail);
            return remainder != 0f && weight > 0.001f ? value + (remainder * float.Min(weight, 1f) * (lattice.Gradient(p, field) + offset) * power) : value;
        }

        public float RidgedMultifractal(Vector4 point, Octaves octaves, FractalOffset offset, FractalGain gain, uint field) {
            float ridge = offset - MathF.Abs(lattice.Gradient(point, field));
            (float signal, float value, float power, Vector4 p) = (ridge * ridge, ridge * ridge, octaves.Roughness, point);
            for (int octave = 1; octave <= octaves.Detail; octave++) {
                p *= octaves.Lacunarity;
                ridge = offset - MathF.Abs(lattice.Gradient(p, field));
                signal = ridge * ridge * Easing.Saturate(signal * gain);
                (value, power) = (value + (signal * power), power * octaves.Roughness);
            }
            return value;
        }

        public Vector4 Distort(Vector4 point, NoiseDistortion distortion, uint field) {
            Vector4 moved = point;
            for (int axis = 0; axis < (distortion == NoiseDistortion.Neutral ? 0 : lattice.Cells.Rank); axis++)
                moved[axis] += distortion * lattice.Gradient(point, CoordinateHash.Branch(field, (uint)axis));
            return moved;
        }

        // --- [CELLS]
        public CellFeature F1(Vector4 point, Octaves octaves, Cellular cellular, uint field) =>
            lattice.Fractal(point, octaves, cellular, 0f, 1f, field, static (lattice, at, cells, _, draw) => lattice.Ranked(at, cells, draw).First);

        public CellFeature F2(Vector4 point, Octaves octaves, Cellular cellular, uint field) =>
            lattice.Fractal(point, octaves, cellular, 0f, 2f, field, static (lattice, at, cells, _, draw) => lattice.Ranked(at, cells, draw).Second);

        public CellFeature SmoothF1(Vector4 point, Octaves octaves, Cellular cellular, AxisFraction smoothness, uint field) =>
            smoothness == AxisFraction.MinValue
                ? lattice.F1(point, octaves, cellular, field)
                : lattice.Fractal(point, octaves, cellular, smoothness / 2f, 1f, field, static (lattice, at, cells, blend, draw) => lattice.Smooth(at, cells, blend, draw));

        public float EdgeDistance(Vector4 point, Octaves octaves, AxisFraction randomness, uint field) {
            Vector4 live = point * lattice.Cells.Live;
            float limit = 0.5f + (0.5f * randomness);
            (float bound, float distance, float amplitude, float scale) = (limit, 8f, 1f, 1f);
            for (int octave = 0; octave < octaves.Cells; octave++) {
                float weight = amplitude * float.Min(1f, octaves.Detail - octave + 1f);
                (bound, distance) = (float.Lerp(bound, limit / scale, weight), float.Lerp(distance, float.Min(distance, lattice.Edge(live * scale, randomness, field) / scale), weight));
                (amplitude, scale) = (amplitude * octaves.Roughness, scale * octaves.Lacunarity);
            }
            return distance / bound;
        }

        private CellFeature Fractal(
            Vector4 point, Octaves octaves, Cellular cellular, float smoothness, float span, uint field, Func<NoiseDimensions, Vector4, Cellular, float, uint, CellFeature> scan) {
            Vector4 live = point * lattice.Cells.Live;
            float limit = span * cellular.Metric.Length((0.5f + (0.5f * cellular.Randomness)) * lattice.Cells.Live, cellular.Exponent);
            (float total, float distance, Vector4 site, float amplitude, float scale) = (0f, 0f, Vector4.Zero, 1f, 1f);
            for (int octave = 0; octave < octaves.Cells; octave++) {
                CellFeature feature = scan(lattice, live * scale, cellular, smoothness, field);
                float weight = amplitude * float.Min(1f, octaves.Detail - octave + 1f);
                (total, distance, site) = (total + weight, distance + (feature.Distance * weight), Vector4.Lerp(site, feature.Site / scale, weight));
                (amplitude, scale) = (amplitude * octaves.Roughness, scale * octaves.Lacunarity);
            }
            return new(distance / (total * limit), site);
        }

        private (CellFeature First, CellFeature Second) Ranked(Vector4 point, Cellular cellular, uint field) {
            Vector4 cell = Vector4.Round(point, MidpointRounding.ToNegativeInfinity);
            Vector4 local = point - cell;
            (CellFeature First, CellFeature Second) nearest = (new(float.MaxValue, Vector4.Zero), new(float.MaxValue, Vector4.Zero));
            foreach (Vector4 offset in lattice.Cells.Near.AsSpan()) {
                Vector4 site = lattice.Site(cell, offset, cellular.Randomness, field);
                float distance = cellular.Metric.Length(site - local, cellular.Exponent);
                nearest = distance < nearest.First.Distance ? (new(distance, site), nearest.First)
                    : distance < nearest.Second.Distance ? (nearest.First, new(distance, site))
                    : nearest;
            }
            return (nearest.First with { Site = cell + nearest.First.Site }, nearest.Second with { Site = cell + nearest.Second.Site });
        }

        private CellFeature Smooth(Vector4 point, Cellular cellular, float smoothness, uint field) {
            Vector4 cell = Vector4.Round(point, MidpointRounding.ToNegativeInfinity);
            Vector4 local = point - cell;
            ReadOnlySpan<Vector4> wide = lattice.Cells.Wide.AsSpan();
            (float distance, Vector4 site) = (0f, Vector4.Zero);
            for (int index = 0; index < wide.Length; index++) {
                Vector4 next = lattice.Site(cell, wide[index], cellular.Randomness, field);
                float reach = cellular.Metric.Length(next - local, cellular.Exponent);
                float blend = index == 0 ? 1f : Easing.SmoothStep(Easing.Saturate(0.5f + (0.5f * (distance - reach) / smoothness)));
                float correction = smoothness * blend * (1f - blend);
                (distance, site) = (float.Lerp(distance, reach, blend) - correction, Vector4.Lerp(site, next, blend) - (correction / (1f + (3f * smoothness)) * lattice.Cells.Live));
            }
            return new(distance, cell + site);
        }

        private float Edge(Vector4 point, AxisFraction randomness, uint field) {
            Vector4 cell = Vector4.Round(point, MidpointRounding.ToNegativeInfinity);
            ReadOnlySpan<Vector4> near = lattice.Cells.Near.AsSpan();
            Span<Vector4> reach = stackalloc Vector4[near.Length];
            Vector4 closest = Vector4.Zero;
            for (int index = 0; index < near.Length; index++) {
                reach[index] = lattice.Site(cell, near[index], randomness, field) - (point - cell);
                closest = index == 0 || reach[index].LengthSquared() < closest.LengthSquared() ? reach[index] : closest;
            }
            float edge = float.MaxValue;
            foreach (Vector4 toward in reach)
                edge = (toward - closest).LengthSquared() > 0.0001f ? float.Min(edge, Vector4.Dot((closest + toward) / 2f, Vector4.Normalize(toward - closest))) : edge;
            return edge;
        }

        private Vector4 Site(Vector4 cell, Vector4 offset, AxisFraction randomness, uint field) => offset + (White(cell + offset, field) * lattice.Cells.Live * randomness);

        // --- [GABOR]
        public Vector2 Gabor(Vector4 point, GaborFrequency frequency, AxisFraction anisotropy, GaborOrientation orientation, uint field) {
            (GaborKernel kernel, int impulses) = (lattice.Gabor, GaborKernel.Impulses);
            Vector4 live = point * kernel.Cells.Live;
            Vector4 cell = Vector4.Round(live, MidpointRounding.ToNegativeInfinity);
            Span<uint> branches = stackalloc uint[2 * impulses];
            for (int index = 0; index < branches.Length; index++)
                branches[index] = CoordinateHash.Branch(field, (uint)index);
            ReadOnlySpan<Vector4> near = kernel.Cells.Near.AsSpan();
            Vector2 phasor = Vector2.Zero;
            for (int index = 0; index < near.Length * impulses; index++) {
                (Vector4 neighbor, int impulse) = (cell + near[index / impulses], index % impulses);
                Vector4 center = White(neighbor, branches[2 * impulse]);
                Vector4 reach = live - neighbor - (center * kernel.Cells.Live);
                float spread = reach.LengthSquared();
                phasor += spread >= 1f ? Vector2.Zero
                    : MathF.SinCos(2f * float.Pi * frequency * Vector4.Dot(reach, kernel.Orient(orientation, 1f - anisotropy, White(neighbor, branches[(2 * impulse) + 1])))) switch {
                        var (sin, cos) => float.CopySign(1f, center.W - 0.5f) * MathF.Exp(-float.Pi * spread) * (0.5f + (0.5f * MathF.Cos(float.Pi * spread))) * new Vector2(cos, sin),
                    };
            }
            return phasor / (6f * kernel.Deviation);
        }

        // --- [PATTERNS]
        public float Wave(Vector4 point, WaveForm form, WaveProfile profile, float phase, NoiseDistortion distortion, Octaves octaves, WaveDetailScale detailScale, uint field) =>
            profile.Level((float.Tau * (form.Distance(point) - point.Z)) + phase + (distortion == NoiseDistortion.Neutral ? 0f
                : distortion * lattice.Fbm(new Vector4(point.AsVector2() * (float.Pi / 10f) * detailScale, 0f, 0f), octaves, CoordinateHash.Branch(field, 0u))));
    }
}
