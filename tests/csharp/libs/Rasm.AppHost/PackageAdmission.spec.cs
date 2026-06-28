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
            "api-pyroscope.md",
            "api-serilog-sinks.md");
        PackageAdmission.CentralPackages("System.Threading.Tasks.Dataflow");

        ProjectAdmission app = PackageAdmission.Project("libs/csharp/Rasm.AppHost/Rasm.AppHost.csproj");
        app.IncludesPackages("ModelContextProtocol", "Pyroscope.OpenTelemetry", "Serilog.Sinks.Console", "Serilog.Sinks.File");
        app.ExcludesPackages("System.Threading.Tasks.Dataflow");
        app.ExcludesPackages("Microsoft.Extensions.Diagnostics.Testing", "Microsoft.Extensions.TimeProvider.Testing", "NodaTime.Testing");

        ProjectAdmission tests = PackageAdmission.Project("tests/csharp/libs/Rasm.AppHost/Rasm.AppHost.Tests.csproj");
        tests.IncludesPackages("Microsoft.Extensions.Diagnostics.Testing", "Microsoft.Extensions.TimeProvider.Testing", "NodaTime.Testing");
    }
}
