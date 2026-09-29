# [SETUP]

Servers, startup, processes, and failures of the Blender binary `BLENDER_PATH` names in `mise.toml` `[env]`.

## [01]-[SERVERS]

| [INDEX] | [SERVER]          | [PROCESS]         | [ADD_ON]                                  | [PORT] |
| :-----: | :---------------- | :---------------- | :---------------------------------------- | :----: |
|  [01]   | `blender`         | `blender-mcp`     | Extension `bl_ext.blender_lab.mcp`        |  9877  |
|  [02]   | `mcp-for-blender` | `mcp-for-blender` | `blender_mcp.py` in user `scripts/addons` |  9876  |

- Extension `bl_ext.blender_lab.mcp` resolves from repository `blender_lab`, absent from each `<major>.<minor>` config folder until added
- `blender -c extension repo-add --name "Blender Lab" --url https://lab.blender.org/ blender_lab` adds the repository
- `blender --online-mode -c extension install --sync --enable mcp` then installs the extension
- `mcp-for-blender install-addon --addons-dir "<USER>/scripts/addons"` writes the community add-on at the server's protocol
- Extension port is an add-on preference defaulting to the community port 9876, a reset of the extension's preferences moves its listener there
- Community port (9876), autostart (on), and asset toggles (off) are Scene properties saved per file
- Community add-on reads each service key from its preferences, then the Scene, then a `BLENDERMCP_*` variable
- Sidebar key fields write add-on preferences, and the Hyper3D trial key alone goes into a Scene property every `.blend` the GUI writes saves
- Files leaving the machine clear `blendermcp_*_api_key`, `blendermcp_hunyuan3d_secret_id`, and `blendermcp_hunyuan3d_secret_key` before the save

## [02]-[STARTUP]

- Factory runs keep the units of the file they open
- Scripts and drivers in opened files run under `filepaths.use_scripts_auto_execute` outside every `autoexec_paths` entry
- `bpy.app.autoexec` reads whether opened files run scripts, `bpy.app.autoexec_override` the `-y` or `-Y` value, `None` without either
- `bpy.data.scenes.new()` starts from factory metric units, `bpy.ops.scene.new(type="EMPTY")` copies the current scene's settings
- Blender's Python starts isolated and takes no `PYTHON*` variable, `--python-use-system-env` reads them for one launch
- `BLENDER_USER_RESOURCES` naming a folder without a `config` subfolder reads and writes the user's own config
- Add-ons and extensions import at startup before `--python` and `--python-expr` run
- `--python` and `--python-expr` compile in memory into a fresh `__main__` and add no folder to `sys.path`

## [03]-[PROCESS]

- `blender` on `PATH` runs the same binary
- `open` without `-n` sends the reopen event to any process LaunchServices lists for the bundle, a gone or windowless one fails with -600
- `--args` passes what follows to Blender, `open -W` returns at exit, and `--stdout <log>` and `--stderr <err>` append the process output
- `blender -c <command>` implies `--background`, a windowed run adds `--python-expr "<code>"` after `--args` in the GUI launch

## [04]-[FAILURES]

Timeouts and refused connections start with one reading, then act on it:

```bash
# Both add-on sockets and any headless session, one Blender line per listening port
lsof -a -nP -iTCP -sTCP:LISTEN -c Blender
```

- No 9877 row with the GUI open means the extension is disabled, its autostart or `bpy.app.online_access` is off, or its port preference is not 9877
- Bind failures show their error in the extension's preferences
- Asset tools answer `Unknown command type` while the active scene's `blendermcp_use_<library>` toggle is off
- `get_addon_status` reports the community add-on's protocol against the server's, a mismatch takes `install-addon`
- `install-addon` without `--addons-dir` rewrites the newest `<major>.<minor>` folder holding the add-on
- `mcp-for-blender` keeps its last socket, its first call after a relaunch answers `Broken pipe` with no code run and the next reaches the new GUI
- Temporary directory is `preferences.filepaths.temporary_directory`, `$TMPDIR` while the preference is empty
- Crashed Blenders write `blender.crash.txt`, or `<stem>.crash.txt` with a file open, to the temporary directory
- Crash logs hold the last operators, the native backtrace, and the Python backtrace, macOS adds `~/Library/Logs/DiagnosticReports/Blender-*.ips`
- `<pid>_autosave.blend`, `<stem>_<pid>_autosave.blend` for a saved file, and `quit.blend` in the temporary directory hold work a crash or quit left
