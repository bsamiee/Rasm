# [TS_IAC_TASKLOG]

Deploy-plane work ledger distilled from `IDEAS.md`: each open card leads with a status marker and three to four scoped bullets, and each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0006]-[ACTIVE]: Receipt evidence arms — policy, stdout, and timing rows land on the engine fold.
- Capability: `RunReceipt` gains `violations` (policy name, enforcement level, message, urn per `policyEvent`), `output` stdout lines, and a `timing` struct (`started`/`settled` from first and last `EngineEvent.timestamp`, derived `elapsed`); `_folded` grows the matching arms so the one-pass law holds.
- Shape: schema fields and fold rows in `libs/typescript/iac/.planning/program/automation.md`.
- Anchors: `.api/pulumi-pulumi.md` `PolicyEvent`/`StdoutEngineEvent`/`EngineEvent` rows; `IDEAS.md` `[0002]`.

[0007]-[QUEUED]: Deploy benchmark series — history-correlated run durations.
- Capability: an `Automation.series` projection joins receipt timing with `history` `UpdateSummary` rows into a per-stack, per-op duration series, so deploy regression reads as data over the ledger vocabulary.
- Shape: one member and its projection law on `libs/typescript/iac/.planning/program/automation.md`.
- Anchors: `Automation.history`; `RunReceipt.ops` anchor; `IDEAS.md` `[0002]`.
- Atomic: one member and one projection law.

[0008]-[QUEUED]: Fleet-workspace verbs — cancel, config, rename, roster reads.
- Capability: `cancel` aborts a wedged update, one polymorphic `config` verb discriminates get/set/refresh bulk config by input shape, `rename` moves stack identity, `whoAmI`/`listStacks`/`installPlugin` read the workspace roster, and `fullyQualifiedStackName` is the one identity spelling.
- Shape: members on `libs/typescript/iac/.planning/program/automation.md` over the engine methods the catalog verifies.
- Anchors: `.api/pulumi-pulumi.md` stack-lifecycle table and workspace member row; `IDEAS.md` `[0003]`.

[0009]-[QUEUED]: Platform-contract rail on deploy-host processes.
- Capability: drift-report sinks write through `FileSystem`, sweep cursors persist through `KeyValueStore`, `_host` resolves beneath `PlatformConfigProvider.layerDotEnv`, and the process edge is `NodeRuntime.runMain` — Tags in domain code, Layers at the root.
- Shape: sink and cursor rows on `libs/typescript/iac/.planning/operate/policy.md`; host-read law on `libs/typescript/iac/.planning/program/automation.md`.
- Anchors: `libs/typescript/.api/effect-platform.md` system-API tables; `libs/typescript/.api/effect-platform-node.md` `NodeContext.layer`; `IDEAS.md` `[0003]`.

[0010]-[QUEUED]: Tier hook rows — ResourceHook/ErrorHook interception as registry data.
- Capability: `Tier` gains a `_HOOKS` row family named `rasm.iac.<tier>.<point>` binding `ResourceHook`/`ErrorHook` through the `child()` fold; a tier earning interception states rows, never callbacks at call sites.
- Shape: hook law and row grammar on `libs/typescript/iac/.planning/program/spec.md`.
- Anchors: `.api/pulumi-pulumi.md` `ResourceHook`/`ErrorHook`; spec.md options-algebra law; `IDEAS.md` `[0004]`.

[0011]-[QUEUED]: Evidence-delivery spine — one sink vocabulary across webhook sources.
- Capability: run settle, drift verdicts, rotation windows, Doppler secret-change, and hosted webhook deliveries share one typed sink row shape; a new evidence source is one row.
- Shape: sink vocabulary on `libs/typescript/iac/.planning/operate/policy.md`, referenced from `libs/typescript/iac/.planning/operate/secret.md` and `libs/typescript/iac/.planning/operate/cloud.md`.
- Anchors: `Drift.sweep` sink parameter; `Secrets.webhook`; `CloudPlane` webhook filters; `IDEAS.md` `[0004]`.
- Atomic: one vocabulary row and two reference edits.

[0012]-[QUEUED]: Bridged-roster depth on the docker cell — grants, ACLs, functions.
- Capability: docker-arm finalization grows `Grant`/`GrantRole` analyst rows, `DefaultPrivileges` future-object ACL, and `Function` rows; the k8s arm mirrors each as an ensure-roster statement or managed-role row.
- Shape: arm rows on `libs/typescript/iac/.planning/program/provider.md`; ensure equivalents on `libs/typescript/iac/.planning/kube/data.md`.
- Anchors: `.api/pulumi-postgresql.md` resource roster; data.md ownership-carries-grants law; `IDEAS.md` `[0005]`.

[0013]-[QUEUED]: Replication slots, security labels, and the sequences read.
- Capability: `ReplicationSlot`/`PhysicalReplicationSlot` rows land beside the publication pair, `SecurityLabel` posture rows carry label facts, and `Drift.conform` widens over `getSequences` beside `getTables`.
- Shape: rows on `libs/typescript/iac/.planning/kube/data.md` and `libs/typescript/iac/.planning/operate/policy.md`.
- Anchors: `.api/pulumi-postgresql.md` roster and data-source tables; `IDEAS.md` `[0005]`.
- Atomic: row additions on settled owners.

[0014]-[QUEUED]: Cloud parity policy rows — bucket durability, IAM floor, tenant fences.
- Capability: `bucket-durability` narrows `aws.s3.BucketV2` and `gcp.storage.Bucket` for versioning and retention presence, `iam-floor` rejects wildcard actions on `aws.iam.Policy`, and `tenant-fence` demands per-tenant-namespace `NetworkPolicy` rows when tenancy escalates.
- Shape: rows appended to `_policies` on `libs/typescript/iac/.planning/operate/policy.md`.
- Anchors: `.api/pulumi-policy.md` `validateResourceOfType`/`validateStackResourcesOfType`; policy.md growth law; `IDEAS.md` `[0006]`.

[0015]-[QUEUED]: Preview-unknown guard and the R2 lifecycle row.
- Capability: validators wrap prop reads in `unknownCheckingProxy` so a preview-unknown never throws a verdict; the cloudflare arm's object cell gains `R2BucketLifecycle` aging aligned with the reference-ledger law.
- Shape: validator law on `libs/typescript/iac/.planning/operate/policy.md`; lifecycle row on `libs/typescript/iac/.planning/program/provider.md` cloudflare arm.
- Anchors: `.api/pulumi-policy.md` reporting section; `.api/pulumi-cloudflare.md` R2 family; `IDEAS.md` `[0006]`.
- Atomic: one guard law and one arm row.

[0016]-[QUEUED]: OpenCost chart row and compiled cost boards.
- Capability: an OpenCost chart row lands in `_charts` with values aiming its exporter at the selected store row, cost series scope by namespace and tenant label, cost boards compile through `_compiled` into default and tenant orgs, and the docker arm states its degrade.
- Shape: chart row, values, and board wiring on `libs/typescript/iac/.planning/operate/observe.md`; `costs` profile toggle on `libs/typescript/iac/.planning/program/spec.md`.
- Anchors: `_charts` growth law; `_stores` read paths; `Boards` tenant loop; `IDEAS.md` `[0007]`.

[0017]-[QUEUED]: Mimir binding — object-store coordinates and org federation.
- Capability: `_estate` threads `objects` into `Lgtm`, the mimir row's `ruler_storage`/`blocks_storage` bind endpoint and bucket, retention lands on per-row dialect keys, and the org header scope proves against a two-stack federation read.
- Shape: values rows on `libs/typescript/iac/.planning/operate/observe.md`; threading edit on `libs/typescript/iac/.planning/program/provider.md` `_estate`.
- Anchors: `Lgtm.Args.objects`; observe.md `[STORE_CHARTS]` research row; `IDEAS.md` `[0008]`.

[0004]-[QUEUED]: Collector depth rows — self-telemetry, eBPF arm, operator-metrics ingest.
- Capability: collector internal telemetry lands in the metrics store through its own `service.telemetry` block; an OBI eBPF arm yields zero-code RED metrics for SDK-less workloads; CNPG operator metrics enter through the collector `prometheusreceiver` under the one-ingress law.
- Shape: rows on `libs/typescript/iac/.planning/operate/observe.md` — a telemetry block in the collector config fence, an eBPF chart row beside `pyroscope`, a `_pg`-adjacent receiver row scraping the CNPG pods.
- Anchors: observe.md collector config fence and the `_pg` receiver pair; `libs/typescript/iac/.planning/kube/data.md` `Postgres` tier exposing operator metrics; `IDEAS.md` `[0008]`.
- Tension: OBI demands privileged eBPF host access — the arm binds only where the deploy target grants it.

[0018]-[QUEUED]: Workload lease rows — LeaseSpec custody cells.
- Capability: `Workload.Args.leases` decodes security-plane `LeaseSpec` values into per-lease custody cells: a config-scoped `ServiceToken` row, a leased-keys `Secret` projection, epoch-keyed rotation, and env rows the entrypoint wrap resolves.
- Shape: args and realization on `libs/typescript/iac/.planning/kube/workload.md`; access rows on `libs/typescript/iac/.planning/operate/secret.md`.
- Anchors: `ARCHITECTURE.md` `[BOUNDARY]: LeaseSpec` edge; secret.md `_ACCESS` record; `IDEAS.md` `[0009]`.
- Tension: `LeaseSpec`'s encoded shape is the security folder's owner — this task consumes it as data and lands only the deploy realization.

[0019]-[QUEUED]: Panel-surface completion — remaining builder members and shared panels.
- Capability: `_minted` lands axes, interaction, links, and transformation members, Table sort and pagination, Geomap layers, and nodegraph frame options; Loki targets gain `maxLines`; boards gain presentation rows and `LibraryPanel` reuse.
- Shape: mint arms, `_POSTURE` rows, and apply rows on `libs/typescript/iac/.planning/operate/observe.md`.
- Anchors: `.api/grafana-grafana-foundation-sdk.md` panel and query member tables; observe.md `[PANEL_OPTIONS]` research row; `IDEAS.md` `[0010]`.

[0005]-[QUEUED]: Grafana delivery and tenant-identity depth rows.
- Capability: notification bodies deepen past the generic `SortedPairs` render — per-severity wording, runbook links, and grouping keys arrive as contact-row data on the `MessageTemplate` rows; each tenant org gains its own read identity — an org-scoped `oss.ServiceAccount` viewer with `oss.ServiceAccountPermissionItem` grants and `oss.OrganizationPreferences` pinning the overview board as the org home.
- Shape: rows in the `Boards` apply fold on `libs/typescript/iac/.planning/operate/observe.md` — a template row per contacts entry, a viewer-identity block inside the tenant-org loop.
- Anchors: observe.md `_alerted` contacts fold and the tenant-organization loop; `.api/pulumiverse-grafana.md` alerting and oss member tables; `IDEAS.md` `[0010]`.
- Tension: tenant credentials egress like the automation token — Doppler custody, never stack outputs.

[0020]-[QUEUED]: Security pack ingest rows — panels and burn-rate alerts compile from the declared row section.
- Capability: `_compiled` gains the security pack ingest, `_alerted` gains `breached`-class burn-rate rows grouped by tenant, and boards apply into the default and tenant orgs.
- Shape: ingest rows on `libs/typescript/iac/.planning/operate/observe.md`.
- Anchors: `_compiled`/`_alerted` folds; security `access/audit.md` declaration section (carded); `IDEAS.md` `[0011]`.
- Atomic: ingest rows on one page.

[0021]-[QUEUED]: BoardPack ingest row — the compile fold decodes the runtime census pack.
- Capability: `_compiled` decodes `BoardPack` into panel and alert mints beside the hand-declared packs; unit and threshold columns map onto builder members.
- Shape: one ingest row on `libs/typescript/iac/.planning/operate/observe.md`.
- Anchors: runtime `otel/meter.md` census projection (carded); `IDEAS.md` `[0012]`.
- Atomic: one ingest row.

[0022]-[QUEUED]: Asset serving rows and the boundary ruling — roster-keyed static distribution.
- Capability: serving rows decode the ui roster into content-addressed immutable paths on the static plane; the `[05]` transcoder-asset boundary re-rules to name the roster as the one identity source.
- Shape: rows on `libs/typescript/iac/.planning/program/source.md`; boundary edit on `libs/typescript/iac/ARCHITECTURE.md`.
- Anchors: ui roster value (carded); `program/source.md` static distribution; `IDEAS.md` `[0013]`.
- Atomic: serving rows and one boundary ruling.

[0023]-[QUEUED]: Pack-decode ingest arm — one parameterized decoder serves the five cross-language descriptor packs.
- Capability: the `_compiled`/`_alerted` folds gain one schema-parameterized pack decoder with five pack rows — Grasshopper fan roster, Compute descriptor, Fabrication SLO family, Persistence census, geometry charter — each row naming its producer wire and alert derivation.
- Shape: decoder arm and pack rows on `libs/typescript/iac/.planning/operate/observe.md` beside the security and runtime ingest rows.
- Anchors: `IDEAS.md` `[0014]`; the `_alerted` burn-rate algebra; producer descriptor cards at their folders.

[0024]-[QUEUED]: Roll-annotation source row — the AppHost wire record decodes onto the annotation fold.
- Capability: `RollAnnotationWire` wave/channel/verdict fields map to the slug/tone annotation shape; rollbacks carry a distinct tone row.
- Shape: one source row on the annotation fold at `libs/typescript/iac/.planning/operate/observe.md`.
- Anchors: `IDEAS.md` `[0015]`; the `RunReceipt` annotation precedent.
- Atomic: one annotation-source row.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: `operate/observe.md` deepened — store family with tenancy/retention/degradation columns, `_pg` ingest arms, `_DEV` row, `_compiled` builder fold, `MessageTemplate`/`MuteTiming`/`Annotation` rows; `spec.md` gained the `observe` profile group.
- [0002]-[COMPLETE]: `@grafana/grafana-foundation-sdk` admitted — README row and `.api/grafana-grafana-foundation-sdk.md`; deploy-host self-telemetry law on `program/automation.md`.
- [0003]-[COMPLETE]: observe realize pass — `_compiled` emission-total (`time`/`annotation`/`withVariable` folds, `description`/`transparent`/`legendFormat`, sorted threshold steps over the `-Infinity` base), `slo.SLO` respell, Editor-role identity with `FolderPermissionItem` grant and `ServiceAccountRotatingToken`, org-scoped tenant fleets, `Dev` tier wired into the docker arm with `Boards` over one URL plane.
