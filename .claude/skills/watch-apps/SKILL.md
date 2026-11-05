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
4. While a call outside `watch-apps` runs (Atelier script, harness case), run `watch` on the app and its dialog helpers in a Monitor

```bash
watch-apps launch [<app>...] [--new] [--wait] \
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
- `quit`, `read`, and `capture` resolve names against running apps by bundle id, display name, or bundle path, then by display name containment, or take `--pid` alone
- `quit`, `read`, and `capture` act on every instance a name matches, and print `{"<name>": {}}` when none runs
- `launch` opens the newest installed release a name matches, equal versions at different paths printing a `fail` line naming each path
- One instance per app: `launch` reuses the instance running at the app's path unless `--new` is passed
- Apps are never hidden: `launch` withholds activation, and new instances open with no state restored, no recent item added, and no prompt
- `launch` prints `{"<bundle>":{"<pid>":{"launched":{"created":<bool>}}}}` first, false for a reused instance
- `launch` prints lines until `status` settles or a row asks, or until exit with `--wait`, then exits leaving the app running
- `launch --wait` returns the app's exit status, or 128 plus its signal
- `quit` prints lines until the instance exits, a row asks (a save sheet), it stops answering, or a read fails, and exits 1 unless each instance exited
- `quit --force` kills the instance, for one no answer or quit frees, and the killed app asks about its previous session at the next launch
- `watch-apps` quits, kills, closes a document, presses a button, or writes a preference only on an agent's command
- `--document` opens each file in the one named app, or with no app named in the file's default app, an unreachable file printing a `fail` line
- Without `--document`, AppKit apps open an untitled document or their open panel, Adobe apps their Home screen with no document
- `--arguments` and `--environment` reach only an instance `launch` creates, one value per flag, launchd's environment fills the rest
- Launches take Rhino `-nosplash` and Blender `--no-window-focus` (absent `-b` or `--background`), other app switches pass through `--arguments`
- `launch` captures neither the app's output nor error stream
- Hooks rewrite `open`, `xed`, a launching `osascript`, and a bundle's executable into `launch`, a headless switch and a background-only or `LSUIElement` bundle running as written
- Hooks rewrite `killall` and `pkill` into `kill` of their pids when each descends from the session, and refuse any other kill of every match
- Hooks run `watch` in a Monitor alone, drop a filter piped after it, and refuse a loop polling `read` or `watch`
- Hooks refuse `osascript` posting through System Events, taking focus, or scripting a `tools/harnesses` host past its launch
- Claude Code watches each app a session launches, reuses, or drives through Rhino or Blender MCP, sending dialogs, hangs, and crashes to its agent
- Claude Code quits each instance an agent created (`created: true`) when that agent's turn ends and the session's when it ends, `--force` staying an agent's command

Every command exits 1 after printing a `fail` line and 0 otherwise, and ends when the process that started it exits, its reader closes, or SIGTERM, SIGINT, or SIGHUP arrives. Malformed arguments and a missing Accessibility grant print a `fail` line keyed by `watch-apps`.

## [02]-[ACTIONS]

Runs other than `watch` act once and exit:

```bash
watch-apps read (<app>... | --pid <pid>) [--window <number>]
watch-apps capture (<app>... | --pid <pid>) [--window <number>] [--longest-side <pixels>]
watch-apps perform <id> <action>
watch-apps set <id> -- '<text>'
watch-apps key --pid <pid> <code>
watch-apps log [--pid <run pid>]
watch-apps serve
```

- `read` prints the whole state with element ids, `--window` walks one row, so answering a dialog costs a fraction of a whole `read`
- `capture` writes one PNG per presented window with no activation to `$TMPDIR/watch-apps/<bundle id>-<pid>-<number>.png`, printing one `captured` line per file
- `capture` reads every window with a window-server number, inactive, covered, minimized, and on another unmanaged Space included, and needs the Screen Recording grant
- `perform` runs one of the element's `actions` through `AXUIElementPerformAction`, `AXPress` presses a button or a menu item
- `AXRaise` is listed in `actions` as the host states it and refused, since it takes focus
- `set` writes text to an element's settable `AXValue`, every other element prints a `fail` line
- `key` posts one virtual key to a process holding a `blocking` row, frontmost and cursor unchanged, and only the row leaving proves an answer: a busy host runs the key when its loop resumes, and no answer leaves the row for a person
- `perform`, `set`, and `key` print the next state of the process once the acted window leaves, bounded at two heartbeats
- `set` bypasses the app's change tracking: a document stays unedited, autosave can write the text, and closing asks nothing
- Ids name one element of one presentation, absent once its window, sheet, or dialog closes or shows again, or the app replaces the element
- Menu item ids name a path of positions from the menu bar, rerun `read` after the app adds or removes an item on that path
- Menu items of a background app take `AXPress` with no Automation grant
- Menu items read `enabled` as the app last validated them, every item false under a modal dialog shown while launching or in Acrobat
- Failed actions print a `fail` line keyed by `id` with `reason` `unresolved`, `disabled`, `refused`, or `unanswered`
- Refused and unanswered actions can have run, `read` the instance before retrying one
- Controls whose action opens a modal dialog or blocks answer once the action returns, menu items answer before their action runs
- `log` prints one line per run of the session with its `pid`, `at`, `argv`, `exit`, and last line per key, `--pid` every record of one run
- Runs append `{at, pid, argv}`, `{at, pid, line}`, and `{at, pid, exit}` to `$WATCH_APPS_LOG` (`.artifacts/logs/watch-apps.log`), run `exit` holding `status`, or `signal` for a run a signal ended
- `serve` answers the `ApplicationServer` wire over standard input and output, holding its parent's grants, for clients of `@rasm/applications`

## [03]-[STATES]

Lines read `{"<bundle id>": {"<pid>": <state>}}`, a field the app leaves unknown is absent:
- States read `{"running": {<fields>}}`, `{"failed": {<fields>}}`, or `{"exited": {<fields>}}`
- `fail` lines read `{"<key>": {"fail": {"reason": <case>, "subject": <text>, "message": <text>}}}` with facts its reason holds, keyed by name, bundle id, pid, element id, `system`, or `watch-apps`
- `system` lines read `{"system": {"memory": "normal|warning|critical", "thermal": "nominal|fair|serious|critical"}}` from the kernel, printing off `normal` and `nominal` and on return
- Lines print when their key's state changes, `process` between running and sleeping, `usage`, `frame`, and `onScreen` printing nothing
- `watch` prints one line per changed instance, exited instances showing in the line of their exit and of their crash report, then leaving

`running` outside `read` is an outline fitted to Monitor's 500 characters:
- `status` reads `launching`, `responding`, or `unresponsive`, `process` reads `stopped` on a stopped process
- `active` is whether the app holds focus, `windows` the numbers of presented rows that ask nothing, `menus` the titles of open menus
- `asking` holds each row requiring an answer with `kind`, `number`, `options` (button labels), `text`, and `title`

`read` gives the whole value, asking rows first:
- `windows` hold `number`, `frame` (`[[x, y], [width, height]]` in screen points), `layer`, `onScreen`, `title` (absent unset or without Screen Recording), `kind`, `asking`, `presented` (false for a window made before the app first responded that no element describes), and `element`
- `kind` reads `filePanel`, `sheet`, `alert`, `modal`, `system`, `menu`, `window`, `undescribed` (no element, Rhino's drawn dialogs among them), or `blocking`
- `system` rows come from dialog helpers (crash alerts, privacy prompts, downloaded-app warnings, Accessibility warnings, keychain and authorization prompts) naming the app's display name as a whole word, and apps sharing a display name share them
- `blocking` states a new window on a blocked main thread with Accessibility silent, InDesign's `CAlert::ModalAlert` among them: a modal session in the window server, or main-thread time flat across two silent beats. A blocked progress window reads the same
- `unresponsive` alone asks nothing: bodies and long work read the same, and rising `usage.cpu` across `read` runs shows a busy app
- `usage` holds `cpu` seconds, `footprint` bytes, and `threads` on `read` and on turning `unresponsive`, `usageFailure` the cause when the read fails
- `menus` hold the menu bar, the extras menu bar, and open application-level menus

`element` holds `id`, `role`, `subrole`, `modal` (`AXModal`), `identifier` (`AXIdentifier`), `description` (`AXDescription`), `title`, `document` (`AXDocument` file URL), `edited` (close button `AXEdited`), `minimized`, `actions`, `kind`, `asking`, `answers`, and `detail`:
- `answers` holds `accept` (`AXDefaultButton`, `OKButton`), `discard` (`DontSaveButton`), `cancel` (`AXCancelButton`, `CancelButton`, close button), and `buttons` (each labeled button, the choice where an app marks no answer control), absent with nothing to press
- `detail` holds `main`, `focused`, `text` (each static text with a character other than whitespace or a private-use icon glyph), `controls`, and `unreadable` (reads the app refused, with `reads`, `role`, `label`, and AXError `error`)
- `controls` hold each text field, text area, combo box, pop-up button, check box, radio button, and element or labeled menu item with `AXPress` once, with `id`, `role`, `subrole`, `label`, `identifier`, `value`, `enabled`, `actions`, `attribute`, and `items`
- Walks descend into scroll areas, visible rows of tables and outlines, and visible children of lists, and reach menus only on an explicit walk, since a walk runs the app's `menuWillOpen`
- Alert button `identifier` reads `action-button-<n>` in the order the buttons were added, panel buttons `OKButton` and `CancelButton` on save and open, `PDPrint` and `PSOK` on print and page setup

Instances are read at insertion and after each window, sheet, title, focus, menu, minimize, or destruction notification they post. A heartbeat reads each instance's open and focused windows and window-server rows once per 1-second messaging timeout, and reads the instance again when either changes. A stopped process reads `unresponsive` with no message sent, `unresponsive` prints from the second silent heartbeat, and `launching` holds until the app finishes launching unless a row asks first. `exited` reads `status` for an exit or `signal` for a kill with `detail` naming a `memory`, `codeSigning`, or `decryption` kill and `reporting` while a crash report is due, then a second line adds `report` (`url`, `pid`, `bundleIdentifier`, Mach `exception`) once ReportCrash writes it, or `reportFailure` after 60 seconds without one. `statusFailure` holds the cause of a failed exit status read.

| [INDEX] | [STATE]                                 | [ACTION]                                                     |
| :-----: | :-------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `launching`, `unresponsive`             | Wait for the next line                                       |
|  [02]   | `responding`, `asking` empty            | `read`, then `perform`, `set`, or `capture --window`         |
|  [03]   | Row in `asking` with `options` or text  | `read --window <number>`, then `perform <id> AXPress`        |
|  [04]   | Row with `kind` `undescribed`           | `capture --window <number>`, read the image                  |
|  [05]   | Row with `kind` `blocking`              | `capture --window <number>`, then `key --pid <pid> <code>`   |
|  [06]   | `unresponsive`, `process` `stopped`     | `kill -CONT <pid>`                                           |
|  [07]   | `unresponsive` after quit, no answer    | `quit --pid <pid> --force`                                   |
|  [08]   | `fail` `untrusted`                      | Grant Accessibility access                                   |
|  [09]   | `exited` with `report`                  | Read the report at `url` before a launch                     |

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
- Grants name a bundle id, side-by-side releases sharing one stay apart only by `pid` in `watch-apps` and Atelier
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
- Harness cases hold no `computer-use`, and products never depend on it
- Tools named `app_*`, where a session lists them, act without the screen and replace the display tools above
