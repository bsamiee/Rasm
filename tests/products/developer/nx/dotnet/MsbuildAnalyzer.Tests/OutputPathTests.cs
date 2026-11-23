using System.Text.Json;
using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class OutputPathWorkspace : Analyzed {
    public OutputPathWorkspace() : base(
        ("built.slnx", """<Solution><Project Path="apps/Lib/Lib.csproj" /><Project Path="apps/Tool/Tool.csproj" /><Project Path="central/src/Core/Core.csproj" /><Project Path="layout/src/Api/Api.csproj" /></Solution>"""),
        ("apps/Lib/Lib.csproj", TempWorkspace.Project()),
        ("apps/Multi/Multi.csproj", """<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFrameworks>net10.0;net9.0</TargetFrameworks></PropertyGroup></Project>"""),
        ("apps/Private/Private.csproj", TempWorkspace.Project("<PropertyGroup><IsPublishable>false</IsPublishable></PropertyGroup>")),
        ("apps/Tool/Tool.csproj", TempWorkspace.Project("<PropertyGroup><OutputType>Exe</OutputType><PublishDir>dist/</PublishDir></PropertyGroup>")),
        ("apps/Tool/Program.cs", "return;"),
        ("tests/Platform/Platform.csproj", TempWorkspace.Project("<PropertyGroup><OutputType>Exe</OutputType><IsTestingPlatformApplication>true</IsTestingPlatformApplication></PropertyGroup>")),
        ("tests/VsTest/VsTest.csproj", TempWorkspace.Project("<PropertyGroup><IsTestProject>true</IsTestProject></PropertyGroup>")),
        ("central/Directory.Build.props", "<Project><PropertyGroup><BaseOutputPath>$(MSBuildThisFileDirectory)dist/$(MSBuildProjectName)/bin/</BaseOutputPath><BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)dist/$(MSBuildProjectName)/obj/</BaseIntermediateOutputPath></PropertyGroup></Project>"),
        ("central/src/Core/Core.csproj", TempWorkspace.Project()),
        ("layout/Directory.Build.props", "<Project><PropertyGroup><UseArtifactsOutput>true</UseArtifactsOutput></PropertyGroup></Project>"),
        ("layout/src/Api/Api.csproj", TempWorkspace.Project()),
        ("layout/tests/ApiTests/ApiTests.csproj", TempWorkspace.Project("<PropertyGroup><IsTestProject>true</IsTestProject></PropertyGroup>"))) {
        Restored = Workspace.Written("restore", "built.slnx");
        Built = Workspace.Written("publish", "built.slnx", "--no-restore", "--configuration", "Release");
        Packed = Workspace.Written("pack", "built.slnx", "--no-build", "--configuration", "Release");
        Debugged = Workspace.Written("build", "built.slnx", "--no-restore");
    }

    public ImmutableArray<string> Restored { get; }
    public ImmutableArray<string> Built { get; }
    public ImmutableArray<string> Packed { get; }
    public ImmutableArray<string> Debugged { get; }
}

public sealed class OutputPathTests(OutputPathWorkspace analyzed) : IClassFixture<OutputPathWorkspace> {

    // --- [SDK]

    [Theory]
    [InlineData("apps/Lib/Lib.csproj", "apps/Lib/")]
    [InlineData("apps/Tool/Tool.csproj", "apps/Tool/")]
    [InlineData("central/src/Core/Core.csproj", "central/")]
    [InlineData("layout/src/Api/Api.csproj", "layout/")]
    public void ReleaseFoldersHoldThePublishedAssemblyEveryReleaseWriteAndThePackageButNoDebugWrite(string project, string area) {
        JsonElement release = analyzed.Node(project).Mode("Release");
        string?[] published = release.Strings("published");
        string?[] outputs = [.. release.Strings("outputs"), .. published];
        string[] built = [.. analyzed.Built.Where(file => file.StartsWith(area, StringComparison.Ordinal))];
        string[] packed = [.. analyzed.Packed.Where(file => file.StartsWith(area, StringComparison.Ordinal) && file.EndsWith(".nupkg", StringComparison.Ordinal))];

        Assert.Contains(built, file => file.EndsWith(".dll", StringComparison.Ordinal) && published.Any(folder => file.StartsWith($"{folder}/", StringComparison.Ordinal)));
        Assert.Contains(analyzed.Debugged, file => file.StartsWith(area, StringComparison.Ordinal) && file.EndsWith(".dll", StringComparison.Ordinal));
        Assert.All(built, file => Assert.Contains(outputs, folder => file.StartsWith($"{folder}/", StringComparison.Ordinal)));
        Assert.DoesNotContain(analyzed.Debugged, file => outputs.Any(folder => file.StartsWith($"{folder}/", StringComparison.Ordinal)));
        Assert.Equal(release.GetProperty("packed").GetString(), Path.GetDirectoryName(Assert.Single(packed))?.Replace('\\', '/'));
    }

    [Theory]
    [InlineData("apps/Lib/Lib.csproj", "apps/Lib/")]
    [InlineData("central/src/Core/Core.csproj", "central/")]
    [InlineData("layout/src/Api/Api.csproj", "layout/")]
    public void RestoredFilesAreTheAssetsFileAndImportsRestoreWrote(string project, string area) =>
        Assert.Equal(
            analyzed.Restored.Where(file => file.StartsWith(area, StringComparison.Ordinal) && (file.EndsWith("/project.assets.json", StringComparison.Ordinal) || file.Contains(".nuget.g.", StringComparison.Ordinal))).Order(StringComparer.Ordinal),
            analyzed.Node(project).Strings("restored"),
            StringComparer.Ordinal);

    // --- [EVALUATION]

    [Fact]
    public void EveryTargetFrameworkAddsItsOwnFolders() =>
        Assert.Equal(["apps/Multi/bin/Debug/net10.0", "apps/Multi/bin/Debug/net9.0", "apps/Multi/obj/Debug/net10.0", "apps/Multi/obj/Debug/net9.0"], analyzed.Node("apps/Multi/Multi.csproj").Strings("evaluated", "outputs"));

    [Fact]
    public void UnpublishableProjectReportsNoPublishFolder() =>
        Assert.Empty(analyzed.Node("apps/Private/Private.csproj").Mode("Release").Strings("published"));

    [Fact]
    public void TestResultsSitUnderTheBuildOfATestingPlatformApplicationTheProjectOfAVsTestProjectAndTheArtifactsTestFolder() {
        Assert.Equal(["tests/Platform/bin/Debug/net10.0/TestResults", "tests/Platform/bin/Release/net10.0/TestResults"], analyzed.Node("tests/Platform/Platform.csproj").Strings("results"));
        Assert.Equal(["tests/VsTest/TestResults"], analyzed.Node("tests/VsTest/VsTest.csproj").Strings("results"));
        Assert.Equal(["layout/artifacts/test/ApiTests"], analyzed.Node("layout/tests/ApiTests/ApiTests.csproj").Strings("results"));
    }
}
