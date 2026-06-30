# [PY_ARTIFACTS_API_DRAWPYO]

`drawpyo` is the categorical-best owner of the draw.io / diagrams.net (`.drawio`) editable-diagram egress and ingest concern for the artifacts diagramming rail: a pure-Python `File` -> `Page` -> `XMLBase` document spine whose `File.write` serializes the native mxGraph `.drawio` XML, a closed diagram-element vocabulary (`Object`/`Group` boxes, `Edge`/`Point` connectors, `TextFormat` label styling, `Geometry`/`EdgeGeometry`) whose every style axis (`line_pattern`/`waypoints`/`connection`/`pattern`/`jumpStyle`) is a TOML-backed bounded vocabulary that rejects off-domain values, the draw.io shape-library factory (`object_from_library` over the built-in `general`/`flowchart`/`infographics` TOML databases), the Edit-Style round-trip (`from_style_string`/`apply_style_string` ingest a draw.io UI style string verbatim), the auto-layout high-level builders (`TreeDiagram`/`BinaryTreeDiagram`/`ClassDiagram` hierarchies, `BarChart`/`PieChart`/`Legend` infographics) that fold a nested `dict` into a positioned diagram, and the inverse `load_diagram` parser (`.drawio`/`.xml` -> a `ParsedDiagram` of typed `Object`/`Edge` rows with `get_by_id`). The owning design page lowers the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph`/`GlyphStyle`/`MarkerKind` closed grammar — the SAME grammar the sibling `visualization/diagram/draw#DRAW` `drawsvg` arm consumes — into the `Object`/`Edge` vocabulary, serializes through one `File.write` off the runtime `async_boundary`/`to_thread` seam, content-keys the emitted bytes through `rasm.runtime.content_identity#ContentIdentity`, and contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case; drawpyo owns the editable-`.drawio` wire format and never rasterizes (raster routes to `resvg-py`/`vl-convert`/`pyvips`), never re-implements graph layout the `rustworkx`/`pyelk`/`fast-sugiyama` owners hold (drawpyo's `TreeDiagram` is the built-in hierarchical fallback, not the routing engine), and never mints the content identity the runtime owns.

## [01]-[PACKAGE_SURFACE]

- package: `drawpyo`
- import: `drawpyo`
- owner: `artifacts`
- rail: diagram
- license: MIT
- installed: `0.2.5`
- build-floor: `Requires-Python >=3.10`; pure-Python (no compiled extension, no abi gate) — resolves on cp315 with no `; python_version` marker. The only conditional dependency is `toml >= 0.10.2; python_version < '3.11'`; on 3.11+ drawpyo imports the stdlib `tomllib` for its shape/style databases, so the dependency closure is empty on the workspace interpreter.
- entry points: none (library only)
- capability: native `.drawio` (mxGraph XML) document author and serialize via the `File`/`Page` spine; the `Object`/`Group`/`Edge`/`Point`/`TextFormat` diagram-element vocabulary with TOML-validated style axes; the draw.io shape-library factory `object_from_library` over the built-in `general`/`flowchart`/`infographics` databases; the Edit-Style round-trip (`from_style_string`/`apply_style_string`/`apply_attribute_dict`); container parenting with autosize/autocontract; auto-layout hierarchical builders (`TreeDiagram`/`BinaryTreeDiagram`/`ClassDiagram`) and infographic builders (`BarChart`/`PieChart`/`Legend`) that fold a nested `dict` into a positioned diagram; the inverse `load_diagram` `.drawio` parser to a typed `ParsedDiagram`; the `StandardColor`/`ColorScheme`/`PageSize` value objects.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document spine
- rail: diagram

The `File` -> `Page` -> element tree is the one document model; `File` owns serialization and disk write, `Page` owns one canvas (the three nested `diagram`/`mxGraphModel`/`root` mxGraph tags plus the two mandatory empty `mxCell` roots), and every drawable derives `XMLBase` so `.xml` recursively assembles the document string. The owning page builds this spine once per diagram artifact and never touches the raw XML.

| [INDEX] | TYPE      | KIND          | ROLE                                                                                |
| :-----: | --------- | ------------- | ----------------------------------------------------------------------------------- |
|  [01]   | `File`    | document root | `.drawio` file; holds `pages`, owns `xml` serialization and `write(...)` disk egress |
|  [02]   | `Page`    | canvas        | one diagram page; holds `objects`, owns page-geometry attrs, links into a `File`     |
|  [03]   | `XMLBase` | element base  | the export base every drawable derives; `xml`/`xml_open_tag`/`xml_close_tag`/`xml_ify` |

[PUBLIC_TYPE_SCOPE]: diagram-element vocabulary
- rail: diagram

`DiagramBase(XMLBase)` is the styled-element base carrying the `style`/`style_attributes`/`apply_*` machinery; `Object` is the box vertex, `Edge` is the connector, `Group` is a geometry-only multi-object handle, and `Geometry`/`EdgeGeometry`/`EdgeLabel`/`Point`/`TextFormat` are the sub-element carriers. This is the closed vocabulary the `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` case lowers onto — `Node` -> `Object`, `Edge` -> `Edge`, `Swimlane` -> a parent `Object` container, `Annotation`/`Marker` -> a styled `Object` — through one total `match`, never a per-mark factory.

| [INDEX] | TYPE           | BASE          | ROLE                                                                          |
| :-----: | -------------- | ------------- | ----------------------------------------------------------------------------- |
|  [01]   | `DiagramBase`  | `XMLBase`     | styled-element base; `style`/`style_attributes`/`apply_style_string`/`from_style_string` |
|  [02]   | `Object`       | `DiagramBase` | the box/shape vertex; geometry, fill/stroke/effect style, parenting, edge tracking |
|  [03]   | `BasicObject`  | `Object`      | bare `Object` alias for un-templated boxes                                    |
|  [04]   | `Edge`         | `DiagramBase` | the connector; `source`/`target`, waypoint/connection/pattern style, line ends, anchors |
|  [05]   | `BasicEdge`    | `Edge`        | bare `Edge` alias                                                             |
|  [06]   | `Group`        | (plain)       | geometry-only handle over many `Object`s; `position`/`center_position`/`size` move the set |
|  [07]   | `Geometry`     | `DiagramBase` | an `Object`'s `mxGeometry` (`x`/`y`/`width`/`height`/`size`)                  |
|  [08]   | `EdgeGeometry` | `DiagramBase` | an `Edge`'s `mxGeometry` carrying the waypoint `Point` array                  |
|  [09]   | `EdgeLabel`    | `DiagramBase` | a standalone edge-label `mxCell`                                              |
|  [10]   | `Point`        | `DiagramBase` | one `mxPoint` (`x`/`y`) waypoint                                              |
|  [11]   | `TextFormat`   | `DiagramBase` | label typography (`fontFamily`/`fontSize`/`fontColor`/`align`/`bold`/`italic`/`direction`) |

[PUBLIC_TYPE_SCOPE]: high-level auto-layout & infographic builders (`drawpyo.diagram_types`)
- rail: diagram

`drawpyo.diagram_types` carries the batteries-included diagram generators: a hierarchical `TreeDiagram` (and its binary specialization) that owns its own `File`/`Page`, ingests a nested `dict` via `from_dict`, runs `auto_layout`, draws orthogonal/straight/curved `connect` edges, and writes — the built-in layout fallback when the `rustworkx`/`pyelk`/`fast-sugiyama` routing engines are not engaged. The infographic builders assemble a self-contained `Group` of `Object`s from a `dict[str, float]`.

| [INDEX] | TYPE                | BASE          | ROLE                                                                         |
| :-----: | ------------------- | ------------- | ---------------------------------------------------------------------------- |
|  [01]   | `TreeDiagram`       | (plain)       | hierarchical generator; owns `File`/`Page`, `from_dict`/`auto_layout`/`connect`/`write` |
|  [02]   | `NodeObject`        | `Object`      | a tree node carrying `tree_parent`/`tree_children`/`add_child`/`add_peer`     |
|  [03]   | `TreeGroup`         | `Group`       | one laid-out tree level's sibling group; `center_parent`                      |
|  [04]   | `BinaryTreeDiagram` | `TreeDiagram` | binary specialization; `add_left`/`add_right`, `from_dict` with `directional` coloring |
|  [05]   | `BinaryNodeObject`  | `NodeObject`  | binary node with `left`/`right` two-slot child accessors                      |
|  [06]   | `ClassDiagram`      | `TreeDiagram` | UML-style class tree; `create_from_module(mdl)` introspects a Python module   |
|  [07]   | `BarChart`          | (plain)       | bar infographic from `dict[str, float]`; axis/ticks/labels into one `Group`   |
|  [08]   | `PieChart`          | (plain)       | pie infographic from `dict[str, float]`; `PieSlice` ring into one `Group`     |
|  [09]   | `Legend`            | (plain)       | color-key legend from `dict[str, color]` into one `Group`                     |
|  [10]   | `List`              | `Object`      | a draw.io list box; `list_items`/`add_item`/`remove_item` with autosize       |
|  [11]   | `PieSlice`          | `Object`      | one infographic pie wedge (`slice_value`/`startAngle`/`endAngle`)             |

[PUBLIC_TYPE_SCOPE]: round-trip ingest (`drawpyo.drawio_import`) & value objects (`drawpyo.utils`)
- rail: diagram

`load_diagram` is the inverse of `File.write`: it parses a `.drawio`/`.xml` document into a `ParsedDiagram` of typed `Object`/`Edge` rows addressable by draw.io cell id — the ingest arm a round-trip (read an existing template, mutate, re-emit) consumer needs. `RawMxCell`/`RawGeometry` are the pre-build raw rows. The `utils` value objects supply bounded color and page-size vocabularies.

| [INDEX] | TYPE            | KIND            | ROLE                                                                       |
| :-----: | --------------- | --------------- | -------------------------------------------------------------------------- |
|  [01]   | `ParsedDiagram` | parse result    | `.shapes`/`.edges` typed rows, `get_by_id(cell_id)`, `element_count`        |
|  [02]   | `RawMxCell`     | raw dataclass   | one parsed `mxCell` (`id`/`parent`/`children`/`value`/`style`/`is_vertex`/`is_edge`/`source`/`target`/`geometry`) |
|  [03]   | `RawGeometry`   | raw dataclass   | one parsed `mxGeometry` (`x`/`y`/`width`/`height`/`relative`/`points`)       |
|  [04]   | `StandardColor` | `str`-Enum      | the 118-row draw.io palette (`StandardColor.BLUE5`, `.NONE`) — a hex `str`   |
|  [05]   | `ColorScheme`   | value object    | a `(fill_color, stroke_color, font_color)` bundle applied to an `Object`     |
|  [06]   | `PageSize`      | tuple-Enum      | `(width, height)` presets — ISO `A0..A7`/`B4..B5` × landscape/portrait, US sizes, `ASPECT16BY9` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document spine — author, serialize, write
- rail: diagram

`File` and `Page` are the one document spine. `File(file_name, file_path)` constructs the `.drawio` container; `Page(file=...)` adds a canvas (auto-linked through the `Page.file` setter, or `File.add_page`); `File.xml` recursively serializes the whole document and `File.write(file_path=, file_name=, overwrite=)` returns the written absolute path. The owning page builds this once per diagram artifact; `Page.size_preset=PageSize.A3LANDSCAPE` seeds the canvas extent from the bounded `PageSize` vocabulary.

| [INDEX] | MEMBER                                                              | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `File(file_name='Drawpyo Diagram.drawio', file_path=<~/Drawpyo Charts>)` | construct | the `.drawio` document container                           |
|  [02]   | `File.add_page(page)` / `File.remove_page(page)`                    | mutate    | attach/detach a `Page` (by object, name, or index)         |
|  [03]   | `File.write(file_path=None, file_name=None, overwrite=True)`        | egress    | serialize and write the `.drawio` to disk, return the path |
|  [04]   | `File.xml` (property)                                               | serialize | the full `.drawio` XML string (the in-memory bytes source) |
|  [05]   | `File.stats()`                                                      | query     | `"Pages: N \| Objects: M"` summary                          |
|  [06]   | `Page(file=None, name=, size_preset=, width=, height=, grid=, ...)` | construct | one canvas; `size_preset=PageSize.*` overrides width/height |
|  [07]   | `Page.add_object(obj)` / `Page.remove_object(obj)`                  | mutate    | place/remove a drawable on the canvas                       |

[ENTRYPOINT_SCOPE]: `Object` — box construct, style, geometry, parent, edge-track
- rail: diagram

`Object(value, position, page=, width=, height=, **style)` is the one box constructor; every style axis is a keyword (`fillColor`/`strokeColor`/`rounded`/`glass`/`shadow`/`comic`/`sketch`/`opacity`/`line_pattern`/`text_format`). The `position`/`center_position`/`size` properties are the geometry surface (position is page-absolute, auto-rebased when a `parent` is set). Container parenting is one axis: pass `parent=`, set `autosize_to_children=True`/`autocontract=`, and the box expands to hug its children — this is how a `Swimlane` glyph lowers. `out_edges`/`in_edges` track connectors automatically as `Edge.source`/`target` are bound. The `create_from_*`/`format_as_library_object` factories are the discrimination surface — one `Object` owner, never an `add_rect`/`add_ellipse` family (the SHAPE is a library-key string, not a subtype).

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `Object(value='', position=(0,0), page=, width=120, height=80, **style)`        | construct | a box vertex with full style keywords                      |
|  [02]   | `Object.create_from_template_object(template_object, value=, position=, page=)` | factory   | clone another `Object`'s style with new text/position      |
|  [03]   | `Object.create_from_library(library, obj_name)` / `format_as_library_object(library, obj_name)` | factory | style from a built-in (`'general'`/`'flowchart'`/`'infographics'`) or custom TOML library |
|  [04]   | `Object.create_from_style_string(style_string)` / `from_style_string(style_string)` | factory | instantiate from a draw.io Edit-Style string               |
|  [05]   | `Object.position` / `center_position` / `position_rel_to_parent` (props)        | geometry  | page-absolute / centroid / parent-relative top-left        |
|  [06]   | `Object.size` / `width` / `height` (props, via `Geometry`)                       | geometry  | box extent (setting width/height re-fits an autosizing parent) |
|  [07]   | `Object.parent` (prop) / `add_object(child)` / `remove_object(child)`           | parent    | container parenting; child geometry rebases to the parent  |
|  [08]   | `Object.resize_to_children()` / `move_wo_children(position)`                     | parent    | hug children / move the container leaving children in place |
|  [09]   | `Object.line_pattern` (prop; TOML-validated) / `dashed` / `dashPattern`          | style     | stroke dash vocabulary; an off-vocabulary value raises `ValueError` |
|  [10]   | `Object.apply_style_string(s)` / `apply_attribute_dict(d)` / `add_style_attribute(k)` | style | merge a draw.io style string / attribute dict / one style key |

[ENTRYPOINT_SCOPE]: `Edge` — connector construct, bind, route, style
- rail: diagram

`Edge(source=, target=, label=, waypoints=, connection=, pattern=, **style)` is the one connector. `source`/`target` bind to `Object`s (the setters auto-register the edge on each endpoint's `out_edges`/`in_edges`). The three primary style axes — `waypoints` (`orthogonal`/`straight`/`curved`/`isometric`/`isometric_vertical`/`entity_relation`/`horizontal`/`vertical`), `connection` (`line`/`link`/`arrow`/`simple_arrow`), `pattern` (`solid`/`dashed_small`/`dashed_medium`/`dashed_large`/`dotted_small`/`dotted_medium`/`dotted_large`) — are the exact TOML-keyed bounded vocabularies that raise `ValueError` off-domain and assemble into the `baseStyle` string on export; the `line_end_*` arrowheads draw from the `classic`/`block`/`open`/`diamond`/`circle`/`async`/`cross`/`ER*` (`ERone`/`ERmany`/`ERzeroToOne`/...) glyph set. `line_end_source`/`line_end_target` + `endFill_*` pick the arrowheads; `entryX/Y`/`exitX/Y` (+ `*Dx`/`*Dy`) pin perimeter anchors; `add_point`/`add_point_pos` push explicit waypoints; `label_position`/`label_offset` place the label. This is the `DiagramGlyph(tag="edge")` lowering target.

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `Edge(page=, source=, target=, label=, waypoints='orthogonal', connection='line', pattern='solid', **style)` | construct | a connector with bounded waypoint/connection/pattern style |
|  [02]   | `Edge.source` / `Edge.target` (props)                                            | bind      | endpoints; setters auto-track on the `Object`'s edge lists  |
|  [03]   | `Edge.waypoints` / `connection` / `pattern` (props; TOML-validated)             | style     | the three primary edge style axes (off-domain -> `ValueError`) |
|  [04]   | `Edge.line_end_source` / `line_end_target` / `endFill_source` / `endFill_target` | style     | arrowhead glyph + fill at each end (`startArrow`/`endArrow` aliases) |
|  [05]   | `Edge.entryX/entryY/exitX/exitY` (+ `entryDx/Dy`, `exitDx/Dy`)                    | anchor    | perimeter attach points (0..1) and pixel offsets            |
|  [06]   | `Edge.add_point(x, y)` / `add_point_pos((x, y))`                                 | route     | append an explicit `Point` waypoint to `EdgeGeometry.points` |
|  [07]   | `Edge.jumpStyle` (`arc`/`gap`/`sharp`/`line`; validated) / `jumpSize` / `flowAnimation` | style | line-crossing jumps; marching-ants animation                |
|  [08]   | `Edge.label` / `label_position` / `label_offset`                                | label     | edge text and its position (-1 source .. 1 target) / offset |
|  [09]   | `Edge.strokeColor` / `strokeWidth` / `fillColor` (props; validated)             | style     | `color_input_check` (hex/`none`/`default`) + `width_input_check` (1..999) gate inputs |

[ENTRYPOINT_SCOPE]: shape-library factory, style helpers, `TextFormat`, `Group`
- rail: diagram

`object_from_library(library, obj_name, **kwargs)` is the one factory pulling a styled `Object` from a draw.io shape database (the built-in `'general'`/`'flowchart'`/`'infographics'`, or a custom TOML imported via `import_shape_database`). `style_str_from_dict`/`import_shape_database`/`color_input_check`/`width_input_check` are the boundary helpers. `TextFormat` is the one label-typography carrier (`font_style` folds bold/italic/underline into the draw.io numeric code). `Group` moves many `Object`s as one geometry handle.

| [INDEX] | MEMBER                                                              | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `object_from_library(library, obj_name, **kwargs)`                  | factory   | a styled `Object` from a shape library (built-in name or TOML dict) |
|  [02]   | `import_shape_database(file_name, relative=False)`                  | load      | parse a TOML shape/style database (resolves `inherit` chains) |
|  [03]   | `style_str_from_dict(style_dict)`                                   | encode    | concatenate a `{baseStyle, k=v, ...}` dict into a draw.io style string |
|  [04]   | `color_input_check(s)` / `width_input_check(w)`                     | validate  | hex/`none`/`default` color gate; 1..999 stroke-width clamp  |
|  [05]   | `TextFormat(fontFamily=, fontSize=, fontColor=, align=, verticalAlign=, bold=, italic=, underline=, direction=, ...)` | construct | label typography carrier |
|  [06]   | `TextFormat.font_style` / `fontStyle` (props)                       | encode    | fold bold/italic/underline into the draw.io 0..7 style code |
|  [07]   | `Group(objects=[...])` / `Group.add_object(obj_or_list)`            | construct | a geometry-only handle; `position`/`center_position`/`size` move the whole set |

[ENTRYPOINT_SCOPE]: high-level builders — `dict` -> positioned diagram
- rail: diagram

The `diagram_types` builders fold a nested `dict` (or `dict[str, float]`) into a fully positioned diagram in one call. `TreeDiagram.from_dict` builds the node graph, `auto_layout` assigns coordinates, and `write` emits the `.drawio`; `coloring` (`depth`/`hash`/`type`, plus `directional` for binary) keys a `colors` palette list. The infographic builders return one self-contained `Group` placed via `add_to_page`.

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `TreeDiagram(direction='down', link_style='orthogonal', level_spacing=60, item_spacing=15, file_name=, file_path=)` | construct | a hierarchical diagram owning its own `File`/`Page`        |
|  [02]   | `TreeDiagram.from_dict(data, *, colors=None, coloring='depth', **kwargs)`        | build     | fold a nested `dict`/`list` into the node graph            |
|  [03]   | `TreeDiagram.auto_layout()` / `connect(source, target)` / `draw_connections()`  | layout    | assign coordinates; draw direction-aware orthogonal edges  |
|  [04]   | `TreeDiagram.add_object(obj, tree_parent=)` / `roots` (prop) / `write(**kwargs)` | mutate    | add a `NodeObject`; the root list; serialize the `.drawio`  |
|  [05]   | `BinaryTreeDiagram.from_dict(...)` / `add_left(parent, child)` / `add_right(parent, child)` | build | binary tree with two-slot children (`directional` coloring) |
|  [06]   | `ClassDiagram.create_from_module(mdl, **kwargs)`                                 | build     | introspect a Python module into a UML-style class tree     |
|  [07]   | `BarChart(data, position=, bar_width=, max_bar_height=, show_axis=, title=, bar_colors=, ...)` | build | bar infographic; `add_to_page(page)` / `group` / `update_data(d)` |
|  [08]   | `PieChart(data, position=, size=, title=, slice_colors=, label_formatter=, ...)` | build     | pie infographic; `add_to_page(page)` / `group` / `update_data(d)` |
|  [09]   | `Legend(mapping, position=, title=, background_color=, ...)`                      | build     | color-key legend; `add_to_page(page)` / `update_mapping(m)`  |

[ENTRYPOINT_SCOPE]: round-trip ingest — `.drawio` -> typed rows
- rail: diagram

`load_diagram(path)` is the inverse egress: parse an existing `.drawio`/`.xml` into a `ParsedDiagram` whose `.shapes`/`.edges` are the same typed `Object`/`Edge` instances the author path builds, addressable by draw.io cell id. This is the round-trip path — read a template diagram, mutate its rows, re-emit through `File.write`.

| [INDEX] | MEMBER                                          | KIND    | ROLE                                                        |
| :-----: | ----------------------------------------------- | ------- | ----------------------------------------------------------- |
|  [01]   | `load_diagram(file_path)`                       | parse   | `.drawio`/`.xml` -> a `ParsedDiagram` (`FileNotFoundError`/`ValueError` on bad input) |
|  [02]   | `ParsedDiagram.shapes` / `edges`                | rows    | the typed `Object` / `Edge` rows                            |
|  [03]   | `ParsedDiagram.get_by_id(cell_id)`              | lookup  | one element by its draw.io cell id (`None` if absent)        |
|  [04]   | `ParsedDiagram.element_count` (prop)            | query   | `len(shapes) + len(edges)`                                   |

## [04]-[IMPLEMENTATION_LAW]

- import: `import drawpyo` at boundary scope only; the distribution and import name are both `drawpyo`; the version is `drawpyo.__version__` (`"0.2.5"`). The top-level namespace re-exports `File`/`Page`/`XMLBase`/`StandardColor`/`ColorScheme`/`PageSize`/`load_diagram`; the element vocabulary lives under `drawpyo.diagram` (`Object`/`Edge`/`Group`/`TextFormat`/`Point`/`object_from_library`/...) and the generators under `drawpyo.diagram_types` (`TreeDiagram`/`BarChart`/...). drawpyo is the editable-`.drawio` boundary owner — keep the import at the egress edge, never let the mxGraph object model leak into domain code; the domain holds the `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` grammar and lowers it here.
- document axis: `File` -> `Page` -> element tree is the one document spine; `File.write` serializes the whole tree to `.drawio` XML and `File.xml` exposes the in-memory bytes (the owning page encodes `File.xml` and content-keys those bytes, then writes through the runtime artifact store, never trusting drawpyo's own disk path as the artifact of record). Two empty `mxCell` roots and the three nested `diagram`/`mxGraphModel`/`root` tags are auto-built by `Page` — never hand-emit them.
- element axis: the closed vocabulary is `Object` (box vertex), `Edge` (connector), `Group` (geometry handle), with `Geometry`/`EdgeGeometry`/`EdgeLabel`/`Point`/`TextFormat` sub-carriers; the SHAPE of an `Object` is a draw.io library-key STRING (`object_from_library('flowchart', 'decision')`), never a Python subtype — there is no `Decision`/`Process`/`Terminator` class family, so a new shape is one library row, not one type. Lower the `DiagramGlyph` case through one total `match`: `Node` -> `Object`, `Edge` -> `Edge`, `Swimlane` -> a parent `Object` container, `Annotation`/`Marker` -> a styled `Object`/`object_from_library` row.
- style axis: every edge style axis (`waypoints`/`connection`/`pattern`) and the object `line_pattern` and edge `jumpStyle` are TOML-backed bounded vocabularies whose property setters raise `ValueError` on an off-domain value — treat them as closed enums the `GlyphStyle` projects onto, never free strings; `color_input_check` (hex / `none` / `default`) and `width_input_check` (clamp 1..999) gate color/width at the property boundary. Compose the style through `apply_attribute_dict`/`apply_style_string`/`object_from_library`, never by writing the `style="..."` attribute by hand.
- round-trip axis: `from_style_string`/`apply_style_string` ingest a draw.io UI Edit-Style string verbatim (the user copies "Edit Style" from the app), and `load_diagram` parses a whole `.drawio` back into the same typed `Object`/`Edge` rows — so a template-driven workflow (read an existing sheet, mutate rows by `get_by_id`, re-emit) routes through `load_diagram` -> mutate -> `File.write`, never a second XML parser.
- builder axis: `TreeDiagram`/`BinaryTreeDiagram`/`ClassDiagram` fold a nested `dict` into a positioned hierarchy in one `from_dict` + `auto_layout`; `BarChart`/`PieChart`/`Legend` fold a `dict[str, float]`/mapping into one self-contained `Group`. These are the built-in batteries — the design uses `TreeDiagram` as the hierarchical layout FALLBACK, while the `visualization/diagram/layout#LAYOUT` owner routes real graph layout through `rustworkx`/`pyelk`/`fast-sugiyama` and lowers the resulting coordinates onto bare `Object`/`Edge` here; drawpyo never re-implements the routing engine.
- color axis: `StandardColor` is a 118-row `str`-Enum (a hex string subtype usable anywhere a color string is), `ColorScheme(fill, stroke, font)` bundles the three object color slots, and the `GlyphStyle.fill`/`stroke` palette INDICES the design carries project to hex through the shared `visualization/chart/spec#hex_ramp` before binding to `Object.fillColor`/`strokeColor` — so color traces to one `graphic/color/derive#DERIVE` palette index, never a per-mark `StandardColor` literal chosen at the egress.
- page axis: `PageSize` is a `(width, height)` tuple-Enum with ISO `A0..A7`/`B4..B5` × landscape/portrait, US sizes, and aspect presets; pass `Page(size_preset=PageSize.A3LANDSCAPE)` so the canvas extent comes from the bounded vocabulary, never a hand-typed width/height pair.
- boundary: drawpyo owns the editable-`.drawio` (mxGraph XML) author/serialize/ingest concern; rasterization to PNG/PDF routes to `resvg-py`/`vl-convert`/`pyvips`; the SVG named-layer emission of the SAME `DiagramGlyph` grammar is the sibling `visualization/diagram/draw#DRAW` `drawsvg` arm (drawpyo is the `.drawio` arm, drawsvg the SVG-layer arm — one glyph grammar, two egress targets, never duplicated logic); graph layout/routing is `rustworkx`/`pyelk`/`fast-sugiyama`/`libavoid`; the content identity is `rasm.runtime.content_identity#ContentIdentity`; live UI is out of scope.
- evidence: each `.drawio` egress captures page count, object/edge counts (`File.stats()`), the rendered `.drawio` byte length, and the layout/coloring policy as a `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` row — the SAME case the `visualization/diagram/draw#DRAW` SVG arm and the `visualization/diagram/layout#LAYOUT` owner contribute, never a parallel drawio-only receipt shape.

[STACKING]:
- `expression` rail: the `File.write` egress is wrapped by the owning page's `RuntimeRail`/`async_boundary` so a write fault (a bad `file_path`, an `overwrite=False` collision, an off-domain style `ValueError`) lands as a typed `Result` failure, never a raw exception crossing the domain — the owning page never lets a `ValueError` from a `waypoints`/`jumpStyle` setter escape the diagram rail.
- `msgspec`/`pydantic` rail: `File.stats()` object/edge counts plus the `File.xml` byte length plus the `TreeDiagram`/`BarChart` layout-and-coloring policy populate a `msgspec.Struct` `ArtifactReceipt.Diagram(key, "diagram-drawio", nodes, edges, algorithm)` row on the one shared `core/receipt#RECEIPT` family — the diagram facts are structured, not stringly, and share the case the `drawsvg` arm emits.
- `ContentIdentity` rail: the owning page encodes `File.xml` to bytes and derives the content key through `rasm.runtime.content_identity#ContentIdentity.of("diagram-drawio", xml_bytes)` over the rendered bytes (never a second serialization), so the `.drawio` artifact is content-addressed in the same artifact index the cache-check law uses — drawpyo never mints identity.
- `anyio` rail: drawpyo is pure-Python and fully synchronous; the owning page offloads the `File.write`/`File.xml` serialization onto `to_thread.run_sync(..., limiter=_DIAGRAM_LANES)` off the event loop (the same shared-address-space thread arm the `drawsvg` `_render` and `numpy`-touching siblings take), so a large multi-page `.drawio` serialization never blocks the loop while the `msgspec` receipt owner stays event-loop-side.
- `glyphset` seam: the design lowers the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph`/`GlyphStyle`/`MarkerKind` closed grammar — the SAME grammar `visualization/diagram/draw#DRAW` consumes — onto the `Object`/`Edge`/`Group` vocabulary through one total `match` (`Node` -> `Object`, `Edge` -> `Edge`, `Swimlane` -> parent `Object`, `Annotation`/`Marker` -> styled `Object`), the `GlyphStyle.fill`/`stroke` indices projected to hex through the shared `visualization/chart/spec#hex_ramp`, so one glyph grammar drives both the `.drawio` and the SVG egress with zero per-arm special-casing.
- round-trip seam: `load_diagram` parses an existing `.drawio` template back into the typed `Object`/`Edge` rows the same grammar describes, so a template-driven AEC sheet (read a title-block diagram, mutate rows by `get_by_id`, re-emit) round-trips through one owner without a second parser or a stringly diff.

## [05]-[LOCAL_ADMISSION]

- Package: `drawpyo`
- Owns: native `.drawio` (mxGraph XML) editable-diagram author/serialize via the `File`/`Page`/`XMLBase` spine; the `Object`/`Group`/`Edge`/`Point`/`TextFormat`/`Geometry` diagram-element vocabulary with TOML-validated `waypoints`/`connection`/`pattern`/`line_pattern`/`jumpStyle` style axes; the `object_from_library` shape-library factory over `general`/`flowchart`/`infographics`; the Edit-Style round-trip (`from_style_string`/`apply_style_string`/`apply_attribute_dict`); container parenting (autosize/autocontract); the `TreeDiagram`/`BinaryTreeDiagram`/`ClassDiagram`/`BarChart`/`PieChart`/`Legend` builders; the inverse `load_diagram` `.drawio` parser to a typed `ParsedDiagram`; and the `StandardColor`/`ColorScheme`/`PageSize` value objects.
- Accept: editable-`.drawio` egress and ingest of the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` grammar — node-link, ER, flowchart, Sankey, section-callout, and AEC schedule/legend diagrams emitted as a diagrams.net-editable file the downstream consumer opens and edits, plus template round-trip; the hierarchical-layout fallback when a dedicated routing engine is not engaged; feeding the one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case and the runtime content-key index.
- Reject: a hand-emitted mxGraph `style="..."` string or raw `<mxCell>` XML where `apply_attribute_dict`/`object_from_library`/`File.write` exist; a free-string `waypoints`/`connection`/`pattern`/`jumpStyle` where the TOML-validated property gate owns the vocabulary; a per-shape `Object` subtype family where the library-key string discriminates; a raster operation where `resvg-py`/`vl-convert`/`pyvips` covers it; a second SVG diagram emitter where the `visualization/diagram/draw#DRAW` `drawsvg` arm renders the same grammar; a re-implemented graph layout where `rustworkx`/`pyelk`/`fast-sugiyama`/`libavoid` route; a second `.drawio` XML parser where `load_diagram` ingests; identity minting the runtime owns.
