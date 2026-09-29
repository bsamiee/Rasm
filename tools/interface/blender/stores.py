# mypy: disable-error-code="no-any-return"
"""Blender's stored preferences and startup files read and written through their own SDNA: the enabled catalog paths of the extension's asset shelves and the stock compositor shelf, and the logical width and zoom of every declared sidebar and toolbar."""

from collections.abc import Iterable, Iterator, Mapping
from functools import cached_property, reduce
from itertools import accumulate, batched, count
from math import prod
from pathlib import Path
import re
import struct
from types import MappingProxyType
from typing import Final

import msgspec

from interface.blender.packages import Manifest
from interface.blender.rows import Width
from interface.host import Change, Error
from interface.report import ABSENT, subscript

# --- [CONSTANTS] ------------------------------------------------------------------------

ENDB: Final = b"ENDB"
DATA: Final = b"DATA"
DNA1: Final = b"DNA1"
USER: Final = b"USER"
SETTINGS: Final = "asset_shelves_settings"
ENABLED: Final = "enabled_catalog_paths"
IDNAME: Final = "shelf_idname"
PATH: Final = "path"

# --- [CODECS] ---------------------------------------------------------------------------

BHEAD: Final = struct.Struct("<4siQqq")
POINTER: Final = struct.Struct("<Q")
SCALARS: Final = MappingProxyType({"char": struct.Struct("<B"), "short": struct.Struct("<h"), "float": struct.Struct("<f")})

# --- [MODELS] ---------------------------------------------------------------------------


class Field(msgspec.Struct, frozen=True):
    """SDNA member with its type name, byte offset, byte size, and whether its name declares a pointer."""

    type: str
    offset: int
    size: int
    pointer: bool

    @property
    def codec(self) -> struct.Struct:
        """Codec of the member's value, a pointer's stored address or a scalar by its SDNA type."""
        return POINTER if self.pointer else SCALARS[self.type]


class Structure(msgspec.Struct, frozen=True):
    """SDNA struct with its byte size and members by name."""

    name: str
    size: int
    fields: Mapping[str, Field]


class Block(msgspec.Struct, frozen=True):
    """File block with its code, SDNA struct index, stored address, element count, and payload."""

    code: bytes
    sdna: int
    old: int
    count: int
    data: bytes


class Blend(msgspec.Struct, frozen=True, dict=True):
    """File header, SDNA structs, and blocks in file order, each index over them derived once on first read."""

    header: bytes
    structures: tuple[Structure, ...]
    blocks: tuple[Block, ...]

    @cached_property
    def named(self) -> Mapping[str, int]:
        """Struct index by struct name."""
        return {item.name: index for index, item in enumerate(self.structures)}

    @cached_property
    def addressed(self) -> Mapping[int, int]:
        """Block index by stored address."""
        return {block.old: index for index, block in enumerate(self.blocks)}

    @cached_property
    def coded(self) -> Mapping[bytes, Block]:
        """Last block of each code, the one DNA1 block and a preferences file's one USER block among them."""
        return {block.code: block for block in self.blocks}

    def member(self, outer: Field, name: str) -> Field:
        """Member of the outer member's struct, its offset counted from the outer member's block start."""
        inner = self.structures[self.named[outer.type]].fields[name]
        return msgspec.structs.replace(inner, offset=outer.offset + inner.offset)

    def located(self, block: Block, path: str) -> Field:
        """Member the dotted path names inside the block's struct, its offset counted from the block start."""
        first, *rest = path.split(".")
        return reduce(self.member, rest, self.structures[block.sdna].fields[first])

    def raw(self, block: Block, path: str) -> bytes:
        """Bytes of the member."""
        field = self.located(block, path)
        return block.data[field.offset : field.offset + field.size]

    def number(self, block: Block, path: str) -> int | float:
        """Value of the scalar or pointer member through its codec, a pointer as its stored address."""
        field = self.located(block, path)
        return field.codec.unpack_from(block.data, field.offset)[0]

    def packed(self, payload: bytearray, block: Block, path: str, value: float) -> None:
        """Write the block's scalar or pointer member into its payload through its codec."""
        field = self.located(block, path)
        field.codec.pack_into(payload, field.offset, value)

    def text(self, block: Block, path: str) -> str:
        """Characters of a char array member up to its terminator."""
        return self.raw(block, path).partition(b"\0")[0].decode()

    def offset(self, name: str, member: str) -> int:
        """Byte offset of the named struct's member."""
        return self.structures[self.named[name]].fields[member].offset

    def target(self, block: Block, path: str) -> Block:
        """Block the pointer member addresses."""
        return self.blocks[self.addressed[int(self.number(block, path))]]

    def chain(self, address: int) -> Iterator[Block]:
        """Block at the address and each block along its next pointers, up to a null one."""
        while address:
            block = self.blocks[self.addressed[address]]
            yield block
            address = int(self.number(block, "next"))

    def listed(self, block: Block, path: str) -> tuple[Block, ...]:
        """Links of the ListBase member in list order."""
        return tuple(self.chain(int(self.number(block, f"{path}.first"))))


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [FORMAT]
def structures(dna: bytes) -> tuple[Structure, ...]:
    """SDNA structs of a DNA1 payload, each member at the offset its predecessors' sizes give."""
    int32 = struct.Struct("<i")

    def strings(offset: int) -> tuple[tuple[str, ...], int]:
        (total,) = int32.unpack_from(dna, offset)
        items = dna[offset + int32.size :].split(b"\0", total)[:total]
        return tuple(item.decode() for item in items), offset + int32.size + sum(len(item) + 1 for item in items) + 3 & ~3

    names, types_at = strings(8)
    types, lengths_at = strings(types_at + 4)
    lengths = struct.unpack_from(f"<{len(types)}H", dna, lengths_at + 4)
    offset = (lengths_at + 4 + 2 * len(types) + 3 & ~3) + 4
    found = list[Structure]()
    for _ in range(int32.unpack_from(dna, offset)[0]):
        kind, members = struct.unpack_from("<2H", dna, offset + int32.size)
        described = [
            (types[kind_index], (name := names[name_index]), "*" in name, lengths[kind_index])
            for kind_index, name_index in batched(struct.unpack_from(f"<{2 * members}H", dna, offset + int32.size + 4), 2, strict=True)
        ]
        sizes = [(POINTER.size if pointer else length) * prod(int(size) for size in re.findall(r"\[(\d+)\]", name)) for _, name, pointer, length in described]
        fields = {
            name.strip("*()").partition("[")[0]: Field(type_name, start, size, pointer)
            for (type_name, name, pointer, _), size, start in zip(described, sizes, accumulate(sizes, initial=0), strict=False)
        }
        found.append(Structure(types[kind], lengths[kind], fields))
        offset += 4 + 4 * members
    return tuple(found)


def scanned(data: bytes, offset: int) -> Iterator[Block]:
    """Blocks from the offset up to the end block."""
    while (head := BHEAD.unpack_from(data, offset))[0] != ENDB:
        code, sdna, old, length, total = head
        start = offset + BHEAD.size
        yield Block(code, sdna, old, total, data[start : start + length])
        offset = start + length


def decoded(path: Path) -> Blend | Error:
    """File the path holds, or the reason it holds no readable file: a header outside the 17-byte format, or a count of SDNA blocks other than one."""
    data = path.read_bytes()
    match re.match(rb"BLENDER17-01v\d{4}", data):
        case None:
            return Error(f"{path} opens with {data[:17]!r}, outside the BLENDER17-01v header format")
        case header:
            blocks = tuple(scanned(data, header.end()))
            match [block for block in blocks if block.code == DNA1]:
                case [dna]:
                    return Blend(header[0], structures(dna.data), blocks)
                case dnas:
                    return Error(f"{path} holds {len(dnas)} DNA1 blocks")


def encoded(blend: Blend) -> bytes:
    """File bytes of the header, every block behind its header, and the end block."""
    return b"".join((blend.header, *(BHEAD.pack(block.code, block.sdna, block.old, len(block.data), block.count) + block.data for block in blend.blocks), BHEAD.pack(ENDB, 0, 0, 0, 0)))


def rebuilt(blend: Blend, order: Iterable[Block], payloads: Mapping[int, bytearray]) -> Blend:
    """File holding the blocks in order, each with its written payload where one exists."""
    return msgspec.structs.replace(blend, blocks=tuple(msgspec.structs.replace(block, data=bytes(payloads.get(block.old, block.data))) for block in order))


# --- [SHELVES]
def string(blend: Blend, link: Block) -> str:
    """Catalog path the path link's string block holds."""
    return blend.target(link, PATH).data.partition(b"\0")[0].decode()


def shelves(blend: Blend) -> dict[str, tuple[str, ...]]:
    """Enabled catalog paths of each stored asset shelf by idname, in list order."""
    return {blend.text(shelf, IDNAME): tuple(string(blend, link) for link in blend.listed(shelf, ENABLED)) for shelf in blend.listed(blend.coded[USER], SETTINGS)}


def created(blend: Blend, name: str, old: int, member: str, value: bytes) -> Block:
    """Data block of the named struct at the stored address, zero but for the member's value."""
    index = blend.named[name]
    return Block(DATA, index, old, 1, (bytes(blend.offset(name, member)) + value).ljust(blend.structures[index].size, b"\0"))


def edited(blend: Blend, wanted: Mapping[str, tuple[str, ...]]) -> Blend:
    """File whose shelf settings hold the wanted paths in order: settings, links, and strings appended, stale links and strings dropped, every list relinked."""
    fresh, preferences = count(max(block.old for block in blend.blocks) + 16, 16), blend.coded[USER]
    held = {blend.text(shelf, IDNAME): shelf for shelf in blend.listed(preferences, SETTINGS)}
    added = {name: created(blend, "bUserAssetShelfSettings", next(fresh), IDNAME, name.encode()) for name in wanted if name not in held}
    settings = {**held, **added}
    stored = {(name, string(blend, link)): link for name, shelf in held.items() for link in blend.listed(shelf, ENABLED)}
    strings = {(name, path): Block(DATA, blend.named["raw_data"], next(fresh), 1, f"{path}\0".encode()) for name, paths in wanted.items() for path in paths if (name, path) not in stored}
    links = {key: created(blend, "AssetCatalogPathLink", next(fresh), PATH, POINTER.pack(block.old)) for key, block in strings.items()}
    linked = {**stored, **links}
    chains = {name: [linked[name, path] for path in paths] for name, paths in wanted.items()}
    stale = {address for (name, path), link in stored.items() if name in wanted and path not in wanted[name] for address in (link.old, blend.number(link, PATH))}
    payloads = {block.old: bytearray(block.data) for block in (*blend.blocks, *added.values(), *links.values())}

    def relinked(owner: Block, path: str, chain: list[Block]) -> None:
        blend.packed(payloads[owner.old], owner, f"{path}.first", chain[0].old if chain else 0)
        blend.packed(payloads[owner.old], owner, f"{path}.last", chain[-1].old if chain else 0)
        for before, link, after in zip([None, *chain], chain, [*chain[1:], None], strict=False):
            blend.packed(payloads[link.old], link, "prev", before.old if before else 0)
            blend.packed(payloads[link.old], link, "next", after.old if after else 0)

    relinked(preferences, SETTINGS, list(settings.values()))
    for name, chain in chains.items():
        relinked(settings[name], ENABLED, chain)
    end = blend.addressed[blend.coded[DNA1].old]
    order = (*blend.blocks[:end], *added.values(), *(block for key in links for block in (links[key], strings[key])), *blend.blocks[end:])
    return rebuilt(blend, (block for block in order if block.old not in stale), payloads)


# --- [REGIONS]
def placed(blend: Blend) -> Iterator[tuple[str, int, int, Block]]:
    """Each stored region with its screen's name, its area's index, and the space type it serves: the active space's regions in the area's list, each stored space's own after them."""
    yield from (
        (blend.text(screen, "id.name")[2:], index, int(blend.number(owner, "spacetype")), region)
        for screen in blend.blocks
        if screen.sdna == blend.named["bScreen"]
        for index, area in enumerate(blend.listed(screen, "areabase"))
        for owner in (area, *blend.listed(area, "spacedata")[1:])
        for region in blend.listed(owner, "regionbase")
    )


# --- [STORE]
def shelved(path: Path, manifest: Manifest, essentials: frozenset[str]) -> tuple[Change | Error, ...]:
    """Change row per shelf whose stored paths differ from the wanted ones, the file rewritten to them: each manifest shelf's catalog paths and the stored compositor list held to the Essentials catalogs, or the error of a file the reader cannot decode or that holds no preferences block."""
    match decoded(path):
        case Error() as error:
            return (error,)
        case Blend() as blend if USER not in blend.coded:
            return (Error(f"{path} holds no USER block"),)
        case Blend() as blend:
            before, compositor = shelves(blend), "NODE_AST_compositor"
            pruned = {} if (stored := before.get(compositor)) is None else {compositor: tuple(each for each in stored if each in essentials)}
            wanted = {**manifest.shelves, **pruned}
            changes = tuple(
                Change(f"preferences.{subscript(SETTINGS, name)}.{ENABLED}", ABSENT if held is None else repr(held), repr(paths))
                for name, paths in wanted.items()
                if (held := before.get(name)) != paths
            )
            if changes:
                path.write_bytes(encoded(edited(blend, wanted)))
            return changes


def regions(path: Path, widths: tuple[Width, ...]) -> tuple[Change | Error, ...]:
    """Change row per stored region of a declared editor and region type whose logical width or zoom differs, the startup file rewritten to them: `sizex`, and the view `cur` whose extent over the last drawn size is the zoom `V2D_KEEPZOOM` keeps at the next layout."""
    if isinstance(blend := decoded(path), Error):
        return (blend,)
    table, payloads, rows = {(width.space, width.region): width for width in widths}, dict[int, bytearray](), list[Change]()
    declared = ((screen, index, region, table[key]) for screen, index, space, region in placed(blend) if (key := (space, int(blend.number(region, "regiontype")))) in table)
    for screen, index, region, width in declared:
        drawn_x, drawn_y, left, right, top = (blend.number(region, f"v2d.{member}") for member in ("oldwinx", "oldwiny", "cur.xmin", "cur.xmax", "cur.ymax"))
        held = (blend.number(region, "sizex"), round(drawn_x / (right - left), 4) if drawn_x else None)
        wanted = (width.width, round(width.zoom, 4) if drawn_x else None)
        if held != wanted:
            payload = payloads[region.old] = bytearray(region.data)
            for member, value in {"sizex": width.width, **({"v2d.cur.xmax": left + drawn_x / width.zoom, "v2d.cur.ymin": top - drawn_y / width.zoom} if drawn_x else {})}.items():
                blend.packed(payload, region, member, value)
            rows.append(Change(f"startup.{subscript('screens', screen)}.areas[{index}].{width.editor}.{width.kind}", repr(held), repr(wanted)))
    if payloads:
        path.write_bytes(encoded(rebuilt(blend, blend.blocks, payloads)))
    return tuple(rows)


def edit(folder: Path, manifest: Manifest, essentials: frozenset[str], widths: tuple[Width, ...]) -> tuple[Change | Error, ...]:
    """Change rows of the stored shelves in the preferences file and the stored region widths in the startup file, each file rewritten on difference."""
    return (*shelved(folder / "userpref.blend", manifest, essentials), *regions(folder / "startup.blend", widths))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["edit"]
