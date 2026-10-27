# [EVALUATION]

## [01]-[EVALUATION]

Tree edits reach every user's next evaluation, nested groups included:
- Node additions, removals, settings, socket defaults, links, and node mutes evaluate with no tag, `link.is_muted` writes after `tree.update_tag()`
- `node_warnings` items hold `message` and `type` (`ERROR`, `WARNING`, `INFO`), filled once an active depsgraph evaluates the modifier
- `mod.is_input_used(identifier)` and `mod.is_input_visible(identifier)` answer what menus and toggles leave in use
- `CURVE` objects pass curves to the modifier, bevel and resolution applying after it

## [02]-[STOCK_EVALUATORS]

Code reads a stock node's function by evaluating the node, in place of a numpy port restating it. Shader nodes (textures, Map Range, Color Ramp) evaluate in geometry trees, and a scratch point cloud holds the points for one depsgraph pass:

```python
from collections.abc import Callable
from contextlib import ExitStack

import bpy
from expression import Option
import numpy as np
from numpy.typing import NDArray

type Field = Callable[[bpy.types.NodeTree, bpy.types.NodeSocket], bpy.types.NodeSocket]


def sampled(context: bpy.types.Context, points: NDArray[np.float32], field: Field, name: str = ".sampled") -> Option[NDArray[np.float32]]:
    def read(cloud: bpy.types.PointCloud) -> NDArray[np.float32]:
        values: NDArray[np.float32] = np.empty(len(points), np.float32)
        stored: bpy.types.FloatAttribute = cloud.attributes[name]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        stored.data.foreach_get("value", values)
        return values

    with ExitStack() as scratch:
        cloud: bpy.types.PointCloud = bpy.data.pointclouds.new(name)
        tree: bpy.types.GeometryNodeTree = bpy.data.node_groups.new(name, "GeometryNodeTree")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        holder: bpy.types.Object = bpy.data.objects.new(name, cloud)
        scratch.callback(bpy.data.batch_remove, ids=(holder, cloud, tree))
        cloud.resize(len(points))
        position: bpy.types.FloatVectorAttribute = cloud.attributes["position"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        position.data.foreach_set("vector", points.ravel())
        interface: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        for side in ("INPUT", "OUTPUT"):
            interface.new_socket("Geometry", in_out=side, socket_type="NodeSocketGeometry")  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        group_input, store, group_output, position_input = (tree.nodes.new(kind.__name__) for kind in (bpy.types.NodeGroupInput, bpy.types.GeometryNodeStoreNamedAttribute, bpy.types.NodeGroupOutput, bpy.types.GeometryNodeInputPosition))
        store.inputs["Name"].default_value = name  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        for output, target in ((group_input.outputs[0], store.inputs[0]), (field(tree, position_input.outputs[0]), store.inputs["Value"]), (store.outputs[0], group_output.inputs[0])):
            tree.links.new(output, target)
        modifier: bpy.types.NodesModifier = holder.modifiers.new("Field", "NODES")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        modifier.node_group = tree
        scene: bpy.types.Scene = context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scene.collection.objects.link(holder)
        geometry: bpy.types.GeometrySet = holder.evaluated_get(context.evaluated_depsgraph_get()).evaluated_geometry()
        return Option.of_optional(geometry.pointcloud).map(read)
```

- `field` adds nodes to the scratch tree and returns the output the attribute stores (Noise Texture `Fac` over `Position`)
- Chains of stock nodes (noise into Map Range into Color Ramp) evaluate in the same pass

## [03]-[MODIFIER_INPUTS]

`NODES` modifiers evaluate `node_group` by its interface:
- Groups without Group Output (logging `Node group must have a group output node`) or without a Geometry first output evaluate empty with no `node_warnings` entry
- Groups with a Geometry output and no Geometry input generate geometry
- `properties.inputs.bl_rna.properties` lists `name` and one pointer per input identifier in interface order, read through `getattr`
- `properties.panels.open_<panel identifier>` holds each panel's open state
- `type = "ATTRIBUTE"` reads the attribute on attribute-capable inputs with a `FIELD` or `DYNAMIC` group input, `value` applying elsewhere with no fault
- Attribute names starting `.select`, `.edge`, or `.corner` read the type's zero
- `properties.outputs.<identifier>.attribute_name` stores each Group Output input past the first on its item's `attribute_domain`
- Output stores replace a same-named attribute of another domain or type
- `value` holds the socket's static type: numbers, data-blocks for ID sockets, and menu item names from `item.bl_rna.properties["value"].enum_items`
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

Compositor effect, strip, and strip modifier inputs hold `value` at `<owner>.properties.inputs.<identifier>`, strip modifier inputs adding `type`, and values take any float past the interface's `min_value` and `max_value`.

## [04]-[BAKES]

Modifier `bakes` hold one item per Simulation Output and Bake node once `node_group` names the group:
- `bake.node` names the node, and a node's bake comes from a scan of `bakes`
- Bake nodes start with no items
- Items hold `bake_id`, and item `bake_target` `INHERIT` follows the modifier's `bake_target`, `PACKED` by default or `DISK`
- Disk bakes write under `<bake_directory>/<bake_id>`, or under `bake.directory` with `use_custom_path`
- `bpy.ops.object.geometry_node_bake_single(session_uid=obj.session_uid, modifier_name=mod.name, bake_id=item.bake_id)` bakes one item
- `geometry_node_bake_delete_single` and `geometry_node_bake_pack_single` take the same arguments, `geometry_node_bake_unpack_single` adding `method`
- `simulation_nodes_cache_bake` bakes simulation zones of objects with `use_simulation_cache` set, skipping Bake nodes
- Baked frames replay in any frame order, hold the last baked frame past the range, and ignore tree edits until the bake is deleted
