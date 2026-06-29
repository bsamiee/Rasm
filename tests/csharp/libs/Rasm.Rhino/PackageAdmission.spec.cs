using Rasm.TestKit;

namespace Rasm.Rhino.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void RhinoBoundaryReferencesOnlyKernelAndDeclaresNoDirectPackages() {
        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj");
        app.IncludesNoPackages();
    }
}
