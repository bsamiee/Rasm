# [TS_IAC_TASKLOG]

Deploy-plane work ledger distilled from `IDEAS.md`: each open card leads with a status marker and three to four scoped bullets, and each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[ENV_KEY_CATALOG]-[QUEUED]: Channel-to-variable spellings become a typed catalog both seam ends derive.
- Capability: environment spellings crossing `StackOutputs` into process config derive from one typed key owner, so a rename breaks both ends at compile time and the OTLP endpoint's two live spellings reconcile to one.
- Shape: the typed catalog at the `StackOutputs` owner in `libs/typescript/iac/.planning/program/spec.md` (or beside `kube/workload.md` `_KEYS`); runtime `proc/config.md` `Setting` group and row names derive from the same owner; the divergence — runtime reads `RUNTIME_OTEL_ORIGIN`, `_KEYS` emits `OTEL_EXPORTER_OTLP_ENDPOINT` — resolves to one spelling.
- Unlocks: publishing a channel to processes is one catalog row; the hand-comment-maintained match dies.
- Anchors: `kube/workload.md` `_KEYS`/`_POLICY`; runtime `proc/config.md` `Setting` `RUNTIME` nesting; the env-catalog ruling at `libs/typescript/.planning/RULINGS.md` `[01]-[SHAPE]`.

[PROVIDER_CREDENTIAL_ROSTER]-[QUEUED]: Generated-credential entries spell once — two byte-identical arm blocks collapse.
- Capability: the credential `entries` roster is one value both selfhosted arms compose, so a credential addition is one row edit and the arms cannot drift on membership.
- Shape: one roster value in `libs/typescript/iac/.planning/program/provider.md` composed by both arm blocks carrying the identical `entries` records.
- Unlocks: credential membership has one spelling across the metal bootstrap and the escalation arm.
- Anchors: `provider.md` byte-identical `entries` blocks (`DB_ADMIN_PASSWORD`/`DB_PASSWORD`/`DB_ANALYST_PASSWORD`/`OBJECT_USER`/`OBJECT_PASSWORD`/`GRAFANA_PASSWORD`) at the two arms.
- Atomic: one hoist, two compositions.

[GRAFANA_POLICY_FIELD]-[BLOCKED]: Notification grouping compiles over the cataloged `NotificationPolicyArgs` field.
- Capability: delivery grouping — tenant identity included — restores as contact `groupBy` data; drives from IDEAS `[BOARD_MEMBER_CATALOGS]`.
- Shape: the grouping restore on `operate/observe.md`, routed by its `[08]-[RESEARCH]` row.
- Unlocks: IDEAS.md [BOARD_MEMBER_CATALOGS] — full Grafana delivery depth.
- Anchors: `operate/observe.md` `[08]-[RESEARCH]`; `libs/typescript/iac/.api/pulumiverse-grafana.md`.
- Arms: `libs/typescript/iac/.api/pulumiverse-grafana.md` catalogs the exact `NotificationPolicyArgs` grouping field and input shape.
[LEASE_CUSTODY_CELLS]-[BLOCKED]: Lease custody cells decode from the security-encoded `LeaseSpec`.
- Capability: the app-root fold realizes each lease as a Doppler token and namespace custody cell; drives from IDEAS `[LEASE_REALIZATION]`.
- Shape: the custody-cell fold answering `kube/workload.md` `[06]-[RESEARCH]`.
- Unlocks: IDEAS.md [LEASE_REALIZATION] — lease-bounded credential custody across workloads.
- Anchors: `kube/workload.md` `[06]-[RESEARCH]`; security `crypt/secret.md` custody rows.
- Arms: security's encoded `LeaseSpec` owner lands at `libs/typescript/security/.planning/crypt/secret.md` under security IDEAS `[LEASE_SPEC_CONTRACT]` and TASKLOG `[LEASE_SPEC_SCHEMA]`.
[FOUNDATION_PANEL_ROWS]-[BLOCKED]: Advanced panel fields compile over cataloged Foundation-SDK builder rows.
- Capability: panel-link, Geomap, Table, and Timeseries fields restore into the board compile; drives from IDEAS `[BOARD_MEMBER_CATALOGS]`.
- Shape: the restored panel-field arms on `operate/observe.md`, routed by its `[08]-[RESEARCH]` row.
- Unlocks: IDEAS.md [BOARD_MEMBER_CATALOGS] — full panel depth on the compiled boards.
- Anchors: `operate/observe.md` `[08]-[RESEARCH]`; `libs/typescript/iac/.api/grafana-grafana-foundation-sdk.md`.
- Arms: `libs/typescript/iac/.api/grafana-grafana-foundation-sdk.md` catalogs the exact panel-link, Geomap, Table, and Timeseries builder rows.
[PULSE_PACK_PROJECTION]-[BLOCKED]: The `runtime.pulse` pack row decodes a real `Pulse.Board` projection.
- Capability: the pack ingest consumes the runtime-owned `DashboardModel` and `Alert.Spec` projection instead of tagging pre-encoded rows; drives from IDEAS `[PULSE_BOARD_DECODER]`.
- Shape: the `runtime.pulse` decode arm on `operate/observe.md` `_PACKS`.
- Unlocks: IDEAS.md [PULSE_BOARD_DECODER] — runtime evidence compiles onto the shared board plane.
- Anchors: `libs/typescript/runtime/.planning/otel/meter.md` board law; `operate/observe.md` `_PACKS`.
- Arms: `libs/typescript/runtime/.planning/otel/meter.md` lands the executable core `DashboardModel` and `Alert.Spec` projection its board law assigns to the app.
[UI_ASSET_IDENTITY_TYPES]-[BLOCKED]: `Source.distribute` consumes typed UI asset identity.
- Capability: `Source.AssetRow` is replaced by the UI-owned encoded roster type, so `Source.distribute` and `_addressed` — already standing — serve typed identity; drives from IDEAS `[UI_ASSET_ROSTER_SEAM]`.
- Shape: the typed-identity swap on `program/source.md`, routed by its `[04]-[RESEARCH]` row.
- Unlocks: IDEAS.md [UI_ASSET_ROSTER_SEAM] — caller-typed asset serving end to end.
- Anchors: `program/source.md` `[04]-[RESEARCH]`; `Source.distribute` and `_addressed`.
- Arms: UI IDEAS `[ASSET_IDENTITY]` and TASKLOG `[ASSET_IDENTITY_ROSTER]` land the encoded roster.
[PACK_DESCRIPTOR_INGEST]-[BLOCKED]: The `_PACKS` descriptor ingest decodes every producer projection.
- Capability: the claimed descriptor ingest lands over real producer projections instead of the closed provenance vocabulary alone; drives from IDEAS `[PRODUCER_PACK_DESCRIPTORS]`.
- Shape: the descriptor ingest arm on `operate/observe.md` `_PACKS`.
- Unlocks: IDEAS.md [PRODUCER_PACK_DESCRIPTORS] — producer-owned dashboards compile without deploy-side re-authoring.
- Anchors: `operate/observe.md` `_PACKS`; `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md` telemetry fan.
- Arms: every producer route exposes an executable `{ boards, alerts }` projection, `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md` first — it carries telemetry fan rows but no dashboard descriptor.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[OBSERVE_DEEPEN]-[COMPLETE]: `operate/observe.md` deepened — store family with tenancy/retention/degradation columns, `_pg` ingest arms, `_DEV` row, `_compiled` builder fold, `MessageTemplate`/`MuteTiming`/`Annotation` rows; `spec.md` gained the `observe` profile group.
[FOUNDATION_SDK_ADMISSION]-[COMPLETE]: `@grafana/grafana-foundation-sdk` admitted — README row and `.api/grafana-grafana-foundation-sdk.md`; deploy-host self-telemetry law on `program/automation.md`.
[OBSERVE_REALIZE_PASS]-[COMPLETE]: observe realize pass — `_compiled` emission-total (`time`/`annotation`/`withVariable` folds, `description`/`transparent`/`legendFormat`, sorted threshold steps over the `-Infinity` base), `slo.SLO` respell, Editor-role identity with `FolderPermissionItem` grant and `ServiceAccountRotatingToken`, org-scoped tenant fleets, `Dev` tier wired into the docker arm with `Boards` over one URL plane.
[COLLECTOR_DEPTH_ROWS]-[COMPLETE]: collector depth rows — `service.telemetry.metrics.readers` self-metrics, the `ebpf` OBI chart row, and the `_CNPG_JOB` operator-metrics scrape on both ingest arms in `operate/observe.md`.
[RECEIPT_EVIDENCE_ARMS]-[COMPLETE]: receipt evidence arms — `violations`/`output`/`timing` fields with matching `_folded` rows on `program/automation.md`.
[DEPLOY_BENCH_SERIES]-[COMPLETE]: deploy benchmark series — `Automation.series` projects `history` rows into `{ version, op, result, seconds, changes }` on `program/automation.md`.
[WORKSPACE_VERB_SET]-[COMPLETE]: fleet-workspace verbs — `cancel`, polymorphic `_config` overload set, `rename`, `whoAmI`/`listStacks`/`installPlugin`, `fullyQualifiedStackName` identity on `program/automation.md`.
[PLATFORM_CONTRACT_RAIL]-[COMPLETE]: platform-contract rail — `Evidence.file` FileSystem sink and `_CURSOR` KeyValueStore checkpoint on `operate/policy.md`; `layerDotEnv`/`NodeContext.layer`/`runMain` composition-root law on `program/automation.md`.
[TIER_HOOK_ROWS]-[COMPLETE]: tier hook rows — `Tier.hooked`/`_HOOKS` named `ResourceHook`/`ErrorHook` registry riding the `child()` fold on `program/spec.md`.
[EVIDENCE_DELIVERY_SPINE]-[COMPLETE]: evidence-delivery spine — the six-row `Evidence` union and never-failing sink on `operate/policy.md`, referenced by the `Secrets.webhook` law and the `CloudPlane` webhook row.
[BRIDGED_ROSTER_DEPTH]-[COMPLETE]: bridged-roster depth — analyst `Role`/`GrantRole`/`Grant`/`DefaultPrivileges` rows on the docker cell in `program/provider.md`; the CNPG `inRoles` analyst law on `kube/data.md`.
[REPLICATION_LABEL_ROWS]-[COMPLETE]: replication slots, labels, sequences — `ReplicationSlot` docker row in `program/provider.md`, `replicationSlots.highAvailability` and the label-posture law on `kube/data.md`, `Drift.conform` widened over `getSequences` on `operate/policy.md`.
[CLOUD_PARITY_POLICIES]-[COMPLETE]: cloud parity policy rows — `bucket-versioned-aws`/`bucket-versioned-gcp`/`iam-floor`/`tenant-fence` appended to `_policies` on `operate/policy.md`.
[PREVIEW_UNKNOWN_GUARD]-[COMPLETE]: preview-unknown guard refuted to the engine seam — the policy host wraps validator props itself (`operate/policy.md` law); `R2BucketLifecycle` aging row on the cloudflare arm in `program/provider.md`.
[OPENCOST_ROW]-[COMPLETE]: OpenCost row — `opencost` chart row over the store row's read URL on `operate/observe.md`; `observe.costs` toggle on `program/spec.md`; docker degrade stated on the dev row.
[MIMIR_STORAGE_BINDING]-[COMPLETE]: mimir binding — `_estate` threads the object plane into `Lgtm.Args.objects`, `structuredConfig.common.storage` + per-section buckets + `compactor_blocks_retention_period` land in the store values, per-row `retain` dialects settle retention.
[SECURITY_PACK_ROW]-[COMPLETE]: security pack ingest — the `security.audit` `_PACKS` row joins decoded boards and alerts into the one compile and `_alerted` folds on `operate/observe.md`.
[ROLL_ANNOTATION_ROW]-[COMPLETE]: roll-annotation source row — `Boards.Args.rolls` maps the AppHost wire onto `oss.Annotation` rows with `_ROLL_TONES` verdict tones on `operate/observe.md`.
