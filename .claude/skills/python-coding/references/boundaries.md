# [BOUNDARIES]

Covers one entry's flow from argv and environment through codecs and resources to an exit code.

## [01]-[SETTINGS]

Entries decode settings once from every source a run reads. `Environment` maps a process variable onto its field through `rename`, `tomllib` parses the file it names, and `key=value` arguments override its table before one `convert` with `strict=False` validates the merge:

```python
class Environment(msgspec.Struct, frozen=True, kw_only=True, rename={"config": "TOOL_CONFIG"}):
    config: str


class Settings(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    target: str
    limit: Annotated[int, msgspec.Meta(ge=1)] = 4
    verbose: bool = False


@catch(exception=msgspec.ValidationError)
def _environment(environ: Mapping[str, str], /) -> Environment:
    return msgspec.convert(environ, Environment)


@catch(exception=ValueError)
def _parsed(raw: bytes, /) -> dict[str, object]:
    return tomllib.loads(raw.decode("utf-8", errors="strict"))


@catch(exception=msgspec.ValidationError)
def _converted(fields: dict[str, object], /) -> Settings:
    return msgspec.convert(fields, Settings, strict=False)


def _table(path: Path, /) -> Result[dict[str, object], Faults]:
    return (
        catch(path.read_bytes, exception=OSError)()
        .map_error(partial(Fault.caught, Reason.UNREADABLE, path))
        .bind(lambda raw: _parsed(raw).map_error(partial(Fault.caught, Reason.INVALID, path)))
    )


def configured(environ: Mapping[str, str], argv: Sequence[str], /) -> Result[Settings, Faults]:
    overrides = {key: value for key, _, value in (pair.partition("=") for pair in argv)}
    return (
        _environment(environ)
        .map_error(partial(Fault.caught, Reason.INVALID, Environment.__name__))
        .bind(lambda environment: _table(Path(environment.config)))
        .bind(lambda table: _converted(table | overrides).map_error(partial(Fault.caught, Reason.INVALID, Settings.__name__)))
    )
```

- `Fault`, `Faults`, and `Reason` are the package's fault record, its tuple alias, and its reason enum
- `convert` over `os.environ` reads the variables `Environment` declares and ignores every other one
- `TOMLDecodeError` and `UnicodeDecodeError` are both `ValueError`, one `catch` over `_parsed` keeps either as cause
- `table | overrides` lets arguments win, and `strict=False` turns `limit=8` and `verbose=true` into an `int` and a `bool`
- `Meta` bounds and `forbid_unknown_fields` hold under `strict=False`, `limit=0` and `color=red` fail as `ValidationError`

## [02]-[CODECS]

Wire records keep an optional field as `T | None` for the codec and hand consumers its `Option` through a property. Request streams decode line by line, each malformed line becomes faults that name its line number, and every other line decodes:

```python
class Quota(msgspec.Struct, frozen=True, kw_only=True):
    name: str
    limit: int | None = None

    @property
    def ceiling(self) -> Option[int]:
        return Option.of_optional(self.limit)


QUOTA = msgspec.json.Decoder(Quota)


def quota(index: int, line: bytes, /) -> Result[Quota, Faults]:
    return catch(QUOTA.decode, exception=msgspec.DecodeError)(line).map_error(partial(Fault.caught, Reason.INVALID, f"line {index}"))


def quotas(raw: bytes, /) -> tuple[Result[Quota, Faults], ...]:
    return tuple(quota(index, line) for index, line in enumerate(raw.splitlines(), start=1) if line.strip())
```

- `catch(QUOTA.decode, ...)` types as `Result[Quota, DecodeError]`, `catch(msgspec.json.decode, ...)(line, type=Quota)` types its value as `Any`
- Numbering runs before the blank-line filter, fault subjects keep each line's position in the input
- Causes keep msgspec's message with its field path (``Expected `str`, got `int` - at `$.name` ``)

## [03]-[RESOURCES]

Writes that replace a file stage their content beside their target, commit through `replace` after the write, and `staging` removes the staged name on every exit:

```python
@contextmanager
def staging(target: Path, /) -> Generator[Path]:
    staged = target.with_name(f".{target.name}.partial")
    try:
        yield staged
    finally:
        staged.unlink(missing_ok=True)


@catch(exception=OSError)
def _written(target: Path, content: bytes, /) -> Path:
    with staging(target) as staged:
        staged.write_bytes(content)
        staged.replace(target)
    return target


def written(target: Path, content: bytes, /) -> Result[Path, Faults]:
    return _written(target, content).map_error(partial(Fault.caught, Reason.UNWRITABLE, target))
```

- Staged names share their target's folder, `replace` stays one rename on one filesystem, and readers see the old file or the new one
- Failed writes leave their target as it was, and release after a commit finds no staged name
- `_written` is the one typed reader for its block, every call in it raises `OSError`

Acquisitions with a count known at runtime enter one `ExitStack`, and `pop_all` moves every release to a new stack once the last acquisition succeeds. A failed open closes each stream opened before it, and success hands the caller a stack owning every stream:

```python
@catch(exception=OSError)
def _opened(paths: Sequence[Path], /) -> tuple[ExitStack, tuple[BufferedReader, ...]]:
    with ExitStack() as stack:
        streams = tuple(stack.enter_context(path.open("rb")) for path in paths)
        return stack.pop_all(), streams


def opened(paths: Sequence[Path], /) -> Result[tuple[ExitStack, tuple[BufferedReader, ...]], Faults]:
    return _opened(paths).map_error(partial(Fault.caught, Reason.UNREADABLE, paths))
```

- Callers enter the returned stack (`with owner:`) or hand it to their entry's stack through `root.enter_context(owner)`

## [04]-[ENTRY]

Entry modules decode settings, bind them into handlers, run the batch, and translate its outcome once. `granted` takes settings ahead of its subject, `run` binds them through `partial` once per run, and `exited` maps a settings or write failure to 2, a refused line to 1, and a clean run to 0:

```python
class Summary(msgspec.Struct, frozen=True, kw_only=True):
    granted: tuple[str, ...]
    refused: tuple[str, ...]

    @classmethod
    def of(cls, outcomes: Iterable[Result[Quota, Faults]], /) -> Summary:
        settled = tuple(outcomes)
        return cls(
            granted=tuple(*outcome.map(lambda quota: (quota.name,)).default_value(()) for outcome in settled),
            refused=tuple(*outcome.swap().map(lambda faults: tuple(fault.message for fault in faults)).default_value(()) for outcome in settled),
        )

    @property
    def code(self) -> int:
        return 1 if self.refused else 0


SUMMARY = msgspec.json.Encoder()


def granted(settings: Settings, quota: Quota, /) -> Result[Quota, Faults]:
    return Ok(quota).filter_with(lambda entry: entry.ceiling.map(lambda limit: limit <= settings.limit).default_value(True), partial(Fault.of, Reason.EXCEEDED))


def run(settings: Settings, raw: bytes, /) -> Result[Summary, Faults]:
    summary = Summary.of(outcome.bind(partial(granted, settings)) for outcome in quotas(raw))
    return written(Path(settings.target), SUMMARY.encode(summary)).map(lambda _: summary)


def exited(outcome: Result[Summary, Faults], /) -> int:
    match outcome.tag:
        case "ok":
            sys.stderr.writelines(f"{line}\n" for line in outcome.ok.refused)
            return outcome.ok.code
        case "error":
            sys.stderr.writelines(f"{fault.message}\n" for fault in outcome.error)
            return 2


def main(environ: Mapping[str, str], argv: Sequence[str], raw: bytes, /) -> int:
    return exited(configured(environ, argv).bind(lambda settings: run(settings, raw)))


if __name__ == "__main__":
    raise SystemExit(main(os.environ, sys.argv[1:], sys.stdin.buffer.read()))
```

- `main` receives process input as arguments, and the `__main__` guard reads `os.environ`, `sys.argv`, and stdin once
- Summaries hold fault messages, msgspec raises `TypeError` encoding a `Fault` with its caught cause
- Refused lines leave the run's result `Ok`, and `Summary.code` carries them into the exit code
- Unwritable targets fail a run after its batch, its summary reaches no reader, and its exit code is 2
