"""Per-extension values held from registration to release."""

from collections.abc import Callable, Generator
from contextlib import AbstractContextManager, contextmanager
from pathlib import Path

from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderResult

# --- [SERVICES] -------------------------------------------------------------------------


class Scoped[T](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Value each extension holds while its registration scope stands, optionally renewed after every file load.

    Rows reading the value list the cell among their prerequisites, and scope builds it once per extension.
    """

    made: Callable[[Identity], AbstractContextManager[T]]
    renewed: Option[Callable[[T], T]] = Nothing
    held: dict[str, T] = msgspec.field(default_factory=dict)

    def of(self, identity: Identity) -> T:
        """Returns the value extension holds.

        Args:
            identity: Extension whose registration acquired the cell.

        Returns:
            Held value.
        """
        return self.held[identity.package]

    def acquisitions(self, identity: Identity, /) -> BlenderResult[Acquisitions]:
        """Builds the resource acquisition, with a `LOAD_POST` renewal when the cell renews.

        Args:
            identity: Extension keying the held value.

        Returns:
            Ok of one resource-band acquisition and an optional load handler.
        """

        def renewal(renew: Callable[[T], T]) -> PathHandler:
            def renewed(_: Option[Path]) -> BlenderResult[None]:
                self.held[identity.package] = renew(self.of(identity))
                return Ok(None)

            return PathHandler(handlers=LOAD_POST, body=renewed)

        return Ok(((Band.RESOURCES, self.holding(identity)), *self.renewed.map(renewal).to_list()))

    @contextmanager
    def holding(self, identity: Identity) -> Generator[None]:
        """Holds the made value under extension's package until scope exit.

        Args:
            identity: Extension keying the held value.

        Yields:
            None while the cell holds the value.
        """
        with self.made(identity) as value:
            self.held[identity.package] = value
            try:
                yield
            finally:
                self.held.pop(identity.package)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Scoped"]
