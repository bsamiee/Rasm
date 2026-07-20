# [PY_COMPUTE_IDEAS]

Forward pool of higher-order folder concepts grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

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

[STUDY_BENCH_PROJECTION]-[QUEUED]: study measurements join the runtime bench fabric.
- Capability: `Measured` wallclock and speedup project onto the runtime benchmark-receipt family — one bench contribution per measured design evaluation, subject-keyed per objective.
- Shape: one projection fold beside the `Measured` owner mapping wallclock onto the bench duration measure and the speedup ratio onto a bench fact, riding the runtime bench receipt and `domain="bench"` rows with zero runtime edits under the bench growth law.
- Unlocks: DOE evaluation cost lands on dashboards beside every other bench subject; regression tracking over study runs without receipt post-processing.
- Anchors: `experiments/study#STUDY` `MeasurementMode`/`Measured`; runtime `observability/profiles#BENCH` `BenchmarkReceipt`/`Bench.run`; the `rasm.bench.*` rows on runtime `observability/metrics#METRIC`.
- Tension: solver receipts hold no benchmark authority and graduation admits no Python-only benchmark conclusion — the projection is observability evidence only, never a handoff verdict; `RESULT` mode contributes nothing.

[EVIDENCE_TRACE_LINKS]-[BLOCKED]: `SpanContext` decode on the `GeometryHandoff` wire when geometry lands trace links.
- Capability: the graduation decode admits an optional `SpanContext` beside the `ContentKey` and folds it as a `Link` on the consuming evidence span.
- Shape: one decode arm on the compute side of the `GeometryHandoff` wire and one `Link` fold at the `evidence_run` span open.
- Unlocks: backend trace click-through from compute evidence spans to the upstream geometry producer trace.
- Anchors: the graduation `HandoffAxis` spine; `Link`/`Span.add_link` on the branch `opentelemetry-api` catalogue.
- Tension: the crossing is content-keyed data carrying no trace identity; the widened `wire()` is geometry's frozen-wire change this decode co-ships.
- Ripple: `geometry` `[EVIDENCE_TRACE_LINKS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
