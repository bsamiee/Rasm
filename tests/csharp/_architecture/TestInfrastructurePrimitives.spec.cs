using System.Collections.Frozen;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rasm.Csp;
using Rasm.TestKit;
using Xunit.Sdk;

namespace Rasm.Architecture.Tests;

// --- [MODELS] --------------------------------------------------------------------------
// One source-generated contract feeds RoundtripBytes, NdjsonOracle, and VariantWriter without a
// reflection serializer; the row carries the deterministic wire shape the byte-identity laws check.
public sealed record SampleRow(string Tag, int Rank);

// The byte-identity falsification carrier: its converter mangles the READ side, so decode drifts
// and the re-encode can never match the raw bytes — the input RoundtripBytes must reject.
[JsonConverter(typeof(DriftingTagConverter))]
public sealed record DriftRow(string Tag);

internal sealed class DriftingTagConverter : JsonConverter<DriftRow> {
    public override DriftRow Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(Tag: (reader.GetString() ?? string.Empty) + "x");

    public override void Write(Utf8JsonWriter writer, DriftRow value, JsonSerializerOptions options) {
        ArgumentNullException.ThrowIfNull(argument: writer);
        ArgumentNullException.ThrowIfNull(argument: value);
        writer.WriteStringValue(value: value.Tag);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(SampleRow))]
[JsonSerializable(typeof(DriftRow))]
internal sealed partial class SampleContext : JsonSerializerContext;

// Production-shaped exemption sites: Laws.Sut must derive them from the typed Csp attributes.
[CspExempt("coverage-gate self-proof site")]
internal static class ExemptSite { public static int Reached() => 1; }

[CspScope(CspScope.Tooling)]
internal static class ToolingSite { public static int Reached() => 2; }

[CspScope(CspScope.Domain)]
internal static class DomainSite { public static int Reached() => 3; }

// --- [OPERATIONS] ----------------------------------------------------------------------
// The kit's only falsification suite: every mechanism must be shown failing, not just passing.
// The class-level [Law] proves ScanAssembly folds type-level markers; the pilot SUT is the
// host-free Csp.Contracts assembly.
[Law(typeof(Laws), "law-manifest-scan")]
public sealed class TestInfrastructurePrimitiveLaws {
    private static readonly Gen<SampleRow> Rows =
        Gen.String[1, 16].Select(Gen.Int[-100, 100], static (string tag, int rank) => new SampleRow(Tag: tag, Rank: rank));
    private static readonly FrozenSet<string> AbsentVariants = FrozenSet.ToFrozenSet(["gone"], StringComparer.Ordinal);

    [Fact]
    [Law(typeof(Laws), nameof(Laws.ScanAssembly), Member = nameof(Laws.ScanAssembly))]
    public void ScanAssemblyFoldsLawMarkers() {
        Seq<LawRecord> manifest = Laws.ScanAssembly(specAssembly: typeof(TestInfrastructurePrimitiveLaws).Assembly);
        Assert.Contains(collection: manifest, filter: static record => record.Subject == typeof(Laws) && string.Equals(a: record.Name, b: "law-manifest-scan", comparisonType: StringComparison.Ordinal) && record.Member.IsNone);
        Assert.Contains(collection: manifest, filter: static record => string.Equals(a: record.CoveredSymbol, b: nameof(Laws.ScanAssembly), comparisonType: StringComparison.Ordinal));
    }

    [Fact]
    [Law(typeof(Laws), nameof(Laws.AssertCoverage), Member = nameof(Laws.AssertCoverage))]
    public void CoverageGateReportsUncoveredSutSurface() {
        SutTarget target = Laws.Sut(sutAssembly: typeof(CspScope).Assembly);
        // The pilot SUT carries no [Law] markers, so the gate must Fail with a ManyErrors body that
        // names the uncovered public symbols — the mechanism, not full coverage, is the law here.
        Spec.Fail(result: Laws.AssertCoverage(target: target, manifest: Seq<LawRecord>()), then: static error => {
            ManyErrors many = Assert.IsType<ManyErrors>(@object: error);
            Assert.NotEmpty(collection: many.Errors);
            Assert.Contains(collection: many.Errors, filter: static e => e.Message.Contains(value: "law coverage gap", comparisonType: StringComparison.Ordinal));
            Assert.Contains(collection: many.Errors, filter: static e => e.Message.Contains(value: $"'{nameof(CspScopeAttribute)}'", comparisonType: StringComparison.Ordinal));
        });
    }

    [Fact]
    [Law(typeof(Laws), nameof(Laws.Sut), Member = nameof(Laws.Sut))]
    public void CoverageCreditsManifestAndTypedExemptions() {
        SutTarget target = Laws.Sut(sutAssembly: typeof(CspScope).Assembly);
        FrozenSet<string> surface = typeof(CspScope).Assembly.GetExportedTypes().Select(static type => type.Name).ToFrozenSet(StringComparer.Ordinal);
        Seq<LawRecord> covering = toSeq(surface).Map(static name => new LawRecord(Subject: typeof(CspScope), Name: name, Member: Optional(name), DeclaringType: typeof(TestInfrastructurePrimitiveLaws)));
        // Covering every exported type name collapses the gap set to member-level symbols only.
        Spec.Fail(result: Laws.AssertCoverage(target: target, manifest: covering), then: error =>
            Assert.DoesNotContain(collection: Assert.IsType<ManyErrors>(@object: error).Errors, filter: e => e.Message.Contains(value: $"'{nameof(CspScopeAttribute)}'", comparisonType: StringComparison.Ordinal)));
        // Typed derivation: [CspExempt] and [CspScope(Tooling)] sites exempt themselves; any other
        // scope carries the full obligation — the matching runs on the real attribute types.
        SutTarget self = Laws.Sut(sutAssembly: typeof(TestInfrastructurePrimitiveLaws).Assembly);
        Assert.Contains(expected: nameof(ExemptSite), collection: self.ExemptNames);
        Assert.Contains(expected: nameof(ExemptSite.Reached), collection: self.ExemptNames);
        Assert.Contains(expected: nameof(ToolingSite), collection: self.ExemptNames);
        Assert.DoesNotContain(expected: nameof(DomainSite), collection: self.ExemptNames);
        Assert.Equal(expected: 3, actual: DomainSite.Reached());
    }

    [Fact]
    [Law(typeof(Law<>), "witness-mandatory-hold")]
    public void HoldDemandsARefutingWitnessBeforeSampling() {
        // A genuine law: the witness fails the property, the sampled band passes it.
        Spec.Hold(law: Law.Of(
            name: "square-grows-past-one",
            gen: Gen.Double[2.0, 1.0e3],
            property: static x => Spec.Holds(condition: x * x >= x, label: "square must not shrink"),
            refutingWitness: 0.5));
        // Refutation matches the sampler's failure notion: a witness crashing a guard (not an
        // assert) still refutes, because any throw fails a sampled property.
        Spec.Hold(law: Law.Of(
            name: "guarded-sqrt",
            gen: Gen.Double[1.0, 1.0e3],
            property: static x => {
                ArgumentOutOfRangeException.ThrowIfNegative(value: x);
                Spec.Holds(condition: Math.Sqrt(d: x) > 0.0, label: "positive root");
            },
            refutingWitness: -1.0));
        // A tautology: no witness can fail it, so registration itself is the failure.
        XunitException tautology = Assert.Throws<XunitException>(testCode: static () => Spec.Hold(law: Law.Of(
            name: "vacuous",
            gen: Gen.Int[0, 10],
            property: static _ => Spec.Holds(condition: true, label: "unreachable"),
            refutingWitness: 0)));
        Assert.Contains(expectedSubstring: "tautology", actualString: tautology.Message, comparisonType: StringComparison.Ordinal);
        // An empty law table proves nothing and must refuse to run.
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Hold<int>());
    }

    [Fact]
    [Law(typeof(Law<>), "algebra-row-constructors")]
    public void AlgebraRowsCarryTheirOwnRefutation() {
        // Strict `==` makes each row genuinely falsifiable: the finite gen passes, and a NaN witness
        // — outside the gen — refutes because every operator propagates NaN and NaN != NaN under `==`.
        static bool strict(double a, double b) => a == b;
        Spec.Hold(
            Law.Identity(name: "times-one-identity", gen: Gen.Double[-1.0e3, 1.0e3], f: static x => x * 1.0, witness: double.NaN, eq: strict),
            Law.Idempotent(name: "clamp-idempotent", gen: Gen.Double[-1.0e3, 1.0e3], f: static x => Math.Clamp(value: x, min: -1.0, max: 1.0), witness: double.NaN, eq: strict),
            Law.Inverse(name: "double-halve-inverse", gen: Gen.Double[-1.0e3, 1.0e3], f: static x => x * 2.0, g: static x => x / 2.0, witness: double.NaN, eq: strict),
            Law.Roundtrip(name: "negate-roundtrips", gen: Gen.Double[-1.0e3, 1.0e3], forward: static x => -x, back: static x => -x, witness: double.NaN, eq: strict));
        // Max/Min form an exact IEEE lattice: associativity and Max-over-Min distributivity hold
        // bit-for-bit on the finite band, and the NaN witness refutes both under strict ==.
        Spec.Hold(
            Law.Associative(name: "max-associates", gen: Gen.Double[-1.0e3, 1.0e3], op: Math.Max, witness: (A: double.NaN, B: 1.0, C: 2.0), eq: strict),
            Law.Distributive(name: "max-distributes-over-min", gen: Gen.Double[-1.0e3, 1.0e3], mul: Math.Max, add: Math.Min, witness: (A: double.NaN, B: 1.0, C: 2.0), eq: strict));
        // The permutation row shares one shuffle walk; the witness is a non-permutation pair the
        // exact integer projection genuinely splits.
        Spec.Hold(law: Law.Permutation(name: "max-permutation-invariant", gen: Gens.NonEmptyArray(element: Gen.Int[-1000, 1000], max: 16),
            f: static xs => xs.Max(), witness: (Source: [1, 2], Shuffled: [3])));
        // A monotone row refutes on an out-of-order pair the OrderedPair gen never emits — no NaN needed.
        Spec.Hold(law: Law.Monotone(name: "identity-monotone", pairs: Gens.OrderedPair(Gen.Double[-1.0e3, 1.0e3]), projection: static x => x, witness: (Lo: 5.0, Hi: 3.0)));
        Spec.Hold(law: Law.Commutative(name: "max-commutes", gen: Gen.Double[-1.0e3, 1.0e3], op: Math.Max, witness: (A: double.NaN, B: 1.0), eq: strict));
        // The commutative row is result-typed: a symmetric projection that is not a closed op rides
        // the same row; the NaN witness refutes under strict ==, which Equals semantics would pass.
        Spec.Hold(law: Law.Commutative(name: "sorted-pair-symmetric", gen: Gen.Double[-1.0e3, 1.0e3],
            op: static (a, b) => (Lo: Math.Min(val1: a, val2: b), Hi: Math.Max(val1: a, val2: b)),
            witness: (A: double.NaN, B: 1.0),
            eq: static (l, r) => l.Lo == r.Lo && l.Hi == r.Hi));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Metamorphic), Member = nameof(Spec.Metamorphic))]
    public void MetamorphicFamilyChecksEveryRelationAndRefusesEmptyTables() {
        Spec.Metamorphic(gen: Gen.Int[-1000, 1000], f: static x => x * 2,
            new MetamorphicRelation<int, int>(Name: "negate", Transform: static x => -x, Relate: static (_, @base, follow) => @base == -follow),
            new MetamorphicRelation<int, int>(Name: "shift", Transform: static x => x + 1, Relate: static (_, @base, follow) => follow == @base + 2),
            new MetamorphicRelation<int, int>(Name: "oracle", Transform: static x => x, Relate: static (source, @base, _) => @base == source * 2));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Metamorphic(gen: Gen.Int[0, 1], f: static x => x));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.RoundtripBytes), Member = nameof(Spec.RoundtripBytes))]
    public void RoundtripBytesProvesByteIdentityAndRefusesADriftingCodec() {
        Spec.RoundtripBytes(gen: Rows, contract: SampleContext.Default.SampleRow, iter: 50);
        // The falsification lane: a read-mangling codec re-encodes different bytes and must fail.
        _ = Assert.ThrowsAny<Exception>(testCode: static () =>
            Spec.RoundtripBytes(gen: Gen.Const(value: new DriftRow(Tag: "a")), contract: SampleContext.Default.DriftRow, iter: 1));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Replay), Member = nameof(Spec.Replay))]
    public void ReplayRefailsThePinnedSeedDeterministically() {
        // The sampler's failure banner carries the shrunk seed as its first quoted token; Replay
        // must re-fail on the pinned seed and pass a law the seeded case satisfies.
        CsCheckException failure = Assert.Throws<CsCheckException>(testCode: static () =>
            Spec.ForAll(gen: Gen.Int[0, 63], property: static x => Spec.Holds(condition: x != 7, label: "planted defect"), iter: 4000, threads: 1));
        string seed = failure.Message.Split('"')[1];
        _ = Assert.ThrowsAny<Exception>(testCode: () =>
            Spec.Replay(gen: Gen.Int[0, 63], property: static x => Spec.Holds(condition: x != 7, label: "planted defect"), seed: seed));
        Spec.Replay(gen: Gen.Int[0, 63], property: static x => Spec.Holds(condition: x is >= 0 and <= 63, label: "band"), seed: seed);
        // The law-shaped overload replays the sampled property off the row without re-refuting.
        Spec.Replay(law: Law.Of(name: "band-law", gen: Gen.Int[0, 63],
            property: static x => Spec.Holds(condition: x is >= 0 and <= 63, label: "band"), refutingWitness: -1), seed: seed);
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.ModelBased), Member = nameof(Spec.ModelBased))]
    public void ModelBasedThreadsActualAgainstModel() =>
        // Actual and model are both mutable cells so each operation advances both; the actual-vs-model
        // equality across the operation sequence IS the stateful law CsCheck shrinks against.
        Spec.ModelBased(
            init: Gen.Int[0, 100].Select(static seed => (Actual: Atom(seed), Model: Atom(seed))),
            equal: static (actual, model) => actual.Value == model.Value,
            operations: [TrackedAdd()],
            iter: 25);

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Distributed), Member = nameof(Spec.Distributed))]
    public void DistributedGatesGeneratorSkewByChiSquared() {
        Spec.Distributed(gen: Gen.Int[0, 3], bucket: static x => x, expected: [1000, 1000, 1000, 1000]);
        // A fair generator against a 9:1 expectation fails chi-squared decisively — the gate rejects.
        _ = Assert.ThrowsAny<Exception>(testCode: static () => Spec.Distributed(gen: Gen.Int[0, 1], bucket: static x => x, expected: [9000, 1000]));
        // A one-bucket expectation proves nothing and must refuse to run.
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Distributed(gen: Gen.Int[0, 0], bucket: static x => x, expected: [100]));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Matrix), Member = nameof(Spec.Matrix))]
    public void MatrixNamesTheLyingRowAndRefusesEmptyTables() {
        XunitException lie = Assert.Throws<XunitException>(testCode: static () =>
            Spec.Matrix((Label: "lying-probe", Probe: static () => false, Expected: true)));
        Assert.Contains(expectedSubstring: "lying-probe", actualString: lie.Message, comparisonType: StringComparison.Ordinal);
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Matrix());
    }

    [Fact]
    [Law(typeof(Spec), "rail-gate-polarity")]
    public void RailGatesFailOnTheWrongSide() {
        Spec.Succ(result: Fin.Succ(value: 1));
        Spec.FailCategory(result: Fin.Fail<int>(error: new Fault.Missing()), category: nameof(Fault.Missing));
        Spec.Some(result: Some(value: 1));
        Spec.None(result: Option<int>.None);
        Spec.Valid(result: Success<Error, int>(value: 1));
        Spec.Invalid(result: Fail<Error, int>(value: new Fault.Rejected()));
        Spec.AllErrors(v: Fail<Error, int>(value: Error.Many(new Fault.Missing(), new Fault.Conflict())), nameof(Fault.Conflict), nameof(Fault.Missing));
        Spec.FailMany(result: Fin.Fail<int>(error: Error.Many(Error.New(message: "alpha"), Error.New(message: "beta"))), expectedCount: 2, "alpha", "beta");
        Assert.Equal(expected: 1, actual: Spec.SuccValue(result: Fin.Succ(value: 1), label: "succ-value"));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Succ(result: Fin.Fail<int>(error: new Fault.Rejected())));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Fail(result: Fin.Succ(value: 1)));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Some(result: Option<int>.None));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.None(result: Some(value: 1)));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Valid(result: Fail<Error, int>(value: new Fault.Cancelled())));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Invalid(result: Success<Error, int>(value: 1)));
        // Every rail gate must also fail on the WRONG value, not merely the wrong rail: a drifted
        // category, a drifted error-set, a drifted count, and an unwrapped failure all reject.
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.SuccValue(result: Fin.Fail<int>(error: new Fault.Missing()), label: "succ-value"));
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.FailCategory(result: Fin.Fail<int>(error: new Fault.Missing()), category: nameof(Fault.Rejected)));
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.AllErrors(v: Success<Error, int>(value: 1), nameof(Fault.Missing)));
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.AllErrors(v: Fail<Error, int>(value: new Fault.Missing()), nameof(Fault.Conflict)));
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.FailMany(result: Fin.Fail<int>(error: Error.Many(Error.New(message: "alpha"), Error.New(message: "beta"))), expectedCount: 3, "alpha"));
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.ManyErrors(error: Error.New(message: "single"), expectedCount: 1));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.FinChainOrder), Member = nameof(Spec.FinChainOrder))]
    public void FinChainOrderDemandsCommutingRailsAndRefutesAsymmetry() {
        // Power-of-two scalings commute exactly; a both-fail pair shares the failure lane even
        // when the short-circuiting order surfaces different errors.
        Spec.FinChainOrder(gen: Gen.Double[-1.0e3, 1.0e3], first: static x => Fin.Succ(value: x * 2.0), second: static x => Fin.Succ(value: x * 4.0));
        Spec.FinChainOrder(gen: Gen.Double[-1.0e3, -1.0],
            first: static _ => Fin.Fail<double>(error: new Fault.Missing()),
            second: static _ => Fin.Fail<double>(error: new Fault.Rejected()));
        // An order-dependent pair refutes: the guard admits the raw value and rejects the shifted one.
        _ = Assert.ThrowsAny<Exception>(testCode: static () => Spec.FinChainOrder(
            gen: Gen.Double[1.0, 100.0],
            first: static x => Fin.Succ(value: x + 1000.0),
            second: static x => x < 500.0 ? Fin.Succ(value: x) : Fin.Fail<double>(error: new Fault.Rejected())));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Classified), Member = nameof(Spec.Classified))]
    public void ClassifiedPrintsEveryBucketOverOneSweep() {
        List<string> lines = [];
        Spec.Classified(gen: Gen.Int[0, 9], classify: static value => value < 5 ? "low" : "high", writeLine: lines.Add, iter: 200);
        Assert.NotEmpty(collection: lines);
        Assert.Contains(collection: lines, filter: static line => line.Contains(value: "low", comparisonType: StringComparison.Ordinal));
        Assert.Contains(collection: lines, filter: static line => line.Contains(value: "high", comparisonType: StringComparison.Ordinal));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Parallel), Member = nameof(Spec.Parallel))]
    public void ParallelChecksLinearizabilityAndSurfacesFailingOperations() {
        // Atomic Swap is linearizable, so the actual-vs-model pairing holds under every interleaving.
        Spec.Parallel(
            init: Gen.Int[0, 10].Select(static seed => (Actual: Atom(seed), Model: Atom(seed))),
            equal: static (actual, model) => actual.Value == model.Value,
            operations: [TrackedAdd()]);
        // A throwing operation is surfaced by the parallel sampler, never swallowed.
        _ = Assert.ThrowsAny<Exception>(testCode: static () => Spec.Parallel(
            init: Gen.Const(value: Atom(0)),
            operations: [GenOperation.Create<Atom<int>, int>(
                gen: Gen.Int[1, 3],
                name: static delta => string.Create(provider: System.Globalization.CultureInfo.InvariantCulture, $"boom({delta})"),
                action: static (_, _) => throw new InvalidOperationException(message: "parallel op failure"))]));
        // An empty operation table proves nothing and must refuse to run.
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Parallel(init: Gen.Const(value: Atom(0)), operations: []));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Parallel(
            init: Gen.Const(value: (Actual: Atom(0), Model: Atom(0))),
            equal: static (actual, model) => actual.Value == model.Value,
            operations: []));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.DualPath), Member = nameof(Spec.DualPath))]
    public void DualPathDemandsConvergentMutationsAndRefutesDivergence() {
        Spec.DualPath(
            initial: Gen.Int[0, 100].Select(static seed => Atom(seed)),
            paramGen: Gen.Int[1, 10],
            name: static delta => string.Create(provider: System.Globalization.CultureInfo.InvariantCulture, $"add({delta})"),
            path1: static (cell, delta) => _ = cell.Swap(state => state + delta),
            path2: static (cell, delta) => _ = cell.Swap(state => state + delta),
            equal: static (left, right) => left.Value == right.Value);
        _ = Assert.ThrowsAny<Exception>(testCode: static () => Spec.DualPath(
            initial: Gen.Int[0, 100].Select(static seed => Atom(seed)),
            paramGen: Gen.Int[1, 10],
            name: static delta => string.Create(provider: System.Globalization.CultureInfo.InvariantCulture, $"drift({delta})"),
            path1: static (cell, delta) => _ = cell.Swap(state => state + delta),
            path2: static (cell, delta) => _ = cell.Swap(state => state + delta + 1),
            equal: static (left, right) => left.Value == right.Value));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Catalog), Member = nameof(Spec.Catalog))]
    public void CatalogGatesKeyUniquenessAndExactMembership() {
        Spec.Catalog(items: (string[])["alpha", "beta"], expectedKeys: ["alpha", "beta"], key: static item => item, law: static item => Assert.NotEqual(expected: "", actual: item));
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.Catalog(items: (string[])["alpha", "alpha"], expectedKeys: ["alpha", "alpha"], key: static item => item));
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.Catalog(items: (string[])["alpha", "beta"], expectedKeys: ["alpha", "gamma"], key: static item => item));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Family), Member = nameof(Spec.Family))]
    public void FamilyDemandsAdmissionAndRejectionTogether() {
        static bool TryPositive(int value, out int owned) { owned = value; return value > 0; }
        static bool AdmitAll(int value, out int owned) { owned = value; return true; }
        Spec.Family(new Spec.ValueObjectShape<int, int>(Valid: Gen.Int[1, 100], Invalid: Gen.Int[-100, 0], TryCreate: TryPositive, Read: static value => value));
        // An admit-everything TryCreate is refuted by the invalid lane — a one-sided family proves
        // nothing; the sampler surfaces the violation as a CsCheck failure carrying the witness.
        _ = Assert.ThrowsAny<Exception>(testCode: static () => Spec.Family(
            new Spec.ValueObjectShape<int, int>(Valid: Gen.Int[1, 10], Invalid: Gen.Int[-10, 0], TryCreate: AdmitAll, Read: static value => value)));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.CountsConserve), Member = nameof(Spec.CountsConserve))]
    public void CountConservationRefusesLeaksAndNegatives() {
        Spec.CountsConserve(attempted: 5, emitted: 3, rejected: 2, label: "conserved");
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.CountsConserve(attempted: 5, emitted: 3, rejected: 1, label: "leak"));
        _ = Assert.ThrowsAny<XunitException>(testCode: static () => Spec.CountsConserve(attempted: -1, emitted: 0, rejected: 0, label: "negative"));
    }

    [Fact]
    [Law(typeof(Spec), "cscheck-hash-regression")]
    public void HashPinsARegressionValueAndRefutesDrift() {
        // First run (expected 0) reports the discovered hash by throwing; the pinned value then
        // passes and any drifted expectation fails — no hardcoded literal survives in the law.
        static void Fold(Hash hash) { hash.Add(Math.PI); hash.Add("kit"); hash.Add(42); }
        CsCheckException discovery = Assert.Throws<CsCheckException>(testCode: static () => Check.Hash(action: Fold, expected: 0));
        long pinned = long.Parse(s: discovery.Message.Split(' ')[^1], provider: System.Globalization.CultureInfo.InvariantCulture);
        Check.Hash(action: Fold, expected: pinned);
        _ = Assert.Throws<CsCheckException>(testCode: () => Check.Hash(action: Fold, expected: pinned == 0L ? 1L : pinned + 1L));
    }

    [Fact]
    [Law(typeof(Spec), "cscheck-faster-comparison")]
    public void FasterDetectsAsymmetryAndRefutesTheInversion() {
        // A spot performance law over a 10^4 workload asymmetry; never a BenchmarkDotNet replacement.
        // The void workload keeps the lambdas on the Action overload — a value-returning lambda
        // selects the Func<T> overload, which gates return-value equality instead of timing.
        static void Burn(int count) => _ = Enumerable.Range(start: 1, count: count).Sum(selector: static i => Math.Sqrt(i));
        Check.Faster(faster: static () => Burn(10), slower: static () => Burn(100_000), threads: 1, timeout: 30);
        _ = Assert.Throws<CsCheckException>(testCode: static () =>
            Check.Faster(faster: static () => Burn(100_000), slower: static () => Burn(10), threads: 1, timeout: 30));
    }

    [Fact]
    [Law(typeof(Manifests), nameof(Manifests.ProjectGraph), Member = nameof(Manifests.ProjectGraph))]
    public void ProjectGraphRefusesEmptyTablesAndNamesDrift() {
        _ = Assert.Throws<XunitException>(testCode: static () => Manifests.ProjectGraph());
        XunitException drift = Assert.Throws<XunitException>(testCode: static () => Manifests.ProjectGraph(
            (Project: "tests/csharp/_testkit/Rasm.TestKit.csproj", References: ["libs/csharp/Phantom/Phantom.csproj"])));
        Assert.Contains(expectedSubstring: "Rasm.TestKit.csproj", actualString: drift.Message, comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    [Law(typeof(Metric), "metric-row-dispatch")]
    public void SignAmbiguityIsAMetricRowNotASiblingMethod() {
        double[] axis = [1.0, -2.0, 3.0];
        double[] flipped = [-1.0, 2.0, -3.0];
        Spec.Equal(left: axis, right: flipped, tolerance: Tolerance.Default, metric: Metric.SignAmbiguous, what: "eigenaxis");
        _ = Assert.Throws<XunitException>(testCode: () => Spec.Equal(left: (ReadOnlySpan<double>)axis, right: flipped, tolerance: Tolerance.Default, what: "eigenaxis-absolute"));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Equal(left: double.NaN, right: double.NaN, what: "nan-never-equal"));
    }

    [Fact]
    [Law(typeof(Tolerance), "ulps-regime")]
    public void UlpBudgetAdmitsBitAdjacencyAndNothingNonFinite() {
        double basis = 1.0 / 3.0;
        Spec.Matrix(
            (Label: "one-ulp-adjacent", Probe: () => Approx.Equal(left: basis, right: Math.BitIncrement(x: basis), tolerance: Tolerance.WithinUlps(units: 1L)), Expected: true),
            (Label: "two-ulp-rejected", Probe: () => Approx.Equal(left: basis, right: Math.BitIncrement(x: Math.BitIncrement(x: basis)), tolerance: Tolerance.WithinUlps(units: 1L)), Expected: false),
            (Label: "zero-budget-disables", Probe: () => Approx.Equal(left: basis, right: Math.BitIncrement(x: basis), tolerance: new Tolerance(Abs: 0.0, Rel: 0.0)), Expected: false),
            (Label: "sign-straddle-counts-through-zero", Probe: static () => Approx.Equal(left: -double.Epsilon, right: double.Epsilon, tolerance: Tolerance.WithinUlps(units: 2L)), Expected: true),
            (Label: "signed-zero-coincides", Probe: static () => Approx.Equal(left: 0.0, right: -0.0, tolerance: Tolerance.WithinUlps(units: 1L)), Expected: true),
            (Label: "nan-never", Probe: static () => Approx.Equal(left: double.NaN, right: double.NaN, tolerance: Tolerance.WithinUlps(units: long.MaxValue)), Expected: false),
            (Label: "infinity-never", Probe: static () => Approx.Equal(left: double.PositiveInfinity, right: double.PositiveInfinity, tolerance: Tolerance.WithinUlps(units: long.MaxValue)), Expected: false));
        // The scalar gate reaches every Tolerance regime without span ceremony, both polarities.
        Spec.Equal(left: basis, right: Math.BitIncrement(x: basis), tolerance: Tolerance.WithinUlps(units: 1L), what: "one-ulp-gate");
        _ = Assert.Throws<XunitException>(testCode: () =>
            Spec.Equal(left: basis, right: Math.BitIncrement(x: Math.BitIncrement(x: basis)), tolerance: Tolerance.WithinUlps(units: 1L), what: "two-ulp-gate"));
    }

    [Fact]
    [Law(typeof(Tolerance), "magnitude-regimes")]
    public void MagnitudeRegimesScaleTheirAdmissionWindows() {
        Spec.Matrix(
            (Label: "relative-scales-with-magnitude", Probe: static () => Approx.Equal(left: 1.0e6, right: 1.0e6 + 0.5, tolerance: Tolerance.Relative(epsilon: 1.0e-6)), Expected: true),
            (Label: "relative-rejects-at-unit-scale", Probe: static () => Approx.Equal(left: 1.0, right: 1.5, tolerance: Tolerance.Relative(epsilon: 1.0e-6)), Expected: false),
            (Label: "absolute-ignores-magnitude", Probe: static () => Approx.Equal(left: 1.0e6, right: 1.0e6 + 1.0, tolerance: Tolerance.Absolute(epsilon: 1.0e-9)), Expected: false),
            (Label: "hybrid-floor-admits-near-zero", Probe: static () => Approx.Equal(left: 0.0, right: 5.0e-13, tolerance: Tolerance.Hybrid(absolute: 1.0e-12, relative: 1.0e-9)), Expected: true),
            (Label: "pure-relative-starves-near-zero", Probe: static () => Approx.Equal(left: 0.0, right: 5.0e-13, tolerance: Tolerance.Relative(epsilon: 1.0e-15)), Expected: false),
            // The Seq call shape reaches the same regime dispatch as scalar and span.
            (Label: "seq-shape-admits", Probe: static () => Approx.Equal(left: Seq(1.0, 2.0), right: Seq(1.0, 2.0), tolerance: Tolerance.Default), Expected: true),
            (Label: "seq-length-mismatch-rejects", Probe: static () => Approx.Equal(left: Seq(1.0), right: Seq(1.0, 2.0), tolerance: Tolerance.Default), Expected: false));
        Spec.Equal(left: Seq(1.0, 2.0), right: Seq(1.0, 2.0), tolerance: Tolerance.Default, what: "seq gate");
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Equal(left: Seq(1.0), right: Seq(1.0, 2.0), tolerance: Tolerance.Default, what: "seq gate length"));
    }

    [Fact]
    [Law(typeof(Metric), nameof(Metric.Periodic), Member = nameof(Metric.Periodic))]
    public void PeriodicMetricAdmitsModuloThePeriodOnly() {
        Spec.ForAll(gen: Gens.Angle, property: static theta => Spec.Equal(
            left: theta, right: theta + Math.Tau, tolerance: Tolerance.Absolute(epsilon: 1.0e-9), metric: Metric.Periodic(period: Math.Tau), what: "full turn"));
        Spec.Matrix(
            (Label: "half-turn-rejected", Probe: static () => Approx.Equal(left: 0.0, right: Math.PI, tolerance: Tolerance.Absolute(epsilon: 1.0e-9), metric: Metric.Periodic(period: Math.Tau)), Expected: false),
            (Label: "nan-never", Probe: static () => Approx.Equal(left: double.NaN, right: 0.0, tolerance: Tolerance.Absolute(epsilon: 1.0e-9), metric: Metric.Periodic(period: Math.Tau)), Expected: false));
        _ = Assert.Throws<ArgumentOutOfRangeException>(testCode: static () => Metric.Periodic(period: 0.0));
        _ = Assert.Throws<ArgumentOutOfRangeException>(testCode: static () => Metric.Periodic(period: double.PositiveInfinity));
    }

    [Fact]
    [Law(typeof(SeamProbe<>), "seam-typed-log-lifo")]
    public void SeamProbeRecordsTypedCallsAndRestoresLifo() {
        SeamProbe<Unit> probe = new();
        string current = "<unbound>";
        Func<Unit, int>? outer = null;
        Func<Unit, int>? inner = null;
        SeamRestore outerScope = probe.Install(member: "outer", shape: new Shape<int>.Canned(Value: 7), bind: canned => {
            string prior = current;
            (current, outer) = ("outer", canned);
            return () => current = prior;
        });
        SeamRestore innerScope = probe.Install(member: "inner", shape: new Shape<int>.Factory(Value: 9), bind: canned => {
            string prior = current;
            (current, inner) = ("inner", canned);
            return () => current = prior;
        });
        Assert.Equal(expected: 7, actual: outer!(unit));
        Assert.Equal(expected: 9, actual: inner!(unit));
        Assert.Equal(expected: "inner", actual: current);
        innerScope.Dispose();
        Assert.Equal(expected: "outer", actual: current);
        outerScope.Dispose();
        Assert.Equal(expected: "<unbound>", actual: current);
        // The typed log: canned records the member with a payload, factory records its inner label
        // with none — the payload bag is a typed Option, never an object?/kwargs pair.
        Assert.Equal(expected: 2, actual: probe.Calls.Count);
        Assert.Equal(expected: new SeamCall<Unit>(Member: "outer", Payload: Some(value: unit)), actual: probe.Calls[0]);
        Assert.Equal(expected: new SeamCall<Unit>(Member: "<factory>.run", Payload: Option<Unit>.None), actual: probe.Calls[1]);
        Assert.Equal(expected: 1, actual: probe.Payloads.Count);
    }

    [Fact]
    [Law(typeof(Shape<>), "seam-shape-switch")]
    public void ShapeSwitchDispatchesEveryCase() {
        static string Render(Shape<int> shape) => shape.Switch(
            canned: static s => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"canned:{s.Value}"),
            fanOut: static s => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"fan:{s.Values.Count}"),
            factory: static s => $"factory:{s.InnerLabel}");
        Assert.Equal(expected: "canned:1", actual: Render(new Shape<int>.Canned(Value: 1)));
        Assert.Equal(expected: "fan:3", actual: Render(new Shape<int>.FanOut(Values: Seq(0, 0, 0))));
        Assert.Equal(expected: "factory:run.x", actual: Render(new Shape<int>.Factory(Value: 4, InnerLabel: "run.x")));
    }

    [Fact]
    [Law(typeof(SeamProbe<>), "seam-fanout-walk")]
    public void FanOutWalksItsValuesPerCallAndExhaustsLoudly() {
        SeamProbe<Unit> probe = new();
        Func<Unit, int> seam = static _ => 0;
        using SeamRestore scope = probe.Install(member: "walk", shape: new Shape<int>.FanOut(Values: Seq(7, 9)), bind: canned => {
            Func<Unit, int> prior = seam;
            seam = canned;
            return () => seam = prior;
        });
        Assert.Equal(expected: 7, actual: seam(unit));
        Assert.Equal(expected: 9, actual: seam(unit));
        XunitException exhausted = Assert.Throws<XunitException>(testCode: () => seam(unit));
        Assert.Contains(expectedSubstring: "exhausted after 2", actualString: exhausted.Message, comparisonType: StringComparison.Ordinal);
        // Each successful walk step records typed; the exhausted call records nothing.
        Assert.Equal(expected: 2, actual: probe.Calls.Count);
        // A fresh install restarts the walk: the cursor is install-scoped, never shape-scoped.
        Shape<int>.FanOut shared = new(Values: Seq(1));
        SeamProbe<Unit> second = new();
        Func<Unit, int>? replay = null;
        using SeamRestore first = second.Install(member: "a", shape: shared, bind: canned => { replay = canned; return () => { }; });
        Assert.Equal(expected: 1, actual: replay!(unit));
        using SeamRestore next = second.Install(member: "b", shape: shared, bind: canned => { replay = canned; return () => { }; });
        Assert.Equal(expected: 1, actual: replay!(unit));
    }

    [Fact]
    [Law(typeof(NdjsonOracle<>), "ndjson-line-gate")]
    public void NdjsonOracleGatesLineCountForOneAndAll() {
        NdjsonOracle<SampleRow> oracle = new(Decoder: SampleContext.Default.SampleRow, ExpectLines: 1);
        byte[] one = JsonSerializer.SerializeToUtf8Bytes(value: new SampleRow(Tag: "x", Rank: 5), jsonTypeInfo: SampleContext.Default.SampleRow);
        string two = $"{Encoding.UTF8.GetString(one)}\n{Encoding.UTF8.GetString(one)}";
        Assert.Equal(expected: new SampleRow(Tag: "x", Rank: 5), actual: oracle.One(raw: one));
        // The string call shape rides the same gate-and-decode walk as the byte shape.
        Assert.Equal(expected: new SampleRow(Tag: "x", Rank: 5), actual: oracle.One(raw: Encoding.UTF8.GetString(one)));
        _ = Assert.Throws<XunitException>(testCode: () => oracle.One(raw: Encoding.UTF8.GetBytes(two)));
        // All decodes every gated row in order and refuses a drifted line count just like One.
        NdjsonOracle<SampleRow> stream = new(Decoder: SampleContext.Default.SampleRow, ExpectLines: 2);
        Assert.Equal(expected: [new SampleRow(Tag: "x", Rank: 5), new SampleRow(Tag: "x", Rank: 5)], actual: stream.All(raw: two));
        _ = Assert.Throws<XunitException>(testCode: () => stream.All(raw: one));
        // Gate and decode share one segmentation walk: a CRLF-terminated row decodes clean, a
        // doubled trailing newline is a counted empty segment that fails at decode — never a
        // gate-passing payload that silently yields fewer rows than it gated.
        string crlf = $"{Encoding.UTF8.GetString(one)}\r\n";
        Assert.Equal(expected: new SampleRow(Tag: "x", Rank: 5), actual: oracle.One(raw: Encoding.UTF8.GetBytes(crlf)));
        _ = Assert.ThrowsAny<Exception>(testCode: () => stream.All(raw: Encoding.UTF8.GetBytes($"{crlf}\r\n")));
        // ExpectLines 0 with All is the empty-stream assertion, not a vacuous gate.
        Assert.Empty(collection: new NdjsonOracle<SampleRow>(Decoder: SampleContext.Default.SampleRow, ExpectLines: 0).All(raw: ""u8));
    }

    [Fact]
    [Law(typeof(Gens), "geometry-bands")]
    public void GeometryBandsCarryTheirConstructionInvariants() {
        // Directions are unit vectors in every dimension the band serves.
        Spec.ForAll(gen: Gen.Int[1, 8].SelectMany(Gens.Direction), property: static direction =>
            Spec.Equal(left: Math.Sqrt(d: direction.Sum(static x => x * x)), right: 1.0, tolerance: 1.0e-12, what: "unit norm"));
        // Householder products pass the orthogonality residual the kit itself owns.
        Spec.ForAll(gen: Gen.Int[1, 6].SelectMany(n => Gens.Orthogonal(n: n).Select(q => (N: n, Q: q))), property: static t =>
            Spec.Holds(condition: Numeric.OrthogonalityResidual(rows: t.N, cols: t.N, at: (i, j) => t.Q[i][j]) <= 1.0e-10, label: "orthogonal residual"), iter: 40);
        // The parity lane covers both O(n) components: determinant sign splits evenly, where a pure
        // n-reflection product would pin every sample to det = (-1)^n.
        Spec.Distributed(gen: Gens.Orthogonal(n: 3), bucket: static q => Numeric.Determinant(n: 3, at: (i, j) => q[i][j]) > 0.0 ? 0 : 1, expected: [500, 500]);
        // Conditioned matrices are symmetric with trace equal to the constructed spectrum sum.
        Spec.ForAll(gen: Gen.Int[2, 6].SelectMany(n => Gens.Conditioned(n: n, kappa: 1.0e6).Select(m => (N: n, M: m))), property: static t => {
            Spec.Holds(condition: Numeric.SymmetryResidual(dimension: t.N, at: (i, j) => t.M[i][j]) <= 1.0e-10, label: "symmetric");
            double expected = Enumerable.Range(start: 0, count: t.N).Sum(i => Math.Pow(x: 1.0e6, y: -(double)i / (t.N - 1)));
            Spec.Equal(left: Enumerable.Range(start: 0, count: t.N).Sum(i => t.M[i][i]), right: expected, tolerance: 1.0e-8, what: "trace = spectrum sum");
        }, iter: 40);
        // Rings are star-shaped and CCW by construction: shoelace area is strictly positive and
        // reversal negates it up to accumulation order over the all-positive term stream.
        Spec.ForAll(gen: Gen.Int[3, 12].SelectMany(Gens.Ring), property: static ring => {
            double area = Numeric.ShoelaceArea(ring: ring);
            Spec.Holds(condition: area > 0.0, label: "ccw ring area positive");
            Spec.Equal(left: Numeric.ShoelaceArea(ring: [.. ring.Reverse()]), right: -area, tolerance: Math.Abs(value: area) * 1.0e-12, what: "reversal negates");
        }, iter: 100);
        _ = Assert.Throws<ArgumentOutOfRangeException>(testCode: static () => Gens.Direction(dim: 0));
        _ = Assert.Throws<ArgumentOutOfRangeException>(testCode: static () => Gens.NearCollinear(dim: 1));
        _ = Assert.Throws<ArgumentOutOfRangeException>(testCode: static () => Gens.Conditioned(n: 2, kappa: 0.5));
        _ = Assert.Throws<ArgumentOutOfRangeException>(testCode: static () => Gens.Ring(vertices: 2));
    }

    [Fact]
    [Law(typeof(Gens), "scalar-bands")]
    public void ScalarBandsCarryTheirConstructionInvariants() {
        Spec.ForAll(gen: Gen.Int[1, 8].SelectMany(count => Gens.Simplex(count: count).Select(weights => (Count: count, Weights: weights))), property: static t => {
            Assert.Equal(expected: t.Count, actual: t.Weights.Count);
            Spec.Holds(condition: t.Weights.Filter(static weight => weight <= 0.0).IsEmpty, label: "simplex weights are strictly positive");
            Spec.Equal(left: Numeric.Sum(values: t.Weights), right: 1.0, what: "simplex mass");
        }, iter: 100);
        // The cancellation band keeps its relative offset inside the annihilation window; a pair
        // rounding to identity is the extreme of the band, never a violation.
        Spec.ForAll(gen: Gens.Cancellation, property: static pair => {
            Spec.Holds(condition: double.IsFinite(d: pair.X) && double.IsFinite(d: pair.Y), label: "cancellation pair finite");
            Spec.Holds(condition: Math.Abs(value: pair.Y - pair.X) <= Math.Abs(value: pair.X) * 1.0e-7, label: "cancellation offset stays in the band");
        });
        Spec.ForAll(gen: Gens.UnitClosed, property: static u => Spec.Holds(condition: u is >= 0.0 and <= 1.0, label: "unit-closed band"));
        Spec.ForAll(gen: Gens.Angle, property: static theta => Spec.Holds(condition: theta is >= -Math.Tau and <= Math.Tau, label: "angle band"));
        Spec.ForAll(gen: Gens.Positive, property: static x => Spec.Holds(condition: x > 0.0 && double.IsFinite(d: x), label: "positive band"));
        Spec.ForAll(gen: Gens.Finite, property: static x => Spec.Holds(condition: double.IsFinite(d: x), label: "finite band"));
        Spec.ForAll(gen: Gens.NonFinite, property: static x => Spec.Holds(condition: !double.IsFinite(d: x), label: "non-finite band"));
        Spec.ForAll(gen: Gens.Tame, property: static x => Spec.Holds(condition: x is >= -1.0e6 and <= 1.0e6, label: "tame band"));
        Spec.Distributed(gen: Gens.AnyDouble, bucket: static x => double.IsFinite(d: x) ? 0 : 1, expected: [9500, 500]);
        _ = Assert.Throws<ArgumentOutOfRangeException>(testCode: static () => Gens.Simplex(count: 0));
    }

    // Collection and admission bands carry checkable construction claims: order, uniqueness,
    // distinctness, size windows, key grammar, and predicate-filtered admission.
    [Fact]
    [Law(typeof(Gens), "collection-bands")]
    public void CollectionBandsCarryTheirConstructionInvariants() {
        static bool TryEven(int value, out int owned) { owned = value; return (value & 1) == 0; }
        Spec.ForAll(gen: Gens.SortedArray(element: Gens.IntEdges), property: static xs =>
            Spec.Holds(condition: xs.Length <= 32 && xs.SequenceEqual(second: xs.Order()), label: "sorted small band"));
        Spec.ForAll(gen: Gens.UniqueArray(element: Gen.Int[0, 1000]), property: static xs =>
            Spec.Holds(condition: xs.Length is >= 1 and <= 64 && xs.ToHashSet().Count == xs.Length, label: "unique band"));
        Spec.ForAll(gen: Gens.DistinctTriple(element: Gen.Int[0, 100]), property: static t =>
            Spec.Holds(condition: t.A != t.B && t.B != t.C && t.A != t.C, label: "distinct triple"));
        Spec.ForAll(gen: Gens.OrderedPair(element: Gens.Finite), property: static p =>
            Spec.Holds(condition: p.Lo.CompareTo(p.Hi) <= 0, label: "ordered pair"));
        Spec.ForAll(gen: Gens.NonEmptySeq(element: Gen.Int[0, 9], max: 8), property: static xs =>
            Spec.Holds(condition: !xs.IsEmpty && xs.Count <= 8, label: "non-empty seq band"));
        Spec.ForAll(gen: Gens.SeqOf(element: Gen.Int[0, 9], max: 4), property: static xs =>
            Spec.Holds(condition: xs.Count <= 4, label: "seq band"));
        Spec.ForAll(gen: Gens.LargeArray(element: Gen.Bool), property: static xs =>
            Spec.Holds(condition: xs.Length is >= 1_000 and <= 10_000, label: "large band"), iter: 5);
        Spec.ForAll(gen: Gens.Key, property: static key =>
            Spec.Holds(condition: key.Length is >= 1 and <= 32 && key.All(predicate: static c => c is >= 'a' and <= 'z'), label: "key band"));
        // Admitted yields only TryCreate-admitted values; rejects ride the filtered lane, never a throw.
        Spec.ForAll(gen: Gens.Admitted<int, int>(source: Gen.Int[0, 1000], tryCreate: TryEven), property: static x =>
            Spec.Holds(condition: (x & 1) == 0, label: "admitted band"));
    }

    [Fact]
    [Law(typeof(Gens), "rail-error-lanes")]
    public void RailErrorLanesKeepTheExpectedExceptionalSplit() {
        Spec.ForAll(gen: Gens.Faults, property: static error => Spec.Holds(condition: error.IsExpected, label: "faults stay expected"));
        Spec.ForAll(gen: Gens.Exceptional, property: static error => Spec.Holds(condition: error.IsExceptional, label: "exceptional lane carries live exceptions"));
    }

    // Chi-squared over non-default weights proves the lane knobs actually steer the distribution.
    [Fact]
    [Law(typeof(Gens), "rail-lane-weights")]
    public void RailGeneratorsHonorTheirLaneWeights() {
        Spec.Distributed(gen: Gens.FinOf(succ: Gen.Int[0, 10], succWeight: 70), bucket: static fin => fin.IsSucc ? 0 : 1, expected: [7000, 3000]);
        Spec.Distributed(gen: Gens.OptionOf(some: Gen.Int[0, 10], someWeight: 50), bucket: static option => option.IsSome ? 0 : 1, expected: [5000, 5000]);
        Spec.Distributed(gen: Gens.ValidationOf(succ: Gen.Int[0, 10]), bucket: static v => v.IsSuccess ? 0 : 1, expected: [5000, 5000]);
    }

    [Fact]
    [Law(typeof(Numeric), nameof(Numeric.OrientSign), Member = nameof(Numeric.OrientSign))]
    public void OrientSignIsExactOnTheCollinearTortureBand() {
        // Closed-form anchors: CCW positive, swap negates, exact collinearity is exactly zero.
        Assert.Equal(expected: 1, actual: Numeric.OrientSign(simplex: [[0.0, 0.0], [1.0, 0.0], [0.0, 1.0]]));
        Assert.Equal(expected: -1, actual: Numeric.OrientSign(simplex: [[1.0, 0.0], [0.0, 0.0], [0.0, 1.0]]));
        Assert.Equal(expected: 0, actual: Numeric.OrientSign(simplex: [[0.0, 0.0], [1.0, 1.0], [2.0, 2.0]]));
        Assert.Equal(expected: 1, actual: Numeric.OrientSign(simplex: [[0.0, 0.0, 0.0], [1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0]]));
        Assert.Equal(expected: -1, actual: Numeric.OrientSign(simplex: [[0.0, 0.0, 0.0], [0.0, 1.0, 0.0], [1.0, 0.0, 0.0], [0.0, 0.0, 1.0]]));
        // Antisymmetry and cyclic invariance are exact integer determinant identities, so they hold
        // even where the double-precision determinant misjudges the near-collinear band.
        Spec.ForAll(gen: Gens.NearCollinear(dim: 2), property: static t => {
            int sign = Numeric.OrientSign(simplex: [t.A, t.B, t.C]);
            Assert.Equal(expected: -sign, actual: Numeric.OrientSign(simplex: [t.B, t.A, t.C]));
            Assert.Equal(expected: sign, actual: Numeric.OrientSign(simplex: [t.B, t.C, t.A]));
        }, iter: 200);
        _ = Assert.Throws<ArgumentException>(testCode: static () => Numeric.OrientSign(simplex: [[0.0, 0.0], [1.0, 1.0]]));
        _ = Assert.Throws<ArgumentException>(testCode: static () => Numeric.OrientSign(simplex: [[double.NaN, 0.0], [1.0, 0.0], [0.0, 1.0]]));
    }

    [Fact]
    [Law(typeof(Numeric), "geometry-oracles")]
    public void AreaAndVolumeOraclesMatchClosedForms() {
        double[][] square = [[0.0, 0.0], [1.0, 0.0], [1.0, 1.0], [0.0, 1.0]];
        Spec.Equal(left: Numeric.ShoelaceArea(ring: square), right: 1.0, what: "ccw unit square");
        Spec.Equal(left: Numeric.ShoelaceArea(ring: [.. square.Reverse()]), right: -1.0, what: "cw negates");
        Assert.True(condition: double.IsNaN(d: Numeric.ShoelaceArea(ring: [[1.0], [2.0]])), userMessage: "malformed ring is NaN");
        Spec.Equal(left: Numeric.SignedTetraVolume(a: [1.0, 0.0, 0.0], b: [0.0, 1.0, 0.0], c: [0.0, 0.0, 1.0]), right: 1.0 / 6.0, what: "unit tetra");
        double[][] cube = [[0, 0, 0], [1, 0, 0], [1, 1, 0], [0, 1, 0], [0, 0, 1], [1, 0, 1], [1, 1, 1], [0, 1, 1]];
        int[][] outward = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [1, 2, 6, 5], [2, 3, 7, 6], [3, 0, 4, 7]];
        Spec.Equal(left: Numeric.SignedVolume(vertices: cube, faces: outward), right: 1.0, what: "unit cube via quad fans");
        Spec.Equal(left: Numeric.SignedVolume(vertices: cube, faces: [.. outward.Select(face => (int[])[.. face.Reverse()])]), right: -1.0, what: "inverted orientation negates");
        Assert.True(condition: double.IsNaN(d: Numeric.SignedVolume(vertices: cube, faces: [[0, 1, 99]])), userMessage: "out-of-range index is NaN");
    }

    [Fact]
    [Law(typeof(Numeric), "point-moment-oracles")]
    public void PointMomentOraclesMatchClosedForms() {
        Spec.Equal(left: Numeric.Centroid(points: [[0.0, 0.0], [2.0, 0.0], [2.0, 2.0], [0.0, 2.0]]), right: [1.0, 1.0], tolerance: Tolerance.Default, what: "uniform centroid");
        Spec.Equal(left: Numeric.Centroid(points: [[0.0], [10.0]], weights: [3.0, 1.0]), right: [2.5], tolerance: Tolerance.Default, what: "weighted centroid");
        // Central second moments of the segment {(0,0),(2,0)} under uniform mass, packed upper row-major.
        Spec.Equal(left: Numeric.CovarianceUpper(points: [[0.0, 0.0], [2.0, 0.0]]), right: [1.0, 0.0, 0.0], tolerance: Tolerance.Default, what: "covariance upper triangle");
        Spec.Equal(left: Numeric.ArcLength(points: [[0.0, 0.0], [3.0, 4.0], [3.0, 4.0]]), right: 5.0, what: "polyline arc length");
        (double min, double mean, double max) = Numeric.PairwiseDistances(points: [[0.0, 0.0], [1.0, 0.0], [1.0, 1.0], [0.0, 1.0]]);
        Spec.Equal(left: min, right: 1.0, what: "unit square min pair");
        Spec.Equal(left: mean, right: (4.0 + (2.0 * Math.Sqrt(d: 2.0))) / 6.0, what: "unit square mean pair");
        Spec.Equal(left: max, right: Math.Sqrt(d: 2.0), what: "unit square max pair");
        Spec.Equal(left: Numeric.ConvergenceOrder(coarseError: 1.0e-2, fineError: 2.5e-3), right: 2.0, what: "second-order convergence");
        Spec.Equal(left: Numeric.Sum(values: Seq(1.5, 2.5, -1.0)), right: 3.0, what: "seq fold");
        // NaN lanes are the reachable reds: every degraded input fails the calling gate loudly.
        Spec.Matrix(
            (Label: "weight-count-mismatch-is-nan", Probe: static () => Numeric.Centroid(points: [[1.0]], weights: [1.0, 2.0]).All(predicate: double.IsNaN), Expected: true),
            (Label: "vanishing-mass-is-nan", Probe: static () => Numeric.Centroid(points: [[1.0], [2.0]], weights: [1.0, -1.0]).All(predicate: double.IsNaN), Expected: true),
            (Label: "ragged-centroid-is-nan", Probe: static () => Numeric.Centroid(points: [[1.0, 2.0], [3.0]]).All(predicate: double.IsNaN), Expected: true),
            (Label: "covariance-weight-mismatch-is-nan-triangle", Probe: static () => Numeric.CovarianceUpper(points: [[1.0, 2.0]], weights: [1.0, 2.0]) is [double a, double b, double c] && double.IsNaN(d: a) && double.IsNaN(d: b) && double.IsNaN(d: c), Expected: true),
            (Label: "distance-dim-mismatch-is-nan", Probe: static () => double.IsNaN(d: Numeric.Distance(left: [1.0], right: [1.0, 2.0])), Expected: true),
            (Label: "convergence-nonpositive-error-is-nan", Probe: static () => double.IsNaN(d: Numeric.ConvergenceOrder(coarseError: 0.0, fineError: 1.0e-3)), Expected: true));
    }

    [Fact]
    [Law(typeof(Norm), "matrix-and-spectral-oracles")]
    public void MatrixAndSpectralOraclesMatchClosedForms() {
        double[][] m = [[1.0, -2.0], [3.0, 4.0]];
        double At(int row, int col) => m[row][col];
        Spec.Equal(left: Norm.MaxAbs.Of(rows: 2, cols: 2, at: At), right: 4.0, what: "max-abs norm");
        Spec.Equal(left: Norm.L1.Of(rows: 2, cols: 2, at: At), right: 6.0, what: "L1 column-sum norm");
        Spec.Equal(left: Norm.LInf.Of(rows: 2, cols: 2, at: At), right: 7.0, what: "LInf row-sum norm");
        Spec.Equal(left: Norm.Frobenius.Of(rows: 2, cols: 2, at: At), right: Math.Sqrt(d: 30.0), what: "frobenius norm");
        double[][] a3 = [[2.0, 0.0, 1.0], [1.0, 3.0, -1.0], [0.0, 5.0, 2.0]];
        Spec.Equal(left: Numeric.Determinant(n: 3, at: (row, col) => a3[row][col]), right: 27.0, what: "3x3 determinant");
        Spec.Equal(left: Numeric.Determinant(n: 0, at: static (_, _) => 0.0), right: 1.0, what: "empty determinant is the empty product");
        // Exact solve and eigenpair residuals vanish; shape mismatches are the reachable NaN reds.
        double[][] diag = [[2.0, 0.0], [0.0, 3.0]];
        Spec.Equal(left: Numeric.SolveResidual(rows: 2, cols: 2, at: (row, col) => diag[row][col], x: [1.0, 2.0], b: [2.0, 6.0]), right: 0.0, what: "exact solve residual");
        Spec.Equal(left: Numeric.EigenpairResidual(n: 2, at: (row, col) => diag[row][col], eigenvalue: 3.0, eigenvector: [0.0, 1.0]), right: 0.0, what: "exact eigenpair residual");
        Spec.Equal(left: Numeric.ProductResidual(rows: 2, width: 2, cols: 2, left: static (row, col) => row == col ? 1.0 : 0.0, right: At, actual: At), right: 0.0, what: "identity product residual");
        Spec.Equal(left: Numeric.FrobeniusDistance(left: At, right: static (_, _) => 0.0, rows: 2, cols: 2), right: Math.Sqrt(d: 30.0), what: "frobenius distance to zero");
        Spec.Matrix(
            (Label: "solve-shape-mismatch-is-nan", Probe: () => double.IsNaN(d: Numeric.SolveResidual(rows: 2, cols: 2, at: At, x: [1.0], b: [2.0, 6.0])), Expected: true),
            (Label: "eigenvector-shape-mismatch-is-nan", Probe: () => double.IsNaN(d: Numeric.EigenpairResidual(n: 2, at: At, eigenvalue: 1.0, eigenvector: [1.0])), Expected: true));
        // The path-graph Laplacian meets its own closed-form eigenpair and conserves row mass.
        double[][] laplacian = Numeric.PathGraphLaplacian(n: 5);
        double lambda = 2.0 - (2.0 * Math.Cos(d: 2.0 * Math.PI / 5.0));
        double[] phi = [.. Enumerable.Range(start: 0, count: 5).Select(j => Math.Cos(d: 2.0 * Math.PI * (j + 0.5) / 5.0))];
        Spec.Holds(condition: Numeric.EigenpairResidual(n: 5, at: (row, col) => laplacian[row][col], eigenvalue: lambda, eigenvector: phi) <= 1.0e-12, label: "closed-form eigenpair");
        Spec.Matrix(rows: [.. Enumerable.Range(start: 0, count: 5).Select(row => (
            Label: string.Create(provider: System.Globalization.CultureInfo.InvariantCulture, $"laplacian row {row} mass"),
            Probe: (Func<bool>)(() => Numeric.LaplacianRowSum(laplacian: laplacian, row: row) == 0.0),
            Expected: true))]);
        _ = Assert.Throws<ArgumentOutOfRangeException>(testCode: static () => Numeric.PathGraphLaplacian(n: 1));
        // Heat kernel over a delta eigenbasis reduces to exp(-lambda t) on the diagonal.
        Spec.Equal(left: Numeric.HeatKernel(eigenvalues: [0.5, 2.0], eigenvectors: static (i, x) => i == x ? 1.0 : 0.0, t: 0.7, x: 0, y: 0), right: Math.Exp(d: -0.35), what: "diagonal heat kernel");
        Spec.Equal(left: Numeric.HeatKernel(eigenvalues: [0.5, 2.0], eigenvectors: static (i, x) => i == x ? 1.0 : 0.0, t: 0.7, x: 0, y: 1), right: 0.0, what: "off-diagonal heat kernel");
        System.Numerics.Complex[] z = [new(1.0, 2.0), new(3.0, -1.0)];
        System.Numerics.Complex selfDot = Numeric.DotComplex(count: 2, left: i => z[i], right: i => z[i]);
        Spec.Equal(left: selfDot.Real, right: 15.0, what: "hermitian self-dot is the squared norm");
        Spec.Equal(left: selfDot.Imaginary, right: 0.0, what: "hermitian self-dot is real");
        Assert.Equal(expected: 2, actual: Numeric.EulerCharacteristic(vertices: 8, edges: 12, faces: 6));
    }

    [Fact]
    [Law(typeof(VariantWriter<>), "variant-writer-emit")]
    public void VariantWriterEmitsPresentAndSkipsAbsent() {
        DirectoryInfo root = Directory.CreateTempSubdirectory(prefix: "rasm-variant-");
        try {
            VariantWriter<string> writer = new(
                Directory: root,
                Names: new Dictionary<string, string>(StringComparer.Ordinal) { ["raw"] = "raw.json", ["enc"] = "enc.json", ["gone"] = "gone.json" }.ToFrozenDictionary(StringComparer.Ordinal),
                Payloads: new Dictionary<string, VariantPayload>(StringComparer.Ordinal) {
                    ["raw"] = new VariantPayload.Raw(Bytes: Encoding.UTF8.GetBytes("{}")),
                    ["enc"] = new VariantPayload.Encoded(Value: new SampleRow(Tag: "e", Rank: 1)),
                    ["gone"] = new VariantPayload.Raw(Bytes: ReadOnlyMemory<byte>.Empty),
                }.ToFrozenDictionary(StringComparer.Ordinal),
                Encode: SampleContext.Default.SampleRow,
                Absent: AbsentVariants);
            FrozenDictionary<string, FileInfo> written = writer.WriteAll();
            Assert.True(condition: written["raw"].Exists);
            Assert.True(condition: written["enc"].Exists);
            Assert.False(condition: written["gone"].Exists);
            // A named variant with no payload row and no absence declaration is a table defect.
            VariantWriter<string> orphan = writer with {
                Names = new Dictionary<string, string>(StringComparer.Ordinal) { ["lost"] = "lost.json" }.ToFrozenDictionary(StringComparer.Ordinal),
            };
            _ = Assert.Throws<XunitException>(testCode: () => orphan.Path(variant: "lost"));
            // The inverse defect: a payload row outside the name table can never emit — WriteAll
            // refuses the whole table and names the stray row.
            VariantWriter<string> stray = writer with {
                Payloads = new Dictionary<string, VariantPayload>(StringComparer.Ordinal) {
                    ["ghost"] = new VariantPayload.Raw(Bytes: ReadOnlyMemory<byte>.Empty),
                }.ToFrozenDictionary(StringComparer.Ordinal),
            };
            XunitException ghost = Assert.Throws<XunitException>(testCode: stray.WriteAll);
            Assert.Contains(expectedSubstring: "ghost", actualString: ghost.Message, comparisonType: StringComparison.Ordinal);
        } finally {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    [Law(typeof(TmpRoot<>), "tmp-root-write")]
    public void TmpRootWritesUnderInjectedSettings() {
        DirectoryInfo root = Directory.CreateTempSubdirectory(prefix: "rasm-tmproot-");
        try {
            TmpRoot<int> tmp = TmpRoot.Of(root: root, makeSettings: static dir => dir.FullName.Length);
            FileInfo file = tmp.Write(relative: "nested/data.txt", text: "payload");
            Assert.True(condition: file.Exists);
            Assert.Equal(expected: root.FullName.Length, actual: tmp.Settings);
            Assert.Equal(expected: "payload", actual: File.ReadAllText(path: file.FullName));
            // The mode knob is real: an executable bit lands on disk (non-Windows lane).
            if (!OperatingSystem.IsWindows()) {
                FileInfo exec = tmp.Write(relative: "bin/run.sh", text: "#!/bin/sh", mode: Some(value: UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute));
                Assert.Equal(expected: UnixFileMode.UserExecute, actual: File.GetUnixFileMode(path: exec.FullName) & UnixFileMode.UserExecute);
            }
            // Isolation is guarded: upward traversal and rooted relatives refuse before any write.
            _ = Assert.ThrowsAny<ArgumentException>(testCode: () => tmp.Write(relative: "../escape.txt"));
            _ = Assert.ThrowsAny<ArgumentException>(testCode: () => tmp.Write(relative: "/rooted/escape.txt"));
        } finally {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.ContentKey), Member = nameof(Spec.ContentKey))]
    public void ContentKeyGaugeDemandsStabilitySeparationAndHonestAxes() {
        KeyAxis<byte[]> copy = new(Name: "representation-copy", Transform: static bytes => [.. bytes], PreservesKey: true);
        KeyAxis<byte[]> append = new(Name: "content-append", Transform: static bytes => [.. bytes, 0x2A], PreservesKey: false);
        Spec.ContentKey(gen: Gens.Payload, mint: Mint, copy, append);
        // The Mutant band is the canonical separation witness: a real mint splits every pair.
        Spec.ForAll(gen: Gens.Mutant, property: static pair =>
            Spec.Holds(condition: !string.Equals(a: Mint(bytes: pair.Original), b: Mint(bytes: pair.Mutated), comparisonType: StringComparison.Ordinal), label: "mint must split a one-byte mutant"));
        // A constant mint survives every preserving axis and is rejected by the separating one.
        _ = Assert.ThrowsAny<Exception>(testCode: () => Spec.ContentKey(gen: Gens.Payload, mint: static _ => "<pinned>", copy, append));
        // An unstable mint is rejected by the stability gate before any axis runs.
        _ = Assert.ThrowsAny<Exception>(testCode: () => Spec.ContentKey(gen: Gens.Payload, mint: static _ => Guid.NewGuid().ToString(), copy, append));
        // A lying preserving axis — one that actually changes content — is rejected.
        _ = Assert.ThrowsAny<Exception>(testCode: () => Spec.ContentKey(gen: Gens.Payload, mint: Mint,
            new KeyAxis<byte[]>(Name: "lying-copy", Transform: static bytes => [.. bytes, 0x00], PreservesKey: true), append));
        // An empty axis table and an all-preserving table both refuse to run.
        _ = Assert.Throws<XunitException>(testCode: () => Spec.ContentKey(gen: Gens.Payload, mint: Mint));
        _ = Assert.Throws<XunitException>(testCode: () => Spec.ContentKey(gen: Gens.Payload, mint: Mint, copy));
        static string Mint(byte[] bytes) => Convert.ToHexString(inArray: System.Security.Cryptography.SHA256.HashData(source: bytes));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Causal), Member = nameof(Spec.Causal))]
    public void CausalGaugeEnforcesTheHlcAdvanceLaw() {
        Gen<(long P, ulong L)> stamps = Gen.Long[0L, 1_000_000L].Select(Gen.ULong[0UL, 8UL], static (long p, ulong l) => (P: p, L: l));
        Spec.Causal(gen: stamps, advance: Advance, physical: static s => s.P, logical: static s => s.L, refutingWitness: (Before: (P: 5L, L: 3UL), After: (P: 5L, L: 3UL)));
        // Physical advance without a logical reset is rejected by sampling.
        _ = Assert.ThrowsAny<Exception>(testCode: () => Spec.Causal(gen: stamps, advance: static s => (s.P + 1L, s.L + 1UL),
            physical: static s => s.P, logical: static s => s.L, refutingWitness: (Before: (P: 5L, L: 3UL), After: (P: 5L, L: 3UL))));
        // A frozen stamp and a regressed stamp are rejected by sampling.
        _ = Assert.ThrowsAny<Exception>(testCode: () => Spec.Causal(gen: stamps, advance: static s => s,
            physical: static s => s.P, logical: static s => s.L, refutingWitness: (Before: (P: 5L, L: 3UL), After: (P: 5L, L: 3UL))));
        _ = Assert.ThrowsAny<Exception>(testCode: () => Spec.Causal(gen: stamps, advance: static s => (s.P - 1L, 0UL),
            physical: static s => s.P, logical: static s => s.L, refutingWitness: (Before: (P: 5L, L: 3UL), After: (P: 5L, L: 3UL))));
        // A witness pair that satisfies the order exposes the registration as a tautology.
        XunitException tautology = Assert.Throws<XunitException>(testCode: () => Spec.Causal(gen: stamps, advance: Advance,
            physical: static s => s.P, logical: static s => s.L, refutingWitness: (Before: (P: 5L, L: 3UL), After: (P: 6L, L: 0UL))));
        Assert.Contains(expectedSubstring: "tautology", actualString: tautology.Message, comparisonType: StringComparison.Ordinal);
        static (long P, ulong L) Advance((long P, ulong L) stamp) => stamp.L < 3UL ? (stamp.P, stamp.L + 1UL) : (stamp.P + 1L, 0UL);
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.FaultBands), Member = nameof(Spec.FaultBands))]
    public void FaultBandRegistryNamesEveryViolationInOneVerdict() {
        Spec.FaultBands(("geometry", 2400, 2449, [2400, 2401, 2424]), ("material", 2450, 2499, [2450]), ("bim", 2600, 2699, []));
        XunitException verdict = Assert.Throws<XunitException>(testCode: static () => Spec.FaultBands(
            ("alpha", 2400, 2449, [2399, 2400, 2400]),
            ("alpha", 2440, 2460, [2455]),
            ("beta", 2500, 2450, [])));
        Assert.Contains(expectedSubstring: "outside band", actualString: verdict.Message, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: "duplicate codes", actualString: verdict.Message, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: "registers more than one band", actualString: verdict.Message, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: "overlap", actualString: verdict.Message, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: "inverted band", actualString: verdict.Message, comparisonType: StringComparison.Ordinal);
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.FaultBands());
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Semilattice), Member = nameof(Spec.Semilattice))]
    public void SemilatticeGaugeProvesTheVerdictFoldExhaustively() {
        int[] vocabulary = [0, 1, 2, 3];
        Spec.Semilattice(vocabulary: vocabulary, combine: Math.Max, identity: 0);
        // Non-associative, non-closed, wrong-identity, and non-idempotent folds all reject.
        _ = Assert.Throws<XunitException>(testCode: () => Spec.Semilattice(vocabulary: vocabulary, combine: static (a, b) => a - b, identity: 0));
        _ = Assert.Throws<XunitException>(testCode: () => Spec.Semilattice(vocabulary: vocabulary, combine: Math.Max, identity: 1));
        _ = Assert.Throws<XunitException>(testCode: () => Spec.Semilattice(vocabulary: vocabulary, combine: Math.Max, identity: 9));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Semilattice(vocabulary: (int[])[0, 1], combine: static (a, b) => a + b, identity: 0));
        // Duplicate vocabulary rows and empty vocabularies refuse to run.
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Semilattice(vocabulary: (int[])[0, 0, 1], combine: Math.Max, identity: 0));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Semilattice(vocabulary: (int[])[], combine: Math.Max, identity: 0));
    }

    [Fact]
    [Law(typeof(Spec), "dual-path-differential")]
    public void DualPathDifferentialArmProvesSubjectAgainstReference() {
        // The pure arm: an iterative subject against a closed-form reference.
        Spec.DualPath(gen: Gen.Int[0, 1000], subject: static n => Enumerable.Range(start: 1, count: n).Sum(), reference: static n => n * (n + 1) / 2);
        _ = Assert.ThrowsAny<Exception>(testCode: static () => Spec.DualPath(gen: Gen.Int[1, 1000],
            subject: static n => Enumerable.Range(start: 1, count: n).Sum(), reference: static n => (n * (n + 1) / 2) + 1));
        // The tolerance arm classifies divergence through the Metric row: a sign flip is an
        // admitted class under SignAmbiguous and a rejection under the default absolute row.
        Spec.DualPath(gen: Gen.Double[0.1, 100.0], subject: Math.Sqrt, reference: static x => -Math.Sqrt(d: x), tolerance: Tolerance.Default, metric: Metric.SignAmbiguous);
        _ = Assert.ThrowsAny<Exception>(testCode: static () => Spec.DualPath(gen: Gen.Double[0.1, 100.0],
            subject: Math.Sqrt, reference: static x => -Math.Sqrt(d: x), tolerance: Tolerance.Default));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Golden), Member = nameof(Spec.Golden))]
    public void GoldenTableGatesEveryRowAndNamesEveryDivergence() {
        Spec.Golden(tolerance: Tolerance.Default, rows: [("pi", Math.PI, 3.14159265358979), ("e", Math.E, 2.71828182845905)]);
        Spec.Golden(tolerance: Tolerance.Absolute(epsilon: 1.0e-9), metric: Metric.SignAmbiguous, rows: [("eigen-axis", 2.0, -2.0)]);
        XunitException drift = Assert.Throws<XunitException>(testCode: static () => Spec.Golden(tolerance: Tolerance.Absolute(epsilon: 1.0e-12),
            rows: [("good", 1.0, 1.0), ("bad-a", 1.0, 2.0), ("bad-b", 3.0, 4.0)]));
        Assert.Contains(expectedSubstring: "bad-a", actualString: drift.Message, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: "bad-b", actualString: drift.Message, comparisonType: StringComparison.Ordinal);
        Assert.DoesNotContain(expectedSubstring: "good", actualString: drift.Message, comparisonType: StringComparison.Ordinal);
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Golden(tolerance: Tolerance.Default));
    }

    [Fact]
    [Law(typeof(Manifests), nameof(Manifests.Corpus), Member = nameof(Manifests.Corpus))]
    public void CorpusDiscoveryKeysFixturesAndTheRoundtripGateNamesDrift() {
        DirectoryInfo root = Directory.CreateTempSubdirectory(prefix: "rasm-corpus-");
        try {
            byte[] alpha = JsonSerializer.SerializeToUtf8Bytes(value: new SampleRow(Tag: "a", Rank: 1), jsonTypeInfo: SampleContext.Default.SampleRow);
            byte[] beta = JsonSerializer.SerializeToUtf8Bytes(value: new SampleRow(Tag: "b", Rank: 2), jsonTypeInfo: SampleContext.Default.SampleRow);
            _ = Directory.CreateDirectory(path: Path.Combine(path1: root.FullName, path2: "nested"));
            File.WriteAllBytes(path: Path.Combine(path1: root.FullName, path2: "alpha.json"), bytes: alpha);
            File.WriteAllBytes(path: Path.Combine(path1: root.FullName, path2: "nested", path3: "beta.json"), bytes: beta);
            CorpusEntry[] corpus = [.. Manifests.Corpus(root: root, pattern: "*.json")];
            string[] paths = [.. corpus.Select(selector: static entry => entry.RelativePath)];
            Assert.Equal(expected: ["alpha.json", "nested/beta.json"], actual: paths);
            Assert.NotEqual(expected: corpus[0].Key, actual: corpus[1].Key);
            Spec.RoundtripBytes(contract: SampleContext.Default.SampleRow, corpus: corpus);
            // Content addressing: identical bytes mint identical keys under any name or nesting,
            // and the gate refuses the byte-identical twins as a corpus defect naming both.
            File.WriteAllBytes(path: Path.Combine(path1: root.FullName, path2: "twin.json"), bytes: alpha);
            CorpusEntry[] twinned = [.. Manifests.Corpus(root: root, pattern: "*.json")];
            Dictionary<string, UInt128> keys = twinned.ToDictionary(keySelector: static entry => entry.RelativePath, elementSelector: static entry => entry.Key, comparer: StringComparer.Ordinal);
            Assert.Equal(expected: keys["alpha.json"], actual: keys["twin.json"]);
            XunitException twins = Assert.Throws<XunitException>(testCode: () => Spec.RoundtripBytes(contract: SampleContext.Default.SampleRow, corpus: twinned));
            Assert.Contains(expectedSubstring: "twin.json", actualString: twins.Message, comparisonType: StringComparison.Ordinal);
            File.Delete(path: Path.Combine(path1: root.FullName, path2: "twin.json"));
            // A fixture that decodes but re-encodes differently is named drift, never accepted.
            File.WriteAllBytes(path: Path.Combine(path1: root.FullName, path2: "gamma.json"), bytes: "{ \"tag\": \"g\",  \"rank\": 3 }"u8.ToArray());
            XunitException drift = Assert.Throws<XunitException>(testCode: () => Spec.RoundtripBytes(contract: SampleContext.Default.SampleRow, corpus: [.. Manifests.Corpus(root: root, pattern: "*.json")]));
            Assert.Contains(expectedSubstring: "gamma.json", actualString: drift.Message, comparisonType: StringComparison.Ordinal);
            // An absent root is an empty corpus, and the gate refuses an empty corpus as vacuous.
            Assert.True(condition: Manifests.Corpus(root: new DirectoryInfo(path: Path.Combine(path1: root.FullName, path2: "absent")), pattern: "*.json").IsEmpty);
            _ = Assert.Throws<XunitException>(testCode: static () => Spec.RoundtripBytes(contract: SampleContext.Default.SampleRow, corpus: (CorpusEntry[])[]));
        } finally {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    [Law(typeof(Timeline), "deterministic-clock")]
    public void TimelineFiresProbesAtExactDueInstantsDeterministically() {
        Timeline timeline = new();
        using ITimer once = timeline.Probe(label: "once", due: TimeSpan.FromMilliseconds(value: 50));
        using ITimer beat = timeline.Probe(label: "beat", due: TimeSpan.FromMilliseconds(value: 30), period: TimeSpan.FromMilliseconds(value: 30));
        // Nothing fires before its due instant; one advance crossing several dues fires each in
        // schedule order, and Elapsed is the schedule's own arithmetic — never the advanced clock.
        Assert.True(condition: timeline.Advance(delta: TimeSpan.FromMilliseconds(value: 29)).IsEmpty);
        Seq<ClockMark> fired = timeline.Advance(delta: TimeSpan.FromMilliseconds(value: 41));
        string[] labels = [.. fired.Map(static mark => mark.Label)];
        double[] instants = [.. fired.Map(static mark => mark.Elapsed.TotalMilliseconds)];
        Assert.Equal(expected: ["beat", "once", "beat"], actual: labels);
        Assert.Equal(expected: [30.0, 50.0, 60.0], actual: instants);
        // Determinism: an identical script on a fresh timeline lands the identical mark log.
        Timeline replay = new();
        using ITimer replayOnce = replay.Probe(label: "once", due: TimeSpan.FromMilliseconds(value: 50));
        using ITimer replayBeat = replay.Probe(label: "beat", due: TimeSpan.FromMilliseconds(value: 30), period: TimeSpan.FromMilliseconds(value: 30));
        _ = replay.Advance(delta: TimeSpan.FromMilliseconds(value: 29));
        _ = replay.Advance(delta: TimeSpan.FromMilliseconds(value: 41));
        ClockMark[] first = [.. timeline.Marks];
        ClockMark[] second = [.. replay.Marks];
        Assert.Equal(expected: first, actual: second);
        // A disposed probe never fires again; a live periodic probe keeps firing.
        Timeline scoped = new();
        using (ITimer transient = scoped.Probe(label: "transient", due: TimeSpan.FromMilliseconds(value: 10), period: TimeSpan.FromMilliseconds(value: 10))) {
            Assert.Equal(expected: 2, actual: scoped.Advance(delta: TimeSpan.FromMilliseconds(value: 20)).Count);
        }
        Assert.True(condition: scoped.Advance(delta: TimeSpan.FromMilliseconds(value: 100)).IsEmpty);
        _ = Assert.ThrowsAny<ArgumentException>(testCode: () => scoped.Probe(label: " ", due: TimeSpan.Zero));
    }

    [Fact]
    [Law(typeof(Gens), "wire-stamp-quantity-bands")]
    public void WireStampAndQuantityBandsCarryTheirConstructionInvariants() {
        // Wire strings are hazard-rich yet UTF-8 total: no lone surrogate ever enters the band.
        Spec.ForAll(gen: Gens.WireString, property: static text =>
            Spec.Holds(condition: string.Equals(a: Encoding.UTF8.GetString(bytes: Encoding.UTF8.GetBytes(s: text)), b: text, comparisonType: StringComparison.Ordinal), label: "wire string survives the UTF-8 wire"));
        Spec.ForAll(gen: Gens.Payload, property: static bytes =>
            Spec.Holds(condition: bytes.Length <= 4096, label: "payload band size"));
        Spec.Distributed(gen: Gens.Payload, bucket: static bytes => bytes.Length == 0 ? 0 : 1, expected: [130, 870]);
        // Mutant pairs are equal-length and differ in exactly one byte — the separation witness.
        Spec.ForAll(gen: Gens.Mutant, property: static pair => {
            Assert.Equal(expected: pair.Original.Length, actual: pair.Mutated.Length);
            Spec.Holds(condition: Enumerable.Range(start: 0, count: pair.Original.Length).Count(i => pair.Original[i] != pair.Mutated[i]) == 1, label: "mutant differs in exactly one byte");
        });
        Spec.ForAll(gen: Gens.Hlc, property: static stamp =>
            Spec.Holds(condition: stamp.Physical >= 0L, label: "hlc physical half is a nonnegative tick"));
        // Quantity bands: seven bounded exponents on every draw, and every draw is a FRESH array —
        // the mutation here can never poison a later canonical-row sample.
        Spec.ForAll(gen: Gens.SiExponents, property: static exponents => {
            Spec.Holds(condition: exponents.Length == 7 && exponents.All(predicate: static e => e is >= -4 and <= 4), label: "seven bounded SI exponents");
            exponents[0] = 99;
        }, iter: 400);
        Spec.ForAll(gen: Gens.Measure, property: static measure =>
            Spec.Holds(condition: double.IsFinite(d: measure.Si) && measure.Exponents.Length == 7, label: "measure carries a finite SI magnitude under a dimension vector"));
    }

    private static GenOperation<Atom<int>, Atom<int>> TrackedAdd() =>
        GenOperation.Create(
            gen: Gen.Int[1, 10],
            name: static (int delta) => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"add({delta})"),
            actual: static (Atom<int> actual, int delta) => _ = actual.Swap(state => state + delta),
            model: static (Atom<int> model, int delta) => _ = model.Swap(state => state + delta));
}
