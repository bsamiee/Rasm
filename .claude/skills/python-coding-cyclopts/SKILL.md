---
name: python-coding-cyclopts
description: "Use when writing or reviewing a cyclopts command-line application, covering commands, parameters, user classes, meta apps, configuration, completion, and exit codes"
---

# [PYTHON_CODING_CYCLOPTS]

Covers cyclopts applications from command registration through parameter binding, user classes, meta apps, configuration, and completion to an exit code. Snippets build on python-coding's `Fault`, `Outcome`, and `Reason` with an `UNWRITABLE` reason.

## [01]-[COMMANDS]

Commands are boundary functions with their signature as their command line: positional-only parameters are arguments, keyword-only parameters are options, a `*args` tail takes remaining tokens, and a docstring `Args` section is per-parameter help. Options shared across commands are one `type` alias over `Annotated`:

```python
type Executable = Annotated[ResolvedExistingFile, Parameter(env_var="HOST_PATH")]


async def launch(folder: Path, /, *arguments: str, executable: Executable) -> Outcome[int]:
    """Run the host executable inside a folder.

    Args:
        folder: Folder the host runs in.
        arguments: Host arguments, after `--` when one opens with `-`.
        executable: Host executable.
    """
    failed = partial(Fault.of, Reason.FAILED, executable.name)
    try:
        done = await anyio.run_process((executable, *arguments), cwd=folder, stdout=None, stderr=None, check=False)
    except OSError as error:
        return Error(failed(error))
    return catch(done.check_returncode, exception=CalledProcessError)().map(lambda _: 0).map_error(failed)
```

`launch <folder> -- -c <code>` passes `-c` to the host, tokens after `--executable <path>` join `arguments`, and `HOST_PATH` fills an omitted `--executable`. Registration reads a command's signature at once or at its first use:
- `@app.command` evaluates every annotation at registration, `lazy` imports resolve, and a class declared after its command or a name imported under `TYPE_CHECKING` raises `NameError`
- `@app.command` and `app.command(f)` return their function unchanged, and both checkers keep its signature for direct calls
- `"<module>:<attr>"` registrations import their module when the command runs, prints its own `--help`, or `app["<name>"]` reads it
- Root `--help` lists a path registration with its `help=` text alone, blank without one, and imports no command module
- Import paths naming no module raise raw `ImportError` at first run outside `CycloptsError`, `app["<name>"]` over each name resolves every path
- Names derive from `__name__` (lowercase, `_` to `-`, outer `_` stripped), `name=` replaces a name and `alias=` adds runnable names
- `app.command(App(name="<group>"))` nests a command group, `app.command(<app>, name="*")` merges its commands into their parent
- Decorators under `@app.command` apply `functools.wraps` to keep the parameters and docstring cyclopts reads
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
|  [06]   | `Literal`        | Listed values alone, help lists each                                               |
|  [07]   | `list[T]` option | One token per occurrence, `consume_multiple=True` takes a run                      |
|  [08]   | Scalar option    | Second occurrence raises `RepeatArgumentError`                                     |
|  [09]   | `dict[str, T]`   | `--<name>.<key> <value>`, a JSON object fails coercion                             |
|  [10]   | `timedelta`      | Unit runs (`30s`, `1h30m`)                                                         |

`Parameter` options merge attribute by attribute from inner to outer `Annotated`, and an outer `validator=` or `converter=` replaces the inner one whole:
- `Annotated[PositiveInt, Parameter(validator=validators.Number(modulo=2))]` accepts `-2`, and `Annotated[ExistingPath, Parameter(validator=validators.Path(ext="txt"))]` accepts a missing file
- Aliases adding a check restate every check they keep (`validators.Number(gt=0, modulo=2)`)
- `cyclopts.types` aliases adding `converter=` alone (`ResolvedExistingFile`) keep their inner validator
- Validators run on defaults, `n: PositiveInt = -1` fails an empty command line with `ValidationError`
- Generic aliases (`type Pair[T] = tuple[T, T]` as `Pair[int]`) register and fail every parse with `'typing.TypeAliasType' object is not callable`
- `negative=()` drops `--no-<name>`, `count=True` on an `int` counts `-vvv`, `allow_leading_hyphen=True` takes values opening with `-`
- `App(default_parameter=Parameter(...))` sets defaults for every parameter of every command under it

## [03]-[USER_CLASSES]

Commands sharing a set of options take one user class under `@Parameter(name="*")`, each field an option with its docstring `Attributes` line as help, and fields without a default are required options:

```python
@Parameter(name="*", group=Group("Remote", sort_key=1))
class Remote(BaseModel, frozen=True):
    """Server every remote command reaches.

    Attributes:
        scheme: Transport the server speaks.
        host: Server host name.
        port: Server port.
    """

    scheme: Literal["https", "http"] = "https"
    host: str
    port: Annotated[int, Field(ge=1, le=65535)] = 443

    @property
    def origin(self) -> str:
        return f"{self.scheme}://{self.host}:{self.port}"


def locate(path: str, /, *, remote: Remote) -> Outcome[str]:
    """Print the address of a server path.

    Args:
        path: Path on the server.
    """
    return Ok(urljoin(remote.origin, path))
```

`locate /a --host <host>` returns `https://<host>:443/a`, `--port 0` reports pydantic's message, and help groups `Remote` fields in their own panel. Class kinds decide which library coerces and validates:

| [INDEX] | [CLASS]                 | [BINDING]                                                              |
| :-----: | :---------------------- | :--------------------------------------------------------------------- |
|  [01]   | Pydantic model          | Pydantic coerces and validates every field, a refusal exits 2          |
|  [02]   | `attrs` class           | Cyclopts coerces, a validator's `ValueError` escapes with exit code 1  |
|  [03]   | Dataclass, `NamedTuple` | Cyclopts coerces, fields hold no bound                                 |
|  [04]   | `TypedDict`             | `**fields: Unpack[<TypedDict>]` binds each field as a top-level option |
|  [05]   | `msgspec.Struct`        | `--<name> <token>` passes one raw token to its constructor             |

Classes under `Parameter(name="*")` with every field defaulted need a default instance, and registration raises `ValueError` without one. Cyclopts ignores `msgspec.Meta`, and records with `Meta` bounds enter through a `TypedDict` typed by the record's own aliases and one `convert`:

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
    return _limits(fields).map_error(partial(Fault.of, Reason.INVALID, Limits.__name__))
```

`--size 0` reaches `convert` and exits 1 with `` Expected `int` >= 1 - at `$.size` ``, and `--size x` fails at parse with exit code 2:
- Omitted `NotRequired` keys stay absent, and an unknown option raises `UnknownOptionError`
- `TypedDict` fields typed `ReadOnly[T]` fail every parse with `Cannot instantiate typing.ReadOnly`
- `--<name>.<field>` on a `msgspec.Struct` parameter fails, and `accepts_keys=True` changes nothing

## [04]-[VALIDATION]

Validators take `(type_, value)` with `type_` stripped of `Annotated` and return `None`. Raised `ValueError`, `TypeError`, and `AssertionError` become a `ValidationError` naming the option, and every other exception escapes raw:

| [INDEX] | [VALIDATOR]                    | [SCOPE]   | [CHECK]                                  |
| :-----: | :----------------------------- | :-------- | :--------------------------------------- |
|  [01]   | `validators.Number`            | Parameter | `gt`, `gte`, `lt`, `lte`, `modulo`       |
|  [02]   | `validators.Path`              | Parameter | `exists`, `file_okay`, `dir_okay`, `ext` |
|  [03]   | `validators.MutuallyExclusive` | Group     | At most one member supplied              |
|  [04]   | `validators.LimitedChoice`     | Group     | Supplied count from `min` to `max`       |
|  [05]   | `validators.all_or_none`       | Group     | Every member or none                     |
|  [06]   | `validator=` on an `App`       | Command   | Supplied arguments as keywords           |

- Converters take `(type_, tokens)` and read each `Token.value`
- `Group("<title>", validator=...)` names a help panel and validates its members together, `Parameter(group=...)` places a parameter in it
- `LimitedChoice(1, 2)` fails a call that supplies no member, `allow_none=True` accepts it
- `App(validator=f)` checks its own default command after binding, `app.command(f, validator=v)` checks one command
- Command validators report a `ValueError` as `Invalid values for command "<name>"`
- Root `App(validator=f)` refusals raise `NotImplementedError` from the error panel and exit 1, a named command or group refusal exits 2
- `msgspec.ValidationError` is a `ValueError`, a validator calling `convert` against a record alias reports its message on the option

## [05]-[META_APPS]

Meta apps own options every command shares and the scope each command runs in. `session` binds its own options, `parse_args` binds every other token without calling, and `ignored` maps each `parse=False` parameter of the command to its type for `session` to supply by name:

```python
app = App(name="tool", result_action=(exited, "sys_exit"))
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
    return catch(target.write_text, exception=OSError)(now.isoformat(), encoding="utf-8").map(lambda _: target).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))


if __name__ == "__main__":
    app.meta()
```

`exited` is the entry's result action, and `stamp --help` lists `--keep` in its `Session` panel. Entry call and parse mode decide binding:
- `app.meta()` is the entry call, and `app()` skips `session` and leaves every `parse=False` parameter unfilled
- `parse=False` removes a parameter from binding and rejects its option, on a positional parameter without a default it raises `ValueError`
- Meta apps answer `--help` and `--version` before `session` runs
- `parse_args` applies no `result_action`, a nested `app(tokens)` inside `session` returns its command result, and the meta app maps it once
- `parse_mode="fallthrough"` (default) binds a meta option after the command name, and a command option of one name wins
- `parse_mode="strict"` raises `UnknownOptionError` for a meta option after the command name
- Meta options naming a settings file set `app.config` before `parse_args`, and `config.Dict` values never bind a meta parameter

## [06]-[CONFIGURATION]

`App(config=...)` takes one source or a sequence, and each source fills arguments a command line and every earlier source left unset. Values resolve from a command line, then `Parameter(env_var=...)`, then each source in order, then a signature default:

```python
app = App(name="tool", config=(config.Env("TOOL_", command=True), config.Toml("tool.toml", search_parents=True)))
```

| [INDEX] | [SOURCE]                      | [KEYS]                                      |
| :-----: | :---------------------------- | :------------------------------------------ |
|  [01]   | `config.Env("<PREFIX>_")`     | `<PREFIX>_<COMMAND>_<PARAMETER>` uppercased |
|  [02]   | `config.Toml`, `Json`, `Yaml` | One table per command under `root_keys`     |
|  [03]   | `config.Dict(mapping)`        | File keys from memory                       |

- `config.Env` names a variable for every parameter, positional ones and user class fields included, `command=False` drops the command segment
- Help lists each variable `config.Env` or `env_var=` names, and `env_var=` stays for a variable its host names (`HOST_PATH`)
- `search_parents=True` finds a file in the working folder or an ancestor, and a missing file supplies nothing
- `must_exist=True` raises raw `FileNotFoundError` at parse outside `CycloptsError`
- `root_keys=("tool", "<name>")` reads a `pyproject.toml` table, and keys at its root fill the default command
- Unknown keys in file and dict sources raise `UnknownOptionError` unless `allow_unknown=True`, unknown variables pass unread
- Source values pass the coercion and validators tokens pass, and errors name their source (`TOOL_WRITE_COUNT from env`)

## [07]-[COMPLETION]

`--install-completion` from `app.register_install_completion_command()` writes a script for the running shell and sources it from its startup file. Scripts hold every command, option, and `Literal` or `Enum` choice, and `completer=` lists values a run alone knows from sibling values in its context:

```python
def entries(context: CompletionContext, /) -> dict[str, str]:
    match context["root"].value:
        case Path() as root:
            return {entry.name: "folder" if entry.is_dir() else entry.suffix for entry in root.iterdir() if entry.name.startswith(context.incomplete)}
        case _:
            return {}


def remove(name: Annotated[str, Parameter(completer=entries)], /, *, root: ResolvedExistingDirectory) -> Outcome[Path]:
    """Remove one file under a folder.

    Args:
        name: File name under the folder.
        root: Folder holding the file.
    """
    target = root / name
    return catch(target.unlink, exception=OSError)().map(lambda _: target).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))
```

`remove --root <folder> <Tab>` offers each entry of `<folder>` with its kind, and no candidate before `--root`:
- `generate_completion` and `--install-completion` import every command module a path registration names
- Each completion of a `completer` parameter runs `<prog> __complete <words>` and imports its command's module
- `context["<name>"].value` holds a sibling's coerced value, its default when omitted, or `UNSET` when coercion fails, and no validator runs
- Completers return a `str`, an iterable of values or `(value, description)` pairs, or a `{value: description}` mapping
- Cyclopts discards a completer's exception, and its parameter offers no candidate

## [08]-[ENTRY]

Entry modules hold one `App`, one registration by import path per command, and one result action. `exited` maps help, a status, a value, a failed child, and faults:

```python
"""Run host executables in prepared folders."""

lazy import subprocess
lazy import sys

from cyclopts import App, Parameter, config
lazy from expression import Result
lazy import msgspec

lazy from tool.faults import Fault, Outcome


def exited(result: Outcome[object] | None, /) -> int:
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=int(status)):
            return status
        case Result(tag="ok", ok=value):
            sys.stdout.buffer.write(msgspec.json.encode(value, enc_hook=str) + b"\n")
            return 0
        case Result(error=(Fault(cause=subprocess.CalledProcessError(returncode=status)) as fault,)):
            sys.stderr.write(f"{fault.message}\n")
            return status
        case Result(error=faults):
            sys.stderr.writelines(f"{fault.message}\n" for fault in faults)
            return 1


app = App(name="tool", help=__doc__, result_action=(exited, "sys_exit"), config=config.Env("TOOL_", command=False), default_parameter=Parameter(negative=()))
app.command("tool.runs:launch")
app.command("tool.remote:locate")
app.command("tool.entries:remove")
app.register_install_completion_command()

if __name__ == "__main__":
    app()
```

`launch` exits with its host's status, `locate` writes its address as JSON on stdout, and each fault goes to stderr. `result_action` turns a command's return value into a process outcome, and default `"print_non_int_sys_exit"` prints a non-`int` result and exits 0, a returned `Error` included:

| [INDEX] | [MODE]                                | [BEHAVIOR]                                 |
| :-----: | :------------------------------------ | :----------------------------------------- |
|  [01]   | `"sys_exit"`                          | Exits with an `int`, `False` as 1, else 0  |
|  [02]   | `"sys_exit_if_non_zero_else_return"`  | Exits on a nonzero code, returns 0         |
|  [03]   | `"return_int_as_exit_code_else_zero"` | Returns a code for a host to exit with     |
|  [04]   | `"return_value"`                      | Returns a command result                   |
|  [05]   | Callable                              | Returns a value for its next pipeline step |

- Pipelines run left to right, and an unknown mode name raises `ValueError` at `App` construction
- Results with a zero-argument `__cyclopts_returncode__` set the code in every mode that exits or returns one
- Parse and validation failures print a panel to stderr and exit 2, help and version print to stdout, exit 0, and pass `None` on
- Exceptions a command raises escape `app()` under every error setting and exit 1 with a traceback, `KeyboardInterrupt` exits 130
- Embedded calls `app(tokens, result_action="return_value", exit_on_error=False, print_error=False)` return a command result and raise a `CycloptsError` for a parse failure
