using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record VortexField(int Arms, Angle Twist, ShortSideLength Radius, float Tunnel, float Spin) {
    public static VortexField Default { get; } = new(1, Angle.FromRadians(2.8d), ShortSideLength.Create(0.5f), 0.6f, 1f / float.Tau);

    public Func<Vector2, Vector4> Shape(float time) {
        (float size, float clock, float twist) = (Radius, Spin * time, (float)Twist.Radians);
        return point => {
            (float radius, float angle) = (point.Length(), float.Atan2(point.Y, point.X));
            float turns = ((((angle * Arms) + (twist * (1f - (radius / size)))) / float.Tau) + clock) switch { var whole => whole - float.Floor(whole) };
            (float sin, float cos) = float.SinCos(turns * float.Tau);
            float reach = float.Min(radius, size);
            return new Vector4(reach * cos, reach * sin, reach * Tunnel, 0f);
        };
    }
}

internal sealed record FieldEnvelope(ShortSideLength Width, ShortSideLength Height) {
    public static FieldEnvelope Default { get; } = new(ShortSideLength.Create(1f), ShortSideLength.Create(1f));

    public float Falloff(Vector2 point) => float.Max(1f - float.Hypot(point.X / Width, point.Y / Height), 0f) switch { var edge => 1f - (edge * edge) };
}

internal sealed record NoiseTexture(
    NoiseSource Noise, FieldPlacement Placement, float Warp, NoiseBasis WarpBasis, Seq<RampStop> Colors, GeneratedLayer Layer, int Seed, Timing Timing) : IPixelStage<NoiseTexture> {
    private static readonly Octaves VoronoiOctaves = new(1f, 0.5f, 2f);

    public Option<VortexField> VortexShape { get; init; }
    public Option<FieldEnvelope> Envelope { get; init; }
    public Option<Blackbody> Incandescence { get; init; }
    public Option<float> Extinction { get; init; }

    public static NoiseTexture Default { get; } = new(
        NoiseSource.Default with { ScaleX = ShortSideLength.Create(1f / 5f), ScaleY = ShortSideLength.Create(1f / 5f) }, FieldPlacement.Default, 0f,
        NoiseBasis.Wave.Default with { Octaves = new(2f, 0.186957f, 2f), Distortion = 14.9f, Phase = Angle.FromRadians(-3d), DetailScale = 1.3f },
        Presets.Grayscale, GeneratedLayer.Default, 0, Timing.Default);

    public static NoiseTexture TurbulentNoise { get; } = Default with { Noise = Default.Noise with { Evolution = 0.3f } };

    public static NoiseTexture BlockNoise { get; } = Default with { Noise = Default.Noise with { Basis = NoiseBasis.White.Default } };

    public static NoiseTexture Diamonds { get; } = Default with {
        Noise = Default.Noise with { Basis = NoiseBasis.Voronoi.Default with { Feature = VoronoiFeature.F2, Octaves = NoiseBasis.Perlin.Plain.Octaves, Randomness = 0f } },
    };

    public static NoiseTexture VoronoiNoise { get; } = TurbulentNoise with { Noise = TurbulentNoise.Noise with { Basis = NoiseBasis.Voronoi.Default with { Octaves = VoronoiOctaves } } };

    public static NoiseTexture Sparkle { get; } = VoronoiNoise with {
        Noise = VoronoiNoise.Noise with { Basis = NoiseBasis.Voronoi.Default with { Octaves = VoronoiOctaves, Exponent = 1f, Smoothness = 1f } },
    };

    public static NoiseTexture VoronoiCells { get; } = Default with {
        Noise = Default.Noise with { Basis = NoiseBasis.Voronoi.Default with { Feature = VoronoiFeature.DistanceToEdge, Octaves = NoiseBasis.Perlin.Plain.Octaves }, DriftY = -0.6f },
    };

    public static NoiseTexture Waves { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Wave.Default with { Distortion = 4f, DetailScale = 2f },
            ScaleX = ShortSideLength.Create(float.Tau / 20f),
            ScaleY = ShortSideLength.Create(float.Tau / 20f),
            DriftX = -0.3f / 20f,
        },
    };

    public static NoiseTexture LightLeak { get; } = (Fade: (Low: 0.1f, High: 0.6f), Bias: (Low: 0.225f, High: 0.775f)) switch {
        var (fade, bias) => Default with {
            Noise = NoiseSource.Default with { Basis = NoiseBasis.Perlin.Plain, ScaleX = ShortSideLength.Create(1f / 0.7f), ScaleY = ShortSideLength.Create(1f / 0.7f), Evolution = 0.5f },
            Colors = Presets.Sampled(n => new Vector4(Vector3.Lerp(new(1f, 0.7529f, 0.6584f), new(0.0437f, 0.0116f, 1f), Presets.Map(n, bias)), 1f - Presets.Map(n, fade)), 0.1f, 0.225f, 0.6f),
            Layer = GeneratedLayer.Added,
        },
    };

    public static NoiseTexture Flow { get; } = (Cover: (Low: 0.37f, High: 0.87f), Tint: (Low: 0.62f, High: 1.1f)) switch {
        var (cover, tint) => Default with {
            Noise = NoiseSource.Default with { Basis = NoiseBasis.Perlin.Default with { Octaves = new(2.48f, 0f, 12.4f), Distortion = 7.25f }, Evolution = 0.15f },
            Warp = 0.0566f,
            Colors = Presets.Sampled(n => new Vector4(Vector3.Lerp(new(0.305f, 0.129f, 1f), new(1f, 0.059f, 0.298f), DistanceField.SmootherStep(tint.Low, tint.High, n)), Presets.Map(n, cover)), 0.37f, 0.62f, 0.87f, 1f),
            Layer = GeneratedLayer.Added,
        },
    };

    public static NoiseTexture Vortex { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Perlin.Default with { Octaves = new(4f, 0.2f, 3f), Distortion = 0.5f },
            ScaleX = ShortSideLength.Create(1f / 6f),
            ScaleY = ShortSideLength.Create(1f / 6f),
            Evolution = 0.3f,
        },
        VortexShape = VortexField.Default,
        Colors = Presets.Sampled(static n => (n * ((0.8f * 2f * new Vector3(0.134f, 0.431f, 0.5f)) + new Vector3(0.2f))) switch { var tint => new Vector4(tint * tint * tint * tint, 1f) }, 0f, 0.5f, 1f),
    };

    public static NoiseTexture Fire { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Perlin.Default with { Octaves = new(4f, 0.5f, 2f), Distortion = 1f },
            ScaleX = ShortSideLength.Create(0.5f / 0.7f),
            ScaleY = ShortSideLength.Create(1f / 0.7f),
            DriftY = 5f,
        },
        Placement = FieldPlacement.Default with { Center = new(0.5f, 1f) },
        Colors = Presets.Threshold(Vector3.One, 0.17f, 1f),
        Envelope = FieldEnvelope.Default,
        Incandescence = Blackbody.Default,
        Layer = GeneratedLayer.Added,
    };

    public static NoiseTexture Smoke { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Perlin.Default with { Octaves = new(5f, 0.55f, 2f), Distortion = 0.5f },
            ScaleX = ShortSideLength.Create(0.2f * ReferenceFrame.GreaterSide),
            ScaleY = ShortSideLength.Create(1.5f * 0.2f * ReferenceFrame.GreaterSide),
            DriftY = 0.05f * ReferenceFrame.GreaterSide,
            Evolution = 0.3f,
        },
        Colors = Presets.Threshold(new Vector3(0.04f), 0.4f, 0.8f, eased: true),
        Extinction = 2f,
    };

    public static NoiseTexture Mist { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Perlin.Default with { Octaves = new(3f, 0.45f, 2f) },
            ScaleX = ShortSideLength.Create(0.5f * ReferenceFrame.GreaterSide),
            ScaleY = ShortSideLength.Create(0.5f * 0.5f * ReferenceFrame.GreaterSide),
            DriftX = 0.02f * ReferenceFrame.GreaterSide,
            Evolution = 0.1f,
        },
        Colors = Presets.Threshold(new Vector3(0.7f), 0.3f, 0.9f, eased: true),
        Extinction = 0.6f,
    };

    public static Seq<(string Name, Func<NoiseTexture, NoiseTexture> Look)> Looks { get; } = [
        (nameof(TurbulentNoise), static _ => TurbulentNoise), (nameof(BlockNoise), static _ => BlockNoise), (nameof(Diamonds), static _ => Diamonds),
        (nameof(VoronoiNoise), static _ => VoronoiNoise), (nameof(Sparkle), static _ => Sparkle), (nameof(VoronoiCells), static _ => VoronoiCells),
        (nameof(Waves), static _ => Waves), (nameof(LightLeak), static _ => LightLeak), (nameof(Flow), static _ => Flow),
        (nameof(Vortex), static _ => Vortex), (nameof(Smoke), static _ => Smoke), (nameof(Mist), static _ => Mist)];

    public static Fin<Option<PixelPass>> Pass(NoiseTexture state, PassContext context) {
        (uint field, float time) = (CoordinateHash.Branch(state.Seed, 0u, 12u), (float)state.Timing.At(context).TotalSeconds);
        Func<Vector2, Vector4> shape = state.VortexShape.Map(vortex => vortex.Shape(time)).IfNone(static point => new Vector4(point, 0f, 0f));
        Func<Vector2, float> rim = state.Envelope.Map<Func<Vector2, float>>(static envelope => envelope.Falloff).IfNone(static _ => 0f);
        Func<Vector4, Vector3> light = state.Incandescence.Map(emission => context.Derivations.Derived((context.Working, Emission: emission), static key => key.Emission.Glow(key.Working)) switch {
            var glow => new Func<Vector4, Vector3>(color => glow.Sample(color.W).AsVector3()),
        }).IfNone(static color => color.AsVector3());
        Func<float, float> opacity = state.Extinction.Map<Func<float, float>>(static depth => cover => 1f - float.Exp(-depth * cover)).IfNone(static cover => cover);
        ColorRamp colors = RampLight.Of(state.Colors, context);
        return state.Layer.Pass((state.Noise.Sampler(time, CoordinateHash.Branch(field, 0u)), Warped(state, time, field)).Apply((noise, warp) =>
            DistanceField.Fill(state.Placement.Frame(context.Extent), point =>
                colors.Sample(noise(warp(shape(point))) - rim(point)) switch { var color => new Vector4(light(color), opacity(color.W)) })).As());
    }

    private static Fin<Func<Vector4, Vector4>> Warped(NoiseTexture state, float time, uint field) =>
        state.Warp is 0f ? new Func<Vector4, Vector4>(static at => at)
            : (state.Noise with { Basis = state.WarpBasis }, 2f * state.Warp) switch {
                var (source, gain) => (source.Sampler(time, CoordinateHash.Branch(field, 1u)), source.Sampler(time, CoordinateHash.Branch(field, 2u)))
                    .Apply((across, along) => new Func<Vector4, Vector4>(at => at + new Vector4(gain * (across(at) - 0.5f), gain * (along(at) - 0.5f), 0f, 0f))).As(),
            };
}
