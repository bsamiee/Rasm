using System.Numerics;
using MathNet.Numerics.Distributions;
using PeterO.Numbers;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Generators;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum NoiseStream {
    FilmGrain = 0,
    SensorNoise = 1,
    AnalogVideo = 2,
    Glitch = 3,
    Mosaic = 4,
    GlyphCells = 5,
    FilmDamage = 6,
    FilmFlicker = 7,
    Wave = 8,
    Turbulence = 9,
    Refraction = 10,
    Shake = 11,
    NoiseTexture = 12,
    Fog = 13,
    Starfield = 14,
    ParticleField = 15,
    Shape = 16,
    Lightning = 17,
    Grid = 18,
    LensFlare = 19,
    LightRays = 20,
    LensDirt = 21,
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
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

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
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second), Scale = TrackScale.Log };

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
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Frequency : IMinMaxValue<Frequency> {
    public static Frequency MinValue { get; } = new(-60f);
    public static Frequency MaxValue { get; } = new(60f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(FrequencyUnit.Hertz), Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct Drift : IMinMaxValue<Drift> {
    public static Drift MinValue { get; } = new(-10f);
    public static Drift MaxValue { get; } = new(10f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioChangeRateUnit.DecimalFractionPerSecond), Origin = 0f };

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
    public static readonly WaveForm Bands = new("bands", static point => point.X, static point => point.X);
    public static readonly WaveForm Rings = new("rings", static point => point.AsVector2().Length(), static point => {
        ERational square = point.X.Multiply(point.X).Add(point.Y.Multiply(point.Y));
        EInteger whole = square.ToEInteger().Sqrt();
        EContext precision = EContext.Binary64.WithUnlimitedExponents();
        EFloat remainder = square.IsZero ? EFloat.Zero : square.Subtract(ERational.FromEInteger(whole.Multiply(whole))).ToEFloat(precision)
            .Divide(square.ToEFloat(precision).Sqrt(precision).Add(EFloat.FromEInteger(whole), precision), precision);
        return ERational.FromEInteger(whole).Add(ERational.FromEFloat(remainder));
    });

    [UseDelegateFromConstructor]
    public partial float Distance(Vector4 point);

    [UseDelegateFromConstructor]
    internal partial ERational Periods((ERational X, ERational Y, ERational Z, ERational W) point);
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
    public static readonly BasisKind Fbm = new("fbm", static basis => basis.Distorted((point, field) => 0.5f + (0.5f * basis.Dimensions.Fbm(point, basis.Octaves, field))),
        static basis => basis.Dimensions.FbmBound(basis.Octaves) switch { var bound => Fin.Succ((0.5f - (0.5f * bound), 0.5f + (0.5f * bound))) });
    public static readonly BasisKind Multifractal = new("multifractal", static basis =>
        basis.Distorted((point, field) => basis.Dimensions.Cascade(point, basis.Octaves, 0f, 1f, field)),
        static basis => basis.Dimensions.CascadeBound(basis.Octaves, 0f, 1f) switch { var bound => Fin.Succ((-bound, bound)) });
    public static readonly BasisKind HeteroTerrain = new("hetero-terrain", static basis =>
        basis.Distorted((point, field) => basis.Dimensions.Cascade(point, basis.Octaves, basis.FractalOffset, 0f, field)),
        static basis => basis.Dimensions.CascadeBound(basis.Octaves, basis.FractalOffset, 0f) switch { var bound => Fin.Succ((-bound, bound)) });
    public static readonly BasisKind HybridMultifractal = new("hybrid-multifractal", static basis =>
        basis.Distorted((point, field) => basis.Dimensions.HybridMultifractal(point, basis.Octaves, basis.FractalOffset, basis.FractalGain, field)),
        static basis => basis.Dimensions.HybridBound(basis.Octaves, basis.FractalOffset, basis.FractalGain) switch { var bound => Fin.Succ((-bound, bound)) });
    public static readonly BasisKind RidgedMultifractal = new("ridged-multifractal", static basis =>
        basis.Distorted((point, field) => basis.Dimensions.RidgedMultifractal(point, basis.Octaves, basis.FractalOffset, basis.FractalGain, field)),
        static basis => Fin.Succ((0f, basis.Dimensions.RidgedBound(basis.Octaves, basis.FractalOffset, basis.FractalGain))));
    public static readonly BasisKind F1 = new("f1", static basis => (point, field) => basis.Dimensions.F1(point, basis.Octaves, basis.Cellular, AxisFraction.MinValue, field),
        static basis => Fin.Succ((0f, basis.Dimensions.CellBound(basis.Octaves, basis.Cellular, 0f, 1f))));
    public static readonly BasisKind F2 = new("f2", static basis => (point, field) => basis.Dimensions.F2(point, basis.Octaves, basis.Cellular, field),
        static basis => Fin.Succ((0f, basis.Dimensions.CellBound(basis.Octaves, basis.Cellular, 0f, 2f))));
    public static readonly BasisKind SmoothF1 = new("smooth-f1", static basis => (point, field) =>
        basis.Dimensions.F1(point, basis.Octaves, basis.Cellular, basis.CellSmoothness, field),
        static basis => basis.Dimensions.CellBound(basis.Octaves, basis.Cellular, basis.CellSmoothness / 2f, 1f) switch { var bound => Fin.Succ((-bound, bound)) });
    public static readonly BasisKind EdgeDistance = new("edge-distance", static basis => (point, field) =>
        new(basis.Dimensions.EdgeDistance(point, basis.Octaves, basis.Cellular.Randomness, field), point),
        static basis => basis.Octaves.Lacunarity == FractalLacunarity.MinValue && basis.Octaves.Detail > FractalDetail.MinValue && basis.Octaves.Roughness > AxisFraction.MinValue
            ? new InvalidGenerator() : Fin.Succ(basis.Dimensions.EdgeRange(basis.Octaves, basis.Cellular.Randomness)));
    public static readonly BasisKind Gabor = new("gabor", static basis => (point, field) =>
        new(0.5f + (0.5f * basis.Dimensions.Gabor(point, basis.GaborFrequency, basis.GaborAnisotropy, basis.GaborOrientation, field).Y), point),
        static basis => basis.Dimensions.GaborBound switch { var bound => Fin.Succ((0.5f - (0.5f * bound), 0.5f + (0.5f * bound))) });
    public static readonly BasisKind Magic = new("magic", static basis => (point, _) => new(Vector3.Sum(NoiseFunctions.Magic(point, basis.MagicDepth, basis.Distortion)) / 3f, point),
        static basis => NoiseFunctions.MagicRange(basis.MagicDepth, basis.Distortion));
    public static readonly BasisKind Wave = new("wave", static basis => (point, field) =>
        new(basis.Dimensions.Wave(point, basis.WaveForm, basis.WaveProfile, basis.WavePhase, basis.Distortion, basis.Octaves, basis.WaveDetailScale, field), point), static _ => Fin.Succ((0f, 1f)));
    public static readonly BasisKind White = new("white", static basis => (point, field) => new(basis.Dimensions.White(point, field).X, point), static _ => Fin.Succ((0f, 1f)));

    [UseDelegateFromConstructor]
    internal partial Func<(ERational X, ERational Y, ERational Z, ERational W), uint, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)>> Sampler(NoiseBasis basis);

    [UseDelegateFromConstructor]
    internal partial Fin<(float Lower, float Upper)> Range(NoiseBasis basis);
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

internal sealed record GaborKernel(Neighborhood Cells, Func<float, float, Vector4, Vector4> Orient) {
    internal const uint Impulses = 8u;
    internal float Normalization => 6f * MathF.Sqrt(Impulses * 0.5f * float.Exp2(-1f - (Cells.Rank / 2f)));
}

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
    internal float SlopePeak => Enumerable.Range(0, 32).Max(top => Cells.Corners.Max(corner => MathF.Abs(Slope((uint)top << 27, ((2f * corner) - Vector4.One) * Cells.Live))));
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
    public static readonly TimingParameter Clock = new("clock", StateParameter.Bounded<Timing, Clock, float, InvalidGenerator>(
        Lens<Timing, Clock>.New(static timing => timing.Clock, static clock => timing => timing with { Clock = clock }), new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) }));
    public static readonly TimingParameter Pace = new("pace", StateParameter.Bounded<Timing, Pace, float, InvalidGenerator>(
        Lens<Timing, Pace>.New(static timing => timing.Pace, static pace => timing => timing with { Pace = pace }), new() { Soft = (0f, 4f) }));

    public StateParameter<Timing> Kind { get; }
}

public sealed record Octaves(FractalDetail Detail, AxisFraction Roughness, FractalLacunarity Lacunarity) : IStateRecord<Octaves, OctavesParameter, InvalidGenerator> {
    public static Octaves Default { get; } = new(FractalDetail.Standard, AxisFraction.Half, FractalLacunarity.Standard);
    public static Octaves Plain { get; } = Default with { Detail = FractalDetail.MinValue };

    internal float Weight(int octave) => float.Min(1f, Detail - (octave - 1));
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OctavesParameter : IStateParameter<Octaves> {
    public static readonly OctavesParameter Detail = new("detail", StateParameter.Bounded<Octaves, FractalDetail, float, InvalidGenerator>(
        Lens<Octaves, FractalDetail>.New(static octaves => octaves.Detail, static detail => octaves => octaves with { Detail = detail }), new()));
    public static readonly OctavesParameter Roughness = new("roughness", StateParameter.Bounded<Octaves, AxisFraction, float, InvalidGrade>(
        Lens<Octaves, AxisFraction>.New(static octaves => octaves.Roughness, static roughness => octaves => octaves with { Roughness = roughness }), new()));
    public static readonly OctavesParameter Lacunarity = new("lacunarity", StateParameter.Bounded<Octaves, FractalLacunarity, float, InvalidGenerator>(
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
    public static readonly CellularParameter Exponent = new("exponent", StateParameter.Bounded<Cellular, MinkowskiExponent, float, InvalidGenerator>(
        Lens<Cellular, MinkowskiExponent>.New(static cells => cells.Exponent, static exponent => cells => cells with { Exponent = exponent }), new() { Scale = TrackScale.Log }));
    public static readonly CellularParameter Randomness = new("randomness", StateParameter.Bounded<Cellular, AxisFraction, float, InvalidGrade>(
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

    internal Func<(ERational X, ERational Y, ERational Z, ERational W), uint, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)>> Sampler => Kind.Sampler(this);

    internal Fin<(Func<(ERational X, ERational Y, ERational Z, ERational W), uint, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)>> Sample, (float Lower, float Upper) Range)> BoundedSampler =>
        Kind.Range(this).Map(range => (Kind.Sampler(this), range));

    internal Fin<Func<(ERational X, ERational Y, ERational Z, ERational W), uint, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)>>> SiteSampler =>
        (Kind == BasisKind.F1 || Kind == BasisKind.F2 || Kind == BasisKind.SmoothF1)
            && Octaves.Lacunarity == FractalLacunarity.MinValue && Octaves.Detail > FractalDetail.MinValue && Octaves.Roughness > AxisFraction.MinValue
                ? new InvalidGenerator() : Kind.Sampler(this);

    internal Func<(ERational X, ERational Y, ERational Z, ERational W), uint, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)>> Distorted(Func<(ERational X, ERational Y, ERational Z, ERational W), uint, float> sum) =>
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
    public static readonly NoiseBasisParameter Distortion = new("distortion", StateParameter.Bounded<NoiseBasis, NoiseDistortion, float, InvalidGenerator>(
        Lens<NoiseBasis, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion }), new() { Origin = 0f }));
    public static readonly NoiseBasisParameter Cellular = new("cellular", new StateParameter<NoiseBasis>.Record<Cellular>(
        Lens<NoiseBasis, Cellular>.New(static basis => basis.Cellular, static cells => basis => basis with { Cellular = cells })));
    public static readonly NoiseBasisParameter Offset = new("offset", StateParameter.Bounded<NoiseBasis, FractalOffset, float, InvalidGenerator>(
        Lens<NoiseBasis, FractalOffset>.New(static basis => basis.FractalOffset, static offset => basis => basis with { FractalOffset = offset }), new() { Origin = 0f }));
    public static readonly NoiseBasisParameter Gain = new("gain", StateParameter.Bounded<NoiseBasis, FractalGain, float, InvalidGenerator>(
        Lens<NoiseBasis, FractalGain>.New(static basis => basis.FractalGain, static gain => basis => basis with { FractalGain = gain }), new()));
    public static readonly NoiseBasisParameter Smoothness = new("smoothness", StateParameter.Bounded<NoiseBasis, AxisFraction, float, InvalidGrade>(
        Lens<NoiseBasis, AxisFraction>.New(static basis => basis.CellSmoothness, static smoothness => basis => basis with { CellSmoothness = smoothness }), new()));
    public static readonly NoiseBasisParameter Frequency = new("frequency", StateParameter.Bounded<NoiseBasis, GaborFrequency, float, InvalidGenerator>(
        Lens<NoiseBasis, GaborFrequency>.New(static basis => basis.GaborFrequency, static frequency => basis => basis with { GaborFrequency = frequency }), new() { Scale = TrackScale.Log }));
    public static readonly NoiseBasisParameter Anisotropy = new("anisotropy", StateParameter.Bounded<NoiseBasis, AxisFraction, float, InvalidGrade>(
        Lens<NoiseBasis, AxisFraction>.New(static basis => basis.GaborAnisotropy, static anisotropy => basis => basis with { GaborAnisotropy = anisotropy }), new()));
    public static readonly NoiseBasisParameter Orientation = new("orientation", StateParameter.Bounded<NoiseBasis, SignedAngle, float, InvalidPixelValue>(
        Lens<NoiseBasis, SignedAngle>.New(static basis => basis.GaborOrientation, static orientation => basis => basis with { GaborOrientation = orientation }), SignedAngle.Presentation));
    public static readonly NoiseBasisParameter Depth = new("depth", StateParameter.Bounded<NoiseBasis, MagicDepth, int, InvalidGenerator>(
        Lens<NoiseBasis, MagicDepth>.New(static basis => basis.MagicDepth, static depth => basis => basis with { MagicDepth = depth }), new() { Step = 1 }));
    public static readonly NoiseBasisParameter Form = new("form", new StateParameter<NoiseBasis>.Choice<WaveForm, InvalidGenerator>(
        Lens<NoiseBasis, WaveForm>.New(static basis => basis.WaveForm, static form => basis => basis with { WaveForm = form })));
    public static readonly NoiseBasisParameter Profile = new("profile", new StateParameter<NoiseBasis>.Choice<WaveProfile, InvalidGenerator>(
        Lens<NoiseBasis, WaveProfile>.New(static basis => basis.WaveProfile, static profile => basis => basis with { WaveProfile = profile })));
    public static readonly NoiseBasisParameter Phase = new("phase", StateParameter.Bounded<NoiseBasis, SignedAngle, float, InvalidPixelValue>(
        Lens<NoiseBasis, SignedAngle>.New(static basis => basis.WavePhase, static phase => basis => basis with { WavePhase = phase }), SignedAngle.Presentation));
    public static readonly NoiseBasisParameter DetailScale = new("detail-scale", StateParameter.Bounded<NoiseBasis, WaveDetailScale, float, InvalidGenerator>(
        Lens<NoiseBasis, WaveDetailScale>.New(static basis => basis.WaveDetailScale, static scale => basis => basis with { WaveDetailScale = scale }), new()));

    public StateParameter<NoiseBasis> Kind { get; }
}

internal readonly record struct NoiseSample<T>(float Value, T Site);

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

    public static Vector4 Uniforms(long column, long line, uint field) =>
        NoiseFunctions.Uniforms(unchecked(Pcg3d((uint)column, (uint)line, Pcg3d((uint)(column >> 32), (uint)(line >> 32), field).X)));

    public static Vector4 Uniforms(EInteger column, EInteger line, uint field) {
        const int wordBits = sizeof(uint) * 8;
        const int rasterBits = sizeof(long) * 8;
        (EInteger x, EInteger y) = (column.ShiftRight(rasterBits), line.ShiftRight(rasterBits));
        for (long words = (long.Max(column.GetSignedBitLengthAsInt64(), line.GetSignedBitLengthAsInt64()) / wordBits) - (rasterBits / wordBits) + 1; words > 0; words--) {
            field = Pcg3d(x.ToUInt32Unchecked(), y.ToUInt32Unchecked(), field).X;
            (x, y) = (x.ShiftRight(wordBits), y.ShiftRight(wordBits));
        }
        return Uniforms(column.ToInt64Unchecked(), line.ToInt64Unchecked(), field);
    }

    public static Vector4 Normals(long column, long line, uint field) => Normals(Uniforms(column, line, field));

    public static Vector4 Normals(EInteger column, EInteger line, uint field) => Normals(Uniforms(column, line, field));

    private static Vector4 Normals(Vector4 uniforms) =>
        new((float)Normal.InvCDF(0d, 1d, uniforms.X), (float)Normal.InvCDF(0d, 1d, uniforms.Y), (float)Normal.InvCDF(0d, 1d, uniforms.Z), (float)Normal.InvCDF(0d, 1d, uniforms.W));
}

internal static class NoiseFunctions {
    // --- [DRAWS]
    private const int GradientRepeat = 100000;

    public static (uint X, uint Y, uint Z) Hash(Vector4 cell, uint field) =>
        unchecked(CoordinateHash.Pcg3d((uint)(int)cell.X, (uint)(int)cell.Y, CoordinateHash.Pcg3d((uint)(int)cell.Z, (uint)(int)cell.W, field).X));

    public static (uint X, uint Y, uint Z) Hash((EInteger X, EInteger Y, EInteger Z, EInteger W) cell, uint field) {
        const int wordBits = sizeof(uint) * 8;
        (EInteger? x, EInteger? y, EInteger? z, EInteger? w) = (cell.X.ShiftRight(wordBits), cell.Y.ShiftRight(wordBits), cell.Z.ShiftRight(wordBits), cell.W.ShiftRight(wordBits));
        long bits = long.Max(long.Max(cell.X.GetSignedBitLengthAsInt64(), cell.Y.GetSignedBitLengthAsInt64()), long.Max(cell.Z.GetSignedBitLengthAsInt64(), cell.W.GetSignedBitLengthAsInt64()));
        for (long words = bits / wordBits; words > 0; words--) {
            field = CoordinateHash.Pcg3d(x.ToUInt32Unchecked(), y.ToUInt32Unchecked(), CoordinateHash.Pcg3d(z.ToUInt32Unchecked(), w.ToUInt32Unchecked(), field).X).X;
            (x, y, z, w) = (x.ShiftRight(wordBits), y.ShiftRight(wordBits), z.ShiftRight(wordBits), w.ShiftRight(wordBits));
        }
        return CoordinateHash.Pcg3d(cell.X.ToUInt32Unchecked(), cell.Y.ToUInt32Unchecked(), CoordinateHash.Pcg3d(cell.Z.ToUInt32Unchecked(), cell.W.ToUInt32Unchecked(), field).X);
    }

    public static Vector4 White(Vector4 point, uint field) =>
        Uniforms(Hash(Vector4.Round(point, MidpointRounding.ToNegativeInfinity), field));

    public static Vector4 White((EInteger X, EInteger Y, EInteger Z, EInteger W) cell, uint field) => Uniforms(Hash(cell, field));

    public static Vector4 White((ERational X, ERational Y, ERational Z, ERational W) point, uint field) =>
        White((Split(point.X).Cell, Split(point.Y).Cell, Split(point.Z).Cell, Split(point.W).Cell), field);

    public static Vector4 Uniforms((uint X, uint Y, uint Z) words) =>
        words switch {
            var (x, y, z) => (new Vector4(x >> 9, y >> 9, z >> 9, CoordinateHash.Pcg3d(x, y, z).X >> 9) + Vector4.Create(0.5f)) * (1f / (1 << 23)),
        };

    private static Vector4 GradientShift(uint field) => 100f * (Vector4.One + White(Vector4.Zero, field));

    private static (ERational X, ERational Y, ERational Z, ERational W) Scale((ERational X, ERational Y, ERational Z, ERational W) point, ERational scale) =>
        (point.X.Multiply(scale), point.Y.Multiply(scale), point.Z.Multiply(scale), point.W.Multiply(scale));

    private static (ERational X, ERational Y, ERational Z, ERational W) Live((ERational X, ERational Y, ERational Z, ERational W) point, Vector4 axes) =>
        (point.X.Multiply((int)axes.X), point.Y.Multiply((int)axes.Y), point.Z.Multiply((int)axes.Z), point.W.Multiply((int)axes.W));

    private static (ERational X, ERational Y, ERational Z, ERational W) Position((EInteger X, EInteger Y, EInteger Z, EInteger W) cell, Vector4 local) =>
        (ERational.FromEInteger(cell.X).Add(ERational.FromSingle(local.X)), ERational.FromEInteger(cell.Y).Add(ERational.FromSingle(local.Y)),
            ERational.FromEInteger(cell.Z).Add(ERational.FromSingle(local.Z)), ERational.FromEInteger(cell.W).Add(ERational.FromSingle(local.W)));

    // --- [FRACTALS]
    internal static readonly float Peak = NoiseDimensions.Items.Max(static lattice => lattice.Scale * lattice.SlopePeak);
    internal static readonly float Ceiling = float.ScaleB(1f, float.ILogB(float.MaxValue));
    internal static readonly float Reach = ((Frequency.MaxValue * (float)Clock.MaxValue) + (NoiseDistortion.MaxValue * Peak)) * WaveDetailScale.MaxValue;

    private static float RoundBound(EFloat magnitude, int operations) {
        EContext precision = EContext.Binary64.WithRounding(ERounding.Ceiling).WithUnlimitedExponents();
        EFloat roundoff = EFloat.FromSingle(float.BitIncrement(1f) - 1f).Divide(2);
        EFloat growth = EFloat.One.Divide(EFloat.One.Subtract(roundoff), precision).Pow(operations, precision);
        EFloat underflow = EFloat.FromSingle(float.Epsilon).Divide(2).Multiply(growth.Subtract(EFloat.One)).Divide(roundoff, precision);
        return magnitude.Multiply(growth, precision).Add(underflow, precision).RoundToPrecision(EContext.Binary32.WithRounding(ERounding.Ceiling)).ToSingle();
    }

    // --- [CELLS]
    public static Vector2 SiteOffset(EInteger column, EInteger line, AxisFraction randomness, uint field) =>
        (CoordinateHash.Uniforms(column, line, field) * randomness).AsVector2();

    public static (float Value, (double X, double Y) Site) F1(double x, double y, Cellular cellular, uint field) =>
        F1(ERational.FromDouble(x), ERational.FromDouble(y), cellular, field) switch {
            var feature => (feature.Value, (feature.Site.X.ToDouble(), feature.Site.Y.ToDouble())),
        };

    public static (float Value, (ERational X, ERational Y) Site, (ERational X, ERational Y) Delta, (EInteger X, EInteger Y) Cell) F1(
        ERational x, ERational y, Cellular cellular, uint field) {
        ((EInteger? column, ERational? localX), (EInteger? line, ERational? localY)) = (Split(x), Split(y));
        (ERational floorX, ERational floorY) = (ERational.FromEInteger(column), ERational.FromEInteger(line));
        (ERational X, ERational Y) = (localX, localY);
        NoiseDimensions lattice = NoiseDimensions.Two;
        Span<Vector4> reach = stackalloc Vector4[lattice.Cells.Near.AsSpan().Length];
        (NoiseSample<Vector4> first, NoiseSample<Vector4> _, Vector4 selectedOffset) = lattice.Ranked(new Vector4(X.ToSingle(), Y.ToSingle(), 0f, 0f), cellular,
            (Column: column, Line: line, cellular.Randomness, Field: field), static (origin, offset) =>
                offset + new Vector4(SiteOffset(origin.Column.Add((int)offset.X), origin.Line.Add((int)offset.Y), origin.Randomness, origin.Field), 0f, 0f), reach);
        (ERational X, ERational Y) site = (ERational.FromSingle(first.Site.X), ERational.FromSingle(first.Site.Y));
        return (first.Value, (floorX.Add(site.X), floorY.Add(site.Y)), (site.X.Subtract(X), site.Y.Subtract(Y)),
            (column.Add((int)selectedOffset.X), line.Add((int)selectedOffset.Y)));
    }

    internal static (EInteger Cell, ERational Local) Split(ERational coordinate) => EFloat.FromEInteger(coordinate.Numerator)
        .DivideToExponent(EFloat.FromEInteger(coordinate.Denominator), 0L, ERounding.Floor).ToEInteger() switch {
            var cell => (cell, coordinate.Subtract(ERational.FromEInteger(cell))),
        };

    private static ((EInteger X, EInteger Y, EInteger Z, EInteger W) Cell, Vector4 Local) Split((ERational X, ERational Y, ERational Z, ERational W) point) =>
        (Split(point.X), Split(point.Y), Split(point.Z), Split(point.W)) switch {
            var (x, y, z, w) => ((x.Cell, y.Cell, z.Cell, w.Cell), new(x.Local.ToSingle(), y.Local.ToSingle(), z.Local.ToSingle(), w.Local.ToSingle())),
        };

    private static float Edge(ReadOnlySpan<Vector4> reach) {
        Vector4 closest = reach[0];
        foreach (Vector4 toward in reach)
            closest = toward.LengthSquared() < closest.LengthSquared() ? toward : closest;
        float edge = float.MaxValue;
        foreach (Vector4 toward in reach)
            edge = (toward - closest).LengthSquared() > 0.0001f ? float.Min(edge, Vector4.Dot((closest + toward) / 2f, Vector4.Normalize(toward - closest))) : edge;
        return edge;
    }

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

    public static Vector3 Magic(Vector4 point, MagicDepth depth, NoiseDistortion distortion) =>
        Magic((point - (Vector4.Tau * Vector4.Round(point / float.Tau, MidpointRounding.ToNegativeInfinity))).AsVector3(), depth, distortion);

    public static Vector3 Magic((ERational X, ERational Y, ERational Z, ERational W) point, MagicDepth depth, NoiseDistortion distortion) {
        ERational period = ERational.FromSingle(float.Tau);
        return Magic(new Vector3(Split(point.X.Divide(period)).Local.Multiply(period).ToSingle(), Split(point.Y.Divide(period)).Local.Multiply(period).ToSingle(),
            Split(point.Z.Divide(period)).Local.Multiply(period).ToSingle()), depth, distortion);
    }

    private static Vector3 Magic(Vector3 p, MagicDepth depth, NoiseDistortion distortion) {
        Vector3 turned = new Vector3(MathF.Sin((p.X + p.Y + p.Z) * 5f), MathF.Cos((-p.X + p.Y - p.Z) * 5f), -MathF.Cos((-p.X - p.Y + p.Z) * 5f))
            * (depth == MagicDepth.MinValue ? 1f : distortion);
        foreach (Func<Vector3, float, Vector3> step in Turbulence.AsSpan(0, depth))
            turned = step(turned, distortion);
        return Magic(turned, distortion);
    }

    private static Vector3 Magic(Vector3 turned, NoiseDistortion distortion) =>
        new Vector3(0.5f) - (distortion == NoiseDistortion.Neutral ? turned : turned / (2f * distortion));

    internal static Fin<(float Lower, float Upper)> MagicRange(MagicDepth depth, NoiseDistortion distortion) {
        Vector3 peak = Vector3.Create(depth == MagicDepth.MinValue ? 1f : float.Abs(distortion));
        (float first, float second) = (Vector3.Sum(Magic(peak, distortion)) / 3f, Vector3.Sum(Magic(-peak, distortion)) / 3f);
        return float.IsFinite(first) && float.IsFinite(second) ? Fin.Succ((float.Min(first, second), float.Max(first, second))) : new InvalidGenerator();
    }

    extension(NoiseDimensions lattice) {
        // --- [DRAWS]
        public Vector4 White((ERational X, ERational Y, ERational Z, ERational W) point, uint field) => White(Live(point, lattice.Cells.Live), field);

        public float Gradient(Vector4 point, uint field) {
            Vector4 wrapped = (point - (GradientRepeat * Vector4.Truncate(point / GradientRepeat))
                + Vector4.ConditionalSelect(Vector4.GreaterThanOrEqual(Vector4.Abs(point), Vector4.Create(10f * GradientRepeat)), Vector4.Create(0.5f), Vector4.Zero)
                + GradientShift(field)) * lattice.Cells.Live;
            Vector4 cell = Vector4.Round(wrapped, MidpointRounding.ToNegativeInfinity);
            return lattice.Gradient(cell, wrapped - cell, field);
        }

        public float Gradient((ERational X, ERational Y, ERational Z, ERational W) point, uint field) {
            ReadOnlySpan<ERational> coordinates = [point.X, point.Y, point.Z, point.W];
            Vector4 shift = GradientShift(field);
            (Vector4 cell, Vector4 local) = (Vector4.Zero, Vector4.Zero);
            for (int axis = 0; axis < lattice.Cells.Rank; axis++) {
                ERational coordinate = coordinates[axis];
                ERational wrapped = coordinate.Remainder(GradientRepeat)
                    .Add(ERational.FromSingle(coordinate.Abs().CompareTo(10 * GradientRepeat) >= 0 ? 0.5f : 0f))
                    .Add(ERational.FromSingle(shift[axis]));
                (EInteger Cell, ERational Local) split = Split(wrapped);
                (cell[axis], local[axis]) = (split.Cell.ToInt32Checked(), split.Local.ToSingle());
            }
            return lattice.Gradient(cell, local, field);
        }

        private float Gradient(Vector4 cell, Vector4 local, uint field) {
            Vector4 fade = new((float)RampInterpolation.SmootherRgb.Weight(local.X), (float)RampInterpolation.SmootherRgb.Weight(local.Y), (float)RampInterpolation.SmootherRgb.Weight(local.Z), (float)RampInterpolation.SmootherRgb.Weight(local.W));
            float level = 0f;
            foreach (Vector4 corner in lattice.Cells.Corners.AsSpan()) {
                Vector4 weights = Vector4.Lerp(Vector4.One - fade, fade, corner);
                level += weights.X * weights.Y * weights.Z * weights.W * lattice.Slope(Hash(cell + corner, field).X, local - corner);
            }
            return lattice.Scale * level;
        }

        // --- [BOUNDS]
        private float GradientBound => RoundBound(EFloat.FromSingle(lattice.Scale).Multiply(EFloat.FromSingle(lattice.SlopePeak)), lattice.Cells.Rank + lattice.Cells.Corners.Count + 4);

        public float FbmBound(Octaves octaves) => lattice.Fbm(lattice.GradientBound, 1f, octaves, 0u,
            static (scale, _) => scale, static (_, peak, _, _) => peak);

        public float CascadeBound(Octaves octaves, float offset, float lead) => lattice.Cascade(lattice.GradientBound, octaves, float.Abs(offset), lead, 0u,
            static (peak, _) => peak, static (_, peak, _) => peak);

        public float HybridBound(Octaves octaves, float offset, float gain) => lattice.HybridMultifractal(lattice.GradientBound, octaves, float.Abs(offset), gain, 0u,
            static (peak, _) => peak, static (_, peak, _) => peak);

        public float RidgedBound(Octaves octaves, float offset, float gain) => lattice.RidgedMultifractal(float.Abs(offset) + lattice.GradientBound, octaves, 0f, gain, 0u,
            static (peak, _) => peak, static (_, peak, _) => peak);

        public float CellBound(Octaves octaves, Cellular cellular, float smoothness, float span) {
            float reach = (smoothness == 0f ? lattice.Cells.Near : lattice.Cells.Wide).Select(offset => cellular.Metric.Length(
                Vector4.Max(Vector4.Abs(offset - lattice.Cells.Live), Vector4.Abs(offset + (cellular.Randomness * lattice.Cells.Live))), cellular.Exponent))
                .Order().ElementAt(smoothness == 0f ? (int)span - 1 : lattice.Cells.Wide.Count - 1);
            float peak = smoothness == 0f ? reach : RoundBound(EFloat.FromSingle(reach).Add(EFloat.FromSingle(smoothness).Multiply(lattice.Cells.Wide.Count).Divide(4)), 8 * lattice.Cells.Wide.Count);
            return lattice.Fractal(peak, 0f, 1f, octaves, cellular, smoothness, span, 0u,
                static (at, _) => at, static (scale, _) => scale, static (_, _, _, _) => 0f,
                static (_, at, _, _, _) => new NoiseSample<float>(at, 0f)).Value;
        }

        public (float Lower, float Upper) EdgeRange(Octaves octaves, AxisFraction randomness) =>
            RoundBound(EFloat.FromInt32(2 * lattice.Cells.Rank), (3 * lattice.Cells.Rank) + 1) switch {
                var bound => (lattice.EdgeDistance(-bound, octaves, randomness, 0u, static (at, _) => at, static (_, at, _, _) => at),
                    lattice.EdgeDistance(bound, octaves, randomness, 0u, static (at, _) => at, static (_, at, _, _) => at)),
            };

        public float GaborBound => GaborKernel.Impulses * lattice.Gabor.Cells.Near.Count / lattice.Gabor.Normalization;

        // --- [FRACTALS]
        public float Fbm(Vector4 point, Octaves octaves, uint field) =>
            lattice.Fbm(point, 1f, octaves, field, static (scale, lacunarity) => scale * lacunarity,
                static (dimensions, at, scale, draw) => dimensions.Gradient(scale * at, draw));

        public float Fbm((ERational X, ERational Y, ERational Z, ERational W) point, Octaves octaves, uint field) =>
            lattice.Fbm(point, ERational.One, octaves, field, static (scale, lacunarity) => scale.Multiply(ERational.FromSingle(lacunarity)),
                static (dimensions, at, scale, draw) => dimensions.Gradient(Scale(at, scale), draw));

        private float Fbm<T, TScale>(T point, TScale scale, Octaves octaves, uint field, Func<TScale, float, TScale> advance, Func<NoiseDimensions, T, TScale, uint, float> sample) {
            (float sum, float total, float level, float amplitude) = (0f, 0f, 0f, 1f);
            for (int octave = 0; octaves.Weight(octave) > 0f; octave++) {
                (sum, total) = (sum + (sample(lattice, point, scale, field) * amplitude), total + amplitude);
                (level, amplitude, scale) = (float.Lerp(level, sum / total, octaves.Weight(octave)), amplitude * octaves.Roughness, advance(scale, octaves.Lacunarity));
            }
            return level;
        }

        public float Cascade(Vector4 point, Octaves octaves, float offset, float lead, uint field) =>
            lattice.Cascade(point, octaves, offset, lead, field, static (at, lacunarity) => at * lacunarity, static (dimensions, at, draw) => dimensions.Gradient(at, draw));

        public float Cascade((ERational X, ERational Y, ERational Z, ERational W) point, Octaves octaves, float offset, float lead, uint field) =>
            lattice.Cascade(point, octaves, offset, lead, field, static (at, lacunarity) => Scale(at, ERational.FromSingle(lacunarity)), static (dimensions, at, draw) => dimensions.Gradient(at, draw));

        private float Cascade<T>(T point, Octaves octaves, float offset, float lead, uint field, Func<T, float, T> advance, Func<NoiseDimensions, T, uint, float> sample) {
            (float value, float amplitude) = (1f, 1f);
            for (int octave = 0; octaves.Weight(octave) > 0f; octave++)
                (value, amplitude, point) = (value * ((octave == 0 ? lead : 1f) + (octaves.Weight(octave) * amplitude * (sample(lattice, point, field) + offset))), amplitude * octaves.Roughness, advance(point, octaves.Lacunarity));
            return value;
        }

        public float HybridMultifractal(Vector4 point, Octaves octaves, FractalOffset offset, FractalGain gain, uint field) =>
            lattice.HybridMultifractal(point, octaves, offset, gain, field, static (at, lacunarity) => at * lacunarity, static (dimensions, at, draw) => dimensions.Gradient(at, draw));

        public float HybridMultifractal((ERational X, ERational Y, ERational Z, ERational W) point, Octaves octaves, FractalOffset offset, FractalGain gain, uint field) =>
            lattice.HybridMultifractal(point, octaves, offset, gain, field, static (at, lacunarity) => Scale(at, ERational.FromSingle(lacunarity)), static (dimensions, at, draw) => dimensions.Gradient(at, draw));

        private float HybridMultifractal<T>(T point, Octaves octaves, float offset, float gain, uint field, Func<T, float, T> advance, Func<NoiseDimensions, T, uint, float> sample) {
            (float value, float weight, float amplitude) = (0f, 1f, 1f);
            for (int octave = 0; weight > 0.001f && octaves.Weight(octave) > 0f; octave++) {
                (float held, float signal) = (float.Min(weight, 1f), (sample(lattice, point, field) + offset) * amplitude);
                (value, weight, amplitude, point) = (value + (octaves.Weight(octave) * held * signal), held * gain * signal, amplitude * octaves.Roughness, advance(point, octaves.Lacunarity));
            }
            return value;
        }

        public float RidgedMultifractal(Vector4 point, Octaves octaves, FractalOffset offset, FractalGain gain, uint field) =>
            lattice.RidgedMultifractal(point, octaves, offset, gain, field, static (at, lacunarity) => at * lacunarity, static (dimensions, at, draw) => dimensions.Gradient(at, draw));

        public float RidgedMultifractal((ERational X, ERational Y, ERational Z, ERational W) point, Octaves octaves, FractalOffset offset, FractalGain gain, uint field) =>
            lattice.RidgedMultifractal(point, octaves, offset, gain, field, static (at, lacunarity) => Scale(at, ERational.FromSingle(lacunarity)), static (dimensions, at, draw) => dimensions.Gradient(at, draw));

        private float RidgedMultifractal<T>(T point, Octaves octaves, float offset, float gain, uint field, Func<T, float, T> advance, Func<NoiseDimensions, T, uint, float> sample) {
            (float signal, float value, float amplitude) = (0f, 0f, 1f);
            for (int octave = 0; octaves.Weight(octave) >= 1f; octave++) {
                float ridge = offset - MathF.Abs(sample(lattice, point, field));
                signal = ridge * ridge * (octave == 0 ? 1f : Easing.Saturate(signal * gain));
                (value, amplitude, point) = (value + (signal * amplitude), amplitude * octaves.Roughness, advance(point, octaves.Lacunarity));
            }
            return value;
        }

        public Vector4 Distort(Vector4 point, NoiseDistortion distortion, uint field) {
            Vector4 moved = point;
            for (int axis = 0; axis < (distortion == NoiseDistortion.Neutral ? 0 : lattice.Cells.Rank); axis++)
                moved[axis] += distortion * lattice.Gradient(point, CoordinateHash.Branch(field, (uint)axis));
            return moved;
        }

        public (ERational X, ERational Y, ERational Z, ERational W) Distort((ERational X, ERational Y, ERational Z, ERational W) point, NoiseDistortion distortion, uint field) {
            Span<ERational> moved = [point.X, point.Y, point.Z, point.W];
            for (int axis = 0; axis < (distortion == NoiseDistortion.Neutral ? 0 : lattice.Cells.Rank); axis++)
                moved[axis] = moved[axis].Add(ERational.FromSingle(distortion * lattice.Gradient(point, CoordinateHash.Branch(field, (uint)axis))));
            return (moved[0], moved[1], moved[2], moved[3]);
        }

        // --- [CELLS]
        public (float Value, float EdgeDistance, (EInteger X, EInteger Y, EInteger Z, EInteger W) Cell) Voronoi(
            (ERational X, ERational Y, ERational Z, ERational W) point, Cellular cellular, uint field) {
            Span<Vector4> reach = stackalloc Vector4[lattice.Cells.Near.AsSpan().Length];
            (NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> First, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> Second, (EInteger X, EInteger Y, EInteger Z, EInteger W) Cell) = lattice.Ranked(Live(point, lattice.Cells.Live), cellular, field, reach);
            return (First.Value, Edge(reach) / (0.5f + (0.5f * cellular.Randomness)), Cell);
        }

        public NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> F1((ERational X, ERational Y, ERational Z, ERational W) point,
            Octaves octaves, Cellular cellular, AxisFraction smoothness, uint field) =>
            smoothness == AxisFraction.MinValue
                ? lattice.Fractal(point, octaves, cellular, 0f, 1f, field, static (dimensions, at, cells, _, draw) => dimensions.Ranked(at, cells, draw).First)
                : lattice.Fractal(point, octaves, cellular, smoothness / 2f, 1f, field, static (dimensions, at, cells, blend, draw) => dimensions.Smooth(at, cells, blend, draw));

        public NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> F2((ERational X, ERational Y, ERational Z, ERational W) point,
            Octaves octaves, Cellular cellular, uint field) =>
            lattice.Fractal(point, octaves, cellular, 0f, 2f, field, static (dimensions, at, cells, _, draw) => dimensions.Ranked(at, cells, draw).Second);

        public NoiseSample<Vector4> F1(Vector4 point, Octaves octaves, Cellular cellular, AxisFraction smoothness, uint field) =>
            smoothness == AxisFraction.MinValue
                ? lattice.Fractal(point, octaves, cellular, 0f, 1f, field, static (lattice, at, cells, _, draw) => lattice.Ranked(at, cells, draw).First)
                : lattice.Fractal(point, octaves, cellular, smoothness / 2f, 1f, field, static (lattice, at, cells, blend, draw) => lattice.Smooth(at, cells, blend, draw));

        public NoiseSample<Vector4> F2(Vector4 point, Octaves octaves, Cellular cellular, uint field) =>
            lattice.Fractal(point, octaves, cellular, 0f, 2f, field, static (lattice, at, cells, _, draw) => lattice.Ranked(at, cells, draw).Second);

        public float EdgeDistance(Vector4 point, Octaves octaves, AxisFraction randomness, uint field) =>
            lattice.EdgeDistance(point * lattice.Cells.Live, 1f, octaves, randomness, field, static (at, scale) => at * scale,
                static (scale, lacunarity) => scale * lacunarity, static value => value, static (value, scale) => value / scale, float.Lerp, float.Min, static (value, bound) => value / bound,
                static (dimensions, at, jitter, draw) => dimensions.Edge(at, jitter, draw));

        public float EdgeDistance((ERational X, ERational Y, ERational Z, ERational W) point, Octaves octaves, AxisFraction randomness, uint field) =>
            lattice.EdgeDistance(Live(point, lattice.Cells.Live), octaves, randomness, field, static (at, scale) => Scale(at, scale),
                static (dimensions, at, jitter, draw) => dimensions.Edge(at, jitter, draw));

        private float EdgeDistance<TPoint>(TPoint point, Octaves octaves, AxisFraction randomness, uint field,
            Func<TPoint, ERational, TPoint> at, Func<NoiseDimensions, TPoint, AxisFraction, uint, float> sample) =>
            lattice.EdgeDistance(point, ERational.One, octaves, randomness, field, at,
                static (scale, lacunarity) => scale.Multiply(ERational.FromSingle(lacunarity)), ERational.FromSingle, static (value, scale) => ERational.FromSingle(value).Divide(scale),
                static (value, next, weight) => value.Multiply(ERational.FromSingle(1f - weight)).Add(next.Multiply(ERational.FromSingle(weight))),
                static (value, next) => value.IsNaN() || next.IsNaN() ? ERational.FromSingle(float.Min(value.ToSingle(), next.ToSingle())) : ERational.Min(value, next),
                static (value, bound) => value.Divide(bound).ToSingle(),
                sample);

        private float EdgeDistance<TPoint, TScale, TDistance>(TPoint point, TScale scale, Octaves octaves, AxisFraction randomness, uint field,
            Func<TPoint, TScale, TPoint> at, Func<TScale, float, TScale> advance, Func<float, TDistance> lift, Func<float, TScale, TDistance> divide,
            Func<TDistance, TDistance, float, TDistance> blend, Func<TDistance, TDistance, TDistance> minimum, Func<TDistance, TDistance, float> ratio,
            Func<NoiseDimensions, TPoint, AxisFraction, uint, float> sample) {
            float limit = 0.5f + (0.5f * randomness);
            (TDistance bound, TDistance distance) = (lift(limit), lift(8f));
            float amplitude = 1f;
            for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
                float weight = amplitude * octaves.Weight(octave);
                (bound, distance) = (blend(bound, divide(limit, scale), weight), blend(distance, minimum(distance, divide(sample(lattice, at(point, scale), randomness, field), scale)), weight));
                (amplitude, scale) = (amplitude * octaves.Roughness, advance(scale, octaves.Lacunarity));
            }
            return ratio(distance, bound);
        }

        private NoiseSample<Vector4> Fractal(Vector4 point, Octaves octaves, Cellular cellular, float smoothness, float span, uint field,
            Func<NoiseDimensions, Vector4, Cellular, float, uint, NoiseSample<Vector4>> scan) =>
            lattice.Fractal(point * lattice.Cells.Live, Vector4.Zero, 1f, octaves, cellular, smoothness, span, field, static (at, scale) => at * scale,
                static (scale, lacunarity) => scale * lacunarity, static (site, next, scale, weight) => Vector4.Lerp(site, next / scale, weight), scan);

        private NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> Fractal((ERational X, ERational Y, ERational Z, ERational W) point,
            Octaves octaves, Cellular cellular, float smoothness, float span, uint field,
            Func<NoiseDimensions, (ERational X, ERational Y, ERational Z, ERational W), Cellular, float, uint, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)>> scan) =>
            lattice.Fractal(Live(point, lattice.Cells.Live), (X: ERational.Zero, Y: ERational.Zero, Z: ERational.Zero, W: ERational.Zero), ERational.One, octaves, cellular, smoothness, span, field,
                static (at, scale) => Scale(at, scale), static (scale, lacunarity) => scale.Multiply(ERational.FromSingle(lacunarity)), static (site, next, scale, weight) => {
                    (ERational held, ERational blend) = (ERational.FromSingle(1f - weight), ERational.FromSingle(weight));
                    return (site.X.Multiply(held).Add(next.X.Divide(scale).Multiply(blend)), site.Y.Multiply(held).Add(next.Y.Divide(scale).Multiply(blend)),
                        site.Z.Multiply(held).Add(next.Z.Divide(scale).Multiply(blend)), site.W.Multiply(held).Add(next.W.Divide(scale).Multiply(blend)));
                }, scan);

        private NoiseSample<TSite> Fractal<TPoint, TSite, TScale>(TPoint point, TSite site, TScale scale, Octaves octaves, Cellular cellular, float smoothness, float span, uint field,
            Func<TPoint, TScale, TPoint> at, Func<TScale, float, TScale> advance, Func<TSite, TSite, TScale, float, TSite> blend,
            Func<NoiseDimensions, TPoint, Cellular, float, uint, NoiseSample<TSite>> scan) {
            float limit = span * cellular.Metric.Length((0.5f + (0.5f * cellular.Randomness)) * lattice.Cells.Live, cellular.Exponent);
            (float total, float distance, float amplitude) = (0f, 0f, 1f);
            for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
                NoiseSample<TSite> feature = scan(lattice, at(point, scale), cellular, smoothness, field);
                float weight = amplitude * octaves.Weight(octave);
                (total, distance, site) = (total + weight, distance + (feature.Value * weight), blend(site, feature.Site, scale, weight));
                (amplitude, scale) = (amplitude * octaves.Roughness, advance(scale, octaves.Lacunarity));
            }
            return new(distance / (total * limit), site);
        }

        private (NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> First, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> Second,
            (EInteger X, EInteger Y, EInteger Z, EInteger W) Cell) Ranked((ERational X, ERational Y, ERational Z, ERational W) point, Cellular cellular, uint field) {
            Span<Vector4> reach = stackalloc Vector4[lattice.Cells.Near.AsSpan().Length];
            return lattice.Ranked(point, cellular, field, reach);
        }

        private (NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> First, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> Second,
            (EInteger X, EInteger Y, EInteger Z, EInteger W) Cell) Ranked((ERational X, ERational Y, ERational Z, ERational W) point, Cellular cellular, uint field, Span<Vector4> reach) {
            ((EInteger X, EInteger Y, EInteger Z, EInteger W) cell, Vector4 local) = Split(point);
            (NoiseSample<Vector4> first, NoiseSample<Vector4> second, Vector4 offset) = lattice.Ranked(local, cellular, (Cell: cell, Lattice: lattice, cellular.Randomness, Field: field),
                static (origin, offset) => origin.Lattice.Site(origin.Cell, offset, origin.Randomness, origin.Field), reach);
            return (new(first.Value, Position(cell, first.Site)), new(second.Value, Position(cell, second.Site)),
                (cell.X.Add((int)offset.X), cell.Y.Add((int)offset.Y), cell.Z.Add((int)offset.Z), cell.W.Add((int)offset.W)));
        }

        private (NoiseSample<Vector4> First, NoiseSample<Vector4> Second) Ranked(Vector4 point, Cellular cellular, uint field) {
            Vector4 cell = Vector4.Round(point, MidpointRounding.ToNegativeInfinity);
            Span<Vector4> reach = stackalloc Vector4[lattice.Cells.Near.AsSpan().Length];
            (NoiseSample<Vector4> first, NoiseSample<Vector4> second, Vector4 _) = lattice.Ranked(point - cell, cellular,
                (Lattice: lattice, Cell: cell, cellular.Randomness, Field: field), static (origin, offset) => origin.Lattice.Site(origin.Cell, offset, origin.Randomness, origin.Field), reach);
            return (first with { Site = cell + first.Site }, second with { Site = cell + second.Site });
        }

        private (NoiseSample<Vector4> First, NoiseSample<Vector4> Second, Vector4 Offset) Ranked<T>(Vector4 local, Cellular cellular, T origin, Func<T, Vector4, Vector4> locate, Span<Vector4> reach) {
            (NoiseSample<Vector4> First, NoiseSample<Vector4> Second, Vector4 Offset) nearest = (new(float.MaxValue, Vector4.Zero), new(float.MaxValue, Vector4.Zero), Vector4.Zero);
            int index = 0;
            foreach (Vector4 offset in lattice.Cells.Near.AsSpan()) {
                Vector4 site = locate(origin, offset);
                reach[index] = site - local;
                float distance = cellular.Metric.Length(reach[index], cellular.Exponent);
                nearest = distance < nearest.First.Value ? (new(distance, site), nearest.First, offset)
                    : distance < nearest.Second.Value ? (nearest.First, new(distance, site), nearest.Offset)
                    : nearest;
                index++;
            }
            return nearest;
        }

        private NoiseSample<Vector4> Smooth(Vector4 point, Cellular cellular, float smoothness, uint field) {
            Vector4 cell = Vector4.Round(point, MidpointRounding.ToNegativeInfinity);
            NoiseSample<Vector4> sample = lattice.Smooth(point - cell, cellular, smoothness, (Lattice: lattice, Cell: cell, cellular.Randomness, Field: field),
                static (origin, offset) => origin.Lattice.Site(origin.Cell, offset, origin.Randomness, origin.Field));
            return sample with { Site = cell + sample.Site };
        }

        private NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> Smooth((ERational X, ERational Y, ERational Z, ERational W) point,
            Cellular cellular, float smoothness, uint field) {
            ((EInteger X, EInteger Y, EInteger Z, EInteger W) cell, Vector4 local) = Split(point);
            NoiseSample<Vector4> sample = lattice.Smooth(local, cellular, smoothness, (Lattice: lattice, Cell: cell, cellular.Randomness, Field: field),
                static (origin, offset) => origin.Lattice.Site(origin.Cell, offset, origin.Randomness, origin.Field));
            return new(sample.Value, Position(cell, sample.Site));
        }

        private NoiseSample<Vector4> Smooth<T>(Vector4 local, Cellular cellular, float smoothness, T origin, Func<T, Vector4, Vector4> locate) {
            ReadOnlySpan<Vector4> wide = lattice.Cells.Wide.AsSpan();
            (float distance, Vector4 site) = (0f, Vector4.Zero);
            for (int index = 0; index < wide.Length; index++) {
                Vector4 next = locate(origin, wide[index]);
                float reach = cellular.Metric.Length(next - local, cellular.Exponent);
                float blend = index == 0 ? 1f : (float)RampInterpolation.Ease.Weight(Easing.Saturate(0.5f + (0.5f * (distance - reach) / smoothness)));
                float correction = smoothness * blend * (1f - blend);
                (distance, site) = (float.Lerp(distance, reach, blend) - correction, Vector4.Lerp(site, next, blend) - (correction / (1f + (3f * smoothness)) * lattice.Cells.Live));
            }
            return new(distance, site);
        }

        private float Edge((ERational X, ERational Y, ERational Z, ERational W) point, AxisFraction randomness, uint field) {
            ((EInteger X, EInteger Y, EInteger Z, EInteger W) cell, Vector4 local) = Split(point);
            ReadOnlySpan<Vector4> near = lattice.Cells.Near.AsSpan();
            Span<Vector4> reach = stackalloc Vector4[near.Length];
            for (int index = 0; index < near.Length; index++)
                reach[index] = lattice.Site(cell, near[index], randomness, field) - local;
            return Edge(reach);
        }

        private float Edge(Vector4 point, AxisFraction randomness, uint field) {
            Vector4 cell = Vector4.Round(point, MidpointRounding.ToNegativeInfinity);
            ReadOnlySpan<Vector4> near = lattice.Cells.Near.AsSpan();
            Span<Vector4> reach = stackalloc Vector4[near.Length];
            for (int index = 0; index < near.Length; index++)
                reach[index] = lattice.Site(cell, near[index], randomness, field) - (point - cell);
            return Edge(reach);
        }

        private Vector4 Site(Vector4 cell, Vector4 offset, AxisFraction randomness, uint field) => offset + (White(cell + offset, field) * lattice.Cells.Live * randomness);

        private Vector4 Site((EInteger X, EInteger Y, EInteger Z, EInteger W) cell, Vector4 offset, AxisFraction randomness, uint field) =>
            offset + (White((cell.X.Add((int)offset.X), cell.Y.Add((int)offset.Y), cell.Z.Add((int)offset.Z), cell.W.Add((int)offset.W)), field) * lattice.Cells.Live * randomness);

        // --- [GABOR]
        public Vector2 Gabor(Vector4 point, GaborFrequency frequency, AxisFraction anisotropy, SignedAngle orientation, uint field) {
            Vector4 live = point * lattice.Gabor.Cells.Live;
            Vector4 cell = Vector4.Round(live, MidpointRounding.ToNegativeInfinity);
            return lattice.Gabor(cell, live - cell, frequency, anisotropy, orientation, field, static (at, neighbor, draw) => White(at + neighbor, draw));
        }

        public Vector2 Gabor((ERational X, ERational Y, ERational Z, ERational W) point, GaborFrequency frequency, AxisFraction anisotropy, SignedAngle orientation, uint field) {
            ((EInteger X, EInteger Y, EInteger Z, EInteger W) cell, Vector4 local) = Split(Live(point, lattice.Gabor.Cells.Live));
            return lattice.Gabor(cell, local, frequency, anisotropy, orientation, field, static (at, neighbor, draw) => White(
                (at.X.Add((int)neighbor.X), at.Y.Add((int)neighbor.Y), at.Z.Add((int)neighbor.Z), at.W.Add((int)neighbor.W)), draw));
        }

        private Vector2 Gabor<T>(T cell, Vector4 local, GaborFrequency frequency, AxisFraction anisotropy, SignedAngle orientation, uint field, Func<T, Vector4, uint, Vector4> white) {
            GaborKernel kernel = lattice.Gabor;
            Vector2 phasor = Vector2.Zero;
            for (uint impulse = 0u; impulse < GaborKernel.Impulses; impulse++) {
                (uint centers, uint turns) = (CoordinateHash.Branch(field, 2u * impulse), CoordinateHash.Branch(field, (2u * impulse) + 1u));
                foreach (Vector4 neighbor in kernel.Cells.Near.AsSpan()) {
                    Vector4 center = white(cell, neighbor, centers);
                    Vector4 reach = local - neighbor - (center * kernel.Cells.Live);
                    float spread = reach.LengthSquared();
                    (float sin, float cos) = spread < 1f
                        ? MathF.SinCos(float.Tau * frequency * Vector4.Dot(reach, kernel.Orient(orientation, 1f - anisotropy, white(cell, neighbor, turns))))
                        : (0f, 0f);
                    phasor += float.CopySign(MathF.Exp(-float.Pi * spread) * (0.5f + (0.5f * MathF.Cos(float.Pi * spread))), center.W - 0.5f) * new Vector2(cos, sin);
                }
            }
            return phasor / kernel.Normalization;
        }

        // --- [PATTERNS]
        public float Wave(Vector4 point, WaveForm form, WaveProfile profile, float phase, NoiseDistortion distortion, Octaves octaves, WaveDetailScale detailScale, uint field) =>
            profile.Level(form.Distance(point) - point.Z + ((phase + (distortion == NoiseDistortion.Neutral ? 0f
                : distortion * lattice.Fbm(new Vector4(point.AsVector2() * (float.Pi / 10f) * detailScale, 0f, 0f), octaves, CoordinateHash.Branch(field, 0u)))) / float.Tau));

        public float Wave((ERational X, ERational Y, ERational Z, ERational W) point, WaveForm form, WaveProfile profile, float phase,
            NoiseDistortion distortion, Octaves octaves, WaveDetailScale detailScale, uint field) {
            ERational detail = ERational.FromSingle(float.Pi / 10f).Multiply(ERational.FromSingle(detailScale));
            float bend = distortion == NoiseDistortion.Neutral ? 0f : distortion * lattice.Fbm(
                (point.X.Multiply(detail), point.Y.Multiply(detail), ERational.Zero, ERational.Zero), octaves, CoordinateHash.Branch(field, 0u));
            ERational periods = form.Periods(point).Subtract(point.Z).Add(ERational.FromSingle((phase + bend) / float.Tau));
            return profile.Level(Split(periods).Local.ToSingle());
        }
    }
}
