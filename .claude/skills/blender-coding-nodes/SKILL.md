---
name: blender-coding-nodes
description: "Use when writing or reviewing Python over a Blender node tree, covering tree structure and reconciliation"
---

# [BLENDER_CODING_NODES]

Covers `bpy` code over node trees.

- Use `blender-coding` for data references, host faults, and evaluation
- Use `blender-coding-render` for compositor effect stacks and strips

[REFERENCES]:
- [01]-[EXPRESSIONS](references/expressions.md): Node graphs as expression values, from operator algebra to lowering by content key
- [02]-[EVALUATION](references/evaluation.md): Tree evaluation, stock nodes computing values for code, modifier inputs and outputs, and bakes

## [01]-[TREES]

Holders decide how code reaches a tree:

| [INDEX] | [HOLDER]                                 | [TREE]                                                                                        |
| :-----: | :--------------------------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `Material`, `World`, `Light`             | Embedded `node_tree` with stock nodes, `use_nodes` ignoring writes                            |
|  [02]   | `Texture`, `FreestyleLineStyle`          | `node_tree` at `None` until `use_nodes = True` builds stock nodes                             |
|  [03]   | `bpy.data.node_groups`                   | `new(name, "<Kind>NodeTree")` with empty nodes and interface                                  |
|  [04]   | Group node, `NODES` modifier             | `node_tree`, `node_group`                                                                     |
|  [05]   | Compositor effect, strip, strip modifier | `node_group` of `SceneCompositorEffect`, `CompositorStrip`, `SequencerCompositorModifierData` |
|  [06]   | Node editor                              | `space.node_tree` root, `space.edit_tree` entered group                                       |

Tree kinds decide node classes and socket types:
- `nodes.new(bl_idname)` raises `RuntimeError` for a class the tree refuses or the build lacks
- `interface.new_socket` raises `TypeError` listing socket types the tree kind takes
- Geometry trees take every zone, shader trees repeat zones and closures, and both with compositor trees take Bundles, Menu Switch, and Index Switch
- Texture trees take `TextureNode*` classes and Float, Vector, and Color sockets alone
- `nodes.new` maps aliased classes to current ones (`ShaderNodeBsdfGlossy` to `ShaderNodeBsdfAnisotropic`), and matches read the created `bl_idname`
- Stock nodes resolve by `bl_idname` and state, `tree.get_output_node("ALL")` in shader trees and `NodeGroupOutput` with `is_active_output` in groups
- Modifier-type geometry editors swap a written `space.node_tree` for the active modifier's tree at next redraw while `space.pin` is off

Groups take flags for pickers and tools, and host members build and inline stock content:
- `is_modifier`, `is_strip_modifier`, and `allow_usage_in_scene_compositor_effect` list a group in pickers and asset menus
- `node_group` writes take any tree of the kind whatever its flags
- Group node `node_tree` writes of another tree kind or a tree containing the host (`tree.contains_tree(other)`) leave `node_tree` empty with no raise
- Node tools set `is_tool`, one `is_type_*` and one `is_mode_*` flag, and `node_tool_idname`
- Background runs register node tool operators through `window_manager.register_node_group_operators()`
- Stock groups in the `assets/nodes` blends hold `Input_<n>` and `Output_<n>` identifiers beside `Socket_<n>`
- `material.inline_shader_nodes()`, alike on worlds and lights, returns an object with a `node_tree` inlining groups, zones, closures, and bundles
- Inlined trees drop reroutes and mutes, and Blender removes inlined trees once code releases the returned object
- `bpy_extras.node_shader_utils.PrincipledBSDFWrapper` reads and writes a material's Principled and output pair with its `*_texture` and `normalmap_texture` slots
- Texture slot writes to `image`, `projection`, and `texcoords` on `ShaderImageTextureWrapper` build Image Texture, Mapping, and Normal Map nodes

## [02]-[SOCKETS]

Mode writes (`data_type`, `operation`) enable sockets or retype them by node:
- Mix and Math hold sockets for every mode (`A_Float`, `A_Vector`, `A_Rotation`, Math `Value_002`), and enabled sockets in input order are the mode's operands
- Socket positions on Mix and Math count disabled variants
- Switch, Compare, Sample Index, Index Switch, Store Named Attribute, Random Value, and Boolean Math hold one socket per role
- Mode writes on one-socket nodes retype each socket under its identifier with its links and drop sockets the mode lacks (Boolean Math `NOT`)
- Code writes mode members before reads, writes, and links on sockets they enable
- Sockets held across a retyping mode write crash Blender at their next read
- `is_unavailable` reads true on disabled sockets, `is_inactive` on those and sockets the mode leaves unused (Switch `True` while `Switch` is false)
- String keys (`inputs[key]`, `get`, `find`) match an enabled socket's identifier or display name and miss every disabled socket (Math `Value_001` under `SINE`)
- Iteration matching `identifier` reaches every socket, disabled ones included

Compositor nodes take every option through input sockets, and geometry nodes take some (Merge by Distance `Mode`, Raycast `Interpolation`):
- Mode choices are `MENU` sockets (Glare `Type`, Blur `Type`, Scale `Frame Type`), `default_value` taking the item name case for case
- Stock menu sockets read empty `bl_rna.properties["default_value"].enum_items`, and the `TypeError` of a wrong name lists valid names
- Compositor node comparisons read socket defaults alone

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
- Endpoints arrive in either order, both pointing one way raising `RuntimeError`, sockets of different trees returning `None`
- Links into a linked single input replace the old link
- `verify_limits=True` removes links past `link_limit` on either endpoint and clears `hide` on both, and `False` stacks links reading `is_valid` true
- Links onto a virtual socket pass `handle_dynamic_sockets=True` to add an item named and typed after the source, `None` answering a refused type
- Plain links onto a virtual socket read `is_valid` false and add nothing
- Multi-input sockets take up to 4095 links and add each new link on top, `socket.links` listing top first by descending `multi_input_sort_id`
- `link.swap_multi_input_sort_id(other)` exchanges places with `other`, a link into the same socket
- `link.is_muted` mutes a link in place, the target reading its own default
- `nodes.remove(node)` removes the node's links, a held node raising `ReferenceError` and a held link reading `None` endpoints with no raise

`NodeSocket.links` scans `tree.links` on each read and lists muted links, and walks over a tree index one `tree.links` pass by target node:

```python
from functools import reduce
from graphlib import TopologicalSorter

import bpy


def upstream(tree: bpy.types.NodeTree, root: bpy.types.Node) -> tuple[bpy.types.Node, ...]:
    feeding: dict[bpy.types.Node, set[bpy.types.Node]] = {root: set()}
    for link in tree.links:
        if link.is_valid and not link.is_muted:
            feeding.setdefault(link.to_node, set()).add(link.from_node)  # type: ignore[arg-type]  # ty: ignore[no-matching-overload]
    order: tuple[bpy.types.Node, ...] = tuple(TopologicalSorter(feeding).static_order())
    reached: set[bpy.types.Node] = reduce(lambda held, node: held | feeding.get(node, set()) if node in held else held, reversed(order), {root})
    return tuple(node for node in order if node in reached)
```

`upstream` returns sources first and `root` last, leaving out nodes no valid unmuted link connects to `root`.

Multi-input reorders read `socket.links` once, then swap sort ids slot by slot:

```python
from collections.abc import Iterator

import bpy
from expression import Error, Ok, Result


def stacked(socket: bpy.types.NodeSocket, sources: tuple[bpy.types.NodeSocket, ...]) -> Result[None, tuple[int, ...]]:
    links: tuple[bpy.types.NodeLink, ...] = socket.links  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    pending: dict[bpy.types.NodeSocket, Iterator[bpy.types.NodeLink]] = {source: iter([link for link in links if link.from_socket == source]) for source in sources}
    found: tuple[bpy.types.NodeLink | None, ...] = tuple(next(pending[source], None) for source in sources)
    match tuple(position for position, link in enumerate(found) if link is None):
        case ():
            wanted: tuple[bpy.types.NodeLink, ...] = tuple(link for link in found if link is not None)
            for slot, link in zip(sorted((held.multi_input_sort_id for held in wanted), reverse=True), wanted, strict=True):
                next(held for held in wanted if held.multi_input_sort_id == slot).swap_multi_input_sort_id(link)
            return Ok(None)
        case absent:
            return Error(absent)
```

- `sources` lists output sockets top first, a source linked twice appearing twice and taking its next link, other sources' links keeping their slots
- Absent positions report together before any swap, leaving the order as found

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
- Texture trees and Python-defined trees validate no link, every pair reading `is_valid` true

Links read `is_valid` false for:
- Disabled endpoint
- Menu socket in a definition conflict
- One link of each cycle
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
- Outputs pair in the step that creates them, an unpaired output in a tree holding a paired zone hanging Blender at the tree's next evaluation
- Links leaving a zone and output nodes inside one evaluate the modifier to empty geometry with no `node_warnings` entry

Zone outputs hold the item collections:

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
- `enum_items` and `rasterize_items` take `new(name)`, `index_switch_items` and `combine_list_items` take `new()`
- Repeat and For Each items hold `socket_type`, Simulation items add `attribute_domain`, Bake items `attribute_domain` and `is_attribute`
- For Each generation items hold `domain`, Capture Attribute holding `domain` on the node, and closure, Evaluate Closure, and bundle items hold `structure_type`
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
- `socket_type` writes reset `default_value`, `min_value`, `max_value`, and `subtype`, an equal value included, and code compares before writing
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
- Code writes `structure_type` alone, `force_non_field` writing it as `SINGLE` or `AUTO` and reading stale after a later `structure_type` write
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
- Stock nodes of one built-in menu share one definition, and one output feeds them all

Interface writes add and remove sockets on every group node and modifier before the call returns, and users keep values by identifier:
- Moves, removals, and retypes rewrite group node keyframe and driver paths to new indices and drop F-curves of removed sockets
- Retypes convert modifier values, floats truncating to integers, numbers testing nonzero as booleans, menus keeping their integer, data-blocks kept
- Retypes between scalars and arrays reset modifier values to the interface default, and arrays keep leading values
- Group nodes rebuild a retyped socket from the interface default and move its links over
- `remove` followed by `new_socket` drops every stored value of the removed item
- `tree.copy()` keeps every identifier
- `node_group` writes on modifiers and compositor effects keep input values across groups with coinciding identifiers

Sockets key by name and side, the first holder of a key staying, and records write onto held identifiers in field order:

```python
from typing import Literal

import bpy
import msgspec


class InterfaceSocket(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    name: str
    in_out: Literal["INPUT", "OUTPUT"]
    socket_type: str
    description: str = ""
    default_value: bool | int | float | str | tuple[float, ...] | None = None
    min_value: float | None = None
    max_value: float | None = None


def interfaced(tree: bpy.types.NodeTree, records: tuple[InterfaceSocket, ...]) -> None:
    interface: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    live: tuple[bpy.types.NodeTreeInterfaceSocket, ...] = tuple(item for item in interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket))
    held: dict[tuple[str, str], str] = dict(reversed([((item.name, item.in_out), item.identifier) for item in live]))
    wanted: frozenset[tuple[str, str]] = frozenset((record.name, record.in_out) for record in records)
    for item in live:
        if (key := (item.name, item.in_out)) not in wanted or held[key] != item.identifier:
            interface.remove(item)
    for record in records:
        identifier: str = held.get((record.name, record.in_out)) or interface.new_socket(record.name, in_out=record.in_out, socket_type=record.socket_type).identifier  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        for member, value in msgspec.to_builtins(record).items():
            socket: bpy.types.NodeTreeInterfaceSocket = interface.items_tree[identifier]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            if msgspec.to_builtins(getattr(socket, member), enc_hook=list) != value:
                setattr(socket, member, value)
        interface.move(interface.items_tree[identifier], len(interface.items_tree))
```

- `InterfaceSocket` fields follow governing order, `socket_type` writing before the defaults it resets
- `bpy_prop_array` values equal other `bpy_prop_array` values alone, every tuple and list reading unequal, and compares convert them through `enc_hook=list`
- `move` to `len(items_tree)` puts each socket last on its side, outputs and inputs each ending in record order
- Refused socket types and values raise `TypeError` uncaught, a defect in the record

## [07]-[PYTHON_TYPES]

Python-defined trees, nodes, and sockets subclass `bpy.types.NodeTree`, `Node`, and `NodeSocket`, and Blender evaluates none of them:
- `NodeTree.update()` runs more than once per node addition and link change, and code evaluating the tree runs once from a deferred step `update` queues
- Socket and node property writes call neither tree nor node `update`, and evaluation they change runs from the property's own `update=` callback
- Nodes with no `poll` join every tree kind, stock trees included, and `poll(cls, ntree)` refusing a tree makes `nodes.new` raise `RuntimeError`
- `Node.init(self, context)` builds sockets through `inputs.new(<socket idname>, <name>)` and `outputs.new`, `nodes.new` passing `None` as `context`
- `NodeSocket` subclasses define `draw(self, context, layout, node, text)`, `register_class` raising `AttributeError` without it
- `links.new` calls `update` on both endpoint nodes and skips `insert_link`
- `NodeTree.valid_socket_type(cls, idname)` filters interface socket types per interface write

## [08]-[RECONCILIATION]

Trees reach a desired state through edits in place and keep nodes a user added:
- Desired nodes, links, and interface items are frozen records keyed by structure, compared against live state before each write
- Owned nodes hold an ID property mark under a key derived from structure, renames and `.001` suffixes breaking `name` keys
- Copies of owned nodes (editor duplicates, `tree.copy()`) hold the mark, and marks dedupe by tree order with the first holder kept
- Fresh materials, worlds, and lights hold unmarked stock nodes, removed before their first reconciliation writes
- Interface items change in place, and reconciliation writes each group node and modifier value an interface default leaves stored

Writes follow socket creation, each step adding sockets the next one reads:
1. Links no desired link keeps
2. Interface items, then nodes no key holds and new nodes
3. Group `node_tree`, zone pairs, item collections, and node content, then mode members and `parent`
4. Links, then multi-input order
5. Socket defaults, then interface defaults, a menu default after its Menu Switch link

Copies take every writable member, sockets pairing by identifier:

```python
from typing import Protocol

import bpy
from expression import Error, Ok, Result
from expression.extra.result import catch

type Struct = bpy.types.bpy_struct[object]
type Items = bpy.types.bpy_prop_collection[Struct]


class MutableItems(Protocol):
    def new(self, *arguments: object) -> Struct: ...

    def remove(self, item: Struct) -> None: ...


def matched(source: Struct, target: Struct, skipped: frozenset[str] = frozenset({"rna_type"})) -> tuple[RuntimeError | TypeError, ...]:
    referenced: tuple[type[Struct], ...] = (bpy.types.ID, bpy.types.Node, bpy.types.NodeInternalLink)
    sockets: bpy.types.Struct = bpy.types.NodeSocket.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    definition: bpy.types.Struct = source.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def socketed(prop: bpy.types.Property) -> bool:
        return isinstance(prop, bpy.types.CollectionProperty) and prop.fixed_type == sockets

    def argument(item: Struct, parameter: bpy.types.Property) -> object:
        match parameter:
            case _ if parameter.identifier in item.bl_rna.properties:
                return getattr(item, parameter.identifier)
            case bpy.types.EnumProperty(enum_items=items):
                return items[0].identifier
            case bpy.types.BoolProperty(default=value) | bpy.types.IntProperty(default=value) | bpy.types.FloatProperty(default=value) | bpy.types.StringProperty(default=value):
                return value
            case _:
                return None

    def resized(prop: bpy.types.CollectionProperty, held: Items, live: Items) -> tuple[RuntimeError, ...]:
        if prop.srna is None or not {"new", "remove"}.issubset(prop.srna.functions.keys()):
            return ()
        mutable: MutableItems = live  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        parameters: tuple[bpy.types.Property, ...] = tuple(parameter for parameter in prop.srna.functions["new"].parameters if not parameter.is_output)
        for item in tuple(live)[len(held) :]:
            mutable.remove(item)
        return tuple(
            error
            for item in tuple(held)[len(live) :]
            for error in catch(mutable.new, exception=RuntimeError)(*(argument(item, parameter) for parameter in parameters)).swap().to_option().to_list()
        )

    def member(prop: bpy.types.Property) -> tuple[RuntimeError | TypeError, ...]:
        name, held, live = prop.identifier, getattr(source, prop.identifier), getattr(target, prop.identifier)
        match held, prop:
            case bpy.types.bpy_prop_collection(), _ if socketed(prop):
                found: dict[str, Struct] = {socket.identifier: socket for socket in live}
                return tuple(error for socket in held for error in matched(socket, found[socket.identifier]))
            case bpy.types.bpy_prop_collection(), bpy.types.CollectionProperty() as collection if not isinstance(next(iter(held), None), referenced):
                return (*resized(collection, held, live), *(error for pair in zip(held, live) for error in matched(*pair)))
            case bpy.types.bpy_struct(), _ if prop.is_readonly and not isinstance(held, referenced):
                return matched(held, live)
            case _ if not prop.is_readonly and held != live and (isinstance(held, bpy.types.ID) or not isinstance(held, bpy.types.bpy_struct | bpy.types.bpy_prop_collection)):
                return tuple(catch(setattr, exception=TypeError)(target, name, held).swap().to_option().to_list())
            case _:
                return ()

    ordered: list[bpy.types.Property] = sorted((prop for prop in definition.properties if prop.identifier not in skipped and not prop.is_registered_optional), key=lambda prop: (prop.is_readonly, socketed(prop)))
    return tuple(error for prop in ordered for error in member(prop))


def copied(source: bpy.types.Node, tree: bpy.types.NodeTree) -> Result[bpy.types.Node, tuple[RuntimeError | TypeError, ...]]:
    node: bpy.types.Node = tree.nodes.new(source.bl_idname)
    match matched(source, node, frozenset({"rna_type", "name", "location", "select"})):
        case ():
            return Ok(node)
        case refused:
            tree.nodes.remove(node)
            return Error(refused)
```

- Copies keep `label`, `mute`, `hide`, `width`, custom color, and `location_absolute`, and leave identity and frame membership to the target tree
- Read-only pointers copy their content (Color Ramp elements, curve points, `image_user`)
- Writable data-block pointers (`node_tree`, `image`) copy as references
- Writes skip equal members, dynamic enums with no items (Image node `layer` and `view` with no image) refusing even their current value
- Sockets sort last, mode members and items creating every socket before defaults copy
- Item collections resize through their own `new` and `remove`
- `new` parameters an item lacks take the first enum item or default, else `None`, the item's own members overwriting them

## [09]-[PLACEMENT]

New nodes sit at the canvas origin:
- `location` is relative to a parent frame, `location_absolute` canvas position
- `parent` writes keep the canvas position and rewrite `location`
- Layouts write positions in one `nodes.foreach_set("location_absolute", positions.ravel())` over a numpy `float32` buffer shaped `(len(nodes), 2)`
- `dimensions` reads zero until a node editor draws the tree, in interface-scaled units, and layouts before a draw space nodes by `width`
- Group nodes take `width = group.default_group_node_width`
- `node.panel_states` items (`identifier`, `is_collapsed`) open and close node panels
- Frames hold `text`, `shrink`, and `label_size`
- Reroutes take their type through `socket_idname`
- Node editors draw a node's preview while `node.show_preview` and `space.overlay.show_previews` are on, and `overlay.preview_shape` takes `FLAT` or `3D`
- `bpy.ops.node.preview_toggle` toggles `show_preview` on selected nodes
