# [PY_ARTIFACTS_API_FAST_SUGIYAMA]

`fast-sugiyama` owns layered-graph coordinate assignment: a Rust/PyO3 abi3 Sugiyama pipeline behind one `from_edges(edges, ...)` boundary that int-maps a mixed-type `(source, target)` edge list for the Rust core, runs the crossing-reduction placement, and re-projects the result to the original node ids as a `Layouts`. It computes coordinates and component extents only — SVG stays with `visualization/diagram/draw#DRAW`, graph analysis with the `data/graph#GRAPH` `rustworkx` kernel — and its synchronous native call runs inside the layout owner's `to_thread.run_sync` `CapacityLimiter` lane, off the event loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fast-sugiyama`
- package: `fast-sugiyama` (MIT)
- module: `fast_sugiyama` (`fast_sugiyama.layout` re-exports the free-function transforms)
- owner: `artifacts`
- rail: diagram-layout
- abi: abi3 stable-ABI extension `_fast_sugiyama.abi3.so`, interpreter-agnostic
- native: Rust/PyO3 `_fast_sugiyama` (`_from_edges`); `Layouts.rect_pack_layouts` imports `rpack` at call time — unadmitted, gating the rect-pack composite
- capability: run the full Sugiyama pipeline (cycle break, layer rank, dummy-vertex routing nodes, ordered crossing minimization, coordinate assignment) over a mixed-type edge list in one native call, returning per-connected-component coordinates, extents, and optional edge-bend points, then re-arrange components through the `Layouts` transform algebra — replacing both a `dot` subprocess and a Python placement loop

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Layouts` placement container and its layout tuple

`from_edges` returns one `Layouts` per connected graph component — a `list[LayoutType]` subclass carrying the transform algebra, each `LayoutType` the 4-tuple `(positions, width, height, edges)` with `positions` keyed on the original node id re-projected from the Rust int indices and `edges` carrying dummy-vertex bend points when `dummy_vertices=True`. `{from_edges, layout, Layouts}` is the public surface; every other name is a `_types` alias or the private `_fast_sugiyama` extension.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [CAPABILITY]                                                                          |
| :-----: | :-------------- | :-------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `Layouts`       | `list` subclass | carries the transform algebra; slice-preserving `__getitem__`                         |
|  [02]   | `LayoutType`    | tuple alias     | `tuple[PositionsType, NumType, NumType, EdgeListType \| None]` per component          |
|  [03]   | `LayoutsType`   | list alias      | `list[LayoutType]` (the structural base `Layouts` subclasses)                         |
|  [04]   | `PositionType`  | tuple alias     | `tuple[NodeIDType, CoordType]` = one placed `(node_id, (x, y))`                       |
|  [05]   | `PositionsType` | sequence alias  | `Sequence[PositionType]` placed-node coordinate list                                  |
|  [06]   | `CoordType`     | tuple alias     | `tuple[float, float] \| tuple[int, int]` a node coordinate                            |
|  [07]   | `LayersType`    | dict alias      | `dict[NumType, Sequence[NodeIDType]]` y-banded layer membership (from `build_layers`) |
|  [08]   | `BBoxesType`    | sequence alias  | `Sequence[tuple[CoordType, NumType, NumType]]` `((x, y), w, h)` debug/pack boxes      |

[PUBLIC_TYPE_SCOPE]: the node/edge/coordinate vocabulary (`_types`)

Mixed-type edge input is by design — `NodeIDType = int | str`, so the boundary int-maps any hashable key for the Rust core then restores it, feeding the stable `rustworkx` integer node index and reading coordinates back on that same index. `Int`-suffixed aliases are the int-keyed Rust-core wire shapes the boundary translates across; a design page composes the non-`Int` aliases.

| [INDEX] | [SYMBOL]         | [DEFINITION]                                                         | [CAPABILITY]                               |
| :-----: | :--------------- | :------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `NodeIDType`     | `int \| str`                                                         | a node key (mixed-type edges supported)    |
|  [02]   | `NumType`        | `int \| float`                                                       | spacing / extent / coordinate scalar       |
|  [03]   | `EdgeType`       | `tuple[NodeIDType, NodeIDType]`                                      | one `(source, target)` directed edge       |
|  [04]   | `EdgeListType`   | `Iterable[EdgeType]`                                                 | the `from_edges` input edge stream         |
|  [05]   | `LayoutIntType`  | `tuple[PositionsIntType, NumType, NumType, EdgeListIntType \| None]` | int-keyed per-component result (Rust wire) |
|  [06]   | `LayoutsIntType` | `list[LayoutIntType]`                                                | int-keyed result list from `_from_edges`   |

[PUBLIC_TYPE_SCOPE]: the placement knob vocabularies (`_types`)

`Ranking` and `CrossingMinimization` are the two closed `Literal` knob vocabularies steering the Rust pipeline; a `LayeredPolicy` field lowers to a `ranking_type`/`crossing_minimization` value directly, never a free-form string.

| [INDEX] | [SYMBOL]               | [VALUES]                                     | [CAPABILITY]                                              |
| :-----: | :--------------------- | :------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `Ranking`              | `"original" \| "minimize" \| "up" \| "down"` | rank: keep-input / minimize-height / longest-path up-down |
|  [02]   | `CrossingMinimization` | `"median" \| "barycenter"`                   | ordering heuristic for the crossing-reduction sweep       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `from_edges` — the one placement boundary

`from_edges(edges, ...)` is the single polymorphic placement entrypoint — no `layout_tree`/`layout_dag` family; one call discriminates on the edge list and lays out every component. Every knob defaults `None` (the Rust core supplies its built-in default) except `vertex_spacing`, which the boundary defaults to `PYDOT_SPACING` (72, the Graphviz-`dot` grid pitch) so default output matches `dot` spacing. `_from_edges` exposes `encode_edges` in the Rust core; the public boundary forces `encode_edges=False` and never surfaces the encoded-edge wire.

| [INDEX] | [SURFACE]                                             | [SHAPE] | [CAPABILITY]                                                     |
| :-----: | :---------------------------------------------------- | :------ | :--------------------------------------------------------------- |
|  [01]   | `from_edges(edges, ...) -> Layouts`                   | layout  | the full Sugiyama pipeline over a mixed-type edge list           |
|  [02]   | `minimum_length: NumType \| None`                     | knob    | minimum layer separation (edge-span lower bound between ranks)   |
|  [03]   | `vertex_spacing: NumType \| None`                     | knob    | within-layer node spacing; boundary default `72` (`dot` pitch)   |
|  [04]   | `dummy_vertices: bool \| None`                        | knob    | insert dummy routing vertices on long edges; `edges` carry bends |
|  [05]   | `dummy_size: NumType \| None`                         | knob    | footprint reserved for each inserted dummy vertex                |
|  [06]   | `ranking_type: Ranking \| None`                       | knob    | layer-rank strategy (`original`/`minimize`/`up`/`down`)          |
|  [07]   | `crossing_minimization: CrossingMinimization \| None` | knob    | crossing-reduction heuristic (`median`/`barycenter`)             |
|  [08]   | `transpose: bool \| None`                             | knob    | run the transpose local-optimization pass after ordering         |
|  [09]   | `check_layout: bool \| None`                          | knob    | run the internal post-placement validity assertion               |

[ENTRYPOINT_SCOPE]: `Layouts` placement-result methods (transform algebra)

`Layouts` carries the post-placement transform algebra as bound methods (drop the `Layouts.` prefix), each returning a new `Layouts` or a projection. Two composites are the recipes a diagram owner calls instead of hand-stitching primitives: `dot_layout` chains top-align then horizontal compaction (the single-`dot`-graph look), `compact_layout` chains rect-pack, horizontal sort, and compaction (dense multi-component packing). `__getitem__` is slice-preserving; `shuffle(seed=)` is deterministic under a seed, so a seeded re-arrange stays content-key-stable.

| [INDEX] | [SURFACE]                                               | [SHAPE]   | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------ | :-------- | :----------------------------------------------------------------- |
|  [01]   | `Layouts(data=None)`                                    | construct | wrap a `LayoutsType`; the boundary builds it empty, `append`s each |
|  [02]   | `to_dict()` / `flatten_positions()`                     | project   | `{node_id:(x,y)}` map / flat `(node,(x,y))` stream                 |
|  [03]   | `get_origins()` / `to_bboxes(spacing=None)`             | query     | per-component origins / `((x,y),w+spacing,h+spacing)` boxes        |
|  [04]   | `sort_horizontal(reverse=False)` / `sort_vertical(...)` | order     | re-order components by component origin x / y                      |
|  [05]   | `align_layouts_vertical(spacing=None)`                  | arrange   | stack components in one vertical strip with `spacing` gaps         |
|  [06]   | `align_layouts_horizontal(spacing=None)`                | arrange   | stack components in one horizontal strip with `spacing` gaps       |
|  [07]   | `align_layouts_vertical_top()` / `_vertical_center()`   | arrange   | top-align / center-align components to the tallest band            |
|  [08]   | `align_layouts_horizontal_center()`                     | arrange   | center-align components to the widest band                         |
|  [09]   | `compact_layouts_horizontal(spacing=None)`              | arrange   | per-layer left-pack into shared y-bands (dot-compaction)           |
|  [10]   | `dot_layout(spacing=None)`                              | composite | the Graphviz-`dot` single-graph arrangement                        |
|  [11]   | `compact_layout(spacing=None, max_width=None, ...)`     | composite | dense multi-component packing                                      |
|  [12]   | `rect_pack_layouts(spacing=None, max_width=None, ...)`  | arrange   | [GATED] disjoint-component rect pack (`import rpack`)              |
|  [13]   | `shuffle(seed=None)` / `to_list()`                      | util      | deterministic-seeded in-place shuffle / view as `LayoutsType`      |

[ENTRYPOINT_SCOPE]: module-level transform primitives (`fast_sugiyama.layout`)

Every `Layouts` arrange/project/pack method has a free-function twin in `fast_sugiyama.layout` over a raw `LayoutsType`, for an owner holding the tuple list rather than a `Layouts`. Only the following are module-exclusive: `PYDOT_SPACING` the default spacing every gap-taking transform falls back to, `build_layers` the y-coordinate → node-list banding the compaction pass reads, the origin queries, and the matplotlib-`Rectangle`-shaped debug boxes.

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------------------------------- |
|  [01]   | `PYDOT_SPACING`                                         | constant | `72` — default within-/between-component spacing (`dot` pitch) |
|  [02]   | `build_layers(positions)`                               | query    | `{y: [node, ...]}` layer banding of one component's positions  |
|  [03]   | `get_position_origin(positions)` / `get_origin(layout)` | query    | min-corner `(x, y)` of a position list / one layout            |
|  [04]   | `get_bboxes(layouts, spacing=None)`                     | query    | per-component `((x,y), w+spacing, h+spacing)` debug/pack boxes |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `from_edges` is fed the `(source, target)` pairs from the `data/tabular#GRAPH` adjacency frame or the `rustworkx` `PyDiGraph.edge_list()`, keyed on the same stable integer node index the layout owner assigns and the glyph marks read back on, so no third re-keying enters; the whole synchronous native call runs inside the layout owner's `_render` kernel that `_compute` offloads onto `to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)`, never on the event loop and never under a second limiter.
- `LayeredPolicy` fields lower to `from_edges` knobs — `direction` maps to `ranking_type` with a coordinate transpose for LR/RL, `crossing_minimization` selects `median` vs `barycenter`, `minimum_length`/`vertex_spacing`/`dummy_size` carry the spacing geometry, and `dummy_vertices=True` is set exactly when the policy needs long-edge bend waypoints returned in the per-component `edges`; a new layout knob is one `LayeredPolicy` field mapped to one `from_edges` argument, never a new entrypoint.
- `from_edges` returns a `Layouts`, one `(positions, width, height, edges)` tuple per connected component: a single-graph diagram reads component `[0]`, a forest arranges every component through `dot_layout` or `compact_layout`, then `to_dict()` flattens to the `{node_index: Point}` `LayoutMap` the glyph-construction fold consumes — `positions` build `LayoutMap`, the dummy-vertex `edges` bend points build `RouteMap`, with no intermediate `dot` parse.
- Each placement records node count, edge count, connected-component count, resolved `ranking_type`/`crossing_minimization`, and per-component extent into the `core/receipt#RECEIPT` `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm, bytes)` case with `algorithm` the placement tag; `bytes` stays unpopulated because layout serializes no artifact, and `visualization/diagram/draw#DRAW` stamps the emitted SVG length onto the same case. Content key is the `xxh3_128` digest over the graph wire joined to resolved positions, so a re-placement under different knobs keys distinctly.
- `Layouts.rect_pack_layouts` (and `compact_layout`) imports `rpack` at call time; `rpack` is unadmitted, so the disjoint-component arrangement defaults to `dot_layout`, and dense packing either admits `rpack` centrally or composes the `Rasm.Fabrication` `RectanglePacker`/NFP nesting kernel over the component `to_bboxes()` rectangles.

[STACKING]:
- A layered diagram kind in `visualization/diagram/layout#LAYOUT` flips `LayeredPolicy.engine` to this arm: `_render` builds the `rustworkx` `PyDiGraph` from the `data/tabular#GRAPH` adjacency frame, calls `from_edges(graph.edge_list(), ...)`, takes `dot_layout()`, and folds the resulting `Layouts` into the `LayoutResult` — positions to `LayoutMap`, dummy-vertex `edges` to `RouteMap` — all inside the one `to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)` `anyio` lane, so the native placement composes the structured-concurrency rail with zero new async surface.
- `LayeredPolicy`'s `expression.tagged_union` case gains a `from_edges` knob mapping rather than a new case: `direction`/`route`/`crossing` fields lower to `ranking_type`/`dummy_vertices`/`crossing_minimization`, the `HierarchyEngine` `StrEnum` discriminates this arm, and the `match`/`case` over `policy.tag` stays the one total dispatch — the `expression` rail owns the discriminant, this engine owns one arm's body.
- Placement is content-addressed on the universal `xxh3_128` rail: the layout owner digests the `rustworkx` `node_link_json` graph wire joined to `repr(sorted(Layouts.to_dict().items()))` and the routed bend points, so two placements of one graph under distinct knobs key distinctly, keyed by the same `ContentIdentity` owner every artifact receipt uses.
- Disjoint architectural components arrange through `Layouts.dot_layout()` by default; when dense packing is wanted the component `to_bboxes()` rectangles feed the `Rasm.Fabrication` nesting kernel at the wire, so layered placement and component packing compose without either owner re-implementing the other.

[LOCAL_ADMISSION]:
- Admitted as the in-process native layered-placement owner for hierarchical AEC diagram kinds (stacking, program-flow, dependency, organization), composing the `rustworkx` presentation graph, the `expression.tagged_union` layout policy, the `to_thread.run_sync` placement lane, and the `xxh3_128` content key, and contributing the `ArtifactReceipt.Diagram` case.

[RAIL_LAW]:
- Package: `fast-sugiyama`
- Owns: layered-graph (Sugiyama) coordinate assignment — cycle break, layer ranking, dummy-vertex long-edge routing-node insertion, ordered crossing minimization, horizontal coordinate assignment — over a mixed-type edge list, returning per-component coordinates, extents, and edge-bend points as a `Layouts`, with the post-placement component-arrangement algebra and the `dot_layout`/`compact_layout` composites
- Accept: layered/hierarchical diagram placement as the native replacement for any `dot` subprocess, composing the `rustworkx` graph, the `expression.tagged_union` policy, the `anyio` placement lane, and the `xxh3_128` key
- Reject: a hand-rolled Sugiyama/layer/crossing-minimization loop where `from_edges` owns it; a `dot`/Graphviz subprocess where this native call replaces it; force/radial/topological layout where `rustworkx` owns it; spline edge-route generation where `grandalf` `route_with_splines` owns it; graph analysis where the `data/graph#GRAPH` `rustworkx` kernel owns it; SVG emission where `visualization/diagram/draw#DRAW` owns it; the `rect_pack_layouts`/`compact_layout` `rpack` path while `rpack` is unadmitted; a re-keying of coordinates off the stable node index; a synchronous native placement left on the event loop; identity minting the runtime owns
