#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.15"
# dependencies = ["anyio", "expression", "msgspec"]
#
# [tool.ty.environment]
# python-platform = "darwin"
#
# [tool.uv]
# prerelease = "allow"
# ///
"""Walks one APFS tree once and reports where its bytes sit, what deleting frees, duplicates, litter, and refused paths, or clones its duplicates."""

from collections import Counter
import ctypes
from datetime import datetime
from enum import CONFORM, IntEnum, IntFlag
import errno
from functools import cache, partial
import hashlib
import heapq
from itertools import chain, groupby
import operator
import os
from pathlib import Path
import queue
import shlex
import shutil
import stat
import string
import struct
import subprocess
import sys
import time
lazy from collections.abc import Callable, Iterable, Iterator
lazy from io import BufferedReader

import anyio
import anyio.to_thread
from expression import NOTHING, Option, Some
from expression.extra.result import catch
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------


class _Vtype(IntEnum):
    VREG = 1
    VDIR = 2
    VLNK = 5


class _Flag(IntFlag, boundary=CONFORM):
    MAY_SHARE_BLOCKS = 0x1
    PURGEABLE = 0x8
    SPARSE = 0x10
    SHARES_ALL_BLOCKS = 0x40
    COMPRESSED = stat.UF_COMPRESSED << 32
    DATALESS = stat.SF_DATALESS << 32


# --- [CONSTANTS] ------------------------------------------------------------------------

_BULK_REQUEST = ctypes.create_string_buffer(struct.pack("<HHIIIII", 5, 0, 0xA2040409, 0, 0x4, 0x7, 0x1308))
_MOUNT_REQUEST = ctypes.create_string_buffer(struct.pack("<HHIIIII", 5, 0, 0, 0, 0x4, 0, 0))
_VOLUME_REQUEST = ctypes.create_string_buffer(struct.pack("<HHIIIII", 5, 0, 0, 0x80800000, 0, 0, 0))
_COMMON = struct.Struct("<I20xIiII")
_FILE = struct.Struct("<q8xIQIqqqQQI")
_UINT32 = struct.Struct("<I")
_MOUNT_ROW = struct.Struct("<II")
_VOLUME_ROW = struct.Struct("<Iq")
_MODTIME_AT = 40
_MOUNTSTATUS_AT = 68
_MOUNTED = 0x3
_POLICIES = ((3, 0, 1), (2, 0, 1), (5, 0, 1))
_BUFFER = 1 << 18
_THREADS = 4
_FLOOR = "1MiB"
_PREFIX = 1 << 20
_TOP = 40
_UNITS = ("B", "KiB", "MiB", "GiB", "TiB", "PiB")
_SCALE = frozendict({unit[0]: 1 << 10 * index for index, unit in enumerate(_UNITS)})
_LIBRARY = "Library"
_CLONE = ("jdupes", "--dedupe", "--param-order")

# --- [MODELS] ---------------------------------------------------------------------------


class _Usage(msgspec.Struct, frozen=True, gc=False, array_like=True):
    files: int = 0
    allocated: int = 0
    private: int = 0
    logical: int = 0
    purgeable: int = 0
    dataless: int = 0
    empty_files: int = 0
    empty_folders: int = 0
    broken_links: int = 0

    def __add__(self, other: _Usage, /) -> _Usage:
        return _Usage(*map(operator.add, msgspec.structs.astuple(self), msgspec.structs.astuple(other), strict=True))

    @property
    def litter(self) -> int:
        return self.empty_files + self.empty_folders + self.broken_links

    @classmethod
    def over(cls, files: Iterable[_File], /) -> _Usage:
        return cls(*map(sum, zip(*(item.usage for item in files), strict=True)))


class _File(msgspec.Struct, frozen=True, gc=False):
    path: str
    vtype: int
    modified: int
    flags: int
    fileid: int
    links: int
    logical: int
    allocated: int
    private: int
    clone: int
    refcount: int
    broken: bool

    @property
    def size(self) -> int:
        return max(self.allocated, self.logical)

    @property
    def usage(self) -> tuple[int, ...]:
        return (
            1,
            self.allocated,
            self.private,
            self.logical,
            self.allocated * bool(self.flags & _Flag.PURGEABLE),
            self.logical * bool(self.flags & _Flag.DATALESS),
            int(self.vtype == _Vtype.VREG and not self.logical),
            0,
            int(self.broken),
        )


class _Refusal(msgspec.Struct, frozen=True, gc=False):
    path: str
    code: int


class _Folder(msgspec.Struct, frozen=True, gc=False):
    path: str
    usage: _Usage = _Usage()
    subfolders: tuple[str, ...] = ()
    files: tuple[_File, ...] = ()
    linked: tuple[_File, ...] = ()
    clones: tuple[tuple[int, int, int, int], ...] = ()
    refused: tuple[_Refusal, ...] = ()


class _Duplicates(msgspec.Struct, frozen=True, gc=False):
    groups: tuple[tuple[_File, ...], ...] = ()
    unread: int = 0


class _Clone(msgspec.Struct, frozen=True, gc=False):
    kept: _File
    copies: tuple[_File, ...]
    held: int
    cloned: int = 0
    freed: int = 0
    status: int = 0
    errors: tuple[str, ...] = ()


class _Tree(msgspec.Struct, frozen=True, gc=False):
    root: str
    floor: int
    folders: tuple[_Folder, ...]
    totals: frozendict[str, _Usage]
    files: tuple[_File, ...]
    physical: int

    @classmethod
    def of(cls, root: str, floor: int, folders: tuple[_Folder, ...], /) -> _Tree:
        linked = {item.fileid: item for item in sorted(chain.from_iterable(folder.linked for folder in folders), key=lambda item: item.path, reverse=True)}
        joined = {parent: _Usage.over(group) for parent, group in groupby(sorted(linked.values(), key=_parent), key=_parent)}
        shared = dict(sorted(((clone, value) for folder in folders for clone, value, _, _ in folder.clones), key=lambda pair: pair[1]))
        full = sorted((clone, fileid, refcount, folder.path) for folder in folders for clone, _, fileid, refcount in folder.clones if refcount)
        streams = ((clone, {fileid: (refcount, path) for _, fileid, refcount, path in group}) for clone, group in groupby(full, key=lambda entry: entry[0]))
        whole = {
            clone: os.path.commonpath(paths) if len(paths := {path for _, path in members.values()}) > 1 else paths.pop()
            for clone, members in streams
            if len(members) == max(refcount for refcount, _ in members.values())
        }
        held = groupby(sorted(whole.items(), key=lambda pair: pair[1]), key=lambda pair: pair[1])
        credited = {path: _Usage(private=sum(shared[clone] for clone, _ in group)) for path, group in held}
        added = {path: joined.get(path, _Usage()) + credited.get(path, _Usage()) for path in joined.keys() | credited.keys()}
        totals: dict[str, _Usage] = {}
        for folder in sorted(folders, key=lambda folder: folder.path.count("/"), reverse=True):
            totals[folder.path] = sum((totals[path] for path in folder.subfolders), start=folder.usage + added.get(folder.path, _Usage()))
        return cls(
            root=root,
            floor=floor,
            folders=folders,
            totals=frozendict(totals),
            files=(*chain.from_iterable(folder.files for folder in folders), *(item for item in linked.values() if item.size >= floor)),
            physical=totals[root].private + sum(value for clone, value in shared.items() if clone not in whole),
        )

    def ranked(self, measure: Callable[[_Usage], int], /) -> tuple[str, ...]:
        scores = ((measure(self.totals[folder.path]) - max((measure(self.totals[path]) for path in folder.subfolders), default=0), folder.path) for folder in self.folders if folder.path != self.root)
        return tuple(sorted(path for _, path in heapq.nlargest(_TOP, (score for score in scores if score[0] > 0))))


# --- [SERVICES] -------------------------------------------------------------------------

_LIBC = ctypes.CDLL(None, use_errno=True)
_LIBC.getattrlistbulk.argtypes = (ctypes.c_int, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_size_t, ctypes.c_uint64)
_LIBC.getattrlist.argtypes = (ctypes.c_char_p, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_size_t, ctypes.c_uint32)
_CF = ctypes.CDLL("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")
_CF.CFURLCreateFromFileSystemRepresentation.argtypes = (ctypes.c_void_p, ctypes.c_char_p, ctypes.c_long, ctypes.c_bool)
_CF.CFURLCreateFromFileSystemRepresentation.restype = ctypes.c_void_p
_CF.CFURLCopyResourcePropertyForKey.argtypes = (ctypes.c_void_p, ctypes.c_void_p, ctypes.POINTER(ctypes.c_void_p), ctypes.c_void_p)
_CF.CFURLCopyResourcePropertyForKey.restype = ctypes.c_bool
_CF.CFRelease.argtypes = (ctypes.c_void_p,)
_IS_PACKAGE = ctypes.c_void_p.in_dll(_CF, "kCFURLIsPackageKey")
_TRUE = ctypes.c_void_p.in_dll(_CF, "kCFBooleanTrue")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _parent(item: _File, /) -> str:
    return item.path.rpartition("/")[0] or "/"


@cache
def _package(path: str, /) -> bool:
    encoded = os.fsencode(path)
    url = _CF.CFURLCreateFromFileSystemRepresentation(None, encoded, len(encoded), Path(path).is_dir())
    value = ctypes.c_void_p()
    read = _CF.CFURLCopyResourcePropertyForKey(url, _IS_PACKAGE, ctypes.byref(value), None)
    _CF.CFRelease(url)
    return read and value.value == _TRUE.value


def _movable(item: _File, /) -> bool:
    path = Path(item.path)
    return item.links == 1 and not any(part.startswith(".") or part == _LIBRARY for part in path.parts) and not any(map(_package, map(str, path.parents)))


def _replaced(item: _File, /) -> bool:
    return catch(os.lstat, exception=OSError)(item.path).map(lambda status: status.st_ino).default_value(item.fileid) != item.fileid


def _size(count: int, /) -> str:
    exponent = min(max(count.bit_length() - 1, 0) // 10, len(_UNITS) - 1)
    return f"{count / (1 << 10 * exponent):.1f}{_UNITS[exponent]}"


def _bytes(text: str, /) -> int:
    number = text.rstrip(string.ascii_letters)
    return int(number) * _SCALE[text[len(number) : len(number) + 1].upper() or "B"]


def _date(seconds: int, /) -> str:
    return datetime.fromtimestamp(seconds).astimezone().date().isoformat()


def _counted(count: int, noun: str, /) -> str:
    return f"{count:,} {noun}{'s' * (count != 1)}"


def _cut(shown: int, total: int, noun: str, /) -> str:
    return f"{shown:,} of {_counted(total, noun)}" if shown < total else _counted(total, noun)


def _count(value: int, /) -> str:
    return f"{value:,}" * bool(value)


def _code(code: int, /) -> str:
    return errno.errorcode.get(code, f"{code}")


def _flags(flags: int, /) -> str:
    return ",".join(f"{flag.name}".lower() for flag in _Flag(flags))


def _row(reference: str, /, **fields: str) -> str:
    return " ".join((reference, *(f"{label}={value}" for label, value in fields.items() if value)))


def _entry(view: memoryview, at: int, prefix: str, /) -> _File | _Refusal | str | None:
    _, error, offset, length, vtype = _COMMON.unpack_from(view, at)
    path = prefix + str(view[at + 28 + offset : at + 27 + offset + length], "utf-8", "surrogateescape")
    if error:
        return _Refusal(path=path, code=error)
    match vtype:
        case _Vtype.VDIR:
            return None if _UINT32.unpack_from(view, at + _MOUNTSTATUS_AT)[0] & _MOUNTED else path
        case _:
            modified, flags, fileid, links, logical, allocated, private, clone, extended, refcount = _FILE.unpack_from(view, at + _MODTIME_AT)
            own = allocated if flags & stat.UF_COMPRESSED and not extended & _Flag.SHARES_ALL_BLOCKS else private
            broken = vtype == _Vtype.VLNK and not Path(path).exists()
            return _File(path, vtype, modified, extended | flags << 32, fileid, links, logical, allocated, own, clone, refcount, broken)


def _entries(descriptor: int, buffer: ctypes.Array[ctypes.c_char], prefix: str, /) -> Iterator[_File | _Refusal | str | None]:
    view = memoryview(buffer).cast("B")
    while (count := int(_LIBC.getattrlistbulk(descriptor, _BULK_REQUEST, buffer, _BUFFER, 0x28))) > 0:
        at = 0
        for _ in range(count):
            yield _entry(view, at, prefix)
            at += _UINT32.unpack_from(view, at)[0]
    if count < 0:
        raise OSError(ctypes.get_errno(), os.strerror(ctypes.get_errno()))


def _folder(floor: int, buffer: ctypes.Array[ctypes.c_char], path: str, /) -> _Folder:
    subfolders: list[str] = []
    single: list[_File] = []
    linked: list[_File] = []
    refused: list[_Refusal] = []
    try:
        descriptor = os.open(path, os.O_RDONLY | os.O_DIRECTORY | os.O_NOFOLLOW)
    except OSError as error:
        return _Folder(path=path, refused=(_Refusal(path=path, code=error.errno or 0),))
    try:
        for entry in _entries(descriptor, buffer, f"{path.rstrip('/')}/"):
            match entry:
                case str():
                    subfolders.append(entry)
                case _Refusal():
                    refused.append(entry)
                case _File(links=links) if links > 1:
                    linked.append(entry)
                case _File():
                    single.append(entry)
    except OSError as error:
        refused.append(_Refusal(path=path, code=error.errno or 0))
    finally:
        os.close(descriptor)
    empty = not (subfolders or single or linked or refused)
    return _Folder(
        path=path,
        usage=msgspec.structs.replace(_Usage.over(single), empty_folders=int(empty)),
        subfolders=tuple(subfolders),
        files=tuple(item for item in single if item.size >= floor),
        linked=tuple(linked),
        clones=tuple(
            (item.clone, item.allocated - item.private, item.fileid, item.refcount if item.refcount > 1 and item.flags & _Flag.SHARES_ALL_BLOCKS else 0)
            for item in chain(single, linked)
            if item.flags & _Flag.MAY_SHARE_BLOCKS
        ),
        refused=tuple(refused),
    )


def _drain(floor: int, pending: queue.Queue[str | None], /) -> list[_Folder]:
    buffer = ctypes.create_string_buffer(_BUFFER)
    folders: list[_Folder] = []
    for path in iter(pending.get, None):
        try:
            folders.append(folder := _folder(floor, buffer, path))
            for subfolder in folder.subfolders:
                pending.put(subfolder)
        except BaseException:
            pending.shutdown(immediate=True)
            raise
        finally:
            pending.task_done()
    return folders


def _close(pending: queue.Queue[str | None], /) -> None:
    pending.join()
    for _ in range(_THREADS):
        pending.put(None)


async def _walk(root: str, floor: int, /) -> _Tree:
    pending: queue.Queue[str | None] = queue.Queue()
    pending.put(root)
    drained, _ = await anyio.gather(anyio.gather(*(anyio.to_thread.run_sync(_drain, floor, pending) for _ in range(_THREADS))), anyio.to_thread.run_sync(_close, pending))
    return _Tree.of(root, floor, tuple(chain.from_iterable(drained)))


def _attributes(request: ctypes.Array[ctypes.c_char], row: struct.Struct, path: str, /) -> Option[int]:
    buffer = ctypes.create_string_buffer(row.size)
    return NOTHING if _LIBC.getattrlist(os.fsencode(path), request, buffer, row.size, 0) else Some(row.unpack(buffer)[1])


@catch(exception=OSError)
def _digest(hasher: Callable[[BufferedReader], bytes], path: str, /) -> bytes:
    with Path(path).open("rb") as stream:
        return hasher(stream)


def _group(files: Iterable[_File], key: Callable[[_File], tuple[int, bytes]], /) -> tuple[tuple[_File, ...], ...]:
    grouped = (tuple(sorted(group, key=lambda item: item.path)) for _, group in groupby(sorted(files, key=key), key=key))
    return tuple(group for group in grouped if len({item.clone for item in group}) > 1)


async def _regroup(limiter: anyio.CapacityLimiter, hasher: Callable[[BufferedReader], bytes], groups: tuple[tuple[_File, ...], ...], /) -> _Duplicates:
    streams = {item.clone: item.path for group in groups for item in group}
    read = await anyio.amap(partial(anyio.to_thread.run_sync, _digest, hasher, limiter=limiter), streams.values())
    digests = {clone: value for clone, digest in zip(streams, read, strict=True) for value in digest.to_option().to_list()}
    found = (item for group in groups for item in group if item.clone in digests)
    return _Duplicates(groups=_group(found, lambda item: (item.logical, digests[item.clone])), unread=len(streams) - len(digests))


async def _duplicates(files: Iterable[_File], /) -> _Duplicates:
    limiter = anyio.CapacityLimiter(os.process_cpu_count() or _THREADS)
    candidates = _group((item for item in files if item.vtype == _Vtype.VREG and item.logical and not item.flags & _Flag.DATALESS), lambda item: (item.logical, b""))
    heads = await _regroup(limiter, lambda stream: hashlib.sha256(stream.read(_PREFIX)).digest(), candidates)
    whole = await _regroup(limiter, lambda stream: hashlib.file_digest(stream, "sha256").digest(), heads.groups)
    return _Duplicates(groups=tuple(sorted(whole.groups, key=_redundant, reverse=True)), unread=heads.unread + whole.unread)


def _redundant(group: tuple[_File, ...], /) -> int:
    return (len({item.clone for item in group}) - 1) * max(item.allocated for item in group)


async def _clone(folders: frozendict[str, _Folder], root: str, group: tuple[_File, ...], /) -> _Clone:
    movable = tuple(filter(_movable, group))
    kept = next(iter(movable), group[0])
    copies = tuple(item for item in movable if item.clone != kept.clone)
    held = len(group) - len(movable) - (kept not in movable)
    if not copies:
        return _Clone(kept=kept, copies=copies, held=held)
    named = {item.path for item in (kept, *copies)}
    places = tuple(dict.fromkeys(map(_parent, (kept, *copies))))
    others = (item.path for place in places for item in chain(folders[place].files, folders[place].linked) if item.logical == kept.logical and item.path not in named)
    before = shutil.disk_usage(root).free
    run = await anyio.run_process((*_CLONE, f"--ext-filter=size=:{kept.logical}", *(f"--ext-filter=nostr:{path}" for path in others), *places), check=False, stdout=subprocess.DEVNULL)
    return _Clone(
        kept=kept,
        copies=copies,
        held=held,
        cloned=sum(map(_replaced, copies)),
        freed=shutil.disk_usage(root).free - before,
        status=run.returncode,
        errors=tuple(str(run.stderr, "utf-8", "surrogateescape").splitlines()),
    )


def _report(tree: _Tree, seconds: float, used: Option[int], duplicates: _Duplicates, /) -> Iterator[str]:
    root, whole, groups, unread = tree.root, tree.totals[tree.root], duplicates.groups, duplicates.unread
    relative = partial(os.path.relpath, start=root)
    refused = sorted((refusal for folder in tree.folders for refusal in folder.refused), key=lambda refusal: refusal.path)
    codes = Counter(_code(refusal.code) for refusal in refused)
    folders, litter = tree.ranked(lambda usage: usage.allocated), tree.ranked(lambda usage: usage.litter)
    pieces = ((whole.empty_files, "empty file"), (whole.empty_folders, "empty folder"), (whole.broken_links, "broken link"))
    totals = (("allocated", whole.allocated), ("physical", tree.physical), ("private", whole.private), ("logical", whole.logical), ("purgeable", whole.purgeable), ("dataless", whole.dataless))
    yield f"usage: {root}"
    yield f"  walked: {_counted(len(tree.folders), 'folder')}, {_counted(whole.files, 'file')}, {seconds:.1f}s"
    yield from (f"  bytes: {', '.join(f'{_size(value)} {label}' for label, value in totals if value)}",) * bool(whole.allocated)
    yield from (f"  volume: {_size(value)} used, {_size(value - tree.physical)} gap" for value in used.to_list())
    if refused:
        call = f" - sudo -H uv run --locked --script {shlex.quote(str(Path(__file__).resolve()))} {shlex.quote(root)}" * ("EACCES" in codes)
        yield f"refused: {_cut(min(_TOP, len(refused)), len(refused), 'path')}, {', '.join(f'{count:,} {code}' for code, count in codes.most_common())}{call}"
        yield from (f"  {_row(relative(refusal.path), code=_code(refusal.code))}" for refusal in refused[:_TOP])
    if folders:
        yield f"folders: {_cut(len(folders), len(tree.folders), 'folder')}"
        yield from (
            f"  {_row(relative(path), allocated=_size(usage.allocated), private=_size(usage.private), logical=_size(usage.logical))}" for path, usage in ((path, tree.totals[path]) for path in folders)
        )
    if tree.files:
        yield f"files: {_cut(min(_TOP, len(tree.files)), whole.files, 'file')}"
        yield from (
            f"  {_row(relative(item.path), allocated=_size(item.allocated), private=_size(item.private), logical=_size(item.logical), modified=_date(item.modified), flags=_flags(item.flags))}"
            for item in heapq.nlargest(_TOP, tree.files, key=lambda item: item.allocated)
        )
    if groups or unread:
        yield f"duplicates: {_cut(min(_TOP, len(groups)), len(groups), 'group')}, {_size(sum(map(_redundant, groups)))} redundant, {_size(tree.floor)} minimum{f', {unread:,} unread' * bool(unread)}"
        yield from (
            line
            for group in groups[:_TOP]
            for streams in ({clone: f"{index}" for index, clone in enumerate(dict.fromkeys(item.clone for item in group), start=1)},)
            for line in (
                f"  {_row(relative(group[0].path), size=_size(group[0].logical), copies=_count(len(group)), streams=_count(len(streams)), redundant=_size(_redundant(group)))}",
                *(f"    {_row(relative(item.path), stream=streams[item.clone])}" for item in group[1:]),
            )
        )
    if litter:
        yield f"litter: {_cut(len(litter), len(tree.folders), 'folder')}, {', '.join(_counted(count, noun) for count, noun in pieces if count)}"
        yield from (
            f"  {_row(relative(path), empty_files=_count(usage.empty_files), empty_folders=_count(usage.empty_folders), broken_links=_count(usage.broken_links))}"
            for path, usage in ((path, tree.totals[path]) for path in litter)
        )


def _cloning(root: str, clones: tuple[_Clone, ...], unread: int, /) -> Iterator[str]:
    relative = partial(os.path.relpath, start=root)
    cloned, copies = sum(item.cloned for item in clones), sum(len(item.copies) for item in clones)
    yield f"clone: {root}"
    yield f"  cloned: {_cut(cloned, copies, 'file')}, {_size(sum(item.freed for item in clones))} freed{f', {unread:,} unread' * bool(unread)}"
    yield from (f"groups: {_cut(min(_TOP, len(clones)), len(clones), 'group')}",) * bool(clones)
    yield from (
        line
        for item in clones[:_TOP]
        for line in (
            f"  {_row(relative(item.kept.path), size=_size(item.kept.logical), copies=_count(len(item.copies)), cloned=_count(item.cloned), held=_count(item.held), freed=_size(item.freed) * bool(item.freed), status=_count(item.status))}",
            *(f"    {error}" for error in item.errors),
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


async def _scan(target: str, floor: str, /) -> tuple[_Tree, float, _Duplicates]:
    for policy in _POLICIES:
        _LIBC.setiopolicy_np(*policy)
    root, started = str(await anyio.Path(target).resolve()), time.monotonic()
    tree = await _walk(root, _bytes(floor))
    seconds = time.monotonic() - started
    return tree, seconds, await _duplicates(tree.files)


async def usage(target: str = "/System/Volumes/Data", floor: str = _FLOOR, /) -> int:
    """Report the tree at target: totals, refused paths, ranked folders, files and duplicates from floor bytes, and litter; exit 66 when target is unreadable."""
    tree, seconds, duplicates = await _scan(target, floor)
    used = _attributes(_MOUNT_REQUEST, _MOUNT_ROW, tree.root).filter(lambda status: bool(status & 1)).bind(lambda _: _attributes(_VOLUME_REQUEST, _VOLUME_ROW, tree.root))
    sys.stdout.writelines(f"{line}\n" for line in _report(tree, seconds, used, duplicates))
    return os.EX_NOINPUT if any(refusal.path == tree.root for folder in tree.folders for refusal in folder.refused) else 0


async def clone(target: str, floor: str = _FLOOR, /) -> int:
    """Clone single-link copies of each duplicate group from floor bytes, outside hidden, Library, and package folders, from one kept file through jdupes; exit with its highest status."""
    tree, _, duplicates = await _scan(target, floor)
    folders = frozendict({folder.path: folder for folder in tree.folders})
    clones = tuple([await _clone(folders, tree.root, group) for group in duplicates.groups])
    sys.stdout.writelines(f"{line}\n" for line in _cloning(tree.root, clones, duplicates.unread))
    return max((item.status for item in clones), default=0)


if __name__ == "__main__":
    match sys.argv[1:]:
        case ["clone", target, *floor]:
            sys.exit(anyio.run(clone, target, *floor))
        case arguments:
            sys.exit(anyio.run(usage, *arguments))
