# [BOUNDARIES]

Covers one command's flow from process input to its outcome.

## [01]-[STREAMS]

Record streams decode one record at a time where a malformed record leaves its siblings valid, and a bound the run states is a type argument the decoder enforces:

```python
class Quota[L: int](msgspec.Struct, frozen=True, kw_only=True):
    name: str
    limit: L


def quotas(limit: int, raw: bytes, /) -> tuple[Outcome[Quota[int]], ...]:
    decoded = catch(msgspec.json.Decoder(Quota[Annotated[int, msgspec.Meta(ge=1, le=limit)]]).decode, exception=msgspec.DecodeError)
    return tuple(decoded(line).map_error(partial(Fault.of, Reason.INVALID, f"line {index}")) for index, line in enumerate(raw.splitlines(), start=1) if line.strip())
```

`quotas(4, raw)` fails a line holding `"limit": 9` with `` Expected `int` <= 4 - at `$.limit` `` and decodes every other line.

## [02]-[TEMPLATES]

Text another program parses builds from a t-string, and one renderer applies the target's escape to each interpolation while literal parts pass unchanged:

```python
def rendered(escape: Callable[[str], str], template: Template, /) -> str:
    return "".join(escape(format(convert(part.value, part.conversion), part.format_spec)) if isinstance(part, Interpolation) else part for part in template)


def argv(template: Template, /) -> tuple[str, ...]:
    return tuple(shlex.split(rendered(shlex.quote, template)))
```

`argv(t"grep -n {pattern} {path}")` keeps each value one argument whatever it holds, `rendered(html.escape, ...)` serves markup, and a query renders `rendered(lambda _: "?", template)` beside `template.values` as its parameters:
- Iterating a `Template` yields literal strings and `Interpolation` parts in source order, empty strings left out
- `template.values` holds each interpolation's value in order, and the placeholder follows the database driver's `paramstyle`

## [03]-[FILE_WRITES]

Changes to a path are one function over the change and its arguments, returning the path or its `UNWRITABLE` fault. Writes that replace a file go to a uniquely named temporary file beside their target and commit through `replace`, and readers see the old file or the new one:

```python
def changed[*Ts](change: Callable[[Path, *Ts], object], target: Path, /, *args: *Ts) -> Outcome[Path]:
    return catch(change, exception=OSError)(target, *args).map(lambda _: target).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))


def rewritten(target: Path, data: bytes, /) -> None:
    with NamedTemporaryFile(dir=target.parent, prefix=f".{target.name}.", delete_on_close=False) as staged:
        staged.write(data)
        staged.flush()
        os.fsync(staged.fileno())
        staged.close()
        Path(staged.name).replace(target)
```

`changed(rewritten, target, data)` replaces a file, and `changed(Path.unlink, target)` and `changed(Path.symlink_to, link, source)` pass each method unbound with its arguments:
- Windows refuses to rename an open file, `close` precedes `replace`, and context exit finds the moved name gone
- Temporary files hold mode `0o600`, and a target other users read takes `Path(staged.name).chmod(<mode>)` before `replace`

## [04]-[PLANS]

Changes to an external target are a plan a pure function derives from wanted and live state, and the boundary applies each change once. `_links` reads a folder's live links, `edits(live, wanted)` keys each link to change by its `(live, wanted)` sources, and `relinked` applies one link's change:

```python
def _links(folder: Path, /) -> frozendict[Path, Path]:
    return frozendict({entry: entry.readlink() for entry in folder.iterdir() if entry.info.is_symlink()})


def reconciled(wanted: Mapping[Path, Path], live: Mapping[Path, Path], /) -> Outcome[tuple[Path, ...]]:
    def relinked(link: Path, sides: tuple[Path | None, Path | None], /) -> Outcome[Path]:
        match sides:
            case (_, None):
                return changed(Path.unlink, link)
            case (None, source):
                return changed(Path.symlink_to, link, source)
            case (_, source):
                return changed(Path.unlink, link).bind(lambda _: changed(Path.symlink_to, link, source))

    return accumulated(starmap(relinked, edits(live, wanted).items()))
```

`read(_links, folder).bind(partial(reconciled, wanted))` reads live state once and applies its plan, `read(_links, folder).map(lambda live: edits(live, wanted))` is its dry run, and a missing folder fails before any change. A failed unlink skips its relink, and `entry.info.is_symlink()` reads the file type `iterdir` took from its directory scan, where `entry.is_symlink()` runs one `lstat` per entry.

## [05]-[ENTRY]

Commands compose the flow and return one outcome. `run` writes every decoded quota and combines the write with each line's outcome through `paired`:

```python
def run(source: Path, target: Path, /, *, limit: PositiveInt = 4) -> Outcome[Path]:
    return content(source).map(partial(quotas, limit)).bind(partial(_kept, target))


def _kept(target: Path, lines: tuple[Outcome[Quota[int]], ...], /) -> Outcome[Path]:
    granted = tuple(*line.to_option().to_list() for line in lines)
    return paired(accumulated(lines), changed(rewritten, target, msgspec.json.encode(granted))).map(lambda _: target)
```

`PositiveInt` from `cyclopts.types` refuses `--limit 0` before any read, an unreadable source fails before any write, and a refused line keeps no decoded quota from the target.
