# [APPHOST_IDEAS]

The forward pool of higher-order concepts for the runtime spine, each grounded in the folder's domain and current platform capability — some are new sub-domain folders that deepen a thin owner, others bind a concrete package to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

## [1]-[OPEN]

[SECRET_LIFECYCLE]: Promote secret handling from a single `ConfigSource` row to a `secrets/` sub-domain owning the credential lifecycle.
- The `SecretsStore` source mounts a keychain provider once at boot, but a runtime spine owns the whole secret lifecycle — short-lived-credential acquisition, lease renewal ahead of expiry on the schedule port, in-memory zeroization on drain, and a rotation receipt — none of which a frozen config row carries; the new sub-domain folds a `SecretLease` row family over the existing `ScheduleEntry`/`LeasePolicy` and `DataClassification.Secret` redaction owners.
- Unlocks no-static-secret operation: a credential is acquired from a workload-identity broker, held only as long as its lease, renewed on a deadline-class cadence, and zeroized at drain, so a leaked process memory image carries no long-lived secret and a rotated upstream credential propagates without a restart.
- Draws on the 2025-2026 workload-identity-federation and short-lived-credential shift (OIDC/SPIFFE workload identity over static API keys) — the runtime that already owns config admission, the schedule port, and the redaction taxonomy is the natural owner of the lease cadence and the zeroization seam that a static config row structurally cannot express.

[FENCING_COORDINATION]: Add a `coordination/` sub-domain owning distributed single-writer election with monotone fencing tokens.
- The maintenance `LeasePolicy`, the `PeerRoster` epoch, and the `FleetRoll` wave each imply a single-writer guarantee but no owner mints the monotone fencing token that makes a stale lease holder's late write rejectable; the new sub-domain folds a `FencingToken` value object and a `LeaseElection` rail over the existing op-log HLC cursor and the `PeerRoster` epoch so every guarded write carries a token the store fences on.
- Unlocks correct cross-process single-writer coordination — maintenance work, fleet-roll conductor election, and the sidecar write-forward owner each acquire a fenced lease whose monotone token a paused-then-resumed holder cannot reuse, closing the classic lease-expiry-during-GC-pause write-corruption window the bare `CrashStaleness` timeout leaves open.
- Draws on the fencing-token correctness law for distributed locks (a timeout-only lease is unsafe under a paused holder; a monotone token the resource checks is the proof) — the runtime that owns the HLC stamp, the lease policy, and the roster epoch already holds the three inputs a fencing token composes from.

## [2]-[CLOSED]

[OFFICIAL_MCP_SDK_COLLAPSE]: [COMPLETE] The agent-facing serving surface is the capability registry projected onto the official `ModelContextProtocol`/`ModelContextProtocol.Core`/`ModelContextProtocol.AspNetCore` SDK — `.planning/agent/mcp-projection.md` holds the descriptor-to-`AIFunction` tool projection, the `CommandAIFunction : AIFunction` schema-overriding adoption seam, the brokered `McpDispatch`, and the `StreamProgress` fan over the SDK's SSE-resumable transport and task primitives, with the SDK owning JSON-RPC framing, the handshake, and the error-code map.

[FOURTH_SIGNAL_PROFILING]: [COMPLETE] `.planning/observability/diagnostics-and-telemetry.md` carries the four-row `TelemetrySignal` set with the `Profile` continuous-profiling row, the `SpanProfileProcessor` `BaseProcessor<Activity>` linking CPU profiles to the minted span through the `AddProcessor` seat, and the GenAI semantic conventions (`gen_ai.*` attributes, `gen_ai.usage.input_tokens`/`output_tokens` instruments) riding the trace and metric signals.

[SIGNED_GRANT_ATTESTATION]: [COMPLETE] The verifiable signed-grant admission is designed across `.planning/capability/registry.md` (the `GrantAttestation` mint over canonical bytes with the `XxHash128` digest seed) and `.planning/determinism/determinism-and-replay.md` #EVENT_LOG (the hash-chain seat that makes the grant decision a content-addressed replayable fact), closing the in-process-only gap a transient `Fin<CostVector>` left open.

[CERTIFIED_INDUSTRIAL_TRANSPORT]: [COMPLETE] `.planning/live-wire/live-wire.md` names `OPCFoundation.NetStandard.Opc.Ua` (with `.PubSub` for PubSub-over-MQTT) and `MQTTnet` as the concrete transport owners behind the one `TransportRow` adapter contract, replacing the abstract protocol-client pin.

[WASM_RUNTIME_OWNER]: [COMPLETE] `.planning/sandbox/sandbox-host.md` names `wasmtime-dotnet` as the isolation-axis host runtime with the WASI-Preview-2 component-model import table scoped to the granted descriptor set, the `Store` fuel/memory counters seating the quota cell, and the `componentize-dotnet` WIT-binding generation pinned at the app root.

[CEDAR_PERMISSION_GRAMMAR]: [DROPPED] Anchored on a Cedar .NET binding that does not exist — Cedar ships Rust/WASM/Java/Go/Ruby only, and a C# binding is an open feature request, not an admissible central package; the typed `PermissionShape` × `GrantScope.Covers` value-object predicate the broker already carries is correct functional ROP, not a hand-rolled defect to replace. The auditable-decision goal it chased is re-targeted at a real technique under `SIGNED_GRANT_ATTESTATION`.
