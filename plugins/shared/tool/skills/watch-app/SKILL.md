---
name: watch-app
description: "Use for desktop app lifecycle, process monitoring, hangs, and diagnostic recording, with background Accessibility actions when needed"
---

# [TOOL_WATCH_APP]

## [01]-[SEQUENCE]

Host MCP servers and Atelier change document content, `watch-app` launches, observes, answers dialogs, and quits. Each command prints one JSON line per state its action reaches:

```bash
watch-app find <app-or-pid>...
watch-app processes <app-or-pid-or-executable>... [--reports <count>]
watch-app launch [<app>...] [--new] [--wait] [--document <path-or-url>...] [--environment NAME=VALUE...] [--line-length <characters>] [--arguments <argument>...]
watch-app watch <app> [<helper>]... [--record <folder>]
watch-app quit <app-or-pid>... [--force]
```

Driving an app follows one sequence:
1. `find <name>` lists installations, and installations sharing a bundle id (an app and its Beta) take their `url` in every later command
2. `launch <url>` prints the `pid` and installation `url` in its `launched` line, then each last line takes its action from the state table
3. Apps without a document window take File > New, as Start windows and Home screens differ per host: `read <pid>`, `read <pid> --element <File id>`, then `perform <New id> AXPress`
4. Host MCP servers or Atelier change document content, and `perform`, `set`, or `key` answer each dialog
5. `quit <pid>` stops at a save prompt's asking row, then `read --window <number>` and `perform <discard id> AXPress` let the app exit
6. `timeout -s INT <seconds> watch-app watch <pid> > <file>` follows an app still busy once a command's bound ends, as a pipe loses lines on the signal

Names resolve whole, ignoring case, diacritics, and `.app`:
- Order runs `file:` URL, path, process ID, bundle id, file name, then display name
- Bundle ids shared by installations open the one LaunchServices picks, as `open -b` does
- Unresolved names print an `unnamed` `fail` line, `candidates` holding the `url` of each app with a display name containing every word of the name
- Process IDs read as `<pid>` or `instance` (`<pid>@<start>`), which never names a later process reusing the pid, and `key` takes `<pid>` alone
- Helper pids inside an app bundle resolve to that app, and `processes` reads the helper alone
- `quit`, `read`, and `capture` act on every running instance of a name, printing `{"<name>": {}}` with none running

`find` and `processes` launch nothing and need no Accessibility grant:
- `find` prints each installation's `url`, `executable`, `identifier`, `name`, `bundleName`, `version`, `build`, `scriptable`, `scriptingDefinitionFile`, `scriptingDefinitionIsDynamic` (a dictionary only a running instance answers), and `activationPolicy`
- `processes` prints `agents` (bundle launch agents with `label`, `program`, `plist`, `keepAlive`), `processes` (`pid`, `instance`, `executable`, `replaced`), and `reports`
- `reports` hold the newest crash, hang, diag, and spin reports of the instance, bundle executables, or agent labels, `--reports` many (3)
- Exited processes read by `instance` alone
- `replaced` reads true for a process running code its `executable` file no longer holds, as after an install over a running app
- Agents with `keepAlive` and no process at `program` failed to register, gain approval, or start, with the cause in `reports`

`launch` opens an app in the background and exits leaving it running:
- Instances at the app's path are reused unless `--new` is passed, and new ones open with no restored state, recent item, or prompt
- `launched` holds `created`, false for a reused instance, `url` of the running installation, and `replaced`
- Lines print through startup until the app responds, a row asks, or it sleeps after launching, then up to 2 heartbeats for a first window
- `--wait` prints until the app exits and returns its status or 128 plus its signal
- `--wait` runs ending first (signal, reader closing, parent exiting) quit an instance they created, saving each document with a file and discarding the rest
- `--document` opens files or URLs in the named app, or in each default handler when no app is named
- `--arguments` takes every input after it, and its values and `--environment` reach only an instance `launch` creates
- Output and error streams of the app stay uncaptured
- `launch` replaces `open`, `xed`, a launching `osascript`, and running an app's executable, which activate the app or register it among the caller's processes

`quit` sends the quit event and prints lines through busy work until the instance exits, a row asks, its main thread sleeps or stops, or a read fails:
- Exit status reads 1 unless each instance exited
- `quit --force` kills the instance, and the app asks about its previous session at the next launch
- Quits, kills, document closes, button presses, and preference writes run only on an agent's command or a `--wait` early end

`watch` runs until no name resolves:
- Lines print each instance's state at start, `{"<bundle id>": {}}` for none running, then one line per change
- Dialog helpers print under their own bundle id while a prompt names a watched app, then `{"<helper bundle id>": {}}`
- Running `watch` processes restart into a newly installed `watch-app` with their own arguments

Commands end when their starting process exits, their reader closes, or SIGTERM, SIGINT, or SIGHUP arrives, a second signal ending a run still quitting its instance. Each exits 1 after a `fail` line and 0 otherwise.

## [02]-[ACTIONS]

Runs other than `watch` act once and exit:

```bash
watch-app read <app-or-pid>... [--window <number> | --element <id>]
watch-app capture <app-or-pid>... [--window <number> | --element <id>] [--longest-side <pixels>]
watch-app perform <id> <action>
watch-app set <id> -- '<text>'
watch-app key <pid> <code>
watch-app log [<run pid>]
```

- `read` prints the whole state with element ids and the menu bar's top-level items, and `--window` one window at a fraction of the size
- `read --element <id>` prints one element's subtree (a menu, pane, or panel) as `node` with `label`, `value`, `settable`, `frame`, and `children`
- `capture` writes one PNG per presented window to `<window>-<time>.png` in the captures folder with no activation, covered, minimized, and other-Space windows included
- Captures and recordings hold one window without its child windows, so sheets and child editors (Grasshopper 2) take their own number
- `capture --element` crops one element's frame (a pane, panel, or canvas) from its window
- `perform` runs one of the element's `actions`, `AXPress` pressing a button or menu item, and refuses `AXRaise`, which takes focus
- `set` writes `AXValue` in its held type and reads it back, then tries the element's menu or list, `AXIncrement` or `AXDecrement`, then focus and posted keys, failing `unchanged`
- `set <row id> -- true` selects a table or outline row, and a number from 0 to 1 moves a slider or scroll bar
- `set` bypasses change tracking: the document stays unedited, autosave can write the text, and closing asks nothing
- `key` posts one virtual key code (36 Return, 53 Escape) to the process's key window, frontmost app and cursor unchanged
- Only a row leaving proves a key answered, as busy hosts run the key when their loop resumes
- `perform`, `set`, and `key` print the next state once the acted window leaves, a row asks, or a menu item's action changes the state, bounded at 2 heartbeats
- Controls opening a modal dialog answer once the action returns
- Ids name one element of one presentation and go stale once its window closes or shows again
- Menu item ids hold a path of positions from the menu bar, and take `AXPress` in a background app with no Automation grant
- Menu items read `enabled` as the app last validated them, false under a launch-time modal and in Acrobat
- Failed actions print a `fail` line keyed by `id` with `reason` `unresolved`, `disabled`, `refused`, or `unanswered`, and refused or unanswered actions can have run, so `read` precedes a retry
- Grants belong to bundle `app.rasm.watch-app`: Accessibility for all but `find`, `processes`, and `log`, Screen Recording for `capture` and `--record`, event posting for `key` and `set`, Automation per app for `quit`
- `log` prints one line per run of the session with `pid`, `at`, `argv`, `exit`, and the last line per key, and a run pid prints every record of that run

Settings read `WATCH_APP_<KEY>`, then `<key>` of defaults domain `app.rasm.watch-app`, and `watch-app --help` prints each folder:
- `session` names the journal runs share, default the Claude Code, Codex, or Antigravity session id, so `log` reads an agent session's runs
- `output.dir` holds `<application>/captures` and `watch-app/logs`
- `log.level`, `log.filter`, `log.size`, `log.age`, and `log.files` set the failure log `watch-app.log`, and `OTEL_*` variables start OpenTelemetry export

## [03]-[STATES]

Lines read `{"<bundle id>": {"<pid>": {"instance": "<pid>@<start>", <state>}}}`, with fields the app leaves unknown absent:
- States read `running`, `failed`, or `exited`
- `fail` lines read `{"<key>": {"fail": {"reason": <case>, "subject": <text>, "message": <text>}}}` with the facts of their reason, keyed by name, bundle id, pid, element id, `system`, or `watch-app`
- `system` lines print kernel `memory` off `normal` and `thermal` off `nominal`, and again on return

`running` outside `read` is an outline, `--line-length` cutting asking text, then options, then windows:
- `status` reads `launching`, `responding`, or `unresponsive`
- `process` holds what a silent main thread did: `running` on a processor, `sleeping` blocked, `runnable` awaiting a processor, or `stopped`
- `active` is whether the app holds focus, `windows` the presented rows that ask nothing, `menus` the titles of open menus
- `asking` holds each row requiring an answer with `kind`, `number`, `options` (button labels), `text`, and `title`

`read` gives the whole value, asking rows first:
- `windows` hold `number`, `frame` (`[[x, y], [width, height]]` in points), `layer`, `onScreen`, `title` (with Screen Recording), `kind`, `asking`, `presented`, and `element`
- `presented` reads false for a window no element describes while it is off screen, which `capture`, `windows`, and `watch` then leave out
- Panels hiding while their app is inactive (Rhino's Start window) read off screen, and `capture --window` still shows them
- `kind` reads `filePanel`, `sheet`, `alert`, `modal`, `system`, `menu`, `window`, `undescribed` (no element, Rhino's drawn windows among them), or `blocking`
- `system` rows come from dialog helpers (crash, privacy, download, Accessibility, keychain, authorization) naming the app as a whole word
- `blocking` rows are on-screen windows Accessibility leaves silent while the main thread sleeps, as InDesign's `CAlert::ModalAlert` and blocked progress windows
- Modal-panel-level windows read `blocking` on the first silent heartbeat, new windows on the second, and a busy main thread never yields one
- `usage` holds `cpu` seconds, `footprint` bytes, and `threads` on `read` and on turning `unresponsive`

`element` holds `id`, `role`, `subrole`, `modal`, `identifier`, `description`, `title`, `document` (file URL), `edited`, `minimized`, `actions`, `kind`, `asking`, `answers`, and `detail`:
- `answers` holds `accept` (default button), `discard` (`DontSaveButton`), `cancel` (cancel or close button), and `buttons` (each labeled button)
- Alerts mark no discard control, and their `buttons` label names it
- `detail` holds `main`, `focused`, `text`, `controls`, and `unreadable`, each refused read with its AXError
- `controls` hold each text field, text area, combo box, pop-up button, check box, radio button, row, and pressable element once
- Controls hold `id`, `role`, `subrole`, `label`, `identifier`, `value`, `enabled`, `actions`, `attribute`, and `items`, rows holding their selection as `value`
- Walks descend into scroll areas and visible rows, and reach menu items only through `--element`, which runs the app's `menuWillOpen`

Instances read on each window, sheet, title, focus, menu, minimize, or destruction notification, and a heartbeat reads open windows once per 1-second messaging timeout. The second silent heartbeat in a row reads `unresponsive`, the first with no earlier answer. A main thread awaiting a processor keeps its last status. `launching` holds until the app finishes launching, a row asks, or its main thread sleeps through a second silent heartbeat. `exited` holds `status`, or `signal` with `detail` naming a `memory`, `codeSigning`, or `decryption` kill, and `reporting` while a crash report is due. A second line adds `report` (`url`, Mach `exception`, `signal`, and `termination`, `CODESIGNING` `Launch Constraint Violation` for a refused launch) once ReportCrash writes it, or `reportFailure` after 60 seconds.

| [INDEX] | [STATE]                                        | [ACTION]                                                 |
| :-----: | :--------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `launching`, `unresponsive` `running`          | Wait for the next line                                   |
|  [02]   | `responding`, `asking` empty                   | `read`, then `perform`, `set`, or `capture --window`     |
|  [03]   | Row with `options` or `text`                   | `read --window <number>`, then `perform <id> AXPress`    |
|  [04]   | Row with neither, `undescribed`, or `blocking` | `capture --window <number>`, then `key <pid> <code>`     |
|  [05]   | `unresponsive`, `process` `stopped`            | `kill -CONT <pid>`                                       |
|  [06]   | `unresponsive`, `sleeping`, none asks          | `capture <pid>`, read each image for a dialog            |
|  [07]   | `unresponsive` after quit, no answer           | `quit <pid> --force`                                     |
|  [08]   | `fail` `noGrant`                               | Ask the person to allow the `grant` pane `message` names |
|  [09]   | `exited` with `report`                         | Read the report at `url` before a launch                 |
|  [10]   | `fail` `host` or `noProcess` at launch         | `processes <app>`, read `reports` for the cause          |
|  [11]   | `launched` with `replaced`                     | `quit <pid>`, then `launch`, or `launch --new`           |

Script calls sent while a `blocking` row stands get no answer or error -29. `sample <pid> 1 -file <path>` reads an instance's stacks without touching it, `sudo spindump <pid> 2 10 -onlyTarget -onlyBlocked -noBinary -o <out>` the blocked threads of a hung one, and `spindump -i /Library/Logs/DiagnosticReports/<app>_<date>.hang -timeline -noBinary -o <out>` a past hang without `sudo`.

## [04]-[FOCUS]

Actions go to the app's process through AX actions on its elements and leave the frontmost app unchanged:
- Background windows, dialogs, and sheets take no focus and order below the frontmost app's front window, above other apps' windows
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app
- `ioreg -c IOHIDSystem -d 4 -r -k HIDIdleTime | rg -m1 HIDIdleTime` prints nanoseconds since the owner's last key or pointer input

## [05]-[RECORDING]

`watch --record <folder>` run in the background records while it watches, with no activation:
- Standard windows and asking rows record to `<window>-<time>.mov` at up to 10 frames a second, writing a frame when the window changes
- Dialogs, hangs, and exits by signal save the 15 seconds before as one `.mov` per recorded window, with a `.png` of each asking or hung window
- `timeline.jsonl` takes one `{"at": <time>, "<source>": <value>}` entry per event, and `jq -s 'sort_by(.at)'` orders them
- Sources read `line` (a `watch` line), `log` (a unified log event of the instance), `host`, `movie`, `clip`, `frame`, and `fail`
- `movie`, `clip`, and `frame` hold `file`, `pid`, and `window`, an entry's offset in a movie being its `at` minus the `movie` entry's `at`
- `host` holds `file` and `text` of each line a host appends to `<folder>/host/<name>.log` during the watch
- `ffmpeg -ss <offset> -i <movie> -frames:v 1 <png>` writes one frame to read as an image
- `--longest-side <pixels>` scales recorded frames, as a full-motion window at native size writes megabytes a second
- Recorded windows show the capture indicator in place of their traffic lights, a `Window` row holding it joining `windows`
- Movies read once their window closes or the watch ends, `SIGKILL` included, and finalization unanswered for 15 seconds takes a `fail` `unanswered` entry
