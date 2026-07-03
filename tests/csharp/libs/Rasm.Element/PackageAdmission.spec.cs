using Rasm.TestKit;

namespace Rasm.Element.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void ElementSeamStaysProviderFreeAndUsesSharedSubstrateCatalogues() {
        PackageAdmission.ApiCatalogues(
            relativeDirectory: "libs/csharp/.api",
            "api-generator-equals.md",
            "api-hashing.md",
            "api-mapperly.md",
            "api-nodatime.md",
            "api-quikgraph.md",
            "api-thinktecture-json.md",
            "api-unitsnet.md");
        Assert.Empty(collection: PackageAdmission.Files(relativeRoot: "libs/csharp/Rasm.Element/.api", pattern: "*.md"));

        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Element/Rasm.Element.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj");
        app.IncludesOnlyPackages(
            "UnitsNet",
            "NodaTime",
            "NodaTime.Serialization.Protobuf",
            "QuikGraph",
            "System.IO.Hashing",
            "Generator.Equals",
            "Google.Protobuf",
            "Riok.Mapperly",
            "Thinktecture.Runtime.Extensions.Json",
            "Grpc.Tools");
        app.PackageReferenceHasAttribute(packageName: "Grpc.Tools", attributeName: "PrivateAssets", expectedValue: "all");
        app.ExcludesProjects(
            "../Rasm.Materials/Rasm.Materials.csproj",
            "../Rasm.Bim/Rasm.Bim.csproj",
            "../Rasm.Fabrication/Rasm.Fabrication.csproj",
            "../Rasm.Compute/Rasm.Compute.csproj",
            "../Rasm.Persistence/Rasm.Persistence.csproj",
            "../Rasm.AppHost/Rasm.AppHost.csproj");
        app.ExcludesPackages("GeometryGymIFC_Core", "VividOrange.Materials", "NetTopologySuite", "Npgsql", "NREL.OpenStudio.macOS-arm64");
    }
}
