---
name: blender-coding-nodes
description: "Use when writing or reviewing Python over a Blender node tree, covering tree structure and reconciliation"
---

# [BLENDER_CODING_NODES]

Covers `bpy` code over node trees, from reaching a tree and its sockets to fields, interfaces, zones, and modifier and compositor effect inputs.

- Use `blender-coding` for data references, host faults, and evaluation
- Use `blender-coding-render` for compositor effect stacks and strips

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
- Geometry trees take every zone, and shader trees take repeat zones and closures
- Bundles, Menu Switch, and Index Switch go to geometry, shader, and compositor trees
- Texture trees take `TextureNode*` classes and Float, Vector, and Color sockets alone
- `nodes.new` maps aliased classes to current ones (`ShaderNodeBsdfGlossy` to `ShaderNodeBsdfAnisotropic`), and matches read the created `bl_idname`
- Stock nodes resolve by `bl_idname` and state, `tree.get_output_node("ALL")` in shader trees and `NodeGroupOutput` with `is_active_output` in groups
- Modifier-type geometry editors swap a written `space.node_tree` for the active modifier's tree at next redraw while `space.pin` is off

Group flags and stock content follow host rules:
- `is_modifier`, `is_strip_modifier`, and `allow_usage_in_scene_compositor_effect` list a group in pickers and asset menus
- `node_group` writes take any tree of the kind whatever its flags
- Group node `node_tree` writes of another tree kind, or of a tree containing the host, leave `node_tree` empty with no raise
- `tree.contains_tree(other)` tests nesting before a `node_tree` write
- Node tools set `is_tool`, one `is_type_*` and one `is_mode_*` flag, and `node_tool_idname`
- Background runs register node tool operators through `window_manager.register_node_group_operators()`
- Stock groups in the `assets/nodes` blends hold `Input_<n>` and `Output_<n>` identifiers beside `Socket_<n>`
- `material.inline_shader_nodes()`, alike on worlds and lights, returns an object with a `node_tree` inlining groups, zones, closures, and bundles
- Inlined trees drop reroutes and mutes, and Blender removes inlined trees once code releases the returned object
- `bpy_extras.node_shader_utils.PrincipledBSDFWrapper` reads and writes a material's Principled and output pair with its `*_texture` and `normalmap_texture` slots
- Texture slot writes to `image`, `projection`, and `texcoords` on `ShaderImageTextureWrapper` build Image Texture, Mapping, and Normal Map nodes

## [02]-[SOCKETS]

Mode sockets take one shape per node:
- Mix and Math hold every mode's variants (`A_Float`, `A_Vector`, `A_Rotation`, `Value_001`), and enabled sockets in input order are a mode's operands after its `data_type` or `operation` write
- Socket positions on Mix and Math count disabled variants
- Switch, Compare, Sample Index, Index Switch, Store Named Attribute, Random Value, and Boolean Math hold one socket per role
- Mode writes on one-socket nodes retype each socket under its identifier and keep its links
- Mode writes drop sockets the mode lacks (Boolean Math `NOT`, Random Value `BOOLEAN`)
- Code writes mode members before reads, writes, and links on sockets they enable
- Sockets held across a retyping mode write crash Blender at their next read
- `is_unavailable` reads true on disabled sockets
- `is_inactive` reads true on disabled sockets and on sockets the current mode leaves unused (Switch `True` while `Switch` is false)
- String keys (`inputs[key]`, `get`, `find`) match an enabled socket's display name alone, `inputs["Value_001"]` raising `KeyError`
- Iteration by `identifier` reaches every socket, disabled ones included

Compositor nodes take every option through input sockets, and geometry nodes take some (Merge by Distance `Mode`, Raycast `Interpolation`):
- Mode choices are `MENU` sockets (Glare `Type`, Blur `Type`, Scale `Frame Type`), `default_value` taking the item name case for case
- Stock menu sockets read empty `bl_rna.properties["default_value"].enum_items`, and the `TypeError` of a wrong name lists valid names
- Compositor node classes hold no settings, and comparisons read their socket defaults

Socket values take per-class shapes:
- Vector defaults hold 2, 3, or 4 components by class (`NodeSocketVector2D`), colors 4, rotations an `Euler`
- `socket.bl_rna.properties["default_value"].array_length` reads the live length
- Matrix, Geometry, Shader, Bundle, and Closure sockets hold no `default_value`
- Group Input, Group Output, zone, and item nodes end socket lists with virtual sockets (`NodeSocketVirtual`, identifiers starting `__extend__`)
- `NodeSocket.links` lists every link from or to the socket, muted ones included, scanning `tree.links` on each read
- Reads over many sockets group one pass over `tree.links` by endpoint, and reads of one socket take `socket.links`
- Keyframes and drivers on socket defaults sit in the tree's `animation_data` at `socket.path_from_id("default_value")`

## [03]-[FIELDS]

Geometry tree sockets take single values or fields, and a field computes per element where a geometry node consumes it:
- `display_shape` states what a socket takes: `LINE` a single value, `DIAMOND` a field, `CIRCLE` whatever reaches it
- `inferred_structure_type` states what reaches a socket (`SINGLE`, `FIELD`, `DYNAMIC`)
- `inferred_structure_type` computes in geometry and compositor trees alone, shader sockets reading `DYNAMIC`
- Unlinked implicit inputs (Set Position `Position`) read `FIELD`
- Fields entering `CIRCLE` sockets (Math, Vector Math) make the node's outputs `FIELD`
- Fields evaluate on the consuming node's `domain`, values from another domain interpolating (Face Area stored on `POINT` averages adjacent faces)
- Capture Attribute holds `domain` on the node, its sockets taking identifiers `Value`, `Value_1` in and `Attribute`, `Attribute_1` out

## [04]-[LINKS]

`tree.links.new(output, input, verify_limits=True, handle_dynamic_sockets=False)` links sockets of one tree:
- Endpoints arrive in either order, both pointing one way raising `RuntimeError`, sockets of different trees returning `None`
- Links into a linked single input replace the old link
- `verify_limits=True` removes links past `link_limit` on either endpoint and clears `hide` on both sockets
- `verify_limits=False` stacks links that read `is_valid` true
- Links onto a virtual socket pass `handle_dynamic_sockets=True` to add an item named and typed after the source, `None` answering a refused type
- Plain links onto a virtual socket read `is_valid` false and add nothing
- Multi-input sockets take up to 4095 links and add each new link on top, `socket.links` listing top first by descending `multi_input_sort_id`
- `link.swap_multi_input_sort_id(other)` exchanges places with `other`, a link into the same socket
- `link.is_muted` mutes a link in place, the target reading its own default
- `nodes.remove(node)` removes the node's links, a held node raising `ReferenceError` and a held link reading `None` endpoints with no raise

Multi-input order resolves every position from one read of `socket.links`, then swaps sort ids slot by slot:

```python
import bpy
from expression import Error, Ok, Result
from expression.collections import Block


def stacked(socket: bpy.types.NodeSocket, sources: Block[bpy.types.NodeSocket]) -> Result[None, Block[Fault]]:
    links = tuple(socket.links)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    pending = {source: iter([link for link in links if link.from_socket == source]) for source in sources}
    if faults := (found := sources.mapi(lambda position, source: required(next(pending[source], None), socket, f"links[{position}]"))).choose(
        lambda result: result.swap().to_option()
    ):
        return Error(faults)
    wanted = found.choose(Result.to_option)
    for slot, link in zip(sorted((link.multi_input_sort_id for link in wanted), reverse=True), wanted):
        next(held for held in wanted if held.multi_input_sort_id == slot).swap_multi_input_sort_id(link)
    return Ok(None)
```

- `sources` lists output sockets top first, a source linked twice appearing twice and taking its next link
- Links of other sources keep their slots
- Absent positions report together before any swap, leaving the order as found

Geometry and compositor trees convert link values through one table:
- Floats, integers, and booleans convert among each other and into vectors and colors, a scalar filling every channel and color alpha reading 1
- Floats and integers read true above zero, vectors true with any nonzero component, colors true through luminance
- Floats convert to integers truncating toward zero
- Vectors convert to floats as the component mean, colors as luminance
- Vectors and colors convert into each other, a vector taking alpha 1 and a color dropping alpha
- Rotations take floats and vectors as XYZ Euler angles in radians, and matrices through their rotation part
- Rotations convert to vectors as Euler angles and to matrices alone, and matrices convert to rotations alone
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

`is_valid` follows host rules:
- `links.new` sets `is_valid` at creation, and a topology change recomputes it, overwriting a Python write
- Passes over `tree.links` find false links, and the node editor alone draws why
- Evaluation ignores `is_valid`, a pair with no conversion leaving its target at the stored `default_value`
- Fields in a `LINE` input hand it the type's zero

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
- Simulation state items read zone input on the first frame
- Simulation outputs replay cached frames when stepped back and compute one step on a jump past the cache
- Simulation state items refuse `CLOSURE` with `RuntimeError`
- For Each bodies run once per element of the output's `domain`, input items taking a field outside and one value inside the zone
- For Each main items return a field on that domain read on the zone output's `Geometry`, and generation items join each pass's geometry

Closures and bundles match items by exact, case-sensitive name:
- Evaluate Closure items match the closure's input and output items by name, and with no closure linked same-named inputs pass through
- Missing closure outputs report `ERROR` in `node_warnings` and read the default, and type differences convert with an `INFO` entry
- Separate Bundle reads Combine Bundle items by name and ignores extra keys
- Missing bundle names and types with no conversion report `ERROR` and read the default
- Bundle item names store `/*&|"^~!,{}()+$#@[];:?<>.-%\=` and leading and trailing spaces as `_`, duplicates taking `_001`
- `define_signature` items sync through `bpy.ops.node.sockets_sync(node_name=...)` in a node editor context alone, `links.new` never syncing them

Item collections (`capture_items`, `bake_items`, `bundle_items`, `index_switch_items`, `enum_items`, `file_output_items`) define variable sockets:
- Collections take `new(socket_type, name)` with socket enums (`FLOAT`, `VECTOR`, `RGBA`, `GEOMETRY`), an idname raising `TypeError` that lists them
- Capture items created with `VECTOR` read back `data_type` `FLOAT_VECTOR`
- Collections take `remove(item)`, `clear()`, and `move(from_index, to_index)`, an index past the range moving nothing
- `enum_items` and `rasterize_items` take `new(name)`, `index_switch_items` and `combine_list_items` take `new()`
- Repeat and For Each items hold `socket_type`, Simulation items add `attribute_domain`, Bake items `attribute_domain` and `is_attribute`
- For Each generation items hold `domain`, and closure, Evaluate Closure, and bundle items hold `structure_type`
- Item sockets follow collection order after the node's fixed sockets
- Item socket identifiers come from a counter per node (`Item_<n>`, For Each `Input_<n>`, `Main_<n>`, `Generation_<n>`) that `clear()` keeps
- Item renames rename item sockets
- Repeat Output starts with one `Geometry` item in geometry trees and none in shader trees, `Item_0` on both zone nodes once paired
- Menu Switch starts with items `A` and `B` and `GEOMETRY` data in geometry trees, `RGBA` in others
- Menu Switch item sockets take identifiers `Item_<n>` in item order on both sides, beside the `Menu` input and `Output` value

## [06]-[INTERFACES]

`NodeTree.interface` declares a group's sockets and panels:
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
- `default_value`, `min_value`, and `max_value` seed group nodes and modifier inputs created after the write, existing ones keeping stored values

Panel toggles follow host rules:
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
- `hide_in_modifier` hides the modifier panel row alone, `properties.inputs.<identifier>` still evaluating its `value`
- Output items take `attribute_domain`
- `default_attribute_name` seeds the modifier's `attribute_name` on an output and sets `type = "ATTRIBUTE"` with that name on an input

Menu sockets store an item's integer identifier and take item names case for case:
- Renames keep every stored selection on sockets, interfaces, and modifiers
- Menu Switch item edits regenerate `properties.inputs` of every modifier on the group
- Values matching no item evaluate to the output type's default with no error
- Group menu inputs list items while a link chain reaches a Menu Switch through group nodes, reroutes, and generic nodes
- Unlinked menu sockets refuse every `default_value`, keeping the stored integer until a definition holding it arrives
- Menu outputs feeding more than one Menu Switch invalidate each such link and list no items with no `node_warnings` entry, equal item names included
- Stock nodes of one built-in menu share one definition, and one output feeds them all

Interface writes update every user before the call returns, and users keep values by identifier:
- Group nodes and modifiers gain and lose sockets with each interface write, with no update call
- Renames and moves keep stored values on group nodes and modifiers
- Moves, removals, and retypes rewrite group node keyframe and driver paths to new indices and drop F-curves of removed sockets
- Retypes convert modifier values, floats truncating to integers, numbers testing nonzero as booleans, menus keeping their integer, data-blocks kept
- Retypes between scalars and arrays reset modifier values to the interface default, and arrays keep leading values
- Group nodes rebuild a retyped socket from the interface default and move its links over
- `remove` followed by `new_socket` issues a fresh identifier, dropping every stored value of the removed item
- `tree.copy()` keeps every identifier
- `node_group` writes on modifiers and compositor effects keep input values across groups with coinciding identifiers

## [07]-[EVALUATION]

Tree edits reach every user's next evaluation, nested groups included:
- Node additions, removals, settings, socket defaults, links, and node mutes evaluate with no tag
- `link.is_muted` writes evaluate after `tree.update_tag()`
- `node_warnings` items hold `message` and `type` (`ERROR`, `WARNING`, `INFO`), filled once an active depsgraph evaluates the modifier
- `node_warnings` lists closure and bundle signature faults, and invalid links and broken zones evaluate with none
- `mod.is_input_used(identifier)` and `mod.is_input_visible(identifier)` answer what menus and toggles leave in use
- Legacy Curve objects pass curves to the modifier, bevel and resolution applying after it

## [08]-[MODIFIER_INPUTS]

`obj.modifiers.new(name, "NODES")` takes a group through `node_group`:
- Groups with no Group Output log `Node group must have a group output node` and evaluate empty with no `node_warnings` entry
- Groups with a first output other than geometry evaluate empty with no `node_warnings` entry
- Groups with a Geometry output and no Geometry input generate geometry
- `properties.inputs.bl_rna.properties` lists `name` and one pointer per input identifier in interface order
- `getattr(properties.inputs, identifier)` reads each input
- `properties.panels.open_<panel identifier>` holds each panel's open state
- `type = "ATTRIBUTE"` reads the attribute on attribute-capable inputs with a `FIELD` or `DYNAMIC` group input, `value` applying elsewhere silently
- Attribute names starting `.select`, `.edge`, or `.corner` read the type's zero
- `properties.outputs.<identifier>.attribute_name` stores each Group Output input past the first on its item's `attribute_domain`
- Output stores replace a same-named attribute of another domain or type
- `value` holds the socket's static type: numbers, data-blocks for ID sockets, item names for menus
- Menu input items list item names in `item.bl_rna.properties["value"].enum_items`
- `item.property_unset("value")` returns an input to the current interface default
- `item.path_from_id("value")` returns the data path keyframes and drivers take on the object
- `mod[identifier]` and `mod.get(identifier)` raise `TypeError`, modifiers holding no ID properties

Input items hold members by socket type:

| [INDEX] | [SOCKETS]                           | [MEMBERS]                                                                             |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | Float, Int, Vector, Color, Rotation | `value`, `type` (`VALUE`, `ATTRIBUTE`), `attribute_name`                              |
|  [02]   | Bool                                | Numeric members, `type` `LAYER` (default under `layer_selection_field`), `layer_name` |
|  [03]   | String, Menu, data-block            | `value`, `type` `VALUE`                                                               |
|  [04]   | Geometry, Matrix, Bundle, Closure   | `name`, `type` `FALLBACK`                                                             |

Compositor effect, strip, and strip modifier inputs sit at `<owner>.properties.inputs.<identifier>`:
- Inputs hold `value`, and strip modifier inputs add `type`
- Values take any float over the interface's `min_value` and `max_value`, the hard range unbounded

## [09]-[BAKING]

Modifier `bakes` hold one item per Simulation Output and Bake node once `node_group` names the group:
- `bake.node` names the node, and a node's bake comes from a scan of `bakes`
- Bake nodes start with no items
- Items hold `bake_id`, and item `bake_target` `INHERIT` follows the modifier's `bake_target`, `PACKED` by default or `DISK`
- Disk bakes write under `<bake_directory>/<bake_id>`, or under `bake.directory` with `use_custom_path`
- `bpy.ops.object.geometry_node_bake_single(session_uid=obj.session_uid, modifier_name=mod.name, bake_id=item.bake_id)` bakes one item
- `geometry_node_bake_delete_single` and `geometry_node_bake_pack_single` take the same arguments, `geometry_node_bake_unpack_single` adding `method`
- `simulation_nodes_cache_bake` bakes simulation zones of objects with `use_simulation_cache` set, skipping Bake nodes
- Baked frames replay in any frame order, hold the last baked frame past the range, and ignore tree edits until the bake is deleted

## [10]-[RECONCILIATION]

Trees reach a desired state through edits in place and keep nodes a user added:
- Desired nodes, links, and interface items are frozen `msgspec.Struct` records keyed by structure, compared against live state before each write
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

Copies take every writable member, item collections growing through their own `new` before sockets pair by identifier:

```python
import bpy
from expression import Error, Ok
from expression.collections import Block


def mirrored(source: Struct, target: Struct, skipped: frozenset[str] = frozenset({"rna_type"})) -> Block[Fault]:
    opaque = (bpy.types.ID, bpy.types.Node, bpy.types.NodeInternalLink)
    sockets: bpy.types.Struct = bpy.types.NodeSocket.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    definition: bpy.types.Struct = source.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def socketed(prop: bpy.types.Property) -> bool:
        return isinstance(prop, bpy.types.CollectionProperty) and prop.fixed_type == sockets

    def grown(prop: bpy.types.CollectionProperty, held: bpy.types.bpy_prop_collection[Struct], live: bpy.types.bpy_prop_collection[Struct]) -> Block[Fault]:
        if prop.srna is None or not {"new", "remove"}.issubset(prop.srna.functions.keys()):
            return Block.empty()
        parameters = Block.of_seq(prop.srna.functions["new"].parameters).filter(lambda parameter: not parameter.is_output)
        for item in tuple(live)[len(held) :]:
            live.remove(item)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return Block.of_seq(tuple(held)[len(live) :]).choose(
            lambda item: host(  # type: ignore[attr-defined, call-overload]  # ty: ignore[unresolved-attribute, invalid-argument-type]
                live.new, *parameters.map(lambda parameter: getattr(item, parameter.identifier) if parameter.identifier in item.bl_rna.properties else parameter.enum_items[0].identifier if parameter.type == "ENUM" else parameter.default)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            ).swap().to_option()
        )

    def member(prop: bpy.types.Property) -> Block[Fault]:
        name, held, live = prop.identifier, getattr(source, prop.identifier), getattr(target, prop.identifier)
        match held, prop:
            case bpy.types.bpy_prop_collection(), _ if socketed(prop):
                found = {socket.identifier: socket for socket in live}
                return Block.of_seq(held).collect(lambda socket: mirrored(socket, found[socket.identifier]) if socket.identifier in found else Block.of(Fault(reason=Reason.ABSENT, subject=name, member=socket.identifier)))
            case bpy.types.bpy_prop_collection(), bpy.types.CollectionProperty() as collection if not isinstance(next(iter(held), None), opaque):
                return grown(collection, held, live) + Block.of_seq(zip(held, live)).collect(lambda pair: mirrored(*pair))
            case bpy.types.bpy_struct(), _ if prop.is_readonly and not isinstance(held, opaque):
                return mirrored(held, live)
            case _ if not prop.is_readonly and held != live and (isinstance(held, bpy.types.ID) or not isinstance(held, bpy.types.bpy_struct | bpy.types.bpy_prop_collection)):
                return Block.of_option(written(target, (name,), held).swap().to_option())
            case _:
                return Block.empty()

    return Block.of_seq(definition.properties).filter(lambda prop: prop.identifier not in skipped and not prop.is_registered_optional).sort_with(lambda prop: (prop.is_readonly, socketed(prop))).collect(member)


def copied(source: bpy.types.Node, tree: bpy.types.NodeTree) -> BlenderResult[bpy.types.Node]:
    def filled(node: bpy.types.Node) -> BlenderResult[bpy.types.Node]:
        if faults := mirrored(source, node, frozenset({"rna_type", "name", "location", "select"})):
            tree.nodes.remove(node)
            return Error(faults)
        return Ok(node)

    return host(tree.nodes.new, source.bl_idname).bind(filled)
```

- Copies keep `label`, `mute`, `hide`, `width`, custom color, and `location_absolute`, and leave identity and frame membership to the target tree
- Read-only pointers copy their content (Color Ramp elements, curve points, `image_user`)
- Writable data-block pointers (`node_tree`, `image`) copy as references
- Writes skip members already equal, dynamic enums with no items (Image node `layer` and `view` with no image) refusing even their current value
- Sockets sort last, mode members and items creating every socket before defaults copy
- Growth fills a `new` parameter its item lacks from the parameter's first enum item or default, the item's own members overwriting it

Group builds declare interface items, then nodes, a zone pair, menu items, and links by identifier, the menu default after its Menu Switch link:

```python
from contextlib import ExitStack

import bpy
from expression import Option, Result
from expression.collections import Block


def subdivided(name: str, chosen: str) -> Result[bpy.types.GeometryNodeTree, Block[Fault]]:
    with ExitStack() as rollback:
        tree: bpy.types.GeometryNodeTree = bpy.data.node_groups.new(name, "GeometryNodeTree")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        rollback.callback(bpy.data.node_groups.remove, tree)
        source, result, steps, shape = (
            tree.interface.new_socket(label, in_out=side, socket_type=kind.bl_rna.identifier)  # type: ignore[union-attr, arg-type]  # ty: ignore[unresolved-attribute, invalid-argument-type]
            for label, side, kind in (("Geometry", "INPUT", bpy.types.NodeSocketGeometry), ("Geometry", "OUTPUT", bpy.types.NodeSocketGeometry), ("Steps", "INPUT", bpy.types.NodeSocketInt), ("Shape", "INPUT", bpy.types.NodeSocketMenu))
        )
        steps.min_value, steps.default_value = 0, 2  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

        def wired(nodes: Block[bpy.types.Node]) -> Result[bpy.types.GeometryNodeTree, Block[Fault]]:
            entry, outlet, start, stop, step, choice = nodes
            start.pair_with_output(stop)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            for item, label in zip(choice.enum_items, ("Coarse", "Fine")):  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                item.name = label
            sockets = {(node, socket.is_output, socket.identifier): socket for node in nodes for socket in (*node.inputs, *node.outputs)}

            def linked(head: bpy.types.Node, output: str, tail: bpy.types.Node, entering: str) -> Result[bpy.types.NodeLink, Fault]:
                return (
                    Option.of_optional(sockets.get((head, True, output)))
                    .map2(tree.links.new, Option.of_optional(sockets.get((tail, False, entering))))
                    .bind(Option.of_optional)
                    .filter(lambda link: link.is_valid)
                    .to_result(Fault(reason=Reason.UNLINKED, subject=repr((head.name, output)), member=repr((tail.name, entering))))
                )

            rows = Block.of(
                (entry, source.identifier, start, "Item_0"),
                (entry, steps.identifier, start, "Iterations"),
                (start, "Item_0", step, "Mesh"),
                (step, "Mesh", stop, "Item_0"),
                (entry, source.identifier, choice, "Item_0"),
                (stop, "Item_0", choice, "Item_1"),
                (entry, shape.identifier, choice, "Menu"),
                (choice, "Output", outlet, result.identifier),
            )
            return accumulated(rows.starmap(linked)).bind(lambda _: written(shape, ("default_value",), chosen)).map(lambda _: tree)

        outcome = accumulated(
            Block.of(bpy.types.NodeGroupInput, bpy.types.NodeGroupOutput, bpy.types.GeometryNodeRepeatInput, bpy.types.GeometryNodeRepeatOutput, bpy.types.GeometryNodeSubdivideMesh, bpy.types.GeometryNodeMenuSwitch).map(
                lambda kind: host(tree.nodes.new, kind.bl_rna.identifier)
            )
        ).bind(wired)
        if outcome.is_ok():
            rollback.pop_all()
        return outcome
```

## [11]-[PLACEMENT]

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
