# [TOOLING]

## [01]-[TOOLS]

- Processes inherit mise's activated environment from their launch shell and call tools by name
- `[tools]` rows name a registry short name or a backend (`github:`, `pypi:`, `npm:`, `cargo:`), `[tool_alias]` names a backend spec holding a URL
- `cargo:` rows take a crate the registry lacks, a `cargo-binstall` row fetches its release binary before a source build
- `dotnet:<id>` rows install a .NET tool package's command, `dotnet dnx` resolves the feed on each launch and keeps a CLI parent process beside the tool
- `http:<tool>` rows install a release source archive for a tool with no registry entry and no release asset
- `http:` rows reading a releases API set `version_order = "semver"`, mise takes the last entry of a listing ordered newest first
- `auto_install = false` fails a shim, `mise exec`, or hook naming a missing tool in place of a download mid-command
- `registry_floating = true` reads current mise and aqua registries, snapshots baked into the mise binary answer when no cached copy loads
- `[settings]` holds no `lockfile` row, lock entries replay recorded versions and `conda:` closures in place of `latest`
- .NET SDKs install into mise's one `dotnet-root`, a running process's `DOTNET_ROOT` reaches each SDK a later `global.json` names
- Shared root holds runtimes of installed SDKs alone, `DOTNET_ROLL_FORWARD = "Major"` runs an older-major tool on it
- `MSBuildLocator` skips an SDK newer than its host runtime, a tool loading MSBuild rolls forward to the SDK's runtime
- Restores share NuGet's default `~/.nuget/packages`
- `pypi:git+` rows off GitHub name a tag prefix, at `latest` mise runs `git ls-remote` per `mise exec`, reads missing in shells, and fails offline
- Install side effects sit in their tool row's `postinstall`, a failing command fails the install
- Row `postinstall` reads `[env]` without `tools = true` rows, `[hooks] postinstall` renders `[env]` from the tools one install names
- Tools a target calls keep their row and read settings from repository files alone, machine setup installs tools and configures none
- `mise.ci.toml` `disable_tools` drops rows no CI job calls, a `tools = true` reader of one guards with `?.path or ""`
- Registry rows resolving to `conda:` (`imagemagick`) install a closure in their own `@rpath` prefix, linking no Homebrew, Nix, or Pixi library
- `mise install` keeps a `conda:` closure while its package version holds, `mise install --force <tool>` takes a conda-forge dependency rebuild

## [02]-[ENVIRONMENT]

- `tools = true` on a row renders `tools.<name>.version` and `tools.<name>.path` after tool resolution, consumers read an install path from that row
- Rows render in file order, later rows read a shared value from one earlier `[env]` row as `{{ env.<NAME> }}`
- `exec(command=...)` templates fail every mise call on a host without their command on PATH
- `[env]` holds values shells and repository programs read, a value one daemon alone reads sits in its `[daemons]` row `env`

## [03]-[SERVICES]

One pitchfork supervisor runs `[daemons]` rows, each local MCP server once as a Streamable HTTP service on a loopback port:
- `experimental = true` under `[settings]` enables `[daemons]`, `[daemons_settings]` `namespace` names daemon IDs `rasm/<name>`
- Rows declare `run` as a shell command opening with `exec`, mise runs it from `{{config_root}}` with project tools and `[env]`
- `run` renders `{{ env.<NAME> }}` and `{{ vars.<name> }}` alone, a daemon reads its own port as `$PORT`
- Row `env` tables reach pitchfork unrendered by mise and apply to the row's `mise x`, a value only the server reads (`XDG_CACHE_HOME`) sits in `run` as `exec env <NAME>=<value> <server>`, as an `env` row it moves mise's own cache
- Fixed `port` rows export `<NAME>_PORT` to every mise process, `[env]` URL rows read it as `{{ env.<NAME>_PORT }}`
- Rows with a `port` take `proxy = false`, mise otherwise exports a `<NAME>_URL` naming a proxy hostname nothing serves
- Daemon names fold into exported variable names, a name another program reads as its own variable (`BLENDER_PORT`) takes a longer name
- `retry = true` restarts a failed process on the supervisor interval, a process exiting 0 stays stopped
- `boot_start = true` rows start when supervisor starts in boot mode, and restart selection reads the flag
- `COLIMA_HOME` and `DOCKER_CONFIG` sit outside `.cache`, the VM disk and CLI plugin links outlive `rasm:clean`
- Readiness checks without `timeout` wait while the process runs, `docker` row's timeout fails a start when VM boots with its engine down
- `docker` readiness runs `colima status` on macOS, a stop after `docker version` passes and before Colima persists its runtime orphans the VM
- `docker` row takes no `boot_start`, and nothing stops the engine on idle
- `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE` names socket inside the engine's host, Ryuk mounts it and fails on Colima's client socket path
- Container images name `latest` with an always-pull policy, each run starts the newest image
- Harness rows hold service URLs and read no `[env]` value into MCP config
- Servers speaking stdio alone run as `rasm-mcp-bridge --listen 127.0.0.1:$PORT --grace 4s -- <command>`, `--grace` above the bridge's 3s stdin close and below the 5s stop timeout ends the upstream on every stop
- Bridge rows take `ready_http` at status `406`, the bridge binds before its upstream handshake and answers a plain `GET` with 406 once the upstream is up, so `ready_port` passes first
- `doppler run` rows take `--forward-signals=false`, the group's SIGTERM reaches the bridge once and a forwarded second signal abandons its drain
- `roslyn-codelens` starts from `/` with no solution, a solution it finds or is passed holds gigabytes from login, `load_solution` holds them from the first session needing them
- Servers starting one process per connection, or approving the connecting process by its code signature, keep a stdio row

Pitchfork has no login start, one `[bootstrap.macos.launchd.agents]` row runs its supervisor at login:
- Supervisor rows name no `stdout_path` or `stderr_path`, pitchfork writes its own file log
- `PITCHFORK_SUPERVISOR_AUTO_START` is false on macOS, a client waits for the launchd supervisor and fails without one
- `mise bootstrap macos launchd-agents apply` writes and loads `dev.mise.pitchfork.plist`, an unchanged plist keeps its process
- `launchctl kickstart -k gui/$UID/dev.mise.pitchfork` sends SIGTERM, the supervisor stops its daemons and launchd starts it with its boot daemons

Daemon operations:
- `mise daemons restart [<name>…]` registers current rows and restarts named daemons onto installed tools, starting a stopped one
- `mise daemons stop [<name>…]` waits for each process group to exit, a stopped daemon stays down until a start
- `mise daemons register` writes current rows to pitchfork without starting, supervisor boot reads registered rows
- `mise daemons logs <name>` reads one daemon's output

## [04]-[MAINTENANCE]

- `mise prune --yes` alone deletes old mise installations
- Orphan removal runs outside Nx, a running Nx process loads modules from the version an upgrade orphans
- `upgrade:docker` runs mise task `docker:update`, its VM restart on macOS starts the upgraded engine past systemd's `docker.service` start limit
- Generic cleanup keeps Pixi prefixes and `PULUMI_HOME` and installs no browser or tool
- Cleanup removes each target in place, an interrupted removal leaves the target at its listed path for next run
- `maintain.sh` quits VS Code before reopening it, the `code` CLI otherwise reuses a running instance and its old environment
- `upgrade:mise` moves the `.config/hk.pkl` package URLs to the `hk` version `mise upgrade` installs, `hk validate` accepts an older URL and reports nothing

## [05]-[TOML]

Tombi formats, lints, and validates TOML through `[tool.tombi]` in `pyproject.toml`:
- Tombi reads no `.editorconfig`, its indent width sits in `[tool.tombi]`
- Category dividers after a row take a blank line before them
- Schema-defined key and array ordering stays off, a sort discards the category and dependency order a file states
- Stock schemas missing a key a file sets take a `[[schemas]]` row: `root` at the key's table, its subschema as `path`, `strict = false`
- `tombi` skips files `.gitignore` excludes, named paths select files through its own CLI
