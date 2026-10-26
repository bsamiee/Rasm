"""Darkroom look tiles: the native looks the formation's view offers and the layer stack presets, each pick tiled over the render proxy and written on change, gathered in the Image Formation panel."""

from collections.abc import Generator
from contextlib import contextmanager
from typing import Annotated, Final

from bl_ui.properties_render import RENDER_PT_color_management
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.data.ids.store import Handle
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.changes import owned
from blender.bpy.render.color.spaces import NO_LOOK, viewed
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.properties import Changed, Choices, EnumRows
from blender.bpy.ui.layout.rows import Caption, Flow, Member, OperatorButton, Property, Rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.templates import IconView
from blender.bpy.ui.surfaces.panels import Child, Panel
from blender.imaging.color.processors import DisplayView
from blender.records.faults import paired
from blender.records.fields import Gated, heading

from ..catalog import stacks
from ..chain.previews import Tiles
from ..effects.formation.display import Formation, formation, looked
from .films import install
from .luts import tones

# --- [MODELS] ---------------------------------------------------------------------------


class FormationLook(msgspec.Struct, frozen=True, kw_only=True):
    """Look the formation's view applies, one of the OCIO looks Blender offers it."""

    look: Annotated[str, msgspec.Meta(title="Look", description="Look the formation's view applies ahead of its transform")] = NO_LOOK


class StackLook(msgspec.Struct, frozen=True, kw_only=True):
    """Layer stack preset the head stack takes, its effects moved from their defaults by Strength."""

    look: Annotated[str, msgspec.Meta(title="Stack Look", description="Layer stack preset the head stack takes")] = ""
    strength: Annotated[int, msgspec.Meta(ge=0, le=200, title="Strength", description="Share of each preset effect's change from its defaults, in percent")] = 100


# --- [SERVICES] -------------------------------------------------------------------------


class LookTiles[S: FormationLook | StackLook](msgspec.Struct, frozen=True, kw_only=True):
    """Scene look pick drawn as tiles over the render proxy, its further fields beneath."""

    attachment: Attachment[S]

    @classmethod
    def formation(cls) -> "LookTiles[FormationLook]":
        """Binds the scene pick to the looks Blender offers the formation's view, read live from the formation and written after each pick."""

        def offered(identity: Identity, owner: "bpy.types.bpy_struct[object]", _context: bpy.types.Context | None) -> EnumRows:
            config = PyOpenColorIO.GetCurrentConfig()
            return tuple(
                (
                    name,
                    label,
                    Option.of_optional(config.getLook(name)).map(PyOpenColorIO.Look.getDescription).default_value(""),
                    Tiles.icon(identity, f"look {name}", Formation(display_view=msgspec.structs.replace(view, look=Gated(enabled=name != NO_LOOK, value=name)))).default_value(0),
                    index,
                )
                for scene in scened(owner).to_list()
                for settings, view in shown(identity, scene).to_list()
                for index, name in enumerate((NO_LOOK, *config.getLookNames()))
                for label in (bpy.types.UILayout.enum_item_name(settings, "look", name),)
                if label
            )

        @contextmanager
        def picked(identity: Identity) -> Generator[Changed]:
            def write(handle: Handle[bpy.types.Scene], look: str) -> BlenderResult[Option[str]]:
                return handle.resolve().bind(lambda scene: looked(identity, scene, look)).map(lambda _: Some(f"Look {look}"))

            with identity.reports.deferred(write) as deferred:
                yield lambda owner, _context, prop: scened(owner).map(lambda scene: deferred(Handle.of(scene), getattr(owner, prop.identifier))).default_value(None)

        choices = Choices(offered, current=Some(lambda identity, owner: scened(owner).bind(lambda scene: shown(identity, scene)).map(lambda held: held[0].look).default_value(NO_LOOK)))
        return LookTiles(attachment=Attachment(key="looks", form=FormationLook, owner=bpy.types.Scene, changed=Some(picked), bindings=Some({vars(FormationLook)["look"]: choices})))

    @classmethod
    def stacked(cls) -> "LookTiles[StackLook]":
        """Binds the scene pick to the layer stack presets, each pick or Strength change writing the picked preset over the head stack."""
        preset = stacks.preset

        def tile(identity: Identity, stem: str) -> int:
            return preset.found(identity, stem).bind(preset.read).to_option().bind(lambda record: Tiles.icon(identity, f"stack look {stem}", record)).default_value(0)

        def offered(identity: Identity, _owner: "bpy.types.bpy_struct[object]", _context: bpy.types.Context | None) -> EnumRows:
            return ((stem, name, tip, tile(identity, stem), index) for index, (stem, name, tip) in enumerate(preset.entries(identity)))

        @contextmanager
        def picked(identity: Identity) -> Generator[Changed]:
            def applied(scene: bpy.types.Scene, pick: StackLook) -> BlenderResult[Option[str]]:
                match pick:
                    case StackLook(look=""):
                        return Ok(Nothing)
                    case StackLook(look=look, strength=strength):
                        record = preset.found(identity, look).bind(preset.read)
                        changed = paired(record, stacks.bound(identity)).bind(lambda parts: parts[1].looked(scene, parts[0], strength, bpy.context))
                        return changed.map(lambda wrote: Some(f"Look {heading(look)}") if wrote else Nothing)

            def write(handle: Handle[bpy.types.Scene]) -> BlenderResult[Option[str]]:
                return handle.resolve().bind(lambda scene: attachment.read(identity, scene).bind(lambda pick: applied(scene, pick)))

            with identity.reports.deferred(write) as deferred:
                yield lambda owner, _context, _prop: scened(owner).map(lambda scene: deferred(Handle.of(scene))).default_value(None)

        attachment = Attachment(key="stack_looks", form=StackLook, owner=bpy.types.Scene, changed=Some(picked), bindings=Some({vars(StackLook)["look"]: Choices(offered)}))
        return LookTiles(attachment=attachment)

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Scene pick the tiles bind."""
        return (self.attachment,)

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Declares nothing beside the scene pick."""
        return Ok(())

    @property
    def row(self) -> Rows:
        """Tiles over the pick's look, then the pick's further fields as sliders."""
        look, *rest = self.attachment.form.__struct_fields__
        return Rows(
            rows=lambda identity: self.attachment.bound(identity).map(
                lambda attached: (
                    IconView(target=Member(root=bpy.types.Scene, path=f"{attached.name}.{look}"), show_labels=True),
                    *(Property(target=Member(root=bpy.types.Scene, path=f"{attached.name}.{field}"), slider=True) for field in rest),
                )
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def scened(owner: "bpy.types.bpy_struct[object]") -> Option[bpy.types.Scene]:
    """Returns the scene owning a pick."""
    match owner.id_data:
        case bpy.types.Scene() as scene:
            return Some(scene)
        case _:
            return Nothing


def shown(identity: Identity, scene: bpy.types.Scene) -> Option[tuple[bpy.types.ColorManagedViewSettings, DisplayView]]:
    """Returns the view settings and view of the earliest formation's display node, else of the scene, `Nothing` while the holder has no view settings."""
    holder = next(
        (
            node
            for entry in formation.entries(identity, scene)
            for group in Option.of_optional(entry.node_group).to_list()
            for node in Option.of_optional(owned(group, identity).get("display")).to_list()
            if isinstance(node, bpy.types.CompositorNodeConvertToDisplay)
        ),
        scene,
    )
    return Option.of_optional(holder.view_settings).map(lambda settings: (settings, viewed(holder)))


# --- [COMPOSITION] ----------------------------------------------------------------------

formation_looks: Final = LookTiles.formation()
stack_looks: Final = LookTiles.stacked()
rows: Final = (
    formation_looks,
    stack_looks,
    Panel(
        section=Single(
            key="image_formation",
            title=heading("image_formation"),
            rows=(formation_looks.row, stack_looks.row, Caption(rows=(OperatorButton(target=install.installing), OperatorButton(target=tones.toning)), flow=Flow.ALIGNED, separator=Nothing)),
        ),
        at=Child(parent=RENDER_PT_color_management),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FormationLook", "LookTiles", "StackLook", "rows"]
