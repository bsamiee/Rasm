# [EVALUATION]

## [01]-[EDITS]

Tree edits reach every user's next evaluation, nested groups included:
- Node additions, removals, mutes, mode members, socket defaults, and links evaluate untagged, `link.is_muted` writes after `tree.update_tag()`
- `NODES` modifiers evaluate `node_group` through its first output, a missing Group Output or non-Geometry first output evaluating empty, unreported
- Groups with a Geometry output and no Geometry input generate geometry
- Curve objects hand the modifier curves, and a curve with `bevel_depth` or `extrude` above zero hands it the beveled or extruded mesh
- `node_warnings` items hold `message` and `type` (`ERROR`, `WARNING`, `INFO`) after each evaluation, background runs included
- Warning nodes reaching Group Output report their `Message` at `warning_type`
- `mod.is_input_used(identifier)` reads false for an input no evaluation reads, `mod.is_input_visible(identifier)` whether the panel draws it
- `execution_time` holds seconds on the evaluated object's modifier, the original modifier reading 0

## [02]-[FIELD_VALUES]

Evaluate built-in nodes (Noise, Voronoi, Map Range, Color Ramp, Math) for their values in place of a numpy port. Shader texture and converter nodes run in geometry trees, an unlinked texture `Vector` reading `Position`, over one scratch point cloud in one depsgraph pass:

```python
def sampled(context: bpy.types.Context, points: NDArray[np.float32], field: Callable[[bpy.types.NodeTree], bpy.types.NodeSocket], /) -> Option[Buffer]:
    def copied(stored: bpy.types.Attribute) -> Option[Buffer]:
        rna: bpy.types.Struct = stored.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        values: Elements[bpy.types.Struct] = stored.data  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        match rna.properties["data"]:
            case bpy.types.CollectionProperty(fixed_type=bpy.types.bpy_struct() as item):
                return Some(column(values, type(item), next(prop.identifier for prop in item.properties if not prop.is_readonly)))
            case _:
                return Nothing

    name = f".{sampled.__name__}"
    cloud = bpy.data.pointclouds.new(name)
    tree: bpy.types.GeometryNodeTree = bpy.data.node_groups.new(name, bpy.types.GeometryNodeTree.__name__)  # type: ignore[arg-type, assignment]  # ty: ignore[invalid-argument-type, invalid-assignment]
    holder = bpy.data.objects.new(name, cloud)
    with ExitStack() as scratch:
        scratch.callback(bpy.data.batch_remove, ids=(holder, cloud, tree))
        cloud.resize(len(points))
        cloud.points.foreach_set("co", points.reshape(-1))
        interface: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        for side in ("INPUT", "OUTPUT"):
            interface.new_socket(name, in_out=side, socket_type=bpy.types.NodeSocketGeometry.__name__)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        source, sink = (tree.nodes.new(kind.__name__) for kind in (bpy.types.NodeGroupInput, bpy.types.NodeGroupOutput))
        tree.links.new(source.outputs[0], sink.inputs[0])
        match tree.links.new(field(tree), sink.inputs[-1], handle_dynamic_sockets=True):
            case bpy.types.NodeLink(to_socket=bpy.types.NodeSocket(identifier=identifier)):
                output: bpy.types.NodeTreeInterfaceSocket = interface.items_tree[identifier]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                output.default_attribute_name = name
            case _:
                return Nothing
        modifier: bpy.types.NodesModifier = holder.modifiers.new(name, "NODES")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        modifier.node_group = tree
        scene: bpy.types.Scene = context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scene.collection.objects.link(holder)
        geometry = holder.evaluated_get(context.evaluated_depsgraph_get()).evaluated_geometry()
        return Option.of_optional(geometry.pointcloud).bind(lambda evaluated: Option.of_optional(evaluated.attributes.get(name))).bind(copied)
```

`sampled(context, points, lambda tree: tree.nodes.new(bpy.types.ShaderNodeTexNoise.__name__).outputs["Fac"])` returns each point's noise factor, and a Noise to Map Range to Color Ramp `field` returns ramp colors in one pass:
- Values keep source type layout: `(n,)` `float32`, `int32`, or `bool`, `(n, 3)` vectors, `(n, 4)` linear colors and quaternions, `(n, 4, 4)` matrices
- Single values (a Value node) return repeated once per point
- Sources whose type no attribute stores (String, Bundle) return `Nothing`
- Objects outside the scene evaluate to their original with no modifier result, and the holder evaluates once linked into `scene.collection`
- Empty `points` arrays return an empty array

## [03]-[MODIFIER_INPUTS]

`NODES` modifiers hold their group's values in `properties`, each member named by an interface identifier:
- `inputs.bl_rna.properties` lists `name` and one pointer per input identifier in interface order, read by `getattr(properties.inputs, identifier)`
- `properties.outputs` holds one item per output past the first, its `attribute_name` storing that output on the interface item's `attribute_domain`
- Output stores replace a same-named attribute of another domain or type
- `properties.panels.open_<panel identifier>` holds each panel's open state
- `type = "ATTRIBUTE"` reads `attribute_name` on an `AUTO` or `FIELD` input, and a `SINGLE` input evaluates `value` with no `node_warnings` entry
- `value` holds numbers, strings, data-blocks, and menu item names
- Menu inputs reaching a Menu Switch list items in `bl_rna.properties["value"].enum_items`
- `value` writes store numbers past the interface's `min_value` and `max_value`
- `item.property_unset("value")` returns an input to the current interface default
- `item.path_from_id("value")` returns the data path keyframes and drivers take on the object
- `mod[identifier]` and `mod.get(identifier)` raise `TypeError`, modifiers holding no ID properties
- `SceneCompositorEffect`, `CompositorStrip`, and `SequencerCompositorModifierData` hold their group's values in `properties.inputs` by identifier

Input items hold members by socket type:

| [INDEX] | [SOCKETS]                           | [MEMBERS]                                                                             |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | Float, Int, Vector, Color, Rotation | `value`, `type` (`VALUE`, `ATTRIBUTE`), `attribute_name`                              |
|  [02]   | Bool                                | Numeric members, `type` `LAYER` (default under `layer_selection_field`), `layer_name` |
|  [03]   | String, Menu, data-block            | `value`, `type` `VALUE`                                                               |
|  [04]   | Geometry, Matrix, Bundle, Closure   | `name`, `type` `FALLBACK`                                                             |

## [04]-[BAKES]

Modifier `bakes` hold one item per Simulation Output and Bake node once `node_group` names the group:
- `bake.node` names the node, and a node's bake comes from a scan of `bakes`
- Bake nodes start with no `bake_items`
- Items hold `bake_id`, item `bake_target` `INHERIT` following the modifier's `bake_target` (`PACKED` by default, or `DISK`)
- Disk bakes write `<bake_id>/meta/<frame>_<subframe>.json` and `<bake_id>/blobs/` under `bake_directory`, `bake.directory` with `use_custom_path`
- Bake nodes bake one frame under `bake_mode` `STILL`, their default, and a frame range under `ANIMATION`
- Ranges follow the scene's `frame_start` and `frame_end`, and `use_custom_simulation_frame_range` takes the item's own `frame_start` and `frame_end`
- Frames before a custom range read the simulation zone's input values, and frames past any range hold the last baked frame
- Baked frames replay in any frame order and ignore tree edits until the bake is deleted
- `bake.data_blocks` items (`id_type`, `id_name`, `lib_name`, `id`) map each data-block the baked data names onto one in the file
- `bpy.ops.object.geometry_node_bake_single(session_uid=obj.session_uid, modifier_name=mod.name, bake_id=item.bake_id)` bakes one item in any run
- `geometry_node_bake_delete_single` and `geometry_node_bake_pack_single` take the same arguments, `geometry_node_bake_unpack_single` adding `method`
- `simulation_nodes_cache_bake(selected=)` bakes simulation zones of objects with `use_simulation_cache` set, skipping Bake nodes
