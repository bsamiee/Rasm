"""Rows drawn through UILayout templates over located data: previews, preview icon galleries and the preset gallery applying a listing pick, image format settings, offscreen pictures, key caps, and the grading wheel."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager
from functools import cached_property, partial
from types import MappingProxyType
from typing import override

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands import operators
from blender.bpy.commands.keymaps import Keymap
from blender.bpy.data.rna.paths import id_data, Struct
from blender.bpy.data.rna.properties import Callbacks, held_choices, IconedItem, offered
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Tier
from blender.bpy.extension.reports import alerted, redrawn
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.drawing.canvas import Canvas, Offscreen, PROGRAMS
from blender.bpy.ui.drawing.handlers import Part, Refresh, refreshing
from blender.bpy.ui.layout.members import LocatedMembers, Member
from blender.bpy.ui.layout.rows import accepted, ConditionalRow, FUNCTIONS, gridded, Keywords, Layout, layout_call, Property, RowDraw
from blender.bpy.ui.layout.values import Listing, StoreCommand
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
    def shown(self, function: bpy.types.Function, keywords: Keywords, held: LocatedMembers, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the located data-block's shaded preview or icon."""
        match getattr(*held[0][1]):
            case bpy.types.Material() | bpy.types.Texture() | bpy.types.World() | bpy.types.Light() | bpy.types.FreestyleLineStyle():
                super().shown(function, keywords, held, layout, _holder, context)
            case bpy.types.ID() as data:
                for preview in Option.of_optional(data.preview_ensure()).to_list():
                    layout_call(layout, FUNCTIONS["template_icon"], preview.icon_id, **self.plain)
            case _:
                pass


class ImageFormat(Property, frozen=True, kw_only=True):
    """Image format settings through `template_image_settings`, followed by view settings while the owning scene renders multiview, `keywords` its parameters after the settings."""

    function: bpy.types.Function = FUNCTIONS["template_image_settings"]

    @override
    def shown(self, function: bpy.types.Function, keywords: Keywords, held: LocatedMembers, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the located settings and their views under a multiview owner scene."""
        super().shown(function, keywords, held, layout, _holder, context)
        settings: bpy.types.ImageFormatSettings = getattr(*held[0][1])
        if id_data(bpy.types.Scene, settings).filter(lambda scene: scene.render.use_multiview).is_some():
            layout.template_image_views(settings)


class Gallery(Property, frozen=True, kw_only=True):
    """Enum of preview icons drawn through `template_icon_view` in the value cell beside the enum's own name, `keywords` its parameters after the enum, the host labelling each item by default."""

    function: bpy.types.Function = FUNCTIONS["template_icon_view"]
    keywords: Keywords = MappingProxyType({"show_labels": True})

    @override
    def shown(self, function: bpy.types.Function, keywords: Keywords, held: LocatedMembers, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the located enum's icons in the value cell under its name in the label cell."""
        super().shown(function, keywords, held, gridded(held[0][0].descriptor.prop.name, layout), _holder, context)


class _Pick(bpy.types.PropertyGroup):
    """Preset a gallery shows by its stem, each gallery declaring the enum of its own presets in a subclass."""

    picked: bpy.props.StringProperty(name="Picked", description="Stem of the preset the gallery shows", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    preset: str


class PresetGallery[T: msgspec.Struct](ConditionalRow, frozen=True, kw_only=True, dict=True):
    """Listing presets as a gallery of preview icons beside the preset kind's name, a pick applying its preset through the listing as one undo step under the apply command's title.

    `tile` returns a preset's preview icon over the drawing context, the stock icon while it has none.
    """

    listing: Listing[T]
    tile: Callable[[Identity, bpy.types.Context | None, T], Option[int]]

    @cached_property
    def attachment(self) -> Attachment[_Pick]:
        """Window manager pick of the shown preset, its items the listing's presets, read through the picked stem, and each pick queueing its apply."""
        key = self.listing.preset.key
        named = bpy.path.display_name(key)
        form: type[_Pick] = type(
            f"Pick_{key}", (_Pick,), {"__doc__": f"{named} preset the gallery shows", "__annotations__": {"preset": bpy.props.EnumProperty(name=named, description="Preset to apply", items=())}}
        )
        return Attachment(key=f"{key}_pick", form=form, owner=bpy.types.WindowManager, callbacks=Some(self.picking))

    def presets(self, identity: Identity, context: bpy.types.Context | None) -> tuple[IconedItem, ...]:
        """Item rows of the presets that load, each under its preview icon over the context."""
        preset = self.listing.preset
        return held_choices(
            tuple(
                (stem, name, description, self.tile(identity, context, value).default_value(0), number)
                for stem, name, description, _, number in preset.entries(identity)
                for value in preset.loaded(identity, stem).to_option().to_list()
            )
        )

    def pick(self, identity: Identity, owner: Struct) -> int:
        """Returns the gallery value of the picked preset, the first row's while the pick names none of them."""
        rows, picked = offered(self.presets(identity, None)), Option.of_optional(try_downcast(_Pick, owner)).map(lambda pick: pick.picked)
        return next((row[4] for row in rows if picked == Some(row[0])), rows[0][4])

    @contextmanager
    def picking(self, identity: Identity) -> Generator[Mapping[str, Callbacks]]:
        """Yields the pick's item, read, and write callbacks while the deferred apply each pick queues stands."""
        title = self.listing.commands[StoreCommand.APPLY].title

        def applied(stem: str) -> Outcome[Option[str]]:
            return self.listing.preset.loaded(identity, stem).bind(partial(self.listing.applied, bpy.context)).map(lambda answer: Some(title) if isinstance(answer, operators.Done) else Nothing)

        def picked(owner: Struct, value: int) -> None:
            for pick, row in ((pick, row) for pick in Option.of_optional(try_downcast(_Pick, owner)).to_list() for row in self.presets(identity, None) if row[4] == value):
                pick.picked = row[0]
                deferred(row[0])

        with identity.reports.deferred(applied) as deferred:
            yield {"preset": Callbacks(items=lambda _owner, context: offered(self.presets(identity, context)), get=partial(self.pick, identity), set=picked)}

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the pick."""
        return Ok((self.attachment,))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the gallery over the pick."""
        return Gallery(target=Member(root=self.attachment, path=("preset",))).drawn(identity)


class Picture[S](ConditionalRow, frozen=True, kw_only=True):
    """Part drawn offscreen at its size into a generated preview, shown as an icon and drawn again after each refresh or interface scale change."""

    slot: Key
    part: Part[S]
    size: Callable[[Metrics], tuple[int, int]]
    scale: float
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Owns the preview collection, draw programs, and refresh sources, each refresh dropping the finished picture and redrawing."""

        def refreshed() -> Outcome[None]:
            return identity.resources[PREVIEWS].dropped(self.slot).bind(lambda _: redrawn()).map(lambda _: None)

        return Ok((PREVIEWS, PROGRAMS, (Tier.DRAW_HANDLERS, refreshing(identity, self.refreshes, refreshed))))

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
            canvas, previews = Canvas.of(area, region), identity.resources[PREVIEWS]
            key = (canvas.metrics.scale, canvas.metrics.pixel_size)
            pictured = Ok(previews.icon_id(self.slot)) if previews.current(self.slot, key) else self.rendered(canvas).bind(partial(previews.written, self.slot, key))
            alerted(layout, pictured)
            for value in pictured.to_option().bind(lambda icon: icon).to_list():
                layout.template_icon(icon_value=value, scale=self.scale)


class Shortcut(ConditionalRow, frozen=True, kw_only=True):
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
    """Grading wheel: the color picker in the value cell, the color's swatch, then the luma slider, `picker` the `template_color_picker` parameters."""
    return Layout(
        rows=(
            Layout(container=Some(partial(gridded, "")), rows=(Property(target=color, function=FUNCTIONS["template_color_picker"], keywords=picker),)),
            Property(target=color),
            *(Property(target=held, keywords={"slider": True}) for held in luma.to_list()),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Gallery", "ImageFormat", "Picture", "PresetGallery", "Preview", "Shortcut", "wheel"]
