---
name: watch-apps
description: "Use when opening, driving, observing, or quitting a desktop app, covering background control through Accessibility and the session watch reporting each app change"
---

# [WATCH_APPS]

## [01]-[LINK]

`watch-apps watch` follows each instance a run of the session names, prints one line per change, and quits the instances the session's runs created when it ends:

| [INDEX] | [HARNESS]   | [LINK]                                                                                      |
| :-----: | :---------- | :------------------------------------------------------------------------------------------ |
|  [01]   | Claude Code | Plugin `watch-apps@rasm` runs it as a session monitor, each line arriving as a notification |
|  [02]   | Codex       | Run it once as a background terminal at session start, read its output after each action    |
|  [03]   | Antigravity | Run it once as a background command at session start, read its output after each action     |

- Watch lines replace polling: a `read` or `capture` repeated 3 times within a minute prints a `polled` line
- Apps opened outside `watch-apps` (an MCP tool, a plug-in) join the watch at the first `read` or `capture` naming them
- `watch <app>` follows every instance of a named app, `--record <folder>` records while it watches

## [02]-[COMMANDS]

`watch-apps <command> --help` states each command's options:

| [INDEX] | [COMMAND]                      | [EFFECT]                                                                              |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `find <app>`                   | Installations with `identifier`, `url`, `version`, and `activationPolicy`, no launch  |
|  [02]   | `processes <app>`              | Launch agents, running processes, and newest crash `reports`, no launch               |
|  [03]   | `launch <app>`                 | Opens without activation, reusing a running instance unless `--new`                   |
|  [04]   | `launch <app> --wait`          | Returns the app's exit status, quitting an instance it created when the run ends      |
|  [05]   | `read <pid> [--window <n>]`    | Whole state with element ids, `--window` walking one window                           |
|  [06]   | `capture <pid> [--window <n>]` | One PNG per presented window with no activation, `--element <id>` one element's frame |
|  [07]   | `perform <id> <action>`        | One Accessibility action, `AXPress` pressing a button or menu item                    |
|  [08]   | `set <id> -- '<text>'`         | Writes a settable `AXValue`, leaving the document unedited                            |
|  [09]   | `key <pid> <code>`             | Posts one key (36 Return, 53 Escape) to a process holding a `blocking` window         |
|  [10]   | `quit <pid> [--force]`         | Sends the quit event, `--force` kills an instance no answer frees                     |
|  [11]   | `log [<run pid>]`              | Each run of the session with its `argv`, `exit`, and last line per key                |

- Names resolve as a `file:` URL, a path, a process ID, a bundle id, a file name, then a display name, ignoring case and diacritics
- Process IDs read as `<pid>` or a line's `instance` (`<pid>@<start>`), which never names a later process reusing the pid
- `launch` takes the place of `open`, `xed`, and a launching `osascript`, which activate the app they open
- Element ids name one presentation of one element, rerun `read` once its window closes or shows again
- Every command exits 1 after printing a `fail` line and needs the Accessibility grant except `find`, `processes`, and `log`

## [03]-[LINES]

Lines read `{"<bundle id>": {"<pid>": {"instance": "<pid>@<start>", <change>}}}`, and session-wide lines key `watch-apps` or `system`:

| [INDEX] | [CHANGE]                                | [ACTION]                                                                 |
| :-----: | :-------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `status` `launching`                    | Wait for the next line                                                   |
|  [02]   | `opened` with `options` or `text`       | `read <pid> --window <number>`, then `perform <id> AXPress`              |
|  [03]   | `opened` kind `undescribed`             | `capture <pid> --window <number>`, read the image                        |
|  [04]   | `opened` kind `blocking`                | `capture <pid> --window <number>`, then `key <pid> <code>`               |
|  [05]   | `status` `unresponsive`, `stopped`      | `kill -CONT <pid>`                                                       |
|  [06]   | `status` `unresponsive`, `sleeping`     | `capture <pid>`, read each image for a dialog                            |
|  [07]   | `exited` with `reporting`               | Wait for the line holding `report`                                       |
|  [08]   | `exited` with `report`                  | Read the report at `url` before a launch                                 |
|  [09]   | `logged`                                | Read `message`, each new error or fault message of the app printing once |
|  [10]   | `busy`                                  | `sample <pid> 1 -file <path>`, a core loaded above 90% for 10 seconds    |
|  [11]   | `polled`                                | Act on watch lines in place of the repeated `argv`                       |
|  [12]   | `fail` `untrusted` or `noCaptureAccess` | Ask the person to allow `watch-apps` in the system prompt                |
|  [13]   | `fail` `host` or `noProcess` at launch  | `processes <app>`, read `reports` for the cause                          |
|  [14]   | `launched` with `replaced`              | `quit <pid>`, then `launch`, or `launch --new`                           |

- `opened` and `closed` name a window by `number`, `opened` adding `kind`, `title`, `options` (button labels), and `text`
- `kind` reads `filePanel`, `sheet`, `alert`, `modal`, `system` (a dialog helper naming the app), `menu`, `window`, `undescribed`, or `blocking`
- `process` reads what an `unresponsive` main thread did: `running`, `runnable`, `sleeping`, or `stopped`
- `idle` ends a `busy` span, `system` reads `memory` and `thermal` pressure off `normal` and `nominal` and on return
- Commands other than `watch` print `running` states, `asking` holding the rows requiring an answer

## [04]-[FOCUS]

Actions go to the app's process and leave the frontmost app unchanged:
- `AXRaise`, `activate`, and an `AXFrontmost` write take focus, and `perform` refuses `AXRaise`
- System Events `keystroke` and `key code` post to the frontmost app whatever process the script names
- Menu items of a background app take `AXPress`, items validated under a modal dialog read `enabled` false

## [05]-[RECORDING]

`watch --record <folder>` writes, with no activation:
- One `<window>-<time>.mov` per standard window and asking row, and the 15 seconds before each dialog, hang, and exit by signal
- `timeline.jsonl` with one `{"at": <time>, "<source>": <value>}` entry per `line`, unified `log` event, `host` line, `movie`, `clip`, `frame`, and `fail`
- `host` entries from each line a host appends to `<folder>/host/<name>.log`
