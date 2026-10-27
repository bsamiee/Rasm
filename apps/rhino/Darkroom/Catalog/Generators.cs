using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text.Json.Serialization;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Darkroom.Chain;
using Emgu.CV;
using MathNet.Numerics.Integration;
using Rasm.Imaging.Color;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp.PixelFormats;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class ReferenceFrame {
    public const float Height = 1080f;
    public const float GreaterSide = 1920f / Height;
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public sealed partial class Seed {
    public static Seed MinValue { get; } = new(0);

    public uint Field(uint stream, uint term) => CoordinateHash.Field((uint)_value, term, stream);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public sealed partial class Hold {
    public static Hold MinValue { get; } = new(0.001f);

    public uint Period(float clock) => (uint)Math.Floor((clock + (1d / TimeSpan.TicksPerSecond)) / _value * (1d + (1d / (1 << 22))));

    public float Held(float clock) => Period(clock) * _value;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) =>
        validationError = value is >= 0.001f and <= 8f ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"hold {value} lies outside [0.001, 8] seconds"));
}

public sealed record Timing(
    [property: Display(Name = "Time", Description = "Seconds a still draws at, recorded frames advancing from it")][property: Range(0d, 16_384d)] float Clock,
    [property: Display(Name = "Pace", Description = "Effective seconds per recorded second")][property: Range(0d, 4d)] float Pace) {
    public static Timing Default { get; } = new(0f, 1f);

    public float At(PassContext context) => context.Time.Match(Some: time => (float)((Clock + (Pace * time.TotalSeconds)) % (1 << 14)), None: Clock);
}

public sealed record Placement(
    [property: Display(Name = "Center", Description = "Layer center and rotation pivot")] FramePosition Center,
    [property: Display(Name = "Rotation", Description = "Counterclockwise turn about center")][property: Range(-double.Pi, double.Pi)] float Rotation) {
    public static Placement Default { get; } = new(FramePosition.Center, 0f);

    public FieldFrame Frame(PixelExtent extent) => new(extent, Center.Point(extent), double.SinCos(Rotation));
}

public sealed record GeneratedLayer(
    [property: Display(Name = "Blend", Description = "Mix covers frame, Add adds light")] BlendingMode Mode,
    [property: Display(Name = "Exposure", Description = "Layer brightness in stops")][property: Range(-10d, 10d)] float Exposure) {
    public static GeneratedLayer Default { get; } = new(BlendingMode.Mix, 0f);
    public static GeneratedLayer Added { get; } = Default with { Mode = BlendingMode.Add };

    public PixelPass Pass(Action<Span<Vector4>, long, long> fill, Gamut gamut) =>
        (float.Exp2(Exposure), Mode) switch {
            var (gain, mode) => new PixelPass.Pointwise((row, column, line) => {
                using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
                using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                fill(layer.Span, column, line);
                for (int i = 0; i < row.Length; i++)
                    (weights.Span[i], layer.Span[i]) = (layer.Span[i].W, new Vector4(layer.Span[i].AsVector3() * gain, 1f));
                if (mode == BlendingMode.Mix)
                    mode.Composite(row, layer.Span, weights.Span, PixelAlphaRepresentation.Unassociated, gamut);
                else {
                    mode.Mixed(row, layer.Span, weights.Span, gamut);
                    layer.Span.CopyTo(row);
                }
            }),
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Fbm), nameof(Fbm))]
[JsonDerivedType(typeof(Multifractal), nameof(Multifractal))]
[JsonDerivedType(typeof(HeteroTerrain), nameof(HeteroTerrain))]
[JsonDerivedType(typeof(HybridMultifractal), nameof(HybridMultifractal))]
[JsonDerivedType(typeof(RidgedMultifractal), nameof(RidgedMultifractal))]
[JsonDerivedType(typeof(F1), nameof(F1))]
[JsonDerivedType(typeof(F2), nameof(F2))]
[JsonDerivedType(typeof(SmoothF1), nameof(SmoothF1))]
[JsonDerivedType(typeof(EdgeDistance), nameof(EdgeDistance))]
[JsonDerivedType(typeof(Gabor), nameof(Gabor))]
[JsonDerivedType(typeof(Magic), nameof(Magic))]
[JsonDerivedType(typeof(Wave), nameof(Wave))]
[JsonDerivedType(typeof(White), nameof(White))]
public abstract partial record NoiseBasis {
    public static NoiseBasis Default { get; } = Fbm.Default;
    public static Fbm Plain => Fbm.Default with { Octaves = Fbm.Default.Octaves with { Detail = 0f } };

    public abstract Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler { get; }

    public abstract Fin<(float Lower, float Upper)> Range { get; }

    private protected virtual bool Degenerate => false;

    public Fin<(Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sample, (float Lower, float Upper) Range)> BoundedSampler => Range.Map(range => (Sampler, range));

    public Fin<Func<Vector256<double>, uint, (float Value, Vector256<double> Site)>> SiteSampler =>
        Degenerate ? new DarkroomFault("cell site sampler", DarkroomReason.Octaves) : Sampler;

    private static Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Distorted(NoiseDimensions dimensions, float distortion, Func<Vector256<double>, uint, float> sum) =>
        (point, field) => (sum(dimensions.Distort(point, distortion, CoordinateHash.Branch(field, 0u)), CoordinateHash.Branch(field, 1u)), point);

    private static Fin<(float Lower, float Upper)> Symmetric(float bound) => (-bound, bound);

    private static Fin<(float Lower, float Upper)> Centered(float bound) => (0.5f - (0.5f * bound), 0.5f + (0.5f * bound));

    // --- [CASES]
    [Display(Name = "fBM")]
    public sealed record Fbm(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Distortion", Description = "Warps noise by its own signal")][property: Range(-1000d, 1000d)] float Distortion) : NoiseBasis {
        public static new Fbm Default { get; } = new(NoiseDimensions.Three, new Octaves(2f, 0.5f, 2f), 0f);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => Distorted(Dimensions, Distortion, (point, hash) => 0.5f + (0.5f * Dimensions.Fbm(point, Octaves, hash)));

        public override Fin<(float Lower, float Upper)> Range => Centered(Dimensions.FbmBound(Octaves));
    }

    public sealed record Multifractal(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Distortion", Description = "Warps noise by its own signal")][property: Range(-1000d, 1000d)] float Distortion) : NoiseBasis {
        public static new Multifractal Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, 0f);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => Distorted(Dimensions, Distortion, (point, hash) => Dimensions.Cascade(point, Octaves, 0f, 1f, hash));

        public override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CascadeBound(Octaves, 0f, 1f));
    }

    public sealed record HeteroTerrain(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Distortion", Description = "Warps noise by its own signal")][property: Range(-1000d, 1000d)] float Distortion,
        [property: Display(Name = "Offset", Description = "Value added to each multifractal octave")][property: Range(-242d, 242d)] float Offset) : NoiseBasis {
        public static new HeteroTerrain Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, 0f, 0f);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => Distorted(Dimensions, Distortion, (point, hash) => Dimensions.Cascade(point, Octaves, Offset, 0f, hash));

        public override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CascadeBound(Octaves, Offset, 0f));
    }

    public sealed record HybridMultifractal(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Distortion", Description = "Warps noise by its own signal")][property: Range(-1000d, 1000d)] float Distortion,
        [property: Display(Name = "Offset", Description = "Value added to each multifractal octave")][property: Range(-242d, 242d)] float Offset,
        [property: Display(Name = "Gain", Description = "Weight each multifractal octave hands the next")][property: Range(0d, 1000d)] float Gain) : NoiseBasis {
        public static new HybridMultifractal Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, 0f, 0f, 1f);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler =>
            Distorted(Dimensions, Distortion, (point, hash) => Dimensions.HybridMultifractal(point, Octaves, Offset, Gain, hash));

        public override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.HybridBound(Octaves, Offset, Gain));
    }

    public sealed record RidgedMultifractal(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Distortion", Description = "Warps noise by its own signal")][property: Range(-1000d, 1000d)] float Distortion,
        [property: Display(Name = "Offset", Description = "Value added to each multifractal octave")][property: Range(-242d, 242d)] float Offset,
        [property: Display(Name = "Gain", Description = "Weight each multifractal octave hands the next")][property: Range(0d, 1000d)] float Gain) : NoiseBasis {
        public static new RidgedMultifractal Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, 0f, 0f, 1f);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler =>
            Distorted(Dimensions, Distortion, (point, hash) => Dimensions.RidgedMultifractal(point, Octaves, Offset, Gain, hash));

        public override Fin<(float Lower, float Upper)> Range => (0f, Dimensions.RidgedBound(Octaves, Offset, Gain));
    }

    public sealed record F1(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Cells", Description = "Cell distance and site placement")] VoronoiCells Cells) : NoiseBasis {
        public static new F1 Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, new VoronoiCells(CellMetric.Euclidean, 0.5f, 1f));

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => (point, hash) => Dimensions.F1(point, Octaves, Cells, 0f, hash);

        public override Fin<(float Lower, float Upper)> Range => (0f, Dimensions.CellBound(Cells, 0f, 1f));

        private protected override bool Degenerate => Octaves.Degenerate;
    }

    public sealed record F2(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Cells", Description = "Cell distance and site placement")] VoronoiCells Cells) : NoiseBasis {
        public static new F2 Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, F1.Default.Cells);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => (point, hash) => Dimensions.F2(point, Octaves, Cells, hash);

        public override Fin<(float Lower, float Upper)> Range => (0f, Dimensions.CellBound(Cells, 0f, 2f));

        private protected override bool Degenerate => Octaves.Degenerate;
    }

    [Display(Name = "Smooth F1")]
    public sealed record SmoothF1(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Cells", Description = "Cell distance and site placement")] VoronoiCells Cells,
        [property: Display(Name = "Smoothness", Description = "Blend between neighboring cells")][property: Range(0d, 1d)] float Smoothness) : NoiseBasis {
        public static new SmoothF1 Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, F1.Default.Cells, 1f);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => (point, hash) => Dimensions.F1(point, Octaves, Cells, Smoothness, hash);

        public override Fin<(float Lower, float Upper)> Range => Symmetric(Dimensions.CellBound(Cells, Smoothness, 1f));

        private protected override bool Degenerate => Octaves.Degenerate;
    }

    public sealed record EdgeDistance(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Randomness", Description = "Share of its cell each site wanders over")][property: Range(0d, 1d)] float Randomness) : NoiseBasis {
        public static new EdgeDistance Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, F1.Default.Cells.Randomness);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => (point, hash) => (Dimensions.EdgeDistance(point, Octaves, Randomness, hash), point);

        public override Fin<(float Lower, float Upper)> Range =>
            Octaves.Degenerate ? new DarkroomFault(nameof(EdgeDistance), DarkroomReason.Octaves) : Dimensions.EdgeRange(Octaves, Randomness);
    }

    public sealed record Gabor(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Frequency", Description = "Gabor cycles per noise unit")][property: Range(0.001d, 1000d)] float Frequency,
        [property: Display(Name = "Anisotropy", Description = "Share of Gabor impulses held at the orientation")][property: Range(0d, 1d)] float Anisotropy,
        [property: Display(Name = "Orientation", Description = "Base angle of Gabor impulses")][property: Range(-double.Pi, double.Pi)] float Orientation) : NoiseBasis {
        public static new Gabor Default { get; } = new(NoiseDimensions.Three, 2f, 1f, float.Pi / 4f);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler =>
            (point, hash) => (0.5f + (0.5f * Dimensions.Gabor(point, Frequency, Anisotropy, Orientation, hash).Y), point);

        public override Fin<(float Lower, float Upper)> Range => Centered(Dimensions.GaborBound);
    }

    public sealed record Magic([property: Display(Name = "Magic", Description = "Turbulence steps and self-warp of magic noise")] MagicNoise Noise) : NoiseBasis {
        public static new Magic Default { get; } = new(new MagicNoise(2, 0f));

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => (point, _) => (Vector3.Sum(Noise.Sample(point)) / 3f, point);

        public override Fin<(float Lower, float Upper)> Range => Noise.Range;
    }

    public sealed record Wave(
        [property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions,
        [property: Display(Name = "Octaves", Description = "Noise detail across scales")] Octaves Octaves,
        [property: Display(Name = "Distortion", Description = "Warps noise by its own signal")][property: Range(-1000d, 1000d)] float Distortion,
        [property: Display(Name = "Type", Description = "Bands or rings the wave draws")] WaveForm Form,
        [property: Display(Name = "Profile", Description = "Shape of each wave period")] WaveProfile Profile,
        [property: Display(Name = "Phase offset", Description = "Turn added to wave argument")][property: Range(-double.Pi, double.Pi)] float Phase,
        [property: Display(Name = "Detail scale", Description = "Distortion noise frequency relative to wave")][property: Range(0d, 1000d)] float DetailScale) : NoiseBasis {
        public static new Wave Default { get; } = new(NoiseDimensions.Three, Fbm.Default.Octaves, 0f, WaveForm.Bands, WaveProfile.Sine, 0f, 1f);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler =>
            (point, hash) => (Dimensions.Wave(point, Form, Profile, Phase, Distortion, Octaves, DetailScale, hash), point);

        public override Fin<(float Lower, float Upper)> Range => (0f, 1f);
    }

    public sealed record White([property: Display(Name = "Dimensions", Description = "Coordinate axes the noise reads")] NoiseDimensions Dimensions) : NoiseBasis {
        public static new White Default { get; } = new(NoiseDimensions.Three);

        public override Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sampler => (point, hash) => (Dimensions.White(point, hash).X, point);

        public override Fin<(float Lower, float Upper)> Range => (0f, 1f);
    }
}

public sealed record NoiseSource(
    [property: Display(Name = "Basis", Description = "Noise the field draws")] NoiseBasis Basis,
    [property: Display(Name = "Scale X", Description = "Feature width in frame short sides")][property: Range(0.01d, 2d)] ShortSideExtent ScaleX,
    [property: Display(Name = "Scale Y", Description = "Feature height in frame short sides")][property: Range(0.01d, 2d)] ShortSideExtent ScaleY,
    [property: Display(Name = "Drift X", Description = "Frame short sides per second the field moves right")][property: Range(-1d, 1d)] float DriftX,
    [property: Display(Name = "Drift Y", Description = "Frame short sides per second the field moves up")][property: Range(-1d, 1d)] float DriftY,
    [property: Display(Name = "Evolution", Description = "Noise units per second the field changes through")][property: Range(-4d, 4d)] float Evolution) {
    public static NoiseSource Default { get; } = new(NoiseBasis.Default, ShortSideExtent.Create(1f), ShortSideExtent.Create(1f), 0f, 0f, 0f);

    public Func<Vector256<double>, float> Sampler(float time, uint field) =>
        (Basis.Sampler, Vector256.Create((double)DriftX * time, (double)DriftY * time, 0d, 0d), Vector256.Create(ScaleX, ScaleY, ScaleX, 1d), Vector256.Create(0d, 0d, (double)Evolution * time, 0d)) switch {
            var (basis, drift, scale, lift) => point => basis(((point - drift) / scale) + lift, field).Value,
        };
}

public sealed record VortexField(
    [property: Display(Name = "Arms", Description = "Field repeats per turn, 0 draws concentric rings")][property: Range(0, 32)] int Arms,
    [property: Display(Name = "Twist", Description = "Turn at center, falling to 0 at vortex radius")][property: Range(-double.Pi, double.Pi)] float Twist,
    [property: Display(Name = "Radius", Description = "Distance where tunnel stops deepening in frame short sides")][property: Range(0.01d, 2d)] ShortSideExtent Radius,
    [property: Display(Name = "Tunnel", Description = "Depth sampled along vortex axis per unit radius")][property: Range(0d, 4d)] float Tunnel,
    [property: Display(Name = "Spin", Description = "Turns per second about center")][property: Range(-1d, 1d)] float Spin) {
    public static VortexField Default { get; } = new(1, 2.8f, ShortSideExtent.Create(0.5f), 0.6f, 1f / float.Tau);

    public Func<(double X, double Y), Vector256<double>> Shape(float time) =>
        ((double)Radius, (double)Spin * time) switch {
            var (size, clock) => point => (double.Hypot(point.X, point.Y), double.Atan2(point.Y, point.X)) switch {
                var (radius, angle) => ((((angle * Arms) + (Twist * (1d - (radius / size)))) / double.Tau) + clock) switch {
                    var turns => (double.SinCos((turns - double.Floor(turns)) * double.Tau), double.Min(radius, size)),
                },
            } switch {
                var ((sin, cos), reach) => Vector256.Create(reach * cos, reach * sin, reach * Tunnel, 0d),
            },
        };
}

public sealed record FieldEnvelope(
    [property: Display(Name = "Width", Description = "Envelope half width in frame short sides")][property: Range(0.1d, 4d)] ShortSideExtent Width,
    [property: Display(Name = "Height", Description = "Envelope half height in frame short sides")][property: Range(0.1d, 4d)] ShortSideExtent Height) {
    public static FieldEnvelope Default { get; } = new(ShortSideExtent.Create(1f), ShortSideExtent.Create(1f));

    public float Falloff((double X, double Y) point) => double.Max(1d - double.Hypot(point.X / Width, point.Y / Height), 0d) switch { var edge => (float)(1d - (edge * edge)) };
}

public sealed record Emission(
    [property: Display(Name = "Cool", Description = "Blackbody temperature at field low end")][property: Range(800d, 12_000d)] double Cool,
    [property: Display(Name = "Hot", Description = "Blackbody temperature at field high end")][property: Range(800d, 12_000d)] double Hot) {
    private const int Intervals = 256;

    public static Emission Default { get; } = new(800d, 2000d);

    public static Func<(Gamut Gamut, Emission Emission), Func<float, Vector3>> Span { get; } = memo<(Gamut Gamut, Emission Emission), Func<float, Vector3>>(static key => {
        Vector3[] samples = [.. Enumerable.Range(0, Intervals + 1).Select(i => Adaptation.Blackbody(key.Gamut, new(double.Lerp(key.Emission.Cool, key.Emission.Hot, i / (double)Intervals))).RgbLinear switch {
            var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B),
        })];
        return t => (float.Clamp(t, 0f, 1f) * Intervals) switch { var texel => int.Min((int)texel, Intervals - 1) switch { var i => Vector3.Lerp(samples[i], samples[i + 1], texel - i) } };
    });
}

public sealed record PointCells(
    [property: Display(Name = "Spacing", Description = "Point cell spacing in frame short sides")][property: Range(0.01d, 0.5d)] ShortSideExtent Spacing,
    [property: Display(Name = "Density", Description = "Share of cells holding a point")][property: Range(0d, 1d)] float Density,
    [property: Display(Name = "Size", Description = "Disc radius as share of largest radius fitting cell")][property: Range(0d, 1d)] float Size,
    [property: Display(Name = "Variation", Description = "Share each radius shrinks at random")][property: Range(0d, 1d)] float Variation,
    [property: Display(Name = "Brightness spread", Description = "Stops each point dims at random")][property: Range(0d, 10d)] float Spread,
    [property: Display(Name = "Softness", Description = "Share of each disc's radius fading at its edge")][property: Range(0d, 1d)] float Softness) {
    private const float Randomness = 0.5f;
    private const float Largest = (1f - Randomness) / 2f;

    public static PointCells Default { get; } = new(ShortSideExtent.Create(0.01f * ReferenceFrame.GreaterSide), 0.5f, 2f * 0.2f, 0.5f, 1f, 0.5f);

    public bool Empty => Size is 0f || Density is 0f;

    public (double Radius, double Gain) Draw(Vector4 draw) => (Size * (1d - (Variation * (double)draw.Y)) * Largest, double.Exp2(-Spread * draw.Z));

    public (float Coverage, float Value) Disc((double X, double Y) point, double side, uint field, Func<float, float> tone) {
        (double spacing, uint sites, uint colors) = (Spacing, CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        double width = 1d / (spacing * side);
        (double x, double y, double half) = (point.X / spacing, point.Y / spacing, width / 2d);
        (double Left, double Top, double Right, double Bottom) box = (x - half, y - half, x + half, y + half);
        double area = width * width;
        PointCells cells = this;
        (double coverage, double value) = Empty ? (0d, 0d) : JitteredCells.Covering(box, Largest, Randomness).Aggregate((Coverage: 0d, Value: 0d), (sum, cell) => {
            Vector4 draw = CoordinateHash.Uniforms(cell.X, cell.Y, colors);
            (double radius, double gain) = cells.Draw(draw);
            if (draw.X >= cells.Density || radius is 0d)
                return sum;
            Vector2 offset = JitteredCells.Offset(cell.X, cell.Y, Randomness, sites);
            (double siteX, double siteY) = (cell.X + offset.X, cell.Y + offset.Y);
            double weight = ShapeEdge.DiscMass(radius, cells.Softness, (box.Left - siteX, box.Top - siteY, box.Right - siteX, box.Bottom - siteY)) * gain / area;
            return (sum.Coverage + weight, sum.Value + (weight * tone(draw.X / cells.Density)));
        });
        return ((float)coverage, (float)value);
    }
}

public sealed class RampLight {
    private const int Texels = 256;

    private readonly Arr<Vector4> texels;

    private RampLight(Arr<Vector4> texels) => this.texels = texels;

    public static RampLight Of(ImmutableArray<RampStop> ramp, Gamut gamut) => new([.. toSeq(Range(0, Texels + 1)).Map(texel => Light(RampStop.At(ramp, texel / (float)Texels), gamut))]);

    public static Vector4 Light(Unicolour color, Gamut gamut) =>
        color.ConvertToConfiguration(gamut.Configuration) switch { var held => new((float)held.RgbLinear.R, (float)held.RgbLinear.G, (float)held.RgbLinear.B, (float)held.Alpha.A) };

    public Vector4 Sample(float position) =>
        float.IsNaN(position) ? new Vector4(float.NaN)
            : (float.Clamp(position, 0f, 1f) * Texels) switch { var texel => int.Min((int)texel, Texels - 1) switch { var at => Vector4.Lerp(texels[at], texels[at + 1], texel - at) } };
}

public sealed class RampFootprint {
    private const int Bins = 4096;

    private readonly Arr<Vector4> bins;
    private readonly Arr<Vector4> sums;

    private RampFootprint(Arr<Vector4> bins) =>
        (this.bins, sums) = (bins, [.. toSeq(bins).Scan((X: 0d, Y: 0d, Z: 0d, W: 0d), static (sum, bin) => (sum.X + bin.X, sum.Y + bin.Y, sum.Z + bin.Z, sum.W + bin.W))
            .Map(static sum => new Vector4((float)sum.X, (float)sum.Y, (float)sum.Z, (float)sum.W))]);

    public static RampFootprint Of(ImmutableArray<RampStop> ramp, Option<WaveProfile> profile) =>
        new([.. toSeq(Range(0, Bins)).Map(bin => RampStop.At(ramp, profile.Match(Some: wave => wave.Level((bin + 0.5f) / Bins), None: () => (bin + 0.5f) / Bins)) switch {
            var color => new Vector4(Presets.Shown(color) * (float)color.Alpha.A, (float)color.Alpha.A),
        })]);

    public Vector4 Mean(double position, double footprint, bool periodic) {
        if (double.IsPositiveInfinity(footprint))
            return Unassociated(sums[Bins] / Bins);
        (double low, double high, double total) = (position - (footprint / 2d), position + (footprint / 2d), footprint * Bins);
        if (periodic) {
            double periods = double.Floor(footprint);
            double start = low - double.Floor(low);
            double end = start + (footprint - periods);
            return Unassociated((sums[Bins] * (float)(periods / total))
                + Segment(start * Bins, double.Min(end, 1d) * Bins, total)
                + (end > 1d ? Segment(0d, (end - 1d) * Bins, total) : Vector4.Zero));
        }
        return Unassociated((low < 0d ? bins[0] * (float)((double.Min(0d, high) - low) / footprint) : Vector4.Zero)
            + (high > 1d ? bins[Bins - 1] * (float)((high - double.Max(low, 1d)) / footprint) : Vector4.Zero)
            + Segment(double.Max(low, 0d) * Bins, double.Min(high, 1d) * Bins, total));
    }

    private static Vector4 Unassociated(Vector4 mean) => mean.W > 0f ? new Vector4(mean.AsVector3() / mean.W, mean.W) : Vector4.Zero;

    private Vector4 Segment(double low, double high, double total) {
        if (low >= high)
            return Vector4.Zero;
        (int first, int last) = ((int)double.Floor(low), (int)double.Floor(high));
        return (bins[first] * (float)((double.Min(high, first + 1) - low) / total))
            + (last > first + 1 ? (sums[last] - sums[first + 1]) * (float)(1d / total) : Vector4.Zero)
            + (last > first && high > last ? bins[last] * (float)((high - last) / total) : Vector4.Zero);
    }
}

public sealed record NoiseTexture(
    [property: Display(Name = "Noise", Description = "Field pattern, feature size, and motion")] NoiseSource Noise,
    [property: Display(Name = "Placement", Description = "Field position and turn")] Placement Placement,
    [property: Display(Name = "Vortex", Description = "Swirls field around center into tunnel")] VortexField? VortexShape,
    [property: Display(Name = "Warp", Description = "Warp noise displacement in frame short sides")][property: Range(-1d, 1d)] float Warp,
    [property: Display(Name = "Warp noise", Description = "Pattern displacing the field")] NoiseBasis WarpBasis,
    [property: Display(Name = "Colors", Description = "Color and coverage by noise value")] ImmutableArray<RampStop> Colors,
    [property: Display(Name = "Envelope", Description = "Elliptical falloff shaping field")] FieldEnvelope? Envelope,
    [property: Display(Name = "Blackbody", Description = "Blackbody light between cool and hot temperatures")] Emission? Incandescence,
    [property: Display(Name = "Extinction", Description = "Optical depth at full coverage, absorbing light like smoke")][property: Range(0d, 8d)] float? Extinction,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<NoiseTexture> {
    private static readonly Octaves VoronoiOctaves = new(1f, 0.5f, 2f);

    public static NoiseTexture Default { get; } = new(
        NoiseSource.Default with { ScaleX = ShortSideExtent.Create(1f / 5f), ScaleY = ShortSideExtent.Create(1f / 5f) }, Placement.Default, VortexShape: null, 0f,
        NoiseBasis.Wave.Default with { Octaves = new(2f, 0.186957f, 2f), Distortion = 14.9f, Phase = -3f, DetailScale = 1.3f },
        Presets.Grayscale, Envelope: null, Incandescence: null, Extinction: null, GeneratedLayer.Default, Seed.MinValue, Timing.Default);

    public static NoiseTexture TurbulentNoise { get; } = Default with { Noise = Default.Noise with { Evolution = 0.3f } };

    public static NoiseTexture BlockNoise { get; } = Default with { Noise = Default.Noise with { Basis = NoiseBasis.White.Default } };

    public static NoiseTexture Diamonds { get; } = Default with {
        Noise = Default.Noise with { Basis = NoiseBasis.F2.Default with { Octaves = NoiseBasis.Plain.Octaves, Cells = NoiseBasis.F2.Default.Cells with { Randomness = 0f } } },
    };

    public static NoiseTexture VoronoiNoise { get; } = TurbulentNoise with { Noise = TurbulentNoise.Noise with { Basis = NoiseBasis.F1.Default with { Octaves = VoronoiOctaves } } };

    public static NoiseTexture Sparkle { get; } = VoronoiNoise with {
        Noise = VoronoiNoise.Noise with {
            Basis = NoiseBasis.SmoothF1.Default with { Octaves = VoronoiOctaves, Cells = NoiseBasis.F1.Default.Cells with { Metric = CellMetric.Minkowski, Exponent = 1f } },
        },
    };

    public static NoiseTexture VoronoiCells { get; } = Default with {
        Noise = Default.Noise with { Basis = NoiseBasis.EdgeDistance.Default with { Octaves = NoiseBasis.Plain.Octaves }, DriftY = -0.6f },
    };

    public static NoiseTexture Waves { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Wave.Default with { Distortion = 4f, DetailScale = 2f },
            ScaleX = ShortSideExtent.Create(float.Tau / 20f),
            ScaleY = ShortSideExtent.Create(float.Tau / 20f),
            DriftX = -0.3f / 20f,
        },
    };

    public static NoiseTexture LightLeak { get; } = (Fade: (Low: 0.1f, High: 0.6f), Bias: (Low: 0.225f, High: 0.775f)) switch {
        var (fade, bias) => Default with {
            Noise = NoiseSource.Default with { Basis = NoiseBasis.Plain, ScaleX = ShortSideExtent.Create(1f / 0.7f), ScaleY = ShortSideExtent.Create(1f / 0.7f), Evolution = 0.5f },
            Colors = Presets.Sampled(n => new Vector4(Vector3.Lerp(new(1f, 0.7529f, 0.6584f), new(0.0437f, 0.0116f, 1f), Presets.Map(n, bias)), 1f - Presets.Map(n, fade)), 0.1f, 0.225f, 0.6f),
            Layer = GeneratedLayer.Added,
        },
    };

    public static NoiseTexture Flow { get; } = (Cover: (Low: 0.37f, High: 0.87f), Tint: (Low: 0.62f, High: 1.1f)) switch {
        var (cover, tint) => Default with {
            Noise = NoiseSource.Default with { Basis = NoiseBasis.Fbm.Default with { Octaves = new(2.48f, 0f, 12.4f), Distortion = 7.25f }, Evolution = 0.15f },
            Warp = 0.0566f,
            Colors = Presets.Sampled(n => new Vector4(Vector3.Lerp(new(0.305f, 0.129f, 1f), new(1f, 0.059f, 0.298f), ShapeEdge.Smoother(Presets.Map(n, tint))), Presets.Map(n, cover)), 0.37f, 0.62f, 0.87f, 1f),
            Layer = GeneratedLayer.Added,
        },
    };

    public static NoiseTexture Vortex { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Fbm.Default with { Octaves = new(4f, 0.2f, 3f), Distortion = 0.5f },
            ScaleX = ShortSideExtent.Create(1f / 6f),
            ScaleY = ShortSideExtent.Create(1f / 6f),
            Evolution = 0.3f,
        },
        VortexShape = VortexField.Default,
        Colors = Presets.Sampled(static n => (n * ((0.8f * 2f * new Vector3(0.134f, 0.431f, 0.5f)) + new Vector3(0.2f))) switch { var tint => new Vector4(tint * tint * tint * tint, 1f) }, 0f, 0.5f, 1f),
    };

    public static NoiseTexture Fire { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Fbm.Default with { Octaves = new(4f, 0.5f, 2f), Distortion = 1f },
            ScaleX = ShortSideExtent.Create(0.5f / 0.7f),
            ScaleY = ShortSideExtent.Create(1f / 0.7f),
            DriftY = 5f,
        },
        Placement = Placement.Default with { Center = new(0.5f, 1f) },
        Colors = Presets.Threshold(Vector3.One, 0.17f, 1f),
        Envelope = FieldEnvelope.Default,
        Incandescence = Emission.Default,
        Layer = GeneratedLayer.Added,
    };

    public static NoiseTexture Smoke { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Fbm.Default with { Octaves = new(5f, 0.55f, 2f), Distortion = 0.5f },
            ScaleX = ShortSideExtent.Create(0.2f * ReferenceFrame.GreaterSide),
            ScaleY = ShortSideExtent.Create(1.5f * 0.2f * ReferenceFrame.GreaterSide),
            DriftY = 0.05f * ReferenceFrame.GreaterSide,
            Evolution = 0.3f,
        },
        Colors = Presets.Threshold(new Vector3(0.04f), 0.4f, 0.8f, eased: true),
        Extinction = 2f,
    };

    public static NoiseTexture Mist { get; } = Default with {
        Noise = NoiseSource.Default with {
            Basis = NoiseBasis.Fbm.Default with { Octaves = new(3f, 0.45f, 2f) },
            ScaleX = ShortSideExtent.Create(0.5f * ReferenceFrame.GreaterSide),
            ScaleY = ShortSideExtent.Create(0.5f * 0.5f * ReferenceFrame.GreaterSide),
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

    public static Option<PixelPass> Pass(NoiseTexture state, PassContext context) {
        (uint field, float time) = (state.Seed.Field(12u, 0u), state.Timing.At(context));
        Func<Vector256<double>, float> noise = state.Noise.Sampler(time, CoordinateHash.Branch(field, 0u));
        Func<(double X, double Y), Vector256<double>> shape = Optional(state.VortexShape).Map(vortex => vortex.Shape(time))
            .IfNone(static point => Vector256.Create(point.X, point.Y, 0d, 0d));
        Func<Vector256<double>, Vector256<double>> warp = state.Warp is 0f ? static at => at
            : (state.Noise with { Basis = state.WarpBasis }, 2d * state.Warp) switch {
                var (source, gain) => (source.Sampler(time, CoordinateHash.Branch(field, 1u)), source.Sampler(time, CoordinateHash.Branch(field, 2u))) switch {
                    var (across, along) => at => at.WithElement(0, at[0] + (gain * (across(at) - 0.5f))).WithElement(1, at[1] + (gain * (along(at) - 0.5f))),
                },
            };
        Func<(double X, double Y), float> rim = Optional(state.Envelope).Map<Func<(double X, double Y), float>>(static envelope => envelope.Falloff).IfNone(static _ => 0f);
        Func<Vector4, Vector3> light = Optional(state.Incandescence).Map<Func<Vector4, Vector3>>(emission => Emission.Span((context.Working, emission)) switch {
            var span => color => span(color.W),
        }).IfNone(static color => color.AsVector3());
        Func<float, float> opacity = Optional(state.Extinction).Map<Func<float, float>>(static depth => cover => 1f - float.Exp(-depth * cover)).IfNone(static cover => cover);
        RampLight colors = RampLight.Of(state.Colors, context.Working);
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
            colors.Sample(noise(warp(shape(point))) - rim(point)) switch { var color => new Vector4(light(color), opacity(color.W)) }), context.Working));
    }
}

public sealed record Fog(
    [property: Display(Name = "Wisps", Description = "Medium pattern, feature size, and motion")] NoiseSource Noise,
    [property: Display(Name = "Variation", Description = "Wisp density variation, 0% uniform")][property: Range(0d, 1d)] float Variation,
    [property: Display(Name = "Start", Description = "Camera distance where medium begins")][property: Range(0d, 100d)] float Start,
    [property: Display(Name = "Attenuation", Description = "Distance through medium passing 1/e of light behind it")][property: Range(0.3d, 1000d)] float? Attenuation,
    [property: Display(Name = "Cutoff", Description = "Distance past which surfaces and background stay clear")][property: Range(0d, 1000d)] float? Cutoff,
    [property: Display(Name = "Diffusion", Description = "Blur radius of frame light the medium scatters, off lights medium evenly")][property: Range(0d, 2d)] ShortSideLength? Diffusion,
    [property: Display(Name = "Density", Description = "Share of medium coverage drawn")][property: Range(0d, 1d)] float Amount,
    [property: Display(Name = "Colors", Description = "Medium color and coverage, near to far by optical depth")] ImmutableArray<RampStop> Colors,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<Fog> {
    public static Fog Default { get; } = new(
        NoiseSource.Default with { Basis = NoiseBasis.Fbm.Default with { Dimensions = NoiseDimensions.Four, Octaves = new(4f, 0.2f, 2f) } }, 0f, 0f, (float)Length.FromFeet(100).Meters,
        Cutoff: null, Diffusion: null, 1f, [new(new Unicolour(ColourSpace.Rgb, 0.85098d, 0.85098d, 0.85098d), 0f)], GeneratedLayer.Default, Seed.MinValue, Timing.Default);

    public static Fog Haze { get; } = Default with {
        Attenuation = null,
        Amount = 0.35f,
        Diffusion = ShortSideLength.Create(0.75f * ReferenceFrame.GreaterSide),
        Colors = Presets.Solid(new Vector3(Exposure.MiddleGray)),
    };

    public static Seq<(string Name, Func<Fog, Fog> Look)> Looks { get; } = [(nameof(Haze), static _ => Haze)];

    public static Seq<GuideChannel> Channels(Fog state) => state.Attenuation is null ? [] : [GuideChannel.Depth];

    public static Option<PixelPass> Pass(Fog state, PassContext context) =>
        Optional(state.Attenuation).Match(
            Some: attenuation => from depth in context.Guides.Find(GuideChannel.Depth)
                                 from camera in context.Camera
                                 select Opacity(state, context, depth, camera, attenuation),
            None: static () => Some<Func<long, long, float>>(static (_, _) => 1f))
        .Map(opacity => Optional(state.Diffusion).Match(
            Some: radius => new PixelPass.Frame((frame, progress) => frame.Filter(source => {
                PixelFrame spread = new(frame.Origin, frame.Size, frame.Extent);
                using Mat target = spread.Header();
                return source.Blur(target, radius.Pixels(context.Extent)).Bind(_ =>
                    state.Layer.Pass(Fill(state, RampLight.Of(state.Colors, context.Working), opacity, (column, line) => spread.Row(line)[(int)(column - spread.Origin.X)].AsVector3()), context.Working).Run(frame, progress));
            })),
            None: () => state.Layer.Pass(Fill(state, RampLight.Of(state.Colors, context.Working), opacity, static (_, _) => Vector3.One), context.Working)));

    private static Func<long, long, float> Opacity(Fog state, PassContext context, PixelFrame depth, Camera camera, float attenuation) {
        FieldFrame frame = Placement.Default.Frame(context.Extent);
        Func<Vector256<double>, float> noise = state.Noise.Sampler(state.Timing.At(context), state.Seed.Field(13u, 0u));
        Func<long, long, float, float> density = state.Variation is 0f ? static (_, _, _) => 1f
            : (column, line, path) => frame.Upward(column, line) switch {
                var point => float.Max(1f + (state.Variation * ((2f * noise(Vector256.Create(point.X, point.Y, 0d, path))) - 1f)), 0f),
            };
        return new Extinction(state.Start, attenuation) { Cutoff = Optional(state.Cutoff) }.Field(depth, camera, density);
    }

    private static Action<Span<Vector4>, long, long> Fill(Fog state, RampLight table, Func<long, long, float> opacity, Func<long, long, Vector3> light) =>
        (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                float coverage = opacity(column + i, line);
                Vector4 medium = table.Sample(coverage);
                row[i] = new Vector4(medium.AsVector3() * light(column + i, line), state.Amount * coverage * medium.W);
            }
        };
}

public sealed record Starfield(
    [property: Display(Name = "Placement", Description = "Field position and turn")] Placement Placement,
    [property: Display(Name = "Stars", Description = "Cellular noise placing bright stars")] NoiseBasis Stars,
    [property: Display(Name = "Star scale", Description = "Bright star cell side in frame short sides")][property: Range(0.04d, 1d)] ShortSideExtent StarScale,
    [property: Display(Name = "Star radius", Description = "Bright star reach in frame short sides")][property: Range(0.01d, 1d)] ShortSideExtent StarRadius,
    [property: Display(Name = "Star colors", Description = "Colors bright stars draw from")] ImmutableArray<RampStop> StarColors,
    [property: Display(Name = "Star exposure", Description = "Star brightness in stops")][property: Range(-10d, 10d)] float StarExposure,
    [property: Display(Name = "Twinkle", Description = "Share of star light twinkle dims, 0% steady")][property: Range(0d, 1d)] float Twinkle,
    [property: Display(Name = "Twinkle rate", Description = "Twinkle periods per second")][property: Range(-4d, 4d)] float TwinkleRate,
    [property: Display(Name = "Nebula", Description = "Nebula pattern, feature size, and motion")] NoiseSource Nebula,
    [property: Display(Name = "Nebula density", Description = "Share of noise range nebula covers")][property: Range(0d, 1d)] float NebulaDensity,
    [property: Display(Name = "Nebula colors", Description = "Nebula color and coverage by noise value")] ImmutableArray<RampStop> NebulaColors,
    [property: Display(Name = "Nebula exposure", Description = "Nebula brightness in stops")][property: Range(-10d, 10d)] float NebulaExposure,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<Starfield> {
    public static Starfield Default { get; } = new(
        Placement.Default, NoiseBasis.SmoothF1.Default with { Octaves = new(2.56f, 0.165441f, 2.6f), Cells = new VoronoiCells(CellMetric.Minkowski, 0.4f, 1f) },
        ShortSideExtent.Create(0.1f), ShortSideExtent.Create(0.1f), Presets.Grayscale, 0f, 0f, 0f,
        NoiseSource.Default with { Basis = NoiseBasis.Fbm.Default with { Octaves = new(6f, 0.56f, 2f), Distortion = 0.1f } },
        0.5f, Presets.Grayscale, 0f, GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(Starfield state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Stars.SiteSampler.Bind(stars => Drawn(state, context, stars).Run(frame, progress))));

    private static PixelPass Drawn(Starfield state, PassContext context, Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> stars) {
        (RampLight starTable, RampLight nebulaTable) = (RampLight.Of(state.StarColors, context.Working), RampLight.Of(state.NebulaColors, context.Working));
        (uint field, float time) = (state.Seed.Field(14u, 0u), state.Timing.At(context));
        (uint glint, uint sites, uint clouds, uint draws) = (CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        (float gain, double scale, double reach) = (float.Exp2(state.StarExposure), state.StarScale, (double)state.StarScale / state.StarRadius);
        Func<Vector256<double>, float> steady = state.Twinkle is 0f ? static _ => 1f
            : ((NoiseBasis.Plain with { Dimensions = NoiseDimensions.Four }).Sampler, (double)state.TwinkleRate * time, state.Twinkle) switch {
                var (plain, phase, depth) => site => 1f - (depth * plain(site.WithElement(3, site[3] + phase), glint).Value),
            };
        Func<(double X, double Y), Vector3> nebula = state.NebulaDensity is 0f ? static _ => Vector3.Zero
            : (state.Nebula.Sampler(time, clouds), float.Exp2(state.NebulaExposure), state.NebulaDensity) switch {
                var (sample, light, density) => point => sample(Vector256.Create(point.X, point.Y, 0d, 0d)) switch {
                    var n => ShapeEdge.Smoother(Presets.Map(n, (1f - density, 1f))) * nebulaTable.Sample(n) switch { var cloud => cloud.W * light * cloud.AsVector3() },
                },
            };
        return state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point => {
            (float value, Vector256<double> site) = stars(Vector256.Create(point.X / scale, point.Y / scale, 0d, 0d), sites);
            Vector4 draw = CoordinateHash.White(site, draws);
            return new Vector4((draw.Y * (float)double.Clamp(1d - (value * reach), 0d, 1d) * steady(site) * gain * starTable.Sample(draw.X).AsVector3()) + nebula(point), 1f);
        }), context.Working);
    }
}

public sealed record ParticleField(
    [property: Display(Name = "Population", Description = "Mote spacing, presence, size, and light")] PointCells Cells,
    [property: Display(Name = "Layers", Description = "Mote depth layers")][property: Range(1, 8)] int Layers,
    [property: Display(Name = "Heading", Description = "Travel direction, counterclockwise from right")][property: Range(-double.Pi, double.Pi)] float Heading,
    [property: Display(Name = "Speed", Description = "Frame short sides per second the nearest layer travels")][property: Range(0d, 2d)] float Speed,
    [property: Display(Name = "Wind", Description = "Noise displacing particles")] NoiseBasis Basis,
    [property: Display(Name = "Turbulence", Description = "Wind sway of each particle in frame short sides")][property: Range(0d, 0.2d)] ShortSideLength Turbulence,
    [property: Display(Name = "Eddy size", Description = "Wind feature width in frame short sides")][property: Range(0.01d, 2d)] ShortSideExtent Eddy,
    [property: Display(Name = "Evolution", Description = "Noise units per second the wind changes through")][property: Range(-4d, 4d)] float Evolution,
    [property: Display(Name = "Depth", Description = "Farthest layer distance over nearest")][property: Range(1d, 4d)] float Depth,
    [property: Display(Name = "Depth variation", Description = "Depth variation within each layer")][property: Range(0d, 1d)] float DepthVariation,
    [property: Display(Name = "Focus", Description = "Depth in focus, 0% nearest layer, 100% farthest")][property: Range(0d, 1d)] float Focus,
    [property: Display(Name = "Defocus", Description = "Largest bokeh radius in frame short sides")][property: Range(0d, 0.05d)] ShortSideLength Defocus,
    [property: Display(Name = "Aperture", Description = "Lens opening shaping bokeh and diffraction spikes")] Iris Aperture,
    [property: Display(Name = "Color shift", Description = "Red and blue bokeh sizes apart from green")][property: Range(-1d, 1d)] float Fringe,
    [property: Display(Name = "Shutter", Description = "Seconds shutter stays open, particles streaking along travel")][property: Range(0d, 1d / 24d)] float Shutter,
    [property: Display(Name = "Fade", Description = "Streak dimming toward tail")][property: Range(0d, 1d)] float Fade,
    [property: Display(Name = "Covering", Description = "Particles cover light behind them")] bool Covering,
    [property: Display(Name = "Coating", Description = "Covering colors and opacity each particle draws from")] ImmutableArray<RampStop> Coating,
    [property: Display(Name = "Radiance", Description = "Emitted particle brightness in stops, independent of covering")][property: Range(-10d, 10d)] float? Radiance,
    [property: Display(Name = "Colors", Description = "Colors each particle draws from")] ImmutableArray<RampStop> Colors,
    [property: Display(Name = "Blackbody", Description = "Blackbody light between cool and hot temperatures")] Emission Emission,
    [property: Display(Name = "Incandescence", Description = "Share of particle color taken from blackbody light")][property: Range(0d, 1d)] float Incandescence,
    [property: Display(Name = "Twinkle", Description = "Share of particle light varying over time")][property: Range(0d, 1d)] float Twinkle,
    [property: Display(Name = "Twinkle rate", Description = "Twinkle periods per second")][property: Range(-4d, 4d)] float TwinkleRate,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<ParticleField> {
    private static readonly GaussLegendreRule Shutters = new(0d, 1d, 32);

    public static ParticleField Default { get; } = new(
        new(ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f / 10f), 1f, 2f * 0.2f, 0.5f, 1f, 1f / 1.8f), 4, float.Atan2(0.5f, 0.4f), 24f * 0.05f * 0.5f * float.Hypot(0.4f, 0.5f) * (ReferenceFrame.GreaterSide / 2f),
        NoiseBasis.Default, ShortSideLength.Create(0.12f / 2f * (ReferenceFrame.GreaterSide / 2f)), ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f / 1.6f), 1.6f * (24f * 0.05f * 0.5f) / 3200f, 4f, 1f, 1f,
        ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide), Iris.Create(6, double.Pi / 3d, 0d, 0d, 1d), 0f, 0.5f / 24f, 0f, Covering: false, Presets.White, 0f, Presets.White, Emission.Default,
        0f, 0f, 0f, GeneratedLayer.Added with { Exposure = 2f }, Seed.MinValue, Timing.Default);

    public static ParticleField Rain { get; } = Default with {
        Cells = PointCells.Default,
        Heading = -float.Pi / 2f,
        Speed = 2.5f * ReferenceFrame.GreaterSide,
        Turbulence = ShortSideLength.Zero,
        Aperture = Iris.Create(6, double.Pi / 3d, 1d, 0d, 1d),
        Layer = GeneratedLayer.Default,
        Covering = true,
        Radiance = null,
    };

    public static ParticleField Snow { get; } = Rain with {
        Cells = new(ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide), 0.5f, 2f * 0.3f, 0.6f, 1.3f, 0.6f),
        Speed = 0.3f * ReferenceFrame.GreaterSide,
        Turbulence = ShortSideLength.Create(0.02f / 2f * ReferenceFrame.GreaterSide),
        Colors = Presets.Solid(new Vector3(0.8f)),
        Coating = Presets.Solid(new Vector3(0.8f)),
    };

    public static ParticleField Sparks { get; } = Default with {
        Cells = Default.Cells with { Density = 0.3f, Size = 2f * 0.1f, Variation = 0.8f, Spread = 2.3f, Softness = 0.3f },
        Heading = float.Pi / 2f,
        Speed = 0.4f * ReferenceFrame.GreaterSide,
        Turbulence = ShortSideLength.Create(0.03f / 2f * ReferenceFrame.GreaterSide),
        Emission = Emission.Default with { Cool = 1000d },
        Incandescence = 1f,
    };

    public static ParticleField CellRain { get; } = Default with {
        Covering = true,
        Radiance = null,
        Cells = Default.Cells with { Variation = 0f, Spread = 0f, Softness = 0f },
        Heading = -float.Pi / 2f,
        Defocus = ShortSideLength.Zero,
        Aperture = Iris.Create(4, double.Pi / 4d, 0d, 0d, 1d),
    };

    public static ParticleField Starfield { get; } = Rain with {
        Cells = new(ShortSideExtent.Create(0.005f * ReferenceFrame.GreaterSide), 0.12f, 2f * 0.1f, 0.5f, 4f, 1f),
        Layers = 1,
        Depth = 1f,
        Speed = 0f,
        Defocus = ShortSideLength.Zero,
        Emission = new(3000d, 12_000d),
        Incandescence = 1f,
        Layer = GeneratedLayer.Added,
        Covering = false,
        Radiance = 0f,
    };

    public static Seq<(string Name, Func<ParticleField, ParticleField> Look)> Looks { get; } = [
        (nameof(Rain), static _ => Rain), (nameof(Snow), static _ => Snow), (nameof(Sparks), static _ => Sparks), (nameof(CellRain), static _ => CellRain), (nameof(Starfield), static _ => Starfield)];

    public static Option<PixelPass> Pass(ParticleField state, PassContext context) =>
        state.Cells.Empty || state.Aperture.Obstruction is 1d || (!state.Covering && state.Radiance is null) ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from bounded in state.Turbulence == ShortSideLength.Zero ? (Sample: state.Basis.Sampler, Range: (Lower: 0f, Upper: 0f)) : state.Basis.BoundedSampler
                from drawn in Drawn(state, context, frame, bounded).Run(frame, progress)
                select drawn));

    private static PixelPass Drawn(ParticleField state, PassContext context, PixelFrame frame, (Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> Sample, (float Lower, float Upper) Range) bounded) {
        (Iris iris, RampLight colors, RampLight coating) = (state.Aperture, RampLight.Of(state.Colors, context.Working), RampLight.Of(state.Coating, context.Working));
        (PointCells cells, int side, double time, double shutter) = (state.Cells, context.Extent.ShortSide, state.Timing.At(context), state.Shutter);
        (double X, double Y) middle = (context.Extent.Width / 2d, context.Extent.Height / 2d);
        Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> plain = (NoiseBasis.Plain with { Dimensions = NoiseDimensions.Four }).Sampler;
        uint field = state.Seed.Field(15u, 0u);
        (uint presence, uint tone, uint across, uint down, uint twinkle) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        Func<float, Vector3> thermal = Emission.Span((context.Working, state.Emission));
        (double focus, double radiance) = (double.Pow(state.Depth, -(double)state.Focus), Optional(state.Radiance).Map(static exposure => (double)float.Exp2(exposure)).IfNone(0d));
        (double X, double Y) heading = double.SinCos(state.Heading) switch { var (sin, cos) => ((double)state.Speed * cos, (double)state.Speed * -sin) };
        (double lowWind, double highWind) = ((2d * bounded.Range.Lower) - 1d, (2d * bounded.Range.Upper) - 1d);
        Mote[] ordered = [.. Enumerable.Range(0, state.Layers).SelectMany(Layer).OrderByDescending(static mote => mote.Depth).ThenBy(static mote => mote.Layer).ThenBy(static mote => mote.Line).ThenBy(static mote => mote.Column)];
        return state.Layer.Pass((row, column, line) => {
            long y = context.Extent.Height - 1L - line;
            for (int x = 0; x < row.Length; x++) {
                Vector256<double> integrated = Vector256<double>.Zero;
                for (int sample = 0; sample < Shutters.Order; sample++) {
                    double u = Shutters.GetAbscissa(sample);
                    double tau = time - (shutter * (1d - u));
                    double weight = Shutters.GetWeight(sample) * (1d - state.Fade + (state.Fade * u)) / (1d - (state.Fade / 2d));
                    integrated += Spatial(column + x, y, tau) * weight;
                }
                row[x] = new((float)integrated[0], (float)integrated[1], (float)integrated[2], (float)integrated[3]);
            }
        }, context.Working);

        IEnumerable<Mote> Layer(int layer) {
            double middleDepth = (layer + 0.5d) / state.Layers;
            double depthWidth = state.DepthVariation / 2d / state.Layers;
            (double reference, double nearest, double farthest) =
                (double.Pow(state.Depth, -middleDepth), double.Pow(state.Depth, -(middleDepth - depthWidth)), double.Pow(state.Depth, -(middleDepth + depthWidth)));
            double cell = cells.Spacing * reference;
            double support = Grown(side * ((cells.Spacing * nearest * cells.Draw(Vector4.Zero).Radius)
                + (state.Defocus * double.Max(double.Abs(nearest - focus), double.Abs(farthest - focus))))) / side;
            double[] wind = [lowWind * nearest, lowWind * farthest, highWind * nearest, highWind * farthest];
            (double windLow, double windHigh) = (wind.Min() * state.Turbulence, wind.Max() * state.Turbulence);
            ((double Low, double High) horizontal, (double Low, double High) vertical) = (Travel(heading.X, nearest, farthest), Travel(heading.Y, nearest, farthest));
            (long left, long top, long right, long bottom) =
                (Lattice(frame.Window.Left, middle.X, horizontal.High + windHigh + support, cell) - 1L,
                    Lattice(frame.Window.Top, middle.Y, vertical.High + windHigh + support, cell) - 1L,
                    Lattice(frame.Window.Right, middle.X, horizontal.Low + windLow - support, cell),
                    Lattice(frame.Window.Bottom, middle.Y, vertical.Low + windLow - support, cell));
            for (long line = top; line <= bottom; line++) {
                for (long column = left; column <= right; column++) {
                    Vector256<double> point = Vector256.Create(column, line, layer, 0d);
                    Vector4 position = CoordinateHash.White(point, presence);
                    if (position.X < cells.Density)
                        yield return Placed(layer, middleDepth, cell, (column, line), point, position);
                }
            }
        }

        Mote Placed(int layer, double middleDepth, double cell, (long Column, long Line) site, Vector256<double> point, Vector4 position) {
            (Vector4 draw, double depth) = (CoordinateHash.White(point, tone), middleDepth + ((position.W - 0.5d) * state.DepthVariation / state.Layers));
            (double scale, (double radius, double gain)) = (double.Pow(state.Depth, -depth), cells.Draw(draw));
            double core = side * (double)cells.Spacing * scale * radius;
            double optical = core + (side * (double)state.Defocus * double.Abs(scale - focus));
            Vector4 spectrum = colors.Sample(draw.X);
            Vector3 tint = Vector3.Lerp(Vector3.One, thermal(draw.X), state.Incandescence);
            Vector256<double> area = iris.Integral(optical, cells.Softness * optical, state.Fringe);
            double energy = radiance * spectrum.W * gain * double.Pi * core * core;
            Vector256<double> light = Vector256.Create(area[0] is 0d ? 0d : spectrum.X * (double)tint.X * energy / area[0],
                area[1] is 0d ? 0d : spectrum.Y * (double)tint.Y * energy / area[1], area[2] is 0d ? 0d : spectrum.Z * (double)tint.Z * energy / area[2], 0d);
            return new(depth, layer, site.Column, site.Line, point, ((site.Column + (double)position.Y) * cell, (site.Line + (double)position.Z) * cell),
                scale, optical, gain, state.Covering ? coating.Sample(draw.X) : Vector4.Zero, light);
        }

        long Lattice(long pixel, double center, double displacement, double cell) => (long)double.Floor((((pixel - center) / side) - displacement) / cell);

        (double Low, double High) Travel(double rate, double near, double far) {
            double[] endpoints = [rate * near * time, rate * far * time, rate * near * (time - shutter), rate * far * (time - shutter)];
            return (endpoints.Min(), endpoints.Max());
        }

        double Grown(double radius) => (iris.Circumradius * radius) + (cells.Softness * radius / 2d);

        (double X, double Y) Moved(Mote mote, double tau) {
            (double X, double Y) = (mote.Rest.X + (heading.X * mote.Scale * tau), mote.Rest.Y + (heading.Y * mote.Scale * tau));
            if (state.Turbulence == ShortSideLength.Zero)
                return (middle.X + (X * side), middle.Y + (Y * side));
            double size = mote.Scale * state.Eddy;
            Vector256<double> sample = Vector256.Create(X / size, Y / size, (double)state.Evolution * tau, 0d);
            double gain = state.Turbulence * mote.Scale;
            return (middle.X + ((X + (gain * ((2d * bounded.Sample(sample, across).Value) - 1d))) * side),
                middle.Y + ((Y + (gain * ((2d * bounded.Sample(sample, down).Value) - 1d))) * side));
        }

        Vector256<double> Spatial(long column, long line, double tau) {
            (double left, double top, double right, double bottom) = (column, line, column + 1L, line + 1L);
            (Mote Mote, (double X, double Y) Point, double LowX, double HighX, double LowY, double HighY, double Modulation)[] active = [..
                from mote in ordered
                let point = Moved(mote, tau)
                let reach = Grown(mote.Radius)
                let lowX = double.Max(left, point.X - reach)
                let highX = double.Min(right, point.X + reach)
                let lowY = double.Max(top, point.Y - reach)
                let highY = double.Min(bottom, point.Y + reach)
                where lowX < highX && lowY < highY
                let modulation = 1d - (state.Twinkle * plain(mote.Point.WithElement(3, (double)state.TwinkleRate * tau), twinkle).Value)
                select (Mote: mote, Point: point, LowX: lowX, HighX: highX, LowY: lowY, HighY: highY, Modulation: modulation)];
            double[] xs = [.. new SortedSet<double>(active.SelectMany(static item => new[] { item.LowX, item.HighX }).Append(left).Append(right))];
            double[] ys = [.. new SortedSet<double>(active.SelectMany(static item => new[] { item.LowY, item.HighY }).Append(top).Append(bottom))];
            Vector256<double> total = Vector256<double>.Zero;
            foreach ((int x, int y) in from y in Enumerable.Range(0, ys.Length - 1) from x in Enumerable.Range(0, xs.Length - 1) select (x, y)) {
                (double width, double height) = (xs[x + 1] - xs[x], ys[y + 1] - ys[y]);
                double area = width * height;
                foreach ((int acrossNode, int downNode) in from down in Enumerable.Range(0, Shutters.Order) from across in Enumerable.Range(0, Shutters.Order) select (across, down)) {
                    (double X, double Y) = (xs[x] + (width * Shutters.GetAbscissa(acrossNode)), ys[y] + (height * Shutters.GetAbscissa(downNode)));
                    Vector256<double> folded = Vector256<double>.Zero;
                    foreach ((Mote Mote, (double X, double Y) Point, double LowX, double HighX, double LowY, double HighY, double Modulation) item in active) {
                        (double reach, double edge) = iris.Outline((X - item.Point.X, Y - item.Point.Y));
                        Vector256<double> profile = iris.Weights(reach, edge, item.Mote.Radius, cells.Softness * item.Mote.Radius, state.Fringe);
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
public sealed partial class ShapeFit {
    public static readonly ShapeFit ShortSide = new("short-side", static _ => (1d, 1d));
    public static readonly ShapeFit LongSide = new("long-side", static extent => ((double)int.Max(extent.Width, extent.Height) / extent.ShortSide) switch { var scale => (scale, scale) });

    [UseDelegateFromConstructor]
    public partial (double X, double Y) Scale(PixelExtent extent);
}

[SmartEnum<string>]
public sealed partial class ShapeKind {
    public static readonly ShapeKind Ellipse = new("ellipse", static (shape, half) => DistanceField.Ellipse(half, shape.Exponent));
    public static readonly ShapeKind Box = new("box", static (shape, half) => DistanceField.Box(half, shape.Roundness));
    public static readonly ShapeKind Polygon = new("polygon", static (shape, half) => DistanceField.Polygon(half, shape.Sides, shape.Inset, shape.Roundness));
    public static readonly ShapeKind Ring = new("ring", static (shape, half) => DistanceField.Ring(half, shape.Exponent, shape.Inset));
    public static readonly ShapeKind Cross = new("cross", static (shape, half) => DistanceField.Cross(half, shape.Inset, shape.Roundness));
    public static readonly ShapeKind Heart = new("heart", static (_, half) => DistanceField.Heart(half));
    public static readonly ShapeKind Triangle = new("triangle", static (shape, half) => DistanceField.Triangle(half, shape.Apex, shape.Roundness));

    [UseDelegateFromConstructor]
    public partial DistanceField Field(ShapePrimitive shape, (double X, double Y) half);
}

[SmartEnum<string>]
public sealed partial class ShapeOperation {
    public static readonly ShapeOperation Union = new("union", static (first, second, smoothing, _) => DistanceField.Union(first, second, smoothing));
    public static readonly ShapeOperation Difference = new("difference", static (first, second, smoothing, _) => DistanceField.Difference(first, second, smoothing));
    public static readonly ShapeOperation Intersection = new("intersection", static (first, second, smoothing, _) => DistanceField.Intersection(first, second, smoothing));
    public static readonly ShapeOperation Morph = new("morph", static (first, second, _, progress) => DistanceField.Morph(first, second, progress));

    [UseDelegateFromConstructor]
    public partial (double Distance, double Width) Join((double Distance, double Width) first, (double Distance, double Width) second, double smoothing, float progress);
}

[SmartEnum<string>]
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
public sealed partial class GridPattern {
    public static readonly GridPattern Lines = new("lines", static (lattice, line, _, _, _) => point => new Vector4(line, lattice.Lines(point)));
    public static readonly GridPattern Checker = new("checker", static (lattice, line, cell, alternate, _) => point => new Vector4(lattice.Cells(point, (i, j) => long.IsEvenInteger(i + j) ? cell : alternate, line), 1f));
    public static readonly GridPattern Brick = new("brick", static (lattice, line, cell, alternate, field) => point =>
        new Vector4(lattice.Cells(point, (i, j) => Vector3.Lerp(cell, alternate, CoordinateHash.Uniforms(i, j, field).X), line), 1f));

    [UseDelegateFromConstructor]
    public partial Func<(double X, double Y), Vector4> Shade(GridLattice lattice, Vector3 line, Vector3 cell, Vector3 alternate, uint field);
}

public sealed record ShapePrimitive(
    [property: Display(Name = "Shape", Description = "Figure the distance field draws")] ShapeKind Kind,
    [property: Display(Name = "Placement", Description = "Figure position and turn")] Placement Placement,
    [property: Display(Name = "Width", Description = "Half width in frame sides Fit selects")][property: Range(0.01d, 2d)] ShortSideLength Width,
    [property: Display(Name = "Height", Description = "Half height in frame sides Fit selects")][property: Range(0.01d, 2d)] ShortSideLength Height,
    [property: Display(Name = "Roundness", Description = "Corner radius as share of shorter half extent")][property: Range(0d, 1d)] float Roundness,
    [property: Display(Name = "Exponent", Description = "Superellipse exponent, 1 diamond, 2 ellipse, above 2 squircle")][property: Range(1d, 8d)] float Exponent,
    [property: Display(Name = "Sides", Description = "Polygon side count")][property: Range(3, 32)] int Sides,
    [property: Display(Name = "Inset", Description = "Polygon star inset, ring band, or cross arm width")][property: Range(0d, 1d)] float Inset,
    [property: Display(Name = "Apex", Description = "Triangle top vertex position between left and right edges")][property: Range(-1d, 1d)] float Apex,
    [property: Display(Name = "Fit", Description = "Frame side figure dimensions follow")] ShapeFit Fit) {
    public static ShapePrimitive Default { get; } = new(ShapeKind.Ellipse, Placement.Default, ShortSideLength.Create(0.5f), ShortSideLength.Create(0.5f), 0f, 2f, 5, 0f, 0f, ShapeFit.ShortSide);

    public (FieldFrame Frame, Func<long, long, (double Distance, double Width)> Sample) Field(PixelExtent extent) =>
        (Placement.Frame(extent), Fit.Scale(extent)) switch {
            var (frame, _) when Width == ShortSideLength.Zero || Height == ShortSideLength.Zero => (frame, static (_, _) => DistanceField.Outside),
            var (frame, scale) => Kind.Field(this, (Width * scale.X, Height * scale.Y)) switch {
                var field => (frame, (column, line) => field.Sample(frame.Upward(column, line))),
            },
        };
}

public sealed record ShapeStyle(
    [property: Display(Name = "Fill", Description = "Color inside boundary, off draws outline alone")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour? Fill,
    [property: Display(Name = "Outline", Description = "Color of band around boundary")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Outline,
    [property: Display(Name = "Outline width", Description = "Outline band width in frame short sides")][property: Range(0d, 0.1d)] ShortSideLength OutlineWidth,
    [property: Display(Name = "Outline offset", Description = "Outline distance from boundary in frame short sides")][property: Range(0d, 0.1d)] ShortSideLength OutlineOffset,
    [property: Display(Name = "Feather", Description = "Edge softness in frame short sides, 0% one pixel wide")][property: Range(0d, 0.25d)] ShortSideLength Feather,
    [property: Display(Name = "Rough edge", Description = "Edge noise displacement in frame short sides")][property: Range(0d, 0.02d)] ShortSideLength Roughness,
    [property: Display(Name = "Invert", Description = "Fills outside boundary")] bool Inverted) {
    private static readonly NoiseBasis Gate = NoiseBasis.RidgedMultifractal.Default with { Dimensions = NoiseDimensions.Four, Octaves = new(15f, 0.9f, 2f), Distortion = 1.9f, Gain = 74.5f };

    public static ShapeStyle Default { get; } = new(new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d),
        ShortSideLength.Zero, ShortSideLength.Zero, ShortSideLength.Zero, ShortSideLength.Zero, Inverted: false);

    public Func<(double Distance, double Width), (double X, double Y), Vector4> Shading(PassContext context, uint field) {
        Gamut working = context.Working;
        Func<Unicolour, Vector3> light = context.Formed ? Presets.Shown : color => RampLight.Light(color, working).AsVector3();
        (Vector4 fill, Vector3 outline, double band) = (Optional(Fill).Map(color => new Vector4(light(color), 1f)).IfNone(Vector4.Zero), light(Outline), ShapeEdge.Band(Feather, context.Extent));
        (double pixels, double offset, double width, double reach) = (context.Extent.ShortSide, OutlineOffset.Pixels(context.Extent), OutlineWidth.Pixels(context.Extent), Roughness.Pixels(context.Extent));
        (double near, double far) = (-reach - (band * 0.5d), offset + width + reach + (band * 0.5d));
        Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> gate = Gate.Sampler;
        Func<double, (double X, double Y), double> rough = reach is 0d ? static (distance, _) => distance
            : (distance, local) => distance < near || distance > far ? distance
                : distance + (reach * ((2f * float.Clamp(gate(Vector256.Create(2.5d * local.X, 2.5d * local.Y, 0d, 0d), field).Value, 0f, 1f)) - 1f));
        bool inverted = Inverted;
        return (sample, local) => (inverted ? DistanceField.Inverted(sample) : sample) switch {
            var (distance, narrowest) => (rough(distance * pixels, local), narrowest * pixels) switch {
                var edge => (fill.W * DistanceField.Coverage(edge, 0d, band), DistanceField.Coverage(edge, offset + width, band) - DistanceField.Coverage(edge, offset, band)) switch {
                    var (covered, ring) when covered + ring > 0d => new Vector4((float)(((covered * fill.X) + (ring * outline.X)) / (covered + ring)),
                        (float)(((covered * fill.Y) + (ring * outline.Y)) / (covered + ring)), (float)(((covered * fill.Z) + (ring * outline.Z)) / (covered + ring)), (float)(covered + ring)),
                    _ => Vector4.Zero,
                },
            },
        };
    }
}

public sealed record Shapes(
    [property: Display(Name = "First", Description = "First figure to combine")] ShapePrimitive First,
    [property: Display(Name = "Operation", Description = "Combination of first figure with second")] ShapeOperation? Operation,
    [property: Display(Name = "Second", Description = "Second figure to combine")] ShapePrimitive Second,
    [property: Display(Name = "Smoothing", Description = "Combination blend width in frame short sides")][property: Range(0d, 0.25d)] ShortSideLength Smoothing,
    [property: Display(Name = "Progress", Description = "Morph from first figure at 0% to second at 100%")][property: Range(0d, 1d)] float Progress,
    [property: Display(Name = "Style", Description = "Figure fill and boundary")] ShapeStyle Style,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed) : IPixelStage<Shapes> {
    public const uint Stream = 16u;

    public static Shapes Default { get; } = new(
        ShapePrimitive.Default, Operation: null, ShapePrimitive.Default with { Kind = ShapeKind.Box, Roundness = 0.3f }, ShortSideLength.Zero, 0f, ShapeStyle.Default, GeneratedLayer.Default, Seed.MinValue);

    public static Option<PixelPass> Pass(Shapes state, PassContext context) {
        (FieldFrame frame, Func<long, long, (double Distance, double Width)> first) = state.First.Field(context.Extent);
        Func<long, long, (double Distance, double Width)> field = Optional(state.Operation).Match(
            Some: operation => (state.Second.Field(context.Extent).Sample, (double)state.Smoothing, state.Progress) switch {
                var (second, smoothing, progress) => (column, line) => operation.Join(first(column, line), second(column, line), smoothing, progress),
            },
            None: () => first);
        Func<(double Distance, double Width), (double X, double Y), Vector4> shade = state.Style.Shading(context, state.Seed.Field(Stream, 0u));
        return Some(state.Layer.Pass((row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = shade(field(column + i, line), frame.Upward(column + i, line));
        }, context.Working));
    }
}

public sealed record Lightning(
    [property: Display(Name = "Placement", Description = "Field position and turn")] Placement Placement,
    [property: Display(Name = "Length", Description = "Half stroke length in frame short sides")][property: Range(0d, 1d)] ShortSideLength Length,
    [property: Display(Name = "Thickness", Description = "Stroke width in frame short sides")][property: Range(0d, 0.02d)] ShortSideLength Thickness,
    [property: Display(Name = "Detail", Description = "Subdivisions of each displaced segment")][property: Range(1, 5)] int Depth,
    [property: Display(Name = "Displacement", Description = "Share of each segment's length the stroke wanders by")][property: Range(0d, 1d)] float Displacement,
    [property: Display(Name = "Branches", Description = "Forks from stroke midpoints")][property: Range(0, 7)] int Branches,
    [property: Display(Name = "Reach", Description = "Branch length as share of stroke length")][property: Range(0d, 1d)] float Reach,
    [property: Display(Name = "Spread", Description = "Angle branches turn from stroke")][property: Range(0d, double.Pi / 3d)] float Spread,
    [property: Display(Name = "Taper", Description = "Branch base width relative to stroke, narrowing to tip")][property: Range(0d, 1d)] float Taper,
    [property: Display(Name = "Speed", Description = "Noise units per second the bolt changes through")][property: Range(-60d, 60d)] float Speed,
    [property: Display(Name = "Style", Description = "Figure fill and boundary")] ShapeStyle Style,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold) : IPixelStage<Lightning> {
    private const int Levels = 5;

    public static Lightning Default { get; } = new(
        Placement.Default, ShortSideLength.Create(1f / 2f), ShortSideLength.Create(0.01f / (5f - (4.5f * 0.5f)) * ReferenceFrame.GreaterSide), Levels, 0.5f, 3, 0.4f, 0.5f, 0.5f, 1f / 0.05f,
        ShapeStyle.Default, GeneratedLayer.Added with { Exposure = 3f }, Seed.MinValue, Timing.Default, Hold.Create(0.05f));

    public static Option<PixelPass> Pass(Lightning state, PassContext context) {
        (uint field, float clock, double chord) = (state.Seed.Field(17u, 0u), 2f * state.Speed * state.Hold.Held(state.Timing.At(context)), 2d * state.Length);
        Func<int, int, int, float> noise = (x, y, z) => 0.5f + (0.5f * NoiseDimensions.Four.Gradient(Vector256.Create((2d * x) + 0.5d, (2d * y) + 0.5d, (2d * z) + 0.5d, clock), field));
        Bolt[] channel = Bolt.Path((0d, state.Length), (0d, -1d), chord, Levels, state.Depth, state.Displacement, k => noise(k, 0, 0), _ => state.Thickness / 2d);
        Bolt[] pieces = [.. channel, .. Enumerable.Range(1, state.Branches).SelectMany(i => (BitOperations.Log2((uint)i), (i % 2 is 0 ? 1f : -1f) * state.Spread * (0.5f + noise(i, 0, 1))) switch {
            var (level, turn) => Bolt.Path(
                channel[((2 * (i - (1 << level))) + 1) * channel.Length / (2 << level)].Start, double.SinCos(turn) switch { var (sin, cos) => (sin, -cos) },
                state.Reach * chord * (0.5f + noise(i, 0, 2)), 3, state.Depth, state.Displacement, j => noise(j, i, 0), along => state.Taper * state.Thickness * (1d - along) / 2d),
        })];
        Func<(double Distance, double Width), (double X, double Y), Vector4> shade = state.Style.Shading(context, CoordinateHash.Branch(field, 0u));
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
            shade(pieces.Aggregate(DistanceField.Outside, (nearest, piece) => piece.Sample(point) switch { var next => next.Distance < nearest.Distance ? next : nearest }), point)), context.Working));
    }
}

public sealed record Gradient(
    [property: Display(Name = "Type", Description = "Field the ramp runs over")] GradientKind Kind,
    [property: Display(Name = "Placement", Description = "Field position and turn")] Placement Placement,
    [property: Display(Name = "Width", Description = "Ramp half width in frame short sides")][property: Range(0.01d, 4d)] ShortSideExtent Width,
    [property: Display(Name = "Height", Description = "Ramp half height in frame short sides")][property: Range(0.01d, 4d)] ShortSideExtent Height,
    [property: Display(Name = "Colors", Description = "Display color and coverage along ramp")] ImmutableArray<RampStop> Colors,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer) : IPixelStage<Gradient> {
    public static Gradient Default { get; } = new(GradientKind.Linear, Placement.Default, ShortSideExtent.Create(1f / 2f), ShortSideExtent.Create(1f / 2f), Presets.Grayscale, GeneratedLayer.Default);

    public static Option<PixelPass> Pass(Gradient state, PassContext context) =>
        (state.Kind.Parameter(((float)state.Width, (float)state.Height)), RampFootprint.Of(state.Colors, None)) switch {
            var (parameter, footprint) => Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
                parameter(point) switch { var (position, slope) => footprint.Mean(position, slope / context.Extent.ShortSide, state.Kind.Periodic) }), context.Working)),
        };
}

public sealed record CornerGradient(
    [property: Display(Name = "Top left", Description = "Display color at top left corner")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour TopLeft,
    [property: Display(Name = "Top right", Description = "Display color at top right corner")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour TopRight,
    [property: Display(Name = "Bottom right", Description = "Display color at bottom right corner")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour BottomRight,
    [property: Display(Name = "Bottom left", Description = "Display color at bottom left corner")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour BottomLeft,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer) : IPixelStage<CornerGradient> {
    public static CornerGradient Default { get; } = new(
        new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), GeneratedLayer.Default);

    public static Option<PixelPass> Pass(CornerGradient state, PassContext context) {
        (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) =
            (Presets.Shown(state.TopLeft), Presets.Shown(state.TopRight), Presets.Shown(state.BottomRight), Presets.Shown(state.BottomLeft));
        PixelExtent extent = context.Extent;
        return Some(state.Layer.Pass((row, column, line) => {
            float down = (float)ShapeEdge.Smoother(double.Clamp((extent.Height - line - 0.5d) / extent.Height, 0d, 1d));
            for (int i = 0; i < row.Length; i++) {
                float across = (float)ShapeEdge.Smoother(double.Clamp((column + i + 0.5d) / extent.Width, 0d, 1d));
                row[i] = new Vector4(Vector3.Lerp(Vector3.Lerp(topLeft, topRight, across), Vector3.Lerp(bottomLeft, bottomRight, across), down), 1f);
            }
        }, context.Working));
    }
}

public sealed record Grid(
    [property: Display(Name = "Pattern", Description = "Lines alone or filled cells")] GridPattern Pattern,
    [property: Display(Name = "Placement", Description = "Field position and turn")] Placement Placement,
    [property: Display(Name = "Cell", Description = "Cell height in frame short sides")][property: Range(0.01d, 1d)] ShortSideExtent Cell,
    [property: Display(Name = "Aspect", Description = "Cell width over cell height")][property: Range(0.25d, 4d)] float Aspect,
    [property: Display(Name = "Brick offset", Description = "Share of cell every second brick row shifts by")][property: Range(0d, 1d)] float Offset,
    [property: Display(Name = "Line width", Description = "Line width in frame short sides")][property: Range(0d, 0.05d)] ShortSideLength LineWidth,
    [property: Display(Name = "Feather", Description = "Edge softness in frame short sides, 0% one pixel wide")][property: Range(0d, 0.05d)] ShortSideLength Feather,
    [property: Display(Name = "Line color", Description = "Display color of lines")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour LineColor,
    [property: Display(Name = "Cell color", Description = "Display color of cells")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour CellColor,
    [property: Display(Name = "Alternate color", Description = "Display color of alternate checker cells and far brick tint end")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour AlternateColor,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed) : IPixelStage<Grid> {
    public static Grid Default { get; } = new(
        GridPattern.Lines, Placement.Default, ShortSideExtent.Create(1f / 3f), 1f, 0f, ShortSideLength.Create(2f * (0.02f + (0.18f * 0.2f)) * (0.1f + (0.4f * 0.1f)) / 3f), ShortSideLength.Zero,
        new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), GeneratedLayer.Default, Seed.MinValue);

    public static Option<PixelPass> Pass(Grid state, PassContext context) =>
        Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(state.Pattern.Shade(
            new GridLattice(state.Cell, (double)state.Cell * state.Aspect, state.Offset, state.LineWidth, ShapeEdge.Band(state.Feather, context.Extent) / context.Extent.ShortSide),
            Presets.Shown(state.LineColor), Presets.Shown(state.CellColor), Presets.Shown(state.AlternateColor), state.Seed.Field(18u, 0u))), context.Working));
}

public sealed record Stripes(
    [property: Display(Name = "Profile", Description = "Level across each stripe period")] WaveProfile Profile,
    [property: Display(Name = "Placement", Description = "Field position and turn")] Placement Placement,
    [property: Display(Name = "Wavelength", Description = "Stripe period in frame short sides")][property: Range(0.01d, 1d)] ShortSideExtent Wavelength,
    [property: Display(Name = "Bend", Description = "Stripe fold about center line, 0 straight")][property: Range(-double.Pi, double.Pi)] float Bend,
    [property: Display(Name = "Phase", Description = "Share of period stripes shift by")][property: Range(0d, 1d)] float Phase,
    [property: Display(Name = "Speed", Description = "Periods per second the stripes scroll")][property: Range(-4d, 4d)] float Speed,
    [property: Display(Name = "Colors", Description = "Display color and coverage across one period")] ImmutableArray<RampStop> Colors,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<Stripes> {
    public static Stripes Default { get; } = new(WaveProfile.Sine, Placement.Default, ShortSideExtent.Create(float.Tau / 20f), 0f, 0f, 0f, Presets.Grayscale, GeneratedLayer.Default, Timing.Default);

    public static Option<PixelPass> Pass(Stripes state, PassContext context) {
        ((double sin, double cos), double start, double wavelength) = (double.SinCos(state.Bend), state.Phase + ((double)state.Speed * state.Timing.At(context)), state.Wavelength);
        (double width, RampFootprint footprint) = (1d / state.Wavelength.Pixels(context.Extent), RampFootprint.Of(state.Colors, Some(state.Profile)));
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
            footprint.Mean((((point.Y * cos) + (double.Abs(point.X) * sin)) / wavelength) + start, width, periodic: true)), context.Working));
    }
}

public sealed record Letterbox(
    [property: Display(Name = "Aspect", Description = "Width over height of window between bars, off follows frame")][property: Range(0.25d, 4d)] float? Aspect,
    [property: Display(Name = "Inset", Description = "Window margin in frame short sides")][property: Range(0d, 0.1d * ReferenceFrame.GreaterSide)] ShortSideLength Inset,
    [property: Display(Name = "Roundness", Description = "Window corner radius in frame short sides")][property: Range(0d, 0.1d * ReferenceFrame.GreaterSide)] ShortSideLength Radius,
    [property: Display(Name = "Style", Description = "Figure fill and boundary")] ShapeStyle Style,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed) : IPixelStage<Letterbox> {
    public static Letterbox Default { get; } = new(
        2.39f, ShortSideLength.Zero, ShortSideLength.Zero, ShapeStyle.Default with { Fill = new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), Inverted = true }, GeneratedLayer.Default, Seed.MinValue);

    public static Letterbox FilmGate { get; } = Default with {
        Aspect = null,
        Inset = ShortSideLength.Create(40f / ReferenceFrame.Height),
        Radius = ShortSideLength.Create(40f / ReferenceFrame.Height),
        Style = Default.Style with { Feather = ShortSideLength.Create(10f / 3f * float.Sqrt(float.Tau) / ReferenceFrame.Height), Roughness = ShortSideLength.Create(6f / ReferenceFrame.Height) },
    };

    public static Seq<(string Name, Func<Letterbox, Letterbox> Look)> Looks { get; } = [(nameof(FilmGate), static _ => FilmGate)];

    public static Option<PixelPass> Pass(Letterbox state, PassContext context) {
        (double X, double Y) frame = ((double)context.Extent.Width / context.Extent.ShortSide, (double)context.Extent.Height / context.Extent.ShortSide);
        (double X, double Y) = Optional(state.Aspect).Map(aspect => aspect >= frame.X / frame.Y ? (frame.X, frame.X / aspect) : (frame.Y * aspect, frame.Y)).IfNone(frame) switch {
            var (x, y) => ((x / 2d) - state.Inset, (y / 2d) - state.Inset),
        };
        (double small, Func<(double Distance, double Width), (double X, double Y), Vector4> shade) = (double.Min(X, Y), state.Style.Shading(context, state.Seed.Field(Shapes.Stream, 0u)));
        double radius = double.Min(state.Radius, small);
        return Some(state.Layer.Pass(FieldFrame.Centered(context.Extent).Fill(point =>
            shade(small <= 0d ? DistanceField.Outside : (DistanceField.Rounded(point, (X, Y), radius), 2d * small), point)), context.Working));
    }
}

[SmartEnum<string>]
public sealed partial class FlareAnchor {
    [Display(Name = "Frame position")]
    public static readonly FlareAnchor Frame = new("frame", [], static (position, _, context) => Some((position.Point(context.Extent), Option<float>.None)));
    public static readonly FlareAnchor Sun = new("sun", [GuideChannel.Depth], static (_, _, context) =>
        context.Lights.ToFin(new DarkroomFault("Sun flare anchor", DarkroomReason.Lights)).Map(lights => lights.Sun.Bind(direction => Distant(direction, context))));
    public static readonly FlareAnchor Light = new("light", [GuideChannel.Depth], static (_, light, context) =>
        Optional(light).Traverse(target => context.Lights.ToFin(new DarkroomFault($"Light flare anchor {target}", DarkroomReason.Lights)).Map(lights =>
            (from point in lights.Points.Find(target) from camera in context.Camera from located in camera.Pixel(point) select (located.Point, Depth: Some(located.Depth)))
            | lights.Directions.Find(target).Bind(direction => Distant(direction, context)))).As().Map(static located => located.Flatten()));

    public Seq<GuideChannel> Occluders { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Option<((double X, double Y) Point, Option<float> Depth)>> Locate(FramePosition position, Guid? light, PassContext context);

    private static Option<((double X, double Y) Point, Option<float> Depth)> Distant(Vector3 direction, PassContext context) =>
        context.Camera.Bind(camera => camera.Distant(direction)).Map(static located => (located.Point, Some(located.Depth)));
}

public sealed record FlareSource(
    [property: Display(Name = "Source", Description = "Point generated light converges on")] FlareAnchor Anchor,
    [property: Display(Name = "Position", Description = "Frame point to converge on when Source is Frame position")] FramePosition Position,
    [property: Display(Name = "Light", Description = "Document light to converge on when Source is Light")] Guid? Light,
    [property: Display(Name = "Occlusion", Description = "Surfaces nearer camera than source hide its light")] bool Occlusion) {
    public static FlareSource Default { get; } = new(FlareAnchor.Frame, FramePosition.Center, Light: null, Occlusion: true);

    public Seq<GuideChannel> Channels => Occlusion ? Anchor.Occluders : [];

    public Option<PixelPass> Pass(PassContext context, Func<(double X, double Y), Option<Occluder>, Func<PixelFrame, IProgress<int>, Fin<Unit>>> kernel) =>
        Anchor.Locate(Position, Light, context).Match(
            Succ: location =>
                from source in location
                from occluder in Occlusion ? source.Depth.Traverse(depth => context.Guides.Find(GuideChannel.Depth).Map(guide => new Occluder(guide, depth))).As() : Some(Option<Occluder>.None)
                select (PixelPass)new PixelPass.Frame(kernel(source.Point, occluder)),
            Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)));
}

public sealed record LensFlare(
    [property: Display(Name = "Source", Description = "Frame position or scene light generated light follows")] FlareSource Source,
    [property: Display(Name = "Hotspot", Description = "Glow brightness at source in stops")][property: Range(-10d, 10d)] float? Hotspot,
    [property: Display(Name = "Hotspot colors", Description = "Color and coverage from center to hotspot edge")] ImmutableArray<RampStop> HotspotColors,
    [property: Display(Name = "Hotspot size", Description = "Hotspot radius in frame short sides")][property: Range(0.001d, 0.25d)] ShortSideLength HotspotSize,
    [property: Display(Name = "Hotspot softness", Description = "Hotspot edge softness in frame short sides")][property: Range(0.001d, 0.25d)] ShortSideLength HotspotSoftness,
    [property: Display(Name = "Starburst", Description = "Ray brightness around source in stops")][property: Range(-10d, 10d)] float? Starburst,
    [property: Display(Name = "Starburst colors", Description = "Color and coverage from source along each ray")] ImmutableArray<RampStop> StarburstColors,
    [property: Display(Name = "Starburst length", Description = "Ray reach in frame short sides")][property: Range(0.01d, 2d)] ShortSideLength StarburstLength,
    [property: Display(Name = "Starburst noise", Description = "Ray pattern around source")] NoiseBasis Basis,
    [property: Display(Name = "Starburst density", Description = "Noise periods per turn around source")][property: Range(1d, 100d)] float StarburstDensity,
    [property: Display(Name = "Starburst sharpness", Description = "Power narrowing each ray around its brightest axis")][property: Range(1d, 8d)] float StarburstSharpness,
    [property: Display(Name = "Shimmer", Description = "Noise units per second the rays change through")][property: Range(0d, 10d)] float Shimmer,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold,
    [property: Display(Name = "Anamorphic bar", Description = "Bar brightness through source in stops")][property: Range(-10d, 10d)] float? Bar,
    [property: Display(Name = "Bar colors", Description = "Color and coverage from source along bar")] ImmutableArray<RampStop> BarColors,
    [property: Display(Name = "Bar length", Description = "Bar half length in frame short sides")][property: Range(0.01d, 2d)] ShortSideLength BarLength,
    [property: Display(Name = "Bar thickness", Description = "Bar width in frame short sides")][property: Range(0.001d, 0.25d)] ShortSideLength BarThickness,
    [property: Display(Name = "Bar angle", Description = "Counterclockwise turn from horizontal")][property: Range(-double.Pi, double.Pi)] float BarAngle,
    [property: Display(Name = "Halo", Description = "Halo brightness in stops")][property: Range(-10d, 10d)] float? Ring,
    [property: Display(Name = "Halo colors", Description = "Color and coverage across halo band")] ImmutableArray<RampStop> RingColors,
    [property: Display(Name = "Halo radius", Description = "Halo radius in frame short sides")][property: Range(0d, 1d)] ShortSideLength RingRadius,
    [property: Display(Name = "Halo width", Description = "Halo band width in frame short sides")][property: Range(0.001d, 0.25d)] ShortSideLength RingWidth,
    [property: Display(Name = "Halo softness", Description = "Halo edge softness in frame short sides")][property: Range(0.001d, 0.25d)] ShortSideLength RingSoftness,
    [property: Display(Name = "Halo position", Description = "Halo center along source axis, 0 at frame center, 1 at source")][property: Range(-2d, 2d)] float RingMagnification,
    [property: Display(Name = "Ghosts", Description = "Iris image brightness along source axis in stops")][property: Range(-10d, 10d)] float? Ghosts,
    [property: Display(Name = "Ghost colors", Description = "Color and coverage along ghost chain")] ImmutableArray<RampStop> GhostColors,
    [property: Display(Name = "Ghost size", Description = "Largest ghost radius in frame short sides")][property: Range(0.001d, 0.25d)] ShortSideLength GhostSize,
    [property: Display(Name = "Ghost count", Description = "Iris images along source axis")][property: Range(1, 16)] int GhostCount,
    [property: Display(Name = "First ghost", Description = "Ghost chain start, 0 at frame center, 1 at source")][property: Range(-2d, 2d)] float GhostFirst,
    [property: Display(Name = "Last ghost", Description = "Ghost chain end, 0 at frame center, 1 at source")][property: Range(-2d, 2d)] float GhostLast,
    [property: Display(Name = "Ghost variation", Description = "Variation in each ghost's position and radius")][property: Range(0d, 0.9d)] float GhostVariation,
    [property: Display(Name = "Ghost softness", Description = "Share of each ghost's radius feathered at its edge")][property: Range(0d, 1d)] float GhostSoftness,
    [property: Display(Name = "Aperture", Description = "Lens opening shaping bokeh and diffraction spikes")] Iris Aperture,
    [property: Display(Name = "Color shift", Description = "Red and blue bokeh sizes apart from green")][property: Range(-1d, 1d)] float Fringe,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<LensFlare> {
    private static readonly ShortSideLength SourceRadius = ShortSideLength.Create(0.2f * 0.3f / 2f * ReferenceFrame.GreaterSide);

    public static LensFlare Default { get; } = new(
        FlareSource.Default with { Position = new(0.3f, 1f - 0.6f) },
        0f, Presets.White, SourceRadius, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide),
        0f, Presets.White, ShortSideLength.Create(0.5f), NoiseBasis.Fbm.Default with { Octaves = NoiseBasis.Fbm.Default.Octaves with { Roughness = 0.563f } }, 30f, 3f, 0f, Hold.MinValue,
        0f, Presets.White, ShortSideLength.Create(0.972f / 2f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide), 0f,
        0f, Presets.White, ShortSideLength.Create(1f / 2.1f), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), 0f,
        0f, Presets.White, SourceRadius, 5, 0.6f, -1f, 0.5f, 0.3f, Iris.Create(6, double.Pi / 3d, 0d, 0d, 1d), 0f, GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static LensFlare NachoLens { get; } = Default with { Aperture = Iris.Create(3, 0.179d, 0.332d, 0d, 1d), Fringe = -0.1f };
    public static LensFlare RingLens { get; } = Default with { Aperture = Iris.Create(6, 0d, 0.796d, 0.902d, 1d), Fringe = 0.189f };
    public static LensFlare HexLens { get; } = Default with { Aperture = Iris.Create(6, 0d, 0d, 0.902d, 1d), Fringe = 0.052f };
    public static LensFlare RoundLens { get; } = Default with { Aperture = Iris.Create(16, 0d, 1d, 0d, 1d), Fringe = 0.012f };

    public static Seq<(string Name, Func<LensFlare, LensFlare> Look)> Looks { get; } = [
        (nameof(NachoLens), static _ => NachoLens), (nameof(RingLens), static _ => RingLens), (nameof(HexLens), static _ => HexLens), (nameof(RoundLens), static _ => RoundLens)];

    public static Seq<GuideChannel> Channels(LensFlare state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LensFlare state, PassContext context) =>
        state.Source.Pass(context, (point, occlusion) => (frame, progress) =>
            Fill(state, context, point).Map(fill => state.Layer.Pass(fill(occlusion.Map(occluder => occluder.Visible(point, state.HotspotSize.Pixels(context.Extent))).IfNone(1f)), context.Working).Run(frame, progress))
                .IfNone(unit));

    private static Option<Func<float, Action<Span<Vector4>, long, long>>> Fill(LensFlare state, PassContext context, (double X, double Y) source) {
        (FieldFrame frame, double side, Iris iris) = (FieldFrame.Centered(context.Extent), context.Extent.ShortSide, state.Aperture);
        (double X, double Y) = frame.Local(source) switch { var (x, y) => (x, -y) };
        (double first, double last, uint draws, uint field) = (state.GhostFirst, state.GhostLast, state.Seed.Field(19u, 1u), state.Seed.Field(19u, 0u));
        (double hotspot, double edge, double burst, double density, float sharpness) =
            (state.HotspotSize, ShapeEdge.Band(state.HotspotSoftness, context.Extent), state.StarburstLength, state.StarburstDensity / double.Tau, state.StarburstSharpness);
        Func<Vector256<double>, uint, (float Value, Vector256<double> Site)> basis = state.Basis.Sampler;
        double shimmer = (double)state.Shimmer * state.Hold.Held(state.Timing.At(context));
        (Matrix3x2 turn, (double X, double Y) halo, double width, double rim) =
            (Matrix3x2.CreateRotation(state.BarAngle), (state.RingMagnification * X, state.RingMagnification * Y), state.RingWidth, ShapeEdge.Band(state.RingSoftness, context.Extent));
        RampLight ghostTable = RampLight.Of(state.GhostColors, context.Working);
        ((double X, double Y) Center, float Flip, double Radius, double Ramp, double Bound, Vector3 Color)[] ghosts = [..
            from k in state.Ghosts is null ? [] : Enumerable.Range(0, state.GhostCount)
            let u = CoordinateHash.Uniforms(k, 0L, draws)
            let position = (k + 0.5d + (state.GhostVariation * (u.Y - 0.5d))) / state.GhostCount
            let m = double.Lerp(first, last, position)
            let shrink = 1d - (state.GhostVariation * u.X)
            let radius = state.GhostSize.Pixels(context.Extent) * shrink
            let ramp = (state.GhostSoftness * radius) + 1d
            let color = ghostTable.Sample((float)position)
            select ((m * X, m * Y), (float)double.CopySign(1d, m), radius, ramp, (iris.Circumradius * radius) + (ramp / 2d), color.AsVector3() * (float)(color.W / (shrink * shrink)))];
        Seq<(float? Gain, ImmutableArray<RampStop> Colors, Func<(double X, double Y), (float Tone, Vector3 Weight)> Shape)> elements =
            [(state.Hotspot, state.HotspotColors, Disc), (state.Starburst, state.StarburstColors, Streaks), (state.Bar, state.BarColors, Anamorphic), (state.Ring, state.RingColors, Halo)];
        Func<(double X, double Y), Vector3>[] terms = [..
            from element in elements
            from gain in Optional(element.Gain).ToSeq()
            let table = RampLight.Of(element.Colors, context.Working)
            select (Func<(double X, double Y), Vector3>)(q => element.Shape(q) switch { var (tone, weight) => float.Exp2(gain) * weight * (table.Sample(tone) switch { var color => color.AsVector3() * color.W }) }),
            .. from gain in Optional(state.Ghosts).ToSeq() select (Func<(double X, double Y), Vector3>)(q => float.Exp2(gain) * Chain(q))];
        return terms.Length is 0 ? None : Some<Func<float, Action<Span<Vector4>, long, long>>>(visible => frame.Fill(q => new Vector4(visible * terms.Aggregate(Vector3.Zero, (light, term) => light + term(q)), 1f)));

        (float, Vector3) Disc((double X, double Y) q) => double.Hypot(q.X - X, q.Y - Y) switch {
            var r => ((float)(hotspot is 0d ? 0d : double.Min(r / hotspot, 1d)), new Vector3(float.Min(ShapeEdge.Coverage((r - hotspot) * side, edge), (float)(2d * hotspot * side / edge)))),
        };

        (float, Vector3) Streaks((double X, double Y) q) => burst is 0d ? (0f, Vector3.Zero) : ((X: q.X - X, Y: q.Y - Y), double.Min(double.Hypot(q.X - X, q.Y - Y) / burst, 1d)) switch {
            (var d, < 1d and var u) => ((float)u, new Vector3(MathF.Pow(float.Max(0f, (2f * Noise(d)) - 1f), sharpness) * (float)((1d - u) * (1d - u)))),
            (_, var u) => ((float)u, Vector3.Zero),
        };

        float Noise((double X, double Y) d) =>
            double.SinCos(double.Atan2(d.Y, d.X)) switch { var (sin, cos) => basis(Vector256.Create(density * cos, density * sin, shimmer, 0d), field).Value };

        (float, Vector3) Anamorphic((double X, double Y) q) => state.BarLength == ShortSideLength.Zero ? (0f, Vector3.Zero) : (q.X - X, q.Y - Y) switch {
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

public sealed record LightRays(
    [property: Display(Name = "Highlights", Description = "Highlights that cast rays")] HighlightKey Highlight,
    [property: Display(Name = "Source", Description = "Frame position or scene light generated light follows")] FlareSource Source,
    [property: Display(Name = "Size", Description = "Longest ray march in frame short sides")][property: Range(0d, 2.04d)] ShortSideLength Size,
    [property: Display(Name = "Jitter", Description = "Share of march steps traded for random sample shifts")][property: Range(0d, 1d)] float Jitter,
    [property: Display(Name = "Tint", Description = "Ray color")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Tint,
    [property: Display(Name = "Layer", Description = "Blend and brightness of generated layer")] GeneratedLayer Layer,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<LightRays> {
    public static LightRays Default { get; } = new(
        HighlightKey.Create(1f, 0.1f), FlareSource.Default, ShortSideLength.Create(0.5f * float.Hypot(ReferenceFrame.GreaterSide, 1f)), 0f, new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d),
        GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static Seq<GuideChannel> Channels(LightRays state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LightRays state, PassContext context) =>
        state.Source.Pass(context, (point, occlusion) => (frame, progress) => {
            Func<(double X, double Y), bool> hidden = occlusion.Match(Some: static occluder => (Func<(double X, double Y), bool>)occluder.Hides, None: static () => static _ => false);
            PixelFrame key = new(frame.Origin, frame.Size, frame.Extent);
            Span2D<Vector4> keys = key.View.Span;
            ReadOnlySpan2D<Vector4> light = frame.View.Span;
            for (int y = 0; y < keys.Height; y++) {
                for (int x = 0; x < keys.Width; x++)
                    keys[y, x] = hidden(((long)frame.Origin.X + x + 0.5d, (long)frame.Origin.Y + y + 0.5d)) ? Vector4.Zero : state.Highlight.Extract(light[y, x]);
            }
            return state.Layer.Pass(Fill(state, context, point, key), context.Working).Run(frame, progress);
        });

    private static Action<Span<Vector4>, long, long> Fill(LightRays state, PassContext context, (double X, double Y) source, PixelFrame key) {
        (double reach, float jitter, PixelExtent extent) = (double.Truncate((double)state.Size * context.Extent.ShortSide), state.Jitter, context.Extent);
        (uint field, Vector3 tint) = (state.Seed.Field(20u, BitConverter.SingleToUInt32Bits(state.Timing.At(context))), RampLight.Light(state.Tint, context.Working).AsVector3());
        Func<long, long, float> shift = state.Jitter is 0f ? static (_, _) => 0f : (column, line) => CoordinateHash.Uniforms(column, line, field).X;
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

file sealed record Mote(
    double Depth, int Layer, long Column, long Line, Vector256<double> Point, (double X, double Y) Rest, double Scale, double Radius, double Gain, Vector4 Coating, Vector256<double> Light);

file sealed record Bolt((double X, double Y) Start, (double X, double Y) End, double From, double To) {
    public (double Distance, double Width) Sample((double X, double Y) point) => DistanceField.Tapered(point, Start, End, From, To);

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Presets {
    public static ImmutableArray<RampStop> Grayscale { get; } = [new(new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), 0f), new(new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), 1f)];
    public static ImmutableArray<RampStop> White { get; } = [new(new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), 0f)];

    public static float Map(float value, (float Low, float High) edges) => float.Clamp((value - edges.Low) / (edges.High - edges.Low), 0f, 1f);

    public static Vector3 Shown(Unicolour color) => new((float)color.Rgb.R, (float)color.Rgb.G, (float)color.Rgb.B);

    public static ImmutableArray<RampStop> Sampled(Func<float, Vector4> light, params ReadOnlySpan<float> positions) =>
        [.. toSeq(positions.ToArray()).Map(position => light(position) switch { var held => new RampStop(new Unicolour(ColourSpace.RgbLinear, held.X, held.Y, held.Z, held.W), position) })];

    public static ImmutableArray<RampStop> Threshold(Vector3 light, float low, float high, bool eased = false) =>
        [new(new Unicolour(ColourSpace.RgbLinear, light.X, light.Y, light.Z, 0d), low), new(new Unicolour(ColourSpace.RgbLinear, light.X, light.Y, light.Z, 1d), high) { Eased = eased }];

    public static ImmutableArray<RampStop> Solid(Vector3 light) => Sampled(_ => new Vector4(light, 1f), 0f);
}
