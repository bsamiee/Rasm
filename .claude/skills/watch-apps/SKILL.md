---
name: watch-apps
description: "Use when starting or driving a desktop app, covering the shared watcher, launches, answers, menus, windows, activations, hangs, exits, and crash reports."
---

# [WATCH_APPS]

`watch-apps` prints a JSON line per running instance, window, dialog, sheet, and menu of named apps as each opens and closes, and per launch, activation, hang, exit, and crash report.

## [01]-[SEQUENCE]

Start the watcher from a terminal or harness with Accessibility access before an app's first launch or script call, and keep it until work ends:
1. Build the watcher with `nx run watch-apps:build`, once per session and after a change to its sources
2. Start a Monitor running `watch` with the app's bundle id or display name, matched without case or diacritics
3. Read the `watch` line, then one `launch` line per running instance and one `open` line per element open at start, a `fail` line names a missing grant or an unmatched name with near candidates
4. Start the app with `launch`, which never activates it, opens no document window, and exits once it finishes launching
5. Act on each event by its event table row, on the `pid` of the instance work drives
6. Re-arm the Monitor with the same command on its expiry notice

```text
Monitor {"command": "${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps watch <app>",
         "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps launch <app> [--new] [--within <seconds>]
```

- `launch` prints a `launch` line with the `pid` of the instance, a running one unless `--new` opens another
- `--within` bounds the wait for the instance to finish launching, unbounded when absent
- A bundle id installed at several paths prints a `fail` line naming each path

Every command exits 1 after printing a `fail` line and 0 otherwise.

## [02]-[ANSWERS]

Watchers perform nothing, an `answer press` or `answer set` run waits for an app element with `<text>` in its title or static text and acts on it, an `answer menu` run presses a menu bar item at once, and each exits:

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps answer press <app> --text '<text>' --action '<action>' [--under '<label>']... [--pid <pid>]
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps answer set <app> --text '<text>' --field '<label>' --value '<text>' [--pid <pid>]
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps answer menu <app> --menu '<menu>'... --item '<item>' [--pid <pid>]
```

- Elements open at the run's start count, a later one is answered when it opens
- `--pid` limits `press` and `set` to one instance of an app running several, and names the instance `menu` presses among several
- `press` performs an action from the element's `actions`, `set` sets the text of the element's control with that label
- `--under` labels lead from an action the element lists through each child to `--action`, `--menu` labels lead from a menu bar item through each submenu to `--item`, outermost first
- Pressed items carry a label, an enabled state, and `AXPress`, and an item ending in an ellipsis is refused because it opens a dialog
- A run prints an `answered` line with `action` as `press` with `label` and `under` or `set` with `field` and `value`, or a `fail` line with `reason` naming the labels offered where a step missed
- Both lines join the app's log, every watcher reports them, and the holder reports the element's `closed` line
- Actions come from `actions`: an enabled control label, a window's `AXCloseButton`, `AXCancelButton`, or `AXDefaultButton`, a menu's `AXCancel`
- Dialogs block every later script call to their app, and a dialog a call opens holds that call until an answer closes it

## [03]-[SHARING]

One watcher per app holds an exclusive lock on `$TMPDIR/<bundle id>.events`, observes the app, and appends events to that log:
- Watchers print running instances and elements open at their start, then follow the log, printing each change once
- Watchers exit with the process that started them, releasing the lock
- When the holder exits, a waiting watcher takes the lock, reports elements opened in between, `closed` for ones gone, and `exit` without `status` for instances gone

## [04]-[EVENTS]

Lines read `{"<bundle id>": {"<event>": {<fields>}}}`, a field the app leaves unknown is absent.

Elements hold:
- `id`, `role`, `subrole`, `title`, and `actions` name the element
- `text` holds the static text of a dialog or sheet
- `controls` hold `role`, `label`, and `value` of each text field, text area, combo box, pop-up button, check box, and radio button
- Check states read as `value` true or false, or 0 off, 1 on, and 2 mixed, as the app reports them
- `unreadable` lists Accessibility reads the app refused, each with `reads`, `role`, `label`, and AXError `error`

Crash reports hold `url`, `pid`, `bundleIdentifier`, and the Mach `exception` type.

| [INDEX] | [EVENT]        | [FIELDS]                 | [MEANING]                                         | [ACTION]                                         |
| :-----: | :------------- | :----------------------- | :------------------------------------------------ | :----------------------------------------------- |
|  [01]   | `watch`        | None                     | Watcher started                                   | Read running instances and open elements         |
|  [02]   | `launch`       | `pid`                    | Instance running at start, started, or launched   | Drive app                                        |
|  [03]   | `activate`     | `pid`                    | Instance became the active app                    | Report a focus change no request made            |
|  [04]   | `exit`         | `pid`, `status`          | Instance ended                                    | Read the `crash` line that follows a crash       |
|  [05]   | `open`         | `pid`, `element`         | Window opened, `subrole` names its kind           | Close an unrequested window with `AXCloseButton` |
|  [06]   | `open`         | `pid`, `element`         | Menu opened, `title` names its menu bar item      | Cancel an unrequested menu with `AXCancel`       |
|  [07]   | `open`         | `pid`, `element`         | Dialog or sheet opened                            | Answer before the next script call               |
|  [08]   | `unresponsive` | `pid`                    | Instance left an Accessibility request unanswered | Wait for `responding` before the next call       |
|  [09]   | `responding`   | `pid`                    | Unresponsive instance answered again              | Continue                                         |
|  [10]   | `answered`     | `pid`, `action`          | Answer run performed `action`                     | Continue                                         |
|  [11]   | `closed`       | `pid`, `element`         | Closed or changed, a changed one opens again      | Continue                                         |
|  [12]   | `crash`        | `report`                 | Crash report written for a watched instance       | Read the report at its `url`                     |
|  [13]   | `fail`         | `pid`, `file`, `reason`  | Failure named by `reason`, of `file` when one     | Grant access, pass a bundle id, or re-answer     |

`crashes` prints a `crash` line per crash report under `~/Library/Logs` naming the app, newest first, and a `fail` line per unreadable file:

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps crashes <app>
```

## [05]-[ACTIONS]

Actions go to the app's process through Apple Events or AX actions on its elements, leaving the frontmost app unchanged:
- `answer menu` presses a menu item of a background app through Accessibility, with no Automation grant
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app
