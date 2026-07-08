using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
public delegate bool TryCreate<TIn, TOut>(TIn value, out TOut obj);

// --- [MODELS] -------------------------------------------------------------------------------
// One metamorphic relation row: a follow-up-input transform and the (source, base, follow)
// relation it must satisfy. A table of rows shares one base evaluation per sample; the source
// argument lets one row family also carry oracle relations — never parallel relation methods.
public sealed record MetamorphicRelation<T, TResult>(string Name, Func<T, T> Transform, Func<T, TResult, TResult, bool> Relate);

// One content-key axis row: a transform and whether the key must survive it. Preserving rows are
// representation changes (copy, re-encode, transcode); separating rows are content changes. The
// gauge demands at least one separating row — without it a constant mint is irrefutable.
public sealed record KeyAxis<T>(string Name, Func<T, T> Transform, bool PreservesKey);

// A law is admissible only with a refuting witness: an input the property MUST fail on. Hold runs
// the refutation before sampling — a witness the property survives exposes the law as a tautology
// no mutant can ever violate, and that registration is itself the failure.
public sealed record Law<T>(string Name, Gen<T> Gen, Action<T> Property, T RefutingWitness);

// Law-row constructors: the algebra vocabulary. Each row closes over its equality policy so the
// witness refutes the same comparison the sampled property runs — a sloppy eq is unregistrable.
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
    // Commutative is result-typed: closed ops and symmetric projections (distance, dot) share one row.
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
    // A permutation-invariance witness is a non-permutation pair the projection genuinely splits.
    public static Law<(T[] Source, T[] Shuffled)> Permutation<T, TResult>(string name, Gen<T[]> gen, Func<T[], TResult> f, (T[] Source, T[] Shuffled) witness, Func<TResult, TResult, bool>? eq = null) =>
        Of(name: name, gen: gen.SelectMany(arr => Gen.Shuffle(arr).Select(perm => (Source: arr, Shuffled: perm))),
           property: p => Eq(name: name, left: f(p.Source), right: f(p.Shuffled), eq: eq), refutingWitness: witness);
    private static void Eq<T>(string name, T left, T right, Func<T, T, bool>? eq) =>
        _ = (eq ?? EqualityComparer<T>.Default.Equals)(left, right) ? true : throw new XunitException($"{name}: {left} != {right}");
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class Spec {
    // Null/-1 sentinels defer to CsCheck's Check.* defaults, which already give the CsCheck_Seed/
    // CsCheck_Iter/CsCheck_Time/CsCheck_Threads env knobs first refusal at static init.
    public static void ForAll<T>(Gen<T> gen, Action<T> property, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(argument: gen);
        ArgumentNullException.ThrowIfNull(argument: property);
        gen.Sample(assert: value => { Cancel(); property(value); }, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    public static void Holds(bool condition, string label) =>
        _ = condition ? unit : throw new XunitException(userMessage: label);
    // The tautology guard: a known-broken witness MUST make `law` throw, or the law proves nothing
    // and every mutant survives it. The sampler counts ANY throw as property failure, so any throw
    // refutes; Try owns the exception-to-Fin conversion, keeping this page catch-free.
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
    // Seed-replay regression: a shrunk counterexample is pinned by its seed and re-runs first.
    public static void Replay<T>(Gen<T> gen, Action<T> property, string seed) =>
        ForAll(gen: gen, property: property, seed: seed, iter: 1);
    public static void Replay<T>(Law<T> law, string seed) {
        ArgumentNullException.ThrowIfNull(argument: law);
        Replay(gen: law.Gen, property: law.Property, seed: seed);
    }

    // --- [METAMORPHIC]
    // One entry family: f(x) is computed once and every relation row checks it against
    // f(transform(x)); an oracle relation rides the same table through the source argument.
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
    // The actual-vs-model pairing IS the stateful law; every stateful sampler shares the seed/iter/
    // time replay knobs so a shrunk interleaving or operation sequence pins by seed like ForAll.
    public static void ModelBased<TActual, TModel>(Gen<(TActual Actual, TModel Model)> init, Func<TActual, TModel, bool> equal, GenOperation<TActual, TModel>[] operations,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: operations);
        Holds(condition: operations.Length > 0, label: "ModelBased: empty operation table proves nothing");
        Cancel();
        init.SampleModelBased(operations: operations, equal: equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    // Two mutation paths over one subject must land in equal states; CsCheck shrinks the parameter.
    public static void DualPath<T, TParam>(Gen<T> initial, Gen<TParam> paramGen, Func<TParam, string> name, Action<T, TParam> path1, Action<T, TParam> path2,
        Func<T, T, bool>? equal = null, string? seed = null, long? iter = null, int? time = null) {
        Cancel();
        initial.SampleMetamorphic(operations: GenMetamorphic.Create(gen: paramGen, name: name, action1: path1, action2: path2), equal: equal, seed: seed, iter: iter ?? -1L, time: time ?? -1);
    }
    // The pure differential arm of the same family: a subject answer against an independent
    // reference over one generated input — golden folds, dual algorithms, and external oracles
    // ride this shape; the mutation arm above shares the name because the law is identical.
    public static void DualPath<T, TResult>(Gen<T> gen, Func<T, TResult> subject, Func<T, TResult> reference, Func<TResult, TResult, bool>? eq = null,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: subject);
        ArgumentNullException.ThrowIfNull(argument: reference);
        ForAll(gen: gen, property: value => {
            (TResult actual, TResult expected) = (subject(value), reference(value));
            Holds(condition: (eq ?? EqualityComparer<TResult>.Default.Equals)(actual, expected), label: $"DualPath diverged: subject={actual}, reference={expected}");
        }, seed: seed, iter: iter, time: time);
    }
    // Tolerance-carrying differential: numeric divergence classifies through the Metric row —
    // sign ambiguity and periodic wrap are admitted classes, magnitude drift is the rejection.
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
    // The failure's closed-family case name, not its message, is the stable failure contract.
    public static void FailCategory<T>(Fin<T> result, string category) =>
        Fail(result: result, then: error => Assert.Equal(expected: category, actual: CategoryOf(error: error)));
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
    // Flattening ManyErrors plus ordinal sort proves Applicative error-order independence.
    public static void AllErrors<T>(Validation<Error, T> v, params string[] expectedCategories) {
        ArgumentNullException.ThrowIfNull(argument: v);
        string[] expected = [.. expectedCategories.Order(comparer: StringComparer.Ordinal)];
        _ = v.Match(Succ: value => throw new XunitException($"Expected Fail; got Succ: {value}"), Fail: error => {
            Seq<Error> errors = error switch { ManyErrors many => toSeq(many.Errors), _ => Seq(error) };
            Assert.Equal(expected: expected, actual: [.. errors.Map(static (Error e) => CategoryOf(error: e)).Order(comparer: StringComparer.Ordinal)]);
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
    public static void FinChainOrder<T>(Gen<T> gen, Func<T, Fin<T>> first, Func<T, Fin<T>> second, Func<T, T, bool>? eq = null) =>
        ForAll(gen: gen, property: value => {
            (Fin<T> left, Fin<T> right) = (first(value).Bind(second), second(value).Bind(first));
            _ = (left.IsSucc, right.IsSucc) switch {
                (true, true) => (eq ?? EqualityComparer<T>.Default.Equals)(SuccValue(result: left, label: "left"), SuccValue(result: right, label: "right"))
                    ? true : throw new XunitException($"FinChainOrder: {left} != {right}"),
                (false, false) => true,
                _ => throw new XunitException($"FinChainOrder asymmetric: left.IsSucc={left.IsSucc}, right.IsSucc={right.IsSucc}"),
            };
        });

    // --- [DISTRIBUTION]
    // Classify-table print: buckets and timing per class over one sample sweep.
    public static void Classified<T>(Gen<T> gen, Func<T, string> classify, Action<string> writeLine, string? seed = null, long? iter = null, int? time = null, int? threads = null) {
        ArgumentNullException.ThrowIfNull(argument: gen);
        ArgumentNullException.ThrowIfNull(argument: classify);
        ArgumentNullException.ThrowIfNull(argument: writeLine);
        gen.Sample(classify: classify, writeLine: writeLine, seed: seed, iter: iter ?? -1L, time: time ?? -1, threads: threads ?? -1);
    }
    // Chi-squared distribution law: expected counts define both the sample size and the buckets.
    public static void Distributed<T>(Gen<T> gen, Func<T, int> bucket, params int[] expected) {
        ArgumentNullException.ThrowIfNull(argument: bucket);
        ArgumentNullException.ThrowIfNull(argument: expected);
        Holds(condition: expected.Length > 1, label: "Distributed: a one-bucket expectation proves nothing");
        int[] actual = new int[expected.Length];
        ForAll(gen: gen, property: value => Interlocked.Increment(location: ref actual[bucket(value)]), iter: expected.Sum(), threads: 1);
        Check.ChiSquared(expected: expected, actual: actual);
    }

    // --- [BYTE_IDENTITY]
    // Deterministic encode -> decode -> re-encode byte identity through the contract's
    // JsonTypeInfo<T>; the re-encode step catches non-deterministic codecs Roundtrip cannot see.
    public static void RoundtripBytes<T>(Gen<T> gen, JsonTypeInfo<T> contract, string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: contract);
        ForAll(gen: gen, property: value => {
            byte[] raw = JsonSerializer.SerializeToUtf8Bytes(value: value, jsonTypeInfo: contract);
            T decoded = JsonSerializer.Deserialize(utf8Json: raw, jsonTypeInfo: contract) ?? throw new XunitException($"RoundtripBytes decoded null for {typeof(T).Name}");
            byte[] reencoded = JsonSerializer.SerializeToUtf8Bytes(value: decoded, jsonTypeInfo: contract);
            Holds(condition: raw.AsSpan().SequenceEqual(reencoded), label: $"RoundtripBytes not byte-identical for {typeof(T).Name}");
        }, seed: seed, iter: iter, time: time);
    }

    // The fixture-corpus arm: every discovered golden fixture must decode and re-encode to its
    // exact producer bytes, and byte-identical twin fixtures are a corpus defect. The fold names
    // EVERY drifting fixture, never the first.
    public static void RoundtripBytes<T>(JsonTypeInfo<T> contract, IReadOnlyCollection<CorpusEntry> corpus) {
        ArgumentNullException.ThrowIfNull(argument: contract);
        ArgumentNullException.ThrowIfNull(argument: corpus);
        Holds(condition: corpus.Count > 0, label: "RoundtripBytes: empty corpus proves nothing");
        string[] twins = [.. corpus.GroupBy(keySelector: static entry => entry.Key)
            .Where(predicate: static group => group.Count() > 1)
            .Select(selector: static group => string.Join(separator: " == ", values: group.Select(selector: static entry => entry.RelativePath)))];
        Holds(condition: twins.Length == 0, label: $"corpus carries byte-identical fixtures: {string.Join(separator: "; ", values: twins)}");
        string[] drift = [.. corpus.SelectMany(selector: entry => {
            byte[] raw = entry.Bytes();
            T? decoded = JsonSerializer.Deserialize(utf8Json: raw, jsonTypeInfo: contract);
            return decoded is null ? (string[])[$"{entry.RelativePath}: decoded null"]
                : raw.AsSpan().SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(value: decoded, jsonTypeInfo: contract)) ? []
                : [$"{entry.RelativePath}: re-encode drifted from producer bytes"];
        })];
        Holds(condition: drift.Length == 0, label: $"corpus roundtrip drift for {typeof(T).Name}: {string.Join(separator: "; ", values: drift)}");
    }

    // --- [IDENTITY]
    // The universal content-key gauge: one mint over one input space proves stability (same input,
    // same key), representation independence (preserving axes keep the key), and separation
    // (content changes move the key) in one table-driven pass over any key type.
    public static void ContentKey<T, TKey>(Gen<T> gen, Func<T, TKey> mint, params KeyAxis<T>[] axes) {
        ArgumentNullException.ThrowIfNull(argument: mint);
        ArgumentNullException.ThrowIfNull(argument: axes);
        Holds(condition: axes.Length > 0, label: "ContentKey: empty axis table proves nothing");
        Holds(condition: axes.Any(predicate: static axis => !axis.PreservesKey),
              label: "ContentKey: an all-preserving axis table cannot refute a constant mint — register a separating axis");
        ForAll(gen: gen, property: value => {
            TKey key = mint(value);
            Holds(condition: EqualityComparer<TKey>.Default.Equals(x: key, y: mint(value)), label: $"ContentKey: unstable mint — two mints of one input disagree ({key})");
            _ = axes.AsIterable().Iter(axis => {
                TKey moved = mint(axis.Transform(value));
                Holds(condition: EqualityComparer<TKey>.Default.Equals(x: key, y: moved) == axis.PreservesKey, label: axis.PreservesKey
                    ? $"ContentKey '{axis.Name}': a preserved representation moved the key ({key} -> {moved})"
                    : $"ContentKey '{axis.Name}': a content change failed to move the key ({key})");
            });
        });
    }

    // --- [RECEIPT_ALGEBRA]
    // The hybrid-logical-clock advance law: physical advance resets the logical half to zero,
    // a held physical strictly increments it, and regression is never admissible. The refuting
    // witness is a PAIR the order relation itself must reject — refutation before sampling.
    public static void Causal<T>(Gen<T> gen, Func<T, T> advance, Func<T, long> physical, Func<T, ulong> logical, (T Before, T After) refutingWitness,
        string? seed = null, long? iter = null, int? time = null) {
        ArgumentNullException.ThrowIfNull(argument: advance);
        ArgumentNullException.ThrowIfNull(argument: physical);
        ArgumentNullException.ThrowIfNull(argument: logical);
        bool Follows(T before, T after) =>
            physical(after) > physical(before) ? logical(after) == 0UL : physical(after) == physical(before) && logical(after) > logical(before);
        Holds(condition: !Follows(refutingWitness.Before, refutingWitness.After),
              label: "Causal: the refuting witness pair satisfies the stamp order — the law is a tautology");
        ForAll(gen: gen, property: value => {
            T next = advance(value);
            Holds(condition: Follows(value, next), label: string.Create(provider: CultureInfo.InvariantCulture,
                $"Causal: advance broke the stamp order (physical {physical(value)} -> {physical(next)}, logical {logical(value)} -> {logical(next)})"));
        }, seed: seed, iter: iter, time: time);
    }
    // One fault-band registry law: every code inside its owner's band, no duplicate codes or
    // owners, and bands pairwise disjoint — one folded verdict naming every violation.
    public static void FaultBands(params (string Owner, int Start, int End, int[] Codes)[] rows) {
        ArgumentNullException.ThrowIfNull(argument: rows);
        Holds(condition: rows.Length > 0, label: "FaultBands: empty band registry proves nothing");
        (string Owner, int Start, int End, int[] Codes)[] ordered = [.. rows.OrderBy(keySelector: static row => row.Start)];
        string[] defects = [
            .. rows.Where(predicate: static row => row.Start > row.End)
                .Select(selector: static row => string.Create(provider: CultureInfo.InvariantCulture, $"{row.Owner}: inverted band [{row.Start}, {row.End}]")),
            .. rows.SelectMany(selector: static row => row.Codes.Where(code => code < row.Start || code > row.End)
                .Select(code => string.Create(provider: CultureInfo.InvariantCulture, $"{row.Owner}: code {code} outside band [{row.Start}, {row.End}]"))),
            .. rows.Where(predicate: static row => row.Codes.Length != row.Codes.Distinct().Count())
                .Select(selector: static row => $"{row.Owner}: duplicate codes"),
            .. rows.GroupBy(keySelector: static row => row.Owner, comparer: StringComparer.Ordinal)
                .Where(predicate: static group => group.Count() > 1)
                .Select(selector: static group => $"owner '{group.Key}' registers more than one band"),
            .. ordered.Zip(ordered.Skip(count: 1))
                .Where(predicate: static pair => pair.Second.Start <= pair.First.End)
                .Select(selector: static pair => $"bands '{pair.First.Owner}' and '{pair.Second.Owner}' overlap"),
        ];
        Holds(condition: defects.Length == 0, label: $"fault-band registry defects: {string.Join(separator: "; ", values: defects)}");
    }
    // The verdict-fold law: a Worst-style status fold over a closed vocabulary is a bounded
    // semilattice — closed, commutative, associative, idempotent, identity-absorbed — proven
    // EXHAUSTIVELY over the vocabulary, so no witness ceremony and no sampling gap exists.
    public static void Semilattice<T>(IReadOnlyList<T> vocabulary, Func<T, T, T> combine, T identity, Func<T, T, bool>? eq = null) {
        ArgumentNullException.ThrowIfNull(argument: vocabulary);
        ArgumentNullException.ThrowIfNull(argument: combine);
        Holds(condition: vocabulary.Count > 0, label: "Semilattice: empty vocabulary proves nothing");
        Func<T, T, bool> equal = eq ?? EqualityComparer<T>.Default.Equals;
        bool Member(T value) => vocabulary.Any(row => equal(row, value));
        for (int i = 0; i < vocabulary.Count; i++) {
            for (int j = i + 1; j < vocabulary.Count; j++) {
                Holds(condition: !equal(vocabulary[i], vocabulary[j]), label: string.Create(provider: CultureInfo.InvariantCulture, $"Semilattice: vocabulary rows {i} and {j} coincide"));
            }
        }
        Holds(condition: Member(identity), label: $"Semilattice: identity {identity} lies outside the vocabulary");
        foreach (T x in vocabulary) {
            Cancel();
            Holds(condition: equal(combine(x, identity), x) && equal(combine(identity, x), x), label: $"Semilattice: identity fails to absorb under {x}");
            Holds(condition: equal(combine(x, x), x), label: $"Semilattice: combine is not idempotent at {x}");
            foreach (T y in vocabulary) {
                T xy = combine(x, y);
                Holds(condition: Member(xy), label: $"Semilattice: combine escapes the vocabulary at ({x}, {y})");
                Holds(condition: equal(xy, combine(y, x)), label: $"Semilattice: combine is not commutative at ({x}, {y})");
                foreach (T z in vocabulary) {
                    Holds(condition: equal(combine(xy, z), combine(x, combine(y, z))), label: $"Semilattice: combine is not associative at ({x}, {y}, {z})");
                }
            }
        }
    }

    // --- [CASE_TABLES]
    // One catalog law: key uniqueness, exact expected membership, and the per-item law in one pass.
    public static void Catalog<T, TKey>(IReadOnlyList<T> items, IReadOnlyList<TKey> expectedKeys, Func<T, TKey> key, Action<T>? law = null) where TKey : notnull {
        ArgumentNullException.ThrowIfNull(argument: items);
        ArgumentNullException.ThrowIfNull(argument: expectedKeys);
        ArgumentNullException.ThrowIfNull(argument: key);
        Assert.Equal(expected: items.Count, actual: items.Select(selector: key).ToHashSet().Count);
        Assert.Equal(expected: [.. expectedKeys.Order()], actual: (TKey[])[.. items.Select(selector: key).Order()]);
        _ = items.AsIterable().Iter(item => { Cancel(); law?.Invoke(item); });
    }
    // Per-row thunks preserve call-site generics and diagnostics without anonymous assertion walls.
    public static void Matrix(params (string Label, Func<bool> Probe, bool Expected)[] rows) {
        ArgumentNullException.ThrowIfNull(argument: rows);
        Holds(condition: rows.Length > 0, label: "Matrix: empty row table proves nothing");
        _ = rows.AsIterable().Iter(row => {
            Cancel();
            bool actual = row.Probe();
            Holds(condition: actual == row.Expected, label: $"{row.Label}: expected {row.Expected}, got {actual}");
        });
    }
    public static void CountsConserve(int attempted, int emitted, int rejected, string label) {
        Holds(condition: attempted >= 0 && emitted >= 0 && rejected >= 0, label: string.Create(provider: CultureInfo.InvariantCulture, $"{label}: negative count (attempted={attempted}, emitted={emitted}, rejected={rejected})"));
        Assert.Equal(expected: attempted, actual: emitted + rejected);
    }

    // --- [VALUE_OBJECTS]
    // A value-object law is admission-and-rejection or nothing: the shape carries both generators.
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
    // Runner cancellation must enter long CsCheck sample loops.
    private static void Cancel() => TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
    private static Unit Tap<T>(Action<T>? action, T value) { action?.Invoke(value); return unit; }
    private static string CategoryOf(Error error) => error.GetType().Name;
}
