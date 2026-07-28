# [APPUI_IDEAS]

Forward concept pool for the product UI engine: each open idea is a higher-order folder concept grounded in the AEC/Rhino purpose and current rendering, reality-capture, and coordination research, and each drives one or more `TASKLOG.md` cards.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[COLLAB_SESSION_GOVERNANCE]-[QUEUED]: Typed session governance gates who edits, reviews, and follows inside a live collab session.
- Capability: membership rows, role vocabulary, and capability gating on `EditIntent` admission make authorization a typed fold — an unauthorized intent rejects at the merge authority with a registry-derived fault, and every admission decision seals as evidence.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Collab/session.md` — a fourth Collab page owning the session lifecycle, member/role rows, invitation flow, per-role capability table, and the admission gate the `Collab/sync.md` merge authority composes; deepens Collab past the stub floor.
- Unlocks: multi-tenant review sessions, read-only stakeholder presence, presenter-locked tours, and per-member audit trails on the durable ledger.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `EditIntent`/`IntentLedger`; `libs/csharp/Rasm.AppUi/.planning/Collab/tour.md` presenter-follow presence; `AppUiFaultBand.Collab` stride; envelope `Tenant` partition.

[DOCUMENT_SEARCH]-[QUEUED]: One typed search plane answers queries across notebook cells, media, issues, and evidence.
- Capability: a closed query union with ranked, source-attributed results makes every product document findable — cell source, markdown content, issue titles and comments, and evidence payloads all answer one query shape routed through the virtual window.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Document/search.md` — a fourth Document page owning the query union, source-coverage rows, result projection, and highlight navigation; index custody stays on the Persistence store and AppUi consumes a typed query/result wire as values; deepens Document past the stub floor.
- Unlocks: command-palette document jumps, issue triage by content, notebook provenance lookup, and evidence retrieval by payload term.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Editing/livedata.md` data-source axis; `libs/csharp/Rasm.AppUi/.planning/Shell/virtualization.md` result windowing; `libs/csharp/Rasm.AppUi/.planning/Collab/issues.md` comment lens.
- Tension: demands a Persistence-owned full-text index lane and its query wire; until that counterpart lands the plane covers in-memory sources alone.

[HOST_SPRING_PARITY]-[QUEUED]: Host-side spring presets mirror the Theme/motion `SpringValue` algebra so shell and host viewport motion read as one system.
- Capability: host canvas and viewport overlay motion reads `Response`/`DampingFraction`/`Mass` and the `Stiffness`/`Damping` derivations verbatim from the motion spring rows.
- Shape: an app-root preset table binding the motion vocabulary values per host surface; AppUi stays the parity source with zero AppUi surface change.
- Unlocks: cross-surface motion-consistency proofs comparing host and shell spring envelopes as values.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Theme/motion.md` `[02]` boundary parity clause; `SpringValue` `[ComplexValueObject]`.

[MATH_TYPESET_OWNER]-[QUEUED]: Math layout owns the typography `MarkdownRow.Math`/`InlineRun.Math` arms, so calc reports and notebooks display engineering notation.
- Capability: TeX-subset typesetting rendered Skia-side, composing the one shaping rail and the offscreen capture capsule.
- Shape: the `Math` materialization row on `libs/csharp/Rasm.AppUi/.planning/Document/media.md` `MarkdownInlineRenderer` and the `libs/csharp/Rasm.AppUi/.planning/Theme/typography.md` `[05]` `Math` arm over the admitted `CSharpMath.SkiaSharp` engine; the typography projection stays typeset-free.
- Unlocks: Mathcad-class calc deliverables across notebook cells, media panels, and paginated PDF exports.
- Anchors: the admitted engine (manifest row, lock graph, `.api/api-csharpmath-skia.md` `SKCanvas`-lease charter); `.api/api-markdig.md` `UseMathematics`.
- Tension: the arm lands over the `[MATH_ENGINE_VERIFY]` render proof — the prerelease TeX-subset coverage is the residual bet, not the admission.

[COLLAB_WIRE_CONTEXT]-[BLOCKED]: AppUi frame carriage is landed, but AppHost has no collab carrier adapter.
- Capability: `CollabWireContext` and `CollabFrame` preserve originating correlation and tenant through `CollabSyncReceipt` on the one `IO` merge rail.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `[04]-[LIVE_WIRE]` binds the AppHost-owned injection and extraction delegates without parsing W3C fields locally.
- Unlocks: cross-host correlation and tenant continuity on the live collab merge rail — every `CollabFrame` crossing carries its originating context into `CollabSyncReceipt` without local W3C parsing.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `[04]-[LIVE_WIRE]` `CollabWireContext`/`CollabFrame`/`CollabSyncReceipt`; the AppHost `TraceContext` carrier the injection and extraction delegates bind.
- Tension: arming trigger — land a `CollabWireContext` getter/setter adapter row under `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md` `TraceContext`, then bind that row to `libs/csharp/Rasm.AppHost/.planning/Wire/topics.md` `[COLLAB_DELTA_FEED]`.

[SUPPORT_BUNDLE]-[BLOCKED]: AppUi contributors are content-keyed, but AppHost drops that identity from the archive manifest.
- Capability: `BundleMember` classifies and hashes each settled diagnostic payload, `SupportBundle.Contribute` delivers the roster through the shared destination, and `ProofLaw.BundleShape` pins the archive tree.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Document/export.md` `[02]-[EXPORT_DESTINATIONS]` and `Diagnostics/proof.md` `[04]-[PROOF_LAW]` are landed.
- Unlocks: content-addressed support archives whose every `BundleMember` is verifiable and de-duplicable, the archive tree pinned by `ProofLaw.BundleShape` across host and shell contributors.
- Anchors: `libs/csharp/Rasm.AppUi/.planning/Document/export.md` `[02]-[EXPORT_DESTINATIONS]`; `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md` `[04]-[PROOF_LAW]`; `BundleMember`/`SupportBundle.Contribute`/`ProofLaw.BundleShape`; the AppHost `SupportManifest.Entry` archive manifest.
- Tension: arming trigger — add the post-redaction, post-cap content key to `SupportManifest.Entry` and every `Written`/`Faulted` construction in `libs/csharp/Rasm.AppHost/.planning/Observability/bundles.md`, then include it in the manifest golden.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[PROFILE_FLAME_JOIN]-[COMPLETE]: the AppHost `ProfileCapture.Bind` producer landed at `Observability/benchmarks.md` `[04]-[PROFILE_CORRELATION]`, so `FlameNode.Of` now folds published `ProfileSample` values; the row widened with `ThreadId`, `Kind`, and the `ProfileFrameForm` posture, and `FromSamples` lane-groups by thread with the posture on the lane name.
[TELEMETRY_BOARD_PAGE]-[COMPLETE]: store-profile tiles read the landed `SeriesKind.Telemetry` projection through `StoreProfileTrack`'s `(domain, slot, measure)` facets and one injected read arrow, so every board tile binds a producer.
[MULTI_MEASUREMENT_LEVELS]-[COMPLETE]: keyed level families ride the kernel `InstrumentSpec.Levels` declaration over `LevelCells.Reader`, declared beside each producer and swapped by the `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]` fan arms.
[COLLAB_FORENSICS]-[COMPLETE]: landed as `LiveWire.TapPreCommit`/`ExportJson` with the devloop `CollabPreCommit`/`CollabJson` verbs sealing `PreCommitFact` evidence in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md`.
[FRAME_BENCH_LANE]-[COMPLETE]: landed as `BenchLane`/`ProofEngine.Bench` with `ProofLaw.FrameBench`/`Divergence` gating through the AppHost `BenchmarkGate` in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md`.
[TENANT_USAGE_FOLD]-[COMPLETE]: landed as `TenantUsage`/`TenantUsageFold.Fold` beside `EvidenceJoin` with the `TenantUsageWire` crossing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[04]-[CORRELATION_JOIN]`.
[FRAME_SLO_FEED]-[COMPLETE]: landed as the `ViewportObjectives` latency indicators in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]`, consumed by `SloTiles`/`BurnFeed` over the kernel burn table.
