"""Stores the Adobe products read at launch, written with each product quit."""

from collections.abc import Awaitable, Callable, Mapping
import ctypes
from functools import reduce
import io
from itertools import accumulate, pairwise, starmap
from pathlib import Path, PurePosixPath
import plistlib
import re
import struct
from typing import Final
import uuid

import anyio
import msgspec
from psd_tools.constants import OSType
from psd_tools.psd.base import ValueElement
from psd_tools.psd.descriptor import Descriptor, List, String, TYPES

from interface import host
from interface.adobe import workspaces
from interface.adobe.aliases import LABEL, Prompt
from interface.adobe.rows import (
    ActionSet,
    Catalog,
    DocumentPresets,
    Fixed,
    Leaf,
    Library,
    Nested,
    Overlay,
    Paper,
    Plugin,
    Product,
    Profiles,
    Record,
    Rendered,
    Row,
    Serialized,
    Shared,
    Toolbar,
    Unit,
    Workspace,
)
from interface.adobe.workspaces import Frame, hex_string
from interface.render import DPI
from interface.report import ABSENT, digest, Kind, line
from interface.units import Length

# --- [TYPES] ----------------------------------------------------------------------------

type Node = int | float | str | bytes | tuple[tuple[str, Node], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

HIVE: Final = "DC"
ATOM: Final = 2
CABINET: Final = 8
TEXT: Final = frozenset({ATOM, 4})
HEADER: Final = b"8BPF" + (1).to_bytes(2) + (16).to_bytes(4)
PAGE: Final = 0x1000
STORE_TEXT: Final = "latin-1"

# --- [MODELS] ---------------------------------------------------------------------------


class Identity(msgspec.Struct, frozen=True, rename={"identity": "id", "source": "presetSource"}):
    """Keys naming one Illustrator New Document preset by its id, title, and source category."""

    identity: str
    title: str
    source: str


class Registration(msgspec.Struct, frozen=True, rename="camel"):
    """UXP plug-in registry row in the key order the registry holds."""

    host_min_version: str
    name: str
    path: str
    plugin_id: str
    status: str
    type: str
    version_string: str


class Folders(msgspec.Struct, frozen=True):
    """Product's stores derived once from its bundle, with the Adobe UXP folder every product's plug-ins, plug-in data, and registries share."""

    bundle: host.Bundle
    uxp: anyio.Path


class Settings(Folders, frozen=True):
    """Product's stores with its settings folder under the home library's preferences and the presets folder it reads scripts from."""

    preferences: anyio.Path
    presets: anyio.Path


class Support(Settings, frozen=True):
    """Illustrator's stores, with its second settings folder under Adobe's application support."""

    support: anyio.Path


class Plan(msgspec.Struct, frozen=True):
    """Rendered file with its report label, its path, the bytes it holds or None while absent, and the bytes its row renders or None to remove it."""

    label: str
    path: anyio.Path
    held: bytes | None
    body: bytes | None


class Size(msgspec.Struct, frozen=True, kw_only=True, rename="camel"):
    """Photoshop New Document user preset in the key order Photoshop writes, each key a paper leaves unset at the factory preset's value."""

    name: str
    identifier: str = ""
    group: str
    width: float
    height: float
    units: str
    profile: str = "default"
    resolution: float
    resolution_units: str
    depth: int = 8
    scale: float = 1.0
    mode: str = "RGB"
    fill: str = "white"
    guides: tuple[()] = ()
    artboards: tuple[()] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [DOMAIN]
async def exported(domain: str) -> dict[str, object]:
    """Preference domain read whole through cfprefsd."""
    return msgspec.convert(plistlib.loads((await anyio.run_process(["/usr/bin/defaults", "export", domain, "-"])).stdout), dict[str, object])


def children(node: object) -> Mapping[str, object]:
    """Keys a hive section or a nested section holds, none for any other node."""
    match node:
        case dict():
            return node
        case [int(), dict() as inner]:
            return inner
        case _:
            return {}


def placed(node: Mapping[str, object], path: tuple[str, ...], leaf: object, *, top: bool) -> dict[str, object]:
    """Node with the leaf at the path, every absent section made as a cabinet, top-level sections as plain dictionaries."""
    head, *rest = path
    if not rest:
        return {**node, head: leaf}
    inner = placed(children(node.get(head)), tuple(rest), leaf, top=False)
    return {**node, head: inner if top else [CABINET, inner]}


def current(domain: Mapping[str, object], access: Leaf | Shared) -> object:
    """Node the domain holds at the accessor, None where it holds none of the accessor's kind: a leaf's typecode and value under the hive, a cabinet for a cabinet leaf, a shared key's value at the top."""
    match access:
        case Leaf(path=path, code=code):
            *sections, key = path
            match children(reduce(lambda node, name: children(node).get(name), sections, domain.get(HIVE))).get(key):
                case [int(), value] as held if isinstance(value, dict) == (code == CABINET):
                    return held
                case _:
                    return None
        case Shared(key=key):
            return domain.get(key)


def put(domain: Mapping[str, object], access: Leaf | Shared, node: object) -> dict[str, object]:
    """Domain holding the stored node at the accessor: a leaf's typecode and value under the hive, a shared key at the top."""
    match access:
        case Leaf(path=path):
            return {**domain, HIVE: placed(children(domain.get(HIVE)), path, node, top=True)}
        case Shared(key=key):
            return {**domain, key: node}


def stored(access: Leaf | Shared, target: object) -> object:
    """Target as the domain stores it: a leaf as its typecode and value, text as NUL-terminated UTF-8, a list of atoms as a cabinet of atom leaves keyed by position, a 16.16 fixed number as the integer or real its typecode holds, and a binary color as Acrobat's RGB struct of fixed channels."""

    def fixed(number: float) -> int:
        return round(number * 0x10000)

    match access, target:
        case Leaf(code=code), _ if code in TEXT:
            return [code, f"{target}\0".encode()]
        case Leaf(path=path, code=code), tuple() as atoms if code == CABINET:
            return [code, {(slot := str(index)): stored(Leaf((*path, slot), ATOM), atom) for index, atom in enumerate(atoms)}]
        case Leaf(code=1), Fixed(value=number):
            return [1, fixed(number)]
        case Leaf(code=3), Fixed(value=number):
            return [3, fixed(number) / fixed(1)]
        case Leaf(code=6), (int() as red, int() as green, int() as blue):
            return [6, struct.pack("<B3x4i", 1, *(fixed(Fixed.channel(byte).value) for byte in (red, green, blue)), 0)]
        case Leaf(code=code), _:
            return [code, target]
        case _:
            return target


def spelled(access: Leaf | Shared, value: object) -> str:
    """Report cell of a stored node, a leaf as its typecode and value, text decoded, a cabinet as its leaves in key order, and binary in hex."""
    match access, value:
        case Leaf(), [int() as code, bytes() as data] if code in TEXT:
            return f"{code} {data.rstrip(b'\0').decode()}"
        case Leaf(path=path), [int() as code, dict() as cabinet]:
            return f"{code} {' '.join(spelled(Leaf((*path, index), ATOM), cabinet[index]) for index in sorted(cabinet, key=int))}"
        case Leaf(), [int() as code, bytes() as data]:
            return f"{code} {data.hex()}"
        case Leaf(), [int() as code, held]:
            return f"{code} {held}"
        case _, bytes() as data:
            return data.hex()
        case _:
            return str(value)


def domains(identifier: str, declared: tuple[Row, ...]) -> list[tuple[str, Row, Leaf | Shared]]:
    """Each domain row with the preference domain holding it: the product's own, its bundle identifier, for a leaf, and the named domain for a shared key."""
    return [(access.domain if isinstance(access, Shared) else identifier, row, access) for row in declared if isinstance(access := row.path, Leaf | Shared)]


async def domain_written(domain: str, declared: tuple[tuple[Row, Leaf | Shared], ...]) -> tuple[str, ...]:
    """Report lines of one preference domain's declared values, the domain written on difference through cfprefsd with every product quit."""
    held = await exported(domain)
    if changed := [(row, access, before, after) for row, access in declared if (before := current(held, access)) != (after := stored(access, row.target))]:
        target = reduce(lambda node, change: put(node, *change), ((access, after) for _, access, _, after in changed), held)
        await anyio.run_process(["/usr/bin/defaults", "import", domain, "-"], input=plistlib.dumps(target, fmt=plistlib.FMT_BINARY))
    return tuple(line(Kind.CHANGE, row.label, spelled(access, before), spelled(access, after)) for row, access, before, after in changed)


# --- [FOLDERS]
def channel(bundle: host.Bundle) -> tuple[str, ...]:
    """Release channel words the bundle name carries in parentheses, none for a release build."""
    return tuple(re.findall(r"\(([^)]*)\)", bundle.name))


async def located(product: Product, bundle: Path) -> Folders:
    """Product's stores from its bundle and Adobe's UXP folder: Illustrator's settings folders and installed `Presets.localized/<locale>`, Photoshop's settings folder and application support `Presets`, else the bundle's keys alone."""
    info, library = await host.bundle(bundle), (await anyio.Path.home()).joinpath("Library")
    adobe = library.joinpath("Application Support", "Adobe")
    match product:
        case Product.ILLUSTRATOR:
            locale = msgspec.convert((await exported("NSGlobalDomain"))["AppleLanguages"], tuple[str, ...])[0].replace("-", "_")
            settings = PurePosixPath(" ".join((bundle.stem, info.version, *channel(info), "Settings")), locale)
            return Support(info, adobe / "UXP", library.joinpath("Preferences", settings), anyio.Path(bundle.parent, "Presets.localized", locale), adobe / settings)
        case Product.PHOTOSHOP:
            return Settings(info, adobe / "UXP", library.joinpath("Preferences", f"{bundle.stem} Settings"), adobe.joinpath(bundle.stem, "Presets"))
        case _:
            return Folders(info, adobe / "UXP")


async def storage(folders: Folders, access: Overlay) -> tuple[anyio.Path, bool]:
    """UXP storage folder of the overlay's plug-in, `<product><CHANNEL>/<major>/External/<id>/PluginData`, and whether the product's registry lists the plug-in enabled."""
    listed = folders.uxp.joinpath("PluginsInfo", "v1", access.registry)
    folder = folders.uxp.joinpath(
        "PluginsStorage", f"{access.product}{''.join(map(str.upper, channel(folders.bundle)))}", folders.bundle.version.partition(".")[0], "External", access.plugin, "PluginData"
    )
    registry = msgspec.json.decode(await listed.read_bytes(), type=dict[str, tuple[Registration, ...]])["plugins"] if await listed.exists() else ()
    return folder, any(entry.plugin_id == access.plugin and entry.status == "enabled" for entry in registry)


# --- [SETTINGS]
async def read_file(path: anyio.Path) -> Descriptor:
    """Action descriptor a settings file holds after its header."""
    return Descriptor.read(io.BytesIO((await path.read_bytes()).removeprefix(HEADER)))


def shown(held: ValueElement | None) -> str:
    """Report cell of a stored item's value."""
    return ABSENT if held is None else str(held.value)


async def file_written(path: anyio.Path, declared: tuple[tuple[Row, Serialized], ...]) -> tuple[str, ...]:
    """Report lines of one settings file's declared items, the file rewritten on difference with its header and every other item unchanged."""
    held = await read_file(path)
    if changed := [(row, key, before, target) for row, access in declared if (before := held.get(key := access.key.encode())) != (target := TYPES[OSType(access.code.encode())](row.target))]:
        held.update({key: target for _, key, _, target in changed})
        buffer = io.BytesIO()
        held.write(buffer)
        await path.write_bytes(HEADER + buffer.getvalue())
    return tuple(line(Kind.CHANGE, row.label, shown(before), shown(target)) for row, _, before, target in changed)


# --- [GROUPS]
async def grouped[Store, Accessor](entries: list[tuple[Store, Row, Accessor]], step: Callable[[Store, tuple[tuple[Row, Accessor], ...]], Awaitable[tuple[str, ...]]]) -> tuple[str, ...]:
    """Lines of the step over each store the entries name, in the order they first name it, with the rows it holds and their accessors."""
    return tuple([reported for store in dict.fromkeys(store for store, _, _ in entries) for reported in await step(store, tuple((row, access) for named, row, access in entries if named == store))])


# --- [DATABASE]
def checksum(page: bytes | bytearray) -> int:
    """InDesign's page checksum over the page with its checksum field zeroed: the byte sum and the lane-weighted sum of its eight byte lanes, each modulo 65521."""
    lanes = tuple((page[: PAGE - 4] + bytes(4))[lane::8] for lane in range(8))
    sums = tuple(map(sum, lanes))
    weighted = sum(byte * (len(data) - index) for data in lanes for index, byte in enumerate(data)) + len(lanes[0]) * sum((len(lanes) - 1 - lane) * total for lane, total in enumerate(sums))
    return sum(sums) % 0xFFF1 | weighted % 0xFFF1 << 16


def recorded(held: bytes, fields: tuple[tuple[Row, Record], ...]) -> bytes:
    """Database with each field set in every copy of its record and every page the fields change re-checksummed."""
    body = bytearray(held)
    for row, access in fields:
        for found in re.finditer(re.escape(struct.pack("<II", access.implementation, access.length)), held):
            struct.pack_into(access.form, body, found.end() + access.offset, row.target)
    for start in range(0, len(body), PAGE):
        if body[start : start + PAGE] != held[start : start + PAGE]:
            struct.pack_into("<I", body, start + PAGE - 4, checksum(body[start : start + PAGE]))
    return bytes(body)


async def database(path: anyio.Path, fields: tuple[tuple[Row, Record], ...]) -> Plan | str:
    """Database's plan with every field set, or the error line of the fields whose record it holds no copy of."""
    held = await path.read_bytes()
    match [row.label for row, access in fields if struct.pack("<II", access.implementation, access.length) not in held]:
        case []:
            return Plan(path.name, path, held, recorded(held, fields))
        case absent:
            return line(Kind.ERROR, f"{path} holds no record for {', '.join(absent)}")


# --- [ACTIONS]
def unsigned(data: bytes, at: int) -> int:
    """Big-endian unsigned 32-bit integer at the offset."""
    return int.from_bytes(data[at : at + 4])


def text_end(data: bytes, at: int) -> int:
    """End of the descriptor text at the offset: its UTF-16 unit count, then the units."""
    return at + 4 + 2 * unsigned(data, at)


def key_end(data: bytes, at: int) -> int:
    """End of the descriptor key or class at the offset: a zero length and a character id, or a length and a string id."""
    return at + 4 + (unsigned(data, at) or 4)


def listed_end(data: bytes, at: int, step: Callable[[bytes, int], int]) -> int:
    """End of the count at the offset and that many items after it, each ending where the step reads it to."""
    return reduce(lambda end, _: step(data, end), range(unsigned(data, at)), at + 4)


def reference_end(data: bytes, at: int) -> int:
    """End of the reference form at the offset: its form code, its text, and its class, then the key, enumeration, index, or name the form adds."""
    classed = key_end(data, text_end(data, at + 4))
    match data[at : at + 4]:
        case b"Clss":
            return classed
        case b"prop":
            return key_end(data, classed)
        case b"Enmr":
            return key_end(data, key_end(data, classed))
        case b"rele" | b"Idnt" | b"indx":
            return classed + 4
        case b"name":
            return text_end(data, classed)
        case form:
            raise ValueError(f"{form!r} at offset {at} names no action reference form")


def value_end(data: bytes, at: int) -> int:
    """End of the typed descriptor value at the offset."""
    body = at + 4
    match data[at:body]:
        case b"obj ":
            return listed_end(data, body, reference_end)
        case b"Objc" | b"GlbO":
            return descriptor_end(data, body)
        case b"ObAr":
            return descriptor_end(data, body + 4)
        case b"VlLs":
            return listed_end(data, body, value_end)
        case b"bool":
            return body + 1
        case b"long":
            return body + 4
        case b"doub" | b"comp":
            return body + 8
        case b"UntF":
            return body + 12
        case b"UnFl":
            return body + 8 + 8 * unsigned(data, body + 4)
        case b"TEXT":
            return text_end(data, body)
        case b"enum":
            return key_end(data, key_end(data, body))
        case b"type" | b"GlbC":
            return key_end(data, text_end(data, body))
        case b"alis" | b"Pth " | b"tdta":
            return body + 4 + unsigned(data, body)
        case kind:
            raise ValueError(f"{kind!r} at offset {at} names no action value type")


def descriptor_end(data: bytes, at: int) -> int:
    """End of the descriptor at the offset: its text, its class, and its counted keys, each followed by its value."""
    return listed_end(data, key_end(data, text_end(data, at)), lambda held, end: value_end(held, key_end(held, end)))


def item_end(data: bytes, at: int) -> int:
    """End of the action item at the offset: its four flags, its event as a string id or a character id, its dictionary name, and its descriptor where its flag is -1."""
    event = at + 8
    dictionary = event + 4 + unsigned(data, event) if data[at + 4 : event] == b"TEXT" else event + 4
    flag = dictionary + 4 + unsigned(data, dictionary)
    return descriptor_end(data, flag + 4) if struct.unpack_from(">i", data, flag)[0] == -1 else flag + 4


def action_end(data: bytes, at: int) -> int:
    """End of the action at the offset: its function key, modifiers, and color, its name, its expanded flag, and its counted items."""
    return listed_end(data, text_end(data, at + 6) + 1, item_end)


def set_end(data: bytes, at: int) -> int:
    """End of the action set at the offset: its name, its expanded flag, and its counted actions."""
    return listed_end(data, text_end(data, at) + 1, action_end)


# --- [RENDER]
def textual(entries: tuple[tuple[str, Node], ...], depth: int) -> str:
    """Entries in the store text form Illustrator's preferences, catalogs, and action files share: a directory in braces, a number in digits, a real at ten decimals, a byte string in parentheses, and text in hex, each line at the depth's tabs and ended by CR."""
    indent = "\t" * depth

    def entry(key: str, value: Node) -> str:
        match value:
            case tuple():
                return f"{indent}/{key} {{\r{textual(value, depth + 1)}{indent}}}\r"
            case bytes():
                return f"{indent}/{key} ({value.decode()})\r"
            case str():
                return f"{indent}/{key} {hex_string(value, depth)}\r"
            case float():
                return f"{indent}/{key} {re.sub(r'(?<=\.\d)0+$|(?<=[1-9])0+$', '', f'{value:.10f}')}\r"
            case int():
                return f"{indent}/{key} {value}\r"

    return "".join(starmap(entry, entries))


def store_entries(text: str) -> tuple[tuple[str, Node], ...]:
    """Entries of text in the store form `textual` writes, read to the first line that opens no entry."""
    rows = iter(text.split("\r"))

    def entry(row: str) -> tuple[str, Node] | None:
        match re.fullmatch(r"(\t*)/((?:\\.|[^ \\])+) (.*)", row):
            case None:
                return None
            case found:
                tabs, key, token = found.groups()
                return key, value(tabs, token)

    def value(tabs: str, token: str) -> Node:
        match token[:1]:
            case "{":
                return tuple(iter(lambda: entry(next(rows)), None))
            case "[":
                return bytes.fromhex("".join(row.strip() for row in iter(lambda: next(rows), f"{tabs}]"))).decode()
            case "(":
                return token[1:-1].encode()
            case _:
                return int(token) if re.fullmatch(r"-?\d+", token) else float(token)

    return tuple(iter(lambda: entry(next(rows, "")), None))


def unordered(node: tuple[tuple[str, Node], ...]) -> frozenset[tuple[str, type, object]]:
    """Directory as the unordered tree Illustrator reads it: each entry's key, value type, and value, a directory nested the same way."""
    return frozenset((key, type(value), unordered(value) if isinstance(value, tuple) else value) for key, value in node)


def catalog(access: Catalog, toolbar: Toolbar) -> bytes:
    """Toolbar catalog in the store text form: one collection named by the access, a slot holding one tool by name and a flyout as a counted item set, the options as footer flags, then the catalog keys."""

    def entries(index: int, slot: tuple[str, ...]) -> tuple[tuple[str, Node], ...]:
        group = sum(len(earlier) > 1 for earlier in toolbar.slots[:index])
        match slot:
            case (tool,):
                return ((f"CustomToolboxItem{index}", tool.encode()),)
            case _:
                return (
                    (f"CustomToolboxItem{index}", f"CustomToolboxItemSet{group}_Count_{len(slot)}".encode()),
                    *((f"CustomToolboxItemSet{group}_{member}", tool.encode()) for member, tool in enumerate(slot)),
                )

    attributes = (("collectionName", access.collection), *(item for index, slot in enumerate(toolbar.slots) for item in entries(index, slot)), *toolbar.options.items())
    return textual(
        (
            ("collection1", (("attributes", attributes), ("canEdit", 1), ("canDelete", 1))),
            ("Sketch", (("Version", 0), ("Description", b"Adobe Custom Toolbar"), ("Owner", b""))),
            ("NumberOfCollections", 1),
            ("CatalogName", b"Adobe Custom Toolbar"),
        ),
        0,
    ).encode()


def profiles(access: Profiles, presets: DocumentPresets, held: bytes, folder: str) -> bytes:
    """Preset list with every row outside the papers kept as held and one row per paper after them copied from the source row: size in 32-bit points, the paper's unit, the raster effects resolution, and the source's profile file in the profile folder."""
    rows = msgspec.json.decode(held, type=tuple[msgspec.Raw, ...])
    named = tuple(zip(rows, (msgspec.json.decode(row, type=Identity) for row in rows), strict=True))
    kept = tuple((row, identity) for row, identity in named if identity.title not in {paper.name for paper in presets.papers})
    source, origin = next((row, identity) for row, identity in named if identity.identity == access.source)
    fields = msgspec.json.decode(source, type=dict[str, msgspec.Raw])
    specific = msgspec.json.decode(fields["appSpecificKey"], type=dict[str, msgspec.Raw])
    profile = str(PurePosixPath(folder, PurePosixPath(msgspec.json.decode(specific["settingsFile"], type=str)).name))
    count = sum(identity.source == origin.source for _, identity in kept)

    def raw(item: object) -> msgspec.Raw:
        return msgspec.Raw(msgspec.json.encode(item))

    def made(index: int, paper: Paper) -> bytes:
        width, height = (ctypes.c_float(side / Length.POINTS).value for side in paper.size)
        size = f"{round(width, 2):g} x {round(height, 2):g} pt"
        extent = {"height": raw(height), "width": raw(width)}
        scoped = {**specific, "rasterEffectSettings": raw(float(DPI)), "settingsFile": raw(profile)}
        return msgspec.json.encode({
            **fields,
            **extent,
            "appSpecificKey": raw(scoped),
            "description": raw(size),
            "id": raw(f"{origin.source}_{count + index}"),
            "tip": raw(f"Start a new {paper.name} document - {size}"),
            "title": raw(paper.name),
            "units": raw(paper.unit.preset_id),
        })

    return b"[" + b",".join((*(row for row, _ in kept), *starmap(made, enumerate(presets.papers)))) + b"]"


def exchange(library: Library) -> bytes:
    """Swatch library in Adobe Swatch Exchange form: one group of RGB process swatches, each channel its byte over 255 as a 32-bit float."""

    def named(text: str) -> bytes:
        encoded = f"{text}\0".encode("utf-16-be")
        return struct.pack(">H", len(encoded) // 2) + encoded

    def block(kind: int, body: bytes) -> bytes:
        return struct.pack(">HI", kind, len(body)) + body

    blocks = (
        block(0xC001, named(library.name)),
        *(block(0x0001, named(swatch.name) + b"RGB " + struct.pack(">3fH", *(byte / 255 for byte in swatch.color), 2)) for swatch in library.swatches),
        block(0xC002, b""),
    )
    return struct.pack(">4sHHI", b"ASEF", 1, 0, len(blocks)) + b"".join(blocks)


def swatch_list(held: bytes, library: Library) -> bytes:
    """Photoshop's swatch list with the library as its last group in place of a held group of its name: the version 1 colors, the version 2 named colors, and the group hierarchy after them, each channel its byte scaled to 16 bits."""
    count, title = int.from_bytes(held[2:4]), f"{library.name}\0"
    ends = tuple(accumulate(range(count), lambda end, _: end + 14 + 2 * unsigned(held, end + 10), initial=4 + 10 * count + 4))
    swatches, hierarchy = iter((held[start : start + 10], held[start + 10 : end]) for start, end in pairwise(ends)), Descriptor.read(io.BytesIO(held[ends[-1] + 16 :]))
    paired = tuple((entry, next(swatches) if entry.classID == b"preset" else None) for entry in hierarchy[b"hierarchy"])
    opened = next((index for index, (entry, _) in enumerate(paired) if entry.classID == b"Grup" and entry[b"Nm  "] == title), len(paired))
    closed = next((index + 1 for index, (entry, _) in enumerate(paired[opened:], start=opened) if entry.classID == b"groupEnd"), len(paired))
    group = Descriptor(name="\0", classID=b"Grup")
    group.update({b"Nm  ": String(title), b"zuid": String(f"{uuid.uuid5(uuid.NAMESPACE_URL, library.name)}\0")})
    added = tuple((Descriptor(name="\0", classID=b"preset"), (struct.pack(">5H", 0, *(byte * 0x101 for byte in swatch.color), 0), unicode(swatch.name))) for swatch in library.swatches)
    entries = (*paired[:opened], *paired[closed:], (group, None), *added, (Descriptor(name="\0", classID=b"groupEnd"), None))
    listed = tuple(swatch for _, swatch in entries if swatch is not None)
    hierarchy[b"hierarchy"] = List(entry for entry, _ in entries)
    buffer = io.BytesIO()
    hierarchy.write(buffer)
    body, size = struct.pack(">I", 16) + buffer.getvalue(), len(listed)
    return b"".join((
        struct.pack(">HH", 1, size),
        *(color for color, _ in listed),
        struct.pack(">HH", 2, size),
        *(color + name for color, name in listed),
        b"8BIMphry",
        struct.pack(">I", len(body)),
        body,
    ))


def sizes(presets: DocumentPresets) -> bytes:
    """New Document presets file holding the user section alone, one preset per paper grouped in that section, in its unit at the render resolution per inch, indented as Photoshop writes it."""
    section = "user"
    user = tuple(
        Size(name=paper.name, group=section, width=width / paper.unit, height=height / paper.unit, units=paper.unit.preset_id, resolution=float(DPI), resolution_units=Unit.INCHES.preset_id)
        for paper in presets.papers
        for width, height in (paper.size,)
    )
    return msgspec.json.format(msgspec.json.encode({"sections": ({"section": section, "presets": user},)}), indent=4)


def keyed(name: str) -> bytes:
    """Photoshop descriptor key or class: a four-character name as a character id, any other as a length-prefixed string id."""
    return struct.pack(">I", 0 if (size := len(name)) == 4 else size) + name.encode()


def unicode(text: str) -> bytes:
    """Photoshop descriptor text: its UTF-16 unit count with the terminating NUL, then the units big-endian."""
    encoded = f"{text}\0".encode("utf-16-be")
    return struct.pack(">I", len(encoded) // 2) + encoded


def customization(toolbar: Toolbar) -> bytes:
    """Toolbar record: the 8BPF header and one `null` descriptor of the record version, its pad, the options as booleans, the slots as lists of `toolKey` objects, and empty Extra Tools, each four-character key written as a character id."""

    def integer(number: bytes) -> bytes:
        return b"long" + number

    def body(items: tuple[tuple[str, bytes], ...]) -> bytes:
        return struct.pack(">IH", 1, 0) + keyed("null") + struct.pack(">I", len(items)) + b"".join(keyed(name) + item for name, item in items)

    def listed(slots: tuple[tuple[str, ...], ...]) -> bytes:
        return (
            b"VlLs"
            + struct.pack(">I", len(slots))
            + b"".join(b"VlLs" + struct.pack(">I", len(slot)) + b"".join(b"Objc" + body((("toolKey", integer(tool.encode())),)) for tool in slot) for slot in slots)
        )

    return HEADER + body((
        ("tver", integer(struct.pack(">i", 1))),
        ("tpad", integer(struct.pack(">i", 0))),
        *((name, b"bool" + bytes((option,))) for name, option in toolbar.options.items()),
        ("tlst", listed(toolbar.slots)),
        ("oflt", listed(())),
    ))


async def called(prompt: Prompt) -> str:
    """Session source called with its table of the prompt's families, the label, and the prompt."""
    table = msgspec.json.encode({"families": prompt.families, "label": LABEL, "prompt": prompt}).decode()
    return f"{(await anyio.Path(__file__).with_name('session.jsx').read_text(encoding='utf-8')).rstrip()}({table});\n"


def saved_sets(held: bytes, defaults: tuple[bytes, ...], key: int, entry: str) -> bytes:
    """Illustrator's preferences with the alias set, one action on the function key playing the entry's Scripts item, among the sets the Actions panel loads at launch, the Action directory rewritten in the panel's own form where it differs as an unordered tree."""
    parameters = ((int.from_bytes(b"itnm"), ""), (int.from_bytes(b"lcnm"), PurePosixPath(entry).stem))
    event: tuple[tuple[str, Node], ...] = (
        ("useRulersIn1stQuadrant", 1),
        ("internalName", b"adobe_commandManager"),
        ("localizedName", "Access Menu Item"),
        ("isOpen", 0),
        ("isOn", 1),
        ("hasDialog", 0),
        ("parameterCount", len(parameters)),
        *((f"parameter-{index}", (("key", name), ("showInPalette", 0xFFFFFFFF), ("type", b"ustring"), ("value", value))) for index, (name, value) in enumerate(parameters, start=1)),
    )
    action = (("name", LABEL), ("keyIndex", key), ("colorIndex", 0), ("isOpen", 0), ("eventCount", 1), ("event-1", event))
    alias: tuple[tuple[str, Node], ...] = (("version", 3), ("name", LABEL), ("isOpen", 0), ("actionCount", 1), ("action-1", action))

    def reversed_tree(node: tuple[tuple[str, Node], ...]) -> tuple[tuple[str, Node], ...]:
        return tuple((name, reversed_tree(value) if isinstance(value, tuple) else value) for name, value in reversed(node))

    before, opening, body, closing, after = re.split(r"(?s)(\r\t/Action \{\r)(.*?)(?<=\r)(\t\}\r)", held.decode(STORE_TEXT), maxsplit=1)
    section = store_entries(body)
    match dict(section):
        case {"SavedSets": tuple() as saved, "SavedSetCount": int() as count}:
            natural = [name for name, _ in section].index("SavedSets") < [name for name, _ in section].index("SavedSetCount")
            loaded = tuple(value if natural else reversed_tree(value) for index in range(1, count + 1) for name, value in saved if name == f"set-{index}" and isinstance(value, tuple))
        case _:
            loaded = tuple(store_entries(data.decode(STORE_TEXT)) for data in defaults)
    sets = (*(alias if dict(item)["name"] == LABEL else item for item in loaded), *(() if any(dict(item)["name"] == LABEL for item in loaded) else (alias,)))
    target: tuple[tuple[str, Node], ...] = (("SavedSets", tuple((f"set-{index}", item) for index, item in reversed(tuple(enumerate(sets, start=1))))), ("SavedSetCount", len(sets)))
    return held if unordered(section) == unordered(target) else f"{before}{opening}{textual(target, 2)}{closing}{after}".encode(STORE_TEXT)


def nested(held: bytes, path: tuple[str, ...], value: int) -> bytes | None:
    """Illustrator's preferences with the entry at the directory path holding the value, each directory opening at its depth inside its parent, whose lines sit deeper or blank, None where no entry sits at the path."""
    *directories, key = path
    opened = "".join(rf"\r{'\t' * depth}/{re.escape(name)} \{{(?:\r(?:{'\t' * (depth + 1)}[^\r]*)?)*?" for depth, name in enumerate(directories))
    text, found = re.subn(rf"({opened}\r{'\t' * len(directories)}/{re.escape(key)} )[^\r]*", rf"\g<1>{value}", held.decode(STORE_TEXT), count=1)
    return text.encode(STORE_TEXT) if found else None


def script_actions(key: int, script: str) -> bytes:
    """Photoshop's alias action set: one action on the function key whose Scripts event evaluates the entry script file, the item laid out as the factory Scripts item."""

    def pascal(text: bytes) -> bytes:
        return struct.pack(">I", len(text)) + text

    source = f"$.evalFile(new File({msgspec.json.encode(script).decode()}));"
    values = (("jsTx", source), ("jsMs", "undefined"))
    descriptor = unicode("") + keyed("dJsc") + struct.pack(">I", len(values)) + b"".join(keyed(name) + b"TEXT" + unicode(value) for name, value in values)
    item = bytes((0, 1, 0, 0)) + b"TEXT" + pascal(b"AdobeScriptAutomation Scripts") + pascal(b"Scripts") + struct.pack(">i", -1) + descriptor
    action = struct.pack(">HBBH", key, 0, 0, 0) + unicode(LABEL) + bytes(1) + struct.pack(">I", 1) + item
    return unicode(LABEL) + bytes(1) + struct.pack(">I", 1) + action


def palette(held: bytes, alias: bytes) -> bytes:
    """Photoshop's actions palette with the alias set in place of the held set of its name, or after the held sets where none holds it, the version and trailer kept."""
    starts = tuple(accumulate(range(unsigned(held, 4)), lambda end, _: set_end(held, end), initial=8))
    name = alias[: text_end(alias, 0)]
    named = tuple(alias if body[: text_end(body, 0)] == name else body for body in (held[start:end] for start, end in pairwise(starts)))
    listed = named if alias in named else (*named, alias)
    return held[:4] + struct.pack(">I", len(listed)) + b"".join(listed) + held[starts[-1] :]


# --- [FILES]
async def contents(path: anyio.Path) -> bytes | None:
    """File's bytes, None while it is absent."""
    return await path.read_bytes() if await path.exists() else None


async def plugin_files(folders: Folders, row: Row, access: Plugin, prompt: Prompt) -> tuple[Plan, ...]:
    """UXP command plug-in's manifest and main script in its folder under the local plug-ins root, and the registry holding every other row as held and the plug-in's row enabled."""
    folder, listed = f"{access.plugin}_{access.version}", folders.uxp.joinpath("PluginsInfo", "v1", f"{access.host}.json")
    minimum = ".".join(folders.bundle.version.split(".")[:3])
    manifest = {
        "manifestVersion": 5,
        "id": access.plugin,
        "name": LABEL,
        "version": access.version,
        "main": "index.js",
        "host": {"app": access.host, "minVersion": minimum},
        "entrypoints": [{"type": "command", "id": access.plugin, "label": LABEL, "shortcut": {"mac": prompt.leader}}],
    }
    source = msgspec.json.encode(await called(prompt)).decode()
    main = (
        f'const {{ app, ScriptLanguage }} = require("indesign");\nrequire("uxp").entrypoints.setup({{ commands: {{ {access.plugin}: () => app.doScript({source}, ScriptLanguage.JAVASCRIPT) }} }});\n'
    )
    held = await listed.read_bytes()
    kept = tuple(entry for entry in msgspec.json.decode(held, type=dict[str, tuple[Registration, ...]])["plugins"] if entry.plugin_id != access.plugin)
    own = Registration(minimum, LABEL, f"$localPlugins/External/{folder}", access.plugin, "enabled", "uxp", access.version)
    files = ((f"{row.label} manifest.json", "manifest.json", msgspec.json.format(msgspec.json.encode(manifest), indent=4)), (f"{row.label} index.js", "index.js", main.encode()))
    return (
        *[Plan(label, path, await contents(path), body) for label, file, body in files for path in (folders.uxp.joinpath("Plugins", "External", folder, file),)],
        Plan(f"{row.label} {listed.name}", listed, held, msgspec.json.encode({"plugins": (*kept, own)})),
    )


async def planned(folders: Folders, row: Row) -> tuple[Plan | str, ...]:
    """Files' paths in the product's stores, the bytes they hold, and the bytes the row renders from them, the skip line of a plug-in the product's registry does not list enabled, or none for a row of another store."""
    match folders, row.path, row.target:
        case Support(preferences=preferences), Catalog(file=file) as named, Toolbar() as toolbar:
            path = preferences / file
            return (Plan(row.label, path, await contents(path), catalog(named, toolbar)),)
        case Support(preferences=preferences, support=support), Profiles(file=file, folder=folder) as listed, DocumentPresets() as presets:
            path = preferences / file
            data = await path.read_bytes()
            return (Plan(row.label, path, data, profiles(listed, presets, data, str(support / folder))),)
        case Support(support=support), Rendered(file=file), Library() as library:
            path = support / file
            return (Plan(row.label, path, await contents(path), exchange(library)),)
        case Settings(preferences=preferences), Rendered(file=file), Library() as library:
            path = preferences / file
            held = await path.read_bytes()
            return (Plan(row.label, path, held, swatch_list(held, library)),)
        case Settings(preferences=preferences, presets=presets), ActionSet(file=file, entry=entry), Prompt(leader=int() as key) as prompt:
            path, script = preferences / file, presets / entry
            held = await path.read_bytes()
            return (
                Plan(entry, script, await contents(script), (await called(prompt)).encode()),
                Plan(
                    row.label,
                    path,
                    held,
                    saved_sets(held, tuple([await action.read_bytes() for action in sorted([action async for action in (presets / "Actions").glob("*.aia")])]), key, entry)
                    if isinstance(folders, Support)
                    else palette(held, script_actions(key, str(script))),
                ),
            )
        case Support(preferences=preferences), Nested(file=file, path=keys), int() as value:
            path = preferences / file
            held = await path.read_bytes()
            body = nested(held, keys, value)
            return (line(Kind.ERROR, f"{row.label} holds no entry in {path}") if body is None else Plan(row.label, path, held, body),)
        case Settings(preferences=preferences), Rendered(file=file), DocumentPresets() as presets:
            path = preferences / file
            return (Plan(row.label, path, await contents(path), sizes(presets)),)
        case Settings(preferences=preferences), Rendered(file=file), Toolbar() as toolbar:
            path = preferences / file
            return (Plan(row.label, path, await contents(path), customization(toolbar)),)
        case Settings(), Overlay(file=file) as overlay, (red, green, blue):
            plugin_data, enabled = await storage(folders, overlay)
            return (Plan(row.label, plugin_data / file, await contents(plugin_data / file), f"#{red:02x}{green:02x}{blue:02x}".encode()) if enabled else line(Kind.SKIP, str(plugin_data)),)
        case _, Plugin() as command, Prompt() as prompt:
            return await plugin_files(folders, row, command, prompt)
        case _:
            return ()


async def workspace_files(folder: anyio.Path, row: Row, access: Workspace, frame: Frame) -> tuple[Plan | str, ...]:
    """Frame's workspace files arranged from the one the product wrote at its last quit, every other workspace file kept as the owner saved it, or the error line of what the written workspace lacks."""
    paths = tuple(folder / file for file in access.files)
    match await contents(paths[0]):
        case None:
            return (line(Kind.ERROR, f"{row.label} {access.files[0]} is absent from {folder}"),)
        case held if lacking := workspaces.missing(frame, held):
            return (line(Kind.ERROR, f"{row.label} {access.files[0]} holds no panel {', '.join(sorted(map(str, lacking)))}"),)
        case held:
            return tuple([Plan(f"{row.label} {file}", path, await contents(path), body) for file, path, body in zip(access.files, paths, workspaces.rendered(frame, held), strict=True)])


def stamped(data: bytes | None) -> str:
    """Report cell of a file's bytes: their digest, or absent."""
    return ABSENT if data is None else digest(data)


async def committed(plans: tuple[Plan | str, ...]) -> tuple[str, ...]:
    """Report lines of the plans, each file written whole into a folder made on demand or removed on difference, through the privileged install where root owns the folder, and each skip or error line."""
    changed = tuple(plan for plan in plans if isinstance(plan, Plan) and plan.held != plan.body)
    for plan in changed:
        await plan.path.parent.mkdir(parents=True, exist_ok=True)
        match plan.body, (await plan.path.parent.stat()).st_uid:
            case None, 0:
                await anyio.run_process(["/usr/bin/sudo", "-n", "/bin/rm", str(plan.path)])
            case None, _:
                await plan.path.unlink()
            case body, 0:
                async with anyio.NamedTemporaryFile() as staged:
                    await staged.write(body)
                    await staged.flush()
                    await anyio.run_process(["/usr/bin/sudo", "-n", "/usr/bin/install", "-m", "644", staged.wrapped.name, str(plan.path)])
            case body, _:
                await plan.path.write_bytes(body)
    return (*(plan for plan in plans if isinstance(plan, str)), *(line(Kind.CHANGE, plan.label, stamped(plan.held), stamped(plan.body)) for plan in changed))


# --- [COMPOSITION] ----------------------------------------------------------------------


async def written(folders: Folders, declared: tuple[Row, ...]) -> tuple[str, ...]:
    """Report lines of the preference domain values, settings file items, and rendered files the rows declare in the product's stores, written on difference with every product quit, each row's files written before the next row renders from them."""
    return (
        *await grouped(domains(folders.bundle.identifier, declared), domain_written),
        *await grouped([(folders.preferences / access.file, row, access) for row in declared if isinstance(folders, Settings) and isinstance(access := row.path, Serialized)], file_written),
        *[reported for row in declared for reported in await committed(await planned(folders, row))],
    )


async def folder_written(folder: anyio.Path, declared: tuple[Row, ...]) -> tuple[str, ...]:
    """Report lines of the workspace and database files the rows declare under the settings folder the write launch reports, written on difference after that launch's quit."""
    records = [(folder / access.file, row, access) for row in declared if isinstance(access := row.path, Record)]
    return await committed((
        *[plan for row in declared if isinstance(access := row.path, Workspace) and isinstance(frame := row.target, Frame) for plan in await workspace_files(folder, row, access, frame)],
        *[await database(path, tuple((row, access) for named, row, access in records if named == path)) for path in dict.fromkeys(path for path, _, _ in records)],
    ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Folders", "Support", "folder_written", "located", "written"]
