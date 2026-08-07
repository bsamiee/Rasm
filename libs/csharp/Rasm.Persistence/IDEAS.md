# [PERSISTENCE_IDEAS]

Forward pool of higher-order concepts for the durable-state spine, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[POINTCLOUD_CODEC_ADMISSION]-[COMPLETE]: landed as `.planning/Ingest/pointcloud.md` — `ScanSource` over the decompile-verified codec pair (`Aardvark.Data.E57` the E57 leg, `Unofficial.laszip.netstandard` the one LAS/LAZ engine with `.lax` windowed reads), the streaming ingest fold cutting `ChunkPolicy.Artifact` chunks and per-cell `ScanRegion` rows in one pass, `ScanHeader`/`ScanRegistration` residence rows, `FaultBand.Scan` (8520), `ArtifactKind.Scan` → `blob` class, and the Compute `ImportPoints` refusal replaced by `ScanSource` composition with `pts` alone pending.
[CLASS_PREFIXED_OBJECT_NAMES]-[COMPLETE]: landed at `Store/blobstore` — `BlobHandle` minted once at the dispatch layer through `BlobName.Handle` over the class-leading `{class}/{tenant:Text}/{key:x32}` projection (inverse unchanged), every `ObjectLeg` naming slot re-cut to the handle, `LifecycleRules.Project` deriving per-provider expiry and k×AgeBound transition rungs from the declared schedule alone (S3/GCS/Minio armed through decompile-proven members, Azure declared management-plane), `Demote` gated on the `StorageTier.Observed` head read so a lifecycle-realized rung never double-pays, and provider-expiry settlement receipted at the sweep's observation.
[FEDERATED_ELEMENT_SET]-[COMPLETE]: landed as the `SetKey`/`SetScope` widening on `Query/lane#ELEMENT_SET_ALGEBRA` — membership is `(ModelId, NodeId)` under the byte-derived total order, the preimage re-framed (fixed-width big-endian model bytes beside length-framed node text) with the `elementset` parity prose re-cut at `Version/commits` in the same pass, evaluation threading the caller-supplied `SetScope`, `Query/topology` gaining `ProjectView` over the durable `ModelLink` rows, and the cascade (cypher model-stamped vertices, federation scope port, issue `SetKey` correlation, retrieval fusion keys) landed whole under the new `[02]-[SHAPE]` scope ruling.
[UNPARTITIONED_USAGE_SERIES]-[COMPLETE]: the kernel `[OPTIONAL_KEY_LEVEL_FAMILY]` landing discharged the arming condition and the census arm realized the untagged series at `.planning/Store/observability#STORE_INSTRUMENTS` — the partition filter deleted, grouping keys on `Tenant.Key`, the three-measure traverse writes the root group untagged on the same instruments, and the `#STORE_BOARD` tenant panels render one unbroken series on an unpartitioned deployment.
[MQTT_DEVICE_EGRESS]-[COMPLETE]: refuted on disk — the `EgressSink.Mqtt` case, its structured-mode encode, its `V500` `UserProperties` trace stamp, and its reason-code fold are all realized at `.planning/Version/egress#EGRESS_SINK` and both MQTT catalogs are landed, so device-grade egress already rides the one sink rail.
[FLIGHT_SQL_SERVING]-[DROPPED]: Flight SQL SERVING is refuted at `.planning/Query/federation#FLIGHT_RESULT_PLANE` — `FlightSqlServer` dispatches SQL-catalog commands alone and matches `CommandStatementSubstraitPlan` nowhere, so `FederationFlight : FlightServer` carries the plan wire on the same Flight transport at a fraction of the surface; the Flight SQL CLIENT stays composable over any served node, and lake LANDING is `Query/columnar#FLAT_TABLE_EGRESS` `Land`, never a serving door.
[STORE_OBSERVABILITY]-[COMPLETE]: Engine-stat observability and the receipt-slot registry — landed as `.planning/Store/observability.md` with the `store.<domain>.<verb>` slot grammar, the composition-time registry, and the pg/DuckDB/SQLite harvest receipts.
[CDC_CONSUME_LEG]-[COMPLETE]: inbound CDC consume leg landed as `.planning/Version/ingress.md` `CdcIngress` — instrumented consumer twins, envelope-`id` content-key dedup, store-first offsets, `FaultBand.Ingress` 8500.
[PERSISTENCE_HOOK_RAIL]-[COMPLETE]: hook rail landed as `.planning/Store/observability#HOOK_RAIL` `PersistenceHooks` — six typed points, `Guarded`/`Swept` composition adapters, per-composition mounts.
[INSTRUMENT_CENSUS_WIRE]-[COMPLETE]: census egress landed as `StoreInstruments.Census` on `.planning/Store/observability#STORE_INSTRUMENTS` — rows, bucket hints, mounted slots, projected-arm keys in one wire record.
[USAGE_ATTRIBUTION]-[COMPLETE]: usage attribution landed as `.planning/Store/observability#USAGE_PROJECTION` `StoreUsage.Fold` with the `rasm.persistence.usage.*` gauge rows.
[PLAN_PROFILE_RAIL]-[COMPLETE]: plan-profile rail landed as `.planning/Store/observability#PLAN_PROFILE` — three engine legs, shape-only digests, `PlanVerdict` under `store.stat.plan`.
[CLIENT_INSTRUMENTATION_ROWS]-[COMPLETE]: Redis/EF/AWS instrumentation landed as the four settled-composition rows on the `.planning/Store/observability.md` lead and the README registry rows.
[PROVISION_MANIFEST]-[COMPLETE]: desired-state manifest landed as `ClusterProvision.Manifest`/`ProvisionManifest` on `.planning/Store/provisioning#SERVER_EXTENSIONS` with `#STORE_AXIS_MAP` axis coordinates on every row.
[ENCRYPTED_EMBEDDED_FLOOR]-[COMPLETE]: encrypted embedded floor landed on `.planning/Store/provisioning#EMBEDDED_FLOOR` — `bundle_e_sqlite3mc` provider, `raw.sqlite3_key` first crossing, `Rekey` rotation, DEK custody through `Element/identity#KMS_CUSTODY`.
[BENCHMARK_CORPUS]-[COMPLETE]: benchmark corpus landed as `BenchmarkFamily` on `.planning/Query/cache#BENCHMARK_INDEX` — six suite rows with subject owners and suite-owned claim keys.
