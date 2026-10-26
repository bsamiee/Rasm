"""Typed JSON record files written atomically."""

from collections.abc import Callable
from pathlib import Path

from atomicwrites import atomic_write
from expression import Error, Ok
import msgspec
from typing_extensions import TypeForm

from blender.imaging.faults import Access, ImagingFileFault, ImagingResult

# --- [SERVICES] -------------------------------------------------------------------------


class Record[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """JSON file holding one record of its form."""

    path: Path
    form: TypeForm[T]

    def read(self) -> ImagingResult[T]:
        """Decodes and validates the record.

        Returns:
            Ok(record), read fault for a missing, unreadable, or invalid file.
        """
        try:
            return Ok(msgspec.json.decode(self.path.read_bytes(), type=self.form))
        except (OSError, msgspec.DecodeError) as error:
            return Error((ImagingFileFault(access=Access.READ, request=self.path, error=error),))

    def written(self, value: T, *, exclusive: bool = False) -> ImagingResult[Path]:
        """Writes the record in sorted key order through one atomic commit, creating its folder.

        Args:
            value: Record to hold.
            exclusive: Refuse a path that already holds a file.

        Returns:
            Ok(path), write fault for a refused write, `FileExistsError` when exclusive meets a held path.
        """
        try:
            self.path.parent.mkdir(parents=True, exist_ok=True)
            with atomic_write(self.path, mode="wb", overwrite=not exclusive) as file:
                file.write(msgspec.json.encode(value, order="deterministic"))
        except OSError as error:
            return Error((ImagingFileFault(access=Access.WRITE, request=self.path, error=error),))
        return Ok(self.path)

    def revised(self, change: Callable[[T], T]) -> ImagingResult[T]:
        """Writes change of the held record.

        Args:
            change: Pure update of the decoded record.

        Returns:
            Ok(changed record), read or write fault.
        """
        return self.read().map(change).bind(lambda value: self.written(value).map(lambda _: value))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Record"]
