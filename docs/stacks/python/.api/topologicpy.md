# [PY_GEOMETRY_API_TOPOLOGICPY]

`topologicpy` supplies the non-manifold topology modeling surface for the geometry-algebra rail: stateless static-method facades over `topologic_core` C++ handles for the `Vertex` through `CellComplex` topology hierarchy, the central polymorphic `Topology` owner, a `Graph` analysis facade, `Dictionary` attribute carriers, `Vector`/`Matrix`/`Color` algebra, and BIM integration facades (`IFC`, `Honeybee`, `Speckle`). The package owner composes `Topology.ByIFCFile`, the `Cell`/`CellComplex` constructors, and `Graph.ByTopology` into the topology owner gated against the C# `IfcSemanticModel` seam; it never re-implements the non-manifold boolean kernel or graph analytics `topologic_core` and `networkx` already own. It is a LICENSE-GATED row: `topologicpy` is `AGPL-3.0-or-later` (network copyleft) — any network-exposed deployment that links it triggers AGPL source-disclosure obligations, so it MUST be excluded from the default server build and admitted opt-in on the Forge lane only; the spine graph/non-manifold path stays AGPL-free. Replacement of the AGPL dependency is mandated by card `geometry [GEO_TOPOLOGICPY_LICENSE_REPLACE]` (reference only).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `topologicpy`
- package: `topologicpy`
- import: `import topologicpy`
- owner: `geometry`
- rail: geometry-algebra (LICENSE-GATED, opt-in / Forge-lane only)
- license: `AGPL-3.0-or-later` (own) — NETWORK COPYLEFT; pure-Python distribution riding the `topologic-core` native floor, but the AGPL source-disclosure trigger fires for any network-accessible service that links it. The package MUST be excluded from the default server build and is admitted opt-in on the Forge lane only; replacement is mandated by card `geometry [GEO_TOPOLOGICPY_LICENSE_REPLACE]`. The upstream PyPI `License ::` classifier is mislabeled `GPLv3`, but the bundled `LICENSE` text and `__init__.py` header are AGPLv3 "or any later version" — treat the binding license as AGPL network-copyleft
- installed: `0.9.50` (dist-info metadata `Requires-Python: <3.15,>=3.8`, `License: AGPL v3 License`, `topologic_core>=7.0.1`). Static-method counts confirmed by AST parse of the `0.9.50` per-facade source; `topologic_core` is the native floor a live import requires but the pure-Python facade surface introspects without it
- requires-python: `>=3.8,<3.15` (declared in package metadata) — a hard upper bound. The wheel is `py3-none-any`, but the `Requires-Python: <3.15` ceiling makes the cp315 project venv refuse the install at resolution time regardless of ABI tag; opt-in installs land on a companion cp313/cp314 interpreter, not cp315
- build exclusion: the default server build excludes `topologicpy`; opt-in installs gate behind the Forge lane and assume the AGPL obligation. Two gates apply — the `AGPL-3.0-or-later` network-copyleft license AND the `<3.15` requires-python ceiling (no cp315 admission until the upstream bound lifts)
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
|  [02]   | `Dictionary`   |        29 | key/value attribute attachment and boolean ops    |
|  [03]   | `Vector`       |        42 | vector algebra, azimuth/altitude, compass         |
|  [04]   | `Plotly`       |        26 | figure construction for topologies and graphs     |
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
|  [01]   | `Topology.ByIFCFile(file: ifcopenshell.file)` | construct | in-memory `ifcopenshell.file` object to non-manifold topology `list[Any]` |
|  [02]   | `Topology.ByBREPString(string)`      | construct      | OpenCASCADE BREP intake               |
|  [03]   | `Topology.ByOBJPath(path)`           | construct      | Wavefront OBJ intake                  |
|  [04]   | `Topology.ByGeometry(vertices, ...)` | construct      | raw vertex/edge/face array intake     |
|  [05]   | `Topology.ByMeshData(mesh)`          | construct      | mesh-dict intake                      |
|  [06]   | `Topology.ByOCCTShape(shape)`        | construct      | OpenCASCADE handle intake             |
|  [07]   | `Topology.ByJSONString(string)`      | construct      | JSON topology intake                  |
|  [08]   | `Topology.ExportToBREP(topology)`    | export         | BREP string/file output               |
|  [09]   | `Topology.ExportToOBJ(topology)`     | export         | OBJ output                            |
|  [10]   | `Topology.ExportToJSON(topology)`    | export         | JSON topology output                  |
|  [11]   | `Topology.ByIFCPath(path)`           | construct      | IFC file-path intake to non-manifold topology `list[Any]` |

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
|  [09]   | `Cluster.ByTopologies(*topologies)`   | construct      | non-manifold `Cluster` from a topology list |
|  [10]   | `Graph.ByTopology(topology)`          | construct      | graph from topology adjacency         |
|  [11]   | `Graph.ShortestPath(graph, a, b)`     | analysis       | geodesic `Wire` between two vertices   |
|  [12]   | `Graph.BetweennessCentrality(graph)`  | analysis       | per-vertex betweenness score list      |
|  [13]   | `Graph.ClosenessCentrality(graph)`    | analysis       | per-vertex closeness score list        |
|  [14]   | `Graph.DegreeCentrality(graph)`       | analysis       | per-vertex degree score list           |
|  [15]   | `Graph.ConnectedComponents(graph)`    | analysis       | island sub-graphs, vertex-count sorted |
|  [16]   | `Graph.MinimumSpanningTree(graph)`    | analysis       | minimum spanning tree `Graph`          |
|  [17]   | `Graph.Edges(graph)`                  | accessor       | constituent edge handles               |
|  [18]   | `Graph.JSONData(graph)`               | export         | node-link `dict` for JSON encode       |
|  [19]   | `Dictionary.ByKeysValues(keys, vals)` | construct      | attribute dictionary                  |
|  [20]   | `Dictionary.ValueAtKey(dict, key)`    | accessor       | read an attribute value               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY_ALGEBRA]:
- import: per-class `from topologicpy.Topology import Topology` at boundary scope only; module-level import is banned by the manifest import policy.
- facade axis: every class is a stateless static-method namespace over `topologic_core` C++ handles; there is no instance constructor pattern. `Topology.ByX(...)` returns a handle of `topologic_core` type that the next static call consumes, so state lives in the handle, never the facade.
- ownership axis: `Topology` is the central polymorphic owner discriminating on the runtime handle kind; `HighestType`/`Type`/`TypeAsString` report the kind, and accessors (`Cells`, `Faces`, `Vertices`) plus boolean ops (`Union`, `Difference`, `Intersect`, `Slice`) work across every topology kind without a per-kind function family.
- construction axis: each sub-topology facade exposes a `By*` constructor family building from the next-lower topology (`Vertex` -> `Edge` -> `Wire` -> `Face` -> `Shell` -> `Cell` -> `CellComplex`); shape primitives (`Wire.Rectangle`, `Cell.Box`, `Cell.Sphere`) are static factories on the owning facade.
- attribute axis: `Dictionary.ByKeysValues` builds attribute carriers attached via `Topology.AddDictionary`; `Graph.ByTopology` lifts adjacency into a `Graph` whose 179 methods own centrality, shortest path, connectivity, and partition analytics.
- graph analytics axis: the centrality verbs `BetweennessCentrality`/`ClosenessCentrality`/`DegreeCentrality` each return a per-vertex score `list` ordered to the graph vertices (NOT values mutated in place), so a leaderboard fold ranks the list directly. `Connectivity` is an ALIAS for `DegreeCentrality` (space-syntax connectivity score), never a component count — the island count is `len(Graph.ConnectedComponents(graph))`, whose return is the list of component sub-graphs sorted by vertex count. `MinimumSpanningTree` returns the MST `Graph`; `Tree` is a vertex-rooted BFS/DFS traversal tree, a different structure. `ShortestPath` returns a `topologic_core.Wire`, so its hop length reads through the polymorphic `Topology.Vertices` accessor (documented over `Topology`, `Graph`, OR `TGraph`), which is the one vertex accessor across both the graph-node count and the path-wire count — there is no separate gated `Graph.Vertices` the analytics rail needs. `JSONData` returns the node-link `dict` the JSON codec encodes; `JSONString` returns the pre-serialized string.
- IFC-gated rail: `Topology.ByIFCFile(file: ifcopenshell.file)` ingests an in-memory `ifcopenshell.file` object and returns a per-product topology `list[Any]` (folded to one non-manifold handle via `Cluster.ByTopologies` at the consumer), while `Topology.ByIFCPath(path)` ingests a path string and the `IFC` facade ingests IFC geometry; all are gated against the C# `IfcSemanticModel` seam so the Python topology side never owns IFC semantic identity. The live return is a per-product topology list, not a single `CellComplex`.
- boundary: topologicpy owns non-manifold topology and graph analytics; watertight mesh CSG routes to `manifold3d`, triangle-mesh exchange to `trimesh`, OpenNURBS exchange to `rhino3dm`, and IFC semantic identity to the C# `IfcSemanticModel`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `topologicpy`
- License gate: `AGPL-3.0-or-later` network copyleft (PyPI classifier mislabeled `GPLv3`; bundled license text is AGPLv3-or-later) — admitted opt-in on the Forge lane only and EXCLUDED from the default server build; any network-exposed deployment linking it inherits the AGPL source-disclosure obligation. Replacement of the AGPL dependency is mandated by card `geometry [GEO_TOPOLOGICPY_LICENSE_REPLACE]` (reference only)
- Owns: non-manifold cell/aperture topology modeling, polymorphic `Topology` analysis and boolean ops, graph construction and analytics, dictionary attributes, and vector/matrix/color algebra gated against the C# `IfcSemanticModel`
- Accept: non-manifold topology and graph analysis feeding the geometry-algebra owner on the opt-in Forge lane, under the AGPL obligation
- Reject: wrapper-renames of `Topology.ByX`/`Graph.ByTopology`; a hand-rolled non-manifold boolean or graph-centrality kernel where topologicpy is admitted; a per-kind accessor/boolean family over the polymorphic `Topology` row; minting IFC semantic identity the C# owner holds; linking `topologicpy` into the default server build or any network-exposed deployment outside the opt-in Forge lane

[CAPTURE_GAP]:
- floor: `topologicpy` is an undeclared, LICENSE-GATED candidate riding the `topologic-core` native floor (declared `topologic-core>=7.0.1`; cp313 introspection ran against `8.0.4`). The wheel is pure-Python `py3-none-any`, but the package declares `Requires-Python: >=3.8,<3.15`, so it carries a hard cp315 ceiling: the cp315 project venv cannot resolve it on requires-python grounds, and opt-in installs land on a cp313/cp314 companion interpreter. Two gates apply — the `AGPL-3.0-or-later` network-copyleft license and the `<3.15` requires-python ceiling
- members: every documented facade, integration module, and `Topology`/`Graph` entrypoint resolves against the `0.9.50` source (`Topology.By*`/`Export*`/accessors/boolean, the `Vertex`->`CellComplex` `By*` constructor chain, the graph analytics surface `Graph.ByTopology`/`ShortestPath`/`BetweennessCentrality`/`ClosenessCentrality`/`DegreeCentrality`/`Connectivity`/`ConnectedComponents`/`MinimumSpanningTree`/`Tree`/`Edges`/`JSONData`/`JSONString` and the polymorphic `Topology.Vertices` graph-or-topology accessor, `Dictionary.ByKeysValues`/`ValueAtKey`); the `IFC`/`Honeybee`/`Speckle`/`EnergyModel`/`Neo4j`/`Kuzu`/`Ontology`/`GraphRAG`/`Polyskel` modules are present as source files. Static-method counts are AST-confirmed against `0.9.50`, and every graph member's exact `def` and documented return shape is line-confirmed in the `0.9.50` `Graph.py` source — no phantom
