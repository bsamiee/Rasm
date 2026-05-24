using Rasm.Domain;
using Rhino.Geometry;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
public delegate bool TryCreate<TIn, TOut>(TIn value, out TOut obj);

// --- [CONSTANTS] ----------------------------------------------------------------------------
public static class TestTraits {
    public const string Category = "Category";
    public const string Speed = "Speed";
    public const string Domain = "Domain";
    public const string Algebra = "Algebra";
    public const string Rail = "Rail";
    public const string Snapshot = "Snapshot";
    public const string Fast = "Fast";
    public const string Slow = "Slow";
    public const string Vectors = "Vectors";
    public const string Stats = "Stats";
    public const string Geometry = "Geometry";
    public const string Grasshopper = "Grasshopper";
}

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Spec {
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long iter = 100, int time = 0, int threads = 0) {
        ArgumentNullException.ThrowIfNull(argument: gen);
        bool Wrapped(T value) => Apply(action: property, value: value);
        Action sample = (seed, iter, time, threads) switch {
            (null, 100, 0, 0) => () => gen.Sample(Wrapped),
            (null, _, _, _) => () => gen.Sample(predicate: Wrapped, iter: iter, time: time, threads: threads),
            _ => () => gen.Sample(predicate: Wrapped, seed: seed!, iter: iter, time: time, threads: threads),
        };
        sample();
    }
    public static void Implies<T>(Gen<T> gen, Func<T, bool> premise, Action<T> body) =>
        gen.Sample(value => premise(value) switch { true => Apply(action: body, value: value), false => true });
    public static void Roundtrip<TIn, TOut>(Gen<TIn> gen, Func<TIn, TOut> forward, Func<TOut, TIn> back, Func<TIn, TIn, bool>? eq = null) =>
        gen.Sample(value => EqOrThrow(left: value, right: back(forward(value)), predicate: eq));
    public static void Identity<T>(Gen<T> gen, Func<T, T> f, Func<T, T, bool>? eq = null) =>
        gen.Sample(value => EqOrThrow(left: value, right: f(value), predicate: eq));
    public static void Idempotent<T>(Gen<T> gen, Func<T, T> f, Func<T, T, bool>? eq = null) =>
        gen.Sample(value => EqOrThrow(left: f(value), right: f(f(value)), predicate: eq));
    public static void Commutative<T>(Gen<T> gen, Func<T, T, T> op, Func<T, T, bool>? eq = null) =>
        gen.Select(gen).Sample((T a, T b) => EqOrThrow(left: op(a, b), right: op(b, a), predicate: eq));
    public static void Associative<T>(Gen<T> gen, Func<T, T, T> op, Func<T, T, bool>? eq = null) =>
        gen.Select(gen, gen).Sample((T a, T b, T c) => EqOrThrow(left: op(op(a, b), c), right: op(a, op(b, c)), predicate: eq));
    public static void Inverse<T>(Gen<T> gen, Func<T, T> f, Func<T, T> g, Func<T, T, bool>? eq = null) =>
        gen.Sample(value => EqOrThrow(left: value, right: g(f(value)), predicate: eq));
    public static void Monotone<T, TKey>(Gen<(T Lo, T Hi)> pairs, Func<T, TKey> projection, IComparer<TKey>? comparer = null) =>
        pairs.Sample(p => (comparer ?? Comparer<TKey>.Default).Compare(x: projection(p.Lo), y: projection(p.Hi)) <= 0
            ? true
            : throw new XunitException($"Monotone violated: f({p.Lo}) = {projection(p.Lo)} > {projection(p.Hi)} = f({p.Hi})"));
    public static void Permutation<T, TResult>(Gen<T[]> gen, Func<T[], TResult> f, Func<TResult, TResult, bool>? eq = null) =>
        gen.SelectMany(arr => Gen.Shuffle(arr).Select(perm => (Original: arr, Shuffled: perm))).Sample(p =>
            EqOrThrow(left: f(p.Original), right: f(p.Shuffled), predicate: eq));
    public static void ConcurrentProfiled<T>(Gen<T> init, Action<string> writeLine, params GenOperation<T>[] operations) =>
        Causal.Profile(action: () => (init ?? throw new ArgumentNullException(nameof(init))).SampleParallel(operations))
              .Output(output: writeLine ?? throw new ArgumentNullException(nameof(writeLine)));
    public static void ConcurrentProfiled<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Action<string> writeLine, params GenOperation<TActual, TModel>[] operations) =>
        Causal.Profile(action: () => (init ?? throw new ArgumentNullException(nameof(init))).SampleParallel(operations))
              .Output(output: writeLine ?? throw new ArgumentNullException(nameof(writeLine)));
    public static void Metamorphic<T, TResult>(Gen<T> gen, Func<T, TResult> path, Func<T, TResult> oracle, Func<TResult, TResult, bool>? eq = null) =>
        gen.Sample(value => EqOrThrow(left: path(value), right: oracle(value), predicate: eq));
    public static void Regression<T>(Gen<T> gen, Action<T> property, string seed) =>
        gen.Sample(value => Apply(action: property, value: value), seed: seed, iter: 1);
    public static T SuccValue<T>(Fin<T> result, string label) =>
        (result ?? throw new ArgumentNullException(nameof(result))).Match(
            Succ: static value => value,
            Fail: error => throw new XunitException($"{label}: expected Succ; got Fail: {error.Message}"));
    public static void Holds(bool condition, string label) =>
        _ = condition ? unit : throw new XunitException(userMessage: label);
    public static void Succ<T>(Fin<T> result, Action<T>? then = null) =>
        _ = (result ?? throw new ArgumentNullException(nameof(result))).Match(
            Succ: value => Tap(action: then, value: value),
            Fail: error => throw new XunitException($"Expected Succ; got Fail: {error.Message}"));
    public static void Fail<T>(Fin<T> result, Action<Error>? then = null) =>
        _ = (result ?? throw new ArgumentNullException(nameof(result))).Match(
            Succ: value => throw new XunitException($"Expected Fail; got Succ: {value}"),
            Fail: error => Tap(action: then, value: error));
    // Category-based failure assertion — couples to Fault.Category (stable contract) instead of error.Message (drifts on refactor).
    public static void FailCategory<T>(Fin<T> result, string category) =>
        Fail(result: result, then: error => Assert.Equal(expected: category, actual: error.Category()));
    public static void FailCode<T>(Fin<T> result, int code) =>
        Fail(result: result, then: error => Assert.Equal(expected: code, actual: error.Code));
    // Accumulated-errors assertion for Validation — order-independent; matches LE's commutative Apply.
    // Validation<Error,T> aggregates via Error.+ which produces ManyErrors; flatten to per-error category list.
    public static void AllErrors<T>(Validation<Error, T> v, params string[] expectedCategories) {
        ArgumentNullException.ThrowIfNull(argument: v);
        string[] expected = [.. (expectedCategories ?? throw new ArgumentNullException(nameof(expectedCategories))).OrderBy(static c => c, StringComparer.Ordinal)];
        _ = v.Match(
            Succ: value => throw new XunitException($"Expected Fail; got Succ: {value}"),
            Fail: error => {
                Seq<Error> errors = error switch {
                    ManyErrors many => toSeq(many.Errors),
                    _ => Seq(error),
                };
                string[] actual = [.. errors.Map(static (Error e) => e.Category()).OrderBy(static c => c, StringComparer.Ordinal)];
                Assert.Equal(expected: expected, actual: actual);
                return unit;
            });
    }
    public static void Some<T>(Option<T> result, Action<T>? then = null) =>
        _ = result.Match(
            Some: value => Tap(action: then, value: value),
            None: () => throw new XunitException(userMessage: "Expected Some; got None"));
    public static void None<T>(Option<T> result) =>
        _ = result.Match(
            Some: value => throw new XunitException(userMessage: $"Expected None; got Some: {value}"),
            None: static () => unit);
    // ManyErrors aggregation assertion — any Error.+ rail (Bind rollback, paint hook failures, document mutation rollbacks)
    // produces a ManyErrors on double-fault. Asserts structural shape (exact count) + that each expected substring appears in at least one inner Message.
    public static void ManyErrors(Error error, int expectedCount, params string[] expectedSubstrings) {
        ArgumentNullException.ThrowIfNull(argument: error);
        ArgumentNullException.ThrowIfNull(argument: expectedSubstrings);
        ManyErrors many = Assert.IsType<ManyErrors>(@object: error);
        Assert.Equal(expected: expectedCount, actual: many.Errors.Count);
        _ = expectedSubstrings.AsIterable().Iter(substring =>
            Assert.Contains(collection: many.Errors, filter: e => e.Message.Contains(value: substring, comparisonType: StringComparison.Ordinal)));
    }
    public static void FailMany<T>(Fin<T> result, int expectedCount, params string[] expectedSubstrings) =>
        Fail(result: result, then: error => ManyErrors(error: error, expectedCount: expectedCount, expectedSubstrings: expectedSubstrings));
    // Thinktecture shape laws — collapse repeated key-distinctness + value-object roundtrip + non-finite-rejection patterns.
    public static void SmartEnumKeysUnique<T, TKey>(IReadOnlyList<T> items, Func<T, TKey> key) where TKey : notnull =>
        Assert.Equal(
            expected: (items ?? throw new ArgumentNullException(nameof(items))).Count,
            actual: items.Select(selector: key ?? throw new ArgumentNullException(nameof(key))).Distinct().Count());
    public static void ValueObjectRoundtrip<TVO, TKey>(Gen<TKey> validGen, TryCreate<TKey, TVO> tryCreate, Func<TVO, TKey> read, Func<TKey, TKey, bool>? eq = null) =>
        Roundtrip(
            gen: validGen ?? throw new ArgumentNullException(nameof(validGen)),
            forward: x => (tryCreate ?? throw new ArgumentNullException(nameof(tryCreate)))(x, out TVO v)
                ? v
                : throw new XunitException(userMessage: $"validGen produced invalid value for {typeof(TVO).Name}: {x}"),
            back: read ?? throw new ArgumentNullException(nameof(read)),
            eq: eq);
    public static void ValueObjectRejectsNonFinite(Func<double, bool> tryCreate) =>
        ForAll(
            gen: Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity),
            property: x => Assert.False(condition: (tryCreate ?? throw new ArgumentNullException(nameof(tryCreate)))(x)));
    public static void ValueObjectAccepts<T>(Gen<T> valid, Func<T, bool> tryCreate) =>
        ForAll(valid ?? throw new ArgumentNullException(nameof(valid)), value =>
            Assert.True(condition: (tryCreate ?? throw new ArgumentNullException(nameof(tryCreate)))(value)));
    public static void ValueObjectRejects<T>(Gen<T> invalid, Func<T, bool> tryCreate) =>
        ForAll(invalid ?? throw new ArgumentNullException(nameof(invalid)), value =>
            Assert.False(condition: (tryCreate ?? throw new ArgumentNullException(nameof(tryCreate)))(value)));
    public static void EqualWithin(double left, double right, double tolerance, string? what = null) =>
        _ = Math.Abs(left - right) <= tolerance ? true : throw new XunitException($"{what ?? "EqualWithin"}: |{left:R} - {right:R}| = {Math.Abs(left - right):R} > {tolerance:R}");
    public static void SeqEqualWithin(Seq<double> left, Seq<double> right, double tolerance, string? what = null) {
        Assert.Equal(expected: left.Count, actual: right.Count);
        _ = toSeq(Enumerable.Range(start: 0, count: left.Count)).Iter(i =>
            EqualWithin(left: left[index: i], right: right[index: i], tolerance: tolerance, what: $"{what ?? "Seq"}[{i}]"));
    }
    public static void NearEqual(Point3d left, Point3d right, double tolerance = 1e-9) =>
        EqualWithin(left: left.DistanceTo(other: right), right: 0.0, tolerance: tolerance, what: "Point3d");
    public static void NearEqual(Vector3d left, Vector3d right, double tolerance = 1e-9) =>
        EqualWithin(left: (left - right).Length, right: 0.0, tolerance: tolerance, what: "Vector3d");
    // BOUNDARY ADAPTER — Action<T> -> Sample's Func<T, bool> contract.
    private static bool Apply<T>(Action<T> action, T value) { action(value); return true; }
    // BOUNDARY ADAPTER — optional Action<T> -> LanguageExt Match's Unit-returning lambda contract.
    private static Unit Tap<T>(Action<T>? action, T value) { action?.Invoke(value); return unit; }
    private static bool EqOrThrow<T>(T left, T right, Func<T, T, bool>? predicate) =>
        (predicate ?? EqualityComparer<T>.Default.Equals)(left, right) ? true : throw new XunitException($"Equality failed: {left} != {right}");
}
