# [TS_IAC_IDEAS]

Deploy-plane idea pool: higher-order concepts grounded in spec-total realization, the arm roster, and the operate/kube tiers; an idea drives one or more `TASKLOG.md` tasks, and a resolved idea closes with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[LEASE_REALIZATION]-[QUEUED]: Security-minted leases realize as deploy custody cells.
- Capability: the app-root fold decodes an encoded `LeaseSpec` into a config-scoped Doppler token and namespace custody cell as pure data, lease semantics never re-derived deploy-side.
- Shape: the app-root Doppler-token and namespace-cell fold on `operate/secret.md`'s `Secrets` custodian, feeding `kube/workload.md`'s `Workload.Args`.
- Unlocks: leased credentials with lease-bounded blast radius across the workload estate.
- Anchors: `libs/typescript/security/.planning/crypt/secret.md` `LeaseSpec` owner and its `SECURITY_LEASE_SPEC` app-root fold; `operate/secret.md` `Secrets` custodian; `kube/workload.md` `Workload.Args`.
- Ripple: `security` `[LEASE_SPEC_CONTRACT]`.

[UI_ASSET_ROSTER_SEAM]-[QUEUED]: Served-asset identity types from the UI-owned roster.
- Capability: static distribution and `served` outputs — already standing — gain typed asset identity from the encoded UI roster, closing the cross-folder identity seam.
- Shape: the identity seam on `program/source.md`, replacing `Source.AssetRow` with `ui:viewer/scene#RESIDENCY_GRAFT`'s `Glb.AssetIdentity` row and typing the `assets` array as `Glb.AssetRoster`.
- Unlocks: content-addressed codec serving with an airtight CSP and no caller-untyped identity.
- Anchors: `program/source.md` `Source.AssetRow`/`_addressed`; `ui:viewer/scene#RESIDENCY_GRAFT` `Glb.AssetIdentity` beside the `Glb.AssetRoster` array `Glb.assetPath` reads one row of, and its `assets/<digest>/<file>` derivation.
- Ripple: `ui` `[ASSET_IDENTITY]`.

[GENERATION_ROLLOUT_STRATEGY]-[QUEUED]: Backend generations cut over under a declared rollout strategy, not one atomic pointer flip.
- Capability: cutover becomes a strategy row the deploy plane folds — immediate, canary over a traffic fraction, or paired blue-green targets — each carrying its own admission evidence and its own abort predicate, so a generation that proves in the cluster but fails under live traffic retires without a manual runbook.
- Shape: one strategy vocabulary on `operate/converge.md` `[04]-[PUBLICATION]` threading the pointer write, with the traffic split reading the `kube/traffic.md` Gateway rows.
- Unlocks: a generation reaches production behind a measured gate instead of an all-at-once pointer write, and rollback becomes strategy data rather than a second verb.
- Anchors: `operate/converge.md` `[04]-[PUBLICATION]` retained-evidence and pointer rows; `kube/traffic.md` Gateway API edge; the Automation-API ledger serializing pointer writes.
- Tension: the pointer write is the atomic cutover and the whole rollback story; a strategy row that keeps two generations live at once forces readiness to compare against a set rather than one pointer value.

[RESIDENCE_SIGNAL_CENSUS]-[QUEUED]: Each residence row censuses what its plane HOLDS, not only what the gateway routes into it.
- Capability: one column answers the whole residence question — which signals a plane holds and therefore which readers may join across it — so an evidence query resolves its plane from the row rather than from knowledge of who wrote the bytes.
- Shape: the cold-tail row's `signals` answer on `operate/observe.md` `[03]-[CHART_ROWS]`, beside the residence law that reads it and the folder ruling stating the two-signal bound.
- Unlocks: a board joining series to wide events picks its plane off the row, and a reader stops inferring residence contents from which branch owns the writer.
- Anchors: the shared `_Plane` floor whose `signals` column the fan-out now reads; the collector-side law bounding the exporter to the wide-event pair; the cold tail's own ingest column naming a data-branch writer rather than a collector exporter.
- Tension: the two residences fill by different mechanisms — the interactive plane takes a collector exporter the gateway bounds to logs and traces, while the cold tail takes data-branch writes including a metric point relation no exporter carries — so one column currently answers a routing question on one row and a contents question on the other, and the folder ruling reads as though it bounded both.
- Tension: widening the cold-tail answer without re-reading the ruling reopens the third-signal case the ruling closed; the honest split states the routing bound at the exporter and the contents census at the column, which is a ruling edit rather than a value flip.
- Ripple: `data` `[OLAP_LAKE_FILLS]`; the folder ruling bounding residence signals precedes any column edit.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[PRODUCER_PACK_DESCRIPTORS]-[COMPLETE]: every signal-bearing producer now reaches the board plane as its own pack — the kernel `BoardPack` carries the provenance key as its first column, so all six C# packs seat beside `runtime.pulse` and `security.audit` and the tuple censuses closed in both directions; python geometry claims no seat because it mints a measure charter and a fault boundary rather than a board-and-alert projection.
[SECURITY_PACK_INGEST]-[COMPLETE]: the producer landed and the seat returned — `Audit.pack`/`Audit.wire` on `libs/typescript/security/.planning/access/audit.md` seal the folder board beside its burn specs as one encoded value, so `security.audit` re-enters `_PACKS` under the tuple's own earn-test rather than as a wire nobody mints.
[STORE_SUFFIX_PARITY]-[COMPLETE]: re-proved against the settled rule — core resolved suffixing as a RENDER property (`Convention.translated` over the `_translation` strategy roster, the `_promUnit` word, and the `_tail` type tail), so each `_stores` row's `translation` answer already reaches every selector through one unchanged query value and the three dialects need no per-row re-derivation.
[RESIDENCE_BOARD_TILES]-[COMPLETE]: one board reads both planes — the `clickhouse` `_SOURCES` row answers the `query` column with a branch-owned dataquery builder, so an evidence tile compiles through the identical `_minted`/`_target`/layout fold a series tile rides; the pinned SDK's codegen roster closes CONVENIENCE and never admission, since `withTarget` types `cog.Builder<cog.Dataquery>` over a marker-only interface carrying no brand, private field, or registry probe.
[PULSE_BOARD_DECODER]-[DROPPED]: refuted at its premise — `runtime/otel/meter#BOARD` rules the pack runtime's mint and the app's projection, and `_PACKS` already ingests core-encoded boards and alerts, so no iac decode of `Pulse.Board` is owed; the projection consumes `DashboardModel.Board` (analytics target, identity, logs source, metrics target), which only a composition root holds, and Tier-0's library-neutrality law seats that root outside `libs/`.
[BOARD_MEMBER_CATALOGS]-[COMPLETE]: full-depth board compilation — the Foundation-SDK catalog answered every axis (`DashboardLinkBuilder` panel and board links, `FrameGeometrySourceBuilder` coordinate mapping, `TableSortByFieldStateBuilder` beside `TableFooterOptionsBuilder.enablePagination`, the Timeseries axis and `VizTooltipOptionsBuilder` rows), so `_minted` and `_compiled` on `operate/observe.md` now reach every field the core panel family declares and no model field survives as an inert emission fact.
[STORE_RULE_EVALUATION]-[COMPLETE]: store-owned burn evaluation — the `rules` column landed on the `_stores` family, each row spelling its own loader (prometheus values, mimir's local ruler storage, victoriametrics declaring the absence on `degrade`), and `_expr` reads the recorded numerator back where the row evaluates it; the Grafana-managed resource is refused by ruling because it writes through remote-write.
[METRICS_STORE_FAMILY]-[COMPLETE]: metrics-store row family + typed board compile — realized on `operate/observe.md`: `_stores` (`prometheus | mimir | victoriametrics`), Pyroscope row, pg-ingest arm pair, dev row, Foundation-SDK compile leg.
[RECEIPT_ENGINE_EVIDENCE]-[COMPLETE]: receipt-total engine evidence — `RunReceipt` carries `violations`/`output`/`timing` with matching one-pass `_folded` arms and `Automation.series` benchmarks history on `program/automation.md`.
[FLEET_WORKSPACE_VERBS]-[COMPLETE]: fleet-workspace verbs + platform-contract rail — `cancel`/polymorphic `config`/`rename`/`whoAmI`/`listStacks`/`installPlugin`/`fullyQualifiedStackName` on `program/automation.md`; `Evidence.file`/`_CURSOR` platform sinks on `operate/policy.md`.
[DEPLOY_HOOK_RAIL]-[COMPLETE]: deploy lifecycle hook rail — `Tier.hooked`/`_HOOKS` named-hook registry (`rasm.iac.<tier>.<point>`) on `program/spec.md`; the `Evidence` sink vocabulary spans run settle, drift, rotation, and both webhook sources on `operate/policy.md`.
[POSTGRES_ESTATE_DEPTH]-[COMPLETE]: Postgres logical-estate depth — docker-cell analyst tier (`Role`/`GrantRole pg_read_all_data`/`Grant`/`DefaultPrivileges`) with `ReplicationSlot` on `program/provider.md`; CNPG analyst `inRoles` row, `replicationSlots.highAvailability`, and label posture on `kube/data.md`; `Drift.conform` widened over `getSequences` on `operate/policy.md`.
[CLOUD_ARM_INVARIANTS]-[COMPLETE]: cloud-arm invariants — `bucket-versioned-aws`/`bucket-versioned-gcp`/`iam-floor`/`tenant-fence` rows and the engine-guard preview-unknown law on `operate/policy.md`; `R2BucketLifecycle` aging on the cloudflare arm.
[COST_ATTRIBUTION_PLANE]-[COMPLETE]: tenant cost-attribution plane — the `opencost` chart row bound to the selected store row's read URL on `operate/observe.md`, the `observe.costs` toggle on `program/spec.md`, the docker-loop degrade stated on the dev row.
[FLEET_METRICS_SETTLEMENT]-[COMPLETE]: fleet metrics settlement — mimir `structuredConfig.common.storage` object binding threaded from `_estate`, per-row `retain` retention dialects, collector `service.telemetry` self-metrics, the `ebpf` OBI row, and the `cnpg` operator-metrics scrape job on `operate/observe.md`; superseded in part by `[GATEWAY_OWNERSHIP]`, which folded every store coordinate into one per-row values body.
[ROLL_ANNOTATION_INGEST]-[COMPLETE]: fleet-roll annotation ingestion — `Boards.Args.rolls` decodes the AppHost roll wire onto `oss.Annotation` rows with `_ROLL_TONES` verdict tones on `operate/observe.md`.
[GATEWAY_OWNERSHIP]-[COMPLETE]: the ingest gateway became an owned deployment rather than a chart default — one decoded `Collector` document with closure proof, gateway-side enrichment and span-derived series, a PVC-backed persistent queue with stated bounds, per-row pinned chart names feeding one endpoint projection, and credential material moved off chart values onto a Secret read through `${env:…}`.
