namespace Rasm.TestSupport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Generators {
    // --- [SCALARS]
    private static readonly Gen<double> MagnitudeDistribution = Gen.Frequency(
        (30, Gen.Double[1.0e-3, 1.0e3]),
        (18, Gen.Double[1.0e3, 1.0e15]),
        (14, Gen.Double[1.0e-30, 1.0e-3]),
        (14, Gen.Double[1.0e15, 1.0e300]),
        (12, Gen.Double[double.Epsilon, 2.0e-308]),
        (12, Gen.OneOf(0.0, -0.0, 1.0, Math.BitIncrement(1.0), Math.BitDecrement(1.0), Math.PI, double.Epsilon, double.MaxValue / 4.0)));
    public static readonly Gen<double> Finite = MagnitudeDistribution.Select(Gen.Bool, static (magnitude, negative) => negative ? -magnitude : magnitude);
    public static readonly Gen<double> NonFinite = Gen.OneOf(double.NaN, double.PositiveInfinity, double.NegativeInfinity);
    public static readonly Gen<double> AnyDouble = Gen.Frequency((95, Finite), (5, NonFinite));
    public static readonly Gen<double> Positive = MagnitudeDistribution.Where(static x => x > 0.0);
    public static readonly Gen<double> ModerateMagnitude = Gen.Double[-1.0e6, 1.0e6];
    public static readonly Gen<double> UnitInterval = Gen.Frequency(
        (92, Gen.Double.Unit),
        (8, Gen.OneOf(0.0, 1.0, Math.BitIncrement(0.0), Math.BitDecrement(1.0))));
    public static readonly Gen<(double X, double Y)> CancellationPair = Finite.Select(Gen.Double[1.0e-16, 1.0e-8], static (value, relativeDifference) => (X: value, Y: value * (1.0 + relativeDifference)));
    public static readonly Gen<double> Angle = Gen.Frequency(
        (70, Gen.Double[-Math.Tau, Math.Tau]),
        (30, Gen.OneOf(0.0, -0.0, Math.PI, -Math.PI, Math.Tau, -Math.Tau, Math.PI / 2.0, -Math.PI / 2.0,
            Math.BitDecrement(Math.Tau), Math.BitIncrement(-Math.Tau), Math.BitIncrement(0.0), Math.BitDecrement(Math.PI))));
    public static readonly Gen<int> IntBoundaryValues = Gen.Frequency(
        (70, Gen.Int[-1_000_000, 1_000_000]),
        (30, Gen.OneOf(int.MinValue, int.MinValue + 1, -1, 0, 1, (1 << 30) - 1, 1 << 30, int.MaxValue - 1, int.MaxValue)));

    // --- [COLLECTIONS]
    public static Gen<T[]> SortedArray<T>(Gen<T> element) where T : IComparable<T> => element.Array[0, 32].Select(static a => a.Order().ToArray());
    public static Gen<(T Lo, T Hi)> OrderedPair<T>(Gen<T> element) where T : IComparable<T> =>
        element.Select(element, static (a, b) => a.CompareTo(b) <= 0 ? (Lo: a, Hi: b) : (Lo: b, Hi: a));
    public static Gen<(T A, T B, T C)> DistinctTriple<T>(Gen<T> element) where T : notnull =>
        element.ArrayUnique[3, 3].Select(static values => (A: values[0], B: values[1], C: values[2]));
    public static Gen<Seq<double>> Simplex(int count) {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        return Gen.Double[1.0e-6, 1.0e6].Array[count].Select(static values => {
            double total = values.Sum();
            return toSeq(values.Select(value => value / total));
        });
    }

    // --- [GEOMETRY]
    public static Gen<double[]> UnitVector(int dimension) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        return Gen.Double[-1.0, 1.0].Array[dimension]
            .Where(static raw => raw.Sum(static x => x * x) >= 1.0e-6)
            .Select(static raw => {
                double norm = Math.Sqrt(raw.Sum(static x => x * x));
                return (double[])[.. raw.Select(x => x / norm)];
            });
    }
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
    public static Gen<double[][]> OrthogonalMatrix(int dimension) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        return UnitVector(dimension).Array[dimension].Select(Gen.Bool, static (reflectors, flip) => {
            double[][] q = reflectors.Aggregate(seed: IdentityMatrix(reflectors.Length), func: Reflect);
            return flip ? [.. q.Select(static (row, i) => i is 0 ? [.. row.Select(static x => -x)] : row)] : q;
        });
    }
    public static Gen<double[][]> SymmetricPositiveDefiniteMatrix(int dimension, double conditionNumber) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(conditionNumber, 1.0);
        ArgumentOutOfRangeException.ThrowIfEqual(conditionNumber, double.PositiveInfinity);
        return OrthogonalMatrix(dimension).Select(orthogonal => {
            double[] spectrum = [.. Enumerable.Range(0, dimension).Select(index => Math.Pow(conditionNumber, dimension is 1 ? 0.0 : -(double)index / (dimension - 1)))];
            return (double[][])[.. (from row in Enumerable.Range(0, dimension)
                                    from column in Enumerable.Range(0, dimension)
                                    select Enumerable.Range(0, dimension).Sum(index => orthogonal[row][index] * spectrum[index] * orthogonal[column][index]))
                .Chunk(dimension)];
        });
    }
    private static double[][] IdentityMatrix(int n) =>
        [.. Enumerable.Range(0, n).Select(i => (double[])[.. Enumerable.Range(0, n).Select(j => i == j ? 1.0 : 0.0)])];
    private static double[][] Reflect(double[][] m, double[] v) {
        double[] w = [.. Enumerable.Range(0, v.Length).Select(j => Enumerable.Range(0, v.Length).Sum(k => v[k] * m[k][j]))];
        return [.. Enumerable.Range(0, v.Length).Select(i => (double[])[.. Enumerable.Range(0, v.Length).Select(j => m[i][j] - (2.0 * v[i] * w[j]))])];
    }
    private static double UlpNudge(double value, int steps) =>
        Enumerable.Range(0, Math.Abs(steps)).Aggregate(value, (acc, _) => steps > 0 ? Math.BitIncrement(acc) : Math.BitDecrement(acc));

    // --- [SERIALIZATION]
    public static readonly Gen<string> UnicodeString = Gen.Frequency(
        (40, Gen.String[Gen.Char[' ', '~'], 0, 24]),
        (20, Gen.String[Gen.Char['\u0080', '\uD7FF'], 1, 12]),
        (14, Gen.Int[0x10000, 0x10FFFF].Array[1, 4].Select(static codes => string.Concat(codes.Select(char.ConvertFromUtf32)))),
        (14, Gen.OneOf("", "\"", "\\", "\r\n", "\t", "{", "}", "\u0000", "\e", "\uFFFD", "\U0001D518\U0001D52B\U0001D526")),
        (12, Gen.String[Gen.Char['\u0000', '\u001F'], 1, 4]));
    public static readonly Gen<byte[]> Bytes = Gen.Frequency(
        (40, Gen.Byte.Array[1, 64]),
        (25, Gen.Byte.Array[65, 4096]),
        (13, Gen.Const<byte[]>([])),
        (12, Gen.Byte.Array[1, 1]),
        (10, Gen.Byte.Select(Gen.Int[16, 256], static (value, count) => (byte[])[.. Enumerable.Repeat(value, count)])));
    public static readonly Gen<(byte[] Original, byte[] Corrupted)> CorruptedBytes =
        from bytes in Gen.Byte.Array[1, 256]
        from index in Gen.Int[0, bytes.Length - 1]
        from mask in Gen.Int[1, 255]
        select (Original: bytes, Corrupted: (byte[])[.. bytes.Select((value, position) => position == index ? (byte)(value ^ mask) : value)]);

    // --- [RESULT]
    private const int Weights = 100;
    public static readonly Gen<Error> Errors = Gen.OneOf(Error.New("resource missing"), Error.New("request rejected"), Error.New("operation canceled"), Error.New("state conflict"));
    public static readonly Gen<Error> ErrorsFromExceptions = Gen.OneOf<Exception>(
            new InvalidOperationException("generated invalid operation"),
            new ArithmeticException("generated arithmetic failure"),
            new IOException("generated I/O failure"),
            new FormatException("generated format failure"))
        .Select(static exception => Error.New(exception));
    public static Gen<Fin<T>> FinOf<T>(Gen<T> success, Option<Gen<Error>> error = default, int successWeight = 80) =>
        Gen.Frequency(
            (successWeight, success.Select(static Fin<T> (value) => value)),
            (Weights - successWeight, error.IfNone(Errors).Select(static Fin<T> (generatedError) => generatedError)));
    public static Gen<Option<T>> OptionOf<T>(Gen<T> some, int someWeight = 80) =>
        Gen.Frequency(
            (someWeight, some.Select(static Option<T> (v) => v)),
            (Weights - someWeight, Gen.Const(Option<T>.None)));
    public static Gen<Validation<Error, T>> ValidationOf<T>(Gen<T> success, Option<Gen<Error>> error = default) =>
        Gen.OneOf(
            success.Select(static Validation<Error, T> (value) => value),
            error.IfNone(Errors).Select(static Validation<Error, T> (generatedError) => generatedError));

    // --- [KEYS]
    public static readonly Gen<string> Key = Gen.String[Gen.Char['a', 'z'], 1, 32];

    // --- [CONSTRUCTION]
    public static Gen<TValueObject> ConstructedValues<TIn, TValueObject>(Gen<TIn> source, Func<TIn, Fin<TValueObject>> create) =>
        source.Select(create).Where(static result => result.IsSucc).Select(static result => (TValueObject)result);
}
