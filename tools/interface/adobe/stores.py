"""Files, preference domains, and UXP plug-ins an Adobe product reads outside its process, each written as the user where it differs."""

from collections.abc import Callable, Mapping, Sequence
from enum import auto, Enum
from functools import reduce
from pathlib import Path, PurePosixPath
import plistlib
import shutil
from typing import Final

import anyio
import msgspec

from interface.host import Change, Error, Skip
from interface.report import ABSENT, digest, subscript

# --- [TYPES] ----------------------------------------------------------------------------

type Reported = Change | Skip | Error


class Folder(Enum):
    """Folder a product's rows resolve against."""

    HOME = auto()
    SETTINGS = auto()
    PREFERENCES = auto()
    FACTORY = auto()
    ARTIFACTS = auto()
    SCRIPTS = auto()
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
    path: tuple[str, *tuple[str | tuple[int, ...], ...]]
    node: Mapping[str, object] | list[object] | tuple[object, ...] | str | float | bytes


class Registration(msgspec.Struct, frozen=True, rename="camel"):
    """UXP registry row of one plug-in."""

    host_min_version: str
    name: str
    path: str
    plugin_id: str
    status: str
    type: str
    version_string: str


class Requirement(msgspec.Struct, frozen=True, rename="camel"):
    """UXP manifest host requirement: the host application code and the version it needs."""

    app: str
    min_version: str


class Manifest(msgspec.Struct, frozen=True):
    """UXP manifest keys the registry row takes."""

    id: str
    name: str
    version: str
    host: Requirement


class UxpPlugin(msgspec.Struct, frozen=True):
    """UXP plug-in by its folder's files by relative path, its manifest among them."""

    files: Mapping[str, bytes]

    @property
    def manifest(self) -> Manifest:
        """Manifest the plug-in's `manifest.json` declares."""
        return msgspec.json.decode(self.files["manifest.json"], type=Manifest)

    @property
    def plugin(self) -> Plugin:
        """Registry plug-in the manifest's host code and id name."""
        return Plugin(self.manifest.host.app, self.manifest.id)


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [FILES]
async def held(path: anyio.Path) -> bytes | None:
    """File's bytes, None while it is absent."""
    try:
        return await path.read_bytes()
    except FileNotFoundError:
        return None


async def file_written(path: anyio.Path, row: File, enabled: frozenset[Plugin]) -> tuple[Reported, ...]:
    """Change of the rendered file written where it differs, the render's skips or error, the skip of a gating plug-in the registry leaves disabled, or the error of a refused write."""
    if row.plugin is not None and row.plugin not in enabled:
        return (Skip(row.plugin.id),)
    before = await held(path)
    match row.render(before):
        case bytes() as body if body == before:
            return ()
        case bytes() as body:
            try:
                await path.write_bytes(body)
            except PermissionError:
                refusing = path.parent if before is None else path
                return (Error(f"{path} is unwritten, {refusing} belongs to {await refusing.owner()}"),)
            except FileNotFoundError:
                return (Error(f"{path} is unwritten, {path.parent} is absent"),)
            return (Change(row.path, ABSENT if before is None else digest(before), digest(body)),)
        case Error() as error:
            return (error,)
        case skips:
            return skips


# --- [DOMAINS]
def child(value: object, step: str | tuple[int, ...]) -> object:
    """Node the value holds under a dictionary key or after an array's leading items, None where it holds none."""
    match step, value:
        case str(), dict():
            return value.get(step)
        case tuple(), list() if len(value) > (size := len(step)) and value[:size] == [*step]:
            return value[size]
        case _:
            return None


def placed(value: object, path: Sequence[str | tuple[int, ...]], node: object) -> object:
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

    def spelled(tree: object, path: Sequence[str | tuple[int, ...]]) -> str:
        return ABSENT if (node := reduce(child, path, tree)) is None else repr(node)

    changes = tuple(
        Change(subscript(domain, *(step if isinstance(step, str) else len(step) for step in row.path)), before, after)
        for row in rows
        if (before := spelled(exported, row.path)) != (after := spelled(imported, row.path))
    )
    if changes:
        await anyio.run_process(["/usr/bin/defaults", "import", domain, "-"], input=merged)
    return changes


# --- [PLUGINS]
async def registered(folders: Mapping[Folder, Path], host: str) -> tuple[anyio.Path, tuple[Registration, ...]]:
    """Registry file of the UXP host and the rows it lists, none while it is absent."""
    registry = anyio.Path(folders[Folder.UXP], "PluginsInfo", "v1", f"{host}.json")
    data = await held(registry)
    return registry, () if data is None else msgspec.json.decode(data, type=dict[str, tuple[Registration, ...]])["plugins"]


async def installed(folders: Mapping[Folder, Path], registry: anyio.Path, listed: Sequence[Registration], rows: Sequence[UxpPlugin]) -> tuple[Change, ...]:
    """Changes of the plug-ins whose registry row or files differ from their own, each registered folder replaced and the registry written once with their rows enabled."""
    local = PurePosixPath("$localPlugins")
    entries = {entry.plugin_id: entry for entry in listed}
    targets = {
        manifest.id: Registration(manifest.host.min_version, manifest.name, str(local / "External" / f"{manifest.id}_{manifest.version}"), manifest.id, "enabled", "uxp", manifest.version)
        for row in rows
        for manifest in (row.manifest,)
    }

    def folder(entry: Registration) -> anyio.Path:
        return anyio.Path(folders[Folder.UXP], "Plugins", PurePosixPath(entry.path).relative_to(local))

    def stamp(entry: Registration, files: Mapping[str, bytes]) -> str:
        return f"{entry.version_string} {digest(msgspec.json.encode((entry, dict(files)), order='sorted'))}"

    async def replaced(row: UxpPlugin, target: Registration) -> Change | None:
        match entries.get(target.plugin_id):
            case None:
                before = ABSENT
            case entry:
                source = folder(entry)
                present = {str((root / name).relative_to(source)): await (root / name).read_bytes() async for root, _, names in source.walk() for name in names}
                if (entry, present) == (target, row.files):
                    return None
                before = stamp(entry, present)
                await anyio.to_thread.run_sync(shutil.rmtree, source)
        destination = folder(target)
        for name, body in row.files.items():
            await (destination / name).parent.mkdir(parents=True, exist_ok=True)
            await (destination / name).write_bytes(body)
        return Change(subscript(registry.name, target.plugin_id), before, stamp(target, row.files))

    if changes := tuple(change for change in await anyio.gather(*map(replaced, rows, targets.values(), strict=True)) if change is not None):
        await registry.write_bytes(msgspec.json.encode({"plugins": (*(entry for entry in listed if entry.plugin_id not in targets), *targets.values())}))
    return changes


# --- [COMPOSITION] ----------------------------------------------------------------------


async def written(folders: Mapping[Folder, Path], rows: Sequence[File | Default | UxpPlugin]) -> tuple[Reported, ...]:
    """Rows of the files, preference domains, and UXP plug-ins the rows declare, each UXP registry a row names read once."""
    defaults = [row for row in rows if isinstance(row, Default)]
    plugins = [row for row in rows if isinstance(row, UxpPlugin)]
    hosts = dict.fromkeys(row.plugin.host for row in rows if isinstance(row, File | UxpPlugin) and row.plugin is not None)
    registries = dict(zip(hosts, await anyio.gather(*(registered(folders, host) for host in hosts)), strict=True))
    enabled = frozenset(Plugin(host, entry.plugin_id) for host, (_, listed) in registries.items() for entry in listed if entry.status == "enabled")
    reported = await anyio.gather(
        *(file_written(anyio.Path(folders[row.folder], row.path), row, enabled) for row in rows if isinstance(row, File)),
        *(domain_written(domain, [row for row in defaults if row.domain == domain]) for domain in dict.fromkeys(row.domain for row in defaults)),
        *(installed(folders, registry, listed, [row for row in plugins if row.plugin.host == host]) for host, (registry, listed) in registries.items()),
    )
    return tuple(line for lines in reported for line in lines)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["UXP", "Default", "File", "Folder", "Plugin", "UxpPlugin", "written"]
