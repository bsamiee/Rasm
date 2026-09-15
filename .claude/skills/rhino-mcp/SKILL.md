---
name: rhino-mcp
description: "Use when a task drives a Rhino session or a Grasshopper 2 canvas, covering slots, scripts, documents, captures, and materials."
---

# [RHINO_MCP]

`.mcp.json` runs `rhino-mcp-platform`, the router of the `Rhino-MCP-Platform` yak package, as a project-scope stdio server.

- Slot: One listener per open Rhino document bound to `RhinoDoc`, one port per listener from 10500 up
- Argument: Every tool but `spawn_slot` and `list_slots` takes `slot`, required on `close_slot` alone
- Default: Omitted `slot` targets the last used or open Rhino, a spawn when none runs
- Result: `{payload}` or `{error: {code, message}}`

## [01]-[SLOTS]

Every router process lists the same slots, other sessions share the slots and `.artifacts/rhino/`

- `spawn_slot {"version": "9"}` — `slotId` of a new document in the running Rhino when one runs
- `list_slots {}` — `payload[]` of `{slotId, port, pid, version, adopted, endpoint}`, proves a held `slot` is live
- `adopted: true` marks a slot from a document that advertised itself
- `close_slot {"slot": "<slot>"}` — Stops a spawned slot's listener, document and Rhino stay open, an adopted slot answers `cannot_close_adopted`
- `ask_user {"question": "<question>", "options": ["<option>"]}` — Answers a refusal, the tool serves the Rhino MCP panel's agent alone

[QUIT]:
- With `doc.Modified` false on every open document, `osascript -e 'tell application "<app>" to quit'` ends a Rhino the router spawned

[ADOPTION]:
- Each open document of a user-started Rhino is a slot after the next `list_slots`, a Rhino at its start dialog has none
- Documents advertise themselves once Rhino Options → AI finds and enables an agent CLI, `[]` with a document open means none is enabled
- `MCPStart` typed in a document starts the document's listener, Return accepts the `Port <10500>` prompt

## [02]-[SCRIPTING]

Scripts print one JSON line holding the post-condition read, Python first and C# for what Python cannot reach:
- `__rhino_doc__` — Slot's `RhinoDoc` in both languages, `scriptcontext.doc` and `rhinoscriptsyntax` resolve elsewhere
- `run_command {"command": "_Box 0,0,0 10,10,10 _Enter"}` — `Done.` after the command
- `get_commands {"filter": "<substring>"}` — First line counts the matches, 200 listed per call
- Use `search-code` for a RhinoCommon or Grasshopper2 member

`run_csharp {"script": "<statements>"}` — Same shape from `Console.WriteLine` in a statement body:
- Compile failures, top-level `return` included, answer `Compile Error` with no detail

`run_python {"script": "<python>"}` — `payload.{stdout, stderr}`, a raise answers `payload.{error, message}` with no stdout:
- `doc.Objects.Count` counts hidden and deleted objects, iterating `doc.Objects` skips both
- Scripts take absolute paths of the local filesystem, the working directory is `/`
- Rhino 9 runs CPython 3.13, a script uses no newer form
- `# r: <package>` as first line installs a pip package into Rhino's site-env before the script runs, `# env: <dir>` adds a directory to `sys.path`
- `RhinoDoc.Create` makes a document without a window, a Rhino quit alone closes such a document

[VERDICT]:
- Results without `error` prove nothing about the document, a silent failure and a no-op read as success
- Post-conditions read through `list_objects`, a document read in the same script, `g2_get_canvas_graph`, or the written `.3dm`

## [03]-[DOCUMENT]

- `open_doc {"path": "</abs/file.3dm>", "clearFirst": false}` — Imports into the slot's document, `imported` counts hidden and skips deleted objects
  - `open -a <app> </abs/file.3dm>` opens a file as a document of its own, a slot after the next `list_slots`
- `save_doc {"path": "</abs/file.3dm>"}` — Writes a copy and leaves `doc.Path` unchanged, `objects` counts hidden and deleted objects
- `close_doc {"path": "</abs/file.3dm>"}` — Result reports closed while the document can stay open, `Rhino.RhinoDoc.OpenDocuments()` proves the close

## [04]-[SCENE]

- `get_context {}` — First call of a task, `grasshopper[]` holds `{version, canvasOpen, componentCount, wireCount}`
- `list_objects {"layer": "<path>", "geometryType": "<type>", "includeHidden": false}` — `{id, name, layer, type}` per object
- `get_selection {}` — Selected objects
- `set_selection {"ids": ["<guid>"], "names": ["<name>"], "layer": "<path>", "geometryType": "<type>"}` — Union of the filters
- `set_layer_material {"layer": "<path>", "color": "#FF0000", "transparency": 0.0, "gloss": 0.5, "applyToLayerColor": true}` — Render material
- Layer changes beyond material go through `doc.Layers` in a script

## [05]-[VIEWPORT]

- `set_camera {"location": {"x": 0, "y": 0, "z": 0}, "target": {"x": 0, "y": 0, "z": 0}, "lensLength": 50}` — Active viewport camera
- `zoom_to_layer {"layer": "<path>"}` — Active viewport to a layer's bounding box
- `zoom_to_object {"ids": ["<guid>"]}` — Active viewport to the objects' bounding box

[CAPTURE]:
- `tools/rhino/capture.py` writes `.artifacts/rhino/<name>.jpg` at 1280x720, the largest size `Read` shows whole
- `<name>` is a short label reused through a task, a re-capture overwrites the file
- `capture(doc, name)` frames every visible object in `Perspective` under `Shaded`, locked objects included, hidden objects and off layers excluded
- `Capture` holds `path` and `frame`, a second capture for `magick compare` runs in the same script after the document change with that `frame`
- Captures draw the selection highlight and gumball, `doc.Objects.UnselectAll()` precedes each
- RhinoCommon stubs live outside the repository, a program under `tools/rhino/` silences the unresolved imports at file scope

```python
# r: msgspec
# [RUN_PYTHON] Capture of every visible object, then one of a world bounding box in another view and mode, <root> is the repository root
import runpy
capture = runpy.run_path("<root>/tools/rhino/capture.py")["capture"]
print(capture(__rhino_doc__, "<name>"))
print(capture(__rhino_doc__, "<name>", ((<x0>, <y0>, <z0>), (<x1>, <y1>, <z1>)), view="Top", mode="Pen"))
```

```bash
# Pixels of <b> that differ from <a> in red over the faded <a>, exit 1 marks a difference
magick compare .artifacts/rhino/<a>.jpg .artifacts/rhino/<b>.jpg .artifacts/rhino/<diff>.png
# Captures tiled at 640x360, four fill one 1280x720 sheet
magick montage .artifacts/rhino/{<a>,<b>,<c>,<d>}.jpg -geometry 640x360 .artifacts/rhino/<sheet>.jpg
```

## [06]-[GRASSHOPPER]

Grasshopper 2 changes apply a graph with a solve, read the diagnostics, then read the canvas:
- `g2_start {}` — `canvasOpen` in `get_context` proves the start
- `g2_search_components {"query": "<substring>", "category": "<chapter>", "subcategory": "<section>"}` — `guid` per match is the selector
  - Filters match a chapter and section name exactly (`Curve`, `Conic`)
- `g2_describe_component {"name": "<Name>"}` — `inputs[]` and `outputs[]` name the port selectors
- `g2_place_component {"selector": "<guid>", "x": 100, "y": 100, "solve": false}` — `id` of one component
- `g2_place_slider {"min": 0, "value": 5, "max": 10, "decimals": 3, "name": "<UserName>", "solve": false}` — `id` of one slider
- `g2_connect {"src_id": "<guid>", "src": "<port>", "dst_id": "<guid>", "dst": "<port>", "solve": false}` — one wire, `""` selects a slider's port
- `g2_connect_many {"wires": [{"SrcId": "<guid>", "Src": "<port>", "DstId": "<guid>", "Dst": "<port>"}]}` — wires with one solve
- `g2_apply_graph {"sliders": [<slider>], "components": [<component>], "wires": [<wire>], "solve": true}` — `placed[]` maps key to id
  - Solve summary of `g2_solve_canvas` follows `placed[]`
- `g2_solve_canvas {}` — solve with `diagnostics[]`
- `g2_get_canvas_graph {"include_data": true, "sample_size": 3}` — `objects[]` with `messages[]` and `inputs[].data.sample`, `wires[]`
  - Wrong-type wires answer `ok: true` and solve with zero diagnostics (a circle into a number samples its circumference)
  - Canvas builds assert every wired input's `sample`
- `Editor.Instance.Canvas.Document` is the canvas in `run_python` after `from Grasshopper2.UI import Editor`, `Objects.ActiveObjects` lists objects
- `g2_clear_canvas {"confirm": true}` — `removed` counts every object
