---
name: watch-apps
description: "Use when starting or driving a desktop app, covering the shared watcher, answers, menus, windows, launches, exits, and crash reports."
---

# [WATCH_APPS]

`watch-apps` prints a JSON line per window, dialog, sheet, and menu of named apps as each opens and closes, and per launch and exit.

## [01]-[SEQUENCE]

Start the watcher from a terminal or harness with Accessibility access before an app's first launch or script call, and keep it until work ends:
1. Build the watcher with `nx run watch-apps:build`, once per session and after a change to its sources
2. Start a Monitor running `watch` with the app's bundle id or display name
3. Read the `watch` line, then one `open` line per element open at start, a `fail` line names a missing grant or an unmatched name
4. Start the app with `open -g -b <bundle id>`, `open -g -n -b <bundle id>` for an instance of its own
5. Act on each event by its event table row, on the `pid` of the instance work drives
6. Re-arm the Monitor with the same command on its expiry notice

```text
Monitor {"command": "${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps watch <app>",
         "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

## [02]-[ANSWERS]

Watchers perform nothing, an `answer` run waits for an app element with `<text>` in its title or static text, acts on it, and exits:

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps answer press <app> --text '<text>' --action '<action>'
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps answer set <app> --text '<text>' --field '<label>' --value '<text>'
```

- Elements open at the run's start count, a later one is answered when it opens
- `press` performs an action from the element's `actions`, `set` sets the text of the element's control with that label
- Exit 0 prints an `answered` line with `action`
- Exit 1 prints a `fail` line with `reason`, naming the element's actions, or its control labels for `set`
- Both lines join the app's log, every watcher reports them, and the holder reports the element's `closed` line
- Actions come from `actions`: an enabled control label, a window's `AXCloseButton`, `AXCancelButton`, or `AXDefaultButton`, a menu's `AXCancel`
- Dialogs block every later script call to their app, and a dialog a call opens holds that call until an answer closes it

## [03]-[SHARING]

One watcher per app holds an exclusive lock on `$TMPDIR/<bundle id>.events`, observes the app, and appends events to that log:
- Watchers print elements open at their start, then follow the log
- Watchers exit with the process that started them, releasing the lock
- When the holder exits, a waiting watcher takes the lock, reports elements opened in between, and reports `closed` for ones gone

## [04]-[EVENTS]

Lines read `{"app": <bundle id>, "event": {<event>: {<fields>}}}`, a field the app leaves unknown is absent.

Elements hold:
- `id`, `role`, `subrole`, `title`, and `actions` name the element
- `text` holds the static text of a dialog or sheet
- `controls` hold `role`, `label`, and `value` of each text field, text area, combo box, pop-up button, check box, and radio button
- Check states read as `value` true or false, or 0 off, 1 on, and 2 mixed, as the app reports them
- `unreadable` lists Accessibility reads the app refused, each with `reads`, `role`, `label`, and AXError `error`

| [INDEX] | [EVENT]    | [FIELDS]         | [MEANING]                                    | [ACTION]                                         |
| :-----: | :--------- | :--------------- | :------------------------------------------- | :----------------------------------------------- |
|  [01]   | `watch`    | None             | Watcher started                              | Read open elements                               |
|  [02]   | `launch`   | `pid`            | Instance started                             | Drive app                                        |
|  [03]   | `exit`     | `pid`, `status`  | Instance ended                               | List crash reports when `status` is nonzero      |
|  [04]   | `open`     | `pid`, `element` | Window opened, `subrole` names its kind      | Close an unrequested window with `AXCloseButton` |
|  [05]   | `open`     | `pid`, `element` | Menu opened, `title` names its menu bar item | Cancel an unrequested menu with `AXCancel`       |
|  [06]   | `open`     | `pid`, `element` | Dialog or sheet opened                       | Answer before the next script call               |
|  [07]   | `answered` | `pid`, `action`  | Answer run performed `action`                | Continue                                         |
|  [08]   | `closed`   | `pid`, `element` | Closed or changed, a changed one opens again | Continue                                         |
|  [09]   | `crash`    | `report`         | Crash report path, newest first              | Read the report                                  |
|  [10]   | `fail`     | `pid`, `reason`  | Failure named by `reason`                    | Grant access, pass a bundle id, or re-answer     |

`crashes` prints a `crash` line per regular non-hidden file under `~/Library/Logs` with `"bundleID":"<bundle id>"` in its first 4 KiB:

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/debug/watch-apps crashes <app>
```

- Exit 0 ends a run that read every file
- Exit 1 prints a `fail` line per unreadable file under `~/Library/Logs` in place of every `crash` line

## [05]-[ACTIONS]

Actions go to the app's process through Apple Events or AX actions on its elements, leaving the frontmost app unchanged:
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- `activate`, `AXRaise`, and an `AXFrontmost` write take focus from the frontmost app

```bash
osascript <<'APPLESCRIPT'
tell application "System Events"
    tell (first process whose unix id is <pid>)
        perform action "AXPress" of menu item "<item>" of menu "<menu>" of menu bar item "<menu>" of menu bar 1
    end tell
end tell
APPLESCRIPT
```
