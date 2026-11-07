# [TOOLING]

## [01]-[TOOLS]

- Processes call tools by name, their shell or agent harness supplies mise's environment
- `[tools]` rows name a registry short name or a backend such as `github:`, `pypi:`, `npm:`, or `cargo:`
- Dylint uses its published binaries. Cargo Edit builds from its crate.
- `dotnet:<id>` rows install a .NET tool package's command, `dotnet dnx` resolves the feed on each launch and keeps a CLI parent process beside the tool
- `http:<tool>` rows install a release source archive for a tool with no registry entry and no release asset
- `http:` rows reading a releases API set `version_order = "semver"`, mise takes the last entry of a listing ordered newest first
- `auto_install = false` prevents downloads and permits the next `PATH` copy, `status.missing_tools = "always"` reports the missing tool
- `registry_floating = true` reads current mise and aqua registries, snapshots baked into the mise binary answer when no cached copy loads
- Node stays on the supported major while dependency-cruiser rejects the next major's prereleases
- .NET SDKs install into mise's one `dotnet-root`, a running process's `DOTNET_ROOT` reaches each SDK a later `global.json` names
- Shared root holds runtimes of installed SDKs alone, `DOTNET_ROLL_FORWARD = "Major"` runs an older-major tool on it
- `MSBuildLocator` skips an SDK newer than its host runtime, a tool loading MSBuild rolls forward to the SDK's runtime
- `pypi:git+` rows off GitHub name a tag prefix, at `latest` mise runs `git ls-remote` per `mise exec`, reads missing in shells, and fails offline
- Install side effects sit in their tool row's `postinstall`, a failing command fails the install
- Row `postinstall` reads `[env]` without `tools = true` rows, `[hooks] postinstall` renders `[env]` from the tools one install names
- Tools a target calls keep their row and read settings from repository files alone, machine setup installs tools and configures none
- The CI setup action selects tools through `MISE_ENABLE_TOOLS`
- `pnpm install --frozen-lockfile` validates every workspace manifest. Locked uv sync uses Pixi locally and installs the `dev` group in CI.
- Registry rows resolving to `conda:` (`imagemagick`) install a closure in their own `@rpath` prefix, linking no Homebrew, Nix, or Pixi library

## [02]-[ENVIRONMENT]

- `tools = true` on a row renders `tools.<name>.version` and `tools.<name>.path` after tool resolution, consumers read an install path from that row
- Rows render in file order, later rows read a shared value from one earlier `[env]` row as `{{ env.<NAME> }}`
- Configuration `exec()` templates need their command on `PATH`, URL templates using `version` render during installation
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
- `boot_start = true` rows start under `pitchfork supervisor run --boot`
- `COLIMA_HOME` and `DOCKER_CONFIG` keep VM and Docker state outside the checkout
- Readiness without a timeout waits while the process runs, Docker times out when the VM boots without its engine
- `rasm:docker` starts Colima on macOS and checks the Docker engine on Linux
- `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE` names socket inside the engine's host, Ryuk mounts it and fails on Colima's client socket path
- PostgreSQL services with persisted data follow one major tag until an upgrade migrates their data.
- Harness rows hold service URLs and read no `[env]` value into MCP config
- Servers speaking stdio alone run as `rasm-mcp-bridge --listen 127.0.0.1:$PORT --grace 4s -- <command>`, `--grace` above the bridge's 3s stdin close and below the 5s stop timeout ends the upstream on every stop
- Bridge rows use `ready_http` with status `406` and a `5m` timeout to wait for the upstream handshake
- `doppler run` rows take `--forward-signals=false`, the group's SIGTERM reaches the bridge once and a forwarded second signal abandons its drain
- `roslyn-codelens` starts from `/` without a solution and gets `30s` to stop
- Servers starting one process per connection, or approving the connecting process by its code signature, keep a stdio row

Mise's launchd row starts Pitchfork in boot mode with the repository environment:
- Supervisor rows name no `stdout_path` or `stderr_path`, pitchfork writes its own file log
- `PITCHFORK_SUPERVISOR_AUTO_START` is false on macOS, a client waits for the launchd supervisor and fails without one
- `mise bootstrap macos launchd-agents apply` writes and loads `dev.mise.pitchfork.plist`, an unchanged plist keeps its process
- Stop daemons before `launchctl kickstart -k gui/$UID/dev.mise.pitchfork`, launchd can kill the supervisor before every daemon drains
- `pitchfork supervisor start --force` drops boot mode, launchd's kickstart preserves it

Daemon operations:
- `mise daemons restart [<name>…]` registers current rows and restarts named daemons onto installed tools, starting a stopped one
- `mise daemons stop [<name>…]` waits for each process group and sends SIGKILL past its configured stop timeout
- `mise daemons register` writes current rows to pitchfork without starting, supervisor boot reads registered rows
- `mise daemons logs <name>` reads one daemon's output

## [04]-[MAINTENANCE]

- `mise prune --yes` alone deletes old mise installations
- Orphan removal runs outside Nx, a running Nx process loads modules from the version an upgrade orphans
- `upgrade:docker` runs `docker:update` on macOS. The update waits for Colima's VM, then restarts it and checks the upgraded Docker engine.
- `mise run clean` keeps native prefixes, task caches, and installed tools
- `maintain.sh` restores Pitchfork on exit and leaves Nx's task cache in place
- `maintain.sh` quits VS Code before reopening it, the `code` CLI otherwise reuses a running instance and its old environment

## [05]-[TOML]

Tombi formats, lints, and validates TOML through `[tool.tombi]` in `pyproject.toml`:
- Tombi reads no `.editorconfig`, its indent width sits in `[tool.tombi]`
- Category dividers after a row take a blank line before them
- Schema-defined key and array ordering stays off, a sort discards the category and dependency order a file states
- Stock schemas missing a key a file sets take a `[[schemas]]` row: `root` at the key's table, its subschema as `path`, `strict = false`
- `tombi` skips files `.gitignore` excludes, named paths select files through its own CLI
