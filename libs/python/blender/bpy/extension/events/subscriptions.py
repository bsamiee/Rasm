"""Msgbus subscriptions renewed per file load."""

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
    """Msgbus callback on one owner type property."""

    owner: "type[bpy.types.bpy_struct[object]]"
    setting: bpy.types.Property
    body: Callable[[], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds subscription and load renewal acquisitions.

        Args:
            identity: Extension with reports holding notification faults.

        Returns:
            Ok of app-handler renewal and subscription-band acquisitions.
        """
        body = identity.reports.reported(self.body)

        def notified() -> None:
            """Runs body through reports."""
            return body().to_optional()

        subscribe = partial(
            bpy.msgbus.subscribe_rna,
            key=(self.owner, self.setting.identifier),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            owner=notified,
            args=(),
            notify=notified,
        )

        def renewed(_: Option[Path]) -> BlenderResult[None]:
            """Subscribes again after file load clears subscribers."""
            return Ok(subscribe())

        @contextmanager
        def held() -> Generator[None]:
            """Subscribes once, clearing notification owner at release."""
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
