using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Procedural;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Octaves(float Detail, float Roughness, float Lacunarity) {
    public bool Degenerate => Lacunarity is 0f && Detail > 0f && Roughness > 0f;

    internal float Weight(int octave) => float.Min(1f, Detail - (octave - 1));
}

[SmartEnum<int>]
public sealed partial class NoiseDimensions {
    private const float Repeat = 100000f;
    private const uint Impulses = 8u;
    private static readonly ImmutableArray<Vector4>[] Corners = Offsets(0, 1);
    private static readonly ImmutableArray<Vector4>[] Near = Offsets(-1, 1);
    private static readonly ImmutableArray<Vector4>[] Wide = Offsets(-2, 2);

    public static readonly NoiseDimensions One = Lattice(1, 0.25f, static (word, offset) => (word >> 28) switch { var h => (1u + (h & 7u)) * ((h & 8u) is not 0u ? -offset.X : offset.X) });
    public static readonly NoiseDimensions Two = Lattice(2, 0.6616f, static (word, offset) => (word >> 29) switch { var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 4u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -2f : 2f) * (h < 4u ? offset.Y : offset.X)) });
    public static readonly NoiseDimensions Three = Lattice(3, 0.982f, static (word, offset) => (word >> 28) switch { var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 8u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -1f : 1f) * (h < 4u ? offset.Y : h is 12u or 14u ? offset.X : offset.Z)) });
    public static readonly NoiseDimensions Four = Lattice(4, 0.8344f, static (word, offset) => (word >> 27) switch {
        var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 24u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -1f : 1f) * (h < 16u ? offset.Y : offset.Z)) + (((h & 4u) is not 0u ? -1f : 1f) * (h < 8u ? offset.Z : offset.W)),
    });

    private float Scale { get; }
    internal float GradientBound { get; }

    internal Vector4 Live => Mask(Key);

    [UseDelegateFromConstructor]
    private partial float Slope(uint word, Vector4 offset);

    // --- [GRADIENTS]
    public Vector4 White(Vector4 point, uint field) => CoordinateHash.Uniforms(Split(point * Live).Cell, field);

    public float Gradient(Vector4 point, uint field) {
        (Vector128<int> cell, Vector4 local) = Split((point - (Repeat * Vector4.Truncate(point / Repeat))
            + Vector4.ConditionalSelect(Vector4.GreaterThanOrEqual(Vector4.Abs(point), new Vector4(10f * Repeat)), new Vector4(0.5f), Vector4.Zero)
            + (100f * (Vector4.One + CoordinateHash.Uniforms(Vector128<int>.Zero, field)))) * Live);
        Vector4 fade = local * local * local * ((local * ((6f * local) - new Vector4(15f))) + new Vector4(10f));
        float level = 0f;
        foreach (Vector4 corner in Corners[Key].AsSpan()) {
            Vector4 weights = Vector4.Lerp(Vector4.One - fade, fade, corner);
            level += weights.X * weights.Y * weights.Z * weights.W * Slope(CoordinateHash.Hash(cell + Step(corner), field).X, local - corner);
        }
        return Scale * level;
    }

    public Vector4 Distort(Vector4 point, float distortion, uint field) {
        Vector4 moved = Vector4.Zero;
        for (int axis = 0; axis < (distortion is 0f ? 0 : Key); axis++)
            moved[axis] = distortion * Gradient(point, CoordinateHash.Branch(field, (uint)axis));
        return point + moved;
    }

    // --- [CELLS]
    public IEnumerable<(Vector128<int> Cell, Vector4 Site)> Sites(Vector4 low, Vector4 high, float randomness, uint field) {
        (Vector128<int> first, Vector128<int> last) = (Split((low - (randomness * Vector4.One)) * Live).Cell, Split(high * Live).Cell);
        return from w in Span(3) from z in Span(2) from y in Span(1) from x in Span(0)
               let cell = Vector128.Create(x, y, z, w)
               select (cell, At(cell, Site(cell, Vector4.Zero, randomness, field)));

        IEnumerable<int> Span(int axis) => Enumerable.Range(first[axis], last[axis] - first[axis] + 1);
    }

    internal ((float Distance, Vector4 Site) First, (float Distance, Vector4 Site) Second) Ranked(Vector4 point, float exponent, float randomness, uint field) {
        (Vector128<int> cell, Vector4 local) = Split(point * Live);
        ((float Distance, Vector4 Site) First, (float Distance, Vector4 Site) Second) nearest = ((float.MaxValue, Vector4.Zero), (float.MaxValue, Vector4.Zero));
        foreach (Vector4 offset in Near[Key].AsSpan()) {
            Vector4 site = Site(cell, offset, randomness, field);
            float distance = Length(site - local, exponent);
            nearest = distance < nearest.First.Distance ? ((distance, site), nearest.First) : distance < nearest.Second.Distance ? (nearest.First, (distance, site)) : nearest;
        }
        return ((nearest.First.Distance, At(cell, nearest.First.Site)), (nearest.Second.Distance, At(cell, nearest.Second.Site)));
    }

    internal (float Distance, Vector4 Site) Smoothed(Vector4 point, float exponent, float randomness, float smoothness, uint field) {
        (Vector128<int> cell, Vector4 local) = Split(point * Live);
        ReadOnlySpan<Vector4> wide = Wide[Key].AsSpan();
        (float distance, Vector4 site) = (0f, Vector4.Zero);
        for (int index = 0; index < wide.Length; index++) {
            Vector4 next = Site(cell, wide[index], randomness, field);
            float reach = Length(next - local, exponent);
            float blend = index is 0 ? 1f : DistanceField.SmoothStep(0f, 1f, 0.5f + (0.5f * (distance - reach) / smoothness));
            float correction = smoothness * blend * (1f - blend);
            (distance, site) = (float.Lerp(distance, reach, blend) - correction, Vector4.Lerp(site, next, blend) - (correction / (1f + (3f * smoothness)) * Live));
        }
        return (distance, At(cell, site));
    }

    internal (float Distance, Vector4 Site) Edge(Vector4 point, float randomness, uint field) {
        (Vector128<int> cell, Vector4 local) = Split(point * Live);
        Vector4 closest = Closest(cell, local, randomness, field).Site;
        float edge = float.MaxValue;
        foreach (Vector4 offset in Near[Key].AsSpan()) {
            Vector4 toward = Site(cell, offset, randomness, field);
            edge = Vector4.DistanceSquared(toward, closest) > 0.0001f ? float.Min(edge, Vector4.Dot(((closest + toward) / 2f) - local, Vector4.Normalize(toward - closest))) : edge;
        }
        return (edge, At(cell, closest));
    }

    internal (float Distance, Vector4 Site) Radius(Vector4 point, float randomness, uint field) {
        (Vector128<int> cell, Vector4 local) = Split(point * Live);
        (Vector4 offset, Vector4 closest) = Closest(cell, local, randomness, field);
        float gap = float.MaxValue;
        foreach (Vector4 step in Near[Key].AsSpan())
            gap = step == Vector4.Zero ? gap : float.Min(gap, Vector4.Distance(Site(cell, offset + step, randomness, field), closest));
        return (gap / 2f, At(cell, closest));
    }

    internal float CellBound(float exponent, float randomness, float smoothness, int span) {
        ImmutableArray<Vector4> scanned = smoothness is 0f ? Near[Key] : Wide[Key];
        float reach = scanned.Select(offset => Length(Vector4.Max(Vector4.Abs(offset - Live), Vector4.Abs(offset + (randomness * Live))), exponent)).Order().ElementAt(smoothness is 0f ? span - 1 : scanned.Length - 1);
        float peak = smoothness is 0f ? reach : Bound(reach + (smoothness / 2d * scanned.Length / 4d), 8 * scanned.Length);
        return peak / (span * Length((0.5f + (0.5f * randomness)) * Live, exponent));
    }

    internal static float Length(Vector4 delta, float exponent) =>
        Vector4.Abs(delta) switch {
            var magnitude => exponent switch {
                1f => Vector4.Sum(magnitude),
                2f => magnitude.Length(),
                float.PositiveInfinity => float.Max(float.Max(magnitude.X, magnitude.Y), float.Max(magnitude.Z, magnitude.W)),
                _ => float.Pow(Vector4.Sum(Vector4.Exp(exponent * Vector4.Log(magnitude))), 1f / exponent),
            },
        };

    private Vector4 Site(Vector128<int> cell, Vector4 offset, float randomness, uint field) =>
        offset + (CoordinateHash.Uniforms(cell + Step(offset), field) * Live * randomness);

    private (Vector4 Offset, Vector4 Site) Closest(Vector128<int> cell, Vector4 local, float randomness, uint field) {
        (Vector4 Offset, Vector4 Site) closest = (Vector4.Zero, new Vector4(float.MaxValue));
        foreach (Vector4 offset in Near[Key].AsSpan())
            closest = Site(cell, offset, randomness, field) switch { var site => Vector4.DistanceSquared(site, local) < Vector4.DistanceSquared(closest.Site, local) ? (offset, site) : closest };
        return closest;
    }

    // --- [GABOR]
    public Vector2 Gabor(Vector4 point, float frequency, float anisotropy, float orientation, uint field) {
        (int rank, Vector4 live) = (GaborRank, Mask(GaborRank));
        (Vector128<int> cell, Vector4 local) = Split(point * Live * live);
        Vector2 phasor = Vector2.Zero;
        for (uint impulse = 0u; impulse < Impulses; impulse++) {
            (uint centers, uint turns) = (CoordinateHash.Branch(field, 2u * impulse), CoordinateHash.Branch(field, (2u * impulse) + 1u));
            foreach (Vector4 neighbor in Near[rank].AsSpan()) {
                Vector128<int> at = cell + Step(neighbor);
                Vector4 center = CoordinateHash.Uniforms(at, centers);
                Vector4 reach = local - neighbor - (center * live);
                float spread = reach.LengthSquared();
                (float sin, float cos) = spread < 1f ? float.SinCos(float.Tau * frequency * Vector4.Dot(reach, Orient(rank, orientation, 1f - anisotropy, CoordinateHash.Uniforms(at, turns)))) : (0f, 0f);
                phasor += float.CopySign(float.Exp(-float.Pi * spread) * (0.5f + (0.5f * float.Cos(float.Pi * spread))), center.W - 0.5f) * new Vector2(cos, sin);
            }
        }
        return phasor / GaborNormalization(rank);
    }

    public float GaborBound => Impulses * Near[GaborRank].Length / GaborNormalization(GaborRank);

    private int GaborRank => int.Clamp(Key, 2, 3);

    private static float GaborNormalization(int rank) => 6f * float.Sqrt(Impulses * 0.5f * float.Exp2(-1f - (rank / 2f)));

    private static Vector4 Orient(int rank, float angle, float isotropy, Vector4 draw) =>
        rank is 2
            ? float.SinCos(angle + ((draw.X - 0.5f) * float.Pi * isotropy)) switch { var (sin, cos) => new Vector4(cos, sin, 0f, 0f) }
            : (float.SinCos((float.Pi / 2f) + (draw.X * float.Pi * isotropy)), float.SinCos(angle + (draw.Y * float.Pi * isotropy))) switch {
                var ((sinInclination, cosInclination), (sinAzimuth, cosAzimuth)) => new Vector4(sinInclination * cosAzimuth, sinInclination * sinAzimuth, cosInclination, 0f),
            };

    // --- [SPECTRAL]
    public static PixelFrame Spectral(PixelExtent extent, double exponent, double scale, double coverage, uint field) {
        static double Frequency(int index, int length) => (index <= length / 2 ? index : index - length) / (double)length;
        Size size = new(CvInvoke.GetOptimalDFTSize(int.Max(2, extent.Width)), CvInvoke.GetOptimalDFTSize(int.Max(2, extent.Height)));
        using Mat draw = new(size.Height, size.Width, DepthType.Cv64F, 1);
        using Mat shaping = new(size.Height, size.Width, DepthType.Cv64F, 2);
        using Mat spectrum = new();
        double knee = 1d / scale;
        double reference = double.Pow(1d / int.Max(size.Width, size.Height), 2d) + (knee * knee);
        Span2D<double> cells = draw.View<double>();
        Span2D<Complex> gains = shaping.View<Complex>();
        for (int y = 0; y < size.Height; y++) {
            for (int x = 0; x < size.Width; x++) {
                double gain = x is 0 && y is 0 ? 0d : double.Pow((double.Pow(Frequency(x, size.Width), 2d) + double.Pow(Frequency(y, size.Height), 2d) + (knee * knee)) / reference, -exponent / 4d);
                (cells[y, x], gains[y, x]) = (Normal.InvCDF(0d, 1d, CoordinateHash.Uniforms(Vector128.Create(x, y, 0, 0), field).X), new Complex(gain, gain));
            }
        }
        CvInvoke.Dft(draw, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.Multiply(spectrum, shaping, spectrum);
        CvInvoke.Dft(spectrum, draw, DxtType.Inverse | DxtType.RealOutput | DxtType.Scale, 0);
        using Mat window = new(draw, new Rectangle(Point.Empty, new Size(extent.Width, extent.Height)));
        using Mat level = new();
        window.ConvertTo(level, DepthType.Cv32F);
        using ScalarArray floor = new(ArrayStatistics.QuantileCustomInplace(level.GetSpan<float>().ToArray(), 1d - coverage, QuantileDefinition.R7));
        CvInvoke.Max(level, floor, level);
        CvInvoke.Normalize(level, level, 0d, 1d, NormType.MinMax);
        PixelFrame specks = new(Point.Empty, extent, extent);
        using VectorOfMat planes = new(level, level, level, level);
        using Mat pixels = specks.Header();
        CvInvoke.Merge(planes, pixels);
        return specks;
    }

    // --- [LATTICE]
    private static NoiseDimensions Lattice(int rank, float scale, Func<uint, Vector4, float> slope) {
        (Vector4 live, float peak) = (Mask(rank), 0f);
        for (uint top = 0u; top < 32u; top++)
            foreach (Vector4 corner in Corners[rank].AsSpan())
                peak = float.Max(peak, float.Abs(slope(top << 27, ((2f * corner) - Vector4.One) * live)));
        return new(rank, scale, Bound(scale * peak, rank + Corners[rank].Length + 4), slope);
    }

    internal static float Bound(double magnitude, int operations) {
        double roundoff = (float.BitIncrement(1f) - 1f) / 2d;
        double growth = double.Pow(1d / (1d - roundoff), operations);
        double bound = (magnitude * growth) + (float.Epsilon / 2d * (growth - 1d) / roundoff);
        return (float)bound switch { var rounded => rounded < bound ? float.BitIncrement(rounded) : rounded };
    }

    private static Vector4 Mask(int rank) => Vector4.Clamp(Vector4.Create(rank) - new Vector4(0f, 1f, 2f, 3f), Vector4.Zero, Vector4.One);

    private static (Vector128<int> Cell, Vector4 Local) Split(Vector4 point) =>
        Vector128.Floor(point.AsVector128()) switch { var floor => (Vector128.ConvertToInt32(floor), point - floor.AsVector4()) };

    private static Vector128<int> Step(Vector4 offset) => Vector128.ConvertToInt32(offset.AsVector128());

    private static Vector4 At(Vector128<int> cell, Vector4 local) => Vector128.ConvertToSingle(cell).AsVector4() + local;

    private static ImmutableArray<Vector4>[] Offsets(int low, int high) {
        return [.. Enumerable.Range(0, 5).Select(rank => ImmutableArray.CreateRange(
            from w in Axis(rank, 3) from z in Axis(rank, 2) from y in Axis(rank, 1) from x in Axis(rank, 0) select new Vector4(x, y, z, w)))];

        IEnumerable<int> Axis(int rank, int axis) => axis < rank ? Enumerable.Range(low, high - low + 1) : [0];
    }
}

[SmartEnum<string>]
public sealed partial class Fractal {
    public static readonly Fractal Fbm = new("fbm", static (gradient, _) => gradient, static (dimensions, point, octaves, _, _, field, sample) => {
        (float sum, float total, float level, float amplitude) = (0f, 0f, 0f, 1f);
        for (int octave = 0; octaves.Weight(octave) > 0f; octave++) {
            (sum, total) = (sum + (sample(dimensions, point, field) * amplitude), total + amplitude);
            (level, amplitude, point) = (float.Lerp(level, sum / total, octaves.Weight(octave)), amplitude * octaves.Roughness, point * octaves.Lacunarity);
        }
        return level;
    });
    public static readonly Fractal Multifractal = new("multifractal", OffsetPeak, static (dimensions, point, octaves, offset, _, field, sample) => Product(dimensions, point, octaves, offset, 1f, field, sample));
    public static readonly Fractal HeteroTerrain = new("hetero-terrain", OffsetPeak, static (dimensions, point, octaves, offset, _, field, sample) => Product(dimensions, point, octaves, offset, 0f, field, sample));
    public static readonly Fractal HybridMultifractal = new("hybrid-multifractal", OffsetPeak, static (dimensions, point, octaves, offset, gain, field, sample) => {
        (float value, float weight, float amplitude) = (0f, 1f, 1f);
        for (int octave = 0; weight > 0.001f && octaves.Weight(octave) > 0f; octave++) {
            (float held, float signal) = (float.Min(weight, 1f), (sample(dimensions, point, field) + offset) * amplitude);
            (value, weight, amplitude, point) = (value + (octaves.Weight(octave) * held * signal), held * gain * signal, amplitude * octaves.Roughness, point * octaves.Lacunarity);
        }
        return value;
    });
    public static readonly Fractal RidgedMultifractal = new("ridged-multifractal", OffsetPeak, static (dimensions, point, octaves, offset, gain, field, sample) => {
        (float signal, float value, float amplitude) = (0f, 0f, 1f);
        for (int octave = 0; octaves.Weight(octave) >= 1f; octave++) {
            float ridge = offset - float.Abs(sample(dimensions, point, field));
            signal = ridge * ridge * (octave is 0 ? 1f : float.Clamp(signal * gain, 0f, 1f));
            (value, amplitude, point) = (value + (signal * amplitude), amplitude * octaves.Roughness, point * octaves.Lacunarity);
        }
        return value;
    });

    public float Sample(NoiseDimensions dimensions, Vector4 point, Octaves octaves, float offset, float gain, uint field) =>
        Folded(dimensions, point, octaves, offset, gain, field, static (lattice, at, draw) => lattice.Gradient(at, draw));

    public float Bound(NoiseDimensions dimensions, Octaves octaves, float offset, float gain) =>
        Peak(dimensions.GradientBound, offset) switch { var peak => Folded(dimensions, Vector4.Zero, octaves, 0f, gain, 0u, (_, _, _) => peak) };

    [UseDelegateFromConstructor]
    private partial float Peak(float gradient, float offset);

    [UseDelegateFromConstructor]
    private partial float Folded(NoiseDimensions dimensions, Vector4 point, Octaves octaves, float offset, float gain, uint field, Func<NoiseDimensions, Vector4, uint, float> sample);

    private static float OffsetPeak(float gradient, float offset) => gradient + float.Abs(offset);

    private static float Product(NoiseDimensions dimensions, Vector4 point, Octaves octaves, float offset, float lead, uint field, Func<NoiseDimensions, Vector4, uint, float> sample) {
        (float value, float amplitude) = (1f, 1f);
        for (int octave = 0; octaves.Weight(octave) > 0f; octave++)
            (value, amplitude, point) = (value * ((octave is 0 ? lead : 1f) + (octaves.Weight(octave) * amplitude * (sample(dimensions, point, field) + offset))), amplitude * octaves.Roughness, point * octaves.Lacunarity);
        return value;
    }
}

[SmartEnum<string>]
public sealed partial class VoronoiFeature {
    public static readonly VoronoiFeature F1 = new("f1",
        static (dimensions, point, octaves, exponent, randomness, smoothness, field) => Averaged(dimensions, point, octaves, exponent, randomness, smoothness, 1, field),
        static (dimensions, _, exponent, randomness, smoothness) => (0f, dimensions.CellBound(exponent, randomness, smoothness, 1)));
    public static readonly VoronoiFeature F2 = new("f2",
        static (dimensions, point, octaves, exponent, randomness, _, field) => Averaged(dimensions, point, octaves, exponent, randomness, 0f, 2, field),
        static (dimensions, _, exponent, randomness, _) => (0f, dimensions.CellBound(exponent, randomness, 0f, 2)));
    public static readonly VoronoiFeature DistanceToEdge = new("distance-to-edge",
        static (dimensions, point, octaves, _, randomness, _, field) => EdgeFolded(dimensions, point, octaves, randomness, field, static (lattice, at, jitter, draw) => lattice.Edge(at, jitter, draw)),
        static (dimensions, octaves, _, randomness, _) => NoiseDimensions.Bound(2d * dimensions.Key, (3 * dimensions.Key) + 1) switch {
            var bound => (EdgeFolded(dimensions, Vector4.Zero, octaves, randomness, 0u, (_, _, _, _) => (-bound, Vector4.Zero)).Distance,
                EdgeFolded(dimensions, Vector4.Zero, octaves, randomness, 0u, (_, _, _, _) => (bound, Vector4.Zero)).Distance),
        });
    public static readonly VoronoiFeature NSphereRadius = new("n-sphere-radius",
        static (dimensions, point, _, _, randomness, _, field) => dimensions.Radius(point, randomness, field),
        static (dimensions, _, _, randomness, _) => (0f, NoiseDimensions.Bound(0.5d * (new Vector4(1f + randomness, randomness, randomness, randomness) * dimensions.Live).Length(), (2 * dimensions.Key) + 4)));

    [UseDelegateFromConstructor]
    public partial (float Distance, Vector4 Site) Sample(NoiseDimensions dimensions, Vector4 point, Octaves octaves, float exponent, float randomness, float smoothness, uint field);

    [UseDelegateFromConstructor]
    public partial (float Lower, float Upper) Range(NoiseDimensions dimensions, Octaves octaves, float exponent, float randomness, float smoothness);

    private static (float Distance, Vector4 Site) Averaged(NoiseDimensions dimensions, Vector4 point, Octaves octaves, float exponent, float randomness, float smoothness, int span, uint field) {
        float limit = span * NoiseDimensions.Length((0.5f + (0.5f * randomness)) * dimensions.Live, exponent);
        (float total, float distance, float amplitude, float scale, Vector4 site) = (0f, 0f, 1f, 1f, Vector4.Zero);
        for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
            Vector4 at = point * scale;
            (float value, Vector4 found) = span is 2 ? dimensions.Ranked(at, exponent, randomness, field).Second
                : smoothness is 0f ? dimensions.Ranked(at, exponent, randomness, field).First
                : dimensions.Smoothed(at, exponent, randomness, smoothness / 2f, field);
            float weight = amplitude * octaves.Weight(octave);
            (total, distance, site) = (total + weight, distance + (value * weight), Vector4.Lerp(site, found / scale, weight));
            (amplitude, scale) = (amplitude * octaves.Roughness, scale * octaves.Lacunarity);
        }
        return (distance / (total * limit), site);
    }

    private static (float Distance, Vector4 Site) EdgeFolded(NoiseDimensions dimensions, Vector4 point, Octaves octaves, float randomness, uint field,
        Func<NoiseDimensions, Vector4, float, uint, (float Distance, Vector4 Site)> sample) {
        float limit = 0.5f + (0.5f * randomness);
        (float scale, float bound, float distance, float amplitude, Vector4 site) = (1f, limit, 8f, 1f, Vector4.Zero);
        for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
            float weight = amplitude * octaves.Weight(octave);
            (float edge, Vector4 found) = sample(dimensions, point * scale, randomness, field);
            (bound, distance, site) = (float.Lerp(bound, limit / scale, weight), float.Lerp(distance, float.Min(distance, edge / scale), weight), Vector4.Lerp(site, found / scale, weight));
            (amplitude, scale) = (amplitude * octaves.Roughness, scale * octaves.Lacunarity);
        }
        return (distance / bound, site);
    }
}

[SmartEnum<string>]
public sealed partial class WaveProfile {
    public static readonly WaveProfile Sine = new("sine", static periods => 0.5f - (0.5f * float.CosPi(2f * periods)));
    public static readonly WaveProfile Saw = new("saw", static periods => periods - float.Floor(periods));
    public static readonly WaveProfile Triangle = new("triangle", static periods => 2f * float.Abs(periods - float.Floor(periods + 0.5f)));
    public static readonly WaveProfile Square = new("square", static periods => float.Floor(2f * (periods - float.Floor(periods))));

    [UseDelegateFromConstructor]
    public partial float Level(float periods);

    public float Wave(NoiseDimensions dimensions, float periods, Vector4 point, float distortion, Octaves octaves, float detailScale, uint field) =>
        Level(distortion is 0f ? periods
            : periods + (distortion * Fractal.Fbm.Sample(dimensions, point * (float.Tau / 20f * detailScale), octaves, 0f, 0f, CoordinateHash.Branch(field, 0u)) / float.Tau));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CoordinateHash {
    public static uint Branch(uint field, uint index, uint stream = 0u) => Pcg3d(field, index, stream).X;

    public static Vector4 Uniforms(Vector128<int> cell, uint field) =>
        Hash(cell, field) switch { var (x, y, z) => (new Vector4(x >> 9, y >> 9, z >> 9, Pcg3d(x, y, z).X >> 9) + new Vector4(0.5f)) * (1f / (1 << 23)) };

    public static Vector4 Normals(Vector128<int> cell, uint field) =>
        Uniforms(cell, field) switch { var u => new((float)Normal.InvCDF(0d, 1d, u.X), (float)Normal.InvCDF(0d, 1d, u.Y), (float)Normal.InvCDF(0d, 1d, u.Z), (float)Normal.InvCDF(0d, 1d, u.W)) };

    internal static (uint X, uint Y, uint Z) Hash(Vector128<int> cell, uint field) =>
        cell.AsUInt32() switch { var words => Pcg3d(words[0], words[1], Pcg3d(words[2], words[3], field).X) };

    private static (uint X, uint Y, uint Z) Pcg3d(uint x, uint y, uint z) {
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
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class MagicNoise {
    private static readonly ImmutableArray<Func<Vector3, float, Vector3>> Turbulence = [
        static (v, d) => v with { Y = -float.Cos(v.X - v.Y + v.Z) * d },
        static (v, d) => v with { X = float.Cos(v.X - v.Y - v.Z) * d },
        static (v, d) => v with { Z = float.Sin(-v.X - v.Y - v.Z) * d },
        static (v, d) => v with { X = -float.Cos(-v.X + v.Y - v.Z) * d },
        static (v, d) => v with { Y = -float.Sin(-v.X + v.Y + v.Z) * d },
        static (v, d) => v with { Y = -float.Cos(-v.X + v.Y + v.Z) * d },
        static (v, d) => v with { X = float.Cos(v.X + v.Y + v.Z) * d },
        static (v, d) => v with { Z = float.Sin(v.X + v.Y - v.Z) * d },
        static (v, d) => v with { X = -float.Cos(-v.X - v.Y + v.Z) * d },
        static (v, d) => v with { Y = -float.Sin(v.X - v.Y + v.Z) * d },
    ];

    public int Depth { get; }
    public float Distortion { get; }

    public (float Lower, float Upper) Range =>
        (Vector3.Sum(Centered(new Vector3(Depth is 0 ? 1f : float.Abs(Distortion)))) / 3f) switch { var first => (float.Min(first, 1f - first), float.Max(first, 1f - first)) };

    public Vector3 Sample(Vector3 point) {
        Vector3 wrapped = point - (float.Tau * Vector128.Floor((point / float.Tau).AsVector128()).AsVector3());
        Vector3 turned = new Vector3(float.Sin((wrapped.X + wrapped.Y + wrapped.Z) * 5f), float.Cos((-wrapped.X + wrapped.Y - wrapped.Z) * 5f), -float.Cos((-wrapped.X - wrapped.Y + wrapped.Z) * 5f))
            * (Depth is 0 ? 1f : Distortion);
        foreach (Func<Vector3, float, Vector3> step in Turbulence.AsSpan()[..Depth])
            turned = step(turned, Distortion);
        return Centered(turned);
    }

    public static Fin<MagicNoise> Validated(int depth, float distortion) =>
        ImagingFault.Unless(depth >= 0 && depth <= Turbulence.Length && float.IsFinite(distortion), ImagingReason.Range, (depth, distortion)).Map(_ => new MagicNoise(depth, distortion));

    private Vector3 Centered(Vector3 turned) => new Vector3(0.5f) - (Distortion is 0f ? turned : turned / (2f * Distortion));
}
