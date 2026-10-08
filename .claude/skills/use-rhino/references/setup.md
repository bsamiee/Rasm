# [SETUP]

Sequences recover a listener, call one directly, release prompts and dialogs, read exits, and relaunch the Rhino process every session shares.

## [01]-[LISTENERS]

Pids `pgrep -x Rhinoceros` lists with no `list_slots` row hold no listener this router reads:
1. `lsof -nP -a -p <pid> -iTCP -sTCP:LISTEN` listing ports marks another RhinoAI build's listener, the relaunch sequence loading the router's build
2. No port with `rhinocode list --json` reading `activeViewport` `null` marks a Rhino at its startup window, `open -g` of a file listening on 10500
3. No port beside a document marks no listener, `rhinocode --rhino <pipeId> script <file>.py` running `RhinoApp.RunScript("_MCPStart _Enter", False)`
4. `list_slots` adopts the new listener, and every document Rhino opens after it starts one

- Rhino starts a listener for each document it opens while `AutoLoadMCP` holds True and `~/.local/bin/claude` exists
- `spawn_slot` answering `startup_timeout` names a slot that can arrive later, `list_slots` preceding any retry
- Rhinos `spawn_slot` launched die with their session's router, user documents opened into them included

Listeners answer JSON-RPC at the `list_slots` `endpoint` with every content block, the resources the router lacks, and no project hook:

```bash
# Every content block of one read-only tool call
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"<tool>","arguments":{<arguments>}}}' | jq -r '.result.content[].text'
# Text of one resource
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"resources/read","params":{"uri":"<uri>"}}' | jq -r '.result.contents[0].text'
```

## [02]-[PROMPTS_AND_DIALOGS]

Commands waiting at a prompt and modal dialogs hold every command, typed writer, `open -g` file, and quit in the process, and `run_python` runs inside their loop. A `describe(doc).prompt` other than `None` takes one release call from any slot:

```python
# Cancel the waiting command, then wake its event loop
from AppKit import NSApplication, NSEvent, NSEventModifierMask, NSEventType
from CoreGraphics import CGPoint
from Rhino import RhinoApp
from System import Int16, IntPtr

serial: int
RhinoApp.PostCancelEvent(serial)
NSApplication.SharedApplication.PostEvent(NSEvent.OtherEvent(NSEventType.ApplicationDefined, CGPoint(0, 0), NSEventModifierMask(0), 0.0, IntPtr.Zero, None, Int16(0), IntPtr.Zero, IntPtr.Zero), True)
```

- `RhinoApp.PostEnterEvent(<serial>, False)` in place of the cancel accepts the prompt's default
- `describe(doc).prompt` reads `None` on the next call
- `NSApplication.SharedApplication.ModalWindow` names an alert holding the UI thread, `StringValue` of its `ContentView` subviews holds its text
- `NSApplication.SharedApplication.StopModalWithCode(IntPtr(<tag>))` answers the alert as the call returns, `NSButton.Tag` an `IntPtr` from 1000
- Eto dialogs close through `next(w for w in Eto.Forms.Application.Instance.Windows if w.Title == "<title>").Close()` and the same posted event

## [03]-[EXITS]

Answers of `rhino_crashed` or `rhino_closed`, and `list_slots` rows missing the recorded `pid`, take one exit read:

```bash
# Exit of one pid, (0, 0, 0) a quit and (2, <n>, <n>) a signal, then pid and exception of each recent crash report
log show --last 10m --style compact --predicate 'process == "runningboardd" AND eventMessage CONTAINS "rhinoceros" AND eventMessage CONTAINS ":<pid>] termination"'
fd -t f 'Rhinoceros-' ~/Library/Logs/DiagnosticReports --changed-within 10m -x jq -s -r '.[1] | "\(.pid) \(.exception.type)"'
```

- Quits are another session's, `close_slot` on the process's last document quitting Rhino, and a crash leaves an `.ips` naming the pid
- `log show --last 10m --predicate 'process == "Rhinoceros" AND eventMessage CONTAINS "Start command"'` lists commands any caller ran
- Edits since the last macOS autosave die with a crashed process, `file3dm.py` shows what a titled file holds
- Untitled documents of a crashed process return at relaunch from `~/Library/Autosave Information/`
- Crashed processes restart within seconds, `list_slots` then names adopted documents anew and a router's own rows under their names
- Crashed processes return under launchd's environment, and quit and relaunch from a shell give Rhino the shell's environment
- `ps -o stat,%cpu -p <pid>` at a steady 100% while every `tools/call` times out marks a UI thread spinning, `sample <pid> 1` names its native frame
- Spinning UI threads take `kill -KILL <pid>` then `open -g -b com.mcneel.rhinoceros.9 <file>...` in place of the quit sequence

## [04]-[QUIT_AND_RELAUNCH]

Quitting takes every session's slots, and files Rhino reads at launch take their edits between the quit and the relaunch:
1. `save(doc)` saves each titled document with `modified` reading `True`
2. One call sets `Modified = False` on untitled documents and runs `Unmodify()` on untitled Grasshopper 2 ones and `PlugIn.FlushSettingsSavedQueue()`
3. `close_slot` closes each row the session's router owns, a Rhino exit brings those rows back through a relaunch
4. `osascript -e 'tell application id "com.mcneel.rhinoceros.9" to quit'` returns before the exit
5. `caffeinate -t <deadline> -w <pid>` in the background returns at the exit, `kill -KILL <pid>` ends a Rhino `ps -p <pid>` lists past the deadline
6. Settings XML, `containers.xml`, `default.rui`, and macOS defaults take their edits
7. `open -g -b com.mcneel.rhinoceros.9 <file>...` reopens every saved file once `lsappinfo find bundleid=com.mcneel.rhinoceros.9` prints nothing

`Modified = False` holds from a call that edits nothing, a call's own edit marks the document modified again at its end.
