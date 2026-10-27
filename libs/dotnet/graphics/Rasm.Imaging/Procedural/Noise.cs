using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using MathNet.Numerics.Distributions;

namespace Rasm.Imaging.Procedural;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CellMetric {
    public static readonly CellMetric Euclidean = new("euclidean", static (delta, _) => delta.Length());
    public static readonly CellMetric Manhattan = new("manhattan", static (delta, _) => Vector4.Sum(Vector4.Abs(delta)));
    public static readonly CellMetric Chebyshev = new("chebyshev", static (delta, _) => Vector4.Abs(delta) switch { var a => float.Max(float.Max(a.X, a.Y), float.Max(a.Z, a.W)) });
    public static readonly CellMetric Minkowski = new("minkowski", static (delta, exponent) => float.Pow(Vector4.Sum(Vector4.Exp(exponent * Vector4.Log(Vector4.Abs(delta)))), 1f / exponent));

    [UseDelegateFromConstructor]
    public partial float Length(Vector4 delta, float exponent);
}

[SmartEnum<string>]
public sealed partial class WaveProfile {
    public static readonly WaveProfile Sine = new("sine", static periods => 0.5f - (0.5f * float.CosPi(2f * periods)));
    public static readonly WaveProfile Saw = new("saw", static periods => periods - float.Floor(periods));
    public static readonly WaveProfile Triangle = new("triangle", static periods => 2f * float.Abs(periods - float.Floor(periods + 0.5f)));
    public static readonly WaveProfile Square = new("square", static periods => float.Floor(2f * (periods - float.Floor(periods))));

    [UseDelegateFromConstructor]
    public partial float Level(float periods);
}

public sealed record Octaves(float Detail, float Roughness, float Lacunarity) {
    public bool Degenerate => Lacunarity is 0f && Detail > 0f && Roughness > 0f;

    public float Weight(int octave) => float.Min(1f, Detail - (octave - 1));
}

public sealed record VoronoiCells(CellMetric Metric, float Exponent, float Randomness) {
    public float Length(Vector4 delta) => Metric.Length(delta, Exponent);
}

[SmartEnum<string>]
public sealed partial class WaveForm {
    public static readonly WaveForm Bands = new("bands", static point => point[0]);
    public static readonly WaveForm Rings = new("rings", static point => double.Hypot(point[0], point[1]));

    [UseDelegateFromConstructor]
    public partial double Periods(Vector256<double> point);
}

public sealed record MagicNoise(int Depth, float Distortion) {
    private static readonly Arr<Func<Vector3, float, Vector3>> Turbulence = [
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

    public Fin<(float Lower, float Upper)> Range {
        get {
            Vector3 peak = Vector3.Create(Depth is 0 ? 1f : float.Abs(Distortion));
            (float first, float second) = (Vector3.Sum(Centered(peak)) / 3f, Vector3.Sum(Centered(-peak)) / 3f);
            return float.IsFinite(first) && float.IsFinite(second) ? (float.Min(first, second), float.Max(first, second))
                : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"magic noise range at depth {Depth} and distortion {Distortion}"), ImagingReason.Range);
        }
    }

    public Vector3 Sample(Vector256<double> point) {
        const double period = float.Tau;
        Vector3 p = Vector256.Narrow(point - (period * Vector256.Floor(point / period)), Vector256<double>.Zero).GetLower().AsVector3();
        Vector3 turned = new Vector3(float.Sin((p.X + p.Y + p.Z) * 5f), float.Cos((-p.X + p.Y - p.Z) * 5f), -float.Cos((-p.X - p.Y + p.Z) * 5f)) * (Depth is 0 ? 1f : Distortion);
        foreach (Func<Vector3, float, Vector3> step in Turbulence.AsSpan()[..Depth])
            turned = step(turned, Distortion);
        return Centered(turned);
    }

    private Vector3 Centered(Vector3 turned) => new Vector3(0.5f) - (Distortion is 0f ? turned : turned / (2f * Distortion));
}

internal sealed record Neighborhood(
    int Rank, Vector4 Live, Vector256<double> Axes, Arr<(Vector4 Offset, Vector256<long> Step)> Corners, Arr<(Vector4 Offset, Vector256<long> Step)> Near,
    Arr<(Vector4 Offset, Vector256<long> Step)> Wide) {
    public static Neighborhood Of(int rank) =>
        Vector4.Clamp(Vector4.Create(rank) - new Vector4(0f, 1f, 2f, 3f), Vector4.Zero, Vector4.One) switch {
            var live => new(rank, live, Widened(live), Offsets(rank, live, 0, 1), Offsets(rank, live, -1, 1), Offsets(rank, live, -2, 2)),
        };

    public static Vector256<double> Widened(Vector4 value) => Vector256.WidenLower(value.AsVector128().ToVector256());

    private static Arr<(Vector4 Offset, Vector256<long> Step)> Offsets(int rank, Vector4 live, int low, int high) {
        return [.. from w in Axis(3) from z in Axis(2) from y in Axis(1) from x in Axis(0)
                   let offset = new Vector4(x, y, z, w) * live
                   select (offset, Vector256.ConvertToInt64(Widened(offset)))];

        IEnumerable<int> Axis(int axis) => axis < rank ? Enumerable.Range(low, high - low + 1) : [0];
    }
}

internal sealed record GaborKernel(Neighborhood Cells, Func<float, float, Vector4, Vector4> Orient) {
    internal const uint Impulses = 8u;
    internal float Normalization => 6f * float.Sqrt(Impulses * 0.5f * float.Exp2(-1f - (Cells.Rank / 2f)));
}

[SmartEnum<string>]
public sealed partial class NoiseDimensions {
    private const double GradientRepeat = 100000d;
    private static readonly GaborKernel Planar = new(Neighborhood.Of(2), static (angle, isotropy, draw) =>
        float.SinCos(angle + ((draw.X - 0.5f) * float.Pi * isotropy)) switch { var (sin, cos) => new Vector4(cos, sin, 0f, 0f) });
    private static readonly GaborKernel Spherical = new(Neighborhood.Of(3), static (angle, isotropy, draw) =>
        (float.SinCos((float.Pi / 2f) + (draw.X * float.Pi * isotropy)), float.SinCos(angle + (draw.Y * float.Pi * isotropy))) switch {
            var ((sinInclination, cosInclination), (sinAzimuth, cosAzimuth)) => new Vector4(sinInclination * cosAzimuth, sinInclination * sinAzimuth, cosInclination, 0f),
        });

    public static readonly NoiseDimensions One = new("1d", Neighborhood.Of(1), 0.25f, Planar, static (word, offset) =>
        (word >> 28) switch { var h => (1u + (h & 7u)) * ((h & 8u) is not 0u ? -offset.X : offset.X) });
    public static readonly NoiseDimensions Two = new("2d", Planar.Cells, 0.6616f, Planar, static (word, offset) =>
        (word >> 29) switch { var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 4u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -2f : 2f) * (h < 4u ? offset.Y : offset.X)) });
    public static readonly NoiseDimensions Three = new("3d", Spherical.Cells, 0.982f, Spherical, static (word, offset) =>
        (word >> 28) switch { var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 8u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -1f : 1f) * (h < 4u ? offset.Y : h is 12u or 14u ? offset.X : offset.Z)) });
    public static readonly NoiseDimensions Four = new("4d", Neighborhood.Of(4), 0.8344f, Spherical, static (word, offset) =>
        (word >> 27) switch {
            var h => (((h & 1u) is not 0u ? -1f : 1f) * (h < 24u ? offset.X : offset.Y)) + (((h & 2u) is not 0u ? -1f : 1f) * (h < 16u ? offset.Y : offset.Z))
                + (((h & 4u) is not 0u ? -1f : 1f) * (h < 8u ? offset.Z : offset.W)),
        });

    public static float Peak => Items.Max(static lattice => lattice.Scale * SlopePeaks(lattice));

    internal Neighborhood Cells { get; }
    internal float Scale { get; }
    internal GaborKernel Kernel { get; }
    private static readonly Func<NoiseDimensions, float> SlopePeaks = memo(static (NoiseDimensions lattice) => Enumerable.Range(0, 32).Max(top => lattice.Cells.Corners.Max(corner => float.Abs(lattice.Slope((uint)top << 27, ((2f * corner.Offset) - Vector4.One) * lattice.Cells.Live)))));

    [UseDelegateFromConstructor]
    internal partial float Slope(uint word, Vector4 offset);

    // --- [GRADIENTS]
    public Vector4 White(Vector256<double> point, uint field) => CoordinateHash.White(point * Cells.Axes, field);

    public float Gradient(Vector256<double> point, uint field) {
        (Vector256<long> cell, Vector4 local) = Split((point - (GradientRepeat * Vector256.Truncate(point / GradientRepeat))
            + Vector256.ConditionalSelect(Vector256.GreaterThanOrEqual(Vector256.Abs(point), Vector256.Create(10d * GradientRepeat)), Vector256.Create(0.5d), Vector256<double>.Zero)
            + GradientShift(field)) * Cells.Axes);
        Vector4 fade = new((float)ShapeEdge.Smoother((double)local.X), (float)ShapeEdge.Smoother((double)local.Y), (float)ShapeEdge.Smoother((double)local.Z), (float)ShapeEdge.Smoother((double)local.W));
        float level = 0f;
        foreach ((Vector4 corner, Vector256<long> step) in Cells.Corners.AsSpan()) {
            Vector4 weights = Vector4.Lerp(Vector4.One - fade, fade, corner);
            level += weights.X * weights.Y * weights.Z * weights.W * Slope(CoordinateHash.Hash(cell + step, field).X, local - corner);
        }
        return Scale * level;
    }

    public Vector256<double> Distort(Vector256<double> point, float distortion, uint field) {
        Vector4 moved = Vector4.Zero;
        for (int axis = 0; axis < (distortion is 0f ? 0 : Cells.Rank); axis++)
            moved[axis] = distortion * Gradient(point, CoordinateHash.Branch(field, (uint)axis));
        return point + Neighborhood.Widened(moved);
    }

    private static Vector256<double> GradientShift(uint field) => Neighborhood.Widened(100f * (Vector4.One + CoordinateHash.White(Vector256<double>.Zero, field)));

    private static (Vector256<long> Cell, Vector4 Local) Split(Vector256<double> point) =>
        Vector256.Floor(point) switch { var floor => (Vector256.ConvertToInt64(floor), Vector256.Narrow(point - floor, Vector256<double>.Zero).GetLower().AsVector4()) };

    private static Vector256<double> At(Vector256<long> cell, Vector4 local) => Vector256.ConvertToDouble(cell) + Neighborhood.Widened(local);

    // --- [BOUNDS]
    public float FbmBound(Octaves octaves) => GradientBound switch { var peak => Fbm(Vector256<double>.Zero, octaves, 0u, (_, _, _) => peak) };

    public float CascadeBound(Octaves octaves, float offset, float lead) =>
        GradientBound switch { var peak => Cascade(Vector256<double>.Zero, octaves, float.Abs(offset), lead, 0u, (_, _, _) => peak) };

    public float HybridBound(Octaves octaves, float offset, float gain) =>
        GradientBound switch { var peak => HybridMultifractal(Vector256<double>.Zero, octaves, float.Abs(offset), gain, 0u, (_, _, _) => peak) };

    public float RidgedBound(Octaves octaves, float offset, float gain) =>
        (float.Abs(offset) + GradientBound) switch { var peak => RidgedMultifractal(Vector256<double>.Zero, octaves, 0f, gain, 0u, (_, _, _) => peak) };

    public float CellBound(VoronoiCells cells, float smoothness, float span) {
        float blend = smoothness / 2f;
        float reach = (blend is 0f ? Cells.Near : Cells.Wide).Select(near => cells.Length(Vector4.Max(Vector4.Abs(near.Offset - Cells.Live), Vector4.Abs(near.Offset + (cells.Randomness * Cells.Live)))))
            .Order().ElementAt(blend is 0f ? (int)span - 1 : Cells.Wide.Count - 1);
        float peak = blend is 0f ? reach : RoundBound(reach + (blend * (double)Cells.Wide.Count / 4d), 8 * Cells.Wide.Count);
        return peak / (span * cells.Length((0.5f + (0.5f * cells.Randomness)) * Cells.Live));
    }

    public (float Lower, float Upper) EdgeRange(Octaves octaves, float randomness) =>
        RoundBound(2d * Cells.Rank, (3 * Cells.Rank) + 1) switch {
            var bound => (EdgeDistance(Vector256<double>.Zero, octaves, randomness, 0u, (_, _, _, _) => -bound),
                EdgeDistance(Vector256<double>.Zero, octaves, randomness, 0u, (_, _, _, _) => bound)),
        };

    public float GaborBound => GaborKernel.Impulses * Kernel.Cells.Near.Count / Kernel.Normalization;

    private float GradientBound => RoundBound((double)Scale * SlopePeaks(this), Cells.Rank + Cells.Corners.Count + 4);

    private static float RoundBound(double magnitude, int operations) {
        double roundoff = (float.BitIncrement(1f) - 1f) / 2d;
        double growth = double.Pow(1d / (1d - roundoff), operations);
        double bound = (magnitude * growth) + (float.Epsilon / 2d * (growth - 1d) / roundoff);
        return (float)bound switch { var rounded => rounded < bound ? float.BitIncrement(rounded) : rounded };
    }

    // --- [FRACTALS]
    public float Fbm(Vector256<double> point, Octaves octaves, uint field) => Fbm(point, octaves, field, static (lattice, at, draw) => lattice.Gradient(at, draw));

    public float Cascade(Vector256<double> point, Octaves octaves, float offset, float lead, uint field) =>
        Cascade(point, octaves, offset, lead, field, static (lattice, at, draw) => lattice.Gradient(at, draw));

    public float HybridMultifractal(Vector256<double> point, Octaves octaves, float offset, float gain, uint field) =>
        HybridMultifractal(point, octaves, offset, gain, field, static (lattice, at, draw) => lattice.Gradient(at, draw));

    public float RidgedMultifractal(Vector256<double> point, Octaves octaves, float offset, float gain, uint field) =>
        RidgedMultifractal(point, octaves, offset, gain, field, static (lattice, at, draw) => lattice.Gradient(at, draw));

    private float Fbm(Vector256<double> point, Octaves octaves, uint field, Func<NoiseDimensions, Vector256<double>, uint, float> sample) {
        (float sum, float total, float level, float amplitude) = (0f, 0f, 0f, 1f);
        for (int octave = 0; octaves.Weight(octave) > 0f; octave++) {
            (sum, total) = (sum + (sample(this, point, field) * amplitude), total + amplitude);
            (level, amplitude, point) = (float.Lerp(level, sum / total, octaves.Weight(octave)), amplitude * octaves.Roughness, point * octaves.Lacunarity);
        }
        return level;
    }

    private float Cascade(Vector256<double> point, Octaves octaves, float offset, float lead, uint field, Func<NoiseDimensions, Vector256<double>, uint, float> sample) {
        (float value, float amplitude) = (1f, 1f);
        for (int octave = 0; octaves.Weight(octave) > 0f; octave++)
            (value, amplitude, point) = (value * ((octave is 0 ? lead : 1f) + (octaves.Weight(octave) * amplitude * (sample(this, point, field) + offset))), amplitude * octaves.Roughness, point * octaves.Lacunarity);
        return value;
    }

    private float HybridMultifractal(Vector256<double> point, Octaves octaves, float offset, float gain, uint field, Func<NoiseDimensions, Vector256<double>, uint, float> sample) {
        (float value, float weight, float amplitude) = (0f, 1f, 1f);
        for (int octave = 0; weight > 0.001f && octaves.Weight(octave) > 0f; octave++) {
            (float held, float signal) = (float.Min(weight, 1f), (sample(this, point, field) + offset) * amplitude);
            (value, weight, amplitude, point) = (value + (octaves.Weight(octave) * held * signal), held * gain * signal, amplitude * octaves.Roughness, point * octaves.Lacunarity);
        }
        return value;
    }

    private float RidgedMultifractal(Vector256<double> point, Octaves octaves, float offset, float gain, uint field, Func<NoiseDimensions, Vector256<double>, uint, float> sample) {
        (float signal, float value, float amplitude) = (0f, 0f, 1f);
        for (int octave = 0; octaves.Weight(octave) >= 1f; octave++) {
            float ridge = offset - float.Abs(sample(this, point, field));
            signal = ridge * ridge * (octave is 0 ? 1f : float.Clamp(signal * gain, 0f, 1f));
            (value, amplitude, point) = (value + (signal * amplitude), amplitude * octaves.Roughness, point * octaves.Lacunarity);
        }
        return value;
    }

    // --- [CELLS]
    public static (float Value, (double X, double Y) Site, (double X, double Y) Delta, (long X, long Y) Cell) PlanarF1(double x, double y, VoronoiCells cells, uint field) {
        (double column, double line) = (double.Floor(x), double.Floor(y));
        (double localX, double localY) = (x - column, y - line);
        Span<Vector4> reach = stackalloc Vector4[Two.Cells.Near.Count];
        ((float Value, Vector4 Site) first, _, Vector256<long> step) = Two.Ranked(new Vector4((float)localX, (float)localY, 0f, 0f), cells,
            (Column: (long)column, Line: (long)line, cells.Randomness, Field: field),
            static (origin, near) => near.Offset + new Vector4(JitteredCells.Offset(origin.Column + near.Step[0], origin.Line + near.Step[1], origin.Randomness, origin.Field), 0f, 0f), reach);
        return (first.Value, (column + first.Site.X, line + first.Site.Y), (first.Site.X - localX, first.Site.Y - localY), ((long)column + step[0], (long)line + step[1]));
    }

    public (float Value, float EdgeDistance, Vector256<long> Cell) Voronoi(Vector256<double> point, VoronoiCells cells, uint field) {
        Span<Vector4> reach = stackalloc Vector4[Cells.Near.Count];
        ((float Value, Vector256<double> Site) first, _, Vector256<long> cell) = Ranked(point * Cells.Axes, cells, field, reach);
        return (first.Value, Edge(reach) / (0.5f + (0.5f * cells.Randomness)), cell);
    }

    public (float Value, Vector256<double> Site) F1(Vector256<double> point, Octaves octaves, VoronoiCells cells, float smoothness, uint field) =>
        smoothness is 0f
            ? Fractal(point, octaves, cells, 0f, 1f, field, static (lattice, at, voronoi, _, draw) => lattice.Ranked(at, voronoi, draw).First)
            : Fractal(point, octaves, cells, smoothness / 2f, 1f, field, static (lattice, at, voronoi, blend, draw) => lattice.Smooth(at, voronoi, blend, draw));

    public (float Value, Vector256<double> Site) F2(Vector256<double> point, Octaves octaves, VoronoiCells cells, uint field) =>
        Fractal(point, octaves, cells, 0f, 2f, field, static (lattice, at, voronoi, _, draw) => lattice.Ranked(at, voronoi, draw).Second);

    public float EdgeDistance(Vector256<double> point, Octaves octaves, float randomness, uint field) =>
        EdgeDistance(point * Cells.Axes, octaves, randomness, field, static (lattice, at, jitter, draw) => lattice.Edge(at, jitter, draw));

    private float EdgeDistance(Vector256<double> point, Octaves octaves, float randomness, uint field, Func<NoiseDimensions, Vector256<double>, float, uint, float> sample) {
        float limit = 0.5f + (0.5f * randomness);
        (double scale, double bound, double distance, float amplitude) = (1d, limit, 8d, 1f);
        for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
            float weight = amplitude * octaves.Weight(octave);
            (double held, double blend) = (1f - weight, weight);
            double nearest = double.Min(distance, sample(this, point * scale, randomness, field) / scale);
            (bound, distance) = ((bound * held) + (limit / scale * blend), (distance * held) + (nearest * blend));
            (amplitude, scale) = (amplitude * octaves.Roughness, scale * octaves.Lacunarity);
        }
        return (float)(distance / bound);
    }

    private (float Value, Vector256<double> Site) Fractal(Vector256<double> point, Octaves octaves, VoronoiCells cells, float smoothness, float span, uint field,
        Func<NoiseDimensions, Vector256<double>, VoronoiCells, float, uint, (float Value, Vector256<double> Site)> scan) {
        float limit = span * cells.Length((0.5f + (0.5f * cells.Randomness)) * Cells.Live);
        (Vector256<double> masked, float total, float distance, float amplitude, double scale, Vector256<double> site) = (point * Cells.Axes, 0f, 0f, 1f, 1d, Vector256<double>.Zero);
        for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
            (float value, Vector256<double> found) = scan(this, masked * scale, cells, smoothness, field);
            float weight = amplitude * octaves.Weight(octave);
            (total, distance, site) = (total + weight, distance + (value * weight), Vector256.Lerp(site, found / scale, Vector256.Create((double)weight)));
            (amplitude, scale) = (amplitude * octaves.Roughness, scale * octaves.Lacunarity);
        }
        return (distance / (total * limit), site);
    }

    private ((float Value, Vector256<double> Site) First, (float Value, Vector256<double> Site) Second, Vector256<long> Cell) Ranked(Vector256<double> point, VoronoiCells cells, uint field) {
        Span<Vector4> reach = stackalloc Vector4[Cells.Near.Count];
        return Ranked(point, cells, field, reach);
    }

    private ((float Value, Vector256<double> Site) First, (float Value, Vector256<double> Site) Second, Vector256<long> Cell) Ranked(
        Vector256<double> point, VoronoiCells cells, uint field, Span<Vector4> reach) {
        (Vector256<long> cell, Vector4 local) = Split(point);
        ((float Value, Vector4 Site) first, (float Value, Vector4 Site) second, Vector256<long> step) = Ranked(local, cells, (Cell: cell, Lattice: this, cells.Randomness, Field: field),
            static (origin, near) => origin.Lattice.Site(origin.Cell, near, origin.Randomness, origin.Field), reach);
        return ((first.Value, At(cell, first.Site)), (second.Value, At(cell, second.Site)), cell + step);
    }

    private ((float Value, Vector4 Site) First, (float Value, Vector4 Site) Second, Vector256<long> Step) Ranked<T>(
        Vector4 local, VoronoiCells cells, T origin, Func<T, (Vector4 Offset, Vector256<long> Step), Vector4> locate, Span<Vector4> reach) {
        ((float Value, Vector4 Site) First, (float Value, Vector4 Site) Second, Vector256<long> Step) nearest = ((float.MaxValue, Vector4.Zero), (float.MaxValue, Vector4.Zero), Vector256<long>.Zero);
        int index = 0;
        foreach ((Vector4 Offset, Vector256<long> Step) near in Cells.Near.AsSpan()) {
            Vector4 site = locate(origin, near);
            reach[index] = site - local;
            float distance = cells.Length(reach[index]);
            nearest = distance < nearest.First.Value ? ((distance, site), nearest.First, near.Step)
                : distance < nearest.Second.Value ? (nearest.First, (distance, site), nearest.Step)
                : nearest;
            index++;
        }
        return nearest;
    }

    private (float Value, Vector256<double> Site) Smooth(Vector256<double> point, VoronoiCells cells, float smoothness, uint field) {
        (Vector256<long> cell, Vector4 local) = Split(point);
        ReadOnlySpan<(Vector4 Offset, Vector256<long> Step)> wide = Cells.Wide.AsSpan();
        (float distance, Vector4 site) = (0f, Vector4.Zero);
        for (int index = 0; index < wide.Length; index++) {
            Vector4 next = Site(cell, wide[index], cells.Randomness, field);
            float reach = cells.Length(next - local);
            float blend = index is 0 ? 1f : (float)ShapeEdge.Smooth((double)float.Clamp(0.5f + (0.5f * (distance - reach) / smoothness), 0f, 1f));
            float correction = smoothness * blend * (1f - blend);
            (distance, site) = (float.Lerp(distance, reach, blend) - correction, Vector4.Lerp(site, next, blend) - (correction / (1f + (3f * smoothness)) * Cells.Live));
        }
        return (distance, At(cell, site));
    }

    private float Edge(Vector256<double> point, float randomness, uint field) {
        (Vector256<long> cell, Vector4 local) = Split(point);
        ReadOnlySpan<(Vector4 Offset, Vector256<long> Step)> near = Cells.Near.AsSpan();
        Span<Vector4> reach = stackalloc Vector4[near.Length];
        for (int index = 0; index < near.Length; index++)
            reach[index] = Site(cell, near[index], randomness, field) - local;
        return Edge(reach);
    }

    private static float Edge(ReadOnlySpan<Vector4> reach) {
        Vector4 closest = reach[0];
        foreach (Vector4 toward in reach)
            closest = toward.LengthSquared() < closest.LengthSquared() ? toward : closest;
        float edge = float.MaxValue;
        foreach (Vector4 toward in reach)
            edge = Vector4.DistanceSquared(toward, closest) > 0.0001f ? float.Min(edge, Vector4.Dot((closest + toward) / 2f, Vector4.Normalize(toward - closest))) : edge;
        return edge;
    }

    private Vector4 Site(Vector256<long> cell, (Vector4 Offset, Vector256<long> Step) near, float randomness, uint field) =>
        near.Offset + (CoordinateHash.Uniforms(CoordinateHash.Hash(cell + near.Step, field)) * Cells.Live * randomness);

    // --- [GABOR]
    public Vector2 Gabor(Vector256<double> point, float frequency, float anisotropy, float orientation, uint field) {
        GaborKernel kernel = Kernel;
        (Vector256<long> cell, Vector4 local) = Split(point * kernel.Cells.Axes);
        Vector2 phasor = Vector2.Zero;
        for (uint impulse = 0u; impulse < GaborKernel.Impulses; impulse++) {
            (uint centers, uint turns) = (CoordinateHash.Branch(field, 2u * impulse), CoordinateHash.Branch(field, (2u * impulse) + 1u));
            foreach ((Vector4 neighbor, Vector256<long> step) in kernel.Cells.Near.AsSpan()) {
                Vector4 center = CoordinateHash.Uniforms(CoordinateHash.Hash(cell + step, centers));
                Vector4 reach = local - neighbor - (center * kernel.Cells.Live);
                float spread = reach.LengthSquared();
                (float sin, float cos) = spread < 1f
                    ? float.SinCos(float.Tau * frequency * Vector4.Dot(reach, kernel.Orient(orientation, 1f - anisotropy, CoordinateHash.Uniforms(CoordinateHash.Hash(cell + step, turns)))))
                    : (0f, 0f);
                phasor += float.CopySign(float.Exp(-float.Pi * spread) * (0.5f + (0.5f * float.Cos(float.Pi * spread))), center.W - 0.5f) * new Vector2(cos, sin);
            }
        }
        return phasor / kernel.Normalization;
    }

    // --- [PATTERNS]
    public float Wave(Vector256<double> point, WaveForm form, WaveProfile profile, float phase, float distortion, Octaves octaves, float detailScale, uint field) {
        double detail = (double)(float.Pi / 10f) * detailScale;
        float bend = distortion is 0f ? 0f : distortion * Fbm(Vector256.Create(point[0] * detail, point[1] * detail, 0d, 0d), octaves, CoordinateHash.Branch(field, 0u));
        double periods = form.Periods(point) - point[2] + ((phase + bend) / float.Tau);
        return profile.Level((float)(periods - double.Floor(periods)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CoordinateHash {
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

    public static uint Field(uint seed, uint term, uint stream) => Pcg3d(seed, term, stream).X;

    public static uint Branch(uint field, uint index) => Pcg3d(field, index, 0u).X;

    public static (uint X, uint Y, uint Z) Hash(Vector256<long> cell, uint field) =>
        unchecked(Pcg3d((uint)cell[0], (uint)cell[1], Pcg3d((uint)cell[2], (uint)cell[3],
            Vector256.EqualsAll(Vector256.ShiftRightArithmetic(cell, 31), Vector256.ShiftRightArithmetic(cell, 63)) ? field
                : Pcg3d((uint)(cell[0] >> 32), (uint)(cell[1] >> 32), Pcg3d((uint)(cell[2] >> 32), (uint)(cell[3] >> 32), field).X).X).X));

    public static Vector4 White(Vector256<double> point, uint field) => Uniforms(Hash(Vector256.ConvertToInt64(Vector256.Floor(point)), field));

    public static Vector4 Uniforms((uint X, uint Y, uint Z) words) =>
        words switch {
            var (x, y, z) => (new Vector4(x >> 9, y >> 9, z >> 9, Pcg3d(x, y, z).X >> 9) + Vector4.Create(0.5f)) * (1f / (1 << 23)),
        };

    public static Vector4 Uniforms(long column, long line, uint field) =>
        Uniforms(unchecked(Pcg3d((uint)column, (uint)line, Pcg3d((uint)(column >> 32), (uint)(line >> 32), field).X)));

    public static Vector4 Normals(long column, long line, uint field) => Normals(Uniforms(column, line, field));

    private static Vector4 Normals(Vector4 uniforms) =>
        new((float)Normal.InvCDF(0d, 1d, uniforms.X), (float)Normal.InvCDF(0d, 1d, uniforms.Y), (float)Normal.InvCDF(0d, 1d, uniforms.Z), (float)Normal.InvCDF(0d, 1d, uniforms.W));
}
