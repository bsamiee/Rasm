# [NX]

Nx infers projects through plugins, orders targets through `dependsOn`, caches outputs by inputs, and selects projects by changed files.

## [01]-[PROJECTS]

- Plugins infer a project from each file matching their glob, `plugins` in `nx.json` registers each with `options`, `include`, and `exclude` globs
- `useDaemonProcess: false` computes the graph on every command
- `cacheDirectory` relocates the task cache, `NX_WORKSPACE_DATA_DIRECTORY` the workspace data
- Manifest `scripts` entries infer as `nx:run-script` targets with no inputs, outputs, or cache, `nx.targets` holds targets with them

## [02]-[TARGETS]

- `targetDefaults.<target>` takes one configuration or an ordered array of entries, the last matching entry wins
- Entry `filter.projects` takes project names, globs, and `tag:<tag>`, `filter.plugin` and `filter.executor` narrow further
- Inferred target fields come from the plugin, `targetDefaults` and project configuration merge over them field by field
- `"..."` in a target, its `options`, `configurations`, `inputs`, or `dependsOn` keeps what the lower layer supplied at that position
- `nx:run-commands` takes `command` with `forwardAllArgs` or a `commands` list with `parallel`, `cwd`, and `env`
- Pipes, loops, conditionals, and variables in a command entry are a script, the second command takes its own entry or target
- `dependsOn` names a target of the project (`build`), of its dependencies (`^build`), or of named projects (`{ projects, target }`)
- `params: forward` on a `dependsOn` entry passes the arguments to the dependency
- Commands that run `nx` inside a target name a dependency, `dependsOn` holds it
- `configurations` holds named option sets merged over `options`, `--configuration <name>` selects one, `defaultConfiguration` the default
- `cache: false` marks targets with outputs that are not a function of their inputs (source writers, lock upgrades, remote operations)
- `parallelism: false` marks targets writing a shared file or directory

## [03]-[INPUTS]

| [INDEX] | [FORM]                                           | [HASHES]                                                   |
| :-----: | :----------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `{projectRoot}/**/*`, `{workspaceRoot}/<path>`   | Files matching the glob, `!` negates                       |
|  [02]   | `<name>`, `^<name>`                              | Named input of the project, of its dependencies            |
|  [03]   | `{ "runtime": "<command>" }`                     | Command output, the tool version                           |
|  [04]   | `{ "env": "<NAME>" }`                            | Variable value                                             |
|  [05]   | `{ "externalDependencies": ["<package>"] }`      | Installed package versions                                 |
|  [06]   | `{ "dependentTasksOutputFiles": "<glob>" }`      | Outputs of dependency tasks, `transitive: true` recurses   |
|  [07]   | `{ "json": "<path>", "fields": ["<field>"] }`    | Listed fields of a JSON file, `excludeFields` the rest     |

- Cache hits restore the files `outputs` name

## [04]-[SCOPE]

- `affected` selects projects from `--files`, `--uncommitted`, `--untracked`, or `--base` with `--head`, `defaultBase` against the tree otherwise
