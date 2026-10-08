# [SESSIONS]

Blender processes each target runs in, their commands and outcomes, and their failures.

## [01]-[SERVERS]

`BLENDER_PATH` in `mise.toml` `[env]` names the binary every route runs, and `blender` on `PATH` runs the same one:

| [INDEX] | [SERVER]          | [PROCESS]         | [ADD_ON]                                  | [PORT] |
| :-----: | :---------------- | :---------------- | :---------------------------------------- | :----: |
|  [01]   | `blender`         | `blender-mcp`     | Extension `bl_ext.blender_lab.mcp`        |  9877  |
|  [02]   | `mcp-for-blender` | `mcp-for-blender` | `blender_mcp.py` in user `scripts/addons` |  9876  |

- `nx run rasm:interface -- blender` installs extension `mcp` from repository `blender_lab` and the community add-on at its server's protocol
- Live calls answer once their server's add-on listens, `headless.py` answering without one

## [02]-[TARGETS]

GUI launches run when no line is the GUI:

```bash
# GUI on <file> behind the frontmost application under the login session's environment
env -i /usr/bin/open -n -g -a Blender <file> --args --no-window-focus
```

- `-n` starts the GUI beside a background Blender LaunchServices registered, and `env -i` keeps the shell's `PATH` and `VIRTUAL_ENV` out
- `blender -c <command>` implies `--background`
- Add-ons and extensions import at startup before `--python` and `--python-expr` run
- `--python` and `--python-expr` compile in memory into a fresh `__main__` and add no folder to `sys.path`
- Blender's Python starts isolated and takes no `PYTHON*` variable, `--python-use-system-env` reads them for one launch
- `BLENDER_USER_*` naming a missing folder falls back to the user's tree, `BLENDER_USER_RESOURCES` without `config/` loads factory values

Each target takes one route, work leaving the user's GUI as found:

| [INDEX] | [TARGET]                          | [ROUTE]                            | [DECIDING_FACT]                                      |
| :-----: | :-------------------------------- | :--------------------------------- | :--------------------------------------------------- |
|  [01]   | Live file                         | `blender` `execute_blender_code`   | Server's wait ends at 300 s                          |
|  [02]   | Live file past 300 s              | Copy, then `headless.py start`     | Session state kept between calls with no client wait |
|  [03]   | Closed file, stock Blender        | `headless.py run <file>`           | Factory start, `-Y` on a file outside the repository |
|  [04]   | Closed file, extensions, or build | `headless.py start`, `call`        | User's preferences, every extension, `bpy.data` kept |
|  [05]   | New file                          | `run` or `start` on a missing path | `run` saves the factory startup, `start` the user's  |

```python
# [EXECUTE_BLENDER_CODE] Live file's current data as a copy a headless process opens, the GUI's file and modified flag unchanged
import bpy
from results import artifacts

copy = artifacts() / "<name>.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(copy), copy=True)
result = {"copy": str(copy)}
```

## [03]-[HEADLESS]

Each `headless.py` command is a Bash call printing a JSON outcome with its case as `kind`, exiting 0 for `Ran`, `Session`, `Stopped`, and `Rendered`:

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
- `run` and `start` open the file itself, and code that saves writes it
- Files inside the repository run scripts and drivers under `filepaths.use_scripts_auto_execute` outside every `autoexec_paths` entry, off in factory starts
- `-Y` turns scripts and drivers off in a file outside the repository, `scripts_blocked` naming the first one Blender skipped
- `bpy.ops.wm.open_mainfile(filepath=bpy.data.filepath, use_scripts=True)` in a call runs blocked drivers and scripts
- Simple and math driver expressions evaluate with scripts off, an expression reaching past the restricted names reads 0
- `start` writes `.artifacts/blender/session/<name>.json` and `<name>.log`, `stop` deletes the JSON record
- Calls keep `bpy.data`, see the file's active object and selection, and wait as long as the code runs
- One session runs per name until `stop`
- `bpy.ops.wm.revert_mainfile()` in a call returns a session to the file on disk
- `stop` saves the file the session holds when its data differ from a copy taken at the last load or save of that file
- Sessions reject `check_is_finished`

## [04]-[BACKGROUND]

Code in a background process sees a context no window draws:
- `bpy.data.window_managers[0].windows` holds every stored screen
- Stored 3D views read through `SpaceView3D.region_3d` with the matrices of their last GUI draw, `Region.data` reads `None`
- `bpy.app.timers` callbacks never fire, code meant for a timer runs inside the call
- Popup operators, `RegionView3D.update()`, and `ui.region_start_filter` polls under a stored view's override crash the process
- `screen.area_close` hangs a background process, and view operators under a stored view's override answer as in the GUI
- Sun Position prints an `AttributeError` traceback on `stderr` when a load or `scene.new` leaves the scene without a world

## [05]-[OUTCOMES]

Each case decides the next step:

| [INDEX] | [KIND]     | [HOLDS]                                            | [NEXT]                                                         |
| :-----: | :--------- | :------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Ran`      | `result`, `stdout`, `stderr`, `seconds`            | Read `result`                                                  |
|  [02]   | `Raised`   | Traceback with `<agent>` lines numbered as sent    | Fix the named line, a session keeps its data                   |
|  [03]   | `Session`  | `port`, `pid`, `log`, `scripts_blocked`, `tempdir` | `call` the name                                                |
|  [04]   | `Stopped`  | `file` held at the stop, `saved`, `log`            | Read the file                                                  |
|  [05]   | `Diverged` | Session running, its data and the file changed     | `save_as_mainfile` elsewhere or `revert_mainfile`, then `stop` |
|  [06]   | `Alive`    | Session outliving its termination by 300 s         | `kill -KILL <pid>`                                             |
|  [07]   | `Lost`     | Session ended in a call or stop, `crash` report    | Read the report's Python backtrace, then `start`               |
|  [08]   | `Failed`   | Exit code, `log`, `crash` report                   | Read the log's error line or the report                        |
|  [09]   | `Faults`   | `items`, each a refused input of the command       | Act on each item's `source`                                    |

- `Session` faults refuse a `start` of a running name, which `call` or `stop` takes, or a `call` or `stop` of a name outside `accepted`
- `ModuleType` faults name the MCP extension package a `start` failed to import, which `nx run rasm:interface -- blender` installs
- `Scope` faults name `--frames` text outside `accepted`, a frame, and an ascending `<first>..<last>`
- `stop` answers `Raised` with the session running when the save raises, a call saving elsewhere then precedes `stop`
- `crash` names `<stem>.crash.txt` Blender wrote as it died, `null` for an exit without a crash
- `Failed` logs stay as `.artifacts/blender/run-<pid>.log` for `run` and `.artifacts/blender/session/<name>.log` for `start`

## [06]-[QUITS]

Preference reloads and quits run through `mcp-for-blender`, its execute running with no sandbox:

```python
# [MCP_FOR_BLENDER] Quit, discarding unsaved changes
import bpy

print(sorted(bpy.ops.wm.quit_blender()))
```

- `blender` raises on `sys.exit`, `wm.quit_blender`, `read_userpref`, `read_factory_settings`, and `read_factory_userpref`, and runs `read_homefile`
- Quits from code skip the save prompt whatever `view.use_save_prompt` holds, and the server answers before Blender exits
- Running modal operators and render jobs leave saves and quits from code finishing
- `ps -p <pid>` in a later call reads the exit, `kill -KILL <pid>` ends a Blender `ps` lists after the quit

## [07]-[CONNECTIONS]

Timeouts and refused connections start from the GUI pid's listening ports:
- Both servers timing out with both ports listening mark a modal dialog holding the interface thread, which no call reaches
- `screencapture -x -o -l <id>` shows the dialog, the user dismisses it, and its preference row in `tools/interface/blender/` keeps it closed
- No 9877 row with the GUI open means the extension is disabled, its autostart or `bpy.app.online_access` is off, or its port is not 9877
- Extension port is an add-on preference defaulting to 9876, a reset of the extension's preferences moves its listener there
- Community add-on port and autostart are Scene properties each file saves
- Bind failures stop the listener, extension preferences drawing "Server is stopped" and the socket error (`[Errno 48] Address already in use`)
- `get_addon_status` reads `up_to_date` false for a community add-on older than its server, `nx run rasm:interface -- upgrade blender` then an apply
- `mcp-for-blender` keeps its last socket, its first call after a lost connection answers `Broken pipe` with no code run and the next reconnects

## [08]-[CRASHES]

Crashes and quits leave their record in Blender's temporary directory:
- Temporary directory is `preferences.filepaths.temporary_directory`, `$TMPDIR` while the preference is empty or under `--factory-startup`
- Crashed Blenders write `blender.crash.txt`, or `<stem>.crash.txt` with a file open, to the temporary directory
- Crash reports hold the last operators, the native backtrace, and a closing `# Python backtrace` with the `<agent>` line that crashed
- Each crash adds a macOS report `~/Library/Logs/DiagnosticReports/Blender-*.ips`
- Kills leave the process's `blender_<id>` folder (a session's `tempdir`) under the temporary directory, an exit or crash removing it
- `<pid>_autosave.blend`, `<stem>_<pid>_autosave.blend` for a saved file, and `quit.blend` in the temporary directory hold work a crash or quit left
