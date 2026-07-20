# [TS_IAC_IDEAS]

Deploy-plane idea pool: higher-order concepts grounded in spec-total realization, the arm roster, and the operate/kube tiers; an idea drives one or more `TASKLOG.md` tasks, and a resolved idea closes with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[0002]-[ACTIVE]: Receipt-total engine evidence — policy verdicts, run timing, and the deploy benchmark series.
- Capability: `RunReceipt` carries every evidence arm the engine emits — `violations` rows folded from `policyEvent`, engine stdout captured from `stdoutEvent`, and a `timing` band derived from first/last `EngineEvent.timestamp` — so a run's gate verdicts, duration, and mutation profile read from one decoded owner; `Automation.history` correlates receipts against `UpdateSummary` rows into a per-stack, per-op run-duration series, the deploy plane's benchmark evidence.
- Shape: fold rows and receipt fields on `libs/typescript/iac/.planning/program/automation.md`; a violation read joining the drift projection on `libs/typescript/iac/.planning/operate/policy.md` so gating and drift stay one vocabulary.
- Unlocks: deploy-time regression tracking across `history`; policy.md's "violations are receipt material" law becomes true by construction; run budgets tuned from measured durations instead of the fixed default.
- Anchors: `.api/pulumi-pulumi.md` `PolicyEvent`/`StdoutEngineEvent`/`SummaryEvent`/`UpdateSummary` members; `EngineEvent.sequence`/`timestamp`; automation.md's one-pass `_folded` law.

[0003]-[QUEUED]: Fleet-workspace verbs and the platform-contract deploy-host rail.
- Capability: `Automation` closes the engine's workspace surface — `cancel` for a wedged update, one polymorphic `config` verb discriminating get/set/refresh bulk config by input shape, `rename`, workspace roster reads (`whoAmI`, `listStacks`, `installPlugin`), and `fullyQualifiedStackName` as the one identity spelling — while deploy-host processes ride the branch platform contracts: `FileSystem` sinks for drift reports, `KeyValueStore` sweep cursors, `PlatformConfigProvider.layerDotEnv` beneath the `_host` read, `NodeRuntime.runMain` as the one imperative edge.
- Shape: members on `libs/typescript/iac/.planning/program/automation.md`; sink and cursor rows on `libs/typescript/iac/.planning/operate/policy.md`.
- Unlocks: fleet operation from any process with zero raw client calls; deploy-host processes obeying the same Tag/Layer law every branch folder obeys.
- Anchors: `.api/pulumi-pulumi.md` stack-lifecycle and workspace tables; `libs/typescript/.api/effect-platform.md` `FileSystem`/`KeyValueStore`/`PlatformConfigProvider`; `libs/typescript/.api/effect-platform-node.md` `NodeContext.layer`/`NodeRuntime.runMain`.

[0004]-[QUEUED]: Deploy lifecycle hook rail — typed interception rows and one evidence-delivery spine.
- Capability: tiers earn lifecycle interception as data — `ResourceHook`/`ErrorHook` rows named `rasm.iac.<tier>.<point>` ride the `child()` fold, so before/after create-update-delete taps and error transforms are registry rows, never scattered callbacks; every deploy-evidence delivery (run settle, drift verdict, rotation window, Doppler secret-change, hosted `DriftDetected`) speaks one typed sink vocabulary the webhook rows and `Drift.sweep` already share.
- Shape: hook-row law and registry grammar on `libs/typescript/iac/.planning/program/spec.md` (`Tier`); sink vocabulary on `libs/typescript/iac/.planning/operate/policy.md`, referenced by the webhook rows on `libs/typescript/iac/.planning/operate/secret.md` and `libs/typescript/iac/.planning/operate/cloud.md`.
- Unlocks: estate hook-rail parity at the deploy plane — observability subscribes to deploy facts as a tap; a new evidence source is one sink row.
- Anchors: `.api/pulumi-pulumi.md` `ResourceHook`/`ErrorHook`; spec.md's options-algebra law naming the hook binding as a fold row; the two-webhook evidence-delivery law spanning secret.md and cloud.md.

[0005]-[QUEUED]: Postgres logical-estate depth — grants, default ACLs, functions, slots, and label posture.
- Capability: logical finalization grows the full bridged roster: an analyst-tier read-only role through `Grant`/`GrantRole` rows, `DefaultPrivileges` so future objects inherit the ACL, `Function` rows for provisioned stored functions, `ReplicationSlot`/`PhysicalReplicationSlot` beside the `Publication`/`Subscription` pair, `SecurityLabel` as the label-posture carrier, and `getSequences` joining `getTables` in the docker-cell conformance read.
- Shape: docker-arm rows on `libs/typescript/iac/.planning/program/provider.md`; ensure-roster and managed-role equivalents for the k8s arm on `libs/typescript/iac/.planning/kube/data.md`; `conform` widening on `libs/typescript/iac/.planning/operate/policy.md`.
- Unlocks: a grant tier beyond ownership without a second grant surface; replication topology as typed rows on both arms; sequence-level drift evidence.
- Anchors: `.api/pulumi-postgresql.md` resource roster and data-source tables; data.md's ownership-carries-grants law.
- Tension: k8s-arm cells cannot ride the bridged provider (no `.svc` line of sight) — every k8s equivalent lands as ensure-roster SQL or a CNPG managed row, never a bridged resource.

[0006]-[QUEUED]: Prepared-arm invariants — cloud parity rows on the policy pack.
- Capability: `Guard` grows the arm-parity rows the settled realizers now warrant: bucket-durability rows narrowing `aws.s3.BucketV2` and `gcp.storage.Bucket`, an IAM-floor row rejecting wildcard actions, an R2 aging row through `R2BucketLifecycle` on the cloudflare arm, a tenant-fence stack row demanding every tenant namespace carries its `NetworkPolicy`, and `unknownCheckingProxy` guarding preview-unknown reads in every validator.
- Shape: policy rows on `libs/typescript/iac/.planning/operate/policy.md`; lifecycle and bucket-posture rows on `libs/typescript/iac/.planning/program/provider.md` arm bodies.
- Unlocks: prepared arms gated at the same bar as the primary arm; preview-time verdicts that never throw on unresolved outputs.
- Anchors: `.api/pulumi-policy.md` typed helper family and `unknownCheckingProxy`; `.api/pulumi-cloudflare.md` R2 policy family; policy.md's growth law naming exactly these deferred rows.

[0007]-[QUEUED]: Tenant cost-attribution plane — spend evidence as one chart row and compiled boards.
- Capability: the estate prices itself — an OpenCost chart row lands beside the collector inside `Lgtm`, exporting namespace and pod cost series into the selected metrics store; tenant scoping rides the same namespace and `rasm.tenant` vocabulary the tenancy owners already realize, cost boards compile through the standing Foundation-SDK fold into the default and tenant orgs, and the docker arm declares its degrade as a container-stats arm without an allocation feed.
- Shape: chart row, values, and endpoint projection on `libs/typescript/iac/.planning/operate/observe.md`; a `costs` toggle on the `_Observe` profile group at `libs/typescript/iac/.planning/program/spec.md`.
- Unlocks: per-tenant spend dashboards from the same compile leg; scale-row and store-escalation decisions read against measured cost.
- Anchors: observe.md `_charts` growth law (a new backend is one chart row with endpoint projections); `_stores` tenancy columns; `Boards` org-scoped apply.

[0008]-[QUEUED]: Fleet metrics settlement — mimir object-store binding, org federation, and collector depth.
- Capability: the mimir escalation completes — `Lgtm.Args.objects` binds `ruler_storage`/`blocks_storage` to the object plane's endpoint and bucket (one storage truth), `_estate` threads the coordinates, org-id tenancy stamps per-stack scope headers, and collector depth lands: `service.telemetry` self-metrics, an OBI eBPF arm for SDK-less workloads, and CNPG operator-metrics ingest through `prometheusreceiver` — every ingest still entering the one collector door.
- Shape: store-row values and collector rows on `libs/typescript/iac/.planning/operate/observe.md`; `objects` threading on `libs/typescript/iac/.planning/program/provider.md` `_estate`.
- Unlocks: fleet-scale metrics without a second program body; store-swap drills proven by row data; closes the `[STORE_CHARTS]` research row.
- Anchors: observe.md `_stores` mimir row and its empty `structuredConfig` placeholder; observe.md research section; `TASKLOG.md` `[0004]` collector scope.

[0009]-[QUEUED]: Security lease seam realized — LeaseSpec custody cells on the workload estate.
- Capability: the `Security -> Kube` `LeaseSpec` boundary gains its typed realization: `Workload` accepts lease rows decoded from the security plane's `LeaseSpec` shape, each realizing a scoped custody cell — a config-scoped Doppler `ServiceToken` per lease, a namespace `Secret` projection carrying only the leased keys, epoch-keyed rotation, and env rows the entrypoint wrap resolves — so a leased credential's blast radius is the lease, never the whole config.
- Shape: `leases` args and realization rows on `libs/typescript/iac/.planning/kube/workload.md`; access-row wiring on `libs/typescript/iac/.planning/operate/secret.md` `_ACCESS`; tenant lease scoping on `libs/typescript/iac/.planning/kube/tenant.md`.
- Unlocks: per-app lease custody composing without collision across tenants — the app-neutrality law for secrets; the seam-diagram edge stops being the only spelling of the contract.
- Anchors: `ARCHITECTURE.md` seam `[BOUNDARY]: LeaseSpec`; secret.md `_ACCESS` record and token-egress law; workload.md env-assembly laws.
- Tension: `LeaseSpec`'s encoded shape is the security folder's owner — this plane consumes it as data and lands only the deploy realization.

[0010]-[QUEUED]: Board-compile completion — full builder surface, presentation policy, and shared panels.
- Capability: `_compiled`/`_minted` land the remaining model rows — axes/interaction/links/transformations members, Table sort and pagination, Geomap layer mapping, nodegraph frame options, Loki `maxLines` policy — beside presentation rows (`editable`/`readonly`, timepicker, dashboard links), `LibraryPanel` rows so a panel compiled once serves many boards and orgs, and tenant delivery depth: an org-scoped viewer identity and org home preferences per tenant.
- Shape: mint arms, posture rows, and apply rows on `libs/typescript/iac/.planning/operate/observe.md`.
- Unlocks: model-total compilation — a core `DashboardModel` field with no builder member becomes impossible; closes the `[PANEL_OPTIONS]` research row; tenant orgs each carrying read identity and a pinned home board.
- Anchors: `.api/grafana-grafana-foundation-sdk.md` dashboard, panel, and query member tables; `.api/pulumiverse-grafana.md` `oss` roster (`LibraryPanel`, `OrganizationPreferences`, `ServiceAccountPermissionItem`); observe.md research section.

[0011]-[QUEUED]: Security board and alert pack compiles — typed panel and burn-rate alert rows from the security fact plane land through the Foundation-SDK leg.
- Capability: security's declared rows — reject-by-kind, policy-deny-by-reason, KDF latency, JWKS health, rotation age, session-reuse — enter `_compiled` as data; burn-rate alerts on `breached`-class fact rates join the standing SLO burn-rate algebra; tenant rides as the governed grouping dimension into the tenant-org apply loop.
- Shape: a pack-ingest row on the `_compiled`/`_alerted` folds at `libs/typescript/iac/.planning/operate/observe.md` consuming the declaration section security lands on its `access/audit.md`.
- Unlocks: every app composing security inherits the attack-visibility board and alert set from one compile leg; a security row edit moves board and alert together.
- Anchors: observe.md `_compiled` builder fold and `_alerted` contacts fold; core `DashboardModel`/`Alert.Spec` projections; security `Convention.instrument.security*` rows.
- Tension: declaration stays security's — this plane compiles rows it never authors.
- Ripple: `security` `[0005]`.

[0012]-[QUEUED]: Runtime BoardPack census feeds the compile leg — panels and alerts derive from the rows the emitters write.
- Capability: runtime's typed `BoardPack` value — Convention instrument rows, work-plane series, `Vital` budget thresholds — arrives as compile-leg data, so panels, units, thresholds, and burn-rate inputs derive from the emission census and a budget edit moves the emission grade and the board panel in one place.
- Shape: a census-ingest row on the `_compiled` fold at `libs/typescript/iac/.planning/operate/observe.md`, decoding the runtime pack beside the security pack ingest.
- Unlocks: zero-drift dashboards — a new runtime instrument appears on the board by construction; alert thresholds read the same budget rows the emitters grade against.
- Anchors: runtime `otel/meter.md` `Pulse` census projection (carded); runtime `otel/vital.md` `Vital.rows`; observe.md `_compiled` fold.
- Tension: pack shape is runtime's mint — the compile leg decodes, never redefines.
- Ripple: `runtime` `[BOARD_FEED]`.

[0013]-[QUEUED]: Served-asset roster realization — ui decoder and engine wasm identities land as content-addressed serving rows.
- Capability: ui's served-asset identity roster — meshopt/draco/ktx2 decoders and the perspective engine wasm — realizes as `StackSpec` serving rows with content-addressed immutable paths, satisfying the `codec-absent` gate the ui viewer holds until each decoder identity carries its serving row.
- Shape: serving rows on the static-distribution plane at `libs/typescript/iac/.planning/program/source.md` keyed by the ui roster value; the `[05]` transcoder-asset boundary on `libs/typescript/iac/ARCHITECTURE.md` re-rules to name the roster as the one identity source.
- Unlocks: CSP stays airtight with zero foreign-CDN side-loads; a new codec is one roster row and one serving row; the ui gate arms from deploy facts.
- Anchors: ui `viewer/scene.md` codec-injection cluster and roster value (carded); `program/source.md` static distribution; `ARCHITECTURE.md` transcoder-asset boundary.
- Tension: `ARCHITECTURE.md` `[05]` rules transcoder assets shipping with the app shell through runtime asset rows — the serving-row realization either keeps that shipping lane and adds the content-addressed identity or amends the boundary; one ruling, recorded at both seam ends.
- Ripple: `ui` `[ASSET_IDENTITY]`.

[0014]-[QUEUED]: Cross-language descriptor-pack compile roster — sibling-runtime instrument censuses enter the `_compiled`/`_alerted` folds as decoded data.
- Capability: five foreign-minted descriptor families join the compile leg exactly as the security and runtime packs do — the Grasshopper `rasm.grasshopper.*` fan roster with per-document attribution variables and frame-budget alert derivation, the Compute `rasm.compute.*` descriptor with bucket-advice thresholds and the solve non-convergence/remote-call failure/backpressure drop/twin anomaly burn-rate rows, the Fabrication SLO row family over `rasm.fabrication.*`, the Persistence wire census (instrument rows, slot roster, projection-arm keys, threshold hints) over `rasm.persistence.*`, and the Python geometry measure charter (deviation, quality, registration fitness, energy, bench) — each consumed as-is, so board truth never drifts from a mounted roster the compile leg never authors.
- Shape: one pack-decode ingest arm on the `_compiled`/`_alerted` folds at `libs/typescript/iac/.planning/operate/observe.md`, parameterized over the pack's declared schema — one arm, five pack rows, sitting beside the security and runtime ingest rows so all seven ride one mechanic.
- Unlocks: every sibling runtime's boards and burn-rate alerts derive from its own declared truth through one leg; a new descriptor family is one pack row.
- Anchors: observe.md `_compiled` builder fold and `_alerted` burn-rate algebra; `.api/grafana-grafana-foundation-sdk.md` builder members; `[0011]`/`[0012]` as the same-branch pack precedents.
- Tension: pack schemas are the producers' mints — this plane decodes, never redefines; a pack row lands only after its producer card realizes the descriptor.
- Ripple: `csharp:Rasm.Grasshopper` `[0001]`, `csharp:Rasm.Compute` `[DASHBOARD_ALERT_DESCRIPTOR]`, `csharp:Rasm.Fabrication` `[FABRICATION_SLO_PACK]`, `csharp:Rasm.Persistence` `[PERS-Q1]`, `python:geometry` `[DASHBOARD_CHARTER]`.

[0015]-[QUEUED]: Fleet-roll annotation ingestion — `RollAnnotationWire` records join the deploy-annotation rail beside `RunReceipt`.
- Capability: AppHost fleet-roll evidence — wave, channel, verdict, host count, instant — decodes onto the standing deploy-annotation rail, so dashboards mark plugin-fleet rollouts beside stack deploys and a latency shift resolves to the wave that shipped it.
- Shape: one annotation-source row on the `_compiled` annotation fold at `libs/typescript/iac/.planning/operate/observe.md`, decoding the AppHost wire record into the same slug/tone annotation shape `RunReceipt` rows ride.
- Unlocks: deploy-correlated regression triage across the plugin fleet; rollbacks as loud as advances on every board.
- Anchors: observe.md annotation fold (`withVariable`/`annotation` law); the `RunReceipt` deploy-annotation rail as the standing precedent.
- Tension: record shape is the AppHost mint — this plane consumes the wire, never re-derives roll facts.
- Ripple: `csharp:Rasm.AppHost` `[FLEET_DEPLOY_ANNOTATIONS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: metrics-store row family + typed board compile — realized on `operate/observe.md`: `_stores` (`prometheus | mimir | victoriametrics`), Pyroscope row, pg-ingest arm pair, dev row, Foundation-SDK compile leg.
