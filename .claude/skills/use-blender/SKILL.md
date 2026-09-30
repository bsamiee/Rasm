---
name: use-blender
description: "Use when a task drives a live or headless Blender or a .blend file, covering sessions, execution, routing, captures, nodes, configuration, interface, extensions, modeling, materials, animation, rendering, drafting, interchange, BIM, and geospatial work."
---

# [BLENDER]

`.mcp.json` runs `blender` (Blender Lab) and `mcp-for-blender` against the user's one GUI Blender, `scripts/headless.py` background processes:
- `bridge.py` runs on the host, `headless.py` and `wrapper.py` on the host and inside each Blender they reach
- Other scripts run inside Blender, imported by module name through the hook
- `as_result` from `results.py` turns a case record into the `result` dict, the class name under `kind` at every level
- `[EXECUTE_BLENDER_CODE]` snippets run through `blender` `execute_blender_code`, `[MCP_FOR_BLENDER]` through `mcp-for-blender`
- `[HEADLESS_CALL]` snippets run through `headless.py call`, or through `run` when they call no extension operator

[REFERENCES]:
- [01]-[SESSIONS](references/sessions.md): Servers, file routes, headless commands, background context, outcomes, quits, and failures
- [02]-[EXECUTION](references/execution.md): Calls and undo steps, operator context and readings, deferred passes, reads and units, and answers
- [03]-[ROUTING](references/routing.md): Tool per job across both servers and `scripts/`, and capability discovery by words
- [04]-[CAPTURES](references/captures.md): Snapshot and capture comparisons of each change, and pictures of the window and its areas
- [05]-[NODES](references/nodes.md): Node trees of every kind, sockets, new and existing trees, modifier inputs, evaluated results, and Sverchok
- [06]-[CONFIGURATION](references/configuration.md): Preferences, add-on records, repositories, keymaps, themes, scale and fonts, and scene units
- [07]-[INTERFACE](references/interface.md): Second windows, views, workspaces, regions, panels, popups, and stored screens
- [08]-[EXTENSIONS](references/extensions.md): Packages, isolated trees, installs, upgrades, removals, and registration with its reads
- [09]-[MODELING](references/modeling.md): Parts built from dimensions with cutters and arrays, edits of existing parts, and joins
- [10]-[LOOK_DEVELOPMENT](references/look-development.md): Materials, library and generated assets, site sky and sun, lights, and the asset library
- [11]-[ANIMATION](references/animation.md): Layered actions, slots, channelbags, and keyframes of new and existing motion
- [12]-[RENDERING](references/rendering.md): Render settings, headless renders, look renders, passes, and renders of the live scene
- [13]-[DRAFTING](references/drafting.md): Sketches, sheet cameras, Line Art, dimensions, and sheets at scale
- [14]-[INTERCHANGE](references/interchange.md): Imports per format, Rhino round trips, CAD file authoring, and batch conversion
- [15]-[BIM](references/bim.md): IFC authoring, Bonsai projects, and IFC drawings and sheets
- [16]-[GEOSPATIAL](references/geospatial.md): Georeference, sun position, Bonsai solar, GIS imports, and climate files

[SCRIPTS]:
- [01]-[HEADLESS](scripts/headless.py): Background Blender commands, `render` frames and a JPEG sheet under `.artifacts/blender/renders/<stem>/`
- [02]-[WRAPPER](scripts/wrapper.py): Hook importing `scripts/` fresh into each call and closing each live call with one undo step
- [03]-[BRIDGE](scripts/bridge.py): Host client of the MCP extension's execute protocol on a session's loopback port
- [04]-[RESULTS](scripts/results.py): Case record conversion, `.artifacts/blender/` folders, and the unknown-object case the scripts share
- [05]-[DISCOVER](scripts/discover.py): Operators, RNA types, and add-on settings matching words across stock Blender and every enabled add-on
- [06]-[RNA](scripts/rna.py): Stored RNA values of a struct as JSON and the function of every registered operator
- [07]-[SCENE](scripts/scene.py): Evaluated points and world bounds of objects and the largest 3D Viewport of open or stored windows
- [08]-[CAPTURE](scripts/capture.py): One scene view to `.artifacts/blender/<name>.png` with the user's view kept, diffed against an earlier capture
- [09]-[SNAPSHOT](scripts/snapshot.py): Evaluated scene state to `.artifacts/blender/<name>.json` with each value changed since an earlier snapshot
- [10]-[NODES](scripts/nodes.py): Digest of one node tree by socket identifier, and its layout through Node Arrange
- [11]-[DRAWING](scripts/drawing.py): Grease Pencil strokes as SVG, PDF, and PNG sheets at scale under `.artifacts/blender/sheets/`
- [12]-[CONVERT](scripts/convert.py): Headless batch of files through importers and one exporter into `.artifacts/blender/convert/<name>/`
