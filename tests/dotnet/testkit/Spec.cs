using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate bool TryCreate<TIn, TOut>(TIn value, out TOut obj);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MetamorphicRelation<T, TResult>(string Name, Func<T, T> Transform, Func<T, TResult, TResult, bool> Relate);

public sealed record Law<T>(string Name, Gen<T> Gen, Action<T> Property, T RefutingWitness);

public static class Law {
    public static Law<T> Of<T>(string name, Gen<T> gen, Action<T> property, T refutingWitness) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: name);
        ArgumentNullException.ThrowIfNull(argument: gen);
        ArgumentNullException.ThrowIfNull(argument: property);
        return new Law<T>(Name: name, Gen: gen, Property: property, RefutingWitness: refutingWitness);
    }
    public static Law<T> Identity<T>(string name, Gen<T> gen, Func<T, T> f, T witness, Func<T, T, bool>? eq = null) =>
        Of(name: name, gen: gen, property: x => Eq(name: name, left: x, right: f(x), eq: eq), refutingWitness: witness);
    public static Law<T> Idempotent<T>(string name, Gen<T> gen, Func<T, T> f, T witness, Func<T, T, bool>? eq = null) =>
        Of(name: name, gen: gen, property: x => Eq(name: name, left: f(x), right: f(f(x)), eq: eq), refutingWitness: witness);
    public static Law<T> Inverse<T>(string name, Gen<T> gen, Func<T, T> f, Func<T, T> g, T witness, Func<T, T, bool>? eq = null) =>
        Of(name: name, gen: gen, property: x => Eq(name: name, left: x, right: g(f(x)), eq: eq), refutingWitness: witness);
    public static Law<TIn> Roundtrip<TIn, TOut>(string name, Gen<TIn> gen, Func<TIn, TOut> forward, Func<TOut, TIn> back, TIn witness, Func<TIn, TIn, bool>? eq = null) =>
        Of(name: name, gen: gen, property: x => Eq(name: name, left: x, right: back(forward(x)), eq: eq), refutingWitness: witness);
    public static Law<(T A, T B)> Commutative<T, TResult>(string name, Gen<T> gen, Func<T, T, TResult> op, (T A, T B) witness, Func<TResult, TResult, bool>? eq = null) =>
        Of(name: name, gen: gen.Select(gen, static (T a, T b) => (A: a, B: b)),
           property: p => Eq(name: name, left: op(p.A, p.B), right: op(p.B, p.A), eq: eq), refutingWitness: witness);
    public static Law<(T A, T B, T C)> Associative<T>(string name, Gen<T> gen, Func<T, T, T> op, (T A, T B, T C) witness, Func<T, T, bool>? eq = null) =>
        Of(name: name, gen: gen.Select(gen, gen, static (T a, T b, T c) => (A: a, B: b, C: c)),
           property: t => Eq(name: name, left: op(op(t.A, t.B), t.C), right: op(t.A, op(t.B, t.C)), eq: eq), refutingWitness: witness);
    public static Law<(T A, T B, T C)> Distributive<T>(string name, Gen<T> gen, Func<T, T, T> mul, Func<T, T, T> add, (T A, T B, T C) witness, Func<T, T, bool>? eq = null) =>
        Of(name: name, gen: gen.Select(gen, gen, static (T a, T b, T c) => (A: a, B: b, C: c)),
           property: t => Eq(name: name, left: mul(t.A, add(t.B, t.C)), right: add(mul(t.A, t.B), mul(t.A, t.C)), eq: eq), refutingWitness: witness);
    public static Law<(T Lo, T Hi)> Monotone<T, TKey>(string name, Gen<(T Lo, T Hi)> pairs, Func<T, TKey> projection, (T Lo, T Hi) witness, IComparer<TKey>? comparer = null) =>
        Of(name: name, gen: pairs, property: p => Spec.Holds(
            condition: (comparer ?? Comparer<TKey>.Default).Compare(x: projection(p.Lo), y: projection(p.Hi)) <= 0,
            label: $"{name}: f({p.Lo}) = {projection(p.Lo)} > {projection(p.Hi)} = f({p.Hi})"), refutingWitness: witness);
    public static Law<(T[] Source, T[] Shuffled)> Permutation<T, TResult>(string name, Gen<T[]> gen, Func<T[], TResult> f, (T[] Source, T[] Shuffled) witness, Func<TResult, TResult, bool>? eq = null) =>
        Of(name: name, gen: gen.SelectMany(arr => Gen.Shuffle(arr).Select(perm => (Source: arr, Shuffled: perm))),
           property: p => Eq(name: name, left: f(p.Source), right: f(p.Shuffled), eq: eq), refutingWitness: witness);
    private static void Eq<T>(string name, T left, T right, Func<T, T, bool>? eq) =>
        _ = (eq ?? EqualityComparer<T>.Default.Equals)(left, right) ? true : throw new XunitException($"{name}: {left} != {right}");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class Spec {
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(argument: gen);
        ArgumentNullException.ThrowIfNull(argument: property);
        gen.Sample(assert: value => { Cancel(); property(value); }, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    public static void Holds(bool condition, string label) =>
        _ = condition ? unit : throw new XunitException(userMessage: label);
    public static void Refutes<T>(T witness, Action<T> law, string? name = null) {
        ArgumentNullException.ThrowIfNull(argument: law);
        _ = Try.lift(f: () => { law(witness); return unit; }).Run().Match(
            Succ: _ => throw new XunitException($"'{name ?? "law"}' is a tautology — its refuting witness survives the property (witness={witness})"),
            Fail: static _ => unit);
    }
    public static void Hold<T>(Law<T> law, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(argument: law);
        Refutes(witness: law.RefutingWitness, law: law.Property, name: law.Name);
        ForAll(gen: law.Gen, property: law.Property, seed: seed, iter: iter, time: time, threads: threads);
    }
    public static void Hold<T>(params Law<T>[] laws) {
        ArgumentNullException.ThrowIfNull(argument: laws);
        Holds(condition: laws.Length > 0, label: "Hold: empty law table proves nothing");
        _ = laws.AsIterable().Iter(law => { Cancel(); Hold(law: law); });
    }
    public static void Replay<T>(Gen<T> gen, Action<T> property, string seed) =>
        ForAll(gen: gen, property: property, seed: seed, iter: 1);
    public static void Replay<T>(Law<T> law, string seed) {
        ArgumentNullException.ThrowIfNull(argument: law);
        Replay(gen: law.Gen, property: law.Property, seed: seed);
    }

    // --- [METAMORPHIC]
    public static void Metamorphic<T, TResult>(Gen<T> gen, Func<T, TResult> f, params MetamorphicRelation<T, TResult>[] relations) {
        ArgumentNullException.ThrowIfNull(argument: f);
        ArgumentNullException.ThrowIfNull(argument: relations);
        Holds(condition: relations.Length > 0, label: "Metamorphic: empty relation table proves nothing");
        ForAll(gen: gen, property: value => {
            TResult @base = f(value);
            _ = relations.AsIterable().Iter(relation => {
                TResult follow = f(relation.Transform(value));
                Holds(condition: relation.Relate(value, @base, follow), label: $"Metamorphic '{relation.Name}': base={@base}, follow={follow}");
            });
        });
    }

    // --- [STATEFUL]
    public static void ModelBased<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Func<TActual, TModel, bool> equal, GenOperation<TActual, TModel>[] operations,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: operations);
        Holds(condition: operations.Length > 0, label: "ModelBased: empty operation table proves nothing");
        Cancel();
        init.SampleModelBased(operations: operations, equal: equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void DualPath<T, TParam>(Gen<T> initial, Gen<TParam> paramGen, Func<TParam, string> name, Action<T, TParam> path1, Action<T, TParam> path2,
        Func<T, T, bool>? equal = null, string? seed = null, long? iter = null, int? time = null) {
        Cancel();
        initial.SampleMetamorphic(operations: GenMetamorphic.Create(gen: paramGen, name: name, action1: path1, action2: path2), equal: equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void DualPath<T, TResult>(Gen<T> gen, Func<T, TResult> subject, Func<T, TResult> reference, Func<TResult, TResult, bool>? eq = null,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: subject);
        ArgumentNullException.ThrowIfNull(argument: reference);
        ForAll(gen: gen, property: value => {
            (TResult actual, TResult expected) = (subject(value), reference(value));
            Holds(condition: (eq ?? EqualityComparer<TResult>.Default.Equals)(actual, expected), label: $"DualPath diverged: subject={actual}, reference={expected}");
        }, seed: seed, iter: iter, time: time);
    }
    public static void DualPath<T>(Gen<T> gen, Func<T, double> subject, Func<T, double> reference, Tolerance tolerance, Metric? metric = null,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: subject);
        ArgumentNullException.ThrowIfNull(argument: reference);
        ForAll(gen: gen, property: value =>
            Equal(left: subject(value), right: reference(value), tolerance: tolerance, metric: metric, what: "DualPath"), seed: seed, iter: iter, time: time);
    }
    public static void Parallel<T>(Gen<T> init, GenOperation<T>[] operations, string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: operations);
        Holds(condition: operations.Length > 0, label: "Parallel: empty operation table proves nothing");
        Cancel();
        init.SampleParallel(operations: operations, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void Parallel<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Func<TActual, TModel, bool> equal, GenOperation<TActual, TModel>[] operations,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: operations);
        Holds(condition: operations.Length > 0, label: "Parallel: empty operation table proves nothing");
        Cancel();
        init.SampleParallel(operations: operations, equal: equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }

    // --- [RAIL_GATES]
    public static T SuccValue<T>(Fin<T> result, string label) {
        ArgumentNullException.ThrowIfNull(argument: result);
        return result.Match(Succ: static value => value, Fail: error => throw new XunitException($"{label}: expected Succ; got Fail: {error.Message}"));
    }
    public static void Succ<T>(Fin<T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(argument: result);
        _ = result.Match(Succ: value => Tap(action: then, value: value), Fail: error => throw new XunitException($"Expected Succ; got Fail: {error.Message}"));
    }
    public static void Fail<T>(Fin<T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(argument: result);
        _ = result.Match(Succ: value => throw new XunitException($"Expected Fail; got Succ: {value}"), Fail: error => Tap(action: then, value: error));
    }
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
    // --- [DISTRIBUTION]
    public static void Classified<T>(Gen<T> gen, Func<T, string> classify, Action<string> writeLine, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(argument: gen);
        ArgumentNullException.ThrowIfNull(argument: classify);
        ArgumentNullException.ThrowIfNull(argument: writeLine);
        gen.Sample(classify: classify, writeLine: writeLine, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    public static void Distributed<T>(Gen<T> gen, Func<T, int> bucket, params int[] expected) {
        ArgumentNullException.ThrowIfNull(argument: bucket);
        ArgumentNullException.ThrowIfNull(argument: expected);
        Holds(condition: expected.Length > 1, label: "Distributed: a one-bucket expectation proves nothing");
        int[] actual = new int[expected.Length];
        ForAll(gen: gen, property: value => Interlocked.Increment(location: ref actual[bucket(value)]), iter: expected.Sum(), threads: 1);
        Check.ChiSquared(expected: expected, actual: actual);
    }

    // --- [BYTE_IDENTITY]
    public static void RoundtripBytes<T>(Gen<T> gen, JsonTypeInfo<T> contract, string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: contract);
        ForAll(gen: gen, property: value => {
            byte[] raw = JsonSerializer.SerializeToUtf8Bytes(value: value, jsonTypeInfo: contract);
            T decoded = JsonSerializer.Deserialize(utf8Json: raw, jsonTypeInfo: contract) ?? throw new XunitException($"RoundtripBytes decoded null for {typeof(T).Name}");
            byte[] reencoded = JsonSerializer.SerializeToUtf8Bytes(value: decoded, jsonTypeInfo: contract);
            Holds(condition: raw.AsSpan().SequenceEqual(reencoded), label: $"RoundtripBytes not byte-identical for {typeof(T).Name}");
        }, seed: seed, iter: iter, time: time);
    }
    // --- [CASE_TABLES]
    public static void Catalog<T, TKey>(IReadOnlyList<T> items, IReadOnlyList<TKey> expectedKeys, Func<T, TKey> key, Action<T>? law = null) where TKey : notnull {
        ArgumentNullException.ThrowIfNull(argument: items);
        ArgumentNullException.ThrowIfNull(argument: expectedKeys);
        ArgumentNullException.ThrowIfNull(argument: key);
        Assert.Equal(expected: items.Count, actual: items.Select(selector: key).ToHashSet().Count);
        Assert.Equal(expected: [.. expectedKeys.Order()], actual: (TKey[])[.. items.Select(selector: key).Order()]);
        _ = items.AsIterable().Iter(item => { Cancel(); law?.Invoke(item); });
    }
    public static void Matrix(params (string Label, Func<bool> Probe, bool Expected)[] rows) {
        ArgumentNullException.ThrowIfNull(argument: rows);
        Holds(condition: rows.Length > 0, label: "Matrix: empty row table proves nothing");
        _ = rows.AsIterable().Iter(row => {
            Cancel();
            bool actual = row.Probe();
            Holds(condition: actual == row.Expected, label: $"{row.Label}: expected {row.Expected}, got {actual}");
        });
    }

    // --- [VALUE_OBJECTS]
    public sealed record ValueObjectShape<TIn, TStruct>(Gen<TIn> Valid, Gen<TIn> Invalid, TryCreate<TIn, TStruct> TryCreate, Func<TStruct, TIn> Read, Func<TIn, TIn, bool>? Eq = null);
    public static void Family<TIn, TStruct>(params ValueObjectShape<TIn, TStruct>[] shapes) {
        ArgumentNullException.ThrowIfNull(argument: shapes);
        Holds(condition: shapes.Length > 0, label: "Family: empty shape table proves nothing");
        _ = shapes.AsIterable().Iter(s => {
            Cancel();
            ForAll(gen: s.Valid, property: x => {
                Holds(condition: s.TryCreate(x, out TStruct owned), label: $"Family: valid input rejected for {typeof(TStruct).Name}: {x}");
                Holds(condition: (s.Eq ?? EqualityComparer<TIn>.Default.Equals)(x, s.Read(owned)), label: $"Family: roundtrip drift for {typeof(TStruct).Name}: {x}");
            });
            ForAll(gen: s.Invalid, property: x => Holds(condition: !s.TryCreate(x, out _), label: $"Family: invalid input admitted for {typeof(TStruct).Name}: {x}"));
        });
    }

    // --- [BOUNDARY_ADAPTERS]
    private static void Cancel() => TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
    private static Unit Tap<T>(Action<T>? action, T value) { action?.Invoke(value); return unit; }
}
