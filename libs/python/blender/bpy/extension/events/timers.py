"""Persistent main-loop timers under registration scope."""

from collections.abc import Callable, Generator
from contextlib import contextmanager

import bpy
from expression import Ok, Option
import msgspec

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderResult

# --- [MODELS] ---------------------------------------------------------------------------


class Timer(msgspec.Struct, frozen=True, kw_only=True):
    """Main-loop work returning next interval, Nothing to stop."""

    body: Callable[[], BlenderResult[Option[float]]]
    first_interval: float = 0.0

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds timer acquisition.

        Args:
            identity: Extension with reports holding callback faults.

        Returns:
            Ok of one timer-band acquisition, idle in background mode.
        """
        body = identity.reports.reported(self.body)

        def ticked() -> float | None:
            """Returns next interval, or `None` to stop timer."""
            return body().bind(lambda interval: interval).to_optional()

        @contextmanager
        def registered() -> Generator[None]:
            """Holds timer until it stops or registration scope closes."""
            bpy.app.timers.register(ticked, first_interval=self.first_interval, persistent=True)
            try:
                yield
            finally:
                if bpy.app.timers.is_registered(ticked):
                    bpy.app.timers.unregister(ticked)

        return Ok(((Band.TIMERS, registered()),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Timer"]
