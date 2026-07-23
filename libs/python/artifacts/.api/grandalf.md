# [PY_ARTIFACTS_API_GRANDALF]

`grandalf` owns the pure-Python hierarchical (Sugiyama layered "dot") graph-drawing surface behind one `HierarchyEngine` arm: a `Graph`/`graph_core` topology algebra, a `SugiyamaLayout` assigning layered coordinates by Brandes-Köpf, and a `route_with_*` family clamping edge endpoints to node bounding boxes. It lays out through a caller-supplied `view` provider without graphviz, a native dependency, or permanent topology mutation; the owner reads `view.xy` and `view._pts` back at the boundary, so raw grandalf vertices, edges, and viewers never cross into glyph or receipt shapes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grandalf`
- package: `grandalf` (`GPLv2 | EPLv1`, dual; the EPLv1 arm is the commercial-safe selection)
- module: `grandalf` (`grandalf.graphs` / `grandalf.layouts` / `grandalf.routing` / `grandalf.utils`)
- owner: `artifacts`
- rail: figure (diagram layout)
- abi: pure-Python, no native extension, ABI-agnostic across 3.15
- depends: `pyparsing` (dot tokenizer); `numpy`/`ply` are the `[full]` extra only — `geometry` falls back to the bundled `linalg` `array`/`matrix` without numpy and `Dot` degrades (`_has_ply=False`) without ply, so the layered+routing path needs neither

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph topology layer (`grandalf.graphs`)

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]      | [CAPABILITY]                                                                               |
| :-----: | :------------ | :----------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Vertex`      | node               | `Vertex(data=None)`; `.data` payload, `.e` edge list, `.c` component back-ref, `.index`    |
|  [02]   | `Edge`        | link               | `Edge(x, y, w=1, data=None, connect=False)`; `.w` weight, `.feedback` flag, `.v` endpoints |
|  [03]   | `graph_core`  | connected graph    | `graph_core(V=None, E=None, directed=True)`; `.sV`/`.sE` posets, `.degenerated_edges`      |
|  [04]   | `Graph`       | disjoint-set graph | `Graph(V=None, E=None, directed=True)`; `.C` components; merges as edges connect           |
|  [05]   | `vertex_core` | node base          | adjacency essentials (`deg`/`e_in`/`e_out`/`N`/`e_to`/`detach`) — `Vertex` superclass      |
|  [06]   | `edge_core`   | link base          | `edge_core(x, y)`; `.v` endpoint pair, `.deg` (0 for a self-loop) — `Edge` superclass      |

[PUBLIC_TYPE_SCOPE]: layout providers (`grandalf.layouts`)

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [CAPABILITY]                                                                                |
| :-----: | :---------------------- | :--------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `SugiyamaLayout`        | layered provider | `SugiyamaLayout(graph_core)`; layered dot — rank, order, Brandes-Köpf xy, routing           |
|  [02]   | `VertexViewer`          | shape provider   | `VertexViewer(w=2, h=2, data=None)`; default node dimension/position view (`.w`/`.h`/`.xy`) |
|  [03]   | `DigcoLayout`           | energy provider  | `DigcoLayout(graph_core)`; force/stress-majorization (DIG-CoLa) placement                   |
|  [04]   | `DwyerLayout`           | energy provider  | `DwyerLayout(graph_core)`; `DigcoLayout` subclass carrying separation constraints           |
|  [05]   | `Layer`                 | layered state    | `Layer(list)`; one rank's vertex list; `order()`/`_cc()` crossing reduction (internal)      |
|  [06]   | `DummyVertex`           | routing control  | `DummyVertex(r=None, viewclass=VertexViewer)`; control point per inner rank of a long edge  |
|  [07]   | `_sugiyama_vertex_attr` | layout attr      | per-vertex `rank`/`pos`/`x`/`bar` state held in `SugiyamaLayout.grx` (layout-internal)      |

[PUBLIC_TYPE_SCOPE]: edge-routing + view providers (`grandalf.routing`)

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `EdgeViewer`                 | edge view     | `setpath(pts)` stores polyline on `._pts`; `.head_angle`/`.splines` |
|  [02]   | `route_with_lines`           | router        | clamp endpoints to node bb, straight polyline                       |
|  [03]   | `route_with_splines`         | router        | line route + round-corner cubic Béziers on `view.splines`           |
|  [04]   | `route_with_rounded_corners` | router        | line route + distance-rounded corners (`ROUND_AT_DISTANCE=40`)      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graph construction + analysis (`grandalf.graphs`)

Rows `[01]`-`[05]` are `Graph.<name>`, `[06]`-`[14]` `graph_core.<name>` (prefix dropped in the cells). `get_scs_with_feedback` is the Tarjan feedback-arc detector `SugiyamaLayout.init_all` calls to acyclic-ize; `dijkstra`/`path`/`partition` stay layout-internal, centrality and force analysis being `rustworkx`'s at the data plane.

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                                               |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------------------------------------- |
|  [01]   | `Graph(V=None, E=None, directed=True)`           | build    | disjoint-set graph; `.C` components; `.C[0]` is the `SugiyamaLayout` input |
|  [02]   | `add_vertex(v)` / `add_edge(e)`                  | build    | add a vertex / edge, merging components                                    |
|  [03]   | `remove_vertex(v)` / `remove_edge(e)`            | build    | mutate topology, merging/splitting components as connectivity changes      |
|  [04]   | `V()` / `E()` / `order()` / `norm()`             | query    | iterate vertices/edges; vertex / edge counts                               |
|  [05]   | `connected()` / `components()`                   | query    | connectivity test; component list                                          |
|  [06]   | `graph_core(V=None, E=None, directed=True)`      | build    | one connected component; raises `ValueError` on an unconnected vertex/edge |
|  [07]   | `V(cond=None)` / `E(cond=None)` / `N(v, f_io=0)` | query    | filtered vertex/edge iterators; neighbors (`f_io>0` out, `<0` in)          |
|  [08]   | `roots()` / `leaves()`                           | query    | vertices with no inward / no outward edges (Sugiyama rank-0 seeds)         |
|  [09]   | `get_scs_with_feedback(roots=None)`              | analysis | Tarjan SCCs; marks `Edge.feedback=True` on the feedback-arc set (O(V+E))   |
|  [10]   | `path(x, y, f_io=0, hook=None)`                  | analysis | BFS shortest path as a `Vertex` list; `hook` per visited vertex            |
|  [11]   | `dijkstra(x, f_io=0, hook=None, subset=None)`    | analysis | weighted single-source shortest-path distance map (heap PQ)                |
|  [12]   | `partition()`                                    | analysis | partition the component into rank-respecting vertex lists                  |
|  [13]   | `M(cond=None)` / `deg_min()` / `deg_max()`       | query    | associativity matrix; min / max degree                                     |
|  [14]   | `deg_avg()` / `eps()`                            | query    | average degree; graph density                                              |

[ENTRYPOINT_SCOPE]: `SugiyamaLayout` layered drawing (`grandalf.layouts`)

Fixed order: `SugiyamaLayout(component)`, bind `route_edge`, `init_all()`, `draw()`. `draw_step`/`ordering_step` are animation/debug iterators only.
- call: `init_all(roots=None, inverted_edges=None, optimize=False)` — rank vertices, build dummies + layers; `optimize=True` pushes long edges down

| [INDEX] | [SURFACE]                                        | [SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :----------------------------------------------- | :-------- | :-------------------------------------------------------------------- |
|  [01]   | `SugiyamaLayout(g)`                              | construct | layered layout; default `dw`/`dh` from view medians                   |
|  [02]   | `init_all(...)`                                  | init      | rank vertices, build dummies/layers (signature in the call above)     |
|  [03]   | `draw(N=1.5)`                                    | run       | converge ordering `N` rounds, Brandes-Köpf xy, route edges            |
|  [04]   | `route_edge`                                     | bind      | router slot; `route_with_lines`/`route_with_splines` pre-`draw()`     |
|  [05]   | `setxy()`                                        | run       | Brandes-Köpf xy assignment writing `view.xy` (4-pass median)          |
|  [06]   | `draw_edges()`                                   | run       | build dummy-vertex polyline, invoke `route_edge`, `setpath`           |
|  [07]   | `dirvh` / `dirv` / `dirh`                        | param     | alignment-policy state (0-3); the 4 Brandes-Köpf passes sweep all     |
|  [08]   | `xspace` / `yspace` / `dw` / `dh` / `order_iter` | param     | intra/inter-layer spacing, default node w/h, ordering count           |
|  [09]   | `draw_step()` / `ordering_step(oneway=False)`    | iter      | per-layer step iterators for animation/debug only (not production)    |
|  [10]   | `grx` / `layers` / `ctrls`                       | state     | per-vertex attrs, the `Layer` list, edge→dummy control map (internal) |

[ENTRYPOINT_SCOPE]: `DigcoLayout` energy/stress placement (`grandalf.layouts`)

Force and radial placement bind to `rustworkx`; `DigcoLayout` is grandalf's own energy/stress-majorization (DIG-CoLa) provider, catalogued off this owner's path — `init_all(alpha, beta)` partitions the directed graph into levels and seeds positions, `draw(N)` minimizes stress by conjugate-gradient Laplacian solving and writes `vertex.view.xy`, running on the bundled `linalg` fallback.

| [INDEX] | [SURFACE]                                    | [SHAPE]   | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------- | :-------- | :-------------------------------------------------------------- |
|  [01]   | `DigcoLayout(g)`                             | construct | energy/stress-majorization placement over a `graph_core`        |
|  [02]   | `DigcoLayout.init_all(alpha=0.1, beta=0.01)` | init      | partition directed graph into levels, seed positions            |
|  [03]   | `DigcoLayout.draw(N=None)`                   | run       | conjugate-gradient stress minimization; writes `vertex.view.xy` |
|  [04]   | `DigcoLayout.xspace` / `yspace` / `dr`       | param     | spacing and node-radius drawing parameters                      |
|  [05]   | `DwyerLayout(g)`                             | construct | `DigcoLayout` subclass adding separation constraints            |

[ENTRYPOINT_SCOPE]: edge routing + geometry helpers (`grandalf.routing` / `grandalf.utils.geometry`)

Each `route_with_*(e, pts)` clamps the polyline tail/head to node bounding boxes (`intersectR`), sets `edge.view.head_angle`, and — for the spline/rounded variants — rewrites the corner sequence over the geometry primitives (`setcurve` the NURBS-book local cubic-Bézier interpolation). `RouteMap` reads the final `edge.view._pts`.

| [INDEX] | [SURFACE]                                               | [SHAPE] | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------ | :------ | :----------------------------------------------------------------- |
|  [01]   | `route_with_lines(e, pts)`                              | route   | clamp endpoints to node bb, straight polyline                      |
|  [02]   | `route_with_splines(e, pts)`                            | route   | line route + `setroundcorner` cubic Béziers on `view.splines`      |
|  [03]   | `route_with_rounded_corners(e, pts)`                    | route   | line route + iterative distance-rounding (`ROUND_AT_DISTANCE`)     |
|  [04]   | `intersectR(view, topt)`                                | geom    | node bounding box vs line to `topt` (endpoint clamp)               |
|  [05]   | `intersectC(view, r, topt)`                             | geom    | circular node radius `r` vs line to `topt`                         |
|  [06]   | `setcurve(e, pts, tgs=None)` / `setroundcorner(e, pts)` | geom    | local cubic-Bézier spline through points / rounded-corner list     |
|  [07]   | `getangle(p1, p2)`                                      | geom    | polyline segment angle                                             |
|  [08]   | `new_point_at_distance(pt, distance, angle)`            | geom    | point advanced by a distance along an angle                        |
|  [09]   | `median_wh(views)` / `rand_ortho1(n)`                   | geom    | median node w/h (Sugiyama sizing); random unit vector perp (1,…,1) |

[ENTRYPOINT_SCOPE]: interop + support utilities (`grandalf.utils`)

`Poset` is the deterministic ordered set backing `graph_core.sV`/`sE`, keeping layout reproducible. `networkx` converters and the `Dot` `.dot` parser are alternative ingress paths this owner leaves unbound, ingesting from `rustworkx`.

| [INDEX] | [SURFACE]                                     | [SHAPE] | [CAPABILITY]                                                           |
| :-----: | :-------------------------------------------- | :------ | :--------------------------------------------------------------------- |
|  [01]   | `Poset(L)`                                    | support | ordered set backing `graph_core.sV`/`sE`; `add`/`remove`/`get`/`index` |
|  [02]   | `convert_nextworkx_graph_to_grandalf(G)`      | interop | build a grandalf `Graph` from a networkx graph                         |
|  [03]   | `convert_grandalf_graph_to_networkx_graph(G)` | interop | export a grandalf graph to a networkx `MultiDiGraph`                   |
|  [04]   | `Dot` (`grandalf.utils.dot`)                  | interop | LALR(1) graphviz `.dot` parser (`ply` `[full]` extra; `_has_ply` gate) |
|  [05]   | `array` / `matrix` (`grandalf.utils.linalg`)  | support | numpy-free linear-algebra fallback `geometry` uses when numpy absent   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Build `GrandalfVertex(node_index)` per `rustworkx` node and `GrandalfEdge(src, dst)` per edge, taking `GrandalfGraph(vertices, edges).C[0]` as the `SugiyamaLayout` component; the stable `rustworkx` integer node index lives on `Vertex.data`, the one key resolved coordinates and emitted glyph marks share and never re-key. An empty graph returns `({}, {})` before construction, a `graph_core` over zero vertices being degenerate.
- Every vertex carries `view = VertexViewer(w, h)` and every edge `view = EdgeViewer()` before layout; `draw()` reads `view.w`/`view.h` for spacing and writes `view.xy` and `view.setpath(pts)`, and the owner reads `vertex.view.xy` into `LayoutMap` and `edge.view._pts` into `RouteMap` through `float(...)`, so raw grandalf vertices, edges, and viewers never cross into glyph or receipt shapes.
- Import at boundary scope inside the offload kernel; alias `Vertex`/`Edge`/`Graph` to `GrandalfVertex`/`GrandalfEdge`/`GrandalfGraph` so the names never collide with the canonical `glyphset` shapes.

[STACKING]:
- `rustworkx`(`.api/rustworkx.md`): grandalf ingests a `PyDiGraph` — `GrandalfVertex(i)` per `graph.node_indices()`, `GrandalfEdge(src, dst)` per `graph.edge_list()` — and its laid-out coordinates and routed edge points join the `rustworkx` `node_link_json` wire as the `ContentIdentity` content-key input, so two layouts of one graph under distinct `LayoutPolicy` values key distinctly and the diagram is content-addressable on its full laid-out form.
- `anyio`(`libs/python/.api/anyio.md`): the synchronous `init_all()`/`draw()` runs under `to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)` bounded by `CapacityLimiter(os.process_cpu_count())`, wrapped by the `runtime` `async_boundary` returning a `RuntimeRail[...]`, so a `ValueError` on an unconnected component surfaces as a typed rail fault and a many-node layered diagram never blocks the event loop.
- `visualization/diagram/layout#LAYOUT`: `_grandalf_layout(graph, policy)` is the one layered arm of the `LayoutPolicy` `match` — `VertexViewer(48, 32)`, `EdgeViewer()`, `route_edge` bound by `_grandalf_router(policy.route)`, `init_all()`+`draw()`, reading `view.xy` into `LayoutMap` and `view._pts` into `RouteMap`; the positioned `DiagramGlyph` sequence threads into `visualization/diagram/draw#DRAW` for SVG emission, both owners contributing one `ArtifactReceipt.Diagram` receipt case off their node/edge and glyph tallies.

[LOCAL_ADMISSION]:
- grandalf is admitted as the `HierarchyEngine.GRANDALF` layered provider behind the one `LayoutPolicy` owner, reached only by the `Layered(engine=GRANDALF)` `match` arm; its layered-engine standing — default, fallback, retirement — is decided at `visualization/diagram/layout#LAYOUT` `HierarchyEngine`.

[RAIL_LAW]:
- Package: `grandalf`
- Owns: the pure-Python layered (Sugiyama "dot") engine for the `HierarchyEngine.GRANDALF` arm — `Graph`/`graph_core` topology with Tarjan feedback-arc detection, `SugiyamaLayout` rank/order/Brandes-Köpf xy assignment, `DummyVertex` long-edge control points, and the `route_with_lines`/`route_with_splines` bounding-box routers, read through `VertexViewer`/`EdgeViewer` into the local `LayoutResult`.
- Accept: layered coordinate assignment and line/spline edge routing for the stacking and program-flow AEC diagram kinds, behind the one `LayoutPolicy` owner, with the `rustworkx` node index as the shared key and the native draw off-loaded onto `to_thread.run_sync`.
- Reject: a hand-rolled Sugiyama or orthogonal-routing loop inside the grandalf arm, `EdgeRoute.ORTHOGONAL` binding `pyelk`; a re-keying of layout coordinates away from the `rustworkx` node index; raw grandalf vertices, edges, or viewers crossing into glyph or receipt shapes; grandalf as a content-key or receipt source where `rustworkx` `node_link_json` and `runtime` `ContentIdentity` own it; the `Dot`/`networkx` ingress adapters or the `[full]` `numpy`/`ply` extras where `rustworkx` ingress and the bundled `linalg` fallback serve this owner.
