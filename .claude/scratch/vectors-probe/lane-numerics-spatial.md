# LANE: Numerics + Spatial — `Rasm.Vectors` mention audit

Scope: every `Rasm.Vectors` mention inside `libs/csharp/Rasm/.planning/Numerics/` and `.../Spatial/`.
Authority: `libs/csharp/Rasm/ARCHITECTURE.md:111` matrix. Row [02] `Rasm.Vectors` maps
`Numerics/{Atoms,Matrix,Integrate,Spectral,Calculus}` + `Spatial/{Support,Cloud,Neighbors,Transport,Fields}`
(+ Parametric/Meshing/Processing, other lanes). Row [04] `Rasm.Geometry.*` maps
`Numerics/{Predicates,Faults}` + `Spatial/{Index,Naming,Reconciliation}`. `Numerics/Faults` is `Rasm.Geometry`
(band-2400 `GeometryFault`) by the explicit two-family decision in the same row block.

## Mention census (14 mentions across 7 files; `faults.md` has none)

| file:line | cited symbol / form | verdict | owning-page proof |
| :-- | :-- | :-- | :-- |
| `Numerics/atoms.md:3` | prose "primitive floor of `Rasm.Vectors`" | LEGITIMATE | Atoms is matrix row-02 → `Rasm.Vectors`; page declares `namespace Rasm.Vectors;` at `:26` |
| `Numerics/atoms.md:26` | `namespace Rasm.Vectors;` | LEGITIMATE | Atoms mapped to `Rasm.Vectors` (row 02); declaration matches |
| `Numerics/calculus.md:3` | prose "analytic math floor of `Rasm.Vectors`" | LEGITIMATE | Calculus row-02; `namespace Rasm.Vectors;` at `:26` |
| `Numerics/calculus.md:26` | `namespace Rasm.Vectors;` | LEGITIMATE | Calculus mapped row 02 |
| `Numerics/integrate.md:3` | prose "ODE/RK integration floor of `Rasm.Vectors`" | LEGITIMATE | Integrate row-02; `namespace Rasm.Vectors;` at `:28` |
| `Numerics/integrate.md:28` | `namespace Rasm.Vectors;` | LEGITIMATE | Integrate mapped row 02 |
| `Numerics/matrix.md:3` | prose "linear-algebra owner of `Rasm.Vectors`" | LEGITIMATE | Matrix row-02; `namespace Rasm.Vectors;` at `:42` |
| `Numerics/matrix.md:42` | `namespace Rasm.Vectors;` | LEGITIMATE | Matrix mapped row 02 |
| `Numerics/spectral.md:3` | prose "mesh-free spectral algebra of `Rasm.Vectors`" | LEGITIMATE | Spectral row-02; `namespace Rasm.Vectors;` at `:28` |
| `Numerics/spectral.md:28` | `namespace Rasm.Vectors;` | LEGITIMATE | Spectral mapped row 02 |
| `Numerics/predicates.md:21` | prose Packages line: "Rasm.Vectors (project, `Point3d` vocabulary)" | MISATTRIBUTED | `predicates.md` is row-04 `Rasm.Geometry.Numerics` (decl `:35`); cited `Point3d` is declared in `Rhino.Geometry` (imported `:32`), NOT `Rasm.Vectors`; no `Rasm.Vectors`-declared symbol is consumed anywhere in the fence |
| `Numerics/predicates.md:31` | `using Rasm.Vectors;` | MISATTRIBUTED (unused import) | Whole-file scan finds zero `Rasm.Vectors`-declared types (no `AtomProjection`/`.Project`/`EpsilonPolicy`/`Direction`/`VectorFrame`/`VectorSpan`/`VectorCone`/`SignedAxis`); `atoms.md` declares no `Point3d`/`Vector3d` extension methods, so the import brings nothing into scope; every consumed type is local (`Sign`/`Axis`/`Expansion`/`Interval`/`Implicit`/`Predicate`) or `Rhino.Geometry` (`Point3d`/`Vector3d`) |
| `Spatial/reconciliation.md:5` | prose "composes `Rasm.Vectors` `MeshSpace`/`VectorCloud`" | LEGITIMATE | `MeshSpace` declared `Meshing/mesh.md:127` under `namespace Rasm.Vectors;` (`:36`, row-02); `VectorCloud` declared `Spatial/cloud.md:26` (`public abstract partial record VectorCloud`, row-02 `Rasm.Vectors`) |
| `Spatial/reconciliation.md:31` | `using Rasm.Vectors;` | LEGITIMATE | `reconciliation.md` is row-04 `Rasm.Geometry.Naming` (decl `:37`); fence consumes `MeshSpace` (`BuildEntities(MeshSpace)` case) and `VectorCloud` (`EncodeForm.Cloud(VectorCloud Source)`), both genuinely declared in `Rasm.Vectors` per above |

## Inverse-defect hunt (types the matrix assigns to `Rasm.Vectors` but a page in these folders declares under another namespace)

- NONE. Every row-04 page in these folders declares only `Rasm.Geometry.*` types: `predicates.md`→`Sign`/`Axis`/`Expansion`/`Interval`/`Predicate`/`Implicit`; `faults.md`→`GeometryFault` (`namespace Rasm.Geometry`, `:50`); `index.md`→`SpatialIndex` (`namespace Rasm.Geometry.Spatial`, `:36`); `naming.md`→`TopoName`/`NameTable`/`Track` (`namespace Rasm.Geometry.Naming`, `:33`); `reconciliation.md`→`ReconcileOp`/`EncodeForm`/`CanonicalTopology`/`NameAddress` (`namespace Rasm.Geometry.Naming`, `:37`). `MeshSpace`/`VectorCloud` are only CONSUMED by `reconciliation.md`, never re-declared there. No `Rasm.Vectors`-owned type is homed under a `Rasm.Geometry` namespace anywhere in these folders.

- OBSERVATION (fence-completeness inconsistency, not a namespace defect): the 5 Spatial `Rasm.Vectors`-mapped pages (`support.md`, `cloud.md`, `neighbors.md`, `transport.md`, `fields.md`) OMIT the `namespace Rasm.Vectors;` prelude line from their code fences, whereas all 5 Numerics `Rasm.Vectors` pages carry it explicitly. `cloud.md:26` declares `VectorCloud` — a type the matrix FROZEN_BY note ("`MeshSpace` + DEC/field/cloud vocabulary") and the whole-kernel consumption (`Analysis/inspect`, `Drawing/pack`, `Processing/decimate`/`extract`, all cite `Rasm`/`Vectors`) treat as `Rasm.Vectors.VectorCloud` — yet its fence never states the namespace. This is a stylistic gap, not a misattribution: there is no competing declaration under a different namespace, and consumption is matrix-consistent.

## Per-folder totals

- Numerics (12 mentions): 10 LEGITIMATE / 0 STALE / 2 MISATTRIBUTED. The two MISATTRIBUTED are the paired `predicates.md:21`+`:31` (one unused `using` + its packages-line rationalization). `faults.md` = 0 mentions.
- Spatial (2 mentions): 2 LEGITIMATE / 0 STALE / 0 MISATTRIBUTED. Both are `reconciliation.md` (prose + import), and both resolve to real `Rasm.Vectors` declarations (`MeshSpace` in `Meshing/mesh.md`, `VectorCloud` in `Spatial/cloud.md`).

## Lane verdict

The `Rasm.Vectors` fabric across Numerics + Spatial is sound: zero STALE mentions (no cited type has died in a rebuild), and every namespace declaration sits on a matrix-row-02 page. The 5 Numerics `Rasm.Vectors` floors (Atoms/Calculus/Integrate/Matrix/Spectral) each correctly declare and are correctly described as `Rasm.Vectors`; `Spatial/reconciliation.md`'s cross-namespace consumption of `MeshSpace`/`VectorCloud` from the row-04 `Rasm.Geometry.Naming` page is legitimate — both types are genuinely declared in `Rasm.Vectors` (`Meshing/mesh.md`, `Spatial/cloud.md`). The single defect is `Numerics/predicates.md`: its `using Rasm.Vectors;` (`:31`) is an unused import and its Packages line (`:21`) misattributes Rhino's `Point3d` to `Rasm.Vectors` — the fence consumes nothing declared in `Rasm.Vectors`, so the import and its rationalization should be struck (or, if the intent was to route raw→`Point3d` materialization through the Atoms `AtomProjection`/`.Project<TOut>` owner, that consumption is missing and the fence must actually call it). Secondary cleanup, not a mention defect: the 5 Spatial `Rasm.Vectors` pages should carry the explicit `namespace Rasm.Vectors;` fence prelude their Numerics siblings already do, so `VectorCloud`'s home namespace is stated at declaration rather than inferred from the matrix and downstream consumers.
