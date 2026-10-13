---
name: watch-apps
description: "Use when starting or driving a desktop app, covering the shared watcher, its flags, dialog answers, menus, windows, launches, exits, and crash reports."
---

# [WATCH_APPS]

`scripts/watch.swift` prints one JSON line per window, dialog, sheet, menu, close, launch, and exit of the named apps, and presses the buttons its flags name. Terminals and harnesses that run it need Accessibility access (System Settings > Privacy & Security > Accessibility).

## [01]-[SEQUENCE]

Start the watcher before an app's first launch or script call, and keep it armed until work ends:
1. Start a Monitor with app bundle id or display name and the answers work expects
2. Read the first `watch` line of each app, a `fail` line names a missing grant or an unmatched name
3. Start the app with `open -b <bundle id>`, `open -n -b <bundle id>` for an instance of its own
4. Act on each event by its row in the event table, on the `pid` of the instance work drives
5. Re-arm the Monitor with the same command on its expiry notice

```text
Monitor {"command": "swift ${CLAUDE_SKILL_DIR}/scripts/watch.swift <app> --dismiss --answer '<text>=<button>'", "description": "<app> dialogs and lifecycle", "timeout_ms": 1800000}
```

`open` runs each app as a launchd job in a process coalition of its own, outside the caller's.

## [02]-[FLAGS]

| [INDEX] | [FLAG]                       | [EFFECT]                                                            |
| :-----: | :--------------------------- | :------------------------------------------------------------------ |
|  [01]   | `<app>`                      | Bundle id or display name, one or more                              |
|  [02]   | `--dismiss`                  | Presses the only button of a dialog                                 |
|  [03]   | `--answer '<text>=<button>'` | Presses `<button>` of an element with `<text>` in its text          |
|  [04]   | `--once`                     | Applies flags to open elements and exits 0 when it pressed a button |

Labels in `buttons` name what `<button>` presses: dialog button titles, `AXCloseButton` for a window or dialog close button, and `AXCancel` for a menu.

## [03]-[SHARING]

One process per app holds an `fcntl` lock on `$TMPDIR/<bundle id>.events`, observes the app, and appends events to that log:
- Later runs for an app follow its log and print elements open at their start
- Holder flags decide answers, a `watch` line with `text` names the holder's flags
- Runs exit with the process that started them, releasing the lock
- When the holder exits, a waiting run takes the lock and prints its own `watch` line
- Each dialog gets one answer across runs

## [04]-[EVENTS]

| [INDEX] | [EVENT]  | [MEANING]                                    | [ACTION]                                             |
| :-----: | :------- | :------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `watch`  | Run started, `text` holds holder flags       | Read flags that answer app dialogs                   |
|  [02]   | `launch` | Instance started                             | Drive app                                            |
|  [03]   | `exit`   | Instance ended, `status` holds exit status   | Read crash report when `status` is nonzero           |
|  [04]   | `window` | Window opened, `role` holds its subrole      | Close unrequested windows with `AXCloseButton`       |
|  [05]   | `menu`   | Menu opened, `text` holds its title          | Cancel unrequested menus with holder flag `AXCancel` |
|  [06]   | `dialog` | Dialog, alert, or sheet with `pressed`       | Continue, policy answered                            |
|  [07]   | `dialog` | Dialog, alert, or sheet without `pressed`    | Answer with `--once` before next call                |
|  [08]   | `closed` | Element closed, `role` and `text` name it    | Continue                                             |
|  [09]   | `fail`   | Refused notification or failed check in text | Grant access, pass a bundle id, or re-arm            |

Dialogs without `pressed` block every later script call to their app. Answer with a line from `text` and a label from `buttons`:

```bash
swift ${CLAUDE_SKILL_DIR}/scripts/watch.swift <app> --once --answer '<line from text>=<label from buttons>'
```

Crash reports under `~/Library/Logs`, macOS `.ips` files and vendor crash logs, name app bundle id in a header line:

```bash
rg -l --sortr modified -F '"bundleID":"<bundle id>"' ~/Library/Logs
```

## [05]-[ACTIONS]

Actions on an app go to its process alone, through Apple Events or AX actions on its elements. System Events `keystroke` and `key code` post to the frontmost app, whatever process the script names:

```bash
osascript -e 'tell application "System Events" to tell (first process whose unix id is <pid>) to perform action "AXPress" of menu item "<item>" of menu "<menu>" of menu bar item "<menu>" of menu bar 1'
```
