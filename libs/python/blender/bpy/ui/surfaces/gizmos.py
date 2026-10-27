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
from msgspec import UNSET, UnsetType

from blender.bpy.commands.calls import Placed, Writer
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import rna, rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisition, Band, entered
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.layout.rows import Member, ResolvedMember
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.bpy.ui.resources.theme import color
from blender.records.faults import accumulated, Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Place = Callable[[bpy.types.Context], Outcome[Option[mathutils.Matrix]]]
type GizmoPart = Button | Primitive | Cage | Arrow | Dial | Move
type Target = Callable[[bpy.types.Gizmo, bpy.types.Context], Outcome[None]]
type Binding = tuple[str, Member | ResolvedMember[bpy.types.FloatProperty]] | Target
type MatrixValues = tuple[float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float]

# --- [MODELS] ---------------------------------------------------------------------------


class Handler[V](msgspec.Struct, frozen=True, kw_only=True):
    """Gizmo target context callbacks and range."""

    get: Callable[[bpy.types.Context], V]
    set: Callable[[bpy.types.Context, V], Outcome[None]]
    range: Option[Callable[[bpy.types.Context], tuple[float, float]]] = Nothing


class GizmoShape(msgspec.Struct, frozen=True, kw_only=True):
    """Placement and gizmo properties every part shares, an `UNSET` property keeping the host default."""

    place: Place
    scale_basis: float | UnsetType = UNSET
    matrix_space: mathutils.Matrix | UnsetType = UNSET
    matrix_offset: mathutils.Matrix | UnsetType = UNSET
    line_width: float | UnsetType = UNSET
    select_bias: float | UnsetType = UNSET
    hide_select: bool | UnsetType = UNSET
    hide_keymap: bool | UnsetType = UNSET
    use_grab_cursor: bool | UnsetType = UNSET
    use_draw_hover: bool | UnsetType = UNSET
    use_draw_modal: bool | UnsetType = UNSET
    use_draw_value: bool | UnsetType = UNSET
    use_draw_offset_scale: bool | UnsetType = UNSET
    use_draw_scale: bool | UnsetType = UNSET
    use_select_background: bool | UnsetType = UNSET
    use_operator_tool_properties: bool | UnsetType = UNSET
    use_event_handle_all: bool | UnsetType = UNSET
    use_tooltip: bool | UnsetType = UNSET
    use_undo: bool | UnsetType = UNSET
    select: bool | UnsetType = UNSET
    color: tuple[float, float, float] | UnsetType = UNSET
    alpha: float | UnsetType = UNSET
    color_highlight: tuple[float, float, float] | UnsetType = UNSET
    alpha_highlight: float | UnsetType = UNSET


class Handle(GizmoShape, frozen=True, kw_only=True):
    """Part with resting color read from theme."""

    role: Callable[[bpy.types.ThemeUserInterface], mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]] = lambda ui: ui.gizmo_primary


class Button(GizmoShape, frozen=True, kw_only=True):
    """Screen button showing a glyph and invoking an operator."""

    kind: ClassVar[str] = "GIZMO_GT_button_2d"
    glyph: Glyph
    press: Placed
    draw_options: frozenset[Literal["OUTLINE", "BACKDROP", "HELPLINE"]] | UnsetType = UNSET
    show_drag: bool | UnsetType = UNSET
    backdrop_fill_alpha: float | UnsetType = UNSET


class Primitive(Handle, frozen=True, kw_only=True):
    """Selectable plane, circle, or annulus invoking an operator."""

    kind: ClassVar[str] = "GIZMO_GT_primitive_3d"
    press: Placed
    draw_style: Literal["PLANE", "CIRCLE", "ANNULUS"] | UnsetType = UNSET
    arc_inner_factor: float | UnsetType = UNSET
    draw_inner: bool | UnsetType = UNSET


class Cage(Handle, frozen=True, kw_only=True):
    """2D transform cage bound to a 16-component matrix."""

    kind: ClassVar[str] = "GIZMO_GT_cage_2d"
    target: ClassVar[str] = "matrix"
    bind: Handler[MatrixValues]
    dimensions: tuple[float, float] | UnsetType = UNSET
    pivot: tuple[float, float] | UnsetType = UNSET
    transform: frozenset[Literal["TRANSLATE", "ROTATE", "SCALE", "SCALE_UNIFORM"]] | UnsetType = UNSET
    draw_style: Literal["BOX", "BOX_TRANSFORM", "CIRCLE"] | UnsetType = UNSET
    draw_options: frozenset[Literal["XFORM_CENTER_HANDLE", "CORNER_HANDLES"]] | UnsetType = UNSET


class Arrow(Handle, frozen=True, kw_only=True):
    """Arrow bound to float member or callback value."""

    kind: ClassVar[str] = "GIZMO_GT_arrow_3d"
    target: ClassVar[str] = "offset"
    bind: Member | Handler[float]
    draw_style: Literal["NORMAL", "CROSS", "BOX", "CONE", "PLANE"] | UnsetType = UNSET
    draw_options: frozenset[Literal["STEM", "ORIGIN"]] | UnsetType = UNSET
    transform: frozenset[Literal["INVERT", "CONSTRAIN"]] | UnsetType = UNSET
    length: float | UnsetType = UNSET
    aspect: tuple[float, float] | UnsetType = UNSET
    incremental_snap_step: float | UnsetType = UNSET


class Dial(Handle, frozen=True, kw_only=True):
    """Dial bound to float member or callback angle."""

    kind: ClassVar[str] = "GIZMO_GT_dial_3d"
    target: ClassVar[str] = "offset"
    bind: Member | Handler[float]
    draw_options: frozenset[Literal["CLIP", "FILL", "FILL_SELECT", "ANGLE_MIRROR", "ANGLE_START_Y", "ANGLE_VALUE"]] | UnsetType = UNSET
    wrap_angle: bool | UnsetType = UNSET
    arc_inner_factor: float | UnsetType = UNSET
    arc_partial_angle: float | UnsetType = UNSET
    incremental_angle: float | UnsetType = UNSET
    click_value: float | UnsetType = UNSET


class Move(Handle, frozen=True, kw_only=True):
    """Move handle bound to a 3-component value or operator."""

    kind: ClassVar[str] = "GIZMO_GT_move_3d"
    target: ClassVar[str] = "offset"
    bind: Handler[tuple[float, float, float]] | Placed
    draw_style: Literal["RING_2D", "CROSS_2D"] | UnsetType = UNSET
    draw_options: frozenset[Literal["FILL", "FILL_SELECT", "ALIGN_VIEW"]] | UnsetType = UNSET
    use_snap: bool | UnsetType = UNSET


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

    def named(self, identity: Identity) -> Outcome[str]:
        """Returns group `bl_idname` for tools and linking."""
        return identity.name(bpy.types.GizmoGroup, self.key)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Binds part targets, registering group class in the interface band once button glyphs resolve."""

        def target[V](key: str, handler: Handler[V]) -> Target:
            """Binds handler as target callbacks."""
            return partial(_bound, key, handler, identity.reports.reported(handler.set))

        def acquired(part: GizmoPart) -> Outcome[tuple[GizmoPart, Binding]]:
            """Resolves operator, property, or handler target."""

            def operator(invocation: tuple[str, Writer]) -> tuple[GizmoPart, Binding]:
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

            def reached(member: Member | ResolvedMember[bpy.types.FloatProperty], context: bpy.types.Context) -> Outcome[Option[ResolvedMember[bpy.types.FloatProperty]]]:
                """Resolves member on its source's current struct."""
                return (
                    member.reach.bind(lambda source: source(context)).map(lambda actual: member.resolved(bpy.types.FloatProperty, root=Some(actual)).map(Some)).default_value(Ok(Nothing))
                    if isinstance(member, Member)
                    else Ok(Some(member))
                )

            def attach(gizmo: bpy.types.Gizmo, binding: Binding, context: bpy.types.Context) -> Outcome[None]:
                """Binds part target, property refusal as edit fault."""
                if not isinstance(binding, tuple):
                    return binding(gizmo, context)
                key, member = binding
                match reached(member, context):
                    case Result(tag="error", error=faults):
                        return Error(faults)
                    case Result(ok=found):
                        return found.map(
                            lambda held: accumulated(
                                catch(gizmo.target_set_prop, exception=RuntimeError)(key, owner, identifier, index=held.descriptor.index.default_value(-1)).map_error(
                                    partial(Fault.of, BlenderReason.REFUSED, rna(held.descriptor.path))
                                )
                                for owner, identifier in held.at(context, context).to_list()
                            ).map(lambda _: None)
                        ).default_value(Ok(None))

            def poll(_cls: type[bpy.types.GizmoGroup], context: bpy.types.Context) -> bool:
                """Returns whether condition holds and every owner resolves."""
                if not all(required.holds(context) for required in self.condition.to_list()):
                    return False
                resolved = accumulated(reached(binding[1], context) for _, binding in parts if isinstance(binding, tuple)).map_error(identity.reports.hold).to_option()
                return resolved.is_some() and all(member.bind(lambda held: held.at(context, context)).is_some() for members in resolved.to_list() for member in members)

            def setup(group: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
                """Creates gizmos with theme colors, then binds targets."""
                preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                ui = preferences.themes[0].user_interface
                for index, (part, _) in enumerate(parts):
                    gizmo = group.gizmos.new(part.kind)
                    for value in icons[index].to_list():
                        gizmo.icon_value = value  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    resting, highlighted = (color(ui.wcol_tool.inner), color(ui.wcol_tool.inner_sel)) if isinstance(part, Button) else (color(part.role(ui)), color(ui.gizmo_hi))
                    gizmo.color, gizmo.alpha = resting[:3], resting[3]
                    gizmo.color_highlight, gizmo.alpha_highlight = highlighted[:3], highlighted[3]
                    gizmo.use_draw_modal = gizmo.use_draw_value = True
                    gizmo.use_undo = not (isinstance(part, (Button, Primitive)) or (isinstance(part, Move) and not isinstance(part.bind, Handler)))
                    values = msgspec.structs.asdict(part)
                    for member, supplied in ((field.name, values[field.name]) for field in msgspec.structs.fields(part) if field.default is UNSET and not isinstance(values[field.name], UnsetType)):
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
        def registered(name: str, parts: tuple[tuple[GizmoPart, Binding], ...]) -> Generator[Outcome[None]]:
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
    def linked(self, identity: Identity) -> Generator[Outcome[None]]:
        """Links group into its regions for the scope."""
        acquired = paired(self.named(identity), rooted(bpy.types.WindowManager, bpy.context)).bind(
            lambda held: catch(held[1].gizmo_group_type_ensure, exception=RuntimeError)(held[0]).map(lambda _: held).map_error(partial(Fault.of, BlenderReason.REFUSED, held[0]))
        )
        try:
            yield acquired.map(lambda _: None)
        finally:
            for name, manager in acquired.to_option().to_list():
                manager.gizmo_group_type_unlink_delayed(name)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _bound[V](key: str, handler: Handler[V], write: Callable[[bpy.types.Context, V], Option[None]], gizmo: bpy.types.Gizmo, context: bpy.types.Context) -> Outcome[None]:
    """Binds handler callbacks as gizmo target in group context."""
    gizmo.target_set_handler(
        key,
        get=lambda: handler.get(context),
        set=lambda value: write(context, value).to_optional(),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        range=handler.range.map(lambda extent: partial(extent, context)).to_optional(),
    )
    return Ok(None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Arrow", "Button", "Cage", "Dial", "GizmoPart", "GizmoShape", "Gizmos", "Handle", "Handler", "Linked", "MatrixValues", "Move", "Place", "Primitive"]
