---
name: python-coding-cyclopts
description: "Use when writing or reviewing a cyclopts app, covering commands, parameters, user classes, meta apps, configuration, completion, and exit codes"
---

# [PYTHON_CODING_CYCLOPTS]

## [01]-[COMMANDS]

Positional-only parameters are arguments, keyword-only ones options, `*args` takes remaining tokens, and docstring `Args` lines give help. Commands share an option through one `type` alias over `Annotated`:

```python
type Executable = Annotated[ResolvedExistingFile, Parameter(env_var="HOST_PATH")]


async def launch(folder: Path, /, *arguments: str, executable: Executable) -> Outcome[int]:
    """Run the host executable inside a folder.

    Args:
        folder: Folder the host runs in.
        arguments: Host arguments, after `--` when one opens with `-`.
        executable: Host executable.

    Returns:
        Host exit status, or the `FAILED` fault of a host that never starts.
    """
    try:
        done = await anyio.run_process((executable, *arguments), cwd=folder, stdout=None, stderr=None, check=False)
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, executable.name, error))
    return Ok(done.returncode)
```

`launch <folder> -- -c <code>` passes `-c` to host, `HOST_PATH` fills an omitted `--executable`, and host status becomes command result. Registration reads a signature at once or at first use:
- `@app.command` evaluates annotations at registration: `lazy` imports resolve, later-declared classes and `TYPE_CHECKING` imports raise `NameError`
- `"<module>:<attr>"` registrations import their module at the command's run, its own `--help`, or an `app["<name>"]` read
- Root `--help` lists an import-path registration with its `help=` text alone, blank without one, and imports no command module
- Names derive from `__name__` (lowercase, `_` to `-`, outer `_` stripped), and `name=` replaces a name
- `alias=` adds runnable names, and `synonym=` adds words that fail with a suggestion of the command
- `app.command(App(name="<group>"))` nests a command group, `app.command(<app>, name="*")` merges its commands into their parent
- `async def` commands run under `asyncio.run` from `app()`, trio with `App(backend="trio")`, or `await app.run_async(tokens)` in a running loop
- Unset `version=` reads the distribution named like the root package, then `__version__`, else `0.0.0`
- Distributions named apart from their package pass `version=partial(importlib.metadata.version, "<distribution>")`

## [02]-[PARAMETERS]

Hints decide coercion, and `Annotated[T, Parameter(...)]` configures one parameter:

| [INDEX] | [HINT]           | [COERCION]                                                                                             |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `bool` option    | Flag with `--no-<name>`, `--<name>=<value>` takes `true`, `yes`, `false`, `no` or an initial, `1`, `0` |
|  [02]   | `int`            | `0x`, `0o`, and `0b` prefixes, a decimal rounds half to even (`2.5` to `2`)                            |
|  [03]   | Union            | Members left to right, `int \| str` reads `5` as `int`, `str \| int` as text                           |
|  [04]   | `T \| None`      | `none` and `null` give `None` unless `str` precedes `None`                                             |
|  [05]   | `Enum`           | Member names through `name_transform` (`dark-red`), values rejected                                    |
|  [06]   | `Flag` enum      | `--<name>.<member>` flag per member or repeated `--<name> <member>`, members OR together               |
|  [07]   | `list[T]` option | One token per occurrence, `--empty-<name>` gives `[]`, `consume_multiple=True` takes a run             |
|  [08]   | Scalar option    | Second occurrence raises `RepeatArgumentError`, `allow_repeating=True` keeps the last                  |
|  [09]   | `dict[str, T]`   | `--<name>.<key> <value>`, a JSON object fails coercion                                                 |
|  [10]   | `timedelta`      | Duration strings (`30s`, `1h30m`)                                                                      |
|  [11]   | `StdioPath`      | `-` reads stdin and writes stdout through every `Path` read and write method, unbound ones included    |

Bounds are `cyclopts.types` aliases or `validators` members. `Parameter` options merge per attribute from inner to outer `Annotated`, and an outer `validator=` or `converter=` replaces the inner one:
- `PositiveInt` under `validator=validators.Number(modulo=2)` accepts `-2`, `ExistingPath` under `validator=validators.Path(ext="txt")` a missing file
- Aliases adding a check restate every check they keep (`validators.Number(gt=0, modulo=2)`)
- `cyclopts.types` aliases adding `converter=` alone (`ResolvedExistingFile`) keep their inner validator
- `annotated_types` and pydantic `Field` bounds on plain parameters apply only while `pydantic` is in `sys.modules`, `msgspec.Meta` bounds never apply
- Validators run on defaults, `n: PositiveInt = -1` fails an empty command line with `ValidationError`
- `Pair[int]` from `type Pair[T] = tuple[T, T]` drops its `Parameter` and fails every parse with `'typing.TypeAliasType' object is not callable`
- `negative=()` drops `--no-` and `--empty-` flags, `count=True` on an `int` counts `-vvv`, `allow_leading_hyphen=True` takes values opening with `-`
- `App(default_parameter=Parameter(short_alias=True))` makes each initial a short flag, uppercase when taken (`--host` as `-H`, `-h` is help)

## [03]-[USER_CLASSES]

Shared options go in one pydantic model under `@Parameter(name="*")`, each field an option with `Attributes` help:

```python
@Parameter(name="*", group=Group.create_ordered("Remote"))
class Remote(BaseModel, frozen=True):
    """Server every remote command reaches.

    Attributes:
        scheme: Transport the server speaks.
        host: Server host name.
        port: Server port, absent for the scheme's default port.
    """

    scheme: Literal["https", "http"] = "https"
    host: str
    port: Annotated[int, Field(ge=1, le=65535)] | None = None


def locate(path: str, /, *, remote: Remote) -> Outcome[str]:
    """Print the address of a server path.

    Args:
        path: Path on the server.

    Returns:
        Server address of the path.
    """
    authority: str = remote.host if remote.port is None else f"{remote.host}:{remote.port}"
    return Ok(urljoin(f"{remote.scheme}://{authority}", path))
```

`locate /a --host <host>` returns `https://<host>/a`, `--port 0` exits 2 with pydantic's message, and help shows `Remote` fields in one panel placed by creation order among `Group.create_ordered` panels. Class kind decides which library coerces and validates:

| [INDEX] | [CLASS]          | [BINDING]                                                                   |
| :-----: | :--------------- | :-------------------------------------------------------------------------- |
|  [01]   | Pydantic model   | Pydantic coerces and validates every field through a `TypeAdapter`, exits 2 |
|  [02]   | `attrs` class    | Cyclopts coerces, a validator's `ValueError` escapes with exit code 1       |
|  [03]   | `TypedDict`      | `**fields: Unpack[<TypedDict>]` binds each field as a top-level option      |
|  [04]   | `msgspec.Struct` | `--<name> <token>` puts the raw token string in its first field, unchecked  |

- Classes under `Parameter(name="*")` with every field defaulted need a default instance, and registration raises `ValueError` without one
- Dataclass, `attrs`, and pydantic parameters bind `--<name>.<field>` keys or one JSON token (`--box '{"w": 3}'`)
- `msgspec.Struct` parameters reject keys whatever `accepts_keys` holds
- Omitted `NotRequired` keys of a `TypedDict` stay absent, and fields typed `ReadOnly[T]` fail every parse with `Cannot instantiate typing.ReadOnly`

## [04]-[VALIDATION]

Validators take `(type_, value)`, `type_` without `Annotated`, and return `None`. Raised `ValueError`, `TypeError`, and `AssertionError` become a `ValidationError` naming the option, and every other exception escapes raw:

| [INDEX] | [VALIDATOR]                    | [SCOPE]   | [CHECK]                                  |
| :-----: | :----------------------------- | :-------- | :--------------------------------------- |
|  [01]   | `validators.Number`            | Parameter | `gt`, `gte`, `lt`, `lte`, `modulo`       |
|  [02]   | `validators.Path`              | Parameter | `exists`, `file_okay`, `dir_okay`, `ext` |
|  [03]   | `validators.MutuallyExclusive` | Group     | At most one member supplied              |
|  [04]   | `validators.LimitedChoice`     | Group     | Supplied count from `min` to `max`       |
|  [05]   | `validators.all_or_none`       | Group     | Every member or none                     |
|  [06]   | `validator=` on an `App`       | Command   | Supplied arguments as keywords           |

- Converters take `(type_, tokens)` and read each `Token.value`
- `Group("<title>", validator=validators.LimitedChoice(1, 2))` fails a call that supplies no member of the group, `allow_none=True` accepts it
- `App(validator=f)` checks its own default command after binding, `app.command(f, validator=v)` checks one command
- Root `App(validator=f)` refusals raise `NotImplementedError` from the error panel and exit 1, a named command or group refusal exits 2
- `Parameter(validator=lambda _, value: msgspec.convert(value, Size))` reports `msgspec.ValidationError` (a `ValueError`) on the option

## [05]-[META_APPS]

Meta apps own options every command shares and the scope each command runs in. `session` binds its options, `parse_args` binds the rest to an uncalled command, `ignored` maps `parse=False` parameters to types `session` supplies:

```python
app = App(result_action=(exited, "sys_exit"))
app.meta.group_parameters = Group.create_ordered("Session")


@app.meta.default
def session(*tokens: Annotated[str, Parameter(show=False, allow_leading_hyphen=True)], keep: bool = False) -> object:
    """Run one command in a scratch folder.

    Args:
        keep: Keep the scratch folder after the command.

    Returns:
        Result of the bound command.
    """
    command, bound, ignored = app.parse_args(tokens)
    with TemporaryDirectory(delete=not keep) as scratch:
        supplied = frozendict({Path: Path(scratch), datetime: datetime.now(UTC)})
        return command(*bound.args, **bound.kwargs, **{name: supplied[kind] for name, kind in ignored.items()})


@app.command
def stamp(name: str, /, *, scratch: Annotated[Path, Parameter(parse=False)], now: Annotated[datetime, Parameter(parse=False)]) -> Outcome[Path]:
    """Write the start time into a scratch file.

    Args:
        name: Scratch file name.

    Returns:
        Scratch file path, or its `UNWRITABLE` fault.
    """
    return changed(Path.write_text, scratch / name, now.isoformat())
```

`stamp --help` lists `--keep` in its `Session` panel, and `--scratch` is an unknown option. Called app and parse mode decide binding:
- `app.meta()` runs `session`, and `app()` skips it and leaves every unparsed parameter unfilled, so a meta app's script entry point names `app.meta`
- `parse=` takes a `bool` or a regex searched against parameter names, and `parse=False` on a positional parameter without default raises `ValueError`
- Meta apps answer `--help` and `--version` before `session` runs
- `parse_args` applies no `result_action`, a nested `app(tokens)` inside `session` returns its command result, and the meta app maps it once
- `parse_mode="fallthrough"` (default) binds a meta option after the command name, and a command option of one name wins
- `parse_mode="strict"` raises `UnknownOptionError` for a meta option after the command name
- Meta options naming a settings file set `app.config` before `parse_args`, and `config.Dict` values never bind a meta parameter

## [06]-[CONFIGURATION]

`App(config=...)` takes a source or a sequence, values resolve from command line, `env_var=`, each source in order, then signature default:

```python
package: str = __name__.partition(".")[0]
app = App(config=(config.Env(f"{package.upper()}_", command=True), config.Toml(f"{package}.toml", search_parents=True)))
```

| [INDEX] | [SOURCE]                      | [KEYS]                                      |
| :-----: | :---------------------------- | :------------------------------------------ |
|  [01]   | `config.Env("<PREFIX>_")`     | `<PREFIX>_<COMMAND>_<PARAMETER>` uppercased |
|  [02]   | `config.Toml`, `Json`, `Yaml` | One table per command under `root_keys`     |
|  [03]   | `config.Dict(mapping)`        | File keys from memory                       |

- `config.Env` names a variable for every parameter, positional ones and user class fields included, `command=False` drops the command segment
- Help lists each variable `config.Env` or `env_var=` names, and `env_var=` names a variable a host defines (`HOST_PATH`)
- `search_parents=True` finds a file in the working folder or an ancestor, and a missing file supplies nothing
- `must_exist=True` raises raw `FileNotFoundError` at parse outside `CycloptsError`
- `root_keys=("tool", "<name>")` reads a `pyproject.toml` table, and keys at its root fill the default command
- Unknown keys in file and dict sources raise `UnknownOptionError` unless `allow_unknown=True`, unknown variables pass unread
- Source values pass the coercion and validators tokens pass, and errors name their source (`TOOL_WRITE_COUNT provided by env`)
- Variables for a multi-token hint split on whitespace, a `Path` collection on `os.pathsep`, and `Parameter(env_var_split=...)` replaces the split

## [07]-[COMPLETION]

`--install-completion` (`app.register_install_completion_command()`) writes a script for the current shell and sources it from its startup file. Scripts hold every command, option, and `Literal` or `Enum` choice, and `Path` hints use shell file completion. `completer=` lists values only a run knows from other arguments in its context:

```python
def entries(context: CompletionContext, /) -> dict[str, str]:
    match context["root"].value:
        case Path() as root:
            return {entry.name: "folder" if entry.info.is_dir() else entry.suffix for entry in root.iterdir()}
        case _:
            return {}


def remove(name: Annotated[str, Parameter(completer=entries)], /, *, root: ResolvedExistingDirectory) -> Outcome[Path]:
    """Remove one file under a folder.

    Args:
        name: File name under the folder.
        root: Folder holding the file.

    Returns:
        Removed file path, or its `UNWRITABLE` fault.
    """
    return changed(Path.unlink, root / name)
```

`remove --root <folder> <Tab>` offers each entry of `<folder>` with its kind, and offers none until `--root` or `TOOL_ROOT` names a folder:
- Shell scripts prefix-match every candidate against the typed word, and completers read `context.incomplete` only to narrow a costly lookup
- `context["<name>"].value` holds another argument's coerced value from tokens, environment, or config, or its default when omitted
- Failed coercion leaves `UNSET` in `value`, and completion runs no validator
- Completers return a `str`, an iterable of values or `(value, description)` pairs, or a `{value: description}` mapping
- Cyclopts discards a completer's exception, and its parameter offers no candidate
- `generate_completion` and `--install-completion` import every command module, a `completer` run imports its own through `<prog> __complete <words>`

## [08]-[ENTRY_POINT]

Entry-point modules hold one `App`, one registration by import path per command with its `help=` summary, and one result action. `exited` maps help, a status, a value, and faults:

```python
"""Run host executables in prepared folders."""

from pathlib import Path
import sys

from cyclopts import App, Parameter, config
lazy from expression import Result
lazy import msgspec

lazy from hosts.faults import EXITS, Outcome


def exited(result: Outcome[object] | None, /) -> int:
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=int(status)):
            return status
        case Result(tag="ok", ok=value):
            sys.stdout.buffer.write(msgspec.json.encode(value, enc_hook=str) + b"\n")
            return 0
        case Result(error=faults):
            sys.stderr.writelines(f"{fault.message}\n" for fault in faults)
            return EXITS[faults[0].reason]


package: str = __name__.partition(".")[0]
app = App(help=__doc__, result_action=(exited, "sys_exit"), config=config.Env(f"{package.upper()}_", command=False), default_parameter=Parameter(negative=()))
app.command(f"{package}._runs:launch", help="Run the host executable inside a folder.")
app.command(f"{package}._remote:locate", help="Print the address of a server path.")
app.command(f"{package}._entries:remove", help="Remove one file under a folder.")
app.register_install_completion_command()
```

`launch` exits with its host's status, `locate` writes its address as JSON on stdout, and each fault goes to stderr. `result_action` maps a return value to process outcome, default `"print_non_int_sys_exit"` prints any non-`int` result, `Error` included, and exits 0:

| [INDEX] | [MODE]                                | [BEHAVIOR]                                 |
| :-----: | :------------------------------------ | :----------------------------------------- |
|  [01]   | `"sys_exit"`                          | Exits with an `int`, `False` as 1, else 0  |
|  [02]   | `"sys_exit_if_non_zero_else_return"`  | Exits on a nonzero code, returns 0         |
|  [03]   | `"return_int_as_exit_code_else_zero"` | Returns a code for a host to exit with     |
|  [04]   | `"return_value"`                      | Returns a command result                   |
|  [05]   | Callable                              | Returns a value for its next pipeline step |

- Results with a zero-argument `__cyclopts_returncode__` set the code in every mode that exits or returns one
- Parse and validation failures print a panel to stderr and exit 2, and `error_formatter=str` prints the message alone
- Help and version print to stdout, exit 0, and pass `None` on
- Exceptions a command raises escape `app()` under every error setting and exit 1 with a traceback, `KeyboardInterrupt` exits 130
- `app(tokens, result_action="return_value", exit_on_error=False, print_error=False)` returns command result, raises `CycloptsError` on parse failure
