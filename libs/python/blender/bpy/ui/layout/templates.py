"""Rows drawn through UILayout templates."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager
from itertools import chain
from typing import ClassVar, Literal

import bpy
from expression import Error, Nothing, Ok, Option, Some
from msgspec.structs import FieldInfo, fields
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.calls import ShelfPopup
from blender.bpy.commands.keymaps import Keymap
from blender.bpy.data.rna.descriptors import Native
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.reports import redrawn
from blender.bpy.faults import BlenderResult
from blender.bpy.settings import shapes
from blender.bpy.ui.drawing.canvas import Programs
from blender.bpy.ui.drawing.handlers import followed, Part, Refresh
from blender.bpy.ui.drawing.offscreen import Offscreen
from blender.bpy.ui.layout.rows import alerted, Binding, bound, colored, fits, gridded, iconed, Line, located, Member, realized, refused, Resolved, ResolvedMember, RowContent, RowDraw
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.previews import Previews
from blender.bpy.ui.resources.theme import Metrics, Theme
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Choice, heading, Rgb

# --- [MODELS] ---------------------------------------------------------------------------


class Wheel(Line, frozen=True, kw_only=True):
    """Grading color picker with luma slider and RGB components."""

    color: Binding
    luma: Option[Binding] = Nothing

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds color and luma, skipping absent luma source."""

        def slider(parts: tuple[Resolved, ...]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for source, member in located(parts[0], holder, context).to_list():
                    layout.prop(source, member, slider=True)

            return shown

        def draw(sliders: tuple[RowDraw, ...]) -> Callable[[tuple[Resolved, ...]], RowDraw]:
            def place(parts: tuple[Resolved, ...]) -> RowDraw:
                def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    for owner, key in located(parts[0], holder, context).to_list():
                        gridded(layout, Some("")).template_color_picker(owner, key, value_slider=True, lock_luminosity=True, cubic=True)
                        for each in sliders:
                            each(layout, holder, context)
                        components = layout.column(align=True)
                        for index, field in enumerate(fields(Rgb)):
                            components.prop(owner, key, index=index, text=heading(field.name))

                return shown

            return place

        return bound(
            ((self.color, colored),), accumulated(bound(((binding, lambda held: fits(held, (bpy.types.FloatProperty,), (shapes.Number,))),), Ok(slider)) for binding in self.luma.to_list()).map(draw)
        )


class Pages(Line, frozen=True, kw_only=True):
    """Choice drawn as tab strip or segmented control over the rows of the chosen page, one page per choice identifier."""

    chooser: Member | FieldInfo
    pages: Mapping[str, RowContent]
    tabs: bool = False

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes every page once, refusing a choice the pages miss or a page no choice names."""

        def offered() -> BlenderResult[tuple[Resolved, tuple[str, ...]]]:
            match self.chooser:
                case Member():
                    return self.chooser.resolved(bpy.types.EnumProperty).map(lambda held: (held, tuple(item.identifier for item in held.descriptor.prop.enum_items)))
                case FieldInfo() as field:
                    return shapes.field_shape(field.type).bind(
                        lambda declared: (
                            Ok((field, tuple(member.name for member in declared.shape.enum)))
                            if isinstance(declared.shape, shapes.Enumerated)
                            else Error(refused(field.name, type(declared.shape).__name__, (Choice.__name__,)))
                        )
                    )

        def draw(held: Resolved, draws: Mapping[str, RowDraw]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in located(held, holder, context).to_list():
                    row = layout.row()
                    row.use_property_split = False
                    if self.tabs:
                        row.prop_tabs_enum(owner, key, expand_as="ROW")
                    else:
                        row.prop(owner, key, expand=True)
                    draws[getattr(owner, key)](layout, holder, context)

            return shown

        def paged(found: tuple[Resolved, tuple[str, ...]]) -> BlenderResult[RowDraw]:
            held, names = found
            name = held.descriptor.path if isinstance(held, ResolvedMember) else held.name
            return accumulated((
                *(realized(self.pages[choice], identity).map(lambda each: (choice, each)) if choice in self.pages else Error(refused(name, choice, tuple(self.pages))) for choice in names),
                *(Error(refused(name, page, names)) for page in self.pages if page not in names),
            )).map(lambda pairs: draw(held, dict(pairs)))

        return offered().bind(paged)


class Curve(Line, frozen=True, kw_only=True):
    """Native curve editor over a curve mapping a host struct owns, channel tabs by `type`."""

    mapping: Binding
    type: Literal["NONE", "VECTOR", "COLOR", "HUE"] = "NONE"
    levels: bool = False
    use_negative_slope: bool = False
    show_tone: bool = False
    show_presets: bool = False

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds curve mapping pointer and draws its editor each redraw."""
        return templated(
            self.mapping,
            (bpy.types.CurveMapping,),
            (),
            lambda layout, owner, key, _context: layout.template_curve_mapping(
                owner, key, type=self.type, levels=self.levels, use_negative_slope=self.use_negative_slope, show_tone=self.show_tone, show_presets=self.show_presets
            ),
        )


class ColorRamp(Line, frozen=True, kw_only=True):
    """Native color ramp editor over a ramp a host struct owns."""

    ramp: Binding
    expand: bool = True

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds color ramp pointer and draws its editor each redraw."""
        return templated(self.ramp, (bpy.types.ColorRamp,), (), lambda layout, owner, key, _context: layout.template_color_ramp(owner, key, expand=self.expand))


class Scope(Line, frozen=True, kw_only=True):
    """Native histogram, waveform, or vectorscope over editor scope data."""

    data: Binding
    kind: Literal["HISTOGRAM", "WAVEFORM", "VECTORSCOPE"]

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds scope pointer and draws its widget each redraw."""

        def place(layout: bpy.types.UILayout, owner: Native, key: str, _context: bpy.types.Context) -> None:
            match self.kind:
                case "HISTOGRAM":
                    layout.template_histogram(owner, key)
                case "WAVEFORM":
                    layout.template_waveform(owner, key)
                case "VECTORSCOPE":
                    layout.template_vectorscope(owner, key)

        return templated(self.data, (bpy.types.Histogram, bpy.types.Scopes), (), place)


class Picture[S](Line, frozen=True, kw_only=True):
    """Part drawn offscreen at its size into a generated preview, shown as an icon and redrawn on each refresh."""

    slot: Key
    part: Part[S]
    size: Callable[[Metrics], tuple[int, int]]
    scale: float
    refreshes: tuple[Refresh, ...] = ()
    generations: ClassVar[dict[tuple[str, Key], int]] = {}

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Owns the preview collection and refresh sources, each refresh renewing the drawing."""
        held = (identity.package, self.slot)
        later: Callable[[], None]

        def refreshed() -> BlenderResult[None]:
            Picture.generations[held] = Picture.generations.get(held, 0) + 1
            return redrawn().map(lambda _: None)

        @contextmanager
        def deferred() -> Generator[None]:
            nonlocal later
            try:
                with identity.reports.deferred(lambda: refreshed().map(lambda _: Nothing)) as later:
                    yield None
            finally:
                Picture.generations.pop(held, None)

        def deferring(*_: object) -> BlenderResult[None]:
            later()
            return Ok(None)

        return accumulated(followed(identity, refresh, refreshed, deferring) for refresh in self.refreshes).map(
            lambda rows: ((Band.RESOURCES, Previews(identity=identity)), (Band.RESOURCES, Programs()), (Band.DRAW_HANDLERS, deferred()), *chain.from_iterable(rows))
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns draw rendering the part once per refresh and interface scale."""
        previews = Previews(identity=identity)

        def rendered(theme: Theme, metrics: Metrics) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
            width, height = self.size(metrics)
            return self.part.read(metrics).bind(
                lambda found: found.map(lambda state: Offscreen(width=width, height=height).drawn(theme, metrics, lambda canvas: self.part.drawn(canvas, state)).map(Some)).default_value(Ok(Nothing))
            )

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for area, region in ((area, region) for area in Option.of_optional(context.area).to_list() for region in Option.of_optional(context.region).to_list()):
                theme = Theme(space=area.type, region=region.type, preferences=Metrics.read().preferences)
                key = Picture.generations.get((identity.package, self.slot), 0)
                pictured = previews.picture(self.slot, key, lambda metrics: rendered(theme, metrics))
                alerted(layout, pictured)
                for value in pictured.to_option().bind(lambda icon: icon).to_list():
                    layout.template_icon(icon_value=value, scale=self.scale)

        return Ok(shown)


class IconView(Line, frozen=True, kw_only=True):
    """Enum thumbnail picker at caller widget scale."""

    choice: Binding
    scale: float
    show_labels: bool = False
    scale_popup: float = 5.0

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds enum and draws its preview icons each redraw."""
        return templated(
            self.choice,
            (bpy.types.EnumProperty,),
            (Choice, shapes.Text),
            lambda layout, owner, key, _context: gridded(layout, Some(owner.bl_rna.properties[key].name)).template_icon_view(  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                owner, key, show_labels=self.show_labels, scale=self.scale, scale_popup=self.scale_popup
            ),
        )


class Preview(Line, frozen=True, kw_only=True):
    """Shaded data-block preview or its preview icon."""

    block: Binding
    scale: float
    show_buttons: bool = True
    parent: Option[bpy.types.ID] = Nothing
    slot: Option[bpy.types.TextureSlot] = Nothing
    preview_id: str = ""

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds ID, drawing without creating or writing a preview."""

        def place(layout: bpy.types.UILayout, owner: Native, key: str, _context: bpy.types.Context) -> None:
            match getattr(owner, key):
                case bpy.types.Material() | bpy.types.Texture() | bpy.types.World() | bpy.types.Light() | bpy.types.FreestyleLineStyle() as shaded:
                    layout.template_preview(shaded, show_buttons=self.show_buttons, parent=self.parent.to_optional(), slot=self.slot.to_optional(), preview_id=self.preview_id)
                case bpy.types.ID(preview=bpy.types.ImagePreview(icon_id=icon)):
                    layout.template_icon(icon_value=icon, scale=self.scale)
                case _:
                    pass

        return templated(self.block, (bpy.types.ID,), (shapes.Pointer,), place)


class ShelfPopover(Line, frozen=True, kw_only=True):
    """Asset shelf popover button."""

    popup: ShelfPopup
    name: Option[str] = Nothing
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves shelf idname and glyph once."""

        def draw(parts: tuple[str, int]) -> RowDraw:
            return lambda layout, _holder, _context: layout.template_asset_shelf_popover(parts[0], name=self.name.default_value(""), icon_value=parts[1])

        target = self.popup.shelf
        return paired(identity.name(Name.SHELF, target) if isinstance(target, str) else Ok(target.bl_rna.identifier), iconed(identity, self.icon)).map(draw)


class ImageFormat(Line, frozen=True, kw_only=True):
    """Image encoding settings followed by multiview controls."""

    settings: Binding
    color_management: bool = False

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds image format, adding view settings under multiview."""

        def place(layout: bpy.types.UILayout, owner: Native, key: str, context: bpy.types.Context) -> None:
            settings = getattr(owner, key)
            layout.template_image_settings(settings, color_management=self.color_management)
            if Option.of_optional(context.scene).map(lambda scene: scene.render.use_multiview).default_value(False):
                layout.template_image_views(settings)

        return templated(self.settings, (bpy.types.ImageFormatSettings,), (), place)


class Shortcut(Line, frozen=True, kw_only=True):
    """User keymap items as key caps."""

    keymap: Keymap

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Returns key caps reading user keymap items each draw."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for _, item in self.keymap.counterparts(context):
                layout.template_event_from_keymap_item(item, text=item.name)

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------


def templated(
    binding: Binding, kinds: "tuple[type[bpy.types.bpy_struct[object]], ...]", forms: tuple[type[object], ...], place: Callable[[bpy.types.UILayout, Native, str, bpy.types.Context], None]
) -> BlenderResult[RowDraw]:
    """Validates property metadata and places template at each owner."""

    def draw(held: tuple[Resolved, ...]) -> RowDraw:
        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for owner, key in located(held[0], holder, context).to_list():
                place(layout, owner, key, context)

        return shown

    return bound(((binding, lambda held: fits(held, kinds, forms)),), Ok(draw))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ColorRamp", "Curve", "IconView", "ImageFormat", "Pages", "Picture", "Preview", "Scope", "ShelfPopover", "Shortcut", "Wheel", "templated"]
