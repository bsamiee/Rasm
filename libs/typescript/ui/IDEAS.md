# [TS_UI_IDEAS]

Forward pool of higher-order folder concepts grounded in the interface domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[TONE_VOCABULARY]-[QUEUED]: One semantic tone vocabulary — the token authority declares the closed tone set every surface keys.
- Capability: color semantics are one closed vocabulary at the color authority — every surface `_tone` table keys onto the declared set, so a new tone is one authority row and a surface-local tone union has no place to exist.
- Shape: the closed tone set lands on `libs/typescript/ui/.planning/system/token.md`'s color authority; keying rows follow at `system/primitive.md` (recipe tone axis and `Note`), `viewer/panel.md` phase table, `viewer/mark.md` status table, `viewer/probe.md` verdict tones, and `system/vital.md`'s rating table — whose `caution` token today appears in no sibling union.
- Unlocks: five divergent inline tone unions collapse to keyed reads; the review surface's change-kind tones key onto the same set.
- Anchors: `token.md` OKLCH color authority; the five probed inline unions (`neutral|accent|danger`, `neutral|accent|success|danger`, `success|caution|danger`); `[DIFF_SURFACE]` as the next consumer; the tone-homing row at `libs/typescript/ui/RULINGS.md` `[02]-[SHAPE]`.
- Ripple: precedes [DIFF_SURFACE].

[DIFF_SURFACE]-[QUEUED]: One model-review surface renders the decoded `BimDiff`/`IdsAudit` verdicts across every echo plane.
- Capability: a viewer review plane folding `BimDiff` element changes and `IdsAudit` verdicts into board rows, per-element tone echoes, and selection ops over the one `GlobalId` set.
- Shape: one `.planning/viewer/` design page — change-kind tones keyed onto the token authority's tone set, a diff board riding `view/table` rows, scene and deck tint echoes through `mark`'s op fold, and a camera reveal per changed element.
- Unlocks: the `DiffWire`/`IdsAuditWire` rows the codec registry decodes for `ui` gain their consumer; review workflows land on the selection, echo, and evidence planes the BCF board already rides.
- Anchors: `core/interchange/codec`'s decoded `BimDiff`/`IdsAudit` owners; `viewer/mark`'s `Selection.Op` fold and tone tables; `viewer/scene#DRAW_COLLAPSE`'s `setVisibleAt` and tint rows.
- Ripple: follows [TONE_VOCABULARY].

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
- Capability: draco/ktx2/meshopt loader statics, model-viewer decoder locations, and the perspective engine wasm resolve from ONE identity roster the `[CODEC_IDENTITY_GATE]` gate reads — identity and serving row per asset, refusals keep their `codec-absent` spelling.
- Shape: `libs/typescript/ui/.planning/viewer/scene.md` extends the codec-injection cluster with the roster value; `libs/typescript/ui/ARCHITECTURE.md` boundary row names the iac counterpart serving the roster.
- Unlocks: iac serves the roster as stack rows with content-addressed immutable paths; CSP stays airtight because no loader ever side-loads a foreign CDN; a new codec is one roster row.
- Anchors: `viewer/scene#RESIDENCY_GRAFT` `setDRACOLoader`/`setKTX2Loader`/`setMeshoptDecoder` statics; `viewer/scene#EMBED_ROW` decoder locations; ARCHITECTURE `codec-absent` boundary.

[POINT_CLOUD]-[QUEUED]: Scan and survey point clouds land as first-class geo rows — LAS ingestion, eye-dome shading, BVH picking.
- Capability: LAS/LAZ point clouds stream into deck point layers beside the tile engine, eye-dome lighting renders depth-legible clouds through the effect-compositing pipeline, and BVH-accelerated raycast keeps mark's pick pipes interactive over merged meshes and dense clouds.
- Shape: `libs/typescript/ui/.planning/viewer/geo.md` gains `PointCloudLayer` payload rows fed by `@loaders.gl/las` over the loaders core and an eye-dome `PostProcessEffect` row on the shared context; `libs/typescript/ui/.planning/viewer/mark.md` gains `three-mesh-bvh` accelerated pick rows; `@loaders.gl/3d-tiles` lands as the loader the existing `Tile3DLayer` row already presumes.
- Unlocks: reality-capture review — site scans beside the model on the one selection plane, point-cloud sectioning and measurement; `PostProcessEffect` gains its consumer.
- Anchors: `viewer/geo#LAYER_ROWS` tile engine; `.api/deck.gl-core.md` `PostProcessEffect<ShaderPassT>`; `viewer/scene#INSTANCED_ROWS` mesh pair; admission lane rows for `@loaders.gl/core`, `@loaders.gl/las`, `@loaders.gl/3d-tiles`, `three-mesh-bvh`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HOOK_RAIL]-[COMPLETE]: `.planning/system/hook.md` and six owning endpoints carry the open `Points` seam and runtime rows; veto consultation is payload-selected, adopted sources merge before registration, publish is bounded, and tap faults isolate.
[VITAL_PLANE]-[COMPLETE]: `.planning/system/vital.md` captures five vitals, owns bounded LoAF/event/long-task and id-keyed Profiler windows, publishes every runtime row through the replay hook, composes shipped cutoffs, and folds compiler diagnostics.
[SIGNAL_WEAVE]-[COMPLETE]: observe seams land at their owners — AtomRpc `spanPrefix`, Form's definition-seam preflight/outcome trip, chart pivot span/frame count, and scene's spanned graft metrics with one adopted residency-fact stream.
