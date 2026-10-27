---
name: python-coding-cyclopts
description: "Use when writing or reviewing a cyclopts command-line application, covering commands, parameters, sessions, and exit codes"
---

# [PYTHON_CODING_CYCLOPTS]

Covers cyclopts applications from command registration through parameter binding, sessions, and configuration to an exit code. Commands are boundary functions returning python-coding's `Outcome` with `Fault` and `Reason` faults, and each command docstring is help cyclopts renders.

## [01]-[COMMANDS]

Command signatures are their command line: positional-only parameters are arguments, keyword-only parameters are options, and a docstring `Args` section is per-parameter help. Command modules hold each function with every import its annotations read:

```python
def write(line: str, /, *, count: Annotated[PositiveInt, Parameter(alias="-n")] = 1, target: Annotated[ResolvedPath, Parameter(env_var="TOOL_TARGET")]) -> Outcome[Path]:
    """Write copies of a line to a file.

    Args:
        line: Text of each line.
        count: Copies to write.
        target: File to write.
    """
    return (
        catch(target.write_text, exception=OSError)(f"{line}\n" * count, encoding="utf-8")
        .map(lambda _: target)
        .map_error(lambda error: (Fault(reason=Reason.UNWRITABLE, subject=str(target), cause=error),))
    )
```

Entry modules hold an `App` and one registration by import path per command, and `exited` maps each command result to an exit code:

```python
app = App(name="tool", help=__doc__, result_action=(exited, "sys_exit"))
app.command("tool.items:write")
app.command("tool.items:limit")

if __name__ == "__main__":
    app()
```

Registration reads a command's signature at once or at its first use:
- `@app.command` and `app.command(f)` return their function unchanged, and both checkers keep its signature for direct calls
- Registration evaluates every annotation, a class declared after its command or a name imported under `TYPE_CHECKING` raises `NameError` there
- `lazy` imports in a command module resolve at registration, and classes a command reads sit above it or in an imported module
- `"<module>:<attr>"` registrations import their module when the command runs, prints its own `--help`, or `app["<name>"]` reads it
- Root `--help` lists a lazy command with its `help=` text alone and imports no command module
- Import paths naming no module raise raw `ImportError` at first run outside `CycloptsError`, `app["<name>"]` over each name resolves every path
- Names derive from `__name__` (lowercase, `_` to `-`, outer `_` stripped), `name=` replaces a name and `alias=` adds runnable names
- `@app.default` runs when no command name matches, a second raises `CommandCollisionError`, and an app without one prints help on empty tokens
- `app.command(App(name="<group>"))` nests a command group, `app.command(<app>, name="*")` merges its commands into their parent
- `async def` commands run under `asyncio.run` from `app()` or trio under `App(backend="trio")`, `await app.run_async(tokens)` inside a loop
- `version=` unset reads an installed distribution named like the root import package, then its `__version__`, else prints `0.0.0`
- Distributions named apart from their import package pass `version=partial(importlib.metadata.version, "<distribution>")`

## [02]-[PARAMETERS]

Hints decide coercion, and `Annotated[T, Parameter(...)]` configures one parameter:

| [INDEX] | [HINT]           | [COERCION]                                                                         |
| :-----: | :--------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `bool` option    | Flag with `--no-<name>`, `--<name>=<value>` takes `true`, `yes`, `1`, `false`, `0` |
|  [02]   | `int`            | `0x`, `0o`, and `0b` prefixes, a decimal rounds half to even (`2.5` to `2`)        |
|  [03]   | Union            | Members left to right, `int \| str` reads `5` as `int`, `str \| int` as text       |
|  [04]   | `T \| None`      | `none` and `null` give `None` unless `str` precedes `None`                         |
|  [05]   | `Enum`           | Member names through `name_transform` (`dark-red`), values rejected                |
|  [06]   | `list[T]` option | One token per occurrence, `consume_multiple=True` takes a run                      |
|  [07]   | Scalar option    | Second occurrence raises `RepeatArgumentError`                                     |
|  [08]   | `dict[str, T]`   | `--<name>.<key> <value>`, a JSON object fails coercion                             |
|  [09]   | `timedelta`      | Unit runs (`30s`, `1h30m`)                                                         |
|  [10]   | `*args: T`       | Positional tail, tokens after `--` bind as values                                  |
|  [11]   | User class       | Dataclass, attrs, pydantic, `NamedTuple`, `TypedDict` fields as `--<name>.<field>` |

`Parameter` options merge attribute by attribute from inner to outer `Annotated`, and an outer `validator=` or `converter=` replaces the inner one whole: `Annotated[PositiveInt, Parameter(validator=validators.Number(modulo=2))]` accepts `-2`, and an alias adding a check restates every check it keeps (`validators.Number(gt=0, modulo=2)`). Cyclopts ignores `msgspec.Meta`, and records with `Meta` bounds enter through `**fields: Unpack[<TypedDict>]` typed by the record's own aliases and one `convert`:

```python
type Size = Annotated[int, msgspec.Meta(ge=1)]
type Depth = Annotated[int, msgspec.Meta(ge=0)]


class Fields(TypedDict):
    size: Size
    depth: NotRequired[Depth]


class Limits(msgspec.Struct, frozen=True, kw_only=True):
    size: Size
    depth: Depth = 0


@catch(exception=msgspec.ValidationError)
def _limits(fields: Fields, /) -> Limits:
    return msgspec.convert(fields, Limits)


def limit(**fields: Unpack[Fields]) -> Outcome[Limits]:
    """Validate limits for one batch."""
    return _limits(fields).map_error(lambda error: (Fault(reason=Reason.INVALID, subject=Limits.__name__, cause=error),))
```

`--size 0` reaches `convert` and returns an `Error` with exit code 1, and `--size x` fails at parse with exit code 2. Parameter forms bind by kind:
- `TypedDict` fields become top-level options, an omitted `NotRequired` key stays absent, and an unknown option raises `UnknownOptionError`
- `TypedDict` fields typed `ReadOnly[T]` fail every parse with `Cannot instantiate typing.ReadOnly`
- `msgspec.Struct` is no user class, `--<name> <token>` passes one raw token to its constructor (`Item(size='1')`), and `--<name>.<field>` fails
- Generic aliases (`type Pair[T] = tuple[T, T]` as `Pair[int]`) register and fail every parse with `'typing.TypeAliasType' object is not callable`
- `cyclopts.types` aliases adding `converter=` alone (`ResolvedExistingFile`) keep their inner validator
- `ExistingPath` under an outer `Parameter(validator=validators.Path(ext="txt"))` accepts a missing file
- Validators run on defaults, `n: PositiveInt = -1` fails an empty command line with `ValidationError`
- `env_var=` reads a variable when a command line omits its option, and coercion errors name that variable
- `negative=()` drops `--no-<name>`, `count=True` on an `int` counts `-vvv`, `allow_leading_hyphen=True` takes values opening with `-`
- `parse=False` removes a parameter from binding and rejects its option, on a positional parameter without a default it raises `ValueError`
- `Parameter(name="*")` on a class or its parameter drops its `--<name>.` prefix, an all-default class there takes a default instance

## [03]-[VALIDATION]

Validators take `(type_, value)` with `type_` stripped of `Annotated` and return `None`. Raised `ValueError`, `TypeError`, and `AssertionError` become a `ValidationError` naming the option, and every other exception escapes raw. Converters take `(type_, tokens)` and read each `Token.value`:

| [INDEX] | [VALIDATOR]                    | [SCOPE]   | [CHECK]                                  |
| :-----: | :----------------------------- | :-------- | :--------------------------------------- |
|  [01]   | `validators.Number`            | Parameter | `gt`, `gte`, `lt`, `lte`, `modulo`       |
|  [02]   | `validators.Path`              | Parameter | `exists`, `file_okay`, `dir_okay`, `ext` |
|  [03]   | `validators.MutuallyExclusive` | Group     | At most one member supplied              |
|  [04]   | `validators.LimitedChoice`     | Group     | Supplied count from `min` to `max`       |
|  [05]   | `validators.all_or_none`       | Group     | Every member or none                     |
|  [06]   | `validator=` on an `App`       | Command   | Supplied arguments as keywords           |

- `Group("<title>", validator=...)` names a help panel and validates its members together, `Parameter(group=...)` places a parameter in it
- `LimitedChoice(1, 2)` fails a call that supplies no member, `allow_none=True` accepts it
- `App(validator=f)` checks its own default command after binding, `app.command(f, validator=v)` checks one command
- Command validators receive defaulted parameters only when supplied, and a `ValueError` reports as `Invalid values for command "<name>"`
- `msgspec.ValidationError` is a `ValueError`, a validator calling `convert` against a record alias reports its message on the option

## [04]-[SESSIONS]

Meta apps own options every command shares and the scope each command runs in. `session` binds its own options, `parse_args` binds every other token without calling, and `ignored` maps each `parse=False` parameter its command declares to a type, which `session` supplies by name:

```python
app = App(name="tool", help=__doc__, result_action=(exited, "sys_exit"))
app.meta.group_parameters = Group("Session", sort_key=0)


@app.meta.default
def session(*tokens: Annotated[str, Parameter(show=False, allow_leading_hyphen=True)], keep: bool = False) -> object:
    """Run one command in a scratch folder.

    Args:
        keep: Keep the scratch folder after the command.
    """
    command, bound, ignored = app.parse_args(tokens)
    with TemporaryDirectory(delete=not keep) as scratch:
        supplied = frozendict(scratch=Path(scratch), now=datetime.now(UTC))
        return command(*bound.args, **bound.kwargs, **{name: supplied[name] for name in ignored})


@app.command
def stamp(name: str, /, *, scratch: Annotated[Path, Parameter(parse=False)], now: Annotated[datetime, Parameter(parse=False)]) -> Outcome[Path]:
    """Write the start time into a scratch file.

    Args:
        name: Scratch file name.
    """
    target = scratch / name
    return (
        catch(target.write_text, exception=OSError)(now.isoformat(), encoding="utf-8")
        .map(lambda _: target)
        .map_error(lambda error: (Fault(reason=Reason.UNWRITABLE, subject=str(target), cause=error),))
    )


if __name__ == "__main__":
    app.meta()
```

Session commands run under their entry call and parse mode:
- `app.meta()` is the entry call, and `app()` skips `session` and leaves every `parse=False` parameter unfilled
- Meta apps answer `--help` and `--version` before `session` runs, and `result_action` receives `None`
- `parse_args` applies no `result_action`, a nested `app(tokens)` inside `session` returns its command result, and the meta app maps it once
- `parse_mode="fallthrough"` (default) binds a meta option after the command name, and a command option of one name wins
- `parse_mode="strict"` raises `UnknownOptionError` for a meta option after the command name
- Meta options naming a settings file set `app.config` before `parse_args`, and `config.Dict` values never bind a meta parameter

## [05]-[CONFIGURATION]

`App(config=...)` takes one source or a sequence, and each source fills arguments a command line and every earlier source left unset. Values resolve from a command line, then `Parameter(env_var=...)`, then each source in order, then a signature default:

```python
app = App(name="tool", config=(config.Env("TOOL_", command=True), config.Toml("tool.toml", search_parents=True)))
```

| [INDEX] | [SOURCE]                      | [KEYS]                                      |
| :-----: | :---------------------------- | :------------------------------------------ |
|  [01]   | `config.Env("<PREFIX>_")`     | `<PREFIX>_<COMMAND>_<PARAMETER>` uppercased |
|  [02]   | `config.Toml`, `Json`, `Yaml` | One table per command under `root_keys`     |
|  [03]   | `config.Dict(mapping)`        | File keys from memory                       |

- `command=False` on `config.Env` drops the command segment (`<PREFIX>_<PARAMETER>`), and unknown variables pass unread
- `search_parents=True` finds a file in the working folder or an ancestor, and a missing file supplies nothing
- `must_exist=True` raises raw `FileNotFoundError` at parse outside `CycloptsError`
- `root_keys=("tool", "<name>")` reads a `pyproject.toml` table, and keys at that table's root fill the default command
- Unknown keys in file and dict sources raise `UnknownOptionError` unless `allow_unknown=True`
- Source values pass the coercion and validators tokens pass, and errors name their source (`TOOL_WRITE_COUNT from env`)

## [06]-[EXIT_CODES]

`result_action` turns a command's return value into a process outcome. Default `"print_non_int_sys_exit"` exits with an `int`, 0 for `True` and 1 for `False`, and prints any other value before exiting 0, a returned `Error` included. Entries map each result through a callable ahead of `"sys_exit"`, and that callable receives `None` after `--help` and `--version`:

```python
def exited(outcome: Outcome[object] | None, /) -> int:
    match outcome:
        case Result(tag="error", error=faults):
            sys.stderr.writelines(f"{fault.message}\n" for fault in faults)
            return 1
        case _:
            return 0
```

| [INDEX] | [MODE]                                | [BEHAVIOR]                                 |
| :-----: | :------------------------------------ | :----------------------------------------- |
|  [01]   | `"print_non_int_sys_exit"`            | Prints a non-`int` result, exits           |
|  [02]   | `"sys_exit"`                          | Exits with an `int` or `bool` code, else 0 |
|  [03]   | `"sys_exit_if_non_zero_else_return"`  | Exits on a nonzero code, returns 0         |
|  [04]   | `"return_int_as_exit_code_else_zero"` | Returns a code for a host to exit with     |
|  [05]   | `"return_value"`                      | Returns a command result                   |
|  [06]   | Callable                              | Returns a value for its next pipeline step |

- Pipelines run left to right on each previous step's value, and an unknown mode name raises `ValueError` at `App` construction
- Results with a zero-argument `__cyclopts_returncode__` set the code in every mode that exits or returns one
- Parse and validation failures print a panel to stderr and exit 2, help and version print to stdout and exit 0
- Exceptions a command raises escape `app()` under every error setting and exit 1 with a traceback, `KeyboardInterrupt` exits 130
- Hosts embedding an app call `app(tokens, result_action="return_value", exit_on_error=False, print_error=False)`
- Embedded calls return a command result and raise a `CycloptsError` for a parse failure
