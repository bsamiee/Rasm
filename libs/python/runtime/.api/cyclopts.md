# [PY_RUNTIME_API_CYCLOPTS]

`cyclopts` supplies a type-annotation-driven CLI framework: an `App` command registry with sub-apps and a `meta` app, `Parameter`/`Argument` annotations with converter/validator/group/env_var/negative binding, a deep `cyclopts.types` constrained-type library (numeric bounds, fixed-width/hex ints, path-existence/resolve/extension, format-validated strings), a `cyclopts.validators` cross-argument family, a layered `cyclopts.config` source chain, a `result_action` string-action vocabulary plus the `ResultAction` enum and `resolve_returncode` return-to-exit resolver, rich help/version formatting (via `rich`), shell-completion installation, and an interactive shell. Dispatch runs sync (`__call__`) or async over an `asyncio`/`trio` backend (`run_async`). It is the runtime owner for the daemon/CLI entrypoint grammar and the only admitted argument parser (argparse/click/typer are banned).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cyclopts`
- package: `cyclopts`
- import: `cyclopts`
- owner: `runtime`
- rail: entry
- version: `4.19.0`
- license: `Apache-2.0`
- namespaces: `cyclopts`, `cyclopts.config`, `cyclopts.types`, `cyclopts.validators`
- capability: annotation-driven CLI, command/sub-app/meta registry, `Parameter`/`Argument` with converter/validator/group/env_var/negative/count/n_tokens, constrained-type library, cross-argument validators, layered config sources, string-action + `ResultAction` return mapping with `resolve_returncode`, help/version formatting, completion, interactive shell, async dispatch over asyncio/trio

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: app and parameter family
- rail: entry

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `App`                | app           | command registry, runner, and sub-app/meta owner        |
|  [02]   | `Parameter`          | annotation    | option/flag spec (`converter`/`validator`/`group`/`env_var`/`negative`/`count`/`n_tokens`/`consume_multiple`/`json_dict`/`required`/`accepts_keys`) |
|  [03]   | `Argument`           | annotation    | resolved positional/keyword argument binding            |
|  [04]   | `ArgumentCollection` | collection    | the bound argument set assembled from a signature       |
|  [05]   | `Group`              | grouping      | command/parameter group with own validator + `sort_key` |
|  [06]   | `Token`              | token         | one parsed CLI token (source-traced)                    |
|  [07]   | `Dispatcher`         | protocol      | command-dispatch hook for `meta`/middleware             |
|  [08]   | `ResultAction`       | enum          | typed post-command action; `result_action` also accepts the equivalent string-literal vocabulary (`'return_int_as_exit_code_else_zero'`/`'sys_exit'`/`'print_non_none_return_zero'`/...) |
|  [09]   | `CycloptsPanel`      | render        | a `rich`-backed help/error panel for custom formatting  |
|  [10]   | `UNSET`              | sentinel      | "no value supplied" marker distinct from `None`         |

[PUBLIC_TYPE_SCOPE]: config source family (`cyclopts.config`)
- rail: entry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :---------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `config.Toml`           | config source | TOML config layer (root_keys, search-parents)   |
|  [02]   | `config.Yaml`           | config source | YAML config layer                               |
|  [03]   | `config.Json`           | config source | JSON config layer                               |
|  [04]   | `config.Env`            | config source | environment-variable config layer (prefix)      |
|  [05]   | `config.Dict`           | config source | in-memory mapping config layer                  |
|  [06]   | `config.ConfigFromFile` | base          | base for file-backed config sources             |

[PUBLIC_TYPE_SCOPE]: validator family (`cyclopts.validators`)
- rail: entry

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [RAIL]                                          |
| :-----: | :----------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `validators.Number`            | value validator | numeric-range (`gt`/`gte`/`lt`/`lte`)          |
|  [02]   | `validators.Path`              | value validator | path existence/type (`exists`/`file_okay`/`dir_okay`) |
|  [03]   | `validators.LimitedChoice`     | value validator | enumerated/min-max choice count                 |
|  [04]   | `validators.Slice`             | value validator | length/slice constraint on sequences            |
|  [05]   | `validators.MutuallyExclusive` | group validator | at most one of a group set                      |
|  [06]   | `validators.all_or_none`       | group validator | all-or-none-of-group function                   |
|  [07]   | `validators.mutually_exclusive`| group validator | mutually-exclusive function form                |

[PUBLIC_TYPE_SCOPE]: constrained-type library (`cyclopts.types`)
- rail: entry
- `Annotated` aliases that bind a `Parameter` converter+validator; compose directly into command signatures instead of hand-rolled `validators.*`.

| [INDEX] | [SYMBOL]                                                                | [TYPE_FAMILY] | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `types.PositiveInt` / `NonNegativeInt` / `NegativeInt` / `NonPositiveInt` | int bound   | sign/zero-bounded integers                        |
|  [02]   | `types.PositiveFloat` / `NonNegativeFloat` / `NegativeFloat` / `NonPositiveFloat` | float bound | sign/zero-bounded floats                  |
|  [03]   | `types.UInt8` / `UInt16` / `UInt32` / `UInt64` / `Int8`/`Int16`/`Int32`/`Int64` | width int | fixed-width bounded integers                      |
|  [04]   | `types.HexUInt` / `HexUInt8`/`16`/`32`/`64`                              | radix int     | hex-parsed unsigned integers                      |
|  [05]   | `types.Port`                                                            | int bound     | TCP port (0–65535)                                |
|  [06]   | `types.PercentInt` / `NormFloat` / `SignedNormFloat`                     | ranged number | percent (0–100) / normalized [0,1] / [-1,1]       |
|  [07]   | `types.File` / `Directory` / `Path`                                     | typed path    | `pathlib.Path` with `Parameter` binding (no existence check) |
|  [08]   | `types.ExistingFile` / `ExistingDirectory` / `ExistingPath`              | path exists   | path must exist (file/dir/either)                 |
|  [09]   | `types.NonExistentFile` / `NonExistentDirectory` / `NonExistentPath`     | path absent   | path must not exist                               |
|  [10]   | `types.ResolvedPath` / `ResolvedFile` / `ResolvedDirectory` / `ResolvedExistingPath` / `ResolvedExistingFile` / `ResolvedExistingDirectory` | path resolve | resolve to absolute, optionally must-exist |
|  [11]   | `types.JsonPath`/`TomlPath`/`YamlPath`/`CsvPath`/`TxtPath`/`BinPath`/`ImagePath`/`Mp4Path` (+ `Existing*` and `NonExistent*` of each) | typed path | extension-checked path families (bare / must-exist / must-not-exist) |
|  [12]   | `types.Email` / `URL`                                                   | format string | format-validated string aliases                  |
|  [13]   | `types.Json`                                                            | parsed         | JSON-string parsed to object                      |
|  [14]   | `types.NonEmptySlice`                                                   | ranged seq    | sequence constrained to length >= 1               |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: entry

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :-------------------------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `CycloptsError`                                           | fault base    | base for all CLI parse/bind faults              |
|  [02]   | `ValidationError`                                         | fault         | converter/validator rejection                   |
|  [03]   | `CoercionError`                                           | fault         | type-coercion failure of a token                |
|  [04]   | `MissingArgumentError`                                    | fault         | required argument absent                        |
|  [05]   | `UnknownOptionError` / `UnknownCommandError`              | fault         | unrecognized option/command                     |
|  [06]   | `UnusedCliTokensError`                                    | fault         | leftover tokens after binding                   |
|  [07]   | `RepeatArgumentError` / `MixedArgumentError`              | fault         | duplicate / positional+keyword collision        |
|  [08]   | `ArgumentOrderError` / `ConsumeMultipleError` / `CombinedShortOptionError` / `RequiresEqualsError` | fault | token-grammar violations              |
|  [09]   | `CommandCollisionError` / `DocstringError`                | fault         | duplicate command name / malformed help docstring |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: app construction and registration
- rail: entry
- defined on `App` (PUBLIC_TYPES [01]).

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `App(name, *, version, help, config, default_parameter, group_commands, group_arguments, group_parameters, console, end_of_options_delimiter, help_flags, version_flags)` | build | construct the CLI app with config + grouping policy |
|  [02]   | `App.command(obj=None, *, name, ...)`                                                       | register       | register a subcommand (function or sub-`App`)     |
|  [03]   | `App.default(obj=None, ...)`                                                                | register       | register the no-subcommand default command        |
|  [04]   | `App.meta` (property)                                                                        | meta app       | the meta/global `App` wrapping argv pre-dispatch  |
|  [05]   | `App.group_commands` / `.group_parameters` / `.group_arguments` (properties)                 | grouping       | default `Group`s for help organization            |
|  [06]   | `App.result_action` / `App.validator` (properties)                                           | hook           | post-command return mapping / app-level validator |
|  [07]   | `App.update(app: App)` (method); `App.subapps` / `App.alias` / `App.synonym` (properties)    | registry       | merge another `App`'s commands / read nested-app + name-alias state |
|  [08]   | `App.backend` (property)                                                                      | async backend  | the `'asyncio'`/`'trio'` runner used by `run_async` |

[ENTRYPOINT_SCOPE]: dispatch and parse
- rail: entry

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `App.__call__(tokens=None, *, console=, exit_on_error=, backend=, result_action=, error_formatter=) -> Any` | run | parse argv and dispatch (sync)                  |
|  [02]   | `App.run_async(tokens=None, *, backend: Literal['asyncio','trio']=None, result_action=, ...) -> Any` | run | async parse and dispatch (asyncio or trio backend) |
|  [03]   | `App.parse_args(tokens, ...) -> (Callable, inspect.BoundArguments, dict)` | parse | bind argv to a command + bound args + meta dict |
|  [04]   | `App.parse_known_args(tokens, ...) -> (Callable, BoundArguments, list[str], dict)` / `App.parse_commands(tokens, *, include_parent_meta=True) -> (names, apps, remaining)` | parse | partial bind / command-chain resolution |
|  [05]   | `App.assemble_argument_collection(*, default_parameter=, parse_docstring=False) -> ArgumentCollection` | parse | build the `ArgumentCollection` for a signature  |
|  [06]   | `cyclopts.run(callable, /, *, result_action=)` / `cyclopts.convert(type_, tokens, converter=, name_transform=)` / `cyclopts.resolve_returncode(value)` | one-shot | module-level single-function run / standalone token->type coercion / return-value->exit-code resolution |

[ENTRYPOINT_SCOPE]: shell integration and docs
- rail: entry

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :---------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `App.interactive_shell(prompt=, quit=)`               | shell          | run an interactive REPL over the command set    |
|  [02]   | `App.install_completion` / `generate_completion` / `register_install_completion_command` | completion | install/emit shell completion             |
|  [03]   | `App.generate_docs(...)`                              | docs           | emit command documentation                      |
|  [04]   | `App.help_print` / `App.version_print` / `App.show`   | render         | render help/version output                      |

## [04]-[IMPLEMENTATION_LAW]

[ENTRY_TOPOLOGY]:
- app law: the daemon/CLI surface is one `App` whose subcommands are registered with `@app.command`; a new command is one decorated function, never a parallel parser or a `get`/`run`/`exec` command family. Nested concerns use sub-`App`s registered via `app.command(subapp)`, not flat name proliferation.
- annotation law: options and arguments are `Annotated[T, Parameter(...)]`; cyclopts derives the parser from the signature, so there is no manual `add_argument` wiring. `env_var`, `negative`, `converter`, and per-parameter `validator` ride on the `Parameter`, not a parallel mechanism.
- constrained-type law: numeric/path/format constraints use the `cyclopts.types` aliases (`Port`, `PositiveInt`, `ExistingFile`, `Email`, ...) directly in the signature; `cyclopts.validators.*` is reserved for cross-argument constraints (`MutuallyExclusive`, `LimitedChoice`, `all_or_none`) bound on a `Group`, not re-implementations of the single-value checks the type library already owns.
- config law: layered configuration uses the `config.*` sources in priority order (later overrides earlier), aligned with the `pydantic-settings` source chain; the CLI and settings model share one configuration vocabulary, and `config.Env` reuses the same env-prefix the settings model declares.
- meta/middleware law: cross-cutting argv handling (auth, profile selection, logging level) lives on `App.meta` as a `Dispatcher`, threaded once before dispatch — not duplicated in every command body.
- result-action law: command return values map to process exit through `App.result_action` (a `ResultAction` enum member or its equivalent string literal, e.g. `'return_int_as_exit_code_else_zero'`); a `Result` `Ok` maps to exit 0 and `Error(BoundaryFault)` to a coded non-zero exit with a rendered message — never an uncaught traceback. `resolve_returncode(value)` is the standalone return->exit-code resolver for one-shot dispatch.
- validation law: a `CycloptsError` (any of `ValidationError`/`CoercionError`/`MissingArgumentError`/...) is rendered to stderr and mapped to a non-zero exit at the boundary; the traceback never escapes. Standalone token coercion outside an `App` uses `cyclopts.convert(type_, tokens)`; `cyclopts.bind` is a submodule, not a callable, so coercion is never invoked through it.
- async law: the daemon entrypoint uses `run_async(backend='asyncio')` (or `'trio'`) so the CLI shares the structured-concurrency lane model the anyio runtime owns; `UNSET` (not `None`) distinguishes "argument not supplied" from "supplied as null".

[LOCAL_ADMISSION]:
- The entrypoint surface composes one `App`; the runtime owns no second argument parser (no argparse/click/typer).
- CLI configuration composes the `pydantic-settings` source chain; the two share configuration, never duplicate it. `config.Toml`/`Env` and the settings model read the same files/env.
- The `cyclopts.types` library is the first-choice input-typing surface; a domain type with its own `msgspec`/pydantic validation is wrapped by a `Parameter(converter=...)` rather than re-validated by hand.

[STACK_LAW]:
- `App.command(func)` with `Annotated[cyclopts.types.Port, Parameter(env_var="PORT")]` parameters -> `App.run_async(backend='asyncio')` under the anyio lane -> command returns a `Result` -> `App.result_action` (`ResultAction`/string) -> exit code: one rail, no manual argv parsing or `sys.exit` scattering.
- `config.Toml(...)`/`config.Env(prefix=...)` chain shares files and env-prefix with the `pydantic-settings` `BaseSettings` sources; the settings model is the typed projection of the same layered configuration.
- `App.meta` `Dispatcher` is the single pre-dispatch seam where the OTel span/log-context for the invoked command is opened, not per-command boilerplate.

[RAIL_LAW]:
- Package: `cyclopts`
- Owns: the daemon/CLI entrypoint grammar — command/sub-app/meta registry, annotation-driven parameters, the constrained-type library, cross-argument validators, layered config, result-action mapping, completion, and the interactive shell
- Accept: one `App`, `@app.command`/sub-`App` registration, `Annotated[..., Parameter(...)]`, `cyclopts.types.*` constrained inputs, `validators.*` group constraints, `config.*` sources aligned with `pydantic-settings`, `App.meta` middleware, `run_async`, `ResultAction`/`resolve_returncode` exit mapping
- Reject: parallel argument parsers (argparse/click/typer), manual `add_argument` wiring, `get`/`run`/`exec` command families for one concept, hand-rolled single-value validators duplicating `cyclopts.types`, duplicated configuration, uncaught tracebacks at the CLI boundary, `None` used where `UNSET` is meant, treating the `cyclopts.bind` submodule as a callable bind entrypoint
