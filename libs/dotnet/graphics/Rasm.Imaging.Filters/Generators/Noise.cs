using System.Globalization;
using System.Numerics;
using MathNet.Numerics.Distributions;
using PeterO.Numbers;
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

internal interface IOctaveSource<TSelf> where TSelf : IOctaveSource<TSelf> {
    public static abstract TSelf Advanced(TSelf at, float lacunarity);
    public static abstract float Sampled(NoiseDimensions lattice, TSelf at, uint field);
}

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class ReferenceFrame {
    public const float Height = 1080f;
    public const float GreaterSide = 1920f / Height;
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Seed : IPresented<Seed, int> {
    public static Seed MinValue { get; } = new(0);
    public static Seed MaxValue { get; } = new(int.MaxValue);
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Clock : IMinMaxValue<Clock> {
    public static Clock MinValue { get; } = new(0f);
    public static Clock MaxValue { get; } = new(1 << 14);

    public uint Term => BitConverter.SingleToUInt32Bits(_value);

    internal Clock Advanced(double seconds) => new((float)((_value + seconds) % MaxValue._value));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Hold : IPresented<Hold, float> {
    public static Hold MinValue { get; } = new(0.001f);
    public static Hold MaxValue { get; } = new(8f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second), Scale = TrackScale.Log };

    public uint Period(Clock clock) => (uint)Math.Floor((clock + (1d / TimeSpan.TicksPerSecond)) / _value * (1d + (1d / (1 << 22))));

    public float Held(Clock clock) => Period(clock) * _value;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Instant", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShutterTime : IPresented<ShutterTime, float> {
    public static ShutterTime MinValue => Instant;
    public static ShutterTime MaxValue { get; } = new(0.5f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Frequency : IPresented<Frequency, float> {
    public static Frequency MinValue { get; } = new(-60f);
    public static Frequency MaxValue { get; } = new(60f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(FrequencyUnit.Hertz), Origin = 0f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Drift : IPresented<Drift, float> {
    public static Drift MinValue { get; } = new(-10f);
    public static Drift MaxValue { get; } = new(10f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioChangeRateUnit.DecimalFractionPerSecond), Origin = 0f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FractalDetail : IMinMaxValue<FractalDetail> {
    public static FractalDetail MinValue { get; } = new(0f);
    public static FractalDetail MaxValue { get; } = new(15f);
    public static FractalDetail Standard { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FractalLacunarity : IMinMaxValue<FractalLacunarity> {
    public static FractalLacunarity MinValue { get; } = new(0f);
    public static FractalLacunarity MaxValue { get; } = new(MathF.Pow(NoiseFunctions.Ceiling / NoiseFunctions.Reach, 1f / FractalDetail.MaxValue));
    public static FractalLacunarity Standard { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FractalOffset : IPresented<FractalOffset, float> {
    public static FractalOffset MaxValue { get; } = new(MathF.Pow(NoiseFunctions.Ceiling, 1f / (FractalDetail.MaxValue + 1f)) - 1f - NoiseFunctions.Peak);
    public static FractalOffset MinValue { get; } = new(-MaxValue._value);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct NoiseGain : IPresented<NoiseGain, float> {
    public static NoiseGain MinValue { get; } = new(0f);
    public static NoiseGain MaxValue { get; } = new(1000f);
    public static NoiseGain Unit { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct NoiseDistortion : IPresented<NoiseDistortion, float> {
    public static NoiseDistortion MinValue { get; } = new(-1000f);
    public static NoiseDistortion MaxValue { get; } = new(1000f);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MinkowskiExponent : IMinMaxValue<MinkowskiExponent> {
    public static MinkowskiExponent MinValue { get; } = new(1f / 32f);
    public static MinkowskiExponent MaxValue { get; } = new(32f);
    public static MinkowskiExponent Standard { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct GaborFrequency : IMinMaxValue<GaborFrequency> {
    public static GaborFrequency MinValue { get; } = new(0.001f);
    public static GaborFrequency MaxValue { get; } = new(1000f);
    public static GaborFrequency Standard { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MagicDepth : IMinMaxValue<MagicDepth> {
    public static MagicDepth MinValue { get; } = new(0);
    public static MagicDepth MaxValue { get; } = new(10);
    public static MagicDepth Standard { get; } = new(2);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CellMetric : ICaptioned {
    public static readonly CellMetric Euclidean = new("euclidean", "Euclidean", static (delta, _) => delta.Length());
    public static readonly CellMetric Manhattan = new("manhattan", "Manhattan", static (delta, _) => Vector4.Sum(Vector4.Abs(delta)));
    public static readonly CellMetric Chebyshev = new("chebyshev", "Chebyshev", static (delta, _) => Vector4.Abs(delta) switch { var a => float.Max(float.Max(a.X, a.Y), float.Max(a.Z, a.W)) });
    public static readonly CellMetric Minkowski = new("minkowski", "Minkowski", static (delta, exponent) => MathF.Pow(Vector4.Sum(Vector4.Exp(exponent * Vector4.Log(Vector4.Abs(delta)))), 1f / exponent));

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial float Length(Vector4 delta, float exponent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveForm : ICaptioned {
    public static readonly WaveForm Bands = new("bands", "Bands", static point => point.X, static point => point.X);
    public static readonly WaveForm Rings = new("rings", "Rings", static point => point.AsVector2().Length(), static point => {
        ERational square = point.X.Multiply(point.X).Add(point.Y.Multiply(point.Y));
        EInteger whole = square.ToEInteger().Sqrt();
        EContext precision = EContext.Binary64.WithUnlimitedExponents();
        EFloat remainder = square.IsZero ? EFloat.Zero : square.Subtract(ERational.FromEInteger(whole.Multiply(whole))).ToEFloat(precision)
            .Divide(square.ToEFloat(precision).Sqrt(precision).Add(EFloat.FromEInteger(whole), precision), precision);
        return ERational.FromEInteger(whole).Add(ERational.FromEFloat(remainder));
    });

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial float Distance(Vector4 point);

    [UseDelegateFromConstructor]
    internal partial ERational Periods(LatticePoint point);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveProfile : ICaptioned {
    public static readonly WaveProfile Sine = new("sine", "Sine", static periods => 0.5f - (0.5f * float.CosPi(2f * periods)));
    public static readonly WaveProfile Saw = new("saw", "Saw", static periods => periods - MathF.Floor(periods));
    public static readonly WaveProfile Triangle = new("triangle", "Triangle", static periods => 2f * MathF.Abs(periods - MathF.Floor(periods + 0.5f)));
    public static readonly WaveProfile Square = new("square", "Square", static periods => MathF.Floor(2f * (periods - MathF.Floor(periods))));

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial float Level(float periods);
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseDimensions : ICaptioned {
    private static readonly GaborKernel Planar = new(Neighborhood.Of(2), static (angle, isotropy, draw) =>
        MathF.SinCos(angle + ((draw.X - 0.5f) * float.Pi * isotropy)) switch { var (sin, cos) => new Vector4(cos, sin, 0f, 0f) });
    private static readonly GaborKernel Spherical = new(Neighborhood.Of(3), static (angle, isotropy, draw) =>
        (MathF.SinCos((float.Pi / 2f) + (draw.X * float.Pi * isotropy)), MathF.SinCos(angle + (draw.Y * float.Pi * isotropy))) switch {
            var ((sinInclination, cosInclination), (sinAzimuth, cosAzimuth)) => new Vector4(sinInclination * cosAzimuth, sinInclination * sinAzimuth, cosInclination, 0f),
        });

    public static readonly NoiseDimensions One = new("1d", "1D", Neighborhood.Of(1), 0.25f, Planar, static (word, offset) =>
        (word >> 28) switch { var h => (1u + (h & 7u)) * ((h & 8u) is not 0u ? -offset.X : offset.X) });
    public static readonly NoiseDimensions Two = new("2d", "2D", Planar.Cells, 0.6616f, Planar, static (word, offset) =>
        (word >> 29) switch { var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 4u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -2f : 2f) * (h < 4u ? offset.Y : offset.X)) });
    public static readonly NoiseDimensions Three = new("3d", "3D", Spherical.Cells, 0.982f, Spherical, static (word, offset) =>
        (word >> 28) switch { var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 8u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -1f : 1f) * (h < 4u ? offset.Y : h is 12u or 14u ? offset.X : offset.Z)) });
    public static readonly NoiseDimensions Four = new("4d", "4D", Neighborhood.Of(4), 0.8344f, Spherical, static (word, offset) =>
        (word >> 27) switch {
            var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 24u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -1f : 1f) * (h < 16u ? offset.Y : offset.Z))
                + (((h & 4u) is not 0u ? -1f : 1f) * (h < 8u ? offset.Z : offset.W)),
        });

    public string Caption { get; }
    internal Neighborhood Cells { get; }
    internal float Scale { get; }
    internal float SlopePeak => Enumerable.Range(0, 32).Max(top => CornerSlope((uint)top << 27));
    internal GaborKernel Gabor { get; }

    [UseDelegateFromConstructor]
    internal partial float Slope(uint word, Vector4 offset);

    private float CornerSlope(uint word) => Cells.Corners.Max(corner => MathF.Abs(Slope(word, ((2f * corner) - Vector4.One) * Cells.Live)));
}

public sealed record Timing(Clock Clock, Magnitude Pace) : IStateRecord<Timing, TimingParameter> {
    public static Timing Default { get; } = new(Clock.MinValue, Magnitude.Unit);

    public Clock At(PassContext context) => context.Time.Match(Some: time => Clock.Advanced(Pace * time.TotalSeconds), None: Clock);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimingParameter : IStateParameter<Timing> {
    public static readonly TimingParameter Clock = new("clock", StateParameter.Bounded(Lens<Timing, Clock>.New(static timing => timing.Clock, static clock => timing => timing with { Clock = clock }), new Presentation<float> { Unit = Quantity.GetUnitInfo(DurationUnit.Second) }),
        new("Time", "Seconds of the moment a still draws, each recorded frame drawing at its own time"));
    public static readonly TimingParameter Pace = new("pace", StateParameter.Bounded(Lens<Timing, Magnitude>.New(static timing => timing.Pace, static pace => timing => timing with { Pace = pace }), new Presentation<float> { Soft = (0f, 4f) }),
        new("Pace", "Rate effective time advances with recorded frame time"));

    public StateParameter<Timing> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Octaves(FractalDetail Detail, AxisFraction Roughness, FractalLacunarity Lacunarity) : IStateRecord<Octaves, OctavesParameter> {
    public static Octaves Default { get; } = new(FractalDetail.Standard, AxisFraction.Half, FractalLacunarity.Standard);
    public static Octaves Plain { get; } = Default with { Detail = FractalDetail.MinValue };

    internal bool Degenerate => Lacunarity == FractalLacunarity.MinValue && Detail > FractalDetail.MinValue && Roughness > AxisFraction.MinValue;

    internal float Weight(int octave) => float.Min(1f, Detail - (octave - 1));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OctavesParameter : IStateParameter<Octaves> {
    public static readonly OctavesParameter Detail = new("detail", StateParameter.Bounded(Lens<Octaves, FractalDetail>.New(static octaves => octaves.Detail, static detail => octaves => octaves with { Detail = detail }), new Presentation<float>()),
        new("Detail", "Octaves added past the first"));
    public static readonly OctavesParameter Roughness = new("roughness", StateParameter.Bounded(Lens<Octaves, AxisFraction>.New(static octaves => octaves.Roughness, static roughness => octaves => octaves with { Roughness = roughness })),
        new("Roughness", "Strength of each octave against the one before"));
    public static readonly OctavesParameter Lacunarity = new("lacunarity", StateParameter.Bounded(Lens<Octaves, FractalLacunarity>.New(static octaves => octaves.Lacunarity, static lacunarity => octaves => octaves with { Lacunarity = lacunarity }), new Presentation<float>()),
        new("Lacunarity", "Frequency of each octave against the one before"));

    public StateParameter<Octaves> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Cellular(CellMetric Metric, MinkowskiExponent Exponent, AxisFraction Randomness) : IStateRecord<Cellular, CellularParameter> {
    public static Cellular Default { get; } = new(CellMetric.Euclidean, MinkowskiExponent.Standard, AxisFraction.MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CellularParameter : IStateParameter<Cellular> {
    public static readonly CellularParameter Metric = new("metric", new StateParameter<Cellular>.Choice<CellMetric>(Lens<Cellular, CellMetric>.New(static cells => cells.Metric, static metric => cells => cells with { Metric = metric })),
        new("Metric", "Distance the cells measure"));
    public static readonly CellularParameter Exponent = new("exponent", StateParameter.Bounded(Lens<Cellular, MinkowskiExponent>.New(static cells => cells.Exponent, static exponent => cells => cells with { Exponent = exponent }), new Presentation<float> { Scale = TrackScale.Log }),
        new("Exponent", "Exponent of the Minkowski distance"));
    public static readonly CellularParameter Randomness = new("randomness", StateParameter.Bounded(Lens<Cellular, AxisFraction>.New(static cells => cells.Randomness, static randomness => cells => cells with { Randomness = randomness })),
        new("Randomness", "Share of its cell each site wanders over"));

    public StateParameter<Cellular> Kind { get; }
    public ParameterText Text { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NoiseBasis {
    public static NoiseBasis Default { get; } = Fbm.Default;
    public static Fbm Plain { get; } = Fbm.Default with { Octaves = Octaves.Plain };

    internal abstract Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler { get; }

    internal abstract Fin<(float Lower, float Upper)> Range { get; }

    internal Fin<(Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sample, (float Lower, float Upper) Range)> BoundedSampler => Range.Map(range => (Sampler, range));

    internal Fin<Func<LatticePoint, uint, NoiseSample<LatticePoint>>> SiteSampler =>
        Switch(
            fbm: static _ => false, multifractal: static _ => false, heteroTerrain: static _ => false, hybridMultifractal: static _ => false, ridgedMultifractal: static _ => false,
            f1: static basis => basis.Octaves.Degenerate, f2: static basis => basis.Octaves.Degenerate, smoothF1: static basis => basis.Octaves.Degenerate,
            edgeDistance: static _ => false, gabor: static _ => false, magic: static _ => false, wave: static _ => false, white: static _ => false)
            ? Error.New(ErrorCodes.ValidationFailed, "cell site noise needs non-degenerate octaves") : Sampler;

    public sealed record Fbm(NoiseDimensions Dimensions, Octaves Octaves, NoiseDistortion Distortion) : NoiseBasis, IStateRecord<Fbm, FbmBasisParameter> {
        public static new Fbm Default { get; } = new(NoiseDimensions.Three, Octaves.Default, NoiseDistortion.Neutral);
        public static string Caption => "fBM";

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => Distorted(Dimensions, Distortion, (point, hash) => 0.5f + (0.5f * Dimensions.Fbm(point, Octaves, hash)));

        internal override Fin<(float Lower, float Upper)> Range => Centered(Dimensions.FbmBound(Octaves));
    }

    public sealed record Multifractal(NoiseDimensions Dimensions, Octaves Octaves, NoiseDistortion Distortion) : NoiseBasis, IStateRecord<Multifractal, MultifractalBasisParameter> {
        public static new Multifractal Default { get; } = new(NoiseDimensions.Three, Octaves.Default, NoiseDistortion.Neutral);

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => Distorted(Dimensions, Distortion, (point, hash) => Dimensions.Cascade(point, Octaves, 0f, 1f, hash));

        internal override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CascadeBound(Octaves, 0f, 1f));
    }

    public sealed record HeteroTerrain(NoiseDimensions Dimensions, Octaves Octaves, NoiseDistortion Distortion, FractalOffset Offset) : NoiseBasis, IStateRecord<HeteroTerrain, HeteroTerrainBasisParameter> {
        public static new HeteroTerrain Default { get; } = new(NoiseDimensions.Three, Octaves.Default, NoiseDistortion.Neutral, FractalOffset.Neutral);
        public static string Caption => "Hetero terrain";

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => Distorted(Dimensions, Distortion, (point, hash) => Dimensions.Cascade(point, Octaves, Offset, 0f, hash));

        internal override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CascadeBound(Octaves, Offset, 0f));
    }

    public sealed record HybridMultifractal(NoiseDimensions Dimensions, Octaves Octaves, NoiseDistortion Distortion, FractalOffset Offset, NoiseGain Gain)
        : NoiseBasis, IStateRecord<HybridMultifractal, HybridMultifractalBasisParameter> {
        public static new HybridMultifractal Default { get; } = new(NoiseDimensions.Three, Octaves.Default, NoiseDistortion.Neutral, FractalOffset.Neutral, NoiseGain.Unit);
        public static string Caption => "Hybrid multifractal";

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            Distorted(Dimensions, Distortion, (point, hash) => Dimensions.HybridMultifractal(point, Octaves, Offset, Gain, hash));

        internal override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.HybridBound(Octaves, Offset, Gain));
    }

    public sealed record RidgedMultifractal(NoiseDimensions Dimensions, Octaves Octaves, NoiseDistortion Distortion, FractalOffset Offset, NoiseGain Gain)
        : NoiseBasis, IStateRecord<RidgedMultifractal, RidgedMultifractalBasisParameter> {
        public static new RidgedMultifractal Default { get; } = new(NoiseDimensions.Three, Octaves.Default, NoiseDistortion.Neutral, FractalOffset.Neutral, NoiseGain.Unit);
        public static string Caption => "Ridged multifractal";

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            Distorted(Dimensions, Distortion, (point, hash) => Dimensions.RidgedMultifractal(point, Octaves, Offset, Gain, hash));

        internal override Fin<(float Lower, float Upper)> Range => Fin.Succ((0f, Dimensions.RidgedBound(Octaves, Offset, Gain)));
    }

    public sealed record F1(NoiseDimensions Dimensions, Octaves Octaves, Cellular Cellular) : NoiseBasis, IStateRecord<F1, F1BasisParameter> {
        public static new F1 Default { get; } = new(NoiseDimensions.Three, Octaves.Default, Cellular.Default);

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => Dimensions.F1(point, Octaves, Cellular, AxisFraction.MinValue, hash);

        internal override Fin<(float Lower, float Upper)> Range => Fin.Succ((0f, Dimensions.CellBound(Octaves, Cellular, 0f, 1f)));
    }

    public sealed record F2(NoiseDimensions Dimensions, Octaves Octaves, Cellular Cellular) : NoiseBasis, IStateRecord<F2, F2BasisParameter> {
        public static new F2 Default { get; } = new(NoiseDimensions.Three, Octaves.Default, Cellular.Default);

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => Dimensions.F2(point, Octaves, Cellular, hash);

        internal override Fin<(float Lower, float Upper)> Range => Fin.Succ((0f, Dimensions.CellBound(Octaves, Cellular, 0f, 2f)));
    }

    public sealed record SmoothF1(NoiseDimensions Dimensions, Octaves Octaves, Cellular Cellular, AxisFraction Smoothness) : NoiseBasis, IStateRecord<SmoothF1, SmoothF1BasisParameter> {
        public static new SmoothF1 Default { get; } = new(NoiseDimensions.Three, Octaves.Default, Cellular.Default, AxisFraction.MaxValue);
        public static string Caption => "Smooth F1";

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => Dimensions.F1(point, Octaves, Cellular, Smoothness, hash);

        internal override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CellBound(Octaves, Cellular, Smoothness / 2f, 1f));
    }

    public sealed record EdgeDistance(NoiseDimensions Dimensions, Octaves Octaves, AxisFraction Randomness) : NoiseBasis, IStateRecord<EdgeDistance, EdgeDistanceBasisParameter> {
        public static new EdgeDistance Default { get; } = new(NoiseDimensions.Three, Octaves.Default, Cellular.Default.Randomness);
        public static string Caption => "Distance to edge";

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => new(Dimensions.EdgeDistance(point, Octaves, Randomness, hash), point);

        internal override Fin<(float Lower, float Upper)> Range =>
            Octaves.Degenerate ? Error.New(ErrorCodes.ValidationFailed, "edge-distance noise needs non-degenerate octaves") : Fin.Succ(Dimensions.EdgeRange(Octaves, Randomness));
    }

    public sealed record Gabor(NoiseDimensions Dimensions, GaborFrequency Frequency, AxisFraction Anisotropy, SignedAngle Orientation) : NoiseBasis, IStateRecord<Gabor, GaborBasisParameter> {
        public static new Gabor Default { get; } = new(NoiseDimensions.Three, GaborFrequency.Standard, AxisFraction.MaxValue, SignedAngle.Diagonal);

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            (point, hash) => new(0.5f + (0.5f * Dimensions.Gabor(point, Frequency, Anisotropy, Orientation, hash).Y), point);

        internal override Fin<(float Lower, float Upper)> Range => Centered(Dimensions.GaborBound);
    }

    public sealed record Magic(MagicDepth Depth, NoiseDistortion Distortion) : NoiseBasis, IStateRecord<Magic, MagicBasisParameter> {
        public static new Magic Default { get; } = new(MagicDepth.Standard, NoiseDistortion.Neutral);

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, _) => new(Vector3.Sum(NoiseFunctions.Magic(point, Depth, Distortion)) / 3f, point);

        internal override Fin<(float Lower, float Upper)> Range => NoiseFunctions.MagicRange(Depth, Distortion);
    }

    public sealed record Wave(NoiseDimensions Dimensions, Octaves Octaves, NoiseDistortion Distortion, WaveForm Form, WaveProfile Profile, SignedAngle Phase, NoiseGain DetailScale)
        : NoiseBasis, IStateRecord<Wave, WaveBasisParameter> {
        public static new Wave Default { get; } = new(NoiseDimensions.Three, Octaves.Default, NoiseDistortion.Neutral, WaveForm.Bands, WaveProfile.Sine, SignedAngle.Neutral, NoiseGain.Unit);

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            (point, hash) => new(Dimensions.Wave(point, Form, Profile, Phase, Distortion, Octaves, DetailScale, hash), point);

        internal override Fin<(float Lower, float Upper)> Range => Fin.Succ((0f, 1f));
    }

    public sealed record White(NoiseDimensions Dimensions) : NoiseBasis, IStateRecord<White, WhiteBasisParameter> {
        public static new White Default { get; } = new(NoiseDimensions.Three);
        public static string Caption => "White noise";

        internal override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => new(Dimensions.White(point, hash).X, point);

        internal override Fin<(float Lower, float Upper)> Range => Fin.Succ((0f, 1f));
    }

    private static Func<LatticePoint, uint, NoiseSample<LatticePoint>> Distorted(NoiseDimensions dimensions, NoiseDistortion distortion, Func<LatticePoint, uint, float> sum) =>
        (point, field) => new(sum(dimensions.Distort(point, distortion, CoordinateHash.Branch(field, 0u)), CoordinateHash.Branch(field, 1u)), point);

    private static Fin<(float Lower, float Upper)> Symmetric(float bound) => (-bound, bound);

    private static Fin<(float Lower, float Upper)> Centered(float bound) => (0.5f - (0.5f * bound), 0.5f + (0.5f * bound));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FbmBasisParameter : IStateParameter<NoiseBasis.Fbm> {
    public static readonly FbmBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.Fbm>.Choice<NoiseDimensions>(Lens<NoiseBasis.Fbm, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly FbmBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.Fbm>.Record<Octaves>(Lens<NoiseBasis.Fbm, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly FbmBasisParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.Fbm, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warp of the noise by its own signal"));

    public StateParameter<NoiseBasis.Fbm> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MultifractalBasisParameter : IStateParameter<NoiseBasis.Multifractal> {
    public static readonly MultifractalBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.Multifractal>.Choice<NoiseDimensions>(Lens<NoiseBasis.Multifractal, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly MultifractalBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.Multifractal>.Record<Octaves>(Lens<NoiseBasis.Multifractal, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly MultifractalBasisParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.Multifractal, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warp of the noise by its own signal"));

    public StateParameter<NoiseBasis.Multifractal> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HeteroTerrainBasisParameter : IStateParameter<NoiseBasis.HeteroTerrain> {
    public static readonly HeteroTerrainBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.HeteroTerrain>.Choice<NoiseDimensions>(Lens<NoiseBasis.HeteroTerrain, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly HeteroTerrainBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.HeteroTerrain>.Record<Octaves>(Lens<NoiseBasis.HeteroTerrain, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly HeteroTerrainBasisParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.HeteroTerrain, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warp of the noise by its own signal"));
    public static readonly HeteroTerrainBasisParameter Offset = new("offset", StateParameter.Bounded(Lens<NoiseBasis.HeteroTerrain, FractalOffset>.New(static basis => basis.Offset, static offset => basis => basis with { Offset = offset })),
        new("Offset", "Value added to each multifractal octave"));

    public StateParameter<NoiseBasis.HeteroTerrain> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HybridMultifractalBasisParameter : IStateParameter<NoiseBasis.HybridMultifractal> {
    public static readonly HybridMultifractalBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.HybridMultifractal>.Choice<NoiseDimensions>(Lens<NoiseBasis.HybridMultifractal, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly HybridMultifractalBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.HybridMultifractal>.Record<Octaves>(Lens<NoiseBasis.HybridMultifractal, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly HybridMultifractalBasisParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.HybridMultifractal, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warp of the noise by its own signal"));
    public static readonly HybridMultifractalBasisParameter Offset = new("offset", StateParameter.Bounded(Lens<NoiseBasis.HybridMultifractal, FractalOffset>.New(static basis => basis.Offset, static offset => basis => basis with { Offset = offset })),
        new("Offset", "Value added to each multifractal octave"));
    public static readonly HybridMultifractalBasisParameter Gain = new("gain", StateParameter.Bounded(Lens<NoiseBasis.HybridMultifractal, NoiseGain>.New(static basis => basis.Gain, static gain => basis => basis with { Gain = gain })),
        new("Gain", "Weight each multifractal octave hands the next"));

    public StateParameter<NoiseBasis.HybridMultifractal> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RidgedMultifractalBasisParameter : IStateParameter<NoiseBasis.RidgedMultifractal> {
    public static readonly RidgedMultifractalBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.RidgedMultifractal>.Choice<NoiseDimensions>(Lens<NoiseBasis.RidgedMultifractal, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly RidgedMultifractalBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.RidgedMultifractal>.Record<Octaves>(Lens<NoiseBasis.RidgedMultifractal, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly RidgedMultifractalBasisParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.RidgedMultifractal, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warp of the noise by its own signal"));
    public static readonly RidgedMultifractalBasisParameter Offset = new("offset", StateParameter.Bounded(Lens<NoiseBasis.RidgedMultifractal, FractalOffset>.New(static basis => basis.Offset, static offset => basis => basis with { Offset = offset })),
        new("Offset", "Value added to each multifractal octave"));
    public static readonly RidgedMultifractalBasisParameter Gain = new("gain", StateParameter.Bounded(Lens<NoiseBasis.RidgedMultifractal, NoiseGain>.New(static basis => basis.Gain, static gain => basis => basis with { Gain = gain })),
        new("Gain", "Weight each multifractal octave hands the next"));

    public StateParameter<NoiseBasis.RidgedMultifractal> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class F1BasisParameter : IStateParameter<NoiseBasis.F1> {
    public static readonly F1BasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.F1>.Choice<NoiseDimensions>(Lens<NoiseBasis.F1, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly F1BasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.F1>.Record<Octaves>(Lens<NoiseBasis.F1, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly F1BasisParameter Cellular = new("cellular", new StateParameter<NoiseBasis.F1>.Record<Cellular>(Lens<NoiseBasis.F1, Cellular>.New(static basis => basis.Cellular, static cells => basis => basis with { Cellular = cells })),
        new("Cells", "Cell distance and site placement"));

    public StateParameter<NoiseBasis.F1> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class F2BasisParameter : IStateParameter<NoiseBasis.F2> {
    public static readonly F2BasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.F2>.Choice<NoiseDimensions>(Lens<NoiseBasis.F2, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly F2BasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.F2>.Record<Octaves>(Lens<NoiseBasis.F2, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly F2BasisParameter Cellular = new("cellular", new StateParameter<NoiseBasis.F2>.Record<Cellular>(Lens<NoiseBasis.F2, Cellular>.New(static basis => basis.Cellular, static cells => basis => basis with { Cellular = cells })),
        new("Cells", "Cell distance and site placement"));

    public StateParameter<NoiseBasis.F2> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SmoothF1BasisParameter : IStateParameter<NoiseBasis.SmoothF1> {
    public static readonly SmoothF1BasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.SmoothF1>.Choice<NoiseDimensions>(Lens<NoiseBasis.SmoothF1, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly SmoothF1BasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.SmoothF1>.Record<Octaves>(Lens<NoiseBasis.SmoothF1, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly SmoothF1BasisParameter Cellular = new("cellular", new StateParameter<NoiseBasis.SmoothF1>.Record<Cellular>(Lens<NoiseBasis.SmoothF1, Cellular>.New(static basis => basis.Cellular, static cells => basis => basis with { Cellular = cells })),
        new("Cells", "Cell distance and site placement"));
    public static readonly SmoothF1BasisParameter Smoothness = new("smoothness", StateParameter.Bounded(Lens<NoiseBasis.SmoothF1, AxisFraction>.New(static basis => basis.Smoothness, static smoothness => basis => basis with { Smoothness = smoothness })),
        new("Smoothness", "Blend between neighboring cells"));

    public StateParameter<NoiseBasis.SmoothF1> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeDistanceBasisParameter : IStateParameter<NoiseBasis.EdgeDistance> {
    public static readonly EdgeDistanceBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.EdgeDistance>.Choice<NoiseDimensions>(Lens<NoiseBasis.EdgeDistance, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly EdgeDistanceBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.EdgeDistance>.Record<Octaves>(Lens<NoiseBasis.EdgeDistance, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly EdgeDistanceBasisParameter Randomness = new("randomness", StateParameter.Bounded(Lens<NoiseBasis.EdgeDistance, AxisFraction>.New(static basis => basis.Randomness, static randomness => basis => basis with { Randomness = randomness })),
        new("Randomness", "Share of its cell each site wanders over"));

    public StateParameter<NoiseBasis.EdgeDistance> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GaborBasisParameter : IStateParameter<NoiseBasis.Gabor> {
    public static readonly GaborBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.Gabor>.Choice<NoiseDimensions>(Lens<NoiseBasis.Gabor, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly GaborBasisParameter Frequency = new("frequency", StateParameter.Bounded(Lens<NoiseBasis.Gabor, GaborFrequency>.New(static basis => basis.Frequency, static frequency => basis => basis with { Frequency = frequency }), new Presentation<float> { Scale = TrackScale.Log }),
        new("Frequency", "Gabor cycles per noise unit"));
    public static readonly GaborBasisParameter Anisotropy = new("anisotropy", StateParameter.Bounded(Lens<NoiseBasis.Gabor, AxisFraction>.New(static basis => basis.Anisotropy, static anisotropy => basis => basis with { Anisotropy = anisotropy })),
        new("Anisotropy", "Share of Gabor impulses held at the orientation"));
    public static readonly GaborBasisParameter Orientation = new("orientation", StateParameter.Bounded(Lens<NoiseBasis.Gabor, SignedAngle>.New(static basis => basis.Orientation, static orientation => basis => basis with { Orientation = orientation })),
        new("Orientation", "Base angle of the Gabor impulses"));

    public StateParameter<NoiseBasis.Gabor> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MagicBasisParameter : IStateParameter<NoiseBasis.Magic> {
    public static readonly MagicBasisParameter Depth = new("depth", StateParameter.Bounded(Lens<NoiseBasis.Magic, MagicDepth>.New(static basis => basis.Depth, static depth => basis => basis with { Depth = depth }), new Presentation<int> { Step = 1 }),
        new("Depth", "Turbulence steps of the magic noise"));
    public static readonly MagicBasisParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.Magic, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warp of the noise by its own signal"));

    public StateParameter<NoiseBasis.Magic> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveBasisParameter : IStateParameter<NoiseBasis.Wave> {
    public static readonly WaveBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.Wave>.Choice<NoiseDimensions>(Lens<NoiseBasis.Wave, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));
    public static readonly WaveBasisParameter Octaves = new("octaves", new StateParameter<NoiseBasis.Wave>.Record<Octaves>(Lens<NoiseBasis.Wave, Octaves>.New(static basis => basis.Octaves, static octaves => basis => basis with { Octaves = octaves })),
        new("Octaves", "Detail of the noise across scales"));
    public static readonly WaveBasisParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.Wave, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warp of the noise by its own signal"));
    public static readonly WaveBasisParameter Form = new("form", new StateParameter<NoiseBasis.Wave>.Choice<WaveForm>(Lens<NoiseBasis.Wave, WaveForm>.New(static basis => basis.Form, static form => basis => basis with { Form = form })),
        new("Type", "Bands or rings the wave noise draws"));
    public static readonly WaveBasisParameter Profile = new("profile", new StateParameter<NoiseBasis.Wave>.Choice<WaveProfile>(Lens<NoiseBasis.Wave, WaveProfile>.New(static basis => basis.Profile, static profile => basis => basis with { Profile = profile })),
        new("Profile", "Shape of each wave period"));
    public static readonly WaveBasisParameter Phase = new("phase", StateParameter.Bounded(Lens<NoiseBasis.Wave, SignedAngle>.New(static basis => basis.Phase, static phase => basis => basis with { Phase = phase })),
        new("Phase offset", "Turn added to the wave's argument"));
    public static readonly WaveBasisParameter DetailScale = new("detail-scale", StateParameter.Bounded(Lens<NoiseBasis.Wave, NoiseGain>.New(static basis => basis.DetailScale, static scale => basis => basis with { DetailScale = scale })),
        new("Detail scale", "Frequency of the wave's distortion noise against the wave's"));

    public StateParameter<NoiseBasis.Wave> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WhiteBasisParameter : IStateParameter<NoiseBasis.White> {
    public static readonly WhiteBasisParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.White>.Choice<NoiseDimensions>(Lens<NoiseBasis.White, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Number of coordinate axes the noise reads"));

    public StateParameter<NoiseBasis.White> Kind { get; }
    public ParameterText Text { get; }
}

internal readonly record struct NoiseSample<T>(float Value, T Site);

internal readonly record struct PeakBound(float Peak) : IOctaveSource<PeakBound> {
    public static PeakBound Advanced(PeakBound at, float lacunarity) => at;

    public static float Sampled(NoiseDimensions lattice, PeakBound at, uint field) => at.Peak;
}

internal readonly record struct LatticePoint(ERational X, ERational Y, ERational Z, ERational W) : IOctaveSource<LatticePoint> {
    public static LatticePoint Advanced(LatticePoint at, float lacunarity) => at.Scaled(ERational.FromSingle(lacunarity));

    public static float Sampled(NoiseDimensions lattice, LatticePoint at, uint field) => lattice.Gradient(at, field);

    public LatticePoint Scaled(ERational scale) => new(X.Multiply(scale), Y.Multiply(scale), Z.Multiply(scale), W.Multiply(scale));

    public LatticePoint Lerp(LatticePoint next, float weight) =>
        (ERational.FromSingle(1f - weight), ERational.FromSingle(weight)) switch {
            var (held, blend) => new(X.Multiply(held).Add(next.X.Multiply(blend)), Y.Multiply(held).Add(next.Y.Multiply(blend)), Z.Multiply(held).Add(next.Z.Multiply(blend)), W.Multiply(held).Add(next.W.Multiply(blend))),
        };

    public LatticePoint Masked(Vector4 axes) => new(X.Multiply((int)axes.X), Y.Multiply((int)axes.Y), Z.Multiply((int)axes.Z), W.Multiply((int)axes.W));

    public (LatticeCell Cell, Vector4 Local) Split() =>
        (NoiseFunctions.Split(X), NoiseFunctions.Split(Y), NoiseFunctions.Split(Z), NoiseFunctions.Split(W)) switch {
            var (x, y, z, w) => (new(x.Cell, y.Cell, z.Cell, w.Cell), new(x.Local.ToSingle(), y.Local.ToSingle(), z.Local.ToSingle(), w.Local.ToSingle())),
        };
}

internal readonly record struct LatticeCell(EInteger X, EInteger Y, EInteger Z, EInteger W) {
    public LatticeCell Offset(Vector4 offset) => new(X.Add((int)offset.X), Y.Add((int)offset.Y), Z.Add((int)offset.Z), W.Add((int)offset.W));

    public LatticePoint At(Vector4 local) =>
        new(ERational.FromEInteger(X).Add(ERational.FromSingle(local.X)), ERational.FromEInteger(Y).Add(ERational.FromSingle(local.Y)),
            ERational.FromEInteger(Z).Add(ERational.FromSingle(local.Z)), ERational.FromEInteger(W).Add(ERational.FromSingle(local.W)));
}

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

    public static (uint X, uint Y, uint Z) Hash(LatticeCell cell, uint field) {
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

    public static Vector4 White(LatticeCell cell, uint field) => Uniforms(Hash(cell, field));

    public static Vector4 White(LatticePoint point, uint field) =>
        White(new LatticeCell(Split(point.X).Cell, Split(point.Y).Cell, Split(point.Z).Cell, Split(point.W).Cell), field);

    public static Vector4 Uniforms((uint X, uint Y, uint Z) words) =>
        words switch {
            var (x, y, z) => (new Vector4(x >> 9, y >> 9, z >> 9, CoordinateHash.Pcg3d(x, y, z).X >> 9) + Vector4.Create(0.5f)) * (1f / (1 << 23)),
        };

    private static Vector4 GradientShift(uint field) => 100f * (Vector4.One + White(Vector4.Zero, field));

    // --- [FRACTALS]
    internal static readonly float Peak = NoiseDimensions.Items.Max(static lattice => lattice.Scale * lattice.SlopePeak);
    internal static readonly float Ceiling = float.ScaleB(1f, float.ILogB(float.MaxValue));
    internal static readonly float Reach = ((Frequency.MaxValue * (float)Clock.MaxValue) + (NoiseDistortion.MaxValue * Peak)) * NoiseGain.MaxValue;

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

    private static float Edge(ReadOnlySpan<Vector4> reach) {
        Vector4 closest = reach[0];
        foreach (Vector4 toward in reach)
            closest = toward.LengthSquared() < closest.LengthSquared() ? toward : closest;
        float edge = float.MaxValue;
        foreach (Vector4 toward in reach)
            edge = Vector4.DistanceSquared(toward, closest) > 0.0001f ? float.Min(edge, Vector4.Dot((closest + toward) / 2f, Vector4.Normalize(toward - closest))) : edge;
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

    public static Vector3 Magic(LatticePoint point, MagicDepth depth, NoiseDistortion distortion) {
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
        return float.IsFinite(first) && float.IsFinite(second) ? Fin.Succ((float.Min(first, second), float.Max(first, second)))
            : Error.New(string.Create(CultureInfo.InvariantCulture, $"magic noise at depth {depth} and distortion {distortion} has no finite range"));
    }

    extension(NoiseDimensions lattice) {
        // --- [DRAWS]
        public Vector4 White(LatticePoint point, uint field) => White(point.Masked(lattice.Cells.Live), field);

        public float Gradient(Vector4 point, uint field) {
            Vector4 wrapped = (point - (GradientRepeat * Vector4.Truncate(point / GradientRepeat))
                + Vector4.ConditionalSelect(Vector4.GreaterThanOrEqual(Vector4.Abs(point), Vector4.Create(10f * GradientRepeat)), Vector4.Create(0.5f), Vector4.Zero)
                + GradientShift(field)) * lattice.Cells.Live;
            Vector4 cell = Vector4.Round(wrapped, MidpointRounding.ToNegativeInfinity);
            return lattice.Gradient(cell, wrapped - cell, field);
        }

        public float Gradient(LatticePoint point, uint field) {
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

        public float CascadeBound(Octaves octaves, float offset, float lead) => lattice.Cascade(new PeakBound(lattice.GradientBound), octaves, float.Abs(offset), lead, 0u);

        public float HybridBound(Octaves octaves, float offset, float gain) => lattice.HybridMultifractal(new PeakBound(lattice.GradientBound), octaves, float.Abs(offset), gain, 0u);

        public float RidgedBound(Octaves octaves, float offset, float gain) => lattice.RidgedMultifractal(new PeakBound(float.Abs(offset) + lattice.GradientBound), octaves, 0f, gain, 0u);

        public float CellBound(Octaves octaves, Cellular cellular, float smoothness, float span) {
            float reach = (smoothness is 0f ? lattice.Cells.Near : lattice.Cells.Wide).Select(offset => cellular.Metric.Length(
                Vector4.Max(Vector4.Abs(offset - lattice.Cells.Live), Vector4.Abs(offset + (cellular.Randomness * lattice.Cells.Live))), cellular.Exponent))
                .Order().ElementAt(smoothness is 0f ? (int)span - 1 : lattice.Cells.Wide.Count - 1);
            float peak = smoothness is 0f ? reach : RoundBound(EFloat.FromSingle(reach).Add(EFloat.FromSingle(smoothness).Multiply(lattice.Cells.Wide.Count).Divide(4)), 8 * lattice.Cells.Wide.Count);
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

        public float Fbm(LatticePoint point, Octaves octaves, uint field) =>
            lattice.Fbm(point, ERational.One, octaves, field, static (scale, lacunarity) => scale.Multiply(ERational.FromSingle(lacunarity)),
                static (dimensions, at, scale, draw) => dimensions.Gradient(at.Scaled(scale), draw));

        private float Fbm<T, TScale>(T point, TScale scale, Octaves octaves, uint field, Func<TScale, float, TScale> advance, Func<NoiseDimensions, T, TScale, uint, float> sample) {
            (float sum, float total, float level, float amplitude) = (0f, 0f, 0f, 1f);
            for (int octave = 0; octaves.Weight(octave) > 0f; octave++) {
                (sum, total) = (sum + (sample(lattice, point, scale, field) * amplitude), total + amplitude);
                (level, amplitude, scale) = (float.Lerp(level, sum / total, octaves.Weight(octave)), amplitude * octaves.Roughness, advance(scale, octaves.Lacunarity));
            }
            return level;
        }

        public float Cascade<T>(T point, Octaves octaves, float offset, float lead, uint field) where T : IOctaveSource<T> {
            (float value, float amplitude) = (1f, 1f);
            for (int octave = 0; octaves.Weight(octave) > 0f; octave++)
                (value, amplitude, point) = (value * ((octave is 0 ? lead : 1f) + (octaves.Weight(octave) * amplitude * (T.Sampled(lattice, point, field) + offset))), amplitude * octaves.Roughness, T.Advanced(point, octaves.Lacunarity));
            return value;
        }

        public float HybridMultifractal<T>(T point, Octaves octaves, float offset, float gain, uint field) where T : IOctaveSource<T> {
            (float value, float weight, float amplitude) = (0f, 1f, 1f);
            for (int octave = 0; weight > 0.001f && octaves.Weight(octave) > 0f; octave++) {
                (float held, float signal) = (float.Min(weight, 1f), (T.Sampled(lattice, point, field) + offset) * amplitude);
                (value, weight, amplitude, point) = (value + (octaves.Weight(octave) * held * signal), held * gain * signal, amplitude * octaves.Roughness, T.Advanced(point, octaves.Lacunarity));
            }
            return value;
        }

        public float RidgedMultifractal<T>(T point, Octaves octaves, float offset, float gain, uint field) where T : IOctaveSource<T> {
            (float signal, float value, float amplitude) = (0f, 0f, 1f);
            for (int octave = 0; octaves.Weight(octave) >= 1f; octave++) {
                float ridge = offset - MathF.Abs(T.Sampled(lattice, point, field));
                signal = ridge * ridge * (octave is 0 ? 1f : Easing.Saturate(signal * gain));
                (value, amplitude, point) = (value + (signal * amplitude), amplitude * octaves.Roughness, T.Advanced(point, octaves.Lacunarity));
            }
            return value;
        }

        public LatticePoint Distort(LatticePoint point, NoiseDistortion distortion, uint field) {
            Span<ERational> moved = [point.X, point.Y, point.Z, point.W];
            for (int axis = 0; axis < (distortion == NoiseDistortion.Neutral ? 0 : lattice.Cells.Rank); axis++)
                moved[axis] = moved[axis].Add(ERational.FromSingle(distortion * lattice.Gradient(point, CoordinateHash.Branch(field, (uint)axis))));
            return new(moved[0], moved[1], moved[2], moved[3]);
        }

        // --- [CELLS]
        public (float Value, float EdgeDistance, LatticeCell Cell) Voronoi(
            LatticePoint point, Cellular cellular, uint field) {
            Span<Vector4> reach = stackalloc Vector4[lattice.Cells.Near.AsSpan().Length];
            (NoiseSample<LatticePoint> First, NoiseSample<LatticePoint> Second, LatticeCell Cell) = lattice.Ranked(point.Masked(lattice.Cells.Live), cellular, field, reach);
            return (First.Value, Edge(reach) / (0.5f + (0.5f * cellular.Randomness)), Cell);
        }

        public NoiseSample<LatticePoint> F1(LatticePoint point,
            Octaves octaves, Cellular cellular, AxisFraction smoothness, uint field) =>
            smoothness == AxisFraction.MinValue
                ? lattice.Fractal(point, octaves, cellular, 0f, 1f, field, static (dimensions, at, cells, _, draw) => dimensions.Ranked(at, cells, draw).First)
                : lattice.Fractal(point, octaves, cellular, smoothness / 2f, 1f, field, static (dimensions, at, cells, blend, draw) => dimensions.Smooth(at, cells, blend, draw));

        public NoiseSample<LatticePoint> F2(LatticePoint point,
            Octaves octaves, Cellular cellular, uint field) =>
            lattice.Fractal(point, octaves, cellular, 0f, 2f, field, static (dimensions, at, cells, _, draw) => dimensions.Ranked(at, cells, draw).Second);

        public float EdgeDistance(LatticePoint point, Octaves octaves, AxisFraction randomness, uint field) =>
            lattice.EdgeDistance(point.Masked(lattice.Cells.Live), octaves, randomness, field, static (at, scale) => at.Scaled(scale),
                static (dimensions, at, jitter, draw) => dimensions.Edge(at, jitter, draw));

        private float EdgeDistance<TPoint>(TPoint point, Octaves octaves, AxisFraction randomness, uint field,
            Func<TPoint, ERational, TPoint> at, Func<NoiseDimensions, TPoint, AxisFraction, uint, float> sample) {
            float limit = 0.5f + (0.5f * randomness);
            (ERational scale, ERational bound, ERational distance, float amplitude) = (ERational.One, ERational.FromSingle(limit), ERational.FromSingle(8f), 1f);
            for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
                float weight = amplitude * octaves.Weight(octave);
                (ERational held, ERational blend) = (ERational.FromSingle(1f - weight), ERational.FromSingle(weight));
                ERational next = ERational.FromSingle(sample(lattice, at(point, scale), randomness, field)).Divide(scale);
                ERational nearest = distance.IsNaN() || next.IsNaN() ? ERational.FromSingle(float.Min(distance.ToSingle(), next.ToSingle())) : ERational.Min(distance, next);
                (bound, distance) = (bound.Multiply(held).Add(ERational.FromSingle(limit).Divide(scale).Multiply(blend)), distance.Multiply(held).Add(nearest.Multiply(blend)));
                (amplitude, scale) = (amplitude * octaves.Roughness, scale.Multiply(ERational.FromSingle(octaves.Lacunarity)));
            }
            return distance.Divide(bound).ToSingle();
        }

        private NoiseSample<LatticePoint> Fractal(LatticePoint point,
            Octaves octaves, Cellular cellular, float smoothness, float span, uint field,
            Func<NoiseDimensions, LatticePoint, Cellular, float, uint, NoiseSample<LatticePoint>> scan) =>
            lattice.Fractal(point.Masked(lattice.Cells.Live), new LatticePoint(ERational.Zero, ERational.Zero, ERational.Zero, ERational.Zero), ERational.One, octaves, cellular, smoothness, span, field,
                static (at, scale) => at.Scaled(scale), static (scale, lacunarity) => scale.Multiply(ERational.FromSingle(lacunarity)), static (site, next, scale, weight) => site.Lerp(next.Scaled(ERational.One.Divide(scale)), weight), scan);

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

        private (NoiseSample<LatticePoint> First, NoiseSample<LatticePoint> Second,
            LatticeCell Cell) Ranked(LatticePoint point, Cellular cellular, uint field) {
            Span<Vector4> reach = stackalloc Vector4[lattice.Cells.Near.AsSpan().Length];
            return lattice.Ranked(point, cellular, field, reach);
        }

        private (NoiseSample<LatticePoint> First, NoiseSample<LatticePoint> Second,
            LatticeCell Cell) Ranked(LatticePoint point, Cellular cellular, uint field, Span<Vector4> reach) {
            (LatticeCell cell, Vector4 local) = point.Split();
            (NoiseSample<Vector4> first, NoiseSample<Vector4> second, Vector4 offset) = lattice.Ranked(local, cellular, (Cell: cell, Lattice: lattice, cellular.Randomness, Field: field),
                static (origin, offset) => origin.Lattice.Site(origin.Cell, offset, origin.Randomness, origin.Field), reach);
            return (new(first.Value, cell.At(first.Site)), new(second.Value, cell.At(second.Site)),
                cell.Offset(offset));
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

        private NoiseSample<LatticePoint> Smooth(LatticePoint point, Cellular cellular, float smoothness, uint field) {
            (LatticeCell cell, Vector4 local) = point.Split();
            ReadOnlySpan<Vector4> wide = lattice.Cells.Wide.AsSpan();
            (float distance, Vector4 site) = (0f, Vector4.Zero);
            for (int index = 0; index < wide.Length; index++) {
                Vector4 next = lattice.Site(cell, wide[index], cellular.Randomness, field);
                float reach = cellular.Metric.Length(next - local, cellular.Exponent);
                float blend = index is 0 ? 1f : (float)RampInterpolation.Ease.Weight(Easing.Saturate(0.5f + (0.5f * (distance - reach) / smoothness)));
                float correction = smoothness * blend * (1f - blend);
                (distance, site) = (float.Lerp(distance, reach, blend) - correction, Vector4.Lerp(site, next, blend) - (correction / (1f + (3f * smoothness)) * lattice.Cells.Live));
            }
            return new(distance, cell.At(site));
        }

        private float Edge(LatticePoint point, AxisFraction randomness, uint field) {
            (LatticeCell cell, Vector4 local) = point.Split();
            ReadOnlySpan<Vector4> near = lattice.Cells.Near.AsSpan();
            Span<Vector4> reach = stackalloc Vector4[near.Length];
            for (int index = 0; index < near.Length; index++)
                reach[index] = lattice.Site(cell, near[index], randomness, field) - local;
            return Edge(reach);
        }

        private Vector4 Site(LatticeCell cell, Vector4 offset, AxisFraction randomness, uint field) =>
            offset + (White(cell.Offset(offset), field) * lattice.Cells.Live * randomness);

        // --- [GABOR]
        public Vector2 Gabor(LatticePoint point, GaborFrequency frequency, AxisFraction anisotropy, SignedAngle orientation, uint field) {
            GaborKernel kernel = lattice.Gabor;
            (LatticeCell cell, Vector4 local) = point.Masked(kernel.Cells.Live).Split();
            Vector2 phasor = Vector2.Zero;
            for (uint impulse = 0u; impulse < GaborKernel.Impulses; impulse++) {
                (uint centers, uint turns) = (CoordinateHash.Branch(field, 2u * impulse), CoordinateHash.Branch(field, (2u * impulse) + 1u));
                foreach (Vector4 neighbor in kernel.Cells.Near.AsSpan()) {
                    Vector4 center = White(cell.Offset(neighbor), centers);
                    Vector4 reach = local - neighbor - (center * kernel.Cells.Live);
                    float spread = reach.LengthSquared();
                    (float sin, float cos) = spread < 1f
                        ? MathF.SinCos(float.Tau * frequency * Vector4.Dot(reach, kernel.Orient(orientation, 1f - anisotropy, White(cell.Offset(neighbor), turns))))
                        : (0f, 0f);
                    phasor += float.CopySign(MathF.Exp(-float.Pi * spread) * (0.5f + (0.5f * MathF.Cos(float.Pi * spread))), center.W - 0.5f) * new Vector2(cos, sin);
                }
            }
            return phasor / kernel.Normalization;
        }

        // --- [PATTERNS]
        public float Wave(LatticePoint point, WaveForm form, WaveProfile profile, float phase,
            NoiseDistortion distortion, Octaves octaves, NoiseGain detailScale, uint field) {
            ERational detail = ERational.FromSingle(float.Pi / 10f).Multiply(ERational.FromSingle(detailScale));
            float bend = distortion == NoiseDistortion.Neutral ? 0f : distortion * lattice.Fbm(
                new LatticePoint(point.X.Multiply(detail), point.Y.Multiply(detail), ERational.Zero, ERational.Zero), octaves, CoordinateHash.Branch(field, 0u));
            ERational periods = form.Periods(point).Subtract(point.Z).Add(ERational.FromSingle((phase + bend) / float.Tau));
            return profile.Level(Split(periods).Local.ToSingle());
        }
    }
}
