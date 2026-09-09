# [TOOLING]

Configure the shared process environment, toolchain, task runner, harness, and editor in the files each consumer reads.

## [01]-[ENVIRONMENT]

Each process takes `mise.toml` `[env]` values through one path:
- Targets and scripts take values from the shell hook or shims, CI steps from the setup action, and the agent shell from the settings hooks
- Outside an activated shell, use `mise exec -- <command>` to select the configured runtime and environment
- `.claude/settings.json` registers the environment hook under `SessionStart` and `CwdChanged`, its output the preamble of every `Bash` command
- function-hooks plugin strips a `mise x`, `mise exec`, or `eval "$(mise env -s bash)" &&` prefix from Bash commands, inside `sh -c` included
- `SHELL=bash` in `.claude/settings.json` `env` makes the `Bash` tool spawn bash from PATH in place of the login zsh
- Plugin child processes take values from the session row `env`
- Hook and plugin run `mise` from the PATH of the `claude` launch
- Editors and MCP servers inherit the launch environment
- `doppler run --project <project> --config <config> -- <command>` injects the config into the process without a shell on every operating system
- Run `mise env` after a `mise.toml` change and read each changed value in its output
- mise dotnet plugin exports `DOTNET_ROOT`, a machine profile export turning the install into a link an SDK bump breaks

Each `[env]` row holds a value with the reason in its comment:
- `UV_PYTHON` renders the mise interpreter path with `tools = true`, proven by `sys.base_prefix` of `.venv/bin/python3`
- `UV_CACHE_DIR` overrides the machine profile export, `cache-dir` in `pyproject.toml` cannot
- `NX_WORKSPACE_DATA_DIRECTORY` relocates the graph database under `.cache/nx/`, `nx.json` holds `cacheDirectory` alone
- `_.path` resolves command names to the workspace package versions
- `LIBKTX_VERSION` renders `exec` of `yq -r .version-string` over `eng/native/ktx/release.json` under `tools = true`
- Windows derives the `include`, `lib`, and DLL directories from `LIBKTX_INSTALL_DIR`, the provision link `.cache/tools/ktx`
- `mise.unix.toml` holds the POSIX-only rows `LIBKTX_INCLUDE_DIR` and `LIBKTX_LIB_DIR`
- `PLAYWRIGHT_BROWSERS_PATH` overrides the machine profile export, `playwright install` writes and `playwright mcp` launches the build there
- `PLAYWRIGHT_MCP_USER_DATA_DIR` names the persistent profile under `{{ xdg_state_home }}`, one signed-in browser for every checkout
- Persistent profile holds one browser at a time, a second server on it fails its first browser call naming `--isolated`
- First login runs headed once, `playwright open --browser chromium --user-data-dir "$PLAYWRIGHT_MCP_USER_DATA_DIR" <url>`
- `.miserc.toml` alone holds `auto_env = true`, the early-init setting that loads `mise.unix.toml`
- `auto_env` under `mise.toml` `[settings]` has no effect

## [02]-[TOOLCHAIN]

`mise.toml` `[settings]` and `[tools]` resolve runtimes and binaries:
- `prereleases = true` and `minimum_release_age = "0s"` take a release the day it appears, mise delays a new release 24h by default
- `idiomatic_version_file_enable_tools = ["dotnet"]` reads the SDK version from `global.json`, no `[tools]` row naming the SDK
- `python` is the one exact `[tools]` pin, `latest` and a major.minor prefix resolve the `-dev` build under prereleases
- `python.uv_venv_auto = "source"` puts `.venv/bin` on PATH after `node_modules/.bin`, the `uv.lock` copies of the checkers resolving by name
- `mise which` reports a package binary (`biome`, `tsc`, `ruff`) as no mise bin, `pnpm exec <binary>` or `uv run <tool>` resolves it
- `dotnet:` rows under `[tools]` add a PATH entry alone
- `pkgx:gnu.org/bash` supplies bash on Linux and macOS
- pkgx backend requires `experimental = true`
- Keep the bash row's postinstall correction until the pkgx backend generates a launcher with a nonrecursive interpreter
- `yq` resolves to Mike Farah's YAML processor, `jq` to jqlang's
- `ripgrep` and `difftastic` supply `rg` and `difft`
- `ruff`, `sqlfluff`, and `yamllint` are `eng` group rows the quality script resolves through the `.venv`
- `pg-formatter`, a root devDependency under `allowBuilds`, fetches the pgFormatter script at postinstall
- `[tool.sqlfluff.core]` excludes the layout rules `pg-formatter` writes
- pipx backend orders its configured Python and uv dependencies before installation
- `claude` is the self-updating native install and no `[tools]` row, `claude update` recreates the launcher beside any other copy
- `mise` is the nix home-manager binary of the machine profile, upgraded there
- `[settings] lockfile` stays off, `mise.lock` pins every `latest` row to the version it held at lock time
- `[tool_alias]` names a version alias
- `[plugins]` repoints a tool to a backend
- `mise install --yes` answers every confirmation prompt

Binary rows are complete when each criterion holds, proven in order:
- No MCP server, installed tool, or package covers the need
- Row comment names the consumer where the tool name does not
- Backend is the entry `mise registry | rg '^<name> '` prints, or `ubi:<owner>/<repo>`, `pipx:<package>`, or `npm:<package>` without one
- Row sits at `latest` in its `[tools]` group
- `mise which <binary>` prints a path under the mise install directory
- `eng/scripts/quality.py` holds the tool in the row of its file kind, with the version in the runtime input
- `Bash(<binary> *)` sits in the `.claude/settings.json` allow list
- Binaries agents run by hand take a `[CLI_TOOLING]` row in `CLAUDE.md`

Use native command options to select and consume results without parsing display text:
- Use `fd -t f -e <extension>` for file selection and `-g` for filename globs, with `-p` when matching full paths
- Use `fd ... -X <command>` to pass paths in batches, or `-x` for independent per-file operations
- Put execution arguments last
- Use `--batch-size` when the consumer limits files per invocation
- Use `rg -F -e <text>` for literals, `-l` for matching files, and `-q` when only existence matters
- Preserve filename boundaries with `fd -0` or `rg -l -0` when another process consumes a path stream
- Use `rg --json` for structured matches
- Distinguish matching lines from occurrence counts
- `fd -X` runs nothing when no path matches, splits a batch at the argument limit, and orders nothing
- Sequential per-file runs take `--threads=1` before `-x`
- `fd <pattern> <path>...` takes the pattern first, `.` matching every name when the scope is the path
- `fd -p` matches the absolute path of a named search path
- `fd -s` on a directory-name pattern stops smart case matching a parent directory of another case
- `fd -H` includes hidden paths, `-I` ignored paths
- `rg --no-config` ignores the options `RIPGREP_CONFIG_PATH` names

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

- Local plugin emits empty `typecheck` and `check` targets the defaults fill, and `nx-release-publish` for a tagged library
- Defaults with a `command` replace a root target's `commands` list, the filtered entry excluding the root project (`!<root>`)
- `"..."` in a filtered entry's `inputs` or a manifest's `nx.targets.<target>.inputs` spreads the inferred inputs
- `sharedGlobals` names the runner, toolchain, and rule files every target reads, with the `ast-grep --version` runtime input
- `sharedGlobals` inside `default` makes an edit to a shared file mark every project affected
- Targets with explicit inputs keep every shared input the command reads
- Extra arguments forward to the command, `nx run Native.Item:stage --rid linux-x64` reaches the script as `--rid=linux-x64`
- `defaultBase` names the base branch a pull request compares against
- `nx affected` compares the working tree with `defaultBase`, a committed tree selecting nothing
- Untracked files mark their project under `nx affected`, `--uncommitted` skips them
- `neverConnectToCloud: true` keeps the cache local
- `cacheDirectory` sits under `.cache/nx/`
- `useDaemonProcess: false` turns the daemon off, its `nx=debug` outputs watcher ignores nothing under `.cache/nx/` and its log never rotates
- `nx sync:check` enters the pipeline when a sync generator is registered, the `@nx/js/typescript` plugin registers one, and the workspace has none

Each part of the task graph has one file:

| [INDEX] | [FILE]             | [HOLDS]                                                  | [NEVER_HOLDS]                                     |
| :-----: | :----------------- | :------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `namedInputs`      | `default`, `production`, `sharedGlobals`, `<language>`   | Root file paths in two inputs                     |
|  [02]   | `targetDefaults`   | Cache flag, one tag-filtered entry per language          | One-project entries, a per-file checker or writer |
|  [03]   | `plugins`          | Plugin path, `exclude`, options off their default        | `test: {}` with no rename or merge                |
|  [04]   | Root manifest `nx` | Tag, `tooling` input, root targets, composed `check`     | Filtered default bodies, a repeated file list     |
|  [05]   | `project.json`     | `name`, tags, `provision`, empty targets                 | Target bodies a default supplies                  |

- Projects reach a `targetDefaults` entry filtered by `tag:language:<language>` through their tag alone
- `targetDefaults` keys an executor name (`"nx:run-commands": {"options": ...}`) with no `executor` pair, in the object and the filtered array form
- Named configurations merge over `options`, `{"parallel": true}` alone runs the base `commands` list in parallel, and own `commands` inherit none
- `readyWhen` beside `commands` is valid under `parallel: true` alone
- `sh` runs the run-commands `command` string and expands a glob or `$( )` at run time
- Targets name the language input (`dotnet`, `python`, `typescript`) in `inputs`
- Sibling files of one directory join as one brace glob, `Directory.{Build.props,Build.targets,Packages.props}`
- `command` holds a single command with `forwardAllArgs` beside it, `commands` plain strings otherwise
- File lists shared by root targets are named inputs under the manifest `nx.namedInputs`, each target naming its input once

`format` writes source with no restorable output, a reverted input runs the corrections again. When extending a target, preserve inferred dependencies with `"..."`, `^typecheck` included.

Root `lint` and `format` take the scope as positionals with no `--`:
- `nx format` is the built-in `format:write` command, the `nx <target> <project>` shorthand exists for target names without a built-in
- `eng/scripts/quality.py` lists the tree through `git ls-files`, each token keeping files of a kind word by suffix or of a path by prefix
- Each kind row runs its tools once over the files in scope, files last on the command, a kind without files running nothing
- Root `check` forwards the scope to `lint` through `{ "target": "lint", "params": "forward" }` and runs `typecheck` with none
- `pnpm exec nx run <root>:upgrade:<python|typescript|dotnet>` runs one manager, the target with no configuration runs every manager
- Nx hashes the scope with the task, a scoped run and a tree run each caching under their own key

`tools/nx/workspace.ts` exports `createNodes` over one glob and `createDependencies` beside it:

```ts
const createNodes: CreateNodes = [
    '{**/*.csproj,{apps,libs,tests}/**/tsconfig.json,{libs/python,apps/*}/*/__init__.py}',
    (files, options, context) => createNodesFromFiles((file) => nodeFor(files, file), files, options, context),
];
const createDependencies: CreateDependencies = (_options, context) => packageReferenceEdges(context);
```

- `createNodesFromFiles` collects each file's failure per file
- Packaging nodes share one cached read of `NuGet.config`
- `createDependencies` turns each `PackageReference` from a changed project file to a packaging project into a static edge
- Nx keeps the cached edges of every file outside `filesToProcess` and validates each edge as the graph builder adds it
- Nx runs each plugin in an isolated worker and loads `.ts` plugins and version actions through Node type stripping
- `NX_PREFER_NODE_STRIP_TYPES` stays unset, the swc loader fails under the native `typescript` compiler
- Release actions' CommonJS default import differs between native and swc loading, an interop expression handling each

Ignored `.artifacts/` tree stays out of the workspace file map, `pack` inputs name it through `dependentTasksOutputFiles`:

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

Root project takes the tag of its manifest language.

| [INDEX] | [TARGET]    | [RUNS]                                                                                                          | [CACHE] |
| :-----: | :---------- | :-------------------------------------------------------------------------------------------------------------- | :------ |
|  [01]   | `restore`   | `dotnet restore <solution>`                                                                                     | `true`  |
|  [02]   | `grammar`   | `tree-sitter build` of the XML grammar under `.cache/ast-grep/`, the scanning targets depend on it              | `true`  |
|  [03]   | `lint`      | Quality script `lint`                                                                                           | `true`  |
|  [04]   | `format`    | Quality script `format`                                                                                         | `false` |
|  [05]   | `check`     | `lint` then `typecheck`, `tsc --build` over root `tsconfig.json`                                                | `true`  |
|  [06]   | `up`        | Infrastructure program `up`                                                                                     | `false` |
|  [07]   | `refresh`   | Infrastructure program `refresh`                                                                                | `false` |
|  [08]   | `coverage`  | Coverage script, one language's reports merged                                                                  | `false` |
|  [09]   | `rewrite`   | `ast-grep scan -U` with the filter, error, and path arguments after `--`                                        | `false` |
|  [10]   | `mutation`  | Mutation script                                                                                                 | `false` |
|  [11]   | `upgrade`   | `uv lock --upgrade`, `pnpm update --latest --recursive`, and dotnet-outdated, one per configuration            | `false` |
|  [12]   | `workflow`  | Workflow script, act over `ci.yml`                                                                              | `false` |
|  [13]   | `harness`   | Harness script, `.claude/types` regenerated, the plugin proven by its load line, its cached copy reinstalled    | `false` |
|  [14]   | `rules`     | `ast-grep test --include-off`                                                                                   | `true`  |
|  [15]   | `outline`   | `ast-grep outline` with every outline rule under `tools/ast-grep/outline/`                                      | `false` |
|  [16]   | `browsers`  | `playwright install chromium --no-shell`, the full build the `chromium` channel launches headless               | `false` |

- `lint` depends on `rules`, `format` and publish on `restore`
- `lint` and `format` take the tree with `.github/` and `.claude/` as inputs beside the script tool versions
- `tooling`, the named input of root `typecheck`, holds the files root `tsconfig.json` compiles
- `nx run rasm:outline -- <paths> --items structure` passes its outline options through unchanged
- `up`, `refresh`, `upgrade`, `workflow`, and `harness` set `parallelism: false`
- `harness` depends on the function-hooks `test` target

Use `ast-grep` for rule checks, extractor design, and views.

## [05]-[RELEASE]

`release` field versions each project from its `<projectName>@<version>` tag. Git-tag version actions read and write no manifest:
- `projectsRelationship: independent` at the root, a fixed group taking one `{releaseGroupName}@{version}` tag for its projects
- `versionActions: tools/nx/version-actions.ts` sets `validManifestFilenames` null and answers `0.0.0` for a project with no tag
- `fallbackCurrentVersionResolver: "disk"` and `automaticFromRef: true` make a first release need no `--first-release`
- `conventionalCommits: true` derives the bump from the commits since the tag, `updateDependents: "never"` bumps no dependent
- `workspaceChangelog: false` and `projectChangelogs.file: false` write no file, `createRelease: "github"` needs `git.push: true`
- `git` tags and pushes with no commit
- Groups select by `tag:release:<language>`
- Local plugin tags each library from its language or its `ReleaseGroup` property
- `nx release --yes` and `--skip-publish` exclude each other, the publish step is its own `nx release publish` command
- Custom release commands consume `NX_DRY_RUN` through their CLI parser before a registry write

## [06]-[HARNESS]

Configure the agent harness, editor, and git by language, file type, or event:
- `extraKnownMarketplaces.rasm` names the marketplace at `./.claude/plugins` a clone gets every plugin from
- Test targets under the hook environment
- `claudeMdExcludes` keeps the `CLAUDE.md` files under caches, `node_modules`, and build outputs out of the context
- `.vscode/settings.json` keys the formatter by language id, Biome resolves the workspace package, and Python tools read the `.venv` copies
- `files.associations` maps `SKILL.md` and the agent files back to markdown
- `files.exclude` hides every cache, output, and dependency directory
- `.mcp.json` holds every server but the Yak router and the `computer-use` connector, `dotnet dnx` for .NET and `type: http` for remote
- Harness script reads `enabledMcpServers` in `~/.claude.json`, the per-project `computer-use` switch
- `${VAR}` headers on the servers that take a token expand from the environment of the `claude` launch
- `doppler run --project agent-runtime --config dev -- claude` supplies every token header
- Servers run from `node_modules/.bin`, the ast-grep `pipx:` row, or `dotnet dnx` through the `_.path` entry `mise exec` puts first on their PATH
- ast-grep entry sets `AST_GREP_CONFIG` to `${CLAUDE_PROJECT_DIR:-.}/sgconfig.yml`
- `.gitattributes` normalizes text to LF and stores binary design assets as Git LFS pointers by extension

## [07]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                                      | [CORRECT_FORM]                                                          |
| :-----: | :----------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `mise x <command>` or a mise task wrapping a target          | Direct command under the mise environment from the hook or the shims    |
|  [02]   | Unneeded runtime pin or release-age delay                    | `latest`, with documented SDK and interpreter constraints               |
|  [03]   | Duplicated manifest settings in `[env]`                      | Manifest setting, or an explicit override of an inherited process value |
|  [04]   | `[env]` rows for a directory one script or program computes  | Script or program derives it beside its other paths                     |
|  [05]   | Binary-only npm packages in the catalog and `allowBuilds`    | `[tools]` rows at `latest`                                              |
|  [06]   | Script file for a hook the documentation states as a command | Command string in `.claude/settings.json`                               |
|  [07]   | Settings deny glob for a phrase                              | Parsed-command row in the plugin's Bash table                           |
