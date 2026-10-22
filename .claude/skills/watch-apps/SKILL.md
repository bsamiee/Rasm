---
name: watch-apps
description: "Use when starting or driving a desktop app, covering the watcher, launches, reads, element actions, menus, dialogs, hangs, exits, and crash reports."
---

# [WATCH_APPS]

`watch-apps` observes and drives desktop apps in the background through Accessibility.

## [01]-[SEQUENCE]

Start the watcher from a terminal or harness with Accessibility access before an app's first launch or script call, and keep it until work ends:
1. Build the watcher with `nx run watch-apps:build`, once per session and after a change to its sources
2. Start a Monitor running `watch` with the app's bundle id or display name, matched without case or diacritics
3. Read the start line, the state of every running instance or `{}` for none, a `fail` line names a missing grant or an unmatched name with near candidates
4. Start the app with `launch`, which sends no activation and exits once the app finishes launching
5. Act on each line by its state table row, on the `pid` of the instance work drives
6. Re-arm the Monitor with the same command on its expiry notice

```text
Monitor {"command": "${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps watch <app>",
         "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps launch <app> [--new] [--hides] \
    [--document <path>]... [--arguments <argument>]... [--environment NAME=VALUE]...
```

- `launch` prints the instance's state line, a running instance's unless `--new` opens another
- `--document` opens each file in the app, without one the app opens no document window
- `--arguments` and `--environment` set the new instance's command-line arguments and environment, one value per flag, launchd's environment fills the rest
- Apps can activate and unhide themselves once launched or once a press answers a dialog they show while launching
- Apps with an argument that stops their activation take the argument through `--arguments`
- `--hides` launches a new instance hidden, a running one stays as it is, an `LSUIElement` menu bar app refuses it
- AppleScript `path to application` launches a stopped app hidden, a hidden menu bar app shows no panel until it relaunches
- Bundle ids installed at more than one path print a `fail` line naming each path
- Output and error streams of the app are its own, `launch` captures neither

Every command exits 1 after printing a `fail` line and 0 otherwise.

## [02]-[ACTIONS]

Runs other than `watch` act once and exit:

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps read <app> [--pid <pid>]
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps perform <id> <action>
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps set <id> '<text>'
```

- `read` prints one state line holding every window, sheet, and dialog with its contents, and the menu bar with each menu under the item opening it
- `--pid` picks the one instance `read` prints
- `perform` runs one of the element's `actions` through `AXUIElementPerformAction`, `AXPress` presses a button or a menu item, `set` writes text to the `AXValue` of a control
- Ids from a `read` or watch line name one element of one presentation, absent once its window, sheet, or dialog closes or shows again
- Menu item ids name a position in their menu, read again after the app changes that menu
- Menu items of a background app take `AXPress` with no Automation grant
- Apps showing a modal dialog disable every menu item the dialog does not take
- Runs print nothing when the app performed the action, the watcher shows its effect as the next state line
- Runs print a `fail` line keyed by the `id`, its `reason` naming an absent element, `AXRaise`, or an action the app refused
- `AXRaise` brings the app forward and is never sent
- Refused actions can have run
- Actions left unanswered for the messaging timeout print a `fail` line naming them and can have run, read the next state line before repeating one
- Controls whose action opens a modal dialog or blocks answer once the action returns, menu items answer before their action runs
- Dialogs and sheets of a background app order below the frontmost app's front window and above other apps' windows, taking no focus
- Modal dialogs bounce the app's Dock icon while open
- Instances launched with `--hides` stay hidden through alerts, sheets, save panels, and modal panels, and a capture finds no window
- Watchers read hidden instances and actions reach them
- Hiding a running instance lasts until its next modal dialog shows it again

## [03]-[STATES]

Lines read `{"<bundle id>": {"<pid>": <state>}}`, one entry per running instance of the app, a field the app leaves unknown is absent:
- States read `{"running": {"status": {"<status>": {<fields>}}}}`, `{"failed": {<fields>}}`, or `{"exited": {<fields>}}`
- `fail` lines read `{"<bundle id or id>": {"fail": {"reason": <text>}}}`
- Lines print at start and when an instance's state changes, each showing every running instance of that app
- Exited instances show in the line of their exit and of their crash report, then leave
- Launching instances, document windows, panels that do not hold the app, menus, and changes inside them print nothing

`responding` in a watch or `launch` line lists the elements that require an answer, `[]` when none does, in a `read` line every window, sheet, dialog, and the menu bar, each holding:
- `id` names the element for `perform` and `set`, `window` holds its CoreGraphics window number, absent on the menu bar
- `role`, `subrole`, `modal` (`AXModal`), `title`, and `description` (`AXDescription`, `alert` on an alert) name the element
- `identifier` (`AXIdentifier`) names its kind as AppKit does, `save-panel`, `open-panel`, and `_NS:87` for an alert, shared by every element of that kind
- `requiresAnswer` holds for a sheet, a window with `modal` true, or a system dialog, and never for a window that only reads `subrole` `AXDialog`
- `text` holds its static text, `controls` each text field, text area, combo box, pop-up button, check box, radio button, and enabled element or menu item offering `AXPress` once, with `id`, `role`, `subrole`, `label`, `identifier`, `value`, `actions`, and the element `attribute` naming it: `AXDefaultButton` answers Return, `AXCancelButton` Escape
- Control `items` hold the controls of the menu the control opens, absent for a control opening none
- Control `identifier` reads `action-button-<n>` on an alert in the order its buttons were added, `OKButton` and `CancelButton` on save and open panels, `PDPrint` and `PSOK` on print and page setup
- Save and open panels set neither `AXDefaultButton` nor `AXCancelButton`
- `actions` hold the element's own Accessibility actions other than `AXRaise`
- `unreadable` lists Accessibility reads the app refused, each with `reads`, `role`, `label`, and AXError `error`

Instances are read once they finish launching and after each Accessibility notification they post. `unresponsive` follows a read or notification registration left unanswered for the messaging timeout of 1 second, the instance is then read once per timeout until it answers, and the next state line follows its answer. A hang no notification reaches prints nothing, a stopped process with nothing asking it prints nothing, and a read refused because the process exited prints nothing before its `exited` line. `exited` reads `status` for an exit or `signal` for a kill, then a second line adds `report` once ReportCrash writes the crash report for that pid, for a signal whose default action dumps core or a code signing kill.

| [INDEX] | [STATE]        | [FIELDS]                                 | [MEANING]                             | [ACTION]                            |
| :-----: | :------------- | :--------------------------------------- | :------------------------------------ | :---------------------------------- |
|  [01]   | `responding`   | `elements` `[]`                          | Running with nothing to answer        | Drive the app                       |
|  [02]   | `responding`   | `elements`                               | Dialog or sheet holds the app         | Press one of its buttons first      |
|  [03]   | `unresponsive` | None                                     | Accessibility request left unanswered | Wait for the next line              |
|  [04]   | `failed`       | `reason`                                 | Status read failed                    | Grant access or restart the watcher |
|  [05]   | `exited`       | `status`, `signal`, `report`, `*Failure` | Instance ended                        | Read the report at its `url`        |
|  [06]   | `fail`         | `reason`                                 | Watcher or command failure            | Grant access or pass a bundle id    |

Crash reports hold `url`, `pid`, `bundleIdentifier`, and Mach `exception` type. `statusFailure` and `reportFailure` say why exit status or crash report could not be read.

## [04]-[FOCUS]

Actions go to the app's process through Apple Events or AX actions on its elements, leaving the frontmost app unchanged:
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app
