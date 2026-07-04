# Vectors-Probe Lane: Analysis / Domain / Meshing (consumers of the Rasm.Vectors plane)

Scope: every `Rasm.Vectors` token inside `libs/csharp/Rasm/.planning/{Analysis,Domain,Meshing}`.
Authority: `ARCHITECTURE.md:104-115` NAMESPACE_MAP row [02] — `Rasm.Vectors` is declared on
`Numerics/{Atoms,Matrix,Integrate,Spectral,Calculus}`, `Spatial/{Support,Cloud,Neighbors,Transport,Fields}`,
`Parametric/Projections`, `Meshing/{Mesh,Dec,Reconstruct}`, `Processing/{Intent,Sample,Extract,Flow,Register,Geodesics,Segment}`.
These three folders are CONSUMERS (Analysis = `Rasm.Analysis`; Domain = `Rasm.Domain`; Meshing splits
`Rasm.Geometry.*` consumers {Arrangement,Delaunay,Edit,Intersect,Offset,+new Skeleton,Slice} vs
`Rasm.Vectors` owners {Mesh,Dec,Reconstruct}).

Token count: 35 total (`rg -o`) — Analysis 19, Domain 3, Meshing 13.

## Mention census (file:line -> cited symbol -> verdict -> owning-page proof)

### Analysis (19 tokens, all LEGITIMATE)

| file:line | form | cited symbol(s) | verdict | owning-page proof |
| --- | --- | --- | --- | --- |
| inspect.md:223 | packages citation | `VectorCloud.Ring`, `VectorCloudMetric`, `VectorCloudShape`, `VectorIntent.Cloud`/`Direction`/`Angular` | LEGITIMATE | `Spatial/cloud.md:26` `record VectorCloud` + `:63` `Ring(...)`; `:140` `VectorCloudMetric [SmartEnum<int>]`; `:219` `struct VectorCloudShape`; `Processing/intent.md:109` `Cloud(...)`, `:85` `Direction(...)`, `:87` `Angular(...)` |
| inspect.md:235 | `using Rasm.Vectors;` | (import for above) | LEGITIMATE | consumes `VectorCloud`/`VectorCloudMetric`/`VectorCloudShape` (cloud.md) + `VectorIntent` (intent.md) |
| measure.md:307 | packages citation | `VectorIntent.Direction` (axis admission) | LEGITIMATE | `Processing/intent.md:85` `public static VectorIntent Direction(Vector3d value)` |
| measure.md:316 | `using Rasm.Vectors;` | (import) | LEGITIMATE | consumes `VectorIntent.Direction` |
| measure.md:509 | packages+seam citation | `SupportSpace`/`SupportProjection` -> `Spatial/support`; `VectorIntent.Support` -> `Processing/intent` | LEGITIMATE | `Spatial/support.md:123` `record SupportSpace` + `:126` `Of(...)`; `:14` `SupportProjection [SmartEnum<int>]`; `Processing/intent.md:88` `Support(...)` |
| measure.md:520 | `using Rasm.Vectors;` | (import) | LEGITIMATE | call-sites `:604 SupportSpace.Of(value,key)`, `:605 SupportProjection.{ContainmentDistance,SignedDistance,Distance}`, `:606 VectorIntent.Support(space,sample,projection,key)` — all match declared arity |
| query.md:18 | packages citation | `NeighborIndex`/`NeighborQuery`/`NeighborSource`/`NeighborAnswer` -> `Spatial/neighbors` | LEGITIMATE | `Spatial/neighbors.md:98` `NeighborIndex [Union]`, `:34` `NeighborQuery`, `:51` `NeighborSource`, `:89` `NeighborAnswer` |
| query.md:28 | `using Rasm.Vectors;` | (import) | LEGITIMATE | consumes `NeighborIndex.Query` (`:135`), answer arms `Hits(Seq<NeighborHit>)` (`:91`) / `PairsFound(Seq<NeighborPair>)` (`:92`), query cases `Box`/`Ball`/`Overlaps`/`Pairs` (`:38-41`) — all present |
| relations.md:391 | inline qualified | `Rasm.Vectors.VectorIntent.Relation(a,b)` | LEGITIMATE | `Processing/intent.md:130` `public static VectorIntent Relation(Vector3d a, Vector3d b)` — arity match |
| relations.md:392 | inline qualified | `.Project<Rasm.Vectors.VectorRelation>(context,key)` | LEGITIMATE | `intent.md:22` `Project<TOut>(Context,Op?)`; RelationCase arm `intent.md:295` projects to `VectorRelation.Of(a,b,context,key)`; `VectorRelation` `Numerics/atoms.md:112` |
| relations.md:393 | inline qualified (x2) | `Rasm.Vectors.VectorRelation.Parallel` / `.AntiParallel` | LEGITIMATE | `Numerics/atoms.md:114` `Parallel`, `:115` `AntiParallel` (sealed partial class smart-enum) |
| relations.md:430 | packages citation | `VectorIntent.Relation`/`VectorRelation` (tangency rail) | LEGITIMATE | as above |
| select.md:19 | packages citation | `VectorIntent.Direction` | LEGITIMATE | `intent.md:85`; call-sites `:265`/`:330`/`:379`/`:548`/`:549` `Direction(value:...)` |
| select.md:31 | `using Rasm.Vectors;` | (import) | LEGITIMATE | consumes `VectorIntent.Direction` |
| select.md:286 | packages citation | `VectorIntent.Direction` | LEGITIMATE | `intent.md:85` |
| select.md:296 | `using Rasm.Vectors;` | (import) | LEGITIMATE | consumes `VectorIntent.Direction` (face-ranking axis) |
| select.md:395 | packages citation | `VectorIntent.Axes`/`Components`/`Direction`, `SymmetricMatrix`/`Dimension` | LEGITIMATE | `intent.md:86` `Axes(Option<Seq<Vector3d>>,bool)`, `:129` `Components(Point3d,Vector3d,Plane)`, `:85` `Direction`; `Numerics/matrix.md:200` `SymmetricMatrix` + `:212` `DecomposeEigen`; `Numerics/atoms.md:36` `Dimension` |
| select.md:407 | `using Rasm.Vectors;` | (import) | LEGITIMATE | call-sites `:500 VectorIntent.Axes(values,planar)`, `:539 VectorIntent.Components(anchor,value,frame)`, `:542 SymmetricMatrix.Of(dim,upper,key)`, `:543 DecomposeEigen(key)` — all match declared arity |

### Domain (3 tokens)

| file:line | form | cited symbol(s) | verdict | owning-page proof |
| --- | --- | --- | --- | --- |
| evaluation.md:18 | packages citation | `Rasm.Vectors (AtomProjection.Rows/ProjectionRow — the promoted projection rail)` | LEGITIMATE | `Numerics/atoms.md:318` `AtomProjection.Rows<TSelf,TOut>`, `:312` `record struct ProjectionRow`. Cross-namespace (Domain -> Vectors) intentional; `internal` members are same-assembly. Corroborated by `validation.md:170` ("raw->typed projection is `Numerics/atoms.md`'s `ProjectionRow` dispatch") |
| evaluation.md:31 | `using Rasm.Vectors;` | (import) | LEGITIMATE | consumes `AtomProjection.Rows`/`ProjectionRow` in the `ClosestHit` projection rail |
| validation.md:172 | prose seam citation | literal token: "every `Rasm.Vectors` receipt register this way" | LEGITIMATE (token) | Vectors receipts implement `IValidityEvidence`; confirmed e.g. `neighbors.md:61/65` `NeighborHit`/`NeighborPair : IValidityEvidence`, `cloud.md:144` `VectorCloudShape : IValidityEvidence` |

### Meshing (13 tokens)

Owner self-declarations (`namespace Rasm.Vectors;` — trivially LEGITIMATE, these ARE row-[02] owners):
`dec.md:33`, `mesh.md:36`, `reconstruct.md:36`.

Owner-page cross-sibling package citations (LEGITIMATE):
- `dec.md:18` cites `Rasm.Vectors` `Meshing/mesh` (`MeshSpace`, `LaplacianCache`, `IntrinsicMesh`/`IntrinsicEdge`, `Cotangent`, `TopologyReceipt`) -> `mesh.md:13` `MeshSpace [BoundaryAdapter]`, `:16/140` `LaplacianCache`, `:55` `MeshKernel.IntrinsicMesh`, `:9/163` `IntrinsicEdge(s)`, `:43` `Cotangent`, `:196` `record struct TopologyReceipt`.
- `mesh.md:18` cites `Rasm.Vectors` `Numerics/matrix` (`SparseMatrix.FromTriplets`) -> `matrix.md:256/268` `SparseMatrix.FromTriplets`.
- `reconstruct.md:18` cites `Rasm.Vectors` `Numerics/matrix` (`Matrix`, `SymmetricMatrix`, `SparseMatrix`, `CholeskySparse`, `GaugePolicy`/`GaugeShift`, `SolveReceipt`) -> `matrix.md:150` `Matrix`, `:200` `SymmetricMatrix`, `:11` `SparseMatrix`/`CholeskySparse`, `:19` `GaugePolicy`, `:110` `GaugeShift`, `:12` `SolveReceipt`.

Consumer-page `using Rasm.Vectors;` imports (all LEGITIMATE; body-consumed symbol proven):

| file:line | consumed Vectors symbol | verdict | owning-page proof |
| --- | --- | --- | --- |
| arrangement.md:36 | `MeshSpace` (19x) | LEGITIMATE | `Meshing/mesh.md:13/135` |
| delaunay.md:33 | `MeshSpace` (11x) | LEGITIMATE | `Meshing/mesh.md:13/135` |
| edit.md:35 | `MeshSpace` (28x), `TopologyReceipt`, `LaplacianCache` | LEGITIMATE | `mesh.md:135`, `:196`, `:140` |
| intersect.md:34 | `MeshSpace` (8x) | LEGITIMATE | `mesh.md:135`. NOTE: the 4 `Direction` hits (`:217`,`:592`,`:593`) are Rhino `Ray3d.Direction` property access, NOT the atoms.md `Direction` value object — not a Vectors consumption |
| offset.md:34 | `VectorAngle` | LEGITIMATE | `Numerics/atoms.md:96` `readonly partial struct VectorAngle` |
| skeleton.md:35 | `Cotangent` (17x), `CholeskySparse` (8x), `SparseMatrix` (7x), `MeshSpace` (2x), `Dimension` (2x) | LEGITIMATE | `mesh.md:43` `Cotangent`, `:135` `MeshSpace`; `matrix.md:11` `CholeskySparse`/`SparseMatrix`; `atoms.md:36` `Dimension` |
| slice.md:35 | `MeshSpace` (4x) | LEGITIMATE | `Meshing/mesh.md:135` |

## Signature-drift findings (consumed vs declared)

Every consuming call-site was checked against the CURRENT owner declaration (name + arity + rail),
not merely type existence. Result: ZERO arity/name/rail drift.

| consumed call-site | declared signature | status |
| --- | --- | --- |
| `measure.md:606 VectorIntent.Support(space,sample,projection,key)` | `intent.md:88 Fin<VectorIntent> Support(SupportSpace,Point3d,SupportProjection,Op?)` | MATCH |
| `measure.md:604 SupportSpace.Of(value,key)` | `support.md:126 Fin<SupportSpace> Of(object?,Op?)` | MATCH |
| `measure.md:605 SupportProjection.{ContainmentDistance,SignedDistance,Distance}` | `support.md:41/45 + row set` | MATCH |
| `select.md:500 VectorIntent.Axes(values,planar)` | `intent.md:86 Axes(Option<Seq<Vector3d>>=default,bool=false)` | MATCH |
| `select.md:539 VectorIntent.Components(anchor,value,frame)` | `intent.md:129 Components(Point3d,Vector3d,Plane)` | MATCH |
| `select.md:542/543 SymmetricMatrix.Of(dim,upper,key).DecomposeEigen(key)` | `matrix.md:201 Of(Dimension,Arr<double>,Op?)` + `:212 DecomposeEigen(Op?)` | MATCH |
| `relations.md:391-392 VectorIntent.Relation(a,b).Project<VectorRelation>(context,key)` | `intent.md:130 Relation(Vector3d,Vector3d)` + `:22 Project<TOut>(Context,Op?)` | MATCH |
| `inspect.md VectorIntent.Cloud/Angular`, `select.md VectorIntent.Direction` | `intent.md:109/87/85` | MATCH |
| `query.md NeighborIndex.Query + Hits/PairsFound arms` | `neighbors.md:135/91/92` | MATCH |

## The one defect (seam-citation staleness on a Vectors-plane owner page)

`Domain/validation.md:172` — in the registration-law example list, cites
"`SpatialHit`/`SpatialPair` (`Spatial/neighbors.md`)" as the neighbors receipts that register `IValidityEvidence`.

Verdict: STALE (retired names). `Spatial/neighbors.md:5` EXPLICITLY retires the `Spatial*` family:
"This page ABSORBS the retired analysis spatial family — `SpatialIndex` ... `SpatialProbe`, `SpatialHit`/`SpatialPair` ...
the hit/pair carriers land as `NeighborHit`/`NeighborPair`." The live carriers are `NeighborHit` (`neighbors.md:61`)
and `NeighborPair` (`neighbors.md:65`). `rg 'record (Spatial)(Hit|Pair)'` across the whole `.planning` corpus finds
ZERO declaration; `SpatialHit`/`SpatialPair` survive only as (a) the retired-name prose in neighbors.md:5 and
(b) this stale citation. The CONCEPT is live and does register via `IValidityEvidence`; only the cited NAMES are dead.

Fix: `validation.md:172` should read `NeighborHit`/`NeighborPair` (`Spatial/neighbors.md`).
Note the literal `Rasm.Vectors` token on that same line ("every `Rasm.Vectors` receipt") is itself LEGITIMATE;
the defect is the adjacent Vectors-plane (`Spatial/neighbors`) type citation carrying retired names.

## Per-folder totals

- Analysis (19 tokens): 19 LEGITIMATE, 0 stale, 0 misattributed. Every consumption (`VectorIntent` factory family,
  `SupportSpace`/`SupportProjection`, `VectorCloud*`, `Neighbor*`, `SymmetricMatrix`/`Dimension`, `VectorRelation`)
  resolves to a current row-[02] owner declaration with matching arity. No Numerics/Spatial rebuild broke an Analysis call-site.
- Domain (3 tokens): 3 LEGITIMATE `Rasm.Vectors` tokens (evaluation `AtomProjection`/`ProjectionRow` import+citation;
  validation "every Rasm.Vectors receipt" seam phrase). ONE adjacent Vectors-plane seam citation on validation.md:172
  (`SpatialHit`/`SpatialPair`) is STALE — the sole defect in the lane.
- Meshing (13 tokens): 3 owner self-declarations + 3 owner cross-sibling package citations + 7 consumer imports,
  all LEGITIMATE. `MeshSpace` (mesh.md) is the dominant consumed handle; `intersect.md`'s `Direction` hits are Rhino
  `Ray.Direction`, not a Vectors symbol (no false import — `MeshSpace` justifies the `using`).

## Lane verdict

The Analysis/Domain/Meshing consumption of the `Rasm.Vectors` plane is sound. All 35 `Rasm.Vectors` tokens
resolve to genuine row-[02] owner declarations (or are trivially-correct owner self-declarations on Mesh/Dec/Reconstruct),
and every one of the ~15 distinct consumed signatures was checked against the CURRENT declaration with zero arity, name,
or rail drift — the Numerics/Spatial/Processing rebuilds did not silently break a downstream consumer in this lane.
The single defect is a name-staleness in a prose seam citation: `Domain/validation.md:172` still names the retired
`SpatialHit`/`SpatialPair` where the live `Spatial/neighbors.md` carriers are `NeighborHit`/`NeighborPair` — a one-token-pair
correction, concept intact, no code fence affected.
