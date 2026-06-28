using Rasm.TestKit;

namespace Rasm.Materials.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void MaterialsRemainsAecDomainAndDoesNotReferenceAppPlatformProjects() {
        PackageAdmission.ApiCatalogues(
            "libs/csharp/Rasm.Materials/.api",
            "api-rectanglebinpack-csharp.md",
            "api-unicolour.md",
            "api-unitsnet.md");

        ProjectAdmission app = PackageAdmission.Project("libs/csharp/Rasm.Materials/Rasm.Materials.csproj");
        app.IncludesProjects("../Rasm/Rasm.csproj");
        app.ExcludesProjects("../Rasm.Compute/Rasm.Compute.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj");
        app.IncludesPackages("RectangleBinPack.CSharp", "UnitsNet", "VividOrange.Materials", "VividOrange.Sections", "Wacton.Unicolour");
    }
}
