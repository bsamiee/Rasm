using Rasm.Domain;
using Rasm.Rhino.Exchange;
using Rasm.TestKit;

namespace Rasm.Rhino.Tests.Exchange;

// The native graph WALK (FileArchiveOps.Resources over a live File3dm) is bridge-owned — File3dm P/Invokes
// rhcommon_c, which a managed test runner process cannot load (see exchange.verify.csx theme 8). These laws cover the
// pure-managed projections the walk feeds into: link validation, the count summary, and the sentinel->Option
// rail (the same TextOption/GuidOption guards that absorb null File3dmObject members in the walk).

// --- [CONSTANTS] ------------------------------------------------------------------------
file static class Fixtures {
    internal static readonly FileArchiveSource Source = new FileArchiveSource.Path(
        Value: FileEndpoint.From(path: Path.Combine(Path.GetTempPath(), "rasm-archive-spec.3dm"))
            .IfFail(error => throw new InvalidOperationException(message: error.Message)));

    internal static FileResourceGraph Graph(Seq<string> linked = default, Seq<string> textures = default, Seq<string> references = default, int objects = 0) =>
        new(Objects: objects, Layers: 0, Materials: 0, Groups: 0, Blocks: 0, ModelViews: 0, LayoutViews: 0, NamedViews: 0, Strings: 0,
            PlugInData: 0, EmbeddedFiles: 0, RenderMaterials: 0, RenderEnvironments: 0, RenderTextures: 0, Linetypes: 0,
            DimensionStyles: 0, HatchPatterns: 0, NamedConstructionPlanes: 0, Manifest: 0, Relations: 0,
            EmbeddedFileNames: Seq<string>(), LinkedBlockArchives: linked, RenderTextureFiles: textures,
            FileReferences: references, Entries: Seq<FileResourceEntry>(), Edges: Seq<FileResourceEdge>());
}

// --- [ALGEBRAIC] ------------------------------------------------------------------------
public sealed class FileResourceGraphLaws {
    [Fact]
    public void ValidateReportsOneBrokenLinkPerDistinctMissingResourceAcrossAllThreeChannels() {
        // One missing path in each channel + a duplicate that must dedup -> 3 distinct BrokenLink issues.
        FileResourceGraph graph = Fixtures.Graph(
            linked: Seq("/rasm/missing-block.3dm", "/rasm/missing-block.3dm"),
            textures: Seq("/rasm/missing-texture.png"),
            references: Seq("/rasm/missing-reference.jpg"));

        Seq<FileIssue> issues = graph.Validate(source: Fixtures.Source, scheduler: new IoScheduler.Sequential())
            .IfFail(error => throw new InvalidOperationException(message: error.Message));

        Assert.Equal(expected: 3, actual: issues.Count);
        Assert.All(collection: issues.AsEnumerable(), action: issue => Assert.Equal(expected: FileIssueCode.BrokenLink, actual: issue.Code));
    }

    [Fact]
    public void ValidateFiltersBlankPathsAndExistingResources() {
        // Blank entries are dropped; a path that resolves to a real file (this test assembly) raises no issue.
        FileResourceGraph graph = Fixtures.Graph(
            linked: Seq("  ", string.Empty),
            references: Seq(typeof(FileResourceGraphLaws).Assembly.Location));

        Assert.True(condition: graph.Validate(source: Fixtures.Source, scheduler: new IoScheduler.Sequential()).IfFail(error => throw new InvalidOperationException(message: error.Message)).IsEmpty);
    }

    [Fact]
    public void ValidateResolvesRelativeResourcesAgainstArchiveFolder() {
        string assemblyPath = typeof(FileResourceGraphLaws).Assembly.Location;
        FileArchiveSource source = new FileArchiveSource.Path(
            Value: FileEndpoint.From(path: assemblyPath).IfFail(error => throw new InvalidOperationException(message: error.Message)));
        FileResourceGraph graph = Fixtures.Graph(references: Seq(Path.GetFileName(path: assemblyPath)));

        Assert.True(condition: graph.Validate(source: source, scheduler: new IoScheduler.Sequential()).IfFail(error => throw new InvalidOperationException(message: error.Message)).IsEmpty);
    }

    [Fact]
    public void ValidateYieldsNoIssuesWhenEveryChannelIsEmpty() =>
        Assert.True(condition: Fixtures.Graph().Validate(source: Fixtures.Source, scheduler: new IoScheduler.Parallel()).IfFail(error => throw new InvalidOperationException(message: error.Message)).IsEmpty);

    [Fact]
    public void SequentialAndParallelSchedulersFindTheSameBrokenLinkCount() {
        FileResourceGraph graph = Fixtures.Graph(linked: Seq("/rasm/a.3dm", "/rasm/b.3dm"), references: Seq("/rasm/c.jpg"));
        static (string Message, FileIssueCode Code)[] Expected(Seq<FileIssue> issues) =>
            [.. issues.Map(static issue => (issue.Message, issue.Code)).OrderBy(static issue => issue.Message, StringComparer.Ordinal)];
        Assert.Equal(
            expected: Expected(graph.Validate(source: Fixtures.Source, scheduler: new IoScheduler.Sequential()).IfFail(error => throw new InvalidOperationException(message: error.Message))),
            actual: Expected(graph.Validate(source: Fixtures.Source, scheduler: new IoScheduler.Parallel()).IfFail(error => throw new InvalidOperationException(message: error.Message))));
    }

    [Fact]
    public void ValidateRejectsNullSourceAndDeduplicatesAfterPathNormalization() {
        string root = Path.GetTempPath();
        string raw = "rasm-missing-normalized.3dm";
        FileResourceGraph graph = Fixtures.Graph(linked: Seq(raw, Path.Combine(path1: root, path2: raw)));
        FileArchiveSource source = new FileArchiveSource.Path(
            Value: FileEndpoint.From(path: Path.Combine(path1: root, path2: "archive.3dm")).IfFail(error => throw new InvalidOperationException(message: error.Message)));

        Spec.FailCategory(graph.Validate(source: null!, scheduler: new IoScheduler.Sequential()), category: "Input");
        _ = Assert.Single(collection: graph.Validate(source: source, scheduler: new IoScheduler.Sequential()).IfFail(error => throw new InvalidOperationException(message: error.Message)).AsEnumerable());
    }

    [Fact]
    public void SummaryAcceptsNonNegativeCountsAndRejectsNegatives() {
        Op op = Op.Of(name: nameof(SummaryAcceptsNonNegativeCountsAndRejectsNegatives));
        Spec.Succ(Fixtures.Graph(objects: 7).Summary(op: op), then: summary => {
            Assert.Equal(expected: 20, actual: summary.Count);
            Spec.Equal(left: summary.Maximum, right: 7.0, tolerance: 0.0, what: "archive summary max");
        });
        Spec.FailCategory((Fixtures.Graph() with { Objects = -1 }).Summary(op: op), category: "Result");
        Spec.FailCategory((Fixtures.Graph() with { Relations = -1 }).Summary(op: op), category: "Result");
    }
}

public sealed class FileArchiveProjectionLaws {
    [Theory]
    [InlineData(null, false, "")]
    [InlineData("", false, "")]
    [InlineData("   ", false, "")]
    [InlineData("plain", true, "plain")]
    [InlineData("  padded  ", true, "padded")]
    public void TextOptionProjectsBlankToNoneAndTrimsValues(string? value, bool some, string expected) {
        Option<string> projected = FileArchiveOps.TextOption(value: value);
        if (some) {
            Spec.Some(result: projected, then: text => Assert.Equal(expected: expected, actual: text));
            return;
        }
        Spec.None(result: projected);
    }

    [Fact]
    public void GuidOptionProjectsEmptyToNoneAndKeepsRealIds() {
        Guid id = Guid.Parse(input: "8983d56c-7e2f-41bf-b365-4c2863f4c82c");
        Spec.None(result: FileArchiveOps.GuidOption(value: Guid.Empty));
        Spec.Some(result: FileArchiveOps.GuidOption(value: id), then: actual => Assert.Equal(expected: id, actual: actual));
    }

    [Fact]
    public void DateTimeOptionProjectsMinValueToNoneAndKeepsRealStamps() {
        DateTime stamp = new(year: 2026, month: 5, day: 29, hour: 12, minute: 0, second: 0, kind: DateTimeKind.Utc);
        Spec.None(result: FileArchiveOps.DateTimeOption(value: DateTime.MinValue));
        Spec.Some(result: FileArchiveOps.DateTimeOption(value: stamp), then: actual => Assert.Equal(expected: stamp, actual: actual));
    }
}

public sealed class FileFormatCustomLaws {
    [Fact]
    public void CustomFormatsJoinKnownLookupDetectionAndFiltersWithoutInventingJsonBuiltIn() {
        string suffix = Guid.NewGuid().ToString(format: "N");
        string key = $"rasmcustom{suffix}";
        string extension = $".rc{suffix[..8]}";

        Spec.Succ(FileFormat.Custom(key: key, extensions: Seq(extension), capability: FileCapability.Import), then: custom => {
            Assert.Equal(expected: key.ToUpperInvariant(), actual: custom.Key);
            Spec.Some(result: FileFormat.Of(keyOrExtension: key).ToOption(), then: actual => Assert.Same(expected: custom, actual: actual));
            Spec.Some(result: FileFormat.Detect(path: $"fixture{extension}"), then: actual => Assert.Same(expected: custom, actual: actual));
            Assert.Contains(expected: custom, collection: FileFormat.Known);
            Assert.Contains(expectedSubstring: $"*.{extension.TrimStart('.')}", actualString: FileFormat.Filter(phase: FilePhase.Import, formats: Seq(custom)), comparisonType: StringComparison.Ordinal);
        });
        Spec.FailCategory(FileFormat.Of(keyOrExtension: "json"), category: "Input");
        Spec.None(result: FileFormat.Detect(path: "model.json"));
        Spec.FailCategory(FileFormat.Custom(key: "json", extensions: Seq(".rjson"), capability: FileCapability.Import), category: "Input");
        Spec.FailCategory(FileFormat.Custom(key: $"rasmjson{suffix}", extensions: Seq(".json"), capability: FileCapability.Import), category: "Input");
    }
}
