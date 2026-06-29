# [RASM_ARCHITECTURE]

The domain map of `Rasm` — the KERNEL RhinoCommon-aware geometry/numeric kernel. The co-located `Vectors`, `Analysis`, and `Domain` sub-domains plus the greenfield robust-core `Geometry`, the `Rasm.Geometry.*` exact-predicate kernel that admits no external geometry library.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm/
├── Vectors/                   # [MATURE]: Typed vector/field/cloud/mesh/matrix/spectral algebra via VectorIntent.Project
├── Analysis/                  # [MATURE]: analyze/measure/query/intersect/topology/spatial over Rhino geometry
├── Domain/                    # [MATURE]: Rhino normalization, Context tolerance, Stats, Validation
└── Geometry/                  # [GREENFIELD]: Robust-core — Rasm.Geometry.* kernel, no external geometry library
    ├── Numerics/              # Exact-predicate floor + GeometryFault family
    │   ├── Predicates.cs      # PrecisionTier ladder (double→ddouble→Expansion→Fraction) exact predicates
    │   └── Faults.cs          # Consolidated band-2400 GeometryFault family
    ├── Spatial/               # Broad-phase acceleration + persistent topological naming
    │   ├── Index.cs           # SAH-BVH/Morton-octree SpatialIndex over NodeStore with query/refit fold
    │   ├── Naming.cs          # TopoName lineage/NameTable/Track re-anchor
    │   └── Reconciliation.cs  # CanonicalTopology↔NamingHash content-hash reconciliation fence
    ├── Meshing/               # Exact-arithmetic mesh-construction lattice
    │   ├── Delaunay.cs        # Constrained Delaunay/tetrahedralization on InCircle/InSphere
    │   ├── Arrangement.cs     # Managed boolean/overlay cell-complex retiring native CSG gate
    │   ├── Intersect.cs       # Predicate-exact IntersectOp crossing lattice
    │   └── Offset.cs          # Aichholzer-Aurenhammer skeleton/medial/minkowski OffsetOp
    ├── Processing/            # Mesh repair/optimization/solve rail
    │   ├── Repair.cs          # HealOp repair algebra
    │   ├── Receipts.cs        # Typed RebuildReceipt chain
    │   ├── Decimate.cs        # Garland-Heckbert QEM SimplifyOp decimation
    │   ├── Flatten.cs         # LSCM/ARAP/BFF ParamOp UV-flattening over Vectors DEC substrate
    │   ├── Fit.cs             # MLESAC FitOp primitive-fit
    │   └── Solver.cs          # Levenberg-Marquardt geometric Constraint solver
    └── Drawing/               # Kernel-quality 2D drawing-geometry producers
        ├── View.cs            # Predicate-exact hidden-line/silhouette ViewOp returning DrawingProjection
        └── Pack.cs            # Canonical PackOp geometry-encoding lattice returning EncodedGeometry
```

The mature siblings carry realized capability in their own source and `Vectors/_ARCHITECTURE.md`; the robust-core transcribes floor-first — the `Numerics` predicate floor before every `Spatial`/`Meshing`/`Processing` consumer, the `Numerics/Faults` family last. `Spatial` groups the BVH/octree acceleration index, the persistent `TopoName` naming, and the naming↔content-hash reconciliation fence; `Processing` groups the heal algebra, its rebuild receipts, and the decimate/flatten/fit/solver kernels. `Meshing` grounds its constrained-Delaunay owner on the `Numerics` in-circle/in-sphere predicates, and `Meshing/Arrangement` retires the native CSG gate. Each robust-core owner composes floors already authored and re-mints none: `Meshing/Arrangement` folds constrained tetrahedralization, the implicit-point predicates, and the GWN inside/outside scalar; `Processing/Fit` folds the BVH neighbourhood query, the LM iterate, and the cloud PCA vocabulary; `Processing/Flatten` folds the `Vectors` DEC operator surface; `Drawing/View` folds the exact `Orient3D` silhouette sign, the `Meshing/Intersect` section cut, and BVH front-to-back traversal. Each reaches its consumers through a settled rail — `Apply`/`ToMesh`/`FitReceipt`/`DrawingProjection`/`DecimationReceipt`/`EncodedGeometry` — never by coupling into a flat store interior.

## [02]-[SEAMS]

```text seams
Domain/ContentHash               →  csharp:Rasm.Element/Projection/address       # [CONTENT_KEY]: the kernel seed-zero XxHash128 ContentHash.Of entry the Rasm.Element seam composes for every NodeId/ContentAddress — ONE hasher, no second hasher (no domain change; the kernel NAMES the already-present capability)
Geometry/Spatial/reconciliation  →  csharp:Rasm.Persistence/Query                # [CONTENT_KEY]: CanonicalTopology→GeometryHash canonical-byte content-identity hashed through the kernel Domain/ContentHash seed-zero entry; geometry crosses the seam by content-hash ONLY (no host geometry below)
Geometry/Spatial/reconciliation  ⇄  python:runtime/evidence                     # [CONTENT_KEY]: canonical-byte content-identity reproducing the one Domain/ContentHash seed (XxHash128 seed-zero)
Geometry/Spatial/reconciliation  ⇄  typescript:interchange/codec                # [CONTENT_KEY]: content-hashing wasm reproducing the one Domain/ContentHash seed (XxHash128 seed-zero)
Geometry/Drawing/pack            →  csharp:Rasm.AppHost/Runtime                 # [WIRE]: EncodedGeometry / PackOp.Apply channel discriminant
Geometry/Meshing/intersection    →  csharp:Rasm.Fabrication/Posting             # [WIRE]: IntersectResult / PlaneMesh section curve
Geometry/Numerics/predicates     ←  csharp:Rasm.Fabrication/Posting             # [WIRE]: Predicate.Orient2D/Orient3D exact verdict
Geometry/Drawing/view            →  csharp:Rasm.Fabrication/Posting             # [PROJECTION]: DrawingProjection / HLR visible/hidden segments
Geometry/Drawing/view            →  csharp:Rasm.AppUi/Render                    # [PROJECTION]: DrawingProjection / drafting-sheet layout
Geometry/Processing/flatten      →  csharp:Rasm.Fabrication/Posting             # [PROJECTION]: ChartAtlas / UV island layout + DistortionReceipt
Geometry/Processing/flatten      →  csharp:Rasm.AppUi/Render                    # [PROJECTION]: ChartAtlas / texture UV channel
Geometry/Meshing/arrangement     →  csharp:Rasm.Fabrication/Posting/projection  # [WIRE]: Arrangement Apply/ToMesh kept-cell boundary watertight outline
Geometry/Spatial/index           →  csharp:Rasm.Fabrication/Toolpath/guard      # [SHAPE]: SpatialIndex BVH broad-phase keep-out prune
Geometry/Spatial/index           ←  csharp:Rasm.Fabrication/Posting/projection  # [SHAPE]: SpatialIndex BVH broad-phase
Geometry/Spatial/index           ⇄  csharp:Rasm.Compute                         # [SHAPE]: SpatialIndex.ToAcceleration BVH/octree node arrays
*                                ←  csharp:Rasm.Fabrication                     # [SHAPE]: Matrix / Point3d / Vector3d
```

## [03]-[NAMESPACE_LAW]

The mature `Domain/Geometry.cs` owner and the greenfield `Geometry/` robust-core sub-domain share the bare token `Geometry` only at the filesystem path level; the C# namespace axes are DISJOINT and never collide. `Domain/Geometry.cs` declares namespace `Rasm.Domain` and owns the Rhino-normalization vocabulary (`Topology`/`Kind`/`CurveForm` — the geometry-kind discriminant and coercion table, NOT a type named `Geometry`). The robust-core lives under the `Rasm.Geometry.*` namespace tree — `Rasm.Geometry.Numerics`/`Spatial`/`Meshing`/`Processing`/`Drawing`, one namespace per sub-domain, each owning the types of the pages it groups: `Numerics` the predicate floor and the `GeometryFault` family, `Spatial` the acceleration index plus the `TopoName` naming and the naming↔hash reconciliation fence, `Meshing` the Delaunay/arrangement/intersection/offset owners, `Processing` the heal/receipt/decimate/flatten/fit/solver kernels, `Drawing` the view and pack producers. `Rasm.Domain` and `Rasm.Geometry.*` are separate roots, and the robust-core mints no `Topology` namespace — the persistent naming and the naming↔hash reconciliation live under `Rasm.Geometry.Spatial` — so the mature `Rasm.Domain.Topology` object-kind smart enum stands alone with no robust-core counterpart to collide with. This is the SETTLED reconciliation: no rename, no namespace re-scope, no source move — the two `Geometry` tokens are a path coincidence the disjoint namespace roots already resolve, and the robust-core transcription lands `Rasm.Geometry.*` source freely beside the unchanged `Rasm.Domain` owner.
