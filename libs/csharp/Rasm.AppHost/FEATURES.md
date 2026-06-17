# [APPHOST_FEATURES]

The realized capability list for the runtime spine. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[ISOLATION_SPINE]

The host-variance, lifecycle, time, configuration, and composition axes that boot, degrade, drain, and report every modality with zero app-side ceremony.

| [INDEX] | [FEATURE]                                                                      | [PAGE#CLUSTER]                              |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------------------ |
|   [1]   | Modality boot from one variance axis                                           | host-profiles#PROFILE_AXIS                  |
|   [2]   | Per-user roots and telemetry resource identity from the resolved record        | host-profiles#RESOURCE_IDENTITY             |
|   [3]   | sd_notify service-state lifecycle mirror with watchdog keep-alive              | host-profiles#LIFETIME_ADAPTERS             |
|   [4]   | Total phase machine with CAS-committed receipts and LIFO subscriptions         | lifecycle-and-drain#PHASE_FAMILY            |
|   [5]   | Crash-recovery and upgrade boot probe over own and host crash markers          | lifecycle-and-drain#FAULT_SPINE             |
|   [6]   | Unhandled, unobserved, and POSIX fault traps with SIGHUP reload, SIGTERM drain | lifecycle-and-drain#FAULT_SPINE             |
|   [7]   | Rank-band cooperative-to-forced drain with per-step straggler receipts         | lifecycle-and-drain#DRAIN_CONDUCTOR         |
|   [8]   | One cancellation root; every token derives with provenance and deadline        | lifecycle-and-drain#CANCEL_SPINE            |
|   [9]   | One injected clock pair: elapsed versus semantic time, sentinel admission      | time-and-deadlines#CLOCK_SPLIT              |
|  [10]   | Deadline taxonomy; every suite duration literal traces to a row                | time-and-deadlines#DEADLINE_TAXONOMY        |
|  [11]   | One scheduler: cron and period rows, lease split, missed-occurrence sweep      | time-and-deadlines#SCHEDULE_PORT            |
|  [12]   | Watchdog heartbeat as a schedule row folding a not-met receipt to a trigger    | time-and-deadlines#SCHEDULE_PORT            |
|  [13]   | Ranked config sources mounted onto one chain with rank-fold precedence         | configuration-and-options#SOURCE_AXIS       |
|  [14]   | Fail-closed source-gen binding into validated immutable policy records         | configuration-and-options#TYPED_BINDING     |
|  [15]   | Validate-once frozen publish with reload-class-gated receipted transitions     | configuration-and-options#POLICY_VALUES     |
|  [16]   | Operator kill-switch as one transition-class row forcing the degradation fold  | configuration-and-options#KILL_SWITCH       |
|  [17]   | One composition root: frozen module table folds and freezes the service graph  | composition-and-modules#MODULE_TABLE        |
|  [18]   | Keyed decoration of contributor ports, profile-conditional via `TryDecorate`   | composition-and-modules#BOUNDARY_ACTIVATION |
|  [19]   | Admission-edge activation: availability probe, async drain scope, validators   | composition-and-modules#BOUNDARY_ACTIVATION |

## [2]-[RESOURCE_AND_OUTBOUND]

Bounded runtime resource lanes and the single outbound boundary — the cache, pool, queue, and hop axes this folder owns so no downstream app hand-rolls them per call site.

| [INDEX] | [FEATURE]                                                                      | [PAGE#CLUSTER]                       |
| :-----: | :----------------------------------------------------------------------------- | :----------------------------------- |
|  [20]   | Lane-keyed hybrid cache: stampede single-flight, tag cut, per-lane keyed L2    | resource-lanes#CACHE_PORT            |
|  [21]   | Delegate-row object pools: reset law, sanity predicate, leak-tracked test row  | resource-lanes#OBJECT_POOLS          |
|  [22]   | Bounded drainable queues with receipted loss; pipe versus network split        | resource-lanes#DRAIN_QUEUES          |
|  [23]   | Outbound hop cases bound to frozen policy rows with total dispatch             | outbound-resilience#HOP_AXIS         |
|  [24]   | Standard and hedging HTTP pipelines; weighted multi-region routing as a subrow | outbound-resilience#HTTP_PIPELINES   |
|  [25]   | One keyed Polly registry per non-HTTP hop; breaker read/write split; chaos     | outbound-resilience#KEYED_PIPELINES  |
|  [26]   | Companion discovery manifest, UDS attach, checksum gate, child lifecycle       | outbound-resilience#DISCOVERY_ATTACH |
|  [27]   | One retry owner per hop: claim cell, conflict receipt, kill-switch enforce     | outbound-resilience#OWNERSHIP_LAW    |

## [3]-[OBSERVABILITY_AND_HARDENING]

The unified observability and hardening posture — traces, metrics, and logs through minted identities, the HLC causal primitive, redaction at every egress, and bounded redacted support capture.

| [INDEX] | [FEATURE]                                                                      | [PAGE#CLUSTER]                               |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------------------- |
|  [28]   | Minted source and meter identity with a source-gen instrument registry         | diagnostics-and-telemetry#TELEMETRY_IDENTITY |
|  [29]   | One boot-minted correlation id stamped across every signal and hop             | diagnostics-and-telemetry#CORRELATION_SPINE  |
|  [30]   | Per-profile log pipeline arbitration: serilog-projection versus otel-export    | diagnostics-and-telemetry#LOG_PROJECTION     |
|  [31]   | Source-gen lib-level log delegates with sink-loss listener fold                | diagnostics-and-telemetry#LOG_PROJECTION     |
|  [32]   | Per-signal governance: sampling ratio, buffering, exemplars, metric views      | diagnostics-and-telemetry#SIGNAL_GOVERNANCE  |
|  [33]   | Checkpoint latency-context recorder drained at the band                        | diagnostics-and-telemetry#SIGNAL_GOVERNANCE  |
|  [34]   | OTLP export backend-parameterized by env across Prometheus, Tempo, Loki, Alloy | diagnostics-and-telemetry#SIGNAL_GOVERNANCE  |
|  [35]   | Classification taxonomy; redaction bound at every exporter seam                | diagnostics-and-telemetry#REDACTION_TAXONOMY |
|  [36]   | Resource-pressure health fold with container-limit cgroup-v2 grading           | health-and-degradation#HEALTH_FOLD           |
|  [37]   | Usable-failure degradation rail with hysteresis                                | health-and-degradation#DEGRADATION_RAIL      |
|  [38]   | Wire health: tag-predicate registry to `grpc.health.v1`; set-degradation route | health-and-degradation#WIRE_HEALTH           |
|  [39]   | Support trigger union auto-arming on fault, health, watchdog, schedule         | support-bundles#TRIGGER_UNION                |
|  [40]   | Bounded redacted capture: window freeze, ordered fan-in, caps, coalesce        | support-bundles#CAPTURE_PIPELINE             |
|  [41]   | Retention sweep: max-bundle and max-age eviction as a schedule row             | support-bundles#MANIFEST_RECEIPT             |
|  [42]   | Process-dump artifact row over the dump tool for support capture               | support-bundles#CAPTURE_PIPELINE             |

## [4]-[CROSS_PACKAGE_PORTS]

The seven inward port records — the only cross-package seam — plus the suite wire law and TS map.

| [INDEX] | [FEATURE]                                                                     | [PAGE#CLUSTER]              |
| :-----: | :---------------------------------------------------------------------------- | :-------------------------- |
|  [43]   | HLC-stamped receipt envelope as the sole cross-process causal primitive       | runtime-ports#PORT_RECORDS  |
|  [44]   | Typed inward ports; one beyond the budgeted set is the named defect           | runtime-ports#PORT_RECORDS  |
|  [45]   | One Strict JSON wire law per package; app roots merge resolvers, emit schemas | runtime-ports#WIRE_LAW      |
|  [46]   | TS dashboard contract: one envelope, schema-derived types, codec residence    | runtime-ports#TS_PROJECTION |

## [5]-[PROVISIONING_AND_CONTROL]

The post-fetch update rail and the inbound control-service host — the self-updating, operationally-controllable, multi-process legs the spine owns.

| [INDEX] | [FEATURE]                                                                        | [PAGE#CLUSTER]                              |
| :-----: | :------------------------------------------------------------------------------- | :------------------------------------------ |
|  [47]   | Post-fetch update state machine: download, stage, rollover, rollback receipts    | provisioning-and-update#UPDATE_RAIL         |
|  [48]   | Three-channel feed axis with explicit-channel and downgrade-policy columns       | provisioning-and-update#CHANNEL_AXIS        |
|  [49]   | Drain-before-swap rollover folding the conductor ahead of the restart            | provisioning-and-update#ROLLOVER_DRAIN      |
|  [50]   | Process-modality axis: companion, sidecar, paired-peer spawn-attach-discovery    | companion-sidecar#PROCESS_MODALITY          |
|  [51]   | Inbound control-service host folding three verbs onto existing owners            | companion-sidecar#CONTROL_SERVICE           |
|  [52]   | gRPC server host over UDS with socket-mode and peer-credential intake gating     | companion-sidecar#SERVICE_HOST              |
|  [53]   | Cross-process degradation cascade: parent floor written to the child cell        | companion-sidecar#DEGRADATION_CASCADE       |
|  [54]   | Accept-side peer-credential read over the platform getsockopt route              | companion-sidecar#PEER_ADMISSION            |
|  [55]   | Host-side attached-peer roster: lease-epoch admit/renew/drop/sweep, presence fan | companion-sidecar#PROCESS_MODALITY          |
|  [56]   | Fleet-wide health-gated rolling-update wave over the attached-peer roster        | provisioning-and-update#ROLLOVER_DRAIN      |
|  [57]   | Boot-minted tenant context: 4th cross-process primitive stamped on the envelope  | runtime-ports#PORT_RECORDS                  |
|  [58]   | W3C trace-context propagation over the control hop via gRPC metadata             | diagnostics-and-telemetry#CORRELATION_SPINE |

## [6]-[CAPABILITY_AND_EXTENSIBILITY]

The self-describing op catalog, the agent and SDK projection, the capability-brokered plugin sandbox, the solver-extension contract, the reactive external-binding studio, and the reproducibility kernel.

| [INDEX] | [FEATURE]                                                                        | [PAGE#CLUSTER]                            |
| :-----: | :------------------------------------------------------------------------------- | :---------------------------------------- |
|  [59]   | Self-describing op catalog with effect/idempotency/cost/permission columns       | capability-registry#DESCRIPTOR_AXIS       |
|  [60]   | Shape-discriminated discovery over the frozen registry, degradation-gated        | capability-registry#DISCOVERY_FOLD        |
|  [61]   | Commit-or-rollback intent transaction over the Compute dispatch rail             | capability-registry#COMMAND_ALGEBRA       |
|  [62]   | Scoped grant algebra with consent/elevation, cost metering, dry-run sim          | capability-registry#GRANT_BROKER          |
|  [63]   | Polyglot C#/TS/Python SDK codegen off one descriptor source                      | capability-registry#SDK_CODEGEN           |
|  [64]   | MCP tool/resource/prompt projection from the capability registry                 | mcp-projection#METHOD_AXIS                |
|  [65]   | MCP dry-run cost preview and brokered tool dispatch                              | mcp-projection#TOOL_DISPATCH              |
|  [66]   | Streaming progress, cancellation, backpressure, resumable agent handles          | mcp-projection#STREAM_PROGRESS            |
|  [67]   | WASM-component and process isolation with no-ambient-authority load              | sandbox-host#ISOLATION_AXIS               |
|  [68]   | Capability-brokered grant handle mediating every plugin host call                | sandbox-host#GRANT_HANDLE                 |
|  [69]   | Per-plugin quota cell with kill-and-quarantine rail                              | sandbox-host#QUOTA_CONTROL                |
|  [70]   | Fail-closed supply-chain gate: signature, SLSA attestation, semver               | sandbox-host#SUPPLY_CHAIN                 |
|  [71]   | Seven-category solver-plugin contract over one hosting fold                      | solver-plugin#SOLVER_KIND                 |
|  [72]   | Declared-op-to-descriptor plugin contract validation                             | solver-plugin#PLUGIN_CONTRACT             |
|  [73]   | Sandboxed solver hosting with canonical-representation negotiation               | solver-plugin#SOLVER_HOSTING              |
|  [74]   | Eight-transport reactive bidirectional external binding axis                     | live-wire#TRANSPORT_AXIS                  |
|  [75]   | Edge unit coercion through the Compute unit algebra on every inbound value       | live-wire#BINDING_SPEC                    |
|  [76]   | Transactional write-back with acknowledgement and rollback                       | live-wire#WRITE_BACK                      |
|  [77]   | Per-binding connect/subscribe/stale/fault health folded into the host fold       | live-wire#BINDING_HEALTH                  |
|  [78]   | Determinism kernel: pinned RNG, float mode, environment fingerprint              | determinism-and-replay#DETERMINISM_KERNEL |
|  [79]   | Hash-chained content-addressed command log riding the durable op-log             | determinism-and-replay#EVENT_LOG          |
|  [80]   | Replay-verify with per-step content-hash identity proof                          | determinism-and-replay#REPLAY_VERIFY      |
|  [81]   | Macro record/replay as a content-addressed parameterized atomic batch            | determinism-and-replay#MACRO_ENGINE       |
|  [82]   | Partial recompute over content-address dependency edges with unchanged-prune     | determinism-and-replay#RECOMPUTE_GRAPH    |
|  [83]   | Energy/thermal-aware compute-fidelity scaling from live power state              | host-profiles#POWER_AND_FIDELITY          |
|  [84]   | Multi-channel delivery fan-out with delivery receipts and dedupe                 | outbound-resilience#DELIVERY_FANOUT       |
|  [85]   | Declarative alert engine: threshold/anomaly/forecast with hysteresis, escalation | health-and-degradation#ALERT_ENGINE       |
