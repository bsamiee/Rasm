# [APPUI_TASKLOG]

Open and closed work for the product UI engine, distilled from `IDEAS.md`; live-host probes and tool-gated arms track as blocked tasks against the substrate that unblocks them.

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

[BOARD-TILE-ROWS]-[QUEUED]: Enumerate the telemetry-board tile rows and mint the `Charts/telemetry.md` spine.
- Capability: each board tile pins to its exact feed — instrument stream rows, SLO burn-rate tiles, store-profile boards, and the evidence-timeline track — with the `dashboards.md` operator each composes.
- Shape: section spine and tile-row table landing in `libs/csharp/Rasm.AppUi/.planning/Charts/telemetry.md`, feed anchors verified against `libs/csharp/Rasm.AppUi/.planning/Charts/dashboards.md` `[05]-[STREAM_BINDING]` and `[06]-[DASHBOARD_TILES]`.
- Unlocks: the `[TELEMETRY-BOARD-PAGE]` write lands on a pinned structure instead of re-deriving feeds.
- Anchors: `IDEAS.md` `[TELEMETRY-BOARD-PAGE]`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `EvidenceFan` roster.

[LEVEL-KIND-ROW]-[QUEUED]: Pin the `Levels` instrument-kind row and the keyed cell families it reads.
- Capability: one `InstrumentKind` row binds `Meter.CreateObservableGauge` over `Measurement<T>` batches; keyed atom families replace the scalar `QualityRank`/`CollabPending` cells.
- Shape: kind row, cell-family reader, and fan-arm swap edits landing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]`, with the keyed families enumerated (per-doc pending, per-pool VRAM, per-screen disposables).
- Unlocks: `[MULTI-MEASUREMENT-LEVELS]` lands as one spine edit with verified member spellings.
- Anchors: `IDEAS.md` `[MULTI-MEASUREMENT-LEVELS]`; `libs/csharp/.api/api-diagnostics-metrics.md` `Measurement<T>`.
- Atomic: one kind row and cell-family enumeration.

[WIRE-CONTEXT-FIELDS]-[QUEUED]: Pin the traceparent and baggage carriage fields on collab frames and the ingest edge.
- Capability: outbound context rides `CommitWith(CommitOptions)` origin metadata; inbound frames decode context back into `CorrelationId`/`TenantContext` before sealing.
- Shape: frame-metadata field rows landing in `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `[04]-[LIVE_WIRE]` and the `Ingest` decode row in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md`.
- Unlocks: `[COLLAB-WIRE-CONTEXT]` lands with exact member spellings and zero transport reference.
- Anchors: `IDEAS.md` `[COLLAB-WIRE-CONTEXT]`; `libs/csharp/Rasm.AppUi/.api/api-loro.md` `CommitWith`/`ImportWith`.

[PRECOMMIT-TAP]-[QUEUED]: Pin the pre-commit tap and JSON op-window export verbs on the dev loop.
- Capability: `SubscribePreCommit(PreCommitCallback)` seals pre-commit facts as evidence; `ExportJsonUpdates(VersionVector, VersionVector)` exports a version window as readable JSON.
- Shape: two verb rows landing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md` `[02]-[DEV_LOOP]` with the subscription-lifetime and evidence-case columns and the sync-owner anchor in `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md`.
- Unlocks: `[COLLAB-FORENSICS]` lands as two verb rows over verified members.
- Anchors: `IDEAS.md` `[COLLAB-FORENSICS]`; `libs/csharp/Rasm.AppUi/.api/api-loro.md` subscriptions table.
- Atomic: two verb rows over verified members.

[SESSION-ROLE-ROWS]-[QUEUED]: Enumerate the session role and capability rows and the `EditIntent` admission seam.
- Capability: role vocabulary, per-role capability table, membership lifecycle, and the admission-gate fold the merge authority composes — every rejection a registry-derived `AppUiFaultBand.Collab` code.
- Shape: section spine and role/capability tables landing in `libs/csharp/Rasm.AppUi/.planning/Collab/session.md`, with the gate seam pinned against `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `EditIntent`/`IntentLedger`.
- Unlocks: `[COLLAB-SESSION-GOVERNANCE]` lands on a pinned vocabulary instead of inventing one mid-write.
- Anchors: `IDEAS.md` `[COLLAB-SESSION-GOVERNANCE]`; envelope `Tenant` partition.

[SEARCH-QUERY-SHAPE]-[QUEUED]: Define the search query union, source-coverage rows, and the Persistence wire fields.
- Capability: one closed query shape answers cell, media, issue, and evidence searches with ranked source-attributed results routed through the virtual window.
- Shape: query/result union and source rows landing in `libs/csharp/Rasm.AppUi/.planning/Document/search.md`, with the store-side query wire fields stated as the consumed contract.
- Unlocks: `[DOCUMENT-SEARCH]` lands with its Persistence counterpart contract already explicit.
- Anchors: `IDEAS.md` `[DOCUMENT-SEARCH]`; `libs/csharp/Rasm.AppUi/.planning/Editing/livedata.md` data-source axis; `libs/csharp/Rasm.AppUi/.planning/Shell/virtualization.md`.

[BENCH-MATRIX-ROW]-[QUEUED]: Derive the benchmark row family from the proof-matrix catalog and pin the regression gate.
- Capability: benchmark rows derive from the same catalog as proof rows; the gate folds measured-vs-baseline distributions under `UiBuckets.DivergenceRatio` and seals a benchmark receipt per lane.
- Shape: row-family and gate-fold rows landing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md` `[03]-[HEADLESS_DERIVATION]`, with the dashboards benchmark-layout feed anchor in `libs/csharp/Rasm.AppUi/.planning/Charts/dashboards.md`.
- Unlocks: `[FRAME-BENCH-LANE]` lands with baseline custody and gate math settled.
- Anchors: `IDEAS.md` `[FRAME-BENCH-LANE]`; `Avalonia.Headless` frame stepping; AppHost `BenchmarkReceipt` precedent.

[FLAME-SOURCE-PORT]-[QUEUED]: Define the profile-sample source delegate and the `FlameNode` merge fold.
- Capability: a composition-bound sample source delivers correlation-keyed CPU stacks; the fold merges them under the existing frame tree beside solve and pass children.
- Shape: delegate shape and merge-fold rows landing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md` `[02]-[DEV_LOOP]`, sample members mapped from the AppHost capture catalogs.
- Unlocks: `[PROFILE-FLAME-JOIN]` lands with the AppHost feed contract explicit for the counterpart card.
- Anchors: `IDEAS.md` `[PROFILE-FLAME-JOIN]`; `libs/csharp/Rasm.AppHost/.api/api-pyroscope.md`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/governor.md` `[03]-[GPU_TIMELINE]`.

[USAGE-RECEIPT-SHAPE]-[QUEUED]: Define the per-tenant usage receipt fields and the envelope-partition fold.
- Capability: GPU nanoseconds, path-trace samples, render/export bytes, collab deltas, and export pages fold per tenant-window from sealed envelopes into one typed receipt with a wire row.
- Shape: receipt record, fold, and wire rows landing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` beside `[04]-[CORRELATION_JOIN]`.
- Unlocks: `[TENANT-USAGE-FOLD]` lands as one pure fold with the cross-libs baggage join stated.
- Anchors: `IDEAS.md` `[TENANT-USAGE-FOLD]`; envelope `Tenant` field; cross-libs `[COST_ATTRIBUTION_BAGGAGE]`.
- Atomic: one receipt record and one fold.

[BUNDLE-MANIFEST]-[QUEUED]: Enumerate the support-bundle members and the archive proof rows.
- Capability: bundle manifest names every member stream — evidence journal, HUD samples, GPU timelines, verdicts, native-asset facts, golden digests, collab JSON ops — with its serializer and content-hash column.
- Shape: manifest row family on the destination union landing in `libs/csharp/Rasm.AppUi/.planning/Document/export.md` `[02]-[EXPORT_DESTINATIONS]` and `VerifyZip`/`VerifyDirectory` proof rows landing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md`.
- Unlocks: `[SUPPORT-BUNDLE]` lands with bundle completeness provable by construction.
- Anchors: `IDEAS.md` `[SUPPORT-BUNDLE]`; `libs/csharp/Rasm.AppUi/.api/api-verify.md` file-and-archive entrypoints.

[SLO-COORD-PIN]-[QUEUED]: Pin the SLO coordinate rows and the burn-rate fold over the viewport instruments.
- Capability: p99 objectives against `FrameBudget` derive burn-rate rows from `viewport.frame.elapsed`/`viewport.gpu.elapsed` histograms with window and threshold columns.
- Shape: SLO-coordinate projection rows landing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[06]-[TS_PROJECTION]`, instrument names verified against `libs/csharp/Rasm.AppUi/.planning/Render/pipeline.md`.
- Unlocks: `[FRAME-SLO-FEED]` lands on the settled wire with zero new shape.
- Anchors: `IDEAS.md` `[FRAME-SLO-FEED]`; `EvidenceTimelineWire` crossing.
- Atomic: one projection row family.

[SPRING-PARITY-MAP]-[QUEUED]: Map the `SpringValue` members and the host surfaces the preset table binds.
- Capability: `Response`/`DampingFraction`/`Mass` and the `Stiffness`/`Damping` derivations enumerate verbatim beside every host canvas and overlay surface consuming them.
- Shape: parity-map rows landing in `libs/csharp/Rasm.AppUi/.planning/Theme/motion.md` `[02]-[MOTION_AXIS]` boundary clause; AppUi stays the parity source with zero surface change.
- Unlocks: `[HOST-SPRING-PARITY]` lands as a value-copy table with proof coordinates.
- Anchors: `IDEAS.md` `[HOST-SPRING-PARITY]`; `SpringValue` `[ComplexValueObject]`.
- Atomic: one parity-map table.

[MATH-ENGINE-VERIFY]-[BLOCKED]: Verify the `CSharpMath.SkiaSharp` engine against the current SkiaSharp closure before the math arms land.
- Capability: engine admission proves the TeX subset renders through the one shaping rail and the offscreen capsule without a second text pipeline.
- Shape: verified member rows landing in `libs/csharp/Rasm.AppUi/.planning/Document/media.md` `MarkdownInlineRenderer` and `libs/csharp/Rasm.AppUi/.planning/Theme/typography.md` `[05]` `Math` arm after admission.
- Unlocks: `[MATH-TYPESET-OWNER]` lands with a live-verified engine instead of an undecided bet.
- Anchors: `IDEAS.md` `[MATH-TYPESET-OWNER]`; `libs/csharp/Rasm.AppUi/.api/api-markdig.md` `UseMathematics`.
- Tension: blocker question — does `CSharpMath.SkiaSharp` resolve against the admitted SkiaSharp closure and render the required TeX subset? Resolution route: nuget MCP live-verify and the admission lane's install gate and `.api` catalog.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
