# [API_CATALOGUE] @pulumi/command

`@pulumi/command` supplies two sub-namespaces for shell command execution: `local` for commands on the deployment host and `remote` for commands on remote hosts over SSH. `local.Command` and `remote.Command` are lifecycle-tracked `CustomResource` types that run on create/update/delete; `local.run` / `local.runOutput` are unconditional invoke functions that run on every program execution.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/command`
- package: `@pulumi/command`
- module: `@pulumi/command` (root), `@pulumi/command/local` and `@pulumi/command/remote` via sub-namespace imports
- asset: local and remote shell command resources and invoke functions
- rail: deployment

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: local namespace family
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [RAIL]                                                  |
| :-----: | :-------------------- | :----------------- | :------------------------------------------------------ |
|   [1]   | `local.Command`       | resource class     | lifecycle-tracked local shell command                   |
|   [2]   | `local.CommandArgs`   | args interface     | `create`, `update`, `delete`, `environment`, `triggers` |
|   [3]   | `local.RunArgs`       | invoke args        | `command` (required), `environment`, `dir`              |
|   [4]   | `local.RunResult`     | invoke result      | `stdout`, `stderr`, `archive`, `assets`                 |
|   [5]   | `local.RunOutputArgs` | output invoke args | `command: Input<string>`, other fields as `Input<T>`    |
|   [6]   | `local.Logging`       | const enum         | `Stdout`, `Stderr`, `StdoutAndStderr`, `None`           |

[PUBLIC_TYPE_SCOPE]: Command output properties (local)
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]                                            | [RAIL]                        |
| :-----: | :-------------------- | :------------------------------------------------------- | :---------------------------- |
|   [1]   | `Command.stdout`      | `Output<string>`                                         | captured standard output      |
|   [2]   | `Command.stderr`      | `Output<string>`                                         | captured standard error       |
|   [3]   | `Command.archive`     | `Output<pulumi.asset.Archive \| undefined>`              | archive of matched file paths |
|   [4]   | `Command.assets`      | `Output<{[key: string]: Asset \| Archive} \| undefined>` | matched asset map             |
|   [5]   | `Command.create`      | `Output<string \| undefined>`                            | create command string         |
|   [6]   | `Command.update`      | `Output<string \| undefined>`                            | update command string         |
|   [7]   | `Command.delete`      | `Output<string \| undefined>`                            | delete command string         |
|   [8]   | `Command.environment` | `Output<{[key: string]: string} \| undefined>`           | env var map                   |
|   [9]   | `Command.triggers`    | `Output<any[] \| undefined>`                             | trigger values for re-run     |
|  [10]   | `Command.interpreter` | `Output<string[] \| undefined>`                          | interpreter argv              |
|  [11]   | `Command.logging`     | `Output<local.Logging \| undefined>`                     | log capture mode              |

[PUBLIC_TYPE_SCOPE]: remote namespace family
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                                                            |
| :-----: | :-------------------- | :------------- | :---------------------------------------------------------------- |
|   [1]   | `remote.Command`      | resource class | lifecycle-tracked SSH command on remote host                      |
|   [2]   | `remote.CommandArgs`  | args interface | `connection` (required), `create`, `update`, `delete`, `triggers` |
|   [3]   | `remote.CopyFile`     | resource class | copy a local file to a remote host over SCP                       |
|   [4]   | `remote.CopyToRemote` | resource class | copy asset to remote host                                         |
|   [5]   | `remote.Logging`      | const enum     | `Stdout`, `Stderr`, `StdoutAndStderr`, `None`                     |

[PUBLIC_TYPE_SCOPE]: Command output properties (remote)
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]                                  | [RAIL]                    |
| :-----: | :-------------------- | :--------------------------------------------- | :------------------------ |
|   [1]   | `Command.stdout`      | `Output<string>`                               | captured standard output  |
|   [2]   | `Command.stderr`      | `Output<string>`                               | captured standard error   |
|   [3]   | `Command.connection`  | `Output<remote.Connection>`                    | SSH connection parameters |
|   [4]   | `Command.create`      | `Output<string \| undefined>`                  | create command string     |
|   [5]   | `Command.update`      | `Output<string \| undefined>`                  | update command string     |
|   [6]   | `Command.delete`      | `Output<string \| undefined>`                  | delete command string     |
|   [7]   | `Command.environment` | `Output<{[key: string]: string} \| undefined>` | env var map               |
|   [8]   | `Command.triggers`    | `Output<any[] \| undefined>`                   | trigger values for re-run |
|   [9]   | `Command.logging`     | `Output<remote.Logging \| undefined>`          | log capture mode          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: local commands
- rail: deployment

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [RAIL]                                                  |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------------------ |
|   [1]   | `new local.Command(name, args?, opts?)`                          | constructor     | lifecycle-tracked command; runs on create/update/delete |
|   [2]   | `local.run(args: RunArgs, opts?): Promise<RunResult>`            | invoke function | unconditional run on every program execution            |
|   [3]   | `local.runOutput(args: RunOutputArgs, opts?): Output<RunResult>` | output invoke   | unconditional run returning `Output`-wrapped result     |

[ENTRYPOINT_SCOPE]: remote commands
- rail: deployment

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                               |
| :-----: | :------------------------------------------- | :------------- | :--------------------------------------------------- |
|   [1]   | `new remote.Command(name, args, opts?)`      | constructor    | lifecycle-tracked SSH command; `connection` required |
|   [2]   | `new remote.CopyFile(name, args, opts?)`     | constructor    | SCP file copy resource                               |
|   [3]   | `new remote.CopyToRemote(name, args, opts?)` | constructor    | asset copy to remote host                            |

[ENTRYPOINT_SCOPE]: CommandArgs key fields
- rail: deployment

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :------------------------------- | :------------- | :---------------------------------------------------- |
|   [1]   | `create?: Input<string>`         | lifecycle hook | shell expression run on resource creation             |
|   [2]   | `update?: Input<string>`         | lifecycle hook | shell expression run on resource update               |
|   [3]   | `delete?: Input<string>`         | lifecycle hook | shell expression run on resource deletion             |
|   [4]   | `triggers?: Input<any[]>`        | change signal  | resource replaced or updated when any element changes |
|   [5]   | `environment?: Input<{...}>`     | env map        | additional env vars for the command process           |
|   [6]   | `interpreter?: Input<string[]>`  | shell override | defaults to `["/bin/sh", "-c"]` on Linux/macOS        |
|   [7]   | `archivePaths?: Input<string[]>` | glob paths     | files to collect into `archive` output after run      |
|   [8]   | `assetPaths?: Input<string[]>`   | glob paths     | files to collect into `assets` map after run          |
|   [9]   | `logging?: Input<local.Logging>` | log mode       | control stdout/stderr logging during deploy           |

## [4]-[IMPLEMENTATION_LAW]

[COMMAND_TOPOLOGY]:
- `local.Command` and `remote.Command` are `CustomResource` instances tracked in Pulumi state; they re-run when `triggers` values change or when the resource inputs change
- `local.run` / `local.runOutput` are invoke functions, not resources; they run unconditionally on every `pulumi up` and `pulumi preview` — use `local.Command` when conditional execution on change is needed
- When `update` is absent, `create` runs on both create and update operations
- `PULUMI_COMMAND_STDOUT` and `PULUMI_COMMAND_STDERR` env vars are injected into subsequent `update` or `delete` invocations from the prior step's outputs when `addPreviousOutputInEnv` is `true` (default)
- `remote.Command.connection` requires a `Connection` object with `host`, `user`, and one of `privateKey` or `password`

[LOCAL_ADMISSION]:
- `triggers` accepts any `Input<any[]>`; include `Output<T>` values from other resources to re-run the command when those resources change
- `archivePaths` glob patterns use `/` separators on all platforms; prefix with `!` to exclude files; `**` matches across directory boundaries
- `local.runOutput` accepts `RunOutputArgs` which mirrors `RunArgs` but wraps fields as `Input<T>` for use inside `Output.apply` chains

[RAIL_LAW]:
- Package: `@pulumi/command`
- Owns: local and remote shell command execution in Pulumi deployment lifecycle
- Accept: `local.Command` for change-triggered side effects; `local.runOutput` for unconditional per-deployment commands
- Reject: using `local.run` when conditional execution (only on resource change) is the intent
