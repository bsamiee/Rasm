using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TestDetectionWorkspace() : Analyzed(
        ("cases/Plain/Plain.csproj", TempWorkspace.Project()),
        ("cases/TestProject/TestProject.csproj", TempWorkspace.Project("<PropertyGroup><IsTestProject>true</IsTestProject></PropertyGroup>")),
        ("cases/PlatformApplication/PlatformApplication.csproj", TempWorkspace.Project("<PropertyGroup><IsTestingPlatformApplication>True</IsTestingPlatformApplication></PropertyGroup>")),
        ("cases/TestSdk/TestSdk.csproj", TempWorkspace.Project(Package("microsoft.net.test.sdk"))),
        ("cases/TestingPackage/TestingPackage.csproj", TempWorkspace.Project(Package("Microsoft.Testing.Platform"))),
        ("cases/TestingPrefix/TestingPrefix.csproj", TempWorkspace.Project(Package("Microsoft.TestingHelpers"))),
        ("cases/FrameworkAlone/FrameworkAlone.csproj", TempWorkspace.Project(Package("xunit.v3"))),
        ("cases/DeclinedWithTestSdk/DeclinedWithTestSdk.csproj", TempWorkspace.Project("<PropertyGroup><IsTestProject>false</IsTestProject></PropertyGroup>", Package("Microsoft.NET.Test.Sdk"))),
        ("cases/NoPlatformWithTestSdk/NoPlatformWithTestSdk.csproj", TempWorkspace.Project("<PropertyGroup><IsTestingPlatformApplication>false</IsTestingPlatformApplication></PropertyGroup>", Package("Microsoft.NET.Test.Sdk"))),
        ("cases/OneFramework/OneFramework.csproj", """<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFrameworks>net10.0;net9.0</TargetFrameworks></PropertyGroup><PropertyGroup Condition="'$(TargetFramework)' == 'net9.0'"><IsTestProject>true</IsTestProject></PropertyGroup></Project>""")
) {
    private static string Package(string id) => $"""<ItemGroup><PackageReference Include="{id}" Version="1.0.0" /></ItemGroup>""";
}

public sealed class TestDetectionTests(TestDetectionWorkspace analyzed) : IClassFixture<TestDetectionWorkspace> {

    // --- [DETECTION]

    [Theory]
    [InlineData("Plain", false)]
    [InlineData("TestProject", true)]
    [InlineData("PlatformApplication", true)]
    [InlineData("TestSdk", true)]
    [InlineData("TestingPackage", true)]
    [InlineData("TestingPrefix", false)]
    [InlineData("FrameworkAlone", false)]
    [InlineData("DeclinedWithTestSdk", false)]
    [InlineData("NoPlatformWithTestSdk", true)]
    [InlineData("OneFramework", true)]
    public void ProjectIsATestProjectByItsTestPropertiesElseItsTestPlatformPackages(string project, bool test) =>
        Assert.Equal(test, analyzed.Node($"cases/{project}/{project}.csproj").GetProperty("test").GetBoolean());
}
