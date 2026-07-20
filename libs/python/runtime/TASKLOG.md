# [PY_RUNTIME_TASKLOG]

Open and closed work for `runtime`, distilled from `IDEAS.md`. Each task card carries a status marker on its leader — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the design page or `.api/` catalogue to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks. A design-complete idea closes here; the downstream source-transcription mode is outside the planning task pool.

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

[EGRESS_TRANSPORT_TABLE]-[QUEUED]: land the egress-transport axis on the telemetry install.
- Capability: `EgressTransport` vocabulary and per-transport exporter-factory rows on `observability/telemetry#TELEMETRY`, http the standing value on every profile row.
- Anchors: `opentelemetry-exporter-otlp-proto-grpc` beside `opentelemetry-exporter-otlp-proto-http`; the `SIGNAL_SPECS` exporter seam.
- Tension: SIDECAR-only gRPC eligibility; fork-hazard fence on every worker-forking profile; compression vocabularies stay per-transport.
- Atomic: one vocabulary, one `SignalProfile` column, factory rows.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[OBSERVABILITY_PAGES]-[COMPLETE]: landed in `.planning/observability/{logging,hooks,profiles}.md` with the `metrics.md`/`telemetry.md`/`receipts.md` deepening — chain ownership moved to the logging page, `SCHEMA_URL` pin and `resource`/`signal_profile`/`ship` injection seams on the telemetry install, instrument names renamed to `rasm.*`, query/geometry/bench domain rows added, and the instrumentation train catalogued under `.api/`.

[REMOTE_KIND_ROW]-[COMPLETE]: landed in `.planning/execution/workers.md` — `WorkerKind.REMOTE` + `KIND_POLICY` SSH restart row, `WorkerPool` remote arm sealing the kernel over `asyncssh` `create_process`, `remote_floor` entry, Supervisor channel probe; `transport/roots` scope law amended with the `RemoteEndpoint` dial owner.

[SHM_CHANNEL_OWNER]-[COMPLETE]: landed in `.planning/execution/workers.md` — `ShmSpan` + `exported`/`released` on the fabric, decode inside `shipped`, exporter-owned unlink after the offload settles.
