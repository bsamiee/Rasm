# [SETUP]

Machine, router, and listener facts behind every slot, with the repair for each failure they produce.

## [01]-[MACHINE]

- Rhino 9 runs from `/Applications/RhinoBETA.app` with bundle id `com.mcneel.rhinoceros.9`, on .NET 10 with CPython 3.13, one process per version
- New documents open in Feet with tolerance 0.001 and one layer `Layer 01`
- `yak` runs from `/Applications/RhinoBETA.app/Contents/Resources/bin` on PATH with the Rhino bundle's own .NET
- `nx run rasm:upgrade --configuration rhino` installs the newest `Rhino-MCP-Platform` build and rewrites the router path in `.mcp.json`

## [02]-[ROUTER]

- `.mcp.json` row `rhino-mcp-platform` runs the router with `--default-version 9`
- Slots sit in `~/Library/Application Support/McNeel/rhino-mcp/state.db`, `RHINO_MCP_HOME` moves that directory
- Subagents of one session share one router, and every slot one of them spawns belongs to that router
- Each call runs on Rhino's UI thread under a 5 minute router limit, Claude Code moves a call past 120 seconds to a background task
- RhinoAI starts a listener for every document Rhino opens or creates, whichever process launched Rhino, and announces it in `listeners/`
- Scans (`list_slots`, a call without `slot`, a router start in any session) drop rows with a `<pid>-<port>.gone` file in `listeners/`
- Scans adopt every listener announced there with no row, listeners announce every 15 seconds, and a dropped live one returns adopted
- Departure files of live listeners come from a `spawn_slot` that launched Rhino and from a new document on a closed document's port
- Rows also drop when the router that adopted them exits or a newer router build starts, and return adopted with the same `pid` and `port`
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

## [04]-[RHINO_IN_FRONT]

Keystrokes to a prompt, a document window becoming main, and a document close need Rhino as the frontmost application, and focus returns to the user's application within a second, so one `osascript` holds Rhino in front around the call, sent through the listener endpoint:

```bash
# Record the user's application, hold Rhino in front around one listener call, return focus
osascript -e 'tell application "System Events" to get name of first application process whose frontmost is true'
osascript -e 'tell application "System Events" to set frontmost of process "Rhinoceros" to true' \
    -e 'do shell script "curl -s -X POST <endpoint>/ -H \"Content-Type: application/json\" --data-binary @<call>.json > <out>.json"' \
    -e 'tell application "System Events" to set frontmost of process "<recorded application>" to true'
```

- Commands waiting at a prompt hold every later command and typed writer in that Rhino, `describe(doc).prompt` names the prompt
- `Rhino.RhinoApp.SendKeystrokes("!", True)` in the call cancels a waiting command, `SendKeystrokes("", True)` accepts its defaults
- `describe(doc).prompt` is `None` on the call after the keystroke
- `close(doc)`, and `save(doc)` or `command` on an inactive document, run as the call, the window becomes main inside it
- `close(doc)` closes the window inside the call and makes another open document active, the next `list_slots` no longer lists the slot
- Closes by other means leave Rhino in the background with no active document
- `RhinoEtoApp.MainWindowForDocument(doc).ControlObject.MakeKeyAndOrderFront(None)` in a call held in front makes `doc` active again
- With no active document, the next draw of an object outside every document (a material swatch, a file preview) spins Rhino at 100% until killed
- Rhino in front takes the user's keystrokes into its command line, a typed letter starts a command and Enter repeats the last one

## [05]-[DIALOGS]

Modal dialogs (a file Rhino cannot read, a script exception, export settings) hold every command, typed writer, and `open -g` file in the process, `run_python` still runs inside their loop:
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

## [06]-[HUNG_OR_CRASHED_PROCESS]

- `ps -o stat,%cpu -p <pid>` at a steady 100% while `tools/list` answers and every `tools/call` times out marks a hung UI thread
- `sample <pid> 1` names the frame a hung UI thread spins in, crash reports land in `~/Library/Logs/DiagnosticReports/Rhinoceros-*.ips`
- Hung or killed processes take every slot, `list_slots` after a restart names the survivors under new animal names
- Crashed Rhino processes start again within seconds on their own, the first document answers as an adopted row
- Unsaved edits die with the process, titled files keep what macOS autosaved in place, `file3dm.py` shows what the file holds

## [07]-[GRASSHOPPER_2]

- `Editor.Instance` is `None` until Grasshopper 2 starts, `g2_start` starts it and loads a registered Grasshopper 2 plugin Rhino has not loaded
- `PlugIn.PlugInExists(PlugIn.IdFromName("Grasshopper2"))` reads `(registered, loaded, protected)`
- Grasshopper 2 restores its last canvas from autosave when Rhino restarts
- `Editor.Instance.Documents.Push(Document.NewActiveDocument(), None)` opens an empty canvas and leaves a restored one untouched
- `Editor.Instance.Documents.Pop(<document>, True)` closes that canvas and makes the earlier one current again
