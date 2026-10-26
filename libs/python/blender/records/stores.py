"""Atomic file commits and typed JSON files of one record."""

from collections.abc import Callable
from pathlib import Path
import tempfile

from atomicwrites import move_atomic, replace_atomic
from expression import Error, Ok, Result
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.records.faults import Faults, FileFault, RecordResult, scoped

# --- [SERVICES] -------------------------------------------------------------------------


class RecordFile[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """JSON file holding one record of its form."""

    path: Path
    form: TypeForm[T]

    def read(self) -> RecordResult[T]:
        """Decodes and validates the record.

        Returns:
            Ok(record), read fault for a missing, unreadable, or invalid file.
        """
        try:
            return Ok(msgspec.json.decode(self.path.read_bytes(), type=self.form))
        except (OSError, msgspec.DecodeError) as error:
            return Error((FileFault(access="read", request=self.path, error=error),))

    def written(self, value: T, *, exclusive: bool = False) -> RecordResult[Path]:
        """Writes the record in sorted key order through one atomic commit.

        Args:
            value: Record to hold.
            exclusive: Refuse a path that already holds a file.

        Returns:
            Ok(path), write fault for a refused write, `FileExistsError` when exclusive meets a held path.
        """
        encoded = msgspec.json.encode(value, order="deterministic")
        return committed(
            self.path, lambda stage: catch(stage.write_bytes, exception=OSError)(encoded).map_error(lambda error: (FileFault(access="write", request=self.path, error=error),)), exclusive=exclusive
        )

    def revised(self, change: Callable[[T], T]) -> RecordResult[T]:
        """Writes change of the held record.

        Args:
            change: Pure update of the decoded record.

        Returns:
            Ok(changed record), read or write fault.
        """
        return self.read().map(change).bind(lambda value: self.written(value).map(lambda _: value))


# --- [OPERATIONS] -----------------------------------------------------------------------


def committed[F](path: Path, write: Callable[[Path], Result[object, Faults[F]]], *, exclusive: bool = False) -> Result[Path, Faults[F | FileFault]]:
    """Writes `path` whole through a stage beside it, replacing it in one atomic step.

    Args:
        path: File or folder to replace, its parent folder created.
        write: Writer of the stage, a path named as `path` in a temporary sibling folder removed on every path.
        exclusive: Refuse a path that already holds an entry.

    Returns:
        Ok(path), writer faults, or write fault for folder creation, replacement, or cleanup, `FileExistsError` when exclusive meets a held path.
    """

    def refused(error: OSError) -> Faults[F | FileFault]:
        return (FileFault(access="write", request=path, error=error),)

    def replaced(stage: Path) -> Result[Path, Faults[F | FileFault]]:
        return (
            Result[object, Faults[F | FileFault]]
            .bind(write(stage), lambda _: catch(move_atomic if exclusive else replace_atomic, exception=OSError)(str(stage), str(path)).map_error(refused))
            .map(lambda _: path)
        )

    return (
        catch(lambda: path.parent.mkdir(parents=True, exist_ok=True), exception=OSError)()
        .map_error(refused)
        .bind(
            lambda _: scoped(
                catch(tempfile.TemporaryDirectory[str], exception=OSError)(dir=path.parent).map_error(refused),
                lambda folder: replaced(Path(folder.name) / path.name),
                lambda folder: catch(folder.cleanup, exception=OSError)().map_error(refused),
            )
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RecordFile", "committed"]
