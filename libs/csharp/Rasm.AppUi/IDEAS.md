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

[TELEMETRY-BOARD-PAGE]-[QUEUED]: An in-app telemetry board page turns the chart plane into the estate observability product surface.
- Capability: instrument streams, frame-budget SLO burn-rate tiles, Persistence store-profile boards, and an evidence-timeline track all render through the settled dashboards operators — one board page owns the product face of every diagnostics fact the spine already seals.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Charts/telemetry.md` — a fourth Charts page composing `dashboards.md` stream binding, aggregate tiles, and KPI alerts over the `EvidenceFan` instrument roster, the `EvidenceTimeline` join, and a Persistence store-profile receipt feed consumed as values.
- Unlocks: viewport-performance, store-latency, and collab-health boards inside the product with zero new chart surface; deepens Charts past the stub floor with real capability.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Charts/dashboards.md` `[05]-[STREAM_BINDING]` and `[06]-[DASHBOARD_TILES]`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `EvidenceTimeline`; `LiveChartsCore.SkiaSharpView.Avalonia` series rows; Persistence `QueryReceipt.profile` and `store.<domain>.<verb>` instrument receipts as feed values.
- Tension: store-profile receipts need a typed dashboard-ingestion projection on the Persistence side; until that counterpart lands the board renders the AppUi-local roster alone.

[MULTI-MEASUREMENT-LEVELS]-[QUEUED]: A multi-measurement level kind projects tagged level families through one observable callback.
- Capability: `InstrumentKind` gains a `Levels` row binding `Meter.CreateObservableGauge` over `IEnumerable<Measurement<T>>` so one collection-cadence scan reports per-doc collab pending, per-pool VRAM watermarks, and per-screen disposable counts as tagged measurements of one instrument.
- Shape: one kind row and one cell-family reader on the telemetry spine; `UiLevelCells` grows keyed atoms and the fan arms swap keyed entries instead of scalar cells.
- Unlocks: cardinality-correct level telemetry for every keyed family without minting per-key instruments; retires the scalar-cell ceiling on `QualityRank`/`CollabPending`.
- Anchors: `libs/csharp/.api/api-diagnostics-metrics.md` `Measurement<T>` multi-measurement observable callbacks; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]` `InstrumentKind`/`UiLevelCells`.

[COLLAB-WIRE-CONTEXT]-[QUEUED]: W3C trace context and tenant baggage ride every collab frame so cross-client edits correlate on one estate timeline.
- Capability: each `LiveWire` delta frame and each remote evidence ingest frame carries traceparent and `rasm.tenant` baggage as typed frame metadata, so a merge applied on one client joins the originating client's correlation on the evidence timeline.
- Shape: `LoroCs` `CommitWith(CommitOptions)` origin metadata carries the context on outbound commits; the ingest edge in `DevLoop.Ingest` and the `LiveWire` decode read it back into `CorrelationId`/`TenantContext` before sealing — AppUi stays transport-agnostic and the carriage is frame metadata, never a transport reference.
- Unlocks: cross-process collab causality on `EvidenceTimeline`, per-tenant partitioning of remote edits, and the estate cost-attribution join for collaborative sessions.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `[04]-[LIVE_WIRE]`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md` `Ingest`; `libs/csharp/Rasm.AppUi/.api/api-loro.md` `CommitWith(CommitOptions)`/`ImportWith(byte[], origin)`; campaign wire law W3C composite propagation.

[COLLAB-FORENSICS]-[QUEUED]: Live-session forensics expose the CRDT op stream as human-readable evidence on the dev loop.
- Capability: pre-commit observation and JSON op exchange turn an opaque merge dispute into an inspectable operation log — each pending commit surfaces before sealing, and any version window exports as readable JSON for cross-implementation comparison.
- Shape: two devloop verbs over the settled sync owner — a `SubscribePreCommit(PreCommitCallback)` tap sealing pre-commit facts onto the evidence stream, and an `ExportJsonUpdates(VersionVector, VersionVector)` window export feeding the REPL and the support bundle.
- Unlocks: merge-dispute debugging, collab divergence triage across runtimes, and op-level session audit without a second collab surface.
- Anchors: `libs/csharp/Rasm.AppUi/.api/api-loro.md` `SubscribePreCommit`/`ExportJsonUpdates`; `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `[02]-[DOCUMENT_OWNER]`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md` `[02]-[DEV_LOOP]`.

[COLLAB-SESSION-GOVERNANCE]-[QUEUED]: Typed session governance gates who edits, reviews, and follows inside a live collab session.
- Capability: membership rows, role vocabulary, and capability gating on `EditIntent` admission make authorization a typed fold — an unauthorized intent rejects at the merge authority with a registry-derived fault, and every admission decision seals as evidence.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Collab/session.md` — a fourth Collab page owning the session lifecycle, member/role rows, invitation flow, per-role capability table, and the admission gate the `Collab/sync.md` merge authority composes; deepens Collab past the stub floor.
- Unlocks: multi-tenant review sessions, read-only stakeholder presence, presenter-locked tours, and per-member audit trails on the durable ledger.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `EditIntent`/`IntentLedger`; `libs/csharp/Rasm.AppUi/.planning/Collab/tour.md` presenter-follow presence; `AppUiFaultBand.Collab` stride; envelope `Tenant` partition.

[DOCUMENT-SEARCH]-[QUEUED]: One typed search plane answers queries across notebook cells, media, issues, and evidence.
- Capability: a closed query union with ranked, source-attributed results makes every product document findable — cell source, markdown content, issue titles and comments, and evidence payloads all answer one query shape routed through the virtual window.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Document/search.md` — a fourth Document page owning the query union, source-coverage rows, result projection, and highlight navigation; index custody stays on the Persistence store and AppUi consumes a typed query/result wire as values; deepens Document past the stub floor.
- Unlocks: command-palette document jumps, issue triage by content, notebook provenance lookup, and evidence retrieval by payload term.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Editing/livedata.md` data-source axis; `libs/csharp/Rasm.AppUi/.planning/Shell/virtualization.md` result windowing; `libs/csharp/Rasm.AppUi/.planning/Collab/issues.md` comment lens.
- Tension: demands a Persistence-owned full-text index lane and its query wire; until that counterpart lands the plane covers in-memory sources alone.

[FRAME-BENCH-LANE]-[QUEUED]: A headless frame-benchmark lane gates render performance the way goldens gate render identity.
- Capability: the proof matrix grows a benchmark row family measuring frame, pass, and solve timings on the headless backend against pinned baselines, so a frame-time regression fails a proof lane instead of shipping.
- Shape: benchmark rows derive from the same catalog the proof matrix derives from; each run seals a benchmark receipt the dashboards benchmark layouts ingest, and the regression gate is a fold over baseline-vs-measured distributions with the divergence-ratio buckets.
- Unlocks: per-commit viewport performance gating, benchmark boards fed by real lanes, and host-fingerprint-qualified perf claims for the estate benchmark corpus.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md` `[03]-[HEADLESS_DERIVATION]`; `libs/csharp/Rasm.AppUi/.planning/Charts/dashboards.md` benchmark layouts; `Avalonia.Headless` frame stepping; `UiBuckets.DivergenceRatio`; AppHost `BenchmarkReceipt` family as the receipt precedent.

[PROFILE-FLAME-JOIN]-[QUEUED]: Host profile samples join the receipt-derived flamegraph so CPU stacks and receipt durations read as one flame surface.
- Capability: `FlameNode` admits an external profile-sample source beside the solve and pass folds, so a hot CPU stack renders under the same frame tree as GPU pass timings and the span-profile correlation carries through the evidence correlation identity.
- Shape: one composition-bound sample-source delegate on the dev loop — AppHost owns capture (Pyroscope span profiles, EventPipe stacks) and AppUi folds delivered samples into `FlameNode` children keyed by correlation; no profiler reference enters this folder.
- Unlocks: unified CPU/GPU/solve attribution in one overlay, profile-correlated fault triage, and the estate profile-signal join at the product surface.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md` `FlameNode`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/governor.md` `[03]-[GPU_TIMELINE]`; AppHost Pyroscope and EventPipe capture catalogs (`libs/csharp/Rasm.AppHost/.api/api-pyroscope.md`).

[TENANT-USAGE-FOLD]-[QUEUED]: A per-tenant usage fold turns the evidence partition into cost-attribution truth.
- Capability: GPU nanoseconds, path-trace samples, render and export bytes, collab deltas, and export pages fold per `Tenant` from the sealed envelope stream into one typed usage receipt — the UI-plane contribution to estate cost attribution, derived, never re-measured.
- Shape: one pure fold over `ReceiptEnvelope` partitions beside `EvidenceJoin`, emitting a usage receipt row per tenant-window that rides the evidence wire; the cross-libs baggage move carries it to billing.
- Unlocks: per-tenant viewport and export cost accounting, usage boards on the telemetry page, and chargeback evidence with envelope-grade provenance.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` envelope `Tenant` field and `[04]-[CORRELATION_JOIN]` fold form; `rasm.tenant` baggage law; cross-libs `[COST_ATTRIBUTION_BAGGAGE]` card.

[SUPPORT-BUNDLE]-[QUEUED]: One verb assembles the complete diagnostic support bundle and proves its shape.
- Capability: evidence journal, HUD samples, GPU timelines, quality verdicts, native-asset facts, proof-golden digests, and collab JSON ops assemble into one content-hashed archive through the export plane, and the proof lane verifies bundle structure so a support artifact is complete by construction.
- Shape: one bundle manifest row family on the export destination union; assembly is a fold over settled receipt streams, emission rides the settled archive leg, and `VerifyZip`/`VerifyDirectory` rows in the proof matrix pin the bundle tree.
- Unlocks: one-action user bug reports carrying replayable evidence, cross-machine divergence triage from the bundle alone, and analytics egress of diagnostic sessions.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Document/export.md` `[02]-[EXPORT_DESTINATIONS]`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md` capture lanes; `libs/csharp/Rasm.AppUi/.api/api-verify.md` `VerifyZip`/`VerifyDirectory`; kernel `ContentHash.Of` identity.

[FRAME-SLO-FEED]-[QUEUED]: Frame-budget SLO derivation over the viewport instruments feeds the estate dashboard plane.
- Capability: p99 `viewport.frame.elapsed` and `viewport.gpu.elapsed` against `FrameBudget` derive burn-rate SLO rows rendered beside the evidence timeline.
- Shape: an SLO-coordinate projection of the AppUi instrument roster riding the settled `EvidenceTimelineWire` dashboard crossing; AppUi stays the fact producer with zero new wire shape.
- Unlocks: viewport-performance regression pages and alerting derived from the same instruments the quality governor consumes.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` telemetry spine; `libs/csharp/Rasm.AppUi/.planning/Render/pipeline.md` `FrameBudget`; the `EvidenceTimelineWire` crossing.

[HOST-SPRING-PARITY]-[QUEUED]: Host-side spring presets mirror the Theme/motion `SpringValue` algebra so shell and host viewport motion read as one system.
- Capability: host canvas and viewport overlay motion reads `Response`/`DampingFraction`/`Mass` and the `Stiffness`/`Damping` derivations verbatim from the motion spring rows.
- Shape: an app-root preset table binding the motion vocabulary values per host surface; AppUi stays the parity source with zero AppUi surface change.
- Unlocks: cross-surface motion-consistency proofs comparing host and shell spring envelopes as values.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Theme/motion.md` `[02]` boundary parity clause; `SpringValue` `[ComplexValueObject]`.

[MATH-TYPESET-OWNER]-[QUEUED]: A math-layout owner renders the typography `MarkdownRow.Math`/`InlineRun.Math` arms so calc reports and notebooks display engineering notation.
- Capability: TeX-subset typesetting rendered Skia-side, composing the one shaping rail and the offscreen capture capsule.
- Shape: one math-layout package admission — `CSharpMath.SkiaSharp` as the candidate engine — with a materialization row on `libs/csharp/Rasm.AppUi/.planning/Document/media.md` `MarkdownInlineRenderer`; the typography projection stays typeset-free.
- Unlocks: Mathcad-class calc deliverables across notebook cells, media panels, and paginated PDF exports.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Theme/typography.md` `[05]` `Math` arm; `libs/csharp/Rasm.AppUi/.api/api-markdig.md` `UseMathematics`.
- Tension: admission lane must live-verify the `CSharpMath.SkiaSharp` engine against the current SkiaSharp closure before the arm lands.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
