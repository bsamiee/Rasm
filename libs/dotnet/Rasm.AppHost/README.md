# [APPHOST]

`Rasm.AppHost` is the host-neutral runtime spine every app root folds, minting one owner per runtime axis from composition-root doctrine and the reproducibility kernel through the observability plane. Crash-durable claims cover node death, delivery states per sink, and replay survives process restarts against the durable store.

## [01]-[ROUTER]

[RUNTIME]:
- [01]-[PROFILES](.planning/Runtime/profiles.md): Consumption-profile boot — `Resolve`-admitted axis row, identity fold, power-fidelity scaling.
- [02]-[LIFECYCLE](.planning/Runtime/lifecycle.md): Lifecycle spine — phase transitions, fault traps, drain completion, cancellation provenance.
- [03]-[TIME](.planning/Runtime/time.md): Injected clock pair, deadline taxonomy, and the one scheduler.
- [04]-[RESOURCES](.planning/Runtime/resources.md): Bounded resource lanes — hybrid cache, object pools, and drainable queues.
- [05]-[MODULES](.planning/Runtime/modules.md): One composition root folding and freezing the service graph.
- [06]-[CONFIG](.planning/Runtime/config.md): Ranked config-source chain with fail-closed source-gen binding.
- [07]-[SECRETS](.planning/Runtime/secrets.md): Credential-material lifecycle — lease acquire/renew/zeroize, public DER wire, and KMS-unwrap port.
- [08]-[PORTS](.planning/Runtime/ports.md): Inward port records — the cross-package boundary every peer crosses.
- [09]-[DETERMINISM](.planning/Runtime/determinism.md): Reproducibility kernel — pinned RNG/float-mode and the hash-chained command log.
- [10]-[ORCHESTRATION](.planning/Runtime/orchestration.md): Crash-durable workflow and persistent-job owner over the command/event/schedule ports.
- [11]-[LANEGUARD](.planning/Runtime/laneguard.md): Keyed Polly pipeline per work lane, resizable permits, and the shed verdict `Rasm.Compute` reads.
- [12]-[FEATURES](.planning/Runtime/features.md): Variant targeting per subject — which variant, for whom, at what exposure — and the kill-switch.

[AGENT]:
- [13]-[MCP](.planning/Agent/mcp.md): MCP-server projection of descriptors to tools, resources, and prompts.
- [14]-[REASONING](.planning/Agent/reasoning.md): In-process agent loop with model-selection and content-filter governance.
- [15]-[FEDERATION](.planning/Agent/federation.md): MCP-client federation — external servers folded into one registry as brokered descriptors.
- [16]-[CAPABILITY](.planning/Agent/capability.md): Typed op catalog, pin-bound peer discovery, command algebra, and scoped grant metering.
- [17]-[IDENTITY](.planning/Agent/identity.md): Authentication boundary — OIDC issuer-trust, rotating token validation, claims-policy gate.
- [18]-[RUNTIME](.planning/Agent/runtime.md): One command-dispatch front door over the command algebra, tool adoption, and result.

[WIRE]:
- [19]-[OUTBOUND](.planning/Wire/outbound.md): Outbound boundary — every external dispatch admitted on `Fin` and returning its settled result.
- [20]-[LIVEWIRE](.planning/Wire/livewire.md): Reactive bidirectional external-binding studio over the industrial-transport axis.
- [21]-[COMPANION](.planning/Wire/companion.md): Multi-process modality, gRPC-over-UDS control, and authenticated CloudEvents webhook ingress.
- [22]-[TOPICS](.planning/Wire/topics.md): In-process event bus — every domain event fanned once and every declined offer accounted.
- [23]-[OUTBOX](.planning/Wire/outbox.md): Transactional outbox and dead-letter relay over the watermark dispatch sweep.
- [24]-[COORDINATION](.planning/Wire/coordination.md): Cluster membership, election, and distributed-lock over the fenced lease.

[SANDBOX]:
- [25]-[ADMISSION](.planning/Sandbox/admission.md): One supply-chain admission gate — offline Sigstore, SLSA provenance, SemVer contract.
- [26]-[ISOLATION](.planning/Sandbox/isolation.md): Capability-brokered WASM and process plugin isolation with unified call mediation.
- [27]-[SOLVER](.planning/Sandbox/solver.md): Solver-plugin contract with canonical-representation negotiation.
- [28]-[PROVISIONING](.planning/Sandbox/provisioning.md): Every update phase after the fetch — release-ring feeds, drain-before-swap, fleet waves.

[OBSERVABILITY]:
- [29]-[TELEMETRY](.planning/Observability/telemetry.md): Four-signal composition with per-signal governance.
- [30]-[HEALTH](.planning/Observability/health.md): Capability-health fold — backing probes onto one degradation ladder with its grading table.
- [31]-[BUNDLES](.planning/Observability/bundles.md): Bounded redacted support capture — trigger vocabulary, frozen window, one zip.
- [32]-[INSTRUMENTS](.planning/Observability/instruments.md): Domain-instrument catalog written directly by the producing operations.
- [33]-[HOOKS](.planning/Observability/hooks.md): Spine extension points a rider governs or watches, each advertising the guard its mechanism runs.
- [34]-[BENCHMARKS](.planning/Observability/benchmarks.md): Benchmark result, the corpus gate, and profile-linked capture rows.
- [35]-[EGRESS](.planning/Observability/egress.md): Durable OTLP transport — blob queue, disposition ledger, mutual-auth mount, persistent handler.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

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
- `LanguageExt.Sys` — Lifts BCL console, clock, file, and environment effects onto runtime-bound traits.
- `Scrutor`
- `System.CommandLine` — Projects the app-root verb table onto `CommandDispatch.Run`.
- `Cronos`
- `Microsoft.Extensions.ObjectPool`
- `System.Threading.Tasks.Dataflow` — Drainable-queue dataflow blocks; framework-provided, no manifest row.

[HEALTH_DIAGNOSTICS]:
- `Microsoft.Extensions.Diagnostics` — `AddMetrics` mints the per-ALC `IMeterFactory` behind the plugin capsule provider.
- `Microsoft.Extensions.Diagnostics.HealthChecks`
- `Microsoft.Extensions.Diagnostics.ResourceMonitoring` — Windows and Linux process and container utilization instruments behind the pressure grade.
- `Microsoft.Diagnostics.NETCore.Client` — Process-dump and EventPipe capture feeding the support-bundle fan.
- `Microsoft.Diagnostics.Runtime` — ClrMD `DataTarget.LoadDump` post-capture triage feeding the support archive.
- `Microsoft.Diagnostics.Tracing.TraceEvent` — Decodes the EventPipe event stream off the diagnostics session.
- `Npgsql` — Pooled `NpgsqlDataSource` the store health driver row binds; the probe shares production pool pressure, never a second connection.
- `Npgsql.OpenTelemetry` — `AddNpgsql` tracing and `AddNpgsqlInstrumentation` metrics registered once at the store-composing root.
- `AspNetCore.HealthChecks.NpgSql`
- `AspNetCore.HealthChecks.Redis`
- `AspNetCore.HealthChecks.System`
- `AspNetCore.HealthChecks.Uris`
- `AspNetCore.HealthChecks.Kafka`
- `AspNetCore.HealthChecks.Nats` — Probes the NATS broker anchor over the pooled connection.

[TELEMETRY_SDK]:
- `Microsoft.Extensions.Http.Diagnostics` — Outbound-request latency breakdown and redaction-aware extended client logging.
- `Microsoft.Extensions.Telemetry` — Composition-root log sampling, buffering, enrichment, redaction activation, and the latency ledger.
- `OpenTelemetry`
- `OpenTelemetry.Extensions` — `BaggageActivityProcessor` cost-attribution baggage promotion; `RateLimitingSampler` per-second head cap.
- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.AspNetCore` — Service-root inbound request spans and the built-in hosting meters.
- `OpenTelemetry.Instrumentation.GrpcNetClient` — Client-span semconv over the `Grpc.Net.Client` hops.
- `OpenTelemetry.Instrumentation.Http`
- `OpenTelemetry.Instrumentation.Process` — Absolute process memory, `cpu.mode`-split CPU seconds, thread count, and uptime series.
- `OpenTelemetry.Instrumentation.Runtime`
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` — Projects the OTLP exporter at the composition root.
- `OpenTelemetry.PersistentStorage.FileSystem` — `FileBlobProvider` offline queue replaying failed OTLP batches on reconnect.
- `OpenTelemetry.Resources.Container` — `container.id` resource detector at the identity boundary.
- `OpenTelemetry.Resources.Host` — `host.name`/`host.id`/`host.arch` resource detector at the identity boundary.
- `OpenTelemetry.Resources.OperatingSystem` — `os.*` resource detector at the identity boundary.
- `OpenTelemetry.Resources.Process` — `process.owner`/`process.pid`/`process.creation.time` resource detector.
- `OpenTelemetry.Resources.ProcessRuntime` — `process.runtime.*` resource detector at the identity boundary.
- `Pyroscope` — Native continuous-profiler agent; `Profiler.Instance` singleton the span processor writes profile ids into.
- `Pyroscope.OpenTelemetry`

[LOG_SINKS]:
- `Serilog`
- `Serilog.Extensions.Hosting`
- `Serilog.Sinks.Console` — Composition-root sink projection.
- `Serilog.Sinks.File` — Composition-root sink projection.

[OUTBOUND_TRANSPORT]:
- `CloudNative.CloudEvents.AspNetCore` — HTTP codec extensions; `Wire/companion#EVENT_INGRESS` owns consent and authenticated delivery.
- `Microsoft.Extensions.Http.Resilience`
- `Polly.Core`
- `Polly.Extensions`
- `Polly.RateLimiting`
- `Microsoft.Extensions.ServiceDiscovery`
- `Grpc.AspNetCore.HealthChecks`
- `Grpc.HealthCheck` — Transitive `grpc.health.v1` serving-status surface under `Grpc.AspNetCore.HealthChecks`, never a direct reference.
- `Grpc.AspNetCore.Web` — Cross-origin grpc-web middleware row at the app root.
- `Microsoft.AspNetCore.App` — Shared framework arriving transitively with the gRPC host; serves `CoHostedAssets` through `UseStaticFiles`.

[CAPABILITY_AGENT]:
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
- `Microsoft.IdentityModel.Protocols` — `ConfigurationManager<T>` refresh interface; transitive under the OpenIdConnect sibling, centrally pinned.
- `Sigstore`
- `NuGet.Versioning`
- `Velopack`
- `OpenFeature`

[SANDBOX_LIVE_WIRE]:
- `Wasmtime`
- `OPCFoundation.NetStandard.Opc.Ua`
- `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- `FluentModbus`
- `BACnet` — Backs the building-automation `bacnet` `ExternalTransport` row feeding twin calibration.
- `System.IO.Ports`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/dotnet/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `Generator.Equals` — `HostFingerprint` structural equality over its stamp map.
- `LanguageExt.Core`
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `QuikGraph` — Adjacency, reachability, and topological order under the determinism recompute graph.
- `Riok.Mapperly` — Generator-only (`PrivateAssets`); one `[Mapper]` per wire boundary owns every domain↔wire correspondence.
- `System.IO.Hashing`
- `System.Numerics.Tensors`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`

[SERVICE_CONTRACTS]:
- `Microsoft.Extensions.AI.Abstractions`
- `Microsoft.Extensions.AI` — Concrete builder and middleware owner behind the injected contracts.
- `Microsoft.Extensions.Caching.Hybrid` — Stampede-guarded recency substrate the host composes once at the root.

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Telemetry.Abstractions`
- `Microsoft.Extensions.Compliance.Redaction`

[WIRE_CODEGEN]:
- `Celly.Protovalidate` — Compiles generated constraints at bootstrap and admits every parsed or gRPC message once.
- `Google.Protobuf` — Runtime under the generated contract and event-extension messages.
- `Grpc.AspNetCore.Server`
- `Grpc.Core.Api` — Control-channel binding seat.
- `Grpc.Net.Client`
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- `NodaTime.Serialization.Protobuf` — Carries `Duration` and `Instant` across the control-verb wire.

[EVENT_TRANSPORT]:
- `CloudNative.CloudEvents` — Message-envelope algebra the HTTP ingress admits and every outbox hop projects.

[MACHINE_CONNECTIVITY]:
- `MTConnect.NET-Common` — Connectivity partition behind the machine-tool `mtconnect` `ExternalTransport` row.
- `MQTTnet` — Subscribe-shaped `mqtt` `ExternalTransport` row, its held client, and the consumer-kinded receive bracket.

[RUNTIME_INBOX]:
- `System.Net.Http` — Handler chain behind every outbound hop, resilience pipeline, and durable OTLP transport.
- `System.Text.Json` — Host-local outcomes and discovery manifest freeze into one merged source-generated options identity.
