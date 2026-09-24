# [SETUP]

Blender 5.2 LTS runs from `/Applications/Blender.app` with bundle id `org.blenderfoundation.blender`, `blender` on `PATH` runs `Contents/MacOS/Blender`, and both MCP add-ons start their listener in the GUI alone.

## [01]-[SERVERS]

| [INDEX] | [SERVER]          | [PROCESS]         | [ADD_ON]                                  | [PORT] |
| :-----: | :---------------- | :---------------- | :---------------------------------------- | :----: |
|  [01]   | `blender`         | `blender-mcp`     | Extension `bl_ext.blender_lab.mcp`        |  9877  |
|  [02]   | `mcp-for-blender` | `mcp-for-blender` | `blender_mcp.py` in user `scripts/addons` |  9876  |

- Extension port is an add-on preference with default 9876, so a reset of the extension's preferences moves its listener onto the community port
- Community port and asset toggles are Scene properties, a file from another startup file holds the asset toggles off

## [02]-[MACHINE]

- `~/Library/Application Support/design-tools/materials` holds ambientCG material sets and HDRIs under `hdri/`, the ambientCG add-on's `cache_dir`
- Sketchfab, Poly Pizza, and Hyper3D keys sit in the community add-on preferences, which win over a Scene key and a `BLENDERMCP_*` variable
- Community add-on copies the Sketchfab and Poly Pizza keys into Scene properties, and every `.blend` the GUI writes saves them
- Files leaving the machine take `blendermcp_sketchfab_api_key` and `blendermcp_polypizza_api_key` set to `""` before the save

## [03]-[STARTUP]

Startup scene and preference values a task inherits:
- Units display imperial feet at unit scale 1.0 with separate units, a factory run keeps the units of the file it opens
- Cycles is the startup engine on the Metal GPU with OpenImageDenoise on the GPU, persistent data, and 1024 adaptive samples at threshold 0.01
- Light paths are Cycles' factory 12, 4, 4, 12, 0, 8 with caustics on, direct clamp off, and indirect clamp 394
- View transform is `AgX` with look `None` at exposure -5.3 on the `sRGB` display device, frame rate 24 fps, render 1920x1080 at 300 dpi
- Startup scenes hold no cube, the `Light` object is a 137 W/m² sun that Sun Position points together with the world's Sky Texture
- Camera sits at eye height 100 ft south of the origin facing north with `clip_end` 3000 ft
- Workspaces are Model, BIM, Drafting, Nodes, Shading, Render, and Script, each at `object_mode` OBJECT with `use_filter_by_owner` on
- `use_save_prompt` is off, `save_modified_images` is `ALWAYS_SAVE`, auto-save runs every minute into `filepaths.temporary_directory`
- Scripts and drivers inside opened files run without a prompt in the GUI
- `bpy.data.scenes.new()` starts from factory metric units, `bpy.ops.scene.new(type="EMPTY")` copies the current scene's settings
- Blender's Python starts isolated and takes no `PYTHON*` variable, `--python-use-system-env` lifts that for one launch
- Add-ons and extensions import at startup before `--python` and `--python-expr` run
- `--python` and `--python-expr` compile in memory into a fresh `__main__` and add no folder to `sys.path`

## [04]-[PROCESS]

- `open` without `-n` sends the reopen event to any process LaunchServices lists for the bundle, a gone or windowless one fails with -600
- `--args` passes what follows to Blender, `open -W` returns at exit, and `--stdout <log>` and `--stderr <err>` append the process output
- `osascript -e 'ignoring application responses' -e 'tell application id "org.blenderfoundation.blender" to quit' -e 'end ignoring'` quits a GUI
- Quit Apple events without `ignoring application responses` exit 1 while Blender quits
- Starting `Contents/MacOS/Blender` directly from a shell blocks the shell until Blender quits

## [05]-[FAILURES]

Timeouts and refused connections start with one reading, then act on it:

```bash
# Both add-on sockets and any headless session, one Blender line per listening port
lsof -a -nP -iTCP -sTCP:LISTEN -c Blender
```

- No 9877 row with the GUI open means the extension is disabled, its autostart is off, or `bpy.app.online_access` is off
- Asset tools answer `Unknown command type` while the active scene's `blendermcp_use_<library>` toggle is off
- `get_addon_status` reports the community add-on's protocol against the server's, a mismatch takes `mcp-for-blender install-addon`
- Crashed GUIs leave `blender.crash.txt`, or `<file stem>.crash.txt` with a file open, in `preferences.filepaths.temporary_directory`
- Crash logs hold the last operators, the native backtrace, and the Python backtrace, macOS adds `~/Library/Logs/DiagnosticReports/Blender-*.ips`
- Other Blenders write reports into the same folders, and a report belongs to a probe when its timestamp falls inside the probe
- `<pid>_autosave.blend` and `quit.blend` in the temporary directory hold the work a crash or quit left
