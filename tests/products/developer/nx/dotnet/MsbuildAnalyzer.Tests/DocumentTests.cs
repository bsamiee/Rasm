using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DocumentWorkspace() : Analyzed(
        ("docs/Inside/Inside.csproj", Documents("openapi")),
        ("docs/Absolute/Absolute.csproj", Documents("$(MSBuildProjectDirectory)/openapi")),
        ("docs/Above/Above.csproj", Documents("$(MSBuildProjectDirectory)/../../contracts/Above")),
        ("docs/Root/Root.csproj", Documents(".")),
        ("docs/Spaced/Spaced.csproj", Documents("openapi", "--file-name PublicApi")),
        ("docs/Equals/Equals.csproj", Documents("openapi", "--file-name=PublicApi")),
        ("docs/Colon/Colon.csproj", Documents("openapi", "--file-name:PublicApi")),
        ("docs/Quoted/Quoted.csproj", Documents("openapi", "--file-name &quot;Public-Api_v2&quot;")),
        ("docs/Later/Later.csproj", Documents("openapi", "--openapi-version v3.1 --file-name PublicApi")),
        ("docs/Unnamed/Unnamed.csproj", Documents("openapi", "--openapi-version v3.1 --document-name internal")),
        ("docs/Off/Off.csproj", TempWorkspace.Project("<PropertyGroup><OpenApiGenerateDocumentsOnBuild>false</OpenApiGenerateDocumentsOnBuild><OpenApiDocumentsDirectory>openapi</OpenApiDocumentsDirectory></PropertyGroup>")),
        ("docs/Outside/Outside.csproj", Documents("$(MSBuildProjectDirectory)/../../../../outside-workspace/openapi")),
        ("docs/Restore/Restore.csproj", Documents("obj/")),
        ("docs/Output/Output.csproj", Documents("bin/Debug/net10.0/openapi"))
) {
    private static string Documents(string directory, params ReadOnlySpan<string> options) =>
        TempWorkspace.Project($"<PropertyGroup><OpenApiDocumentsDirectory>{directory}</OpenApiDocumentsDirectory><OpenApiGenerateDocumentsOptions>{string.Join(' ', options)}</OpenApiGenerateDocumentsOptions></PropertyGroup>");
}

public sealed class DocumentTests(DocumentWorkspace analyzed) : IClassFixture<DocumentWorkspace> {

    // --- [DOCUMENTS]

    [Theory]
    [InlineData("Inside", "docs/Inside/openapi/Inside.json", "docs/Inside/openapi/Inside_*.json")]
    [InlineData("Absolute", "docs/Absolute/openapi/Absolute.json", "docs/Absolute/openapi/Absolute_*.json")]
    [InlineData("Above", "contracts/Above/Above.json", "contracts/Above/Above_*.json")]
    [InlineData("Root", "docs/Root/Root.json", "docs/Root/Root_*.json")]
    [InlineData("Spaced", "docs/Spaced/openapi/PublicApi.json", "docs/Spaced/openapi/PublicApi_*.json")]
    [InlineData("Equals", "docs/Equals/openapi/PublicApi.json", "docs/Equals/openapi/PublicApi_*.json")]
    [InlineData("Colon", "docs/Colon/openapi/PublicApi.json", "docs/Colon/openapi/PublicApi_*.json")]
    [InlineData("Quoted", "docs/Quoted/openapi/Public-Api_v2.json", "docs/Quoted/openapi/Public-Api_v2_*.json")]
    [InlineData("Later", "docs/Later/openapi/PublicApi.json", "docs/Later/openapi/PublicApi_*.json")]
    [InlineData("Unnamed", "docs/Unnamed/openapi/Unnamed.json", "docs/Unnamed/openapi/Unnamed_*.json")]
    public void BuildWritesTheDocumentAndItsNamedVariantsIntoTheDocumentsDirectoryInEveryConfiguration(string project, params string[] documents) {
        Assert.Equal(documents, analyzed.Node($"docs/{project}/{project}.csproj").Strings("evaluated", "documents"));
        Assert.Equal(documents, analyzed.Node($"docs/{project}/{project}.csproj").Mode("Release").Strings("documents"));
    }

    [Theory]
    [InlineData("Off")]
    [InlineData("Outside")]
    [InlineData("Restore")]
    [InlineData("Output")]
    public void NoDocumentIsAnOutputWhereGenerationIsOffOrTheDirectoryIsOutsideTheWorkspaceOrAnotherOutput(string project) =>
        Assert.Empty(analyzed.Node($"docs/{project}/{project}.csproj").Strings("evaluated", "documents"));
}
