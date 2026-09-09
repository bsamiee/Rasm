# [NX]

Nx orders tool calls, caches call outputs, and selects the projects a change touches. Nx runs tools and holds no logic of its own.

## [01]-[PROJECTS]

- Plugins infer projects from manifests: a project file, a package manifest, a Python project file
- `nx.json` registers plugins, one per manifest kind
- Project files for the graph alone (`project.json`) signal failed inference, fix the plugin or the manifest
- Ignore files for the graph (`.nxignore`) hide files inference should skip by pattern, fix the plugin glob
- Project tags name the language
- Target defaults filter on the language tag

## [02]-[TARGETS]

- Targets are one tool call: the command, its arguments, and the cwd
- Tool configuration sits in the tool's own file
- Pipes, loops, conditionals, and variables in a command string signal a script placed in the graph, give the second command its own target
- Inferred targets keep their body at the plugin
- Manifest entries restating an inferred body are second declarations, delete them
- `targetDefaults` entries filtered by `filter.projects` on `tag:<tag>` hold the body of a target every project of that tag shares
- Root manifest `nx` field holds root targets: operations over the tree with no owning project
- `scripts` entries become targets with no inputs or outputs
- Tasks the graph caches are written under `nx.targets`
- `nx.json` holds named inputs, defaults, and plugins
- Each target sits in one file, `nx.json` or the root manifest
- Targets depend on other targets through `dependsOn`
- Runner calls inside a target command signal a dependency written wrong, move it to `dependsOn`
- Target families differing by one flag become one target with a `configurations` entry, or one target taking the flag as an argument

## [03]-[INPUTS]

- Inputs name the files a tool reads, the tool version as a `runtime` entry, and dependency outputs as `dependentTasksOutputFiles`
- Outputs name the files a tool writes
- Cache hits restore exactly the named outputs
- Whole-tree inputs make every change a cache miss
- Whole-tree inputs signal real inputs left unread, name the files the tool reads
- Targets writing source, a lock, or a remote resource run with `cache: false`
- Targets writing a shared file run with `parallelism: false`

## [04]-[SCOPE]

- `run-many -t <target>` runs one target over every project
- `run <project>:<target>` runs one target over one project
- `affected -t <target> --files=<paths>` runs the projects the listed files belong to and their dependents
- Root targets over the tree are checkers or writers reading the tree in one process

## [05]-[DAEMON]

- `useDaemonProcess: false` at the root of `nx.json` keeps the daemon off
- With the daemon off, every command computes the graph from the manifests it reads
- Slow graph computation signals a plugin doing work at inference time, fix the plugin
- Workspace data sits under the cache directory the environment names
