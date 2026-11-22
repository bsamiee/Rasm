using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class InputWorkspace() : Analyzed(
        ("shared/Directory.Build.props", """<Project><Import Project="$(MSBuildThisFileDirectory)build/Common.props" /></Project>"""),
        ("shared/build/Common.props", "<Project />"),
        ("shared/src/Shared.cs", "class Shared;"),
        ("shared/config/stylecop.json", "{}"),
        ("shared/schema/api.json", "{}"),
        ("shared/apps/Lib/build/Local.props", "<Project />"),
        ("shared/apps/Lib/Lib.csproj", TempWorkspace.Project(
            """<Import Project="build/Local.props" />""",
            """<ItemGroup><Compile Include="../../src/Shared.cs" Link="Shared.cs" /><AdditionalFiles Include="../../config/stylecop.json" /></ItemGroup>""",
            """<Target Name="Generate" BeforeTargets="CoreCompile" Inputs="../../schema/api.json" Outputs="$(IntermediateOutputPath)api.g.cs" />""")),
        ("plain/Plain/Plain.csproj", TempWorkspace.Project()),
        ("nearest/global.json", "{}"),
        ("nearest/apps/global.json", "{}"),
        ("nearest/Directory.Build.rsp", ""),
        ("nearest/Directory.Build.props", "<Project />"),
        ("nearest/apps/Lib/Directory.Build.props", "<Project />"),
        ("nearest/apps/Lib/Lib.csproj", TempWorkspace.Project()),
        ("cascading/.editorconfig", "root = true"),
        ("cascading/.globalconfig", "is_global = true"),
        ("cascading/NuGet.Config", "<configuration />"),
        ("cascading/apps/.editorconfig", ""),
        ("cascading/apps/Lib/nuget.config", "<configuration />"),
        ("cascading/apps/Lib/Lib.csproj", TempWorkspace.Project()),
        ("redirected/Directory.Build.props", """<Project><PropertyGroup><BaseOutputPath>..\..\out\$(MSBuildProjectName)\bin\</BaseOutputPath></PropertyGroup></Project>"""),
        ("redirected/out/Lib/bin/Generated.cs", "class Generated;"),
        ("redirected/apps/Lib/Lib.csproj", TempWorkspace.Project("""<ItemGroup><Compile Include="$(BaseOutputPath)Generated.cs" /></ItemGroup>"""))
);

public sealed class InputTests(InputWorkspace analyzed) : IClassFixture<InputWorkspace> {

    // --- [EVALUATION]

    [Fact]
    public void WorkspaceFilesOutsideTheProjectFolderTheEvaluationReadsAreInputs() =>
        Assert.Equal(["shared/Directory.Build.props", "shared/build/Common.props", "shared/config/stylecop.json", "shared/schema/api.json", "shared/src/Shared.cs"], analyzed.Node("shared/apps/Lib/Lib.csproj").Strings("inputs"));

    [Fact]
    public void FilesUnderABuildFolderRedirectedOutsideTheProjectAreNoInputs() =>
        Assert.Equal(["redirected/Directory.Build.props"], analyzed.Node("redirected/apps/Lib/Lib.csproj").Strings("inputs"));

    [Fact]
    public void FilesOutsideTheWorkspaceAreNoInputs() =>
        Assert.Empty(analyzed.Node("plain/Plain/Plain.csproj").Strings("inputs"));

    [Fact]
    public void EvaluatedFilesAreTheProjectAndEveryImportInsideTheWorkspace() {
        string?[] evaluated = analyzed.Report.Strings("evaluated");

        Assert.All((string[])["shared/apps/Lib/Lib.csproj", "shared/apps/Lib/build/Local.props", "shared/Directory.Build.props", "shared/build/Common.props", "plain/Plain/Plain.csproj"], file => Assert.Contains(file, evaluated, StringComparer.Ordinal));
        Assert.DoesNotContain(evaluated, static file => file is not { } held || held.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(held));
    }

    // --- [ANCESTOR_FILES]

    [Fact]
    public void NearestGlobalJsonAndResponseFileAreInputsAndTheProjectFolderIsLeftToItsDefaultInputs() =>
        Assert.Equal(["nearest/Directory.Build.rsp", "nearest/apps/global.json"], analyzed.Node("nearest/apps/Lib/Lib.csproj").Strings("inputs"));

    [Fact]
    public void EveryAncestorCodeStyleAndNuGetFileIsAnInputSpelledAsOnDisk() =>
        Assert.Equal(["cascading/.editorconfig", "cascading/.globalconfig", "cascading/NuGet.Config", "cascading/apps/.editorconfig", "cascading/apps/Lib/nuget.config"], analyzed.Node("cascading/apps/Lib/Lib.csproj").Strings("inputs"));
}
