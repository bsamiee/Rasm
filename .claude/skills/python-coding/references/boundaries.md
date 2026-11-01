# [BOUNDARIES]

Covers one command's flow from process input through decoded streams, rendered templates, resources, and planned changes to an exit code.

## [01]-[STREAMS]

Record streams decode one record at a time where a malformed record leaves its siblings valid, and a bound the run states is a type argument the decoder enforces. `Fault`, `Outcome`, `Reason` with an `UNWRITABLE` reason, `content`, and `accumulated` come from the package faults, byte reader, and accumulator:

```python
class Quota[L: int](msgspec.Struct, frozen=True, kw_only=True):
    name: str
    limit: L | None = None


def quotas(limit: int, raw: bytes, /) -> tuple[Outcome[Quota[int]], ...]:
    decoded = catch(msgspec.json.Decoder(Quota[Annotated[int, msgspec.Meta(ge=1, le=limit)]]).decode, exception=msgspec.DecodeError)
    return tuple(decoded(line).map_error(partial(Fault.of, Reason.INVALID, f"line {index}")) for index, line in enumerate(raw.splitlines(), start=1) if line.strip())
```

`quotas(4, raw)` fails a line holding `"limit": 9` with `` Expected `int` <= 4 - at `$.limit` `` and decodes every other line:
- Unparametrized `Quota` decodes `limit` as its bound `int`, and each reader states its own bound as the type argument
- One decoder serves a run, built where its bound is known
- Numbering runs before the blank-line filter, and fault subjects keep each line's position in the input
- `Decoder.decode_lines` reads JSON Lines whole and fails the batch on one bad line

## [02]-[TEMPLATES]

Text another program parses builds from a t-string, and one renderer applies the target's escape to each interpolation while literal parts pass unchanged:

```python
def rendered(escape: Callable[[str], str], template: Template, /) -> str:
    return "".join(escape(format(convert(part.value, part.conversion), part.format_spec)) if isinstance(part, Interpolation) else part for part in template)


def argv(template: Template, /) -> tuple[str, ...]:
    return tuple(shlex.split(rendered(shlex.quote, template)))
```

`argv(t"grep -n {pattern} {path}")` keeps each value one argument whatever it holds, `rendered(html.escape, ...)` serves markup, and a query renders `rendered(lambda _: "?", template)` beside `template.values` as its parameters:
- `convert` applies `!r`, `!s`, and `!a`, and `format` applies the spec after it
- Iterating a `Template` yields literal strings and `Interpolation` parts in source order, empty strings left out
- `template.values` holds each interpolation's value in order, and the placeholder follows the database driver's `paramstyle`
- `Interpolation.expression` holds each field's source text for a renderer's error to name

## [03]-[RESOURCES]

Writes that replace a file stage their content in a uniquely named file beside their target and commit through `replace`, and readers see the old file or the new one:

```python
@catch(exception=OSError)
def _replaced(target: Path, data: bytes, /) -> Path:
    with NamedTemporaryFile(dir=target.parent, prefix=f".{target.name}.", delete_on_close=False) as staged:
        staged.write(data)
        staged.flush()
        os.fsync(staged.fileno())
        staged.close()
        Path(staged.name).replace(target)
    return target


def written(target: Path, data: bytes, /) -> Outcome[Path]:
    return _replaced(target, data).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))
```

Staged files share their target's folder, and `replace` stays one rename on one filesystem:
- Unique staged names keep concurrent writers apart, and a failed write leaves its target unchanged and its staged file removed
- `fsync` before `replace` keeps a crash from committing an empty file
- Windows refuses to rename an open file, `close` precedes `replace`, and context exit finds the moved name gone
- Staged files hold mode `0o600`, and a target other users read takes `Path(staged.name).chmod(<mode>)` before `replace`
- `@catch` covers `_replaced` whole, every call in it raises `OSError`
- `ExitStack.pop_all()` hands a caller every resource a function opened, to enter or to join its own stack through `enter_context`
- Process-lifetime state is a resource the entry scope yields to its dependents

## [04]-[PLANS]

Changes to an external target are a plan a pure function derives from wanted and live state, and the boundary applies each change once. `held` reads a folder's live links, `planned` returns the targets to unlink and the pairs to link as differences of both mappings' items, and `reconciled` performs the plan:

```python
def held(folder: Path, /) -> Outcome[frozendict[Path, Path]]:
    return catch(lambda: frozendict({entry: entry.readlink() for entry in folder.iterdir() if entry.is_symlink()}), exception=OSError)().map_error(partial(Fault.of, Reason.UNREADABLE, str(folder)))


def planned(wanted: Mapping[Path, Path], live: Mapping[Path, Path], /) -> tuple[frozenset[Path], frozendict[Path, Path]]:
    return frozenset(target for target, _ in live.items() - wanted.items()), frozendict(wanted.items() - live.items())


def _changed(target: Path, change: Callable[[], object], /) -> Outcome[Path]:
    return catch(change, exception=OSError)().map(lambda _: target).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))


def reconciled(wanted: Mapping[Path, Path], live: Mapping[Path, Path], /) -> Outcome[tuple[Path, ...]]:
    unlinked, linked = planned(wanted, live)
    return accumulated((*(_changed(target, target.unlink) for target in unlinked), *(_changed(target, partial(target.symlink_to, source)) for target, source in linked.items())))
```

`held(folder).bind(partial(reconciled, wanted))` reads live state once and applies its plan, `held(folder).map(partial(planned, wanted))` is its dry run, and a missing folder fails before any change:
- `ItemsView` is a `Set`, and `-` over mappings' items yields each pair the left side holds and the right lacks
- Retargeted links unlink before they link, and a converged folder plans empty values and changes nothing
- `accumulated` applies every change and returns each failed one's fault in plan order

## [05]-[ENTRY]

Commands are the boundary: their signature is the command line, settings arrive as validated parameters, and a run returns one outcome the app translates into an exit code. `run` partitions each line's outcome into granted quotas and refused faults and writes both:

```python
def run(source: Path, target: Path, /, *, limit: PositiveInt = 4) -> Outcome[int]:
    return content(source).map(partial(quotas, limit)).bind(partial(_summarized, target))


def _summarized(target: Path, lines: tuple[Outcome[Quota[int]], ...], /) -> Outcome[int]:
    granted = tuple(*line.to_option().to_list() for line in lines)
    refused = tuple(*(fault.message for fault in line.swap().default_value(())) for line in lines)
    return written(target, msgspec.json.encode({"granted": granted, "refused": refused})).map(lambda _: 1 if refused else 0)
```

`PositiveInt` refuses `--limit 0` before any read, and `limit` reaches the decoder bound once:
- Refused lines leave the run `Ok` with status 1, an unreadable source or unwritable target fails the run with its fault
- Summaries hold fault messages, msgspec raises `TypeError` encoding a `Fault` with its caught cause
- Commands alone read process input, every other function takes the value it needs as an argument
