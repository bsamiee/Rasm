# [APPHOST]

`Rasm.AppHost` is the APP-PLATFORM runtime spine — the host-neutral C# backbone that `Compute`, `Persistence`, and `AppUi` adapt to and never reverse. It owns host-profile variance, lifecycle/drain/cancellation, time/deadlines/scheduling, configuration/composition, resource lanes, four-signal observability, outbound resilience, the seven inward port records, signature-and-contract-verified post-fetch provisioning, the companion/sidecar control host, an OIDC authentication boundary, and a capability-and-extensibility platform: a self-describing op catalog, command algebra, grant/cost broker, polyglot SDK codegen, MCP projection, plugin sandbox, solver contract, live external binding, and determinism/replay. The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[PROFILES](.planning/Runtime/profiles.md)
- [02]-[LIFECYCLE](.planning/Runtime/lifecycle.md)
- [03]-[TIME](.planning/Runtime/time.md)
- [04]-[RESOURCES](.planning/Runtime/resources.md)
- [05]-[MODULES](.planning/Runtime/modules.md)
- [06]-[CONFIG](.planning/Runtime/config.md)
- [07]-[PORTS](.planning/Runtime/ports.md)
- [08]-[DETERMINISM](.planning/Runtime/determinism.md)
- [09]-[ORCHESTRATION](.planning/Runtime/orchestration.md)
- [10]-[LANEGUARD](.planning/Runtime/laneguard.md)
- [11]-[FEATURES](.planning/Runtime/features.md)
- [12]-[MCP](.planning/Agent/mcp.md)
- [13]-[REASONING](.planning/Agent/reasoning.md)
- [14]-[FEDERATION](.planning/Agent/federation.md)
- [15]-[CAPABILITY](.planning/Agent/capability.md)
- [16]-[IDENTITY](.planning/Agent/identity.md)
- [17]-[RUNTIME](.planning/Agent/runtime.md)
- [18]-[OUTBOUND](.planning/Wire/outbound.md)
- [19]-[LIVEWIRE](.planning/Wire/livewire.md)
- [20]-[COMPANION](.planning/Wire/companion.md)
- [21]-[TOPICS](.planning/Wire/topics.md)
- [22]-[OUTBOX](.planning/Wire/outbox.md)
- [23]-[COORDINATION](.planning/Wire/coordination.md)
- [24]-[ISOLATION](.planning/Sandbox/isolation.md)
- [25]-[SOLVER](.planning/Sandbox/solver.md)
- [26]-[PROVISIONING](.planning/Sandbox/provisioning.md)
- [27]-[TELEMETRY](.planning/Observability/telemetry.md)
- [28]-[HEALTH](.planning/Observability/health.md)
- [29]-[BUNDLES](.planning/Observability/bundles.md)

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
- `Microsoft.Diagnostics.NETCore.Client` — process-dump + EventPipe capture for the support-bundle `[DUMP_ADMISSION]` fan.
- `Microsoft.Diagnostics.Tracing.TraceEvent` — the EventPipe/ETW event-stream decoder consuming the diagnostics-client session.
- `AspNetCore.HealthChecks.NpgSql`
- `AspNetCore.HealthChecks.Redis`
- `AspNetCore.HealthChecks.System`
- `AspNetCore.HealthChecks.Uris`
- `AspNetCore.HealthChecks.Kafka`
- `AspNetCore.HealthChecks.Nats` — the `[V6]` NATS broker-anchor probe row binding the pooled `NATS.Net` connection.
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
- `Serilog.Sinks.Console`
- `Serilog.Sinks.File`

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
- `Microsoft.ML.Tokenizers`
- `Microsoft.ML.Tokenizers.Data.O200kBase`
- `Microsoft.ML.Tokenizers.Data.Cl100kBase`

[IDENTITY_AUTHZ]:
- `OpenIddict.Client`
- `Microsoft.AspNetCore.Authorization`
- `Microsoft.IdentityModel.JsonWebTokens`
- `Microsoft.IdentityModel.Tokens`
- `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- `Microsoft.IdentityModel.Protocols` — transitive base of OpenIdConnect and `OpenIddict.Client`, version-locked; the `ConfigurationManager<T>` refresh seam.

[SUPPLY_CHAIN]:
- `Sigstore`
- `NuGet.Versioning`

[FEATURE_FLAGS]:
- `OpenFeature`

[SANDBOX_LIVE_WIRE]:
- `Wasmtime`
- `OPCFoundation.NetStandard.Opc.Ua`
- `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- `MQTTnet`
- `FluentModbus`
- `BACnet` — building-automation transport (ela-compil `System.IO.BACnet`); one `bacnet` `ExternalTransport` row feeding the twin-calibration lane.
- `MTConnect.NET-Common` — machine-tool connectivity model slice (TrakHound); one `mtconnect` `ExternalTransport` row feeding Fabrication tool-life/probing.
- `System.IO.Ports`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting C# substrate libraries AppHost consumes; package charters live in `libs/csharp/.planning/README.md` and shared API evidence lives in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `System.Numerics.Tensors`

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
