# [TS_UI_IDEAS]

Forward pool of higher-order folder concepts grounded in the interface domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

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

[DIFF_SURFACE]-[QUEUED]: One model-review surface renders the decoded `BimDiff`/`IdsAudit` verdicts across every echo plane.
- Capability: a viewer review plane folding `BimDiff` element changes and `IdsAudit` verdicts into board rows, per-element tone echoes, and selection ops over the one `GlobalId` set.
- Shape: one `.planning/viewer/` design page — a change-kind tone vocabulary, a diff board riding `view/table` rows, scene and deck tint echoes through `mark`'s op fold, and a camera reveal per changed element.
- Unlocks: the `DiffWire`/`IdsAuditWire` rows the codec registry decodes for `ui` gain their consumer; review workflows land on the selection, echo, and evidence planes the BCF board already rides.
- Anchors: `core/interchange/codec`'s decoded `BimDiff`/`IdsAudit` owners; `viewer/mark`'s `Selection.Op` fold and tone tables; `viewer/scene#DRAW_COLLAPSE`'s `setVisibleAt` and tint rows.

[HOOK_RAIL]-[QUEUED]: One typed hook registry stands the `rasm.ui.<domain>.<point>` fact rail under every ui plane.
- Capability: typed hook points with veto/observe/replay modalities, subscriber-fault isolation, and telemetry-as-tap — domain facts publish once and observers subscribe, so no emit-call ever scatters into owner code.
- Shape: new page `libs/typescript/ui/.planning/system/hook.md` — point rows minted per owner (`rasm.ui.mark.op`, `rasm.ui.form.submit`, `rasm.ui.scene.residency`, `rasm.ui.panel.egress`, `rasm.ui.overlay.present`), one registry value per app composition (scoped, never process-global, so two apps never contend over points), modality as a row discriminant, subscriber faults isolated onto their own rail without touching the publishing owner.
- Unlocks: scene's residency telemetry lane, probe's app-composed tap, and mark's echo fan graduate from ad-hoc taps to registry rows; the app OTel bridge subscribes hooks instead of wrapping owners; replay modality feeds history and probe evidence from the same rail.
- Anchors: `viewer/scene#RESIDENCY_GRAFT` arrival broadcast; `viewer/probe#METRIC_FOLD` tap boundary; `viewer/mark` `Selection.Op` fold; `viewer/panel#CONTROL_SINKS` egress records; branch hook-rail law of per-package typed registries.

[VITAL_PLANE]-[QUEUED]: One browser performance-evidence owner folds vitals, long tasks, and render profiling into the probe row shape.
- Capability: Core Web Vitals, `PerformanceObserver` long-task/LoAF/event-timing entries, and React `Profiler` commit folds captured as the same `label`/`value`/`unit` metric rows probe boards and chart series already render; the plane mints no instrument — app taps carry rows to the OTel spine.
- Shape: new page `libs/typescript/ui/.planning/system/vital.md` — `web-vitals` handlers (`onLCP`/`onCLS`/`onINP`/`onFCP`/`onTTFB`) as capture rows, observer registrations as scoped resources, one bounded seed fold per probe's window law, compile-time render diagnostics through `runBabelPluginReactCompiler` as the build-lane counterpart row.
- Unlocks: interaction jank, layout shift, and commit-cost evidence land beside the GPU rows probe already captures; one board answers whether cost sits in the render loop or the React tree.
- Anchors: `viewer/probe#METRIC_FOLD` `Metric` row shape and window policy; `view/chart#SERIES_SURFACE` streaming; `.api/babel-plugin-react-compiler.md` diagnostic surface; `web-vitals` through the admission lane.

[EXPORT_PLANE]-[QUEUED]: One export owner folds every surface's native serializer into a content-minted egress plane.
- Capability: charts, pivots, grids, scenes, and evidence rows leave the browser through one typed export surface — serializer rows per surface, octets content-minted, delivery through one download/share port.
- Shape: new page `libs/typescript/ui/.planning/view/export.md` — perspective `View.to_arrow`/`to_csv`, model-viewer `exportScene` Blob, Plot figure SVG serialization, grid and selection rows through `tableToIPC`, probe `Probe.line` text — each a row on one owner; `ContentKey` mint delegate parameterized exactly as `viewer/probe#CAPTURE_FOLD`; delivery per the `system/primitive#CLIPBOARD_PORT` law.
- Unlocks: operator evidence, review boards, and pivot results round-trip to files and journals; `exportScene` gains its consumer.
- Anchors: `.api/google-model-viewer.md` `exportScene`; `.api/perspective-dev-client.md` view serializers; `.api/apache-arrow.md` IPC writers; `viewer/probe#EVIDENCE_ROWS` copy law.

[ASSET_CACHE]-[QUEUED]: Content-keyed OPFS persistence stands the browser residency cache under scene, chart, and form.
- Capability: one storage owner keyed by the kernel `ContentKey` brand — GLB octets survive sessions so residency re-warms without refetch, Arrow frame windows persist, upload staging spills to disk; quota and eviction are policy rows over `StorageManager.estimate`.
- Shape: new page `libs/typescript/ui/.planning/system/cache.md` — OPFS access as a scoped resource, Schema-coded index per atom's kvs law, eviction policy rows, per-app directory scoping so two apps never share a cache root.
- Unlocks: cold-start residency, offline evidence boards, resumable uploads across reloads; perspective `page_to_disk` gains a sibling law instead of a lone comment.
- Anchors: `viewer/scene#RESIDENCY_GRAFT` ledger; `system/atom#STORE_ROOT` `Atom.kvs` schema-coded persistence; `view/form#UPLOAD_LANE` tus sessions; `@effect/platform-browser` `KeyValueStore`.

[ASSET_IDENTITY]-[QUEUED]: One served-asset identity vocabulary unifies every self-hosted wasm and codec path.
- Capability: draco/ktx2/meshopt loader statics, model-viewer decoder locations, and the perspective engine wasm resolve from ONE identity roster the `[R23]` gate reads — identity and serving row per asset, refusals keep their `codec-absent` spelling.
- Shape: `libs/typescript/ui/.planning/viewer/scene.md` extends the codec-injection cluster with the roster value; `libs/typescript/ui/ARCHITECTURE.md` boundary row names the iac counterpart serving the roster.
- Unlocks: iac serves the roster as stack rows with content-addressed immutable paths; CSP stays airtight because no loader ever side-loads a foreign CDN; a new codec is one roster row.
- Anchors: `viewer/scene#RESIDENCY_GRAFT` `setDRACOLoader`/`setKTX2Loader`/`setMeshoptDecoder` statics; `viewer/scene#EMBED_ROW` decoder locations; ARCHITECTURE `codec-absent` boundary.

[POINT_CLOUD]-[QUEUED]: Scan and survey point clouds land as first-class geo rows — LAS ingestion, eye-dome shading, BVH picking.
- Capability: LAS/LAZ point clouds stream into deck point layers beside the tile engine, eye-dome lighting renders depth-legible clouds through the effect-compositing pipeline, and BVH-accelerated raycast keeps mark's pick pipes interactive over merged meshes and dense clouds.
- Shape: `libs/typescript/ui/.planning/viewer/geo.md` gains `PointCloudLayer` payload rows fed by `@loaders.gl/las` over the loaders core and an eye-dome `PostProcessEffect` row on the shared context; `libs/typescript/ui/.planning/viewer/mark.md` gains `three-mesh-bvh` accelerated pick rows; `@loaders.gl/3d-tiles` lands as the loader the existing `Tile3DLayer` row already presumes.
- Unlocks: reality-capture review — site scans beside the model on the one selection plane, point-cloud sectioning and measurement; `PostProcessEffect` gains its consumer.
- Anchors: `viewer/geo#LAYER_ROWS` tile engine; `.api/deck.gl-core.md` `PostProcessEffect<ShaderPassT>`; `viewer/scene#INSTANCED_ROWS` mesh pair; admission lane rows for `@loaders.gl/core`, `@loaders.gl/las`, `@loaders.gl/3d-tiles`, `three-mesh-bvh`.

[SIGNAL_WEAVE]-[QUEUED]: Effect observe seams weave through every ui effect rail with zero collector import.
- Capability: spans, log annotations, and metric counters ride the branch substrate's own combinators on every effect pipeline — remote binding queries, submit trips, residency grafts, capture folds — so an app bridge lights the whole plane without one owner edit.
- Shape: `withSpan` on named rails, `annotateLogs` carrying `GlobalId`/`ContentKey` context, `Metric` counters where receipts fold; rows land across `libs/typescript/ui/.planning/system/atom.md`, `libs/typescript/ui/.planning/view/form.md`, `libs/typescript/ui/.planning/view/chart.md`, `libs/typescript/ui/.planning/viewer/scene.md`.
- Unlocks: browser traces join the estate fabric the moment an app composes the bridge layer; probe and vital local evidence correlates with spine spans by name.
- Anchors: `libs/typescript/.api/effect.md` `withSpan`/`annotateLogs`/`withMetric`; altitude law — library emits Effect signals, never a collector import; `view/form#SUBMIT_TRIP`; `viewer/scene#RESIDENCY_GRAFT`; `system/atom#REMOTE_BINDING`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
