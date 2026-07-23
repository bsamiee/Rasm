# [PY_ARTIFACTS_API_DRAWPYO]

`drawpyo` owns the editable draw.io (`.drawio`) egress and ingest concern for the artifacts diagram rail: a pure-Python `File` -> `Page` -> `XMLBase` spine whose `File.write` serializes native mxGraph XML, a closed `Object`/`Edge`/`Group`/`Point`/`TextFormat` vocabulary with TOML-bounded style axes, the `object_from_library` shape factory and Edit-Style round-trip, the `dict`-folding `TreeDiagram`/`BarChart` auto-layout builders, and the inverse `load_diagram` parser to a typed `ParsedDiagram`. It owns the editable-`.drawio` wire alone: rasterization routes to `resvg-py`/`vl-convert`/`pyvips`, graph routing to `rustworkx`/`pyelk`/`fast-sugiyama` (`TreeDiagram` is the built-in hierarchical fallback, never the routing engine), and content identity to `rasm.runtime.identity#ContentIdentity`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `drawpyo`
- package: `drawpyo` (MIT)
- module: `drawpyo`
- namespaces: `drawpyo`, `drawpyo.diagram`, `drawpyo.diagram_types`, `drawpyo.drawio_import`, `drawpyo.utils`
- rail: diagram

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document spine

`File` -> `Page` -> element tree is the one document model: `File` owns serialization and disk write, `Page` owns one canvas, and every drawable derives `XMLBase` so `.xml` recursively assembles the document string.

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                                                                         |
| :-----: | :-------- | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `File`    | document root | `.drawio` file; holds `pages`, owns `xml` serialization and `write(...)` disk egress |
|  [02]   | `Page`    | canvas        | one diagram page; holds `objects`, owns page-geometry attrs, links into a `File`     |
|  [03]   | `XMLBase` | element base  | export base every drawable derives; `xml`/`xml_open_tag`/`xml_close_tag`/`xml_ify`   |

[PUBLIC_TYPE_SCOPE]: diagram-element vocabulary

`DiagramBase(XMLBase)` is the styled-element base carrying the `style`/`style_attributes`/`apply_*` machinery; `Object` is the box vertex, `Edge` the connector, `Group` a geometry-only multi-object handle, with `Geometry`/`EdgeGeometry`/`EdgeLabel`/`Point`/`TextFormat` sub-carriers. This closed vocabulary is the `DiagramGlyph` lowering target.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                                               |
| :-----: | :------------- | :------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | `DiagramBase`  | `XMLBase`     | styled-element base; `style`/`style_attributes`/`apply_style_string`/`from_style_string`   |
|  [02]   | `Object`       | `DiagramBase` | box/shape vertex; geometry, fill/stroke/effect style, parenting, edge tracking             |
|  [03]   | `BasicObject`  | `Object`      | bare `Object` for un-templated boxes                                                       |
|  [04]   | `Edge`         | `DiagramBase` | connector; `source`/`target`, waypoint/connection/pattern style, line ends, anchors        |
|  [05]   | `BasicEdge`    | `Edge`        | bare `Edge`                                                                                |
|  [06]   | `Group`        | (plain)       | geometry-only handle over many `Object`s; `position`/`center_position`/`size` move the set |
|  [07]   | `Geometry`     | `DiagramBase` | an `Object`'s `mxGeometry` (`x`/`y`/`width`/`height`/`size`)                               |
|  [08]   | `EdgeGeometry` | `DiagramBase` | an `Edge`'s `mxGeometry` carrying the waypoint `Point` array                               |
|  [09]   | `EdgeLabel`    | `DiagramBase` | a standalone edge-label `mxCell`                                                           |
|  [10]   | `Point`        | `DiagramBase` | one `mxPoint` (`x`/`y`) waypoint                                                           |
|  [11]   | `TextFormat`   | `DiagramBase` | label typography (`fontFamily`/`fontSize`/`fontColor`/`align`/`bold`/`italic`/`direction`) |

[PUBLIC_TYPE_SCOPE]: auto-layout & infographic builders (`drawpyo.diagram_types`)

`drawpyo.diagram_types` carries the batteries-included generators: a hierarchical `TreeDiagram` owning its own `File`/`Page`, ingesting a nested `dict` via `from_dict`, running `auto_layout`, and writing. Infographic builders assemble a self-contained `Group` of `Object`s from a `dict[str, float]`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `TreeDiagram`       | (plain)       | hierarchical generator; owns `File`/`Page`, `from_dict`/`auto_layout`/`connect`/`write` |
|  [02]   | `NodeObject`        | `Object`      | tree node carrying `tree_parent`/`tree_children`/`add_child`/`add_peer`                 |
|  [03]   | `TreeGroup`         | `Group`       | one laid-out tree level's sibling group; `center_parent`                                |
|  [04]   | `BinaryTreeDiagram` | `TreeDiagram` | binary specialization; `add_left`/`add_right`, `from_dict` with `directional` coloring  |
|  [05]   | `BinaryNodeObject`  | `NodeObject`  | binary node with `left`/`right` two-slot child accessors                                |
|  [06]   | `ClassDiagram`      | `TreeDiagram` | UML-style class tree; `create_from_module(mdl)` introspects a Python module             |
|  [07]   | `BarChart`          | (plain)       | bar infographic from `dict[str, float]`; axis/ticks/labels into one `Group`             |
|  [08]   | `PieChart`          | (plain)       | pie infographic from `dict[str, float]`; `PieSlice` ring into one `Group`               |
|  [09]   | `Legend`            | (plain)       | color-key legend from `dict[str, color]` into one `Group`                               |
|  [10]   | `List`              | `Object`      | draw.io list box; `list_items`/`add_item`/`remove_item` with autosize                   |
|  [11]   | `PieSlice`          | `Object`      | one infographic pie wedge (`slice_value`/`startAngle`/`endAngle`)                       |

[PUBLIC_TYPE_SCOPE]: round-trip ingest (`drawpyo.drawio_import`) & value objects (`drawpyo.utils`)

`load_diagram` is the inverse of `File.write`, parsing a `.drawio`/`.xml` into a `ParsedDiagram` of typed `Object`/`Edge` rows addressable by cell id; `RawMxCell`/`RawGeometry` are the pre-build raw rows, and the `utils` value objects supply bounded color and page-size vocabularies.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :-------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `ParsedDiagram` | parse result  | `.shapes`/`.edges` typed rows, `get_by_id(cell_id)`, `element_count`  |
|  [02]   | `RawMxCell`     | raw dataclass | one parsed `mxCell` row (fields below)                                |
|  [03]   | `RawGeometry`   | raw dataclass | one parsed `mxGeometry` row (fields below)                            |
|  [04]   | `StandardColor` | `str`-Enum    | the draw.io palette (`StandardColor.BLUE5`/`.NONE`), a hex `str`      |
|  [05]   | `ColorScheme`   | value object  | a `(fill_color, stroke_color, font_color)` bundle applied to `Object` |
|  [06]   | `PageSize`      | tuple-Enum    | `(width, height)` presets — ISO A/B-series, US sizes, `ASPECT16BY9`   |

`RawMxCell` fields: `id`/`parent`/`children`/`value`/`style`/`is_vertex`/`is_edge`/`source`/`target`/`geometry`; `RawGeometry` fields: `x`/`y`/`width`/`height`/`relative`/`points`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document spine — author, serialize, write

`File(file_name, file_path)` constructs the container, `Page(file=...)` adds a canvas (auto-linked via the `Page.file` setter or `File.add_page`), `File.xml` serializes the whole tree, and `File.write` returns the written absolute path; `Page.size_preset` seeds the extent from the bounded `PageSize` vocabulary.

| [INDEX] | [SURFACE]                                                    | [SHAPE]   | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------------------- | :-------- | :--------------------------------------------------------- |
|  [01]   | `File(file_name=, file_path=)`                               | construct | the `.drawio` document container                           |
|  [02]   | `File.add_page(page)` / `File.remove_page(page)`             | mutate    | attach/detach a `Page` (by object, name, or index)         |
|  [03]   | `File.write(file_path=None, file_name=None, overwrite=True)` | egress    | serialize + write the `.drawio`, return the path           |
|  [04]   | `File.xml` (property)                                        | serialize | the full `.drawio` XML string (the in-memory bytes source) |
|  [05]   | `File.stats()`                                               | query     | `"Pages: N \| Objects: M"` summary                         |
|  [06]   | `Page(file=, name=, size_preset=, …)`                        | construct | one canvas; `size_preset` overrides w/h                    |
|  [07]   | `Page.add_object(obj)` / `Page.remove_object(obj)`           | mutate    | place/remove a drawable on the canvas                      |

[ENTRYPOINT_SCOPE]: `Object` — box construct, style, geometry, parent, edge-track

`Object(value, position, page=, width=, height=, **style)` is the one box constructor, every style axis a keyword; `position`/`center_position`/`size` are the geometry surface (page-absolute, auto-rebased under a `parent`). Container parenting is one axis: `parent=` with `autosize_to_children`/`autocontract` expands the box to hug its children (how a `Swimlane` glyph lowers). A shape is a library-key string via `create_from_library`/`object_from_library`, never an `add_rect`/`add_ellipse` subtype family.

| [INDEX] | [SURFACE]                                                        | [SHAPE]   | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------------- | :-------- | :-------------------------------------------------- |
|  [01]   | `Object(value=, position=, …, width=, height=, **style)`         | construct | a box vertex with full style keywords               |
|  [02]   | `Object.create_from_template_object(…)`                          | factory   | clone another `Object` style with new text/position |
|  [03]   | `Object.create_from_library` / `format_as_library_object`        | factory   | styled `Object` from a built-in/TOML library        |
|  [04]   | `Object.create_from_style_string` / `from_style_string`          | factory   | instantiate from a draw.io Edit-Style string        |
|  [05]   | `Object.position` / `center_position` / `position_rel_to_parent` | geometry  | page-absolute / centroid / parent-relative          |
|  [06]   | `Object.size` / `width` / `height` (via `Geometry`)              | geometry  | box extent (re-fits an autosizing parent)           |
|  [07]   | `Object.parent` / `add_object(child)` / `remove_object(child)`   | parent    | child geometry rebases to the parent                |
|  [08]   | `Object.resize_to_children()` / `move_wo_children(position)`     | parent    | hug children / move container, children stay        |
|  [09]   | `Object.line_pattern` / `dashed` / `dashPattern`                 | style     | stroke dash vocab; off-domain -> `ValueError`       |
|  [10]   | `Object.apply_style_string` / `apply_attribute_dict`             | style     | merge a style string / attr dict / one key          |

[ENTRYPOINT_SCOPE]: `Edge` — connector construct, bind, route, style

`Edge(source=, target=, label=, waypoints=, connection=, pattern=, **style)` is the one connector; the `source`/`target` setters auto-register it on each endpoint's `out_edges`/`in_edges`. Constructor defaults `waypoints='orthogonal'`, `connection='line'`, `pattern='solid'`; the three axes are bounded TOML vocabularies raising `ValueError` off-domain, and `line_end_*` arrowheads draw from the `classic`/`block`/`open`/`diamond`/`circle`/`async`/`cross`/`ER*` glyph set.

| [INDEX] | [SURFACE]                                                      | [SHAPE]   | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------------------- | :-------- | :---------------------------------------------------------- |
|  [01]   | `Edge(page=, source=, target=, label=, **style)`               | construct | connector; bounded waypoint/connection/pattern style        |
|  [02]   | `Edge.source` / `Edge.target` (props)                          | bind      | endpoints; auto-track on the `Object` edge lists            |
|  [03]   | `Edge.waypoints` / `connection` / `pattern`                    | style     | three primary style axes (off-domain -> `ValueError`)       |
|  [04]   | `Edge.line_end_{source,target}` / `endFill_{source,target}`    | style     | arrowhead glyph + fill per end (`startArrow`/`endArrow`)    |
|  [05]   | `Edge.entryX/entryY/exitX/exitY` (+ `entryDx/Dy`, `exitDx/Dy`) | anchor    | perimeter attach points (0..1) and pixel offsets            |
|  [06]   | `Edge.add_point(x, y)` / `add_point_pos((x, y))`               | route     | append a `Point` waypoint to `EdgeGeometry.points`          |
|  [07]   | `Edge.jumpStyle` / `jumpSize` / `flowAnimation`                | style     | line-crossing jumps; marching-ants animation                |
|  [08]   | `Edge.label` / `label_position` / `label_offset`               | label     | edge text and its position (-1 source .. 1 target) / offset |
|  [09]   | `Edge.strokeColor` / `strokeWidth` / `fillColor`               | style     | hex/`none`/`default` color + 1..999 width gate              |

[ENTRYPOINT_SCOPE]: shape-library factory, style helpers, `TextFormat`, `Group`

`object_from_library(library, obj_name, **kwargs)` pulls a styled `Object` from a shape database (the built-in `'general'`/`'flowchart'`/`'infographics'`, or a custom TOML via `import_shape_database`). `TextFormat` carries label typography (`font_style` folds bold/italic/underline into the draw.io numeric code); `Group` moves many `Object`s as one geometry handle.

| [INDEX] | [SURFACE]                                                | [SHAPE]   | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------- | :-------- | :------------------------------------------------------------ |
|  [01]   | `object_from_library(library, obj_name, **kwargs)`       | factory   | styled `Object` from a shape library (built-in or TOML)       |
|  [02]   | `import_shape_database(file_name, relative=False)`       | load      | parse a TOML shape/style database (resolves `inherit` chains) |
|  [03]   | `style_str_from_dict(style_dict)`                        | encode    | concatenate a style dict into a draw.io style string          |
|  [04]   | `color_input_check(s)` / `width_input_check(w)`          | validate  | hex/`none`/`default` color gate; 1..999 stroke-width clamp    |
|  [05]   | `TextFormat(fontFamily=, fontSize=, …, direction=, ...)` | construct | label typography carrier                                      |
|  [06]   | `TextFormat.font_style` / `fontStyle` (props)            | encode    | fold bold/italic/underline into the 0..7 code                 |
|  [07]   | `Group(objects=[...])` / `Group.add_object(obj_or_list)` | construct | geometry handle; `position`/`size` move the set               |

[ENTRYPOINT_SCOPE]: high-level builders — `dict` -> positioned diagram

`diagram_types` builders fold a nested `dict` (or `dict[str, float]`) into a positioned diagram in one call: `TreeDiagram.from_dict` + `auto_layout` + `write`, with `coloring` (`depth`/`hash`/`type`, `directional` for binary) keying a `colors` palette; infographic builders return one self-contained `Group` placed via `add_to_page`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]   | [CAPABILITY]                                            |
| :-----: | :---------------------------------------------------------------- | :-------- | :------------------------------------------------------ |
|  [01]   | `TreeDiagram(direction=, link_style=, …, file_name=, file_path=)` | construct | hierarchical diagram owning its own `File`/`Page`       |
|  [02]   | `TreeDiagram.from_dict(data, *, coloring='depth', **kwargs)`      | build     | fold a nested `dict`/`list` into the node graph         |
|  [03]   | `TreeDiagram.auto_layout()` / `connect` / `draw_connections`      | layout    | assign coords; draw direction-aware orthogonal edges    |
|  [04]   | `TreeDiagram.add_object(obj, …)` / `roots` / `write(**kwargs)`    | mutate    | add a `NodeObject`; serialize the `.drawio`             |
|  [05]   | `BinaryTreeDiagram.from_dict` / `add_left` / `add_right`          | build     | binary tree, two-slot children (`directional` coloring) |
|  [06]   | `ClassDiagram.create_from_module(mdl, **kwargs)`                  | build     | introspect a module into a UML class tree               |
|  [07]   | `BarChart(data, position=, …)`                                    | build     | bar infographic; `add_to_page`/`group`/`update_data`    |
|  [08]   | `PieChart(data, position=, …)`                                    | build     | pie infographic; `add_to_page`/`group`/`update_data`    |
|  [09]   | `Legend(mapping, position=, title=, …)`                           | build     | color-key legend; `add_to_page`/`update_mapping`        |

[ENTRYPOINT_SCOPE]: round-trip ingest — `.drawio` -> typed rows

`load_diagram(path)` is the inverse egress: parse an existing `.drawio`/`.xml` into a `ParsedDiagram` whose `.shapes`/`.edges` are the same typed `Object`/`Edge` instances the author path builds, addressable by cell id — the template round-trip (read, mutate by `get_by_id`, re-emit through `File.write`).

| [INDEX] | [SURFACE]                            | [SHAPE] | [CAPABILITY]                                                                          |
| :-----: | :----------------------------------- | :------ | :------------------------------------------------------------------------------------ |
|  [01]   | `load_diagram(file_path)`            | parse   | `.drawio`/`.xml` -> a `ParsedDiagram` (`FileNotFoundError`/`ValueError` on bad input) |
|  [02]   | `ParsedDiagram.shapes` / `edges`     | rows    | the typed `Object` / `Edge` rows                                                      |
|  [03]   | `ParsedDiagram.get_by_id(cell_id)`   | lookup  | one element by its draw.io cell id (`None` if absent)                                 |
|  [04]   | `ParsedDiagram.element_count` (prop) | query   | `len(shapes) + len(edges)`                                                            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import drawpyo` at egress scope only; the mxGraph object model never leaks into domain code — the domain holds the `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` grammar and lowers it here.
- document: `File` -> `Page` -> element tree is the one spine; the owning page encodes `File.xml`, content-keys those bytes, and writes through the runtime artifact store, never trusting drawpyo's own disk path as the artifact of record. `Page` auto-builds the two empty `mxCell` roots and the nested `diagram`/`mxGraphModel`/`root` tags.
- element: an `Object`'s SHAPE is a draw.io library-key string (`object_from_library('flowchart', 'decision')`), never a Python subtype — a new shape is one library row, not one class. Lower the `DiagramGlyph` case through one total `match`: `Node` -> `Object`, `Edge` -> `Edge`, `Swimlane` -> a parent `Object` container, `Annotation`/`Marker` -> a styled `Object`.
- style: every edge axis (`waypoints`/`connection`/`pattern`), the object `line_pattern`, and the edge `jumpStyle` is a TOML-backed bounded vocabulary whose setter raises `ValueError` off-domain; `color_input_check` (hex/`none`/`default`) and `width_input_check` (1..999 clamp) gate at the property boundary. Compose through `apply_attribute_dict`/`apply_style_string`/`object_from_library`, never a hand-written `style="..."`.
- round-trip: `from_style_string`/`apply_style_string` ingest a draw.io Edit-Style string verbatim, and `load_diagram` parses a whole `.drawio` back into typed `Object`/`Edge` rows — a template workflow routes `load_diagram` -> mutate by `get_by_id` -> `File.write`, never a second parser.
- builder: `TreeDiagram`/`BinaryTreeDiagram`/`ClassDiagram` fold a nested `dict` into a positioned hierarchy; `BarChart`/`PieChart`/`Legend` fold a mapping into one `Group`. `TreeDiagram` is the hierarchical-layout fallback — the `visualization/diagram/layout#LAYOUT` owner routes real graph layout through `rustworkx`/`pyelk`/`fast-sugiyama` and lowers the coordinates onto bare `Object`/`Edge` here.
- color: `StandardColor` is a `str`-Enum (a hex-string subtype usable anywhere a color string is), `ColorScheme(fill, stroke, font)` bundles the object color slots, and the `GlyphStyle` palette indices project to hex through the shared `graphic/color/derive#DERIVE` `hex_ramp` before binding `Object.fillColor`/`strokeColor` — color traces to a palette index, never a per-mark `StandardColor` literal chosen at the egress.
- page: `PageSize` is a `(width, height)` tuple-Enum (ISO A/B-series × orientation, US sizes, aspect presets); pass `Page(size_preset=PageSize.A3LANDSCAPE)` so the extent comes from the vocabulary, never a hand-typed width/height pair.
- evidence: each egress captures page/object/edge counts (`File.stats()`), the rendered `.drawio` byte length, and the layout/coloring policy as one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` row — the same case the `visualization/diagram/draw#DRAW` SVG arm and `visualization/diagram/layout#LAYOUT` owner contribute.

[STACKING]:
- `expression`(`.api/expression.md`): `File.write` runs inside the owning page's `RuntimeRail`/`async_boundary`, so a write fault — bad `file_path`, an `overwrite=False` collision, an off-domain style `ValueError` — lands as a typed `Result` failure, never a raw exception crossing the domain.
- `msgspec`(`.api/msgspec.md`): `File.stats()` counts, `len(File.xml.encode("utf-8"))` over the exact written bytes (never a `str` character count), and the layout/coloring policy populate a `msgspec.Struct` `ArtifactReceipt.Diagram(key, "diagram-drawio", nodes, edges, algorithm, bytes)` on the shared `core/receipt#RECEIPT` family, sharing the case the `drawsvg` arm emits.
- `rasm.runtime.identity#ContentIdentity`: its owning page mints the node key pre-run over a length-framed canonical glyph⊕palette seed — never over rendered `File.xml` bytes — so keyed elision probes the warm seed before serialization and `receipt.slot == node.key`; drawpyo never mints identity.
- `anyio`(`.api/anyio.md`): drawpyo is pure-Python and synchronous, so the owning page offloads `File.write`/`File.xml` serialization through `lane.offload(Kernel.of(..., KernelTrait.RELEASING))` off the event loop, the shared-address-space thread arm the `drawsvg` `_render` sibling takes.
- `glyphset` seam: the design lowers the `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph`/`GlyphStyle`/`MarkerKind` grammar — the same grammar `visualization/diagram/draw#DRAW` consumes — onto the `Object`/`Edge`/`Group` vocabulary, so one glyph grammar drives both the `.drawio` and the SVG egress with no per-arm special-casing.
- round-trip seam: `load_diagram` parses an existing `.drawio` template back into the typed rows the same grammar describes, so a title-block AEC sheet round-trips (read, mutate by `get_by_id`, re-emit) through one owner without a second parser.

[LOCAL_ADMISSION]:
- Admit `drawpyo` as the sole editable-`.drawio` author/serialize/ingest owner on the diagram rail; a second `.drawio` emitter or XML parser is rejected.

[RAIL_LAW]:
- Package: `drawpyo`
- Owns: editable-`.drawio` (mxGraph XML) author/serialize/ingest via the `File`/`Page`/`XMLBase` spine, the `Object`/`Edge`/`Group`/`Point`/`TextFormat` vocabulary with TOML-validated style axes, the `object_from_library` shape factory, the Edit-Style round-trip, container parenting, the `TreeDiagram`/`BarChart`/`PieChart`/`Legend` builders, the `load_diagram` inverse parser, and the `StandardColor`/`ColorScheme`/`PageSize` value objects.
- Accept: editable-`.drawio` egress and ingest of the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` grammar (node-link, ER, flowchart, Sankey, AEC schedule/legend), plus template round-trip and the hierarchical-layout fallback, feeding the one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case and the runtime content-key index.
- Reject: a hand-emitted mxGraph `style="..."` or raw `<mxCell>` XML; a free-string `waypoints`/`connection`/`pattern`/`jumpStyle`; a per-shape `Object` subtype family; a raster op `resvg-py`/`vl-convert`/`pyvips` owns; a second SVG emitter where the `visualization/diagram/draw#DRAW` `drawsvg` arm renders the same grammar; a re-implemented graph layout `rustworkx`/`pyelk`/`fast-sugiyama`/`libavoid` route; a second `.drawio` parser where `load_diagram` ingests; identity minting the runtime owns.
