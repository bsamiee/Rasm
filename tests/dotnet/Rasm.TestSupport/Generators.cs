namespace Rasm.TestSupport;

// --- [ERRORS] --------------------------------------------------------------------------
[Union]
public abstract partial record GeneratedError : Expected {
    private GeneratedError(string detail, int code) : base(detail, code, None) { }
    public sealed record Missing : GeneratedError { public Missing() : base("resource missing", 1) { } }
    public sealed record Rejected : GeneratedError { public Rejected() : base("request rejected", 2) { } }
    public sealed record Cancelled : GeneratedError { public Cancelled() : base("operation cancelled", 3) { } }
    public sealed record Conflict : GeneratedError { public Conflict() : base("state conflict", 4) { } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Generators {
    // --- [SCALARS] ---------------------------------------------------------------------
    private static readonly Gen<double> MagnitudeDistribution = Gen.Frequency(
        (30, Gen.Double[1.0e-3, 1.0e3]),
        (18, Gen.Double[1.0e3, 1.0e15]),
        (14, Gen.Double[1.0e-30, 1.0e-3]),
        (14, Gen.Double[1.0e15, 1.0e300]),
        (12, Gen.Double[double.Epsilon, 2.0e-308]),
        (12, Gen.OneOfConst(0.0, -0.0, 1.0, Math.BitIncrement(1.0), Math.BitDecrement(1.0), Math.PI, double.Epsilon, double.MaxValue / 4.0)));
    public static readonly Gen<double> Finite = MagnitudeDistribution.Select(Gen.Bool, static (magnitude, negative) => negative ? -magnitude : magnitude);
    public static readonly Gen<double> NonFinite = Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity);
    public static readonly Gen<double> AnyDouble = Gen.Frequency((95, Finite), (5, NonFinite));
    public static readonly Gen<double> Positive = MagnitudeDistribution.Where(static x => x > 0.0);
    public static readonly Gen<double> ModerateMagnitude = Gen.Double[-1.0e6, 1.0e6];
    public static readonly Gen<double> UnitInterval = Gen.Frequency(
        (92, Gen.Double.Unit),
        (8, Gen.OneOfConst(0.0, 1.0, Math.BitIncrement(0.0), Math.BitDecrement(1.0))));
    public static readonly Gen<(double X, double Y)> CancellationPair = Finite.Select(Gen.Double[1.0e-16, 1.0e-8], static (value, relativeDifference) => (X: value, Y: value * (1.0 + relativeDifference)));
    public static readonly Gen<double> Angle = Gen.Frequency(
        (70, Gen.Double[-Math.Tau, Math.Tau]),
        (30, Gen.OneOfConst(0.0, -0.0, Math.PI, -Math.PI, Math.Tau, -Math.Tau, Math.PI / 2.0, -Math.PI / 2.0,
            Math.BitDecrement(Math.Tau), Math.BitIncrement(-Math.Tau), Math.BitIncrement(0.0), Math.BitDecrement(Math.PI))));
    public static readonly Gen<int> IntBoundaryValues = Gen.Frequency(
        (70, Gen.Int[-1_000_000, 1_000_000]),
        (30, Gen.OneOfConst(int.MinValue, int.MinValue + 1, -1, 0, 1, (1 << 30) - 1, 1 << 30, int.MaxValue - 1, int.MaxValue)));

    // --- [COLLECTIONS] -----------------------------------------------------------------
    public static Gen<T[]> SmallArray<T>(Gen<T> element) {
        ArgumentNullException.ThrowIfNull(element);
        return element.Array[0, 32];
    }
    public static Gen<T[]> NonEmptyArray<T>(Gen<T> element, int max = 256) {
        ArgumentNullException.ThrowIfNull(element);
        return element.Array[1, max];
    }
    public static Gen<T[]> LargeArray<T>(Gen<T> element) {
        ArgumentNullException.ThrowIfNull(element);
        return element.Array[1_000, 10_000];
    }
    public static Gen<T[]> UniqueArray<T>(Gen<T> element) {
        ArgumentNullException.ThrowIfNull(element);
        return element.ArrayUnique[1, 64];
    }
    public static Gen<T[]> SortedArray<T>(Gen<T> element) where T : IComparable<T> => SmallArray(element).Select(static a => a.Order().ToArray());
    public static Gen<(T Lo, T Hi)> OrderedPair<T>(Gen<T> element) where T : IComparable<T> =>
        element.Select(element, static (a, b) => a.CompareTo(b) <= 0 ? (Lo: a, Hi: b) : (Lo: b, Hi: a));
    public static Gen<(T A, T B, T C)> DistinctTriple<T>(Gen<T> element) where T : notnull {
        ArgumentNullException.ThrowIfNull(element);
        return element.ArrayUnique[3, 3].Select(static values => (A: values[0], B: values[1], C: values[2]));
    }
    public static Gen<Seq<T>> NonEmptySeq<T>(Gen<T> element, int max = 256) => NonEmptyArray(element, max).Select(static xs => toSeq(xs));
    public static Gen<Seq<T>> SeqOf<T>(Gen<T> element, int max = 256) {
        ArgumentNullException.ThrowIfNull(element);
        return element.Array[0, max].Select(static xs => toSeq(xs));
    }
    public static Gen<Seq<double>> Simplex(int count) {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        return Gen.Double[1.0e-6, 1.0e6].Array[count].Select(static values => {
            double total = values.Sum();
            return toSeq(values.Select(value => value / total));
        });
    }

    // --- [GEOMETRY] --------------------------------------------------------------------
    public static Gen<double[]> UnitVector(int dimension) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        return Gen.Double[-1.0, 1.0].Array[dimension]
            .Where(static raw => raw.Sum(static x => x * x) >= 1.0e-6)
            .Select(static raw => {
                double norm = Math.Sqrt(raw.Sum(static x => x * x));
                return (double[])[.. raw.Select(x => x / norm)];
            });
    }
    // Vertices sit at increasing angles, the polygon is star-shaped about the origin and counterclockwise
    public static Gen<double[][]> RadialPolygon(int vertices) {
        ArgumentOutOfRangeException.ThrowIfLessThan(vertices, 3);
        return Gen.Double[1.0e-3, 1.0e3].Array[vertices].Select(Gen.Double[-Math.Tau, Math.Tau],
            static (radii, phase) => (double[][])[.. radii.Select((radius, i) => {
                double theta = phase + (i * Math.Tau / radii.Length);
                return (double[])[radius * Math.Cos(theta), radius * Math.Sin(theta)];
            })]);
    }
    public static Gen<(double[] A, double[] B, double[] C)> NearlyCollinearPoints(int dimension) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 2);
        return ModerateMagnitude.Array[dimension].Select(ModerateMagnitude.Array[dimension], Gen.Double[-2.0, 2.0],
                static (a, b, t) => (A: a, B: b, C: (double[])[.. a.Select((x, i) => x + (t * (b[i] - x)))]))
            .Select(Gen.Int[0, dimension - 1], Gen.Int[-8, 8],
                static (p, axis, steps) => (p.A, p.B, C: (double[])[.. p.C.Select((x, i) => i == axis ? UlpNudge(x, steps) : x)]));
    }
    // A product of Householder reflections, the flip of one row covers both determinant signs
    public static Gen<double[][]> OrthogonalMatrix(int dimension) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        return UnitVector(dimension).Array[dimension].Select(Gen.Bool, static (reflectors, flip) => {
            double[][] q = reflectors.Aggregate(seed: IdentityMatrix(reflectors.Length), func: Reflect);
            return flip ? [.. q.Select(static (row, i) => i == 0 ? [.. row.Select(static x => -x)] : row)] : q;
        });
    }
    // Eigenvalues descend geometrically from 1 to the reciprocal of the condition number, and a 1x1 matrix has condition number 1 whatever the argument
    public static Gen<double[][]> SymmetricPositiveDefiniteMatrix(int dimension, double conditionNumber) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(conditionNumber, 1.0);
        _ = double.IsFinite(conditionNumber) ? conditionNumber : throw new ArgumentOutOfRangeException(nameof(conditionNumber), conditionNumber, "condition number must be finite");
        return OrthogonalMatrix(dimension).Select(orthogonal => {
            double[] spectrum = [.. Enumerable.Range(0, dimension).Select(index => Math.Pow(conditionNumber, dimension == 1 ? 0.0 : -(double)index / (dimension - 1)))];
            return (double[][])[.. Enumerable.Range(0, dimension).Select(row =>
                (double[])[.. Enumerable.Range(0, dimension).Select(column => Enumerable.Range(0, dimension).Sum(index => orthogonal[row][index] * spectrum[index] * orthogonal[column][index]))])];
        });
    }

    // --- [SERIALIZATION] ---------------------------------------------------------------
    public static readonly Gen<string> UnicodeString = Gen.Frequency(
        (40, Gen.Char[' ', '~'].Array[0, 24].Select(static chars => new string(chars))),
        (20, Gen.Char['\u0080', '\uD7FF'].Array[1, 12].Select(static chars => new string(chars))),
        (14, Gen.Int[0x10000, 0x10FFFF].Array[1, 4].Select(static codes => string.Concat(codes.Select(char.ConvertFromUtf32)))),
        (14, Gen.OneOfConst("", "\"", "\\", "\r\n", "\t", "{", "}", "\u0000", "\u001B", "\uFFFD", "\U0001D518\U0001D52B\U0001D526")),
        (12, Gen.Char['\u0000', '\u001F'].Array[1, 4].Select(static chars => new string(chars))));
    public static readonly Gen<byte[]> Bytes = Gen.Frequency(
        (40, Gen.Byte.Array[1, 64]),
        (25, Gen.Byte.Array[65, 4096]),
        (13, Gen.Const<byte[]>([])),
        (12, Gen.Byte.Array[1, 1]),
        (10, Gen.Byte.Select(Gen.Int[16, 256], static (value, count) => (byte[])[.. Enumerable.Repeat(value, count)])));
    public static readonly Gen<(byte[] Original, byte[] Corrupted)> CorruptedBytes =
        Gen.Byte.Array[1, 256].SelectMany(static bytes => Gen.Int[0, bytes.Length - 1].Select(Gen.Int[1, 255], (index, mask) => {
            byte[] copy = [.. bytes];
            copy[index] ^= (byte)mask;
            return (Original: bytes, Corrupted: copy);
        }));

    // --- [RESULT] ----------------------------------------------------------------------
    public static readonly Gen<Error> Errors = Gen.OneOfConst<Error>(new GeneratedError.Missing(), new GeneratedError.Rejected(), new GeneratedError.Cancelled(), new GeneratedError.Conflict());
    public static readonly Gen<Error> ErrorsFromExceptions = Gen.Int[0, 3].Select(static kind => Error.New(kind switch {
        0 => new InvalidOperationException("generated invalid operation"),
        1 => new ArithmeticException("generated arithmetic failure"),
        2 => new IOException("generated I/O failure"),
        _ => new FormatException("generated format failure"),
    }));
    public static Gen<Fin<T>> FinOf<T>(Gen<T> success, Gen<Error>? error = null, int successWeight = 80) =>
        Gen.Frequency(
            (successWeight, success.Select(static Fin<T> (value) => value)),
            (100 - successWeight, (error ?? Errors).Select(static Fin<T> (generatedError) => generatedError)));
    public static Gen<Option<T>> OptionOf<T>(Gen<T> some, int someWeight = 80) =>
        Gen.Frequency(
            (someWeight, some.Select(static Option<T> (v) => v)),
            (100 - someWeight, Gen.Const(Option<T>.None)));
    public static Gen<Validation<Error, T>> ValidationOf<T>(Gen<T> success, Gen<Error>? error = null) =>
        Gen.OneOf(
            success.Select(static Validation<Error, T> (value) => value),
            (error ?? Errors).Select(static Validation<Error, T> (generatedError) => generatedError));

    // --- [KEYS] ------------------------------------------------------------------------
    public static readonly Gen<string> Key = Gen.Char['a', 'z'].Array[1, 32].Select(static chars => new string(chars));
    public static readonly Gen<Guid> Id = Gen.Guid;

    // --- [CONSTRUCTION] ----------------------------------------------------------------
    public static Gen<TVo> ConstructedValues<TIn, TVo>(Gen<TIn> source, TryCreate<TIn, TVo> tryCreate) {
        ArgumentNullException.ThrowIfNull(tryCreate);
        return source
            .Select(v => tryCreate(v, out TVo owned) ? Some(owned) : Option<TVo>.None)
            .Where(static o => o.IsSome)
            .Select(static o => (TVo)o);
    }

    private static double[][] IdentityMatrix(int n) =>
        [.. Enumerable.Range(0, n).Select(i => (double[])[.. Enumerable.Range(0, n).Select(j => i == j ? 1.0 : 0.0)])];
    private static double[][] Reflect(double[][] m, double[] v) {
        double[] w = [.. Enumerable.Range(0, v.Length).Select(j => Enumerable.Range(0, v.Length).Sum(k => v[k] * m[k][j]))];
        return [.. Enumerable.Range(0, v.Length).Select(i => (double[])[.. Enumerable.Range(0, v.Length).Select(j => m[i][j] - (2.0 * v[i] * w[j]))])];
    }
    private static double UlpNudge(double value, int steps) =>
        Enumerable.Range(0, Math.Abs(steps)).Aggregate(value, (acc, _) => steps > 0 ? Math.BitIncrement(acc) : Math.BitDecrement(acc));
}
