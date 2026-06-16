# [APPHOST_TASKLOG]

Open work owned by this folder; closed items do not appear. Each row names its owner page#cluster and the closure environment that discharges it. Residual `[STATE]=SPIKE` owners (host-profiles#LIFETIME_ADAPTERS `ProfileBoot`, outbound-resilience#DISCOVERY_ATTACH `DiscoveryManifest`, companion-sidecar#DEGRADATION_CASCADE `DegradationCascade`) carry their probe here and on the ROADMAP START_GATES; every other owner is FINALIZED.

## [1]-[LIVE_HOST_PROBES]

Tier-3 probes that strictly require the running integrated host (RhinoWIP plugin ALC, the live service manager, or a paired companion process); each is named in its page RESEARCH cluster.

| [INDEX] | [ITEM] | [OWNER (page#cluster)] | [CLOSURE_ENV] | [EXIT] |
| :-----: | ------ | ---------------------- | ------------- | ------ |
| [1] | Generic Host boot and unload inside the RhinoWIP plugin load context without process exit | host-profiles#LIFETIME_ADAPTERS [PLUGIN_HOST] (`ProfileBoot` SPIKE) | live RhinoWIP plugin ALC; `bridge verify --pattern host_boot_drain` | load-context teardown sequence under live host eviction confirmed; boot/unload cycle completes without process exit |
| [2] | Kestrel + Grpc.AspNetCore hosting and gRPC-over-UDS plus hardened Windows named-pipe control intake under the plugin ALC shared framework | companion-sidecar#SERVICE_HOST [KESTREL_ENDPOINT] + outbound-resilience#DISCOVERY_ATTACH (`DiscoveryManifest` SPIKE) | live RhinoWIP app root serving grpc.health over UDS | gRPC loopback completes with no ALC isolation failure; ControlService served over UDS; named-pipe intake hardened to the current-user SID |
| [3] | Cross-process degradation-cascade convergence: a companion observes the parent level over the control hop, floors its `DegradationCell.Cascade`, and re-derives on release | companion-sidecar#DEGRADATION_CASCADE [CASCADE_CONVERGENCE] (`DegradationCascade` SPIKE) | paired and companion topologies inside the running integrated host | the inbound route that lands a parent-peer floor through `Cascade` rather than operator `Force` confirmed; convergence and release re-derive observed |
| [4] | Drain-deadline conformance under live plugin unload | time-and-deadlines#DEADLINE_TAXONOMY | live RhinoWIP unload; `scenarios/drain-deadlines.verify.csx` | drain deadline honoured; unload completes within the cooperative allotment |
| [5] | SIGHUP delivery under launchd and systemd; standalone crash-marker schema; macOS keychain secrets-store route | lifecycle-and-drain#FAULT_SPINE + configuration-and-options#SOURCE_AXIS | scratch headless host under the live service manager; `kill -HUP` and keychain P/Invoke spike | SIGHUP delivers one reload zero drains; crash-marker schema confirmed; keychain provider route recorded |

## [2]-[SPEC_AND_TOOL_GATES]

Implementation-time gates discharged against scratch processes, the spec project, or a manifest decision — no live host required.

| [INDEX] | [ITEM] | [OWNER (page#cluster)] | [CLOSURE_ENV] | [EXIT] |
| :-----: | ------ | ---------------------- | ------------- | ------ |
| [1] | Generated `ControlService` base/client and request/reply members plus the `Grpc.Core.Api` `Metadata` carrier `TraceContext` reads compile through the G7 spec-compile gate until the `Grpc.Core.Api` assay source map registers the transitive package | companion-sidecar#CONTROL_SERVICE [SPEC_COMPILE] + diagnostics-and-telemetry#CORRELATION_SPINE [GRPC_METADATA_CARRIER] | G7 spec-compile (`Grpc.Core.Api` members) | generated members and `Metadata.Add`/`GetAll`/`Entry.Value` compile; assay source-map coverage lands the rail |
| [2] | NodaTime converter precedence over combined source-gen metadata in the Strict merge | runtime-ports#WIRE_LAW | G4 spec round-trip | precedence order confirmed by spec; merge round-trip passes |
| [3] | Velopack `VelopackHook` delegate-signature probe at the app-root `VelopackApp.Build()...Run()` bootstrap | provisioning-and-update#UPDATE_RAIL | `dotnet build` probe reflecting `typeof(VelopackHook)` at the app-root bootstrap | parameter shape confirmed; `OnFirstRun`/`OnRestarted`/`OnAfter*FastCallback` registrations transcribe |
| [4] | `ServiceMappingCollection.Map` versus `MapService` behavioral distinction on `GrpcHealthChecksOptions.Services` | companion-sidecar#CONTROL_SERVICE [MAP_SERVICE] | `dotnet build` probe inspecting `GrpcHealthChecksOptions.Services` member behavior | by-service-name versus predicate-routing distinction confirmed; wire-health registration row settles |
| [5] | dotnet-counters/trace/gcdump tool-manifest admission; re-verify the `.config/dotnet-tools.json` servicing line | support-bundles#CAPTURE_PIPELINE | `dotnet package search`; `.config/dotnet-tools.json` row | tool-manifest row verified at current servicing line; pin recorded |
| [6] | Production feed URIs per channel replace the placeholder authority on the `UpdateChannel` rows | provisioning-and-update#CHANNEL_AXIS [STAGED_FEED] | release-feed host provisioning at deployment | per-channel `Feed` URIs land once the release-feed host is provisioned |
