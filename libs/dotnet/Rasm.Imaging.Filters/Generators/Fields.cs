using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Generators;

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

public sealed record GeneratedLayer(BlendingMode Mode, Exposure Exposure) : IStateRecord<GeneratedLayer, GeneratedLayerParameter, InvalidGenerator> {
    public static GeneratedLayer Default { get; } = new(BlendingMode.Mix, Exposure.Neutral);
    public static GeneratedLayer Added { get; } = new(BlendingMode.Add, Exposure.Neutral);

    internal Action<Span<Vector4>, int, int> Kernel(Action<Span<Vector4>, int, int> fill) {
        (float gain, BlendingMode mode) = (Exposure.Scale, Mode);
        return (row, column, line) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
            fill(layer.Span, column, line);
            for (int i = 0; i < row.Length; i++)
                (weights.Span[i], layer.Span[i]) = (layer.Span[i].W, new Vector4(layer.Span[i].AsVector3() * gain, 1f));
            if (mode == BlendingMode.Mix)
                mode.Composite(row, layer.Span, weights.Span);
            else {
                mode.Mixed(row, layer.Span, weights.Span);
                layer.Span.CopyTo(row);
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeneratedLayerParameter : IStateParameter<GeneratedLayer> {
    public static readonly GeneratedLayerParameter Mode = new("mode", new StateParameter<GeneratedLayer>.Choice<BlendingMode, InvalidGrade>(
        Lens<GeneratedLayer, BlendingMode>.New(static layer => layer.Mode, static mode => layer => layer with { Mode = mode })));
    public static readonly GeneratedLayerParameter Exposure = new("exposure", new StateParameter<GeneratedLayer>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<GeneratedLayer, Exposure>.New(static layer => layer.Exposure, static exposure => layer => layer with { Exposure = exposure }), Tone.Exposure.Presentation));
    public StateParameter<GeneratedLayer> Kind { get; }
}

public sealed record Placement(FramePosition Center, SignedAngle Rotation) : IStateRecord<Placement, PlacementParameter, InvalidGenerator> {
    public static Placement Default { get; } = new(FramePosition.Center, SignedAngle.Neutral);

    internal FieldFrame Frame(PixelExtent extent) =>
        (Half: new Vector2(extent.Width / 2f, extent.Height / 2f), Scale: 1f / extent.ShortSide) switch {
            var (half, scale) => new(half, scale, (Center.Point(extent) - half) * scale, Matrix3x2.CreateRotation(Rotation)),
        };
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
    public static readonly PlacementParameter Rotation = new("rotation", new StateParameter<Placement>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Placement, SignedAngle>.New(static placement => placement.Rotation, static rotation => placement => placement with { Rotation = rotation }),
        new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral }));
    public StateParameter<Placement> Kind { get; }
}

internal readonly record struct FieldFrame(Vector2 Half, float Scale, Vector2 Origin, Matrix3x2 Turn) {
    public Vector2 Point(int column, int line) => new Vector2(column + 0.5f - Half.X, Half.Y - line - 0.5f) * Scale;

    public Vector2 Local(Vector2 q) => Vector2.Transform(q - Origin, Turn);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Rings", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct VortexArms : IMinMaxValue<VortexArms> {
    public static VortexArms MinValue => Rings;
    public static VortexArms MaxValue { get; } = new(32);
    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

public sealed record VortexField(VortexArms Arms, SignedAngle Twist, ShortSideExtent Radius, TunnelSlope Tunnel, Spin Spin)
    : IStateRecord<VortexField, VortexFieldParameter, InvalidGenerator> {
    public static VortexField Default { get; } = new(VortexArms.Create(1), SignedAngle.Create(2.8f), ShortSideExtent.Create(0.5f), TunnelSlope.Create(0.6f), Spin.Create(1f));
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VortexFieldParameter : IStateParameter<VortexField> {
    public static readonly VortexFieldParameter Arms = new("arms", new StateParameter<VortexField>.Bounded<VortexArms, int, InvalidGenerator>(
        Lens<VortexField, VortexArms>.New(static state => state.Arms, static value => state => state with { Arms = value }), new() { Step = 1 }));
    public static readonly VortexFieldParameter Twist = new("twist", new StateParameter<VortexField>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<VortexField, SignedAngle>.New(static state => state.Twist, static value => state => state with { Twist = value }), new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) }));
    public static readonly VortexFieldParameter Radius = new("radius", new StateParameter<VortexField>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<VortexField, ShortSideExtent>.New(static state => state.Radius, static value => state => state with { Radius = value }), new() { Soft = (0.01f, 2f), Scale = TrackScale.Log }));
    public static readonly VortexFieldParameter Tunnel = new("tunnel", new StateParameter<VortexField>.Bounded<TunnelSlope, float, InvalidGenerator>(
        Lens<VortexField, TunnelSlope>.New(static state => state.Tunnel, static value => state => state with { Tunnel = value }), new() { Soft = (0f, 4f) }));
    public static readonly VortexFieldParameter Spin = new("spin", new StateParameter<VortexField>.Bounded<Spin, float, InvalidGenerator>(
        Lens<VortexField, Spin>.New(static state => state.Spin, static value => state => state with { Spin = value }), Generators.Spin.Presentation));
    public StateParameter<VortexField> Kind { get; }
}

public sealed record FieldEnvelope(ShortSideExtent Width, ShortSideExtent Height) : IStateRecord<FieldEnvelope, FieldEnvelopeParameter, InvalidGenerator> {
    public static FieldEnvelope Default { get; } = new(Looks.ShortSide, Looks.ShortSide);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldEnvelopeParameter : IStateParameter<FieldEnvelope> {
    public static readonly FieldEnvelopeParameter Width = new("width", new StateParameter<FieldEnvelope>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<FieldEnvelope, ShortSideExtent>.New(static state => state.Width, static value => state => state with { Width = value }), new() { Soft = (0.1f, 4f), Scale = TrackScale.Log }));
    public static readonly FieldEnvelopeParameter Height = new("height", new StateParameter<FieldEnvelope>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<FieldEnvelope, ShortSideExtent>.New(static state => state.Height, static value => state => state with { Height = value }), new() { Soft = (0.1f, 4f), Scale = TrackScale.Log }));
    public StateParameter<FieldEnvelope> Kind { get; }
}

public sealed record ThermalRange(ColorTemperature Cool, ColorTemperature Hot) : IStateRecord<ThermalRange, ThermalRangeParameter, InvalidGenerator> {
    public static ThermalRange Default { get; } = new(ColorTemperature.MinValue, ColorTemperature.Flame);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThermalRangeParameter : IStateParameter<ThermalRange> {
    public static readonly ThermalRangeParameter Cool = new("cool", new StateParameter<ThermalRange>.Bounded<ColorTemperature, double, InvalidColor>(
        Lens<ThermalRange, ColorTemperature>.New(static state => state.Cool, static value => state => state with { Cool = value }), Emission.Presentation));
    public static readonly ThermalRangeParameter Hot = new("hot", new StateParameter<ThermalRange>.Bounded<ColorTemperature, double, InvalidColor>(
        Lens<ThermalRange, ColorTemperature>.New(static state => state.Hot, static value => state => state with { Hot = value }), Emission.Presentation));
    public StateParameter<ThermalRange> Kind { get; }
}

public sealed record NoiseTexture(
    NoiseBasis Basis, ShortSideExtent ScaleX, ShortSideExtent ScaleY, ShortSideExtent ScaleZ, Placement Placement, Option<VortexField> VortexShape,
    Drift DriftX, Drift DriftY, Frequency Evolution, ShortSideOffset Slice,
    ShortSideOffset Warp, ShortSideExtent WarpScale, NoiseBasis WarpBasis, SignedAngle WarpRotation, Drift WarpDrift,
    CellularNoise CellWarpBasis, ShortSideExtent CellWarpScale, Mix CellWarp,
    Ramp Colors, Levels ColorLevels, GammaExponent ColorPower, Option<Ramp> Coverage,
    Option<FieldEnvelope> Envelope, Option<ThermalRange> Incandescence, Option<OpticalDepth> Extinction,
    GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<NoiseTexture, NoiseTextureParameter, InvalidGenerator>, IPixelStage<NoiseTexture> {
    public static NoiseTexture Default { get; } = ShortSideExtent.Create(1f / 5f) switch {
        var features => new(
            NoiseBasis.Default, features, features, features, Placement.Default, None, Drift.Neutral, Drift.Neutral,
            Frequency.Neutral, ShortSideOffset.Neutral, ShortSideOffset.Neutral, Looks.Wavelength(1.3f),
            NoiseBasis.Default with {
                Kind = BasisKind.Wave,
                Octaves = Octaves.Default with { Roughness = AxisFraction.Create(0.186957f) },
                Distortion = NoiseDistortion.Create(14.9f),
                WaveForm = Some(WaveForm.Bands),
                WaveProfile = Some(WaveProfile.Sine),
                WavePhase = Some(SignedAngle.Create(-3f)),
                WaveDetailScale = Some(WaveDetailScale.Create(1.3f)),
            },
            SignedAngle.Neutral, Drift.Neutral, CellularNoise.Default, Looks.ShortSide, Mix.MinValue,
            Ramp.Grayscale, Levels.Default, GammaExponent.Create(1f), None, None, None, None, GeneratedLayer.Default, Seed.MinValue, Timing.Standard),
    };

    public static NoiseTexture TurbulentNoise { get; } = Default with { Evolution = Frequency.Create(0.3f) };

    public static NoiseTexture BlockNoise { get; } = Default with { Basis = NoiseBasis.Default with { Kind = BasisKind.White } };

    public static NoiseTexture Diamonds { get; } = Default with {
        Basis = NoiseBasis.Default with { Kind = BasisKind.Cellular, Feature = CellFeatureKind.F2, Octaves = Octaves.Plain, Cellular = Cellular.Default with { Randomness = AxisFraction.MinValue } },
    };

    public static NoiseTexture VoronoiNoise { get; } = TurbulentNoise with {
        Basis = NoiseBasis.Default with {
            Kind = BasisKind.Cellular,
            Feature = CellFeatureKind.F1,
            Octaves = Octaves.Default with { Detail = FractalDetail.Create(1f) },
        },
    };

    public static NoiseTexture Sparkle { get; } = TurbulentNoise with {
        Basis = VoronoiNoise.Basis with {
            Kind = BasisKind.Cellular,
            Feature = CellFeatureKind.SmoothF1,
            Cellular = Cellular.Default with { Metric = CellMetric.Minkowski, Exponent = MinkowskiExponent.Create(1f) },
            CellSmoothness = Some(AxisFraction.MaxValue),
        },
    };

    public static NoiseTexture VoronoiCells { get; } = Default with {
        Basis = NoiseBasis.Default with { Kind = BasisKind.EdgeDistance, Octaves = Octaves.Plain },
        DriftY = Drift.Create(-0.6f),
    };

    public static NoiseTexture Waves { get; } = Default with {
        Basis = NoiseBasis.Default with {
            Kind = BasisKind.Wave,
            Distortion = NoiseDistortion.Create(4f),
            WaveForm = Some(WaveForm.Bands),
            WaveProfile = Some(WaveProfile.Sine),
            WaveDetailScale = Some(WaveDetailScale.Create(2f)),
        },
        ScaleX = Looks.Wavelength(1f),
        ScaleY = Looks.Wavelength(1f),
        Evolution = Frequency.Create(-0.3f / float.Tau),
    };

    public static NoiseTexture LightLeak { get; } = Default with {
        Basis = NoiseBasis.Plain,
        ScaleX = ShortSideExtent.Create(1f / 0.7f),
        ScaleY = ShortSideExtent.Create(1f / 0.7f),
        ScaleZ = ShortSideExtent.Create(1f / 0.7f),
        Evolution = Frequency.Create(0.5f),
        Layer = GeneratedLayer.Added,
        Colors = Looks.Ramp(RampInterpolation.LinearRgb, (0.225d, new Vector3(1f, 0.7529f, 0.6584f), 1f), (0.775d, new Vector3(0.0437f, 0.0116f, 1f), 1f)),
        Coverage = Some(Looks.Ramp(RampInterpolation.LinearRgb, (0.1d, Vector3.One, 1f), (0.6d, Vector3.One, 0f))),
    };

    public static NoiseTexture Flow { get; } = Default with {
        Basis = NoiseBasis.Default with {
            Dimensions = NoiseDimensions.Four,
            Octaves = new(FractalDetail.Create(2.48f), AxisFraction.MinValue, FractalLacunarity.Create(12.4f)),
            Distortion = NoiseDistortion.Create(7.25f),
        },
        ScaleX = Looks.ShortSide,
        ScaleY = Looks.ShortSide,
        ScaleZ = Looks.ShortSide,
        Evolution = Frequency.Create(0.15f),
        Warp = ShortSideOffset.Create(0.0566038f),
        Layer = GeneratedLayer.Added,
        ColorLevels = new(BlackLevel.Create(0.62f), Exposure.Create(float.Log2(1.1f - 0.62f))),
        Colors = Looks.Ramp(RampInterpolation.SmootherRgb, (0d, new Vector3(0.305f, 0.129f, 1f), 1f), (1d, new Vector3(1f, 0.059f, 0.298f), 1f)),
        Coverage = Some(Looks.Ramp(RampInterpolation.LinearRgb, (0.37d, Vector3.One, 0f), (0.87d, Vector3.One, 1f))),
    };

    public static NoiseTexture Vortex { get; } = Default with {
        Basis = NoiseBasis.Default with {
            Octaves = new(FractalDetail.Create(4f), AxisFraction.Create(0.2f), FractalLacunarity.Create(3f)),
            Distortion = NoiseDistortion.Create(0.5f),
        },
        ScaleX = ShortSideExtent.Create(1f / 6f),
        ScaleY = ShortSideExtent.Create(1f / 6f),
        ScaleZ = ShortSideExtent.Create(1f / 6f),
        VortexShape = Some(VortexField.Default),
        Evolution = Frequency.Create(0.3f),
        ColorPower = GammaExponent.Create(4f),
        Colors = Looks.Ramp(RampInterpolation.LinearRgb, (0d, Vector3.Zero, 1f), (1d, (0.8f * 2f * new Vector3(0.134f, 0.431f, 0.5f)) + new Vector3(0.2f), 1f)),
    };

    public static NoiseTexture Fire { get; } = Default with {
        Basis = NoiseBasis.Default with { Octaves = Octaves.Default with { Detail = FractalDetail.Create(4f) }, Distortion = NoiseDistortion.Create(1f) },
        ScaleX = ShortSideExtent.Create(1f / (0.7f * 2f)),
        ScaleY = ShortSideExtent.Create(1f / 0.7f),
        ScaleZ = ShortSideExtent.Create(1f / 0.7f),
        Placement = new(new FramePosition(FrameAxis.Middle, FrameAxis.End), SignedAngle.Neutral),
        DriftY = Drift.Rise,
        Slice = ShortSideOffset.Create(2.8f),
        Layer = GeneratedLayer.Added,
        Warp = ShortSideOffset.Create(0.1f),
        WarpScale = Looks.Wavelength(0.5f),
        WarpRotation = SignedAngle.Create(-float.Pi / 2f),
        WarpDrift = Drift.Create(2f),
        WarpBasis = Default.WarpBasis with {
            Octaves = Octaves.Default with { Roughness = AxisFraction.MinValue },
            Distortion = NoiseDistortion.Create(1f),
            WavePhase = Some(SignedAngle.Neutral),
            WaveDetailScale = Some(WaveDetailScale.Create(14.9f)),
        },
        CellWarp = Mix.Create(0.5f),
        CellWarpScale = ShortSideExtent.Create(1f / 3f),
        CellWarpBasis = CellularNoise.Default with { Octaves = new(FractalDetail.Create(4f), AxisFraction.Create(0.680498f), FractalLacunarity.Create(1.7f)) },
        Envelope = Some(FieldEnvelope.Default),
        Incandescence = Some(ThermalRange.Default),
        Colors = Looks.Ramp(RampInterpolation.LinearRgb, (0.17d, Vector3.Zero, 0f), (1d, Vector3.One, 1f)),
    };

    public static NoiseTexture Smoke { get; } = Default with {
        Basis = NoiseBasis.Default with {
            Octaves = Octaves.Default with {
                Detail = FractalDetail.Create(5f),
                Roughness = AxisFraction.Create(0.55f),
            },
            Distortion = NoiseDistortion.Create(0.5f),
        },
        ScaleX = ShortSideExtent.Create(0.2f * ReferenceFrame.GreaterSide),
        ScaleY = ShortSideExtent.Create(1.5f * 0.2f * ReferenceFrame.GreaterSide),
        DriftY = Drift.Create(0.05f * ReferenceFrame.GreaterSide),
        Evolution = Frequency.Create(0.3f),
        Colors = Looks.Ramp(RampInterpolation.Ease, (0.4d, new Vector3(0.04f), 0f), (0.8d, new Vector3(0.04f), 1f)),
        Extinction = Some(OpticalDepth.Create(2f)),
    };

    public static NoiseTexture Mist { get; } = Default with {
        Basis = NoiseBasis.Default with {
            Octaves = Octaves.Default with {
                Detail = FractalDetail.Create(3f),
                Roughness = AxisFraction.Create(0.45f),
            },
        },
        ScaleX = ShortSideExtent.Create(0.5f * ReferenceFrame.GreaterSide),
        ScaleY = ShortSideExtent.Create(0.5f * 0.5f * ReferenceFrame.GreaterSide),
        DriftX = Drift.Create(0.02f * ReferenceFrame.GreaterSide),
        Evolution = Frequency.Create(0.1f),
        Colors = Looks.Ramp(RampInterpolation.Ease, (0.3d, new Vector3(0.7f), 0f), (0.9d, new Vector3(0.7f), 1f)),
        Extinction = Some(OpticalDepth.Create(0.6f)),
    };

    public static Option<PixelPass> Pass(NoiseTexture state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context))));

    private static Action<Span<Vector4>, int, int> Fill(NoiseTexture state, PassContext context) {
        FieldFrame frame = state.Placement.Frame(context.Extent);
        uint field = CoordinateHash.Field(NoiseStream.NoiseTexture, state.Seed, 0u);
        float time = state.Timing.At(context);
        (Func<Vector4, uint, float> basis, Func<Vector4, uint, float> warp) = (state.Basis.Sampler, state.WarpBasis.Sampler);
        (RampTable colors, Option<RampTable> coverage) = (state.Colors.Tabulate(context.Working), state.Coverage.Map(ramp => ramp.Tabulate(context.Working)));
        (Vector3 scale, Vector2 drift) = (new(state.ScaleX, state.ScaleY, state.ScaleZ), new Vector2(state.DriftX, state.DriftY) * time);
        Vector4 evolution = Vector4.UnitZ * state.Evolution * time;
        Matrix3x2 warpRotation = Matrix3x2.CreateRotation(state.WarpRotation);
        Func<Vector2, Vector3> position = state.VortexShape.Match<Func<Vector2, Vector3>>(
            Some: vortex => p => (p.Length(), float.Atan2(p.Y, p.X)) switch {
                var (radius, angle) => float.SinCos((vortex.Arms * angle) + (vortex.Twist * (1f - (radius / vortex.Radius))) + (vortex.Spin * time)) switch {
                    var (sin, cos) => float.Min(radius, vortex.Radius) * new Vector3(cos, sin, vortex.Tunnel),
                },
            },
            None: static () => static p => new Vector3(p, 0f));
        Func<Vector2, float> envelope = state.Envelope.Match<Func<Vector2, float>>(
            Some: static size => p => float.Max(1f - (p / new Vector2(size.Width, size.Height)).Length(), 0f) switch { var edge => 1f - (edge * edge) },
            None: static () => static _ => 0f);
        Option<Func<float, Vector3>> thermal = state.Incandescence.Map(range => Emission.Span((context.Working, range.Cool, range.Hot)));
        Func<float, float> opacity = state.Extinction.Match<Func<float, float>>(
            Some: static extinction => density => 1f - float.Exp(-extinction * density),
            None: static () => static density => density);
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 local = frame.Local(frame.Point(column + i, line));
                Vector2 uv = new(local.X, -local.Y);
                Vector3 point = position(uv) + new Vector3(-drift, state.Slice);
                Vector2 wavePoint = Vector2.Transform(uv - new Vector2(0f, state.WarpDrift * time), warpRotation);
                Vector3 scalarWarp = new(2f * state.Warp * (warp(new Vector4(wavePoint / state.WarpScale, 0f, 0f), CoordinateHash.Branch(field, 1u)) - 0.5f));
                Vector3 cellWarp = state.CellWarp == Mix.MinValue ? Vector3.Zero
                    : 2f * state.CellWarp * ((state.CellWarpBasis.Feature.Sample(state.CellWarpBasis, new Vector4(point / state.CellWarpScale, 0f), CoordinateHash.Branch(field, 2u)).Site.AsVector3() * state.CellWarpScale) - new Vector3(0.5f));
                float value = basis(new Vector4((point + scalarWarp + cellWarp) / scale, 0f) + evolution, field) - envelope(uv);
                Vector4 color = colors.Sample((value - state.ColorLevels.Black) / state.ColorLevels.White.Scale);
                float density = coverage.Match(table => table.Sample(value).W, () => color.W);
                Vector3 light = thermal.Match(spectrum => spectrum(density), () => Vector3.Exp(state.ColorPower * Vector3.Log(color.AsVector3())));
                row[i] = new Vector4(light, opacity(density));
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseTextureParameter : IStateParameter<NoiseTexture> {
    private static readonly (StateParameter<NoiseTexture> Clock, StateParameter<NoiseTexture> Pace) Time =
        Timing.Kinds(Lens<NoiseTexture, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly NoiseTextureParameter Basis = new("basis", new StateParameter<NoiseTexture>.Record<NoiseBasis>(Lens<NoiseTexture, NoiseBasis>.New(static texture => texture.Basis, static basis => texture => texture with { Basis = basis })));
    public static readonly NoiseTextureParameter WarpBasis = new("warp-basis", new StateParameter<NoiseTexture>.Record<NoiseBasis>(Lens<NoiseTexture, NoiseBasis>.New(static texture => texture.WarpBasis, static basis => texture => texture with { WarpBasis = basis })));
    public static readonly NoiseTextureParameter Placement = new("placement", new StateParameter<NoiseTexture>.Record<Placement>(Lens<NoiseTexture, Placement>.New(static texture => texture.Placement, static placement => texture => texture with { Placement = placement })));
    public static readonly NoiseTextureParameter Layer = new("layer", new StateParameter<NoiseTexture>.Record<GeneratedLayer>(Lens<NoiseTexture, GeneratedLayer>.New(static texture => texture.Layer, static layer => texture => texture with { Layer = layer })));
    private static readonly Presentation<ShortSideExtent, float> Extent = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log };
    private static readonly Presentation<Drift, float> Travel = Drift.Presentation with { Soft = (-1f, 1f) };

    public static readonly NoiseTextureParameter ScaleX = new("scale-x", new StateParameter<NoiseTexture>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideExtent>.New(static texture => texture.ScaleX, static scale => texture => texture with { ScaleX = scale }), Extent));
    public static readonly NoiseTextureParameter ScaleY = new("scale-y", new StateParameter<NoiseTexture>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideExtent>.New(static texture => texture.ScaleY, static scale => texture => texture with { ScaleY = scale }), Extent));
    public static readonly NoiseTextureParameter Vortex = new("vortex", new StateParameter<NoiseTexture>.OptionalRecord<VortexField>(
        Lens<NoiseTexture, Option<VortexField>>.New(static state => state.VortexShape, static value => state => state with { VortexShape = value })));
    public static readonly NoiseTextureParameter DriftX = new("drift-x", new StateParameter<NoiseTexture>.Bounded<Drift, float, InvalidGenerator>(
        Lens<NoiseTexture, Drift>.New(static texture => texture.DriftX, static drift => texture => texture with { DriftX = drift }), Travel));
    public static readonly NoiseTextureParameter DriftY = new("drift-y", new StateParameter<NoiseTexture>.Bounded<Drift, float, InvalidGenerator>(
        Lens<NoiseTexture, Drift>.New(static texture => texture.DriftY, static drift => texture => texture with { DriftY = drift }), Travel));
    public static readonly NoiseTextureParameter Evolution = new("evolution", new StateParameter<NoiseTexture>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<NoiseTexture, Frequency>.New(static texture => texture.Evolution, static evolution => texture => texture with { Evolution = evolution }),
        Frequency.Presentation with { Soft = (-4f, 4f) }));
    public static readonly NoiseTextureParameter Warp = new("warp", new StateParameter<NoiseTexture>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideOffset>.New(static texture => texture.Warp, static warp => texture => texture with { Warp = warp }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (-1f, 1f), Origin = 0f }));
    public static readonly NoiseTextureParameter WarpScale = new("warp-scale", new StateParameter<NoiseTexture>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideExtent>.New(static texture => texture.WarpScale, static scale => texture => texture with { WarpScale = scale }), Extent));
    public static readonly NoiseTextureParameter Colors = new("colors", new StateParameter<NoiseTexture>.Gradient(
        Lens<NoiseTexture, Ramp>.New(static texture => texture.Colors, static colors => texture => texture with { Colors = colors })));
    public static readonly NoiseTextureParameter ColorLevels = new("color-levels", new StateParameter<NoiseTexture>.Record<Levels>(
        Lens<NoiseTexture, Levels>.New(static state => state.ColorLevels, static value => state => state with { ColorLevels = value })));
    public static readonly NoiseTextureParameter ColorPower = new("color-power", new StateParameter<NoiseTexture>.Bounded<GammaExponent, float, InvalidColor>(
        Lens<NoiseTexture, GammaExponent>.New(static state => state.ColorPower, static value => state => state with { ColorPower = value }), new() { Soft = (0.001f, 10f) }));
    public static readonly NoiseTextureParameter Coverage = new("coverage", new StateParameter<NoiseTexture>.OptionalKeyed<Ramp, string, InvalidPixelValue>(
        Lens<NoiseTexture, Option<Ramp>>.New(static state => state.Coverage, static value => state => state with { Coverage = value })));
    public static readonly NoiseTextureParameter Envelope = new("envelope", new StateParameter<NoiseTexture>.OptionalRecord<FieldEnvelope>(
        Lens<NoiseTexture, Option<FieldEnvelope>>.New(static state => state.Envelope, static value => state => state with { Envelope = value })));
    public static readonly NoiseTextureParameter Incandescence = new("incandescence", new StateParameter<NoiseTexture>.OptionalRecord<ThermalRange>(
        Lens<NoiseTexture, Option<ThermalRange>>.New(static state => state.Incandescence, static value => state => state with { Incandescence = value })));
    public static readonly NoiseTextureParameter CellWarpBasis = new("cell-warp-basis", new StateParameter<NoiseTexture>.Record<CellularNoise>(
        Lens<NoiseTexture, CellularNoise>.New(static state => state.CellWarpBasis, static value => state => state with { CellWarpBasis = value })));
    public static readonly NoiseTextureParameter CellWarpScale = new("cell-warp-scale", new StateParameter<NoiseTexture>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideExtent>.New(static state => state.CellWarpScale, static value => state => state with { CellWarpScale = value }), Extent));
    public static readonly NoiseTextureParameter CellWarp = new("cell-warp", new StateParameter<NoiseTexture>.Bounded<Mix, float, InvalidGrade>(
        Lens<NoiseTexture, Mix>.New(static state => state.CellWarp, static value => state => state with { CellWarp = value }), new()));
    public static readonly NoiseTextureParameter WarpRotation = new("warp-rotation", new StateParameter<NoiseTexture>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<NoiseTexture, SignedAngle>.New(static state => state.WarpRotation, static value => state => state with { WarpRotation = value }), new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) }));
    public static readonly NoiseTextureParameter WarpDrift = new("warp-drift", new StateParameter<NoiseTexture>.Bounded<Drift, float, InvalidGenerator>(
        Lens<NoiseTexture, Drift>.New(static state => state.WarpDrift, static value => state => state with { WarpDrift = value }), Travel));
    public static readonly NoiseTextureParameter Slice = new("slice", new StateParameter<NoiseTexture>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideOffset>.New(static state => state.Slice, static value => state => state with { Slice = value }), new()));
    public static readonly NoiseTextureParameter ScaleZ = new("scale-z", new StateParameter<NoiseTexture>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideExtent>.New(static state => state.ScaleZ, static value => state => state with { ScaleZ = value }), Extent));
    public static readonly NoiseTextureParameter Extinction = new("extinction", new StateParameter<NoiseTexture>.OptionalBounded<OpticalDepth, float, InvalidGenerator>(
        Lens<NoiseTexture, Option<OpticalDepth>>.New(static texture => texture.Extinction, static extinction => texture => texture with { Extinction = extinction }),
        new() { Soft = (0f, 8f) }));
    public static readonly NoiseTextureParameter Seed = new("seed", new StateParameter<NoiseTexture>.Bounded<Seed, int, InvalidGenerator>(
        Lens<NoiseTexture, Seed>.New(static texture => texture.Seed, static seed => texture => texture with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly NoiseTextureParameter Clock = new("clock", Time.Clock);
    public static readonly NoiseTextureParameter Pace = new("pace", Time.Pace);

    public StateParameter<NoiseTexture> Kind { get; }
}

public sealed record Fog(
    NoiseBasis Basis, ShortSideExtent Scale, Drift DriftX, Drift DriftY, Frequency Evolution, Mix Variation,
    FogDistance Start, Option<FogAttenuation> Attenuation, Option<FogDistance> Cutoff, Option<ShortSideLength> Diffusion,
    Mix Amount, Ramp Colors, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Fog, FogParameter, InvalidGenerator>, IPixelStage<Fog> {
    public static Fog Default { get; } = new(
        NoiseBasis.Default with { Dimensions = NoiseDimensions.Four, Octaves = Octaves.Default with { Detail = FractalDetail.Create(4f), Roughness = AxisFraction.Create(0.2f) } },
        Looks.ShortSide, Drift.Neutral, Drift.Neutral, Frequency.Neutral, Mix.MinValue,
        FogDistance.Neutral, Some(FogAttenuation.Standard), None, None, Mix.MaxValue,
        Ramp.Create(Seq(RampStop.Create(RampPosition.MinValue, new Vector4(new Vector3(MathF.Pow(0.85098f, 2.2f)), 1f))), RampInterpolation.Linear), GeneratedLayer.Default, Seed.MinValue, Timing.Standard);

    public static Fog Haze { get; } = Default with {
        Attenuation = None,
        Amount = Mix.Create(0.35f),
        Diffusion = Some(ShortSideLength.Create(0.75f * ReferenceFrame.GreaterSide)),
        Colors = Ramp.Create(Seq(RampStop.Create(RampPosition.MinValue, new Vector4(new Vector3(0.18f), 1f))), RampInterpolation.Linear),
    };

    public static Seq<GuideChannel> Channels(Fog state) => state.Attenuation.Match(static _ => Seq(GuideChannel.Depth), static () => Seq<GuideChannel>());

    public static Option<PixelPass> Pass(Fog state, PassContext context) =>
        state.Attenuation.Match(
            Some: attenuation => from depth in context.Guides.Find(GuideChannel.Depth)
                                 from camera in context.Camera
                                 select Extinction(state, context, depth, camera, attenuation),
            None: static () => Some<Func<int, int, float>>(static (_, _) => 1f))
        .Map(opacity => state.Diffusion.Match<PixelPass>(
            Some: radius => new PixelPass.Frame((frame, progress) => {
                float pixels = radius.Pixels(context.Extent);
                int taps = (2 * (int)float.Ceiling(pixels)) + 1;
                PixelFrame spread = new(frame.Origin, frame.Size, frame.Extent, static _ => { });
                using Mat source = frame.Header();
                using Mat target = spread.Header();
                CvInvoke.GaussianBlur(source, target, new Size(taps, taps), pixels / 3f, pixels / 3f, BorderType.Replicate);
                return new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context, opacity, (column, line) => spread.Row(line)[column - spread.Origin.X].AsVector3())))
                    .Run(frame, progress);
            }),
            None: () => new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context, opacity, static (_, _) => Vector3.One)))));

    private static Func<int, int, float> Extinction(Fog state, PassContext context, PixelFrame depth, Camera camera, FogAttenuation attenuation) {
        FieldFrame frame = Placement.Default.Frame(context.Extent);
        uint field = CoordinateHash.Field(NoiseStream.Fog, state.Seed, 0u);
        float time = state.Timing.At(context);
        Func<Vector4, uint, float> basis = state.Basis.Sampler;
        Vector3 axis = Vector3.Transform(-Vector3.UnitZ, camera.Orientation);
        Vector2 drift = new Vector2(state.DriftX, state.DriftY) * time;
        float evolution = state.Evolution * time;
        Func<Vector2, float, float> density = state.Variation == Mix.MinValue ? static (_, _) => 1f
            : (point, path) => float.Max(1f + (state.Variation * ((2f * basis(new Vector4((new Vector2(point.X, -point.Y) - drift) / state.Scale, evolution, path), field)) - 1f)), 0f);
        Func<float, bool> beyond = state.Cutoff.Match(
            Some: static cutoff => (Func<float, bool>)(distance => distance > cutoff), None: static () => static _ => false);
        return (column, line) => {
            float distance = depth.Row(line)[column - depth.Origin.X].X / Vector3.Dot(camera.Ray(new Vector2(column + 0.5f, context.Extent.Height - line - 0.5f)).Direction, axis);
            float path = float.MaxNumber(distance - state.Start, 0f) / attenuation;
            Vector2 point = frame.Point(column, line);
            return beyond(distance) ? 0f : float.IsPositiveInfinity(path) ? 1f : 1f - MathF.Exp(-path * density(point, path));
        };
    }

    private static Action<Span<Vector4>, int, int> Fill(Fog state, PassContext context, Func<int, int, float> opacity, Func<int, int, Vector3> light) {
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
    private static readonly (StateParameter<Fog> Clock, StateParameter<Fog> Pace) Time =
        Timing.Kinds(Lens<Fog, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly FogParameter Basis = new("basis", new StateParameter<Fog>.Record<NoiseBasis>(Lens<Fog, NoiseBasis>.New(static fog => fog.Basis, static basis => fog => fog with { Basis = basis })));
    public static readonly FogParameter Layer = new("layer", new StateParameter<Fog>.Record<GeneratedLayer>(Lens<Fog, GeneratedLayer>.New(static fog => fog.Layer, static layer => fog => fog with { Layer = layer })));
    private static readonly Presentation<Drift, float> Travel = Drift.Presentation with { Soft = (-1f, 1f) };

    public static readonly FogParameter Scale = new("scale", new StateParameter<Fog>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Fog, ShortSideExtent>.New(static fog => fog.Scale, static scale => fog => fog with { Scale = scale }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log }));
    public static readonly FogParameter DriftX = new("drift-x", new StateParameter<Fog>.Bounded<Drift, float, InvalidGenerator>(
        Lens<Fog, Drift>.New(static fog => fog.DriftX, static drift => fog => fog with { DriftX = drift }), Travel));
    public static readonly FogParameter DriftY = new("drift-y", new StateParameter<Fog>.Bounded<Drift, float, InvalidGenerator>(
        Lens<Fog, Drift>.New(static fog => fog.DriftY, static drift => fog => fog with { DriftY = drift }), Travel));
    public static readonly FogParameter Evolution = new("evolution", new StateParameter<Fog>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Fog, Frequency>.New(static fog => fog.Evolution, static evolution => fog => fog with { Evolution = evolution }),
        Frequency.Presentation with { Soft = (-4f, 4f) }));
    public static readonly FogParameter Variation = new("variation", new StateParameter<Fog>.Bounded<Mix, float, InvalidGrade>(
        Lens<Fog, Mix>.New(static fog => fog.Variation, static variation => fog => fog with { Variation = variation }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly FogParameter Start = new("start", new StateParameter<Fog>.Bounded<FogDistance, float, InvalidGenerator>(
        Lens<Fog, FogDistance>.New(static fog => fog.Start, static start => fog => fog with { Start = start }),
        new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0f, 100f), Scale = TrackScale.Cubic }));
    public static readonly FogParameter Attenuation = new("attenuation", new StateParameter<Fog>.OptionalBounded<FogAttenuation, float, InvalidGenerator>(
        Lens<Fog, Option<FogAttenuation>>.New(static fog => fog.Attenuation, static attenuation => fog => fog with { Attenuation = attenuation }),
        new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0.3f, 1000f), Scale = TrackScale.Log }));
    public static readonly FogParameter Cutoff = new("cutoff", new StateParameter<Fog>.OptionalBounded<FogDistance, float, InvalidGenerator>(
        Lens<Fog, Option<FogDistance>>.New(static fog => fog.Cutoff, static cutoff => fog => fog with { Cutoff = cutoff }),
        new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0f, 1000f), Scale = TrackScale.Cubic }));
    public static readonly FogParameter Diffusion = new("diffusion", new StateParameter<Fog>.OptionalBounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Fog, Option<ShortSideLength>>.New(static fog => fog.Diffusion, static diffusion => fog => fog with { Diffusion = diffusion }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 2f) }));
    public static readonly FogParameter Amount = new("amount", new StateParameter<Fog>.Bounded<Mix, float, InvalidGrade>(
        Lens<Fog, Mix>.New(static fog => fog.Amount, static amount => fog => fog with { Amount = amount }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly FogParameter Colors = new("colors", new StateParameter<Fog>.Gradient(
        Lens<Fog, Ramp>.New(static fog => fog.Colors, static colors => fog => fog with { Colors = colors })));
    public static readonly FogParameter Seed = new("seed", new StateParameter<Fog>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Fog, Seed>.New(static fog => fog.Seed, static seed => fog => fog with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FogParameter Clock = new("clock", Time.Clock);
    public static readonly FogParameter Pace = new("pace", Time.Pace);

    public StateParameter<Fog> Kind { get; }
}

public sealed record Starfield(
    Placement Placement,
    CellularNoise Stars, ShortSideExtent StarScale, ShortSideExtent StarRadius, Ramp StarColors, Exposure StarExposure, Mix Twinkle, Frequency TwinkleRate,
    NoiseBasis Nebula, ShortSideExtent NebulaScale, AxisFraction NebulaDensity, Ramp NebulaColors, Exposure NebulaExposure, Frequency Evolution,
    GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Starfield, StarfieldParameter, InvalidGenerator>, IPixelStage<Starfield> {
    public static Starfield Default { get; } = new(
        Placement.Default,
        CellularNoise.Default with {
            Feature = CellFeatureKind.SmoothF1,
            Octaves = new(FractalDetail.Create(2.56f), AxisFraction.Create(0.165441f), FractalLacunarity.Create(2.6f)),
            Cells = new(CellMetric.Minkowski, MinkowskiExponent.Create(0.4f), AxisFraction.MaxValue),
            Smoothness = AxisFraction.MaxValue,
        },
        ShortSideExtent.Create(1f / 10f),
        ShortSideExtent.Create(1f / 10f), Ramp.Grayscale, Exposure.Neutral, Mix.MinValue, Frequency.Neutral,
        NoiseBasis.Default with { Octaves = Octaves.Default with { Detail = FractalDetail.Create(6f), Roughness = AxisFraction.Create(0.56f) }, Distortion = NoiseDistortion.Create(0.1f) },
        Looks.ShortSide,
        AxisFraction.Half, Ramp.Grayscale, Exposure.Neutral, Frequency.Neutral,
        GeneratedLayer.Added, Seed.MinValue, Timing.Standard);

    public static Option<PixelPass> Pass(Starfield state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context))));

    private static Action<Span<Vector4>, int, int> Fill(Starfield state, PassContext context) {
        FieldFrame frame = state.Placement.Frame(context.Extent);
        uint field = CoordinateHash.Field(NoiseStream.Starfield, state.Seed, 0u);
        float t = state.Timing.At(context);
        (Func<Vector4, uint, float> plain, Func<Vector4, uint, float> nebula) = ((NoiseBasis.Plain with { Dimensions = NoiseDimensions.Four }).Sampler, state.Nebula.Sampler);
        (RampTable starTable, RampTable nebulaTable) = (state.StarColors.Tabulate(context.Working), state.NebulaColors.Tabulate(context.Working));
        (uint glint, uint sites, uint clouds, uint draws) =
            (CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        (float starScale, float starRadius, float nebulaScale, float nebulaDensity) =
            (state.StarScale, state.StarRadius, state.NebulaScale, state.NebulaDensity);
        (float starGain, float nebulaGain) = (state.StarExposure.Scale, state.NebulaExposure.Scale);
        (float twinkle, float phase, float evolution) = (state.Twinkle, state.TwinkleRate * t, state.Evolution * t);
        Func<Vector4, float> steady = state.Twinkle == Mix.MinValue
            ? static _ => 1f
            : at => 1f - (twinkle * plain(at + new Vector4(0f, 0f, 0f, phase), glint));
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 local = frame.Local(frame.Point(column + i, line));
                Vector2 p = new(local.X, -local.Y);
                CellFeature cell = state.Stars.Feature.Sample(state.Stars, new Vector4(p / starScale, 0f, 0f), sites);
                Vector4 draw = NoiseFunctions.White(cell.Site, draws);
                float b = draw.Y * Easing.Saturate(1f - (cell.Distance * starScale / starRadius)) * steady(cell.Site);
                float n = nebula(new Vector4(p / nebulaScale, evolution, 0f), clouds);
                Vector4 cloud = nebulaTable.Sample(n);
                row[i] = new Vector4(
                    (b * starGain * starTable.Sample(draw.X).AsVector3())
                    + ((nebulaDensity == 0f ? 0f : Easing.SmootherStep(Easing.Saturate((n - (1f - nebulaDensity)) / nebulaDensity))) * cloud.W * nebulaGain * cloud.AsVector3()),
                    1f);
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StarfieldParameter : IStateParameter<Starfield> {
    private static readonly (StateParameter<Starfield> Clock, StateParameter<Starfield> Pace) Time =
        Timing.Kinds(Lens<Starfield, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly StarfieldParameter Placement = new("placement", new StateParameter<Starfield>.Record<Placement>(Lens<Starfield, Placement>.New(static sky => sky.Placement, static placement => sky => sky with { Placement = placement })));
    public static readonly StarfieldParameter Stars = new("stars", new StateParameter<Starfield>.Record<CellularNoise>(Lens<Starfield, CellularNoise>.New(static sky => sky.Stars, static stars => sky => sky with { Stars = stars })));
    public static readonly StarfieldParameter Nebula = new("nebula", new StateParameter<Starfield>.Record<NoiseBasis>(Lens<Starfield, NoiseBasis>.New(static sky => sky.Nebula, static nebula => sky => sky with { Nebula = nebula })));
    public static readonly StarfieldParameter Layer = new("layer", new StateParameter<Starfield>.Record<GeneratedLayer>(Lens<Starfield, GeneratedLayer>.New(static sky => sky.Layer, static layer => sky => sky with { Layer = layer })));
    private static readonly Presentation<AxisFraction, float> Share = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly Presentation<Frequency, float> Rate = Frequency.Presentation with { Soft = (-4f, 4f) };

    public static readonly StarfieldParameter StarScale = new("star-scale", new StateParameter<Starfield>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Starfield, ShortSideExtent>.New(static sky => sky.StarScale, static scale => sky => sky with { StarScale = scale }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.04f, 1f), Scale = TrackScale.Log }));
    public static readonly StarfieldParameter StarRadius = new("star-radius", new StateParameter<Starfield>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Starfield, ShortSideExtent>.New(static sky => sky.StarRadius, static radius => sky => sky with { StarRadius = radius }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 1f), Scale = TrackScale.Log }));
    public static readonly StarfieldParameter StarColors = new("star-colors", new StateParameter<Starfield>.Gradient(
        Lens<Starfield, Ramp>.New(static sky => sky.StarColors, static colors => sky => sky with { StarColors = colors })));
    public static readonly StarfieldParameter StarExposure = new("star-exposure", new StateParameter<Starfield>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<Starfield, Exposure>.New(static sky => sky.StarExposure, static exposure => sky => sky with { StarExposure = exposure }), Exposure.Presentation));
    public static readonly StarfieldParameter Twinkle = new("twinkle", new StateParameter<Starfield>.Bounded<Mix, float, InvalidGrade>(
        Lens<Starfield, Mix>.New(static sky => sky.Twinkle, static twinkle => sky => sky with { Twinkle = twinkle }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly StarfieldParameter TwinkleRate = new("twinkle-rate", new StateParameter<Starfield>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Starfield, Frequency>.New(static sky => sky.TwinkleRate, static rate => sky => sky with { TwinkleRate = rate }), Rate));
    public static readonly StarfieldParameter NebulaScale = new("nebula-scale", new StateParameter<Starfield>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Starfield, ShortSideExtent>.New(static sky => sky.NebulaScale, static scale => sky => sky with { NebulaScale = scale }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.2f, 8f), Scale = TrackScale.Log }));
    public static readonly StarfieldParameter NebulaDensity = new("nebula-density", new StateParameter<Starfield>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Starfield, AxisFraction>.New(static sky => sky.NebulaDensity, static density => sky => sky with { NebulaDensity = density }), Share));
    public static readonly StarfieldParameter NebulaColors = new("nebula-colors", new StateParameter<Starfield>.Gradient(
        Lens<Starfield, Ramp>.New(static sky => sky.NebulaColors, static colors => sky => sky with { NebulaColors = colors })));
    public static readonly StarfieldParameter NebulaExposure = new("nebula-exposure", new StateParameter<Starfield>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<Starfield, Exposure>.New(static sky => sky.NebulaExposure, static exposure => sky => sky with { NebulaExposure = exposure }), Exposure.Presentation));
    public static readonly StarfieldParameter Evolution = new("evolution", new StateParameter<Starfield>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Starfield, Frequency>.New(static sky => sky.Evolution, static evolution => sky => sky with { Evolution = evolution }), Rate));
    public static readonly StarfieldParameter Seed = new("seed", new StateParameter<Starfield>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Starfield, Seed>.New(static sky => sky.Seed, static seed => sky => sky with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly StarfieldParameter Clock = new("clock", Time.Clock);
    public static readonly StarfieldParameter Pace = new("pace", Time.Pace);

    public StateParameter<Starfield> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Emission {
    public static readonly Func<(Gamut Gamut, ColorTemperature Cool, ColorTemperature Hot), Func<float, Vector3>> Span =
        memo<(Gamut Gamut, ColorTemperature Cool, ColorTemperature Hot), Func<float, Vector3>>(static key => {
            const int intervals = 256;
            Vector3[] samples = [.. toSeq(Range(0, intervals + 1)).Map(i => Light(key.Gamut, ColorTemperature.Create(double.Lerp(key.Cool, key.Hot, i / (double)intervals))))];
            return t => (t * intervals) switch {
                var texel => int.Min((int)texel, intervals - 1) switch { var i => Vector3.Lerp(samples[i], samples[i + 1], texel - i) },
            };
        });

    private static ColorTemperature Hottest { get; } = ColorTemperature.Create(12000d);

    public static Presentation<ColorTemperature, double> Presentation { get; } = new() {
        Unit = ColorTemperature.Unit,
        Soft = (ColorTemperature.MinValue, Hottest),
        Stops = Ramp.Create(Seq(ColorTemperature.MinValue, ColorTemperature.Flame, ColorTemperature.ViewDefault, Hottest).Map(static kelvin =>
            RampStop.Create(RampPosition.Create((kelvin - (double)ColorTemperature.MinValue) / (Hottest - (double)ColorTemperature.MinValue)), new Vector4(Light(Gamut.StandardRgb, kelvin), 1f))), RampInterpolation.Linear),
    };

    private static Vector3 Light(Gamut gamut, ColorTemperature kelvin) =>
        Adaptation.Blackbody(gamut, kelvin).RgbLinear switch { var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B) };
}

file static class Looks {
    public static ShortSideExtent ShortSide { get; } = ShortSideExtent.Create(1f);

    public static ShortSideExtent Wavelength(float scale) => ShortSideExtent.Create(float.Pi / (10f * scale));

    public static Ramp Ramp(RampInterpolation interpolation, params ReadOnlySpan<(double Position, Vector3 Light, float Coverage)> stops) =>
        Valid.Value(Pixels.Ramp.Validate(toSeq(stops.ToArray()).Map(static stop =>
            Valid.Value(RampStop.Validate(RampPosition.Create(stop.Position), new Vector4(stop.Light, stop.Coverage), out RampStop item), item)),
            interpolation, out Ramp? ramp), ramp);
}
