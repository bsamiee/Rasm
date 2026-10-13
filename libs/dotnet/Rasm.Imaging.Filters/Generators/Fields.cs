using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using PeterO.Numbers;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using TorchSharp;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Generators;

// --- [CONSTANTS] -----------------------------------------------------------------------
file static class Tracks {
    public static Presentation<float> Extent { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log };
    public static Presentation<float> Rate { get; } = Frequency.Presentation with { Soft = (-4f, 4f) };
    public static Presentation<float> Share { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct FogDistance : IMinMaxValue<FogDistance> {
    public static FogDistance MinValue => Neutral;
    public static FogDistance MaxValue { get; } = new(1e6f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct FogAttenuation : IMinMaxValue<FogAttenuation> {
    public static FogAttenuation MinValue { get; } = new(0.001f);
    public static FogAttenuation MaxValue { get; } = new(1e6f);
    public static FogAttenuation Standard { get; } = new((float)(double)Length.FromFeet(100).Meters);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct OpticalDepth : IMinMaxValue<OpticalDepth> {
    public static OpticalDepth MinValue { get; } = new(0f);
    public static OpticalDepth MaxValue { get; } = new(64f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct TunnelSlope : IMinMaxValue<TunnelSlope> {
    public static TunnelSlope MinValue => Neutral;
    public static TunnelSlope MaxValue { get; } = new(10f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Rings", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct VortexArms : IMinMaxValue<VortexArms> {
    public static VortexArms MinValue => Rings;
    public static VortexArms MaxValue { get; } = new(32);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

public sealed record GeneratedLayer(BlendingMode Mode, Exposure Exposure) : IStateRecord<GeneratedLayer, GeneratedLayerParameter, InvalidGenerator> {
    public static GeneratedLayer Default { get; } = new(BlendingMode.Mix, Exposure.Neutral);
    public static GeneratedLayer Added { get; } = Default with { Mode = BlendingMode.Add };

    internal PixelPass Pass(Action<Span<Vector4>, long, long> fill) =>
        (Exposure.Scale, Mode) switch {
            var (gain, mode) => new PixelPass.Pointwise((row, column, line) => {
                using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
                using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                fill(layer.Span, column, line);
                for (int i = 0; i < row.Length; i++)
                    (weights.Span[i], layer.Span[i]) = (layer.Span[i].W, new Vector4(layer.Span[i].AsVector3() * gain, 1f));
                if (mode == BlendingMode.Mix)
                    mode.Composite(row, layer.Span, weights.Span, AlphaRepresentation.Unassociated);
                else {
                    mode.Mixed(row, layer.Span, weights.Span);
                    layer.Span.CopyTo(row);
                }
            }),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeneratedLayerParameter : IStateParameter<GeneratedLayer> {
    public static readonly GeneratedLayerParameter Mode = new("mode", new StateParameter<GeneratedLayer>.Choice<BlendingMode, InvalidGrade>(
        Lens<GeneratedLayer, BlendingMode>.New(static layer => layer.Mode, static mode => layer => layer with { Mode = mode })));
    public static readonly GeneratedLayerParameter Exposure = new("exposure", StateParameter.Bounded<GeneratedLayer, Exposure, float, InvalidToneValue>(Lens<GeneratedLayer, Exposure>.New(static layer => layer.Exposure, static exposure => layer => layer with { Exposure = exposure }), Tone.Exposure.Presentation));

    public StateParameter<GeneratedLayer> Kind { get; }
}

public sealed record Placement(FramePosition Center, SignedAngle Rotation) : IStateRecord<Placement, PlacementParameter, InvalidGenerator> {
    public static Placement Default { get; } = new(FramePosition.Center, SignedAngle.Neutral);

    internal FieldFrame Frame(PixelExtent extent) => new(extent, Center.Point(extent), double.SinCos(Rotation));
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlacementParameter : IStateParameter<Placement> {
    private static readonly (StateParameter<Placement> X, StateParameter<Placement> Y) Center =
        FramePosition.Kinds(Lens<Placement, FramePosition>.New(static placement => placement.Center, static center => placement => placement with { Center = center }));
    public static readonly PlacementParameter CenterX = new("center-x", Center.X);
    public static readonly PlacementParameter CenterY = new("center-y", Center.Y);
    public static readonly PlacementParameter Rotation = new("rotation", StateParameter.Bounded<Placement, SignedAngle, float, InvalidPixelValue>(Lens<Placement, SignedAngle>.New(static placement => placement.Rotation, static rotation => placement => placement with { Rotation = rotation }), SignedAngle.Presentation));

    public StateParameter<Placement> Kind { get; }
}

internal readonly record struct FieldFrame(PixelExtent Extent, (double X, double Y) Center, (double Sin, double Cos) Turn) {
    public (double X, double Y) Local((double X, double Y) point) =>
        ((point.X - Center.X) / Extent.ShortSide, (point.Y - Center.Y) / Extent.ShortSide) switch {
            var (x, y) => ((x * Turn.Cos) - (y * Turn.Sin), (x * Turn.Sin) + (y * Turn.Cos)),
        };

    public (double X, double Y) Upward(long column, long line) => Local((column + 0.5d, Extent.Height - line - 0.5d)) switch { var (x, y) => (x, -y) };

    public Action<Span<Vector4>, long, long> Fill(Func<(double X, double Y), Vector4> shade) {
        FieldFrame frame = this;
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = shade(frame.Upward(column + i, line));
        };
    }
}

public sealed record NoiseSource(NoiseBasis Basis, ShortSideExtent ScaleX, ShortSideExtent ScaleY, Drift DriftX, Drift DriftY, Frequency Evolution)
    : IStateRecord<NoiseSource, NoiseSourceParameter, InvalidGenerator> {
    public static NoiseSource Default { get; } = Looks.Source(NoiseBasis.Default, 1f, Vector2.One);

    internal Func<(ERational X, ERational Y, ERational Z), ERational, float> Sampler(float time, uint field) =>
        (Basis.Sampler, ERational.FromSingle(ScaleX), ERational.FromSingle(ScaleY), ERational.FromSingle(time)) switch {
            var (basis, scaleX, scaleY, clock) => (ERational.FromSingle(DriftX).Multiply(clock), ERational.FromSingle(DriftY).Multiply(clock), ERational.FromSingle(Evolution).Multiply(clock)) switch {
                var (driftX, driftY, lift) => (point, w) => basis((point.X.Subtract(driftX).Divide(scaleX), point.Y.Subtract(driftY).Divide(scaleY), point.Z.Divide(scaleX).Add(lift), w), field).Value,
            },
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseSourceParameter : IStateParameter<NoiseSource> {
    private static readonly Presentation<float> Travel = Drift.Presentation with { Soft = (-1f, 1f) };

    public static readonly NoiseSourceParameter Basis = new("basis", new StateParameter<NoiseSource>.Record<NoiseBasis>(Lens<NoiseSource, NoiseBasis>.New(static noise => noise.Basis, static basis => noise => noise with { Basis = basis })));
    public static readonly NoiseSourceParameter ScaleX = new("scale-x", StateParameter.Bounded<NoiseSource, ShortSideExtent, float, InvalidPixelValue>(Lens<NoiseSource, ShortSideExtent>.New(static noise => noise.ScaleX, static scale => noise => noise with { ScaleX = scale }), Tracks.Extent));
    public static readonly NoiseSourceParameter ScaleY = new("scale-y", StateParameter.Bounded<NoiseSource, ShortSideExtent, float, InvalidPixelValue>(Lens<NoiseSource, ShortSideExtent>.New(static noise => noise.ScaleY, static scale => noise => noise with { ScaleY = scale }), Tracks.Extent));
    public static readonly NoiseSourceParameter DriftX = new("drift-x", StateParameter.Bounded<NoiseSource, Drift, float, InvalidGenerator>(Lens<NoiseSource, Drift>.New(static noise => noise.DriftX, static drift => noise => noise with { DriftX = drift }), Travel));
    public static readonly NoiseSourceParameter DriftY = new("drift-y", StateParameter.Bounded<NoiseSource, Drift, float, InvalidGenerator>(Lens<NoiseSource, Drift>.New(static noise => noise.DriftY, static drift => noise => noise with { DriftY = drift }), Travel));
    public static readonly NoiseSourceParameter Evolution = new("evolution", StateParameter.Bounded<NoiseSource, Frequency, float, InvalidGenerator>(Lens<NoiseSource, Frequency>.New(static noise => noise.Evolution, static evolution => noise => noise with { Evolution = evolution }), Tracks.Rate));

    public StateParameter<NoiseSource> Kind { get; }
}

public sealed record VortexField(VortexArms Arms, SignedAngle Twist, ShortSideExtent Radius, TunnelSlope Tunnel, Frequency Spin)
    : IStateRecord<VortexField, VortexFieldParameter, InvalidGenerator> {
    public static VortexField Default { get; } = new(
        VortexArms.Create(1), SignedAngle.Create(2.8f), ShortSideExtent.Create(0.5f), TunnelSlope.Create(0.6f), Frequency.Create((float)(double)UnitsNet.Frequency.FromRadiansPerSecond(1).Hertz));

    internal Func<(double X, double Y), (ERational X, ERational Y, ERational Z)> Shape(float time) =>
        (ERational.FromSingle(Radius), ERational.FromDouble(double.Tau), ERational.FromSingle(Spin).Multiply(ERational.FromSingle(time))) switch {
            var (size, period, clock) => point => (double.Hypot(point.X, point.Y), double.Atan2(point.Y, point.X)) switch {
                var (radius, angle) => (ERational.FromDouble(angle).Multiply((int)Arms).Add(ERational.FromSingle(Twist).Multiply(ERational.One.Subtract(ERational.FromDouble(radius).Divide(size)))).Divide(period).Add(clock), ERational.Min(ERational.FromDouble(radius), size)) switch {
                    var (phase, reach) => double.SinCos(NoiseFunctions.Split(phase).Local.Multiply(period).ToDouble()) switch {
                        var (sin, cos) => (reach.Multiply(ERational.FromDouble(cos)), reach.Multiply(ERational.FromDouble(sin)), reach.Multiply(ERational.FromSingle(Tunnel))),
                    },
                },
            },
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VortexFieldParameter : IStateParameter<VortexField> {
    public static readonly VortexFieldParameter Arms = new("arms", StateParameter.Bounded<VortexField, VortexArms, int, InvalidGenerator>(Lens<VortexField, VortexArms>.New(static vortex => vortex.Arms, static arms => vortex => vortex with { Arms = arms }), new() { Step = 1 }));
    public static readonly VortexFieldParameter Twist = new("twist", StateParameter.Bounded<VortexField, SignedAngle, float, InvalidPixelValue>(Lens<VortexField, SignedAngle>.New(static vortex => vortex.Twist, static twist => vortex => vortex with { Twist = twist }), SignedAngle.Presentation));
    public static readonly VortexFieldParameter Radius = new("radius", StateParameter.Bounded<VortexField, ShortSideExtent, float, InvalidPixelValue>(Lens<VortexField, ShortSideExtent>.New(static vortex => vortex.Radius, static radius => vortex => vortex with { Radius = radius }), Tracks.Extent));
    public static readonly VortexFieldParameter Tunnel = new("tunnel", StateParameter.Bounded<VortexField, TunnelSlope, float, InvalidGenerator>(Lens<VortexField, TunnelSlope>.New(static vortex => vortex.Tunnel, static tunnel => vortex => vortex with { Tunnel = tunnel }), new() { Soft = (0f, 4f) }));
    public static readonly VortexFieldParameter Spin = new("spin", StateParameter.Bounded<VortexField, Frequency, float, InvalidGenerator>(Lens<VortexField, Frequency>.New(static vortex => vortex.Spin, static spin => vortex => vortex with { Spin = spin }), Frequency.Presentation with { Soft = (-1f, 1f) }));

    public StateParameter<VortexField> Kind { get; }
}

public sealed record FieldEnvelope(ShortSideExtent Width, ShortSideExtent Height) : IStateRecord<FieldEnvelope, FieldEnvelopeParameter, InvalidGenerator> {
    public static FieldEnvelope Default { get; } = new(Looks.Features(1f), Looks.Features(1f));

    internal float Falloff((double X, double Y) point) => double.Max(1d - double.Hypot(point.X / Width, point.Y / Height), 0d) switch { var edge => (float)(1d - (edge * edge)) };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldEnvelopeParameter : IStateParameter<FieldEnvelope> {
    private static readonly Presentation<float> Size = Tracks.Extent with { Soft = (0.1f, 4f) };

    public static readonly FieldEnvelopeParameter Width = new("width", StateParameter.Bounded<FieldEnvelope, ShortSideExtent, float, InvalidPixelValue>(Lens<FieldEnvelope, ShortSideExtent>.New(static envelope => envelope.Width, static width => envelope => envelope with { Width = width }), Size));
    public static readonly FieldEnvelopeParameter Height = new("height", StateParameter.Bounded<FieldEnvelope, ShortSideExtent, float, InvalidPixelValue>(Lens<FieldEnvelope, ShortSideExtent>.New(static envelope => envelope.Height, static height => envelope => envelope with { Height = height }), Size));

    public StateParameter<FieldEnvelope> Kind { get; }
}

public sealed record Emission(ColorTemperature Cool, ColorTemperature Hot) : IStateRecord<Emission, EmissionParameter, InvalidGenerator> {
    private static readonly ColorTemperature Hottest = ColorTemperature.Create(12000d);

    public static Emission Default { get; } = new(ColorTemperature.MinValue, ColorTemperature.Flame);

    public static Presentation<double> Presentation { get; } = new() {
        Unit = ColorTemperature.Unit,
        Soft = (ColorTemperature.MinValue, Hottest),
        Stops = Ramp.Create(Seq(ColorTemperature.MinValue, ColorTemperature.Flame, ColorTemperature.ViewDefault, Hottest).Map(static kelvin =>
            RampStop.Create(RampPosition.Create((kelvin - (double)ColorTemperature.MinValue) / (Hottest - (double)ColorTemperature.MinValue)), new Vector4(Planck(Gamut.StandardRgb, kelvin), 1f))), RampInterpolation.Linear),
    };

    internal static Func<(Gamut Gamut, ColorTemperature Cool, ColorTemperature Hot), Func<float, Vector3>> Span { get; } =
        memo<(Gamut Gamut, ColorTemperature Cool, ColorTemperature Hot), Func<float, Vector3>>(static key => {
            const int intervals = 256;
            Vector3[] samples = [.. Enumerable.Range(0, intervals + 1).Select(i => Planck(key.Gamut, ColorTemperature.Create(double.Lerp(key.Cool, key.Hot, i / (double)intervals))))];
            return t => (t * intervals) switch { var texel => int.Min((int)texel, intervals - 1) switch { var i => Vector3.Lerp(samples[i], samples[i + 1], texel - i) } };
        });

    private static Vector3 Planck(Gamut gamut, ColorTemperature kelvin) =>
        Adaptation.Blackbody(gamut, kelvin).RgbLinear switch { var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B) };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EmissionParameter : IStateParameter<Emission> {
    public static readonly EmissionParameter Cool = new("cool", StateParameter.Bounded<Emission, ColorTemperature, double, InvalidColor>(Lens<Emission, ColorTemperature>.New(static emission => emission.Cool, static cool => emission => emission with { Cool = cool }), Emission.Presentation));
    public static readonly EmissionParameter Hot = new("hot", StateParameter.Bounded<Emission, ColorTemperature, double, InvalidColor>(Lens<Emission, ColorTemperature>.New(static emission => emission.Hot, static hot => emission => emission with { Hot = hot }), Emission.Presentation));

    public StateParameter<Emission> Kind { get; }
}

public sealed record NoiseTexture(
    NoiseSource Noise, Placement Placement, Gated<VortexField> VortexShape, ShortSideOffset Warp, NoiseBasis WarpBasis, Ramp Colors,
    Gated<FieldEnvelope> Envelope, Gated<Emission> Incandescence, Gated<OpticalDepth> Extinction, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<NoiseTexture, NoiseTextureParameter, InvalidGenerator>, IPixelStage<NoiseTexture> {
    public static NoiseTexture Default { get; } = new(
        Looks.Source(NoiseBasis.Default, 5f, Vector2.One), Placement.Default, new(Enabled: false, VortexField.Default), ShortSideOffset.Neutral,
        Looks.Fractal(2f, 0.186957f, 2f, 14.9f) with { Kind = BasisKind.Wave, WavePhase = SignedAngle.Create(-3f), WaveDetailScale = WaveDetailScale.Create(1.3f) },
        Ramp.Grayscale, new(Enabled: false, FieldEnvelope.Default), new(Enabled: false, Emission.Default), new(Enabled: false, OpticalDepth.MinValue), GeneratedLayer.Default, Seed.MinValue, Timing.Default);

    public static NoiseTexture TurbulentNoise { get; } = Default with { Noise = Default.Noise with { Evolution = Frequency.Create(0.3f) } };

    public static NoiseTexture BlockNoise { get; } = Default with { Noise = Default.Noise with { Basis = NoiseBasis.Default with { Kind = BasisKind.White } } };

    public static NoiseTexture Diamonds { get; } = Default with {
        Noise = Default.Noise with { Basis = NoiseBasis.Plain with { Kind = BasisKind.F2, Cellular = Cellular.Default with { Randomness = AxisFraction.MinValue } } },
    };

    public static NoiseTexture VoronoiNoise { get; } = TurbulentNoise with { Noise = TurbulentNoise.Noise with { Basis = Looks.Fractal(1f, 0.5f, 2f, 0f) with { Kind = BasisKind.F1 } } };

    public static NoiseTexture Sparkle { get; } = VoronoiNoise with {
        Noise = VoronoiNoise.Noise with {
            Basis = VoronoiNoise.Noise.Basis with { Kind = BasisKind.SmoothF1, Cellular = Cellular.Default with { Metric = CellMetric.Minkowski, Exponent = MinkowskiExponent.Create(1f) } },
        },
    };

    public static NoiseTexture VoronoiCells { get; } = Default with {
        Noise = Default.Noise with { Basis = NoiseBasis.Plain with { Kind = BasisKind.EdgeDistance }, DriftY = Drift.Create(-0.6f) },
    };

    public static NoiseTexture Waves { get; } = Default with {
        Noise = Looks.Source(NoiseBasis.Default with { Kind = BasisKind.Wave, Distortion = NoiseDistortion.Create(4f), WaveDetailScale = WaveDetailScale.Create(2f) }, 20f / float.Tau, Vector2.One) with {
            DriftX = Drift.Create(-0.3f / 20f),
        },
    };

    public static NoiseTexture LightLeak { get; } = (Fade: (Low: 0.1f, High: 0.6f), Bias: (Low: 0.225f, High: 0.775f)) switch {
        var (fade, bias) => Default with {
            Noise = Looks.Source(NoiseBasis.Plain, 0.7f, Vector2.One) with { Evolution = Frequency.Create(0.5f) },
            Colors = Looks.Sampled(RampInterpolation.LinearRgb, n =>
                new Vector4(Vector3.Lerp(new(1f, 0.7529f, 0.6584f), new(0.0437f, 0.0116f, 1f), Looks.Map(n, bias)), 1f - Looks.Map(n, fade)), fade.Low, bias.Low, fade.High),
            Layer = GeneratedLayer.Added,
        },
    };

    public static NoiseTexture Flow { get; } = (Cover: (Low: 0.37f, High: 0.87f), Tint: (Low: 0.62f, High: 1.1f)) switch {
        var (cover, tint) => Default with {
            Noise = Looks.Source(Looks.Fractal(2.48f, 0f, 12.4f, 7.25f), 1f, Vector2.One) with { Evolution = Frequency.Create(0.15f) },
            Warp = ShortSideOffset.Create(0.0566f),
            Colors = Looks.Sampled(RampInterpolation.LinearRgb, n => new Vector4(
                Vector3.Lerp(new(0.305f, 0.129f, 1f), new(1f, 0.059f, 0.298f), (float)RampInterpolation.SmootherRgb.Weight(Looks.Map(n, tint))), Looks.Map(n, cover)),
                cover.Low, tint.Low, cover.High, 1f),
            Layer = GeneratedLayer.Added,
        },
    };

    public static NoiseTexture Vortex { get; } = Default with {
        Noise = Looks.Source(Looks.Fractal(4f, 0.2f, 3f, 0.5f), 6f, Vector2.One) with { Evolution = Frequency.Create(0.3f) },
        VortexShape = Default.VortexShape with { Enabled = true },
        Colors = Looks.Sampled(RampInterpolation.LinearRgb, static n =>
            (n * ((0.8f * 2f * new Vector3(0.134f, 0.431f, 0.5f)) + new Vector3(0.2f))) switch { var tint => new Vector4(tint * tint * tint * tint, 1f) }, 0f, 0.5f, 1f),
    };

    public static NoiseTexture Fire { get; } = Default with {
        Noise = Looks.Source(Looks.Fractal(4f, 0.5f, 2f, 1f), 0.7f, new(1f / 2f, 1f)) with { DriftY = Drift.Create(5f) },
        Placement = Placement.Default with { Center = new(FrameAxis.Middle, FrameAxis.End) },
        Colors = Looks.Threshold(RampInterpolation.LinearRgb, Vector3.One, 0.17f, 1f),
        Envelope = Default.Envelope with { Enabled = true },
        Incandescence = Default.Incandescence with { Enabled = true },
        Layer = GeneratedLayer.Added,
    };

    public static NoiseTexture Smoke { get; } = Default with {
        Noise = Looks.Source(Looks.Fractal(5f, 0.55f, 2f, 0.5f), 1f / (0.2f * ReferenceFrame.GreaterSide), new(1f, 1.5f)) with {
            DriftY = Drift.Create(0.05f * ReferenceFrame.GreaterSide),
            Evolution = Frequency.Create(0.3f),
        },
        Colors = Looks.Threshold(RampInterpolation.EaseRgb, new Vector3(0.04f), 0.4f, 0.8f),
        Extinction = new(Enabled: true, OpticalDepth.Create(2f)),
    };

    public static NoiseTexture Mist { get; } = Default with {
        Noise = Looks.Source(Looks.Fractal(3f, 0.45f, 2f, 0f), 1f / (0.5f * ReferenceFrame.GreaterSide), new(1f, 0.5f)) with {
            DriftX = Drift.Create(0.02f * ReferenceFrame.GreaterSide),
            Evolution = Frequency.Create(0.1f),
        },
        Colors = Looks.Threshold(RampInterpolation.EaseRgb, new Vector3(0.7f), 0.3f, 0.9f),
        Extinction = new(Enabled: true, OpticalDepth.Create(0.6f)),
    };

    public static Option<PixelPass> Pass(NoiseTexture state, PassContext context) {
        (uint field, float time, RampTable colors) = (CoordinateHash.Field(NoiseStream.NoiseTexture, state.Seed, 0u), state.Timing.At(context), state.Colors.Tabulate(context.Working));
        Func<(ERational X, ERational Y, ERational Z), ERational, float> noise = state.Noise.Sampler(time, CoordinateHash.Branch(field, 0u));
        Func<(double X, double Y), (ERational X, ERational Y, ERational Z)> shape = state.VortexShape.Active.Map(vortex => vortex.Shape(time)).IfNone(static point => (ERational.FromDouble(point.X), ERational.FromDouble(point.Y), ERational.Zero));
        Func<(ERational X, ERational Y, ERational Z), (ERational X, ERational Y, ERational Z)> warp = state.Warp == ShortSideOffset.Neutral ? static at => at
            : (state.Noise with { Basis = state.WarpBasis }, ERational.FromSingle(state.Warp).Multiply(2)) switch {
                var (source, gain) => (source.Sampler(time, CoordinateHash.Branch(field, 1u)), source.Sampler(time, CoordinateHash.Branch(field, 2u))) switch {
                    var (across, along) => at => (at.X.Add(gain.Multiply(ERational.FromSingle(across(at, ERational.Zero) - 0.5f))), at.Y.Add(gain.Multiply(ERational.FromSingle(along(at, ERational.Zero) - 0.5f))), at.Z),
                },
            };
        Func<(double X, double Y), float> rim = state.Envelope.Active.Map<Func<(double X, double Y), float>>(static envelope => envelope.Falloff).IfNone(static _ => 0f);
        Func<Vector4, Vector3> light = state.Incandescence.Active.Map<Func<Vector4, Vector3>>(emission => Emission.Span((context.Working, emission.Cool, emission.Hot)) switch {
            var span => color => span(color.W),
        }).IfNone(static color => color.AsVector3());
        Func<float, float> opacity = state.Extinction.Active.Map<Func<float, float>>(static depth => cover => 1f - float.Exp(-depth * cover)).IfNone(static cover => cover);
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
            colors.Sample(noise(warp(shape(point)), ERational.Zero) - rim(point)) switch { var color => new Vector4(light(color), opacity(color.W)) })));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseTextureParameter : IStateParameter<NoiseTexture> {
    public static readonly NoiseTextureParameter Noise = new("noise", new StateParameter<NoiseTexture>.Record<NoiseSource>(
        Lens<NoiseTexture, NoiseSource>.New(static texture => texture.Noise, static noise => texture => texture with { Noise = noise })));
    public static readonly NoiseTextureParameter Placement = new("placement", new StateParameter<NoiseTexture>.Record<Placement>(
        Lens<NoiseTexture, Placement>.New(static texture => texture.Placement, static placement => texture => texture with { Placement = placement })));
    public static readonly NoiseTextureParameter Vortex = new("vortex", new StateParameter<NoiseTexture>.OptionalRecord<VortexField>(
        Lens<NoiseTexture, Gated<VortexField>>.New(static texture => texture.VortexShape, static vortex => texture => texture with { VortexShape = vortex })));
    public static readonly NoiseTextureParameter Warp = new("warp", StateParameter.Bounded<NoiseTexture, ShortSideOffset, float, InvalidPixelValue>(Lens<NoiseTexture, ShortSideOffset>.New(static texture => texture.Warp, static warp => texture => texture with { Warp = warp }), ShortSideOffset.Presentation));
    public static readonly NoiseTextureParameter WarpBasis = new("warp-basis", new StateParameter<NoiseTexture>.Record<NoiseBasis>(
        Lens<NoiseTexture, NoiseBasis>.New(static texture => texture.WarpBasis, static basis => texture => texture with { WarpBasis = basis })));
    public static readonly NoiseTextureParameter Colors = new("colors", new StateParameter<NoiseTexture>.Gradient(
        Lens<NoiseTexture, Ramp>.New(static texture => texture.Colors, static colors => texture => texture with { Colors = colors })));
    public static readonly NoiseTextureParameter Envelope = new("envelope", new StateParameter<NoiseTexture>.OptionalRecord<FieldEnvelope>(
        Lens<NoiseTexture, Gated<FieldEnvelope>>.New(static texture => texture.Envelope, static envelope => texture => texture with { Envelope = envelope })));
    public static readonly NoiseTextureParameter Incandescence = new("incandescence", new StateParameter<NoiseTexture>.OptionalRecord<Emission>(
        Lens<NoiseTexture, Gated<Emission>>.New(static texture => texture.Incandescence, static emission => texture => texture with { Incandescence = emission })));
    public static readonly NoiseTextureParameter Extinction = new("extinction", StateParameter.OptionalBounded<NoiseTexture, OpticalDepth, float, InvalidGenerator>(Lens<NoiseTexture, Gated<OpticalDepth>>.New(static texture => texture.Extinction, static extinction => texture => texture with { Extinction = extinction }), new() { Soft = (0f, 8f) }));
    public static readonly NoiseTextureParameter Layer = new("layer", new StateParameter<NoiseTexture>.Record<GeneratedLayer>(
        Lens<NoiseTexture, GeneratedLayer>.New(static texture => texture.Layer, static layer => texture => texture with { Layer = layer })));
    public static readonly NoiseTextureParameter Seed = new("seed", StateParameter.Bounded<NoiseTexture, Seed, int, InvalidGenerator>(Lens<NoiseTexture, Seed>.New(static texture => texture.Seed, static seed => texture => texture with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly NoiseTextureParameter Timing = new("timing", new StateParameter<NoiseTexture>.Record<Timing>(
        Lens<NoiseTexture, Timing>.New(static texture => texture.Timing, static timing => texture => texture with { Timing = timing })));

    public StateParameter<NoiseTexture> Kind { get; }
}

public sealed record Fog(
    NoiseSource Noise, Mix Variation, FogDistance Start, Gated<FogAttenuation> Attenuation, Gated<FogDistance> Cutoff, Gated<ShortSideLength> Diffusion,
    Mix Amount, Ramp Colors, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Fog, FogParameter, InvalidGenerator>, IPixelStage<Fog> {
    public static Fog Default { get; } = new(
        Looks.Source(Looks.Fractal(4f, 0.2f, 2f, 0f) with { Dimensions = NoiseDimensions.Four }, 1f, Vector2.One), Mix.MinValue, FogDistance.Neutral,
        new(Enabled: true, FogAttenuation.Standard), new(Enabled: false, FogDistance.Neutral), new(Enabled: false, ShortSideLength.Neutral), Mix.MaxValue, Looks.Sampled(RampInterpolation.Linear, static _ => new Vector4(new Vector3(MathF.Pow(0.85098f, GammaExponent.Gamma22)), 1f), 0f),
        GeneratedLayer.Default, Seed.MinValue, Timing.Default);

    public static Fog Haze { get; } = Default with {
        Attenuation = Default.Attenuation with { Enabled = false },
        Amount = Mix.Create(0.35f),
        Diffusion = new(Enabled: true, ShortSideLength.Create(0.75f * ReferenceFrame.GreaterSide)),
        Colors = Looks.Sampled(RampInterpolation.Linear, static _ => new Vector4(new Vector3(Exposure.MiddleGrey), 1f), 0f),
    };

    public static Seq<GuideChannel> Channels(Fog state) => state.Attenuation.Active.Map(static _ => GuideChannel.Depth).ToSeq();

    public static Option<PixelPass> Pass(Fog state, PassContext context) =>
        state.Attenuation.Active.Match(
            Some: attenuation => from depth in context.Guides.Find(GuideChannel.Depth)
                                 from camera in context.Camera
                                 select Extinction(state, context, depth, camera, attenuation),
            None: static () => Some<Func<long, long, float>>(static (_, _) => 1f))
        .Map(opacity => state.Diffusion.Active.Match(
            Some: radius => new PixelPass.Frame((frame, progress) => {
                double pixels = radius.Pixels(context.Extent);
                return from taps in PixelSampling.KernelTaps<long>(pixels)
                       from result in frame.Filter(source => {
                           PixelFrame spread = new(frame.Origin, frame.Size, frame.Extent, static _ => { });
                           using Mat target = spread.Header();
                           return source.Gaussian(target, (taps, taps), pixels / 3d, pixels / 3d, PaddingModes.Replicate)
                               .Bind(_ => state.Layer.Pass(Fill(state, context, opacity, (column, line) => spread.Row(line)[(int)(column - spread.Origin.X)].AsVector3())).Run(frame, progress));
                       })
                       select result;
            }),
            None: () => state.Layer.Pass(Fill(state, context, opacity, static (_, _) => Vector3.One))));

    private static Func<long, long, float> Extinction(Fog state, PassContext context, PixelFrame depth, Camera camera, FogAttenuation attenuation) {
        FieldFrame frame = Placement.Default.Frame(context.Extent);
        Func<(ERational X, ERational Y, ERational Z), ERational, float> noise = state.Noise.Sampler(state.Timing.At(context), CoordinateHash.Field(NoiseStream.Fog, state.Seed, 0u));
        Vector3 axis = Vector3.Transform(-Vector3.UnitZ, camera.Orientation);
        Func<long, long, float, float> density = state.Variation == Mix.MinValue ? static (_, _, _) => 1f
            : (column, line, path) => frame.Upward(column, line) switch { var point => float.Max(1f + (state.Variation * ((2f * noise((ERational.FromDouble(point.X), ERational.FromDouble(point.Y), ERational.Zero), ERational.FromSingle(path))) - 1f)), 0f) };
        Func<float, bool> beyond = state.Cutoff.Active.Map<Func<float, bool>>(static cutoff => distance => distance > cutoff).IfNone(static _ => false);
        return (column, line) => {
            (double X, double Y) point = (column + 0.5d, context.Extent.Height - line - 0.5d);
            float distance = depth.Sample(point, (WrapMode.Black, WrapMode.Black), float.PositiveInfinity,
                static (_, sample, _) => sample.X) / Vector3.Dot(camera.Ray(point).Direction, axis);
            float path = float.MaxNumber(distance - state.Start, 0f) / attenuation;
            return beyond(distance) ? 0f : float.IsPositiveInfinity(path) ? 1f : 1f - float.Exp(-path * density(column, line, path));
        };
    }

    private static Action<Span<Vector4>, long, long> Fill(Fog state, PassContext context, Func<long, long, float> opacity, Func<long, long, Vector3> light) {
        RampTable table = state.Colors.Tabulate(context.Working);
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                float coverage = opacity(column + i, line);
                Vector4 medium = table.Sample(coverage);
                row[i] = new Vector4(medium.AsVector3() * light(column + i, line), state.Amount * coverage * medium.W);
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FogParameter : IStateParameter<Fog> {
    private static readonly Presentation<float> Reach = new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Scale = TrackScale.Cubic };

    public static readonly FogParameter Noise = new("noise", new StateParameter<Fog>.Record<NoiseSource>(Lens<Fog, NoiseSource>.New(static fog => fog.Noise, static noise => fog => fog with { Noise = noise })));
    public static readonly FogParameter Variation = new("variation", StateParameter.Bounded<Fog, Mix, float, InvalidGrade>(Lens<Fog, Mix>.New(static fog => fog.Variation, static variation => fog => fog with { Variation = variation }), Tracks.Share));
    public static readonly FogParameter Start = new("start", StateParameter.Bounded<Fog, FogDistance, float, InvalidGenerator>(Lens<Fog, FogDistance>.New(static fog => fog.Start, static start => fog => fog with { Start = start }), Reach with { Soft = (0f, 100f) }));
    public static readonly FogParameter Attenuation = new("attenuation", StateParameter.OptionalBounded<Fog, FogAttenuation, float, InvalidGenerator>(Lens<Fog, Gated<FogAttenuation>>.New(static fog => fog.Attenuation, static attenuation => fog => fog with { Attenuation = attenuation }),
    new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0.3f, 1000f), Scale = TrackScale.Log }));
    public static readonly FogParameter Cutoff = new("cutoff", StateParameter.OptionalBounded<Fog, FogDistance, float, InvalidGenerator>(Lens<Fog, Gated<FogDistance>>.New(static fog => fog.Cutoff, static cutoff => fog => fog with { Cutoff = cutoff }), Reach with { Soft = (0f, 1000f) }));
    public static readonly FogParameter Diffusion = new("diffusion", StateParameter.OptionalBounded<Fog, ShortSideLength, float, InvalidPixelValue>(Lens<Fog, Gated<ShortSideLength>>.New(static fog => fog.Diffusion, static diffusion => fog => fog with { Diffusion = diffusion }), ShortSideLength.Presentation with { Soft = (0f, 2f) }));
    public static readonly FogParameter Amount = new("amount", StateParameter.Bounded<Fog, Mix, float, InvalidGrade>(Lens<Fog, Mix>.New(static fog => fog.Amount, static amount => fog => fog with { Amount = amount }), Tracks.Share));
    public static readonly FogParameter Colors = new("colors", new StateParameter<Fog>.Gradient(Lens<Fog, Ramp>.New(static fog => fog.Colors, static colors => fog => fog with { Colors = colors })));
    public static readonly FogParameter Layer = new("layer", new StateParameter<Fog>.Record<GeneratedLayer>(Lens<Fog, GeneratedLayer>.New(static fog => fog.Layer, static layer => fog => fog with { Layer = layer })));
    public static readonly FogParameter Seed = new("seed", StateParameter.Bounded<Fog, Seed, int, InvalidGenerator>(Lens<Fog, Seed>.New(static fog => fog.Seed, static seed => fog => fog with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FogParameter Timing = new("timing", new StateParameter<Fog>.Record<Timing>(Lens<Fog, Timing>.New(static fog => fog.Timing, static timing => fog => fog with { Timing = timing })));

    public StateParameter<Fog> Kind { get; }
}

public sealed record Starfield(
    Placement Placement, NoiseBasis Stars, ShortSideExtent StarScale, ShortSideExtent StarRadius, Ramp StarColors, Exposure StarExposure, Mix Twinkle, Frequency TwinkleRate,
    NoiseSource Nebula, AxisFraction NebulaDensity, Ramp NebulaColors, Exposure NebulaExposure, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Starfield, StarfieldParameter, InvalidGenerator>, IPixelStage<Starfield> {
    public static Starfield Default { get; } = new(
        Placement.Default, Looks.Fractal(2.56f, 0.165441f, 2.6f, 0f) with { Kind = BasisKind.SmoothF1, Cellular = new(CellMetric.Minkowski, MinkowskiExponent.Create(0.4f), AxisFraction.MaxValue) },
        Looks.Features(10f), Looks.Features(10f), Ramp.Grayscale, Exposure.Neutral, Mix.MinValue, Frequency.Neutral, Looks.Source(Looks.Fractal(6f, 0.56f, 2f, 0.1f), 1f, Vector2.One),
        AxisFraction.Half, Ramp.Grayscale, Exposure.Neutral, GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(Starfield state, PassContext context) => Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Stars.SiteSampler.Bind(stars => {
        (uint field, float time) = (CoordinateHash.Field(NoiseStream.Starfield, state.Seed, 0u), state.Timing.At(context));
        (uint glint, uint sites, uint clouds, uint draws) = (CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        (RampTable starTable, float gain, ERational scale, double reach) =
            (state.StarColors.Tabulate(context.Working), state.StarExposure.Scale, ERational.FromSingle(state.StarScale), (double)state.StarScale / state.StarRadius);
        Func<(ERational X, ERational Y, ERational Z, ERational W), float> steady = state.Twinkle == Mix.MinValue ? static _ => 1f
            : ((NoiseBasis.Plain with { Dimensions = NoiseDimensions.Four }).Sampler, ERational.FromSingle(state.TwinkleRate).Multiply(ERational.FromSingle(time)), (float)state.Twinkle) switch {
                var (plain, phase, depth) => site => 1f - (depth * plain((site.X, site.Y, site.Z, site.W.Add(phase)), glint).Value),
            };
        Func<(double X, double Y), Vector3> nebula = state.NebulaDensity == AxisFraction.MinValue ? static _ => Vector3.Zero
            : (state.Nebula.Sampler(time, clouds), state.NebulaColors.Tabulate(context.Working), state.NebulaExposure.Scale, (float)state.NebulaDensity) switch {
                var (sample, table, light, density) => point => sample((ERational.FromDouble(point.X), ERational.FromDouble(point.Y), ERational.Zero), ERational.Zero) switch {
                    var n => table.Sample(n) switch { var cloud => (float)RampInterpolation.SmootherRgb.Weight(Looks.Map(n, (1f - density, 1f))) * cloud.W * light * cloud.AsVector3() },
                },
            };
        return state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point => {
            NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)> cell = stars((ERational.FromDouble(point.X).Divide(scale), ERational.FromDouble(point.Y).Divide(scale), ERational.Zero, ERational.Zero), sites);
            Vector4 draw = NoiseFunctions.White(cell.Site, draws);
            return new Vector4((draw.Y * (float)double.Clamp(1d - (cell.Value * reach), 0d, 1d) * steady(cell.Site) * gain * starTable.Sample(draw.X).AsVector3()) + nebula(point), 1f);
        })).Run(frame, progress);
    })));
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StarfieldParameter : IStateParameter<Starfield> {
    public static readonly StarfieldParameter Placement = new("placement", new StateParameter<Starfield>.Record<Placement>(Lens<Starfield, Placement>.New(static sky => sky.Placement, static placement => sky => sky with { Placement = placement })));
    public static readonly StarfieldParameter Stars = new("stars", new StateParameter<Starfield>.Record<NoiseBasis>(Lens<Starfield, NoiseBasis>.New(static sky => sky.Stars, static stars => sky => sky with { Stars = stars })));
    public static readonly StarfieldParameter StarScale = new("star-scale", StateParameter.Bounded<Starfield, ShortSideExtent, float, InvalidPixelValue>(Lens<Starfield, ShortSideExtent>.New(static sky => sky.StarScale, static scale => sky => sky with { StarScale = scale }), Tracks.Extent with { Soft = (0.04f, 1f) }));
    public static readonly StarfieldParameter StarRadius = new("star-radius", StateParameter.Bounded<Starfield, ShortSideExtent, float, InvalidPixelValue>(Lens<Starfield, ShortSideExtent>.New(static sky => sky.StarRadius, static radius => sky => sky with { StarRadius = radius }), Tracks.Extent with { Soft = (0.01f, 1f) }));
    public static readonly StarfieldParameter StarColors = new("star-colors", new StateParameter<Starfield>.Gradient(
        Lens<Starfield, Ramp>.New(static sky => sky.StarColors, static colors => sky => sky with { StarColors = colors })));
    public static readonly StarfieldParameter StarExposure = new("star-exposure", StateParameter.Bounded<Starfield, Exposure, float, InvalidToneValue>(Lens<Starfield, Exposure>.New(static sky => sky.StarExposure, static exposure => sky => sky with { StarExposure = exposure }), Exposure.Presentation));
    public static readonly StarfieldParameter Twinkle = new("twinkle", StateParameter.Bounded<Starfield, Mix, float, InvalidGrade>(Lens<Starfield, Mix>.New(static sky => sky.Twinkle, static twinkle => sky => sky with { Twinkle = twinkle }), Tracks.Share));
    public static readonly StarfieldParameter TwinkleRate = new("twinkle-rate", StateParameter.Bounded<Starfield, Frequency, float, InvalidGenerator>(Lens<Starfield, Frequency>.New(static sky => sky.TwinkleRate, static rate => sky => sky with { TwinkleRate = rate }), Tracks.Rate));
    public static readonly StarfieldParameter Nebula = new("nebula", new StateParameter<Starfield>.Record<NoiseSource>(Lens<Starfield, NoiseSource>.New(static sky => sky.Nebula, static nebula => sky => sky with { Nebula = nebula })));
    public static readonly StarfieldParameter NebulaDensity = new("nebula-density", StateParameter.Bounded<Starfield, AxisFraction, float, InvalidGrade>(Lens<Starfield, AxisFraction>.New(static sky => sky.NebulaDensity, static density => sky => sky with { NebulaDensity = density }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly StarfieldParameter NebulaColors = new("nebula-colors", new StateParameter<Starfield>.Gradient(
        Lens<Starfield, Ramp>.New(static sky => sky.NebulaColors, static colors => sky => sky with { NebulaColors = colors })));
    public static readonly StarfieldParameter NebulaExposure = new("nebula-exposure", StateParameter.Bounded<Starfield, Exposure, float, InvalidToneValue>(Lens<Starfield, Exposure>.New(static sky => sky.NebulaExposure, static exposure => sky => sky with { NebulaExposure = exposure }), Exposure.Presentation));
    public static readonly StarfieldParameter Layer = new("layer", new StateParameter<Starfield>.Record<GeneratedLayer>(Lens<Starfield, GeneratedLayer>.New(static sky => sky.Layer, static layer => sky => sky with { Layer = layer })));
    public static readonly StarfieldParameter Seed = new("seed", StateParameter.Bounded<Starfield, Seed, int, InvalidGenerator>(Lens<Starfield, Seed>.New(static sky => sky.Seed, static seed => sky => sky with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly StarfieldParameter Timing = new("timing", new StateParameter<Starfield>.Record<Timing>(Lens<Starfield, Timing>.New(static sky => sky.Timing, static timing => sky => sky with { Timing = timing })));

    public StateParameter<Starfield> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Looks {
    public static ShortSideExtent Features(float scale) => ShortSideExtent.Create(1f / scale);

    public static NoiseSource Source(NoiseBasis basis, float scale, Vector2 stretch) =>
        new(basis, Features(scale / stretch.X), Features(scale / stretch.Y), Drift.Neutral, Drift.Neutral, Frequency.Neutral);

    public static NoiseBasis Fractal(float detail, float roughness, float lacunarity, float distortion) =>
        NoiseBasis.Default with { Octaves = new(FractalDetail.Create(detail), AxisFraction.Create(roughness), FractalLacunarity.Create(lacunarity)), Distortion = NoiseDistortion.Create(distortion) };

    public static float Map(float value, (float Low, float High) edges) => Easing.Saturate((value - edges.Low) / (edges.High - edges.Low));

    public static Ramp Sampled(RampInterpolation interpolation, Func<float, Vector4> source, params ReadOnlySpan<float> positions) =>
        Ramp.Create(toSeq(positions.ToArray()).Map(position => RampStop.Create(RampPosition.Create(position), source(position))), interpolation);

    public static Ramp Threshold(RampInterpolation interpolation, Vector3 light, float low, float high) =>
        Sampled(interpolation, value => new Vector4(light, Map(value, (low, high))), low, high);
}
