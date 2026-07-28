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

[FEDERATED_ELEMENT_SET]-[QUEUED]: Project-scoped element-set currency — `ElementSet` membership widened to `(ModelId, NodeId)` so coordination selections span federated models.
- Capability: element-set algebra whose members carry their owning model — cross-model clash sets, whole-project QTO subjects, and discipline-spanning rule selections as one content-addressed currency.
- Shape: a model-qualified membership axis on `Query/lane#ELEMENT_SET_ALGEBRA` `ElementSet` (preimage re-framed over model-qualified keys), evaluation resolving across the roster `ProjectGraph` carries; `Query/topology` gains the multi-graph view the durable `ModelLink` edges anticipate.
- Unlocks: selection and topology answer at the federation altitude the `ModelLink` edge family opened — a duct-penetrates-wall selection spans models as one `SetExpr`, and a project QTO subject is one set.
- Anchors: `Element/graph` `ModelLink`/`LinkKind`/`ProjectGraph`/`ProjectRollup` (landed), the length-framed content-addressed preimage discipline, the one-stream-per-model law.
- Tension: the frozen `elementset` parity vector (`ContentParityCorpus` `ParitySlot.ElementSet`) binds the `NodeId`-only preimage — widening membership re-cuts that parity contract in the same pass.

[POINTCLOUD_CODEC_ADMISSION]-[BLOCKED]: Reality-capture codec — E57/LAS/LAZ point-cloud ingest into chunked residence with H3 spatial bucketing.
- Capability: the as-built half of the model lifecycle — scan header/metadata rows, registration transform, chunked blob residence, per-region cells — feeding compare-to-design compute without owning scan semantics.
- Shape: one new Ingest codec page at `libs/csharp/Rasm.Persistence/.planning/Ingest/pointcloud.md` under the [A.4] growth row, bytes through Store/blobstore#MULTIPART_TRANSFER + Element/codec#CONTENT_CHUNKING, region cells through Element/identity H3Cell.
- Unlocks: scan-to-BIM verification; the heaviest residence-demanding payload class gains an entry point.
- Anchors: Element/codec#CONTENT_CHUNKING (FastCDC), Element/identity H3Cell, Ingest/geospatial and Ingest/issue (the [A.4] codec-page pattern).
- Arms: one answerable question resolved — which managed E57/LAS/LAZ codec package admits under the gate (license, maintenance signal, net10 asset)?
- Route: nuget MCP survey over the E57/LAS candidate family; hand-rolling the E57 XML+binary layout without that ruling is the forbidden alternative.

[UNPARTITIONED_USAGE_SERIES]-[BLOCKED]: Unpartitioned usage census reports its own series — the per-tenant usage levels project untagged when no tenant partitions.
- Capability: usage attribution answers under both tenancy modes from one instrument roster — a partitioned store reads per-tenant keyed levels and an unpartitioned store reads the same three measures untagged, with no sentinel dimension and no second instrument name.
- Shape: the three usage rows beside the census arm on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md` `#STORE_INSTRUMENTS`.
- Unlocks: an unpartitioned deployment's storage-bytes, object-count, and delivery tiles render off the same census the multi-tenant board reads.
- Anchors: the kernel `TenantContext.Partitions`/`Tags` absent-tenant arm; the `Levels` kind beside its keyed `LevelCells` reader; the `StoreUsage.Tenancy` lift every raw partition key already crosses.
- Arms: the kernel keyed cell entry admits an absent key and the `Levels` bind arm projects an untagged `Measurement<T>` for it; until then the census arm mounts entries for a partitioning tenant alone.
- Ripple: follows `Rasm` `[OPTIONAL_KEY_LEVEL_FAMILY]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

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
