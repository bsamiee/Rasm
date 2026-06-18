using Rasm.TestKit;

namespace Rasm.Compute.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void ComputeProductionOwnsRemoteWireAndTestsOwnInProcessServerProof() {
        PackageAdmission.ApiCatalogues(
            "libs/csharp/Rasm.Compute/.api",
            "api-grpc-aspnetcore.md",
            "api-microsoftaspnetcoretesthost.md",
            "api-extensions-ai.md");

        ProjectAdmission app = PackageAdmission.Project("libs/csharp/Rasm.Compute/Rasm.Compute.csproj");
        app.IncludesProjects("../Rasm/Rasm.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj");
        app.IncludesPackages("Grpc.Net.Client", "Microsoft.ML.OnnxRuntime", "Microsoft.ML.OnnxRuntime.Extensions");
        app.ExcludesPackages("Microsoft.AspNetCore.TestHost");

        ProjectAdmission tests = PackageAdmission.Project("tests/csharp/libs/Rasm.Compute/Rasm.Compute.Tests.csproj");
        tests.IncludesPackages("Microsoft.AspNetCore.TestHost");
    }
}
