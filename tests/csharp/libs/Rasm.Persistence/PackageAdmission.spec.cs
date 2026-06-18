using Rasm.TestKit;

namespace Rasm.Persistence.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void PersistenceStorePackagesAreCentralAndManagedSpecsCarryNoRuntimeProofPackages() {
        PackageAdmission.ApiCatalogues(
            "libs/csharp/Rasm.Persistence/.api",
            "api-arrow.md",
            "api-redis.md",
            "api-thinktecture-ef.md");

        ProjectAdmission app = PackageAdmission.Project("libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj");
        app.IncludesProjects("../Rasm.AppHost/Rasm.AppHost.csproj");
        app.IncludesPackages("DuckDB.NET.Data.Full", "Google.Cloud.Storage.V1", "Microsoft.EntityFrameworkCore.Design", "Thinktecture.Runtime.Extensions.EntityFrameworkCore10");

        ProjectAdmission tests = PackageAdmission.Project("tests/csharp/libs/Rasm.Persistence/Rasm.Persistence.Tests.csproj");
        tests.ExcludesPackages("Verify.XunitV3", "Microsoft.AspNetCore.TestHost");
    }
}
