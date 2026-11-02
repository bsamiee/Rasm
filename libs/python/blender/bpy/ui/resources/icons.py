"""Tinted icon previews from packaged SVG images, data-block icons, and tool icon paths."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, nullcontext
from functools import partial
import importlib.resources
from importlib.resources.abc import Traversable
from pathlib import Path
from typing import Final, Self, TYPE_CHECKING

import bpy
import bpy.utils.previews
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.extension.events import Subscription
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions, Tier
from blender.bpy.extension.reports import redrawn
from blender.bpy.ui.resources.previews import PREVIEWS
from blender.bpy.ui.resources.theme import Color, color, Metrics
from blender.records.faults import accumulated, Fault, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Icon = rna_enums.IconItems | Path | bpy.types.ID

# --- [CONSTANTS] ------------------------------------------------------------------------

_ICON: Final[bpy.types.EnumProperty] = definition(bpy.types.UILayout).functions["prop"].parameters["icon"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

# --- [SERVICES] -------------------------------------------------------------------------


class IconSet:
    """Packaged icons of one extension in its preview collection, with original pixels and applied tint per icon and the brand icon ID."""

    __slots__ = ("brand", "collection", "sources")

    def __init__(self, collection: bpy.utils.previews.ImagePreviewCollection, sources: dict[Path, tuple[npt.NDArray[np.float32], Option[Color]]]) -> None:
        """Holds the collection and icon sources with no brand icon."""
        self.collection, self.sources = collection, sources
        self.brand: Option[int] = Nothing

    @classmethod
    def loaded(cls, identity: Identity) -> Self:
        """Loads every packaged SVG icon into the extension's preview collection, its icon buffer at the full rasterized size."""
        collection = identity.resources[PREVIEWS].collection

        def source(entry: Traversable) -> tuple[Path, tuple[npt.NDArray[np.float32], Option[Color]]]:
            icon = Path(entry.name)
            preview = collection.load(icon.as_posix(), str(entry), "IMAGE")
            pixels = np.empty(np.asarray(preview.image_size).prod() * 4, dtype=np.float32)
            preview.image_pixels_float.foreach_get(pixels)
            preview.icon_size = preview.image_size
            preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            return icon, (pixels.reshape(-1, 4), Nothing)

        return cls(collection=collection, sources=dict(source(entry) for entry in Icons.folder(identity).iterdir() if Path(entry.name).suffix == ".svg"))

    def retinted(self, tint: Callable[[Path], Option[Color]]) -> bool:
        """Writes changed tints over source alpha, returning whether one changed."""
        changed = {source: (original, current) for source, (original, applied) in self.sources.items() if applied != (current := tint(source))}
        for source, (original, current) in changed.items():
            preview = self.collection[source.as_posix()]
            pixels = current.map(lambda rgb: np.concatenate((original[:, 3:] * np.asarray(rgb[:3], dtype=np.float32), original[:, 3:]), axis=1)).default_value(original).reshape(-1)
            preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            preview.image_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
            self.sources[source] = (original, current)
        return bool(changed)

    def icon(self, identity: Identity, source: Path) -> Outcome[int]:
        """Returns a packaged icon ID, absent fault for a file the folder lacks."""
        return (
            Option
            .of_optional(self.collection.get(source.as_posix()))
            .map(lambda preview: preview.icon_id)
            .to_result_with(lambda: Fault.of(Reason.ABSENT, str(Icons.folder(identity)), member=source.as_posix()))
        )


class Icons(msgspec.Struct, frozen=True, kw_only=True):
    """Packaged image icons of one extension with an optional brand icon, tinted in regular widget text color by default and tinted again on a change of each `watched` msgbus key the tint reads."""

    tint: Callable[[Path], Option[Color]] = lambda _: Some(color(Metrics.read().preferences.themes[0].user_interface.wcol_regular.text))
    watched: tuple[tuple[type[Struct], str], ...] = ((bpy.types.ThemeWidgetColors, "text"), (bpy.types.Theme, "filepath"))
    brand: Option[Path] = Nothing

    # --- [ACQUISITION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Builds the icon set, its tints and brand held in the resources band, and a subscription per watched key scheduling one deferred tint write, absent fault per key naming a member its type lacks."""

        def retinted() -> Outcome[Option[str]]:
            """Writes changed tints, redrawing when one changed."""
            return redrawn() if identity.resources[ICONS].retinted(self.tint) else Ok(Nothing)

        @contextmanager
        def tinted() -> Generator[Outcome[None]]:
            """Tints icons and records the brand icon."""
            icons = identity.resources[ICONS]
            icons.retinted(self.tint)
            branded: Outcome[Option[int]] = self.brand.map(lambda source: icons.icon(identity, source).map(Some)).default_value(Ok(Nothing))
            for brand in branded.to_option().to_list():
                icons.brand = brand
            yield branded.map(lambda _: None)

        with identity.reports.deferred(retinted) as later:
            subscriptions = accumulated(
                Option
                .of_optional(definition(owner).properties.get(member))
                .map(lambda setting: Subscription(owner=owner, setting=setting, body=lambda: Ok(later())))
                .to_result_with(partial(Fault.of, Reason.ABSENT, owner.__name__, member=member))
                for owner, member in self.watched
            )
            yield subscriptions.map(lambda held: (ICONS, (Tier.RESOURCES, tinted()), *held))

    # --- [READING]

    @staticmethod
    def folder(identity: Identity) -> Traversable:
        """Returns packaged icon folder."""
        return importlib.resources.files(identity.package) / "icons"

    @staticmethod
    def value(identity: Identity, icon: Icon) -> Outcome[int]:
        """Returns built-in icon value, packaged icon ID, or data-block preview icon, absent fault for an icon the host or the packaged set lacks."""
        match icon:
            case Path():
                return identity.resources.get(ICONS).to_result_with(lambda: Fault.of(Reason.ABSENT, str(Icons.folder(identity)), member=icon.as_posix())).bind(lambda icons: icons.icon(identity, icon))
            case bpy.types.ID():
                return Ok(bpy.types.UILayout.icon(icon))
            case _:
                return Option.of_optional(_ICON.enum_items.get(icon)).map(lambda item: item.value).to_result_with(partial(Fault.of, Reason.ABSENT, bpy.types.UILayout.__name__, member=icon))

    @staticmethod
    def path(identity: Identity, source: Path) -> str:
        """Returns suffixless absolute path for tool `bl_icon`."""
        return str(Icons.folder(identity).joinpath(source.with_suffix("").as_posix()))


# --- [COMPOSITION] ----------------------------------------------------------------------

ICONS: Final = Resource[IconSet](made=lambda identity: nullcontext(IconSet.loaded(identity)), prerequisites=(PREVIEWS,))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ICONS", "Icon", "IconSet", "Icons"]
