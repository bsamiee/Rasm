using Rasm.TestKit;

namespace Rasm.Materials.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void MaterialsRemainsAecDomainAndDoesNotReferenceAppPlatformProjects() {
        PackageAdmission.ApiCatalogues(
            relativeDirectory: "libs/csharp/Rasm.Materials/.api",
            "api-rectanglebinpack-csharp.md",
            "api-unicolour.md",
            "api-unitsnet.md");

        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Materials/Rasm.Materials.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj");
        app.IncludesOnlyPackages(
            "Wacton.Unicolour",
            "Wacton.Unicolour.Datasets",
            "UnitsNet",
            "VividOrange.Profiles.Catalogue",
            "VividOrange.Sections",
            "VividOrange.Sections.SectionProperties",
            "VividOrange.Materials",
            "VividOrange.Standards",
            "VividOrange.InteractionDiagram",
            "VividOrange.Uncertainties",
            "VividOrange.Uncertainties.Quantities",
            "RectangleBinPack.CSharp",
            "MathNet.Numerics",
            "Thinktecture.Runtime.Extensions.Json",
            "Thinktecture.Runtime.Extensions.MessagePack");
        app.ExcludesProjects("../Rasm.Compute/Rasm.Compute.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj");
    }
}
