using Rasm.TestKit;

namespace Rasm.AppUi.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void AppUiKeepsProductUiPackagesExplicitAndSnapshotPackagesInTests() {
        PackageAdmission.ApiCatalogues(
            relativeDirectory: "libs/csharp/Rasm.AppUi/.api",
            "api-silk-webgpu.md",
            "api-headless.md",
            "api-verify.md",
            "api-avalonia-xaml-loader.md");

        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Compute/Rasm.Compute.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj");
        app.IncludesOnlyPackages(
            "Avalonia",
            "Avalonia.Controls.ColorPicker",
            "Avalonia.Controls.DataGrid",
            "Avalonia.Desktop",
            "Avalonia.Fonts.Inter",
            "Avalonia.Themes.Fluent",
            "ReactiveUI",
            "ReactiveUI.Avalonia",
            "ReactiveUI.Validation",
            "Xaml.Behaviors.Avalonia",
            "DynamicData",
            "LiveChartsCore.SkiaSharpView.Avalonia",
            "SkiaSharp",
            "SkiaSharp.HarfBuzz",
            "Svg.Controls.Skia.Avalonia",
            "Svg.Skia",
            "System.Reactive",
            "Avalonia.Headless",
            "Avalonia.Skia",
            "Silk.NET.OpenXR",
            "Silk.NET.OpenXR.Extensions.EXT",
            "Silk.NET.OpenXR.Extensions.FB",
            "Silk.NET.OpenXR.Extensions.KHR",
            "Silk.NET.WebGPU",
            "Silk.NET.WebGPU.Extensions.WGPU",
            "Silk.NET.WebGPU.Native.WGPU",
            "HarfBuzzSharp.NativeAssets.macOS",
            "SkiaSharp.NativeAssets.macOS",
            "AsyncImageLoader.Avalonia",
            "Avalonia.AvaloniaEdit",
            "AvaloniaEdit.TextMate",
            "bodong.Avalonia.PropertyGrid",
            "bodong.PropertyModels",
            "DialogHost.Avalonia",
            "Dock.Avalonia",
            "Dock.Model.ReactiveUI",
            "Dock.Serializer.SystemTextJson",
            "FluentIcons.Avalonia",
            "FluentIcons.Common",
            "Markdig",
            "PanAndZoom",
            "Thinktecture.Runtime.Extensions.Json",
            "HanumanInstitute.LibMpv",
            "HanumanInstitute.LibMpv.Avalonia",
            "Kiwi",
            "HidSharp",
            "Melanchall.DryWetMidi",
            "Silk.NET.Input",
            "Silk.NET.SDL",
            "NodaTime",
            "System.IO.Hashing",
            "UnitsNet",
            "Wacton.Unicolour",
            "ACadSharp",
            "DocumentFormat.OpenXml",
            "netDxf",
            "PDFsharp",
            "PDFsharp-MigraDoc",
            "Semi.Avalonia",
            "Semi.Avalonia.AvaloniaEdit",
            "Semi.Avalonia.ColorPicker",
            "Semi.Avalonia.DataGrid",
            "Semi.Avalonia.Dock",
            "Irihi.Ursa",
            "Irihi.Ursa.ReactiveUIExtension",
            "Irihi.Ursa.Themes.Semi",
            "Mapsui.Avalonia12",
            "NodeEditorAvalonia",
            "LoroCs",
            "MessageFormat",
            "Avalonia.Markup.Xaml.Loader",
            "HotAvalonia");
        app.PackageReferenceHasAttribute(packageName: "Avalonia.Markup.Xaml.Loader", attributeName: "PrivateAssets", expectedValue: "all");
        app.PackageReferenceHasAttribute(packageName: "HotAvalonia", attributeName: "PrivateAssets", expectedValue: "all");
        app.ExcludesPackages("Avalonia.Headless.XUnit", "Verify.XunitV3");

        ProjectAdmission tests = PackageAdmission.Project(relativePath: "tests/csharp/libs/Rasm.AppUi/Rasm.AppUi.Tests.csproj");
        tests.IncludesOnlyPackages("Avalonia.Headless.XUnit", "Avalonia.Skia", "Verify.XunitV3");
    }
}
