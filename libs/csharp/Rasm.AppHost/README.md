# [APPHOST]

`Rasm.AppHost` is the federation's runtime spine — the host-neutral backbone every app root folds. It owns the composition-root doctrine, the reproducibility kernel — pinned RNG and float mode, the hash-chained command log, replay and bisect — the crash-durable workflow engine over one `CommandDispatch`, the agentic capability surface — a self-describing op catalog, commit-or-rollback command algebra, fenced grant and cost broker, MCP projection with external-server federation, governed model reasoning — the industrial external-binding studio, fleet provisioning and supply-chain admission, and the four-signal observability plane. Its bar is distributed honesty: a crash-durable claim covers node death, not just process reboot; delivery honesty is stated per sink; a dead holder's in-flight work reclaims under a fresh fenced lease; a replay claim survives process restarts against the durable store.

`Compute`, `Persistence`, and `AppUi` adapt to the spine and never reverse it; it mints one owner per runtime axis across the Runtime, Agent, Wire, Sandbox, and Observability folders, holds no AEC-domain reference, and contributes only the `ProjectionContext` ingredients the app composition root assembles.

## [01]-[ROUTER]

[RUNTIME]:
- [01]-[PROFILES](.planning/Runtime/profiles.md): Host-variance profile axis with lifetime adapters and power/thermal fidelity.
- [02]-[LIFECYCLE](.planning/Runtime/lifecycle.md): Total lifecycle/phase/drain/cancellation spine with fault-to-capture trigger.
- [03]-[TIME](.planning/Runtime/time.md): Injected clock pair, deadline taxonomy, and the one scheduler.
- [04]-[RESOURCES](.planning/Runtime/resources.md): Bounded resource lanes — hybrid cache, object pools, and drainable queues.
- [05]-[MODULES](.planning/Runtime/modules.md): One composition root folding and freezing the service graph.
- [06]-[CONFIG](.planning/Runtime/config.md): Ranked config-source chain with fail-closed source-gen binding.
- [07]-[SECRETS](.planning/Runtime/secrets.md): Credential-material lifecycle — lease acquire/renew/zeroize, PEM wire, and KMS-unwrap port.
- [08]-[PORTS](.planning/Runtime/ports.md): Inward port records — the cross-package seam every peer crosses.
- [09]-[DETERMINISM](.planning/Runtime/determinism.md): Reproducibility kernel — pinned RNG/float-mode and the hash-chained command log.
- [10]-[ORCHESTRATION](.planning/Runtime/orchestration.md): Crash-durable workflow and persistent-job owner over the command/event/schedule ports.
- [11]-[LANEGUARD](.planning/Runtime/laneguard.md): In-process `WorkLane` resilience governor — bulkhead, adaptive concurrency, load-shed, hedge.
- [12]-[FEATURES](.planning/Runtime/features.md): Config-backed OpenFeature targeting and rollout minting the one `FlagVerdict` seam.

[AGENT]:
- [13]-[MCP](.planning/Agent/mcp.md): MCP-server projection of descriptors to tools, resources, and prompts.
- [14]-[REASONING](.planning/Agent/reasoning.md): In-process agent loop with model-selection and content-filter governance.
- [15]-[FEDERATION](.planning/Agent/federation.md): Folds external MCP servers into one registry as brokered descriptors.
- [16]-[CAPABILITY](.planning/Agent/capability.md): Self-describing op catalog, command algebra, and fenced distributed quota.
- [17]-[IDENTITY](.planning/Agent/identity.md): Authentication boundary — OIDC issuer-trust, rotating token validation, claims-policy gate.
- [18]-[RUNTIME](.planning/Agent/runtime.md): One command-dispatch front door over the command algebra, tool adoption, and receipt.

[WIRE]:
- [19]-[OUTBOUND](.planning/Wire/outbound.md): Single outbound boundary with per-seam retry/cache and delivery fan-out.
- [20]-[LIVEWIRE](.planning/Wire/livewire.md): Reactive bidirectional external-binding studio over the industrial-transport axis.
- [21]-[COMPANION](.planning/Wire/companion.md): Multi-process modality axis and gRPC-over-UDS control-service host.
- [22]-[TOPICS](.planning/Wire/topics.md): In-process event-bus topology with fan-out, join, and coalesce builders.
- [23]-[OUTBOX](.planning/Wire/outbox.md): Transactional outbox and dead-letter relay over the watermark dispatch sweep.
- [24]-[COORDINATION](.planning/Wire/coordination.md): Cluster membership, election, and distributed-lock over the fenced lease.

[SANDBOX]:
- [25]-[ADMISSION](.planning/Sandbox/admission.md): One supply-chain admission gate — offline Sigstore, SLSA provenance, SemVer contract.
- [26]-[ISOLATION](.planning/Sandbox/isolation.md): Capability-brokered WASM and process plugin isolation with unified call mediation.
- [27]-[SOLVER](.planning/Sandbox/solver.md): Solver-plugin contract with canonical-representation negotiation.
- [28]-[PROVISIONING](.planning/Sandbox/provisioning.md): Post-fetch self-update state machine over the canary, blue-green, and linear-wave roll axis.

[OBSERVABILITY]:
- [29]-[TELEMETRY](.planning/Observability/telemetry.md): Unified four-signal telemetry through minted identities and egress redaction.
- [30]-[HEALTH](.planning/Observability/health.md): Resource-pressure health fold and degradation/alert rails over one atomic reading cell.
- [31]-[BUNDLES](.planning/Observability/bundles.md): Bounded redacted support capture.

## [02]-[DOMAIN_PACKAGES]

AppHost-domain libraries admitted by this folder; versions centralize in the one C# manifest and corroborate against this folder's `.api/`.

[HOSTING_COMPOSITION]:
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.Hosting.Systemd`
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
- `System.CommandLine` — projects the app-root verb table onto `CommandDispatch.Run`.
- `Cronos`
- `Microsoft.Extensions.Caching.Hybrid`
- `Microsoft.Extensions.ObjectPool`
- `System.Threading.Tasks.Dataflow` — carries the drainable-queue dataflow blocks, framework-provided with no package row.

[OBSERVABILITY]:
- `Microsoft.Extensions.Diagnostics.HealthChecks`
- `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- `Microsoft.Diagnostics.NETCore.Client` — process-dump and EventPipe capture feeding the support-bundle fan.
- `Microsoft.Diagnostics.Tracing.TraceEvent` — decodes the EventPipe event stream off the diagnostics session.
- `AspNetCore.HealthChecks.NpgSql`
- `AspNetCore.HealthChecks.Redis`
- `AspNetCore.HealthChecks.System`
- `AspNetCore.HealthChecks.Uris`
- `AspNetCore.HealthChecks.Kafka`
- `AspNetCore.HealthChecks.Nats` — probes the NATS broker anchor over the pooled connection.
- `Microsoft.Extensions.Compliance.Redaction`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Telemetry`
- `Microsoft.Extensions.Telemetry.Abstractions`
- `OpenTelemetry`
- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.Http`
- `OpenTelemetry.Instrumentation.Runtime`
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` — projects the OTLP exporter at the composition root.
- `Pyroscope.OpenTelemetry`
- `Serilog`
- `Serilog.Extensions.Hosting`
- `Serilog.Sinks.Console` — composition-root sink projection.
- `Serilog.Sinks.File` — composition-root sink projection.

[OUTBOUND_TRANSPORT]:
- `Microsoft.Extensions.Http.Resilience`
- `Polly.Core`
- `Polly.Extensions`
- `Polly.RateLimiting`
- `Microsoft.Extensions.ServiceDiscovery`
- `Grpc.AspNetCore.HealthChecks`
- `Grpc.AspNetCore.Web` — binds the `UseGrpcWeb` middleware at the composition root.
- `Grpc.Core.Api`
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`

[CAPABILITY_AGENT]:
- `Microsoft.Extensions.AI.Abstractions`
- `Microsoft.Extensions.AI`
- `ModelContextProtocol`
- `ModelContextProtocol.Core`
- `ModelContextProtocol.AspNetCore`
- `Microsoft.ML.Tokenizers`
- `Microsoft.ML.Tokenizers.Data.O200kBase`
- `Microsoft.ML.Tokenizers.Data.Cl100kBase`

[IDENTITY_SUPPLY]:
- `OpenIddict.Client`
- `Microsoft.AspNetCore.Authorization`
- `Microsoft.IdentityModel.JsonWebTokens`
- `Microsoft.IdentityModel.Tokens`
- `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- `Microsoft.IdentityModel.Protocols` — drives the `ConfigurationManager<T>` refresh seam under OpenIdConnect.
- `Sigstore`
- `NuGet.Versioning`
- `Velopack`
- `OpenFeature`

[SANDBOX_LIVE_WIRE]:
- `Wasmtime`
- `OPCFoundation.NetStandard.Opc.Ua`
- `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- `MQTTnet`
- `FluentModbus`
- `BACnet` — backs the building-automation `bacnet` `ExternalTransport` row feeding twin calibration.
- `MTConnect.NET-Common` — backs the machine-tool `mtconnect` `ExternalTransport` row feeding Fabrication tool-life and probing.
- `System.IO.Ports`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting C# substrate AppHost consumes; the C# registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

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
