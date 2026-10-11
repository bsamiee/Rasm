"""Persistent main-loop work held by an extension's registration scope."""

from collections.abc import Callable, Generator
from contextlib import contextmanager

import bpy
from expression import Ok, Option
import msgspec

from rasm.blender.extension.identity import Identity
from rasm.blender.extension.registration import Acquisition, Band
from rasm.blender.faults import BlenderResult

# --- [MODELS] ---------------------------------------------------------------------------


class Timer(msgspec.Struct, frozen=True, kw_only=True):
    """Main-loop work answering its next interval, absent to stop."""

    body: Callable[[], BlenderResult[Option[float]]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds one persistent timer acquisition.

        Args:
            identity: Extension whose reports hold callback failures.

        Returns:
            Ok of one timer-band acquisition, running once inline in background mode.
        """
        body = identity.reports.reported(self.body)

        def ticked() -> float | None:
            """Translates the reported interval into Blender's timer answer."""
            return body().bind(lambda interval: interval).to_optional()

        @contextmanager
        def registered() -> Generator[None]:
            """Holds the timer until it stops or the registration scope closes."""
            if bpy.app.background:
                ticked()
            else:
                bpy.app.timers.register(ticked, persistent=True)
            try:
                yield
            finally:
                if bpy.app.timers.is_registered(ticked):
                    bpy.app.timers.unregister(ticked)

        return Ok(((Band.TIMERS, registered()),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Timer"]
