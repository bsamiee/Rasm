# [PY_RUNTIME_API_CYCLOPTS]

`cyclopts` is the type-annotation-driven CLI framework backing runtime's `Entrypoint` owner (`serve.md` `[04]-[ENTRY]`, `companion_app`): `companion_app` returns one `cyclopts.App` whose `@app.command` methods bind arguments from type hints, `Parameter(env_var=...)` threads env binding, and `result_action` folds the railed command outcome to the process exit. That entry-grammar slice — App, command registration, `Parameter` env binding, result-action exit mapping, and the single `NonNegativeFloat` constrained grace — is the runtime consume. The layered `config` sources, the `validators` cross-argument family, the deep `cyclopts.types` constrained-type library, `App.meta` middleware, completion, docs, and the interactive shell are the Assay public-command owner's to mine (`tools/assay`); runtime holds no public CLI surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cyclopts`
- package: `cyclopts`
- import: `cyclopts`
- owner: `runtime`
- rail: entry
- license: `Apache-2.0`
- namespaces: `cyclopts`, `cyclopts.types`
- capability: the `Entrypoint` command grammar — one `App`, `@app.command` registration, `Parameter` env binding, `result_action`/`resolve_returncode` return-to-exit mapping, async dispatch, and the `cyclopts.types.NonNegativeFloat` constrained grace input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: app and parameter family (entry slice)
- rail: entry

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                                                      |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `App`                    | app           | the companion command registry, runner, and `result_action` owner                           |
|  [02]   | `Parameter`              | annotation    | option/flag spec; binds `env_var`; `negative`/`converter`/`group` unconsumed                |
|  [03]   | `ResultAction`           | enum          | typed post-command action; `result_action` also takes `'return_int_as_exit_code_else_zero'` |
|  [04]   | `types.NonNegativeFloat` | float bound   | the constrained grace input bound in the companion `serve` command                          |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: entry
- a `CycloptsError` subtype is rendered to stderr and mapped to a non-zero exit at the boundary; the traceback never escapes.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :--------------------- | :------------ | :--------------------------------- |
|  [01]   | `CycloptsError`        | fault base    | base for all CLI parse/bind faults |
|  [02]   | `ValidationError`      | fault         | validator rejection                |
|  [03]   | `CoercionError`        | fault         | coercion failure                   |
|  [04]   | `MissingArgumentError` | fault         | required argument absent           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: app construction, registration, and dispatch
- rail: entry
- defined on `App` (PUBLIC_TYPES [01]); the companion daemon's private entry, never a public CLI axis.

`run_async` binds `backend: Literal['asyncio','trio']`; the `-> Any` return tail is dropped from the SURFACE cells.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `App(name, *, help, result_action)`                            | build          | construct the companion app with return-to-exit policy |
|  [02]   | `App.command(obj=None, *, name, ...)`                          | register       | register a subcommand (function or sub-`App`)          |
|  [03]   | `App.__call__(tokens=None, *, exit_on_error=, result_action=)` | run            | parse argv and dispatch (sync)                         |
|  [04]   | `App.run_async(tokens=None, *, backend=None)`                  | run            | async dispatch on the asyncio backend                  |
|  [05]   | `cyclopts.resolve_returncode(value)`                           | resolve        | standalone return-value->exit-code resolver            |

## [04]-[IMPLEMENTATION_LAW]

[ENTRY_TOPOLOGY]:
- app law: the companion surface is one `App` whose subcommands register with `@app.command`; a new private command is one decorated method folding its railed outcome to the exit code, never a parallel parser or a `get`/`run`/`exec` command family.
- annotation law: arguments are `Annotated[T, Parameter(...)]`; cyclopts derives the parser from the signature, so there is no `add_argument` wiring. `env_var` rides on the `Parameter`, not a parallel mechanism.
- constrained-type law: the one runtime input constraint is `cyclopts.types.NonNegativeFloat` bound directly in the `serve` signature; the deep `cyclopts.types` library (path/format/width families) and the `cyclopts.validators` cross-argument family are UNCONSUMED in runtime and route to the Assay owner, never hand-rolled here.
- result-action law: command return values map to process exit through `App.result_action` (`'return_int_as_exit_code_else_zero'` / a `ResultAction` member); a railed `Ok` maps to exit `0` and an `Error(BoundaryFault)` folds to a non-zero exit — never an uncaught traceback. `resolve_returncode` is the standalone resolver.
- async law: the daemon entry dispatches the `async def` commands under the asyncio backend, sharing the anyio structured-concurrency lane the serve leg pins.
- scope law: the layered `cyclopts.config` source chain, the `cyclopts.validators` group family, the full `cyclopts.types` library beyond `NonNegativeFloat`, `App.meta`/`Dispatcher` middleware, `install_completion`/`generate_completion`, `generate_docs`, and `interactive_shell` are the Assay public-command owner's slice — a runtime fence consumes none of them, so this catalog never carries them.

[LOCAL_ADMISSION]:
- The `Entrypoint` owner composes one private `App`; the runtime owns no second argument parser (no argparse/click/typer) and no public CLI surface — public commands are reserved to the suite Assay command surface (`serve.md` `Entrypoint` boundary).
- CLI configuration is not layered in runtime: the companion reads its settings through the `pydantic-settings` `SettingsAdmission` spine, not a `cyclopts.config` chain — the config-source surface belongs to the Assay owner where a public command wants file/env layering.

[STACK_LAW]:
- `App(name, help, result_action="return_int_as_exit_code_else_zero")` with `@app.command async def serve(bind, *, grace: Annotated[NonNegativeFloat, Parameter(env_var="RASM_COMPANION_GRACE")])` -> the railed `ServerHost.serve` outcome folds `Ok`->`0`/`Error`->`1` through the result action (`serve.md` `companion_app`): one entry rail, no manual argv parsing, no `sys.exit` scattering, the traceback never escaping the boundary.

[RAIL_LAW]:
- Package: `cyclopts`
- Owns: the `Entrypoint` command grammar — one private companion `App`, `@app.command` registration, `Parameter` env binding, `result_action`/`resolve_returncode` return-to-exit mapping, async dispatch, and the `NonNegativeFloat` constrained grace input
- Accept: one `App` with `result_action`, `@app.command`/sub-`App` registration, `Annotated[..., Parameter(env_var=...)]`, `cyclopts.types.NonNegativeFloat`, `run_async(backend='asyncio')`, `ResultAction`/`resolve_returncode` exit mapping, stderr-rendered `CycloptsError` at the boundary
- Reject: parallel argument parsers (argparse/click/typer), manual `add_argument` wiring, a public CLI surface in runtime, layered `cyclopts.config` in runtime (the `pydantic-settings` spine owns settings), and cataloguing the `config`/`validators`/full-`types`/`meta`/completion/docs/shell surfaces the Assay owner mines
