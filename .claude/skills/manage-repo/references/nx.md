# [NX]

Nx infers projects through plugins, orders targets through `dependsOn`, caches outputs by inputs, and selects projects by changed files.

## [01]-[PROJECTS]

- Plugins infer a project from each file matching their glob, a matching file that defines no project joins the plugin's `exclude`
- Plugins export `createDependencies` beside `createNodes` for undeclared edges, a static edge names its `sourceFile`
- `useDaemonProcess: false` computes graphs in the calling process
- `NX_CACHE_PROJECT_GRAPH=false` disables graph caching
- `package.json` `scripts` entries infer as `nx:run-script` targets with no inputs, outputs, or cache, `nx.targets` holds targets with them

## [02]-[TARGETS]

- `targetDefaults.<target>` takes one configuration or an array of entries, matching entries merge in order and later values win
- Entry `filter` keys `projects` (names, globs, `tag:<tag>`, `directory:<glob>`, `!` exclusions), `plugin`, and `executor` must all match
- `targetDefaults` configures existing targets alone, project configuration overrides a default
- `"...": true` in objects and `"..."` in arrays preserve lower-layer values at their position
- `command` on a target runs one command with its `options`, `executor: nx:run-commands` with `commands` and `parallel` exists for a list alone
- Command lists run in order under `parallel: false`, the `true` default terminates running commands when one fails unless `readyWhen` is set
- `options.args` and CLI arguments append to a command holding no `{args}` or `{args.<name>}`, `forwardAllArgs: false` stops the append
- `{projectRoot}` and `{projectName}` interpolate anywhere in an option value, `{workspaceRoot}` at its start alone
- Commands with `cwd` at their project read the workspace root through `$NX_WORKSPACE_ROOT`
- Targets with `options` and no `executor`, `command`, or `targetDefaults` executor resolve to `nx:noop` with a `dependsOn` and drop without one
- Pipes, loops, conditionals, and variables in a command entry are a script, the second command takes its own entry or target
- Tools taking no directory or walking ignored trees (`yamlfmt`, `yamllint`) run as `fd --hidden --extension <ext> --exec-batch <tool>`
- `fd --exec-batch` runs no process without a match
- `dependsOn` names a target of the project (`build`), of its dependencies (`^build`), or of named projects (`{ projects, target }`)
- `params: forward` on a `dependsOn` entry passes its arguments to the dependency
- Commands that run `nx` inside a target name a dependency, `dependsOn` holds it
- `configurations` holds named option sets merged over `options`, `--configuration <name>` selects one, `defaultConfiguration` the default
- Configurations differing in arguments alone set `args` over one `command`
- `cache: true` alone caches a target, `cache: false` overrides a `targetDefaults` entry setting it for the name
- Plugin source states each option's default (`testMode ??= 'watch'` in `@nx/vitest`)
- `parallelism: false` runs a target writing shared files or directories with no other task of its `nx` process
- `--output-style=static-failures-only` prints failing tasks and a single-project run in full, agent default `summary` names failing task log files

## [03]-[INPUTS]

| [INDEX] | [FORM]                                         | [HASHES]                                                 |
| :-----: | :--------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `{projectRoot}/**/*`, `{workspaceRoot}/<path>` | Files matching the glob, `!` negates                     |
|  [02]   | `<name>`, `^<name>`                            | Named input of the project, of its dependencies          |
|  [03]   | `{ "runtime": "<command>" }`                   | Stdout and stderr of the command, exit code unread       |
|  [04]   | `{ "env": "<NAME>" }`                          | Variable value                                           |
|  [05]   | `{ "externalDependencies": ["<package>"] }`    | Installed package versions                               |
|  [06]   | `{ "dependentTasksOutputFiles": "<glob>" }`    | Outputs of dependency tasks, `transitive: true` recurses |
|  [07]   | `{ "json": "<path>", "fields": ["<field>"] }`  | Listed fields of a JSON file, `excludeFields` the rest   |

- Cache hits replay a successful run at the same hash and restore the files `outputs` name
- File inputs include untracked files and exclude ignored files
- `includeIgnored: true` on a `{ "fileset": "<glob>" }` entry hashes ignored matches from disk, on a `!` entry excludes ignored matches
- Named inputs hold files every target naming them reads, a file one target's tool reads joins its target's `inputs`
- Named inputs one target reads inline into the target, one a plugin reads (`production` in `@nx/dotnet`) stays
- `{projectRoot}/**/*` hashes the project without nested projects, `{workspaceRoot}/**/*` every workspace file

## [04]-[SCOPE]

- `affected` selects changed projects and dependents from `--files`, `--uncommitted`, `--untracked`, or `--base` with `--head`
- Base resolves from `--base`, `NX_BASE`, `nx.json` `defaultBase`, then `main`, and head from `--head` or `NX_HEAD`
- Base selection without a head diffs against `HEAD` and adds uncommitted and untracked files
- Files belong to the nearest containing project root
- `affected` runs each selected target's command unchanged, a root target over its whole file set
