---
name: host
description: "Use when driving a Rhino document, .3dm file, or Grasshopper 2 canvas or debugging a plug-in or script in a running Rhino, covering background MCP sessions, debugger attaches, and script breakpoints"
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

| [INDEX] | [READS]                                | [ROUTE]                                                                             |
| :-----: | :------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | Plug-in state                          | `run_csharp` reading its statics and RhinoCommon members                            |
|  [02]   | Stack of each managed thread           | `dotnet-stack report -p <pid>`                                                      |
|  [03]   | Stops, locals, expressions in plug-ins | `debugger` server, attached before the plug-in loads                                |
|  [04]   | Stops, locals, steps in scripts        | RhinoCode script debugger below                                                     |
|  [05]   | Runtime events                         | `dotnet-trace collect -p <pid> --duration <dd:hh:mm:ss> -o <file>`                  |
|  [06]   | Heap, GC, and thread counts            | `dotnet-counters collect -p <pid> --format json --duration <dd:hh:mm:ss> -o <file>` |
|  [07]   | Commands run and lines printed         | Command log below                                                                   |

- Rhino holds no `get-task-allow`, so `lldb -p <pid>` and `dotnet-dump collect` fail `task_for_pid`, and netcoredbg attaches over debugger pipes
- Rhino command `TestKeepDebuggable` sets `MRKeepDebuggableAfterUpdate`, and Rhino re-signs itself ad hoc with `get-task-allow` once per build
- Ad hoc signatures drop Rhino's TCC grants, so `defaults delete com.mcneel.rhinoceros.9 MRKeepDebuggableAfterUpdate` follows the session

`debugger` tools load through ToolSearch as `mcp__plugin_tool_debugger__<tool>`, and a plug-in session runs in order:
1. `nx install <project> -c debug` installs an unoptimized build where Rhino loads it, before the launch
2. `create_debug_session` takes `language` `dotnet`, then `attach_to_process` takes `processId` `<pid>` and `stopOnEntry` false
3. `set_breakpoint` takes `file` as the repository path and `line`, and reads `verified` false until the module loads
4. `run_csharp` loads the plug-in with `PlugIn.LoadPlugIn(<guid>)` and starts the code under test on a worker through `Task.Run`
5. `wait_for_stop` returns frames and locals, and `get_stack_trace`, `get_local_variables`, and `evaluate_expression` read the stop
6. `continue_execution`, `detach_from_process`, and `close_debug_session` leave Rhino running

Plug-in sessions follow runtime rules:
- Products put a `<Name>.rhp` shell without product code or symbols in Rhino's context and run `lib/<Name>.dll` in their own load context
- Rasm assemblies embed their PDB with repository paths and sources, so breakpoints bind with no `sourceFileMap`
- Breakpoints bind in the first loaded module holding their line, so library code each plug-in copies stops in the first plug-in alone
- CI builds set `ContinuousIntegrationBuild`, which writes `/_/` in place of the repository root, and take a `sourceFileMap` from `/_/` to it
- Debug builds stop at breakpoints with locals readable, loaded before the attach or after it, plug-ins Rhino loads at startup included
- Release modules loading after attach run unoptimized and stop, while Release modules loaded before attach never stop, `verified` true included
- `attach` ignores `justMyCode`, so Just My Code stays on and treats optimized code as foreign
- Stops pause every thread, so Rhino's interface and `rhino-mcp-platform` calls wait until `continue_execution`
- Work started on Rhino's main thread (a `run_command` trigger) holds its call until the stop continues
- Rhino unloads no plug-in and its runtime configuration turns `MetadataUpdater` off, so a rebuilt plug-in takes a quit, a launch, and a new attach
- `netcoredbg --interpreter=cli --attach <pid>` stops every thread at attach until `continue`, and reads `set just-my-code 0`, `break <file>:<line>`, `backtrace`, `print <expression>`, `detach`, and `quit` from stdin

RhinoCode traces C# and Python 3 scripts itself and pauses on a `Select Action` command-line prompt, with one `run_csharp` call starting a debug run:

```csharp
using Rhino.Runtime.Code;
using Rhino.Runtime.Code.Execution;
using Rhino.Runtime.Code.Execution.Debugging;
var code = RhinoCode.CreateCode(new Uri("file://<script>"));
var platform = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "RhinoCodePlatform.Rhino3D").GetType("RhinoCodePlatform.Rhino3D.Rhino3DPlatform");
var controls = (DebugControls)platform.GetMethod("CreateDebugControls").Invoke(platform.GetProperty("Instance").GetValue(null), null);
controls.Breakpoints.Add(new CodeReferenceBreakpoint(code, <line>));
controls.Paused += paused => {
    var frame = ((DebugControlsData)paused.Results).CurrentThread.CurrentFrame;
    var slots = frame.TryGetSlots(out var held) ? string.Join(", ", held.Select(slot => $"{slot.Expression}={slot.ValueString}")) : "none";
    System.IO.File.AppendAllText("<log>", $"{frame.Reference} {slots}\n");
};
code.DebugControls = controls;
System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ => Rhino.RhinoApp.InvokeOnUiThread((Action)(() => code.Debug(new DebugContext("<name>", true, true)))));
```

Script debug runs follow RhinoCode rules:
- `.py` scripts resolve their language once `run_command` with `-_ScriptEditor _Run "<file.py>"` started Python 3 in that Rhino
- `InvokeOnUiThread` called inside `run_csharp` runs the debug run inside that call, holding it until the script ends
- Paused runs answer `run_csharp`, while `run_command`, `rhinocode command`, and `DebugControls.StepOver()` leave the prompt waiting
- `watch-app set <field id> -- <option>`, then `watch-app key <pid> 36`, answers `Continue`, `StepIn`, `StepOver`, `StepOut`, `RunToEnd`, or `Stop`
- Field ids are the first `AXTextField` of `watch-app read <pid> --window <number>`, and each answer makes the last id stale
- Prompts ask again on any input other than an option
- `ExecFrame.GetSlots` lists names and values, and `ExecFrame.TryEval` fails for C# and Python alike

One `run_csharp` call registers a command log until Rhino quits, each command appending its English name, its result, and its command window lines to the `host` timeline of `watch-app watch --record <folder>`:

```csharp
Rhino.RhinoApp.CommandWindowCaptureEnabled = true;
Rhino.Commands.Command.EndCommand += (sender, e) => System.IO.File.AppendAllLines("<folder>/host/rhino.log", Rhino.RhinoApp.CapturedCommandWindowStrings(true).Prepend($"{e.CommandEnglishName} {e.CommandResult}"));
```
