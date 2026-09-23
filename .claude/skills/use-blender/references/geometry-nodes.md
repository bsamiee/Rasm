# [GEOMETRY_NODES]

Parametric geometry is a node group on a Nodes modifier: the group's interface declares inputs, the modifier holds their values, and the depsgraph evaluates the result.

## [01]-[GROUP]

`describe_node_type` lists each node's sockets before code links them, `digest("<Group>")` from `scripts/nodes.py` reads an existing tree, links name sockets by their `name`:

```python
# [EXECUTE_BLENDER_CODE] Row of instances of the object's own geometry, count and spacing as modifier inputs
import bpy

obj = bpy.data.objects["<Object>"]
tree = bpy.data.node_groups.new("<Group>", "GeometryNodeTree")
tree.interface.new_socket("Geometry", in_out="INPUT", socket_type="NodeSocketGeometry")
count = tree.interface.new_socket("Count", in_out="INPUT", socket_type="NodeSocketInt")
count.default_value, count.min_value = 3, 1
spacing = tree.interface.new_socket("Spacing", in_out="INPUT", socket_type="NodeSocketFloat")
spacing.default_value, spacing.subtype = 2.5, "DISTANCE"
tree.interface.new_socket("Geometry", in_out="OUTPUT", socket_type="NodeSocketGeometry")
nodes, link = tree.nodes, tree.links.new
group_in, group_out = nodes.new("NodeGroupInput"), nodes.new("NodeGroupOutput")
line, offset = nodes.new("GeometryNodeMeshLine"), nodes.new("ShaderNodeCombineXYZ")
instance, realize = nodes.new("GeometryNodeInstanceOnPoints"), nodes.new("GeometryNodeRealizeInstances")
line.mode = "OFFSET"
link(group_in.outputs["Count"], line.inputs["Count"])
link(group_in.outputs["Spacing"], offset.inputs["X"])
link(offset.outputs["Vector"], line.inputs["Offset"])
link(line.outputs["Mesh"], instance.inputs["Points"])
link(group_in.outputs["Geometry"], instance.inputs["Instance"])
link(instance.outputs["Instances"], realize.inputs["Geometry"])
link(realize.outputs["Geometry"], group_out.inputs["Geometry"])
modifier = obj.modifiers.new("<Group>", "NODES")
modifier.node_group = tree
evaluated = obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
result = {"inputs": [(i.name, i.identifier) for i in tree.interface.items_tree if i.in_out == "INPUT"], "vertices": len(evaluated.data.vertices), "dimensions": list(evaluated.dimensions), "warnings": [w.message for w in modifier.node_warnings]}
```

- `tree.interface.items_tree` maps each socket name to its identifier (`Socket_1`), identifiers follow creation order and survive a rename
- Interface sockets hold `default_value`, `min_value`, `max_value`, and `subtype`
- `modifier.node_warnings` stays empty on a tree that outputs nothing, the evaluated vertex count proves output

## [02]-[INPUTS]

Modifier input values sit on `modifier.properties.inputs`, one member per interface identifier, an item assignment on the modifier raises `TypeError: id properties not supported`:

```python
# [EXECUTE_BLENDER_CODE] Set a group input by its interface name and read the evaluated result
import bpy

obj = bpy.data.objects["<Object>"]
modifier = obj.modifiers["<Modifier>"]
identifier = next(i.identifier for i in modifier.node_group.interface.items_tree if i.item_type == "SOCKET" and i.in_out == "INPUT" and i.name == "<Input>")
getattr(modifier.properties.inputs, identifier).value = <value>
obj.update_tag()
depsgraph = bpy.context.evaluated_depsgraph_get()
instances = sum(1 for i in depsgraph.object_instances if i.is_instance and i.parent and i.parent.original == obj)
result = {"vertices": len(obj.evaluated_get(depsgraph).data.vertices), "instances": instances}
```

- Each input holds `value`, `type` (`VALUE` or `ATTRIBUTE`), and `attribute_name` for the attribute an `ATTRIBUTE` input reads
- `obj.update_tag()` after a value change makes the next evaluated read hold it, in the session as in background
- Depsgraph updates without the tag keep the old result

## [03]-[RESULTS]

- Instances stay out of the evaluated mesh and out of `Object.dimensions`, `depsgraph.object_instances` counts them and `snapshot` bounds include them
- Realize Instances turns instances into mesh, then vertices, attributes, and `Object.dimensions` include them
- `evaluated.evaluated_geometry()` holds `mesh`, `curves`, `pointcloud`, and `instances_pointcloud()`
- `instance_transform` reads column-major through `foreach_get`, the translation sits at `[3, :3]`
- `obj.evaluated_get(depsgraph).data.attributes` holds evaluated attributes
- `bpy.data.meshes.new_from_object(<evaluated>)` bakes the result to a mesh datablock
