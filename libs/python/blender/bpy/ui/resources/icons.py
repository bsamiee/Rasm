"""Tinted glyph previews from staged images and tool icon paths."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, nullcontext
import importlib.resources
from importlib.resources.abc import Traversable
import math
from pathlib import Path
from typing import Final, Self, TYPE_CHECKING

import bpy
import bpy.utils.previews
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.reports import redrawn
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.resources.previews import PREVIEWS
from blender.bpy.ui.resources.theme import Color, color
from blender.records.faults import Fault, Faults, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Glyph = rna_enums.IconItems | Path

# --- [SERVICES] -------------------------------------------------------------------------


class IconSet:
    """Staged glyphs of one extension in its preview collection, with original pixels and applied tint per glyph and the brand glyph's icon ID."""

    __slots__ = ("brand", "collection", "sources")

    def __init__(self, collection: bpy.utils.previews.ImagePreviewCollection, sources: dict[Path, tuple[npt.NDArray[np.float32], Option[Color]]]) -> None:
        """Holds the collection and glyph sources with no brand icon."""
        self.collection, self.sources = collection, sources
        self.brand: Option[int] = Nothing

    @classmethod
    def loaded(cls, identity: Identity) -> Self:
        """Loads every staged glyph image into the extension's preview collection at full size."""
        collection = identity.held[PREVIEWS].collection

        def source(entry: Traversable) -> tuple[Path, tuple[npt.NDArray[np.float32], Option[Color]]]:
            glyph = Path(entry.name).with_suffix(".svg")
            preview = collection.load(glyph.as_posix(), str(entry), "IMAGE")
            pixels = np.empty(math.prod(preview.image_size) * 4, dtype=np.float32)  # type: ignore[call-overload]
            preview.image_pixels_float.foreach_get(pixels)
            preview.icon_size = preview.image_size
            preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            return glyph, (pixels.reshape(-1, 4), Nothing)

        return cls(collection=collection, sources=dict(source(entry) for entry in Icons.folder(identity).iterdir() if Path(entry.name).suffix == ".png"))

    def reinked(self, tint: Callable[[Path], Option[Color]]) -> bool:
        """Writes changed tints over source alpha, returning whether one changed."""
        changed = {source: (original, current) for source, (original, applied) in self.sources.items() if applied != (current := tint(source))}
        for source, (original, current) in changed.items():
            preview = self.collection[source.as_posix()]
            pixels = current.map(lambda rgb: np.concatenate((original[:, 3:] * np.asarray(rgb[:3], dtype=np.float32), original[:, 3:]), axis=1)).default_value(original).reshape(-1)
            preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            preview.image_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            self.sources[source] = (original, current)
        return bool(changed)

    def icon(self, identity: Identity, glyph: Path) -> Outcome[int]:
        """Returns staged glyph icon ID, absent fault for a glyph the folder lacks."""
        return (
            Option
            .of_optional(self.collection.get(glyph.as_posix()))
            .map(lambda preview: preview.icon_id)
            .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=str(Icons.folder(identity)), member=glyph.as_posix()),))
        )


class Icons(msgspec.Struct, frozen=True, kw_only=True):
    """Staged image glyphs of one extension, inked in regular widget text color by default."""

    tint: Callable[[Path], Option[Color]] = lambda _: Some(ink())
    brand: Option[Path] = Nothing

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Builds the glyph set, its tints and brand held in the resources band, and theme color subscriptions rewriting tints."""
        later: Callable[[], None]

        def reinked() -> Outcome[Option[str]]:
            """Writes changed tints, redrawing when one changed."""
            return redrawn() if identity.held[ICONS].reinked(self.tint) else Ok(Nothing)

        def scheduled() -> Outcome[None]:
            """Schedules tint write on theme color change."""
            later()
            return Ok(None)

        @contextmanager
        def inked() -> Generator[Outcome[None]]:
            """Inks glyphs and records the brand icon, holding the deferred tint writer for the scope."""
            nonlocal later
            icons = identity.held[ICONS]
            with identity.reports.deferred(reinked) as later:
                icons.reinked(self.tint)
                branded: Outcome[Option[int]] = self.brand.map(lambda glyph: icons.icon(identity, glyph).map(Some)).default_value(Ok(Nothing))
                for brand in branded.to_option().to_list():
                    icons.brand = brand
                yield branded.map(lambda _: None)

        return Ok((
            PREVIEWS,
            ICONS,
            (Band.RESOURCES, inked()),
            *(Subscription(owner=owner, setting=owner.bl_rna.properties[member], body=scheduled) for owner, member in ((bpy.types.ThemeWidgetColors, "text"), (bpy.types.Theme, "filepath"))),
        ))

    # --- [READING]

    @staticmethod
    def folder(identity: Identity) -> Traversable:
        """Returns staged icon folder."""
        return importlib.resources.files(identity.package) / "icons"

    @staticmethod
    def value(identity: Identity, glyph: Glyph) -> Outcome[int]:
        """Returns built-in icon value or staged glyph icon ID, absent fault for a glyph the host or the staged set lacks."""
        match glyph:
            case Path():
                return Result[IconSet, Faults].bind(
                    Option.of_optional(ICONS.held.get(identity.package)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=str(Icons.folder(identity)), member=glyph.as_posix()),)),
                    lambda icons: icons.icon(identity, glyph),
                )
            case _:
                return (
                    Option
                    .of_optional(bpy.types.UILayout.bl_rna.functions["prop"].parameters["icon"].enum_items.get(glyph))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    .map(lambda item: item.value)
                    .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=bpy.types.UILayout.__name__, member=glyph),))
                )

    @staticmethod
    def mark(identity: Identity) -> Option[int]:
        """Returns brand glyph icon ID, Nothing for an extension without staged glyphs or brand."""
        return Option.of_optional(ICONS.held.get(identity.package)).bind(lambda icons: icons.brand)

    @staticmethod
    def path(identity: Identity, source: Path) -> str:
        """Returns suffixless absolute path for tool `bl_icon`."""
        return str(Icons.folder(identity).joinpath(source.with_suffix("").as_posix()))


# --- [OPERATIONS] -----------------------------------------------------------------------


def ink() -> Color:
    """Returns regular widget text color in display RGBA."""
    return color(bpy.context.preferences.themes[0].user_interface.wcol_regular.text)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


# --- [COMPOSITION] ----------------------------------------------------------------------

ICONS: Final = Scoped[IconSet](made=lambda identity: nullcontext(IconSet.loaded(identity)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ICONS", "Glyph", "IconSet", "Icons", "ink"]
