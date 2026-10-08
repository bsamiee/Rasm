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

public sealed record GeneratedLayer(BlendingMode Mode, Exposure Exposure) {
    public static GeneratedLayer Mixed { get; } = new(BlendingMode.Mix, Exposure.Neutral);
    public static GeneratedLayer Added { get; } = new(BlendingMode.Add, Exposure.Neutral);

    private static readonly Lens<GeneratedLayer, BlendingMode> ModeOf =
        Lens<GeneratedLayer, BlendingMode>.New(static layer => layer.Mode, static mode => layer => layer with { Mode = mode });
    private static readonly Lens<GeneratedLayer, Exposure> ExposureOf =
        Lens<GeneratedLayer, Exposure>.New(static layer => layer.Exposure, static exposure => layer => layer with { Exposure = exposure });

    internal static (StateParameter<TRecord> Mode, StateParameter<TRecord> Exposure) Kinds<TRecord>(Lens<TRecord, GeneratedLayer> layer) =>
        (new StateParameter<TRecord>.Choice<BlendingMode, InvalidGrade>(lens(layer, ModeOf)),
         new StateParameter<TRecord>.Bounded<Exposure, float, InvalidToneValue>(lens(layer, ExposureOf), Exposure.Presentation));

    internal Action<Span<Vector4>, int, int> Kernel(Action<Span<Vector4>, int, int> fill) {
        (float gain, BlendingMode mode) = (Exposure.Scale, Mode);
        return (row, column, line) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
            fill(layer.Span, column, line);
            for (int i = 0; i < row.Length; i++)
                (weights.Span[i], layer.Span[i]) = (layer.Span[i].W, layer.Span[i] * gain);
            mode.Mixed(row, layer.Span, weights.Span);
            layer.Span.CopyTo(row);
        };
    }
}

public sealed record Placement(FramePosition Center, SignedAngle Rotation) {
    public static Placement Centered { get; } = new(FramePosition.Center, SignedAngle.Neutral);

    private static readonly Lens<Placement, FramePosition> CenterOf =
        Lens<Placement, FramePosition>.New(static placement => placement.Center, static center => placement => placement with { Center = center });
    private static readonly Lens<Placement, SignedAngle> RotationOf =
        Lens<Placement, SignedAngle>.New(static placement => placement.Rotation, static rotation => placement => placement with { Rotation = rotation });

    internal static (StateParameter<TRecord> CenterX, StateParameter<TRecord> CenterY, StateParameter<TRecord> Rotation) Kinds<TRecord>(Lens<TRecord, Placement> placement) =>
        FramePosition.Kinds(lens(placement, CenterOf)) switch {
            var (x, y) => (x, y, new StateParameter<TRecord>.Bounded<SignedAngle, float, InvalidPixelValue>(
                lens(placement, RotationOf), new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral })),
        };

    internal FieldFrame Frame(PixelExtent extent) =>
        (Half: new Vector2(extent.Width / 2f, extent.Height / 2f), Scale: 1f / int.Min(extent.Width, extent.Height)) switch {
            var (half, scale) => new(half, scale, (Center.Point(extent) - half) * scale, Matrix3x2.CreateRotation(Rotation)),
        };
}

internal readonly record struct FieldFrame(Vector2 Half, float Scale, Vector2 Origin, Matrix3x2 Turn) {
    public Vector2 Point(int column, int line) => new Vector2(column + 0.5f - Half.X, Half.Y - line - 0.5f) * Scale;

    public Vector2 Local(Vector2 q) => Vector2.Transform(q - Origin, Turn);
}

internal sealed class EmissionSpan(Vector3[] samples) {
    public const int Texels = 256;

    public Vector3 Sample(float t) =>
        (t * Texels) switch { var texel => int.Min((int)texel, Texels - 1) switch { var i => Vector3.Lerp(samples[i], samples[i + 1], texel - i) } };
}

public sealed record NoiseTexture(
    NoiseBasis Basis, ShortSideExtent ScaleX, ShortSideExtent ScaleY, Placement Placement, SignedAngle Twist, ShortSideExtent Radius, TunnelSlope Tunnel,
    Spin Spin, Drift DriftX, Drift DriftY, Frequency Evolution, ShortSideOffset Warp, ShortSideExtent WarpScale, NoiseBasis WarpBasis, Ramp Colors, Option<OpticalDepth> Extinction,
    GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<NoiseTexture, NoiseTextureParameter, InvalidGenerator>, IPixelStage<NoiseTexture> {
    public static NoiseTexture Default { get; } = Valid.Value(ShortSideExtent.Validate(1f / 5f, provider: null, out ShortSideExtent scale), scale) switch {
        var features => new(
            NoiseBasis.Standard, features, features, Placement.Centered, SignedAngle.Neutral,
            Valid.Value(ShortSideExtent.Validate(0.5f, provider: null, out ShortSideExtent radius), radius), TunnelSlope.Neutral, Spin.Neutral, Drift.Neutral, Drift.Neutral,
            Frequency.Neutral, ShortSideOffset.Neutral, Looks.Wavelength(1.3f),
            NoiseBasis.Standard with {
                Kind = BasisKind.Wave,
                Octaves = Octaves.Standard with { Roughness = AxisFraction.Create(0.186957f) },
                Distortion = NoiseDistortion.Create(14.9f),
                WaveForm = Some(WaveForm.Bands),
                WaveProfile = Some(WaveProfile.Sine),
                WavePhase = Some(SignedAngle.Create(-3f)),
                WaveDetailScale = Some(WaveDetailScale.Create(1.3f)),
            },
            Ramp.Grayscale, None, GeneratedLayer.Mixed, Seed.MinValue, Timing.Standard),
    };

    public static NoiseTexture TurbulentNoise { get; } = Default with { Evolution = Valid.Value(Frequency.Validate(0.3f, provider: null, out Frequency evolution), evolution) };

    public static NoiseTexture BlockNoise { get; } = Default with { Basis = NoiseBasis.Standard with { Kind = BasisKind.White } };

    public static NoiseTexture Diamonds { get; } = Default with {
        Basis = NoiseBasis.Standard with { Kind = BasisKind.F2, Octaves = Octaves.Plain, Cellular = Cellular.Standard with { Randomness = AxisFraction.MinValue } },
    };

    public static NoiseTexture VoronoiNoise { get; } = TurbulentNoise with {
        Basis = NoiseBasis.Standard with {
            Kind = BasisKind.F1,
            Octaves = Octaves.Standard with { Detail = Valid.Value(FractalDetail.Validate(1f, provider: null, out FractalDetail detail), detail) },
        },
    };

    public static NoiseTexture Sparkle { get; } = TurbulentNoise with {
        Basis = VoronoiNoise.Basis with {
            Kind = BasisKind.SmoothF1,
            Cellular = Cellular.Standard with { Metric = CellMetric.Minkowski, Exponent = Valid.Value(MinkowskiExponent.Validate(1f, provider: null, out MinkowskiExponent exponent), exponent) },
            CellSmoothness = Some(AxisFraction.MaxValue),
        },
    };

    public static NoiseTexture VoronoiCells { get; } = Default with {
        Basis = NoiseBasis.Standard with { Kind = BasisKind.EdgeDistance, Octaves = Octaves.Plain },
        DriftY = Valid.Value(Drift.Validate(-0.6f, provider: null, out Drift drift), drift),
    };

    public static NoiseTexture Waves { get; } = Default with {
        Basis = NoiseBasis.Standard with {
            Kind = BasisKind.Wave,
            Distortion = Valid.Value(NoiseDistortion.Validate(4f, provider: null, out NoiseDistortion distortion), distortion),
            WaveForm = Some(WaveForm.Bands),
            WaveProfile = Some(WaveProfile.Sine),
            WaveDetailScale = Some(Valid.Value(WaveDetailScale.Validate(2f, provider: null, out WaveDetailScale detailScale), detailScale)),
        },
        ScaleX = Looks.Wavelength(1f),
        ScaleY = Looks.Wavelength(1f),
        Evolution = Valid.Value(Frequency.Validate(-0.3f / float.Tau, provider: null, out Frequency evolution), evolution),
    };

    public static NoiseTexture LightLeak { get; } =
        (Highlights: new Vector3(1f, 0.7529f, 0.6584f), Base: new Vector3(0.0437f, 0.0116f, 1f), Lit: 0.1f, Dark: 0.6f, Bias: 0.225f, Edge: 0.775f,
         Scale: Valid.Value(ShortSideExtent.Validate(1f / 0.7f, provider: null, out ShortSideExtent scale), scale)) switch {
             var (highlights, @base, lit, dark, bias, edge, features) => Default with {
                 Basis = NoiseBasis.Plain,
                 ScaleX = features,
                 ScaleY = features,
                 Evolution = Valid.Value(Frequency.Validate(0.5f, provider: null, out Frequency evolution), evolution),
                 Colors = Looks.Ramp(RampInterpolation.Linear, [.. Seq(lit, bias, dark).Map(n =>
                    ((double)n, Vector3.Lerp(highlights, @base, Easing.Saturate((n - bias) / (edge - bias))), 1f - Easing.Saturate((n - lit) / (dark - lit))))]),
                 Layer = GeneratedLayer.Added,
             },
         };

    public static NoiseTexture Flow { get; } =
        (Shadows: new Vector3(0.305f, 0.129f, 1f), Highlights: new Vector3(1f, 0.059f, 0.298f), Dark: 0.37f, Lit: 0.87f, From: 0.62f, To: 1.1f) switch {
            var (shadows, highlights, dark, lit, from, to) => Default with {
                Basis = NoiseBasis.Standard with {
                    Octaves = new(
                        Valid.Value(FractalDetail.Validate(2.48f, provider: null, out FractalDetail detail), detail), AxisFraction.MinValue,
                        Valid.Value(FractalLacunarity.Validate(12.4f, provider: null, out FractalLacunarity lacunarity), lacunarity)),
                    Distortion = Valid.Value(NoiseDistortion.Validate(7.25f, provider: null, out NoiseDistortion distortion), distortion),
                },
                ScaleX = Looks.ShortSide,
                ScaleY = Looks.ShortSide,
                Evolution = Valid.Value(Frequency.Validate(0.15f, provider: null, out Frequency evolution), evolution),
                Warp = Valid.Value(ShortSideOffset.Validate(0.0566f, provider: null, out ShortSideOffset warp), warp),
                Colors = Looks.Ramp(RampInterpolation.Linear, [.. Seq(dark, from, lit, 1f).Map(n =>
                    ((double)n, Vector3.Lerp(shadows, highlights, Easing.SmootherStep(Easing.Saturate((n - from) / (to - from)))), Easing.Saturate((n - dark) / (lit - dark))))]),
                Layer = GeneratedLayer.Added,
            },
        };

    public static NoiseTexture Vortex { get; } =
        (Gain: (0.8f * 2f * new Vector3(0.134f, 0.431f, 0.5f)) + new Vector3(0.2f), Gamma: 4f, Scale: Valid.Value(ShortSideExtent.Validate(1f / 6f, provider: null, out ShortSideExtent scale), scale)) switch {
            var (gain, gamma, features) => Default with {
                Basis = NoiseBasis.Standard with {
                    Octaves = new(
                        Valid.Value(FractalDetail.Validate(4f, provider: null, out FractalDetail detail), detail),
                        Valid.Value(AxisFraction.Validate(0.2f, provider: null, out AxisFraction roughness), roughness),
                        Valid.Value(FractalLacunarity.Validate(3f, provider: null, out FractalLacunarity lacunarity), lacunarity)),
                    Distortion = Valid.Value(NoiseDistortion.Validate(0.5f, provider: null, out NoiseDistortion distortion), distortion),
                },
                ScaleX = features,
                ScaleY = features,
                Twist = Valid.Value(SignedAngle.Validate(2.8f, provider: null, out SignedAngle twist), twist),
                Tunnel = Valid.Value(TunnelSlope.Validate(0.6f, provider: null, out TunnelSlope tunnel), tunnel),
                Spin = Valid.Value(Spin.Validate(1f, provider: null, out Spin spin), spin),
                Evolution = Valid.Value(Frequency.Validate(0.3f, provider: null, out Frequency evolution), evolution),
                Colors = Looks.Ramp(RampInterpolation.Linear, [.. Seq(0f, 0.5f, 1f).Map(n => ((double)n, Vector3.Exp(gamma * Vector3.Log(n * gain)), 1f))]),
            },
        };

    public static NoiseTexture Smoke { get; } = Default with {
        Basis = NoiseBasis.Standard with {
            Octaves = Octaves.Standard with {
                Detail = Valid.Value(FractalDetail.Validate(5f, provider: null, out FractalDetail detail), detail),
                Roughness = Valid.Value(AxisFraction.Validate(0.55f, provider: null, out AxisFraction roughness), roughness),
            },
            Distortion = Valid.Value(NoiseDistortion.Validate(0.5f, provider: null, out NoiseDistortion distortion), distortion),
        },
        ScaleX = Valid.Value(ShortSideExtent.Validate(0.2f * ReferenceFrame.GreaterSide, provider: null, out ShortSideExtent scaleX), scaleX),
        ScaleY = Valid.Value(ShortSideExtent.Validate(1.5f * 0.2f * ReferenceFrame.GreaterSide, provider: null, out ShortSideExtent scaleY), scaleY),
        DriftY = Valid.Value(Drift.Validate(0.05f * ReferenceFrame.GreaterSide, provider: null, out Drift drift), drift),
        Evolution = Valid.Value(Frequency.Validate(0.3f, provider: null, out Frequency evolution), evolution),
        Colors = Looks.Ramp(RampInterpolation.Ease, (0.4d, new Vector3(0.04f), 0f), (0.8d, new Vector3(0.04f), 1f)),
        Extinction = Some(Valid.Value(OpticalDepth.Validate(2f, provider: null, out OpticalDepth extinction), extinction)),
    };

    public static NoiseTexture Mist { get; } = Default with {
        Basis = NoiseBasis.Standard with {
            Octaves = Octaves.Standard with {
                Detail = Valid.Value(FractalDetail.Validate(3f, provider: null, out FractalDetail detail), detail),
                Roughness = Valid.Value(AxisFraction.Validate(0.45f, provider: null, out AxisFraction roughness), roughness),
            },
        },
        ScaleX = Valid.Value(ShortSideExtent.Validate(0.5f * ReferenceFrame.GreaterSide, provider: null, out ShortSideExtent scaleX), scaleX),
        ScaleY = Valid.Value(ShortSideExtent.Validate(0.5f * 0.5f * ReferenceFrame.GreaterSide, provider: null, out ShortSideExtent scaleY), scaleY),
        DriftX = Valid.Value(Drift.Validate(0.02f * ReferenceFrame.GreaterSide, provider: null, out Drift drift), drift),
        Evolution = Valid.Value(Frequency.Validate(0.1f, provider: null, out Frequency evolution), evolution),
        Colors = Looks.Ramp(RampInterpolation.Ease, (0.3d, new Vector3(0.7f), 0f), (0.9d, new Vector3(0.7f), 1f)),
        Extinction = Some(Valid.Value(OpticalDepth.Validate(0.6f, provider: null, out OpticalDepth extinction), extinction)),
    };

    public static Option<PixelPass> Pass(NoiseTexture state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context))));

    private static Action<Span<Vector4>, int, int> Fill(NoiseTexture state, PassContext context) {
        FieldFrame frame = state.Placement.Frame(context.Extent);
        uint field = CoordinateHash.Field(NoiseStream.NoiseTexture, state.Seed, 0u);
        float t = state.Timing.At(context);
        (NoiseSampler basis, NoiseSampler warp, RampTable table) = (state.Basis.Sampler, state.WarpBasis.Sampler, state.Colors.Tabulate(context.Working));
        (uint level, uint across, uint down) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u));
        (Vector2 scale, Vector2 drift, float wavelength) = (new(state.ScaleX, state.ScaleY), new Vector2(state.DriftX, state.DriftY) * t, state.WarpScale);
        Vector2 reach = new Vector2(state.Warp) / scale;
        (float radius, float twist, float turn, float evolution, float tunnel) = (state.Radius, state.Twist, state.Spin * t, state.Evolution * t, state.Radius * state.Tunnel / state.ScaleX);
        (bool turned, bool warped) = (state.Twist != SignedAngle.Neutral || state.Spin != Spin.Neutral, state.Warp != ShortSideOffset.Neutral);
        Func<float, float> coverage = state.Extinction.Match(
            Some: static extinction => (Func<float, float>)(density => 1f - MathF.Exp(-extinction * density)),
            None: static () => static density => density);
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 local = frame.Local(frame.Point(column + i, line));
                Vector2 p = new(local.X, -local.Y);
                float r = p.Length() / radius;
                Vector2 o = (turned ? Vector2.Transform(p, Matrix3x2.CreateRotation((twist * (1f - r)) + turn)) : p) - drift;
                (Vector2 q, float z) = (o / scale, evolution + (float.Min(r, 1f) * tunnel));
                Vector4 w = new(o / wavelength, z, 0f);
                Vector2 moved = warped ? q + (reach * new Vector2((2f * warp.Sample(w, across)) - 1f, (2f * warp.Sample(w, down)) - 1f)) : q;
                Vector4 color = table.Sample(basis.Sample(new Vector4(moved, z, 0f), level));
                row[i] = new Vector4(color.AsVector3(), coverage(color.W));
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
    private static readonly BasisKinds<NoiseTexture> Basis =
        NoiseBasis.Kinds(Lens<NoiseTexture, NoiseBasis>.New(static texture => texture.Basis, static basis => texture => texture with { Basis = basis }));
    private static readonly BasisKinds<NoiseTexture> WarpBasis =
        NoiseBasis.Kinds(Lens<NoiseTexture, NoiseBasis>.New(static texture => texture.WarpBasis, static basis => texture => texture with { WarpBasis = basis }));
    private static readonly (StateParameter<NoiseTexture> CenterX, StateParameter<NoiseTexture> CenterY, StateParameter<NoiseTexture> Rotation) Placed =
        Placement.Kinds(Lens<NoiseTexture, Placement>.New(static texture => texture.Placement, static placement => texture => texture with { Placement = placement }));
    private static readonly (StateParameter<NoiseTexture> Mode, StateParameter<NoiseTexture> Exposure) Layer =
        GeneratedLayer.Kinds(Lens<NoiseTexture, GeneratedLayer>.New(static texture => texture.Layer, static layer => texture => texture with { Layer = layer }));
    private static readonly Presentation<ShortSideExtent, float> Extent = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log };
    private static readonly Presentation<Drift, float> Travel = Drift.Presentation with { Soft = (-1f, 1f) };

    public static readonly NoiseTextureParameter BasisKind = new("basis-kind", Basis.Kind);
    public static readonly NoiseTextureParameter BasisDimensions = new("basis-dimensions", Basis.Dimensions);
    public static readonly NoiseTextureParameter BasisDetail = new("basis-detail", Basis.Octaves.Detail);
    public static readonly NoiseTextureParameter BasisRoughness = new("basis-roughness", Basis.Octaves.Roughness);
    public static readonly NoiseTextureParameter BasisLacunarity = new("basis-lacunarity", Basis.Octaves.Lacunarity);
    public static readonly NoiseTextureParameter BasisDistortion = new("basis-distortion", Basis.Distortion);
    public static readonly NoiseTextureParameter BasisMetric = new("basis-metric", Basis.Metric);
    public static readonly NoiseTextureParameter BasisExponent = new("basis-exponent", Basis.Exponent);
    public static readonly NoiseTextureParameter BasisRandomness = new("basis-randomness", Basis.Randomness);
    public static readonly NoiseTextureParameter BasisOffset = new("basis-offset", Basis.Offset);
    public static readonly NoiseTextureParameter BasisGain = new("basis-gain", Basis.Gain);
    public static readonly NoiseTextureParameter BasisSmoothness = new("basis-smoothness", Basis.Smoothness);
    public static readonly NoiseTextureParameter BasisFrequency = new("basis-frequency", Basis.Frequency);
    public static readonly NoiseTextureParameter BasisAnisotropy = new("basis-anisotropy", Basis.Anisotropy);
    public static readonly NoiseTextureParameter BasisOrientation = new("basis-orientation", Basis.Orientation);
    public static readonly NoiseTextureParameter BasisDepth = new("basis-depth", Basis.Depth);
    public static readonly NoiseTextureParameter BasisForm = new("basis-form", Basis.Form);
    public static readonly NoiseTextureParameter BasisProfile = new("basis-profile", Basis.Profile);
    public static readonly NoiseTextureParameter BasisPhase = new("basis-phase", Basis.Phase);
    public static readonly NoiseTextureParameter BasisDetailScale = new("basis-detail-scale", Basis.DetailScale);
    public static readonly NoiseTextureParameter ScaleX = new("scale-x", new StateParameter<NoiseTexture>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideExtent>.New(static texture => texture.ScaleX, static scale => texture => texture with { ScaleX = scale }), Extent));
    public static readonly NoiseTextureParameter ScaleY = new("scale-y", new StateParameter<NoiseTexture>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideExtent>.New(static texture => texture.ScaleY, static scale => texture => texture with { ScaleY = scale }), Extent));
    public static readonly NoiseTextureParameter CenterX = new("center-x", Placed.CenterX);
    public static readonly NoiseTextureParameter CenterY = new("center-y", Placed.CenterY);
    public static readonly NoiseTextureParameter Rotation = new("rotation", Placed.Rotation);
    public static readonly NoiseTextureParameter Twist = new("twist", new StateParameter<NoiseTexture>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<NoiseTexture, SignedAngle>.New(static texture => texture.Twist, static twist => texture => texture with { Twist = twist }),
        new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral }));
    public static readonly NoiseTextureParameter Radius = new("radius", new StateParameter<NoiseTexture>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<NoiseTexture, ShortSideExtent>.New(static texture => texture.Radius, static radius => texture => texture with { Radius = radius }), Extent));
    public static readonly NoiseTextureParameter Tunnel = new("tunnel", new StateParameter<NoiseTexture>.Bounded<TunnelSlope, float, InvalidGenerator>(
        Lens<NoiseTexture, TunnelSlope>.New(static texture => texture.Tunnel, static tunnel => texture => texture with { Tunnel = tunnel }), new() { Soft = (0f, 4f) }));
    public static readonly NoiseTextureParameter Spin = new("spin", new StateParameter<NoiseTexture>.Bounded<Spin, float, InvalidGenerator>(
        Lens<NoiseTexture, Spin>.New(static texture => texture.Spin, static spin => texture => texture with { Spin = spin }),
        Generators.Spin.Presentation with { Soft = (-float.Tau, float.Tau) }));
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
    public static readonly NoiseTextureParameter WarpKind = new("warp-kind", WarpBasis.Kind);
    public static readonly NoiseTextureParameter WarpDimensions = new("warp-dimensions", WarpBasis.Dimensions);
    public static readonly NoiseTextureParameter WarpDetail = new("warp-detail", WarpBasis.Octaves.Detail);
    public static readonly NoiseTextureParameter WarpRoughness = new("warp-roughness", WarpBasis.Octaves.Roughness);
    public static readonly NoiseTextureParameter WarpLacunarity = new("warp-lacunarity", WarpBasis.Octaves.Lacunarity);
    public static readonly NoiseTextureParameter WarpDistortion = new("warp-distortion", WarpBasis.Distortion);
    public static readonly NoiseTextureParameter WarpMetric = new("warp-metric", WarpBasis.Metric);
    public static readonly NoiseTextureParameter WarpExponent = new("warp-exponent", WarpBasis.Exponent);
    public static readonly NoiseTextureParameter WarpRandomness = new("warp-randomness", WarpBasis.Randomness);
    public static readonly NoiseTextureParameter WarpOffset = new("warp-offset", WarpBasis.Offset);
    public static readonly NoiseTextureParameter WarpGain = new("warp-gain", WarpBasis.Gain);
    public static readonly NoiseTextureParameter WarpSmoothness = new("warp-smoothness", WarpBasis.Smoothness);
    public static readonly NoiseTextureParameter WarpFrequency = new("warp-frequency", WarpBasis.Frequency);
    public static readonly NoiseTextureParameter WarpAnisotropy = new("warp-anisotropy", WarpBasis.Anisotropy);
    public static readonly NoiseTextureParameter WarpOrientation = new("warp-orientation", WarpBasis.Orientation);
    public static readonly NoiseTextureParameter WarpDepth = new("warp-depth", WarpBasis.Depth);
    public static readonly NoiseTextureParameter WarpForm = new("warp-form", WarpBasis.Form);
    public static readonly NoiseTextureParameter WarpProfile = new("warp-profile", WarpBasis.Profile);
    public static readonly NoiseTextureParameter WarpPhase = new("warp-phase", WarpBasis.Phase);
    public static readonly NoiseTextureParameter WarpDetailScale = new("warp-detail-scale", WarpBasis.DetailScale);
    public static readonly NoiseTextureParameter Colors = new("colors", new StateParameter<NoiseTexture>.Gradient(
        Lens<NoiseTexture, Ramp>.New(static texture => texture.Colors, static colors => texture => texture with { Colors = colors })));
    public static readonly NoiseTextureParameter Extinction = new("extinction", new StateParameter<NoiseTexture>.OptionalBounded<OpticalDepth, float, InvalidGenerator>(
        Lens<NoiseTexture, Option<OpticalDepth>>.New(static texture => texture.Extinction, static extinction => texture => texture with { Extinction = extinction }),
        new() { Soft = (0f, 8f) }));
    public static readonly NoiseTextureParameter Mode = new("mode", Layer.Mode);
    public static readonly NoiseTextureParameter Exposure = new("exposure", Layer.Exposure);
    public static readonly NoiseTextureParameter Seed = new("seed", new StateParameter<NoiseTexture>.Bounded<Seed, int, InvalidGenerator>(
        Lens<NoiseTexture, Seed>.New(static texture => texture.Seed, static seed => texture => texture with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly NoiseTextureParameter Clock = new("clock", Time.Clock);
    public static readonly NoiseTextureParameter Pace = new("pace", Time.Pace);

    public StateParameter<NoiseTexture> Kind { get; }
}

public sealed record Fire(
    NoiseBasis Basis, ShortSideExtent ScaleX, ShortSideExtent ScaleY, Placement Placement, ShortSideExtent Width, ShortSideExtent Height,
    Drift Rise, Frequency Evolution, AxisFraction Density, ColorTemperature Cool, ColorTemperature Hot, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Fire, FireParameter, InvalidGenerator>, IPixelStage<Fire> {
    public static Fire Default { get; } = 0.7f switch {
        var flames => new(
            NoiseBasis.Standard with { Octaves = Octaves.Standard with { Detail = FractalDetail.Create(4f) }, Distortion = NoiseDistortion.Create(1f) },
            Valid.Value(ShortSideExtent.Validate(1f / (flames * 2f), provider: null, out ShortSideExtent scaleX), scaleX),
            Valid.Value(ShortSideExtent.Validate(1f / flames, provider: null, out ShortSideExtent scaleY), scaleY),
            new Placement(new FramePosition(FrameAxis.Middle, FrameAxis.End), SignedAngle.Neutral), Looks.ShortSide, Looks.ShortSide,
            Drift.Rise, Frequency.Neutral, Valid.Value(AxisFraction.Validate(1f - 0.17f, provider: null, out AxisFraction density), density), ColorTemperature.MinValue, ColorTemperature.Flame,
            GeneratedLayer.Added, Seed.MinValue, Timing.Standard),
    };

    public static Option<PixelPass> Pass(Fire state, PassContext context) =>
        state.Density == AxisFraction.MinValue ? None : Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context))));

    private static Action<Span<Vector4>, int, int> Fill(Fire state, PassContext context) {
        FieldFrame frame = state.Placement.Frame(context.Extent);
        uint field = CoordinateHash.Field(NoiseStream.Fire, state.Seed, 0u);
        float t = state.Timing.At(context);
        (NoiseSampler basis, EmissionSpan span) = (state.Basis.Sampler, Emission.Span((context.Working, state.Cool, state.Hot)));
        (Vector2 scale, Vector2 envelope, Vector2 rise) = (new(state.ScaleX, state.ScaleY), new(state.Width, state.Height), new(0f, state.Rise * t));
        (float evolution, float density) = (state.Evolution * t, state.Density);
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 local = frame.Local(frame.Point(column + i, line));
                Vector2 uv = new(local.X, -local.Y);
                float n = basis.Sample(new Vector4((uv - rise) / scale, evolution, 0f), field);
                float e = float.Max(1f - (uv / envelope).Length(), 0f);
                float d = Easing.Saturate((n - (1f - (e * e)) - (1f - density)) / density);
                row[i] = new Vector4(span.Sample(d), d);
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FireParameter : IStateParameter<Fire> {
    private static readonly (StateParameter<Fire> Clock, StateParameter<Fire> Pace) Time =
        Timing.Kinds(Lens<Fire, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    private static readonly BasisKinds<Fire> Basis =
        NoiseBasis.Kinds(Lens<Fire, NoiseBasis>.New(static fire => fire.Basis, static basis => fire => fire with { Basis = basis }));
    private static readonly (StateParameter<Fire> CenterX, StateParameter<Fire> CenterY, StateParameter<Fire> Rotation) Placed =
        Placement.Kinds(Lens<Fire, Placement>.New(static fire => fire.Placement, static placement => fire => fire with { Placement = placement }));
    private static readonly (StateParameter<Fire> Mode, StateParameter<Fire> Exposure) Layer =
        GeneratedLayer.Kinds(Lens<Fire, GeneratedLayer>.New(static fire => fire.Layer, static layer => fire => fire with { Layer = layer }));
    private static readonly Presentation<ShortSideExtent, float> Feature = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (1f / 16f, 8f), Scale = TrackScale.Log };
    private static readonly Presentation<ShortSideExtent, float> Envelope = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.1f, 4f), Scale = TrackScale.Log };

    public static readonly FireParameter BasisKind = new("basis-kind", Basis.Kind);
    public static readonly FireParameter BasisDimensions = new("basis-dimensions", Basis.Dimensions);
    public static readonly FireParameter BasisDetail = new("basis-detail", Basis.Octaves.Detail);
    public static readonly FireParameter BasisRoughness = new("basis-roughness", Basis.Octaves.Roughness);
    public static readonly FireParameter BasisLacunarity = new("basis-lacunarity", Basis.Octaves.Lacunarity);
    public static readonly FireParameter BasisDistortion = new("basis-distortion", Basis.Distortion);
    public static readonly FireParameter BasisMetric = new("basis-metric", Basis.Metric);
    public static readonly FireParameter BasisExponent = new("basis-exponent", Basis.Exponent);
    public static readonly FireParameter BasisRandomness = new("basis-randomness", Basis.Randomness);
    public static readonly FireParameter BasisOffset = new("basis-offset", Basis.Offset);
    public static readonly FireParameter BasisGain = new("basis-gain", Basis.Gain);
    public static readonly FireParameter BasisSmoothness = new("basis-smoothness", Basis.Smoothness);
    public static readonly FireParameter BasisFrequency = new("basis-frequency", Basis.Frequency);
    public static readonly FireParameter BasisAnisotropy = new("basis-anisotropy", Basis.Anisotropy);
    public static readonly FireParameter BasisOrientation = new("basis-orientation", Basis.Orientation);
    public static readonly FireParameter BasisDepth = new("basis-depth", Basis.Depth);
    public static readonly FireParameter BasisForm = new("basis-form", Basis.Form);
    public static readonly FireParameter BasisProfile = new("basis-profile", Basis.Profile);
    public static readonly FireParameter BasisPhase = new("basis-phase", Basis.Phase);
    public static readonly FireParameter BasisDetailScale = new("basis-detail-scale", Basis.DetailScale);
    public static readonly FireParameter ScaleX = new("scale-x", new StateParameter<Fire>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Fire, ShortSideExtent>.New(static fire => fire.ScaleX, static scale => fire => fire with { ScaleX = scale }), Feature));
    public static readonly FireParameter ScaleY = new("scale-y", new StateParameter<Fire>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Fire, ShortSideExtent>.New(static fire => fire.ScaleY, static scale => fire => fire with { ScaleY = scale }), Feature));
    public static readonly FireParameter CenterX = new("center-x", Placed.CenterX);
    public static readonly FireParameter CenterY = new("center-y", Placed.CenterY);
    public static readonly FireParameter Rotation = new("rotation", Placed.Rotation);
    public static readonly FireParameter Width = new("width", new StateParameter<Fire>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Fire, ShortSideExtent>.New(static fire => fire.Width, static width => fire => fire with { Width = width }), Envelope));
    public static readonly FireParameter Height = new("height", new StateParameter<Fire>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Fire, ShortSideExtent>.New(static fire => fire.Height, static height => fire => fire with { Height = height }), Envelope));
    public static readonly FireParameter Rise = new("rise", new StateParameter<Fire>.Bounded<Drift, float, InvalidGenerator>(
        Lens<Fire, Drift>.New(static fire => fire.Rise, static rise => fire => fire with { Rise = rise }),
        Drift.Presentation with { Soft = (0f, 10f) }));
    public static readonly FireParameter Evolution = new("evolution", new StateParameter<Fire>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Fire, Frequency>.New(static fire => fire.Evolution, static evolution => fire => fire with { Evolution = evolution }),
        Frequency.Presentation with { Soft = (-4f, 4f) }));
    public static readonly FireParameter Density = new("density", new StateParameter<Fire>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Fire, AxisFraction>.New(static fire => fire.Density, static density => fire => fire with { Density = density }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly FireParameter Cool = new("cool", new StateParameter<Fire>.Bounded<ColorTemperature, double, InvalidColor>(
        Lens<Fire, ColorTemperature>.New(static fire => fire.Cool, static cool => fire => fire with { Cool = cool }), Emission.Presentation));
    public static readonly FireParameter Hot = new("hot", new StateParameter<Fire>.Bounded<ColorTemperature, double, InvalidColor>(
        Lens<Fire, ColorTemperature>.New(static fire => fire.Hot, static hot => fire => fire with { Hot = hot }), Emission.Presentation));
    public static readonly FireParameter Mode = new("mode", Layer.Mode);
    public static readonly FireParameter Exposure = new("exposure", Layer.Exposure);
    public static readonly FireParameter Seed = new("seed", new StateParameter<Fire>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Fire, Seed>.New(static fire => fire.Seed, static seed => fire => fire with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FireParameter Clock = new("clock", Time.Clock);
    public static readonly FireParameter Pace = new("pace", Time.Pace);

    public StateParameter<Fire> Kind { get; }
}

public sealed record Fog(
    NoiseBasis Basis, ShortSideExtent Scale, Drift DriftX, Drift DriftY, Frequency Evolution, Mix Variation,
    FogDistance Start, FogAttenuation Attenuation, Option<FogDistance> Cutoff, Option<ShortSideLength> Diffusion,
    Ramp Colors, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Fog, FogParameter, InvalidGenerator>, IPixelStage<Fog> {
    public static Fog Default { get; } = new(
        NoiseBasis.Standard with { Dimensions = NoiseDimensions.Four, Octaves = Octaves.Standard with { Detail = FractalDetail.Create(4f), Roughness = AxisFraction.Create(0.2f) } },
        Looks.ShortSide, Drift.Neutral, Drift.Neutral, Frequency.Neutral, Mix.MinValue,
        FogDistance.Neutral, FogAttenuation.Standard, None, None, Looks.Ramp(RampInterpolation.Linear, (0d, new Vector3(MathF.Pow(0.85098f, 2.2f)), 1f)),
        GeneratedLayer.Mixed, Seed.MinValue, Timing.Standard);

    public static Fog Haze { get; } = Default with {
        Attenuation = FogAttenuation.MinValue,
        Diffusion = Some(Valid.Value(ShortSideLength.Validate(0.75f * ReferenceFrame.GreaterSide, provider: null, out ShortSideLength diffusion), diffusion)),
        Colors = Looks.Ramp(RampInterpolation.Linear, (0d, new Vector3(0.18f), 1f)),
    };

    public static Seq<GuideChannel> Channels(Fog state) => [GuideChannel.Depth];

    public static Option<PixelPass> Pass(Fog state, PassContext context) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        from camera in context.Camera
        select state.Diffusion.Match<PixelPass>(
            Some: radius => new PixelPass.Frame((frame, progress) => Diffused(state, context, depth, camera, radius, frame, progress)),
            None: () => new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context, depth, camera, static (_, _) => Vector3.One))));

    private static Fin<Unit> Diffused(Fog state, PassContext context, PixelFrame depth, Camera camera, ShortSideLength diffusion, PixelFrame frame, IProgress<int> progress) {
        float radius = diffusion.Pixels(context.Extent);
        int taps = (2 * (int)float.Ceiling(radius)) + 1;
        PixelFrame spread = new(frame.Origin, frame.Size, frame.Extent, static _ => { });
        using Mat source = frame.Header();
        using Mat target = spread.Header();
        CvInvoke.GaussianBlur(source, target, new Size(taps, taps), radius / 3f, radius / 3f, BorderType.Replicate);
        return new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context, depth, camera, (column, line) => spread.Row(line)[column - spread.Origin.X].AsVector3())))
            .Run(frame, progress);
    }

    private static Action<Span<Vector4>, int, int> Fill(Fog state, PassContext context, PixelFrame depth, Camera camera, Func<int, int, Vector3> light) {
        FieldFrame frame = Placement.Centered.Frame(context.Extent);
        uint field = CoordinateHash.Field(NoiseStream.Fog, state.Seed, 0u);
        float t = state.Timing.At(context);
        (NoiseSampler basis, RampTable table) = (state.Basis.Sampler, state.Colors.Tabulate(context.Working));
        (Vector3 axis, Vector2 drift) = (Vector3.Transform(-Vector3.UnitZ, camera.Orientation), new Vector2(state.DriftX, state.DriftY) * t);
        (float scale, float evolution, float start, float attenuation, float variation, float height) =
            (state.Scale, state.Evolution * t, state.Start, state.Attenuation, state.Variation, context.Extent.Height);
        bool varied = state.Variation != Mix.MinValue;
        Func<float, bool> beyond = state.Cutoff.Match(
            Some: static cutoff => (Func<float, bool>)(distance => distance > cutoff),
            None: static () => static _ => false);
        return (row, column, line) => {
            Span<Vector4> guide = depth.Row(line);
            for (int i = 0; i < row.Length; i++) {
                float d = guide[column + i - depth.Origin.X].X / Vector3.Dot(camera.Ray(new Vector2(column + i + 0.5f, height - line - 0.5f)).Direction, axis);
                float p = float.MinNumber(float.MaxNumber(d - start, 0f), FogDistance.MaxValue);
                Vector2 q = frame.Point(column + i, line);
                float rho = varied
                    ? float.Max(1f + (variation * ((2f * basis.Sample(new Vector4((new Vector2(q.X, -q.Y) - drift) / scale, evolution, p / attenuation), field)) - 1f)), 0f)
                    : 1f;
                float transmittance = beyond(d) ? 1f : MathF.Exp(-rho * p / attenuation);
                Vector4 medium = table.Sample(1f - transmittance);
                row[i] = new Vector4(medium.AsVector3() * light(column + i, line), (1f - transmittance) * medium.W);
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
    private static readonly BasisKinds<Fog> Basis =
        NoiseBasis.Kinds(Lens<Fog, NoiseBasis>.New(static fog => fog.Basis, static basis => fog => fog with { Basis = basis }));
    private static readonly (StateParameter<Fog> Mode, StateParameter<Fog> Exposure) Layer =
        GeneratedLayer.Kinds(Lens<Fog, GeneratedLayer>.New(static fog => fog.Layer, static layer => fog => fog with { Layer = layer }));
    private static readonly Presentation<Drift, float> Travel = Drift.Presentation with { Soft = (-1f, 1f) };

    public static readonly FogParameter BasisKind = new("basis-kind", Basis.Kind);
    public static readonly FogParameter BasisDimensions = new("basis-dimensions", Basis.Dimensions);
    public static readonly FogParameter BasisDetail = new("basis-detail", Basis.Octaves.Detail);
    public static readonly FogParameter BasisRoughness = new("basis-roughness", Basis.Octaves.Roughness);
    public static readonly FogParameter BasisLacunarity = new("basis-lacunarity", Basis.Octaves.Lacunarity);
    public static readonly FogParameter BasisDistortion = new("basis-distortion", Basis.Distortion);
    public static readonly FogParameter BasisMetric = new("basis-metric", Basis.Metric);
    public static readonly FogParameter BasisExponent = new("basis-exponent", Basis.Exponent);
    public static readonly FogParameter BasisRandomness = new("basis-randomness", Basis.Randomness);
    public static readonly FogParameter BasisOffset = new("basis-offset", Basis.Offset);
    public static readonly FogParameter BasisGain = new("basis-gain", Basis.Gain);
    public static readonly FogParameter BasisSmoothness = new("basis-smoothness", Basis.Smoothness);
    public static readonly FogParameter BasisFrequency = new("basis-frequency", Basis.Frequency);
    public static readonly FogParameter BasisAnisotropy = new("basis-anisotropy", Basis.Anisotropy);
    public static readonly FogParameter BasisOrientation = new("basis-orientation", Basis.Orientation);
    public static readonly FogParameter BasisDepth = new("basis-depth", Basis.Depth);
    public static readonly FogParameter BasisForm = new("basis-form", Basis.Form);
    public static readonly FogParameter BasisProfile = new("basis-profile", Basis.Profile);
    public static readonly FogParameter BasisPhase = new("basis-phase", Basis.Phase);
    public static readonly FogParameter BasisDetailScale = new("basis-detail-scale", Basis.DetailScale);
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
    public static readonly FogParameter Attenuation = new("attenuation", new StateParameter<Fog>.Bounded<FogAttenuation, float, InvalidGenerator>(
        Lens<Fog, FogAttenuation>.New(static fog => fog.Attenuation, static attenuation => fog => fog with { Attenuation = attenuation }),
        new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0.3f, 1000f), Scale = TrackScale.Log }));
    public static readonly FogParameter Cutoff = new("cutoff", new StateParameter<Fog>.OptionalBounded<FogDistance, float, InvalidGenerator>(
        Lens<Fog, Option<FogDistance>>.New(static fog => fog.Cutoff, static cutoff => fog => fog with { Cutoff = cutoff }),
        new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0f, 1000f), Scale = TrackScale.Cubic }));
    public static readonly FogParameter Diffusion = new("diffusion", new StateParameter<Fog>.OptionalBounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Fog, Option<ShortSideLength>>.New(static fog => fog.Diffusion, static diffusion => fog => fog with { Diffusion = diffusion }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 2f) }));
    public static readonly FogParameter Colors = new("colors", new StateParameter<Fog>.Gradient(
        Lens<Fog, Ramp>.New(static fog => fog.Colors, static colors => fog => fog with { Colors = colors })));
    public static readonly FogParameter Mode = new("mode", Layer.Mode);
    public static readonly FogParameter Exposure = new("exposure", Layer.Exposure);
    public static readonly FogParameter Seed = new("seed", new StateParameter<Fog>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Fog, Seed>.New(static fog => fog.Seed, static seed => fog => fog with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FogParameter Clock = new("clock", Time.Clock);
    public static readonly FogParameter Pace = new("pace", Time.Pace);

    public StateParameter<Fog> Kind { get; }
}

public sealed record Starfield(
    Placement Placement, ShortSideExtent PointScale, AxisFraction PointDensity, Exposure PointExposure,
    NoiseBasis Stars, ShortSideExtent StarScale, ShortSideExtent StarRadius, Ramp StarColors, Exposure StarExposure, Mix Twinkle, Frequency TwinkleRate,
    NoiseBasis Nebula, ShortSideExtent NebulaScale, AxisFraction NebulaDensity, Ramp NebulaColors, Exposure NebulaExposure, Frequency Evolution,
    GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<Starfield, StarfieldParameter, InvalidGenerator>, IPixelStage<Starfield> {
    public static Starfield Default { get; } = new(
        Placement.Centered, Valid.Value(ShortSideExtent.Validate(1f / 100f, provider: null, out ShortSideExtent pointScale), pointScale),
        Valid.Value(AxisFraction.Validate(0.12f, provider: null, out AxisFraction pointDensity), pointDensity), Exposure.Neutral,
        NoiseBasis.Standard with {
            Kind = BasisKind.SmoothF1,
            Octaves = new(FractalDetail.Create(2.56f), AxisFraction.Create(0.165441f), FractalLacunarity.Create(2.6f)),
            Cellular = new(CellMetric.Minkowski, MinkowskiExponent.Create(0.4f), AxisFraction.MaxValue),
            CellSmoothness = Some(AxisFraction.MaxValue),
        },
        Valid.Value(ShortSideExtent.Validate(1f / 10f, provider: null, out ShortSideExtent starScale), starScale),
        Valid.Value(ShortSideExtent.Validate(1f / 10f, provider: null, out ShortSideExtent starRadius), starRadius), Ramp.Grayscale, Exposure.Neutral, Mix.MinValue, Frequency.Neutral,
        NoiseBasis.Standard with { Octaves = Octaves.Standard with { Detail = FractalDetail.Create(6f), Roughness = AxisFraction.Create(0.56f) }, Distortion = NoiseDistortion.Create(0.1f) },
        Looks.ShortSide,
        AxisFraction.Half, Ramp.Grayscale, Exposure.Neutral, Frequency.Neutral,
        GeneratedLayer.Added, Seed.MinValue, Timing.Standard);

    public static Option<PixelPass> Pass(Starfield state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context))));

    private static Action<Span<Vector4>, int, int> Fill(Starfield state, PassContext context) {
        FieldFrame frame = state.Placement.Frame(context.Extent);
        uint field = CoordinateHash.Field(NoiseStream.Starfield, state.Seed, 0u);
        float t = state.Timing.At(context);
        (NoiseSampler plain, NoiseSampler stars, NoiseSampler nebula) = (NoiseBasis.Plain.Sampler, state.Stars.Sampler, state.Nebula.Sampler);
        (RampTable starTable, RampTable nebulaTable) = (state.StarColors.Tabulate(context.Working), state.NebulaColors.Tabulate(context.Working));
        (uint points, uint glint, uint sites, uint clouds, uint draws) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        (float pointScale, float pointDensity, float starScale, float starRadius, float nebulaScale, float nebulaDensity) =
            (state.PointScale, state.PointDensity, state.StarScale, state.StarRadius, state.NebulaScale, state.NebulaDensity);
        (float pointGain, float starGain, float nebulaGain) = (state.PointExposure.Scale, state.StarExposure.Scale, state.NebulaExposure.Scale);
        (float twinkle, float phase, float evolution) = (state.Twinkle, state.TwinkleRate * t, state.Evolution * t);
        Func<Vector2, float> steady = state.Twinkle == Mix.MinValue
            ? static _ => 1f
            : at => 1f - (twinkle * plain.Sample(new Vector4(at, phase, 0f), glint));
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 local = frame.Local(frame.Point(column + i, line));
                Vector2 p = new(local.X, -local.Y);
                Vector2 dot = p / pointScale;
                float s = Easing.Saturate((plain.Sample(new Vector4(dot, 0f, 0f), points) - (1f - pointDensity)) / pointDensity);
                CellFeature cell = stars.Cell(new Vector4(p / starScale, 0f, 0f), sites);
                Vector4 draw = NoiseFunctions.White(cell.Site, draws);
                float b = draw.Y * Easing.Saturate(1f - (cell.Distance * starScale / starRadius)) * steady(new Vector2(cell.Site.X, cell.Site.Y));
                float n = nebula.Sample(new Vector4(p / nebulaScale, evolution, 0f), clouds);
                Vector4 cloud = nebulaTable.Sample(n);
                row[i] = new Vector4(
                    (s * steady(dot) * pointGain * Vector3.One) + (b * starGain * starTable.Sample(draw.X).AsVector3())
                    + (Easing.SmootherStep(Easing.Saturate((n - (1f - nebulaDensity)) / nebulaDensity)) * cloud.W * nebulaGain * cloud.AsVector3()),
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
    private static readonly (StateParameter<Starfield> CenterX, StateParameter<Starfield> CenterY, StateParameter<Starfield> Rotation) Placed =
        Placement.Kinds(Lens<Starfield, Placement>.New(static sky => sky.Placement, static placement => sky => sky with { Placement = placement }));
    private static readonly BasisKinds<Starfield> Stars =
        NoiseBasis.Kinds(Lens<Starfield, NoiseBasis>.New(static sky => sky.Stars, static stars => sky => sky with { Stars = stars }));
    private static readonly BasisKinds<Starfield> Nebula =
        NoiseBasis.Kinds(Lens<Starfield, NoiseBasis>.New(static sky => sky.Nebula, static nebula => sky => sky with { Nebula = nebula }));
    private static readonly (StateParameter<Starfield> Mode, StateParameter<Starfield> Exposure) Layer =
        GeneratedLayer.Kinds(Lens<Starfield, GeneratedLayer>.New(static sky => sky.Layer, static layer => sky => sky with { Layer = layer }));
    private static readonly Presentation<AxisFraction, float> Share = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly Presentation<Frequency, float> Rate = Frequency.Presentation with { Soft = (-4f, 4f) };

    public static readonly StarfieldParameter CenterX = new("center-x", Placed.CenterX);
    public static readonly StarfieldParameter CenterY = new("center-y", Placed.CenterY);
    public static readonly StarfieldParameter Rotation = new("rotation", Placed.Rotation);
    public static readonly StarfieldParameter PointScale = new("point-scale", new StateParameter<Starfield>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Starfield, ShortSideExtent>.New(static sky => sky.PointScale, static scale => sky => sky with { PointScale = scale }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.001f, 0.04f), Scale = TrackScale.Log }));
    public static readonly StarfieldParameter PointDensity = new("point-density", new StateParameter<Starfield>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Starfield, AxisFraction>.New(static sky => sky.PointDensity, static density => sky => sky with { PointDensity = density }), Share));
    public static readonly StarfieldParameter PointExposure = new("point-exposure", new StateParameter<Starfield>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<Starfield, Exposure>.New(static sky => sky.PointExposure, static exposure => sky => sky with { PointExposure = exposure }), Tone.Exposure.Presentation));
    public static readonly StarfieldParameter StarsKind = new("stars-kind", Stars.Kind);
    public static readonly StarfieldParameter StarsDimensions = new("stars-dimensions", Stars.Dimensions);
    public static readonly StarfieldParameter StarsDetail = new("stars-detail", Stars.Octaves.Detail);
    public static readonly StarfieldParameter StarsRoughness = new("stars-roughness", Stars.Octaves.Roughness);
    public static readonly StarfieldParameter StarsLacunarity = new("stars-lacunarity", Stars.Octaves.Lacunarity);
    public static readonly StarfieldParameter StarsDistortion = new("stars-distortion", Stars.Distortion);
    public static readonly StarfieldParameter StarsMetric = new("stars-metric", Stars.Metric);
    public static readonly StarfieldParameter StarsExponent = new("stars-exponent", Stars.Exponent);
    public static readonly StarfieldParameter StarsRandomness = new("stars-randomness", Stars.Randomness);
    public static readonly StarfieldParameter StarsOffset = new("stars-offset", Stars.Offset);
    public static readonly StarfieldParameter StarsGain = new("stars-gain", Stars.Gain);
    public static readonly StarfieldParameter StarsSmoothness = new("stars-smoothness", Stars.Smoothness);
    public static readonly StarfieldParameter StarsFrequency = new("stars-frequency", Stars.Frequency);
    public static readonly StarfieldParameter StarsAnisotropy = new("stars-anisotropy", Stars.Anisotropy);
    public static readonly StarfieldParameter StarsOrientation = new("stars-orientation", Stars.Orientation);
    public static readonly StarfieldParameter StarsDepth = new("stars-depth", Stars.Depth);
    public static readonly StarfieldParameter StarsForm = new("stars-form", Stars.Form);
    public static readonly StarfieldParameter StarsProfile = new("stars-profile", Stars.Profile);
    public static readonly StarfieldParameter StarsPhase = new("stars-phase", Stars.Phase);
    public static readonly StarfieldParameter StarsDetailScale = new("stars-detail-scale", Stars.DetailScale);
    public static readonly StarfieldParameter StarScale = new("star-scale", new StateParameter<Starfield>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Starfield, ShortSideExtent>.New(static sky => sky.StarScale, static scale => sky => sky with { StarScale = scale }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.04f, 1f), Scale = TrackScale.Log }));
    public static readonly StarfieldParameter StarRadius = new("star-radius", new StateParameter<Starfield>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Starfield, ShortSideExtent>.New(static sky => sky.StarRadius, static radius => sky => sky with { StarRadius = radius }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 1f), Scale = TrackScale.Log }));
    public static readonly StarfieldParameter StarColors = new("star-colors", new StateParameter<Starfield>.Gradient(
        Lens<Starfield, Ramp>.New(static sky => sky.StarColors, static colors => sky => sky with { StarColors = colors })));
    public static readonly StarfieldParameter StarExposure = new("star-exposure", new StateParameter<Starfield>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<Starfield, Exposure>.New(static sky => sky.StarExposure, static exposure => sky => sky with { StarExposure = exposure }), Tone.Exposure.Presentation));
    public static readonly StarfieldParameter Twinkle = new("twinkle", new StateParameter<Starfield>.Bounded<Mix, float, InvalidGrade>(
        Lens<Starfield, Mix>.New(static sky => sky.Twinkle, static twinkle => sky => sky with { Twinkle = twinkle }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly StarfieldParameter TwinkleRate = new("twinkle-rate", new StateParameter<Starfield>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Starfield, Frequency>.New(static sky => sky.TwinkleRate, static rate => sky => sky with { TwinkleRate = rate }), Rate));
    public static readonly StarfieldParameter NebulaKind = new("nebula-kind", Nebula.Kind);
    public static readonly StarfieldParameter NebulaDimensions = new("nebula-dimensions", Nebula.Dimensions);
    public static readonly StarfieldParameter NebulaDetail = new("nebula-detail", Nebula.Octaves.Detail);
    public static readonly StarfieldParameter NebulaRoughness = new("nebula-roughness", Nebula.Octaves.Roughness);
    public static readonly StarfieldParameter NebulaLacunarity = new("nebula-lacunarity", Nebula.Octaves.Lacunarity);
    public static readonly StarfieldParameter NebulaDistortion = new("nebula-distortion", Nebula.Distortion);
    public static readonly StarfieldParameter NebulaMetric = new("nebula-metric", Nebula.Metric);
    public static readonly StarfieldParameter NebulaExponent = new("nebula-exponent", Nebula.Exponent);
    public static readonly StarfieldParameter NebulaRandomness = new("nebula-randomness", Nebula.Randomness);
    public static readonly StarfieldParameter NebulaOffset = new("nebula-offset", Nebula.Offset);
    public static readonly StarfieldParameter NebulaGain = new("nebula-gain", Nebula.Gain);
    public static readonly StarfieldParameter NebulaSmoothness = new("nebula-smoothness", Nebula.Smoothness);
    public static readonly StarfieldParameter NebulaFrequency = new("nebula-frequency", Nebula.Frequency);
    public static readonly StarfieldParameter NebulaAnisotropy = new("nebula-anisotropy", Nebula.Anisotropy);
    public static readonly StarfieldParameter NebulaOrientation = new("nebula-orientation", Nebula.Orientation);
    public static readonly StarfieldParameter NebulaDepth = new("nebula-depth", Nebula.Depth);
    public static readonly StarfieldParameter NebulaForm = new("nebula-form", Nebula.Form);
    public static readonly StarfieldParameter NebulaProfile = new("nebula-profile", Nebula.Profile);
    public static readonly StarfieldParameter NebulaPhase = new("nebula-phase", Nebula.Phase);
    public static readonly StarfieldParameter NebulaDetailScale = new("nebula-detail-scale", Nebula.DetailScale);
    public static readonly StarfieldParameter NebulaScale = new("nebula-scale", new StateParameter<Starfield>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Starfield, ShortSideExtent>.New(static sky => sky.NebulaScale, static scale => sky => sky with { NebulaScale = scale }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.2f, 8f), Scale = TrackScale.Log }));
    public static readonly StarfieldParameter NebulaDensity = new("nebula-density", new StateParameter<Starfield>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Starfield, AxisFraction>.New(static sky => sky.NebulaDensity, static density => sky => sky with { NebulaDensity = density }), Share));
    public static readonly StarfieldParameter NebulaColors = new("nebula-colors", new StateParameter<Starfield>.Gradient(
        Lens<Starfield, Ramp>.New(static sky => sky.NebulaColors, static colors => sky => sky with { NebulaColors = colors })));
    public static readonly StarfieldParameter NebulaExposure = new("nebula-exposure", new StateParameter<Starfield>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<Starfield, Exposure>.New(static sky => sky.NebulaExposure, static exposure => sky => sky with { NebulaExposure = exposure }), Tone.Exposure.Presentation));
    public static readonly StarfieldParameter Evolution = new("evolution", new StateParameter<Starfield>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Starfield, Frequency>.New(static sky => sky.Evolution, static evolution => sky => sky with { Evolution = evolution }), Rate));
    public static readonly StarfieldParameter Mode = new("mode", Layer.Mode);
    public static readonly StarfieldParameter Exposure = new("exposure", Layer.Exposure);
    public static readonly StarfieldParameter Seed = new("seed", new StateParameter<Starfield>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Starfield, Seed>.New(static sky => sky.Seed, static seed => sky => sky with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly StarfieldParameter Clock = new("clock", Time.Clock);
    public static readonly StarfieldParameter Pace = new("pace", Time.Pace);

    public StateParameter<Starfield> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Emission {
    public static readonly Func<(Gamut Gamut, ColorTemperature Cool, ColorTemperature Hot), EmissionSpan> Span =
        memo(static ((Gamut Gamut, ColorTemperature Cool, ColorTemperature Hot) key) =>
            new EmissionSpan([.. toSeq(Range(0, EmissionSpan.Texels + 1)).Map(i =>
                Light(key.Gamut, Valid.Value(ColorTemperature.Validate(double.Lerp(key.Cool, key.Hot, i / (double)EmissionSpan.Texels), provider: null, out ColorTemperature kelvin), kelvin)))]));

    private static ColorTemperature Hottest { get; } = Valid.Value(ColorTemperature.Validate(12000d, provider: null, out ColorTemperature hottest), hottest);

    public static Presentation<ColorTemperature, double> Presentation { get; } = new() {
        Unit = ColorTemperature.Unit,
        Soft = (ColorTemperature.MinValue, Hottest),
        Stops = Looks.Ramp(RampInterpolation.Linear, [.. Seq(ColorTemperature.MinValue, ColorTemperature.Flame, ColorTemperature.ViewDefault, Hottest).Map(static kelvin =>
            ((kelvin - (double)ColorTemperature.MinValue) / (Hottest - (double)ColorTemperature.MinValue), Light(Gamut.StandardRgb, kelvin), 1f))]),
    };

    private static Vector3 Light(Gamut gamut, ColorTemperature kelvin) =>
        Adaptation.Blackbody(gamut, kelvin).RgbLinear switch { var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B) };
}

file static class Looks {
    public static ShortSideExtent ShortSide { get; } = Valid.Value(ShortSideExtent.Validate(1f, provider: null, out ShortSideExtent extent), extent);

    public static ShortSideExtent Wavelength(float scale) => Valid.Value(ShortSideExtent.Validate(float.Pi / (10f * scale), provider: null, out ShortSideExtent extent), extent);

    public static Ramp Ramp(RampInterpolation interpolation, params ReadOnlySpan<(double Position, Vector3 Light, float Coverage)> stops) =>
        Valid.Value(Pixels.Ramp.Validate(toSeq(stops.ToArray()).Map(static stop =>
            Valid.Value(RampStop.Validate(Valid.Value(RampPosition.Validate(stop.Position, provider: null, out RampPosition position), position), new Vector4(stop.Light, stop.Coverage), out RampStop item), item)),
            interpolation, out Ramp? ramp), ramp);
}
