"""Native glyph previews, optional source tint, and workspace-tool icon paths."""

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

from rasm.blender.extension.events.subscriptions import Subscription
from rasm.blender.extension.identity import Identity
from rasm.blender.extension.registration import Acquisition, Band
from rasm.blender.extension.reports import redrawn
from rasm.blender.faults import BlenderResult
from rasm.blender.ui.resources.theme import Color, color
from rasm.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Glyph = rna_enums.IconItems | Path

# --- [SERVICES] -------------------------------------------------------------------------


class IconSet(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Stable native previews with each source's original pixels and applied tint."""

    collection: bpy.utils.previews.ImagePreviewCollection
    sources: dict[Path, tuple[npt.NDArray[np.float32], Option[Color]]]
    tint: Callable[[Path], Option[Color]]
    brand: Option[Path]
    pending: Callable[[], None]

    # --- [READING]

    def refresh(self) -> None:
        """Schedules changed source colors through the shared deferred write."""
        if any(applied != self.tint(source) for source, (_, applied) in self.sources.items()):
            self.pending()

    def value(self, source: Path) -> int:
        """Returns a source's stable preview ID and schedules changed tint."""
        self.refresh()
        return self.collection[source.as_posix()].icon_id

    # --- [WRITING]

    def reinked(self) -> bool:
        """Returns whether changed tints wrote pixels, retaining each source's native alpha."""
        changed = {source: (native, current) for source, (native, applied) in self.sources.items() if applied != (current := self.tint(source))}
        for source, (native, current) in changed.items():
            preview = self.collection[source.as_posix()]
            pixels = current.map(lambda rgb: np.concatenate((native[:, 3:] * np.asarray(rgb[:3], dtype=np.float32), native[:, 3:]), axis=1)).default_value(native).reshape(-1)
            preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            preview.image_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            self.sources[source] = (native, current)
        return bool(changed)


class Icons(msgspec.Struct, frozen=True, kw_only=True):
    """One extension's staged image glyphs and declared optional brand."""

    tint: Callable[[Path], Option[Color]] = lambda _: Nothing
    brand: Option[Path] = Nothing
    held: ClassVar[dict[str, IconSet]] = {}

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds native preview and theme-subscription acquisitions."""
        folder = Icons.folder(identity)

        def reinked() -> BlenderResult[Option[str]]:
            """Updates the held set and redraws only when source colors changed."""
            return redrawn() if Icons.held[identity.package].reinked() else Ok(Nothing)

        pending: Callable[[], None]

        @contextmanager
        def loaded() -> Generator[None]:
            """Holds full-size previews and retires write admission before releasing them."""
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
                icons = IconSet(collection=collection, sources=sources, tint=self.tint, brand=self.brand, pending=pending)
                icons.reinked()
                Icons.held[identity.package] = icons
                scope.callback(Icons.held.pop, identity.package)
                yield

        def scheduled() -> BlenderResult[None]:
            """Admits a scoped write and answers the subscription's successful notification."""
            pending()
            return Ok(None)

        keys = ((bpy.types.ThemeWidgetColors, "text"), (bpy.types.Theme, "filepath"))
        subscriptions = (Subscription(owner=owner, setting=owner.bl_rna.properties[member], body=scheduled) for owner, member in keys)
        return accumulated(row.acquisitions(identity) for row in subscriptions).map(lambda pairs: ((Band.RESOURCES, loaded()), *chain.from_iterable(pairs)))

    # --- [READING]

    @staticmethod
    def folder(identity: Identity) -> Traversable:
        """Returns the enabled extension's staged icon folder."""
        return importlib.resources.files(identity.package) / "icons"

    @classmethod
    def value(cls, identity: Identity, glyph: Glyph) -> int:
        """Returns a native RNA icon value or the declared source preview ID."""
        return cls.held[identity.package].value(glyph) if isinstance(glyph, Path) else bpy.types.UILayout.bl_rna.functions["prop"].parameters["icon"].enum_items[glyph].value  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    @classmethod
    def mark(cls, identity: Identity) -> Option[int]:
        """Returns the declared brand's stable ID, absent without a brand or icon row."""
        return Option.of_optional(cls.held.get(identity.package)).bind(lambda icons: icons.brand.map(icons.value))

    @classmethod
    def path(cls, identity: Identity, source: Path) -> str:
        """Returns the absolute suffixless path Blender's native tool loader consumes."""
        return str(cls.folder(identity).joinpath(source.with_suffix("").as_posix()))


# --- [OPERATIONS] -----------------------------------------------------------------------


def ink() -> Color:
    """Returns Blender's regular-widget text color in display RGBA."""
    return color(bpy.context.preferences.themes[0].user_interface.wcol_regular.text)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Glyph", "IconSet", "Icons", "ink"]
