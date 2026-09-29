# [SETUP]

Process, router, and listener behavior behind every slot, with the fix for each failure it produces.

## [01]-[PROCESS]

- Rhino 9 runs from `/Applications/RhinoBETA.app` as `com.mcneel.rhinoceros.9` on .NET 10, a second process from `open -n` exits within seconds
- Process names differ per API: System Events `"Rhinoceros"`, CGWindowList owner `"RhinoBETA"`, AppleScript `tell application "RhinoBETA"`
- WIP builds raise a daily expiry modal in their last 14 days and expire 45 days after release, an expired build saves no file and loads no plugin
- `LicenseUtils.GetLicenseStatus()` and the `RhinoApp` license properties read license state without a dialog

## [02]-[ROUTER]

- Slots sit in `ai/state.db` and listeners announce in `ai/listeners/` under `~/Library/Application Support/McNeel/Rhinoceros` or `RHINO_MCP_HOME`
- Router and the RhinoAI build Rhino loaded come from one package version, a Rhino holding another build lists no slot until it relaunches
- MCP server rows run `ai/bin/rhino-mcp-router`, a copy RhinoAI stages at each load, a router started before a relaunch runs the previous build
- Subagents of one session share one router, and every slot one of them spawns belongs to it
- Router calls time out after 5 minutes
- Claude Code moves an MCP call past 120 seconds to a background task
- Calls past the router limit keep running on Rhino's UI thread and hold every later call until they finish
- RhinoAI starts a listener per new or opened document after any listener started in the process, or with `RHINO_MCP_AUTOSTART_PORT` set
- `AutoLoadMCP` True starts listeners while an enabled agent CLI outside `DisabledAgents` sits on its search path
- Listeners need RhinoAI loaded at startup, `LoadMode` 1 with no `LoadProtection`
- Merged and referenced opens start no listener
- Listeners write their announcement after their port listens
- `rhinocode list` from `Contents/Resources/bin` names each Rhino pid and active document, `lsof -nP -a -p <pid> -iTCP -sTCP:LISTEN` its ports
- Listeners bind from port 10500 up, one port per document
- Scans (`list_slots`, a call without `slot`, a router start in any session) drop rows with a `<pid>-<port>.gone` file in `ai/listeners/`
- Scans adopt every announced listener with no row
- Listeners rewrite their announcement at their first 15 idle seconds after each rewrite, a dropped live listener returns adopted at the next scan
- Live listeners sit beside a `.gone` file after a `spawn_slot` that launched Rhino and after taking a closed document's port
- Rows drop when their adopting router exits or a newer router build starts, and return adopted with the same `pid` and `port`
- Rhino exits bring the router's own rows back through `-nosplash -runscript=_MCPSpawn`, under their names with `adopted: false` on new ports
- `spawn_slot` into a running Rhino answers `unexpected` with `JsonReaderException` when `_New` through a sibling listener adds no document
- `spawn_slot` after a `close_slot` left no active document answers `JsonReaderException`
- `spawn_slot` answers `cancelled` when Rhino quits before the new document starts
- `close_slot` marks the document unmodified, writes it to a temporary `.3dm` with a preview image, and runs `_-Close` on it
- `close_slot` stops the listener without a `.gone` file, its announcement stays until a scan drops it
- Router shutdown at session end kills the Rhino process of every row it owns with `adopted: false`, user documents in it included

## [03]-[LISTENER]

Each slot's document answers JSON-RPC at its `endpoint` in `list_slots`, with every content block and resources the router does not serve:

```bash
# Every content block of one call, router keeps its first alone, a mutating tool mutates again
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"<tool>","arguments":{}}}' | jq -r '.result.content[].text'
# Third-party plugins Rhino has not loaded, with command names and file types
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"resources/read","params":{"uri":"rhino://host/plugins"}}' | jq -r '.result.contents[0].text'
```

- Resource `rhino://host/environment` names Rhino, OS, and .NET
- `run_python` takes its code as `script`, sent from a file through `--data-binary @<file>`, shell-quoted JSON breaks on a quote
- Listener calls skip the project hook, a script there opens with `# env: <skill>/scripts` and `# env: <repository>/tools` to import skill modules
- Direct results hold `stdout` and `stderr` blocks, a raise adds an `error` block first and keeps the stdout printed before it
- `PlugIn.GetPlugInInfo(pair.Key)` over `PlugIn.GetInstalledPlugIns()` pairs lists each plugin's `IsLoaded`, `FileName`, and visible `CommandNames`

## [04]-[PROMPTS_AND_DOCUMENTS]

Calls release prompts and save and close documents with Rhino behind the user's application:
- Commands waiting at a prompt hold every later command, typed writer, and quit in the process, `describe(doc).prompt` names it in every document
- `RhinoApp.PostCancelEvent(<serial>)` cancels a waiting command and `RhinoApp.PostEnterEvent(<serial>, False)` accepts its default at its loop's next event:

```python
# Cancel the waiting command, then wake its event loop
from AppKit import NSApplication, NSEvent, NSEventModifierMask, NSEventType
from CoreGraphics import CGPoint
from Rhino import RhinoApp, RhinoDoc
from System import Int16, IntPtr

RhinoApp.PostCancelEvent(RhinoDoc.ActiveDoc.RuntimeSerialNumber)
NSApplication.SharedApplication.PostEvent(NSEvent.OtherEvent(NSEventType.ApplicationDefined, CGPoint(0, 0), NSEventModifierMask(0), 0.0, IntPtr.Zero, None, Int16(0), IntPtr.Zero, IntPtr.Zero), True)
```

- `describe(doc).prompt` is `None` on the call after a posted event
- `save(doc)` and `close(doc)` finish in-call, inactive documents included
- `close(doc)` runs through another document's listener, a closed document's own listener ends inside the call and returns no reply
- `RhinoDoc.ActiveDoc` assignment and window key and main calls leave the active document unchanged while Rhino is behind
- Files `open -g` opens become the active document behind

## [05]-[DIALOGS]

Modal dialogs hold every command, typed writer, and `open -g` file in the process, `run_python` runs inside their loop:
- `NSApplication.SharedApplication.ModalWindow` names an alert or dialog holding the UI thread, its `ContentView` subviews hold its text and each `NSButton` title and tag
- `NSApplication.SharedApplication.StopModalWithCode(IntPtr(<tag>))` answers an alert with its `<tag>` button once the call returns

- Eto dialogs a call opened keep their message loop after their window closes, a posted event ends it and finishes the call:

```python
# Close the Eto window, then wake its event loop
from AppKit import NSApplication, NSEvent, NSEventModifierMask, NSEventType
from CoreGraphics import CGPoint
from System import Int16, IntPtr
import Eto.Forms

next(window for window in Eto.Forms.Application.Instance.Windows if window.Title == "<dialog title>").Close()
NSApplication.SharedApplication.PostEvent(NSEvent.OtherEvent(NSEventType.ApplicationDefined, CGPoint(0, 0), NSEventModifierMask(0), 0.0, IntPtr.Zero, None, Int16(0), IntPtr.Zero, IntPtr.Zero), True)
```

Modals code raises, each with the call that avoids it:

| [INDEX] | [MODAL]                                        | [AVOIDED_BY]                                                                  |
| :-----: | :--------------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | Changed linked blocks on open                  | `doc.LinkedInstanceDefinitionUpdate` other than `Prompt`                      |
|  [02]   | Unknown plugin data a package supplies on open | Answering No once per plugin id                                               |
|  [03]   | "Unable to find a Zoo server"                  | No `LicenseUtils` checkout, check-in, or return call                          |
|  [04]   | "The requested hatch pattern cannot be found"  | Setting `CurrentHatchPatternIndex` to an index the hatch table holds          |
|  [05]   | "Restart Rhino" after load protection          | Calling `PlugIn.SetLoadProtection` on unloaded plugins alone                  |
|  [06]   | Plugin crashed at last load, stop using it     | Nothing, the answer is recorded                                               |
|  [07]   | Missing Plugins inside `g2_start`              | `yak install` per plugin or `ReinstateSession.Value = False` before the start |

## [06]-[HUNG_OR_CRASHED_PROCESS]

- `ps -o stat,%cpu -p <pid>` at a steady 100% while `tools/list` answers and every `tools/call` times out marks a hung UI thread
- `sample <pid> 1` names the frame a hung UI thread spins in, crash reports naming their pid go to `~/Library/Logs/DiagnosticReports/Rhinoceros-*.ips`
- Reports listed before and after a call batch attribute a crash, other sessions' processes write into the same folder
- Members that can abort the process read one class per call, a batch hides which member is at fault
- Hung or killed processes take every slot, `list_slots` after a restart names adopted documents anew and a router's own rows under their names
- Crashed Rhino processes start again within seconds, a listener `AutoLoadMCP` starts in their first document answers as an adopted row
- Edits since the last macOS autosave die with their process, `file3dm.py` shows what a titled file holds
- Untitled documents of a crashed process return at relaunch from `~/Library/Autosave Information/`
- Unified log reads name how a pid ended and which commands ran:

```bash
# Exit of one pid, (0, 0, 0) a quit and (2, 9, 9) SIGKILL, then every command any session started
log show --last 10m --style compact --predicate 'process == "runningboardd" AND eventMessage CONTAINS "<pid>] termination"'
log show --last 10m --predicate 'process == "Rhinoceros" AND eventMessage CONTAINS "Start command"'
```

## [07]-[QUIT_AND_RELAUNCH]

Quitting Rhino takes every slot with it, and files Rhino reads at launch are edited between quit and relaunch:
1. `save(doc)` saves each titled document with `modified` reading `True`
2. Next call sets untitled `doc.Modified = False` and Grasshopper 2 `Unmodify()`, runs `PlugIn.FlushSettingsSavedQueue()` and `RhinoApp.Exit(False)`
3. Exit reads from `ps -p <pid>` in a later call, `kill -KILL <pid>` ends a Rhino it lists after the quit
4. Settings XML, `containers.xml`, `default.rui`, Grasshopper 2 font files, and macOS defaults take their edits
5. `spawn_slot {"version": "9"}` launches Rhino on the edited files, `open -g -b com.mcneel.rhinoceros.9 <file>...` reopens every saved file, each as its own slot

`doc.Modified = False` holds from a call that edits nothing, a call's own edit marks the document modified again at its end.

`RhinoApp.Exit()` lets Rhino cancel its quit and leaves it running with every document open, a Rhino with no listener quits through AppleScript:

```bash
# Quit a Rhino with no listener
osascript -e "ignoring application responses" -e 'tell application id "com.mcneel.rhinoceros.9" to quit' -e "end ignoring"
```

## [08]-[GRASSHOPPER_2]

- `Editor.Instance` is `None` until Grasshopper 2 starts, `g2_start` starts it and loads a registered Grasshopper 2 plugin Rhino has not loaded
- `PlugIn.LoadPlugIn(PlugIn.IdFromName("Grasshopper2"))` enables `import Grasshopper2` and loads no component library, `g2_start` loads them
- `g2_start` reopens every canvas `~/Library/Application Support/Grasshopper2/Session.ghsession` names
- `Editor.Instance.Close()` hides the editor and makes its canvas inactive, `Visible = False` hides it with its preview and solves running
- `~/Library/Application Support/Grasshopper2/Diagnostics/*.ghlog` logs session restores, plugin load failures, and solve faults
- Opening Grasshopper 2's application menu, by click or by a System Events read, posts to DeepL and writes translation caches
- `Editor.Instance.Menu` reads the menu as Eto objects without opening it
