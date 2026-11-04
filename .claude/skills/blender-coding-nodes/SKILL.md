---
name: blender-coding-nodes
description: "Use when writing or reviewing Python over a Blender node tree, covering tree structure and reconciliation"
---

# [BLENDER_CODING_NODES]

Covers `bpy` code over node trees:
- Use `blender-coding` for data references, host faults, and evaluation
- Use `blender-coding-render` for compositor effect stacks and strips

[REFERENCES]:
- [01]-[EXPRESSIONS](references/expressions.md): Node graphs as expression records built into trees by content hash
- [02]-[EVALUATION](references/evaluation.md): Tree evaluation, built-in node sampling, modifier inputs, and bakes

## [01]-[TREES]

Holders decide how code reaches a tree:

| [INDEX] | [HOLDER]                                 | [TREE]                                                                                        |
| :-----: | :--------------------------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `Material`, `World`, `Light`             | Embedded `node_tree` with built-in nodes, `use_nodes` ignoring writes                         |
|  [02]   | `Texture`, `FreestyleLineStyle`          | `node_tree` at `None` until `use_nodes = True` builds default nodes                           |
|  [03]   | `bpy.data.node_groups`                   | `new(name, <tree class>.bl_rna.identifier)` with empty nodes and interface                    |
|  [04]   | Group node, `NODES` modifier             | `node_tree`, `node_group`                                                                     |
|  [05]   | Compositor effect, strip, strip modifier | `node_group` of `SceneCompositorEffect`, `CompositorStrip`, `SequencerCompositorModifierData` |
|  [06]   | Node editor                              | `space.node_tree` root, `space.edit_tree` entered group                                       |

Tree kinds decide node classes and socket types:
- Tree, node, and socket idnames read `<class>.bl_rna.identifier`: built-in classes hold `bl_idname` `None`, Python classes a `__name__` other than the idname
- `nodes.new(bl_idname)` raises `RuntimeError` for a class the tree refuses or the build lacks
- `interface.new_socket` raises `TypeError` listing socket types the tree kind takes
- Geometry, shader, and compositor trees take Bundles, Menu Switch, and Index Switch, geometry trees all zones, shader trees Repeat and Closure zones
- Texture trees take `TextureNode*` classes and Float, Vector, and Color sockets alone
- `nodes.new` maps aliased classes to current ones (`ShaderNodeBsdfGlossy` to `ShaderNodeBsdfAnisotropic`), and matches read the created `bl_idname`
- Find built-in nodes by `bl_idname` and state: `tree.get_output_node("ALL")` in shader trees, `NodeGroupOutput` with `is_active_output` in groups
- Modifier-type geometry editors swap a written `space.node_tree` for the active modifier's tree at next redraw while `space.pin` is off

Groups take picker and tool flags, and host helpers inline a tree or add built-in nodes:
- `is_modifier`, `is_strip_modifier`, and `allow_usage_in_scene_compositor_effect` list a group in pickers and asset menus
- `node_group` writes take any tree of the kind whatever its flags
- Group node `node_tree` writes of another tree kind or a tree containing the node's tree (`tree.contains_tree(other)`) leave it empty, no raise
- Node tools set `is_tool`, one `is_type_*` and one `is_mode_*` flag, and `node_tool_idname`
- Background runs register node tool operators through `window_manager.register_node_group_operators()`
- Built-in groups in the `assets/nodes` blends hold `Input_<n>` and `Output_<n>` identifiers beside `Socket_<n>`
- `inline_shader_nodes()` on materials, worlds, and lights returns an object with a `node_tree` that inlines groups, zones, closures, and bundles
- Inlined trees drop reroutes and mutes, and Blender removes inlined trees once code releases the returned object
- `bpy_extras.node_shader_utils.PrincipledBSDFWrapper` reads and writes Principled and output nodes
- `PrincipledBSDFWrapper` `*_texture` `image`, `projection`, and `texcoords` writes add Image Texture, Mapping, and Normal Map nodes

## [02]-[SOCKETS]

Mode writes (`data_type`, `operation`) enable sockets or retype them by node:
- Mix and Math hold sockets for every mode (`A_Float`, `A_Vector`, Math `Value_002`), enabled sockets in input order being the mode's operands
- Socket positions on Mix and Math count disabled variants
- Switch, Compare, Sample Index, Index Switch, Store Named Attribute, Random Value, and Boolean Math hold one socket per role
- Mode writes on one-socket nodes retype each socket under its identifier with its links and drop sockets the mode lacks (Boolean Math `NOT`)
- Write mode members before reading, writing, or linking sockets they enable
- Sockets held across a retyping mode write crash Blender at their next read
- `is_unavailable` reads true on disabled sockets, `is_inactive` on those and sockets the mode leaves unused (Switch `True` while `Switch` is false)
- String keys (`inputs[key]`, `get`, `find`) match an enabled socket's identifier or name and miss disabled ones (Math `Value_001` under `SINE`)
- Iteration matching `identifier` reaches every socket, disabled ones included

Compositor nodes take every option through input sockets, and geometry nodes take some (Merge by Distance `Mode`, Raycast `Interpolation`):
- Mode choices are `MENU` sockets (Glare `Type`, Blur `Type`, Scale `Frame Type`), `default_value` taking the exact item name
- Built-in menu sockets read empty `bl_rna.properties["default_value"].enum_items`, and the `TypeError` of a wrong name lists valid names
- Compositor node state compares through socket defaults alone

Socket values take per-class shapes:
- Vector defaults hold 2, 3, or 4 components by class (`NodeSocketVector2D`), colors 4, rotations an `Euler`
- `socket.bl_rna.properties["default_value"].array_length` reads the live length
- Matrix, Geometry, Shader, Bundle, and Closure sockets hold no `default_value`
- Group Input, Group Output, zone, and item nodes end socket lists with virtual sockets (`NodeSocketVirtual`, identifiers starting `__extend__`)
- Keyframes and drivers on socket defaults sit in the tree's `animation_data` at `socket.path_from_id("default_value")`

## [03]-[FIELDS]

Geometry tree sockets take single values or fields, and a field computes per element where a geometry node consumes it:
- `display_shape` states what a socket takes: `LINE` a single value, `DIAMOND` a field, `CIRCLE` whatever reaches it
- `inferred_structure_type` reads what reaches a socket (`SINGLE`, `FIELD`, `DYNAMIC`) in geometry and compositor trees and `DYNAMIC` in shader trees
- Unlinked implicit inputs (Set Position `Position`) read `FIELD`
- Fields entering `CIRCLE` sockets (Math, Vector Math) make the node's outputs `FIELD`

## [04]-[LINKS]

`tree.links.new(output, input, verify_limits=True, handle_dynamic_sockets=False)` links sockets of one tree:
- Endpoints go in either order, a pair of inputs or of outputs raising `RuntimeError`
- Links into a linked single input replace the old link
- `verify_limits=True` removes links past `link_limit` on either endpoint, and `links.new` clears `hide` on both endpoints
- Links onto a virtual socket pass `handle_dynamic_sockets=True` to add an item named and typed after the source, returning `None` for a refused type
- Plain links onto a virtual socket read `is_valid` false and add nothing
- `link.is_muted` mutes a link in place, the target reading its own default
- `nodes.remove(node)` removes the node's links, a held node raising `ReferenceError` and a held link reading `None` endpoints with no raise

Multi-input sockets order their links by `multi_input_sort_id`, each new link added on top:
- `socket.links` lists top first by descending sort id, up to 4095 links
- `links.new` with `verify_limits=True` replaces matching pairs with a new link; `False` adds another link from that output
- `link.swap_multi_input_sort_id(other)` swaps link positions on one socket, keeping both links and their mute state

`NodeSocket.links` scans `tree.links` on each read and lists muted links. Graph walks index one `tree.links` pass by target node:

```python
def upstream(tree: bpy.types.NodeTree, root: bpy.types.Node, /, *, kept: Callable[[bpy.types.NodeLink], bool] = lambda link: link.is_valid and not link.is_muted) -> tuple[bpy.types.Node, ...]:
    feeding: defaultdict[bpy.types.Node, dict[bpy.types.Node, None]] = defaultdict(dict)
    for link in filter(kept, tree.links):
        feeding[link.to_node][link.from_node] = None
    reached: dict[bpy.types.Node, dict[bpy.types.Node, None]] = {}
    pending = [root]
    while pending:
        node = pending.pop()
        if node not in reached:
            reached[node] = feeding[node]
            pending.extend(reversed(feeding[node]))
    return tuple(TopologicalSorter(reached).static_order())
```

`upstream` returns sources first and `root` last over links `kept` passes:
- `kept=lambda link: link.is_valid` follows muted links
- Iteration reaches the root's dependencies once before sorting them, leaving disconnected components outside the operation

Geometry and compositor trees convert link values by type pair:
- Floats, integers, and booleans convert among each other and into vectors and colors, a scalar filling every channel and color alpha reading 1
- Floats and integers read true above zero, vectors true with any nonzero component, colors true through luminance
- Floats convert to integers truncating toward zero, vectors to floats as component mean, and colors as luminance
- Vectors and colors convert into each other, a vector taking alpha 1 and a color dropping alpha
- Rotations take floats and vectors as XYZ Euler radians and matrices through their rotation part, and convert to Euler vectors and matrices
- Matrices convert to rotations alone
- Strings, menus, bundles, closures, geometry, and data-blocks link to their own type alone

Other tree kinds validate links by their own rules:
- Shader trees link any value into a `Shader` input as emission and a `Shader` output into `Shader` inputs alone
- Shader trees link Bundle, Closure, Menu, and String sockets to their own type alone
- Texture trees and Python-defined trees impose no socket-type conversion restriction

Links read `is_valid` false for:
- Disabled endpoint
- Menu socket in a definition conflict
- Type pair with no conversion
- Zone-internal value reaching a node downstream of the zone output
- Field reaching a `LINE` input

`links.new` sets `is_valid` at creation, and topology changes recompute it, overwriting Python writes:
- Passes over `tree.links` find false links, `node_warnings` holding no entry for them and the node editor alone drawing why
- Evaluation ignores `is_valid`, unconvertible pairs leaving targets at the stored `default_value` and fields handing `LINE` inputs the type's zero

## [05]-[ZONES_AND_ITEMS]

Zone input and output nodes start unpaired:
- `input.pair_with_output(output)` adds item sockets to the input, raising `RuntimeError` for another zone kind or an output already paired
- `input.paired_output` names the partner, and an output's input comes from a scan of `tree.nodes`
- Unpaired inputs are ignored, their consumers reading socket defaults
- Pair outputs in the step that creates them, an unpaired output beside a paired zone hanging Blender at next evaluation
- Links leaving a zone and output nodes inside one evaluate the modifier to empty geometry with no `node_warnings` entry

Zone outputs hold item collections:

| [INDEX] | [ZONE]     | [ITEMS]                                         |
| :-----: | :--------- | :---------------------------------------------- |
|  [01]   | Repeat     | `repeat_items`                                  |
|  [02]   | Simulation | `state_items`                                   |
|  [03]   | For Each   | `input_items`, `main_items`, `generation_items` |
|  [04]   | Closure    | `input_items`, `output_items`                   |

Zone bodies run per kind:
- Simulation state items read zone input on the first frame and refuse `CLOSURE` with `RuntimeError`
- Simulation outputs replay cached frames when stepped back and compute one step on a jump past the cache
- For Each bodies run once per element of the output's `domain`, input items taking a field outside and one value inside the zone
- For Each main items return a field on that domain read on the zone output's `Geometry`, and generation items join each pass's geometry

Closures and bundles match items by exact, case-sensitive name:
- Evaluate Closure items match the closure's input and output items by name, and with no closure linked same-named inputs pass through
- Missing closure outputs report `ERROR` in `node_warnings` and read the default, and type differences convert with an `INFO` entry
- Separate Bundle reads Combine Bundle items by name and ignores extra keys, missing names and unconvertible types reading the default with `ERROR`
- Bundle item names store `/*&|"^~!,{}()+$#@[];:?<>.-%\=` and leading and trailing spaces as `_`, duplicates taking `_001`
- `define_signature` items sync through `bpy.ops.node.sockets_sync(node_name=...)` in a node editor context alone, `links.new` never syncing them

Item collections (`capture_items`, `bake_items`, `bundle_items`, `index_switch_items`, `enum_items`, `file_output_items`) define variable sockets:
- Collections take `new(socket_type, name)` with socket enums (`FLOAT`, `VECTOR`, `RGBA`, `GEOMETRY`), an idname raising `TypeError` that lists them
- Capture items created with `VECTOR` read back `data_type` `FLOAT_VECTOR`
- Collections take `remove(item)`, `clear()`, and `move(from_index, to_index)`, an index past the range moving nothing
- Array-backed items reallocate on addition and removal; moves shift values between slots, so hold identifiers and reacquire items after edits
- `enum_items` and `rasterize_items` take `new(name)`, `index_switch_items` and `combine_list_items` take `new()`
- Repeat and For Each items hold `socket_type`, Simulation items add `attribute_domain`, Bake items `attribute_domain` and `is_attribute`
- For Each generation items hold `domain` (Capture Attribute on the node), closure, Evaluate Closure, and bundle items `structure_type`
- Item sockets follow collection order after the node's fixed sockets and take each item rename
- Item socket identifiers come from a counter per node (`Item_<n>`, For Each `Input_<n>`, `Main_<n>`, `Generation_<n>`) that `clear()` keeps
- Capture items take identifiers `Value`, `Value_1` on inputs and `Attribute`, `Attribute_1` on outputs
- Repeat Output starts with one `Geometry` item in geometry trees and none in shader trees, `Item_0` on both zone nodes once paired
- Menu Switch starts with items `A` and `B` and `GEOMETRY` data in geometry trees, `RGBA` in others
- Menu Switch item sockets take identifiers `Item_<n>` in item order on both sides, beside the `Menu` input and `Output` value

## [06]-[INTERFACES]

`NodeTree.interface` declares a group's parameters as sockets and panels:
- `new_socket(name, description=, in_out=, socket_type=, parent=)` takes a base socket idname (`NodeSocketFloat`)
- `subtype` and vector `dimensions` follow as separate writes refining the item class `items_tree` returns (`NodeTreeInterfaceSocketFloatDistance`)
- Refined items keep the base idname in `socket_type`
- `subtype` `enum_items` read `DEFAULT` alone on every item class, and an unknown value's `TypeError` lists subtypes the socket type takes
- `new_panel(name, description=, default_closed=)` appends at root, and `move_to_parent(item, parent, to_position)` nests it
- Sockets and panels take identifiers from one counter per tree (`Socket_<n>`, panel `<n>`) that `remove` and `clear()` never rewind
- `items_tree[key]` resolves a socket identifier first, then a socket or panel name
- `items_tree` walks pre-order with panels among sockets, and filters read `item_type` and `in_out`
- `copy(item)` inserts at `position + 1` in the same parent under fresh identifiers, panels copying their children
- Each parent orders outputs, then inputs, then panels, and `move` and `move_to_parent` clamp a request to the nearest legal slot with no error
- Top-level items name `interface.root_panel` as `parent`, a nameless panel outside `items_tree`
- `remove(panel)` moves panel contents up one level, and `move_content_to_parent=False` deletes contents with the panel
- `new_socket` with no `socket_type` creates a `NodeSocketFloat` item
- `socket_type` writes reset `default_value`, `min_value`, `max_value`, and `subtype`, an equal value included
- `socket_type` writes reset `default_input` to `VALUE` where the new type lacks it, and `structure_type` to `AUTO` on types without fields or grids
- Float socket and interface `default_value` writes clamp to `float32` limits, and `min_value` and `max_value` writes past them store infinity
- `default_value`, `min_value`, and `max_value` seed group nodes and modifier inputs created after the write, existing ones keeping stored values

Panel toggles bind a Bool socket to its panel header:
- `is_panel_toggle = True` takes effect on a Bool socket inside a non-root panel alone, and other writes do nothing
- Toggles move to position 0 and take their panel's name, following it on every tree update
- `remove(panel)` lifts the toggle into the parent with `is_panel_toggle` kept, and removal takes the toggle first

Input items shape the values a group takes:
- Group Input sockets of `structure_type` `AUTO` items read `FIELD` while feeding `DIAMOND` sockets alone and `SINGLE` once feeding a `LINE` socket
- `structure_type = "SINGLE"` holds an input to one value wherever it links
- Non-geometry trees refuse `structure_type` values past `AUTO` and `SINGLE` with `TypeError`
- Write `structure_type` alone, `force_non_field` writing it as `SINGLE` or `AUTO` and reading stale after a later `structure_type` write
- `default_input` feeds an implicit input into an unlinked group node socket, the socket reading `hide_value` true
- `default_input` takes `INDEX` and `ID_OR_INDEX` on Int, `POSITION`, `NORMAL`, and handles on Vector, and `INSTANCE_TRANSFORM` on Matrix
- `SCENE_FRAME` takes Float or Int, `SELF_OBJECT` Object, and `UNIFORM_IMAGE_COORDINATES` compositor Vector sockets
- `default_input` writes outside a socket's set raise a `TypeError` listing the set
- Modifier inputs ignore `default_input` and evaluate `default_value`
- `hide_in_modifier` hides the modifier panel row alone, `properties.inputs.<identifier>` evaluating its `value`
- Output items take `attribute_domain`
- `default_attribute_name` seeds the modifier's `attribute_name` on an output and sets `type = "ATTRIBUTE"` with that name on an input

Menu sockets store an item's integer identifier, keeping each selection across renames:
- Menu Switch item edits regenerate `properties.inputs` of every modifier on the group
- Values matching no item evaluate to the output type's default with no error
- Group menu inputs list items while a link chain reaches a Menu Switch through group nodes, reroutes, and generic nodes
- Unlinked menu sockets refuse every `default_value`, keeping the stored integer until a definition holding it arrives
- Menu outputs feeding more than one Menu Switch invalidate each such link and list no items with no `node_warnings` entry, equal item names included
- Built-in nodes with the same menu share one definition, and one output feeds them all

Interface writes add and remove sockets on every group node and modifier before the call returns, and users keep values by identifier:
- Moves, removals, and retypes rewrite group node keyframe and driver paths to new indices and drop F-curves of removed sockets
- Retypes convert modifier values, floats truncating to integers, numbers testing nonzero as booleans, menus keeping their integer, data-blocks kept
- Retypes between scalars and arrays reset modifier values to the interface default, and arrays keep leading values
- Group nodes rebuild a retyped socket from the interface default and move its links over
- `remove` followed by `new_socket` drops every stored value of the removed item
- `tree.copy()` keeps every identifier
- `node_group` writes on modifiers and compositor effects keep input values across groups with coinciding identifiers

Every interface write, an equal value and a `move` to the held position included, retags the tree and each user for evaluation. Sockets key by name and side, first holder staying, and each record writes differing members onto its held identifier in field order:

```python
class Socket(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    name: str
    in_out: Literal["INPUT", "OUTPUT"]
    socket_type: type[bpy.types.NodeSocket]
    description: str | None = None
    default_value: bool | int | float | str | tuple[float, ...] | None = None
    min_value: float | None = None
    max_value: float | None = None


def interfaced(tree: bpy.types.NodeTree, sockets: Iterable[Socket], /) -> None:
    interface: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    live = tuple(item for item in interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket))
    held = dict(reversed([((item.name, item.in_out), item.identifier) for item in live]))
    wanted = {(socket.name, socket.in_out): socket for socket in sockets}
    for item in live:
        if (key := (item.name, item.in_out)) not in wanted or held[key] != item.identifier:
            interface.remove(item)
    for key, socket in wanted.items():
        identifier = held.get(key) or interface.new_socket(socket.name, in_out=socket.in_out).identifier
        for member, value in msgspec.to_builtins(socket, enc_hook=lambda kind: kind.bl_rna.identifier).items():
            if msgspec.to_builtins(getattr(current := interface.items_tree[identifier], member), enc_hook=list) != (np.asarray(value, np.float32).tolist() if isinstance(value, float | tuple) else value):
                setattr(current, member, value)
```

Repeat `interfaced` calls with the same sockets write and tag nothing:
- Field order writes `socket_type` before the defaults it resets, retyping a fresh item under its identifier
- `None` fields leave a member at the value the socket type gives it, `omit_defaults` keeping them out of the write
- Float members read back at `float32` precision, and compares round the record's floats through `np.float32`
- `bpy_prop_array` values equal other `bpy_prop_array` values alone, and compares convert them through `enc_hook=list`
- When live order per side differs from records, `move(item, len(items_tree))` per socket in record order moves each last on its side
- Refused socket types and values raise `TypeError` uncaught, a defect in the record

## [07]-[PYTHON_TYPES]

Python-defined trees, nodes, and sockets subclass `bpy.types.NodeTree`, `Node`, and `NodeSocket`, and Blender evaluates none of them:
- `NodeTree.update()` runs more than once per node addition and link change, and tree evaluation runs once from a deferred step it queues
- Socket and node property writes call neither tree nor node `update`, and evaluation they change runs from the property's own `update=` callback
- Nodes with no `poll` join every tree kind, built-in trees included, and `poll(cls, ntree)` refusing a tree makes `nodes.new` raise `RuntimeError`
- `Node.init(self, context)` builds sockets through `inputs.new(<socket idname>, <name>)` and `outputs.new`, `nodes.new` passing `None` as `context`
- `NodeSocket` subclasses define `draw(self, context, layout, node, text)`, `register_class` raising `AttributeError` without it
- `links.new` calls `update` on both endpoint nodes and skips `insert_link`
- `NodeTree.valid_socket_type(cls, idname)` filters interface socket types per interface write

## [08]-[RECONCILIATION]

Trees reach a desired state through edits in place and keep nodes a user added:
- Desired nodes, links, and interface items are frozen records keyed by structure, compared against live state before each write
- Owned nodes hold an ID property mark under a key derived from structure, renames and `.001` suffixes breaking `name` keys
- Copies of owned nodes (editor duplicates, clipboard pastes, `tree.copy()`) hold the mark, and marks dedupe by tree order with the first holder kept
- Fresh materials, worlds, and lights hold unmarked built-in nodes, removed before their first reconciliation writes
- Interface items change in place, and reconciliation writes each group node and modifier value an interface default leaves stored

Writes follow socket creation, each step adding sockets the next one reads:
1. Links no desired link keeps
2. Interface items, then nodes no key holds and new nodes
3. Group `node_tree`, zone pairs, item collections, and node content, then mode members and `parent`
4. Links, then multi-input order
5. Socket defaults, then interface defaults, a menu default after its Menu Switch link

## [09]-[COPIES]

Node clipboard operators copy whole nodes between trees through a node editor, any screen area serving once its `ui_type` names the tree class:

```python
def pasted(context: bpy.types.Context, area: bpy.types.Area, nodes: tuple[bpy.types.Node, *tuple[bpy.types.Node, ...]], target: bpy.types.NodeTree, /) -> Outcome[frozenset[bpy.types.Node]]:
    source: bpy.types.NodeTree = nodes[0].id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    before = frozenset(target.nodes)
    with ExitStack() as scope:
        scope.callback(setattr, area, "ui_type", area.ui_type)
        area.ui_type = source.bl_idname
        space: bpy.types.SpaceNodeEditor = area.spaces.active  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        for node in source.nodes:
            node.select = node in nodes
        space.node_tree = source
        with context.temp_override(area=area):
            copied = catch(bpy.ops.node.clipboard_copy, exception=RuntimeError)()
        area.ui_type, space.node_tree = target.bl_idname, target
        with context.temp_override(area=area):
            answer = copied.bind(lambda _: catch(bpy.ops.node.clipboard_paste, exception=RuntimeError)())
    created = frozenset(target.nodes) - before
    for node in created if answer.is_error() else ():
        target.nodes.remove(node)
    return answer.map(lambda _: created).map_error(partial(Fault.of, Reason.FAILED, target.name_full))
```

`pasted(context, context.screen.areas[0], (ramp, noise), material.node_tree)` copies geometry tree nodes into an embedded shader tree:
- Pastes keep `location`, ID property marks, Color Ramp elements, zone pairs, item collections, and inner links, clashing names taking `.001`
- `space.node_tree` writes set `edit_tree` outside `temp_override` alone, inside it leaving `edit_tree` `None` and clipboard operators failing `poll`
- `nodes` holds at least one node, a copy with no selection keeping the previous clipboard for the paste to repeat
- Tree kinds refusing a node paste the accepted ones and raise `RuntimeError` naming the refused one, and `pasted` removes them all

## [10]-[PLACEMENT]

New nodes sit at the canvas origin:
- `location` is relative to a parent frame, `location_absolute` canvas position
- `parent` writes keep the canvas position and rewrite `location`
- Layouts write positions in one `nodes.foreach_set("location_absolute", positions.ravel())` over a numpy `float32` buffer shaped `(len(nodes), 2)`
- `dimensions` reads zero until a node editor draws the tree, in interface-scaled units, and layouts before a draw space nodes by `width`
- Group nodes take `width = group.default_group_node_width`
- `node.panel_states` items (`identifier`, `is_collapsed`) open and close node panels
- Frames hold `text`, `shrink`, and `label_size`
- Reroutes take their type through `socket_idname`
- Node editors draw previews while `node.show_preview` and `space.overlay.show_previews` are on, `overlay.preview_shape` taking `FLAT` or `3D`
- `bpy.ops.node.preview_toggle` toggles `show_preview` on selected nodes
