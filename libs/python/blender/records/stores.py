"""Atomic file and folder commits, folder, file, and removal writes, command argument codecs, mirror paths, and typed JSON files of one record."""

import base64
from collections.abc import Callable, Iterable
from contextlib import ExitStack
from functools import partial
from itertools import starmap
import os
from pathlib import Path
import shutil
import tempfile
from typing import overload

from atomicwrites import move_atomic, replace_atomic
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.records.faults import accumulated, Fault, Outcome, RecordReason

# --- [SERVICES] -------------------------------------------------------------------------


class RecordFile[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """JSON file holding one record of its form."""

    path: Path
    form: TypeForm[T]

    def read(self) -> Outcome[T]:
        """Decodes and validates the record.

        Returns:
            Ok(record), read fault for a missing, unreadable, or invalid file.
        """
        refused = partial(Fault.of, RecordReason.UNREADABLE, str(self.path))
        return catch(self.path.read_bytes, exception=OSError)().map_error(refused).bind(lambda data: catch(msgspec.json.decode, exception=msgspec.DecodeError)(data, type=self.form).map_error(refused))

    def written(self, value: T, *, exclusive: bool = False) -> Outcome[Path]:
        """Writes the record in sorted key order through one atomic commit.

        Args:
            value: Record to hold.
            exclusive: Refuse a path that already holds a file.

        Returns:
            Ok(path), write fault for a refused write, `FileExistsError` when exclusive meets a held path.
        """
        return committed(self.path, msgspec.json.encode(value, order="deterministic"), exclusive=exclusive)

    def revised(self, change: Callable[[T], T]) -> Outcome[T]:
        """Writes change of the held record.

        Args:
            change: Pure update of the decoded record.

        Returns:
            Ok(changed record), read or write fault.
        """
        return self.read().map(change).bind(lambda value: self.written(value).map(lambda _: value))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WRITES]


def folder_made(folder: Path) -> Outcome[Path]:
    """Returns existing or created folder, or write fault."""
    return catch(folder.mkdir, exception=OSError)(parents=True, exist_ok=True).map(lambda _: folder).map_error(partial(Fault.of, RecordReason.UNWRITABLE, str(folder)))


def removed(path: Path) -> Outcome[Path]:
    """Returns path after removing the file it holds, a missing file counting as removed, or write fault."""
    return catch(path.unlink, exception=OSError)(missing_ok=True).map(lambda _: path).map_error(partial(Fault.of, RecordReason.UNWRITABLE, str(path)))


@catch(exception=OSError)
def _swapped(stage: Path, path: Path, prior: Path) -> Path:
    path.replace(prior)
    with ExitStack() as undo:
        undo.callback(prior.replace, path)
        stage.replace(path)
        undo.pop_all()
    return path


@catch(exception=OSError)
def _placed(stage: Path, path: Path, *, exclusive: bool) -> Path:
    (move_atomic if exclusive else replace_atomic)(str(stage), str(path))
    return path


@overload
def committed(path: Path, write: bytes, /, *, exclusive: bool = False) -> Outcome[Path]: ...
@overload
def committed(path: Path, write: Callable[[Path], Outcome[object]], /) -> Outcome[Path]: ...
def committed(path: Path, write: bytes | Callable[[Path], Outcome[object]], /, *, exclusive: bool = False) -> Outcome[Path]:
    """Writes `path` whole through a stage beside it, replacing it in one atomic step.

    Args:
        path: File or folder to replace, its parent folder created.
        write: Bytes the file holds, or writer of the stage, a path named as `path` in a temporary sibling folder removed on every path.
        exclusive: Refuse a path that already holds a file, held by the bytes form alone since the atomic move links files and no folder.

    Returns:
        Ok(path), writer faults, or write fault for folder creation or replacement, `FileExistsError` when exclusive meets a held path. A held folder leaves for the stage folder by two renames, renamed back when the second fails.
    """
    unwritable = partial(Fault.of, RecordReason.UNWRITABLE, str(path))

    def staged(stage: Path) -> Outcome[object]:
        match write:
            case bytes(data):
                return catch(stage.write_bytes, exception=OSError)(data).map_error(unwritable)
            case writer:
                return writer(stage)

    def published(stage: Path, prior: Path) -> Outcome[Path]:
        return (_swapped(stage, path, prior) if stage.is_dir() and path.is_dir() else _placed(stage, path, exclusive=exclusive)).map_error(unwritable)

    def replaced(folder: tempfile.TemporaryDirectory[str]) -> Outcome[Path]:
        with folder as name:
            stage = Path(name) / path.name
            return staged(stage).bind(lambda _: published(stage, Path(name) / f"{path.name}.prior"))

    return folder_made(path.parent).bind(lambda parent: catch(tempfile.TemporaryDirectory[str], exception=OSError)(dir=parent).map_error(unwritable)).bind(replaced)


def files_copied(pairs: Iterable[tuple[Path, Path]]) -> Outcome[tuple[Path, ...]]:
    """Copies distinct pairs, returning targets in path order."""

    def copy(source: Path, target: Path) -> Path:
        return target if target.exists() and target.samefile(source) else Path(shutil.copytree(source, target, dirs_exist_ok=True) if source.is_dir() else shutil.copy2(source, target))

    def one(source: Path, target: Path) -> Outcome[Path]:
        return folder_made(target.parent).bind(lambda _: catch(copy, exception=OSError)(source, target).map_error(partial(Fault.of, RecordReason.UNWRITABLE, str(target))))

    return accumulated(starmap(one, sorted(frozenset(pairs))))


# --- [ARGUMENTS]


def argument(record: msgspec.Struct) -> str:
    """Returns the command argument carrying record, base64 JSON that no shell, worker, or host argument parser rewrites."""
    return base64.b64encode(msgspec.json.encode(record)).decode("ascii")


def argued[T](decoder: msgspec.json.Decoder[T], subject: str, text: str, /) -> Outcome[T]:
    """Returns the record a command argument carries, read fault over subject for text no `argument` wrote."""
    unreadable = partial(Fault.of, RecordReason.UNREADABLE, subject)
    return catch(base64.b64decode, exception=ValueError)(text, validate=True).map_error(unreadable).bind(lambda data: catch(decoder.decode, exception=msgspec.DecodeError)(data).map_error(unreadable))


# --- [PATHS]


def mirrored(root: Path, path: Path) -> Path:
    """Returns path under mirror root, drive kept as directory."""
    normal = Path(os.path.abspath(path))
    return root / normal.drive.removesuffix(":").lstrip("\\/") / normal.relative_to(normal.anchor)


def existing(path: Path) -> Path:
    """Returns deepest existing folder on absolute path."""
    return next(folder for folder in (path, *path.parents) if folder.is_dir())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RecordFile", "argued", "argument", "committed", "existing", "files_copied", "folder_made", "mirrored", "removed"]
