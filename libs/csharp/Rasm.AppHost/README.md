# [APPHOST]

`Rasm.AppHost` is the APP-PLATFORM runtime spine — the host-neutral C# backbone every sibling (Compute, Persistence, AppUi) adapts to and never reverses. It owns host-profile variance, lifecycle/drain/cancellation, time/deadlines/scheduling, configuration/composition, resource lanes, four-signal observability, outbound resilience, the seven inward port records, post-fetch provisioning, the companion/sidecar control host, and a capability-and-extensibility platform (self-describing op catalog, command algebra, grant/cost broker, polyglot SDK codegen, MCP projection, plugin sandbox, solver contract, live external binding, determinism/replay). This README routes the `.planning/` design pages and lists every external package the folder uses; the sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- hosting: [host-profiles](.planning/hosting/host-profiles.md), [lifecycle-and-drain](.planning/hosting/lifecycle-and-drain.md)
- time: [time-and-deadlines](.planning/time/time-and-deadlines.md)
- configuration: [configuration-and-options](.planning/configuration/configuration-and-options.md), [composition-and-modules](.planning/configuration/composition-and-modules.md)
- resources: [resource-lanes](.planning/resources/resource-lanes.md)
- observability: [diagnostics-and-telemetry](.planning/observability/diagnostics-and-telemetry.md), [health-and-degradation](.planning/observability/health-and-degradation.md), [support-bundles](.planning/observability/support-bundles.md)
- outbound: [outbound-resilience](.planning/outbound/outbound-resilience.md)
- ports: [runtime-ports](.planning/ports/runtime-ports.md)
- provisioning: [provisioning-and-update](.planning/provisioning/provisioning-and-update.md)
- companion: [companion-sidecar](.planning/companion/companion-sidecar.md)
- capability: [registry](.planning/capability/registry.md)
- agent: [mcp-projection](.planning/agent/mcp-projection.md), [tool-federation](.planning/agent/tool-federation.md), [reasoning-runtime](.planning/agent/reasoning-runtime.md)
- sandbox: [sandbox-host](.planning/sandbox/sandbox-host.md), [solver-plugin](.planning/sandbox/solver-plugin.md)
- live-wire: [live-wire](.planning/live-wire/live-wire.md)
- determinism: [determinism-and-replay](.planning/determinism/determinism-and-replay.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions are centralized in the one C# manifest and never pinned here; admissions land here from the folder's ideas and tasks.

[HOSTING_LIFECYCLE]:
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Hosting.Systemd

[CONFIGURATION_COMPOSITION]:
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.Binder
- Microsoft.Extensions.Configuration.CommandLine
- Microsoft.Extensions.Configuration.EnvironmentVariables
- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.Configuration.UserSecrets
- Microsoft.Extensions.Options
- Microsoft.Extensions.Options.ConfigurationExtensions
- Microsoft.Extensions.DependencyInjection
- FluentValidation
- FluentValidation.DependencyInjectionExtensions
- Scrutor
- System.CommandLine

[TIME]:
- Cronos
- NodaTime
- NodaTime.Serialization.SystemTextJson

[RESOURCES]:
- Microsoft.Extensions.Caching.Hybrid
- Microsoft.Extensions.ObjectPool
- System.Threading.Tasks.Dataflow

[OBSERVABILITY]:
- Microsoft.Extensions.Diagnostics.HealthChecks
- Microsoft.Extensions.Diagnostics.ResourceMonitoring
- Microsoft.Extensions.Compliance.Redaction
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Telemetry
- Microsoft.Extensions.Telemetry.Abstractions
- OpenTelemetry
- OpenTelemetry.Extensions.Hosting
- OpenTelemetry.Instrumentation.Http
- OpenTelemetry.Instrumentation.Runtime
- OpenTelemetry.Exporter.OpenTelemetryProtocol
- Pyroscope.OpenTelemetry
- Serilog
- Serilog.Extensions.Hosting

[OUTBOUND]:
- Microsoft.Extensions.Http.Resilience
- Polly.Core
- Polly.Extensions
- Polly.RateLimiting

[PORTS_TRANSPORT]:
- Grpc.Net.Client
- Grpc.AspNetCore
- Grpc.AspNetCore.Web
- Grpc.AspNetCore.HealthChecks
- Grpc.Core.Api
- Microsoft.AspNetCore.JsonPatch.SystemTextJson

[PROVISIONING]:
- Velopack

[CAPABILITY_AGENT]:
- Microsoft.Extensions.AI.Abstractions
- Microsoft.Extensions.AI
- ModelContextProtocol
- ModelContextProtocol.Core
- ModelContextProtocol.AspNetCore
- System.IO.Hashing

[SANDBOX_LIVE_WIRE]:
- Wasmtime
- OPCFoundation.NetStandard.Opc.Ua
- OPCFoundation.NetStandard.Opc.Ua.PubSub
- MQTTnet
- FluentModbus
- System.IO.Ports

[FUNCTIONAL_CORE]:
- LanguageExt.Core
- Thinktecture.Runtime.Extensions
- Thinktecture.Runtime.Extensions.Json

[TESTING]:
- Microsoft.Extensions.TimeProvider.Testing
- NodaTime.Testing
- Microsoft.Extensions.Diagnostics.Testing
