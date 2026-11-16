using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class EvaluatedInputsTests : IDisposable {
    private const string Project = "apps/foo/foo.csproj";
    private readonly TempWorkspace workspace = new();

    public void Dispose() => workspace.Dispose();

    // --- [SHARED_INPUTS]

    [Fact]
    public void SharedInputsKeepsWorkspaceFilesOutsideTheProjectDirectory() =>
        Assert.Equal(
            ["build/Common.Build.props", "shared/Shared.cs"],
            workspace
                .Write("build/Common.Build.props", "<Project />")
                .Write("shared/Shared.cs", "class Shared {}")
                .Write(Project, TempWorkspace.Project("""<Import Project="../../build/Common.Build.props" />""", """<ItemGroup><Compile Include="../../shared/Shared.cs" /></ItemGroup>"""))
                .Node(Project)
                .Strings("inputs"));

    [Fact]
    public void SharedInputsDropsFilesUnderTheProjectDirectory() =>
        Assert.Empty(workspace
            .Write("apps/foo/build/local.props", "<Project />")
            .Write(Project, TempWorkspace.Project("""<Import Project="build/local.props" />"""))
            .Node(Project)
            .Strings("inputs"));

    [Fact]
    public void SharedInputsDropsFilesOutsideTheWorkspace() =>
        Assert.Empty(workspace.Write(Project, TempWorkspace.Project()).Node(Project).Strings("inputs"));

    [Fact]
    public void SharedInputsDeduplicatesAndSorts() =>
        Assert.Equal(
            ["build/A.props", "shared/Z.cs"],
            workspace
                .Write("shared/Z.cs", "class Z {}")
                .Write("build/A.props", "<Project />")
                .Write(Project, TempWorkspace.Project("""<Import Project="../../build/A.props" />""", """<PropertyGroup><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup><Compile Include="../../shared/Z.cs" /><Compile Include="../../shared/Z.cs" /></ItemGroup>"""))
                .Node(Project)
                .Strings("inputs"));

    // --- [EVALUATED]

    [Fact]
    public void WorkspaceRelativePathsKeepsEveryWorkspaceFileIncludingTheProjectsOwn() =>
        Assert.Equal(
            ["apps/foo/foo.csproj", "build/Common.Build.props"],
            workspace
                .Write("build/Common.Build.props", "<Project />")
                .Write(Project, TempWorkspace.Project("""<Import Project="../../build/Common.Build.props" />"""))
                .Analyze(Project)
                .Strings("evaluated"));
}
