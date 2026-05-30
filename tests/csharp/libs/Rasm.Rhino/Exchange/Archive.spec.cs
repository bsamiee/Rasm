using Rasm.Domain;
using Rasm.Rhino.Exchange;

namespace Rasm.Rhino.Tests.Exchange;

// The native graph WALK (FileArchiveOps.Resources over a live File3dm) is bridge-owned — File3dm P/Invokes
// rhcommon_c, which a VSTest process cannot load (see exchange.verify.csx theme 8). These laws cover the
// pure-managed projections the walk feeds into: link validation, the count summary, and the sentinel->Option
// rail (the same TextOption/GuidOption guards that absorb null File3dmObject members in the walk).

// --- [CONSTANTS] ------------------------------------------------------------------------
file static class Fixtures {
    internal static readonly FileArchiveSource Source = new FileArchiveSource.Path(
        Value: FileEndpoint.From(path: Path.Combine(Path.GetTempPath(), "rasm-archive-spec.3dm"))
            .IfFail(error => throw new InvalidOperationException(message: error.Message)));

    internal static FileResourceGraph Graph(Seq<string> linked = default, Seq<string> textures = default, Seq<string> references = default, int objects = 0) =>
        new(Objects: objects, Layers: 0, Materials: 0, Groups: 0, Blocks: 0, Views: 0, NamedViews: 0, Strings: 0,
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

        Seq<FileIssue> issues = graph.Validate(source: Fixtures.Source, scheduler: new IoScheduler.Sequential());

        Assert.Equal(expected: 3, actual: issues.Count);
        Assert.All(collection: issues.AsEnumerable(), action: issue => Assert.Equal(expected: FileIssueCode.BrokenLink, actual: issue.Code));
    }

    [Fact]
    public void ValidateFiltersBlankPathsAndExistingResources() {
        // Blank entries are dropped; a path that resolves to a real file (this test assembly) raises no issue.
        FileResourceGraph graph = Fixtures.Graph(
            linked: Seq("  ", string.Empty),
            references: Seq(typeof(FileResourceGraphLaws).Assembly.Location));

        Assert.True(condition: graph.Validate(source: Fixtures.Source, scheduler: new IoScheduler.Sequential()).IsEmpty);
    }

    [Fact]
    public void ValidateYieldsNoIssuesWhenEveryChannelIsEmpty() =>
        Assert.True(condition: Fixtures.Graph().Validate(source: Fixtures.Source, scheduler: new IoScheduler.Parallel()).IsEmpty);

    [Fact]
    public void SequentialAndParallelSchedulersFindTheSameBrokenLinkCount() {
        FileResourceGraph graph = Fixtures.Graph(linked: Seq("/rasm/a.3dm", "/rasm/b.3dm"), references: Seq("/rasm/c.jpg"));
        Assert.Equal(
            expected: graph.Validate(source: Fixtures.Source, scheduler: new IoScheduler.Sequential()).Count,
            actual: graph.Validate(source: Fixtures.Source, scheduler: new IoScheduler.Parallel()).Count);
    }

    [Fact]
    public void SummaryAcceptsNonNegativeCountsAndRejectsNegatives() {
        Assert.True(condition: Fixtures.Graph(objects: 7).Summary(op: Op.Of(name: nameof(SummaryAcceptsNonNegativeCountsAndRejectsNegatives))).IsSucc);
        Assert.True(condition: (Fixtures.Graph() with { Objects = -1 }).Summary(op: Op.Of(name: nameof(SummaryAcceptsNonNegativeCountsAndRejectsNegatives))).IsFail);
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
        Assert.Equal(expected: some, actual: projected.IsSome);
        _ = projected.IfSome(text => Assert.Equal(expected: expected, actual: text));
    }

    [Fact]
    public void GuidOptionProjectsEmptyToNoneAndKeepsRealIds() {
        Guid id = Guid.Parse(input: "8983d56c-7e2f-41bf-b365-4c2863f4c82c");
        Assert.True(condition: FileArchiveOps.GuidOption(value: Guid.Empty).IsNone);
        Assert.Equal(expected: id, actual: FileArchiveOps.GuidOption(value: id).IfNone(Guid.Empty));
    }

    [Fact]
    public void DateTimeOptionProjectsMinValueToNoneAndKeepsRealStamps() {
        DateTime stamp = new(year: 2026, month: 5, day: 29, hour: 12, minute: 0, second: 0, kind: DateTimeKind.Utc);
        Assert.True(condition: FileArchiveOps.DateTimeOption(value: DateTime.MinValue).IsNone);
        Assert.Equal(expected: stamp, actual: FileArchiveOps.DateTimeOption(value: stamp).IfNone(DateTime.MinValue));
    }
}
