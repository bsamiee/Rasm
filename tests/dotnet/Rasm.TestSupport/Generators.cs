using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using System.Text;

namespace Rasm.TestSupport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Generators {
    // --- [SCALARS]
    public static readonly Gen<double> Finite = Gen.Double.Special.Where(double.IsFinite);
    public static readonly Gen<double> NonFinite = Gen.OneOf(double.NaN, double.PositiveInfinity, double.NegativeInfinity);
    public static readonly Gen<double> Positive = Finite.Where(static value => value > 0d);
    public static readonly Gen<double> ModerateMagnitude = Gen.Double[-1.0e6, 1.0e6];
    public static readonly Gen<double> UnitInterval = Gen.OneOf(Gen.Double[0d, 1d], Finite.Where(static value => value is >= 0d and <= 1d));
    public static readonly Gen<(double X, double Y)> CancellationPair = Finite.Where(static value => double.Abs(value) <= double.MaxValue / 2d)
        .Select(Gen.Double[Precision.DoublePrecision, Math.Sqrt(Precision.DoublePrecision)], static (value, relativeDifference) => (X: value, Y: value * (1d + relativeDifference)));
    public static readonly Gen<double> Angle = Gen.OneOf(Gen.Double[-Math.Tau, Math.Tau],
        Gen.Int[-4, 4].Select(Gen.Int[-1, 1], static (quarter, step) => UlpNudge(quarter * Math.PI / 2d, step)));
    public static readonly Gen<int> IntBoundaryValues = Gen.OneOf(Gen.Int,
        Gen.Int[0, (sizeof(int) * 8) - 1].Select(Gen.Int[-1, 1], static (bit, step) => unchecked((1 << bit) + step)));

    // --- [COLLECTIONS]
    public static Gen<T[]> SortedArray<T>(Gen<T> element) where T : IComparable<T> => element.Array[0, 32].Select(static a => a.Order().ToArray());
    public static Gen<(T Lo, T Hi)> OrderedPair<T>(Gen<T> element) where T : IComparable<T> =>
        element.Select(element, static (a, b) => a.CompareTo(b) <= 0 ? (Lo: a, Hi: b) : (Lo: b, Hi: a));
    public static Gen<(T A, T B, T C)> DistinctTriple<T>(Gen<T> element) where T : notnull =>
        element.ArrayUnique[3, 3].Select(static values => (A: values[0], B: values[1], C: values[2]));
    public static Gen<Vector<double>> Simplex(int count) {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        return Gen.Double[1.0e-6, 1.0e6].Array[count].Select(static values => Vector<double>.Build.DenseOfArray(values).Normalize(1d));
    }

    // --- [GEOMETRY]
    public static Gen<Vector<double>> UnitVector(int dimension) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        return Gen.Double[-1d, 1d].Array[dimension]
            .Where(static raw => raw.Any(static value => value is not 0d))
            .Select(static raw => Vector<double>.Build.DenseOfArray(raw).Normalize(2d));
    }
    public static Gen<Matrix<double>> RadialPolygon(int vertices) {
        ArgumentOutOfRangeException.ThrowIfLessThan(vertices, 3);
        return Gen.Double[1.0e-3, 1.0e3].Array[vertices].Select(Gen.Double[-Math.Tau, Math.Tau],
            static (radii, phase) => Matrix<double>.Build.DenseOfRowArrays(radii.Select((radius, i) => {
                double theta = phase + (i * Math.Tau / radii.Length);
                return (double[])[radius * Math.Cos(theta), radius * Math.Sin(theta)];
            })));
    }
    public static Gen<(Vector<double> A, Vector<double> B, Vector<double> C)> NearlyCollinearPoints(int dimension) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 2);
        Gen<Vector<double>> point = ModerateMagnitude.Array[dimension].Select(Vector<double>.Build.DenseOfArray);
        return point.Select(point, Gen.Double[-2d, 2d], static (a, b, t) => (A: a, B: b, C: a + (t * (b - a))))
            .Select(Gen.Int[0, dimension - 1], Gen.Int[-8, 8],
                static (p, axis, steps) => (p.A, p.B, C: p.C.MapIndexed((i, value) => i == axis ? UlpNudge(value, steps) : value)));
    }
    public static Gen<Matrix<double>> OrthogonalMatrix(int dimension) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        return Gen.Double[-1d, 1d].Array2D[dimension, dimension].Select(Gen.Bool, static (entries, flip) => {
            Matrix<double> q = Matrix<double>.Build.DenseOfArray(entries).QR().Q;
            if (flip) q.SetRow(0, -q.Row(0));
            return q;
        });
    }
    public static Gen<Matrix<double>> SymmetricPositiveDefiniteMatrix(int dimension, double conditionNumber) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(conditionNumber, 1.0);
        ArgumentOutOfRangeException.ThrowIfEqual(conditionNumber, double.PositiveInfinity);
        return OrthogonalMatrix(dimension).Select(orthogonal => {
            double[] spectrum = [.. Enumerable.Range(0, dimension).Select(index => Math.Pow(conditionNumber, dimension is 1 ? 0.0 : -(double)index / (dimension - 1)))];
            return (orthogonal * Matrix<double>.Build.DiagonalOfDiagonalArray(spectrum)).TransposeAndMultiply(orthogonal);
        });
    }
    private static double UlpNudge(double value, int steps) =>
        Enumerable.Range(0, Math.Abs(steps)).Aggregate(value, (acc, _) => steps > 0 ? Math.BitIncrement(acc) : Math.BitDecrement(acc));

    // --- [SERIALIZATION]
    public static readonly Gen<string> UnicodeString = Gen.OneOf(Gen.String,
        Gen.Int[0, 0x10FFFF].Where(Rune.IsValid).Array.Select(static scalars => string.Concat(scalars.Select(static scalar => new Rune(scalar).ToString()))));
    public static readonly Gen<byte[]> Bytes = Gen.OneOf(Gen.Byte.Array,
        Gen.Byte.Array[0, 4096], Gen.Byte.Select(Gen.Int[0, 4096], static (value, count) => (byte[])[.. Enumerable.Repeat(value, count)]));
    public static readonly Gen<(byte[] Original, byte[] Corrupted)> CorruptedBytes =
        from bytes in Gen.Byte.Array[1, 256]
        from index in Gen.Int[0, bytes.Length - 1]
        from mask in Gen.Int[1, 255]
        select (Original: bytes, Corrupted: (byte[])[.. bytes.Select((value, position) => position == index ? (byte)(value ^ mask) : value)]);

    // --- [RESULT]
    public static readonly Gen<Error> Errors = Gen.String.Select(static message => Error.New(message));
    public static readonly Gen<Error> ErrorsFromExceptions = Gen.OneOf<Exception>(
            new InvalidOperationException(), new ArithmeticException(), new IOException(), new FormatException(), new OperationCanceledException(), new TimeoutException())
        .Select(static exception => Error.New(exception));
    public static Gen<Fin<T>> FinOf<T>(Gen<T> success, Option<Gen<Error>> error = default) =>
        Gen.OneOf(success.Select(static Fin<T> (value) => value), error.IfNone(Errors).Select(static Fin<T> (generatedError) => generatedError));
    public static Gen<Option<T>> OptionOf<T>(Gen<T> some) =>
        Gen.OneOf(some.Select(static Option<T> (value) => value), Gen.Const(Option<T>.None));
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
