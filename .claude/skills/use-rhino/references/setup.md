# [SETUP]

Sequences reach one document of the Rhino process every session shares, run scripts in it, read their results, and recover from an exit.

## [01]-[SHARED_PROCESS]

Each call leaves screen, front application, and other sessions' documents as it found them:
- Calls send no pointer or key event, and Rhino stays behind the user's application
- `RhinoDoc.ActiveDoc` names whichever document any session last activated, scripts bind their document by serial
- Documents become active only while the user holds Rhino in front, saves, closes, and prompt releases run behind
- Temporary changes (color, setting, selection, `ViewportInfo(viewport)` camera, display mode) revert in their call's `finally`
- Trial table rows (hatch patterns, linetypes) go in a `RhinoDoc.CreateHeadless(None)` the same call disposes
- Untitled documents of other sessions stay open and inactive, closing one destroys its session's work
- Grasshopper 2 definitions build in task documents, and the user's current canvas stays as it was
- Tasks close documents and canvases they opened and remove packages and files they added, files the task produces stay

## [02]-[NEW_TARGET]

Tasks without a named document work in a template copy, one sequence whether Rhino runs or not:
1. `cp ~/"Library/Application Support/McNeel/Rhinoceros/Template Files/<template>.3dm" <task folder>/<name>.3dm`, `Default` feet, `Metric` millimeters
2. `open -g -b com.mcneel.rhinoceros.9 <copy>` opens it behind the user's application as the active document with its own listener
3. `documents()` in any `list_slots` slot lists the copy by `path` with its `serial` and `port`, the working document's record
4. `document.close(doc)` from another slot closes the copy at task end, then `rm <copy>`

- `open -g` launches Rhino when none runs, `AutoLoadMCP` True starts a listener for each document
- `spawn_slot {"version": "9"}` gives an untitled document in place of a copy, `close_slot` closes it
- Rhinos `spawn_slot` launched die with their session's router, user documents opened into them included

Pids `pgrep -x Rhinoceros` lists with no `list_slots` row hold no listener this router reads:
1. `lsof -nP -a -p <pid> -iTCP -sTCP:LISTEN` listing ports marks a listener of another RhinoAI build, the quit and relaunch sequence loads the router's build
2. No listening port marks a Rhino no listener started in, `rhinocode list --json` names its `pipeId`
3. `rhinocode --rhino <pipeId> script <file>.py` runs `RhinoApp.RunScript("_MCPStart _Enter", False)` in it
4. `list_slots` adopts the active document's listener, and every document Rhino opens after it starts one

## [03]-[EXISTING_TARGET]

Tasks naming an open document find it in `documents()` from any slot:
1. Titled documents match by `path`, the working untitled document is the `active` row while the user holds Rhino in front
2. Files the task names that no document holds read first through `file3dm.py`, and a file it answers with `Faults` stays unopened
3. Other named files open through `open -g`, each as its own document
4. `describe(doc)` fields decide the next step:

| [INDEX] | [FIELD]                   | [DECIDES]                                                                                          |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | `units`, `tolerance`      | Every number in a script or macro is in model units                                                |
|  [02]   | `active`                  | `command` refuses an inactive document, `close(doc)` then `open -g` on its file reopen it active   |
|  [03]   | `prompt`                  | Command waiting in the process, released through the prompt sequence before any command            |
|  [04]   | `layers`, `current_layer` | Hierarchy new objects join, adds without attributes take the active document's current layer index |
|  [05]   | `selected`                | Objects the user means by "this" or "these"                                                        |
|  [06]   | `current_view`, `views`   | View the user looks at, names and cameras `capture`, `show`, and `save_view` take                  |
|  [07]   | `page_units`, `style`     | Paper size units, and the annotation style new dimensions take                                     |
|  [08]   | `named_cplanes`           | Construction planes a script restores by name                                                      |
|  [09]   | `materials`               | Physically based materials a `Properties(material=)` names                                         |
|  [10]   | `path`, `modified`        | File `save(doc)` writes, `None` takes a path, `True` marks unsaved edits                           |

## [04]-[SLOTS]

Record the working document's `serial`, its `list_slots` `pid`, and its `documents()` `port`, then pass `slot` on every call:
- `list_slots` runs before each writing step, the row on the recorded `pid` and `port` names the step's slot
- Scripts bind `doc = RhinoDoc.FromRuntimeSerialNumber(<serial>)` in any slot, `None` means the document closed
- Slot names and `adopted` change while a document stays open, and a closed document's port passes to the next document
- Rows sharing a port reach the one document listening on it, `documents()` names its serial
- Documents with `port` `None` answer no slot, a call in another slot reaches them by serial
- Calls without `slot` run in the session's last-used slot, else in the user's first document

## [05]-[SCRIPTS]

`run_python` runs CPython on Rhino's UI thread with `__rhino_doc__` as its slot's document, and the project hook runs every script through `document.run`:
- Scripts `import document`, `records`, and `canvas` fresh from the skill's directory
- Directive comments (`# r:`, `# env:`, `# flag:`) in a script's first 30 lines reach RhinoCode, the hook's `# env:` line holding line 1
- `# r: <package>` in one script installs a package a skill module's `dependencies` names after its import raises `ModuleNotFoundError`
- Edits of one call form one undo step named after the script's first comment line and mark the document modified
- `scriptcontext.doc` and `rhinoscriptsyntax` see the slot's document during each call
- One CPython serves every run in the process, listener calls and Grasshopper 2 components included, and reads no `PYTHON*` variable
- Modules a `# env: <folder>` imports stay in `sys.modules` until a run deletes them or Rhino quits
- `# flag: python.reloadEngine` reloads every loaded module in place for every caller, a reloaded module keeps names its source dropped
- `_NoEcho _-ScriptEditor _Run "<file>"` runs a `.py` file on the shared CPython with no history line

## [06]-[RESULTS]

`run_python` returns one `payload` holding `stdout`, with stderr and a raise's traceback on its `<run_python>` line numbers, and the call succeeds. Other router tools keep the plugin's first content block alone:
- `guidance` alone means the call ran and its result dropped, fix each argument its note names and read state back
- `error` with `message` drops detail blocks, a read-only call repeats through the listener for every block
- `unexpected` naming `TaskCanceledException` is the router's 300 s limit, the call keeps running on Rhino's UI thread and holds every later call
- `rhino_crashed` and `rhino_closed` mean Rhino ended mid-call, the exit read tells a crash from another session's quit
- `close_failed` from `close_slot` with its slot gone from `list_slots` means the close ran
- Calls past 120 s move to a Claude Code background task, a script splits work into calls under it

Listeners answer JSON-RPC at the `list_slots` `endpoint` with every content block, resources the router lacks, and no project hook:

```bash
# Every block of one call, the script file opens with `# env: <skill>/scripts` to import skill modules
jq -n --rawfile script <file>.py '{jsonrpc: "2.0", id: 1, method: "tools/call", params: {name: "run_python", arguments: {script: $script}}}' \
    | curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' --data-binary @- | jq -r '.result.content[].text'
# Third-party plugins Rhino has not loaded, with command names and file types
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"resources/read","params":{"uri":"rhino://host/plugins"}}' | jq -r '.result.contents[0].text'
```

- Direct results hold `stdout` and `stderr` blocks, a raise adds an `error` block first and keeps the stdout printed before it
- Resource `rhino://host/environment` names Rhino, OS, and .NET

## [07]-[PROMPTS_AND_DIALOGS]

Commands waiting at a prompt and modal dialogs hold every command, typed writer, `open -g` file, and quit in the process, and `run_python` runs inside their loop. A `describe(doc).prompt` other than `None` takes one release call from any slot:

```python
# Cancel the waiting command, then wake its event loop
from AppKit import NSApplication, NSEvent, NSEventModifierMask, NSEventType
from CoreGraphics import CGPoint
from Rhino import RhinoApp
from System import Int16, IntPtr

RhinoApp.PostCancelEvent(<serial>)
NSApplication.SharedApplication.PostEvent(NSEvent.OtherEvent(NSEventType.ApplicationDefined, CGPoint(0, 0), NSEventModifierMask(0), 0.0, IntPtr.Zero, None, Int16(0), IntPtr.Zero, IntPtr.Zero), True)
```

- `RhinoApp.PostEnterEvent(<serial>, False)` in place of the cancel accepts the prompt's default
- `describe(doc).prompt` reads `None` on the next call
- `NSApplication.SharedApplication.ModalWindow` names an alert holding the UI thread, its `ContentView` subviews hold its text and each `NSButton` title and tag
- `NSApplication.SharedApplication.StopModalWithCode(IntPtr(<tag>))` answers the alert with its `<tag>` button once the call returns
- Eto dialogs a call opened close through `next(w for w in Eto.Forms.Application.Instance.Windows if w.Title == "<title>").Close()` and the same posted event

## [08]-[EXITS]

Answers of `rhino_crashed` or `rhino_closed`, and `list_slots` rows missing the recorded `pid`, take one exit read:

```bash
# Exit of one pid, (0, 0, 0) a quit and (2, <n>, <n>) a signal, then pid and exception of each recent crash report
log show --last 10m --style compact --predicate 'process == "runningboardd" AND eventMessage CONTAINS "rhinoceros" AND eventMessage CONTAINS ":<pid>] termination"'
fd -t f 'Rhinoceros-' ~/Library/Logs/DiagnosticReports --changed-within 10m -x jq -s -r '.[1] | "\(.pid) \(.exception.type)"'
```

- Quits are another session's, a crash leaves an `.ips` naming the pid, and `log show --predicate 'process == "Rhinoceros" AND eventMessage CONTAINS "Start command"'` lists commands before it
- Edits since the last macOS autosave die with a crashed process, `file3dm.py` shows what a titled file holds
- Untitled documents of a crashed process return at relaunch from `~/Library/Autosave Information/`
- Crashed processes restart within seconds, `list_slots` then names adopted documents anew and a router's own rows under their names
- `ps -o stat,%cpu -p <pid>` at a steady 100% while every `tools/call` times out marks a UI thread spinning, `sample <pid> 1` names its native frame
- Members that can abort the process read one class per call

## [09]-[QUIT_AND_RELAUNCH]

Quitting takes every session's slots, and files Rhino reads at launch take their edits between the quit and the relaunch:
1. `save(doc)` saves each titled document with `modified` reading `True`
2. One call sets `Modified = False` on untitled documents, runs `Unmodify()` on untitled Grasshopper 2 documents, and `PlugIn.FlushSettingsSavedQueue()`
3. `close_slot` closes each row the session's router owns, a Rhino exit brings those rows back through a relaunch
4. `osascript -e 'tell application id "com.mcneel.rhinoceros.9" to quit'` returns before the exit
5. `caffeinate -t <deadline> -w <pid>` in the background returns at the exit, `kill -KILL <pid>` ends a Rhino `ps -p <pid>` still lists
6. Settings XML, `containers.xml`, `default.rui`, Grasshopper 2 font files, and macOS defaults take their edits
7. `open -g -b com.mcneel.rhinoceros.9 <file>...` reopens every saved file once `lsappinfo find bundleid=com.mcneel.rhinoceros.9` prints nothing

`Modified = False` holds from a call that edits nothing, a call's own edit marks the document modified again at its end.

## [10]-[GRASSHOPPER_2]

`g2_start` starts the editor once per process and loads every library, and each `g2_*` tool starts it when `Editor.Instance` is `None`:
- `PlugIn.LoadPlugIn(PlugIn.IdFromName("Grasshopper2"))` enables `import Grasshopper2` without the editor or its libraries
- Starts reopen every canvas `~/Library/Application Support/Grasshopper2/Session.ghsession` names while `ReinstateSession` holds True
- `Editor.Instance.Visible = False` hides the editor with its preview and solves running, `Editor.Instance.Close()` also leaves its canvas inactive
- `~/Library/Application Support/Grasshopper2/Diagnostics/*.ghlog` logs session restores, plugin load failures, and solve faults
