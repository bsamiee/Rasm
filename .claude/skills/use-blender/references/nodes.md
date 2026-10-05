# [NODES]

Node trees of every kind (geometry, shader, world, compositor, Sverchok) built, read, and changed from code through `scripts/nodes.py`.

## [01]-[SOCKETS]

Sockets of every tree kind take one sequence, mode first:
1. `describe_node_type` lists the sockets of every mode of a type with index, identifier, name, and default
2. Mode properties (`data_type`, `mode`) take their value before any socket write
3. `next(s for s in node.inputs if s.identifier == "<id>")` reaches a socket by the identifier `describe_node_type` or a digest names
4. `tree.links.new(<output>, <input>)` returns the link, `is_valid` false on a type mismatch

- Compositor modes take a `MENU` input's item display name (`glare.inputs["Type"].default_value = "Bloom"`)
- `inputs["<key>"]` finds a current-mode socket by name or identifier (Math `Value_001`), Mix sockets by name alone (`B` reads `B_Color` under `RGBA`)
- Integer indexes count every socket, unavailable ones included
- Modes change a socket's `is_unavailable` and `label` and keep its identifier and name (Mesh Line `END_POINTS` labels `Offset` "End Location")
- Links into a linked single input replace its link

## [02]-[NEW_TREE]

New trees build in one call, interface sockets first, nodes by type, links by socket, then the layout `arrange` draws:

```python
# [EXECUTE_BLENDER_CODE] Row of instances of the object's own geometry, count and spacing as modifier inputs, laid out for the editor
import bpy
from nodes import arrange
from results import as_result

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
result = {
    "layout": as_result(arrange(tree.name)),
    "inputs": [(i.name, i.identifier) for i in tree.interface.items_tree if i.in_out == "INPUT"],
    "vertices": len(evaluated.data.vertices),
    "dimensions": list(evaluated.dimensions),
    "warnings": [w.message for w in modifier.node_warnings],
}
```

- Materials and worlds hold their tree from creation (`material.node_tree`), compositor effects a `CompositorNodeTree` node group
- `tree.interface.items_tree` maps each socket name to its identifier (`Socket_1`), identifiers following creation order and surviving a rename
- Interface sockets hold `default_value`, `min_value`, `max_value`, and `subtype`
- Evaluated vertex counts show a tree's output, `modifier.node_warnings` staying empty on a tree that outputs nothing
- `arrange` prints one `Warning: 1 × Draw region` timing line from its draw, and Node Arrange raises on a tree holding a cycle
- Headless builds take `arrange` once the GUI opens the file

## [03]-[EXISTING_TREE]

Existing trees change in one sequence:
1. `digest("<name>")` reads the tree's interface, values differing from a fresh node, and links by socket identifier
2. Writes take the identifiers the digest names
3. `digest` run again shows the change

```python
# [EXECUTE_BLENDER_CODE] Interface, changed values, and links of one tree
from nodes import digest
from results import as_result

result = as_result(digest("<name>"))
```

- `<name>` names a node group or the ID holding a tree (material, world, light, scene)
- Sverchok socket values show as the node properties their `prop_name` names

## [04]-[MODIFIER_INPUTS]

Modifier input values sit on `modifier.properties.inputs` as one member per interface identifier:

```python
# [EXECUTE_BLENDER_CODE] Group input <Input> set by its interface name, the evaluated result read
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
- `obj.update_tag()` after a value write makes the next evaluated read hold it, live and in background

## [05]-[RESULTS]

Evaluated results read through the depsgraph, instances apart from the mesh until Realize Instances joins them:
- Instances stay out of the evaluated mesh and `Object.dimensions`, `depsgraph.object_instances` counting them and `snapshot` bounds including them
- Vertices, attributes, and `Object.dimensions` include instances after Realize Instances turns them into mesh
- `evaluated.evaluated_geometry()` holds `mesh`, `curves`, `pointcloud`, and `instances_pointcloud()`
- `instance_transform` reads column-major through `foreach_get`, the translation at `[3, :3]`
- `obj.evaluated_get(depsgraph).data.attributes` holds evaluated attributes
- `bpy.data.meshes.new_from_object(<evaluated>)` bakes the result to a mesh datablock

## [06]-[SVERCHOK]

Sverchok loads in `start` sessions and the GUI, and its trees evaluate through a task queue the call drains:

```python
# [HEADLESS_CALL] Box generator into a mesh viewer, evaluated in the call
import bpy
from sverchok.core.tasks import tasks

tree = bpy.data.node_groups.new("<tree>", "SverchCustomTreeType")
box, viewer = tree.nodes.new("SvBoxNodeMk2"), tree.nodes.new("SvMeshViewer")
box.Size = <meters>
for source, target in (("Vers", "vertices"), ("Edgs", "edges"), ("Pols", "faces")):
    tree.links.new(box.outputs[source], viewer.inputs[target])
while tasks:
    tasks.run()
result = {"object": viewer.base_data_name, "dimensions": list(bpy.data.objects[viewer.base_data_name].dimensions)}
```

- Generators output `Vers`, `Edgs`, and `Pols`, `SvMeshViewer` taking `vertices`, `edges`, and `faces`
- Unlinked inputs read the node property their socket's `prop_name` names (`SvBoxNodeMk2` input `Size` reads `Size`)
- Viewer nodes write the object `base_data_name` names
