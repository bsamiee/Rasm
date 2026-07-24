# [PERSISTENCE_IDEAS]

Forward pool of higher-order concepts for the durable-state spine, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[PERS_E2]-[QUEUED]: Project-scoped element-set currency — `ElementSet` membership widened to `(ModelId, NodeId)` so coordination selections span federated models.
- Capability: element-set algebra whose members carry their owning model — cross-model clash sets, whole-project QTO subjects, and discipline-spanning rule selections as one content-addressed currency.
- Shape: a model-qualified membership axis on `Query/lane#ELEMENT_SET_ALGEBRA` `ElementSet` (preimage re-framed over model-qualified keys), evaluation resolving across the roster `ProjectGraph` carries; `Query/topology` gains the multi-graph view the durable `ModelLink` edges anticipate.
- Unlocks: selection and topology answer at the federation altitude the `ModelLink` edge family opened — a duct-penetrates-wall selection spans models as one `SetExpr`, and a project QTO subject is one set.
- Anchors: `Element/graph` `ModelLink`/`LinkKind`/`ProjectGraph`/`ProjectRollup` (landed), the length-framed content-addressed preimage discipline, the one-stream-per-model law.
- Tension: the frozen `elementset` parity vector (`ContentParityCorpus` `ParitySlot.ElementSet`) binds the `NodeId`-only preimage — widening membership re-cuts that parity contract in the same pass.

[PERS_I3]-[BLOCKED]: Reality-capture codec — E57/LAS/LAZ point-cloud ingest into chunked residence with H3 spatial bucketing.
- Capability: the as-built half of the model lifecycle — scan header/metadata rows, registration transform, chunked blob residence, per-region cells — feeding compare-to-design compute without owning scan semantics.
- Shape: one new Ingest codec page at `libs/csharp/Rasm.Persistence/.planning/Ingest/pointcloud.md` under the [A.4] growth row, bytes through Store/blobstore#MULTIPART_TRANSFER + Element/codec#CONTENT_CHUNKING, region cells through Element/identity H3Cell.
- Unlocks: scan-to-BIM verification; the heaviest residence-demanding payload class gains an entry point.
- Anchors: Store/blobstore#CONTENT_CHUNKING (FastCDC), Element/identity H3Cell, Ingest/geospatial and Ingest/issue (the [A.4] codec-page pattern).
- Arms: one answerable question resolved — which managed E57/LAS/LAZ codec package admits under the gate (license, maintenance signal, net10 asset)?
- Route: nuget MCP survey over the E57/LAS candidate family; hand-rolling the E57 XML+binary layout without that ruling is the forbidden alternative.

[PERS_V5]-[BLOCKED]: MQTT egress settles beside the landed AMQP sink.
- Capability: verified message, publish, header, and QoS members compose into `EgressSink.Mqtt`, the sink family carrying both wire dialects on one rail.
- Shape: a settled `EgressSink.Mqtt` fence on `libs/csharp/Rasm.Persistence/.planning/Version/egress.md` `#RESEARCH`.
- Unlocks: device-grade egress rides the same sink family as AMQP, no second egress rail.
- Anchors: `MQTTnet` and `CloudNative.CloudEvents.Mqtt` packages; `Version/egress.md` sink family.
- Arms: both packages gain exact folder-tier `.api` catalogs; until then the MQTT settlement stays research.

[PERS_L1]-[BLOCKED]: Flight SQL serving settles over the landed columnar plane.
- Capability: a transcription-complete `FederationFlightSql` handler surface serves the columnar landing over Flight SQL.
- Shape: a settled `FederationFlightSql` fence on `libs/csharp/Rasm.Persistence/.planning/Query/federation.md` `#RESEARCH`.
- Unlocks: analytical consumers query the durable plane over Flight SQL with no bespoke wire.
- Anchors: `.api/api-arrow.md` `FlightSqlServer` roster; `Query/federation.md` federation owner.
- Arms: `.api/api-arrow.md` gains every exact protected abstract `FlightSqlServer` handler row.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[STORE_OBSERVABILITY]-[COMPLETE]: Engine-stat observability and the receipt-slot registry — landed as `.planning/Store/observability.md` with the `store.<domain>.<verb>` slot grammar, the composition-time registry, and the pg/DuckDB/SQLite harvest receipts.
[PERS_V4]-[COMPLETE]: inbound CDC consume leg landed as `.planning/Version/ingress.md` `CdcIngress` — instrumented consumer twins, envelope-`id` content-key dedup, store-first offsets, `FaultBand.Ingress` 8500.
[PERS_S1]-[COMPLETE]: hook rail landed as `.planning/Store/observability.md#HOOK_RAIL` `PersistenceHooks` — six typed points, `Guarded`/`Swept` composition adapters, per-composition mounts.
[PERS_Q1]-[COMPLETE]: census egress landed as `StoreInstruments.Census` on `.planning/Store/observability.md#STORE_INSTRUMENTS` — rows, bucket hints, mounted slots, projected-arm keys in one wire record.
[PERS_S2]-[COMPLETE]: usage attribution landed as `.planning/Store/observability.md#USAGE_PROJECTION` `StoreUsage.Fold` with the `rasm.persistence.usage.*` gauge rows.
[PERS_S3]-[COMPLETE]: plan-profile rail landed as `.planning/Store/observability.md#PLAN_PROFILE` — three engine legs, shape-only digests, `PlanVerdict` under `store.stat.plan`.
[PERS_S5]-[COMPLETE]: Redis/EF/AWS instrumentation landed as the four settled-composition rows on the `.planning/Store/observability.md` lead and the README registry rows.
[PERS_S6]-[COMPLETE]: desired-state manifest landed as `ClusterProvision.Manifest`/`ProvisionManifest` on `.planning/Store/provisioning.md#SERVER_EXTENSIONS` with `#STORE_AXIS_MAP` axis coordinates on every row.
[PERS_S4]-[COMPLETE]: encrypted embedded floor landed on `.planning/Store/provisioning.md#EMBEDDED_FLOOR` — `bundle_e_sqlite3mc` provider, `raw.sqlite3_key` first crossing, `Rekey` rotation, DEK custody through `Element/identity#KMS_CUSTODY`.
[PERS_Q2]-[COMPLETE]: benchmark corpus landed as `BenchmarkFamily` on `.planning/Query/cache.md#BENCHMARK_INDEX` — six suite rows with subject owners and suite-owned claim keys.
