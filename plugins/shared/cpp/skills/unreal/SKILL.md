---
name: unreal
description: "Use when debugging, rebuilding, or reloading code running inside Unreal Engine on macOS, or reading its logs and crash reports, covering mcp-debugger attaches, Hot Reload, Python remote execution, and Blueprint and Verse limits"
---

# [CPP_UNREAL]

Covers agent routes into a running Unreal Engine on a Mac, with the editor left behind the owner's windows. Use `cpp:coding` for the C++ a session debugs.

## [01]-[ENGINE]

Engine facts every route reads:
- `<engine>` is `/Users/Shared/Epic Games/UE_5.8/Engine`, and `.uproject` `EngineAssociation` names it through `LauncherInstalled.dat`
- `RunUAT`, `RunUBT`, `UnrealEditor-Cmd`, `UnrealAtoS`, `UnrealPak`, and `zen` run the engine's scripts and binaries from `PATH`
- `UnrealEditor.app` carries `get-task-allow` and `UnrealEditor-Cmd` has no hardened runtime, so `lldb` attaches to both
- `UnrealEditor-Cmd <args> -WaitForDebugger` waits in `GuardedMain` until a debugger attaches, then stops on `EXC_BREAKPOINT` in `GameThread`
- `-WaitForDebuggerNoBreak` waits without the stop, and a commandlet run (`-run=pythonscript -script=<py> -unattended -nullrhi`) needs no project
- Engine frames name functions and hold no locals until the editor symbols below install
- Logs sit in `~/Library/Logs/Unreal Engine/<Project>Editor/`, and `GetLogEntries` on `unreal-mcp` reads the running editor's log
- Crash reports sit in `~/Library/Application Support/Epic/UnrealEngine/5.8/Saved/Crashes` and `~/Library/Logs/DiagnosticReports`
- `UnrealAtoS <binary> <pc>` prints the symbol and line of one crash address, and UBT logs to `~/Library/Application Support/Epic/UnrealBuildTool/Log.txt`

## [02]-[DEBUGGING]

Routes by code kind:

| [INDEX] | [CODE]                | [ROUTE]                                                                      |
| :-----: | :-------------------- | :--------------------------------------------------------------------------- |
|  [01]   | C++ modules, plug-ins | `debugger` server, `language` `cpp`                                          |
|  [02]   | Embedded Python       | `remote_execution.py` for runs, `debugger` server `python` for stops         |
|  [03]   | Blueprint             | `debugger` stop in `FFrame::KismetExecutionMessage`, no Blueprint breakpoint |
|  [04]   | Verse                 | None, the engine holds no Verse debugger client                              |

`debugger` tools load through ToolSearch as `mcp__plugin_tool_debugger__<tool>`, and a C++ session runs in order:
1. `create_debug_session` takes `language` `cpp`, and `CODELLDB_PATH` points it at the CodeLLDB that mise installs
2. `attach_to_process` takes `processId` `<pid>` and pauses the engine, and `list_threads` names `GameThread`
3. `continue_execution`, then `wait_for_stop`, returns the `EXC_BREAKPOINT` stop in `GuardedMain` with its frames
4. `get_stack_trace`, `get_local_variables`, and `evaluate_expression` read a stop, native C++ taking a `/nat ` prefix
5. `detach_from_process` leaves the engine running, and `terminateProcess` true ends it

`debugger` rules:
- CodeLLDB 1.11.8 bundled with `mcp-debugger` crashes in `ParseTrieEntries` reading engine symbols, and `CODELLDB_PATH` replaces it
- `mcp-debugger doctor cpp` names the `env:CODELLDB_PATH` adapter and prints version 1.11.8 for any adapter lacking `version.json`
- Expressions without `/nat ` take CodeLLDB's simple syntax and refuse a cast

Blueprint debugging reads the script stack from native stops:
- `FKismetDebugUtilities` breakpoints have no Python, console, or MCP binding, and an editor C++ module exposing them is the route
- `-FATALSCRIPTWARNINGS` turns script warnings fatal, and `FFrame::GetScriptCallstack` prints the script stack where debug info exists

## [03]-[RELOAD]

C++ rebuilds reach a running editor through Hot Reload, as Live Coding sits under `Source/Developer/Windows` with no Mac module:
- `RunUBT -TargetType=Editor Mac Development -Project=<uproject> -Module=<module>` builds while an editor runs
- UBT reads `<engine>/Intermediate/EditorRuns/<pid>`, writes `UnrealEditor-<module>-<n>.dylib`, and the editor's watcher loads it
- Console `Module Recompile <module>` builds and reloads, and `Module List|Load|Unload|Reload <module>` act on loaded modules
- Hot Reload stays off under `-unattended` and in commandlets, and a changed reflected header takes an editor restart
- Hot Reload on a Mac is read from source and unverified, as Rasm holds no `.uproject`

## [04]-[PYTHON]

`<engine>/Plugins/Experimental/PythonScriptPlugin/Content/Python` holds the shell routes into an editor's Python, read from source and unverified:
- `remote_execution.py` runs `ExecuteFile`, `ExecuteStatement`, and `EvaluateStatement` in each editor it finds
- Editors answer multicast `239.0.0.1:6766` and take commands over TCP `127.0.0.1:6776` while `bRemoteExecution` holds `True`
- `unreal.SystemLibrary.execute_console_command(None, "<command>")` sent through `remote_execution.py` runs console commands
- `debugpy_unreal.start(<port>)` blocks the editor until a client attaches, and `debugger` attaches with `language` `python` and `port`
- `UnrealEditor-Cmd <project> -run=pythonscript -script=<py>` runs Python with no editor window

## [05]-[SYMBOLS]

Editor symbols give engine frames locals, and install once per engine:
1. `jq .InstallTags` of the `.item` with `AppName` `UE_5.8` under `~/Library/Application Support/Epic/EpicGamesLauncher/Data/Manifests` lacks `editor_symbols`
2. `<engine>/Binaries/Mac/libUnrealEditor-Core.dSYM` is absent while the tag is missing, as each dSYM sits beside its dylib
3. `df -h /Users/Shared` shows room for 136.80 GiB installed, as the tag holds 3263 files and downloads at most 24.66 GiB of chunks
4. `watch-app` drives Epic Games Launcher in the background to the engine's Options and "Editor symbols for debugging", read before each press
5. `get_local_variables` at an engine stop after the install lists the frame's locals

Symbol installs follow launcher rules:
- Epic Games Launcher holds the Epic sign-in, and no CLI on disk adds an install tag
- `legendary` adds tags through `--install-tag` and needs its own Epic sign-in, which the owner declined
- Engine manifest `<engine>/../.egstore/<id>.manifest` lists each tag's files, and legendary's manifest reader sizes them offline
