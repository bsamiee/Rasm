---
name: host
description: "Use when driving a Rhino document, .3dm file, or Grasshopper 2 canvas or debugging a plug-in in a running Rhino, covering background MCP sessions and attach routes"
---

# [RHINO_HOST]

Covers Rhino and Grasshopper 2 work through the `rhino-mcp-platform` server, with every application left in the background.

[SCRIPTS]:
- [01]-[INSTANCES](scripts/Instances.cs): Session-opening read of every Rhino the router reaches

## [01]-[TOOL_INDEX]

| [INDEX] | [TOOL]                  | [USE_WHEN]                                                       |
| :-----: | :---------------------- | :--------------------------------------------------------------- |
|  [01]   | `list_slots`            | "Which Rhino slots does the router run or adopt?"                |
|  [02]   | `spawn_slot`            | "Start a Rhino the router owns"                                  |
|  [03]   | `close_slot`            | "Close a Rhino the router started, saving nothing"               |
|  [04]   | `get_context`           | "What selection, viewport, document, and canvas state is live?"  |
|  [05]   | `list_objects`          | "Which objects match this name, layer, or geometry type?"        |
|  [06]   | `get_selection`         | "What is selected?"                                              |
|  [07]   | `set_selection`         | "Select these ids, names, layer, or geometry type"               |
|  [08]   | `get_viewport_image`    | "What does the viewport show?" / "Frame this box and capture it" |
|  [09]   | `set_camera`            | "Place the active viewport camera"                               |
|  [10]   | `zoom_to_layer`         | "Fit the view to this layer"                                     |
|  [11]   | `zoom_to_object`        | "Fit the view to these objects"                                  |
|  [12]   | `set_layer_material`    | "Set this layer's render material and display color"             |
|  [13]   | `get_commands`          | "Which command name does this need?"                             |
|  [14]   | `run_command`           | "Run this Rhino command string"                                  |
|  [15]   | `run_csharp`            | "Read or edit the document where no tool or command covers it"   |
|  [16]   | `open_doc`              | "Import a file into the current document"                        |
|  [17]   | `save_doc`              | "Write the document to this .3dm path"                           |
|  [18]   | `close_doc`             | "Close the document, saving to a path or discarding"             |
|  [19]   | `ask_user`              | "Ask the user a decision only they can make"                     |
|  [20]   | `g2_start`              | "Open Grasshopper 2"                                             |
|  [21]   | `g2_search_components`  | "Which component matches these words?"                           |
|  [22]   | `g2_describe_component` | "What inputs and outputs does this component take?"              |
|  [23]   | `g2_apply_graph`        | "Place sliders and components, wire them, and solve in one call" |
|  [24]   | `g2_place_component`    | "Place one component"                                            |
|  [25]   | `g2_place_slider`       | "Place one number slider"                                        |
|  [26]   | `g2_connect`            | "Wire one output to one input"                                   |
|  [27]   | `g2_connect_many`       | "Wire a batch of outputs to inputs"                              |
|  [28]   | `g2_delete_component`   | "Delete one component"                                           |
|  [29]   | `g2_get_canvas_graph`   | "What does the canvas hold, and what data flows on each wire?"   |
|  [30]   | `g2_solve_canvas`       | "Did the canvas solve, and which components failed?"             |
|  [31]   | `g2_clear_canvas`       | "Remove every object from the canvas"                            |

## [02]-[SESSION]

- Inspect UI, logs, and crash reports, resolve dialogs and errors, discard untitled and scratch work, save named files in place, decline submission

Sessions open in order:
1. `dotnet run scripts/Instances.cs` lists each Rhino visible to the router with its documents, slots, and Grasshopper definitions
2. New work takes an open untitled, unmodified document
3. `list_slots` adopts the announced listeners, then `get_context` reads the target slot's selection, viewport, document, and canvas state

- `Instances.cs` prints `Rhino not running` and `list_slots` an empty `payload` with no Rhino up, while `get_context` and every tool taking `slot` start one
- `close_slot` refuses an adopted slot with `cannot_close_adopted`, as Rhinos the owner started stay open
- RhinoAI announces each document's listener while `RHINO_MCP_AUTOSTART_PORT` holds a value or its MCP auto-load option is on
- Rhino Settings and About float as panels that stay off screen while Rhino runs in the background, listed in `windows` and absent from `elements`
- Documents outside the task close through `close_doc` with `path` set to a named document's file, untitled and scratch documents with none
- Grasshopper 2 keeps definitions as tabs of one editor: `g2_start`, then `Editor.Instance.Documents.Queue(Document.NewActiveDocument(), null)`
- `run_csharp` runs C# 10, one call per application at a time on its main-thread queue, with `#r "<assembly>"` below line 1
- `inspect_external_assembly` on `roslyn-codelens` lists members of the RhinoCommon build Rasm compiles against by `assemblyName` (no path), `mode` `namespace`, and one `namespaceFilter`
- `ilspycmd -t <Namespace.Type> <rhcore>/RhinoCommon.dll` prints members of the installed Rhino build with generics and `out` parameters, `<rhcore>` being `/Applications/Rhino 9.app/Contents/Frameworks/RhCore.framework/Versions/A/Resources`
- Grasshopper 2 members read the same way from `<rhcore>/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll`

## [03]-[DEBUGGING]

Rhino runs plug-ins on its bundled .NET under the hardened runtime, and each route takes the `pid` of a `watch-app` line:

| [INDEX] | [READS]                          | [ROUTE]                                                              |
| :-----: | :------------------------------- | :------------------------------------------------------------------- |
|  [01]   | Plug-in state                    | `run_csharp` reading its statics and RhinoCommon members             |
|  [02]   | Stack of each managed thread     | `dnx --yes dotnet-stack report -p <pid>`                             |
|  [03]   | Stack and values at a breakpoint | `netcoredbg --interpreter=cli --attach <pid> --command=<file>`       |
|  [04]   | Runtime events                   | `dnx --yes dotnet-trace collect -p <pid> -o <file>`                  |
|  [05]   | Heap, GC, and thread counts      | `dnx --yes dotnet-counters collect -p <pid> --format json -o <file>` |
|  [06]   | Commands run and lines printed   | Command log below                                                    |

- `lldb -p <pid>` and `dotnet-dump collect` are refused (`task_for_pid`), since Rhino holds no `get-task-allow` entitlement
    - Rhino command `TestKeepDebuggable` sets `MRKeepDebuggableAfterUpdate`, so Rhino re-signs itself ad hoc with `get-task-allow` once per build
    - Ad hoc signatures drop Rhino's TCC grants, so `defaults delete com.mcneel.rhinoceros.9 MRKeepDebuggableAfterUpdate` follows the session
- `netcoredbg` reads `interrupt`, `info threads`, `backtrace`, `break <file>:<line>`, `print <expression>`, `continue`, `detach`, and `quit`, one per line
- `interrupt` stops every thread, so `continue` or `detach` precedes the next MCP call, and Rhino answers again after `detach`
- Breakpoints in Rhino's own optimized assemblies stay pending
- `--duration <dd:hh:mm:ss>` bounds a `collect` of either tool
- People debug in VS Code through the `coreclr` launch configuration `dotnet new rhino` writes

One `run_csharp` call registers a command log until Rhino quits, each command appending its English name, its result, and the lines it wrote to the command window to the `host` timeline of `watch-app watch --record <folder>`:

```csharp
Rhino.RhinoApp.CommandWindowCaptureEnabled = true;
Rhino.Commands.Command.EndCommand += (sender, e) => System.IO.File.AppendAllLines("<folder>/host/rhino.log", Rhino.RhinoApp.CapturedCommandWindowStrings(true).Prepend($"{e.CommandEnglishName} {e.CommandResult}"));
```
