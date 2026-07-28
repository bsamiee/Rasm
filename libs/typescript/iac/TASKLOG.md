# [TS_IAC_TASKLOG]

Deploy-plane work ledger distilled from `IDEAS.md`: each open card leads with a status marker and three to four scoped bullets, and each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
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

[LEASE_CUSTODY_CELLS]-[QUEUED]: Lease custody cells decode from the security-encoded `LeaseSpec`.
- Capability: the app-root fold realizes each lease as a Doppler token and namespace custody cell; drives from IDEAS `[LEASE_REALIZATION]`.
- Shape: the custody-cell fold on `operate/secret.md`'s `Secrets` custodian, decoding the security-owned `LeaseSpec` into a keys-only Doppler config, a read-only config-scoped service token, and one namespace secret keyed by `scope + epoch`.
- Unlocks: IDEAS.md [LEASE_REALIZATION] — lease-bounded credential custody across workloads.
- Anchors: `libs/typescript/security/.planning/crypt/secret.md` `LeaseSpec` and its `SECURITY_LEASE_SPEC` fold; `operate/secret.md` `Secrets` custodian; `kube/workload.md` `Workload.Args`.

[UI_ASSET_IDENTITY_TYPES]-[QUEUED]: `Source.distribute` consumes typed UI asset identity.
- Capability: `Source.AssetRow` is replaced by the UI-owned encoded roster type, so `Source.distribute` and `_addressed` — already standing — serve typed identity; drives from IDEAS `[UI_ASSET_ROSTER_SEAM]`.
- Shape: the typed-identity swap on `program/source.md` — `Source.AssetRow` becomes `ui:viewer/scene#RESIDENCY_GRAFT`'s `Glb.AssetIdentity`, and `_addressed` keeps deriving the one address both ends compute.
- Unlocks: IDEAS.md [UI_ASSET_ROSTER_SEAM] — caller-typed asset serving end to end.
- Anchors: `program/source.md` `Source.distribute`/`_addressed`; `ui:viewer/scene#RESIDENCY_GRAFT` `Glb.AssetIdentity`/`Glb.assetPath`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[PACK_DESCRIPTOR_INGEST]-[COMPLETE]: the unrostered producers earned their seats in one structural move — `BoardPack` gained its `Wire` column at the kernel, the three port-published packs (`apphost.instrument`, `compute.receipt`, `grasshopper.fan`) mint their keys beside the three that already did, the standalone `Wire` constants collapsed into the pack they name, and `_PACKS` closed at eight rows.
[STORE_TRANSLATION_REPROOF]-[COMPLETE]: re-proved on the landed rule — core's `Convention.translated` renders every selector through the store row's own `translation` value, so the prometheus and mimir suffixing dialects and the victoriametrics identity transform each read their own series off one unchanged query target with no per-row re-derivation owed.
[SECURITY_PACK_ROW]-[COMPLETE]: the `security.audit` seat stands again on a landed producer — `Audit.pack` seals the folder board beside `Audit.alerts` under `Audit.wire`, and the ingest arm tags the compiled board and folds the burn rows exactly as it does for every sibling seat.
[METRICS_READ_SCOPE]-[COMPLETE]: the read plane took the isolation posture its ingest already carried — `_scoped` is the one scope-header projection both ends read, `_headed` folds it into the indexed `httpHeaderNameN`/`httpHeaderValueN` pair the datasource stores, and the `plugin` column lets a row shipping its own query plugin provision that plugin instead of the reference row's.
[DIALECT_ROW_COLLAPSE]-[COMPLETE]: the panel-target dispatch became a table — `_FORMS` is the neutral form vocabulary, `_DIALECTS` carries one mapped row per renderable dialect answering every form in its own alphabet beside its own builder, `_FORMED` keys the tag, and one correlated indexed call replaced the per-dialect switch; loki gained the form column it never had, `_CEILING` took the two display ceilings off the tag rows, and `_refuse` became the one refusal owner.
[RESIDENCE_DOOR_ROW]-[COMPLETE]: each residence publishes its own read door — `_RESIDENCE` answers the shared `_Plane` floor with a `door` projection, `urls.query.residence` replaced the product-named row that projected a service address for a chart the `lake` and `none` selections never install, and `program/provider.md` reads that one door instead of re-branching to build it.
[RESIDENCE_RELATION_CONTRACT]-[COMPLETE]: `Lgtm.residence` publishes the catalog, both planted relations, the partition expression, the tenant sort-key lead, and the evidence horizon as one value, and `_ddl` composes the partition and sort anchors rather than re-spelling them, so a reading branch proves against a stated contract instead of a DDL string.
[PACK_TUPLE_CENSUS]-[COMPLETE]: `_PACKS` censused both ways against real producers and closed at eight seats — every C# `BoardPack` rostered (`apphost.instrument`, `compute.receipt`, `fabrication.slo`, `grasshopper.fan`, `materials.catalogue`, `persistence.census`) beside `runtime.pulse` and `security.audit`, `geometry.charter` culled because its branch mints a measure charter and a fault boundary rather than a projection, and the kernel `BoardPack` gained the provenance key as its first column so a pack cannot exist without the spelling its seat admits.
[RESIDENCE_DATASOURCE_ROW]-[COMPLETE]: the `clickhouse` `_SOURCES` row landed with its `_target` arm, `_FORMS` column, and `_ResidenceQuery` builder transcribing the plugin's `CHSqlQuery`; the family gained a `settings` column every row answers through one `_provisioned` fold both org scopes compose, since that driver dials `host`/`port`/`protocol`/`username`/`defaultDatabase` and ignores the provisioned url outright.
[PULSE_PACK_PROJECTION]-[DROPPED]: dropped with its idea — `_PACKS` ingests core-encoded boards and alerts already, `runtime/otel/meter#BOARD` rules the projection the app's, and its `DashboardModel.Board` input exists only at a composition root Tier-0 seats outside `libs/`.
[BURN_NUMERATOR_RULES]-[COMPLETE]: recorded burn numerators — `_groups` compiles each `Alert.Spec` into a `<slug>:burn:<window>` recording rule off the one `Query.breach` projection, the `_stores` `rules` column carries it in each row's own dialect, and `_expr` reads the recorded series where the row evaluates it and renders inline where it does not.
[FOUNDATION_PANEL_ROWS]-[COMPLETE]: full panel-field compilation — panel and board links, the Geomap `Coords` location layer, Table sort beside footer pagination, and the Timeseries axis, scale, and tooltip rows all land on `operate/observe.md`, each member verified against the installed Foundation SDK and rowed in its catalog.
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
[MIMIR_STORAGE_BINDING]-[COMPLETE]: mimir binding — `_estate` threads the object plane into `Lgtm.Args.objects`, and `structuredConfig.common.storage` + per-section buckets + `compactor_blocks_retention_period` land inside the mimir row's own `values` body.
[SECURITY_PACK_ROW]-[COMPLETE]: security pack ingest — the `security.audit` `_PACKS` row joins decoded boards and alerts into the one compile and `_alerted` folds on `operate/observe.md`.
[ROLL_ANNOTATION_ROW]-[COMPLETE]: roll-annotation source row — `Boards.Args.rolls` maps the AppHost wire onto `oss.Annotation` rows with `_ROLL_TONES` verdict tones on `operate/observe.md`.
[STORE_ROW_VALUES_FOLD]-[COMPLETE]: store rows answer one values body — `retain` and the inline per-store arms collapsed into `_stores.<row>.values`, carrying retention, the translation column in each row's own dialect, exemplar flags, the mimir ruler and disabled bundled minio, and the pinned fullname.
[COLLECTOR_TYPED_PLAN]-[COMPLETE]: the collector config became a decoded owner — `Collector` `Schema.Class` with the component-closure filter, `_plan` folding guard/enrichment/shaping/batch processors, the `spanmetrics` and `servicegraph` connectors, `file_storage` on a PVC with full `sending_queue` and `retry_on_failure` policy, receiver hardening, three-signal self-telemetry, and `_PRUNE` deleting the chart defaults the estate never opened.
[BACKEND_VALUES_ROWS]-[COMPLETE]: Loki, Tempo, and Pyroscope stopped installing on chart defaults — retention, compaction, the Tempo OTLP door, persistence, and the tenancy arm the store row's `tenancy` column governs across every signal.
[PINNED_CHART_FULLNAMES]-[COMPLETE]: `_charts` rows carry a `fullname` projection `row()` folds into every install, `_urls` gained the `collector` row, and the dead `${name}-otel` endpoint that fed every workload's egress and both eBPF exports is gone.
[PG_RECEIVER_SHAPE_AND_CUSTODY]-[COMPLETE]: `Lgtm.Args.dsn` decomposed into discrete `data` coordinates — `postgresqlreceiver` takes `host:port` with separate credential and tls fields, `sqlqueryreceiver` keeps its assembled datasource, both read `${env:PG_PASSWORD}` off one tier Secret, and the exporter Deployment reads its DSN by `secretKeyRef`.
[GRAFANA_POLICY_FIELD]-[COMPLETE]: delivery grouping landed — `NotificationPolicyArgs.groupBies` (required) and the per-route optional carry the `_GROUPED` roster on `operate/observe.md`, route matchers moved off a free-string `severity` onto `Convention.rasm.sloSeverity`, and the rule's owned `Convention` rows moved from annotations to labels so matchers, grouping, and silences read one plane.
[GATEWAY_RESOURCE_PIN]-[COMPLETE]: the collector's self-telemetry resource stamps `service.namespace` — `Convention.wire` became a triple carrying the estate pin, `Convention.identity` projects the dimension unconditionally with the identity's own namespace as the override, and `service.telemetry.resource` reads the constant instead of a tier-local literal.
[COLLECTOR_METRICS_PORT]-[COMPLETE]: the gateway's `metrics` port ships closed — the self-telemetry `readers` list replaces the chart's default pull reader, so `:8888` served nothing while the values row published it.
[COLLECTOR_PRUNE_POSITION]-[COMPLETE]: chart-default tombstones moved inside the decoded document's own maps — the sibling spread beside `_plan` was replacing `receivers` and `exporters` whole and dropping every null it carried, so `jaeger`, `zipkin`, and `debug` survived a prune that read complete; `prometheus` left the roster because both ingest arms define that receiver and a redefined key displaces the default.
[COLLECTOR_CONTRACT_CATALOG]-[COMPLETE]: `libs/typescript/iac/.api/opentelemetry-collector.md` mints the chart-values and config-document contract the collector fences spell, with the fullname rule, the env-expansion rule, and the per-component field rosters.
