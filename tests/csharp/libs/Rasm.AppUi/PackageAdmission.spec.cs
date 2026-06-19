using Rasm.TestKit;

namespace Rasm.AppUi.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void AppUiBackendsAndSnapshotPackagesAreRoutedByOwner() {
        PackageAdmission.ApiCatalogues(
            "libs/csharp/Rasm.AppUi/.api",
            "api-silk-webgpu.md",
            "api-headless.md",
            "api-verify.md",
            "api-avalonia-xaml-loader.md");

        ProjectAdmission app = PackageAdmission.Project("libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj");
        app.IncludesPackages("Avalonia.Headless", "Avalonia.Skia", "Silk.NET.WebGPU", "Avalonia.Markup.Xaml.Loader");
        app.ExcludesPackages("Avalonia.Headless.XUnit", "Verify.XunitV3");

        ProjectAdmission tests = PackageAdmission.Project("tests/csharp/libs/Rasm.AppUi/Rasm.AppUi.Tests.csproj");
        tests.IncludesPackages("Avalonia.Headless.XUnit", "Avalonia.Skia", "Verify.XunitV3");
    }
}
