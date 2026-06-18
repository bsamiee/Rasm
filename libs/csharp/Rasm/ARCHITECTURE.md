# [RASM_ARCHITECTURE]

The professional domain folder-map of the geometry/numeric kernel: the three mature co-located sub-domains (`Vectors`, `Analysis`, `Domain`) and the greenfield robust-core `Geometry` sub-domain, mirroring the eventual `Rasm.Geometry.*` source tree. Each leaf carries a one-line capability charter naming its eventual source files. The mature sub-domains are settled source; the robust-core leaves are the transcription units. Dependency direction is settled once in the branch architecture and is not restated here; the `.planning/` page router lives in `README.md` and is not duplicated into this map.

## [1]-[DOMAIN_MAP]

```text codemap
Rasm/
├── Vectors/                       # MATURE — typed vector/field/cloud/mesh/matrix/spectral operator vocabulary through the singular VectorIntent.Project rail (Vectors/_ARCHITECTURE.md)
├── Analysis/                      # MATURE — analysis algebra: analyze/measure/query/intersect/topology/spatial/bounds/conformance over Rhino geometry
├── Domain/                        # MATURE — Rhino-normalization, Context tolerance, shared geometry normalization, Stats, Validation
└── Geometry/                      # GREENFIELD robust-core — the Rasm.Geometry.* namespaces, authored from first principles, admitting no external geometry library
    ├── Numerics/                  # the adaptive-precision exact-predicate floor — Predicate, Expansion, ErrorBound, NumericsPolicy
    ├── Spatial/                   # one polymorphic broad-phase acceleration owner — SAH-BVH + Morton linear octree over one NodeStore, the query fold, refit, the Compute clash projection
    ├── Topology/                  # persistent topological naming (TopoName lineage, NameTable, Track re-anchor) AND the naming↔content-hash reconciliation fence (CanonicalTopology, NamingHash) — two concepts, two source files
    ├── Healing/                   # the geometry repair/rebuild rail — the HealOp closed repair algebra AND the typed RebuildReceipt chain — two concepts, two source files
    ├── Constraints/               # one author-kernel geometric constraint solver — the Constraint residual/Jacobian algebra, the DofAnalysis verdict, the Levenberg-Marquardt iterate
    ├── Tessellation/              # the author-kernel constrained Delaunay triangulation/tetrahedralization owner grounded on the InCircle/InSphere predicates; the arrangement substrate the boolean gate and the AEC-domain fabrication/nesting consumers compose
    └── Faults/                    # the consolidated band-2400 GeometryFault family and the one ordinal geometry key policy
```

The mature siblings carry their realized capability in their own source and `Vectors/_ARCHITECTURE.md`; the robust-core sub-domains are authored in the transcription pass (the numeric floor first, predicates before the index/topology/healing/constraint/tessellation consumers, the consolidated fault family last). `Topology` and `Healing` each hold two distinct concepts in one professional domain, so each splits into two source files. `Tessellation` grounds its constrained-Delaunay owner on the exact in-circle/in-sphere predicates the numeric floor already carries; its `Build`/`ToMesh` rail is the arrangement substrate the boolean gate and the AEC-domain fabrication/nesting consumers compose.
