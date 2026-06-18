# [APPHOST_TASKLOG]

The open and closed work for the runtime spine, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

## [1]-[OPEN]

[T-SECRET-LIFECYCLE] [QUEUED]: Author the `secrets/` sub-domain owning the credential lifecycle.
- Capability: a `SecretLease` row family carrying the credential handle, its lease window, and its rotation receipt; an acquire-renew-zeroize fold that pulls a short-lived credential, schedules renewal ahead of expiry, and zeroizes the in-memory copy on drain. Lands as a new `.planning/secrets/secret-lifecycle.md` page, transcribes to `secrets/Secrets.cs`.
- Packages: Microsoft.Extensions.Configuration.UserSecrets (existing); the concrete workload-identity/credential-broker package admits here once the acquisition route settles from the secrets-store research item.
- Integration: the lease renewal registers one `ScheduleEntry` on `time#SCHEDULE_PORT` at a credential-rotation `DeadlineClass` row, internal to the folder; the credential value carries `DataClassification.Secret` so observability#REDACTION_TAXONOMY erases it at every egress; the keychain acquisition reuses the `configuration#SOURCE_AXIS` `SecretsSource` boundary, never a parallel provider; the zeroization is a `hosting#DRAIN_CONDUCTOR` Stores-band participant row.
- Considerations: a lease must renew strictly before expiry or the fold degrades through observability#DEGRADATION_RAIL, never a hard fault; the zeroization runs under the drain forced token so a hung renewal never strands a live secret; the rotation receipt carries no secret bytes, only the lease window and a redacted credential id.

[T-FENCING-COORDINATION] [QUEUED]: Author the `coordination/` sub-domain owning fenced single-writer election.
- Capability: a `FencingToken` `[ValueObject<UInt128>]` monotone token and a `LeaseElection` rail that acquires a fenced lease, mints a strictly-increasing token per acquisition, and rejects a write carrying a stale token. Lands as a new `.planning/coordination/lease-election.md` page, transcribes to `coordination/Coordination.cs`.
- Packages: System.IO.Hashing (existing); no new central admission — the token composes from the op-log HLC cursor and the roster epoch.
- Integration: the token derives from the `ports#PORT_RECORDS` HLC stamp and the `companion#PROCESS_MODALITY` `PeerRoster` epoch, internal to the folder; the maintenance lease at `time#SCHEDULE_PORT`, the `provisioning#ROLLOVER_DRAIN` `FleetRoll` conductor election, and the sidecar `companion#PROCESS_MODALITY` write-forward each acquire through this rail; the store fences the token at Persistence/server-tier, aligned to a sibling branch, never coupled.
- Considerations: the token must be checked by the resource, not merely held — a timeout-only lease is unsafe under a paused holder; a resumed stale holder presents a lower token and the fenced write rejects; the election reuses the existing `CrashStaleness` window as the lease timeout but adds the token as the correctness proof the timeout alone cannot give.

## [2]-[CLOSED]

[T-MCP-SDK] [COMPLETE]: The MCP serving surface design landed in `.planning/agent/mcp-projection.md` — the registry projects through the official SDK (`ToolProjection`, the `CommandAIFunction : AIFunction` schema-overriding subclass `McpServerTool.Create` adopts, `McpDispatch` brokered dispatch, `StreamProgress` over the SDK's SSE-resumable progress and task primitives), and the `[6]-[RESEARCH]` cards carry the SDK member spellings, the 4640-band ⟶ `-32xxx` fault map, and the build-order prerequisite (the capability page and the `Rasm.Compute` intent-and-selection contract settle before transcription).

[T-PROFILE-SIGNAL] [COMPLETE]: The fourth-signal design landed in `.planning/observability/diagnostics-and-telemetry.md` — `TelemetrySignal` carries the four-row trace/metric/log/profile set with the `Profile` row, `SpanProfileProcessor` links CPU profiles to the minted span through the `AddProcessor` seat gated to service app roots, and the `gen_ai.*` attributes plus `gen_ai.usage.input_tokens`/`output_tokens` instruments ride the trace and metric signals through the minted meter.

[T-GRANT-ATTESTATION] [COMPLETE]: The signed-grant design landed across `.planning/capability/registry.md` (the `[GRANT_ATTESTATION]` research card: the detached-signature primitive over the canonical `GrantAttestation` bytes, the `XxHash128` digest seed shared with the determinism kernel) and `.planning/determinism/determinism-and-replay.md` #EVENT_LOG (the hash-chain seat the attestation rides), closing the `T-CEDAR-BROKER` auditable-decision re-target.

[T-INDUSTRIAL-TRANSPORT] [COMPLETE]: The certified-transport design landed in `.planning/live-wire/live-wire.md` — the `[TRANSPORT_CLIENTS]` research card composes `OPCFoundation.NetStandard.Opc.Ua` (session/subscription/monitored-item/`WriteValue`) with `.PubSub` for PubSub-over-MQTT and `MQTTnet` for the MQTT leg, all eight transports riding the one `TransportRow` adapter contract over their `OutboundHop` resilience rows.

[T-WASM-RUNTIME] [COMPLETE]: The WASM-runtime design landed in `.planning/sandbox/sandbox-host.md` — the wasm-component isolation row hosts on the `wasmtime-dotnet` embedding with the WASI-Preview-2 import table scoped to the granted `CapabilityDescriptor` set, the `Store` fuel and linear-memory counters seat the quota cell, and the `[WASM_RUNTIME]` research card carries the `componentize-dotnet` WIT-binding distinction the live runtime resolves.

[T-CEDAR-BROKER] [DROPPED]: Route the grant-broker through Cedar — dropped with its parent idea. No Cedar .NET binding exists (Cedar ships Rust/WASM/Java/Go/Ruby; the C# binding is an open feature request), so the central manifest cannot admit it; the broker keeps its typed `PermissionShape` × `GrantScope.Covers` value-object predicate, and the auditable-decision goal re-targets to `T-GRANT-ATTESTATION`.

[T-DOC-MIGRATE] [COMPLETE]: Re-homed the 19 design pages into the single `.planning/<sub-domain>/<page>.md` tree, rebuilt `ARCHITECTURE.md` as a codemap, `README.md` as router-plus-package-registry, and authored `IDEAS.md`/`TASKLOG.md`. The per-folder `.api/` stays at the package root.
