---
name: watch-apps
description: "Use when opening or driving a desktop app, covering background control through Accessibility"
---

# [WATCH_APPS]

`watch-apps` observes and drives desktop apps in the background through Accessibility.

## [01]-[SEQUENCE]

Start the watcher from a terminal or harness with Accessibility access before an app's first launch or script call, and keep it until work ends:
1. Start a Monitor running `watch` with the app's bundle id or display name
2. Read the start lines: `system` pressure, then each app's running instances or `{}` for none
3. Open the app with `launch`, which sends no activation
4. Act on each line by its state table row, on the `pid` of the instance work drives
5. Restart the Monitor with the same command on its expiry notice

```text
Monitor {"command": "watch-apps watch <app>",
         "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

```bash
watch-apps launch <app> [--new] [--hides] \
    [--document <path>]... [--arguments <argument>]... [--environment NAME=VALUE]...
```

- Names resolve by bundle id through Launch Services, then by Spotlight name or id ignoring case and diacritics, then to the one app containing them
- Names matching no app or apps of more than one bundle id print a `fail` line listing candidates, every other name runs, and `watch` exits when none runs
- `launch` prints the instance's state lines until it leaves `launching`, a running instance's unless `--new` opens another
- `--document` opens each reachable file in the app, without one the app opens no document window
- `--arguments` and `--environment` set the new instance's command-line arguments and environment, one value per flag, launchd's environment fills the rest
- `--hides` launches a new instance hidden and hides it again once launched when `--document` shows its windows, a running instance stays as it is
- `--new` on a running app setting `LSMultipleInstancesProhibited` prints a `fail` line and opens nothing
- Apps holding one instance by their own check show an alert in the new instance, which reads `responding` with a `requiresAnswer` element, then `exited`
- `--new` restores the first instance's documents unless `--arguments -ApplePersistenceIgnoreState --arguments YES` follows it
- Apps can activate and unhide themselves once launched or once a press answers a dialog they show while launching
- Apps with an argument that stops their activation take the argument through `--arguments`
- Apps supporting automatic termination can quit a hidden instance that shows no window
- AppleScript `path to application` launches a stopped app hidden, a hidden menu bar app shows no panel until it restarts
- Bundle ids installed at more than one path print a `fail` line naming each path
- Output and error streams of the app are its own, `launch` captures neither

Every command exits 1 after printing a `fail` line and 0 otherwise, exits when the process that started it exits, and prints help with exit 0 for `--help`. Malformed arguments and a missing Accessibility grant print a `fail` line keyed by `watch-apps`.

## [02]-[ACTIONS]

Runs other than `watch` act once and exit:

```bash
watch-apps read <app>... [--pid <pid>]
watch-apps capture <app>... [--pid <pid>] [--window <window>] [--longest-side <pixels>]
watch-apps perform <id> <action>
watch-apps set <id> '<text>'
```

- `read` prints one state line per app, each menu under the menu bar item opening it
- `--pid` selects the one instance `read` or `capture` prints and takes one app
- `capture` writes a PNG of the instance's on-screen windows to `$TMPDIR/watch-apps/<bundle id>-<pid>.png`
- `--window` captures one window from a line's `window` field, `--longest-side` scales the image down to that many pixels
- Hidden and minimized windows have no on-screen window to capture, a capture needs the Screen Recording grant
- `perform` runs one of the element's `actions` through `AXUIElementPerformAction`, `AXPress` presses a button or a menu item
- `set` writes text to the `AXValue` of a control that lists a settable `AXValue`, every other element prints a `fail` line
- `set` bypasses the app's change tracking: a document stays unedited, autosave can write the text, and closing asks nothing
- Ids name one element of one presentation, absent once its window, sheet, or dialog closes or shows again, or the app replaces the element
- Menu item ids name a position in their menu, rerun `read` after the app changes that menu
- Menu items of a background app take `AXPress` with no Automation grant, items a background app disables read `enabled` false
- Apps showing a modal dialog disable every menu item the dialog does not take
- Runs print nothing when the app performed the action, the watcher shows its effect as the next state line
- Runs print a `fail` line keyed by the `id`, its `reason` naming an absent element, `AXRaise`, or an action the app refused
- `AXRaise` brings the app forward and is never sent
- Refused actions can have run
- Actions left unanswered for the messaging timeout print a `fail` line naming them and can have run, read the next state line before retrying one
- Controls whose action opens a modal dialog or blocks answer once the action returns, menu items answer before their action runs
- Dialogs and sheets of a background app order below the frontmost app's front window and above other apps' windows, taking no focus
- Modal dialogs bounce the app's Dock icon while open
- Instances launched with `--hides` stay hidden through alerts, sheets, save panels, and modal panels, and a capture finds no window
- Watchers read hidden instances and actions reach them
- Hiding a running instance lasts until its next modal dialog shows it again

## [03]-[STATES]

Lines read `{"<bundle id>": {"<pid>": <state>}}`, one entry per running instance of the app, a field the app leaves unknown is absent:
- States read `{"running": {"status": {"<status>": {<fields>}}, "usage": {<fields>}}}`, `{"failed": {<fields>}}`, or `{"exited": {<fields>}}`
- `fail` lines read `{"<key>": {"fail": {"reason": <text>}}}`, keyed by name, bundle id, element id, or `watch-apps`
- `capture` lines read `{"<bundle id>": {"<pid>": {"file": <path>, "windows": [<window>]}}}`, an instance with no on-screen window printing a `fail` line in place of its entry
- `system` lines read `{"system": {"memory": "normal|warning|critical", "thermal": "nominal|fair|serious|critical"}}`, the kernel memory pressure level and thermal state
- Lines print at start and when a state changes, each showing every running instance of that app
- Exited instances show in the line of their exit and of their crash report, then leave

`watch` adds a line keyed by `com.apple.UserNotificationCenter`, `com.apple.coreservices.uiagent`, or `com.apple.accessibility.universalAccessAuthWarn` once that process shows a dialog requiring an answer with a title or text holding a watched app's display name as a whole word, holding those dialogs alone, and a `{}` line once they close. Crash alerts, privacy prompts, downloaded-app warnings, and Accessibility warnings belong to these processes, and apps sharing a display name share their lines.

`responding` lists elements in every watch, `launch`, and `read` line:
- Watch and `launch` lines hold each window as an outline and give each element requiring an answer its `detail`
- `read` lines give every window, sheet, dialog, and the menu bar its `detail`
- `id` names the element for `perform` and `set`, `window` holds its CoreGraphics window number, absent on the menu bar
- `role`, `subrole`, `modal` (`AXModal`), `title`, and `description` (`AXDescription`, `alert` on an alert) name the element
- `subrole` reads `AXDialog` on some hidden and minimized windows
- `identifier` (`AXIdentifier`) names its kind as AppKit does, `save-panel`, `open-panel`, and `_NS:87` for an alert, shared by every element of that kind
- `document` holds the window's `AXDocument` file URL, `edited` its close button's `AXEdited`
- `requiresAnswer` holds for a sheet, a window with `modal` true, or a system dialog, and never for a window that only reads `subrole` `AXDialog`
- `actions` hold the element's own Accessibility actions other than `AXRaise`

`detail` holds what a walk reads:
- `frame` reads `[[x, y], [width, height]]` in screen points, `main`, `focused`, and `minimized` the window's flags
- `text` holds its static text with visible characters, whitespace and private-use icon glyphs left out
- `controls` hold each text field, text area, combo box, pop-up button, check box, radio button, and element or labeled menu item offering `AXPress` once
- Walks descend into scroll areas, the visible rows of tables and outlines, and the visible children of lists, a text area's `value` holds its visible text
- Controls hold `id`, `role`, `subrole`, `label`, `identifier`, `value`, `enabled`, `actions`, and `attribute`
- Control `attribute` holds the element attribute naming the control, `AXDefaultButton` answers Return and `AXCancelButton` Escape
- Control `items` hold the controls of the menu the control opens, absent for a control opening none, the Apple menu is absent
- Control `identifier` reads `action-button-<n>` on an alert in the order its buttons were added, `OKButton` and `CancelButton` on save and open panels, `PDPrint` and `PSOK` on print and page setup
- Save and open panels set neither `AXDefaultButton` nor `AXCancelButton`
- `unreadable` lists Accessibility reads the app refused, each with `reads`, `role`, `label`, and AXError `error`

`usage` holds `cpu` seconds, `footprint` bytes of physical memory, and `threads`, in every `read` line and in each line turning an instance `unresponsive`. Rising `cpu` beside `unresponsive` `running` shows a busy app, flat `cpu` beside `sleeping` an app waiting on something.

Instances are read at insertion and after each window, sheet, title, focus, or destruction notification they post. A heartbeat asks each instance one attribute once per messaging timeout of 1 second. A stopped process reads `unresponsive` `stopped` with no message sent, a heartbeat left unanswered reads `unresponsive` with the process status, and the next answer prints the instance's state again. `launching` holds until the app finishes launching, unless a dialog requiring an answer shows first. `exited` reads `status` for an exit or `signal` for a kill, then a second line adds `report` once ReportCrash writes the crash report for that pid, for a signal with a default action that dumps core or a code signing kill, or `reportFailure` after 60 seconds without one.

| [INDEX] | [STATE]        | [FIELDS]                                 | [MEANING]                             | [ACTION]                            |
| :-----: | :------------- | :--------------------------------------- | :------------------------------------ | :---------------------------------- |
|  [01]   | `launching`    | None                                     | Launching, nothing to answer          | Wait for the next line              |
|  [02]   | `responding`   | `elements` with no `requiresAnswer`      | Running with nothing to answer        | Drive the app                       |
|  [03]   | `responding`   | `elements` with `requiresAnswer`         | Dialog or sheet holds the app         | Press one of its buttons first      |
|  [04]   | `unresponsive` | `process` `running`, `sleeping`          | Accessibility request left unanswered | Read `usage` for the cause          |
|  [05]   | `unresponsive` | `process` `stopped`                      | Stopped by a signal or debugger       | Resume it or wait                   |
|  [06]   | `failed`       | `reason`                                 | Status read failed                    | Grant access or restart the watcher |
|  [07]   | `exited`       | `status`, `signal`, `report`, `*Failure` | Instance ended                        | Read the report at its `url`        |
|  [08]   | `fail`         | `reason`                                 | Watcher or command failure            | Grant access or pass a bundle id    |

Crash reports hold `url`, `pid`, `bundleIdentifier`, and Mach `exception` type. `statusFailure` and `reportFailure` hold the cause of a failed exit status or crash report read. Interface an app draws itself (a Rhino command prompt, Blender's windows) exposes no Accessibility element, and a capture shows it.

## [04]-[FOCUS]

Actions go to the app's process through Apple Events or AX actions on its elements and leave the frontmost app unchanged:
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app
