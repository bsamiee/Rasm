"""Tinted glyph previews from staged images and tool icon paths."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import importlib.resources
from importlib.resources.abc import Traversable
import math
from pathlib import Path
from typing import ClassVar, TYPE_CHECKING

import bpy
import bpy.utils.previews
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.reports import redrawn
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.ui.resources.theme import Color, color

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Glyph = rna_enums.IconItems | Path

# --- [SERVICES] -------------------------------------------------------------------------


class IconSet(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Preview collection with original pixels and tint per source, brand icon ID, and deferred tint write."""

    collection: bpy.utils.previews.ImagePreviewCollection
    sources: dict[Path, tuple[npt.NDArray[np.float32], Option[Color]]]
    tint: Callable[[Path], Option[Color]]
    brand: Option[int]
    pending: Callable[[], None]

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
    """Staged image glyphs of one extension, inked in regular widget text color by default."""

    tint: Callable[[Path], Option[Color]] = lambda _: Some(ink())
    brand: Option[Path] = Nothing
    held: ClassVar[dict[str, IconSet]] = {}

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Builds full-size previews held in the resources band, and theme color subscriptions rewriting tints."""

        def reinked() -> BlenderResult[Option[str]]:
            """Writes changed tints, redrawing when one changed."""
            return redrawn() if Icons.held[identity.package].reinked() else Ok(Nothing)

        def scheduled() -> BlenderResult[None]:
            """Schedules tint write on theme color change."""
            Icons.held[identity.package].pending()
            return Ok(None)

        @contextmanager
        def loaded() -> Generator[BlenderResult[None]]:
            """Holds previews and deferred tint writer for the scope."""
            with ExitStack() as scope:
                collection = bpy.utils.previews.new()
                scope.callback(bpy.utils.previews.remove, collection)
                pending = scope.enter_context(identity.reports.deferred(reinked))
                sources: dict[Path, tuple[npt.NDArray[np.float32], Option[Color]]] = {}
                for entry in Icons.folder(identity).iterdir():
                    if Path(entry.name).suffix == ".png":
                        source = Path(entry.name).with_suffix(".svg")
                        preview = collection.load(source.as_posix(), str(entry), "IMAGE")
                        pixels = np.empty(math.prod(preview.image_size) * 4, dtype=np.float32)  # type: ignore[call-overload]
                        preview.image_pixels_float.foreach_get(pixels)
                        preview.icon_size = preview.image_size
                        preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
                        sources[source] = (pixels.reshape(-1, 4), Nothing)

                def branded() -> BlenderResult[Option[int]]:
                    """Returns brand glyph preview icon ID, absent fault for a glyph the folder lacks."""
                    return self.brand.map(
                        lambda source: (
                            Option
                            .of_optional(collection.get(source.as_posix()))
                            .map(lambda preview: Some(preview.icon_id))
                            .to_result_with(lambda: (BlenderAbsentFault(owner=str(Icons.folder(identity)), key=source.as_posix()),))
                        )
                    ).default_value(Ok(Nothing))

                def held(brand: Option[int]) -> None:
                    """Holds inked icon set until scope exit."""
                    icons = IconSet(collection=collection, sources=sources, tint=self.tint, brand=brand, pending=pending)
                    icons.reinked()
                    Icons.held[identity.package] = icons
                    scope.callback(Icons.held.pop, identity.package)

                yield branded().map(held)

        return Ok((
            (Band.RESOURCES, loaded()),
            *(Subscription(owner=owner, setting=owner.bl_rna.properties[member], body=scheduled) for owner, member in ((bpy.types.ThemeWidgetColors, "text"), (bpy.types.Theme, "filepath"))),
        ))

    # --- [READING]

    @staticmethod
    def folder(identity: Identity) -> Traversable:
        """Returns staged icon folder."""
        return importlib.resources.files(identity.package) / "icons"

    @classmethod
    def value(cls, identity: Identity, glyph: Glyph) -> BlenderResult[int]:
        """Returns built-in icon value or staged preview icon ID, absent fault for a glyph the host or icon set lacks."""
        match glyph:
            case Path():
                return (
                    Option
                    .of_optional(cls.held.get(identity.package))
                    .bind(lambda icons: Option.of_optional(icons.collection.get(glyph.as_posix())))
                    .map(lambda preview: preview.icon_id)
                    .to_result_with(lambda: (BlenderAbsentFault(owner=str(cls.folder(identity)), key=glyph.as_posix()),))
                )
            case _:
                return (
                    Option
                    .of_optional(bpy.types.UILayout.bl_rna.functions["prop"].parameters["icon"].enum_items.get(glyph))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    .map(lambda item: item.value)
                    .to_result_with(lambda: (BlenderAbsentFault(owner=bpy.types.UILayout.__name__, key=glyph),))
                )

    @classmethod
    def mark(cls, identity: Identity) -> Option[int]:
        """Returns brand glyph icon ID, Nothing without brand or icon set."""
        return Option.of_optional(cls.held.get(identity.package)).bind(lambda icons: icons.brand)

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
