"""Tinted glyph previews from staged images and tool icon paths."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import importlib.resources
from importlib.resources.abc import Traversable
from itertools import chain
import math
from pathlib import Path
from typing import ClassVar, TYPE_CHECKING

import bpy
import bpy.utils.previews
from expression import Nothing, Ok, Option
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.extension.reports import redrawn
from blender.bpy.faults import BlenderResult
from blender.bpy.ui.resources.theme import Color, color
from blender.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Glyph = rna_enums.IconItems | Path

# --- [SERVICES] -------------------------------------------------------------------------


class IconSet(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Preview collection with original pixels and tint per source."""

    collection: bpy.utils.previews.ImagePreviewCollection
    sources: dict[Path, tuple[npt.NDArray[np.float32], Option[Color]]]
    tint: Callable[[Path], Option[Color]]
    brand: Option[Path]

    # --- [READING]

    def value(self, source: Path) -> int:
        """Returns stable preview icon ID."""
        return self.collection[source.as_posix()].icon_id

    # --- [WRITING]

    def reinked(self) -> bool:
        """Writes changed tints over source alpha, returning whether one changed."""
        changed = {source: (original, current) for source, (original, applied) in self.sources.items() if applied != (current := self.tint(source))}
        for source, (original, current) in changed.items():
            preview = self.collection[source.as_posix()]
            pixels = current.map(lambda rgb: np.concatenate((original[:, 3:] * np.asarray(rgb[:3], dtype=np.float32), original[:, 3:]), axis=1)).default_value(original).reshape(-1)
            preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            preview.image_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            self.sources[source] = (original, current)
        return bool(changed)


class Icons(msgspec.Struct, frozen=True, kw_only=True):
    """Staged image glyphs of one extension."""

    tint: Callable[[Path], Option[Color]] = lambda _: Nothing
    brand: Option[Path] = Nothing
    held: ClassVar[dict[str, IconSet]] = {}

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds preview collection and theme color subscriptions."""
        folder = Icons.folder(identity)

        def reinked() -> BlenderResult[Option[str]]:
            """Writes changed tints, redrawing when one changed."""
            return redrawn() if Icons.held[identity.package].reinked() else Ok(Nothing)

        pending: Callable[[], None]

        @contextmanager
        def loaded() -> Generator[None]:
            """Holds full-size previews, ending tint writes before removal."""
            nonlocal pending
            with ExitStack() as scope, identity.reports.deferred(reinked) as pending:
                collection = bpy.utils.previews.new()
                scope.callback(bpy.utils.previews.remove, collection)
                sources: dict[Path, tuple[npt.NDArray[np.float32], Option[Color]]] = {}
                for entry in folder.iterdir():
                    if Path(entry.name).suffix == ".png":
                        source = Path(entry.name).with_suffix(".svg")
                        preview = collection.load(source.as_posix(), str(entry), "IMAGE")
                        pixels = np.empty(math.prod(preview.image_size) * 4, dtype=np.float32)  # type: ignore[call-overload]
                        preview.image_pixels_float.foreach_get(pixels)
                        preview.icon_size = preview.image_size
                        preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
                        sources[source] = (pixels.reshape(-1, 4), Nothing)
                icons = IconSet(collection=collection, sources=sources, tint=self.tint, brand=self.brand)
                icons.reinked()
                Icons.held[identity.package] = icons
                scope.callback(Icons.held.pop, identity.package)
                yield

        def scheduled() -> BlenderResult[None]:
            """Schedules tint write on theme color change."""
            pending()
            return Ok(None)

        keys = ((bpy.types.ThemeWidgetColors, "text"), (bpy.types.Theme, "filepath"))
        subscriptions = (Subscription(owner=owner, setting=owner.bl_rna.properties[member], body=scheduled) for owner, member in keys)
        return accumulated(row.acquisitions(identity) for row in subscriptions).map(lambda pairs: ((Band.RESOURCES, loaded()), *chain.from_iterable(pairs)))

    # --- [READING]

    @staticmethod
    def folder(identity: Identity) -> Traversable:
        """Returns staged icon folder."""
        return importlib.resources.files(identity.package) / "icons"

    @classmethod
    def value(cls, identity: Identity, glyph: Glyph) -> int:
        """Returns built-in icon value or image preview icon ID."""
        return cls.held[identity.package].value(glyph) if isinstance(glyph, Path) else bpy.types.UILayout.bl_rna.functions["prop"].parameters["icon"].enum_items[glyph].value  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    @classmethod
    def mark(cls, identity: Identity) -> Option[int]:
        """Returns brand glyph icon ID, Nothing without brand or icon set."""
        return Option.of_optional(cls.held.get(identity.package)).bind(lambda icons: icons.brand.map(icons.value))

    @classmethod
    def path(cls, identity: Identity, source: Path) -> str:
        """Returns suffixless absolute path for tool `bl_icon`."""
        return str(cls.folder(identity).joinpath(source.with_suffix("").as_posix()))


# --- [OPERATIONS] -----------------------------------------------------------------------


def ink() -> Color:
    """Returns regular widget text color in display RGBA."""
    return color(bpy.context.preferences.themes[0].user_interface.wcol_regular.text)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Glyph", "IconSet", "Icons", "ink"]
