# [SETUP]

Blender runs from `/Applications/Blender.app` with bundle id `org.blenderfoundation.blender`, `blender` on `PATH` runs `Contents/MacOS/Blender`, and both MCP add-ons start their listener in the GUI alone.

## [01]-[SERVERS]

| [INDEX] | [SERVER]          | [PROCESS]         | [ADD_ON]                                  | [PORT] |
| :-----: | :---------------- | :---------------- | :---------------------------------------- | :----: |
|  [01]   | `blender`         | `blender-mcp`     | Extension `bl_ext.blender_lab.mcp`        |  9877  |
|  [02]   | `mcp-for-blender` | `mcp-for-blender` | `blender_mcp.py` in user `scripts/addons` |  9876  |

- Extension port is an add-on preference with default 9876, so a reset of the extension's preferences moves its listener onto the community port
- Community port and asset toggles are Scene properties that default off and save per file
- Community add-on reads each service key from its preferences, then the Scene, then a `BLENDERMCP_*` variable
- Community add-on copies the Sketchfab and Poly Pizza keys into Scene properties, and every `.blend` the GUI writes saves them
- Files leaving the machine take `blendermcp_sketchfab_api_key` and `blendermcp_polypizza_api_key` set to `""` before the save

## [02]-[STARTUP]

- Factory runs keep the units of the file they open
- Scripts and drivers in opened files run under `filepaths.use_scripts_auto_execute` outside every `autoexec_paths` entry
- `bpy.ops.wm.open_mainfile(filepath=, use_scripts=True)` runs them in a file under an excluded path
- `bpy.data.scenes.new()` starts from factory metric units, `bpy.ops.scene.new(type="EMPTY")` copies the current scene's settings
- Blender's Python starts isolated and takes no `PYTHON*` variable, `--python-use-system-env` lifts that for one launch
- Add-ons and extensions import at startup before `--python` and `--python-expr` run
- `--python` and `--python-expr` compile in memory into a fresh `__main__` and add no folder to `sys.path`

## [03]-[PROCESS]

- `open` without `-n` sends the reopen event to any process LaunchServices lists for the bundle, a gone or windowless one fails with -600
- `--args` passes what follows to Blender, `open -W` returns at exit, and `--stdout <log>` and `--stderr <err>` append the process output
- `osascript -e 'ignoring application responses' -e 'tell application id "org.blenderfoundation.blender" to quit' -e 'end ignoring'` quits a GUI
- Quit Apple events without `ignoring application responses` exit 1 while Blender quits
- Starting `Contents/MacOS/Blender` directly from a shell blocks the shell until Blender quits

## [04]-[FAILURES]

Timeouts and refused connections start with one reading, then act on it:

```bash
# Both add-on sockets and any headless session, one Blender line per listening port
lsof -a -nP -iTCP -sTCP:LISTEN -c Blender
```

- No 9877 row with the GUI open means the extension is disabled, its autostart is off, or `bpy.app.online_access` is off
- Asset tools answer `Unknown command type` while the active scene's `blendermcp_use_<library>` toggle is off
- `get_addon_status` reports the community add-on's protocol against the server's, a mismatch takes `mcp-for-blender install-addon`
- `mcp-for-blender` keeps its last socket, its first call after a relaunch answers `Broken pipe` with no code run and the next reaches the new GUI
- Crashed GUIs leave `blender.crash.txt`, or `<file stem>.crash.txt` with a file open, in `preferences.filepaths.temporary_directory`
- Crash logs hold the last operators, the native backtrace, and the Python backtrace, macOS adds `~/Library/Logs/DiagnosticReports/Blender-*.ips`
- Other Blenders write reports into the same folders, and a report belongs to a run when its timestamp falls inside it
- `<pid>_autosave.blend` and `quit.blend` in the temporary directory hold the work a crash or quit left
