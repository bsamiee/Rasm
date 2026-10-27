# [BOUNDARIES]

Covers one entry's flow from argv and environment through codecs, templates, resources, and planned changes to an exit code.

## [01]-[SETTINGS]

Entries decode settings once from every source a run reads. `Environment` maps a process variable onto its field through `rename`, `tomllib` parses the file it names, and `key=value` arguments override its table before one `convert` with `strict=False` validates the merge. `Fault`, `Outcome`, `Reason`, and `accumulated` come from the package faults with an `UNWRITABLE` reason, and `content` is the package byte reader:

```python
class Environment(msgspec.Struct, frozen=True, kw_only=True, rename={"config": "TOOL_CONFIG"}):
    config: str


class Settings(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    target: str
    limit: Annotated[int, msgspec.Meta(ge=1)] = 4
    verbose: bool = False


@catch(exception=msgspec.ValidationError)
def _environment(environ: Mapping[str, str], /) -> Environment:
    return msgspec.convert(environ, Environment, strict=False)


@catch(exception=ValueError)
def _parsed(raw: bytes, /) -> dict[str, object]:
    return tomllib.loads(raw.decode())


@catch(exception=msgspec.ValidationError)
def _converted(fields: Mapping[str, object], /) -> Settings:
    return msgspec.convert(fields, Settings, strict=False)


def _table(path: Path, /) -> Outcome[dict[str, object]]:
    return content(path).bind(lambda raw: _parsed(raw).map_error(partial(Fault.of, Reason.INVALID, str(path))))


def configured(environ: Mapping[str, str], argv: Sequence[str], /) -> Outcome[Settings]:
    overrides = {key: value for key, _, value in (pair.partition("=") for pair in argv)}
    return (
        _environment(environ)
        .map_error(partial(Fault.of, Reason.INVALID, Environment.__name__))
        .bind(lambda environment: _table(Path(environment.config)))
        .bind(lambda table: _converted(table | overrides).map_error(partial(Fault.of, Reason.INVALID, Settings.__name__)))
    )
```

`configured` stops at the first failing source:
- `convert` over `os.environ` reads the variables `Environment` declares and ignores every other one, and builds no `Path` from text
- `TOMLDecodeError` and `UnicodeDecodeError` are both `ValueError`, one `catch` over `_parsed` keeps either as cause
- `table | overrides` lets arguments win, and `strict=False` turns `limit=8` and `verbose=true` into an `int` and a `bool`
- `Meta` bounds and `forbid_unknown_fields` hold under `strict=False`, `limit=0` and `color=red` fail as `ValidationError`
- Bytes decode to text once at entry, and text encodes once at exit

## [02]-[CODECS]

`ceiling` reads the decoded `limit` of `Quota` as an `Option`. Request streams decode line by line, each malformed line becomes faults that name its line number, and every other line decodes:

```python
class Quota(msgspec.Struct, frozen=True, kw_only=True):
    name: str
    limit: int | None = None

    @property
    def ceiling(self) -> Option[int]:
        return Option.of_optional(self.limit)


QUOTA = msgspec.json.Decoder(Quota)


def quota(index: int, line: bytes, /) -> Outcome[Quota]:
    return catch(QUOTA.decode, exception=msgspec.DecodeError)(line).map_error(partial(Fault.of, Reason.INVALID, f"line {index}"))


def quotas(raw: bytes, /) -> tuple[Outcome[Quota], ...]:
    return tuple(quota(index, line) for index, line in enumerate(raw.splitlines(), start=1) if line.strip())
```

Numbering runs before the blank-line filter, and fault subjects keep each line's position in the input:
- Causes keep msgspec's message with its field path (``Expected `str`, got `int` - at `$.name` ``)
- `Decoder.decode_lines` and `Encoder.encode_lines` read and write JSON Lines whole, one bad line failing the batch
- Persisted state encodes through a msgspec JSON or MessagePack codec, `pickle` and `shelve` run code from the store on load

## [03]-[TEMPLATES]

Text another program parses (a shell line, markup, a query) builds from a t-string, and one renderer escapes each interpolation while literal parts pass unchanged:

```python
def rendered(escape: Callable[[str], str], template: Template, /) -> str:
    return "".join(escape(format(convert(part.value, part.conversion), part.format_spec)) if isinstance(part, Interpolation) else part for part in template)


def argv(template: Template, /) -> tuple[str, ...]:
    return tuple(shlex.split(rendered(shlex.quote, template)))
```

`argv(t"grep -n {name} file")` returns `name` as one argument whatever it holds, `rendered(html.escape, ...)` serves markup through the same renderer, and an f-string in either place splices raw text:
- `convert` applies `!r`, `!s`, and `!a`, and `format` applies the spec after it
- Iterating a `Template` yields literal strings and `Interpolation` parts in source order, empty strings left out
- `Interpolation.expression` holds each field's source text for a renderer's error to name

## [04]-[RESOURCES]

Writes that replace a file stage their content beside their target and commit through `replace` after the write, and `staging` registers the staged name's removal before yielding it. `subscribed` holds any register and unregister pair a host exposes, registering the release only after the register call succeeds:

```python
@contextmanager
def staging(target: Path, /) -> Generator[Path]:
    with ExitStack() as stack:
        staged = target.with_name(f".{target.name}.partial")
        stack.callback(staged.unlink, missing_ok=True)
        yield staged


@catch(exception=OSError)
def _written(target: Path, data: bytes, /) -> Path:
    with staging(target) as staged:
        staged.write_bytes(data)
        staged.replace(target)
    return target


def written(target: Path, data: bytes, /) -> Outcome[Path]:
    return _written(target, data).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))


@contextmanager
def subscribed[H](subscribe: Callable[[], H], unsubscribe: Callable[[H], object], subject: str, /) -> Generator[Outcome[None]]:
    with ExitStack() as stack:
        yield (
            catch(subscribe, exception=ValueError)()
            .map(lambda handle: stack.callback(unsubscribe, handle))
            .map(lambda _: None)
            .map_error(partial(Fault.of, Reason.FAILED, subject))
        )
```

Staged names share their target's folder, `replace` stays one rename on one filesystem, and readers see the old file or the new one:
- Failed writes leave their target as it was, and release after a commit finds no staged name
- `@catch` covers `_written` whole, every call in it raises `OSError`
- Process-lifetime state is a resource the entry scope yields to its dependents
- Libraries return their result type with their own faults, applications compose fallback order and caching around them

`_opened` hands its caller a stack owning every stream, to enter (`with owner:`) or to pass to its entry's stack (`root.enter_context(owner)`):

```python
@catch(exception=OSError)
def _opened(paths: Sequence[Path], /) -> tuple[ExitStack, tuple[BufferedReader, ...]]:
    with ExitStack() as stack:
        streams = tuple(stack.enter_context(path.open("rb")) for path in paths)
        return stack.pop_all(), streams
```

## [05]-[PLANS]

Changes to an external target are a plan of records a pure function derives from wanted and live state, and the boundary applies each change once. `held` reads the live links of a folder, `planned` returns the changes between both mappings, and `applied` performs one:

```python
class Step(msgspec.Struct, frozen=True, kw_only=True):
    target: Path


class Link(Step, frozen=True, kw_only=True):
    source: Path


class Unlink(Step, frozen=True, kw_only=True): ...


type Change = Link | Unlink


def held(folder: Path, /) -> frozendict[Path, Path]:
    return frozendict({entry: entry.readlink() for entry in folder.iterdir() if entry.is_symlink()})


def planned(wanted: Mapping[Path, Path], live: Mapping[Path, Path], /) -> tuple[Change, ...]:
    return (
        *(Unlink(target=target) for target, source in live.items() if wanted.get(target) != source),
        *(Link(target=target, source=source) for target, source in wanted.items() if live.get(target) != source),
    )


def applied(change: Change, /) -> Outcome[Path]:
    match change:
        case Link(target=target, source=source):
            done = catch(target.symlink_to, exception=OSError)(source)
        case Unlink(target=target):
            done = catch(target.unlink, exception=OSError)(missing_ok=True)
    return done.map(lambda _: target).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))


def reconciled(wanted: Mapping[Path, Path], folder: Path, /) -> Outcome[tuple[Path, ...]]:
    return accumulated(map(applied, planned(wanted, held(folder))))
```

Retargeted links unlink before they link, and a converged folder plans `()`:
- Plans are values a dry run prints and a check compares, `reconciled` adds only the call that performs them
- Live state reads once before planning, and a second `reconciled` over the same wanted mapping changes nothing
- Each change case names what it acts on, and `applied` matches the closed union once

## [06]-[ENTRY]

Entry modules decode settings, bind them into handlers, run the batch, and translate its outcome once. `granted` takes settings ahead of its subject, `run` binds them through `partial` once per run, and `exited` maps a settings or write failure to 2, a refused line to 1, and a clean run to 0:

```python
class Summary(msgspec.Struct, frozen=True, kw_only=True):
    granted: tuple[str, ...]
    refused: tuple[str, ...]

    @classmethod
    def of(cls, outcomes: Sequence[Outcome[Quota]], /) -> Summary:
        return cls(
            granted=tuple(*outcome.map(lambda entry: (entry.name,)).default_value(()) for outcome in outcomes),
            refused=tuple(*outcome.swap().map(lambda faults: tuple(fault.message for fault in faults)).default_value(()) for outcome in outcomes),
        )


SUMMARY = msgspec.json.Encoder()


def granted(settings: Settings, entry: Quota, /) -> Outcome[Quota]:
    return Ok(entry).filter_with(
        lambda held: held.ceiling.filter(lambda limit: limit > settings.limit).is_none(),
        lambda held: Fault.of(Reason.OUTSIDE, held.name, held.limit, member=f"a limit up to {settings.limit}"),
    )


def run(settings: Settings, raw: bytes, /) -> Outcome[Summary]:
    summary = Summary.of(tuple(outcome.bind(partial(granted, settings)) for outcome in quotas(raw)))
    return written(Path(settings.target), SUMMARY.encode(summary)).map(lambda _: summary)


def exited(outcome: Outcome[Summary], /) -> int:
    match outcome.tag:
        case "ok":
            sys.stderr.writelines(f"{line}\n" for line in outcome.ok.refused)
            return 1 if outcome.ok.refused else 0
        case "error":
            sys.stderr.writelines(f"{fault.message}\n" for fault in outcome.error)
            return 2


def main(environ: Mapping[str, str], argv: Sequence[str], raw: bytes, /) -> int:
    return exited(configured(environ, argv).bind(lambda settings: run(settings, raw)))


if __name__ == "__main__":
    raise SystemExit(main(os.environ, sys.argv[1:], sys.stdin.buffer.read()))
```

`Settings`, `configured`, `quotas`, and `written` come from the settings, codec, and resource flows:
- `main` receives process input as arguments, and the `__main__` guard reads `os.environ`, `sys.argv`, and stdin once
- Summaries hold fault messages, msgspec raises `TypeError` encoding a `Fault` with its caught cause
- Refused lines leave the run's result `Ok`
- Unwritable targets fail a run after its batch, and its summary reaches no reader
