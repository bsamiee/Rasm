using System.Globalization;
using System.Numerics;
using MathNet.Numerics.Distributions;
using PeterO.Numbers;

namespace Rasm.Imaging.Procedural;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IOctaveSource<TSelf> where TSelf : IOctaveSource<TSelf> {
    public static abstract TSelf Advanced(TSelf at, float lacunarity);
    public static abstract float Sampled(NoiseDimensions lattice, TSelf at, uint field);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CellMetric {
    public static readonly CellMetric Euclidean = new("euclidean", static (delta, _) => delta.Length());
    public static readonly CellMetric Manhattan = new("manhattan", static (delta, _) => Vector4.Sum(Vector4.Abs(delta)));
    public static readonly CellMetric Chebyshev = new("chebyshev", static (delta, _) => Vector4.Abs(delta) switch { var a => float.Max(float.Max(a.X, a.Y), float.Max(a.Z, a.W)) });
    public static readonly CellMetric Minkowski = new("minkowski", static (delta, exponent) => MathF.Pow(Vector4.Sum(Vector4.Exp(exponent * Vector4.Log(Vector4.Abs(delta)))), 1f / exponent));

    [UseDelegateFromConstructor]
    public partial float Length(Vector4 delta, float exponent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveProfile {
    public static readonly WaveProfile Sine = new("sine", static periods => 0.5f - (0.5f * float.CosPi(2f * periods)));
    public static readonly WaveProfile Saw = new("saw", static periods => periods - MathF.Floor(periods));
    public static readonly WaveProfile Triangle = new("triangle", static periods => 2f * MathF.Abs(periods - MathF.Floor(periods + 0.5f)));
    public static readonly WaveProfile Square = new("square", static periods => MathF.Floor(2f * (periods - MathF.Floor(periods))));

    [UseDelegateFromConstructor]
    public partial float Level(float periods);
}

public readonly record struct Octaves(float Detail, float Roughness, float Lacunarity) {
    public bool Degenerate => Lacunarity is 0f && Detail > 0f && Roughness > 0f;

    public float Weight(int octave) => float.Min(1f, Detail - (octave - 1));
}

public readonly record struct VoronoiCells(CellMetric Metric, float Exponent, float Randomness) {
    public float Length(Vector4 delta) => Metric.Length(delta, Exponent);
}

public readonly record struct NoiseSample<T>(float Value, T Site);

public readonly record struct LatticeCell(EInteger X, EInteger Y, EInteger Z, EInteger W) {
    public LatticeCell Offset(Vector4 offset) => new(X.Add((int)offset.X), Y.Add((int)offset.Y), Z.Add((int)offset.Z), W.Add((int)offset.W));

    public LatticePoint At(Vector4 local) =>
        new(ERational.FromEInteger(X).Add(ERational.FromSingle(local.X)), ERational.FromEInteger(Y).Add(ERational.FromSingle(local.Y)),
            ERational.FromEInteger(Z).Add(ERational.FromSingle(local.Z)), ERational.FromEInteger(W).Add(ERational.FromSingle(local.W)));

    public (uint X, uint Y, uint Z) Hash(uint field) {
        const int wordBits = sizeof(uint) * 8;
        (EInteger x, EInteger y, EInteger z, EInteger w) = (X.ShiftRight(wordBits), Y.ShiftRight(wordBits), Z.ShiftRight(wordBits), W.ShiftRight(wordBits));
        long bits = long.Max(long.Max(X.GetSignedBitLengthAsInt64(), Y.GetSignedBitLengthAsInt64()), long.Max(Z.GetSignedBitLengthAsInt64(), W.GetSignedBitLengthAsInt64()));
        for (long words = bits / wordBits; words > 0; words--) {
            field = CoordinateHash.Pcg3d(x.ToUInt32Unchecked(), y.ToUInt32Unchecked(), CoordinateHash.Pcg3d(z.ToUInt32Unchecked(), w.ToUInt32Unchecked(), field).X).X;
            (x, y, z, w) = (x.ShiftRight(wordBits), y.ShiftRight(wordBits), z.ShiftRight(wordBits), w.ShiftRight(wordBits));
        }
        return CoordinateHash.Pcg3d(X.ToUInt32Unchecked(), Y.ToUInt32Unchecked(), CoordinateHash.Pcg3d(Z.ToUInt32Unchecked(), W.ToUInt32Unchecked(), field).X);
    }

    public Vector4 White(uint field) => CoordinateHash.Uniforms(Hash(field));
}

public readonly record struct LatticePoint(ERational X, ERational Y, ERational Z, ERational W) : IOctaveSource<LatticePoint> {
    public LatticeCell Cell => new(Split(X).Cell, Split(Y).Cell, Split(Z).Cell, Split(W).Cell);

    static LatticePoint IOctaveSource<LatticePoint>.Advanced(LatticePoint at, float lacunarity) => at.Scaled(ERational.FromSingle(lacunarity));

    static float IOctaveSource<LatticePoint>.Sampled(NoiseDimensions lattice, LatticePoint at, uint field) => lattice.Gradient(at, field);

    public static (EInteger Cell, ERational Local) Split(ERational coordinate) =>
        EFloat.FromEInteger(coordinate.Numerator).DivideToExponent(EFloat.FromEInteger(coordinate.Denominator), 0L, ERounding.Floor).ToEInteger() switch {
            var cell => (cell, coordinate.Subtract(ERational.FromEInteger(cell))),
        };

    public LatticePoint Scaled(ERational scale) => new(X.Multiply(scale), Y.Multiply(scale), Z.Multiply(scale), W.Multiply(scale));

    public LatticePoint Lerp(LatticePoint next, float weight) =>
        (ERational.FromSingle(1f - weight), ERational.FromSingle(weight)) switch {
            var (held, blend) => new(X.Multiply(held).Add(next.X.Multiply(blend)), Y.Multiply(held).Add(next.Y.Multiply(blend)), Z.Multiply(held).Add(next.Z.Multiply(blend)), W.Multiply(held).Add(next.W.Multiply(blend))),
        };

    public LatticePoint Masked(Vector4 axes) => new(X.Multiply((int)axes.X), Y.Multiply((int)axes.Y), Z.Multiply((int)axes.Z), W.Multiply((int)axes.W));

    public (LatticeCell Cell, Vector4 Local) Split() =>
        (Split(X), Split(Y), Split(Z), Split(W)) switch {
            var (x, y, z, w) => (new(x.Cell, y.Cell, z.Cell, w.Cell), new(x.Local.ToSingle(), y.Local.ToSingle(), z.Local.ToSingle(), w.Local.ToSingle())),
        };

    public Vector4 White(uint field) => Cell.White(field);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveForm {
    public static readonly WaveForm Bands = new("bands", static point => point.X, static point => point.X);
    public static readonly WaveForm Rings = new("rings", static point => point.AsVector2().Length(), static point => {
        ERational square = point.X.Multiply(point.X).Add(point.Y.Multiply(point.Y));
        EInteger whole = square.ToEInteger().Sqrt();
        EContext precision = EContext.Binary64.WithUnlimitedExponents();
        EFloat remainder = square.IsZero ? EFloat.Zero : square.Subtract(ERational.FromEInteger(whole.Multiply(whole))).ToEFloat(precision)
            .Divide(square.ToEFloat(precision).Sqrt(precision).Add(EFloat.FromEInteger(whole), precision), precision);
        return ERational.FromEInteger(whole).Add(ERational.FromEFloat(remainder));
    });

    [UseDelegateFromConstructor]
    public partial float Distance(Vector4 point);

    [UseDelegateFromConstructor]
    public partial ERational Periods(LatticePoint point);
}

public readonly record struct MagicNoise(int Depth, float Distortion) {
    private static readonly Arr<Func<Vector3, float, Vector3>> Turbulence = [
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

    public Fin<(float Lower, float Upper)> Range {
        get {
            Vector3 peak = Vector3.Create(Depth is 0 ? 1f : float.Abs(Distortion));
            (float first, float second) = (Vector3.Sum(Centered(peak)) / 3f, Vector3.Sum(Centered(-peak)) / 3f);
            return float.IsFinite(first) && float.IsFinite(second) ? (float.Min(first, second), float.Max(first, second))
                : Error.New(string.Create(CultureInfo.InvariantCulture, $"Magic noise at depth {Depth} and distortion {Distortion} has no finite range"));
        }
    }

    public Vector3 Sample(LatticePoint point) {
        ERational period = ERational.FromSingle(float.Tau);
        Vector3 p = new(LatticePoint.Split(point.X.Divide(period)).Local.Multiply(period).ToSingle(), LatticePoint.Split(point.Y.Divide(period)).Local.Multiply(period).ToSingle(),
            LatticePoint.Split(point.Z.Divide(period)).Local.Multiply(period).ToSingle());
        Vector3 turned = new Vector3(MathF.Sin((p.X + p.Y + p.Z) * 5f), MathF.Cos((-p.X + p.Y - p.Z) * 5f), -MathF.Cos((-p.X - p.Y + p.Z) * 5f)) * (Depth is 0 ? 1f : Distortion);
        foreach (Func<Vector3, float, Vector3> step in Turbulence.AsSpan()[..Depth])
            turned = step(turned, Distortion);
        return Centered(turned);
    }

    private Vector3 Centered(Vector3 turned) => new Vector3(0.5f) - (Distortion is 0f ? turned : turned / (2f * Distortion));
}

internal readonly record struct PeakBound(float Peak) : IOctaveSource<PeakBound> {
    static PeakBound IOctaveSource<PeakBound>.Advanced(PeakBound at, float lacunarity) => at;

    static float IOctaveSource<PeakBound>.Sampled(NoiseDimensions lattice, PeakBound at, uint field) => at.Peak;
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
public sealed partial class NoiseDimensions {
    private const int GradientRepeat = 100000;
    private static readonly GaborKernel Planar = new(Neighborhood.Of(2), static (angle, isotropy, draw) =>
        MathF.SinCos(angle + ((draw.X - 0.5f) * float.Pi * isotropy)) switch { var (sin, cos) => new Vector4(cos, sin, 0f, 0f) });
    private static readonly GaborKernel Spherical = new(Neighborhood.Of(3), static (angle, isotropy, draw) =>
        (MathF.SinCos((float.Pi / 2f) + (draw.X * float.Pi * isotropy)), MathF.SinCos(angle + (draw.Y * float.Pi * isotropy))) switch {
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

    public static float Peak => Items.Max(static lattice => lattice.Scale * lattice.SlopePeak);

    internal Neighborhood Cells { get; }
    internal float Scale { get; }
    internal GaborKernel Kernel { get; }
    private float SlopePeak => Enumerable.Range(0, 32).Max(top => Cells.Corners.Max(corner => MathF.Abs(Slope((uint)top << 27, ((2f * corner) - Vector4.One) * Cells.Live))));

    [UseDelegateFromConstructor]
    internal partial float Slope(uint word, Vector4 offset);

    // --- [GRADIENTS]
    public Vector4 White(LatticePoint point, uint field) => point.Masked(Cells.Live).White(field);

    public float Gradient(Vector4 point, uint field) {
        Vector4 wrapped = (point - (GradientRepeat * Vector4.Truncate(point / GradientRepeat))
            + Vector4.ConditionalSelect(Vector4.GreaterThanOrEqual(Vector4.Abs(point), Vector4.Create(10f * GradientRepeat)), Vector4.Create(0.5f), Vector4.Zero)
            + GradientShift(field)) * Cells.Live;
        Vector4 cell = Vector4.Round(wrapped, MidpointRounding.ToNegativeInfinity);
        return Gradient(cell, wrapped - cell, field);
    }

    public float Gradient(LatticePoint point, uint field) {
        ReadOnlySpan<ERational> coordinates = [point.X, point.Y, point.Z, point.W];
        Vector4 shift = GradientShift(field);
        (Vector4 cell, Vector4 local) = (Vector4.Zero, Vector4.Zero);
        for (int axis = 0; axis < Cells.Rank; axis++) {
            ERational coordinate = coordinates[axis];
            ERational wrapped = coordinate.Remainder(GradientRepeat)
                .Add(ERational.FromSingle(coordinate.Abs().CompareTo(10 * GradientRepeat) >= 0 ? 0.5f : 0f))
                .Add(ERational.FromSingle(shift[axis]));
            (EInteger Cell, ERational Local) split = LatticePoint.Split(wrapped);
            (cell[axis], local[axis]) = (split.Cell.ToInt32Checked(), split.Local.ToSingle());
        }
        return Gradient(cell, local, field);
    }

    public LatticePoint Distort(LatticePoint point, float distortion, uint field) {
        Span<ERational> moved = [point.X, point.Y, point.Z, point.W];
        for (int axis = 0; axis < (distortion is 0f ? 0 : Cells.Rank); axis++)
            moved[axis] = moved[axis].Add(ERational.FromSingle(distortion * Gradient(point, CoordinateHash.Branch(field, (uint)axis))));
        return new(moved[0], moved[1], moved[2], moved[3]);
    }

    private float Gradient(Vector4 cell, Vector4 local, uint field) {
        Vector4 fade = new((float)ShapeEdge.Smoother((double)local.X), (float)ShapeEdge.Smoother((double)local.Y), (float)ShapeEdge.Smoother((double)local.Z), (float)ShapeEdge.Smoother((double)local.W));
        float level = 0f;
        foreach (Vector4 corner in Cells.Corners.AsSpan()) {
            Vector4 weights = Vector4.Lerp(Vector4.One - fade, fade, corner);
            level += weights.X * weights.Y * weights.Z * weights.W * Slope(CoordinateHash.Hash(cell + corner, field).X, local - corner);
        }
        return Scale * level;
    }

    private static Vector4 GradientShift(uint field) => 100f * (Vector4.One + CoordinateHash.White(Vector4.Zero, field));

    // --- [BOUNDS]
    public float FbmBound(Octaves octaves) => Fbm(GradientBound, 1f, octaves, 0u, static (scale, _) => scale, static (_, peak, _, _) => peak);

    public float CascadeBound(Octaves octaves, float offset, float lead) => Cascade(new PeakBound(GradientBound), octaves, float.Abs(offset), lead, 0u);

    public float HybridBound(Octaves octaves, float offset, float gain) => HybridMultifractal(new PeakBound(GradientBound), octaves, float.Abs(offset), gain, 0u);

    public float RidgedBound(Octaves octaves, float offset, float gain) => RidgedMultifractal(new PeakBound(float.Abs(offset) + GradientBound), octaves, 0f, gain, 0u);

    public float CellBound(Octaves octaves, VoronoiCells cells, float smoothness, float span) {
        float blend = smoothness / 2f;
        float reach = (blend is 0f ? Cells.Near : Cells.Wide).Select(offset => cells.Length(Vector4.Max(Vector4.Abs(offset - Cells.Live), Vector4.Abs(offset + (cells.Randomness * Cells.Live)))))
            .Order().ElementAt(blend is 0f ? (int)span - 1 : Cells.Wide.Count - 1);
        float peak = blend is 0f ? reach : RoundBound(EFloat.FromSingle(reach).Add(EFloat.FromSingle(blend).Multiply(Cells.Wide.Count).Divide(4)), 8 * Cells.Wide.Count);
        return Fractal(peak, 0f, 1f, octaves, cells, blend, span, 0u,
            static (at, _) => at, static (scale, _) => scale, static (_, _, _, _) => 0f,
            static (_, at, _, _, _) => new NoiseSample<float>(at, 0f)).Value;
    }

    public (float Lower, float Upper) EdgeRange(Octaves octaves, float randomness) =>
        RoundBound(EFloat.FromInt32(2 * Cells.Rank), (3 * Cells.Rank) + 1) switch {
            var bound => (EdgeDistance(-bound, octaves, randomness, 0u, static (at, _) => at, static (_, at, _, _) => at),
                EdgeDistance(bound, octaves, randomness, 0u, static (at, _) => at, static (_, at, _, _) => at)),
        };

    public float GaborBound => GaborKernel.Impulses * Kernel.Cells.Near.Count / Kernel.Normalization;

    private float GradientBound => RoundBound(EFloat.FromSingle(Scale).Multiply(EFloat.FromSingle(SlopePeak)), Cells.Rank + Cells.Corners.Count + 4);

    private static float RoundBound(EFloat magnitude, int operations) {
        EContext precision = EContext.Binary64.WithRounding(ERounding.Ceiling).WithUnlimitedExponents();
        EFloat roundoff = EFloat.FromSingle(float.BitIncrement(1f) - 1f).Divide(2);
        EFloat growth = EFloat.One.Divide(EFloat.One.Subtract(roundoff), precision).Pow(operations, precision);
        EFloat underflow = EFloat.FromSingle(float.Epsilon).Divide(2).Multiply(growth.Subtract(EFloat.One)).Divide(roundoff, precision);
        return magnitude.Multiply(growth, precision).Add(underflow, precision).RoundToPrecision(EContext.Binary32.WithRounding(ERounding.Ceiling)).ToSingle();
    }

    // --- [FRACTALS]
    public float Fbm(Vector4 point, Octaves octaves, uint field) =>
        Fbm(point, 1f, octaves, field, static (scale, lacunarity) => scale * lacunarity, static (lattice, at, scale, draw) => lattice.Gradient(scale * at, draw));

    public float Fbm(LatticePoint point, Octaves octaves, uint field) =>
        Fbm(point, ERational.One, octaves, field, static (scale, lacunarity) => scale.Multiply(ERational.FromSingle(lacunarity)),
            static (lattice, at, scale, draw) => lattice.Gradient(at.Scaled(scale), draw));

    public float Cascade<T>(T point, Octaves octaves, float offset, float lead, uint field) where T : IOctaveSource<T> {
        (float value, float amplitude) = (1f, 1f);
        for (int octave = 0; octaves.Weight(octave) > 0f; octave++)
            (value, amplitude, point) = (value * ((octave is 0 ? lead : 1f) + (octaves.Weight(octave) * amplitude * (T.Sampled(this, point, field) + offset))), amplitude * octaves.Roughness, T.Advanced(point, octaves.Lacunarity));
        return value;
    }

    public float HybridMultifractal<T>(T point, Octaves octaves, float offset, float gain, uint field) where T : IOctaveSource<T> {
        (float value, float weight, float amplitude) = (0f, 1f, 1f);
        for (int octave = 0; weight > 0.001f && octaves.Weight(octave) > 0f; octave++) {
            (float held, float signal) = (float.Min(weight, 1f), (T.Sampled(this, point, field) + offset) * amplitude);
            (value, weight, amplitude, point) = (value + (octaves.Weight(octave) * held * signal), held * gain * signal, amplitude * octaves.Roughness, T.Advanced(point, octaves.Lacunarity));
        }
        return value;
    }

    public float RidgedMultifractal<T>(T point, Octaves octaves, float offset, float gain, uint field) where T : IOctaveSource<T> {
        (float signal, float value, float amplitude) = (0f, 0f, 1f);
        for (int octave = 0; octaves.Weight(octave) >= 1f; octave++) {
            float ridge = offset - MathF.Abs(T.Sampled(this, point, field));
            signal = ridge * ridge * (octave is 0 ? 1f : float.Clamp(signal * gain, 0f, 1f));
            (value, amplitude, point) = (value + (signal * amplitude), amplitude * octaves.Roughness, T.Advanced(point, octaves.Lacunarity));
        }
        return value;
    }

    private float Fbm<T, TScale>(T point, TScale scale, Octaves octaves, uint field, Func<TScale, float, TScale> advance, Func<NoiseDimensions, T, TScale, uint, float> sample) {
        (float sum, float total, float level, float amplitude) = (0f, 0f, 0f, 1f);
        for (int octave = 0; octaves.Weight(octave) > 0f; octave++) {
            (sum, total) = (sum + (sample(this, point, scale, field) * amplitude), total + amplitude);
            (level, amplitude, scale) = (float.Lerp(level, sum / total, octaves.Weight(octave)), amplitude * octaves.Roughness, advance(scale, octaves.Lacunarity));
        }
        return level;
    }

    // --- [CELLS]
    public static (float Value, (ERational X, ERational Y) Site, (ERational X, ERational Y) Delta, (EInteger X, EInteger Y) Cell) PlanarF1(ERational x, ERational y, VoronoiCells cells, uint field) {
        ((EInteger column, ERational localX), (EInteger line, ERational localY)) = (LatticePoint.Split(x), LatticePoint.Split(y));
        Span<Vector4> reach = stackalloc Vector4[Two.Cells.Near.Count];
        (NoiseSample<Vector4> first, NoiseSample<Vector4> _, Vector4 selected) = Two.Ranked(new Vector4(localX.ToSingle(), localY.ToSingle(), 0f, 0f), cells,
            (Column: column, Line: line, Sites: new JitteredCells(cells.Randomness), Field: field),
            static (origin, offset) => offset + new Vector4(origin.Sites.Offset(origin.Column.Add((int)offset.X), origin.Line.Add((int)offset.Y), origin.Field), 0f, 0f), reach);
        (ERational siteX, ERational siteY) = (ERational.FromSingle(first.Site.X), ERational.FromSingle(first.Site.Y));
        return (first.Value, (ERational.FromEInteger(column).Add(siteX), ERational.FromEInteger(line).Add(siteY)), (siteX.Subtract(localX), siteY.Subtract(localY)),
            (column.Add((int)selected.X), line.Add((int)selected.Y)));
    }

    public (float Value, float EdgeDistance, LatticeCell Cell) Voronoi(LatticePoint point, VoronoiCells cells, uint field) {
        Span<Vector4> reach = stackalloc Vector4[Cells.Near.Count];
        (NoiseSample<LatticePoint> first, NoiseSample<LatticePoint> _, LatticeCell cell) = Ranked(point.Masked(Cells.Live), cells, field, reach);
        return (first.Value, Edge(reach) / (0.5f + (0.5f * cells.Randomness)), cell);
    }

    public NoiseSample<LatticePoint> F1(LatticePoint point, Octaves octaves, VoronoiCells cells, float smoothness, uint field) =>
        smoothness is 0f
            ? Fractal(point, octaves, cells, 0f, 1f, field, static (lattice, at, voronoi, _, draw) => lattice.Ranked(at, voronoi, draw).First)
            : Fractal(point, octaves, cells, smoothness / 2f, 1f, field, static (lattice, at, voronoi, blend, draw) => lattice.Smooth(at, voronoi, blend, draw));

    public NoiseSample<LatticePoint> F2(LatticePoint point, Octaves octaves, VoronoiCells cells, uint field) =>
        Fractal(point, octaves, cells, 0f, 2f, field, static (lattice, at, voronoi, _, draw) => lattice.Ranked(at, voronoi, draw).Second);

    public float EdgeDistance(LatticePoint point, Octaves octaves, float randomness, uint field) =>
        EdgeDistance(point.Masked(Cells.Live), octaves, randomness, field, static (at, scale) => at.Scaled(scale), static (lattice, at, jitter, draw) => lattice.Edge(at, jitter, draw));

    private float EdgeDistance<TPoint>(TPoint point, Octaves octaves, float randomness, uint field, Func<TPoint, ERational, TPoint> at, Func<NoiseDimensions, TPoint, float, uint, float> sample) {
        float limit = 0.5f + (0.5f * randomness);
        (ERational scale, ERational bound, ERational distance, float amplitude) = (ERational.One, ERational.FromSingle(limit), ERational.FromSingle(8f), 1f);
        for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
            float weight = amplitude * octaves.Weight(octave);
            (ERational held, ERational blend) = (ERational.FromSingle(1f - weight), ERational.FromSingle(weight));
            ERational next = ERational.FromSingle(sample(this, at(point, scale), randomness, field)).Divide(scale);
            ERational nearest = distance.IsNaN() || next.IsNaN() ? ERational.FromSingle(float.Min(distance.ToSingle(), next.ToSingle())) : ERational.Min(distance, next);
            (bound, distance) = (bound.Multiply(held).Add(ERational.FromSingle(limit).Divide(scale).Multiply(blend)), distance.Multiply(held).Add(nearest.Multiply(blend)));
            (amplitude, scale) = (amplitude * octaves.Roughness, scale.Multiply(ERational.FromSingle(octaves.Lacunarity)));
        }
        return distance.Divide(bound).ToSingle();
    }

    private NoiseSample<LatticePoint> Fractal(LatticePoint point, Octaves octaves, VoronoiCells cells, float smoothness, float span, uint field,
        Func<NoiseDimensions, LatticePoint, VoronoiCells, float, uint, NoiseSample<LatticePoint>> scan) =>
        Fractal(point.Masked(Cells.Live), new LatticePoint(ERational.Zero, ERational.Zero, ERational.Zero, ERational.Zero), ERational.One, octaves, cells, smoothness, span, field,
            static (at, scale) => at.Scaled(scale), static (scale, lacunarity) => scale.Multiply(ERational.FromSingle(lacunarity)),
            static (site, next, scale, weight) => site.Lerp(next.Scaled(ERational.One.Divide(scale)), weight), scan);

    private NoiseSample<TSite> Fractal<TPoint, TSite, TScale>(TPoint point, TSite site, TScale scale, Octaves octaves, VoronoiCells cells, float smoothness, float span, uint field,
        Func<TPoint, TScale, TPoint> at, Func<TScale, float, TScale> advance, Func<TSite, TSite, TScale, float, TSite> blend,
        Func<NoiseDimensions, TPoint, VoronoiCells, float, uint, NoiseSample<TSite>> scan) {
        float limit = span * cells.Length((0.5f + (0.5f * cells.Randomness)) * Cells.Live);
        (float total, float distance, float amplitude) = (0f, 0f, 1f);
        for (int octave = 0; amplitude > 0f && octaves.Weight(octave) > 0f; octave++) {
            NoiseSample<TSite> feature = scan(this, at(point, scale), cells, smoothness, field);
            float weight = amplitude * octaves.Weight(octave);
            (total, distance, site) = (total + weight, distance + (feature.Value * weight), blend(site, feature.Site, scale, weight));
            (amplitude, scale) = (amplitude * octaves.Roughness, advance(scale, octaves.Lacunarity));
        }
        return new(distance / (total * limit), site);
    }

    private (NoiseSample<LatticePoint> First, NoiseSample<LatticePoint> Second, LatticeCell Cell) Ranked(LatticePoint point, VoronoiCells cells, uint field) {
        Span<Vector4> reach = stackalloc Vector4[Cells.Near.Count];
        return Ranked(point, cells, field, reach);
    }

    private (NoiseSample<LatticePoint> First, NoiseSample<LatticePoint> Second, LatticeCell Cell) Ranked(LatticePoint point, VoronoiCells cells, uint field, Span<Vector4> reach) {
        (LatticeCell cell, Vector4 local) = point.Split();
        (NoiseSample<Vector4> first, NoiseSample<Vector4> second, Vector4 offset) = Ranked(local, cells, (Cell: cell, Lattice: this, cells.Randomness, Field: field),
            static (origin, offset) => origin.Lattice.Site(origin.Cell, offset, origin.Randomness, origin.Field), reach);
        return (new(first.Value, cell.At(first.Site)), new(second.Value, cell.At(second.Site)), cell.Offset(offset));
    }

    private (NoiseSample<Vector4> First, NoiseSample<Vector4> Second, Vector4 Offset) Ranked<T>(Vector4 local, VoronoiCells cells, T origin, Func<T, Vector4, Vector4> locate, Span<Vector4> reach) {
        (NoiseSample<Vector4> First, NoiseSample<Vector4> Second, Vector4 Offset) nearest = (new(float.MaxValue, Vector4.Zero), new(float.MaxValue, Vector4.Zero), Vector4.Zero);
        int index = 0;
        foreach (Vector4 offset in Cells.Near.AsSpan()) {
            Vector4 site = locate(origin, offset);
            reach[index] = site - local;
            float distance = cells.Length(reach[index]);
            nearest = distance < nearest.First.Value ? (new(distance, site), nearest.First, offset)
                : distance < nearest.Second.Value ? (nearest.First, new(distance, site), nearest.Offset)
                : nearest;
            index++;
        }
        return nearest;
    }

    private NoiseSample<LatticePoint> Smooth(LatticePoint point, VoronoiCells cells, float smoothness, uint field) {
        (LatticeCell cell, Vector4 local) = point.Split();
        ReadOnlySpan<Vector4> wide = Cells.Wide.AsSpan();
        (float distance, Vector4 site) = (0f, Vector4.Zero);
        for (int index = 0; index < wide.Length; index++) {
            Vector4 next = Site(cell, wide[index], cells.Randomness, field);
            float reach = cells.Length(next - local);
            float blend = index is 0 ? 1f : (float)ShapeEdge.Smooth((double)float.Clamp(0.5f + (0.5f * (distance - reach) / smoothness), 0f, 1f));
            float correction = smoothness * blend * (1f - blend);
            (distance, site) = (float.Lerp(distance, reach, blend) - correction, Vector4.Lerp(site, next, blend) - (correction / (1f + (3f * smoothness)) * Cells.Live));
        }
        return new(distance, cell.At(site));
    }

    private float Edge(LatticePoint point, float randomness, uint field) {
        (LatticeCell cell, Vector4 local) = point.Split();
        ReadOnlySpan<Vector4> near = Cells.Near.AsSpan();
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

    private Vector4 Site(LatticeCell cell, Vector4 offset, float randomness, uint field) =>
        offset + (cell.Offset(offset).White(field) * Cells.Live * randomness);

    // --- [GABOR]
    public Vector2 Gabor(LatticePoint point, float frequency, float anisotropy, float orientation, uint field) {
        GaborKernel kernel = Kernel;
        (LatticeCell cell, Vector4 local) = point.Masked(kernel.Cells.Live).Split();
        Vector2 phasor = Vector2.Zero;
        for (uint impulse = 0u; impulse < GaborKernel.Impulses; impulse++) {
            (uint centers, uint turns) = (CoordinateHash.Branch(field, 2u * impulse), CoordinateHash.Branch(field, (2u * impulse) + 1u));
            foreach (Vector4 neighbor in kernel.Cells.Near.AsSpan()) {
                Vector4 center = cell.Offset(neighbor).White(centers);
                Vector4 reach = local - neighbor - (center * kernel.Cells.Live);
                float spread = reach.LengthSquared();
                (float sin, float cos) = spread < 1f
                    ? MathF.SinCos(float.Tau * frequency * Vector4.Dot(reach, kernel.Orient(orientation, 1f - anisotropy, cell.Offset(neighbor).White(turns))))
                    : (0f, 0f);
                phasor += float.CopySign(MathF.Exp(-float.Pi * spread) * (0.5f + (0.5f * MathF.Cos(float.Pi * spread))), center.W - 0.5f) * new Vector2(cos, sin);
            }
        }
        return phasor / kernel.Normalization;
    }

    // --- [PATTERNS]
    public float Wave(LatticePoint point, WaveForm form, WaveProfile profile, float phase, float distortion, Octaves octaves, float detailScale, uint field) {
        ERational detail = ERational.FromSingle(float.Pi / 10f).Multiply(ERational.FromSingle(detailScale));
        float bend = distortion is 0f ? 0f : distortion * Fbm(
            new LatticePoint(point.X.Multiply(detail), point.Y.Multiply(detail), ERational.Zero, ERational.Zero), octaves, CoordinateHash.Branch(field, 0u));
        ERational periods = form.Periods(point).Subtract(point.Z).Add(ERational.FromSingle((phase + bend) / float.Tau));
        return profile.Level(LatticePoint.Split(periods).Local.ToSingle());
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

    public static (uint X, uint Y, uint Z) Hash(Vector4 cell, uint field) =>
        unchecked(Pcg3d((uint)(int)cell.X, (uint)(int)cell.Y, Pcg3d((uint)(int)cell.Z, (uint)(int)cell.W, field).X));

    public static Vector4 White(Vector4 point, uint field) => Uniforms(Hash(Vector4.Round(point, MidpointRounding.ToNegativeInfinity), field));

    public static Vector4 Uniforms((uint X, uint Y, uint Z) words) =>
        words switch {
            var (x, y, z) => (new Vector4(x >> 9, y >> 9, z >> 9, Pcg3d(x, y, z).X >> 9) + Vector4.Create(0.5f)) * (1f / (1 << 23)),
        };

    public static Vector4 Uniforms(long column, long line, uint field) =>
        Uniforms(unchecked(Pcg3d((uint)column, (uint)line, Pcg3d((uint)(column >> 32), (uint)(line >> 32), field).X)));

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
