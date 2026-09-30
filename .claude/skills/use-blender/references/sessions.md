# [SESSIONS]

Blender processes each target runs in, their commands and outcomes, and their failures.

## [01]-[SERVERS]

`BLENDER_PATH` in `mise.toml` `[env]` names the binary every route runs, and `blender` on `PATH` runs the same one:

| [INDEX] | [SERVER]          | [PROCESS]         | [ADD_ON]                                  | [PORT] |
| :-----: | :---------------- | :---------------- | :---------------------------------------- | :----: |
|  [01]   | `blender`         | `blender-mcp`     | Extension `bl_ext.blender_lab.mcp`        |  9877  |
|  [02]   | `mcp-for-blender` | `mcp-for-blender` | `blender_mcp.py` in user `scripts/addons` |  9876  |

- `nx run rasm:interface -- blender` installs extension `mcp` from repository `blender_lab` and the community add-on at its server's protocol
- Live calls answer once their server's add-on listens, `_for_cli` tools and `headless.py` answer without one

## [02]-[TARGETS]

One GUI Blender runs, and every agent drives it through its MCP servers:

```bash
# Every Blender with its arguments, the line without --background is the GUI both servers answer
pgrep -lf "^$BLENDER_PATH"
```

- Scripted launches under `tools/interface` wait while another agent drives Blender
- `blender -c <command>` implies `--background`
- Add-ons and extensions import at startup before `--python` and `--python-expr` run

Each target takes one route, work leaving the user's GUI as found:

| [INDEX] | [TARGET]                          | [ROUTE]                            | [DECIDING_FACT]                                         |
| :-----: | :-------------------------------- | :--------------------------------- | :------------------------------------------------------ |
|  [01]   | Live file                         | `blender` `execute_blender_code`   | One undo step per call, the server's wait ends at 300 s |
|  [02]   | Summary of the live file          | `get_blendfile_summary_*`          | Data-block counts, workspace, and render engine         |
|  [03]   | Live file past 300 s              | Copy, then `headless.py start`     | Session state kept between calls with no client wait    |
|  [04]   | Closed file, stock Blender        | `headless.py run <file>`           | Factory start, `-Y` on a file outside the repository    |
|  [05]   | Closed file, extensions, or build | `headless.py start`, `call`        | User's preferences, every extension, `bpy.data` kept    |
|  [06]   | Summary of a closed file          | `get_blendfile_summary_*_for_cli`  | Background Blender on the named file                    |
|  [07]   | New file                          | `run` or `start` on a missing path | Startup file saved under the path before the job        |

- `_for_cli` tools run under the user's preferences and Auto Run setting, drop printed output and tracebacks, and stop at 120 s
- `_for_cli` tools read a copy `<stem>_mcp_<nnnn>.blend` the GUI saves beside its own file while that file holds unsaved changes

```python
# [EXECUTE_BLENDER_CODE] Live file's current data as a copy a headless process opens, the GUI's file and modified flag unchanged
import bpy
from results import artifacts

copy = artifacts() / "<name>.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(copy), copy=True)
result = {"copy": str(copy)}
```

## [03]-[HEADLESS]

Every `headless.py` command runs as one Bash call and prints one JSON outcome with its case under `kind`:

```bash
# Code on stdin in a fresh factory process on <file>
python .claude/skills/use-blender/scripts/headless.py run <file> <<'PY'
import bpy
result = {"objects": sorted(bpy.data.objects.keys())}
PY

# Session <name> on <file> under the user's preferences, answered once it listens
python .claude/skills/use-blender/scripts/headless.py start <file> <name>

# Code on stdin in session <name>
python .claude/skills/use-blender/scripts/headless.py call <name> <<'PY'
import bpy
result = {"objects": sorted(bpy.data.objects.keys())}
PY

# Session's changes saved to the file it holds once a running call returns, then a quit
python .claude/skills/use-blender/scripts/headless.py stop <name>
```

- `run` and `call` execute through the MCP extension's own execute and sandbox
- Printed output returns in `stdout` and `stderr` and goes to the process log, which `run` deletes once it answers
- `run` points `BLENDER_USER_EXTENSIONS`, `BLENDER_USER_CONFIG`, and `BLENDER_USER_SCRIPTS` at `.artifacts/blender/<extensions|config|scripts>/`
- Factory starts hold stock add-ons and the wheels of installed extensions, no extension operator, and keep the units of the file they open
- `run` on a missing path saves the factory startup file there, `start` saves the user's startup file
- `run` and `start` open the file itself, and code that saves writes it
- Scripts and drivers run under `filepaths.use_scripts_auto_execute` outside every `autoexec_paths` entry, factory starts leave it off
- `-Y` turns scripts and drivers off in a file outside the repository, `scripts_blocked` holding `bpy.app.autoexec_fail_message`
- Auto Run gates text blocks and driver expressions the simple-expression evaluator cannot handle
- Sun Position prints an `AttributeError` traceback on `stderr` when a load leaves the scene without a world, and the load finishes
- `start` writes `.artifacts/blender/session/<name>.json` and `<name>.log`, `stop` deletes the JSON record
- Calls keep `bpy.data`, see the file's active object and selection, and wait as long as the code runs
- One session runs per name until `stop`, concurrent work takes distinct names
- `bpy.ops.wm.revert_mainfile()` in a call returns a session to the file on disk
- `stop` saves the file the session holds when its data differ from a copy taken at the last load or save of that file
- Sessions reject `check_is_finished`

## [04]-[BACKGROUND]

Code in a background process sees a context no window draws:
- Stored 3D views read through `SpaceView3D.region_3d` with the matrices of their last GUI draw, `Region.data` reads `None`
- `bpy.app.timers` callbacks never fire, code meant for a timer runs inside the call

## [05]-[OUTCOMES]

Each case decides the next step:

| [INDEX] | [KIND]           | [HOLDS]                                         | [NEXT]                                                         |
| :-----: | :--------------- | :---------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Ran`            | `result`, `stdout`, `stderr`, `seconds`         | Read `result`                                                  |
|  [02]   | `Raised`         | Traceback with `<agent>` lines numbered as sent | Fix the named line, a session keeps its data                   |
|  [03]   | `Session`        | `port`, `pid`, `log`, `scripts_blocked`         | `call` the name                                                |
|  [04]   | `Stopped`        | `file` held at the stop, `saved`, `log`         | Read the file                                                  |
|  [05]   | `Diverged`       | Session running, its data and the file changed  | `save_as_mainfile` elsewhere or `revert_mainfile`, then `stop` |
|  [06]   | `Alive`          | Session outliving its termination by 300 s      | `kill -KILL <pid>`                                             |
|  [07]   | `Lost`           | Session ended in a call or stop, `crash` report | Read the report's Python backtrace, then `start`               |
|  [08]   | `Failed`         | Exit code, `log`, `crash` report                | Read the log's error line or the report                        |
|  [09]   | `NoSession`      | Record of a name with no running process        | `start` the name                                               |
|  [10]   | `SessionRunning` | Session holding the name                        | `call` or `stop` the name                                      |
|  [11]   | `NoBridge`       | First package Blender failed to import          | `nx run rasm:interface -- blender` installs the extension      |

- Exit code 0 marks `Ran`, `Session`, `Stopped`, and `Rendered`, every other case exits 1
- `stop` answers `Raised` with the session running when the save raises, a call saving elsewhere then precedes `stop`
- `crash` names `<stem>.crash.txt` Blender wrote as it died, `null` for an exit without a crash
- Crash reports hold the last operators, the native backtrace, and a closing `# Python backtrace` with the `<agent>` line that crashed
- `Failed` logs stay as `.artifacts/blender/run-<pid>.log` for `run` and `.artifacts/blender/session/<name>.log` for `start`

## [06]-[QUITS]

Quits and preference reloads run through `mcp-for-blender`, the `blender` sandbox blocking `sys.exit`, `wm.quit_blender`, `read_userpref`, `read_factory_settings`, and `read_factory_userpref`:

```python
# [MCP_FOR_BLENDER] Quit, saving a titled file with unsaved changes and discarding an untitled one
import bpy

status = bpy.ops.wm.save_mainfile(exit=True) if bpy.data.filepath and bpy.data.is_dirty else bpy.ops.wm.quit_blender()
print(sorted(status))
```

- `mcp-for-blender` `execute_blender_code` answers with printed output and drops `result`, a raise answers with the traceback alone
- Quits from code skip the save prompt, and the server answers before Blender exits
- `ps -p <pid>` in a later call reads the exit, `kill -KILL <pid>` ends a Blender `ps` lists after the quit

## [07]-[FAILURES]

Timeouts and refused connections start with one reading:

```bash
# Every listening Blender socket, one line per port of the GUI and each session
lsof -a -nP -iTCP -sTCP:LISTEN -c Blender
```

- No 9877 row with the GUI open means the extension is disabled, its autostart or `bpy.app.online_access` is off, or its port is not 9877
- Extension port is an add-on preference defaulting to 9876, a reset of the extension's preferences moves its listener there
- Community add-on port and autostart are Scene properties each file saves
- Blender's Python starts isolated and takes no `PYTHON*` variable, `--python-use-system-env` reads them for one launch
- `BLENDER_USER_CONFIG` naming a missing folder falls back to the user's config, `BLENDER_USER_RESOURCES` without `config/` loads factory values
- Temporary directory is `preferences.filepaths.temporary_directory`, `$TMPDIR` while the preference is empty or under `--factory-startup`
- Crashed Blenders write `blender.crash.txt`, or `<stem>.crash.txt` with a file open, to the temporary directory
- Each crash adds a macOS report `~/Library/Logs/DiagnosticReports/Blender-*.ips`
- Kills leave the process's `blender_<id>` folder under the temporary directory, a normal exit and a crash remove it
- `<pid>_autosave.blend`, `<stem>_<pid>_autosave.blend` for a saved file, and `quit.blend` in the temporary directory hold work a crash or quit left
