namespace Rasm.TestKit;

// --- [ERRORS] --------------------------------------------------------------------------
[Union]
public abstract partial record Fault : Expected {
    private Fault(string detail, int code) : base(detail, code, None) { }
    public sealed record Missing : Fault { public Missing() : base("<fault-missing>", 9001) { } }
    public sealed record Rejected : Fault { public Rejected() : base("<fault-rejected>", 9002) { } }
    public sealed record Cancelled : Fault { public Cancelled() : base("<fault-cancelled>", 9003) { } }
    public sealed record Conflict : Fault { public Conflict() : base("<fault-conflict>", 9004) { } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Gens {
    // --- [SCALARS]
    private static readonly Gen<double> MagnitudeBand = Gen.Frequency(
        (30, Gen.Double[1.0e-3, 1.0e3]),
        (18, Gen.Double[1.0e3, 1.0e15]),
        (14, Gen.Double[1.0e-30, 1.0e-3]),
        (14, Gen.Double[1.0e15, 1.0e300]),
        (12, Gen.Double[double.Epsilon, 2.0e-308]),
        (12, Gen.OneOfConst(0.0, -0.0, 1.0, Math.BitIncrement(1.0), Math.BitDecrement(1.0), Math.PI, double.Epsilon, double.MaxValue / 4.0)));
    public static readonly Gen<double> Finite = MagnitudeBand.Select(Gen.Bool, static (magnitude, negative) => negative ? -magnitude : magnitude);
    public static readonly Gen<double> NonFinite = Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity);
    public static readonly Gen<double> AnyDouble = Gen.Frequency((95, Finite), (5, NonFinite));
    public static readonly Gen<double> Positive = MagnitudeBand.Where(static x => x > 0.0);
    public static readonly Gen<double> Tame = Gen.Double[-1.0e6, 1.0e6];
    public static readonly Gen<double> UnitClosed = Gen.Frequency(
        (92, Gen.Double.Unit),
        (8, Gen.OneOfConst(0.0, 1.0, Math.BitIncrement(0.0), Math.BitDecrement(1.0))));
    public static readonly Gen<(double X, double Y)> Cancellation = Finite.Select(Gen.Double[1.0e-16, 1.0e-8], static (x, eps) => (X: x, Y: x * (1.0 + eps)));
    public static readonly Gen<double> Angle = Gen.Frequency(
        (70, Gen.Double[-Math.Tau, Math.Tau]),
        (30, Gen.OneOfConst(0.0, -0.0, Math.PI, -Math.PI, Math.Tau, -Math.Tau, Math.PI / 2.0, -Math.PI / 2.0,
            Math.BitDecrement(Math.Tau), Math.BitIncrement(-Math.Tau), Math.BitIncrement(0.0), Math.BitDecrement(Math.PI))));
    public static readonly Gen<int> IntEdges = Gen.Frequency(
        (70, Gen.Int[-1_000_000, 1_000_000]),
        (30, Gen.OneOfConst(int.MinValue, int.MinValue + 1, -1, 0, 1, (1 << 30) - 1, 1 << 30, int.MaxValue - 1, int.MaxValue)));

    // --- [COLLECTIONS]
    public static Gen<T[]> SmallArray<T>(Gen<T> element) => Element(element).Array[0, 32];
    public static Gen<T[]> NonEmptyArray<T>(Gen<T> element, int max = 256) => Element(element).Array[1, max];
    public static Gen<T[]> LargeArray<T>(Gen<T> element) => Element(element).Array[1_000, 10_000];
    public static Gen<T[]> UniqueArray<T>(Gen<T> element) => Element(element).ArrayUnique[1, 64];
    public static Gen<T[]> SortedArray<T>(Gen<T> element) where T : IComparable<T> => SmallArray(element).Select(static a => a.Order().ToArray());
    public static Gen<(T Lo, T Hi)> OrderedPair<T>(Gen<T> element) where T : IComparable<T> =>
        Element(element).Select(element, static (a, b) => a.CompareTo(b) <= 0 ? (Lo: a, Hi: b) : (Lo: b, Hi: a));
    public static Gen<(T A, T B, T C)> DistinctTriple<T>(Gen<T> element) where T : notnull =>
        Element(element).ArrayUnique[3, 3].Select(static values => (A: values[0], B: values[1], C: values[2]));
    public static Gen<Seq<T>> NonEmptySeq<T>(Gen<T> element, int max = 256) => NonEmptyArray(element, max).Select(static xs => toSeq(xs));
    public static Gen<Seq<T>> SeqOf<T>(Gen<T> element, int max = 256) => Element(element).Array[0, max].Select(static xs => toSeq(xs));
    public static Gen<Seq<double>> Simplex(int count) {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        return Gen.Double[1.0e-6, 1.0e6].Array[count].Select(static values => {
            double total = values.Sum();
            return toSeq(values.Select(value => value / total));
        });
    }

    // --- [GEOMETRY]
    public static Gen<double[]> Direction(int dim) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dim, 1);
        return Gen.Double[-1.0, 1.0].Array[dim]
            .Where(static raw => raw.Sum(static x => x * x) >= 1.0e-6)
            .Select(static raw => {
                double norm = Math.Sqrt(raw.Sum(static x => x * x));
                return (double[])[.. raw.Select(x => x / norm)];
            });
    }
    public static Gen<double[][]> Ring(int vertices) {
        ArgumentOutOfRangeException.ThrowIfLessThan(vertices, 3);
        return Gen.Double[1.0e-3, 1.0e3].Array[vertices].Select(Gen.Double[-Math.Tau, Math.Tau],
            static (radii, phase) => (double[][])[.. radii.Select((radius, i) => {
                double theta = phase + (i * Math.Tau / radii.Length);
                return (double[])[radius * Math.Cos(theta), radius * Math.Sin(theta)];
            })]);
    }
    public static Gen<(double[] A, double[] B, double[] C)> NearCollinear(int dim) {
        ArgumentOutOfRangeException.ThrowIfLessThan(dim, 2);
        return Tame.Array[dim].Select(Tame.Array[dim], Gen.Double[-2.0, 2.0],
                static (a, b, t) => (A: a, B: b, C: (double[])[.. a.Select((x, i) => x + (t * (b[i] - x)))]))
            .Select(Gen.Int[0, dim - 1], Gen.Int[-8, 8], static (p, axis, steps) => {
                p.C[axis] = UlpNudge(p.C[axis], steps);
                return (p.A, p.B, p.C);
            });
    }
    public static Gen<double[][]> Orthogonal(int n) {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 1);
        return Direction(n).Array[n].Select(Gen.Bool, static (reflectors, flip) => {
            double[][] q = reflectors.Aggregate(seed: IdentityMatrix(reflectors.Length), func: Reflect);
            return flip ? [.. q.Select(static (row, i) => i == 0 ? [.. row.Select(static x => -x)] : row)] : q;
        });
    }
    public static Gen<double[][]> Conditioned(int n, double kappa) {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(kappa, 1.0);
        _ = double.IsFinite(kappa) ? kappa : throw new ArgumentOutOfRangeException(nameof(kappa), kappa, "kappa must be finite");
        return Orthogonal(n).Select(q => {
            double[] spectrum = [.. Enumerable.Range(0, n).Select(i => Math.Pow(kappa, n == 1 ? 0.0 : -(double)i / (n - 1)))];
            return (double[][])[.. Enumerable.Range(0, n).Select(i =>
                (double[])[.. Enumerable.Range(0, n).Select(j => Enumerable.Range(0, n).Sum(k => q[i][k] * spectrum[k] * q[j][k]))])];
        });
    }

    // --- [WIRE]
    public static readonly Gen<string> WireString = Gen.Frequency(
        (40, Gen.Char[' ', '~'].Array[0, 24].Select(static chars => new string(chars))),
        (20, Gen.Char['\u0080', '\uD7FF'].Array[1, 12].Select(static chars => new string(chars))),
        (14, Gen.Int[0x10000, 0x10FFFF].Array[1, 4].Select(static codes => string.Concat(codes.Select(char.ConvertFromUtf32)))),
        (14, Gen.OneOfConst("", "\"", "\\", "\r\n", "\t", "{", "}", "\u0000", "\u001B", "\uFFFD", "\U0001D518\U0001D52B\U0001D526")),
        (12, Gen.Char['\u0000', '\u001F'].Array[1, 4].Select(static chars => new string(chars))));
    public static readonly Gen<byte[]> Payload = Gen.Frequency(
        (40, Gen.Byte.Array[1, 64]),
        (25, Gen.Byte.Array[65, 4096]),
        (13, Gen.Const<byte[]>([])),
        (12, Gen.Byte.Array[1, 1]),
        (10, Gen.Byte.Select(Gen.Int[16, 256], static (value, count) => (byte[])[.. Enumerable.Repeat(value, count)])));
    public static readonly Gen<(byte[] Original, byte[] Mutated)> Mutant =
        Gen.Byte.Array[1, 256].SelectMany(bytes => Gen.Int[0, bytes.Length - 1].Select(Gen.Int[1, 255], (index, mask) => {
            byte[] copy = [.. bytes];
            copy[index] ^= (byte)mask;
            return (Original: bytes, Mutated: copy);
        }));

    // --- [RESULT]
    public static readonly Gen<Error> Faults = Gen.OneOfConst<Error>(new Fault.Missing(), new Fault.Rejected(), new Fault.Cancelled(), new Fault.Conflict());
    public static readonly Gen<Error> Exceptional = Gen.Int[0, 3].Select(static kind => Error.New(kind switch {
        0 => new InvalidOperationException("<fault-exceptional-invalid>"),
        1 => new ArithmeticException("<fault-exceptional-arithmetic>"),
        2 => new IOException("<fault-exceptional-io>"),
        _ => new FormatException("<fault-exceptional-format>"),
    }));
    public static Gen<Fin<T>> FinOf<T>(Gen<T> succ, Gen<Error>? fail = null, int succWeight = 80) =>
        Gen.Frequency(
            (succWeight, Element(succ).Select(static Fin<T> (v) => v)),
            (100 - succWeight, (fail ?? Faults).Select(static Fin<T> (e) => e)));
    public static Gen<Option<T>> OptionOf<T>(Gen<T> some, int someWeight = 80) =>
        Gen.Frequency(
            (someWeight, Element(some).Select(static Option<T> (v) => v)),
            (100 - someWeight, Gen.Const(Option<T>.None)));
    public static Gen<Validation<Error, T>> ValidationOf<T>(Gen<T> succ, Gen<Error>? fail = null) =>
        Gen.OneOf(
            Element(succ).Select(static Validation<Error, T> (v) => v),
            (fail ?? Faults).Select(static Validation<Error, T> (e) => e));

    // --- [KEYS]
    public static readonly Gen<string> Key = Gen.Char['a', 'z'].Array[1, 32].Select(static chars => new string(chars));
    public static readonly Gen<Guid> Id = Gen.Guid;

    // --- [ADMISSION]
    public static Gen<TVo> Admitted<TIn, TVo>(Gen<TIn> source, TryCreate<TIn, TVo> tryCreate) {
        ArgumentNullException.ThrowIfNull(tryCreate);
        return Element(source)
            .Select(v => tryCreate(v, out TVo owned) ? Some(owned) : Option<TVo>.None)
            .Where(static o => o.IsSome)
            .Select(static o => o.Case is TVo value ? value : default!);
    }

    private static Gen<T> Element<T>(Gen<T> element) {
        ArgumentNullException.ThrowIfNull(element);
        return element;
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
