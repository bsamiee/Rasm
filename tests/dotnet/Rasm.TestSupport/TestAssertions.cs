using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Xunit.Sdk;

namespace Rasm.TestSupport;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate bool TryCreate<TIn, TOut>(TIn value, out TOut obj);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MetamorphicRelation<T, TResult>(string Name, Func<T, T> Transform, Func<T, TResult, TResult, bool> Relate);

// A predicate carries the counterexample it must reject, and a law compares two evaluations and needs none
public sealed record PropertyDefinition<T>(string Name, Gen<T> Generator, Action<T> Property, Option<T> Counterexample);

public static class Properties {
    public static PropertyDefinition<T> Define<T>(string name, Gen<T> generator, Action<T> property, T counterexample) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(property);
        return new PropertyDefinition<T>(name, generator, property, Some(counterexample));
    }
    public static PropertyDefinition<T> Identity<T>(string name, Gen<T> generator, Func<T, T> function, Func<T, T, bool>? equal = null) =>
        Law(name, generator, value => Equal(name, value, function(value), equal));
    public static PropertyDefinition<T> Idempotent<T>(string name, Gen<T> generator, Func<T, T> function, Func<T, T, bool>? equal = null) =>
        Law(name, generator, value => Equal(name, function(value), function(function(value)), equal));
    public static PropertyDefinition<T> Inverse<T>(string name, Gen<T> generator, Func<T, T> forward, Func<T, T> inverse, Func<T, T, bool>? equal = null) =>
        Law(name, generator, value => Equal(name, value, inverse(forward(value)), equal));
    public static PropertyDefinition<TIn> Roundtrip<TIn, TOut>(string name, Gen<TIn> generator, Func<TIn, TOut> encode, Func<TOut, TIn> decode, Func<TIn, TIn, bool>? equal = null) =>
        Law(name, generator, value => Equal(name, value, decode(encode(value)), equal));
    public static PropertyDefinition<(T A, T B)> Commutative<T, TResult>(string name, Gen<T> generator, Func<T, T, TResult> operation, Func<TResult, TResult, bool>? equal = null) =>
        Law(name, generator.Select(generator, static (a, b) => (A: a, B: b)), pair => Equal(name, operation(pair.A, pair.B), operation(pair.B, pair.A), equal));
    public static PropertyDefinition<(T A, T B, T C)> Associative<T>(string name, Gen<T> generator, Func<T, T, T> operation, Func<T, T, bool>? equal = null) =>
        Law(name, Triples(generator), triple => Equal(name, operation(operation(triple.A, triple.B), triple.C), operation(triple.A, operation(triple.B, triple.C)), equal));
    public static PropertyDefinition<(T A, T B, T C)> Distributive<T>(string name, Gen<T> generator, Func<T, T, T> multiply, Func<T, T, T> add, Func<T, T, bool>? equal = null) =>
        Law(name, Triples(generator), triple => Equal(name, multiply(triple.A, add(triple.B, triple.C)), add(multiply(triple.A, triple.B), multiply(triple.A, triple.C)), equal));
    public static PropertyDefinition<(T Lower, T Upper)> Monotone<T, TKey>(string name, Gen<(T Lower, T Upper)> pairs, Func<T, TKey> projection, IComparer<TKey>? comparer = null) =>
        Law(name, pairs, pair => Assert.True(
            (comparer ?? Comparer<TKey>.Default).Compare(projection(pair.Lower), projection(pair.Upper)) <= 0,
            $"{name}: f({pair.Lower}) = {projection(pair.Lower)} > {projection(pair.Upper)} = f({pair.Upper})"));
    public static PropertyDefinition<(T[] Source, T[] Shuffled)> Permutation<T, TResult>(string name, Gen<T[]> generator, Func<T[], TResult> function, Func<TResult, TResult, bool>? equal = null) =>
        Law(name, generator.SelectMany(static array => Gen.Shuffle(array).Select(shuffled => (Source: array, Shuffled: shuffled))), pair => Equal(name, function(pair.Source), function(pair.Shuffled), equal));

    private static PropertyDefinition<T> Law<T>(string name, Gen<T> generator, Action<T> property) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(generator);
        return new PropertyDefinition<T>(name, generator, property, Option<T>.None);
    }
    private static Gen<(T A, T B, T C)> Triples<T>(Gen<T> gen) => gen.Select(gen, gen, static (a, b, c) => (A: a, B: b, C: c));
    private static void Equal<T>(string name, T left, T right, Func<T, T, bool>? equal) =>
        Assert.True((equal ?? EqualityComparer<T>.Default.Equals)(left, right), $"{name}: {left} != {right}");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class TestAssertions {
    // CsCheck samples on pool threads that carry no ambient test context, and the token read here on the calling thread reaches every worker
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(gen);
        ArgumentNullException.ThrowIfNull(property);
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        gen.Sample(value => { cancellation.ThrowIfCancellationRequested(); property(value); }, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    // Only an assertion failure rejects the counterexample, and any other exception is a defect in the property that propagates
    public static void RejectsCounterexample<T>(T counterexample, Action<T> property, string? name = null) {
        ArgumentNullException.ThrowIfNull(property);
        try {
            property(counterexample);
        } catch (XunitException) {
            return;
        }
        throw new XunitException($"Property '{name ?? "unnamed"}' accepts its counterexample ({counterexample})");
    }
    public static void Verify<T>(PropertyDefinition<T> definition, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(definition);
        _ = definition.Counterexample.Iter(counterexample => RejectsCounterexample(counterexample, definition.Property, definition.Name));
        ForAll(definition.Generator, definition.Property, seed, iter, time, threads);
    }
    public static void Verify<T>(params PropertyDefinition<T>[] definitions) {
        NonEmpty(definitions, "Verify requires property definitions");
        _ = definitions.AsIterable().Iter(static definition => { Cancel(); Verify(definition); });
    }
    public static void Replay<T>(Gen<T> gen, Action<T> property, string seed) => ForAll(gen, property, seed, iter: 1);
    public static void Replay<T>(PropertyDefinition<T> definition, string seed) {
        ArgumentNullException.ThrowIfNull(definition);
        Replay(definition.Generator, definition.Property, seed);
    }

    // --- [METAMORPHIC] -----------------------------------------------------------------
    public static void Metamorphic<T, TResult>(Gen<T> gen, Func<T, TResult> f, params MetamorphicRelation<T, TResult>[] relations) {
        ArgumentNullException.ThrowIfNull(f);
        NonEmpty(relations, "Metamorphic requires relations");
        ForAll(gen, value => {
            TResult source = f(value);
            _ = relations.AsIterable().Iter(relation => {
                TResult transformed = f(relation.Transform(value));
                Assert.True(relation.Relate(value, source, transformed), $"Metamorphic relation '{relation.Name}' failed: source={source}, transformed={transformed}");
            });
        });
    }

    // --- [DIFFERENTIAL] ----------------------------------------------------------------
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
            Assert.True((equal ?? EqualityComparer<TResult>.Default.Equals)(actual, expected), $"Differential test failed: implementation={actual}, reference={expected}");
        }, seed, iter, time);
    }
    public static void Differential<T>(Gen<T> generator, Func<T, double> implementation, Func<T, double> reference, Tolerance tolerance, NumericComparison? comparison = null,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(implementation);
        ArgumentNullException.ThrowIfNull(reference);
        ForAll(generator, value => Equal(implementation(value), reference(value), tolerance, comparison, label: "Differential"), seed, iter, time);
    }

    // --- [RESULT_ASSERTIONS] -----------------------------------------------------------
    public static T SuccValue<T>(Fin<T> result, string label) {
        ArgumentNullException.ThrowIfNull(result);
        return result.Match(Succ: static value => value, Fail: error => throw new XunitException($"{label}: expected Succ, got Fail: {error.Message}"));
    }
    public static void Succ<T>(Fin<T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Succ: value => Tap(then, value), Fail: static error => throw new XunitException($"Expected Succ, got Fail: {error.Message}"));
    }
    public static void Fail<T>(Fin<T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Succ: static value => throw new XunitException($"Expected Fail, got Succ: {value}"), Fail: error => Tap(then, error));
    }
    public static void Valid<T>(Validation<Error, T> result, Action<T>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Fail: static error => throw new XunitException($"Expected Valid, got Invalid: {error.Message}"), Succ: value => Tap(then, value));
    }
    public static void Invalid<T>(Validation<Error, T> result, Action<Error>? then = null) {
        ArgumentNullException.ThrowIfNull(result);
        _ = result.Match(Fail: error => Tap(then, error), Succ: static value => throw new XunitException($"Expected Invalid, got Valid: {value}"));
    }
    public static void Some<T>(Option<T> result, Action<T>? then = null) =>
        _ = result.Match(Some: value => Tap(then, value), None: static () => throw new XunitException("Expected Some, got None"));
    public static void None<T>(Option<T> result) =>
        _ = result.Match(Some: static value => throw new XunitException($"Expected None, got Some: {value}"), None: static () => unit);

    // --- [DISTRIBUTION] ----------------------------------------------------------------
    // Twelve sigma draws a spurious failure once in 44,000 runs where the CsCheck default of six draws one in 483, and a shifted distribution exceeds it by orders of magnitude
    public static void ChiSquared<T>(Gen<T> gen, Func<T, int> bucket, params int[] expected) {
        ArgumentNullException.ThrowIfNull(bucket);
        ArgumentNullException.ThrowIfNull(expected);
        Assert.True(expected.Length > 1, "ChiSquared requires at least 2 expected buckets");
        int[] actual = new int[expected.Length];
        ForAll(gen, value => actual[bucket(value)]++, iter: expected.Sum(), threads: 1);
        Check.ChiSquared(expected, actual, sigma: 12.0);
    }

    // --- [BYTE_IDENTITY] ---------------------------------------------------------------
    public static void RoundtripBytes<T>(Gen<T> gen, JsonTypeInfo<T> contract, string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(contract);
        ForAll(gen, value => {
            byte[] raw = JsonSerializer.SerializeToUtf8Bytes(value, contract);
            T decoded = JsonSerializer.Deserialize(raw, contract) ?? throw new XunitException($"RoundtripBytes decoded null for {typeof(T).Name}");
            Assert.True(raw.AsSpan().SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(decoded, contract)), $"RoundtripBytes not byte-identical for {typeof(T).Name}");
        }, seed, iter, time);
    }

    // --- [CASE_TABLES] -----------------------------------------------------------------
    // Set equality keeps the comparison ordinal for string keys, which the default order comparer makes culture-sensitive, and the BCL set is qualified because LanguageExt declares a HashSet too
    public static void KeySet<T, TKey>(IReadOnlyList<T> items, IReadOnlyList<TKey> expectedKeys, Func<T, TKey> key, Action<T>? assertion = null) where TKey : notnull {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(expectedKeys);
        ArgumentNullException.ThrowIfNull(key);
        System.Collections.Generic.HashSet<TKey> expected = [.. expectedKeys];
        System.Collections.Generic.HashSet<TKey> actual = [.. items.Select(key)];
        Assert.Equal(items.Count, actual.Count);
        Assert.Equal(expected, actual);
        _ = items.AsIterable().Iter(item => { Cancel(); assertion?.Invoke(item); });
    }
    public static void CaseTable(params (string Label, Func<bool> Evaluate, bool Expected)[] rows) {
        NonEmpty(rows, "CaseTable requires rows");
        _ = rows.AsIterable().Iter(static row => {
            Cancel();
            bool actual = row.Evaluate();
            Assert.True(actual == row.Expected, $"{row.Label}: expected {row.Expected}, got {actual}");
        });
    }

    // --- [VALUE_OBJECTS] ---------------------------------------------------------------
    public sealed record ValueObjectCase<TIn, TStruct>(Gen<TIn> Valid, Gen<TIn> Invalid, TryCreate<TIn, TStruct> TryCreate, Func<TStruct, TIn> Read, Func<TIn, TIn, bool>? Equal = null);
    public static void ValueObjects<TIn, TStruct>(params ValueObjectCase<TIn, TStruct>[] cases) {
        NonEmpty(cases, "ValueObjects requires cases");
        _ = cases.AsIterable().Iter(static testCase => {
            Cancel();
            ForAll(testCase.Valid, value => {
                Assert.True(testCase.TryCreate(value, out TStruct constructed), $"Valid input rejected for {typeof(TStruct).Name}: {value}");
                Assert.True((testCase.Equal ?? EqualityComparer<TIn>.Default.Equals)(value, testCase.Read(constructed)), $"Round-trip mismatch for {typeof(TStruct).Name}: {value}");
            });
            ForAll(testCase.Invalid, value => Assert.False(testCase.TryCreate(value, out _), $"Invalid input accepted for {typeof(TStruct).Name}: {value}"));
        });
    }

    // --- [HELPERS] ---------------------------------------------------------------------
    private static void NonEmpty<T>(T[] table, string label) {
        ArgumentNullException.ThrowIfNull(table);
        Assert.True(table.Length > 0, label);
    }
    private static void Cancel() => TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
    private static Unit Tap<T>(Action<T>? action, T value) { action?.Invoke(value); return unit; }
}
