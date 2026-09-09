# [TOOLING]

## [01]-[MANAGER]

- Tool manager's one file owns every standalone binary and the process environment
- Tool rows are `latest` under the registry backend
- Pinned rows state the reason as a fact the manifest needs
- Runtimes a host binds to one version are pinned by the file the host documents
- Tool manager reads the host's version file
- Tools no target, workflow, or agent runs belong to the machine profile

## [02]-[ENVIRONMENT]

- Process variables sit in the manager's environment table, one file every process reads
- Values a tool reads from its own manifest field stay in the tool manifest
- Environment holds what no manifest field covers
- `exec` templates running a tool at environment load couple the environment to a build product, move the value to the manifest
- Second config files for one platform signal a variable to unset or a package to remove
- Experimental flags, backends past the registry, and post-install binary patches are workarounds, replace them with the documented form

## [03]-[CHECKERS_AND_WRITERS]

- Checkers read their configuration file and the ignore file
- Checkers select their own files
- Checkers for a file kind absent from the tree join with the first file of the kind

## [04]-[HARNESS]

- Agent shell holds the manager's environment through harness hooks
- Commands run by name
- Hooks refuse a call from what the engine event shows
- Hooks that rewrite a command, inject guidance, or demand a comment enforce prose, move the rule to prose
- Hook reasons name the correct form once
- Guidance sentences copied into hook code with a spec holding them equal are copies, delete them
- Plugins hold their guards and nothing that observes them
- Scripts driving the agent host to prove a hook are harnesses, delete them
