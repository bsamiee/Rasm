---
name: watch-apps
description: "Use when opening or driving a desktop app, covering background control through Accessibility"
---

# [WATCH_APPS]

## [01]-[SEQUENCE]

Start the watcher from a terminal or harness with Accessibility access before an app's first launch or script call, and keep it until work ends:
1. Start a Monitor running `watch` on the app's bundle id or display name and on helpers showing its dialogs (`com.adobe.crashreporter` for Adobe)
2. Read start lines: `system` pressure and each app's running instances or `{}` for none
3. Before opening another instance, answer, quit, or kill one already `unresponsive` or showing `requiresAnswer` by its `pid`
4. Open the app with `launch` and read its `pid` from the last line
5. Act on each line by its state table row, on the `pid` of the instance work drives
6. Run `read` for control ids a watch line leaves out, then `perform` or `set` on one id
7. Read the effect from the next watch line, `perform` and `set` print nothing on success
8. End work through the app's `Quit` menu item (`AXPress` on its `read` id) or script command, then answer save prompts the next lines show
9. Restart the Monitor with the same command on its expiry notice

```text
Monitor {"command": "watch-apps watch <app> [<helper>]...",
         "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

```bash
watch-apps launch <app> [--new] \
    [--document <path>]... [--arguments <argument>]... [--environment NAME=VALUE]...
```

- Names resolve as a path when starting with `/`, then by bundle id, Spotlight name or id, and containment, ignoring case and diacritics
- Unmatched names print a `fail` line, names matching more than one bundle id one listing them, and `watch` exits when no name resolves
- `launch` of a bundle id installed at more than one path prints a `fail` line naming each path, pass one path as the name
- `launch` reuses the instance running at the app's path unless `--new` is passed
- New instances open hidden and unfocused with no state restored, no recent item added, and no prompt
- `launch` prints state lines until the instance leaves `launching` or an element requires an answer, then exits
- `watch-apps` quits, kills, hides, closes a document, presses a button, or writes a preference only on an agent's command
- `--document` opens each named file, an unreachable file fails the command before anything opens
- Without `--document`, AppKit apps open an untitled document and Adobe apps their Home screen until a `make document` script call
- `--arguments` and `--environment` set the new instance's command-line arguments and environment, one value per flag, launchd's environment fills the rest
- App switches (Rhino `-nosplash`, Blender `--no-window-focus`) pass through `--arguments`
- `launch` captures neither the app's output nor error stream

Every command exits 1 after printing a `fail` line and 0 otherwise, and ends when the process that started it exits or its reader closes. Malformed arguments and a missing Accessibility grant print a `fail` line keyed by `watch-apps`.

## [02]-[ACTIONS]

Runs other than `watch` act once and exit:

```bash
watch-apps read <app>... [--pid <pid>]
watch-apps capture <app>... [--pid <pid>] [--window <window>] [--longest-side <pixels>]
watch-apps show <app> --pid <pid>
watch-apps perform <id> <action>
watch-apps set <id> -- '<text>'
```

- `--pid` selects the one instance `read`, `capture`, or `show` acts on and takes one app
- `capture` writes a PNG of the instance's on-screen windows to `$TMPDIR/watch-apps/<bundle id>-<pid>-windows.png`
- `--window` captures one window by a line's `window` or `windows` `number` value to a file ending in that number
- `--longest-side` scales the image down to that many pixels
- `capture` skips hidden and minimized windows and needs the Screen Recording grant
- `show` unhides a launched instance for `capture` without activating it
- `perform` runs one of the element's `actions` through `AXUIElementPerformAction`, `AXPress` presses a button or a menu item
- `set` writes text to an element's settable `AXValue`, every other element prints a `fail` line
- `set` bypasses the app's change tracking: a document stays unedited, autosave can write the text, and closing asks nothing
- Ids name one element of one presentation, absent once its window, sheet, or dialog closes or shows again, or the app replaces the element
- Menu item ids name a position in their menu, rerun `read` after the app changes that menu
- Menu items of a background app take `AXPress` with no Automation grant and read `enabled` false where the app disables them
- Modal dialogs shown while launching leave every menu item `enabled` false, modal dialogs a menu item opens leave `enabled` as it read
- Failed runs print a `fail` line keyed by `id` with a `reason`: absent or disabled element, `AXRaise`, or a refused or unanswered action
- Refused and unanswered actions can have run, read the next state line before retrying one
- Controls whose action opens a modal dialog or blocks answer once the action returns, menu items answer before their action runs
- Watchers read hidden instances and actions reach them

## [03]-[STATES]

Lines read `{"<bundle id>": {"<pid>": <state>}}`, a field the app leaves unknown is absent:
- States read `{"running": {"status": {"<status>": {<fields>}}, "usage": {<fields>}}}`, `{"failed": {<fields>}}`, or `{"exited": {<fields>}}`
- `fail` lines read `{"<key>": {"fail": {"reason": <text>}}}`, keyed by name, bundle id, element id, `system`, or `watch-apps`
- `capture` lines read `{"<bundle id>": {"<pid>": {"file": <path>, "windows": [<window>]}}}` or `fail` for an instance with no on-screen window
- `system` lines read `{"system": {"memory": "normal|warning|critical", "thermal": "nominal|fair|serious|critical"}}` from the kernel
- Lines print at start and when a state changes, each showing every running instance of that app
- Exited instances show in the line of their exit and of their crash report, then leave

`watch` adds a line keyed by `com.apple.UserNotificationCenter`, `com.apple.coreservices.uiagent`, `com.apple.accessibility.universalAccessAuthWarn`, or `com.apple.SecurityAgent` holding each dialog of that process requiring an answer with a watched app's display name as a whole word in its title or text, and a `{}` line once the dialogs close. Crash alerts, privacy prompts, downloaded-app warnings, Accessibility warnings, and keychain and authorization prompts come from system processes, and apps sharing a display name share their lines.

`responding` lists `elements` and `windows` in every watch, `launch`, and `read` line:
- Watch and `launch` lines hold each window as an outline and give each element requiring an answer its `detail`
- `read` lines give every window, sheet, dialog, and the menu bar its `detail`
- `id` names the element for `perform` and `set`, `window` holds its CoreGraphics window number, absent on the menu bar
- `role`, `subrole`, `modal` (`AXModal`), `title`, and `description` (`AXDescription`, `alert` on an alert) name the element
- `identifier` (`AXIdentifier`) names its kind as AppKit sets it (`save-panel`, `open-panel`), shared by every element of that kind
- `document` holds the window's `AXDocument` file URL, `edited` its close button's `AXEdited`, `minimized` its `AXMinimized`
- `requiresAnswer` holds for sheets, `modal` windows, and system dialogs, never `AXDialog` subrole alone (hidden and minimized windows can read it)
- `actions` hold the element's own Accessibility actions other than `AXRaise`
- `windows` lists each CoreGraphics window of the instance no element describes: a dialog the app draws itself (Rhino, Blender) or refuses to describe
- Windows hold `number`, `title` (absent when the app sets none or without the Screen Recording grant), `frame`, `layer`, and `onScreen`

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

`unresponsive` holds `process` and `windows`, every CoreGraphics window of the instance. Alerts with an event loop that stops answering Accessibility (InDesign's `CAlert::ModalAlert`) read `unresponsive` with the alert among `windows`, and a `capture --window` image shows its text and buttons. Hold script calls until the alert closes, calls sent before get no answer or an error (-29).

`usage` holds `cpu` seconds, `footprint` bytes of physical memory, and `threads`, in every `read` line and, as sampled then, in the watch line turning an instance `unresponsive`, with `usageFailure` holding the cause when the read fails. Rising `cpu` across `read` runs beside `unresponsive` `running` shows a busy app, flat `cpu` beside `sleeping` an app waiting on something. `sample <pid> 1 -file <path>` reads an instance's stacks without touching it.

Instances are read at insertion and after each window, sheet, title, focus, minimize, or destruction notification they post. Hidden apps open windows with no notification. A heartbeat reads each instance's open and focused windows in one request once per 1-second messaging timeout and reads the instance again when either changes, `edited` keeping its value from the last full read. A stopped process reads `unresponsive` `stopped` with no message sent, a heartbeat left unanswered reads `unresponsive` with the main thread's run state, and the next answer prints the instance's state again. `launching` holds until the app finishes launching, unless a dialog requiring an answer shows first. `exited` reads `status` for an exit or `signal` for a kill, then a second line adds `report` once ReportCrash writes the crash report for that pid, for a signal with a default action that dumps core or a code signing kill, or `reportFailure` after 60 seconds without one.

| [INDEX] | [STATE]        | [FIELDS]                                 | [MEANING]                        | [ACTION]                                 |
| :-----: | :------------- | :--------------------------------------- | :------------------------------- | :--------------------------------------- |
|  [01]   | `launching`    | None                                     | Launching, nothing to answer     | Wait for the next line                   |
|  [02]   | `responding`   | `elements` with no `requiresAnswer`      | Running, nothing to answer       | Drive the app                            |
|  [03]   | `responding`   | `elements` with `requiresAnswer`         | Dialog or sheet holds the app    | Read its `text`, press one button first  |
|  [04]   | `responding`   | `windows` beside `elements`              | Window Accessibility cannot read | `capture --window` and read the image    |
|  [05]   | `unresponsive` | `process` `running`, `sleeping`          | Accessibility read unanswered    | Read `windows` and `usage` for the cause |
|  [06]   | `unresponsive` | `process` `stopped`                      | Stopped by a signal              | Resume it or wait                        |
|  [07]   | `failed`       | `reason`                                 | Status read failed               | Grant access                             |
|  [08]   | `exited`       | `status`, `signal`, `report`, `*Failure` | Instance ended                   | Read the report at `url` before a launch |
|  [09]   | `fail`         | `reason`                                 | Watcher or command failure       | Grant access or pass a bundle id         |

Crash reports hold `url`, `pid`, `bundleIdentifier`, and Mach `exception` type. `statusFailure` and `reportFailure` hold the cause of a failed exit status or crash report read. Killed apps ask about the previous session at their next launch, kill only an instance no answer or quit frees.

## [04]-[FOCUS]

Actions go to the app's process through AX actions on its elements and leave the frontmost app unchanged:
- Dialogs and sheets of a background app take no focus and order below the frontmost app's front window, above other apps' windows
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app
