using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Xunit.Sdk;

namespace Rasm.TestSupport;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate bool TryCreate<TIn, TOut>(TIn value, out TOut obj);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MetamorphicRelation<T, TResult>(string Name, Func<T, T> Transform, Func<T, TResult, TResult, bool> Relate);

public sealed record PropertyDefinition<T>(string Name, Gen<T> Generator, Action<T> Property, T Counterexample);

public static class Properties {
    public static PropertyDefinition<T> Define<T>(string name, Gen<T> generator, Action<T> property, T counterexample) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(property);
        return new PropertyDefinition<T>(name, generator, property, counterexample);
    }
    public static PropertyDefinition<T> Identity<T>(string name, Gen<T> generator, Func<T, T> function, T counterexample, Func<T, T, bool>? equal = null) =>
        Define(name, generator, value => Equal(name, value, function(value), equal), counterexample);
    public static PropertyDefinition<T> Idempotent<T>(string name, Gen<T> generator, Func<T, T> function, T counterexample, Func<T, T, bool>? equal = null) =>
        Define(name, generator, value => Equal(name, function(value), function(function(value)), equal), counterexample);
    public static PropertyDefinition<T> Inverse<T>(string name, Gen<T> generator, Func<T, T> forward, Func<T, T> inverse, T counterexample, Func<T, T, bool>? equal = null) =>
        Define(name, generator, value => Equal(name, value, inverse(forward(value)), equal), counterexample);
    public static PropertyDefinition<TIn> Roundtrip<TIn, TOut>(string name, Gen<TIn> generator, Func<TIn, TOut> encode, Func<TOut, TIn> decode, TIn counterexample, Func<TIn, TIn, bool>? equal = null) =>
        Define(name, generator, value => Equal(name, value, decode(encode(value)), equal), counterexample);
    public static PropertyDefinition<(T A, T B)> Commutative<T, TResult>(string name, Gen<T> generator, Func<T, T, TResult> operation, (T A, T B) counterexample, Func<TResult, TResult, bool>? equal = null) =>
        Define(name, generator.Select(generator, static (a, b) => (A: a, B: b)), pair => Equal(name, operation(pair.A, pair.B), operation(pair.B, pair.A), equal), counterexample);
    public static PropertyDefinition<(T A, T B, T C)> Associative<T>(string name, Gen<T> generator, Func<T, T, T> operation, (T A, T B, T C) counterexample, Func<T, T, bool>? equal = null) =>
        Define(name, Triples(generator), triple => Equal(name, operation(operation(triple.A, triple.B), triple.C), operation(triple.A, operation(triple.B, triple.C)), equal), counterexample);
    public static PropertyDefinition<(T A, T B, T C)> Distributive<T>(string name, Gen<T> generator, Func<T, T, T> multiply, Func<T, T, T> add, (T A, T B, T C) counterexample, Func<T, T, bool>? equal = null) =>
        Define(name, Triples(generator), triple => Equal(name, multiply(triple.A, add(triple.B, triple.C)), add(multiply(triple.A, triple.B), multiply(triple.A, triple.C)), equal), counterexample);
    public static PropertyDefinition<(T Lower, T Upper)> Monotone<T, TKey>(string name, Gen<(T Lower, T Upper)> pairs, Func<T, TKey> projection, (T Lower, T Upper) counterexample, IComparer<TKey>? comparer = null) =>
        Define(name, pairs, pair => TestAssertions.True(
            (comparer ?? Comparer<TKey>.Default).Compare(projection(pair.Lower), projection(pair.Upper)) <= 0,
            $"{name}: f({pair.Lower}) = {projection(pair.Lower)} > {projection(pair.Upper)} = f({pair.Upper})"), counterexample);
    public static PropertyDefinition<(T[] Source, T[] Shuffled)> Permutation<T, TResult>(string name, Gen<T[]> generator, Func<T[], TResult> function, (T[] Source, T[] Shuffled) counterexample, Func<TResult, TResult, bool>? equal = null) =>
        Define(name, generator.SelectMany(static array => Gen.Shuffle(array).Select(shuffled => (Source: array, Shuffled: shuffled))), pair => Equal(name, function(pair.Source), function(pair.Shuffled), equal), counterexample);

    private static Gen<(T A, T B, T C)> Triples<T>(Gen<T> gen) => gen.Select(gen, gen, static (a, b, c) => (A: a, B: b, C: c));
    private static void Equal<T>(string name, T left, T right, Func<T, T, bool>? equal) =>
        TestAssertions.True((equal ?? EqualityComparer<T>.Default.Equals)(left, right), $"{name}: {left} != {right}");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class TestAssertions {
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(gen);
        ArgumentNullException.ThrowIfNull(property);
        gen.Sample(value => { Cancel(); property(value); }, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    public static void True(bool condition, string label) =>
        _ = condition ? unit : throw new XunitException(label);
    public static void RejectsCounterexample<T>(T counterexample, Action<T> property, string? name = null) {
        ArgumentNullException.ThrowIfNull(property);
        _ = Try.lift(() => { property(counterexample); return unit; }).Run().Match(
            Succ: _ => throw new XunitException($"Property '{name ?? "unnamed"}' accepts its counterexample ({counterexample})"),
            Fail: static _ => unit);
    }
    public static void Verify<T>(PropertyDefinition<T> definition, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(definition);
        RejectsCounterexample(definition.Counterexample, definition.Property, definition.Name);
        ForAll(definition.Generator, definition.Property, seed, iter, time, threads);
    }
    public static void Verify<T>(params PropertyDefinition<T>[] definitions) {
        NonEmpty(definitions, "Verify requires at least one property definition");
        _ = definitions.AsIterable().Iter(static definition => { Cancel(); Verify(definition); });
    }
    public static void Replay<T>(Gen<T> gen, Action<T> property, string seed) => ForAll(gen, property, seed, iter: 1);
    public static void Replay<T>(PropertyDefinition<T> definition, string seed) {
        ArgumentNullException.ThrowIfNull(definition);
        Replay(definition.Generator, definition.Property, seed);
    }

    // --- [METAMORPHIC]
    public static void Metamorphic<T, TResult>(Gen<T> gen, Func<T, TResult> f, params MetamorphicRelation<T, TResult>[] relations) {
        ArgumentNullException.ThrowIfNull(f);
        NonEmpty(relations, "Metamorphic requires at least one relation");
        ForAll(gen, value => {
            TResult @base = f(value);
            _ = relations.AsIterable().Iter(relation => {
                TResult follow = f(relation.Transform(value));
                True(relation.Relate(value, @base, follow), $"Metamorphic relation '{relation.Name}' failed: source={@base}, transformed={follow}");
            });
        });
    }

    // --- [STATEFUL]
    public static void ModelBased<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Func<TActual, TModel, bool> equal, GenOperation<TActual, TModel>[] operations,
        string? seed = null, long? iter = null, int? time = null) {
        NonEmpty(operations, "ModelBased requires at least one operation");
        Cancel();
        init.SampleModelBased(operations, equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void Differential<T, TParam>(Gen<T> initial, Gen<TParam> parameters, Func<TParam, string> name, Action<T, TParam> implementation, Action<T, TParam> reference,
        Func<T, T, bool>? equal = null, string? seed = null, long? iter = null, int? time = null) {
        Cancel();
        initial.SampleMetamorphic(GenMetamorphic.Create(parameters, name, implementation, reference), equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void Differential<T, TResult>(Gen<T> generator, Func<T, TResult> implementation, Func<T, TResult> reference, Func<TResult, TResult, bool>? equal = null,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(implementation);
        ArgumentNullException.ThrowIfNull(reference);
        ForAll(generator, value => {
            (TResult actual, TResult expected) = (implementation(value), reference(value));
            True((equal ?? EqualityComparer<TResult>.Default.Equals)(actual, expected), $"Differential test failed: implementation={actual}, reference={expected}");
        }, seed, iter, time);
    }
    public static void Differential<T>(Gen<T> generator, Func<T, double> implementation, Func<T, double> reference, Tolerance tolerance, NumericComparison? comparison = null,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(implementation);
        ArgumentNullException.ThrowIfNull(reference);
        ForAll(generator, value => Equal(implementation(value), reference(value), tolerance, comparison, label: "Differential"), seed, iter, time);
    }
    public static void Parallel<T>(Gen<T> init, GenOperation<T>[] operations, string? seed = null, long? iter = null, int? time = null) {
        NonEmpty(operations, "Parallel requires at least one operation");
        Cancel();
        init.SampleParallel(operations, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    public static void Parallel<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Func<TActual, TModel, bool> equal, GenOperation<TActual, TModel>[] operations,
        string? seed = null, long? iter = null, int? time = null) {
        NonEmpty(operations, "Parallel requires at least one operation");
        Cancel();
        init.SampleParallel(operations, equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }

    // --- [RESULT_ASSERTIONS]
    public static T SuccValue<T>(Fin<T> result, string label) {
        ArgumentNullException.ThrowIfNull(result);
        return result.Match(Succ: static value => value, Fail: error => throw new XunitException($"{label}: expected Succ; got Fail: {error.Message}"));
    }
    public static void Succ<T>(Fin<T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Succ: value => Tap(then, value), Fail: static error => throw new XunitException($"Expected Succ; got Fail: {error.Message}"));
    }
    public static void Fail<T>(Fin<T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Succ: static value => throw new XunitException($"Expected Fail; got Succ: {value}"), Fail: error => Tap(then, error));
    }
    public static void Valid<T>(Validation<Error, T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Fail: static error => throw new XunitException($"Expected Valid; got Invalid: {error.Message}"), Succ: value => Tap(then, value));
    }
    public static void Invalid<T>(Validation<Error, T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Fail: error => Tap(then, error), Succ: static value => throw new XunitException($"Expected Invalid; got Valid: {value}"));
    }
    public static void Some<T>(Option<T> result, Action<T>? then = null) =>
        _ = result.Match(Some: value => Tap(then, value), None: static () => throw new XunitException("Expected Some; got None"));
    public static void None<T>(Option<T> result) =>
        _ = result.Match(Some: static value => throw new XunitException($"Expected None; got Some: {value}"), None: static () => unit);

    // --- [DISTRIBUTION]
    public static void Classified<T>(Gen<T> gen, Func<T, string> classify, Action<string> writeLine, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(gen);
        ArgumentNullException.ThrowIfNull(classify);
        ArgumentNullException.ThrowIfNull(writeLine);
        gen.Sample(classify, writeLine, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    public static void ChiSquaredDistribution<T>(Gen<T> gen, Func<T, int> bucket, params int[] expected) {
        ArgumentNullException.ThrowIfNull(bucket);
        ArgumentNullException.ThrowIfNull(expected);
        True(expected.Length > 1, "ChiSquaredDistribution requires at least two expected buckets");
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
            True(raw.AsSpan().SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(decoded, contract)), $"RoundtripBytes not byte-identical for {typeof(T).Name}");
        }, seed, iter, time);
    }

    // --- [CASE_TABLES]
    public static void KeySet<T, TKey>(IReadOnlyList<T> items, IReadOnlyList<TKey> expectedKeys, Func<T, TKey> key, Action<T>? assertion = null) where TKey : notnull {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(expectedKeys);
        ArgumentNullException.ThrowIfNull(key);
        Assert.Equal(items.Count, items.Select(key).ToHashSet().Count);
        Assert.Equal([.. expectedKeys.Order()], (TKey[])[.. items.Select(key).Order()]);
        _ = items.AsIterable().Iter(item => { Cancel(); assertion?.Invoke(item); });
    }
    public static void CaseTable(params (string Label, Func<bool> Evaluate, bool Expected)[] rows) {
        NonEmpty(rows, "CaseTable requires at least one row");
        _ = rows.AsIterable().Iter(static row => {
            Cancel();
            bool actual = row.Evaluate();
            True(actual == row.Expected, $"{row.Label}: expected {row.Expected}, got {actual}");
        });
    }

    // --- [VALUE_OBJECTS]
    public sealed record ValueObjectCase<TIn, TStruct>(Gen<TIn> Valid, Gen<TIn> Invalid, TryCreate<TIn, TStruct> TryCreate, Func<TStruct, TIn> Read, Func<TIn, TIn, bool>? Equal = null);
    public static void ValueObjects<TIn, TStruct>(params ValueObjectCase<TIn, TStruct>[] cases) {
        NonEmpty(cases, "ValueObjects requires at least one case");
        _ = cases.AsIterable().Iter(static testCase => {
            Cancel();
            ForAll(testCase.Valid, value => {
                True(testCase.TryCreate(value, out TStruct constructed), $"Valid input rejected for {typeof(TStruct).Name}: {value}");
                True((testCase.Equal ?? EqualityComparer<TIn>.Default.Equals)(value, testCase.Read(constructed)), $"Round-trip mismatch for {typeof(TStruct).Name}: {value}");
            });
            ForAll(testCase.Invalid, value => True(!testCase.TryCreate(value, out _), $"Invalid input accepted for {typeof(TStruct).Name}: {value}"));
        });
    }

    // --- [BOUNDARY_ADAPTERS]
    private static void NonEmpty<T>(T[] table, string label) {
        ArgumentNullException.ThrowIfNull(table);
        True(table.Length > 0, label);
    }
    private static void Cancel() => TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
    private static Unit Tap<T>(Action<T>? action, T value) { action?.Invoke(value); return unit; }
}
