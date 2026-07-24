# [APPUI_TASKLOG]

Open and closed work for the product UI engine, distilled from `IDEAS.md`; live-host probes and tool-gated arms track as blocked tasks against the substrate that unblocks them.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[SESSION_ROLE_ROWS]-[QUEUED]: Enumerate the session role and capability rows and the `EditIntent` admission seam.
- Capability: role vocabulary, per-role capability table, membership lifecycle, and the admission-gate fold the merge authority composes — every rejection a registry-derived `AppUiFaultBand.Collab` code.
- Shape: section spine and role/capability tables landing in `libs/csharp/Rasm.AppUi/.planning/Collab/session.md`, with the gate seam pinned against `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `EditIntent`/`IntentLedger`.
- Unlocks: `[COLLAB_SESSION_GOVERNANCE]` lands on a pinned vocabulary instead of inventing one mid-write.
- Anchors: `IDEAS.md` `[COLLAB_SESSION_GOVERNANCE]`; envelope `Tenant` partition.

[SEARCH_QUERY_SHAPE]-[QUEUED]: Define the search query union, source-coverage rows, and the Persistence wire fields.
- Capability: one closed query shape answers cell, media, issue, and evidence searches with ranked source-attributed results routed through the virtual window.
- Shape: query/result union and source rows landing in `libs/csharp/Rasm.AppUi/.planning/Document/search.md`, with the store-side query wire fields stated as the consumed contract.
- Unlocks: `[DOCUMENT_SEARCH]` lands with its Persistence counterpart contract already explicit.
- Anchors: `IDEAS.md` `[DOCUMENT_SEARCH]`; `libs/csharp/Rasm.AppUi/.planning/Editing/livedata.md` data-source axis; `libs/csharp/Rasm.AppUi/.planning/Shell/virtualization.md`.

[SPRING_PARITY_MAP]-[QUEUED]: Map the `SpringValue` members and the host surfaces the preset table binds.
- Capability: `Response`/`DampingFraction`/`Mass` and the `Stiffness`/`Damping` derivations enumerate verbatim beside every host canvas and overlay surface consuming them.
- Shape: parity-map rows landing in `libs/csharp/Rasm.AppUi/.planning/Theme/motion.md` `[02]-[MOTION_AXIS]` boundary clause; AppUi stays the parity source with zero surface change.
- Unlocks: `[HOST_SPRING_PARITY]` lands as a value-copy table with proof coordinates.
- Anchors: `IDEAS.md` `[HOST_SPRING_PARITY]`; `SpringValue` `[ComplexValueObject]`.
- Atomic: one parity-map table.

[SIGNAL_CAPSULE_COMPOSE]-[QUEUED]: Diagnostics spine composes the kernel instrument spec and burn algebra — the folder twins retire.
- Capability: instrument specs, their kind vocabulary, and the SLO burn windows arrive from the kernel capsule; the diagnostics spine keeps only its viewport tile and threshold projections.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]`/`[06]` — the folder spec records and the `SloCoordinate` window constants re-cut over the kernel `InstrumentSpec` and burn carrier.
- Unlocks: a factor or vocabulary change lands once at the kernel; AppUi tiles and Compute rules stop diverging silently.
- Anchors: `libs/csharp/.planning/RULINGS.md` instrument-spec and burn-rate rows; kernel `Domain/telemetry.md`.
- Ripple: follows `Rasm` `[CAPSULE_EXTENSION_MINTS]`; mirrors `Rasm.Compute` `[SIGNAL_CAPSULE_COMPOSE]`.

[MATH_ENGINE_VERIFY]-[QUEUED]: Prove the TeX-subset render through the admitted engine's painter path — the residual half of the math-engine question.
- Capability: the required TeX subset renders through the one shaping rail and the offscreen capsule without a second text pipeline; closure resolution is settled — the engine is admitted and its binding draws into the leased `SKCanvas`.
- Shape: render-proof rows landing in `libs/csharp/Rasm.AppUi/.planning/Document/media.md` `MarkdownInlineRenderer` and `libs/csharp/Rasm.AppUi/.planning/Theme/typography.md` `[05]` `Math` arm.
- Unlocks: `[MATH_TYPESET_OWNER]` lands over a proven painter path.
- Anchors: `IDEAS.md` `[MATH_TYPESET_OWNER]`; `.api/api-csharpmath-skia.md` painter and `LaTeXParser` rows; `.api/api-markdig.md` `UseMathematics`.
- Route: the `LaTeXParser` to painter to headless-encode chain over the engineering-notation sample set, verdicts onto the `.api` catalog; a prerelease bump breaking the `SKCanvas` lease binding re-arms this leg.

[BUNDLE_MANIFEST]-[BLOCKED]: AppUi declares and proves the bundle roster, but AppHost manifest entries omit content identity.
- Capability: `BundleMember` carries classification and `ContentHash.Of` identity, and `ProofLaw.BundleShape` pins the zip roster and extracted tree.
- Shape: AppUi rows are landed in `libs/csharp/Rasm.AppUi/.planning/Document/export.md` `[02]-[EXPORT_DESTINATIONS]` and `Diagnostics/proof.md` `[04]-[PROOF_LAW]`.
- Unlocks: `[SUPPORT_BUNDLE]` lands with host-side manifest entries carrying content identity, the bundle roster verifiable end to end.
- Anchors: `IDEAS.md` `[SUPPORT_BUNDLE]`; `libs/csharp/Rasm.AppUi/.planning/Document/export.md` `[02]-[EXPORT_DESTINATIONS]`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md` `[04]-[PROOF_LAW]`; `BundleMember`/`ContentHash.Of`/`ProofLaw.BundleShape`.
- Tension: arming trigger — add the post-redaction, post-cap content key to `SupportManifest.Entry`, `Written`, and `Faulted` in `libs/csharp/Rasm.AppHost/.planning/Observability/bundles.md`, then pin that field in the manifest golden.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[BOARD_TILE_ROWS]-[COMPLETE]: tile-row table [01]-[10] and the `TelemetryBoard` spine landed in `libs/csharp/Rasm.AppUi/.planning/Charts/telemetry.md` `[02]-[BOARD_ROWS]`.
[LEVEL_KIND_ROW]-[COMPLETE]: `InstrumentKind.Levels` row, keyed kernel `LevelCells` families, and fan-arm swaps landed in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]`.
[WIRE_CONTEXT_FIELDS]-[COMPLETE]: carriage landed as `CollabWireContext`/`CollabFrame` frame metadata in `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `[04]-[LIVE_WIRE]` with the ingest edge re-emitting envelope-carried context unchanged; the `CommitWith` origin-metadata claim was refuted — `CommitOptions` seals only change origin, timestamp, and message.
[PRECOMMIT_TAP]-[COMPLETE]: `SubscribePreCommit` tap and `ExportJsonUpdates` window export landed as `LiveWire.TapPreCommit`/`ExportJson` with the devloop `CollabPreCommit`/`CollabJson` verbs.
[BENCH_MATRIX_ROW]-[COMPLETE]: `BenchLane` derivation and the `FrameBench`/`Divergence` gate landed in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md`, judged through the AppHost `BenchmarkGate` with the dashboards benchmark layout fed off the envelope stream.
[FLAME_SOURCE_PORT]-[COMPLETE]: `ProfileSampleSource` reads the AppHost `UiSchedulerPort.ProfileSamples` feed by correlation, and `FlameNode.Of` prefix-merges its AppHost-owned `ProfileSample` values in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md` `[02]-[DEV_LOOP]`.
[USAGE_RECEIPT_SHAPE]-[COMPLETE]: `TenantUsage` record, envelope-partition fold, and `TenantUsageWire` landed in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[04]-[CORRELATION_JOIN]`.
[SLO_COORD_PIN]-[COMPLETE]: the signal × window `SloCoordinate.Viewport` family with threshold columns and the `Burn` fold landed in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[06]-[TS_PROJECTION]`.
