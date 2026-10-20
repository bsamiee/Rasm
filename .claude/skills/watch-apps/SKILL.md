---
name: watch-apps
description: "Use when starting or driving a desktop app, covering the shared watcher, answers, menus, windows, launches, exits, and crash reports."
---

# [WATCH_APPS]

`watch-apps`, the executable of the `RasmMacOS` package at `libs/swift/macos` that `nx run libs/swift/macos:build` builds, prints a JSON line per window, dialog, sheet, and menu of named apps as it opens and closes, and per launch and exit. Terminals and harnesses running the watcher need Accessibility access (System Settings > Privacy & Security > Accessibility).

## [01]-[SEQUENCE]

Start the watcher before an app's first launch or script call, and keep it running until work ends:
1. Build the watcher with `nx run libs/swift/macos:build`, once per session and after a change to the package
2. Start a Monitor with the app's bundle id or display name
3. Read the `watch` line, then one `open` line per element open at start, a `fail` line names a missing grant or an unmatched name
4. Start the app with `open -b <bundle id>`, `open -n -b <bundle id>` for an instance of its own
5. Act on each event by its event table row, on the `pid` of the instance work drives
6. Re-arm the Monitor with the same command on its expiry notice

```text
Monitor {"command": "${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/libs/swift/macos/debug/watch-apps <app>", "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

`open` runs each app as a launchd job in a process coalition of its own, outside the caller's.

## [02]-[ANSWERS]

Watchers perform nothing, an answer run waits for an app element with `<text>` in its title or text, performs `<action>` on it, and exits:

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/libs/swift/macos/debug/watch-apps <app> --answer '<text>' --action '<action>'
```

- Elements open at the run's start count, a later one is answered when it opens
- Exit 0 prints an `answered` line with `action`, exit 1 prints a `fail` line with the reason and the element's offered actions
- Both lines join app's log, every watcher reports them, and the holder reports element's `closed` line
- Actions come from the element's `actions`: an enabled control label, `AXCloseButton` for a window close button, `AXCancel` for a menu
- Dialogs block every later script call to their app, and a dialog a call opens holds that call until an answer closes it

## [03]-[SHARING]

One watcher per app holds an `fcntl` lock on `$TMPDIR/<bundle id>.events`, observes the app, and appends events to that log:
- Watchers print elements open at their start, then follow the log
- Watchers exit with the process that started them, releasing the lock
- When the holder exits, a waiting watcher takes the lock, reports elements opened in between, and reports `closed` for ones gone

## [04]-[EVENTS]

Lines carry `app`, instance lines `pid`, element lines add `element` with `id`, `kind`, `role`, `title`, `text` (static text of a dialog or sheet), `controls` (text fields, check boxes, and pop-up buttons with their values), and `actions`:

| [INDEX] | [EVENT]    | [MEANING]                                                         | [ACTION]                                         |
| :-----: | :--------- | :---------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `watch`    | Watcher started                                                   | Read open elements                               |
|  [02]   | `launch`   | Instance started                                                  | Drive app                                        |
|  [03]   | `exit`     | Instance ended, `status` holds exit status                        | List crash reports when `status` is nonzero      |
|  [04]   | `open`     | Window opened, `element.role` holds its subrole                   | Close an unrequested window with `AXCloseButton` |
|  [05]   | `open`     | Menu opened, `element.title` holds its menu bar item              | Cancel an unrequested menu with `AXCancel`       |
|  [06]   | `open`     | Dialog or sheet opened                                            | Answer before the next script call               |
|  [07]   | `answered` | Answer run performed `action`                                     | Continue                                         |
|  [08]   | `closed`   | Element closed or changed, a changed one opens again              | Continue                                         |
|  [09]   | `crash`    | Crash report path, newest first                                   | Read the report                                  |
|  [10]   | `fail`     | Refused notification, lock, log, discovery, argument, or answer   | Grant access, pass a bundle id, or re-answer     |

`--crashes` prints a `crash` line per report under `~/Library/Logs` (macOS `.ips` files and vendor crash logs) with app's bundle id in a header line, and exits:

```bash
${CLAUDE_SKILL_DIR}/../../../.cache/swiftpm/build/libs/swift/macos/debug/watch-apps <app> --crashes
```

## [05]-[ACTIONS]

Actions on an app go to its process alone, through Apple Events or AX actions on its elements. System Events `keystroke` and `key code` post to the frontmost app, whatever process the script names:

```bash
osascript <<'APPLESCRIPT'
tell application "System Events"
    tell (first process whose unix id is <pid>)
        perform action "AXPress" of menu item "<item>" of menu "<menu>" of menu bar item "<menu>" of menu bar 1
    end tell
end tell
APPLESCRIPT
```
