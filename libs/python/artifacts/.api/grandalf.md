# [PY_ARTIFACTS_API_GRANDALF]

`grandalf` supplies the pure-Python hierarchical (Sugiyama / layered "dot") graph-drawing engine the AEC-diagram coordinate-assignment owner composes for its stacking and program-flow kinds: a `graph_core`/`Graph` topology layer (vertices, weighted edges, Tarjan feedback-arc detection, Dijkstra, BFS path, connected-component partitioning), a `SugiyamaLayout` that ranks vertices into layers, inserts `DummyVertex` control points for long edges, reduces bilayer crossings (Barth-Mutzel insert-sort counting + median ordering), and assigns x/y by the Brandes-Köpf horizontal-compaction algorithm, plus a `route_with_*` edge-routing family that adjusts endpoints to node bounding boxes and rounds polyline corners with NURBS-local cubic Béziers. The package is `visualization/diagram/layout#LAYOUT`'s admitted `HierarchyEngine.GRANDALF` provider behind one local `LayoutPolicy` owner — `rustworkx` (`libs/python/data`) remains the presentation-graph builder, force/radial/topological provider, and `node_link_json` content-key wire; `grandalf` is reached only through aliased `Vertex`/`Edge`/`Graph` construction, `VertexViewer`/`EdgeViewer` shape providers, `SugiyamaLayout` + `route_edge`, and the read of `vertex.view.xy` / `edge.view._pts` into the local `LayoutResult`. Raw grandalf objects never cross into glyph or receipt shapes, the native draw runs off-loop on `anyio.to_thread.run_sync` under a `CapacityLimiter`, and the package is **superseded-pending-removal**: `fast-sugiyama` (Rust/PyO3) replaces the placement core and `pyelk` owns native orthogonal/port/nesting layout once parity lands — grandalf stays only as the fallback layered engine until then. It has no native orthogonal router, so `EdgeRoute.ORTHOGONAL` currently degrades to `route_with_lines`; native orthogonal routing is `pyelk`/`libavoid`'s, never a hand-rolled grandalf loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grandalf`
- package: `grandalf`
- import: `grandalf` (submodules `grandalf.graphs` / `grandalf.layouts` / `grandalf.routing` / `grandalf.utils`)
- owner: `artifacts`
- rail: figure (diagram layout)
- license: `GPLv2 | EPLv1` (dual; the EPLv1 arm is the commercial-safe selection)
- installed: `0.8`
- python: pure-Python (no native extension, no abi/cp gate; ABI-agnostic across 3.15)
- dependencies: `pyparsing` (required, dot-grammar tokenizer); `numpy` + `ply` are the `[full]` extra only — `grandalf.utils.geometry` falls back to the bundled `grandalf.utils.linalg` `array`/`matrix` when numpy is absent, and `grandalf.utils.dot.Dot` degrades (`_has_ply=False`) when `ply` is absent, so the layered+routing path this owner uses needs neither
- entry points: none (library only)
- capability: build a directed graph from vertices and weighted edges; detect the feedback-arc set by Tarjan, rank vertices into layers, insert dummy control vertices for multi-rank edges, reduce bilayer crossings, and assign 2-D coordinates by the Brandes-Köpf algorithm (`SugiyamaLayout`); energy/stress-majorization placement (`DigcoLayout`/`DwyerLayout`); route an edge polyline to node bounding boxes with straight, spline-rounded, or distance-rounded corners; and read laid-out coordinates/paths off a user-supplied `view` provider — all without graphviz, without a C/C++ dependency, and without mutating the input graph topology permanently

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph topology layer (`grandalf.graphs`)
- rail: figure

`Vertex`/`Edge` are the node/link primitives carrying an arbitrary `.data` payload (the local owner stores the stable `rustworkx` integer node index here, the one key coordinates and glyphs share); `graph_core` is a single connected component and `Graph` is the disjoint-set container over `graph_core` components (`Graph(V, E).C[0]` is the component `SugiyamaLayout` consumes). The topology layer is the bounded algebra the layout reads — a layout op walks `graph_core.V()`/`E()`, never a re-emitted adjacency structure.

| [INDEX] | TYPE          | KIND               | ROLE                                                                          |
| :-----: | ------------- | ------------------ | ----------------------------------------------------------------------------- |
|  [01]   | `Vertex`      | node               | `Vertex(data=None)`; `.data` payload, `.e` edge list, `.c` component back-ref, `.index` |
|  [02]   | `Edge`        | link               | `Edge(x, y, w=1, data=None, connect=False)`; `.w` weight, `.feedback` Tarjan flag, `.v` endpoint pair |
|  [03]   | `graph_core`  | connected graph    | `graph_core(V=None, E=None, directed=True)`; one connected component; `.sV`/`.sE` posets, `.degenerated_edges` |
|  [04]   | `Graph`       | disjoint-set graph | `Graph(V=None, E=None, directed=True)`; `.C` list of `graph_core` components; merges components as edges connect |
|  [05]   | `vertex_core` | node base          | `vertex_core`; adjacency essentials (`deg`/`e_in`/`e_out`/`N`/`e_to`/`detach`) — `Vertex` superclass |
|  [06]   | `edge_core`   | link base          | `edge_core(x, y)`; `.v` endpoint pair, `.deg` (0 for a self-loop) — `Edge` superclass |

[PUBLIC_TYPE_SCOPE]: layout providers (`grandalf.layouts`)
- rail: figure

`SugiyamaLayout` is the layered "dot" provider this owner binds; `VertexViewer` is the default node-dimension/position provider attached to each vertex as `vertex.view` (the layout reads `view.w`/`view.h` and writes `view.xy`). `DigcoLayout` is the alternative energy/stress-majorization (DIG-CoLa) placement provider; `Layer`/`DummyVertex`/`_sugiyama_vertex_attr` are the layout's internal layered-state owners (a layered figure dispatches over `SugiyamaLayout`, never these internals).

| [INDEX] | TYPE                    | KIND              | ROLE                                                                         |
| :-----: | ----------------------- | ----------------- | ---------------------------------------------------------------------------- |
|  [01]   | `SugiyamaLayout`        | layered provider  | `SugiyamaLayout(graph_core)`; layered "dot" layout — rank, order, Brandes-Köpf xy, edge routing |
|  [02]   | `VertexViewer`          | shape provider    | `VertexViewer(w=2, h=2, data=None)`; default node dimension/position view (`.w`/`.h`/`.xy`) |
|  [03]   | `DigcoLayout`           | energy provider   | `DigcoLayout(graph_core)`; force/stress-majorization (DIG-CoLa) constrained placement |
|  [04]   | `DwyerLayout`           | energy provider   | `DwyerLayout(graph_core)`; subclass of `DigcoLayout` (placement parity; reserved for constraint extension) |
|  [05]   | `Layer`                 | layered state     | `Layer(list)`; one rank's ordered vertex list; `order()`/`_cc()` crossing reduction (layout-internal) |
|  [06]   | `DummyVertex`           | routing control   | `DummyVertex(r=None, viewclass=VertexViewer)`; control point inserted per inner rank for a long edge |
|  [07]   | `_sugiyama_vertex_attr` | layout attr       | per-vertex `rank`/`pos`/`x`/`bar` state held in `SugiyamaLayout.grx` (layout-internal) |

[PUBLIC_TYPE_SCOPE]: edge-routing + view providers (`grandalf.routing`)
- rail: figure

`EdgeViewer` is the default edge-view provider attached as `edge.view`; its `setpath(pts)` stores the routed polyline on `edge.view._pts` (the exact attribute the local owner reads into `RouteMap`). The `route_with_*` functions are bound to `SugiyamaLayout.route_edge` and invoked per edge during `draw()`; they adjust the endpoint to each node's bounding box and optionally round corners.

| [INDEX] | TYPE/MEMBER                  | KIND          | ROLE                                                                  |
| :-----: | ---------------------------- | ------------- | --------------------------------------------------------------------- |
|  [01]   | `EdgeViewer`                 | edge view     | `EdgeViewer()`; `setpath(pts)` stores routed polyline on `._pts`; `.head_angle`/`.splines` set by routers |
|  [02]   | `route_with_lines`           | router        | `route_with_lines(e, pts)`; clamp endpoints to node bb, straight polyline, set `head_angle` |
|  [03]   | `route_with_splines`         | router        | `route_with_splines(e, pts)`; line route + NURBS-local round-corner cubic Béziers on `view.splines` |
|  [04]   | `route_with_rounded_corners` | router        | `route_with_rounded_corners(e, pts)`; line route + custom distance-rounded corners (`ROUND_AT_DISTANCE=40`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graph construction + analysis (`graph_core` / `Graph`)
- rail: figure

`Graph(V, E)` ingests `Vertex`/`Edge` lists and unions vertices into connected components, exposed as `.C`; `graph_core` is one component with the full analysis surface. The local owner builds `GrandalfVertex(node_index)` per `rustworkx` node, `GrandalfEdge(src, dst)` per edge, then takes `Graph(...).C[0]` as the `SugiyamaLayout` input. `get_scs_with_feedback` is the Tarjan strongly-connected-component + feedback-arc detector `SugiyamaLayout.init_all` calls internally to acyclic-ize the graph; `dijkstra`/`path`/`partition` are the analysis members (force/radial/centrality analysis is `rustworkx`'s at the data plane — these stay layout-internal).

| [INDEX] | MEMBER                                                              | KIND     | ROLE                                                                 |
| :-----: | ------------------------------------------------------------------ | -------- | ------------------------------------------------------------------- |
|  [01]   | `Graph(V=None, E=None, directed=True)`                             | build    | disjoint-set graph; `.C` connected components; `.C[0]` is the `SugiyamaLayout` input |
|  [02]   | `Graph.add_vertex(v)` / `add_edge(e)` / `remove_vertex(v)` / `remove_edge(e)` | build | mutate topology, merging/splitting components as connectivity changes |
|  [03]   | `Graph.V()` / `E()` / `order()` / `norm()` / `connected()` / `components()` | query | iterate vertices/edges; counts; connectivity; component list         |
|  [04]   | `graph_core(V=None, E=None, directed=True)`                        | build    | one connected component; raises `ValueError` on an unconnected vertex/edge |
|  [05]   | `graph_core.V(cond=None)` / `E(cond=None)` / `N(v, f_io=0)`        | query    | filtered vertex/edge iterators; neighbor list (`f_io>0` outward, `<0` inward) |
|  [06]   | `graph_core.roots()` / `leaves()`                                  | query    | vertices with no inward / no outward edges (Sugiyama rank-0 seeds)    |
|  [07]   | `graph_core.get_scs_with_feedback(roots=None)`                     | analysis | Tarjan SCCs; marks `Edge.feedback=True` on the feedback-arc set (O(V+E)) |
|  [08]   | `graph_core.path(x, y, f_io=0, hook=None)`                         | analysis | BFS shortest path as a `Vertex` list; `hook` called per visited vertex |
|  [09]   | `graph_core.dijkstra(x, f_io=0, hook=None, subset=None)`           | analysis | weighted single-source shortest-path distance map (heap priority queue) |
|  [10]   | `graph_core.partition()`                                           | analysis | partition the component into rank-respecting vertex lists            |
|  [11]   | `graph_core.M(cond=None)` / `deg_min()` / `deg_max()` / `deg_avg()` / `eps()` | query | associativity matrix; degree statistics                              |

[ENTRYPOINT_SCOPE]: `SugiyamaLayout` layered drawing
- rail: figure

`SugiyamaLayout(component)` is constructed over a `graph_core`; every vertex must carry a `view` (set `vertex.view = VertexViewer(w, h)` first). `init_all()` computes roots, the feedback-arc set, vertex ranks, dummy vertices, and layers; `draw(N=1.5)` converges the ordering over `N` crossing-reduction rounds, runs Brandes-Köpf xy assignment (`setxy`), and routes edges (`draw_edges`). The owner binds `layout.route_edge = route_with_lines | route_with_splines` before `draw()` so each edge's polyline is computed and stored on `edge.view._pts`. Tunable drawing parameters are plain attributes set after construction; `draw_step`/`ordering_step` are the per-step iterators for animation/debugging only.

| [INDEX] | MEMBER                                                  | KIND      | ROLE                                                                  |
| :-----: | ------------------------------------------------------ | --------- | -------------------------------------------------------------------- |
|  [01]   | `SugiyamaLayout(g)`                                     | construct | layered layout over a `graph_core`; computes default `dw`/`dh` from view medians |
|  [02]   | `SugiyamaLayout.init_all(roots=None, inverted_edges=None, optimize=False)` | init | ranks vertices, builds dummies + layers; `optimize=True` pushes long edges down |
|  [03]   | `SugiyamaLayout.draw(N=1.5)`                            | run       | converge ordering over `N` rounds, Brandes-Köpf xy, then route edges — the one layout driver |
|  [04]   | `SugiyamaLayout.route_edge`                             | bind      | per-edge router slot the owner sets to `route_with_lines`/`route_with_splines` before `draw()` |
|  [05]   | `SugiyamaLayout.setxy()`                                | run       | Brandes-Köpf horizontal-coordinate assignment writing `vertex.view.xy` (the 4-pass median) |
|  [06]   | `SugiyamaLayout.draw_edges()`                           | run       | build each edge's dummy-vertex polyline and invoke `route_edge`, storing it via `view.setpath` |
|  [07]   | `SugiyamaLayout.dirvh` / `dirv` / `dirh`               | param     | alignment-policy state (0-3); the 4 Brandes-Köpf passes sweep all values |
|  [08]   | `SugiyamaLayout.xspace` / `yspace` / `dw` / `dh` / `order_iter` | param | intra-layer / inter-layer spacing, default node w/h, ordering-iteration count |
|  [09]   | `SugiyamaLayout.draw_step()` / `ordering_step(oneway=False)` | iter | per-layer step iterators for animation/debugging only (inefficient — not the production path) |
|  [10]   | `SugiyamaLayout.grx` / `layers` / `ctrls`              | state     | per-vertex layout attrs, the `Layer` list, edge→dummy-vertex control map (layout-internal) |

[ENTRYPOINT_SCOPE]: `DigcoLayout` energy/stress placement
- rail: figure

`DigcoLayout(component)` is the alternative force/stress-majorization (DIG-CoLa) provider for non-hierarchical placement: `init_all(alpha, beta)` partitions a directed graph into hierarchical levels and seeds positions (random in x); `draw(N)` minimizes layout stress by conjugate-gradient solving of the Laplacian system, then writes `vertex.view.xy`. It needs the `[full]` numpy extra for performance but runs on the bundled `linalg` fallback. The local owner currently routes force/radial placement to `rustworkx` (`spring_layout`/`circular_layout`) rather than `DigcoLayout`; this entry documents the available capability, not the bound path.

| [INDEX] | MEMBER                                          | KIND      | ROLE                                                                  |
| :-----: | ----------------------------------------------- | --------- | -------------------------------------------------------------------- |
|  [01]   | `DigcoLayout(g)`                                 | construct | energy/stress-majorization placement over a `graph_core`             |
|  [02]   | `DigcoLayout.init_all(alpha=0.1, beta=0.01)`     | init      | partition directed graph into levels, seed positions                 |
|  [03]   | `DigcoLayout.draw(N=None)`                        | run       | conjugate-gradient stress minimization; writes `vertex.view.xy`       |
|  [04]   | `DigcoLayout.xspace` / `yspace` / `dr`           | param     | spacing and node-radius drawing parameters                           |
|  [05]   | `DwyerLayout(g)`                                 | construct | `DigcoLayout` subclass (reserved for separation-constraint extension) |

[ENTRYPOINT_SCOPE]: edge routing + geometry helpers (`grandalf.routing` / `grandalf.utils.geometry`)
- rail: figure

The `route_with_*` functions take `(edge, pts)` where `pts` is the layout-produced polyline; they clamp the tail/head to each node's bounding box (`intersectR`), set `edge.view.head_angle`, and — for the spline/rounded variants — replace the corner sequence. The geometry helpers are the math primitives the routers compose; `setcurve` implements the NURBS-book local cubic-Bézier interpolation, exposed for a figure that wants the spline control points directly. The local owner reads the final `edge.view._pts` into its `RouteMap`; raw geometry primitives stay layout-internal.

| [INDEX] | MEMBER                                                       | KIND    | ROLE                                                                  |
| :-----: | ------------------------------------------------------------ | ------- | --------------------------------------------------------------------- |
|  [01]   | `route_with_lines(e, pts)`                                   | route   | clamp endpoints to node bb, straight polyline, set `view.head_angle`  |
|  [02]   | `route_with_splines(e, pts)`                                 | route   | line route then `setroundcorner` cubic Béziers stored on `view.splines` |
|  [03]   | `route_with_rounded_corners(e, pts)`                         | route   | line route then iterative distance-rounding of corners (`ROUND_AT_DISTANCE`) |
|  [04]   | `intersectR(view, topt)`                                     | geom    | intersection of the node bounding box with the line to `topt` (endpoint clamp) |
|  [05]   | `intersectC(view, r, topt)`                                  | geom    | intersection of a circular node of radius `r` with the line to `topt` |
|  [06]   | `setcurve(e, pts, tgs=None)` / `setroundcorner(e, pts)`      | geom    | NURBS-book local cubic-Bézier spline through points / rounded-corner spline list |
|  [07]   | `getangle(p1, p2)` / `new_point_at_distance(pt, distance, angle)` | geom | polyline angle; point advanced by a distance along an angle           |
|  [08]   | `median_wh(views)` / `rand_ortho1(n)`                        | geom    | median node width/height (Sugiyama default sizing); random unit vector ⟂ (1,…,1) |

[ENTRYPOINT_SCOPE]: interop + support utilities (`grandalf.utils`)
- rail: figure

`Poset` is the deterministic-ordered set backing `graph_core.sV`/`sE` (the stable iteration order keeps layout reproducible). The `networkx` and `Dot` adapters are alternative ingress paths grandalf ships; this owner ingests from `rustworkx`, not these, so they stay available but unbound.

| [INDEX] | MEMBER                                                          | KIND    | ROLE                                                                  |
| :-----: | -------------------------------------------------------------- | ------- | --------------------------------------------------------------------- |
|  [01]   | `Poset(L)`                                                     | support | deterministic-ordered set backing `graph_core.sV`/`sE`; `add`/`remove`/`get`/`index` |
|  [02]   | `convert_nextworkx_graph_to_grandalf(G)`                      | interop | build a grandalf `Graph` from a networkx graph (alternative ingress; unbound here) |
|  [03]   | `convert_grandalf_graph_to_networkx_graph(G)`                 | interop | export a grandalf graph to a networkx `MultiDiGraph` (alternative egress) |
|  [04]   | `Dot` (`grandalf.utils.dot`)                                  | interop | LALR(1) graphviz `.dot` parser (needs the `ply` `[full]` extra; `_has_ply` gate) |
|  [05]   | `array` / `matrix` (`grandalf.utils.linalg`)                  | support | bundled numpy-free linear-algebra fallback `geometry` uses when numpy is absent |

## [04]-[IMPLEMENTATION_LAW]

- import: `from grandalf.graphs import Graph, Vertex, Edge`; `from grandalf.layouts import SugiyamaLayout, VertexViewer`; `from grandalf.routing import EdgeViewer, route_with_lines, route_with_splines` — at boundary scope inside the `to_thread.run_sync` kernel only; the local owner aliases `Vertex`/`Edge`/`Graph` as `GrandalfVertex`/`GrandalfEdge`/`GrandalfGraph` so the names never collide with the canonical `visualization/diagram/glyphset#GLYPHSET` shapes; the version is `importlib.metadata.version("grandalf")` (no `__version__`).
- provider axis: grandalf is `visualization/diagram/layout#LAYOUT`'s `HierarchyEngine.GRANDALF` provider behind the one `LayoutPolicy` owner, reached only by the `Layered(engine=GRANDALF)` `match` arm; `rustworkx` (`libs/python/data`) stays the presentation-graph builder, force/radial/topological layout provider, and `node_link_json` content-key wire. The two never overlap: a hand-rolled Sugiyama loop where grandalf owns it, a hand-rolled spring loop where `rustworkx` owns it, or a second analysis kernel where the `data/graph#GRAPH` `rustworkx` kernel owns centrality/shortest-path/community are the deleted forms.
- topology axis: build `GrandalfVertex(node_index)` per `rustworkx` node and `GrandalfEdge(src, dst)` per edge, take `GrandalfGraph(vertices, edges).C[0]` as the `SugiyamaLayout` component; the stable `rustworkx` integer node index lives on `Vertex.data`, the one key the resolved coordinates and the emitted glyph marks share — coordinates are never re-keyed away from it. An empty graph returns `({}, {})` before construction (a `graph_core` over zero vertices is degenerate).
- view axis: every vertex must carry a `view` before `SugiyamaLayout`; set `vertex.view = VertexViewer(w, h)` (the glyph box extent) and `edge.view = EdgeViewer()` so `draw()` can write `view.xy` and `view.setpath(pts)`. The layout reads `view.w`/`view.h` for spacing and writes `view.xy`; the owner reads the laid-out `vertex.view.xy` into `LayoutMap` and `edge.view._pts` into `RouteMap`, projecting each through `float(...)` — raw grandalf vertices/edges/viewers never cross into glyph or receipt shapes.
- draw axis: `SugiyamaLayout(component)` then `layout.route_edge = <router>` then `init_all()` then `draw()` is the fixed call order; `init_all` runs Tarjan feedback-arc detection and ranking, `draw` converges crossing reduction and runs Brandes-Köpf xy assignment plus routing. The whole synchronous build+layout+glyph emission is one `_render` kernel `_compute` offloads onto `anyio.to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)` under the module `CapacityLimiter`, so the native layout never blocks the event loop.
- routing axis: bind `route_with_lines` or `route_with_splines` to `route_edge`; grandalf has **no native orthogonal router**, so `EdgeRoute.ORTHOGONAL` degrades to `route_with_lines` until `pyelk`/`libavoid` owns true orthogonal connector routing — a hand-rolled orthogonal loop inside the grandalf arm is the deleted form. The routed polyline is read off `edge.view._pts` (the attribute `EdgeViewer.setpath` writes), never recomputed.
- receipt axis: the layout contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm)` case — flat scalars, the algorithm the matched `LayoutPolicy.tag` (`"layered"`), the node/edge counts the built-`rustworkx`-graph extents (not grandalf's), keyed by `ContentIdentity` over the `node_link_json` wire joined to the resolved coordinate + route maps; grandalf is a layout function, never a receipt or content-key source.
- boundary: grandalf owns the layered Sugiyama coordinate assignment and the bounding-box edge routing for the stacking/program layered diagram kinds; `rustworkx` owns presentation-graph build, force/radial/topological layout, graph analysis, and the content-key wire; `visualization/diagram/glyphset#GLYPHSET` owns the `DiagramGlyph`/`DiagramKind`/`GlyphStyle` shapes; `visualization/diagram/draw#DRAW` owns SVG emission off the positioned marks; `graphic/color/derive#DERIVE` owns the palette the `GlyphStyle` indices key. Native orthogonal/port/nesting layout is `pyelk`'s, constraint-solver layout is `kiwisolver`'s, and Rust Sugiyama placement is `fast-sugiyama`'s — grandalf is the fallback layered engine pending their parity.

[STACKING]:
- `visualization/diagram/layout#LAYOUT` `_grandalf_layout(graph, policy)` folds a `rustworkx` `PyDiGraph` into grandalf: `GrandalfVertex(i)` per `graph.node_indices()`, `GrandalfEdge(src, dst)` per `graph.edge_list()`, `GrandalfGraph(...).C[0]`, `vertex.view = VertexViewer(48, 32)`, `edge.view = EdgeViewer()`, `SugiyamaLayout(component)` with `route_edge` bound by `_grandalf_router(policy.route)`, then `init_all()` + `draw()`, reading `vertex.view.xy` into `LayoutMap` and `edge.view._pts` into `RouteMap` — one layered arm of the `LayoutPolicy` `match`, the other arms `rustworkx`'s, never two parallel layout owners.
- The grandalf layered coordinates + routed edge points join the `rustworkx` `node_link_json` wire as the `ContentIdentity` content-key input, so two layouts of one graph under distinct `LayoutPolicy` values key distinctly and the diagram is content-addressable on its full laid-out form — grandalf contributes the coordinates, `rustworkx` the canonical topology wire, the `runtime` `ContentIdentity` the key.
- The positioned `DiagramGlyph` sequence the layout emits from the grandalf coordinates threads into `visualization/diagram/draw#DRAW`, which folds the marks (boxes from `LayoutMap`, edge polylines from `RouteMap`) to an SVG `Drawing` via `drawsvg`, contributing the same `ArtifactReceipt.Diagram` case off its glyph tallies — one receipt family, the layout's node/edge counts and the draw owner's glyph counts both as `Diagram` cases.
- The synchronous grandalf `init_all()`/`draw()` runs under `anyio.to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)` (the shared `libs/python/.api/anyio` structured-concurrency rail), bounded by the module `CapacityLimiter(os.process_cpu_count())`, so a many-node layered diagram never blocks the figure rail; the whole rail is wrapped by the `runtime` `async_boundary` returning a `RuntimeRail[...]` (`expression`-folded result), so a grandalf `ValueError` on an unconnected component surfaces as a typed rail fault, never an unbounded exception.

## [05]-[LOCAL_ADMISSION]

- Package: `grandalf`
- Status: **SUPERSEDED-PENDING-REMOVAL** — flagged in `pyproject.toml` as the fallback layered engine "until fast-sugiyama parity lands"; `fast-sugiyama` (Rust/PyO3) replaces the placement core and `pyelk` owns native orthogonal/port/nesting layout. Remove from `pyproject.toml` in the final reconciliation once `fast-sugiyama`/`pyelk` parity is proven; until then it is the only working layered+routing provider and stays admitted.
- Owns: the pure-Python layered (Sugiyama "dot") graph-drawing engine for the `HierarchyEngine.GRANDALF` arm — `Graph`/`graph_core` topology with Tarjan feedback-arc detection, `SugiyamaLayout` rank+order+Brandes-Köpf xy assignment, `DummyVertex` long-edge control points, and the `route_with_lines`/`route_with_splines` bounding-box edge routers, all read through `VertexViewer`/`EdgeViewer` view providers into the local `LayoutResult`
- Accept: layered coordinate assignment + line/spline edge routing for the stacking and program-flow AEC diagram kinds, behind the one `LayoutPolicy` owner, with the stable `rustworkx` node index as the shared key and the native draw off-loaded onto `to_thread.run_sync`
- Reject: a second diagram-layout owner where the one `LayoutPolicy` discriminates (force/radial→`rustworkx`, layered→grandalf, projected→deterministic transform); a hand-rolled Sugiyama or spring loop where grandalf/`rustworkx` own them; a hand-rolled orthogonal-routing loop inside the grandalf arm where `pyelk`/`libavoid` own native orthogonal routing (`EdgeRoute.ORTHOGONAL` degrades to `route_with_lines` for now); a re-keying of layout coordinates away from the `rustworkx` node index; raw grandalf vertices/edges/viewers/components crossing into glyph or receipt shapes; a second graph-analysis kernel where the `data/graph#GRAPH` `rustworkx` kernel owns it; a synchronous native layout left on the event loop; the `Dot`/`networkx` ingress adapters or the `[full]` `numpy`/`ply` extras where `rustworkx` ingress and the bundled `linalg` fallback already serve this owner; and grandalf as a content-key or receipt source where `rustworkx` `node_link_json` + `runtime` `ContentIdentity` own it
