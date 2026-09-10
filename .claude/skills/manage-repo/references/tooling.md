# [TOOLING]

mise owns tool binaries and the process environment.

## [01]-[TOOLS]

- `[tools]` rows name a registry short name or a backend (`github:<owner>/<repo>`, `pipx:<package>`, `npm:<package>`)
- `prereleases = true` includes prereleases in `latest`, `minimum_release_age = "0s"` removes the 24h delay on a new release
- `idiomatic_version_file_enable_tools` names the tools with a version file mise reads, `global.json` for `dotnet`
- `mise exec -- <command>` runs a command under the configured tools and environment outside an activated shell

## [02]-[ENVIRONMENT]

- `{{config_root}}` renders the directory of `mise.toml`, `_.path` prepends directories to PATH
- `tools = true` on a row renders `tools.<name>.version` and `tools.<name>.path` after tool resolution
- `exec(command='<command>')` in a template runs at every render of the file, `cache_duration` keeps its output for that duration
