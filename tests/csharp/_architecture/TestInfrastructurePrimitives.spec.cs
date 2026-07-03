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

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(SampleRow))]
internal sealed partial class SampleContext : JsonSerializerContext;

// Production-shaped exemption sites: Laws.Sut must derive them from the typed Csp attributes.
[CspExempt("coverage-gate self-proof site")]
internal static class ExemptSite { public static int Reached() => 1; }

[CspScope(CspScope.Tooling)]
internal static class ToolingSite { public static int Reached() => 2; }

[CspScope(CspScope.Domain)]
internal static class DomainSite { public static int Reached() => 3; }

// --- [OPERATIONS] --------------------------------------------------------------------------
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
        // A tautology: no witness can fail it, so registration itself is the failure.
        XunitException tautology = Assert.Throws<XunitException>(testCode: static () => Spec.Hold(law: Law.Of(
            name: "vacuous",
            gen: Gen.Int[0, 10],
            property: static _ => Spec.Holds(condition: true, label: "unreachable"),
            refutingWitness: 0)));
        Assert.Contains(expectedSubstring: "tautology", actualString: tautology.Message, comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    [Law(typeof(Law<>), "algebra-row-constructors")]
    public void AlgebraRowsCarryTheirOwnRefutation() {
        // Strict `==` makes each row genuinely falsifiable: the finite gen passes, and a NaN witness
        // — outside the gen — refutes because every operator propagates NaN and NaN != NaN under `==`.
        static bool strict(double a, double b) => a == b;
        Spec.Hold(
            Law.Idempotent(name: "clamp-idempotent", gen: Gen.Double[-1.0e3, 1.0e3], f: static x => Math.Clamp(value: x, min: -1.0, max: 1.0), witness: double.NaN, eq: strict),
            Law.Roundtrip(name: "negate-roundtrips", gen: Gen.Double[-1.0e3, 1.0e3], forward: static x => -x, back: static x => -x, witness: double.NaN, eq: strict));
        // A monotone row refutes on an out-of-order pair the OrderedPair gen never emits — no NaN needed.
        Spec.Hold(law: Law.Monotone(name: "identity-monotone", pairs: Gens.OrderedPair(Gen.Double[-1.0e3, 1.0e3]), projection: static x => x, witness: (Lo: 5.0, Hi: 3.0)));
        Spec.Hold(law: Law.Commutative(name: "max-commutes", gen: Gen.Double[-1.0e3, 1.0e3], op: Math.Max, witness: (A: double.NaN, B: 1.0), eq: strict));
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
    public void RoundtripBytesProvesByteIdentity() =>
        Spec.RoundtripBytes(gen: Rows, contract: SampleContext.Default.SampleRow, iter: 50);

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
    public void DistributedGatesGeneratorSkewByChiSquared() =>
        Spec.Distributed(gen: Gen.Int[0, 3], bucket: static x => x, expected: [1000, 1000, 1000, 1000]);

    [Fact]
    [Law(typeof(Spec), "rail-gate-polarity")]
    public void RailGatesFailOnTheWrongSide() {
        Spec.Succ(result: Fin.Succ(value: 1));
        Spec.FailCategory(result: Fin.Fail<int>(error: new Fault.Missing()), category: nameof(Fault.Missing));
        Spec.Some(result: Some(value: 1));
        Spec.None(result: Option<int>.None);
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Succ(result: Fin.Fail<int>(error: new Fault.Rejected())));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Fail(result: Fin.Succ(value: 1)));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Some(result: Option<int>.None));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.None(result: Some(value: 1)));
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
    [Law(typeof(SeamProbe<>), "seam-typed-log-lifo")]
    public void SeamProbeRecordsTypedCallsAndRestoresLifo() {
        SeamProbe<Unit> probe = new();
        string current = "<unbound>";
        Func<Unit, int>? outer = null;
        Func<Unit, int>? inner = null;
        SeamRestore outerScope = probe.Install(member: "outer", shape: new Shape<int>.Sync(Value: 7), bind: canned => {
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
        // The typed log: sync records the member with a payload, factory records its inner label
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
            sync: static s => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"sync:{s.Value}"),
            async: static s => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"async:{s.Value}"),
            fanOut: static s => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"fan:{s.Values.Count}"),
            factory: static s => $"factory:{s.InnerLabel}");
        Assert.Equal(expected: "sync:1", actual: Render(new Shape<int>.Sync(Value: 1)));
        Assert.Equal(expected: "async:2", actual: Render(new Shape<int>.Async(Value: 2)));
        Assert.Equal(expected: "fan:3", actual: Render(new Shape<int>.FanOut(Values: Seq(0, 0, 0))));
        Assert.Equal(expected: "factory:run.x", actual: Render(new Shape<int>.Factory(Value: 4, InnerLabel: "run.x")));
    }

    [Fact]
    [Law(typeof(NdjsonOracle<>), "ndjson-line-gate")]
    public void NdjsonOracleGatesLineCount() {
        NdjsonOracle<SampleRow> oracle = new(Decoder: SampleContext.Default.SampleRow, ExpectLines: 1);
        byte[] one = JsonSerializer.SerializeToUtf8Bytes(value: new SampleRow(Tag: "x", Rank: 5), jsonTypeInfo: SampleContext.Default.SampleRow);
        Assert.Equal(expected: new SampleRow(Tag: "x", Rank: 5), actual: oracle.One(raw: one));
        _ = Assert.Throws<XunitException>(testCode: () => oracle.One(raw: Encoding.UTF8.GetBytes($"{Encoding.UTF8.GetString(one)}\n{Encoding.UTF8.GetString(one)}")));
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
        } finally {
            root.Delete(recursive: true);
        }
    }

    private static GenOperation<Atom<int>, Atom<int>> TrackedAdd() =>
        GenOperation.Create(
            gen: Gen.Int[1, 10],
            name: static (int delta) => string.Create(System.Globalization.CultureInfo.InvariantCulture, $"add({delta})"),
            actual: static (Atom<int> actual, int delta) => _ = actual.Swap(state => state + delta),
            model: static (Atom<int> model, int delta) => _ = model.Swap(state => state + delta));
}
