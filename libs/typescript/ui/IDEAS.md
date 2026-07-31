# [TS_UI_IDEAS]

Forward pool of higher-order folder concepts grounded in the interface domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. Ideas drive one or more `TASKLOG.md` tasks.

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

[MATERIAL_UPGRADE]-[QUEUED]: A standard-material graft gains the physical lobes its appearance document seats.
- Capability: a GLB minted without the KHR PBR extensions parses to `MeshStandardMaterial`, which declares no coat, sheen, transmission, iridescence, or anisotropy slot — today `Pbr.seat` surfaces `plane-unbound` `<material-unphysical>` as evidence; upgrading the material in place (`MeshPhysicalMaterial` minted via `copy()` over the standard source, original disposed, graft ledger re-pointed) would let the full lobe set seat on any authored asset.
- Shape: one upgrade arm in `libs/typescript/ui/.planning/viewer/scene.md` `[08]` ahead of `Pbr.seat`'s lobe fold, gated on the resolved appearance demanding a physical-only lobe.
- Unlocks: the set-bind seats every wire lobe regardless of the authoring exporter's extension roster; `<material-unphysical>` narrows to genuinely unseatable cases.
- Anchors: `Pbr.seat` and `_dress` in `viewer/scene.md` `[08]`; `@types/three` `MeshPhysicalMaterial`; the `_release` disposal visitor.
- Route: verify `MeshPhysicalMaterial.prototype.copy` behavior over a `MeshStandardMaterial` source in the shipped three source (does copy tolerate a narrower source, and which defaults land on the physical-only lobes) before any fence lands.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[POINT_CLOUD]-[COMPLETE]: scan and survey clouds are first-class — geo's `_scan` LAS lane (point-record-format descriptor, roster-served worker) and `_depth` eye-dome row, mark's BVH-accelerated pick pipes (`firstHitOnly`, `_marqueeScene` volume descent, one `Selection.picked` family), scene's `_accelerate` visitor with the serialize band, and the data plane's `points` category (`Asset.Lod` decimation/order/prune rows) close the chain producer to pick.
[TONE_VOCABULARY]-[COMPLETE]: `token.md` `[03]-[TONE_VOCABULARY]` declares the closed eight-tone roster (`neutral`/`accent`/`success`/`caution`/`danger`/`added`/`removed`/`changed`) with hue+chroma rows, one perceptual ramp policy, APCA-floored pair guards, and the dark plane derived by reading the same ladder reversed; every surface `_tone` table (panel, mark, probe, vital, primitive, review) now keys `Theme.Tone` and the five divergent inline unions are gone.
[ASSET_CACHE]-[COMPLETE]: `system/cache.md` stands the content-keyed OPFS residency cache — band ledger, integrity gate on read-back, quota sweep over the transcribed `Opfs.Budget` vocabulary, the multi-leaf `bvh` snapshot band under the geometry's own `ContentKey`; egress stays `view/export.md`'s per the recorded boundary.
[DIFF_SURFACE]-[COMPLETE]: `viewer/review.md` renders decoded `BimDiff`/`IdsAudit` per `GlobalId` — change/verdict row tables guarded against the wire shapes, one `HashMap.modifyAt` join fold, board rows contributed to `Grid`, echo/tint/reveal as projections over `Selection.Op` and `Camera.Intent`; the missing audit model-digest provenance rides `[AUDIT_PROVENANCE]` routed at the emitting C# owner.
[EXPORT_PLANE]-[COMPLETE]: `view/export.md` is the one export owner — `_formatRows` vocabulary, `Export.Parcel` Schema owner, the `_MATRIX` mapped serializer record making an illegal source-format pair a compile error, `ExportFault` over `FaultClass.family`, the `Egress` capability Tag beside the clipboard port, and the `RecordBatchWriter.throughDOM` streaming lane.
[GEO_REASON_SPLIT]-[COMPLETE]: `tile-unreachable` classifies `unavailable` beside `frame-refused` at `malformed` on `_geoFamily`, and the scan lane's fused fetch-and-decode gained the `_scanFault` `Match.instanceOf(FetchError)` triage so a transport failure is never quarantined as a payload defect.
[ASSET_IDENTITY]-[COMPLETE]: `Glb.AssetRoster` owns the slug-unique served-asset identity, `Glb.asset` resolves one row with `codec-absent` on absence, `Glb.assetPath` derives the immutable file address, and `Glb.assetDir` the one-digest directory form multi-leaf decoders consume.
[ENVIRONMENT_DOME]-[COMPLETE]: `viewer/scene.md` `[06]-[ENVIRONMENT_FOLD]` folds container-sniffed HDR/EXR arrivals through one backend-matched prefilter per arrival — keyed slot with the analytic floor as its boot row, read policy at the scene fields, producer-agnostic across both set-manifest producers.
[DOME_IRRADIANCE]-[COMPLETE]: the dome slot carries the producer's nine-band irradiance beside its GPU handles and answers directional queries by moving the BASIS at the read, so the wire bands stay verbatim, the prefiltered dome keeps sole ownership of the diffuse term, and the analytic probe row loses its double-counting claim.
[DOME_COMMIT_UNIFICATION]-[COMPLETE]: one commit owns every dome write and three entries reach it — a key change decodes and prefilters, a repeat re-reads stored-frame policy over carried handles, a backend loss re-derives the target — with retirement identity-keyed so a torn dome is unreachable rather than guarded, and each container row owning its whole decode so a callback-shaped deep store rides the lane HDR and EXR already ride.
[HOOK_RAIL]-[COMPLETE]: `.planning/system/hook.md` and six owning endpoints carry the open `Points` seam and runtime rows; veto consultation is payload-selected, adopted sources merge before registration, publish is bounded, and tap faults isolate.
[VITAL_PLANE]-[COMPLETE]: `.planning/system/vital.md` owns the browser evidence this floor can see — bounded LoAF and event-timing windows over one multi-part measure table, id-keyed Profiler windows, compiler diagnostics — publishes every row through the replay hook, and spells the runtime plane's grade vocabulary for tone alone; Core Web Vitals collapsed onto `runtime:otel/vital`.
[SIGNAL_WEAVE]-[COMPLETE]: observe seams land at their owners — AtomRpc `spanPrefix`, Form's definition-seam preflight/outcome trip, chart pivot span/frame count, and scene's spanned graft metrics with one adopted residency-fact stream.
