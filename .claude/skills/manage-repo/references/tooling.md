# [TOOLING]

Configure the shared process environment, toolchain, task runner, harness, and editor in the files each consumer reads.

## [01]-[ENVIRONMENT]

`mise.toml` `[env]` holds the process settings no manifest field can hold, and each process takes them through one path:
- Targets and scripts take the values from the shell hook or the shims, CI steps from the setup action, and the agent shell from the settings hooks
- Outside an activated shell, use `mise exec -- <command>` to select the configured runtime and environment
- `.claude/settings.json` registers the environment hook under `SessionStart` and `CwdChanged`, its output the preamble of every `Bash` command
- The plugin rewrites a `mise x`, `mise exec`, or `eval "$(mise env -s bash)" &&` leaf to the command it wraps, and reads inside a `sh -c` wrapper
- `.claude/settings.json` `env` sets `SHELL` to `bash`, and the `Bash` tool spawns bash 5.3 from PATH in place of the login zsh
- The plugin's child processes take the values from the session row's `env`
- Both channels run `mise` from the PATH `claude` was launched with
- Editors and MCP servers inherit their launch environment, and `mise exec` in `.mcp.json` gives each server the workspace PATH
- `doppler run --project <project> --config <config> -- <command>` injects the config into the process without a shell on every operating system
- Run `mise env` after a `mise.toml` change, and read each changed value in its output
- The mise dotnet plugin exports `DOTNET_ROOT`, and a machine profile export turns the install into a link an SDK bump breaks

Each `[env]` row holds a value with a reason its comment names:
- `UV_PYTHON` renders the mise interpreter path with `tools = true`, and `.venv/bin/python3` reports it as `sys.base_prefix`
- `UV_CACHE_DIR` overrides the machine profile's exported cache path, which wins over `cache-dir` in `pyproject.toml`
- `NX_WORKSPACE_DATA_DIRECTORY` relocates the graph database under `.cache/nx/`, `nx.json` holds `cacheDirectory` alone
- `_.path` resolves command names to the workspace package versions
- `LIBKTX_VERSION` renders `exec` of `yq -r .version-string` over `eng/native/ktx/release.json` under `tools = true`
- `LIBKTX_INSTALL_DIR` is the provision link `.cache/tools/ktx`, and Windows derives the `include`, `lib`, and DLL directories from it
- `mise.unix.toml` holds the POSIX-only rows `LIBKTX_INCLUDE_DIR` and `LIBKTX_LIB_DIR`, and `LIBKTX_LIB_DIR` stays unset on Windows
- `.miserc.toml` alone holds `auto_env = true`, the early-init setting that loads `mise.unix.toml`, a `[settings]` row in `mise.toml` has no effect

## [02]-[TOOLCHAIN]

Use `mise.toml` `[settings]` and `[tools]` to resolve runtimes and binaries:
- `prereleases = true` and `minimum_release_age = "0s"` take a release the day it appears, mise delays a new release 24h by default
- `idiomatic_version_file_enable_tools = ["dotnet"]` reads the SDK version from `global.json`, and no `[tools]` row names the SDK
- `python` is the one exact `[tools]` pin, `latest` and the major.minor prefix resolve the `-dev` build under prereleases, other rows are `latest`
- `python.uv_venv_auto = "source"` puts `.venv/bin` on PATH after `node_modules/.bin`, and the `uv.lock` copies of the checkers resolve by name
- `mise which` reports a package binary (`biome`, `tsc`, `ruff`) as no mise bin, and `pnpm exec <binary>` or `uv run <tool>` resolves it
- `dotnet:` rows under `[tools]` add a PATH entry alone, and a NuGet tool package runs through `dotnet dnx <tool>` on the command
- `pkgx:gnu.org/bash` supplies Bash 5.3 on Linux and macOS, and its backend requires `experimental = true`
- Preserve the Bash tool's postinstall correction until the pkgx backend generates a launcher with a nonrecursive interpreter
- `yq` resolves to Mike Farah's YAML processor, and `jq` resolves to jqlang's JSON processor
- `ripgrep` supplies `rg`, and `difftastic` supplies `difft`
- `ruff`, `sqlfluff`, and `yamllint` are `eng` group rows the quality script resolves through the `.venv`
- `pg-formatter`'s postinstall fetches the pgFormatter script its binary runs, a root devDependency under `allowBuilds`
- `pg-formatter` writes the SQL layout, and `[tool.sqlfluff.core]` excludes the layout rules
- Let the pipx backend order its configured Python and uv dependencies before installation
- `claude` is the self-updating native install and no `[tools]` row, `claude update` recreates the launcher beside any other copy
- `mise` itself is the nix home-manager binary of the machine profile, and its upgrade belongs there
- `mise.lock` pins every `latest` row to the version it held at lock time, and `[settings] lockfile` stays off
- `[tool_alias]` names a version alias, `[plugins]` repoints a tool to a backend, and `mise install --yes` answers every confirmation prompt

Binary rows are complete when each criterion holds under `manage-repo` approach principles, proven in order by the tooling maintainer:
- No MCP server, installed tool, or package covers the need, and the row's comment names the consumer where the tool name does not
- The backend is the entry `mise registry | rg '^<name> '` prints, or `ubi:<owner>/<repo>`, `pipx:<package>`, or `npm:<package>` without one
- The row sits at `latest` in its group of `[tools]`, and `mise which <binary>` prints a path under the mise install directory
- Checkers sit in the `lint` row of their file kind in `eng/scripts/quality.py` and writers in the `format` row, the version in the runtime input
- Its settings with no central file sit on its command in the kind row
- `Bash(<binary> *)` sits in the `.claude/settings.json` allow list, and binaries agents run by hand take a `[CLI_TOOLING]` row in `CLAUDE.md`

Use native command options to select and consume results without parsing display text:
- Use `fd -t f -e <extension>` for file selection and `-g` for filename globs, with `-p` when matching full paths
- Use `fd ... -X <command>` to pass paths directly in batches, or `-x` for independent per-file operations
- Put execution arguments last, and use `--batch-size` when the consumer limits files per invocation
- Use `rg -F -e <text>` for literals, `-l` for matching files, and `-q` when only existence matters
- Preserve filename boundaries with `fd -0` or `rg -l -0` when another process consumes a path stream
- Use `rg --json` for structured matches, and distinguish matching lines from occurrence counts

`fd -X` runs nothing when no path matches, splits a batch at the argument limit, and orders nothing. Sequential per-file runs take `--threads=1` before `-x`. `fd <pattern> <path>...` takes the pattern first, and `.` matches every name when the scope is the path. `fd -p` matches the pattern against the absolute path of a named search path, smart case matches a parent directory of another case, and a pattern over a directory name takes `-s`. `fd -H` includes hidden paths, `-I` includes ignored paths, and `rg --no-config` ignores the options `RIPGREP_CONFIG_PATH` names.

## [03]-[TASK_RUNNER]

`nx.json` registers each plugin by path and holds the named inputs and the tag-filtered defaults:

```json
{
    "plugins": [
        { "plugin": "@nx/vite/plugin", "exclude": ["vite.config.ts"] },
        { "plugin": "@nx/vitest", "exclude": ["vite.config.ts", "vitest.config.ts"] },
        {
            "plugin": "@nx/dotnet",
            "exclude": ["eng/native/**"],
            "options": {
                "clean": false,
                "pack": false,
                "publish": false,
                "restore": false,
                "watch": { "dependsOn": [{ "projects": ["<root>"], "target": "restore" }] }
            }
        },
        { "plugin": "./tools/nx/workspace.ts" }
    ]
}
```

- The local plugin emits empty `typecheck` and `check` targets the defaults fill, and `nx-release-publish` for a tagged library
- Defaults with a `command` replace a root target's `commands` list, and the filtered entry excludes the root project (`!<root>`) in that case
- `"..."` in a filtered entry's `inputs`, or in a manifest's `nx.targets.<target>.inputs`, spreads the inputs the plugin inferred
- `sharedGlobals` names the runner, toolchain, and rule files every target reads, with the `ast-grep --version` runtime input
- `default` includes `sharedGlobals`, and an edit to a shared file marks every project affected
- Targets with explicit inputs keep every shared input their command reads
- Extra arguments forward to the command, `nx run Native.Item:stage --rid linux-x64` reaches the script as `--rid=linux-x64`
- `defaultBase` names the base branch, and a pull request compares against it
- `nx affected` compares the working tree with `defaultBase`, and a committed tree selects nothing
- Untracked files mark their project under `nx affected`, and `--uncommitted` skips them
- `neverConnectToCloud: true` keeps the cache local, and `cacheDirectory` sits under `.cache/nx/`
- `useDaemonProcess: false` turns the daemon off, its `nx=debug` outputs watcher ignores nothing under `.cache/nx/` and its log never rotates
- `nx sync:check` enters the pipeline when a sync generator is registered, the `@nx/js/typescript` plugin registers one, and the workspace has none

Each part of the task graph has one file, and a fact appears in one of them:

| [INDEX] | [FILE]             | [HOLDS]                                                  | [NEVER_HOLDS]                                     |
| :-----: | :----------------- | :------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `namedInputs`      | `default`, `production`, `sharedGlobals`, `<language>`   | Root file paths in two inputs                     |
|  [02]   | `targetDefaults`   | Cache flag, one tag-filtered entry per language          | One-project entries, a per-file checker or writer |
|  [03]   | `plugins`          | Plugin path, `exclude`, options off their default        | `test: {}`, it renames and merges nothing         |
|  [04]   | Root manifest `nx` | Tag, `tooling` input, root targets, composed `check`     | Filtered default bodies, a repeated file list     |
|  [05]   | `project.json`     | `name`, tags, `provision`, empty targets                 | Target bodies a default supplies                  |

- `targetDefaults` filter by `tag:language:<language>`, and a project reaches a default through its tag alone
- `targetDefaults` takes an executor name as a key (`"nx:run-commands": {"options": ...}`) with no `executor` pair, in the filtered array form too
- Named configurations merge over `options`, `{"parallel": true}` alone runs the base `commands` list in parallel, and own `commands` inherit none
- `readyWhen` beside `commands` is valid under `parallel: true` alone, and keeping it under `parallel: false` writes a form the executor refuses
- The run-commands executor spawns the `command` string through `sh`, and a glob or `$( )` expands at run time
- Targets name the language input (`dotnet`, `python`, `typescript`) in `inputs`
- Sibling files of one directory join as one brace glob, `Directory.{Build.props,Build.targets,Packages.props}`
- One command sits under `command` with `forwardAllArgs` beside it, and `commands` holds plain strings for two or more
- File lists two root targets read are named inputs under the manifest's `nx.namedInputs`, and each target names its input once

`format` writes source with no restorable output, a reverted input runs its corrections again, and the target stays uncached. Preserve inferred dependencies with `"..."`, `^typecheck` included, when extending a target.

Root `lint` and `format` take the scope as positionals with no `--`:
- `nx format` is the built-in `format:write` command, and the `nx <target> <project>` shorthand exists for target names without a built-in
- `eng/scripts/quality.py` lists the tree once through `git ls-files`, and each token keeps the files of a kind word by suffix or of a path by prefix
- Each kind row runs its tools once over the files in scope, files last on the command, and a kind without files runs nothing
- The root `check` forwards its scope to `lint` through `{ "target": "lint", "params": "forward" }` and runs `typecheck` with none
- `upgrade` takes a language as a configuration, `pnpm exec nx run <root>:upgrade:<python|typescript|dotnet>`, and the bare target runs every manager
- Nx hashes the scope with the task, and a scoped run and a tree run each cache under their own key

`tools/nx/workspace.ts` exports `createNodes` over one glob and `createDependencies` beside it:

```ts
const createNodes: CreateNodes = [
    '{**/*.csproj,{apps,libs,tests}/**/tsconfig.json,{libs/python,apps/*}/*/__init__.py}',
    (files, options, context) => createNodesFromFiles((file) => nodeFor(files, file), files, options, context),
];
const createDependencies: CreateDependencies = (_options, context) => packageReferenceEdges(context);
```

- `createNodesFromFiles` collects each file's failure per file, and the packaging nodes share one cached read of `NuGet.config`
- `createDependencies` turns each `PackageReference` from a changed project file to a packaging project into a static edge
- Nx keeps the cached edges of every file outside `filesToProcess` and validates each edge as the graph builder adds it
- Nx runs each plugin in an isolated worker and loads `.ts` plugins and version actions through Node type stripping
- The swc loader fails under the native `typescript` compiler, and `NX_PREFER_NODE_STRIP_TYPES` stays unset
- The release actions' CommonJS default import differs between native and swc loading, and its interop expression handles both

The staged tree enters the `pack` inputs through `dependentTasksOutputFiles`, and the ignored `.artifacts/` tree stays out of the workspace file map:

```json
{
    "pack": {
        "command": "dotnet pack eng/native/Native.Item --configuration Release --output .artifacts/nuget",
        "cache": true,
        "dependsOn": [{ "projects": ["Native.Item"], "target": "stage" }],
        "inputs": [
            "{projectRoot}/**/*",
            "{workspaceRoot}/eng/native/Directory.Build.*",
            "{workspaceRoot}/eng/native/item/**/*",
            "dotnet",
            { "runtime": "dotnet --version" },
            { "dependentTasksOutputFiles": "**/*" }
        ],
        "outputs": ["{workspaceRoot}/.artifacts/nuget/Native.Item.1.2.3.nupkg", "{workspaceRoot}/.artifacts/native/msbuild/{bin,obj}/Native.Item"]
    }
}
```

## [04]-[ROOT_TARGETS]

Declare operations shared across projects in the root manifest's `nx` field. Tag the root project with its manifest's language:

| [INDEX] | [TARGET]    | [RUNS]                                                                                                          | [CACHE] |
| :-----: | :---------- | :-------------------------------------------------------------------------------------------------------------- | :------ |
|  [01]   | `restore`   | `dotnet restore <solution>`, the one restore the .NET `build`, `format`, and publish defaults need              | `true`  |
|  [02]   | `grammar`   | `tree-sitter build` of the XML grammar under `.cache/ast-grep/`, the scanning targets depend on it              | `true`  |
|  [03]   | `lint`      | Quality script `lint`, the checkers of every file kind in the scope                                             | `true`  |
|  [04]   | `format`    | Quality script `format`, the writers of every file kind in the scope                                            | `false` |
|  [05]   | `check`     | `lint` with the scope forwarded, then `typecheck`, `tsc --build` over the root configuration and program files | `true`  |
|  [06]   | `up`        | `doppler run` around the infrastructure program's `up`                                                          | `false` |
|  [07]   | `refresh`   | `doppler run` around the infrastructure program's `refresh`                                                     | `false` |
|  [08]   | `coverage`  | Coverage script, one language's reports merged                                                                  | `false` |
|  [09]   | `rewrite`   | `ast-grep scan -U` with the filter, error, and path arguments the root `README.md` invocation passes after `--` | `false` |
|  [10]   | `mutation`  | Mutation script                                                                                                 | `false` |
|  [11]   | `upgrade`   | `uv lock --upgrade`, `pnpm update --latest --recursive`, and dotnet-outdated, one per configuration            | `false` |
|  [12]   | `workflow`  | Workflow script, `act` over `ci.yml` on the host architecture, the job containers removed on every exit         | `false` |
|  [13]   | `harness`   | Harness script, `.claude/types` regenerated, the plugin proven by its load line, its cached copy reinstalled    | `false` |
|  [14]   | `rules`     | `ast-grep test --include-off`                                                                                   | `true`  |
|  [15]   | `outline`   | `ast-grep outline` with every outline rule under `tools/ast-grep/outline/`                                      | `false` |

`lint` depends on `rules` and `grammar`, `format` on `restore`. Both take the tree with `.github/` and `.claude/` as inputs beside the script's tool versions.

`tooling`, the named input of root `typecheck`, holds the files root `tsconfig.json` compiles.

`nx run rasm:outline -- <paths> --items structure` outlines repository source, and the native outline options pass through unchanged. Use `ast-grep` for one rule's checks, the extractor design, and the views.

- `upgrade` moves every language's dependency set to its newest release, prereleases included
- `up`, `refresh`, `upgrade`, `workflow`, and `harness` set `parallelism: false`, each writes a shared file or shares one daemon
- `harness` depends on the `function-hooks` `test` target before regenerating declarations and compiling the plugin

## [05]-[RELEASE]

The `release` field versions each project independently from its `<projectName>@<version>` tag, and the git-tag version actions read and write no manifest:
- `projectsRelationship: independent` at the root, and a fixed group takes one `{releaseGroupName}@{version}` tag for its projects
- `versionActions: tools/nx/version-actions.ts` sets `validManifestFilenames` null and answers `0.0.0` for a project with no tag
- `fallbackCurrentVersionResolver: "disk"` and `automaticFromRef: true` make a first release need no `--first-release`
- `conventionalCommits: true` derives the bump from the commits since the tag, `updateDependents: "never"` bumps no dependent
- `workspaceChangelog: false` and `projectChangelogs.file: false` write no file, `createRelease: "github"` needs `git.push: true`
- `git` commits nothing, tags, and pushes
- Groups select by `tag:release:<language>`, and the local plugin tags each library from its language or its `ReleaseGroup` property
- `nx release --yes` and `--skip-publish` exclude each other, and the publish step is its own `nx release publish` command
- One dispatch workflow runs `nx release --skip-publish` then `nx release publish`
- Custom release commands must consume `NX_DRY_RUN` through their CLI parser before executing publication

## [06]-[HARNESS]

Configure the agent harness, editor, and git by language, file type, or event:
- `.claude/settings.json` enables `function-hooks@rasm` in `enabledPlugins`
- `extraKnownMarketplaces.rasm` names the marketplace at `./.claude/plugins`, and a clone gets every plugin from that entry
- Test targets under the hook's environment
- `claudeMdExcludes` keeps the `CLAUDE.md` files under caches, `node_modules`, and build outputs out of the context
- `.vscode/settings.json` keys the formatter by language id, Biome resolves the workspace package, and Python tools read the `.venv` copies
- `files.associations` maps `SKILL.md` and the agent files back to markdown, and `files.exclude` hides every cache, output, and dependency directory
- `.mcp.json` holds every server but the Yak router and the `computer-use` connector, `dotnet dnx` for .NET and `type: http` for remote
- `~/.claude.json` enables `computer-use` per project under `enabledMcpServers`, which the harness script reads
- `${VAR}` headers on the servers that take a token expand from the environment of the `claude` launch
- `doppler run --project agent-runtime --config dev -- claude` supplies every token header
- `mise exec` puts `_.path` first on each server's PATH, and the servers run from `node_modules/.bin`, the ast-grep `pipx:` row, or `dotnet dnx`
- The ast-grep entry sets `AST_GREP_CONFIG` to `${CLAUDE_PROJECT_DIR:-.}/sgconfig.yml`, and the plugin's `mise x` rows rewrite Bash commands alone
- `.gitattributes` normalizes text to LF and stores binary design assets as Git LFS pointers by extension

## [07]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                                      | [CORRECT_FORM]                                                          |
| :-----: | :----------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `mise x <command>` or a mise task wrapping a target          | Direct command under the mise environment from the hook or the shims    |
|  [02]   | Unneeded runtime pin or release-age delay                    | `latest`, with documented SDK and interpreter constraints               |
|  [03]   | Duplicated manifest settings in `[env]`                      | Manifest setting, or an explicit override of an inherited process value |
|  [04]   | `[env]` rows for a directory one script or program computes  | Script or program derives it beside its other paths                     |
|  [05]   | Binary-only npm packages in the catalog and `allowBuilds`    | `[tools]` rows at `latest`                                              |
|  [06]   | Script file for a hook the documentation states as a command | The command itself in `.claude/settings.json`                           |
|  [07]   | Settings deny glob for a phrase                              | Parsed-command row in the plugin's Bash table                           |
