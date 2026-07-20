# [CROSS_LIBS_TASKLOG]

Open and closed cross-language tasks — the wire seams that span two or more of C# / Python / TypeScript. Per-language and per-folder work lives in the branch and folder `TASKLOG.md`; this node carries only the seam each branch consumes at the boundary, never a re-aggregation of branch work. Each task names its producer and consumer touchpoints in `lang:pkg/page#CLUSTER` notation plus the considerations that scope it; a closed task compacts to one or two lines.

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

[ESTATE_OTLP_BACKEND]-[QUEUED]: A live estate backend receives the three runtimes' OTLP egress.
- Capability: The collector gateway and store set the iac observe page models — Prometheus reference row, log and trace stores, Pyroscope, Grafana — runs as estate infrastructure, so the published OTLP endpoint resolves to a live sink for every runtime.
- Shape: `typescript:iac/operate/observe.md#_stores` realizes the stack; app roots read the endpoint from stack outputs; the dev loop rides the single all-in-one row with byte-identical SDK export config.
- Anchors: `typescript:iac/program/spec.md#_Observe`; `typescript:runtime/otel/emit.md`; `python:runtime/observability/telemetry.md`; `csharp:Rasm.AppHost/Observability/telemetry.md` exporter seam.
- Tension: Container placement lands in the estate host repos, never in libs; the iac package stays host-agnostic and the deployment consumes it.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
