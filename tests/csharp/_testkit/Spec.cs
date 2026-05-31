using System.Globalization;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
public delegate bool TryCreate<TIn, TOut>(TIn value, out TOut obj);

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Spec {
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        bool Wrapped(T value) { Cancel(); return Apply(action: property, value: value); }
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
        EqLaw(gen: gen, left: x => x, right: x => back(forward(x)), eq: eq);
    public static void Identity<T>(Gen<T> gen, Func<T, T> f, Func<T, T, bool>? eq = null) =>
        EqLaw(gen: gen, left: x => x, right: f, eq: eq);
    public static void Idempotent<T>(Gen<T> gen, Func<T, T> f, Func<T, T, bool>? eq = null) =>
        EqLaw(gen: gen, left: f, right: x => f(f(x)), eq: eq);
    public static void Inverse<T>(Gen<T> gen, Func<T, T> f, Func<T, T> g, Func<T, T, bool>? eq = null) =>
        EqLaw(gen: gen, left: x => x, right: x => g(f(x)), eq: eq);
    public static void Commutative<T>(Gen<T> gen, Func<T, T, T> op, Func<T, T, bool>? eq = null) =>
        ForAll(gen.Select(gen), ((T a, T b) p) => _ = EqOrThrow(left: op(p.a, p.b), right: op(p.b, p.a), predicate: eq));
    public static void Associative<T>(Gen<T> gen, Func<T, T, T> op, Func<T, T, bool>? eq = null) =>
        ForAll(gen.Select(gen, gen), ((T a, T b, T c) p) => _ = EqOrThrow(left: op(op(p.a, p.b), p.c), right: op(p.a, op(p.b, p.c)), predicate: eq));
    public static void Monotone<T, TKey>(Gen<(T Lo, T Hi)> pairs, Func<T, TKey> projection, IComparer<TKey>? comparer = null) =>
        ForAll(pairs, p => Holds(condition: (comparer ?? Comparer<TKey>.Default).Compare(x: projection(p.Lo), y: projection(p.Hi)) <= 0,
            label: $"Monotone violated: f({p.Lo}) = {projection(p.Lo)} > {projection(p.Hi)} = f({p.Hi})"));
    public static void Permutation<T, TResult>(Gen<T[]> gen, Func<T[], TResult> f, Func<TResult, TResult, bool>? eq = null) =>
        ForAll(gen.SelectMany(arr => Gen.Shuffle(arr).Select(perm => (Original: arr, Shuffled: perm))), p =>
            _ = EqOrThrow(left: f(p.Original), right: f(p.Shuffled), predicate: eq));
    public static void ConcurrentProfiled<T>(Gen<T> init, Action<string> writeLine, params GenOperation<T>[] operations) =>
        Causal.Profile(action: () => { Cancel(); init.SampleParallel(operations); }).Output(output: writeLine);
    public static void ConcurrentProfiled<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Action<string> writeLine, params GenOperation<TActual, TModel>[] operations) =>
        Causal.Profile(action: () => { Cancel(); init.SampleParallel(operations); }).Output(output: writeLine);
    public static void Metamorphic<T, TResult>(Gen<T> gen, Func<T, TResult> path, Func<T, TResult> oracle, Func<TResult, TResult, bool>? eq = null, string? seed = null, long? iter = null, int? time = null, int? threads = null) =>
        ForAll(gen: gen, property: value => _ = EqOrThrow(left: path(value), right: oracle(value), predicate: eq), seed: seed, iter: iter, time: time, threads: threads);
    public static void Metamorphic<TSource, TFollow, TObserved>(Gen<(TSource Source, TFollow Follow)> gen, Func<TSource, TObserved> observeSource, Func<TFollow, TObserved> observeFollow,
        Func<TSource, TFollow, TObserved, TObserved, bool> relation, string? label = null, string? seed = null, long? iter = null, int? time = null, int? threads = null) =>
        ForAll(gen: gen, property: sample => {
            TObserved source = observeSource(sample.Source);
            TObserved follow = observeFollow(sample.Follow);
            Holds(condition: relation(arg1: sample.Source, arg2: sample.Follow, arg3: source, arg4: follow),
                label: label ?? $"Metamorphic relation failed: source={source}; followup={follow}");
        }, seed: seed, iter: iter, time: time, threads: threads);
    public static void Regression<T>(Gen<T> gen, Action<T> property, string seed) =>
        ForAll(gen: gen, property: property, seed: seed, iter: 1);

    public static T SuccValue<T>(Fin<T> result, string label) {
        ArgumentNullException.ThrowIfNull(argument: result);
        return result.Match(Succ: static value => value, Fail: error => throw new XunitException($"{label}: expected Succ; got Fail: {error.Message}"));
    }
    public static void Holds(bool condition, string label) =>
        _ = condition ? unit : throw new XunitException(userMessage: label);
    public static void Succ<T>(Fin<T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(argument: result);
        _ = result.Match(Succ: value => Tap(action: then, value: value), Fail: error => throw new XunitException($"Expected Succ; got Fail: {error.Message}"));
    }
    public static void Fail<T>(Fin<T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(argument: result);
        _ = result.Match(Succ: value => throw new XunitException($"Expected Fail; got Succ: {value}"), Fail: error => Tap(action: then, value: error));
    }
    // Fault.Category is the stable contract (Message drifts on refactor) — couples failure tests to dispatch category.
    public static void FailCategory<T>(Fin<T> result, string category) =>
        Fail(result: result, then: error => Assert.Equal(expected: category, actual: error.Category()));
    public static void FailCode<T>(Fin<T> result, int code) =>
        Fail(result: result, then: error => Assert.Equal(expected: code, actual: error.Code));
    public static void Valid<T>(Validation<Error, T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(argument: result);
        _ = result.Match(Succ: value => Tap(action: then, value: value), Fail: error => throw new XunitException($"Expected Valid; got Invalid: {error.Message}"));
    }
    public static void Invalid<T>(Validation<Error, T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(argument: result);
        _ = result.Match(Succ: value => throw new XunitException($"Expected Invalid; got Valid: {value}"), Fail: error => Tap(action: then, value: error));
    }
    public static void Some<T>(Option<T> result, Action<T>? then = null) =>
        _ = result.Match(Some: value => Tap(action: then, value: value), None: () => throw new XunitException(userMessage: "Expected Some; got None"));
    public static void None<T>(Option<T> result) =>
        _ = result.Match(Some: value => throw new XunitException(userMessage: $"Expected None; got Some: {value}"), None: static () => unit);
    public static void CountsConserve(int attempted, int emitted, int rejected, string label) {
        Holds(condition: attempted >= 0 && emitted >= 0 && rejected >= 0, label: $"{label}: negative count (attempted={attempted}, emitted={emitted}, rejected={rejected})");
        Assert.Equal(expected: attempted, actual: emitted + rejected);
    }
    // Order-independent: Error.+ produces ManyErrors on double-fault; flatten + ordinal-sort asserts Applicative commutativity.
    public static void AllErrors<T>(Validation<Error, T> v, params string[] expectedCategories) {
        ArgumentNullException.ThrowIfNull(argument: v);
        string[] expected = [.. expectedCategories.OrderBy(static c => c, StringComparer.Ordinal)];
        _ = v.Match(Succ: value => throw new XunitException($"Expected Fail; got Succ: {value}"), Fail: error => {
            Seq<Error> errors = error switch { ManyErrors many => toSeq(many.Errors), _ => Seq(error) };
            string[] actual = [.. errors.Map(static (Error e) => e.Category()).OrderBy(static c => c, StringComparer.Ordinal)];
            Assert.Equal(expected: expected, actual: actual);
            return unit;
        });
    }
    public static void ManyErrors(Error error, int expectedCount, params string[] expectedSubstrings) {
        ManyErrors many = Assert.IsType<ManyErrors>(@object: error);
        Assert.Equal(expected: expectedCount, actual: many.Errors.Count);
        _ = expectedSubstrings.AsIterable().Iter(substring =>
            Assert.Contains(collection: many.Errors, filter: e => e.Message.Contains(value: substring, comparisonType: StringComparison.Ordinal)));
    }
    public static void FailMany<T>(Fin<T> result, int expectedCount, params string[] expectedSubstrings) =>
        Fail(result: result, then: error => ManyErrors(error: error, expectedCount: expectedCount, expectedSubstrings: expectedSubstrings));

    public static void SmartEnumKeysUnique<T, TKey>(IReadOnlyList<T> items, Func<T, TKey> key) where TKey : notnull {
        ArgumentNullException.ThrowIfNull(argument: items);
        Assert.Equal(expected: items.Count, actual: items.Select(selector: key).Distinct().Count());
    }
    public static void Cases<T, TKey>(IReadOnlyList<T> items, Func<T, TKey> key, Action<T> law) where TKey : notnull {
        SmartEnumKeysUnique(items: items, key: key);
        _ = items.AsIterable().Iter(value => { Cancel(); law(value); });
    }
    public static void ValueObjectRoundtrip<TVO, TKey>(Gen<TKey> validGen, TryCreate<TKey, TVO> tryCreate, Func<TVO, TKey> read, Func<TKey, TKey, bool>? eq = null) =>
        Roundtrip(gen: validGen, forward: x => tryCreate(x, out TVO v) ? v : throw new XunitException(userMessage: $"validGen produced invalid value for {typeof(TVO).Name}: {x}"), back: read, eq: eq);
    public static void ValueObjectRejectsNonFinite(Func<double, bool> tryCreate) =>
        ForAll(gen: Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity), property: x => Assert.False(condition: tryCreate(x)));
    public static void ValueObjectAccepts<T>(Gen<T> valid, Func<T, bool> tryCreate) =>
        ForAll(valid, value => Assert.True(condition: tryCreate(value)));
    public static void ValueObjectRejects<T>(Gen<T> invalid, Func<T, bool> tryCreate) =>
        ForAll(invalid, value => Assert.False(condition: tryCreate(value)));

    // --- [APPROX_EQUALITY] -------------------------------------------------------------
    // Facade over Approx.Equal defaulting to absolute tolerance; advanced specs build Tolerance directly (Hybrid/FromContext).
    public static void Equal(double left, double right, double tolerance = 1e-9, string? what = null) =>
        Eq(ok: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance)),
           label: $"{what ?? "Equal"}: |{left:R} - {right:R}| = {Math.Abs(left - right):R} > {tolerance:R}");
    public static void Equal(Point3d left, Point3d right, double tolerance = 1e-9, string? what = null) =>
        Eq(ok: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance)),
           label: $"{what ?? "Point3d"}: {left} ≠ {right} (d={left.DistanceTo(other: right):R} > {tolerance:R})");
    public static void Equal(Vector3d left, Vector3d right, double tolerance = 1e-9, string? what = null) =>
        Eq(ok: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance)),
           label: $"{what ?? "Vector3d"}: {left} ≠ {right} (|Δ|={(left - right).Length:R} > {tolerance:R})");
    public static void Equal(Plane left, Plane right, double tolerance = 1e-9, string? what = null) =>
        Eq(ok: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance)), label: $"{what ?? "Plane"} mismatch (tol={tolerance:R})");
    public static void Equal(Transform left, Transform right, double tolerance = 1e-9, string? what = null) =>
        Eq(ok: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance)), label: $"{what ?? "Transform"} mismatch (tol={tolerance:R})");
    public static void Equal(Arr<double> left, Arr<double> right, double tolerance = 1e-9, string? what = null) {
        Assert.Equal(expected: left.Count, actual: right.Count);
        Eq(ok: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance)), label: $"{what ?? "Arr"} mismatch (tol={tolerance:R})");
    }
    public static void Equal(Seq<double> left, Seq<double> right, double tolerance = 1e-9, string? what = null) {
        Assert.Equal(expected: left.Count, actual: right.Count);
        Eq(ok: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance)), label: $"{what ?? "Seq"} mismatch (tol={tolerance:R})");
    }
    // Sign-ambiguous unit-vector equality — eigenvectors and principal axes have arbitrary sign.
    public static void EqualSignAmbiguous(Vector3d left, Vector3d right, double tolerance = 1e-9, string? what = null) =>
        Eq(ok: Math.Min(val1: (left - right).Length, val2: (left + right).Length) <= tolerance,
           label: $"{what ?? "UnitVec"} (sign-amb): min(|Δ|, |Σ|)={Math.Min(val1: (left - right).Length, val2: (left + right).Length):R} > {tolerance:R}");

    // --- [POLYMORPHIC_COMBINATORS] -----------------------------------------------------
    public static void MonotoneSeq<T>(Gen<Seq<T>> gen, IComparer<T>? comparer = null, string? seed = null, long? iter = null, int? time = null, int? threads = null) =>
        ForAll(gen: gen, property: values => _ = toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: values.Count - 1))).Iter(i =>
            Holds(condition: (comparer ?? Comparer<T>.Default).Compare(x: values[index: i - 1], y: values[index: i]) <= 0,
                  label: $"MonotoneSeq[{i}]: {values[index: i - 1]} > {values[index: i]}")), seed: seed, iter: iter, time: time, threads: threads);
    public static void Distributive<T>(Gen<(T A, T B, T C)> gen, Func<T, T, T> mul, Func<T, T, T> add, Func<T, T, bool>? eq = null) =>
        EqLaw(gen: gen, left: t => mul(t.A, add(t.B, t.C)), right: t => add(mul(t.A, t.B), mul(t.A, t.C)), eq: eq);
    public static void Linearity<T>(Gen<(T A, T B, double K)> gen, Func<T, T, T> add, Func<T, double, T> scale, Func<T, T, bool>? eq = null) =>
        EqLaw(gen: gen, left: t => scale(add(t.A, t.B), t.K), right: t => add(scale(t.A, t.K), scale(t.B, t.K)), eq: eq);
    public static void TriangleInequality<T>(Gen<(T A, T B, T C)> gen, Func<T, T, double> distance) =>
        ForAll(gen: gen, property: t => Holds(condition: distance(t.A, t.C) <= distance(t.A, t.B) + distance(t.B, t.C) + 1.0e-9,
            label: $"TriangleInequality: d(a,c)={distance(t.A, t.C):R} > d(a,b)+d(b,c)={distance(t.A, t.B) + distance(t.B, t.C):R}"));
    public static void FinChainOrder<T>(Gen<T> gen, Func<T, Fin<T>> first, Func<T, Fin<T>> second, Func<T, T, bool>? eq = null) =>
        ForAll(gen: gen, property: value => {
            (Fin<T> left, Fin<T> right) = (first(value).Bind(second), second(value).Bind(first));
            _ = (left.IsSucc, right.IsSucc) switch {
                (true, true) => EqOrThrow(left: SuccValue(result: left, label: "left"), right: SuccValue(result: right, label: "right"), predicate: eq),
                (false, false) => true,
                _ => throw new XunitException($"FinChainOrder asymmetric: left.IsSucc={left.IsSucc}, right.IsSucc={right.IsSucc}"),
            };
        });
    public static void ValidationAccumOrder<T>(Gen<T> gen, Func<T, Validation<Error, T>> path, params string[] expectedCategories) =>
        ForAll(gen: gen, property: value => AllErrors(v: path(value), expectedCategories: expectedCategories));
    public static void OptionExclusive<T>(Gen<Option<T>> gen, Action<T> whenSome, Action whenNone) =>
        ForAll(gen: gen, property: opt => _ = opt.Match(Some: v => Apply(action: whenSome, value: v), None: () => Apply(action: _ => whenNone(), value: unit)));
    public static void EquivalenceClass<T, TClass>(Gen<T> gen, Func<T, TClass> classify, Func<T, T, bool> eq) where TClass : notnull =>
        ForAll(gen.Select(gen), ((T A, T B) p) => Holds(
            condition: EqualityComparer<TClass>.Default.Equals(classify(p.A), classify(p.B)) == eq(p.A, p.B),
            label: $"EquivalenceClass: classify mismatch for {p.A}, {p.B}"));
    public static void SmartEnumCatalogMatches<T, TKey>(IReadOnlyList<T> production, IReadOnlyList<TKey> expectedKeys, Func<T, TKey> key) where TKey : notnull {
        ArgumentNullException.ThrowIfNull(argument: expectedKeys);
        SmartEnumKeysUnique(items: production, key: key);
        TKey[] expected = [.. expectedKeys.OrderBy(static k => k)];
        TKey[] actual = [.. production.Select(key).OrderBy(static k => k)];
        Assert.Equal(expected: expected, actual: actual);
    }
    // expectedOutput is the spec's INDEPENDENT table, not production's per-case Output — the law cross-checks the two.
    public static void SmartEnumOutputCatalog<T, TKey>(IReadOnlyList<T> items, IReadOnlyList<TKey> expectedKeys, Func<T, TKey> key, Func<T, Type> output, Func<T, Type> expectedOutput) where TKey : notnull {
        ArgumentNullException.ThrowIfNull(argument: items);
        SmartEnumCatalogMatches(production: items, expectedKeys: expectedKeys, key: key);
        _ = items.AsIterable().Iter(item => { Cancel(); Assert.Equal(expected: expectedOutput(item), actual: output(item)); });
    }
    public static void ProjectionDispatch<TSelf, TValue>(IReadOnlyList<(TSelf Input, TValue Expected, string Label)> cases, Func<TSelf, Fin<TValue>> project, Func<TValue, TValue, bool>? eq = null) =>
        _ = cases.AsIterable().Iter(c => {
            Cancel();
            Succ(result: project(c.Input), then: actual => Holds(condition: (eq ?? EqualityComparer<TValue>.Default.Equals)(c.Expected, actual), label: c.Label));
        });
    // VO shape lives as a record so generators+validators+readers can be packaged together for Family iteration.
    public sealed record ValueObjectShape<TIn, TStruct>(Gen<TIn> Valid, Gen<TIn> Invalid, TryCreate<TIn, TStruct> TryCreate, Func<TStruct, TIn> Read, Func<TIn, TIn, bool>? Eq = null);
    public static void Family<TIn, TStruct>(params ValueObjectShape<TIn, TStruct>[] shapes) =>
        _ = shapes.AsIterable().Iter(s => {
            Cancel();
            ValueObjectRoundtrip(validGen: s.Valid, tryCreate: s.TryCreate, read: s.Read, eq: s.Eq);
            ValueObjectRejects(invalid: s.Invalid, tryCreate: value => s.TryCreate(value, out _));
        });
    public static void AcceptValidatedCategory<TVO>(Gen<double> invalidGen, string expectedCategory, Func<double, Fin<TVO>> create) =>
        ForAll(gen: invalidGen, property: x => FailCategory(result: create(x), category: expectedCategory));
    public sealed record OutputCase<TCase>(TCase Case, IReadOnlyList<Type> SupportedOuts, IReadOnlyList<Type> UnsupportedOuts);
    public static void OutputDispatchTable<TCase>(IReadOnlyList<OutputCase<TCase>> cases, Func<TCase, Type, Fin<object>> runProjection) =>
        _ = cases.AsIterable().Iter(c => {
            Cancel();
            _ = c.SupportedOuts.AsIterable().Iter(t => Succ(result: runProjection(c.Case, t)));
            _ = c.UnsupportedOuts.AsIterable().Iter(t => FailCategory(result: runProjection(c.Case, t), category: "Unsupported"));
        });
    public sealed record ValidityCase<T>(string Label, T Value, bool Expected);
    public static void ValidityMatrix<T>(IReadOnlyList<ValidityCase<T>> cases, Func<T, bool> valid) {
        ArgumentNullException.ThrowIfNull(argument: cases);
        ArgumentNullException.ThrowIfNull(argument: valid);
        _ = cases.AsIterable().Iter(c => {
            Cancel();
            bool actual = valid(arg: c.Value);
            Holds(condition: actual == c.Expected, label: $"{c.Label}: expected {c.Expected}, got {actual}");
        });
    }
    public static void ValidityMatrix<T>(IReadOnlyList<(string Label, T Value, bool Expected)> cases, Func<T, bool> valid) =>
        ValidityMatrix(cases: [.. cases.Select(static c => new ValidityCase<T>(Label: c.Label, Value: c.Value, Expected: c.Expected))], valid: valid);
    public static void OutputPartition<T>(IReadOnlyList<T> items, Func<T, Type> output, params (Type Output, int Count)[] expected) {
        ArgumentNullException.ThrowIfNull(argument: items);
        ArgumentNullException.ThrowIfNull(argument: output);
        (string Output, int Count)[] actual = [.. items.GroupBy(keySelector: output)
            .Select(static g => (Output: g.Key.FullName ?? g.Key.Name, Count: g.Count()))
            .OrderBy(static row => row.Output, StringComparer.Ordinal)];
        (string Output, int Count)[] want = [.. expected
            .Select(static row => (Output: row.Output.FullName ?? row.Output.Name, row.Count))
            .OrderBy(static row => row.Output, StringComparer.Ordinal)];
        Assert.Equal(expected: want, actual: actual);
    }
    public sealed record SupportOracleCase<TCase>(string Label, TCase Case, Type Geometry, Type Output, Func<bool> Probe);
    public static void SupportOracle<TCase>(IReadOnlyList<SupportOracleCase<TCase>> cases, Func<TCase, Type, Type, bool> expected) {
        ArgumentNullException.ThrowIfNull(argument: cases);
        ArgumentNullException.ThrowIfNull(argument: expected);
        _ = cases.AsIterable().Iter(c => {
            Cancel();
            bool actual = c.Probe();
            bool want = expected(arg1: c.Case, arg2: c.Geometry, arg3: c.Output);
            Holds(condition: actual == want, label: $"{c.Label}: expected support {want}, got {actual}");
        });
    }
    public sealed record ProjectionMatrixCase<TIntent>(string Label, TIntent Intent, Type SupportedOut, Func<object, bool> Oracle, Type UnsupportedOut);
    public static void ProjectionMatrix<TIntent>(IReadOnlyList<ProjectionMatrixCase<TIntent>> cases, Context context, Op key, Func<TIntent, Context, Op, Type, Fin<object>> project) =>
        _ = cases.AsIterable().Iter(c => {
            Cancel();
            Succ(result: project(c.Intent, context, key, c.SupportedOut), then: o => Holds(condition: c.Oracle(o), label: c.Label));
            FailCategory(result: project(c.Intent, context, key, c.UnsupportedOut), category: "Unsupported");
        });
    public static void MinPairwiseDistanceAtLeast(Seq<Point3d> points, double minDistance, string label = "MinPairwiseDistance") =>
        _ = toSeq(Enumerable.Range(start: 0, count: points.Count)
            .SelectMany(i => Enumerable.Range(start: i + 1, count: points.Count - i - 1).Select(j => (i, j))))
            .Iter(p => Holds(condition: points[index: p.i].DistanceTo(other: points[index: p.j]) >= minDistance - RhinoMath.ZeroTolerance,
                label: $"{label}[{p.i},{p.j}]: {points[index: p.i].DistanceTo(other: points[index: p.j]):R} < {minDistance:R}"));
    public static void Converged<TStop>(TStop actualStop, TStop expectedStop, int iterations, int maxIterations) {
        Assert.Equal(expected: expectedStop, actual: actualStop);
        Assert.InRange(actual: iterations, low: 1, high: maxIterations);
    }
    // Translation MR: f(translate(x, t)) == translate(f(x), t).
    public static void MetamorphicTranslation<T>(Gen<(T X, Vector3d Translation)> gen, Func<T, Vector3d, T> translate, Func<T, Point3d> evaluate, double tolerance = 1e-9) =>
        ForAll(gen: gen, property: p => Equal(left: evaluate(translate(p.X, p.Translation)), right: evaluate(p.X) + p.Translation, tolerance: tolerance, what: "MetamorphicTranslation"));
    public static void AdditiveIdentity<T>(Gen<T> gen, Func<T, T, T> op, T identity, Func<T, T, bool>? eq = null) =>
        ForAll(gen: gen, property: value => {
            _ = EqOrThrow(left: op(identity, value), right: value, predicate: eq);
            _ = EqOrThrow(left: op(value, identity), right: value, predicate: eq);
        });
    public static void FailUnsupportedFor<T>(Fin<T> result, Type geometryType, Type outputType) =>
        Fail(result: result, then: error => {
            Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
            Assert.Equal(expected: geometryType, actual: fault.GeometryType);
            Assert.Equal(expected: outputType, actual: fault.OutputType);
        });
    // Func<bool> thunks keep each row's generics at the call site and carry per-row diagnostics, replacing anonymous Assert.True/False walls.
    public static void SupportMatrix(params (string Label, Func<bool> Probe, bool Expected)[] rows) {
        ArgumentNullException.ThrowIfNull(argument: rows);
        _ = rows.AsIterable().Iter(row => {
            Cancel();
            bool actual = row.Probe();
            Holds(condition: actual == row.Expected, label: $"{row.Label}: expected {row.Expected}, got {actual}");
        });
    }
    // Wraps Check.SampleModelBased.
    public static void ModelBased<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Func<TActual, TModel, bool> equal, params GenOperation<TActual, TModel>[] operations) {
        Cancel();
        init.SampleModelBased(operations: operations, equal: equal);
    }
    // Wraps Check.SampleMetamorphic.
    public static void MetamorphicOps<T, TParam>(Gen<T> initial, Gen<TParam> paramGen, Func<TParam, string> name, Action<T, TParam> path1, Action<T, TParam> path2, Func<T, T, bool>? equal = null) {
        Cancel();
        initial.SampleMetamorphic(operations: GenMetamorphic.Create(gen: paramGen, name: name, action1: path1, action2: path2), equal: equal);
    }
    // Wraps Check.SampleParallel without Causal profiling output (vs ConcurrentProfiled).
    public static void Parallel<T>(Gen<T> init, params GenOperation<T>[] operations) {
        Cancel();
        init.SampleParallel(operations: operations);
    }

    // --- [BOUNDARY_ADAPTERS] -----------------------------------------------------------
    // Propagates xUnit v3 runner cancellation into long CsCheck sample loops.
    private static void Cancel() => TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
    // Action<T> -> CsCheck Func<T, bool> contract.
    private static bool Apply<T>(Action<T> action, T value) { action(value); return true; }
    // Optional Action<T> -> LanguageExt Match's Unit-returning lambda contract.
    private static Unit Tap<T>(Action<T>? action, T value) { action?.Invoke(value); return unit; }
    private static bool EqOrThrow<T>(T left, T right, Func<T, T, bool>? predicate) =>
        (predicate ?? EqualityComparer<T>.Default.Equals)(left, right) ? true : throw new XunitException($"Equality failed: {left} != {right}");
    // Generic dispatcher for ForAll-based equality laws — collapses Roundtrip/Identity/Idempotent/Inverse/Distributive/Linearity shapes.
    private static void EqLaw<TIn, TOut>(Gen<TIn> gen, Func<TIn, TOut> left, Func<TIn, TOut> right, Func<TOut, TOut, bool>? eq = null) =>
        ForAll(gen: gen, property: x => _ = EqOrThrow(left: left(x), right: right(x), predicate: eq));
    private static void Eq(bool ok, string label) => Holds(condition: ok, label: label);

    private readonly record struct SamplePolicy(string? Seed, long? Iter, int? Time, int? Threads) {
        public bool IsDefault => Seed is null && Iter is null && Time is null && Threads is null;
        public long IterOrDefault => Iter ?? Check.Iter;
        public int TimeOrDefault => Time ?? Check.Time;
        public int ThreadsOrDefault => Threads ?? Check.Threads;
        public static SamplePolicy Of(string? seed, long? iter, int? time, int? threads) =>
            new(Seed: seed ?? Environment.GetEnvironmentVariable(variable: "CsCheck_Seed"),
                Iter: iter ?? EnvLong(name: "CsCheck_Iter"),
                Time: time ?? EnvInt(name: "CsCheck_Time"),
                Threads: threads ?? EnvInt(name: "CsCheck_Threads"));
        private static long? EnvLong(string name) =>
            long.TryParse(s: Environment.GetEnvironmentVariable(variable: name), style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out long value) && value > 0L ? value : null;
        private static int? EnvInt(string name) =>
            int.TryParse(s: Environment.GetEnvironmentVariable(variable: name), style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out int value) && value > 0 ? value : null;
    }
}
