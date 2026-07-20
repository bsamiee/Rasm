# [PY_ARTIFACTS_TASKLOG]

`artifacts` open and closed work, distilled from `IDEAS.md`. Open tasks are cards in `[1]-[OPEN]` with a `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` leader; closed tasks move to `[2]-[CLOSED]` with `[COMPLETE]`/`[DROPPED]`. Each task names the exact sub-domain or file it lands in.

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

[HOOKS-PAGE]-[QUEUED]: Author `libs/python/artifacts/.planning/core/hooks.md` — the `rasm.artifacts.<domain>.<point>` HookPoint row table.
- Capability: point rows for issue admitted/planned/refused, front drained, producer emitted, and transmittal issued, each with a closed msgspec payload Struct projected from the owning receipt and one modality.
- Shape: one page carrying the row table, payload Structs, and composition law — the point set registers per app instance, never process-global.
- Unlocks: `[PRODUCTION-HOOKS]` landing surface; cures the `core` stub with real capability.
- Anchors: runtime `observability/hooks.md` `HookPoint`/`Hooks`/`Modality`; runtime `transport/serve.md` `LIFECYCLE_POINTS` row pattern.

[HOOKS-SEAMS]-[QUEUED]: Thread the hook fire seams into `libs/python/artifacts/.planning/core/issue.md` and `libs/python/artifacts/.planning/core/receipt.md`.
- Capability: `_nodes` refusal and `_driven` front fold fire lifecycle facts; the `contribute` fold fires the emitted observe tap.
- Shape: fire rows at the three seams, payload projected from the receipt or fault in hand, no parallel fact construction.
- Unlocks: `[PRODUCTION-HOOKS]` live end to end over the existing spine.
- Anchors: `core/issue.md` `_nodes`/`_driven`/`_cleared`; `core/receipt.md` `contribute`.
- Atomic: three fire seams on two existing pages.

[ATTRIB-BIND]-[QUEUED]: Bind tenant and issue baggage at `ArtifactIssue.issue` and carry it across the lane crossing.
- Capability: `rasm.tenant` and issue-scope entries set once per issue, context carried into every producer render under the W3C composite.
- Shape: bind and carry rows on `libs/python/artifacts/.planning/core/issue.md`, the composite propagator named at the worker crossing.
- Unlocks: `[ISSUE-ATTRIBUTION]` context spine.
- Anchors: `opentelemetry-api` `baggage.set_baggage(name, value, context)`; `CompositePropagator(propagators)`; runtime `LanePolicy` drain.

[ATTRIB-DIMS]-[QUEUED]: Land attribution dimensions on the `_METRIC` projection and structured-log keys in `libs/python/artifacts/.planning/core/receipt.md`.
- Capability: tenant and issue dimensions ride `Metrics.record` and `bind_contextvars` so every numeric fact and error log carries its attribution.
- Shape: dimension rows on `[METRIC_SIGNALS]` and log-key rows on `[SPAN_ERROR]`.
- Unlocks: `[ISSUE-ATTRIBUTION]` billing-grade slicing off the one stream.
- Anchors: `structlog` `bind_contextvars(**kw)`; `core/receipt.md` signals.
- Atomic: dimension and key rows on one existing page.

[NOTICE-PAGE]-[QUEUED]: Author `libs/python/artifacts/.planning/delivery/notice.md` — the `TransmittalNotice` CloudEvents envelope owner.
- Capability: structured JSON binding, distributed-tracing extension, container digest rows, and register row references keyed by the issue content key.
- Shape: one page owning the envelope model, the attribute vocabulary, and the trace-context injection seam.
- Unlocks: `[TRANSMITTAL-NOTICE]` landing surface; deepens the `delivery` stub with real capability.
- Anchors: `cloudevents` bindings and tracing extension; `opentelemetry-api` `propagate.inject(carrier, context, setter)`; `Transmittal`.

[NOTICE-FOLD]-[QUEUED]: Fold the notice as a terminal member of the transmittal closure in `libs/python/artifacts/.planning/delivery/transmittal.md`.
- Capability: the issue closure carries the notice node so the envelope emits with the archive, sign, and register members.
- Shape: one closure row and its parent wiring on the transmittal aggregate.
- Unlocks: `[TRANSMITTAL-NOTICE]` reaches the drain without a second orchestration path.
- Anchors: `Transmittal.emit`; `core/issue.md` `sheet_set` arm.
- Atomic: one closure row on one existing page.

[GATE-PAGE]-[QUEUED]: Author `libs/python/artifacts/.planning/delivery/gate.md` — the `QualityGate` verdict fold and threshold policy.
- Capability: graded verdict union folding measured scores, preflight, conformance, and extraction audits, per-kind thresholds as policy rows.
- Shape: one page owning the fold, the grade vocabulary, and the transmittal refusal seam contract.
- Unlocks: `[QUALITY-GATE]` landing surface; deepens the `delivery` stub with real capability.
- Anchors: `graphic/raster/measure.md`; `document/tagged.md` `UaCheck`; `exchange/conformance.md` `ConformanceVerdict`.

[GATE-LENS-AUDIT]-[QUEUED]: Land the `Page.debug_tablefinder` extraction-audit overlay projection in `libs/python/artifacts/.planning/document/lens.md`.
- Capability: table-extraction QA overlays as a lens examination projection whose audit scalars feed the gate fold.
- Shape: one projection row on the lens examination ops.
- Unlocks: `[QUALITY-GATE]` extraction evidence; exploits the unexploited `pdfplumber` member.
- Anchors: `pdfplumber` `Page.debug_tablefinder`; `document/lens.md` examination ops.
- Atomic: one projection row on one existing page.

[DASH-PAGE]-[QUEUED]: Author `libs/python/artifacts/.planning/visualization/dashboard.md` — the `DashboardPlan` composed-pane owner.
- Capability: chart, table, and diagram panes composed into one offline single-file HTML artifact, theme-graded and register-indexable.
- Shape: one page owning the pane vocabulary, the composition fold, and the single-file emit contract.
- Unlocks: `[DASHBOARD-ARTIFACT]` landing surface; deepens the `visualization` direct tier.
- Anchors: `altair` `JupyterChart`; `TablePlan` HTML arm; `DiagramDraw`; `Theme` rows.

[DASH-HTML-ROW]-[QUEUED]: Land the interactive-HTML format row in `libs/python/artifacts/.planning/visualization/chart/export.md`.
- Capability: self-contained HTML chart output beside the existing raster and vector formats, host-free.
- Shape: one format-dispatch row naming the HTML arm and its embed policy.
- Unlocks: `[DASHBOARD-ARTIFACT]` pane source; exploits the unexploited `altair` member.
- Anchors: `altair` `JupyterChart` and HTML export; `vl-convert-python`.
- Atomic: one format row on one existing page.

[BENCH-PAGE]-[QUEUED]: Author `libs/python/artifacts/.planning/core/bench.md` — the `BenchCorpus` subject rows and threshold policy.
- Capability: per-`ArtifactKind` subjects with deterministic-input recipes and regression thresholds concentrated on the native-offload class.
- Shape: one page owning the corpus rows and policy; timing stays the runtime bench tier's.
- Unlocks: `[PRODUCER-BENCH]` landing surface; cures the `core` stub with real capability.
- Anchors: runtime `observability/profiles.md` `Bench.run`/`BenchmarkReceipt`/`BenchMode`; `media/synthesis.md` deterministic inputs.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
