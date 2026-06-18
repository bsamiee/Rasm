using Rasm.TestKit;

namespace Rasm.Materials.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void MaterialsRemainsAecDomainAndDoesNotReferenceAppPlatformProjects() {
        ProjectAdmission app = PackageAdmission.Project("libs/csharp/Rasm.Materials/Rasm.Materials.csproj");
        app.IncludesProjects("../Rasm/Rasm.csproj");
        app.ExcludesProjects("../Rasm.Compute/Rasm.Compute.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj");
        app.IncludesPackages("Wacton.Unicolour", "UnitsNet");
    }
}
