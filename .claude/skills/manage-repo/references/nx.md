# [NX]

## [01]-[PROJECTS]

- Plugins export `createDependencies` beside `createNodes` for undeclared edges, a static edge names its `sourceFile`
- Nx merges inferred projects sharing a root, each file of one directory adds its targets to one project
- Workspace plugin markers match a base name, an extension, or `<directory>/<base>` for a generic name
- Task hashing reads files by project directory, a root naming a file owns none and its tasks fail to hash
- Plugins infer each target and tag from project files, `targetDefaults` configure them, `package.json` files hold no `nx` field
- Workspace plugin `root` option holds root targets under root `package.json` `name`, Nx infers a root project from an `nx` field alone
- `useDaemonProcess: false` keeps plugin code current, a daemon reloads plugins on an `nx.json` `plugins` edit alone and serves old `tools/nx` code
- `analytics: false` disables telemetry `neverConnectToCloud` leaves on, without a boolean Nx prompts and rewrites `nx.json` expanded
- Sync generators take one row in the `generators` file `@rasm/nx` names
- `@rasm/nx:<name>` sits in `syncGenerators` of the target reading what it writes, else in `sync.globalGenerators`
- `lint:nx` reports generator drift, the workspace `format` target runs `nx sync` before `hk fix`
- Target `inputs` name files the generators read or write, the plugin adds each generator's declared `files` and rule folders, the `**/*.{ts,tsx}` input covers generator imports
- Generators on a target rerun `createNodes` per run and off a TTY refuse the task over another writer's drift
- Each `grammar:<lang>` target builds `.cache/ast-grep/outline/<lang>.yml` from `tools/ast-grep/outline/<lang>.yml` as a cached output, edits go to the source
- Catalog sources open with an idless document, each row receiving `roles` key defaults and the `utils`, `rewriters`, `transform`, and `variants` it calls

## [02]-[DISCOVERY]

Use JSON discovery from workspace root:

| [INDEX] | [QUESTION]                      | [COMMAND]                                                           |
| :-----: | :------------------------------ | :------------------------------------------------------------------ |
|  [01]   | Projects exposing a target      | `nx show projects --with-target <target> --json`                    |
|  [02]   | Project declaration             | `nx show project <project> --json`                                  |
|  [03]   | Target and prerequisites        | `nx show target <project>:<target> --json`                          |
|  [04]   | Hash inputs, or file membership | `nx show target inputs <project>:<target> --json`, `--check <file>` |
|  [05]   | Declared and existing outputs   | `nx show target outputs <project>:<target> --json`                  |

- Discovery merges a configuration's options under `--configuration` alone, without it a target reports the configuration name
- Target options show a shallow merge before executor normalization
- `nx show project` without `--json` opens a browser in a TTY

## [03]-[TARGETS]

- `targetDefaults` tries executor key, exact target name, then globs longest first, and applies the first key holding a matching entry alone
- Matching entries inside one `targetDefaults` key merge in order, later values win, a key with no matching entry falls through to the next
- Entry `filter` keys `projects` (names, globs, `tag:<tag>`, `directory:<glob>`, `!` exclusions), `plugin`, and `executor` must all match
- Filters for workspace root use `projects: ["directory:."]`, unqualified names can match child project name prefixes
- `filter.plugin` matches the plugin that set a target's `executor` or `command`, an empty inferred target matches by tag or name glob
- Entries setting a `command` over an inferred target holding one merge as an option, the later entry winning
- Over an inferred target holding no `command`, an entry with another `command` or `executor` than the result merged before it replaces it whole, inferred `cwd`, `args`, and `parallelism` included
- Plugins state the `command` of each target they infer, so a `targetDefaults` row setting another one merges
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
- File checkers run `hk check --glob='**/*' --step=<step>`
- Target names in `dependsOn` match as minimatch globs, `!(<name>)` excludes a target the glob matches
- `params: 'forward'` hands the run's CLI arguments to its edge from anywhere in the closure, `rasm:format -- <path>` reaches a forwarding edge under it
- Targets reach a target they need through `dependsOn` in place of a command running `nx`
- Configurations differing in arguments alone set `args` over one `command`
- `cache: true` alone caches a target, `cache: false` overrides a `targetDefaults` entry setting it for the name
- Container targets take `cache: false` and `parallelism: false`, no task input hashes container or service state
- `parallelism: false` runs a target with no other task of its `nx` process
- Nx uses `summary` for agents and gives the log path for each failed task
- Use `--outputStyle=stream-without-prefixes` when you need task output as it happens
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

- Every task hashes all targets of its project, `nx.json` and `.gitignore` whole, and root `tsconfig.base.json`, no input excludes them
- File inputs include untracked files and exclude ignored files
- `includeIgnored: true` on a `{ "fileset": "<glob>" }` entry hashes ignored matches from disk, on a `!` entry excludes ignored matches
- `includeIgnored` filesets rooted at `{workspaceRoot}/**` walk every ignored tree on disk per task graph
- `--graph=stdout` hashes each task and runs its `runtime` inputs, a slow one (a JVM start) adds to each `affected` query
- Lock file inputs and `ls -lL` over an executable carry a tool's version without starting it
- Named inputs hold files every target naming them reads, an input one target reads inlines into it, one a plugin reads (`production`) stays
- Compiler inputs include declared target sources disabled for execution and imported workspace source absent from graph edges

## [05]-[SCOPE]

- `affected` with a base and no head diffs against `HEAD` and adds uncommitted and untracked files
- `nx show projects --affected` selects affected projects. With `--with-target` and without `--projects`, Nx selects affected tasks
- Files belong to the nearest containing project root, a plugin file `nx.json` names to every project
- `affected` runs each selected target's command unchanged, a root target over its whole file set
- Root check takes inputs over the whole workspace, task-level affected selects it on files its checkers read in nested project roots

## [06]-[RELEASE]

Products version by Conventional Commits into tags alone, `nx.json` `release` and `eng/typescript/release.ts` hold the flow:
- Release groups map one product `output:*` tag to a group named for the host
- MinVer tag prefixes spell the group name, target options interpolate no `{releaseGroupName}`
- Commits count toward a product when they touch its files or a module it links
- Untagged commits pack the next patch prerelease with height
- `pack` caches where every input it reads hashes, Blender's through a runtime input over `HEAD` and its group's tags
- Xcode and Rhino `pack` read host state no input hashes and run uncached
- `publish` reads its release tag from `HEAD` under the project's `MinVerTagPrefix`, a `git describe --exact-match` failing on an untagged commit
- Products on a channel with no publish API hold no `publish`, `rasm:release` tags and releases them alone
- Feed channels (`output:appcast`, `output:extension`) take a rolling `<project>-updates` release from `infra/`
- First releases start from `0.0.0`, GitHub creates each tag with its release
