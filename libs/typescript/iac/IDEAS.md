# [TS_IAC_IDEAS]

Deploy-plane idea pool: higher-order concepts grounded in spec-total realization, the arm roster, and the operate/kube tiers; an idea drives one or more `TASKLOG.md` tasks, and a resolved idea closes with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[LEASE_REALIZATION]-[BLOCKED]: Security-minted leases realize as deploy custody cells.
- Capability: the app-root fold decodes an encoded `LeaseSpec` into a config-scoped Doppler token and namespace custody cell as pure data, lease semantics never re-derived deploy-side.
- Shape: the app-root Doppler-token and namespace-cell fold cited by `kube/workload.md` `[06]-[RESEARCH]`.
- Unlocks: leased credentials with lease-bounded blast radius across the workload estate.
- Anchors: `libs/typescript/security/.planning/crypt/secret.md` `LeaseSpec` owner (carded); `kube/workload.md` `[06]-[RESEARCH]`.
- Arms: `libs/typescript/security/.planning/crypt/secret.md` carries the encoded `LeaseSpec` owner required by security IDEAS `[LEASE_SPEC_CONTRACT]` and TASKLOG `[LEASE_SPEC_SCHEMA]`.
- Ripple: `security` `[LEASE_SPEC_CONTRACT]`.
[BOARD_MEMBER_CATALOGS]-[BLOCKED]: Board compilation and delivery grouping compile over cataloged builder members.
- Capability: panel-link, Geomap, Table, Timeseries, and `NotificationPolicyArgs` members carry the full board and delivery depth through verified rows.
- Shape: the restored `_compiled`/`_minted` arms on `operate/observe.md`, routed by its `[08]-[RESEARCH]` rows.
- Unlocks: full-depth board and alert delivery on the observe plane.
- Anchors: `operate/observe.md` `[08]-[RESEARCH]`; the Foundation-SDK and Grafana catalogs.
- Arms: the exact rows named by `operate/observe.md` `[08]-[RESEARCH]` enter the Foundation-SDK and Grafana catalogs.
[PULSE_BOARD_DECODER]-[BLOCKED]: `runtime.pulse` becomes a decoded producer pack, not a bare provenance key.
- Capability: the runtime-owned projection from `Pulse.Board` into core-encoded boards and alerts decodes through the shared pack ingest.
- Shape: the `_PACKS` decode arm on `operate/observe.md` consuming the runtime-owned projection.
- Unlocks: runtime work-plane evidence compiles onto the shared board plane.
- Anchors: `libs/typescript/runtime/.planning/otel/meter.md` `Pulse.Board`; `Boards.Pack`.
- Arms: `libs/typescript/runtime/.planning/otel/meter.md` carries the executable projection from `Pulse.Board` into core-encoded boards and alerts that `Boards.Pack` accepts.
[UI_ASSET_ROSTER_SEAM]-[BLOCKED]: Served-asset identity types from the UI-owned roster.
- Capability: static distribution and `served` outputs — already standing — gain typed asset identity from the encoded UI roster, closing the cross-folder identity seam.
- Shape: the identity seam on `program/source.md`, routed by its `[04]-[RESEARCH]` row.
- Unlocks: content-addressed codec serving with an airtight CSP and no caller-untyped identity.
- Anchors: `program/source.md` `[04]-[RESEARCH]`; ui `viewer/scene.md` codec-injection law.
- Arms: `libs/typescript/ui/IDEAS.md` `[ASSET_IDENTITY]` and `libs/typescript/ui/TASKLOG.md` `[ASSET_IDENTITY_ROSTER]` land the encoded roster cited by `program/source.md` `[04]-[RESEARCH]`.
- Ripple: `ui` `[ASSET_IDENTITY]`.
[PRODUCER_PACK_DESCRIPTORS]-[BLOCKED]: `_PACKS` decodes executable producer descriptors from every producer route.
- Capability: each producer endpoint carries or routes an executable `{ boards, alerts }` projection the pack ingest decodes, cross-language dashboards compiling without deploy-side re-authoring.
- Shape: the descriptor ingest arm over the `_PACKS` provenance vocabulary on `operate/observe.md`.
- Unlocks: producer-owned dashboards and alerts compile onto the one board plane.
- Anchors: the `_PACKS` provenance rows; `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md` telemetry fan.
- Arms: every producer route exposes an executable `{ boards, alerts }` projection, the absent `grasshopper.fan` dashboard descriptor at `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md` first.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[METRICS_STORE_FAMILY]-[COMPLETE]: metrics-store row family + typed board compile — realized on `operate/observe.md`: `_stores` (`prometheus | mimir | victoriametrics`), Pyroscope row, pg-ingest arm pair, dev row, Foundation-SDK compile leg.
[RECEIPT_ENGINE_EVIDENCE]-[COMPLETE]: receipt-total engine evidence — `RunReceipt` carries `violations`/`output`/`timing` with matching one-pass `_folded` arms and `Automation.series` benchmarks history on `program/automation.md`.
[FLEET_WORKSPACE_VERBS]-[COMPLETE]: fleet-workspace verbs + platform-contract rail — `cancel`/polymorphic `config`/`rename`/`whoAmI`/`listStacks`/`installPlugin`/`fullyQualifiedStackName` on `program/automation.md`; `Evidence.file`/`_CURSOR` platform sinks on `operate/policy.md`.
[DEPLOY_HOOK_RAIL]-[COMPLETE]: deploy lifecycle hook rail — `Tier.hooked`/`_HOOKS` named-hook registry (`rasm.iac.<tier>.<point>`) on `program/spec.md`; the `Evidence` sink vocabulary spans run settle, drift, rotation, and both webhook sources on `operate/policy.md`.
[POSTGRES_ESTATE_DEPTH]-[COMPLETE]: Postgres logical-estate depth — docker-cell analyst tier (`Role`/`GrantRole pg_read_all_data`/`Grant`/`DefaultPrivileges`) with `ReplicationSlot` on `program/provider.md`; CNPG analyst `inRoles` row, `replicationSlots.highAvailability`, and label posture on `kube/data.md`; `Drift.conform` widened over `getSequences` on `operate/policy.md`.
[CLOUD_ARM_INVARIANTS]-[COMPLETE]: cloud-arm invariants — `bucket-versioned-aws`/`bucket-versioned-gcp`/`iam-floor`/`tenant-fence` rows and the engine-guard preview-unknown law on `operate/policy.md`; `R2BucketLifecycle` aging on the cloudflare arm.
[COST_ATTRIBUTION_PLANE]-[COMPLETE]: tenant cost-attribution plane — the `opencost` chart row bound to the selected store row's read URL on `operate/observe.md`, the `observe.costs` toggle on `program/spec.md`, the docker-loop degrade stated on the dev row.
[FLEET_METRICS_SETTLEMENT]-[COMPLETE]: fleet metrics settlement — mimir `structuredConfig.common.storage` object binding threaded from `_estate`, per-row `retain` retention dialects, collector `service.telemetry` self-metrics, the `ebpf` OBI row, and the `cnpg` operator-metrics scrape job on `operate/observe.md`.
[SECURITY_PACK_INGEST]-[COMPLETE]: security board and alert packs — the `security.audit` row of the `_PACKS` ingest arm compiles pack boards and joins their burn rows into the one `_alerted` fold on `operate/observe.md`.
[ROLL_ANNOTATION_INGEST]-[COMPLETE]: fleet-roll annotation ingestion — `Boards.Args.rolls` decodes the AppHost roll wire onto `oss.Annotation` rows with `_ROLL_TONES` verdict tones on `operate/observe.md`.
