# [APPHOST_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[LIVE_HOST_PROBES]

Tier-3 probes that strictly require the running integrated host (RhinoWIP plugin ALC, the live service manager, or a paired companion process); each is named in its page RESEARCH cluster.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Generic Host boot and unload inside the RhinoWIP plugin load context without process exit | host-profiles#LIFETIME_ADAPTERS | SPIKE |
| [2] | Kestrel + Grpc.AspNetCore hosting and gRPC-over-UDS control intake under the plugin ALC shared framework; socket-file mode + accept-side `PeerAdmission` credential read gate the connecting peer | companion-sidecar#SERVICE_HOST · outbound-resilience#DISCOVERY_ATTACH | SPIKE |
| [3] | Cross-process degradation-cascade convergence: a companion floors its `DegradationCell.Cascade` from the parent level over the control hop and re-derives on release | companion-sidecar#DEGRADATION_CASCADE | SPIKE |
| [4] | Drain-deadline conformance under live plugin unload within the cooperative allotment | time-and-deadlines#DEADLINE_TAXONOMY | SPIKE |
| [5] | SIGHUP delivery under launchd and systemd; standalone crash-marker schema; macOS keychain secrets-store route | lifecycle-and-drain#FAULT_SPINE · configuration-and-options#SOURCE_AXIS | SPIKE |
| [6] | macOS IOKit power-source and SMC thermal-pressure live reads feeding the fidelity grade | host-profiles#POWER_AND_FIDELITY | SPIKE |
| [7] | WASM component-model instantiation with a scope-derived import table and fuel/memory counters | sandbox-host#ISOLATION_AXIS | SPIKE |
| [8] | Artifact signature verification and SLSA in-toto attestation parse against the pinned publisher key | sandbox-host#SUPPLY_CHAIN | SPIKE |
| [9] | Solver-plugin representation negotiation: lossless canonical `EncodedTensor` round-trip | solver-plugin#SOLVER_HOSTING | SPIKE |
| [10] | Industrial-transport client reads/writes per OPC-UA/Modbus/MQTT/serial; bidirectional feedback-loop guard | live-wire#TRANSPORT_AXIS · live-wire#BINDING_SPEC | SPIKE |
| [11] | Cross-platform floating-point determinism: bit-identical reproduction across RIDs | determinism-and-replay#DETERMINISM_KERNEL | SPIKE |
| [12] | MCP streaming progress over the server-stream substrate; resumable handle reattach after a transport bounce | mcp-projection#STREAM_PROGRESS | SPIKE |

## [2]-[SPEC_AND_TOOL_GATES]

Implementation-time gates discharged against scratch processes, the spec project, or a manifest decision — no live host required.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Generated `ControlService` base/client and request/reply members plus the `Grpc.Core.Api` `Metadata` carrier `TraceContext` reads compile through the G7 spec-compile gate until the `Grpc.Core.Api` assay source map registers the transitive package | companion-sidecar#CONTROL_SERVICE · diagnostics-and-telemetry#CORRELATION_SPINE | SPIKE |
| [2] | NodaTime converter precedence over combined source-gen metadata in the Strict merge | runtime-ports#WIRE_LAW | SPIKE |
| [3] | Velopack `VelopackHook` delegate-signature probe at the app-root `VelopackApp.Build()...Run()` bootstrap | provisioning-and-update#UPDATE_RAIL | SPIKE |
| [4] | `ServiceMappingCollection.Map` versus `MapService` behavioral distinction on `GrpcHealthChecksOptions.Services` | companion-sidecar#CONTROL_SERVICE | SPIKE |
| [5] | dotnet-counters/trace/gcdump tool-manifest admission; re-verify the `.config/dotnet-tools.json` servicing line | support-bundles#CAPTURE_PIPELINE | QUEUED |
| [6] | Production feed URIs per channel replace the placeholder authority on the `UpdateChannel` rows | provisioning-and-update#CHANNEL_AXIS | BLOCKED |
| [7] | MCP `IServerStreamWriter<ProgressFrame>` server-stream fan and JSON-RPC error-code mapping for the 4640-band `McpFault` cases | mcp-projection#TOOL_DISPATCH · mcp-projection#STREAM_PROGRESS | SPIKE |
| [8] | SDK codegen cross-language shape identity: C#/TS/Python emitted methods bind one `JsonSchemaExporter` schema per descriptor | capability-registry#SDK_CODEGEN | SPIKE |
| [9] | `ComputeIntent.Spec` field arity and `SelectionReceipt.None` sentinel the command algebra constructs for a brokered command | capability-registry#COMMAND_ALGEBRA | SPIKE |

## [3]-[TRANSCRIPTION]

The implementation sequence is the `ARCHITECTURE.md` `[SOURCE_TREE]` build order (vocabulary owners before consumers, `Time.cs` through `LiveWire.cs`); each file transcribes its page clusters verbatim and resolves the RESEARCH rows those pages carry. Production source is absent.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the BUILD_ORDER files per `ARCHITECTURE.md` `[SOURCE_TREE]`; the test project `Rasm.AppHost.Tests` node is present and empty | host-profiles#PROFILE_AXIS | QUEUED |
