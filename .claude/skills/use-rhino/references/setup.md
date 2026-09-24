# [SETUP]

Process, router, and listener facts behind every slot, with the repair for each failure they produce.

## [01]-[PROCESS]

- Rhino 9 runs from `/Applications/RhinoBETA.app` as `com.mcneel.rhinoceros.9` on .NET 10, a second process from `open -n` exits within seconds
- Process names differ per API: System Events `"Rhinoceros"`, CGWindowList owner `"RhinoBETA"`, AppleScript `tell application "RhinoBETA"`
- WIP builds expire 45 days after release, an expired build saves no file and loads no plugin until the next WIP build replaces it
- `LicenseUtils.GetLicenseStatus()` and the `RhinoApp` license properties read license state without a dialog
- Geist and Geist Mono sit under `~/Library/Fonts`, where CoreText and Eto read them

## [02]-[ROUTER]

- Slots sit in `~/Library/Application Support/McNeel/rhino-mcp/state.db`, `RHINO_MCP_HOME` moves that directory
- Subagents of one session share one router, and every slot one of them spawns belongs to that router
- Each call runs on Rhino's UI thread under a 5 minute router limit, Claude Code moves a call past 120 seconds to a background task
- Calls past the router limit keep running on Rhino's UI thread and hold every later call until they finish
- RhinoAI starts a listener for every document Rhino opens or creates, whichever process launched Rhino, and announces it in `listeners/`
- Listeners need RhinoAI loaded at startup, `LoadMode` 1 with no `LoadProtection`, and an agent CLI on its search paths kept out of `DisabledAgents`
- Listeners bind from port 10500 up, one port per document, a closed document's port goes to the next document
- Scans (`list_slots`, a call without `slot`, a router start in any session) drop rows with a `<pid>-<port>.gone` file in `listeners/`
- Scans adopt every listener announced there with no row, listeners announce every 15 seconds, and a dropped live one returns adopted
- Departure files of live listeners come from a `spawn_slot` that launched Rhino and from a new document on a closed document's port
- Rows drop when the router that adopted them exits or a newer router build starts, and return adopted with the same `pid` and `port`
- Router shutdown at session end kills the Rhino process of every row it owns with `adopted: false`, the user's documents in that process included

## [03]-[LISTENER]

Each slot's document answers JSON-RPC at the `endpoint` in `list_slots`, with every content block and the resources the router does not serve:

```bash
# Every content block of one call, the router keeps the first alone, a mutating tool mutates again
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"<tool>","arguments":{}}}' | jq -r '.result.content[].text'
# Third-party plugins with load state, command names, and file types, rhino://host/environment names Rhino, OS, and .NET
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"resources/read","params":{"uri":"rhino://host/plugins"}}' | jq -r '.result.contents[0].text'
```

- `run_python` takes its code as `script`, sent from a scratchpad file through `--data-binary @<file>`, shell-quoted JSON breaks on a quote
- Listener calls skip the project hook, a script there opens with `# env: <skill>/scripts` to import the skill's modules
- Direct results hold `stdout` and `stderr` blocks, a raise adds an `error` block first and keeps the stdout printed before it

## [04]-[RHINO_IN_FRONT]

Keystrokes to a prompt, a document window becoming main, and a document close need Rhino as the frontmost application. Focus returns to the user's application within a second, so one `osascript` holds Rhino in front around one listener call:

```bash
# Record the user's application, hold Rhino in front around one listener call, return focus
osascript -e 'tell application "System Events" to get name of first application process whose frontmost is true'
osascript -e 'tell application "System Events" to set frontmost of process "Rhinoceros" to true' \
    -e 'do shell script "curl -s -X POST <endpoint>/ -H \"Content-Type: application/json\" --data-binary @<scratchpad>/<call>.json > <scratchpad>/<out>.json"' \
    -e 'tell application "System Events" to set frontmost of process "<recorded application>" to true'
```

- Commands waiting at a prompt hold every later command, typed writer, and quit in that Rhino, `describe(doc).prompt` names the prompt
- `Rhino.RhinoApp.SendKeystrokes("!", True)` in the call cancels a waiting command, `SendKeystrokes("", True)` accepts its defaults
- `describe(doc).prompt` is `None` on the call after the keystroke
- `close(doc)` closes the window inside the call and makes another open document active, the next `list_slots` no longer lists the slot
- `RhinoEtoApp.MainWindowForDocument(doc).ControlObject.MakeKeyAndOrderFront(None)` held in front activates `doc` while no command waits
- Rhino in front takes the user's keystrokes into its command line, a typed letter starts a command and Enter repeats the last one

## [05]-[DIALOGS]

Modal dialogs hold every command, typed writer, and `open -g` file in the process, `run_python` still runs inside their loop:
- `NSApplication.SharedApplication.ModalWindow` names the alert or dialog holding the UI thread
- System Events reads an alert and clicks its button without bringing Rhino forward:

```bash
osascript -e 'tell application "System Events" to get entire contents of window 1 of process "Rhinoceros"'
osascript -e 'tell application "System Events" to click button "OK" of window 1 of process "Rhinoceros"'
```

- Eto dialogs a call opened keep their message loop after the window closes, a posted event ends it and finishes the call:

```python
# End a dialog a call opened: close the Eto window, then wake the loop waiting for its next event
from AppKit import NSApplication, NSEvent, NSEventModifierMask, NSEventType
from CoreGraphics import CGPoint
from System import Int16, IntPtr
import Eto.Forms
next(window for window in Eto.Forms.Application.Instance.Windows if window.Title == "<dialog title>").Close()
NSApplication.SharedApplication.PostEvent(NSEvent.OtherEvent(NSEventType.ApplicationDefined, CGPoint(0, 0), NSEventModifierMask(0), 0.0, IntPtr.Zero, None, Int16(0), IntPtr.Zero, IntPtr.Zero), True)
```

Modals a setting suppresses, and modals code raises:

| [INDEX] | [MODAL]                                        | [SUPPRESSED_BY]                                                                  |
| :-----: | :--------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | Opening a file another Rhino holds             | `FileSettings.FileLockingOpenWarning = False`                                    |
|  [02]   | Missing annotation font on open                | `Warnings/MissingFontWarning` False                                              |
|  [03]   | Model base point off the origin on open        | `Options/Advanced/DisplayNonOriginModelBasepointWarning` False                   |
|  [04]   | Model and page units differ, page units        | `Options/Advanced/DisableModelAndPageUnitsDifferDialog` and `...OrMMDialog` True |
|  [05]   | Changed linked blocks on open                  | `doc.LinkedInstanceDefinitionUpdate` other than `Prompt`                         |
|  [06]   | Unknown plugin data a package supplies on open | Answering No once per plugin id                                                  |
|  [07]   | Clipboard data at quit                         | `FileSettings.ClipboardOnExit = ClipboardState.DeleteData`                       |
|  [08]   | Update available                               | `Advanced/EnableCheckForUpdates` and `PackageManager/CheckForUpdates` False      |
|  [09]   | Middle mouse click                             | `GeneralSettings.MiddleMousePopupToolbar` naming an existing toolbar             |
|  [10]   | Startup command waiting for input              | `Options/General/StartupCommands` empty                                          |
|  [11]   | "Unable to find a Zoo server"                  | No `LicenseUtils` checkout, check-in, or return call                             |
|  [12]   | "The requested hatch pattern cannot be found"  | Setting `CurrentHatchPatternIndex` to an index the table holds                   |
|  [13]   | "Restart Rhino" after load protection          | Calling `PlugIn.SetLoadProtection` on unloaded plugins alone                     |
|  [14]   | Plugin crashed at last load, stop using it     | Nothing, the answer is recorded                                                  |
|  [15]   | Daily beta expiry in a build's last 14 days    | Nothing, the next WIP build                                                      |
|  [16]   | Missing Plugins inside `g2_start`              | `yak install` of the plugin or `ReinstateSession.Value = False` before the start |

## [06]-[HUNG_OR_CRASHED_PROCESS]

- `ps -o stat,%cpu -p <pid>` at a steady 100% while `tools/list` answers and every `tools/call` times out marks a hung UI thread
- `sample <pid> 1` names the frame a hung UI thread spins in, crash reports land in `~/Library/Logs/DiagnosticReports/Rhinoceros-*.ips`
- Reports listed before and after a probe batch attribute a crash, other sessions' processes write into the same folder
- Members that can abort the process read one class per call, a batch hides the member at fault
- Hung or killed processes take every slot, `list_slots` after a restart names the survivors under new animal names
- Crashed Rhino processes start again within seconds on their own, the first document answers as an adopted row
- Edits since the last macOS autosave die with the process, `file3dm.py` shows what a titled file holds
- Untitled documents of a crashed process return at relaunch from `~/Library/Autosave Information/`
- Pid changes with no new `Rhinoceros-*.ips` report are a quit or relaunch by another session

## [07]-[QUIT_AND_RELAUNCH]

Quitting Rhino takes every slot with it, and files Rhino reads at launch are edited between the quit and the relaunch:
1. `save(doc)` saves each titled document whose `modified` reads `True`, in a call held in front
2. Next call sets `doc.Modified = False` on untitled documents, then `PlugIn.FlushSettingsSavedQueue()` and `RhinoApp.Exit()` (`rhino_closed`)
3. `caffeinate -t 30 -w <pid>` returns once Rhino exits or 30 seconds pass, `kill -KILL <pid>` ends a Rhino that `ps -p <pid>` still lists
4. Settings XML, `containers.xml`, `default.rui`, Grasshopper 2 font files, and macOS defaults take their edits now
5. `spawn_slot {"version": "9"}` launches Rhino, `open -g -b com.mcneel.rhinoceros.9 <file>...` reopens every saved file, each as its own slot
6. Every value written before the quit reads back in the relaunched process

`osascript -e "ignoring application responses" -e 'tell application id "com.mcneel.rhinoceros.9" to quit' -e "end ignoring"` quits Rhino without a listener call.

## [08]-[GRASSHOPPER_2]

- `Editor.Instance` is `None` until Grasshopper 2 starts, `g2_start` starts it and loads a registered Grasshopper 2 plugin Rhino has not loaded
- `PlugIn.LoadPlugIn(PlugIn.IdFromName("Grasshopper2"))` enables `import Grasshopper2` and loads no component library, `g2_start` loads them
- `g2_start` reopens every canvas `~/Library/Application Support/Grasshopper2/Session.ghsession` names
- `Editor.Instance.Close()` hides the editor and makes its canvas inactive, `Visible = False` hides it with its preview and solves still running
- `~/Library/Application Support/Grasshopper2/Diagnostics/*.ghlog` logs session restores, plugin load failures, and solve faults
- Opening Grasshopper 2's application menu, by click or by a System Events read, posts to DeepL and writes translation caches
- `Editor.Instance.Menu` reads the menu as Eto objects without opening it
