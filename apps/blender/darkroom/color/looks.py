"""Darkroom look tiles: the native looks the formation's view offers and the layer stack presets, each pick tiled over the render proxy and written on change, gathered in the Image Formation panel."""

from collections.abc import Generator
from contextlib import contextmanager
import functools
from typing import ClassVar, Final

from bl_ui.properties_render import RENDER_PT_color_management
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.mirrors import Changed
from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.addressing import owned
from blender.bpy.render.color.spaces import NO_LOOK, viewed
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import FUNCTIONS, gridded, Layout, OperatorButton, Property
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.surfaces.panels import Child, Panel
from blender.imaging.color.processors import DisplayView
from blender.records.faults import Outcome, paired

from ..catalog import stacks
from ..chain.previews import Tiles
from ..effects.formation.display import Formation, formation, looked
from .films import install
from .luts import toning

# --- [MODELS] ---------------------------------------------------------------------------


class FormationLook(bpy.types.PropertyGroup):
    """Look the formation's view applies, one of the OCIO looks Blender offers it, read from the formation and written on each pick."""

    look: bpy.props.EnumProperty(name="Look", description="Look the formation's view applies ahead of its transform", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class StackLook(bpy.types.PropertyGroup):
    """Layer stack preset the head stack takes, its effects mixed by Strength."""

    look: bpy.props.EnumProperty(name="Stack Look", description="Layer stack preset the head stack takes", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    strength: bpy.props.IntProperty(name="Strength", description="Share each preset effect mixes over its input, in percent", subtype="PERCENTAGE", min=0, max=200, default=100)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class FormationLooks(msgspec.Struct, frozen=True, kw_only=True):
    """Scene pick over the looks Blender offers the formation's view, tiled over the render proxy."""

    attachment: ClassVar[Attachment[FormationLook]] = Attachment(key="looks", form=FormationLook, owner=bpy.types.Scene)

    @staticmethod
    @contextmanager
    def acquisitions(identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Binds the pick rows, its live read from the formation, and its deferred write, then acquires the pick."""

        def shown(scene: bpy.types.Scene) -> tuple[bpy.types.ColorManagedViewSettings, DisplayView]:
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
            settings: bpy.types.ColorManagedViewSettings = holder.view_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return settings, viewed(holder)

        def offered(owner: Struct, _context: bpy.types.Context | None) -> tuple[tuple[str, str, str, int, int], ...]:
            config = PyOpenColorIO.GetCurrentConfig()
            return _rows(
                tuple(
                    (
                        name,
                        label,
                        Option.of_optional(config.getLook(name)).map(PyOpenColorIO.Look.getDescription).default_value(""),
                        Tiles.icon(
                            identity, f"look {name}", Valued(structure=Entry(record=Formation(display_view=msgspec.structs.replace(view, looks_override=name, looks_override_enabled=name != NO_LOOK))))
                        ).default_value(0),
                        index,
                    )
                    for scene in scened(owner).to_list()
                    for settings, view in (shown(scene),)
                    for index, name in enumerate((NO_LOOK, *config.getLookNames()))
                    for label in (bpy.types.UILayout.enum_item_name(settings, "look", name),)
                    if label
                )
            )

        def current(owner: Struct) -> int:
            look = scened(owner).map(lambda scene: shown(scene)[0].look).default_value(NO_LOOK)
            return next((index for index, name in enumerate((NO_LOOK, *PyOpenColorIO.GetCurrentConfig().getLookNames())) if name == look), 0)

        def write(handle: Handle[bpy.types.Scene], look: str) -> Outcome[Option[str]]:
            return handle.resolve().bind(lambda scene: looked(identity, scene, look)).map(lambda wrote: Some(f"Look {look}") if wrote else Nothing)

        with identity.reports.deferred(write) as deferred:

            def pick(owner: Struct, value: int) -> None:
                for scene in scened(owner).to_list():
                    deferred(Handle.of(scene), dict(enumerate((NO_LOOK, *PyOpenColorIO.GetCurrentConfig().getLookNames()))).get(value, NO_LOOK))

            FormationLook.__annotations__["look"].keywords.update(items=offered, get=current, set=pick)
            yield Ok((FormationLooks.attachment,))

    @property
    def row(self) -> Layout:
        """Tiles over the pick's look."""
        return Layout(rows=lambda identity: self.attachment.bound(identity).map(lambda attached: (_tiles("Look", Member(root=bpy.types.Scene, path=(attached.name, "look"))),)))


class StackLooks(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Scene pick over the layer stack presets, each pick or Strength change writing the picked preset over the head stack."""

    @functools.cached_property
    def attachment(self) -> Attachment[StackLook]:
        """Scene pick, a change deferring the preset write."""
        return Attachment(key="stack_looks", form=StackLook, owner=bpy.types.Scene, changed=Some(self.picked))

    @contextmanager
    def picked(self, identity: Identity) -> Generator[Changed]:
        """Holds the deferred preset write a pick or Strength change calls."""
        preset = stacks.preset

        def applied(scene: bpy.types.Scene, pick: StackLook) -> Outcome[Option[str]]:
            return (
                Ok(Nothing)
                if not pick.look
                else paired(preset.loaded(identity, pick.look), stacks.bound(identity))
                .bind(lambda parts: parts[1].looked(scene, parts[0], pick.strength, bpy.context))
                .map(lambda wrote: Some(f"Look {bpy.path.display_name(pick.look)}") if wrote else Nothing)
            )

        def write(handle: Handle[bpy.types.Scene]) -> Outcome[Option[str]]:
            return handle.resolve().bind(lambda scene: self.attachment.read(identity, scene).bind(lambda pick: applied(scene, pick)))

        with identity.reports.deferred(write) as deferred:
            yield lambda owner, _context: scened(owner).map(lambda scene: deferred(Handle.of(scene))).default_value(None)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Binds the pick rows to the stack presets of the extension, each tiled over the render proxy, then acquires the pick."""
        preset = stacks.preset

        def offered(_owner: Struct, _context: bpy.types.Context | None) -> tuple[tuple[str, str, str, int, int], ...]:
            def tile(stem: str) -> int:
                return preset.loaded(identity, stem).to_option().bind(lambda record: Tiles.icon(identity, f"stack look {stem}", record)).default_value(0)

            return _rows(tuple((stem, name, tip, tile(stem), index) for stem, name, tip, _, index in preset.entries(identity)))

        StackLook.__annotations__["look"].keywords["items"] = offered
        return Ok((self.attachment,))

    @property
    def row(self) -> Layout:
        """Tiles over the pick's look, then its Strength slider."""
        return Layout(
            rows=lambda identity: self.attachment.bound(identity).map(
                lambda attached: (
                    _tiles("Stack Look", Member(root=bpy.types.Scene, path=(attached.name, "look"))),
                    Property(target=Member(root=bpy.types.Scene, path=(attached.name, "strength")), keywords={"slider": True}),
                )
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def _rows(rows: tuple[tuple[str, str, str, int, int], ...]) -> tuple[tuple[str, str, str, int, int], ...]:
    """Holds look tile enum rows so the strings Blender reads stay referenced."""
    return rows


def scened(owner: Struct) -> Option[bpy.types.Scene]:
    """Returns the scene owning a pick."""
    match owner.id_data:
        case bpy.types.Scene() as scene:
            return Some(scene)
        case _:
            return Nothing


def _tiles(label: str, member: Member) -> Layout:
    """Enum tiles beside the label of the enum they pick."""
    return Layout(rows=(Property(target=member, function=FUNCTIONS["template_icon_view"], keywords={"show_labels": True}),), container=Some(functools.partial(gridded, label)))


# --- [COMPOSITION] ----------------------------------------------------------------------

formation_looks: Final = FormationLooks()
stack_looks: Final = StackLooks()
rows: Final = (
    formation_looks,
    stack_looks,
    Panel(
        section=Section(
            key="image_formation",
            title=bpy.path.display_name("image_formation"),
            rows=(formation_looks.row, stack_looks.row, Layout(rows=(OperatorButton(target=install.installing), OperatorButton(target=toning)), container=Some(lambda layout: layout.row(align=True)))),
        ),
        at=Child(parent=RENDER_PT_color_management),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FormationLook", "FormationLooks", "StackLook", "StackLooks", "rows"]
