# [PY_ARTIFACTS_API_RUSTWORKX]

`rustworkx` (artifacts overlay) owns presentation-graph BUILD, coordinate LAYOUT, DAG-ORDER for AEC cross-reference dependency graphs, and the `node_link_json` content-key WIRE. Graph-ANALYSIS — path, centrality, coloring, matching, cut, isomorphism, community — stays the `data/graph#GRAPH` owner's, the two `.api` tiers splitting by concern; `libs/python/data/.api/rustworkx.md` carries the full Rust-core surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rustworkx`
- package: `rustworkx` (Apache-2.0)
- module: `import rustworkx as rx`; `rustworkx.visualization` / `rustworkx.generators` submodules
- owner: `artifacts`
- rail: figure
- role: figure + AEC-documentation overlay of the data-tier canonical surface — BUILD, LAYOUT, DAG-ORDER, WIRE
- abi: abi3 wheel, interpreter-independent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph containers — `_as_graph` build targets over arbitrary node/edge payloads with stable, never-recycled integer indices, so a `NodeIndices` value is the durable key the resolved `Pos2DMapping`, the emitted `DiagramGlyph`, and a sibling node-attribute frame all share unre-keyed

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]    | [CAPABILITY]                                                                         |
| :-----: | :------------- | :--------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `rx.PyDiGraph` | directed         | `_as_graph` build target and cross-reference DAG; `check_cycle` / `multigraph` gates |
|  [02]   | `rx.PyGraph`   | undirected       | `multigraph` parallel-edge gate, `degree`, `to_directed`                             |
|  [03]   | `rx.PyDAG`     | directed acyclic | `PyDiGraph` with `check_cycle=True`; rejects a cycle at mutation via `DAGWouldCycle` |

[PUBLIC_TYPE_SCOPE]: `PyGraph` / `PyDiGraph` instance-method surface a presentation graph is built, queried, derived, and serialized through; the full mutation and edge-view families are the data-tier owner's
- [01]-[BUILD]: `add_node(obj)`, `add_nodes_from(objs)`, `add_edge(a, b, payload)`, `add_edges_from(rows)`, `add_edges_from_no_data(pairs)`, `extend_from_edge_list(pairs)`, `extend_from_weighted_edge_list(rows)`, `contract_nodes(nodes, obj, weight_combo_fn=None)`
- [02]-[QUERY]: `node_indices()`, `edge_list()`, `weighted_edge_list()`, `num_nodes()`, `num_edges()`, `get_node_data(i)`, `get_edge_data(a, b)`, `has_edge(a, b)`, `has_parallel_edges()`, `neighbors(i)`, `attrs`
- [03]-[DERIVATION]: `subgraph(nodes, preserve_attrs=False)`, `subgraph_with_nodemap(nodes)`, `edge_subgraph(pairs)`, `filter_nodes(fn)`, `filter_edges(fn)`, `copy()`
- [04]-[DIGRAPH]: `successor_indices(i)` / `predecessor_indices(i)`, `successors(i)` / `predecessors(i)`, `in_degree(i)` / `out_degree(i)`, `reverse()`, `to_undirected(multigraph=True, weight_combo_fn=None)`
- [05]-[IO]: `to_dot(node_attr=None, edge_attr=None, graph_attr=None, filename=None)`, `read_edge_list(path)` (static), `write_edge_list(path)`

[PUBLIC_TYPE_SCOPE]: layout / topology result carriers — mapping and sequence views over node/edge indices that iterate and index without a Python re-materialization, lowering directly into the glyph fold and the `numpy` array lane

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :---------------------------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `rx.Pos2DMapping`                         | mapping       | node index -> `(x, y)`; the layout family produces it                       |
|  [02]   | `rx.NodeIndices`                          | sequence      | `add_nodes_from` / `topological_sort` / `dag_longest_path` / `isolates` key |
|  [03]   | `rx.EdgeList` / `rx.WeightedEdgeList`     | sequence      | `edge_list()` / `weighted_edge_list()` / `dfs_edges()` `(int, int(, _T))`   |
|  [04]   | `rx.NodeMap`                              | mapping       | `subgraph_with_nodemap` / `substitute_node_with_subgraph` old->new map      |
|  [05]   | `rx.BFSSuccessors` / `rx.BFSPredecessors` | sequence      | `bfs_successors` / `bfs_predecessors` `(node, [successors])` rows           |

[PUBLIC_TYPE_SCOPE]: incremental DAG sorter, GraphML egress keys, and the typed faults the `expression` Result rail folds instead of a bare raise crossing the domain
- call: `TopologicalSorter(dag, check_cycle, *, reverse=False, initial=None, check_args=True)` — ready-set loop; `.get_ready()` / `.done(nodes)` / `.is_active()`

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :---------------------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `rx.TopologicalSorter`                                      | sorter        | `graphlib`-shaped streaming ready-set sorter            |
|  [02]   | `rx.GraphMLKey` / `rx.GraphMLDomain` / `rx.GraphMLType`     | IO key        | `id`/`domain`/`name`/`ty`/`default` GraphML keys        |
|  [03]   | `rx.DAGWouldCycle` / `rx.DAGHasCycle`                       | exception     | cyclic `PyDAG` mutation rejection                       |
|  [04]   | `rx.JSONSerializationError` / `rx.JSONDeserializationError` | exception     | `node_link_json` / `parse_node_link_json` round-trip    |
|  [05]   | `rx.InvalidNode` / `rx.NoEdgeBetweenNodes` / `rx.NullGraph` | exception     | invalid index / missing edge / empty-graph precondition |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: presentation-graph construction from the `data/tabular#GRAPH` adjacency frame — the container build and derive members are `[02]`; the module-level ingress functions below feed the `_as_graph` fold

| [INDEX] | [SURFACE]                                           | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `from_adjacency_matrix(matrix, null_value=0.0)`     | static  | dense `npt.NDArray[np.float64]` matrix ingress |
|  [02]   | `from_complex_adjacency_matrix(matrix, null_value)` | static  | complex-weighted dense-matrix ingress          |
|  [03]   | `networkx_converter(graph, keep_attributes=False)`  | static  | one-way NetworkX -> rustworkx bridge           |

[ENTRYPOINT_SCOPE]: coordinate layout -> `Pos2DMapping` — the `LayoutPolicy` arms select among these layout functions, each taking the built graph and returning node index -> `(x, y)`
- shared carry: `scale=1`, `center=None`
- call: `spring_layout(graph, pos, fixed, k, repulsive_exponent, adaptive_cooling, num_iter, tol, weight_fn, default_weight, scale, center, seed)`
- call: `shell_layout(graph, nlist, rotate, scale, center)`; `spiral_layout(graph, scale, center, resolution, equidistant)`
- call: `bipartite_layout(graph, first_nodes, horizontal, scale, center, aspect_ratio)`; `kamada_kawai_layout(graph, pos, fixed, weight_fn, default_weight, epsilon, max_outer, max_inner, scale, center)`

| [INDEX] | [SURFACE]                | [SHAPE] | [CAPABILITY]                                                           |
| :-----: | :----------------------- | :------ | :--------------------------------------------------------------------- |
|  [01]   | `rx.spring_layout`       | fold    | force-directed Fruchterman-Reingold placement minimizing edge crossing |
|  [02]   | `rx.circular_layout`     | fold    | nodes on one ring (sun-path / compass radial kinds)                    |
|  [03]   | `rx.shell_layout`        | fold    | concentric shells from a node-partition list (`nlist`)                 |
|  [04]   | `rx.spiral_layout`       | fold    | Archimedean spiral (`resolution` tunes compactness)                    |
|  [05]   | `rx.bipartite_layout`    | fold    | two-column placement keyed by a `first_nodes` set                      |
|  [06]   | `rx.kamada_kawai_layout` | fold    | stress-majorization minimizing path-length distortion                  |
|  [07]   | `rx.random_layout`       | fold    | uniform-random seed positions (a `spring_layout` `pos=` warm start)    |

[ENTRYPOINT_SCOPE]: DAG ordering for AEC cross-reference dependency graphs — a drawing<->spec resolver, a detail-callout sheet cross-reference, and an ISO 19650 delivery/transmittal assembly fold a `PyDiGraph` of dependencies through these; `topological_generations` is the deterministic layered-coordinate source the `Layered(engine=RUSTWORKX)` arm reads, depth is rank
- call: `lexicographical_topological_sort(dag, /, key, *, reverse=False, initial=None)` — tie-broken by a node-attribute `key`
- call: `dag_longest_path(graph, weight_fn=None)` / `dag_weighted_longest_path(graph, weight_fn)` — hop-count and edge-weighted single longest chain, `_length` variants return the scalar

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------------------- | :------ | :---------------------------------------------------------- |
|  [01]   | `topological_generations(dag)`                                  | fold    | per-generation node layers -> rank / depth coordinates      |
|  [02]   | `topological_sort(graph)`                                       | fold    | single linear dependency order (plan-set / spec-section)    |
|  [03]   | `lexicographical_topological_sort`                              | fold    | deterministic tie-broken topo order (signature above)       |
|  [04]   | `transitive_reduction(graph)`                                   | fold    | minimal dependency DAG plus `dict[int, int]` index map      |
|  [05]   | `ancestors(graph, node)` / `descendants(graph, node)`           | fold    | upstream / downstream cross-reference closure as `set[int]` |
|  [06]   | `bfs_layers(graph, sources=None)`                               | fold    | BFS dependency layers                                       |
|  [07]   | `bfs_successors(graph, node)` / `bfs_predecessors(graph, node)` | fold    | `(node, [successors])` hierarchy rows                       |
|  [08]   | `digraph_find_cycle(graph, source=None)`                        | fold    | first cross-reference cycle as an `EdgeList`                |
|  [09]   | `is_directed_acyclic_graph(graph)`                              | fold    | acyclicity predicate                                        |

- `dag_weighted_longest_path` weighs EDGES and yields one path, carrying neither node weights nor per-node total float, so the `core/plan#PLAN` min-slack Critical-Path-Method front order and `critical` set stay own-code.

[ENTRYPOINT_SCOPE]: content-key wire, round-trip, and DOT / preview egress — `node_link_json` is the canonical-topology serialization the consuming producer folds into its `ContentIdentity` content-key input
- call: `node_link_json(graph, path=None, graph_attrs=None, node_attrs=None, edge_attrs=None)` — `path=` writes a file and returns `None`, omit for the string
- call: `graphviz_draw(graph, node_attr_fn=None, edge_attr_fn=None, graph_attr=None, filename=None, image_type=None, method=None)` -> `PIL.Image`; `image_type` in `{svg, png, pdf}`, `method` in `{dot, neato, circo, fdp, sfdp, twopi}`

| [INDEX] | [SURFACE]                                         | [SHAPE] | [CAPABILITY]                                                            |
| :-----: | :------------------------------------------------ | :------ | :---------------------------------------------------------------------- |
|  [01]   | `rx.node_link_json`                               | static  | serialize to node-link JSON; `node_attrs` / `edge_attrs` carry payloads |
|  [02]   | `rx.parse_node_link_json`                         | static  | parse node-link JSON back into a graph via the decode callbacks         |
|  [03]   | `rx.from_node_link_json_file`                     | static  | round-trip a node-link JSON file from disk                              |
|  [04]   | `PyDiGraph.to_dot` / `rx.from_dot`                | egress  | emit / read Graphviz DOT (draw.io / external-tool interchange)          |
|  [05]   | `rx.visualization.graphviz_draw`                  | egress  | Graphviz-rendered `PIL.Image` fallback raster, not the bound `drawsvg`  |
|  [06]   | `rx.generators.{grid_graph, full_rary_tree, ...}` | static  | synthetic layout / figure fixtures; `directed_*` return a `PyDiGraph`   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Import at boundary scope inside the `to_thread.run_sync` offload kernel; this owner never subclasses the `rustworkx.visit` bases — event-stepped walks are the data plane.
- artifacts reaches rustworkx for BUILD, LAYOUT, DAG-ORDER, and the content-key WIRE only; the graph-ANALYSIS suite is the `data/graph#GRAPH` owner's and a second analysis kernel here is the deleted form.
- Polymorphic bare-name forms (`spring_layout`, `topological_sort`, `node_link_json`, `union`) dispatch on graph type; a `graph_*` / `digraph_*` form binds only inside a single-graph-type kernel.
- `_as_graph` folds the adjacency frame into a `PyDiGraph` once and the stable node index is the join key the resolved `Pos2DMapping` and the emitted `DiagramGlyph` both read; an empty frame returns the empty result before construction.
- Every layout computes a `Pos2DMapping` (coordinates only, never SVG); the `LayoutPolicy` match selects the arm and a force `seed=` fixes the coordinate map, and thus the content key.
- An AEC cross-reference owner rejecting a cycle at authoring time constructs a `PyDAG` (`check_cycle=True`) and folds `DAGWouldCycle`, never a post-hoc scan.
- `node_link_json` supplies `node_attrs` / `edge_attrs` so two glyph-distinct graphs serialize distinctly; two layouts of one graph under distinct `LayoutPolicy` values key distinctly because the coordinate and route maps join the topology wire, so a diagram is content-addressable on its full laid-out form.

[STACKING]:
- `expression` / `beartype`(`libs/python/.api/{expression,beartype}.md`): the figure/AEC owner wraps build+layout+serialize in a `RuntimeRail[...]` (`expression`-folded `Result`) so a `NullGraph`, `DAGWouldCycle`, or `JSONSerializationError` folds to a typed rail fault, and the `@beartype`-validated boundary rejects a wrong-shaped adjacency row at the seam, not deep in the Rust core.
- `msgspec` / `pydantic`(`libs/python/.api/{msgspec,pydantic}.md`): the diagram facts (matched `LayoutPolicy.tag` as algorithm, `num_nodes()` / `num_edges()`, the `DiagramKind` or register descriptor, the `ContentIdentity` key) populate one `ArtifactReceipt.Diagram` case contributed through the `core/receipt#RECEIPT` `ReceiptContributor` port — the same tagged union the `data/graph#GRAPH` analysis receipt contributes a different case to, never a parallel rail.
- `numpy`(`libs/python/.api/numpy.md`): `Pos2DMapping` is `__array__`-convertible and `from_adjacency_matrix` / `adjacency_matrix` exchange a dense `npt.NDArray[np.float64]` with the shared substrate — a coordinate map crosses into the numeric lane for a bounding-box extent and a numpy matrix crosses back into a graph, one array contract with no second copy.
- `anyio` / `stamina` / `structlog` / OpenTelemetry(`libs/python/.api/{anyio,stamina,structlog,opentelemetry-api}.md`): the synchronous native build+layout+DAG-order+serialize is one `_render` kernel crossed through the instance `lane.offload(Kernel.of(..., KernelTrait.RELEASING))` seam — the RELEASING trait (not the subinterpreter) because the kernel touches the isolate-unsafe `numpy` / `msgspec` C-extensions — with capacity and deadline owned by the runtime `LanePolicy` and emitted as a `structlog` event inside an OpenTelemetry span.
- `visualization/diagram/layout#LAYOUT` + `grandalf`(`.api/grandalf.md`): `_as_graph` folds the adjacency frame into a `PyDiGraph` and `_position` discriminates the `LayoutPolicy` — `Force` -> `spring_layout`, `Radial` -> `circular_layout` / `shell_layout`, `Layered(engine=RUSTWORKX)` -> `topological_generations`, `Layered(engine=GRANDALF)` -> the `grandalf` `SugiyamaLayout` arm — with `node_link_json` the content-key wire; rustworkx is the builder, the force/radial/topological provider, and the wire, `grandalf` only the layered arm.
- `visualization/diagram/draw#DRAW` + `export/layered#LAYERED`: the positioned `DiagramGlyph` sequence off the `Pos2DMapping` / `topological_generations` coordinates threads into the draw owner, which folds the marks to a per-layer `drawsvg.Drawing` and contributes the `ArtifactReceipt.Diagram` case off its glyph tallies while the layered-export owner contributes the named-layer `Preview` / `Egress` evidence — the two `ContentIdentity` keys of one diagram are the `node_link_json` topology wire and the joined per-layer SVG bytes.
- AEC-documentation dependency-DAG seam: a `delivery/register` / `delivery/transmittal`, a `specification/classify` drawing<->spec resolver, and a `drawing/detail` sheet cross-reference owner build a `PyDiGraph` of container/sheet/section/detail dependencies and fold it through `transitive_reduction` / `topological_sort` / `ancestors` / `descendants` / `TopologicalSorter`, contributing the delivery/register `ArtifactReceipt` case; rustworkx holds only the dependency-graph algebra, never the ISO 19650 metadata or CSI classification authority those owners hold.

[LOCAL_ADMISSION]:
- Admitted through the data-tier canonical catalog and registered for artifacts, never re-catalogued; this overlay scopes only the figure and AEC-documentation BUILD/LAYOUT/DAG-ORDER/WIRE surface, the graph-ANALYSIS kernel staying the `data/graph#GRAPH` owner's as the no-overlap line between the two tiers.

[RAIL_LAW]:
- Package: `rustworkx`
- Owns: the figure and AEC presentation/dependency graph containers, the container BUILD and derive surface, the coordinate LAYOUT family returning `Pos2DMapping`, the DAG-ORDER surface for cross-reference dependency graphs, the `node_link_json` content-key and round-trip WIRE, and the DOT / preview egress.
- Accept: a presentation graph built once from the adjacency feed with the stable node index as the shared coordinate and glyph key; polymorphic bare-name dispatch; coordinate layout behind the one `LayoutPolicy` owner with `grandalf` the layered sibling; the DAG-order surface with `PyDAG` / `DAGWouldCycle` the mutation-time cycle rejection; `node_link_json` with `node_attrs` / `edge_attrs` as the `ContentIdentity` wire; typed result carriers as receipts; the native build and layout off-loaded onto `to_thread.run_sync` under a `CapacityLimiter`; one `ArtifactReceipt.Diagram` or delivery/register case on the shared receipt family.
- Reject: a second graph-ANALYSIS kernel where `data/graph#GRAPH` holds it; a per-graph-type or per-diagram-kind layout family where the bare polymorphic form discriminates; a hand-rolled spring/Fruchterman-Reingold or Sugiyama loop where `spring_layout` / `grandalf` own them; a re-keying of layout coordinates away from the stable node index; a bare `node_link_json` call whose null `data` collapses glyph-distinct graphs; a post-hoc cross-reference cycle scan where `PyDAG` / `check_cycle` rejects at mutation; a synchronous native layout left on the event loop; the `visualization.graphviz_draw` / `mpl_draw` bundled drawers as the bound figure renderer where `visualization/diagram/draw#DRAW` owns SVG emission; rustworkx as the ISO 19650 or CSI classification authority; re-exporting result carriers through thin rename wrappers.
