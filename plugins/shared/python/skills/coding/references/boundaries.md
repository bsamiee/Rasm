# [BOUNDARIES]

## [01]-[STREAMS]

Streams decode one record at a time where a malformed record leaves siblings valid, and a bound the run states is a decoder type argument:

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

Text another program parses builds from a t-string, one renderer escapes each interpolation for the target and passes literals unchanged:

```python
def rendered(escape: Callable[[str], str], template: Template, /) -> str:
    return "".join(escape(format(convert(part.value, part.conversion), part.format_spec)) if isinstance(part, Interpolation) else part for part in template)


def argv(template: Template, /) -> tuple[str, ...]:
    return tuple(shlex.split(rendered(shlex.quote, template)))
```

`argv(t"grep -n {pattern} {path}")` keeps each value one argument whatever it holds, and `rendered(html.escape, ...)` serves markup. Queries render `rendered(lambda _: "?", template)` with `template.values` as parameters:
- Iterating a `Template` yields literal strings and `Interpolation` parts in source order, empty strings left out
- `template.values` holds each interpolation's value in order, and the placeholder follows the database driver's `paramstyle`

## [03]-[FILE_WRITES]

Path changes are one function over the change and its arguments that returns the path or its `UNWRITABLE` fault. File replacements write a uniquely named temporary file beside the target and commit through `replace`, readers see the old file or the new one:

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

`changed(rewritten, target, data)` replaces a file. `changed(Path.unlink, target)` and `changed(Path.symlink_to, link, source)` pass methods unbound with their arguments:
- Windows refuses to rename an open file, `close` precedes `replace`, and context exit finds the moved name gone
- Temporary files hold mode `0o600`, and a target other users read takes `Path(staged.name).chmod(<mode>)` before `replace`

## [04]-[PLANS]

Changes to an external target derive from wanted and live state, and the boundary applies each change once:

```python
def _links(folder: Path, /) -> frozendict[Path, Path]:
    return frozendict({entry: entry.readlink() for entry in folder.iterdir() if entry.info.is_symlink()})


def reconciled(wanted: Mapping[Path, Path], live: Mapping[Path, Path], /) -> Outcome[tuple[Path, ...]]:
    def relinked(link: Path, sides: tuple[Option[Path], Option[Path]], /) -> Outcome[Path]:
        before, after = sides
        removed: Outcome[Path] = changed(Path.unlink, link) if before.is_some() else Ok(link)
        return after.map(lambda source: removed.bind(lambda _: changed(Path.symlink_to, link, source))).default_value(removed)

    return accumulated(starmap(relinked, edits(live, wanted).items()))
```

`read(_links, folder).bind(partial(reconciled, wanted))` reads live state once and applies its plan. `read(_links, folder).map(lambda live: edits(live, wanted))` returns a dry run. Failed reads stop all changes, failed unlinks stop their relink, and `entry.info.is_symlink()` reuses directory-scan metadata.

## [05]-[ENTRY_POINTS]

Commands compose the flow and return one outcome. `run` writes every decoded quota and combines the write with each line's outcome through `paired`:

```python
def run(source: Path, target: Path, /, *, limit: PositiveInt) -> Outcome[Path]:
    return content(source).map(partial(quotas, limit)).bind(partial(_kept, target))


def _kept(target: Path, lines: tuple[Outcome[Quota[int]], ...], /) -> Outcome[Path]:
    granted = tuple(*line.to_option().to_list() for line in lines)
    return paired(accumulated(lines), changed(rewritten, target, msgspec.json.encode(granted))).map(lambda _: target)
```

`PositiveInt` from `cyclopts.types` refuses `--limit 0` before any read, and an unreadable source fails before any write. Refused lines keep no decoded quota from the target.
