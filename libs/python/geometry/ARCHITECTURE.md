# [PY_GEOMETRY_ARCHITECTURE]

The professional-domain folder-map for `geometry`: the host-free geometry and IFC/BIM companion. The map is the full sub-domain structure including planned sub-domains that hold no design page yet, each named by its real domain concept with a one-line charter. The tessellation companion is the load-bearing cross-boundary owner; the analysis, scan, topology, and computational-geometry domains ride alongside it; the step-bridge and mesh-utility domains are the visible planned gaps that fuel the forward pool. Dependency direction across the Python packages lives once in the branch `ARCHITECTURE.md`; the wires each sub-domain crosses live on the tasks that build them, never in a folder seam ledger.

## [1]-[DOMAIN_MAP]

The sub-domain layout mirrors the eventual source tree, one folder per professional concept. A leaf `(planned)` carries no design page yet and is a visible gap the ideas and tasks close.

```text codemap
geometry/
├── tessellation/             # IfcOpenShell GLB tessellation daemon — the load-bearing cross-boundary owner
│   └── daemon.py             # TessellationDaemon: source bytes + tolerance → per-element GLB + semantic header
├── ifc-analysis/             # IFC property/quantity/relationship analysis + buildingSMART validation
│   └── analysis.py           # IfcAnalysis: QTO, Pset, IDS, clash, space-program, BCF over the IfcOpenShell ecosystem
├── scan-registration/        # point-cloud and 3D-scan registration
│   └── registration.py       # ScanRegistration: global RANSAC+FPFH, tensor multi_scale_icp, VGICP, multiway pose-graph
├── topology/                 # non-manifold topological modeling over topologicpy
│   └── nonmanifold.py        # TopologyAlgebra: CellComplex/Cell/Aperture construction, decomposition, adjacency, dual-graph
├── computational-geometry/   # AEC computational and numerical geometry over compas
│   └── algebra.py            # ComputationalGeometry: network adjacency, form-finding, numerical primitives, mesh algebra
├── step-bridge/              # (planned) ISO 10303 AP242/AP203/AP214 + IGES CAD-STEP tessellation hop
│   └── cad.py                # (planned) StepBridge: STEP/IGES B-rep bytes + tolerance → GLB over OCCT, the companion the C# StepIso10303 codec calls
└── mesh-utility/             # (planned) robust mesh algebra shared by tessellation, scan, and step hops
    └── repair.py             # (planned) MeshOp: watertight repair, winding/normal fix, manifold3d boolean, mesh-file IO
```

## [2]-[CHARTERS]

- `tessellation`: the persistent IfcOpenShell tessellation companion daemon — source bytes plus a deflection/tolerance policy into per-element GLB and a lightweight semantic header through the `geom.iterator` and the native `geom.serializers.gltf`, served through the runtime `ServerHost` over the existing C# `ComputeService`/`ArtifactSync` contract and content-addressed via runtime `ContentIdentity`.
- `ifc-analysis`: IFC property/quantity/relationship analysis and standards-conformant validation — quantity takeoff, Pset queries, IDS model-checking, clash detection, space-program validation, and BCF round-trip over `ifcopenshell.util`, `ifctester`, `ifcclash`, and `bcf`, emitting evidence that graduates through the compute `HandoffAxis` geometry case.
- `scan-registration`: point-cloud and 3D-scan registration — global RANSAC+FPFH bootstrap, coarse-to-fine `multi_scale_icp` over the open3d tensor backend with robust kernels, the `small_gicp` VGICP speed path, and multiway pose-graph optimization, with transforms graduating through the geometry case.
- `topology`: non-manifold topological modeling over `topologicpy` — `CellComplex`/`Cell`/`Aperture` construction, decomposition, and adjacency/dual-graph extraction the C# `IfcSemanticModel` spatial projection does not perform.
- `computational-geometry`: AEC computational and numerical geometry over `compas` — graph/network adjacency, form-finding over `compas_dr`/`compas_tna`, best-fit/boolean/bbox numerical primitives, and mesh datastructure algebra.
- `step-bridge` (planned): the ISO 10303 AP242/AP203/AP214 and IGES CAD-STEP tessellation hop riding the same two-hop companion shape as IFC — STEP/IGES B-rep bytes plus tolerance into GLB over the OCCT-backed reader, aligned at the wire to the C# `StepIso10303` codec which requests CAD tessellation from this companion rather than re-implementing a managed reader.
- `mesh-utility` (planned): robust mesh algebra the tessellation, scan, and step hops consume downstream — watertight repair, winding/normal fixing, exact boolean over the `trimesh` `manifold3d` backend, and `rhino3dm`/`meshio` mesh-file decode/encode at the data seam.

## [3]-[INTERPRETER_FLOOR]

Every sub-domain rides the companion interpreter floor the branch manifest owns — the sanctioned divergence from the Python core floor, forced by the compiled geometry/IFC cores and isolating the copyleft IFC wheel at the process boundary. The folder consumes this floor as settled and never re-decides it; it surfaces here only because the whole map sits below it.
