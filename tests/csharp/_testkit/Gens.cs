namespace Rasm.TestKit;

// --- [ERRORS] -------------------------------------------------------------------------------
// The typed fault vocabulary rail generators inject: one closed family over `Expected` so failure
// lanes carry case identity (`IsType`, code) instead of anonymous message strings.
[Union]
public abstract partial record Fault : Expected {
    private Fault(string detail, int code) : base(detail, code, None) { }
    public sealed record Missing : Fault { public Missing() : base(detail: "<fault-missing>", code: 9001) { } }
    public sealed record Rejected : Fault { public Rejected() : base(detail: "<fault-rejected>", code: 9002) { } }
    public sealed record Cancelled : Fault { public Cancelled() : base(detail: "<fault-cancelled>", code: 9003) { } }
    public sealed record Conflict : Fault { public Conflict() : base(detail: "<fault-conflict>", code: 9004) { } }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Gens {
    // --- [SCALARS]
    // Magnitude stratification makes every float hazard a weighted lane, never a rare accident:
    // denormals, huge magnitudes, ulp-adjacent neighbours, and signed zero all sample every run.
    private static readonly Gen<double> MagnitudeBand = Gen.Frequency(
        (30, Gen.Double[start: 1.0e-3, finish: 1.0e3]),
        (18, Gen.Double[start: 1.0e3, finish: 1.0e15]),
        (14, Gen.Double[start: 1.0e-30, finish: 1.0e-3]),
        (14, Gen.Double[start: 1.0e15, finish: 1.0e300]),
        (12, Gen.Double[start: double.Epsilon, finish: 2.0e-308]),
        (12, Gen.OneOfConst(0.0, -0.0, 1.0, Math.BitIncrement(x: 1.0), Math.BitDecrement(x: 1.0), Math.PI, double.Epsilon, double.MaxValue / 4.0)));
    public static readonly Gen<double> Finite = MagnitudeBand.Select(Gen.Bool, static (double magnitude, bool negative) => negative ? -magnitude : magnitude);
    public static readonly Gen<double> NonFinite = Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity);
    public static readonly Gen<double> AnyDouble = Gen.Frequency((95, Finite), (5, NonFinite));
    public static readonly Gen<double> Positive = MagnitudeBand.Where(predicate: static x => x > 0.0);
    public static readonly Gen<double> Tame = Gen.Double[start: -1.0e6, finish: 1.0e6];
    public static readonly Gen<double> UnitClosed = Gen.Frequency(
        (92, Gen.Double.Unit),
        (8, Gen.OneOfConst(0.0, 1.0, Math.BitIncrement(x: 0.0), Math.BitDecrement(x: 1.0))));
    // Cancellation pairs: subtraction annihilates leading digits; oracles must survive the band.
    public static readonly Gen<(double X, double Y)> Cancellation = Finite.Select(Gen.Double[start: 1.0e-16, finish: 1.0e-8],
        static (double x, double eps) => (X: x, Y: x * (1.0 + eps)));
    // Wrap hazards are lanes: the seam constants and their ulp neighbours sample every run.
    public static readonly Gen<double> Angle = Gen.Frequency(
        (70, Gen.Double[start: -Math.Tau, finish: Math.Tau]),
        (30, Gen.OneOfConst(0.0, -0.0, Math.PI, -Math.PI, Math.Tau, -Math.Tau, Math.PI / 2.0, -Math.PI / 2.0,
            Math.BitDecrement(x: Math.Tau), Math.BitIncrement(x: -Math.Tau), Math.BitIncrement(x: 0.0), Math.BitDecrement(x: Math.PI))));
    public static readonly Gen<int> IntEdges = Gen.Frequency(
        (70, Gen.Int[start: -1_000_000, finish: 1_000_000]),
        (30, Gen.OneOfConst(int.MinValue, int.MinValue + 1, -1, 0, 1, (1 << 30) - 1, 1 << 30, int.MaxValue - 1, int.MaxValue)));

    // --- [COLLECTIONS]
    public static Gen<T[]> SmallArray<T>(Gen<T> element) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Array[0, 32];
    public static Gen<T[]> NonEmptyArray<T>(Gen<T> element, int max = 256) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Array[1, max];
    public static Gen<T[]> LargeArray<T>(Gen<T> element) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Array[1_000, 10_000];
    public static Gen<T[]> UniqueArray<T>(Gen<T> element) =>
        (element ?? throw new ArgumentNullException(nameof(element))).ArrayUnique[1, 64];
    public static Gen<T[]> SortedArray<T>(Gen<T> element) where T : IComparable<T> =>
        SmallArray(element: element).Select(static a => a.Order().ToArray());
    public static Gen<(T Lo, T Hi)> OrderedPair<T>(Gen<T> element) where T : IComparable<T> =>
        (element ?? throw new ArgumentNullException(nameof(element))).Select(element, static (T a, T b) => a.CompareTo(b) <= 0 ? (Lo: a, Hi: b) : (Lo: b, Hi: a));
    public static Gen<(T A, T B, T C)> DistinctTriple<T>(Gen<T> element) where T : notnull =>
        (element ?? throw new ArgumentNullException(nameof(element))).ArrayUnique[3, 3]
        .Select(static values => (A: values[0], B: values[1], C: values[2]));
    public static Gen<Seq<T>> NonEmptySeq<T>(Gen<T> element, int max = 256) =>
        NonEmptyArray(element: element, max: max).Select(static (T[] xs) => toSeq(xs));
    public static Gen<Seq<T>> SeqOf<T>(Gen<T> element, int max = 256) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Array[0, max].Select(static (T[] xs) => toSeq(xs));
    // Partition of unity: strictly positive weights normalized to sum 1; oracles get a real simplex.
    public static Gen<Seq<double>> Simplex(int count) {
        ArgumentOutOfRangeException.ThrowIfLessThan(value: count, other: 1);
        return Gen.Double[start: 1.0e-6, finish: 1.0e6].Array[count].Select(static values => {
            double total = values.Sum();
            return toSeq(values.Select(value => value / total));
        });
    }

    // --- [GEOMETRY]
    // Unit vectors by annulus-rejected normalization: Where preserves shrinking and the norm floor
    // keeps the division stable in every dimension.
    public static Gen<double[]> Direction(int dim) {
        ArgumentOutOfRangeException.ThrowIfLessThan(value: dim, other: 1);
        return Gen.Double[start: -1.0, finish: 1.0].Array[dim]
            .Where(predicate: static raw => raw.Sum(static x => x * x) >= 1.0e-6)
            .Select(selector: static raw => {
                double norm = Math.Sqrt(d: raw.Sum(static x => x * x));
                return (double[])[.. raw.Select(x => x / norm)];
            });
    }
    // Star-shaped simple rings: equally spaced angles under a random phase with magnitude-banded
    // radii, so CCW orientation and strictly positive shoelace area hold BY CONSTRUCTION.
    public static Gen<double[][]> Ring(int vertices) {
        ArgumentOutOfRangeException.ThrowIfLessThan(value: vertices, other: 3);
        return Gen.Double[start: 1.0e-3, finish: 1.0e3].Array[vertices].Select(Gen.Double[start: -Math.Tau, finish: Math.Tau],
            static (double[] radii, double phase) => (double[][])[.. radii.Select((radius, i) => {
                double theta = phase + (i * Math.Tau / radii.Length);
                return (double[])[radius * Math.Cos(d: theta), radius * Math.Sin(a: theta)];
            })]);
    }
    // The exact-arithmetic torture band: C sits on the AB line (steps == 0) or a few ulps off it,
    // where double-precision orientation determinants misjudge and only exact predicates survive.
    public static Gen<(double[] A, double[] B, double[] C)> NearCollinear(int dim) {
        ArgumentOutOfRangeException.ThrowIfLessThan(value: dim, other: 2);
        return Tame.Array[dim].Select(Tame.Array[dim], Gen.Double[start: -2.0, finish: 2.0],
                static (double[] a, double[] b, double t) => (A: a, B: b, C: (double[])[.. a.Select((x, i) => x + (t * (b[i] - x)))]))
            .Select(Gen.Int[start: 0, finish: dim - 1], Gen.Int[start: -8, finish: 8], static ((double[] A, double[] B, double[] C) p, int axis, int steps) => {
                p.C[axis] = UlpNudge(value: p.C[axis], steps: steps);
                return (p.A, p.B, p.C);
            });
    }
    // Householder products are orthogonal up to rounding but n reflections pin det = (-1)^n; the
    // Bool-driven row flip covers BOTH O(n) components, so rotations and reflections sample every run.
    public static Gen<double[][]> Orthogonal(int n) {
        ArgumentOutOfRangeException.ThrowIfLessThan(value: n, other: 1);
        return Direction(dim: n).Array[n].Select(Gen.Bool, static (double[][] reflectors, bool flip) => {
            double[][] q = reflectors.Aggregate(seed: IdentityMatrix(n: reflectors.Length), func: Reflect);
            return flip ? [.. q.Select(static (row, i) => i == 0 ? [.. row.Select(static x => -x)] : row)] : q;
        });
    }
    // Q D Qᵀ with a log-spaced spectrum 1 … 1/kappa: the condition number is known BY CONSTRUCTION,
    // so conditioning-aware tolerances (κ·base) come from the generator, never a guessed constant.
    public static Gen<double[][]> Conditioned(int n, double kappa) {
        ArgumentOutOfRangeException.ThrowIfLessThan(value: n, other: 1);
        _ = double.IsFinite(d: kappa) && kappa >= 1.0
            ? kappa : throw new ArgumentOutOfRangeException(paramName: nameof(kappa), actualValue: kappa, message: "kappa must be finite and >= 1");
        return Orthogonal(n: n).Select(selector: q => {
            double[] spectrum = [.. Enumerable.Range(start: 0, count: n).Select(i => Math.Pow(x: kappa, y: n == 1 ? 0.0 : -(double)i / (n - 1)))];
            return (double[][])[.. Enumerable.Range(start: 0, count: n).Select(i =>
                (double[])[.. Enumerable.Range(start: 0, count: n).Select(j => Enumerable.Range(start: 0, count: n).Sum(k => q[i][k] * spectrum[k] * q[j][k]))])];
        });
    }
    private static double[][] IdentityMatrix(int n) =>
        [.. Enumerable.Range(start: 0, count: n).Select(i => (double[])[.. Enumerable.Range(start: 0, count: n).Select(j => i == j ? 1.0 : 0.0)])];
    private static double[][] Reflect(double[][] m, double[] v) {
        double[] w = [.. Enumerable.Range(start: 0, count: v.Length).Select(j => Enumerable.Range(start: 0, count: v.Length).Sum(k => v[k] * m[k][j]))];
        return [.. Enumerable.Range(start: 0, count: v.Length).Select(i => (double[])[.. Enumerable.Range(start: 0, count: v.Length).Select(j => m[i][j] - (2.0 * v[i] * w[j]))])];
    }
    private static double UlpNudge(double value, int steps) =>
        Enumerable.Range(start: 0, count: Math.Abs(value: steps)).Aggregate(seed: value, func: (acc, _) => steps > 0 ? Math.BitIncrement(x: acc) : Math.BitDecrement(x: acc));

    // --- [WIRE]
    // Wire-safe hazard strings: every JSON hazard a codec must survive — controls, quotes,
    // escapes, BMP unicode, astral pairs — never a lone surrogate no UTF-8 wire can carry.
    public static readonly Gen<string> WireString = Gen.Frequency(
        (40, Gen.Char[start: ' ', finish: '~'].Array[0, 24].Select(selector: static chars => new string(value: chars))),
        (20, Gen.Char[start: '\u0080', finish: '\uD7FF'].Array[1, 12].Select(selector: static chars => new string(value: chars))),
        (14, Gen.Int[start: 0x10000, finish: 0x10FFFF].Array[1, 4].Select(selector: static codes => string.Concat(values: codes.Select(selector: char.ConvertFromUtf32)))),
        (14, Gen.OneOfConst("", "\"", "\\", "\r\n", "\t", "{", "}", "\u0000", "\u001B", "\uFFFD", "\U0001D518\U0001D52B\U0001D526")),
        (12, Gen.Char[start: '\u0000', finish: '\u001F'].Array[1, 4].Select(selector: static chars => new string(value: chars))));
    // Content payload bands: empty, single, small, large, and constant-run blocks so identity,
    // codec, and chunking hazards all sample every run instead of arriving as rare accidents.
    public static readonly Gen<byte[]> Payload = Gen.Frequency(
        (40, Gen.Byte.Array[1, 64]),
        (25, Gen.Byte.Array[65, 4096]),
        (13, Gen.Const<byte[]>(value: [])),
        (12, Gen.Byte.Array[1, 1]),
        (10, Gen.Byte.Select(Gen.Int[start: 16, finish: 256], static (byte value, int count) => (byte[])[.. Enumerable.Repeat(element: value, count: count)])));
    // One-byte-flip near-duplicate pairs: the canonical separation witness for content-key laws —
    // a mint that cannot split a mutant pair cannot address content.
    public static readonly Gen<(byte[] Original, byte[] Mutated)> Mutant =
        Gen.Byte.Array[1, 256].SelectMany(selector: bytes => Gen.Int[start: 0, finish: bytes.Length - 1].Select(Gen.Int[start: 1, finish: 255],
            (int index, int mask) => {
                byte[] copy = [.. bytes];
                copy[index] ^= (byte)mask;
                return (Original: bytes, Mutated: copy);
            }));

    // --- [STAMPS]
    // HLC stamp band: physical half is a NodaTime-style Unix tick long, logical half a monotone
    // ulong; saturation edges ride as lanes so overflow seams sample every run.
    public static readonly Gen<(long Physical, ulong Logical)> Hlc = Gen.Frequency(
            (70, Gen.Long[start: 0L, finish: 3_155_378_975_999_999_999L]),
            (30, Gen.OneOfConst(0L, 1L, 621_355_968_000_000_000L, long.MaxValue - 1L, long.MaxValue)))
        .Select(Gen.Frequency((70, Gen.ULong[start: 0UL, finish: 4096UL]), (30, Gen.OneOfConst(0UL, 1UL, ulong.MaxValue - 1UL, ulong.MaxValue))),
            static (long physical, ulong logical) => (Physical: physical, Logical: logical));

    // --- [QUANTITIES]
    // Seven SI base exponents ordered (mass, length, time, current, temperature, amount,
    // luminosity); canonical rows include the energy/torque coincidence QuantityType must split.
    // Every draw is a fresh array, so a mutating consumer never poisons the band.
    public static readonly Gen<int[]> SiExponents = Gen.Frequency(
            (70, Gen.Int[start: -4, finish: 4].Array[7]),
            (30, Gen.OneOfConst<int[]>(
                [0, 0, 0, 0, 0, 0, 0],
                [0, 1, 0, 0, 0, 0, 0],
                [1, 0, 0, 0, 0, 0, 0],
                [0, 0, 1, 0, 0, 0, 0],
                [1, 1, -2, 0, 0, 0, 0],
                [1, 2, -2, 0, 0, 0, 0],
                [1, -1, -2, 0, 0, 0, 0])))
        .Select(selector: static row => (int[])[.. row]);
    // SI-normalized measure: magnitude rides the full finite hazard band under a dimension vector.
    public static readonly Gen<(double Si, int[] Exponents)> Measure =
        Finite.Select(SiExponents, static (double si, int[] exponents) => (Si: si, Exponents: exponents));

    // --- [RAIL]
    public static readonly Gen<Error> Faults = Gen.OneOfConst<Error>(
        new Fault.Missing(), new Fault.Rejected(), new Fault.Cancelled(), new Fault.Conflict());
    // Exceptional-lane errors carry live exceptions, so recovery rails prove they survive the
    // Exceptional/Expected split. Error.New remaps Timeout/OperationCanceled/Aggregate to Expected
    // rows, so only genuinely-exceptional types ride here; instances mint fresh per sample.
    public static readonly Gen<Error> Exceptional = Gen.Int[start: 0, finish: 3].Select(selector: static kind => kind switch {
        0 => Error.New(thisException: new InvalidOperationException(message: "<fault-exceptional-invalid>")),
        1 => Error.New(thisException: new ArithmeticException(message: "<fault-exceptional-arithmetic>")),
        2 => Error.New(thisException: new IOException(message: "<fault-exceptional-io>")),
        _ => Error.New(thisException: new FormatException(message: "<fault-exceptional-format>")),
    });
    public static Gen<Fin<T>> FinOf<T>(Gen<T> succ, Gen<Error>? fail = null, int succWeight = 80) =>
        Gen.Frequency(
            (succWeight, (succ ?? throw new ArgumentNullException(nameof(succ))).Select(static (T v) => Fin.Succ(value: v))),
            (100 - succWeight, (fail ?? Faults).Select(static (Error e) => Fin.Fail<T>(error: e))));
    public static Gen<Option<T>> OptionOf<T>(Gen<T> some, int someWeight = 80) =>
        Gen.Frequency(
            (someWeight, (some ?? throw new ArgumentNullException(nameof(some))).Select(static (T v) => Some(value: v))),
            (100 - someWeight, Gen.Const(value: Option<T>.None)));
    public static Gen<Validation<Error, T>> ValidationOf<T>(Gen<T> succ, Gen<Error>? fail = null) =>
        Gen.OneOf(
            (succ ?? throw new ArgumentNullException(nameof(succ))).Select(static (T v) => Success<Error, T>(value: v)),
            (fail ?? Faults).Select(static (Error e) => Fail<Error, T>(value: e)));

    // --- [KEYS]
    public static readonly Gen<string> Key = Gen.Char[start: 'a', finish: 'z'].Array[1, 32].Select(selector: static chars => new string(value: chars));
    public static readonly Gen<Guid> Id = Gen.Guid;

    // --- [ADMISSION]
    // Filtering Option preserves shrinking; Optional folds a null-yielding TryCreate into the
    // filtered lane and the post-filter arm stays total — no throw ever enters a Select.
    public static Gen<TVo> Admitted<TIn, TVo>(Gen<TIn> source, TryCreate<TIn, TVo> tryCreate) {
        ArgumentNullException.ThrowIfNull(argument: source);
        ArgumentNullException.ThrowIfNull(argument: tryCreate);
        return source.Select(v => tryCreate(v, out TVo owned) ? Optional(value: owned) : Option<TVo>.None)
            .Where(predicate: static o => o.IsSome)
            .Select(selector: static o => o.Case is TVo value ? value : default!);
    }
}
