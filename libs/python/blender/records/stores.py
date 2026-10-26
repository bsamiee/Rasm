"""Atomic file commits and typed JSON files of one record."""

from collections.abc import Callable
from pathlib import Path
import tempfile

from atomicwrites import move_atomic, replace_atomic
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.records.faults import Fault, Faults, Outcome, RecordReason

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
        refused: Callable[[OSError | msgspec.DecodeError], Faults] = lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=str(self.path), cause=error),)
        return (
            catch(self.path.read_bytes, exception=OSError)()
            .map_error(refused)
            .bind(lambda data: catch(lambda: msgspec.json.decode(data, type=self.form), exception=msgspec.DecodeError)().map_error(refused))
        )

    def written(self, value: T, *, exclusive: bool = False) -> Outcome[Path]:
        """Writes the record in sorted key order through one atomic commit.

        Args:
            value: Record to hold.
            exclusive: Refuse a path that already holds a file.

        Returns:
            Ok(path), write fault for a refused write, `FileExistsError` when exclusive meets a held path.
        """
        encoded = msgspec.json.encode(value, order="deterministic")
        return committed(
            self.path,
            lambda stage: catch(stage.write_bytes, exception=OSError)(encoded).map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(self.path), cause=error),)),
            exclusive=exclusive,
        )

    def revised(self, change: Callable[[T], T]) -> Outcome[T]:
        """Writes change of the held record.

        Args:
            change: Pure update of the decoded record.

        Returns:
            Ok(changed record), read or write fault.
        """
        return self.read().map(change).bind(lambda value: self.written(value).map(lambda _: value))


# --- [OPERATIONS] -----------------------------------------------------------------------


def committed(path: Path, write: Callable[[Path], Outcome[object]], *, exclusive: bool = False) -> Outcome[Path]:
    """Writes `path` whole through a stage beside it, replacing it in one atomic step.

    Args:
        path: File or folder to replace, its parent folder created.
        write: Writer of the stage, a path named as `path` in a temporary sibling folder removed on every path.
        exclusive: Refuse a path that already holds an entry.

    Returns:
        Ok(path), writer faults, or write fault for folder creation or replacement, `FileExistsError` when exclusive meets a held path.
    """

    def refused(error: OSError) -> Faults:
        return (Fault(reason=RecordReason.UNWRITABLE, subject=str(path), cause=error),)

    def replaced(folder: tempfile.TemporaryDirectory[str]) -> Outcome[Path]:
        with folder as name:
            stage = Path(name) / path.name
            return write(stage).bind(lambda _: catch(move_atomic if exclusive else replace_atomic, exception=OSError)(str(stage), str(path)).map_error(refused)).map(lambda _: path)

    return (
        catch(path.parent.mkdir, exception=OSError)(parents=True, exist_ok=True)
        .bind(lambda _: catch(tempfile.TemporaryDirectory[str], exception=OSError)(dir=path.parent))
        .map_error(refused)
        .bind(replaced)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RecordFile", "committed"]
