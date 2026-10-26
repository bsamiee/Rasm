"""Persistent main-loop timers and startup work under registration scope."""

from collections.abc import Callable, Generator
from contextlib import contextmanager

import bpy
from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Acquisitions, Band
from blender.bpy.faults import BlenderResult

# --- [MODELS] ---------------------------------------------------------------------------


class Timer(msgspec.Struct, frozen=True, kw_only=True):
    """Main-loop work returning next interval, Nothing to stop."""

    body: Callable[[], BlenderResult[Option[float]]]
    first_interval: float = 0.0

    def acquisitions(self, identity: Identity, /) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
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


class Startup(msgspec.Struct, frozen=True, kw_only=True):
    """Work run on the first main-loop pass after registration and after every file load."""

    body: Callable[[], BlenderResult[None]]

    def acquisitions(self, _identity: Identity, /) -> BlenderResult[Acquisitions]:
        """Builds the load handler and one-shot timer running body.

        Args:
            _identity: Extension with reports holding body faults.

        Returns:
            Ok of a `LOAD_POST` path handler and a one-shot timer, the timer idle in background mode.
        """
        return Ok((PathHandler(handlers=LOAD_POST, body=lambda _: self.body()), Timer(body=lambda: self.body().map(lambda _: Nothing))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Startup", "Timer"]
