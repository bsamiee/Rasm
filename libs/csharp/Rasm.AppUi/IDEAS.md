# [APPUI_IDEAS]

Forward concept pool for the product UI engine: each open idea is a higher-order folder concept grounded in the AEC/Rhino purpose and current rendering, reality-capture, and coordination research, and each drives one or more `TASKLOG.md` cards.

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

[FRAME-SLO-FEED]-[QUEUED]: Frame-budget SLO derivation over the viewport instruments feeds the estate dashboard plane.
- Capability: p99 `viewport.frame.elapsed` and `viewport.gpu.elapsed` against `FrameBudget` derive burn-rate SLO rows rendered beside the evidence timeline.
- Shape: an SLO-coordinate projection of the AppUi instrument roster riding the settled `EvidenceTimelineWire` dashboard crossing; AppUi stays the fact producer with zero new wire shape.
- Unlocks: viewport-performance regression pages and alerting derived from the same instruments the quality governor consumes.
- Anchors: `.planning/Diagnostics/evidence.md` telemetry spine; `.planning/Render/pipeline.md` `FrameBudget`; the `EvidenceTimelineWire` crossing.

[HOST-SPRING-PARITY]-[QUEUED]: Host-side spring presets mirror the Theme/motion `SpringValue` algebra so shell and host viewport motion read as one system.
- Capability: host canvas and viewport overlay motion reads `Response`/`DampingFraction`/`Mass` and the `Stiffness`/`Damping` derivations verbatim from the motion spring rows.
- Shape: an app-root preset table binding the motion vocabulary values per host surface; AppUi stays the parity source with zero AppUi surface change.
- Unlocks: cross-surface motion-consistency proofs comparing host and shell spring envelopes as values.
- Anchors: `.planning/Theme/motion.md` [02] boundary parity clause; `SpringValue` `[ComplexValueObject]`.

[MATH-TYPESET-OWNER]-[QUEUED]: A math-layout owner renders the typography `MarkdownRow.Math`/`InlineRun.Math` arms so calc reports and notebooks display engineering notation.
- Capability: TeX-subset typesetting rendered Skia-side, composing the one shaping rail and the offscreen capture capsule.
- Shape: one math-layout package admission with a materialization row on `.planning/Document/media.md` `MarkdownInlineRenderer`; the typography projection stays typeset-free.
- Unlocks: Mathcad-class calc deliverables across notebook cells, media panels, and paginated PDF exports.
- Anchors: `.planning/Theme/typography.md` [05] `Math` arm; `.api/api-markdig.md` `UseMathematics`.
- Tension: demands a new package admission for math layout; engine selection and channel undecided.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
