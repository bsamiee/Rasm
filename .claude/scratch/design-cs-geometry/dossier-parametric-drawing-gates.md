# [DOSSIER] parametric-drawing-gates

Lane survey over `libs/csharp/Rasm/.planning/` — the Parametric robust-core page (`Parametric/curve.md`) and the Drawing robust-core pages (`Drawing/view.md`, `Drawing/pack.md`), against both consumer gates: the Generation KERNEL GATE (`RASM-GENERATION-SPEC.md:179` + SpineRef law `:164`) and the Fabrication toolpath/HLR gate (`Rasm.Fabrication/ARCHITECTURE.md`). Standing-law context: the landed `Parametric/projections.md` (`Rasm.Vectors`), `Parametric/locate.md` (`Rasm.Analysis`), `Analysis/select.md` (`Rasm.Analysis`) — composed, never re-litigated. All anchors re-verified ON DISK. Stance: hostile; incumbency is not evidence.

Bottom line: all three primary pages are naive-as-shipped and every assigned register row survives on disk. `view.md` ships a dead engine (the register understates it — the prose even names a method that does not exist). `curve.md` is a signature-only stub shell that owns none of the five Generation gate items. `pack.md` advertises residency savings its own storage type falsifies and imports a namespace the landed kernel now makes a hard collision. The two consumer gates are unowned at the kernel exactly where the brief charges.

---

## [00]-[REGISTER_VERDICTS] (assigned rows, re-verified on disk)

| Row | Verdict | On-disk finding |
|---|---|---|
| E1 (dead HLR) | HOLD | Every anchor exact; sharpened below (phantom method name, dead columns, dead free-list). |
| E2 (gate-plane absence) | HOLD | `curve.md:116-139` stubs exact; `:19` Offset in Growth; all 5 NEW parametric pages absent (grep). |
| E3 (retired-system fences) | HOLD + DRIFT | Phantom sites exact; the `Analysis/Intersect.cs` citation is at `curve.md:{3,20,170,190}` — brief named only `170,190`. |
| E5 (unratified ns + phantom) | HOLD | `pack.md:33,18` + `solver.md:13` are the exact three phantom sites (corpus grep); `ARCH:110` placeholder row exact. |
| E11 (dead/misleading carriers) | HOLD | `pack.md:13/53/126-129` false-Half exact; `view.md` BspNode apparatus dead + two fresh sub-carriers. |
| E12 (seam-ledger residue) | HOLD | `curve.md:192` consumer fan is prose not rows (brief `~190`); `pack` seams `ARCH:92-93` composed in-fence. |
| E16 (index-doc obligations) | HOLD + DRIFT | `README:65` advertises the BSP kernel (exact, falsified by fence); `ARCH:63` does NOT carry the BSP claim — it is the accurate codemap row, so the E16 `ARCH:63` pairing is drift. |
| Generation KERNEL GATE | HOLD | `RASM-GENERATION-SPEC.md:179` gate roster, `:226` exposure row, `:164` SpineRef, `:138` PathRow, `:173` Placement — all exact. |
| Fabrication toolpath/HLR gate | HOLD | `FAB:22,23,33,44,46,47,48` all exact (skeleton/slicing/BSP-HLR/BooleanOp/view-consume/HiddenLineResult/slice-reroute). |

---

## [01]-[PAGE] Parametric/curve.md — REBUILD (grade 3 → 9.5)

Namespace `Rasm.Geometry.Parametric` (`curve.md:40`). Charter thesis (`:3`): the host-neutral GShark NURBS evaluation owner meeting the Rhino host at the wire. The thesis is sound; the fence is a stub shell that owns nothing the Generation gate demands.

### Confirmed defects

- **STUB SHELL (E2).** `CurveFrom`/`SurfaceFrom` (`:116-117`), `CurveApply`/`SurfaceApply` (`:130-131`), `ToPolyline`/`ToMesh` (`:138-139`) are all signature-only (`;`-terminated, no body). `Apply` (`:123-128`) is a two-arm dispatch into the two stub marshals. The entire evaluation surface is unrealized — `[03]-[DENSITY_BAR]` (`:186`) itself concedes these are "signature-fixed transcription targets". The register anchor `116-140` is exact for the stub cluster.
- **GATE ITEMS ABSENT (E2, both gates).** `ParametricOp` (`:94-102`) is 6 cases: `Evaluate`/`Measure`/`Divide`/`Split`/`Reconstruct`/`Intersect`. NONE is a station/frame family. `Divide` (`:99`) returns `Division(Point3d[] Points, double[] Parameters)` — points and parameters, **no frames**. `Evaluate` (`:97`) returns a single `Sample(… Plane Frame)` per parameter, via the SINGULAR `PerpendicularFrameAt(t)` (`:16`). A corpus grep confirms `curve.md` contains no `PerpendicularFrames` (batch RMF), no `PointAtLength`, no "station" concept. Independent per-parameter `PerpendicularFrameAt(t)` calls do NOT produce a coherent rotation-minimizing sequence (RMF is a sweep — each frame depends on its predecessor), so the current shape cannot answer the SpineRef station demand even by iteration.
- **OFFSET DEMOTED (E2, Fabrication gate).** Curve `Offset` (`:19`) sits in the Growth bullet ("a `Offset` planar-offset op reading `NurbsBase.Offset(distance, plane)`"), not a first-class `ParametricOp` case — yet it is the parametric-curve leg of the three-way offset partition (`[V10]`a: region/polygon → `offset.md`, parametric-curve → this page, surface-normal → `surface.md`).
- **DELETED-SOURCE CITATIONS (E3 — anchor DRIFT).** `Analysis/Intersect.cs` (deleted; capability moved to `Analysis/relations.md`) is cited at **`curve.md:{3, 20, 170, 190}`** — FOUR sites, not the two (`170,190`) the register names. Corrected anchor set: `{3,20,170,190}`.
- **RE-ANCHOR IS TWO-TARGET, not one (refines V2).** The `Analysis/Intersect.cs` citations conflate two concerns. `:3`/`:20`/`:190` frame the peer as the *parametric-evaluation* owner ("owns the SAME parametric concept") — that landed owner is `Parametric/projections.md` (`Rasm.Vectors`, the RhinoCommon `Curve`/`Surface` evaluation selectors), NOT `relations.md`. Only the *intersection* concern re-anchors to `Analysis/relations.md` (the 25-row Rhino intersection lattice, `README:52`/`:34`). `projections.md:3` already states its side of the evaluation boundary ("the host-neutral GShark sibling `curve.md` … meets only at the wire") and `locate.md:22` states the location-algebra side; the brief's blanket "re-anchor to `relations.md`" would mis-home the evaluation edge. Split: evaluation → `projections.md`, intersection → `relations.md`.
- **HOST-NEUTRALITY vs `using Rhino.Geometry;` (fresh).** `:33` imports `using Rhino.Geometry;` under a page whose entire thesis (`:3`) is "WITHOUT RhinoCommon … never inside one runtime". The fence body uses only GShark types and `Rasm.Vectors`; `Ray3d` (`:78`) is the only candidate host type and resolves from `Rasm.Vectors`. The import is either dead or a direct contradiction of the host-neutral charter — a defect either way in the load-bearing engine of the campaign-defining tier.
- **SSI UNADDRESSED (Generation gate).** GShark has no surface-surface intersection; `curve.md` mentions no SSI anywhere. `README:52` even mis-advertises GShark's "curve-surface Intersection" as if the surface case were covered (that is CSI, not SSI). The V2 ruled default (host-deferred charter row) is nowhere on disk.

### Charter-as-it-should-be

Full-body rebuild composing GShark at the instance surface plus the landed field/geodesic planes and `flatten` `ChartAtlas`, meeting `projections.md`/`locate.md`/`relations.md` at the wire (control points + knots + weights), never a second location algebra:
- Kill the six stubs; author the marshals transcription-complete.
- Promote `Offset` to a first-class `ParametricOp` case (`NurbsBase.Offset(distance, plane)`).
- Add the **station/frame family** as first-class cases: arc-length stations via `Divide(maxSegmentLength, equalSegmentLengths)`/`ParameterAtLength`/`PointAtLength` sweeping **`PerpendicularFrames`** (batch RMF, `api-gshark.md:81-88`) — evaluated over the SpineRef `[T0,T1]` window on GShark's normalized-domain convention (`api-gshark.md:165`). This is the direct producer of the `PathRow` placement input (`RASM-GENERATION-SPEC.md:138,164`) and the per-station scalars the `Placement` record needs (`:173-176`: `StationMm`/`PathAngleDegrees`/`Orientation`). Emit `(station, RMF-frame)` pairs, never points-only.
- State the SSI disposition explicitly (host-deferred row).
- Parametric content-identity: emit the canonical-bytes projection (degree · knots · weights · control net) through the `reconciliation` `Encode` family composing the landed `Domain/identity.md` `ContentHash.Of` — a COMPONENT of the node's `ToCanonicalBytes(tolerance)`, never a sibling SpineRef key.
- Re-anchor evaluation citations → `projections.md`, intersection → `relations.md`; drop or justify the `Rhino.Geometry` import against the host-neutral thesis.
- Fault: end the borrowed `ParameterizationFault` per `[V12]` band re-plan (subdivision/develop/panelize concern-adjacent cluster).
- The five NEW parametric pages (`surface.md`/`subdivide.md`/`develop.md`/`panelize.md`/`patternmap.md`) sit beside it; `curve.md`'s `ToMesh` (`:17`) already routes trimmed-surface fill to `arrangement PlanarOverlay` (consistent with `README:99`) — preserve that.

---

## [02]-[PAGE] Drawing/view.md — REBUILD ground-up (grade 4 → 9.5)

Namespace `Rasm.Geometry.Projection` (`view.md:39`). The register (E1) is exact and, if anything, understates the rot: the advertised engine does not merely go unused — the prose names a method that does not exist in the fence.

### Confirmed defects (E1 — every anchor holds)

- **`Paint` never reads `bsp` (`:302`).** `Paint(… BspNode bsp, SpatialIndex index …)` (`:302`) takes `bsp` and never references it; visibility flows entirely through `Clip`→`OccludedAt` (`:310`, `:339`), which read `index` (the BVH) and `policy`, never `bsp`. HOLD, anchor exact.
- **BSP built then discarded (`:263-300`).** `Partition` (`:263-271`) → `Split` (`:273-300`) fills the `BspNode` SoA and `Render` (`:260-261`) threads it into `Paint`, which ignores it. The whole `BspNode`/`Partition`/`Split`/`Spawn` apparatus is dead. HOLD.
- **Uniform `SampleStep` sampling, not Appel QI (`:320-347`).** `Clip` (`:321`) sets `samples = length / policy.SampleStep` (0.5 screen units, `ViewPolicy.Canonical:73`) and marks each sample via `OccludedAt` (`:339-346`) — a single ray toward the eye through the BVH. An occluder edge between two samples is missed; the visible/hidden boundary is quantized to `SampleStep`. This is exactly the sampling non-robustness the page (`:3`,`:16`) claims to eliminate. HOLD.
- **Phantom method name (fresh sharpening of E1).** `:16` prose: "The painter (**`PaintBackToFront`**) traverses the BSP in far-to-near order (at each node, the child on the far side … painted first, then the node's coplanar faces, then the near child)". No `PaintBackToFront` exists in the fence (corpus grep: the string appears ONLY at `:16`); the real method is `Paint` (`:302`) and it performs no such traversal. The advertised engine is fictional down to the method name.
- **`ToPolylines` chains by kind-key, not connectivity (`:130-135`).** `Visible.Append(Hidden).GroupBy(s => s.Edge.Key)` concatenates every segment of an `EdgeKind` (4 possible keys) into one `Polyline` regardless of connectivity, AND merges visible+hidden of the same kind into one loop. Garbage polylines. HOLD, anchor exact.
- **`README:65` advertises the fake (E16).** "one `ViewOp` over a BSP visibility kernel" — the index doc states the target the fence does not deliver. HOLD. (`ARCH:63` DRIFT: it reads "Predicate-exact hidden-line/silhouette ViewOp returning DrawingProjection" — accurate to the result type, silent on the engine; it does NOT carry a falsifiable BSP claim, so the E16 `ARCH:63` pairing is drift — README-only.)

### Fresh dead-carrier findings (beyond the register)

- **Dead `ViewKind` columns.** `EmitsHidden`/`NeedsBsp` (`:46-52`) are declared and never read (grep: only the declarations). `Apply` (`:195-202`) dispatches on the `ViewOp` case TYPE and passes `emitHidden:` as literals (`:199-200`), never consulting `ViewKind.EmitsHidden`/`NeedsBsp`. Two dead metadata columns.
- **Dead free-list inside BspNode.** `Kill` (`:124`) is defined but never called (grep); `Split` (`:273-300`) only `Spawn`s, never collapses. So `Dead[]`/`FreeList` are inert and `Spawn` (`:116`) always takes `Live[0]++`. The charter's "`Dead` plus the free list reuse a collapsed-node slot" (`:13`) is unrealized.
- **`MeshSpace`→soup→native `Mesh`→`MeshSpace` round-trip.** `CreaseEdges` (`:243-248`) rebuilds a native Rhino `Mesh` via `BuildNative` (`:391-396`) from the soup `Soup` (`:386-389`) already extracted from a `MeshSpace`, only to call `MeshSpace.Of` again for the `VectorIntent.Features` dihedral lift. A settled `MeshSpace` is discarded and re-synthesized.
- **`view.md:386` soup adapter is 1 of 3 (E9/V6).** `Soup(MeshSpace)` (`:386`) is one of the three near-identical soup extractions (`intersect.md:326`, `arrangement.md:352-365`) the `[V6]` substrate collapses.
- **`using Rasm.Geometry.Healing` (`:30`) is a V6 ripple.** Imports `MeshEdit` (`:387 MeshEdit.OfMesh`) from Healing; when `MeshEdit` extracts to the Meshing substrate plane (`[V6]`), this import re-anchors — a 1-hop inbound-reference ripple this page carries.
- **`OccludedAt` self-occlusion risk.** The ray originates on the mesh surface (silhouette/crease/boundary edges lie ON the mesh) and casts toward the eye; `OcclusionBias` = 1e-7 world units (`:73`) may not clear an adjacent face, risking false-positive occlusion. Moot under rebuild but records the sampling engine is also numerically fragile.

### Salvage (preserve)

- Exact silhouette locus: `FacesOppose` (`:235-241`) via `camera.SideOf`→`Predicate.Orient3D` (`:90`). Genuinely award-grade; keep.
- Section delegation: `Cut` (`:349-353`) composes `Intersection.Apply(new IntersectOp.PlaneMesh(...))` — the correct single-owner delegation. Keep (`[SECTION_CUT_COLLAPSE]:436`).

### Charter-as-it-should-be

Rebuild the visibility engine as ONE of: (a) a REAL BSP painter `Paint` actually consumes (back-to-front traversal driving the solve), or (b) exact analytic Appel quantitative-invisibility (count updated ONLY at silhouette crossings through the crossing lattice, no sampling). The dead `BspNode`/`Partition`/`Split` apparatus (incl. `Dead`/`Kill`/`FreeList`) becomes load-bearing or dies with the choice. `ToPolylines` chains by connectivity (successor-linked walk), never kind-key concat, and never merges visible with hidden. Kill or consume the `EmitsHidden`/`NeedsBsp` columns. Collapse the soup round-trip onto the `[V6]` substrate owner.
- **State the select boundary on view's side (V3 gap).** `view.md` references `select` NOWHERE (grep). `select.md:593` [HOST_CAPTURE_SEAMS] already states its side ("`Silhouette.Compute`/`ComputeDraftCurve` is the view-dependent capture tier standing beside the settled `Drawing/view` robust hidden-line owner … consumers select by altitude"). V3 requires the boundary on BOTH pages — add it to view.
- **Fabrication consumes `DrawingProjection` (V3, verified).** `FAB:33` `Posting/Projection.cs` "BSP front-to-back HLR projection" is a cross-strata DUPLICATE of view's (dead) BSP HLR — and dies for the kernel `DrawingProjection` per `[PLACEMENT_LAW]`. `FAB:46` already consumes view ("DrawingProjection HLR visible/hidden segments **beside the in-folder BSP solver**" — the "beside" clause dies with `FAB:33`). `FAB:47` routes `HiddenLineResult` to AppUi ("the BSP visibility solver superseding the AppUi painter sort") — becomes a thin projection of the kernel result. The documentation dry-run (`[05]` acceptance c) gates on connectivity-chained visible/hidden/silhouette/section polylines with **zero sampling misses** — precisely the E1 failure class this rebuild must eliminate. Landed seams: `ARCH:90` (view→Fabrication/Posting), `ARCH:91` (view→AppUi/Render).
- Polygon-fill: re-land the purged-LibTessDotNet concern at `arrangement PlanarOverlay` (`README:99`) or re-scope the README row (`[INDEX_DOC_OBLIGATIONS]`).

---

## [03]-[PAGE] Drawing/pack.md — REBUILD (grade 7 → 9.5)

Namespace `Rasm.Geometry.Encoding` (`pack.md:39`). The strongest of the three pages structurally (real `Readers` data-table dispatch, real round-trip witness), but it advertises a residency saving its storage type falsifies and imports a namespace the landed kernel makes a hard collision.

### Confirmed defects

- **FALSE Half-residency (E11 — all anchors exact).** `:13` claims "a `Half` halves residency at a bounded round-trip error". But `ChannelDtype.Pack` (`:53`) is `float16: (float)(Half)value` — narrow-then-**widen-back** to `float`; `Unpack` (`:56`) is identity `float`. `EncodedStore.Payload` is `float[]` (`:126`, sized `count·arity` floats at `:128-129`) and `EncodedGeometry.Payload` is `ReadOnlyMemory<float>` (`:120`). Every channel occupies a full 32-bit float slot regardless of `Dtype`; `Float16` yields the WORST case — precision loss AND zero residency savings. HOLD, anchors `13/53/126-129` exact.
- **PHANTOM `Rasm.Geometry.Topology` (E3/E5 — exact).** `:33` `using Rasm.Geometry.Topology;` and `:18` packages row crediting `CanonicalTopology.OfMesh`/`NamingHashOps.Encode` to that namespace. With `:31` `using Rasm.Domain;` also in scope and the page residing in `Rasm.Geometry.Encoding`, a bare `Topology` reference is ambiguous between the namespace `Rasm.Geometry.Topology` and the landed `Rasm.Domain.Topology` smart enum (`Domain/normalization.md`, props global-usings-injected) — the collision `[V1]` proves. The types (`CanonicalTopology`/`NamingHashOps`) are used at `:224-226`, `:440`, `:464`. Corpus grep confirms the three phantom sites are exactly `pack.md:18`, `pack.md:33`, `solver.md:13`. HOLD.
- **SEAM MIS-ATTRIBUTION (fresh, internal inconsistency).** `:3` attributes `VectorCloudMetric.OrientedNormals` to **`Processing/fit#FITTING`**. But `:5`/`:463` attribute the SAME surface to the Vectors cloud metric run through `VectorIntent.Cloud`, and the landed MST-orientation owner is `cloud.md:176`→`neighbors.md OrientNormals` (per `[V13]`). `fit` CONSUMES oriented normals; it does not OWN them. `:3` and `:5` contradict each other on the provenance — the `:3` attribution is drift.

### Verified-landed (preserve verbatim, NOT duties — V13)

- `pack.md:346` composes the public `ScalarField.SampleDetailed` rail (`field.SampleDetailed(native.Vertices[i], tolerance)`) — onto landed `fields.md:367`. Also `:463`. Preserve.
- `pack.md:463` reads `VectorCloudMetric.OrientedNormals` through `VectorIntent.Cloud` (`:278`,`:303`,`:332`) — the landed cloud→neighbors path. Preserve the COMPOSITION (fix only the `:3` provenance sentence).
- Seams composed in-fence: `ARCH:92` (pack→AppHost/Runtime) and `ARCH:93` (pack→Compute/Tensor/residency) — present as `[TENSOR_RESIDENCY_SEAM]` (`:465`) and mermaid (`:442-443`). VERIFIED LANDED.
- Content hash composes the landed `Domain/identity.md` `ContentHash.Of` directly for clouds (`:239`) and via `NamingHashOps.Encode` for meshes (`:224-226`) — the one federation hasher, no second key. (Minor asymmetry: cloud path bypasses `NamingHashOps`, calling `ContentHash.Of` directly — both land on the one owner, acceptable.)

### Charter-as-it-should-be (V13)

- **Dtype-strided residency, made real (ruled default).** `EncodedStore.Payload` becomes `byte[]` dtype-strided with per-channel byte descriptors; the `System.Numerics.Tensors` seam (`:465`) survives as descriptor-dispatched typed views — `ReadOnlyTensorSpan<float>` vs `ReadOnlyTensorSpan<Half>` selected on the descriptor's `Dtype` row (the tensor span is element-type-generic, so the strided store is a dispatch row, not a seam defeat; the `[TELOS]` residency lane binds). Re-scope the Dtype column to round-trip-error-only honesty ONLY if drafting proves the typed-view dispatch unviable on the wire.
- **Kill the phantom import (V1).** `using Rasm.Geometry.Topology;` → the ratified naming/reconciliation namespace (ruled `Rasm.Geometry.Naming`); re-credit the `:18` packages row and re-anchor `CanonicalTopology`/`NamingHashOps` uses.
- **Land the `field`·`toolpath` PackKind rows (V13 — consumer-demanded).** `PackKind` has 4 rows (`:80-83`); the page ANTICIPATES two more in prose (`:14`,`:19`) but has not landed them. Downstream demand is REAL and explicit: Compute `ARCH` (`Tensor/residency ⇄ AppHost/Sandbox/solver`) states "**Field/Toolpath land as kernel PackKind rows** + a residency Wire row, never a residency-side GeometryEncoding owner"; `Rasm.AppHost/.planning/Sandbox/solver.md:49-51` mints `EncodingKind` rows (PointCloud/MeshPatch/…) that "project onto the `Compute/Tensor/residency` axis" (`:16`). Land `field` (geodesic/weight) and `toolpath` (position/weight) as two data rows per `[GENERATOR_LAW]`. HARDENING LAW satisfied — the rows have named consumers.
- Fix the `:3` OrientedNormals provenance (→ cloud/neighbors, not fit).

---

## [04]-[CONSUMER_GATES] (both verified, both unowned at the kernel where charged)

### Generation KERNEL GATE (`RASM-GENERATION-SPEC.md`)

Anchors exact: KERNEL GATE `:179`, exposure row `:226`, `SpineRef(ContentKey Geometry, double T0, double T1)` `:164` (factories `Of`/`OfLine` `:165-166`), `PathRow(… SpineRef Spine …)` `:138`, `Placement(int Course, int Sequence, double StationMm … double PathAngleDegrees, Orientation …)` `:173-176`. The gate is the SpineRef consumption law: forms live in kernel geometry, the engine consumes stations/frames/isolines/geodesics/offsets/subdivisions the kernel computes (`:157`,`:179`), enumerating no form vocabulary of its own.

Gate item → kernel owner → on-disk status:

| Gate item (`:179`) | Owner | Status |
|---|---|---|
| arc-length stations + frames on arbitrary curves | `curve.md` (`[V2]`) | ABSENT — `Divide` gives points/params only; no `PerpendicularFrames` batch RMF; no station op. |
| surface isolines/geodesics/offsets | `surface.md` (NEW) | ABSENT — page does not exist; `curve.md` has only single `IsoCurve`. |
| region tessellation + subdivision | `subdivide.md` (NEW) | ABSENT — no Catmull-Clark/Loop owner (grep). |
| developable/panelization | `develop.md` + `panelize.md` (NEW) | ABSENT — `flatten`/`segment` are conformal/host-capture, not isometric-developable; no panelize owner. |
| pattern-to-surface mapping | `patternmap.md` (NEW) | ABSENT — zero matches corpus-wide. |

`SpineRef` resolves geometry by content key (`:164`) — the tier owes a canonical-bytes projection through the landed `Domain/identity.md` `ContentHash.Of` (a COMPONENT of the node's `ToCanonicalBytes`, never a sibling SpineRef key). The acceptance dry-run (`RASM-CS-GEOMETRY-BRIEF.md:226`a) composes the spine RESOLVED by content key, arc-length stations carrying RMF over `[T0,T1]`, geodesic+isoline families on a UV-pulled-back surface, one subdivision pass, one panelized region, one pattern-mapped instance stream with per-instance frames — none of which any current owner can produce.

### Fabrication toolpath/HLR gate (`Rasm.Fabrication/ARCHITECTURE.md`)

All cited rows exact:

| Anchor | Row | Kernel disposition |
|---|---|---|
| `FAB:22` | `Toolpath/Skeleton.cs` — straight-skeleton/medial with **wavefront clearance-radius field** | Dies for `Offsetting.Apply`; kernel medial carries per-point radius + arbitrary-probe clearance query (`[V10]`c/d). Not my page's owner (offset), but the clearance-radius payload is the mandate. |
| `FAB:23` | `Slicing.cs` — FFF/DED planar-section-and-infill slicer | Dies for the NEW kernel slice-stack owner (`[V10]`b). |
| `FAB:33` | `Posting/Projection.cs` — **BSP front-to-back HLR** for AppUi Viewport2D | Dies for kernel `DrawingProjection` (`[V3]`); a cross-strata DUPLICATE of view's (dead) BSP HLR. |
| `FAB:44` | `Posting/projection ← Rasm/Processing/repair` — BooleanOp | Re-anchors to `Meshing/arrangement` under `[V5]` (BooleanOp re-home) — a 1-hop ledger ripple. |
| `FAB:46` | `Posting/projection ← Rasm/Drawing/view` — DrawingProjection HLR "beside the in-folder BSP solver" | Consumes kernel view; the "beside the in-folder BSP" clause dies with `FAB:33`. |
| `FAB:47` | `Posting/projection → Rasm.AppUi/Render` — HiddenLineResult, "BSP visibility solver superseding the AppUi painter sort" | Kernel `DrawingProjection` supersedes; Fabrication's HiddenLineResult becomes a thin projection. |
| `FAB:48` | `Toolpath/slicing → Rasm/Meshing/intersect` — "Section re-routes to kernel IntersectOp.PlaneMesh **when realized**" | The promise resolves only when the kernel lands the slice-stack owner (`[V10]`b). |

The toolpath dry-run (`brief:226`b) and documentation dry-run (`brief:226`c) both gate on this leg: corner-strategy offset + oriented slice contours + medial clearance-radius read + developable-strip unroll + plan/section HLR with connectivity-chained, zero-sampling-miss polylines. For my lane specifically: view's E1 sampling misses directly block the documentation dry-run feeding `FAB:47`.

---

## [05]-[CROSS_CUTTING]

- **Namespace partition (V1) — the three pages, three tiers.** The `Parametric/` folder holds `curve.md` (`Rasm.Geometry.Parametric`, robust-core, `ARCH:110`), `projections.md` (`Rasm.Vectors`, landed, `ARCH:108`), `locate.md` (`Rasm.Analysis`, landed, `ARCH:109`) — resolving the one-file violation. `Drawing/` holds `view.md` (`Rasm.Geometry.Projection`) + `pack.md` (`Rasm.Geometry.Encoding`) — two concept namespaces, one folder; V1's ruled Drawing charter is "downstream emission = projection + residency encoding". Both survive the growth-axis test as-is. The `Rasm.Geometry.*` placeholder (`ARCH:110`, 18 pages, "the geometry campaign owns its namespace reconciliation") is the V1 duty; `curve`/`view`/`pack` supply three of the ~14 fence namespaces the ratified partition enumerates.
- **Phantom namespace census is exactly three (V1/E5).** `pack.md:18`, `pack.md:33`, `solver.md:13` — no more, no fewer (corpus grep). All re-anchor to the ratified naming/reconciliation namespace in one motion; the `topology` fault cluster (2404-2407) renames with them.
- **Dual-paradigm confirmed on all three pages (E5).** Every fence rides bare `Fin` entries with hand-rolled guards and per-page fault routing (`curve` `GeometryFault.ParameterizationFault`; `view` `ProjectionFault`; `pack` `EncodingFault`) — none composes the landed `Op? key = null` threading (`rails.md:14`), the `IValidityEvidence` validity fold, or the `Domain/validation` admission vocabulary. The landed `locate.md` (`CurvatureSample : IValidityEvidence`, `:425-427`; `LocationKeys` `Op.Of` table, `:249-265`) is the in-corpus exemplar the robust-core rebuilds adopt. `GeometryFault` stays the robust-core fault family per the two-family seam (`ARCH:112`).
- **Boundary statements are one-sided.** `projections.md:3` and `locate.md:22` state the curve↔host boundary from the landed side; `select.md:593` states the view↔select boundary from the landed side. NONE of my three pages reciprocates — `curve.md` cites deleted `Analysis/Intersect.cs` instead of `projections.md`/`relations.md`; `view.md` never mentions `select.md`. The rebuilds owe the reciprocal statements (one anchor per seam, both directions).
- **Store-mutability contract (V6).** `view.md`'s `BspNode` (`:93-125`) is a mutable SoA build arena (`Spawn`/`Kill`/`FreeList`) — its "flat partition memory" prose is honest, but the dead `Kill`/`FreeList` must resolve under the `[SEAM_AND_RAIL_LAW]` store-mutability + arena-concurrency contract. `pack.md`'s `EncodedStore` (`:126-131`) is a mutable pack arena written single-threaded in `PackChannels` (`:178-194`) — clean under the contract.
- **RhinoCommon-aware end to end (PLACEMENT_LAW).** `view.md:35` and `pack.md:35` import `Rhino.Geometry` and genuinely use native `Mesh`/`Ray3d`/`BoundingBox`/`Point3f`/`MeshFace` — consistent with the kernel's RhinoCommon-aware compile surface. Only `curve.md:33`'s `using Rhino.Geometry;` is anomalous (host-neutral thesis, no host type used).
- **Index-doc obligations touching this lane.** `README:65` (view BSP kernel) — LAND the real engine or the fence keeps faking it. `README:99` (arrangement PlanarOverlay fill) — `curve.md:17` already routes to it; the V3 view fill and V13 residency re-scopes ride the same `[INDEX_DOC_OBLIGATIONS]` ledger. `ARCH:63` carries no falsifiable view claim (drift vs the register's E16 pairing).

---

## [06]-[PAGES_READ]

Full: `RASM-CS-GEOMETRY-BRIEF.md` (238), `RASM-GENERATION-SPEC.md` (243), `Parametric/curve.md` (193), `Parametric/projections.md` (251), `Parametric/locate.md` (516), `Drawing/view.md` (437), `Drawing/pack.md` (465). Context/targeted: kernel `README.md`/`ARCHITECTURE.md` (index-doc + seam ledger), `Rasm.Fabrication/ARCHITECTURE.md` (codemap + seams `FAB:22-48`), `Analysis/select.md` (silhouette capture tier `:52-99,262-274,593`), `Rasm.Compute/ARCHITECTURE.md` (pack residency seam), `Rasm.AppHost/.planning/Sandbox/solver.md` (EncodingKind consumers). Corpus greps: gate-plane absence, phantom census, deleted-source citations, dead-carrier consumption.
