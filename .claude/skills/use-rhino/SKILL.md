---
name: use-rhino
description: "Use when a task drives a Rhino document, a .3dm file, or a Grasshopper 2 canvas, covering slots, orientation, layers, selection, commands, files, views, materials, settings, and definitions."
---

# [USE_RHINO]

Covers Rhino and Grasshopper 2 work through the `rhino-mcp-platform` server, with every application left in the background.

[SCRIPTS]:
- [01]-[INSTANCES](scripts/Instances.cs): Session-opening read of every Rhino the router reaches

## [01]-[TOOL_INDEX]

| [INDEX] | [TOOL]                                           | [USE_WHEN]                                                       |
| :-----: | :----------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `mcp__rhino-mcp-platform__list_slots`            | "Which Rhino slots does the router run or adopt?"                |
|  [02]   | `mcp__rhino-mcp-platform__spawn_slot`            | "Start a disposable document owned by the router"                |
|  [03]   | `mcp__rhino-mcp-platform__close_slot`            | "Close a spawned slot"                                           |
|  [04]   | `mcp__rhino-mcp-platform__get_context`           | "What selection, viewport, document, and canvas state is live?"  |
|  [05]   | `mcp__rhino-mcp-platform__list_objects`          | "Which objects match this name, layer, or geometry type?"        |
|  [06]   | `mcp__rhino-mcp-platform__get_selection`         | "What is selected?"                                              |
|  [07]   | `mcp__rhino-mcp-platform__set_selection`         | "Select these ids, names, layer, or geometry type"               |
|  [08]   | `mcp__rhino-mcp-platform__get_viewport_image`    | "What does the viewport show?" / "Frame this box and capture it" |
|  [09]   | `mcp__rhino-mcp-platform__set_camera`            | "Place the active viewport camera"                               |
|  [10]   | `mcp__rhino-mcp-platform__zoom_to_layer`         | "Fit the view to this layer"                                     |
|  [11]   | `mcp__rhino-mcp-platform__zoom_to_object`        | "Fit the view to these objects"                                  |
|  [12]   | `mcp__rhino-mcp-platform__set_layer_material`    | "Set this layer's render material and display color"             |
|  [13]   | `mcp__rhino-mcp-platform__get_commands`          | "Which command name does this need?"                             |
|  [14]   | `mcp__rhino-mcp-platform__run_command`           | "Run this Rhino command string"                                  |
|  [15]   | `mcp__rhino-mcp-platform__run_csharp`            | "Read or edit the document where no tool or command covers it"   |
|  [16]   | `mcp__rhino-mcp-platform__open_doc`              | "Import a file into the current document"                        |
|  [17]   | `mcp__rhino-mcp-platform__save_doc`              | "Write the document to this .3dm path"                           |
|  [18]   | `mcp__rhino-mcp-platform__close_doc`             | "Close the document, saving to a path or discarding"             |
|  [19]   | `mcp__rhino-mcp-platform__ask_user`              | "Ask the user a decision only they can make"                     |
|  [20]   | `mcp__rhino-mcp-platform__g2_start`              | "Start Grasshopper 2"                                            |
|  [21]   | `mcp__rhino-mcp-platform__g2_search_components`  | "Which component matches these words?"                           |
|  [22]   | `mcp__rhino-mcp-platform__g2_describe_component` | "What inputs and outputs does this component take?"              |
|  [23]   | `mcp__rhino-mcp-platform__g2_apply_graph`        | "Place sliders and components, wire them, and solve in one call" |
|  [24]   | `mcp__rhino-mcp-platform__g2_place_component`    | "Place one component"                                            |
|  [25]   | `mcp__rhino-mcp-platform__g2_place_slider`       | "Place one number slider"                                        |
|  [26]   | `mcp__rhino-mcp-platform__g2_connect`            | "Wire one output to one input"                                   |
|  [27]   | `mcp__rhino-mcp-platform__g2_connect_many`       | "Wire a batch of outputs to inputs"                              |
|  [28]   | `mcp__rhino-mcp-platform__g2_delete_component`   | "Delete one component"                                           |
|  [29]   | `mcp__rhino-mcp-platform__g2_get_canvas_graph`   | "What does the canvas hold, and what data flows on each wire?"   |
|  [30]   | `mcp__rhino-mcp-platform__g2_solve_canvas`       | "Did the canvas solve, and which components failed?"             |
|  [31]   | `mcp__rhino-mcp-platform__g2_clear_canvas`       | "Remove every object from the canvas"                            |

## [02]-[SESSION]

- Inspect UI, logs and crash reports; resolve dialogs and errors, discard untitled and scratch work, save named files in place, decline submission

Sessions open on:
1. `dotnet run ${CLAUDE_SKILL_DIR}/scripts/Instances.cs` lists each Rhino visible to the router with its documents, slots, and Grasshopper definitions
2. Document of the work, one case:
    - New work: an open untitled, unmodified document, else `spawn_slot` starts one without a splash screen on the default template
    - Existing work: use the `watch-apps` skill to open the file in Rhino
3. `list_slots` adopts the announced listeners, then `get_context` reads the target slot's selection, viewport, document, and canvas state

- One Rhino application holds every open document, and each `spawn_slot` adds a document window to it
- Documents outside the task close through `close_doc` with `path` set to a named document's file, untitled and scratch documents with none
- Grasshopper 2 keeps definitions as tabs of one editor: `g2_start`, then `Editor.Instance.Documents.Queue(Document.NewActiveDocument(), null)`
- `run_csharp` runs C# 10, one call per application at a time on its main-thread queue, with `#r "<assembly>"` below line 1
