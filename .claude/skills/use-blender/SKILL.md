---
name: use-blender
description: "Use when a task drives a live or headless Blender or a .blend file, covering sessions, execution, captures, nodes, configuration, interface, extensions, modeling, materials, animation, rendering, drafting, interchange, BIM, and geospatial work."
---

# [USE_BLENDER]

## [01]-[SERVERS]

Each MCP server reaches live Blender through its own add-on and runs `bpy` through `execute_blender_code`:

| [INDEX] | [SERVER]          | [TASK]                                                                        |
| :-----: | :---------------- | :---------------------------------------------------------------------------- |
|  [01]   | `blender`         | Object and `.blend` file summaries, editor navigation                         |
|  [02]   | `blender`         | Window and area screenshots, viewport and thumbnail renders                   |
|  [03]   | `blender`         | Python API and manual search over bundled reference                           |
|  [04]   | `mcp-for-blender` | `generate_3d` models, `search_assets` and `import_asset` from asset libraries |
|  [05]   | `mcp-for-blender` | `get_guide` before rigging, retopology, animation, scene, and material work   |
|  [06]   | `mcp-for-blender` | `look` for visual review of changes                                           |
