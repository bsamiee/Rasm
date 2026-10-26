"""Msgbus subscriptions under one owner per extension, renewed after every file load."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from typing import Final, Self

import bpy
from expression import Ok, Some
import msgspec

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band, Registrable
from blender.bpy.extension.scoped import Scoped
from blender.records.faults import Outcome

# --- [MODELS] ---------------------------------------------------------------------------


class Subscription(msgspec.Struct, frozen=True, kw_only=True):
    """Msgbus callback on one owner type property."""

    owner: "type[bpy.types.bpy_struct[object]]"
    setting: bpy.types.Property
    body: Callable[[], Outcome[None]]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Extension bus the subscription holds its key on."""
        return (BUS,)

    def acquisitions(self, identity: Identity, /) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the subscription on its extension's bus.

        Args:
            identity: Extension with reports holding notification faults.

        Returns:
            Ok of one subscription-band acquisition.
        """
        return Ok(((Band.SUBSCRIPTIONS, self.subscribed(identity)),))

    @contextmanager
    def subscribed(self, identity: Identity) -> Generator[None]:
        """Holds key on the extension's bus until scope exit.

        Args:
            identity: Extension with the bus and reports holding notification faults.

        Yields:
            None while the bus holds the key.
        """
        bus, body = BUS.of(identity), identity.reports.reported(self.body)

        def notified() -> None:
            """Runs body through reports."""
            return body().to_optional()

        held = ((self.owner, self.setting.identifier), notified)
        bus.keys.append(held)
        bpy.msgbus.subscribe_rna(key=held[0], owner=bus, args=(), notify=notified)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        try:
            yield
        finally:
            bus.keys.remove(held)


# --- [SERVICES] -------------------------------------------------------------------------


class Bus:
    """Msgbus owner of one extension with its keys, each subscribed again after every file load."""

    __slots__ = ("keys",)

    def __init__(self) -> None:
        """Holds no key."""
        self.keys: list[tuple[tuple[type[bpy.types.bpy_struct[object]], str], Callable[[], None]]] = []

    @classmethod
    @contextmanager
    def opened(cls, _: Identity) -> Generator[Self]:
        """Holds a new bus, clearing its subscriptions at exit."""
        bus = cls()
        try:
            yield bus
        finally:
            bpy.msgbus.clear_by_owner(bus)

    def renewed(self) -> Self:
        """Subscribes every held key again, clearing earlier subscriptions first."""
        bpy.msgbus.clear_by_owner(self)
        for key, notify in self.keys:
            bpy.msgbus.subscribe_rna(key=key, owner=self, args=(), notify=notify)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        return self


BUS: Final = Scoped[Bus](made=Bus.opened, renewed=Some(Bus.renewed))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BUS", "Bus", "Subscription"]
