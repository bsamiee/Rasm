# [TS_UI_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

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

[REVIEW_SURFACE_PAGE]-[QUEUED]: `viewer/review.md` — the model-review surface.
- Capability: decoded `BimDiff` changes and `IdsAudit` verdicts fold into board rows, tone echoes, and selection ops over the one `GlobalId` set; drives from IDEAS `[DIFF_SURFACE]`.
- Shape: new page `.planning/viewer/review.md` — change-kind tones keyed onto the token authority's tone set, diff board on `view/table` rows, tint echoes through `mark`'s op fold, camera reveal per changed element.
- Unlocks: IDEAS.md [DIFF_SURFACE] — the `DiffWire`/`IdsAuditWire` rows the codec registry decodes gain their consumer, review workflows landing on the selection, echo, and evidence planes.
- Anchors: `core/interchange/codec` decoded owners; `viewer/mark` `Selection.Op`; `viewer/scene` tint rows.
- Tension: core's BIM census reconciliation stays blocked on the C# wire owners — this page consumes the spellings that settlement fixes.
- Ripple: follows IDEAS `[TONE_VOCABULARY]`.

[VIEWER_TIME_AUTHORITY]-[QUEUED]: One viewer time source — mixer deltas and the rAF atom clock read the same authority.
- Capability: the GLB mixer advance and the deck/atom animation clock derive from one time authority, so construction-sequence scrub across scene and geo reconciles by construction and the one-clock law both pages assert is structural.
- Shape: the `_loop` `new Clock()` mint in `libs/typescript/ui/.planning/viewer/scene.md` folds onto the rAF-fed atom clock `geo.md`'s trips row and scene's `_animations` already ride; delta derivation feeds `Glb.Loop.advance`.
- Unlocks: scrub and replay drive every animated surface from one coordinate; scene's one-clock law stops contradicting its own fence.
- Anchors: `scene.md` `_loop` (`new Clock()`); `scene.md`/`geo.md` one-animation-clock laws; `Glb.Loop.advance(delta)`; the one-time-authority row at `libs/typescript/ui/RULINGS.md` `[02]-[SHAPE]`.
- Atomic: one clock-source fold.

[CAMERA_INTENT_SEAM]-[QUEUED]: Gesture camera writes speak the viewer's intent vocabulary across the strata boundary.
- Capability: one camera vocabulary — the gesture recognizer's writes and the viewer's camera authority reconcile: the floor emits intent-shaped values, or a minimal camera-state shape homes at the floor, replacing the local `Camera` type that spells `center` differently and omits `pitch`.
- Shape: the `Gesture` namespace `Camera` type in `libs/typescript/ui/.planning/system/act.md` re-shapes to the seam contract; `geo.md`'s law naming the gesture owner an intent producer aligns with what the floor actually writes.
- Unlocks: gesture-driven camera motion replays through the one intent write path; no un-reconcilable shape pair spans the strata boundary.
- Anchors: `act.md` `Gesture` `Camera` (`center: Vector2`, no `pitch`); `geo.md` `Camera.State` (`[lng, lat]` tuple, `pitch`) and its intents-only write-path law; `act.md`'s boundary line already deferring the shape to the viewer plane.

[EVIDENCE_WINDOW_ALGEBRA]-[QUEUED]: One bounded-window measure algebra — the floor owns it, the evidence boards consume.
- Capability: the bounded-window sample fold with its count/mean/peak projections is one floor-owned algebra, so a new statistic is one row for every evidence surface instead of a per-page re-derivation.
- Shape: the algebra homes at `libs/typescript/ui/.planning/system/vital.md` as the floor owner; `viewer/probe.md`'s measure and projection tables key onto it.
- Unlocks: evidence surfaces share one window law; the `_WINDOW` cap homing lands into an owned algebra, never a lone constant.
- Anchors: `probe.md` `_PROJECTION` and measure-table rows; `vital.md` `_entryRows`/`_ENTRY_SEED` seed folds and `Chunk.takeRight` window law.
- Ripple: mirrors [FLOOR_TWIN_SWEEP].

[NOTE_SURFACE_DEPTH]-[QUEUED]: Toast notes carry their full domain — action, priority, dismissal, and announce assertiveness as fields.
- Capability: a note is the whole notification concept — action affordance, priority, dismiss policy, and the tone-to-assertiveness mapping the announce rail reads — so surfaces stop encoding urgency at ad-hoc call sites.
- Shape: the `Note` shape and toast queue rows in `libs/typescript/ui/.planning/system/primitive.md` widen; the live-region announce mapping keys tone onto assertive/polite.
- Unlocks: every fault-to-toast fold carries an actionable repair affordance; screen-reader severity derives from data.
- Anchors: `primitive.md` `Note` three-field shape, toast queue, and live-region announce rows.

[GRANT_STATUS_SUBSCRIPTION]-[QUEUED]: Permission grants subscribe — mid-session revocation becomes visible.
- Capability: permission state is a subscribed fact, never a boot-time read — the port carries status-change subscription so a revoked grant degrades the consuming surface live, matching the subscription posture every other host capability rides.
- Shape: a status-subscription member on the `Grant` port in `libs/typescript/ui/.planning/viewer/geo.md`, riding the port's floor re-homing.
- Unlocks: geolocation and clipboard surfaces degrade at the revocation instant; no stale-grant render exists.
- Anchors: `geo.md` `Grant.query`; the platform `PermissionStatus` change surface; the port split [FLOOR_TWIN_SWEEP] lands.
- Ripple: follows [FLOOR_TWIN_SWEEP].

[FLOOR_TWIN_SWEEP]-[QUEUED]: Floor twins resolve — the sample window, the pulse motion, and the permissions port each get one owner.
- Capability: the shared sampling cap has one floor owner both evidence planes read; the panel's `pulse` motion is a row in the closed motion vocabulary or remaps onto one; the generic permissions port homes at the floor so its clipboard arm serves the floor consumer downward.
- Shape: `_WINDOW` homes once with `libs/typescript/ui/.planning/system/vital.md` as floor owner and `viewer/probe.md` reading it; `"pulse"` reconciles against `system/act.md`'s `Motion` rows in `viewer/panel.md`; `viewer/geo.md`'s `Grant` port splits its `clipboard-read` arm toward `system/primitive.md`'s `Clipboard` port.
- Unlocks: zero duplicate policy constants across strata; the motion and permission vocabularies close.
- Anchors: `vital.md` and `probe.md` twin `_WINDOW = { samples: 120 }`; `panel.md` `motion: Option.some("pulse")`; `act.md` `Motion` kind table; `geo.md` `Grant` `"clipboard-read"`; `primitive.md` `Clipboard` Tag.
- Atomic: three small reconciliations.

[EXPORT_PLANE_PAGE]-[QUEUED]: `view/export.md` — the export-plane page.
- Capability: serializer rows per surface — perspective `to_arrow`/`to_csv`, model-viewer `exportScene`, Plot SVG, grid rows via `tableToIPC`, probe lines — content-minted, port-delivered; drives from IDEAS `[EXPORT_PLANE]`.
- Shape: new page `libs/typescript/ui/.planning/view/export.md` — one owner, serializer rows, mint delegate parameter, download/share port.
- Unlocks: IDEAS.md [EXPORT_PLANE] — operator evidence, review boards, and pivot results round-trip to files and journals, `exportScene` gaining its consumer.
- Anchors: `.api/google-model-viewer.md`; `.api/perspective-dev-client.md`; `.api/apache-arrow.md` IPC writers; `system/primitive#CLIPBOARD_PORT` law.

[OPFS_CACHE_PAGE]-[QUEUED]: `system/cache.md` — the content-keyed OPFS persistence page.
- Capability: `ContentKey`-keyed octet store, Schema-coded index, quota and eviction policy rows, per-app directory scoping; drives from IDEAS `[ASSET_CACHE]`.
- Shape: new page `libs/typescript/ui/.planning/system/cache.md` — OPFS scoped resource, residency re-warm seam, upload staging seam.
- Unlocks: IDEAS.md [ASSET_CACHE] — cold-start residency, offline evidence boards, and resumable uploads across reloads.
- Anchors: `viewer/scene#RESIDENCY_GRAFT`; `system/atom#STORE_ROOT` kvs law; `view/form#UPLOAD_LANE`.

[ASSET_IDENTITY_ROSTER]-[QUEUED]: Consolidate the served-asset identity roster in `viewer/scene.md`.
- Capability: one roster value feeding the `setDRACOLoader`/`setKTX2Loader`/`setMeshoptDecoder` paths, model-viewer decoder locations, and the perspective wasm identity; the `[CODEC_IDENTITY_GATE]` gate reads the roster; drives from IDEAS `[ASSET_IDENTITY]`.
- Shape: roster rows in `libs/typescript/ui/.planning/viewer/scene.md#RESIDENCY_GRAFT` and `#EMBED_ROW`; `libs/typescript/ui/ARCHITECTURE.md` boundary row naming the iac serving counterpart.
- Unlocks: IDEAS.md [ASSET_IDENTITY] — iac serves the roster as content-addressed stack rows, CSP stays airtight with no foreign-CDN side-load, and a new codec is one roster row.
- Anchors: scene codec-injection law; ARCHITECTURE `codec-absent` boundary.
- Atomic: one roster value and its boundary rows.

[POINT_CLOUD_LAYER_ROWS]-[QUEUED]: Land the point-cloud layer rows in `viewer/geo.md`.
- Capability: `PointCloudLayer` payload rows over `@loaders.gl/las`, an eye-dome `PostProcessEffect` on the shared context, loaders-core registration; `@loaders.gl/3d-tiles` pinned as the existing `Tile3DLayer` row's loader; drives from IDEAS `[POINT_CLOUD]`.
- Shape: rows in `libs/typescript/ui/.planning/viewer/geo.md#LAYER_ROWS` and `#EXTENSION_ROWS`.
- Unlocks: IDEAS.md [POINT_CLOUD] — reality-capture review lands site scans beside the model on the one selection plane, `PostProcessEffect` gaining its consumer.
- Anchors: geo tile-engine law; `.api/deck.gl-core.md` `PostProcessEffect<ShaderPassT>`.
- Tension: the loaders family rides the admission lane; the rows assume the admission lands.

[BVH_PICK_ACCELERATION]-[QUEUED]: Land BVH pick acceleration in `viewer/mark.md`.
- Capability: `three-mesh-bvh` accelerated raycast rows in the pick pipes — BVH built at graft, shared across pick, section, and measure, with the graft fold owning invalidation: a residency mutation or streamed-geometry edit rebuilds or refits the tree (the hierarchy is static per build), and consumers read only the version-stamped structure so spatial queries never go stale; drives from IDEAS `[POINT_CLOUD]`.
- Shape: rows in `libs/typescript/ui/.planning/viewer/mark.md#PICK_PIPES`; BVH build, version stamp, and rebuild/refit seam on `libs/typescript/ui/.planning/viewer/scene.md#RESIDENCY_GRAFT`.
- Unlocks: IDEAS.md [POINT_CLOUD] — mark's pick pipes stay interactive over merged meshes and dense clouds, point-cloud sectioning and measurement riding accelerated raycast.
- Anchors: mark pick pipes; scene graft fold; admission lane row for `three-mesh-bvh`.

[FAULT_CLASS_CONFORMANCE]-[QUEUED]: Ui fault families carry the core class field the branch fault ruling demands.
- Capability: every ui fault family derives the core `FaultClass` kind from its reason vocabulary, so fault policy reads the one core row table and a local rank or retry column has no place to exist.
- Shape: `class` derivation on `libs/typescript/ui/.planning/viewer/scene.md` `GlbFault` — whose `_reasons` rows carry local `rank`/`retry` beside the domain `evict` axis — with the sibling reason-family faults conformed in the same pass.
- Unlocks: the branch fault ruling holds with zero exceptions; recovery policy derives from the core lattice while `evict` stays the genuine viewer domain axis.
- Anchors: `libs/typescript/.planning/RULINGS.md` `[01]-[SHAPE]` fault row; `scene.md` `_reasons` (`rank`/`retry`/`evict`); the runtime families' class-derivation pattern.
- Ripple: mirrors `data` `[FAULT_CLASS_CONFORMANCE]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HOOK_CENSUS_ROWS]-[COMPLETE]: `.planning/system/hook.md` and every census owner carry matching point/runtime rows; form consultation selects preflight only, scene and mark expose adopted sources, and tap faults isolate.
[VITAL_CAPTURE_PAGE]-[COMPLETE]: `.planning/system/vital.md` owns five-vital capture, bounded observer and Profiler windows, replay-point publication, shipped cutoffs, and the compiler evidence fold.
[DECK_METRIC_FOLD]-[COMPLETE]: probe `#METRIC_FOLD` composes every `DeckMetrics` member and the renderer render/compute/memory counters through `_DECK_TIMERS`/`_RENDERER_INFO` and one mean/latest/peak algebra.
[OBSERVE_SEAM_EXECUTABLES]-[COMPLETE]: observe seams are executable at each owner — `Form.observed` definition-seam trip, AtomRpc `spanPrefix`, pivot acquisition/frame evidence, and one spanned scene residency fact source.
