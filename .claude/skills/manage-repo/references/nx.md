# [NX]

## [01]-[PROJECTS]

- Plugins export `createDependencies` beside `createNodes` for undeclared edges, a static edge names its `sourceFile`
- Nx merges inferred projects sharing a root, each file of one directory adds its targets to one project
- Workspace plugin markers match a base name, an extension, or `<directory>/<base>` for a generic name (`.storybook/main.ts`)
- Projects with a `tsconfig.json` and no `package.json` take the nearest package's name joined to their relative path, their source imports giving edges
- `analyzeSourceFiles` stays off, Nx resolves `#/` imports through a TypeScript API 7.1 lacks
- Cargo `[package.metadata.nx]` merges over a package's inferred targets as a later layer of its project, a target one package owns sits there
- Task hashing reads files by project directory, a root naming a file owns none and its tasks fail to hash
- Plugins infer each target and tag from project files, `targetDefaults` configure them, `package.json` files hold no `nx` field
- Workspace plugin `root` option holds root targets under root `package.json` `name`, Nx infers a root project from an `nx` field alone
- Workspace plugin `grammars` option holds rows of project `grammars`, a project apart keeps root rows out of grammar hashes
- `analytics: false` disables telemetry `neverConnectToCloud` leaves on, without a boolean Nx prompts and rewrites `nx.json` expanded
- `useDaemonProcess: false` stays, concurrent agents race daemon starts into a socket error and a disabled marker only `nx reset` clears
- Live daemons recompute the graph on every file another agent writes
- Sync generators take one `tools/nx/sync/<written kind>/<name>.ts` and one `tools/nx/sync/generators.json` row, in `syncGenerators` of the target reading what they write, else in `sync.globalGenerators`
- `lint:nx` reports generator drift and `format:nx` writes it, `nx.json` `inputs` name each file a generator reads or writes, the plugin adds imports
- Generators on a target rerun `createNodes` per run and off a TTY refuse the task over another writer's drift
- `outline-catalogs` writes `tools/ast-grep/outline/<lang>.yml` from `sources/<lang>.yml`, edits go to the source
- Catalog sources open with an idless document, each row receiving `roles` key defaults and the `utils`, `rewriters`, `transform`, and `variants` it calls

## [02]-[DISCOVERY]

Use JSON discovery from workspace root:

| [INDEX] | [QUESTION]                    | [COMMAND]                                          |
| :-----: | :---------------------------- | :------------------------------------------------- |
|  [01]   | Projects exposing a target    | `nx show projects --with-target <target> --json`   |
|  [02]   | Project declaration           | `nx show project <project> --json`                 |
|  [03]   | Target and prerequisites      | `nx show target <project>:<target> --json`         |
|  [04]   | Hash inputs                   | `nx show target inputs <project>:<target> --json`  |
|  [05]   | Declared and existing outputs | `nx show target outputs <project>:<target> --json` |

- Discovery merges a configuration's options under `--configuration` alone, without it a target reports the configuration name
- Target options show a shallow merge before executor normalization
- `nx show project` without `--json` opens a browser in a TTY

## [03]-[TARGETS]

- `targetDefaults` tries executor key, exact target name, then globs longest first, and applies the first key holding a matching entry alone
- Matching entries inside one `targetDefaults` key merge in order, later values win, a key with no matching entry falls through to the next
- Entry `filter` keys `projects` (names, globs, `tag:<tag>`, `directory:<glob>`, `!` exclusions), `plugin`, and `executor` must all match
- Filters for workspace root use `projects: ["directory:."]`, unqualified names can match child project name prefixes
- `filter.plugin` matches the plugin that set a target's `executor` or `command`, an empty inferred target matches by tag or name glob
- Entries setting no `command` precede a key's entry setting one, Nx stamps a later entry with the inferred command and it replaces target
- `targetDefaults` configures existing targets alone, over plugin-inferred values and under project configuration
- `"...": true` in objects and `"..."` in arrays preserve lower-layer values at their position
- `command` on a target runs one command with its `options`, `executor: nx:run-commands` with `commands` and `parallel` exists for a list alone
- Command lists run in order under `parallel: false`, the `true` default terminates running commands when one fails unless `readyWhen` is set
- `options.args` supplies defaults, CLI `--args` replaces them with shell source, `forwardAllArgs: false` stops automatic forwarding
- Forwarded flags named as a run-commands option (`--verbose`, `--cwd`, `--env`, `--color`) never reach the command
- `{projectRoot}` and `{projectName}` interpolate anywhere in an option value, `{workspaceRoot}` at its start alone
- Commands with `cwd` at their project read the workspace root through `$NX_WORKSPACE_ROOT`
- Targets with `options` and no `executor`, `command`, or `targetDefaults` executor resolve to `nx:noop` with a `dependsOn` and drop without one
- Pipes, loops, conditionals, and variables in a command entry make it a script
- Tools taking no directory or walking ignored trees (`yamlfmt`, `yamllint`) run as `fd --hidden --extension <ext> --exec-batch <tool>`
- `fd --exec-batch` runs no process without a match
- Target names in `dependsOn` match as minimatch globs, `!(<name>)` excludes a target the glob matches
- Targets reach a target they need through `dependsOn` in place of a command running `nx`
- Configurations differing in arguments alone set `args` over one `command`
- `cache: true` alone caches a target, `cache: false` overrides a `targetDefaults` entry setting it for the name
- Container targets take `cache: false` and `parallelism: false`, no task input hashes container or service state
- `parallelism: false` runs a target with no other task of its `nx` process
- `NX_DEFAULT_OUTPUT_STYLE` in `mise.toml` overrides the `summary` style Nx picks for an agent
- `static` and `static-failures-only` hold task output until the task exits, `stream-without-prefixes` streams it live
- Nx reads an unknown `<project>:<target>:<config>` as target `<target>` and drops the configuration

## [04]-[INPUTS]

| [INDEX] | [FORM]                                         | [HASHES]                                                     |
| :-----: | :--------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `{projectRoot}/**/*`, `{workspaceRoot}/<path>` | Files matching the glob without nested projects, `!` negates |
|  [02]   | `<name>`, `^<name>`                            | Named input of the project, of its dependencies              |
|  [03]   | `{ "runtime": "<command>" }`                   | Stdout and stderr of the command, exit code unread           |
|  [04]   | `{ "env": "<NAME>" }`                          | Variable value                                               |
|  [05]   | `{ "externalDependencies": ["<package>"] }`    | Installed package versions                                   |
|  [06]   | `{ "dependentTasksOutputFiles": "<glob>" }`    | Outputs of dependency tasks, `transitive: true` recurses     |
|  [07]   | `{ "json": "<path>", "fields": ["<field>"] }`  | Listed fields of a JSON file, `excludeFields` the rest       |

- Every task hashes all targets of its project and `nx.json`, `.gitignore`, `.nxignore` whole, no input excludes them
- File inputs include untracked files and exclude ignored files
- `includeIgnored: true` on a `{ "fileset": "<glob>" }` entry hashes ignored matches from disk, on a `!` entry excludes ignored matches
- Named inputs hold files every target naming them reads, an input one target reads inlines into it, one a plugin reads (`production`) stays
- Compiler inputs include declared target sources disabled for execution and imported workspace source absent from graph edges

## [05]-[SCOPE]

- `affected` with a base and no head diffs against `HEAD` and adds uncommitted and untracked files
- Files belong to the nearest containing project root, a plugin file `nx.json` names to every project
- `affected` runs each selected target's command unchanged, a root target over its whole file set

## [06]-[RELEASE]

Products version by Conventional Commits into tags alone, `nx.json` `release` and `eng/typescript/release` hold the flow:
- Release groups map one product `output:*` tag to a group named for the host
- MinVer tag prefixes spell the group name, target options interpolate no `{releaseGroupName}`
- Commits count toward a product when they touch its files or a module it links
- Untagged commits pack the next patch prerelease with height
- `pack` caches where every input it reads hashes, Blender's through a runtime input over `HEAD` and its group's tags
- Xcode and Rhino `pack` read host state no input hashes and run uncached
- `publish` uploads `pack` output to the release `--tag` names, then pushes it to host's channel
- Products on a channel with no publish API hold no `publish`, `rasm:release` tags and releases them alone
- Feed channels (`output:appcast`, `output:extension`) take a rolling `<project>-updates` release from `infra/`
- First releases start from an unsigned lightweight `<group>-<project>-v0.0.0` tag and `rasm:release -- --specifier <version>`, the tag then goes
