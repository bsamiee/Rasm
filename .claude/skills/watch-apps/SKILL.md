---
name: watch-apps
description: "Use when opening or driving a desktop app, covering background control through Accessibility"
---

# [WATCH_APPS]

## [01]-[SEQUENCE]

Start the watcher from a terminal or harness with Accessibility access before an app's first launch or script call, and keep it until work ends:
1. Start a Monitor running `watch` with the app's bundle id or display name
2. Read the start lines: `system` pressure and each app's running instances or `{}` for none
3. Open the app with `launch` under `run_in_background: true`, stopping that task when work ends
4. Act on each line by its state table row, on the `pid` of the instance work drives
5. Run `read` for the control ids a watch line leaves out, then `perform` or `set` on one id
6. Read the effect from the next watch line, `perform` and `set` print nothing on success
7. Restart the Monitor with the same command on its expiry notice

```text
Monitor {"command": "watch-apps watch <app>",
         "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

```bash
watch-apps launch <app> [--new] \
    [--document <path>]... [--arguments <argument>]... [--environment NAME=VALUE]...
```

- Names resolve as a path when starting with `/`, then by bundle id, Spotlight name or id, and containment, ignoring case and diacritics
- Names matching no app print a `fail` line, names matching apps of more than one bundle id a `fail` line listing them, and `watch` exits when no name resolves
- `launch` of a bundle id installed at more than one path prints a `fail` line naming each path, pass one path as the name
- `launch` reuses the instance running at the app's path unless `--new` is passed, and opens a new one hidden, unfocused, and without splash screen
- `launch` prints state lines until the instance leaves `launching`, then exits on a reused one or quits a new one without saving when its starter exits or signals it
- `--document` opens each named file, an unreachable file fails the command before anything opens
- Without `--document`, an AppKit app opens an untitled document with no restored windows and an Adobe app its Home screen, `make document` creating one
- `--arguments` and `--environment` set the new instance's command-line arguments and environment, one value per flag, launchd's environment fills the rest
- `launch` captures neither the app's output nor error stream

Every command exits 1 after printing a `fail` line and 0 otherwise, and ends when the process that started it exits or its reader closes. Malformed arguments and a missing Accessibility grant print a `fail` line keyed by `watch-apps`.

## [02]-[ACTIONS]

Runs other than `watch` act once and exit:

```bash
watch-apps read <app>... [--pid <pid>]
watch-apps capture <app>... [--pid <pid>] [--window <window>] [--longest-side <pixels>]
watch-apps perform <id> <action>
watch-apps set <id> -- '<text>'
```

- `--pid` selects the one instance `read` or `capture` prints and takes one app
- `capture` writes a PNG of the instance's on-screen windows to `$TMPDIR/watch-apps/<bundle id>-<pid>-windows.png`
- `--window` captures one window from a line's `window` field to a file ending in its number, `--longest-side` scales the image down to that many pixels
- `capture` skips hidden and minimized windows and needs the Screen Recording grant
- `perform` runs one of the element's `actions` through `AXUIElementPerformAction`, `AXPress` presses a button or a menu item
- `set` writes text to an element's settable `AXValue`, every other element prints a `fail` line
- `set` bypasses the app's change tracking: a document stays unedited, autosave can write the text, and closing asks nothing
- Ids name one element of one presentation, absent once its window, sheet, or dialog closes or shows again, or the app replaces the element
- Menu item ids name a position in their menu, rerun `read` after the app changes that menu
- Menu items of a background app take `AXPress` with no Automation grant, items a background app disables read `enabled` false
- Modal dialogs shown while launching leave every menu item `enabled` false, modal dialogs a menu item opens leave `enabled` as it read
- Failed runs print a `fail` line keyed by the `id`, its `reason` naming an absent or disabled element, `AXRaise`, or an action refused or left unanswered
- Refused and unanswered actions can have run, read the next state line before retrying one
- Controls whose action opens a modal dialog or blocks answer once the action returns, menu items answer before their action runs
- Watchers read hidden instances and actions reach them

## [03]-[STATES]

Lines read `{"<bundle id>": {"<pid>": <state>}}`, a field the app leaves unknown is absent:
- States read `{"running": {"status": {"<status>": {<fields>}}, "usage": {<fields>}}}`, `{"failed": {<fields>}}`, or `{"exited": {<fields>}}`
- `fail` lines read `{"<key>": {"fail": {"reason": <text>}}}`, keyed by name, bundle id, element id, `system`, or `watch-apps`
- `capture` lines read `{"<bundle id>": {"<pid>": {"file": <path>, "windows": [<window>]}}}`, an instance with no on-screen window printing a `fail` line in place of its entry
- `system` lines read `{"system": {"memory": "normal|warning|critical", "thermal": "nominal|fair|serious|critical"}}`, the kernel memory pressure level and thermal state
- Lines print at start and when a state changes, each showing every running instance of that app
- Exited instances show in the line of their exit and of their crash report, then leave

`watch` adds a line keyed by `com.apple.UserNotificationCenter`, `com.apple.coreservices.uiagent`, `com.apple.accessibility.universalAccessAuthWarn`, or `com.apple.SecurityAgent` once that process shows a dialog requiring an answer with a title or text holding a watched app's display name as a whole word, holding those dialogs alone, and a `{}` line once they close. Crash alerts, privacy prompts, downloaded-app warnings, Accessibility warnings, and keychain and authorization prompts belong to these processes, and apps sharing a display name share their lines.

`responding` lists elements in every watch, `launch`, and `read` line:
- Watch and `launch` lines hold each window as an outline and give each element requiring an answer its `detail`
- `read` lines give every window, sheet, dialog, and the menu bar its `detail`
- `id` names the element for `perform` and `set`, `window` holds its CoreGraphics window number, absent on the menu bar
- `role`, `subrole`, `modal` (`AXModal`), `title`, and `description` (`AXDescription`, `alert` on an alert) name the element
- `identifier` (`AXIdentifier`) names its kind as AppKit sets it (`save-panel`, `open-panel`), shared by every element of that kind
- `document` holds the window's `AXDocument` file URL, `edited` its close button's `AXEdited`, `minimized` its `AXMinimized`
- `requiresAnswer` holds for a sheet, a window with `modal` true, or a system dialog, never for `subrole` `AXDialog` alone, which some hidden and minimized windows read
- `actions` hold the element's own Accessibility actions other than `AXRaise`

`detail` holds what a walk reads:
- `frame` reads `[[x, y], [width, height]]` in screen points, `main` and `focused` the window's flags
- `text` holds each static text with a character other than whitespace or a private-use icon glyph
- `controls` hold each text field, text area, combo box, pop-up button, check box, radio button, and element or labeled menu item offering `AXPress` once
- Walks descend into scroll areas, the visible rows of tables and outlines, and the visible children of lists, a text area's `value` holds its visible text
- Controls hold `id`, `role`, `subrole`, `label`, `identifier`, `value`, `enabled`, `actions`, and `attribute`
- Control `attribute` holds the element attribute naming the control, `AXDefaultButton` answers Return and `AXCancelButton` Escape
- Control `items` hold the controls of each menu bar item's menu except the Apple menu, and of a pop-up button's menu while `AXShowMenu` holds it open
- Control `identifier` reads `action-button-<n>` on an alert in the order its buttons were added, `OKButton` and `CancelButton` on save and open panels, `PDPrint` and `PSOK` on print and page setup
- Save and open panels set neither `AXDefaultButton` nor `AXCancelButton`
- `unreadable` lists Accessibility reads the app refused, each with `reads`, `role`, `label`, and AXError `error`

`usage` holds `cpu` seconds, `footprint` bytes of physical memory, and `threads`, in every `read` line and, as sampled then, in the watch line turning an instance `unresponsive`. Rising `cpu` across `read` runs beside `unresponsive` `running` shows a busy app, flat `cpu` beside `sleeping` an app waiting on something.

Instances are read at insertion and after each window, sheet, title, focus, minimize, or destruction notification they post. A heartbeat reads each instance's open and focused windows in one request once per messaging timeout of 1 second and reads the instance again when either changes, windows a hidden app opens post no notification, and `edited` holds its value from the last full read. A stopped process reads `unresponsive` `stopped` with no message sent, a heartbeat left unanswered reads `unresponsive` with the main thread's run state, and the next answer prints the instance's state again. `launching` holds until the app finishes launching, unless a dialog requiring an answer shows first. `exited` reads `status` for an exit or `signal` for a kill, then a second line adds `report` once ReportCrash writes the crash report for that pid, for a signal with a default action that dumps core or a code signing kill, or `reportFailure` after 60 seconds without one.

| [INDEX] | [STATE]        | [FIELDS]                                 | [MEANING]                             | [ACTION]                         |
| :-----: | :------------- | :--------------------------------------- | :------------------------------------ | :------------------------------- |
|  [01]   | `launching`    | None                                     | Launching, nothing to answer          | Wait for the next line           |
|  [02]   | `responding`   | `elements` with no `requiresAnswer`      | Running with nothing to answer        | Drive the app                    |
|  [03]   | `responding`   | `elements` with `requiresAnswer`         | Dialog or sheet holds the app         | Press one of its buttons first   |
|  [04]   | `unresponsive` | `process` `running`, `sleeping`          | Accessibility request left unanswered | Read `usage` for the cause       |
|  [05]   | `unresponsive` | `process` `stopped`                      | Stopped by a signal                   | Resume it or wait                |
|  [06]   | `failed`       | `reason`                                 | Status read failed                    | Grant access                     |
|  [07]   | `exited`       | `status`, `signal`, `report`, `*Failure` | Instance ended                        | Read the report at its `url`     |
|  [08]   | `fail`         | `reason`                                 | Watcher or command failure            | Grant access or pass a bundle id |

Crash reports hold `url`, `pid`, `bundleIdentifier`, and Mach `exception` type. `statusFailure` and `reportFailure` hold the cause of a failed exit status or crash report read. Interface an app draws itself (a Rhino command prompt, Blender's windows) exposes no Accessibility element, and a capture shows it.

## [04]-[FOCUS]

Actions go to the app's process through AX actions on its elements and leave the frontmost app unchanged:
- Dialogs and sheets of a background app take no focus and order below the frontmost app's front window, above other apps' windows
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app
