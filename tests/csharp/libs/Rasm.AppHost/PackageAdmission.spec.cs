using Rasm.TestKit;

namespace Rasm.AppHost.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void AppHostKeepsRuntimeSpinePackagesExplicitAndTestSeamsInTests() {
        PackageAdmission.ApiCatalogues(
            relativeDirectory: "libs/csharp/Rasm.AppHost/.api",
            "api-dataflow.md",
            "api-mcp.md",
            "api-pyroscope.md",
            "api-serilog-sinks.md");
        PackageAdmission.CentralPackages("System.Threading.Tasks.Dataflow");

        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.AppHost/Rasm.AppHost.csproj");
        app.IncludesNoProjects();
        app.IncludesOnlyPackages(
            "AspNetCore.HealthChecks.Kafka",
            "AspNetCore.HealthChecks.NpgSql",
            "AspNetCore.HealthChecks.Redis",
            "AspNetCore.HealthChecks.System",
            "AspNetCore.HealthChecks.Uris",
            "Cronos",
            "FluentValidation",
            "FluentValidation.DependencyInjectionExtensions",
            "Microsoft.AspNetCore.Authorization",
            "Microsoft.Extensions.Caching.Hybrid",
            "Microsoft.Extensions.Compliance.Redaction",
            "Microsoft.Extensions.Configuration",
            "Microsoft.Extensions.Configuration.Binder",
            "Microsoft.Extensions.Configuration.CommandLine",
            "Microsoft.Extensions.Configuration.EnvironmentVariables",
            "Microsoft.Extensions.Configuration.Json",
            "Microsoft.Extensions.Configuration.UserSecrets",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Diagnostics.HealthChecks",
            "Microsoft.Extensions.Diagnostics.ResourceMonitoring",
            "Microsoft.Extensions.Hosting",
            "Microsoft.Extensions.Hosting.Systemd",
            "Microsoft.Extensions.Http.Resilience",
            "Microsoft.Extensions.Logging.Abstractions",
            "Microsoft.Extensions.ObjectPool",
            "Microsoft.Extensions.Options",
            "Microsoft.Extensions.Options.ConfigurationExtensions",
            "Microsoft.Extensions.ServiceDiscovery",
            "Microsoft.Extensions.Telemetry",
            "Microsoft.Extensions.Telemetry.Abstractions",
            "Microsoft.IdentityModel.JsonWebTokens",
            "Microsoft.IdentityModel.Protocols.OpenIdConnect",
            "Microsoft.IdentityModel.Tokens",
            "NuGet.Versioning",
            "OpenFeature",
            "OpenIddict.Client",
            "OpenTelemetry",
            "OpenTelemetry.Exporter.OpenTelemetryProtocol",
            "OpenTelemetry.Extensions.Hosting",
            "OpenTelemetry.Instrumentation.Http",
            "OpenTelemetry.Instrumentation.Runtime",
            "Polly.Core",
            "Polly.Extensions",
            "Polly.RateLimiting",
            "Pyroscope.OpenTelemetry",
            "Scrutor",
            "Serilog",
            "Serilog.Extensions.Hosting",
            "Serilog.Sinks.Console",
            "Serilog.Sinks.File",
            "Sigstore",
            "System.CommandLine",
            "Thinktecture.Runtime.Extensions.Json",
            "NodaTime",
            "NodaTime.Serialization.SystemTextJson",
            "Grpc.AspNetCore",
            "Grpc.AspNetCore.HealthChecks",
            "Grpc.AspNetCore.Web",
            "Grpc.Core.Api",
            "Grpc.Net.Client",
            "Microsoft.AspNetCore.JsonPatch.SystemTextJson",
            "Velopack",
            "Microsoft.Extensions.AI",
            "Microsoft.Extensions.AI.Abstractions",
            "Microsoft.ML.Tokenizers",
            "Microsoft.ML.Tokenizers.Data.Cl100kBase",
            "Microsoft.ML.Tokenizers.Data.O200kBase",
            "ModelContextProtocol",
            "ModelContextProtocol.AspNetCore",
            "ModelContextProtocol.Core",
            "System.IO.Hashing",
            "System.Numerics.Tensors",
            "FluentModbus",
            "MQTTnet",
            "OPCFoundation.NetStandard.Opc.Ua",
            "OPCFoundation.NetStandard.Opc.Ua.PubSub",
            "System.IO.Ports",
            "Wasmtime");
        app.ExcludesPackages("System.Threading.Tasks.Dataflow", "Microsoft.Extensions.Diagnostics.Testing", "Microsoft.Extensions.TimeProvider.Testing", "NodaTime.Testing");

        ProjectAdmission tests = PackageAdmission.Project(relativePath: "tests/csharp/libs/Rasm.AppHost/Rasm.AppHost.Tests.csproj");
        tests.IncludesOnlyPackages("Microsoft.Extensions.Diagnostics.Testing", "Microsoft.Extensions.TimeProvider.Testing", "NodaTime.Testing");
    }
}
