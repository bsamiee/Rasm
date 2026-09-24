using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LanguageExt.UnsafeValueAccess;
using Xunit.Sdk;

namespace Rasm.TestSupport;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MetamorphicRelation<T, TResult>(string Name, Func<T, T> Transform, Func<T, TResult, TResult, bool> Relate);

public sealed record PropertyDefinition<T>(string Name, Gen<T> Generator, Action<T> Property, Option<T> Counterexample);

public sealed record ValueObjectCase<TIn, TValueObject>(Gen<TIn> Valid, Gen<TIn> Invalid, Func<TIn, Fin<TValueObject>> Create, Func<TValueObject, TIn> Read, Option<Func<TIn, TIn, bool>> Equal = default);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Properties {
    public static PropertyDefinition<T> Define<T>(string name, Gen<T> generator, Action<T> property, Option<T> counterexample = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new PropertyDefinition<T>(name, generator, property, counterexample);
    }
    public static PropertyDefinition<T> Identity<T>(string name, Gen<T> generator, Func<T, T> function, Option<Func<T, T, bool>> equal = default) =>
        Define(name, generator, value => Equal(name, value, function(value), equal));
    public static PropertyDefinition<T> Idempotent<T>(string name, Gen<T> generator, Func<T, T> function, Option<Func<T, T, bool>> equal = default) =>
        Define(name, generator, value => Equal(name, function(value), function(function(value)), equal));
    public static PropertyDefinition<T> Inverse<T>(string name, Gen<T> generator, Func<T, T> forward, Func<T, T> inverse, Option<Func<T, T, bool>> equal = default) =>
        Define(name, generator, value => Equal(name, value, inverse(forward(value)), equal));
    public static PropertyDefinition<TIn> Roundtrip<TIn, TOut>(string name, Gen<TIn> generator, Func<TIn, TOut> encode, Func<TOut, TIn> decode, Option<Func<TIn, TIn, bool>> equal = default) =>
        Define(name, generator, value => Equal(name, value, decode(encode(value)), equal));
    public static PropertyDefinition<(T A, T B)> Commutative<T, TResult>(string name, Gen<T> generator, Func<T, T, TResult> operation, Option<Func<TResult, TResult, bool>> equal = default) =>
        Define(name, generator.Select(generator, static (a, b) => (A: a, B: b)), pair => Equal(name, operation(pair.A, pair.B), operation(pair.B, pair.A), equal));
    public static PropertyDefinition<(T A, T B, T C)> Associative<T>(string name, Gen<T> generator, Func<T, T, T> operation, Option<Func<T, T, bool>> equal = default) =>
        Define(name, Triples(generator), triple => Equal(name, operation(operation(triple.A, triple.B), triple.C), operation(triple.A, operation(triple.B, triple.C)), equal));
    public static PropertyDefinition<(T A, T B, T C)> Distributive<T>(string name, Gen<T> generator, Func<T, T, T> multiply, Func<T, T, T> add, Option<Func<T, T, bool>> equal = default) =>
        Define(name, Triples(generator), triple => Equal(name, multiply(triple.A, add(triple.B, triple.C)), add(multiply(triple.A, triple.B), multiply(triple.A, triple.C)), equal));
    public static PropertyDefinition<(T Lower, T Upper)> Monotone<T, TKey>(string name, Gen<(T Lower, T Upper)> pairs, Func<T, TKey> projection, Option<IComparer<TKey>> comparer = default) =>
        Define(name, pairs, pair => Assert.True(
            comparer.IfNone(Comparer<TKey>.Default).Compare(projection(pair.Lower), projection(pair.Upper)) <= 0,
            $"{name}: f({pair.Lower}) = {projection(pair.Lower)} > {projection(pair.Upper)} = f({pair.Upper})"));
    public static PropertyDefinition<(T[] Source, T[] Shuffled)> Permutation<T, TResult>(string name, Gen<T[]> generator, Func<T[], TResult> function, Option<Func<TResult, TResult, bool>> equal = default) =>
        Define(name, from array in generator from shuffled in Gen.Shuffle(array) select (Source: array, Shuffled: shuffled), pair => Equal(name, function(pair.Source), function(pair.Shuffled), equal));

    private static Gen<(T A, T B, T C)> Triples<T>(Gen<T> gen) => gen.Select(gen, gen, static (a, b, c) => (A: a, B: b, C: c));
    private static void Equal<T>(string name, T left, T right, Option<Func<T, T, bool>> equal) =>
        Assert.True(equal.IfNone(EqualityComparer<T>.Default.Equals)(left, right), $"{name}: {left} != {right}");
}

public static class TestAssertions {
    // --- [PROPERTIES]
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long iter = -1L, int time = -1, int threads = -1) {
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        gen.Sample(value => { cancellation.ThrowIfCancellationRequested(); property(value); }, seed: seed, iter: iter, time: time, threads: threads);
    }
    public static void RejectsCounterexample<T>(T counterexample, Action<T> property) =>
        _ = Assert.ThrowsAny<XunitException>(() => property(counterexample));
    public static void Verify<T>(PropertyDefinition<T> definition, string? seed = null, long iter = -1L, int time = -1, int threads = -1) {
        _ = definition.Counterexample.Iter(counterexample => RejectsCounterexample(counterexample, definition.Property));
        ForAll(definition.Generator, definition.Property, seed, iter, time, threads);
    }
    public static void Verify<T>(params PropertyDefinition<T>[] definitions) {
        NonEmpty(definitions, "Verify requires property definitions");
        _ = definitions.AsIterable().Iter(static definition => { Cancel(); Verify(definition); });
    }
    public static void Replay<T>(Gen<T> gen, Action<T> property, string seed) => ForAll(gen, property, seed, iter: 1L);
    public static void Replay<T>(PropertyDefinition<T> definition, string seed) => Replay(definition.Generator, definition.Property, seed);

    // --- [METAMORPHIC]
    public static void Metamorphic<T, TResult>(Gen<T> gen, Func<T, TResult> f, params MetamorphicRelation<T, TResult>[] relations) {
        NonEmpty(relations, "Metamorphic requires relations");
        ForAll(gen, value => {
            TResult source = f(value);
            _ = relations.AsIterable().Iter(relation => {
                TResult transformed = f(relation.Transform(value));
                Assert.True(relation.Relate(value, source, transformed), $"Metamorphic relation '{relation.Name}' failed: source={source}, transformed={transformed}");
            });
        });
    }

    // --- [APPROXIMATION]
    public static void Equal(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, NumericComparison comparison, string label = nameof(Equal)) {
        Assert.True(left.Length == right.Length, string.Create(CultureInfo.InvariantCulture, $"{label}: length {left.Length} != {right.Length}"));
        Assert.True(comparison.Matches(left, right, tolerance), $"{label} ({comparison}): {Render(left)} vs {Render(right)} exceed ({tolerance})");
    }

    private static string Render(ReadOnlySpan<double> values) {
        string head = string.Join(", ", values[..Math.Min(8, values.Length)].ToArray().Select(static x => x.ToString("R", CultureInfo.InvariantCulture)));
        return values.Length > 8 ? $"[{head}, .. {values.Length} total]" : $"[{head}]";
    }

    // --- [DIFFERENTIAL]
    public static void Differential<T, TParam>(Gen<T> initial, Gen<TParam> parameters, Func<TParam, string> name, Action<T, TParam> implementation, Action<T, TParam> reference,
        Option<Func<T, T, bool>> equal = default, string? seed = null, long iter = -1L, int time = -1, int threads = -1) {
        Cancel();
        initial.SampleMetamorphic(GenMetamorphic.Create(parameters, name, implementation, reference), equal.ValueUnsafe(), seed: seed, iter: iter, time: time, threads: threads);
    }
    public static void Differential<T, TResult>(Gen<T> generator, Func<T, TResult> implementation, Func<T, TResult> reference, Option<Func<TResult, TResult, bool>> equal = default,
        string? seed = null, long iter = -1L, int time = -1, int threads = -1) =>
        ForAll(generator, value => {
            (TResult actual, TResult expected) = (implementation(value), reference(value));
            Assert.True(equal.IfNone(EqualityComparer<TResult>.Default.Equals)(actual, expected), $"Differential test failed: implementation={actual}, reference={expected}");
        }, seed, iter, time, threads);
    public static void Differential<T>(Gen<T> generator, Func<T, double> implementation, Func<T, double> reference, Tolerance tolerance, NumericComparison comparison,
        string? seed = null, long iter = -1L, int time = -1, int threads = -1) =>
        ForAll(generator, value => Equal([implementation(value)], [reference(value)], tolerance, comparison, label: nameof(Differential)), seed, iter, time, threads);

    // --- [RESULT_ASSERTIONS]
    public static T SuccValue<T>(Fin<T> result, string label) =>
        result.IfFail(error => throw new XunitException($"{label}: expected Succ, got Fail: {error.Message}"));
    public static void Succ<T>(Fin<T> result, Action<T>? then = null) {
        T value = SuccValue(result, nameof(Succ));
        then?.Invoke(value);
    }
    public static void Fail<T>(Fin<T> result, Action<Error>? then = null) =>
        _ = result.Match(Succ: static value => throw new XunitException($"Expected Fail, got Succ: {value}"), Fail: error => then?.Invoke(error));
    public static void Valid<T>(Validation<Error, T> result, Action<T>? then = null) =>
        _ = result.Match(Fail: static error => throw new XunitException($"Expected Valid, got Invalid: {error.Message}"), Succ: value => then?.Invoke(value));
    public static void Invalid<T>(Validation<Error, T> result, Action<Error>? then = null) =>
        _ = result.Match(Fail: error => then?.Invoke(error), Succ: static value => throw new XunitException($"Expected Invalid, got Valid: {value}"));
    public static void Some<T>(Option<T> result, Action<T>? then = null) =>
        _ = result.Match(Some: value => then?.Invoke(value), None: static () => throw new XunitException("Expected Some, got None"));
    public static void None<T>(Option<T> result) =>
        _ = result.Match(Some: static value => throw new XunitException($"Expected None, got Some: {value}"), None: static () => unit);

    // --- [DISTRIBUTION]
    public static void ChiSquared<T>(Gen<T> gen, Func<T, int> bucket, params int[] expected) {
        Assert.True(expected.Length > 1, "ChiSquared requires at least 2 expected buckets");
        int[] actual = new int[expected.Length];
        ForAll(gen, value => actual[bucket(value)]++, iter: expected.Sum(), threads: 1);
        Check.ChiSquared(expected, actual, sigma: 12.0);
    }

    // --- [BYTE_IDENTITY]
    public static void RoundtripBytes<T>(Gen<T> gen, JsonTypeInfo<T> contract, string? seed = null, long iter = -1L, int time = -1, int threads = -1) =>
        ForAll(gen, value => {
            byte[] raw = JsonSerializer.SerializeToUtf8Bytes(value, contract);
            T decoded = JsonSerializer.Deserialize(raw, contract) ?? throw new XunitException($"RoundtripBytes decoded null for {typeof(T).Name}");
            Assert.True(raw.AsSpan().SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(decoded, contract)), $"RoundtripBytes not byte-identical for {typeof(T).Name}");
        }, seed, iter, time, threads);

    // --- [CASE_TABLES]
    public static void KeySet<T, TKey>(IReadOnlyList<T> items, IReadOnlyList<TKey> expectedKeys, Func<T, TKey> key, Action<T>? assertion = null) where TKey : notnull {
        System.Collections.Generic.HashSet<TKey> expected = [.. expectedKeys];
        System.Collections.Generic.HashSet<TKey> actual = [.. items.Select(key)];
        Assert.Equal(items.Count, actual.Count);
        Assert.Equal(expected, actual);
        _ = items.AsIterable().Iter(item => { Cancel(); assertion?.Invoke(item); });
    }
    public static void CaseTable(params (string Label, Func<bool> Evaluate, bool Required)[] rows) {
        NonEmpty(rows, "CaseTable requires rows");
        _ = rows.AsIterable().Iter(static row => {
            Cancel();
            bool actual = row.Evaluate();
            Assert.True(actual == row.Required, $"{row.Label}: expected {row.Required}, got {actual}");
        });
    }

    // --- [VALUE_OBJECTS]
    public static void ValueObjects<TIn, TValueObject>(params ValueObjectCase<TIn, TValueObject>[] cases) {
        NonEmpty(cases, "ValueObjects requires cases");
        _ = cases.AsIterable().Iter(static testCase => {
            Cancel();
            ForAll(testCase.Valid, value => Assert.True(
                testCase.Equal.IfNone(EqualityComparer<TIn>.Default.Equals)(value, testCase.Read(SuccValue(testCase.Create(value), $"Valid input rejected for {typeof(TValueObject).Name}: {value}"))),
                $"Round-trip mismatch for {typeof(TValueObject).Name}: {value}"));
            ForAll(testCase.Invalid, value => Assert.True(testCase.Create(value).IsFail, $"Invalid input accepted for {typeof(TValueObject).Name}: {value}"));
        });
    }

    // --- [PRECONDITIONS]
    private static void NonEmpty<T>(T[] table, string label) => Assert.True(table.Length > 0, label);
    private static void Cancel() => TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
}
