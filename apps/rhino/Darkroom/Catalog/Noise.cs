using System.Numerics;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class WaveForm {
    public static readonly WaveForm Bands = new("bands", static point => point.X);
    public static readonly WaveForm Rings = new("rings", static point => float.Hypot(point.X, point.Y));

    [UseDelegateFromConstructor]
    public partial float Periods(Vector4 point);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Perlin), nameof(Perlin))]
[JsonDerivedType(typeof(Voronoi), nameof(Voronoi))]
[JsonDerivedType(typeof(Gabor), nameof(Gabor))]
[JsonDerivedType(typeof(Magic), nameof(Magic))]
[JsonDerivedType(typeof(Wave), nameof(Wave))]
[JsonDerivedType(typeof(White), nameof(White))]
public abstract partial record NoiseBasis {
    public abstract Fin<Func<Vector4, uint, (float Value, Vector4 Site)>> Sampler { get; }

    public abstract Fin<(float Lower, float Upper)> Range { get; }

    // --- [CASES]
    public sealed record Perlin(NoiseDimensions Dimensions, Octaves Octaves, float Distortion, Fractal Fractal, float Offset, float Gain) : NoiseBasis {
        public static Perlin Default { get; } = new(NoiseDimensions.Three, new Octaves(2f, 0.5f, 2f), 0f, Fractal.Fbm, 0f, 1f);
        public static Perlin Plain { get; } = Default with { Octaves = Default.Octaves with { Detail = 0f } };

        public override Fin<Func<Vector4, uint, (float Value, Vector4 Site)>> Sampler =>
            Fractal.Map(fbm: (Scale: 0.5f, Shift: 0.5f), multifractal: (Scale: 1f, Shift: 0f), heteroTerrain: (Scale: 1f, Shift: 0f), hybridMultifractal: (Scale: 1f, Shift: 0f), ridgedMultifractal: (Scale: 1f, Shift: 0f)) switch {
                var (scale, shift) => new Func<Vector4, uint, (float Value, Vector4 Site)>((point, hash) =>
                    (shift + (scale * Fractal.Sample(Dimensions, Dimensions.Distort(point, Distortion, CoordinateHash.Branch(hash, 0u)), Octaves, Offset, Gain, CoordinateHash.Branch(hash, 1u))), point)),
            };

        public override Fin<(float Lower, float Upper)> Range =>
            Fractal.Bound(Dimensions, Octaves, Offset, Gain) switch {
                var bound => Fractal.Map(fbm: (0.5f - (0.5f * bound), 0.5f + (0.5f * bound)), multifractal: (-bound, bound), heteroTerrain: (-bound, bound), hybridMultifractal: (-bound, bound), ridgedMultifractal: (0f, bound)),
            };
    }

    public sealed record Voronoi(NoiseDimensions Dimensions, Octaves Octaves, VoronoiFeature Feature, float Exponent, float Randomness, float Smoothness) : NoiseBasis {
        public static Voronoi Default { get; } = new(NoiseDimensions.Three, Perlin.Default.Octaves, VoronoiFeature.F1, 2f, 1f, 0f);

        public override Fin<Func<Vector4, uint, (float Value, Vector4 Site)>> Sampler =>
            Advancing.Map(_ => new Func<Vector4, uint, (float Value, Vector4 Site)>((point, hash) =>
                Feature.Sample(Dimensions, point, Octaves, Exponent, Randomness, Smoothness, hash)));

        public override Fin<(float Lower, float Upper)> Range =>
            Advancing.Map(_ => Feature.Range(Dimensions, Octaves, Exponent, Randomness, Smoothness));

        private Fin<Unit> Advancing => DarkroomFault.Unless(!Octaves.Degenerate, DarkroomReason.Octaves, Octaves);
    }

    public sealed record Gabor(NoiseDimensions Dimensions, float Frequency, float Anisotropy, Angle Orientation) : NoiseBasis {
        public static Gabor Default { get; } = new(NoiseDimensions.Three, 2f, 1f, Angle.FromDegrees(45d));

        public override Fin<Func<Vector4, uint, (float Value, Vector4 Site)>> Sampler =>
            (float)Orientation.Radians switch {
                var orientation => new Func<Vector4, uint, (float Value, Vector4 Site)>((point, hash) => (0.5f + (0.5f * Dimensions.Gabor(point, Frequency, Anisotropy, orientation, hash).Y), point)),
            };

        public override Fin<(float Lower, float Upper)> Range => (0.5f - (0.5f * Dimensions.GaborBound), 0.5f + (0.5f * Dimensions.GaborBound));
    }

    public sealed record Magic(MagicNoise Noise) : NoiseBasis {
        public static Magic Default { get; } = new(MagicNoise.Create(2, 0f));

        public override Fin<Func<Vector4, uint, (float Value, Vector4 Site)>> Sampler =>
            Noise switch { var noise => new Func<Vector4, uint, (float Value, Vector4 Site)>((point, _) => (Vector3.Sum(noise.Sample(point.AsVector3())) / 3f, point)) };

        public override Fin<(float Lower, float Upper)> Range => Noise.Range;
    }

    public sealed record Wave(NoiseDimensions Dimensions, Octaves Octaves, float Distortion, WaveForm Form, WaveProfile Profile, Angle Phase, float DetailScale) : NoiseBasis {
        public static Wave Default { get; } = new(NoiseDimensions.Three, Perlin.Default.Octaves, 0f, WaveForm.Bands, WaveProfile.Sine, Angle.Zero, 1f);

        public override Fin<Func<Vector4, uint, (float Value, Vector4 Site)>> Sampler =>
            ((float)Phase.Radians / float.Tau) switch {
                var turn => new Func<Vector4, uint, (float Value, Vector4 Site)>((point, hash) => (Profile.Wave(Dimensions, Form.Periods(point) - point.Z + turn, new Vector4(point.X, point.Y, 0f, 0f), Distortion, Octaves, DetailScale, hash), point)),
            };

        public override Fin<(float Lower, float Upper)> Range => (0f, 1f);
    }

    public sealed record White(NoiseDimensions Dimensions) : NoiseBasis {
        public static White Default { get; } = new(NoiseDimensions.Three);

        public override Fin<Func<Vector4, uint, (float Value, Vector4 Site)>> Sampler =>
            new Func<Vector4, uint, (float Value, Vector4 Site)>((point, hash) => (Dimensions.White(point, hash).X, point));

        public override Fin<(float Lower, float Upper)> Range => (0f, 1f);
    }
}

public sealed record NoiseSource(NoiseBasis Basis, ShortSideLength ScaleX, ShortSideLength ScaleY, float DriftX, float DriftY, float Evolution) {
    public static NoiseSource Default { get; } = new(NoiseBasis.Perlin.Default, ShortSideLength.Create(1f), ShortSideLength.Create(1f), 0f, 0f, 0f);

    public Fin<Func<Vector4, float>> Sampler(float time, uint field) =>
        Basis.Sampler.Map(basis => (new Vector4(DriftX * time, DriftY * time, 0f, 0f), new Vector4(ScaleX, ScaleY, ScaleX, 1f), new Vector4(0f, 0f, Evolution * time, 0f)) switch {
            var (drift, scale, lift) => new Func<Vector4, float>(point => basis(((point - drift) / scale) + lift, field).Value),
        });
}
