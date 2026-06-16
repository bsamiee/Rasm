# [APPHOST_FEATURES]

Isolation-concept atlas for the runtime spine. Every concept is a row or case on a budgeted DENSITY_BAR owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from the charter DENSITY_BAR via the `[OWNER]` cell.

## [1]-[ISOLATION_SPINE]

The spine concepts this folder owns and no sibling reverses — the host-variance, lifecycle, time, configuration, and composition axes that boot, degrade, drain, and report every modality with zero app-side ceremony.

| [INDEX] | [FEATURE]                                                                      | [OWNER]                | [PAGE#CLUSTER]                                |
| :-----: | :---------------------------------------------------------------------------- | :--------------------- | :------------------------------------------- |
|   [1]   | Modality boot from one variance axis                                          | `HostProfile`          | host-profiles#PROFILE_AXIS                   |
|   [2]   | Per-user roots and telemetry resource identity from the resolved record       | `ProfileIdentity`      | host-profiles#RESOURCE_IDENTITY              |
|   [3]   | sd_notify service-state lifecycle mirror with watchdog keep-alive             | `ProfileBoot`          | host-profiles#LIFETIME_ADAPTERS              |
|   [4]   | Total phase machine with CAS-committed receipts and LIFO subscriptions        | `Lifecycle`            | lifecycle-and-drain#PHASE_FAMILY             |
|   [5]   | Crash-recovery and upgrade boot probe over own and host crash markers         | `FaultSource`          | lifecycle-and-drain#FAULT_SPINE             |
|   [6]   | Unhandled, unobserved, and POSIX fault traps with SIGHUP reload, SIGTERM drain | `FaultSource`         | lifecycle-and-drain#FAULT_SPINE             |
|   [7]   | Rank-band cooperative-to-forced drain with per-step straggler receipts        | `DrainBand`            | lifecycle-and-drain#DRAIN_CONDUCTOR         |
|   [8]   | One cancellation root; every token derives with provenance and deadline       | `CancelScope`          | lifecycle-and-drain#CANCEL_SPINE           |
|   [9]   | One injected clock pair: elapsed versus semantic time, sentinel admission     | `ClockPolicy`          | time-and-deadlines#CLOCK_SPLIT             |
|  [10]   | Deadline taxonomy; every suite duration literal traces to a row               | `DeadlineClass`        | time-and-deadlines#DEADLINE_TAXONOMY       |
|  [11]   | One scheduler: cron and period rows, lease split, missed-occurrence sweep     | `ScheduleEntry`        | time-and-deadlines#SCHEDULE_PORT           |
|  [12]   | Watchdog heartbeat as a schedule row folding a not-met receipt to a trigger   | `ScheduleEntry`        | time-and-deadlines#SCHEDULE_PORT           |
|  [13]   | Ranked config sources mounted onto one chain with rank-fold precedence        | `ConfigSource`         | configuration-and-options#SOURCE_AXIS      |
|  [14]   | Fail-closed source-gen binding into validated immutable policy records        | `ConfigError`          | configuration-and-options#TYPED_BINDING    |
|  [15]   | Validate-once frozen publish with reload-class-gated receipted transitions    | `ReloadOutcome`        | configuration-and-options#POLICY_VALUES    |
|  [16]   | Operator kill-switch as one transition-class row forcing the degradation fold | `OperatorOverride`     | configuration-and-options#KILL_SWITCH      |
|  [17]   | One composition root: frozen module table folds and freezes the service graph | `CompositionSurface`   | composition-and-modules#MODULE_TABLE       |
|  [18]   | Keyed decoration of contributor ports, profile-conditional via `TryDecorate`  | `BoundaryActivation`   | composition-and-modules#BOUNDARY_ACTIVATION |
|  [19]   | Admission-edge activation: availability probe, async drain scope, validators  | `BoundaryActivation`   | composition-and-modules#BOUNDARY_ACTIVATION |

## [2]-[RESOURCE_AND_OUTBOUND]

Bounded runtime resource lanes and the single outbound boundary — the cache, pool, queue, and hop axes this folder owns so no downstream app hand-rolls them per call site.

| [INDEX] | [FEATURE]                                                                      | [OWNER]              | [PAGE#CLUSTER]                       |
| :-----: | :---------------------------------------------------------------------------- | :------------------- | :----------------------------------- |
|  [20]   | Lane-keyed hybrid cache: stampede single-flight, tag cut, per-lane keyed L2   | `CacheLane`          | resource-lanes#CACHE_PORT            |
|  [21]   | Delegate-row object pools: reset law, sanity predicate, leak-tracked test row | `PoolPolicy<T>`      | resource-lanes#OBJECT_POOLS          |
|  [22]   | Bounded drainable queues with receipted loss; pipe versus network split       | `DrainQueue<T>`      | resource-lanes#DRAIN_QUEUES          |
|  [23]   | Outbound hop cases bound to frozen policy rows with total dispatch             | `OutboundHop`        | outbound-resilience#HOP_AXIS         |
|  [24]   | Standard and hedging HTTP pipelines; weighted multi-region routing as a subrow | `OutboundHop`        | outbound-resilience#HTTP_PIPELINES   |
|  [25]   | One keyed Polly registry per non-HTTP hop; breaker read/write split; chaos     | `OutboundHop`        | outbound-resilience#KEYED_PIPELINES  |
|  [26]   | Companion discovery manifest, UDS attach, checksum gate, child lifecycle       | `DiscoveryManifest`  | outbound-resilience#DISCOVERY_ATTACH |
|  [27]   | One retry owner per hop: claim cell, conflict receipt, kill-switch enforce     | `OutboundSurface`    | outbound-resilience#OWNERSHIP_LAW    |

## [3]-[OBSERVABILITY_AND_HARDENING]

The unified observability and hardening posture — traces, metrics, and logs through minted identities, the HLC causal primitive, redaction at every egress, and bounded redacted support capture. One posture, owned here, never re-derived by an app.

| [INDEX] | [FEATURE]                                                                      | [OWNER]                  | [PAGE#CLUSTER]                               |
| :-----: | :---------------------------------------------------------------------------- | :----------------------- | :------------------------------------------- |
|  [28]   | Minted source and meter identity with a source-gen instrument registry        | `TelemetrySource`        | diagnostics-and-telemetry#TELEMETRY_IDENTITY |
|  [29]   | One boot-minted correlation id stamped across every signal and hop            | `Correlation`            | diagnostics-and-telemetry#CORRELATION_SPINE  |
|  [30]   | Per-profile log pipeline arbitration: serilog-projection versus otel-export   | `LogPipeline`            | diagnostics-and-telemetry#LOG_PROJECTION     |
|  [31]   | Source-gen lib-level log delegates with sink-loss listener fold               | `LogPipeline`            | diagnostics-and-telemetry#LOG_PROJECTION     |
|  [32]   | Per-signal governance: sampling ratio, buffering, exemplars, metric views     | `TelemetrySignal`        | diagnostics-and-telemetry#SIGNAL_GOVERNANCE  |
|  [33]   | Checkpoint latency-context recorder drained at the band                         | `TelemetrySignal`        | diagnostics-and-telemetry#SIGNAL_GOVERNANCE  |
|  [34]   | OTLP export backend-parameterized by env across Prometheus, Tempo, Loki, Alloy | `TelemetrySignal`       | diagnostics-and-telemetry#SIGNAL_GOVERNANCE  |
|  [35]   | Classification taxonomy; redaction bound at every exporter seam                | `DataClassification`     | diagnostics-and-telemetry#REDACTION_TAXONOMY |
|  [36]   | Resource-pressure health fold with container-limit cgroup-v2 grading           | `HealthContributorRow`   | health-and-degradation#HEALTH_FOLD           |
|  [37]   | Usable-failure degradation rail with hysteresis                                | `DegradationLevel`       | health-and-degradation#DEGRADATION_RAIL      |
|  [38]   | Wire health: tag-predicate registry to `grpc.health.v1`; set-degradation route | `WireHealthRow`         | health-and-degradation#WIRE_HEALTH           |
|  [39]   | Support trigger union auto-arming on fault, health, watchdog, schedule          | `SupportTrigger`         | support-bundles#TRIGGER_UNION                |
|  [40]   | Bounded redacted capture: window freeze, ordered fan-in, caps, coalesce        | `SupportTrigger`         | support-bundles#CAPTURE_PIPELINE             |
|  [41]   | Retention sweep: max-bundle and max-age eviction as a schedule row             | `SupportReceipt`         | support-bundles#MANIFEST_RECEIPT             |
|  [42]   | Process-dump artifact row over the dump tool for support capture               | `SupportTrigger`         | support-bundles#CAPTURE_PIPELINE             |

## [4]-[CROSS_PACKAGE_PORTS]

The seven inward port records — the only cross-package seam — plus the suite wire law and TS map. Siblings adapt to these; the dependency never reverses.

| [INDEX] | [FEATURE]                                                                      | [OWNER]               | [PAGE#CLUSTER]              |
| :-----: | :---------------------------------------------------------------------------- | :-------------------- | :------------------------- |
|  [43]   | HLC-stamped receipt envelope as the sole cross-process causal primitive       | `ReceiptSinkPort`     | runtime-ports#PORT_RECORDS |
|  [44]   | Typed inward ports; one beyond the budgeted set is the named defect           | `ReceiptSinkPort`     | runtime-ports#PORT_RECORDS |
|  [45]   | One Strict JSON wire law per package; app roots merge resolvers, emit schemas | `AppHostWireContext`  | runtime-ports#WIRE_LAW     |
|  [46]   | TS dashboard contract: one envelope, schema-derived types, codec residence    | `AppHostWireContext`  | runtime-ports#TS_PROJECTION |

## [5]-[PROVISIONING_AND_CONTROL]

The post-fetch update rail and the inbound control-service host — the self-updating, operationally-controllable, multi-process legs the spine owns so a companion farm updates, serves its control plane, and cascades degradation with zero app-side ceremony.

| [INDEX] | [FEATURE]                                                                      | [OWNER]              | [PAGE#CLUSTER]                          |
| :-----: | :---------------------------------------------------------------------------- | :------------------- | :-------------------------------------- |
|  [47]   | Post-fetch update state machine: download, stage, rollover, rollback receipts | `UpdateRail`         | provisioning-and-update#UPDATE_RAIL     |
|  [48]   | Three-channel feed axis with explicit-channel and downgrade-policy columns     | `UpdateChannel`      | provisioning-and-update#CHANNEL_AXIS    |
|  [49]   | Drain-before-swap rollover folding the conductor ahead of the restart          | `RolloverDrain`      | provisioning-and-update#ROLLOVER_DRAIN  |
|  [50]   | Process-modality axis: companion, sidecar, paired-peer spawn-attach-discovery  | `ProcessModality`    | companion-sidecar#PROCESS_MODALITY      |
|  [51]   | Inbound control-service host folding three verbs onto existing owners          | `ControlInbound`     | companion-sidecar#CONTROL_SERVICE       |
|  [52]   | gRPC server host over UDS and hardened Windows named-pipe control intake       | `ServiceHost`        | companion-sidecar#SERVICE_HOST          |
|  [53]   | Cross-process degradation cascade: parent floor written to the child cell      | `DegradationCascade` | companion-sidecar#DEGRADATION_CASCADE   |
|  [54]   | Accept-side peer-credential read over the platform getsockopt route            | `PeerAdmission`      | companion-sidecar#PEER_ADMISSION        |

## [6]-[ROUTING]

- Concept mechanics: `.planning/<page>.md` at the cluster anchor on each row.
- Owner budget, public-surface cap, and owner realization state: `.planning/README.md#DENSITY_BAR`.
- Gap closure and admissions ledger: `.planning/README.md#GAP_LEDGER`, `#ADMISSIONS_RECORD`, `#CATALOGUE_PENDING`.
- Package API truths grounding each fence: `.api/*.md` routed by `.api/README.md`.
- Cross-package and multi-process topologies consuming these owners: `../../.planning/FEATURES.md`.
