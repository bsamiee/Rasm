"""Native setting subscriptions renewed after each Blender file load."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from functools import partial
from pathlib import Path

import bpy
from expression import Ok, Option
import msgspec

from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderResult

# --- [MODELS] ---------------------------------------------------------------------------


class Subscription(msgspec.Struct, frozen=True, kw_only=True):
    """Callback on one resolved native property of an owner type."""

    owner: "type[bpy.types.bpy_struct[object]]"
    setting: bpy.types.Property
    body: Callable[[], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds load renewal followed by the native message-bus acquisition.

        Args:
            identity: Extension whose reports hold notification failures.

        Returns:
            Ok of the app-handler renewal and subscription-band acquisitions.
        """
        body = identity.reports.reported(self.body)

        def notified() -> None:
            """Reports the body while answering the native notification result."""
            return body().default_value(None)

        subscribe = partial(
            bpy.msgbus.subscribe_rna,
            key=(self.owner, self.setting.identifier),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            owner=notified,
            args=(),
            notify=notified,
        )

        def renewed(_: Option[Path]) -> BlenderResult[None]:
            """Renews the subscription after Blender clears subscribers on file load."""
            return Ok(subscribe())

        @contextmanager
        def held() -> Generator[None]:
            """Subscribes once and clears this notification owner at release."""
            subscribe()
            try:
                yield
            finally:
                bpy.msgbus.clear_by_owner(notified)

        return (
            PathHandler(
                handlers=bpy.app.handlers.load_post,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                body=renewed,
            )
            .acquisitions(identity)
            .map(lambda pairs: (*pairs, (Band.SUBSCRIPTIONS, held())))
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Subscription"]
