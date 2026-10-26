"""Rows drawn through UILayout templates."""

from collections.abc import Callable, Mapping
from contextlib import nullcontext
from typing import Final, override

import bpy
from expression import Error, Nothing, Ok, Option, Some
from msgspec import UNSET, UnsetType
from msgspec.structs import fields
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.keymaps import Keymap
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import redrawn
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderResult
from blender.bpy.settings import shapes
from blender.bpy.ui.drawing.canvas import PROGRAMS
from blender.bpy.ui.drawing.handlers import Part, Refresh, refreshing
from blender.bpy.ui.drawing.offscreen import Offscreen
from blender.bpy.ui.layout.rows import alerted, At, Binding, Bound, bound, FUNCTIONS, gridded, Line, Member, realized, refused, ResolvedMember, RowContent, RowDraw
from blender.bpy.ui.resources.previews import PREVIEWS
from blender.bpy.ui.resources.theme import Metrics, Theme
from blender.imaging.faults import accumulated
from blender.imaging.parameters import heading, Rgb

# --- [MODELS] ---------------------------------------------------------------------------


class Wheel(Line, frozen=True, kw_only=True):
    """Grading color picker with luma slider and RGB components."""

    color: Binding
    luma: Option[Binding] = Nothing

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds color and luma, skipping absent luma source."""

        def colored(held: ResolvedMember) -> BlenderResult[ResolvedMember]:
            match held.descriptor.prop, held.shape.to_optional():
                case (bpy.types.FloatProperty(is_array=True, subtype="COLOR" | "COLOR_GAMMA"), _) | (_, shapes.Color()):
                    return Ok(held)
                case prop, _:
                    return Error(refused(held.descriptor.path, prop.subtype, ("COLOR", "COLOR_GAMMA")))

        def slider(parts: tuple[ResolvedMember, ...]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for source, member in parts[0].at(holder, context).to_list():
                    layout.prop(source, member, slider=True)

            return shown

        def draw(sliders: tuple[RowDraw, ...]) -> Callable[[tuple[ResolvedMember, ...]], RowDraw]:
            def place(parts: tuple[ResolvedMember, ...]) -> RowDraw:
                def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    for owner, key in parts[0].at(holder, context).to_list():
                        gridded(layout, Some("")).template_color_picker(owner, key, value_slider=True, lock_luminosity=True, cubic=True)
                        for each in sliders:
                            each(layout, holder, context)
                        components = layout.column(align=True)
                        for index, field in enumerate(fields(Rgb)):
                            components.prop(owner, key, index=index, text=heading(field.name))

                return shown

            return place

        return accumulated(bound(((binding, lambda held: held.accepts((bpy.types.FloatProperty,))),), slider) for binding in self.luma.to_list()).bind(
            lambda sliders: bound(((self.color, colored),), draw(sliders))
        )


class Pages(Line, frozen=True, kw_only=True):
    """Choice drawn as tab strip or segmented control over the rows of the chosen page, one page per choice identifier."""

    chooser: Member
    pages: Mapping[str, RowContent]
    tabs: bool = False

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes every page once, refusing a choice the pages miss or a page no choice names."""

        def draw(held: ResolvedMember, draws: Mapping[str, RowDraw]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in held.at(holder, context).to_list():
                    row = layout.row()
                    row.use_property_split = False
                    if self.tabs:
                        row.prop_tabs_enum(owner, key, expand_as="ROW")
                    else:
                        row.prop(owner, key, expand=True)
                    draws[getattr(owner, key)](layout, holder, context)

            return shown

        def paged(held: ResolvedMember[bpy.types.EnumProperty]) -> BlenderResult[RowDraw]:
            path, names = held.descriptor.path, tuple(item.identifier for item in held.descriptor.prop.enum_items)
            return accumulated((
                *(realized(self.pages[choice], identity).map(lambda each: (choice, each)) if choice in self.pages else Error(refused(path, choice, tuple(self.pages))) for choice in names),
                *(Error(refused(path, page, names)) for page in self.pages if page not in names),
            )).map(lambda pairs: draw(held, dict(pairs)))

        return self.chooser.resolved(bpy.types.EnumProperty).bind(paged)


class Curve(Bound, frozen=True, kw_only=True):
    """Native curve editor over a curve mapping a host struct owns, channel tabs by `type`."""

    function = FUNCTIONS["template_curve_mapping"]
    kinds = (bpy.types.CurveMapping,)
    type: str | UnsetType = UNSET
    levels: bool | UnsetType = UNSET
    use_negative_slope: bool | UnsetType = UNSET
    show_tone: bool | UnsetType = UNSET
    show_presets: bool | UnsetType = UNSET


class ColorRamp(Bound, frozen=True, kw_only=True):
    """Native color ramp editor over a ramp a host struct owns, expanded unless declared otherwise."""

    function = FUNCTIONS["template_color_ramp"]
    kinds = (bpy.types.ColorRamp,)
    expand: bool | UnsetType = True


class Histogram(Bound, frozen=True, kw_only=True):
    """Native histogram over editor scope data."""

    function = FUNCTIONS["template_histogram"]
    kinds = (bpy.types.Histogram, bpy.types.Scopes)


class Waveform(Bound, frozen=True, kw_only=True):
    """Native waveform over editor scope data."""

    function = FUNCTIONS["template_waveform"]
    kinds = (bpy.types.Scopes,)


class Vectorscope(Bound, frozen=True, kw_only=True):
    """Native vectorscope over editor scope data."""

    function = FUNCTIONS["template_vectorscope"]
    kinds = (bpy.types.Scopes,)


class LightLinking(Bound, frozen=True, kw_only=True):
    """Native light linking collection view, the drawing layout taking its active element as context."""

    function = FUNCTIONS["template_light_linking_collection"]
    kinds = (bpy.types.Collection,)

    @override
    def placed(self, layout: bpy.types.UILayout, at: At, _context: bpy.types.Context, options: dict[str, object], /) -> None:
        """Draws the collection view with layout as its context layout."""
        self.called(layout, layout, *at, **options)


class IconView(Bound, frozen=True, kw_only=True):
    """Enum thumbnail picker on the property grid under the enum's name."""

    function = FUNCTIONS["template_icon_view"]
    kinds = (bpy.types.EnumProperty,)
    scale: float | UnsetType = UNSET
    show_labels: bool | UnsetType = UNSET
    scale_popup: float | UnsetType = UNSET

    @override
    def placed(self, layout: bpy.types.UILayout, at: At, _context: bpy.types.Context, options: dict[str, object], /) -> None:
        """Draws the picker beside the enum's name."""
        owner, key = at
        self.called(gridded(layout, Some(owner.bl_rna.properties[key].name)), owner, key, **options)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


class Preview(Bound, frozen=True, kw_only=True):
    """Shaded data-block preview, or the preview icon of an ID without shaded previews."""

    function = FUNCTIONS["template_preview"]
    kinds = (bpy.types.ID,)
    scale: float | UnsetType = UNSET
    show_buttons: bool | UnsetType = UNSET
    preview_id: str | UnsetType = UNSET
    parent: Option[bpy.types.ID] = Nothing
    slot: Option[bpy.types.TextureSlot] = Nothing

    @override
    def values(self, _identity: Identity, /) -> BlenderResult[dict[str, object]]:
        """Passes declared parent and texture slot."""
        return Ok({key: held for key, value in (("parent", self.parent), ("slot", self.slot)) for held in value.to_list()})

    @override
    def placed(self, layout: bpy.types.UILayout, at: At, _context: bpy.types.Context, options: dict[str, object], /) -> None:
        """Draws the shaded preview or the ID's icon, writing no preview."""
        match getattr(*at):
            case bpy.types.Material() | bpy.types.Texture() | bpy.types.World() | bpy.types.Light() | bpy.types.FreestyleLineStyle() as shaded:
                self.called(layout, shaded, **options)
            case bpy.types.ID(preview=bpy.types.ImagePreview(icon_id=icon)):
                layout.template_icon(icon_value=icon, **({} if isinstance(self.scale, UnsetType) else {"scale": self.scale}))
            case _:
                pass


class ImageFormat(Bound, frozen=True, kw_only=True):
    """Image encoding settings followed by multiview controls."""

    function = FUNCTIONS["template_image_settings"]
    kinds = (bpy.types.ImageFormatSettings,)
    color_management: bool | UnsetType = UNSET

    @override
    def placed(self, layout: bpy.types.UILayout, at: At, context: bpy.types.Context, options: dict[str, object], /) -> None:
        """Draws the format, adding view settings under multiview."""
        settings = getattr(*at)
        self.called(layout, settings, **options)
        if Option.of_optional(context.scene).map(lambda scene: scene.render.use_multiview).default_value(False):
            layout.template_image_views(settings)


class Picture[S](Line, frozen=True, kw_only=True):
    """Part drawn offscreen at its size into a generated preview, shown as an icon and redrawn on each refresh."""

    slot: Key
    part: Part[S]
    size: Callable[[Metrics], tuple[int, int]]
    scale: float
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Owns the preview collection, generation counts, and refresh sources, each refresh renewing the drawing."""

        def refreshed() -> BlenderResult[None]:
            generations = GENERATIONS.of(identity)
            generations[self.slot] = generations.get(self.slot, 0) + 1
            return redrawn().map(lambda _: None)

        return refreshing(identity, self.refreshes, refreshed).map(lambda rows: (PREVIEWS, GENERATIONS, PROGRAMS, *rows))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns draw rendering the part once per refresh and interface scale."""

        def rendered(theme: Theme, metrics: Metrics) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
            width, height = self.size(metrics)
            return self.part.read(metrics).bind(
                lambda found: found.map(lambda state: Offscreen(width=width, height=height).drawn(theme, metrics, lambda canvas: self.part.drawn(canvas, state)).map(Some)).default_value(Ok(Nothing))
            )

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for area, region in ((area, region) for area in Option.of_optional(context.area).to_list() for region in Option.of_optional(context.region).to_list()):
                theme = Theme(space=area.type, region=region.type, preferences=Metrics.read().preferences)
                key = GENERATIONS.of(identity).get(self.slot, 0)
                pictured = PREVIEWS.of(identity).picture(self.slot, key, lambda metrics: rendered(theme, metrics))
                alerted(layout, pictured)
                for value in pictured.to_option().bind(lambda icon: icon).to_list():
                    layout.template_icon(icon_value=value, scale=self.scale)

        return Ok(shown)


class Shortcut(Line, frozen=True, kw_only=True):
    """User keymap items as key caps."""

    keymap: Keymap

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Returns key caps reading user keymap items each draw."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for _, item in self.keymap.counterparts(context):
                layout.template_event_from_keymap_item(item, text=item.name)

        return Ok(shown)


# --- [COMPOSITION] ----------------------------------------------------------------------

GENERATIONS: Final = Scoped[dict[Key, int]](made=lambda _identity: nullcontext({}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GENERATIONS", "ColorRamp", "Curve", "Histogram", "IconView", "ImageFormat", "LightLinking", "Pages", "Picture", "Preview", "Shortcut", "Vectorscope", "Waveform", "Wheel"]
