using Rasm.TestKit;

namespace Rasm.Grasshopper.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void GrasshopperBoundaryReferencesOnlyKernelAndDeclaresNoDirectPackages() {
        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj");
        app.IncludesNoPackages();
    }
}
