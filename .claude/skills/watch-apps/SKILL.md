---
name: watch-apps
description: "Use when opening, driving, observing, or quitting a desktop app, covering background control through Accessibility and computer-use"
---

# [WATCH_APPS]

## [01]-[SEQUENCE]

Run commands from a terminal or harness with Accessibility access, each prints the states its action reaches:
1. Open the app with `launch` and read its `pid` from the `launched` line
2. Act on each last line by the state table, on the `pid` of the instance work drives
3. Run `read` for element ids a line leaves out, then `perform` or `set` on one id
4. While a call outside `watch-apps` runs (Atelier script), run `watch` on the app and its dialog helpers in a Monitor

```bash
watch-apps find <app-or-pid>...
watch-apps processes <app-or-pid-or-executable>... [--reports <count>]
watch-apps launch [<app>...] [--new] [--wait] [--document <path>...] [--environment NAME=VALUE...] [--line-length <characters>] [--arguments <argument>...]
watch-apps quit <app-or-pid>... [--force]
```

```text
Monitor {"command": "watch-apps watch <app> [<helper>]... --line-length 500",
         "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

- Monitors watch Adobe apps with `com.adobe.crashreporter`, restart with their command on an expiry notice, and stop with TaskStop when work ends
- Codex sessions run `read` on the app after such a call in place of a Monitor
- Pass a bundle id, a path, or a `url` from a `find` line or a `fail` line's `candidates`
- `find`, `watch`, and `launch` resolve a name as a `file:` URL, a path (a name with `/`), a process ID, a bundle id, a file name, then a display name
- Names resolve whole, ignoring case, diacritics, and a file name's `.app`
- Process IDs read as `<pid>`, or as a line's `instance` (`<pid>@<start>`), which names one process and never a later process reusing its pid, `key` taking `<pid>` alone
- `find`, `watch`, `launch`, and `processes` resolve the process ID of a helper executing inside an app bundle to that app
- `find` prints `{"<name>": [<installation>...]}` per name, launching nothing and needing no Accessibility grant
- Installations hold `url`, `executable`, `identifier`, `name`, `version`, `build`, `scriptable`, `scriptingDefinitionFile`, and `activationPolicy` (`prohibited` for `LSBackgroundOnly`, `accessory` for `LSUIElement`, else `regular`)
- `processes` prints `{"<name>": {"agents": [...], "processes": [...], "reports": [...]}}` per name, launching nothing and needing no Accessibility grant
- `processes` takes names as `find` does, an executable path, a process ID, or an `instance`, an exited one included
- `agents` hold each `Contents/Library/LaunchAgents` plist of the named apps with `label`, `program`, `plist`, and `keepAlive`
- `processes` hold each process executing a file in the named bundles with `pid`, `instance`, `executable`, and `replaced`
- `replaced` reads true for a process running other code than the file at `executable` holds, as after an install over a running app
- `reports` hold the newest crash reports of the named instance, bundle id, or agent label, `--reports` many per name (3)
- Reports name executables under `/Users` and `/private/tmp` redacted, so an executable path there matches none, pass its `instance`
- Agents with `keepAlive` and no process at `program` are not running: not registered, not approved, or failed to start with its cause in `reports`
- Names resolving to no app print an `unnamed` `fail` line, `candidates` holding the `url` of each app with a display name containing it
- `watch` follows every installation its names resolve to and exits when no name resolves
- `quit`, `read`, and `capture` take a line's process ID, or resolve a name as `find` does and act on every instance running its installations
- `quit`, `read`, and `capture` print `{"<name>": {}}` when the named app runs no instance
- `launch` opens the one installation a name resolves to, LaunchServices choosing among installations sharing one bundle id as `open -b` does
- `launch` prints an `unnamed` `fail` line with each installation's `url` in `candidates` for a name resolving to more than one bundle id
- One instance per app: `launch` reuses the instance running at the app's path unless `--new` is passed
- Apps are never hidden: `launch` withholds activation, and new instances open with no state restored, no recent item added, and no prompt
- `launch` prints `{"<bundle>":{"<pid>":{"instance":"<pid>@<start>","launched":{"created":<bool>}}}}` first, false for a reused instance
- `launched` holds `replaced` true for a reused instance running code its path no longer holds, `quit` it or pass `--new` for the installed code
- `launch` prints lines until `status` settles or a row asks, or until exit with `--wait`, then exits leaving the app running
- `launch --wait` returns the app's exit status, or 128 plus its signal
- `launch --wait` quits an instance it created when the run ends first (signal, reader closing, starting process exiting), and exits with its status
- Runs ending during startup wait for the launch request to return before quitting
- Early-end quits save each document with a file, autosaving apps keep untitled ones, and only an untitled document without autosave takes a kill
- `quit` prints lines until the instance exits, a row asks (save sheet, left for an agent), it stops answering, or a read fails
- `quit` stops printing when its quit event goes unanswered past the Apple Event default timeout while reads answer, and exits 1 unless each instance exited
- `quit --force` kills the instance, for one no answer or quit frees, and the killed app asks about its previous session at the next launch
- `watch-apps` quits, kills, closes a document, presses a button, or writes a preference only on an agent's command or early end of the creating `launch --wait` run
- `--document` opens each file in the one named app, or with no app named in the file's default app, an unreachable file printing a `fail` line
- Without `--document`, AppKit apps open an untitled document or their open panel, Adobe apps their Home screen with no document
- `--arguments` takes every input after it, and its values and `--environment` values reach only an instance `launch` creates, launchd's environment fills the rest
- App switches pass after `--arguments`, `launch` adding none of its own
- `launch` captures neither the app's output nor error stream
- Hooks rewrite `open`, `xed`, a launching `osascript`, and a bundle's executable into `launch`, a headless switch and a background-only or `LSUIElement` bundle running as written
- Hooks rewrite `killall` and `pkill` into `kill` of their pids when each descends from the session, and refuse any other kill of every match
- Hooks add Blender `--no-window-focus` (absent a headless switch) and Rhino `-nosplash` to `launch` and to each rewrite into it
- Hooks run `watch` in a Monitor alone with `--line-length 500`, drop a filter piped after it, and refuse a loop polling `read` or `watch`
- Hooks refuse `osascript` posting through System Events, taking focus
- Claude Code sends dialogs, hangs, and crashes of apps a session launches, reuses, or drives by Rhino or Blender MCP to its agent as hook context
- Claude Code quits each instance an agent created (`created: true`) when that agent's turn ends and the session's when it ends, `--force` staying an agent's command

Every command exits 1 after printing a `fail` line and 0 otherwise, and ends when the process that started it exits, its reader closes, or SIGTERM, SIGINT, or SIGHUP arrives. Malformed arguments and a missing Accessibility grant print a `fail` line keyed by `watch-apps`.

## [02]-[ACTIONS]

Runs other than `watch` act once and exit:

```bash
watch-apps read <app-or-pid>... [--window <number>]
watch-apps capture <app-or-pid>... [--window <number>] [--longest-side <pixels>]
watch-apps perform <id> <action>
watch-apps set <id> -- '<text>'
watch-apps key <pid> <code>
watch-apps log [<run pid>]
watch-apps serve
```

- `read` prints the whole state with element ids, `--window` walks one row, so answering a dialog costs a fraction of a whole `read`
- `capture` writes one PNG per presented window with no activation to `$TMPDIR/watch-apps/<number>.png`, printing one `captured` line per file
- `capture` reads every window with a window-server number (inactive, covered, minimized, on another unmanaged Space) and needs the Screen Recording grant
- `perform` runs one of the element's `actions` through `AXUIElementPerformAction`, `AXPress` presses a button or a menu item
- `AXRaise` is listed in `actions` as the host states it and refused, since it takes focus
- `set` writes text to an element's settable `AXValue`, every other element prints a `fail` line
- `key` posts one virtual key to a process holding a `blocking` row, frontmost app and cursor unchanged
- Only a `blocking` row leaving proves a key answered, busy hosts run the key when their loop resumes, and no answer leaves the row for a person
- `perform`, `set`, and `key` print the next state of the process once the acted window leaves, bounded at 2 heartbeats
- `set` bypasses the app's change tracking: a document stays unedited, autosave can write the text, and closing asks nothing
- Ids name one element of one presentation, absent once its window, sheet, or dialog closes or shows again, or the app replaces the element
- Menu item ids name a path of positions from the menu bar, rerun `read` after the app adds or removes an item on that path
- Menu items of a background app take `AXPress` with no Automation grant
- Menu items read `enabled` as the app last validated them, every item false under a modal dialog shown while launching or in Acrobat
- Failed actions print a `fail` line keyed by `id` with `reason` `unresolved`, `disabled`, `refused`, or `unanswered`
- Refused and unanswered actions can have run, `read` the instance before retrying one
- Controls whose action opens a modal dialog or blocks answer once the action returns, menu items answer before their action runs
- `log` prints one line per run sharing the caller's `WATCH_APPS_SESSION` with its `pid`, `at`, `argv`, `exit`, and last line per key, a run pid every record of that run
- Each run writes `{at, pid, argv}`, `{at, pid, line}`, and `{at, pid, exit}` to its own file `watch-apps/<session>.<pid>.jsonl` in `$WATCH_APPS_LOG_DIR` (`.artifacts/logs` here), else `~/Library/Logs/app.rasm.watch-apps`, a run with no session as `<pid>.jsonl`
- Run `exit` holds its exit `status`, and `signal` for a run a signal ended
- Run files past 1 MB keep the run start and its last line per key, and the newest 64 run files stay
- Failures append one OpenTelemetry log record each to `watch-apps.log` in the same folder, with run `trace_id` and `span_id` while traces export
- `watch-apps.log` rotates at 8 MB or 1 day to `.1` through `.7` beside it, the oldest replaced and each one older than 1 day deleted
- `serve` answers the `ApplicationServer` wire over standard input and output, holding its parent's grants

## [03]-[STATES]

Lines read `{"<bundle id>": {"<pid>": {"instance": "<pid>@<start>", <state>}}}`, a field the app leaves unknown is absent:
- `instance` holds the pid with its kernel start time in Mach ticks, one value per process whatever pid reuse follows
- States read `{"running": {<fields>}}`, `{"failed": {<fields>}}`, or `{"exited": {<fields>}}`
- `fail` lines read `{"<key>": {"fail": {"reason": <case>, "subject": <text>, "message": <text>}}}` with facts its reason holds, keyed by name, bundle id, pid, element id, `system`, or `watch-apps`
- `system` lines read `{"system": {"memory": "normal|warning|critical", "thermal": "nominal|fair|serious|critical"}}` from the kernel, printing off `normal` and `nominal` and on return
- Lines print when their key's state changes, `usage`, `frame`, and `onScreen` printing nothing
- `watch` prints one line per changed instance, exited instances showing in the line of their exit and of their crash report, then leaving

`running` outside `read` is an outline, `--line-length` cutting asking text, then options, then windows to fit:
- `status` reads `launching`, `responding`, or `unresponsive`, `process` holding what an `unresponsive` main thread did while silent
- `process` reads `running` after time on a processor (a body, long work), `sleeping` while blocked, `runnable` awaiting a processor, or `stopped`
- `active` is whether the app holds focus, `windows` the numbers of presented rows that ask nothing, `menus` the titles of open menus
- `asking` holds each row requiring an answer with `kind`, `number`, `options` (button labels), `text`, and `title`

`read` gives the whole value, asking rows first:
- `windows` hold `number`, `frame` (`[[x, y], [width, height]]` in screen points), `layer`, `onScreen`, `title` (absent unset or without Screen Recording), `kind`, `asking`, `presented`, and `element`
- `presented` reads false for a window made before the app first responded that no element describes
- `kind` reads `filePanel`, `sheet`, `alert`, `modal`, `system`, `menu`, `window`, `undescribed` (no element, Rhino's drawn dialogs among them), or `blocking`
- `system` rows come from dialog helpers naming the app's display name as a whole word
- Dialog helpers show crash alerts, privacy prompts, downloaded-app and Accessibility warnings, and keychain and authorization prompts
- Apps sharing a display name share `system` rows
- `blocking` states a new window on a blocked main thread with Accessibility silent (InDesign's `CAlert::ModalAlert`), a blocked progress window reading the same
- `blocking` reads from an on-screen window at the modal-panel level, or a new window while the main thread sleeps through a second silent heartbeat
- `unresponsive` alone asks nothing: `running` ends with its body or work, `runnable` answers once a processor frees
- `usage` holds `cpu` seconds, `footprint` bytes, and `threads` on `read` and on turning `unresponsive`, `usageFailure` the cause when the read fails
- `menus` hold the menu bar, the extras menu bar, and open application-level menus

`element` holds `id`, `role`, `subrole`, `modal` (`AXModal`), `identifier` (`AXIdentifier`), `description` (`AXDescription`), `title`, `document` (`AXDocument` file URL), `edited` (close button `AXEdited`), `minimized`, `actions`, `kind`, `asking`, `answers`, and `detail`:
- `answers` holds `accept` (`AXDefaultButton`, `OKButton`), `discard` (`DontSaveButton`), `cancel` (`AXCancelButton`, `CancelButton`, close button), and `buttons` (each labeled button, the choice where an app marks no answer control), absent with nothing to press
- `detail` holds `main`, `focused`, `text` (each static text holding a character beyond whitespace and private-use icon glyphs), `controls`, and `unreadable` (refused reads, with `reads`, `role`, `label`, AXError `error`)
- `controls` hold once each text field, text area, combo box, pop-up button, check box, radio button, and element or labeled menu item with `AXPress`
- Each control holds `id`, `role`, `subrole`, `label`, `identifier`, `value`, `enabled`, `actions`, `attribute`, and `items`
- Walks descend into scroll areas and visible table rows, outline rows, and list children, and reach menus only on explicit walks running the app's `menuWillOpen`
- Alert button `identifier` reads `action-button-<n>` in the order the buttons were added, panel buttons `OKButton` and `CancelButton` on save and open, `PDPrint` and `PSOK` on print and page setup

Instances are read at insertion and after each window, sheet, title, focus, menu, minimize, or destruction notification they post. A heartbeat reads each instance's open and focused windows and window-server rows once per 1-second messaging timeout, and reads the instance again when either changes. A stopped process reads `unresponsive` with no message sent. One silent heartbeat keeps the last status, and the second in a row reads `unresponsive`, `process` holding what the main thread did during it. With no earlier answer (`read`, a new `watch`), the first silent heartbeat reads `unresponsive`. A main thread awaiting a processor (an idle app under App Nap on a loaded machine) keeps its last status, and with no earlier answer reads `runnable` after a second message. `launching` holds until the app finishes launching unless a row asks first or its main thread sleeps with no processor time through a second silent heartbeat in a row, which reads `unresponsive`. `exited` reads `status` for an exit or `signal` for a kill with `detail` naming a `memory`, `codeSigning`, or `decryption` kill and `reporting` while a crash report is due, then a second line adds `report` (`url`, `pid`, `instance`, `executable`, `service` launchd label, `bundleIdentifier`, Mach `exception`, `signal`, and `termination` with `namespace`, `code`, and `indicator`, `CODESIGNING` with `Launch Constraint Violation` for a refused launch) once ReportCrash writes it, or `reportFailure` after 60 seconds without one. `statusFailure` holds the cause of a failed exit status read.

| [INDEX] | [STATE]                                | [ACTION]                                              |
| :-----: | :------------------------------------- | :---------------------------------------------------- |
|  [01]   | `launching`, `unresponsive`            | Wait for the next line                                |
|  [02]   | `responding`, `asking` empty           | `read`, then `perform`, `set`, or `capture --window`  |
|  [03]   | Row in `asking` with `options` or text | `read --window <number>`, then `perform <id> AXPress` |
|  [04]   | Row with `kind` `undescribed`          | `capture --window <number>`, read the image           |
|  [05]   | Row with `kind` `blocking`             | `capture --window <number>`, then `key <pid> <code>`  |
|  [06]   | `unresponsive`, `process` `stopped`    | `kill -CONT <pid>`                                    |
|  [07]   | `unresponsive`, `sleeping`, none asks  | `capture <pid>`, read each image for a dialog         |
|  [08]   | `unresponsive` after quit, no answer   | `quit <pid> --force`                                  |
|  [09]   | `fail` `untrusted`                     | Grant Accessibility access                            |
|  [10]   | `exited` with `report`                 | Read the report at `url` before a launch              |
|  [11]   | `fail` `host` or `noProcess` at launch | `processes <app>`, read `reports` for the cause       |
|  [12]   | `launched` with `replaced`             | `quit <pid>`, then `launch`, or `launch --new`        |

Hold script calls while a `blocking` row stands, calls sent before get no answer or an error (-29). `sample <pid> 1 -file <path>` reads an instance's stacks without touching it.

## [04]-[FOCUS]

Actions go to the app's process through AX actions on its elements and leave the frontmost app unchanged:
- Background windows, dialogs, and sheets take no focus and order below the frontmost app's front window, above other apps' windows
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app

## [05]-[COMPUTER_USE]

`computer-use` presses what no script or Accessibility action reaches, on a machine the owner's message hands over, while `launch`, Atelier, and `read` open and read the app:
1. Leads run `request_access` once by bundle id for every app and flag of the run while the owner answers
2. Agents run `list_granted_applications` before a first action, and report an absent app with the state that needed it
3. `capture --window <number>` reads the window first, and work stops there while the owner uses the machine
4. `computer_batch` clicks and takes its image, `read` confirms the state

- `request_access` serves the lead, it blocks with no timeout until a person answers, and a new flag asks again for a granted app
- `list_granted_applications` serves any agent, grants last for the session and reach every agent in it
- Grants name a bundle id, side-by-side releases sharing one stay apart by `pid` or `instance` in `watch-apps` and by `application` name or `instance` in Atelier
- `screenshot` and `zoom` serve click coordinates alone, `capture` serves reading
- `left_click`, `double_click`, and `computer_batch` serve an alert in `unresponsive` `windows`, a control in a `capture` image, a press that left its state unchanged, and a populated window with an empty `detail`
- `type` and `key` serve input that needs the app's change tracking, after a click on the field, input going to the frontmost granted app
- `left_click_drag` and `scroll` serve a canvas or slider no element exposes
- `open_application` serves raising one running granted app above another, `launch` and Atelier open every app
- `switch_display` serves a target `request_access` lists on another display
- `read_clipboard` serves a value only a copy exposes, and returns the owner's clipboard when the copy misses

- Every action hides each ungranted app, hosts of other agents and system dialogs included, and moves the real cursor
- Hidden apps and the Esc key return when the lead's turn ends
- Host terminals stay above every target and absent from every image, `would land on` naming one means moving the target by script
- Coordinates come from the last `computer-use` image, `frame` values never feed a click
- One agent acts at a time, and no script or other `computer-use` call shares its block
- Products never depend on `computer-use`
- Tools named `app_*`, where a session lists them, act without the screen and replace the display tools above
