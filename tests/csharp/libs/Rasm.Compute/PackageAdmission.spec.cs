using Rasm.TestKit;

namespace Rasm.Compute.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void ComputeKeepsExecutionPackagesExplicitAndInProcessServerProofInTests() {
        PackageAdmission.ApiCatalogues(
            relativeDirectory: "libs/csharp/Rasm.Compute/.api",
            "api-grpc-aspnetcore.md",
            "api-microsoftaspnetcoretesthost.md",
            "api-extensions-ai.md");

        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Compute/Rasm.Compute.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj");
        app.IncludesOnlyPackages(
            "CommunityToolkit.HighPerformance",
            "CSparse",
            "GeneticSharp",
            "Google.OrTools",
            "libtorch-cpu",
            "MathNet.Numerics",
            "MathNet.Numerics.Providers.MKL",
            "MathNet.Numerics.Providers.OpenBLAS",
            "System.Numerics.Tensors",
            "TorchSharp",
            "FParsec",
            "MathNet.Symbolics",
            "BriefFiniteElement.Net",
            "BriefFiniteElementNet.CustomElements",
            "FEALiTE2D",
            "FEALiTE2D.Plotting",
            "NREL.OpenStudio.macOS-arm64",
            "Microsoft.Extensions.AI.Abstractions",
            "Microsoft.Extensions.Caching.Hybrid",
            "Microsoft.ML.OnnxRuntime",
            "Microsoft.ML.OnnxRuntime.DirectML",
            "Microsoft.ML.OnnxRuntime.Extensions",
            "Microsoft.ML.OnnxRuntime.Gpu",
            "Microsoft.ML.OnnxRuntimeGenAI",
            "Silk.NET.WebGPU",
            "Silk.NET.WebGPU.Extensions.WGPU",
            "Google.Protobuf",
            "Grpc.AspNetCore",
            "Grpc.Net.Client",
            "Grpc.Net.Client.Web",
            "Grpc.Net.Common",
            "NodaTime.Serialization.Protobuf",
            "Alimer.Bindings.MeshOptimizer",
            "Microsoft.IO.RecyclableMemoryStream",
            "SharpGLTF.Core",
            "SharpGLTF.Ext.3DTiles",
            "SharpGLTF.Toolkit",
            "NodaTime",
            "System.IO.Hashing",
            "Thinktecture.Runtime.Extensions.Json",
            "UnitsNet",
            "Grpc.Tools");
        app.PackageReferenceHasAttribute(packageName: "Grpc.Tools", attributeName: "PrivateAssets", expectedValue: "all");
        app.ExcludesPackages("Microsoft.AspNetCore.TestHost", "netDxf.netstandard", "Silk.NET.WebGPU.Native.WGPU");

        ProjectAdmission tests = PackageAdmission.Project(relativePath: "tests/csharp/libs/Rasm.Compute/Rasm.Compute.Tests.csproj");
        tests.IncludesOnlyPackages("Microsoft.AspNetCore.TestHost");
    }
}
