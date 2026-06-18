# [FABRICATION_IDEAS]

The forward concept pool for `Rasm.Fabrication`. Each idea is a card — a bracketed slug leader plus a few bullets stating the capability, what it unlocks, and the gap or modern technique it draws on. An idea drives one or more `TASKLOG.md` tasks; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition.

## [1]-[OPEN]

[CLIPPER2_GEOMETRY2D]:
- Admit Clipper2 (Boost-1.0, dependency-free, integer-robust) as the one 2D polygon-algebra substrate in a `geometry2d` sub-domain owning offset/inflate, Boolean clip, Minkowski sum, and the open-path screen clip.
- Collapses three fragile hand-rolled kernels onto one library — the CAM contour `OffsetRing`, the NFP Minkowski merge, and the HLR screen-clip parameter-interval subtraction — at integer-robust precision, freeing author-kernel budget for the BSP visibility, the FK/IK, and the straight skeleton.
- The pages hand-rolled all three against the `algorithms.md` rule that a hand-rolled kernel waits for a benchmark to defeat the library; Clipper2's `Clipper` facade owns the offset, the Boolean, and the Minkowski sum, and its `ClipperD` engine owns the open-path Boolean, and the kerf/lead/tab geometry the `posting` emitter needs rides the same offset owner. Constrained triangulation is outside Clipper2's public surface (its `Delaunay` is an internal demo type), so no 2D-meshing arm is claimed here. The orientation verdict stays the kernel `Predicate.Orient2D` exact sign.

[TROCHOIDAL_SKELETON]:
- A straight-skeleton/medial-axis author-kernel in `toolpath/skeleton` driving a trochoidal/adaptive-clearing `ToolpathKind` row — adaptive arc radius plus radial step holding constant material-removal rate and radial engagement.
- Unlocks the dominant 2025 HEM toolpath class, exact constant-offset contours where a naive per-vertex-normal offset self-intersects (subsuming the old `OffsetRing`), and the pocket-clearing primitive constant-MRR strategies require.
- The contour/pocket-spiral/drill triad omits the HEM class; no managed straight-skeleton library exists on NuGet (CGAL is C++/GPL with license and per-RID burden), so the wavefront-propagation primitive is a genuine forward author-kernel atop the Clipper2 offset substrate.

[PORTABLE_POSTING]:
- A host-neutral `posting` sub-domain: a typed RS-274/ISO-6983 cut-program AST plus kerf-compensation, lead-in/out, micro-tab/bridge, and cut-sequencing geometry over the Geometry2D offset.
- Unlocks the end-to-end portable pipeline (model to HLR/CAM/nest to cut program) for any runtime, kerf-aware dimensions, tab/bridge part retention, and crash-safe inner-before-outer cut ordering.
- The `Motion` and `Placement` streams dead-end to a downstream post-processor with no owner; a portable cut-program emitter is a real fabrication data contract distinct from the Rhino-native file I/O, and the two coexist.

[DRL_NEST_POLICY]:
- A deep-reinforcement-learning-guided nesting placement policy as a `NestPolicy` column, aligning to the `Rasm.Compute/models/inference#INFERENCE_MODES` OrtValue run-mode fold to rank placements over the same NFP primitive.
- Unlocks higher utilization on irregular sheet-metal nesting as one `NestPolicy` column, the NFP primitive unchanged, the inference reusing the existing Compute session substrate rather than a second runtime.
- Bottom-left plus genetic is a deterministic heuristic pair that plateaus on irregular mixed-part sheets; a learned placement-ranking policy over the NFP feasibility set raises utilization on those layouts, and aligning the inference to the Compute session seam composes capability forward rather than minting a parallel inference surface in this folder.

[CSG_SILHOUETTE]:
- A watertight-solid silhouette path — the exact outline of a boolean-combined solid rather than the per-facet silhouette the HLR kernel extracts — gated behind a stable managed CSG admission or a native-asset/RID deploy decision.
- Unlocks exact drafted outlines of boolean-assembled solids (the true profile a mesh-facet silhouette only approximates); held forward and gated so the per-facet HLR kernel stays pure-managed and the folder carries no native asset.
- Exact CSG silhouette is the one fabrication-drafting capability no robust pure-managed library fills today: a managed CSG kernel mature enough to trust for robustness is the missing admission, and the native/GPL alternatives carry license and per-RID deploy burden, so the admission is a branch-level decision outside this folder's write-scope and the idea stays a held boundary, not an in-folder author-kernel.

## [2]-[CLOSED]

No closed ideas.
