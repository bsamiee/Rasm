"""Packaged SVG icons drawn as previews tinted in the text color of the widget or region holding them, their quarter-turned variants, data-block icons, and built-in icon values."""

from collections.abc import Callable, Generator, Hashable, Mapping
from contextlib import contextmanager, nullcontext
import importlib.resources
from importlib.resources.abc import Traversable
from pathlib import Path
from types import MappingProxyType
from typing import Final, Self, TYPE_CHECKING

import bpy
import bpy.utils.previews
from expression import Error, NOTHING, Ok, Option, option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from rasm.extension.data._lifecycle.identity import Identity
from rasm.extension.data._lifecycle.registration import Acquisitions, Declaration, Resource, Tier
from rasm.extension.data._rna.paths import rna_definition
from rasm.extension.ui._resources.previews import filled, PREVIEWS
from rasm.extension.ui._resources.theme import Color, display_color, Metrics, Theme
from rasm.records import accumulated, Fault, Outcome, Reason, typed

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Icon = rna_enums.IconItems | Path | bpy.types.ID
type IconValue = Callable[[Hashable, Color], int]
type Widget = Callable[[bpy.types.ThemeUserInterface], bpy.types.ThemeWidgetColors]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ICON: Final = typed(bpy.types.EnumProperty, bpy.types.UILayout, rna_definition(bpy.types.UILayout).functions["prop"].parameters["icon"])

# --- [SERVICES] -------------------------------------------------------------------------


class _IconSet:
    """Coverage of one extension's packaged icons, rows bottom first, and one tinted preview per icon and place with the color it holds."""

    __slots__ = ("collection", "coverage", "tints")

    def __init__(self, collection: bpy.utils.previews.ImagePreviewCollection, coverage: dict[Path, npt.NDArray[np.float32]]) -> None:
        """Holds the collection and icon coverage with no tinted preview."""
        self.collection, self.coverage = collection, coverage
        self.tints: dict[str, Color] = {}

    @classmethod
    def loaded(cls, identity: Identity) -> Self:
        """Reads the alpha of every packaged SVG icon at the size the host rasterizes it, dropping each untinted preview."""
        collection = PREVIEWS[identity].collection

        def alpha(entry: Traversable) -> tuple[Path, npt.NDArray[np.float32]]:
            icon = Path(entry.name)
            preview = collection.load(icon.as_posix(), str(entry), "IMAGE")
            width, height = preview.image_size[0], preview.image_size[1]
            pixels = np.empty(width * height * 4, dtype=np.float32)
            preview.image_pixels_float.foreach_get(pixels)
            del collection[icon.as_posix()]
            return icon, pixels.reshape(height, width, 4)[..., 3]

        return cls(collection=collection, coverage=dict(alpha(entry) for entry in Icons.folder(identity).iterdir() if Path(entry.name).suffix == ".svg"))

    def turned(self, identity: Identity, variants: Mapping[Path, Path]) -> Outcome[None]:
        """Adds each variant as its master turned a quarter clockwise, the host's bottom-first rows turning a quarter counterclockwise, absent fault for a master the folder lacks."""
        missing = tuple(master for master in variants.values() if master not in self.coverage)
        self.coverage.update({variant: np.ascontiguousarray(np.rot90(self.coverage[master])) for variant, master in variants.items()} if not missing else {})
        return accumulated(Error(Fault.of(Reason.ABSENT, Icons.folder(identity), member=master)) for master in missing).map(lambda _: None)

    def tinted(self, source: Path, place: Hashable, color: Color) -> int:
        """Returns the icon ID of the source's preview at the place, its pixels rewritten as coverage times color, premultiplied as the host draws previews, while the place held another color."""
        name = repr((source.as_posix(), place))
        preview = self.collection.get(name) or self.collection.new(name)
        if self.tints.get(name) != color:
            coverage = self.coverage[source]
            filled(preview, (coverage[..., None] * np.asarray((*color[:3], 1.0), dtype=np.float32)).reshape(-1), (coverage.shape[1], coverage.shape[0]))
            self.tints[name] = color
        return preview.icon_id


class Icons(msgspec.Struct, frozen=True, kw_only=True):
    """Packaged SVG icons of one extension and quarter-turned variants keyed by the name rows state, each drawn in the text color of what holds it."""

    turned: Mapping[Path, Path] = MappingProxyType({})

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Builds the icon set, then adds the turned variants in the resources tier."""

        @contextmanager
        def turning() -> Generator[Outcome[None]]:
            """Adds the turned variants."""
            yield _ICONS[identity].turned(identity, self.turned)

        return (_ICONS, (Tier.RESOURCES, turning()))

    # --- [READS]

    @staticmethod
    def folder(identity: Identity) -> Traversable:
        """Returns packaged icon folder."""
        return importlib.resources.files(identity.package) / "icons"

    @staticmethod
    def declarations(icon: Option[Icon]) -> tuple[Declaration, ...]:
        """Icon set a packaged icon reads, none for a stock icon or data-block preview."""
        return tuple(_ICONS for _ in icon.filter(lambda held: isinstance(held, Path)).to_list())

    @staticmethod
    def value(identity: Identity, icon: Option[Icon]) -> Outcome[Option[IconValue]]:
        """Returns the icon value at a place and text color: a packaged icon's preview tinted there, a data-block's preview icon, or a built-in icon's value, absent fault for an icon the host or the packaged set lacks."""
        stated: Option[Path | bpy.types.ID | str] = icon
        match stated:
            case Some(Path() as source):
                return (
                    _ICONS
                    .get(identity)
                    .filter(lambda icons: source in icons.coverage)
                    .to_result_with(lambda: Fault.of(Reason.ABSENT, Icons.folder(identity), member=source))
                    .map(lambda icons: Some(lambda place, color: icons.tinted(source, place, color)))
                )
            case Some(bpy.types.ID() as block):
                return Ok(Some(lambda _place, _color: bpy.types.UILayout.icon(block)))
            case Some(str(name)):
                return (
                    _ICON
                    .bind(lambda prop: option.of_optional(prop.enum_items.get(name)).to_result_with(lambda: Fault.of(Reason.ABSENT, bpy.types.UILayout, member=name)))
                    .map(lambda item: item.value)
                    .map(lambda value: Some(lambda _place, _color: value))
                )
            case _:
                return Ok(NOTHING)

    @staticmethod
    def tint(emboss: str, area: bpy.types.Area | None, region: bpy.types.Region | None, widget: Option[Widget], *, selected: bool) -> tuple[Hashable, Color]:
        """Returns the place and text color of a button the layout of an `emboss` draws in an editor region, selected text while selected.

        Pull-down and pie menus draw every button as their item widget, other embossed buttons draw as their own `widget`, and labels and unembossed buttons take their region's text, the regular widget's outside a region.
        """
        preferences = Metrics.read().preferences
        ui = preferences.themes[0].user_interface
        match emboss, widget, area, region:
            case "PULLDOWN_MENU", _, _, _:
                colors = ui.wcol_menu_item
            case "PIE_MENU", _, _, _:
                colors = ui.wcol_pie_menu
            case "NORMAL", Some(own), _, _:
                colors = own(ui)
            case _, _, bpy.types.Area(type=space), bpy.types.Region(type=kind):
                theme = Theme(space=space, region=kind, preferences=preferences)
                return (space, kind, selected), theme.text_hi if selected else theme.text
            case _:
                colors = ui.wcol_regular
        return (colors.as_pointer(), selected), display_color(colors.text_sel if selected else colors.text)


# --- [COMPOSITION] ----------------------------------------------------------------------

_ICONS: Final = Resource[_IconSet](made=lambda identity: nullcontext(_IconSet.loaded(identity)), prerequisites=(PREVIEWS,))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Icon", "IconValue", "Icons", "Widget"]
