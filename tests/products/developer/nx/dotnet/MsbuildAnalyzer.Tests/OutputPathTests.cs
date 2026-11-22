using System.Text.Json;
using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class OutputPathWorkspace() : Analyzed(
        ("apps/Lib/Lib.csproj", TempWorkspace.Project()),
        ("apps/Multi/Multi.csproj", """<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFrameworks>net10.0;net9.0</TargetFrameworks></PropertyGroup></Project>"""),
        ("apps/Private/Private.csproj", TempWorkspace.Project("<PropertyGroup><IsPublishable>false</IsPublishable></PropertyGroup>")),
        ("apps/Tool/Tool.csproj", TempWorkspace.Project("<PropertyGroup><OutputType>Exe</OutputType><PublishDir>dist/</PublishDir></PropertyGroup>")),
        ("apps/Restored/Restored.csproj", TempWorkspace.Project()),
        ("apps/Restored/obj/Restored.csproj.nuget.g.props", "<Project />"),
        ("tests/Platform/Platform.csproj", TempWorkspace.Project("<PropertyGroup><OutputType>Exe</OutputType><IsTestingPlatformApplication>true</IsTestingPlatformApplication></PropertyGroup>")),
        ("tests/VsTest/VsTest.csproj", TempWorkspace.Project("<PropertyGroup><IsTestProject>true</IsTestProject></PropertyGroup>")),
        ("central/Directory.Build.props", "<Project><PropertyGroup><BaseOutputPath>$(MSBuildThisFileDirectory)dist/$(MSBuildProjectName)/bin/</BaseOutputPath><BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)dist/$(MSBuildProjectName)/obj/</BaseIntermediateOutputPath></PropertyGroup></Project>"),
        ("central/src/Lib/Lib.csproj", TempWorkspace.Project()),
        ("layout/Directory.Build.props", "<Project><PropertyGroup><UseArtifactsOutput>true</UseArtifactsOutput></PropertyGroup></Project>"),
        ("layout/src/Api/Api.csproj", TempWorkspace.Project()),
        ("layout/tests/ApiTests/ApiTests.csproj", TempWorkspace.Project("<PropertyGroup><IsTestProject>true</IsTestProject></PropertyGroup>"))
);

public sealed class OutputPathTests(OutputPathWorkspace analyzed) : IClassFixture<OutputPathWorkspace> {

    // --- [BUILD]

    [Fact]
    public void BuildFoldersAreTheOutputAndIntermediateFoldersOfEachConfiguration() {
        JsonElement node = analyzed.Node("apps/Lib/Lib.csproj");

        Assert.Equal(["apps/Lib/bin/Debug/net10.0", "apps/Lib/obj/Debug/net10.0"], node.Strings("evaluated", "outputs"));
        Assert.Equal(["apps/Lib/bin/Release/net10.0", "apps/Lib/obj/Release/net10.0"], node.Mode("Release").Strings("outputs"));
        Assert.Equal(["apps/Lib/bin", "apps/Lib/obj"], node.Strings("bases"));
    }

    [Fact]
    public void EveryTargetFrameworkAddsItsOwnFolders() =>
        Assert.Equal(["apps/Multi/bin/Debug/net10.0", "apps/Multi/bin/Debug/net9.0", "apps/Multi/obj/Debug/net10.0", "apps/Multi/obj/Debug/net9.0"], analyzed.Node("apps/Multi/Multi.csproj").Strings("evaluated", "outputs"));

    [Fact]
    public void BuildFoldersRedirectedOutsideTheProjectAreWorkspaceRelative() {
        JsonElement node = analyzed.Node("central/src/Lib/Lib.csproj");

        Assert.Equal(["central/dist/Lib/bin", "central/dist/Lib/obj"], node.Strings("bases"));
        Assert.Equal(["central/dist/Lib/bin/Debug/net10.0", "central/dist/Lib/obj/Debug/net10.0"], node.Strings("evaluated", "outputs"));
    }

    // --- [PUBLISH_AND_PACK]

    [Fact]
    public void PublishFolderIsAnOutputOfPublishableProjectsAlone() {
        Assert.Equal(["apps/Lib/bin/Debug/net10.0/publish"], analyzed.Node("apps/Lib/Lib.csproj").Strings("evaluated", "published"));
        Assert.Equal(["apps/Lib/bin/Release/net10.0/publish"], analyzed.Node("apps/Lib/Lib.csproj").Mode("Release").Strings("published"));
        Assert.Empty(analyzed.Node("apps/Private/Private.csproj").Strings("evaluated", "published"));
        Assert.Equal(["apps/Tool/dist"], analyzed.Node("apps/Tool/Tool.csproj").Mode("Release").Strings("published"));
    }

    [Fact]
    public void PackageFolderIsThePackageOutputPathOfEachConfiguration() {
        JsonElement node = analyzed.Node("apps/Lib/Lib.csproj");

        Assert.Equal("apps/Lib/bin/Debug", node.GetProperty("evaluated").GetProperty("packed").GetString());
        Assert.Equal("apps/Lib/bin/Release", node.Mode("Release").GetProperty("packed").GetString());
    }

    // --- [RESTORE_AND_TEST]

    [Fact]
    public void RestoredFilesAreTheAssetsFileAndTheRestoreGeneratedImports() =>
        Assert.Equal(["apps/Restored/obj/Restored.csproj.nuget.g.props", "apps/Restored/obj/project.assets.json"], analyzed.Node("apps/Restored/Restored.csproj").Strings("restored"));

    [Fact]
    public void TestResultsSitUnderTheBuildOfATestingPlatformApplicationAndTheProjectOfAVsTestProject() {
        Assert.Equal(["tests/Platform/bin/Debug/net10.0/TestResults", "tests/Platform/bin/Release/net10.0/TestResults"], analyzed.Node("tests/Platform/Platform.csproj").Strings("results"));
        Assert.Equal(["tests/VsTest/TestResults"], analyzed.Node("tests/VsTest/VsTest.csproj").Strings("results"));
    }

    // --- [ARTIFACTS]

    [Fact]
    public void ArtifactsLayoutPlacesEveryFolderUnderTheArtifactsPath() {
        JsonElement node = analyzed.Node("layout/src/Api/Api.csproj");

        Assert.Equal(["layout/artifacts/bin/Api", "layout/artifacts/obj/Api"], node.Strings("bases"));
        Assert.Equal(["layout/artifacts/bin/Api/debug", "layout/artifacts/obj/Api/debug"], node.Strings("evaluated", "outputs"));
        Assert.Equal(["layout/artifacts/publish/Api/debug"], node.Strings("evaluated", "published"));
        Assert.Equal("layout/artifacts/package/debug", node.GetProperty("evaluated").GetProperty("packed").GetString());
        Assert.Equal(["layout/artifacts/obj/Api/project.assets.json"], node.Strings("restored"));
        Assert.Equal(["layout/artifacts/test/ApiTests"], analyzed.Node("layout/tests/ApiTests/ApiTests.csproj").Strings("results"));
    }
}
