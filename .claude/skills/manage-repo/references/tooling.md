# [TOOLING]

mise owns tool binaries, the process environment, and local services.

## [01]-[TOOLS]

- Processes inherit mise's activated environment from their launch shell and call tools by name
- `[tools]` rows name a registry short name or a backend (`github:<owner>/<repo>`, `pypi:<package>`, `npm:<package>`)
- `http:<tool>` rows install a release source archive for a tool with no registry entry and no release asset
- `http:` rows reading a releases API set `version_order = "semver"`, mise takes the last entry of a listing ordered newest first
- `prereleases = true` includes prereleases in `latest`, `minimum_release_age = "0s"` removes the 24h delay on a new release
- `registry_floating = true` reads current mise and aqua registries, snapshots baked into the mise binary answer when no cached copy loads
- `idiomatic_version_file_enable_tools` names the tools with a version file mise reads, `global.json` for `dotnet`, `rust-toolchain.toml` for `rust`
- .NET SDKs install into one root Parametric Forge names, a running process's `DOTNET_ROOT` reaches each SDK a later `global.json` names
- Shared root holds runtimes of installed SDKs alone, `DOTNET_ROLL_FORWARD = "Major"` runs an older-major tool on it
- `MSBuildLocator` skips an SDK newer than its host runtime, a tool loading MSBuild rolls forward to the SDK's runtime
- Restores share NuGet's default packages folder `~/.nuget/packages`, the folder Parametric Forge's `NUGET_PACKAGES` names
- `mise upgrade` moves `latest` rows alone, `--bump <tool>` rewrites one exact row with its options, a release-less GitHub `pypi:` row keeps its installed HEAD
- `pypi:git+` rows off GitHub name a tag prefix, at `latest` mise runs `git ls-remote` per `mise exec`, reads missing in shells, and fails offline
- `postinstall` hooks see the tools of the install that ran them alone, `"{{ mise_bin }}" exec --` reaches the project's whole toolset
- `conda:` rows install a library and its dependency closure into one prefix, `[vars]` `installs` joined with `conda-<name>/latest` names the newest

## [02]-[ENVIRONMENT]

- `{{config_root}}` renders the directory of `mise.toml`, `_.path` prepends directories to PATH
- `tools = true` on a row renders `tools.<name>.version` and `tools.<name>.path` after tool resolution
- Rows render in file order, later rows read a shared value from one earlier `[env]` row as `{{ env.<NAME> }}`
- Paths name a mise install by its `latest` link and a Homebrew formula by its `opt` link, a server reading mise's environment needs neither
- `exec(command=...)` templates fail every mise call on a host without their command on PATH
- `[env]` holds values shells and repository programs read, a value one local service alone reads sits on its launchd row

## [03]-[SERVICES]

Launchd agents run each local MCP server once as a Streamable HTTP service on a loopback port, harness rows hold the service URL:
- Ports a repository program also reads sit once in `[vars]`, rendered into the launchd row and an `[env]` URL row
- Harnesses read no `[env]` value into MCP config
- `[bootstrap.macos.launchd.agents]` rows run `{{ mise_bin }}` with `args` opening `exec --` and `working_directory` at `{{config_root}}`
- `keep_alive` restarts an exited process, and launchd starts a `KeepAlive` agent at login with no `RunAtLoad`
- Row `environment` tables render into plist `EnvironmentVariables`
- Row values render no `exec(command=...)` template
- Programs on no PATH mise builds take an absolute path in `args`, `args` expand no `~`
- Agents inherit the GUI domain environment, and `exec --` adds repository `[env]` and tool paths
- `mise bootstrap macos launchd-agents apply` writes and loads `~/Library/LaunchAgents/dev.mise.<name>.plist`, an unchanged plist keeps its process
- Running agents keep the environment they started with, `launchctl kickstart -k gui/$UID/dev.mise.<name>` restarts one onto current values
- `[vars]` `log_dir` names README's log directory for `[env]` and launchd rows
- Rows set `stdout_path` and `stderr_path` under `log_dir`, launchd creates the directory and discards unnamed output
- `mise bootstrap macos launchd-agents status` lists each agent's state, `launchctl print gui/$UID/dev.mise.<name>` its pid and last exit code
- Rows set `MISE_PREFER_OFFLINE` in `environment`, `exec --` then resolves no `latest` row over the network at each start
- Servers speaking stdio alone run as `rasm-mcp-bridge --listen 127.0.0.1:<port> -- <command>`
- Servers starting one process per connection, or approving the connecting process by its code signature, keep a stdio row

## [04]-[TOML]

Tombi formats, lints, and validates every TOML file against its schema through `.config/tombi.toml`:
- Tombi reads no `.editorconfig`, its indent width sits in its own file
- Category dividers after a row take a blank line before them
- Schema-defined key and array ordering stays off, a sort discards the category and dependency order a file states
- Stock schemas missing a key a file sets take a `[[schemas]]` row with `root` at the table holding the key, its subschema as `path`, and `strict = false`
- `tombi` walks the project directory under `.gitignore`, its targets take no `fd` list and no path argument
