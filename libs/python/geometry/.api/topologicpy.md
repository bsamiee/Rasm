# [PY_GEOMETRY_API_TOPOLOGICPY]

`topologicpy` API capture placeholder for `geometry`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `topologicpy`
- package: `topologicpy`
- import: `topologicpy`
- owner: `geometry`
- rail: geometry-algebra
- capability: non-manifold cell/aperture topology modeling gated against the C# IfcSemanticModel

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- topology classes (each a static-method facade over `topologic_core`): `Vertex`, `Edge`, `Wire`, `Face`, `Shell`, `Cell`, `CellComplex`, `Cluster`, `Topology`, `Aperture`, `Context`, `Grid`, `Dictionary`
- analysis/utility static-method facades: `Graph` (179 static methods), `Vector` (53), `Matrix` (13), `Helper`, `Color`, `Plotly`, `Sun`, `BVH`, `CSG`, `ShapeGrammar`; `Polyskel` is a free-function polygon-skeleton module (no facade class)
- integration modules: `IFC`, `Speckle`, `Honeybee`, `EnergyModel`, `Neo4j`, `Kuzu`, `GraphDB`, `Ontology`, `GraphRAG`, `LLM`, `ANN`, `GA`, `PyG`, `GQL`

[ENTRYPOINTS]:
- `Topology` central polymorphic owner (157 static methods): constructors `ByGeometry`, `ByBREPString`/`ByBREPFile`/`ByBREPPath`, `ByOBJFile`/`ByOBJPath`, `ByIFCFile`/`ByIFCPath`, `ByJSONString`/`ByJSONFile`/`ByJSONDictionary`, `ByOCCTShape`, `ByMeshData`; analysis `Analyze`, `BoundingBox`, `Centroid`, `CenterOfMass`, `ConvexHull`, `Contains`, `CoveredBy`, `Decompose`, `AdjacentTopologies`; sub-topology accessors `Cells`, `Clusters`, `CellComplexes`; dictionary attach `AddDictionary`, `AddApertures`, `AddContent`
- `Graph` constructors/analysis (`ByTopology`, `ByVerticesEdges`, adjacency/shortest-path/centrality verbs); `Dictionary.ByKeysValues`, `Dictionary.ValueAtKey`
- per-class constructor families: `Cell.ByFaces`/`ByThickenedFace`, `Face.ByWire`/`ByVertices`, `Wire.ByVertices`/`ByEdges`, `CellComplex.ByCells`/`ByFaces`

[IMPLEMENTATION_LAW]:
- API shape: every class is a stateless static-method namespace operating on `topologic_core` C++ handles; there is no instance constructor pattern — `Topology.ByX(...)` returns a core handle consumed by the next static call.
- IFC-gated rail: `Topology.ByIFCFile`/`IFC` module ingests IFC geometry into non-manifold `CellComplex`/`Cell` topology, gated against the C# IfcSemanticModel seam.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `topologicpy`
- Owns: non-manifold cell/aperture topology modeling gated against the C# IfcSemanticModel
- Accept: companion-floor capture on a `python_version<'3.13'` interpreter
- Reject: wrapper-renames and weaker local reimplementation

[CAPTURE_GAP]:
- floor: companion interpreter `python_version<'3.13'`; rides the native geometry stack floor (`topologic-core` cp312 wheel)
- state: `topologicpy==0.9.43` (+`topologic-core==8.0.4`) installs and reflects on a cp312 companion interpreter; the `>=3.15` project venv carries no cp315 wheel, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp312 distribution; every documented class facade, integration module, and `Topology`/`Graph` static-method count resolves — `Polyskel` corrected to a free-function module, no remaining phantom
