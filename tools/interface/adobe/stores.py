"""Files and preference domains an Adobe product reads outside its process, each written as the user where it differs."""

from collections.abc import Callable, Mapping, Sequence
from enum import auto, Enum
from functools import reduce
from pathlib import Path
import plistlib
from typing import Final

import anyio
import msgspec

from interface.host import Change, Error, Skip
from interface.report import ABSENT, digest, subscript

# --- [TYPES] ----------------------------------------------------------------------------

type Reported = Change | Skip | Error
type Step = str | tuple[int, ...]


class Folder(Enum):
    """Folder a product's rows resolve against."""

    HOME = auto()
    SETTINGS = auto()
    PREFERENCES = auto()
    FACTORY = auto()
    PLUGIN_DATA = auto()
    UXP = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

UXP: Final = Path("Library", "Application Support", "Adobe", "UXP")

# --- [MODELS] ---------------------------------------------------------------------------


class Plugin(msgspec.Struct, frozen=True):
    """UXP plug-in by the host code naming the registry file that lists it, and its id."""

    host: str
    id: str


class File(msgspec.Struct, frozen=True):
    """File by folder and relative path, the render from its held bytes to its bytes, skips, or an error, and the plug-in whose enabled registry row gates it."""

    folder: Folder
    path: str
    render: Callable[[bytes | None], bytes | tuple[Skip, ...] | Error]
    plugin: Plugin | None = None


class Default(msgspec.Struct, frozen=True):
    """Preference domain node by domain and path, each step past the top-level key a dictionary key or the leading items of an array whose next item holds the rest."""

    domain: str
    path: tuple[str, *tuple[Step, ...]]
    node: Mapping[str, object] | list[object] | tuple[object, ...] | str | float | bytes


class Registration(msgspec.Struct, frozen=True, rename="camel"):
    """UXP registry row of one plug-in by the keys its gate reads."""

    plugin_id: str
    status: str


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [FILES]
async def held(path: anyio.Path) -> bytes | None:
    """File's bytes, None while it is absent."""
    try:
        return await path.read_bytes()
    except FileNotFoundError:
        return None


async def file_written(path: anyio.Path, row: File, enabled: frozenset[Plugin]) -> tuple[Reported, ...]:
    """Change of the rendered file written where it differs with its folders made, the render's skips or error, the skip of a gating plug-in the registry leaves disabled, or the error of a refused write."""
    if row.plugin is not None and row.plugin not in enabled:
        return (Skip(row.plugin.id),)
    before = await held(path)
    match row.render(before):
        case bytes() as body if body == before:
            return ()
        case bytes() as body:
            try:
                await path.parent.mkdir(parents=True, exist_ok=True)
                await path.write_bytes(body)
            except PermissionError:
                refusing = path.parent if before is None else path
                return (Error(f"{path} is unwritten, {refusing} belongs to {await refusing.owner()}"),)
            return (Change(row.path, ABSENT if before is None else digest(before), digest(body)),)
        case Error() as error:
            return (error,)
        case skips:
            return skips


# --- [DOMAINS]
def child(value: object, step: Step) -> object:
    """Node the value holds under a dictionary key or after an array's leading items, None where it holds none."""
    match step, value:
        case str(), dict():
            return value.get(step)
        case tuple(), list() if len(value) > (size := len(step)) and value[:size] == [*step]:
            return value[size]
        case _:
            return None


def placed(value: object, path: Sequence[Step], node: object) -> object:
    """Value with the node at the path, each dictionary or array the value lacks made new."""
    match path:
        case [str() as key, *rest]:
            return {**(value if isinstance(value, dict) else {}), key: placed(child(value, key), rest, node)}
        case [tuple() as head, *rest]:
            return [*head, placed(child(value, head), rest, node)]
        case _:
            return node


def folded(tree: dict[str, object], row: Default) -> dict[str, object]:
    """Domain tree with the row's node at its path."""
    key, *rest = row.path
    return {**tree, key: placed(tree.get(key), rest, row.node)}


async def domain_written(domain: str, rows: Sequence[Default]) -> tuple[Change, ...]:
    """Changes of the domain's nodes the rows move, the domain exported once and imported whole when a node moved."""
    exported = plistlib.loads((await anyio.run_process(["/usr/bin/defaults", "export", domain, "-"])).stdout)
    merged = plistlib.dumps(reduce(folded, rows, exported), fmt=plistlib.FMT_BINARY)
    imported = plistlib.loads(merged)

    def spelled(tree: object, path: Sequence[Step]) -> str:
        return ABSENT if (node := reduce(child, path, tree)) is None else repr(node)

    if changes := tuple(
        Change(subscript(domain, *(step if isinstance(step, str) else len(step) for step in row.path)), before, after)
        for row in rows
        if (before := spelled(exported, row.path)) != (after := spelled(imported, row.path))
    ):
        await anyio.run_process(["/usr/bin/defaults", "import", domain, "-"], input=merged)
    return changes


# --- [PLUGINS]
async def registered(folders: Mapping[Folder, Path], host: str) -> tuple[Registration, ...]:
    """Rows the registry file of the UXP host lists, none while it is absent."""
    data = await held(anyio.Path(folders[Folder.UXP], "PluginsInfo", "v1", f"{host}.json"))
    return () if data is None else msgspec.json.decode(data, type=dict[str, tuple[Registration, ...]])["plugins"]


# --- [COMPOSITION] ----------------------------------------------------------------------


async def written(folders: Mapping[Folder, Path], rows: Sequence[File | Default]) -> tuple[Reported, ...]:
    """Rows of the files and preference domains the rows declare, each UXP registry a gated file names read once."""
    defaults = [row for row in rows if isinstance(row, Default)]
    hosts = tuple(dict.fromkeys(row.plugin.host for row in rows if isinstance(row, File) and row.plugin is not None))
    registries = await anyio.gather(*(registered(folders, host) for host in hosts))
    enabled = frozenset(Plugin(host, entry.plugin_id) for host, listed in zip(hosts, registries, strict=True) for entry in listed if entry.status == "enabled")
    reported = await anyio.gather(
        *(file_written(anyio.Path(folders[row.folder], row.path), row, enabled) for row in rows if isinstance(row, File)),
        *(domain_written(domain, [row for row in defaults if row.domain == domain]) for domain in dict.fromkeys(row.domain for row in defaults)),
    )
    return tuple(line for lines in reported for line in lines)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["UXP", "Default", "File", "Folder", "Plugin", "written"]
