# [PARAMETRIC]

Covers parametric design and analysis through the installed extensions.

## [01]-[ROUTES]

| [INDEX] | [TASK]                                        | [ROUTE]                                                    | [SESSION]         |
| :-----: | :-------------------------------------------- | :--------------------------------------------------------- | :---------------- |
|  [01]   | Procedural geometry outside a Sverchok tree   | Stock geometry nodes on a modifier                         | Background or GUI |
|  [02]   | Sverchok tree build, edit, or read            | Sverchok data API, task queue drained inside the call      | Background or GUI |
|  [03]   | Scalar or attribute formula into geometry     | Formula to Nodes `api` module                              | Background or GUI |
|  [04]   | Scalar formula into shader or compositor math | Node Quick Maths composer classes                          | Background or GUI |
|  [05]   | Node tree into Python                         | RNA walk of the tree                                       | Background or GUI |
|  [06]   | Node layout                                   | Node Arrange in a node editor that drew the tree           | GUI               |
|  [07]   | Shadow, daylight, energy, sun path            | VI-Suite operators under a `node` override                 | GUI               |
|  [08]   | Topology optimization                         | Modal solve in a GUI, solver generator in a background run | Background or GUI |
|  [09]   | Camera or object pose from 2D-3D pins         | PinSolver `view3d.pinsolver_solve`                         | Background or GUI |

## [02]-[SVERCHOK]

Sverchok queues one task per tree on each node addition, link, and property write. With the queue-draining `bpy.app.timers` callback never firing in a background run and firing in a GUI session only after the call returns, a call that reads results drains the queue itself:

```python
import bpy
from sverchok.core.tasks import tasks

tree = bpy.data.node_groups.new("<Tree>", "SverchCustomTreeType")
tree.use_fake_user = True
box, viewer = tree.nodes.new("SvBoxNodeMk2"), tree.nodes.new("SvMeshViewer")
box.Size = <meters>
for source, target in (("Vers", "vertices"), ("Edgs", "edges"), ("Pols", "faces")):
    tree.links.new(box.outputs[source], viewer.inputs[target])
while tasks:
    tasks.run()
bpy.context.view_layer.update()
result = {"object": viewer.base_data_name, "errors": {node.name: node["US_error"] for node in tree.nodes if node.get("US_error")}}
```

- Trees made through the data API hold zero users and leave the saved file unless `use_fake_user` holds
- `index.yaml` at the package root lists every node `bl_idname` by category, and `docs/nodes/<category>/<module>.rst` documents each node
- `node.outputs["<name>"].sv_get()` returns the cached output nested as objects, then elements, then components
- Failed nodes hold their message in `US_error` and traceback in `US_error_stack`
- With generator and range nodes computing linked outputs alone, an output read links it into a viewer or `SvStethoscopeNodeMK2` first
- Unlinked inputs take values through the node property each socket's `prop_name` names
- Property names differ from socket names (`SvGenNumberRange` input `Step` reads `stop_float`)
- `SvFormulaNodeMk5` turns every free name in `formula1`, unknown functions included, into an input socket, and skips processing while one is unlinked
- With socket caches in process memory, reads after a file load drain the queue again
- Viewer objects outlive their node, and viewers sharing one `base_data_name` write one object
- `scene.frame_set(<frame>)` evaluates animated nodes synchronously while `tree.sv_animate` holds
- JSON round trips run through `JSONExporter.get_tree_structure(tree)` and `JSONImporter(<dict>).import_into_tree(tree, print_log=False)`
- JSON imports queue the tree

## [03]-[FORMULA_NODES]

Formula to Nodes compiles a VEX-like script into a geometry node group, and its `api` functions answer dicts holding `ok`:

```python
import importlib

api = importlib.import_module("bl_ext.blender_org.formula_to_nodes.api")
built = api.build_group('float amp = chf("amplitude", 0.2, min=0);\n@P.z += sin(@P.x * chf("frequency", 3.0)) * amp;', name="<Group>", object_name="<Object>")
result = {"built": built, "set": api.set_parameters("<Object>", {"amplitude": 1.0}), "read": api.inspect("<Object>", attributes=["height"], limit=8)}
```

1. `ai.build_system_prompt(for_tools=True)` from `formula_to_nodes.ai` returns the language reference
2. `compiler.check(source, mode)` returns `(ok, error, notes)` with no Blender write
3. `api.build_group(script, name=, object_name=)` builds the group and adds its modifier when the script writes attributes
4. `api.build_group` answers a failed compile with `ok` False and the line, building nothing
5. `mode="FORMULA"` on `api.build_group` builds a field group with one float input per free variable
6. `api.set_parameters(object_name, {name: value})` writes modifier inputs by interface name and tags the object
7. `api.build_group(script, update_group="<Group>")` rebuilds a group the extension made and keeps its modifiers and values
8. `api.inspect(object_name, attributes=, limit=)` returns evaluated attribute values and node warnings

Shader, compositor, and texture math comes from Node Quick Maths composers, its operator gathering input through its dialog alone:

```python
import ast
import importlib

operations = importlib.import_module("bl_ext.blender_org.node_quick_maths.operations")
composers = importlib.import_module("bl_ext.blender_org.node_quick_maths.node_composers")
expression = ast.parse("<expression>", mode="exec").body[0]
parsed = operations.ShaderMathOperation.parse(expression).to_tree(sort_mode="NONE")
composers.ComposeShaderMathNodes(socket_type="GROUP", center_nodes=True, hide_nodes=False).execute(expression, parsed, <tree>)
```

- `ComposeGeometryMathNodes`, `ComposeCompositorMathNodes`, and `ComposeTextureMathNodes` serve the other tree types
- `operations.ShaderMathOperation.validate(module)` answers an `Err` for attribute access, subscripts, and calls outside the scalar function table
- Composed nodes sit at the origin until a layout or `location` writes

## [04]-[TREE_CODE]

Tree code comes from an RNA walk that writes each node's settings and socket values that differ from a fresh node of the same `bl_idname`:

```python
import bpy

SKIPPED = {"rna_type", "name", "location", "location_absolute", "width", "height", "dimensions", "select", "show_options", "show_preview", "hide", "color", "use_custom_color", "color_tag", "warning_propagation"}


def literal(value: object) -> object:
    return tuple(value) if hasattr(value, "__len__") and not isinstance(value, str) else value


def tree_code(tree: bpy.types.NodeTree) -> str:
    scratch = bpy.data.node_groups.new("<Scratch>", tree.bl_idname)
    lines = [f"tree = bpy.data.node_groups.new({tree.name!r}, {tree.bl_idname!r})"]
    for item in (i for i in tree.interface.items_tree if i.item_type == "SOCKET"):
        lines.append(f"socket = tree.interface.new_socket({item.name!r}, in_out={item.in_out!r}, socket_type={item.socket_type!r})")
        lines += [f"socket.default_value = {literal(item.default_value)!r}"] if item.in_out == "INPUT" and hasattr(item, "default_value") else []
    for node in tree.nodes:
        fresh = scratch.nodes.new(node.bl_idname)
        changed = {p.identifier: literal(getattr(node, p.identifier)) for p in node.bl_rna.properties
                   if p.identifier not in SKIPPED and not p.is_readonly and p.type in {"BOOLEAN", "INT", "FLOAT", "ENUM", "STRING"}
                   and literal(getattr(node, p.identifier)) != literal(getattr(fresh, p.identifier))}
        for key, value in changed.items():
            setattr(fresh, key, value)
        lines += [f"node = tree.nodes.new({node.bl_idname!r})", f"node.name, node.location = {node.name!r}, {tuple(node.location)!r}"]
        lines += [f"node.{key} = {value!r}" for key, value in changed.items()]
        lines += [f"node.inputs[{s.identifier!r}].default_value = {literal(s.default_value)!r}" for s in node.inputs
                  if s.enabled and not s.is_linked and hasattr(s, "default_value")
                  and (fresh.inputs.get(s.identifier) is None or literal(s.default_value) != literal(fresh.inputs[s.identifier].default_value))]
    lines += [f"tree.links.new(tree.nodes[{l.from_node.name!r}].outputs[{l.from_socket.identifier!r}], tree.nodes[{l.to_node.name!r}].inputs[{l.to_socket.identifier!r}])" for l in tree.links]
    bpy.data.node_groups.remove(scratch)
    return "\n".join(lines)
```

- With a node declaring only the sockets of its current mode, settings apply to the fresh node before socket comparison
- Interface items emit in `items_tree` order, the order fixing the `Socket_<n>` identifiers links name
- Nested groups emit after their own tree's code and assign `node.node_tree = bpy.data.node_groups[<name>]`
- Node To Python exports drop node settings, which the walk keeps

## [05]-[NODE_LAYOUT]

Node Arrange sizes the layout from node dimensions a node editor fills when it draws the tree:
1. Set `space.pin = True`, then `space.node_tree = <tree>` on a node editor, an unpinned geometry editor following the active object's modifier
2. Select the nodes, then end the call so the editor draws
3. Run `bpy.ops.node.na_arrange_selected()` in the next call under `temp_override(window=, area=<that editor>, region=<its WINDOW region>)`

`scene.na_settings` holds the options (`margin`, `direction`, `socket_alignment`, `add_reroutes`, `iterations`), and `node.na_batch_arrange` arranges every tree of the file.

## [06]-[VI_SUITE]

With VI-Suite operators finding their node through `context.node` and acting from `invoke`, every analysis runs in a GUI:
1. Save the file, `viparams()` writing under `<blend folder>/<blend name>/`
2. Build a `ViN` tree through `bpy.data.node_groups.new("<Tree>", "ViN")`, linking `No_Loc` `Location out` into each task node's `Location in`
3. Run each operator as `bpy.ops.node.<op>("INVOKE_DEFAULT")` inside `temp_override(node=<node>)`
4. Read results in a later call, `livicalc` and `ensim` stepping as modals after the call returns

Site and geometry setup:
- `scene.vi_params.latitude` and `longitude` set a manual site
- EPW sites take `No_Loc.loc = "1"`, then `No_Loc.weather = "<path>.epw"`, both under an override with a node editor as `space_data`
- EPW loads overwrite latitude, longitude, and `vi_params.year`
- Calculation surfaces are faces with a material holding `vi_params.mattype = "1"`, on a visible mesh with no modifiers
- Every other visible mesh shades the calculation surfaces
- Results sit on the calculation mesh as float attributes suffixed with the frame (`scene.frame_current` for a static run)

| [INDEX] | [ANALYSIS] | [NODES]                               | [OPERATORS_IN_ORDER]                | [RESULT]                       |
| :-----: | :--------- | :------------------------------------ | :---------------------------------- | :----------------------------- |
|  [01]   | Shadow     | `No_Vi_SS`                            | `shad`                              | `sm<frame>`, `cindex`          |
|  [02]   | Daylight   | `No_Li_Geo`, `No_Li_Con`, `No_Li_Sim` | `ligexport`, `liexport`, `livicalc` | Attribute the unit names       |
|  [03]   | Energy     | `No_En_Geo`, `No_En_Con`, `No_En_Sim` | `engexport`, `encon`, `ensim`       | `No_En_Sim["reslists"]` rows   |
|  [04]   | Sun path   | `No_Vi_SP` (`suns`)                   | `sunpath`                           | `SunPath` collection, `Sun<n>` |

Task nodes take settings and links:
- `No_Vi_SS` takes `sdoy`, `edoy`, `starthour`, `endhour`, `interval` (steps per hour), and `cpoint` (`"0"` faces, `"1"` vertices)
- `sm<frame>` holds the percent of sun-up steps each point is sunlit
- `No_Li_Con` takes `contextmenu` (`"Basic"`, `"CBDM"`), `skyprog`, `skymenu`, `sdoy`, and `shour`
- `No_Li_Con` `Context out` appears after `liexport`, when it links into `No_Li_Sim`
- Daylight units name attributes, `Lux` holding `illu<frame>` and `virradm2<frame>`, `DF (%)` `df<frame>`, and `W/m2 (f)` `firradm2<frame>`
- Daylight `res<frame>` attributes hold zero
- Energy zones are closed meshes with `vi_params.vi_type = "1"`, one user collection per zone
- Each zone face material takes `bpy.ops.material.envi_node("INVOKE_DEFAULT")` under `temp_override(material=<material>)`
- Each zone material's `No_En_Mat_Con` node then takes `envi_con_type` and `envi_con_con`
- `No_En_Con` takes `sdoy`, `edoy`, `timesteps`, and the zone metric flags (`restt`, `restwh`, `restwc`, `resh`), each off by default
- `No_En_Con` `Context out` links into `No_En_Sim` after `encon`
- Heating and cooling rows read zero until a zone network node holds an HVAC node
- Energy runs write EnergyPlus's error report to text `<resname><frame>out.err`, closing with its completion line and severe-error count

Analysis order follows host effects:
- With shadow runs clearing every ID property of each calculation object's `vi_params`, `vi_type` included, energy zone marks follow a shadow study
- LiVi runs clear every float layer of the calculation mesh, `sm<frame>` included
- Basic sunny skies keep one SUN named `Sun` and delete every other visible SUN
- Ending a sun path display removes other extensions' `frame_change_post` handlers, Sverchok's included
- Sun path displays follow frame-driven Sverchok work

## [07]-[TOPOLOGY_OPTIMIZATION]

Problem setup holds for background runs and GUI sessions:
1. Tag meshes through `obj.topopt.role`: one `DOMAIN`, at least one `SUPPORT` and one `LOAD`, any `PROPERTY` regions
2. Set `load_direction` and `load_total_force_kN` on each load
3. Set `domain_target_density`, `domain_youngs_modulus`, and `domain_poissons_ratio` on the domain
4. Set `scene.topopt.voxel_size` in meters, 20 or more voxels spanning the shortest grid axis
5. Set `max_iterations`, `penalty`, `filter_radius_voxels`, and `density_threshold` on `scene.topopt`
6. `bpy.ops.topopt.voxelize_preview()` applies tagged mesh rotation and scale, builds `TopOpt_Preview`, and writes `scene.topopt.grid_domain_voxels`

GUI solves run `bpy.ops.topopt.solve_3d()`, a modal stepping on a window timer, and later calls read `scene.topopt.is_solving` and `solve_status`, then run `bpy.ops.topopt.generate_mesh()` once solving reads False. With the modal never stepping in a background run, background solves run the solver generator in a Bash call with `run_in_background: true`:

```python
import importlib

import bpy

problem, runner, results = (importlib.import_module(f"bl_ext.blender_org.struct_topo_opt.{name}") for name in ("problem", "solver.runner", "results"))
bpy.ops.topopt.voxelize_preview()
settings = bpy.context.scene.topopt
final = None
for final in runner.solve_3d(problem.gather_problem(bpy.context), penal=settings.penalty, filter_radius=settings.filter_radius_voxels, max_iter=settings.max_iterations, conv_tol=settings.convergence_tol, move_limit=settings.oc_move_limit):
    pass
results.cache_density(final.density)
bpy.ops.topopt.generate_mesh()
bpy.ops.wm.save_mainfile()
```

- With density caches in module memory, `generate_mesh` runs in the solving process
- `gather_problem` raises `problem.ProblemError` naming a missing or duplicate role

## [08]-[PIN_SOLVE]

PinSolver solves a camera or object pose from 2D-3D pin pairs, background runs included:
1. Set the camera as `scene.camera` and `camera.pinsolver_data.solve_mode` (`CAMERA`, `PARENT`, or `OBJECT` with `target_objects`)
2. Add 3 or more `layout_pins` with `use_initial` driving the solve, each with world `pos_3d` and `pos_2d`
3. Write `pos_2d` in the normalized frame coordinates `bpy_extras.object_utils.world_to_camera_view` returns
4. Set calibration flags (`calib_focal_length`, `calib_optical_center`, `calib_k1` to `calib_k3`) with 6 or more pins
5. Run `bpy.ops.view3d.pinsolver_solve()`, then `view_layer.update()`

Other PinSolver work:
- Single-pin solves, pin picking, tweaking, and the ICP preview run in a GUI camera view
- `view3d.pinsolver_bake_animation` solves each frame from `target_clip` tracks and keys the camera
- Mesh alignment sets `scene.pinsolver_icp.source_object` and `target_object`
- `view3d.pinsolver_icp_preview_pin_alignment` over 3 or more pin pairs, then `view3d.pinsolver_icp_apply`, aligns the meshes
