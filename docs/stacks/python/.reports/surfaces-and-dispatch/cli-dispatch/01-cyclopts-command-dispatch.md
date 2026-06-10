# [CLI_COMMAND_DISPATCH]

[DISPATCH_MODEL]:
- `cyclopts.App` is the CLI dispatch table: `App._commands: dict[str, App | CommandSpec]`; `parse_commands` is an O(1) token-walk into that dict (with an O(n) camelCase-normalize fallback marked for removal in v5), returning `(command_chain, execution_path, remaining_tokens)`, then `parse_args` binds remaining tokens to the chosen `App.default_command` signature. [cyclopts 4.16.1 core.py, installed, 2026-06-09]
- A CLI command surface is a polymorphic entrypoint discriminating on the subcommand token then binding typed parameters from annotations — the same dispatch law as the in-process forms, with the discriminant being argv rather than a type or tag.

[REGISTRATION_FORMS]:
- `@app.command` (bare; name via `name_transform`, default pascal-to-snake), `@app.command(name=, alias=, **App_kwargs)`, `app.command(sub_app)` for nesting, `app.command("module.path:attr", name=)` for a LAZY-import `CommandSpec` resolved and cached on first invocation, and `app.command(sub_app, name="*")` to flatten a sub-app into the parent namespace. [core.py 4.16.1, 2026-06-09]
- The lazy `CommandSpec` populates the dispatch table eagerly from string keys while deferring the module import until first invocation — the AOT registration primitive for large CLIs.
- `@app.default` registers the handler for the app itself (no subcommand token consumed); single-command apps use it.

[INTERCEPTION_LAYERS]:
- `@app.meta.default(*tokens: str)` is the token-level interceptor: it receives raw tokens before binding, may return an `int` exit code, and is the canonical seam for tracing, auth, and result-to-exit mapping; the parent app's parameters are consumed from the leading token stream.
- The `Dispatcher` Protocol `(command, BoundArguments, ignored) -> Any` is the post-binding executor seam (e.g. wrap the final `command(*bound.args, **bound.kwargs)` in a span). Two interception levels: meta is pre-binding, `Dispatcher` is post-binding.

[TYPED_BINDING]:
- Binding is annotation-driven and reads `dataclass`/`attrs`/`pydantic.BaseModel` (model_fields, aliases, description)/`NamedTuple`/`TypedDict`. `Parameter` fields include converter, validator, alias, negative, group, env_var, consume_multiple, count, json_dict, json_list. `Group(name, validator, default_parameter)` runs a cross-parameter validator over the `ArgumentCollection`; built-in validators are `Number`, `Path`, `MutuallyExclusive`, `LimitedChoice`, `all_or_none`. [parameter.py / group.py 4.16.1, 2026-06-09]
- `config.Env(prefix)`, `config.Toml/Json/Yaml(path, ...)`, and `config.Dict` inject layered defaults before binding via a callable `(App, commands, ArgumentCollection) -> Any`; `config.Dict` is the in-memory test seam.

[ROP_INTEGRATION]:
- Commands return domain rails (`Result`, typed carriers, or `int`); the meta handler maps the return to an exit code. `CycloptsError` parse and bind failures convert at the dispatch boundary and never propagate into command logic, so domain commands never raise.
- `result_action="return_value"` with `exit_on_error=False, print_error=False` gives full programmatic and test embedding with zero process-exit side effects; combined with `config.Dict` it is the canonical test-embedding pattern.

[ASYNC]:
- `App.__call__` detects `iscoroutinefunction` and runs via `backend=Literal["asyncio","trio"]`; `App.run_async` embeds in an existing loop; async commands work transparently. [_run.py 4.16.1, 2026-06-09]
