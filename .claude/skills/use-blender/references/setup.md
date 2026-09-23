# [SETUP]

Blender 5.2 LTS runs from `/Applications/Blender.app/Contents/MacOS/Blender`, a Homebrew cask the machine profile owns that puts `blender` on `PATH`, with both MCP add-ons starting their listener when the GUI opens.

## [01]-[SERVERS]

| [INDEX] | [SERVER]          | [PROCESS]                                       | [ADD_ON]                                  | [PORT] |
| :-----: | :---------------- | :---------------------------------------------- | :---------------------------------------- | :----: |
|  [01]   | `blender`         | `.venv/bin/blender-mcp`, a `uv.lock` git source | Extension `bl_ext.blender_lab.mcp`        |  9877  |
|  [02]   | `mcp-for-blender` | `mcp-for-blender`, a `mise.toml` `pypi:` row    | `blender_mcp.py` in user `scripts/addons` |  9876  |

- `.mcp.json` and `.codex/config.toml` hold both rows and start each through `mise exec`
- `blender` reads `BLENDER_MCP_PORT=9877` and `BLENDER_PATH`, and `headless.py` reads both from the same row
- Headless sessions serve the Lab add-on's execute protocol on a free port, 9876 and 9877 stay with the GUI
- Extension port is an add-on preference, community port and asset toggles are Scene properties, off in a file saved from another startup file

## [02]-[FAILURES]

Timeouts and refused connections start with one reading, then act on it:

```bash
# Both add-on sockets and any headless session, one Blender line per listening port
lsof -a -nP -iTCP -sTCP:LISTEN -c Blender
```

- No 9877 row with Blender open means the extension is disabled, its autostart is off, or `bpy.app.online_access` is off
- Asset tools answer `Unknown command type` while the active scene's `blendermcp_use_<library>` toggle is off
- `get_addon_status` reports the community add-on's protocol against the server's, a mismatch takes `mise exec -- mcp-for-blender install-addon`
- `start` answering `Failed` holds the lines Blender printed before it quit, `session.log` holds the rest
- `call` answering `NoSession` after a crash means the recorded process is gone, `start` opens a new one

## [03]-[SESSION]

Startup scene and preference values a task inherits:
- Units display imperial feet at unit scale 1.0, a factory run keeps the units of the file it opens
- EEVEE is the startup engine, Cycles renders on the Metal GPU with GPU denoising and persistent data
- Background processes start with an empty Cycles device list and render on the CPU until `refresh_devices()`, `headless.py` refreshes it
- View transform is `Khronos PBR Neutral`, frame rate 24 fps
- Scripts and drivers inside opened files run without a prompt in the GUI and in sessions on repository files
- Sketchfab and Poly Pizza keys sit in the community add-on preferences, from 1Password Tokens items named by their `BLENDERMCP_*` variable
- Hunyuan3D holds no account and its tools fail

## [04]-[EXTENSIONS]

Preference, extension, and workspace changes for the user run in the live session, which saves `userpref.blend` on quit over any other process's write:
- Zip packages install through `bpy.ops.extensions.package_install_files(filepath=<zip>, repo="user_default", enable_on_install=True)`
- Platform packages take `bpy.ops.extensions.repo_sync_all()`, then `package_install(repo_index=<i>, pkg_id="<id>", enable_on_install=True)`
- `<i>` is the index of `blender_org` in `preferences.extensions.repos`
- Installs enable inside the call with no restart, and the wheel sync keeps every package
- `bpy.ops.extensions.package_uninstall(repo_index=<i>, pkg_id="<id>")` removes one
- Workspace and screen edits of the open document run live, background Blender holds no screens

Extension development validates, builds, and tests a package under `BLENDER_USER_RESOURCES=<dir>`, apart from the user's packages:

```bash
# Manifest check, exit 1 naming each invalid key or the missing blender_manifest.toml
blender --factory-startup -c extension validate <source>
# Zip named <id>-<version>.zip under an existing <dist>
blender --factory-startup -c extension build --source-dir <source> --output-dir <dist>
```

- `-c extension install-file -r user_default -e <zip>` installs the built zip
- `--online-mode -c extension install -s -e blender_org.<id>` installs a platform package, empty resources start with online access off
- Background runs under the same variable call the installed package's operators
