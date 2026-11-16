using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DirectoryBuildInputsTests : IDisposable {
    private const string Project = "apps/foo/foo.csproj";
    private readonly TempWorkspace workspace = new();

    public void Dispose() => workspace.Dispose();

    private string?[] Inputs(string project, params ReadOnlySpan<string> files) {
        foreach (string file in files)
            workspace.Write(file, "<Project />");
        return workspace.Write(project, TempWorkspace.Project()).Node(project).Strings("inputs");
    }

    // --- [NEAREST_ANCESTOR]

    [Fact]
    public void ReturnsEmptyWhenNoDirectoryFilesIndexed() => Assert.Empty(Inputs(Project));

    [Fact]
    public void ReturnsWorkspaceRootFileWhenOnlyRootHasOne() =>
        Assert.Equal(["Directory.Build.props"], Inputs(Project, "Directory.Build.props"));

    [Fact]
    public void ClosestAncestorWinsPerFileName() =>
        Assert.Equal(["apps/Directory.Build.props"], Inputs(Project, "Directory.Build.props", "apps/Directory.Build.props"));

    [Fact]
    public void DifferentFileNamesPickedUpFromDifferentAncestorsLeaveTheProjectFolderToDefault() =>
        Assert.Equal(["Directory.Build.targets"], Inputs(Project, "apps/foo/Directory.Build.props", "Directory.Build.targets"));

    [Fact]
    public void ProjectAtWorkspaceRootLeavesRootLevelFilesToDefault() =>
        Assert.Empty(Inputs("root.csproj", "Directory.Build.props", "Directory.Packages.props"));

    [Fact]
    public void MissingIntermediateAncestorIsTransparentlySkipped() {
        string?[] inputs = Inputs("apps/foo/bar/bar.csproj", "apps/foo/Directory.Build.targets", "Directory.Build.props");

        Assert.Contains("apps/foo/Directory.Build.targets", inputs, StringComparer.Ordinal);
        Assert.Contains("Directory.Build.props", inputs, StringComparer.Ordinal);
        Assert.Equal(2, inputs.Length);
    }

    // --- [CANONICAL_NAMES]

    [Fact]
    public void OnlyCanonicalFileNamesArePickedAndSolutionFilesOnlySolutionBuildsRead() {
        string?[] inputs = Inputs(Project, "Directory.Build.props", "Directory.Build.targets", "Directory.Build.rsp", "Directory.Solution.props", "Directory.Solution.targets", "Directory.Packages.props", "Directory.NotRecognized.props");

        Assert.Equal(4, inputs.Length);
        Assert.DoesNotContain(inputs, static input => input?.Contains("Directory.NotRecognized.props", StringComparison.Ordinal) ?? false);
    }

    [Fact]
    public void AllProjectBuildFileNamesAreSupportedAtSameDirectoryAndSolutionFilesOnlySolutionBuildsRead() {
        string?[] inputs = Inputs(Project, "Directory.Build.props", "Directory.Build.targets", "Directory.Build.rsp", "Directory.Solution.props", "Directory.Solution.targets", "Directory.Packages.props");

        Assert.Contains("Directory.Build.props", inputs, StringComparer.Ordinal);
        Assert.Contains("Directory.Build.targets", inputs, StringComparer.Ordinal);
        Assert.Contains("Directory.Build.rsp", inputs, StringComparer.Ordinal);
        Assert.DoesNotContain("Directory.Solution.props", inputs, StringComparer.Ordinal);
        Assert.DoesNotContain("Directory.Solution.targets", inputs, StringComparer.Ordinal);
        Assert.Contains("Directory.Packages.props", inputs, StringComparer.Ordinal);
    }

    [Fact]
    public void ShadowedAncestorCopiesAreNotIncludedAndProjectFolderCopiesLeftToDefault() {
        string?[] inputs = Inputs(
            Project,
            "apps/foo/Directory.Build.props",
            "apps/foo/Directory.Build.targets",
            "apps/foo/Directory.Build.rsp",
            "apps/foo/Directory.Solution.props",
            "apps/foo/Directory.Solution.targets",
            "apps/foo/Directory.Packages.props",
            "apps/Directory.Build.props",
            "Directory.Build.props");

        Assert.All(inputs, static input => Assert.StartsWith("apps/foo/", input, StringComparison.Ordinal));
        Assert.DoesNotContain("apps/foo/Directory.Build.props", inputs, StringComparer.Ordinal);
    }

    // --- [CASCADING_NAMES]

    [Fact]
    public void GlobalJsonNearestAncestorWins() {
        workspace.Write("global.json", "{}").Write("apps/global.json", "{}");

        Assert.Equal(["apps/global.json"], Inputs(Project));
    }

    [Fact]
    public void NuGetConfigEveryAncestorIsAnInput() {
        workspace.Write("nuget.config", "<configuration />").Write("apps/foo/nuget.config", "<configuration />");
        string?[] inputs = Inputs(Project);

        Assert.Contains("apps/foo/nuget.config", inputs, StringComparer.Ordinal);
        Assert.Contains("nuget.config", inputs, StringComparer.Ordinal);
        Assert.Equal(2, inputs.Length);
    }

    [Fact]
    public void NuGetConfigKeepsTheOnDiskCasing() {
        workspace.Write("NuGet.Config", "<configuration />");

        Assert.Equal(["NuGet.Config"], Inputs(Project));
    }

    [Fact]
    public void EditorConfigEveryAncestorIsAnInput() {
        string?[] inputs = Inputs(Project, ".editorconfig", "apps/foo/.editorconfig");

        Assert.Contains("apps/foo/.editorconfig", inputs, StringComparer.Ordinal);
        Assert.Contains(".editorconfig", inputs, StringComparer.Ordinal);
        Assert.Equal(2, inputs.Length);
    }
}
