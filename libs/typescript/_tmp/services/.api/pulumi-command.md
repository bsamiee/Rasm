# [API_CATALOGUE] @pulumi/command

`@pulumi/command` supplies two sub-namespaces for shell command execution: `local` for commands on the deployment host and `remote` for commands on remote hosts over SSH. `local.Command` and `remote.Command` are lifecycle-tracked `CustomResource` types that run on create/update/delete; `local.run` / `local.runOutput` are unconditional invoke functions that run on every program execution.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/command`
- package: `@pulumi/command`
- module: `@pulumi/command` (root), `@pulumi/command/local` and `@pulumi/command/remote` via sub-namespace imports
- asset: local and remote shell command resources and invoke functions
- rail: deployment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: local namespace family
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [RAIL]                                                  |
| :-----: | :-------------------- | :----------------- | :------------------------------------------------------ |
|  [01]   | `local.Command`       | resource class     | lifecycle-tracked local shell command                   |
|  [02]   | `local.CommandArgs`   | args interface     | `create`, `update`, `delete`, `environment`, `triggers` |
|  [03]   | `local.RunArgs`       | invoke args        | `command` (required), `environment`, `dir`              |
|  [04]   | `local.RunResult`     | invoke result      | `stdout`, `stderr`, `archive`, `assets`                 |
|  [05]   | `local.RunOutputArgs` | output invoke args | `command: Input<string>`, other fields as `Input<T>`    |
|  [06]   | `local.Logging`       | const enum         | `Stdout`, `Stderr`, `StdoutAndStderr`, `None`           |

[PUBLIC_TYPE_SCOPE]: Command output properties (local)
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]                                            | [RAIL]                        |
| :-----: | :-------------------- | :------------------------------------------------------- | :---------------------------- |
|  [01]   | `Command.stdout`      | `Output<string>`                                         | captured standard output      |
|  [02]   | `Command.stderr`      | `Output<string>`                                         | captured standard error       |
|  [03]   | `Command.archive`     | `Output<pulumi.asset.Archive \| undefined>`              | archive of matched file paths |
|  [04]   | `Command.assets`      | `Output<{[key: string]: Asset \| Archive} \| undefined>` | matched asset map             |
|  [05]   | `Command.create`      | `Output<string \| undefined>`                            | create command string         |
|  [06]   | `Command.update`      | `Output<string \| undefined>`                            | update command string         |
|  [07]   | `Command.delete`      | `Output<string \| undefined>`                            | delete command string         |
|  [08]   | `Command.environment` | `Output<{[key: string]: string} \| undefined>`           | env var map                   |
|  [09]   | `Command.triggers`    | `Output<any[] \| undefined>`                             | trigger values for re-run     |
|  [10]   | `Command.interpreter` | `Output<string[] \| undefined>`                          | interpreter argv              |
|  [11]   | `Command.logging`     | `Output<local.Logging \| undefined>`                     | log capture mode              |

[PUBLIC_TYPE_SCOPE]: remote namespace family
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                                                            |
| :-----: | :-------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `remote.Command`      | resource class | lifecycle-tracked SSH command on remote host                      |
|  [02]   | `remote.CommandArgs`  | args interface | `connection` (required), `create`, `update`, `delete`, `triggers` |
|  [03]   | `remote.CopyFile`     | resource class | copy a local file to a remote host over SCP                       |
|  [04]   | `remote.CopyToRemote` | resource class | copy asset to remote host                                         |
|  [05]   | `remote.Logging`      | const enum     | `Stdout`, `Stderr`, `StdoutAndStderr`, `None`                     |

[PUBLIC_TYPE_SCOPE]: Command output properties (remote)
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]                                  | [RAIL]                    |
| :-----: | :-------------------- | :--------------------------------------------- | :------------------------ |
|  [01]   | `Command.stdout`      | `Output<string>`                               | captured standard output  |
|  [02]   | `Command.stderr`      | `Output<string>`                               | captured standard error   |
|  [03]   | `Command.connection`  | `Output<remote.Connection>`                    | SSH connection parameters |
|  [04]   | `Command.create`      | `Output<string \| undefined>`                  | create command string     |
|  [05]   | `Command.update`      | `Output<string \| undefined>`                  | update command string     |
|  [06]   | `Command.delete`      | `Output<string \| undefined>`                  | delete command string     |
|  [07]   | `Command.environment` | `Output<{[key: string]: string} \| undefined>` | env var map               |
|  [08]   | `Command.triggers`    | `Output<any[] \| undefined>`                   | trigger values for re-run |
|  [09]   | `Command.logging`     | `Output<remote.Logging \| undefined>`          | log capture mode          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: local commands
- rail: deployment

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [RAIL]                                                  |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `new local.Command(name, args?, opts?)`                          | constructor     | lifecycle-tracked command; runs on create/update/delete |
|  [02]   | `local.run(args: RunArgs, opts?): Promise<RunResult>`            | invoke function | unconditional run on every program execution            |
|  [03]   | `local.runOutput(args: RunOutputArgs, opts?): Output<RunResult>` | output invoke   | unconditional run returning `Output`-wrapped result     |

[ENTRYPOINT_SCOPE]: remote commands
- rail: deployment

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                               |
| :-----: | :------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `new remote.Command(name, args, opts?)`      | constructor    | lifecycle-tracked SSH command; `connection` required |
|  [02]   | `new remote.CopyFile(name, args, opts?)`     | constructor    | SCP file copy resource                               |
|  [03]   | `new remote.CopyToRemote(name, args, opts?)` | constructor    | asset copy to remote host                            |

[ENTRYPOINT_SCOPE]: CommandArgs key fields
- rail: deployment

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `create?: Input<string>`         | lifecycle hook | shell expression run on resource creation             |
|  [02]   | `update?: Input<string>`         | lifecycle hook | shell expression run on resource update               |
|  [03]   | `delete?: Input<string>`         | lifecycle hook | shell expression run on resource deletion             |
|  [04]   | `triggers?: Input<any[]>`        | change signal  | resource replaced or updated when any element changes |
|  [05]   | `environment?: Input<{...}>`     | env map        | additional env vars for the command process           |
|  [06]   | `interpreter?: Input<string[]>`  | shell override | defaults to `["/bin/sh", "-c"]` on Linux/macOS        |
|  [07]   | `archivePaths?: Input<string[]>` | glob paths     | files to collect into `archive` output after run      |
|  [08]   | `assetPaths?: Input<string[]>`   | glob paths     | files to collect into `assets` map after run          |
|  [09]   | `logging?: Input<local.Logging>` | log mode       | control stdout/stderr logging during deploy           |

## [04]-[IMPLEMENTATION_LAW]

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
