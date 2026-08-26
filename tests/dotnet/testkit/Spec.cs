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
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(gen);
        ArgumentNullException.ThrowIfNull(property);
        return new Law<T>(name, gen, property, refutingWitness);
    }
    public static Law<T> Identity<T>(string name, Gen<T> gen, Func<T, T> f, T witness, Func<T, T, bool>? eq = null) =>
        Of(name, gen, x => Eq(name, x, f(x), eq), witness);
    public static Law<T> Idempotent<T>(string name, Gen<T> gen, Func<T, T> f, T witness, Func<T, T, bool>? eq = null) =>
        Of(name, gen, x => Eq(name, f(x), f(f(x)), eq), witness);
    public static Law<T> Inverse<T>(string name, Gen<T> gen, Func<T, T> f, Func<T, T> g, T witness, Func<T, T, bool>? eq = null) =>
        Of(name, gen, x => Eq(name, x, g(f(x)), eq), witness);
    public static Law<TIn> Roundtrip<TIn, TOut>(string name, Gen<TIn> gen, Func<TIn, TOut> forward, Func<TOut, TIn> back, TIn witness, Func<TIn, TIn, bool>? eq = null) =>
        Of(name, gen, x => Eq(name, x, back(forward(x)), eq), witness);
    public static Law<(T A, T B)> Commutative<T, TResult>(string name, Gen<T> gen, Func<T, T, TResult> op, (T A, T B) witness, Func<TResult, TResult, bool>? eq = null) =>
        Of(name, gen.Select(gen, static (a, b) => (A: a, B: b)), p => Eq(name, op(p.A, p.B), op(p.B, p.A), eq), witness);
    public static Law<(T A, T B, T C)> Associative<T>(string name, Gen<T> gen, Func<T, T, T> op, (T A, T B, T C) witness, Func<T, T, bool>? eq = null) =>
        Of(name, Triples(gen), t => Eq(name, op(op(t.A, t.B), t.C), op(t.A, op(t.B, t.C)), eq), witness);
    public static Law<(T A, T B, T C)> Distributive<T>(string name, Gen<T> gen, Func<T, T, T> mul, Func<T, T, T> add, (T A, T B, T C) witness, Func<T, T, bool>? eq = null) =>
        Of(name, Triples(gen), t => Eq(name, mul(t.A, add(t.B, t.C)), add(mul(t.A, t.B), mul(t.A, t.C)), eq), witness);
    public static Law<(T Lo, T Hi)> Monotone<T, TKey>(string name, Gen<(T Lo, T Hi)> pairs, Func<T, TKey> projection, (T Lo, T Hi) witness, IComparer<TKey>? comparer = null) =>
        Of(name, pairs, p => Spec.Holds(
            (comparer ?? Comparer<TKey>.Default).Compare(projection(p.Lo), projection(p.Hi)) <= 0,
            $"{name}: f({p.Lo}) = {projection(p.Lo)} > {projection(p.Hi)} = f({p.Hi})"), witness);
    public static Law<(T[] Source, T[] Shuffled)> Permutation<T, TResult>(string name, Gen<T[]> gen, Func<T[], TResult> f, (T[] Source, T[] Shuffled) witness, Func<TResult, TResult, bool>? eq = null) =>
        Of(name, gen.SelectMany(arr => Gen.Shuffle(arr).Select(perm => (Source: arr, Shuffled: perm))), p => Eq(name, f(p.Source), f(p.Shuffled), eq), witness);

    private static Gen<(T A, T B, T C)> Triples<T>(Gen<T> gen) => gen.Select(gen, gen, static (a, b, c) => (A: a, B: b, C: c));
    private static void Eq<T>(string name, T left, T right, Func<T, T, bool>? eq) =>
        Spec.Holds((eq ?? EqualityComparer<T>.Default.Equals)(left, right), $"{name}: {left} != {right}");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class Spec {
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(gen);
        ArgumentNullException.ThrowIfNull(property);
        gen.Sample(value => { Cancel(); property(value); }, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    public static void Holds(bool condition, string label) =>
        _ = condition ? unit : throw new XunitException(label);
    public static void Refutes<T>(T witness, Action<T> law, string? name = null) {
        ArgumentNullException.ThrowIfNull(law);
        _ = Try.lift(() => { law(witness); return unit; }).Run().Match(
            Succ: _ => throw new XunitException($"'{name ?? "law"}' is a tautology — its refuting witness survives the property (witness={witness})"),
            Fail: static _ => unit);
    }
    public static void Hold<T>(Law<T> law, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(law);
        Refutes(law.RefutingWitness, law.Property, law.Name);
        ForAll(law.Gen, law.Property, seed, iter, time, threads);
    }
    public static void Hold<T>(params Law<T>[] laws) {
        NonEmpty(laws, "Hold: empty law table proves nothing");
        _ = laws.AsIterable().Iter(law => { Cancel(); Hold(law); });
    }
    public static void Replay<T>(Gen<T> gen, Action<T> property, string seed) => ForAll(gen, property, seed, iter: 1);
    public static void Replay<T>(Law<T> law, string seed) {
        ArgumentNullException.ThrowIfNull(law);
        Replay(law.Gen, law.Property, seed);
    }

    // --- [METAMORPHIC]
    public static void Metamorphic<T, TResult>(Gen<T> gen, Func<T, TResult> f, params MetamorphicRelation<T, TResult>[] relations) {
        ArgumentNullException.ThrowIfNull(f);
        NonEmpty(relations, "Metamorphic: empty relation table proves nothing");
        ForAll(gen, value => {
            TResult @base = f(value);
            _ = relations.AsIterable().Iter(relation => {
                TResult follow = f(relation.Transform(value));
                Holds(relation.Relate(value, @base, follow), $"Metamorphic '{relation.Name}': base={@base}, follow={follow}");
            });
        });
    }

    // --- [STATEFUL]
    public static void ModelBased<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Func<TActual, TModel, bool> equal, GenOperation<TActual, TModel>[] operations,
        string? seed = null, long? iter = null, int? time = null) {
        NonEmpty(operations, "ModelBased: empty operation table proves nothing");
        Cancel();
        init.SampleModelBased(operations, equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void DualPath<T, TParam>(Gen<T> initial, Gen<TParam> paramGen, Func<TParam, string> name, Action<T, TParam> path1, Action<T, TParam> path2,
        Func<T, T, bool>? equal = null, string? seed = null, long? iter = null, int? time = null) {
        Cancel();
        initial.SampleMetamorphic(GenMetamorphic.Create(paramGen, name, path1, path2), equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void DualPath<T, TResult>(Gen<T> gen, Func<T, TResult> subject, Func<T, TResult> reference, Func<TResult, TResult, bool>? eq = null,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(reference);
        ForAll(gen, value => {
            (TResult actual, TResult expected) = (subject(value), reference(value));
            Holds((eq ?? EqualityComparer<TResult>.Default.Equals)(actual, expected), $"DualPath diverged: subject={actual}, reference={expected}");
        }, seed, iter, time);
    }
    public static void DualPath<T>(Gen<T> gen, Func<T, double> subject, Func<T, double> reference, Tolerance tolerance, Metric? metric = null,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(reference);
        ForAll(gen, value => Equal(subject(value), reference(value), tolerance, metric, what: "DualPath"), seed, iter, time);
    }
    public static void Parallel<T>(Gen<T> init, GenOperation<T>[] operations, string? seed = null, long? iter = null, int? time = null) {
        NonEmpty(operations, "Parallel: empty operation table proves nothing");
        Cancel();
        init.SampleParallel(operations, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void Parallel<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Func<TActual, TModel, bool> equal, GenOperation<TActual, TModel>[] operations,
        string? seed = null, long? iter = null, int? time = null) {
        NonEmpty(operations, "Parallel: empty operation table proves nothing");
        Cancel();
        init.SampleParallel(operations, equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }

    // --- [RESULT_GATES]
    public static T SuccValue<T>(Fin<T> result, string label) {
        ArgumentNullException.ThrowIfNull(result);
        return result.Match(Succ: static value => value, Fail: error => throw new XunitException($"{label}: expected Succ; got Fail: {error.Message}"));
    }
    public static void Succ<T>(Fin<T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Succ: value => Tap(then, value), Fail: error => throw new XunitException($"Expected Succ; got Fail: {error.Message}"));
    }
    public static void Fail<T>(Fin<T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Succ: value => throw new XunitException($"Expected Fail; got Succ: {value}"), Fail: error => Tap(then, error));
    }
    public static void Valid<T>(Validation<Error, T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Succ: value => Tap(then, value), Fail: error => throw new XunitException($"Expected Valid; got Invalid: {error.Message}"));
    }
    public static void Invalid<T>(Validation<Error, T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Succ: value => throw new XunitException($"Expected Invalid; got Valid: {value}"), Fail: error => Tap(then, error));
    }
    public static void Some<T>(Option<T> result, Action<T>? then = null) =>
        _ = result.Match(Some: value => Tap(then, value), None: () => throw new XunitException("Expected Some; got None"));
    public static void None<T>(Option<T> result) =>
        _ = result.Match(Some: value => throw new XunitException($"Expected None; got Some: {value}"), None: static () => unit);

    // --- [DISTRIBUTION]
    public static void Classified<T>(Gen<T> gen, Func<T, string> classify, Action<string> writeLine, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(gen);
        ArgumentNullException.ThrowIfNull(classify);
        ArgumentNullException.ThrowIfNull(writeLine);
        gen.Sample(classify, writeLine, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    public static void Distributed<T>(Gen<T> gen, Func<T, int> bucket, params int[] expected) {
        ArgumentNullException.ThrowIfNull(bucket);
        ArgumentNullException.ThrowIfNull(expected);
        Holds(expected.Length > 1, "Distributed: a one-bucket expectation proves nothing");
        int[] actual = new int[expected.Length];
        ForAll(gen, value => Interlocked.Increment(ref actual[bucket(value)]), iter: expected.Sum(), threads: 1);
        Check.ChiSquared(expected, actual);
    }

    // --- [BYTE_IDENTITY]
    public static void RoundtripBytes<T>(Gen<T> gen, JsonTypeInfo<T> contract, string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(contract);
        ForAll(gen, value => {
            byte[] raw = JsonSerializer.SerializeToUtf8Bytes(value, contract);
            T decoded = JsonSerializer.Deserialize(raw, contract) ?? throw new XunitException($"RoundtripBytes decoded null for {typeof(T).Name}");
            Holds(raw.AsSpan().SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(decoded, contract)), $"RoundtripBytes not byte-identical for {typeof(T).Name}");
        }, seed, iter, time);
    }

    // --- [CASE_TABLES]
    public static void Catalog<T, TKey>(IReadOnlyList<T> items, IReadOnlyList<TKey> expectedKeys, Func<T, TKey> key, Action<T>? law = null) where TKey : notnull {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(expectedKeys);
        ArgumentNullException.ThrowIfNull(key);
        Assert.Equal(items.Count, items.Select(key).ToHashSet().Count);
        Assert.Equal([.. expectedKeys.Order()], (TKey[])[.. items.Select(key).Order()]);
        _ = items.AsIterable().Iter(item => { Cancel(); law?.Invoke(item); });
    }
    public static void Matrix(params (string Label, Func<bool> Probe, bool Expected)[] rows) {
        NonEmpty(rows, "Matrix: empty row table proves nothing");
        _ = rows.AsIterable().Iter(row => {
            Cancel();
            bool actual = row.Probe();
            Holds(actual == row.Expected, $"{row.Label}: expected {row.Expected}, got {actual}");
        });
    }

    // --- [VALUE_OBJECTS]
    public sealed record ValueObjectShape<TIn, TStruct>(Gen<TIn> Valid, Gen<TIn> Invalid, TryCreate<TIn, TStruct> TryCreate, Func<TStruct, TIn> Read, Func<TIn, TIn, bool>? Eq = null);
    public static void Family<TIn, TStruct>(params ValueObjectShape<TIn, TStruct>[] shapes) {
        NonEmpty(shapes, "Family: empty shape table proves nothing");
        _ = shapes.AsIterable().Iter(s => {
            Cancel();
            ForAll(s.Valid, x => {
                Holds(s.TryCreate(x, out TStruct owned), $"Family: valid input rejected for {typeof(TStruct).Name}: {x}");
                Holds((s.Eq ?? EqualityComparer<TIn>.Default.Equals)(x, s.Read(owned)), $"Family: roundtrip drift for {typeof(TStruct).Name}: {x}");
            });
            ForAll(s.Invalid, x => Holds(!s.TryCreate(x, out _), $"Family: invalid input admitted for {typeof(TStruct).Name}: {x}"));
        });
    }

    // --- [BOUNDARY_ADAPTERS]
    private static void NonEmpty<T>(T[] table, string label) {
        ArgumentNullException.ThrowIfNull(table);
        Holds(table.Length > 0, label);
    }
    private static void Cancel() => TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
    private static Unit Tap<T>(Action<T>? action, T value) { action?.Invoke(value); return unit; }
}
