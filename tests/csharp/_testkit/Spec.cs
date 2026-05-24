using System.Globalization;
using Rasm.Domain;
using Rhino.Geometry;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
public delegate bool TryCreate<TIn, TOut>(TIn value, out TOut obj);

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Spec {
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(argument: gen);
        bool Wrapped(T value) => Apply(action: property, value: value);
        SamplePolicy policy = SamplePolicy.Of(seed: seed, iter: iter, time: time, threads: threads);
        Action sample = policy switch {
            { IsDefault: true } => () => gen.Sample(Wrapped),
            { Seed: null } => () => gen.Sample(predicate: Wrapped, iter: policy.IterOrDefault, time: policy.TimeOrDefault, threads: policy.ThreadsOrDefault),
            _ => () => gen.Sample(predicate: Wrapped, seed: policy.Seed!, iter: policy.IterOrDefault, time: policy.TimeOrDefault, threads: policy.ThreadsOrDefault),
        };
        sample();
    }
    public static void Implies<T>(Gen<T> gen, Func<T, bool> premise, Action<T> body) =>
        ForAll(gen: gen, property: value => _ = premise(value) switch { true => Apply(action: body, value: value), false => true });
    public static void Roundtrip<TIn, TOut>(Gen<TIn> gen, Func<TIn, TOut> forward, Func<TOut, TIn> back, Func<TIn, TIn, bool>? eq = null) =>
        ForAll(gen: gen, property: value => _ = EqOrThrow(left: value, right: back(forward(value)), predicate: eq));
    public static void Identity<T>(Gen<T> gen, Func<T, T> f, Func<T, T, bool>? eq = null) =>
        ForAll(gen: gen, property: value => _ = EqOrThrow(left: value, right: f(value), predicate: eq));
    public static void Idempotent<T>(Gen<T> gen, Func<T, T> f, Func<T, T, bool>? eq = null) =>
        ForAll(gen: gen, property: value => _ = EqOrThrow(left: f(value), right: f(f(value)), predicate: eq));
    public static void Commutative<T>(Gen<T> gen, Func<T, T, T> op, Func<T, T, bool>? eq = null) =>
        ForAll(gen.Select(gen), ((T a, T b) p) => _ = EqOrThrow(left: op(p.a, p.b), right: op(p.b, p.a), predicate: eq));
    public static void Associative<T>(Gen<T> gen, Func<T, T, T> op, Func<T, T, bool>? eq = null) =>
        ForAll(gen.Select(gen, gen), ((T a, T b, T c) p) => _ = EqOrThrow(left: op(op(p.a, p.b), p.c), right: op(p.a, op(p.b, p.c)), predicate: eq));
    public static void Inverse<T>(Gen<T> gen, Func<T, T> f, Func<T, T> g, Func<T, T, bool>? eq = null) =>
        ForAll(gen: gen, property: value => _ = EqOrThrow(left: value, right: g(f(value)), predicate: eq));
    public static void Monotone<T, TKey>(Gen<(T Lo, T Hi)> pairs, Func<T, TKey> projection, IComparer<TKey>? comparer = null) =>
        ForAll(pairs, p => _ = (comparer ?? Comparer<TKey>.Default).Compare(x: projection(p.Lo), y: projection(p.Hi)) <= 0
            ? true
            : throw new XunitException($"Monotone violated: f({p.Lo}) = {projection(p.Lo)} > {projection(p.Hi)} = f({p.Hi})"));
    public static void Permutation<T, TResult>(Gen<T[]> gen, Func<T[], TResult> f, Func<TResult, TResult, bool>? eq = null) =>
        ForAll(gen.SelectMany(arr => Gen.Shuffle(arr).Select(perm => (Original: arr, Shuffled: perm))), p =>
            _ = EqOrThrow(left: f(p.Original), right: f(p.Shuffled), predicate: eq));
    public static void ConcurrentProfiled<T>(Gen<T> init, Action<string> writeLine, params GenOperation<T>[] operations) =>
        Causal.Profile(action: () => (init ?? throw new ArgumentNullException(nameof(init))).SampleParallel(operations))
              .Output(output: writeLine ?? throw new ArgumentNullException(nameof(writeLine)));
    public static void ConcurrentProfiled<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Action<string> writeLine, params GenOperation<TActual, TModel>[] operations) =>
        Causal.Profile(action: () => (init ?? throw new ArgumentNullException(nameof(init))).SampleParallel(operations))
              .Output(output: writeLine ?? throw new ArgumentNullException(nameof(writeLine)));
    public static void Metamorphic<T, TResult>(Gen<T> gen, Func<T, TResult> path, Func<T, TResult> oracle, Func<TResult, TResult, bool>? eq = null, string? seed = null, long? iter = null, int? time = null, int? threads = null) =>
        ForAll(gen: gen, property: value => _ = EqOrThrow(left: path(value), right: oracle(value), predicate: eq), seed: seed, iter: iter, time: time, threads: threads);
    public static void Regression<T>(Gen<T> gen, Action<T> property, string seed) =>
        ForAll(gen: gen, property: property, seed: seed, iter: 1);
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
    public static void Valid<T>(Validation<Error, T> result, Action<T>? then = null) =>
        _ = (result ?? throw new ArgumentNullException(nameof(result))).Match(
            Succ: value => Tap(action: then, value: value),
            Fail: error => throw new XunitException($"Expected Valid; got Invalid: {error.Message}"));
    public static void Invalid<T>(Validation<Error, T> result, Action<Error>? then = null) =>
        _ = (result ?? throw new ArgumentNullException(nameof(result))).Match(
            Succ: value => throw new XunitException($"Expected Invalid; got Valid: {value}"),
            Fail: error => Tap(action: then, value: error));
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
    public static void Cases<T, TKey>(IReadOnlyList<T> items, Func<T, TKey> key, Action<T> law) where TKey : notnull {
        SmartEnumKeysUnique(items: items, key: key);
        _ = (items ?? throw new ArgumentNullException(nameof(items))).AsIterable().Iter(value => (law ?? throw new ArgumentNullException(nameof(law)))(value));
    }
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
    private readonly record struct SamplePolicy(string? Seed, long? Iter, int? Time, int? Threads) {
        public bool IsDefault => Seed is null && Iter is null && Time is null && Threads is null;
        public long IterOrDefault => Iter ?? Check.Iter;
        public int TimeOrDefault => Time ?? Check.Time;
        public int ThreadsOrDefault => Threads ?? Check.Threads;
        public static SamplePolicy Of(string? seed, long? iter, int? time, int? threads) =>
            new(
                Seed: seed ?? Environment.GetEnvironmentVariable(variable: "CsCheck_Seed"),
                Iter: iter ?? EnvLong(name: "CsCheck_Iter"),
                Time: time ?? EnvInt(name: "CsCheck_Time"),
                Threads: threads ?? EnvInt(name: "CsCheck_Threads"));
        private static long? EnvLong(string name) =>
            long.TryParse(s: Environment.GetEnvironmentVariable(variable: name), style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out long value) && value > 0L ? value : null;
        private static int? EnvInt(string name) =>
            int.TryParse(s: Environment.GetEnvironmentVariable(variable: name), style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out int value) && value > 0 ? value : null;
    }
}
