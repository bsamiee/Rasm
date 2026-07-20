# [TS_UI_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

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

[0001]-[QUEUED]: `viewer/review.md` — the model-review surface.
- Capability: decoded `BimDiff` changes and `IdsAudit` verdicts fold into board rows, tone echoes, and selection ops over the one `GlobalId` set; drives from IDEAS `[DIFF_SURFACE]`.
- Shape: new page `.planning/viewer/review.md` — change-kind tone vocabulary, diff board on `view/table` rows, tint echoes through `mark`'s op fold, camera reveal per changed element.
- Anchors: `core/interchange/codec` decoded owners; `viewer/mark` `Selection.Op`; `viewer/scene` tint rows.
- Tension: core's BIM census reconciliation stays blocked on the C# wire owners — this page consumes the spellings that settlement fixes.

[0002]-[QUEUED]: `system/hook.md` — the typed hook registry page.
- Capability: `rasm.ui.<domain>.<point>` rows, veto/observe/replay modality discriminant, subscriber-fault isolation, per-app registry scoping; drives from IDEAS `[HOOK_RAIL]`.
- Shape: new page `libs/typescript/ui/.planning/system/hook.md` — point census over the existing tap sites, registry value shape, modality dispatch, tap-to-bridge law.
- Anchors: `viewer/scene` residency broadcast; `viewer/probe` tap boundary; `viewer/panel` egress records; `viewer/mark` op fold.

[0003]-[QUEUED]: `system/vital.md` — the browser performance-evidence page.
- Capability: `web-vitals` handlers, `PerformanceObserver` long-task/LoAF/event-timing rows, React `Profiler` fold, compiler diagnostics row; drives from IDEAS `[VITAL_PLANE]`.
- Shape: new page `libs/typescript/ui/.planning/system/vital.md` — capture rows as scoped resources, one bounded seed fold per probe's window law, rows shaped as probe's `Metric`.
- Anchors: `viewer/probe#METRIC_FOLD` row shape; `view/chart#SERIES_SURFACE`; `.api/babel-plugin-react-compiler.md`.
- Tension: `web-vitals` rides the admission lane; the page assumes the admission lands.

[0004]-[QUEUED]: Drain probe's open counter research and land the verified rows.
- Capability: resolve `[DECK_TIMERS]` and `[RENDERER_INFO]` — verify the shipped luma metrics payload members and the `WebGPURenderer.info` counter spellings, then land each as one `_Sums` seed field and one `_rows` entry; drives from IDEAS `[VITAL_PLANE]`.
- Shape: verified rows in `libs/typescript/ui/.planning/viewer/probe.md#METRIC_FOLD`; research rows delete on resolution.
- Anchors: `.api/deck.gl-core.md`; `.api/three.md`; `viewer/probe#RESEARCH` routes.
- Atomic: member verifications and their seed rows.

[0005]-[QUEUED]: `view/export.md` — the export-plane page.
- Capability: serializer rows per surface — perspective `to_arrow`/`to_csv`, model-viewer `exportScene`, Plot SVG, grid rows via `tableToIPC`, probe lines — content-minted, port-delivered; drives from IDEAS `[EXPORT_PLANE]`.
- Shape: new page `libs/typescript/ui/.planning/view/export.md` — one owner, serializer rows, mint delegate parameter, download/share port.
- Anchors: `.api/google-model-viewer.md`; `.api/perspective-dev-client.md`; `.api/apache-arrow.md` IPC writers; `system/primitive#CLIPBOARD_PORT` law.

[0006]-[QUEUED]: `system/cache.md` — the content-keyed OPFS persistence page.
- Capability: `ContentKey`-keyed octet store, Schema-coded index, quota and eviction policy rows, per-app directory scoping; drives from IDEAS `[ASSET_CACHE]`.
- Shape: new page `libs/typescript/ui/.planning/system/cache.md` — OPFS scoped resource, residency re-warm seam, upload staging seam.
- Anchors: `viewer/scene#RESIDENCY_GRAFT`; `system/atom#STORE_ROOT` kvs law; `view/form#UPLOAD_LANE`.

[0007]-[QUEUED]: Consolidate the served-asset identity roster in `viewer/scene.md`.
- Capability: one roster value feeding the `setDRACOLoader`/`setKTX2Loader`/`setMeshoptDecoder` paths, model-viewer decoder locations, and the perspective wasm identity; the `[R23]` gate reads the roster; drives from IDEAS `[ASSET_IDENTITY]`.
- Shape: roster rows in `libs/typescript/ui/.planning/viewer/scene.md#RESIDENCY_GRAFT` and `#EMBED_ROW`; `libs/typescript/ui/ARCHITECTURE.md` boundary row naming the iac serving counterpart.
- Anchors: scene codec-injection law; ARCHITECTURE `codec-absent` boundary.
- Atomic: one roster value and its boundary rows.

[0008]-[QUEUED]: Land the point-cloud layer rows in `viewer/geo.md`.
- Capability: `PointCloudLayer` payload rows over `@loaders.gl/las`, an eye-dome `PostProcessEffect` on the shared context, loaders-core registration; `@loaders.gl/3d-tiles` pinned as the existing `Tile3DLayer` row's loader; drives from IDEAS `[POINT_CLOUD]`.
- Shape: rows in `libs/typescript/ui/.planning/viewer/geo.md#LAYER_ROWS` and `#EXTENSION_ROWS`.
- Anchors: geo tile-engine law; `.api/deck.gl-core.md` `PostProcessEffect<ShaderPassT>`.
- Tension: the loaders family rides the admission lane; the rows assume the admission lands.

[0009]-[QUEUED]: Land BVH pick acceleration in `viewer/mark.md`.
- Capability: `three-mesh-bvh` accelerated raycast rows in the pick pipes — BVH built at graft, shared across pick, section, and measure, with the graft fold owning invalidation: a residency mutation or streamed-geometry edit rebuilds or refits the tree (the hierarchy is static per build), and consumers read only the version-stamped structure so spatial queries never go stale; drives from IDEAS `[POINT_CLOUD]`.
- Shape: rows in `libs/typescript/ui/.planning/viewer/mark.md#PICK_PIPES`; BVH build, version stamp, and rebuild/refit seam on `libs/typescript/ui/.planning/viewer/scene.md#RESIDENCY_GRAFT`.
- Anchors: mark pick pipes; scene graft fold; admission lane row for `three-mesh-bvh`.

[0010]-[QUEUED]: Weave the Effect observe seam across the ui effect rails.
- Capability: `withSpan` named rails, `annotateLogs` context, `Metric` counters beside receipt folds; drives from IDEAS `[SIGNAL_WEAVE]`.
- Shape: rows across `libs/typescript/ui/.planning/system/atom.md#REMOTE_BINDING`, `libs/typescript/ui/.planning/view/form.md#SUBMIT_TRIP`, `libs/typescript/ui/.planning/view/chart.md#PIVOT_SURFACE`, `libs/typescript/ui/.planning/viewer/scene.md#RESIDENCY_GRAFT`.
- Anchors: `libs/typescript/.api/effect.md` observe members; altitude law — zero collector import at library tier.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
