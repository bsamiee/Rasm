# [APPHOST]

`Rasm.AppHost` is the APP-PLATFORM runtime spine — the host-neutral C# backbone that `Compute`, `Persistence`, and `AppUi` adapt to and never reverse. It owns host-profile variance, lifecycle/drain/cancellation, time/deadlines/scheduling, configuration/composition, resource lanes, four-signal observability, outbound resilience, the seven inward port records, post-fetch provisioning, the companion/sidecar control host, and a capability-and-extensibility platform: a self-describing op catalog, command algebra, grant/cost broker, polyglot SDK codegen, MCP projection, plugin sandbox, solver contract, live external binding, and determinism/replay. The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[PROFILES](.planning/Runtime/profiles.md)
- [02]-[LIFECYCLE](.planning/Runtime/lifecycle.md)
- [03]-[TIME](.planning/Runtime/time.md)
- [04]-[RESOURCES](.planning/Runtime/resources.md)
- [05]-[MODULES](.planning/Runtime/modules.md)
- [06]-[CONFIG](.planning/Runtime/config.md)
- [07]-[PORTS](.planning/Runtime/ports.md)
- [08]-[DETERMINISM](.planning/Runtime/determinism.md)
- [09]-[MCP](.planning/Agent/mcp.md)
- [10]-[REASONING](.planning/Agent/reasoning.md)
- [11]-[FEDERATION](.planning/Agent/federation.md)
- [12]-[CAPABILITY](.planning/Agent/capability.md)
- [13]-[OUTBOUND](.planning/Wire/outbound.md)
- [14]-[LIVEWIRE](.planning/Wire/livewire.md)
- [15]-[COMPANION](.planning/Wire/companion.md)
- [16]-[ISOLATION](.planning/Sandbox/isolation.md)
- [17]-[SOLVER](.planning/Sandbox/solver.md)
- [18]-[PROVISIONING](.planning/Sandbox/provisioning.md)
- [19]-[TELEMETRY](.planning/Observability/telemetry.md)
- [20]-[HEALTH](.planning/Observability/health.md)
- [21]-[BUNDLES](.planning/Observability/bundles.md)

## [02]-[DOMAIN_PACKAGES]

Every AppHost-domain library the folder uses, planned or implemented. Versions are centralized in the one C# manifest and never pinned here; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below.

[HOSTING_LIFECYCLE]:
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.Hosting.Systemd`

[CONFIGURATION_COMPOSITION]:
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Configuration.Binder`
- `Microsoft.Extensions.Configuration.CommandLine`
- `Microsoft.Extensions.Configuration.EnvironmentVariables`
- `Microsoft.Extensions.Configuration.Json`
- `Microsoft.Extensions.Configuration.UserSecrets`
- `Microsoft.Extensions.Options`
- `Microsoft.Extensions.Options.ConfigurationExtensions`
- `Microsoft.Extensions.DependencyInjection`
- `FluentValidation`
- `FluentValidation.DependencyInjectionExtensions`
- `Scrutor`
- `System.CommandLine`

[TIME]:
- `Cronos`

[RESOURCES]:
- `Microsoft.Extensions.Caching.Hybrid`
- `Microsoft.Extensions.ObjectPool`
- `System.Threading.Tasks.Dataflow`

[OBSERVABILITY]:
- `Microsoft.Extensions.Diagnostics.HealthChecks`
- `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- `Microsoft.Extensions.Compliance.Redaction`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Telemetry`
- `Microsoft.Extensions.Telemetry.Abstractions`
- `OpenTelemetry`
- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.Http`
- `OpenTelemetry.Instrumentation.Runtime`
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- `Pyroscope.OpenTelemetry`
- `Serilog`
- `Serilog.Extensions.Hosting`

[OUTBOUND]:
- `Microsoft.Extensions.Http.Resilience`
- `Polly.Core`
- `Polly.Extensions`
- `Polly.RateLimiting`
- `Microsoft.Extensions.ServiceDiscovery`

[PORTS_TRANSPORT]:
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`

[PROVISIONING]:
- `Velopack`

[CAPABILITY_AGENT]:
- `Microsoft.Extensions.AI.Abstractions`
- `Microsoft.Extensions.AI`
- `ModelContextProtocol`
- `ModelContextProtocol.Core`
- `ModelContextProtocol.AspNetCore`

[IDENTITY_AUTHZ]:
- `OpenIddict.Client`
- `Microsoft.AspNetCore.Authorization`

[FEATURE_FLAGS]:
- `OpenFeature`

[SANDBOX_LIVE_WIRE]:
- `Wasmtime`
- `OPCFoundation.NetStandard.Opc.Ua`
- `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- `MQTTnet`
- `FluentModbus`
- `System.IO.Ports`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting C# substrate libraries AppHost consumes; these are owned at the monorepo substrate layer. Package charters and API evidence live in `libs/csharp/.planning/README.md` and the adjacent `.api/` folder.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `System.IO.Hashing`

[WIRE_CODEGEN]:
- `Grpc.Net.Client`
- `Grpc.AspNetCore`
- `Grpc.AspNetCore.Web`
- `Grpc.AspNetCore.HealthChecks`
- `Grpc.Core.Api`

[TEST_SUBSTRATE]:
- `Microsoft.Extensions.TimeProvider.Testing`
- `NodaTime.Testing`
- `Microsoft.Extensions.Diagnostics.Testing`
