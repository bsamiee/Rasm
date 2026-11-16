using System.Text.Json;
using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class OutputPathsTests : IDisposable {
    private const string Artifacts = "<UseArtifactsOutput>true</UseArtifactsOutput>";
    private readonly TempWorkspace workspace = new();

    public void Dispose() => workspace.Dispose();

    private OutputPathsTests Props(string properties) {
        _ = workspace.Write("Directory.Build.props", $"<Project><PropertyGroup>{properties}</PropertyGroup></Project>");
        return this;
    }

    private JsonElement Node(string project, params ReadOnlySpan<string> properties) =>
        workspace.Write(project, TempWorkspace.Project([.. properties.ToArray().Select(static property => $"<PropertyGroup>{property}</PropertyGroup>")])).Node(project);

    private JsonElement WebNode(string project) =>
        workspace.Write(project, TempWorkspace.Project().Replace("Microsoft.NET.Sdk", "Microsoft.NET.Sdk.Web", StringComparison.Ordinal)).Node(project);

    private static string?[] Released(JsonElement node, string property) =>
        Assert.Single(node.GetProperty("declared").EnumerateArray(), static mode => mode.GetProperty("configuration").ValueEquals("Release")).Strings(property);

    private static string? Packed(JsonElement mode) => mode.GetProperty("packed").GetString();

    // --- [WEB_SDK]

    [Fact]
    public void BuildWebSdkWithAbsoluteBaseIntermediatePathEmitsProjectRootObj() =>
        Assert.Equal(["MyWebApi/bin", "MyWebApi/obj"], WebNode("MyWebApi/MyWebApi.csproj").Strings("bases"));

    [Fact]
    public void BuildSdkWithRelativeDefaultsEmitsProjectRootBinAndObj() =>
        Assert.Equal(["MyLibrary/bin", "MyLibrary/obj"], Node("MyLibrary/MyLibrary.csproj").Strings("bases"));

    [Fact]
    public void BuildWebSdkDoesNotDoubleCountProjectDirectory() =>
        Assert.DoesNotContain(WebNode("MyWebApp/MyWebApp.csproj").Strings("restored"), static output => output?.Contains("MyWebApp/MyWebApp", StringComparison.Ordinal) ?? false);

    // --- [CENTRALIZED_DIST]

    [Fact]
    public void BuildCentralizedDistEmitsWorkspaceRootRelativeOutputs() =>
        Assert.Equal(
            ["dist/foo/bin", "dist/intermediates/foo/obj"],
            Props("<BaseOutputPath>$(MSBuildThisFileDirectory)dist/$(MSBuildProjectName)/bin/</BaseOutputPath><BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)dist/intermediates/$(MSBuildProjectName)/obj/</BaseIntermediateOutputPath>").Node("apps/foo/foo.csproj").Strings("bases"));

    [Fact]
    public void BuildPartialRedirectMixesProjectRootAndWorkspaceRoot() =>
        Assert.Equal(
            ["apps/foo/bin", "dist/intermediates/foo/obj"],
            Props("<BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)dist/intermediates/$(MSBuildProjectName)/obj/</BaseIntermediateOutputPath>").Node("apps/foo/foo.csproj").Strings("bases"));

    // --- [ARTIFACTS_OUTPUT]

    [Fact]
    public void BuildArtifactsOutputEmitsWorkspaceRootOutputs() =>
        Assert.Equal(["artifacts/bin/foo", "artifacts/obj/foo"], Props(Artifacts).Node("apps/foo/foo.csproj").Strings("bases"));

    [Fact]
    public void BuildArtifactsOutputWithCustomArtifactsPathEmitsWorkspaceRootOutputs() =>
        Assert.Equal(["build-output/bin/foo", "build-output/obj/foo"], Props("<ArtifactsPath>$(MSBuildThisFileDirectory)build-output</ArtifactsPath>").Node("apps/foo/foo.csproj").Strings("bases"));

    [Fact]
    public void BuildArtifactsOutputWithRenamedNxProjectUsesTheMSBuildProjectName() {
        JsonElement node = Props(Artifacts).Node("apps/foo/Renamed.csproj", "<PackageId>my-renamed-api</PackageId>");

        Assert.Equal("my-renamed-api", node.GetProperty("name").GetString());
        Assert.Equal(["artifacts/bin/Renamed", "artifacts/obj/Renamed"], node.Strings("bases"));
    }

    [Fact]
    public void BuildArtifactsOutputHonoursArtifactsProjectNameAndBinOutputName() =>
        Assert.Equal(
            ["artifacts/binaries/custom-name", "artifacts/obj/Override"],
            Props(Artifacts).Node("apps/foo/Override.csproj", "<ArtifactsProjectName>custom-name</ArtifactsProjectName><ArtifactsBinOutputName>binaries</ArtifactsBinOutputName>").Strings("bases"));

    [Fact]
    public void PublishArtifactsOutputHonoursArtifactsPublishOutputName() {
        JsonElement node = Props(Artifacts).Node("apps/foo/Foo.csproj", "<ArtifactsPublishOutputName>published</ArtifactsPublishOutputName>");

        Assert.Equal(["artifacts/published/Foo/debug"], node.Strings("evaluated", "published"));
        Assert.Equal(["artifacts/obj/Foo"], node.Strings("restored"));
    }

    // --- [PUBLISH]

    [Fact]
    public void PublishDeclaresTheReleasePublishDirOfTheReleaseConfiguration() =>
        Assert.Equal(["apps/foo/bin/Release/net10.0/publish"], Released(Node("apps/foo/foo.csproj", "<OutputType>Exe</OutputType>"), "published"));

    [Fact]
    public void PublishLeavesCustomPublishDirWithoutConfigurationSegmentAlone() =>
        Assert.Equal(["apps/foo/dist-publish"], Released(Node("apps/foo/foo.csproj", "<OutputType>Exe</OutputType><PublishDir>dist-publish</PublishDir>"), "published"));

    [Fact]
    public void PublishArtifactsLayoutEmitsWorkspaceRootPublishPath() =>
        Assert.Equal(["artifacts/publish/foo/debug"], Props(Artifacts).Node("apps/foo/foo.csproj", "<OutputType>Exe</OutputType>").Strings("evaluated", "published"));

    [Fact]
    public void PublishNonCanonicallyCasedPublishDirIsStillHonoured() =>
        Assert.Equal(["apps/foo/custompub"], Node("apps/foo/foo.csproj", "<OutputType>Exe</OutputType><publishdir>custompub/</publishdir>").Strings("evaluated", "published"));

    [Fact]
    public void PublishLeavesLiteralReleaseDirectoryAlone() =>
        Assert.Equal(["apps/foo/out/release"], Released(Node("apps/foo/foo.csproj", "<OutputType>Exe</OutputType><PublishDir>out/release/</PublishDir>"), "published"));

    // --- [PACK]

    [Fact]
    public void PackEmitsTheEvaluatedPackageOutputPath() =>
        Assert.Equal("libs/foo/bin/Debug", Packed(Node("libs/foo/foo.csproj").GetProperty("evaluated")));

    [Fact]
    public void PackDeclaresTheReleasePackageOutputPathOfTheReleaseConfiguration() =>
        Assert.Equal("libs/foo/bin/Release", Packed(Assert.Single(Node("libs/foo/foo.csproj").GetProperty("declared").EnumerateArray())));

    [Fact]
    public void PackArtifactsLayoutEmitsWorkspaceRootPackageAndObjPaths() {
        JsonElement node = Props(Artifacts).Node("libs/foo/foo.csproj");

        Assert.Equal("artifacts/package/debug", Packed(node.GetProperty("evaluated")));
        Assert.Equal(["artifacts/obj/foo"], node.Strings("restored"));
    }

    [Fact]
    public void PackArtifactsOutputHonoursArtifactsPackageOutputName() =>
        Assert.Equal("artifacts/packages/debug", Packed(Props(Artifacts).Node("libs/foo/foo.csproj", "<ArtifactsPackageOutputName>packages</ArtifactsPackageOutputName>").GetProperty("evaluated")));

    [Fact]
    public void PackNonCanonicallyCasedPackageOutputPathIsStillHonoured() =>
        Assert.Equal("libs/foo/custompkg", Packed(Node("libs/foo/foo.csproj", "<packageoutputpath>custompkg/</packageoutputpath>").GetProperty("evaluated")));

    [Fact]
    public void PackLeavesLiteralReleaseDirectoryAlone() =>
        Assert.Equal("libs/foo/nupkgs/release", Packed(Assert.Single(Node("libs/foo/foo.csproj", "<PackageOutputPath>nupkgs/release/</PackageOutputPath>").GetProperty("declared").EnumerateArray())));
}
