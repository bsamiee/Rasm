# [FABRICATION_TASKLOG]

The open and closed work for `Rasm.Fabrication`, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — plus bullets naming the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations.

## [1]-[OPEN]

[UPSTREAM-BLOCKED] NFP_DRL_POLICY — from [DRL_NEST_POLICY]:
- Add a DRL-guided placement `NestPolicy` column in `Nesting/nfp#NESTING` carrying an injected `Func<NoFitPolygon, PartTransform, double>` placement-score delegate ranking placements over the existing NFP primitive; the delegate slot is Fabrication-author, the inference that fills it is consumer-author.
- Strata + external dependency: Fabrication is AEC-domain and the strata law forbids the downward edge to the app-platform `Rasm.Compute`, so the inference lane is never referenced from this folder — the app-platform consumer runs the realized `Rasm.Compute/Model/inference#INFERENCE_MODES` `RunOps.Infer` over a trained placement-ranking model and fills the delegate with a raw scalar, exactly as `REMOVAL_BUDGET_UNION` admits quantity ingress as raw doubles. The inference lane is realized; the blocking residual is the trained placement-ranking model asset, outside this folder's write-scope.
- Fallback path: phase-1 ships the realized bottom-left/genetic folds with the Geometry2D-routed NFP and the content-keyed `Remnant`/`Stock` arm; the learned column lands as one `NestPolicy` delegate column once the consumer-side inference wiring and the model asset exist, the NFP and heuristic folds unchanged. Consumer seam (not a Fabrication reference): `Rasm.Compute/Model/inference#INFERENCE_MODES`.

[UPSTREAM-BLOCKED] CSG_WATERTIGHT_SILHOUETTE — from [CSG_SILHOUETTE]:
- A watertight-solid silhouette arm on `Posting/projection#PROJECTION_HIDDEN_LINE` producing the exact outline of a boolean-combined solid by composing the kernel `Rasm.Geometry/Meshing/arrangement#ARRANGEMENT` `Arrangement` `[Union]` (`MeshBoolean`/`PlanarOverlay`/`CellComplex`) exact arrangement over the settled `Meshing/delaunay#TESSELLATION` constrained-Delaunay and `Meshing/offset#STRAIGHT_SKELETON` substrates — the managed exact path the C# branch `ROBUST_ARRANGEMENT_SUBSTRATE` provides, not a native CSG asset.
- Trickle-down dependency: the kernel exact-arrangement owner's realized `Apply`/`ToMesh` C# surface is the upstream the silhouette composes; the admission is no longer an external NuGet block but a compose-the-kernel-arrangement task held until that C# owner lands (the arrangement DESIGN page IS authored, so the compose-target seam anchor is settled), never an in-folder CSG author-kernel and never coupling into the kernel's `ArrangementStore`/`SimplexStore` interior.
- Fallback path: the per-facet `Posting/projection#PROJECTION_HIDDEN_LINE` HLR kernel stays the only silhouette owner and remains pure-managed with no native asset until the C# arrangement owner lands; the curved-surface analytic silhouette growth arm stays a per-facet-builder extension.

## [2]-[CLOSED]

(none)
