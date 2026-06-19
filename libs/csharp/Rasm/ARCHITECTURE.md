# [RASM_ARCHITECTURE]

The professional domain map of `Rasm` — the KERNEL RhinoCommon-aware geometry/numeric kernel. Three mature co-located sub-domains (`Vectors`, `Analysis`, `Domain`) plus the greenfield robust-core `Geometry`, the `Rasm.Geometry.*` exact-predicate kernel that admits no external geometry library.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
Rasm/
├── Vectors/                   # MATURE — typed vector/field/cloud/mesh/matrix/spectral algebra via VectorIntent.Project
├── Analysis/                  # MATURE — analyze/measure/query/intersect/topology/spatial over Rhino geometry
├── Domain/                    # MATURE — Rhino normalization, Context tolerance, Stats, Validation
└── Geometry/                  # GREENFIELD robust-core — the Rasm.Geometry.* kernel, no external geometry library
    ├── Numerics/              # the exact-predicate floor + GeometryFault family
    │   ├── Predicates.cs      # adaptive-precision exact predicates over expansion/error-bound arithmetic
    │   └── Faults.cs          # consolidated band-2400 GeometryFault family + ordinal GeometryKeyPolicy
    ├── Spatial/               # broad-phase acceleration + persistent topological naming
    │   ├── Index.cs           # SAH-BVH/Morton-octree SpatialIndex over NodeStore with query/refit fold
    │   ├── Naming.cs          # TopoName lineage/NameTable/Track re-anchor
    │   └── Reconciliation.cs  # CanonicalTopology↔NamingHash content-hash reconciliation fence
    ├── Meshing/               # exact-arithmetic mesh-construction lattice
    │   ├── Delaunay.cs        # constrained Delaunay/tetrahedralization on InCircle/InSphere
    │   ├── Arrangement.cs     # managed boolean/overlay cell-complex retiring the native CSG gate
    │   ├── Intersect.cs       # predicate-exact IntersectOp crossing lattice
    │   └── Offset.cs          # Aichholzer-Aurenhammer skeleton/medial/minkowski OffsetOp
    ├── Processing/            # mesh repair/optimization/solve rail
    │   ├── Repair.cs          # HealOp repair algebra
    │   ├── Receipts.cs        # typed RebuildReceipt chain
    │   ├── Decimate.cs        # Garland-Heckbert QEM SimplifyOp decimation
    │   ├── Flatten.cs         # LSCM/ARAP/BFF ParamOp UV-flattening over the Vectors DEC substrate
    │   ├── Fit.cs             # MLESAC FitOp primitive-fit
    │   └── Solver.cs          # Levenberg-Marquardt geometric Constraint solver
    └── Drawing/               # kernel-quality 2D drawing-geometry producers
        ├── View.cs            # predicate-exact hidden-line/silhouette ViewOp returning DrawingProjection
        └── Pack.cs            # canonical PackOp geometry-encoding lattice returning EncodedGeometry
```

The mature siblings carry realized capability in their own source and `Vectors/_ARCHITECTURE.md`; the robust-core transcribes floor-first — the `Numerics` predicate floor before every `Spatial`/`Meshing`/`Processing` consumer, the `Numerics/Faults` family last. `Spatial` groups the BVH/octree acceleration index, the persistent `TopoName` naming, and the naming↔content-hash reconciliation fence; `Processing` groups the heal algebra, its rebuild receipts, and the decimate/flatten/fit/solver kernels. `Meshing` grounds its constrained-Delaunay owner on the `Numerics` in-circle/in-sphere predicates, and `Meshing/Arrangement` retires the native CSG gate. Each frontier owner composes floors already authored and re-mints none: `Meshing/Arrangement` folds constrained tetrahedralization, the implicit-point predicates, and the GWN inside/outside scalar; `Processing/Fit` folds the BVH neighbourhood query, the LM iterate, and the cloud PCA vocabulary; `Processing/Flatten` folds the `Vectors` DEC operator surface; `Drawing/View` folds the exact `Orient3D` silhouette sign, the `Meshing/Intersect` section cut, and BVH front-to-back traversal. Each reaches its consumers through a settled rail — `Apply`/`ToMesh`/`FitReceipt`/`DrawingProjection`/`DecimationReceipt`/`EncodedGeometry` — never by coupling into a flat store interior.

## [2]-[SEAMS]

```text seams
Geometry/Spatial/reconciliation  ⇄  python:runtime/evidence          # XxHash128 canonical-byte content-identity (content-key)
Geometry/Spatial/reconciliation  ⇄  typescript:interchange/codec     # XxHash128 content-hashing wasm (content-key)
Geometry/Drawing/pack            →  csharp:Rasm.AppHost/Runtime      # EncodedGeometry / PackOp.Apply channel discriminant (wire)
Geometry/Meshing/intersection    →  csharp:Rasm.Fabrication/Posting  # IntersectResult / PlaneMesh section curve (wire)
Geometry/Numerics/predicates     ←  csharp:Rasm.Fabrication/Posting  # Predicate.Orient2D/Orient3D exact verdict (wire)
Geometry/Drawing/view            →  csharp:Rasm.Fabrication/Posting  # DrawingProjection / HLR visible/hidden segments (projection)
Geometry/Drawing/view            →  csharp:Rasm.AppUi/Render         # DrawingProjection / drafting-sheet layout (projection)
Geometry/Processing/flatten      →  csharp:Rasm.Fabrication/Posting  # ChartAtlas / UV island layout + DistortionReceipt (projection)
Geometry/Processing/flatten      →  csharp:Rasm.AppUi/Render         # ChartAtlas / texture UV channel (projection)
```

## [3]-[NAMESPACE_LAW]

The mature `Domain/Geometry.cs` owner and the greenfield `Geometry/` robust-core sub-domain share the bare token `Geometry` only at the filesystem path level; the C# namespace axes are DISJOINT and never collide. `Domain/Geometry.cs` declares namespace `Rasm.Domain` and owns the Rhino-normalization vocabulary (`Topology`/`Kind`/`CurveForm` — the geometry-kind discriminant and coercion table, NOT a type named `Geometry`). The robust-core lives under the `Rasm.Geometry.*` namespace tree — `Rasm.Geometry.Numerics`/`Spatial`/`Meshing`/`Processing`/`Drawing`, one namespace per sub-domain, each owning the types of the pages it groups: `Numerics` the predicate floor and the `GeometryFault` family, `Spatial` the acceleration index plus the `TopoName` naming and the naming↔hash reconciliation fence, `Meshing` the Delaunay/arrangement/intersection/offset owners, `Processing` the heal/receipt/decimate/flatten/fit/solver kernels, `Drawing` the view and pack producers. `Rasm.Domain` and `Rasm.Geometry.*` are separate roots, and the robust-core mints no `Topology` namespace — the persistent naming and the naming↔hash reconciliation live under `Rasm.Geometry.Spatial` — so the mature `Rasm.Domain.Topology` object-kind smart enum stands alone with no robust-core counterpart to collide with. This is the SETTLED reconciliation: no rename, no namespace re-scope, no source move — the two `Geometry` tokens are a path coincidence the disjoint namespace roots already resolve, and the robust-core transcription lands `Rasm.Geometry.*` source freely beside the unchanged `Rasm.Domain` owner.
