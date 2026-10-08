using System.Numerics;
using MathNet.Numerics.Distributions;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Generators;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum NoiseStream {
    FilmGrain, SensorNoise, AnalogVideo, Glitch, Mosaic, GlyphCells, FilmDamage, FilmFlicker, Wave, Turbulence, Refraction, Shake,
    NoiseTexture, Fog, Starfield, ParticleField, Shape, Lightning, Grid, LensFlare, LightRays, LensDirt,
}

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class ReferenceFrame {
    public const float Height = 1080f;
    public const float GreaterSide = 1920f / Height;
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

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Clock : IMinMaxValue<Clock> {
    public static Clock MinValue { get; } = new(0f);
    public static Clock MaxValue { get; } = new(1 << 14);

    public uint Term => BitConverter.SingleToUInt32Bits(_value);

    internal Clock Advanced(double seconds) => new((float)((_value + seconds) % MaxValue._value));

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Pace : IMinMaxValue<Pace> {
    public static Pace MinValue { get; } = new(0f);
    public static Pace MaxValue { get; } = new(float.MaxValue);
    public static Pace Standard { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Hold : IMinMaxValue<Hold> {
    public static Hold MinValue { get; } = new(0.001f);
    public static Hold MaxValue { get; } = new(8f);
    public static Presentation<Hold, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second), Scale = TrackScale.Log };

    public uint Period(Clock clock) => (uint)Math.Floor((clock + (1d / TimeSpan.TicksPerSecond)) / _value * (1d + (1d / (1 << 22))));

    public float Held(Clock clock) => Period(clock) * _value;

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Instant", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ShutterTime : IMinMaxValue<ShutterTime> {
    public static ShutterTime MinValue => Instant;
    public static ShutterTime MaxValue { get; } = new(0.5f);
    public static Presentation<ShutterTime, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Frequency : IMinMaxValue<Frequency> {
    public static Frequency MinValue { get; } = new(-60f);
    public static Frequency MaxValue { get; } = new(60f);
    public static Presentation<Frequency, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(FrequencyUnit.Hertz), Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Drift : IMinMaxValue<Drift> {
    public static Drift MinValue { get; } = new(-10f);
    public static Drift MaxValue { get; } = new(10f);
    public static Presentation<Drift, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioChangeRateUnit.DecimalFractionPerSecond), Origin = 0f };

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

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct MinkowskiExponent : IMinMaxValue<MinkowskiExponent> {
    public static MinkowskiExponent MinValue { get; } = new(1f / 32f);
    public static MinkowskiExponent MaxValue { get; } = new(32f);
    public static MinkowskiExponent Standard { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct GaborFrequency : IMinMaxValue<GaborFrequency> {
    public static GaborFrequency MinValue { get; } = new(0.001f);
    public static GaborFrequency MaxValue { get; } = new(1000f);
    public static GaborFrequency Standard { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct MagicDepth : IMinMaxValue<MagicDepth> {
    public static MagicDepth MinValue { get; } = new(0);
    public static MagicDepth MaxValue { get; } = new(10);
    public static MagicDepth Standard { get; } = new(2);

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
    public static readonly CellMetric Chebyshev = new("chebyshev", static (delta, _) => Vector4.Abs(delta) switch { var a => float.Max(float.Max(a.X, a.Y), float.Max(a.Z, a.W)) });
    public static readonly CellMetric Minkowski = new("minkowski", static (delta, exponent) => MathF.Pow(Vector4.Sum(Vector4.Exp(exponent * Vector4.Log(Vector4.Abs(delta)))), 1f / exponent));

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
    public static readonly WaveProfile Sine = new("sine", static periods => 0.5f - (0.5f * float.CosPi(2f * periods)));
    public static readonly WaveProfile Saw = new("saw", static periods => periods - MathF.Floor(periods));
    public static readonly WaveProfile Triangle = new("triangle", static periods => 2f * MathF.Abs(periods - MathF.Floor(periods + 0.5f)));
    public static readonly WaveProfile Square = new("square", static periods => MathF.Floor(2f * (periods - MathF.Floor(periods))));

    [UseDelegateFromConstructor]
    public partial float Level(float periods);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BasisKind {
    public static readonly BasisKind Fbm = new("fbm", static basis => basis.Distorted((point, field) => 0.5f + (0.5f * basis.Dimensions.Fbm(point, basis.Octaves, field))));
    public static readonly BasisKind Multifractal = new("multifractal", static basis =>
        basis.Distorted((point, field) => basis.Dimensions.Cascade(point, basis.Octaves, 0f, 1f, field)));
    public static readonly BasisKind HeteroTerrain = new("hetero-terrain", static basis =>
        basis.Distorted((point, field) => basis.Dimensions.Cascade(point, basis.Octaves, basis.FractalOffset, 0f, field)));
    public static readonly BasisKind HybridMultifractal = new("hybrid-multifractal", static basis =>
        basis.Distorted((point, field) => basis.Dimensions.HybridMultifractal(point, basis.Octaves, basis.FractalOffset, basis.FractalGain, field)));
    public static readonly BasisKind RidgedMultifractal = new("ridged-multifractal", static basis =>
        basis.Distorted((point, field) => basis.Dimensions.RidgedMultifractal(point, basis.Octaves, basis.FractalOffset, basis.FractalGain, field)));
    public static readonly BasisKind F1 = new("f1", static basis => (point, field) => basis.Dimensions.F1(point, basis.Octaves, basis.Cellular, AxisFraction.MinValue, field));
    public static readonly BasisKind F2 = new("f2", static basis => (point, field) => basis.Dimensions.F2(point, basis.Octaves, basis.Cellular, field));
    public static readonly BasisKind SmoothF1 = new("smooth-f1", static basis => (point, field) =>
        basis.Dimensions.F1(point, basis.Octaves, basis.Cellular, basis.CellSmoothness, field));
    public static readonly BasisKind EdgeDistance = new("edge-distance", static basis => (point, field) =>
        new(basis.Dimensions.EdgeDistance(point, basis.Octaves, basis.Cellular.Randomness, field), point));
    public static readonly BasisKind Gabor = new("gabor", static basis => (point, field) =>
        new(0.5f + (0.5f * basis.Dimensions.Gabor(point, basis.GaborFrequency, basis.GaborAnisotropy, basis.GaborOrientation, field).Y), point));
    public static readonly BasisKind Magic = new("magic", static basis => (point, _) => new(Vector3.Sum(NoiseFunctions.Magic(point, basis.MagicDepth, basis.Distortion)) / 3f, point));
    public static readonly BasisKind Wave = new("wave", static basis => (point, field) =>
        new(basis.Dimensions.Wave(point, basis.WaveForm, basis.WaveProfile, basis.WavePhase, basis.Distortion, basis.Octaves, basis.WaveDetailScale, field), point));
    public static readonly BasisKind White = new("white", static basis => (point, field) => new(NoiseFunctions.White(point * basis.Dimensions.Cells.Live, field).X, point));

    [UseDelegateFromConstructor]
    internal partial Func<Vector4, uint, NoiseSample> Sampler(NoiseBasis basis);
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

internal sealed record GaborKernel(Neighborhood Cells, Func<float, float, Vector4, Vector4> Orient);

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseDimensions {
    private static readonly GaborKernel Planar = new(Neighborhood.Of(2), static (angle, isotropy, draw) =>
        MathF.SinCos(angle + ((draw.X - 0.5f) * float.Pi * isotropy)) switch { var (sin, cos) => new Vector4(cos, sin, 0f, 0f) });
    private static readonly GaborKernel Spherical = new(Neighborhood.Of(3), static (angle, isotropy, draw) =>
        (MathF.SinCos((float.Pi / 2f) + (draw.X * float.Pi * isotropy)), MathF.SinCos(angle + (draw.Y * float.Pi * isotropy))) switch {
            var ((sinInclination, cosInclination), (sinAzimuth, cosAzimuth)) => new Vector4(sinInclination * cosAzimuth, sinInclination * sinAzimuth, cosInclination, 0f),
        });

    public static readonly NoiseDimensions One = new("1d", Neighborhood.Of(1), 0.25f, Planar, static (word, offset) =>
        (word >> 28) switch { var h => (1u + (h & 7u)) * ((h & 8u) != 0u ? -offset.X : offset.X) });
    public static readonly NoiseDimensions Two = new("2d", Planar.Cells, 0.6616f, Planar, static (word, offset) =>
        (word >> 29) switch { var h => (((h & 1u) != 0u ? -1f : 1f) * (h < 4u ? offset.X : offset.Y)) + (((h & 2u) != 0u ? -2f : 2f) * (h < 4u ? offset.Y : offset.X)) });
    public static readonly NoiseDimensions Three = new("3d", Spherical.Cells, 0.982f, Spherical, static (word, offset) =>
        (word >> 28) switch { var h => (((h & 1u) != 0u ? -1f : 1f) * (h < 8u ? offset.X : offset.Y)) + (((h & 2u) != 0u ? -1f : 1f) * (h < 4u ? offset.Y : h is 12u or 14u ? offset.X : offset.Z)) });
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

public sealed record Timing(Clock Clock, Pace Pace) : IStateRecord<Timing, TimingParameter, InvalidGenerator> {
    public static Timing Default { get; } = new(Clock.MinValue, Pace.Standard);

    public Clock At(PassContext context) => context.Time.Map(time => Clock.Advanced((double)Pace * time.TotalSeconds)).IfNone(Clock);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimingParameter : IStateParameter<Timing> {
    public static readonly TimingParameter Clock = new("clock", new StateParameter<Timing>.Bounded<Clock, float, InvalidGenerator>(
        Lens<Timing, Clock>.New(static timing => timing.Clock, static clock => timing => timing with { Clock = clock }), new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) }));
    public static readonly TimingParameter Pace = new("pace", new StateParameter<Timing>.Bounded<Pace, float, InvalidGenerator>(
        Lens<Timing, Pace>.New(static timing => timing.Pace, static pace => timing => timing with { Pace = pace }), new() { Soft = (0f, 4f) }));

    public StateParameter<Timing> Kind { get; }
}

public sealed record Octaves(FractalDetail Detail, AxisFraction Roughness, FractalLacunarity Lacunarity) : IStateRecord<Octaves, OctavesParameter, InvalidGenerator> {
    public static Octaves Default { get; } = new(FractalDetail.Standard, AxisFraction.Half, FractalLacunarity.Standard);
    public static Octaves Plain { get; } = Default with { Detail = FractalDetail.MinValue };

    internal float Weight(int octave) => float.Min(1f, Detail - octave + 1f);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OctavesParameter : IStateParameter<Octaves> {
    public static readonly OctavesParameter Detail = new("detail", new StateParameter<Octaves>.Bounded<FractalDetail, float, InvalidGenerator>(
        Lens<Octaves, FractalDetail>.New(static octaves => octaves.Detail, static detail => octaves => octaves with { Detail = detail }), new()));
    public static readonly OctavesParameter Roughness = new("roughness", new StateParameter<Octaves>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Octaves, AxisFraction>.New(static octaves => octaves.Roughness, static roughness => octaves => octaves with { Roughness = roughness }), new()));
    public static readonly OctavesParameter Lacunarity = new("lacunarity", new StateParameter<Octaves>.Bounded<FractalLacunarity, float, InvalidGenerator>(
        Lens<Octaves, FractalLacunarity>.New(static octaves => octaves.Lacunarity, static lacunarity => octaves => octaves with { Lacunarity = lacunarity }), new()));

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
        Lens<Cellular, CellMetric>.New(static cells => cells.Metric, static metric => cells => cells with { Metric = metric })));
    public static readonly CellularParameter Exponent = new("exponent", new StateParameter<Cellular>.Bounded<MinkowskiExponent, float, InvalidGenerator>(
        Lens<Cellular, MinkowskiExponent>.New(static cells => cells.Exponent, static exponent => cells => cells with { Exponent = exponent }), new() { Scale = TrackScale.Log }));
    public static readonly CellularParameter Randomness = new("randomness", new StateParameter<Cellular>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Cellular, AxisFraction>.New(static cells => cells.Randomness, static randomness => cells => cells with { Randomness = randomness }), new()));

    public StateParameter<Cellular> Kind { get; }
}

public sealed record NoiseBasis(
    BasisKind Kind, NoiseDimensions Dimensions, Octaves Octaves, NoiseDistortion Distortion, Cellular Cellular, FractalOffset FractalOffset, FractalGain FractalGain,
    AxisFraction CellSmoothness, GaborFrequency GaborFrequency, AxisFraction GaborAnisotropy, SignedAngle GaborOrientation, MagicDepth MagicDepth,
    WaveForm WaveForm, WaveProfile WaveProfile, SignedAngle WavePhase, WaveDetailScale WaveDetailScale) : IStateRecord<NoiseBasis, NoiseBasisParameter, InvalidGenerator> {
    public static NoiseBasis Default { get; } = new(
        BasisKind.Fbm, NoiseDimensions.Three, Octaves.Default, NoiseDistortion.Neutral, Cellular.Default, FractalOffset.Neutral, FractalGain.Standard,
        AxisFraction.MaxValue, GaborFrequency.Standard, AxisFraction.MaxValue, SignedAngle.Diagonal, MagicDepth.Standard,
        WaveForm.Bands, WaveProfile.Sine, SignedAngle.Neutral, WaveDetailScale.Standard);
    public static NoiseBasis Plain { get; } = Default with { Octaves = Octaves.Plain };

    internal Func<Vector4, uint, NoiseSample> Sampler => Kind.Sampler(this);

    internal Func<Vector4, uint, NoiseSample> Distorted(Func<Vector4, uint, float> sum) =>
        (point, field) => new(sum(Dimensions.Distort(point, Distortion, CoordinateHash.Branch(field, 0u)), CoordinateHash.Branch(field, 1u)), point);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseBasisParameter : IStateParameter<NoiseBasis> {
    public static readonly NoiseBasisParameter Basis = new("kind", new StateParameter<NoiseBasis>.Choice<BasisKind, InvalidGenerator>(
        Lens<NoiseBasis, BasisKind>.New(static basis => basis.Kind, static kind => basis => basis with { Kind = kind })));
    public static readonly NoiseBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis>.Choice<NoiseDimensions, InvalidGenerator>(
        Lens<NoiseBasis, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })));
    public static readonly NoiseBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis>.Record<Octaves>(
        Lens<NoiseBasis, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })));
    public static readonly NoiseBasisParameter Distortion = new("distortion", new StateParameter<NoiseBasis>.Bounded<NoiseDistortion, float, InvalidGenerator>(
        Lens<NoiseBasis, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion }), new() { Origin = 0f }));
    public static readonly NoiseBasisParameter Cellular = new("cellular", new StateParameter<NoiseBasis>.Record<Cellular>(
        Lens<NoiseBasis, Cellular>.New(static basis => basis.Cellular, static cells => basis => basis with { Cellular = cells })));
    public static readonly NoiseBasisParameter Offset = new("offset", new StateParameter<NoiseBasis>.Bounded<FractalOffset, float, InvalidGenerator>(
        Lens<NoiseBasis, FractalOffset>.New(static basis => basis.FractalOffset, static offset => basis => basis with { FractalOffset = offset }), new() { Origin = 0f }));
    public static readonly NoiseBasisParameter Gain = new("gain", new StateParameter<NoiseBasis>.Bounded<FractalGain, float, InvalidGenerator>(
        Lens<NoiseBasis, FractalGain>.New(static basis => basis.FractalGain, static gain => basis => basis with { FractalGain = gain }), new()));
    public static readonly NoiseBasisParameter Smoothness = new("smoothness", new StateParameter<NoiseBasis>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<NoiseBasis, AxisFraction>.New(static basis => basis.CellSmoothness, static smoothness => basis => basis with { CellSmoothness = smoothness }), new()));
    public static readonly NoiseBasisParameter Frequency = new("frequency", new StateParameter<NoiseBasis>.Bounded<GaborFrequency, float, InvalidGenerator>(
        Lens<NoiseBasis, GaborFrequency>.New(static basis => basis.GaborFrequency, static frequency => basis => basis with { GaborFrequency = frequency }), new() { Scale = TrackScale.Log }));
    public static readonly NoiseBasisParameter Anisotropy = new("anisotropy", new StateParameter<NoiseBasis>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<NoiseBasis, AxisFraction>.New(static basis => basis.GaborAnisotropy, static anisotropy => basis => basis with { GaborAnisotropy = anisotropy }), new()));
    public static readonly NoiseBasisParameter Orientation = new("orientation", new StateParameter<NoiseBasis>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<NoiseBasis, SignedAngle>.New(static basis => basis.GaborOrientation, static orientation => basis => basis with { GaborOrientation = orientation }), SignedAngle.Presentation));
    public static readonly NoiseBasisParameter Depth = new("depth", new StateParameter<NoiseBasis>.Bounded<MagicDepth, int, InvalidGenerator>(
        Lens<NoiseBasis, MagicDepth>.New(static basis => basis.MagicDepth, static depth => basis => basis with { MagicDepth = depth }), new() { Step = 1 }));
    public static readonly NoiseBasisParameter Form = new("form", new StateParameter<NoiseBasis>.Choice<WaveForm, InvalidGenerator>(
        Lens<NoiseBasis, WaveForm>.New(static basis => basis.WaveForm, static form => basis => basis with { WaveForm = form })));
    public static readonly NoiseBasisParameter Profile = new("profile", new StateParameter<NoiseBasis>.Choice<WaveProfile, InvalidGenerator>(
        Lens<NoiseBasis, WaveProfile>.New(static basis => basis.WaveProfile, static profile => basis => basis with { WaveProfile = profile })));
    public static readonly NoiseBasisParameter Phase = new("phase", new StateParameter<NoiseBasis>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<NoiseBasis, SignedAngle>.New(static basis => basis.WavePhase, static phase => basis => basis with { WavePhase = phase }), SignedAngle.Presentation));
    public static readonly NoiseBasisParameter DetailScale = new("detail-scale", new StateParameter<NoiseBasis>.Bounded<WaveDetailScale, float, InvalidGenerator>(
        Lens<NoiseBasis, WaveDetailScale>.New(static basis => basis.WaveDetailScale, static scale => basis => basis with { WaveDetailScale = scale }), new()));

    public StateParameter<NoiseBasis> Kind { get; }
}

internal readonly record struct NoiseSample(float Value, Vector4 Site);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Easing {
    public static float Saturate(float value) => value > 0f ? float.Min(value, 1f) : 0f;
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

    public static uint Field(NoiseStream stream, Seed seed, uint term) => Pcg3d((uint)(int)seed, term, (uint)stream).X;

    public static uint Branch(uint field, uint index) => Pcg3d(field, index, 0u).X;

    public static Vector4 Normals(int column, int line, uint field) =>
        NoiseFunctions.White(new Vector4(column, line, 0f, 0f), field) switch {
            var u => new((float)Normal.InvCDF(0d, 1d, u.X), (float)Normal.InvCDF(0d, 1d, u.Y), (float)Normal.InvCDF(0d, 1d, u.Z), (float)Normal.InvCDF(0d, 1d, u.W)),
        };
}

internal static class NoiseFunctions {
    // --- [DRAWS]
    public static (uint X, uint Y, uint Z) Hash(Vector4 cell, uint field) =>
        unchecked(CoordinateHash.Pcg3d((uint)(int)cell.X, (uint)(int)cell.Y, CoordinateHash.Pcg3d((uint)(int)cell.Z, (uint)(int)cell.W, field).X));

    public static Vector4 White(Vector4 point, uint field) =>
        Hash(Vector4.Round(point, MidpointRounding.ToNegativeInfinity), field) switch {
            var (x, y, z) => (new Vector4(x >> 9, y >> 9, z >> 9, CoordinateHash.Pcg3d(x, y, z).X >> 9) + Vector4.Create(0.5f)) * (1f / (1 << 23)),
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
            Vector4 fade = new((float)RampInterpolation.SmootherRgb.Weight(local.X), (float)RampInterpolation.SmootherRgb.Weight(local.Y), (float)RampInterpolation.SmootherRgb.Weight(local.Z), (float)RampInterpolation.SmootherRgb.Weight(local.W));
            float level = 0f;
            foreach (Vector4 corner in lattice.Cells.Corners.AsSpan()) {
                Vector4 weights = Vector4.Lerp(Vector4.One - fade, fade, corner);
                level += weights.X * weights.Y * weights.Z * weights.W * lattice.Slope(Hash(cell + corner, field).X, local - corner);
            }
            return lattice.Scale * level;
        }

        // --- [FRACTALS]
        public float Fbm(Vector4 point, Octaves octaves, uint field) {
            (float sum, float total, float level, float amplitude, float scale) = (0f, 0f, 0f, 1f, 1f);
            for (int octave = 0; octaves.Weight(octave) > 0f; octave++) {
                (sum, total) = (sum + (lattice.Gradient(scale * point, field) * amplitude), total + amplitude);
                (level, amplitude, scale) = (float.Lerp(level, sum / total, octaves.Weight(octave)), amplitude * octaves.Roughness, scale * octaves.Lacunarity);
            }
            return level;
        }

        public float Cascade(Vector4 point, Octaves octaves, float offset, float lead, uint field) {
            (float value, float amplitude, Vector4 p) = (1f, 1f, point);
            for (int octave = 0; octaves.Weight(octave) > 0f; octave++)
                (value, amplitude, p) = (value * ((octave == 0 ? lead : 1f) + (octaves.Weight(octave) * amplitude * (lattice.Gradient(p, field) + offset))), amplitude * octaves.Roughness, p * octaves.Lacunarity);
            return value;
        }

        public float HybridMultifractal(Vector4 point, Octaves octaves, FractalOffset offset, FractalGain gain, uint field) {
            (float value, float weight, float amplitude, Vector4 p) = (0f, 1f, 1f, point);
            for (int octave = 0; weight > 0.001f && octaves.Weight(octave) > 0f; octave++) {
                (float held, float signal) = (float.Min(weight, 1f), (lattice.Gradient(p, field) + offset) * amplitude);
                (value, weight, amplitude, p) = (value + (octaves.Weight(octave) * held * signal), held * gain * signal, amplitude * octaves.Roughness, p * octaves.Lacunarity);
            }
            return value;
        }

        public float RidgedMultifractal(Vector4 point, Octaves octaves, FractalOffset offset, FractalGain gain, uint field) {
            (float signal, float value, float amplitude, Vector4 p) = (0f, 0f, 1f, point);
            for (int octave = 0; octaves.Weight(octave) >= 1f; octave++) {
                float ridge = offset - MathF.Abs(lattice.Gradient(p, field));
                signal = ridge * ridge * (octave == 0 ? 1f : Easing.Saturate(signal * gain));
                (value, amplitude, p) = (value + (signal * amplitude), amplitude * octaves.Roughness, p * octaves.Lacunarity);
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
        public NoiseSample F1(Vector4 point, Octaves octaves, Cellular cellular, AxisFraction smoothness, uint field) =>
            smoothness == AxisFraction.MinValue
                ? lattice.Fractal(point, octaves, cellular, 0f, 1f, field, static (lattice, at, cells, _, draw) => lattice.Ranked(at, cells, draw).First)
                : lattice.Fractal(point, octaves, cellular, smoothness / 2f, 1f, field, static (lattice, at, cells, blend, draw) => lattice.Smooth(at, cells, blend, draw));

        public NoiseSample F2(Vector4 point, Octaves octaves, Cellular cellular, uint field) =>
            lattice.Fractal(point, octaves, cellular, 0f, 2f, field, static (lattice, at, cells, _, draw) => lattice.Ranked(at, cells, draw).Second);

        public float EdgeDistance(Vector4 point, Octaves octaves, AxisFraction randomness, uint field) {
            (Vector4 live, float limit) = (point * lattice.Cells.Live, 0.5f + (0.5f * randomness));
            (float bound, float distance, float amplitude, float scale) = (limit, 8f, 1f, 1f);
            for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
                float weight = amplitude * octaves.Weight(octave);
                (bound, distance) = (float.Lerp(bound, limit / scale, weight), float.Lerp(distance, float.Min(distance, lattice.Edge(live * scale, randomness, field) / scale), weight));
                (amplitude, scale) = (amplitude * octaves.Roughness, scale * octaves.Lacunarity);
            }
            return distance / bound;
        }

        private NoiseSample Fractal(
            Vector4 point, Octaves octaves, Cellular cellular, float smoothness, float span, uint field, Func<NoiseDimensions, Vector4, Cellular, float, uint, NoiseSample> scan) {
            Vector4 live = point * lattice.Cells.Live;
            float limit = span * cellular.Metric.Length((0.5f + (0.5f * cellular.Randomness)) * lattice.Cells.Live, cellular.Exponent);
            (float total, float distance, Vector4 site, float amplitude, float scale) = (0f, 0f, Vector4.Zero, 1f, 1f);
            for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
                NoiseSample feature = scan(lattice, live * scale, cellular, smoothness, field);
                float weight = amplitude * octaves.Weight(octave);
                (total, distance, site) = (total + weight, distance + (feature.Value * weight), Vector4.Lerp(site, feature.Site / scale, weight));
                (amplitude, scale) = (amplitude * octaves.Roughness, scale * octaves.Lacunarity);
            }
            return new(distance / (total * limit), site);
        }

        private (NoiseSample First, NoiseSample Second) Ranked(Vector4 point, Cellular cellular, uint field) {
            Vector4 cell = Vector4.Round(point, MidpointRounding.ToNegativeInfinity);
            (NoiseSample First, NoiseSample Second) nearest = (new(float.MaxValue, Vector4.Zero), new(float.MaxValue, Vector4.Zero));
            foreach (Vector4 offset in lattice.Cells.Near.AsSpan()) {
                Vector4 site = lattice.Site(cell, offset, cellular.Randomness, field);
                float distance = cellular.Metric.Length(site - (point - cell), cellular.Exponent);
                nearest = distance < nearest.First.Value ? (new(distance, site), nearest.First)
                    : distance < nearest.Second.Value ? (nearest.First, new(distance, site))
                    : nearest;
            }
            return (nearest.First with { Site = cell + nearest.First.Site }, nearest.Second with { Site = cell + nearest.Second.Site });
        }

        private NoiseSample Smooth(Vector4 point, Cellular cellular, float smoothness, uint field) {
            Vector4 cell = Vector4.Round(point, MidpointRounding.ToNegativeInfinity);
            ReadOnlySpan<Vector4> wide = lattice.Cells.Wide.AsSpan();
            (float distance, Vector4 site) = (0f, Vector4.Zero);
            for (int index = 0; index < wide.Length; index++) {
                Vector4 next = lattice.Site(cell, wide[index], cellular.Randomness, field);
                float reach = cellular.Metric.Length(next - (point - cell), cellular.Exponent);
                float blend = index == 0 ? 1f : (float)RampInterpolation.Ease.Weight(Easing.Saturate(0.5f + (0.5f * (distance - reach) / smoothness)));
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
        public Vector2 Gabor(Vector4 point, GaborFrequency frequency, AxisFraction anisotropy, SignedAngle orientation, uint field) {
            const uint impulses = 8u;
            GaborKernel kernel = lattice.Gabor;
            Vector4 live = point * kernel.Cells.Live;
            Vector4 cell = Vector4.Round(live, MidpointRounding.ToNegativeInfinity);
            Vector2 phasor = Vector2.Zero;
            for (uint impulse = 0u; impulse < impulses; impulse++) {
                (uint centers, uint turns) = (CoordinateHash.Branch(field, 2u * impulse), CoordinateHash.Branch(field, (2u * impulse) + 1u));
                foreach (Vector4 neighbor in kernel.Cells.Near.AsSpan()) {
                    Vector4 center = White(cell + neighbor, centers);
                    Vector4 reach = live - cell - neighbor - (center * kernel.Cells.Live);
                    float spread = reach.LengthSquared();
                    (float sin, float cos) = spread < 1f
                        ? MathF.SinCos(float.Tau * frequency * Vector4.Dot(reach, kernel.Orient(orientation, 1f - anisotropy, White(cell + neighbor, turns))))
                        : (0f, 0f);
                    phasor += float.CopySign(MathF.Exp(-float.Pi * spread) * (0.5f + (0.5f * MathF.Cos(float.Pi * spread))), center.W - 0.5f) * new Vector2(cos, sin);
                }
            }
            return phasor / (6f * MathF.Sqrt(impulses * 0.5f * float.Exp2(-1f - (kernel.Cells.Rank / 2f))));
        }

        // --- [PATTERNS]
        public float Wave(Vector4 point, WaveForm form, WaveProfile profile, float phase, NoiseDistortion distortion, Octaves octaves, WaveDetailScale detailScale, uint field) =>
            profile.Level(form.Distance(point) - point.Z + ((phase + (distortion == NoiseDistortion.Neutral ? 0f
                : distortion * lattice.Fbm(new Vector4(point.AsVector2() * (float.Pi / 10f) * detailScale, 0f, 0f), octaves, CoordinateHash.Branch(field, 0u)))) / float.Tau));
    }
}
