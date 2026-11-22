using System.Text.Json;
using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ProjectFactsWorkspace() : Analyzed(
        ("graph/Directory.Build.props", """<Project><ItemGroup Condition="'$(MSBuildProjectName)' == 'App'"><ProjectReference Include="$(MSBuildThisFileDirectory)libs/Shared/Shared.csproj" /></ItemGroup></Project>"""),
        ("graph/apps/App/App.csproj", TempWorkspace.Project("""<ItemGroup><ProjectReference Include="../../libs/Core/Core.csproj" /></ItemGroup>""")),
        ("graph/libs/Core/Core.csproj", TempWorkspace.Project()),
        ("graph/libs/Shared/Shared.csproj", TempWorkspace.Project()),
        ("locked/App/App.csproj", TempWorkspace.Project()),
        ("locked/App/packages.lock.json", JsonElement.Parse("""
            {
              "version": 2,
              "dependencies": {
                "net10.0": {
                  "Serilog.Sinks.File": { "type": "Direct", "requested": "[7.0.0, )", "resolved": "7.0.0", "contentHash": "sha512-file", "dependencies": { "Serilog": "4.3.0" } },
                  "Serilog": { "type": "Transitive", "resolved": "4.3.0", "contentHash": "sha512-serilog" },
                  "core": { "type": "Project" }
                }
              }
            }
            """).GetRawText()),
        ("locked/Custom/Custom.csproj", TempWorkspace.Project("<PropertyGroup><NuGetLockFilePath>locks/custom.json</NuGetLockFilePath></PropertyGroup>")),
        ("locked/Custom/locks/custom.json", JsonElement.Parse("""{ "version": 2, "dependencies": { "net10.0": {} } }""").GetRawText()),
        ("stated/Plugin/Plugin.csproj", """<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFrameworks>net10.0;net10.0-windows</TargetFrameworks><EnableWindowsTargeting>true</EnableWindowsTargeting></PropertyGroup><ItemGroup><NxTag Include="host:rhino" /><NxImplicitDependency Include="Host" /></ItemGroup><ItemGroup Condition="'$(TargetFramework)' == 'net10.0-windows'"><NxTag Include="os:windows" /></ItemGroup></Project>"""),
        ("stated/External/External.csproj", TempWorkspace.Project($"""<ItemGroup><UpToDateCheckInput Include="{typeof(ProjectFactsTests).Assembly.Location}" /><UpToDateCheckInput Include="Inside.txt" /></ItemGroup>""")),
        ("stated/External/Inside.txt", ""),
        ("stated/Rules/Rules.csproj", TempWorkspace.Project("""<ItemGroup><AdditionalFiles Include="AnalyzerReleases.Shipped.md;AnalyzerReleases.Unshipped.md" /></ItemGroup>""")),
        ("stated/Rules/AnalyzerReleases.Shipped.md", ""),
        ("stated/Rules/AnalyzerReleases.Unshipped.md", ""),
        ("release/nuget.config", """<configuration><config><add key="defaultPushSource" value="https://api.nuget.org/v3/index.json" /></config></configuration>"""),
        ("release/Directory.Build.props", "<Project><PropertyGroup><Version>2.0.0</Version></PropertyGroup></Project>"),
        ("release/Own/Own.csproj", TempWorkspace.Project("<PropertyGroup><Version>1.0.0</Version><PackageId>fixture-own</PackageId></PropertyGroup>")),
        ("release/Inherited/Inherited.csproj", TempWorkspace.Project("<PropertyGroup><IsPackable>false</IsPackable><PackageId>fixture-inherited</PackageId></PropertyGroup>"))
);

public sealed class ProjectFactsTests(ProjectFactsWorkspace analyzed) : IClassFixture<ProjectFactsWorkspace> {

    // --- [GRAPH]

    [Fact]
    public void ReferencesNameTheFileDeclaringEach() =>
        Assert.Equal(
            ["graph/libs/Core <- graph/apps/App/App.csproj", "graph/libs/Shared <- graph/Directory.Build.props"],
            analyzed.Node("graph/apps/App/App.csproj").GetProperty("references").EnumerateArray().Select(static reference => $"{reference.GetProperty("root").GetString()} <- {reference.GetProperty("declared").GetString()}").Order(StringComparer.Ordinal),
            StringComparer.Ordinal);

    [Fact]
    public void LockedPackagesAreThePackagesOfTheLockFileWithoutProjects() {
        JsonElement node = analyzed.Node("locked/App/App.csproj");

        Assert.Equal(["Serilog.Sinks.File 7.0.0 sha512-file direct [Serilog]", "Serilog 4.3.0 sha512-serilog transitive []"], node.GetProperty("packages").EnumerateArray().Select(static package => $"{package.GetProperty("id").GetString()} {package.GetProperty("version").GetString()} {package.GetProperty("hash").GetString()} {(package.GetProperty("direct").GetBoolean() ? "direct" : "transitive")} [{string.Join(',', package.Strings("dependencies"))}]"), StringComparer.Ordinal);
        Assert.Equal(["locked/App/packages.lock.json"], node.Strings("locks"));
    }

    [Fact]
    public void LockFileIsTheOneNuGetLockFilePathNames() =>
        Assert.Equal(["locked/Custom/locks/custom.json"], analyzed.Node("locked/Custom/Custom.csproj").Strings("locks"));

    // --- [STATED]

    [Fact]
    public void TagsAndImplicitDependenciesAreTheNxItemsOfEveryEvaluation() {
        JsonElement node = analyzed.Node("stated/Plugin/Plugin.csproj");

        Assert.Equal(["host:rhino", "os:windows"], node.Strings("tags"));
        Assert.Equal(["Host"], node.Strings("implicit"));
        Assert.Equal(["", "Windows"], node.Strings("platforms"));
    }

    [Fact]
    public void ExternalFilesAreTheBuildInputsOutsideTheWorkspace() =>
        Assert.Equal([typeof(ProjectFactsTests).Assembly.Location], analyzed.Node("stated/External/External.csproj").Strings("external"));

    [Fact]
    public void ReleaseNotesAreTheAnalyzerReleaseFilesOfTheProject() {
        Assert.Equal("stated/Rules/AnalyzerReleases.Shipped.md", analyzed.Node("stated/Rules/Rules.csproj").GetProperty("releases").GetProperty("shipped").GetString());
        Assert.Equal(JsonValueKind.Null, analyzed.Node("stated/External/External.csproj").GetProperty("releases").ValueKind);
    }

    // --- [RELEASE]

    [Fact]
    public void PackablePackageIsNamedByItsPackageIdAndVersionedByItsOwnFile() {
        JsonElement own = analyzed.Node("release/Own/Own.csproj");
        JsonElement inherited = analyzed.Node("release/Inherited/Inherited.csproj");

        Assert.Equal(("fixture-own", true, true), (own.GetProperty("name").GetString(), own.GetProperty("versioned").GetBoolean(), own.GetProperty("pushed").GetBoolean()));
        Assert.Equal(("Inherited", false, true), (inherited.GetProperty("name").GetString(), inherited.GetProperty("versioned").GetBoolean(), inherited.GetProperty("pushed").GetBoolean()));
        Assert.False(analyzed.Node("graph/libs/Core/Core.csproj").GetProperty("pushed").GetBoolean());
    }
}
