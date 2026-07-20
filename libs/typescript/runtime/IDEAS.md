# [TS_RUNTIME_IDEAS]

Forward pool of higher-order runtime concepts grounded in the execution-substrate domain and the monorepo purpose. `[1]-[OPEN]` carries live ideas; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

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

[BENCH_CLAIM_PRODUCER]-[QUEUED]: Node-side benchmark runs mint wire-grade claims.
- Capability: measured workload runs fold into the same suite/metrics/host claim shape the C# host mints — quantile folds under `Clock`, a node host-fingerprint mirror of the browser probe's, app-egress delivery through wire encode — so cross-runtime performance comparison rides one admitted claim plane.
- Shape: a measured-run owner on the proc plane — bracketed runs at `Proc.Receipt`-grade timing, warmup and iteration policy as row data, folds to `label`/`value`/`unit` metric rows, host fingerprint minted from process facts; heavy bodies execute off-thread on the worker pool.
- Unlocks: TS engines and work-plane paths gain admissible benchmark evidence on the claim board beside the C# claims; a regression reads as a claim delta.
- Anchors: core `interchange/codec` claim landing with its host-fingerprint admission gate; `proc/exec.md` measured receipts; `proc/worker.md` off-thread pool; ui `viewer/probe` claim-board join.
- Tension: the tests tier owns corpus benchmarking — this owner mints in-product claims, and the two never share a harness.

[PROFILE_SIGNAL]-[QUEUED]: Continuous profiling joins the signal plane as its fourth lane.
- Capability: always-on wall and heap profiles pushed from the node lane, labeled by the estate resource identity and correlated to spans, so a burn alert walks metric to trace to profile in one pane.
- Shape: an `otel/profile.md` owner — `init`/`start`/`stop` lifecycle over `@pyroscope/nodejs`, label bands via `wrapWithLabels` around work-plane workloads, a `Life` drain row joining the telemetry flush, `Setting`-fed backend origin.
- Unlocks: cpu and allocation attribution for the work plane and AI lanes; the iac Pyroscope ingest row gains its runtime producer.
- Anchors: `otel/emit.md` lane law and the rank-90 drain idiom; `proc/config.md` `Setting.otel` admission rows; iac `operate/observe.md` profile ingest row; `.api/pyroscope-nodejs.md`.
- Tension: profile push rides the Pyroscope wire, not OTLP — the lane stays its own owner so the OTLP wire owner never forks.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: work-plane meter bridge — realized as `otel/meter.md` (`Pulse`): fact→instrument projection, census gauges, log-floor wiring, tenant views.
