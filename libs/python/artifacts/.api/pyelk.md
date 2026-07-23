# [PY_ARTIFACTS_API_PYELK]

`pyelk` owns ports-and-nesting graph layout with native orthogonal edge routing — a pure-Python Eclipse Layout Kernel port whose one `ELK().layout(graph)` boundary consumes and returns the recursive ELK-JSON document (`{id, children, edges, ports, labels, layoutOptions}`), writing node and port coordinates, nested-container extents, and `edges[].sections[]` route geometry in place. It is the one diagram-layout owner handling the three concerns `fast-sugiyama`/`grandalf`/`rustworkx` cannot — typed ports, recursive nesting, and orthogonal edge routing — assigning coordinates and route geometry only, its synchronous pass off-loaded to the layout owner's `anyio.to_thread.run_sync` `CapacityLimiter` lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyelk`
- package: `pyelk` (EPL-2.0)
- module: `pyelk` (`pyelk.options`, `pyelk.graph`, `pyelk.elk`, `pyelk.exceptions`)
- owner: `artifacts`
- rail: diagram-layout
- native: none — pure Python, zero non-dev runtime dependencies, so it composes the layout owner with no transitive surface
- capability: run any of the nine ELK algorithms over a recursive ELK-JSON graph in one in-process call with typed port constraints, recursive compound-node nesting, and native orthogonal edge routing, returning the same document with positions and edge `sections` filled in

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the ELK-JSON graph document (one model — a plain `dict`, not a class)

pyelk's model IS the ELK-JSON graph — a plain recursive `dict`, never a class-based node/edge graph, so a `rustworkx`/`polars` adjacency frame lowers to it by construction and the laid-out result reads back by key. `ELK.layout` returns the same document shape with coordinates and route geometry written in place; `__all__` binds `ELK` and the four-member `ElkError` family, and the document keys below are the load-bearing contract a design page builds and reads.

| [INDEX] | [DOCUMENT_KEY]     | [KIND]      | [ROLE]                                                                                              |
| :-----: | :----------------- | :---------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `id`               | str (req)   | unique element id (root/node/port/edge/label); `validate_id` enforces non-empty unique ids          |
|  [02]   | `children`         | list[dict]  | child NODES of a compound node; recursion is hierarchical nesting, parent sized to enclose children |
|  [03]   | `edges`            | list[dict]  | edges at this scope: `{id, sources, targets}`; hyperedge ID lists; endpoint may be a port id        |
|  [04]   | `ports`            | list[dict]  | typed PORTS `{id, width, height, layoutOptions}`; side/index under `elk.portConstraints`            |
|  [05]   | `labels`           | list[dict]  | text labels on a node/edge/port: `{text, width, height}`; placement via `elk.nodeLabels.placement`  |
|  [06]   | `width` / `height` | float       | INPUT leaf-node fixed size; OUTPUT computed container extent enclosing children + padding           |
|  [07]   | `x` / `y`          | float (out) | OUTPUT: assigned top-left coordinate relative to parent; absent on input leaf nodes                 |
|  [08]   | `layoutOptions`    | dict        | per-element overrides (`{"elk.algorithm": ..., "elk.direction": ...}`); override global + parent    |
|  [09]   | `edges[].sections` | list[dict]  | OUTPUT routed edge geometry: `{id, startPoint, endPoint, bendPoints}`; orthogonal waypoints         |
|  [10]   | `logging`          | dict (out)  | OUTPUT: present when `layout(..., logging=True)`; the per-run diagnostic trace                      |

[PUBLIC_TYPE_SCOPE]: the exception family (`pyelk.exceptions`)

Every exception descends from `ElkError`, the one diagram-layout failure class the layout owner's `async_boundary` traps; the three leaf subclasses discriminate the cause for the receipt. `validate_graph` checks id presence and shape, not duplicate ids, so the caller dedups upstream — keying off `str(node_index)` already guarantees uniqueness.

| [INDEX] | [TYPE]                              | [RAISED_WHEN]                                                                        |
| :-----: | :---------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `ElkError`                          | `Exception` subclass; base ELK layout error — the one class `async_boundary` traps   |
|  [02]   | `InvalidGraphException`             | malformed graph: bad ids or missing required fields (`validate_graph`/`validate_id`) |
|  [03]   | `UnsupportedGraphException`         | valid JSON but the selected algorithm cannot lay the shape out                       |
|  [04]   | `UnsupportedConfigurationException` | the `layoutOptions` combination is unsupported by the algorithm                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ELK` — the one layout engine boundary

`ELK().layout(graph)` is the one polymorphic placement boundary — no `layout_layered`/`layout_force`/`layout_tree` family; one call discriminates the algorithm off the graph's `elk.algorithm` option (or the constructor's `default_layout_options`), dispatches to the registered provider, and returns the positioned document. `ELK` is built once and reused; `default_layout_options` sets a global option floor every call inherits, the per-call `layout_options` overlays it, and `logging`/`measure_execution_time` are diagnostic switches riding the result dict.

| [INDEX] | [SURFACE]                                           | [SHAPE]    | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------------------- | :--------- | :---------------------------------------------------------------- |
|  [01]   | `ELK(default_layout_options=None, algorithms=None)` | construct  | the engine; `default_layout_options` sets the global option floor |
|  [02]   | `ELK.layout(graph, …) -> dict`                      | layout     | placement boundary; writes positions + edge `sections`            |
|  [03]   | `ELK.known_layout_algorithms()`                     | introspect | the nine providers as `{id, name}` rows                           |
|  [04]   | `ELK.known_layout_options()`                        | introspect | layout options as `{id, type}` rows                               |
|  [05]   | `ELK.known_layout_categories()`                     | introspect | category roster as `{id, name, knownLayouters}` rows              |

[ENTRYPOINT_SCOPE]: the nine layout providers (`elk.ALGORITHM_REGISTRY` / `ALGORITHM_ALIASES`)

`elk.ALGORITHM_REGISTRY` maps each `org.eclipse.elk.<short>` id to its provider class and `elk.ALGORITHM_ALIASES` maps the short and `elk.`-prefixed names onto it (`elk.resolve_algorithm` resolves), so a `LayeredPolicy`/`HierarchyEngine` selection sets the short alias as the `elk.algorithm` value and `layout` dispatches — the layout owner never imports a provider class. `disco` resolves to no registered provider and raises `ElkError`, so the policy admits only the nine registered ids below.

| [INDEX] | [ALGORITHM_SHORT] | [PROVIDER_CLASS]          | [OWNS]                                                                               |
| :-----: | :---------------- | :------------------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `layered`         | `LayeredLayoutProvider`   | Sugiyama flow with PORTS, nesting, orthogonal bend-routed edges (primary AEC engine) |
|  [02]   | `force`           | `ForceLayoutProvider`     | force-directed (Eades/Fruchterman-Reingold) placement                                |
|  [03]   | `stress`          | `StressLayoutProvider`    | stress-majorization placement (distance-faithful)                                    |
|  [04]   | `mrtree`          | `MrTreeLayoutProvider`    | tidy multi-root tree layout                                                          |
|  [05]   | `radial`          | `RadialLayoutProvider`    | radial / concentric-ring layout (sun-path/compass diagrams)                          |
|  [06]   | `rectpacking`     | `RectPackingProvider`     | disjoint-rectangle packing of independent components (no-edge node sets)             |
|  [07]   | `sporeOverlap`    | `SporeOverlapProvider`    | overlap-removal pass (separate overlapping fixed-size nodes)                         |
|  [08]   | `sporeCompaction` | `SporeCompactionProvider` | post-placement compaction (shrink whitespace, preserve relative order)               |
|  [09]   | `fixed`           | `FixedLayoutProvider`     | honor pre-assigned `x`/`y` + `position`/`bendPoints` (passthrough placement)         |

[ENTRYPOINT_SCOPE]: the option vocabulary (`pyelk.options` — `ALIASES`, `DEFAULTS`, helpers)

`pyelk.options` resolves layout options: `ALIASES` maps short keys onto canonical `elk.*` keys, `DEFAULTS` seeds the global option floor, and the resolver helpers fold global → parent → element overrides into the effective option set. A `LayeredPolicy` lowers its fields onto the `elk.*` keys — the option document is the knob surface, never a positional argument family — and `get_padding`/`get_spacing`/`get_effective_options` re-read a resolved value so a design page sizes a swimlane to the resolved container padding rather than re-deriving it.

| [INDEX] | [SURFACE]                                       | [SHAPE] | [CAPABILITY]                                                             |
| :-----: | :---------------------------------------------- | :------ | :----------------------------------------------------------------------- |
|  [01]   | `ALIASES`                                       | dict    | the 11-entry short-key → canonical `elk.*` key map; keys below           |
|  [02]   | `DEFAULTS`                                      | dict    | the seeded option floor a `layout` call inherits; load-bearing set below |
|  [03]   | `resolve_algorithm(name)`                       | resolve | short/aliased name → FQ algorithm id                                     |
|  [04]   | `resolve_option_key(key)`                       | resolve | short → canonical `elk.*` option key                                     |
|  [05]   | `get_effective_options(element, …)`             | resolve | resolved option set (global → parent → element)                          |
|  [06]   | `get_algorithm(element, …)`                     | resolve | resolved algorithm id for one element                                    |
|  [07]   | `get_direction(element, …)`                     | resolve | resolved layout direction for one element                                |
|  [08]   | `get_padding(element, …)`                       | resolve | resolved `{left, top, right, bottom}` padding dict (from `elk.padding`)  |
|  [09]   | `get_spacing(element, key, …, default=20.0)`    | resolve | resolved float spacing for a spacing key                                 |
|  [10]   | `get_option(element, key, default=None)`        | resolve | a single resolved option value                                           |
|  [11]   | `merge_options(base, override) -> dict`         | resolve | merged option dict (override wins)                                       |
|  [12]   | `parse_padding(value)` / `parse_kvector(value)` | parse   | decode k-vector `"[left=12,...]"`/`"(x,y)"` → `dict[str, float]`         |
|  [13]   | `parse_kvector_chain(value)`                    | parse   | decode chained k-vectors → `list`                                        |

- `[01]-[ALIASES]`: `algorithm`, `direction`, `spacing`, `layering.strategy`, `hierarchyHandling`, `portConstraints`, `port.side`, `port.index`, `layerConstraint`, `position`, `bendPoints`.
- `[02]-[DEFAULTS]` (the floor a `LayeredPolicy` overrides): `elk.direction=RIGHT`, `elk.spacing.nodeNode=20.0`, `elk.padding=[left=12,...]`, `elk.portConstraints=UNDEFINED`, `elk.layered.crossingMinimization.strategy=LAYER_SWEEP`, `elk.layered.layering.strategy=LONGEST_PATH`, `elk.hierarchyHandling=SEPARATE_CHILDREN`.

[ENTRYPOINT_SCOPE]: the graph helpers (`pyelk.graph` / `pyelk.elk`)

`pyelk.graph` helpers (re-exported through `pyelk.elk`) operate on the ELK-JSON `dict` — index the node set, normalize edges, validate the document, size a compound graph, deep-copy before mutation. A design page calls `validate_graph` at the boundary to fail a malformed adjacency frame as a typed `InvalidGraphException` before the layout pass, `normalize_edges` to canonicalize the source/target shape, and `deep_copy_graph` to keep the input immutable while `layout` writes positions in place.

| [INDEX] | [SURFACE]                                       | [SHAPE]   | [CAPABILITY]                                                              |
| :-----: | :---------------------------------------------- | :-------- | :------------------------------------------------------------------------ |
|  [01]   | `validate_graph(graph) -> None`                 | validate  | raise `InvalidGraphException` on bad ids/missing fields                   |
|  [02]   | `validate_id(element_id) -> str`                | validate  | enforce a non-empty unique element id, returning the normalized id        |
|  [03]   | `collect_nodes(graph)` / `collect_edges(graph)` | index     | nodes+ports flatten to an id → element dict; edges to a flat `list[dict]` |
|  [04]   | `normalize_edges(graph) -> None`                | normalize | canonicalize every edge to `{sources, targets}` list shape in place       |
|  [05]   | `compute_graph_size(graph)`                     | size      | compute the bounding extent of a (sub-)graph                              |
|  [06]   | `set_defaults(graph)`                           | seed      | apply the `DEFAULTS` option floor onto a graph document                   |
|  [07]   | `deep_copy_graph(graph) -> dict`                | copy      | structural deep copy; input immutable while `layout` writes positions     |
|  [08]   | `elk.get_layout_provider(algorithm_id)`         | resolve   | provider class for a FQ algorithm id (the internal dispatch)              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- model: the graph IS a plain ELK-JSON `dict`, never a wrapped class — the `visualization/diagram/layout#LAYOUT` owner lowers the `rustworkx` `PyDiGraph` (or the `data/tabular#GRAPH` adjacency frame) to `{"id":"root","children":[...],"edges":[...]}` keyed on the stable stringified node index, so no re-keying enters; `ELK.layout` writes `x`/`y` onto each child and `sections` onto each edge, read back off `collect_nodes(result)[str(i)]["x"]`.
- placement: `ELK().layout(graph, layout_options=...)` is the one synchronous placement boundary, run inside the layout owner's `_render` kernel that `_compute` offloads onto `to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)` — never on the event loop, never under a second limiter; the engine is constructed once and one `layout` call covers every algorithm, the `elk.algorithm` option selecting the provider.
- ports + nesting: the capability `fast-sugiyama`/`grandalf`/`rustworkx` lack — a node carries typed `ports` under `elk.portConstraints`, an edge endpoint id may be a port id so the route connects to the port coordinate rather than the node center, and a compound node carries its own `children`+`edges`, laid out as an enclosed sub-graph then sized to contain them; center-to-center kinds stay on `fast-sugiyama`.
- routing: a long layered edge is orthogonally routed and its `sections[].bendPoints` carry the polyline waypoints; the layout owner threads `[startPoint, *bendPoints, endPoint]` into `RouteMap[(source, target)]`, the native port-aware equivalent of `grandalf`'s `route_with_lines` — the one admitted engine whose routing respects ports and obstacle boxes.
- option: `LayeredPolicy` fields lower to the `layoutOptions` dict (`direction`→`elk.direction`, crossing→`elk.layered.crossingMinimization.strategy`, layering→`elk.layered.layering.strategy`, nesting→`elk.hierarchyHandling`, ports→`elk.portConstraints`), validated against `ELK().known_layout_options()`; a new layout knob is one `LayeredPolicy` field mapped to one `elk.*` key, never a new entrypoint.
- result: `layout` returns the same document with positions filled — `children[].{x,y,width,height}`, `ports[].{x,y}`, `edges[].sections[]` — which the layout owner folds into the `LayoutResult` `(LayoutMap, RouteMap)`: `collect_nodes(result)` filtered to the stable node ids (port entries excluded from `LayoutMap`), each edge's `[startPoint, *bendPoints, endPoint]` to `RouteMap`, no intermediate parse; compound extents feed the `Swimlane`/zone glyph sizes.
- failure: every failure is an `ElkError` (or leaf subclass) mapped to the runtime fault rail — `InvalidGraphException` for a malformed graph, `UnsupportedConfigurationException` for a bad option combination, `UnsupportedGraphException` for an unlayoutable shape — recorded in the diagnostic, and `graph.validate_graph` at the boundary fails a bad adjacency frame before the layout pass.
- evidence: each placement records node count, edge count, resolved algorithm id, direction, and port-constraint/hierarchy mode into the `core/receipt#RECEIPT` `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm, bytes)` case, `algorithm` the ELK tag (`elk-layered`, `elk-radial`, ...); the content key is the `xxh3_128` digest over the input graph wire joined to the resolved positions and edge sections, so a re-placement under different `layoutOptions` keys distinctly.

[STACKING]:
- A ports/nesting/orthogonal diagram kind in `visualization/diagram/layout#LAYOUT` selects `LayeredPolicy.engine = HierarchyEngine.ELK`: `_render` builds the `rustworkx` `PyDiGraph` from the `data/tabular#GRAPH` adjacency frame, lowers it to ELK-JSON, calls `ELK().layout` with the policy's `elk.*` options, and folds the result into the `LayoutResult` — all inside the one `to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)` `anyio` lane wrapped by the runtime `async_boundary`.
- `HierarchyEngine` `StrEnum` gains an `ELK` member beside `GRANDALF`/`RUSTWORKX`/`fast-sugiyama`; the `LayeredPolicy` `expression.tagged_union` case gains an `elk.*` option mapping rather than a new case, and the `match`/`case` over `policy.tag` stays the one total dispatch — the `expression` rail owns the discriminant, pyelk owns one arm's body.
- Placement is content-addressed on the universal `xxh3_128` rail: the layout owner digests the `rustworkx` `node_link_json` wire joined to the resolved positions and routed sections, so two placements of one graph under distinct `layoutOptions` key distinctly, under the same `ContentIdentity` owner every artifact receipt uses.
- ELK-JSON documents trace through the universal rails: `ELK.layout(..., logging=True)` rides a `structlog`-bound diagnostic when the layout owner traces, and the `beartype`-checked `LayoutResult` shape builds straight off the positioned document with no class-model intermediary, folding into the `msgspec`-backed glyph owners.
- pyelk is co-resident with `fast-sugiyama`/`grandalf`/`rustworkx` by distinct capability: `fast-sugiyama` owns center-to-center layered placement, `grandalf` curved-spline routes until removal, `rustworkx` force/radial/topological data-plane layout and the content-key wire, pyelk the ports/nesting/orthogonal kinds none handle — the `HierarchyEngine` selection routes each diagram to its categorical-best provider, no two engines owning one shape.

[LOCAL_ADMISSION]:
- Admitted as the in-process pure-Python ELK engine behind `visualization/diagram/layout#LAYOUT`'s `HierarchyEngine.ELK` arm for ports/nesting/orthogonal-routed AEC and technical diagrams, composing the `rustworkx` graph lowered to ELK-JSON, the `expression.tagged_union` policy on the `elk.*` vocabulary, the `anyio.to_thread.run_sync` lane, the `xxh3_128` content key, and the `ElkError` rail at the runtime `async_boundary`, contributing the `ArtifactReceipt.Diagram` case with the ELK tag.

[RAIL_LAW]:
- Package: `pyelk`
- Owns: ELK graph coordinate assignment and edge-route geometry — the nine Eclipse Layout Kernel providers over a recursive ELK-JSON document, with typed PORTS (side/index constraints), recursive HIERARCHICAL NESTING (compound nodes sized to enclose children), and native ORTHOGONAL edge routing emitting per-edge `sections[].bendPoints` waypoints, returning the same document with positions and route geometry filled in
- Accept: ports/nesting/orthogonal-routed diagram placement as the pure-Python in-process ELK engine composing the `rustworkx` graph, the `expression.tagged_union` policy, the `anyio` placement lane, and the `xxh3_128` key
- Reject: a hand-rolled port-placement, nesting, or orthogonal-routing loop where the ELK providers own it; an `elkjs`/Java-ELK subprocess where this in-process call replaces it; a wrapped node/edge class model over the native ELK-JSON dict; force/radial/topological layout for center-to-center diagrams where `rustworkx` and `fast-sugiyama` own it; curved-spline routes where `grandalf` `route_with_splines` covers it until removal; graph analysis where the `data/graph#GRAPH` `rustworkx` kernel owns it; SVG emission where `visualization/diagram/draw#DRAW` owns it; the unregistered `disco` alias; a re-keying of coordinates off the stable node index; a synchronous layout left on the event loop; identity minting the runtime owns
