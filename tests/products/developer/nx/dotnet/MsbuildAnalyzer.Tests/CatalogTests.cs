using System.IO.Compression;
using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CatalogTests : IDisposable {
    private const string Catalog = """
        <Project>
            <PropertyGroup>
                <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                <FakeCVersion>1.0.0</FakeCVersion>
            </PropertyGroup>
            <ItemGroup>
                <PackageVersion Include="Fake.A"      Version="1.0.0" />              <!-- plain row -->
                <PackageVersion Include="Fake.B"      Version="[1.0.0, 2.0.0)" />     <!-- ranged row -->
                <PackageVersion Include="Fake.C"      Version="$(FakeCVersion)" />    <!-- property row -->
                <PackageVersion Include="Fake.D"      Version="[1.0.0]" />            <!-- exact row -->
            </ItemGroup>
        </Project>
        """;

    private readonly TempWorkspace workspace = new();

    public void Dispose() => workspace.Dispose();

    private CatalogTests Feed() {
        workspace
            .Write("nuget.config", """<configuration><packageSources><clear /><add key="feed" value="feed" /></packageSources></configuration>""")
            .Write("Directory.Packages.props", Catalog)
            .Write("Workspace.slnx", """<Solution><Project Path="app/app.csproj" /></Solution>""")
            .Write("feed/.keep", "")
            .Write("app/app.csproj", TempWorkspace.Project("""<ItemGroup><PackageReference Include="Fake.A" /><PackageReference Include="Fake.B" /><PackageReference Include="Fake.C" /><PackageReference Include="Fake.D" /></ItemGroup>"""));
        foreach ((string id, string version) in from id in (string[])["Fake.A", "Fake.B", "Fake.C", "Fake.D"] from version in (string[])["1.0.0", "1.10.0", "2.0.0"] select (id, version)) {
            using ZipArchive archive = ZipFile.Open(workspace.Located($"feed/{id}.{version}.nupkg"), ZipArchiveMode.Create);
            using StreamWriter nuspec = new(archive.CreateEntry($"{id}.nuspec").Open());
            nuspec.Write($"""<?xml version="1.0"?><package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"><metadata><id>{id}</id><version>{version}</version><authors>t</authors><description>t</description></metadata></package>""");
        }
        return this;
    }

    // --- [UPGRADE]

    [Fact]
    public void UpgradeMovesEachRowWithinItsRangeAndRewritesTheValueAlone() {
        string printed = TempWorkspace.Run("", "upgrade", Feed().workspace.Located("Workspace.slnx"));

        Assert.Equal(
            Catalog
                .Replace("""Version="1.0.0" />""", """Version="2.0.0" />""", StringComparison.Ordinal)
                .Replace("[1.0.0, 2.0.0)", "[1.10.0, 2.0.0)", StringComparison.Ordinal)
                .Replace("<FakeCVersion>1.0.0</FakeCVersion>", "<FakeCVersion>2.0.0</FakeCVersion>", StringComparison.Ordinal),
            workspace.Read("Directory.Packages.props"));
        Assert.Contains("Fake.B: [1.0.0, 2.0.0) -> [1.10.0, 2.0.0)", printed, StringComparison.Ordinal);
        Assert.DoesNotContain("Fake.D", printed, StringComparison.Ordinal);
    }

    [Fact]
    public void UpgradeDryRunWritesNothing() {
        TempWorkspace.Run("", "upgrade", Feed().workspace.Located("Workspace.slnx"), "--dry-run");

        Assert.Equal(Catalog, workspace.Read("Directory.Packages.props"));
    }
}
