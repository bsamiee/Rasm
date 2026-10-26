using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Darkroom.Chain;
using Emgu.CV;
using MathNet.Numerics.Integration;
using PeterO.Numbers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using Rasm.UI;
using Rasm.UI.Styling;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using UnitsNet;
using UnitsNet.Units;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class ReferenceFrame {
    public const float Height = 1080f;
    public const float GreaterSide = 1920f / Height;
}

public static class Presentations {
    public static Func<AxisRange, Axis> Cubic { get; } = static range => new Axis.Scale(range,
        static (value, span) => ((value * value * value) - (span.From * span.From * span.From)) / ((span.To * span.To * span.To) - (span.From * span.From * span.From)),
        static (fraction, span) => MathF.Cbrt((span.From * span.From * span.From) + (fraction * ((span.To * span.To * span.To) - (span.From * span.From * span.From)))));

    public static Func<AxisRange, Axis> Decades { get; } = static range => new Axis.Log(range, 10f);

    public static Presentation<float> Length { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 1f) };
    public static Presentation<float> Extent { get; } = Length with { Soft = (0.001f, 8f), Track = Some(Decades) };
    public static Presentation<float> Offset { get; } = Length with { Soft = (-1f, 1f), Origin = 0f };
    public static Presentation<float> Frame { get; } = Length;
    public static Presentation<float> Angle { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = 0f };
    public static Presentation<float> Turn { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Revolution) };
    public static Presentation<float> Seconds { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) };
    public static Presentation<double> Kelvin { get; } = new() { Unit = Quantity.GetUnitInfo(TemperatureUnit.Kelvin), Soft = (800d, 12_000d) };
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Seed : IPresented<Seed, int> {
    public static Seed MinValue { get; } = new(0);
    public static Seed MaxValue { get; } = new(int.MaxValue);
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

    public uint Field(uint stream, uint term) => CoordinateHash.Field((uint)_value, term, stream);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Clock : IPresented<Clock, float> {
    public static Clock MinValue { get; } = new(0f);
    public static Clock MaxValue { get; } = new(1 << 14);
    public static Presentation<float> Presentation => Presentations.Seconds;

    public uint Term => BitConverter.SingleToUInt32Bits(_value);

    public Clock Advanced(double seconds) => new((float)((_value + seconds) % MaxValue._value));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Hold : IPresented<Hold, float> {
    public static Hold MinValue { get; } = new(0.001f);
    public static Hold MaxValue { get; } = new(8f);
    public static Presentation<float> Presentation { get; } = Presentations.Seconds with { Track = Some(Presentations.Decades) };

    public uint Period(Clock clock) => (uint)Math.Floor((clock + (1d / TimeSpan.TicksPerSecond)) / _value * (1d + (1d / (1 << 22))));

    public float Held(Clock clock) => Period(clock) * _value;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Instant", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShutterTime : IPresented<ShutterTime, float> {
    public static ShutterTime MinValue => Instant;
    public static ShutterTime MaxValue { get; } = new(0.5f);
    public static Presentation<float> Presentation => Presentations.Seconds;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Hertz : IPresented<Hertz, float> {
    public static Hertz MinValue { get; } = new(-60f);
    public static Hertz MaxValue { get; } = new(60f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(FrequencyUnit.Hertz), Origin = 0f };
    public static Presentation<float> Slow { get; } = Presentation with { Soft = (-4f, 4f) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Drift : IPresented<Drift, float> {
    public static Drift MinValue { get; } = new(-10f);
    public static Drift MaxValue { get; } = new(10f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioChangeRateUnit.DecimalFractionPerSecond), Origin = 0f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Stops : IPresented<Stops, float> {
    public static Stops MinValue { get; } = new(-32f);
    public static Stops MaxValue { get; } = new(32f);
    public static Presentation<float> Presentation { get; } = new() { Soft = (-10f, 10f), Origin = 0f, Step = 0.01f, Decimals = 3 };

    public float Scale => float.Exp2(_value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PrimaryScale : IMinMaxValue<PrimaryScale> {
    public static PrimaryScale MinValue { get; } = new(1f);
    public static PrimaryScale MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Steepness : IMinMaxValue<Steepness> {
    public static Steepness MinValue => Zero;
    public static Steepness MaxValue { get; } = new(10f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FractalDetail : IPresented<FractalDetail, float> {
    public static FractalDetail MinValue { get; } = new(0f);
    public static FractalDetail MaxValue { get; } = new(15f);
    public static FractalDetail Standard { get; } = new(2f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct NoiseDistortion : IPresented<NoiseDistortion, float> {
    public static NoiseDistortion MinValue { get; } = new(-1000f);
    public static NoiseDistortion MaxValue { get; } = new(1000f);
    public static Presentation<float> Presentation { get; } = new() { Origin = 0f };

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
public readonly partial struct FractalOffset : IPresented<FractalOffset, float> {
    public static float Ceiling { get; } = float.ScaleB(1f, float.ILogB(float.MaxValue));
    public static FractalOffset MaxValue { get; } = new(MathF.Pow(Ceiling, 1f / ((float)FractalDetail.MaxValue + 1f)) - 1f - NoiseDimensions.Peak);
    public static FractalOffset MinValue { get; } = new(-MaxValue._value);
    public static Presentation<float> Presentation { get; } = new() { Origin = 0f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FractalLacunarity : IPresented<FractalLacunarity, float> {
    public static FractalLacunarity MinValue { get; } = new(0f);
    public static FractalLacunarity MaxValue { get; } =
        new(MathF.Pow(FractalOffset.Ceiling / ((((float)Hertz.MaxValue * (float)Clock.MaxValue) + ((float)NoiseDistortion.MaxValue * NoiseDimensions.Peak)) * (float)NoiseGain.MaxValue), 1f / (float)FractalDetail.MaxValue));
    public static FractalLacunarity Standard { get; } = new(2f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MinkowskiExponent : IPresented<MinkowskiExponent, float> {
    public static MinkowskiExponent MinValue { get; } = new(1f / 32f);
    public static MinkowskiExponent MaxValue { get; } = new(32f);
    public static MinkowskiExponent Standard { get; } = new(0.5f);
    public static Presentation<float> Presentation { get; } = new() { Track = Some(Presentations.Decades) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct GaborFrequency : IPresented<GaborFrequency, float> {
    public static GaborFrequency MinValue { get; } = new(0.001f);
    public static GaborFrequency MaxValue { get; } = new(1000f);
    public static GaborFrequency Standard { get; } = new(2f);
    public static Presentation<float> Presentation { get; } = new() { Track = Some(Presentations.Decades) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MagicDepth : IPresented<MagicDepth, int> {
    public static MagicDepth MinValue { get; } = new(0);
    public static MagicDepth MaxValue { get; } = new(10);
    public static MagicDepth Standard { get; } = new(2);
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FogDistance : IMinMaxValue<FogDistance> {
    public static FogDistance MinValue => Neutral;
    public static FogDistance MaxValue { get; } = new(1e6f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FogAttenuation : IMinMaxValue<FogAttenuation> {
    public static FogAttenuation MinValue { get; } = new(0.001f);
    public static FogAttenuation MaxValue { get; } = new(1e6f);
    public static FogAttenuation Standard { get; } = new((float)Length.FromFeet(100).Meters);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct OpticalDepth : IPresented<OpticalDepth, float> {
    public static OpticalDepth MinValue { get; } = new(0f);
    public static OpticalDepth MaxValue { get; } = new(64f);
    public static Presentation<float> Presentation { get; } = new() { Soft = (0f, 8f) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Rings", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct VortexArms : IPresented<VortexArms, int> {
    public static VortexArms MinValue => Rings;
    public static VortexArms MaxValue { get; } = new(32);
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SuperellipseExponent : IPresented<SuperellipseExponent, float> {
    public static SuperellipseExponent MinValue { get; } = new(1f);
    public static SuperellipseExponent MaxValue { get; } = new(8f);
    public static SuperellipseExponent Ellipse { get; } = new(2f);
    public static Presentation<float> Presentation { get; } = new() { Track = Some(Presentations.Decades), Origin = 2f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SideCount : IPresented<SideCount, int> {
    public static SideCount MinValue { get; } = new(3);
    public static SideCount MaxValue { get; } = new(32);
    public static SideCount Five { get; } = new(5);
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SubdivisionDepth : IPresented<SubdivisionDepth, int> {
    public static SubdivisionDepth MinValue { get; } = new(1);
    public static SubdivisionDepth MaxValue { get; } = new(5);
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct BranchCount : IPresented<BranchCount, int> {
    public static BranchCount MinValue => Neutral;
    public static BranchCount MaxValue { get; } = new(7);
    public static BranchCount Standard { get; } = new(3);
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ForkSpread : IPresented<ForkSpread, float> {
    public static ForkSpread MinValue { get; } = new(0f);
    public static ForkSpread MaxValue { get; } = new(float.Pi / 3f);
    public static ForkSpread Standard { get; } = new(0.5f);
    public static Presentation<float> Presentation => Presentations.Angle;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ApertureBlades : IPresented<ApertureBlades, int> {
    public static ApertureBlades MinValue { get; } = new(3);
    public static ApertureBlades MaxValue { get; } = new(24);
    public static ApertureBlades Six { get; } = new(6);
    public static Presentation<int> Presentation { get; } = new() { Step = 1 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct AnamorphicSqueeze : IPresented<AnamorphicSqueeze, float> {
    public static AnamorphicSqueeze MinValue => Spherical;
    public static AnamorphicSqueeze MaxValue { get; } = new(2f);
    public static AnamorphicSqueeze Spherical { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct HighlightLevel : IPresented<HighlightLevel, float> {
    public static HighlightLevel MinValue { get; } = new(0f);
    public static HighlightLevel MaxValue { get; } = new(HistogramAxis.LowerBound(HistogramAxis.Bins - 1));
    public static HighlightLevel White { get; } = new(1f);
    public static HighlightLevel OneStopOver { get; } = new(2f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record Timing(Clock Clock, Magnitude Pace) : IStateRecord<Timing, TimingParameter> {
    public static Timing Default { get; } = new(Clock.MinValue, Magnitude.Unit);

    public Clock At(PassContext context) => context.Time.Match(Some: time => Clock.Advanced((float)Pace * time.TotalSeconds), None: Clock);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimingParameter : IStateParameter<Timing> {
    public static readonly TimingParameter Clock = new("clock", StateParameter.Bounded(Lens<Timing, Clock>.New(static timing => timing.Clock, static clock => timing => timing with { Clock = clock })),
        new("Time", "Seconds a still draws at, recorded frames advancing from it"));
    public static readonly TimingParameter Pace = new("pace", StateParameter.Bounded(Lens<Timing, Magnitude>.New(static timing => timing.Pace, static pace => timing => timing with { Pace = pace }), Magnitude.Presentation with { Soft = (0f, 4f) }),
        new("Pace", "Effective seconds per recorded second"));

    public StateParameter<Timing> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Position(FrameAxis X, FrameAxis Y) : IStateRecord<Position, PositionParameter> {
    public static Position Default { get; } = new(FrameAxis.Middle, FrameAxis.Middle);

    public (double X, double Y) Point(PixelExtent extent) => new FramePosition(X, Y).Point(extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PositionParameter : IStateParameter<Position> {
    public static readonly PositionParameter X = new("x", StateParameter.Bounded(Lens<Position, FrameAxis>.New(static position => position.X, static x => position => position with { X = x }), Presentations.Frame),
        new("X", "Position rightward from left frame edge"));
    public static readonly PositionParameter Y = new("y", StateParameter.Bounded(Lens<Position, FrameAxis>.New(static position => position.Y, static y => position => position with { Y = y }), Presentations.Frame),
        new("Y", "Position downward from top frame edge"));

    public StateParameter<Position> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Placement(Position Center, SignedAngle Rotation) : IStateRecord<Placement, PlacementParameter> {
    public static Placement Default { get; } = new(Position.Default, SignedAngle.Neutral);

    public FieldFrame Frame(PixelExtent extent) => new(extent, Center.Point(extent), double.SinCos(Rotation));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlacementParameter : IStateParameter<Placement> {
    public static readonly PlacementParameter Center = new("center", new StateParameter<Placement>.Record<Position>(Lens<Placement, Position>.New(static placement => placement.Center, static center => placement => placement with { Center = center })),
        new("Center", "Layer center and rotation pivot"));
    public static readonly PlacementParameter Rotation = new("rotation", StateParameter.Bounded(Lens<Placement, SignedAngle>.New(static placement => placement.Rotation, static rotation => placement => placement with { Rotation = rotation }), Presentations.Angle),
        new("Rotation", "Counterclockwise turn about center"));

    public StateParameter<Placement> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record GeneratedLayer(BlendingMode Mode, Stops Exposure) : IStateRecord<GeneratedLayer, GeneratedLayerParameter> {
    public static GeneratedLayer Default { get; } = new(BlendingMode.Mix, Stops.Zero);
    public static GeneratedLayer Added { get; } = Default with { Mode = BlendingMode.Add };

    public PixelPass Pass(Action<Span<Vector4>, long, long> fill) =>
        (Exposure.Scale, Mode) switch {
            var (gain, mode) => new PixelPass.Pointwise((row, column, line) => {
                using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
                using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                fill(layer.Span, column, line);
                for (int i = 0; i < row.Length; i++)
                    (weights.Span[i], layer.Span[i]) = (layer.Span[i].W, new Vector4(layer.Span[i].AsVector3() * gain, 1f));
                if (mode == BlendingMode.Mix)
                    mode.Composite(row, layer.Span, weights.Span, PixelAlphaRepresentation.Unassociated);
                else {
                    mode.Mixed(row, layer.Span, weights.Span);
                    layer.Span.CopyTo(row);
                }
            }),
        };

    public PixelPass Pass<TPrepared>(Fin<TPrepared> prepared, Func<TPrepared, Action<Span<Vector4>, long, long>> fill) =>
        prepared.Match(Succ: held => Pass(fill(held)), Fail: static error => new PixelPass.Frame((_, _) => error));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeneratedLayerParameter : IStateParameter<GeneratedLayer> {
    public static readonly GeneratedLayerParameter Mode = new("mode", new StateParameter<GeneratedLayer>.Choice<BlendingMode>(Lens<GeneratedLayer, BlendingMode>.New(static layer => layer.Mode, static mode => layer => layer with { Mode = mode })),
        new("Blend", "Mix covers frame, Add adds light"));
    public static readonly GeneratedLayerParameter Exposure = new("exposure", StateParameter.Bounded(Lens<GeneratedLayer, Stops>.New(static layer => layer.Exposure, static exposure => layer => layer with { Exposure = exposure })),
        new("Exposure", "Layer brightness in stops"));

    public StateParameter<GeneratedLayer> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Fractal(FractalDetail Detail, Fraction Roughness, FractalLacunarity Lacunarity) : IStateRecord<Fractal, FractalParameter> {
    public static Fractal Default { get; } = new(FractalDetail.Standard, Fraction.Create(0.5f), FractalLacunarity.Standard);
    public static Fractal Plain { get; } = Default with { Detail = FractalDetail.MinValue };

    public Octaves Octaves => new(Detail, Roughness, Lacunarity);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FractalParameter : IStateParameter<Fractal> {
    public static readonly FractalParameter Detail = new("detail", StateParameter.Bounded(Lens<Fractal, FractalDetail>.New(static fractal => fractal.Detail, static detail => fractal => fractal with { Detail = detail })),
        new("Detail", "Octaves added past the first"));
    public static readonly FractalParameter Roughness = new("roughness", StateParameter.Bounded(Lens<Fractal, Fraction>.New(static fractal => fractal.Roughness, static roughness => fractal => fractal with { Roughness = roughness })),
        new("Roughness", "Strength of each octave relative to previous"));
    public static readonly FractalParameter Lacunarity = new("lacunarity", StateParameter.Bounded(Lens<Fractal, FractalLacunarity>.New(static fractal => fractal.Lacunarity, static lacunarity => fractal => fractal with { Lacunarity = lacunarity })),
        new("Lacunarity", "Frequency of each octave relative to previous"));

    public StateParameter<Fractal> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Cellular(CellMetric Metric, MinkowskiExponent Exponent, Fraction Randomness) : IStateRecord<Cellular, CellularParameter> {
    public static Cellular Default { get; } = new(CellMetric.Euclidean, MinkowskiExponent.Standard, Fraction.MaxValue);

    public VoronoiCells Cells => new(Metric, Exponent, Randomness);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CellularParameter : IStateParameter<Cellular> {
    public static readonly CellularParameter Metric = new("metric", new StateParameter<Cellular>.Choice<CellMetric>(Lens<Cellular, CellMetric>.New(static cells => cells.Metric, static metric => cells => cells with { Metric = metric })),
        new("Metric", "Distance the cells measure"));
    public static readonly CellularParameter Exponent = new("exponent", StateParameter.Bounded(Lens<Cellular, MinkowskiExponent>.New(static cells => cells.Exponent, static exponent => cells => cells with { Exponent = exponent })),
        new("Exponent", "Minkowski distance exponent"));
    public static readonly CellularParameter Randomness = new("randomness", StateParameter.Bounded(Lens<Cellular, Fraction>.New(static cells => cells.Randomness, static randomness => cells => cells with { Randomness = randomness })),
        new("Randomness", "Share of its cell each site wanders over"));

    public StateParameter<Cellular> Kind { get; }
    public ParameterText Text { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NoiseBasis {
    public static NoiseBasis Default => Fbm.Default;
    public static Fbm Plain => Fbm.Default with { Fractal = Fractal.Plain };

    public abstract Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler { get; }

    public abstract Fin<(float Lower, float Upper)> Range { get; }

    private protected virtual bool Degenerate => false;

    public Fin<(Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sample, (float Lower, float Upper) Range)> BoundedSampler => Range.Map(range => (Sampler, range));

    public Fin<Func<LatticePoint, uint, NoiseSample<LatticePoint>>> SiteSampler =>
        Degenerate ? new Unrenderable(Captions.Of(GetType()), "cell sites need octaves that advance in scale") : Sampler;

    private static Func<LatticePoint, uint, NoiseSample<LatticePoint>> Distorted(NoiseDimensions dimensions, NoiseDistortion distortion, Func<LatticePoint, uint, float> sum) =>
        (point, field) => new(sum(dimensions.Distort(point, distortion, CoordinateHash.Branch(field, 0u)), CoordinateHash.Branch(field, 1u)), point);

    private static Fin<(float Lower, float Upper)> Symmetric(float bound) => (-bound, bound);

    private static Fin<(float Lower, float Upper)> Centered(float bound) => (0.5f - (0.5f * bound), 0.5f + (0.5f * bound));

    // --- [CASES]
    public sealed record Fbm(NoiseDimensions Dimensions, Fractal Fractal, NoiseDistortion Distortion) : NoiseBasis, IStateRecord<Fbm, FbmParameter> {
        public static new Fbm Default { get; } = new(NoiseDimensions.Three, Fractal.Default, NoiseDistortion.Neutral);
        public static string Caption => "fBM";

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => Distorted(Dimensions, Distortion, (point, hash) => 0.5f + (0.5f * Dimensions.Fbm(point, Fractal.Octaves, hash)));

        public override Fin<(float Lower, float Upper)> Range => Centered(Dimensions.FbmBound(Fractal.Octaves));
    }

    public sealed record Multifractal(NoiseDimensions Dimensions, Fractal Fractal, NoiseDistortion Distortion) : NoiseBasis, IStateRecord<Multifractal, MultifractalParameter> {
        public static new Multifractal Default { get; } = new(NoiseDimensions.Three, Fractal.Default, NoiseDistortion.Neutral);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => Distorted(Dimensions, Distortion, (point, hash) => Dimensions.Cascade(point, Fractal.Octaves, 0f, 1f, hash));

        public override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CascadeBound(Fractal.Octaves, 0f, 1f));
    }

    public sealed record HeteroTerrain(NoiseDimensions Dimensions, Fractal Fractal, NoiseDistortion Distortion, FractalOffset Offset)
        : NoiseBasis, IStateRecord<HeteroTerrain, HeteroTerrainParameter> {
        public static new HeteroTerrain Default { get; } = new(NoiseDimensions.Three, Fractal.Default, NoiseDistortion.Neutral, FractalOffset.Neutral);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => Distorted(Dimensions, Distortion, (point, hash) => Dimensions.Cascade(point, Fractal.Octaves, Offset, 0f, hash));

        public override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CascadeBound(Fractal.Octaves, Offset, 0f));
    }

    public sealed record HybridMultifractal(NoiseDimensions Dimensions, Fractal Fractal, NoiseDistortion Distortion, FractalOffset Offset, NoiseGain Gain)
        : NoiseBasis, IStateRecord<HybridMultifractal, HybridMultifractalParameter> {
        public static new HybridMultifractal Default { get; } = new(NoiseDimensions.Three, Fractal.Default, NoiseDistortion.Neutral, FractalOffset.Neutral, NoiseGain.Unit);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            Distorted(Dimensions, Distortion, (point, hash) => Dimensions.HybridMultifractal(point, Fractal.Octaves, Offset, Gain, hash));

        public override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.HybridBound(Fractal.Octaves, Offset, Gain));
    }

    public sealed record RidgedMultifractal(NoiseDimensions Dimensions, Fractal Fractal, NoiseDistortion Distortion, FractalOffset Offset, NoiseGain Gain)
        : NoiseBasis, IStateRecord<RidgedMultifractal, RidgedMultifractalParameter> {
        public static new RidgedMultifractal Default { get; } = new(NoiseDimensions.Three, Fractal.Default, NoiseDistortion.Neutral, FractalOffset.Neutral, NoiseGain.Unit);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            Distorted(Dimensions, Distortion, (point, hash) => Dimensions.RidgedMultifractal(point, Fractal.Octaves, Offset, Gain, hash));

        public override Fin<(float Lower, float Upper)> Range => (0f, Dimensions.RidgedBound(Fractal.Octaves, Offset, Gain));
    }

    public sealed record F1(NoiseDimensions Dimensions, Fractal Fractal, Cellular Cellular) : NoiseBasis, IStateRecord<F1, F1Parameter> {
        public static new F1 Default { get; } = new(NoiseDimensions.Three, Fractal.Default, Cellular.Default);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => Dimensions.F1(point, Fractal.Octaves, Cellular.Cells, 0f, hash);

        public override Fin<(float Lower, float Upper)> Range => (0f, Dimensions.CellBound(Fractal.Octaves, Cellular.Cells, 0f, 1f));

        private protected override bool Degenerate => Fractal.Octaves.Degenerate;
    }

    public sealed record F2(NoiseDimensions Dimensions, Fractal Fractal, Cellular Cellular) : NoiseBasis, IStateRecord<F2, F2Parameter> {
        public static new F2 Default { get; } = new(NoiseDimensions.Three, Fractal.Default, Cellular.Default);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => Dimensions.F2(point, Fractal.Octaves, Cellular.Cells, hash);

        public override Fin<(float Lower, float Upper)> Range => (0f, Dimensions.CellBound(Fractal.Octaves, Cellular.Cells, 0f, 2f));

        private protected override bool Degenerate => Fractal.Octaves.Degenerate;
    }

    public sealed record SmoothF1(NoiseDimensions Dimensions, Fractal Fractal, Cellular Cellular, Fraction Smoothness) : NoiseBasis, IStateRecord<SmoothF1, SmoothF1Parameter> {
        public static new SmoothF1 Default { get; } = new(NoiseDimensions.Three, Fractal.Default, Cellular.Default, Fraction.MaxValue);
        public static string Caption => "Smooth F1";

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => Dimensions.F1(point, Fractal.Octaves, Cellular.Cells, Smoothness, hash);

        public override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CellBound(Fractal.Octaves, Cellular.Cells, Smoothness, 1f));

        private protected override bool Degenerate => Fractal.Octaves.Degenerate;
    }

    public sealed record EdgeDistance(NoiseDimensions Dimensions, Fractal Fractal, Fraction Randomness) : NoiseBasis, IStateRecord<EdgeDistance, EdgeDistanceParameter> {
        public static new EdgeDistance Default { get; } = new(NoiseDimensions.Three, Fractal.Default, Cellular.Default.Randomness);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => new(Dimensions.EdgeDistance(point, Fractal.Octaves, Randomness, hash), point);

        public override Fin<(float Lower, float Upper)> Range =>
            Fractal.Octaves.Degenerate ? new Unrenderable(Captions.Of(typeof(EdgeDistance)), "edge distance needs octaves that advance in scale") : Dimensions.EdgeRange(Fractal.Octaves, Randomness);
    }

    public sealed record Gabor(NoiseDimensions Dimensions, GaborFrequency Frequency, Fraction Anisotropy, SignedAngle Orientation) : NoiseBasis, IStateRecord<Gabor, GaborParameter> {
        public static new Gabor Default { get; } = new(NoiseDimensions.Three, GaborFrequency.Standard, Fraction.MaxValue, SignedAngle.Diagonal);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            (point, hash) => new(0.5f + (0.5f * Dimensions.Gabor(point, Frequency, Anisotropy, Orientation, hash).Y), point);

        public override Fin<(float Lower, float Upper)> Range => Centered(Dimensions.GaborBound);
    }

    public sealed record Magic(MagicDepth Depth, NoiseDistortion Distortion) : NoiseBasis, IStateRecord<Magic, MagicParameter> {
        public static new Magic Default { get; } = new(MagicDepth.Standard, NoiseDistortion.Neutral);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            new MagicNoise(Depth, Distortion) switch { var magic => (point, _) => new(Vector3.Sum(magic.Sample(point)) / 3f, point) };

        public override Fin<(float Lower, float Upper)> Range => new MagicNoise(Depth, Distortion).Range;
    }

    public sealed record Wave(NoiseDimensions Dimensions, Fractal Fractal, NoiseDistortion Distortion, WaveForm Form, WaveProfile Profile, SignedAngle Phase, NoiseGain DetailScale)
        : NoiseBasis, IStateRecord<Wave, WaveParameter> {
        public static new Wave Default { get; } = new(NoiseDimensions.Three, Fractal.Default, NoiseDistortion.Neutral, WaveForm.Bands, WaveProfile.Sine, SignedAngle.Neutral, NoiseGain.Unit);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler =>
            (point, hash) => new(Dimensions.Wave(point, Form, Profile, Phase, Distortion, Fractal.Octaves, DetailScale, hash), point);

        public override Fin<(float Lower, float Upper)> Range => (0f, 1f);
    }

    public sealed record White(NoiseDimensions Dimensions) : NoiseBasis, IStateRecord<White, WhiteParameter> {
        public static new White Default { get; } = new(NoiseDimensions.Three);

        public override Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sampler => (point, hash) => new(Dimensions.White(point, hash).X, point);

        public override Fin<(float Lower, float Upper)> Range => (0f, 1f);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FbmParameter : IStateParameter<NoiseBasis.Fbm> {
    public static readonly FbmParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.Fbm>.Choice<NoiseDimensions>(Lens<NoiseBasis.Fbm, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly FbmParameter Fractal = new("octaves", new StateParameter<NoiseBasis.Fbm>.Record<Fractal>(Lens<NoiseBasis.Fbm, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly FbmParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.Fbm, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warps noise by its own signal"));

    public StateParameter<NoiseBasis.Fbm> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MultifractalParameter : IStateParameter<NoiseBasis.Multifractal> {
    public static readonly MultifractalParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.Multifractal>.Choice<NoiseDimensions>(Lens<NoiseBasis.Multifractal, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly MultifractalParameter Fractal = new("octaves", new StateParameter<NoiseBasis.Multifractal>.Record<Fractal>(Lens<NoiseBasis.Multifractal, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly MultifractalParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.Multifractal, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warps noise by its own signal"));

    public StateParameter<NoiseBasis.Multifractal> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HeteroTerrainParameter : IStateParameter<NoiseBasis.HeteroTerrain> {
    public static readonly HeteroTerrainParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.HeteroTerrain>.Choice<NoiseDimensions>(Lens<NoiseBasis.HeteroTerrain, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly HeteroTerrainParameter Fractal = new("octaves", new StateParameter<NoiseBasis.HeteroTerrain>.Record<Fractal>(Lens<NoiseBasis.HeteroTerrain, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly HeteroTerrainParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.HeteroTerrain, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warps noise by its own signal"));
    public static readonly HeteroTerrainParameter Offset = new("offset", StateParameter.Bounded(Lens<NoiseBasis.HeteroTerrain, FractalOffset>.New(static basis => basis.Offset, static offset => basis => basis with { Offset = offset })),
        new("Offset", "Value added to each multifractal octave"));

    public StateParameter<NoiseBasis.HeteroTerrain> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HybridMultifractalParameter : IStateParameter<NoiseBasis.HybridMultifractal> {
    public static readonly HybridMultifractalParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.HybridMultifractal>.Choice<NoiseDimensions>(Lens<NoiseBasis.HybridMultifractal, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly HybridMultifractalParameter Fractal = new("octaves", new StateParameter<NoiseBasis.HybridMultifractal>.Record<Fractal>(Lens<NoiseBasis.HybridMultifractal, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly HybridMultifractalParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.HybridMultifractal, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warps noise by its own signal"));
    public static readonly HybridMultifractalParameter Offset = new("offset", StateParameter.Bounded(Lens<NoiseBasis.HybridMultifractal, FractalOffset>.New(static basis => basis.Offset, static offset => basis => basis with { Offset = offset })),
        new("Offset", "Value added to each multifractal octave"));
    public static readonly HybridMultifractalParameter Gain = new("gain", StateParameter.Bounded(Lens<NoiseBasis.HybridMultifractal, NoiseGain>.New(static basis => basis.Gain, static gain => basis => basis with { Gain = gain })),
        new("Gain", "Weight each multifractal octave hands the next"));

    public StateParameter<NoiseBasis.HybridMultifractal> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RidgedMultifractalParameter : IStateParameter<NoiseBasis.RidgedMultifractal> {
    public static readonly RidgedMultifractalParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.RidgedMultifractal>.Choice<NoiseDimensions>(Lens<NoiseBasis.RidgedMultifractal, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly RidgedMultifractalParameter Fractal = new("octaves", new StateParameter<NoiseBasis.RidgedMultifractal>.Record<Fractal>(Lens<NoiseBasis.RidgedMultifractal, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly RidgedMultifractalParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.RidgedMultifractal, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warps noise by its own signal"));
    public static readonly RidgedMultifractalParameter Offset = new("offset", StateParameter.Bounded(Lens<NoiseBasis.RidgedMultifractal, FractalOffset>.New(static basis => basis.Offset, static offset => basis => basis with { Offset = offset })),
        new("Offset", "Value added to each multifractal octave"));
    public static readonly RidgedMultifractalParameter Gain = new("gain", StateParameter.Bounded(Lens<NoiseBasis.RidgedMultifractal, NoiseGain>.New(static basis => basis.Gain, static gain => basis => basis with { Gain = gain })),
        new("Gain", "Weight each multifractal octave hands the next"));

    public StateParameter<NoiseBasis.RidgedMultifractal> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class F1Parameter : IStateParameter<NoiseBasis.F1> {
    public static readonly F1Parameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.F1>.Choice<NoiseDimensions>(Lens<NoiseBasis.F1, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly F1Parameter Fractal = new("octaves", new StateParameter<NoiseBasis.F1>.Record<Fractal>(Lens<NoiseBasis.F1, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly F1Parameter Cellular = new("cellular", new StateParameter<NoiseBasis.F1>.Record<Cellular>(Lens<NoiseBasis.F1, Cellular>.New(static basis => basis.Cellular, static cells => basis => basis with { Cellular = cells })),
        new("Cells", "Cell distance and site placement"));

    public StateParameter<NoiseBasis.F1> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class F2Parameter : IStateParameter<NoiseBasis.F2> {
    public static readonly F2Parameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.F2>.Choice<NoiseDimensions>(Lens<NoiseBasis.F2, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly F2Parameter Fractal = new("octaves", new StateParameter<NoiseBasis.F2>.Record<Fractal>(Lens<NoiseBasis.F2, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly F2Parameter Cellular = new("cellular", new StateParameter<NoiseBasis.F2>.Record<Cellular>(Lens<NoiseBasis.F2, Cellular>.New(static basis => basis.Cellular, static cells => basis => basis with { Cellular = cells })),
        new("Cells", "Cell distance and site placement"));

    public StateParameter<NoiseBasis.F2> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SmoothF1Parameter : IStateParameter<NoiseBasis.SmoothF1> {
    public static readonly SmoothF1Parameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.SmoothF1>.Choice<NoiseDimensions>(Lens<NoiseBasis.SmoothF1, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly SmoothF1Parameter Fractal = new("octaves", new StateParameter<NoiseBasis.SmoothF1>.Record<Fractal>(Lens<NoiseBasis.SmoothF1, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly SmoothF1Parameter Cellular = new("cellular", new StateParameter<NoiseBasis.SmoothF1>.Record<Cellular>(Lens<NoiseBasis.SmoothF1, Cellular>.New(static basis => basis.Cellular, static cells => basis => basis with { Cellular = cells })),
        new("Cells", "Cell distance and site placement"));
    public static readonly SmoothF1Parameter Smoothness = new("smoothness", StateParameter.Bounded(Lens<NoiseBasis.SmoothF1, Fraction>.New(static basis => basis.Smoothness, static smoothness => basis => basis with { Smoothness = smoothness })),
        new("Smoothness", "Blend between neighboring cells"));

    public StateParameter<NoiseBasis.SmoothF1> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeDistanceParameter : IStateParameter<NoiseBasis.EdgeDistance> {
    public static readonly EdgeDistanceParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.EdgeDistance>.Choice<NoiseDimensions>(Lens<NoiseBasis.EdgeDistance, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly EdgeDistanceParameter Fractal = new("octaves", new StateParameter<NoiseBasis.EdgeDistance>.Record<Fractal>(Lens<NoiseBasis.EdgeDistance, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly EdgeDistanceParameter Randomness = new("randomness", StateParameter.Bounded(Lens<NoiseBasis.EdgeDistance, Fraction>.New(static basis => basis.Randomness, static randomness => basis => basis with { Randomness = randomness })),
        new("Randomness", "Share of its cell each site wanders over"));

    public StateParameter<NoiseBasis.EdgeDistance> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GaborParameter : IStateParameter<NoiseBasis.Gabor> {
    public static readonly GaborParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.Gabor>.Choice<NoiseDimensions>(Lens<NoiseBasis.Gabor, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly GaborParameter Frequency = new("frequency", StateParameter.Bounded(Lens<NoiseBasis.Gabor, GaborFrequency>.New(static basis => basis.Frequency, static frequency => basis => basis with { Frequency = frequency })),
        new("Frequency", "Gabor cycles per noise unit"));
    public static readonly GaborParameter Anisotropy = new("anisotropy", StateParameter.Bounded(Lens<NoiseBasis.Gabor, Fraction>.New(static basis => basis.Anisotropy, static anisotropy => basis => basis with { Anisotropy = anisotropy })),
        new("Anisotropy", "Share of Gabor impulses held at the orientation"));
    public static readonly GaborParameter Orientation = new("orientation", StateParameter.Bounded(Lens<NoiseBasis.Gabor, SignedAngle>.New(static basis => basis.Orientation, static orientation => basis => basis with { Orientation = orientation }), Presentations.Angle),
        new("Orientation", "Base angle of Gabor impulses"));

    public StateParameter<NoiseBasis.Gabor> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MagicParameter : IStateParameter<NoiseBasis.Magic> {
    public static readonly MagicParameter Depth = new("depth", StateParameter.Bounded(Lens<NoiseBasis.Magic, MagicDepth>.New(static basis => basis.Depth, static depth => basis => basis with { Depth = depth })),
        new("Depth", "Turbulence steps of magic noise"));
    public static readonly MagicParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.Magic, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warps noise by its own signal"));

    public StateParameter<NoiseBasis.Magic> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveParameter : IStateParameter<NoiseBasis.Wave> {
    public static readonly WaveParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.Wave>.Choice<NoiseDimensions>(Lens<NoiseBasis.Wave, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));
    public static readonly WaveParameter Fractal = new("octaves", new StateParameter<NoiseBasis.Wave>.Record<Fractal>(Lens<NoiseBasis.Wave, Fractal>.New(static basis => basis.Fractal, static fractal => basis => basis with { Fractal = fractal })),
        new("Octaves", "Noise detail across scales"));
    public static readonly WaveParameter Distortion = new("distortion", StateParameter.Bounded(Lens<NoiseBasis.Wave, NoiseDistortion>.New(static basis => basis.Distortion, static distortion => basis => basis with { Distortion = distortion })),
        new("Distortion", "Warps noise by its own signal"));
    public static readonly WaveParameter Form = new("form", new StateParameter<NoiseBasis.Wave>.Choice<WaveForm>(Lens<NoiseBasis.Wave, WaveForm>.New(static basis => basis.Form, static form => basis => basis with { Form = form })),
        new("Type", "Bands or rings the wave draws"));
    public static readonly WaveParameter Profile = new("profile", new StateParameter<NoiseBasis.Wave>.Choice<WaveProfile>(Lens<NoiseBasis.Wave, WaveProfile>.New(static basis => basis.Profile, static profile => basis => basis with { Profile = profile })),
        new("Profile", "Shape of each wave period"));
    public static readonly WaveParameter Phase = new("phase", StateParameter.Bounded(Lens<NoiseBasis.Wave, SignedAngle>.New(static basis => basis.Phase, static phase => basis => basis with { Phase = phase }), Presentations.Angle),
        new("Phase offset", "Turn added to wave argument"));
    public static readonly WaveParameter DetailScale = new("detail-scale", StateParameter.Bounded(Lens<NoiseBasis.Wave, NoiseGain>.New(static basis => basis.DetailScale, static scale => basis => basis with { DetailScale = scale })),
        new("Detail scale", "Distortion noise frequency relative to wave"));

    public StateParameter<NoiseBasis.Wave> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WhiteParameter : IStateParameter<NoiseBasis.White> {
    public static readonly WhiteParameter Dimensions = new("dimensions", new StateParameter<NoiseBasis.White>.Choice<NoiseDimensions>(Lens<NoiseBasis.White, NoiseDimensions>.New(static basis => basis.Dimensions, static dimensions => basis => basis with { Dimensions = dimensions })),
        new("Dimensions", "Coordinate axes the noise reads"));

    public StateParameter<NoiseBasis.White> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record NoiseSource(NoiseBasis Basis, ShortSideExtent ScaleX, ShortSideExtent ScaleY, Drift DriftX, Drift DriftY, Hertz Evolution)
    : IStateRecord<NoiseSource, NoiseSourceParameter> {
    public static NoiseSource Default { get; } = Presets.Source(NoiseBasis.Default, ShortSideExtent.Create(1f), ShortSideExtent.Create(1f));

    public Func<LatticePoint, float> Sampler(float time, uint field) =>
        (Basis.Sampler, ERational.FromSingle(ScaleX), ERational.FromSingle(ScaleY), ERational.FromSingle(time)) switch {
            var (basis, scaleX, scaleY, clock) => (ERational.FromSingle(DriftX).Multiply(clock), ERational.FromSingle(DriftY).Multiply(clock), ERational.FromSingle(Evolution).Multiply(clock)) switch {
                var (driftX, driftY, lift) => point => basis(new(point.X.Subtract(driftX).Divide(scaleX), point.Y.Subtract(driftY).Divide(scaleY), point.Z.Divide(scaleX).Add(lift), point.W), field).Value,
            },
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseSourceParameter : IStateParameter<NoiseSource> {
    private static readonly Presentation<float> Feature = Presentations.Extent with { Soft = (0.01f, 2f) };
    private static readonly Presentation<float> Travel = Drift.Presentation with { Soft = (-1f, 1f) };

    public static readonly NoiseSourceParameter Basis = new("basis", new StateParameter<NoiseSource>.Variant<NoiseBasis>(Lens<NoiseSource, NoiseBasis>.New(static noise => noise.Basis, static basis => noise => noise with { Basis = basis })),
        new("Basis", "Noise the field draws"));
    public static readonly NoiseSourceParameter ScaleX = new("scale-x", StateParameter.Bounded(Lens<NoiseSource, ShortSideExtent>.New(static noise => noise.ScaleX, static scale => noise => noise with { ScaleX = scale }), Feature),
        new("Scale X", "Feature width in frame short sides"));
    public static readonly NoiseSourceParameter ScaleY = new("scale-y", StateParameter.Bounded(Lens<NoiseSource, ShortSideExtent>.New(static noise => noise.ScaleY, static scale => noise => noise with { ScaleY = scale }), Feature),
        new("Scale Y", "Feature height in frame short sides"));
    public static readonly NoiseSourceParameter DriftX = new("drift-x", StateParameter.Bounded(Lens<NoiseSource, Drift>.New(static noise => noise.DriftX, static drift => noise => noise with { DriftX = drift }), Travel),
        new("Drift X", "Frame short sides per second the field moves right"));
    public static readonly NoiseSourceParameter DriftY = new("drift-y", StateParameter.Bounded(Lens<NoiseSource, Drift>.New(static noise => noise.DriftY, static drift => noise => noise with { DriftY = drift }), Travel),
        new("Drift Y", "Frame short sides per second the field moves up"));
    public static readonly NoiseSourceParameter Evolution = new("evolution", StateParameter.Bounded(Lens<NoiseSource, Hertz>.New(static noise => noise.Evolution, static evolution => noise => noise with { Evolution = evolution }), Hertz.Slow),
        new("Evolution", "Noise units per second the field changes through"));

    public StateParameter<NoiseSource> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record VortexField(VortexArms Arms, SignedAngle Twist, ShortSideExtent Radius, Steepness Tunnel, Hertz Spin) : IStateRecord<VortexField, VortexFieldParameter> {
    public static VortexField Default { get; } = new(VortexArms.Create(1), SignedAngle.Create(2.8f), ShortSideExtent.Create(0.5f), Steepness.Create(0.6f), Hertz.Create(1f / float.Tau));

    public Func<(double X, double Y), LatticePoint> Shape(float time) =>
        (ERational.FromSingle(Radius), ERational.FromDouble(double.Tau), ERational.FromSingle(Spin).Multiply(ERational.FromSingle(time))) switch {
            var (size, period, clock) => point => (double.Hypot(point.X, point.Y), double.Atan2(point.Y, point.X)) switch {
                var (radius, angle) => (
                    double.SinCos(LatticePoint.Split(ERational.FromDouble(angle).Multiply((int)Arms).Add(ERational.FromSingle(Twist).Multiply(ERational.One.Subtract(ERational.FromDouble(radius).Divide(size))))
                        .Divide(period).Add(clock)).Local.Multiply(period).ToDouble()),
                    ERational.Min(ERational.FromDouble(radius), size)),
            } switch {
                var ((sin, cos), reach) => new LatticePoint(reach.Multiply(ERational.FromDouble(cos)), reach.Multiply(ERational.FromDouble(sin)), reach.Multiply(ERational.FromSingle(Tunnel)), ERational.Zero),
            },
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VortexFieldParameter : IStateParameter<VortexField> {
    public static readonly VortexFieldParameter Arms = new("arms", StateParameter.Bounded(Lens<VortexField, VortexArms>.New(static vortex => vortex.Arms, static arms => vortex => vortex with { Arms = arms })),
        new("Arms", "Field repeats per turn, 0 draws concentric rings"));
    public static readonly VortexFieldParameter Twist = new("twist", StateParameter.Bounded(Lens<VortexField, SignedAngle>.New(static vortex => vortex.Twist, static twist => vortex => vortex with { Twist = twist }), Presentations.Angle),
        new("Twist", "Turn at center, falling to 0 at vortex radius"));
    public static readonly VortexFieldParameter Radius = new("radius", StateParameter.Bounded(Lens<VortexField, ShortSideExtent>.New(static vortex => vortex.Radius, static radius => vortex => vortex with { Radius = radius }), Presentations.Extent with { Soft = (0.01f, 2f) }),
        new("Radius", "Distance where tunnel stops deepening in frame short sides"));
    public static readonly VortexFieldParameter Tunnel = new("tunnel", StateParameter.Bounded(Lens<VortexField, Steepness>.New(static vortex => vortex.Tunnel, static tunnel => vortex => vortex with { Tunnel = tunnel }), new Presentation<float> { Soft = (0f, 4f) }),
        new("Tunnel", "Depth sampled along vortex axis per unit radius"));
    public static readonly VortexFieldParameter Spin = new("spin", StateParameter.Bounded(Lens<VortexField, Hertz>.New(static vortex => vortex.Spin, static spin => vortex => vortex with { Spin = spin }), Hertz.Presentation with { Soft = (-1f, 1f) }),
        new("Spin", "Turns per second about center"));

    public StateParameter<VortexField> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record FieldEnvelope(ShortSideExtent Width, ShortSideExtent Height) : IStateRecord<FieldEnvelope, FieldEnvelopeParameter> {
    public static FieldEnvelope Default { get; } = new(ShortSideExtent.Create(1f), ShortSideExtent.Create(1f));

    public float Falloff((double X, double Y) point) => double.Max(1d - double.Hypot(point.X / Width, point.Y / Height), 0d) switch { var edge => (float)(1d - (edge * edge)) };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldEnvelopeParameter : IStateParameter<FieldEnvelope> {
    private static readonly Presentation<float> Size = Presentations.Extent with { Soft = (0.1f, 4f) };

    public static readonly FieldEnvelopeParameter Width = new("width", StateParameter.Bounded(Lens<FieldEnvelope, ShortSideExtent>.New(static envelope => envelope.Width, static width => envelope => envelope with { Width = width }), Size),
        new("Width", "Envelope half width in frame short sides"));
    public static readonly FieldEnvelopeParameter Height = new("height", StateParameter.Bounded(Lens<FieldEnvelope, ShortSideExtent>.New(static envelope => envelope.Height, static height => envelope => envelope with { Height = height }), Size),
        new("Height", "Envelope half height in frame short sides"));

    public StateParameter<FieldEnvelope> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Emission(Wacton.Unicolour.Temperature Cool, Wacton.Unicolour.Temperature Hot) : IStateRecord<Emission, EmissionParameter> {
    private const int Intervals = 256;
    private static readonly (double Coolest, double Flame, double Daylight, double Hottest) Kelvins = (800d, 2000d, 6500d, 12_000d);

    public static Emission Default { get; } = new(new(Kelvins.Coolest), new(Kelvins.Flame));

    public static NumericDomain<Wacton.Unicolour.Temperature, double> Domain { get; } =
        new(static kelvin => new Wacton.Unicolour.Temperature(kelvin), static temperature => temperature.Cct, (Kelvins.Coolest, 100_000d), Presentations.Kelvin);

    public static ColorRamp Ramp { get; } = new ColorRamp.Smooth([
        new(Glow(Kelvins.Coolest), Fraction.MinValue), new(Glow(Kelvins.Flame), Fraction.Create((2000f - 800f) / (12_000f - 800f))),
        new(Glow(Kelvins.Daylight), Fraction.Create((6500f - 800f) / (12_000f - 800f))), new(Glow(Kelvins.Hottest), Fraction.MaxValue)]) { Space = CssSpace.SrgbLinear };

    public static Func<(Gamut Gamut, Emission Emission), Func<float, Vector3>> Span { get; } = memo<(Gamut Gamut, Emission Emission), Func<float, Vector3>>(static key => {
        Vector3[] samples = [.. Enumerable.Range(0, Intervals + 1).Select(i => Adaptation.Blackbody(key.Gamut, new(double.Lerp(key.Emission.Cool.Cct, key.Emission.Hot.Cct, i / (double)Intervals))).RgbLinear switch {
            var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B),
        })];
        return t => (float.Clamp(t, 0f, 1f) * Intervals) switch { var texel => int.Min((int)texel, Intervals - 1) switch { var i => Vector3.Lerp(samples[i], samples[i + 1], texel - i) } };
    });

    private static SKColorF Glow(double kelvin) =>
        Adaptation.Peaked(new Unicolour(kelvin, Locus.Blackbody)).MapToRgbGamut() switch { var glow => new SKColorF((float)glow.Rgb.R, (float)glow.Rgb.G, (float)glow.Rgb.B) };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EmissionParameter : IStateParameter<Emission> {
    public static readonly EmissionParameter Cool = new("cool", new StateParameter<Emission>.Numeric<Wacton.Unicolour.Temperature, double>(
        Lens<Emission, Wacton.Unicolour.Temperature>.New(static emission => emission.Cool, static cool => emission => emission with { Cool = cool }), Emission.Domain),
        new("Cool", "Blackbody temperature at field low end"));
    public static readonly EmissionParameter Hot = new("hot", new StateParameter<Emission>.Numeric<Wacton.Unicolour.Temperature, double>(
        Lens<Emission, Wacton.Unicolour.Temperature>.New(static emission => emission.Hot, static hot => emission => emission with { Hot = hot }), Emission.Domain),
        new("Hot", "Blackbody temperature at field high end"));

    public StateParameter<Emission> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Aperture(ApertureBlades Blades, SignedAngle Rotation, Fraction Roundness, Fraction Obstruction, AnamorphicSqueeze Squeeze) : IStateRecord<Aperture, ApertureParameter> {
    public static Aperture Default { get; } = new(ApertureBlades.Six, SignedAngle.Create(float.Pi / 3f), Fraction.MinValue, Fraction.MinValue, AnamorphicSqueeze.Spherical);

    public Fin<Iris> Pupil => ValidationResults.ToFin(Iris.Validate(Blades, Rotation, Roundness, Obstruction, Squeeze, out Iris? iris), iris);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ApertureParameter : IStateParameter<Aperture> {
    public static readonly ApertureParameter Blades = new("blades", StateParameter.Bounded(Lens<Aperture, ApertureBlades>.New(static aperture => aperture.Blades, static blades => aperture => aperture with { Blades = blades })),
        new("Flaps", "Aperture blade count shaping bokeh and diffraction spikes"));
    public static readonly ApertureParameter Rotation = new("rotation", StateParameter.Bounded(Lens<Aperture, SignedAngle>.New(static aperture => aperture.Rotation, static rotation => aperture => aperture with { Rotation = rotation }), Presentations.Angle),
        new("Angle", "Counterclockwise turn of aperture blades"));
    public static readonly ApertureParameter Roundness = new("roundness", StateParameter.Bounded(Lens<Aperture, Fraction>.New(static aperture => aperture.Roundness, static roundness => aperture => aperture with { Roundness = roundness })),
        new("Roundness", "Blade curvature from straight at 0% to circle at 100%"));
    public static readonly ApertureParameter Obstruction = new("obstruction", StateParameter.Bounded(Lens<Aperture, Fraction>.New(static aperture => aperture.Obstruction, static obstruction => aperture => aperture with { Obstruction = obstruction })),
        new("Catadioptric size", "Mirror lens central obstruction turning bokeh into rings"));
    public static readonly ApertureParameter Squeeze = new("squeeze", StateParameter.Bounded(Lens<Aperture, AnamorphicSqueeze>.New(static aperture => aperture.Squeeze, static squeeze => aperture => aperture with { Squeeze = squeeze })),
        new("Anamorphic", "Horizontal aperture squeeze, 2 for 2x anamorphic lens"));

    public StateParameter<Aperture> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Highlights(HighlightLevel Threshold, Fraction Softness) : IStateRecord<Highlights, HighlightsParameter> {
    public static Highlights Default { get; } = new(HighlightLevel.White, Fraction.Create(0.1f));

    public Fin<HighlightKey> Key => ValidationResults.ToFin(HighlightKey.Validate(Threshold, Softness, out HighlightKey key), key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HighlightsParameter : IStateParameter<Highlights> {
    public static readonly HighlightsParameter Threshold = new("threshold", StateParameter.Bounded(Lens<Highlights, HighlightLevel>.New(static key => key.Threshold, static threshold => key => key with { Threshold = threshold })),
        new("Threshold", "Scene-linear value above which highlights feed the effect"));
    public static readonly HighlightsParameter Softness = new("softness", StateParameter.Bounded(Lens<Highlights, Fraction>.New(static key => key.Softness, static softness => key => key with { Softness = softness })),
        new("Smoothness", "Knee width below the threshold"));

    public StateParameter<Highlights> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PointCells(ShortSideExtent Spacing, Fraction Density, Fraction Size, Fraction Variation, Magnitude Spread, Fraction Softness) : IStateRecord<PointCells, PointCellsParameter> {
    private static readonly JitteredCells Sites = new(0.5f);
    private static readonly float Largest = (1f - Sites.Randomness) / 2f;

    public static PointCells Default { get; } = new(
        ShortSideExtent.Create(0.01f * ReferenceFrame.GreaterSide), Fraction.Create(0.5f), Fraction.Create(2f * 0.2f), Fraction.Create(0.5f), Magnitude.Unit, Fraction.Create(0.5f));

    public bool Empty => Size == Fraction.MinValue || Density == Fraction.MinValue;

    public (double Radius, double Gain) Draw(Vector4 draw) => ((float)Size * (1d - ((float)Variation * (double)draw.Y)) * Largest, double.Exp2(-(float)Spread * draw.Z));

    public (float Coverage, float Value) Disc((ERational X, ERational Y) point, double side, uint field, Func<float, float> tone) {
        (ERational spacing, uint sites, uint colors) = (ERational.FromSingle(Spacing), CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        ERational width = ERational.One.Divide(spacing.Multiply(ERational.FromDouble(side)));
        (ERational x, ERational y, ERational half) = (point.X.Divide(spacing), point.Y.Divide(spacing), width.Divide(2));
        (ERational left, ERational top, ERational right, ERational bottom) = (x.Subtract(half), y.Subtract(half), x.Add(half), y.Add(half));
        ERational area = width.Multiply(width);
        PointCells cells = this;
        (ERational coverage, ERational value) = Empty ? (ERational.Zero, ERational.Zero) : Sites.Covering(left, top, right, bottom, ERational.FromSingle(Largest)).Aggregate((Coverage: ERational.Zero, Value: ERational.Zero), (sum, cell) => {
            Vector4 draw = CoordinateHash.Uniforms(cell.X, cell.Y, colors);
            (double radius, double gain) = cells.Draw(draw);
            if (draw.X >= cells.Density || radius is 0d)
                return sum;
            Vector2 offset = Sites.Offset(cell.X, cell.Y, sites);
            (ERational siteX, ERational siteY) = (ERational.FromEInteger(cell.X).Add(ERational.FromSingle(offset.X)), ERational.FromEInteger(cell.Y).Add(ERational.FromSingle(offset.Y)));
            ERational weight = new SoftDisc(radius, cells.Softness).Mass(left.Subtract(siteX), top.Subtract(siteY), right.Subtract(siteX), bottom.Subtract(siteY)).Multiply(ERational.FromDouble(gain)).Divide(area);
            return (sum.Coverage.Add(weight), sum.Value.Add(weight.Multiply(ERational.FromSingle(tone(draw.X / cells.Density)))));
        });
        return (coverage.ToSingle(), value.ToSingle());
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointCellsParameter : IStateParameter<PointCells> {
    public static readonly PointCellsParameter Spacing = new("spacing", StateParameter.Bounded(Lens<PointCells, ShortSideExtent>.New(static cells => cells.Spacing, static spacing => cells => cells with { Spacing = spacing }), Presentations.Extent with { Soft = (0.01f, 0.5f) }),
        new("Spacing", "Point cell spacing in frame short sides"));
    public static readonly PointCellsParameter Density = new("density", StateParameter.Bounded(Lens<PointCells, Fraction>.New(static cells => cells.Density, static density => cells => cells with { Density = density })),
        new("Density", "Share of cells holding a point"));
    public static readonly PointCellsParameter Size = new("size", StateParameter.Bounded(Lens<PointCells, Fraction>.New(static cells => cells.Size, static size => cells => cells with { Size = size })),
        new("Size", "Disc radius as share of largest radius fitting cell"));
    public static readonly PointCellsParameter Variation = new("variation", StateParameter.Bounded(Lens<PointCells, Fraction>.New(static cells => cells.Variation, static variation => cells => cells with { Variation = variation })),
        new("Variation", "Share each radius shrinks at random"));
    public static readonly PointCellsParameter Spread = new("spread", StateParameter.Bounded(Lens<PointCells, Magnitude>.New(static cells => cells.Spread, static spread => cells => cells with { Spread = spread }), Stops.Presentation with { Soft = (0f, 10f), Origin = None }),
        new("Brightness spread", "Stops each point dims at random"));
    public static readonly PointCellsParameter Softness = new("softness", StateParameter.Bounded(Lens<PointCells, Fraction>.New(static cells => cells.Softness, static softness => cells => cells with { Softness = softness })),
        new("Softness", "Share of each disc's radius fading at its edge"));

    public StateParameter<PointCells> Kind { get; }
    public ParameterText Text { get; }
}

public sealed class RampLight {
    private const int Texels = 256;
    private static readonly Func<(ColorRamp Ramp, Gamut Gamut), Fin<RampLight>> Tables = memo(static ((ColorRamp Ramp, Gamut Gamut) key) =>
        toSeq(Range(0, Texels + 1)).Traverse(static texel => Fraction.Validated(texel / (float)Texels)).As().Map(stops => new RampLight([.. stops.Map(at => Light(key.Ramp.At(at), key.Gamut))])));

    private readonly Arr<Vector4> texels;

    private RampLight(Arr<Vector4> texels) => this.texels = texels;

    public ReadOnlySpan<Vector4> Lights => texels.AsSpan();

    public static Fin<RampLight> Of(ColorRamp ramp, Gamut gamut) => Tables((ramp, gamut));

    public static Vector4 Light(SKColorF color, Gamut gamut) =>
        new Unicolour(ColourSpace.Rgb, color.Red, color.Green, color.Blue).ConvertToConfiguration(gamut.Configuration).RgbLinear switch {
            var linear => new((float)linear.R, (float)linear.G, (float)linear.B, color.Alpha),
        };

    public Vector4 Sample(float position) =>
        float.IsNaN(position) ? new Vector4(float.NaN)
            : (float.Clamp(position, 0f, 1f) * Texels) switch { var texel => int.Min((int)texel, Texels - 1) switch { var at => Vector4.Lerp(texels[at], texels[at + 1], texel - at) } };
}

public sealed class RampFootprint {
    private const int Bins = 4096;
    private static readonly Func<(ColorRamp Ramp, Option<WaveProfile> Profile), Fin<RampFootprint>> Tables = memo(static ((ColorRamp Ramp, Option<WaveProfile> Profile) key) =>
        toSeq(Range(0, Bins)).Traverse(bin => Fraction.Validated(key.Profile.Match(Some: wave => wave.Level((bin + 0.5f) / Bins), None: () => (bin + 0.5f) / Bins))).As()
            .Map(stops => new RampFootprint([.. stops.Map(at => key.Ramp.At(at) switch { var color => new Vector4(color.Red * color.Alpha, color.Green * color.Alpha, color.Blue * color.Alpha, color.Alpha) })])));

    private readonly Arr<Vector4> bins;
    private readonly Arr<Vector4> sums;

    private RampFootprint(Arr<Vector4> bins) =>
        (this.bins, sums) = (bins, [.. toSeq(bins).Scan((X: 0d, Y: 0d, Z: 0d, W: 0d), static (sum, bin) => (sum.X + bin.X, sum.Y + bin.Y, sum.Z + bin.Z, sum.W + bin.W))
            .Map(static sum => new Vector4((float)sum.X, (float)sum.Y, (float)sum.Z, (float)sum.W))]);

    public static Fin<RampFootprint> Of(ColorRamp ramp, Option<WaveProfile> profile) => Tables((ramp, profile));

    public Vector4 Mean(double position, double footprint, bool periodic) =>
        double.IsPositiveInfinity(footprint) ? Unassociated(sums[Bins] / Bins) : Mean(ERational.FromDouble(position), ERational.FromDouble(footprint), periodic);

    public Vector4 Mean(ERational position, ERational footprint, bool periodic) {
        (ERational low, ERational high, ERational total) = (position.Subtract(footprint.Divide(2)), position.Add(footprint.Divide(2)), footprint.Multiply(Bins));
        if (periodic) {
            (EInteger periods, ERational remainder) = LatticePoint.Split(footprint);
            ERational start = LatticePoint.Split(low).Local;
            ERational end = start.Add(remainder);
            return Unassociated((sums[Bins] * ERational.FromEInteger(periods).Divide(total).ToSingle())
                + Segment(start.Multiply(Bins), ERational.Min(end, ERational.One).Multiply(Bins), total)
                + (end.CompareTo(ERational.One) > 0 ? Segment(ERational.Zero, end.Subtract(ERational.One).Multiply(Bins), total) : Vector4.Zero));
        }
        return Unassociated((low.Sign < 0 ? bins[0] * ERational.Min(ERational.Zero, high).Subtract(low).Divide(footprint).ToSingle() : Vector4.Zero)
            + (high.CompareTo(ERational.One) > 0 ? bins[Bins - 1] * high.Subtract(ERational.Max(low, ERational.One)).Divide(footprint).ToSingle() : Vector4.Zero)
            + Segment(ERational.Max(low, ERational.Zero).Multiply(Bins), ERational.Min(high, ERational.One).Multiply(Bins), total));
    }

    private static Vector4 Unassociated(Vector4 mean) => mean.W > 0f ? new Vector4(mean.AsVector3() / mean.W, mean.W) : Vector4.Zero;

    private Vector4 Segment(ERational low, ERational high, ERational total) {
        if (low.CompareTo(high) >= 0)
            return Vector4.Zero;
        (int first, int last) = (LatticePoint.Split(low).Cell.ToInt32Checked(), LatticePoint.Split(high).Cell.ToInt32Checked());
        ERational next = ERational.FromInt32(first + 1);
        return (bins[first] * ERational.Min(high, next).Subtract(low).Divide(total).ToSingle())
            + (last > first + 1 ? (sums[last] - sums[first + 1]) * ERational.One.Divide(total).ToSingle() : Vector4.Zero)
            + (last > first && high.CompareTo(last) > 0 ? bins[last] * high.Subtract(last).Divide(total).ToSingle() : Vector4.Zero);
    }
}

public sealed record NoiseTexture(
    NoiseSource Noise, Placement Placement, Gated<VortexField> VortexShape, ShortSideOffset Warp, NoiseBasis WarpBasis, ColorRamp Colors,
    Gated<FieldEnvelope> Envelope, Gated<Emission> Incandescence, Gated<OpticalDepth> Extinction, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<NoiseTexture, NoiseTextureParameter>, IPixelStage<NoiseTexture> {
    private static readonly Fractal VoronoiOctaves = new Fractal(FractalDetail.Create(1f), Fraction.Create(0.5f), FractalLacunarity.Create(2f));

    public static NoiseTexture Default { get; } = new(
        Presets.Source(NoiseBasis.Default, ShortSideExtent.Create(1f / 5f), ShortSideExtent.Create(1f / 5f)), Placement.Default, new(Enabled: false, VortexField.Default), ShortSideOffset.Neutral,
        NoiseBasis.Wave.Default with { Fractal = new Fractal(FractalDetail.Create(2f), Fraction.Create(0.186957f), FractalLacunarity.Create(2f)), Distortion = NoiseDistortion.Create(14.9f), Phase = SignedAngle.Create(-3f), DetailScale = NoiseGain.Create(1.3f) },
        Presets.Grayscale, new(Enabled: false, FieldEnvelope.Default), new(Enabled: false, Emission.Default), new(Enabled: false, OpticalDepth.MinValue), GeneratedLayer.Default, Seed.MinValue, Timing.Default);

    public static NoiseTexture TurbulentNoise { get; } = Default with { Noise = Default.Noise with { Evolution = Hertz.Create(0.3f) } };

    public static NoiseTexture BlockNoise { get; } = Default with { Noise = Default.Noise with { Basis = NoiseBasis.White.Default } };

    public static NoiseTexture Diamonds { get; } = Default with {
        Noise = Default.Noise with { Basis = NoiseBasis.F2.Default with { Fractal = Fractal.Plain, Cellular = Cellular.Default with { Randomness = Fraction.MinValue } } },
    };

    public static NoiseTexture VoronoiNoise { get; } = TurbulentNoise with { Noise = TurbulentNoise.Noise with { Basis = NoiseBasis.F1.Default with { Fractal = VoronoiOctaves } } };

    public static NoiseTexture Sparkle { get; } = VoronoiNoise with {
        Noise = VoronoiNoise.Noise with {
            Basis = NoiseBasis.SmoothF1.Default with { Fractal = VoronoiOctaves, Cellular = Cellular.Default with { Metric = CellMetric.Minkowski, Exponent = MinkowskiExponent.Create(1f) } },
        },
    };

    public static NoiseTexture VoronoiCells { get; } = Default with {
        Noise = Default.Noise with { Basis = NoiseBasis.EdgeDistance.Default with { Fractal = Fractal.Plain }, DriftY = Drift.Create(-0.6f) },
    };

    public static NoiseTexture Waves { get; } = Default with {
        Noise = Presets.Source(NoiseBasis.Wave.Default with { Distortion = NoiseDistortion.Create(4f), DetailScale = NoiseGain.Create(2f) }, ShortSideExtent.Create(float.Tau / 20f), ShortSideExtent.Create(float.Tau / 20f)) with {
            DriftX = Drift.Create(-0.3f / 20f),
        },
    };

    public static NoiseTexture LightLeak { get; } = (Fade: (Low: 0.1f, High: 0.6f), Bias: (Low: 0.225f, High: 0.775f)) switch {
        var (fade, bias) => Default with {
            Noise = Presets.Source(NoiseBasis.Plain, ShortSideExtent.Create(1f / 0.7f), ShortSideExtent.Create(1f / 0.7f)) with { Evolution = Hertz.Create(0.5f) },
            Colors = Presets.Sampled(CssSpace.SrgbLinear, n =>
                new Vector4(Vector3.Lerp(new(1f, 0.7529f, 0.6584f), new(0.0437f, 0.0116f, 1f), Presets.Map(n, bias)), 1f - Presets.Map(n, fade)), Fraction.Create(0.1f), Fraction.Create(0.225f), Fraction.Create(0.6f)),
            Layer = GeneratedLayer.Added,
        },
    };

    public static NoiseTexture Flow { get; } = (Cover: (Low: 0.37f, High: 0.87f), Tint: (Low: 0.62f, High: 1.1f)) switch {
        var (cover, tint) => Default with {
            Noise = Presets.Source((NoiseBasis.Fbm.Default with { Fractal = new(FractalDetail.Create(2.48f), Fraction.Create(0f), FractalLacunarity.Create(12.4f)), Distortion = NoiseDistortion.Create(7.25f) }), ShortSideExtent.Create(1f), ShortSideExtent.Create(1f)) with { Evolution = Hertz.Create(0.15f) },
            Warp = ShortSideOffset.Create(0.0566f),
            Colors = Presets.Sampled(CssSpace.SrgbLinear, n => new Vector4(
                Vector3.Lerp(new(0.305f, 0.129f, 1f), new(1f, 0.059f, 0.298f), ShapeEdge.Smoother(Presets.Map(n, tint))), Presets.Map(n, cover)),
                Fraction.Create(0.37f), Fraction.Create(0.62f), Fraction.Create(0.87f), Fraction.MaxValue),
            Layer = GeneratedLayer.Added,
        },
    };

    public static NoiseTexture Vortex { get; } = Default with {
        Noise = Presets.Source((NoiseBasis.Fbm.Default with { Fractal = new(FractalDetail.Create(4f), Fraction.Create(0.2f), FractalLacunarity.Create(3f)), Distortion = NoiseDistortion.Create(0.5f) }), ShortSideExtent.Create(1f / 6f), ShortSideExtent.Create(1f / 6f)) with { Evolution = Hertz.Create(0.3f) },
        VortexShape = Default.VortexShape with { Enabled = true },
        Colors = Presets.Sampled(CssSpace.SrgbLinear, static n =>
            (n * ((0.8f * 2f * new Vector3(0.134f, 0.431f, 0.5f)) + new Vector3(0.2f))) switch { var tint => new Vector4(tint * tint * tint * tint, 1f) }, Fraction.MinValue, Fraction.Create(0.5f), Fraction.MaxValue),
    };

    public static NoiseTexture Fire { get; } = Default with {
        Noise = Presets.Source((NoiseBasis.Fbm.Default with { Fractal = new(FractalDetail.Create(4f), Fraction.Create(0.5f), FractalLacunarity.Create(2f)), Distortion = NoiseDistortion.Create(1f) }), ShortSideExtent.Create(0.5f / 0.7f), ShortSideExtent.Create(1f / 0.7f)) with { DriftY = Drift.Create(5f) },
        Placement = Placement.Default with { Center = new(FrameAxis.Middle, FrameAxis.End) },
        Colors = Presets.Threshold(CssSpace.SrgbLinear, Vector3.One, Fraction.Create(0.17f), Fraction.MaxValue),
        Envelope = Default.Envelope with { Enabled = true },
        Incandescence = Default.Incandescence with { Enabled = true },
        Layer = GeneratedLayer.Added,
    };

    public static NoiseTexture Smoke { get; } = Default with {
        Noise = Presets.Source((NoiseBasis.Fbm.Default with { Fractal = new(FractalDetail.Create(5f), Fraction.Create(0.55f), FractalLacunarity.Create(2f)), Distortion = NoiseDistortion.Create(0.5f) }), ShortSideExtent.Create(0.2f * ReferenceFrame.GreaterSide), ShortSideExtent.Create(1.5f * 0.2f * ReferenceFrame.GreaterSide)) with {
            DriftY = Drift.Create(0.05f * ReferenceFrame.GreaterSide),
            Evolution = Hertz.Create(0.3f),
        },
        Colors = Presets.Threshold(CssSpace.SrgbLinear, new Vector3(0.04f), Fraction.Create(0.4f), Fraction.Create(0.8f)) with { Easing = Presets.Ease },
        Extinction = new(Enabled: true, OpticalDepth.Create(2f)),
    };

    public static NoiseTexture Mist { get; } = Default with {
        Noise = Presets.Source((NoiseBasis.Fbm.Default with { Fractal = new(FractalDetail.Create(3f), Fraction.Create(0.45f), FractalLacunarity.Create(2f)), Distortion = NoiseDistortion.Create(0f) }), ShortSideExtent.Create(0.5f * ReferenceFrame.GreaterSide), ShortSideExtent.Create(0.5f * 0.5f * ReferenceFrame.GreaterSide)) with {
            DriftX = Drift.Create(0.02f * ReferenceFrame.GreaterSide),
            Evolution = Hertz.Create(0.1f),
        },
        Colors = Presets.Threshold(CssSpace.SrgbLinear, new Vector3(0.7f), Fraction.Create(0.3f), Fraction.Create(0.9f)) with { Easing = Presets.Ease },
        Extinction = new(Enabled: true, OpticalDepth.Create(0.6f)),
    };

    public static Seq<(string Name, EffectLook<NoiseTexture> Look)> Looks { get; } = [
        Presets.Look(nameof(TurbulentNoise), TurbulentNoise), Presets.Look(nameof(BlockNoise), BlockNoise), Presets.Look(nameof(Diamonds), Diamonds),
        Presets.Look(nameof(VoronoiNoise), VoronoiNoise), Presets.Look(nameof(Sparkle), Sparkle), Presets.Look(nameof(VoronoiCells), VoronoiCells),
        Presets.Look(nameof(Waves), Waves), Presets.Look(nameof(LightLeak), LightLeak), Presets.Look(nameof(Flow), Flow),
        Presets.Look(nameof(Vortex), Vortex), Presets.Look(nameof(Smoke), Smoke), Presets.Look(nameof(Mist), Mist)];

    public static Option<PixelPass> Pass(NoiseTexture state, PassContext context) {
        (uint field, float time) = (state.Seed.Field(12u, 0u), state.Timing.At(context));
        Func<LatticePoint, float> noise = state.Noise.Sampler(time, CoordinateHash.Branch(field, 0u));
        Func<(double X, double Y), LatticePoint> shape = state.VortexShape.Active.Map(vortex => vortex.Shape(time))
            .IfNone(static point => new LatticePoint(ERational.FromDouble(point.X), ERational.FromDouble(point.Y), ERational.Zero, ERational.Zero));
        Func<LatticePoint, LatticePoint> warp = state.Warp == ShortSideOffset.Neutral ? static at => at
            : (state.Noise with { Basis = state.WarpBasis }, ERational.FromSingle(state.Warp).Multiply(2)) switch {
                var (source, gain) => (source.Sampler(time, CoordinateHash.Branch(field, 1u)), source.Sampler(time, CoordinateHash.Branch(field, 2u))) switch {
                    var (across, along) => at => at with { X = at.X.Add(gain.Multiply(ERational.FromSingle(across(at) - 0.5f))), Y = at.Y.Add(gain.Multiply(ERational.FromSingle(along(at) - 0.5f))) },
                },
            };
        Func<(double X, double Y), float> rim = state.Envelope.Active.Map<Func<(double X, double Y), float>>(static envelope => envelope.Falloff).IfNone(static _ => 0f);
        Func<Vector4, Vector3> light = state.Incandescence.Active.Map<Func<Vector4, Vector3>>(emission => Emission.Span((context.Working, emission)) switch {
            var span => color => span(color.W),
        }).IfNone(static color => color.AsVector3());
        Func<float, float> opacity = state.Extinction.Active.Map<Func<float, float>>(static depth => cover => 1f - float.Exp(-depth * cover)).IfNone(static cover => cover);
        return Some(state.Layer.Pass(RampLight.Of(state.Colors, context.Working), colors => state.Placement.Frame(context.Extent).Fill(point =>
            colors.Sample(noise(warp(shape(point))) - rim(point)) switch { var color => new Vector4(light(color), opacity(color.W)) })));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseTextureParameter : IStateParameter<NoiseTexture> {
    public static readonly NoiseTextureParameter Noise = new("noise", new StateParameter<NoiseTexture>.Record<NoiseSource>(Lens<NoiseTexture, NoiseSource>.New(static texture => texture.Noise, static noise => texture => texture with { Noise = noise })),
        new("Noise", "Field pattern, feature size, and motion"));
    public static readonly NoiseTextureParameter Placement = new("placement", new StateParameter<NoiseTexture>.Record<Placement>(Lens<NoiseTexture, Placement>.New(static texture => texture.Placement, static placement => texture => texture with { Placement = placement })),
        new("Placement", "Field position and turn"));
    public static readonly NoiseTextureParameter Vortex = new("vortex", new StateParameter<NoiseTexture>.OptionalRecord<VortexField>(Lens<NoiseTexture, Gated<VortexField>>.New(static texture => texture.VortexShape, static vortex => texture => texture with { VortexShape = vortex })),
        new("Vortex", "Swirls field around center into tunnel"));
    public static readonly NoiseTextureParameter Warp = new("warp", StateParameter.Bounded(Lens<NoiseTexture, ShortSideOffset>.New(static texture => texture.Warp, static warp => texture => texture with { Warp = warp }), Presentations.Offset),
        new("Warp", "Warp noise displacement in frame short sides"));
    public static readonly NoiseTextureParameter WarpBasis = new("warp-basis", new StateParameter<NoiseTexture>.Variant<NoiseBasis>(Lens<NoiseTexture, NoiseBasis>.New(static texture => texture.WarpBasis, static basis => texture => texture with { WarpBasis = basis })),
        new("Warp noise", "Pattern displacing the field"));
    public static readonly NoiseTextureParameter Colors = new("colors", new StateParameter<NoiseTexture>.Gradient(Lens<NoiseTexture, ColorRamp>.New(static texture => texture.Colors, static colors => texture => texture with { Colors = colors })),
        new("Colors", "Color and coverage by noise value"));
    public static readonly NoiseTextureParameter Envelope = new("envelope", new StateParameter<NoiseTexture>.OptionalRecord<FieldEnvelope>(Lens<NoiseTexture, Gated<FieldEnvelope>>.New(static texture => texture.Envelope, static envelope => texture => texture with { Envelope = envelope })),
        new("Envelope", "Elliptical falloff shaping field"));
    public static readonly NoiseTextureParameter Incandescence = new("incandescence", new StateParameter<NoiseTexture>.OptionalRecord<Emission>(Lens<NoiseTexture, Gated<Emission>>.New(static texture => texture.Incandescence, static emission => texture => texture with { Incandescence = emission })),
        new("Blackbody", "Blackbody light between cool and hot temperatures"));
    public static readonly NoiseTextureParameter Extinction = new("extinction", StateParameter.OptionalBounded(Lens<NoiseTexture, Gated<OpticalDepth>>.New(static texture => texture.Extinction, static extinction => texture => texture with { Extinction = extinction })),
        new("Extinction", "Optical depth at full coverage, absorbing light like smoke"));
    public static readonly NoiseTextureParameter Layer = new("layer", new StateParameter<NoiseTexture>.Record<GeneratedLayer>(Lens<NoiseTexture, GeneratedLayer>.New(static texture => texture.Layer, static layer => texture => texture with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly NoiseTextureParameter Seed = new("seed", StateParameter.Bounded(Lens<NoiseTexture, Seed>.New(static texture => texture.Seed, static seed => texture => texture with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly NoiseTextureParameter Timing = new("timing", new StateParameter<NoiseTexture>.Record<Timing>(Lens<NoiseTexture, Timing>.New(static texture => texture.Timing, static timing => texture => texture with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<NoiseTexture> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Fog(
    NoiseSource Noise, Fraction Variation, FogDistance Start, Gated<FogAttenuation> Attenuation, Gated<FogDistance> Cutoff, Gated<ShortSideLength> Diffusion,
    Fraction Amount, ColorRamp Colors, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Fog, FogParameter>, IPixelStage<Fog> {
    public static Fog Default { get; } = new(
        Presets.Source((NoiseBasis.Fbm.Default with { Fractal = new(FractalDetail.Create(4f), Fraction.Create(0.2f), FractalLacunarity.Create(2f)), Distortion = NoiseDistortion.Create(0f) }) with { Dimensions = NoiseDimensions.Four }, ShortSideExtent.Create(1f), ShortSideExtent.Create(1f)), Fraction.MinValue, FogDistance.Neutral,
        new(Enabled: true, FogAttenuation.Standard), new(Enabled: false, FogDistance.Neutral), new(Enabled: false, ShortSideLength.Neutral), Fraction.MaxValue,
        new ColorRamp.Smooth([new(new SKColorF(0.85098f, 0.85098f, 0.85098f), Fraction.MinValue)]), GeneratedLayer.Default, Seed.MinValue, Timing.Default);

    public static Fog Haze { get; } = Default with {
        Attenuation = Default.Attenuation with { Enabled = false },
        Amount = Fraction.Create(0.35f),
        Diffusion = new(Enabled: true, ShortSideLength.Create(0.75f * ReferenceFrame.GreaterSide)),
        Colors = Presets.Solid(new Vector3(Exposure.MiddleGray)),
    };

    public static Seq<(string Name, EffectLook<Fog> Look)> Looks { get; } = [Presets.Look(nameof(Haze), Haze)];

    public static Seq<GuideChannel> Channels(Fog state) => state.Attenuation.Active.Map(static _ => GuideChannel.Depth).ToSeq();

    public static Option<PixelPass> Pass(Fog state, PassContext context) =>
        state.Attenuation.Active.Match(
            Some: attenuation => from depth in context.Guides.Find(GuideChannel.Depth)
                                 from camera in context.Camera
                                 select Opacity(state, context, depth, camera, attenuation),
            None: static () => Some<Func<long, long, float>>(static (_, _) => 1f))
        .Map(opacity => state.Diffusion.Active.Match(
            Some: radius => new PixelPass.Frame((frame, progress) => frame.Filter(source => {
                PixelFrame spread = new(frame.Origin, frame.Size, frame.Extent);
                using Mat target = spread.Header();
                return source.Blur(target, radius.Pixels(context.Extent))
                    .Bind(_ => state.Layer.Pass(RampLight.Of(state.Colors, context.Working), table => Fill(state, table, opacity, (column, line) => spread.Row(line)[(int)(column - spread.Origin.X)].AsVector3())).Run(frame, progress));
            })),
            None: () => state.Layer.Pass(RampLight.Of(state.Colors, context.Working), table => Fill(state, table, opacity, static (_, _) => Vector3.One))));

    private static Func<long, long, float> Opacity(Fog state, PassContext context, PixelFrame depth, Camera camera, FogAttenuation attenuation) {
        FieldFrame frame = Placement.Default.Frame(context.Extent);
        Func<LatticePoint, float> noise = state.Noise.Sampler(state.Timing.At(context), state.Seed.Field(13u, 0u));
        Func<long, long, float, float> density = state.Variation == Fraction.MinValue ? static (_, _, _) => 1f
            : (column, line, path) => frame.Upward(column, line) switch {
                var point => float.Max(1f + ((float)state.Variation * ((2f * noise(new LatticePoint(ERational.FromDouble(point.X), ERational.FromDouble(point.Y), ERational.Zero, ERational.FromSingle(path)))) - 1f)), 0f),
            };
        return new Extinction(state.Start, attenuation, state.Cutoff.Active.Map(static cutoff => (float)cutoff)).Field(depth, camera, density);
    }

    private static Action<Span<Vector4>, long, long> Fill(Fog state, RampLight table, Func<long, long, float> opacity, Func<long, long, Vector3> light) =>
        (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                float coverage = opacity(column + i, line);
                Vector4 medium = table.Sample(coverage);
                row[i] = new Vector4(medium.AsVector3() * light(column + i, line), (float)state.Amount * coverage * medium.W);
            }
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FogParameter : IStateParameter<Fog> {
    private static readonly Presentation<float> Reach = new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Track = Some(Presentations.Cubic) };

    public static readonly FogParameter Noise = new("noise", new StateParameter<Fog>.Record<NoiseSource>(Lens<Fog, NoiseSource>.New(static fog => fog.Noise, static noise => fog => fog with { Noise = noise })),
        new("Wisps", "Medium pattern, feature size, and motion"));
    public static readonly FogParameter Variation = new("variation", StateParameter.Bounded(Lens<Fog, Fraction>.New(static fog => fog.Variation, static variation => fog => fog with { Variation = variation })),
        new("Variation", "Wisp density variation, 0% uniform"));
    public static readonly FogParameter Start = new("start", StateParameter.Bounded(Lens<Fog, FogDistance>.New(static fog => fog.Start, static start => fog => fog with { Start = start }), Reach with { Soft = (0f, 100f) }),
        new("Start", "Camera distance where medium begins"));
    public static readonly FogParameter Attenuation = new("attenuation", StateParameter.OptionalBounded(Lens<Fog, Gated<FogAttenuation>>.New(static fog => fog.Attenuation, static attenuation => fog => fog with { Attenuation = attenuation }),
        Reach with { Soft = (0.3f, 1000f), Track = Some(Presentations.Decades) }),
        new("Attenuation", "Distance through medium passing 1/e of light behind it"));
    public static readonly FogParameter Cutoff = new("cutoff", StateParameter.OptionalBounded(Lens<Fog, Gated<FogDistance>>.New(static fog => fog.Cutoff, static cutoff => fog => fog with { Cutoff = cutoff }), Reach with { Soft = (0f, 1000f) }),
        new("Cutoff", "Distance past which surfaces and background stay clear"));
    public static readonly FogParameter Diffusion = new("diffusion", StateParameter.OptionalBounded(Lens<Fog, Gated<ShortSideLength>>.New(static fog => fog.Diffusion, static diffusion => fog => fog with { Diffusion = diffusion }), Presentations.Length with { Soft = (0f, 2f) }),
        new("Diffusion", "Blur radius of frame light the medium scatters, off lights medium evenly"));
    public static readonly FogParameter Amount = new("amount", StateParameter.Bounded(Lens<Fog, Fraction>.New(static fog => fog.Amount, static amount => fog => fog with { Amount = amount })),
        new("Density", "Share of medium coverage drawn"));
    public static readonly FogParameter Colors = new("colors", new StateParameter<Fog>.Gradient(Lens<Fog, ColorRamp>.New(static fog => fog.Colors, static colors => fog => fog with { Colors = colors })),
        new("Colors", "Medium color and coverage, near to far by optical depth"));
    public static readonly FogParameter Layer = new("layer", new StateParameter<Fog>.Record<GeneratedLayer>(Lens<Fog, GeneratedLayer>.New(static fog => fog.Layer, static layer => fog => fog with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly FogParameter Seed = new("seed", StateParameter.Bounded(Lens<Fog, Seed>.New(static fog => fog.Seed, static seed => fog => fog with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly FogParameter Timing = new("timing", new StateParameter<Fog>.Record<Timing>(Lens<Fog, Timing>.New(static fog => fog.Timing, static timing => fog => fog with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<Fog> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Starfield(
    Placement Placement, NoiseBasis Stars, ShortSideExtent StarScale, ShortSideExtent StarRadius, ColorRamp StarColors, Stops StarExposure, Fraction Twinkle, Hertz TwinkleRate,
    NoiseSource Nebula, Fraction NebulaDensity, ColorRamp NebulaColors, Stops NebulaExposure, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Starfield, StarfieldParameter>, IPixelStage<Starfield> {
    public static Starfield Default { get; } = new(
        Placement.Default, NoiseBasis.SmoothF1.Default with { Fractal = new Fractal(FractalDetail.Create(2.56f), Fraction.Create(0.165441f), FractalLacunarity.Create(2.6f)), Cellular = new(CellMetric.Minkowski, MinkowskiExponent.Create(0.4f), Fraction.MaxValue) },
        ShortSideExtent.Create(0.1f), ShortSideExtent.Create(0.1f), Presets.Grayscale, Stops.Zero, Fraction.MinValue, Hertz.Neutral, Presets.Source((NoiseBasis.Fbm.Default with { Fractal = new(FractalDetail.Create(6f), Fraction.Create(0.56f), FractalLacunarity.Create(2f)), Distortion = NoiseDistortion.Create(0.1f) }), ShortSideExtent.Create(1f), ShortSideExtent.Create(1f)),
        Fraction.Create(0.5f), Presets.Grayscale, Stops.Zero, GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(Starfield state, PassContext context) => Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
        from stars in state.Stars.SiteSampler
        from starTable in RampLight.Of(state.StarColors, context.Working)
        from nebulaTable in RampLight.Of(state.NebulaColors, context.Working)
        from drawn in Drawn(state, context, stars, starTable, nebulaTable).Run(frame, progress)
        select drawn));

    private static PixelPass Drawn(Starfield state, PassContext context, Func<LatticePoint, uint, NoiseSample<LatticePoint>> stars, RampLight starTable, RampLight nebulaTable) {
        (uint field, float time) = (state.Seed.Field(14u, 0u), state.Timing.At(context));
        (uint glint, uint sites, uint clouds, uint draws) = (CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        (float gain, ERational scale, double reach) = (state.StarExposure.Scale, ERational.FromSingle(state.StarScale), (double)state.StarScale / state.StarRadius);
        Func<LatticePoint, float> steady = state.Twinkle == Fraction.MinValue ? static _ => 1f
            : ((NoiseBasis.Plain with { Dimensions = NoiseDimensions.Four }).Sampler, ERational.FromSingle(state.TwinkleRate).Multiply(ERational.FromSingle(time)), (float)state.Twinkle) switch {
                var (plain, phase, depth) => site => 1f - (depth * plain(site with { W = site.W.Add(phase) }, glint).Value),
            };
        Func<(double X, double Y), Vector3> nebula = state.NebulaDensity == Fraction.MinValue ? static _ => Vector3.Zero
            : (state.Nebula.Sampler(time, clouds), state.NebulaExposure.Scale, (float)state.NebulaDensity) switch {
                var (sample, light, density) => point => sample(new LatticePoint(ERational.FromDouble(point.X), ERational.FromDouble(point.Y), ERational.Zero, ERational.Zero)) switch {
                    var n => ShapeEdge.Smoother(Presets.Map(n, (1f - density, 1f))) * nebulaTable.Sample(n) switch { var cloud => cloud.W * light * cloud.AsVector3() },
                },
            };
        return state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point => {
            NoiseSample<LatticePoint> cell = stars(new(ERational.FromDouble(point.X).Divide(scale), ERational.FromDouble(point.Y).Divide(scale), ERational.Zero, ERational.Zero), sites);
            Vector4 draw = cell.Site.White(draws);
            return new Vector4((draw.Y * (float)double.Clamp(1d - (cell.Value * reach), 0d, 1d) * steady(cell.Site) * gain * starTable.Sample(draw.X).AsVector3()) + nebula(point), 1f);
        }));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StarfieldParameter : IStateParameter<Starfield> {
    public static readonly StarfieldParameter Placement = new("placement", new StateParameter<Starfield>.Record<Placement>(Lens<Starfield, Placement>.New(static sky => sky.Placement, static placement => sky => sky with { Placement = placement })),
        new("Placement", "Field position and turn"));
    public static readonly StarfieldParameter Stars = new("stars", new StateParameter<Starfield>.Variant<NoiseBasis>(Lens<Starfield, NoiseBasis>.New(static sky => sky.Stars, static stars => sky => sky with { Stars = stars })),
        new("Stars", "Cellular noise placing bright stars"));
    public static readonly StarfieldParameter StarScale = new("star-scale", StateParameter.Bounded(Lens<Starfield, ShortSideExtent>.New(static sky => sky.StarScale, static scale => sky => sky with { StarScale = scale }), Presentations.Extent with { Soft = (0.04f, 1f) }),
        new("Star scale", "Bright star cell side in frame short sides"));
    public static readonly StarfieldParameter StarRadius = new("star-radius", StateParameter.Bounded(Lens<Starfield, ShortSideExtent>.New(static sky => sky.StarRadius, static radius => sky => sky with { StarRadius = radius }), Presentations.Extent with { Soft = (0.01f, 1f) }),
        new("Star radius", "Bright star reach in frame short sides"));
    public static readonly StarfieldParameter StarColors = new("star-colors", new StateParameter<Starfield>.Gradient(Lens<Starfield, ColorRamp>.New(static sky => sky.StarColors, static colors => sky => sky with { StarColors = colors })),
        new("Star colors", "Colors bright stars draw from"));
    public static readonly StarfieldParameter StarExposure = new("star-exposure", StateParameter.Bounded(Lens<Starfield, Stops>.New(static sky => sky.StarExposure, static exposure => sky => sky with { StarExposure = exposure })),
        new("Star exposure", "Star brightness in stops"));
    public static readonly StarfieldParameter Twinkle = new("twinkle", StateParameter.Bounded(Lens<Starfield, Fraction>.New(static sky => sky.Twinkle, static twinkle => sky => sky with { Twinkle = twinkle })),
        new("Twinkle", "Share of star light twinkle dims, 0% steady"));
    public static readonly StarfieldParameter TwinkleRate = new("twinkle-rate", StateParameter.Bounded(Lens<Starfield, Hertz>.New(static sky => sky.TwinkleRate, static rate => sky => sky with { TwinkleRate = rate }), Hertz.Slow),
        new("Twinkle rate", "Twinkle periods per second"));
    public static readonly StarfieldParameter Nebula = new("nebula", new StateParameter<Starfield>.Record<NoiseSource>(Lens<Starfield, NoiseSource>.New(static sky => sky.Nebula, static nebula => sky => sky with { Nebula = nebula })),
        new("Nebula", "Nebula pattern, feature size, and motion"));
    public static readonly StarfieldParameter NebulaDensity = new("nebula-density", StateParameter.Bounded(Lens<Starfield, Fraction>.New(static sky => sky.NebulaDensity, static density => sky => sky with { NebulaDensity = density })),
        new("Nebula density", "Share of noise range nebula covers"));
    public static readonly StarfieldParameter NebulaColors = new("nebula-colors", new StateParameter<Starfield>.Gradient(Lens<Starfield, ColorRamp>.New(static sky => sky.NebulaColors, static colors => sky => sky with { NebulaColors = colors })),
        new("Nebula colors", "Nebula color and coverage by noise value"));
    public static readonly StarfieldParameter NebulaExposure = new("nebula-exposure", StateParameter.Bounded(Lens<Starfield, Stops>.New(static sky => sky.NebulaExposure, static exposure => sky => sky with { NebulaExposure = exposure })),
        new("Nebula exposure", "Nebula brightness in stops"));
    public static readonly StarfieldParameter Layer = new("layer", new StateParameter<Starfield>.Record<GeneratedLayer>(Lens<Starfield, GeneratedLayer>.New(static sky => sky.Layer, static layer => sky => sky with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly StarfieldParameter Seed = new("seed", StateParameter.Bounded(Lens<Starfield, Seed>.New(static sky => sky.Seed, static seed => sky => sky with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly StarfieldParameter Timing = new("timing", new StateParameter<Starfield>.Record<Timing>(Lens<Starfield, Timing>.New(static sky => sky.Timing, static timing => sky => sky with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<Starfield> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ParticleField(
    PointCells Cells, PositiveCount Layers, SignedAngle Heading, Drift Speed, NoiseBasis Basis, ShortSideLength Turbulence, ShortSideExtent Eddy, Hertz Evolution,
    PrimaryScale Depth, Fraction DepthVariation, Fraction Focus, ShortSideLength Defocus, Aperture Aperture, SignedFraction Fringe, ShutterTime Shutter, Fraction Fade,
    bool Covering, ColorRamp Coating, Gated<Stops> Radiance, ColorRamp Colors, Emission Emission, Fraction Incandescence, Fraction Twinkle, Hertz TwinkleRate,
    GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<ParticleField, ParticleFieldParameter>, IPixelStage<ParticleField> {
    private static readonly GaussLegendreRule Shutters = new(0d, 1d, 32);

    public static ParticleField Default { get; } = (Unit: ReferenceFrame.GreaterSide / 2f, Pace: 24f * 0.05f * 0.5f, Scale: 1.6f) switch {
        var (unit, pace, scale) => new(
            new(ShortSideExtent.Create(unit / 10f), Fraction.MaxValue, Fraction.Create(2f * 0.2f), Fraction.Create(0.5f), Magnitude.Unit, Fraction.Create(1f / 1.8f)), PositiveCount.Create(4),
            SignedAngle.Create(float.Atan2(0.5f, 0.4f)), Drift.Create(pace * float.Hypot(0.4f, 0.5f) * unit), NoiseBasis.Default,
            ShortSideLength.Create(0.12f / 2f * unit), ShortSideExtent.Create(unit / scale), Hertz.Create(scale * pace / 3200f),
            PrimaryScale.Create(4f), Fraction.MaxValue, Fraction.MaxValue, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide), Aperture.Default, SignedFraction.Neutral,
            ShutterTime.Create(0.5f / 24f), Fraction.MinValue, Covering: false, Presets.White, new(Enabled: true, Stops.Zero), Presets.White, Emission.Default,
            Fraction.MinValue, Fraction.MinValue, Hertz.Neutral, GeneratedLayer.Added with { Exposure = Stops.Create(2f) }, Seed.MinValue, Timing.Default),
    };

    public static ParticleField Rain { get; } = Default with {
        Cells = PointCells.Default,
        Heading = SignedAngle.Down,
        Speed = Drift.Create(2.5f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Neutral,
        Aperture = Aperture.Default with { Roundness = Fraction.MaxValue },
        Layer = GeneratedLayer.Default,
        Covering = true,
        Radiance = new(Enabled: false, Stops.Zero),
    };

    public static ParticleField Snow { get; } = Rain with {
        Cells = new(ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide), Fraction.Create(0.5f), Fraction.Create(2f * 0.3f), Fraction.Create(0.6f), Magnitude.Create(1.3f), Fraction.Create(0.6f)),
        Speed = Drift.Create(0.3f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Create(0.02f / 2f * ReferenceFrame.GreaterSide),
        Colors = Presets.Solid(new Vector3(0.8f)),
        Coating = Presets.Solid(new Vector3(0.8f)),
    };

    public static ParticleField Sparks { get; } = Default with {
        Cells = Default.Cells with { Density = Fraction.Create(0.3f), Size = Fraction.Create(2f * 0.1f), Variation = Fraction.Create(0.8f), Spread = Magnitude.Create(2.3f), Softness = Fraction.Create(0.3f) },
        Heading = SignedAngle.Up,
        Speed = Drift.Create(0.4f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Create(0.03f / 2f * ReferenceFrame.GreaterSide),
        Emission = Emission.Default with { Cool = new(1000d) },
        Incandescence = Fraction.MaxValue,
    };

    public static ParticleField CellRain { get; } = Default with {
        Covering = true,
        Radiance = new(Enabled: false, Stops.Zero),
        Cells = Default.Cells with { Variation = Fraction.MinValue, Spread = Magnitude.Zero, Softness = Fraction.MinValue },
        Heading = SignedAngle.Down,
        Defocus = ShortSideLength.Neutral,
        Aperture = Aperture.Default with { Blades = ApertureBlades.Create(4), Rotation = SignedAngle.Diagonal },
    };

    public static ParticleField Starfield { get; } = Rain with {
        Cells = new(ShortSideExtent.Create(0.005f * ReferenceFrame.GreaterSide), Fraction.Create(0.12f), Fraction.Create(2f * 0.1f), Fraction.Create(0.5f), Magnitude.Create(4f), Fraction.MaxValue),
        Layers = PositiveCount.MinValue,
        Depth = PrimaryScale.MinValue,
        Speed = Drift.Neutral,
        Defocus = ShortSideLength.Neutral,
        Emission = new(new(3000d), new(12_000d)),
        Incandescence = Fraction.MaxValue,
        Layer = GeneratedLayer.Added,
        Covering = false,
        Radiance = new(Enabled: true, Stops.Zero),
    };

    public static Seq<(string Name, EffectLook<ParticleField> Look)> Looks { get; } = [
        Presets.Look(nameof(Rain), Rain), Presets.Look(nameof(Snow), Snow), Presets.Look(nameof(Sparks), Sparks),
        Presets.Look(nameof(CellRain), CellRain), Presets.Look(nameof(Starfield), Starfield)];

    public static Option<PixelPass> Pass(ParticleField state, PassContext context) =>
        state.Cells.Empty || state.Aperture.Obstruction == Fraction.MaxValue || (!state.Covering && !state.Radiance.Enabled) ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from iris in state.Aperture.Pupil
                from colors in RampLight.Of(state.Colors, context.Working)
                from coating in RampLight.Of(state.Coating, context.Working)
                from bounded in state.Turbulence == ShortSideLength.Neutral ? (Sample: state.Basis.Sampler, Range: (Lower: 0f, Upper: 0f)) : state.Basis.BoundedSampler
                from drawn in Drawn(state, context, frame, iris, colors, coating, bounded).Run(frame, progress)
                select drawn));

    private static PixelPass Drawn(
        ParticleField state, PassContext context, PixelFrame frame, Iris iris, RampLight colors, RampLight coating,
        (Func<LatticePoint, uint, NoiseSample<LatticePoint>> Sample, (float Lower, float Upper) Range) bounded) {
        (PointCells cells, int side, ERational time, ERational shutter) = (state.Cells, context.Extent.ShortSide, ERational.FromSingle(state.Timing.At(context)), ERational.FromSingle(state.Shutter));
        (ERational X, ERational Y) middle = (ERational.FromInt32(context.Extent.Width).Divide(2), ERational.FromInt32(context.Extent.Height).Divide(2));
        Func<LatticePoint, uint, NoiseSample<LatticePoint>> plain = (NoiseBasis.Plain with { Dimensions = NoiseDimensions.Four }).Sampler;
        uint field = state.Seed.Field(15u, 0u);
        (uint presence, uint tone, uint across, uint down, uint twinkle) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        Func<float, Vector3> thermal = Emission.Span((context.Working, state.Emission));
        (double focus, double radiance) = (double.Pow(state.Depth, -(double)state.Focus), state.Radiance.Active.Match(Some: static exposure => (double)exposure.Scale, None: static () => 0d));
        (ERational X, ERational Y) heading = double.SinCos(state.Heading) switch {
            var (sin, cos) => (ERational.FromSingle(state.Speed).Multiply(ERational.FromDouble(cos)), ERational.FromSingle(state.Speed).Multiply(ERational.FromDouble(-sin))),
        };
        (ERational lowWind, ERational highWind) = (ERational.FromSingle(bounded.Range.Lower).Multiply(2).Subtract(1), ERational.FromSingle(bounded.Range.Upper).Multiply(2).Subtract(1));
        List<Mote> motes = [];
        for (int layer = 0; layer < state.Layers; layer++) {
            ERational middleDepth = ERational.FromInt32(layer).Add(ERational.One.Divide(2)).Divide((int)state.Layers);
            ERational depthWidth = ERational.FromSingle(state.DepthVariation).Divide(2).Divide((int)state.Layers);
            (double reference, double nearest, double farthest) =
                (double.Pow(state.Depth, -middleDepth.ToDouble()), double.Pow(state.Depth, -middleDepth.Subtract(depthWidth).ToDouble()), double.Pow(state.Depth, -middleDepth.Add(depthWidth).ToDouble()));
            ERational cell = ERational.FromSingle(cells.Spacing).Multiply(ERational.FromDouble(reference));
            ERational support = ERational.FromDouble(Grown(side * (((double)cells.Spacing * nearest * cells.Draw(Vector4.Zero).Radius)
                + (state.Defocus * double.Max(double.Abs(nearest - focus), double.Abs(farthest - focus))))) / side);
            ERational[] wind = [lowWind.Multiply(ERational.FromDouble(nearest)), lowWind.Multiply(ERational.FromDouble(farthest)), highWind.Multiply(ERational.FromDouble(nearest)), highWind.Multiply(ERational.FromDouble(farthest))];
            (ERational windLow, ERational windHigh) = (wind.Aggregate(ERational.Min).Multiply(ERational.FromSingle(state.Turbulence)), wind.Aggregate(ERational.Max).Multiply(ERational.FromSingle(state.Turbulence)));
            ((ERational Low, ERational High) horizontal, (ERational Low, ERational High) vertical) = (Travel(heading.X, nearest, farthest), Travel(heading.Y, nearest, farthest));
            (EInteger left, EInteger top, EInteger right, EInteger bottom) =
                (Lattice(frame.Window.Left, middle.X, horizontal.High.Add(windHigh).Add(support), cell).Subtract(1),
                    Lattice(frame.Window.Top, middle.Y, vertical.High.Add(windHigh).Add(support), cell).Subtract(1),
                    Lattice(frame.Window.Right, middle.X, horizontal.Low.Add(windLow).Subtract(support), cell),
                    Lattice(frame.Window.Bottom, middle.Y, vertical.Low.Add(windLow).Subtract(support), cell));
            EInteger width = right.Subtract(left).Add(1);
            EInteger count = width.Multiply(bottom.Subtract(top).Add(1));
            for (EInteger index = EInteger.Zero; index.CompareTo(count) < 0; index = index.Add(1)) {
                (EInteger column, EInteger line) = (left.Add(index.Remainder(width)), top.Add(index.Divide(width)));
                LatticePoint point = new(ERational.FromEInteger(column), ERational.FromEInteger(line), ERational.FromInt32(layer), ERational.Zero);
                Vector4 position = point.White(presence);
                if (position.X >= cells.Density)
                    continue;
                (Vector4 draw, ERational depth) = (point.White(tone),
                    middleDepth.Add(ERational.FromSingle(position.W).Subtract(ERational.One.Divide(2)).Multiply(ERational.FromSingle(state.DepthVariation)).Divide((int)state.Layers)));
                (double scale, (double radius, double gain)) = (double.Pow(state.Depth, -depth.ToDouble()), cells.Draw(draw));
                double core = side * (double)cells.Spacing * scale * radius;
                double optical = core + (side * (double)state.Defocus * double.Abs(scale - focus));
                Vector4 spectrum = colors.Sample(draw.X);
                Vector3 tint = Vector3.Lerp(Vector3.One, thermal(draw.X), state.Incandescence);
                Vector256<double> area = iris.Integral(optical, (double)cells.Softness * optical, state.Fringe);
                double energy = radiance * spectrum.W * gain * double.Pi * core * core;
                Vector256<double> light = Vector256.Create(area[0] is 0d ? 0d : spectrum.X * (double)tint.X * energy / area[0],
                    area[1] is 0d ? 0d : spectrum.Y * (double)tint.Y * energy / area[1], area[2] is 0d ? 0d : spectrum.Z * (double)tint.Z * energy / area[2], 0d);
                motes.Add(new(depth, layer, column, line, point,
                    (ERational.FromEInteger(column).Add(ERational.FromSingle(position.Y)).Multiply(cell), ERational.FromEInteger(line).Add(ERational.FromSingle(position.Z)).Multiply(cell)),
                    scale, optical, gain, state.Covering ? coating.Sample(draw.X) : Vector4.Zero, light));
            }
        }
        Mote[] ordered = [.. motes.OrderByDescending(static mote => mote.Depth).ThenBy(static mote => mote.Layer).ThenBy(static mote => mote.Line).ThenBy(static mote => mote.Column)];
        return state.Layer.Pass((row, column, line) => {
            long y = context.Extent.Height - 1L - line;
            for (int x = 0; x < row.Length; x++) {
                Vector256<double> integrated = Vector256<double>.Zero;
                for (int sample = 0; sample < Shutters.Order; sample++) {
                    double u = Shutters.GetAbscissa(sample);
                    ERational tau = time.Subtract(shutter.Multiply(ERational.FromDouble(1d - u)));
                    double weight = Shutters.GetWeight(sample) * (1d - (double)state.Fade + ((double)state.Fade * u)) / (1d - ((double)state.Fade / 2d));
                    integrated += Spatial(column + x, y, tau) * weight;
                }
                row[x] = new((float)integrated[0], (float)integrated[1], (float)integrated[2], (float)integrated[3]);
            }
        });

        EInteger Lattice(long pixel, ERational center, ERational displacement, ERational cell) =>
            LatticePoint.Split(ERational.FromInt64(pixel).Subtract(center).Divide(side).Subtract(displacement).Divide(cell)).Cell;

        (ERational Low, ERational High) Travel(ERational rate, double near, double far) {
            ERational[] endpoints = [
                rate.Multiply(ERational.FromDouble(near)).Multiply(time), rate.Multiply(ERational.FromDouble(far)).Multiply(time),
                rate.Multiply(ERational.FromDouble(near)).Multiply(time.Subtract(shutter)), rate.Multiply(ERational.FromDouble(far)).Multiply(time.Subtract(shutter))];
            return (endpoints.Aggregate(ERational.Min), endpoints.Aggregate(ERational.Max));
        }

        double Grown(double radius) => (iris.Circumradius * radius) + ((double)cells.Softness * radius / 2d);

        (ERational X, ERational Y) Moved(Mote mote, ERational tau) {
            ERational scale = ERational.FromDouble(mote.Scale);
            (ERational X, ERational Y) = (mote.Rest.X.Add(heading.X.Multiply(scale).Multiply(tau)), mote.Rest.Y.Add(heading.Y.Multiply(scale).Multiply(tau)));
            if (state.Turbulence == ShortSideLength.Neutral)
                return (middle.X.Add(X.Multiply(side)), middle.Y.Add(Y.Multiply(side)));
            ERational size = scale.Multiply(ERational.FromSingle(state.Eddy));
            LatticePoint sample = new(X.Divide(size), Y.Divide(size), ERational.FromSingle(state.Evolution).Multiply(tau), ERational.Zero);
            ERational gain = ERational.FromSingle(state.Turbulence).Multiply(scale);
            return (middle.X.Add(X.Add(gain.Multiply(ERational.FromSingle(bounded.Sample(sample, across).Value).Multiply(2).Subtract(1))).Multiply(side)),
                middle.Y.Add(Y.Add(gain.Multiply(ERational.FromSingle(bounded.Sample(sample, down).Value).Multiply(2).Subtract(1))).Multiply(side)));
        }

        Vector256<double> Spatial(long column, long line, ERational tau) {
            (ERational left, ERational top, ERational right, ERational bottom) = (ERational.FromInt64(column), ERational.FromInt64(line), ERational.FromInt64(column + 1), ERational.FromInt64(line + 1));
            (Mote Mote, (ERational X, ERational Y) Point, ERational LowX, ERational HighX, ERational LowY, ERational HighY, double Modulation)[] active = [..
                from mote in ordered
                let point = Moved(mote, tau)
                let reach = ERational.FromDouble(Grown(mote.Radius))
                let lowX = ERational.Max(left, point.X.Subtract(reach))
                let highX = ERational.Min(right, point.X.Add(reach))
                let lowY = ERational.Max(top, point.Y.Subtract(reach))
                let highY = ERational.Min(bottom, point.Y.Add(reach))
                where lowX.CompareTo(highX) < 0 && lowY.CompareTo(highY) < 0
                let modulation = 1d - ((double)state.Twinkle * plain(mote.Point with { W = ERational.FromSingle(state.TwinkleRate).Multiply(tau) }, twinkle).Value)
                select (Mote: mote, Point: point, LowX: lowX, HighX: highX, LowY: lowY, HighY: highY, Modulation: modulation)];
            ERational[] xs = [.. new SortedSet<ERational>(active.SelectMany(static item => new[] { item.LowX, item.HighX }).Append(left).Append(right))];
            ERational[] ys = [.. new SortedSet<ERational>(active.SelectMany(static item => new[] { item.LowY, item.HighY }).Append(top).Append(bottom))];
            Vector256<double> total = Vector256<double>.Zero;
            foreach ((int x, int y) in from y in Enumerable.Range(0, ys.Length - 1) from x in Enumerable.Range(0, xs.Length - 1) select (x, y)) {
                (ERational width, ERational height) = (xs[x + 1].Subtract(xs[x]), ys[y + 1].Subtract(ys[y]));
                double area = width.Multiply(height).ToDouble();
                for (int sample = 0; sample < Shutters.Order * Shutters.Order; sample++) {
                    (int acrossNode, int downNode) = (sample % Shutters.Order, sample / Shutters.Order);
                    (ERational X, ERational Y) = (xs[x].Add(width.Multiply(ERational.FromDouble(Shutters.GetAbscissa(acrossNode)))), ys[y].Add(height.Multiply(ERational.FromDouble(Shutters.GetAbscissa(downNode)))));
                    Vector256<double> folded = Vector256<double>.Zero;
                    foreach ((Mote Mote, (ERational X, ERational Y) Point, ERational LowX, ERational HighX, ERational LowY, ERational HighY, double Modulation) item in active) {
                        (double reach, double edge) = iris.Outline((X.Subtract(item.Point.X).ToDouble(), Y.Subtract(item.Point.Y).ToDouble()));
                        Vector256<double> profile = iris.Weights(reach, edge, item.Mote.Radius, (double)cells.Softness * item.Mote.Radius, state.Fringe);
                        double opacity = profile[3] * item.Mote.Gain * item.Mote.Coating.W;
                        Vector256<double> coat = Vector256.Create(item.Mote.Coating.X, item.Mote.Coating.Y, item.Mote.Coating.Z, 1d);
                        folded = ((1d - opacity) * folded) + (opacity * coat) + (profile * item.Mote.Light * item.Modulation);
                    }
                    total += folded * (area * Shutters.GetWeight(acrossNode) * Shutters.GetWeight(downNode));
                }
            }
            return total;
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParticleFieldParameter : IStateParameter<ParticleField> {
    public static readonly ParticleFieldParameter Cells = new("cells", new StateParameter<ParticleField>.Record<PointCells>(Lens<ParticleField, PointCells>.New(static particles => particles.Cells, static cells => particles => particles with { Cells = cells })),
        new("Population", "Mote spacing, presence, size, and light"));
    public static readonly ParticleFieldParameter Layers = new("layers", StateParameter.Bounded(Lens<ParticleField, PositiveCount>.New(static particles => particles.Layers, static layers => particles => particles with { Layers = layers }), PositiveCount.Presentation with { Step = 1, Soft = (1, 8) }),
        new("Layers", "Mote depth layers"));
    public static readonly ParticleFieldParameter Heading = new("heading", StateParameter.Bounded(Lens<ParticleField, SignedAngle>.New(static particles => particles.Heading, static heading => particles => particles with { Heading = heading }), Presentations.Angle),
        new("Heading", "Travel direction, counterclockwise from right"));
    public static readonly ParticleFieldParameter Speed = new("speed", StateParameter.Bounded(Lens<ParticleField, Drift>.New(static particles => particles.Speed, static speed => particles => particles with { Speed = speed }), Drift.Presentation with { Soft = (0f, 2f) }),
        new("Speed", "Frame short sides per second the nearest layer travels"));
    public static readonly ParticleFieldParameter Basis = new("basis", new StateParameter<ParticleField>.Variant<NoiseBasis>(Lens<ParticleField, NoiseBasis>.New(static particles => particles.Basis, static basis => particles => particles with { Basis = basis })),
        new("Wind", "Noise displacing particles"));
    public static readonly ParticleFieldParameter Turbulence = new("turbulence", StateParameter.Bounded(Lens<ParticleField, ShortSideLength>.New(static particles => particles.Turbulence, static turbulence => particles => particles with { Turbulence = turbulence }), Presentations.Length with { Soft = (0f, 0.2f) }),
        new("Turbulence", "Wind sway of each particle in frame short sides"));
    public static readonly ParticleFieldParameter Eddy = new("eddy", StateParameter.Bounded(Lens<ParticleField, ShortSideExtent>.New(static particles => particles.Eddy, static eddy => particles => particles with { Eddy = eddy }), Presentations.Extent with { Soft = (0.01f, 2f) }),
        new("Eddy size", "Wind feature width in frame short sides"));
    public static readonly ParticleFieldParameter Evolution = new("evolution", StateParameter.Bounded(Lens<ParticleField, Hertz>.New(static particles => particles.Evolution, static evolution => particles => particles with { Evolution = evolution }), Hertz.Slow),
        new("Evolution", "Noise units per second the wind changes through"));
    public static readonly ParticleFieldParameter Depth = new("depth", StateParameter.Bounded(Lens<ParticleField, PrimaryScale>.New(static particles => particles.Depth, static depth => particles => particles with { Depth = depth }), new Presentation<float> { Soft = (1f, 4f) }),
        new("Depth", "Farthest layer distance over nearest"));
    public static readonly ParticleFieldParameter DepthVariation = new("depth-variation", StateParameter.Bounded(Lens<ParticleField, Fraction>.New(static particles => particles.DepthVariation, static variation => particles => particles with { DepthVariation = variation })),
        new("Depth variation", "Depth variation within each layer"));
    public static readonly ParticleFieldParameter Focus = new("focus", StateParameter.Bounded(Lens<ParticleField, Fraction>.New(static particles => particles.Focus, static focus => particles => particles with { Focus = focus })),
        new("Focus", "Depth in focus, 0% nearest layer, 100% farthest"));
    public static readonly ParticleFieldParameter Defocus = new("defocus", StateParameter.Bounded(Lens<ParticleField, ShortSideLength>.New(static particles => particles.Defocus, static defocus => particles => particles with { Defocus = defocus }), Presentations.Length with { Soft = (0f, 0.05f) }),
        new("Defocus", "Largest bokeh radius in frame short sides"));
    public static readonly ParticleFieldParameter Aperture = new("iris", new StateParameter<ParticleField>.Record<Aperture>(Lens<ParticleField, Aperture>.New(static particles => particles.Aperture, static aperture => particles => particles with { Aperture = aperture })),
        new("Aperture", "Lens opening shaping bokeh and diffraction spikes"));
    public static readonly ParticleFieldParameter Fringe = new("fringe", StateParameter.Bounded(Lens<ParticleField, SignedFraction>.New(static particles => particles.Fringe, static fringe => particles => particles with { Fringe = fringe })),
        new("Color shift", "Red and blue bokeh sizes apart from green"));
    public static readonly ParticleFieldParameter Shutter = new("shutter", StateParameter.Bounded(Lens<ParticleField, ShutterTime>.New(static particles => particles.Shutter, static shutter => particles => particles with { Shutter = shutter }), ShutterTime.Presentation with { Soft = (0f, 1f / 24f) }),
        new("Shutter", "Seconds shutter stays open, particles streaking along travel"));
    public static readonly ParticleFieldParameter Fade = new("fade", StateParameter.Bounded(Lens<ParticleField, Fraction>.New(static particles => particles.Fade, static fade => particles => particles with { Fade = fade })),
        new("Fade", "Streak dimming toward tail"));
    public static readonly ParticleFieldParameter Colors = new("colors", new StateParameter<ParticleField>.Gradient(Lens<ParticleField, ColorRamp>.New(static particles => particles.Colors, static colors => particles => particles with { Colors = colors })),
        new("Colors", "Colors each particle draws from"));
    public static readonly ParticleFieldParameter Covering = new("covering", new StateParameter<ParticleField>.Toggle(Lens<ParticleField, bool>.New(static particles => particles.Covering, static covering => particles => particles with { Covering = covering })),
        new("Covering", "Particles cover light behind them"));
    public static readonly ParticleFieldParameter Coating = new("coating", new StateParameter<ParticleField>.Gradient(Lens<ParticleField, ColorRamp>.New(static particles => particles.Coating, static coating => particles => particles with { Coating = coating })),
        new("Coating", "Covering colors and opacity each particle draws from"));
    public static readonly ParticleFieldParameter Radiance = new("radiance", StateParameter.OptionalBounded(Lens<ParticleField, Gated<Stops>>.New(static particles => particles.Radiance, static radiance => particles => particles with { Radiance = radiance })),
        new("Radiance", "Emitted particle brightness in stops, independent of covering"));
    public static readonly ParticleFieldParameter Emission = new("emission", new StateParameter<ParticleField>.Record<Emission>(Lens<ParticleField, Emission>.New(static particles => particles.Emission, static emission => particles => particles with { Emission = emission })),
        new("Blackbody", "Blackbody light between cool and hot temperatures"));
    public static readonly ParticleFieldParameter Incandescence = new("incandescence", StateParameter.Bounded(Lens<ParticleField, Fraction>.New(static particles => particles.Incandescence, static incandescence => particles => particles with { Incandescence = incandescence })),
        new("Incandescence", "Share of particle color taken from blackbody light"));
    public static readonly ParticleFieldParameter Twinkle = new("twinkle", StateParameter.Bounded(Lens<ParticleField, Fraction>.New(static particles => particles.Twinkle, static twinkle => particles => particles with { Twinkle = twinkle })),
        new("Twinkle", "Share of particle light varying over time"));
    public static readonly ParticleFieldParameter TwinkleRate = new("twinkle-rate", StateParameter.Bounded(Lens<ParticleField, Hertz>.New(static particles => particles.TwinkleRate, static rate => particles => particles with { TwinkleRate = rate }), Hertz.Slow),
        new("Twinkle rate", "Twinkle periods per second"));
    public static readonly ParticleFieldParameter Layer = new("layer", new StateParameter<ParticleField>.Record<GeneratedLayer>(Lens<ParticleField, GeneratedLayer>.New(static particles => particles.Layer, static layer => particles => particles with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly ParticleFieldParameter Seed = new("seed", StateParameter.Bounded(Lens<ParticleField, Seed>.New(static particles => particles.Seed, static seed => particles => particles with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly ParticleFieldParameter Timing = new("timing", new StateParameter<ParticleField>.Record<Timing>(Lens<ParticleField, Timing>.New(static particles => particles.Timing, static timing => particles => particles with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<ParticleField> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeFit {
    public static readonly ShapeFit ShortSide = new("short-side", static _ => (1d, 1d));
    public static readonly ShapeFit LongSide = new("long-side", static extent => ((double)int.Max(extent.Width, extent.Height) / extent.ShortSide) switch { var scale => (scale, scale) });

    [UseDelegateFromConstructor]
    public partial (double X, double Y) Scale(PixelExtent extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeKind {
    public static readonly ShapeKind Ellipse = new("ellipse", static (shape, half, math) => DistanceField.Ellipse(half, shape.Exponent, math));
    public static readonly ShapeKind Box = new("box", static (shape, half, math) => DistanceField.Box(half, shape.Roundness, math));
    public static readonly ShapeKind Polygon = new("polygon", static (shape, half, math) => DistanceField.Polygon(half, shape.Sides, shape.Inset, shape.Roundness, math));
    public static readonly ShapeKind Ring = new("ring", static (shape, half, math) => DistanceField.Ring(half, shape.Exponent, shape.Inset, math));
    public static readonly ShapeKind Cross = new("cross", static (shape, half, math) => DistanceField.Cross(half, shape.Inset, shape.Roundness, math));
    public static readonly ShapeKind Heart = new("heart", static (_, half, math) => DistanceField.Heart(half, math));
    public static readonly ShapeKind Triangle = new("triangle", static (shape, half, math) => DistanceField.Triangle(half, shape.Apex, shape.Roundness, math));

    [UseDelegateFromConstructor]
    public partial DistanceField Field(ShapePrimitive shape, (EFloat X, EFloat Y) half, EContext math);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeOperation {
    public static readonly ShapeOperation Union = new("union", static (first, second, smoothing, _, math) => first.Union(second, smoothing, math));
    public static readonly ShapeOperation Difference = new("difference", static (first, second, smoothing, _, math) => first.Difference(second, smoothing, math));
    public static readonly ShapeOperation Intersection = new("intersection", static (first, second, smoothing, _, math) => first.Intersection(second, smoothing, math));
    public static readonly ShapeOperation Morph = new("morph", static (first, second, _, progress, _) => first.Morph(second, progress));

    [UseDelegateFromConstructor]
    public partial FieldSample Join(FieldSample first, FieldSample second, EFloat smoothing, float progress, EContext math);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientKind {
    public static readonly GradientKind Linear = new("linear", periodic: false, static extent => point => (0.5d + (point.X / (2d * extent.X)), 1d / (2d * extent.X)));
    public static readonly GradientKind Radial = new("radial", periodic: false, static extent => point => double.Hypot(point.X / extent.X, point.Y / extent.Y) switch {
        var level => (level, level > 0d ? double.Hypot(point.X / (extent.X * extent.X * level), point.Y / (extent.Y * extent.Y * level)) : 1d / extent.Y),
    });
    public static readonly GradientKind Angular = new("angular", periodic: true, static _ => static point => ((double.Atan2(point.Y, point.X) / double.Tau) + 0.5d, 1d / (double.Tau * double.Hypot(point.X, point.Y))));
    public static readonly GradientKind Box = new("box", periodic: false, static extent => point => (X: double.Abs(point.X) / extent.X, Y: double.Abs(point.Y) / extent.Y) switch {
        var reach => reach.X >= reach.Y ? (reach.X, 1d / extent.X) : (reach.Y, 1d / extent.Y),
    });

    public bool Periodic { get; }

    [UseDelegateFromConstructor]
    public partial Func<(double X, double Y), (double Position, double Slope)> Parameter((double X, double Y) extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridPattern {
    public static readonly GridPattern Lines = new("lines");
    public static readonly GridPattern Checker = new("checker");
    public static readonly GridPattern Brick = new("brick");
}

public sealed record ShapePrimitive(
    ShapeKind Kind, Placement Placement, ShortSideLength Width, ShortSideLength Height, Fraction Roundness, SuperellipseExponent Exponent, SideCount Sides, Fraction Inset, SignedFraction Apex)
    : IStateRecord<ShapePrimitive, ShapePrimitiveParameter> {
    public static ShapePrimitive Default { get; } =
        new(ShapeKind.Ellipse, Placement.Default, ShortSideLength.Create(0.5f), ShortSideLength.Create(0.5f), Fraction.MinValue, SuperellipseExponent.Ellipse, SideCount.Five, Fraction.MinValue, SignedFraction.Neutral);

    public ShapeFit Fit { get; init; } = ShapeFit.ShortSide;

    public (FieldFrame Frame, Func<long, long, FieldSample> Sample) Field(PixelExtent extent, EContext math) =>
        (Placement.Frame(extent), Fit.Scale(extent)) switch {
            var (frame, _) when Width == ShortSideLength.Neutral || Height == ShortSideLength.Neutral => (frame, static (_, _) => FieldSample.Empty),
            var (frame, scale) => Kind.Field(this, (EFloat.FromSingle(Width) * scale.X, EFloat.FromSingle(Height) * scale.Y), math) switch {
                var field => (frame, (column, line) => field.Sample(frame.Upward(column, line))),
            },
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapePrimitiveParameter : IStateParameter<ShapePrimitive> {
    private static readonly Presentation<float> Extent = Presentations.Length with { Soft = (0.01f, 2f) };

    public static readonly ShapePrimitiveParameter Kind = new("kind", new StateParameter<ShapePrimitive>.Choice<ShapeKind>(Lens<ShapePrimitive, ShapeKind>.New(static shape => shape.Kind, static kind => shape => shape with { Kind = kind })),
        new("Shape", "Figure the distance field draws"));
    public static readonly ShapePrimitiveParameter Placement = new("placement", new StateParameter<ShapePrimitive>.Record<Placement>(Lens<ShapePrimitive, Placement>.New(static shape => shape.Placement, static placement => shape => shape with { Placement = placement })),
        new("Placement", "Figure position and turn"));
    public static readonly ShapePrimitiveParameter Width = new("width", StateParameter.Bounded(Lens<ShapePrimitive, ShortSideLength>.New(static shape => shape.Width, static width => shape => shape with { Width = width }), Extent),
        new("Width", "Half width in frame sides Fit selects"));
    public static readonly ShapePrimitiveParameter Height = new("height", StateParameter.Bounded(Lens<ShapePrimitive, ShortSideLength>.New(static shape => shape.Height, static height => shape => shape with { Height = height }), Extent),
        new("Height", "Half height in frame sides Fit selects"));
    public static readonly ShapePrimitiveParameter Roundness = new("roundness", StateParameter.Bounded(Lens<ShapePrimitive, Fraction>.New(static shape => shape.Roundness, static roundness => shape => shape with { Roundness = roundness })),
        new("Roundness", "Corner radius as share of shorter half extent"));
    public static readonly ShapePrimitiveParameter Exponent = new("exponent", StateParameter.Bounded(Lens<ShapePrimitive, SuperellipseExponent>.New(static shape => shape.Exponent, static exponent => shape => shape with { Exponent = exponent })),
        new("Exponent", "Superellipse exponent, 1 diamond, 2 ellipse, above 2 squircle"));
    public static readonly ShapePrimitiveParameter Sides = new("sides", StateParameter.Bounded(Lens<ShapePrimitive, SideCount>.New(static shape => shape.Sides, static sides => shape => shape with { Sides = sides })),
        new("Sides", "Polygon side count"));
    public static readonly ShapePrimitiveParameter Inset = new("inset", StateParameter.Bounded(Lens<ShapePrimitive, Fraction>.New(static shape => shape.Inset, static inset => shape => shape with { Inset = inset })),
        new("Inset", "Polygon star inset, ring band, or cross arm width"));
    public static readonly ShapePrimitiveParameter Apex = new("apex", StateParameter.Bounded(Lens<ShapePrimitive, SignedFraction>.New(static shape => shape.Apex, static apex => shape => shape with { Apex = apex }),
        SignedFraction.Presentation with { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }),
        new("Apex", "Triangle top vertex position between left and right edges"));
    public static readonly ShapePrimitiveParameter Fit = new("fit", new StateParameter<ShapePrimitive>.Choice<ShapeFit>(Lens<ShapePrimitive, ShapeFit>.New(static shape => shape.Fit, static fit => shape => shape with { Fit = fit })),
        new("Fit", "Frame side figure dimensions follow"));

    StateParameter<ShapePrimitive> IStateParameter<ShapePrimitive>.Kind => Parameter;

    private StateParameter<ShapePrimitive> Parameter { get; }
    public ParameterText Text { get; }
}

public sealed record ShapeStyle(Gated<SKColorF> Fill, SKColorF Outline, ShortSideLength OutlineWidth, ShortSideLength OutlineOffset, ShortSideLength Feather, ShortSideLength Roughness, bool Inverted)
    : IStateRecord<ShapeStyle, ShapeStyleParameter> {
    private static readonly NoiseBasis Gate = NoiseBasis.RidgedMultifractal.Default with {
        Dimensions = NoiseDimensions.Four,
        Fractal = new(FractalDetail.MaxValue, Fraction.Create(0.9f), FractalLacunarity.Standard),
        Distortion = NoiseDistortion.Create(1.9f),
        Gain = NoiseGain.Create(74.5f),
    };

    public static ShapeStyle Default { get; } = new(new(Enabled: true, new SKColorF(1f, 1f, 1f)), new SKColorF(0f, 0f, 0f),
        ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, Inverted: false);

    public Func<FieldSample, (double X, double Y), Vector4> Shading(PassContext context, uint field, EContext math) {
        Gamut working = context.Working;
        Func<SKColorF, Vector3> light = context.Formed ? static color => new(color.Red, color.Green, color.Blue) : color => RampLight.Light(color, working).AsVector3();
        (Vector4 fill, Vector3 outline, EFloat pixels, EFloat band) =
            (Fill.Active.Map(color => new Vector4(light(color), 1f)).IfNone(Vector4.Zero), light(Outline), EFloat.FromInt32(context.Extent.ShortSide), EFloat.FromDouble(ShapeEdge.Band(Feather, context.Extent)));
        (EFloat offset, EFloat width, EFloat reach) = (EFloat.FromSingle(OutlineOffset) * pixels, EFloat.FromSingle(OutlineWidth) * pixels, EFloat.FromSingle(Roughness) * pixels);
        (EFloat near, EFloat far) = (-reach - (band * 0.5d), offset + width + reach + (band * 0.5d));
        Func<LatticePoint, uint, NoiseSample<LatticePoint>> gate = Gate.Sampler;
        Func<EFloat, (double X, double Y), EFloat> rough = reach.IsZero ? static (distance, _) => distance
            : (distance, local) => distance.CompareTo(near) < 0 || distance.CompareTo(far) > 0 ? distance
                : distance + (reach * ((2f * float.Clamp(gate(new(ERational.FromDouble(2.5d * local.X), ERational.FromDouble(2.5d * local.Y), ERational.Zero, ERational.Zero), field).Value, 0f, 1f)) - 1f));
        bool inverted = Inverted;
        return (sample, local) => (inverted ? sample.Inverted : sample) switch {
            var (distance, narrowest) => new FieldSample(rough(distance * pixels, local), narrowest * pixels) switch {
                var edge => ((fill.W * edge.Coverage(EFloat.Zero, band, math)).ToDouble(), (edge.Coverage(offset + width, band, math) - edge.Coverage(offset, band, math)).ToDouble()) switch {
                    var (covered, ring) when covered + ring > 0d => new Vector4((float)(((covered * fill.X) + (ring * outline.X)) / (covered + ring)),
                        (float)(((covered * fill.Y) + (ring * outline.Y)) / (covered + ring)), (float)(((covered * fill.Z) + (ring * outline.Z)) / (covered + ring)), (float)(covered + ring)),
                    _ => Vector4.Zero,
                },
            },
        };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeStyleParameter : IStateParameter<ShapeStyle> {
    private static readonly Presentation<float> Ring = Presentations.Length with { Soft = (0f, 0.1f) };

    public static readonly ShapeStyleParameter Fill = new("fill", new StateParameter<ShapeStyle>.OptionalColor(Lens<ShapeStyle, Gated<SKColorF>>.New(static style => style.Fill, static value => style => style with { Fill = value })),
        new("Fill", "Color inside boundary, off draws outline alone"));
    public static readonly ShapeStyleParameter Outline = new("outline", new StateParameter<ShapeStyle>.Color(Lens<ShapeStyle, SKColorF>.New(static style => style.Outline, static value => style => style with { Outline = value })),
        new("Outline", "Color of band around boundary"));
    public static readonly ShapeStyleParameter OutlineWidth = new("outline-width", StateParameter.Bounded(Lens<ShapeStyle, ShortSideLength>.New(static style => style.OutlineWidth, static value => style => style with { OutlineWidth = value }), Ring),
        new("Outline width", "Outline band width in frame short sides"));
    public static readonly ShapeStyleParameter OutlineOffset = new("outline-offset", StateParameter.Bounded(Lens<ShapeStyle, ShortSideLength>.New(static style => style.OutlineOffset, static value => style => style with { OutlineOffset = value }), Ring),
        new("Outline offset", "Outline distance from boundary in frame short sides"));
    public static readonly ShapeStyleParameter Feather = new("feather", StateParameter.Bounded(Lens<ShapeStyle, ShortSideLength>.New(static style => style.Feather, static value => style => style with { Feather = value }), Presentations.Length with { Soft = (0f, 0.25f) }),
        new("Feather", "Edge softness in frame short sides, 0% one pixel wide"));
    public static readonly ShapeStyleParameter Roughness = new("roughness", StateParameter.Bounded(Lens<ShapeStyle, ShortSideLength>.New(static style => style.Roughness, static value => style => style with { Roughness = value }), Presentations.Length with { Soft = (0f, 0.02f) }),
        new("Rough edge", "Edge noise displacement in frame short sides"));
    public static readonly ShapeStyleParameter Inverted = new("inverted", new StateParameter<ShapeStyle>.Toggle(Lens<ShapeStyle, bool>.New(static style => style.Inverted, static inverted => style => style with { Inverted = inverted })),
        new("Invert", "Fills outside boundary"));

    public StateParameter<ShapeStyle> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Shapes(
    ShapePrimitive First, Option<ShapeOperation> Operation, ShapePrimitive Second, ShortSideLength Smoothing, Fraction Progress, ShapeStyle Style, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Shapes, ShapesParameter>, IPixelStage<Shapes> {
    public const uint Stream = 16u;

    public static Shapes Default { get; } = new(
        ShapePrimitive.Default, None, ShapePrimitive.Default with { Kind = ShapeKind.Box, Roundness = Fraction.Create(0.3f) }, ShortSideLength.Neutral, Fraction.MinValue, ShapeStyle.Default,
        GeneratedLayer.Default, Seed.MinValue);

    public static Option<PixelPass> Pass(Shapes state, PassContext context) {
        EContext math = DistanceField.Precision(context.Extent);
        (FieldFrame frame, Func<long, long, FieldSample> first) = state.First.Field(context.Extent, math);
        Func<long, long, FieldSample> field = state.Operation.Match(
            Some: operation => (state.Second.Field(context.Extent, math).Sample, EFloat.FromSingle(state.Smoothing), (float)state.Progress) switch {
                var (second, smoothing, progress) => (Func<long, long, FieldSample>)((column, line) => operation.Join(first(column, line), second(column, line), smoothing, progress, math)),
            },
            None: () => first);
        Func<FieldSample, (double X, double Y), Vector4> shade = state.Style.Shading(context, state.Seed.Field(Stream, 0u), math);
        return Some(state.Layer.Pass((row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = shade(field(column + i, line), frame.Upward(column + i, line));
        }));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapesParameter : IStateParameter<Shapes> {
    public static readonly ShapesParameter First = new("first", new StateParameter<Shapes>.Record<ShapePrimitive>(Lens<Shapes, ShapePrimitive>.New(static shape => shape.First, static first => shape => shape with { First = first })),
        new("First", "First figure to combine"));
    public static readonly ShapesParameter Second = new("second", new StateParameter<Shapes>.Record<ShapePrimitive>(Lens<Shapes, ShapePrimitive>.New(static shape => shape.Second, static second => shape => shape with { Second = second })),
        new("Second", "Second figure to combine"));
    public static readonly ShapesParameter Operation = new("operation", new StateParameter<Shapes>.OptionalChoice<ShapeOperation>(Lens<Shapes, Option<ShapeOperation>>.New(static shape => shape.Operation, static operation => shape => shape with { Operation = operation }), "None"),
        new("Operation", "Combination of first figure with second"));
    public static readonly ShapesParameter Smoothing = new("smoothing", StateParameter.Bounded(Lens<Shapes, ShortSideLength>.New(static shape => shape.Smoothing, static smoothing => shape => shape with { Smoothing = smoothing }), Presentations.Length with { Soft = (0f, 0.25f) }),
        new("Smoothing", "Combination blend width in frame short sides"));
    public static readonly ShapesParameter Progress = new("progress", StateParameter.Bounded(Lens<Shapes, Fraction>.New(static shape => shape.Progress, static progress => shape => shape with { Progress = progress })),
        new("Progress", "Morph from first figure at 0% to second at 100%"));
    public static readonly ShapesParameter Style = new("style", new StateParameter<Shapes>.Record<ShapeStyle>(Lens<Shapes, ShapeStyle>.New(static shape => shape.Style, static style => shape => shape with { Style = style })),
        new("Style", "Figure fill and boundary"));
    public static readonly ShapesParameter Layer = new("layer", new StateParameter<Shapes>.Record<GeneratedLayer>(Lens<Shapes, GeneratedLayer>.New(static shape => shape.Layer, static layer => shape => shape with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly ShapesParameter Seed = new("seed", StateParameter.Bounded(Lens<Shapes, Seed>.New(static shape => shape.Seed, static seed => shape => shape with { Seed = seed })),
        new("Seed", "Number selecting pattern"));

    public StateParameter<Shapes> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Lightning(
    Placement Placement, ShortSideLength Length, ShortSideLength Thickness, SubdivisionDepth Depth, Fraction Displacement, BranchCount Branches, Fraction Reach,
    ForkSpread Spread, Fraction Taper, Hertz Speed, ShapeStyle Style, GeneratedLayer Layer, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Lightning, LightningParameter>, IPixelStage<Lightning> {
    public static Lightning Default { get; } = new(
        Placement.Default, ShortSideLength.Create(1f / 2f), ShortSideLength.Create(0.01f / (5f - (4.5f * 0.5f)) * ReferenceFrame.GreaterSide), SubdivisionDepth.MaxValue, Fraction.Create(0.5f),
        BranchCount.Standard, Fraction.Create(0.4f), ForkSpread.Standard, Fraction.Create(0.5f), Hertz.Create(1f / 0.05f), ShapeStyle.Default,
        GeneratedLayer.Added with { Exposure = Stops.Create(3f) }, Seed.MinValue, Timing.Default, Hold.Create(0.05f));

    public static Option<PixelPass> Pass(Lightning state, PassContext context) {
        (uint field, float clock, double chord) = (state.Seed.Field(17u, 0u), 2f * state.Speed * state.Hold.Held(state.Timing.At(context)), 2d * state.Length);
        Func<int, int, int, float> noise = (x, y, z) => 0.5f + (0.5f * NoiseDimensions.Four.Gradient(new Vector4((2f * x) + 0.5f, (2f * y) + 0.5f, (2f * z) + 0.5f, clock), field));
        Bolt[] channel = Bolt.Path((0d, state.Length), (0d, -1d), chord, SubdivisionDepth.MaxValue, state.Depth, state.Displacement, k => noise(k, 0, 0), _ => state.Thickness / 2d);
        Bolt[] pieces = [.. channel, .. Enumerable.Range(1, state.Branches).SelectMany(i => (BitOperations.Log2((uint)i), (i % 2 is 0 ? 1f : -1f) * state.Spread * (0.5f + noise(i, 0, 1))) switch {
            var (level, turn) => Bolt.Path(
                channel[((2 * (i - (1 << level))) + 1) * channel.Length / (2 << level)].Start, double.SinCos(turn) switch { var (sin, cos) => (sin, -cos) },
                state.Reach * chord * (0.5f + noise(i, 0, 2)), 3, state.Depth, state.Displacement, j => noise(j, i, 0), along => (double)state.Taper * state.Thickness * (1d - along) / 2d),
        })];
        EContext math = DistanceField.Precision(context.Extent);
        Func<FieldSample, (double X, double Y), Vector4> shade = state.Style.Shading(context, CoordinateHash.Branch(field, 0u), math);
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
            shade(pieces.Aggregate(FieldSample.Empty, (nearest, piece) => piece.Sample(point, math) switch { var next => next.Distance.CompareTo(nearest.Distance) < 0 ? next : nearest }), point))));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightningParameter : IStateParameter<Lightning> {
    public static readonly LightningParameter Placement = new("placement", new StateParameter<Lightning>.Record<Placement>(Lens<Lightning, Placement>.New(static bolt => bolt.Placement, static placement => bolt => bolt with { Placement = placement })),
        new("Placement", "Field position and turn"));
    public static readonly LightningParameter Length = new("length", StateParameter.Bounded(Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Length, static length => bolt => bolt with { Length = length }), Presentations.Length),
        new("Length", "Half stroke length in frame short sides"));
    public static readonly LightningParameter Thickness = new("thickness", StateParameter.Bounded(Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Thickness, static thickness => bolt => bolt with { Thickness = thickness }), Presentations.Length with { Soft = (0f, 0.02f) }),
        new("Thickness", "Stroke width in frame short sides"));
    public static readonly LightningParameter Depth = new("depth", StateParameter.Bounded(Lens<Lightning, SubdivisionDepth>.New(static bolt => bolt.Depth, static depth => bolt => bolt with { Depth = depth })),
        new("Detail", "Subdivisions of each displaced segment"));
    public static readonly LightningParameter Displacement = new("displacement", StateParameter.Bounded(Lens<Lightning, Fraction>.New(static bolt => bolt.Displacement, static displacement => bolt => bolt with { Displacement = displacement })),
        new("Displacement", "Share of each segment's length the stroke wanders by"));
    public static readonly LightningParameter Branches = new("branches", StateParameter.Bounded(Lens<Lightning, BranchCount>.New(static bolt => bolt.Branches, static branches => bolt => bolt with { Branches = branches })),
        new("Branches", "Forks from stroke midpoints"));
    public static readonly LightningParameter Reach = new("reach", StateParameter.Bounded(Lens<Lightning, Fraction>.New(static bolt => bolt.Reach, static reach => bolt => bolt with { Reach = reach })),
        new("Reach", "Branch length as share of stroke length"));
    public static readonly LightningParameter Spread = new("spread", StateParameter.Bounded(Lens<Lightning, ForkSpread>.New(static bolt => bolt.Spread, static spread => bolt => bolt with { Spread = spread })),
        new("Spread", "Angle branches turn from stroke"));
    public static readonly LightningParameter Taper = new("taper", StateParameter.Bounded(Lens<Lightning, Fraction>.New(static bolt => bolt.Taper, static taper => bolt => bolt with { Taper = taper })),
        new("Taper", "Branch base width relative to stroke, narrowing to tip"));
    public static readonly LightningParameter Speed = new("speed", StateParameter.Bounded(Lens<Lightning, Hertz>.New(static bolt => bolt.Speed, static speed => bolt => bolt with { Speed = speed })),
        new("Speed", "Noise units per second the bolt changes through"));
    public static readonly LightningParameter Style = new("style", new StateParameter<Lightning>.Record<ShapeStyle>(Lens<Lightning, ShapeStyle>.New(static bolt => bolt.Style, static style => bolt => bolt with { Style = style })),
        new("Style", "Figure fill and boundary"));
    public static readonly LightningParameter Layer = new("layer", new StateParameter<Lightning>.Record<GeneratedLayer>(Lens<Lightning, GeneratedLayer>.New(static bolt => bolt.Layer, static layer => bolt => bolt with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly LightningParameter Seed = new("seed", StateParameter.Bounded(Lens<Lightning, Seed>.New(static bolt => bolt.Seed, static seed => bolt => bolt with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly LightningParameter Timing = new("timing", new StateParameter<Lightning>.Record<Timing>(Lens<Lightning, Timing>.New(static bolt => bolt.Timing, static timing => bolt => bolt with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));
    public static readonly LightningParameter Hold = new("hold", StateParameter.Bounded(Lens<Lightning, Hold>.New(static bolt => bolt.Hold, static hold => bolt => bolt with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));

    public StateParameter<Lightning> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Gradient(GradientKind Kind, Placement Placement, ShortSideExtent Width, ShortSideExtent Height, ColorRamp Colors, GeneratedLayer Layer)
    : IStateRecord<Gradient, GradientParameter>, IPixelStage<Gradient> {
    public static Gradient Default { get; } = new(GradientKind.Linear, Placement.Default, ShortSideExtent.Create(1f / 2f), ShortSideExtent.Create(1f / 2f), Presets.Grayscale, GeneratedLayer.Default);

    public static Option<PixelPass> Pass(Gradient state, PassContext context) {
        Func<(double X, double Y), (double Position, double Slope)> parameter = state.Kind.Parameter(((float)state.Width, (float)state.Height));
        return Some(state.Layer.Pass(RampFootprint.Of(state.Colors, None), footprint => state.Placement.Frame(context.Extent).Fill(point =>
            parameter(point) switch { var (position, slope) => footprint.Mean(position, slope / context.Extent.ShortSide, state.Kind.Periodic) })));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientParameter : IStateParameter<Gradient> {
    private static readonly Presentation<float> Extent = Presentations.Extent with { Soft = (0.01f, 4f) };

    public static readonly GradientParameter Kind = new("kind", new StateParameter<Gradient>.Choice<GradientKind>(Lens<Gradient, GradientKind>.New(static gradient => gradient.Kind, static kind => gradient => gradient with { Kind = kind })),
        new("Type", "Field the ramp runs over"));
    public static readonly GradientParameter Placement = new("placement", new StateParameter<Gradient>.Record<Placement>(Lens<Gradient, Placement>.New(static gradient => gradient.Placement, static placement => gradient => gradient with { Placement = placement })),
        new("Placement", "Field position and turn"));
    public static readonly GradientParameter Width = new("width", StateParameter.Bounded(Lens<Gradient, ShortSideExtent>.New(static gradient => gradient.Width, static width => gradient => gradient with { Width = width }), Extent),
        new("Width", "Ramp half width in frame short sides"));
    public static readonly GradientParameter Height = new("height", StateParameter.Bounded(Lens<Gradient, ShortSideExtent>.New(static gradient => gradient.Height, static height => gradient => gradient with { Height = height }), Extent),
        new("Height", "Ramp half height in frame short sides"));
    public static readonly GradientParameter Colors = new("colors", new StateParameter<Gradient>.Gradient(Lens<Gradient, ColorRamp>.New(static gradient => gradient.Colors, static colors => gradient => gradient with { Colors = colors })),
        new("Colors", "Display color and coverage along ramp"));
    public static readonly GradientParameter Layer = new("layer", new StateParameter<Gradient>.Record<GeneratedLayer>(Lens<Gradient, GeneratedLayer>.New(static gradient => gradient.Layer, static layer => gradient => gradient with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));

    StateParameter<Gradient> IStateParameter<Gradient>.Kind => Parameter;

    private StateParameter<Gradient> Parameter { get; }
    public ParameterText Text { get; }
}

public sealed record CornerGradient(SKColorF TopLeft, SKColorF TopRight, SKColorF BottomRight, SKColorF BottomLeft, GeneratedLayer Layer)
    : IStateRecord<CornerGradient, CornerGradientParameter>, IPixelStage<CornerGradient> {
    public static CornerGradient Default { get; } = new(new SKColorF(1f, 1f, 1f), new SKColorF(0f, 0f, 0f), new SKColorF(1f, 1f, 1f), new SKColorF(0f, 0f, 0f), GeneratedLayer.Default);

    public static Option<PixelPass> Pass(CornerGradient state, PassContext context) {
        (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) = (Shown(state.TopLeft), Shown(state.TopRight), Shown(state.BottomRight), Shown(state.BottomLeft));
        PixelExtent extent = context.Extent;
        return Some(state.Layer.Pass((row, column, line) => {
            float down = (float)ShapeEdge.Smoother(double.Clamp((extent.Height - line - 0.5d) / extent.Height, 0d, 1d));
            for (int i = 0; i < row.Length; i++) {
                float across = (float)ShapeEdge.Smoother(double.Clamp((column + i + 0.5d) / extent.Width, 0d, 1d));
                row[i] = new Vector4(Vector3.Lerp(Vector3.Lerp(topLeft, topRight, across), Vector3.Lerp(bottomLeft, bottomRight, across), down), 1f);
            }
        }));

        static Vector3 Shown(SKColorF color) => new(color.Red, color.Green, color.Blue);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CornerGradientParameter : IStateParameter<CornerGradient> {
    public static readonly CornerGradientParameter TopLeft = new("top-left", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, SKColorF>.New(static corners => corners.TopLeft, static color => corners => corners with { TopLeft = color })),
        new("Top left", "Display color at top left corner"));
    public static readonly CornerGradientParameter TopRight = new("top-right", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, SKColorF>.New(static corners => corners.TopRight, static color => corners => corners with { TopRight = color })),
        new("Top right", "Display color at top right corner"));
    public static readonly CornerGradientParameter BottomRight = new("bottom-right", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, SKColorF>.New(static corners => corners.BottomRight, static color => corners => corners with { BottomRight = color })),
        new("Bottom right", "Display color at bottom right corner"));
    public static readonly CornerGradientParameter BottomLeft = new("bottom-left", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, SKColorF>.New(static corners => corners.BottomLeft, static color => corners => corners with { BottomLeft = color })),
        new("Bottom left", "Display color at bottom left corner"));
    public static readonly CornerGradientParameter Layer = new("layer", new StateParameter<CornerGradient>.Record<GeneratedLayer>(Lens<CornerGradient, GeneratedLayer>.New(static corners => corners.Layer, static layer => corners => corners with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));

    public StateParameter<CornerGradient> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Grid(
    GridPattern Pattern, Placement Placement, ShortSideExtent Cell, Factor Aspect, Fraction Offset, ShortSideLength LineWidth, ShortSideLength Feather,
    SKColorF LineColor, SKColorF CellColor, SKColorF AlternateColor, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Grid, GridParameter>, IPixelStage<Grid> {
    public static Grid Default { get; } = (Size: 3f, Thickness: 0.2f, Scale: 0.1f) switch {
        var (size, thickness, scale) => new(
            GridPattern.Lines, Placement.Default, ShortSideExtent.Create(1f / size), Factor.Unit, Fraction.MinValue,
            ShortSideLength.Create(2f * (0.02f + (0.18f * thickness)) * (0.1f + (0.4f * scale)) / size), ShortSideLength.Neutral,
            new SKColorF(1f, 1f, 1f), new SKColorF(0f, 0f, 0f), new SKColorF(1f, 1f, 1f), GeneratedLayer.Default, Seed.MinValue),
    };

    public static Option<PixelPass> Pass(Grid state, PassContext context) {
        GridLattice lattice = new(ERational.FromSingle(state.Cell), ERational.FromSingle(state.Cell).Multiply(ERational.FromSingle(state.Aspect)), ERational.FromSingle(state.Offset),
            state.LineWidth, ShapeEdge.Band(state.Feather, context.Extent) / context.Extent.ShortSide);
        Func<(double X, double Y), Vector4> shade = state.Pattern.Switch(
            (Lattice: lattice, Line: new Vector3(state.LineColor.Red, state.LineColor.Green, state.LineColor.Blue), Cell: new Vector3(state.CellColor.Red, state.CellColor.Green, state.CellColor.Blue),
                Alternate: new Vector3(state.AlternateColor.Red, state.AlternateColor.Green, state.AlternateColor.Blue), Field: state.Seed.Field(18u, 0u)),
            lines: static at => (Func<(double X, double Y), Vector4>)(point => new Vector4(at.Line, at.Lattice.Lines(point))),
            checker: static at => (Func<(double X, double Y), Vector4>)(point => new Vector4(at.Lattice.Cells(point, (i, j) => i.Add(j).IsEven ? at.Cell : at.Alternate, at.Line), 1f)),
            brick: static at => (Func<(double X, double Y), Vector4>)(point => new Vector4(at.Lattice.Cells(point, (i, j) => Vector3.Lerp(at.Cell, at.Alternate, CoordinateHash.Uniforms(i, j, at.Field).X), at.Line), 1f)));
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(shade)));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridParameter : IStateParameter<Grid> {
    private static readonly Presentation<float> Line = Presentations.Length with { Soft = (0f, 0.05f) };

    public static readonly GridParameter Pattern = new("pattern", new StateParameter<Grid>.Choice<GridPattern>(Lens<Grid, GridPattern>.New(static grid => grid.Pattern, static pattern => grid => grid with { Pattern = pattern })),
        new("Pattern", "Lines alone or filled cells"));
    public static readonly GridParameter Placement = new("placement", new StateParameter<Grid>.Record<Placement>(Lens<Grid, Placement>.New(static grid => grid.Placement, static placement => grid => grid with { Placement = placement })),
        new("Placement", "Field position and turn"));
    public static readonly GridParameter Cell = new("cell", StateParameter.Bounded(Lens<Grid, ShortSideExtent>.New(static grid => grid.Cell, static cell => grid => grid with { Cell = cell }), Presentations.Extent with { Soft = (0.01f, 1f) }),
        new("Cell", "Cell height in frame short sides"));
    public static readonly GridParameter Aspect = new("aspect", StateParameter.Bounded(Lens<Grid, Factor>.New(static grid => grid.Aspect, static aspect => grid => grid with { Aspect = aspect })),
        new("Aspect", "Cell width over cell height"));
    public static readonly GridParameter Offset = new("offset", StateParameter.Bounded(Lens<Grid, Fraction>.New(static grid => grid.Offset, static offset => grid => grid with { Offset = offset })),
        new("Brick offset", "Share of cell every second brick row shifts by"));
    public static readonly GridParameter LineWidth = new("line-width", StateParameter.Bounded(Lens<Grid, ShortSideLength>.New(static grid => grid.LineWidth, static width => grid => grid with { LineWidth = width }), Line),
        new("Line width", "Line width in frame short sides"));
    public static readonly GridParameter Feather = new("feather", StateParameter.Bounded(Lens<Grid, ShortSideLength>.New(static grid => grid.Feather, static feather => grid => grid with { Feather = feather }), Line),
        new("Feather", "Edge softness in frame short sides, 0% one pixel wide"));
    public static readonly GridParameter LineColor = new("line-color", new StateParameter<Grid>.Color(Lens<Grid, SKColorF>.New(static grid => grid.LineColor, static color => grid => grid with { LineColor = color })),
        new("Line color", "Display color of lines"));
    public static readonly GridParameter CellColor = new("cell-color", new StateParameter<Grid>.Color(Lens<Grid, SKColorF>.New(static grid => grid.CellColor, static color => grid => grid with { CellColor = color })),
        new("Cell color", "Display color of cells"));
    public static readonly GridParameter AlternateColor = new("alternate-color", new StateParameter<Grid>.Color(Lens<Grid, SKColorF>.New(static grid => grid.AlternateColor, static color => grid => grid with { AlternateColor = color })),
        new("Alternate color", "Display color of alternate checker cells and far brick tint end"));
    public static readonly GridParameter Layer = new("layer", new StateParameter<Grid>.Record<GeneratedLayer>(Lens<Grid, GeneratedLayer>.New(static grid => grid.Layer, static layer => grid => grid with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly GridParameter Seed = new("seed", StateParameter.Bounded(Lens<Grid, Seed>.New(static grid => grid.Seed, static seed => grid => grid with { Seed = seed })),
        new("Seed", "Number selecting pattern"));

    public StateParameter<Grid> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Stripes(
    WaveProfile Profile, Placement Placement, ShortSideExtent Wavelength, SignedAngle Bend, Fraction Phase, Hertz Speed, ColorRamp Colors, GeneratedLayer Layer, Timing Timing)
    : IStateRecord<Stripes, StripesParameter>, IPixelStage<Stripes> {
    public static Stripes Default { get; } = new(
        WaveProfile.Sine, Placement.Default, ShortSideExtent.Create(float.Tau / 20f), SignedAngle.Neutral, Fraction.MinValue, Hertz.Neutral, Presets.Grayscale, GeneratedLayer.Default, Timing.Default);

    public static Option<PixelPass> Pass(Stripes state, PassContext context) {
        ((double sin, double cos), ERational start, ERational wavelength) = (double.SinCos(state.Bend),
            ERational.FromSingle(state.Phase).Add(ERational.FromSingle(state.Speed).Multiply(ERational.FromSingle(state.Timing.At(context)))), ERational.FromSingle(state.Wavelength));
        ERational width = ERational.One.Divide(wavelength.Multiply(context.Extent.ShortSide));
        return Some(state.Layer.Pass(RampFootprint.Of(state.Colors, Some(state.Profile)), footprint => state.Placement.Frame(context.Extent).Fill(point =>
            footprint.Mean(ERational.FromDouble(point.Y).Multiply(ERational.FromDouble(cos)).Add(ERational.FromDouble(double.Abs(point.X)).Multiply(ERational.FromDouble(sin))).Divide(wavelength).Add(start),
                width, periodic: true))));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StripesParameter : IStateParameter<Stripes> {
    public static readonly StripesParameter Profile = new("profile", new StateParameter<Stripes>.Choice<WaveProfile>(Lens<Stripes, WaveProfile>.New(static stripes => stripes.Profile, static profile => stripes => stripes with { Profile = profile })),
        new("Profile", "Level across each stripe period"));
    public static readonly StripesParameter Placement = new("placement", new StateParameter<Stripes>.Record<Placement>(Lens<Stripes, Placement>.New(static stripes => stripes.Placement, static placement => stripes => stripes with { Placement = placement })),
        new("Placement", "Field position and turn"));
    public static readonly StripesParameter Wavelength = new("wavelength", StateParameter.Bounded(Lens<Stripes, ShortSideExtent>.New(static stripes => stripes.Wavelength, static wavelength => stripes => stripes with { Wavelength = wavelength }), Presentations.Extent with { Soft = (0.01f, 1f) }),
        new("Wavelength", "Stripe period in frame short sides"));
    public static readonly StripesParameter Bend = new("bend", StateParameter.Bounded(Lens<Stripes, SignedAngle>.New(static stripes => stripes.Bend, static bend => stripes => stripes with { Bend = bend }), Presentations.Angle),
        new("Bend", "Stripe fold about center line, 0 straight"));
    public static readonly StripesParameter Phase = new("phase", StateParameter.Bounded(Lens<Stripes, Fraction>.New(static stripes => stripes.Phase, static phase => stripes => stripes with { Phase = phase })),
        new("Phase", "Share of period stripes shift by"));
    public static readonly StripesParameter Speed = new("speed", StateParameter.Bounded(Lens<Stripes, Hertz>.New(static stripes => stripes.Speed, static speed => stripes => stripes with { Speed = speed }), Hertz.Slow),
        new("Speed", "Periods per second the stripes scroll"));
    public static readonly StripesParameter Colors = new("colors", new StateParameter<Stripes>.Gradient(Lens<Stripes, ColorRamp>.New(static stripes => stripes.Colors, static colors => stripes => stripes with { Colors = colors })),
        new("Colors", "Display color and coverage across one period"));
    public static readonly StripesParameter Layer = new("layer", new StateParameter<Stripes>.Record<GeneratedLayer>(Lens<Stripes, GeneratedLayer>.New(static stripes => stripes.Layer, static layer => stripes => stripes with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly StripesParameter Timing = new("timing", new StateParameter<Stripes>.Record<Timing>(Lens<Stripes, Timing>.New(static stripes => stripes.Timing, static timing => stripes => stripes with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<Stripes> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Letterbox(Gated<Factor> Aspect, ShortSideLength Inset, ShortSideLength Radius, ShapeStyle Style, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Letterbox, LetterboxParameter>, IPixelStage<Letterbox> {
    public static Letterbox Default { get; } = new(
        new(Enabled: true, Factor.Create(2.39f)), ShortSideLength.Neutral, ShortSideLength.Neutral,
        ShapeStyle.Default with { Fill = new(Enabled: true, new SKColorF(0f, 0f, 0f)), Inverted = true }, GeneratedLayer.Default, Seed.MinValue);

    public static Letterbox FilmGate { get; } = (Padding: 40f, Radius: 40f, Blur: 10f, Intensity: 6f) switch {
        var (padding, radius, blur, intensity) => Default with {
            Aspect = Default.Aspect with { Enabled = false },
            Inset = ShortSideLength.Create(padding / ReferenceFrame.Height),
            Radius = ShortSideLength.Create(radius / ReferenceFrame.Height),
            Style = Default.Style with { Feather = ShortSideLength.Create(blur / 3f * float.Sqrt(float.Tau) / ReferenceFrame.Height), Roughness = ShortSideLength.Create(intensity / ReferenceFrame.Height) },
        },
    };

    public static Seq<(string Name, EffectLook<Letterbox> Look)> Looks { get; } = [Presets.Look(nameof(FilmGate), FilmGate)];

    public static Option<PixelPass> Pass(Letterbox state, PassContext context) {
        EContext math = DistanceField.Precision(context.Extent);
        (double X, double Y) frame = ((double)context.Extent.Width / context.Extent.ShortSide, (double)context.Extent.Height / context.Extent.ShortSide);
        (double X, double Y) = state.Aspect.Active.Map(aspect => aspect >= frame.X / frame.Y ? (frame.X, frame.X / aspect) : (frame.Y * aspect, frame.Y)).IfNone(frame) switch {
            var (x, y) => ((x / 2d) - state.Inset, (y / 2d) - state.Inset),
        };
        (double small, Func<FieldSample, (double X, double Y), Vector4> shade) = (double.Min(X, Y), state.Style.Shading(context, state.Seed.Field(Shapes.Stream, 0u), math));
        EFloat radius = EFloat.FromDouble(double.Min(state.Radius, small));
        return Some(state.Layer.Pass(FieldFrame.Centered(context.Extent).Fill(point =>
            shade(small <= 0d ? FieldSample.Empty : new(DistanceField.Rounded(point, (X, Y), radius, math), 2d * small), point))));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LetterboxParameter : IStateParameter<Letterbox> {
    private static readonly Presentation<float> Margin = Presentations.Length with { Soft = (0f, 0.1f * ReferenceFrame.GreaterSide) };

    public static readonly LetterboxParameter Aspect = new("aspect", StateParameter.OptionalBounded(Lens<Letterbox, Gated<Factor>>.New(static bars => bars.Aspect, static aspect => bars => bars with { Aspect = aspect })),
        new("Aspect", "Width over height of window between bars, off follows frame"));
    public static readonly LetterboxParameter Inset = new("inset", StateParameter.Bounded(Lens<Letterbox, ShortSideLength>.New(static bars => bars.Inset, static inset => bars => bars with { Inset = inset }), Margin),
        new("Inset", "Window margin in frame short sides"));
    public static readonly LetterboxParameter Radius = new("radius", StateParameter.Bounded(Lens<Letterbox, ShortSideLength>.New(static bars => bars.Radius, static radius => bars => bars with { Radius = radius }), Margin),
        new("Roundness", "Window corner radius in frame short sides"));
    public static readonly LetterboxParameter Style = new("style", new StateParameter<Letterbox>.Record<ShapeStyle>(Lens<Letterbox, ShapeStyle>.New(static bars => bars.Style, static style => bars => bars with { Style = style })),
        new("Style", "Figure fill and boundary"));
    public static readonly LetterboxParameter Layer = new("layer", new StateParameter<Letterbox>.Record<GeneratedLayer>(Lens<Letterbox, GeneratedLayer>.New(static bars => bars.Layer, static layer => bars => bars with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly LetterboxParameter Seed = new("seed", StateParameter.Bounded(Lens<Letterbox, Seed>.New(static bars => bars.Seed, static seed => bars => bars with { Seed = seed })),
        new("Seed", "Number selecting pattern"));

    public StateParameter<Letterbox> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlareAnchor : ICaptioned {
    public static readonly FlareAnchor Frame = new("frame", "Frame position", [], static (position, _, context) => Some((position.Point(context.Extent), Option<float>.None)));
    public static readonly FlareAnchor Sun = new("sun", "Sun", [GuideChannel.Depth], static (_, _, context) =>
        context.Lights.ToFin(new Unrenderable("Sun flare anchor", "it needs scene lights")).Map(lights => lights.Sun.Bind(direction => Distant(direction, context))));
    public static readonly FlareAnchor Light = new("light", "Light", [GuideChannel.Depth], static (_, light, context) =>
        light.Traverse(target => context.Lights.ToFin(new Unrenderable(string.Create(CultureInfo.InvariantCulture, $"Light flare anchor {(Guid)target}"), "it needs scene lights")).Map(lights =>
            (from point in lights.Points.Find(target) from camera in context.Camera from located in camera.Pixel(point) select (located.Point, Depth: Some(located.Depth)))
            | lights.Directions.Find(target).Bind(direction => Distant(direction, context)))).As().Map(static located => located.Flatten()));

    public string Caption { get; }
    public Seq<GuideChannel> Occluders { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Option<((double X, double Y) Point, Option<float> Depth)>> Locate(Position position, Option<LightTarget> light, PassContext context);

    private static Option<((double X, double Y) Point, Option<float> Depth)> Distant(Vector3 direction, PassContext context) =>
        context.Camera.Bind(camera => camera.Distant(direction)).Map(static located => (located.Point, Some(located.Depth)));
}

public sealed record FlareSource(FlareAnchor Anchor, Position Position, Option<LightTarget> Light, bool Occlusion) : IStateRecord<FlareSource, FlareSourceParameter> {
    public static FlareSource Default { get; } = new(FlareAnchor.Frame, Position.Default, None, Occlusion: true);

    public Seq<GuideChannel> Channels => Occlusion ? Anchor.Occluders : [];

    public Option<PixelPass> Pass(PassContext context, Func<(double X, double Y), Option<Occluder>, Func<PixelFrame, IProgress<int>, Fin<Unit>>> kernel) =>
        Anchor.Locate(Position, Light, context).Match(
            Succ: location =>
                from source in location
                from occluder in Occlusion ? source.Depth.Traverse(depth => context.Guides.Find(GuideChannel.Depth).Map(guide => new Occluder(guide, depth))).As() : Some(Option<Occluder>.None)
                select (PixelPass)new PixelPass.Frame(kernel(source.Point, occluder)),
            Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlareSourceParameter : IStateParameter<FlareSource> {
    public static readonly FlareSourceParameter Anchor = new("anchor", new StateParameter<FlareSource>.Choice<FlareAnchor>(Lens<FlareSource, FlareAnchor>.New(static source => source.Anchor, static anchor => source => source with { Anchor = anchor })),
        new("Source", "Point generated light converges on"));
    public static readonly FlareSourceParameter Position = new("position", new StateParameter<FlareSource>.Record<Position>(Lens<FlareSource, Position>.New(static source => source.Position, static position => source => source with { Position = position })),
        new("Position", "Frame point to converge on when Source is Frame position"));
    public static readonly FlareSourceParameter Light = new("light", new StateParameter<FlareSource>.OptionalKeyed<LightTarget, Guid>(Lens<FlareSource, Option<LightTarget>>.New(static source => source.Light, static light => source => source with { Light = light })),
        new("Light", "Document light to converge on when Source is Light"));
    public static readonly FlareSourceParameter Occlusion = new("occlusion", new StateParameter<FlareSource>.Toggle(Lens<FlareSource, bool>.New(static source => source.Occlusion, static occlusion => source => source with { Occlusion = occlusion })),
        new("Occlusion", "Surfaces nearer camera than source hide its light"));

    public StateParameter<FlareSource> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record LensFlare(
    FlareSource Source, Gated<Stops> Hotspot, ColorRamp HotspotColors, ShortSideLength HotspotSize, ShortSideLength HotspotSoftness,
    Gated<Stops> Starburst, ColorRamp StarburstColors, ShortSideLength StarburstLength, NoiseBasis Basis, Magnitude StarburstDensity, Magnitude StarburstSharpness, Hertz Shimmer, Hold Hold,
    Gated<Stops> Bar, ColorRamp BarColors, ShortSideLength BarLength, ShortSideLength BarThickness, SignedAngle BarAngle,
    Gated<Stops> Ring, ColorRamp RingColors, ShortSideLength RingRadius, ShortSideLength RingWidth, ShortSideLength RingSoftness, SignedMagnitude RingMagnification,
    Gated<Stops> Ghosts, ColorRamp GhostColors, ShortSideLength GhostSize, PositiveCount GhostCount, SignedMagnitude GhostFirst, SignedMagnitude GhostLast, ProperFraction GhostVariation,
    Fraction GhostSoftness, Aperture Aperture, SignedFraction Fringe, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LensFlare, LensFlareParameter>, IPixelStage<LensFlare> {
    private const uint Stream = 19u;
    private static readonly ShortSideLength SourceRadius = ShortSideLength.Create(0.2f * 0.3f / 2f * ReferenceFrame.GreaterSide);

    public static LensFlare Default { get; } = new(
        FlareSource.Default with { Position = new(FrameAxis.Create(0.3f), FrameAxis.Create(1f - 0.6f)) },
        new(Enabled: true, Stops.Zero), Presets.White, SourceRadius, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide),
        new(Enabled: true, Stops.Zero), Presets.White, ShortSideLength.Create(0.5f), NoiseBasis.Fbm.Default with { Fractal = Fractal.Default with { Roughness = Fraction.Create(0.563f) } },
        Magnitude.Create(30f), Magnitude.Create(3f), Hertz.Neutral, Hold.MinValue,
        new(Enabled: true, Stops.Zero), Presets.White, ShortSideLength.Create(0.972f / 2f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide), SignedAngle.Neutral,
        new(Enabled: true, Stops.Zero), Presets.White, ShortSideLength.Create(1f / 2.1f), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide),
        SignedMagnitude.Zero, new(Enabled: true, Stops.Zero), Presets.White, SourceRadius, PositiveCount.Create(5), SignedMagnitude.Create(0.6f), SignedMagnitude.Create(-1f), ProperFraction.Create(0.5f),
        Fraction.Create(0.3f), Aperture.Default, SignedFraction.Neutral, GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static LensFlare NachoLens { get; } =
        Default with { Aperture = new(ApertureBlades.Create(3), SignedAngle.Create(0.179f), Fraction.Create(0.332f), Fraction.MinValue, AnamorphicSqueeze.Spherical), Fringe = SignedFraction.Create(-0.1f) };
    public static LensFlare RingLens { get; } =
        Default with { Aperture = new(ApertureBlades.Six, SignedAngle.Neutral, Fraction.Create(0.796f), Fraction.Create(0.902f), AnamorphicSqueeze.Spherical), Fringe = SignedFraction.Create(0.189f) };
    public static LensFlare HexLens { get; } =
        Default with { Aperture = new(ApertureBlades.Six, SignedAngle.Neutral, Fraction.MinValue, Fraction.Create(0.902f), AnamorphicSqueeze.Spherical), Fringe = SignedFraction.Create(0.052f) };
    public static LensFlare RoundLens { get; } =
        Default with { Aperture = new(ApertureBlades.Create(16), SignedAngle.Neutral, Fraction.MaxValue, Fraction.MinValue, AnamorphicSqueeze.Spherical), Fringe = SignedFraction.Create(0.012f) };

    public static Seq<(string Name, EffectLook<LensFlare> Look)> Looks { get; } = [
        Presets.Look(nameof(NachoLens), NachoLens), Presets.Look(nameof(RingLens), RingLens), Presets.Look(nameof(HexLens), HexLens), Presets.Look(nameof(RoundLens), RoundLens)];

    public static Seq<GuideChannel> Channels(LensFlare state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LensFlare state, PassContext context) =>
        state.Source.Pass(context, (point, occlusion) => (frame, progress) =>
            from iris in state.Aperture.Pupil
            from hotspot in RampLight.Of(state.HotspotColors, context.Working)
            from starburst in RampLight.Of(state.StarburstColors, context.Working)
            from bar in RampLight.Of(state.BarColors, context.Working)
            from ring in RampLight.Of(state.RingColors, context.Working)
            from ghost in RampLight.Of(state.GhostColors, context.Working)
            from drawn in Fill(state, context, point, iris, (hotspot, starburst, bar, ring, ghost)).Match(
                Some: fill => state.Layer.Pass(fill(occlusion.Map(occluder => occluder.Visible(point, state.HotspotSize.Pixels(context.Extent))).IfNone(1f))).Run(frame, progress),
                None: static () => unit)
            select drawn);

    private static Option<Func<float, Action<Span<Vector4>, long, long>>> Fill(
        LensFlare state, PassContext context, (double X, double Y) source, Iris iris, (RampLight Hotspot, RampLight Starburst, RampLight Bar, RampLight Ring, RampLight Ghost) tables) {
        (FieldFrame frame, double side) = (FieldFrame.Centered(context.Extent), context.Extent.ShortSide);
        (double X, double Y) = frame.Local(source) switch { var (x, y) => (x, -y) };
        (double first, double last, uint draws, uint field) = (state.GhostFirst, state.GhostLast, state.Seed.Field(Stream, 1u), state.Seed.Field(Stream, 0u));
        (double hotspot, double edge, double burst, double density, float sharpness) =
            (state.HotspotSize, ShapeEdge.Band(state.HotspotSoftness, context.Extent), state.StarburstLength, state.StarburstDensity / double.Tau, state.StarburstSharpness);
        Func<LatticePoint, uint, NoiseSample<LatticePoint>> basis = state.Basis.Sampler;
        ERational shimmer = ERational.FromSingle(state.Shimmer).Multiply(ERational.FromSingle(state.Hold.Held(state.Timing.At(context))));
        (Matrix3x2 turn, (double X, double Y) halo, double width, double rim) =
            (Matrix3x2.CreateRotation(state.BarAngle), (state.RingMagnification * X, state.RingMagnification * Y), state.RingWidth, ShapeEdge.Band(state.RingSoftness, context.Extent));
        ((double X, double Y) Center, float Flip, double Radius, double Ramp, double Bound, Vector3 Color)[] ghosts = [..
            from k in state.Ghosts.Enabled ? Enumerable.Range(0, state.GhostCount) : []
            let u = CoordinateHash.Uniforms(k, 0L, draws)
            let position = (k + 0.5d + ((double)state.GhostVariation * (u.Y - 0.5d))) / state.GhostCount
            let m = double.Lerp(first, last, position)
            let shrink = 1d - ((double)state.GhostVariation * u.X)
            let radius = state.GhostSize.Pixels(context.Extent) * shrink
            let ramp = ((double)state.GhostSoftness * radius) + 1d
            let color = tables.Ghost.Sample((float)position)
            select ((m * X, m * Y), (float)double.CopySign(1d, m), radius, ramp, (iris.Circumradius * radius) + (ramp / 2d), color.AsVector3() * (float)(color.W / (shrink * shrink)))];
        Seq<(Gated<Stops> Gain, RampLight Table, Func<(double X, double Y), (float Tone, Vector3 Weight)> Shape)> elements =
            [(state.Hotspot, tables.Hotspot, Disc), (state.Starburst, tables.Starburst, Streaks), (state.Bar, tables.Bar, Anamorphic), (state.Ring, tables.Ring, Halo)];
        Func<(double X, double Y), Vector3>[] terms = [..
            from element in elements
            from gain in element.Gain.Active.ToSeq()
            select (Func<(double X, double Y), Vector3>)(q => element.Shape(q) switch { var (tone, weight) => gain.Scale * weight * (element.Table.Sample(tone) switch { var color => color.AsVector3() * color.W }) }),
            .. from gain in state.Ghosts.Active.ToSeq() select (Func<(double X, double Y), Vector3>)(q => gain.Scale * Chain(q))];
        return terms.Length is 0 ? None : Some<Func<float, Action<Span<Vector4>, long, long>>>(visible => frame.Fill(q => new Vector4(visible * terms.Aggregate(Vector3.Zero, (light, term) => light + term(q)), 1f)));

        (float, Vector3) Disc((double X, double Y) q) => double.Hypot(q.X - X, q.Y - Y) switch {
            var r => ((float)(hotspot is 0d ? 0d : double.Min(r / hotspot, 1d)), new Vector3(float.Min(ShapeEdge.Coverage((r - hotspot) * side, edge), (float)(2d * hotspot * side / edge)))),
        };

        (float, Vector3) Streaks((double X, double Y) q) => burst is 0d ? (0f, Vector3.Zero) : ((X: q.X - X, Y: q.Y - Y), double.Min(double.Hypot(q.X - X, q.Y - Y) / burst, 1d)) switch {
            (var d, < 1d and var u) => ((float)u, new Vector3(MathF.Pow(float.Max(0f, (2f * Noise(d)) - 1f), sharpness) * (float)((1d - u) * (1d - u)))),
            (_, var u) => ((float)u, Vector3.Zero),
        };

        float Noise((double X, double Y) d) =>
            double.SinCos(double.Atan2(d.Y, d.X)) switch { var (sin, cos) => basis(new(ERational.FromDouble(density * cos), ERational.FromDouble(density * sin), shimmer, ERational.Zero), field).Value };

        (float, Vector3) Anamorphic((double X, double Y) q) => state.BarLength == ShortSideLength.Neutral ? (0f, Vector3.Zero) : (q.X - X, q.Y - Y) switch {
            var (x, y) => double.Min(double.Abs((x * turn.M11) + (y * turn.M21)) / state.BarLength, 1d) switch {
                var u => ((float)u, new Vector3((float)((1d - u) * (1d - u)) * ShapeEdge.Strip(double.Abs((x * turn.M12) + (y * turn.M22)) * side, state.BarThickness.Pixels(context.Extent), 1d))),
            },
        };

        (float, Vector3) Halo((double X, double Y) q) => (double.Hypot(q.X - halo.X, q.Y - halo.Y) - state.RingRadius) switch {
            var delta => (float.Clamp((float)(width is 0d ? (double.Sign(delta) + 1d) / 2d : (delta / width) + 0.5d), 0f, 1f), new Vector3(ShapeEdge.Strip(double.Abs(delta) * side, width * side, rim))),
        };

        Vector3 Chain((double X, double Y) q) {
            Vector256<double> sum = Vector256<double>.Zero;
            foreach (((double X, double Y) center, float flip, double radius, double ramp, double bound, Vector3 color) in ghosts) {
                (double x, double y) = ((q.X - center.X) * side, (q.Y - center.Y) * side);
                if (double.Hypot(x, y) > bound)
                    continue;
                (double reach, double outline) = iris.Outline((flip * x, -flip * y));
                sum += Vector256.Create(color.X, color.Y, color.Z, 0d) * iris.Weights(reach, outline, radius, ramp, state.Fringe);
            }
            return new((float)sum[0], (float)sum[1], (float)sum[2]);
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensFlareParameter : IStateParameter<LensFlare> {
    private static readonly Presentation<float> Spot = Presentations.Length with { Soft = (0.001f, 0.25f) };
    private static readonly Presentation<float> Reach = Presentations.Length with { Soft = (0.01f, 2f) };
    private static readonly Presentation<float> Thin = Presentations.Length with { Soft = (0.001f, 0.25f), Track = Some(Presentations.Cubic) };
    private static readonly Presentation<float> Magnification = SignedMagnitude.Presentation with { Soft = (-2f, 2f) };

    public static readonly LensFlareParameter Source = new("source", new StateParameter<LensFlare>.Record<FlareSource>(Lens<LensFlare, FlareSource>.New(static flare => flare.Source, static source => flare => flare with { Source = source })),
        new("Source", "Frame position or scene light generated light follows"));
    public static readonly LensFlareParameter Hotspot = new("hotspot", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Stops>>.New(static flare => flare.Hotspot, static hotspot => flare => flare with { Hotspot = hotspot })),
        new("Hotspot", "Glow brightness at source in stops"));
    public static readonly LensFlareParameter HotspotColors = new("hotspot-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, ColorRamp>.New(static flare => flare.HotspotColors, static colors => flare => flare with { HotspotColors = colors })),
        new("Hotspot colors", "Color and coverage from center to hotspot edge"));
    public static readonly LensFlareParameter HotspotSize = new("hotspot-size", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.HotspotSize, static size => flare => flare with { HotspotSize = size }), Spot),
        new("Hotspot size", "Hotspot radius in frame short sides"));
    public static readonly LensFlareParameter HotspotSoftness = new("hotspot-softness", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.HotspotSoftness, static softness => flare => flare with { HotspotSoftness = softness }), Thin),
        new("Hotspot softness", "Hotspot edge softness in frame short sides"));
    public static readonly LensFlareParameter Starburst = new("starburst", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Stops>>.New(static flare => flare.Starburst, static starburst => flare => flare with { Starburst = starburst })),
        new("Starburst", "Ray brightness around source in stops"));
    public static readonly LensFlareParameter StarburstColors = new("starburst-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, ColorRamp>.New(static flare => flare.StarburstColors, static colors => flare => flare with { StarburstColors = colors })),
        new("Starburst colors", "Color and coverage from source along each ray"));
    public static readonly LensFlareParameter StarburstLength = new("starburst-length", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.StarburstLength, static length => flare => flare with { StarburstLength = length }), Reach),
        new("Starburst length", "Ray reach in frame short sides"));
    public static readonly LensFlareParameter Basis = new("basis", new StateParameter<LensFlare>.Variant<NoiseBasis>(Lens<LensFlare, NoiseBasis>.New(static flare => flare.Basis, static basis => flare => flare with { Basis = basis })),
        new("Starburst noise", "Ray pattern around source"));
    public static readonly LensFlareParameter StarburstDensity = new("starburst-density", StateParameter.Bounded(Lens<LensFlare, Magnitude>.New(static flare => flare.StarburstDensity, static density => flare => flare with { StarburstDensity = density }),
        new Presentation<float> { Track = Some(Presentations.Cubic), Soft = (1f, 100f) }),
        new("Starburst density", "Noise periods per turn around source"));
    public static readonly LensFlareParameter StarburstSharpness = new("starburst-sharpness", StateParameter.Bounded(Lens<LensFlare, Magnitude>.New(static flare => flare.StarburstSharpness, static sharpness => flare => flare with { StarburstSharpness = sharpness }),
        new Presentation<float> { Soft = (1f, 8f) }),
        new("Starburst sharpness", "Power narrowing each ray around its brightest axis"));
    public static readonly LensFlareParameter Shimmer = new("shimmer", StateParameter.Bounded(Lens<LensFlare, Hertz>.New(static flare => flare.Shimmer, static shimmer => flare => flare with { Shimmer = shimmer }), Hertz.Presentation with { Soft = (0f, 10f) }),
        new("Shimmer", "Noise units per second the rays change through"));
    public static readonly LensFlareParameter Hold = new("hold", StateParameter.Bounded(Lens<LensFlare, Hold>.New(static flare => flare.Hold, static hold => flare => flare with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly LensFlareParameter Bar = new("bar", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Stops>>.New(static flare => flare.Bar, static bar => flare => flare with { Bar = bar })),
        new("Anamorphic bar", "Bar brightness through source in stops"));
    public static readonly LensFlareParameter BarColors = new("bar-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, ColorRamp>.New(static flare => flare.BarColors, static colors => flare => flare with { BarColors = colors })),
        new("Bar colors", "Color and coverage from source along bar"));
    public static readonly LensFlareParameter BarLength = new("bar-length", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.BarLength, static length => flare => flare with { BarLength = length }), Reach),
        new("Bar length", "Bar half length in frame short sides"));
    public static readonly LensFlareParameter BarThickness = new("bar-thickness", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.BarThickness, static thickness => flare => flare with { BarThickness = thickness }), Thin),
        new("Bar thickness", "Bar width in frame short sides"));
    public static readonly LensFlareParameter BarAngle = new("bar-angle", StateParameter.Bounded(Lens<LensFlare, SignedAngle>.New(static flare => flare.BarAngle, static angle => flare => flare with { BarAngle = angle }), Presentations.Angle),
        new("Bar angle", "Counterclockwise turn from horizontal"));
    public static readonly LensFlareParameter Ring = new("ring", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Stops>>.New(static flare => flare.Ring, static ring => flare => flare with { Ring = ring })),
        new("Halo", "Halo brightness in stops"));
    public static readonly LensFlareParameter RingColors = new("ring-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, ColorRamp>.New(static flare => flare.RingColors, static colors => flare => flare with { RingColors = colors })),
        new("Halo colors", "Color and coverage across halo band"));
    public static readonly LensFlareParameter RingRadius = new("ring-radius", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.RingRadius, static radius => flare => flare with { RingRadius = radius }), Presentations.Length),
        new("Halo radius", "Halo radius in frame short sides"));
    public static readonly LensFlareParameter RingWidth = new("ring-width", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.RingWidth, static width => flare => flare with { RingWidth = width }), Thin),
        new("Halo width", "Halo band width in frame short sides"));
    public static readonly LensFlareParameter RingSoftness = new("ring-softness", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.RingSoftness, static softness => flare => flare with { RingSoftness = softness }), Thin),
        new("Halo softness", "Halo edge softness in frame short sides"));
    public static readonly LensFlareParameter RingMagnification = new("ring-magnification", StateParameter.Bounded(Lens<LensFlare, SignedMagnitude>.New(static flare => flare.RingMagnification, static magnification => flare => flare with { RingMagnification = magnification }), Magnification),
        new("Halo position", "Halo center along source axis, 0 at frame center, 1 at source"));
    public static readonly LensFlareParameter Ghosts = new("ghosts", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Stops>>.New(static flare => flare.Ghosts, static ghosts => flare => flare with { Ghosts = ghosts })),
        new("Ghosts", "Iris image brightness along source axis in stops"));
    public static readonly LensFlareParameter GhostColors = new("ghost-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, ColorRamp>.New(static flare => flare.GhostColors, static colors => flare => flare with { GhostColors = colors })),
        new("Ghost colors", "Color and coverage along ghost chain"));
    public static readonly LensFlareParameter GhostSize = new("ghost-size", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static flare => flare.GhostSize, static size => flare => flare with { GhostSize = size }), Spot),
        new("Ghost size", "Largest ghost radius in frame short sides"));
    public static readonly LensFlareParameter GhostCount = new("ghost-count", StateParameter.Bounded(Lens<LensFlare, PositiveCount>.New(static flare => flare.GhostCount, static count => flare => flare with { GhostCount = count }), PositiveCount.Presentation with { Step = 1, Soft = (1, 16) }),
        new("Ghost count", "Iris images along source axis"));
    public static readonly LensFlareParameter GhostFirst = new("ghost-first", StateParameter.Bounded(Lens<LensFlare, SignedMagnitude>.New(static flare => flare.GhostFirst, static ghost => flare => flare with { GhostFirst = ghost }), Magnification),
        new("First ghost", "Ghost chain start, 0 at frame center, 1 at source"));
    public static readonly LensFlareParameter GhostLast = new("ghost-last", StateParameter.Bounded(Lens<LensFlare, SignedMagnitude>.New(static flare => flare.GhostLast, static ghost => flare => flare with { GhostLast = ghost }), Magnification),
        new("Last ghost", "Ghost chain end, 0 at frame center, 1 at source"));
    public static readonly LensFlareParameter GhostVariation = new("ghost-variation", StateParameter.Bounded(Lens<LensFlare, ProperFraction>.New(static flare => flare.GhostVariation, static variation => flare => flare with { GhostVariation = variation }),
        ProperFraction.Presentation with { Soft = (0f, 0.9f) }),
        new("Ghost variation", "Variation in each ghost's position and radius"));
    public static readonly LensFlareParameter GhostSoftness = new("ghost-softness", StateParameter.Bounded(Lens<LensFlare, Fraction>.New(static flare => flare.GhostSoftness, static softness => flare => flare with { GhostSoftness = softness })),
        new("Ghost softness", "Share of each ghost's radius feathered at its edge"));
    public static readonly LensFlareParameter Aperture = new("iris", new StateParameter<LensFlare>.Record<Aperture>(Lens<LensFlare, Aperture>.New(static flare => flare.Aperture, static aperture => flare => flare with { Aperture = aperture })),
        new("Aperture", "Lens opening shaping bokeh and diffraction spikes"));
    public static readonly LensFlareParameter Fringe = new("fringe", StateParameter.Bounded(Lens<LensFlare, SignedFraction>.New(static flare => flare.Fringe, static fringe => flare => flare with { Fringe = fringe })),
        new("Color shift", "Red and blue bokeh sizes apart from green"));
    public static readonly LensFlareParameter Layer = new("layer", new StateParameter<LensFlare>.Record<GeneratedLayer>(Lens<LensFlare, GeneratedLayer>.New(static flare => flare.Layer, static layer => flare => flare with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly LensFlareParameter Seed = new("seed", StateParameter.Bounded(Lens<LensFlare, Seed>.New(static flare => flare.Seed, static seed => flare => flare with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly LensFlareParameter Timing = new("timing", new StateParameter<LensFlare>.Record<Timing>(Lens<LensFlare, Timing>.New(static flare => flare.Timing, static timing => flare => flare with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<LensFlare> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record LightRays(Highlights Highlight, FlareSource Source, ShortSideLength Size, Fraction Jitter, SKColorF Tint, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LightRays, LightRaysParameter>, IPixelStage<LightRays> {
    public static float Diagonal { get; } = float.Hypot(ReferenceFrame.GreaterSide, 1f);

    public static LightRays Default { get; } =
        new(Highlights.Default, FlareSource.Default, ShortSideLength.Create(0.5f * Diagonal), Fraction.MinValue, new SKColorF(1f, 1f, 1f), GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static Seq<GuideChannel> Channels(LightRays state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LightRays state, PassContext context) =>
        state.Source.Pass(context, (point, occlusion) => (frame, progress) => state.Highlight.Key.Bind(highlight => {
            Func<(double X, double Y), bool> hidden = occlusion.Match(Some: static occluder => (Func<(double X, double Y), bool>)occluder.Hides, None: static () => static _ => false);
            PixelFrame key = new(frame.Origin, frame.Size, frame.Extent, block => {
                Span2D<Vector4> keys = block.AsSpan().Cast<float, Vector4>().AsSpan2D(frame.Size.Height, frame.Size.Width);
                ReadOnlySpan2D<Vector4> light = frame.View.Span;
                for (int y = 0; y < keys.Height; y++) {
                    for (int x = 0; x < keys.Width; x++)
                        keys[y, x] = hidden(((long)frame.Origin.X + x + 0.5d, (long)frame.Origin.Y + y + 0.5d)) ? Vector4.Zero : highlight.Extract(light[y, x]);
                }
            });
            return state.Layer.Pass(Fill(state, context, point, key)).Run(frame, progress);
        }));

    private static Action<Span<Vector4>, long, long> Fill(LightRays state, PassContext context, (double X, double Y) source, PixelFrame key) {
        (double reach, float jitter, PixelExtent extent) = (double.Truncate((double)state.Size * context.Extent.ShortSide), state.Jitter, context.Extent);
        (uint field, Vector3 tint) = (state.Seed.Field(20u, state.Timing.At(context).Term), RampLight.Light(state.Tint, context.Working).AsVector3());
        Func<long, long, float> shift = state.Jitter == Fraction.MinValue ? static (_, _) => 0f : (column, line) => CoordinateHash.Uniforms(column, line, field).X;
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                (double X, double Y) p = (column + i + 0.5d, extent.Height - line - 0.5d);
                row[i] = new Vector4(tint * (reach is 0d ? Sample(p) : Rays(p, shift(column + i, line))).AsVector3(), 1f);
            }
        };

        Vector4 Sample((double X, double Y) p) => key.Sample(p, (WrapMode.Black, WrapMode.Black), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight));

        Vector4 Rays((double X, double Y) p, float offset) {
            double distance = double.Max(1d, double.Hypot(source.X - p.X, source.Y - p.Y));
            (double steps, (double X, double Y) step) = (double.Floor(double.Min(reach, distance)), ((source.X - p.X) / distance, (source.Y - p.Y) / distance));
            (Vector4 sum, float total) = (Vector4.Zero, 0f);
            for (long k = 0; k <= (1d - jitter) * steps; k++) {
                double at = jitter is 1f ? 0d : (k + (double)offset) / (1d - jitter);
                (double X, double Y) t = (p.X + (at * step.X), p.Y + (at * step.Y));
                if (t.X < 0d || t.X > extent.Width || t.Y < 0d || t.Y > extent.Height)
                    break;
                float fall = (float)(1d - (at / steps));
                (sum, total) = (sum + (fall * fall * Sample(t)), total + (fall * fall));
            }
            return total is not 0f ? sum / total : Sample(p);
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightRaysParameter : IStateParameter<LightRays> {
    public static readonly LightRaysParameter Highlight = new("highlight", new StateParameter<LightRays>.Record<Highlights>(Lens<LightRays, Highlights>.New(static rays => rays.Highlight, static highlight => rays => rays with { Highlight = highlight })),
        new("Highlights", "Highlights that cast rays"));
    public static readonly LightRaysParameter Source = new("source", new StateParameter<LightRays>.Record<FlareSource>(Lens<LightRays, FlareSource>.New(static rays => rays.Source, static source => rays => rays with { Source = source })),
        new("Source", "Frame position or scene light generated light follows"));
    public static readonly LightRaysParameter Size = new("size", StateParameter.Bounded(Lens<LightRays, ShortSideLength>.New(static rays => rays.Size, static size => rays => rays with { Size = size }), Presentations.Length with { Soft = (0f, LightRays.Diagonal) }),
        new("Size", "Longest ray march in frame short sides"));
    public static readonly LightRaysParameter Jitter = new("jitter", StateParameter.Bounded(Lens<LightRays, Fraction>.New(static rays => rays.Jitter, static jitter => rays => rays with { Jitter = jitter })),
        new("Jitter", "Share of march steps traded for random sample shifts"));
    public static readonly LightRaysParameter Tint = new("tint", new StateParameter<LightRays>.Color(Lens<LightRays, SKColorF>.New(static rays => rays.Tint, static tint => rays => rays with { Tint = tint })),
        new("Tint", "Ray color"));
    public static readonly LightRaysParameter Layer = new("layer", new StateParameter<LightRays>.Record<GeneratedLayer>(Lens<LightRays, GeneratedLayer>.New(static rays => rays.Layer, static layer => rays => rays with { Layer = layer })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly LightRaysParameter Seed = new("seed", StateParameter.Bounded(Lens<LightRays, Seed>.New(static rays => rays.Seed, static seed => rays => rays with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly LightRaysParameter Timing = new("timing", new StateParameter<LightRays>.Record<Timing>(Lens<LightRays, Timing>.New(static rays => rays.Timing, static timing => rays => rays with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<LightRays> Kind { get; }
    public ParameterText Text { get; }
}

file readonly record struct Mote(
    ERational Depth, int Layer, EInteger Column, EInteger Line, LatticePoint Point, (ERational X, ERational Y) Rest, double Scale, double Radius, double Gain, Vector4 Coating, Vector256<double> Light);

file readonly record struct Bolt((double X, double Y) Start, (double X, double Y) End, double From, double To) {
    public FieldSample Sample((double X, double Y) point, EContext math) => FieldSample.Tapered(point, Start, End, From, To, math);

    public static Bolt[] Path((double X, double Y) start, (double X, double Y) direction, double length, int levels, int depth, float displacement, Func<int, float> draw, Func<double, double> radius) {
        int segments = 1 << levels;
        double[] offsets = new double[segments + 1];
        for (int level = 1; level <= levels; level++)
            for (int step = segments >> level, k = step; k < segments; k += 2 * step)
                offsets[k] = ((offsets[k - step] + offsets[k + step]) / 2d) + (level <= depth ? displacement * (2d * step * length / segments) * ((2f * draw(k)) - 1f) : 0d);
        (double X, double Y)[] vertices = [.. offsets.Select((offset, k) => (start.X + (k * length / segments * direction.X) - (offset * direction.Y), start.Y + (k * length / segments * direction.Y) + (offset * direction.X)))];
        return [.. Enumerable.Range(0, segments).Select(k => new Bolt(vertices[k], vertices[k + 1], radius(k / (double)segments), radius((k + 1d) / segments)))];
    }
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record Unrenderable(string Subject, string Reason) : Expected($"{Subject} cannot render: {Reason}", 0);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Presets {
    public static ColorRamp.Smooth Grayscale { get; } = new([new(new SKColorF(0f, 0f, 0f), Fraction.MinValue), new(new SKColorF(1f, 1f, 1f), Fraction.MaxValue)]);
    public static ColorRamp.Smooth White { get; } = new([new(new SKColorF(1f, 1f, 1f), Fraction.MinValue)]);
    public static Easing Ease { get; } = Easing.Create(1d / 3d, 0d, 2d / 3d, 1d);

    public static NoiseSource Source(NoiseBasis basis, ShortSideExtent scaleX, ShortSideExtent scaleY) => new(basis, scaleX, scaleY, Drift.Neutral, Drift.Neutral, Hertz.Neutral);

    public static float Map(float value, (float Low, float High) edges) => float.Clamp((value - edges.Low) / (edges.High - edges.Low), 0f, 1f);

    public static SKColorF Shown(Vector4 light) =>
        new Unicolour(ColourSpace.RgbLinear, light.X, light.Y, light.Z).Rgb switch { var rgb => new((float)rgb.R, (float)rgb.G, (float)rgb.B, light.W) };

    public static ColorRamp.Smooth Sampled(CssSpace space, Func<float, Vector4> light, params ReadOnlySpan<Fraction> positions) =>
        new(toSeq(positions.ToArray()).Map(position => new Stop<SKColorF>(Shown(light(position)), position))) { Space = space };

    public static ColorRamp.Smooth Threshold(CssSpace space, Vector3 light, Fraction low, Fraction high) => Sampled(space, value => new Vector4(light, Map(value, (low, high))), low, high);

    public static ColorRamp.Smooth Solid(Vector3 light) => Sampled(CssSpace.Oklab, _ => new Vector4(light, 1f), Fraction.MinValue);

    public static (string Name, EffectLook<TState> Look) Look<TState>(string member, TState state) where TState : IStateRecord<TState> => (Captions.Of(member), new EffectLook<TState>.Whole(state));
}
