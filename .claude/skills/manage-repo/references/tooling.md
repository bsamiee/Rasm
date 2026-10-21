# [TOOLING]

mise owns tool binaries, the process environment, and local services.

## [01]-[TOOLS]

- Processes inherit mise's activated environment from their launch shell and call tools by name
- `[tools]` rows name a registry short name or a backend (`github:`, `pypi:`, `npm:`, `cargo:`), `[tool_alias]` names a backend spec holding a URL
- `cargo:` rows take a crate the registry lacks, a `cargo-binstall` row fetches its release binary before a source build
- `http:<tool>` rows install a release source archive for a tool with no registry entry and no release asset
- `http:` rows reading a releases API set `version_order = "semver"`, mise takes the last entry of a listing ordered newest first
- `prereleases = true` includes prereleases in `latest`, `minimum_release_age = "0s"` removes the 24h delay on a new release
- `registry_floating = true` reads current mise and aqua registries, snapshots baked into the mise binary answer when no cached copy loads
- `[settings]` holds no `lockfile` row, lock entries replay recorded versions and `conda:` closures in place of `latest`
- `idiomatic_version_file_enable_tools` makes mise read each named tool's version file
- .NET SDKs install into one root Parametric Forge names, a running process's `DOTNET_ROOT` reaches each SDK a later `global.json` names
- Shared root holds runtimes of installed SDKs alone, `DOTNET_ROLL_FORWARD = "Major"` runs an older-major tool on it
- `MSBuildLocator` skips an SDK newer than its host runtime, a tool loading MSBuild rolls forward to the SDK's runtime
- Restores share NuGet's default `~/.nuget/packages`
- `pypi:git+` rows off GitHub name a tag prefix, at `latest` mise runs `git ls-remote` per `mise exec`, reads missing in shells, and fails offline
- Install side effects sit in their tool row's `postinstall`, a failing command fails the install
- Row `postinstall` reads `[env]` without `tools = true` rows, `[hooks] postinstall` renders `[env]` from the tools one install names
- Tools a target calls keep their row where Parametric Forge installs the same tool machine-wide, CI runners and other hosts hold no Forge
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
- Row `env` tables reach pitchfork unrendered by mise, pitchfork renders `{{ dir }}` there as project root
- Fixed `port` rows export `<NAME>_PORT` to every mise process, `[env]` URL rows read it as `{{ env.<NAME>_PORT }}`
- Daemon names fold into exported variable names, a name another program reads as its own variable (`BLENDER_PORT`) takes a longer name
- `retry = true` restarts a failed process on the supervisor interval, a process exiting 0 stays stopped
- `boot_start = true` rows start when the supervisor starts in boot mode, and restart selection reads the flag
- `COLIMA_HOME` and `DOCKER_CONFIG` sit outside `.cache`, the VM disk and CLI plugin links outlive `rasm:clean`
- Readiness checks without `timeout` wait while the process lives, `docker` row's timeout fails a start when the VM boots with its engine down
- `docker` row takes no `boot_start`, and nothing stops the engine on idle
- `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE` names the socket inside the engine's host, Ryuk mounts it and fails on Colima's client socket path
- Container images name `latest` with an always-pull policy, each run starts the newest image
- Harness rows hold service URLs and read no `[env]` value into MCP config
- Servers speaking stdio alone run as `rasm-mcp-bridge --listen 127.0.0.1:$PORT -- <command>`
- Servers starting one process per connection, or approving the connecting process by its code signature, keep a stdio row
- Pitchfork keeps daemon output in its log store, `mise daemons logs <name>` reads one daemon's output

Pitchfork has no login start, one `[bootstrap.macos.launchd.agents]` row runs its supervisor at login:
- Supervisor rows name no `stdout_path` or `stderr_path`, pitchfork writes its own file log and keeps output in its log store
- `PITCHFORK_SUPERVISOR_AUTO_START` is false on macOS, a client waits for the launchd supervisor and fails without one
- `mise bootstrap macos launchd-agents apply` writes and loads `dev.mise.pitchfork.plist`, an unchanged plist keeps its process
- `launchctl kickstart -k gui/$UID/dev.mise.pitchfork` sends SIGTERM, the supervisor stops its daemons and launchd starts it with its boot daemons

Daemon operations:
- `mise daemons restart [<name>…]` registers current rows and restarts named daemons onto installed tools, starting a stopped one
- `mise daemons stop [<name>…]` waits for each process group to exit, a stopped daemon stays down until a start
- `mise daemons register` writes current rows to pitchfork without starting, supervisor boot reads registered rows
- Linked worktrees take namespace `rasm-<hash>`, their fixed ports collide with the persistent checkout's daemons

## [04]-[MAINTENANCE]

- `.claude/skills/manage-repo/scripts/maintain.sh` runs from any directory and upgrades the checkout holding it
- `mise prune --yes` alone retires old mise installations
- Orphan removal runs outside Nx, a running Nx process loads modules from the version an upgrade orphans
- Root `upgrade` runs its prerequisites, then fails on each package a resolver moved below the version Git's index records for its lock
- `upgrade:docker` runs mise task `docker:update`, its VM restart on macOS starts the upgraded engine past systemd's `docker.service` start limit
- Generic cleanup keeps Pixi prefixes and `PULUMI_HOME` and installs no browser or tool
- Generic cleanup prunes caches and each Pulumi plugin no installed provider package declares
- Cleanup renames each target to a temporary path beside it before removing it, a concurrent writer recreates the original path
- `maintain.sh` quits VS Code before its fresh launch, the `code` CLI otherwise reuses a running instance and its old environment

## [05]-[TOML]

Tombi formats, lints, and validates TOML through `[tool.tombi]` in `pyproject.toml`:
- Tombi reads no `.editorconfig`, its indent width sits in `[tool.tombi]`
- Category dividers after a row take a blank line before them
- Schema-defined key and array ordering stays off, a sort discards the category and dependency order a file states
- Stock schemas missing a key a file sets take a `[[schemas]]` row: `root` at the key's table, its subschema as `path`, `strict = false`
- `tombi` skips files `.gitignore` excludes, named paths select files through its own CLI
