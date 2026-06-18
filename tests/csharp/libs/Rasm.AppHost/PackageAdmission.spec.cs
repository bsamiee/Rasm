using Rasm.TestKit;

namespace Rasm.AppHost.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void RuntimeSpinePackagesAreCentrallyOwnedAndTestSeamsStayInTests() {
        PackageAdmission.ApiCatalogues(
            "libs/csharp/Rasm.AppHost/.api",
            "api-dataflow.md",
            "api-mcp.md",
            "api-pyroscope.md");

        ProjectAdmission app = PackageAdmission.Project("libs/csharp/Rasm.AppHost/Rasm.AppHost.csproj");
        app.IncludesPackages("System.Threading.Tasks.Dataflow", "ModelContextProtocol", "Pyroscope.OpenTelemetry");
        app.ExcludesPackages("Microsoft.Extensions.Diagnostics.Testing", "Microsoft.Extensions.TimeProvider.Testing", "NodaTime.Testing");

        ProjectAdmission tests = PackageAdmission.Project("tests/csharp/libs/Rasm.AppHost/Rasm.AppHost.Tests.csproj");
        tests.IncludesPackages("Microsoft.Extensions.Diagnostics.Testing", "Microsoft.Extensions.TimeProvider.Testing", "NodaTime.Testing");
    }
}
