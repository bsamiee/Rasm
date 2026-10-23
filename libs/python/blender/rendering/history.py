"""Bounded history of keyed entries, newest first."""

import datetime
from typing import Annotated, Self

from expression import Option, Some
import msgspec

from blender.imaging.hashing import ContentHash
from blender.imaging.parameters import Presentation, Quantity

# --- [MODELS] ---------------------------------------------------------------------------


class Retention(msgspec.Struct, frozen=True, kw_only=True):
    """History capacity and merge interval."""

    capacity: Annotated[int, msgspec.Meta(ge=1, le=100, description="Entries kept, newest first")] = 10
    interval: Annotated[
        float, msgspec.Meta(ge=0.0, le=600.0, title="Merge Interval", description="Seconds within which next entry replaces newest"), Presentation(quantity=Some(Quantity.DURATION))
    ] = 5.0


class Entry[T](msgspec.Struct, frozen=True, kw_only=True):
    """Value keyed by content hash with add moment."""

    key: ContentHash
    moment: Annotated[datetime.datetime, msgspec.Meta(tz=True)]
    value: T


class History[T](msgspec.Struct, frozen=True, kw_only=True):
    """Entries newest first with evicted and replaced counts."""

    entries: tuple[Entry[T], ...] = ()
    shed: Annotated[int, msgspec.Meta(ge=0)] = 0
    lost: Annotated[int, msgspec.Meta(ge=0)] = 0

    def add(self, entry: Entry[T], retention: Retention) -> Self:
        """Adds entry unless key is held, replacing newest within merge interval."""
        origin = datetime.datetime.min.replace(tzinfo=datetime.UTC)
        match self.entries:
            case _ if any(present.key == entry.key for present in self.entries):
                return self
            case (head, *rest) if 0 <= ((entry.moment - origin) - (head.moment - origin)).total_seconds() < retention.interval:
                held, lost = (entry, *rest), self.lost + 1
            case _:
                held, lost = (entry, *self.entries), self.lost
        kept = held[: retention.capacity]
        return msgspec.structs.replace(self, entries=kept, shed=self.shed + len(held) - len(kept), lost=lost)

    def remove(self, key: ContentHash) -> Self:
        """Removes key, counts unchanged."""
        return msgspec.structs.replace(self, entries=tuple(entry for entry in self.entries if entry.key != key))

    def step(self, key: ContentHash, offset: int) -> Option[Entry[T]]:
        """Returns entry offset from held key, Nothing past either end."""
        return (
            Option
            .of_optional(next((index for index, entry in enumerate(self.entries) if entry.key == key), None))
            .map(lambda index: index + offset)
            .filter(lambda index: 0 <= index < len(self.entries))
            .map(self.entries.__getitem__)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Entry", "History", "Retention"]
