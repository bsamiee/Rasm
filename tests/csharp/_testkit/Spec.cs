using CsCheck;
using Rhino.Geometry;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Spec {
    public static void ForAll<T>(Gen<T> gen, Action<T> property) =>
        gen.Sample(value => Apply(action: property, value: value));
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
    public static void Succ<T>(Fin<T> result, Action<T>? then = null) =>
        _ = (result ?? throw new ArgumentNullException(nameof(result))).Match(
            Succ: value => Tap(action: then, value: value),
            Fail: error => throw new XunitException($"Expected Succ; got Fail: {error.Message}"));
    public static void Fail<T>(Fin<T> result, Action<Error>? then = null) =>
        _ = (result ?? throw new ArgumentNullException(nameof(result))).Match(
            Succ: value => throw new XunitException($"Expected Fail; got Succ: {value}"),
            Fail: error => Tap(action: then, value: error));
    public static void EqualWithin(double left, double right, double tolerance, string? what = null) =>
        _ = Math.Abs(left - right) <= tolerance ? true : throw new XunitException($"{what ?? "EqualWithin"}: |{left:R} - {right:R}| = {Math.Abs(left - right):R} > {tolerance:R}");
    public static void NearEqual(Point3d left, Point3d right, double tolerance = 1e-9) =>
        EqualWithin(left: left.DistanceTo(other: right), right: 0.0, tolerance: tolerance, what: "Point3d");
    public static void NearEqual(Vector3d left, Vector3d right, double tolerance = 1e-9) =>
        EqualWithin(left: (left - right).Length, right: 0.0, tolerance: tolerance, what: "Vector3d");
    // BOUNDARY ADAPTER — Action<T> -> Sample's Func<T, bool> contract.
    private static bool Apply<T>(Action<T> action, T value) { action(value); return true; }
    // BOUNDARY ADAPTER — optional Action<T> -> LanguageExt Match's Unit-returning lambda contract.
    private static Unit Tap<T>(Action<T>? action, T value) { action?.Invoke(value); return unit; }
    private static bool EqOrThrow<T>(T left, T right, Func<T, T, bool>? predicate) =>
        (predicate ?? EqualityComparer<T>.Default.Equals)(left!, right!) ? true : throw new XunitException($"Equality failed: {left} != {right}");
}
