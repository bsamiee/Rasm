# [NX]

Nx infers projects through plugins, orders targets through `dependsOn`, caches outputs by inputs, and selects projects by changed files.

## [01]-[PROJECTS]

- Plugins infer a project from each file matching their glob, a matching file that defines no project joins the plugin's `exclude`
- `useDaemonProcess: false` computes the graph on every command
- `cacheDirectory` relocates the task cache, `NX_WORKSPACE_DATA_DIRECTORY` workspace data
- `package.json` `scripts` entries infer as `nx:run-script` targets with no inputs, outputs, or cache, `nx.targets` holds targets with them

## [02]-[TARGETS]

- `targetDefaults.<target>` takes one configuration or an ordered array of entries, the last matching entry wins
- Entry `filter.projects` takes project names, globs, and `tag:<tag>`, `filter.plugin` and `filter.executor` narrow further
- Inferred target fields come from the plugin, `targetDefaults` and project configuration merge over them field by field
- `"..."` in a target, its `options`, `configurations`, `inputs`, or `dependsOn` keeps what the lower layer supplied at its position
- `command` on a target runs one command with its `options`, `executor: nx:run-commands` with `commands` and `parallel` exists for a list alone
- `{projectRoot}` and `{projectName}` interpolate anywhere in an option value, `{workspaceRoot}` at its start alone
- Commands with `cwd` at their project read the workspace root through `$NX_WORKSPACE_ROOT`
- Target with `options` and no `executor`, `command`, or `targetDefaults` executor resolves to `nx:noop` with a `dependsOn` and drops without one
- Pipes, loops, conditionals, and variables in a command entry are a script, the second command takes its own entry or target
- Tools that take files and no directory run as `fd --hidden --extension <ext> --exec-batch <tool>`, one process over every file `.gitignore` leaves
- `fd --exec-batch` runs nothing without a match, a row for an extension with no tracked file goes
- `dependsOn` names a target of the project (`build`), of its dependencies (`^build`), or of named projects (`{ projects, target }`)
- `params: forward` on a `dependsOn` entry passes its arguments to the dependency
- Commands that run `nx` inside a target name a dependency, `dependsOn` holds it
- `configurations` holds named option sets merged over `options`, `--configuration <name>` selects one, `defaultConfiguration` the default
- `args` in `options` appends to the command, configurations differing in arguments alone set `args` over one `command`
- `cache: true` alone caches a target, `cache: false` overrides a `targetDefaults` entry setting it for the name
- Plugin source states each option's default (`testMode ??= 'watch'` in `@nx/vitest`)
- `parallelism: false` marks targets writing a shared file or directory
- `--output-style=static-failures-only` prints failing task logs inline, the `summary` style Nx picks under an agent names each log file alone

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

- Cache hits restore the files `outputs` name
- File inputs hash tracked files alone, a target reading a dependency's output under an ignored path names it through `dependentTasksOutputFiles`
- Named inputs hold files every target naming them reads, a file one target's tool reads joins its target's `inputs`
- Named input one target reads inlines into the target, one a plugin reads (`production` in `@nx/dotnet`) stays
- Root project's `default` named input (`{projectRoot}/**/*`) is the whole workspace, root targets list their own inputs

## [04]-[SCOPE]

- `affected` selects projects from `--files`, `--uncommitted`, `--untracked`, or `--base` with `--head`, `main` against the tree otherwise
- Files outside every inferred project belong to the root project, `affected --files=<file>` then runs root targets over every file
