# [NODES]

Node trees of every kind (geometry, shader, world, compositor, Sverchok) built, read, and changed from code through `scripts/nodes.py`.

## [01]-[SOCKETS]

Every tree kind reaches sockets by one rule, mode first:
- `describe_node_type` lists the sockets of every mode of a type together with index, identifier, name, and default
- Mode properties (`data_type`, `mode`) take their value before any socket write, compositor nodes take theirs as a `MENU` input
- `MENU` inputs take the item's display name (`glare.inputs["Type"].default_value = "Bloom"`)
- `inputs["<key>"]` matches a socket available in the current mode by name or identifier, Mix sockets by name alone
- `next(s for s in node.inputs if s.identifier == "<id>")` reaches a repeated name (Mix `B_Color`)
- Integer indexes count every socket, unavailable ones included
- Modes change a socket's `is_unavailable` and `label` and keep its identifier and name (Mesh Line `END_POINTS` labels `Offset` "End Location")
- `links.new` on a type mismatch returns a link with `is_valid` false and raises nothing, into a linked single input it replaces the link

## [02]-[NEW_TREE]

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
- `tree.interface.items_tree` maps each socket name to its identifier
- `arrange("<tree>")` lays the tree out through Node Arrange in a temporary window it draws once and closes, node selection kept
- `Arranged.reroutes` names the reroutes the layout added, a tree holding a cycle raises from Node Arrange
- `arrange` answers `NoWindow` in a background process, and headless builds take it once the GUI opens the file

## [03]-[EXISTING_TREE]

```python
# [EXECUTE_BLENDER_CODE] Interface, changed values, and links of one tree
from nodes import digest
from results import as_result

result = as_result(digest("<name>"))
```

- `<name>` is a node group or the ID holding a tree (material, world, light, scene), a node group first on a shared name, else `UnknownTree`
- `owner` names the holder's RNA type, `interface` each socket with the values that differ from a fresh socket of its type
- `nodes` hold settings, available unlinked inputs, and outputs that differ from a fresh node of their type, by socket identifier
- Group nodes compare against their group's interface defaults, Sverchok socket values show as the node properties their `prop_name` names
- `links` name node and socket identifier at each end with `is_valid` and `is_muted`
- Values round to `Object.location` precision, and layout, width, and selection stay out
- Writes take the identifiers the digest names, and a second `digest` shows the change

## [04]-[MODIFIER_INPUTS]

Modifier input values sit on `modifier.properties.inputs` as one member per interface identifier:

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

- Each input holds `value`, `type`, and `attribute_name`
- `obj.update_tag()` after a value write makes the next evaluated read hold it
- Item assignments on the modifier raise `TypeError: id properties not supported for this type`

## [05]-[RESULTS]

- `depsgraph.object_instances` counts instances, and `snapshot` bounds include them
- `evaluated.evaluated_geometry()` holds `mesh`, `curves`, `pointcloud`, and `instances_pointcloud()`

## [06]-[SVERCHOK]

Sverchok loads in `start` sessions and the GUI, and its trees evaluate through a task queue a GUI timer drains:

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

- Generators output `Vers`, `Edgs`, and `Pols`, `SvMeshViewer` takes `vertices`, `edges`, and `faces`
- Unlinked inputs read the node property their socket's `prop_name` names (`SvBoxNodeMk2` input `Size` reads `Size`)
- Viewer nodes write the object `base_data_name` names
