"""Crash-consistent replacement of one or more sibling-staged files or directories."""

from dataclasses import dataclass
from enum import StrEnum
import hashlib
import math
import os
from pathlib import Path, PurePath
from secrets import token_hex
from shutil import copy2, copytree
import stat
from typing import Final, Self

from expression import Error, Ok, Result
import msgspec
import psutil
import structlog

from assay.core.govern import proc_identity_dead
from assay.core.model import Fault, RailStatus


# --- [TYPES] ----------------------------------------------------------------------------


type Source = tuple[Path, Path]
type Swap = tuple[Path, Path]


class _Phase(StrEnum):
    STAGING = "staging"
    PREPARED = "prepared"
    COMMITTED = "committed"


class _Kind(StrEnum):
    FILE = "file"
    DIRECTORY = "directory"


# --- [CONSTANTS] ------------------------------------------------------------------------


_JOURNAL_LIMIT: Final[int] = 1 << 20
_PREVIOUS: Final[str] = ".previous."
_STAGED: Final[str] = ".staged."


def _required_flag(name: str) -> int:
    value = getattr(os, name, None)
    if not isinstance(value, int):
        raise TypeError(f"Assay transactions require os.{name}")
    return value


CLOSE_ON_EXEC: Final[int] = _required_flag("O_CLOEXEC")
NO_FOLLOW: Final[int] = _required_flag("O_NOFOLLOW")
DIRECTORY_OPEN_FLAGS: Final[int] = os.O_RDONLY | _required_flag("O_DIRECTORY") | CLOSE_ON_EXEC | NO_FOLLOW
FILE_READ_FLAGS: Final[int] = os.O_RDONLY | CLOSE_ON_EXEC | NO_FOLLOW | _required_flag("O_NONBLOCK")


# --- [MODELS] ---------------------------------------------------------------------------


class _Identity(msgspec.Struct, frozen=True, gc=False, forbid_unknown_fields=True):
    device: int
    inode: int
    kind: _Kind


class _Artifact(msgspec.Struct, frozen=True, gc=False, forbid_unknown_fields=True):
    identity: _Identity
    content: bytes


class _Entry(msgspec.Struct, frozen=True, gc=False, forbid_unknown_fields=True):
    staged: str
    target: str
    previous: str
    original: _Artifact | None
    image: _Artifact | None = None


class _Journal(msgspec.Struct, frozen=True, gc=False, forbid_unknown_fields=True):
    pid: int
    create_time: float
    phase: _Phase
    entries: tuple[_Entry, ...]


class _Node(msgspec.Struct, frozen=True, gc=False):
    path: str
    kind: str
    mode: int
    payload: bytes


# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: Final[msgspec.json.Decoder[_Journal]] = msgspec.json.Decoder(_Journal)


# --- [SERVICES] -------------------------------------------------------------------------

_LOG = structlog.get_logger()


class _Directories:
    def __init__(self, paths: tuple[Path, ...], /) -> None:
        self._paths = tuple(dict.fromkeys(paths))
        self._fds: dict[Path, int] = {}

    def __enter__(self) -> Self:
        try:
            self._fds = {path: _open_directory(path) for path in self._paths}
        except BaseException:
            self.__exit__(None, None, None)
            raise
        return self

    def __exit__(self, *_exc: object) -> None:
        for descriptor in self._fds.values():
            os.close(descriptor)
        self._fds.clear()

    def fd(self, path: Path) -> int:
        try:
            return self._fds[path.parent]
        except KeyError as exc:
            raise OSError(f"transaction path escaped its opened parent: {path}") from exc

    def status(self, path: Path) -> os.stat_result | None:
        try:
            return os.stat(path.name, dir_fd=self.fd(path), follow_symlinks=False)
        except FileNotFoundError:
            return None

    def identity(self, path: Path) -> _Identity | None:
        status = self.status(path)
        return None if status is None else _identity(status)

    def artifact(self, path: Path) -> _Artifact | None:
        before = self.identity(path)
        if before is None:
            return None
        nodes = _snapshot(path)
        after = self.identity(path)
        if after != before:
            raise OSError(f"transaction artifact changed while audited: {path}")
        return _Artifact(identity=after, content=_content(nodes))

    def replace(self, source: Path, target: Path) -> None:
        os.replace(source.name, target.name, src_dir_fd=self.fd(source), dst_dir_fd=self.fd(target))

    def remove(self, path: Path, expected: _Artifact | None = None) -> None:
        def remove_at(parent: int, name: str) -> None:
            try:
                status = os.stat(name, dir_fd=parent, follow_symlinks=False)
            except FileNotFoundError:
                return
            if stat.S_ISDIR(status.st_mode):
                child = os.open(name, DIRECTORY_OPEN_FLAGS, dir_fd=parent)
                try:
                    opened = os.fstat(child)
                    if (opened.st_dev, opened.st_ino) != (status.st_dev, status.st_ino):
                        raise OSError(f"transaction cleanup path changed while opened: {path}")
                    with os.scandir(child) as members:
                        for member in members:
                            remove_at(child, member.name)
                finally:
                    os.close(child)
                os.rmdir(name, dir_fd=parent)
            else:
                os.unlink(name, dir_fd=parent)

        if expected is None:
            remove_at(self.fd(path), path.name)
            return
        held = self.artifact(path)
        if held is None:
            return
        if held != expected:
            raise OSError(f"transaction cleanup identity or content changed: {path}")
        remove_at(self.fd(path), path.name)

    def sync(self, *parents: Path) -> None:
        for parent in dict.fromkeys(parents):
            os.fsync(self._fds[parent])


def _sync_path(path: Path, directories: _Directories) -> None:
    def sync_at(parent: int, name: str) -> None:
        status = os.stat(name, dir_fd=parent, follow_symlinks=False)
        if stat.S_ISLNK(status.st_mode):
            return
        if stat.S_ISDIR(status.st_mode):
            child = os.open(name, DIRECTORY_OPEN_FLAGS, dir_fd=parent)
            try:
                with os.scandir(child) as members:
                    for member in members:
                        sync_at(child, member.name)
                os.fsync(child)
            finally:
                os.close(child)
            return
        if not stat.S_ISREG(status.st_mode) or status.st_nlink != 1:
            raise OSError(f"transaction stage contains a special or hard-linked file: {path}")
        child = os.open(name, FILE_READ_FLAGS, dir_fd=parent)
        try:
            opened = os.fstat(child)
            if not stat.S_ISREG(opened.st_mode) or opened.st_nlink != 1 or (opened.st_dev, opened.st_ino) != (status.st_dev, status.st_ino):
                raise OSError(f"transaction stage changed while synchronized: {path}")
            os.fsync(child)
        finally:
            os.close(child)

    sync_at(directories.fd(path), path.name)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PATHS]


def _normalized(path: Path) -> bool:
    return path.is_absolute() and path.name not in {"", ".", ".."} and "." not in path.parts and ".." not in path.parts


def _overlap(left: Path, right: Path) -> bool:
    return left == right or left.is_relative_to(right) or right.is_relative_to(left)


def _scope_safe(marker: Path, targets: tuple[Path, ...]) -> bool:
    return (
        _normalized(marker)
        and bool(targets)
        and len(set(targets)) == len(targets)
        and all(_normalized(target) for target in targets)
        and not any(_overlap(left, right) for index, left in enumerate(targets) for right in targets[index + 1 :])
        and not any(_overlap(marker, target) for target in targets)
    )


# --- [DESCRIPTORS]


def _open_component(parent: int, name: str, path: Path) -> int:
    try:
        return os.open(name, DIRECTORY_OPEN_FLAGS, dir_fd=parent)
    except OSError as exc:
        raise OSError(f"transaction directory path is absent or not alias-free: {path}") from exc


def _open_directory(path: Path) -> int:
    if not path.is_absolute() or "." in path.parts or ".." in path.parts:
        raise OSError(f"directory path is not canonical: {path}")
    parts = path.parts
    descriptor = os.open(parts[0], DIRECTORY_OPEN_FLAGS)
    try:
        for part in parts[1:]:
            child = _open_component(descriptor, part, path)
            os.close(descriptor)
            descriptor = child
    except BaseException:
        os.close(descriptor)
        raise
    return descriptor


def _write_all(descriptor: int, payload: bytes) -> None:
    offset = 0
    while offset < len(payload):
        written = os.write(descriptor, payload[offset:])
        if written <= 0:
            raise OSError("transaction journal write made no progress")
        offset += written


def _digest(descriptor: int) -> bytes:
    digest = hashlib.sha256()
    while chunk := os.read(descriptor, 1 << 20):
        digest.update(chunk)
    return digest.digest()


# --- [IDENTITY]


def _kind(mode: int) -> _Kind:
    if stat.S_ISREG(mode):
        return _Kind.FILE
    if stat.S_ISDIR(mode):
        return _Kind.DIRECTORY
    raise OSError("transaction roots must be regular files or directories")


def _identity(status: os.stat_result) -> _Identity:
    if stat.S_ISREG(status.st_mode) and status.st_nlink != 1:
        raise OSError("hard-linked transaction root is not a complete image")
    return _Identity(device=status.st_dev, inode=status.st_ino, kind=_kind(status.st_mode))


# --- [IMAGE]


def _snapshot(root: Path) -> tuple[_Node, ...]:
    root_status = os.lstat(root)
    root_kind = _kind(root_status.st_mode)
    if root_kind is _Kind.FILE and root_status.st_nlink != 1:
        raise OSError(f"hard-linked transaction source is not a complete image: {root}")
    nodes: list[_Node] = []

    def walk(directory: int, prefix: Path) -> None:
        for name in sorted(os.listdir(directory)):
            relative = prefix / name
            status = os.stat(name, dir_fd=directory, follow_symlinks=False)
            mode = stat.S_IMODE(status.st_mode)
            if stat.S_ISDIR(status.st_mode):
                nodes.append(_Node(path=relative.as_posix(), kind="directory", mode=mode, payload=b""))
                child = os.open(name, DIRECTORY_OPEN_FLAGS, dir_fd=directory)
                try:
                    opened = os.fstat(child)
                    if (opened.st_dev, opened.st_ino) != (status.st_dev, status.st_ino):
                        raise OSError(f"transaction source changed while audited: {root / relative}")
                    walk(child, relative)
                finally:
                    os.close(child)
            elif stat.S_ISREG(status.st_mode):
                if status.st_nlink != 1:
                    raise OSError(f"hard-linked transaction source is not a complete image: {root / relative}")
                child = os.open(name, FILE_READ_FLAGS, dir_fd=directory)
                try:
                    opened = os.fstat(child)
                    if not stat.S_ISREG(opened.st_mode) or opened.st_nlink != 1 or (opened.st_dev, opened.st_ino) != (status.st_dev, status.st_ino):
                        raise OSError(f"transaction source changed while audited: {root / relative}")
                    nodes.append(_Node(path=relative.as_posix(), kind="file", mode=mode, payload=_digest(child)))
                finally:
                    os.close(child)
            elif stat.S_ISLNK(status.st_mode):
                if status.st_nlink != 1:
                    raise OSError(f"hard-linked transaction symlink is not a complete image: {root / relative}")
                target = os.readlink(name, dir_fd=directory)
                if Path(target).is_absolute():
                    raise OSError(f"absolute symlink escapes the transaction image: {root / relative}")
                confirmed = os.stat(name, dir_fd=directory, follow_symlinks=False)
                if confirmed.st_nlink != 1 or (confirmed.st_dev, confirmed.st_ino) != (status.st_dev, status.st_ino):
                    raise OSError(f"transaction source changed while audited: {root / relative}")
                nodes.append(_Node(path=relative.as_posix(), kind="symlink", mode=mode, payload=os.fsencode(target)))
            else:
                raise OSError(f"special file is not admitted in a transaction image: {root / relative}")

    if root_kind is _Kind.DIRECTORY:
        nodes.append(_Node(path=".", kind="directory", mode=stat.S_IMODE(root_status.st_mode), payload=b""))
        descriptor = _open_directory(root)
        try:
            opened = os.fstat(descriptor)
            if (opened.st_dev, opened.st_ino) != (root_status.st_dev, root_status.st_ino):
                raise OSError(f"transaction source changed while audited: {root}")
            walk(descriptor, Path())
        finally:
            os.close(descriptor)
    else:
        descriptor = os.open(root, FILE_READ_FLAGS)
        try:
            opened = os.fstat(descriptor)
            if opened.st_nlink != 1 or (opened.st_dev, opened.st_ino) != (root_status.st_dev, root_status.st_ino):
                raise OSError(f"transaction source changed while audited: {root}")
            nodes.append(_Node(path=".", kind="file", mode=stat.S_IMODE(root_status.st_mode), payload=_digest(descriptor)))
        finally:
            os.close(descriptor)
    result = tuple(nodes)
    _validate_links(root, result)
    return result


def _validate_links(root: Path, nodes: tuple[_Node, ...]) -> None:
    kinds = {node.path: node.kind for node in nodes}
    links = {node.path: os.fsdecode(node.payload) for node in nodes if node.kind == "symlink"}
    for origin, target in links.items():
        pending = [*PurePath(origin).parent.parts, *PurePath(target).parts]
        resolved: list[str] = []
        visited: set[tuple[str, tuple[str, ...]]] = set()
        while pending:
            part = pending.pop(0)
            if part in {"", "."}:
                continue
            if part == "..":
                if not resolved:
                    raise OSError(f"symlink escapes the transaction image: {root / origin}")
                resolved.pop()
                continue
            resolved.append(part)
            candidate = PurePath(*resolved).as_posix()
            if candidate in links:
                state = (candidate, tuple(pending))
                if state in visited:
                    raise OSError(f"cyclic symlink in transaction image: {root / origin}")
                visited.add(state)
                resolved.pop()
                pending[:0] = list(PurePath(links[candidate]).parts)
            elif pending and kinds.get(candidate) != "directory":
                raise OSError(f"dangling symlink in transaction image: {root / origin}")
        destination = PurePath(*resolved).as_posix() if resolved else "."
        if destination not in kinds:
            raise OSError(f"dangling symlink in transaction image: {root / origin}")


def _content(nodes: tuple[_Node, ...]) -> bytes:
    digest = hashlib.sha256()
    for node in nodes:
        for field in (os.fsencode(node.path), node.kind.encode(), node.mode.to_bytes(4, "big"), node.payload):
            digest.update(len(field).to_bytes(8, "big"))
            digest.update(field)
    return digest.digest()


def audit_image(root: Path, argv: tuple[str, ...]) -> Result[None, Fault]:
    """Prove one filesystem image contains only stable, unaliased transaction shapes.

    Returns:
        ``None`` for a complete image, or a fault naming the unsafe shape.
    """
    try:
        _snapshot(root)
    except OSError as exc:
        return Error(Fault(argv, message=str(exc)[:1024]))
    return Ok(None)


# --- [JOURNAL]


def _journal(phase: _Phase, entries: tuple[_Entry, ...]) -> _Journal:
    process = psutil.Process(os.getpid())
    return _Journal(pid=process.pid, create_time=process.create_time(), phase=phase, entries=entries)


def _entry(staged: Path, target: Path, index: int, directories: _Directories) -> _Entry:
    previous = target.with_name(f"{target.name}{_PREVIOUS}{os.getpid()}.{index}")
    return _Entry(staged=str(staged), target=str(target), previous=str(previous), original=directories.artifact(target))


def _safe(entry: _Entry, pid: int, index: int, phase: _Phase) -> bool:
    staged, target, previous = Path(entry.staged), Path(entry.target), Path(entry.previous)
    artifacts = tuple(artifact for artifact in (entry.original, entry.image) if artifact is not None)
    return (
        staged.parent == target.parent == previous.parent
        and len({staged, target, previous}) == 3
        and staged.name == f"{target.name}{_STAGED}{pid}.{index}"
        and previous.name == f"{target.name}{_PREVIOUS}{pid}.{index}"
        and (phase is _Phase.STAGING or entry.image is not None)
        and all(
            artifact.identity.device >= 0 and artifact.identity.inode > 0 and len(artifact.content) == hashlib.sha256().digest_size
            for artifact in artifacts
        )
    )


def _entries_safe(marker: Path, entries: tuple[_Entry, ...], pid: int, phase: _Phase) -> bool:
    targets = {Path(entry.target) for entry in entries}
    auxiliaries = tuple(Path(path) for entry in entries for path in (entry.staged, entry.previous))
    return (
        all(_safe(entry, pid, index, phase) for index, entry in enumerate(entries))
        and len(set(auxiliaries)) == len(auxiliaries)
        and marker not in auxiliaries
        and not targets.intersection(auxiliaries)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class SwapTransaction:
    """One journal whose rollback or committed cleanup covers an ordered set of directory and file swaps."""

    marker: Path
    argv: tuple[str, ...]
    targets: tuple[Path, ...]

    # --- [JOURNAL]

    def _directories(self) -> _Directories:
        return _Directories((self.marker.parent, *(target.parent for target in self.targets)))

    def _read(self, directories: _Directories) -> _Journal | None:
        status = directories.status(self.marker)
        if status is None:
            return None
        if not stat.S_ISREG(status.st_mode) or status.st_nlink != 1 or status.st_size > _JOURNAL_LIMIT:
            raise OSError(f"unsafe transaction journal retained at {self.marker}")
        descriptor = os.open(self.marker.name, FILE_READ_FLAGS, dir_fd=directories.fd(self.marker))
        try:
            opened = os.fstat(descriptor)
            if not stat.S_ISREG(opened.st_mode) or opened.st_nlink != 1 or (opened.st_dev, opened.st_ino) != (status.st_dev, status.st_ino):
                raise OSError(f"transaction journal changed while read: {self.marker}")
            chunks: list[bytes] = []
            size = 0
            while chunk := os.read(descriptor, min(65536, _JOURNAL_LIMIT + 1 - size)):
                chunks.append(chunk)
                size += len(chunk)
                if size > _JOURNAL_LIMIT:
                    break
            payload = b"".join(chunks)
        finally:
            os.close(descriptor)
        if len(payload) > _JOURNAL_LIMIT:
            raise OSError(f"oversized transaction journal retained at {self.marker}")
        return _DECODER.decode(payload)

    def _write(self, journal: _Journal, directories: _Directories, *, create: bool = False) -> None:
        payload = msgspec.json.encode(journal)
        parent = directories.fd(self.marker)
        if create:
            try:
                descriptor = os.open(self.marker.name, os.O_WRONLY | os.O_CREAT | os.O_EXCL | CLOSE_ON_EXEC | NO_FOLLOW, 0o600, dir_fd=parent)
            except FileExistsError as exc:
                raise OSError(f"transaction journal already exists; recover it first: {self.marker}") from exc
            try:
                _write_all(descriptor, payload)
                os.fsync(descriptor)
            finally:
                os.close(descriptor)
            directories.sync(self.marker.parent)
            return
        current = self._read(directories)
        transitions = {
            _Phase.STAGING: frozenset((_Phase.STAGING, _Phase.PREPARED)),
            _Phase.PREPARED: frozenset((_Phase.COMMITTED,)),
            _Phase.COMMITTED: frozenset(),
        }
        if (
            current is None
            or (current.pid, current.create_time) != (journal.pid, journal.create_time)
            or journal.phase not in transitions[current.phase]
        ):
            raise OSError(f"transaction journal ownership or phase changed at {self.marker}")
        pending = self.marker.with_name(f"{self.marker.name}.write.{os.getpid()}.{token_hex(8)}")
        descriptor = os.open(pending.name, os.O_WRONLY | os.O_CREAT | os.O_EXCL | CLOSE_ON_EXEC | NO_FOLLOW, 0o600, dir_fd=parent)
        try:
            _write_all(descriptor, payload)
            os.fsync(descriptor)
            os.close(descriptor)
            descriptor = -1
            if self._read(directories) != current:
                raise OSError(f"transaction journal changed before phase replacement at {self.marker}")
            directories.replace(pending, self.marker)
            directories.sync(self.marker.parent)
        finally:
            if descriptor >= 0:
                os.close(descriptor)
            directories.remove(pending)

    def _discard_marker(self, directories: _Directories) -> None:
        marker = directories.artifact(self.marker)
        if marker is None:
            raise OSError(f"transaction journal disappeared before cleanup: {self.marker}")
        directories.remove(self.marker, marker)
        directories.sync(self.marker.parent)

    # --- [PHASES]

    def _finish_staging(self, entries: tuple[_Entry, ...], directories: _Directories) -> None:
        for entry in entries:
            staged, target, previous = Path(entry.staged), Path(entry.target), Path(entry.previous)
            if directories.identity(previous) is not None:
                raise OSError(f"unexpected recovery backup exists during staging: {previous}")
            if directories.artifact(target) != entry.original:
                raise OSError(f"transaction target changed during staging: {target}")
            held = directories.artifact(staged)
            if held is not None:
                if entry.image is None:
                    raise OSError(f"unowned partial transaction stage retained at {staged}")
                if held != entry.image:
                    raise OSError(f"staging recovery identity or content changed: {staged}")
                directories.remove(staged, held)
        directories.sync(*(Path(entry.target).parent for entry in entries))
        self._discard_marker(directories)

    def _finish_rollback(self, entries: tuple[_Entry, ...], directories: _Directories) -> None:
        for entry in reversed(entries):
            staged, target, previous = Path(entry.staged), Path(entry.target), Path(entry.previous)
            held = directories.artifact(staged)
            backup = directories.artifact(previous)
            if entry.image is None:
                raise OSError(f"prepared transaction has no staged identity: {staged}")
            if held is not None and held != entry.image:
                raise OSError(f"staged recovery identity or content changed: {staged}")
            if entry.original is None:
                if backup is not None:
                    raise OSError(f"unexpected recovery backup exists: {previous}")
                committed = directories.artifact(target)
                if committed is not None and committed != entry.image:
                    raise OSError(f"new transaction target was replaced externally: {target}")
                if committed == entry.image:
                    directories.remove(target, committed)
            elif backup is not None:
                if backup != entry.original:
                    raise OSError(f"recovery backup identity or content changed: {previous}")
                committed = directories.artifact(target)
                if committed is not None and committed != entry.image:
                    raise OSError(f"transaction target was replaced externally: {target}")
                if committed == entry.image:
                    directories.remove(target, committed)
                directories.replace(previous, target)
            elif directories.artifact(target) != entry.original:
                raise OSError(f"recovery source is absent or changed for {target}")
            if held is not None:
                directories.remove(staged, held)
        directories.sync(*(Path(entry.target).parent for entry in entries))
        self._discard_marker(directories)

    def _finish_commit(self, entries: tuple[_Entry, ...], directories: _Directories) -> None:
        for entry in entries:
            staged, target, previous = Path(entry.staged), Path(entry.target), Path(entry.previous)
            if entry.image is None or directories.artifact(target) != entry.image:
                raise OSError(f"committed transaction target is absent or changed: {target}")
            held = directories.artifact(staged)
            if held is not None and held != entry.image:
                raise OSError(f"committed staged identity or content changed: {staged}")
            backup = directories.artifact(previous)
            if entry.original is None and backup is not None:
                raise OSError(f"unexpected committed backup exists: {previous}")
            if entry.original is not None and backup is not None and backup != entry.original:
                raise OSError(f"committed backup identity or content changed: {previous}")
            if backup is not None:
                directories.remove(previous, backup)
            if held is not None:
                directories.remove(staged, held)
        directories.sync(*(Path(entry.target).parent for entry in entries))
        self._discard_marker(directories)

    # --- [SWAP]

    def _prepare(self, entries: tuple[_Entry, ...], directories: _Directories) -> tuple[_Entry, ...]:
        prepared: list[_Entry] = []
        working = list(entries)
        for index, pending in enumerate(entries):
            staged = Path(pending.staged)
            before = directories.artifact(staged)
            if before is None or (pending.image is not None and before != pending.image):
                raise OSError(f"staged transaction input is absent or changed: {staged}")
            if pending.image is None:
                owned = msgspec.structs.replace(pending, image=before)
                working[index] = owned
                self._write(_journal(_Phase.STAGING, tuple(working)), directories)
            else:
                owned = pending
            _sync_path(staged, directories)
            after = directories.artifact(staged)
            if after != before:
                raise OSError(f"staged transaction input changed while synchronized: {staged}")
            prepared.append(msgspec.structs.replace(owned, image=after))
        directories.sync(*(Path(entry.target).parent for entry in prepared))
        result = tuple(prepared)
        self._write(_journal(_Phase.PREPARED, result), directories)
        return result

    def _apply(self, entries: tuple[_Entry, ...], directories: _Directories) -> None:
        for entry in entries:
            staged, target, previous = Path(entry.staged), Path(entry.target), Path(entry.previous)
            if entry.image is None or directories.artifact(staged) != entry.image or directories.artifact(target) != entry.original:
                raise OSError(f"transaction input changed before swap: {target}")
            if directories.identity(previous) is not None:
                raise OSError(f"transaction backup path is occupied: {previous}")
            if entry.original is not None:
                directories.replace(target, previous)
            directories.replace(staged, target)
        directories.sync(*(Path(entry.target).parent for entry in entries))
        self._write(_journal(_Phase.COMMITTED, entries), directories)
        self._finish_commit(entries, directories)

    def _commit(self, entries: tuple[_Entry, ...], directories: _Directories) -> Result[None, Fault]:
        try:
            self._apply(entries, directories)
            return Ok(None)
        except OSError as exc:
            try:
                journal = self._read(directories)
                if journal is None or journal.phase is _Phase.COMMITTED:
                    return Error(Fault(self.argv, message=str(exc)[:1024]))
                self._finish_rollback(entries, directories)
            except (OSError, msgspec.DecodeError) as rollback:
                return Error(Fault(self.argv, message=f"{exc}; rollback: {rollback}"[:1024]))
            return Error(Fault(self.argv, message=str(exc)[:1024]))

    def _abort_staging(self, entries: tuple[_Entry, ...], directories: _Directories, error: OSError) -> Result[None, Fault]:
        try:
            self._finish_staging(entries, directories)
        except OSError as rollback:
            return Error(Fault(self.argv, message=f"{error}; rollback: {rollback}"[:1024]))
        return Error(Fault(self.argv, message=str(error)[:1024]))

    # --- [RECOVER]

    def recover(self) -> Result[str, Fault]:
        """Resolve a dead journal under the owning exclusive lease.

        Returns:
            Recovery direction, or a fault when the journal is live, undecodable, or unsafe.
        """
        if not _scope_safe(self.marker, self.targets):
            return Error(Fault(self.argv, message="transaction target scope is unsafe"))
        try:
            with self._directories() as directories:
                return self._recover_open(directories)
        except OSError as exc:
            return Error(Fault(self.argv, message=str(exc)[:1024]))

    def _recover_open(self, directories: _Directories) -> Result[str, Fault]:
        try:
            journal = self._read(directories)
        except (OSError, msgspec.DecodeError) as exc:
            return Error(Fault(self.argv, message=f"undecodable transaction journal retained at {self.marker}: {exc}"[:1024]))
        if journal is None:
            return Ok("absent")
        if (
            journal.pid <= 0
            or not math.isfinite(journal.create_time)
            or journal.create_time <= 0
            or not journal.entries
            or tuple(Path(entry.target) for entry in journal.entries) != self.targets
            or not _entries_safe(self.marker, journal.entries, journal.pid, journal.phase)
        ):
            return Error(Fault(self.argv, message=f"unsafe transaction journal retained at {self.marker}"))
        if not proc_identity_dead(journal.pid, journal.create_time):
            return Error(Fault(self.argv, status=RailStatus.BUSY, message=f"transaction pending under live pid {journal.pid}"))
        try:
            direction = self._recover_dead(journal, directories)
        except OSError as exc:
            return Error(Fault(self.argv, message=str(exc)[:1024]))
        _LOG.warning("transaction.recover", direction=direction, marker=str(self.marker), phase=journal.phase, pid=journal.pid)
        return Ok(direction)

    def _recover_dead(self, journal: _Journal, directories: _Directories) -> str:
        if journal.phase is _Phase.COMMITTED:
            self._finish_commit(journal.entries, directories)
            return "forward"
        if journal.phase is _Phase.PREPARED:
            self._finish_rollback(journal.entries, directories)
            return "back"
        self._finish_staging(journal.entries, directories)
        return "back"

    # --- [INSTALL]

    def install(self, sources: tuple[Source, ...]) -> Result[None, Fault]:
        """Copy a validated image beside its targets, then commit every target through this journal.

        Returns:
            ``None`` after a complete commit, or a fault after rollback preserves the prior targets.
        """
        invalid = (
            "transaction target scope is unsafe"
            if not _scope_safe(self.marker, self.targets)
            else "transaction has no sources"
            if not sources
            else "transaction targets repeat"
            if len({target for _, target in sources}) != len(sources)
            else "transaction targets differ from the admitted scope"
            if tuple(target for _, target in sources) != self.targets
            else "transaction source scope is unsafe"
            if any(
                not _normalized(source) or any(_overlap(source, target) for target in self.targets) or _overlap(source, self.marker)
                for source, _ in sources
            )
            else ""
        )
        if invalid:
            return Error(Fault(self.argv, message=invalid))
        try:
            with self._directories() as directories:
                return self._install_open(sources, directories)
        except OSError as exc:
            return Error(Fault(self.argv, message=str(exc)[:1024]))

    def _install_open(self, sources: tuple[Source, ...], directories: _Directories) -> Result[None, Fault]:
        swaps = tuple((target.with_name(f"{target.name}{_STAGED}{os.getpid()}.{index}"), target) for index, (_, target) in enumerate(sources))
        entries = tuple(_entry(staged, target, index, directories) for index, (staged, target) in enumerate(swaps))
        if not _entries_safe(self.marker, entries, os.getpid(), _Phase.STAGING):
            return Error(Fault(self.argv, message="transaction auxiliary path scope is unsafe"))
        try:
            source_images = tuple(_snapshot(source) for source, _ in sources)
            self._write(_journal(_Phase.STAGING, entries), directories, create=True)
        except OSError as exc:
            return Error(Fault(self.argv, message=str(exc)[:1024]))
        working = list(entries)
        try:
            self._copy_stages(sources, swaps, source_images, working, directories)
            prepared = self._prepare(tuple(working), directories)
        except OSError as exc:
            return self._abort_staging(tuple(working), directories, exc)
        return self._commit(prepared, directories)

    def _copy_stages(
        self,
        sources: tuple[Source, ...],
        swaps: tuple[Swap, ...],
        source_images: tuple[tuple[_Node, ...], ...],
        entries: list[_Entry],
        directories: _Directories,
    ) -> None:
        for index, ((source, _), (staged, _), expected) in enumerate(zip(sources, swaps, source_images, strict=True)):
            if directories.identity(staged) is not None:
                raise OSError(f"transaction stage path is occupied: {staged}")
            copytree(source, staged, symlinks=True) if source.is_dir() else copy2(source, staged, follow_symlinks=False)
            image = directories.artifact(staged)
            entries[index] = msgspec.structs.replace(entries[index], image=image)
            self._write(_journal(_Phase.STAGING, tuple(entries)), directories)
            if _snapshot(staged) != expected:
                raise OSError(f"staged transaction image differs from its source: {source}")

    # --- [COMMIT]

    def commit(self, swaps: tuple[Swap, ...]) -> Result[None, Fault]:
        """Commit pre-existing sibling stages, as used by the package publisher.

        Returns:
            ``None`` after a complete commit, or a fault after rollback preserves the prior targets.
        """
        invalid = (
            "transaction target scope is unsafe"
            if not _scope_safe(self.marker, self.targets)
            else "transaction has no swaps"
            if not swaps
            else "transaction targets repeat"
            if len({target for _, target in swaps}) != len(swaps)
            else "transaction targets differ from the admitted scope"
            if tuple(target for _, target in swaps) != self.targets
            else "staged transaction input must be a canonical sibling of its target"
            if any(staged != target.with_name(f"{target.name}{_STAGED}{os.getpid()}.{index}") for index, (staged, target) in enumerate(swaps))
            else ""
        )
        if invalid:
            return Error(Fault(self.argv, message=invalid))
        try:
            with self._directories() as directories:
                return self._commit_open(swaps, directories)
        except OSError as exc:
            return Error(Fault(self.argv, message=str(exc)[:1024]))

    def _commit_open(self, swaps: tuple[Swap, ...], directories: _Directories) -> Result[None, Fault]:
        entries = tuple(_entry(staged, target, index, directories) for index, (staged, target) in enumerate(swaps))
        if not _entries_safe(self.marker, entries, os.getpid(), _Phase.STAGING):
            return Error(Fault(self.argv, message="transaction auxiliary path scope is unsafe"))
        try:
            self._write(_journal(_Phase.STAGING, entries), directories, create=True)
        except OSError as exc:
            return Error(Fault(self.argv, message=str(exc)[:1024]))
        try:
            prepared = self._prepare(entries, directories)
        except OSError as exc:
            return self._abort_staging(entries, directories, exc)
        return self._commit(prepared, directories)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("audit_image", "CLOSE_ON_EXEC", "DIRECTORY_OPEN_FLAGS", "FILE_READ_FLAGS", "NO_FOLLOW", "Source", "Swap", "SwapTransaction")
