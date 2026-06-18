using System.Collections.Frozen;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

// --- [OPERATIONS] --------------------------------------------------------------------------
// The class-level [Law] proves ScanAssembly folds type-level markers; per-method [Law]s prove
// member-scoped coverage. The pilot SUT is Rasm.csproj.
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
        SutTarget target = Laws.Sut(sutAssembly: typeof(Op).Assembly);
        // The pilot SUT carries no [Law] markers, so the gate must Fail with a ManyErrors body that
        // names the uncovered public symbols — the mechanism, not full Rasm coverage, is the law here.
        Spec.Fail(result: Laws.AssertCoverage(target: target, manifest: Seq<LawRecord>()), then: static error => {
            ManyErrors many = Assert.IsType<ManyErrors>(@object: error);
            Assert.NotEmpty(collection: many.Errors);
            Assert.Contains(collection: many.Errors, filter: static e => e.Message.Contains(value: "law coverage gap", comparisonType: StringComparison.Ordinal));
        });
    }

    [Fact]
    [Law(typeof(Laws), nameof(Laws.Sut), Member = nameof(Laws.Sut))]
    public void CoverageCreditsManifestAndExemptions() {
        SutTarget target = Laws.Sut(sutAssembly: typeof(Op).Assembly);
        FrozenSet<string> surface = typeof(Op).Assembly.GetExportedTypes().Select(static type => type.Name).ToFrozenSet(StringComparer.Ordinal);
        Seq<LawRecord> covering = toSeq(surface).Map(static name => new LawRecord(Subject: typeof(Op), Name: name, Member: Optional(name), DeclaringType: typeof(TestInfrastructurePrimitiveLaws)));
        // A manifest covering every exported type name plus the derived exemptions collapses the gap to
        // member-level symbols only; the gate stays Fail but the type-name gaps are gone.
        Spec.Fail(result: Laws.AssertCoverage(target: target, manifest: covering), then: error =>
            Assert.DoesNotContain(collection: Assert.IsType<ManyErrors>(@object: error).Errors, filter: e => e.Message.Contains(value: $"'{nameof(Op)}'", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Refutes), Member = nameof(Spec.Refutes))]
    public void RefutesPassesOnBrokenWitnessAndFailsOnTautology() {
        Spec.Refutes(witness: 1, law: static x => Spec.Holds(condition: x == 0, label: "broken witness must fail"));
        _ = Assert.Throws<XunitException>(testCode: static () => Spec.Refutes(witness: 1, law: static _ => Spec.Holds(condition: true, label: "tautology")));
    }

    [Fact]
    [Law(typeof(Spec), nameof(Spec.MetamorphicSweep), Member = nameof(Spec.MetamorphicSweep))]
    public void MetamorphicSweepChecksEveryRelation() =>
        Spec.MetamorphicSweep(gen: Gen.Int[-1000, 1000], f: static x => x * 2,
            new MetamorphicRelation<int, int>(Name: "negate", Transform: static x => -x, Relate: static (@base, follow) => @base == -follow),
            new MetamorphicRelation<int, int>(Name: "shift", Transform: static x => x + 1, Relate: static (@base, follow) => follow == @base + 2));

    [Fact]
    [Law(typeof(Spec), nameof(Spec.RoundtripBytes), Member = nameof(Spec.RoundtripBytes))]
    public void RoundtripBytesProvesByteIdentity() =>
        Spec.RoundtripBytes(gen: Rows, contract: SampleContext.Default.SampleRow, iter: 50);

    [Fact]
    [Law(typeof(Spec), nameof(Spec.Stateful), Member = nameof(Spec.Stateful))]
    public void StatefulThreadsActualAgainstModel() =>
        // Actual and model are both mutable cells so each operation advances both; the actual-vs-model
        // equality across the operation sequence IS the stateful law CsCheck shrinks against.
        Spec.Stateful(
            init: Gen.Int[0, 100].Select(static seed => (Actual: Atom(seed), Model: Atom(seed))),
            equal: static (actual, model) => actual.Value == model.Value,
            operations: [TrackedAdd()],
            iter: 25);

    [Fact]
    [Law(typeof(Shape<>), "seam-shape-dispatch")]
    public void SeamProbeRecordsAndRestoresLifo() {
        SeamProbe probe = new();
        string current = "<unbound>";
        SeamRestore outerScope = probe.Install(member: "outer", shape: new Shape<int>.Sync(Value: 7), bind: canned => {
            string prior = current;
            current = "outer";
            return () => current = prior;
        });
        SeamRestore innerScope = probe.Install(member: "inner", shape: new Shape<int>.Factory(Value: 9), bind: canned => {
            string prior = current;
            current = "inner";
            return () => current = prior;
        });
        Assert.Equal(expected: "inner", actual: current);
        innerScope.Dispose();
        Assert.Equal(expected: "outer", actual: current);
        outerScope.Dispose();
        Assert.Equal(expected: "<unbound>", actual: current);
        Assert.Equal(expected: 0, actual: probe.Calls.Count);
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
