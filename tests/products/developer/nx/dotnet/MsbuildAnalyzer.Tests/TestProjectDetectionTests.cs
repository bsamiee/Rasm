using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TestProjectDetectionTests : IDisposable {
    private readonly TempWorkspace workspace = new();

    public void Dispose() => workspace.Dispose();

    private bool IsTestProject(params ReadOnlySpan<string> elements) =>
        workspace
            .Write("apps/foo/foo.csproj", TempWorkspace.Project(elements))
            .Node("apps/foo/foo.csproj")
            .GetProperty("test")
            .GetBoolean();

    private static string Property(string name, string value) => $"<PropertyGroup><{name}>{value}</{name}></PropertyGroup>";

    private static string Package(string id) => $"""<ItemGroup><PackageReference Include="{id}" Version="1.0.0" /></ItemGroup>""";

    // --- [PROPERTIES]

    [Fact]
    public void NoSignalsIsNotATestProject() => Assert.False(IsTestProject());

    [Fact]
    public void OrdinaryLibraryIsNotATestProject() => Assert.False(IsTestProject(Package("Newtonsoft.Json"), Package("Serilog")));

    [Theory]
    [InlineData("IsTestProject")]
    [InlineData("IsTestingPlatformApplication")]
    public void TestPropertyIsATestProject(string property) => Assert.True(IsTestProject(Property(property, "true")));

    [Theory]
    [InlineData("IsTestProject", "True")]
    [InlineData("IsTestProject", "TRUE")]
    [InlineData("IsTestingPlatformApplication", "True")]
    public void TestPropertyIsCaseInsensitive(string property, string value) => Assert.True(IsTestProject(Property(property, value)));

    [Theory]
    [InlineData("IsTestProject")]
    [InlineData("IsTestingPlatformApplication")]
    public void TestPropertySetFalseIsNotATestProject(string property) => Assert.False(IsTestProject(Property(property, "false")));

    [Fact]
    public void IsTestProjectSetFalseOverridesTheTestPackages() =>
        Assert.False(IsTestProject(Property("IsTestProject", "false"), Package("Microsoft.NET.Test.Sdk"), Package("Microsoft.Testing.Platform")));

    [Fact]
    public void MSTestSdkShapeIsATestProject() =>
        Assert.True(IsTestProject(Property("IsTestProject", "true"), Property("IsTestingPlatformApplication", "false")));

    // --- [PACKAGES]

    [Fact]
    public void TestingPlatformApplicationSetFalseDoesNotSuppressTheTestPackages() =>
        Assert.True(IsTestProject(Property("IsTestingPlatformApplication", "false"), Package("Microsoft.NET.Test.Sdk")));

    [Theory]
    [InlineData("Microsoft.NET.Test.Sdk")]
    [InlineData("microsoft.net.test.sdk")]
    [InlineData("Microsoft.Testing.Platform")]
    [InlineData("Microsoft.Testing.Extensions.CodeCoverage")]
    public void TestSdkPackageWithNoPropertiesIsATestProject(string package) => Assert.True(IsTestProject(Package(package)));

    [Fact]
    public void TestSdkPackageAmongOthersIsATestProject() => Assert.True(IsTestProject(Package("Newtonsoft.Json"), Package("Microsoft.NET.Test.Sdk"), Package("Moq")));

    [Theory]
    [InlineData("xunit")]
    [InlineData("xunit.v3")]
    [InlineData("xunit.v3.mtp-v2")]
    [InlineData("xunit.abstractions")]
    [InlineData("xunit.extensibility.core")]
    [InlineData("NUnit")]
    [InlineData("NUnit3TestAdapter")]
    [InlineData("NUnit.Analyzers")]
    [InlineData("MSTest.TestFramework")]
    [InlineData("TUnit")]
    public void FrameworkPackageAloneIsNotATestProject(string package) => Assert.False(IsTestProject(Package(package)));

    [Fact]
    public void PackageNamedLikeTheTestingPrefixIsNotATestProject() => Assert.False(IsTestProject(Package("Microsoft.TestingHelpers")));
}
