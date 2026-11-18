---
name: unreal
description: "Use when building, packaging, profiling, or driving an Unreal Engine project or plug-in, covering engine command lines, containers, Zen storage, traces, and editor servers"
---

# [CPP_UNREAL]

Covers the installed engine through its own command lines and servers, with every editor left in the background. `<engine>` is `/Users/Shared/Epic Games/UE_5.8/Engine`, `<ubt>` is `<engine>/Binaries/DotNET/UnrealBuildTool/UnrealBuildTool`, and `UnrealPak`, `UnrealEditor-Cmd`, and `zen` sit on `PATH`. Use `cpp:coding` for the C++ a project compiles.

[REFERENCES]:
- [01]-[TOOLSETS](references/toolsets.md): Toolset authoring in C++ or Python, registration, errors, and tests
- [02]-[AGENT_SKILLS](references/agent-skills.md): Unreal Agent Skill authoring as Python classes or assets

## [01]-[ROUTES]

| [INDEX] | [USE_WHEN]                                    | [ROUTE]                                                                          |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | "Which automation commands exist?"            | `<engine>/Build/BatchFiles/RunUAT.sh -List`                                      |
|  [02]   | "Build, cook, and package this project"       | `RunUAT.sh BuildCookRun -project=<uproject> -build -cook -stage -pak -archive`   |
|  [03]   | "Package this plug-in"                        | `RunUAT.sh BuildPlugin -Plugin=<uplugin> -Package=<dir>`                         |
|  [04]   | "Which nodes does this graph run?"            | `RunUAT.sh BuildGraph -Script=<xml> -Target=<node> -ListOnly`                    |
|  [05]   | "Which targets does the project declare?"     | `<ubt> -Mode=QueryTargets -Project=<uproject>`                                   |
|  [06]   | "Give clangd a compile database"              | `<ubt> -Mode=GenerateClangDatabase -Project=<uproject> <target> Mac Development` |
|  [07]   | "Run Python in the editor with no window"     | `UnrealEditor-Cmd <uproject> -run=pythonscript -script=<py>`                     |
|  [08]   | "What does this pak hold?"                    | `repak list <pak>`, `repak info <pak>`                                           |
|  [09]   | "Extract files from an Oodle pak"             | `UnrealPak <pak> -Extract <dir> -Filter=<glob>`                                  |
|  [10]   | "What does this IoStore container hold?"      | `retoc info <utoc>`, `retoc list <utoc>`                                         |
|  [11]   | "What do running Zen servers hold?"           | `zen ps`, `zen status`, `zen project ls`, `zen cache info`                       |
|  [12]   | "Which threads and events does a trace hold?" | `zen trace inspect <utrace>`, `zen trace analyze <utrace>`                       |
|  [13]   | "Which timers did a trace record?"            | Insights export below                                                            |
|  [14]   | "Which frame does this address name?"         | `<engine>/Binaries/Mac/UnrealAtoS <binary> <pc>`                                 |

- `BuildCookRun` writes the archive to `-archivedirectory=<dir>`, `BuildGraph -Export=<file.json>` writes the graph as JSON
- Commandlet runs take `-unattended -nullrhi -nosplash -stdout`, engine content with no project, and `-EnablePlugins=<plugin>,<plugin>`
- Engine runs record a trace with `-trace=cpu,log -tracefile=<utrace>` on any `UnrealEditor-Cmd` line
- `repak info` prints version, mount point, and compression, `repak get` fails loading Oodle where `UnrealPak -Extract` reads it
- `retoc to-legacy` converts an IoStore container to a pak, `zen trace analyze` reports live allocations
- UAT logs land in `~/Library/Logs/Unreal Engine/LocalBuildLogs/Log.txt`, editor logs in `<project>/Saved/Logs/<project>.log`
- Derived data cache paths past 119 characters abort the editor, so a scratch home folder stays short

Insights exports CSV with no window from the binary inside its bundle, the loose `Binaries/Mac/UnrealInsights` failing to load its plug-in libraries:

```bash
"<engine>/Binaries/Mac/UnrealInsights.app/Contents/MacOS/UnrealInsights" -OpenTraceFile=<utrace> -unattended -autoquit -noui -nullrhi -ExecOnAnalysisCompleteCmd="TimingInsights.ExportTimers <out.csv>"
```

- `TimingInsights.ExportThreads`, `ExportTimingEvents` with `-columns=`, `-threads=`, `-timers=`, `-startTime=`, `-endTime=`, and `ExportTimerStatistics` take the same place
- `RunUAT.sh ExportTimerStatisticsFromUtrace -TraceFile=<utrace> -CSVFile=<csv> -TimerRegion=<region> -MaxTimerCount=<n>` wraps the statistics export

## [02]-[EDITOR_SERVERS]

Plug-ins enabled in the `.uproject` serve a running editor:

| [INDEX] | [SERVER]       | [PLUG_INS]                                | [ENDPOINT]                      |
| :-----: | :------------- | :---------------------------------------- | :------------------------------ |
|  [01]   | MCP            | `ModelContextProtocol` with `AllToolsets` | `http://127.0.0.1:8000/mcp`     |
|  [02]   | Remote Control | `RemoteControl`                           | HTTP `30010`, WebSocket `30020` |

- MCP with no toolset plug-in enabled starts with no toolset, single toolset plug-ins under `<engine>/Plugins/Experimental/Toolsets` narrow the set
- MCP answers `tools/list` with `list_toolsets`, `describe_toolset`, and `call_tool` while `bEnableToolSearch` stays true, toolset tools dispatching through `call_tool`
- MCP starts with an interactive editor when `bAutoStartServer` is true, in a commandlet only under `-ModelContextProtocolStartServer`, and on demand through console command `ModelContextProtocol.StartServer`
- `ModelContextProtocol.GenerateClientConfig ClaudeCode|Codex|Cursor|VSCode|Gemini|All` merges server `unreal-mcp` into `.mcp.json` beside the `.uproject`, at the workspace root for a source build, which Claude Code joins with the `cpp` row by URL
- `GenerateClientConfig Codex` refuses to overwrite an existing `.codex/config.toml`
- `ModelContextProtocol.StopServer` stops the server, `ModelContextProtocol.RefreshTools` re-registers every toolset after a toolset plug-in is enabled
- Editors on one machine each take a fixed `ServerPortNumber` per workspace, as a client talks to whatever answers on its configured port
- Remote Control `GET /remote/info` lists routes, `PUT /remote/object/call` calls a `UFUNCTION`, `PUT /remote/object/property` reads or writes a property
- Remote Control serves inside the editor, outside it under `-RCWebControlEnable`, and never under `-nullrhi`
- Remote Control runs Python and console commands only with `bRestrictServerAccess`, `bEnableRemotePythonExecution`, and `bAllowConsoleCommandRemoteExecution` set

## [03]-[EDITOR_MCP]

Tool calls inside a live editor go through server `unreal-mcp`, or `unreal-mcp-proxy` where the engine's proxy is installed:
1. `list_toolsets`, then `describe_toolset` on a candidate, reads tool schemas, and a known toolset skips the listing
2. `call_tool` with `toolset_name`, `tool_name`, and `arguments` matching the schema runs the tool, `toolset_name` omitted only for tools the server registers itself
3. `AgentSkillToolset.ListSkills`, then `GetSkills` on a matching entry, loads a project's Agent Skills, which take precedence over generic defaults

- Toolset tools (`BlueprintTools.create`) stay out of `tools/list`, and `bEnableToolSearch=False` registers each one natively at startup
- Bulk changes take a save before and after, as MCP edits across a compile boundary can be irreversible
- C++ rebuilds go through `LiveCodingToolset.CompileLiveCoding`, which returns when the compile finishes with its diagnostics, and other calls during a compile hang or fail
- Dependent calls and mutations of one asset run in sequence, as request order sets no execution order
- Results report failure as a status with no wire error, so anything short of explicit success stops the work
- Play-in-Editor changes editor-only tools, asset creation included, so a wrong result checks for a running PIE
- Instance metadata under `_meta["com.epicgames.unrealengine/instance"]` in the `initialize` result names `processId`, `projectDirectory`, and `engineDirectory` of the answering editor
- `AIAssistantToolset.GetDockedContext` returns empty unless the Claude Code tab is docked inside an asset editor

Proxy binaries under `<engine>/Plugins/Experimental/ModelContextProtocol/Extras/Proxy/Bin/<platform>` keep the client session open across editor restarts:
- `unreal_mcp_proxy --mcp-json <path> install` replaces the HTTP entry with a stdio `unreal-mcp-proxy` entry and keeps the old one under `_upstreamEntry`, `uninstall` restores it
- `unreal_mcp_status` reports the proxy's upstream session, never reachability, so a read-only tool call proves access past a stored catalog
- Proxy recovery sends `notifications/tools/list_changed`, and a client ignoring it keeps a stale catalog until a reconnect or restart
- Killing the proxy closes the client's stdio connection, which only a client reconnect restores
- Missing tools prove no stopped editor, the client configuration, editor MCP startup log, and endpoint come first, then `RefreshTools` and the `.uproject` toolset plug-ins

## [04]-[SETTINGS]

Per-user settings reach every project through the user layer `~/Library/Application Support/Epic/Unreal Engine/Engine/Config/UserEditorPerProjectUserSettings.ini`, and a project's own `Saved/Config/MacEditor/EditorPerProjectUserSettings.ini` overrides it:

```ini
[/Script/PythonScriptPlugin.PythonScriptPluginUserSettings]
bDeveloperMode=True

[/Script/ModelContextProtocolEngine.ModelContextProtocolSettings]
bAutoStartServer=True
```

- Developer mode makes an editor write the `unreal` module stub to `<project>/Intermediate/PythonStub/unreal.py` at startup, commandlets skipping it
- Editors point a Python language server at that folder, `TypeHintingMode` in the same section setting PEP 484 hints
- `ServerPortNumber` and `ServerUrlPath` in the MCP section move its endpoint, `-ModelContextProtocolPort=<port>` moves it for one run
- Settings read back through a Python commandlet: `unreal.find_object(None, "/Script/<module>.Default__<class>").get_editor_property("<name>")`
