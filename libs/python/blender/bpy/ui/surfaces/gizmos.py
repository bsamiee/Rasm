"""Gizmo groups of themed stock gizmos bound to operators and values."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack
from functools import partial
from typing import ClassVar, Literal, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import mathutils
import msgspec

from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisition, Band, entered
from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.bpy.ui.layout.rows import located, Member, ResolvedMember
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.bpy.ui.resources.theme import color
from blender.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

    from blender.bpy.commands.operators import Placed, Writer

# --- [TYPES] ----------------------------------------------------------------------------

type Place = Callable[[bpy.types.Context], BlenderResult[Option[mathutils.Matrix]]]
type GizmoPart = Button | Primitive | Cage | Arrow | Dial | Move
type Target = Callable[[bpy.types.Gizmo, bpy.types.Context], BlenderResult[None]]
type Binding = tuple[str, Member | ResolvedMember[bpy.types.FloatProperty]] | Target
type MatrixValues = tuple[float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float]

# --- [MODELS] ---------------------------------------------------------------------------


class Handler[V](msgspec.Struct, frozen=True, kw_only=True):
    """Gizmo target context callbacks and range."""

    get: Callable[[bpy.types.Context], V]
    set: Callable[[bpy.types.Context, V], BlenderResult[None]]
    range: Option[Callable[[bpy.types.Context], tuple[float, float]]] = Nothing


class GizmoShape(msgspec.Struct, frozen=True, kw_only=True):
    """Placement and gizmo properties every part shares."""

    place: Place
    scale_basis: Option[float] = Nothing
    matrix_space: Option[mathutils.Matrix] = Nothing
    matrix_offset: Option[mathutils.Matrix] = Nothing
    line_width: Option[float] = Nothing
    select_bias: Option[float] = Nothing
    hide_select: Option[bool] = Nothing
    hide_keymap: Option[bool] = Nothing
    use_grab_cursor: Option[bool] = Nothing
    use_draw_hover: Option[bool] = Nothing
    use_draw_modal: Option[bool] = Nothing
    use_draw_value: Option[bool] = Nothing
    use_draw_offset_scale: Option[bool] = Nothing
    use_draw_scale: Option[bool] = Nothing
    use_select_background: Option[bool] = Nothing
    use_operator_tool_properties: Option[bool] = Nothing
    use_event_handle_all: Option[bool] = Nothing
    use_tooltip: Option[bool] = Nothing
    use_undo: Option[bool] = Nothing
    select: Option[bool] = Nothing
    color: Option[tuple[float, float, float]] = Nothing
    alpha: Option[float] = Nothing
    color_highlight: Option[tuple[float, float, float]] = Nothing
    alpha_highlight: Option[float] = Nothing


class Handle(GizmoShape, frozen=True, kw_only=True):
    """Part with resting color read from theme."""

    role: Callable[[bpy.types.ThemeUserInterface], mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]] = lambda ui: ui.gizmo_primary


class Button(GizmoShape, frozen=True, kw_only=True):
    """Screen button showing a glyph and invoking an operator."""

    kind: ClassVar[str] = "GIZMO_GT_button_2d"
    glyph: "Glyph"
    press: "Placed"
    draw_options: Option[frozenset[Literal["OUTLINE", "BACKDROP", "HELPLINE"]]] = Nothing
    show_drag: Option[bool] = Nothing
    backdrop_fill_alpha: Option[float] = Nothing


class Primitive(Handle, frozen=True, kw_only=True):
    """Selectable plane, circle, or annulus invoking an operator."""

    kind: ClassVar[str] = "GIZMO_GT_primitive_3d"
    press: "Placed"
    draw_style: Option[Literal["PLANE", "CIRCLE", "ANNULUS"]] = Nothing
    arc_inner_factor: Option[float] = Nothing
    draw_inner: Option[bool] = Nothing


class Cage(Handle, frozen=True, kw_only=True):
    """2D transform cage bound to a 16-component matrix."""

    kind: ClassVar[str] = "GIZMO_GT_cage_2d"
    target: ClassVar[str] = "matrix"
    bind: Handler[MatrixValues]
    dimensions: Option[tuple[float, float]] = Nothing
    pivot: Option[tuple[float, float]] = Nothing
    transform: Option[frozenset[Literal["TRANSLATE", "ROTATE", "SCALE", "SCALE_UNIFORM"]]] = Nothing
    draw_style: Option[Literal["BOX", "BOX_TRANSFORM", "CIRCLE"]] = Nothing
    draw_options: Option[frozenset[Literal["XFORM_CENTER_HANDLE", "CORNER_HANDLES"]]] = Nothing


class Arrow(Handle, frozen=True, kw_only=True):
    """Arrow bound to float member or callback value."""

    kind: ClassVar[str] = "GIZMO_GT_arrow_3d"
    target: ClassVar[str] = "offset"
    bind: "Member | Handler[float]"
    draw_style: Option[Literal["NORMAL", "CROSS", "BOX", "CONE", "PLANE"]] = Nothing
    draw_options: Option[frozenset[Literal["STEM", "ORIGIN"]]] = Nothing
    transform: Option[frozenset[Literal["INVERT", "CONSTRAIN"]]] = Nothing
    length: Option[float] = Nothing
    aspect: Option[tuple[float, float]] = Nothing
    incremental_snap_step: Option[float] = Nothing


class Dial(Handle, frozen=True, kw_only=True):
    """Dial bound to float member or callback angle."""

    kind: ClassVar[str] = "GIZMO_GT_dial_3d"
    target: ClassVar[str] = "offset"
    bind: "Member | Handler[float]"
    draw_options: Option[frozenset[Literal["CLIP", "FILL", "FILL_SELECT", "ANGLE_MIRROR", "ANGLE_START_Y", "ANGLE_VALUE"]]] = Nothing
    wrap_angle: Option[bool] = Nothing
    arc_inner_factor: Option[float] = Nothing
    arc_partial_angle: Option[float] = Nothing
    incremental_angle: Option[float] = Nothing
    click_value: Option[float] = Nothing


class Move(Handle, frozen=True, kw_only=True):
    """Move handle bound to a 3-component value or operator."""

    kind: ClassVar[str] = "GIZMO_GT_move_3d"
    target: ClassVar[str] = "offset"
    bind: "Handler[tuple[float, float, float]] | Placed"
    draw_style: Option[Literal["RING_2D", "CROSS_2D"]] = Nothing
    draw_options: Option[frozenset[Literal["FILL", "FILL_SELECT", "ALIGN_VIEW"]]] = Nothing
    use_snap: Option[bool] = Nothing


class Gizmos(msgspec.Struct, frozen=True, kw_only=True):
    """Persistent gizmo group of themed stock parts."""

    persistent: ClassVar[bool] = True
    key: Key
    title: str
    space: "rna_enums.SpaceTypeItems"
    parts: tuple[GizmoPart, ...]
    region: "rna_enums.RegionTypeItems" = "WINDOW"
    condition: Option[Condition] = Nothing
    owner: Option[str] = Nothing
    options: frozenset[Literal["3D", "SCALE", "DEPTH_3D", "SELECT", "SHOW_MODAL_ALL", "EXCLUDE_MODAL", "TOOL_INIT", "TOOL_FALLBACK_KEYMAP", "VR_REDRAWS"]] = frozenset()

    # --- [NAMING]

    def named(self, identity: Identity) -> BlenderResult[str]:
        """Returns group `bl_idname` for tools and linking."""
        return identity.name(Name.GIZMO_GROUP, self.key)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Binds part targets, registering group class in the interface band once button glyphs resolve."""

        def target[V](key: str, handler: Handler[V]) -> Target:
            """Binds handler as target callbacks."""
            write = identity.reports.reported(handler.set)

            def bound(gizmo: bpy.types.Gizmo, context: bpy.types.Context) -> BlenderResult[None]:
                """Binds callbacks to group context."""
                gizmo.target_set_handler(
                    key,
                    get=lambda: handler.get(context),
                    set=lambda value: write(context, value).to_optional(),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    range=handler.range.map(lambda extent: partial(extent, context)).to_optional(),
                )
                return Ok(None)

            return bound

        def acquired(part: GizmoPart) -> BlenderResult[tuple[GizmoPart, Binding]]:
            """Resolves operator, property, or handler target."""

            def operator(invocation: tuple[str, "Writer"]) -> tuple[GizmoPart, Binding]:
                """Pairs part with operator target."""
                name, writer = invocation
                return part, lambda gizmo, _context: writer(gizmo.target_set_operator(name, index=0))

            match part:
                case Button(press=press) | Primitive(press=press):
                    return press.invocation(identity).map(operator)
                case Arrow(bind=binding) | Dial(bind=binding):
                    return (
                        binding.reach.map(lambda _: Ok((part, (part.target, binding)))).default_with(lambda: binding.resolved(bpy.types.FloatProperty).map(lambda held: (part, (part.target, held))))
                        if isinstance(binding, Member)
                        else Ok((part, target(part.target, binding)))
                    )
                case Cage(bind=handler):
                    return Ok((part, target(part.target, handler)))
                case Move():
                    return Ok((part, target(part.target, part.bind))) if isinstance(part.bind, Handler) else part.bind.invocation(identity).map(operator)

        def built(name: str, parts: tuple[tuple[GizmoPart, Binding], ...], icons: tuple[Option[int], ...]) -> type[bpy.types.GizmoGroup]:
            """Builds gizmo group class over resolved parts and button icons."""

            def reached(member: Member | ResolvedMember[bpy.types.FloatProperty], context: bpy.types.Context) -> BlenderResult[Option[ResolvedMember[bpy.types.FloatProperty]]]:
                """Resolves member on its source's current struct."""
                return (
                    member.reach.bind(lambda source: source(context)).map(lambda actual: member.resolved(bpy.types.FloatProperty, root=Some(actual)).map(Some)).default_value(Ok(Nothing))
                    if isinstance(member, Member)
                    else Ok(Some(member))
                )

            def attach(gizmo: bpy.types.Gizmo, binding: Binding, context: bpy.types.Context) -> BlenderResult[None]:
                """Binds part target, property refusal as edit fault."""
                if not isinstance(binding, tuple):
                    return binding(gizmo, context)
                key, member = binding
                match reached(member, context):
                    case Result(tag="error", error=faults):
                        return Error(faults)
                    case Result(ok=found):
                        for held in found.to_list():
                            try:
                                for owner, identifier in located(held, context, context).to_list():
                                    gizmo.target_set_prop(key, owner, identifier, index=held.descriptor.index.default_value(-1))
                            except RuntimeError as error:
                                return Error((BlenderEditFault(request=held.descriptor.path, error=error),))
                        return Ok(None)

            def poll(_cls: type[bpy.types.GizmoGroup], context: bpy.types.Context) -> bool:
                """Returns whether condition holds and every owner resolves."""
                if not all(holds(required, context) for required in self.condition.to_list()):
                    return False
                resolved = accumulated(reached(binding[1], context) for _, binding in parts if isinstance(binding, tuple)).map_error(identity.reports.hold).to_option()
                return resolved.is_some() and all(member.bind(partial(located, holder=context, context=context)).is_some() for members in resolved.to_list() for member in members)

            def setup(group: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
                """Creates gizmos with theme colors, then binds targets."""
                ui = context.preferences.themes[0].user_interface  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                for index, (part, _) in enumerate(parts):
                    gizmo = group.gizmos.new(part.kind)
                    for value in icons[index].to_list():
                        gizmo.icon_value = value  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    resting, highlighted = (color(ui.wcol_tool.inner), color(ui.wcol_tool.inner_sel)) if isinstance(part, Button) else (color(part.role(ui)), color(ui.gizmo_hi))
                    gizmo.color, gizmo.alpha = resting[:3], resting[3]
                    gizmo.color_highlight, gizmo.alpha_highlight = highlighted[:3], highlighted[3]
                    gizmo.use_draw_modal = gizmo.use_draw_value = True
                    gizmo.use_undo = not (isinstance(part, (Button, Primitive)) or (isinstance(part, Move) and not isinstance(part.bind, Handler)))
                    for member, supplied in ((member, supplied) for member, value in msgspec.structs.asdict(part).items() if isinstance(value, Option) for supplied in value.to_list()):
                        setattr(gizmo, member, set(supplied) if isinstance(supplied, frozenset) else supplied)
                return accumulated(attach(group.gizmos[index], binding, context) for index, (_, binding) in enumerate(parts)).map(lambda _: None).default_with(identity.reports.hold)

            def refresh(group: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
                """Rebinds property targets to current owners."""
                return (
                    accumulated(attach(group.gizmos[index], binding, context) for index, (_, binding) in enumerate(parts) if isinstance(binding, tuple))
                    .map(lambda _: None)
                    .default_with(identity.reports.hold)
                )

            def draw_prepare(group: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
                """Places parts, hiding unplaced ones."""
                for index, (part, _) in enumerate(parts):
                    gizmo, placed = group.gizmos[index], identity.reports.reported(part.place)(context).bind(lambda found: found)
                    gizmo.hide = placed.is_none()
                    for matrix in placed.to_list():
                        gizmo.matrix_basis = matrix

            flags = {*self.options, *(("PERSISTENT",) if self.persistent else ())}
            if (self.space == "VIEW_3D" and any(not isinstance(part, Button) for part, _ in parts)) or any(isinstance(part, (Primitive, Dial)) for part, _ in parts):
                flags.add("3D")
            native = {
                "bl_idname": name,
                "bl_label": self.title,
                "bl_space_type": self.space,
                "bl_region_type": self.region,
                "bl_options": flags,
                "poll": classmethod(poll),
                "setup": setup,
                "refresh": refresh,
                "draw_prepare": draw_prepare,
            }
            for owner in self.owner.to_list():
                native["bl_owner_id"] = owner
            return type(name, (bpy.types.GizmoGroup,), native)

        @contextmanager
        def registered(name: str, parts: tuple[tuple[GizmoPart, Binding], ...]) -> Generator[BlenderResult[None]]:
            """Registers group class for the scope once every button glyph resolves."""
            with ExitStack() as stack:
                yield accumulated(Icons.value(identity, part.glyph).map(Some) if isinstance(part, Button) else Ok(Nothing) for part, _ in parts).bind(
                    lambda icons: entered(stack, (built(name, parts, icons),))
                )

        return paired(self.named(identity), accumulated(acquired(part) for part in self.parts)).map(lambda held: ((Band.INTERFACE, registered(*held)),))


class Linked(Gizmos, frozen=True, kw_only=True):
    """Group shown by a tool or linked for a scope."""

    persistent: ClassVar[bool] = False

    @contextmanager
    def linked(self, identity: Identity) -> Generator[BlenderResult[None]]:
        """Links group into its regions for the scope."""
        acquired = paired(self.named(identity), rooted(bpy.types.WindowManager, bpy.context)).bind(
            lambda held: (
                catch(lambda: held[1].gizmo_group_type_ensure(held[0]), exception=RuntimeError)().map(lambda _: held).map_error(lambda error: (BlenderEditFault(request=held[0], error=error),))
            )
        )
        try:
            yield acquired.map(lambda _: None)
        finally:
            for name, manager in acquired.to_option().to_list():
                manager.gizmo_group_type_unlink_delayed(name)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Arrow", "Button", "Cage", "Dial", "GizmoPart", "GizmoShape", "Gizmos", "Handle", "Handler", "Linked", "MatrixValues", "Move", "Place", "Primitive"]
