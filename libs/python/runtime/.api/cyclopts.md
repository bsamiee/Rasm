# [PY_RUNTIME_API_CYCLOPTS]

`cyclopts` supplies a type-annotation-driven CLI framework: an `App` command registry, `Parameter`/`Argument` annotations, grouping and validation, a layered config surface, rich help formatting, shell-completion installation, and an interactive shell. It is the runtime owner for the daemon/CLI entrypoint grammar.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cyclopts`
- package: `cyclopts`
- import: `cyclopts`
- owner: `runtime`
- rail: entry
- namespaces: `cyclopts`, `cyclopts.config`, `cyclopts.types`, `cyclopts.validators`
- capability: annotation-driven CLI, command registry, parameter/argument annotations, groups, layered config, help formatting, completion, interactive shell

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: app and parameter family
- rail: entry

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :------------------ | :------------ | :-------------------------- |
|  [01]   | `App`               | app           | command registry and runner |
|  [02]   | `Parameter`         | annotation    | option/flag specification   |
|  [03]   | `Argument`          | annotation    | positional argument spec    |
|  [04]   | `Group`             | grouping      | command/parameter group     |
|  [05]   | `Token`             | token         | parsed argument token       |
|  [06]   | `Parameter.env_var` | binding       | environment-variable source |

[PUBLIC_TYPE_SCOPE]: config and validation family
- rail: entry

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :------------------------- | :------------ | :-------------------------- |
|  [01]   | `config.Toml`              | config source | TOML config layer           |
|  [02]   | `config.Yaml`              | config source | YAML config layer           |
|  [03]   | `config.Json`              | config source | JSON config layer           |
|  [04]   | `config.Env`               | config source | environment config layer    |
|  [05]   | `validators.Number`        | validator     | numeric-range validator     |
|  [06]   | `validators.Path`          | validator     | path-existence validator    |
|  [07]   | `validators.LimitedChoice` | validator     | enumerated-choice validator |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: entry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :---------------------- | :------------ | :-------------------------- |
|  [01]   | `CycloptsError`         | fault base    | base CLI error              |
|  [02]   | `ValidationError`       | fault         | argument-validation failure |
|  [03]   | `MissingArgumentError`  | fault         | required argument absent    |
|  [04]   | `CommandCollisionError` | fault         | duplicate command name      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: CLI operations
- rail: entry

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :----------------------- | :------------- | :------------------------------- |
|  [01]   | `App`                    | build          | construct the CLI app            |
|  [02]   | `App.command`            | register       | register a subcommand            |
|  [03]   | `App.default`            | register       | register the default command     |
|  [04]   | `App.meta.command`       | register       | register a meta/global command   |
|  [05]   | `App.__call__`           | run            | parse argv and dispatch          |
|  [06]   | `App.run_async`          | run            | async parse and dispatch         |
|  [07]   | `App.parse_args`         | parse          | bind argv to a command signature |
|  [08]   | `App.interactive_shell`  | shell          | run an interactive REPL          |
|  [09]   | `App.install_completion` | completion     | install shell completion         |
|  [10]   | `App.generate_docs`      | docs           | emit command documentation       |

## [04]-[IMPLEMENTATION_LAW]

[ENTRY_TOPOLOGY]:
- app law: the daemon/CLI surface is one `App` whose subcommands are registered with `@app.command`; a new command is one decorated function, never a parallel argument-parser or a `get`/`run`/`exec` family.
- annotation law: options and arguments are `Annotated[T, Parameter(...)]`/`Argument(...)`; cyclopts derives the parser from the type signature, so there is no manual `add_argument` wiring.
- config law: layered configuration uses the `config.*` sources in priority order, aligned with the `pydantic-settings` source chain; the CLI and settings model share one configuration vocabulary.
- validation law: argument constraints are `validators.*` on the annotation; a validation failure surfaces as a `CycloptsError` mapped to a non-zero exit through the boundary, never an uncaught traceback.
- async law: the daemon entrypoint uses `run_async` under the anyio runner so the CLI shares the structured-concurrency lane model.
- result law: command functions return `Result`; the boundary maps `Ok`→exit 0 and `Error(BoundaryFault)`→a coded non-zero exit with a rendered message.

[LOCAL_ADMISSION]:
- The entrypoint surface composes one `App`; the runtime owns no second argument parser (no argparse/click/typer).
- CLI configuration composes the `pydantic-settings` source chain; the two share configuration, never duplicate it.

[RAIL_LAW]:
- Package: `cyclopts`
- Owns: the daemon/CLI entrypoint grammar — command registry, annotation-driven parameters, layered config, validation, completion, and interactive shell
- Accept: one `App`, `@app.command` registration, `Annotated` parameters, `config.*`/`validators.*`, `run_async`, `Result`-to-exit mapping
- Reject: parallel argument parsers, manual `add_argument` wiring, duplicated configuration, uncaught tracebacks at the CLI boundary
