"""Adobe products as the host drives them, their script calls through AppleScript, and the quit, launch, write, and reopen sequence converging each one."""

from collections.abc import Callable, Mapping, Sequence
from pathlib import Path

import anyio
import msgspec

from interface.adobe.rows import Converge, Release, Row, SCRIPT
from interface.adobe.stores import Default, File, Folder, UXP, UxpPlugin, written
from interface.host import (
    Applied,
    Bundle,
    Change,
    DEADLINE,
    Error,
    Failed,
    Header,
    Host,
    launch,
    LAUNCH_ENVIRONMENT,
    literal,
    located,
    Measurement,
    outcome,
    parse,
    quitted,
    registered,
    rendered,
    reopened,
    running,
    Skip,
)
from interface.units import Units

# --- [MODELS] ---------------------------------------------------------------------------


class Scripted(msgspec.Struct, frozen=True):
    """Product reached through its script: name, bundle ids Beta first, the raw Apple event of its scripting command with the parameter clause before its argument, rows of the unit system and bundle, folders, and workspace files."""

    name: str
    identifiers: tuple[str, ...]
    command: str
    arguments: str
    rows: Callable[[Units, Bundle], tuple[Row | File | Default | UxpPlugin, ...]]
    folders: Callable[[Bundle, Mapping[Folder, Path]], Mapping[Folder, Path]]
    workspace: Callable[[Sequence[bytes]], tuple[File, ...]]


class Unscripted(msgspec.Struct, frozen=True):
    """Product reached through its preference domains alone: name, bundle ids, and rows."""

    name: str
    identifiers: tuple[str, ...]
    rows: Callable[[Units], tuple[Default, ...]]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [TRANSPORT]
async def executed(bundle: Bundle, product: Scripted, argument: Release | Converge) -> tuple[Header | Change | Skip | Measurement | Error, ...]:
    """Report rows of the product's script run on the argument by the scripting command sent to the bundle's one finished-launching instance, or one error row of the command's error output."""
    code = rendered(t"$.evalFile(new File({str(SCRIPT / f'{product.name}.jsx')}))")
    source = rendered(t"with timeout of {DEADLINE!s:.0f} seconds\ntell application {str(bundle.path)}\n{product.command!s} {code} {product.arguments!s} {{{literal(argument)}}}\nend tell\nend timeout")
    ran = await anyio.run_process(["/usr/bin/osascript"], input=source.encode(), check=False, env=LAUNCH_ENVIRONMENT)
    return parse(ran.stdout.decode()) if ran.returncode == 0 else (Error(f"{product.command} to {bundle.path} failed with {ran.stderr.decode().strip()}"),)


# --- [LIFECYCLE]
async def launched(host: Host, product: Scripted, bundle: Bundle) -> Applied | Failed:
    """Outcome of the artifact files written, the rows converged in one launch, and the other files and the workspace written once that launch quit."""
    home, declared = Path(host.environ["HOME"]), product.rows(host.units, bundle)
    artifacts = [row for row in declared if isinstance(row, File) and row.folder is Folder.ARTIFACTS]
    await anyio.Path(host.artifacts).mkdir(parents=True, exist_ok=True)
    staged = await written(frozendict({Folder.ARTIFACTS: host.artifacts}), artifacts)
    if any(isinstance(row, Error) for row in staged):
        return outcome(product.name, staged)
    instance = await launch(bundle)
    replied = await executed(bundle, product, Converge(tuple(row for row in declared if isinstance(row, Row)), str(host.artifacts)))
    reported = (*staged, *replied, *map(Error, await quitted((instance,))))
    match outcome(product.name, reported):
        case Failed() as failed:
            return failed
        case Applied(folder=folder):
            base = frozendict({Folder.HOME: home, Folder.SETTINGS: folder, Folder.ARTIFACTS: host.artifacts, Folder.UXP: home / UXP})
            folders = frozendict({**base, **product.folders(bundle, base)})
            factory = await anyio.gather(*(path.read_bytes() for path in sorted([path async for path in anyio.Path(folders[Folder.FACTORY]).iterdir()])))
            stored = (*(row for row in declared if not isinstance(row, Row) and row not in artifacts), *product.workspace(factory))
            return outcome(product.name, (*reported, *await written(folders, stored)))


async def scripted(host: Host, product: Scripted, bundle: Bundle) -> Applied | Failed:
    """Outcome of the write launch once the running instance finished launching, released its documents, and quit, reopened on its titled documents, or the failure of a titled document holding unsaved edits or of more than one running instance."""
    discovered = await registered(running(bundle))
    if len(discovered) > 1:
        return Failed(product.name, (f"{len(discovered)} instances of {bundle.path} run and the release script reaches one, quit all but one",))
    rows = await executed(bundle, product, Release()) if discovered else ()
    titled = tuple(path for row in rows if isinstance(row, Measurement) for path in msgspec.json.decode(row.record, type=tuple[str, ...]))
    if errors := tuple(row.text for row in rows if isinstance(row, Error)) or await quitted(discovered):
        return Failed(product.name, errors)
    async with reopened(bundle, discovered, *titled):
        return await launched(host, product, bundle)


async def unscripted(host: Host, product: Unscripted, bundle: Bundle) -> Applied | Failed:
    """Outcome of the preference domains written while every instance is quit, the instance reopened when one ran."""
    discovered = await registered(running(bundle))
    if errors := await quitted(discovered):
        return Failed(product.name, errors)
    async with reopened(bundle, discovered):
        rows = await written(frozendict(), product.rows(host.units))
    return outcome(product.name, (Header(bundle.version, str(Path(host.environ["HOME"], "Library", "Preferences"))), *rows))


async def converged(host: Host, product: Scripted | Unscripted) -> Applied | Failed | None:
    """Outcome of the product on the newest bundle of its first installed bundle id, none when no bundle of it is installed."""
    installed = next(filter(None, await anyio.gather(*(located(identifier) for identifier in product.identifiers))), ())
    match product, max(installed, key=lambda each: tuple(map(int, each.version.split("."))), default=None):
        case _, None:
            return None
        case Scripted(), bundle:
            return await scripted(host, product, bundle)
        case Unscripted(), bundle:
            return await unscripted(host, product, bundle)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Scripted", "Unscripted", "converged"]
