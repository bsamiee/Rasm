"""Blender's stored preferences file read and written through its own SDNA, holding the asset shelf catalog tabs the libraries' catalog trees declare."""

from collections.abc import Iterable, Iterator, Mapping
from compression import zstd
from functools import reduce
from itertools import accumulate, count
from math import prod
from pathlib import Path, PurePosixPath
import re
import struct
from typing import Final
from uuid import UUID

import msgspec

from interface.host import Change

# --- [TYPES] ----------------------------------------------------------------------------

type Shelves = Mapping[str, tuple[str, ...]]

# --- [CONSTANTS] ------------------------------------------------------------------------

BHEAD: Final = struct.Struct("<4siQqq")
POINTER: Final = struct.Struct("<Q")
COUNT: Final = struct.Struct("<i")
ENDB: Final = b"ENDB"
DATA: Final = b"DATA"
SETTINGS: Final = "asset_shelves_settings"
ENABLED: Final = "enabled_catalog_paths"
IDNAME: Final = "shelf_idname"
PATH: Final = "path"

# --- [MODELS] ---------------------------------------------------------------------------


class Field(msgspec.Struct, frozen=True):
    """SDNA member with its type name, byte offset, and byte size."""

    type: str
    offset: int
    size: int


class Structure(msgspec.Struct, frozen=True):
    """SDNA struct with its byte size and members by name."""

    name: str
    size: int
    fields: dict[str, Field]


class Block(msgspec.Struct, frozen=True):
    """File block with its code, SDNA struct index, stored address, element count, and payload."""

    code: bytes
    sdna: int
    old: int
    count: int
    data: bytes


class Blend(msgspec.Struct, frozen=True):
    """File header, SDNA structs, and blocks in file order, with the struct index by name and the block index by stored address."""

    header: bytes
    structures: tuple[Structure, ...]
    blocks: tuple[Block, ...]
    named: dict[str, int]
    addressed: dict[int, int]

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

    def pointer(self, block: Block, path: str) -> int:
        """Stored address the pointer member holds."""
        return int.from_bytes(self.raw(block, path), "little")

    def text(self, block: Block, path: str) -> str:
        """Characters of a char array member up to its terminator."""
        return self.raw(block, path).partition(b"\0")[0].decode()

    def offset(self, name: str, member: str) -> int:
        """Byte offset of the named struct's member."""
        return self.structures[self.named[name]].fields[member].offset

    def target(self, block: Block, path: str) -> Block:
        """Block the pointer member addresses."""
        return self.blocks[self.addressed[self.pointer(block, path)]]

    def chain(self, address: int) -> Iterator[Block]:
        """Block at the address and each block along its next pointers, up to a null one."""
        while address:
            block = self.blocks[self.addressed[address]]
            yield block
            address = self.pointer(block, "next")

    def listed(self, block: Block, path: str) -> tuple[Block, ...]:
        """Links of the ListBase member in list order."""
        return tuple(self.chain(self.pointer(block, f"{path}.first")))


class Library(msgspec.Struct, frozen=True):
    """Asset library folder, remote when a downloaded listing names its assets."""

    folder: Path
    remote: bool


class Meta(msgspec.Struct, frozen=True):
    """Remote asset metadata holding its catalog."""

    catalog_id: UUID | None = None


class RemoteAsset(msgspec.Struct, frozen=True):
    """Remote library asset with its ID type and metadata."""

    id_type: str
    meta: Meta = Meta()


class IndexPage(msgspec.Struct, frozen=True):
    """Page file a remote library index names, relative to the library folder."""

    url: str


class RemoteIndex(msgspec.Struct, frozen=True):
    """Processed remote library index naming its page files."""

    pages: tuple[IndexPage, ...]


class RemotePage(msgspec.Struct, frozen=True):
    """Remote library page of assets."""

    assets: tuple[RemoteAsset, ...]


# --- [ERRORS] ---------------------------------------------------------------------------


class Unreadable(msgspec.Struct, frozen=True):
    """Preferences file absent or outside the 17-byte header format with 8-byte little-endian pointers."""

    path: str

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"{self.path} is absent or no BLENDER17-01v file"


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [FORMAT]
def structures(dna: bytes) -> tuple[Structure, ...]:
    """SDNA structs of a DNA1 payload, each member at the offset its predecessors' sizes give."""

    def strings(offset: int) -> tuple[tuple[str, ...], int]:
        (total,) = COUNT.unpack_from(dna, offset)
        items = dna[offset + COUNT.size :].split(b"\0", total)[:total]
        return tuple(item.decode() for item in items), offset + COUNT.size + sum(len(item) + 1 for item in items) + 3 & ~3

    names, types_at = strings(8)
    types, lengths_at = strings(types_at + 4)
    lengths = struct.unpack_from(f"<{len(types)}H", dna, lengths_at + 4)
    offset = (lengths_at + 4 + 2 * len(types) + 3 & ~3) + 4
    found = list[Structure]()
    for _ in range(COUNT.unpack_from(dna, offset)[0]):
        kind, members = struct.unpack_from("<2H", dna, offset + COUNT.size)
        pairs = struct.unpack_from(f"<{2 * members}H", dna, offset + COUNT.size + 4)
        described = [(types[pairs[index]], names[pairs[index + 1]], lengths[pairs[index]]) for index in range(0, 2 * members, 2)]
        sizes = [(POINTER.size if "*" in name else length) * prod(int(size) for size in re.findall(r"\[(\d+)\]", name)) for _, name, length in described]
        fields = {name.strip("*()").partition("[")[0]: Field(type_name, start, size) for (type_name, name, _), size, start in zip(described, sizes, accumulate(sizes, initial=0), strict=False)}
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


def decoded(data: bytes) -> Blend | None:
    """File the bytes hold, decompressed from Zstandard, None for bytes outside the 17-byte header format."""
    raw = data if data.startswith(b"BLENDER") else zstd.decompress(data)
    match re.match(rb"BLENDER17-01v\d{4}", raw):
        case None:
            return None
        case header:
            blocks = tuple(scanned(raw, header.end()))
            found = structures(next(block.data for block in blocks if block.code == b"DNA1"))
            return Blend(header[0], found, blocks, {item.name: index for index, item in enumerate(found)}, {block.old: index for index, block in enumerate(blocks)})


def encoded(blend: Blend) -> bytes:
    """File bytes of the header, every block behind its header, and the end block."""
    return b"".join((blend.header, *(BHEAD.pack(block.code, block.sdna, block.old, len(block.data), block.count) + block.data for block in blend.blocks), BHEAD.pack(ENDB, 0, 0, 0, 0)))


# --- [SHELVES]
def user(blend: Blend) -> Block:
    """Preferences block."""
    return next(block for block in blend.blocks if block.code == b"USER")


def string(blend: Blend, link: Block) -> str:
    """Catalog path the path link's string block holds."""
    return blend.target(link, PATH).data.partition(b"\0")[0].decode()


def shelves(blend: Blend) -> dict[str, tuple[str, ...]]:
    """Enabled catalog paths of each stored asset shelf by idname, in list order."""
    return {blend.text(shelf, IDNAME): tuple(string(blend, link) for link in blend.listed(shelf, ENABLED)) for shelf in blend.listed(user(blend), SETTINGS)}


def libraries(blend: Blend) -> tuple[Library, ...]:
    """Asset library folders the preferences list."""
    return tuple(Library(Path(blend.text(held, PATH)), bool(blend.text(held, "remote_url"))) for held in blend.listed(user(blend), "asset_libraries"))


def created(blend: Blend, name: str, old: int, member: str, value: bytes) -> Block:
    """Data block of the named struct at the stored address, zero but for the member's value."""
    index = blend.named[name]
    return Block(DATA, index, old, 1, (bytes(blend.offset(name, member)) + value).ljust(blend.structures[index].size, b"\0"))


def edited(blend: Blend, wanted: Shelves) -> Blend:
    """File whose shelf settings hold the wanted paths in order: settings, links, and strings appended, stale links and strings dropped, every list relinked."""
    fresh, preferences = count(max(block.old for block in blend.blocks) + 16, 16), user(blend)
    held = {blend.text(shelf, IDNAME): shelf for shelf in blend.listed(preferences, SETTINGS)}
    added = {name: created(blend, "bUserAssetShelfSettings", next(fresh), IDNAME, name.encode()) for name in wanted if name not in held}
    settings = {**held, **added}
    stored = {name: {string(blend, link): link for link in blend.listed(shelf, ENABLED)} for name, shelf in held.items()}
    strings = {(name, path): Block(DATA, blend.named["raw_data"], next(fresh), 1, f"{path}\0".encode()) for name, paths in wanted.items() for path in paths if path not in stored.get(name, {})}
    links = {key: created(blend, "AssetCatalogPathLink", next(fresh), PATH, POINTER.pack(block.old)) for key, block in strings.items()}
    chains = {name: [stored.get(name, {}).get(path) or links[name, path] for path in paths] for name, paths in wanted.items()}
    stale = {address for name in wanted.keys() & stored.keys() for path, link in stored[name].items() if path not in wanted[name] for address in (link.old, blend.pointer(link, PATH))}
    payloads = {block.old: bytearray(block.data) for block in (*blend.blocks, *added.values(), *links.values())}

    def relinked(owner: Block, path: str, chain: list[Block]) -> None:
        POINTER.pack_into(payloads[owner.old], blend.located(owner, f"{path}.first").offset, chain[0].old if chain else 0)
        POINTER.pack_into(payloads[owner.old], blend.located(owner, f"{path}.last").offset, chain[-1].old if chain else 0)
        for before, link, after in zip([None, *chain], chain, [*chain[1:], None], strict=False):
            POINTER.pack_into(payloads[link.old], blend.located(link, "prev").offset, before.old if before else 0)
            POINTER.pack_into(payloads[link.old], blend.located(link, "next").offset, after.old if after else 0)

    relinked(preferences, SETTINGS, list(settings.values()))
    for name, chain in chains.items():
        relinked(settings[name], ENABLED, chain)
    end = next(index for index, block in enumerate(blend.blocks) if index > blend.addressed[preferences.old] and block.code != DATA)
    order = (*blend.blocks[:end], *added.values(), *(block for key in links for block in (links[key], strings[key])), *blend.blocks[end:])
    kept = tuple(msgspec.structs.replace(block, data=bytes(payloads.get(block.old, block.data))) for block in order if block.old not in stale)
    return msgspec.structs.replace(blend, blocks=kept, addressed={block.old: index for index, block in enumerate(kept)})


# --- [CATALOGS]
def catalogs(folder: Path) -> dict[UUID, str]:
    """Catalog path by id from the library folder's catalog definition file, empty for a folder holding none."""
    definitions, line = folder / "blender_assets.cats.txt", re.compile(r"^\s*([0-9A-Fa-f-]{36})\s*:([^:\n]+)", re.MULTILINE)
    return {UUID(identity): path.strip() for identity, path in line.findall(definitions.read_text(encoding="utf-8"))} if definitions.is_file() else {}


def listing(library: Library) -> Iterator[tuple[UUID, str]]:
    """Catalog id and ID type of each asset a remote library's downloaded listing pages name."""
    index = library.folder / PurePosixPath("_v1", "asset-index.processed.json")
    pages = msgspec.json.decode(index.read_bytes(), type=RemoteIndex).pages if index.is_file() else ()
    return ((asset.meta.catalog_id, asset.id_type) for page in pages for asset in msgspec.json.decode((library.folder / page.url).read_bytes(), type=RemotePage).assets if asset.meta.catalog_id)


def marked(path: Path, types: Mapping[str, str]) -> Iterator[tuple[UUID, str]]:
    """Catalog id and ID type of each asset a blend file marks, its ID type read through the ID code table."""
    match decoded(path.read_bytes()):
        case None:
            return iter(())
        case blend:
            identified = ((block, types[code]) for block in blend.blocks if (code := block.code.rstrip(b"\0").decode("latin-1")) in types and blend.pointer(block, "id.asset_data"))
            return ((UUID(bytes_le=blend.raw(blend.target(block, "id.asset_data"), "catalog_id")), kind) for block, kind in identified)


def assets(library: Library, types: Mapping[str, str]) -> Iterator[tuple[UUID, str]]:
    """Catalog id and ID type of each asset, from a remote library's listing or every blend file under a local library."""
    return listing(library) if library.remote else (asset for path in sorted(library.folder.rglob("*.blend")) for asset in marked(path, types))


def ancestors(path: str) -> Iterator[str]:
    """Catalog path's top-level path and each deeper path down to itself."""
    return accumulate(path.split("/"), lambda head, part: f"{head}/{part}")


def declared(stored: Shelves, listed: Iterable[tuple[str, str]], paths: frozenset[str], kinds: Mapping[str, frozenset[str]]) -> dict[str, tuple[str, ...]]:
    """Stored lists held to catalog paths a library lists, and each of the extension's shelves given its topmost catalogs whose assets are all of the ID types it shows."""
    pairs = {(ancestor, kind) for path, kind in listed for ancestor in ancestors(path)}
    held = {path: frozenset(kind for each, kind in pairs if each == path) for path, _ in pairs}
    return {
        **{name: tuple(path for path in enabled if path in paths) for name, enabled in stored.items()},
        **{name: tuple(sorted(path for path, found in held.items() if found <= shown and not ("/" in path and held[path.rpartition("/")[0]] <= shown))) for name, shown in kinds.items()},
    }


# --- [FILES]
def shelved(path: Path, essentials: Path, types: Mapping[str, str], kinds: Mapping[str, frozenset[str]]) -> tuple[Change, ...] | Unreadable:
    """Change row per shelf whose stored paths differ from the declared ones, the file rewritten to the declared paths, each extension shelf by the ID types it shows."""
    match decoded(path.read_bytes()) if path.is_file() else None:
        case None:
            return Unreadable(str(path))
        case blend:
            folders = (Library(essentials, remote=False), *libraries(blend))
            named = {identity: catalog for library in folders for identity, catalog in catalogs(library.folder).items()}
            listed = tuple((named[identity], kind) for library in folders for identity, kind in assets(library, types) if identity in named)
            before = shelves(blend)
            after = declared(before, listed, frozenset(ancestor for catalog in named.values() for ancestor in ancestors(catalog)), kinds)
            if (data := encoded(edited(blend, after))) != encoded(blend):
                path.write_bytes(data)
            return tuple(Change(f"preferences.asset_shelves.{name}", repr(stored), repr(paths)) for name, paths in after.items() if (stored := before.get(name, ())) != paths)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Unreadable", "shelved"]
