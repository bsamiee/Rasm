# [TOOLING]

Tool-general configuration is the environment every process reads, the toolchain manager, the task runner, and the harness and editor settings, each fact with one owner a reader reaches by directory walk or by hook.

## [01]-[ENVIRONMENT]

`mise.toml` `[env]` holds the process settings no manifest field can hold, and each process takes them through one path:
- Targets and scripts take the values from the shell hook or the shims, CI steps from the setup action, and the agent shell from the settings hooks
- `.claude/settings.json` registers the environment hook under `SessionStart` and `CwdChanged`, its output the preamble of every `Bash` command
- `.claude/settings.json` `env` sets `SHELL` to `bash`, so the `Bash` tool spawns bash 5.3 from PATH in place of the login zsh
- The plugin's child processes take the values from the session row's `env`
- Both channels run `mise` from the PATH `claude` was launched with
- Processes outside `Bash`, the plugin, and `mise exec` (the editor, the `dotnet dnx` servers) hold no `[env]` value or PATH addition
- `doppler run --project <project> --config <config> -- <command>` injects the config into the process without a shell on every operating system
- Run `mise env` after a `mise.toml` change, and read each changed value in its output
- The mise dotnet plugin exports `DOTNET_ROOT`, and a machine profile export turns the install into a link an SDK bump breaks

The `[env]` table holds `_.path = "./node_modules/.bin"`, the .NET no-logo and telemetry opt-out, `UV_PYTHON`, `PYTHONPYCACHEPREFIX`, and `NX_WORKSPACE_DATA_DIRECTORY`:
- `UV_PYTHON` renders the mise interpreter path with `tools = true`, and `.venv/bin/python3` reports it as `sys.base_prefix`
- `PYTHONPYCACHEPREFIX` sits in `[env]` because no `pyproject.toml` table sets it
- `NX_WORKSPACE_DATA_DIRECTORY` relocates the graph database under `.cache/nx/`, `nx.json` holds `cacheDirectory` alone
- `_.path = "./node_modules/.bin"` puts the pnpm lock copies first on PATH

## [02]-[TOOLCHAIN]

`mise.toml` `[settings]` and `[tools]` resolve every runtime and binary at its newest release, and the language lock files hold the only other pins:
- `prereleases = true` and `minimum_release_age = "0s"` take a release the day it appears, mise delays a new release 24h by default
- `idiomatic_version_file_enable_tools = ["dotnet"]` reads the SDK version from `global.json`, and no `[tools]` row names the SDK
- `python` is the one exact `[tools]` pin, `latest` and the major.minor prefix resolve the `-dev` build under prereleases, other rows are `latest`
- `python.uv_venv_auto = "source"` puts `.venv/bin` on PATH after `node_modules/.bin`, and the `uv.lock` copies of the checkers resolve by name
- `dotnet:` rows under `[tools]` add a PATH entry alone, and a NuGet tool package runs through `dotnet dnx <tool>` on the command
- `[tools]` rows name each binary by its registry short name, and a package with an importer or a config reader stays in its package manager
- `claude` is the native install from the Claude Code installer, self-updating, the one binary the `harness` target and the agent sessions run
- `claude` is no `[tools]` row: the docs list no mise route, and `claude update` recreates the native launcher beside any other copy

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

- The local plugin emits empty `lint`, `format`, `typecheck`, and `check` targets the defaults fill, and `nx-release-publish` for a tagged library
- When the root target lists `commands`, exclude the root project (`!<root>`) from the filtered entry, because a default `command` replaces the list
- `"..."` in a filtered entry's `inputs`, or in a manifest's `nx.targets.<target>.inputs`, spreads the inputs the plugin inferred
- `sharedGlobals` names `nx.json`, `mise.toml`, `tools/nx/*.ts`, `sgconfig.yml`, `tools/ast-grep/**/*`, and the `ast-grep --version` runtime input
- Every project reaches `sharedGlobals` through the `default` named input, and an edit to a shared file marks every project affected
- Extra arguments forward to the command, `nx run Native.Item:stage --rid linux-x64` reaches the script as `--rid=linux-x64`
- `defaultBase` names the base branch, and a pull request compares against it
- `neverConnectToCloud: true` keeps the cache local, and `cacheDirectory` sits under `.cache/nx/`
- `nx sync:check` enters the pipeline when a sync generator is registered, the `@nx/js/typescript` plugin registers one, and the workspace has none

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
- CommonJS default imports arrive as the module object under the native loader and as the class under swc, and one interop expression handles both

The staged tree enters the `pack` inputs through `dependentTasksOutputFiles`, and the ignored `.artifacts/` tree stays out of the workspace file map:

```json
{
    "pack": {
        "command": "dotnet pack eng/native/Native.Item --configuration Release --output .artifacts/nuget --nologo",
        "cache": true,
        "dependsOn": [{ "projects": ["Native.Item"], "target": "stage" }],
        "inputs": ["{projectRoot}/**/*", "{workspaceRoot}/eng/native/item/**/*", "{workspaceRoot}/global.json", { "dependentTasksOutputFiles": "**/*" }],
        "outputs": ["{workspaceRoot}/.artifacts/nuget/Native.Item.1.2.3.nupkg", "{workspaceRoot}/.artifacts/native/msbuild/{bin,obj}/Native.Item"]
    }
}
```

## [04]-[ROOT_TARGETS]

Root targets exist when the root manifest `nx` field declares them, one per operation, with no owning project, and the root project holds the tag of its manifest's language:

| [INDEX] | [TARGET]    | [RUNS]                                                                                                         | [CACHE] |
| :-----: | :---------- | :------------------------------------------------------------------------------------------------------------- | :-----: |
|  [01]   | `restore`   | `dotnet restore <solution>`, the one restore the .NET `build`, `format`, and publish defaults need             | `true`  |
|  [02]   | `grammar`   | `tree-sitter build` of the XML grammar under `.cache/ast-grep/`, the `lint` defaults depend on it              | `true`  |
|  [03]   | `lint`      | Biome, `actionlint`, `ast-grep test --include-off`, and `ast-grep scan` over the root files and the tool trees | `true`  |
|  [04]   | `format`    | `biome format --write` over the root files and the tool trees                                                  | `true`  |
|  [05]   | `check`     | Nothing, the tag-filtered `check` default fills it                                                             |  Unset  |
|  [06]   | `typecheck` | `tsc --build` over the root configuration files, the plugin files, and the infrastructure program              | `true`  |
|  [07]   | `up`        | `doppler run` around the infrastructure program's `up`                                                         | `false` |
|  [08]   | `refresh`   | `doppler run` around the infrastructure program's `refresh`                                                    | `false` |
|  [09]   | `coverage`  | Coverage script, one language's reports merged                                                                 | `false` |
|  [10]   | `rewrite`   | `ast-grep scan --filter '^<id>$' --error=<id> -U <paths>` from `--id` and `--paths`, after `grammar`           | `false` |
|  [11]   | `mutation`  | Mutation script                                                                                                | `false` |
|  [12]   | `upgrade`   | `uv lock --upgrade`, `pnpm update --latest --recursive`, and dotnet-outdated under `dotnet dnx`                | `false` |
|  [13]   | `workflow`  | `act push` over the Linux jobs                                                                                 | `false` |
|  [14]   | `harness`   | Harness script, `.claude/types` regenerated, the plugin proven by its load line, its cached copy reinstalled   | `false` |

- `upgrade` moves every language's dependency set to its newest release, prereleases included, and every command writes a shared file
- `upgrade` runs `dotnet dnx dotnet-outdated-tool --yes -- --upgrade --pre-release Always --no-restore <solution>` for the .NET set
- `up`, `refresh`, `upgrade`, `workflow`, and `harness` set `parallelism: false`, each writes a shared file or shares one daemon
- `harness` depends on the `function-hooks` `lint`, `format`, and `test` targets in the object form, then `tsc -p` and `claude plugin validate`
- `restore` inputs are the solution, the project and directory files, `NuGet.config`, and `global.json`, and its outputs the `obj/` restore files

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

## [06]-[HARNESS]

The agent harness, the editor, and git read their own root configuration files, each keyed by language, file type, or event:
- `.claude/settings.json` enables `function-hooks@rasm` in `enabledPlugins`
- `extraKnownMarketplaces.rasm` names the marketplace at `./.claude/plugins`, and a clone gets every plugin from that entry
- Proofs run the target itself under the hook's environment
- `claudeMdExcludes` keeps the `CLAUDE.md` files under caches, `node_modules`, and build outputs out of the context
- `.vscode/settings.json` keys the formatter by language id, points the Biome server at the workspace copy per platform, and reads the `.venv` copies
- `files.associations` maps `SKILL.md` and the agent files back to markdown, and `files.exclude` hides every cache, output, and dependency directory
- `.mcp.json` holds every server but the Yak router and the `computer-use` connector, `dotnet dnx` for .NET and `type: http` for remote
- `~/.claude.json` enables `computer-use` per project under `enabledMcpServers`, which the harness script reads
- `${VAR}` headers on the servers that take a token expand from the environment of the `claude` launch
- `doppler run --project agent-runtime --config dev -- claude` supplies every token header
- `mise exec` puts `_.path` first on the child's PATH, the npm servers run from `node_modules/.bin` and ast-grep from its `pipx:` row
- The ast-grep entry sets `AST_GREP_CONFIG` to `${CLAUDE_PROJECT_DIR:-.}/sgconfig.yml`, and the plugin's `mise x` row binds Bash commands alone
- `.gitattributes` normalizes text to LF and stores binary design assets as Git LFS pointers by extension

## [07]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                                      | [CORRECT_FORM]                                               |
| :-----: | :----------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `test: {}` plugin options or an option restating its default | No option, an empty object renames and merges nothing        |
|  [02]   | `mise x <command>` or a mise task wrapping a target          | Target under the mise environment from the hook or the shims |
|  [03]   | Versions in `mise.toml`, `mise.lock`, or a release-age delay | `latest`, with the language lock files as the only pins      |
|  [04]   | `[env]` rows for a setting a manifest table holds            | Manifest the tool reads by directory walk (`UV_CACHE_DIR`)   |
|  [05]   | `[env]` rows for a directory one script or program computes  | Script or program derives it beside its other paths          |
|  [06]   | Binary-only npm packages in the catalog and `allowBuilds`    | `[tools]` rows at `latest`                                   |
|  [07]   | Script file for a hook the documentation states as a command | The command itself in `.claude/settings.json`                |
|  [08]   | Settings deny glob for a phrase                              | Parsed-command row in the plugin's Bash table                |
