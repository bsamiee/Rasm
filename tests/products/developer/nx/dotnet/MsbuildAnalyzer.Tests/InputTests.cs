using System.Text.Json;
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
        ("redirected/apps/Lib/Lib.csproj", TempWorkspace.Project("""<ItemGroup><Compile Include="$(BaseOutputPath)Generated.cs" /></ItemGroup>""")),
        ("central/Directory.Build.props", "<Project><PropertyGroup><ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally></PropertyGroup></Project>"),
        ("central/Directory.Packages.props", """<Project><ItemGroup><PackageVersion Include="Unused.Package" Version="1.0.0" /></ItemGroup></Project>"""),
        ("central/apps/Lib/Lib.csproj", TempWorkspace.Project()),
        ("pinned/Directory.Packages.props", """<Project><PropertyGroup><HostVersion>9.0.1</HostVersion></PropertyGroup><ItemGroup><PackageVersion Include="Host.Sdk" Version="$(HostVersion)" /></ItemGroup></Project>"""),
        ("pinned/apps/Lib/Lib.csproj", TempWorkspace.Project())
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

    // --- [LOADING]

    [Fact]
    public void LoadedFilesAreTheImportsOutsideTheProjectFolderAndNoFileOnlyTheCompilerReads() =>
        Assert.Equal(["shared/Directory.Build.props", "shared/build/Common.props"], analyzed.Node("shared/apps/Lib/Lib.csproj").Strings("loaded"));

    [Fact]
    public void LoadedAncestorFilesAreTheSdkResponseAndNuGetFilesAndNoCodeStyleFile() =>
        Assert.Equal(["cascading/NuGet.Config", "cascading/apps/Lib/nuget.config"], analyzed.Node("cascading/apps/Lib/Lib.csproj").Strings("loaded"));

    // --- [CENTRAL_PACKAGES]

    [Fact]
    public void TheCentralPackageFileIsNoInputAndStaysEvaluatedAndCatalogued() {
        JsonElement node = analyzed.Node("central/apps/Lib/Lib.csproj");

        Assert.Equal(["central/Directory.Build.props"], node.Strings("inputs"));
        Assert.Equal(["central/Directory.Build.props"], node.Strings("loaded"));
        Assert.Equal(["central/Directory.Packages.props"], node.Strings("catalogs"));
        Assert.Contains("central/Directory.Packages.props", analyzed.Report.Strings("evaluated"), StringComparer.Ordinal);
    }

    [Fact]
    public void AProjectOutsideCentralPackageManagementHasNoCatalog() =>
        Assert.Empty(analyzed.Node("plain/Plain/Plain.csproj").Strings("catalogs"));

    [Fact]
    public void ACentralPackageFileDefiningAPropertyFailsEachProjectReadingItNamingTheProperty() {
        JsonElement failure = Assert.Single(analyzed.Report.GetProperty("failures").EnumerateArray(), static failure => failure.GetProperty("file").ValueEquals("pinned/apps/Lib/Lib.csproj"));

        Assert.Contains("pinned/Directory.Packages.props defines HostVersion", failure.GetProperty("message").GetString(), StringComparison.Ordinal);
    }
}
