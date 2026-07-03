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

// --- [SERVICES] -----------------------------------------------------------------------------
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
    public static Gen<Seq<double>> Simplex(int count) =>
        count switch {
            <= 0 => Gen.Const(value: Seq<double>()),
            _ => Gen.Double[start: 1.0e-6, finish: 1.0e6].Array[count].Select(static values => {
                double total = values.Sum();
                return toSeq(values.Select(value => value / total));
            }),
        };

    // --- [RAIL]
    public static readonly Gen<Error> Faults = Gen.OneOfConst<Error>(
        new Fault.Missing(), new Fault.Rejected(), new Fault.Cancelled(), new Fault.Conflict());
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
    // Filtering Option preserves shrinking; throwing in Select turns rejects into failures.
    public static Gen<TVo> Admitted<TIn, TVo>(Gen<TIn> source, TryCreate<TIn, TVo> tryCreate) {
        ArgumentNullException.ThrowIfNull(argument: source);
        ArgumentNullException.ThrowIfNull(argument: tryCreate);
        return source.Select(v => tryCreate(v, out TVo owned) ? Some(value: owned) : Option<TVo>.None)
            .Where(predicate: static o => o.IsSome)
            .Select(selector: static o => o.Case is TVo value ? value : throw new InvalidOperationException(message: "Filtered option generator produced None."));
    }
}
