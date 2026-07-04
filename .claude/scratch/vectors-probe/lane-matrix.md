# [VECTORS_PROBE] — lane: THE LAW + THE CATALOGS

Question: kernel `.planning/` has no `Vectors/` folder yet `Rasm.Vectors` mentions persist. Hypothesis
tested: the kernel conversion retired the FOLDER, kept `Rasm.Vectors` as a canonical NAMESPACE ruled by
`ARCHITECTURE.md` row [02] onto pages in `Numerics/`, `Spatial/`, `Parametric/`, `Meshing/`, `Processing/`.
VERDICT: hypothesis CONFIRMED. Law is sound; `using Rasm.Vectors;` in kernel fences is correct by law.

## [01]-[THE_RULED_NAMESPACE_LAW]

`ARCHITECTURE.md:104-115` `[03]-[NAMESPACE_MAP]`. Explicit doctrine (`ARCHITECTURE.md:3,106`,
`README.md:3`): "Folder is domain grouping; fence namespace is the frozen contract axis." Four namespace
roots, pinned by live consumers + sibling corpus, NEVER by folder path:

| Row | Namespace | Matrix page count | Codemap pages (all verified on disk) |
| :-: | :-- | :-: | :-- |
| [01] | `Rasm.Domain` | 7 | `Domain/{rails,context,identity,validation,normalization,evaluation,stats}` |
| [02] | `Rasm.Vectors` | 21 | `Numerics/{atoms,matrix,integrate,spectral,calculus}` (5), `Spatial/{support,cloud,neighbors,transport,fields}` (5), `Parametric/projections` (1), `Meshing/{mesh,dec,reconstruct}` (3), `Processing/{intent,sample,extract,flow,register,geodesics,segment}` (7) |
| [03] | `Rasm.Analysis` | 6 | `Analysis/{query,measure,inspect,select,relations}` (5), `Parametric/locate` (1) |
| [04] | `Rasm.Geometry.*` | 19 | `Numerics/{predicates,faults}`, `Spatial/{index,naming,reconciliation}`, `Parametric/curve`, `Meshing/{edit,delaunay,arrangement,intersect,offset}`, `Processing/{repair,receipts,decimate,flatten}`, `Solving/{solver,fit}`, `Drawing/{view,pack}` |

Two-fault-family carve-out (`ARCHITECTURE.md:115`): `Rasm.Domain.Fault` vs the `Rasm.Geometry` band-2400
`GeometryFault` are two families by explicit decision; `Numerics/faults.cs` + `Domain/rails.cs` each state
the seam; neither absorbs the other. Not a namespace conflict.

### Matrix internal-consistency check: PASS

- Every mapped page exists on disk (all 53 codemap nodes present; verified against `tree` snapshot).
- ZERO double-assignment: the four rows form an exact partition of the 53 codemap pages
  (7 + 21 + 6 + 19 = 53). Every folder's pages split cleanly across at most rows [02]/[03]/[04]:
  Numerics 5→[02]/2→[04]; Spatial 5→[02]/3→[04]; Parametric 1→[02]/1→[03]/1→[04];
  Meshing 3→[02]/5→[04]; Processing 7→[02]/4→[04]; Domain 7→[01]; Analysis 5→[03]; Solving+Drawing→[04].
- Row-4 sub-namespaces vary by domain (`Rasm.Geometry.Numerics`/`.Spatial`/`.Naming`/`.Meshing`/`.Projection`/
  `.Constraints`) — the `Rasm.Geometry.*` star is real, not a single flat namespace.

### IN-FLIGHT (provisional — geometry rebuild leg 4 actively writing; EXCLUDED from the census)

- `Meshing/slice.md` (mtime ~4 min) and `Meshing/skeleton.md` (created after the tree snapshot) are NEW pages
  absent from the codemap AND the namespace matrix. Not judged; not counted. The law will need a matrix row
  update once leg 4 lands — flag for the integrator, not a current defect.

## [02]-[MATRIX_VS_DECLARATIONS_CENSUS]

Method: grep each of the 21 row-[02] pages for in-fence `namespace` declarations; classify.

| Class | Count | Pages |
| :-- | :-: | :-- |
| POSITIVELY declares `namespace Rasm.Vectors;` in a fence | 16 | `Numerics/{atoms,matrix,integrate,spectral,calculus}`, `Parametric/projections`, `Meshing/{mesh,dec,reconstruct}`, `Processing/{intent,sample,extract,flow,register,geodesics,segment}` |
| Silent (namespace-omitting `csharp signature` fences), ZERO conflicting namespace | 5 | `Spatial/{support,cloud,neighbors,transport,fields}` |
| Declares a DIFFERENT (conflicting) namespace = DRIFT | 0 | — |

Matrix claim 21 vs positive-declaration 16: NOT drift. The 5 Spatial pages author their surface in
`csharp signature` fences that omit the `[RUNTIME_PRELUDE] namespace X;` line by page convention (the same
fence flavor CAN carry the namespace — `Parametric/projections` uses `csharp signature` AND declares
`namespace Rasm.Vectors;` at line 35). Fence-flavor evidence: the 16 declarers use `csharp` (atoms, matrix,
integrate, spectral, calculus, register, geodesics, segment), `csharp contract` (mesh, dec, reconstruct,
intent, sample, extract, flow — all carry the ns), or `csharp signature` w/ ns (projections). The 5 silent
pages use `csharp signature` exclusively with no prelude.

DRIFT ANCHOR (false alarm, RESOLVED): `Spatial/neighbors.md:7` names `Rasm.Geometry.Spatial`. This is NOT
neighbors' own namespace — it is a deliberate cross-page distinction: "The settled `Rasm.Geometry.Spatial`
index (first-principles SAH-BVH + Morton octree) is a DIFFERENT altitude by standing decision ... the two
coexist under the host-capture law and neither re-implements the other." `Rasm.Geometry.Spatial` is the
namespace of the row-[04] page `Spatial/index.md`. `neighbors` correctly belongs to `Rasm.Vectors` (row [02]).
No drift.

HYGIENE NOTE (not a defect): the 5 silent Spatial pages carry no POSITIVE in-fence confirmation of their
`Rasm.Vectors` mapping. A future pass could add the namespace prelude for parity with the other 16, but
nothing on those pages contradicts the law.

## [03]-[CATALOG_MENTION_VERDICTS]

The four catalogs cite `Rasm.Vectors` as the NAMESPACE / vector-carrier vocabulary (an integration
boundary), NOT as specific named types. The anchor owner is `Numerics/atoms.md` (`namespace Rasm.Vectors;`
at line 26) — the typed vector-algebra primitive floor: `Direction`/`VectorSpan`/`VectorFrame`/`VectorCone`
value objects + `AtomProjection`/`ProjectionRow` projection dispatch. Consumer anchors are the row-[02]
`Spatial` pages (`neighbors` kd-tree lane, `cloud` hull rail). No catalog names a Rasm.Vectors TYPE that
fails to exist.

| Catalog | Mentions | Verdict | Anchor page + declaring fence |
| :-- | :-: | :-- | :-- |
| `.api/api-gshark.md` (L20,45,163,170,176,184) | 6 | LEGITIMATE | "maps `Rasm.Vectors` ↔ GShark `Point3`/`Vector3`/`Plane` at the boundary, keeps `Rasm.Vectors` canonical" — the Rasm carrier vocabulary is `Numerics/atoms.md` (`namespace Rasm.Vectors;` L26). GShark's own types (Point3/Vector3/Plane) are the OTHER side of the adapter, not Rasm types. |
| `.api/api-kdtree.md` (L14,96,109) | 3 | LEGITIMATE | "maps `Rasm.Vectors` points → `IReadOnlyList<TDimension>` at the boundary" — carrier = `Numerics/atoms.md` `Direction` etc.; consumer = `Spatial/neighbors.md` (`Supercluster.KDTree.Net` lane), a row-[02] page. |
| `.api/api-miconvexhull.md` (L120) | 1 | LEGITIMATE | "adapts `Rasm.Vectors` → its point type (`IVertex`)" — carrier = `Numerics/atoms.md`; consumer = `Spatial/cloud.md` hull rail (`VectorCloud`), a row-[02] page. |
| `.api/api-rhino.md` (L3,27,96,183,184,196) | 6 | LEGITIMATE | "`Rhino.Geometry` value types (`Vector3d`) composed through `Rasm.Vectors`; values cross the seam as `Rasm.Vectors` carriers" — carrier = `Numerics/atoms.md` `Direction`/`VectorFrame`; `Vector3d` is Rhino's, composed through the vocabulary. |

PHANTOMS: 0. Every mention resolves to the `Rasm.Vectors` namespace anchored at `Numerics/atoms.md`
(declaring fence confirmed) plus mapped consumer pages. All references are namespace/boundary citations,
none an undeclared type name.

## [04]-[FOLDER_RETIREMENT_TIMELINE]

The `.planning/Vectors` folder NEVER existed (`git log --all -- .planning/Vectors` = 0 rows). What retired
was DURABLE SOURCE `libs/csharp/Rasm/Vectors/` — pre-conversion the namespace was ALREADY `Rasm.Vectors`
(`git show 741ea2c7f~1:.../Vectors/Atoms.cs` → `namespace Rasm.Vectors;`; same for Cloud.cs, Space.cs).

| Date | Commit | Motion |
| :-- | :-- | :-- |
| 2026-05-21 | `2b47be8d1` | "Collapse Vector dispatcher into VectorIntent.Project; silence CA2225" — pre-conversion durable-source refactor. |
| 2026-05-22 | `744a94462` | "Refactor Rasm vectors architecture" — pre-conversion durable-source refactor. |
| **2026-07-03** | **`741ea2c7f`** | **"Rasm kernel conversion: land the physical restructure — one .planning root, source retired"** — DELETED the 13-file durable source `Rasm/Vectors/{Align,Atoms,Cloud,Extraction,Field,Flow,Intent,Matrix,Mesh,Modes,Sample,Space,Spectral}.cs` + `_ARCHITECTURE.md` + specs/scenarios, AND `git mv`-renamed the sibling `Rasm/Geometry/.planning/*` pages up to `Rasm/.planning/*` (`{Geometry => }` renames on delaunay/intersect/offset/faults/predicates/curve/decimate/fit/flatten/pack/view). One `.planning/` root results. The `Rasm.Vectors` NAMESPACE was preserved and re-homed, decomposed by domain across Numerics/Spatial/Parametric/Meshing/Processing folders. |
| 2026-07-04 | `39aedd1be`, `3ff34266e` | Rebuild-engine redesign + DECISION-census fixes touching `ARCHITECTURE.md`; the current namespace matrix as ruled. |

The kernel conversion PREDATES this session's geometry campaign — confirmed from history, factual.

## [05]-[VERDICT]

`using Rasm.Vectors;` in kernel fences is CORRECT BY LAW. `ARCHITECTURE.md` row [02] rules `Rasm.Vectors`
as one of four frozen contract namespaces, deliberately decoupled from folder path (folder = domain grouping,
namespace = frozen contract). The namespace predates the 2026-07-03 kernel conversion (`741ea2c7f`), which
retired the durable `Rasm/Vectors/` SOURCE folder — never a `.planning/Vectors` folder — while preserving
the namespace and re-homing its 21 pages across five domain folders. The law is internally SOUND: the four
rows exactly partition the 53 codemap pages with zero double-assignment and every mapped page present on
disk. The matrix-vs-declarations census shows 16/21 pages positively declare `namespace Rasm.Vectors;`, the
other 5 (`Spatial/{support,cloud,neighbors,transport,fields}`) omit the prelude via `csharp signature` fence
convention with ZERO conflicting namespace — a documentation-convention gap, not drift. All four catalog
mentions (16 total across gshark/kdtree/miconvexhull/rhino) are LEGITIMATE namespace/boundary citations
anchored at `Numerics/atoms.md`; zero phantoms. Two in-flight Meshing pages (`slice`, `skeleton`) are
provisional and out of the ruled set pending leg-4 landing.
