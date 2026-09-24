# [TOOLING]

mise owns tool binaries and the process environment.

## [01]-[TOOLS]

- `[tools]` rows name a registry short name or a backend (`github:<owner>/<repo>`, `pypi:<package>`, `npm:<package>`)
- `prereleases = true` includes prereleases in `latest`, `minimum_release_age = "0s"` removes the 24h delay on a new release
- `idiomatic_version_file_enable_tools` names the tools with a version file mise reads, `global.json` for `dotnet`
- `dotnet.isolated = true` installs each SDK under its own root and `dotnet` host, macOS kills a shared-root host a new SDK overwrote
- Isolated roots hold the SDK's runtime alone, `DOTNET_ROLL_FORWARD = "Major"` runs an older-major tool on it
- `MSBuildLocator` skips an SDK newer than its host runtime, a tool loading MSBuild rolls forward to the SDK's runtime
- Editor settings name a mise install by its `latest` link, a server reading mise's environment takes none
- `mise activate` drops the shim directory from PATH unless `not_found_auto_install` holds, `auto_install = false` clears that setting
- `mise upgrade` moves each `latest` row, a `pypi:` row from a GitHub repository with no release stays at the branch HEAD it installed
- `mise exec -- <command>` runs a command under the configured tools and environment outside an activated shell

## [02]-[ENVIRONMENT]

- `{{config_root}}` renders the directory of `mise.toml`, `_.path` prepends directories to PATH
- `tools = true` on a row renders `tools.<name>.version` and `tools.<name>.path` after tool resolution
- `exec(command='<command>')` in a template runs at every render of the file, `cache_duration` keeps its output for that duration
