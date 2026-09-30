# [ROUTING]

One tool per job across both servers and `scripts/`, chosen over any tool the server instructions name.

Use sessions.md for routes.

## [01]-[JOBS]

| [INDEX] | [JOB]                        | [TOOL]                             | [DECIDING_FACT]                                                    |
| :-----: | :--------------------------- | :--------------------------------- | :----------------------------------------------------------------- |
|  [01]   | Scene inventory              | `get_objects_summary`              | Collection tree with parent, selection, and visibility per object  |
|  [02]   | One object's stacks          | `get_object_detail_summary`        | Modifiers, constraints, materials, collections, and visibility     |
|  [03]   | Capability by words          | `discover.py`                      | Operators, types, and settings with owner, polls, and values       |
|  [04]   | Operator or type members     | `bpy_api_lookup`                   | Enum items, defaults, and ranges of operators and exposed types    |
|  [05]   | Stock class or module page   | `get_python_api_docs`              | Bundled pages with prose and `examples`, `X.*` lists a namespace   |
|  [06]   | Stock member or phrase       | `search_api_docs`                  | Ranked bundled hits, members outside `bl_rna` included             |
|  [07]   | Concept or workflow          | `search_manual_docs`               | Bundled manual of stock Blender                                    |
|  [08]   | Node sockets per mode        | `describe_node_type`               | Sockets of an exposed node type under `property_overrides`         |
|  [09]   | Existing node tree           | `nodes.py`                         | Values differing from a fresh node, links by socket identifier     |
|  [10]   | Extension on the platform    | `blender -c extension list`        | Local index of every repository, `[installed]` per package         |
|  [11]   | Batch of interchange files   | `convert.py`                       | Headless process, importer per suffix, one outcome per file        |
|  [12]   | GLB or FBX of the live scene | `export_scene`                     | Whole scene, selection, or named selectable objects with children  |
|  [13]   | Geometry from a chosen view  | `capture.py`                       | Framed on objects or the user's view, overlays off                 |
|  [14]   | Editor, panel, node canvas   | `get_screenshot_of_area_as_image`  | One area from Blender's framebuffer whatever window is in front    |
|  [15]   | Whole window with chrome     | `screencapture -x -o -l <id>`      | Drawn window at 1:1 device pixels, top bar and status bar included |
|  [16]   | Layout, mode, selection      | `get_screenshot_of_window_as_json` | Areas, shading, view, active object with mode                      |
|  [17]   | Camera shot preview          | `render_thumbnail_to_path`         | Scene camera at 320 px and 16 Cycles samples                       |
|  [18]   | Library or generated asset   | `mcp-for-blender` asset tools      | One call downloads and imports an asset                            |
|  [19]   | Show the user an object      | `jump_to_view3d_object_by_name`    | Object Mode, the object alone selected and active, framed          |
|  [20]   | Show the user an editor      | `jump_to_tab_by_space_type`        | Workspace whose main area shows the space type                     |

- Precision work (architecture, CAD, BIM) builds geometry from dimensions in code and extension operators, library assets serve props and context

```bash
# Packages of every repository's local index matching <word>, under the user's preferences
blender -c extension list | rg -i '^  \S+( \[installed\])?: .*<word>'
```

Use extensions.md for installs.

## [02]-[DISCOVERY]

`discover("<word>", ...)` lists what stock Blender and every enabled add-on hold for a capability, each hit holding every word as a case-folded substring of its name, label, or description:

```python
# [EXECUTE_BLENDER_CODE] Operators, types, and add-on settings holding every word
from discover import discover
from results import as_result

result = as_result(discover("<word>", "<word>"))
```

```python
# [HEADLESS_CALL] Add-on setting values of a closed file, the session returned to the file on disk in the same call
import bpy
from discover import discover
from results import as_result

result = as_result(discover("<word>", "<word>"))
bpy.ops.wm.revert_mainfile()
```

1. Pass words that together name the capability, one add-on prefix alone (`bim`) returns every class of the add-on
2. Run it live for what exists, the GUI holds every add-on a session loads
3. Run the session form for a closed file's setting values, add-on polls and group reads create group storage the revert drops before `stop`
4. Call an operator with `poll` true as is, one with `poll_in_view` true alone under the largest 3D Viewport override
5. Read `owner` (enabled add-on module, `None` for Blender) and `source` (file defining a Python class) before calling an extension operator
6. Read `params` for the keywords, then `bpy_api_lookup("bpy.ops.<category>.<name>")` for enum items, defaults, and ranges
7. Read a type with `exposed` true through `bpy_api_lookup("<identifier>")` and a node type's sockets through `describe_node_type`
8. Read a type with `exposed` false through `bpy.types.<base>.bl_rna_get_subclass_py("<identifier>").bl_rna` in code
9. Read a compiled stock type (`owner` and `source` `None`) through `get_python_api_docs("bpy.types.<identifier>")`, a Python class from `source`
10. Read a setting's `values` by ID name for each context member of its ID type, by module for add-on preferences, `PASSWORD` strings left out
11. Write a setting through `bpy.context.<member>.<property>` or `bpy.context.preferences.addons["<module>"].preferences.<property>`

- `poll_in_view` reads `None` in a background process and in a window with no 3D Viewport
- Add-on poll and enum callback tracebacks stay out of the answer, a raising poll reads `False`

Use execution.md for the override each operator takes.
