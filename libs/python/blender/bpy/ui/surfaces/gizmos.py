"""Gizmo groups of stock and custom-shaped gizmos bound to members, handlers, or operators and themed at each draw."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import cache, partial
from types import MappingProxyType
from typing import Final, Literal, override, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import mathutils
import msgspec

from blender.bpy.commands.calls import filled, Pairs, Placement
from blender.bpy.commands.keymaps import Location, nesting
from blender.bpy.commands.modal import Press, Travel
from blender.bpy.data.rna.conditions import admits, Condition
from blender.bpy.data.rna.descriptors import assign
from blender.bpy.data.rna.paths import rna
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.ui.layout.rows import Member, ResolvedMember
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.bpy.ui.resources.theme import color, Shade
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Value = float | tuple[float, ...]
type _Stock = Literal[
    "GIZMO_GT_arrow_3d", "GIZMO_GT_blank_3d", "GIZMO_GT_button_2d", "GIZMO_GT_cage_2d", "GIZMO_GT_cage_3d", "GIZMO_GT_dial_3d", "GIZMO_GT_move_3d", "GIZMO_GT_primitive_3d", "GIZMO_GT_snap_3d"
]
type _Attach = Callable[[bpy.types.Gizmo, bpy.types.Context], Outcome[None]]
type _Bound = tuple[str, Member | ResolvedMember] | _Attach

# --- [CONSTANTS] ------------------------------------------------------------------------

_TARGETS: Final[Mapping[_Stock, str]] = MappingProxyType({
    "GIZMO_GT_arrow_3d": "offset",
    "GIZMO_GT_dial_3d": "offset",
    "GIZMO_GT_move_3d": "offset",
    "GIZMO_GT_cage_2d": "matrix",
    "GIZMO_GT_cage_3d": "matrix",
})

# --- [MODELS] ---------------------------------------------------------------------------


class TargetHandler[V](msgspec.Struct, frozen=True, kw_only=True):
    """Gizmo target the host reads and writes through `target_set_handler` over the group's context, with its range."""

    get: Callable[[bpy.types.Context], V]
    set: Callable[[bpy.types.Context, V], Outcome[None]]
    range: Option[Callable[[bpy.types.Context], tuple[float, float]]] = Nothing


class Shape(msgspec.Struct, frozen=True, kw_only=True):
    """Custom gizmo type drawn and selected through one fixed shape, its float targets moved by `drag` from their values at press and restored on cancel."""

    key: Key
    primitive: Literal["POINTS", "LINES", "TRIS", "LINE_STRIP"]
    vertices: tuple[tuple[float, ...], ...]
    targets: Mapping[str, int] = MappingProxyType({})
    drag: Option[Callable[[bpy.types.Gizmo, bpy.types.Context, tuple[tuple[str, _Value], ...], Travel], Outcome[None]]] = Nothing

    def named(self, identity: Identity) -> Outcome[str]:
        """Returns the gizmo type `bl_idname` gizmo groups create."""
        return identity.name(bpy.types.Gizmo, self.key)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the gizmo type class drawing the shape, batched on its first draw, and dragging through `drag` while one is stated, its press state held on the gizmo as Blender's custom gizmo template holds it."""
        batch = cache(partial(bpy.types.Gizmo.new_custom_shape, self.primitive, self.vertices))
        hold, targets, drags = identity.reports.hold, self.targets, self.drag

        class Shaped(bpy.types.Gizmo):
            @override
            def draw(self, context: bpy.types.Context) -> None:
                self.draw_custom_shape(batch())

            @override
            def draw_select(self, context: bpy.types.Context, select_id: int | None = 0) -> None:
                self.draw_custom_shape(batch(), select_id=select_id)

        class Dragged(Shaped):
            __slots__ = ("initial", "press")

            @override
            def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> set["rna_enums.OperatorReturnItems"]:
                self.press = Press(x=event.mouse_x, y=event.mouse_y, threshold=0)
                self.initial = tuple((target, self.target_get_value(target)) for target in targets if self.target_is_valid(target))
                return {"RUNNING_MODAL"}

            @override
            def modal(self, context: bpy.types.Context, event: bpy.types.Event, tweak: set[Literal["PRECISE", "SNAP"]] | None) -> set["rna_enums.OperatorReturnItems"]:
                tweaks: set[Literal["PRECISE", "SNAP"]] = tweak  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                for drag in drags.to_list():
                    drag(self, context, self.initial, self.press.travel(event, snapped="SNAP" in tweaks, precise="PRECISE" in tweaks)).default_with(hold)
                return {"RUNNING_MODAL"}

            @override
            def exit(self, context: bpy.types.Context, cancel: bool | None) -> None:
                cancelled: bool = cancel  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                for target, value in self.initial if cancelled else ():
                    self.target_set_value(target, value)  # type: ignore[call-arg]  # ty: ignore[too-many-positional-arguments]

        base = Dragged if drags.is_some() else Shaped
        declared = tuple({"id": target, "type": "FLOAT", "array_length": length} for target, length in targets.items())
        return self.named(identity).map(lambda name: (type(name, (base,), {"bl_idname": name, "bl_target_properties": declared}),))


class GizmoPart(msgspec.Struct, frozen=True, kw_only=True):
    """Gizmo of a stock type or custom shape placed each redraw, bound to a member, a target handler, or one operator per highlighted part.

    `role` gives the color with alpha drawn normally and highlighted, read each draw so theme edits apply.
    `properties` set every other gizmo member and type property alike, the host forwarding a type property through the gizmo.
    A member or handler binding pushes one undo step per use, and `prepare` runs as the gizmo is pressed.
    """

    kind: _Stock | Shape
    place: Callable[[bpy.types.Context], Outcome[Option[mathutils.Matrix]]]
    bind: Member | TargetHandler[float] | TargetHandler[tuple[float, ...]] | tuple[Placement, ...] = ()
    properties: Mapping[str, object] = MappingProxyType({})
    glyph: Option[Glyph] = Nothing
    role: Callable[[bpy.types.ThemeUserInterface], tuple[Shade, Shade]] = lambda ui: ((*color(ui.gizmo_primary)[:3], 0.5), ui.gizmo_hi)
    prepare: Option[Callable[[bpy.types.Context, bpy.types.Gizmo], Outcome[None]]] = Nothing

    def bound(self, identity: Identity) -> Outcome[_Bound]:
        """Resolves the binding into a member with its target name or the attach of a handler or operators, absent fault for a member or handler on a kind without target."""
        named = (Block.of_seq(self.kind.targets).try_head() if isinstance(self.kind, Shape) else Option.of_optional(_TARGETS.get(self.kind))).to_result_with(
            lambda: Fault.of(Reason.ABSENT, repr(self.kind), member="target")
        )
        match self.bind:
            case tuple(placements):
                return accumulated(placement.resolved(identity) for placement in placements).map(lambda placed: partial(_operated, placed))
            case TargetHandler() as handler:
                return named.map(lambda key: partial(_handled, key, handler, identity.reports.reported(handler.set)))
            case Member() as member:
                return paired(named, member.bound()).map(lambda pair: (pair[0], pair[1]))


class Gizmos(msgspec.Struct, frozen=True, kw_only=True):
    """Gizmo group of parts in one editor region, labelled by its key, polled by its condition and every member owner it binds.

    A group without `PERSISTENT` shows while a tool names it or an `ensured` scope stands, and `keymap` names the keymap handling its events in place of the host's generic one.
    """

    key: Key
    space: "rna_enums.SpaceTypeItems"
    parts: tuple[GizmoPart, ...]
    region: "rna_enums.RegionTypeItems" = "WINDOW"
    condition: Option[Condition] = Nothing
    keymap: Option[str] = Nothing
    options: frozenset[Literal["3D", "SCALE", "DEPTH_3D", "SELECT", "PERSISTENT", "SHOW_MODAL_ALL", "EXCLUDE_MODAL", "TOOL_INIT", "TOOL_FALLBACK_KEYMAP", "VR_REDRAWS"]] = frozenset({"PERSISTENT"})

    # --- [NAMING]

    def named(self, identity: Identity) -> Outcome[str]:
        """Returns group `bl_idname` for tools and linking."""
        return identity.name(bpy.types.GizmoGroup, self.key)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns each custom shape, then the group class built when the interface band opens and its glyphs resolve."""
        kinds = accumulated(part.kind.named(identity) if isinstance(part.kind, Shape) else Ok(part.kind) for part in self.parts)
        bound = accumulated(part.bound(identity) for part in self.parts)
        located = self.keymap.map(lambda name: nesting(name).map(lambda found: Some(found[0]))).default_value(Ok(Nothing))
        shapes = tuple(part.kind for part in self.parts if isinstance(part.kind, Shape))
        return paired(self.named(identity), kinds, bound, located).map(lambda held: (*shapes, (Band.INTERFACE, partial(_grouped, self, identity, *held))))

    # --- [LINKING]

    @contextmanager
    def ensured(self, identity: Identity) -> Generator[Outcome[None]]:
        """Links a group without `PERSISTENT` into every region of its type for the scope, refused for a persistent or unregistered group."""
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        with ExitStack() as stack:
            yield self.named(identity).bind(
                lambda name: (
                    catch(manager.gizmo_group_type_ensure, exception=RuntimeError)(name)
                    .map(lambda _: stack.callback(manager.gizmo_group_type_unlink_delayed, name))
                    .map(lambda _: None)
                    .map_error(partial(Fault.of, Reason.REFUSED, name))
                )
            )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSES]


def _grouped(group: Gizmos, identity: Identity, name: str, kinds: tuple[str, ...], bound: tuple[_Bound, ...], keymap: Option[Location]) -> Outcome[tuple[type[bpy.types.GizmoGroup]]]:
    """Builds the group class once every glyph resolves: creating its gizmos with their operators, rebinding members on refresh, and at each draw rebinding handlers, whose values the host caches until a rebind, then theming and placing them."""
    hold, parts = identity.reports.hold, group.parts
    members = tuple(binding for binding in bound if isinstance(binding, tuple))

    def rebound(gizmo: bpy.types.Gizmo, binding: tuple[str, Member | ResolvedMember], context: bpy.types.Context) -> Outcome[None]:
        """Binds the target to the member's current owner, leaving it while the reach finds none."""
        key, member = binding
        return member.current((), context).bind(lambda found: found.map(partial(_targeted, gizmo, key, context)).default_value(Ok(None)))

    def poll(_cls: type[bpy.types.GizmoGroup], context: bpy.types.Context) -> bool:
        if not admits(group.condition, context):
            return False
        found = accumulated(member.current((), context) for _, member in members).map_error(hold).to_option()
        return found.is_some() and all(present.bind(lambda held: held.at(context, context)).is_some() for each in found.to_list() for present in each)

    def setup(owner: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
        made = tuple((owner.gizmos.new(kind), kind, part) for kind, part in zip(kinds, parts, strict=True))
        written = accumulated(
            assign(gizmo, member, value).map_error(partial(Fault.of, Reason.REFUSED, f"{kind}.{member}"))
            for gizmo, kind, part in made
            for member, value in (
                ("use_undo", isinstance(part.bind, Member | TargetHandler)),
                *(("icon_value", value) for glyph in part.glyph.to_list() for value in Icons.value(identity, glyph).to_option().to_list()),
                *part.properties.items(),
            )
        )
        attached = accumulated(binding(gizmo, context) for (gizmo, _, part), binding in zip(made, bound, strict=True) if not isinstance(binding, tuple) and not isinstance(part.bind, TargetHandler))
        paired(written, attached).default_with(hold)

    def refresh(owner: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
        accumulated(rebound(gizmo, binding, context) for gizmo, binding in zip(owner.gizmos, bound, strict=True) if isinstance(binding, tuple)).default_with(hold)

    def draw_prepare(owner: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
        preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        ui = preferences.themes[0].user_interface
        accumulated(
            binding(gizmo, context) for gizmo, part, binding in zip(owner.gizmos, parts, bound, strict=True) if isinstance(part.bind, TargetHandler) and not isinstance(binding, tuple)
        ).default_with(hold)
        for gizmo, part in zip(owner.gizmos, parts, strict=True):
            (red, green, blue, alpha), (lit_red, lit_green, lit_blue, lit_alpha) = map(color, part.role(ui))
            gizmo.color, gizmo.alpha = (red, green, blue), alpha
            gizmo.color_highlight, gizmo.alpha_highlight = (lit_red, lit_green, lit_blue), lit_alpha
            placed = identity.reports.reported(part.place)(context).bind(lambda found: found)
            gizmo.hide = placed.is_none()
            for matrix in placed.to_list():
                gizmo.matrix_basis = matrix

    def invoke_prepare(owner: bpy.types.GizmoGroup, context: bpy.types.Context, gizmo: bpy.types.Gizmo) -> None:
        for prepare in (prepare for held, part in zip(owner.gizmos, parts, strict=True) if held == gizmo for prepare in part.prepare.to_list()):
            prepare(context, gizmo).default_with(hold)

    native = {
        "bl_idname": name,
        "bl_label": bpy.path.display_name(group.key),
        "bl_space_type": group.space,
        "bl_region_type": group.region,
        "bl_options": set(group.options),
        "poll": classmethod(poll),
        "setup": setup,
        "refresh": refresh,
        "draw_prepare": draw_prepare,
        **({"invoke_prepare": invoke_prepare} if any(part.prepare.is_some() for part in parts) else {}),
        **keymap.map(lambda location: {"setup_keymap": classmethod(lambda _cls, keyconfig: keyconfig.keymaps.new(location[0], space_type=location[1], region_type=location[2]))}).default_value({}),
    }
    glyphs = accumulated(Icons.value(identity, glyph) for part in parts for glyph in part.glyph.to_list())
    return glyphs.map(lambda _: (type(name, (bpy.types.GizmoGroup,), native),))


# --- [TARGETS]


def _targeted(gizmo: bpy.types.Gizmo, key: str, context: bpy.types.Context, held: ResolvedMember) -> Outcome[None]:
    """Binds the target to the member at its located owner, a property the target cannot take refused."""
    return accumulated(
        catch(gizmo.target_set_prop, exception=RuntimeError)(key, owner, identifier, index=held.descriptor.index.default_value(-1)).map_error(
            partial(Fault.of, Reason.REFUSED, rna(held.descriptor.path))
        )
        for owner, identifier in held.at(context, context).to_list()
    ).map(lambda _: None)


def _handled[V](key: str, handler: TargetHandler[V], write: Callable[[bpy.types.Context, V], Option[None]], gizmo: bpy.types.Gizmo, context: bpy.types.Context) -> Outcome[None]:
    """Binds handler callbacks as the target over the group's context."""
    gizmo.target_set_handler(
        key,
        get=lambda: handler.get(context),
        set=lambda value: write(context, value).to_optional(),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        range=handler.range.map(lambda extent: partial(extent, context)).to_optional(),
    )
    return Ok(None)


def _operated(placed: tuple[tuple[str, Pairs], ...], gizmo: bpy.types.Gizmo, _context: bpy.types.Context) -> Outcome[None]:
    """Sets one operator per highlighted part in placement order and fills its properties."""
    return accumulated(filled(each, gizmo.target_set_operator(each[0], index=index)) for index, each in enumerate(placed)).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GizmoPart", "Gizmos", "Shape", "TargetHandler"]
