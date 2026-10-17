"""Native template binding policy and data-block preview rows."""

from collections.abc import Callable, Mapping

import bpy
from expression import Error, Nothing, Ok, Option, Some
from msgspec.structs import FieldInfo, fields

from blender.bpy.commands.calls import ShelfPopup
from blender.bpy.commands.keymaps import Keymap
from blender.bpy.data.rna.descriptors import Native
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.faults import BlenderResult
from blender.bpy.settings import shapes
from blender.bpy.ui.layout.rows import Binding, bound, colored, fits, gridded, iconed, Line, located, realized, refused, Resolved, RowDraw
from blender.bpy.ui.resources.icons import Glyph
from blender.imaging.faults import accumulated
from blender.imaging.parameters import Choice, heading, Rgb

# --- [MODELS] ---------------------------------------------------------------------------


class Wheel(Line, frozen=True, kw_only=True):
    """Grading picker with an optional luma slider and aligned RGB components."""

    color: Binding
    luma: Option[Binding] = Nothing

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the color and luma bindings, drawing the picker while a reached luma source is absent."""

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


class Ranged(Line, frozen=True, kw_only=True):
    """Expanded range choice with the selected member's acquired grading wheel."""

    chooser: FieldInfo
    wheels: Mapping[Choice, Wheel]

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires every declared range in enum order and retains each wheel's condition."""

        def draw(choices: type[Choice], draws: Mapping[Choice, RowDraw]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in located(self.chooser, holder, context).to_list():
                    row = layout.row()
                    row.use_property_split = False
                    row.prop(owner, key, expand=True)
                    draws[choices[getattr(owner, key)]](layout, holder, context)

            return shown

        def ranged(shape: shapes.Shape) -> BlenderResult[RowDraw]:
            match shape:
                case shapes.Enumerated(enum=choices) if issubclass(choices, Choice):
                    offered = tuple(member.name for member in choices if member in self.wheels)
                    return accumulated(
                        realized(self.wheels[member], identity).map(lambda draw: (member, draw)) if member in self.wheels else Error(refused(self.chooser.name, member.name, offered))
                        for member in choices
                    ).map(lambda pairs: draw(choices, dict(pairs)))
                case _:
                    return Error(refused(self.chooser.name, type(shape).__name__, (Choice.__name__,)))

        return shapes.field_shape(self.chooser.type).bind(lambda declared: ranged(declared.shape))


class IconView(Line, frozen=True, kw_only=True):
    """Enum thumbnail browser at the caller's chosen native widget scale."""

    choice: Binding
    scale: float
    show_labels: bool = False
    scale_popup: float = 5.0

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the enum binding and draws native preview IDs at each redraw."""
        return templated(
            self.choice,
            (bpy.types.EnumProperty,),
            (Choice, shapes.Text),
            lambda layout, owner, key, _context: gridded(layout, Some(owner.bl_rna.properties[key].name)).template_icon_view(  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                owner, key, show_labels=self.show_labels, scale=self.scale, scale_popup=self.scale_popup
            ),
        )


class Preview(Line, frozen=True, kw_only=True):
    """Shaded data-block preview or its existing native icon at the caller's scale."""

    block: Binding
    scale: float
    show_buttons: bool = True
    parent: Option[bpy.types.ID] = Nothing
    slot: Option[bpy.types.TextureSlot] = Nothing
    preview_id: str = ""

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the ID binding without creating or writing a preview during drawing."""

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
    """Registered shelf button resolving its package glyph at each redraw."""

    popup: ShelfPopup
    name: Option[str] = Nothing
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the native shelf name and retains draw-time icon refresh."""

        def draw(idname: str) -> RowDraw:
            return lambda layout, _holder, _context: layout.template_asset_shelf_popover(idname, name=self.name.default_value(""), icon_value=iconed(identity, self.icon))

        target = self.popup.shelf
        return (identity.name(Name.SHELF, target) if isinstance(target, str) else Ok(target.bl_rna.identifier)).map(draw)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]


class ImageFormat(Line, frozen=True, kw_only=True):
    """Image encoding settings followed by their native multiview controls."""

    settings: Binding
    color_management: bool = False

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the image format pointer and composes its format and views blocks."""

        def place(layout: bpy.types.UILayout, owner: Native, key: str, context: bpy.types.Context) -> None:
            settings = getattr(owner, key)
            layout.template_image_settings(settings, color_management=self.color_management)
            if Option.of_optional(context.scene).map(lambda scene: scene.render.use_multiview).default_value(False):
                layout.template_image_views(settings)

        return templated(self.settings, (bpy.types.ImageFormatSettings,), (), place)


class Shortcut(Line, frozen=True, kw_only=True):
    """Current user's keymap counterparts rendered as named key caps."""

    keymap: Keymap

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Returns live key caps without capturing replaceable user keymap items."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for _, _, item in self.keymap.counterparts(context):
                layout.template_event_from_keymap_item(item, text=item.name)

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------


def templated(
    binding: Binding, kinds: "tuple[type[bpy.types.bpy_struct[object]], ...]", forms: tuple[type[object], ...], place: Callable[[bpy.types.UILayout, Native, str, bpy.types.Context], None]
) -> BlenderResult[RowDraw]:
    """Admits static or reached native metadata and places the template at each located owner."""

    def draw(held: tuple[Resolved, ...]) -> RowDraw:
        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for owner, key in located(held[0], holder, context).to_list():
                place(layout, owner, key, context)

        return shown

    return bound(((binding, lambda held: fits(held, kinds, forms)),), Ok(draw))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["IconView", "ImageFormat", "Preview", "Ranged", "ShelfPopover", "Shortcut", "Wheel", "templated"]
