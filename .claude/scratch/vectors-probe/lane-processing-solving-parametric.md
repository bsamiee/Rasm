# [VECTORS_PROBE] Lane: Processing / Solving / Parametric / Drawing

Kernel `Rasm/.planning/` has no `Vectors/` folder. `Rasm.Vectors` survives as a canonical NAMESPACE per `ARCHITECTURE.md:104-113` `[03]-[NAMESPACE_MAP]` row [02]: the four namespace roots are the frozen contract axis; folder is domain grouping only. Row [02] maps these lane pages INTO `Rasm.Vectors`: `Parametric/Projections`, `Processing/{Intent,Sample,Extract,Flow,Register,Geodesics,Segment}`. Row [04] `Rasm.Geometry.*` owns `Processing/{Repair,Receipts,Decimate,Flatten}`, `Solving/{Solver,Fit}`, `Parametric/Curve`, `Drawing/*` (CONSUMERS of `Rasm.Vectors`). Row [03] `Rasm.Analysis` owns `Parametric/Locate` (consumer).

Census scope: the exact `rg -n 'Rasm\.Vectors'` set (dot-form only) — 37 mentions. Slash-form `Rasm`/Vectors Packages headers are not in this set except where the same line also carries a dot-form token.

In-flight (leg 4, this hour): `Parametric/curve.md` (mtime 07:05), `Drawing/pack.md` (07:05), `Drawing/view.md` (06:42) — every verdict on these is PROVISIONAL. `Processing/` + `Solving/` landed leg 3, settled — full-strength. `Parametric/locate.md` (mtime 07:17, recent) + `projections.md` (03:18) are not in the leg-4 rebuild list; full-strength with a live-ripple flag on locate.

## [01]-[MENTION_CENSUS]

| file:line | mention kind | cited symbol(s) | verdict | owning-page proof |
| :--- | :--- | :--- | :--- | :--- |
| Processing/decimate.md:18 | prose (`Rasm.Vectors Numerics/matrix`) | `SymmetricMatrix.DecomposeCholesky`→`CholeskyResult.SolveDetailed`/`SolveReceipt` (also MeshSpace, VectorCloudMetric.PrincipalCurvature, VectorCloud.Cluster→CurvatureResult/CurvatureSample, MeshFeaturePolicy/VectorIntent.Features/FeatureReceipt/FeatureEdge/MeshFeatureKind, ScalarField.SignedDistanceFromMeshCase) | LEGITIMATE | `Numerics/matrix.md:42,200,214,243,249` (ns `Rasm.Vectors`, `SymmetricMatrix`/`DecomposeCholesky`/`CholeskyResult`/`SolveDetailed`/`SolveReceipt`) |
| Processing/decimate.md:38 | `using Rasm.Vectors;` | consumes MeshSpace/VectorCloud/matrix/ScalarField/VectorIntent | LEGITIMATE | `Meshing/mesh.md:36`, `Spatial/cloud.md`, `Spatial/neighbors.md:272,319`, `Spatial/fields.md:284`, `Numerics/matrix.md:42`, `Processing/intent.md:38` — all `Rasm.Vectors` band |
| Processing/extract.md:45 | `namespace Rasm.Vectors;` | self (Extract) | LEGITIMATE | row [02] maps `Processing/Extract`→`Rasm.Vectors` |
| Processing/flatten.md:3 | prose (`Rasm.Vectors` DEC substrate) | `MeshAdjointSnapshot.Of`/`DiscreteCalculus` | LEGITIMATE | `Meshing/mesh.md:5` (MeshAdjointSnapshot frozen), `Meshing/dec.md:33,9` (ns `Rasm.Vectors`, `DiscreteCalculus`) |
| Processing/flatten.md:18 | prose (`Rasm.Vectors Numerics/matrix`) | `SparseMatrix.FromTriplets`/`SmallestEigenpairsDetailed`, `CholeskySparse.Of`/`SolveDetailed`/`FactorNonZeros` (+MeshSpace, FeatureEdge/MeshFeatureKind, Point3d/Point2d/Vector3d) | LEGITIMATE | `Numerics/matrix.md:267,268,292,336` (`SparseMatrix`/`FromTriplets`/`SmallestEigenpairsDetailed`/`CholeskySparse`); `Processing/segment.md:162,174` |
| Processing/flatten.md:38 | `using Rasm.Vectors;` | consumer | LEGITIMATE | as flatten:18 owners |
| Processing/flow.md:43 | `namespace Rasm.Vectors;` | self (Flow) | LEGITIMATE | row [02] maps `Processing/Flow`→`Rasm.Vectors` |
| Processing/geodesics.md:26 | `using IntrinsicEdge = Rasm.Vectors.MeshKernel.IntrinsicEdge;` | `MeshKernel.IntrinsicEdge` | LEGITIMATE | `Meshing/mesh.md:409` (`internal static class MeshKernel`, ns `Rasm.Vectors`), `mesh.md:5,9` (IntrinsicEdge frozen nested) |
| Processing/geodesics.md:27 | `using IntrinsicMesh = Rasm.Vectors.MeshKernel.IntrinsicMesh;` | `MeshKernel.IntrinsicMesh` | LEGITIMATE | `Meshing/mesh.md:409,55` (`MeshKernel.IntrinsicMesh` nested, ns `Rasm.Vectors`) |
| Processing/geodesics.md:29 | `namespace Rasm.Vectors;` | self (Geodesics) | LEGITIMATE | row [02] maps `Processing/Geodesics`→`Rasm.Vectors` |
| Processing/intent.md:38 | `namespace Rasm.Vectors;` | self (Intent) | LEGITIMATE | row [02] maps `Processing/Intent`→`Rasm.Vectors` |
| Processing/receipts.md:29 | `using Rasm.Vectors;` (ns `Rasm.Geometry.Healing`) | `TopologyReceipt` via `ProjectionRow` | LEGITIMATE | `Meshing/mesh.md:5` (TopologyReceipt frozen, `Rasm.Vectors`), `Numerics/atoms.md:26,301` (`ProjectionRow`) |
| Processing/register.md:26 | `namespace Rasm.Vectors;` | self (Register) | LEGITIMATE | row [02] maps `Processing/Register`→`Rasm.Vectors` |
| Processing/repair.md:38 | `using Rasm.Vectors;` (ns `Rasm.Geometry.Healing`) | `MeshSpace`, `VectorIntent.Topology` | LEGITIMATE | `Meshing/mesh.md:13`, `Processing/intent.md:71,214` (`TopologyCase`/`Topology(MeshSpace)`, ns `Rasm.Vectors`) |
| Processing/sample.md:45 | `namespace Rasm.Vectors;` | self (Sample) | LEGITIMATE | row [02] maps `Processing/Sample`→`Rasm.Vectors` |
| Processing/segment.md:28 | `namespace Rasm.Vectors;` | self (Segment) | LEGITIMATE | row [02] maps `Processing/Segment`→`Rasm.Vectors` |
| Solving/fit.md:37 | `using Rasm.Vectors;` (ns `Rasm.Geometry.Fitting`) | `CloudKernel.CovarianceOf`, `SymmetricMatrix.DecomposeEigen`, `Matrix.SolveDetailed`/`LeastSquaresDetailed`, `EpsilonPolicy` (+Point3d/Vector3d/Plane/Sphere/Cylinder/Line carriers) | LEGITIMATE | `Spatial/cloud.md:5` (`CloudKernel.CovarianceOf`), `Numerics/matrix.md:192,193,211` (`Matrix.SolveDetailed`/`LeastSquaresDetailed`/`SymmetricMatrix.DecomposeEigen`), `Numerics/atoms.md:15` (`EpsilonPolicy`) |
| Solving/solver.md:7 | prose (`the Rasm.Vectors linear-solve SolveReceipt`) | `SolveReceipt` (name-distinct from `ConstraintSolveReceipt`) | LEGITIMATE | `Numerics/matrix.md:192` (`Fin<SolveReceipt> SolveDetailed`, ns `Rasm.Vectors`) |
| Solving/solver.md:34 | `using Rasm.Vectors;` | `SymmetricMatrix`/`CholeskyResult`/`Matrix` owners, Point3d/Vector3d | LEGITIMATE | `Numerics/matrix.md:42,185,200,214` |
| Solving/solver.md:777 | prose (`the Rasm.Vectors matrix owners`) | matrix owners (stable vocab) | LEGITIMATE | `Numerics/matrix.md:42,150,200` |
| Parametric/curve.md:3 | prose | `Rasm.Vectors` boundary-map seam (GShark) | PROVISIONAL | in-flight leg-4 (mtime 07:05); ns `Rasm.Geometry.Parametric` correct per row [04] |
| Parametric/curve.md:15 | prose (control net) | `Point3d` net mapped to GShark | PROVISIONAL | in-flight; Point3d = Rhino carrier per `Numerics/atoms.md:5` |
| Parametric/curve.md:16 | prose (`Apply` marshal) | `Point3d`/`Vector3d`/`Plane`→GShark `Point3`/`Vector3`/`Plane` | PROVISIONAL | in-flight; carriers per `atoms.md:5` |
| Parametric/curve.md:17 | prose (EvalResult) | projection re-emit | PROVISIONAL | in-flight; projection rail = `Numerics/atoms.md` (`Rasm.Vectors`) |
| Parametric/curve.md:20 | prose (Boundary) | sibling evaluator family | PROVISIONAL | in-flight |
| Parametric/curve.md:32 | `using Rasm.Vectors;` (ns `Rasm.Geometry.Parametric`) | Point3d/Vector3d/Plane (Rhino carriers) | PROVISIONAL | in-flight; WATCH: page currently touches only Rhino-carried atoms — a genuinely `Rasm.Vectors`-declared consumption (e.g. the `.Project<TOut>` rail) is unconfirmed on the half-written page, so the using may lean solely on the Atoms settled-carrier framing |
| Parametric/curve.md:120 | fence comment | boundary map note | PROVISIONAL | in-flight |
| Parametric/curve.md:137 | fence comment | GShark→`Rasm.Vectors` map-once note | PROVISIONAL | in-flight |
| Parametric/curve.md:163 | mermaid node | `Rasm.Vectors Point3d net` | PROVISIONAL | in-flight |
| Parametric/curve.md:169 | mermaid node | map-back to `Rasm.Vectors` | PROVISIONAL | in-flight |
| Parametric/curve.md:186 | prose (transcription) | Apply/CurveFrom/marshal seam | PROVISIONAL | in-flight |
| Parametric/curve.md:190 | prose (`[PARAMETRIC_CONTRACT]`) | host-neutral contract | PROVISIONAL | in-flight |
| Parametric/locate.md:32 | fence comment | states `Rasm.Vectors` is NOT a global using (explains the explicit using) | LEGITIMATE | consistent with the props global-usings set; mtime 07:17 (recent — possible live ripple) |
| Parametric/locate.md:33 | `using Rasm.Vectors;` (ns `Rasm.Analysis`) | `.Project<TOut>` projection rail (intent.Project), `Point2d` | LEGITIMATE | `Numerics/atoms.md:3,11,108,301` (`AtomProjection`/`ProjectionRow`/`Project<TOut>`, ns `Rasm.Vectors`); mtime 07:17 recent-flag |
| Parametric/projections.md:35 | `namespace Rasm.Vectors;` | self (Projections) | LEGITIMATE | row [02] maps `Parametric/Projections`→`Rasm.Vectors` (settled, mtime 03:18) |
| Drawing/pack.md:34 | `using Rasm.Vectors;` (ns `Rasm.Geometry.Encoding`) | `MeshSpace`, `VectorCloud.ClusterCase`, `VectorCloudMetric.OrientedNormals` (+Point3d/Vector3d) | PROVISIONAL | in-flight leg-4 (mtime 07:05); substance sound — `Meshing/mesh.md:13`, `Spatial/cloud.md:26,30,176` (`VectorCloud.ClusterCase`/`OrientedNormals` key 23) |
| Drawing/view.md:35 | `using Rasm.Vectors;` (ns `Rasm.Geometry.Projection`) | `MeshSpace`, `FeatureEdge`, `FeatureReceipt` (+Point3d/Vector3d/Line/Plane/Polyline) | PROVISIONAL | in-flight leg-4 (mtime 06:42); substance sound — `Meshing/mesh.md:13`, `Processing/segment.md:174,177`. NOTE: view.md:18 Packages bullet misgroups `MeshEdit` under `Rasm`/Vectors — `MeshEdit` is `Rasm.Geometry.Meshing` (`Meshing/edit.md:38`); provisional misattribution to flag for the leg-4 author |

## [02]-[PER_FOLDER_TOTALS]

| folder | mentions | LEGITIMATE | STALE | MISATTRIBUTED | PROVISIONAL |
| :--- | :---: | :---: | :---: | :---: | :---: |
| Processing (settled) | 16 | 16 | 0 | 0 | 0 |
| Solving (settled) | 4 | 4 | 0 | 0 | 0 |
| Parametric (mixed) | 15 | 3 | 0 | 0 | 12 |
| Drawing (in-flight) | 2 | 0 | 0 | 0 | 2 |
| TOTAL | 37 | 23 | 0 | 0 | 14 |

Settled full-strength set (23 mentions, Processing 16 + Solving 4 + Parametric/locate 2 + Parametric/projections 1): all LEGITIMATE, zero drift.

Provisional set (14 mentions, all leg-4 in-flight): `Parametric/curve.md` ×12, `Drawing/pack.md` ×1, `Drawing/view.md` ×1. Substance of pack/view usings already resolves to genuine `Rasm.Vectors` owners; the only substantive watch-items are (a) `curve.md:32` using currently backed only by Rhino-carried atoms, and (b) `view.md:18` slash-form Packages bullet misgrouping `MeshEdit` — both to be settled by the leg-4 author, neither is settled-corpus drift.

Watch-flag (not provisional, not drift): `Parametric/locate.md` mtime 07:17 sits inside the leg-4 window though the page is not on the rebuild roster; its two mentions verify clean now but may receive live ripples.

## [03]-[LANE_VERDICT]

The lane is drift-free. Across 37 dot-form `Rasm.Vectors` mentions, the 23 settled-page mentions (all of Processing and Solving, plus `Parametric/locate` and `Parametric/projections`) are each LEGITIMATE and verified against a live declaration: the seven Processing self-declarations (`Extract`/`Flow`/`Geodesics`/`Intent`/`Register`/`Sample`/`Segment`) match row [02]; every consumer `using Rasm.Vectors;` resolves to a genuinely `Rasm.Vectors`-declared owner — the matrix owners in `Numerics/matrix.md:42`, `MeshSpace`/`MeshKernel.IntrinsicMesh`/`MeshAdjointSnapshot`/`TopologyReceipt` in `Meshing/mesh.md`, `DiscreteCalculus` in `Meshing/dec.md`, `VectorCloud`/`VectorCloudMetric`/`CloudKernel` in `Spatial/cloud.md`, `CurvatureResult`/`CurvatureSample` in `Spatial/neighbors.md:272,319`, `ScalarField.SignedDistanceFromMeshCase` in `Spatial/fields.md:284`, the `AtomProjection`/`ProjectionRow`/`.Project<TOut>` rail in `Numerics/atoms.md`, and `VectorIntent`/`Features`/`Topology`/`MeshFeaturePolicy`/`FeatureEdge`/`FeatureReceipt`/`MeshFeatureKind` in `Processing/intent.md`+`Processing/segment.md`. No signature drift appeared on any settled page — cited arities and member names (`DecomposeCholesky`→`CholeskyResult.SolveDetailed`, `SparseMatrix.SmallestEigenpairsDetailed`, `Matrix.LeastSquaresDetailed`, `VectorIntent.Topology(MeshSpace)`) match the current declarations exactly. A subtlety worth recording: `Point3d`/`Vector3d`/`Plane`/`Line` are Rhino.Geometry structs re-carried as `Rasm.Vectors` settled vocabulary (`Numerics/atoms.md:5`), so prose that lists them under "Rasm.Vectors" is loose-but-sanctioned, and each such consumer independently touches a genuinely-declared `Rasm.Vectors` symbol that carries the using. The remaining 14 mentions are all leg-4 in-flight (`curve.md`, `pack.md`, `view.md`) and are marked PROVISIONAL by protocol; their current state is coherent with the settled corpus (correct row-[04] host-neutral namespaces, boundary-map framing intact), with two author-facing watch-items — `curve.md:32`'s using presently leaning on Rhino carriers, and `view.md:18`'s `MeshEdit` misgrouping (`MeshEdit` is `Rasm.Geometry.Meshing`, `Meshing/edit.md:38`) — that a half-written page does not convert into drift.
