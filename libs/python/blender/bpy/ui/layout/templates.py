"""Rows drawn through UILayout templates over located data: previews, image format settings, offscreen pictures, key caps, and the grading wheel."""

from collections.abc import Callable
from functools import partial
from types import MappingProxyType
from typing import override

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.keymaps import Keymap
from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.reports import alerted, redrawn
from blender.bpy.ui.drawing.canvas import Canvas, Offscreen, PROGRAMS
from blender.bpy.ui.drawing.handlers import Part, Refresh, refreshing
from blender.bpy.ui.layout.rows import accepted, At, FUNCTIONS, gridded, Keywords, Layout, Line, Member, placed, Property, ResolvedMember, RowDraw
from blender.bpy.ui.resources.previews import PREVIEWS
from blender.bpy.ui.resources.theme import Metrics
from blender.records.faults import Outcome, paired

# --- [MODELS] ---------------------------------------------------------------------------


class Preview(Property, frozen=True, kw_only=True):
    """Data-block pointer drawn as the shaded preview `template_preview` renders for materials, textures, worlds, lights, and line styles, else as the ID's own preview image.

    `keywords` hold `template_preview` parameters after the ID, `plain` the `template_icon` parameters after the icon.
    """

    function: bpy.types.Function = FUNCTIONS["template_preview"]
    plain: Keywords = MappingProxyType({})

    @override
    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Binds the pointer as a `template_preview` row and checks the icon parameters, accumulating both refusals."""
        return paired(accepted(FUNCTIONS["template_icon"], self.plain), super().drawn(identity)).map(lambda parts: parts[1])

    @override
    def shown(self, function: bpy.types.Function, keywords: Keywords, held: tuple[tuple[ResolvedMember, At], ...], layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the located data-block's shaded preview or icon."""
        match getattr(*held[0][1]):
            case bpy.types.Material() | bpy.types.Texture() | bpy.types.World() | bpy.types.Light() | bpy.types.FreestyleLineStyle():
                super().shown(function, keywords, held, layout, _holder, context)
            case bpy.types.ID() as data:
                for preview in Option.of_optional(data.preview_ensure()).to_list():
                    placed(layout, FUNCTIONS["template_icon"], preview.icon_id, **self.plain)
            case _:
                pass


class ImageFormat(Property, frozen=True, kw_only=True):
    """Image format settings through `template_image_settings`, followed by view settings while the owning scene renders multiview, `keywords` its parameters after the settings."""

    function: bpy.types.Function = FUNCTIONS["template_image_settings"]

    @override
    def shown(self, function: bpy.types.Function, keywords: Keywords, held: tuple[tuple[ResolvedMember, At], ...], layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the located settings and their views under a multiview owner scene."""
        super().shown(function, keywords, held, layout, _holder, context)
        settings: bpy.types.ImageFormatSettings = getattr(*held[0][1])
        if Option.of_optional(try_downcast(bpy.types.Scene, settings.id_data)).filter(lambda scene: scene.render.use_multiview).is_some():
            layout.template_image_views(settings)


class Picture[S](Line, frozen=True, kw_only=True):
    """Part drawn offscreen at its size into a generated preview, shown as an icon and drawn again after each refresh or interface scale change."""

    slot: Key
    part: Part[S]
    size: Callable[[Metrics], tuple[int, int]]
    scale: float
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Owns the preview collection, draw programs, and refresh sources, each refresh dropping the finished picture and redrawing."""

        def refreshed() -> Outcome[None]:
            return identity.held[PREVIEWS].dropped(self.slot).bind(lambda _: redrawn()).map(lambda _: None)

        return Ok((PREVIEWS, PROGRAMS, (Band.DRAW_HANDLERS, refreshing(identity, self.refreshes, refreshed))))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the draw rendering the part once per refresh and interface scale."""
        return Ok(partial(self.shown, identity))

    def rendered(self, canvas: Canvas) -> Outcome[Option[npt.NDArray[np.float32]]]:
        """Renders the part's current state at its size in the canvas's metrics and theme, Nothing while the part holds no state."""
        width, height = self.size(canvas.metrics)
        return self.part.read(canvas.metrics).bind(
            lambda found: found.map(lambda state: Offscreen(width=width, height=height).drawn(canvas, lambda pixels: self.part.drawn(pixels, state)).map(Some)).default_value(Ok(Nothing))
        )

    def shown(self, identity: Identity, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the finished picture, rendering it first while the slot misses this interface scale."""
        for area, region in ((area, region) for area in Option.of_optional(context.area).to_list() for region in Option.of_optional(context.region).to_list()):
            canvas, previews = Canvas.of(area, region), identity.held[PREVIEWS]
            key = (canvas.metrics.scale, canvas.metrics.pixel_size)
            pictured = Ok(previews.icon_id(self.slot)) if previews.current(self.slot, key) else self.rendered(canvas).bind(partial(previews.written, self.slot, key))
            alerted(layout, pictured)
            for value in pictured.to_option().bind(lambda icon: icon).to_list():
                layout.template_icon(icon_value=value, scale=self.scale)


class Shortcut(Line, frozen=True, kw_only=True):
    """User keymap items of a keymap as key caps."""

    keymap: Keymap

    def drawn(self, _identity: Identity, /) -> Outcome[RowDraw]:
        """Returns key caps reading user keymap items each draw."""
        return Ok(self.shown)

    def shown(self, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws one key cap per user keymap item under the operator name the host translates."""
        for _, item in self.keymap.counterparts(context):
            layout.template_event_from_keymap_item(item, text=item.name, translate=False)


# --- [OPERATIONS] -----------------------------------------------------------------------


def wheel(color: Member, luma: Option[Member] = Nothing, picker: Keywords = MappingProxyType({})) -> Layout:
    """Grading wheel: the color picker in the value cell, the luma slider, then the color's components in one aligned column, `picker` the `template_color_picker` parameters."""
    return Layout(
        rows=(
            Layout(container=Some(partial(gridded, "")), rows=(Property(target=color, function=FUNCTIONS["template_color_picker"], keywords=picker),)),
            *(Property(target=held, keywords={"slider": True}) for held in luma.to_list()),
            Layout(container=Some(lambda layout: layout.column(align=True)), rows=(Property(target=color, keywords={"expand": True}),)),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ImageFormat", "Picture", "Preview", "Shortcut", "wheel"]
