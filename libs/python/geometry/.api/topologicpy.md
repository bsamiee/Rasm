# [PY_GEOMETRY_API_TOPOLOGICPY]

`topologicpy` supplies the non-manifold topology modeling surface for the geometry-algebra rail: stateless static-method facades over `topologic_core` C++ handles for the `Vertex` through `CellComplex` topology hierarchy, the central polymorphic `Topology` owner, a `Graph` analysis facade, `Dictionary` attribute carriers, `Vector`/`Matrix`/`Color` algebra, and BIM integration facades (`IFC`, `Honeybee`, `Speckle`). The package owner composes `Topology.ByIFCFile`, the `Cell`/`CellComplex` constructors, and `Graph.ByTopology` into the topology owner gated against the C# `IfcSemanticModel` seam; it never re-implements the non-manifold boolean kernel or graph analytics `topologic_core` and `networkx` already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `topologicpy`
- package: `topologicpy`
- import: `import topologicpy`
- owner: `geometry`
- rail: geometry-algebra
- installed: `0.9.43` with `topologic-core==8.0.4` reflected via per-class import on cp313
- entry points: none (library only)
- capability: non-manifold vertex/edge/wire/face/shell/cell/cell-complex topology, polymorphic `Topology` analysis and boolean ops, graph construction and centrality/shortest-path analytics, dictionary attribute attachment, vector/matrix/color algebra, and IFC/Honeybee/Speckle BIM integration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: topology hierarchy facades
- rail: geometry-algebra

Each facade is a stateless static-method namespace; `By*` constructors return a `topologic_core` handle consumed by the next static call. The method count is the facade breadth.

| [INDEX] | [SYMBOL]      | [METHODS] | [CAPABILITY]                                                |
| :-----: | :------------ | --------: | :---------------------------------------------------------- |
|  [01]   | `Topology`    |       157 | central polymorphic owner: IO, analysis, boolean, transform |
|  [02]   | `Vertex`      |        45 | point topology, coordinates, collinearity, containment      |
|  [03]   | `Edge`        |        37 | line topology, direction, length, intersection              |
|  [04]   | `Wire`        |        68 | polyline/profile topology, shape primitives, skeleton       |
|  [05]   | `Face`        |        67 | planar-region topology, boundaries, medial axis             |
|  [06]   | `Shell`       |        32 | connected face set topology                                 |
|  [07]   | `Cell`        |        55 | solid topology, primitives, volume, containment             |
|  [08]   | `CellComplex` |        29 | non-manifold solid assembly, Voronoi/Delaunay               |
|  [09]   | `Cluster`     |        26 | heterogeneous topology grouping                             |
|  [10]   | `Aperture`    |         2 | opening topology bound to a host context                    |
|  [11]   | `Context`     |         2 | host-topology parameter binding                             |
|  [12]   | `Grid`        |         5 | parametric grid vertices and edges                          |

[PUBLIC_TYPE_SCOPE]: analysis and algebra facades
- rail: geometry-algebra

| [INDEX] | [SYMBOL]       | [METHODS] | [CAPABILITY]                                      |
| :-----: | :------------- | --------: | :------------------------------------------------ |
|  [01]   | `Graph`        |       179 | graph build, adjacency, centrality, shortest path |
|  [02]   | `Dictionary`   |        30 | key/value attribute attachment and boolean ops    |
|  [03]   | `Vector`       |        53 | vector algebra, azimuth/altitude, compass         |
|  [04]   | `Plotly`       |        25 | figure construction for topologies and graphs     |
|  [05]   | `Helper`       |        20 | iteration, flattening, and conversion utilities   |
|  [06]   | `Sun`          |        17 | solar position and shadow analysis                |
|  [07]   | `Matrix`       |        13 | transform matrices, eigen, multiply/invert        |
|  [08]   | `Color`        |        11 | color conversion and CSS named colors             |
|  [09]   | `ShapeGrammar` |         9 | rule-based shape grammar application              |
|  [10]   | `BVH`          |         6 | bounding-volume hierarchy clash/nearest/raycast   |
|  [11]   | `CSG`          |         6 | constructive solid geometry operation graph       |
|  [12]   | `Polyskel`     |       n/a | free-function straight-skeleton module            |

[PUBLIC_TYPE_SCOPE]: BIM and database integration modules
- rail: geometry-algebra

| [INDEX] | [SYMBOL]      | [INTEGRATION]   | [CAPABILITY]                             |
| :-----: | :------------ | :-------------- | :--------------------------------------- |
|  [01]   | `IFC`         | IFC exchange    | entities, objects, properties, mesh data |
|  [02]   | `Honeybee`    | energy model    | HBJSON model from topology               |
|  [03]   | `Speckle`     | Speckle stream  | stream send/receive (needs `specklepy`)  |
|  [04]   | `EnergyModel` | energy analysis | OSM energy model from topology           |
|  [05]   | `Neo4j`       | graph database  | Neo4j graph persistence                  |
|  [06]   | `Kuzu`        | graph database  | Kuzu embedded graph store                |
|  [07]   | `Ontology`    | semantics       | OWL/RDF ontology classes                 |
|  [08]   | `GraphRAG`    | retrieval       | graph retrieval-augmented generation     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Topology construction and export (`Topology`)
- rail: geometry-algebra

`By*File`/`By*Path`/`By*String` read external geometry into a core handle; `ExportTo*` write it back. `ByGeometry`/`ByMeshData` ingest in-memory arrays.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `Topology.ByIFCFile(file)`           | construct      | IFC geometry to non-manifold topology |
|  [02]   | `Topology.ByBREPString(string)`      | construct      | OpenCASCADE BREP intake               |
|  [03]   | `Topology.ByOBJPath(path)`           | construct      | Wavefront OBJ intake                  |
|  [04]   | `Topology.ByGeometry(vertices, ...)` | construct      | raw vertex/edge/face array intake     |
|  [05]   | `Topology.ByMeshData(mesh)`          | construct      | mesh-dict intake                      |
|  [06]   | `Topology.ByOCCTShape(shape)`        | construct      | OpenCASCADE handle intake             |
|  [07]   | `Topology.ByJSONString(string)`      | construct      | JSON topology intake                  |
|  [08]   | `Topology.ExportToBREP(topology)`    | export         | BREP string/file output               |
|  [09]   | `Topology.ExportToOBJ(topology)`     | export         | OBJ output                            |
|  [10]   | `Topology.ExportToJSON(topology)`    | export         | JSON topology output                  |

[ENTRYPOINT_SCOPE]: Topology analysis and boolean (`Topology`)
- rail: geometry-algebra

Sub-topology accessors return the constituent handles; boolean ops return a new combined handle.

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `Topology.Cells(topology)`               | accessor       | constituent cells                     |
|  [02]   | `Topology.Faces(topology)`               | accessor       | constituent faces                     |
|  [03]   | `Topology.Vertices(topology)`            | accessor       | constituent vertices                  |
|  [04]   | `Topology.AdjacentTopologies(...)`       | accessor       | topologies adjacent to a sub-topology |
|  [05]   | `Topology.Union(a, b)`                   | boolean        | non-manifold union                    |
|  [06]   | `Topology.Difference(a, b)`              | boolean        | non-manifold difference               |
|  [07]   | `Topology.Intersect(a, b)`               | boolean        | non-manifold intersection             |
|  [08]   | `Topology.Slice(topology, tool)`         | boolean        | slice by a cutting topology           |
|  [09]   | `Topology.BoundingBox(topology)`         | analysis       | axis-aligned bound cell               |
|  [10]   | `Topology.Centroid(topology)`            | analysis       | centroid vertex                       |
|  [11]   | `Topology.Contains(topology, vtx)`       | analysis       | point containment test                |
|  [12]   | `Topology.AddDictionary(topology, dict)` | attribute      | attach a `Dictionary`                 |

[ENTRYPOINT_SCOPE]: sub-topology constructors and graph/dictionary
- rail: geometry-algebra

Per-class `By*` constructors build the named handle from lower topology; `Graph`/`Dictionary` rows drive analysis and attribute carriers.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :------------------------------------ | :------------- | :------------------------------------ |
|  [01]   | `Vertex.ByCoordinates(x, y, z)`       | construct      | point handle from XYZ                 |
|  [02]   | `Edge.ByStartVertexEndVertex(a, b)`   | construct      | edge from two vertices                |
|  [03]   | `Wire.ByVertices(vertices)`           | construct      | wire from ordered vertices            |
|  [04]   | `Face.ByWire(wire)`                   | construct      | face from a closed wire               |
|  [05]   | `Cell.ByFaces(faces)`                 | construct      | solid from bounding faces             |
|  [06]   | `Cell.ByThickenedFace(face, ...)`     | construct      | solid by thickening a face            |
|  [07]   | `CellComplex.ByCells(cells)`          | construct      | assembly from cells                   |
|  [08]   | `CellComplex.ByFaces(faces)`          | construct      | assembly from internal/external faces |
|  [09]   | `Graph.ByTopology(topology)`          | construct      | graph from topology adjacency         |
|  [10]   | `Graph.ShortestPath(graph, a, b)`     | analysis       | shortest path between vertices        |
|  [11]   | `Graph.BetweennessCentrality(graph)`  | analysis       | vertex betweenness centrality         |
|  [12]   | `Dictionary.ByKeysValues(keys, vals)` | construct      | attribute dictionary                  |
|  [13]   | `Dictionary.ValueAtKey(dict, key)`    | accessor       | read an attribute value               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY_ALGEBRA]:
- import: per-class `from topologicpy.Topology import Topology` at boundary scope only; module-level import is banned by the manifest import policy.
- facade axis: every class is a stateless static-method namespace over `topologic_core` C++ handles; there is no instance constructor pattern. `Topology.ByX(...)` returns a handle of `topologic_core` type that the next static call consumes, so state lives in the handle, never the facade.
- ownership axis: `Topology` is the central polymorphic owner discriminating on the runtime handle kind; `HighestType`/`Type`/`TypeAsString` report the kind, and accessors (`Cells`, `Faces`, `Vertices`) plus boolean ops (`Union`, `Difference`, `Intersect`, `Slice`) work across every topology kind without a per-kind function family.
- construction axis: each sub-topology facade exposes a `By*` constructor family building from the next-lower topology (`Vertex` -> `Edge` -> `Wire` -> `Face` -> `Shell` -> `Cell` -> `CellComplex`); shape primitives (`Wire.Rectangle`, `Cell.Box`, `Cell.Sphere`) are static factories on the owning facade.
- attribute axis: `Dictionary.ByKeysValues` builds attribute carriers attached via `Topology.AddDictionary`; `Graph.ByTopology` lifts adjacency into a `Graph` whose 179 methods own centrality, shortest path, connectivity, and partition analytics.
- IFC-gated rail: `Topology.ByIFCFile`/`Topology.ByIFCPath` and the `IFC` facade ingest IFC geometry into non-manifold `CellComplex`/`Cell` topology, gated against the C# `IfcSemanticModel` seam so the Python topology side never owns IFC semantic identity.
- boundary: topologicpy owns non-manifold topology and graph analytics; watertight mesh CSG routes to `manifold3d`, triangle-mesh exchange to `trimesh`, OpenNURBS exchange to `rhino3dm`, and IFC semantic identity to the C# `IfcSemanticModel`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `topologicpy`
- Owns: non-manifold cell/aperture topology modeling, polymorphic `Topology` analysis and boolean ops, graph construction and analytics, dictionary attributes, and vector/matrix/color algebra gated against the C# `IfcSemanticModel`
- Accept: non-manifold topology and graph analysis feeding the geometry-algebra owner
- Reject: wrapper-renames of `Topology.ByX`/`Graph.ByTopology`; a hand-rolled non-manifold boolean or graph-centrality kernel where topologicpy is admitted; a per-kind accessor/boolean family over the polymorphic `Topology` row; minting IFC semantic identity the C# owner holds

[CAPTURE_GAP]:
- floor: `topologicpy` is an undeclared candidate riding the `topologic-core` native floor; `0.9.43` plus `topologic-core==8.0.4` reflect on a cp313 companion interpreter, while the `>=3.15` project venv carries no wheel and the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp313 distribution; every documented facade, integration module, and `Topology`/`Graph` static-method count resolves against the live class surfaces — no phantom
