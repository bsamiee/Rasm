# [PY_GEOMETRY_API_TOPOLOGICPY]

`topologicpy` owns the geometry-algebra rail's non-manifold topology modeling and graph analytics: stateless static-method facades over `topologic_core` C++ handles across the `Vertex`-through-`CellComplex` hierarchy, the polymorphic `Topology` owner, and a `Graph` facade. It never re-implements the boolean kernel or graph analytics `topologic_core` and `networkx` own, and IFC semantic identity stays with the C# `IfcSemanticModel` seam. Its `AGPL-3.0-or-later` network copyleft gates admission opt-in on the Forge lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `topologicpy`
- package: `topologicpy` (`AGPL-3.0-or-later`)
- module: `topologicpy`
- owner: `geometry`
- rail: geometry-algebra, opt-in Forge lane
- capability: non-manifold vertex/edge/wire/face/shell/cell/cell-complex topology, polymorphic `Topology` analysis and boolean ops, graph construction and centrality/shortest-path analytics, dictionary attribute attachment, vector/matrix/color algebra, and IFC/Honeybee/Speckle BIM integration

## [02]-[PUBLIC_TYPES]

Every symbol is a stateless static-method facade over a `topologic_core` handle: a `By*` constructor returns the handle the next static call consumes, so state lives in the handle. `Polyskel` is a free-function straight-skeleton module; the BIM rows are integration facades.

[PUBLIC_TYPE_SCOPE]: topology hierarchy facades

| [INDEX] | [SYMBOL]      | [CAPABILITY]                                                |
| :-----: | :------------ | :---------------------------------------------------------- |
|  [01]   | `Topology`    | central polymorphic owner: IO, analysis, boolean, transform |
|  [02]   | `Vertex`      | point topology, coordinates, collinearity, containment      |
|  [03]   | `Edge`        | line topology, direction, length, intersection              |
|  [04]   | `Wire`        | polyline/profile topology, shape primitives, skeleton       |
|  [05]   | `Face`        | planar-region topology, boundaries, medial axis             |
|  [06]   | `Shell`       | connected face set topology                                 |
|  [07]   | `Cell`        | solid topology, primitives, volume, containment             |
|  [08]   | `CellComplex` | non-manifold solid assembly, Voronoi/Delaunay               |
|  [09]   | `Cluster`     | heterogeneous topology grouping                             |
|  [10]   | `Aperture`    | opening topology bound to a host context                    |
|  [11]   | `Context`     | host-topology parameter binding                             |
|  [12]   | `Grid`        | parametric grid vertices and edges                          |

[PUBLIC_TYPE_SCOPE]: analysis and algebra facades

| [INDEX] | [SYMBOL]       | [CAPABILITY]                                      |
| :-----: | :------------- | :------------------------------------------------ |
|  [01]   | `Graph`        | graph build, adjacency, centrality, shortest path |
|  [02]   | `Dictionary`   | key/value attribute attachment and boolean ops    |
|  [03]   | `Vector`       | vector algebra, azimuth/altitude, compass         |
|  [04]   | `Plotly`       | figure construction for topologies and graphs     |
|  [05]   | `Helper`       | iteration, flattening, and conversion utilities   |
|  [06]   | `Sun`          | solar position and shadow analysis                |
|  [07]   | `Matrix`       | transform matrices, eigen, multiply/invert        |
|  [08]   | `Color`        | color conversion and CSS named colors             |
|  [09]   | `ShapeGrammar` | rule-based shape grammar application              |
|  [10]   | `BVH`          | bounding-volume hierarchy clash/nearest/raycast   |
|  [11]   | `CSG`          | constructive solid geometry operation graph       |
|  [12]   | `Polyskel`     | free-function straight-skeleton module            |

[PUBLIC_TYPE_SCOPE]: BIM and database integration modules

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [CAPABILITY]                             |
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

`By*File`/`By*Path`/`By*String` read external geometry into a core handle; `ExportTo*` write it back; `ByGeometry`/`ByMeshData` ingest in-memory arrays.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `Topology.ByIFCFile(file)`           | construct      | in-memory `ifcopenshell.file` to topology `list[Any]` |
|  [02]   | `Topology.ByIFCPath(path)`           | construct      | IFC file-path intake to topology `list[Any]`          |
|  [03]   | `Topology.ByBREPString(string)`      | construct      | OpenCASCADE BREP intake                               |
|  [04]   | `Topology.ByOBJPath(path)`           | construct      | Wavefront OBJ intake                                  |
|  [05]   | `Topology.ByGeometry(vertices, ...)` | construct      | raw vertex/edge/face array intake                     |
|  [06]   | `Topology.ByMeshData(mesh)`          | construct      | mesh-dict intake                                      |
|  [07]   | `Topology.ByOCCTShape(shape)`        | construct      | OpenCASCADE handle intake                             |
|  [08]   | `Topology.ByJSONString(string)`      | construct      | JSON topology intake                                  |
|  [09]   | `Topology.ExportToBREP(topology)`    | export         | BREP string/file output                               |
|  [10]   | `Topology.ExportToOBJ(topology)`     | export         | OBJ output                                            |
|  [11]   | `Topology.ExportToJSON(topology)`    | export         | JSON topology output                                  |

[ENTRYPOINT_SCOPE]: Topology analysis and boolean (`Topology`)

Accessors return the constituent handles; boolean ops return a new combined handle.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `Topology.Cells(topology)`                          | accessor       | constituent cells                      |
|  [02]   | `Topology.Faces(topology)`                          | accessor       | constituent faces                      |
|  [03]   | `Topology.Edges(topology)`                          | accessor       | constituent edges                      |
|  [04]   | `Topology.Vertices(topology)`                       | accessor       | constituent vertices                   |
|  [05]   | `Topology.SubTopologies(topology, subTopologyType)` | accessor       | typed sub-topology `list`; [05]        |
|  [06]   | `Topology.AdjacentTopologies(...)`                  | accessor       | topologies adjacent to a sub-topology  |
|  [07]   | `Topology.Union(a, b)`                              | boolean        | non-manifold union                     |
|  [08]   | `Topology.Difference(a, b)`                         | boolean        | non-manifold difference                |
|  [09]   | `Topology.Intersect(a, b)`                          | boolean        | non-manifold intersection              |
|  [10]   | `Topology.Slice(topology, tool)`                    | boolean        | slice by a cutting topology            |
|  [11]   | `Topology.BoundingBox(topology)`                    | analysis       | axis-aligned bound cell                |
|  [12]   | `Topology.Centroid(topology)`                       | analysis       | centroid vertex                        |
|  [13]   | `Topology.Contains(topology, vtx)`                  | analysis       | point containment test                 |
|  [14]   | `Topology.Analyze(topology)`                        | analysis       | formatted analysis summary `str`       |
|  [15]   | `Topology.Decompose(topology, ...)`                 | analysis       | building-element category `dict`; [15] |
|  [16]   | `Topology.AddDictionary(topology, dict)`            | attribute      | attach a `Dictionary`                  |

- `Topology.SubTopologies`: `subTopologyType` one of `"vertex"`/`"edge"`/`"wire"`/`"face"`/`"shell"`/`"cell"`/`"cellcomplex"`/`"cluster"`/`"aperture"`, `None` on a bad kind.
- `Topology.Decompose`: `(topology, tiltAngle=10.0, tolerance=0.0001, silent=False) -> dict` of `cells` with external/internal/free vertical/horizontal/inclined face and aperture lists — a role classifier, never a hierarchy accessor.

[ENTRYPOINT_SCOPE]: sub-topology constructors and graph/dictionary

Per-class `By*` constructors build the named handle from lower topology; `Graph`/`Dictionary` rows drive analysis and attribute carriers.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `Vertex.ByCoordinates(x, y, z)`       | construct      | point handle from XYZ                       |
|  [02]   | `Edge.ByStartVertexEndVertex(a, b)`   | construct      | edge from two vertices                      |
|  [03]   | `Wire.ByVertices(vertices)`           | construct      | wire from ordered vertices                  |
|  [04]   | `Face.ByWire(wire)`                   | construct      | face from a closed wire                     |
|  [05]   | `Cell.ByFaces(faces)`                 | construct      | solid from bounding faces                   |
|  [06]   | `Cell.ByThickenedFace(face, ...)`     | construct      | solid by thickening a face                  |
|  [07]   | `CellComplex.ByCells(cells)`          | construct      | assembly from cells                         |
|  [08]   | `CellComplex.ByFaces(faces)`          | construct      | assembly from internal/external faces       |
|  [09]   | `Cluster.ByTopologies(*topologies)`   | construct      | non-manifold `Cluster` from a topology list |
|  [10]   | `Graph.ByTopology(topology)`          | construct      | graph from topology adjacency               |
|  [11]   | `Graph.ShortestPath(graph, a, b)`     | analysis       | geodesic `Wire` between two vertices        |
|  [12]   | `Graph.BetweennessCentrality(graph)`  | analysis       | per-vertex betweenness score list           |
|  [13]   | `Graph.ClosenessCentrality(graph)`    | analysis       | per-vertex closeness score list             |
|  [14]   | `Graph.DegreeCentrality(graph)`       | analysis       | per-vertex degree score list                |
|  [15]   | `Graph.ConnectedComponents(graph)`    | analysis       | island sub-graphs, vertex-count sorted      |
|  [16]   | `Graph.MinimumSpanningTree(graph)`    | analysis       | minimum spanning tree `Graph`               |
|  [17]   | `Graph.Edges(graph)`                  | accessor       | constituent edge handles                    |
|  [18]   | `Graph.JSONData(graph)`               | export         | node-link `dict` for JSON encode            |
|  [19]   | `Dictionary.ByKeysValues(keys, vals)` | construct      | attribute dictionary                        |
|  [20]   | `Dictionary.ValueAtKey(dict, key)`    | accessor       | read an attribute value                     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Import per class at boundary scope (`from topologicpy.Topology import Topology`); the manifest import policy bans module-level import.
- `Topology` is the central polymorphic owner discriminating on the runtime handle kind; `HighestType`/`Type`/`TypeAsString` report the kind, and accessors (`Cells`/`Faces`/`Vertices`) and boolean ops (`Union`/`Difference`/`Intersect`/`Slice`) span every kind without a per-kind family.
- Each sub-topology facade exposes a `By*` constructor building from the next-lower topology (`Vertex` -> `Edge` -> `Wire` -> `Face` -> `Shell` -> `Cell` -> `CellComplex`); shape primitives (`Wire.Rectangle`, `Cell.Box`, `Cell.Sphere`) are static factories on the owning facade.
- `Dictionary.ByKeysValues` builds attribute carriers attached via `Topology.AddDictionary`; `Graph.ByTopology` lifts adjacency into a `Graph` owning centrality, shortest-path, connectivity, and partition analytics.
- Centrality verbs (`BetweennessCentrality`/`ClosenessCentrality`/`DegreeCentrality`) each return a per-vertex score `list` ordered to the graph vertices, never a value mutated in place, so a leaderboard fold ranks the list directly.
- `Connectivity` aliases `DegreeCentrality` (space-syntax connectivity score), never a component count; the island count is `len(Graph.ConnectedComponents(graph))`, whose return is the component sub-graphs sorted by vertex count.
- `MinimumSpanningTree` returns the MST `Graph`; `Tree` is a vertex-rooted BFS/DFS traversal tree. `ShortestPath` returns a `topologic_core.Wire` whose hop length reads through the polymorphic `Topology.Vertices` accessor — the one vertex accessor over both graph nodes and path wires.
- `JSONData` returns the node-link `dict` the JSON codec encodes; `JSONString` returns the pre-serialized string.
- `Topology.ByIFCFile(file)` and `Topology.ByIFCPath(path)` return a per-product topology `list[Any]`, folded to one handle via `Cluster.ByTopologies` at the consumer, never a single `CellComplex`; all IFC intake gates against the C# `IfcSemanticModel`, so the Python side never owns IFC semantic identity.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): `ifcopenshell.open` yields the `ifcopenshell.file` that `Topology.ByIFCFile(file)` ingests into a per-product topology `list`, folded through `Cluster.ByTopologies`.
- `networkx`(`.api/networkx.md`): `Graph.JSONData` emits the node-link `dict` that `networkx.node_link_graph(data, directed=, multigraph=)` rebuilds for the centrality/community analytics the topologicpy `Graph` does not own.
- `trimesh`(`.api/trimesh.md`): `Topology.ExportToOBJ`/`ByMeshData` bridge topology to `trimesh.Trimesh(vertices=, faces=)` and onward to the `manifold3d` watertight-CSG kernel; non-manifold cells never enter the boolean path directly.
- within-lib: the non-manifold cell-complex owner composes `Topology.ByIFCFile`, the `Cell`/`CellComplex` `By*` constructors, and `Graph.ByTopology` into the `topology-graph` subject.

[LOCAL_ADMISSION]:
- `topologicpy` admits opt-in on the Forge lane only, excluded from the default server build: its `AGPL-3.0-or-later` network copyleft makes any network-exposed deployment that links it inherit the source-disclosure obligation. Its PyPI `License ::` classifier reads `GPLv3`, but the bundled `LICENSE` and `__init__.py` header bind AGPLv3-or-later, so the binding license is network copyleft.

[RAIL_LAW]:
- Package: `topologicpy`
- Owns: non-manifold cell/aperture topology modeling, polymorphic `Topology` analysis and boolean ops, graph construction and analytics, dictionary attributes, and vector/matrix/color algebra, gated against the C# `IfcSemanticModel`
- Accept: non-manifold topology and graph analysis feeding the geometry-algebra owner on the opt-in Forge lane
- Reject: wrapper-renames of `Topology.ByX`/`Graph.ByTopology`; a hand-rolled non-manifold boolean or graph-centrality kernel; a per-kind accessor/boolean family over the polymorphic `Topology` row; minting the IFC semantic identity the C# owner holds; linking into the default server build or any network-exposed deployment off the Forge lane
