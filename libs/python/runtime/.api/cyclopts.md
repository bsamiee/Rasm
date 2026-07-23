# [PY_RUNTIME_API_CYCLOPTS]

`cyclopts` is the type-annotation-driven CLI framework backing runtime's `Entrypoint` owner: `companion_app` (`serve.md`) returns one `App` whose `@app.command` methods derive arguments from type hints, `Parameter(env_var=...)` threads env binding, and `result_action` folds each railed outcome to the process exit. Runtime consumes only this private entry-grammar slice with no public CLI surface; broader cyclopts capability routes to the Assay owner (`tools/assay`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cyclopts`
- package: `cyclopts` (`Apache-2.0`)
- module: `cyclopts`
- namespaces: `cyclopts`, `cyclopts.types`
- rail: entry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: app and parameter family (entry slice)

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                                 |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `App`                    | app           | the companion command registry, runner, and `result_action` owner                            |
|  [02]   | `Parameter`              | annotation    | option/flag spec; binds `env_var`+`env_var_split`; `negative`/`converter`/`group` unconsumed |
|  [03]   | `ResultAction`           | enum          | typed post-command action; `result_action` also takes `'return_int_as_exit_code_else_zero'`  |
|  [04]   | `types.NonNegativeFloat` | float bound   | the constrained grace input bound in the companion `serve` command                           |

[PUBLIC_TYPE_SCOPE]: fault family
- a `CycloptsError` subtype renders to stderr and maps to a non-zero exit at the boundary.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :--------------------- | :------------ | :--------------------------------- |
|  [01]   | `CycloptsError`        | fault base    | base for all CLI parse/bind faults |
|  [02]   | `ValidationError`      | fault         | validator rejection                |
|  [03]   | `CoercionError`        | fault         | coercion failure                   |
|  [04]   | `MissingArgumentError` | fault         | required argument absent           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: app construction, registration, and dispatch
- defined on `App` (PUBLIC_TYPES [01]); the companion daemon's private entry, never a public CLI axis.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `App(name, *, help, result_action)`                            | ctor     | construct the companion app with return-to-exit policy |
|  [02]   | `App.command(obj=None, *, name, ...)`                          | instance | register a subcommand (function or sub-`App`)          |
|  [03]   | `App.__call__(tokens=None, *, exit_on_error=, result_action=)` | instance | parse argv and dispatch (sync)                         |
|  [04]   | `App.run_async(tokens=None, *, backend=None)`                  | instance | async dispatch on the asyncio backend                  |
|  [05]   | `cyclopts.resolve_returncode(value)`                           | static   | standalone return-value->exit-code resolver            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- app law: the companion surface is one `App` whose subcommands register through `@app.command`; a new private command is one decorated method folding its railed outcome to the exit code, never a parallel parser or a `get`/`run`/`exec` family.
- annotation law: arguments are `Annotated[T, Parameter(...)]` and cyclopts derives the parser from the signature — no `add_argument` wiring; `env_var` rides the `Parameter`, and an iterable env value decomposes through `env_var_split`, inert for the scalar grace bound.
- result-action law: command return values map to the process exit through `App.result_action` (`'return_int_as_exit_code_else_zero'` or a `ResultAction` member) — a railed `Ok` folds to exit `0`, an `Error(BoundaryFault)` to non-zero, a `CycloptsError` to stderr, and the traceback never escapes; `resolve_returncode` is the standalone resolver.

[STACKING]:
- `pydantic-settings`(`runtime/.api/pydantic-settings.md`): the companion `App` binds one env scalar per argument through `Parameter(env_var=...)`; layered file/env settings flow from the `BaseSettings` model the daemon admits, never a `cyclopts.config` chain.
- `anyio`(`libs/python/.api/anyio.md`): `App.run_async(backend='asyncio')` dispatches the `async def` commands inside the anyio structured-concurrency group `_supervised` opens, so command dispatch and the supervised worker lane share one cancellation scope.
- `Entrypoint`/`companion_app` (`serve.md`): `companion_app(routes, drains, charges)` builds `App(name, help, result_action='return_int_as_exit_code_else_zero')`, registers each `@app.command` method — `serve` binding `Annotated[NonNegativeFloat, Parameter(env_var='RASM_COMPANION_GRACE')]` — and folds the `ServerHost.serve` `Ok`/`Error` outcome to the exit, one entry rail with no manual argv parsing and no scattered `sys.exit`.

[LOCAL_ADMISSION]:
- `Entrypoint` composes one private `App`; runtime owns no second argument parser and no public CLI surface — public commands are reserved to the suite Assay command owner (`serve.md` `Entrypoint` boundary).

[RAIL_LAW]:
- Package: `cyclopts`
- Owns: the `Entrypoint` command grammar — one private companion `App`, `@app.command` registration, `Parameter` env binding, `result_action`/`resolve_returncode` return-to-exit mapping, async dispatch, and the `NonNegativeFloat` constrained grace input
- Accept: one `App` with `result_action`, `@app.command`/sub-`App` registration, `Annotated[..., Parameter(env_var=...)]`, `cyclopts.types.NonNegativeFloat`, `run_async(backend='asyncio')`, `ResultAction`/`resolve_returncode` exit mapping, stderr-rendered `CycloptsError` at the boundary
- Reject: parallel argument parsers (argparse/click/typer), manual `add_argument` wiring, a public CLI surface in runtime, and layered `cyclopts.config` in runtime — the `pydantic-settings` spine owns settings, and the Assay owner mines the `config`/`validators`/`types`/`meta`/completion/docs/shell surfaces
