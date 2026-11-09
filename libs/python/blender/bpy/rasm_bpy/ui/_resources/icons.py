"""Tinted icon previews from packaged SVG images and their quarter-turned variants, data-block icons, and tool icon paths."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, nullcontext
from functools import partial
import importlib.resources
from importlib.resources.abc import Traversable
from pathlib import Path
from types import MappingProxyType
from typing import Final, Self, TYPE_CHECKING

import bpy
import bpy.utils.previews
from expression import Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import _Tier, Acquisitions, Resource
from rasm_bpy.data._extension.reports import _redrawn
from rasm_bpy.data._rna.paths import _definition
from rasm_bpy.ui._drawing.handlers import _refreshing, Refresh
from rasm_bpy.ui._resources.previews import _filled, _PREVIEWS
from rasm_bpy.ui._resources.theme import _color, Color, Metrics
from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Icon = rna_enums.IconItems | Path | bpy.types.ID

# --- [CONSTANTS] ------------------------------------------------------------------------

_ICON: Final[bpy.types.EnumProperty] = _definition(bpy.types.UILayout).functions["prop"].parameters["icon"]  # ty: ignore[invalid-assignment]

# --- [SERVICES] -------------------------------------------------------------------------


class _IconSet:
    """Packaged icons of one extension in its preview collection, with original pixels and applied tint per icon and the brand icon ID."""

    __slots__ = ("brand", "collection", "sources")

    def __init__(self, collection: bpy.utils.previews.ImagePreviewCollection, sources: dict[Path, tuple[npt.NDArray[np.float32], Option[Color]]]) -> None:
        """Holds the collection and icon sources with no brand icon."""
        self.collection, self.sources = collection, sources
        self.brand: Option[int] = Nothing

    @classmethod
    def loaded(cls, identity: Identity) -> Self:
        """Loads every packaged SVG icon into the extension's preview collection, its icon buffer at the full rasterized size."""
        collection = _PREVIEWS[identity].collection

        def source(entry: Traversable) -> tuple[Path, tuple[npt.NDArray[np.float32], Option[Color]]]:
            icon = Path(entry.name)
            preview = collection.load(icon.as_posix(), str(entry), "IMAGE")
            pixels = np.empty(np.asarray(preview.image_size).prod() * 4, dtype=np.float32)
            preview.image_pixels_float.foreach_get(pixels)
            _filled(preview, pixels, preview.image_size)
            return icon, (pixels.reshape(-1, 4), Nothing)

        return cls(collection=collection, sources=dict(source(entry) for entry in Icons.folder(identity).iterdir() if Path(entry.name).suffix == ".svg"))

    def turned(self, identity: Identity, variants: Mapping[Path, Path]) -> Outcome[None]:
        """Adds each variant as its master turned a quarter clockwise, the host's bottom-first rows turning a quarter counterclockwise, absent fault for a master the folder lacks."""
        missing = tuple(master for master in variants.values() if master not in self.sources)
        for variant, master in variants.items() if not missing else ():
            size = self.collection[master.as_posix()].image_size
            pixels = np.ascontiguousarray(np.rot90(self.sources[master][0].reshape(size[1], size[0], 4))).reshape(-1, 4)
            _filled(self.collection.new(variant.as_posix()), pixels.reshape(-1), (size[1], size[0]))
            self.sources[variant] = (pixels, Nothing)
        return accumulated(Error(Fault.of(Reason.ABSENT, str(Icons.folder(identity)), member=master.as_posix())) for master in missing).map(lambda _: None)

    def retinted(self, tint: Callable[[Path], Option[Color]]) -> bool:
        """Writes changed tints over source alpha, returning whether one changed."""
        changed = {source: (original, current) for source, (original, applied) in self.sources.items() if applied != (current := tint(source))}
        for source, (original, current) in changed.items():
            preview = self.collection[source.as_posix()]
            pixels = current.map(lambda rgb: np.concatenate((original[:, 3:] * np.asarray(rgb[:3], dtype=np.float32), original[:, 3:]), axis=1)).default_value(original).reshape(-1)
            _filled(preview, pixels, preview.image_size)
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
    """Packaged image icons of one extension with an optional brand icon and quarter-turned variants keyed by the name rows state, tinted in regular widget text color by default and tinted again after each `watched` source fires."""

    # --- [STATE]

    tint: Callable[[Path], Option[Color]] = lambda _: Some(_color(Metrics.read().preferences.themes[0].user_interface.wcol_regular.text))
    watched: tuple[Refresh, ...] = ((bpy.types.ThemeWidgetColors, "text"), (bpy.types.Theme, "filepath"))
    brand: Option[Path] = Nothing
    turned: Mapping[Path, Path] = MappingProxyType({})

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Builds the icon set, its tints and brand held in the resources tier, and the watched sources each scheduling one deferred tint write."""

        def retinted() -> Outcome[Option[str]]:
            """Writes changed tints, redrawing when one changed."""
            return _redrawn() if _ICONS[identity].retinted(self.tint) else Ok(Nothing)

        @contextmanager
        def tinted() -> Generator[Outcome[None]]:
            """Adds turned variants, tints icons, and records the brand icon."""
            icons = _ICONS[identity]
            turned = icons.turned(identity, self.turned)
            icons.retinted(self.tint)
            branded: Outcome[Option[int]] = self.brand.map(lambda source: icons.icon(identity, source).map(Some)).default_value(Ok(Nothing))
            for brand in branded.to_option().to_list():
                icons.brand = brand
            yield paired(turned, branded).map(lambda _: None)

        with identity.reports.deferred(retinted) as later:
            yield (_ICONS, (_Tier.RESOURCES, tinted()), (_Tier.SUBSCRIPTIONS, _refreshing(identity, self.watched, lambda: Ok(later()))))

    # --- [READS]

    @staticmethod
    def folder(identity: Identity) -> Traversable:
        """Returns packaged icon folder."""
        return importlib.resources.files(identity.package) / "icons"

    @staticmethod
    def value(identity: Identity, icon: Option[Icon]) -> Outcome[Option[int]]:
        """Returns built-in icon value, packaged icon ID, or data-block preview icon of a stated icon, absent fault for an icon the host or the packaged set lacks."""
        stated: Option[Path | bpy.types.ID | str] = icon
        match stated:
            case Option(some=Path() as source):
                return (
                    _ICONS
                    .get(identity)
                    .to_result_with(lambda: Fault.of(Reason.ABSENT, str(Icons.folder(identity)), member=source.as_posix()))
                    .bind(lambda icons: icons.icon(identity, source))
                    .map(Some)
                )
            case Option(some=bpy.types.ID() as block):
                return Ok(Some(bpy.types.UILayout.icon(block)))
            case Option(some=str(name)):
                return (
                    catch(lambda: _ICON.enum_items[name], exception=KeyError)().map(lambda item: Some(item.value)).map_error(partial(Fault.of, Reason.ABSENT, bpy.types.UILayout.__name__, member=name))
                )
            case _:
                return Ok(Nothing)


# --- [COMPOSITION] ----------------------------------------------------------------------

_ICONS: Final = Resource[_IconSet](made=lambda identity: nullcontext(_IconSet.loaded(identity)), prerequisites=(_PREVIEWS,))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Icon", "Icons"]
