# [APPHOST_TASKLOG]

Open work owned by this folder; closed items do not appear.

## [1]-[NATIVE_AND_SERVER_PROBES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | SIGHUP delivery under launchd/systemd; standalone crash-marker schema; macOS keychain secrets-store route | all three probes recorded; delivery and schema confirmed |
| [2] | UDS peer-credential read (LOCAL_PEERCRED) admission probe | peer-credential read confirmed on target platform |

## [2]-[SPEC_PROOFS_AT_IMPLEMENTATION]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | NodaTime converter precedence over combined source-gen metadata in the Strict merge | precedence order confirmed by spec; merge round-trip passes |

## [3]-[ADMISSION_DECISIONS_PENDING]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | dotnet-counters/trace/gcdump tool-manifest admission; re-verify the `.config/dotnet-tools.json` servicing line | tool-manifest row verified at current servicing line; pin recorded |
| [2] | Microsoft.Extensions.Compliance.Redaction registration surface (AddRedaction/SetHmacRedactor/SetRedactor/SetFallbackRedactor) | EXTEXP0002 gate resolved; registration surface admitted or deferred with recorded verdict |
| [3] | Velopack `VelopackHook` delegate-signature probe at the app-root `VelopackApp.Build()...Run()` bootstrap | `typeof(VelopackHook)` parameter shape confirmed; `OnFirstRun`/`OnRestarted`/`OnAfter*FastCallback` registrations transcribe |
| [4] | `ServiceMappingCollection.Map` versus `MapService` behavioral distinction on `GrpcHealthChecksOptions.Services` | by-service-name versus predicate-routing distinction confirmed; wire-health registration row settles |

## [4]-[PROVISIONING_AND_CONTROL_SPIKES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | gRPC-over-UDS plus hardened Windows named-pipe control intake under the plugin ALC shared framework | ControlService served over UDS from the host; named-pipe intake hardened to the current-user SID |
| [2] | cross-process degradation-cascade convergence (companion-sidecar#DEGRADATION_CASCADE `[CASCADE_CONVERGENCE]`) | companion observes parent level over the control hop, floors its `DegradationCell.Cascade`, re-derives on release under the live integrated host |

## [5]-[PLANNING_CLOSE_OUT_SPIKES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Generic Host boot + unload inside the RhinoWIP plugin ALC without process exit | boot and unload cycle verified without process exit |
| [2] | Kestrel + Grpc.AspNetCore hosting inside the plugin ALC (gRPC loopback from the Rhino host) | gRPC loopback call completes; no ALC isolation failure |
| [3] | Drain-deadline conformance scenario under live plugin unload | drain deadline honoured; unload completes within deadline |
