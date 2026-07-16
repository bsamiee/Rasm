# [PY_ARTIFACTS_API_RUSTWORKX]

Full surface and stacking: `libs/python/data/.api/rustworkx.md` (data-tier canonical owner).

| [INDEX] | [SYMBOL]                                  | [PRODUCED_BY]                                                                |
| :-----: | :---------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `rx.Pos2DMapping`                         | the layout family — node-index -> `(x, y)` coordinate map                    |
|  [02]   | `rx.NodeIndices`                          | `add_nodes_from`/`topological_sort`/`isolates` — `Sequence[int]` join key    |
|  [03]   | `rx.EdgeList` / `rx.WeightedEdgeList`     | `edge_list()`/`weighted_edge_list()`/`dfs_edges()` — `Sequence[tuple]` rows  |
|  [04]   | `rx.NodeMap`                              | `subgraph_with_nodemap`/`substitute_node_with_subgraph` — old->new index map |
|  [05]   | `rx.BFSSuccessors` / `rx.BFSPredecessors` | `bfs_successors`/`bfs_predecessors` — `(node, [successors])` rows            |

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph containers
- rail: figure
- node/edge payloads are arbitrary Python objects (the artifacts owner stores the `data/tabular#GRAPH` row's node attribute on the node and the edge weight on the edge); integer indices are stable and never recycled after removal, so a `NodeIndices` value is a durable key the laid-out `Pos2DMapping`, the emitted `DiagramGlyph`, and a sibling node-attribute frame all share without re-keying

| [INDEX] | [TYPE]         | [KIND]           | [ROLE]                                                                                               |
| :-----: | :------------- | :--------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `rx.PyDiGraph` | directed         | `PyDiGraph(check_cycle=False, multigraph=True, ...)`; `_as_graph` build target + cross-reference DAG |
|  [02]   | `rx.PyGraph`   | undirected       | `PyGraph(multigraph=True, ...)`; undirected graph; `multigraph` gate, `degree`, `to_directed`        |
|  [03]   | `rx.PyDAG`     | directed acyclic | `PyDAG(PyDiGraph)` = `check_cycle=True`; rejects a cycle via `DAGWouldCycle`, not a post-hoc scan    |

[PUBLIC_TYPE_SCOPE]: graph-build + layout-relevant container members (`PyGraph` and `PyDiGraph`)
- rail: figure
- the artifacts owner uses the BUILD/QUERY/DERIVATION/IO members below; the mutation members beyond `add_*`/`contract_nodes` and the full edge-view family are documented in the data-plane catalog — here they are the construction surface a presentation graph is assembled and serialized through

- [01]-[BUILD]: `add_node(obj) -> int`, `add_nodes_from(obj_list) -> NodeIndices`, `add_edge(a, b, payload) -> int`, `add_edges_from(rows) -> list[int]`, `add_edges_from_no_data(pairs) -> list[int]`, `extend_from_edge_list(pairs)`, `extend_from_weighted_edge_list(rows)`, `contract_nodes(nodes, obj, weight_combo_fn=None) -> int`.
- [02]-[QUERY]: `node_indices() -> NodeIndices`, `edge_list() -> EdgeList`, `weighted_edge_list() -> WeightedEdgeList`, `num_nodes() -> int`, `num_edges() -> int`, `get_node_data(i) -> _S`, `get_edge_data(a, b) -> _T`, `has_edge(a, b) -> bool`, `has_parallel_edges() -> bool`, `neighbors(i) -> NodeIndices`, `attrs`.
- [03]-[DERIVATION]: `subgraph(nodes, preserve_attrs=False)`, `subgraph_with_nodemap(nodes, ...) -> (graph, NodeMap)`, `edge_subgraph(pairs)`, `filter_nodes(fn) -> NodeIndices`, `filter_edges(fn) -> EdgeIndices`, `copy() -> Self`.
- [04]-[DIGRAPH]: `successor_indices(i)`/`predecessor_indices(i) -> NodeIndices`, `successors(i)`/`predecessors(i) -> list[_S]`, `in_degree(i)`/`out_degree(i) -> int`, `reverse()`, `to_undirected(multigraph=True, weight_combo_fn=None)`; `check_cycle` is the `PyDAG` mutation gate.
- [05]-[IO]: `to_dot(node_attr=None, edge_attr=None, graph_attr=None, filename=None) -> str`, `from_adjacency_matrix(matrix, null_value=0.0)` (static), `read_edge_list(path, ...)` (static), `write_edge_list(path, ...)`.

[PUBLIC_TYPE_SCOPE]: layout/topology result carriers
- rail: figure
- the layout-relevant result carriers are mapping/sequence-like views over node/edge indices; they iterate and index without a Python re-materialization step, and a `Pos2DMapping`/`NodeIndices` lowers directly into the glyph-construction fold and the `numpy` array lane

| [INDEX] | [SYMBOL]                                  | [PRODUCED_BY]                                                                           |
| :-----: | :---------------------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `rx.Pos2DMapping`                         | the layout family — `Mapping[int, tuple[float, float]]` node-index -> `(x, y)`          |
|  [02]   | `rx.NodeIndices`                          | `add_nodes_from`/`topological_sort`/`dag_longest_path`/`isolates` — `Sequence[int]` key |
|  [03]   | `rx.EdgeList` / `rx.WeightedEdgeList`     | `edge_list()`/`weighted_edge_list()`/`dfs_edges()` — `Sequence[tuple[int, int(, _T)]]`  |
|  [04]   | `rx.NodeMap`                              | `subgraph_with_nodemap`/`substitute_node_with_subgraph` — `Mapping[int, int]` index map |
|  [05]   | `rx.BFSSuccessors` / `rx.BFSPredecessors` | `bfs_successors`/`bfs_predecessors` — `(node, [successors])` hierarchy rows             |

[PUBLIC_TYPE_SCOPE]: incremental DAG sorter + IO key typing + control exceptions
- rail: figure
- `TopologicalSorter` is the `graphlib`-shaped streaming sorter a staged delivery/plan-set assembly drives; `GraphMLKey`/`GraphMLDomain`/`GraphMLType` type a GraphML egress; the exceptions are the typed faults the `expression` Result rail folds, never a bare raise crossing the domain

| [INDEX] | [SYMBOL]                                                    | [KIND]    | [CONSUMER_NOTE]                                               |
| :-----: | :---------------------------------------------------------- | :-------- | :------------------------------------------------------------ |
|  [01]   | `rx.TopologicalSorter`                                      | sorter    | ready-set sorter — `get_ready()`/`done()`/`is_active()`       |
|  [02]   | `rx.GraphMLKey` / `rx.GraphMLDomain` / `rx.GraphMLType`     | IO key    | keys for `write_graphml`/`read_graphml` egress                |
|  [03]   | `rx.DAGWouldCycle` / `rx.DAGHasCycle`                       | exception | typed rejection of a cyclic `PyDAG` mutation                  |
|  [04]   | `rx.JSONSerializationError` / `rx.JSONDeserializationError` | exception | `node_link_json`/`parse_node_link_json` round-trip IO failure |
|  [05]   | `rx.InvalidNode` / `rx.NoEdgeBetweenNodes` / `rx.NullGraph` | exception | invalid index / missing edge / empty-graph precondition       |

- [01]-[SORTER]: `TopologicalSorter(dag, check_cycle, *, reverse=False, initial=None, check_args=True)` — the incremental ready-set loop a staged transmittal/plan-set assembly releases nodes through as dependencies complete.
- [02]-[GRAPHML]: the `id`/`domain`/`name`/`ty`/`default` attribute-key fields typing a GraphML egress.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: presentation-graph construction from the `data/tabular#GRAPH` adjacency frame
- rail: figure
- the layout owner's `_as_graph` fold: `add_nodes_from` over the node-attribute rows yields the stable index, `add_edges_from` over the `(source, target, weight)` rows yields the edges, so the presentation graph is built ONCE from the adjacency feed and never re-keyed; `from_adjacency_matrix` is the dense-matrix ingress and `networkx_converter` the one-way NetworkX bridge

| [INDEX] | [MEMBER]                                                  | [KIND]  | [ROLE]                                                            |
| :-----: | :-------------------------------------------------------- | :------ | :---------------------------------------------------------------- |
|  [01]   | `add_nodes_from` / `add_node`                             | build   | node-attribute rows; the returned stable index keys glyphs/coords |
|  [02]   | `add_edges_from`                                          | build   | `(source, target, payload)` edge rows (the adjacency edge feed)   |
|  [03]   | `add_edges_from_no_data` / `extend_from_edge_list`        | build   | unweighted `(source, target)` edges / `[None]`-payload extend     |
|  [04]   | `from_adjacency_matrix` / `from_complex_adjacency_matrix` | ingress | dense `npt.NDArray[np.float64]` matrix; complex variant           |
|  [05]   | `networkx_converter`                                      | ingress | one-way NetworkX -> rustworkx bridge (`keep_attributes=False`)    |
|  [06]   | `subgraph` / `subgraph_with_nodemap`                      | derive  | per-sheet slice; `subgraph_with_nodemap` -> `NodeMap`             |
|  [07]   | `contract_nodes`                                          | derive  | collapse a node cluster; fold weights via `weight_combo_fn`       |

[ENTRYPOINT_SCOPE]: coordinate layout -> `Pos2DMapping`
- rail: figure
- every layout function takes the built graph and returns a `Pos2DMapping` (node index -> `(x, y)`); they compute COORDINATES ONLY and feed the downstream glyph-construction fold, never render. The `LayoutPolicy` arms select among them: `Force` -> `spring_layout`, `Radial` -> `circular_layout`/`shell_layout`, and the deterministic layered fallback -> `topological_generations` (next scope). `seed=` makes a force layout reproducible for the content key

Every layout takes the built `graph` and returns a `Pos2DMapping`, threading a shared `scale=1, center=None`; the full verified signatures are keyed below the grid.

| [INDEX] | [MEMBER]                 | [ROLE]                                                                                               |
| :-----: | :----------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `rx.spring_layout`       | force-directed Fruchterman-Reingold placement minimizing edge crossing (circulation/adjacency kinds) |
|  [02]   | `rx.circular_layout`     | nodes on one ring (the sun-path / compass radial kinds)                                              |
|  [03]   | `rx.shell_layout`        | concentric shells from a node-partition list (`nlist`) — a multi-ring radial kind                    |
|  [04]   | `rx.spiral_layout`       | an Archimedean spiral placement (`resolution` tunes compactness)                                     |
|  [05]   | `rx.bipartite_layout`    | two-column placement keyed by a `first_nodes` set (a drawing<->spec cross-reference view)            |
|  [06]   | `rx.kamada_kawai_layout` | stress-majorization placement minimizing path-length distortion                                      |
|  [07]   | `rx.random_layout`       | uniform-random seed positions (a `spring_layout` `pos=` warm start)                                  |

- [01]-[SPRING]: `spring_layout(graph, pos=None, fixed=None, k=None, repulsive_exponent=2, adaptive_cooling=True, num_iter=50, tol=1e-6, weight_fn=None, default_weight=1, scale=1, center=None, seed=None)` — `seed=` fixes the coordinate map (and content key) reproducibly.
- [02]-[RADIAL]: `circular_layout(graph, scale=1, center=None)`; `shell_layout(graph, nlist=None, rotate=None, scale=1, center=None)`; `spiral_layout(graph, scale=1, center=None, resolution=..., equidistant=False)`.
- [03]-[STRESS]: `bipartite_layout(graph, first_nodes, horizontal=False, scale=1, center=None, aspect_ratio=...)`; `kamada_kawai_layout(graph, pos=None, fixed=None, weight_fn=None, default_weight=1, epsilon=1e-4, max_outer=500, max_inner=10, scale=1, center=None)`; `random_layout(graph, center=None, seed=None)`.

[ENTRYPOINT_SCOPE]: DAG ordering for AEC cross-reference dependency graphs
- rail: figure
- the layered no-route fallback and the AEC-documentation-plane dependency-ordering surface: a drawing<->spec cross-reference resolver, a detail-callout sheet cross-reference, and an ISO 19650 delivery/transmittal assembly fold a `PyDiGraph` of dependencies through these. `topological_generations` is the deterministic layered-coordinate source the `LayeredPolicy(engine=RUSTWORKX)` arm reads (depth = rank); `digraph_find_cycle` is the cross-reference-cycle detector

The full verified signatures are keyed below the grid.

| [INDEX] | [MEMBER]                                                      | [ROLE]                                                                  |
| :-----: | :------------------------------------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | `rx.topological_generations`                                  | per-generation node layers — the deterministic layered fallback         |
|  [02]   | `rx.topological_sort`                                         | a single linear dependency order (plan-set / spec-section assembly)     |
|  [03]   | `rx.lexicographical_topological_sort`                         | deterministic tie-broken topo order keyed by a `key` callback           |
|  [04]   | `rx.transitive_reduction`                                     | minimal dependency DAG + index map (the clean cross-reference graph)    |
|  [05]   | `rx.ancestors` / `rx.descendants`                             | upstream / downstream cross-reference closure of a sheet/detail/section |
|  [06]   | `rx.bfs_layers` / `rx.bfs_successors` / `rx.bfs_predecessors` | BFS dependency layers / `(node, [successors])` hierarchy rows           |
|  [07]   | `rx.digraph_find_cycle` / `rx.is_directed_acyclic_graph`      | first cross-reference cycle as an edge list / the acyclicity predicate  |
|  [08]   | `rx.TopologicalSorter`                                        | incremental ready-set sorter releasing nodes as dependencies complete   |
|  [09]   | `rx.dag_longest_path` / `rx.dag_weighted_longest_path`        | the single longest DAG chain: hop-count and edge-weighted forms         |

- [01]-[GENERATIONS]: `topological_generations(dag) -> list[NodeIndices]` maps rank (list position) to `TB`/`BT`/`LR`/`RL` coordinates for the `engine=RUSTWORKX` arm.
- [02]-[SORT]: `topological_sort(graph) -> NodeIndices`; `lexicographical_topological_sort(dag, /, key, *, reverse=False, initial=None) -> list[_S]` tie-broken by a node-attribute `key`.
- [03]-[REDUCE]: `transitive_reduction(graph) -> (PyDiGraph, dict[int, int])`; `ancestors(graph, node) -> set[int]` / `descendants(graph, node) -> set[int]` (the full cross-reference closure).
- [04]-[BFS]: `bfs_layers(graph, sources=None) -> list[list[int]]`; `bfs_successors(graph, node)`; `bfs_predecessors(graph, node)`.
- [05]-[CYCLE]: `digraph_find_cycle(graph, source=None) -> EdgeList`; `is_directed_acyclic_graph(graph) -> bool`; `TopologicalSorter(dag, check_cycle, *, reverse=False, initial=None)` -> `.get_ready()`/`.done(nodes)`/`.is_active()`.
- [06]-[LONGEST]: `dag_longest_path(graph, weight_fn=None) -> NodeIndices`; `dag_weighted_longest_path(graph, weight_fn) -> NodeIndices` (`_length` variants return the scalar): the hop-count and EDGE-weighted forms; `core/plan#PLAN` names it to fix why the node-weighted per-node Critical-Path-Method slack memo stays own-code: it weighs EDGES and yields one path, carrying neither node weights nor the per-node total float the min-slack front order and the `critical` set read.

[ENTRYPOINT_SCOPE]: the content-key wire, round-trip, and DOT/preview egress
- rail: figure
- `node_link_json` is the canonical-topology serialization the `visualization/diagram/layout#LAYOUT` owner joins to the resolved coordinate + route maps as the `ContentIdentity` content-key input — supply the `node_attrs`/`edge_attrs` callbacks (the bare call emits a null `data` field that collapses two graphs differing only in a node label or an edge weight, both of which feed the emitted glyphs); `parse_node_link_json`/`from_node_link_json_file` are the round-trip; `to_dot`/`graphviz_draw` are the DOT/raster preview egress

| [INDEX] | [MEMBER]                           | [KIND]  | [ROLE]                                                                                   |
| :-----: | :--------------------------------- | :------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `rx.node_link_json`                | wire    | serialize to node-link JSON; `node_attrs`/`edge_attrs` carry payloads to the wire        |
|  [02]   | `rx.parse_node_link_json`          | wire    | parse node-link JSON back into a graph via the decode callbacks                          |
|  [03]   | `rx.from_node_link_json_file`      | wire    | round-trip a node-link JSON file from disk                                               |
|  [04]   | `PyDiGraph.to_dot` / `rx.from_dot` | egress  | emit / read Graphviz DOT (the draw.io / external-tool interchange)                       |
|  [05]   | `rx.visualization.graphviz_draw`   | egress  | a Graphviz-rendered `PIL.Image` preview: a fallback raster, not the bound `drawsvg` path |
|  [06]   | `rx.generators.{...}`              | fixture | synthetic graph constructors for layout/figure test fixtures                             |

- [01]-[WIRE]: `node_link_json(graph, path=None, graph_attrs=None, node_attrs=None, edge_attrs=None) -> str | None` — `node_attrs`/`edge_attrs` (`Callable[[_S], dict[str, str]]`) push node/edge payloads onto the wire so two glyph-distinct graphs key distinctly; `path=` writes a file and returns `None`, omit it for the string.
- [02]-[PARSE]: `parse_node_link_json(data, graph_attrs=None, node_attrs=None, edge_attrs=None) -> PyDiGraph | PyGraph` (callbacks decode `dict[str, str]` into typed objects); `from_node_link_json_file(path, node_attrs=None, edge_attrs=None, ...)`.
- [03]-[EGRESS]: `to_dot(node_attr=None, edge_attr=None, graph_attr=None, filename=None) -> str` / `from_dot(dot_str)`; `visualization.graphviz_draw(graph, node_attr_fn=None, edge_attr_fn=None, graph_attr=None, filename=None, image_type=None, method=None) -> PIL.Image` (`image_type` in `{'svg','png','pdf',...}`, `method` in `{'dot','neato','circo','fdp','sfdp','twopi'}`).
- [04]-[FIXTURE]: `generators.{directed_path_graph, directed_star_graph, complete_graph, grid_graph, full_rary_tree, binomial_tree_graph, ...}` — the `directed_*` mirrors return a `PyDiGraph`.

## [03]-[IMPLEMENTATION_LAW]

- import: `import rustworkx as rx` at boundary scope inside the `to_thread.run_sync` kernel; the visitor bases are `from rustworkx import visit` only if an event-stepped hierarchy walk is needed (the analysis-plane pattern — the artifacts layout owner does NOT subclass visitors). The version is `importlib.metadata.version("rustworkx")` or `rx.__version__`; the abi3 wheel is the same on every interpreter.
- scope axis: artifacts reaches rustworkx for THREE concerns only — presentation-graph BUILD, coordinate LAYOUT, and the content-key/DAG-order WIRE. The graph-ANALYSIS kernel (shortest-path, centrality, coloring, matching, cut, isomorphism, bisimulation, community) is the `data/graph#GRAPH` owner's (`libs/python/data/.api/rustworkx.md`); a second analysis kernel inside a figure owner is the deleted form. The two `.api` tiers split by concern, never overlap.
- dispatch axis: the polymorphic bare-name forms (`spring_layout`, `topological_sort`, `node_link_json`, `union`) dispatch on graph type — route through the bare name; bind a `graph_*`/`digraph_*` type form only inside a single-graph-type kernel. A per-graph-type layout method family where the bare form covers both is the deleted form.
- build axis: `_as_graph` folds the `data/tabular#GRAPH` adjacency frame into a `PyDiGraph` ONCE — `add_nodes_from` over the node-attribute rows (the returned stable index is the glyph/coordinate key), `add_edges_from` over the `(source, target, weight)` rows — so the presentation graph is built from the feed and never re-keyed. Integer index stability with no recycling after removal makes a `NodeIndices` value a durable join key the resolved `Pos2DMapping` and the emitted `DiagramGlyph` both read; coordinates are never re-keyed away from it. An empty adjacency frame returns the empty result before construction (a layout over zero nodes is degenerate).
- layout axis: the `LayoutPolicy` `match` selects the layout function — `Force(seed, iterations)` -> `spring_layout(graph, num_iter=iterations, seed=seed)`, `Radial(scale)` -> `circular_layout(graph, scale=scale)`/`shell_layout`, `Layered(engine=RUSTWORKX, direction)` -> `topological_generations(graph)` mapped to per-generation depth/rank coordinates, `Layered(engine=GRANDALF)` -> the `grandalf` `SugiyamaLayout` sibling — every arm returning a `LayoutMap`. The layout functions compute coordinates only (a `Pos2DMapping`), never SVG; a hand-rolled spring/Fruchterman-Reingold loop where `spring_layout` owns it is the deleted form. A force layout's `seed=` is fixed so the coordinate map (and thus the content key) is reproducible.
- dag axis: the AEC-documentation cross-reference owners (drawing<->spec resolver, detail-callout sheet cross-references, ISO 19650 register/transmittal) fold a `PyDiGraph` of dependencies through the DAG surface — `transitive_reduction` for the minimal cross-reference graph, `topological_sort`/`lexicographical_topological_sort` for the assembly order, `ancestors`/`descendants` for the cross-reference closure, `TopologicalSorter` for staged release, `digraph_find_cycle`/`is_directed_acyclic_graph` for the cycle gate. A cross-reference owner that must reject a cycle at authoring time constructs a `PyDAG` (`check_cycle=True`) and folds `DAGWouldCycle` as the typed rejection, never a post-hoc scan.
- content-key axis: `node_link_json(graph, node_attrs=_wire_node, edge_attrs=_wire_edge)` is the canonical-topology wire the CONSUMING producer (`visualization/diagram/draw#DRAW`, the detail/register DAG owners) folds into ITS pre-run `ContentIdentity.key` seed — the layout owner mints no key and no receipt — and the `node_attrs`/`edge_attrs` callbacks are MANDATORY because the bare call emits a null `data` field that collapses two graphs differing only in a node label or edge weight (both of which feed the emitted glyphs), so two glyph-distinct graphs must serialize distinctly. Two layouts of one graph under distinct `LayoutPolicy` values also key distinctly because the coordinate/route maps join the topology wire — a diagram is content-addressable on its full laid-out form, not its bare topology.
- evidence: the CONSUMING diagram producer mints one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm, bytes)` case — flat scalars: the `kind` the `DiagramKind` value (or a delivery/register descriptor), the `nodes`/`edges` the built-graph `num_nodes()`/`num_edges()` extents, the `algorithm` the matched `LayoutPolicy.tag` (`"force"`/`"radial"`/`"layered"`) or the DAG-op name, the `key` the producer's own pre-run mint — contributed through the existing `ReceiptContributor` port as ONE case on the tagged union, never a parallel diagram receipt rail and never a layout-owned mint (`visualization/diagram/layout#LAYOUT` owns no receipt and no content key).
- boundary: rustworkx (artifacts) owns presentation-graph BUILD, coordinate LAYOUT, DAG-ORDER for cross-reference dependency graphs, and the `node_link_json` content-key WIRE; the graph-ANALYSIS kernel stays the `data/graph#GRAPH` owner's; `grandalf` owns the layered Sugiyama placement + bounding-box routing for the `engine=GRANDALF` arm (`grandalf.md`); `visualization/diagram/glyphset#GLYPHSET` owns the `DiagramGlyph`/`DiagramKind`/`GlyphStyle` shapes; `visualization/diagram/draw#DRAW` owns SVG emission off the positioned marks via `drawsvg`; `graphic/color/derive#DERIVE` owns the palette the `GlyphStyle` indices key; the `data/tabular#GRAPH` adjacency frame is the BYODF ingress; native orthogonal/port/nesting layout is `pyelk`'s and constraint-solver layout is `kiwisolver`'s. The `visualization.graphviz_draw`/`mpl_draw` bundled drawers are a preview fallback, never the bound figure renderer.

[STACKING]:
- `expression` Result/`beartype`: the figure/AEC owner wraps the build+layout+serialize kernel in a `RuntimeRail[...]` (`expression`-folded `Result`) — a `NullGraph` on an empty feed, a `DAGWouldCycle` on a cross-reference cycle, or a `JSONSerializationError` on the content-key wire folds to a typed rail fault, never a raised rustworkx exception crossing the domain; the boundary signature is `@beartype`-validated so a wrong-shaped adjacency row is rejected at the seam, not deep in the Rust core.
- `msgspec`/`pydantic` discriminated receipt: the diagram facts (the matched `LayoutPolicy.tag` as algorithm, the built-graph `num_nodes()`/`num_edges()`, the `DiagramKind`/register descriptor, the `ContentIdentity` key) populate one `ArtifactReceipt.Diagram` `msgspec.Struct`/`pydantic` case — flat typed scalars contributed through the `core/receipt#RECEIPT` `ReceiptContributor` port as ONE more case on the tagged union, the same family the `data/graph#GRAPH` analysis receipt contributes a different case to, never a parallel rail.
- `numpy`: `Pos2DMapping` is `__array__`-convertible and `from_adjacency_matrix(matrix)`/`adjacency_matrix(graph, ...)` exchange a dense `npt.NDArray[np.float64]` with the shared `libs/python/.api/numpy.md` substrate rustworkx already depends on — a coordinate map crosses into the numeric lane for a bounding-box/extent computation and a numpy adjacency matrix crosses back into a graph, one array contract, no second copy.
- `anyio` structured concurrency + `stamina`/`structlog`/OpenTelemetry: the synchronous native build+layout+DAG-order+serialize is one `_render` kernel the owner crosses through its instance `lane.offload(..., modality=Modality.THREAD)` seam — the GIL-releasing Rust core off the event loop, the THREAD arm (not the subinterpreter) chosen because the kernel touches the isolate-unsafe `numpy`/`msgspec` C-extensions — with capacity, retry, and worker-death policy owned by the runtime `LanePolicy` row, emitted as a `structlog` event inside an OpenTelemetry span; the same shared observability rails (`libs/python/.api/{anyio,stamina,structlog,opentelemetry-api}.md`) every artifacts owner stacks, the rustworkx layout call slotting in as the worker body.
- `visualization/diagram/layout#LAYOUT` + `grandalf` (folder `.api`): `_as_graph` folds the `data/tabular#GRAPH` adjacency frame into a `PyDiGraph`, `_position` discriminates the `LayoutPolicy` (`Force` -> `spring_layout`, `Radial` -> `circular_layout`/`shell_layout`, `Layered(engine=RUSTWORKX)` -> `topological_generations`, `Layered(engine=GRANDALF)` -> the `grandalf` `SugiyamaLayout` arm reading `vertex.view.xy`/`edge.view._pts`), and `node_link_json` is the content-key wire — rustworkx the presentation-graph builder + force/radial/topological provider + canonical-topology wire, `grandalf` only the layered placement provider behind the one `LayoutPolicy` `match`, never two parallel layout owners.
- `visualization/diagram/draw#DRAW` + `export/layered#LAYERED`: the positioned `DiagramGlyph` sequence the layout emits from the `Pos2DMapping`/`topological_generations` coordinates threads into the draw owner, which folds the marks to a per-layer `drawsvg.Drawing` and contributes the `ArtifactReceipt.Diagram` case off its glyph tallies while the layered-export owner contributes the named-layer `Preview`/`Egress` evidence — one receipt family, the layout's node/edge counts and the draw owner's glyph counts both as `Diagram` cases; the layout content key (`ContentIdentity` over the `node_link_json` wire) and the draw content key (`ContentIdentity` over the joined per-layer SVG bytes) are the two `runtime` `ContentIdentity` keys of the one diagram.
- AEC-documentation dependency-DAG seam: a `delivery/register`/`delivery/transmittal`, a `specification/classify` drawing<->spec resolver, and a `drawing/detail` sheet cross-reference owner build a `PyDiGraph` of container/sheet/section/detail dependencies and fold it through `transitive_reduction`/`topological_sort`/`ancestors`/`descendants`/`TopologicalSorter` to order the plan-set assembly and resolve the cross-reference closure, contributing the delivery/register `ArtifactReceipt` case (not a `Diagram` case) — rustworkx holds only the dependency-graph algebra, never the ISO 19650 metadata authority or the CSI classification tables those owners hold.

## [04]-[LOCAL_ADMISSION]

- Package: `rustworkx`
- Status: admitted through the data-tier canonical catalog; this overlay scopes the artifacts figure + AEC-documentation BUILD/LAYOUT/DAG-ORDER/WIRE surface.
- Owns (artifacts scope): the presentation/dependency graph CONTAINERS (`PyGraph`/`PyDiGraph`/`PyDAG` with stable non-recycled integer indices and arbitrary node/edge payloads); the BUILD surface (`add_nodes_from`/`add_edges_from`/`add_edges_from_no_data`/`extend_from_edge_list`/`from_adjacency_matrix`/`networkx_converter`/`subgraph`/`subgraph_with_nodemap`/`contract_nodes`/`node_indices`/`edge_list`/`weighted_edge_list`); the coordinate LAYOUT family (`spring_layout`/`circular_layout`/`shell_layout`/`spiral_layout`/`bipartite_layout`/`kamada_kawai_layout`/`random_layout` -> `Pos2DMapping`); the DAG-ORDER surface for cross-reference dependency graphs (`topological_sort`/`topological_generations`/`lexicographical_topological_sort`/`transitive_reduction`/`ancestors`/`descendants`/`bfs_layers`/`bfs_successors`/`TopologicalSorter`/`dag_longest_path`/`dag_weighted_longest_path`/`digraph_find_cycle`/`is_directed_acyclic_graph`); the content-key + round-trip WIRE (`node_link_json`/`parse_node_link_json`/`from_node_link_json_file`) and the `to_dot`/`from_dot`/`graphviz_draw` DOT/preview egress; the `generators` synthetic-fixture submodule
- Accept: a presentation graph built ONCE from the `data/tabular#GRAPH` adjacency feed with the stable node index as the shared coordinate/glyph key; polymorphic bare-name dispatch over `graph_*`/`digraph_*` forms; coordinate layout (`Pos2DMapping`) behind the one `LayoutPolicy` owner with `grandalf` the layered sibling; the DAG-order surface for the AEC drawing<->spec / detail / ISO 19650 cross-reference dependency owners with `PyDAG`/`DAGWouldCycle` the mutation-time cycle rejection; `node_link_json` with `node_attrs`/`edge_attrs` supplied as the `ContentIdentity` content-key wire; typed result carriers (`Pos2DMapping`/`NodeIndices`/`EdgeList`/`NodeMap`) as receipts; the native build+layout off-loaded onto `to_thread.run_sync` under a `CapacityLimiter`; one `ArtifactReceipt.Diagram` (or delivery/register) case on the shared receipt family
- Reject: a second graph-ANALYSIS kernel (centrality/shortest-path/coloring/matching/cut/isomorphism/community) where the `data/graph#GRAPH` owner holds it — that is the no-overlap line between the two `.api` tiers; a per-graph-type or per-diagram-kind layout method family where the bare polymorphic form discriminates; a hand-rolled spring/Fruchterman-Reingold or Sugiyama loop where `spring_layout`/`grandalf` own them; a re-keying of layout coordinates away from the stable `rustworkx` node index; a bare `node_link_json` (null `data`) content-key call that collapses glyph-distinct graphs; a post-hoc cross-reference cycle scan where `PyDAG`/`check_cycle` rejects at mutation; a synchronous native layout left on the event loop; the `visualization.graphviz_draw`/`mpl_draw` bundled drawers as the bound figure renderer where `drawsvg`/`visualization/diagram/draw#DRAW` owns SVG emission; rustworkx as the ISO 19650 metadata or CSI classification authority where the `delivery/*` and `specification/*` owners hold those; re-exporting rustworkx result carriers through thin rename wrappers
