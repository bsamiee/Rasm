"""Record files, atomic writes, command arguments, content hashes and the generators they seed, and mirror paths."""

import base64
from collections.abc import Callable, Iterable
from contextlib import ExitStack
from datetime import datetime
from functools import partial
import hashlib
from itertools import starmap
import os
from pathlib import Path
import shutil
import sys
import tempfile
from typing import Annotated, Final, overload, Protocol

import cbor2
from expression import Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
from typing_extensions import TypeForm

from rasm_records._faults import accumulated, Error, Fault, Faults, Nothing, Ok, Outcome, Reason

if sys.platform == "darwin":
    import fcntl

# --- [TYPES] ----------------------------------------------------------------------------


class Provider(Protocol):
    """Package whose name and version a content hash holds."""

    @property
    def __name__(self) -> str:
        """Import name of the package."""
        ...

    @property
    def __version__(self) -> str:
        """Release version of the package."""
        ...


type ContentHash = Annotated[bytes, msgspec.Meta(min_length=_DIGEST_SIZE, max_length=_DIGEST_SIZE)]

# --- [CONSTANTS] ------------------------------------------------------------------------

_DIGEST_SIZE: Final = 16

# --- [SERVICES] -------------------------------------------------------------------------


class RecordFile[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """JSON or TOML file, by its suffix, holding one record of its form."""

    path: Path
    form: TypeForm[T]

    def read(self) -> Outcome[T]:
        """Decodes and validates the record.

        Returns:
            Ok(record), `UNREADABLE` fault for a missing or unreadable file, `INVALID` for a refused decode.
        """
        decode = msgspec.toml.decode if self.path.suffix == ".toml" else msgspec.json.decode
        return read(self.path).bind(lambda data: catch(lambda: decode(data, type=self.form), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, str(self.path))))  # ty: ignore[unsound-return-statement]

    def found(self) -> Outcome[Option[T]]:
        """Decodes and validates the record a present file holds.

        Returns:
            Ok(Some(record)), Ok(Nothing) while no file sits at the path, `UNREADABLE` fault for an unreadable file, `INVALID` for a refused decode.
        """

        def absent(faults: Faults) -> Outcome[Option[T]]:
            match faults:
                case (Fault(reason=Reason.UNREADABLE, cause=FileNotFoundError()),):
                    return Ok(Nothing)
                case _:
                    return Error(faults)

        return self.read().map(Some).or_else_with(absent)

    def written(self, value: T, *, place: Callable[[Path, Path], None] = os.replace) -> Outcome[Path]:
        """Writes the record in sorted key order through one atomic commit.

        Args:
            value: Record to hold.
            place: Atomic rename onto the path, `no_clobber_move` refusing a held file.

        Returns:
            Ok(path), `UNWRITABLE` fault for a refused write.
        """
        encode = msgspec.toml.encode if self.path.suffix == ".toml" else msgspec.json.encode
        return committed(self.path, encode(value, order="deterministic"), place=place)

    def revised(self, change: Callable[[T], T]) -> Outcome[T]:
        """Writes change of the held record.

        Args:
            change: Pure update of the decoded record.

        Returns:
            Ok(changed record), `UNREADABLE` or `UNWRITABLE` fault.
        """
        return self.read().map(change).bind(lambda value: self.written(value).map(lambda _: value))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [READS]


def read(path: Path, /) -> Outcome[bytes]:
    """Reads one file's bytes, `UNREADABLE` fault for a failed read."""
    return catch(Path.read_bytes, exception=OSError)(path).map_error(partial(Fault.of, Reason.UNREADABLE, str(path)))


# --- [WRITES]


def _written(act: Callable[[Path], object], path: Path, /) -> Outcome[Path]:
    return catch(act, exception=OSError)(path).map(lambda _: path).map_error(partial(Fault.of, Reason.UNWRITABLE, str(path)))


folder_made: Final = partial(_written, partial(Path.mkdir, parents=True, exist_ok=True))
removed: Final = partial(_written, partial(Path.unlink, missing_ok=True))


def _flushed(*paths: Path) -> None:
    """Holds each file and POSIX folder through power loss, `F_FULLFSYNC` past the drive cache on macOS, Windows opening no folder descriptor and syncing only a writable one."""
    for path in paths:
        if path.is_file() or (os.name == "posix" and path.is_dir()):
            descriptor = os.open(path, os.O_RDONLY if os.name == "posix" else os.O_RDWR)
            try:
                if sys.platform == "darwin":
                    fcntl.fcntl(descriptor, fcntl.F_FULLFSYNC)  # ty: ignore[possibly-missing-attribute]
                else:
                    os.fsync(descriptor)
            finally:
                os.close(descriptor)


def no_clobber_move(source: Path, target: Path, /) -> None:
    """Moves source onto a free target by a hard link then an unlink, `FileExistsError` refusing a held target, a case change of source's own name renaming it in place."""
    try:
        target.hardlink_to(source)
    except FileExistsError:
        if not source.samefile(target):
            raise
        source.replace(target)
    else:
        source.unlink()
    _flushed(*{source.parent, target.parent})


@catch(exception=OSError)
def _placed(place: Callable[[Path, Path], None], stage: Path, path: Path, prior: Path, /) -> Path:
    _flushed(stage, *stage.rglob("*"))
    with ExitStack() as undo:
        if stage.is_dir() and path.is_dir():
            path.replace(prior)
            undo.callback(prior.replace, path)
        place(stage, path)
        undo.pop_all()
    _flushed(path.parent)
    return path


@overload
def committed(path: Path, write: bytes, /, *, place: Callable[[Path, Path], None] = os.replace) -> Outcome[Path]: ...
@overload
def committed(path: Path, write: Callable[[Path], Outcome[object]], /) -> Outcome[Path]: ...
def committed(path: Path, write: bytes | Callable[[Path], Outcome[object]], /, *, place: Callable[[Path, Path], None] = os.replace) -> Outcome[Path]:
    """Writes `path` whole through a stage beside it, replacing it in one atomic step.

    Args:
        path: File or folder to replace, its parent folder created.
        write: Bytes the file holds, or writer of the stage, a path named as `path` in a temporary sibling folder removed on every path.
        place: Atomic rename of the stage onto `path`, `no_clobber_move` refusing a held file with `FileExistsError` as fault cause.

    Returns:
        Ok(path), writer faults, or `UNWRITABLE` fault for folder creation or replacement. A held folder leaves for the stage folder by two renames, renamed back when the second fails.
    """
    unwritable = partial(Fault.of, Reason.UNWRITABLE, str(path))

    def replaced(folder: tempfile.TemporaryDirectory[str]) -> Outcome[Path]:
        with folder as name:
            stage = Path(name) / path.name
            staged: Outcome[object] = catch(stage.write_bytes, exception=OSError)(write).map_error(unwritable) if isinstance(write, bytes) else write(stage)
            return staged.bind(lambda _: _placed(place, stage, path, Path(name) / f"{path.name}.prior").map_error(unwritable))

    return folder_made(path.parent).bind(lambda parent: catch(tempfile.TemporaryDirectory[str], exception=OSError)(dir=parent).map_error(unwritable)).bind(replaced)


def files_copied(pairs: Iterable[tuple[Path, Path]]) -> Outcome[tuple[Path, ...]]:
    """Copies distinct pairs, returning targets in path order."""

    def copy(source: Path, target: Path) -> Path:
        return target if target.exists() and target.samefile(source) else Path(shutil.copytree(source, target, dirs_exist_ok=True) if source.is_dir() else shutil.copy2(source, target))

    def one(source: Path, target: Path) -> Outcome[Path]:
        return folder_made(target.parent).bind(lambda _: _written(partial(copy, source), target))

    return accumulated(starmap(one, sorted(frozenset(pairs))))


# --- [ARGUMENTS]


def argument(record: msgspec.Struct) -> str:
    """Returns the command argument holding record, base64 JSON that no shell, worker, or host argument parser rewrites."""
    return base64.b64encode(msgspec.json.encode(record)).decode("ascii")


def argued[T](decoder: msgspec.json.Decoder[T], subject: str, text: str, /) -> Outcome[T]:
    """Returns the record a command argument holds, `UNREADABLE` fault over subject for text no `argument` wrote."""
    unreadable = partial(Fault.of, Reason.UNREADABLE, subject)
    return catch(base64.b64decode, exception=ValueError)(text, validate=True).map_error(unreadable).bind(lambda data: catch(decoder.decode, exception=msgspec.DecodeError)(data).map_error(unreadable))


# --- [HASHES]


def content_hash(
    scope: type[object],
    *records: object,
    arrays: tuple[npt.NDArray[np.bool | np.int8 | np.int16 | np.int32 | np.int64 | np.uint8 | np.uint16 | np.uint32 | np.uint64 | np.float16 | np.float32 | np.float64], ...] = (),
    providers: tuple[Provider, ...] = (),
) -> ContentHash:
    """Hashes records, arrays, and providers.

    Args:
        scope: Type naming content domain by its qualified name, so a hash holds across module moves and extension install locations.
        *records: Native values or records with naive or aware datetimes, excluding MessagePack Raw and Ext.
        arrays: Fixed-width real arrays hashed with shape and canonical bytes.
        providers: Modules hashed by name and version.

    Returns:
        Framed BLAKE2b digest.
    """
    hasher = hashlib.blake2b((1 + len(records) + 2 * len(arrays) + len(providers)).to_bytes(4, "big"), digest_size=_DIGEST_SIZE)

    def framed(data: bytes) -> None:
        hasher.update(len(data).to_bytes(8, "big"))
        hasher.update(data)

    encoders: dict[type[object], cbor2.EncoderHook] = {
        memoryview: lambda encoder, value: encoder.encode_bytes(bytes(value)),
        datetime: lambda encoder, value: encoder.encode(msgspec.to_builtins(value)) if value.tzinfo is None else encoder.encode_datetime(value),
    }
    framed(cbor2.dumps((scope.__qualname__, len(records), len(arrays), len(providers))))
    for record in records:
        framed(cbor2.dumps(msgspec.to_builtins(record, builtin_types=(bytes, bytearray, memoryview, datetime), order="deterministic"), canonical=True, encoders=encoders))
    for array in arrays:
        dtype = np.promote_types(array.dtype, np.int64 if np.can_cast(array.dtype, np.int64) else np.uint64).newbyteorder("<")
        framed(cbor2.dumps((dtype.str, array.shape)))
        hasher.update((array.size * dtype.itemsize).to_bytes(8, "big"))
        with np.nditer(array, flags=("external_loop", "buffered", "zerosize_ok"), op_flags=("readonly",), op_dtypes=dtype, order="C") as chunks:
            for chunk in chunks:
                hasher.update(np.ascontiguousarray(chunk))
    for module in providers:
        framed(cbor2.dumps((module.__name__, module.__version__)))
    return hasher.digest()


def rng(content: ContentHash, *key: int) -> np.random.Generator:
    """Seeds independent generator from content hash and key."""
    return np.random.default_rng(np.random.SeedSequence(int.from_bytes(content, "little"), spawn_key=key))


# --- [PATHS]


def mirrored(root: Path, path: Path) -> Path:
    """Returns path under mirror root, drive kept as directory."""
    normal = Path(os.path.abspath(path))
    return root / normal.drive.removesuffix(":").lstrip("\\/") / normal.relative_to(normal.anchor)


def existing(path: Path) -> Path:
    """Returns deepest existing folder on absolute path."""
    return next(folder for folder in (path, *path.parents) if folder.is_dir())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ContentHash",
    "Provider",
    "RecordFile",
    "argued",
    "argument",
    "committed",
    "content_hash",
    "existing",
    "files_copied",
    "folder_made",
    "mirrored",
    "no_clobber_move",
    "read",
    "removed",
    "rng",
]
