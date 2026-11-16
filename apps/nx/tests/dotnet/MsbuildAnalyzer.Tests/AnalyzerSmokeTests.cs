using System.Text.Json;
using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class AnalyzerSmokeTests : IDisposable {
    private readonly TempWorkspace workspace = new();

    public void Dispose() => workspace.Dispose();

    // --- [OUTPUTS]

    [Fact]
    public void DefaultProjectDeclaresBinAndObj() {
        JsonElement node = workspace.Write("apps/MyLib/MyLib.csproj", TempWorkspace.Project()).Node("apps/MyLib/MyLib.csproj");

        Assert.Equal(["apps/MyLib/bin", "apps/MyLib/obj"], node.Strings("bases"));
        Assert.Equal(["apps/MyLib/obj"], node.Strings("restored"));
    }

    [Fact]
    public void OpenApiDocumentsDirectoryDeclaresTheDocumentGlobs() =>
        Assert.Equal(
            ["apps/MyApi/openapi/MyApi.json", "apps/MyApi/openapi/MyApi_*.json"],
            workspace
                .Write("apps/MyApi/MyApi.csproj", TempWorkspace.Project("<PropertyGroup><OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)/openapi</OpenApiDocumentsDirectory></PropertyGroup>"))
                .Node("apps/MyApi/MyApi.csproj")
                .Strings("evaluated", "documents"));

    [Fact]
    public void PackDeclaresTheReleasePackageDirectory() {
        JsonElement node = workspace.Write("apps/MyLib/MyLib.csproj", TempWorkspace.Project()).Node("apps/MyLib/MyLib.csproj");

        Assert.Equal("apps/MyLib/bin/Release", Assert.Single(node.GetProperty("declared").EnumerateArray()).GetProperty("packed").GetString());
    }

    [Fact]
    public void ArtifactsLayoutDeclaresTheMSBuildProjectNameNotTheNxName() {
        JsonElement node = workspace
            .Write("Directory.Build.props", "<Project><PropertyGroup><UseArtifactsOutput>true</UseArtifactsOutput></PropertyGroup></Project>")
            .Write("apps/Renamed/Renamed.csproj", TempWorkspace.Project("<PropertyGroup><PackageId>my-renamed-api</PackageId></PropertyGroup>"))
            .Node("apps/Renamed/Renamed.csproj");

        Assert.Equal("my-renamed-api", node.GetProperty("name").GetString());
        Assert.Equal(["artifacts/bin/Renamed", "artifacts/obj/Renamed"], node.Strings("bases"));
    }

    // --- [TESTS]

    [Fact]
    public void TestingPlatformApplicationGetsATestTarget() =>
        Assert.True(workspace
            .Write("apps/MyTests/MyTests.csproj", TempWorkspace.Project("<PropertyGroup><OutputType>Exe</OutputType><IsTestingPlatformApplication>true</IsTestingPlatformApplication></PropertyGroup>"))
            .Node("apps/MyTests/MyTests.csproj")
            .GetProperty("test")
            .GetBoolean());

    [Fact]
    public void PlainExecutableGetsNoTestTarget() =>
        Assert.False(workspace
            .Write("apps/MyApp/MyApp.csproj", TempWorkspace.Project("<PropertyGroup><OutputType>Exe</OutputType></PropertyGroup>"))
            .Node("apps/MyApp/MyApp.csproj")
            .GetProperty("test")
            .GetBoolean());

    [Fact]
    public void LibraryReferencingATestFrameworkKeepsPackAndGetsNoTestTarget() {
        JsonElement node = workspace
            .Write("apps/MyHelpers/MyHelpers.csproj", TempWorkspace.Project("<PropertyGroup><IsTestProject>false</IsTestProject></PropertyGroup>", """<ItemGroup><PackageReference Include="xunit.abstractions" Version="2.0.3" /></ItemGroup>"""))
            .Node("apps/MyHelpers/MyHelpers.csproj");

        Assert.False(node.GetProperty("test").GetBoolean());
        Assert.True(node.GetProperty("packable").GetBoolean());
    }

    // --- [INPUTS]

    [Fact]
    public void EvaluatedImportsAndLinkedFilesAreInputsOnBuild() {
        string?[] inputs = workspace
            .Write("Directory.Build.props", """<Project><Import Project="$(MSBuildThisFileDirectory)build/Common.Build.props" /></Project>""")
            .Write("build/Common.Build.props", "<Project />")
            .Write("shared/Shared.cs", "class Shared {}")
            .Write("config/stylecop.json", "{}")
            .Write("global.json", "{}")
            .Write("apps/MyLib/MyLib.csproj", TempWorkspace.Project("""<ItemGroup><Compile Include="../../shared/Shared.cs" Link="Shared.cs" /><AdditionalFiles Include="../../config/stylecop.json" /></ItemGroup>"""))
            .Node("apps/MyLib/MyLib.csproj")
            .Strings("inputs");

        Assert.Contains("Directory.Build.props", inputs, StringComparer.Ordinal);
        Assert.Contains("build/Common.Build.props", inputs, StringComparer.Ordinal);
        Assert.Contains("shared/Shared.cs", inputs, StringComparer.Ordinal);
        Assert.Contains("config/stylecop.json", inputs, StringComparer.Ordinal);
        Assert.DoesNotContain(inputs, static input => input?.Contains("Microsoft.Common", StringComparison.Ordinal) ?? false);
        Assert.DoesNotContain(inputs, static input => input?.Contains("MyLib.csproj", StringComparison.Ordinal) ?? false);
    }

    [Fact]
    public void EvaluationInputsListEveryWorkspaceFileMSBuildImported() {
        string?[] evaluated = workspace
            .Write("Directory.Build.props", """<Project><Import Project="$(MSBuildThisFileDirectory)build/Common.Build.props" /></Project>""")
            .Write("build/Common.Build.props", "<Project />")
            .Write("apps/MyLib/MyLib.csproj", TempWorkspace.Project())
            .Analyze("apps/MyLib/MyLib.csproj")
            .Strings("evaluated");

        Assert.Contains("Directory.Build.props", evaluated, StringComparer.Ordinal);
        Assert.Contains("build/Common.Build.props", evaluated, StringComparer.Ordinal);
        Assert.Contains("apps/MyLib/MyLib.csproj", evaluated, StringComparer.Ordinal);
        Assert.DoesNotContain(evaluated, static input => input?.Contains("Microsoft.Common", StringComparison.Ordinal) ?? false);
    }

    [Fact]
    public void EvaluationInputsExcludeRestoreGeneratedImports() {
        JsonElement report = workspace
            .Write("apps/MyLib/MyLib.csproj", TempWorkspace.Project())
            .Write("apps/MyLib/obj/MyLib.csproj.nuget.g.props", "<Project />")
            .Analyze("apps/MyLib/MyLib.csproj");
        string?[] evaluated = report.Strings("evaluated");
        string?[] inputs = Assert.Single(report.GetProperty("nodes").EnumerateArray()).Strings("inputs");

        Assert.Contains("apps/MyLib/MyLib.csproj", evaluated, StringComparer.Ordinal);
        Assert.DoesNotContain(evaluated, static input => input?.Contains("nuget.g.props", StringComparison.Ordinal) ?? false);
        Assert.DoesNotContain(inputs, static input => input?.Contains("nuget.g.props", StringComparison.Ordinal) ?? false);
    }
}
