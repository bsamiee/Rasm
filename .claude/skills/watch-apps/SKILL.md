---
name: watch-apps
description: "Use when opening, driving, observing, or quitting a desktop app, covering background control through Accessibility"
---

# [WATCH_APPS]

## [01]-[SEQUENCE]

Run commands from a terminal or harness with Accessibility access, each prints the states its action reaches:
1. Open the app with `launch` and read its `pid` from the last line keyed by its bundle id
2. Before `--new`, answer or quit the instance `launch` returned when its `next` reads `answer` or `wait`
3. Act on each last line by the state table row of its `next`, on the `pid` of the instance work drives
4. Run `read` for control ids a line leaves out, then `perform` or `set` on one id
5. While a call outside `watch-apps` runs (Atelier script, harness case), run `watch` on the app and its dialog helpers in a Monitor

```bash
watch-apps launch [<app>...] [--new] \
    [--document <path>]... [--arguments <argument>]... [--environment NAME=VALUE]...
watch-apps quit (<app>... | --pid <pid>) [--force]
```

```text
Monitor {"command": "watch-apps watch <app> [<helper>]...",
         "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

- Monitors watch Adobe apps with `com.adobe.crashreporter`, restart with their command on an expiry notice, and stop with TaskStop when work ends
- Codex sessions run `read` on the app after such a call in place of a Monitor
- `watch` and `launch` resolve names as a path when starting with `/`, then by bundle id, Spotlight name or id, and containment, ignoring case and diacritics
- Unmatched names print a `fail` line, `watch` follows every installation a name matches and exits when no name resolves
- `quit`, `read`, `capture`, and `show` resolve names against running apps by bundle id, display name, or bundle path, then by display name containment, or take `--pid` alone
- `quit`, `read`, and `capture` act on every instance a name matches, and print `{"<name>": {}}` when none runs
- `launch` opens the newest installed release a name matches, equal versions at different paths printing a `fail` line naming each path
- `launch` reuses the instance running at the app's path unless `--new` is passed
- New instances open hidden and unfocused with no state restored, no recent item added, and no prompt
- `launch` prints lines until `next` reads other than `wait` or the instance exits, or a system prompt naming the app shows, then exits leaving it running
- Every `launch` line holds `created`, false for a reused instance, and every `running` state holds `hidden`
- `quit` prints lines until the instance exits, requires an answer (a save sheet), stops answering, or fails a read
- `quit` sends the quit Apple event unless its first line ends the run
- Apps answering the quit event with `-128` print `refused` when the next heartbeat read finds them responding with nothing to answer, else their next state
- `quit --force` kills the instance, for one no answer or quit frees, and the killed app asks about its previous session at the next launch
- `watch-apps` quits, kills, hides, closes a document, presses a button, or writes a preference only on an agent's command
- `--document` opens each file in the one named app, or with no app named in the file's default app, an unreachable file printing a `fail` line
- Without `--document`, AppKit apps open an untitled document or their open panel, Adobe apps their Home screen with no document
- `--arguments` and `--environment` set the new instance's command-line arguments and environment, one value per flag, launchd's environment fills the rest
- Launches take Rhino `-nosplash` and Blender `--no-window-focus` (absent `-b` or `--background`), other app switches pass through `--arguments`
- `launch` captures neither the app's output nor error stream
- PreToolUse hooks rewrite app launch, quit, and kill commands into `watch-apps` forms
- Claude Code quits each instance its session created (`created: true`) when the session ends

Every command exits 1 after printing a `fail` line and 0 otherwise, and ends when the process that started it exits, its reader closes, or SIGTERM, SIGINT, or SIGHUP arrives. Malformed arguments and a missing Accessibility grant print a `fail` line keyed by `watch-apps`.

## [02]-[ACTIONS]

Runs other than `watch` act once and exit:

```bash
watch-apps read (<app>... | --pid <pid>)
watch-apps capture (<app>... | --pid <pid>) [--window <window>] [--longest-side <pixels>]
watch-apps show (<app>... | --pid <pid>)
watch-apps perform <id> <action>
watch-apps set <id> -- '<text>'
watch-apps log [--pid <run pid>]
```

- `capture` writes a PNG of the instance's on-screen windows to `$TMPDIR/watch-apps/<bundle id>-<pid>-windows.png`
- `--window` captures one window by a line's `window` or `windows` `number` value to a file ending in that number
- `--longest-side` scales the image down to that many pixels
- `capture` unhides a hidden instance for the capture and hides it again, skips minimized windows, and needs the Screen Recording grant
- `show` unhides an instance without activating it and leaves it visible, printing a `fail` line only when it stays hidden
- `perform` runs one of the element's `actions` through `AXUIElementPerformAction`, `AXPress` presses a button or a menu item
- `set` writes text to an element's settable `AXValue`, every other element prints a `fail` line
- `perform` and `set` print the next state of the process owning the element once a window the action closes leaves, `unresponsive` for an action starting long work, or its exit
- `set` bypasses the app's change tracking: a document stays unedited, autosave can write the text, and closing asks nothing
- Ids name one element of one presentation, absent once its window, sheet, or dialog closes or shows again, or the app replaces the element
- Menu item ids name a path of positions from the menu bar, rerun `read` after the app adds or removes an item on that path
- Menu items of a background app take `AXPress` with no Automation grant
- Menu items read `enabled` as the app last validated them, every item false under a modal dialog shown while launching or in Acrobat
- Failed actions print a `fail` line keyed by `id` with `reason` `unresolved`, `disabled`, `activating` (`AXRaise`), `refused`, or `unanswered`
- Refused and unanswered actions can have run, `read` the instance before retrying one
- Controls whose action opens a modal dialog or blocks answer once the action returns, menu items answer before their action runs
- Watchers read hidden instances and actions reach them, `launch`, `perform`, `set`, and `quit` hiding again an instance that unhid itself
- `log` prints one line per run of the session with its `pid`, `at`, `argv`, `exit`, and `last` line per key outlined to its `status` case without `usage`, `--pid` every record of one run
- Runs append `{at, pid, session, argv}`, `{at, pid, line}`, and `{at, pid, exit}` to `$WATCH_APPS_LOG` (`.artifacts/logs/watch-apps.log`)
- Run `exit` holds `status`, or `signal` for a run a signal ended

## [03]-[STATES]

Lines read `{"<bundle id>": {"<pid>": <state>}}`, a field the app leaves unknown is absent:
- States read `{"running": {"status": {"<status>": {<fields>}}, <fields>}}`, `{"failed": {<fields>}}`, or `{"exited": {<fields>}}`
- `fail` lines read `{"<key>": {"fail": {"reason": <case>, "subject": <text>, "message": <text>}}}` with facts its reason holds (`candidates`, `error`)
- `fail` lines take a key of name, bundle id, pid, element id, `system`, or `watch-apps`
- `capture` lines read `{"<bundle id>": {"<pid>": {"file": <path>, "windows": [<window>]}}}` or `fail` for an instance with no on-screen window
- `system` lines read `{"system": {"memory": "normal|warning|critical", "thermal": "nominal|fair|serious|critical"}}` from the kernel
- Lines print when their key's state changes
- `watch` lines print at start and show every running instance of their app, other lines show one instance
- Exited instances show in the line of their exit and of their crash report, then leave

`watch` adds a line keyed by `com.apple.UserNotificationCenter`, `com.apple.coreservices.uiagent`, `com.apple.accessibility.universalAccessAuthWarn`, or `com.apple.SecurityAgent` holding each dialog of that process requiring an answer with a watched app's display name as a whole word in its title or text, and a `{}` line once the dialogs close, and `launch` exits on the first such line. Crash alerts, privacy prompts, downloaded-app warnings, Accessibility warnings, and keychain and authorization prompts come from system processes, and apps sharing a display name share their lines.

`responding` lists `elements` and `windows`:
- `read` lines give every window, sheet, dialog, and the menu bar its `detail`
- Other lines hold each window as an outline without the menu bar, and give each element requiring an answer its `detail`
- `id` names the element for `perform` and `set`, `window` holds its CoreGraphics window number, absent on the menu bar
- `role`, `subrole`, `modal` (`AXModal`), `title`, and `description` (`AXDescription`, `alert` on an alert) name the element
- `identifier` (`AXIdentifier`) names its kind as AppKit sets it (`save-panel`, `open-panel`), shared by every element of that kind
- `document` holds the window's `AXDocument` file URL, `edited` its close button's `AXEdited`, `minimized` its `AXMinimized`
- `requiresAnswer` holds for sheets, `modal` windows, and system dialogs, never `AXDialog` subrole alone (hidden and minimized windows can read it)
- `actions` hold the element's own Accessibility actions other than `AXRaise`
- `windows` lists each CoreGraphics window of the instance no element describes: a dialog the app draws as a window (Rhino) or refuses to describe
- Windows with an area hold `number`, `title` (absent unset or without Screen Recording), `frame`, `layer`, `onScreen`, on screen alone outside `read`

`detail` holds what a walk reads:
- `frame` reads `[[x, y], [width, height]]` in screen points, `main` and `focused` the window's flags
- `text` holds each static text with a character other than whitespace or a private-use icon glyph
- `controls` hold each text field, text area, combo box, pop-up button, check box, radio button, and element or labeled menu item with `AXPress` once
- Walks descend into scroll areas, visible rows of tables and outlines, and visible children of lists
- Text area `value` holds visible text
- Controls hold `id`, `role`, `subrole`, `label`, `identifier`, `value`, `enabled`, `actions`, and `attribute`
- Control `attribute` holds the element attribute naming the control, `AXDefaultButton` answers Return and `AXCancelButton` Escape
- Control `items` hold the controls of each menu bar item's menu except the Apple menu, and of a pop-up button's menu while `AXShowMenu` holds it open
- Alert button `identifier` reads `action-button-<n>` in the order the buttons were added
- Panel button `identifier` reads `OKButton` and `CancelButton` on save and open, `PDPrint` and `PSOK` on print and page setup
- Save and open panels set neither `AXDefaultButton` nor `AXCancelButton`
- `unreadable` lists Accessibility reads the app refused, each with `reads`, `role`, `label`, and AXError `error`

`unresponsive` adds `process` (`running`, `sleeping`, `stopped`) to `windows`, and a change between `running` and `sleeping` alone prints no line. Alerts with an event loop that stops answering Accessibility (InDesign's `CAlert::ModalAlert`) read `unresponsive` with the alert among `windows`, and a `capture --window` image shows its text and buttons. Hold script calls until the alert closes, calls sent before get no answer or an error (-29).

`usage` holds `cpu` seconds, `footprint` bytes of physical memory, and `threads` in every `read` and `unresponsive` line, with `usageFailure` holding the cause when the read fails. Rising `cpu` across `read` runs beside `unresponsive` `running` shows a busy app, flat `cpu` beside `sleeping` an app waiting on something. `sample <pid> 1 -file <path>` reads an instance's stacks without touching it.

Instances are read at insertion and after each window, sheet, title, focus, minimize, or destruction notification they post. Hidden apps open windows with no notification. A heartbeat reads each instance's open and focused windows in one request once per 1-second messaging timeout and reads the instance again when either changes, `edited` keeping its value from the last full read. A stopped process reads `unresponsive` `stopped` with no message sent, a heartbeat left unanswered reads `unresponsive` with the main thread's run state, and the next answer prints the instance's state again. `launching` holds until the app finishes launching, unless a dialog requiring an answer shows first. `exited` reads `status` for an exit or `signal` for a kill with `detail` naming a `memory`, `codeSigning`, or `decryption` kill, then a second line adds `report` once ReportCrash writes the crash report for that pid, for a signal with a default action that dumps core or a code signing kill, or `reportFailure` after 60 seconds without one.

`next` reads `{"<case>": {<fields>}}` in each state with an action, and states outside the table hold none:

| [INDEX] | [NEXT]    | [STATE]                                                  | [ACTION]                                         |
| :-----: | :-------- | :------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `wait`    | `launching`, `unresponsive`, or `failed` `unanswered`    | Wait for the next line                           |
|  [02]   | `drive`   | `responding`, no `requiresAnswer` or on-screen `windows` | `read`, then `perform` or `set`                  |
|  [03]   | `answer`  | `responding`, a dialog or sheet with `requiresAnswer`    | Read `text`, `perform <answer field id> AXPress` |
|  [04]   | `capture` | `responding`, no `requiresAnswer`, on-screen `windows`   | `capture --window <number>`, read the image      |
|  [05]   | `resume`  | `unresponsive` `process` `stopped` by a signal           | `kill -CONT <pid>`                               |
|  [06]   | `force`   | `unresponsive` after the quit event, no answer since     | `quit --pid <pid> --force`                       |
|  [07]   | `grant`   | `failed` `untrusted`, Accessibility access missing       | Grant Accessibility access                       |
|  [08]   | `report`  | `exited` with the crash `report` written                 | Read the report at `url` before a launch         |

`answer` holds the element `id` and `accept` (`AXDefaultButton`, `OKButton`), `discard` (`DontSaveButton`), and `cancel` (`AXCancelButton`, `CancelButton`, close button) control ids:
- `buttons` holds each labeled button's `id` and `label`, the choice where an app marks no answer control (Adobe apps)
- `capture` `windows` holds the `number` of each on-screen window no element describes
- Crash reports hold `url`, `pid`, `bundleIdentifier`, and Mach `exception` type
- `statusFailure` holds the cause of a failed exit status read, `reportFailure` the `reason` of a failed crash report read

## [04]-[FOCUS]

Actions go to the app's process through AX actions on its elements and leave the frontmost app unchanged:
- Dialogs and sheets of a background app take no focus and order below the frontmost app's front window, above other apps' windows
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app
