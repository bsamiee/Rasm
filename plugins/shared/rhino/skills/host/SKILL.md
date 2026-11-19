---
name: host
description: "Use when driving a Rhino document, .3dm file, or Grasshopper 2 canvas or debugging a plug-in in a running Rhino, covering background MCP sessions and attach routes"
---

# [RHINO_HOST]

Covers Rhino and Grasshopper 2 work through the `rhino-mcp-platform` server, with every application left in the background.

[SCRIPTS]:
- [01]-[INSTANCES](scripts/Instances.cs): Documents, slots, and Grasshopper definitions of each Rhino the router reaches

## [01]-[TOOLS]

Tools load through ToolSearch as `mcp__plugin_rhino_rhino-mcp-platform__<tool>` and take an optional `slot`, the Rhino last used when omitted:
- Slots: `list_slots`, `spawn_slot`, `close_slot`
- Reads: `get_context`, `list_objects`, `get_selection`, `get_viewport_image`, `get_commands`
- Edits: `run_command`, `run_csharp`, `set_selection`, `set_camera`, `zoom_to_layer`, `zoom_to_object`, `set_layer_material`
- Documents: `open_doc` (imports into the current document), `save_doc`, `close_doc`
- Grasshopper 2: `g2_start`, `g2_search_components`, `g2_describe_component`, `g2_apply_graph`, `g2_place_component`, `g2_place_slider`, `g2_connect`, `g2_connect_many`, `g2_delete_component`, `g2_get_canvas_graph`, `g2_solve_canvas`, `g2_clear_canvas`

## [02]-[SESSION]

Sessions run in order:
1. `list_slots` lists each Rhino serving a document listener, and `dotnet run scripts/Instances.cs` lists the documents of each
2. With none listed, `watch-app launch com.mcneel.rhinoceros.9 --arguments -nosplash` opens Rhino in the background on an untitled document the router adopts, as `-nosplash` skips the Start window
3. `get_context` reads the slot's selection, viewport, document, and canvas state before the first edit
4. `g2_start` opens the Grasshopper 2 editor on an empty definition, and `g2_apply_graph` places, wires, and solves it in one call
5. `close_doc` discards the task's document, or saves it first with `path`, then `watch-app quit <pid>` exits Rhino with no prompt

Rhino and the router follow host rules:
- Rhinos started without `-nosplash` (Dock, Finder) open the Start window, holding no document and no listener, so the router lists none
- Launch arguments reach only a new instance, so a Rhino already at its Start window keeps it
- Tool calls with no Rhino listening spawn one with `-nosplash -runscript=_MCPSpawn`, taking focus while it launches, and `close_slot` closes it saving nothing
- Spawns while a Rhino sits at its Start window run `_MCPSpawn` in that Rhino behind a "Please Wait..." panel, failing `startup_timeout` after 120 seconds
- Start windows clear through File > New with `watch-app perform`, or a quit and the launch above
- Rhino's Start window, progress, Settings, and About panels hide while Rhino is inactive, reading off screen and `presented` false in `watch-app read`, where `capture --window` still shows them
- `close_slot` refuses an adopted slot with `cannot_close_adopted`, and `watch-app quit` closes it
- Quitting with an edited document asks in a file panel whose `discard` answer reads Delete
- User-started Rhinos announce a listener per document while `RHINO_MCP_AUTOSTART_PORT` holds a value or RhinoAI's MCP auto-load option is on
- `run_csharp` runs C# 10 against `__rhino_doc__`, one call per application at a time on its main-thread queue, with `#r "<assembly>"` below line 1

Members of RhinoCommon and Grasshopper 2 read from their assemblies:
- `inspect_external_assembly` on `roslyn-codelens` lists the build Rasm compiles against by `assemblyName`, `mode` `namespace`, and one `namespaceFilter`
- `ilspycmd -t <Namespace.Type> <rhcore>/RhinoCommon.dll` prints the installed build with generics and `out` parameters, Grasshopper 2 from `<rhcore>/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll`
- `<rhcore>` is `/Applications/Rhino 9.app/Contents/Frameworks/RhCore.framework/Versions/A/Resources`

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
- Rhino command `TestKeepDebuggable` sets `MRKeepDebuggableAfterUpdate`, and Rhino re-signs itself ad hoc with `get-task-allow` once per build
- Ad hoc signatures drop Rhino's TCC grants, so `defaults delete com.mcneel.rhinoceros.9 MRKeepDebuggableAfterUpdate` follows the session
- `netcoredbg` reads `interrupt`, `info threads`, `backtrace`, `break <file>:<line>`, `print <expression>`, `continue`, `detach`, and `quit`, one per line
- `interrupt` stops every thread, so `continue` or `detach` precedes the next MCP call
- Breakpoints in Rhino's own optimized assemblies stay pending, and `--duration <dd:hh:mm:ss>` bounds a `collect`
- People debug in VS Code through the `coreclr` launch configuration `dotnet new rhino` writes

One `run_csharp` call registers a command log until Rhino quits, each command appending its English name, its result, and its command window lines to the `host` timeline of `watch-app watch --record <folder>`:

```csharp
Rhino.RhinoApp.CommandWindowCaptureEnabled = true;
Rhino.Commands.Command.EndCommand += (sender, e) => System.IO.File.AppendAllLines("<folder>/host/rhino.log", Rhino.RhinoApp.CapturedCommandWindowStrings(true).Prepend($"{e.CommandEnglishName} {e.CommandResult}"));
```
