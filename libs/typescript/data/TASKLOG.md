# [TS_DATA_TASKLOG]

Open and closed `data` work distilled from `IDEAS.md`; each task names the exact sub-domain or file it lands in.

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

[0002]-[QUEUED]: Flight SQL lands as an analytical ingress row.
- Capability: `FlightSqlClient` queries remote columnar engines and decodes straight to Arrow tables, joining the analytical matrix as an engine-blind wire row beside the ClickHouse and DuckDB rows.
- Shape: a `lane/olap.md` row — `createFlightSqlClient` construction off lane config, `decodeFlightDataToTable` landing on the same Arrow plane `Olap.ingest` rides.
- Anchors: `.api/qualithm-arrow-flight-client.md`; `lane/olap.md` `_engines` and `Olap.ingest`; the `@connectrpc/connect` transport substrate.
- Tension: the flight row is a read and ingest wire, never a record of truth — journal law holds.

[0003]-[QUEUED]: DuckDB query-profile harvest as a receipt projection.
- Capability: per-query profile receipts harvested through `PRAGMA enable_profiling` json output or `EXPLAIN ANALYZE` — latency, cpu time, rows, operator timings — projected onto Convention duration instruments; receipts stay the truth, instruments the lossy projection.
- Shape: a profile-harvest row on `lane/olap.md` beside `_engines` — profiling keys as row data, one receipt shape spanning the node and wasm arms.
- Anchors: `lane/olap.md` `_engines`; the `journal/fact.md` and `read/batch.md` instrument-row idiom; embedded engines expose no scrape surface, so the harvest is their whole observability.
- Tension: profiling toggles are per-connection state — the row scopes them to the profiled query, never lane-global.

[0004]-[QUEUED]: Engine escalation triggers consume measured probe evidence.
- Capability: lane engine probes — bounded, repeatable measurement runs per `_engines` row — yield claim-shaped receipts whose deltas arm the row's escalation trigger, so an engine escalation is evidence-driven row data.
- Shape: a probe-evidence row beside `_engines` on `lane/olap.md`, receipts in the core claim metric-row shape; the DuckDB profile-harvest receipts supply the measured fields where the engine exposes them.
- Anchors: `lane/olap.md` `_engines` trigger column; core `interchange/codec` claim metric rows; ui `viewer/probe` board join for operator review.
- Tension: probes run beside production lanes — budget and isolation policy bound them to idle windows.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: signal-site conformance — `read/fold.md` checkpoint gauge re-keyed to `Convention.metric.laneCheckpoint` tagged `rasm.lane.name`; `read/batch.md` gained the `rasm.batch.duration` histogram on the timing bracket; `journal/append.md` gained `Journal.census`, the outbox probe the runtime meter bridge samples.
